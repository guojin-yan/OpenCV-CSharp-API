# Codec Preflight And Limits / 编解码预检与限制

Cv2.Identify performs a managed header inspection before native decoding. It recognizes common PNG, JPEG, GIF, WebP, BMP, TIFF, and PNM signatures, reports dimensions when the header proves them, and never opens the native runtime. It is intended for admission control and diagnostics, not for claiming that a file is valid or that every codec feature is available.

Cv2.Identify 会在 native 解码前执行 managed 头检查。它识别常见的 PNG、JPEG、GIF、WebP、BMP、TIFF 和 PNM 签名，在头部能够证明时报告尺寸，并且不会加载 native runtime。它用于输入准入和诊断，不代表文件一定完整有效，也不代表所有 codec 功能都可用。

## Identify First / 先识别

~~~csharp
ImageIdentifyResult identified = Cv2.Identify(encodedBytes);
if (identified.IsSizeKnown && (long)identified.Width * identified.Height > 100000000)
{
    throw new InvalidDataException("Image is larger than the application budget.");
}
~~~

ImageIdentifyResult distinguishes an unknown format, unknown dimensions, and a proven single-frame header. Classic TIFF (version 42, little- or big-endian) and BigTIFF (version 43 with 64-bit IFD offsets) report dimensions, page count, and cumulative pixels from a complete, bounded next-IFD chain when those facts are provable. Heterogeneous page sizes keep the overall width/height fact unknown, but the parser can still prove `CumulativePixelCount` by summing every page when all page dimensions are known. Cyclic/out-of-range directories and incomplete directories keep the affected facts unknown; an unknown fact must not be treated as zero-sized input. BigTIFF facts are managed preflight facts and do not imply that the native codec is available.

ImageIdentifyResult 区分未知格式、未知尺寸和已证明的单帧头部。经典 TIFF（版本 42，支持小端/大端）和 BigTIFF（版本 43、64 位 IFD 偏移）会在事实可证明时，从完整且有界的 next-IFD 链报告尺寸、页数和累计像素。页尺寸不一致时，整体宽高事实保持未知；但只要每一页的尺寸都已知，解析器仍可通过逐页求和证明 `CumulativePixelCount`。循环/越界目录或目录不完整时，受影响事实保持未知；未知事实不能被当作零尺寸输入。BigTIFF 事实属于托管层预检事实，并不代表 native codec 一定可用。

`Identify(Stream, ImageDecodeOptions)` applies the same admission policy while reading from the stream's current position. It reads no more than `MaxInputBytes + 1` bytes, restores the original position for seekable streams even when validation fails, and consumes non-seekable streams. This is deliberately a whole-input preflight rather than an incremental decoder: callers that need to preserve a non-seekable source must provide their own replayable buffering boundary.

`Identify(Stream, ImageDecodeOptions)` 会从流的当前位置读取并应用同一准入策略。它最多读取 `MaxInputBytes + 1` 字节；对于可 seek 流，即使验证失败也会恢复原位置；对于不可 seek 流则会消费输入。这是有意设计为完整输入预检，而非增量解码器：需要保留不可 seek 源的调用方必须自行提供可重放的缓冲边界。

For structurally complete GIF, APNG, and animated WebP containers, `FrameCount` is counted from the parsed frame/image records. APNG sequence numbers, declared frame count, WebP animation chunks, GIF image descriptors/extensions, and the final container terminator must all be consistent; truncation, trailing bytes, or an incomplete frame leave `IsFrameCountKnown` false. JPEG is treated as a known single frame only when a valid EOI marker terminates the input. Static PNG/WebP/GIF inputs report one frame when their single-image boundary is proven.

对于结构完整的 GIF、APNG 和动画 WebP 容器，`FrameCount` 根据已解析的帧/图像记录统计。APNG 序列号与声明帧数、WebP 动画块、GIF 图像描述符/扩展块以及容器结束标记必须一致；截断、尾部多余字节或未完成帧都会使 `IsFrameCountKnown` 保持为 false。JPEG 只有在合法 EOI 标记位于输入末尾时才报告已知单帧。能够证明单图边界的静态 PNG/WebP/GIF 报告一帧。

## Decode With A Budget / 带预算解码

~~~csharp
var options = new ImageDecodeOptions(
    maxInputBytes: 64L * 1024 * 1024,
    maxWidth: 16384,
    maxHeight: 16384,
    maxPixels: 100000000,
    maxFrames: 64,
    rejectUnknownFormat: true,
    requireKnownSize: true,
    maxMetadataBytes: 8L * 1024 * 1024,
    maxIccProfileBytes: 2L * 1024 * 1024,
    requireKnownMetadataSize: true,
    requireKnownIccProfileSize: true);

using (Mat image = Cv2.ImDecode(encodedBytes, options, ImreadModes.Color))
{
    // Native decoding starts only after the managed budget checks pass.
}
~~~

The same policy is available for a stream. Its seek/consumption behavior is identical to `Identify(Stream, ImageDecodeOptions)`.

对于流也可以使用同一策略，其 seek/消费语义与 `Identify(Stream, ImageDecodeOptions)` 完全相同。

~~~csharp
using (Mat image = Cv2.ImDecode(uploadStream, options, ImreadModes.Color))
{
    // A seekable uploadStream remains at its original position after this call.
}
~~~

The overload checks encoded byte length, recognized format, known dimensions, checked width-times-height arithmetic, known frame/page count, cumulative width-times-height budget, encoded depth/channel limits when proven, and known metadata/ICC payload limits before calling the existing native ImDecode. The original overloads remain unchanged for applications that already enforce their own input policy. A preflight pass does not replace sandboxing, codec patching, timeout controls, or post-decode validation.

该重载会在调用现有 native ImDecode 前检查编码字节长度、可识别格式、已知尺寸、checked 的宽高乘法、累计宽高像素预算、已证明的编码精度/通道上限以及已知 metadata/ICC 负载上限。原有重载保持不变，已经有自有输入策略的应用可以继续使用。预检不能替代沙箱、codec 更新、超时控制或解码后的结果校验。

## Metadata And ICC Facts / Metadata 与 ICC 事实

For structurally complete PNG, JPEG, and WebP containers, `ImageIdentifyResult` reports `MetadataBytes` and `IccProfileBytes` when the managed parser can prove their serialized payload lengths. The selected metadata count includes PNG `tEXt`/`zTXt`/`iTXt`/`eXIf`/`iCCP`, JPEG APPn and COM segments, and WebP EXIF/XMP/ICCP chunks. It is a stored-byte admission fact, not an estimate of expanded EXIF, XMP, compressed PNG `iCCP`, or native decoder allocation. For other formats, truncated containers, or unsupported container details, the corresponding fact is unknown.

对于结构完整的 PNG、JPEG 和 WebP 容器，若 managed 解析器能够证明序列化负载长度，`ImageIdentifyResult` 会报告 `MetadataBytes` 和 `IccProfileBytes`。选定的 metadata 统计包括 PNG 的 `tEXt`/`zTXt`/`iTXt`/`eXIf`/`iCCP`、JPEG APPn/COM 段，以及 WebP EXIF/XMP/ICCP 块。该数值是存储字节的准入事实，不是展开后的 EXIF、XMP、压缩 PNG `iCCP` 或 native 解码分配量的估算。对于其他格式、截断容器或尚未支持的容器细节，相应事实为未知。

`MaxMetadataBytes` and `MaxIccProfileBytes` reject only known facts. Set `RequireKnownMetadataSize` and `RequireKnownIccProfileSize` for a fail-closed boundary; otherwise an unknown metadata fact remains admissible, like an unknown image size when `RequireKnownSize` is disabled.

`MaxMetadataBytes` 和 `MaxIccProfileBytes` 只拒绝已知事实。设置 `RequireKnownMetadataSize` 和 `RequireKnownIccProfileSize` 可形成失败关闭边界；否则未知 metadata 事实仍会被允许，这与未启用 `RequireKnownSize` 时的未知图像尺寸一致。

## Policy Boundaries / 策略边界

- ImageDecodeOptions is an admission budget, not a promise of peak native allocation.
- Stream preflight buffers the complete admitted input in managed memory; MaxInputBytes limits that buffer but does not bound native peak allocation.
- Unknown dimensions or frame counts are accepted by default unless RequireKnownSize is enabled; applications handling untrusted input should choose an explicit policy.
- A recognized signature can still be truncated, malformed, or hostile. Native failure remains possible and must be handled.
- Other format-specific multi-frame parsers, native peak-allocation accounting, decoded color/final precision facts, and IBufferWriter output remain separate follow-up contracts. Classic TIFF and BigTIFF cumulative accounting is available only when every inspected page dimension is proven.

- ImageDecodeOptions 是输入准入预算，不是 native 峰值内存承诺。
- 流预检会将已准入的完整输入缓冲到 managed 内存中；MaxInputBytes 限制该缓冲，但不限制 native 峰值分配。
- 默认允许未知尺寸或帧数；处理不可信输入时应显式设置策略，例如启用 RequireKnownSize。
- 已识别的签名仍可能被截断、损坏或恶意构造，native 失败仍然可能发生，调用方必须处理。
- 其他格式专用多帧解析器、native 峰值分配核算、解码后颜色/最终精度事实和 IBufferWriter 输出仍属于后续独立契约。经典 TIFF 与 BigTIFF 的累计核算仅在每个已检查页面尺寸均可证明时生效。
## Pixel Format And Multi-Frame Budgets / 像素格式与多帧预算

For PNG, the parser reports `BitDepth` and channel count from a valid IHDR color type. For JPEG, it reports sample precision and component count from the first SOF marker. WebP reports 8-bit RGB/RGBA when a complete VP8/VP8L image header or VP8X alpha flag proves the layout. Classic TIFF and BigTIFF report a uniform `BitsPerSample` value and `SamplesPerPixel` value when every inspected page proves the same values. PNM P1/P4 reports its one-bit monochrome header, while P2/P5 and P3/P6 report one or three channels and 8- or 16-bit storage implied by `maxval`. These are encoded-header facts; they do not promise the final `Mat` depth or channel layout after `ImreadModes` conversion. Other formats remain unknown until their headers can be proven without guessing.

对于 PNG，解析器从合法 IHDR 色彩类型报告 `BitDepth` 和通道数；对于 JPEG，解析器从第一个 SOF 标记报告样本精度和组件数。WebP 在完整 VP8/VP8L 图像头或 VP8X alpha 标志能够证明布局时报告 8-bit RGB/RGBA。经典 TIFF 和 BigTIFF 在每个已检查页面都能证明相同值时报告统一的 `BitsPerSample` 和 `SamplesPerPixel`。PNM 的 P1/P4 报告头部规定的单通道 1-bit；P2/P5 与 P3/P6 报告一个或三个通道，以及由 `maxval` 推导出的 8/16-bit 存储宽度。这些是编码头事实，不承诺 `ImreadModes` 转换后的最终 `Mat` 深度或通道布局。其他格式只有在无需猜测且头部可证明时才会报告，否则保持未知。

`MaxBitDepth` and `MaxChannels` reject only known encoded facts. Set `RejectUnknownPixelFormat` when the admission boundary must fail closed. `MaxCumulativePixels` first uses a format parser's proven cumulative frame/page pixel count (including heterogeneous classic TIFF and BigTIFF pages); otherwise it multiplies the proven width-times-height by the proven frame/page count with checked arithmetic. It is an admission budget, not a peak native allocation guarantee. If no complete cumulative fact can be proven, the budget check remains non-blocking unless a separate strict policy rejects the unknown dimensions or frame count.

`MaxBitDepth` 和 `MaxChannels` 只拒绝已知的编码事实；需要失败关闭时设置 `RejectUnknownPixelFormat`。`MaxCumulativePixels` 会优先使用格式解析器已证明的帧/页累计像素数（包括页尺寸不一致的经典 TIFF 与 BigTIFF）；否则使用 checked 算术将已证明的宽高像素数乘以已证明的帧/页数。它是输入准入预算，不是 native 峰值分配保证。如果无法证明完整累计事实，预算检查不会单独阻断输入，除非另有严格策略拒绝未知尺寸或帧数。

The preflight test suite includes a deterministic malformed corpus: every strict prefix of complete PNG, JPEG, GIF, APNG, WebP, classic TIFF, and BigTIFF fixtures is inspected, together with oversized container lengths and broken TIFF directory pointers. These cases must return unknown affected facts without leaking indexing, arithmetic, or native exceptions. This is evidence for the managed parser's bounded behavior only; native decoder isolation, timeout, and peak-allocation controls remain separate deployment responsibilities.

预检测试套件包含确定性的 malformed corpus：对完整 PNG、JPEG、GIF、APNG、WebP、经典 TIFF 和 BigTIFF fixture 的每一个严格前缀执行识别，并加入超大容器长度和损坏 TIFF 目录指针。此类输入必须将受影响事实置为未知，不能泄漏索引、算术或 native 异常。这只证明 managed 解析器的有界行为；native 解码隔离、超时和峰值分配控制仍属于部署侧职责。
