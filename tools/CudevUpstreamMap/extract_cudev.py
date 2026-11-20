#!/usr/bin/env python3
"""Retain cudev's parser subset AND its uninstantiated template/macro source.

This inventory intentionally makes no complete callable/instantiation claim.
It requires only the Python standard library and the pinned OpenCV parser.
"""

import argparse
import importlib.util
import json
import re
import sys
import tempfile
from collections import Counter, defaultdict
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parents[1] / "XImgProcUpstreamMap"))
from extract_ximgproc import convert, rel, sha256
from source_inventory import inventory, macro_invocations, self_test

DEFINITIONS = {"CV_VERSION_MAJOR": 5, "OPENCV_ABI_COMPATIBILITY": 500, "__CUDACC__": 1,
               "CUDART_VERSION": 12040, "__CUDACC_VER_MAJOR__": 12, "__CUDACC_VER_MINOR__": 4,
               "__CUDA_ARCH__": 750, "CV_CUDEV_ARCH": 750}
ATTRIBUTES = re.compile(r"\b(?:__host__|__device__|__global__|__forceinline__|__shared__|__constant__)\b")
LIMITATION = ("The pinned hdr_parser skips template declarations and does not expand declaration macros. "
              "Its declaration/callable counts cover only the non-template parser subset under the recorded source configuration. "
              "The companion source inventory records template definitions (including nested template-template parameters), "
              "function-like macro definitions/invocations and every conditional directive across all installed headers. "
              "These lexical source units overlap and are not concrete callable counts. A linked C++ CST inventory separately "
              "enumerates source function declarations, class-template members and the 44 finite local declaration generators. "
              "External using/include closure and concrete template instantiations are not semantically enumerated. "
              "No CUDA compilation, linking, device/runtime validation or complete callable parity is claimed.")


def write_json(path, data):
    with path.open("w", encoding="utf-8", newline="\n") as stream:
        stream.write(json.dumps(data, ensure_ascii=True, indent=2) + "\n")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True)
    parser.add_argument("--opencv-root", required=True)
    parser.add_argument("--output", required=True)
    args = parser.parse_args()
    workspace, root, output = (Path(value).resolve() for value in (args.workspace, args.opencv_root, args.output))
    module = workspace / "opencv-source/opencv_contrib-5.0.0/modules/cudev"
    include = module / "include/opencv2"
    files = sorted(include.rglob("*.hpp"))
    if len(files) != 76:
        raise RuntimeError("cudev installed header inventory drifted")
    parser_path = root / "modules/python/src2/hdr_parser.py"
    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    hdr = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(hdr)
    self_test()
    source_headers, declarations, reviews, token_sets, spans = [], [], [], {}, {}
    static_name_repairs = []
    definitions = defaultdict(list)
    with tempfile.TemporaryDirectory(prefix="cudev_hdr_parser_") as temp_dir:
        for file in files:
            source, text = rel(file, workspace), file.read_text(encoding="utf-8")
            normalized = ATTRIBUTES.sub("", text)
            temp_file = Path(temp_dir) / file.relative_to(include)
            temp_file.parent.mkdir(parents=True, exist_ok=True)
            temp_file.write_text(normalized, encoding="utf-8", newline="\n")
            values = hdr.CppHeaderParser(preprocessor_definitions=dict(DEFINITIONS)).parse(str(temp_file), wmode=False)
            # parse_func_decl computes namespace-scope static names through
            # parse_arg before no-wrap mode correctly strips static. Repair
            # that return-type contamination without removing /S metadata.
            for value in values:
                if "/S" not in value[2] or " " not in value[0] or "operator " in value[0]:
                    continue
                prefix, name = value[0].rsplit(".", 1)
                return_type = value[4] or value[1]
                if not name.startswith(return_type + " ") or not re.fullmatch(r"[A-Za-z_]\w*", name[len(return_type) + 1:]):
                    raise RuntimeError("Unexpected cudev static parser name: " + value[0])
                corrected = prefix + "." + name[len(return_type) + 1:]
                static_name_repairs.append({"sourceHeader": source, "parserName": value[0], "correctedName": corrected})
                value[0] = corrected
            source_headers.append({"path": source, "sha256": sha256(file), "startOrdinal": len(declarations), "declarationCount": len(values)})
            start = len(declarations)
            declarations.extend(convert(value, start + index, source) for index, value in enumerate(values))
            review, tokens, span = inventory(text)
            review = {"path": source, "sha256": sha256(file), **review}
            for index, definition in enumerate(review["macroDefinitions"]):
                definition["id"] = f"{source}:{definition['line']}:{index}"
                definitions[definition["name"]].append(definition["id"])
            reviews.append(review)
            token_sets[source], spans[source] = tokens, span
        probes = []
        for arch in (0, 120, 200, 300, 750):
            config = {**DEFINITIONS, "CV_CUDEV_ARCH": arch}
            if arch:
                config["__CUDA_ARCH__"] = arch
            else:
                config.pop("__CUDA_ARCH__")
            counts = {}
            for name in ("warp/shuffle.hpp", "util/atomic.hpp", "util/simd_functions.hpp"):
                path = Path(temp_dir) / "cudev" / name
                values = hdr.CppHeaderParser(preprocessor_definitions=dict(config)).parse(str(path), wmode=False)
                counts[name] = len(values)
            probes.append({"configuration": f"source-arch-{arch}", "preprocessorDefinitions": config, "headerDeclarationCounts": counts})
    duplicate = [key for key, count in Counter(row["identity"] for row in declarations).items() if count > 1]
    if duplicate:
        raise RuntimeError("cudev parser subset contains duplicate identities: " + ", ".join(duplicate))
    for review, file in zip(reviews, files):
        source = review["path"]
        review["macroInvocations"] = macro_invocations(file.read_text(encoding="utf-8"), token_sets[source], spans[source], definitions)
    counts = {field + "Count": sum(len(row[field]) for row in reviews) for field in ("templates", "templateParameters", "macroDefinitions", "macroInvocations", "conditionalDirectives")}
    counts["macroBodyTemplateKeywordCount"] = sum(item["templateKeywordCount"] for row in reviews for item in row["macroDefinitions"])
    extension = {"schemaVersion": 1, "generator": "tools/CudevUpstreamMap/source_inventory.py", "upstreamOpenCvVersion": "5.0.0",
                 "scope": "All 76 installed cudev .hpp files, including detail headers, with all source conditional branches retained.",
                 "coordinateSystem": "Zero-based Unicode offsets, exclusive end, UTF-8 source normalized to LF; line numbers are one-based.",
                 "reviewDisposition": "intentionally-omitted-unwrapped-device-source", "limitation": LIMITATION,
                 "completeCallableCoverageClaimed": False, "macroExpansionClaimed": False, "templateInstantiationCoverageClaimed": False,
                 "counts": counts, "headers": reviews}
    extension_path = output.parent / "cudev-source-reviewed-extensions.json"
    output.parent.mkdir(parents=True, exist_ok=True)
    write_json(extension_path, extension)
    umbrella = include / "cudev.hpp"
    result = {"schemaVersion": 1, "generator": "tools/CudevUpstreamMap/extract_cudev.py", "upstreamOpenCvVersion": "5.0.0",
              "headerPath": rel(umbrella, workspace), "headerSha256": sha256(umbrella), "parserPath": rel(parser_path, workspace), "parserSha256": sha256(parser_path),
              "preprocessorDefinitions": DEFINITIONS, "parserMode": "non-wrapper-non-template-subset",
              "parserNormalization": "CUDA attribute tokens are removed only in temporary copies. Thirteen namespace-scope static parser names have return-type contamination repaired after parsing; /S metadata is retained. Static/const qualifiers, operators and upstream source hashes are retained. Templates and declaration macros remain outside the pinned parser subset.",
              "staticParserNameRepairs": static_name_repairs,
              "sourceHeaders": source_headers, "declarationCount": len(declarations), "declarations": declarations,
              "parserDuplicateIdentities": duplicate, "parserConfigurationProbes": probes,
              "sourceReviewedExtensionPath": "compatibility/cudev-source-reviewed-extensions.json", "sourceReviewedExtensionSha256": sha256(extension_path),
              "sourceInventoryCounts": counts, "sourceInventoryLimitation": LIMITATION,
              "completeCallableCoverageClaimed": False, "cudaExecutionVerified": False,
              "compatibilityHeaders": [{"path": rel(module / "CMakeLists.txt", workspace), "sha256": sha256(module / "CMakeLists.txt"), "reason": "HAVE_CUDA module disable; recursive installed-header scope; opencv_core include dependency and conditional CUDA tests."}],
              "moduleBuildCondition": "HAVE_CUDA", "moduleDependencies": ["opencv_core"],
              "externalDependencies": ["CUDA toolkit/runtime and compatible NVIDIA driver/device", "CUDA/Thrust headers including tuple", "OpenCV core CUDA GpuMat and Stream infrastructure"],
              "sourceConditionBoundaries": ["CV_CUDEV_ARCH >= 120", "CV_CUDEV_ARCH >= 200", "CV_CUDEV_ARCH >= 300", "__CUDACC_VER_MAJOR__ >= 9", "CUDART_VERSION < 12040"],
              "configurationLimitation": "The recorded CUDA 12.4 / source architecture 750 defines are extraction inputs, not measured compiler or device capabilities. Header visibility does not establish build, package or runtime availability."}
    write_json(output, result)
    print(f"CUDEV_UPSTREAM_EXTRACTION_OK declarations={len(declarations)} callable={sum(row['kind'] == 'callable' for row in declarations)} headers=76 sourceInventory={counts}")


if __name__ == "__main__":
    main()
