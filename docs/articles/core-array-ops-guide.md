# Core Array Operations Guide / Core 数组运算指南

`OpenCvSharp.Core.Cv2` exposes OpenCV core array operations through the stable native C ABI.

`OpenCvSharp.Core.Cv2` 通过稳定 native C ABI 暴露 OpenCV core 数组运算。

When a file also imports `OpenCvSharp.ImgProc` or `OpenCvSharp.ImgCodecs`, use aliases to avoid multiple `Cv2` classes with the same short name.

当文件同时引用 `OpenCvSharp.ImgProc` 或 `OpenCvSharp.ImgCodecs` 时，建议使用别名，避免多个 `Cv2` 类短名冲突。

```csharp
using System;
using OpenCvSharp.Core;
using CoreCv2 = OpenCvSharp.Core.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat a = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat b = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat added = new Mat())
            using (Mat mask = new Mat())
            {
                a.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6 });
                b.CopyFrom(new byte[] { 6, 5, 4, 3, 2, 1 });

                CoreCv2.Add(a, b, added);
                CoreCv2.Compare(a, b, mask, CmpTypes.LT);

                Console.WriteLine(string.Join(",", added.ToBytes()));
                Console.WriteLine(string.Join(",", mask.ToBytes()));
            }
        }
    }
}
```

## Arithmetic And Bitwise / 算术与位运算

The first core operation batch includes destination-matrix overloads for repeated high-throughput processing and convenience overloads that allocate and return a new `Mat`.

第一批 core 运算同时提供目标矩阵重载和返回新 `Mat` 的便利重载。前者适合高吞吐重复处理，后者适合快速上手。

- `Add`, `Subtract`, `Multiply`, `Divide`
- `ScaleAdd`, `AddWeighted`, `AbsDiff`
- `BitwiseAnd`, `BitwiseOr`, `BitwiseXor`, `BitwiseNot`
- `Compare`, `Min`, `Max`, `InRange`, `PatchNaNs`

## Statistics / 统计

Statistics APIs return C# value objects where OpenCV returns multiple out values.

当 OpenCV 返回多个输出值时，统计 API 会使用 C# 值对象表达结果。

```csharp
using System;
using OpenCvSharp.Core;
using CoreCv2 = OpenCvSharp.Core.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat values = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat normalized = new Mat())
            {
                values.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6 });

                Scalar sum = CoreCv2.Sum(values);
                Scalar mean = CoreCv2.Mean(values);
                MeanStdDevResult meanStdDev = CoreCv2.MeanStdDev(values);
                MinMaxLocResult minMax = CoreCv2.MinMaxLoc(values);
                double norm = CoreCv2.Norm(values, NormTypes.L2);

                CoreCv2.Normalize(values, normalized, 0.0, 255.0, NormTypes.MinMax);

                Console.WriteLine($"sum={sum.V0}, mean={mean.V0}, norm={norm}");
                Console.WriteLine($"std={meanStdDev.StdDev.V0}, min={minMax.MinVal}, max={minMax.MaxVal}");
            }
        }
    }
}
```

Implemented statistics and linear algebra APIs:

已实现的统计和线性代数 API：

- `CountNonZero`, `Mean`, `MeanStdDev`, `MinMaxLoc`
- `Norm`, `Normalize`, `Reduce`, `Sum`, `Trace`
- `Determinant`, `Invert`, `Solve`, `Mahalanobis`
- `Gemm`, `MulTransposed`, `Eigen`, `EigenNonSymmetric`, `SolveCubic`, `SolvePoly`

Dedicated guides cover the newer object and transform groups:

新一批对象与变换 API 有单独指南：

- [Core Linear Algebra Guide](core-linear-algebra-guide.md)
- [Core Decomposition Objects Guide](core-decomposition-objects-guide.md)
- [Core Spectral Transform Guide](core-spectral-transform-guide.md)

## Modern Fast Paths / 现代快速路径

On `netcoreapp3.1` and newer, scalar result buffers use stack-allocated spans and pointer-based interop where possible. Older .NET Framework targets keep array-based marshaling fallback while preserving the same public API shape.

在 `netcoreapp3.1` 及更新框架上，标量结果缓冲会尽可能使用 stackalloc span 和指针 interop。老 .NET Framework 目标保留数组 marshaling fallback，同时保持相同的 public API 形状。
