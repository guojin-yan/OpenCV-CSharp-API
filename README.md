<p align="center">
  <img src="https://socialify.git.ci/guojin-yan/OpenCV-CSharp-API/image?description=1&descriptionEditable=OpenCV%205.0%20bindings%20for%20C%23%20and%20.NET&forks=1&issues=1&name=1&owner=1&pattern=Circuit%20Board&pulls=1&stargazers=1&theme=Light" alt="OpenCV CSharp API" width="100%" />
</p>

<h1 align="center">OpenCV CSharp API</h1>

<p align="center">
  Version-neutral OpenCV 5.0 bindings for C# and .NET, with managed APIs and verified native runtime packages.
</p>

<p align="center">
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-Apache%202.0-blue.svg" alt="Apache-2.0 License" /></a>
  <a href="https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/"><img src="https://img.shields.io/nuget/vpre/JYPPX.OpenCV.CSharp.API.svg" alt="NuGet prerelease version" /></a>
  <a href="https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/"><img src="https://img.shields.io/nuget/dt/JYPPX.OpenCV.CSharp.API.svg" alt="NuGet downloads" /></a>
  <a href="https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.CSharp.API"><img src="https://img.shields.io/badge/GitHub%20Packages-package%20feed-24292f" alt="GitHub Packages feed" /></a>
  <a href="https://github.com/guojin-yan/OpenCV-CSharp-API/releases"><img src="https://img.shields.io/github/v/release/guojin-yan/OpenCV-CSharp-API?include_prereleases&label=Release" alt="GitHub Release" /></a>
  <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-Framework%204.6--4.8.1%20%7C%20Core%203.1%20%7C%205--10-512BD4" alt="Supported .NET versions" /></a>
  <a href="https://opencv.org/"><img src="https://img.shields.io/badge/OpenCV-5.0.0-5C3EE8" alt="Upstream OpenCV 5.0.0" /></a>
</p>

<p align="center">
  <a href="https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-managed.yml"><img src="https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-managed.yml/badge.svg?branch=opencv5.x" alt="Managed CI" /></a>
  <a href="https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-native.yml"><img src="https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-native.yml/badge.svg?branch=opencv5.x" alt="Native CI" /></a>
</p>

<p align="center"><strong>English</strong> | <a href="README_cn.md">简体中文</a></p>

## Introduction

OpenCV CSharp API brings [OpenCV 5.0](https://opencv.org/) to C# through the familiar `JYPPX.OpenCvSharp.*` namespace family. It combines an idiomatic managed API, a stable native C ABI, explicit native-resource ownership, and runtime NuGet packages for verified Windows, Linux, and Android emulator targets.

The current public version is reported by the live NuGet badges below. The preview channel is intended for early adopters who need broad computer-vision coverage while keeping public API and ABI identities stable across later iterations.

## Release Highlights

- Broad coverage of Core, ImgProc, ImgCodecs, VideoIO, Calib3D, DNN, Features, ObjDetect, Photo, Video, HighGui, Stitching, ML, Tracking, and selected contrib APIs.
- One managed package plus full and mini native runtime profiles for 14 verified Windows, Linux, and Android emulator RIDs.
- .NET Framework 4.6 through 4.8.1, .NET Core 3.1, and .NET 5 through .NET 10.
- Deterministic packages on NuGet.org and GitHub Packages, SPDX 2.3 SBOMs, protected release approval, and verified GitHub Release assets.
- Headless, executable examples that generate inspectable PNG output without a camera, model download, or GUI session.
- Checked compatibility baselines covering 611 public managed types, 6,300 public/protected members, 41 namespaces under `JYPPX.OpenCvSharp`, and the declared native ABI.

## Get Started In 30 Seconds

### 1. Install The Packages

Install the managed API and exactly one runtime package at the same package version. This example selects the full Windows x64 runtime:

```powershell
dotnet add package JYPPX.OpenCV.CSharp.API --prerelease
dotnet add package JYPPX.OpenCV.runtime.win-x64 --prerelease # current Windows x64 example
```

### 2. Write C# Code

Use the namespace that owns each OpenCV module:

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;

using Mat left = new Mat(2, 3, MatType.CV_8UC1);
using Mat right = new Mat(2, 3, MatType.CV_8UC1);
using Mat result = new Mat();

left.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6 });
right.CopyFrom(new byte[] { 6, 5, 4, 3, 2, 1 });
CoreCv2.Add(left, right, result);

Console.WriteLine(string.Join(",", result.ToBytes()));
```

### 3. Run

```powershell
dotnet run
```

Expected output:

```text
7,7,7,7,7,7
```

The complete [Quick Start](docs/articles/quick-start.md) covers package selection, matrices, image codecs, geometry, and deterministic disposal.

## NuGet Packages

### Managed API

| Package | Version | NuGet.org | GitHub Packages | Purpose |
| --- | --- | --- | --- | --- |
| `JYPPX.OpenCV.CSharp.API` | [![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.CSharp.API.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/) | [NuGet Gallery](https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/) | [Package page](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.CSharp.API) | Version-neutral managed API for every supported .NET target |

### Distribution Channels

| Channel | Public entry point | Published content |
| --- | --- | --- |
| NuGet.org | [JYPPX.OpenCV.CSharp.API](https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/) | Managed API and all currently verified runtime packages |
| GitHub Packages | [JYPPX.OpenCV.CSharp.API](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.CSharp.API) | The exact reviewed managed and runtime package bytes |
| GitHub Releases | [OpenCV-CSharp-API Releases](https://github.com/guojin-yan/OpenCV-CSharp-API/releases) | Repository-signed packages, SBOMs, manifests, and verification reports |

Full runtimes use the version-neutral pattern `JYPPX.OpenCV.runtime.<rid>`; mini runtimes use `JYPPX.OpenCV.runtime.<rid>.mini`.

Choose the runtime package that matches your target RID and preferred profile.

The full profile guarantees its matrix-required modules, including DNN, calibration, features, Photo, Video, HighGui, and Stitching. ML, Tracking, and selected contrib modules are optional staged modules: their managed APIs remain stable, and unavailable native features report `NOT_LINKED`. The mini profile focuses on `core`, `imgproc`, `imgcodecs`, and `videoio`, with the required `geometry` and `flann` dependencies.

Do not reference full and mini runtime packages together. Keep the managed and runtime packages on the same normalized NuGet package version.

## Native Runtime Packages

The first preview publishes the managed package and every runtime package classified as `realSupport` in [`runtime-support-contract.json`](packaging/runtime/runtime-support-contract.json). Each package is published to both public registries from the same reviewed candidate; the formal assets and evidence are published on [GitHub Releases](https://github.com/guojin-yan/OpenCV-CSharp-API/releases).

| Platform | Architecture | Full runtime | Mini runtime |
| --- | --- | --- | --- |
| Windows 10/11 | x64 | `JYPPX.OpenCV.runtime.win-x64`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.win-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.win-x64) | `JYPPX.OpenCV.runtime.win-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.win-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.win-x64.mini) |
| Windows 11 | ARM64 | `JYPPX.OpenCV.runtime.win-arm64`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.win-arm64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-arm64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-arm64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.win-arm64) | `JYPPX.OpenCV.runtime.win-arm64.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.win-arm64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-arm64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-arm64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.win-arm64.mini) |
| Android 7.0+ emulator | x86_64 | `JYPPX.OpenCV.runtime.android-x64`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.android-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.android-x64) | `JYPPX.OpenCV.runtime.android-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.android-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.android-x64.mini) |
| Android 7.0+ emulator | x86 | `JYPPX.OpenCV.runtime.android-x86`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.android-x86.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x86/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x86/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.android-x86) | `JYPPX.OpenCV.runtime.android-x86.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.android-x86.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x86.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x86.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.android-x86.mini) |
| Ubuntu 22.04 | x64 | `JYPPX.OpenCV.runtime.ubuntu.22.04-x64`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.ubuntu.22.04-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.22.04-x64) | `JYPPX.OpenCV.runtime.ubuntu.22.04-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.ubuntu.22.04-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.22.04-x64.mini) |
| Ubuntu 22.04 | ARM64 | `JYPPX.OpenCV.runtime.ubuntu.22.04-arm64`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64) | `JYPPX.OpenCV.runtime.ubuntu.22.04-arm64.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64.mini) |
| Ubuntu 24.04 | x64 | `JYPPX.OpenCV.runtime.ubuntu.24.04-x64`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.ubuntu.24.04-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.24.04-x64) | `JYPPX.OpenCV.runtime.ubuntu.24.04-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.ubuntu.24.04-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.24.04-x64.mini) |
| Ubuntu 24.04 | ARM64 | `JYPPX.OpenCV.runtime.ubuntu.24.04-arm64`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64) | `JYPPX.OpenCV.runtime.ubuntu.24.04-arm64.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64.mini) |
| Debian 12 | x64 | `JYPPX.OpenCV.runtime.debian.12-x64`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.debian.12-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.debian.12-x64) | `JYPPX.OpenCV.runtime.debian.12-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.debian.12-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.debian.12-x64.mini) |
| Debian 12 | ARM64 | `JYPPX.OpenCV.runtime.debian.12-arm64`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.debian.12-arm64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-arm64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-arm64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.debian.12-arm64) | `JYPPX.OpenCV.runtime.debian.12-arm64.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.debian.12-arm64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-arm64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-arm64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.debian.12-arm64.mini) |
| Fedora 40 | x64 | `JYPPX.OpenCV.runtime.fedora.40-x64`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.fedora.40-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.fedora.40-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.fedora.40-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.fedora.40-x64) | `JYPPX.OpenCV.runtime.fedora.40-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.fedora.40-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.fedora.40-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.fedora.40-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.fedora.40-x64.mini) |
| RHEL 9 / UBI 9 | x64 | `JYPPX.OpenCV.runtime.rhel.9-x64`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.rhel.9-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rhel.9-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rhel.9-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.rhel.9-x64) | `JYPPX.OpenCV.runtime.rhel.9-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.rhel.9-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rhel.9-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rhel.9-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.rhel.9-x64.mini) |
| Rocky Linux 9 | x64 | `JYPPX.OpenCV.runtime.rocky.9-x64`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.rocky.9-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rocky.9-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rocky.9-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.rocky.9-x64) | `JYPPX.OpenCV.runtime.rocky.9-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.rocky.9-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rocky.9-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rocky.9-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.rocky.9-x64.mini) |
| Alpine 3.20 | x64 / musl | `JYPPX.OpenCV.runtime.alpine.3.20-x64`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.alpine.3.20-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.alpine.3.20-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.alpine.3.20-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.alpine.3.20-x64) | `JYPPX.OpenCV.runtime.alpine.3.20-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/vpre/JYPPX.OpenCV.runtime.alpine.3.20-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.alpine.3.20-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.alpine.3.20-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.alpine.3.20-x64.mini) |

Linux packages use a distro-specific Linux RID rather than a generic `linux-x64` identity. If the .NET SDK does not recognize a project-defined RID, set `RuntimeIdentifierGraphPath` to [`packaging/runtime/runtime-distro-rid-graph.json`](packaging/runtime/runtime-distro-rid-graph.json) or copy that graph into the consuming project before restore.

Android x64/x86 Full and Mini are real-supported after authoritative single-loader NDK builds, package/APK audits, and emulator-native `Mat` plus `Cv2.Sum` loading; the retired dual-loader records remain under `superseded` in [`android-runtime-evidence.json`](packaging/runtime/android-runtime-evidence.json). Android ARM/ARM64 remain `android-evidence-pending`: their hosted production and package evidence has passed, but ABI-matched physical-device loading is still required before promotion. `win-x86/full` remains `hosted-evidence-pending`, `win-x86/mini` is excluded, and macOS is outside the declared runtime package matrix. Package-matrix presence is not a production-support claim.

If there is no matching runtime package, build a local native runtime with `scripts/Build-OpenCV.ps1`, stage it with `scripts/Stage-Runtime.ps1 -OpenCvNativeRuntimeDir <path>`, and point local samples or tests at it with `OpenCvNativeRuntimeDir`. The corresponding package fallback is `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>`. See the [Linked Runtime Build Guide](docs/articles/linked-runtime-build-guide.md) for the supported local native runtime workflow.

## Choose Full Or Mini

| Capability | Full | Mini |
| --- | :---: | :---: |
| Core arrays, matrices, persistence | Yes | Yes |
| ImgProc, ImgCodecs, VideoIO | Yes | Yes |
| Geometry and FLANN runtime dependencies | Yes | Yes |
| DNN, object detection, calibration, features | Yes | No |
| Photo, Video, HighGui, Stitching | Yes | No |
| ML, Tracking, selected contrib modules | Runtime-dependent | No |
| Stable response for an unavailable module | `NOT_LINKED` | `NOT_LINKED` |

## Tutorial Series And Visual Results

The [`samples/ConsoleSamples`](samples/ConsoleSamples) project includes broad smoke coverage and a six-part, headless tutorial series. Set a CJK font path to run the complete series, including OpenCV `putText` with Chinese:

```powershell
$env:OPENCV_CSHARP_CJK_FONT = "C:\path\to\a-cjk-font.ttf"
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release `
  -p:OpenCvNativeRuntimeDir=E:\path\to\full-runtime `
  -- tutorial all .\artifacts\tutorials
```

The command writes image processing, native OpenCV Chinese text, contour, ORB feature, template matching, and KNN classification results:

![OpenCV CSharp API visual showcase](docs/images/showcase/showcase-overview.png)

Start with the [Tutorial Series](docs/articles/tutorial-series.md). Each output has a matching technical article, runnable command, focused code, runtime profile, and links into the deeper module guides. The earlier `showcase` command remains a compatibility alias.

## Documentation

| Resource | Description |
| --- | --- |
| [Documentation site](https://guojin-yan.github.io/OpenCV-CSharp-API/) | API reference and articles |
| [Quick Start](docs/articles/quick-start.md) | Install and write the first program |
| [Tutorial Series](docs/articles/tutorial-series.md) | Six executable tutorials with synchronized technical articles |
| [OpenCV PutText With Chinese](docs/articles/tutorial-02-chinese-puttext.md) | Render UTF-8 Chinese directly into `Mat` through OpenCV 5 `putText` |
| [Visual Showcase](docs/articles/visual-showcase.md) | Output gallery and compatibility commands |
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

## Acknowledgments

This project builds on [OpenCV](https://opencv.org/) and its contributors. OpenCV remains the authoritative source for algorithm behavior and native runtime licensing.

## License

The managed API, native wrapper, and OpenCV runtime are licensed under the [Apache License 2.0](LICENSE), so managed and runtime NuGet packages use the SPDX expression `Apache-2.0`. Packaged third-party notices remain authoritative for their respective components.

## Support And Contact

- [GitHub Issues](https://github.com/guojin-yan/OpenCV-CSharp-API/issues) for bugs and feature requests.
- [GitHub Discussions](https://github.com/guojin-yan/OpenCV-CSharp-API/discussions) for usage questions.
- QQ group `945057948` for community discussion.

This is a preview release. Test representative workflows before using it in production, industrial, or mission-critical systems.

## Software Notice

This project uses AI-assisted development and has not been exhaustively tested across every device, workload, and edge case. The source contains no intentionally introduced malicious functionality. Review the license and third-party notices, validate the library against your own requirements, and perform rigorous testing before production or safety-critical use.

<details>
<summary>Maintainer build and compatibility boundaries</summary>

The following compact notes preserve the repository's audited build contracts. Most users should follow the linked guides instead.

- The native CMake project is currently source-tree build only and does not currently install or export a reusable CMake package or SDK target. The only native target and loader identity is `JYPPX.OpenCV.Native`.
- Runtime NuGet packages do not currently distribute native C headers. The source-tree header surface is `src/OpenCvSharp.Native/include/open_cv_sharp`.
- Native CTest and local build output names are version-neutral: `JYPPX.OpenCV.NativeSmoke` and `JYPPX.OpenCV.NativeAbiExportAudit`.
- Native CMake runtime-root/PATH copy is neutral-first. `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT` may put that target output directory first in CTest `PATH`; the copied `opencv*.dll` names remain factual upstream artifacts.
- Local sample and test builds use `OpenCvNativeRuntimeDir`. To build a missing runtime locally, point local samples/tests at that property.
- The public managed build-info surface is `OpenCvSharpBuildInfo`; native interop imports use `JYPPX.OpenCV.Native` and `jyppx_ocv_*` entry points.
- `runtime-input.yml` produces factual `runtime-input-<rid>-<profile>` artifacts containing `native-wrapper/`, `opencv-runtime/`, and `opencv-source/`. Named examples include `runtime-input-win-x64-full`, `runtime-input-ubuntu.24.04-x64-full`, `runtime-input-ubuntu.24.04-x64-mini`, `runtime-input-ubuntu.24.04-arm64-full`, `runtime-input-ubuntu.22.04-x64-full`, `runtime-input-ubuntu.22.04-x64-mini`, `runtime-input-ubuntu.22.04-arm64-full`, `runtime-input-debian.12-x64-full`, `runtime-input-debian.12-arm64-full`, `runtime-input-fedora.40-x64-full`, `runtime-input-rhel.9-x64-full`, `runtime-input-rocky.9-x64-full`, and `runtime-input-alpine.3.20-x64-full`.
- Windows runtime evidence distinguishes `CMAKE_ASM_COMPILER=NOTFOUND`, `OPENCV_DNN_MLAS_ENABLED=0`, factual `opencv_<module>500.dll` files, the single-loader 17 AMD64 DLL full payload, and Linux SONAME layouts.
- The pack workflow operates over the active multi-RID runtime package matrix and its full/mini profiles. Synthetic jobs validate package shape, while publishable jobs require factual inputs and consumer restore verification. All normalized nupkg outputs remain neutral workflow artifacts rooted under `artifacts/packages`.
- `pack.yml` does not build real runtime inputs. Real input paths must already exist on the selected runner or come from `real_runtime_artifact_run_id`. Synthetic runtime inputs are package-surface validation only; real publishable runtime packages require `SyntheticRuntimeInputs=false`, non-synthetic provenance, and release preflight.
- Before packing, pass the exact release version with `-PackageVersion` and the factual staged runtime with `-OpenCvNativeRuntimeDir`. Public package IDs stay version-neutral; normalized `.nupkg` files are written beneath `artifacts\packages`.
- Full payload verification uses matrix-required modules plus provenance-recorded staged optional modules. Hosted native verification covers Ubuntu 24.04 x64 full/mini and Ubuntu 22.04 x64 full/mini.
- Ubuntu 24.04 ARM64 full runs natively on `ubuntu-24.04-arm`. Ubuntu 22.04 ARM64 full runs through a separate host-orchestrated `docker run` verifier. Debian 12 ARM64 full uses its own host-orchestrated `docker run` verifier on native `ubuntu-24.04-arm`.
- Debian 12 x64 full runs in a separate `debian:12` job container. Fedora 40 full runs in its own separate `fedora:40` job container. Rocky Linux 9 full runs in a fourth separate `rockylinux:9` job container. RHEL 9 full runs in a fifth separate official Red Hat UBI 9 job container. Alpine 3.20 full runs through a separate host-orchestrated `docker run alpine:3.20` verifier.

</details>

Copyright (c) 2026 Guojin Yan.
