# 5.1.0 Modern API And Platform Preview Guide / 5.1.0 现代 API 与平台预览指南

This guide is the entry point for the 5.1.0 capability, typed-pixel, bounded-codec, and platform-boundary work inherited from the unpublished 5.0.1 preview. It groups the current contracts without turning preview evidence into a stable support claim.

本文是 5.1.0 capability、typed pixel、bounded codec 和平台边界工作的入口，承接尚未发布的 5.0.1 preview。它汇总当前契约，但不会把 preview 证据升级为稳定支持声明。

The [OpenCV 5.1.0 local evidence](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/modern-api-5.1.0-evidence.json) records three isolated capability snapshots and 76 passing tests across net8.0/net10.0 and no-runtime/Full/Mini profiles. Full's 43 native DLLs and Mini's 7 native DLLs match the verified local package-consumer payload hashes. Writer segment, reuse and fault recovery, plus Mini HighGUI/DNN omission and missing-file VideoIO behavior, are covered. Allocation thresholds, stable writer promotion, a headless package and cross-RID support retain their separate gates.

[OpenCV 5.1.0 本地证据](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/modern-api-5.1.0-evidence.json) 包含三个隔离能力快照，以及 net8.0/net10.0 与无 runtime/Full/Mini 六组共 76 项通过的测试。Full 的 43 个及 Mini 的 7 个 native DLL 与已验证的本地包消费者负载哈希一致；覆盖 writer segment、复用、fault recovery，以及 Mini 的 HighGUI/DNN 省略和 VideoIO 缺失文件行为。分配阈值、writer 稳定晋升、headless 包和跨 RID 支持仍按各自门槛验收。

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

The ConsoleSamples preview commands provide runnable, machine-readable examples:

```powershell
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release -- typed-mat
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release -- buffered-codec
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release -- platform-probe
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release -- headless-smoke
```

With no native runtime they return `status=skipped` and `reason=native-runtime-required`. With a factual runtime directory, `typed-mat` reports a non-contiguous ROI and an owned clone, while `buffered-codec` reports the encoded byte count and the single-`GetSpan`/single-`Advance` writer boundary.

`platform-probe` emits only OS/process/RID/framework/native-state fields and warnings. It intentionally excludes absolute paths and runtime environment variables so the JSON can be attached to platform certification evidence.

`headless-smoke` is the `HeadlessServer` sample boundary. It performs a file-based Mat encode/decode pipeline without calling HighGUI and reports whether `DISPLAY`/`WAYLAND_DISPLAY` are unset. It is an application smoke example; it does not promote the candidate-only headless runtime profile or create a `.headless` package identity.

The `platform-probe` command is the `PlatformProbe` sample boundary for 5.1.0 certification evidence.

## Bounded codec paths / 有界编解码

Use `ImageIdentifyResult` and `ImageDecodeOptions` before decoding untrusted bytes. Limits cover input bytes, dimensions, pixels, cumulative pixels, frames, metadata, ICC, encoded pixel storage, depth, and channels. Unknown facts remain unknown unless a strict option rejects them. Seekable stream positions are restored by preflight; non-seekable streams are consumed.

Complete GIF containers now expose stored extension and ICC payload lengths. Exact budgets can admit those inputs; truncated or trailing containers keep both facts unknown and are rejected when the corresponding strict-known options are enabled. These facts do not validate ICC content, expanded allocation, or decoded color/precision.

AVIF dimensions and encoded pixel facts require `pitm`/`ipma` associations to the primary item. Thumbnail and unassociated `ispe/pixi` properties cannot supply those facts. Missing or malformed associations remain unknown and fail the relevant strict-known options.

JPEG checks any present SOF/SOS component tables and requires a marker walk ending exactly at EOI for frame/metadata completion. A zero-height non-hierarchical SOF may resolve its height from one length-four DNL marker inside entropy data; DNL outside a scan or repeated/ambiguous DNL keeps size and completion facts unknown. A frame fact also requires one supported SOF. APPn/COM payloads cannot supply EOI markers. ICC fragments may arrive in any order; missing, duplicate, or inconsistent fragments keep metadata/ICC lengths unknown and fail strict-known options. These checks do not validate entropy coding or decoded color/precision.

APNG frame counts require bounded frame rectangles, valid operations, and consistent default-image/IDAT/fdAT transitions. A separate default still image is excluded from the animation count. Cropped frames use the full canvas for cumulative-pixel and encoded-storage limits; malformed animation boundaries keep those facts unknown.

WebP supports simple VP8/VP8L dimensions and requires bounded image data, exact RIFF padding, and canvas/bitstream agreement before reporting frames or encoded pixel facts. Animation rectangles use doubled offsets; each cropped frame budgets the full canvas. Header-only ANMF, conflicting image chunks, or invalid nested boundaries keep those facts unknown. Reserved fields remain ignored as the format requires.

TIFF/BigTIFF reject duplicate admission tags and require consistent explicit sample layouts before reporting depth/storage. Directory and sample scans are bounded by the actual input; a sample scan budget failure keeps depth/storage unknown. Main-chain page counts and encoded header budgets do not prove strip/tile completeness or final Mat precision.

The existing `byte[]` APIs remain the compatibility baseline. On modern target frameworks, preview `ImEncodeTo(..., IBufferWriter<byte>)` performs one final `GetSpan`/copy/`Advance`; writer faults are terminal and no native result is retried. The internal `CodecBufferLease` prototype validates pinned owner and stride boundaries but does not create a Mat header or ABI entry.

Untrusted decode policy is documented in [Codec Preflight And Limits](codec-preflight-and-limits.md). Buffer ownership and rollback are in [Codec Buffer And Owner ADR](codec-buffer-and-owner-adr.md) and [Codec Buffer Lease ADR](codec-buffer-lease-adr.md).

对于不可信输入，先使用 `ImageIdentifyResult` 与 `ImageDecodeOptions` 做预检。限制覆盖输入字节、尺寸、像素、累计像素、帧数、metadata、ICC、编码像素存储、深度和通道。现有 `byte[]` API 仍是兼容基线；`IBufferWriter<byte>` 与 pinned lease 仍是 preview/internal，未进入稳定 ABI。

完整 GIF 容器现在提供扩展块及 ICC 的存储负载长度，可按精确预算准入；截断或尾部多余输入使两项事实保持未知，启用相应 strict-known 选项时会被拒绝。这些事实不验证 ICC 内容、展开后的分配量或解码后的颜色/精度。

AVIF 尺寸和编码像素事实要求通过 `pitm`/`ipma` 关联到主图，缩略图或未关联的 `ispe/pixi` 不能提供这些事实；缺失或损坏的关联保持未知，启用相应 strict-known 选项时拒绝。

JPEG 检查出现的 SOF/SOS 组件表，并要求标记遍历精确止于 EOI 才能证明帧或 metadata 闭合。非层次 SOF 高度为零时，只有熵数据中的一个长度为 4 的 DNL 标记可以补充高度；扫描外或重复/有歧义的 DNL 会使尺寸和完整性事实保持未知。帧事实还要求存在单一受支持 SOF。APPn/COM 负载中的 EOI 字节不计为结束标记。ICC 分片允许任意顺序，缺失、重复或数量不一致使 metadata/ICC 长度未知，并触发相应 strict-known 拒绝。这些检查不验证熵编码或解码颜色/精度。

APNG 帧数要求帧矩形有界、操作值有效，且默认图像与 IDAT/fdAT 转换一致；独立默认静态图不计入动画帧数。裁剪帧的累计像素与编码存储限制按完整画布计算，损坏的动画边界使这些事实保持未知。

WebP 支持普通 VP8/VP8L 尺寸；报告帧数和编码像素事实前，要求图像数据有界、RIFF 填充精确、画布/bitstream 尺寸一致。动画矩形使用乘二偏移，每个裁剪帧按完整画布预算。只有帧头的 ANMF、冲突图像块或无效嵌套边界使这些事实未知；按格式要求忽略保留字段。

TIFF/BigTIFF 拒绝重复的准入标签，并要求显式样本布局一致才能报告深度/存储。目录和样本扫描量受实际输入限制；样本扫描预算不足时深度/存储保持未知。主目录链的页数及编码头预算不证明 strip/tile 完整性或最终 Mat 精度。

JPEG 2000 checks Part 1 SIZ precision and tile-grid limits. JP2 additionally requires bounded, unique file-type/header/codestream boxes and agreement between IHDR/BPCC and SIZ dimensions, component counts, precision and sign descriptors. Conflicting headers or broken suffix boxes keep admission facts unknown. Complete codestream/frame validity, palette/color conversion, native availability and final Mat precision remain separate.

JPEG 2000 检查 Part 1 SIZ 精度和 tile 网格限制；JP2 还要求文件类型、头与码流盒有界且唯一，并交叉核对 IHDR/BPCC 与 SIZ 的尺寸、组件数、精度及符号描述。矛盾头部或损坏尾盒使准入事实未知；完整码流/帧有效性、调色板/颜色转换、native 可用性和最终 Mat 精度仍单独验证。

OpenEXR requires a complete first header, known version flags, unique typed admission attributes and unique byte-exact channel names. Channel sampling must agree with the data window; tiled/deep first parts require unit sampling. HALF stores 16 bits, UINT/FLOAT store 32; mixed widths keep depth unknown. Multipart facts describe the first part only. Chunks, frame count, aggregate allocation and final Mat precision remain unproven.

OpenEXR 要求首个头部完整、版本标志已知、准入属性类型正确且唯一、通道名逐字节唯一。通道采样必须与 dataWindow 一致，平铺/deep 首部件要求单位采样。HALF 为 16-bit 存储，UINT/FLOAT 为 32-bit；混合位宽使深度未知。多部件文件只报告首部件事实，不证明 chunk、帧数、总分配量或最终 Mat 精度。

BMP reads geometry from complete known DIB layouts, including unsigned CORE/OS2 dimensions and signed Windows height. BI_RGB frame/pixel facts additionally require complete palettes, consistent byte declarations and exact pixel/profile file ranges. V5 linked profiles are range-checked without opening names. Compressed/bitfield facts and metadata/ICC lengths remain unknown; encoded budgets do not prove palette expansion, color conversion or native allocation.

BMP 从完整已知 DIB 读取尺寸，区分 CORE/OS2 无符号宽高和 Windows 有符号高度。BI_RGB 帧/像素事实还要求色表完整、字节声明一致、像素/profile 文件范围精确。V5 linked profile 只检查范围，不打开名称。压缩/bitfield 事实及 metadata/ICC 长度仍未知；编码预算不证明调色板展开、颜色转换或 native 分配量。

PNG requires a complete supported IHDR for geometry/pixels and consistent core chunk, palette, tRNS and ICC/Exif envelopes for frame and stored-metadata completion. Truncated/contradictory containers preserve independently proven first-header facts. Fifteen real fixtures cover all color/depth pairs and three Adam7 cases; encoded channels exclude palette/tRNS expansion. CRC, compressed/metadata contents, other ancillary semantics and final/native allocation remain unproven. Existing APNG frame checks apply after the shared core checks.

PNG 仅从完整受支持的 IHDR 报告尺寸/像素，帧和 metadata 存储长度闭合还要求核心 chunk、色表、tRNS 和 ICC/Exif 外层结构一致。截断或矛盾容器可保留已独立证明的首个头部事实。15 份真实文件覆盖全部颜色/位深组合及 3 份 Adam7；编码通道不含色表/tRNS 展开。CRC、压缩/metadata 内容、其他 ancillary 语义及最终/native 分配仍未证明。既有 APNG 帧检查在共用核心检查之后执行。

PAM requires unique line-based WIDTH/HEIGHT/DEPTH/MAXVAL and complete ENDHDR. Header whitespace and arbitrary positive tuple depth are supported; optional repeated TUPLTYPE does not determine channels. Checked exact raster length gates frame/storage facts, while proven header facts survive payload failure. Thirteen real files include 5/17-channel custom tuples; OpenCV native PAM remains limited to 1..4 channels. Header facts do not prove native support, tuple/sample validity, metadata lengths or final/native allocation.

PAM 要求 WIDTH/HEIGHT/DEPTH/MAXVAL 按行且唯一，以及完整 ENDHDR。支持头部空白和任意正数元组深度，可选/重复的 TUPLTYPE 不决定通道数。checked 的精确 raster 长度决定帧/存储事实，负载失败可保留已证明的头部。13 份真实文件包含 5/17 通道自定义元组；OpenCV native PAM 仍限于 1..4 通道。头部事实不证明 native 支持、元组/样本有效性、metadata 长度或最终/native 分配量。

PNM P1 counts individual ASCII bits, P2/P3 count bounded decimal samples, and P4/P5/P6 require exact raw bytes after a single delimiter. Comments terminate at CR or LF; LF after a raw CR delimiter is a raster byte. Sixteen independently decoded fixtures cover plain/raw, 8/16-bit and packed-row boundaries. Conservative standalone completion excludes trailing junk and image sequences; raw sample/color semantics and final/native allocation remain unknown.

PNM P1 逐个计数 ASCII bit，P2/P3 计数有界十进制样本，P4/P5/P6 在单个分隔字节后核对精确 raw 长度。注释以 CR 或 LF 结束，raw CR 分隔符后的 LF 属于 raster。16 份独立解码文件覆盖 plain/raw、8/16 位及位打包行边界。保守单图完整性不涵盖尾部杂项和图像序列；raw 样本/色彩语义与最终/native 分配仍未知。

PFM scans nonzero decimal scale syntax without token allocation or native range inference. One whitespace byte ends scale; LF after CR belongs to the float raster. Checked exact float32 bytes gate frame/storage facts while a proven header survives payload failure. Eight files have independent NumPy sample/byte-order checks, with six also decoded by Netpbmfile. PNM-style comments remain a compatibility extension. Scale conversion, sample/orientation semantics, native support/allocation and final Mat precision remain separate.

PFM 扫描非零十进制 scale 语法，不分配 token，也不推断 native 表示范围。scale 仅以一个空白字节结束，CR 后的 LF 属于 float raster。checked 的精确 float32 长度决定帧/存储事实，负载失败可保留已证明的头部。8 份文件由 NumPy 独立核对样本/字节序，其中 6 份额外经 Netpbmfile 解码。PNM 风格注释保持为兼容扩展；scale 转换、样本/方向语义、native 支持/分配和最终 Mat 精度独立处理。

Radiance HDR requires one RGBE/XYZE FORMAT and supports all eight signed axis orders. The second axis determines scanline length; exact flat/old-repeat/component-RLE records gate frame/storage facts without expanding pixels. Sixteen files have independent Radiance C sample checks, with eleven also checked by imagecodecs. The four encoded 8-bit components include a shared exponent, not alpha; exposure/color/orientation transforms, metadata lengths, native support/allocation and final Mat precision remain unknown.

Radiance HDR 要求唯一 RGBE/XYZE FORMAT，支持全部八种有符号轴序。第二轴决定 scanline 长度，精确 flat/旧式重复/component-RLE record 决定帧/存储事实，不展开像素。16 份文件经独立 Radiance C 样本核对，其中 11 份另经 imagecodecs 验证。四个 8-bit 编码分量包含共享指数，而非 alpha；曝光/色彩/方向变换、metadata 长度、native 支持/分配和最终 Mat 精度仍未知。

Sun Raster proves raw/old/RGB or byte-RLE completion at all supported 1/8/24/32-bit storage depths using exact 16-bit-padded rows and file ranges. RLE runs cross rows; Int64 expansion counts can exceed 2 GiB without allocating decoded pixels. Sixteen files have independent Netpbm C storage-byte checks, with ten also checked by Pillow. Header facts survive unknown completion; color maps, the 32-bit padding byte, native decoding/allocation and final Mat precision require separate evidence. The storage estimate rounds each sample up to a byte, including packed 1-bit input.

Sun Raster 的 raw/old/RGB 或 byte-RLE 完整性覆盖支持的全部 1/8/24/32-bit 存储深度，使用精确 16-bit 对齐行和文件范围。RLE run 可跨行，Int64 展开计数可超过 2 GiB，无需分配解码像素。16 份文件经独立 Netpbm C 存储字节核对，其中 10 份另经 Pillow 验证。完整性未知时保留头部事实；色表、32-bit padding 字节、native 解码/分配和最终 Mat 精度需要单独证据。存储估算将每个样本向上取整到字节，也包括位打包的 1-bit 输入。

`ConsoleSamples` 的 `typed-mat` 与 `buffered-codec` 命令提供可运行的机器可读示例；没有 native runtime 时明确 skipped，不能把 skipped 当作能力已验证。

## Platform and headless boundaries / 平台与 headless 边界

`OperatingSystemDescription`, `ProcessArchitecture`, `RuntimeIdentifier`, and `ProcessBitness` are diagnostic fields. They are not support claims. Use the exact runtime package RID and require producer, package, independent consumer, loader, dependency, and native smoke evidence before promoting a target.

The headless profile is candidate-only. It keeps the managed API shape, omits the native HighGUI module, and requires deterministic `NOT_LINKED`/unsupported behavior for GUI calls, codec smoke, VideoIO results, and no-display execution. It has no active NuGet identity until the profile-specific ABI and dependency evidence is complete. The same rule applies to generic Linux ARM64 and domestic Linux candidates: architecture or cross-compilation alone is insufficient.

平台字段是诊断信息，不是支持声明。headless profile 仍是 candidate-only；在 profile-specific ABI、依赖、独立 consumer 和无显示环境 smoke 闭合前，不创建 active NuGet identity。generic Linux ARM64 与国产平台也必须完成真实 producer/package/consumer 证据，不能仅凭架构或交叉编译晋升。

## Evidence commands / 证据命令

```powershell
pwsh -NoProfile -File .\scripts\Test-CapabilitiesJsonContract.ps1
pwsh -NoProfile -File .\scripts\Test-ModernApi51Evidence.ps1
pwsh -NoProfile -File .\scripts\Test-GenericLinuxArm64PreviewMatrix.ps1
pwsh -NoProfile -File .\scripts\Test-HeadlessRuntimeProfileContract.ps1
dotnet test .\tests\OpenCvSharp.Tests\OpenCvSharp.Tests.csproj -c Release -f net8.0 --filter "FullyQualifiedName~ImagePreflightTests|FullyQualifiedName~GifMetadataPreflightTests|FullyQualifiedName~AvifPrimaryItemPreflightTests"
dotnet test .\tests\OpenCvSharp.Tests\OpenCvSharp.Tests.csproj -c Release -f net8.0 --filter FullyQualifiedName~CodecBufferLeaseTests
```

These checks prove the local contract and focused behavior. They do not replace a factual native runtime, hosted ARM64 consumer, or vendor certification evidence.
