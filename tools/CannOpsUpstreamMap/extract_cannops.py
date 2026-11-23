#!/usr/bin/env python3
"""Extract the installed CANN header closure with the pinned OpenCV parser."""

import argparse
import importlib.util
import json
import re
import sys
import tempfile
from collections import Counter
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parents[1] / "XImgProcUpstreamMap"))
from extract_ximgproc import convert, rel, sha256


HEADERS = [
    "ascendc_kernels.hpp", "cann.hpp", "cann.inl.hpp", "cann_call.hpp",
    "cann_interface.hpp", "cann_private.hpp", "dvpp_call.hpp", "stream_accessor.hpp",
]
BUILD_CONDITION = "HAVE_CANN_AND_SUPPORTED_UNIX_X86_64_OR_AARCH64_AND_NOT_IOS_WINRT_ANDROID_APPLE_WIN32_AND_ASCENDC_TOOLCHAIN"
DEFINITIONS = {"CV_VERSION_MAJOR": 5, "OPENCV_ABI_COMPATIBILITY": 500, "HAVE_CANN": 1}


def callable_key(value):
    return value[0], tuple(argument[0] for argument in value[3])


def normalize_impl_classes(text):
    for owner in ("AscendStream", "AscendEvent"):
        pattern = r"class " + owner + r"::Impl\s*\{.*?\n\};"
        matches = list(re.finditer(pattern, text, re.DOTALL))
        if len(matches) != 1:
            raise RuntimeError(f"CANN qualified implementation class drifted: {owner}")
        match = matches[0]
        block = re.sub(r"\bImpl\b", owner + "_Impl", match.group())
        block = block.replace("class " + owner + "::" + owner + "_Impl", "class " + owner + "_Impl")
        text = text[:match.start()] + block + text[match.end():]
    return text


def restore_impl_names(value):
    for owner in ("AscendStream", "AscendEvent"):
        value[0] = value[0].replace("cv.cann." + owner + "_Impl", "cv.cann." + owner + ".Impl")
        value[0] = value[0].replace("." + owner + "_Impl", ".Impl").replace(".~" + owner + "_Impl", ".~Impl")
        for index in (1, 4):
            if isinstance(value[index], str):
                value[index] = value[index].replace(owner + "_Impl", owner + "::Impl")
        for argument in value[3]:
            argument[0] = argument[0].replace(owner + "_Impl", owner + "::Impl")
    return value


def main():
    args_parser = argparse.ArgumentParser()
    args_parser.add_argument("--workspace", required=True)
    args_parser.add_argument("--opencv-root", required=True)
    args_parser.add_argument("--output", required=True)
    args_parser.add_argument("--upstream-version", choices=("5.0.0", "5.1.0"), default="5.0.0")
    args = args_parser.parse_args()
    workspace, root, output = (Path(value).resolve() for value in (args.workspace, args.opencv_root, args.output))
    module = workspace / f"opencv-source/opencv_contrib-{args.upstream_version}/modules/cannops"
    include = module / "include/opencv2"
    parser_path = root / "modules/python/src2/hdr_parser.py"
    if sorted(path.name for path in include.glob("*.hpp")) != HEADERS:
        raise RuntimeError("CANN installed header inventory drifted")
    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    hdr = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(hdr)

    def parse(path):
        return hdr.CppHeaderParser(preprocessor_definitions=dict(DEFINITIONS)).parse(str(path), wmode=False)

    declarations, source_headers, inline_definitions, private_inline = [], [], [], []
    base = parse(include / "cann.hpp")
    with tempfile.TemporaryDirectory(prefix="opencv-cannops-parser-") as temporary:
        for name in HEADERS:
            path = include / name
            if name == "cann_call.hpp":
                temporary_header = Path(temporary) / name
                temporary_header.write_text(normalize_impl_classes(path.read_text(encoding="utf-8")), encoding="utf-8")
                values = [restore_impl_names(value) for value in parse(temporary_header)]
            else:
                values = parse(path)
            parser_count, start = len(values), len(declarations)
            if name == "cann.inl.hpp":
                for value in values:
                    matches = [candidate for candidate in base if callable_key(candidate) == callable_key(value)]
                    if len(matches) == 1:
                        inline_definitions.append(convert(matches[0], 0, rel(include / "cann.hpp", workspace))["identity"])
                    elif not matches and callable_key(value) in {
                        ("cv.cann.AscendStream.AscendStream", ("const Ptr<AscendStream::Impl>&",)),
                        ("cv.cann.AscendEvent.AscendEvent", ("const Ptr<AscendEvent::Impl>&",)),
                    }:
                        private_inline.append(convert(value, 0, rel(path, workspace))["identity"])
                    else:
                        raise RuntimeError("CANN inline definition has no unique declaration/private-constructor classification")
                values = []
            for value in values:
                declarations.append(convert(value, len(declarations), rel(path, workspace)))
            source_headers.append({"path": rel(path, workspace), "sha256": sha256(path), "startOrdinal": start,
                                   "declarationCount": len(values), "parserDeclarationCount": parser_count})

    duplicate_identities = sorted(key for key, count in Counter(row["identity"] for row in declarations).items() if count > 1)
    if duplicate_identities or len(inline_definitions) != 16 or len(private_inline) != 2:
        raise RuntimeError("CANN declaration/inline reconciliation drifted")
    compatibility = [
        (module / "CMakeLists.txt", "Module platform exclusions, HAVE_CANN gate, OpenCV core and CANN library dependencies, and AscendC kernel integration."),
        (module / "ascendc_kernels/CMakeLists.txt", "External AscendC compiler package, SOC_VERSION, RUN_MODE and threshold kernel build contract."),
        (root / "cmake/OpenCVFindCANN.cmake", "UNIX/x86_64/amd64/aarch64, external toolkit/shared libraries and VALID_ASCENDCL compile probe."),
        (module / "tutorials/ascend_npu_image_processing.markdown", "External Ascend NPU/CANN execution and stream/event synchronization ownership."),
    ]
    result = {
        "schemaVersion": 1, "generator": "tools/CannOpsUpstreamMap/extract_cannops.py", "upstreamOpenCvVersion": args.upstream_version,
        "headerPath": rel(include / "cann.hpp", workspace), "headerSha256": sha256(include / "cann.hpp"),
        "parserPath": rel(parser_path, workspace), "parserSha256": sha256(parser_path),
        "preprocessorDefinitions": DEFINITIONS, "parserMode": "non-wrapper-public-declarations",
        "parserNormalization": "All eight installed headers remain in source scope, including implementation helpers. Qualified AscendStream::Impl and AscendEvent::Impl class/constructor spellings are renamed only in a temporary parser copy and restored in identities. Sixteen inline definitions are reconciled to existing declarations by name and argument types; two inline definitions of private constructors remain explicitly excluded. NEVER_DEFINED binding-only placeholders are inactive. No upstream source file is modified.",
        "qualifiedImplementationClasses": ["cv.cann.AscendStream.Impl", "cv.cann.AscendEvent.Impl"],
        "inlineDefinitionIdentities": inline_definitions, "excludedPrivateInlineDefinitions": private_inline,
        "bindingOnlyPlaceholderBlockCount": len(re.findall(r"^#ifdef NEVER_DEFINED", (include / "cann_interface.hpp").read_text(encoding="utf-8"), re.MULTILINE)),
        "parserDuplicateIdentities": duplicate_identities, "sourceHeaders": source_headers,
        "declarationCount": len(declarations), "declarations": declarations,
        "compatibilityHeaders": [{"path": rel(path, workspace), "sha256": sha256(path), "reason": reason} for path, reason in compatibility],
        "moduleDependencies": ["opencv_core"], "externalLibraries": "CANN_LIBRARIES",
        "moduleBuildCondition": BUILD_CONDITION,
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    with output.open("w", encoding="utf-8", newline="\n") as stream:
        stream.write(json.dumps(result, ensure_ascii=True, indent=2) + "\n")
    print(f"CANNOPS_UPSTREAM_EXTRACTION_OK declarations={len(declarations)} headers={len(source_headers)} callable={sum(row['kind'] == 'callable' for row in declarations)} inlineReconciled=16 privateInlineExcluded=2")


if __name__ == "__main__":
    main()
