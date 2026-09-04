# Release Candidate Closeout

The current candidate is a local, deterministic preflight only. The generated record is `packaging/release/local-release-candidate-closeout.json`; `scripts/Test-ReleaseCandidateFinalCloseout.ps1` verifies its source-set digest, evidence hashes, API/ABI baselines, support contract, and fail-closed release states. Candidate identity is derived only from the normalized source-set SHA-256, so committing an already verified source set does not invalidate the record through a changing Git HEAD.

## Local Checklist

- [x] Managed package and runtime package identities remain version-neutral.
- [x] Native full/mini ABI manifests and managed API baseline are generated and checked for deterministic drift.
- [x] The bounded OpenCV 5.0.0 ImgProc map classifies all 203 parsed declarations, with 161 implemented callables, zero measured gaps, 6 intentional omissions, and no repository-wide parity claim.
- [x] Runtime support is classified as 29 real, 4 pending, 1 excluded, with macOS outside the matrix and Android ARM/ARM64 awaiting physical-device evidence.
- [x] Deterministic package normalization runs before the NuGet.org repository-signing boundary.
- [x] The stable-release signing strategy is `nuget.org-repository-signing`; no author certificate or project private key is required.
- [x] Structured and cryptographic verification bind the public Repository signature to owner `GuojinYan` and compare every non-signature payload entry with the frozen unsigned package.
- [x] Formal publication binds the same support-contract-derived package candidate to NuGet.org and GitHub Packages, with exact-byte, public-visibility, and authoritative-repository verification before Release.
- [x] Android x64/x86 Full and Mini single-loader payloads execute package-owned native calls in ABI-matched hosted emulators; retired dual-loader runs are retained only as superseded history.
- [x] Deterministic SPDX-2.3 generation and byte-for-byte check mode are guarded with 17 negative fixtures.
- [x] Provenance, license, runtime payload, change-control, rollback, and read-only public-feed evidence are locally validated.
- [x] .NET 10 SDK resolution, DocFX `2.78.5`, workflow syntax, actionlint, and aggregate invariant checks are local gates.
- [x] Stable managed package validation compares `5.0.0` against the published `5.0.0-preview.1` package API baseline.
- [x] Main Video ECC and TrackerMIL callables are closed, with 138 implemented callables, zero measured gaps, and 7 intentional omissions in the bounded Video partition.
- [x] Hosted `win-x86/full` producer-to-X86-consumer evidence, including artifact digests, PE/I386 closure, and the WoW64 probe.
- [ ] Final-commit package-bound SPDX-2.3 documents and dry-run publication bundle.
- [x] Protected `nuget-production` Environment and API-key custody are configured; no secret value is recorded in source, logs, or review artifacts.
- [x] Designated publisher `Guojin Yan` and the explicitly authorized, version-bounded stable `5.0.0` single-maintainer exception with approver recorded as `not-available`.
- [ ] Public visibility and authoritative repository link for every package in the final support-contract-derived candidate.
- [x] Auditable stable-release owner risk acceptance explicitly records that no independent reviewer is available; exact candidate bytes still require a separate dry-run token authorization.
- [ ] Exact candidate publication authorization, stable Release, and public installation verification.

## Stop Conditions

Stop immediately on source or artifact hash drift, synthetic runtime inputs in a publish-capable path, incomplete PE/ELF closure, producer/search-path overrides, missing license or provenance evidence, a non-Repository or wrong-owner NuGet signature, payload drift outside `.signature.p7s`, private or incorrectly linked GitHub Packages, GitHub package byte drift, unapproved SBOM/publication state, missing Environment approval, or any attempt to publish from the compilation mirror.

## Current Stable Package Contract

The current release channel uses strict pack input `5.0.0.0` and normalized NuGet identity `5.0.0`. `5.0.0-preview.1` is already published and immutable, so its package identities, hashes, tag, and Release cannot be reused for the corrected ML runtime. The pack scripts accept exactly four numeric source components plus an optional lowercase SemVer prerelease suffix. Missing revision components, uppercase or malformed labels, numeric prerelease identifiers with leading zeroes, build metadata, OpenCV version drift, and package revision drift fail before `dotnet pack` runs.

Local real-input win-x64 full and mini stable-candidate packages have passed release preflight, exact artifact inspection, isolated restore/build, and package-owned native smoke. Full proved `core,imgproc,imgcodecs,videoio,dnn`; mini proved `core,imgproc,imgcodecs,videoio,not_linked`. These are local candidate-path results, not publication approval. Final normalized unsigned package bytes and hashes must be regenerated from the final release commit before dry-run review or upload.

After rebuilding the normalized unsigned packages from the final release commit, generate one SPDX document per exact package and immediately verify it with `-Check`:

```powershell
pwsh -NoProfile -File ./scripts/New-ReleasePackageSbom.ps1 -PackagePath <normalized-unsigned.nupkg> -SourceCommit <40-hex-final-commit> -Created <factual-UTC-timestamp> -OpenCvVersion 5.0.0 -OutputPath <package.spdx.json>
pwsh -NoProfile -File ./scripts/New-ReleasePackageSbom.ps1 -PackagePath <normalized-unsigned.nupkg> -SourceCommit <40-hex-final-commit> -Created <same-factual-UTC-timestamp> -OpenCvVersion 5.0.0 -OutputPath <package.spdx.json> -Check
pwsh -NoProfile -File ./scripts/Test-ReleasePackageSbom.ps1
pwsh -NoProfile -File ./scripts/Test-NuGetPublicationManifest.ps1 -ManifestPath <input-manifest.json> -SourceCommit <40-hex-final-commit> -PackageVersion 5.0.0 -OutputPath <publication-manifest.json>
pwsh -NoProfile -File ./scripts/Test-ReleaseChangeControlRecord.ps1 -PackageRoot <package-root> -SbomRoot <sbom-root> -OutputPath <release-change-control.json> -Created <same-factual-UTC-timestamp> -ExpectedPackageCount 30
pwsh -NoProfile -File ./scripts/Test-ReleaseChangeControlRecord.ps1 -PackageRoot <package-root> -SbomRoot <sbom-root> -OutputPath <release-change-control.json> -Created <same-factual-UTC-timestamp> -ExpectedPackageCount 30 -Check
pwsh -NoProfile -File ./scripts/New-NuGetPublicationBundle.ps1 -PackageRoot <package-root> -SbomRoot <sbom-root> -ChangeControlPath <release-change-control.json> -SourceCommit <40-hex-final-commit> -PackageVersion 5.0.0 -Created <same-factual-UTC-timestamp> -PublicationManifestPath <publication-manifest.json> -OutputPath <nuget-publication-bundle.json>
```

The generators reject signed or nondeterministically normalized inputs, repository/source drift, unsafe archive entries, synthetic runtime provenance, version/OpenCV/license drift, and stale SBOM bytes. The change-control record classifies exact packages as `current-unsigned-candidate`, SBOMs as `generated-unapproved`, and signing as `repository-signing-pending`. `New-NuGetPublicationBundle.ps1` emits a candidate-specific `publish-nuget:sha256:<hash>` token that binds both public registries. A dry run keeps publication phases false. The upload run uses `publish=true` only with that exact token and the protected `nuget-production` Environment. The normal approval path requires named distinct publisher/approver identities. The explicitly authorized stable `5.0.0` path requires `single_maintainer_exception=true`, publisher `Guojin Yan`, approver `not-available`, dispatcher `guojin-yan`, and an exact candidate token; it records explicit owner risk acceptance and cannot authorize another version. After all GitHub package pages are Public, a separate run uses `verify_publication=true` and may use `create_github_release=true`.

NuGet.org adds the primary Repository signature only after upload. `Test-NuGetRepositorySignedPackage.ps1` runs `dotnet nuget verify --all`, uses `NuGet.Packaging` to require a Repository signature from `https://api.nuget.org/v3/index.json` owned by `GuojinYan`, requires a trusted timestamp, and compares every unsigned payload entry byte-for-byte. The public package SHA256 is expected to differ because `.signature.p7s` is added; both prepublication and repository-signed hashes remain evidence. GitHub Packages must return the exact reviewed unsigned archive SHA256, be Public, and link to `guojin-yan/OpenCV-CSharp-API`. GitHub Release creation is downstream of both 30/30 verification jobs and attaches the repository-signed packages, unsigned-payload SPDX documents, normalized publication manifest, and both registry verification reports.

当前发布渠道使用严格打包输入 `5.0.0.0`，对应 NuGet 规范身份 `5.0.0`。`5.0.0-preview.1` 已公开发布且不可覆盖，因此不能复用其包身份、hash、tag 或 Release 来承载修正后的 ML runtime。使用真实 win-x64 输入生成的 full/mini 包已通过 release preflight、产物检查、隔离还原/构建和包内 native smoke；这些结果只证明本地稳定版路径，不代表精确候选已获发布授权。dry-run 审核或上传前，必须基于最终 release commit 重新生成并核对最终规范化未签名包字节与哈希。

最终提交的规范化未签名包生成后，必须为全部 30 个候选包生成并 `-Check` SPDX-2.3、完整 publication manifest、durable change-control 和 NuGet publication bundle。集合只能包含一个 managed 包和 `realSupport` 中的全部 29 个 runtime 包；pending、excluded 与 synthetic 目标必须失败。状态分别保持 `current-unsigned-candidate`、`generated-unapproved` 与 `repository-signing-pending`。首次 `publish-nuget.yml` dispatch 只生成绑定两个 registry 的精确授权 token；正式上传必须回填该 token 并通过 `nuget-production` Environment。常规路径要求不同的 publisher/approver；本次明确授权且仅限稳定版 `5.0.0` 的例外必须使用 `single_maintainer_exception=true`、publisher `Guojin Yan`、approver `not-available`，由 `guojin-yan` 发起并使用精确 candidate token，在决策记录中明确接受没有独立审核的风险；该例外不能授权其他版本。随后将全部 GitHub package 页面逐项设为 Public，再单独运行双源验证与 Release。NuGet.org 包要求 Repository signature、owner `GuojinYan`、可信 timestamp 以及除 `.signature.p7s` 外的 payload entry 全部逐字节一致；GitHub Packages 要求精确 unsigned SHA256、Public 可见性和正式仓库关联。

## Blocker Ledger

The local candidate remains `locally-validated`, unsigned, and unpublished, with signing state `repository-signing-pending`. The protected `nuget-production` Environment and secret custody are already configured. The stable `5.0.0` owner risk exception is approved in principle, but the refreshed final package/SBOM bundle and its exact dry-run authorization token still require separate candidate-specific authorization. GitHub Packages public visibility and both post-upload verification proofs also remain required. Long-term gaps remain Android ARM/ARM64 physical-device evidence, macOS matrix decisions, repository-wide API implementation, and unmeasured upstream surfaces; none is silently promoted into support.
