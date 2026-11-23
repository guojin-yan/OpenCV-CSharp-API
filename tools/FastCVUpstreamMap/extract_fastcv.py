#!/usr/bin/env python3
"""Extract the parser-backed OpenCV 5.0.0 contrib FastCV public header closure."""

import argparse
import hashlib
import importlib.util
import json
import re
from collections import Counter
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


def parser_key(value):
    return (value[0], len(value[3]))


def convert(value, ordinal, source_header, wrapper_keys):
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
    elif kind == "callable":
        arguments = [
            {"type": item[0], "name": item[1], "default": item[2], "modifiers": list(item[3])}
            for item in value[3]
        ]
    return_type = value[4] or value[1] or ""
    declaration_identity = identity(kind, name, return_type, arguments, enum_values)
    return {
        "ordinal": ordinal,
        "sourceHeader": source_header,
        "kind": kind,
        "name": name,
        "identity": declaration_identity,
        "returnType": return_type,
        "modifiers": list(value[2]),
        "arguments": arguments,
        "enumValues": enum_values,
        "baseDeclaration": value[1] if kind == "class" else "",
        # hdr_parser's wmode=True pass normalizes OpenCV InputArray/OutputArray,
        # vector, and return types. Fully-qualified declaration name plus arity
        # is unambiguous for this pinned closure and binds those annotations.
        "upstreamWrapperAnnotated": parser_key(value) in wrapper_keys,
        "documentation": value[5] or "",
    }


def make_parser(parser_module, definitions):
    return parser_module.CppHeaderParser(
        preprocessor_definitions=dict(definitions),
    )


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
    parser_path = root / "modules/python/src2/hdr_parser.py"
    module = root.parent.parent / f"opencv-source/opencv_contrib-{args.upstream_version}/modules/fastcv"
    include = module / "include/opencv2"
    umbrella = include / "fastcv.hpp"
    cmake = module / "CMakeLists.txt"
    readme = module / "README.md"

    include_names = re.findall(
        r'^\s*#include\s+"opencv2/fastcv/([^"\r\n]+\.hpp)"',
        umbrella.read_text(encoding="utf-8"),
        flags=re.MULTILINE,
    )
    files = sorted((include / "fastcv").glob("*.hpp"))
    file_names = [file.name for file in files]
    if len(include_names) != 30 or sorted(include_names) != file_names:
        raise RuntimeError(
            "fastcv umbrella/header closure drifted: "
            f"includes={len(include_names)} files={len(file_names)}"
        )

    cmake_text = cmake.read_text(encoding="utf-8")
    if not re.search(r"(?s)if\(HAVE_FASTCV\).*?ocv_define_module\(fastcv\s+opencv_core\s+opencv_geometry\s+opencv_imgproc\s+opencv_features\s+opencv_video\s+WRAP\s+python\s+java\).*?\$\{FASTCV_LIBRARY\}.*?else\s*\(\)\s*ocv_module_disable\(fastcv\)", cmake_text):
        raise RuntimeError("FastCV CMake build/dependency contract drifted")

    spec = importlib.util.spec_from_file_location("opencv_hdr_parser", parser_path)
    hdr_module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(hdr_module)
    definitions = {
        "CV_VERSION_MAJOR": 5,
        "OPENCV_ABI_COMPATIBILITY": 500,
        "HAVE_OPENCV_FLANN": 1,
        "HAVE_OPENCV_DNN": 1,
    }

    wrapper_parser = make_parser(hdr_module, definitions)
    wrapper_values = [
        value
        for file in files
        for value in wrapper_parser.parse(str(file), wmode=True)
    ]
    wrapper_keys = {parser_key(value) for value in wrapper_values}
    if len(wrapper_keys) != len(wrapper_values):
        raise RuntimeError("FastCV hdr_parser wmode=True emitted duplicate name/arity keys")

    public_parser = make_parser(hdr_module, definitions)
    if public_parser.parse(str(umbrella), wmode=False):
        raise RuntimeError("FastCV umbrella unexpectedly contains parser declarations")
    source_headers = []
    parsed_rows = []
    parser_ordinal = 0
    for file in files:
        parsed = public_parser.parse(str(file), wmode=False)
        source = rel(file, workspace)
        source_headers.append(
            {
                "path": source,
                "sha256": sha256(file),
                "startOrdinal": parser_ordinal,
                "declarationCount": len(parsed),
            }
        )
        parsed_rows.extend((file, value) for value in parsed)
        parser_ordinal += len(parsed)

    public_key_counts = Counter(parser_key(value) for _, value in parsed_rows)
    if any(public_key_counts[key] != 1 for key in wrapper_keys):
        raise RuntimeError("FastCV wrapper annotation name/arity join is absent or ambiguous")

    declarations = [
        convert(value, ordinal, rel(file, workspace), wrapper_keys)
        for ordinal, (file, value) in enumerate(parsed_rows)
    ]
    counts = Counter(item["identity"] for item in declarations)
    duplicates = sorted(item for item, count in counts.items() if count > 1)
    # Preserve only the first parser occurrence for exact duplicate identities.
    first_by_identity = {}
    for item in declarations:
        first_by_identity.setdefault(item["identity"], item)
    declarations = list(first_by_identity.values())
    for ordinal, item in enumerate(declarations):
        item["ordinal"] = ordinal

    include_paths = [rel(include / "fastcv" / name, workspace) for name in include_names]
    result = {
        "schemaVersion": 1,
        "generator": "tools/FastCVUpstreamMap/extract_fastcv.py",
        "upstreamOpenCvVersion": args.upstream_version,
        "headerPath": rel(umbrella, workspace),
        "headerSha256": sha256(umbrella),
        "parserPath": rel(parser_path, workspace),
        "parserSha256": sha256(parser_path),
        "preprocessorDefinitions": definitions,
        "parserMode": "non-wrapper-public-declarations",
        "parserNormalization": "The broad public declaration pass is deduplicated by full exact signature; a separate hdr_parser wmode=True pass marks upstream CV_EXPORTS_W/CV_WRAP declarations by fully qualified name plus arity because the wrapper parser normalizes InputArray/OutputArray and vector types. This is upstream Python/Java wrapper annotation evidence, not a claim of a C# wrapper or bundled FastCV runtime.",
        "parserDuplicateIdentities": duplicates,
        "parserDeclarationCountBeforeExactDeduplication": len(parsed_rows),
        "upstreamWrapperAnnotatedDeclarationCount": sum(1 for item in declarations if item["upstreamWrapperAnnotated"]),
        "upstreamWrapperAnnotatedCallableCount": sum(1 for item in declarations if item["kind"] == "callable" and item["upstreamWrapperAnnotated"]),
        "sourceHeaders": source_headers,
        "umbrellaIncludes": include_paths,
        "declarationCount": len(declarations),
        "declarations": declarations,
        "compatibilityHeaders": [
            {
                "path": rel(cmake, workspace),
                "sha256": sha256(cmake),
                "reason": "Upstream fastcv is disabled unless HAVE_FASTCV is enabled; the CMake target requires an external FastCV library and OpenCV core/geometry/imgproc/features/video modules.",
            },
            {
                "path": rel(readme, workspace),
                "sha256": sha256(readme),
                "reason": "The pinned upstream module documents ARM-only support and says the module cannot configure or build on x86; no Qualcomm FastCV runtime is bundled by this package.",
            },
        ],
        "moduleDependencies": ["opencv_core", "opencv_geometry", "opencv_imgproc", "opencv_features", "opencv_video"],
        "externalLibrary": "FASTCV_LIBRARY",
        "moduleBuildCondition": "HAVE_FASTCV_AND_SUPPORTED_ARM_ARCHITECTURE_AND_EXTERNAL_FASTCV_LIBRARY",
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    with output.open("w", encoding="utf-8", newline="\n") as stream:
        stream.write(json.dumps(result, ensure_ascii=True, indent=2) + "\n")
    print(
        "FASTCV_UPSTREAM_EXTRACTION_OK "
        f"declarations={len(declarations)} headers={len(files)} "
        f"callable={sum(1 for item in declarations if item['kind'] == 'callable')} "
        f"wrapperAnnotated={result['upstreamWrapperAnnotatedDeclarationCount']} "
        f"duplicates={len(duplicates)}"
    )


if __name__ == "__main__":
    main()
