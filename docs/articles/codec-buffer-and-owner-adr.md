# Codec Buffer And Owner ADR / 编解码缓冲区与所有权 ADR

## Status

Accepted for the 5.0.1 P0 design boundary. Existing `byte[]` encode/decode APIs remain stable. A writer backed encode API is a conditional preview until allocation, target framework, and owner evidence are complete.

## Scope

This record defines the boundary for encoded bytes, streams, `IBufferWriter<byte>`, native encoded buffers, and decode preflight. It does not claim that a codec is available merely because an extension is recognized, and it does not claim a native peak allocation limit from a managed estimate.

本记录定义编码字节、流、`IBufferWriter<byte>`、native encoded buffer 和解码预检的边界。识别扩展名不能证明 codec 一定可用；managed 估算也不能冒充 native 峰值分配限制。

## Decisions

1. Existing `ImEncode` overloads returning `byte[]` remain the compatibility baseline. Their native buffer is released before the method returns, and the returned array owns its bytes.
2. `ImageDecodeOptions` is an admission policy. `MaxInputBytes`, dimensions, pixels, frames, metadata, ICC, encoded pixel storage estimates, and strict-known flags run before native `ImDecode`. Unknown facts remain unknown unless the caller selects a fail-closed option.
3. Stream preflight reads from the current position. A seekable stream is restored in `finally`, including validation failures. A non-seekable stream is consumed and must be made replayable by the caller when preservation is required.
4. The preview writer API is named `ImEncodeTo` instead of overloading `ImEncode`. This avoids making existing calls such as `ImEncode(ext, mat, null)` ambiguous with the existing `int[]` parameter overload. It writes one completed encoded result, validates `ext`, `Mat`, and parameter pairs before native entry, calls `GetSpan` only for the final payload, calls `Advance` exactly once after the copy, and never retains the writer, span, or caller memory after return.
5. Writer exceptions are terminal for that call. The API must not retry native encoding or write a second segment after `GetSpan` or `Advance` throws. A writer may have partially advanced its own storage; rollback is the writer's responsibility and must be documented as such.
6. Native encoded buffers remain owned by the wrapper until copied. The wrapper releases them in `finally`; no native pointer or lease crosses the public API. An external buffer lease therefore stays internal until a cross-TFM owner and release callback contract is proven.
7. The stable package target matrix does not acquire a new third-party buffer package for this feature. `IBufferWriter<byte>` and Span-based overloads may be compiled only on target frameworks that expose the required BCL contract; old .NET Framework targets retain the existing array and stream paths.
8. The first writer implementation, if promoted, must reuse the existing native encoded buffer and perform one managed copy. A callback-based native ABI is a separate performance decision and cannot be introduced merely to give the overload a low-allocation name.

## Required validation

- Null, empty, invalid extension, disposed matrix, odd parameter pairs, zero-length destination, and writer faults are rejected before or at the documented boundary.
- A writer test covers a single segment, a segmented writer, an undersized `GetSpan` result, `Advance` failure, and a writer that throws after accepting the span.
- Stream tests cover seekable position restoration on success and failure, short reads, input-limit rejection, empty streams, and non-seekable consumption.
- Decode tests cover unknown format, unknown dimensions, malformed headers, cumulative pixel overflow, metadata/ICC limits, and encoded pixel storage overflow.
- Every native buffer and partially constructed managed result is released on success and failure.

## Current preview evidence

`tests/OpenCvSharp.Tests/ImgCodecs/Cv2InteropTests.cs` now includes an exact caller-owned segment writer. It verifies one `GetSpan`/`Advance` pair, the requested payload length, and byte-for-byte equality with the compatibility `byte[]` encoder on both `net8.0` and `net10.0`. The benchmark also measures a single preallocated writer reused for all 100 iterations, so writer construction and growth are outside the measured loop. [`codec-writer-native-evidence.json`](../../packaging/performance/codec-writer-native-evidence.json) records the focused native run for full and mini Windows x64 runtime packages: four rows (full/mini x net8.0/net10.0), 32/32 passed, native smoke enabled, and exact package/payload hashes. [`codec-writer-allocation-evidence.json`](../../packaging/performance/codec-writer-allocation-evidence.json) adds four more native rows (8/8 passed) covering 64 repeated caller-owned writes and repeated `Advance` faults followed by a successful encode. The writer remains preview-only; this evidence strengthens the allocation and owner/lifetime review but does not establish a cross-runner stable allocation guarantee.

当前 preview 已加入 exact caller-owned segment writer 测试：验证一次 `GetSpan`/`Advance`、请求长度以及与兼容 `byte[]` encoder 的逐字节一致性，并在 `net8.0` 与 `net10.0` 通过。benchmark 使用一个预分配 writer 重复 100 次并记录一次 `GetSpan`/`Advance` 对，避免把 writer 创建和扩容成本混入 API 路径。[`codec-writer-native-evidence.json`](../../packaging/performance/codec-writer-native-evidence.json) 记录 Windows x64 full/mini runtime 的 focused native 矩阵：full/mini x net8.0/net10.0 共四行、32/32 passed、native smoke 已启用，并绑定准确的 package/payload hash。[`codec-writer-allocation-evidence.json`](../../packaging/performance/codec-writer-allocation-evidence.json) 追加四行 native owner/lifetime 矩阵，重复写入与 Advance fault 后恢复共 8/8 passed。writer 仍保持 preview；这些证据加强了 allocation 与 owner/lifetime 评审，但尚未形成跨 runner 的稳定分配保证。

## Promotion gate

The writer overload remains preview or internal until it has focused tests on every supported modern TFM, a fixed-input allocation comparison against `byte[]`, and owner/lifetime review. No stable API claim is made from a benchmark on one runner. The existing byte-array API remains the rollback path.

writer overload 在所有支持的现代 TFM 上完成 focused tests、与 `byte[]` 的固定输入分配对比以及 owner/lifetime review 之前，保持 preview 或 internal。单 runner benchmark 不能形成稳定 API 声明。现有 byte-array API 是回退路径。
