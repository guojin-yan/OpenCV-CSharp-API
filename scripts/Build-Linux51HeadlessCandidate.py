"""Build a local, non-publishable headless overlay; never edit the base producer."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import zipfile


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def manifest(root):
    return [{"path": p.relative_to(root).as_posix(), "bytes": p.stat().st_size,
             "sha256": digest(p)} for p in sorted(root.rglob("*")) if p.is_file()]


def write_json(path, value):
    path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")


def checked(args, log, cwd=None):
    with log.open("w", encoding="utf-8") as output:
        subprocess.run([str(a) for a in args], cwd=cwd, check=True, stdout=output,
                       stderr=subprocess.STDOUT, timeout=1800)


def replace_once(text, old, new):
    if text.count(old) != 1:
        raise ValueError("Headless overlay input drift: " + old[:100])
    return text.replace(old, new)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo", type=Path, required=True)
    parser.add_argument("--workspace", type=Path, required=True)
    parser.add_argument("--install", type=Path, required=True)
    parser.add_argument("--baseline", type=Path, required=True)
    args = parser.parse_args()
    repo, work, install = args.repo.resolve(), args.workspace.resolve(), args.install.resolve()
    if work.exists():
        raise ValueError("Use a new isolated candidate workspace; prior outputs are retained.")
    if subprocess.check_output(["getconf", "GNU_LIBC_VERSION"], text=True).strip() != "glibc 2.35":
        raise ValueError("The candidate requires the measured Ubuntu 22.04 producer.")
    for key in os.environ:
        if key in {"DISPLAY", "WAYLAND_DISPLAY", "LD_LIBRARY_PATH", "LD_PRELOAD", "LD_AUDIT"} or key.startswith(("OPENCV_", "OpenCvSharp")):
            raise ValueError("Unexpected producer environment override: " + key)
    work.mkdir(parents=True)
    output = work / "output"
    output.mkdir()
    overlay = work / "overlay"
    native = overlay / "src/OpenCvSharp.Native"
    shutil.copytree(repo / "src/OpenCvSharp.Native", native)
    (overlay / "scripts").mkdir()
    for name in ("Generate-NativeAbiManifest.ps1", "Test-NativeAbiExports.ps1"):
        shutil.copyfile(repo / "scripts" / name, overlay / "scripts" / name)
    edits = []

    def edit(relative, transform):
        path = native / relative
        before = path.read_bytes()
        after = transform(before.decode("utf-8").replace("\r\n", "\n"))
        path.write_text(after, encoding="utf-8", newline="\n")
        edits.append({"path": relative, "baseSha256": hashlib.sha256(before).hexdigest(),
                      "derivedSha256": digest(path)})

    def cmake_overlay(text):
        text = replace_once(text, "geometry highgui imgcodecs", "geometry imgcodecs")
        # CTest must exercise the staged closure with no loader search overrides.
        start = text.index("if(UNIX AND NOT APPLE AND OPENCV_CSHARP_BUILD_WITH_OPENCV AND TARGET opencv_core)")
        end = text.index("\nif(WIN32)\n", start)
        text = text[:start] + text[end:]
        return text + '''
# Local candidate overlay only. ptcloud's exported CMake interface includes
# highgui although its measured ELF does not need it. Remove that interface
# edge locally; the subsequent ELF/ldd audit must prove the actual closure.
foreach(_module core imgproc imgcodecs videoio flann geometry calib stereo dnn ml objdetect photo features video stitching ptcloud)
    if(TARGET opencv_${_module})
        get_target_property(_deps opencv_${_module} INTERFACE_LINK_LIBRARIES)
        if(_deps)
            list(REMOVE_ITEM _deps opencv_highgui)
            set_target_properties(opencv_${_module} PROPERTIES INTERFACE_LINK_LIBRARIES "${_deps}")
        endif()
    endif()
endforeach()
target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HEADLESS=1)
set_target_properties(${OPENCV_CSHARP_NATIVE_SMOKE_TARGET} PROPERTIES BUILD_WITH_INSTALL_RPATH TRUE INSTALL_RPATH "$ORIGIN")
'''

    def highgui_overlay(text):
        condition = "#if defined(OPENCV_CSHARP_HAS_OPENCV)"
        if text.count(condition) != 30:
            raise ValueError("HighGui conditional count drifted")
        # Keep mat_handle.h's global HAS_OPENCV definition and its ABI intact.
        return text.replace(condition, condition + " && !defined(OPENCV_CSHARP_HEADLESS)")

    def smoke_overlay(text):
        start = text.index("    int run_highgui_structured_smoke()\n")
        end = text.index("\n#endif", start)
        return text[:start] + '''    int run_highgui_structured_smoke()
    {
        int framework_length = -1;
        if (jyppx_ocv_highgui_current_ui_framework_length(&framework_length) != OPENCV_CSHARP_STATUS_NOT_LINKED)
            return 961;
        if (jyppx_ocv_highgui_named_window("headless", 1) != OPENCV_CSHARP_STATUS_NOT_LINKED ||
            jyppx_ocv_highgui_destroy_window("headless") != OPENCV_CSHARP_STATUS_NOT_LINKED)
            return 962;
        const unsigned char invalid_utf8[] = { 0xc0, 0x80 };
        auto* registration = reinterpret_cast<jyppx_ocv_highgui_callback_registration*>(static_cast<std::uintptr_t>(1));
        if (jyppx_ocv_highgui_mouse_callback_create_utf8(
                invalid_utf8, 2, highgui_smoke_mouse_callback, nullptr, &registration) != OPENCV_CSHARP_STATUS_INVALID_ARGUMENT ||
            registration != reinterpret_cast<jyppx_ocv_highgui_callback_registration*>(static_cast<std::uintptr_t>(1)))
            return 963;
        return 0;
    }
''' + text[end:]

    edit("CMakeLists.txt", cmake_overlay)
    edit("src/highgui/highgui.cpp", highgui_overlay)
    edit("tests/native_smoke.cpp", smoke_overlay)
    # Delete copied generated manifests, then regenerate from this overlay's headers.
    for path in (native / "generated").glob("native_abi*manifest.txt"):
        path.unlink()
    checked(["pwsh", "-NoProfile", "-File", overlay / "scripts/Generate-NativeAbiManifest.ps1",
             "-RepositoryRoot", overlay], output / "abi-generate.log")
    shutil.copytree(native / "generated", output / "generated-abi")
    base_matrix = json.loads((repo / "packaging/runtime/runtime-generic-linux-preview-matrix.json").read_text())
    matrix = {"profiles": [{"name": p["name"], "modules": [m for m in p["modules"] if m != "highgui"]}
                           for p in base_matrix["profiles"]]}
    write_json(output / "candidate-modules.json", matrix)
    spec = importlib.util.spec_from_file_location("base_elf_audit", repo / "scripts/Audit-Linux51Runtime.py")
    audit_module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(audit_module)
    commit = subprocess.check_output(["git", "-C", str(repo), "rev-parse", "HEAD"], text=True).strip()
    version = "5.1.0-headless." + commit[:7]
    results = []
    for profile in matrix["profiles"]:
        name, modules = profile["name"], profile["modules"]
        build, runtime = work / (name + "-build"), output / (name + "-runtime")
        runtime.mkdir()
        checked(["cmake", "-S", native, "-B", build, "-G", "Ninja", "-DCMAKE_BUILD_TYPE=Release",
                 "-DCMAKE_EXPORT_COMPILE_COMMANDS=ON", "-DOPENCV_CSHARP_OPENCV_DIR=" + str(install / "lib/cmake/opencv5"),
                 "-DOPENCV_CSHARP_RUNTIME_PROFILE=" + name,
                 "-DOPENCV_CSHARP_OPENCV_BUILD_LIST=" + ",".join(modules)], output / (name + "-configure.log"))
        checked(["cmake", "--build", build, "--parallel", "2"], output / (name + "-build.log"))
        for module in modules:
            for suffix in ("", ".501", ".5.1.0"):
                filename = "libopencv_" + module + ".so" + suffix
                shutil.copyfile(install / "lib" / filename, runtime / filename)
                shutil.copyfile(install / "lib" / filename, build / filename)
        shutil.copyfile(build / "libJYPPX.OpenCV.Native.so", runtime / "libJYPPX.OpenCV.Native.so")
        checked(["ctest", "--test-dir", build, "--output-on-failure", "--timeout", "120"], output / (name + "-ctest.log"))
        if "100% tests passed, 0 tests failed out of 3" not in (output / (name + "-ctest.log")).read_text():
            raise ValueError("Incomplete CTest execution")
        elf = audit_module.audit(runtime, name, matrix)
        if any(re.search(r"lib(?:gtk|gdk|Qt|X11|Xext|wayland|opencv_highgui)", d)
               for f in elf["files"] for d in f["needed"]):
            raise ValueError("Forbidden GUI dependency")
        write_json(output / (name + "-elf.json"), elf)
        commands = json.loads((build / "compile_commands.json").read_text())
        sources = [c for c in commands if "CMakeFiles/JYPPX.OpenCV.Native.dir/" in c["command"]]
        if len(sources) != {"full": 49, "mini": 9}[name]:
            raise ValueError("Wrapper compilation input count drifted")
        shutil.copyfile(build / "compile_commands.json", output / (name + "-compile-commands.json"))
        shutil.copyfile(build / "CMakeCache.txt", output / (name + "-CMakeCache.txt"))
        package_id = "JYPPX.OpenCV.runtime.linux-x64.preview" + (".mini" if name == "mini" else "")
        pack = work / (name + "-pack")
        pack.mkdir()
        shutil.copytree(runtime, pack / "runtimes/linux-x64/native")
        base_packages = list((args.baseline / ("nupkg-linux-x64-preview-" + name)).glob("*.nupkg"))
        if len(base_packages) != 1:
            raise ValueError("Missing measured baseline package")
        with zipfile.ZipFile(base_packages[0]) as archive:
            for entry in archive.namelist():
                if entry.startswith("licenses/") and not entry.endswith("/"):
                    path = pack / entry
                    path.parent.mkdir(exist_ok=True, parents=True)
                    path.write_bytes(archive.read(entry))
        provenance = {"status": "candidate-only", "publicationAllowed": False, "activeIdentityAdded": False,
                      "packageId": package_id, "packageVersion": version, "profile": name,
                      "sourceBaseCommit": commit, "worktreeChangesIncluded": True, "openCvVersion": "5.1.0",
                      "upstreamReuse": "Existing measured 17-module Ubuntu 22.04 install; only the selected 16/6 modules are staged. This is not a fresh minimal upstream build.",
                      "modules": modules, "omittedModules": ["highgui"], "overlayEdits": edits,
                      "nativeFiles": manifest(runtime), "abi": {"generatedIndependently": True,
                      "manifestSha256": digest(native / "generated" / ("native_abi_mini_manifest.txt" if name == "mini" else "native_abi_manifest.txt")),
                      "exportCount": elf["exportCount"], "exportNamesSha256": elf["exportNamesSha256"]}}
        (pack / "build").mkdir()
        write_json(pack / "build/headless-candidate.json", provenance)
        (pack / "README.md").write_text("# Local OpenCV 5.1 headless candidate\n\nNot publishable. Reuses the measured upstream install and rebuilds the wrapper without HighGui. Full retains DNN; Mini omits it. Camera/network backends and hosted support are not certified.\n", encoding="utf-8")
        (pack / "candidate.csproj").write_text(f'''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework><PackageId>{package_id}</PackageId>
    <Version>{version}</Version><PackageVersion>{version}</PackageVersion><Authors>JYPPX</Authors>
    <Description>Local non-publishable OpenCV 5.1 headless candidate ({name}).</Description>
    <IncludeBuildOutput>false</IncludeBuildOutput><SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
    <EnableDefaultItems>false</EnableDefaultItems><PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression><NoWarn>NU5128</NoWarn>
  </PropertyGroup>
  <ItemGroup><None Include="runtimes/**/*;licenses/**/*;build/**/*;README.md" Pack="true" PackagePath="%(Identity)" /></ItemGroup>
</Project>
''', encoding="utf-8")
        packages = output / "packages"
        checked(["dotnet", "pack", pack / "candidate.csproj", "-c", "Release", "-o", packages], output / (name + "-pack.log"))
        package = packages / (package_id + "." + version + ".nupkg")
        # NuGet ZIP container metadata is deterministic for independent repetition.
        with zipfile.ZipFile(package) as archive:
            entries = {n: archive.read(n) for n in archive.namelist()}
        with zipfile.ZipFile(package, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as archive:
            for path, data in sorted(entries.items()):
                info = zipfile.ZipInfo(path, (1980, 1, 1, 0, 0, 0))
                info.compress_type = zipfile.ZIP_DEFLATED
                archive.writestr(info, data)
        results.append({"profile": name, "modules": modules, "wrapperSourceCount": len(sources),
                        "runtimeFiles": manifest(runtime), "abi": provenance["abi"],
                        "package": {"id": package_id, "version": version, "path": package.relative_to(output).as_posix(),
                                    "bytes": package.stat().st_size, "sha256": digest(package)}})
        print("HEADLESS_51_BUILD_OK profile=" + name, flush=True)
    write_json(output / "producer.json", {"status": "candidate-only", "openCvVersion": "5.1.0",
               "sourceBaseCommit": commit, "worktreeChangesIncluded": True, "overlayEdits": edits,
               "upstreamInstall": str(install), "upstreamBuildReused": True,
               "upstreamProducer": json.loads((args.baseline / "producer.json").read_text()),
               "profiles": results})
    print("HEADLESS_51_PRODUCER_OK", flush=True)


if __name__ == "__main__":
    main()
