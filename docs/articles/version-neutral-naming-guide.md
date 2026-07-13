# Version-Neutral Naming Guide / 版本中立命名指南

The currently packaged runtime is OpenCV 5.0.0. That version is a package/runtime fact, not the primary identity of generic managed code, projects, loaders, scripts, or documentation.

当前打包的 runtime 是 OpenCV 5.0.0。该版本是包与 runtime 的事实信息，不是通用 managed 代码、项目、loader、脚本或文档的主身份。

## Primary Names / 主名称

| Layer | Primary value | Rule |
| --- | --- | --- |
| Managed package | `JYPPX.OpenCV.CSharp.API` | OpenCV version and package revision belong in package version metadata. |
| Managed assembly | `JYPPX.OpenCV.CSharp.API.dll` | Do not encode an OpenCV major in the assembly filename. |
| Public namespace | `OpenCvSharp.*` | Current source, tests, samples, and generated API docs use this namespace. |
| Runtime package | `JYPPX.OpenCV.runtime.<rid>` | Keep the package ID stable; distinguish OpenCV runtimes with package versions. |
| Native loader | `JYPPX.OpenCV.Native.dll` | Current managed P/Invoke declarations load this file. |
| Native include tree | `open_cv_sharp` | Primary include path for wrapper headers. |
| Native ABI | `jyppx_ocv_*` | Current headers, definitions, and managed entry points use the neutral prefix. |
| Native status | `OPENCV_CSHARP_STATUS_*` | Current implementation code uses neutral status constants. |
| Build variables | `OPENCV_CSHARP_*`, `OpenCvNativeRuntimeDir` | New scripts, tests, and build instructions use neutral names. |

| 层次 | 主值 | 规则 |
| --- | --- | --- |
| Managed 包 | `JYPPX.OpenCV.CSharp.API` | OpenCV 版本与打包修订号放在 package version 元数据中。 |
| Managed 程序集 | `JYPPX.OpenCV.CSharp.API.dll` | 程序集文件名不固化 OpenCV major。 |
| 公开命名空间 | `OpenCvSharp.*` | 当前源码、测试、样例与生成 API 文档统一使用该命名空间。 |
| Runtime 包 | `JYPPX.OpenCV.runtime.<rid>` | 包 ID 保持稳定，通过 package version 区分 OpenCV runtime。 |
| Native loader | `JYPPX.OpenCV.Native.dll` | 当前 managed P/Invoke 声明加载该文件。 |
| Native include 树 | `open_cv_sharp` | wrapper headers 的主 include 路径。 |
| Native ABI | `jyppx_ocv_*` | 当前 headers、definitions 与 managed entry points 使用中性前缀。 |
| Native status | `OPENCV_CSHARP_STATUS_*` | 当前实现代码使用中性状态常量。 |
| 构建变量 | `OPENCV_CSHARP_*`、`OpenCvNativeRuntimeDir` | 新脚本、测试与构建说明统一使用中性名称。 |

## Existing-Consumer Compatibility Names / 既有消费者兼容名称

These names remain only for existing compiled consumers, existing automation/build scripts, or source-compatible native include paths:

以下名称只为既有已编译消费者、既有自动化/构建脚本或源码兼容 native include 路径保留：

- `OpenCv5Sharp.Native.dll`: explicit compatibility loader copy for managed assemblies compiled against earlier package revisions.
- `OpenCv5SharpBuildInfo`: build-info facade for existing callers; new code uses `OpenCvSharpBuildInfo`.
- `OpenCv5Sharp.Native`: compatibility loader copy kept for earlier fixed-major managed consumers.
- `NativeLibraryName`: existing-caller build-info value; new code uses `CurrentNativeLibraryName`.
- `OPENCV5SHARP_*` and `OpenCv5SharpNativeRuntimeDir`: accepted compatibility variables for existing automation, existing build scripts, and existing native include guards.
- `open_cv_5_sharp`: source-compatible include tree for existing native code that includes old wrapper headers.
- `jyppx_ocv5_*` and `OPENCV5SHARP_STATUS_*`: existing exported ABI/status names preserved for compiled and native consumers.

- `OpenCv5Sharp.Native.dll`：供按早期包修订版编译的 managed 程序集使用的明确兼容 loader 副本。
- `OpenCv5SharpBuildInfo`：供既有调用方使用的 build-info facade；新代码使用 `OpenCvSharpBuildInfo`。
- `OpenCv5Sharp.Native`：为早期固定大版本 managed 消费者保留的 compatibility loader 副本。
- `NativeLibraryName`：既有调用方 build-info 值；新代码使用 `CurrentNativeLibraryName`。
- `OPENCV5SHARP_*` 与 `OpenCv5SharpNativeRuntimeDir`：为既有自动化、既有构建脚本和既有 native include guard 保留的兼容变量。
- `open_cv_5_sharp`：供包含旧 wrapper header 的既有 native 代码使用的 source-compatible include 树。
- `jyppx_ocv5_*` 与 `OPENCV5SHARP_STATUS_*`：为已编译消费者和 native 消费者保留的既有导出 ABI/status 名称。

The compatibility ABI is generated, not hand-maintained. `scripts/Generate-NativeAbiCompatibility.ps1` parses every public neutral declaration, writes `generated/legacy_abi.cpp`, writes the `open_cv_5_sharp/legacy_names.h` source aliases, and records the expected symbol pairs in `generated/legacy_abi_manifest.txt`. CTest verifies generated-file freshness, checks neutral/legacy include-tree parity, and loads every neutral export and its generated compatibility counterpart from the built library.

兼容 ABI 通过生成器维护，而不是手工复制。`scripts/Generate-NativeAbiCompatibility.ps1` 会解析每个公开中性声明，生成 `generated/legacy_abi.cpp`、`open_cv_5_sharp/legacy_names.h` 源码别名，以及记录符号对的 `generated/legacy_abi_manifest.txt`。CTest 会校验生成文件是否最新、检查 neutral/legacy include 树是否匹配，并从构建后的库中逐个加载全部中性导出及其生成的兼容对应项。

The previous public namespace `OpenCv5Sharp.*` belongs to earlier package revisions and is discussed only as historical compatibility context. It is not the primary namespace of the current source tree or current generated documentation.

旧公开命名空间 `OpenCv5Sharp.*` 属于早期包修订版，此处仅作为历史兼容背景说明，不再是当前源码树或当前生成文档的主命名空间。

## Version Facts / 版本事实

Keep fixed-major text when it names the actual runtime being packaged or an upstream file:

当文本描述实际打包 runtime 或上游文件时，应保留固定 major：

- `OpenCV 5.0.0` in build output, release notes, and package version metadata.
- `opencv_*500.dll` because those are the real upstream OpenCV 5.0.0 runtime filenames.
- `opencv-5.0.0` source/install directory names when referring to a specific upstream checkout.
- Existing local major-version source directories derived from `-OpenCvVersion` only as compatibility fallbacks when the older checkout path already exists; generic automation should use `opencv-source`.

- 构建输出、发行说明和 package version 元数据中的 `OpenCV 5.0.0`。
- `opencv_*500.dll`，因为它们是 OpenCV 5.0.0 上游 runtime 的真实文件名。
- 指向特定事实性上游 checkout 时使用的 `opencv-5.0.0` 源码或安装目录名。
- 由 `-OpenCvVersion` 推导出的既有本地 major-version 源码目录只在旧 checkout 路径已经存在时作为兼容 fallback；通用自动化应使用 `opencv-source`。

## Rules / 规则

- Do not introduce new generic directories, files, classes, targets, assemblies, loaders, or variables containing an OpenCV major.
- Do not describe accepted compatibility names for existing consumers as preferred or current names.
- Do not rename factual upstream binaries.
- Regenerate DocFX output after namespace changes instead of hand-renaming generated files.
- Audit every remaining `OpenCv5`, `opencv5`, `open_cv_5`, and `OPENCV5` occurrence by category.
- Run `scripts/Test-ProjectInvariants.ps1` for the lightweight invariant guard suite.
- Run `scripts/Test-VersionNeutralNaming.ps1`; it checks both active content and repository path names, with the generated compatibility include tree `src/OpenCvSharp.Native/include/open_cv_5_sharp` as the only fixed-major path-name allowlist entry.
- Run `scripts/Test-PublicApiNamespaceNeutrality.ps1`; it verifies managed source namespaces stay under `OpenCvSharp.*`, rejects `OpenCv5Sharp.*` namespace references, and allows `OpenCv5SharpBuildInfo` only as the documented/tested compatibility facade.
- Run `scripts/Test-ConsumerFacingNaming.ps1`; it verifies samples, tests, docs, workflow snippets, and package README files do not recommend fixed-major package IDs or `OpenCv5Sharp.*` namespaces, and that retained fixed-major mentions are explicitly compatibility-scoped.
- Run `scripts/Test-PackageInstallConsumerSurface.ps1`; it verifies quick-start install commands, issue-template package placeholders, smoke/linked-runtime consumer docs, sample/test runtime-copy properties, matching four-part package version metadata, and fixed-major compatibility context on consumer acquisition surfaces.
- Run `scripts/Test-PackageMetadataNeutrality.ps1`; it verifies managed and runtime project metadata, pack script package IDs, root namespaces, assembly names, and four-part package versions keep package identity version-neutral.
- Run `scripts/Test-ManagedPackageIsolatedArtifactSurface.ps1`; it packs `JYPPX.OpenCV.CSharp.API` with temporary target framework, build output, restore cache, and package output paths, then verifies the normalized `.nupkg`, nuspec ID/version, root `README.md`, `lib/net8.0/JYPPX.OpenCV.CSharp.API.dll`, metadata-only assembly name, and absence of repo output residues.
- Run `scripts/Test-ManagedPackageStandaloneLocalConsumerCompile.ps1`; it packs the managed API into an isolated local package source, restores/builds a temporary consumer that references only `JYPPX.OpenCV.CSharp.API` at four-part package version metadata, and compiles a representative managed API surface across core and selected module namespaces without runtime package or native asset requirements.
- Run `scripts/Test-ReleasePackageArtifactSurface.ps1`; it verifies pack workflow artifact labels, pack/stage script output paths, runtime package project contents, package README/release docs, generated package ignores, normalized `.nupkg` verification, and compatibility/factual context for fixed-major release-surface mentions.
- Run `scripts/Test-PathArtifactNaming.ps1`; it verifies repository path names and path snippets keep generic project-owned roots neutral, allowing fixed-version OpenCV source/install/cache paths only with factual or compatibility context.
- Run `scripts/Test-DocumentationSurfaceNeutrality.ps1`; it verifies DocFX metadata, docs workflow, TOC API entries, ignored generated docs paths, and any present generated DocFX output stay aligned with `OpenCvSharp.*` and neutral documentation identities.
- Run `scripts/Test-WorkflowInvariantCoverage.ps1`; it verifies managed, pack, docs, and native workflows run the aggregate invariant suite before restore, build, pack, DocFX, or CMake work begins, keeps `build-managed` on the native-free representative managed package consumer compile guard instead of an unstaged full `dotnet test`, and keeps GitHub workflow/PR/issue/release surfaces free of fixed-major project identities.
- Run `scripts/Test-ManagedNativeInteropNeutrality.ps1`; it verifies managed imports use `NativeLibraryNames.CurrentNativeLibrary`, neutral `jyppx_ocv_*` entry points, and symbols present in the generated native ABI manifest.
- Run `scripts/Test-RuntimePackageNeutrality.ps1`; it verifies runtime package IDs, staging scripts, staged loader names, and sample/test runtime-copy properties stay neutral-first with fixed-major loader names scoped to explicit compatibility.
- Run `scripts/Test-RuntimeRidPackageTemplateScalability.ps1`; it verifies `JYPPX.OpenCV.runtime.<rid>` and `JYPPX.OpenCV.runtime.<rid>.mini` package IDs, `RuntimePackageRid`/`RuntimePackageProfile`-driven runtime payload paths, pack workflow RID/profile inputs, and the configured runtime package matrix.
- Run `scripts/Test-RuntimeRidConsumerSelectionSurface.ps1`; it verifies consumer install, smoke, issue-template, and runtime-copy guidance tells users to choose `JYPPX.OpenCV.runtime.<rid>` for their target RID while keeping `win-x64` only as the current Windows x64 example.
- Run `scripts/Test-RuntimePackageAvailabilityFallbackGuidance.ps1`; it verifies availability docs track the runtime package matrix, describe full/mini package IDs, keep synthetic validation non-publishable, and point users without a matching runtime package to `Build-OpenCV.ps1`, `Stage-Runtime.ps1`, `OpenCvNativeRuntimeDir`, and `-OpenCvNativeRuntimeDir`.
- Run `scripts/Test-RuntimeFallbackCommandPathConsistency.ps1`; it verifies fallback docs keep the same local native runtime route through `Build-OpenCV.ps1`, `Stage-Runtime.ps1`, `OpenCvNativeRuntimeDir`, and `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>`, while legacy path names remain compatibility-only.
- Run `scripts/Test-RuntimeNativeCopyPropertyPropagation.ps1`; it discovers sample/test project files, verifies `OpenCvNativeRuntimeDir` flows through the MSBuild copy target, and dry-runs synthetic DLL copying outside project `bin`/`obj`, while `OpenCv5SharpNativeRuntimeDir` stays an explicit compatibility alias bridge.
- Run `scripts/Test-RuntimeStagingDryRunIsolation.ps1`; it dry-runs `Stage-Runtime.ps1` with synthetic native wrapper, OpenCV runtime, source, install, absolute `OutputRoot`, and absolute `RuntimeProject` paths outside the repo, then verifies required DLLs, compatibility loader copy, license mirrors, optional-module warnings, and cleanup without creating repo runtime mirrors.
- Run `scripts/Test-RuntimePackStageForwardingIsolation.ps1`; it dry-runs `Pack-Runtime.ps1 -StageRuntime` with synthetic runtime inputs, absolute `StageOutputRoot`, absolute runtime package project, and temporary package output outside the repo, then verifies the package contains staged DLLs and licenses while repo staging/package mirrors remain absent.
- Run `scripts/Test-RuntimePackageLocalConsumerRestore.ps1`; it builds a synthetic runtime package, restores/builds a temporary consumer from an isolated local NuGet source and package cache, then verifies `runtimes/<rid>/native` assets are selected and copied without fixed-major package IDs or repo output residues.
- Run `scripts/Test-ManagedRuntimePackagePairLocalConsumer.ps1`; it packs the managed API and synthetic runtime packages into an isolated local package source, restores/builds a temporary consumer referencing both neutral package IDs at matching four-part version metadata, and verifies managed compile assets plus RID native assets are copied without repo output residues.
- Run `scripts/Test-RuntimeAvailabilityWorkflowReleaseSurface.ps1`; it verifies pack workflow `rid` defaults, `${{ inputs.rid }}` forwarding, neutral `nupkg`/`artifacts/packages` release artifacts, and prevents workflow/release surfaces from implying an active multi-RID release matrix before package projects and release artifacts exist.
- Run `scripts/Test-RuntimePackageDocsDiscoverability.ps1`; it verifies runtime package docs stay cross-linked through [Quick Start](quick-start.md), [Linked Runtime Build Guide](linked-runtime-build-guide.md), [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md), [Smoke Profiles Guide](smoke-profiles-guide.md), [Runtime Licenses](runtime-licenses.md), and the [runtime package README](../../packaging/runtime/JYPPX.OpenCV.runtime/README.md) without hiding `JYPPX.OpenCV.runtime.<rid>`, `JYPPX.OpenCV.runtime.<rid>.mini`, or the no-matching-runtime-package fallback.
- Run `scripts/Test-RuntimeDocLinkIntegrity.ps1`; it verifies runtime docs Markdown links, docs/toc runtime `href` entries, package README back-links, docs/articles package README links, and issue-template plain docs paths resolve from their source locations without using the old fixed-major repository root.

- 不要新增包含 OpenCV major 的通用目录、文件、类、target、程序集、loader 或变量。
- 不要把面向既有消费者接受的兼容名称描述为首选或当前名称。
- 不要重命名事实性的上游二进制文件。
- 命名空间变化后应重新生成 DocFX 输出，而不是手工改生成文件名。
- 对每个剩余的 `OpenCv5`、`opencv5`、`open_cv_5` 和 `OPENCV5` 出现位置按类别审计。
- 运行 `scripts/Test-ProjectInvariants.ps1` 作为轻量项目不变量守卫套件。
- 运行 `scripts/Test-VersionNeutralNaming.ps1`；它会同时检查活跃内容与仓库路径名，且只把生成的兼容 include 树 `src/OpenCvSharp.Native/include/open_cv_5_sharp` 作为固定 major 路径名 allowlist。
- 运行 `scripts/Test-PublicApiNamespaceNeutrality.ps1`；它会校验 managed 源码命名空间保持在 `OpenCvSharp.*` 下，拒绝 `OpenCv5Sharp.*` namespace 引用，并且只允许 `OpenCv5SharpBuildInfo` 作为已文档化、已测试的兼容 facade。
- 运行 `scripts/Test-ConsumerFacingNaming.ps1`；它会校验 samples、tests、docs、workflow snippets 与 package README 不推荐固定大版本 package ID 或 `OpenCv5Sharp.*` namespace，并要求保留的固定大版本提及都明确处于兼容语境。
- 运行 `scripts/Test-PackageInstallConsumerSurface.ps1`；它会校验 quick-start 安装命令、issue template 包占位符、smoke/linked-runtime 消费者文档、sample/test runtime-copy 属性、匹配的四段 package version 元数据，以及 consumer acquisition 表面的固定 major 兼容语境。
- 运行 `scripts/Test-PackageMetadataNeutrality.ps1`；它会校验 managed 与 runtime 项目元数据、pack 脚本 package ID、root namespace、程序集名称和四段 package version 都让包身份保持版本中立。
- 运行 `scripts/Test-ManagedPackageIsolatedArtifactSurface.ps1`；它会用临时 target framework、build output、restore cache 和 package output 路径打包 `JYPPX.OpenCV.CSharp.API`，并校验规范化 `.nupkg`、nuspec ID/version、根目录 `README.md`、`lib/net8.0/JYPPX.OpenCV.CSharp.API.dll`、仅元数据读取的程序集名称，以及不留下仓库输出残留。
- 运行 `scripts/Test-ManagedPackageStandaloneLocalConsumerCompile.ps1`；它会把 managed API 打入隔离 local package source，restore/build 只引用 `JYPPX.OpenCV.CSharp.API` 且使用四段 package version 元数据的临时 consumer，并编译覆盖 core 与 selected module namespaces 的代表性 managed API surface，同时不要求 runtime package 或 native asset。
- 运行 `scripts/Test-ReleasePackageArtifactSurface.ps1`；它会校验 pack workflow 产物标签、pack/stage 脚本输出路径、runtime package project 内容、package README/release 文档、生成包产物 ignore、规范化 `.nupkg` 验证，以及固定 major 发布表面提及是否带有兼容/事实语境。
- 运行 `scripts/Test-PathArtifactNaming.ps1`；它会校验仓库路径名和路径示例让通用项目根保持中立，只允许带事实性或兼容语境的固定版本 OpenCV source/install/cache 路径。
- 运行 `scripts/Test-DocumentationSurfaceNeutrality.ps1`；它会校验 DocFX metadata、docs workflow、TOC API 入口、被忽略的生成文档路径，以及任何已存在的生成 DocFX 输出都与 `OpenCvSharp.*` 和中性文档身份保持一致。
- 运行 `scripts/Test-WorkflowInvariantCoverage.ps1`；它会校验 managed、pack、docs 与 native workflow 在 restore、build、pack、DocFX 或 CMake 工作开始前运行总不变量套件，确保 `build-managed` 使用不需要 native 的代表性 managed package consumer compile guard 而不是未暂存 native runtime 的完整 `dotnet test`，并确保 GitHub workflow/PR/issue/release 表面没有固定 major 项目身份。
- 运行 `scripts/Test-ManagedNativeInteropNeutrality.ps1`；它会校验 managed imports 使用 `NativeLibraryNames.CurrentNativeLibrary`、中性的 `jyppx_ocv_*` entry points，并且这些符号存在于生成的 native ABI manifest 中。
- 运行 `scripts/Test-RuntimePackageNeutrality.ps1`；它会校验 runtime package ID、staging 脚本、暂存 loader 名称以及 samples/tests 的 runtime-copy 属性保持中性优先，固定 major loader 名称只出现在明确兼容语境中。
- 运行 `scripts/Test-RuntimeRidPackageTemplateScalability.ps1`；它会校验 `JYPPX.OpenCV.runtime.<rid>` 与 `JYPPX.OpenCV.runtime.<rid>.mini` package ID、由 `RuntimePackageRid`/`RuntimePackageProfile` 驱动的 runtime payload 路径、pack workflow RID/profile 输入，以及配置好的 runtime package matrix。
- 运行 `scripts/Test-RuntimeRidConsumerSelectionSurface.ps1`；它会校验 consumer install、smoke、issue template 和 runtime-copy guidance 要求用户按 target RID 选择 `JYPPX.OpenCV.runtime.<rid>`，并确保 `win-x64` 仅作为当前 Windows x64 示例保留。
- 运行 `scripts/Test-RuntimePackageAvailabilityFallbackGuidance.ps1`；它会校验 availability docs 是否跟踪 runtime package matrix、是否描述 full/mini package ID、是否确保 synthetic validation 不可发布，以及是否把没有匹配 runtime 包的用户引导到 `Build-OpenCV.ps1`、`Stage-Runtime.ps1`、`OpenCvNativeRuntimeDir` 和 `-OpenCvNativeRuntimeDir`。
- 运行 `scripts/Test-RuntimeFallbackCommandPathConsistency.ps1`；它会校验 fallback docs 是否保持同一条 local native runtime 路线：`Build-OpenCV.ps1`、`Stage-Runtime.ps1`、`OpenCvNativeRuntimeDir` 和 `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>`，并确保 legacy path 名称只作为兼容项保留。
- 运行 `scripts/Test-RuntimeNativeCopyPropertyPropagation.ps1`；它会动态发现 sample/test project files，校验 `OpenCvNativeRuntimeDir` 会流入 MSBuild copy target，并在项目 `bin`/`obj` 外 dry-run 合成 DLL 复制，而 `OpenCv5SharpNativeRuntimeDir` 只作为明确的 compatibility alias bridge 保留。
- 运行 `scripts/Test-RuntimeStagingDryRunIsolation.ps1`；它会使用仓库外的合成 native wrapper、OpenCV runtime、source、install、绝对 `OutputRoot` 和绝对 `RuntimeProject` 路径干跑 `Stage-Runtime.ps1`，并校验 required DLL、兼容 loader copy、license mirror、optional-module warning 与清理流程，同时不创建仓库 runtime mirror。
- 运行 `scripts/Test-RuntimePackStageForwardingIsolation.ps1`；它会使用合成 runtime 输入、仓库外绝对 `StageOutputRoot`、绝对 runtime package project 和临时 package 输出干跑 `Pack-Runtime.ps1 -StageRuntime`，并校验 package 包含暂存 DLL 与 license，同时不创建仓库 staging/package mirror。
- 运行 `scripts/Test-RuntimePackageLocalConsumerRestore.ps1`；它会构建合成 runtime package，并从隔离 local NuGet source 与 package cache restore/build 临时 consumer，然后校验 `runtimes/<rid>/native` assets 被选择并复制，且没有固定 major package ID 或仓库输出残留。
- 运行 `scripts/Test-ManagedRuntimePackagePairLocalConsumer.ps1`；它会把 managed API 与合成 runtime package 打入隔离 local package source，restore/build 同时引用两个中性 package ID 且使用相同四段版本元数据的临时 consumer，并校验 managed compile asset 与 RID native asset 被复制且不留下仓库输出残留。
- 运行 `scripts/Test-RuntimeAvailabilityWorkflowReleaseSurface.ps1`；它会校验 pack workflow 的 `rid` 默认值、`${{ inputs.rid }}` 转发、中性的 `nupkg`/`artifacts/packages` 发布产物，并防止 workflow/release 表面在 package projects 与 release artifacts 存在前暗示已有 active multi-RID release matrix。
- 运行 `scripts/Test-RuntimePackageDocsDiscoverability.ps1`；它会校验 runtime package docs 是否通过 [Quick Start](quick-start.md)、[Linked Runtime Build Guide](linked-runtime-build-guide.md)、[Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md)、[Smoke Profiles Guide](smoke-profiles-guide.md)、[Runtime Licenses](runtime-licenses.md) 和[runtime package README](../../packaging/runtime/JYPPX.OpenCV.runtime/README.md) 保持交叉链接，同时不隐藏 `JYPPX.OpenCV.runtime.<rid>`、`JYPPX.OpenCV.runtime.<rid>.mini` 或 no-matching-runtime-package fallback。
- 运行 `scripts/Test-RuntimeDocLinkIntegrity.ps1`；它会校验 runtime docs 的 Markdown links、docs/toc runtime `href` entries、package README back-links、docs/articles package README links 以及 issue-template plain docs paths 是否能从各自源位置解析，并避免使用旧的固定 major repository root。

Future OpenCV 4 or OpenCV 6 runtime packages should reuse the same managed package, assembly, namespace, loader, project, and script identities. Runtime differences belong in package versions, native build inputs, and factual runtime filenames.

未来的 OpenCV 4 或 OpenCV 6 runtime 包应复用相同的 managed 包、程序集、命名空间、loader、项目和脚本身份。runtime 差异只应体现在 package version、native 构建输入和事实性 runtime 文件名中。

## Workspace Layout / 工作区布局

The outer workspace root is also a generic project identity and must remain version-neutral. A preferred layout is shown below; the versioned install directory is a factual upstream cache, not a project-owned identity:

外层工作区根目录同样属于通用项目身份，也必须保持版本中立。推荐布局如下；其中带版本的安装目录是事实性上游缓存，不是项目自有身份：

```text
OpenCV-CSharp-API-workspace/
  OpenCV-CSharp-API/
  plan/
  diary/
  opencv-source/
  artifacts/
    opencv-install/
      opencv-5.0.0-windows-x64/  # factual upstream cache
```

`OpenCV-CSharp-API-workspace` is generic and neutral. `opencv-5.0.0-windows-x64` is intentionally versioned because it identifies one factual upstream installation.

`OpenCV-CSharp-API-workspace` 是通用且中性的工作区名称。`opencv-5.0.0-windows-x64` 则有意保留版本，因为它标识一个事实性的上游安装。

This neutral layout is now the canonical workspace structure. The one-time relocation helper has been retired; maintained scripts must discover the workspace from repository-relative inputs or explicit neutral path parameters rather than embedding a fixed-major outer directory.

该中性布局现已成为正式工作区结构。一次性迁移脚本已经退役；维护中的脚本必须通过仓库相对路径或显式的中性路径参数发现工作区，不得嵌入固定 major 的外层目录。
