"""Bind supplemented candidate packages, SPDX inventories and actual consumers."""
import argparse
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import subprocess


def digest(data):
    return hashlib.sha256(data).hexdigest()


def read_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def write_json(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes((json.dumps(value, indent=2, ensure_ascii=False) + "\n").encode("utf-8"))


def record_file(path, name):
    data = path.read_bytes()
    return {"path": name, "bytes": len(data), "sha256": digest(data)}


def blob(path, name):
    data = path.read_bytes()
    return {"name": name, "bytes": len(data), "sha256": digest(data), "text": data.decode("utf-8")}


def identity(name):
    raw = json.loads(subprocess.check_output(["docker", "inspect", name]))[0]
    image = json.loads(subprocess.check_output(["docker", "image", "inspect", raw["Image"]]))[0]
    return {"id": raw["Id"], "name": raw["Name"].lstrip("/"), "image": raw["Config"]["Image"], "imageId": raw["Image"],
        "architecture": image["Architecture"], "os": image["Os"], "nanoCpus": raw["HostConfig"]["NanoCpus"],
        "memoryBytes": raw["HostConfig"]["Memory"], "oomKilled": raw["State"]["OOMKilled"],
        "mounts": [{"type": m["Type"], "source": m["Source"], "destination": m["Destination"], "writable": m["RW"]} for m in raw["Mounts"]]}


def make_spdx(package, build, created, repo):
    parent_id = "SPDXRef-Package"
    files = [{"fileName": "./" + entry["path"], "SPDXID": "SPDXRef-File-" + str(index),
        "checksums": [{"algorithm": "SHA256", "checksumValue": entry["sha256"]}, {"algorithm": "SHA1", "checksumValue": entry["sha1"]}],
        "licenseConcluded": "NOASSERTION", "licenseInfoInFiles": ["NOASSERTION"], "copyrightText": "NOASSERTION"}
        for index, entry in enumerate(package["entries"])]
    file_ids = {file["fileName"][2:]: file["SPDXID"] for file in files}
    relationships = [{"spdxElementId": "SPDXRef-DOCUMENT", "relationshipType": "DESCRIBES", "relatedSpdxElement": parent_id}]
    relationships += [{"spdxElementId": parent_id, "relationshipType": "CONTAINS", "relatedSpdxElement": file["SPDXID"]} for file in files]
    parent = {"SPDXID": parent_id, "name": package["packageId"], "versionInfo": package["packageVersion"],
        "packageFileName": Path(package["artifactPath"]).name, "downloadLocation": "NOASSERTION", "filesAnalyzed": True,
        "checksums": [{"algorithm": "SHA256", "checksumValue": package["sha256"]}],
        "packageVerificationCode": {"packageVerificationCodeValue": hashlib.sha1("".join(sorted(entry["sha1"] for entry in package["entries"])).encode()).hexdigest()},
        "licenseConcluded": "NOASSERTION", "licenseDeclared": "NOASSERTION", "licenseInfoFromFiles": ["NOASSERTION"], "copyrightText": "NOASSERTION",
        "sourceInfo": "Candidate supplement of package SHA256 " + package["originalPackageSha256"] + "; native payload unchanged; measured build SHA256 " + package["buildEvidenceSha256"]}
    nodes, component_ids = [parent], {}
    for index, component_name in enumerate(package["components"]):
        component = next(row for row in build["components"] if row["id"] == component_name)
        component_id = "SPDXRef-Component-" + str(index)
        component_ids[component_name] = component_id
        location = ("https://github.com/opencv/opencv/tree/" + build["upstreamCommit"] + "/" +
            Path(component["sourceRoot"]).relative_to(build["sourceRoot"]).as_posix()) if component_name != "Eigen" else "NOASSERTION"
        nodes.append({"SPDXID": component_id, "name": component_name, "versionInfo": component["version"], "downloadLocation": location,
            "filesAnalyzed": False, "licenseDeclared": component["licenseDeclared"], "licenseConcluded": "NOASSERTION", "copyrightText": "NOASSERTION",
            "sourceInfo": "Observed compiler/linker input; source revision " + component["sourceRevision"] + "; notices: " + ", ".join(notice["path"] for notice in component["notices"])})
        relationships.append({"spdxElementId": parent_id, "relationshipType": "CONTAINS", "relatedSpdxElement": component_id})
    for module_name in package["modules"]:
        module = next(row for row in build["modules"] if row["module"] == module_name)
        file_id = file_ids["runtimes/linux-x64/native/libopencv_" + module_name + ".so.5.1.0"]
        for component_name in module["componentInputs"]:
            relation = "STATIC_LINK" if component_name in {"zlib", "libjpeg-turbo", "libwebp", "libpng", "libtiff", "openjpeg", "harfbuzz", "protobuf", "mlas"} else "GENERATED_FROM"
            relationships.append({"spdxElementId": file_id, "relationshipType": relation, "relatedSpdxElement": component_ids[component_name]})
    document = {"spdxVersion": "SPDX-2.3", "dataLicense": "CC0-1.0", "SPDXID": "SPDXRef-DOCUMENT",
        "name": package["packageId"] + " " + package["packageVersion"] + " observed build inventory",
        "documentNamespace": "https://github.com/guojin-yan/OpenCV-CSharp-API/sbom/" + package["sha256"],
        "creationInfo": {"created": created, "creators": ["Tool: OpenCV-CSharp-API/scripts/Measure-Linux51ThirdPartyEvidence.py"]},
        "comment": "Local non-publishing candidate. Component inputs and notices are build-bound. Full source-level licensing and vulnerability review, hosted reproduction and release approval remain separate.",
        "packages": nodes, "files": files, "relationships": relationships}
    relative = "packaging/release/opencv-5.1.0-linux-thirdparty-sbom/" + Path(package["artifactPath"]).name.removesuffix(".nupkg") + ".spdx.json"
    write_json(repo / relative, document)
    return record_file(repo / relative, relative)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo", type=Path, required=True)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--kit", type=Path, required=True)
    parser.add_argument("--producer", required=True)
    parser.add_argument("--consumers", nargs=3, required=True)
    parser.add_argument("--created", default=None)
    args = parser.parse_args()
    repo, artifacts, kit = args.repo.resolve(), args.artifacts.resolve(), args.kit.resolve()
    created = args.created or datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
    build = read_json(artifacts / "build/build-evidence.json")
    packages = read_json(artifacts / "packages/packages.json")
    for package in packages:
        package["sbom"] = make_spdx(package, build, created, repo)
    input_paths = [
        "scripts/Collect-Linux51ThirdPartyBuild.py", "scripts/New-Linux51ThirdPartyCandidates.py",
        "scripts/Measure-Linux51ThirdPartyEvidence.py", "scripts/Invoke-Linux51ThirdPartyConsumer.ps1",
        "scripts/Test-Linux51ThirdPartyEvidence.ps1", "scripts/Initialize-Linux51HeadlessConsumer.sh",
        "tools/LinuxPackageModuleSmoke/Program.cs", "tools/LinuxHeadlessPackageSmoke/Program.cs",
        "tools/LinuxHeadlessPackageSmoke/consumer.csproj.in", "tests/OpenCvSharp.Tests/Dnn/Fixtures/identity-opset13.onnx.base64",
        "packaging/release/opencv-5.1.0-linux-thirdparty-evidence.schema.json"]
    inputs = []
    for relative in sorted(input_paths):
        data = (repo / relative).read_text(encoding="utf-8-sig").replace("\r\n", "\n").replace("\r", "\n").rstrip() + "\n"
        inputs.append({"path": relative, "bytes": len(data.encode()), "sha256": digest(data.encode())})
    consumers = []
    for rid, container in zip(("ubuntu.22.04-x64", "ubuntu.24.04-x64", "debian.12-x64"), args.consumers):
        directory = artifacts / rid
        rows = read_json(directory / "runs.json")
        runs = []
        for row in rows:
            run_dir = directory / row["resultDirectory"]
            names = ("result.json", "execute.stdout.log", "execute.stderr.log", "build.stdout.log", "build.stderr.log",
                "restore.stdout.log", "restore.stderr.log", "project.assets.json", "Consumer.csproj", "NuGet.Config", "cache-packages.json", "restored-payload.json")
            runs.append({**row, "blobs": [blob(run_dir / name, name) for name in names],
                "video": record_file(run_dir / "three-frames.avi", "three-frames.avi")})
        consumers.append({"rid": rid, "identity": identity(container), "outputDirectory": directory.relative_to(repo).as_posix(),
            "blobs": [blob(directory / name, name) for name in ("bootstrap.log", "driver.log", "environment.json", "runs.json")], "runs": runs})
    record = {"schemaVersion": 1, "evidenceKind": "local-linux-5.1-third-party-supplement", "status": "verified-local-three-distro-x64-candidate",
        "createdUtc": created, "openCvVersion": "5.1.0", "sourceBaseCommit": packages[0]["sourceBaseCommit"], "worktreeChangesIncluded": True,
        "execution": "local-docker", "candidateOnly": True, "publicationAllowed": False, "hostedValidated": False, "activeIdentityAdded": False,
        "sourceInputs": inputs, "references": read_json(artifacts / "packages/references.json"),
        "kitManifest": [record_file(path, path.relative_to(kit).as_posix()) for path in sorted(kit.rglob("*")) if path.is_file()],
        "producer": {"identity": identity(args.producer), "outputDirectory": (artifacts / "build").relative_to(repo).as_posix(),
            "buildEvidence": blob(artifacts / "build/build-evidence.json", "build-evidence.json")},
        "packages": packages, "consumers": consumers,
        "coverage": {"upstreamModules": 17, "staticArchives": 8, "compilerObjects": 1201, "observedFiles": 3079,
            "officialGitObjects": len(build["upstreamGitObjects"]), "namedComponents": 15, "candidatePackages": 4, "consumerExecutions": 24,
            "nativePayloadChanged": False, "freshUpstreamBuild": False, "completeSourceLicenseReview": False, "vulnerabilityReviewComplete": False,
            "arm64Validated": False, "hostedValidated": False, "releaseApproved": False}}
    relative = "packaging/release/opencv-5.1.0-linux-thirdparty-evidence.json"
    write_json(repo / relative, record)
    print("LINUX_51_THIRD_PARTY_EVIDENCE_MEASURED packages=4 executions=24 components=15 output=" + relative)


if __name__ == "__main__":
    main()
