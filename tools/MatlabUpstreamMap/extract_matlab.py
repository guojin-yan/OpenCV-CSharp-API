#!/usr/bin/env python3
"""Retain MATLAB's installed source and an explicitly partial parser map."""

import argparse
import importlib.util
import json
import re
import sys
import tempfile
from collections import Counter
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parents[1] / "XImgProcUpstreamMap"))
sys.path.insert(0, str(Path(__file__).parents[1] / "CudevUpstreamMap"))
from extract_ximgproc import convert, rel, sha256
from source_inventory import inventory, self_test

HEADERS = ["bridge.hpp", "map.hpp", "mxarray.hpp", "transpose.hpp"]
CPP_DEFINE = "MATLAB_SOURCE_CPLUSPLUS"
DEFINITIONS = {"CV_VERSION_MAJOR": 5, "OPENCV_ABI_COMPATIBILITY": 500, CPP_DEFINE: 201103}
BUILD_CONDITION = "MATLAB_FOUND_AND_PYTHON_AND_JINJA2_AND_MATCHING_BITNESS_AND_MEX_PROBES_AND_NOT_ANDROID_APPLE_FRAMEWORK"
LIMITATION = (
    "This is the pinned hdr_parser non-template public subset, not complete MATLAB callable coverage. "
    "All four installed headers and every lexical template/conditional branch remain in a linked source inventory. "
    "The parser ignores __cplusplus checks, so temporary copies rename that identifier to a recorded numeric input. "
    "Templates, class-template members, specializations and generated +cv gateways are not enumerated as concrete APIs. "
    "External mex.h/standard-library/OpenCV include closure, C++ type checking, MATLAB/MEX build/link/runtime "
    "and complete callable parity remain unverified; the package has no MATLAB native or managed wrapper."
)
COMPATIBILITY = [
    ("CMakeLists.txt", "BINDINGS module; MATLAB/Python/Jinja2/platform/bitness and generator/MEX probes; optional OpenCV inputs and installed headers/+cv gateways."),
    ("cmake/init.cmake", "Registers the external MATLAB discovery script."),
    ("cmake/OpenCVFindMatlab.cmake", "External MATLAB installation, mex.h, mx/mex/mat libraries, MEX executable/extension and architecture discovery."),
    ("compile.cmake", "Invokes MEX for generated gateway sources and unconditionally skips Debug configurations, despite the preceding MSVC-only comment."),
    ("README.md", "MATLAB generator, MEX usage, scoped mxArray ownership and generated-binding installation."),
    ("generator/gen_matlab.py", "Parses available module headers and renders generated binding templates; not this installed-helper declaration map."),
    ("generator/parse_tree.py", "Transforms parser declarations into generator namespaces/classes/functions."),
    ("generator/templates/functional.cpp", "Generated MEX entrypoint, argument transport, exception and returned mxArray ownership boundary."),
    ("generator/templates/template_class_base.cpp", "Generated object dispatch and instance storage boundary."),
    ("generator/templates/template_function_base.cpp", "Generated per-function MEX gateway template."),
    ("test/test_compiler.cpp", "Upstream configure-time MEX compile fixture; pinned source is not an executed probe here."),
    ("test/test_generator.hpp", "Upstream configure-time generator fixture; pinned source is not executed MATLAB evidence."),
]


def write_json(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as stream:
        stream.write(json.dumps(data, ensure_ascii=True, indent=2) + "\n")


def parse_header(hdr, path, cpp=201103, normalize=True):
    text = path.read_text(encoding="utf-8")
    definitions = {**DEFINITIONS, CPP_DEFINE: cpp}
    if not normalize:
        definitions.pop(CPP_DEFINE)
        definitions["__cplusplus"] = cpp
        return hdr.CppHeaderParser(preprocessor_definitions=definitions).parse(str(path), wmode=False)
    # No upstream source is edited. The pinned evaluator deliberately ignores
    # __cplusplus directives; renaming just that identifier makes the measured
    # source-language branch explicit without claiming compiler verification.
    text = re.sub(r"\b__cplusplus\b", CPP_DEFINE, text)
    text = re.sub(r"(?m)^(#if " + CPP_DEFINE + r" >= 201103)L$", r"\1", text)
    with tempfile.TemporaryDirectory(prefix="matlab_hdr_parser_") as directory:
        temporary = Path(directory) / path.name
        temporary.write_text(text, encoding="utf-8", newline="\n")
        return hdr.CppHeaderParser(preprocessor_definitions=definitions).parse(str(temporary), wmode=False)


def extract(workspace, root, output):
    module = workspace / "opencv-source/opencv_contrib-5.0.0/modules/matlab"
    include = module / "include/opencv2/matlab"
    if sorted(path.relative_to(include).as_posix() for path in include.rglob("*.hpp")) != HEADERS:
        raise RuntimeError("MATLAB installed header inventory drifted")
    parser_path = root / "modules/python/src2/hdr_parser.py"
    spec = importlib.util.spec_from_file_location("matlab_pinned_hdr_parser", parser_path)
    hdr = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(hdr)
    self_test()
    declarations, sources, reviews, repairs, duplicates = [], [], [], [], []
    for name in HEADERS:
        path = include / name
        source = rel(path, workspace)
        values = parse_header(hdr, path)
        seen, start = {}, len(declarations)
        for value in values:
            original = value[0]
            if original in {"matlab.void conditionalError", "matlab.void error"}:
                value[0] = original.replace("matlab.void ", "matlab.")
            elif original.startswith("cv.bridge.operator "):
                value[0] = "cv.bridge.Bridge.operator " + original[len("cv.bridge.operator "):].replace(".", "::")
            if value[0] != original:
                repairs.append({"sourceHeader": source, "parserName": original, "correctedName": value[0]})
            row = convert(value, len(declarations), source)
            row["requiresCpp11"] = row["name"] == "matlab.MxArray.MxArray" and any(arg["type"] == "MxArray&&" for arg in row["arguments"])
            if row["identity"] in seen:
                if name != "bridge.hpp" or row["identity"] not in {
                    "cv.bridge.Bridge.operator=(const cv::Mat& mat)->Bridge&",
                    "cv.bridge.Bridge.toMat()->cv::Mat",
                }:
                    raise RuntimeError("Unexpected duplicate MATLAB parser identity: " + row["identity"])
                if declarations[seen[row["identity"]]]["modifiers"] != row["modifiers"]:
                    raise RuntimeError("MATLAB inline reconciliation lost const/static metadata")
                duplicates.append({"sourceHeader": source, "identity": row["identity"], "retainedOrdinal": seen[row["identity"]],
                                   "reason": "Out-of-class inline definition reconciled to the existing class declaration; const/operator metadata is identical."})
                continue
            seen[row["identity"]] = row["ordinal"]
            declarations.append(row)
        sources.append({"path": source, "sha256": sha256(path), "startOrdinal": start, "declarationCount": len(declarations) - start})
        review, _, _ = inventory(path.read_text(encoding="utf-8"))
        reviews.append({"path": source, "sha256": sha256(path), **review})
    if len({row["identity"] for row in declarations}) != len(declarations):
        raise RuntimeError("MATLAB normalized parser identities are not unique")
    counts = {field + "Count": sum(len(row[field]) for row in reviews)
              for field in ("templates", "templateParameters", "macroDefinitions", "conditionalDirectives")}
    counts["templateKindCounts"] = dict(sorted(Counter(unit["kind"] for header in reviews for unit in header["templates"]).items()))
    extension = {
        "schemaVersion": 1, "generator": "tools/CudevUpstreamMap/source_inventory.py", "upstreamOpenCvVersion": "5.0.0",
        "scope": "All four installed opencv2/matlab/*.hpp headers with every source conditional branch retained.",
        "coordinateSystem": "Zero-based Unicode offsets, exclusive end, UTF-8 source normalized to LF; line numbers are one-based.",
        "reviewDisposition": "intentionally-omitted-unwrapped-matlab-source", "limitation": LIMITATION,
        "completeCallableCoverageClaimed": False, "macroExpansionClaimed": False, "templateInstantiationCoverageClaimed": False,
        "counts": counts, "headers": reviews,
    }
    extension_path = output.parent / "matlab-source-reviewed-extensions.json"
    write_json(extension_path, extension)
    probes = []
    for cpp in (199711, 201103, 201703):
        probes.append({"configuration": f"source-cplusplus-{cpp}", "cplusplus": cpp,
                       "originalMxarrayDeclarationCount": len(parse_header(hdr, include / "mxarray.hpp", cpp, False)),
                       "normalizedMxarrayDeclarationCount": len(parse_header(hdr, include / "mxarray.hpp", cpp)),
                       "normalizedMapDeclarationCount": len(parse_header(hdr, include / "map.hpp", cpp))})
    result = {
        "schemaVersion": 1, "generator": "tools/MatlabUpstreamMap/extract_matlab.py", "upstreamOpenCvVersion": "5.0.0",
        "headerPath": rel(include / "bridge.hpp", workspace), "headerSha256": sha256(include / "bridge.hpp"),
        "parserPath": rel(parser_path, workspace), "parserSha256": sha256(parser_path),
        "parserMode": "non-wrapper-non-template-subset", "preprocessorDefinitions": DEFINITIONS,
        "parserNormalization": "Temporary copies rename __cplusplus to MATLAB_SOURCE_CPLUSPLUS=201103 and remove the L integer suffix only in its two >= 201103L directives for the Python expression evaluator. Two static names lose parser-injected return-type text; qualified conversion operators regain Bridge ownership and C++ target spelling. Two inline definitions reconcile to their class declarations. /S and /C metadata remain unchanged; upstream source files remain untouched.",
        "parserNameRepairs": repairs, "reconciledInlineDefinitions": duplicates, "parserConfigurationProbes": probes,
        "sourceHeaders": sources, "declarationCount": len(declarations), "declarations": declarations,
        "sourceReviewedExtensionPath": "compatibility/matlab-source-reviewed-extensions.json",
        "sourceReviewedExtensionSha256": sha256(extension_path), "sourceInventoryCounts": counts,
        "sourceInventoryLimitation": LIMITATION, "completeCallableCoverageClaimed": False, "matlabExecutionVerified": False,
        "compatibilityHeaders": [{"path": rel(module / name, workspace), "sha256": sha256(module / name), "reason": reason}
                                 for name, reason in COMPATIBILITY],
        "moduleBuildCondition": BUILD_CONDITION,
        "optionalGeneratorModuleDependencies": re.findall(r"\bopencv_[a-z0-9_]+\b", (module / "CMakeLists.txt").read_text(encoding="utf-8").split("ocv_add_module(matlab", 1)[1].split(")", 1)[0]),
        "ownershipBoundaries": ["MxArray from const mxArray* borrows and does not destroy the input.",
                                "MxArray copy construction duplicates the array; owned destruction calls mxDestroyArray.",
                                "releaseOwnership clears ownership before the pointer is returned to MATLAB.",
                                "Bridge Mat conversion uses deepCopyAndTranspose; generated object pointers and MEX exception handling require their own lifetime contract."],
        "generatedBindingSurfaceIncluded": False,
        "upstreamPlaceholderBoundaries": {
            "noOpAssignmentDefinitionCount": len(re.findall(r'Bridge& operator=\([^\n]*\) \{ return \*this; \}', (include / "bridge.hpp").read_text(encoding="utf-8"))),
            "boolConversionReturnsZero": "bool toBool() { return 0; }" in (include / "bridge.hpp").read_text(encoding="utf-8"),
            "limitation": "Several Bridge conversions are upstream placeholder bodies. Header declarations and parser identities are not conversion correctness or MATLAB runtime evidence.",
        },
    }
    write_json(output, result)
    print(f"MATLAB_UPSTREAM_EXTRACTION_OK declarations={len(declarations)} callable={sum(row['kind'] == 'callable' for row in declarations)} headers=4 repairs={len(repairs)} inlineReconciliations={len(duplicates)} sourceInventory={counts}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True)
    parser.add_argument("--opencv-root", required=True)
    parser.add_argument("--output", required=True)
    arguments = parser.parse_args()
    extract(*(Path(value).resolve() for value in (arguments.workspace, arguments.opencv_root, arguments.output)))
