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

The 12-row public-warper partition contains two metadata rows and ten `PyRotationWarper` callable rows. All ten callables are implemented. The 28-row `detail/blenders.hpp` partition contains four metadata rows and 24 callable rows. All 24 callables are implemented.

The 23-row `detail/matchers.hpp` partition contains seven metadata rows and 16 callable rows. Fourteen callables are implemented through `ImageFeatures`, `MatchesInfo`, `FeaturesMatcher`, `BestOf2NearestMatcher`, `BestOf2NearestRangeMatcher`, and `AffineBestOf2NearestMatcher`. The two LightGlue callables are explicitly unsupported because they require an externally supplied ONNX model and an owned `cv::LightGlueMatcher` API that this repository does not expose.

The two autocalibration callables, `CameraParams.K`, and all 17 motion-estimator callables are implemented through `StitcherCameraParams`, `Estimator`, the homography and affine estimators, five bundle adjusters, and `StitchingMotion`. All nine seam-finder, four timelapser, seven detail-utility, and two spherical-projector callables are also implemented. The measured module result is 156 implemented, zero missing, and two unsupported callable rows.

## Camera and motion estimation

```csharp
using System.Linq;
using OpenCvSharp.Core;
using OpenCvSharp.Stitching;

using var adjuster = new NoBundleAdjuster();
adjuster.ConfidenceThreshold = 0.0;
adjuster.TerminationCriteria = new TermCriteria(TermCriteriaTypes.Count, 2, 0.0);

bool succeeded = adjuster.Apply(features, pairwiseMatches, initialCameras, out StitcherCameraParams[] cameras);
try
{
    using Mat intrinsic = cameras[0].GetCameraMatrix();
    StitchingMotion.WaveCorrect(cameras.Select(camera => camera.Rotation).ToArray(), WaveCorrectKind.Horizontal);
    string graph = StitchingMotion.MatchesGraphAsString(paths, pairwiseMatches, 0.0f);
}
finally
{
    foreach (StitcherCameraParams camera in cameras)
    {
        camera.Rotation.Dispose();
        camera.Translation.Dispose();
    }
}
```

Estimator input features and matches use the same exact `N * N` row-major contract as batch matching. `HomographyBasedEstimator(false)` and `AffineBasedEstimator` allocate their initial camera vector upstream. `HomographyBasedEstimator(true)` and every bundle adjuster require exactly `N` initial cameras. Inputs are borrowed and never mutated; each returned camera owns new `R` and `t` Mats that the caller must dispose. `R` is a single-channel `3 x 3 CV_32F` or `CV_64F` matrix, and `t` is a matching-depth single-channel `3 x 1` matrix. The affine bundle adjusters require upstream-compatible `CV_32F` rotations, so convert custom `CV_64F` initial rotations explicitly before calling them.

`StitcherCameraParams.GetCameraMatrix` returns or copies a `3 x 3 CV_64FC1` intrinsic matrix. `FocalsFromHomography` accepts exactly `3 x 3 CV_64FC1`; its upstream formula assumes the zero-principal-point rotating-camera model. `CalibrateRotatingCamera` only replaces caller-owned output when upstream reports success. The convenience `TryCalibrateRotatingCamera` returns independently owned output storage.

`WaveCorrect` accepts distinct `3 x 3 CV_32FC1` rotation objects. The native boundary clones every matrix, invokes OpenCV, and commits all results only after the whole operation succeeds. `MatchesGraphAsString` preserves UTF-8 path bytes and rejects embedded nulls. `LeaveBiggestComponent` does not mutate its inputs; its selected feature array and selected `M * M` row-major match array contain independent owned handles and must both be disposed.

## Detail feature matching

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;
using OpenCvSharp.Stitching;

using var first = new Mat(96, 96, MatType.CV_8UC1, new Scalar(0));
using var second = new Mat(96, 96, MatType.CV_8UC1, new Scalar(0));
using var orb = ORB.Create(maxFeatures: 200);
using var matcher = new BestOf2NearestMatcher(matchConfidence: 0.8f);

ImageFeatures[] features = ImageFeatures.Compute(orb, new[] { first, second });
MatchesInfo[] pairwise = matcher.Match(features);
try
{
    MatchesInfo forward = pairwise[0 * features.Length + 1];
    using Mat homography = forward.GetHomography();
    System.Console.WriteLine("matches=" + forward.Matches.Length + ", inliers=" + forward.NumberOfInliers);
}
finally
{
    foreach (MatchesInfo item in pairwise) item.Dispose();
    foreach (ImageFeatures item in features) item.Dispose();
}
```

`ImageFeatures.Compute` accepts the repository's built-in ORB, SIFT, FastFeatureDetector, GFTTDetector, MSER, SimpleBlobDetector, BRISK, KAZE, AKAZE, and AffineFeature wrappers. It borrows the finder, image, and optional mask only for the call. A non-empty mask must be same-sized `CV_8UC1`; non-contiguous image and mask ROIs are valid. Single-image computation assigns `ImageIndex = -1`; batch computation assigns stable zero-based indices. Keypoint arrays are copied, and `GetDescriptors` or `CopyDescriptorsTo` returns independent CPU Mat storage.

Manually constructed `ImageFeatures` clones its descriptors and keypoints. Non-empty descriptors must be two-dimensional, single-channel `CV_8U` or `CV_32F`, with one row per keypoint. Pair matching additionally requires equal descriptor types. Empty descriptors are valid and produce no matches.

Batch matching always returns exactly `N * N` independently owned `MatchesInfo` objects in row-major order: the pair from image `i` to image `j` is at `i * N + j`. An optional match mask must be an exact non-empty `N x N CV_8UC1` matrix. `BestOf2NearestRangeMatcher` further limits candidate pairs to its positive range width. Unmatched and diagonal cells retain source and destination indices of `-1`, empty matches/inliers, zero confidence, and an empty transform.

`MatchesInfo.Matches` and `MatchesInfo.Inliers` return copies. `GetHomography` and `CopyHomographyTo` clone OpenCV's transform into caller-owned storage; homography and affine matchers both expose the upstream 3 by 3 matrix. Disposing a matcher does not invalidate returned feature or match records. All three built-in matchers currently report OpenCV's `IsThreadSafe` result, but callers must query that property rather than infer it from the managed type. `CollectGarbage` releases reusable native matcher working memory.

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

## Detail blender workflow

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Stitching;

using var image = new Mat(64, 64, MatType.CV_8UC3, new Scalar(40, 60, 80));
using var mask = new Mat(64, 64, MatType.CV_8UC1, new Scalar(255));
using var panorama16 = new Mat();
using var panoramaMask = new Mat();
using var blender = new MultiBandBlender(tryGpu: true, numberOfBands: 3);

blender.Prepare(new Rect(0, 0, image.Cols, image.Rows));
blender.Feed(image, mask, new Point(0, 0));
blender.Blend(panorama16, panoramaMask);
```

`Blender.CreateDefault` preserves OpenCV's `None`, `Feather`, and `MultiBand` values. `Blender` and `FeatherBlender` accept `CV_16SC3` feed images. `MultiBandBlender` accepts `CV_8UC3` or `CV_16SC3`. Every feed mask must be a same-sized non-empty `CV_8UC1` Mat. The image rectangle at its top-left coordinate must fit completely within the prepared ROI; the wrapper validates that boundary before upstream pointer arithmetic.

Both `Prepare(corners, sizes)` and `Prepare(Rect)` establish one state cycle. `Feed` may be repeated, and `Blend` is valid even before a feed, producing the prepared zero image and mask. A successful or failed blend invalidates the prepared state because upstream releases or may partially consume its internal destination. Call `Prepare` again before reuse. Blender instances are stateful and not thread-safe.

Blend results are written into caller-owned Mats. The panorama has type `CV_16SC3`; its mask has type `CV_8UC1`. Inputs are borrowed only for the synchronous call. Base blending overwrites masked pixels, feather blending accumulates normalized distance weights, and multi-band blending constructs and normalizes Laplacian pyramids. `tryGpu: true` falls back to the CPU path when the required CUDA modules or a CUDA device are unavailable. `MultiBandBlender` accepts only `CV_32FC1` or `CV_16SC1` weight types and band counts from 0 through 30.

`FeatherBlender.CreateWeightMaps` returns independently owned normalized `CV_32FC1` Mats and their union ROI. Dispose every returned map. `Blender.CreateWeightMap` writes one caller-owned `CV_32FC1` map, while `NormalizeUsingWeightMap` mutates a same-sized `CV_16SC3` source using `CV_32FC1` or `CV_16SC1` weights.

`Blender.CreateLaplacePyramid` returns `numberOfLevels + 1` independently owned Mats. Eight-bit input becomes `CV_16S` at every level; other supported depths retain their type. Each level uses `ceil(previous size / 2)`. `RestoreImageFromLaplacePyramid` reconstructs the first level in place. Empty restore collections are a no-op, but non-empty collections require distinct, non-empty, type-consistent levels with the exact size progression.

The installed OpenCV 5.0.0 full runtime used for current evidence has no CUDA blender backend. Its `createLaplacePyrGpu` and `restoreImageFromLaplacePyrGpu` implementations throw `StsNotImplemented` with `CUDA optimization is unavailable`; the managed GPU-named helpers preserve that `OpenCvException`. They do not silently run the CPU helper and are not evidence of CUDA support.

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

## Seam, timelapse, and geometry details

`SeamFinder.CreateDefault` preserves the upstream `None`, `Voronoi`, and `DynamicProgramming` values. `DpSeamFinder` and `GraphCutSeamFinder` expose strongly typed costs instead of case-sensitive upstream strings. `Find` requires equal non-empty image, corner, and mask collections. Every mask is an exact same-sized `CV_8UC1` Mat, and mask objects must be distinct. Images are borrowed. The native boundary clones all masks into temporary UMat storage and commits every result only after OpenCV succeeds, so exceptions never expose partially updated masks. Non-contiguous images and masks are valid.

`Timelapser.CreateDefault` preserves `AsIs` union and `Crop` intersection behavior. Call `Initialize` with equal non-empty corner and positive-size arrays before `Process` or `GetDestination`. Process images are exact two-dimensional `CV_16SC3` Mats. The upstream mask parameter is currently ignored, but a supplied managed mask must still be a live non-empty Mat. `GetDestination` copies the retained upstream UMat into independently owned CPU Mat storage that remains valid after the timelapser is disposed.

`StitchingUtilities` validates collection lengths, positive sizes, and checked 32-bit placement boundaries before calling `overlapRoi`, both `resultRoi` overloads, `resultRoiIntersection`, or `resultTl`. `SelectRandomSubset` rejects negative counts and `count > size`, returns exactly `count` distinct ascending indices, and uses bounded caller-owned storage. OpenCV 5.0.0 can overshoot that declared count because its implementation applies signed modulo to a random integer; the boundary detects that case and rebuilds the intended exact-count sample with OpenCV's bounded RNG. `LogLevel` is read-only because upstream exposes a mutable process-global reference.

`SphericalProjector` is an owned source-reviewed configuration extension around parser-visible `mapForward` and `mapBackward`. Construction requires a finite positive scale, exact `3 x 3 CV_32FC1` K and R matrices, and an optional `3 x 1` or `1 x 3 CV_32FC1` translation. The matrices are consumed synchronously by upstream `ProjectorBase.setCameraParams`; no Mat pointer is retained. Point mapping then uses only the copied projector state.

## Profile boundaries

Stitching entrypoints are full-profile only. A mini-linked or unlinked wrapper keeps the ABI shape but returns `NOT_LINKED` without allocating handles or mutating outputs; that is not evidence that `opencv_stitching` is present. CUDA/OpenCL-specific input surfaces beyond the source-true blender fallbacks, callback shapes, templates, and mutable nested detail containers remain outside the completed families. LightGlue matching remains explicitly unsupported until the repository owns a stable model-input and `cv::LightGlueMatcher` lifecycle contract.

Parser-emitted rows and source-reviewed extensions remain separate. `Stitcher.WaveCorrectKind`, `Stitcher.GetResultMask`, the explicit managed no-op constructor, and owned `SphericalProjector` configuration are source-reviewed adaptations and do not alter the 207/158 parser counts.
