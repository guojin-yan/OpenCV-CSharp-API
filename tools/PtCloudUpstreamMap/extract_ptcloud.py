#!/usr/bin/env python3
"""Extract the parser-backed OpenCV ptcloud depth header closure."""

import hashlib
import importlib.util
import json
from pathlib import Path


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def rel(path: Path, workspace: Path) -> str:
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
        arguments = [
            {"type": item[0], "name": item[1], "default": item[2], "modifiers": list(item[3])}
            for item in value[3]
        ]
    return_type = value[4] or value[1] or ""
    return {
        "ordinal": ordinal,
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
    import argparse

    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True)
    parser.add_argument("--opencv-root", required=True)
    parser.add_argument("--output", required=True)
    args = parser.parse_args()
    workspace = Path(args.workspace).resolve()
    root = Path(args.opencv_root).resolve()
    output = Path(args.output).resolve()
    parser_path = root / "modules/python/src2/hdr_parser.py"
    header = root / "modules/ptcloud/include/opencv2/ptcloud/depth.hpp"
    umbrella = root / "modules/ptcloud/include/opencv2/ptcloud.hpp"
    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    hdr_module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(hdr_module)
    definitions = {
        "CV_VERSION_MAJOR": 5,
        "OPENCV_ABI_COMPATIBILITY": 500,
        "HAVE_OPENCV_FLANN": 1,
        "HAVE_OPENCV_DNN": 1,
    }
    parser_instance = hdr_module.CppHeaderParser(preprocessor_definitions=dict(definitions))
    parsed = parser_instance.parse(str(header))
    source_header = rel(header, workspace)
    declarations = [convert(value, ordinal, source_header) for ordinal, value in enumerate(parsed)]
    identities = [item["identity"] for item in declarations]
    if len(identities) != len(set(identities)):
        duplicates = sorted(item for item in set(identities) if identities.count(item) > 1)
        raise RuntimeError("ptcloud parser closure contains duplicate identities: " + ", ".join(duplicates))
    result = {
        "schemaVersion": 1,
        "generator": "tools/PtCloudUpstreamMap/extract_ptcloud.py",
        "upstreamOpenCvVersion": "5.0.0",
        "headerPath": rel(umbrella, workspace),
        "headerSha256": sha256(umbrella),
        "parserPath": rel(parser_path, workspace),
        "parserSha256": sha256(parser_path),
        "preprocessorDefinitions": definitions,
        "sourceHeaders": [
            {
                "path": source_header,
                "sha256": sha256(header),
                "startOrdinal": 0,
                "declarationCount": len(declarations),
            }
        ],
        "compatibilityHeaders": [],
        "excludedPublicHeaders": [
            {
                "path": rel(umbrella, workspace),
                "sha256": sha256(umbrella),
                "reason": "Umbrella includes volume/odometry headers outside the selected depth/RGB-D wrapper slice.",
            }
        ],
        "declarationCount": len(declarations),
        "declarations": declarations,
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(result, ensure_ascii=True, indent=2) + "\n", encoding="utf-8", newline="\n")
    print(f"PTCLOUD_UPSTREAM_EXTRACTION_OK declarations={len(declarations)} headers=1")


if __name__ == "__main__":
    main()
