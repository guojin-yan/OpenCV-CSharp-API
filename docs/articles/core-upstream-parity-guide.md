# Core Upstream Coverage And Ownership Guide

This guide describes the deterministic OpenCV `5.0.0` public Core compatibility include closure and the completed array/reduction/transform, persistence, numerical/collection/solver, and runtime diagnostics/timing families. It is a measured module slice, not a claim that every OpenCV C++ API in the repository is bound.

## Measured Header Closure

The checked extraction uses OpenCV's own `modules/python/src2/hdr_parser.py` with `CV_VERSION_MAJOR=5` and `OPENCV_ABI_COMPATIBILITY=500`. It parses the public contributors reached through `opencv2/core.hpp`: `base.hpp`, `cvstd.hpp`, `traits.hpp`, `matx.hpp`, `types.hpp`, `mat.hpp`, `persistence.hpp`, `core.hpp`, `operations.hpp`, `utility.hpp`, and `optim.hpp`.

The 258 parser identities contain 34 enums, 9 classes/structs, and 215 callables. The callable partition is 176 implemented, 29 intentionally omitted, 5 unsupported, 5 upstream-conditional, and zero missing. The four selected families contain 108 declarations, 11 managed public type additions, and 226 managed public member additions. Raw output records source-header order and SHA256 values; the classification, map, summary, and selected-family files live under `compatibility/`. The guard includes 13 fail-closed fixtures for identity collapse, overload order, constructor confusion, source/parser/hash drift, false evidence, undocumented omissions, fixed-major names, conditional-build mistakes, and evidence ordering.

```powershell
pwsh -NoProfile -File ./scripts/Test-CoreUpstreamMap.ps1 `
  -DotNetPath C:\Users\guoji\.dotnet\dotnet.exe
```

Regenerate the raw artifact only against the pinned source tree and parser. Normal checks use the checked-in raw data and do not require Python.

## Shape And Depth Rules

`HasNonZero` and `FindNonZero` accept single-channel matrices. `FindNonZero` returns an owned `CV_32SC2` coordinate matrix; its orientation is an OpenCV implementation detail, so use `Total` for the point count rather than assuming rows equal count.

`ReduceArgMin` and `ReduceArgMax` operate on two-dimensional, single-channel input and accept axis `0` or `1`. Output is caller-owned or newly owned `CV_32SC1`. `lastIndex` chooses the last equal extremum instead of the first.

`FiniteMask` accepts `CV_32F` or `CV_64F` with one through four channels and produces `CV_8UC1`. Each output byte is `255` when all values at that element are finite and `0` when any channel is NaN or infinity.

`TransposeND` requires continuous, single-channel input and a complete dimension permutation. OpenCV does not support the same input/output storage for this operation, so the managed API rejects in-place output. `FlipND` accepts positive or negative axes in the OpenCV range and copies before reversing, which permits caller-owned output safely.

`Broadcast` requires a continuous, single-channel source and a non-empty `CV_32SC1` shape matrix. The target dimensionality must be at least the source dimensionality, and each aligned source dimension must equal the target or be `1`. Unsupported shape or element-size combinations remain native OpenCV failures and surface as `NativeException`.

## Values, Ordering, And Borders

`Psnr` requires identical matrix type, size, and dimensionality. `maxValue` must be finite and positive. Identical arrays yield a large finite result because OpenCV adds `DBL_EPSILON` before division.

`CheckRange` uses the half-open interval `[minValue,maxValue)` and always asks OpenCV for the first invalid position without printing diagnostic output. `CheckRangeResult.Position` is `(-1,-1)` when valid. NaN and infinity fail the check.

`Sort` returns values and `SortIdx` returns `CV_32S` indices. `SortFlags` combines a row/column axis with ascending/descending direction. `BorderInterpolate` follows exact OpenCV semantics: for example `Reflect` maps coordinate `-1` to `0`, while `Reflect101` maps it to `1`. `CopyMakeBorder` accepts non-negative border widths and uses a scalar only for border modes that consume it.

## Ownership And Aliasing

Every native output uses a caller-owned `jyppx_ocv_mat*`; no `cv::Mat`, STL vector, C++ reference, or exception crosses the C ABI. Managed overloads that return `Mat` allocate one owned wrapper and dispose it if native execution fails. Caller-owned overloads never replace or retain the managed pointer.

ROI inputs are borrowed ref-counted `Mat` headers. `FindNonZero`, masked `CopyTo`, reductions, finite masks, sorting, and border copying do not retain the ROI pointer after return. Disposing the parent before an ROI remains safe under OpenCV reference counting, while use-after-dispose is rejected by the managed handle boundary.

Masked `CopyTo` requires a same-size `CV_8UC1` mask. `TransposeND` explicitly rejects aliasing. Other operations follow OpenCV's documented in-place behavior; callers should use distinct output unless the operation is documented to allow aliasing.

## Persistence Modes And Values

`FileStorage` supports file and memory workflows in read, write, and append modes. Combine an operation with at most one `FormatXml`, `FormatYaml`, `FormatJson`, or `FormatYaml10` flag. `FormatAuto` uses input content or a file extension; memory writes should provide a format hint such as `memory.yml` or select an explicit format. `ReleaseAndGetString` is the terminal operation for a memory writer and returns the complete UTF-8 document.

OpenCV 5's YAML 1.2 emitter can place a leading standalone comment where its own parser cannot read it back. The native wrapper defers a comment written before the first value and attaches it to that first value, preserving the comment while keeping the generated stream self-readable. On Windows, OpenCV opens FileStorage paths through narrow `fopen`; the wrapper therefore converts validated UTF-8 file paths to the active Windows file code page and rejects paths that cannot be represented without substitution. Memory documents remain exact UTF-8 on every platform.

The managed `Write` overloads preserve 32-bit integers, Boolean values, 64-bit integers, doubles, strings, `Mat`, and vectors of strings. `StartWriteStruct` and `EndWriteStruct` delimit maps and sequences. Names are required for map members and top-level values; an empty name writes the next item while a sequence is active. A matrix read through `FileNode.ToMat` is an independently owned `Mat` header with OpenCV reference-counted data.

Map keys are returned in storage order through an owned native string-list handle. Sequences use indexed child access and managed enumeration; the C++ `FileNodeIterator` never crosses the ABI. Every child returned by an indexer or enumerator is independently disposable. String vectors are represented as sequence nodes and retain empty elements.

## Persistence Ownership And Generation

`FileStorage` owns an opaque native wrapper whose state is shared with every `FileNode` obtained from it. Disposing the managed storage wrapper only releases that wrapper: surviving nodes keep the native storage state alive, so reading them does not dereference a destroyed parent.

`Open`, `Release`, and `ReleaseAndGetString` advance the storage generation. Nodes capture the generation at creation, and any subsequent access through a stale node fails through the native error bridge. This prevents a node from silently referring to a different document after reopen. Disposing a node is idempotent and does not release sibling nodes or the managed storage wrapper.

Native-owned UTF-8 result and string-list handles have explicit release entrypoints. Managed wrappers release them on both success and failure paths. No `cv::FileStorage`, `cv::FileNode`, `cv::String`, STL collection, iterator, C++ reference, or exception is exposed through the C ABI.

## UTF-8 Boundary

Every persistence input crosses the native boundary as a pointer plus an explicit byte length. Both managed and native layers use strict UTF-8 validation. Empty strings and empty string vectors are supported, including a null data pointer with zero length. Embedded NUL characters are rejected because OpenCV treats paths, names, encoding labels, and serialized strings as C-style text rather than length-preserving binary data.

Returned documents, node names, node strings, and keys use native-owned UTF-8 result handles with explicit lengths. This avoids locale-dependent ANSI conversion and permits Unicode file paths, keys, and values on supported platforms.

## Numerical And Collection Rules

`CubeRoot` and `FastAtan2` reject NaN and infinity before entering OpenCV. `FastAtan2` returns degrees in `[0,360)` with the upstream approximately 0.3-degree accuracy contract.

`BatchDistance` treats rows as vectors. Both sources must have identical column counts and must be either `CV_8UC1` or `CV_32FC1`. Numeric L1, L2, and squared-L2 distances produce `CV_32F` by default; byte L1 or squared-L2 may explicitly use `CV_32S`, while Hamming modes require byte input and `CV_32S`. `indices` is present exactly when `k > 0`; a full pairwise matrix (`k=0`) has no index output. A mask is `CV_8UC1` with `src1.Rows` by `src2.Rows` shape. Cross-check is valid only for `k=1`, no mask, and `update=0`. Output Mats may be initially empty but cannot alias either input or each other.

`Split` returns independently owned `Mat` wrappers through a count/fill ABI. A partial native allocation failure releases every channel already created. `PatchNaNs` modifies `CV_32F` or `CV_64F` input in place and preserves non-contiguous ROI stride; it does not replace infinities.

## Covariance And PCA

`CalcCovarMatrix` requires exactly one of `CovarFlags.Rows` and `CovarFlags.Cols`. `Normal` selects feature covariance; omitting it selects OpenCV's scrambled sample covariance. `UseAverage` changes `mean` from output to input/output and requires a row or column vector matching the chosen sample orientation. Output depth is `CV_32F`, `CV_64F`, or `-1` for source-derived behavior.

The four `PcaCompute` overloads preserve the upstream distinction between maximum component count and retained variance, and between requested or omitted eigenvalue output. Data is interpreted as row samples and must be non-empty `CV_32FC1` or `CV_64FC1`. An empty caller-owned mean asks OpenCV to calculate it; a non-empty mean is a one-row floating vector matching the feature count. Retained variance is finite in `(0,1]`; OpenCV deliberately retains at least two components for that overload when enough components exist.

`PcaProject` maps row samples from feature space to component space. `PcaBackProject` maps component rows back to feature space. Managed return overloads own their result and dispose it after native failure. Caller-owned results cannot alias data, mean, or eigenvectors. The parser-emitted `SVDecomp` and `SVBackSubst` rows reuse the already established `Svd.Compute` and `Svd.BackSubst` ABI/API rather than adding duplicate `Cv2` names.

## Random State And Linear Programming

`SetRngSeed`, `Randu`, and `Randn` use OpenCV's default generator, which is thread-local rather than process-global. Deterministic workflows must seed and consume values on the same thread. Mat parameter overloads preserve the upstream `InputArray` form; Scalar overloads cover the common one-to-four-channel case. Random destinations must be preallocated. Scalar standard deviations are finite and non-negative.

`RandShuffle` accepts the default thread-local generator or a borrowed `Rng` handle. The handle is not retained after the call, and a disposed generator fails before native use. Both continuous matrices and two-dimensional ROIs are supported when element size is at most 32 bytes. The destination is modified in place.

`SolveLp` accepts a 32- or 64-bit floating objective row/column vector and an `m` by `n+1` floating constraint matrix. The last constraint column is the right-hand side. Output is a caller-owned `CV_64FC1` column vector on successful solution, while `SolveLpResult` preserves OpenCV's lost, unbounded, infeasible, single, and multiple-solution statuses. Constraint epsilon is finite and non-negative; the default is `1e-12`.

## Runtime Diagnostics, Threads, And Optimization

`OpenCvSharpBuildInfo.GetNativeOpenCvVersion`, `GetNativeOpenCvVersionMajor`, `GetNativeOpenCvVersionMinor`, and `GetNativeOpenCvVersionRevision` read the existing stable version ABI. `Cv2.GetBuildInformation`, `Cv2.GetCpuFeaturesLine`, and `Cv2.GetHardwareFeatureName` return managed strings copied from an owned native UTF-8 result handle. Build configuration and CPU feature text are factual properties of the loaded runtime: their wording, length, and feature set are not portable assertions. Feature identifiers are validated in the inclusive OpenCV range `0..512`; an undefined but in-range identifier may yield an empty feature name.

`Cv2.SetNumThreads` and `Cv2.SetUseOptimized` mutate OpenCV process-global state. They must not be called concurrently with OpenCV work or from a parallel region. Passing `1` requests sequential OpenCV execution; a negative thread count asks OpenCV to reset to its backend default. Zero and positive counts retain backend-specific OpenCV semantics. `GetNumThreads` and `GetThreadNum` report backend-dependent observations, not a portable scheduling guarantee. State-changing code must save the observed value and restore it in `finally`; tests and the default sample follow that rule.

Disabling optimization makes OpenCV report no hardware capability through `CheckHardwareSupport` until optimization is re-enabled. `GetNumberOfCpus` reports CPUs available to the current process, which can reflect container or affinity limits. `GetCPUTickCount` is architecture-dependent, can be affected by migration and frequency changes, and must not be converted directly to elapsed time. `GetTickCount` with `GetTickFrequency` is the portable OpenCV interval-measurement pair; use ordering and unit relationships rather than wall-clock or exact-delay assertions.

`TickMeter` owns an opaque native timer through a `SafeHandle`; no C++ timer layout crosses the ABI. Call `Start` and `Stop` around an interval, then inspect total `Time*`, last-interval `LastTime*`, `Counter`, `FramesPerSecond`, and average values. `Stop` without an active start does not add a measurement. `Reset` clears every total, last value, and counter. The managed object is idempotently disposable and rejects use after disposal. The values are OpenCV tick-derived and are suitable for relationships and measurements, not for asserting a specific duration.

## Deferred Surfaces

Raw persistence (`writeRaw`, `readRaw`, raw format descriptors, and pointer-based byte ranges) remains unsupported because it requires a separate typed buffer contract with explicit element count, byte count, format, and stride validation. `FileNode.RawSize` exposes only OpenCV's size query; it does not expose raw memory.

Algorithm `read`, `write`, `load`, and `save` persistence remains deferred until each concrete algorithm family has a stable owned-handle and factory contract. The generic C++ template helpers and virtual dispatch surface are not treated as a safe C ABI. Multi-stream append edge cases and custom user-type registration are likewise outside this family.

IPP controls, sample-data search helpers, and allocator/OpenCL/UMat surfaces are not promoted by this batch. The map classifies each remaining parser identity as unsupported, conditional, or intentionally omitted with a source-reviewed reason. Those labels are scope decisions, not repository-wide parity claims.

The executable evidence is `tests/OpenCvSharp.Tests/Core/CoreUpstreamParityTests.cs`, `tests/OpenCvSharp.Tests/Core/CorePersistenceTests.cs`, `tests/OpenCvSharp.Tests/Core/CoreNumericalCollectionSolverTests.cs`, `tests/OpenCvSharp.Tests/Core/CoreRuntimeDiagnosticsTimingTests.cs`, `src/OpenCvSharp.Native/tests/native_smoke.cpp`, and the default `ConsoleSamples` Core workflow.
