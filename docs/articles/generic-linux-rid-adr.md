# Generic linux-x64 RID Feasibility ADR

## Status

Accepted as a preview-only P0 decision for `V501-RT-004`. This ADR defines the evidence required to evaluate a generic `linux-x64` package. It does not add `linux-x64` to the active package matrix and does not authorize a generic NuGet package.

## Context

The repository currently publishes distro-specific Linux runtime identities. That is intentional: the native artifact, libc boundary, dependency closure, and consumer userspace are part of the support claim. A process being x64 is not enough to select a generic package, and a successful cross-build is not proof that the resulting ELF runs on another distribution.

The .NET RID catalog distinguishes portable RIDs from distro-specific RIDs and warns that RID graph behavior is a compatibility mechanism rather than a substitute for runtime evidence. The repository therefore keeps custom distro RID mappings in [`runtime-distro-rid-graph.json`](../../packaging/runtime/runtime-distro-rid-graph.json), while this feasibility record remains outside the active graph and package matrix.

## Decision

1. Evaluate a future `linux-x64` package against three existing glibc reference targets: Ubuntu 22.04 x64 as the proposed baseline, Debian 12 x64, and Fedora 40 x64 as cross-distribution checks. Each target must pass both Full and Mini contracts independently.
2. Treat glibc `2.35` as a provisional baseline derived from the oldest active glibc reference target, not as a promise. The producer must measure the lowest `GLIBC_*` symbol required by every shipped ELF; the measured symbol floor, not the distro label, controls promotion.
3. Keep `linux-x64` `preview-only` with `packageIdentityAllowed=false` until the producer, package, independent consumer, native smoke, and dependency-closure evidence is complete on all three references. RT-005 may create an isolated package-shaped artifact using the explicit identities `JYPPX.OpenCV.runtime.linux-x64.preview` and `JYPPX.OpenCV.runtime.linux-x64.preview.mini`; these are candidate artifact names, not active package identities, and publication is hard-disabled.
4. Respect the repository RID graph direction: distro-specific RIDs import `linux-x64`, and `linux-x64` imports `linux`; exact RID assets are preferred before imported fallback assets. This graph relationship does not authorize publication or automatic package selection. Package IDs are explicit; managed package does not auto-select a runtime package, and any preview consumer must opt into the generic identity.
5. Preserve separate Full and Mini package contracts. Full evidence cannot promote Mini, and a Mini package cannot inherit Full's optional-module or DNN claims.

## Required Evidence

- Exact host or container userspace, architecture, libc, compiler, assembler, CMake, and .NET SDK identity.
- ELF export/dependency audit for the wrapper and every shipped OpenCV module, including the measured glibc symbol floor.
- Full and Mini ABI/module/payload contracts, with the existing 2663-function Full and 527-function Mini wrapper baselines intact.
- Same-run managed/runtime package hashes and an independent consumer that restores only those packages.
- Native loader and representative core/imgproc/imgcodecs/videoio smoke without `LD_LIBRARY_PATH`, producer `PATH`, or runtime-root overrides.
- Dependency closure for glibc, libstdc++, codecs, and optional modules, plus deterministic provenance and rollback records.

## Promotion And Rollback

Promotion requires all three reference distributions, both profiles, and every gate in [`runtime-generic-linux-feasibility.json`](../../packaging/runtime/runtime-generic-linux-feasibility.json). A failed or incomplete target remains distro-specific; no package identity, RID graph entry, support-contract entry, or publication manifest is changed implicitly. The rollback path is the current `runtime-package-matrix.json` with its exact distro-specific package identities.

The same contract is the prerequisite for domestic Linux certification. openEuler, UOS, KylinOS, and other C#/.NET platforms may reuse a generic package only after their own userspace, libc, ABI, and package-consumer evidence is independently recorded. Certification alone must not create a vendor-specific package identity.

## Non-goals

- This ADR does not implement a generic package or alter active package selection.
- This ADR does not claim musl, ARM64, headless, macOS, or domestic-platform support.
- This ADR does not treat CPU feature enumeration, a build flag, or a DNN backend name as GPU/OpenCL or runtime portability evidence.
- This ADR does not adopt a manylinux/PEP 600 promise; the provisional glibc floor is measured from shipped ELF symbols and remains tied to the evidence images.

## Machine Contract

The machine-readable contract is [`runtime-generic-linux-feasibility.json`](../../packaging/runtime/runtime-generic-linux-feasibility.json), validated by [`runtime-generic-linux-feasibility.schema.json`](../../packaging/runtime/runtime-generic-linux-feasibility.schema.json) and [`Test-GenericLinuxFeasibilityContract.ps1`](../../scripts/Test-GenericLinuxFeasibilityContract.ps1). The RT-005 preview matrix is [`runtime-generic-linux-preview-matrix.json`](../../packaging/runtime/runtime-generic-linux-preview-matrix.json), with its own schema and [`Test-GenericLinuxPreviewMatrix.ps1`](../../scripts/Test-GenericLinuxPreviewMatrix.ps1) guard. [`New-GenericLinuxPreviewPackage.ps1`](../../scripts/New-GenericLinuxPreviewPackage.ps1) repackages only a real Ubuntu 22.04 baseline runtime-input into the isolated preview identities; it does not publish or alter the active matrix.

## RT-005 Preview Dispatch

The preview chain is an explicit manual dispatch of `.github/workflows/pack.yml`. Set `generic_linux_preview=true`, provide the successful `runtime-input.yml` run IDs in `generic_linux_preview_full_artifact_run_id` and/or `generic_linux_preview_mini_artifact_run_id`, and choose `all`, `full`, or `mini` in `generic_linux_preview_profile`. Each selected profile is bound to its own producer run because the factual Full and Mini artifacts may be produced independently. The producer downloads only `runtime-input-ubuntu.22.04-x64-<profile>`, creates `.preview` runtime identities, and uploads isolated managed/runtime artifacts. Ubuntu 22.04, Debian 12, and Fedora 40 consumers then restore those artifacts and run native smoke independently. The dispatch path has no package-feed or release permission and cannot promote the result automatically.

RT-005 remains incomplete until the hosted run produces producer provenance, deterministic package hashes, dependency/ELF closure, and successful Full/Mini native smoke evidence for all three consumers. A failed or missing hosted run keeps the matrix `preview-only` and leaves the active distro-specific package matrix unchanged.

## 中文决策摘要

本 ADR 将 generic `linux-x64` 定义为“只验证、不发布”的 P0 预览边界。Ubuntu 22.04 x64（glibc 2.35）是暂定基线，Debian 12 和 Fedora 40 用于跨发行版检查；Full 与 Mini 必须分别通过 producer、package、独立 consumer、native smoke、ELF 符号下限和依赖闭包证据。RT-005 可以生成带 `.preview` 后缀的隔离候选包，但这些不是 active package identity，发布始终硬禁止。当前不会把 `linux-x64` 加入 active package matrix，也不会让 managed 包隐式选择 generic runtime 包。

RT-005 通过 `.github/workflows/pack.yml` 的手动 dispatch 执行：`generic_linux_preview=true`，按所选 profile 把成功的 `runtime-input.yml` run ID 分别填入 `generic_linux_preview_full_artifact_run_id` 和/或 `generic_linux_preview_mini_artifact_run_id`，并选择 `all`、`full` 或 `mini`。Full 与 Mini 可以来自不同的真实 producer run，动态矩阵会把每个 profile 绑定到自己的 run ID。producer 只下载 Ubuntu 22.04 x64 的真实 runtime-input，生成带 `.preview` 的隔离包；Ubuntu 22.04、Debian 12、Fedora 40 随后分别还原并执行 native smoke。该路径没有包 feed 或 Release 写权限，也不能自动 promotion。只有三发行版的 Full/Mini hosted producer、确定性包 hash、ELF/依赖闭包和 native smoke 证据全部完成后，RT-005 才能关闭；否则仍保持 preview-only，不修改 active 发行版矩阵。

RID 图的方向是发行版 RID 导入 `linux-x64`，而 `linux-x64` 导入 `linux`；精确 RID 资产优先于导入的 fallback 资产。这个图关系只是 NuGet 资产解析机制，不是兼容性声明。失败时保留现有发行版专用包身份，并将预览证据标记为失败。openEuler、UOS、麒麟、Loongnix 等国产平台仍需在各自 C#/.NET 用户空间中独立完成 ABI、libc、依赖、打包和 consumer 证据，不能由 generic 预览自动推出支持。
