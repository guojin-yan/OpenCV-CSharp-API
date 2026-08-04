# 5.0.0-preview.1 Release Notes / 5.0.0-preview.1 发布说明

`5.0.0-preview.1` is the first planned public preview of OpenCV CSharp API. It binds OpenCV 5.0.0 through the version-neutral `JYPPX.OpenCvSharp.*` managed API, `JYPPX.OpenCV.CSharp.API` package, and stable `jyppx_ocv_*` native C ABI. Public availability must be confirmed on NuGet.org, GitHub Packages, and the GitHub Release page; these notes do not treat a local candidate as published.

`5.0.0-preview.1` 是 OpenCV CSharp API 计划发布的首个公开预览版。它通过版本中立的 `JYPPX.OpenCvSharp.*` managed API、`JYPPX.OpenCV.CSharp.API` 包和稳定的 `jyppx_ocv_*` native C ABI 绑定 OpenCV 5.0.0。是否已经公开可用必须以 NuGet.org、GitHub Packages 和 GitHub Release 页面为准；本文不会把本地候选误称为已发布版本。

## Highlights / 主要内容

- 611 public managed types, 6,300 public/protected members, and 41 namespaces rooted at `JYPPX.OpenCvSharp` under a checked compatibility baseline.
- 2,656 full-profile and 526 mini-profile native ABI functions, with complete native-to-managed binding coverage for the declared ABI.
- Image decode/encode, processing, geometry, calibration, video, DNN, object detection, Photo, machine learning, Tracking, Stitching, and selected contrib workflows.
- Deterministic normalized NuGet packages, package-owned full/mini native smoke, package provenance, SPDX-2.3 SBOM generation, and fail-closed release review.
- The exact support-contract-derived candidate is published to NuGet.org and GitHub Packages, then attached to a verified GitHub prerelease with both registry proofs; the intended final set is 29 packages after Android single-loader revalidation.
- A seven-part [Tutorial Series](tutorial-series.md), eight generated images, direct Chinese rendering through OpenCV 5 `putText`, Android package consumption, compatibility `showcase` commands, and task-oriented [Scenario Recipes](scenario-recipes.md).

- 611 个 public managed type、6,300 个 public/protected member 和 `JYPPX.OpenCvSharp` 根下的 41 个 namespace，全部受兼容性基线约束。
- full profile 2,656 个、mini profile 526 个 native ABI function，并对声明 ABI 保持完整 native-to-managed binding coverage。
- 覆盖图像编解码、处理、几何、标定、视频、DNN、目标检测、Photo、机器学习、Tracking、Stitching 和部分 contrib 工作流。
- 提供确定性规范化 NuGet 包、包内 full/mini native smoke、package provenance、SPDX-2.3 SBOM 和 fail-closed 发布审核。
- 同一份精确的 29 包 candidate 发布到 NuGet.org 和 GitHub Packages，并在两个 registry 验证通过后附加到 GitHub prerelease。
- 提供 7 个案例的[系列教程](tutorial-series.md)、8 张生成图片、通过 OpenCV 5 `putText` 直接绘制中文、Android 包消费、兼容 `showcase` 命令和面向任务的 [Scenario Recipes](scenario-recipes.md)。

## Package Selection / 包选择

Install the managed package and exactly one runtime package at the same normalized version. The first candidate contains the managed package plus every RID/profile pair classified as `realSupport` in the checked-in support contract.

managed 包与一个 runtime 包必须使用相同的 NuGet 规范版本。首个候选包含 managed 包，以及仓库 support contract 中全部 `realSupport` RID/profile runtime 包。

| Profile | Package | Use it for |
| --- | --- | --- |
| Full | `JYPPX.OpenCV.runtime.win-x64` | DNN, calibration, features, Photo, HighGui, ML, Tracking, Stitching, and broad module workflows |
| Mini | `JYPPX.OpenCV.runtime.win-x64.mini` | Smaller `core,imgproc,imgcodecs,videoio` workflows plus required `geometry,flann` runtime dependencies |

Mini deliberately excludes full-only wrapper sources and reports the stable `NOT_LINKED` boundary where a compatibility entrypoint exists. It is not a transparent replacement for full.

mini 会有意排除 full-only wrapper source；当兼容入口存在时，它会报告稳定的 `NOT_LINKED` 边界。mini 不是 full 的透明替代品。

## Install / 安装

Full:

```powershell
dotnet add package JYPPX.OpenCV.CSharp.API --prerelease
dotnet add package JYPPX.OpenCV.runtime.win-x64 --prerelease
dotnet restore
```

Mini:

```powershell
dotnet add package JYPPX.OpenCV.CSharp.API --prerelease
dotnet add package JYPPX.OpenCV.runtime.win-x64.mini --prerelease
dotnet restore
```

Do not reference full and mini runtime packages together. Keep package versions exact and avoid floating prerelease ranges in production or reproducibility-sensitive builds.

不要同时引用 full 与 mini runtime 包。package version 应保持精确；生产环境或需要可重复构建时不要使用浮动 prerelease 范围。

## Upgrade And Profile Change / 升级与 Profile 切换

Commit the project file and package lock state before changing package versions. Update the managed and runtime packages together. To change between full and mini, remove the old runtime package first, add the other profile at the same exact version, restore, and rerun the application's representative native workflows.

修改 package version 前，应提交 project file 与 package lock 状态。managed 和 runtime 包必须同时升级。切换 full/mini 时，先移除旧 runtime 包，再添加相同精确版本的另一 profile，执行 restore，并重新运行应用的代表性 native 工作流。

```powershell
dotnet remove package JYPPX.OpenCV.runtime.win-x64
dotnet add package JYPPX.OpenCV.runtime.win-x64.mini --prerelease
dotnet restore
```

## Rollback And Uninstall / 回滚与卸载

This is the first public-preview version, so there is no earlier published OpenCV CSharp API package version that these notes can promise as a rollback target. Before adoption, retain the application's prior source/lock-file state and any internally approved package archive. A future rollback must pin one exact known-good managed/runtime pair rather than selecting `latest`.

这是首个公开预览版本，因此本文不能承诺一个更早的已发布 OpenCV CSharp API 包作为降级目标。采用本预览版前，请保留应用此前的 source/lock-file 状态以及内部批准的 package archive。后续回滚必须固定到一组精确的 known-good managed/runtime 包，不能选择 `latest`。

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

- The release is a preview. Compatibility baselines prevent accidental drift, but additional APIs and runtime packages will continue to be added.
- Measured module partitions have explicit zero-gap evidence, but this is not repository-wide or all-OpenCV parity.
- The current fail-closed candidate contains 29 packages: one managed package plus 28 real-supported runtime packages across Android x64/x86, Windows x64/ARM64, and the declared Ubuntu, Debian, Fedora, RHEL, Rocky, and Alpine targets. Android x64/x86 Full/Mini have authoritative single-loader emulator evidence; Android ARM/ARM64 Full/Mini remain `android-evidence-pending` until physical-device loading passes; `win-x86/full` remains hosted-evidence-pending, `win-x86/mini` is excluded, and macOS is outside the declared matrix.
- Mini excludes DNN, calibration, features, Photo, HighGui, ML, Tracking, Stitching, and other full-only modules.
- Some algorithms require user-supplied models, training data, codecs, GUI backends, or optional OpenCV build features. The library does not silently download them.
- HighGui requires a compatible desktop UI backend and event-thread model. Server, container, and unattended workflows should prefer file or memory encoding.
- Fedora 40 and Alpine 3.20 are exact compatibility targets with ended standard lifecycle; they are not current-lifecycle distribution promises.
- The normalized candidate is intentionally unsigned before upload. NuGet.org must add a `Repository` primary signature owned by `GuojinYan`; the downloaded public package must pass `dotnet nuget verify --all` and exact payload comparison before the release is accepted.
- GitHub Packages initially creates the 29 user-scoped packages as private. Each must be made Public, linked to `guojin-yan/OpenCV-CSharp-API`, and verified byte-for-byte against the reviewed candidate before the Release is created.
- Independent approval, both public package registries, repository-signature evidence, exact GitHub package hashes, and support status must be verified from the published artifacts, not inferred from local candidates or mirror CI.

- 本版本为 preview。兼容性基线会阻止意外漂移，但后续仍会继续增加 API 与 runtime 包。
- 已测量模块分区具有明确的 zero-gap 证据，但不代表整个仓库或全部 OpenCV 已达到 parity。
- 当前 fail-closed 候选包含 29 个包：一个 managed 包，加上 Android x64/x86、Windows x64/ARM64 以及声明的 Ubuntu、Debian、Fedora、RHEL、Rocky、Alpine 目标共 28 个真实支持 runtime 包。Android x64/x86 的 Full/Mini 已有正式单加载器模拟器证据；Android ARM/ARM64 的 Full/Mini 在真机加载通过前仍为 `android-evidence-pending`；`win-x86/full` 仍为 hosted-evidence-pending，`win-x86/mini` 被排除，macOS 位于声明矩阵之外。
- mini 排除 DNN、标定、Features、Photo、HighGui、ML、Tracking、Stitching 和其他 full-only 模块。
- 部分算法需要用户提供模型、训练数据、codec、GUI backend 或 OpenCV 可选构建能力；库不会静默下载这些输入。
- HighGui 需要兼容的桌面 UI backend 和事件线程模型。服务器、容器和无人值守流程应优先使用文件或内存编码。
- Fedora 40 与 Alpine 3.20 是标准生命周期已结束的精确兼容目标，不代表当前生命周期发行版承诺。
- 规范化候选在上传前有意保持未签名；NuGet.org 必须为公开下载包添加 owner 为 `GuojinYan` 的 `Repository` primary signature，并通过 `dotnet nuget verify --all` 与精确 payload 对比。
- GitHub Packages 初次创建 29 个 user-scoped 包时默认为 private；每个包都必须设为 Public、关联 `guojin-yan/OpenCV-CSharp-API`，并与审核 candidate 逐字节验证后才能创建 Release。
- 独立审批、两个公开 package registry、repository-signature 证据、GitHub package 精确 hash 和支持状态必须以发布产物为准，不能从本地候选或 mirror CI 推断。

## Verification / 验证

After installation, start with [Quick Start](quick-start.md), run the package-owned smoke appropriate to the selected profile, and execute at least one representative application workflow. For release evidence and support boundaries, see [Release Candidate Closeout](release-candidate-closeout.md) and [Support And Lifecycle Policy](support-lifecycle-policy.md).

安装后先阅读 [Quick Start](quick-start.md)，运行所选 profile 对应的 package-owned smoke，并至少执行一个代表性应用工作流。发布证据和支持边界见 [Release Candidate Closeout](release-candidate-closeout.md) 与 [Support And Lifecycle Policy](support-lifecycle-policy.md)。
