#!/usr/bin/env python3
"""Extract OpenCV VideoIO declarations with OpenCV's own hdr_parser.py."""

import argparse
import hashlib
import importlib.util
import json
from pathlib import Path


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def normalize_path(path):
    return str(path).replace("\\", "/")


def declaration_identity(kind, name, return_type, arguments):
    if kind != "callable":
        return f"{kind} {name}"

    argument_text = []
    for argument in arguments:
        modifiers = ",".join(argument["modifiers"])
        value = f'{argument["type"]} {argument["name"]}'
        if argument["default"]:
            value += f'={argument["default"]}'
        if modifiers:
            value += f'[{modifiers}]'
        argument_text.append(value)
    return f'{name}({";".join(argument_text)})->{return_type}'


def main():
    argument_parser = argparse.ArgumentParser()
    argument_parser.add_argument("--workspace", required=True)
    argument_parser.add_argument("--opencv-root", required=True)
    argument_parser.add_argument("--output", required=True)
    args = argument_parser.parse_args()

    workspace = Path(args.workspace).resolve()
    opencv_root = Path(args.opencv_root).resolve()
    output = Path(args.output).resolve()
    header = opencv_root / "modules/videoio/include/opencv2/videoio.hpp"
    parser_path = opencv_root / "modules/python/src2/hdr_parser.py"

    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    preprocessor_definitions = {"CV_VERSION_MAJOR": 5}
    parsed = module.CppHeaderParser(
        preprocessor_definitions=dict(preprocessor_definitions)
    ).parse(str(header))

    declarations = []
    for ordinal, value in enumerate(parsed):
        raw_name = value[0]
        if raw_name.startswith("enum "):
            kind = "enum"
            name = raw_name[5:]
        elif raw_name.startswith("class ") or raw_name.startswith("struct "):
            prefix_length = 6 if raw_name.startswith("class ") else 7
            kind = "class"
            name = raw_name[prefix_length:]
        else:
            kind = "callable"
            name = raw_name

        arguments = []
        enum_values = []
        if kind == "enum":
            for enum_value in value[3]:
                enum_values.append({"name": enum_value[0], "value": enum_value[1]})
        else:
            for argument in value[3]:
                arguments.append(
                    {
                        "type": argument[0],
                        "name": argument[1],
                        "default": argument[2],
                        "modifiers": list(argument[3]),
                    }
                )

        return_type = value[4] or value[1] or ""
        identity = declaration_identity(kind, name, return_type, arguments)
        if kind == "enum":
            enum_signature = ";".join(
                f"{item['name']}={item['value']}" for item in enum_values
            )
            identity = f"enum {name}[{enum_signature}]"
        declarations.append(
            {
                "ordinal": ordinal,
                "kind": kind,
                "name": name,
                "identity": identity,
                "returnType": return_type,
                "modifiers": list(value[2]),
                "arguments": arguments,
                "enumValues": enum_values,
                "baseDeclaration": value[1] if kind == "class" else "",
                "documentation": value[5] or "",
            }
        )

    result = {
        "schemaVersion": 1,
        "generator": "tools/VideoIOUpstreamMap/extract_videoio.py",
        "upstreamOpenCvVersion": "5.0.0",
        "headerPath": normalize_path(header.relative_to(workspace)),
        "headerSha256": sha256(header),
        "parserPath": normalize_path(parser_path.relative_to(workspace)),
        "parserSha256": sha256(parser_path),
        "preprocessorDefinitions": preprocessor_definitions,
        "declarationCount": len(declarations),
        "declarations": declarations,
    }
    text = json.dumps(result, ensure_ascii=True, indent=2) + "\n"
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(text, encoding="utf-8", newline="\n")
    print(
        "VIDEOIO_UPSTREAM_EXTRACTION_OK declarations={} enums={} classes={} callables={}".format(
            len(declarations),
            sum(item["kind"] == "enum" for item in declarations),
            sum(item["kind"] == "class" for item in declarations),
            sum(item["kind"] == "callable" for item in declarations),
        )
    )


if __name__ == "__main__":
    main()
