# Features2D Batch Detect Guide / Features2D 批量检测指南

`Feature2D` provides managed batch detection helpers so ORB, SIFT, FAST, GFTT, MSER, and SimpleBlobDetector can process image collections through a common API.

`Feature2D` 提供 managed 批量检测辅助方法，因此 ORB、SIFT、FAST、GFTT、MSER 和 SimpleBlobDetector 可以通过统一 API 处理图像集合。

## Array API / 数组 API

The array overload is available on every supported target framework. It validates the image collection and then reuses each detector's single-image native `Detect` implementation.

数组重载在所有目标框架上可用。它会校验图像集合，然后复用每个检测器的单图 native `Detect` 实现。

```csharp
using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat first = new Mat(96, 96, MatType.CV_8UC1, new Scalar(0)))
            using (Mat second = new Mat(96, 96, MatType.CV_8UC1, new Scalar(0)))
            using (ORB orb = ORB.Create(maxFeatures: 128, fastThreshold: 8))
            {
                Mat[] images = new[] { first, second };
                KeyPoint[][] keypoints = orb.Detect(images);
                Console.WriteLine("images=" + keypoints.Length);
            }
        }
    }
}
```

## Masks / 掩码

When masks are provided, the mask count must either be zero or match the image count. A `null` masks array means no masks.

提供掩码时，掩码数量必须为 0 或与图像数量一致。`null` 掩码数组表示不使用掩码。

```csharp
using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(96, 96, MatType.CV_8UC1, new Scalar(0)))
            using (Mat mask = new Mat(96, 96, MatType.CV_8UC1, new Scalar(255)))
            using (GFTTDetector gftt = GFTTDetector.Create(maxCorners: 32, qualityLevel: 0.01, minDistance: 2.0))
            {
                KeyPoint[][] keypoints = gftt.Detect(new[] { image }, new[] { mask });
                Console.WriteLine("masked images=" + keypoints.Length);
            }
        }
    }
}
```

## Span API / Span API

Modern target frameworks also expose `ReadOnlySpan<Mat>` overloads. This keeps short-lived image batches on the caller side without forcing an extra collection abstraction.

现代目标框架还提供 `ReadOnlySpan<Mat>` 重载。这样短生命周期图像批次可以留在调用方一侧，不需要额外集合抽象。

```csharp
using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat first = new Mat(96, 96, MatType.CV_8UC1, new Scalar(0)))
            using (Mat second = new Mat(96, 96, MatType.CV_8UC1, new Scalar(0)))
            using (FastFeatureDetector fast = FastFeatureDetector.Create(threshold: 12))
            {
                Mat[] images = new[] { first, second };
#if NETCOREAPP3_1_OR_GREATER
                KeyPoint[][] keypoints = fast.Detect(images.AsSpan());
#else
                KeyPoint[][] keypoints = fast.Detect(images);
#endif
                Console.WriteLine("batch=" + keypoints.Length);
            }
        }
    }
}
```

The current batch helper is intentionally managed-side. If native batching proves useful later, the public API can remain stable while the implementation switches to a lower-copy ABI.

当前批量辅助方法刻意放在 managed 层实现。后续如果 native 批处理确实有价值，可以在保持 public API 稳定的同时把内部实现切换到更少拷贝的 ABI。
