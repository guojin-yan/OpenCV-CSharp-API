#!/usr/bin/env python3
"""Extract the exact OpenCV 5.0.0 main HighGui parser surface."""

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
    main_header = root / "modules/highgui/include/opencv2/highgui.hpp"
    compatibility_header = root / "modules/highgui/include/opencv2/highgui/highgui.hpp"
    winrt_header = root / "modules/highgui/include/opencv2/highgui/highgui_winrt.hpp"

    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    definitions = {"CV_VERSION_MAJOR": 5, "OPENCV_ABI_COMPATIBILITY": 500}
    parsed = module.CppHeaderParser(preprocessor_definitions=dict(definitions)).parse(str(main_header))
    source = rel(main_header, workspace)
    declarations = [convert(value, index, source) for index, value in enumerate(parsed)]
    identities = [item["identity"] for item in declarations]
    if len(identities) != len(set(identities)):
        raise RuntimeError("HighGui parser identities are not unique")

    result = {
        "schemaVersion": 1,
        "generator": "tools/HighGuiUpstreamMap/extract_highgui.py",
        "upstreamOpenCvVersion": "5.0.0",
        "headerPath": source,
        "headerSha256": sha256(main_header),
        "parserPath": rel(parser_path, workspace),
        "parserSha256": sha256(parser_path),
        "preprocessorDefinitions": definitions,
        "compatibilityHeaders": [
            {"path": source, "sha256": sha256(main_header), "includes": "main HighGui public declarations"},
            {"path": rel(compatibility_header, workspace), "sha256": sha256(compatibility_header), "includes": "compatibility include forwarding to opencv2/highgui.hpp"},
            {"path": rel(winrt_header, workspace), "sha256": sha256(winrt_header), "includes": "WinRT-only source-reviewed conditional declaration; official parser emits zero rows"},
        ],
        "excludedPublicHeaders": [],
        "sourceHeaders": [
            {"path": source, "sha256": sha256(main_header), "startOrdinal": 0, "declarationCount": len(declarations)}
        ],
        "declarationCount": len(declarations),
        "declarations": declarations,
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(result, ensure_ascii=True, indent=2) + "\n", encoding="utf-8", newline="\n")
    print(
        "HIGHGUI_UPSTREAM_EXTRACTION_OK declarations={} enums={} classes={} callables={} headers=1".format(
            len(declarations),
            sum(item["kind"] == "enum" for item in declarations),
            sum(item["kind"] == "class" for item in declarations),
            sum(item["kind"] == "callable" for item in declarations),
        )
    )


if __name__ == "__main__":
    main()
