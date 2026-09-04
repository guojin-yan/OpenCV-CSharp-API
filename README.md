<picture>
  <source media="(prefers-color-scheme: dark)" srcset="docs/images/readme/hero-dark.svg">
  <source media="(prefers-color-scheme: light)" srcset="docs/images/readme/hero-light.svg">
  <img alt="OpenCV CSharp API - OpenCV 5 bindings for C# and .NET" src="docs/images/readme/hero-light.svg" width="100%">
</picture>

<h1 align="center">📷 OpenCV CSharp API</h1>

<p align="center">
  Version-neutral OpenCV 5.0 bindings for C# and .NET, with managed APIs and verified native runtime packages.
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/"><img src="https://img.shields.io/nuget/vpre/JYPPX.OpenCV.CSharp.API.svg?label=nuget" alt="Latest NuGet version" /></a>
  <a href="https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/"><img src="https://img.shields.io/nuget/dt/JYPPX.OpenCV.CSharp.API.svg" alt="NuGet downloads" /></a>
  <a href="https://github.com/guojin-yan/OpenCV-CSharp-API/releases"><img src="https://img.shields.io/github/v/release/guojin-yan/OpenCV-CSharp-API?include_prereleases&amp;label=release" alt="Latest GitHub release" /></a>
  <a href="https://github.com/guojin-yan/OpenCV-CSharp-API/stargazers"><img src="https://img.shields.io/github/stars/guojin-yan/OpenCV-CSharp-API?style=flat&amp;label=stars" alt="GitHub stars" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-Apache%202.0-blue.svg" alt="Apache-2.0 license" /></a>
</p>

<p align="center">
  <a href="https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-managed.yml"><img src="https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-managed.yml/badge.svg?branch=opencv5.x" alt="Managed CI" /></a>
  <a href="https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-native.yml"><img src="https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-native.yml/badge.svg?branch=opencv5.x" alt="Native CI" /></a>
</p>

<p align="center"><strong>English</strong> | <a href="README_cn.md">简体中文</a></p>

## 📖 Introduction

OpenCV CSharp API brings [OpenCV 5.0](https://opencv.org/) to C# through the familiar `JYPPX.OpenCvSharp.*` namespace family. It combines an idiomatic managed API, a stable native C ABI, explicit native-resource ownership, and runtime NuGet packages for verified Windows, Linux, and Android emulator targets.

The current public version is reported by the live NuGet badges below. Stable releases preserve the declared managed API and native ABI under semantic versioning while continuing to add compatible computer-vision capabilities.

## ✨ Release Highlights

- Broad coverage of Core, ImgProc, ImgCodecs, VideoIO, Calib3D, DNN, Features, ObjDetect, Photo, Video, HighGui, Stitching, ML, Tracking, and selected contrib APIs.
- One managed package plus full and mini native runtime profiles for 14 verified Windows, Linux, and Android emulator RIDs.
- .NET Framework 4.6 through 4.8.1, .NET Core 3.1, and .NET 5 through .NET 10.
- Deterministic packages on NuGet.org and GitHub Packages, SPDX 2.3 SBOMs, protected release approval, and verified GitHub Release assets.
- Managed codec preflight identifies common image headers and applies explicit input, dimension, pixel, and frame budgets before native decoding.
- Twenty-three grouped, headless workflows with inspectable PNG output; model-backed DNN cases use one-time, hash-verified asset bundles while the remaining cases stay fully offline.
- Checked compatibility baselines covering 644 public managed types, 6,963 public/protected members, 41 namespaces under `JYPPX.OpenCvSharp`, and the declared native ABI.

## 📢 What's New In 5.0.0

- Added package-version diagnostics, broader color conversions, safe non-contiguous `Mat` row/stride access, typed pixel vectors, and type-safe image encoder parameters.
- Added `VideoCapture.TryRead`/`TryRetrieve` and OpenCV-backed standard, batched, rotated-box, and Soft-NMS detection postprocessing.
- Expanded the verified runtime/package pipeline, grouped examples, tutorials, and repository-signing checks for NuGet.org, GitHub Packages, and GitHub Releases.

See the [detailed 5.0.0 notes](docs/releases/5.0.0.md) or browse the complete [changelog](CHANGELOG.md). Public availability is confirmed by the live NuGet badges, GitHub Packages pages, and the matching GitHub Release.

## 🚀 Get Started In 30 Seconds

### 1. Install The Packages

Install the managed API and exactly one runtime package at the same package version. This example selects the full Windows x64 runtime:

```powershell
dotnet add package JYPPX.OpenCV.CSharp.API
dotnet add package JYPPX.OpenCV.runtime.win-x64 # current Windows x64 example
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

## 📦 NuGet Packages

### Managed API

| Package | Version | NuGet.org | GitHub Packages | Purpose |
| --- | --- | --- | --- | --- |
| `JYPPX.OpenCV.CSharp.API` | [![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.CSharp.API.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/) | [NuGet Gallery](https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/) | [Package page](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.CSharp.API) | Version-neutral managed API for every supported .NET target |

### Distribution Channels

| Channel | Public entry point | Published content |
| --- | --- | --- |
| NuGet.org | [JYPPX.OpenCV.CSharp.API](https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/) | Managed API and all currently verified runtime packages |
| GitHub Packages | [JYPPX.OpenCV.CSharp.API](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.CSharp.API) | The exact reviewed managed and runtime package bytes |
| GitHub Releases | [OpenCV-CSharp-API Releases](https://github.com/guojin-yan/OpenCV-CSharp-API/releases) | Repository-signed packages, SBOMs, manifests, and verification reports |

Full runtimes use the version-neutral pattern `JYPPX.OpenCV.runtime.<rid>`; mini runtimes use `JYPPX.OpenCV.runtime.<rid>.mini`.

Choose the runtime package that matches your target RID and preferred profile.

The full profile guarantees its matrix-required modules, including DNN, ML, calibration, features, Photo, Video, HighGui, and Stitching. Tracking and additional contrib modules are optional staged modules: their managed APIs remain stable, and unavailable native features report `NOT_LINKED`. The mini profile focuses on `core`, `imgproc`, `imgcodecs`, and `videoio`, with the required `geometry` and `flann` dependencies. Packages published before the corrected Full/ML boundary can report `NOT_LINKED` for ML; use the latest Full runtime for ML workloads.

Do not reference full and mini runtime packages together. Keep the managed and runtime packages on the same normalized NuGet package version.

For startup diagnostics, `OpenCvSharpBuildInfo.NuGetPackageVersion` reports the exact normalized package version (including `preview.N`), while `OpenCvSharpBuildInfo.NativeAbiVersion` and `GetLoadedNativeAbiVersion()` identify the managed/native ABI pair. Call `VerifyNativeRuntimeCompatibility()` after the runtime is loaded to fail fast when the managed package, native ABI, or OpenCV runtime version do not match.

## 🧩 Native Runtime Packages

Each release publishes the managed package and every runtime package classified as `realSupport` in [`runtime-support-contract.json`](packaging/runtime/runtime-support-contract.json). Every package is promoted to both public registries from the same reviewed candidate; the formal assets and verification evidence are published on [GitHub Releases](https://github.com/guojin-yan/OpenCV-CSharp-API/releases).

| Platform | Architecture | Full runtime | Mini runtime |
| --- | --- | --- | --- |
| Windows 10/11 | x64 | `JYPPX.OpenCV.runtime.win-x64`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.win-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.win-x64) | `JYPPX.OpenCV.runtime.win-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.win-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.win-x64.mini) |
| Windows 10/11 (WoW64) | x86 | `JYPPX.OpenCV.runtime.win-x86`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.win-x86.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-x86/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-x86/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.win-x86) | Excluded (`win-x86/mini`) |
| Windows 11 | ARM64 | `JYPPX.OpenCV.runtime.win-arm64`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.win-arm64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-arm64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-arm64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.win-arm64) | `JYPPX.OpenCV.runtime.win-arm64.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.win-arm64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-arm64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.win-arm64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.win-arm64.mini) |
| Android 7.0+ emulator | x86_64 | `JYPPX.OpenCV.runtime.android-x64`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.android-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.android-x64) | `JYPPX.OpenCV.runtime.android-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.android-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.android-x64.mini) |
| Android 7.0+ emulator | x86 | `JYPPX.OpenCV.runtime.android-x86`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.android-x86.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x86/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x86/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.android-x86) | `JYPPX.OpenCV.runtime.android-x86.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.android-x86.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x86.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.android-x86.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.android-x86.mini) |
| Ubuntu 22.04 | x64 | `JYPPX.OpenCV.runtime.ubuntu.22.04-x64`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.ubuntu.22.04-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.22.04-x64) | `JYPPX.OpenCV.runtime.ubuntu.22.04-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.ubuntu.22.04-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.22.04-x64.mini) |
| Ubuntu 22.04 | ARM64 | `JYPPX.OpenCV.runtime.ubuntu.22.04-arm64`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64) | `JYPPX.OpenCV.runtime.ubuntu.22.04-arm64.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.22.04-arm64.mini) |
| Ubuntu 24.04 | x64 | `JYPPX.OpenCV.runtime.ubuntu.24.04-x64`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.ubuntu.24.04-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.24.04-x64) | `JYPPX.OpenCV.runtime.ubuntu.24.04-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.ubuntu.24.04-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.24.04-x64.mini) |
| Ubuntu 24.04 | ARM64 | `JYPPX.OpenCV.runtime.ubuntu.24.04-arm64`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64) | `JYPPX.OpenCV.runtime.ubuntu.24.04-arm64.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.ubuntu.24.04-arm64.mini) |
| Debian 12 | x64 | `JYPPX.OpenCV.runtime.debian.12-x64`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.debian.12-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.debian.12-x64) | `JYPPX.OpenCV.runtime.debian.12-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.debian.12-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.debian.12-x64.mini) |
| Debian 12 | ARM64 | `JYPPX.OpenCV.runtime.debian.12-arm64`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.debian.12-arm64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-arm64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-arm64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.debian.12-arm64) | `JYPPX.OpenCV.runtime.debian.12-arm64.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.debian.12-arm64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-arm64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.debian.12-arm64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.debian.12-arm64.mini) |
| Fedora 40 | x64 | `JYPPX.OpenCV.runtime.fedora.40-x64`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.fedora.40-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.fedora.40-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.fedora.40-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.fedora.40-x64) | `JYPPX.OpenCV.runtime.fedora.40-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.fedora.40-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.fedora.40-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.fedora.40-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.fedora.40-x64.mini) |
| RHEL 9 / UBI 9 | x64 | `JYPPX.OpenCV.runtime.rhel.9-x64`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.rhel.9-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rhel.9-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rhel.9-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.rhel.9-x64) | `JYPPX.OpenCV.runtime.rhel.9-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.rhel.9-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rhel.9-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rhel.9-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.rhel.9-x64.mini) |
| Rocky Linux 9 | x64 | `JYPPX.OpenCV.runtime.rocky.9-x64`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.rocky.9-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rocky.9-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rocky.9-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.rocky.9-x64) | `JYPPX.OpenCV.runtime.rocky.9-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.rocky.9-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rocky.9-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.rocky.9-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.rocky.9-x64.mini) |
| Alpine 3.20 | x64 / musl | `JYPPX.OpenCV.runtime.alpine.3.20-x64`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.alpine.3.20-x64.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.alpine.3.20-x64/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.alpine.3.20-x64/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.alpine.3.20-x64) | `JYPPX.OpenCV.runtime.alpine.3.20-x64.mini`<br>[![NuGet version](https://img.shields.io/nuget/v/JYPPX.OpenCV.runtime.alpine.3.20-x64.mini.svg?label=version)](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.alpine.3.20-x64.mini/)<br>[NuGet.org](https://www.nuget.org/packages/JYPPX.OpenCV.runtime.alpine.3.20-x64.mini/) · [GitHub](https://github.com/users/guojin-yan/packages/nuget/package/JYPPX.OpenCV.runtime.alpine.3.20-x64.mini) |

Linux packages use a distro-specific Linux RID rather than a generic `linux-x64` identity. If the .NET SDK does not recognize a project-defined RID, set `RuntimeIdentifierGraphPath` to [`packaging/runtime/runtime-distro-rid-graph.json`](packaging/runtime/runtime-distro-rid-graph.json) or copy that graph into the consuming project before restore.

Windows x86 Full is real-supported after verified hosted WoW64 producer, artifact, package, PE/I386 closure, and X86 consumer evidence; Windows x86 Mini remains excluded. Android x64/x86 Full and Mini are real-supported after authoritative single-loader NDK builds, package/APK audits, and emulator-native `Mat` plus `Cv2.Sum` loading; the retired dual-loader records remain under `superseded` in [`android-runtime-evidence.json`](packaging/runtime/android-runtime-evidence.json). Android ARM/ARM64 remain `android-evidence-pending`: their hosted production and package evidence has passed, but ABI-matched physical-device loading is still required before promotion. macOS is outside the declared runtime package matrix. Package-matrix presence is not a production-support claim.

If there is no matching runtime package, build a local native runtime with `scripts/Build-OpenCV.ps1`, stage it with `scripts/Stage-Runtime.ps1 -OpenCvNativeRuntimeDir <path>`, and point local samples or tests at it with `OpenCvNativeRuntimeDir`. The corresponding package fallback is `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>`. See the [Linked Runtime Build Guide](docs/articles/linked-runtime-build-guide.md) for the supported local native runtime workflow.

## ⚖️ Choose Full Or Mini

| Capability | Full | Mini |
| --- | :---: | :---: |
| Core arrays, matrices, persistence | Yes | Yes |
| ImgProc, ImgCodecs, VideoIO | Yes | Yes |
| Geometry and FLANN runtime dependencies | Yes | Yes |
| DNN, object detection, calibration, features | Yes | No |
| Photo, Video, HighGui, Stitching | Yes | No |
| ML | Yes | No |
| Tracking, additional contrib modules | Runtime-dependent | No |
| Stable response for an unavailable module | `NOT_LINKED` | `NOT_LINKED` |

## 🧪 Tutorial Series And Visual Results

The [`samples/ConsoleSamples`](samples/ConsoleSamples) project includes broad smoke coverage and the original six-part, headless showcase. The expanded grouped catalog is listed in [`samples/README.md`](samples/README.md). Set a CJK font path to run the complete showcase, including OpenCV `putText` with Chinese:

```powershell
$env:OPENCV_CSHARP_CJK_FONT = "C:\path\to\a-cjk-font.ttf"
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release `
  -p:OpenCvNativeRuntimeDir=E:\path\to\full-runtime `
  -- tutorial all .\artifacts\tutorials
```

The command writes image processing, native OpenCV Chinese text, contour, ORB feature, template matching, and KNN classification results:

![OpenCV CSharp API visual showcase](docs/images/showcase/showcase-overview.png)

Start with the [Tutorial Series](docs/articles/tutorial-series.md). Each output has a matching technical article, runnable command, focused code, runtime profile, and links into the deeper module guides. The earlier `showcase` command remains a compatibility alias.

The [`samples`](samples) directory is a scalable catalog of 23 complete examples. Image processing, features, geometry, video, tracking, stitching, classical ML, and deep learning each have numbered subdirectories. The full catalog, commands, outputs, and article map are maintained in [`samples/README.md`](samples/README.md); model provenance and verified downloads are documented in [Sample Model Assets](docs/articles/sample-model-assets-guide.md).

Run only the project for the feature you are learning. Each case restores the public managed API and the matching runtime fixture, performs a complete workflow, writes a focused result, and prints package/native build metadata. The reproducible fixture pin lives in `samples/SamplePackages.props`; normal installation commands remain version-neutral.

## 📚 Documentation

| Resource | Description |
| --- | --- |
| [Documentation site](https://guojin-yan.github.io/OpenCV-CSharp-API/) | API reference and articles |
| [Quick Start](docs/articles/quick-start.md) | Install and write the first program |
| [Tutorial Series](docs/articles/tutorial-series.md) | Grouped executable examples with synchronized technical articles |
| [Example Catalog](docs/articles/example-catalog.md) | Run the package-backed examples by capability group |
| [Sample Model Assets](docs/articles/sample-model-assets-guide.md) | Download pinned models with source, hash, and license verification |
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

## 🔧 Build From Source

Requirements:

- Any .NET 10 SDK for the repository validation path. The root `global.json` permits roll-forward within .NET 10.
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

## 🏗️ Project Structure

```text
OpenCV-CSharp-API/
|-- src/OpenCvSharp/                    Managed API
|-- src/OpenCvSharp.Native/             Stable native C ABI
|-- samples/ConsoleSamples/             Broad API smoke and visual showcase
|-- samples/ImageProcessing/            Image processing examples (01..06)
|-- samples/Features/                   Feature detection and matching examples
|-- samples/Geometry/                   Projective geometry examples
|-- samples/Video/                      Motion and temporal examples
|-- samples/Tracking/                   Stateful object-tracking examples
|-- samples/Stitching/                  Multi-image panorama examples
|-- samples/MachineLearning/            KNN and SVM examples
|-- samples/DeepLearning/               ONNX classification, detection, and segmentation
|-- samples/Common/                     Shared sample infrastructure
|-- packaging/runtime/                  RID/profile runtime package template
|-- compatibility/                      API, ABI, and upstream maps
|-- docs/                               DocFX configuration and articles
|-- scripts/                            Build, pack, verification, and release guards
`-- .github/workflows/                  CI, runtime production, pack, and publication
```

## 🤝 Contributing

Issues and pull requests are welcome. Before changing public API, native ABI, package identity, ownership, or runtime behavior, read [CONTRIBUTING.md](CONTRIBUTING.md) and the [API/ABI Compatibility Policy](docs/articles/api-abi-compatibility-policy.md).

Please include focused tests, preserve version-neutral public identities, and document any unsupported upstream behavior explicitly.

## 🙏 Acknowledgments

This project builds on [OpenCV](https://opencv.org/) and its contributors. OpenCV remains the authoritative source for algorithm behavior and native runtime licensing.

## 📄 License

The managed API, native wrapper, and OpenCV runtime are licensed under the [Apache License 2.0](LICENSE), so managed and runtime NuGet packages use the SPDX expression `Apache-2.0`. Packaged third-party notices remain authoritative for their respective components.

## 📮 Support And Contact

- [GitHub Issues](https://github.com/guojin-yan/OpenCV-CSharp-API/issues) for bugs and feature requests.
- [GitHub Discussions](https://github.com/guojin-yan/OpenCV-CSharp-API/discussions) for usage questions.
- QQ group `945057948` for community discussion.

`5.0.0` is the first stable release. Test representative workflows before using it in production, industrial, or mission-critical systems, especially on platform/runtime combinations that differ from the published evidence matrix.

<p align="center">
  <img src="docs/images/readme/contact-sponsor-en.png" alt="Author contact and sponsorship QR codes" width="100%">
</p>

## 📢 Software Notice

**1. Open-Source License Statement**

All open-source project code released by the author is licensed under the **Apache License 2.0**.

*Special note: This project integrates several third-party libraries. If the license of any third-party library conflicts with or differs from the Apache License 2.0, that third-party library's original license shall prevail. This project's license statement neither covers nor represents the licensing terms of those third-party libraries. Before use, be sure to read and comply with the applicable license of each third-party library.*

**2. Code Development And Quality Statement**

- **AI-Assisted Development**: Artificial intelligence (AI) was used to assist in generating and optimizing this code during development; it was not written entirely by hand, line by line.
- **Security Commitment**: **The author solemnly declares that this code contains no intentionally embedded backdoors, viruses, Trojan horses, or other malicious code intended to damage user devices or steal data.**
- **Technical Limitations**: Due to limitations in the author's individual technical knowledge and capabilities, the code may contain basic issues caused by insufficiently rigorous logic, inadequate optimization, or limited experience, including but not limited to memory leaks, intermittent crashes, or unreleased resources. Such issues result solely from limited capability and are not intentional.
- **Scope Of Testing**: Due to the author's limited time and energy, the software has not undergone comprehensive testing covering every edge case.

**3. Disclaimer (Important)**

**Before applying this code to any real-world project, especially in commercial, industrial, or mission-critical environments, you must conduct thorough and rigorous testing and validation yourself.** Given the possible code defects and limited test coverage described above, **the author assumes no responsibility for any direct or indirect loss arising from the use of this code, including but not limited to equipment failure, data loss, system outages, or loss of profit.** By using this code, you acknowledge these risks and agree to bear all resulting consequences; the author shall not be held responsible for related issues.

**4. Scope Of Open-Source Code**

This project commits to keeping its core logic fully open source. However, the binary files, source code, and related resources of the third-party libraries mentioned above are outside the scope of this project's open-source obligations. Obtain them according to their respective instructions.

**5. Community And Feedback**

Despite the limitations described above, everyone is welcome to download and use the project, submit Issues, or participate in testing to help improve it. If you discover bugs, memory overflows, or opportunities for improvement, please contact the author through the channels provided on the project homepage. The author will make a reasonable effort to assist within the time available.

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
- Windows runtime evidence distinguishes `CMAKE_ASM_COMPILER=NOTFOUND`, `OPENCV_DNN_MLAS_ENABLED=0`, factual `opencv_<module>500.dll` files, and Linux SONAME layouts. The corrected Full contract is one loader plus 17 modules, for 18 AMD64 DLLs. The previously verified 17 AMD64 DLL result belongs only to the 16-module first-preview historical candidate and cannot authorize the corrected release.
- The pack workflow operates over the active multi-RID runtime package matrix and its full/mini profiles. Synthetic jobs validate package shape, while publishable jobs require factual inputs and consumer restore verification. All normalized nupkg outputs remain neutral workflow artifacts rooted under `artifacts/packages`.
- `pack.yml` does not build real runtime inputs. Real input paths must already exist on the selected runner or come from `real_runtime_artifact_run_id`. Synthetic runtime inputs are package-surface validation only; real publishable runtime packages require `SyntheticRuntimeInputs=false`, non-synthetic provenance, and release preflight.
- Before packing, pass the exact release version with `-PackageVersion` and the factual staged runtime with `-OpenCvNativeRuntimeDir`. Public package IDs stay version-neutral; normalized `.nupkg` files are written beneath `artifacts\packages`.
- Full payload verification uses matrix-required modules plus provenance-recorded staged optional modules. Hosted native verification covers Ubuntu 24.04 x64 full/mini and Ubuntu 22.04 x64 full/mini.
- Ubuntu 24.04 ARM64 full runs natively on `ubuntu-24.04-arm`. Ubuntu 22.04 ARM64 full runs through a separate host-orchestrated `docker run` verifier. Debian 12 ARM64 full uses its own host-orchestrated `docker run` verifier on native `ubuntu-24.04-arm`.
- Debian 12 x64 full runs in a separate `debian:12` job container. Fedora 40 full runs in its own separate `fedora:40` job container. Rocky Linux 9 full runs in a fourth separate `rockylinux:9` job container. RHEL 9 full runs in a fifth separate official Red Hat UBI 9 job container. Alpine 3.20 full runs through a separate host-orchestrated `docker run alpine:3.20` verifier.

</details>

Copyright (c) 2026 Guojin Yan.
