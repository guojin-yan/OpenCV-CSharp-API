"""Build independent Full/Mini Fedora 44 OpenCV 5.1 candidates locally."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess


def sha(data):
    return hashlib.sha256(data).hexdigest()


def write_json(path, data):
    path.write_bytes((json.dumps(data, indent=2) + "\n").encode())


def record(path, name=None):
    data = path.read_bytes()
    return {"path": name or str(path), "bytes": len(data), "sha256": sha(data)}


def invoke(arguments, log, cwd):
    with log.open("wb") as output:
        result = subprocess.run(arguments, cwd=cwd, stdout=output, stderr=subprocess.STDOUT)
    if result.returncode:
        raise RuntimeError(str(arguments[0]) + " failed: " + str(log))


def output(*args):
    return subprocess.check_output(args, text=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--workspace", type=Path, default=Path("/workspace"))
    parser.add_argument("--parallel", type=int, default=3, choices=(1, 2, 3, 4))
    args = parser.parse_args()
    work = args.workspace.resolve()
    repo = work / "repo"
    destination = work / "output"
    if output("uname", "-m").strip() != "x86_64" or 'VERSION_ID=44' not in Path("/etc/os-release").read_text():
        raise RuntimeError("Native Fedora 44 x86_64 required")
    revision = output("git", "-C", str(repo), "rev-parse", "HEAD").strip()
    if revision != "b7f8180c3949aa341bbdf2be97432fb08d1e85e2":
        raise RuntimeError("Unexpected base revision")
    for name in list(os.environ):
        if name.startswith(("OPENCV_", "OpenCvSharp")) or name in {"LD_LIBRARY_PATH", "LD_PRELOAD", "LD_AUDIT"}:
            del os.environ[name]
    os.environ["CMAKE_BUILD_PARALLEL_LEVEL"] = str(args.parallel)
    matrix = json.loads((repo / "packaging/runtime/runtime-lifecycle-refresh-matrix.json").read_bytes())
    # Explicit dependency policy for this distro candidate. This does not claim
    # the generic Ubuntu GLIBC baseline or availability of external backends.
    cmake_options = [
        "-DBUILD_ZLIB=ON", "-DBUILD_TIFF=ON", "-DBUILD_OPENJPEG=ON", "-DBUILD_JPEG=ON",
        "-DBUILD_PNG=ON", "-DBUILD_WEBP=ON", "-DBUILD_PROTOBUF=ON", "-DWITH_PROTOBUF=ON",
        "-DWITH_AVIF=OFF", "-DWITH_JPEGXL=OFF", "-DWITH_OPENEXR=OFF", "-DWITH_GSTREAMER=OFF",
        "-DWITH_GTK=OFF", "-DWITH_V4L=OFF", "-DWITH_GDAL=OFF", "-DWITH_LAPACK=OFF",
        "-DWITH_TBB=OFF", "-DWITH_OBSENSOR=OFF", "-DWITH_ITT=OFF", "-DWITH_VA=OFF",
        "-DWITH_VA_INTEL=OFF", "-DEIGEN_INCLUDE_PATH=/usr/include/eigen3"]
    profiles = []
    for profile in matrix["profiles"]:
        name = profile["name"]
        directory = destination / name
        directory.mkdir()
        profile_work = work / ("producer-" + name)
        profile_work.mkdir()
        arguments = ["pwsh", "-NoProfile", "-File", str(repo / "scripts/Build-OpenCV.ps1"),
            "-OpenCvVersion", "5.1.0", "-WorkspaceRoot", str(profile_work),
            "-OpenCvSourceRoot", str(work / "opencv-source"), "-Rid", "fedora.44-x64",
            "-RuntimePackageMatrix", "packaging/runtime/runtime-lifecycle-refresh-matrix.json",
            "-Generator", "Ninja", "-BuildList", profile["buildList"], "-ExtraCMakeArgs", " ".join(cmake_options)]
        invoke(arguments + ["-DescribeOnly"], directory / "build-plan.json", repo)
        print("FEDORA_51_UPSTREAM_STARTED profile=" + name, flush=True)
        invoke(arguments + ["-Build"], directory / "opencv-build.log", repo)
        build = profile_work / "artifacts/opencv-build/opencv-5.1.0-fedora.44-x64"
        install = profile_work / "artifacts/opencv-install/opencv-5.1.0-fedora.44-x64"
        lib = next(p for p in (install / "lib64", install / "lib") if (p / "cmake/opencv5/OpenCVConfig.cmake").is_file())
        wrapper = profile_work / "wrapper"
        wrapper_arguments = ["cmake", "-S", str(repo / "src/OpenCvSharp.Native"), "-B", str(wrapper), "-G", "Ninja",
            "-DCMAKE_BUILD_TYPE=Release", "-DCMAKE_EXPORT_COMPILE_COMMANDS=ON", "-DOPENCV_VERSION=5.1.0",
            "-DOPENCV_CSHARP_OPENCV_DIR=" + str(lib / "cmake/opencv5"),
            "-DOPENCV_CSHARP_RUNTIME_PROFILE=" + name, "-DOPENCV_CSHARP_OPENCV_BUILD_LIST=" + profile["buildList"]]
        invoke(wrapper_arguments, directory / "wrapper-configure.log", repo)
        invoke(["cmake", "--build", str(wrapper), "--parallel", str(args.parallel)], directory / "wrapper-build.log", repo)
        invoke(["ctest", "--test-dir", str(wrapper), "--output-on-failure"], directory / "ctest.log", repo)
        if "100% tests passed, 0 tests failed out of 3" not in (directory / "ctest.log").read_text():
            raise RuntimeError("Incomplete native CTest")
        commands = json.loads((wrapper / "compile_commands.json").read_bytes())
        sources = sorted(str(Path(row["file"]).relative_to(repo)) for row in commands if "CMakeFiles/JYPPX.OpenCV.Native.dir/" in row["command"])
        if len(sources) != (49 if name == "full" else 9):
            raise RuntimeError("Unexpected wrapper source set")
        stage = directory / "runtime"
        stage.mkdir()
        for module in profile["modules"]:
            for suffix in ("", ".501", ".5.1.0"):
                shutil.copyfile(lib / ("libopencv_" + module + ".so" + suffix), stage / ("libopencv_" + module + ".so" + suffix))
        shutil.copyfile(wrapper / "libJYPPX.OpenCV.Native.so", stage / "libJYPPX.OpenCV.Native.so")
        for source, target in ((build / "CMakeCache.txt", "opencv-CMakeCache.txt"), (wrapper / "CMakeCache.txt", "wrapper-CMakeCache.txt"),
            (wrapper / "compile_commands.json", "compile_commands.json"), (build / "build.ninja", "build.ninja")):
            shutil.copyfile(source, directory / target)
        # Audit each build independently with the actual Fedora runtime, never
        # classify these files as generic GLIBC 2.35 artifacts.
        audit_script = Path(__file__).with_name("Audit-LocalFedora51Runtime.py")
        invoke(["python3", str(audit_script), "--directory", str(stage), "--profile", name, "--output", str(directory / "elf.json")], directory / "elf.log", repo)
        profiles.append({"profile": name, "buildList": profile["buildList"], "modules": profile["modules"],
            "independentUpstreamBuild": True, "buildDirectory": str(build), "installDirectory": str(install),
            "wrapperDirectory": str(wrapper), "wrapperSources": sources, "wrapperSourceCount": len(sources),
            "runtimeFiles": [record(path, path.name) for path in sorted(stage.iterdir())],
            "elf": json.loads((directory / "elf.json").read_bytes())})
        print("FEDORA_51_PROFILE_OK profile=" + name, flush=True)
    source = work / "opencv-source/opencv-5.1.0"
    contrib = work / "opencv-source/opencv_contrib-5.1.0"
    if output("git", "-C", str(source), "diff", "--name-only").splitlines() != ["modules/photo/src/ccm/ccm.cpp"]:
        raise RuntimeError("Unexpected OpenCV modification")
    if output("git", "-C", str(contrib), "diff", "--name-only").strip():
        raise RuntimeError("Contrib source changed")
    subprocess.run(["git", "-C", str(source), "apply", "--reverse", "--check", str(repo / "packaging/runtime/patches/opencv-5.1.0-photo-ccm-instance-color-space.patch")], check=True)
    write_json(destination / "producer.json", {"schemaVersion": 1, "sourceBaseCommit": revision, "openCvVersion": "5.1.0",
        "rid": "fedora.44-x64", "execution": "local-docker", "candidateOnly": True, "publicationAllowed": False,
        "hostedValidated": False, "activeIdentityPromoted": False, "upstreamCommit": output("git", "-C", str(source), "rev-parse", "HEAD").strip(),
        "contribCommit": output("git", "-C", str(contrib), "rev-parse", "HEAD").strip(), "modifiedOpenCvFiles": ["modules/photo/src/ccm/ccm.cpp"],
        "compiler": output("g++", "--version"), "compilerTarget": output("g++", "-dumpmachine").strip(),
        "glibc": output("getconf", "GNU_LIBC_VERSION").strip(), "cmake": output("cmake", "--version"),
        "ninja": output("ninja", "--version").strip(), "powerShell": output("pwsh", "-NoProfile", "-Command", "$PSVersionTable.PSVersion.ToString()").strip(),
        "profiles": profiles, "extraCMakeArguments": cmake_options})
    print("FEDORA_51_PRODUCER_OK profiles=2 native_ctest=6", flush=True)


if __name__ == "__main__":
    main()
