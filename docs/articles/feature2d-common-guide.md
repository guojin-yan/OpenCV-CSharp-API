# Feature2D Common Guide / Feature2D 通用指南

`Feature2D` is the managed base class for OpenCV feature detectors and descriptor extractors. ORB, SIFT, FAST, GFTT, MSER, SimpleBlobDetector, and AffineFeature share this contract.

`Feature2D` 是 OpenCV 特征检测器与描述子提取器的 managed 基类。ORB、SIFT、FAST、GFTT、MSER、SimpleBlobDetector 和 AffineFeature 都共享这份契约。

## Common Metadata / 通用元数据

Every implemented detector exposes the same OpenCV metadata shape:

每个已实现检测器都暴露一致的 OpenCV 元数据形状：

- `Empty`: whether the native algorithm object is empty.
- `DescriptorSize`: descriptor size reported by OpenCV.
- `DescriptorType`: descriptor matrix type reported by OpenCV.
- `DefaultNorm`: default norm for descriptor matching.
- `DefaultName`: OpenCV's default algorithm name for serialization and diagnostics.

- `Empty`：native 算法对象是否为空。
- `DescriptorSize`：OpenCV 返回的描述子尺寸。
- `DescriptorType`：OpenCV 返回的描述子矩阵类型。
- `DefaultNorm`：描述子匹配默认范数。
- `DefaultName`：OpenCV 用于序列化和诊断的默认算法名称。

```csharp
using System;
using OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (ORB orb = ORB.Create(maxFeatures: 128, fastThreshold: 8))
            {
                Console.WriteLine("name=" + orb.DefaultName);
                Console.WriteLine("descriptor size=" + orb.DescriptorSize);
                Console.WriteLine("descriptor type=" + orb.DescriptorType);
                Console.WriteLine("default norm=" + orb.DefaultNorm);
            }
        }
    }
}
```

## Batch Detect / 批量检测

`Feature2D` provides array-based batch detection on all target frameworks and span-based overloads on modern .NET targets.

`Feature2D` 在所有目标框架上提供数组批量检测，并在现代 .NET 目标上提供 Span 重载。

Batch detection is a managed helper over the single-image `Detect` path. Image collections must be non-null, non-empty, and contain only non-null `Mat` instances. Optional mask collections may be null or empty to mean "no masks"; when masks are supplied, the mask count must match the image count. Invalid image collections throw `ArgumentNullException` or `ArgumentException` before any per-image detect call, and mismatched mask counts throw `ArgumentException`.

批量检测是基于单图 `Detect` 路径的 managed 辅助方法。图像集合必须非空、至少包含一张图像，并且每个元素都必须是非 null 的 `Mat`。可选 mask 集合可以为 null 或空集合，表示不使用 mask；如果提供 mask，mask 数量必须与图像数量一致。非法图像集合会在执行逐图检测前抛出 `ArgumentNullException` 或 `ArgumentException`，mask 数量不匹配会抛出 `ArgumentException`。

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
                Console.WriteLine("image count=" + keypoints.Length);
            }
        }
    }
}
```

## Lifetime / 生命周期

All concrete `Feature2D` objects own a native handle and implement `IDisposable`. After disposal, object methods and metadata properties throw `ObjectDisposedException`.

所有具体 `Feature2D` 对象都拥有 native handle，并实现 `IDisposable`。释放后，对象方法和元数据属性会抛出 `ObjectDisposedException`。

This rule also applies when a detector is used through the base class:

通过基类引用使用检测器时，也遵守这条规则：

```csharp
using System;
using OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Feature2D feature = ORB.Create())
            {
                Console.WriteLine(feature.DefaultName);
            }
        }
    }
}
```
