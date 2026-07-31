#!/usr/bin/env python3
"""Extract the OpenCV 5.0.0 installed public Stitching header closure."""

import argparse
import hashlib
import importlib.util
import json
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


def convert(value, ordinal, surface, source_header):
    raw_name = value[0]
    if raw_name.startswith("enum "):
        kind, name = "enum", raw_name[5:]
    elif raw_name.startswith("class ") or raw_name.startswith("struct "):
        prefix = 6 if raw_name.startswith("class ") else 7
        kind, name = "class", raw_name[prefix:]
    else:
        kind, name = "callable", raw_name
    arguments = []
    enum_values = []
    if kind == "enum":
        enum_values = [{"name": item[0], "value": item[1]} for item in value[3]]
    else:
        arguments = [
            {"type": item[0], "name": item[1], "default": item[2], "modifiers": list(item[3])}
            for item in value[3]
        ]
    return_type = value[4] or value[1] or ""
    return {
        "ordinal": ordinal,
        "surface": surface,
        "sourceHeader": source_header,
        "kind": kind,
        "name": name,
        "identity": identity(kind, name, return_type, arguments, enum_values),
        "returnType": return_type,
        "modifiers": list(value[2]),
        "arguments": arguments,
        "enumValues": enum_values,
        "baseDeclaration": value[1] if kind == "class" else "",
        "documentation": value[5] or "",
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True)
    parser.add_argument("--opencv-root", required=True)
    parser.add_argument("--output", required=True)
    args = parser.parse_args()

    workspace = Path(args.workspace).resolve()
    opencv = Path(args.opencv_root).resolve()
    output = Path(args.output).resolve()
    parser_path = opencv / "modules/python/src2/hdr_parser.py"
    include = opencv / "modules/stitching/include/opencv2"
    headers = [
        ("primary", include / "stitching.hpp", "opencv2/core.hpp;opencv2/features2d.hpp;opencv2/imgproc.hpp;opencv2/stitching/warpers.hpp"),
        ("public-warpers", include / "stitching/warpers.hpp", "opencv2/core.hpp;opencv2/imgproc.hpp;opencv2/stitching/detail/warpers.hpp"),
        ("detail-autocalib", include / "stitching/detail/autocalib.hpp", "opencv2/core.hpp;opencv2/stitching/detail/camera.hpp"),
        ("detail-blenders", include / "stitching/detail/blenders.hpp", "opencv2/core.hpp"),
        ("detail-camera", include / "stitching/detail/camera.hpp", "opencv2/core.hpp"),
        ("detail-exposure", include / "stitching/detail/exposure_compensate.hpp", "opencv2/core.hpp"),
        ("detail-matchers", include / "stitching/detail/matchers.hpp", "opencv2/core.hpp;opencv2/features2d.hpp;opencv2/stitching/detail/camera.hpp"),
        ("detail-motion-estimators", include / "stitching/detail/motion_estimators.hpp", "opencv2/core.hpp;opencv2/stitching/detail/camera.hpp;opencv2/stitching/detail/matchers.hpp"),
        ("detail-seam-finders", include / "stitching/detail/seam_finders.hpp", "opencv2/core.hpp"),
        ("detail-timelapsers", include / "stitching/detail/timelapsers.hpp", "opencv2/core.hpp"),
        ("detail-util", include / "stitching/detail/util.hpp", "opencv2/core.hpp"),
        ("detail-util-inl", include / "stitching/detail/util_inl.hpp", "opencv2/stitching/detail/util.hpp"),
        ("detail-warpers", include / "stitching/detail/warpers.hpp", "opencv2/core.hpp;opencv2/imgproc.hpp"),
        ("detail-warpers-inl", include / "stitching/detail/warpers_inl.hpp", "opencv2/stitching/detail/warpers.hpp"),
    ]

    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    definitions = {"CV_VERSION_MAJOR": 5, "OPENCV_ABI_COMPATIBILITY": 500}
    declarations = []
    source_headers = []
    compatibility_headers = []
    surface_counts = {}
    for surface, header, includes in headers:
        parser_instance = module.CppHeaderParser(preprocessor_definitions=dict(definitions))
        parsed = parser_instance.parse(str(header))
        source = rel(header, workspace)
        start = len(declarations)
        declarations.extend(convert(value, start + index, surface, source) for index, value in enumerate(parsed))
        source_headers.append({
            "surface": surface,
            "path": source,
            "sha256": sha256(header),
            "startOrdinal": start,
            "declarationCount": len(parsed),
        })
        compatibility_headers.append({"path": source, "sha256": sha256(header), "includes": includes})
        surface_counts[surface] = len(parsed)

    identities = [item["identity"] for item in declarations]
    if len(identities) != len(set(identities)):
        duplicates = sorted(item for item in set(identities) if identities.count(item) > 1)
        raise RuntimeError("Stitching parser surfaces contain duplicate identities: " + ", ".join(duplicates))

    result = {
        "schemaVersion": 1,
        "generator": "tools/StitchingUpstreamMap/extract_stitching.py",
        "upstreamOpenCvVersion": "5.0.0",
        "headerPath": rel(headers[0][1], workspace),
        "headerSha256": sha256(headers[0][1]),
        "parserPath": rel(parser_path, workspace),
        "parserSha256": sha256(parser_path),
        "preprocessorDefinitions": definitions,
        "compatibilityHeaders": compatibility_headers,
        "excludedPublicHeaders": [],
        "sourceHeaders": source_headers,
        "declarationCount": len(declarations),
        "surfaceCounts": surface_counts,
        "declarations": declarations,
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(result, ensure_ascii=True, indent=2) + "\n", encoding="utf-8", newline="\n")
    print("STITCHING_UPSTREAM_EXTRACTION_OK declarations={} enums={} classes={} callables={} headers={}".format(
        len(declarations),
        sum(item["kind"] == "enum" for item in declarations),
        sum(item["kind"] == "class" for item in declarations),
        sum(item["kind"] == "callable" for item in declarations),
        len(headers),
    ))


if __name__ == "__main__":
    main()
