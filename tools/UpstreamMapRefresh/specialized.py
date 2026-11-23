"""Run the reviewed specialized strategies for versioned current source inputs."""

from contextlib import redirect_stdout
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import sys
import sysconfig


EXTRACTORS = {
    "cannops": "CannOps",
    "cudev": "Cudev",
    "fastcv": "FastCV",
    "matlab": "Matlab",
    "sfm": "Sfm",
    "viz": "Viz",
}
CORE_FIELDS = {
    "schemaVersion", "generator", "upstreamOpenCvVersion", "module", "headerPath",
    "headerSha256", "parserPath", "parserSha256", "preprocessorDefinitions",
    "sourceHeaders", "declarationCount", "declarations", "parserMode",
    "parserNormalization", "compatibilityHeaders", "moduleBuildCondition",
}


def syntax_target(repo):
    requirements = repo / "tools/CudevUpstreamMap/syntax-requirements.txt"
    digest = hashlib.sha256(requirements.read_bytes()).hexdigest()[:16]
    tag = sys.implementation.cache_tag + "-" + sysconfig.get_platform()
    return repo / "artifacts/tools/cudev-syntax" / (tag + "-" + digest)


def load(path):
    # These names also satisfy the specialized tools' reviewed local imports.
    spec = importlib.util.spec_from_file_location(path.stem, path)
    module = importlib.util.module_from_spec(spec)
    sys.modules[path.stem] = module
    spec.loader.exec_module(module)
    return module


def extract(repo, workspace, module, destination, sha, syntax_root=None):
    mid = module["id"]
    directory = repo / ("tools/" + EXTRACTORS[mid] + "UpstreamMap")
    sys.path[:0] = [str(directory), str(repo / "tools/CudevUpstreamMap"),
                    str(repo / "tools/XImgProcUpstreamMap")]
    strategy = load(directory / ("extract_" + mid + ".py"))
    # Current-source hashes use official LF Git blobs, including CRLF checkouts.
    # The standalone tools retain their historical byte-hash policy by default.
    strategy.sha256 = sha
    path = destination / (mid + "-upstream-raw.json")
    path.parent.mkdir(parents=True, exist_ok=True)
    argv = sys.argv
    capture = io.StringIO()
    try:
        with redirect_stdout(capture):
            if mid == "matlab":
                strategy.extract(workspace, workspace / "opencv-source/opencv-5.1.0", path,
                                 upstream_version="5.1.0")
            else:
                sys.argv = [str(directory / ("extract_" + mid + ".py")),
                            "--workspace", str(workspace), "--opencv-root",
                            str(workspace / "opencv-source/opencv-5.1.0"),
                            "--output", str(path), "--upstream-version", "5.1.0"]
                strategy.main()
            if mid in {"cudev", "matlab"}:
                target = Path(syntax_root) if syntax_root else syntax_target(repo)
                if not target.is_dir():
                    raise ValueError("Initialize the pinned syntax parser with scripts/Initialize-CudevSyntaxParser.ps1")
                name = "extract_source_callables.py" if mid == "cudev" else "extract_matlab_callables.py"
                syntax = load(directory / name)
                syntax.sha256 = sha
                output = destination / (mid + "-source-callables.json")
                if mid == "matlab":
                    syntax.extract(workspace, output, target, upstream_version="5.1.0")
                else:
                    sys.argv = [str(directory / name), "--workspace", str(workspace),
                                "--output", str(output), "--syntax-parser-root", str(target),
                                "--upstream-version", "5.1.0"]
                    syntax.main()
    finally:
        sys.argv = argv
    raw = json.loads(path.read_text(encoding="utf-8"))
    if raw["upstreamOpenCvVersion"] != "5.1.0":
        raise ValueError("Specialized extraction used another upstream revision")
    # Partition the additional evidence without losing normalization/probe data.
    metadata = {key: value for key, value in raw.items() if key not in CORE_FIELDS}
    # Viz's historical extractor reports header partitions before removing two
    # duplicate inline constructors. Retain those counts as parser evidence and
    # bind each surviving declaration to its actual contiguous source partition.
    next_ordinal = 0
    partitions = []
    for header in raw["sourceHeaders"]:
        count = sum(row["sourceHeader"] == header["path"] for row in raw["declarations"])
        if header["startOrdinal"] != next_ordinal or header["declarationCount"] != count:
            partitions.append(dict(header))
            header["parserDeclarationCount"] = header["declarationCount"]
        header["startOrdinal"] = next_ordinal
        header["declarationCount"] = count
        next_ordinal += count
    if partitions:
        metadata["parserHeaderPartitionsBeforeExactDeduplication"] = partitions
    raw = {key: value for key, value in raw.items() if key in CORE_FIELDS}
    raw["parserEvidence"] = metadata
    if mid in {"cudev", "matlab"}:
        raw["parserEvidence"]["sourceCallableInventory"] = json.loads(
            (destination / (mid + "-source-reviewed-extensions.json")).read_text(encoding="utf-8")
        )["sourceCallableInventory"]
    return raw
