# Package Publication And Repository Signing Guide

OpenCV CSharp API `5.0.0-preview.1` publishes the same reviewed, support-contract-derived candidate to NuGet.org and GitHub Packages. The intended final set is 29 packages after Android single-loader revalidation. The signing model matches TensorRtSharp: the local package is deterministically normalized and intentionally unsigned, NuGet.org adds a Repository primary signature after upload, and both public registries are downloaded and verified before the release is accepted.

OpenCV CSharp API `5.0.0-preview.1` 将同一份审核通过的 29 包 candidate 发布到 NuGet.org 和 GitHub Packages。签名模型与 TensorRtSharp 相同：本地包经过确定性规范化并有意保持未签名，上传后由 NuGet.org 添加 Repository primary signature；只有两个公开 registry 均重新下载并验证通过后，发布才可被接受。

## Security Model / 安全模型

The model separates three identities:

1. The Git source commit and normalized unsigned package hashes identify what the project approved for upload.
2. The NuGet.org Repository signature proves that the downloaded bytes came through the official NuGet.org repository and package-owner route.
3. The GitHub Packages SHA256, public visibility, and repository link prove that the second registry exposes the exact reviewed bytes from `guojin-yan/OpenCV-CSharp-API`.
4. The publication authorization and protected GitHub Environment prove who approved and executed the upload.

该模型分离三种身份：

1. Git source commit 与规范化未签名包 hash 标识项目批准上传的内容。
2. NuGet.org Repository signature 证明下载字节经过官方 NuGet.org repository 与 package-owner 路径。
3. GitHub Packages SHA256、公开可见性和 repository link 证明第二个 registry 从 `guojin-yan/OpenCV-CSharp-API` 提供精确审核字节。
4. publication authorization 与受保护 GitHub Environment 证明谁批准并执行了上传。

Repository signing is not an author certificate. The project does not claim that a local self-signed certificate, PFX, hardware token, or private key signed the package. No project private key is required or permitted in the repository, candidate directory, logs, or Actions artifacts.

Repository signing 不是 author certificate。本项目不会声称本地自签名证书、PFX、硬件令牌或 private key 签署了 package；仓库、candidate、日志和 Actions artifact 均不需要也不允许出现项目 private key。

## Prepublication State / 发布前状态

The first-preview set remains `repository-signing-pending`: one managed package plus every runtime package in `runtime-support-contract.json` `realSupport`. While Android single-loader evidence is regenerated, that fail-closed set contains 25 packages; after all four emulator profiles are promoted it returns to the intended 29 packages. The normalized publication manifest binds every authoritative pack run ID and package SHA256 to the package-bound SPDX documents and durable change-control record. Generate the final publication bundle only from the final source commit:

29 个 package 在发布前保持 `repository-signing-pending`：一个 managed 包，加上 `runtime-support-contract.json` 中 28 个 `realSupport` runtime 包。规范化 publication manifest 将每个正式 pack run ID 和 SHA256 绑定到 package-bound SPDX 与 durable change-control。只能从最终 source commit 生成 publication bundle：

```powershell
pwsh -NoProfile -File ./scripts/Test-NuGetPublicationManifest.ps1 `
  -ManifestPath <input-manifest.json> `
  -SourceCommit <40-hex-commit> `
  -PackageVersion 5.0.0-preview.1 `
  -OutputPath <publication-manifest.json>
pwsh -NoProfile -File ./scripts/New-NuGetPublicationBundle.ps1 `
  -PackageRoot <package-root> `
  -SbomRoot <sbom-root> `
  -ChangeControlPath <release-change-control.json> `
  -SourceCommit <40-hex-commit> `
  -PackageVersion 5.0.0-preview.1 `
  -Created <factual-UTC> `
  -PublicationManifestPath <publication-manifest.json> `
  -OutputPath <nuget-publication-bundle.json>
```

The output contains `publish-nuget:sha256:<candidate-hash>`. This is a public authorization identifier, not a credential. It binds the exact source, packages, SPDX documents, change-control record, NuGet.org target, GitHub Packages target, required public visibility, and authoritative repository.

输出包含 `publish-nuget:sha256:<candidate-hash>`。它是公开 authorization identifier，不是 credential；它绑定精确 source、packages、SPDX、change-control、NuGet.org target、GitHub Packages target、公开可见性和正式 repository。

## Three-Phase Workflow / 三阶段工作流

Run `.github/workflows/publish-nuget.yml` only in `guojin-yan/OpenCV-CSharp-API`.

1. Dry run: set `publish=false`, provide the exact source run IDs and package hashes, and leave `publish_authorization` empty. Review the uploaded `nuget-publication-candidate` artifact and emitted token.
2. Upload run: use the same source, run IDs, hashes, version, and UTC creation time; set `publish=true`, `verify_publication=false`, and `create_github_release=false`; provide the exact token; name a designated publisher and a different independent approver.
3. GitHub pauses both upload jobs at the protected `nuget-production` Environment. That Environment must hold `NUGET_API_KEY` and require the configured reviewer. The jobs recheck the bundle byte-for-byte, then upload the exact support-contract-derived package set to NuGet.org and `https://nuget.pkg.github.com/guojin-yan/index.json`. Duplicate identity is an error; `--skip-duplicate` is forbidden.
4. GitHub Packages initially creates user-scoped packages with private visibility. Set every candidate package page to Public and confirm that each is linked to `guojin-yan/OpenCV-CSharp-API` before verification. Public visibility is irreversible on GitHub.
5. Verification/release run: keep `publish=false`, set `verify_publication=true`, and optionally set `create_github_release=true`. Reuse the exact token and identities. The workflow verifies every candidate package on both registries before it may create the prerelease.

`.github/workflows/publish-nuget.yml` 只能在 `guojin-yan/OpenCV-CSharp-API` 执行。

1. Dry run：设置 `publish=false`，提供精确 source run IDs 与 package hashes，保持 `publish_authorization` 为空；审核 `nuget-publication-candidate` artifact 和输出 token。
2. 上传 run：保持 source、run IDs、hashes、version、UTC creation time 完全相同，设置 `publish=true`、`verify_publication=false`、`create_github_release=false`，回填精确 token，并指定不同的 publisher 与 independent approver。
3. 两个上传 job 均受 `nuget-production` Environment 保护。该 Environment 保存 `NUGET_API_KEY` 并要求配置的 reviewer。job 逐字节复核 bundle 后，分别向 NuGet.org 与 `https://nuget.pkg.github.com/guojin-yan/index.json` 上传 support contract 精确确定的全部候选包。重复身份必须失败，禁止 `--skip-duplicate`。
4. GitHub Packages 初次创建 user-scoped package 时默认为 private。验证前必须在 29 个 package 页面中逐项设为 Public，并确认关联 `guojin-yan/OpenCV-CSharp-API`。GitHub 的 Public 可见性不可逆。
5. 验证/Release run：保持 `publish=false`，设置 `verify_publication=true`，按需设置 `create_github_release=true`，复用精确 token 与身份。只有两个 registry 均达到 29/29 验证通过后才能创建 prerelease。

The compilation mirror `grape-yan/OpenCV-CSharp-API` cannot enter this workflow. A green mirror build is never publication authorization.

编译镜像 `grape-yan/OpenCV-CSharp-API` 无法进入该工作流；mirror build 全绿永远不能替代 publication authorization。

## Postpublication Verification / 发布后验证

NuGet.org changes the archive-level SHA256 when it adds `.signature.p7s`. That difference is expected and does not replace the unsigned candidate hash. Both hashes are retained.

NuGet.org 添加 `.signature.p7s` 后，archive-level SHA256 必然改变；这是预期行为，不能覆盖未签名候选 hash，两个 hash 必须同时保留。

For each package, `Test-NuGetRepositorySignedPackage.ps1` performs both checks:

- `dotnet nuget verify --all --verbosity detailed` validates the cryptographic signature and trust chain.
- `NuGet.Packaging` requires `RepositoryPrimarySignature`, service index `https://api.nuget.org/v3/index.json`, owner `GuojinYan`, SHA256 content hashing, the NuGet.org repository certificate, and at least one timestamp.
- Every non-signature ZIP entry path, length, and SHA256 must equal the frozen unsigned package. The only allowed additional entry is non-empty `.signature.p7s`.

每个 package 都会执行两类检查：

- `dotnet nuget verify --all --verbosity detailed` 验证密码学签名与信任链。
- `NuGet.Packaging` 要求 `RepositoryPrimarySignature`、精确 service index、owner `GuojinYan`、SHA256 content hash、NuGet.org repository certificate 和至少一个 timestamp。
- 除 `.signature.p7s` 外，每个 ZIP entry 的 path、length、SHA256 必须与冻结未签名包一致；唯一允许的新增 entry 是非空 `.signature.p7s`。

```powershell
pwsh -NoProfile -File ./scripts/Test-NuGetRepositorySignedPackage.ps1 `
  -UnsignedPackagePath <reviewed-unsigned.nupkg> `
  -RepositorySignedPackagePath <downloaded-from-nuget-org.nupkg> `
  -PackageId JYPPX.OpenCV.CSharp.API `
  -PackageVersion 5.0.0-preview.1 `
  -ExpectedOwner GuojinYan `
  -VerifiedAt <factual-UTC> `
  -OutputPath <repository-signature-report.json>
```

GitHub Packages must also expose every package as Public, link it to `guojin-yan/OpenCV-CSharp-API`, contain the exact requested version once, and return bytes whose SHA256 equals the reviewed unsigned candidate. The GitHub prerelease may be created only after both registries pass 29/29 verification. Its `.nupkg` assets are the NuGet.org repository-signed public bytes; SPDX documents remain bound to the reviewed unsigned payload, while both verification summaries are attached to the Release.

GitHub Packages 还必须将每个包设为 Public、关联 `guojin-yan/OpenCV-CSharp-API`、只包含一个精确目标版本，并返回 SHA256 与审核 unsigned candidate 相同的字节。两个 registry 均达到 29/29 后才允许创建 GitHub prerelease。Release 中的 `.nupkg` 是 NuGet.org repository-signed 公开字节；SPDX 继续绑定已审核的 unsigned payload，两份 verification summary 同时附加到 Release。

## Consumer Check / 使用者检查

Consumers can verify a downloaded package directly:

使用者可以直接验证下载包：

```powershell
dotnet nuget verify --all JYPPX.OpenCV.CSharp.API.5.0.0-preview.1.nupkg
```

The expected signature type is `Repository`, and the signer subject contains `NuGet.org Repository by Microsoft`. A missing signature, Author-only signature, wrong repository, wrong owner, failed timestamp, or trust-chain error is a stop condition.

预期 signature type 为 `Repository`，signer subject 包含 `NuGet.org Repository by Microsoft`。缺少签名、只有 Author signature、repository/owner 错误、timestamp 失败或信任链错误都必须停止使用和发布。
