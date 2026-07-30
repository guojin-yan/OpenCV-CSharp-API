#!/usr/bin/env python3
"""Extract the OpenCV 5.0.0 Core public compatibility include closure."""

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
        return f"enum {name}[" + ";".join(f"{item['name']}={item['value']}" for item in enum_values) + "]"
    if kind != "callable":
        return f"{kind} {name}"
    args = []
    for arg in arguments:
        value = f"{arg['type']} {arg['name']}"
        if arg["default"]:
            value += f"={arg['default']}"
        if arg["modifiers"]:
            value += "[" + ",".join(arg["modifiers"]) + "]"
        args.append(value)
    return f"{name}({';'.join(args)})->{return_type}"


def convert(value, ordinal, source):
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
        "sourceHeader": source,
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
    ap = argparse.ArgumentParser()
    ap.add_argument("--workspace", required=True)
    ap.add_argument("--opencv-root", required=True)
    ap.add_argument("--output", required=True)
    args = ap.parse_args()
    workspace = Path(args.workspace).resolve()
    root = Path(args.opencv_root).resolve()
    output = Path(args.output).resolve()
    parser_path = root / "modules/python/src2/hdr_parser.py"
    umbrella = root / "modules/core/include/opencv2/core.hpp"
    relative_headers = [
        "modules/core/include/opencv2/core/base.hpp",
        "modules/core/include/opencv2/core/cvstd.hpp",
        "modules/core/include/opencv2/core/traits.hpp",
        "modules/core/include/opencv2/core/matx.hpp",
        "modules/core/include/opencv2/core/types.hpp",
        "modules/core/include/opencv2/core/mat.hpp",
        "modules/core/include/opencv2/core/persistence.hpp",
        "modules/core/include/opencv2/core.hpp",
        "modules/core/include/opencv2/core/operations.hpp",
        "modules/core/include/opencv2/core/utility.hpp",
        "modules/core/include/opencv2/core/optim.hpp",
    ]
    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    definitions = {"CV_VERSION_MAJOR": 5, "OPENCV_ABI_COMPATIBILITY": 500}
    declarations = []
    source_headers = []
    for relative in relative_headers:
        path = root / relative
        parsed = module.CppHeaderParser(preprocessor_definitions=dict(definitions)).parse(str(path))
        start = len(declarations)
        declarations.extend(convert(value, len(declarations), rel(path, workspace)) for value in parsed)
        source_headers.append({
            "path": rel(path, workspace),
            "sha256": sha256(path),
            "startOrdinal": start,
            "declarationCount": len(parsed),
        })
    ids = [item["identity"] for item in declarations]
    if len(ids) != len(set(ids)):
        raise RuntimeError("Core parser closure contains duplicate identities")
    result = {
        "schemaVersion": 1,
        "generator": "tools/CoreUpstreamMap/extract_core.py",
        "upstreamOpenCvVersion": "5.0.0",
        "headerPath": rel(umbrella, workspace),
        "headerSha256": sha256(umbrella),
        "parserPath": rel(parser_path, workspace),
        "parserSha256": sha256(parser_path),
        "preprocessorDefinitions": definitions,
        "sourceHeaders": source_headers,
        "declarationCount": len(declarations),
        "declarations": declarations,
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(result, ensure_ascii=True, indent=2) + "\n", encoding="utf-8", newline="\n")
    callables = sum(item["kind"] == "callable" for item in declarations)
    print(f"CORE_UPSTREAM_EXTRACTION_OK declarations={len(declarations)} callables={callables} headers={len(source_headers)}")


if __name__ == "__main__":
    main()
