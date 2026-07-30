# Photo Intelligent Scissors Guide

`OpenCvSharp.Photo.IntelligentScissorsMB` exposes OpenCV 5.0.0's live-wire image segmentation object. One instance owns one native model through a SafeHandle and follows an explicit configure, apply, build, and retrieve lifecycle.

## Basic Workflow

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Photo;

using var image = new Mat(64, 64, MatType.CV_8UC1, new Scalar(0));
using var scissors = new IntelligentScissorsMB();

scissors.SetEdgeFeatureCannyParameters(20.0, 60.0);
scissors.ApplyImage(image);
scissors.BuildMap(new Point(8, 8));

using Mat contour = scissors.GetContour(new Point(55, 8));
Console.WriteLine($"{contour.Rows} points, type={contour.Type}");
```

`ApplyImage` accepts a non-empty two-dimensional `CV_8UC1`, `CV_8UC3`, or `CV_8UC4` Mat. The OpenCV public comment lists one- and three-channel input, while the audited 5.0.0 implementation explicitly accepts four channels as well. ROI and other non-contiguous inputs are supported.

## State And Configuration

The default weights are 0.43 for non-edge cost, 0.43 for gradient direction, and 0.14 for gradient magnitude. `SetWeights` accepts finite non-negative values whose sum is greater than `FLT_EPSILON`; normalization to one is upstream guidance, not an enforced rule.

`SetGradientMagnitudeMaxLimit(0)` disables gradient-magnitude thresholding. `SetEdgeFeatureZeroCrossingParameters` selects the default zero-crossing extractor. `SetEdgeFeatureCannyParameters` selects Canny and accepts aperture `-1`, `3`, `5`, or `7`.

Every configuration setter clears previously applied features and the optimal-path map, matching OpenCV. Call `ApplyImage` or `ApplyImageFeatures` again before `BuildMap`. Each apply call also invalidates an earlier map. `GetContour` requires a successful `BuildMap`.

Source and target points must lie inside the applied image. The wrapper checks both lower and upper bounds before calling OpenCV because the audited implementation does not safely reject every negative coordinate in release builds.

One instance is stateful and not thread-safe. Use separate instances for concurrent calculations.

## Custom Features

`ApplyImageFeatures` accepts nullable or empty Mats:

| Feature | Mat contract | Expected values |
| --- | --- | --- |
| Non-edge | `CV_8UC1` | 0 or 1 |
| Gradient direction | `CV_32FC2` | normalized x/y direction; zero is valid where the gradient is zero |
| Gradient magnitude cost | `CV_32FC1` | `[0,1]` |
| Optional image | `CV_8UC1`, `CV_8UC3`, or `CV_8UC4` | source pixels |

All non-empty inputs must have identical rows and columns. A missing feature with zero weight is replaced by zeros. A missing feature with non-zero weight is derived from the optional image, so the image is required in that case.

OpenCV 5.0.0 enforces feature type and size but treats the documented value ranges as caller preconditions. The binding intentionally does not impose a stricter scan that would reject source-accepted data. Supplying out-of-contract values can produce meaningless costs.

Provided feature Mats are stored by OpenCV as ref-counted Mat headers. Disposing the managed Mat wrappers after `ApplyImageFeatures` is safe because native references keep the allocation alive. Mutating another Mat that shares the same feature storage before `BuildMap` changes the calculation. The optional image is used only to derive missing features and is not retained after the call. `ApplyImage` derives all features during the call and does not retain the source image.

## Contour Ownership And Order

Both `GetContour(Point, Mat, bool)` and the allocating `GetContour(Point, bool)` overload produce a caller-owned `N x 1 CV_32SC2` Mat. The native boundary normalizes OpenCV's direction-dependent vector layout to this stable shape without changing point values.

With `backward=false`, points run from the map source to the requested target. With `backward=true`, they run from target to source. Repeated retrieval is supported and each allocating call returns independent Mat storage. Repeated `BuildMap` calls replace the map and may use a different source point.

## Profile And Lifetime

The API is backed by the full native profile and `opencv_photo500.dll`. The mini profile does not include Photo wrapper sources or these entrypoints. Public managed shape remains available for compile-time compatibility, but execution requires a matching full runtime.

Dispose the model and every returned Mat. Repeated model disposal is harmless; operations after disposal throw `ObjectDisposedException`. Native exceptions remain inside the C ABI and are translated through `NativeException`.
