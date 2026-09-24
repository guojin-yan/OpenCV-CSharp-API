# Async Video Owner ADR / 异步视频所有权 ADR

## Status

Accepted as a 5.0.1 P1 design boundary. Existing synchronous `VideoCapture`, `TryRead`, `TryRetrieve`, `WaitAny`, and `VideoStreamReader` remain the stable surface. No public async frame API is promoted by this record.

## Proposed model

A future file-oriented API may expose `IAsyncEnumerable<VideoFrame>` with a dedicated producer loop. `VideoFrame` owns one independent `Mat`, carries frame index and timestamps when the backend reports them, and is disposed by the consumer. The producer never reuses or overwrites a frame after yielding it.

未来文件型 API 可以通过专用 producer loop 暴露 `IAsyncEnumerable<VideoFrame>`。`VideoFrame` 拥有一个独立 `Mat`，在 backend 提供时携带 frame index 和 timestamps，并由 consumer 负责 dispose。producer yield 之后不能复用或覆盖该 frame。

## Queue and backpressure

- Options must define bounded capacity and one policy: `Block`, `DropOldest`, or `DropNewest`.
- Dropped frames are disposed by the queue owner before removal; cancellation drains and disposes every queued frame.
- The default policy is bounded `Block` for file input. Unbounded queues and silent frame loss are not allowed.
- A consumer cannot retain a pooled frame after returning it. Pooling is not part of the first public contract; independent owners are the default.

## Cancellation and backend threading

Cancellation stops the managed producer and drains the queue. It cannot promise that every native backend interrupts an already blocking `Read`; the API must document a backend-specific cancellation latency and cleanup deadline. A backend that cannot meet the deadline stays synchronous/preview. One `VideoCapture` cannot be read synchronously and asynchronously at the same time.

`VideoStreamReader` already pins callback buffers only for each read and stores callback exceptions for the managed caller. An async layer must preserve that behavior, keep `leaveOpen` semantics, and never let callback exceptions escape unmanaged code.

## End-of-stream and failure states

The contract distinguishes successful frame, ordinary EOF, timeout, cancellation, backend disconnect, and decode/backend error. `Read()` returning false is not converted into a generic exception. Producer failure completes the async sequence after queued frames are disposed and exposes the original bounded exception.

## Promotion gate

Before public preview, the implementation needs a deterministic file fixture, a cancellable fake/test backend, queue cleanup tests, producer/consumer disposal stress, EOF/error separation, and one real backend evidence record. Camera/network backends remain separate because their cancellation and threading behavior is backend-specific. Until those gates pass, consumers use synchronous `Read`/`TryRead` and `WaitAny`.

在 deterministic 文件 fixture、可取消 fake/test backend、queue cleanup、producer/consumer dispose stress、EOF/error 分离和至少一个真实 backend 证据完成前，不公开 async API。摄像头/网络 backend 单独评估；当前消费者继续使用同步 `Read`/`TryRead` 和 `WaitAny`。
