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

The package identities remain `JYPPX.OpenCV.runtime.linux-x64.preview` and `.preview.mini`, with publication disabled. The original producer/consumer record above covers Ubuntu 22.04 x64. Historical 5.0 three-distro results retain their original revision; stable generic promotion and the active support matrix remain unchanged.

The [local three-distro consumer evidence](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/opencv-5.1.0-local-linux-distro-consumer-evidence.json) adds independent Debian 12 and Fedora 44 consumers of the same managed/Full/Mini package bytes. It references the original Ubuntu record by hash and preserves that record's one-distro measurement boundary.

| Consumer | glibc | Full / Mini | Execution |
| --- | --- | --- | --- |
| Ubuntu 22.04 x64 | 2.35 | Passed / Passed | Original local independent consumer |
| Debian 12 x64 | 2.36 | Passed / Passed | New local independent container |
| Fedora 44 x64 | 2.43 | Passed / Passed | New local independent container |

Each new container uses an official image pinned by digest, a separate writable data volume and the same read-only kit. The kit contains eight validation/configuration files, three verified toolchain/CA archives and three NuGet packages. It contains no producer checkout, OpenCV installation or C/C++ toolchain. The bootstrap verifies the PowerShell 7.4.17 SHA256 and .NET SDK 10.0.401 SHA512 before extraction. Debian HTTPS bootstrap uses the recorded CA bundle only when its minimal image lacks one; normal TLS and archive-signature verification remain enabled.

The [local consumer matrix](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/runtime-generic-linux-5.1-local-consumer-matrix.json) preserves the original producer/profile/package contract and selects Fedora 44 for this local check. The new driver adds `fedora.44-x64 -> linux-x64` only in the disposable validation kit's RID graph. Debian already has the required import. The record retains the original and derived graph bytes and hashes; the canonical graph and active package matrix are unchanged.

All four new consumers restore from an isolated local NuGet source, compile net10.0 and execute native code. Full covers core/imgproc/PNG, a missing-file VideoIO check, a DNN blob and a trained KNN prediction. Mini covers the shared image path and omitted-module behavior. Together with the two original Ubuntu results, this completes six local profile/distro pairs. Model inference, video files/devices, additional architectures, hosted execution, stable promotion, signing and the final release set require separate evidence.

After preparing the small kit and verified archives/packages in a fresh matching container, bootstrap and execute:

```sh
sh /kit/repository/scripts/Initialize-LocalLinux51DistroConsumer.sh debian.12-x64
pwsh -NoProfile -File /consumer/repository/scripts/Invoke-LocalLinux51DistroConsumer.ps1 -ConsumerRid debian.12-x64
```

Use `fedora.44-x64` in the matching Fedora image. Retain bootstrap/driver logs and export `/consumer/artifacts` without sharing either consumer's data volume. Collect and verify on the host:

```powershell
./scripts/Measure-LocalLinux51DistroEvidence.ps1 -DebianOutput <debian-artifacts> -FedoraOutput <fedora-artifacts> -DebianContainer <debian-container> -FedoraContainer <fedora-container> -KitRoot <read-only-kit>
./scripts/Test-LocalLinux51DistroEvidence.ps1 -RequirePackageArtifacts
```

The guard binds all twelve source/collector inputs, all fourteen kit files, image architectures, container mounts, package hashes and raw preflight/consumer outputs. Its 25 negative fixtures reject incomplete matrices, altered package bytes, skipped native execution, shared volumes, mutable kits, incorrect RID imports and overstated coverage. The closeout records the completed local consumer set separately from the still-pending hosted release gate.

The [VideoIO and ONNX package evidence](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/opencv-5.1.0-local-linux-module-consumer-evidence.json) adds twelve fresh executions: Ubuntu 22.04, Debian 12 and Fedora 44, each with Full/Mini on net8.0 and net10.0. Three new independent containers use the same original package bytes, separate writable volumes and a small read-only kit. Each project restores from the local package feed into a fresh cache using its selected official SDK. The measured Linux toolchains are SDK 8.0.428/runtime 8.0.34 and SDK 10.0.401/runtime 10.0.12, with roll-forward disabled.

All twelve runs perform an exact PNG pixel roundtrip and write/read a three-frame, 32 x 24 MJPG AVI using the explicit `CV_MJPEG` backend. Frame order, dimensions, pixel checksums, a maximum channel error of two (within a tolerance of five), and EOF are checked. The retained 6,288-byte AVI is byte-identical across all runs. This demonstrates the bundled OpenCV MJPEG file backend; camera, network, FFmpeg and GStreamer behavior require separate validation.

The six Full runs each execute the repository's fixed 147-byte identity ONNX fixture twice with different input values. Named input/output and exact output values are verified on the OpenCV CPU backend. The six Mini runs exercise the same importer and verify the absent `jyppx_ocv_dnn_read_net_from_onnx_buffer` entry point. Mini DNN omission is recorded as `entrypoint-absent`; it is distinct from the earlier compatibility `NOT_LINKED` smoke.

The standalone consumer source is `tools/LinuxPackageModuleSmoke/Program.cs`, with a package-only project template beside it. `Initialize-LocalLinux51ModuleConsumer.sh` verifies the PowerShell and both SDK archives, and `Invoke-LocalLinux51ModuleConsumer.ps1` restores/builds/runs each fresh pair with timeouts. Collect exported logs, projects, NuGet assets, result JSON and AVI files with `Measure-LocalLinux51ModuleEvidence.ps1`; verify them with `Test-LocalLinux51ModuleEvidence.ps1 -RequireArtifacts`. The evidence guard binds all loaded native files to the original package manifest and both managed assemblies to the existing SBOM, checks `/proc/self/maps` paths, and rejects 32 negative fixtures. These are local functional results; they do not promote the preview or establish arbitrary model, accelerator, device or hosted coverage.

The [Linux NativeAOT evidence](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/opencv-5.1.0-local-linux-aot-evidence.json) adds two package-only net10.0/linux-x64 native publishes and six direct executable runs across the same three distributions. The compiled runtime, ILCompiler and ILLink are 10.0.15. Restore/publish emitted zero warnings. The Full/Mini deployments contain one native executable plus the unchanged 52/19 OpenCV package files, with system libraries supplied by each distribution. All six executions use no installed .NET or compilation tools, verify the absence of JIT/external CLR, and cover PNG, MJPG and the Full fixed ONNX/Mini importer-omission paths. These are three new runtime containers with separate volumes and a read-only publish kit; the AOT producer is a fourth independent container. The existing OpenCV graph-engine target warning is retained in Full runtime logs. See the [NativeAOT ADR](native-aot-trim-single-file-adr.md#opencv-510-local-linux-package-consumers) for toolchain, deployment and reproduction details. The active stable support matrix and hosted release gate retain their existing status.

The [Full/Mini headless candidate](headless-runtime-profile-adr.md#opencv-510-local-candidate-results) adds a separately rebuilt wrapper and isolated prerelease packages. Twelve new package-only net8/net10 executions pass on Ubuntu 22.04, Ubuntu 24.04 and Debian 12 with no display server, GUI libraries or producer paths. The original preview and AOT package bytes remain the basis of their original records. The headless record binds the separate candidate package hashes, generated ABI, GUI negative recovery, PNG/MJPG and Full ONNX results; active headless identity, hosted reproduction and camera/network coverage remain pending.
