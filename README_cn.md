<p align="center">
  <img src="https://socialify.git.ci/guojin-yan/OpenCV-CSharp-API/image?description=1&descriptionEditable=OpenCV%205.0%20bindings%20for%20C%23%20and%20.NET&forks=1&issues=1&name=1&owner=1&pattern=Circuit%20Board&pulls=1&stargazers=1&theme=Light" alt="OpenCV CSharp API" width="100%" />
</p>

<h1 align="center">OpenCV CSharp API</h1>

<p align="center">
  面向 C# 与 .NET 的版本中立 OpenCV 5.0 封装，提供 managed API 和经过验证的 native runtime 包。
</p>

<p align="center">
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-blue.svg" alt="MIT License" /></a>
  <a href="https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/"><img src="https://img.shields.io/nuget/v/JYPPX.OpenCV.CSharp.API.svg" alt="NuGet 版本" /></a>
  <a href="https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/"><img src="https://img.shields.io/nuget/dt/JYPPX.OpenCV.CSharp.API.svg" alt="NuGet 下载量" /></a>
  <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-Framework%204.6--4.8.1%20%7C%20Core%203.1%20%7C%205--10-512BD4" alt="支持的 .NET 版本" /></a>
  <a href="https://opencv.org/"><img src="https://img.shields.io/badge/OpenCV-5.0.0-5C3EE8" alt="上游 Upstream OpenCV 5.0.0" /></a>
</p>

<p align="center">
  <a href="https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-managed.yml"><img src="https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-managed.yml/badge.svg?branch=opencv5.x" alt="Managed CI" /></a>
  <a href="https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-native.yml"><img src="https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-native.yml/badge.svg?branch=opencv5.x" alt="Native CI" /></a>
</p>

<p align="center"><a href="README.md">English</a> | <strong>简体中文</strong></p>

## 项目简介

OpenCV CSharp API 通过熟悉的 `OpenCvSharp.*` 命名空间，将 [OpenCV 5.0](https://opencv.org/) 引入 C#。项目提供符合 .NET 使用习惯的 managed API、稳定的 native C ABI、明确的 native 资源所有权，以及面向已验证 Windows 和 Linux 目标的 runtime NuGet 包。

首个公开版本通道为 `5.0.0-preview.1`，适合需要广泛计算机视觉能力、同时重视后续 API 与 ABI 身份稳定性的早期使用者。

## 版本亮点

- 广泛覆盖 Core、ImgProc、ImgCodecs、VideoIO、Calib3D、DNN、Features、ObjDetect、Photo、Video、HighGui、Stitching、ML、Tracking 和部分 contrib API。
- 一个 managed 包，以及面向 12 个已验证 Windows/Linux RID 的 full 与 mini native runtime profile。
- 支持 .NET Framework 4.6 至 4.8.1、.NET Core 3.1，以及 .NET 5 至 .NET 10。
- 提供确定性 NuGet 包、SPDX 2.3 SBOM、受保护的发布审批和 NuGet.org Repository signature 验证。
- 提供可直接执行的无头示例，无需相机、模型下载或桌面 GUI，即可生成可检查的 PNG 结果。
- 兼容性基线覆盖 612 个 public managed type、6,314 个 public/protected member、41 个 namespace 和已声明的 native ABI。

## 30 秒快速开始

### 1. 安装 NuGet 包

安装 managed API 和一个 runtime 包，并确保二者使用相同版本。以下示例选择 Windows x64 full runtime：

```powershell
dotnet add package JYPPX.OpenCV.CSharp.API --version 5.0.0-preview.1
dotnet add package JYPPX.OpenCV.runtime.win-x64 --version 5.0.0-preview.1 # 当前 Windows x64 示例
```

### 2. 编写 C# 代码

根据 OpenCV 模块使用对应的命名空间：

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

### 3. 运行程序

```powershell
dotnet run
```

预期输出：

```text
7,7,7,7,7,7
```

完整的[快速开始](docs/articles/quick-start.md)包含 package 选择、Mat、图像编解码、几何运算和确定性资源释放说明。

## NuGet 包

| 包 | 用途 |
| --- | --- |
| [`JYPPX.OpenCV.CSharp.API`](https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/) | 面向全部受支持 .NET target 的版本中立 managed API |
| `JYPPX.OpenCV.runtime.<rid>` | 面向一个受支持 RID 的 full native runtime |
| `JYPPX.OpenCV.runtime.<rid>.mini` | 面向一个受支持 RID 的轻量 native runtime |

请根据 target RID 和所需 profile 选择对应的 runtime 包。

full profile 保证包含矩阵要求的模块，包括 DNN、标定、Features、Photo、Video、HighGui 和 Stitching。ML、Tracking 和部分 contrib 模块属于按实际构建暂存的可选模块：managed API 保持稳定，native 功能不可用时返回 `NOT_LINKED`。mini profile 聚焦 `core`、`imgproc`、`imgcodecs`、`videoio`，并包含必需的 `geometry` 与 `flann` 依赖。

不要同时引用 full 与 mini runtime 包。managed 和 runtime 包必须使用同一个 NuGet 规范版本。

## 支持的平台

首个 preview 发布 managed 包，以及 [`runtime-support-contract.json`](packaging/runtime/runtime-support-contract.json) 中归类为 `realSupport` 的 24 个 runtime 目标。

| 平台 | RID | 架构 | Profile |
| --- | --- | --- | --- |
| Windows 10/11 | `win-x64` | x64 | full、mini |
| Windows 11 | `win-arm64` | ARM64 | full、mini |
| Ubuntu 22.04 | `ubuntu.22.04-x64`、`ubuntu.22.04-arm64` | x64、ARM64 | full、mini |
| Ubuntu 24.04 | `ubuntu.24.04-x64`、`ubuntu.24.04-arm64` | x64、ARM64 | full、mini |
| Debian 12 | `debian.12-x64`、`debian.12-arm64` | x64、ARM64 | full、mini |
| Fedora 40 | `fedora.40-x64` | x64 | full、mini |
| RHEL 9 / UBI 9 | `rhel.9-x64` | x64 | full、mini |
| Rocky Linux 9 | `rocky.9-x64` | x64 | full、mini |
| Alpine 3.20 | `alpine.3.20-x64` | x64 / musl | full、mini |

Linux runtime 包使用 distro-specific Linux RID，不使用模糊的通用 `linux-x64` 身份。如果 .NET SDK 无法识别项目定义的 RID，请将 `RuntimeIdentifierGraphPath` 指向 [`packaging/runtime/runtime-distro-rid-graph.json`](packaging/runtime/runtime-distro-rid-graph.json)，或在 restore 前把该文件复制到 consumer 项目。

`win-x86/full` 仍为 `hosted-evidence-pending`。`win-x86/mini` 与 Android profile 因缺少完整真实 producer/consumer 证据，不进入首个 preview。macOS 位于声明的 runtime package matrix 之外。矩阵中存在某一行，不等于项目对该平台作出正式支持承诺。

如果 no matching runtime package，请使用 `scripts/Build-OpenCV.ps1` 构建 local native runtime，再通过 `scripts/Stage-Runtime.ps1 -OpenCvNativeRuntimeDir <path>` 暂存，并用 `OpenCvNativeRuntimeDir` 将本地示例或测试指向该目录。对应 package fallback 命令为 `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>`。完整流程见 [Linked Runtime 构建指南](docs/articles/linked-runtime-build-guide.md)。

## 选择 Full 或 Mini

| 能力 | Full | Mini |
| --- | :---: | :---: |
| Core 数组、Mat、持久化 | 支持 | 支持 |
| ImgProc、ImgCodecs、VideoIO | 支持 | 支持 |
| Geometry 与 FLANN runtime 依赖 | 支持 | 支持 |
| DNN、目标检测、标定、特征 | 支持 | 不支持 |
| Photo、Video、HighGui、Stitching | 支持 | 不支持 |
| ML、Tracking、部分 contrib 模块 | 取决于 runtime | 不支持 |
| 模块不可用时的稳定响应 | `NOT_LINKED` | `NOT_LINKED` |

## 示例与可视化结果

[`samples/ConsoleSamples`](samples/ConsoleSamples) 包含广覆盖 smoke 和图像处理 showcase：

```powershell
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release `
  -p:OpenCvNativeRuntimeDir=E:\path\to\full-runtime `
  -- showcase all .\artifacts\showcase
```

该命令会生成图像处理、ORB 特征、模板匹配和 KNN 分类结果：

![OpenCV CSharp API 可视化案例](docs/images/showcase/showcase-overview.png)

更多可执行流程见[可视化案例](docs/articles/visual-showcase.md)和[场景配方](docs/articles/scenario-recipes.md)。

## 文档

| 资源 | 说明 |
| --- | --- |
| [在线文档](https://guojin-yan.github.io/OpenCV-CSharp-API/) | API reference 和专题文章 |
| [快速开始](docs/articles/quick-start.md) | 安装并编写第一个程序 |
| [可视化案例](docs/articles/visual-showcase.md) | 图像、特征、模板与 ML 示例 |
| [场景配方](docs/articles/scenario-recipes.md) | 面向任务的使用流程 |
| [Linked Runtime 构建指南](docs/articles/linked-runtime-build-guide.md) | 构建和暂存真实 native runtime |
| [Linked Runtime Smoke 指南](docs/articles/linked-runtime-smoke-guide.md) | 验证 native 加载与 package 执行 |
| [Smoke Profile 指南](docs/articles/smoke-profiles-guide.md) | 选择 full 或 mini 验证 |
| [Runtime License](docs/articles/runtime-licenses.md) | runtime 与第三方许可证 |
| [Runtime package README](packaging/runtime/JYPPX.OpenCV.runtime/README.md) | 包布局与 provenance |
| [API/ABI 兼容策略](docs/articles/api-abi-compatibility-policy.md) | 兼容性与 gap 统计 |
| [支持与生命周期策略](docs/articles/support-lifecycle-policy.md) | real、pending 与 excluded 目标 |
| [NuGet Repository Signing 指南](docs/articles/nuget-repository-signing-guide.md) | 发布信任和验证流程 |

## 从源码构建

环境要求：

- 仓库精确验证路径使用 .NET SDK 10.0.302。
- native 构建需要 CMake 和受支持的 C/C++ toolchain。
- linked native 构建需要 OpenCV 5.0.0 源码或已安装的 OpenCV 5.0.0 runtime。

```powershell
git clone https://github.com/guojin-yan/OpenCV-CSharp-API.git
cd OpenCV-CSharp-API
git switch opencv5.x

dotnet restore .\OpenCV-CSharp-API.slnx
dotnet build .\OpenCV-CSharp-API.slnx -c Release --no-restore
```

native 和 package 构建请使用 [Linked Runtime 构建指南](docs/articles/linked-runtime-build-guide.md)。synthetic runtime input 只用于验证 package shape，禁止发布。

## 项目结构

```text
OpenCV-CSharp-API/
|-- src/OpenCvSharp/                    Managed API
|-- src/OpenCvSharp.Native/             稳定 native C ABI
|-- samples/ConsoleSamples/             Smoke 与可视化示例
|-- packaging/runtime/                  RID/profile runtime package 模板
|-- compatibility/                      API、ABI 与 upstream map
|-- docs/                               DocFX 配置与文章
|-- scripts/                            构建、打包、验证与发布 guard
`-- .github/workflows/                  CI、runtime 生产、打包与发布
```

## 参与贡献

欢迎提交 Issue 和 Pull Request。修改 public API、native ABI、package identity、所有权或 runtime 行为之前，请阅读 [CONTRIBUTING.md](CONTRIBUTING.md) 和 [API/ABI 兼容策略](docs/articles/api-abi-compatibility-policy.md)。

请提供聚焦测试，保持 public identity 版本中立，并明确记录无法支持的 upstream 行为。

## 致谢

本项目建立在 [OpenCV](https://opencv.org/) 及其社区贡献之上。算法行为和 native runtime 许可证以 OpenCV 上游内容为准。

## 许可证

managed API 与项目代码使用 [MIT License](LICENSE)。runtime 包同时包含项目代码与 OpenCV runtime 文件，因此 package license expression 为 `MIT AND Apache-2.0`；包内第三方 notice 对相应组件具有最终效力。

## 技术支持与联系方式

- 通过 [GitHub Issues](https://github.com/guojin-yan/OpenCV-CSharp-API/issues) 报告 Bug 或提出功能建议。
- 通过 [GitHub Discussions](https://github.com/guojin-yan/OpenCV-CSharp-API/discussions) 交流使用问题。
- QQ 交流群：`945057948`。

本项目当前为 preview。用于生产、工业或关键任务系统前，请针对实际业务流程完成严格测试。

## 软件声明

本项目在开发过程中使用了 AI 辅助，尚未在所有设备、工作负载和边缘场景下完成穷尽测试。源码中不包含任何有意引入的恶意功能。使用前请阅读许可证和第三方 notice，根据自身需求审查代码，并在生产或安全关键场景中完成严格验证。

<details>
<summary>维护者构建与兼容边界</summary>

以下内容用于保留仓库审计约束。普通使用者应优先阅读上方链接的指南。

- native CMake 工程目前只支持 source-tree build，不安装或导出通用 CMake package/SDK target。`JYPPX.OpenCV.Native` 是主 target，`OpenCv5Sharp.Native` 仅为 compatibility alias。
- runtime NuGet 包当前不分发 native C header。主 header tree 为 `src/OpenCvSharp.Native/include/open_cv_sharp`；`src/OpenCvSharp.Native/include/open_cv_5_sharp` 是兼容树。
- native CTest 和本地输出命名保持版本中立：`JYPPX.OpenCV.NativeSmoke` 与 `JYPPX.OpenCV.NativeCompatibilitySourceSmoke`。`OpenCv5Sharp.Native` loader 只保留为 compatibility copy。
- 本地 sample 与 test 优先使用 `OpenCvNativeRuntimeDir`；`OpenCv5SharpNativeRuntimeDir` 仅作为 compatibility alias。`OpenCv5SharpBuildInfo` 仍是有文档和测试保护的 compatibility facade。
- `runtime-input.yml` 生成真实 `runtime-input-<rid>-<profile>` artifact，内含 `native-wrapper/`、`opencv-runtime/` 与 `opencv-source/`。
- pack workflow 覆盖当前 multi-RID runtime package matrix 及 full/mini profile。synthetic job 只验证 package shape；可发布 job 要求真实输入与 consumer restore 验证。规范化 nupkg 输出位于 `artifacts/packages`。
- `pack.yml` 不构建真实 runtime input。真实输入必须已存在于 runner，或来自 `real_runtime_artifact_run_id`。synthetic runtime input 只验证 package surface；real publishable runtime packages require `SyntheticRuntimeInputs=false`。
- Before packing，使用 `-PackageVersion` 传入精确发布版本，并通过 `-OpenCvNativeRuntimeDir` 传入已暂存的真实 runtime。package IDs stay version-neutral；normalized `.nupkg` 写入 `artifacts\packages`。
- Ubuntu 24.04 ARM64 full 直接运行在 `ubuntu-24.04-arm`。Ubuntu 22.04 ARM64 full 使用独立 host-orchestrated `docker run` verifier。Debian 12 ARM64 full 使用由原生 `ubuntu-24.04-arm` 宿主机编排的独立 `docker run` verifier。

</details>

Copyright (c) 2026 Guojin Yan.
