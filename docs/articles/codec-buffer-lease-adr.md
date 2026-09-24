# Codec Buffer Lease ADR / 编解码缓冲区 Lease ADR

## Status

Accepted as an internal/P1 prototype boundary for 5.0.1. No stable public `MatBufferLease` type is added by this record.

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

## Stop and rollback

If any target framework cannot express the owner lifetime or if a native factory would retain the pointer beyond the guarded call, keep the lease internal and use `Mat` copy APIs. The rollback path is the existing managed copy/clone contract; no stable package or ABI identity is created for the prototype.

如果任一目标框架无法表达 owner 生命周期，或 native factory 会在 guard 结束后继续保留指针，则 lease 保持 internal，使用现有 `Mat` copy/clone 契约。回退路径是现有 managed copy/clone，不创建稳定 package 或 ABI 身份。
