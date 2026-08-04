# Calib3D Upstream Coverage And Workflow Guide

This guide describes the checked OpenCV `5.0.0` declaration closure reached through `opencv2/calib3d.hpp`. It is a measured compatibility slice, not a repository-wide claim that every OpenCV C++ declaration is bound.

## Measured Include Closure

OpenCV 5 keeps `opencv2/calib3d.hpp` as a compatibility include. The checked extraction uses OpenCV's own `modules/python/src2/hdr_parser.py` and records four source headers:

- `opencv2/geometry/2d.hpp`: 53 declarations
- `opencv2/geometry/3d.hpp`: 67 declarations
- `opencv2/stereo.hpp`: 55 declarations
- `opencv2/calib.hpp`: 19 declarations

The resulting 194 identities contain 22 enums, 5 classes, and 167 callables. All 167 callable identities have stable native C ABI and public managed evidence; none are missing, unsupported, conditional, or intentionally omitted.

The source-order evidence is checked into `compatibility/calib3d-upstream-map.txt`. Raw parser output, reviewed classifications, summary, and family inventory live beside it. Normal validation does not require Python:

```powershell
pwsh -NoProfile -File ./scripts/Test-Calib3DUpstreamMap.ps1 `
  -DotNetPath C:\Users\guoji\.dotnet\dotnet.exe
```

Only regenerate raw extraction when the pinned OpenCV source or parser changes.

## Subdiv2D Ownership And Output

`JYPPX.OpenCvSharp.ImgProc.Subdiv2D` owns a native `cv::Subdiv2D` through a `SafeHandle`. Dispose it deterministically. Integer and single-precision rectangles are supported, and `InitDelaunay` can reuse an existing object.

```csharp
using var subdiv = new Subdiv2D(new Rect2f(0, 0, 640, 480));
subdiv.Insert(new[]
{
    new Point2f(100, 100),
    new Point2f(500, 100),
    new Point2f(500, 380),
    new Point2f(100, 380),
    new Point2f(320, 240)
});

Vec4f[] edges = subdiv.GetEdgeList();
Vec6f[] triangles = subdiv.GetTriangleList();
Subdiv2DPointLocation location = subdiv.Locate(
    new Point2f(320, 240), out int edge, out int vertex);
subdiv.GetVoronoiFacetList(
    new[] { vertex }, out Point2f[][] facets, out Point2f[] centers);
```

Native edge, triangle, leading-edge, and Voronoi results use count/fill protocols. Managed callers receive owned arrays; no `std::vector` layout crosses the ABI. `Subdiv2DEdgeNavigation` preserves OpenCV's quad-edge navigation constants.

## USAC Parameter Records

`UsacParams` reads OpenCV 5's native defaults and marshals a version-neutral POD record. C++ object layout never crosses the C boundary. The parameter object is supported by homography, PnP RANSAC, fundamental-matrix, two-camera essential-matrix, and full affine estimation overloads.

```csharp
var parameters = new UsacParams
{
    Confidence = 0.999,
    Threshold = 1.0,
    MaxIterations = 10000,
    SamplingMethod = UsacSamplingMethod.Prosac,
    ScoreMethod = UsacScoreMethod.Magsac,
    RandomGeneratorState = 42
};

using var mask = new Mat();
using Mat homography = Calib3DCv2.FindHomography(
    sourcePoints, destinationPoints, mask, parameters);
```

Confidence, threshold, iteration counts, and enum domains are validated before native execution. Keep `RandomGeneratorState` fixed when deterministic regression evidence matters.

## Fisheye Stereo Rectification

Fisheye rectification is not interchangeable with pinhole `StereoRectify`. Use the model-specific entrypoint and dispose every owned output in `FisheyeStereoRectifyResult`.

```csharp
FisheyeStereoRectifyResult result = Calib3DCv2.FisheyeStereoRectify(
    cameraMatrix1, distCoeffs1,
    cameraMatrix2, distCoeffs2,
    imageSize, rotation, translation);

using (result.R1)
using (result.R2)
using (result.P1)
using (result.P2)
using (result.Q)
{
    // Build fisheye undistort/rectify maps from R1/P1 and R2/P2.
}
```

The focused runtime contract is `tests/OpenCvSharp.Tests/Calib3D/Calib3DUpstreamParityTests.cs`; the native count/fill and USAC paths are also exercised by `src/OpenCvSharp.Native/tests/native_smoke.cpp`. `samples/ConsoleSamples/Program.cs` runs a compact default workflow for all three newly completed families.
