# Features2D BRISK KAZE AKAZE Guide / Features2D BRISK KAZE AKAZE 指南

`BRISK`, `KAZE`, and `AKAZE` are managed `Feature2D` wrappers for OpenCV 5.0.0 contrib `xfeatures2d` detectors and descriptor extractors.

`BRISK`、`KAZE` 和 `AKAZE` 是 OpenCV 5.0.0 contrib `xfeatures2d` 检测器与描述子提取器的 managed `Feature2D` 封装。

## Native Availability / Native 可用性

These classes are always present in `JYPPX.OpenCV.CSharp.API`, but they require a runtime package whose native layer was linked with `opencv_xfeatures2d`. If the module is absent, calls throw `OpenCvException` with a `NOT_LINKED` message.

这些类始终存在于 `JYPPX.OpenCV.CSharp.API` 中，但需要 native 层已链接 `opencv_xfeatures2d` 的 runtime 包。如果该模块缺失，调用会抛出带有 `NOT_LINKED` 信息的 `OpenCvException`。

```csharp
using System;
using OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            try
            {
                using (BRISK brisk = BRISK.Create())
                {
                    Console.WriteLine(brisk.DefaultName);
                }
            }
            catch (OpenCvException ex)
            {
                Console.WriteLine("xfeatures2d unavailable: " + ex.Message);
            }
        }
    }
}
```

## BRISK / BRISK

`BRISK` exposes OpenCV-style `Threshold`, `Octaves`, and `PatternScale` settings. It also supports custom sampling pattern creation through array overloads on all frameworks and `ReadOnlySpan<T>` overloads on modern .NET.

`BRISK` 暴露与 OpenCV 对齐的 `Threshold`、`Octaves` 和 `PatternScale` 设置。它还支持自定义采样 pattern：所有框架可使用数组重载，现代 .NET 可使用 `ReadOnlySpan<T>` 重载。

Custom pattern array overloads require non-null `radiusList` and `numberList` values and throw `ArgumentNullException` before native dispatch when either is null. `indexChange` is optional; passing null uses an empty index-change list. Span overloads pin caller-provided spans only for the duration of the native call.

自定义 pattern 的数组重载要求 `radiusList` 和 `numberList` 都非 null，任一为空时会在进入 native 前抛出 `ArgumentNullException`。`indexChange` 是可选参数；传入 null 会按空 index-change 列表处理。Span 重载只会在 native 调用期间固定调用方提供的 span。

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
            using (Mat image = new Mat(96, 96, MatType.CV_8UC1))
            using (Mat descriptors = new Mat())
            using (BRISK brisk = BRISK.Create(threshold: 24, octaves: 2, patternScale: 1.0F))
            {
                KeyPoint[] keypoints = brisk.Detect(image);
                KeyPoint[] kept = brisk.Compute(image, keypoints, descriptors);
                Console.WriteLine("BRISK keypoints=" + keypoints.Length + ", kept=" + kept.Length);
            }
        }
    }
}
```

```csharp
using System;
using OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
#if NETCOREAPP3_1_OR_GREATER
            ReadOnlySpan<float> radii = stackalloc float[] { 2.0F, 4.0F, 8.0F };
            ReadOnlySpan<int> counts = stackalloc int[] { 8, 12, 16 };
            using (BRISK brisk = BRISK.Create(20, 2, radii, counts))
            {
                Console.WriteLine(brisk.DefaultName);
            }
#else
            using (BRISK brisk = BRISK.Create(20, 2, new[] { 2.0F, 4.0F, 8.0F }, new[] { 8, 12, 16 }))
            {
                Console.WriteLine(brisk.DefaultName);
            }
#endif
        }
    }
}
```

## KAZE / KAZE

`KAZE` exposes `Extended`, `Upright`, `Threshold`, `NOctaves`, `NOctaveLayers`, and `Diffusivity`. `Diffusivity` uses the managed `KazeDiffusivityType` enum.

`KAZE` 暴露 `Extended`、`Upright`、`Threshold`、`NOctaves`、`NOctaveLayers` 和 `Diffusivity`。`Diffusivity` 使用 managed 枚举 `KazeDiffusivityType`。

`Diffusivity` accepts only the defined `KazeDiffusivityType` values: `DiffPmG1`, `DiffPmG2`, `DiffWeickert`, and `DiffCharbonnier`. Unknown enum values throw `ArgumentOutOfRangeException` before native KAZE creation or before setting the property.

`Diffusivity` 只接受已定义的 `KazeDiffusivityType` 值：`DiffPmG1`、`DiffPmG2`、`DiffWeickert` 和 `DiffCharbonnier`。未知枚举值会在创建 native KAZE 前或设置属性前抛出 `ArgumentOutOfRangeException`。

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
            using (Mat image = new Mat(96, 96, MatType.CV_8UC1))
            using (Mat descriptors = new Mat())
            using (KAZE kaze = KAZE.Create(extended: false, upright: false, nOctaves: 3, nOctaveLayers: 3))
            {
                kaze.Diffusivity = KazeDiffusivityType.DiffPmG2;
                kaze.DetectAndCompute(image, null, out KeyPoint[] keypoints, descriptors);
                Console.WriteLine("KAZE keypoints=" + keypoints.Length + ", descriptor type=" + descriptors.Type);
            }
        }
    }
}
```

## AKAZE / AKAZE

`AKAZE` exposes descriptor settings separately from the base `Feature2D.DescriptorType` and `Feature2D.DescriptorSize` metadata. Use `AkazeDescriptorType` and `AkazeDescriptorSize` for OpenCV AKAZE configuration, and use the inherited metadata properties to inspect the actual descriptor matrix shape reported by OpenCV.

`AKAZE` 将描述子配置与基类 `Feature2D.DescriptorType`、`Feature2D.DescriptorSize` 元数据分开。使用 `AkazeDescriptorType` 和 `AkazeDescriptorSize` 配置 OpenCV AKAZE；使用继承的元数据属性查看 OpenCV 实际返回的描述子矩阵形态。

`AkazeDescriptorType` accepts only `DescriptorKazeUpright`, `DescriptorKaze`, `DescriptorMldbUpright`, and `DescriptorMldb`. `Diffusivity` follows the same defined `KazeDiffusivityType` values as `KAZE`. Unknown enum values throw `ArgumentOutOfRangeException` before native AKAZE creation or before setting the property.

`AkazeDescriptorType` 只接受 `DescriptorKazeUpright`、`DescriptorKaze`、`DescriptorMldbUpright` 和 `DescriptorMldb`。`Diffusivity` 使用与 `KAZE` 相同的已定义 `KazeDiffusivityType` 值。未知枚举值会在创建 native AKAZE 前或设置属性前抛出 `ArgumentOutOfRangeException`。

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
            using (Mat image = new Mat(96, 96, MatType.CV_8UC1))
            using (Mat descriptors = new Mat())
            using (AKAZE akaze = AKAZE.Create(
                descriptorType: AkazeDescriptorType.DescriptorMldb,
                descriptorChannels: 3,
                maxPoints: 256))
            {
                akaze.DetectAndCompute(image, null, out KeyPoint[] keypoints, descriptors);
                Console.WriteLine("AKAZE keypoints=" + keypoints.Length + ", descriptor size=" + akaze.DescriptorSize);
            }
        }
    }
}
```

## AffineFeature Backends / AffineFeature 后端

`AffineFeature.Create(Feature2D backend, ...)` and typed overloads support `BRISK`, `KAZE`, and `AKAZE` when the native `xfeatures2d` module is available.

当 native `xfeatures2d` 模块可用时，`AffineFeature.Create(Feature2D backend, ...)` 以及强类型重载支持 `BRISK`、`KAZE` 和 `AKAZE`。

```csharp
using System;
using OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (AKAZE akaze = AKAZE.Create(maxPoints: 128))
            using (AffineFeature affine = AffineFeature.Create(akaze, maxTilt: 1, minTilt: 0))
            {
                Console.WriteLine("backend=" + affine.Backend.DefaultName);
            }
        }
    }
}
```
