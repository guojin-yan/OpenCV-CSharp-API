# Version-Neutral Naming Guide / 版本中立命名指南

The currently packaged runtime is OpenCV 5.0.0. That version is a package/runtime fact, not the primary identity of generic managed code, projects, loaders, scripts, or documentation.

当前打包的 runtime 是 OpenCV 5.0.0。该版本是包与 runtime 的事实信息，不是通用 managed 代码、项目、loader、脚本或文档的主身份。

## Primary Names / 主名称

| Layer | Primary value | Rule |
| --- | --- | --- |
| Managed package | `JYPPX.OpenCV.CSharp.API` | OpenCV version and package revision belong in package version metadata. |
| Managed assembly | `JYPPX.OpenCV.CSharp.API.dll` | Do not encode an OpenCV major in the assembly filename. |
| Public namespace | `JYPPX.OpenCvSharp.*` | Current source, tests, samples, and generated API docs use this namespace. |
| Runtime package | `JYPPX.OpenCV.runtime.<rid>` | Keep the package ID stable; distinguish OpenCV runtimes with package versions. |
| Native loader | `JYPPX.OpenCV.Native.dll` | Current managed P/Invoke declarations load this file. |
| Native CMake target | `JYPPX.OpenCV.Native` | Current source-tree build target; not a public install/export package surface. |
| Native CTest/output names | `JYPPX.OpenCV.Native*` | Local CTest smoke/audit targets and primary native output names are neutral-first. |
| Native runtime-root/PATH copy | `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT` | Windows linked CMake builds copy factual `opencv*.dll` artifacts into the neutral target output and put that target output directory first on CTest `PATH`. |
| Native include tree | `open_cv_sharp` | Primary include path for current wrapper headers and examples. |
| Native ABI | `jyppx_ocv_*` | Current headers, definitions, and managed entry points use the neutral prefix. |
| Native status | `OPENCV_CSHARP_STATUS_*` | Current implementation code uses neutral status constants. |
| Build variables | `OPENCV_CSHARP_*`, `OpenCvNativeRuntimeDir` | New scripts, tests, and build instructions use neutral names. |

| 层次 | 主值 | 规则 |
| --- | --- | --- |
| Managed 包 | `JYPPX.OpenCV.CSharp.API` | OpenCV 版本与打包修订号放在 package version 元数据中。 |
| Managed 程序集 | `JYPPX.OpenCV.CSharp.API.dll` | 程序集文件名不固化 OpenCV major。 |
| 公开命名空间 | `JYPPX.OpenCvSharp.*` | 当前源码、测试、样例与生成 API 文档统一使用该命名空间。 |
| Runtime 包 | `JYPPX.OpenCV.runtime.<rid>` | 包 ID 保持稳定，通过 package version 区分 OpenCV runtime。 |
| Native loader | `JYPPX.OpenCV.Native.dll` | 当前 managed P/Invoke 声明加载该文件。 |
| Native CMake target | `JYPPX.OpenCV.Native` | 当前 source-tree build target；不是 public install/export package surface。 |
| Native CTest/output names | `JYPPX.OpenCV.Native*` | 本地 CTest smoke/audit targets 和主 native output 名称保持 neutral-first。 |
| Native runtime-root/PATH copy | `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT` | Windows linked CMake build 会把事实性 `opencv*.dll` 产物复制到中性 target output，并把该 target output directory 放在 CTest `PATH` 首位。 |
| Native include 树 | `open_cv_sharp` | wrapper headers 的主 include 路径。 |
| Native ABI | `jyppx_ocv_*` | 当前 headers、definitions 与 managed entry points 使用中性前缀。 |
| Native status | `OPENCV_CSHARP_STATUS_*` | 当前实现代码使用中性状态常量。 |
| 构建变量 | `OPENCV_CSHARP_*`、`OpenCvNativeRuntimeDir` | 新脚本、测试与构建说明统一使用中性名称。 |

## First-Release Boundary / 首版边界

The first public release has no fixed-major project compatibility layer. Managed code loads only `JYPPX.OpenCV.Native`; native consumers include only `open_cv_sharp`, and exported wrapper symbols use only `jyppx_ocv_*`. Windows linked CMake builds use `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT`, copy factual `opencv*.dll` artifacts into the neutral target output directory, and put that directory first on CTest `PATH`.

首个公开版本不提供固定 major 的项目兼容层。Managed 代码只加载 `JYPPX.OpenCV.Native`，native consumer 只包含 `open_cv_sharp`，wrapper 导出符号只使用 `jyppx_ocv_*`。Windows linked CMake build 使用 `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT`，把事实性 `opencv*.dll` 产物复制到中性 target output directory，并把该目录放在 CTest `PATH` 首位。
## Version Facts / 版本事实

Keep fixed-major text when it names the actual runtime being packaged or an upstream file:

当文本描述实际打包 runtime 或上游文件时，应保留固定 major：

- `OpenCV 5.0.0` in build output, release notes, and package version metadata.
- `opencv_*500.dll` because those are the real upstream OpenCV 5.0.0 runtime filenames.
- `opencv-5.0.0` source/install directory names when referring to a specific upstream checkout.
- The repository-owned OpenCV checkout uses the version-neutral `opencv-source` workspace directory.

- 构建输出、发行说明和 package version 元数据中的 `OpenCV 5.0.0`。
- `opencv_*500.dll`，因为它们是 OpenCV 5.0.0 上游 runtime 的真实文件名。
- 指向特定事实性上游 checkout 时使用的 `opencv-5.0.0` 源码或安装目录名。
- 仓库管理的 OpenCV checkout 统一使用版本中立的 `opencv-source` workspace 目录。

## Rules / 规则

- Do not introduce new generic directories, files, classes, targets, assemblies, loaders, or variables containing an OpenCV major.
- Do not add fixed-major project compatibility names before the first public release.
- Do not rename factual upstream binaries.
- Regenerate DocFX output after namespace changes instead of hand-renaming generated files.
- Audit every remaining `OpenCv5`, `opencv5`, `open_cv_5`, and `OPENCV5` occurrence by category.
- Run `scripts/Test-ProjectInvariants.ps1` for the lightweight invariant guard suite.
- Run `scripts/Test-PackageInstallConsumerSurface.ps1`; it verifies quick-start install commands, issue-template package placeholders, smoke/linked-runtime consumer docs, sample/test runtime-copy properties, matching package version metadata, and neutral consumer acquisition surfaces.
- Run `scripts/Test-PackageMetadataNeutrality.ps1`; it verifies managed and runtime project metadata, pack script package IDs, root namespaces, assembly names, and four-part package versions keep package identity version-neutral.
- Run `scripts/Test-ManagedPackageIsolatedArtifactSurface.ps1`; it packs `JYPPX.OpenCV.CSharp.API` with temporary target framework, build output, restore cache, and package output paths, then verifies the normalized `.nupkg`, nuspec ID/version, root `README.md`, `lib/net8.0/JYPPX.OpenCV.CSharp.API.dll`, metadata-only assembly name, and absence of repo output residues.
- Run `scripts/Test-ManagedPackageStandaloneLocalConsumerCompile.ps1`; it packs the managed API into an isolated local package source, restores/builds a temporary consumer that references only `JYPPX.OpenCV.CSharp.API` at four-part package version metadata, and compiles a representative managed API surface across core and selected module namespaces without runtime package or native asset requirements.
- Run `scripts/Test-ReleasePackageArtifactSurface.ps1`; it verifies pack workflow artifact labels, pack/stage script output paths, runtime package project contents, generated runtime provenance manifest packaging, package README/release docs, generated package ignores, normalized `.nupkg` verification, and compatibility/factual context for fixed-major release-surface mentions.
- Run `scripts/Test-PathArtifactNaming.ps1`; it verifies repository path names and path snippets keep generic project-owned roots neutral, allowing fixed-version OpenCV source/install/cache paths only with factual or compatibility context.
- Run `scripts/Test-DocumentationSurfaceNeutrality.ps1`; it verifies DocFX metadata, docs workflow, TOC API entries, ignored generated docs paths, and any present generated DocFX output stay aligned with `JYPPX.OpenCvSharp.*` and neutral documentation identities.
- Run `scripts/Test-WorkflowInvariantCoverage.ps1`; it verifies managed, pack, docs, and native workflows run the aggregate invariant suite before restore, build, pack, DocFX, or CMake work begins, keeps `build-managed` on the native-free representative managed package consumer compile guard instead of an unstaged full `dotnet test`, and keeps GitHub workflow/PR/issue/release surfaces free of fixed-major project identities.
- Run `scripts/Test-GitHubActionSupplyChainBoundary.ps1`; it requires every official Action to use an immutable commit from an audited stable release, keeps the workflow comment aligned with that release major, and records Node.js 24 for every JavaScript action. The composite Pages uploader is bound to its immutable Node.js 24 `upload-artifact` dependency instead of a movable transitive tag.
- Run `scripts/Test-ManagedNativeInteropNeutrality.ps1`; it verifies managed imports use `NativeLibraryNames.CurrentNativeLibrary`, neutral `jyppx_ocv_*` entry points, and symbols present in the generated native ABI manifest.
- Run `scripts/Test-RuntimePackageNeutrality.ps1`; it verifies runtime package IDs, staging scripts, the single staged loader name, and sample/test runtime-copy properties stay version-neutral.
- Run `scripts/Test-RuntimeRidPackageTemplateScalability.ps1`; it verifies `JYPPX.OpenCV.runtime.<rid>` and `JYPPX.OpenCV.runtime.<rid>.mini` package IDs, `RuntimePackageRid`/`RuntimePackageProfile`-driven runtime payload paths, pack workflow RID/profile inputs, and the configured runtime package matrix.
- Run `scripts/Test-RuntimeRidConsumerSelectionSurface.ps1`; it verifies consumer install, smoke, issue-template, and runtime-copy guidance tells users to choose `JYPPX.OpenCV.runtime.<rid>` for their target RID while keeping `win-x64` only as the current Windows x64 example.
- Run `scripts/Test-RuntimePackageAvailabilityFallbackGuidance.ps1`; it verifies availability docs track the runtime package matrix, describe full/mini package IDs, keep synthetic validation non-publishable, and point users without a matching runtime package to `Build-OpenCV.ps1`, `Stage-Runtime.ps1`, `OpenCvNativeRuntimeDir`, and `-OpenCvNativeRuntimeDir`.
- Run `scripts/Test-RuntimeFallbackCommandPathConsistency.ps1`; it verifies fallback docs keep the same local native runtime route through `Build-OpenCV.ps1`, `Stage-Runtime.ps1`, `OpenCvNativeRuntimeDir`, and `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>`, with no unpublished aliases.
- Run `scripts/Test-RuntimeNativeCopyPropertyPropagation.ps1`; it verifies `OpenCvNativeRuntimeDir` reaches sample/test build targets and copies only the neutral loader plus factual OpenCV runtime files.
- Run `scripts/Test-RuntimeStagingDryRunIsolation.ps1`; it dry-runs `Stage-Runtime.ps1` with synthetic native wrapper, OpenCV runtime, source, install, absolute `OutputRoot`, and absolute `RuntimeProject` paths outside the repo, then verifies required DLLs, the single neutral loader, license mirrors, optional-module warnings, and cleanup without creating repo runtime mirrors.
- Run `scripts/Test-RuntimePackStageForwardingIsolation.ps1`; it dry-runs `Pack-Runtime.ps1 -StageRuntime` with synthetic runtime inputs, absolute `StageOutputRoot`, absolute runtime package project, and temporary package output outside the repo, then verifies the package contains staged DLLs and licenses while repo staging/package mirrors remain absent.
- Run `scripts/Test-RuntimeReleaseCandidatePreflightGuard.ps1`; it validates that release-shaped runtime staging passes preflight, runs `Pack-Runtime.ps1 -StageRuntime -RequireReleasePreflight` in an isolated temporary runtime package project, verifies the package output, and confirms synthetic provenance plus stale runtime mirrors are rejected before publish-capable runtime package paths.
- Run `scripts/Test-ReleaseCandidateProvenance.ps1`; it creates and verifies deterministic package-level provenance, explicit signing/SBOM readiness, rollback abort metadata, and non-publishing policy, with negative fixtures for hash, entry, RID/profile, feed, manifest, and publication drift.
- Run `scripts/Test-ReleaseReadinessContract.ps1`, `scripts/Test-ReleaseSigningBoundary.ps1`, and `scripts/Test-NuGetRepositorySigningBoundary.ps1`; the first-preview policy keeps normalized packages `repository-signing-pending`, excludes project private keys, and requires NuGet.org Repository-signature plus exact payload verification after upload. The legacy author-signing state machine remains guarded only as an optional future policy.
- Run `scripts/Test-ReleasePackageSbom.ps1`; it guards deterministic SPDX-2.3 generation from exact normalized unsigned packages, source/runtime provenance, complete file relationships, byte-for-byte `-Check`, and 17 negative fixtures without private keys or remote mutation.
- Run `scripts/Test-PublicFeedVerificationContract.ps1`; it verifies the NuGet v3 service index and exact package URL with HTTPS-only read-only GET/HEAD checks, distinguishes `404` not-published evidence, and rejects HTTP, credentials, mutable paths, wrong identity, and upload commands.
- Run `scripts/Test-ReleaseChangeControlRecord.ps1`; its default fixture guards the schema, while explicit package/SBOM/output inputs create and byte-check a durable `current-unsigned-candidate` / `generated-unapproved` review record that keeps signing, approval, rollback, and publication fail-closed.
- Run `scripts/Test-RealRuntimePackInputBoundary.ps1`; it verifies `pack.yml` rejects synthetic publishing, validates existing or artifact-handoff real runtime input directories before non-synthetic packaging, documents that the workflow does not build real runtime inputs, and keeps `SyntheticRuntimeInputs=false` plus release preflight as the publishable runtime boundary.
- Run `scripts/Test-RealRuntimeInputProducerSurface.ps1`; it verifies the first real `runtime-input.yml` producer builds OpenCV, links/tests `JYPPX.OpenCV.Native`, uploads the neutral `runtime-input-ubuntu.24.04-x64-full` handoff layout, and does not use synthetic runtime inputs or package publishing.
- Run `scripts/Test-RuntimePackageLocalConsumerRestore.ps1`; it builds a synthetic runtime package, restores/builds a temporary consumer from an isolated local NuGet source and package cache, then verifies `runtimes/<rid>/native` assets are selected and copied without fixed-major package IDs or repo output residues.
- Run `scripts/Test-ManagedRuntimePackagePairLocalConsumer.ps1`; it packs the managed API and synthetic runtime packages into an isolated local package source, restores/builds a temporary consumer referencing both neutral package IDs at matching four-part version metadata, and verifies managed compile assets plus RID native assets are copied without repo output residues.
- Run `scripts/Test-RuntimeAvailabilityWorkflowReleaseSurface.ps1`; it verifies pack workflow `rid` defaults, `${{ inputs.rid }}` forwarding, neutral `nupkg`/`artifacts/packages` release artifacts, and prevents workflow/release surfaces from implying an active multi-RID release matrix before package projects and release artifacts exist.
- Run `scripts/Test-RealNativeRuntimeBuildMatrixCoverage.ps1`; it verifies `Build-OpenCV.ps1 -DescribeOnly` maps every runtime package RID/profile to an explicit real OpenCV build plan, and checks RID-aware staging defaults for representative Windows, Linux, and Android runtime inputs.
- Run `scripts/Test-RuntimePackageDocsDiscoverability.ps1`; it verifies runtime package docs stay cross-linked through [Quick Start](quick-start.md), [Linked Runtime Build Guide](linked-runtime-build-guide.md), [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md), [Smoke Profiles Guide](smoke-profiles-guide.md), [Runtime Licenses](runtime-licenses.md), and the [runtime package README](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/JYPPX.OpenCV.runtime/README.md) without hiding `JYPPX.OpenCV.runtime.<rid>`, `JYPPX.OpenCV.runtime.<rid>.mini`, or the no-matching-runtime-package fallback.
- Run `scripts/Test-NativeRuntimeRootPathCopyBoundary.ps1`; it verifies native runtime-root/PATH copy logic uses `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT`, copies factual `opencv*.dll` artifacts into the neutral target output directory, and puts that directory first on CTest `PATH`.
- Run `scripts/Test-RuntimeDocLinkIntegrity.ps1`; it verifies runtime docs Markdown links, docs/toc runtime `href` entries, package README back-links, docs/articles package README links, and issue-template plain docs paths resolve from their source locations without using the old fixed-major repository root.

- 不要新增包含 OpenCV major 的通用目录、文件、类、target、程序集、loader 或变量。
- 不要把面向既有消费者接受的兼容名称描述为首选或当前名称。
- 不要重命名事实性的上游二进制文件。
- 命名空间变化后应重新生成 DocFX 输出，而不是手工改生成文件名。
- 对每个剩余的 `OpenCv5`、`opencv5`、`open_cv_5` 和 `OPENCV5` 出现位置按类别审计。
- 运行 `scripts/Test-ProjectInvariants.ps1` 作为轻量项目不变量守卫套件。
- 运行 `scripts/Test-PackageInstallConsumerSurface.ps1`；它会校验 quick-start 安装命令、issue template 包占位符、smoke/linked-runtime 消费者文档、sample/test runtime-copy 属性、匹配的四段 package version 元数据，以及 consumer acquisition 表面的固定 major 兼容语境。
- 运行 `scripts/Test-PackageMetadataNeutrality.ps1`；它会校验 managed 与 runtime 项目元数据、pack 脚本 package ID、root namespace、程序集名称和四段 package version 都让包身份保持版本中立。
- 运行 `scripts/Test-ManagedPackageIsolatedArtifactSurface.ps1`；它会用临时 target framework、build output、restore cache 和 package output 路径打包 `JYPPX.OpenCV.CSharp.API`，并校验规范化 `.nupkg`、nuspec ID/version、根目录 `README.md`、`lib/net8.0/JYPPX.OpenCV.CSharp.API.dll`、仅元数据读取的程序集名称，以及不留下仓库输出残留。
- 运行 `scripts/Test-ManagedPackageStandaloneLocalConsumerCompile.ps1`；它会把 managed API 打入隔离 local package source，restore/build 只引用 `JYPPX.OpenCV.CSharp.API` 且使用四段 package version 元数据的临时 consumer，并编译覆盖 core 与 selected module namespaces 的代表性 managed API surface，同时不要求 runtime package 或 native asset。
- 运行 `scripts/Test-ReleasePackageArtifactSurface.ps1`；它会校验 pack workflow 产物标签、pack/stage 脚本输出路径、runtime package project 内容、生成 runtime provenance manifest 打包、package README/release 文档、生成包产物 ignore、规范化 `.nupkg` 验证，以及固定 major 发布表面提及是否带有兼容/事实语境。
- 运行 `scripts/Test-PathArtifactNaming.ps1`；它会校验仓库路径名和路径示例让通用项目根保持中立，只允许带事实性或兼容语境的固定版本 OpenCV source/install/cache 路径。
- 运行 `scripts/Test-DocumentationSurfaceNeutrality.ps1`；它会校验 DocFX metadata、docs workflow、TOC API 入口、被忽略的生成文档路径，以及任何已存在的生成 DocFX 输出都与 `JYPPX.OpenCvSharp.*` 和中性文档身份保持一致。
- 运行 `scripts/Test-WorkflowInvariantCoverage.ps1`；它会校验 managed、pack、docs 与 native workflow 在 restore、build、pack、DocFX 或 CMake 工作开始前运行总不变量套件，确保 `build-managed` 使用不需要 native 的代表性 managed package consumer compile guard 而不是未暂存 native runtime 的完整 `dotnet test`，并确保 GitHub workflow/PR/issue/release 表面没有固定 major 项目身份。
- 运行 `scripts/Test-GitHubActionSupplyChainBoundary.ps1`；它要求每个官方 Action 使用来自已审计稳定 release 的不可变 commit，让 workflow 注释与该 release major 保持一致，并为每个 JavaScript action 记录 Node.js 24。复合 Pages uploader 绑定其不可变的 Node.js 24 `upload-artifact` 依赖，而不是可移动的传递 tag。
- 运行 `scripts/Test-ManagedNativeInteropNeutrality.ps1`；它会校验 managed imports 使用 `NativeLibraryNames.CurrentNativeLibrary`、中性的 `jyppx_ocv_*` entry points，并且这些符号存在于生成的 native ABI manifest 中。
- 运行 `scripts/Test-RuntimePackageNeutrality.ps1`；它会校验 runtime package ID、staging 脚本、暂存 loader 名称以及 samples/tests 的 runtime-copy 属性保持中性优先，固定 major loader 名称只出现在明确兼容语境中。
- 运行 `scripts/Test-RuntimeRidPackageTemplateScalability.ps1`；它会校验 `JYPPX.OpenCV.runtime.<rid>` 与 `JYPPX.OpenCV.runtime.<rid>.mini` package ID、由 `RuntimePackageRid`/`RuntimePackageProfile` 驱动的 runtime payload 路径、pack workflow RID/profile 输入，以及配置好的 runtime package matrix。
- 运行 `scripts/Test-RuntimeRidConsumerSelectionSurface.ps1`；它会校验 consumer install、smoke、issue template 和 runtime-copy guidance 要求用户按 target RID 选择 `JYPPX.OpenCV.runtime.<rid>`，并确保 `win-x64` 仅作为当前 Windows x64 示例保留。
- 运行 `scripts/Test-RuntimePackageAvailabilityFallbackGuidance.ps1`；它会校验 availability docs 是否跟踪 runtime package matrix、是否描述 full/mini package ID、是否确保 synthetic validation 不可发布，以及是否把没有匹配 runtime 包的用户引导到 `Build-OpenCV.ps1`、`Stage-Runtime.ps1`、`OpenCvNativeRuntimeDir` 和 `-OpenCvNativeRuntimeDir`。
- 运行 `scripts/Test-RuntimeFallbackCommandPathConsistency.ps1`；它会校验 fallback docs 是否保持同一条 local native runtime 路线：`Build-OpenCV.ps1`、`Stage-Runtime.ps1`、`OpenCvNativeRuntimeDir` 和 `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>`，并确保未发布别名不存在。
- 运行 `scripts/Test-RuntimeNativeCopyPropertyPropagation.ps1`；它会校验 `OpenCvNativeRuntimeDir` 能传递到 sample/test build target，并且只复制中性 loader 与事实性 OpenCV runtime 文件。
- 运行 `scripts/Test-RuntimeStagingDryRunIsolation.ps1`；它会使用仓库外的合成 native wrapper、OpenCV runtime、source、install、绝对 `OutputRoot` 和绝对 `RuntimeProject` 路径干跑 `Stage-Runtime.ps1`，并校验 required DLL、唯一中性 loader、license mirror、optional-module warning 与清理流程，同时不创建仓库 runtime mirror。
- 运行 `scripts/Test-RuntimePackStageForwardingIsolation.ps1`；它会使用合成 runtime 输入、仓库外绝对 `StageOutputRoot`、绝对 runtime package project 和临时 package 输出干跑 `Pack-Runtime.ps1 -StageRuntime`，并校验 package 包含暂存 DLL 与 license，同时不创建仓库 staging/package mirror。
- 运行 `scripts/Test-RuntimeReleaseCandidatePreflightGuard.ps1`；它会验证 release-shaped runtime staging 可以通过 preflight，在隔离 temporary runtime package project 中运行 `Pack-Runtime.ps1 -StageRuntime -RequireReleasePreflight` 并校验 package 输出，同时确认 synthetic provenance 与 stale runtime mirror 会在 publish-capable runtime package 路径前被拒绝。
- 运行 `scripts/Test-ReleaseCandidateProvenance.ps1`；它会验证 package-level 确定性 provenance、明确的 signing/SBOM readiness、rollback abort metadata 与 non-publishing policy，并用 hash、entry、RID/profile、feed、manifest、publication 负向 fixture 检查漂移。
- 运行 `scripts/Test-ReleaseReadinessContract.ps1`、`scripts/Test-ReleaseSigningBoundary.ps1` 与 `scripts/Test-NuGetRepositorySigningBoundary.ps1`；首版策略把规范化包保持为 `repository-signing-pending`，排除项目 private key，并要求上传后验证 NuGet.org Repository signature 与完整 payload。旧 author-signing 状态机仅作为未来可选策略继续守卫。
- 运行 `scripts/Test-ReleasePackageSbom.ps1`；它会守卫从精确规范化未签名包生成确定性 SPDX-2.3、source/runtime provenance、完整 file relationship、逐字节 `-Check` 与 17 个负向 fixture，且不接触 private key 或远程状态。
- 运行 `scripts/Test-PublicFeedVerificationContract.ps1`；它会通过 HTTPS-only 的 GET/HEAD 只读检查验证 NuGet v3 service index 与精确 package URL，区分 `404` not-published evidence，并拒绝 HTTP、credential、可变路径、错误 identity 和 upload command。
- 运行 `scripts/Test-ReleaseChangeControlRecord.ps1`；默认 fixture 会守卫 schema，显式 package/SBOM/output 输入则会创建并逐字节检查 durable `current-unsigned-candidate` / `generated-unapproved` review record，同时保持 signing、approval、rollback 与 publication fail-closed。
- 运行 `scripts/Test-RealRuntimePackInputBoundary.ps1`；它会校验 `pack.yml` 会拒绝 synthetic publishing，在 non-synthetic packaging 前验证真实 runtime 输入目录已经存在或来自 artifact handoff，文档说明 workflow 不会构建真实 runtime 输入，并把 `SyntheticRuntimeInputs=false` 加 release preflight 作为可发布 runtime 边界。
- 运行 `scripts/Test-RealRuntimeInputProducerSurface.ps1`；它会校验第一条真实 `runtime-input.yml` producer 会构建 OpenCV，链接/测试 `JYPPX.OpenCV.Native`，上传中性的 `runtime-input-ubuntu.24.04-x64-full` handoff layout，并且不使用 synthetic runtime inputs 或 package publishing。
- 运行 `scripts/Test-RuntimePackageLocalConsumerRestore.ps1`；它会构建合成 runtime package，并从隔离 local NuGet source 与 package cache restore/build 临时 consumer，然后校验 `runtimes/<rid>/native` assets 被选择并复制，且没有固定 major package ID 或仓库输出残留。
- 运行 `scripts/Test-ManagedRuntimePackagePairLocalConsumer.ps1`；它会把 managed API 与合成 runtime package 打入隔离 local package source，restore/build 同时引用两个中性 package ID 且使用相同四段版本元数据的临时 consumer，并校验 managed compile asset 与 RID native asset 被复制且不留下仓库输出残留。
- 运行 `scripts/Test-RuntimeAvailabilityWorkflowReleaseSurface.ps1`；它会校验 pack workflow 的 `rid` 默认值、`${{ inputs.rid }}` 转发、中性的 `nupkg`/`artifacts/packages` 发布产物，并防止 workflow/release 表面在 package projects 与 release artifacts 存在前暗示已有 active multi-RID release matrix。
- 运行 `scripts/Test-RealNativeRuntimeBuildMatrixCoverage.ps1`；它会校验 `Build-OpenCV.ps1 -DescribeOnly` 是否把每个 runtime package RID/profile 映射到明确的真实 OpenCV build plan，并抽样检查 Windows、Linux 和 Android runtime 输入的 RID-aware staging 默认值。
- 运行 `scripts/Test-RuntimePackageDocsDiscoverability.ps1`；它会校验 runtime package docs 是否通过 [Quick Start](quick-start.md)、[Linked Runtime Build Guide](linked-runtime-build-guide.md)、[Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md)、[Smoke Profiles Guide](smoke-profiles-guide.md)、[Runtime Licenses](runtime-licenses.md) 和[runtime package README](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/JYPPX.OpenCV.runtime/README.md) 保持交叉链接，同时不隐藏 `JYPPX.OpenCV.runtime.<rid>`、`JYPPX.OpenCV.runtime.<rid>.mini` 或 no-matching-runtime-package fallback。
- 运行 `scripts/Test-NativeRuntimeRootPathCopyBoundary.ps1`；它会校验 native runtime-root/PATH copy 逻辑使用 `OPENCV_CSHARP_OPENCV_RUNTIME_ROOT`，把事实性 `opencv*.dll` 产物复制到中性 target output directory，并把该目录放在 CTest `PATH` 首位。
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
