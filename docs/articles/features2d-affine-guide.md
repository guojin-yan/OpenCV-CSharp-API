# Features2D AffineFeature Guide / Features2D AffineFeature 指南

`AffineFeature` wraps OpenCV `cv::AffineFeature`. It takes an existing `Feature2D` backend and samples affine views before delegating keypoint detection to that backend.

`AffineFeature` 封装 OpenCV `cv::AffineFeature`。它接收一个已有 `Feature2D` 后端，先采样仿射视角，再把关键点检测委托给该后端。

## Create From A Backend / 从后端创建

The managed wrapper keeps the backend object alive for as long as the `AffineFeature` wrapper is alive. Disposing the affine wrapper does not dispose the backend.

managed 封装会在 `AffineFeature` 存活期间保留后端对象引用。释放仿射封装器不会释放后端对象。

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;
using JYPPX.OpenCvSharp.ImgProc;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(96, 96, MatType.CV_8UC1))
            using (ORB orb = ORB.Create(maxFeatures: 128, fastThreshold: 8))
            using (AffineFeature affine = AffineFeature.Create(orb, maxTilt: 1, minTilt: 0))
            {
                image.SetTo(new Scalar(0));
                ImgProcCv2.Rectangle(image, new Rect(8, 8, 28, 28), new Scalar(255), -1);
                ImgProcCv2.Circle(image, new Point(64, 64), 14, new Scalar(230), -1);

                affine.SetViewParams(new[] { 1.0F, 2.0F }, new[] { 0.0F, 45.0F });
                KeyPoint[] keypoints = affine.Detect(image);

                Console.WriteLine("backend=" + affine.Backend.DefaultName);
                Console.WriteLine("affine=" + affine.DefaultName);
                Console.WriteLine("views=" + affine.ViewCount + ", keypoints=" + keypoints.Length);
            }
        }
    }
}
```

## Supported Backends / 支持的后端

The first managed ABI exposes typed creation helpers for these backends:

第一版 managed ABI 为这些后端提供强类型创建辅助方法：

- `ORB`
- `SIFT`
- `FastFeatureDetector`
- `GFTTDetector`
- `MSER`
- `SimpleBlobDetector`
- `BRISK`
- `KAZE`
- `AKAZE`

`AffineFeature.Create(Feature2D backend, ...)` dispatches to one of those typed paths. Unsupported custom subclasses throw `NotSupportedException`.

`AffineFeature.Create(Feature2D backend, ...)` 会分派到对应的强类型路径。不支持的自定义子类会抛出 `NotSupportedException`。

`BRISK`, `KAZE`, and `AKAZE` require the optional contrib `opencv_xfeatures2d` native target. Their managed overloads are always present, but calls report `NOT_LINKED` when the runtime package does not include that module.

`BRISK`、`KAZE` 和 `AKAZE` 需要可选 contrib `opencv_xfeatures2d` native target。对应 managed 重载始终存在，但 runtime 包不包含该模块时会报告 `NOT_LINKED`。

## Managed Validation / managed 参数校验

`AffineFeature.Create(...)` rejects null backends with `ArgumentNullException` before native dispatch. The generic `Feature2D` overload supports only the typed backends listed above; unsupported custom subclasses throw `NotSupportedException` so the managed-to-native backend boundary remains explicit.

`AffineFeature.Create(...)` 会在进入 native 前用 `ArgumentNullException` 拒绝空后端。泛型 `Feature2D` 重载只支持上面列出的强类型后端；不支持的自定义子类会抛出 `NotSupportedException`，从而保持 managed 到 native 后端边界明确。

`SetViewParams` requires tilt and roll collections with matching lengths. Array overloads reject null tilt or roll arrays with `ArgumentNullException`, and both array and span overloads reject mismatched lengths with `ArgumentException`. Span-based `GetViewParams` requires caller-provided tilt and roll destination spans large enough for the configured view count and throws `ArgumentException` when either span is too small.

`SetViewParams` 要求 tilt 和 roll 集合长度一致。数组重载会用 `ArgumentNullException` 拒绝空 tilt 或 roll 数组，数组和 span 重载都会用 `ArgumentException` 拒绝长度不一致的输入。基于 span 的 `GetViewParams` 要求调用方提供的 tilt 和 roll 目标 span 足够容纳当前视角数量，任一 span 过小时会抛出 `ArgumentException`。

## View Parameters / 视角参数

Array overloads are available on every target framework. Modern target frameworks also expose `ReadOnlySpan<float>` and `Span<float>` overloads for short-lived buffers.

数组重载在所有目标框架上可用。现代目标框架还提供 `ReadOnlySpan<float>` 和 `Span<float>` 重载，用于短生命周期缓冲。

```csharp
using System;
using JYPPX.OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (ORB orb = ORB.Create())
            using (AffineFeature affine = AffineFeature.Create(orb))
            {
#if NETCOREAPP3_1_OR_GREATER
                ReadOnlySpan<float> tilts = stackalloc float[] { 1.0F, 1.41421356F };
                ReadOnlySpan<float> rolls = stackalloc float[] { 0.0F, 30.0F };
                Span<float> returnedTilts = stackalloc float[2];
                Span<float> returnedRolls = stackalloc float[2];

                affine.SetViewParams(tilts, rolls);
                int written = affine.GetViewParams(returnedTilts, returnedRolls);
                Console.WriteLine("span views=" + written);
#else
                affine.SetViewParams(new[] { 1.0F, 1.41421356F }, new[] { 0.0F, 30.0F });
                affine.GetViewParams(out float[] returnedTilts, out float[] returnedRolls);
                Console.WriteLine("array views=" + returnedTilts.Length + "/" + returnedRolls.Length);
#endif
            }
        }
    }
}
```

## Native Boundary / native 边界

The C ABI exposes typed `jyppx_ocv_features2d_affine_create_from_*` functions instead of passing C++ `cv::Ptr<Feature2D>` across the boundary. This keeps the ABI stable and avoids leaking C++ ownership semantics into managed code.

C ABI 暴露强类型 `jyppx_ocv_features2d_affine_create_from_*` 函数，而不是跨边界传递 C++ `cv::Ptr<Feature2D>`。这样可以保持 ABI 稳定，并避免把 C++ 所有权语义泄漏到 managed 层。
