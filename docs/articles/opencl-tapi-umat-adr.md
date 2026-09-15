# OpenCL / T-API / UMat boundary ADR

Status: research-only; the 5.0.1 runtime remains CPU-first.

This ADR answers a deceptively simple question: does the presence of an OpenCL
symbol, a `UMat` type, or a DNN OpenCL target mean that this library accelerates
an operation with a GPU? The answer is no. OpenCL is an optional execution
backend, while T-API is a storage and dispatch path that may fall back to CPU.
CUDA/GpuMat is a separate backend and is not implied by either of them.

## Decision

The 5.0.1 managed API and published runtime packages do not claim OpenCL, T-API,
UMat, CUDA, or GPU acceleration. The active package matrix remains CPU-only and
does not carry an OpenCL ICD, a vendor driver, a CUDA runtime, or a GPU-specific
package identity. Existing DNN target enums and upstream compatibility records
are descriptive surfaces; they are not proof that a device or an operation is
available.

An eventual implementation must pass through three distinct capability states:

| State | Meaning | What it may be used for |
| --- | --- | --- |
| `Declared` | The native build or managed enum exposes a backend name. | Diagnostics and compatibility reporting only. |
| `Available` | OpenCV can initialize the backend on this process and device. | An opt-in probe may report availability; no performance claim. |
| `Verified` | A fixed operation pipeline actually executes on the backend and passes correctness, synchronization, and performance evidence. | A versioned opt-in feature contract may make a backend claim. |

`HaveOpenCL`, platform enumeration, a driver package, `setUseOpenCL(true)`,
`DnnTarget.OpenCL`, or a CUDA-named method can move a probe to `Available`; none
can move it to `Verified`. A `Verified` result is operation- and profile-specific
and must never be collapsed into one `HasGpu` boolean.

## Thread-local context and ownership

OpenCV's OpenCL context and command queue are process resources with thread-local
execution implications. A managed wrapper must not install a mutable global
context behind an apparently pure method. A future API must either:

1. own a scoped execution context and queue for the calling operation, or
2. accept an explicitly owned context whose lifetime, device identity, and
   thread affinity are observable.

The wrapper must document whether a context is borrowed or owned, make disposal
idempotent, reject use after disposal, and prevent a queue from being used by a
different managed context without an explicit hand-off. Parallel calls must not
silently share a queue, and a failed context initialization must leave the CPU
path usable.

## Fallback and synchronization

Fallback is a semantic decision, not merely an implementation detail. If a
backend is unavailable, unsupported for an operation, or times out, the caller
must receive a structured `Unavailable`/`Unsupported`/`Failed` outcome or an
explicitly requested CPU fallback. A silent fallback cannot be reported as GPU
execution and cannot be used to publish a speed-up number.

An eventual T-API surface must define when a pending queue is synchronized. The
boundary includes an explicit `Finish`/wait operation, disposal behavior, copy
behavior, exception behavior, and timeout/cleanup behavior. A readback to a
`Mat`, a native pointer borrow, or a managed span is a synchronization point
unless the API says otherwise. No object may be disposed while a queued command
still refers to its storage.

## Mat and UMat conversion rules

`Mat` and `UMat` are not interchangeable views. A conversion may borrow storage,
copy storage, or enqueue a device transfer; the contract must expose which one
occurs. The following are mandatory before publication:

- lifetime and ownership for source, destination, and temporary storage;
- continuity, row stride, ROI, depth, channels, and alignment preservation;
- copy direction and synchronization point for every conversion;
- behavior when an OpenCL device cannot represent the requested format;
- deterministic disposal and no use of a borrowed native handle after its owner
  is disposed.

The current managed API intentionally stays on `Mat`/CPU semantics. It does not
add a fake `UMat` wrapper merely to mirror an upstream class name.

## DNN and other backend names

DNN backend/target enums describe requested targets. Availability must be queried
through the native capability probe (for example, `GetAvailableTargets`) and
reported per device. A target that is enumerated but returns no usable device is
`Declared`, not `Available`. CUDA, OpenCL, CANN, TIM-VX, Vulkan, and vendor NPU
paths each require separate capability, package, and evidence records.

## Benchmark and promotion protocol

No GPU/OpenCL claim can be promoted without a reproducible benchmark record. A
record must pin the OpenCV revision, runtime package hash, OS/libc, driver and
ICD versions, device identity, input corpus hash, operation graph, warm-up and
iteration counts, synchronization points, timeout, peak RSS, CPU baseline,
device time, transfer time, numerical tolerance, and fallback outcome. Cold and
warm runs are reported separately; end-to-end transfer overhead is included.
The same pipeline must pass on an independent consumer, not only on the build
machine. Correctness is checked after every required readback, and a timeout or
driver reset is a failed run with cleanup evidence.

The first candidate may only be a separate research profile. It needs native
producer and independent consumer evidence, an explicit ICD/driver dependency
manifest, a package identity that cannot be selected by the CPU package, and a
rollback path to the current CPU matrix. Until those gates pass, the supported
answer is `Unsupported`/CPU fallback.

## Explicit non-claims and stop conditions

This repository does not claim GPU acceleration because:

- OpenCV was built with `WITH_OPENCL`, an OpenCL header is present, or an ICD is
  installed on a developer machine;
- `UMat`, `ocl`, `OpenCL`, `GpuMat`, or DNN target names occur in upstream data;
- a benchmark measures only a kernel and excludes host-device transfers;
- a QEMU/emulated device, cross-compiled binary, or one-off local driver run
  succeeds;
- a CPU fallback returns the correct pixels.

The experiment stops and stays research-only when context lifetime cannot be
made deterministic, a required driver/ICD is unbounded, a pipeline mixes CPU
and device work without observable synchronization, numerical behavior is not
within the declared tolerance, or an independent consumer cannot reproduce the
result. Rollback removes only the candidate profile; it never changes RID
fallback precedence or the CPU package identity.

## References inside this repository

- `docs/articles/core-upstream-parity-guide.md` — allocator/OpenCL/UMat surfaces
  are not promoted by the core parity baseline.
- `docs/articles/dnn-structured-parity-guide.md` — target availability is
  queried separately from enum presence.
- `docs/articles/generic-linux-rid-adr.md` — CPU feature enumeration and build
  flags are not runtime portability or GPU evidence.
- `packaging/runtime/JYPPX.OpenCV.runtime/README.md` — current runtime wording
  keeps GPU-named paths CPU-safe and does not claim CUDA execution.
- `packaging/runtime/runtime-opencl-tapi-umat-contract.json` — the machine
  readable contract enforced by the invariant suite.
