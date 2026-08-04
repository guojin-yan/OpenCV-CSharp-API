# ImgProc Upstream Coverage And Workflow Guide

This guide covers the structured OpenCV `5.0.0` declaration slice extracted from `opencv2/imgproc.hpp` and the practical workflow families completed from that evidence. It is not a repository-wide OpenCV C++ parity claim.

## Measured Header Slice

The checked extraction uses OpenCV's own `modules/python/src2/hdr_parser.py`. The regular repository guard does not require Python: it validates the checked extraction, exact header and parser SHA256 values, reviewed classifications, native ABI manifest, managed API baseline, generated map, and ten negative fixtures with the exact .NET SDK.

The current slice contains 203 declarations:

- 29 enum declarations retained as normalized metadata
- 7 class declarations retained as normalized metadata
- 167 callable declarations
- 161 implemented callable declarations
- 0 missing callable declarations
- 6 intentionally omitted declaration identities

The source-order map is `compatibility/imgproc-upstream-map.txt`; JSON provenance and classifications are in `imgproc-upstream-raw.json`, `imgproc-upstream-classifications.json`, and `imgproc-upstream-summary.json`. `imgproc-implemented-families.json` correlates selected enum, class, and callable declarations with native entrypoints, managed members, tests, sample, and this guide.

Regenerate the raw parser extraction only when the OpenCV source or parser changes:

```powershell
pwsh -NoProfile -File ./scripts/Generate-ImgProcUpstreamMap.ps1 `
  -RegenerateRaw `
  -PythonPath C:\path\to\python.exe `
  -InitializeClassification
```

Normal freshness checks require no Python:

```powershell
pwsh -NoProfile -File ./scripts/Test-ImgProcUpstreamMap.ps1
```

## Generalized Hough

`GeneralizedHoughBallard` detects translation. `GeneralizedHoughGuil` also searches rotation and scale. Both own a native `cv::Ptr` through a `SafeHandle`; dispose them deterministically.

```csharp
using Mat template = new Mat(24, 24, MatType.CV_8UC1);
using Mat image = new Mat(96, 96, MatType.CV_8UC1);
using Mat positions = new Mat();
using GeneralizedHoughBallard detector = Cv2.CreateGeneralizedHoughBallard();

template.SetTo(new Scalar(0));
image.SetTo(new Scalar(0));
Cv2.Rectangle(template, new Rect(4, 4, 16, 16), new Scalar(255), 1);
Cv2.Rectangle(image, new Rect(40, 32, 16, 16), new Scalar(255), 1);

detector.CannyLowThreshold = 25;
detector.CannyHighThreshold = 75;
detector.Levels = 180;
detector.VotesThreshold = 1;
detector.SetTemplate(template);
detector.Detect(image, positions);
```

Use the edge overloads when edges and `CV_32F` derivatives are already part of the pipeline. A detector must receive a template before detection. Access after disposal throws `ObjectDisposedException`; invalid managed ranges fail before native invocation.

## Color And Visualization

`CvtColorTwoPlane` consumes a full-resolution `CV_8UC1` Y plane and a half-resolution `CV_8UC2` interleaved UV plane for NV12/NV21 codes. `Demosaicing` accepts the Bayer conversion codes exposed by `ColorConversionCodes`. `ApplyColorMap` accepts either `ColormapTypes` or a 256-entry `CV_8UC1`/`CV_8UC3` user map.

```csharp
using Mat y = new Mat(480, 640, MatType.CV_8UC1);
using Mat uv = new Mat(240, 320, MatType.CV_8UC2);
using Mat bgr = Cv2.CvtColorTwoPlane(y, uv, ColorConversionCodes.YUV2BGR_NV12);
using Mat heat = Cv2.ApplyColorMap(y, ColormapTypes.Turbo);
```

The OpenCV 5 declaration includes an explicit `AlgorithmHint` parameter for `CvtColorTwoPlane`. The stable C ABI intentionally uses OpenCV's default hint; the map therefore records that exact declaration identity as intentionally omitted while retaining evidence for the default behavior.

## Preparation And Overlays

Use `BlendLinear` with single-channel `CV_32F` weight matrices, `StackBlur` with positive odd kernel dimensions, and `SpatialGradient` with OpenCV's supported `ksize=3`. `ThresholdWithMask` changes only pixels selected by a nonzero mask, so initialize the destination when unselected pixels must retain a known value.

```csharp
using Mat dx = new Mat();
using Mat dy = new Mat();
Cv2.SpatialGradient(gray, dx, dy);

double used = Cv2.ThresholdWithMask(
    gray, destination, mask, 96, 255, ThresholdTypes.Binary);

Cv2.DrawMarker(destination, new Point(20, 20), new Scalar(255), MarkerTypes.Star);
Cv2.FillConvexPoly(
    destination,
    new[] { new Point(4, 28), new Point(20, 4), new Point(36, 28) },
    new Scalar(192));
double fontScale = Cv2.GetFontScaleFromHeight(HersheyFonts.HersheySimplex, 24);
```

`FillConvexPoly(ReadOnlySpan<Point>)` is available on modern targets and passes the proven sequential point layout directly to the native ABI. The array overload remains available on every supported framework.

## Calibration, Sampling, And Coordinates

Camera-oriented methods live on `JYPPX.OpenCvSharp.Calib3D.Cv2`, while their neutral ABI entrypoints remain in the ImgProc wrapper because OpenCV 5 declares these operations in `opencv2/imgproc.hpp`:

```csharp
using Mat undistorted = Calib3DCv2.Undistort(source, cameraMatrix, distortion);
using Mat fisheye = Calib3DCv2.FisheyeUndistortImage(
    source, cameraMatrix, fisheyeDistortion, newCameraMatrix, source.Size());
using Mat patch = ImgProcCv2.GetRectSubPix(
    undistorted, new Size(31, 31), new Point2f(120.5F, 80.25F));
using Mat polar = ImgProcCv2.WarpPolar(
    undistorted, source.Size(), new Point2f(120, 80), 100,
    InterpolationFlags.Linear, WarpPolarMode.Linear);
```

`InitInverseRectificationMap` returns an `UndistortRectifyMapResult`; dispose both owned maps. `DrawFrameAxes` mutates its image. Positive image sizes and axis length are checked in managed code, while OpenCV validates the numeric matrix shapes and element types.

## Accumulation, Registration, And Matching

The four accumulation methods mutate the caller-owned floating-point accumulator and accept an optional mask. Phase correlation accepts an optional Hanning window and exposes the optional response through an `out` parameter; the iterative variant exposes OpenCV 5's neighborhood and iteration controls.

```csharp
using Mat window = ImgProcCv2.CreateHanningWindow(image.Size(), MatType.CV_32F);
ImgProcCv2.AccumulateWeighted(frame, accumulator, 0.1, mask);
Point2d shift = ImgProcCv2.PhaseCorrelate(previous, current, window, out double response);
Point2d refined = ImgProcCv2.PhaseCorrelateIterative(previous, current);
using Mat match = ImgProcCv2.MatchTemplate(image, template, TemplateMatchModes.CCoeffNormed);
```

`EMD` treats both signatures and an optional user-distance cost matrix as borrowed inputs. An optional caller-owned `flow` Mat receives the transport matrix. The overload with `ref float lowerBound` passes a real optional native lower-bound pointer and writes the updated bound back; the simpler overload passes no lower-bound pointer. The public method is named `EMD`, while the parser-only upstream wrapper name remains mapping evidence rather than leaking into the managed API.

## Segmentation And Link-Runs Contours

`Watershed` mutates `CV_32SC1` markers. `GrabCut` mutates its mask and both model Mats; empty model Mats are allocated by OpenCV. OpenCV 5 defines `GrabCutModes` as `InitWithRect=0`, `InitWithMask=1`, `Eval=2`, and `EvalFreezeModel=3`, which differs from older enum orderings.

```csharp
using Mat filtered = ImgProcCv2.PyrMeanShiftFiltering(color, 8, 24);
ImgProcCv2.GrabCut(
    color, mask, region, backgroundModel, foregroundModel,
    3, GrabCutModes.InitWithRect);
ImgProcCv2.Watershed(color, markers);
ImgProcCv2.FindContoursLinkRuns(binary, out Point[][] contours, out Vec4i[] hierarchy);
```

The C ABI never exposes `std::vector`. Link-runs contour transfer uses deterministic count/fill calls: the first call reports contour, point, and hierarchy counts; managed code allocates flat buffers; the second call fills them and must report identical counts before reconstruction. The hierarchy-free overload uses the same path without requesting hierarchy.

## OpenCV 5 FontFace

`FontFace` owns a native `cv::FontFace` through a safe handle. Construct it with a built-in name such as `"sans"` or a supported font path, and dispose it deterministically. Names and rendered text cross the ABI as null-terminated UTF-8. Access after disposal throws `ObjectDisposedException` before native invocation.

```csharp
using FontFace font = new FontFace("sans");
font.SetInstance(Array.Empty<int>());
Point next = ImgProcCv2.PutText(
    canvas, "OpenCV", new Point(8, 32), new Scalar(255, 255, 255), font, 24);
Rect bounds = ImgProcCv2.GetTextSize(
    canvas.Size(), "OpenCV", new Point(8, 32), font, 24);
```

Variable-font instances are transferred as tag/value integer pairs and retrieved through a separate count/fill exchange. `PutText` returns the continuation point; `GetTextSize` returns the bounding rectangle. `TextWrapRange` models an optional OpenCV range without a borrowed pointer, and `PutTextFlags` models alignment/origin flags. These custom-font overloads are distinct from the Hershey-font `PutText` and `GetTextSize` APIs.

## Final Boundary

All 26 callable rows that began this stage as missing are supported by the locally linked OpenCV 5.0.0 build and are now implemented. The measured callable partition is therefore 161 implemented, zero missing, and six intentionally omitted identities. Both full and mini native profiles include the new wrapper entrypoints because `imgproc.cpp` belongs to both profiles. This closes only the exact parser-derived `opencv2/imgproc.hpp` slice described above; it does not imply repository-wide C++ API parity.
