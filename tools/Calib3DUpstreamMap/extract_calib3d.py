#!/usr/bin/env python3
"""Extract the OpenCV 5 Calib3D compatibility include closure with hdr_parser.py."""

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


def extract_declaration(value, ordinal, source_header):
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
    return {
        "ordinal": ordinal,
        "sourceHeader": source_header,
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


def main():
    argument_parser = argparse.ArgumentParser()
    argument_parser.add_argument("--workspace", required=True)
    argument_parser.add_argument("--opencv-root", required=True)
    argument_parser.add_argument("--output", required=True)
    args = argument_parser.parse_args()

    workspace = Path(args.workspace).resolve()
    opencv_root = Path(args.opencv_root).resolve()
    output = Path(args.output).resolve()
    umbrella = opencv_root / "modules/calib/include/opencv2/calib3d.hpp"
    parser_path = opencv_root / "modules/python/src2/hdr_parser.py"
    source_paths = [
        opencv_root / "modules/geometry/include/opencv2/geometry/2d.hpp",
        opencv_root / "modules/geometry/include/opencv2/geometry/3d.hpp",
        opencv_root / "modules/stereo/include/opencv2/stereo.hpp",
        opencv_root / "modules/calib/include/opencv2/calib.hpp",
    ]

    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    preprocessor_definitions = {"CV_VERSION_MAJOR": 5}

    declarations = []
    source_headers = []
    for source_path in source_paths:
        relative_path = normalize_path(source_path.relative_to(workspace))
        parsed = module.CppHeaderParser(
            preprocessor_definitions=dict(preprocessor_definitions)
        ).parse(str(source_path))
        start_ordinal = len(declarations)
        for value in parsed:
            declarations.append(
                extract_declaration(value, len(declarations), relative_path)
            )
        source_headers.append(
            {
                "path": relative_path,
                "sha256": sha256(source_path),
                "startOrdinal": start_ordinal,
                "declarationCount": len(parsed),
            }
        )

    identities = [item["identity"] for item in declarations]
    if len(set(identities)) != len(identities):
        duplicates = sorted({value for value in identities if identities.count(value) > 1})
        raise RuntimeError("Parser closure contains duplicate identities: " + ", ".join(duplicates))

    result = {
        "schemaVersion": 1,
        "generator": "tools/Calib3DUpstreamMap/extract_calib3d.py",
        "upstreamOpenCvVersion": "5.0.0",
        "headerPath": normalize_path(umbrella.relative_to(workspace)),
        "headerSha256": sha256(umbrella),
        "parserPath": normalize_path(parser_path.relative_to(workspace)),
        "parserSha256": sha256(parser_path),
        "preprocessorDefinitions": preprocessor_definitions,
        "sourceHeaders": source_headers,
        "declarationCount": len(declarations),
        "declarations": declarations,
    }
    text = json.dumps(result, ensure_ascii=True, indent=2) + "\n"
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(text, encoding="utf-8", newline="\n")
    print(
        "CALIB3D_UPSTREAM_EXTRACTION_OK declarations={} enums={} classes={} callables={} headers={}".format(
            len(declarations),
            sum(item["kind"] == "enum" for item in declarations),
            sum(item["kind"] == "class" for item in declarations),
            sum(item["kind"] == "callable" for item in declarations),
            len(source_headers),
        )
    )


if __name__ == "__main__":
    main()
