# Release Candidate Closeout

The current candidate is a local, deterministic preflight only. The generated record is `packaging/release/local-release-candidate-closeout.json`; `scripts/Test-ReleaseCandidateFinalCloseout.ps1` verifies its source-set digest, evidence hashes, API/ABI baselines, support contract, and fail-closed release states. Candidate identity is derived only from the normalized source-set SHA-256, so committing an already verified source set does not invalidate the record through a changing Git HEAD.

## Local Checklist

- [x] Managed package and runtime package identities remain version-neutral.
- [x] Native full/mini ABI manifests and managed API baseline are generated and checked for deterministic drift.
- [x] The bounded OpenCV 5.0.0 ImgProc map classifies all 203 parsed declarations, with 161 implemented callables, zero measured gaps, 6 intentional omissions, and no repository-wide parity claim.
- [x] Runtime support is classified as 24 real, 1 pending, 9 excluded, with macOS outside the matrix.
- [x] Deterministic package normalization runs before any future signing input.
- [x] Provenance, license, runtime payload, change-control, rollback, and read-only public-feed evidence are locally validated.
- [x] Exact SDK `10.0.302`, DocFX `2.78.5`, workflow syntax, actionlint, and aggregate invariant checks are local gates.
- [x] Main Video ECC and TrackerMIL callables are closed, with 138 implemented callables, zero measured gaps, and 7 intentional omissions in the bounded Video partition.
- [ ] Hosted `win-x86/full` producer-to-X86-consumer evidence after quota restoration.
- [ ] External signing and SPDX-2.3 SBOM inputs with immutable public references.
- [ ] Independent reviewer/approver acceptance of exact normalized/post-signing bytes.
- [ ] Publication authorization, preview release, and public installation verification.

## Stop Conditions

Stop immediately on source or artifact hash drift, synthetic runtime inputs in a publish-capable path, incomplete PE/ELF closure, producer/search-path overrides, missing license or provenance evidence, mutable certificate/feed references, unapproved signing/SBOM state, remote mutation, or any attempt to publish before the complete hosted and approval chain succeeds.

## First Preview Package Contract

The first release channel uses strict pack input `5.0.0.0-preview.1` and normalized NuGet identity `5.0.0-preview.1`. The pack scripts accept exactly four numeric source components plus an optional lowercase SemVer prerelease suffix. Missing revision components, uppercase or malformed labels, numeric prerelease identifiers with leading zeroes, build metadata, OpenCV version drift, and package revision drift fail before `dotnet pack` runs.

Local real-input win-x64 full and mini preview packages have passed release preflight, exact artifact inspection, isolated restore/build, and package-owned native smoke. Full proved `core,imgproc,imgcodecs,videoio,dnn`; mini proved `core,imgproc,imgcodecs,videoio,not_linked`. These are local preview-path results, not publication approval. Final package bytes and hashes must be regenerated from the final release commit before signing, approval, or upload.

首个发布渠道使用严格打包输入 `5.0.0.0-preview.1`，对应 NuGet 规范身份 `5.0.0-preview.1`。使用真实 win-x64 输入生成的 full/mini preview 包已通过 release preflight、产物检查、隔离还原/构建和包内 native smoke；这些结果只证明本地 preview 路径，不代表已批准或已发布。签名、审批或上传前，必须基于最终 release commit 重新生成并核对最终包字节与哈希。

## Blocker Ledger

The local candidate remains `locally-validated`, `not-approved`, unsigned, and unpublished. Remaining blockers are hosted CI compatibility and `win-x86/full`, signing/SBOM/approval/publication inputs, Android and macOS support decisions, repository-wide API gap implementation, and still-unmeasured upstream surfaces outside the bounded module partitions. These blockers are external or future implementation work; they are not silently converted into support claims.
