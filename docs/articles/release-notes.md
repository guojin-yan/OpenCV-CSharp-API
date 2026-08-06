# 5.0.0 Release Notes / 5.0.0 发布说明

`5.0.0` is the first stable release of OpenCV CSharp API. It binds OpenCV 5.0.0 through the version-neutral `JYPPX.OpenCvSharp.*` managed API, `JYPPX.OpenCV.CSharp.API` package, and stable `jyppx_ocv_*` native C ABI. Public availability must be confirmed on NuGet.org, GitHub Packages, and the GitHub Release page; these notes do not treat a local candidate as published.

`5.0.0` 是 OpenCV CSharp API 的首个稳定版。它通过版本中立的 `JYPPX.OpenCvSharp.*` managed API、`JYPPX.OpenCV.CSharp.API` 包和稳定的 `jyppx_ocv_*` native C ABI 绑定 OpenCV 5.0.0。是否已经公开可用必须以 NuGet.org、GitHub Packages 和 GitHub Release 页面为准；本文不会把本地候选误称为已发布版本。

## Highlights / 主要内容

- 632 public managed types, 6,817 public/protected members, and 41 namespaces rooted at `JYPPX.OpenCvSharp` under a checked compatibility baseline.
- 2,663 full-profile and 527 mini-profile native ABI functions, with complete native-to-managed binding coverage for the declared ABI.
- `OpenCvSharpBuildInfo.NuGetPackageVersion` reports the exact `5.0.0` package identity; native ABI version `1` is exported and verified before runtime use.
- `ColorConversionCodes` mirrors the OpenCV 5.0.0 conversion table, including BGR/RGB, BGRA/RGBA, grayscale, HSV, Lab, YUV, and Bayer families.
- `Mat.CopyTo`/`CopyFrom` now honor non-contiguous views; typed row spans, `GetValue`/`SetValue`, OpenCV pixel-vector structs, and external-stride buffer copies provide safe image access without repeating pointer arithmetic in every consumer.
- DNN detection postprocessing now exposes OpenCV-backed standard, class-aware batched, rotated-box, and Soft-NMS operations.
- Type-safe `ImageEncodingParam`, `VideoCapture.TryRead`/`TryRetrieve`, `Point3d`, 16-bit vector pixels, and dynamic-channel `MatType` factories reduce common interop boilerplate.
- Image decode/encode, processing, geometry, calibration, video, DNN, object detection, Photo, machine learning, Tracking, Stitching, and selected contrib workflows.
- Corrected Full runtimes make OpenCV `ml` mandatory, ship one neutral loader plus 17 required OpenCV modules, and verify both DNN inference and trained KNN prediction.
- Deterministic normalized NuGet packages, package-owned full/mini native smoke, package provenance, SPDX-2.3 SBOM generation, and fail-closed release review.
- The stable managed package is checked by .NET package validation against the published `5.0.0-preview.1` API baseline before it can enter the candidate set.
- Until the stable packages are public, repository-only sample package fixtures continue to pin the existing public `5.0.0-preview.1`; normal installation commands remain version-free and resolve the current stable package.
- The exact support-contract-derived candidate is published to NuGet.org and GitHub Packages, then attached to a verified stable GitHub Release with both registry proofs; the intended final set is 29 packages after Android single-loader revalidation.
- A 24-part [Tutorial Series](tutorial-series.md) and grouped standalone projects covering image processing, Chinese text, geometry, features, ML, DNN classification/detection/segmentation, document scanning, panorama stitching, motion analysis, tracking, and Android package loading.

- 632 个 public managed type、6,817 个 public/protected member 和 `JYPPX.OpenCvSharp` 根下的 41 个 namespace，全部受兼容性基线约束。
- full profile 2,663 个、mini profile 527 个 native ABI function，并对声明 ABI 保持完整 native-to-managed binding coverage。
- `OpenCvSharpBuildInfo.NuGetPackageVersion` 返回精确的 `5.0.0` 包身份；native ABI 版本 `1` 已导出并在 runtime 使用前校验。
- `ColorConversionCodes` 对齐 OpenCV 5.0.0 转换表，覆盖 BGR/RGB、BGRA/RGBA、灰度、HSV、Lab、YUV 和 Bayer 系列。
- `Mat.CopyTo`/`CopyFrom` 可正确处理非连续视图；类型化行 Span、`GetValue`/`SetValue`、OpenCV 像素向量结构和外部 stride 缓冲区复制，避免上层项目重复编写指针运算。
- DNN 目标检测后处理新增 OpenCV 原生实现的普通 NMS、按类别批量 NMS、旋转框 NMS 和 Soft-NMS。
- 新增类型安全的 `ImageEncodingParam`、`VideoCapture.TryRead`/`TryRetrieve`、`Point3d`、16 位像素向量和动态通道 `MatType` 工厂。
- 覆盖图像编解码、处理、几何、标定、视频、DNN、目标检测、Photo、机器学习、Tracking、Stitching 和部分 contrib 工作流。
- 修正 Full runtime 契约，将 OpenCV `ml` 设为必需模块，交付一个中性 loader 加 17 个必需 OpenCV 模块，并同时验证 DNN 推理和真实训练后的 KNN 预测。
- 提供确定性规范化 NuGet 包、包内 full/mini native smoke、package provenance、SPDX-2.3 SBOM 和 fail-closed 发布审核。
- 稳定版 managed 包进入候选集合前，必须通过 .NET package validation 与已发布 `5.0.0-preview.1` API 基线的兼容性比较。
- 在稳定版包正式公开前，仓库内部案例的 package fixture 继续固定现有公开的 `5.0.0-preview.1`；普通安装命令不写死版本并解析当前稳定包。
- 同一份精确的 29 包 candidate 发布到 NuGet.org 和 GitHub Packages，并在两个 registry 验证通过后附加到稳定版 GitHub Release。
- 提供 24 篇[系列教程](tutorial-series.md)和分组独立项目，覆盖图像处理、中文写字、几何、特征、机器学习、DNN 分类/检测/分割、文档扫描、全景拼接、运动分析、跟踪和 Android 包加载。

## Package Selection / 包选择

Install the managed package and exactly one runtime package at the same normalized version. The current candidate contains the managed package plus every RID/profile pair classified as `realSupport` in the checked-in support contract.

managed 包与一个 runtime 包必须使用相同的 NuGet 规范版本。当前候选包含 managed 包，以及仓库 support contract 中全部 `realSupport` RID/profile runtime 包。

| Profile | Package | Use it for |
| --- | --- | --- |
| Full | `JYPPX.OpenCV.runtime.win-x64` | DNN, calibration, features, Photo, HighGui, ML, Tracking, Stitching, and broad module workflows |
| Mini | `JYPPX.OpenCV.runtime.win-x64.mini` | Smaller `core,imgproc,imgcodecs,videoio` workflows plus required `geometry,flann` runtime dependencies |

Mini deliberately excludes full-only wrapper sources and reports the stable `NOT_LINKED` boundary where a compatibility entrypoint exists. It is not a transparent replacement for full.

mini 会有意排除 full-only wrapper source；当兼容入口存在时，它会报告稳定的 `NOT_LINKED` 边界。mini 不是 full 的透明替代品。

## Install / 安装

Full:

```powershell
dotnet add package JYPPX.OpenCV.CSharp.API
dotnet add package JYPPX.OpenCV.runtime.win-x64
dotnet restore
```

Mini:

```powershell
dotnet add package JYPPX.OpenCV.CSharp.API
dotnet add package JYPPX.OpenCV.runtime.win-x64.mini
dotnet restore
```

Do not reference full and mini runtime packages together. Keep managed and runtime package versions aligned and avoid floating ranges in production or reproducibility-sensitive builds.

不要同时引用 full 与 mini runtime 包。package version 应保持精确；生产环境或需要可重复构建时不要使用浮动 prerelease 范围。

## Upgrade And Profile Change / 升级与 Profile 切换

Commit the project file and package lock state before changing package versions. Update the managed and runtime packages together. To change between full and mini, remove the old runtime package first, add the other profile at the same exact version, restore, and rerun the application's representative native workflows.

修改 package version 前，应提交 project file 与 package lock 状态。managed 和 runtime 包必须同时升级。切换 full/mini 时，先移除旧 runtime 包，再添加相同精确版本的另一 profile，执行 restore，并重新运行应用的代表性 native 工作流。

```powershell
dotnet remove package JYPPX.OpenCV.runtime.win-x64
dotnet add package JYPPX.OpenCV.runtime.win-x64.mini
dotnet restore
```

## Rollback And Uninstall / 回滚与卸载

`5.0.0-preview.1` is the previous published preview and may be used only as an explicit rollback pair when its older Full runtime boundary is acceptable. It predates the required `ml` module and corrected Full payload, so ML-dependent applications must not treat it as equivalent to this release. Pin the managed package and exactly one runtime package to the same known-good version rather than selecting `latest`.

`5.0.0-preview.1` 是上一个已发布预览版；仅当应用可以接受旧 Full runtime 边界时，才可把它作为精确的 managed/runtime 回滚组合。该版本早于必需 `ml` 模块和修正后的 Full payload，依赖 ML 的应用不能把它视为与本版本等价。回滚时必须把 managed 包和唯一一个 runtime 包固定到相同的 known-good 版本，不能选择 `latest`。

To remove the preview completely:

```powershell
dotnet remove package JYPPX.OpenCV.runtime.win-x64
dotnet remove package JYPPX.OpenCV.runtime.win-x64.mini
dotnet remove package JYPPX.OpenCV.CSharp.API
dotnet restore
```

`dotnet remove package` reports an error when a listed profile is not referenced; remove only the runtime package present in the project. Do not delete global NuGet caches as a normal rollback step. Restore the prior project/lock-file revision and run the prior application's tests.

如果列出的 profile 未被引用，`dotnet remove package` 会报告错误；只需移除项目实际使用的 runtime 包。正常回滚不应删除全局 NuGet cache。恢复此前的 project/lock-file revision 后，应重新执行原应用测试。

## Known Limitations / 已知限制

- This is the first stable release. Semantic-versioning compatibility applies to the declared managed API and native ABI; additional APIs and runtime packages may be added compatibly in later releases.
- Measured module partitions have explicit zero-gap evidence, but this is not repository-wide or all-OpenCV parity.
- The current fail-closed candidate contains 29 packages: one managed package plus 28 real-supported runtime packages across Android x64/x86, Windows x64/ARM64, and the declared Ubuntu, Debian, Fedora, RHEL, Rocky, and Alpine targets. Android x64/x86 Full/Mini have authoritative single-loader emulator evidence; Android ARM/ARM64 Full/Mini remain `android-evidence-pending` until physical-device loading passes; `win-x86/full` remains hosted-evidence-pending, `win-x86/mini` is excluded, and macOS is outside the declared matrix.
- Mini excludes DNN, calibration, features, Photo, HighGui, ML, Tracking, Stitching, and other full-only modules.
- Some algorithms require user-supplied models, training data, codecs, GUI backends, or optional OpenCV build features. The library does not silently download them.
- HighGui requires a compatible desktop UI backend and event-thread model. Server, container, and unattended workflows should prefer file or memory encoding.
- Fedora 40 and Alpine 3.20 are exact compatibility targets with ended standard lifecycle; they are not current-lifecycle distribution promises.
- The normalized candidate is intentionally unsigned before upload. NuGet.org must add a `Repository` primary signature owned by `GuojinYan`; the downloaded public package must pass `dotnet nuget verify --all` and exact payload comparison before the release is accepted.
- GitHub Packages initially creates the 29 user-scoped packages as private. Each must be made Public, linked to `guojin-yan/OpenCV-CSharp-API`, and verified byte-for-byte against the reviewed candidate before the Release is created.
- The explicitly authorized, version-bounded single-maintainer stable-release exception, both public package registries, repository-signature evidence, exact GitHub package hashes, and support status must be verified from the published artifacts, not inferred from local candidates or mirror CI.

- 本版本是首个稳定版。声明的 managed API 与 native ABI 遵循语义化版本兼容规则；后续版本仍可兼容性地增加 API 与 runtime 包。
- 已测量模块分区具有明确的 zero-gap 证据，但不代表整个仓库或全部 OpenCV 已达到 parity。
- 当前 fail-closed 候选包含 29 个包：一个 managed 包，加上 Android x64/x86、Windows x64/ARM64 以及声明的 Ubuntu、Debian、Fedora、RHEL、Rocky、Alpine 目标共 28 个真实支持 runtime 包。Android x64/x86 的 Full/Mini 已有正式单加载器模拟器证据；Android ARM/ARM64 的 Full/Mini 在真机加载通过前仍为 `android-evidence-pending`；`win-x86/full` 仍为 hosted-evidence-pending，`win-x86/mini` 被排除，macOS 位于声明矩阵之外。
- mini 排除 DNN、标定、Features、Photo、HighGui、ML、Tracking、Stitching 和其他 full-only 模块。
- 部分算法需要用户提供模型、训练数据、codec、GUI backend 或 OpenCV 可选构建能力；库不会静默下载这些输入。
- HighGui 需要兼容的桌面 UI backend 和事件线程模型。服务器、容器和无人值守流程应优先使用文件或内存编码。
- Fedora 40 与 Alpine 3.20 是标准生命周期已结束的精确兼容目标，不代表当前生命周期发行版承诺。
- 规范化候选在上传前有意保持未签名；NuGet.org 必须为公开下载包添加 owner 为 `GuojinYan` 的 `Repository` primary signature，并通过 `dotnet nuget verify --all` 与精确 payload 对比。
- GitHub Packages 初次创建 29 个 user-scoped 包时默认为 private；每个包都必须设为 Public、关联 `guojin-yan/OpenCV-CSharp-API`，并与审核 candidate 逐字节验证后才能创建 Release。
- 经明确授权且仅限本次版本的单维护者稳定版例外、两个公开 package registry、repository-signature 证据、GitHub package 精确 hash 和支持状态必须以发布产物为准，不能从本地候选或 mirror CI 推断。

## Verification / 验证

After installation, start with [Quick Start](quick-start.md), run the package-owned smoke appropriate to the selected profile, and execute at least one representative application workflow. For release evidence and support boundaries, see [Release Candidate Closeout](release-candidate-closeout.md) and [Support And Lifecycle Policy](support-lifecycle-policy.md).

安装后先阅读 [Quick Start](quick-start.md)，运行所选 profile 对应的 package-owned smoke，并至少执行一个代表性应用工作流。发布证据和支持边界见 [Release Candidate Closeout](release-candidate-closeout.md) 与 [Support And Lifecycle Policy](support-lifecycle-policy.md)。
