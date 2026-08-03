![OpenCV CSharp API](https://socialify.git.ci/guojin-yan/OpenCV-CSharp-API/image?description=1&descriptionEditable=OpenCV%205.0%20bindings%20for%20C%23%20and%20.NET&forks=1&issues=1&name=1&owner=1&pattern=Circuit%20Board&pulls=1&stargazers=1&theme=Light)

# OpenCV CSharp API

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/JYPPX.OpenCV.CSharp.API.svg)](https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/)
[![Downloads](https://img.shields.io/nuget/dt/JYPPX.OpenCV.CSharp.API.svg)](https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.6--4.8.1%20%7C%20Core%203.1%20%7C%205--10-512BD4)](https://dotnet.microsoft.com/)
[![Upstream OpenCV](https://img.shields.io/badge/OpenCV-5.0.0-5C3EE8)](https://opencv.org/)
[![Managed CI](https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-managed.yml/badge.svg?branch=opencv5.x)](https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-managed.yml)
[![Native CI](https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-native.yml/badge.svg?branch=opencv5.x)](https://github.com/guojin-yan/OpenCV-CSharp-API/actions/workflows/build-native.yml)

[English](README.md) | 简体中文

OpenCV CSharp API 是面向 OpenCV 5.0 的版本中立 .NET 封装。项目通过 `OpenCvSharp.*` 提供符合 C# 使用习惯的 managed API，通过稳定 native C ABI 连接 OpenCV，并为已验证的 Windows 与 Linux 目标提供确定性 NuGet runtime 包。

首个公开版本通道为 `5.0.0-preview.1`，适合需要广泛 OpenCV 能力、同时重视后续 API/ABI 一致性的早期使用者。

## 主要特点

- 612 个 public managed type、6,314 个 public/protected member、41 个 namespace，均受兼容性基线约束。
- full profile 包含 2,656 个 native ABI function，mini profile 包含 526 个，并对声明 ABI 保持完整 native-to-managed binding coverage。
- 覆盖 Core、ImgProc、ImgCodecs、VideoIO、Calib3D、DNN、Features、HighGui、ObjDetect、Photo、Video、ML、Tracking、Stitching 和部分 contrib 模块。
- 支持 .NET Framework 4.6 至 4.8.1、.NET Core 3.1，以及 .NET 5 至 .NET 10。
- 提供 full 与 mini runtime profile；mini 中不可用的兼容入口会明确返回 `NOT_LINKED`。
- 提供确定性 NuGet 包、SPDX 2.3 SBOM、受保护发布审批和 NuGet.org Repository signature 回读验证。
- 提供无头可视化示例，不需要相机、模型下载或桌面 GUI，即可生成可检查的 PNG 结果。

![OpenCV CSharp API 可视化案例](docs/images/showcase/showcase-overview.png)

## 快速开始

安装 managed 包和一个与目标平台匹配的 runtime 包，并确保二者使用相同的 NuGet 规范版本。以下示例使用 Windows x64 full runtime：

```powershell
dotnet add package JYPPX.OpenCV.CSharp.API --version 5.0.0-preview.1
dotnet add package JYPPX.OpenCV.runtime.win-x64 --version 5.0.0-preview.1 # 当前 Windows x64 示例
```

然后使用 API 所属的模块 namespace：

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

完整的[快速开始](docs/articles/quick-start.md)包含 package 选择、Mat、图像编解码、几何运算和资源释放说明。

## NuGet 包

### Managed API

| 包 | 用途 |
| --- | --- |
| [`JYPPX.OpenCV.CSharp.API`](https://www.nuget.org/packages/JYPPX.OpenCV.CSharp.API/) | 面向全部受支持 .NET framework 的版本中立 managed API |

### Runtime 包

请根据 target RID 和 profile 选择一个包：

- Full：`JYPPX.OpenCV.runtime.<rid>`
- Mini：`JYPPX.OpenCV.runtime.<rid>.mini`

full profile 包含 DNN、标定、Features、Photo、HighGui、ML、Tracking、Stitching 等广泛模块。mini profile 聚焦 `core`、`imgproc`、`imgcodecs`、`videoio`，并包含必需的 `geometry` 与 `flann` runtime 依赖。

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

## Full 与 Mini

| 能力 | Full | Mini |
| --- | :---: | :---: |
| Core 数组、Mat、持久化 | 支持 | 支持 |
| ImgProc、ImgCodecs、VideoIO | 支持 | 支持 |
| Geometry 与 FLANN runtime 依赖 | 支持 | 支持 |
| DNN 与目标检测 | 支持 | 不支持 |
| 标定与特征匹配 | 支持 | 不支持 |
| Photo、Video、ML、Tracking、Stitching | 支持 | 不支持 |
| HighGui | 支持 | 不支持 |
| 缺失功能的稳定响应 | 不适用 | `NOT_LINKED` |

## 示例

[`samples/ConsoleSamples`](samples/ConsoleSamples) 包含广覆盖 smoke 和可视化 showcase：

```powershell
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release `
  -p:OpenCvNativeRuntimeDir=E:\path\to\full-runtime `
  -- showcase all .\artifacts\showcase
```

showcase 会生成图像处理、ORB 特征、模板匹配和 KNN 分类结果。详细说明见[可视化案例](docs/articles/visual-showcase.md)和[场景配方](docs/articles/scenario-recipes.md)。

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

## 许可证

managed API 与项目代码使用 [MIT License](LICENSE)。runtime 包同时包含项目代码与 OpenCV runtime 文件，因此 package license expression 为 `MIT AND Apache-2.0`；包内第三方 notice 对相应组件具有最终效力。

## 技术支持

- 通过 [GitHub Issues](https://github.com/guojin-yan/OpenCV-CSharp-API/issues) 报告 Bug 或提出功能建议。
- 通过 [GitHub Discussions](https://github.com/guojin-yan/OpenCV-CSharp-API/discussions) 交流使用问题。
- QQ 交流群：`945057948`。

本项目当前为 preview。用于生产、工业或关键任务系统前，请针对实际业务流程完成严格测试。

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
