# NuGet.org Repository Signing Guide

OpenCV CSharp API `5.0.0-preview.1` uses the same public-package signing model as TensorRtSharp: the reviewed local package is deterministically normalized and intentionally unsigned, NuGet.org adds a Repository primary signature after upload, and the public bytes are downloaded and verified before the release is accepted.

OpenCV CSharp API `5.0.0-preview.1` 与 TensorRtSharp 使用同一种公开包签名模型：本地审核包经过确定性规范化并有意保持未签名；上传后由 NuGet.org 添加 Repository primary signature；只有重新下载并验证公开包后，发布才可被接受。

## Security Model / 安全模型

The model separates three identities:

1. The Git source commit and normalized unsigned package hashes identify what the project approved for upload.
2. The NuGet.org Repository signature proves that the downloaded bytes came through the official NuGet.org repository and package-owner route.
3. The publication authorization and protected GitHub Environment prove who approved and executed the upload.

该模型分离三种身份：

1. Git source commit 与规范化未签名包 hash 标识项目批准上传的内容。
2. NuGet.org Repository signature 证明下载字节经过官方 NuGet.org repository 与 package-owner 路径。
3. publication authorization 与受保护 GitHub Environment 证明谁批准并执行了上传。

Repository signing is not an author certificate. The project does not claim that a local self-signed certificate, PFX, hardware token, or private key signed the package. No project private key is required or permitted in the repository, candidate directory, logs, or Actions artifacts.

Repository signing 不是 author certificate。本项目不会声称本地自签名证书、PFX、硬件令牌或 private key 签署了 package；仓库、candidate、日志和 Actions artifact 均不需要也不允许出现项目 private key。

## Prepublication State / 发布前状态

The three package files remain `repository-signing-pending`. Their SHA256 values bind the package-bound SPDX documents and durable change-control record. Generate the final publication bundle only from the final source commit:

三个 package 在发布前保持 `repository-signing-pending`，SHA256 同时绑定 package-bound SPDX 与 durable change-control。只能从最终 source commit 生成 publication bundle：

```powershell
pwsh -NoProfile -File ./scripts/New-NuGetPublicationBundle.ps1 `
  -PackageRoot <package-root> `
  -SbomRoot <sbom-root> `
  -ChangeControlPath <release-change-control.json> `
  -SourceCommit <40-hex-commit> `
  -PackageVersion 5.0.0-preview.1 `
  -Created <factual-UTC> `
  -ExpectedManagedSha256 <managed-sha256> `
  -ExpectedFullSha256 <full-sha256> `
  -ExpectedMiniSha256 <mini-sha256> `
  -OutputPath <nuget-publication-bundle.json>
```

The output contains `publish-nuget:sha256:<candidate-hash>`. This is a public authorization identifier, not a credential. It binds the exact source, packages, SPDX documents, change-control record, NuGet owner, and service index.

输出包含 `publish-nuget:sha256:<candidate-hash>`。它是公开 authorization identifier，不是 credential；它绑定精确 source、packages、SPDX、change-control、NuGet owner 与 service index。

## Two-Stage Workflow / 两阶段工作流

Run `.github/workflows/publish-nuget.yml` only in `guojin-yan/OpenCV-CSharp-API`.

1. Dry run: set `publish=false`, provide the exact source run IDs and package hashes, and leave `publish_authorization` empty. Review the uploaded `nuget-publication-candidate` artifact and emitted token.
2. Publication run: use the same source, run IDs, hashes, version, and UTC creation time; set `publish=true`; provide the exact token; name a designated publisher and a different independent approver.
3. GitHub pauses the upload job at the protected `nuget-production` Environment. That Environment must hold `NUGET_API_KEY` and require the configured reviewer.
4. The job rechecks the bundle byte-for-byte and uploads exactly three packages. Duplicate identity is an error; `--skip-duplicate` is forbidden.

`.github/workflows/publish-nuget.yml` 只能在 `guojin-yan/OpenCV-CSharp-API` 执行。

1. Dry run：设置 `publish=false`，提供精确 source run IDs 与 package hashes，保持 `publish_authorization` 为空；审核 `nuget-publication-candidate` artifact 和输出 token。
2. 正式 run：保持 source、run IDs、hashes、version、UTC creation time 完全相同，设置 `publish=true`，回填精确 token，并指定不同的 publisher 与 independent approver。
3. GitHub 会在受保护的 `nuget-production` Environment 暂停上传 job；该 Environment 保存 `NUGET_API_KEY` 并要求配置的 reviewer。
4. job 会逐字节复核 bundle，只上传三个精确包。重复身份必须失败，禁止 `--skip-duplicate`。

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

The GitHub prerelease may be created only after all three public packages pass. Its `.nupkg` assets are the repository-signed public bytes; SPDX documents remain bound to the reviewed unsigned payload, while the verification reports prove payload equivalence and record the public signed hashes.

只有三个公开包全部通过后，才允许创建 GitHub prerelease。Release 中的 `.nupkg` 是 repository-signed 公开字节；SPDX 继续绑定已审核的 unsigned payload，verification reports 证明 payload 等价并记录公开签后 hash。

## Consumer Check / 使用者检查

Consumers can verify a downloaded package directly:

使用者可以直接验证下载包：

```powershell
dotnet nuget verify --all JYPPX.OpenCV.CSharp.API.5.0.0-preview.1.nupkg
```

The expected signature type is `Repository`, and the signer subject contains `NuGet.org Repository by Microsoft`. A missing signature, Author-only signature, wrong repository, wrong owner, failed timestamp, or trust-chain error is a stop condition.

预期 signature type 为 `Repository`，signer subject 包含 `NuGet.org Repository by Microsoft`。缺少签名、只有 Author signature、repository/owner 错误、timestamp 失败或信任链错误都必须停止使用和发布。
