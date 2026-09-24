# Codec Buffer Lease ADR / 编解码缓冲区 Lease ADR

## Status

Accepted as an internal/P1 prototype boundary for 5.0.1. The checked `CodecBufferLease` prototype is internal only; no stable public `MatBufferLease` type is added by this record.

## Problem

OpenCV can consume a pointer, row stride, and byte length without copying, but a managed array may move and a `Memory<T>` owner may be disposed while native code still uses it. A lease must keep the owner alive, pin only for the native call lifetime, and release exactly once.

OpenCV 可以在不复制的情况下消费指针、行 stride 和字节长度，但 managed array 可能移动，`Memory<T>` owner 也可能在 native 仍使用时被释放。Lease 必须保持 owner 存活，只在 native 调用期间 pin，并且只释放一次。

## Decisions

1. The prototype accepts a pinned managed array owner or an explicitly owned native pointer. It never accepts an arbitrary `IntPtr` without a release callback or an explicit non-owning lifetime token.
2. A lease records `data`, `lengthBytes`, `stepBytes`, `rows`, and the Mat element contract. All products and offsets use checked arithmetic; `stepBytes` must be at least the logical row payload and `lengthBytes` must cover `(rows - 1) * stepBytes + rowPayload`.
3. The managed array owner remains referenced by the lease. `GCHandle.Alloc(..., Pinned)` is created once and freed once in `Dispose`; repeated disposal is harmless. A release callback is invoked at most once and exceptions are captured as an internal diagnostic rather than crossing a native boundary.
4. A lease cannot be used after disposal. Native calls must acquire a short operation guard that keeps the lease alive until the call returns; the lease must not expose a span that survives the guard.
5. The first prototype does not create a `Mat` header or add a C ABI entry. It only validates ownership and layout so a future native `Mat` factory can consume a reviewed lease contract.
6. Legacy .NET Framework targets retain explicit pinned-array/internal APIs. `Memory<T>`, `ReadOnlyMemory<T>`, and `IBufferMemory` abstractions remain out of the stable cross-TFM surface until the BCL owner and callback semantics are proven.

## Required tests before implementation promotion

- Null owner, empty owner, zero rows/stride, negative dimensions, stride underflow, checked multiplication overflow, and insufficient byte length fail before pinning.
- Dispose is idempotent; release callback runs once; callback exceptions do not cross the lease disposal boundary.
- A native-call harness proves the owner remains alive during the operation and that access after disposal is rejected.
- ROI and external stride cases validate the final row boundary without exposing padding as image pixels.
- A stress loop covers pin/unpin and callback release under exceptions.

## Current prototype evidence

`src/OpenCvSharp/Internal/CodecBufferLease.cs` implements the pinned-array path and an explicitly owned native-pointer path. It validates the owner, byte offset, rows, stride, row payload, checked final-row boundary, and pointer arithmetic before pinning or wrapping. `EnterOperation()` increments a short native-call guard; `Dispose()` defers unpinning until all guards exit, and repeated disposal is harmless. `tests/OpenCvSharp.Tests/Core/CodecBufferLeaseTests.cs` covers layout rejection, ROI-style offsets, row pointers, deferred release, callback exception capture, native-pointer callback ownership, and 256 pin/unpin iterations on both `net8.0` and `net10.0`. The prototype does not create a Mat header or native ABI entry.

当前 prototype 已落地到 `src/OpenCvSharp/Internal/CodecBufferLease.cs`，同时支持 pinned array 和带 release callback 的显式 native pointer。它在 pin 或 wrap 之前校验 owner、offset、rows、stride、row payload、末行边界和 checked 指针算术；`EnterOperation()` 提供短 native-call guard，`Dispose()` 等待 guard 退出后再 unpin，重复释放安全。测试覆盖布局拒绝、ROI 风格 offset、行指针、延迟释放、callback 异常隔离、native pointer owner 以及 256 次 pin/unpin，并在 `net8.0` 与 `net10.0` 通过；`net46` 编译通过。prototype 不创建 Mat header 或 native ABI。

## Stop and rollback

If any target framework cannot express the owner lifetime or if a native factory would retain the pointer beyond the guarded call, keep the lease internal and use `Mat` copy APIs. The rollback path is the existing managed copy/clone contract; no stable package or ABI identity is created for the prototype.

如果任一目标框架无法表达 owner 生命周期，或 native factory 会在 guard 结束后继续保留指针，则 lease 保持 internal，使用现有 `Mat` copy/clone 契约。回退路径是现有 managed copy/clone，不创建稳定 package 或 ABI 身份。
