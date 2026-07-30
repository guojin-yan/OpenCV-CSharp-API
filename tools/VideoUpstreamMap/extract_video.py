#!/usr/bin/env python3
"""Extract the OpenCV 5.0.0 main Video public include closure."""

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


def convert(value, ordinal, source_header):
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
    root = Path(args.opencv_root).resolve()
    output = Path(args.output).resolve()
    parser_path = root / "modules/python/src2/hdr_parser.py"
    umbrella = root / "modules/video/include/opencv2/video.hpp"
    nested_compatibility = root / "modules/video/include/opencv2/video/video.hpp"
    relative_headers = [
        "modules/video/include/opencv2/video/tracking.hpp",
        "modules/video/include/opencv2/video/background_segm.hpp",
    ]

    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    definitions = {
        "CV_VERSION_MAJOR": 5,
        "OPENCV_ABI_COMPATIBILITY": 500,
    }

    declarations = []
    source_headers = []
    for relative in relative_headers:
        path = root / relative
        parsed = module.CppHeaderParser(
            preprocessor_definitions=dict(definitions)
        ).parse(str(path))
        start = len(declarations)
        source = rel(path, workspace)
        declarations.extend(
            convert(value, len(declarations), source) for value in parsed
        )
        source_headers.append(
            {
                "path": source,
                "sha256": sha256(path),
                "startOrdinal": start,
                "declarationCount": len(parsed),
            }
        )

    identities = [item["identity"] for item in declarations]
    if len(identities) != len(set(identities)):
        duplicates = sorted(
            item for item in set(identities) if identities.count(item) > 1
        )
        raise RuntimeError(
            "Video parser closure contains duplicate identities: "
            + ", ".join(duplicates)
        )

    result = {
        "schemaVersion": 1,
        "generator": "tools/VideoUpstreamMap/extract_video.py",
        "upstreamOpenCvVersion": "5.0.0",
        "headerPath": rel(umbrella, workspace),
        "headerSha256": sha256(umbrella),
        "parserPath": rel(parser_path, workspace),
        "parserSha256": sha256(parser_path),
        "preprocessorDefinitions": definitions,
        "compatibilityHeaders": [
            {
                "path": rel(umbrella, workspace),
                "sha256": sha256(umbrella),
                "includes": "tracking.hpp and background_segm.hpp",
            },
            {
                "path": rel(nested_compatibility, workspace),
                "sha256": sha256(nested_compatibility),
                "includes": "compatibility header with no parser-emitted declarations",
            },
        ],
        "excludedPublicHeaders": [
            {
                "path": rel(root / "modules/video/include/opencv2/video/legacy/constants_c.h", workspace),
                "reason": "Legacy C constants are compatibility metadata, not part of the C++ parser-derived callable closure.",
            },
            {
                "path": rel(root / "modules/video/include/opencv2/video/detail/tracking.private.hpp", workspace),
                "reason": "Private implementation header is not public API.",
            },
        ],
        "sourceHeaders": source_headers,
        "declarationCount": len(declarations),
        "declarations": declarations,
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(
        json.dumps(result, ensure_ascii=True, indent=2) + "\n",
        encoding="utf-8",
        newline="\n",
    )
    print(
        "VIDEO_UPSTREAM_EXTRACTION_OK declarations={} enums={} classes={} callables={} headers={}".format(
            len(declarations),
            sum(item["kind"] == "enum" for item in declarations),
            sum(item["kind"] == "class" for item in declarations),
            sum(item["kind"] == "callable" for item in declarations),
            len(source_headers),
        )
    )


if __name__ == "__main__":
    main()
