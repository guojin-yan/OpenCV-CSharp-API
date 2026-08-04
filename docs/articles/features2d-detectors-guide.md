# Features2D Detectors Guide / Features2D 检测器指南

`JYPPX.OpenCvSharp.Features2D` now exposes object wrappers for ORB, SIFT, FAST, GFTT, MSER, SimpleBlobDetector, and the contrib-backed BRISK, KAZE, and AKAZE wrappers. The managed API keeps OpenCV C++ names recognizable while using .NET naming and lifetime rules.

`JYPPX.OpenCvSharp.Features2D` 现在提供 ORB、SIFT、FAST、GFTT、MSER、SimpleBlobDetector 以及 contrib 版本的 BRISK、KAZE、AKAZE 对象封装。managed API 保持 OpenCV C++ 名称可识别，同时遵循 .NET 命名与生命周期规则。

## SIFT / SIFT

`SIFT` supports detection, descriptor computation, and detect-and-compute in one call. Descriptors are `CV_32F` by default, which is suitable for FLANN matching.

`SIFT` 支持检测、描述子计算，以及一次性 detect-and-compute。描述子默认是 `CV_32F`，适合配合 FLANN 匹配。

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(64, 64, MatType.CV_8UC1, new Scalar(0)))
            using (Mat descriptors = new Mat())
            using (SIFT sift = SIFT.Create(nFeatures: 128))
            {
                sift.DetectAndCompute(image, null, out KeyPoint[] keypoints, descriptors);
                Console.WriteLine("SIFT keypoints=" + keypoints.Length + ", descriptor type=" + descriptors.Type);
            }
        }
    }
}
```

## FAST / FAST

`FastFeatureDetector` is detect-only. It exposes `Threshold`, `NonmaxSuppression`, and `Type`, matching OpenCV `cv::FastFeatureDetector`.

`FastFeatureDetector` 只负责检测。它暴露 `Threshold`、`NonmaxSuppression` 和 `Type`，对应 OpenCV `cv::FastFeatureDetector`。

`Type` accepts only `FastFeatureDetectorType.Type5_8`, `Type7_12`, and `Type9_16`. Unknown enum values throw `ArgumentOutOfRangeException` before native FAST creation or before setting the property.

`Type` 只接受 `FastFeatureDetectorType.Type5_8`、`Type7_12` 和 `Type9_16`。未知枚举值会在创建 native FAST 前或设置属性前抛出 `ArgumentOutOfRangeException`。

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(64, 64, MatType.CV_8UC1, new Scalar(0)))
            using (FastFeatureDetector fast = FastFeatureDetector.Create(12, true, FastFeatureDetectorType.Type9_16))
            {
                KeyPoint[] keypoints = fast.Detect(image);
                Console.WriteLine("FAST keypoints=" + keypoints.Length);
            }
        }
    }
}
```

## GFTT / GFTT

`GFTTDetector` wraps OpenCV `cv::GFTTDetector` and mirrors the main `goodFeaturesToTrack` knobs.

`GFTTDetector` 封装 OpenCV `cv::GFTTDetector`，并对齐 `goodFeaturesToTrack` 的主要参数。

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(64, 64, MatType.CV_8UC1, new Scalar(0)))
            using (GFTTDetector gftt = GFTTDetector.Create(maxCorners: 32, qualityLevel: 0.01, minDistance: 2.0))
            {
                KeyPoint[] keypoints = gftt.Detect(image);
                Console.WriteLine("GFTT keypoints=" + keypoints.Length);
            }
        }
    }
}
```

## MSER / MSER

`MSER` wraps OpenCV `cv::MSER`. It can be used as a normal `Feature2D` detector through `Detect`, or as a region detector through `DetectRegions`.

`MSER` 封装 OpenCV `cv::MSER`。它既可以通过 `Detect` 当作普通 `Feature2D` 检测器使用，也可以通过 `DetectRegions` 返回区域点集和边界框。

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(160, 160, MatType.CV_8UC1, new Scalar(0)))
            using (MSER mser = MSER.Create(delta: 6, minArea: 20, maxArea: 30000))
            {
                MserRegion[] regions = mser.DetectRegions(image);
                KeyPoint[] keypoints = mser.Detect(image);
                Console.WriteLine("MSER regions=" + regions.Length + ", keypoints=" + keypoints.Length);
            }
        }
    }
}
```

## Batch Detect / 批量检测

All `Feature2D` objects inherit managed batch detection helpers. The current implementation loops through the single-image native path so API shape is stable before adding a native batch ABI.

所有 `Feature2D` 对象都继承 managed 批量检测辅助方法。当前实现会复用单图 native 路径循环处理，这样可以先稳定 API 形状，后续再决定是否加入 native 批处理 ABI。

```csharp
Mat[] images = new[] { image1, image2 };
KeyPoint[][] perImage = orb.Detect(images);
```

`SimpleBlobDetector` is covered in a separate guide because its parameter object is intentionally larger than the other detectors.

`BRISK`, `KAZE`, and `AKAZE` are covered in a separate guide because they live behind the optional contrib `xfeatures2d` module.

`SimpleBlobDetector` 有单独指南，因为它的参数对象明显比其他检测器更大。

`BRISK`、`KAZE` 和 `AKAZE` 有单独指南，因为它们位于可选 contrib `xfeatures2d` 模块后面。

## Native Boundary / Native 边界

These objects require a native build linked with OpenCV `opencv_features`. When a runtime package does not include that optional module, constructors throw `OpenCvException` with a clear `NOT_LINKED` message.

这些对象需要 native 层真实链接 OpenCV `opencv_features`。当 runtime 包未包含该可选模块时，构造函数会抛出带有明确 `NOT_LINKED` 信息的 `OpenCvException`。
