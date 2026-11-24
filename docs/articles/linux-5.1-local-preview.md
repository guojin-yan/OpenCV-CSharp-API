# OpenCV 5.1 local Linux preview

The [local Linux evidence](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json) records a source build of OpenCV and opencv_contrib 5.1.0 on native x86_64 in a local Docker VM. Both the producer and a separate consumer use Ubuntu 22.04/glibc 2.35, pinned to image digest `sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982`. The record captures their different container IDs, mounts and resource limits; it makes no hosted-runner claim.

| Profile | OpenCV modules | Wrapper sources / exports | Canonical ELFs | Native package entries |
| --- | ---: | ---: | ---: | ---: |
| Full preview | 17 | 49 / 2,669 | 18 | 52 |
| Mini preview | 6 | 9 / 527 | 7 | 19 |

Full follows the existing generic preview matrix. Its 17 modules include contrib `ml`; the additional 25 optional contrib modules enabled by the Windows Full build are outside this Linux preview build. Mini reuses the six required libraries from the Full upstream install and builds a separate wrapper. Its staged input and package contain only the Mini dependency closure; this is not an independent minimal upstream compilation. The repository-owned photo CCM instance-state patch is applied to the official OpenCV commit; contrib remains clean.

OpenCV 5.1.0's actual ELF SONAME is `.so.501`, derived from upstream `OPENCV_SOVERSION` as major plus a two-digit minor. Its Windows DLL suffix is `510`, and its canonical Linux filename ends in `.so.5.1.0`. The shared package consumer derives the ELF ABI name separately. The producer, package-provenance and package-closure workflows now use the same upstream rule through dedicated PowerShell/POSIX helpers. This includes the lifecycle-refresh candidate producer. No live workflow pins `.so.500`; historical 5.0 fixtures retain their original names.

The [workflow audit evidence](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/opencv-5.1.0-elf-workflow-evidence.json) binds the current workflow/helper bytes and the original Full/Mini package hashes. The exact generic producer audit body ran locally against both preserved runtime directories, with GitHub inputs resolved and paths relocated into temporary workspaces. Full validates 18 canonical ELFs/52 native files and Mini 7/19; dependency and loader checks pass. Restoring the old `.so.500` filename expectation fails for both profiles. This is a local workflow check on the existing Ubuntu producer.

`Test-OpenCvElfSonameSurface.ps1` exercises seven version cases, rejects twelve invalid inputs and executes all nine PowerShell name blocks, thirteen shell name blocks and fourteen dependency-count expressions, including wrong-ABI negative controls. With the retained package artifacts, it also verifies the complete 52/19 native-entry sets and byte equality of each unversioned/SONAME/full-version triplet:

```powershell
./scripts/Test-OpenCvElfSonameSurface.ps1 -RequirePackageArtifacts
```

Each wrapper must pass all three native CTests, including ABI manifest consistency and actual exports. The isolated ELF audit checks every canonical binary and alias, x86_64 machine type, exact `$ORIGIN` runtime paths, direct required OpenCV links, resolved dependencies and package payload hashes. It rejects external codec/backend/protobuf SONAMEs and producer search paths. Symbol ceilings are GLIBC 2.35, GLIBCXX 3.4.30 and CXXABI 1.3.13. The recorded per-file floors are measured from `readelf --version-info`.

The independent consumer receives only the two NuGet preview packages, the existing verified managed 5.1.0 package, validation scripts and hash-checked .NET/PowerShell archives. It has no producer volume, source checkout, OpenCV install, C++ compiler, CMake or loader overrides. Separate Full/Mini restores use an isolated NuGet source and cache. Real calls cover Mat, color conversion, PNG encode/decode and VideoCapture construction; Full also checks DNN blob construction and KNN training/prediction, while Mini checks the expected NOT_LINKED result. This smoke does not establish model inference, GPU support or camera/network/video-file backend coverage.

PowerShell 7.4.17's official linux-x64 asset SHA256 is `dcfe6060fc86abcb859ce1f8f80843ce50bab0585396de56380ed9f25176ac6d`, verified against the [official release asset metadata](https://api.github.com/repos/PowerShell/PowerShell/releases/tags/v7.4.17). The previous Fedora candidate bootstrap value omitted a character and could never pass `sha256sum`; that configuration is corrected as part of this work. The SDK is 10.0.401, with its archive SHA512 obtained from Microsoft's official release metadata and checked before extraction.

The immutable Windows SBOM record retains its actual measurement commit and package hashes. Its guard reconstructs the historical consumer-verifier input from that Git object after the Linux-only naming correction; all production and packaging inputs still have to match that Windows measurement. No Windows SBOM or runtime measurement is relabeled as a new Linux result.

To reproduce the producer after installing the recorded toolchain and cloning the exact official commits under `<workspace>/opencv-source`:

```powershell
./scripts/Build-LocalLinux51Preview.ps1 -WorkspaceRoot <workspace> -ContainerImage <pinned-image> -ContainerImageId <actual-image-id>
```

Copy the output preview packages and the recorded managed package into a separate consumer's artifact directory, together with the small validation-script kit and toolchain. Run `Invoke-LocalLinux51Consumer.ps1` there, retaining `consumer-driver.log`. Export producer/consumer outputs under repository `artifacts/`, then collect and verify:

```powershell
./scripts/Measure-LocalLinux51Evidence.ps1 -ProducerOutput <producer-output> -ConsumerOutput <consumer-output> -ProducerContainer <producer-name> -ConsumerContainer <consumer-name>
./scripts/Test-LocalLinux51Evidence.ps1 -RequirePackageArtifacts
```

The record binds the normalized source input set, raw producer/provenance/package/consumer outputs, complete ZIP entry manifests and both package hashes. Its guard includes negative fixtures for skipped native execution, shared producer inputs, changed package payloads, external dependencies, absolute loader paths and overstated publication/hosted coverage.

The package identities remain `JYPPX.OpenCV.runtime.linux-x64.preview` and `.preview.mini`, with publication disabled. Only Ubuntu 22.04 x64 is covered by this local 5.1 measurement. Current Debian/Fedora consumers, hosted execution, ARM64, signing and the final release set remain open. Historical 5.0 three-distro results retain their original revision; stable generic promotion and the active support matrix remain unchanged.
