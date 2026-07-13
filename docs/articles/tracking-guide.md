# Tracking Guide

`OpenCvSharp.Tracking` wraps the first contrib tracking object family from the local OpenCV 5.0.0 tree. The runtime module is the factual OpenCV 5.0.0 runtime artifact `opencv_tracking500.dll`; it depends on main OpenCV modules staged as factual OpenCV 5.0.0 runtime artifacts, such as `opencv_video500.dll`, `opencv_imgproc500.dll`, and `opencv_core500.dll`.

`OpenCvSharp.Tracking` 封装本地 OpenCV 5.0.0 contrib tracking 的第一批目标跟踪对象族。runtime 模块是事实性 OpenCV 5.0.0 runtime 产物 `opencv_tracking500.dll`；它依赖作为事实性 OpenCV 5.0.0 runtime 产物暂存的 `opencv_video500.dll`、`opencv_imgproc500.dll` 和 `opencv_core500.dll` 等主线模块。

## Modern Trackers

Modern trackers use integer `Rect` boxes and the `OpenCvSharp.Tracking.Tracker` base class.

现代 tracker 使用整数 `Rect` 边界框，并以 `OpenCvSharp.Tracking.Tracker` 作为基类。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Tracking;

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

OpenCV legacy trackers live under `OpenCvSharp.Tracking.Legacy` because OpenCV exposes them through `cv::legacy::Tracker`, which is a different native boundary from modern `cv::Tracker`.

OpenCV legacy tracker 位于 `OpenCvSharp.Tracking.Legacy`，因为 OpenCV 通过 `cv::legacy::Tracker` 暴露它们；这与 modern `cv::Tracker` 是不同 native 边界。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Tracking.Legacy;

using TrackerMIL tracker = TrackerMIL.Create();
Rect2d box = new Rect2d(6, 7, 8, 8);
tracker.Init(first, box);
LegacyTrackerUpdateResult update = tracker.Update(second, box);
```

The first batch includes `LegacyTracker`, `TrackerMOSSE`, `TrackerMIL`, `TrackerMedianFlow`, `TrackerMILParams`, `TrackerMedianFlowParams`, and `MultiTracker`.

`MultiTracker` returns arrays through stable count/fill native marshalling. No STL vector, `cv::Ptr`, `InputArray`, or `OutputArray` crosses the C ABI.

`MultiTracker` 通过稳定的 count/fill native 封送返回数组。STL vector、`cv::Ptr`、`InputArray` 和 `OutputArray` 都不会穿过 C ABI。

## Smoke Testing

Default tests do not require real videos, cameras, windows, downloads, or tracking images. Linked tracking smoke is guarded by:

默认测试不依赖真实视频、摄像头、窗口、下载文件或跟踪图片。linked tracking smoke 由以下环境变量保护：

```powershell
$env:OPENCV_CSHARP_NATIVE_SMOKE='1'
dotnet test .\tests\OpenCvSharp.Tests\OpenCvSharp.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~Tracking"
```

The older `OPENCV5SHARP_NATIVE_SMOKE=1` name remains accepted only as an existing-smoke-workflow compatibility alias.

旧的 `OPENCV5SHARP_NATIVE_SMOKE=1` 名称仍仅作为既有 smoke workflow 的兼容别名使用。

Tiny synthetic-frame smoke tests only prove the wrapper call path and output shape. They do not measure real tracking quality.

tiny 合成帧 smoke 只证明 wrapper 调用路径和输出形状，不代表真实跟踪质量。
