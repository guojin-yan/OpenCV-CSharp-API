# Tracking Guide

`JYPPX.OpenCvSharp.Tracking` wraps the measured contrib tracking object family from the local OpenCV 5.0.0 tree. The runtime module is the factual OpenCV 5.0.0 runtime artifact `opencv_tracking500.dll`; it depends on main OpenCV modules staged as factual OpenCV 5.0.0 runtime artifacts, such as `opencv_video500.dll`, `opencv_imgproc500.dll`, and `opencv_core500.dll`.

`JYPPX.OpenCvSharp.Tracking` 封装本地 OpenCV 5.0.0 contrib tracking 的已测量目标跟踪对象族。runtime 模块是事实性 OpenCV 5.0.0 runtime 产物 `opencv_tracking500.dll`；它依赖作为事实性 OpenCV 5.0.0 runtime 产物暂存的 `opencv_video500.dll`、`opencv_imgproc500.dll` 和 `opencv_core500.dll` 等主线模块。

## Measured Contract

The official OpenCV parser emits 35 declarations across two deliberately separate public surfaces:

- primary `opencv2/tracking.hpp`: 10 declarations, 5 callables, all implemented
- public `opencv2/tracking/tracking_legacy.hpp`: 25 declarations, 16 callables, all implemented
- combined metadata: 1 enum and 13 classes
- missing, omitted, unsupported, and conditional callables: zero

The primary and legacy counts are never mixed. `opencv2/tracking.hpp` includes main `opencv2/video/tracking.hpp`, but main Video TrackerMIL and external-model tracker rows remain owned by the separate Video map and are not counted again. The eight installed feature, Kalman, online-boosting, dataset, matching, internals, older tracking, and twist headers remain explicitly outside these consumer tracker partitions.

The checked artifacts are `compatibility/tracking-upstream-raw.json`, `tracking-upstream-classifications.json`, `tracking-upstream-map.txt`, `tracking-upstream-summary.json`, and `tracking-implemented-families.json`. They do not claim repository-wide parity.

## Modern Trackers

Modern trackers use integer `Rect` boxes and the `JYPPX.OpenCvSharp.Tracking.Tracker` base class.

现代 tracker 使用整数 `Rect` 边界框，并以 `JYPPX.OpenCvSharp.Tracking.Tracker` 作为基类。

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Tracking;

using Mat first = new Mat(32, 32, MatType.CV_8UC3, new Scalar(0));
using Mat second = first.Clone();
using TrackerKCF tracker = TrackerKCF.Create(TrackerKCFParams.Default);

Rect box = new Rect(6, 7, 8, 8);
tracker.Init(first, box);
TrackerUpdateResult result = tracker.Update(second, box);
```

The first batch includes:

- `Tracker`: base `Init` and `Update`.
- `TrackerKCF`: default creation and flat `TrackerKCFParams`.
- `TrackerCSRT`: default creation, flat `TrackerCSRTParams`, and `SetInitialMask(Mat mask)`.
- `TrackerKCFMode`: `Gray`, `Cn`, and `Custom`.

KCF feature-extractor callbacks are not exposed in this batch because callback lifetime and ABI stability need a dedicated managed/native contract.

本批次暂不暴露 KCF feature extractor 回调，因为回调生命周期和 ABI 稳定性需要单独设计 managed/native 契约。

## OpenCV Legacy Trackers

OpenCV legacy trackers live under `JYPPX.OpenCvSharp.Tracking.Legacy` because OpenCV exposes them through `cv::legacy::Tracker`, which is a different native boundary from modern `cv::Tracker`.

OpenCV legacy tracker 位于 `JYPPX.OpenCvSharp.Tracking.Legacy`，因为 OpenCV 通过 `cv::legacy::Tracker` 暴露它们；这与 modern `cv::Tracker` 是不同 native 边界。

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Tracking.Legacy;

using TrackerMIL tracker = TrackerMIL.Create();
Rect2d box = new Rect2d(6, 7, 8, 8);
tracker.Init(first, box);
LegacyTrackerUpdateResult update = tracker.Update(second, box);
```

The closed public legacy surface includes `LegacyTracker`, `TrackerMOSSE`, `TrackerMIL`, `TrackerMedianFlow`, `TrackerBoosting`, `TrackerTLD`, legacy `TrackerKCF`, legacy `TrackerCSRT`, parameter values, and `MultiTracker`.

```csharp
using JYPPX.OpenCvSharp.Tracking.Legacy;

using TrackerKCF legacy = TrackerKCF.Create(JYPPX.OpenCvSharp.Tracking.TrackerKCFParams.Default);
using JYPPX.OpenCvSharp.Tracking.Tracker modern = legacy.Upgrade();

legacy.Dispose(); // the modern adapter retains the native cv::Ptr state
modern.Init(first, new Rect(6, 7, 8, 8));
TrackerUpdateResult result = modern.Update(second, new Rect(6, 7, 8, 8));
```

`TrackerBoostingParams` is a copied immutable value. Its OpenCV 5.0.0 defaults are 100 classifiers, overlap 0.99, search factor 1.8, 50 initialization iterations, and 1,050 features. Legacy KCF and CSRT reuse the already reviewed primary managed parameter values; native code copies them into the derived legacy parameter objects. The temporary CSRT window-function UTF-8 buffer is pinned only for the factory call and is never retained.

Legacy CSRT's initial mask is caller-owned. Set it before `Init`; the tracker consumes OpenCV's ref-counted Mat view according to the upstream implementation. A tracker instance is stateful and should not be updated concurrently. Repeated initialization returns the upstream legacy failure and is surfaced as a native exception by this managed API.

`Upgrade()` creates a new modern adapter owning its own native `cv::Ptr` reference to the legacy tracker. Disposing the original managed legacy wrapper does not invalidate the adapter. The adapter converts legacy `Rect2d` results to rounded integer `Rect` values and clips them to the current image bounds, matching the audited upstream wrapper.

`MultiTracker` returns arrays through stable count/fill native marshalling. No STL vector, `cv::Ptr`, `InputArray`, or `OutputArray` crosses the C ABI.

`MultiTracker` 通过稳定的 count/fill native 封送返回数组。STL vector、`cv::Ptr`、`InputArray` 和 `OutputArray` 都不会穿过 C ABI。

The non-parser KCF feature-extractor callback remains deliberately separate. It is not exposed because the upstream API retains a function pointer and therefore requires a complete callback lifetime, GCHandle, reentrancy, exception-capture, and thread-origin contract.

## Smoke Testing

Default tests do not require real videos, cameras, windows, downloads, or tracking images. Linked tracking smoke is guarded by:

默认测试不依赖真实视频、摄像头、窗口、下载文件或跟踪图片。linked tracking smoke 由以下环境变量保护：

```powershell
$env:OPENCV_CSHARP_NATIVE_SMOKE='1'
dotnet test .\tests\OpenCvSharp.Tests\OpenCvSharp.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~Tracking"
```



Tiny synthetic-frame smoke tests only prove the wrapper call path and output shape. They do not measure real tracking quality.

tiny 合成帧 smoke 只证明 wrapper 调用路径和输出形状，不代表真实跟踪质量。
