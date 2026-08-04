# Video Upstream Coverage

The main Video contract is measured from the exact OpenCV `5.0.0` compatibility include `opencv2/video.hpp`. The official `hdr_parser.py` output contributes 168 declarations from `tracking.hpp` and `background_segm.hpp`: 145 callables, 20 classes or structs, and 3 enums. The checked map classifies 138 callables as implemented, none as missing, and 7 as intentionally omitted. This is a main Video module result, not a repository-wide OpenCV parity claim.

The three selected families account for 83 parser rows, 13 public types, 110 public/protected members, and 45 primary native entrypoints. They cover the owned optical-flow objects, ECC registration, and the model-free main Video `TrackerMIL` lifecycle.

## Dense Flow

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Video;

using Mat first = LoadFirstGrayFrame();
using Mat second = LoadSecondGrayFrame();
using FarnebackOpticalFlow algorithm = FarnebackOpticalFlow.Create(
    numLevels: 3,
    winSize: 11,
    numIterations: 4);

algorithm.PolyN = 5;
algorithm.PolySigma = 1.2;
algorithm.Flags = OpticalFlowFlags.FarnebackGaussian;
using Mat flow = algorithm.Calc(first, second);
```

`Calc(first, second)` allocates an independently owned, zero-initialized `first.Rows x first.Cols` `CV_32FC2` matrix before invoking OpenCV. The initialized matrix is required by `VariationalRefinement` and is also a valid zero estimate when Farneback uses `UseInitialFlow`. Use `Calc(first, second, flow)` when an existing flow estimate must be refined in place.

`VariationalRefinement.CalcUV` accepts two caller-owned `CV_32FC1` matrices for horizontal and vertical flow. The packed `Calc` overload uses one `CV_32FC2` matrix. `CollectGarbage` releases algorithm caches without disposing the object.

## Sparse LK

```csharp
Point2f[] previousPoints =
{
    new Point2f(10, 10),
    new Point2f(15, 15),
    new Point2f(20, 20)
};

using SparsePyrLkOpticalFlow algorithm = SparsePyrLkOpticalFlow.Create(
    winSize: new Size(11, 11),
    maxLevel: 1,
    flags: OpticalFlowFlags.UseInitialFlow);

Point2f[] nextPoints = algorithm.Calc(
    first,
    second,
    previousPoints,
    previousPoints,
    out byte[] status,
    out float[] error);
```

The returned point array and the status/error arrays are independently managed. Initial and previous point counts must match. When `UseInitialFlow` is enabled, the overload without `initialNextPoints` throws `InvalidOperationException` before native execution so a missing estimate cannot silently become zero coordinates.

## ECC Registration

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Video;
using VideoCv2 = JYPPX.OpenCvSharp.Video.Cv2;

double correlation = VideoCv2.ComputeECC(reference, sample, inputMask);

using ECCRegistrationResult result = VideoCv2.FindTransformECC(
    reference,
    sample,
    MotionType.Affine,
    TermCriteria.ByCountAndEpsilon(50, 1e-6),
    inputMask,
    gaussianFilterSize: 5);
```

Single-scale images must be non-empty two-dimensional Mats with matching types and one or three channels. `ComputeECC` additionally requires matching sizes. Supported depths are `CV_8U`, `CV_16U`, `CV_32F`, and `CV_64F`. A mask is empty or a matching `CV_8UC1` Mat. Caller-supplied single-scale warps are empty or `CV_32FC1`; OpenCV initializes an empty warp to identity. Translation, Euclidean, and affine models use a `2x3` warp, while homography uses `3x3`.

The allocating overload returns `ECCRegistrationResult`, which independently owns its warp Mat and releases it on `Dispose`. Caller-warp overloads update the supplied Mat and do not transfer ownership. Dual-mask registration validates each mask against its corresponding image.

`FindTransformECCMultiScale` uses immutable `ECCParameters`. Images are single-channel; warps may be empty, `CV_32FC1`, or `CV_64FC1`. An explicit iteration schedule is empty or contains exactly one non-negative count per pyramid level. The multiscale implementation accepts nearest or linear interpolation, and a Gaussian size of zero disables smoothing. The OpenCV 5.0.0 defaults are affine motion, count-or-epsilon `50/1e-6`, Gaussian size 5, four levels, and linear interpolation.

## Main Video TrackerMIL

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Video;

using TrackerMIL tracker = TrackerMIL.Create(TrackerMILParams.Default);
var box = new Rect(20, 22, 20, 20);
tracker.Init(firstFrame, box);
bool found = tracker.Update(nextFrame, ref box);
```

This `JYPPX.OpenCvSharp.Video.TrackerMIL` is separate from contrib and legacy Tracking wrappers. It owns an opaque `cv::Ptr<cv::Tracker>` through a safe handle, and the native factory copies the complete parameter value. `Init` requires a non-empty two-dimensional frame and a positive rectangle fully contained in that frame. `Update` before successful initialization throws without entering OpenCV, avoiding the upstream null-state dereference.

The update rectangle is written only when OpenCV reports success. A false result leaves the caller value unchanged. `TrackingScore` exposes the base implementation; OpenCV 5.0.0 reports `-1` for TrackerMIL. Disposal is idempotent, clears managed initialization state, and makes subsequent operations throw `ObjectDisposedException`.

## Ownership And Errors

Each algorithm or tracker owns one opaque native handle and is idempotently disposable. Input and caller-output `Mat` objects remain caller-owned; allocating ECC overloads document their independent result ownership. Native OpenCV exceptions cross the existing exception bridge. Null references, invalid matrix shapes or types, mismatched point counts, unsupported flags, non-finite scalar properties, invalid tracker rectangles, disposed objects, and missing initialization states receive deterministic managed exceptions where the public contract can reject them before native execution.

The object APIs are full-profile surfaces backed by `opencv_video`. They are excluded from mini builds; full-runtime managed tests must use a full linked runtime, while mini CTest verifies that the profile boundary remains intact.

## Intentional Omissions

There are no missing main Video callable rows. The seven intentional omissions are the empty Kalman constructor plus model-backed DaSiamRPN, Nano, and Vit constructors or factories that require external DNN assets. Their exact ordinals and reasons remain in `compatibility/video-upstream-classifications.json`.

Run the deterministic checks with the repository SDK and PowerShell toolchain:

```powershell
pwsh -NoProfile -File ./scripts/Test-VideoUpstreamMap.ps1
dotnet test ./tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj -c Release -f net8.0 --filter FullyQualifiedName~VideoOpticalFlowObjectTests
dotnet test ./tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj -c Release -f net10.0 --filter FullyQualifiedName~VideoOpticalFlowObjectTests
dotnet test ./tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj -c Release -f net8.0 --filter FullyQualifiedName~VideoEccTrackerMilTests
dotnet test ./tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj -c Release -f net10.0 --filter FullyQualifiedName~VideoEccTrackerMilTests
```

The executable counterpart is `samples/ConsoleSamples/Program.cs`; native lifetime, property, calculation, invalid-input, and wrong-concrete-type paths are covered by `src/OpenCvSharp.Native/tests/native_smoke.cpp`.
