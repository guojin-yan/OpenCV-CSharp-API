# 5.0.1 Modern API And Platform Preview Guide / 5.0.1 现代 API 与平台预览指南

This guide is the entry point for the 5.0.1 capability, typed-pixel, bounded-codec, and platform-boundary work. It groups the current contracts without turning preview evidence into a stable support claim.

本文是 5.0.1 capability、typed pixel、bounded codec 和平台边界工作的入口。它汇总当前契约，但不会把 preview 证据升级为稳定支持声明。

## Capability snapshot / 能力快照

Call `OpenCvCapabilities.GetCurrent()` before choosing a pipeline. The snapshot is side-effect-free: it does not open a camera, load a model, or execute an image algorithm. `ToJson()` is deterministic and follows [`runtime-capabilities-json.schema.json`](../../packaging/runtime/runtime-capabilities-json.schema.json). The `capabilities-json` ConsoleSamples command produces an evidence-friendly record:

```powershell
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release -- capabilities-json
```

`Verified` means the specific probe completed. It does not infer optional contrib modules, GPU execution, camera usability, or a published platform claim. Read the detailed field and state rules in [Runtime Capability And Platform Guide](runtime-capability-and-platform-guide.md).

在选择 pipeline 前调用 `OpenCvCapabilities.GetCurrent()`。快照无副作用，不会打开摄像头、加载模型或执行图像算法。`ToJson()` 输出确定性 JSON，并遵循 [`runtime-capabilities-json.schema.json`](../../packaging/runtime/runtime-capabilities-json.schema.json)。`Verified` 只表示对应探针完成，不推断可选 contrib、GPU 执行、摄像头可用性或正式平台支持。

## Typed pixels / Typed Mat

`PixelTypeDescriptor` and `PixelTypeTraits` are the stable managed registry boundary. `MatView<TPixel>` and `ReadOnlyMatView<TPixel>` remain conditional Span-capable previews. A vector storage type does not prove BGR, RGB, or alpha semantics; adapters must state those conventions explicitly.

Use row access for a non-contiguous ROI and keep borrowed spans inside the owner/view lifetime:

```csharp
using ReadOnlyMatView<Vec3b> view = mat.AsReadOnlyView<Vec3b>();
ReadOnlySpan<Vec3b> row = view.AsReadOnlyRowSpan(0);
```

Do not retain a span across `Dispose`, `Mat.Create`, or another native header change. Use `Clone`, `CopyTo`, or `ToArray` when an independent lifetime is required. See [Typed Mat Views And Pixel Traits](typed-mat-view-and-pixel-traits.md) and [Typed Mat View ADR](typed-mat-view-adr.md).

## Bounded codec paths / 有界编解码

Use `ImageIdentifyResult` and `ImageDecodeOptions` before decoding untrusted bytes. Limits cover input bytes, dimensions, pixels, cumulative pixels, frames, metadata, ICC, encoded pixel storage, depth, and channels. Unknown facts remain unknown unless a strict option rejects them. Seekable stream positions are restored by preflight; non-seekable streams are consumed.

The existing `byte[]` APIs remain the compatibility baseline. On modern target frameworks, preview `ImEncodeTo(..., IBufferWriter<byte>)` performs one final `GetSpan`/copy/`Advance`; writer faults are terminal and no native result is retried. The internal `CodecBufferLease` prototype validates pinned owner and stride boundaries but does not create a Mat header or ABI entry.

Untrusted decode policy is documented in [Codec Preflight And Limits](codec-preflight-and-limits.md). Buffer ownership and rollback are in [Codec Buffer And Owner ADR](codec-buffer-and-owner-adr.md) and [Codec Buffer Lease ADR](codec-buffer-lease-adr.md).

对于不可信输入，先使用 `ImageIdentifyResult` 与 `ImageDecodeOptions` 做预检。限制覆盖输入字节、尺寸、像素、累计像素、帧数、metadata、ICC、编码像素存储、深度和通道。现有 `byte[]` API 仍是兼容基线；`IBufferWriter<byte>` 与 pinned lease 仍是 preview/internal，未进入稳定 ABI。

## Platform and headless boundaries / 平台与 headless 边界

`OperatingSystemDescription`, `ProcessArchitecture`, `RuntimeIdentifier`, and `ProcessBitness` are diagnostic fields. They are not support claims. Use the exact runtime package RID and require producer, package, independent consumer, loader, dependency, and native smoke evidence before promoting a target.

The headless profile is candidate-only. It keeps the managed API shape, omits the native HighGUI module, and requires deterministic `NOT_LINKED`/unsupported behavior for GUI calls, codec smoke, VideoIO results, and no-display execution. It has no active NuGet identity until the profile-specific ABI and dependency evidence is complete. The same rule applies to generic Linux ARM64 and domestic Linux candidates: architecture or cross-compilation alone is insufficient.

平台字段是诊断信息，不是支持声明。headless profile 仍是 candidate-only；在 profile-specific ABI、依赖、独立 consumer 和无显示环境 smoke 闭合前，不创建 active NuGet identity。generic Linux ARM64 与国产平台也必须完成真实 producer/package/consumer 证据，不能仅凭架构或交叉编译晋升。

## Evidence commands / 证据命令

```powershell
pwsh -NoProfile -File .\scripts\Test-CapabilitiesJsonContract.ps1
pwsh -NoProfile -File .\scripts\Test-GenericLinuxArm64PreviewMatrix.ps1
pwsh -NoProfile -File .\scripts\Test-HeadlessRuntimeProfileContract.ps1
dotnet test .\tests\OpenCvSharp.Tests\OpenCvSharp.Tests.csproj -c Release -f net8.0 --filter FullyQualifiedName~ImagePreflightTests
dotnet test .\tests\OpenCvSharp.Tests\OpenCvSharp.Tests.csproj -c Release -f net8.0 --filter FullyQualifiedName~CodecBufferLeaseTests
```

These checks prove the local contract and focused behavior. They do not replace a factual native runtime, hosted ARM64 consumer, or vendor certification evidence.
