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

The remaining 92 callable rows in public warpers and other detail headers remain explicit `missing` rows. They are not described as implemented, omitted, or unsupported merely to reduce the gap count.

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

Stitching entrypoints are full-profile only. A mini-linked or unlinked wrapper keeps the ABI shape but returns `NOT_LINKED`; that is not evidence that `opencv_stitching` is present. CUDA/OpenCL-specific paths, retained matcher/estimator/seam/blender strategies, callback shapes, templates, mutable nested detail containers, and public warpers remain outside this completed family.

Parser-emitted rows and source-reviewed extensions remain separate. `Stitcher.WaveCorrectKind`, `Stitcher.GetResultMask`, and the explicit managed no-op constructor are source-reviewed adaptations and do not alter the 207/158 parser counts.
