#!/usr/bin/env python3
"""Extract the OpenCV 5.0.0 contrib Tracking primary and public legacy surfaces."""

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
            {
                "type": item[0],
                "name": item[1],
                "default": item[2],
                "modifiers": list(item[3]),
            }
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
    parser.add_argument("--opencv-contrib-root", required=True)
    parser.add_argument("--output", required=True)
    args = parser.parse_args()

    workspace = Path(args.workspace).resolve()
    main_root = Path(args.opencv_root).resolve()
    contrib_root = Path(args.opencv_contrib_root).resolve()
    output = Path(args.output).resolve()
    parser_path = main_root / "modules/python/src2/hdr_parser.py"
    primary_header = contrib_root / "modules/tracking/include/opencv2/tracking.hpp"
    legacy_header = contrib_root / "modules/tracking/include/opencv2/tracking/tracking_legacy.hpp"
    public_dir = contrib_root / "modules/tracking/include/opencv2/tracking"

    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    definitions = {
        "CV_VERSION_MAJOR": 5,
        "OPENCV_ABI_COMPATIBILITY": 500,
    }

    parser_instance = module.CppHeaderParser(preprocessor_definitions=dict(definitions))
    primary_parsed = parser_instance.parse(str(primary_header))
    parser_instance = module.CppHeaderParser(preprocessor_definitions=dict(definitions))
    legacy_parsed = parser_instance.parse(str(legacy_header))

    primary_source = rel(primary_header, workspace)
    legacy_source = rel(legacy_header, workspace)
    declarations = [
        convert(value, ordinal, "primary", primary_source)
        for ordinal, value in enumerate(primary_parsed)
    ]
    declarations.extend(
        convert(value, len(primary_parsed) + ordinal, "legacy", legacy_source)
        for ordinal, value in enumerate(legacy_parsed)
    )

    identities = [item["identity"] for item in declarations]
    if len(identities) != len(set(identities)):
        duplicates = sorted(item for item in set(identities) if identities.count(item) > 1)
        raise RuntimeError(
            "Tracking parser surfaces contain duplicate identities: " + ", ".join(duplicates)
        )

    excluded = []
    for header in sorted(public_dir.glob("*.hpp")):
        if header == legacy_header:
            continue
        excluded.append(
            {
                "path": rel(header, workspace),
                "reason": "Publicly installed detail, experimental, dataset, callback, or implementation-oriented header; reviewed separately from the primary and legacy tracker contracts.",
            }
        )

    result = {
        "schemaVersion": 1,
        "generator": "tools/TrackingUpstreamMap/extract_tracking.py",
        "upstreamOpenCvVersion": "5.0.0",
        "headerPath": primary_source,
        "headerSha256": sha256(primary_header),
        "parserPath": rel(parser_path, workspace),
        "parserSha256": sha256(parser_path),
        "preprocessorDefinitions": definitions,
        "compatibilityHeaders": [
            {
                "path": primary_source,
                "sha256": sha256(primary_header),
                "includes": "opencv2/core.hpp;opencv2/video/tracking.hpp",
            },
            {
                "path": legacy_source,
                "sha256": sha256(legacy_header),
                "includes": "opencv2/tracking.hpp;opencv2/tracking/tracking_internals.hpp",
            },
        ],
        "sourceHeaders": [
            {
                "surface": "primary",
                "path": primary_source,
                "sha256": sha256(primary_header),
                "startOrdinal": 0,
                "declarationCount": len(primary_parsed),
            },
            {
                "surface": "legacy",
                "path": legacy_source,
                "sha256": sha256(legacy_header),
                "startOrdinal": len(primary_parsed),
                "declarationCount": len(legacy_parsed),
            },
        ],
        "excludedPublicHeaders": excluded,
        "declarationCount": len(declarations),
        "surfaceCounts": {
            "primary": len(primary_parsed),
            "legacy": len(legacy_parsed),
        },
        "declarations": declarations,
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(
        json.dumps(result, ensure_ascii=True, indent=2) + "\n",
        encoding="utf-8",
        newline="\n",
    )
    print(
        "TRACKING_UPSTREAM_EXTRACTION_OK declarations={} primary={} legacy={} enums={} classes={} callables={} headers=2".format(
            len(declarations),
            len(primary_parsed),
            len(legacy_parsed),
            sum(item["kind"] == "enum" for item in declarations),
            sum(item["kind"] == "class" for item in declarations),
            sum(item["kind"] == "callable" for item in declarations),
        )
    )


if __name__ == "__main__":
    main()
