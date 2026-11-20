#!/usr/bin/env python3
"""Extract all installed SFM headers and retain the Ceres build distinction."""

import argparse
import importlib.util
import json
import re
import sys
from collections import Counter
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parents[1] / "XImgProcUpstreamMap"))
from extract_ximgproc import convert, rel, sha256


HEADERS = [
    "sfm.hpp", "sfm/conditioning.hpp", "sfm/fundamental.hpp", "sfm/io.hpp",
    "sfm/numeric.hpp", "sfm/projection.hpp", "sfm/reconstruct.hpp", "sfm/robust.hpp",
    "sfm/simple_pipeline.hpp", "sfm/triangulation.hpp",
]
CERES_HEADERS = {"sfm/reconstruct.hpp", "sfm/simple_pipeline.hpp"}
BUILD_CONDITION = "HAVE_EIGEN_AND_SFM_DEPS_OK"
DEFINITIONS = {"CV_VERSION_MAJOR": 5, "OPENCV_ABI_COMPATIBILITY": 500, "CERES_FOUND": 1}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True)
    parser.add_argument("--opencv-root", required=True)
    parser.add_argument("--output", required=True)
    args = parser.parse_args()
    workspace, root, output = (Path(value).resolve() for value in (args.workspace, args.opencv_root, args.output))
    module = workspace / "opencv-source/opencv_contrib-5.0.0/modules/sfm"
    include = module / "include/opencv2"
    if sorted(path.relative_to(include).as_posix() for path in include.rglob("*.hpp")) != HEADERS:
        raise RuntimeError("SFM installed header inventory drifted")
    parser_path = root / "modules/python/src2/hdr_parser.py"
    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    hdr = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(hdr)

    def parse(path, definitions):
        return hdr.CppHeaderParser(preprocessor_definitions=dict(definitions)).parse(str(path), wmode=False)

    umbrella = include / "sfm.hpp"
    umbrella_text = umbrella.read_text(encoding="utf-8")
    umbrella_names = re.findall(r"^#include <opencv2/(sfm/[^>]+)>", umbrella_text, re.MULTILINE)
    if sorted(set(HEADERS[1:]) - set(umbrella_names)) != ["sfm/robust.hpp"] or len(umbrella_names) != 8:
        raise RuntimeError("SFM umbrella/standalone robust header closure drifted")
    declarations, source_headers = [], []
    for name in HEADERS:
        path = include / name
        values, start = parse(path, DEFINITIONS), len(declarations)
        for value in values:
            row = convert(value, len(declarations), rel(path, workspace))
            row["requiresCeres"] = name in CERES_HEADERS
            declarations.append(row)
        source_headers.append({"path": rel(path, workspace), "sha256": sha256(path),
                               "startOrdinal": start, "declarationCount": len(values),
                               "requiresCeres": name in CERES_HEADERS})
    duplicates = sorted(key for key, count in Counter(row["identity"] for row in declarations).items() if count > 1)
    if duplicates:
        raise RuntimeError("SFM parser emitted duplicate exact identities")
    probes = []
    for name, value in (("undefined", None), ("defined-zero", 0), ("defined-one", 1)):
        definitions = {key: item for key, item in DEFINITIONS.items() if key != "CERES_FOUND"}
        if value is not None:
            definitions["CERES_FOUND"] = value
        probes.append({"configuration": name, "preprocessorDefinitions": dict(definitions),
                       "reconstructHeaderDeclarationCount": len(parse(include / "sfm/reconstruct.hpp", definitions))})
    if [probe["reconstructHeaderDeclarationCount"] for probe in probes] != [0, 4, 4]:
        raise RuntimeError("SFM defined(CERES_FOUND) header guard semantics drifted")
    compatibility = [
        ("CMakeLists.txt", "Eigen and Glog/Gflags compile/link probe; optional Ceres definition and module dependencies."),
        ("src/libmv_light/libmv/CMakeLists.txt", "simple_pipeline subdirectory is built only when Ceres_FOUND is true."),
        ("src/libmv_light/libmv/simple_pipeline/CMakeLists.txt", "External CERES_LIBRARIES linkage for the reconstruction pipeline."),
        ("src/reconstruct.cpp", "Reconstruction definitions require CERES_FOUND; all four affine branches raise StsNotImplemented."),
        ("src/simple_pipeline.cpp", "Pipeline implementation definitions require CERES_FOUND even though its direct header is unguarded."),
        ("src/fundamental.cpp", "The computeOrientation implementation retains an upstream TODO: finish implementation."),
        ("README.md", "Documentary Linux/GNU platform advice, external dependencies and caller image/track data."),
        ("tutorials/sfm_installation/sfm_installation.markdown", "Eigen/Glog/Gflags installation and Ceres reconstruction requirement."),
    ]
    result = {
        "schemaVersion": 1, "generator": "tools/SfmUpstreamMap/extract_sfm.py", "upstreamOpenCvVersion": "5.0.0",
        "headerPath": rel(umbrella, workspace), "headerSha256": sha256(umbrella),
        "parserPath": rel(parser_path, workspace), "parserSha256": sha256(parser_path),
        "preprocessorDefinitions": DEFINITIONS, "parserMode": "non-wrapper-public-declarations",
        "parserNormalization": "No source normalization or identity deduplication is needed. All ten installed headers remain in scope, including standalone robust.hpp and the optional Ceres headers. Anonymous enums retain exact parser identities and value lists.",
        "umbrellaIncludes": [rel(include / name, workspace) for name in umbrella_names],
        "standaloneInstalledHeaders": [rel(include / "sfm/robust.hpp", workspace)],
        "ceresConditionalHeaders": [rel(include / name, workspace) for name in sorted(CERES_HEADERS)],
        "ceresHeaderGuardProbes": probes,
        "ceresHeaderGuardLimitation": "reconstruct.hpp tests defined(CERES_FOUND), so defining the macro as zero still emits declarations. sfm.hpp and implementation files test its value; direct header visibility is not linkable Ceres support. simple_pipeline.hpp has no Ceres guard of its own.",
        "parserDuplicateIdentities": duplicates, "sourceHeaders": source_headers,
        "declarationCount": len(declarations), "declarations": declarations,
        "baseDeclarationCount": sum(not row["requiresCeres"] for row in declarations),
        "ceresDeclarationCount": sum(row["requiresCeres"] for row in declarations),
        "ceresCallableCount": sum(row["requiresCeres"] and row["kind"] == "callable" for row in declarations),
        "compatibilityHeaders": [{"path": rel(module / path, workspace), "sha256": sha256(module / path), "reason": reason} for path, reason in compatibility],
        "moduleDependencies": ["opencv_core", "opencv_geometry", "opencv_features", "opencv_xfeatures2d", "opencv_imgcodecs"],
        "moduleBuildCondition": BUILD_CONDITION, "ceresBuildCondition": BUILD_CONDITION + "_AND_CERES_FOUND",
        "readmePlatformAdvice": "The pinned README says Linux/GNU systems only; CMake has no corresponding unconditional platform disable and contains Windows-specific handling. Neither establishes verified package platform support.",
        "affineReconstructionNotImplementedBranchCount": len(re.findall(r'CV_Error\(Error::StsNotImplemented, "Affine reconstruction not yet implemented"\)', (module / "src/reconstruct.cpp").read_text(encoding="utf-8"))),
        "upstreamRuntimeLimitations": ["The upstream computeOrientation implementation is unfinished.", "The four reconstruct overloads reject their affine reconstruction branch with StsNotImplemented."],
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    with output.open("w", encoding="utf-8", newline="\n") as stream:
        stream.write(json.dumps(result, ensure_ascii=True, indent=2) + "\n")
    print(f"SFM_UPSTREAM_EXTRACTION_OK declarations={len(declarations)} callable={sum(row['kind'] == 'callable' for row in declarations)} headers=10 base={result['baseDeclarationCount']} ceres={result['ceresDeclarationCount']} ceresCallable={result['ceresCallableCount']}")


if __name__ == "__main__":
    main()
