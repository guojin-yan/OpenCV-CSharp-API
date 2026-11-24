"""Audit Fedora 44's actual ELF closure and ABI, retaining its symbol floors."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess


def run(*args):
    return subprocess.check_output(args, text=True, env={
        key: value for key, value in os.environ.items()
        if key not in {"LD_LIBRARY_PATH", "LD_PRELOAD", "LD_AUDIT"}
    })


def audit(directory, profile, root):
    matrix = json.loads((root / "packaging/runtime/runtime-lifecycle-refresh-matrix.json").read_bytes())
    modules = next(row["modules"] for row in matrix["profiles"] if row["name"] == profile)
    names = {"libJYPPX.OpenCV.Native.so"} | {
        f"libopencv_{module}.so{suffix}" for module in modules for suffix in ("", ".501", ".5.1.0")}
    if {path.name for path in directory.iterdir()} != names:
        raise ValueError("Unexpected native runtime closure")
    for module in modules:
        hashes = {hashlib.sha256((directory / f"libopencv_{module}.so{suffix}").read_bytes()).hexdigest()
                  for suffix in ("", ".501", ".5.1.0")}
        if len(hashes) != 1:
            raise ValueError("OpenCV SONAME aliases have different bytes")
    opencv = {f"libopencv_{module}.so.501" for module in modules}
    allowed = {"libstdc++.so.6", "libm.so.6", "libgcc_s.so.1", "libc.so.6", "libpthread.so.0",
               "libdl.so.2", "librt.so.1", "ld-linux-x86-64.so.2"}
    files = []
    for name in sorted({"libJYPPX.OpenCV.Native.so"} | {f"libopencv_{module}.so.5.1.0" for module in modules}):
        path = directory / name
        header = run("readelf", "-h", str(path))
        dynamic = run("readelf", "-d", str(path))
        versions = run("readelf", "--version-info", str(path))
        dependencies = run("ldd", str(path))
        if "Advanced Micro Devices X86-64" not in header or "ELF64" not in header:
            raise ValueError("Wrong ELF architecture: " + name)
        rpaths = re.findall(r"\((?:RPATH|RUNPATH)\).*?\[(.*?)\]", dynamic)
        if rpaths != ["$ORIGIN"]:
            raise ValueError("Nonportable runtime paths: " + name + ": " + str(rpaths))
        needed = re.findall(r"\(NEEDED\).*?\[(.*?)\]", dynamic)
        if set(needed) - allowed - opencv or "not found" in dependencies:
            raise ValueError("Unresolved or unexpected dependencies: " + name + ": " + dependencies)
        resolved_files = []
        for dependency, location in re.findall(r"(\S+) => (\S+)", dependencies):
            resolved = Path(location).resolve()
            if dependency in opencv:
                if resolved.parent != directory.resolve():
                    raise ValueError("OpenCV dependency escaped the candidate directory")
            elif dependency not in allowed or not str(resolved).startswith(("/usr/lib/", "/usr/lib64/", "/lib/", "/lib64/")):
                raise ValueError("Dependency did not resolve to a system runtime: " + location)
            resolved_files.append({"name": dependency, "path": str(resolved),
                                   "sha256": hashlib.sha256(resolved.read_bytes()).hexdigest()})
        sonames = re.findall(r"\(SONAME\).*?\[(.*?)\]", dynamic)
        expected_soname = name if name == "libJYPPX.OpenCV.Native.so" else name.replace(".5.1.0", ".501")
        if sonames != [expected_soname]:
            raise ValueError("Unexpected SONAME: " + name)
        if name == "libJYPPX.OpenCV.Native.so" and set(needed) & opencv != opencv:
            raise ValueError("Wrapper must directly link every profile module")
        floors = {}
        for family in ("GLIBC", "GLIBCXX", "CXXABI"):
            values = [tuple(map(int, value.split("."))) for value in
                      re.findall(rf"\b{family}_(\d+(?:\.\d+)+)\b", versions)]
            floors[family] = ".".join(map(str, max(values, default=(0,))))
        files.append({"name": name, "bytes": path.stat().st_size,
                      "sha256": hashlib.sha256(path.read_bytes()).hexdigest(), "soname": sonames[0],
                      "runtimePaths": rpaths, "needed": needed, "symbolFloors": floors,
                      "resolvedDependencies": resolved_files, "lddOutput": dependencies})
    symbols = run("nm", "-D", "--defined-only", str(directory / "libJYPPX.OpenCV.Native.so"))
    exports = sorted(re.findall(r"\bT (jyppx_ocv_\w+)$", symbols, re.M))
    manifest = root / ("src/OpenCvSharp.Native/generated/native_abi_mini_manifest.txt" if profile == "mini"
                       else "src/OpenCvSharp.Native/generated/native_abi_manifest.txt")
    expected = sorted(line.split("|")[0] for line in manifest.read_text().splitlines()
                      if line.startswith("jyppx_ocv_") and "|" in line)
    if exports != expected or len(exports) != {"full": 2669, "mini": 527}[profile]:
        raise ValueError("Native ABI names differ from the exact profile manifest")
    return {"profile": profile, "rid": "fedora.44-x64", "runtimeFileCount": len(names),
            "exportCount": len(exports), "exactAbiManifestMatched": True,
            "exportNamesSha256": hashlib.sha256(("\n".join(exports) + "\n").encode()).hexdigest(),
            "glibc": run("getconf", "GNU_LIBC_VERSION").strip(), "genericLinuxBaselineClaimed": False,
            "files": files}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--directory", type=Path, required=True)
    parser.add_argument("--profile", choices=("full", "mini"), required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    result = audit(args.directory, args.profile, Path(__file__).resolve().parent.parent)
    args.output.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(f"FEDORA_51_ELF_OK profile={args.profile} files={len(result['files'])} exports={result['exportCount']}")
