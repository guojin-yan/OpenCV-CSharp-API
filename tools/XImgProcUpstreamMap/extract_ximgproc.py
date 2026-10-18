#!/usr/bin/env python3
"""Extract parser-derived public header closures for OpenCV contrib modules."""

import argparse
import hashlib
import importlib.util
import json
import tempfile
from pathlib import Path


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def rel(path, workspace):
    return str(path.relative_to(workspace)).replace("\\", "/")


def identity(kind, name, return_type, arguments, enum_values):
    if kind == "enum":
        values = ";".join(f"{item['name']}={item['value']}" for item in enum_values)
        return f"enum {name}[{values}]"
    if kind != "callable":
        return f"{kind} {name}"
    args = []
    for argument in arguments:
        value = f"{argument['type']} {argument['name']}"
        if argument["default"]:
            value += f"={argument['default']}"
        if argument["modifiers"]:
            value += "[" + ",".join(argument["modifiers"]) + "]"
        args.append(value)
    return f"{name}({';'.join(args)})->{return_type}"


def convert(value, ordinal, source_header):
    raw_name = value[0]
    if raw_name.startswith("enum "):
        kind, name = "enum", raw_name[5:]
    elif raw_name.startswith("class ") or raw_name.startswith("struct "):
        prefix = 6 if raw_name.startswith("class ") else 7
        kind, name = "class", raw_name[prefix:]
    else:
        kind, name = "callable", raw_name
    enum_values = []
    arguments = []
    if kind == "enum":
        enum_values = [{"name": item[0], "value": item[1]} for item in value[3]]
    else:
        arguments = [{"type": item[0], "name": item[1], "default": item[2], "modifiers": list(item[3])} for item in value[3]]
    return_type = value[4] or value[1] or ""
    return {"ordinal": ordinal, "sourceHeader": source_header, "kind": kind, "name": name, "identity": identity(kind, name, return_type, arguments, enum_values), "returnType": return_type, "modifiers": list(value[2]), "arguments": arguments, "enumValues": enum_values, "baseDeclaration": value[1] if kind == "class" else "", "documentation": value[5] or ""}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True)
    parser.add_argument("--opencv-root", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--module", default="ximgproc")
    args = parser.parse_args()
    workspace = Path(args.workspace).resolve()
    root = Path(args.opencv_root).resolve()
    output = Path(args.output).resolve()
    parser_path = root / "modules/python/src2/hdr_parser.py"
    module = args.module
    if module not in {"ximgproc", "optflow", "bgsegm", "face", "quality", "img_hash", "line_descriptor", "freetype", "alphamat", "intensity_transform", "plot", "bioinspired", "phase_unwrapping", "hfs", "fuzzy", "rapid", "shape", "structured_light", "saliency", "surface_matching", "reg", "xstereo", "xobjdetect", "superres", "dnn_superres", "wechat_qrcode", "cnn_3dobj", "signal", "dpm", "hdf", "text", "xfeatures2d", "dnn_objdetect", "ccalib", "julia", "rgbd", "cudaarithm", "cudabgsegm", "cudafilters", "cudaimgproc", "cudacodec", "cudawarping", "cudafeatures2d", "cudaobjdetect", "cudaoptflow", "cudastereo"}:
        raise RuntimeError("Unsupported contrib extraction module: " + module)
    contrib_include = root.parent.parent / f"opencv-source/opencv_contrib-5.0.0/modules/{module}/include/opencv2"
    contrib_root = contrib_include / module
    if module == "reg":
        umbrella = contrib_root / "map.hpp"
    elif module == "dnn_objdetect":
        umbrella = contrib_include / "core_detect.hpp"
    else:
        umbrella = contrib_include / f"{module}.hpp"
    module_headers = sorted(contrib_root.glob("*.hpp"))
    if module in {"xobjdetect", "dnn_objdetect"}:
        files = [umbrella]
    elif module == "superres":
        # The umbrella contains the FrameSource/SuperResolution declarations;
        # optical_flow.hpp contributes the dense-flow families.  Keep both
        # source headers in the measured closure instead of silently dropping
        # the umbrella when a subheader exists.
        files = [umbrella] + module_headers
    elif module == "rgbd":
        # The umbrella names five public component headers but has no parser
        # declarations itself.  Keep it and all component headers in scope.
        files = [umbrella] + module_headers
    else:
        files = module_headers if module_headers else [umbrella]
    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    hdr_module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(hdr_module)
    definitions = {"CV_VERSION_MAJOR": 5, "OPENCV_ABI_COMPATIBILITY": 500, "HAVE_OPENCV_FLANN": 1, "HAVE_OPENCV_DNN": 1}
    parser_instance = hdr_module.CppHeaderParser(preprocessor_definitions=dict(definitions))
    declarations = []
    source_headers = []
    for file in files:
        if module == "dnn_objdetect":
            # hdr_parser cannot parse this header's anonymous typedef struct.
            # Normalize only that spelling for parsing, then retain the exact
            # upstream header path/hash as the evidence source. This module has
            # no CV_WRAP annotations, so parse its public declarations in
            # non-wrapper mode rather than silently producing an empty map.
            header_text = file.read_text(encoding="utf-8")
            normalized_text = header_text.replace("typedef struct\n    {", "struct object\n    {", 1).replace("    } object;", "    };", 1)
            # hdr_parser records data members only when they carry CV_PROP;
            # expose the public fields as parser properties in the temporary
            # copy so the class metadata retains the complete data shape.
            for field in (
                "      int xmin, xmax;",
                "      int ymin, ymax;",
                "      size_t class_idx;",
                "      std::string label_name;",
                "      double class_prob;",
                "        std::vector<object> detections;",
            ):
                normalized_text = normalized_text.replace(field, field.replace("      ", "      CV_PROP_RW ", 1), 1)
            # Non-wrapper parser mode also admits protected members by design;
            # keep this map limited to the actual public native surface.
            normalized_text = normalized_text.replace("      protected:", "      private:", 1)
            if normalized_text == header_text:
                raise RuntimeError("Could not normalize the known dnn_objdetect object typedef for hdr_parser")
            with tempfile.TemporaryDirectory(prefix="dnn_objdetect_hdr_parser_") as temp_dir:
                parser_file = Path(temp_dir) / file.name
                parser_file.write_text(normalized_text, encoding="utf-8", newline="\n")
                parsed = parser_instance.parse(str(parser_file), wmode=False)
        else:
            parsed = parser_instance.parse(str(file), wmode=(module != "superres"))
            if module == "superres":
                # superres headers expose CV_EXPORTS rather than CV_WRAP.  The
                # non-wrapper parser mode therefore also emits the three
                # protected SuperResolution implementation hooks.  They are
                # deliberately excluded from the public header scope.
                protected = {
                    "cv.superres.SuperResolution.SuperResolution",
                    "cv.superres.SuperResolution.initImpl",
                    "cv.superres.SuperResolution.processImpl",
                }
                parsed = [value for value in parsed if value[0] not in protected]
        source = rel(file, workspace)
        source_headers.append({"path": source, "sha256": sha256(file), "startOrdinal": len(declarations), "declarationCount": len(parsed)})
        declarations.extend(convert(value, ordinal, source) for ordinal, value in enumerate(parsed, start=len(declarations)))
    identities = [item["identity"] for item in declarations]
    if len(identities) != len(set(identities)):
        duplicates = sorted(item for item in set(identities) if identities.count(item) > 1)
        raise RuntimeError(module + " parser closure contains duplicate identities: " + ", ".join(duplicates))
    title = {"ximgproc": "XImgProc", "optflow": "OptFlow", "bgsegm": "BgSegm", "face": "Face", "quality": "Quality", "img_hash": "ImgHash", "line_descriptor": "LineDescriptor", "freetype": "Freetype", "alphamat": "AlphaMat", "intensity_transform": "IntensityTransform", "plot": "Plot", "bioinspired": "BioInspired", "phase_unwrapping": "PhaseUnwrapping", "hfs": "Hfs", "fuzzy": "Fuzzy", "rapid": "Rapid", "shape": "Shape", "structured_light": "StructuredLight", "saliency": "Saliency", "surface_matching": "SurfaceMatching", "reg": "Reg", "xstereo": "XStereo", "xobjdetect": "XObjDetect", "superres": "Superres", "dnn_superres": "DnnSuperres", "wechat_qrcode": "WechatQRCode", "cnn_3dobj": "Cnn3DObj", "signal": "Signal", "dpm": "Dpm", "hdf": "Hdf", "text": "Text", "xfeatures2d": "XFeatures2D", "dnn_objdetect": "DnnObjDetect", "ccalib": "Ccalib", "julia": "Julia", "rgbd": "Rgbd", "cudaarithm": "CudaArithm", "cudabgsegm": "CudaBgSegm", "cudafilters": "CudaFilters", "cudaimgproc": "CudaImgProc", "cudacodec": "CudaCodec", "cudawarping": "CudaWarping", "cudafeatures2d": "CudaFeatures2D", "cudaobjdetect": "CudaObjDetect", "cudaoptflow": "CudaOptFlow", "cudastereo": "CudaStereo"}[module]
    result = {"schemaVersion": 1, "generator": f"tools/{title}UpstreamMap/extract_{module}.py", "upstreamOpenCvVersion": "5.0.0", "headerPath": rel(umbrella, workspace), "headerSha256": sha256(umbrella), "parserPath": rel(parser_path, workspace), "parserSha256": sha256(parser_path), "preprocessorDefinitions": definitions, "sourceHeaders": source_headers, "declarationCount": len(declarations), "declarations": declarations}
    if module == "cudaarithm":
        build_contract = root.parent.parent / "opencv-source/opencv_contrib-5.0.0/modules/cudaarithm/CMakeLists.txt"
        result["compatibilityHeaders"] = [{"path": rel(build_contract, workspace), "sha256": sha256(build_contract), "reason": "Upstream module activation and CUDA/NPP/optional cuBLAS/cuFFT dependency contract; parser declarations are not gated in the public header."}]
        result["moduleBuildCondition"] = "NOT_IOS_AND_NOT_WINRT_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)"
    if module == "cudabgsegm":
        build_contract = root.parent.parent / "opencv-source/opencv_contrib-5.0.0/modules/cudabgsegm/CMakeLists.txt"
        result["compatibilityHeaders"] = [{"path": rel(build_contract, workspace), "sha256": sha256(build_contract), "reason": "Upstream module activation and optional CUDA runtime dependency contract; parser declarations are not gated in the public header."}]
        result["moduleBuildCondition"] = "NOT_IOS_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)"
    if module == "cudafilters":
        build_contract = root.parent.parent / "opencv-source/opencv_contrib-5.0.0/modules/cudafilters/CMakeLists.txt"
        result["compatibilityHeaders"] = [{"path": rel(build_contract, workspace), "sha256": sha256(build_contract), "reason": "Upstream module activation, OpenCV dependency, and CUDA/NPP image-filter dependency contract; parser declarations are not gated in the public header."}]
        result["moduleBuildCondition"] = "NOT_IOS_AND_NOT_WINRT_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)"
    if module == "cudaimgproc":
        build_contract = root.parent.parent / "opencv-source/opencv_contrib-5.0.0/modules/cudaimgproc/CMakeLists.txt"
        result["compatibilityHeaders"] = [{"path": rel(build_contract, workspace), "sha256": sha256(build_contract), "reason": "Upstream module activation, image-processing dependency, and CUDA/NPP image-processing dependency contract; parser declarations are not gated in the public header."}]
        result["moduleBuildCondition"] = "NOT_IOS_AND_NOT_WINRT_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)"
    if module == "cudacodec":
        build_contract = root.parent.parent / "opencv-source/opencv_contrib-5.0.0/modules/cudacodec/CMakeLists.txt"
        result["compatibilityHeaders"] = [{"path": rel(build_contract, workspace), "sha256": sha256(build_contract), "reason": "Upstream module activation, CUDA/videoio/cudaarithm/cudawarping dependency, and optional NVIDIA Video Codec SDK dependency contract; parser declarations are not gated in the public header."}]
        result["moduleBuildCondition"] = "NOT_IOS_AND_NOT_APPLE_AND_NOT_WINRT_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)"
    if module == "cudawarping":
        build_contract = root.parent.parent / "opencv-source/opencv_contrib-5.0.0/modules/cudawarping/CMakeLists.txt"
        result["compatibilityHeaders"] = [{"path": rel(build_contract, workspace), "sha256": sha256(build_contract), "reason": "Upstream module activation, core/imgproc/geometry dependency, and CUDA/NPP image-warping dependency contract; parser declarations are not gated in the public header."}]
        result["moduleBuildCondition"] = "NOT_IOS_AND_NOT_WINRT_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)"
    if module == "cudafeatures2d":
        build_contract = root.parent.parent / "opencv-source/opencv_contrib-5.0.0/modules/cudafeatures2d/CMakeLists.txt"
        result["compatibilityHeaders"] = [{"path": rel(build_contract, workspace), "sha256": sha256(build_contract), "reason": "Upstream module activation and CUDA feature-detection dependency contract; parser declarations are not gated in the public header."}]
        result["moduleBuildCondition"] = "NOT_IOS_AND_NOT_WINRT_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)"
    if module == "cudaobjdetect":
        build_contract = root.parent.parent / "opencv-source/opencv_contrib-5.0.0/modules/cudaobjdetect/CMakeLists.txt"
        result["compatibilityHeaders"] = [{"path": rel(build_contract, workspace), "sha256": sha256(build_contract), "reason": "Upstream module activation, object-detection dependency, and CUDA runtime dependency contract; parser declarations are not gated in the public header."}]
        result["moduleBuildCondition"] = "NOT_IOS_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)"
    if module == "cudaoptflow":
        build_contract = root.parent.parent / "opencv-source/opencv_contrib-5.0.0/modules/cudaoptflow/CMakeLists.txt"
        result["compatibilityHeaders"] = [{"path": rel(build_contract, workspace), "sha256": sha256(build_contract), "reason": "Upstream module activation, optical-flow dependency, CUDA runtime, and optional NVIDIA Optical Flow SDK dependency contract; parser declarations are not gated in the public header."}]
        result["moduleBuildCondition"] = "NOT_IOS_AND_NOT_WINRT_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)"
    if module == "cudastereo":
        build_contract = root.parent.parent / "opencv-source/opencv_contrib-5.0.0/modules/cudastereo/CMakeLists.txt"
        result["compatibilityHeaders"] = [{"path": rel(build_contract, workspace), "sha256": sha256(build_contract), "reason": "Upstream module activation, stereo/geometry dependency, optional cudev, and CUDA runtime dependency contract; parser declarations are not gated in the public header."}]
        result["moduleBuildCondition"] = "NOT_IOS_AND_NOT_WINRT_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)"
    if module == "dnn_objdetect":
        result["parserMode"] = "non-wrapper-public-declarations"
        result["parserNormalization"] = "temporary parser-only normalization: name anonymous typedef struct object; annotate six public data members as CV_PROP_RW; protected -> private to exclude non-public methods; upstream source/hash unchanged"
    if module == "superres":
        result["parserMode"] = "non-wrapper-public-declarations"
        result["parserNormalization"] = "non-wrapper parser mode because superres uses CV_EXPORTS without CV_WRAP; exclude the three protected SuperResolution constructor/initImpl/processImpl declarations; upstream source/hash unchanged"
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(result, ensure_ascii=True, indent=2) + "\n", encoding="utf-8", newline="\n")
    print("{}_UPSTREAM_EXTRACTION_OK declarations={} headers={}".format(module.upper(), len(declarations), len(files)))


if __name__ == "__main__":
    main()
