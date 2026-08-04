# Release Candidate Closeout

The current candidate is a local, deterministic preflight only. The generated record is `packaging/release/local-release-candidate-closeout.json`; `scripts/Test-ReleaseCandidateFinalCloseout.ps1` verifies its source-set digest, evidence hashes, API/ABI baselines, support contract, and fail-closed release states. Candidate identity is derived only from the normalized source-set SHA-256, so committing an already verified source set does not invalidate the record through a changing Git HEAD.

## Local Checklist

- [x] Managed package and runtime package identities remain version-neutral.
- [x] Native full/mini ABI manifests and managed API baseline are generated and checked for deterministic drift.
- [x] The bounded OpenCV 5.0.0 ImgProc map classifies all 203 parsed declarations, with 161 implemented callables, zero measured gaps, 6 intentional omissions, and no repository-wide parity claim.
- [x] Runtime support is classified as 28 real, 5 pending, 1 excluded, with macOS outside the matrix and Android ARM/ARM64 awaiting physical-device evidence.
- [x] Deterministic package normalization runs before the NuGet.org repository-signing boundary.
- [x] The first-preview signing strategy is `nuget.org-repository-signing`; no author certificate or project private key is required.
- [x] Structured and cryptographic verification bind the public Repository signature to owner `GuojinYan` and compare every non-signature payload entry with the frozen unsigned package.
- [x] Formal publication binds the same support-contract-derived package candidate to NuGet.org and GitHub Packages, with exact-byte, public-visibility, and authoritative-repository verification before Release.
- [x] Android x64/x86 Full and Mini single-loader payloads execute package-owned native calls in ABI-matched hosted emulators; retired dual-loader runs are retained only as superseded history.
- [x] Deterministic SPDX-2.3 generation and byte-for-byte check mode are guarded with 17 negative fixtures.
- [x] Provenance, license, runtime payload, change-control, rollback, and read-only public-feed evidence are locally validated.
- [x] Exact SDK `10.0.302`, DocFX `2.78.5`, workflow syntax, actionlint, and aggregate invariant checks are local gates.
- [x] Main Video ECC and TrackerMIL callables are closed, with 138 implemented callables, zero measured gaps, and 7 intentional omissions in the bounded Video partition.
- [ ] Hosted `win-x86/full` producer-to-X86-consumer evidence after quota restoration.
- [ ] Final-commit package-bound SPDX-2.3 documents and dry-run publication bundle.
- [ ] Protected `nuget-production` Environment, API-key custody owner, designated publisher, and distinct independent approver.
- [ ] Public visibility and authoritative repository link for every package in the final support-contract-derived candidate.
- [ ] Independent reviewer/approver acceptance of exact normalized/post-signing bytes.
- [ ] Publication authorization, preview release, and public installation verification.

## Stop Conditions

Stop immediately on source or artifact hash drift, synthetic runtime inputs in a publish-capable path, incomplete PE/ELF closure, producer/search-path overrides, missing license or provenance evidence, a non-Repository or wrong-owner NuGet signature, payload drift outside `.signature.p7s`, private or incorrectly linked GitHub Packages, GitHub package byte drift, unapproved SBOM/publication state, missing Environment approval, or any attempt to publish from the compilation mirror.

## First Preview Package Contract

The first release channel uses strict pack input `5.0.0.0-preview.1` and normalized NuGet identity `5.0.0-preview.1`. The pack scripts accept exactly four numeric source components plus an optional lowercase SemVer prerelease suffix. Missing revision components, uppercase or malformed labels, numeric prerelease identifiers with leading zeroes, build metadata, OpenCV version drift, and package revision drift fail before `dotnet pack` runs.

Local real-input win-x64 full and mini preview packages have passed release preflight, exact artifact inspection, isolated restore/build, and package-owned native smoke. Full proved `core,imgproc,imgcodecs,videoio,dnn`; mini proved `core,imgproc,imgcodecs,videoio,not_linked`. These are local preview-path results, not publication approval. Final normalized unsigned package bytes and hashes must be regenerated from the final release commit before dry-run review or upload.

After rebuilding the normalized unsigned packages from the final release commit, generate one SPDX document per exact package and immediately verify it with `-Check`:

```powershell
pwsh -NoProfile -File ./scripts/New-ReleasePackageSbom.ps1 -PackagePath <normalized-unsigned.nupkg> -SourceCommit <40-hex-final-commit> -Created <factual-UTC-timestamp> -OpenCvVersion 5.0.0 -OutputPath <package.spdx.json>
pwsh -NoProfile -File ./scripts/New-ReleasePackageSbom.ps1 -PackagePath <normalized-unsigned.nupkg> -SourceCommit <40-hex-final-commit> -Created <same-factual-UTC-timestamp> -OpenCvVersion 5.0.0 -OutputPath <package.spdx.json> -Check
pwsh -NoProfile -File ./scripts/Test-ReleasePackageSbom.ps1
pwsh -NoProfile -File ./scripts/Test-NuGetPublicationManifest.ps1 -ManifestPath <input-manifest.json> -SourceCommit <40-hex-final-commit> -PackageVersion 5.0.0-preview.1 -OutputPath <publication-manifest.json>
pwsh -NoProfile -File ./scripts/Test-ReleaseChangeControlRecord.ps1 -PackageRoot <package-root> -SbomRoot <sbom-root> -OutputPath <release-change-control.json> -Created <same-factual-UTC-timestamp> -ExpectedPackageCount 29
pwsh -NoProfile -File ./scripts/Test-ReleaseChangeControlRecord.ps1 -PackageRoot <package-root> -SbomRoot <sbom-root> -OutputPath <release-change-control.json> -Created <same-factual-UTC-timestamp> -ExpectedPackageCount 29 -Check
pwsh -NoProfile -File ./scripts/New-NuGetPublicationBundle.ps1 -PackageRoot <package-root> -SbomRoot <sbom-root> -ChangeControlPath <release-change-control.json> -SourceCommit <40-hex-final-commit> -PackageVersion 5.0.0-preview.1 -Created <same-factual-UTC-timestamp> -PublicationManifestPath <publication-manifest.json> -OutputPath <nuget-publication-bundle.json>
```

The generators reject signed or nondeterministically normalized inputs, repository/source drift, unsafe archive entries, synthetic runtime provenance, version/OpenCV/license drift, and stale SBOM bytes. The change-control record classifies exact packages as `current-unsigned-candidate`, SBOMs as `generated-unapproved`, and signing as `repository-signing-pending`. `New-NuGetPublicationBundle.ps1` emits a candidate-specific `publish-nuget:sha256:<hash>` token that binds both public registries. A dry run uses all action inputs false. The upload run uses `publish=true` only with that exact token, named distinct publisher/approver identities, and approval through the protected `nuget-production` Environment; it publishes the exact support-contract-derived set to NuGet.org and GitHub Packages. After all GitHub package pages are manually made Public, a separate run uses `verify_publication=true` and may use `create_github_release=true`.

NuGet.org adds the primary Repository signature only after upload. `Test-NuGetRepositorySignedPackage.ps1` runs `dotnet nuget verify --all`, uses `NuGet.Packaging` to require a Repository signature from `https://api.nuget.org/v3/index.json` owned by `GuojinYan`, requires a trusted timestamp, and compares every unsigned payload entry byte-for-byte. The public package SHA256 is expected to differ because `.signature.p7s` is added; both prepublication and repository-signed hashes remain evidence. GitHub Packages must return the exact reviewed unsigned archive SHA256, be Public, and link to `guojin-yan/OpenCV-CSharp-API`. GitHub Release creation is downstream of both 29/29 verification jobs and attaches the repository-signed packages, unsigned-payload SPDX documents, normalized publication manifest, and both registry verification reports.

首个发布渠道使用严格打包输入 `5.0.0.0-preview.1`，对应 NuGet 规范身份 `5.0.0-preview.1`。使用真实 win-x64 输入生成的 full/mini preview 包已通过 release preflight、产物检查、隔离还原/构建和包内 native smoke；这些结果只证明本地 preview 路径，不代表已批准或已发布。dry-run 审核或上传前，必须基于最终 release commit 重新生成并核对最终规范化未签名包字节与哈希。

最终提交的规范化未签名包生成后，必须为全部 29 个候选包生成并 `-Check` SPDX-2.3、完整 publication manifest、durable change-control 和 NuGet publication bundle。集合只能包含一个 managed 包和 `realSupport` 中的全部 28 个 runtime 包；pending、excluded 与 synthetic 目标必须失败。状态分别保持 `current-unsigned-candidate`、`generated-unapproved` 与 `repository-signing-pending`。首次 `publish-nuget.yml` dispatch 只生成绑定两个 registry 的精确授权 token；正式上传必须回填该 token、不同的 publisher/approver，并通过 `nuget-production` Environment，将同一候选物发布到 NuGet.org 和 GitHub Packages。随后将全部 GitHub package 页面逐项设为 Public，再单独运行双源验证与 Release。NuGet.org 包要求 Repository signature、owner `GuojinYan`、可信 timestamp 以及除 `.signature.p7s` 外的 payload entry 全部逐字节一致；GitHub Packages 要求精确 unsigned SHA256、Public 可见性和正式仓库关联。

## Blocker Ledger

The local candidate remains `locally-validated`, `not-approved`, unsigned, and unpublished, with signing state `repository-signing-pending`. Remaining first-publication inputs are the refreshed final package/SBOM bundle, protected `nuget-production` Environment and API-key custody, named distinct publisher/approver, exact authorization token, GitHub Packages public-visibility confirmation, and both post-upload verification proofs. Long-term gaps remain `win-x86/full`, Android ARM/ARM64 physical-device evidence, macOS matrix decisions, repository-wide API implementation, and unmeasured upstream surfaces; none is silently promoted into support.
