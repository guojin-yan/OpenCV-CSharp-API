# OpenCV CSharp API

OpenCV CSharp API is a .NET binding project for OpenCV. The current packaged OpenCV runtime identity is OpenCV 5.0.0, and the project exposes OpenCV C++ capabilities to C# through a stable native C ABI and a managed object model under the version-neutral `OpenCvSharp` namespace.

OpenCV CSharp API 是面向 OpenCV 的 .NET 封装项目。当前打包的 OpenCV runtime 身份为 OpenCV 5.0.0；项目通过稳定的 native C ABI 间接调用 OpenCV C++，并在 C# 层以版本中立的 `OpenCvSharp` 命名空间提供接近 OpenCV C++ 使用体验的对象模型。

## Goals / 目标

- Wrap OpenCV C++ through a stable C API.
- Provide C# APIs that stay close to OpenCV C++ naming while following .NET conventions.
- Support .NET Framework, .NET Core 3.1, and modern .NET versions from .NET 5 to .NET 10.
- Keep modern .NET fast paths such as `Span<T>`, `MemoryMarshal`, `LibraryImport`, and low-copy data access.
- Publish managed and runtime NuGet packages with versions aligned to OpenCV.

## Target Frameworks / 目标框架

```text
net46;net461;net462;net47;net471;net472;net48;net481;
netcoreapp3.1;
net5.0;net6.0;net7.0;net8.0;net9.0;net10.0
```

## Packages / 包

- Managed API: `JYPPX.OpenCV.CSharp.API`
- Managed assembly: `JYPPX.OpenCV.CSharp.API.dll`
- Public namespace: `OpenCvSharp.*`
- Runtime packages: `JYPPX.OpenCV.runtime.<rid>` for full builds and `JYPPX.OpenCV.runtime.<rid>.mini` for mini builds
- Package version metadata: `OpenCV major.minor.patch.packageRevision`, for example `5.0.0.0`

Use the managed and runtime packages together on the same four-part package version metadata. Consumers should choose the full `JYPPX.OpenCV.runtime.<rid>` package or the smaller `JYPPX.OpenCV.runtime.<rid>.mini` package that matches their exact target RID. The current runtime package matrix covers `win-x64`, `win-x86`, `win-arm64`, distro-specific Linux RID package IDs such as `ubuntu.22.04-x64`, `ubuntu.24.04-x64`, `debian.12-x64`, `fedora.40-x64`, `rhel.9-x64`, `rocky.9-x64`, and `alpine.3.20-x64`, plus `android-arm64`, `android-arm`, `android-x64`, and `android-x86`. Linux runtime packages are built and named per distro/runtime family, so `JYPPX.OpenCV.runtime.ubuntu.22.04-x64` and `JYPPX.OpenCV.runtime.alpine.3.20-x64` are separate package identities. For custom distro-specific Linux RID restore, set `RuntimeIdentifierGraphPath` to `packaging/runtime/runtime-distro-rid-graph.json` or copy that graph into the consuming project before restore.

managed 主包和 runtime 包应使用相同的四段 package version 元数据。消费者应选择与精确 target RID 匹配的 full `JYPPX.OpenCV.runtime.<rid>` 包，或更小的 `JYPPX.OpenCV.runtime.<rid>.mini` 包。当前 runtime package matrix 覆盖 `win-x64`、`win-x86`、`win-arm64`，以及 `ubuntu.22.04-x64`、`ubuntu.24.04-x64`、`debian.12-x64`、`fedora.40-x64`、`rhel.9-x64`、`rocky.9-x64`、`alpine.3.20-x64` 等 distro-specific Linux RID package IDs，并覆盖 `android-arm64`、`android-arm`、`android-x64` 和 `android-x86`。Linux runtime 包按发行版/runtime family 分别构建和命名，因此 `JYPPX.OpenCV.runtime.ubuntu.22.04-x64` 与 `JYPPX.OpenCV.runtime.alpine.3.20-x64` 是不同包身份。使用自定义 distro-specific Linux RID restore 时，请把 `RuntimeIdentifierGraphPath` 指向 `packaging/runtime/runtime-distro-rid-graph.json`，或在 restore 前把该 graph 复制到 consumer project。

The mini profile builds and packages `core,imgproc,imgcodecs,videoio,geometry,flann`. OpenCV 5 split geometry-backed contour, shape, and transform APIs out of imgproc, and `geometry` links `flann`, so both runtime dependencies are required to keep the common imgproc wrapper useful and loadable; DNN, calib, features, photo, highgui, and other full-only modules remain excluded. The native wrapper still compiles only common infrastructure plus core/imgproc/imgcodecs/videoio sources and exports a reduced compatibility ABI; `geometry` and `flann` do not add wrapper source modules. APIs moved to excluded OpenCV 5 modules, including `GoodFeaturesToTrack` in `features`, keep their managed/native entrypoint shape but report `NOT_LINKED` under mini.

mini profile 会构建并打包 `core,imgproc,imgcodecs,videoio,geometry,flann`。OpenCV 5 已把 imgproc 中基于 geometry 的轮廓、形状和变换 API 拆到 `geometry`，而 `geometry` 会链接 `flann`，因此为了保留实用且可加载的常用 imgproc wrapper，mini 必须包含这两个运行时依赖；DNN、calib、features、photo、highgui 等 full-only 模块仍被排除。native wrapper 仍只编译公共基础设施与 core/imgproc/imgcodecs/videoio 源，并导出缩减后的兼容 ABI；`geometry` 与 `flann` 不增加 wrapper 源模块。被 OpenCV 5 移到排除模块的 API（例如位于 `features` 的 `GoodFeaturesToTrack`）保持 managed/native 入口形状，但在 mini 下明确返回 `NOT_LINKED`。

Runtime package template project: `packaging/runtime/JYPPX.OpenCV.runtime`. The matrix lives in `packaging/runtime/runtime-package-matrix.json`; Actions validate the full/mini package surface with synthetic runtime inputs by default, while real publishing requires native wrapper and OpenCV runtime outputs for the selected RID/profile. If no matching published runtime package is available yet, build and stage a local native runtime with `Build-OpenCV.ps1` and `Stage-Runtime.ps1`, then point local samples/tests at it with `OpenCvNativeRuntimeDir` or package it with `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>`.

runtime package 模板项目为 `packaging/runtime/JYPPX.OpenCV.runtime`。矩阵定义在 `packaging/runtime/runtime-package-matrix.json`；Actions 默认使用 synthetic runtime inputs 验证 full/mini package surface，真实发布则必须提供所选 RID/profile 的 native wrapper 与 OpenCV runtime 输出。如果 no matching published runtime package is available yet，请用 `Build-OpenCV.ps1` 和 `Stage-Runtime.ps1` 构建并暂存 local native runtime，然后通过 `OpenCvNativeRuntimeDir` 指向本地样例/测试，或使用 `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>` 打包。

`pack.yml` does not build real runtime inputs. When `validate_synthetic_runtime=false`, real input paths must already exist on the selected runner or come from `real_runtime_artifact_run_id`; that run must contain a neutral `runtime-input-<rid>-<profile>` artifact with `native-wrapper/`, `opencv-runtime/`, `opencv-source/`, and optional `opencv-install/` directories. Synthetic runtime inputs are package-surface validation only; real publishable runtime packages require `SyntheticRuntimeInputs=false` provenance and release preflight.

`pack.yml` 当前不会构建真实 runtime 输入；当 `validate_synthetic_runtime=false` 时，真实输入路径必须已经存在于 selected runner，或来自 `real_runtime_artifact_run_id`；该 run 必须包含中性的 `runtime-input-<rid>-<profile>` artifact，并带有 `native-wrapper/`、`opencv-runtime/`、`opencv-source/` 与可选 `opencv-install/` 目录。synthetic runtime inputs 只用于 package-surface validation；真实可发布 runtime 包必须带有 `SyntheticRuntimeInputs=false` provenance 并通过 release preflight。

`runtime-input.yml` is the first real producer workflow for that handoff. It currently produces `runtime-input-ubuntu.24.04-x64-full` and `runtime-input-ubuntu.24.04-x64-mini` on `ubuntu-24.04`, `runtime-input-ubuntu.22.04-x64-full` on `ubuntu-22.04`, `runtime-input-debian.12-x64-full` inside `debian:12`, `runtime-input-fedora.40-x64-full` inside `fedora:40`, `runtime-input-rhel.9-x64-full` inside the official `registry.access.redhat.com/ubi9/ubi:9.8` image, `runtime-input-rocky.9-x64-full` inside `rockylinux:9`, and `runtime-input-alpine.3.20-x64-full` inside `alpine:3.20` on Ubuntu hosted Docker runners. Each target fetches factual OpenCV source, builds the selected OpenCV profile, links/tests the matching profile of `JYPPX.OpenCV.Native`, records distro/profile/build-list provenance, and uploads the agreed `runtime-input-<rid>-<profile>` layout. The containerized distro producers additionally record hosted-runner, container image, container distro/version, and libc evidence so they are not confused with generic Ubuntu-hosted package-surface runs. Mini producers for all remaining RIDs, including Alpine, stay disabled until their own linked-component and platform boundaries are verified.

The full-only `runtime-input-ubuntu.24.04-arm64-full` producer runs directly on `ubuntu-24.04-arm`. It rejects non-AArch64 execution, records the actual runner image, Ubuntu 24.04, `aarch64`/`arm64`, glibc, CPU, disk, and OpenCV NEON configuration in its handoff provenance, requires linked CTest 5/5, and audits 18 canonical AArch64 ELFs for `$ORIGIN`, zero producer paths, 16 direct OpenCV dependencies, and zero missing dependencies. No x86 CPU workaround is applied. Ubuntu ARM64 mini remains synthetic package-surface validation only.

The Rocky Linux 9 and RHEL UBI 9 producers independently record `-DCMAKE_CXX_FLAGS=-DCV_AVXVNNI_AVAILABLE=0` in their own runtime-input provenance. Both audited GCC 11 / GNU assembler 2.35.2 toolchains reject OpenCV 5.0.0's AVX-VNNI `vpdpbusd` DNN path; the targeted per-producer define leaves AVX2 and the remaining OpenCV CPU-dispatch configuration enabled and is not a global distro-family assumption.

The Alpine producer uses the official `alpine:3.20` image, its `v3.20/main` and `v3.20/community` repositories, `samurai` as the Ninja-compatible build tool, and `linux-headers` required by OpenCV core. Alpine 3.20's audited GCC 13.2.1 / GNU assembler 2.42 path compiles `vpdpbusd`, so its provenance keeps `OpenCvExtraCMakeArgs` empty and never inherits the Rocky/RHEL workaround. The audited image is Alpine 3.20.10 with musl 1.2.5. Alpine 3.20 standard support ended on 2026-04-01 and fixes are now on request, so this RID is an exact compatibility boundary rather than a claim that 3.20 is a current standard-support branch.

`runtime-input.yml` 是该 handoff 的第一条真实 producer workflow。当前它会在 `ubuntu-24.04` 上生产 full 与 mini `runtime-input-ubuntu.24.04-x64-*` artifact，在 `ubuntu-22.04` 上生产 `runtime-input-ubuntu.22.04-x64-full`，并通过 Ubuntu hosted runner 中的 `debian:12`、`fedora:40`、官方 `registry.access.redhat.com/ubi9/ubi:9.8`、`rockylinux:9` 与 `alpine:3.20` 容器分别生产对应的 full artifact。每个目标都会获取事实性 OpenCV 源码、构建所选 OpenCV profile、链接并测试匹配 profile 的 `JYPPX.OpenCV.Native`，记录 distro/profile/build-list provenance，然后上传约定的 `runtime-input-<rid>-<profile>` layout。容器化 distro producer 还会记录 hosted runner、container image、container distro/version 和 libc 证据。其余 RID（包括 Alpine）的 mini producer 会保持禁用，直到各自 linked-component 与平台边界完成并通过验证。

full-only 的 `runtime-input-ubuntu.24.04-arm64-full` producer 会直接运行在 `ubuntu-24.04-arm`。它拒绝非 AArch64 执行，在 handoff provenance 中记录真实 runner image、Ubuntu 24.04、`aarch64`/`arm64`、glibc、CPU、磁盘和 OpenCV NEON 配置，要求 linked CTest 5/5，并审计 18 个 canonical AArch64 ELF 的 `$ORIGIN`、零 producer path、16 条 OpenCV 直接依赖和零缺失依赖。该目标不会使用任何 x86 CPU workaround；Ubuntu ARM64 mini 仍只做 synthetic package-surface validation。

Rocky Linux 9 与 RHEL UBI 9 producer 会分别在自己的 runtime-input provenance 中记录 `-DCMAKE_CXX_FLAGS=-DCV_AVXVNNI_AVAILABLE=0`。两个经过独立审计的 GCC 11 / GNU assembler 2.35.2 工具链都无法汇编 OpenCV 5.0.0 AVX-VNNI DNN 的 `vpdpbusd` 路径；该逐 producer 定向 define 仍保留 AVX2 与其余 OpenCV CPU-dispatch 配置，不是对整个发行版家族的全局假设。

Alpine producer 使用官方 `alpine:3.20`、`v3.20/main`/`v3.20/community` 仓库、提供 Ninja 兼容命令的 `samurai`，以及 OpenCV core 所需的 `linux-headers`。独立审计的 Alpine GCC 13.2.1 / GNU assembler 2.42 能正常编译 `vpdpbusd`，因此其 provenance 中 `OpenCvExtraCMakeArgs` 保持为空，绝不复制 Rocky/RHEL workaround。审计镜像为 Alpine 3.20.10 / musl 1.2.5；Alpine 3.20 已于 2026-04-01 结束常规支持，后续修复为 on request，所以该 RID 是精确兼容边界，不表示 3.20 仍处于当前常规支持期。

Naming policy: package IDs, managed assembly, public namespaces, project paths, and primary native loader stay version-neutral. The current packaged OpenCV runtime identity is expressed through package version metadata and factual runtime filenames. `OpenCv5Sharp.Native.dll` and `jyppx_ocv5_*` remain only as explicit compatibility contracts for already-compiled consumers.

命名策略：包 ID、managed 程序集、公开命名空间、项目路径和主 native loader 都保持版本中立。当前打包的 OpenCV runtime 身份通过 package version 元数据和事实性 runtime 文件名表达。`OpenCv5Sharp.Native.dll` 与 `jyppx_ocv5_*` 仅作为供已编译消费者使用的明确兼容契约保留。

The primary native ABI uses `jyppx_ocv_*` exports and `OPENCV_CSHARP_STATUS_*` status constants. A generated compatibility translation unit forwards every public `jyppx_ocv5_*` export to its neutral implementation, and the source-compatible include tree keeps old wrapper-header identifiers available for existing native code.

主 native ABI 使用 `jyppx_ocv_*` 导出和 `OPENCV_CSHARP_STATUS_*` 状态常量。生成的兼容 translation unit 会把每个公开 `jyppx_ocv5_*` 导出转发到中性实现，source-compatible include 树则为既有 native 代码继续提供旧 wrapper-header 标识。

Current native source and examples should include wrapper headers through `open_cv_sharp/...`. The generated `open_cv_5_sharp/...` tree is retained only as a source-compatibility wrapper surface for existing native code.

当前 native 源码和示例应通过 `open_cv_sharp/...` include wrapper headers。生成的 `open_cv_5_sharp/...` 树仅作为既有 native 代码的源码兼容 wrapper surface 保留。

Runtime NuGet packages do not currently distribute native C headers. The current advanced/source-tree native build surface is `src/OpenCvSharp.Native/include/open_cv_sharp`; `src/OpenCvSharp.Native/include/open_cv_5_sharp` remains only as a compatibility wrapper tree for existing native source includes.

runtime NuGet 包当前不分发 native C headers。当前 advanced/source-tree native build surface 是 `src/OpenCvSharp.Native/include/open_cv_sharp`；`src/OpenCvSharp.Native/include/open_cv_5_sharp` 仅作为既有 native source include 的兼容 wrapper tree 保留。

The native CMake project is currently source-tree build only and does not currently install or export a reusable CMake package or SDK target. The `JYPPX.OpenCV.Native` CMake target is primary; `OpenCv5Sharp.Native` remains only a compatibility alias for existing build scripts and loaders.

native CMake 项目当前只作为 source-tree build surface 使用，当前不 install 或 export 可复用的 CMake package / SDK target。`JYPPX.OpenCV.Native` CMake target 是主目标；`OpenCv5Sharp.Native` 仅作为既有构建脚本和 loader 的兼容 alias 保留。

Native CTest and local build output names are neutral-first. CTest names derive from the primary target, including `JYPPX.OpenCV.NativeSmoke`, `JYPPX.OpenCV.NativeCompatibilitySourceSmoke`, `JYPPX.OpenCV.NativeAbiGeneratedCheck`, `JYPPX.OpenCV.NativeLegacyIncludeParity`, and `JYPPX.OpenCV.NativeAbiExportAudit`; the `OpenCv5Sharp.Native` loader file remains only a compatibility copy.

native CTest 和本地 build output 名称保持 neutral-first。CTest 名称从主 target 派生，包括 `JYPPX.OpenCV.NativeSmoke`、`JYPPX.OpenCV.NativeCompatibilitySourceSmoke`、`JYPPX.OpenCV.NativeAbiGeneratedCheck`、`JYPPX.OpenCV.NativeLegacyIncludeParity` 和 `JYPPX.OpenCV.NativeAbiExportAudit`；`OpenCv5Sharp.Native` loader file 仅作为兼容副本保留。

Native CMake runtime-root/PATH copy is neutral-first. Windows linked builds discover `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT`, copy factual upstream `opencv*.dll` files into `$<TARGET_FILE_DIR:${OPENCV_CSHARP_NATIVE_TARGET}>`, and put that target output directory first in CTest `PATH`; the copied `opencv*.dll` names remain factual upstream artifacts, not project identities.

native CMake runtime-root/PATH copy 保持 neutral-first。Windows linked build 会发现 `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT`，把事实性上游 `opencv*.dll` 文件复制到 `$<TARGET_FILE_DIR:${OPENCV_CSHARP_NATIVE_TARGET}>`，并把该 target output directory 放在 CTest `PATH` 首位；复制的 `opencv*.dll` 名称仍是事实性上游产物，不是项目身份。

The canonical outer workspace root is version-neutral, for example `OpenCV-CSharp-API-workspace`. Generic repository, plan, diary, source-cache, and artifact-root directories do not encode an OpenCV major. Versioned names remain valid below factual dependency caches such as `artifacts/opencv-install/opencv-5.0.0-windows-x64`.

正式的外层工作区根目录保持版本中立，例如 `OpenCV-CSharp-API-workspace`。通用的仓库、计划、日记、源码缓存和产物根目录均不编码 OpenCV major；事实性依赖缓存内部仍可使用版本目录，例如 `artifacts/opencv-install/opencv-5.0.0-windows-x64`。

The primary native loader is `JYPPX.OpenCV.Native.dll`, selected by `NativeLibraryNames.CurrentNativeLibrary` and exposed by build-info `CurrentNativeLibraryName`. `OpenCv5Sharp.Native.dll`, `NativeLibraryNames.LegacyNativeLibrary`, `NativeLibraryName`, and `OpenCv5SharpBuildInfo` remain only as explicitly preserved loader/build-info compatibility names for existing consumers.

主 native loader 为 `JYPPX.OpenCV.Native.dll`，由 `NativeLibraryNames.CurrentNativeLibrary` 选择，并由 build-info `CurrentNativeLibraryName` 暴露。`OpenCv5Sharp.Native.dll`、`NativeLibraryNames.LegacyNativeLibrary`、`NativeLibraryName` 和 `OpenCv5SharpBuildInfo` 仅作为供既有消费者使用、明确保留的 loader/build-info 兼容名称保留。

## Build / 构建

Fast project invariant checks do not require external OpenCV runtime artifacts:

快速项目不变量检查不需要外部 OpenCV runtime 产物：

```powershell
pwsh -NoProfile -File .\scripts\Test-ProjectInvariants.ps1
```

The invariant suite includes public API namespace, consumer-facing naming, package install consumer surface, package metadata, release package artifact surface, path/artifact naming, documentation surface, and workflow/release gate coverage guards, which keep current managed APIs and examples under `OpenCvSharp.*`, keep package install commands, package IDs, assembly names, package artifact labels, generic project-owned paths, DocFX publish surfaces, and CI release gates version-neutral, reject fixed-major package recommendations, and allow `OpenCv5SharpBuildInfo` only as the documented/tested existing-caller compatibility facade.

该不变量套件包含公开 API 命名空间、面向消费者命名、package install consumer 表面、包元数据、release package artifact 表面、路径/产物命名、文档发布面与 workflow/release gate 覆盖守卫，确保当前 managed API 和示例保持在 `OpenCvSharp.*` 下，确保 package install 命令、package ID、程序集名称、包产物标签、通用项目路径、DocFX 发布面和 CI release gate 保持版本中立，拒绝固定大版本包推荐，并且仅允许 `OpenCv5SharpBuildInfo` 作为已文档化、已测试的既有调用方兼容 facade。

```powershell
dotnet restore .\OpenCV-CSharp-API.slnx
dotnet build .\OpenCV-CSharp-API.slnx -c Release
```

For native CMake configuration, prefer version-neutral cache variables such as `OPENCV_CSHARP_OPENCV_DIR`, `OPENCV_CSHARP_OPENCV_BUILD_LIST`, and `OPENCV_CSHARP_BUILD_WITH_OPENCV`. Older `OPENCV5SHARP_*` CMake variables remain accepted only as existing-build-script compatibility aliases, not as public CMake package variables.

native CMake 配置优先使用版本中立的 cache 变量，例如 `OPENCV_CSHARP_OPENCV_DIR`、`OPENCV_CSHARP_OPENCV_BUILD_LIST` 和 `OPENCV_CSHARP_BUILD_WITH_OPENCV`。旧的 `OPENCV5SHARP_*` CMake 变量仍仅作为既有构建脚本的兼容别名接受，不作为 public CMake package variables。

## Smoke Switches / Smoke 开关

Default tests and samples avoid downloads, cameras, GUI windows, real models, and known unstable tiny-data paths. Use `OPENCV_CSHARP_NATIVE_SMOKE=1` for ordinary linked native smoke. Use `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1` only for experimental optional contrib paths that may expose local runtime crashes, currently including BioInspired Retina/tone/transient algorithm smoke. Older `OPENCV5SHARP_*` names remain accepted only as existing-smoke-workflow compatibility aliases.

默认测试和样例避免下载、摄像头、GUI 窗口、真实模型以及已知不稳定的 tiny-data 路径。普通 linked native smoke 使用 `OPENCV_CSHARP_NATIVE_SMOKE=1`。`OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1` 只用于可能暴露本地 runtime crash 的实验性 optional contrib 路径，目前包括 BioInspired Retina/tone/transient 算法 smoke。旧的 `OPENCV5SHARP_*` 名称仍仅作为既有 smoke workflow 的兼容别名保留。

## Pack / 打包

```powershell
pwsh -NoProfile -File .\scripts\Pack-Managed.ps1 -OpenCvVersion 5.0.0 -PackageRevision 0
pwsh -NoProfile -File .\scripts\Pack-Runtime.ps1 -Rid win-x64 -OpenCvVersion 5.0.0 -PackageRevision 0
pwsh -NoProfile -File .\scripts\Pack-Runtime.ps1 -Rid ubuntu.22.04-x64 -RuntimeProfile mini -OpenCvVersion 5.0.0 -PackageRevision 0
```

`Pack-Managed.ps1` accepts `-ProjectPath` as either a repository-relative or absolute project path. Its default is the version-neutral managed project `src\OpenCvSharp\OpenCvSharp.csproj`, and another managed project layout can be selected without changing the script. Its neutral `-OutputDir` parameter also accepts a repository-relative or absolute package output directory; the default remains `artifacts\packages`.

For isolated validation or local package-source dry-runs, `Pack-Managed.ps1` also accepts `-TargetFrameworks`, `-BuildOutputRoot`, and `-RestorePackagesPath`. These parameters only scope MSBuild target-framework selection, `bin`/`obj`, and NuGet restore outputs; they do not change the managed package ID, assembly name, or OpenCV runtime identity carried by package version metadata.

`Pack-Managed.ps1` 的 `-ProjectPath` 可以使用仓库相对路径或绝对项目路径。默认值是版本中立的 managed 项目 `src\OpenCvSharp\OpenCvSharp.csproj`，同时可以在不修改脚本的情况下选择其他 managed 项目布局。中性的 `-OutputDir` 参数也可以使用仓库相对或绝对包输出目录，默认值仍为 `artifacts\packages`。

用于隔离验证或 local package-source dry-run 时，`Pack-Managed.ps1` 也接受 `-TargetFrameworks`、`-BuildOutputRoot` 和 `-RestorePackagesPath`。这些参数只限定 MSBuild target-framework 选择、`bin`/`obj` 和 NuGet restore 输出；不会改变 managed package ID、assembly name，也不会改变由 package version metadata 承载的 OpenCV runtime 身份。

`Pack-Runtime.ps1 -Rid <rid>` derives full runtime package IDs as `JYPPX.OpenCV.runtime.<rid>` and `-RuntimeProfile mini` derives mini package IDs as `JYPPX.OpenCV.runtime.<rid>.mini`. It passes `RuntimePackageRid` and `RuntimePackageProfile` into the generic runtime package project.

`Pack-Runtime.ps1` packs runtime files staged in the generic `packaging/runtime/JYPPX.OpenCV.runtime` project. Its neutral `-RuntimeProject` parameter accepts a repository-relative or absolute runtime project path; the default remains `packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj`. Its neutral `-OutputDir` parameter accepts a repository-relative or absolute package output directory; the default remains `artifacts\packages`. To stage fresh runtime files and then pack in one command, pass `-StageRuntime` with the native wrapper output directory:

`Stage-Runtime.ps1` also exposes a neutral `-OutputRoot` parameter for the standalone runtime staging tree. It accepts a repository-relative or absolute staging root, while the compatibility default remains `artifacts\runtime`. When `Pack-Runtime.ps1 -StageRuntime` is used, its neutral `-StageOutputRoot` parameter forwards to that staging root without changing the package `-OutputDir`.

`Stage-Runtime.ps1 -RuntimeProject` is a repository-relative or absolute runtime package directory used for generated `runtimes/<rid>/native` and `licenses/` mirrors. `Pack-Runtime.ps1 -RuntimeProject` is a repository-relative or absolute runtime package `.csproj` file path used by `dotnet pack`.

The pack workflow exposes `rid` and `runtime_profile` inputs. `all` runs the configured multi-RID matrix for full and mini profiles; individual RID/profile selections are supported for targeted packaging. The workflow uploads neutral `nupkg-*` artifacts from `artifacts/packages`, self-validates the full matrix artifacts with `Test-GitHubPackArtifactMatrixSurface.ps1`, then verifies consumer restore/build behavior with `Test-GitHubPackConsumerRestoreSurface.ps1` against the same downloaded artifacts. Non-synthetic hosted native execution uses an explicit proven allowlist: Ubuntu 24.04 x64 full/mini and Ubuntu 22.04 x64 full, each on its matching Ubuntu runner. Debian 12 full runs in a separate `debian:12` job container, which verifies `/etc/os-release`, Debian version 12, and glibc before restoring and executing the exact same-run `debian.12-x64/full` package. Fedora 40 full runs in its own separate `fedora:40` job container with the equivalent Fedora/version/glibc evidence and exact `fedora.40-x64/full` package boundary. Rocky Linux 9 full runs in a fourth separate `rockylinux:9` job container, verifies Rocky Linux 9 distro/version/glibc evidence, and consumes only the exact same-run `rocky.9-x64/full` package. RHEL 9 full runs in a fifth separate official Red Hat UBI 9 job container using `registry.access.redhat.com/ubi9/ubi:9.8`; it requires factual `ID=rhel`, `platform:el9`, version, glibc, and UBI repository evidence before consuming only `rhel.9-x64/full`. Alpine 3.20 full runs through a separate host-orchestrated `docker run alpine:3.20` verifier: checkout and artifact-download actions remain on the Ubuntu host because their Node runtime is not a musl execution boundary, while package guards, isolated .NET 8 restore/build, loader execution, and deterministic DNN smoke run inside Alpine after explicit 3.20/x86_64/musl evidence. Microsoft's RHEL 9-path RPM feed is a compatible PowerShell source for Fedora and Rocky and the matching tooling feed for RHEL UBI; it is never runtime identity evidence. The independently audited Rocky and RHEL `-DCMAKE_CXX_FLAGS=-DCV_AVXVNNI_AVAILABLE=0` workarounds remain producer provenance and are not consumer-side environment overrides. These runs download only their same-run managed/runtime artifacts, use an isolated NuGet source/cache, and execute native calls through the packaged loader without `LD_LIBRARY_PATH`. Mini keeps its exact 20-file six-module payload and runs core/imgproc/imgcodecs/videoio; full derives its exact payload from matrix-required modules plus provenance-recorded staged optional modules and adds a deterministic DNN smoke. Linux loaders use `$ORIGIN` so packaged OpenCV SONAME dependencies resolve beside `JYPPX.OpenCV.Native` without retaining a producer path; both linked profiles retain their declared OpenCV closure as direct dependencies so edges such as `geometry -> flann` do not rely on transitive RUNPATH lookup. Synthetic runtime inputs are only for non-publishing package-surface validation, and publishing is rejected when synthetic inputs are enabled.

Ubuntu 24.04 ARM64 full runs natively on `ubuntu-24.04-arm` in a separate exact verifier. It requires Ubuntu 24.04, `aarch64`, Debian architecture `arm64`, and glibc evidence, downloads only the same-run `nupkg-managed` and `nupkg-ubuntu.24.04-arm64-full` artifacts, requires `SyntheticRuntimeInputs=false`, and executes the full native smoke including deterministic DNN without `LD_LIBRARY_PATH`. It does not use x64, cross-compilation, QEMU, containers, or an ARM64 mini producer as evidence.

For real non-synthetic workflow runs, `native_runtime_dir`, `opencv_runtime_dir`, and `opencv_source_dir` are existing directories on the selected runner unless `real_runtime_artifact_run_id` is provided. With artifact handoff, `pack.yml` downloads `runtime-input-<rid>-<profile>` into `artifacts/real-runtime-inputs/<rid>-<profile>` and resolves `native-wrapper/`, `opencv-runtime/`, `opencv-source/`, and optional `opencv-install/` before packaging. The workflow validates resolved paths before packaging but does not build them.

To try a real producer/consumer chain, dispatch `runtime-input.yml` with `rid=ubuntu.24.04-x64` and either `runtime_profile=full` or `runtime_profile=mini`; the other currently enabled RIDs accept `runtime_profile=full`. Then dispatch `pack.yml` with the same RID/profile, `validate_synthetic_runtime=false`, `publish_github_packages=false`, and `real_runtime_artifact_run_id=<runtime-input-run-id>`.

```powershell
pwsh -NoProfile -File ./scripts/Pack-Runtime.ps1 -Rid win-x64 -OpenCvVersion 5.0.0 -PackageRevision 0 -StageRuntime -OpenCvNativeRuntimeDir ./build/native-opencv-core/Release
```

The `./build/native-opencv-core/Release` path in the example is the current local native wrapper build-output location, not a package identity or a new generic naming pattern.

Use the version-neutral `-OpenCvNativeRuntimeDir` parameter for new runtime staging commands. The older `-NativeRuntimeDir` parameter remains accepted only as an existing-packaging-script compatibility alias.

The pack scripts derive the four-part package version from `-OpenCvVersion` plus `-PackageRevision`; package IDs stay version-neutral, so the OpenCV runtime identity is carried by package versions and build metadata. `-PackageVersion` remains accepted only as an explicit version-metadata compatibility override, not as a package identity or naming surface.

打包脚本会从 `-OpenCvVersion` 和 `-PackageRevision` 推导四段 package version；包 ID 保持版本中立，因此 OpenCV runtime 身份由包版本和构建元数据承载。`-PackageVersion` 仍仅作为版本元数据的显式兼容覆盖口接受，不作为包身份或命名面。

示例中的 `./build/native-opencv-core/Release` 路径只是当前本地 native wrapper build-output 位置，不是 package 身份或新的通用命名模式。

`Pack-Runtime.ps1 -Rid <rid>` 会把 full runtime package ID 推导为 `JYPPX.OpenCV.runtime.<rid>`；`-RuntimeProfile mini` 会推导 mini package ID `JYPPX.OpenCV.runtime.<rid>.mini`。脚本会把 `RuntimePackageRid` 和 `RuntimePackageProfile` 传给通用 runtime package project。

`Pack-Runtime.ps1` 会打包暂存在通用 `packaging/runtime/JYPPX.OpenCV.runtime` 项目下的 runtime 文件。中性的 `-RuntimeProject` 参数可以使用仓库相对或绝对 runtime 项目路径，默认值仍为 `packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj`。中性的 `-OutputDir` 参数可以使用仓库相对或绝对包输出目录，默认值仍为 `artifacts\packages`。若要先暂存最新 runtime 文件再打包，可传入 `-StageRuntime` 和 native wrapper 输出目录。优先使用版本中立的 `-OpenCvNativeRuntimeDir`；旧的 `-NativeRuntimeDir` 仍仅作为既有 packaging script 的兼容别名接受。

`Stage-Runtime.ps1` 还为独立 runtime staging 目录提供了中性的 `-OutputRoot` 参数。该参数可以使用仓库相对或绝对 staging 根目录，兼容默认值仍为 `artifacts\runtime`。使用 `Pack-Runtime.ps1 -StageRuntime` 时，中性的 `-StageOutputRoot` 参数会转发到该 staging root，且不会改变 package `-OutputDir`。

`Stage-Runtime.ps1 -RuntimeProject` 是仓库相对或绝对 runtime package 目录，用于生成 `runtimes/<rid>/native` 和 `licenses/` 镜像；`Pack-Runtime.ps1 -RuntimeProject` 是供 `dotnet pack` 使用的仓库相对或绝对 runtime package `.csproj` 文件路径。

pack workflow 暴露 `rid` 与 `runtime_profile` 输入。`all` 会运行配置好的 full/mini 多 RID 矩阵；也可以选择单个 RID/profile 做定向打包。workflow 从 `artifacts/packages` 上传中性的 `nupkg-*` 产物，先用 `Test-GitHubPackArtifactMatrixSurface.ps1` 自检 full matrix artifacts，再用 `Test-GitHubPackConsumerRestoreSurface.ps1` 针对同一批下载产物验证 consumer restore/build。真实 non-synthetic hosted verifier 的 allowlist 仍严格限定为 Ubuntu 24.04 x64 full/mini 与 Ubuntu 22.04 x64 full，并在匹配的 Ubuntu runner 上执行。Debian 12 full、Fedora 40 full、Rocky Linux 9 full 分别使用独立的 `debian:12`、`fedora:40` 与 `rockylinux:9` job container；RHEL 9 full 使用第五个独立的官方 Red Hat UBI 9 `registry.access.redhat.com/ubi9/ubi:9.8` job container。四个 glibc 容器 job 都先核验事实发行版、版本与 glibc，RHEL 还要求 `ID=rhel`、`platform:el9` 和 UBI 仓库证据。Alpine 3.20 full 使用另一条由 Ubuntu host 编排的显式 `docker run alpine:3.20` verifier：checkout/artifact download actions 留在 host，artifact guard、隔离 .NET 8 restore/build、loader 与确定性 DNN 执行全部位于核验过 3.20/x86_64/musl 的 Alpine 容器内。所有 verifier 只消费同 run 的精确 RID/full 包，全程不设置 `LD_LIBRARY_PATH`。Microsoft 的 RHEL 9 路径 RPM feed 对 Fedora/Rocky 只是兼容 PowerShell 来源，对 RHEL UBI 是匹配的工具来源，但都不是 runtime identity 证据。Rocky 与 RHEL 独立审计得到的 `-DCMAKE_CXX_FLAGS=-DCV_AVXVNNI_AVAILABLE=0` workaround 只保留在各自 producer provenance 中，不是 consumer 环境覆盖；Alpine 不使用该 workaround。默认仅使用 synthetic runtime inputs 验证 package surface，并在启用 synthetic inputs 时拒绝发布。

Ubuntu 24.04 ARM64 full 使用独立的 `ubuntu-24.04-arm` 原生 verifier。它要求 Ubuntu 24.04、`aarch64`、Debian `arm64` 与 glibc 事实证据，只下载同 run 的 `nupkg-managed` 和 `nupkg-ubuntu.24.04-arm64-full`，要求 `SyntheticRuntimeInputs=false`，并在不设置 `LD_LIBRARY_PATH` 的情况下执行包括确定性 DNN 在内的 full native smoke。x64、交叉编译、QEMU、容器和 ARM64 mini producer 都不能作为这条原生边界的证据。

对于真实 non-synthetic workflow run，除非提供 `real_runtime_artifact_run_id`，否则 `native_runtime_dir`、`opencv_runtime_dir` 和 `opencv_source_dir` 必须是 selected runner 上已存在的目录。使用 artifact handoff 时，`pack.yml` 会把 `runtime-input-<rid>-<profile>` 下载到 `artifacts/real-runtime-inputs/<rid>-<profile>`，并解析 `native-wrapper/`、`opencv-runtime/`、`opencv-source/` 和可选 `opencv-install/` 后再打包。workflow 会在打包前验证解析后的路径，但不会构建这些路径。

要试跑真实 producer/consumer 链路，请先 dispatch `runtime-input.yml`；`rid=ubuntu.24.04-x64` 可选择 `runtime_profile=full` 或 `mini`，其他当前已启用 RID 仍只接受 `full`。随后 dispatch `pack.yml`，使用相同 RID/profile，并设置 `validate_synthetic_runtime=false`、`publish_github_packages=false` 和 `real_runtime_artifact_run_id=<runtime-input-run-id>`。

Absolute `-ProjectPath` and `-RuntimeProject` values are accepted as-is and can point outside the repository when the caller chooses that layout.

绝对 `-ProjectPath` 与 `-RuntimeProject` 会按原样接受；如果调用方选择仓库外项目路径，也可以指向仓库外。

An absolute `-OutputDir` is accepted as-is and can write package artifacts outside the repository when the caller chooses that path.

绝对 `-OutputDir` 会按原样接受；如果调用方选择仓库外路径，包产物也会写到仓库外。

NuGet may normalize `5.0.0.0` package file names to `5.0.0`; non-zero package revisions such as `5.0.0.1` remain visible in the file name.

NuGet 可能会把 `5.0.0.0` 的包文件名规范化为 `5.0.0`；非零打包修订号，例如 `5.0.0.1`，会保留在文件名中。

The pack scripts verify the normalized `.nupkg` artifact path after `dotnet pack` completes.

打包脚本会在 `dotnet pack` 完成后验证规范化后的 `.nupkg` 产物路径。

Before packing, the scripts remove the expected normalized package file if it already exists, so a successful run proves the artifact was produced by the current invocation rather than left over from an earlier build.

打包前，脚本会先删除预期的规范化包文件（如果已存在），因此成功运行可以证明该产物来自本次调用，而不是早先构建遗留。

The optional `-NoBuild` and `-NoRestore` switches only forward `--no-build` and `--no-restore` to `dotnet pack`; stale artifact removal and normalized `.nupkg` verification still run.

可选的 `-NoBuild` 与 `-NoRestore` 开关只会把 `--no-build` 和 `--no-restore` 转发给 `dotnet pack`；旧产物删除和规范化 `.nupkg` 验证仍会执行。

Quick install and minimal usage are covered by the [Quick Start](docs/articles/quick-start.md). Release, runtime staging, smoke, and license details are covered by the [Linked Runtime Build Guide](docs/articles/linked-runtime-build-guide.md), [Linked Runtime Smoke Guide](docs/articles/linked-runtime-smoke-guide.md), [Smoke Profiles Guide](docs/articles/smoke-profiles-guide.md), [Runtime Licenses](docs/articles/runtime-licenses.md), and the [runtime package README](packaging/runtime/JYPPX.OpenCV.runtime/README.md).

快速安装和最小用法见 [Quick Start](docs/articles/quick-start.md)。发布、runtime 暂存、smoke 和 license 细节见 [Linked Runtime Build Guide](docs/articles/linked-runtime-build-guide.md)、[Linked Runtime Smoke Guide](docs/articles/linked-runtime-smoke-guide.md)、[Smoke Profiles Guide](docs/articles/smoke-profiles-guide.md)、[Runtime Licenses](docs/articles/runtime-licenses.md) 以及 [runtime package README](packaging/runtime/JYPPX.OpenCV.runtime/README.md)。

## Current Scope / 当前范围

- `Core`: `Mat` lifetime, ROI/view operations, typed data access, array arithmetic, bitwise operations, statistics, normalization, matrix solve/invert, SVD, RNG, generalized matrix multiplication, vector transforms, spectral transforms, channel split/merge, and layout helpers.
- `ImgCodecs`: encode/decode and file read/write APIs with parameter flags.
- `ImgProc`: color conversion, resize, threshold, filtering, morphology, drawing, geometric transforms, contours, connected components, moments, histograms, Hough features, CLAHE, and line segment detection.
- `Features2D`: feature package with `KeyPoint`, `DMatch`, `Feature2D`, `DescriptorMatcher`, `ORB`, `SIFT`, `FastFeatureDetector`, `GFTTDetector`, `MSER`, `MserRegion`, `SimpleBlobDetector`, `SimpleBlobDetectorParams`, contrib-backed `BRISK`, `KAZE`, `AKAZE`, `AffineFeature`, `BFMatcher`, `FlannBasedMatcher`, common `DefaultName` metadata, descriptor compute/detect-and-compute helpers, batch keypoint detection, `DrawKeypoints`, `DrawMatches`, and `DrawMatchesKnn`. The ABI remains available when OpenCV was built without `opencv_features` or optional `opencv_xfeatures2d`; those calls report a clear `NOT_LINKED` boundary.
- `Calib3D`: geometry and calibration package with `Rodrigues`, `RQDecomp3x3`, projection decomposition, `ProjectPoints`, `SolvePnP`, `SolvePnPRansac`, `SolvePnPGeneric`, PnP refinement, homography/fundamental/essential matrix estimation, essential matrix decomposition, pose recovery, epilines, triangulation, point undistortion, rectification maps, full `CalibrateCamera` / `CalibrateCameraExtended`, stereo calibration, calibrated and uncalibrated stereo rectification, `Rectify3Collinear`, chessboard/circles-grid detection, camera-matrix utilities, and the complete `StereoBM` object wrapper.
- `VideoIO`: video package with complete `VideoCapture` and `VideoWriter` object wrappers, file/device opening, frame grab/retrieve/read, frame writing, backend names, `VideoIORegistry` backend queries, FourCC helpers, capture/writer property enums, and hardware-acceleration enum values.
- `Video`: motion-analysis package with Lucas-Kanade and Farneback optical flow, optical-flow pyramid construction, `.flo` optical-flow file read/write helpers, mean-shift/CamShift tracking, background subtraction objects (`BackgroundSubtractorMOG2` and `BackgroundSubtractorKNN`), and a complete `KalmanFilter` object wrapper.
- `OptFlow`: optional contrib optical-flow package with `DenseOpticalFlow`, `SparseOpticalFlow`, `DualTVL1OpticalFlow`, RLOF parameters and objects, SimpleFlow/SparseToDense/RLOF helpers, and motion-template functions; runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_optflow500.dll` when available and can use the staged XImgProc module for related algorithms.
- `XImgProc`: optional contrib extended image-processing package with `XImgProcCv2` local thresholding, thinning, edge-aware filters, Hough helpers, ridge and recursive gradient helpers, Fourier descriptors and contour fitting, run-length morphology, `GuidedFilter`, `FastGlobalSmootherFilter`, `SuperpixelSLIC`, `SuperpixelSEEDS`, `SuperpixelLSC`, `FastLineDetector`, disparity WLS filtering and metrics, `FastBilateralSolverFilter`, sparse match interpolation, `EdgeDrawing`, `EdgeBoxes`, `ScanSegment`, `GraphSegmentation`, Selective Search strategies, and covariance estimation; runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_ximgproc500.dll` when available.
- `BgSegm`: optional contrib background-segmentation package with `BackgroundSubtractorMOG`, `BackgroundSubtractorGMG`, `BackgroundSubtractorCNT`, a separate `BgSegmBackgroundSubtractor` base, and `SyntheticSequenceGenerator`.
- `Tracking`: optional contrib tracking package with modern `Tracker`, `TrackerKCF`, `TrackerCSRT`, CSRT initial-mask support, OpenCV legacy (`cv::legacy`) `TrackerMOSSE`, `TrackerMIL`, `TrackerMedianFlow`, `MultiTracker`, and separate modern/`cv::legacy` native boundaries.
- `Face`: optional contrib face package with `FaceRecognizer`, `BasicFaceRecognizer`, `EigenFaceRecognizer`, `FisherFaceRecognizer`, `LBPHFaceRecognizer`, `StandardCollector`, `FacePrediction`, `FacePredictionResult`, `BIF`, `Facemark`, `FacemarkTrain`, `FacemarkLBF`, `FacemarkLBFParams`, `FacemarkFitResult`, and `MACE`; runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_face500.dll` when available.
- `Saliency`: optional contrib saliency package with `Saliency`, `StaticSaliency`, `StaticSaliencySpectralResidual`, `StaticSaliencyFineGrained`, `MotionSaliencyBinWangApr2014`, `ObjectnessBING`, `ObjectnessBINGResult`, and `ObjectnessBINGBox`; runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_saliency500.dll` when available.
- `Dnn`: deep-neural-network package with `Net` object wrapping, model path/buffer loading, ONNX/TensorFlow/TFLite/OpenVINO convenience readers, input setup, single and multi-output forward passes, layer-name and metadata queries, profile/FLOPS helpers, blob creation, blob image extraction, DNN backend/target enums, and model-engine selection.
- `HighGui`: guarded window/key package with named windows, image display, wait/poll key helpers, window move/resize, window properties, title/image-rectangle helpers, trackbar/mouse/button callbacks, and default samples/tests that avoid creating windows unless explicitly enabled.
- `Stitching`: high-level panorama package with `Stitcher`, mode/status/wave-correction enums, stitch/estimate/compose calls, component indices, camera parameter output, result masks, and defined `NOT_LINKED` behavior when `opencv_stitching` is unavailable.
- `ObjDetect`: main OpenCV object-detection package with complete `QRCodeDetector`, `BarcodeDetector`, `QRCodeDetectorAruco`, `QRCodeEncoder`, ArUco dictionary/detector/grid-board/ChArUco wrappers, `ArucoDetector.RefineDetectedMarkers`, MCC checker detector/checker wrappers, DNN-backed `FaceDetectorYN`, DNN-backed `FaceRecognizerSF`, QR ECI and encoder enums, DNN backend/target enums, QR/barcode/ArUco/ChArUco result objects, UTF-8 string marshalling, model path and model buffer creation, grouped point-array marshalling, and modern `ReadOnlySpan<byte>` / `ReadOnlySpan<float>` overloads where useful.
- `Photo`: main OpenCV photo package with `Inpaint`, single-frame and multi-frame fast non-local means denoising, colored denoising, decoloring, seamless cloning, local color/illumination/texture editing, edge-preserving/detail/sketch/stylization functions, and `Tonemap` / `TonemapDrago` / `TonemapReinhard` / `TonemapMantiuk` object wrappers.
- `XObjDetect`: optional contrib object-detection package for the local OpenCV 5.0.0 `xobjdetect` module, including `CascadeClassifier`, `HOGDescriptor`, detection result objects, cascade flags, HOG histogram norm enum, and defined `NOT_LINKED` behavior when `opencv_xobjdetect` is unavailable.
- `PtCloud`: main OpenCV depth/RGB-D package with `RescaleDepth`, `DepthTo3d`, `DepthTo3dSparse`, `RegisterDepth`, `WarpFrame`, `FindPlanes`, `RgbdNormals`, and depth/plane method enums.
- `Quality`: optional contrib quality package with `QualityMSE`, `QualityPSNR`, `QualitySSIM`, `QualityGMSD`, `QualityBRISQUE`, quality-map output, scalar score helpers, BRISQUE feature extraction, and model/range-file guarded smoke paths.
- `XPhoto`: optional contrib xphoto package with `WhiteBalancer`, `SimpleWB`, `GrayworldWB`, `LearningBasedWB`, channel gains, DCT denoising, BM3D denoising, oil painting, and BM3D enum wrappers.
- `ML`: contrib-backed OpenCV machine-learning package with `TrainData`, `ParamGrid`, common `StatModel` operations, `KNearest`, `SVM`, `NormalBayesClassifier`, model save/load paths, and tiny-matrix smoke coverage.
- `ImgHash`: optional contrib perceptual-hash package with `ImgHashBase`, `AverageHash`, `PHash`, `BlockMeanHash`, `ColorMomentHash`, `MarrHildrethHash`, `RadialVarianceHash`, and matching one-shot helpers.
- `Plot`: optional contrib plot package with `Plot2d`, Y-series and X/Y-series factories, styling setters, and `Mat` render output.
- `Shape`: optional contrib model-free shape package with `EMDL1`, histogram cost extractors, `ShapeContextDistanceExtractor`, and `HausdorffDistanceExtractor` for tiny descriptor and contour smoke coverage.
- `LineDescriptor`: optional contrib line-descriptor namespace with `KeyLine`, `BinaryDescriptor`, `BinaryDescriptorMatcher`, match/draw helpers, and drawing-flag value types for tiny synthetic line-image smoke coverage.
- `PhaseUnwrapping`: optional contrib phase-unwrapping namespace with `HistogramPhaseUnwrappingParams`, `HistogramPhaseUnwrapping`, `UnwrapPhaseMap`, and inverse reliability-map output for tiny `CV_32FC1` phase-map smoke coverage.
- `StructuredLight`: optional contrib structured-light namespace with `StructuredLightPattern`, `GrayCodePattern`, `SinusoidalPattern`, pattern generation, Gray-code shadow-mask/projection-pixel helpers, and sinusoidal phase/data-modulation helpers.
- `IntensityTransform`: optional contrib image-enhancement namespace with log transform, gamma correction, autoscaling, contrast stretching, and BIMEF wrappers; BIMEF also depends on OpenCV EIGEN support.
- `Fuzzy`: optional contrib fuzzy mathematics namespace with kernel creation, inpaint/filter helpers, and F0/F1 transform helpers for tiny matrix smoke coverage.
- `Hfs`: optional contrib Hierarchical Feature Selection segmentation namespace with `HfsSegmentParams`, `HfsSegment`, seven parameter properties, and CPU/GPU segmentation calls.
- `Reg`: optional contrib image-registration namespace with `RegMap`, `MapShift`, `MapAffine`, `MapProjec`, gradient mappers, `MapperPyramid`, and map warp/inverse/compose/scale calls.
- `SurfaceMatching`: optional contrib 3D registration namespace with `Icp`, `Ppf3DDetector`, flat ICP pose output, and `Pose3DResult` summaries for PPF matches.
- `Rapid`: optional contrib silhouette-tracking namespace with RAPID draw/extract/find/convert/run helpers plus `RapidSilhouetteTracker` and `OlsTracker`.
- `AlphaMat`: optional contrib alpha matting namespace with `AlphaMatCv2.InfoFlow` overloads.
- `BioInspired`: optional contrib bio-inspired vision namespace with `Retina`, `RetinaFastToneMapping`, `TransientAreasSegmentationModule`, and flat parameter value types.
- `XStereo`: optional contrib extended stereo namespace with census descriptor helpers, `StereoBinaryBM`, `StereoBinarySGBM`, `QuasiDenseStereo`, flat match output, and parameter/enum value types.

- `Core`：`Mat` 生命周期、ROI/视图操作、类型化数据访问、数组算术、位运算、统计、归一化、矩阵求解/求逆、SVD、RNG、广义矩阵乘法、向量变换、频谱变换、通道拆合和布局辅助。
- `ImgCodecs`：编码/解码、文件读写和参数标志。
- `ImgProc`：颜色转换、缩放、阈值、滤波、形态学、绘图、几何变换、轮廓、连通域、矩、直方图、霍夫特征、CLAHE 和线段检测。
- `Features2D`：特征模块能力，包含 `KeyPoint`、`DMatch`、`Feature2D`、`DescriptorMatcher`、`ORB`、`SIFT`、`FastFeatureDetector`、`GFTTDetector`、`MSER`、`MserRegion`、`SimpleBlobDetector`、`SimpleBlobDetectorParams`、contrib 版本 `BRISK`、`KAZE`、`AKAZE`、`AffineFeature`、`BFMatcher`、`FlannBasedMatcher`、通用 `DefaultName` 元数据、描述子 compute/detect-and-compute 辅助、批量关键点检测、`DrawKeypoints`、`DrawMatches` 和 `DrawMatchesKnn`。当 OpenCV 构建未包含 `opencv_features` 或可选 `opencv_xfeatures2d` 时，ABI 仍然导出并返回明确的 `NOT_LINKED` 边界。
- `Calib3D`：几何与标定能力包，包含 `Rodrigues`、`RQDecomp3x3`、投影矩阵分解、`ProjectPoints`、`SolvePnP`、`SolvePnPRansac`、`SolvePnPGeneric`、PnP 细化、单应/基础/本质矩阵估计、本质矩阵分解、位姿恢复、极线、三角化、点去畸变、校正映射、完整 `CalibrateCamera` / `CalibrateCameraExtended`、双目标定、已标定和未标定双目校正、`Rectify3Collinear`、棋盘格/圆点阵列检测、相机矩阵工具，以及完整 `StereoBM` 对象封装。
- `VideoIO`：视频模块能力，包含完整 `VideoCapture` 和 `VideoWriter` 对象封装、文件/设备打开、帧 grab/retrieve/read、帧写入、后端名称、`VideoIORegistry` 后端查询、FourCC helper、捕获/写入器属性枚举和硬件加速枚举值。
- `Video`：运动分析能力包，包含 Lucas-Kanade 和 Farneback 光流、光流金字塔构建、`.flo` 光流文件读写 helper、mean-shift/CamShift 跟踪、背景减除对象（`BackgroundSubtractorMOG2` 和 `BackgroundSubtractorKNN`），以及完整 `KalmanFilter` 对象封装。
- `OptFlow`：可选 contrib 光流能力包，包含 `DenseOpticalFlow`、`SparseOpticalFlow`、`DualTVL1OpticalFlow`、RLOF 参数/对象、SimpleFlow/SparseToDense/RLOF helper 和 motion-template 函数；runtime staging 会在可用时包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_optflow500.dll`，并可复用已暂存的 XImgProc 模块处理相关算法。
- `XImgProc`：可选 contrib 扩展图像处理能力包，包含 `XImgProcCv2` 局部阈值、细化、edge-aware filter、Hough helper、ridge 和递归梯度 helper、Fourier descriptor 与轮廓拟合、run-length morphology、`GuidedFilter`、`FastGlobalSmootherFilter`、`SuperpixelSLIC`、`SuperpixelSEEDS`、`SuperpixelLSC`、`FastLineDetector`、disparity WLS 滤波与指标、`FastBilateralSolverFilter`、稀疏匹配插值、`EdgeDrawing`、`EdgeBoxes`、`ScanSegment`、`GraphSegmentation`、Selective Search strategy 和 covariance estimation；runtime staging 会在可用时包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_ximgproc500.dll`。
- `BgSegm`：可选 contrib 背景分割能力包，包含 `BackgroundSubtractorMOG`、`BackgroundSubtractorGMG`、`BackgroundSubtractorCNT`、独立的 `BgSegmBackgroundSubtractor` 基类和 `SyntheticSequenceGenerator`。
- `Tracking`：可选 contrib 跟踪能力包，包含 modern `Tracker`、`TrackerKCF`、`TrackerCSRT`、CSRT 初始 mask、OpenCV legacy（`cv::legacy`）`TrackerMOSSE`、`TrackerMIL`、`TrackerMedianFlow`、`MultiTracker`，并区分 modern/`cv::legacy` native 边界。
- `Face`：可选 contrib face 能力包，包含 `FaceRecognizer`、`BasicFaceRecognizer`、`EigenFaceRecognizer`、`FisherFaceRecognizer`、`LBPHFaceRecognizer`、`StandardCollector`、`FacePrediction`、`FacePredictionResult`、`BIF`、`Facemark`、`FacemarkTrain`、`FacemarkLBF`、`FacemarkLBFParams`、`FacemarkFitResult` 和 `MACE`；runtime staging 会在可用时包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_face500.dll`。
- `Saliency`：可选 contrib 显著性能力包，包含 `Saliency`、`StaticSaliency`、`StaticSaliencySpectralResidual`、`StaticSaliencyFineGrained`、`MotionSaliencyBinWangApr2014`、`ObjectnessBING`、`ObjectnessBINGResult` 和 `ObjectnessBINGBox`；runtime staging 会在可用时包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_saliency500.dll`。
- `Dnn`：深度神经网络能力包，包含 `Net` 对象封装、模型路径/缓冲加载、ONNX/TensorFlow/TFLite/OpenVINO 便捷读取入口、输入设置、单输出和多输出 forward、层名称和元数据查询、profile/FLOPS helper、blob 创建、从 blob 拆图像、DNN backend/target 枚举和模型 engine 选择。
- `HighGui`：受控窗口/key 能力包，包含命名窗口、图像显示、wait/poll key、窗口移动/缩放、窗口属性、标题/图像区域 helper、trackbar/鼠标/按钮回调；默认样例和测试不会创建窗口，除非显式启用。
- `Stitching`：高层全景拼接能力包，包含 `Stitcher`、mode/status/wave-correction 枚举、stitch/estimate/compose 调用、component 索引、相机参数输出、result mask，以及 `opencv_stitching` 不可用时定义明确的 `NOT_LINKED` 行为。
- `ObjDetect`：OpenCV 主线目标检测能力包，包含完整 `QRCodeDetector`、`BarcodeDetector`、`QRCodeDetectorAruco`、`QRCodeEncoder`、ArUco 字典/检测器/网格板/ChArUco 封装、`ArucoDetector.RefineDetectedMarkers`、MCC checker detector/checker 封装、DNN 版本 `FaceDetectorYN`、DNN 版本 `FaceRecognizerSF`、QR ECI 和 encoder 枚举、DNN backend/target 枚举、二维码/条形码/ArUco/ChArUco 结果对象、UTF-8 字符串封送、模型路径和模型缓冲创建、分组点集数组封送，以及必要位置的现代 `ReadOnlySpan<byte>` / `ReadOnlySpan<float>` 重载。
- `Photo`：OpenCV 主线 photo 能力包，包含 `Inpaint`、单帧和多帧 fast non-local means 去噪、彩色去噪、decolor、seamless cloning、局部颜色/光照/纹理编辑、边缘保持/detail/sketch/stylization 函数，以及 `Tonemap` / `TonemapDrago` / `TonemapReinhard` / `TonemapMantiuk` 对象封装。
- `XObjDetect`：可选 contrib 目标检测能力包，对应本地 OpenCV 5.0.0 `xobjdetect` 模块，包含 `CascadeClassifier`、`HOGDescriptor`、检测结果对象、cascade flags、HOG 直方图归一化枚举，以及 `opencv_xobjdetect` 不可用时定义明确的 `NOT_LINKED` 行为。
- `PtCloud`：OpenCV 主线 depth/RGB-D 能力包，包含 `RescaleDepth`、`DepthTo3d`、`DepthTo3dSparse`、`RegisterDepth`、`WarpFrame`、`FindPlanes`、`RgbdNormals` 和 depth/plane 方法枚举。
- `Quality`：可选 contrib 图像质量能力包，包含 `QualityMSE`、`QualityPSNR`、`QualitySSIM`、`QualityGMSD`、`QualityBRISQUE`、质量图输出、标量评分 helper、BRISQUE 特征提取，以及由模型/range 文件环境变量保护的 smoke 路径。
- `XPhoto`：可选 contrib xphoto 能力包，包含 `WhiteBalancer`、`SimpleWB`、`GrayworldWB`、`LearningBasedWB`、channel gains、DCT 去噪、BM3D 去噪、oil painting 和 BM3D 枚举封装。
- `ML`：由 contrib 支撑的 OpenCV 机器学习能力包，包含 `TrainData`、`ParamGrid`、通用 `StatModel` 操作、`KNearest`、`SVM`、`NormalBayesClassifier`、模型保存/加载路径和 tiny matrix smoke 覆盖。
- `ImgHash`：可选 contrib 感知哈希能力包，包含 `ImgHashBase`、`AverageHash`、`PHash`、`BlockMeanHash`、`ColorMomentHash`、`MarrHildrethHash`、`RadialVarianceHash` 以及对应一次性 helper。
- `Plot`：可选 contrib plot 能力包，包含 `Plot2d`、Y 序列与 X/Y 序列工厂、样式 setter，以及渲染到 `Mat` 的输出路径。
- `Shape`：可选 contrib 无模型 shape 能力包，包含 `EMDL1`、直方图代价提取器、`ShapeContextDistanceExtractor` 与 `HausdorffDistanceExtractor`，覆盖 tiny descriptor 与 contour smoke。
- `LineDescriptor`：可选 contrib line_descriptor 命名空间，包含 `KeyLine`、`BinaryDescriptor`、`BinaryDescriptorMatcher`、match/draw helper 和绘制标志值类型，覆盖 tiny 合成线段图 smoke。
- `PhaseUnwrapping`：可选 contrib 相位展开命名空间，包含 `HistogramPhaseUnwrappingParams`、`HistogramPhaseUnwrapping`、`UnwrapPhaseMap` 和 inverse reliability map 输出，覆盖 tiny `CV_32FC1` 相位图 smoke。
- `StructuredLight`：可选 contrib 结构光命名空间，包含 `StructuredLightPattern`、`GrayCodePattern`、`SinusoidalPattern`、图案生成、Gray-code shadow-mask/projector-pixel helper，以及正弦 phase/data-modulation helper。
- `IntensityTransform`：可选 contrib 图像增强命名空间，包含 log transform、gamma correction、autoscaling、contrast stretching 和 BIMEF 封装；BIMEF 还依赖 OpenCV EIGEN 支持。
- `Fuzzy`：可选 contrib fuzzy mathematics 命名空间，包含 kernel 创建、inpaint/filter helper，以及 F0/F1 transform helper，覆盖 tiny matrix smoke。
- `Hfs`：可选 contrib Hierarchical Feature Selection 分割命名空间，包含 `HfsSegmentParams`、`HfsSegment`、七个参数属性和 CPU/GPU 分割调用。
- `Reg`：可选 contrib 图像配准命名空间，包含 `RegMap`、`MapShift`、`MapAffine`、`MapProjec`、梯度 mapper、`MapperPyramid`，以及 map warp/inverse/compose/scale 调用。
- `SurfaceMatching`：可选 contrib 三维配准命名空间，包含 `Icp`、`Ppf3DDetector`、平铺 ICP pose 输出，以及 PPF match 的 `Pose3DResult` 摘要。
- `Rapid`：可选 contrib 轮廓跟踪命名空间，包含 RAPID draw/extract/find/convert/run helper，以及 `RapidSilhouetteTracker` 与 `OlsTracker`。
- `AlphaMat`：可选 contrib alpha matting 命名空间，包含 `AlphaMatCv2.InfoFlow` 重载。
- `BioInspired`：可选 contrib 生物启发视觉命名空间，包含 `Retina`、`RetinaFastToneMapping`、`TransientAreasSegmentationModule` 和平铺参数值类型。
- `XStereo`：可选 contrib 扩展 stereo 命名空间，包含 census descriptor helper、`StereoBinaryBM`、`StereoBinarySGBM`、`QuasiDenseStereo`、平铺 match 输出和参数/枚举值类型。

The native layer is prepared under `src/OpenCvSharp.Native` and is expanded module by module through stable, version-neutral `jyppx_ocv_` C ABI entries.

native 层位于 `src/OpenCvSharp.Native`，通过稳定、版本中立的 `jyppx_ocv_` C ABI 按 OpenCV 模块逐步扩展。

Those names are compatibility contracts. New generic scripts, docs, and package metadata should prefer version-neutral names and refer to OpenCV 5.0.0 only as the current packaged runtime identity or a factual runtime artifact.

这些名称属于兼容契约。新增的通用脚本、文档和包元数据应优先使用版本中立名称，只在描述当前打包 runtime 身份或事实性 runtime 产物时写明 OpenCV 5.0.0。

For scripted native builds, use the version-neutral `-NativeWrapperSourceDir` parameter. Its default is the version-neutral source directory `src/OpenCvSharp.Native`, and another wrapper source layout can be selected without changing the script. The neutral `-BuildDir` parameter accepts a repository-relative or absolute native build directory; its default remains `build/native`.

```powershell
pwsh -NoProfile -File ./scripts/Build-Native.ps1 -NativeWrapperSourceDir ./src/OpenCvSharp.Native
```

脚本化 native 构建应使用版本中立的 `-NativeWrapperSourceDir` 参数。其默认值是版本中立的源码目录 `src/OpenCvSharp.Native`，同时可以在不修改脚本的情况下选择其他 wrapper 源码布局。中性的 `-BuildDir` 参数可以使用仓库相对或绝对 native 构建目录，默认值仍为 `build/native`。

## Code Style / 代码风格

All applications, samples, tests, and tools must use explicit `Program` classes and `Main` methods. Top-level statements are not allowed.

所有应用程序、示例、测试和工具程序都必须显式声明 `Program` 类和 `Main` 方法，不允许使用顶级语句。
