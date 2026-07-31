# Stitching Structured Parity Guide

The OpenCV 5.0.0 main Stitching contract is measured from all 14 installed public headers under `modules/stitching/include/opencv2`. The deterministic map contains 207 parser declarations and 158 callable rows. It partitions the high-level `Stitcher`, public warpers, and each `cv::detail` header instead of treating one included header as the whole module.

This is a module-scoped compatibility map. It does not claim repository-wide OpenCV parity.

## Measured coverage

The 24-row high-level `stitching.hpp` partition contains three metadata rows and 21 callable rows. All 21 callable rows map to the existing `OpenCvSharp.Stitching.Stitcher` lifecycle, properties, estimate/compose/stitch operations, components, copied cameras, and work scale.

The 53-row `detail/exposure_compensate.hpp` partition contains eight metadata rows and 45 callable rows. All 45 callables map to:

- `ExposureCompensator.CreateDefault`
- `NoExposureCompensator`
- `GainCompensator`
- `ChannelsCompensator`
- `BlocksCompensator`
- `BlocksGainCompensator`
- `BlocksChannelsCompensator`
- `Feed`, `Apply`, `GetMatGains`, `SetMatGains`, and exact property round trips

The 12-row public-warper partition contains two metadata rows and ten `PyRotationWarper` callable rows. All ten callables are implemented. The remaining 82 callable rows in other detail headers remain explicit `missing` rows. They are not described as implemented, omitted, or unsupported merely to reduce the gap count.

## Public rotation warper

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.Stitching;

using var camera = Mat.Eye(3, 3, MatType.CV_32FC1);
using var rotation = Mat.Eye(3, 3, MatType.CV_32FC1);
using var source = new Mat(4, 5, MatType.CV_8UC1, new Scalar(37));
using var projected = new Mat();
using var restored = new Mat();
using var warper = new PyRotationWarper("plane", 1.0f);

Point2f point = warper.WarpPoint(new Point2f(2, 3), camera, rotation);
Rect roi = warper.WarpRoi(new Size(source.Cols, source.Rows), camera, rotation);
Point topLeft = warper.Warp(source, camera, rotation, InterpolationFlags.Nearest, BorderTypes.Replicate, projected);
warper.WarpBackward(projected, camera, rotation, InterpolationFlags.Nearest, BorderTypes.Replicate,
    new Size(source.Cols, source.Rows), restored);
```

The exact accepted, case-sensitive OpenCV 5.0.0 names are `plane`, `affine`, `cylindrical`, `spherical`, `fisheye`, `stereographic`, `compressedPlaneA2B1`, `compressedPlaneA1.5B1`, `compressedPlanePortraitA2B1`, `compressedPlanePortraitA1.5B1`, `paniniA2B1`, `paniniA1.5B1`, `paniniPortraitA2B1`, `paniniPortraitA1.5B1`, `mercator`, and `transverseMercator`.

The constructor scale must be finite and positive and controls the retained projector. OpenCV 5.0.0's public `PyRotationWarper.getScale()` nevertheless always returns `1`, while `setScale()` is a no-op; the managed `Scale` property preserves that upstream behavior and validates only finite positive setter inputs. The parameterless constructor preserves the parser-emitted upstream state but has no projector. Its point, map, ROI, and image operations fail deterministically instead of dereferencing OpenCV's null internal pointer.

Camera and rotation matrices must be `3 x 3 CV_32FC1`. Non-contiguous ROI matrices are accepted. `BuildMaps` writes distinct caller-owned `CV_32FC1` x/y maps. For this upstream API, its returned rectangle uses the inclusive bottom-right coordinate while map dimensions include that endpoint, so map width and height are each one larger than the returned rectangle dimensions. `WarpRoi` returns the conventional full bounding rectangle.

Forward and backward image operations borrow the source, K, and R only for the call and write to caller-owned destination Mats. OpenCV may allocate or resize ordinary destination storage. Correctly sized ROI destinations retain their view; a mismatched fixed ROI fails through `NativeException`. In-place source/destination use is rejected because `remap` does not support it. Output depth and channels match the source. Interpolation and border modes pass through the existing strongly typed enums and unsupported combinations retain OpenCV's native error.

## Exposure workflow

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Stitching;

using var first = new Mat(64, 64, MatType.CV_8UC3, new Scalar(40, 40, 40));
using var second = new Mat(64, 64, MatType.CV_8UC3, new Scalar(80, 80, 80));
using var firstMask = new Mat(64, 64, MatType.CV_8UC1, new Scalar(255));
using var secondMask = new Mat(64, 64, MatType.CV_8UC1, new Scalar(255));
using var compensator = new GainCompensator(numberOfFeeds: 1);

var corners = new[] { new Point(0, 0), new Point(0, 0) };
compensator.Feed(corners, new[] { first, second }, new[] { firstMask, secondMask });
compensator.Apply(0, corners[0], first, firstMask);
compensator.Apply(1, corners[1], second, secondMask);

Mat[] gains = compensator.GetMatGains();
try
{
    System.Console.WriteLine("gain matrices=" + gains.Length);
}
finally
{
    foreach (Mat gain in gains) gain.Dispose();
}
```

`Feed` requires non-empty, equal-length corner, image, and mask collections. Every mask must be a non-empty `CV_8UC1` matrix with the same dimensions as its image. Inputs remain caller-owned.

## Ownership and mutation

The C ABI stores `cv::Ptr<cv::detail::ExposureCompensator>` behind an opaque owned handle. Managed disposal is idempotent, and no C++ object layout crosses the ABI.

`Feed` converts caller-owned Mat headers to temporary UMat views for the duration of the native call. The wrapper does not expose UMat execution as a managed capability and does not retain managed Mat handles. `Apply` mutates the caller-owned image in place and borrows its mask for that call.

`GetMatGains` uses count/fill. Every returned `Mat` is a cloned, independently owned handle and remains valid after the compensator is disposed. Partial allocation failures release all native handles already created. `SetMatGains` borrows the supplied matrices only for the call.

Exposure compensators are stateful and are not thread-safe. Serialize `Feed`, property changes, gain replacement, and `Apply` calls on each instance.

## Properties and validation

Gain, channel, and block compensators require `NumberOfFeeds > 0`. Similarity thresholds must be finite. Block width and height must be positive, and gain filtering iterations must be non-negative. Default block size is 32 by 32, default feed count is one, default similarity threshold is one, and default filtering iteration count is two.

`UpdateGain` controls whether a later feed updates the estimated gains. The no-op compensator supports the common feed/apply/gain contract but has no feed-count, similarity, or block properties.

## High-level state and results

`Stitcher.EstimateTransform` establishes camera state for a later `ComposePanorama`. `Stitcher.Stitch` performs both phases. Each method returns the exact `StitcherStatus`; callers must handle need-more-images, homography-estimation failure, and camera-adjustment failure without assuming a panorama exists.

Component indices are copied values. Camera results own independent rotation and translation matrices, which callers dispose. Result masks copy into caller-owned or newly allocated managed matrices.

## Profile boundaries

Stitching entrypoints are full-profile only. A mini-linked or unlinked wrapper keeps the ABI shape but returns `NOT_LINKED`; that is not evidence that `opencv_stitching` is present. CUDA/OpenCL-specific input surfaces, retained matcher/estimator/seam/blender strategies, callback shapes, templates, mutable nested detail containers, and internal detail warpers remain outside the completed families.

Parser-emitted rows and source-reviewed extensions remain separate. `Stitcher.WaveCorrectKind`, `Stitcher.GetResultMask`, and the explicit managed no-op constructor are source-reviewed adaptations and do not alter the 207/158 parser counts.
