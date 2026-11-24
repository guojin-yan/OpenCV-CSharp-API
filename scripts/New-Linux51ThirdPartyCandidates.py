"""Add measured third-party notices to four isolated, non-publishing candidates."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET
import zipfile


def sha(data):
    return hashlib.sha256(data).hexdigest()


def file_record(path):
    data = path.read_bytes()
    return {"bytes": len(data), "sha256": sha(data)}


def read_archive(path):
    with zipfile.ZipFile(path) as archive:
        names = archive.namelist()
        if len(set(names)) != len(names) or any(n.startswith("/") or ".." in n.split("/") or "\\" in n or ":" in n for n in names):
            raise ValueError("Unsafe or duplicated archive entry")
        if archive.testzip():
            raise ValueError("Archive CRC failure")
        return {name: archive.read(name) for name in names if not name.endswith("/")}


def write_archive(path, entries):
    with zipfile.ZipFile(path, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as archive:
        for name, data in sorted(entries.items()):
            info = zipfile.ZipInfo(name, (1980, 1, 1, 0, 0, 0))
            info.compress_type = zipfile.ZIP_DEFLATED
            archive.writestr(info, data)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo", type=Path, required=True)
    parser.add_argument("--build-evidence", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--revision", required=True)
    args = parser.parse_args()
    repo, measured, output = args.repo.resolve(), args.build_evidence.resolve(), args.output.resolve()
    if not re.fullmatch(r"[a-f0-9]{40}", args.revision) or output.exists():
        raise ValueError("Expected a fixed source revision and a fresh output directory")
    evidence = json.loads((measured / "build-evidence.json").read_text())
    if evidence["openCvVersion"] != "5.1.0" or len(evidence["modules"]) != 17 or len(evidence["components"]) != 15:
        raise ValueError("Unexpected measured component inventory")
    output.mkdir(parents=True)
    components = {row["id"]: row for row in evidence["components"]}
    modules = {row["module"]: row for row in evidence["modules"]}
    references, results = [], []
    for flavor, relative in (
            ("preview", "packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json"),
            ("headless", "packaging/runtime/opencv-5.1.0-local-linux-headless-evidence.json")):
        reference = repo / relative
        baseline = json.loads(reference.read_text())
        references.append({"path": relative, **file_record(reference)})
        for profile in ("full", "mini"):
            if flavor == "headless":
                original = next(row for row in baseline["packages"] if row["profile"] == profile)
                original_path = repo / original["artifactPath"]
            else:
                # The original Linux record stores packages inside each profile.
                row = next(row for row in baseline["profiles"] if row["name"] == profile)
                original = {"id": row["packageId"], "version": row["packageVersion"], "artifactPath": row["artifactPath"],
                    "bytes": row["packageBytes"], "sha256": row["packageSha256"]}
                original_path = repo / original["artifactPath"]
            if file_record(original_path) != {"bytes": original["bytes"], "sha256": original["sha256"]}:
                raise ValueError("Baseline package drift")
            original_entries = read_archive(original_path)
            entries = dict(original_entries)
            selected_modules = sorted({re.fullmatch(r"runtimes/linux-x64/native/libopencv_(.+)\.so\.5\.1\.0", name)[1]
                for name in entries if re.fullmatch(r"runtimes/linux-x64/native/libopencv_(.+)\.so\.5\.1\.0", name)})
            for module in selected_modules:
                name = "runtimes/linux-x64/native/libopencv_" + module + ".so.5.1.0"
                if sha(entries[name]) != modules[module]["installedElf"]["sha256"]:
                    raise ValueError("Package ELF does not match the measured build: " + module)
            selected_components = sorted({name for module in selected_modules for name in modules[module]["componentInputs"]})
            added = {}
            for name in selected_components:
                for notice in components[name]["notices"]:
                    data = (measured / "additions" / notice["path"]).read_bytes()
                    if len(data) != notice["bytes"] or sha(data) != notice["sha256"]:
                        raise ValueError("Notice source drift")
                    added[notice["path"]] = data
            if "Eigen" in selected_components:
                for path in (measured / "additions/sources/eigen3").rglob("*"):
                    if path.is_file():
                        added[path.relative_to(measured / "additions").as_posix()] = path.read_bytes()
            added["licenses/embedded-source-notices.txt"] = (measured / "additions/licenses/embedded-source-notices.txt").read_bytes()
            version = "5.1.0-" + ("headless-" if flavor == "headless" else "") + "licenses." + args.revision[:7]
            notice_text = (
                "OpenCV 5.1.0 third-party notices\n\n"
                "This software is based in part on the work of the Independent JPEG Group.\n\n"
                "These notices accompany the measured Linux " + flavor + "/" + profile + " candidate.\n"
                "The exact Eigen headers observed by the compiler are included under sources/eigen3.\n"
                "The bundled embedded-source-notices.txt preserves leading notices from the shared\n"
                "upstream build, including source units outside this individual profile.\n"
                "Component versions and inputs refer to the retained build, not newer releases.\n"
                "System runtime libraries are supplied by the operating system.\n\n")
            for name in selected_components:
                row = components[name]
                notice_text += name + " / " + row["version"] + "\nSource: " + row["sourceRoot"] + "\n"
                if name != "Eigen":
                    notice_text += "https://github.com/opencv/opencv/tree/" + evidence["upstreamCommit"] + "/" + Path(row["sourceRoot"]).relative_to(evidence["sourceRoot"]).as_posix() + "\n"
                notice_text += "License declaration: " + row["licenseDeclared"] + "\n"
                for notice in row["notices"]:
                    notice_text += "\n" + notice["path"] + "\n" + added[notice["path"]].decode("utf-8-sig") + "\n"
                notice_text += "\n"
            added["THIRD_PARTY_NOTICES.txt"] = notice_text.encode("utf-8")
            supplement = {"schemaVersion": 1, "candidateOnly": True, "publicationAllowed": False,
                "packageId": original["id"], "packageVersion": version, "sourceBaseCommit": args.revision,
                "flavor": flavor, "profile": profile, "originalPackageVersion": original["version"],
                "originalPackageSha256": original["sha256"], "nativeBytesChanged": False,
                "buildEvidenceSha256": file_record(measured / "build-evidence.json")["sha256"],
                "modules": selected_modules, "components": selected_components,
                "completeSourceLicenseReview": False, "vulnerabilityReviewComplete": False}
            added["build/third-party-supplement.json"] = (json.dumps(supplement, indent=2) + "\n").encode()
            for name in added:
                if name in entries:
                    raise ValueError("Supplement would overwrite an existing archive entry")
            entries.update(added)
            nuspecs = [name for name in entries if name.endswith(".nuspec")]
            if len(nuspecs) != 1:
                raise ValueError("Ambiguous nuspec")
            nuspec = ET.fromstring(entries[nuspecs[0]])
            namespace = nuspec.tag.split("}")[0] + "}"
            ET.register_namespace("", namespace[1:-1])
            metadata = nuspec.find(namespace + "metadata")
            metadata.find(namespace + "version").text = version
            license_node = metadata.find(namespace + "license")
            if license_node is None:
                license_node = ET.SubElement(metadata, namespace + "license")
            license_node.attrib = {"type": "file"}
            license_node.text = "THIRD_PARTY_NOTICES.txt"
            license_url = metadata.find(namespace + "licenseUrl")
            if license_url is not None:
                metadata.remove(license_url)
            entries[nuspecs[0]] = ET.tostring(nuspec, encoding="utf-8", xml_declaration=True)
            core_properties = [name for name in entries if name.endswith(".psmdcp")]
            if len(core_properties) != 1:
                raise ValueError("Ambiguous package core properties")
            properties = ET.fromstring(entries[core_properties[0]])
            properties_namespace = properties.tag.split("}")[0] + "}"
            ET.register_namespace("", properties_namespace[1:-1])
            properties.find(properties_namespace + "version").text = version
            entries[core_properties[0]] = ET.tostring(properties, encoding="utf-8", xml_declaration=True)
            # Preserve the old measurement as parent provenance, while making
            # its original version explicit alongside the new package identity.
            headless_path = "build/headless-candidate.json"
            if headless_path in entries:
                parent = json.loads(entries[headless_path])
                parent["originalPackageVersion"] = parent["packageVersion"]
                parent["packageVersion"] = version
                parent["thirdPartySupplement"] = "build/third-party-supplement.json"
                entries[headless_path] = (json.dumps(parent, indent=2) + "\n").encode()
            content_types = ET.fromstring(entries["[Content_Types].xml"])
            content_namespace = content_types.tag.split("}")[0] + "}"
            ET.register_namespace("", content_namespace[1:-1])
            for extension in sorted({Path(name).suffix.removeprefix(".") for name in entries} - {""}):
                media = "application/json" if extension == "json" else "text/plain" if extension in {"txt", "md", "h", "hpp"} else "application/octet-stream"
                if not any(node.attrib.get("Extension") == extension for node in content_types):
                    ET.SubElement(content_types, content_namespace + "Default", {"Extension": extension, "ContentType": media})
            entries["[Content_Types].xml"] = ET.tostring(content_types, encoding="utf-8", xml_declaration=True)
            path = output / (original["id"] + "." + version + ".nupkg")
            write_archive(path, entries)
            restored = read_archive(path)
            for name, data in original_entries.items():
                if name.startswith("runtimes/") and restored[name] != data:
                    raise ValueError("Native bytes changed")
            results.append({**supplement, "artifactPath": path.relative_to(repo).as_posix(), **file_record(path),
                "originalArtifactPath": original_path.relative_to(repo).as_posix(),
                "addedEntries": sorted(added), "entries": [{"path": name, "bytes": len(data), "sha256": sha(data),
                    "sha1": hashlib.sha1(data).hexdigest()} for name, data in sorted(restored.items())]})
            print("LINUX_51_THIRD_PARTY_PACKAGE_OK flavor=" + flavor + " profile=" + profile + " components=" + str(len(selected_components)))
    (output / "packages.json").write_text(json.dumps(results, indent=2) + "\n")
    (output / "references.json").write_text(json.dumps(references, indent=2) + "\n")


if __name__ == "__main__":
    main()
