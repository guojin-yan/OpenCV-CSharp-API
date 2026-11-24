"""Audit the actual isolated Full/Mini ELF closure, without producer search paths."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess


def run(*args):
    return subprocess.check_output(args, text=True, env={
        k: v for k, v in os.environ.items()
        if k not in {"LD_LIBRARY_PATH", "LD_PRELOAD", "LD_AUDIT"}
    })


def audit(directory, profile, matrix):
    modules = next(p["modules"] for p in matrix["profiles"] if p["name"] == profile)
    names = {"libJYPPX.OpenCV.Native.so"} | {
        f"libopencv_{m}.so{s}" for m in modules for s in ("", ".501", ".5.1.0")
    }
    actual = {p.name for p in directory.iterdir() if p.is_file() or p.is_symlink()}
    if actual != names:
        raise ValueError(f"Runtime closure drift: missing={names-actual}, extra={actual-names}")
    allowed = {"libstdc++.so.6", "libm.so.6", "libgcc_s.so.1", "libc.so.6",
               "libpthread.so.0", "libdl.so.2", "librt.so.1", "ld-linux-x86-64.so.2"}
    opencv = {f"libopencv_{m}.so.501" for m in modules}
    files = []
    for name in sorted({"libJYPPX.OpenCV.Native.so"} | {f"libopencv_{m}.so.5.1.0" for m in modules}):
        path = directory / name
        header = run("readelf", "-h", str(path))
        dynamic = run("readelf", "-d", str(path))
        versions = run("readelf", "--version-info", str(path))
        dependencies = run("ldd", str(path))
        if "Advanced Micro Devices X86-64" not in header or "ELF64" not in header:
            raise ValueError(f"Wrong ELF architecture: {name}")
        rpaths = re.findall(r"\((?:RPATH|RUNPATH)\).*?\[(.*?)\]", dynamic)
        if rpaths != ["$ORIGIN"]:
            raise ValueError(f"Nonportable runtime paths: {name}: {rpaths}")
        needed = re.findall(r"\(NEEDED\).*?\[(.*?)\]", dynamic)
        if set(needed) - allowed - opencv or "not found" in dependencies:
            raise ValueError(f"Incomplete or distro-specific dependency closure: {name}")
        for dependency, location in re.findall(r"(\S+) => (\S+)", dependencies):
            resolved = Path(location).resolve()
            if dependency in opencv and resolved.parent != directory.resolve():
                raise ValueError(f"OpenCV dependency escaped staged closure: {dependency} -> {location}")
            if dependency not in opencv and not str(resolved).startswith(("/usr/lib/", "/lib/")):
                raise ValueError(f"Non-system external dependency: {dependency} -> {location}")
        floors = {}
        for family, maximum in (("GLIBC", (2, 35)), ("GLIBCXX", (3, 4, 30)), ("CXXABI", (1, 3, 13))):
            values = [tuple(map(int, v.split("."))) for v in re.findall(rf"\b{family}_(\d+(?:\.\d+)+)\b", versions)]
            floor = max(values, default=(0,))
            if floor > maximum:
                raise ValueError(f"{name} exceeds {family} ceiling: {floor}")
            floors[family] = ".".join(map(str, floor))
        sonames = re.findall(r"\(SONAME\).*?\[(.*?)\]", dynamic)
        expected_soname = name if name == "libJYPPX.OpenCV.Native.so" else name.replace(".5.1.0", ".501")
        if sonames != [expected_soname]:
            raise ValueError(f"Unexpected ABI SONAME: {name}: {sonames}")
        if name == "libJYPPX.OpenCV.Native.so" and set(needed) & opencv != opencv:
            raise ValueError("Wrapper does not directly link every required module")
        files.append({"name": name, "bytes": path.stat().st_size,
                      "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
                      "soname": sonames[0], "runtimePaths": rpaths,
                      "needed": needed, "symbolFloors": floors,
                      "lddOutput": dependencies})
    symbols = run("nm", "-D", "--defined-only", str(directory / "libJYPPX.OpenCV.Native.so"))
    exports = sorted(re.findall(r"\bT (jyppx_ocv_\w+)$", symbols, re.M))
    if len(exports) != {"full": 2669, "mini": 527}[profile] or len(set(exports)) != len(exports):
        raise ValueError("Native ABI export count drift")
    return {"profile": profile, "runtimeFileCount": len(names), "exportCount": len(exports),
            "exportNamesSha256": hashlib.sha256(("\n".join(exports) + "\n").encode()).hexdigest(), "files": files}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--directory", type=Path, required=True)
    parser.add_argument("--profile", choices=("full", "mini"), required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    root = Path(__file__).resolve().parent.parent
    matrix = json.loads((root / "packaging/runtime/runtime-generic-linux-preview-matrix.json").read_text())
    result = audit(args.directory, args.profile, matrix)
    args.output.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(f"LOCAL_LINUX_51_ELF_OK profile={args.profile} files={len(result['files'])} exports={result['exportCount']}")
