![OpenCV CSharp API](https://socialify.git.ci/guojin-yan/OpenCV-CSharp-API/image?description=1&descriptionEditable=OpenCV%205.0%20bindings%20for%20C%23%20and%20.NET&forks=1&issues=1&name=1&owner=1&pattern=Circuit%20Board&pulls=1&stargazers=1&theme=Light)

# OpenCV CSharp API

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/JYPPX.OpenCV.CSharp.API.svg)](https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/)
[![Downloads](https://img.shields.io/nuget/dt/JYPPX.OpenCV.CSharp.API.svg)](https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.6--4.8.1%20%7C%20Core%203.1%20%7C%205--10-512BD4)](https://dotnet.microsoft.com/)
[![Upstream OpenCV](https://img.shields.io/badge/OpenCV-5.0.0-5C3EE8)](https://opencv.org/)
[![Managed CI](https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-managed.yml/badge.svg?branch=opencv5.x)](https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-managed.yml)
[![Native CI](https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-native.yml/badge.svg?branch=opencv5.x)](https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-native.yml)

English | [简体中文](README_cn.md)

OpenCV CSharp API is a version-neutral .NET binding for OpenCV 5.0. It combines an idiomatic managed API under `OpenCvSharp.*` with a stable native C ABI, deterministic NuGet packages, explicit native ownership, and real runtime packages for supported Windows and Linux targets.

The first public channel is `5.0.0-preview.1`. It is designed for early adopters who need broad OpenCV coverage while preserving API and ABI identity across later iterations.

## Highlights

- 612 public managed types, 6,314 public/protected members, and 41 namespaces under a checked compatibility baseline.
- 2,656 full-profile and 526 mini-profile native ABI functions with complete declared native-to-managed binding coverage.
- Practical coverage across Core, ImgProc, ImgCodecs, VideoIO, Calib3D, DNN, Features, HighGui, ObjDetect, Photo, Video, ML, Tracking, Stitching, and selected contrib modules.
- Targets .NET Framework 4.6 through 4.8.1, .NET Core 3.1, and .NET 5 through .NET 10.
- Full and mini runtime profiles with package-owned native smoke tests and explicit `NOT_LINKED` behavior for unavailable mini features.
- Deterministic packages, SPDX 2.3 SBOMs, protected publication approval, and post-upload NuGet.org Repository-signature verification.
- Headless visual samples that generate inspection-ready PNG files without a camera, model download, or GUI session.

![OpenCV CSharp API visual showcase](docs/images/showcase/showcase-overview.png)

## Quick Start

Install the managed package and exactly one runtime package on the same normalized NuGet package version. The example below uses the full Windows x64 runtime:

```powershell
dotnet add package JYPPX.OpenCV.CSharp.API --version 5.0.0-preview.1
dotnet add package JYPPX.OpenCV.runtime.win-x64 --version 5.0.0-preview.1 # current Windows x64 example
```

Then use the module namespace that owns the API:

```csharp
using System;
using OpenCvSharp.Core;
using CoreCv2 = OpenCvSharp.Core.Cv2;

using Mat left = new Mat(2, 3, MatType.CV_8UC1);
using Mat right = new Mat(2, 3, MatType.CV_8UC1);
using Mat result = new Mat();

left.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6 });
right.CopyFrom(new byte[] { 6, 5, 4, 3, 2, 1 });
CoreCv2.Add(left, right, result);

Console.WriteLine(string.Join(",", result.ToBytes()));
```

The complete [Quick Start](docs/articles/quick-start.md) covers package selection, matrices, image codecs, geometry, and disposal.

## NuGet Packages

### Managed API

| Package | Purpose |
| --- | --- |
| [`JYPPX.OpenCV.CSharp.API`](https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/) | Version-neutral managed API for all supported frameworks |

### Runtime Packages

Choose one package matching your target RID and profile:

- Full: `JYPPX.OpenCV.runtime.<rid>`
- Mini: `JYPPX.OpenCV.runtime.<rid>.mini`

The full profile contains broad OpenCV modules including DNN, calibration, features, Photo, HighGui, ML, Tracking, and Stitching. The mini profile focuses on `core`, `imgproc`, `imgcodecs`, and `videoio`, with the required `geometry` and `flann` runtime dependencies.

Do not reference full and mini runtime packages together. Keep the managed and runtime packages on the same normalized NuGet package version.

## Supported Runtime Targets

The first preview publishes the managed package and the 24 runtime targets classified as `realSupport` in [`runtime-support-contract.json`](packaging/runtime/runtime-support-contract.json).

| Platform | RID | Architectures | Profiles |
| --- | --- | --- | --- |
| Windows 10/11 | `win-x64` | x64 | full, mini |
| Windows 11 | `win-arm64` | ARM64 | full, mini |
| Ubuntu 22.04 | `ubuntu.22.04-x64`, `ubuntu.22.04-arm64` | x64, ARM64 | full, mini |
| Ubuntu 24.04 | `ubuntu.24.04-x64`, `ubuntu.24.04-arm64` | x64, ARM64 | full, mini |
| Debian 12 | `debian.12-x64`, `debian.12-arm64` | x64, ARM64 | full, mini |
| Fedora 40 | `fedora.40-x64` | x64 | full, mini |
| RHEL 9 / UBI 9 | `rhel.9-x64` | x64 | full, mini |
| Rocky Linux 9 | `rocky.9-x64` | x64 | full, mini |
| Alpine 3.20 | `alpine.3.20-x64` | x64 / musl | full, mini |

Linux packages use a distro-specific Linux RID rather than a generic `linux-x64` identity. If the .NET SDK does not recognize a project-defined RID, set `RuntimeIdentifierGraphPath` to [`packaging/runtime/runtime-distro-rid-graph.json`](packaging/runtime/runtime-distro-rid-graph.json) or copy that graph into the consuming project before restore.

`win-x86/full` remains `hosted-evidence-pending`. `win-x86/mini` and Android profiles are excluded from the first preview because they do not have complete real producer and consumer evidence. macOS is outside the declared runtime package matrix. Package-matrix presence is not a production-support claim.

If there is no matching runtime package, build a local native runtime with `scripts/Build-OpenCV.ps1`, stage it with `scripts/Stage-Runtime.ps1 -OpenCvNativeRuntimeDir <path>`, and point local samples or tests at it with `OpenCvNativeRuntimeDir`. The corresponding package fallback is `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>`. See the [Linked Runtime Build Guide](docs/articles/linked-runtime-build-guide.md) for the supported local native runtime workflow.

## Full And Mini Profiles

| Capability | Full | Mini |
| --- | :---: | :---: |
| Core arrays, matrices, persistence | Yes | Yes |
| ImgProc, ImgCodecs, VideoIO | Yes | Yes |
| Geometry and FLANN runtime dependencies | Yes | Yes |
| DNN and object detection | Yes | No |
| Calibration and feature matching | Yes | No |
| Photo, Video, ML, Tracking, Stitching | Yes | No |
| HighGui | Yes | No |
| Stable missing-feature response | N/A | `NOT_LINKED` |

## Samples

The [`samples/ConsoleSamples`](samples/ConsoleSamples) project includes broad smoke coverage and a visual showcase:

```powershell
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release `
  -p:OpenCvNativeRuntimeDir=E:\path\to\full-runtime `
  -- showcase all .\artifacts\showcase
```

The showcase writes image processing, ORB feature, template matching, and KNN classification results. See [Visual Showcase](docs/articles/visual-showcase.md) and [Scenario Recipes](docs/articles/scenario-recipes.md).

## Documentation

| Resource | Description |
| --- | --- |
| [Documentation site](https://guojin-yan.github.io/OpenCV-CSharp-API/) | API reference and articles |
| [Quick Start](docs/articles/quick-start.md) | Install and write the first program |
| [Visual Showcase](docs/articles/visual-showcase.md) | Executable image, feature, template, and ML examples |
| [Scenario Recipes](docs/articles/scenario-recipes.md) | Task-oriented workflows |
| [Linked Runtime Build Guide](docs/articles/linked-runtime-build-guide.md) | Build and stage a factual native runtime |
| [Linked Runtime Smoke Guide](docs/articles/linked-runtime-smoke-guide.md) | Validate native loading and package execution |
| [Smoke Profiles Guide](docs/articles/smoke-profiles-guide.md) | Select full or mini verification |
| [Runtime Licenses](docs/articles/runtime-licenses.md) | Runtime license and third-party notices |
| [Runtime package README](packaging/runtime/JYPPX.OpenCV.runtime/README.md) | Package layout and provenance |
| [API/ABI Compatibility Policy](docs/articles/api-abi-compatibility-policy.md) | Compatibility and gap accounting |
| [Support and Lifecycle Policy](docs/articles/support-lifecycle-policy.md) | Real support, pending, and excluded targets |
| [NuGet Repository Signing Guide](docs/articles/nuget-repository-signing-guide.md) | Publication trust and verification |

## Build From Source

Requirements:

- .NET SDK 10.0.302 for the repository's exact validation path.
- CMake and a supported C/C++ toolchain for native builds.
- OpenCV 5.0.0 source or an installed OpenCV 5.0.0 runtime for linked native builds.

```powershell
git clone https://github.com/guojin-yan/OpenCV-CSharp-API.git
cd OpenCV-CSharp-API
git switch opencv5.x

dotnet restore .\OpenCV-CSharp-API.slnx
dotnet build .\OpenCV-CSharp-API.slnx -c Release --no-restore
```

Native and package instructions are intentionally kept in the [Linked Runtime Build Guide](docs/articles/linked-runtime-build-guide.md). Synthetic runtime inputs validate package shape only and must never be published.

## Project Structure

```text
OpenCV-CSharp-API/
|-- src/OpenCvSharp/                    Managed API
|-- src/OpenCvSharp.Native/             Stable native C ABI
|-- samples/ConsoleSamples/             Smoke and visual examples
|-- packaging/runtime/                  RID/profile runtime package template
|-- compatibility/                      API, ABI, and upstream maps
|-- docs/                               DocFX configuration and articles
|-- scripts/                            Build, pack, verification, and release guards
`-- .github/workflows/                  CI, runtime production, pack, and publication
```

## Contributing

Issues and pull requests are welcome. Before changing public API, native ABI, package identity, ownership, or runtime behavior, read [CONTRIBUTING.md](CONTRIBUTING.md) and the [API/ABI Compatibility Policy](docs/articles/api-abi-compatibility-policy.md).

Please include focused tests, preserve version-neutral public identities, and document any unsupported upstream behavior explicitly.

## License

The managed API and project code are licensed under the [MIT License](LICENSE). Runtime packages combine project code with OpenCV runtime files and use the package license expression `MIT AND Apache-2.0`; packaged third-party notices remain authoritative for their respective components.

## Support

- [GitHub Issues](https://github.com/guojin-yan/OpenCV-CSharp-API/issues) for bugs and feature requests.
- [GitHub Discussions](https://github.com/guojin-yan/OpenCV-CSharp-API/discussions) for usage questions.
- QQ group `945057948` for community discussion.

This is a preview release. Test representative workflows before using it in production, industrial, or mission-critical systems.

<details>
<summary>Maintainer build and compatibility boundaries</summary>

The following compact notes preserve the repository's audited build contracts. Most users should follow the linked guides instead.

- The native CMake project is currently source-tree build only and does not currently install or export a reusable CMake package or SDK target. The `JYPPX.OpenCV.Native` CMake target is primary; `OpenCv5Sharp.Native` remains only a compatibility alias.
- Runtime NuGet packages do not currently distribute native C headers. The primary header tree is `src/OpenCvSharp.Native/include/open_cv_sharp`; `src/OpenCvSharp.Native/include/open_cv_5_sharp` is a compatibility tree.
- Native CTest and local build output names are neutral-first: `JYPPX.OpenCV.NativeSmoke` and `JYPPX.OpenCV.NativeCompatibilitySourceSmoke`. The `OpenCv5Sharp.Native` loader file remains only a compatibility copy.
- Native CMake runtime-root/PATH copy is neutral-first. `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT` may put that target output directory first in CTest `PATH`; the copied `opencv*.dll` names remain factual upstream artifacts.
- Local sample and test builds prefer `OpenCvNativeRuntimeDir`; `OpenCv5SharpNativeRuntimeDir` remains a compatibility alias. To build a missing runtime locally, point local samples/tests at it with `OpenCvNativeRuntimeDir`.
- `OpenCv5SharpBuildInfo` remains a documented and tested compatibility facade; new public API continues to use version-neutral identities.
- `runtime-input.yml` produces factual `runtime-input-<rid>-<profile>` artifacts containing `native-wrapper/`, `opencv-runtime/`, and `opencv-source/`. Named examples include `runtime-input-win-x64-full`, `runtime-input-ubuntu.24.04-x64-full`, `runtime-input-ubuntu.24.04-x64-mini`, `runtime-input-ubuntu.24.04-arm64-full`, `runtime-input-ubuntu.22.04-x64-full`, `runtime-input-ubuntu.22.04-x64-mini`, `runtime-input-ubuntu.22.04-arm64-full`, `runtime-input-debian.12-x64-full`, `runtime-input-debian.12-arm64-full`, `runtime-input-fedora.40-x64-full`, `runtime-input-rhel.9-x64-full`, `runtime-input-rocky.9-x64-full`, and `runtime-input-alpine.3.20-x64-full`.
- Windows runtime evidence distinguishes `CMAKE_ASM_COMPILER=NOTFOUND`, `OPENCV_DNN_MLAS_ENABLED=0`, factual `opencv_<module>500.dll` files, the 18 AMD64 DLL full payload, and Linux SONAME layouts.
- The pack workflow operates over the active multi-RID runtime package matrix and its full/mini profiles. Synthetic jobs validate package shape, while publishable jobs require factual inputs and consumer restore verification. All normalized nupkg outputs remain neutral workflow artifacts rooted under `artifacts/packages`.
- `pack.yml` does not build real runtime inputs. Real input paths must already exist on the selected runner or come from `real_runtime_artifact_run_id`. Synthetic runtime inputs are package-surface validation only; real publishable runtime packages require `SyntheticRuntimeInputs=false`, non-synthetic provenance, and release preflight.
- Before packing, pass the exact release version with `-PackageVersion` and the factual staged runtime with `-OpenCvNativeRuntimeDir`. Public package IDs stay version-neutral; normalized `.nupkg` files are written beneath `artifacts\packages`.
- Full payload verification uses matrix-required modules plus provenance-recorded staged optional modules. Hosted native verification covers Ubuntu 24.04 x64 full/mini and Ubuntu 22.04 x64 full/mini.
- Ubuntu 24.04 ARM64 full runs natively on `ubuntu-24.04-arm`. Ubuntu 22.04 ARM64 full runs through a separate host-orchestrated `docker run` verifier. Debian 12 ARM64 full uses its own host-orchestrated `docker run` verifier on native `ubuntu-24.04-arm`.
- Debian 12 x64 full runs in a separate `debian:12` job container. Fedora 40 full runs in its own separate `fedora:40` job container. Rocky Linux 9 full runs in a fourth separate `rockylinux:9` job container. RHEL 9 full runs in a fifth separate official Red Hat UBI 9 job container. Alpine 3.20 full runs through a separate host-orchestrated `docker run alpine:3.20` verifier.

</details>

Copyright (c) 2026 Guojin Yan.
