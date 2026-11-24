"""Collect observed static/header dependencies from the retained Linux 5.1 build."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shlex
import subprocess


def sha(data):
    return hashlib.sha256(data).hexdigest()


def checked(*args, cwd=None):
    return subprocess.check_output(args, cwd=cwd, text=True)


def manifest(path, name=None):
    data = path.read_bytes()
    return {"path": name or str(path), "bytes": len(data), "sha256": sha(data)}


def parse_ninja(text):
    edges = {}
    current = None
    # This collector accepts CMake's observed path alphabet; ambiguous Ninja
    # escapes must fail rather than silently losing a link or source input.
    if any(token in text for token in ("$ ", "$:", "$|")):
        raise ValueError("Unsupported escaped Ninja path")
    for line in text.replace("$\n", "").splitlines():
        if line.startswith("build "):
            outputs, rhs = line[6:].split(": ", 1)
            rule, *tokens = rhs.split()
            groups, group = [[], [], []], 0
            for token in tokens:
                if token == "|":
                    group = 1
                elif token == "||":
                    group = 2
                else:
                    groups[group].append(token)
            current = {"rule": rule, "inputs": groups, "variables": {}}
            for output in outputs.split():
                if output == "|":  # Ninja's implicit outputs share the edge.
                    continue
                if output in edges:
                    raise ValueError("Duplicated Ninja output: " + output)
                edges[output] = current
        elif line.startswith("  ") and " = " in line and current is not None:
            key, value = line.strip().split(" = ", 1)
            current["variables"][key] = value
        elif line and not line.startswith(" "):
            current = None
    return edges


def parse_deps(text):
    result, current = {}, None
    for line in text.splitlines():
        if line and not line.startswith(" "):
            match = re.fullmatch(r"(.+): #deps (\d+), deps mtime (\d+) \(VALID\)", line)
            if not match or match[1] in result:
                raise ValueError("Missing, stale or duplicated compiler dependency record: " + line)
            current = {"mtime": int(match[3]), "expected": int(match[2]), "paths": []}
            result[match[1]] = current
        elif line.startswith("    "):
            if current is None:
                raise ValueError("Dependency without object")
            current["paths"].append(line.strip())
    if not result or any(len(v["paths"]) != v["expected"] for v in result.values()):
        raise ValueError("Incomplete compiler dependencies")
    return result


def leading_notice(data):
    text = data.decode("utf-8", errors="strict")
    match = re.match(r"(?s)\A(?:\s+|//[^\n]*(?:\n|$)|/\*.*?\*/)+", text)
    if not match or not re.search(r"(?i)copyright|licen[cs]e|permission|redistribut", match[0]):
        raise ValueError("Expected original source license header")
    return match[0].encode("utf-8")


def archive_contents(data):
    if not data.startswith(b"!<arch>\n"):
        raise ValueError("Expected regular GNU ar archive")
    offset, names, members = 8, b"", []
    while offset < len(data):
        header = data[offset:offset + 60]
        if len(header) != 60 or header[58:] != b"`\n":
            raise ValueError("Malformed archive member")
        name, size = header[:16].decode().strip(), int(header[48:58])
        start = offset + 60
        body = data[start:start + size]
        if len(body) != size:
            raise ValueError("Truncated archive")
        if name == "//":
            names = body
        elif name != "/":
            if name.startswith("/"):
                index = int(name[1:])
                name = names[index:names.index(b"/\n", index)].decode()
            else:
                name = name.removesuffix("/")
            members.append((name, body))
        offset = start + size + size % 2
    if offset != len(data):
        raise ValueError("Unexpected archive trailer")
    return members


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--build", type=Path, required=True)
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--install", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    build, source, output = args.build.resolve(), args.source.resolve(), args.output.resolve()
    contrib = source.parent / "opencv_contrib-5.1.0"
    if output.exists():
        raise ValueError("Use a fresh evidence directory")
    if checked("git", "-C", str(source), "rev-parse", "HEAD").strip() != "e4f78fbcf1a773fbed69e14500e8b734963eb585":
        raise ValueError("Unexpected official OpenCV revision")
    modified = checked("git", "-C", str(source), "diff", "--name-only").splitlines()
    if modified != ["modules/photo/src/ccm/ccm.cpp"]:
        raise ValueError("Unexpected producer source modifications")
    checked("git", "-C", str(source), "apply", "--reverse", "--check",
            str(Path(__file__).resolve().parents[1] / "packaging/runtime/patches/opencv-5.1.0-photo-ccm-instance-color-space.patch"))
    if checked("git", "-C", str(contrib), "rev-parse", "HEAD").strip() != "b67a855188a3d683813731010f6d39ac614733a4" or checked("git", "-C", str(contrib), "diff", "--name-only").strip():
        raise ValueError("Unexpected contrib revision or edits")
    output.mkdir(parents=True)
    ninja_text = (build / "build.ninja").read_text()
    deps_text = checked("ninja", "-t", "deps", cwd=build)
    (output / "build.ninja").write_text(ninja_text)
    (output / "ninja-deps.txt").write_text(deps_text)
    edges, deps = parse_ninja(ninja_text), parse_deps(deps_text)
    # Versions refer to the actual vendored snapshots, not the newest upstream.
    specs = [
        ("zlib", "3rdparty/zlib", "1.3.2", "Zlib", ["LICENSE"]),
        ("libjpeg-turbo", "3rdparty/libjpeg-turbo", "3.1.2", "IJG AND BSD-3-Clause AND Zlib", ["LICENSE.md", "README.ijg"]),
        ("libwebp", "3rdparty/libwebp", "vendored-e4f78fbc", "BSD-3-Clause", ["COPYING"]),
        ("libpng", "3rdparty/libpng", "1.6.57", "libpng-2.0", ["LICENSE"]),
        ("libtiff", "3rdparty/libtiff", "4.7.2", "NOASSERTION", ["LICENSE.md"]),
        ("openjpeg", "3rdparty/openjpeg", "2.5.4", "BSD-2-Clause", ["LICENSE"]),
        ("harfbuzz", "3rdparty/harfbuzz", "14.2.1", "MIT-old", ["COPYING"]),
        ("protobuf", "3rdparty/protobuf", "3.19.1", "BSD-3-Clause", ["LICENSE"]),
        ("flatbuffers", "3rdparty/flatbuffers", "25.9.23", "Apache-2.0", ["LICENSE.txt"]),
        ("mlas", "3rdparty/mlas", "onnxruntime-62f742f1aa0c3102745ed35e3d869eaee845b9ac", "MIT", ["LICENSE", "README.md"]),
        ("annoy", "modules/features/3rdparty/annoy", "vendored-e4f78fbc", "Apache-2.0", ["annoylib.h:header"]),
        ("mscr", "modules/features/3rdparty/mscr", "vendored-e4f78fbc", "BSD-3-Clause", ["chi_table.h:header"]),
        ("flann", "modules/flann", "1.6.10-opencv-fork", "BSD-2-Clause", ["include/opencv2/flann/defines.h:header"]),
        ("softfloat-fdlibm", "modules/core/src/softfloat.cpp", "SoftFloat-3c-and-fdlibm", "BSD-3-Clause AND SunPro", [":header"]),
        ("Eigen", "/usr/include/eigen3", "3.4.0", "MPL-2.0 AND BSD-3-Clause", ["/usr/share/doc/libeigen3-dev/copyright"]),
    ]
    roots = {name: (Path(root) if root.startswith("/") else source / root) for name, root, *_ in specs}

    def component(path):
        for name, root in roots.items():
            if path == root or root in path.parents:
                return name
        if build in path.parents and "/3rdparty/" in str(path):
            family = str(path).split("/3rdparty/", 1)[1].split("/", 1)[0]
            if family in roots:
                return family
            raise ValueError("Unclassified generated third-party input: " + str(path))
        if (source in path.parents or contrib in path.parents) and "/3rdparty/" in str(path):
            raise ValueError("Unclassified vendored input: " + str(path))
        if source not in path.parents and contrib not in path.parents and build not in path.parents and not str(path).startswith(("/usr/include/", "/usr/lib/gcc/")):
            raise ValueError("Unclassified external compiler input: " + str(path))
        return None

    def rooted(path):
        path = Path(path)
        return path if path.is_absolute() else build / path

    def object_inputs(name):
        edge = edges[name]
        if "_COMPILER__" not in edge["rule"] or len(edge["inputs"][0]) != 1:
            raise ValueError("Unexpected compiler edge: " + name)
        observed = deps.get(name)
        if observed:
            if rooted(name).stat().st_mtime_ns != observed["mtime"]:
                raise ValueError("Compiler dependency timestamp drift: " + name)
            paths = {rooted(p).resolve() for p in observed["paths"]}
        elif edge["rule"].startswith("ASM_"):
            paths = set()
        else:
            raise ValueError("Missing compiler-observed dependency: " + name)
        paths.add(rooted(edge["inputs"][0][0]).resolve())
        return paths

    module_rows, used_files, used_objects, archive_rows = [], set(), set(), {}
    for target in sorted(edges):
        if not re.fullmatch(r"lib/libopencv_[a-z0-9_]+\.so\.5\.1\.0", target):
            continue
        edge = edges[target]
        objects = {p for p in edge["inputs"][0] if p.endswith(".o")}
        links = shlex.split(edge["variables"]["LINK_LIBRARIES"])
        archives = sorted({p for p in links if p.endswith(".a")})
        for archive in archives:
            if archive not in edges or not archive.startswith("3rdparty/lib/"):
                raise ValueError("Unresolved static link: " + archive)
            members = [p for p in edges[archive]["inputs"][0] if p.endswith(".o")]
            if not members:
                raise ValueError("Empty static link: " + archive)
            objects.update(members)
            if archive not in archive_rows:
                actual = archive_contents((build / archive).read_bytes())
                if len(actual) != len(members) or any(name != Path(path).name or data != (build / path).read_bytes()
                        for (name, data), path in zip(actual, members)):
                    raise ValueError("Static archive differs from compiler objects: " + archive)
                archive_rows[archive] = {**manifest(build / archive, archive), "objects": sorted(members),
                    "archiveMembers": checked("ar", "t", str(build / archive)).splitlines(), "objectBytesVerified": True}
        files = set().union(*(object_inputs(p) for p in objects))
        groups = {}
        for path in files:
            name = component(path)
            if name:
                groups.setdefault(name, []).append(str(path))
        # Installation changes RUNPATH bytes only. Verify every other ELF byte.
        original, installed = (build / target).read_bytes(), (args.install / "lib" / Path(target).name).read_bytes()
        dynamic = checked("readelf", "-d", str(build / target))
        old = re.findall(r"\(RUNPATH\).*\[(.*?)\]", dynamic)
        if len(old) != 1 or len(original) != len(installed):
            raise ValueError("Unsupported ELF install transformation: " + target)
        needle = old[0].encode() + b"\0"
        offset = original.find(needle)
        if offset < 0 or original.count(needle) != 1 or not installed[offset:].startswith(b"$ORIGIN\0"):
            raise ValueError("Unexpected installed RUNPATH: " + target)
        end = offset + len(needle)
        if original[:offset] != installed[:offset] or original[end:] != installed[end:] or installed[offset + 8:end].strip(b"\0"):
            raise ValueError("Installed ELF differs beyond RUNPATH: " + target)
        module_rows.append({"module": Path(target).name.removeprefix("libopencv_").split(".so.")[0],
            "buildElf": manifest(build / target, target), "installedElf": manifest(args.install / "lib" / Path(target).name),
            "runpathOnlyInstallChange": True, "runpathOffset": offset, "originalRunpath": old[0],
            "needed": re.findall(r"\(NEEDED\).*\[(.*?)\]", dynamic), "archives": archives,
            "objects": sorted(objects), "componentInputs": {k: sorted(v) for k, v in sorted(groups.items())}})
        used_files.update(files)
        used_objects.update(objects)
    if len(module_rows) != 17 or len(archive_rows) != 8:
        raise ValueError("Unexpected measured module/archive closure")
    version_checks = {
        "zlib": (source / "3rdparty/zlib/zlib.h", [r'#define ZLIB_VERSION "1\.3\.2"']),
        "libjpeg-turbo": (source / "3rdparty/libjpeg-turbo/CMakeLists.txt", [r'set\(VERSION 3\.1\.2\)']),
        "libpng": (source / "3rdparty/libpng/png.h", [r'libpng version 1\.6\.57']),
        "libtiff": (build / "3rdparty/libtiff/tiffvers.h", [r'TIFFLIB_VERSION_STR "LIBTIFF, Version 4\.7\.2']),
        "openjpeg": (source / "3rdparty/openjpeg/CMakeLists.txt", [r'set\(OPENJPEG_VERSION_MAJOR 2\)', r'set\(OPENJPEG_VERSION_MINOR 5\)', r'set\(OPENJPEG_VERSION_BUILD 4\)']),
        "harfbuzz": (source / "3rdparty/harfbuzz/src/hb-version.h", [r'#define HB_VERSION_STRING "14\.2\.1"']),
        "protobuf": (source / "3rdparty/protobuf/src/google/protobuf/stubs/common.h", [r'#define GOOGLE_PROTOBUF_VERSION 3019001']),
        "flatbuffers": (source / "3rdparty/flatbuffers/include/flatbuffers/base.h", [r'#define FLATBUFFERS_VERSION_MAJOR 25', r'#define FLATBUFFERS_VERSION_MINOR 9', r'#define FLATBUFFERS_VERSION_REVISION 23']),
        "mlas": (source / "3rdparty/mlas/README.md", [r'62f742f1aa0c3102745ed35e3d869eaee845b9ac']),
        "flann": (source / "modules/flann/include/opencv2/flann/config.h", [r'#define FLANN_VERSION_ "1\.6\.10"']),
        "softfloat-fdlibm": (source / "modules/core/src/softfloat.cpp", [r'Package, Release 3c', r'FDLIBM licenses']),
        "Eigen": (Path("/usr/include/eigen3/Eigen/src/Core/util/Macros.h"), [r'#define EIGEN_WORLD_VERSION 3', r'#define EIGEN_MAJOR_VERSION 4', r'#define EIGEN_MINOR_VERSION 0']),
    }
    components = []
    extra_source_files = set()
    observed_families = {component(path) for path in used_files} - {None}
    if observed_families != set(roots):
        raise ValueError("Observed component inventory differs: " + str(observed_families))
    for name, relative, version, license_id, license_paths in specs:
        inputs = sorted(path for path in used_files if component(path) == name)
        notices = []
        base = roots[name]
        if base.is_dir() and name != "Eigen":
            legal_names = re.compile(r'(?i)^(?:licen[cs]e|copying|copyright|notice|patents|authors)(?:[._-].*)?$')
            additional = [p.relative_to(base).as_posix() for p in base.rglob("*") if p.is_file() and legal_names.fullmatch(p.name)]
            license_paths = sorted(set(license_paths + additional))
        for relative_license in license_paths:
            header = relative_license.endswith(":header")
            relative_license = relative_license.removesuffix(":header")
            path = Path(relative_license) if relative_license.startswith("/") else (base / relative_license if base.is_dir() else base)
            data = path.read_bytes()
            content = leading_notice(data) if header else data
            relative_notice = path.relative_to(base).as_posix() if base.is_dir() and base in path.parents else path.name
            entry = "licenses/third-party/" + name + "/" + relative_notice + (".notice.txt" if header else "")
            destination = output / "additions" / entry
            destination.parent.mkdir(parents=True, exist_ok=True)
            destination.write_bytes(content)
            notices.append({"source": manifest(path), "headerOnly": header, **manifest(destination, entry)})
            extra_source_files.add(path)
        if name == "Eigen":
            # Ship the exact MPL-covered headers used by this build, making the
            # corresponding source available with every supplemented package.
            for path in inputs:
                entry = "sources/eigen3/" + path.relative_to(roots[name]).as_posix()
                destination = output / "additions" / entry
                destination.parent.mkdir(parents=True, exist_ok=True)
                destination.write_bytes(path.read_bytes())
        version_proof = None
        if name in version_checks:
            path, patterns = version_checks[name]
            text = path.read_text()
            matches = [re.search(pattern, text) for pattern in patterns]
            if not all(matches):
                raise ValueError("Vendored component version drift: " + name)
            version_proof = {**manifest(path), "matched": [m[0] for m in matches]}
            extra_source_files.add(path)
        components.append({"id": name, "version": version, "licenseDeclared": license_id,
            "sourceRoot": str(roots[name]), "sourceRevision": "e4f78fbcf1a773fbed69e14500e8b734963eb585" if name != "Eigen" else checked("dpkg-query", "-W", "-f=${Version}", "libeigen3-dev"),
            "versionEvidence": version_proof,
            "observedInputs": [manifest(path) for path in inputs], "notices": notices})
    # Preserve the original leading notices in the remaining observed OpenCV
    # source files too; this does not claim a source-wide licensing review.
    notices = []
    for path in sorted(used_files):
        origin = source if source in path.parents else contrib if contrib in path.parents else None
        if origin is None:
            continue
        try:
            data = leading_notice(path.read_bytes())
        except (ValueError, UnicodeError):
            continue
        notices.append("Source: " + origin.name + "/" + path.relative_to(origin).as_posix() + "\n" + data.decode("utf-8"))
    notice_path = output / "additions/licenses/embedded-source-notices.txt"
    notice_path.parent.mkdir(parents=True, exist_ok=True)
    notice_path.write_text("\n\n".join(notices) + "\n")
    upstream_objects = []
    for origin in (source, contrib):
        paths = sorted(path for path in used_files | extra_source_files if origin in path.parents and path.relative_to(origin).as_posix() not in modified)
        queries = "".join("HEAD:" + path.relative_to(origin).as_posix() + "\n" for path in paths)
        proc = subprocess.run(["git", "-C", str(origin), "cat-file", "--batch"], input=queries.encode(), stdout=subprocess.PIPE, check=True)
        offset = 0
        for path in paths:
            end = proc.stdout.index(b"\n", offset)
            info = proc.stdout[offset:end].split()
            if len(info) != 3 or info[1] != b"blob":
                raise ValueError("Observed source is not in the official Git tree: " + str(path))
            size = int(info[2])
            data = proc.stdout[end + 1:end + 1 + size]
            if path.read_bytes().replace(b"\r\n", b"\n") != data.replace(b"\r\n", b"\n"):
                raise ValueError("Observed source differs from the official Git object: " + str(path))
            upstream_objects.append({"path": str(path), "gitBlob": info[0].decode(), "sha256": sha(data)})
            offset = end + 1 + size + 1
        if offset != len(proc.stdout):
            raise ValueError("Unexpected Git object stream trailer")
    raw = {"schemaVersion": 1, "openCvVersion": "5.1.0", "buildRoot": str(build), "sourceRoot": str(source),
        "upstreamCommit": "e4f78fbcf1a773fbed69e14500e8b734963eb585", "modifiedOpenCvFiles": modified,
        "modifiedSource": [manifest(source / path, path) for path in modified],
        "rawInputs": [manifest(output / n, n) for n in ("build.ninja", "ninja-deps.txt")],
        "compilerDependencyRecords": len(deps), "modules": module_rows, "archives": list(archive_rows.values()),
        "objects": [manifest(build / name, name) for name in sorted(used_objects)],
        "observedFileManifest": [manifest(path) for path in sorted(used_files)],
        "upstreamGitObjects": upstream_objects,
        "components": sorted(components, key=lambda row: row["id"]),
        "embeddedLeadingNotices": len(notices), "fullSourceLicenseReviewComplete": False}
    (output / "build-evidence.json").write_text(json.dumps(raw, indent=2) + "\n")
    print(json.dumps({"modules": len(module_rows), "archives": len(archive_rows), "objects": len(used_objects),
        "observedFiles": len(used_files), "components": len(components), "embeddedNotices": len(notices)}))


if __name__ == "__main__":
    main()
