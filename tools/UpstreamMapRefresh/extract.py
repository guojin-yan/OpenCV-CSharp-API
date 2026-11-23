#!/usr/bin/env python3
"""Extract explicitly reviewed 5.1 parser slices; never relabel historical maps."""

import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import re


def sha(path):
    # Match official Git blobs on both LF downloads and CRLF Windows checkouts.
    text = path.read_text(encoding="utf-8").replace("\r\n", "\n").replace("\r", "\n")
    return hashlib.sha256(text.encode("utf-8")).hexdigest()


def load(path, name):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def extract(repo, workspace, module, destination):
    root = workspace / "opencv-source/opencv-5.1.0"
    version = (root / "modules/core/include/opencv2/core/version.hpp").read_text()
    for name, value in (("MAJOR", 5), ("MINOR", 1), ("REVISION", 0)):
        if not re.search(r"^#define\s+CV_VERSION_" + name + r"\s+" + str(value) + r"\s*$", version, re.M):
            raise ValueError("Extraction requires the reviewed OpenCV 5.1.0 source")
    parser_path = root / "modules/python/src2/hdr_parser.py"
    if sha(parser_path) != "867e3de5ad45fd3996e8c9097ffc47f13c8651e20dda2318d70e273262c8dcee":
        raise ValueError("Official 5.1 parser hash drifted")
    parser = load(parser_path, "opencv_parser51")
    convert = load(repo / "tools/CoreUpstreamMap/extract_core.py", "core_dto").convert
    headers = []
    rows = []
    for item in module["sourceHeaders"]:
        path = workspace / item["path"]
        if sha(path) != item["sha256"]:
            raise ValueError("Reviewed source hash drifted: " + item["path"])
        parsed = parser.CppHeaderParser(preprocessor_definitions=dict(module["preprocessorDefinitions"])).parse(str(path))
        start = len(rows)
        for value in parsed:
            row = convert(value, len(rows), item["path"])
            # Preserve each historical extractor's enum identity convention.
            # Enum values are always retained in the DTO and reviewed in deltas.
            if row["kind"] != "callable" and module["identityPolicy"] == "named-enum":
                row["identity"] = row["kind"] + " " + row["name"]
            rows.append(row)
        headers.append(dict(item, startOrdinal=start, declarationCount=len(parsed)))
    if len({row["identity"] for row in rows}) != len(rows):
        raise ValueError("Duplicate parser identity")
    raw = {
        "schemaVersion": 1,
        "generator": "tools/UpstreamMapRefresh/extract.py",
        "upstreamOpenCvVersion": "5.1.0",
        "module": module["id"],
        "headerPath": module["primaryHeader"]["path"],
        "headerSha256": sha(workspace / module["primaryHeader"]["path"]),
        "parserPath": str(parser_path.relative_to(workspace)).replace("\\", "/"),
        "parserSha256": sha(parser_path),
        "preprocessorDefinitions": module["preprocessorDefinitions"],
        "sourceHeaders": headers,
        "declarationCount": len(rows),
        "declarations": rows,
    }
    path = destination / (module["id"] + "-upstream-raw.json")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(raw, ensure_ascii=True, indent=2) + "\n", encoding="utf-8", newline="\n")
    print(f"UPSTREAM_510_EXTRACTION_OK module={module['id']} declarations={len(rows)} headers={len(headers)}")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--repository", required=True)
    ap.add_argument("--workspace", required=True)
    ap.add_argument("--catalog", required=True)
    ap.add_argument("--output-directory", required=True)
    args = ap.parse_args()
    repo = Path(args.repository).resolve()
    catalog = json.loads(Path(args.catalog).read_text(encoding="utf-8"))
    for module in catalog["modules"]:
        extract(repo, Path(args.workspace).resolve(), module, Path(args.output_directory).resolve())


if __name__ == "__main__":
    main()
