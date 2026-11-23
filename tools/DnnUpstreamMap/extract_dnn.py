#!/usr/bin/env python3
"""Extract the OpenCV 5.0.0 DNN public compatibility include closure."""

import argparse
import hashlib
import importlib.util
import json
import re
from pathlib import Path


def sha256(path, canonical=False):
    data = path.read_bytes()
    if canonical:
        data = data.replace(b"\r\n", b"\n").replace(b"\r", b"\n")
    return hashlib.sha256(data).hexdigest()


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
    parser.add_argument("--upstream-version", choices=("5.0.0", "5.1.0"), default="5.0.0")
    args = parser.parse_args()

    workspace = Path(args.workspace).resolve()
    root = Path(args.opencv_root).resolve()
    output = Path(args.output).resolve()
    version_path = root / "modules/core/include/opencv2/core/version.hpp"
    # Historical sparse source sets predate version.hpp provenance.
    if args.upstream_version == "5.1.0" or version_path.is_file():
        version_header = version_path.read_text(encoding="utf-8")
        version = ".".join(re.search(r"#define\s+CV_VERSION_" + part + r"\s+(\d+)", version_header).group(1)
                           for part in ("MAJOR", "MINOR", "REVISION"))
        if version != args.upstream_version:
            raise ValueError(f"Expected OpenCV {args.upstream_version}, found {version}")
    def source_sha(path):
        return sha256(path, canonical=args.upstream_version == "5.1.0")
    parser_path = root / "modules/python/src2/hdr_parser.py"
    umbrella = root / "modules/dnn/include/opencv2/dnn.hpp"
    relative_headers = [
        "modules/dnn/include/opencv2/dnn/dict.hpp",
        "modules/dnn/include/opencv2/dnn/dnn.hpp",
        "modules/dnn/include/opencv2/dnn/utils/inference_engine.hpp",
    ]

    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    definitions = {"CV_VERSION_MAJOR": 5, "OPENCV_ABI_COMPATIBILITY": 500}

    declarations = []
    source_headers = []
    for relative in relative_headers:
        path = root / relative
        parsed = module.CppHeaderParser(
            preprocessor_definitions=dict(definitions)
        ).parse(str(path))
        start = len(declarations)
        declarations.extend(
            convert(value, len(declarations), rel(path, workspace)) for value in parsed
        )
        source_headers.append(
            {
                "path": rel(path, workspace),
                "sha256": source_sha(path),
                "startOrdinal": start,
                "declarationCount": len(parsed),
            }
        )

    identities = [item["identity"] for item in declarations]
    if len(identities) != len(set(identities)):
        duplicates = sorted(
            identity for identity in set(identities) if identities.count(identity) > 1
        )
        raise RuntimeError(
            "DNN parser closure contains duplicate identities: " + ", ".join(duplicates)
        )

    result = {
        "schemaVersion": 1,
        "generator": "tools/DnnUpstreamMap/extract_dnn.py",
        "upstreamOpenCvVersion": args.upstream_version,
        "headerPath": rel(umbrella, workspace),
        "headerSha256": source_sha(umbrella),
        "parserPath": rel(parser_path, workspace),
        "parserSha256": source_sha(parser_path),
        "preprocessorDefinitions": definitions,
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
        "DNN_UPSTREAM_EXTRACTION_OK declarations={} enums={} classes={} callables={} headers={}".format(
            len(declarations),
            sum(item["kind"] == "enum" for item in declarations),
            sum(item["kind"] == "class" for item in declarations),
            sum(item["kind"] == "callable" for item in declarations),
            len(source_headers),
        )
    )


if __name__ == "__main__":
    main()
