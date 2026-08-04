# Core Linear Algebra Guide / Core 线性代数指南

`JYPPX.OpenCvSharp.Core.Cv2` exposes the first OpenCV core linear algebra batch through the native C ABI.

`JYPPX.OpenCvSharp.Core.Cv2` 通过 native C ABI 暴露第一批 OpenCV core 线性代数能力。

## Matrix Multiplication / 矩阵乘法

Use `Gemm` for OpenCV-compatible generalized matrix multiplication:

使用 `Gemm` 执行与 OpenCV 兼容的广义矩阵乘法：

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat a = new Mat(2, 2, MatType.CV_64FC1))
            using (Mat b = new Mat(2, 2, MatType.CV_64FC1))
            using (Mat bias = new Mat(2, 2, MatType.CV_64FC1, new Scalar(1.0)))
            using (Mat dst = new Mat())
            {
                a.CopyFrom<double>(new double[] { 1.0, 2.0, 3.0, 4.0 });
                b.CopyFrom<double>(new double[] { 5.0, 6.0, 7.0, 8.0 });

                CoreCv2.Gemm(a, b, 1.0, bias, 1.0, dst);

                Console.WriteLine(string.Join(",", dst.ToArray<double>()));
            }
        }
    }
}
```

`GemmFlags` maps to OpenCV transpose flags and keeps C# names readable:

`GemmFlags` 映射到 OpenCV 转置标志，并保持 C# 命名可读：

- `TransposeSrc1`
- `TransposeSrc2`
- `TransposeSrc3`

## Matrix Products And Eigen APIs / 矩阵乘积与特征值 API

The current batch includes destination overloads for repeated processing and convenience overloads that allocate a new `Mat`.

当前批次同时包含适合重复处理的目标矩阵重载，以及会分配并返回新 `Mat` 的便利重载。

- `Gemm`
- `MulTransposed`
- `Eigen`
- `EigenNonSymmetric`
- `SolveCubic`
- `SolvePoly`

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat symmetric = new Mat(2, 2, MatType.CV_64FC1))
            using (Mat eigenvalues = new Mat())
            using (Mat eigenvectors = new Mat())
            using (Mat coeffs = new Mat(4, 1, MatType.CV_64FC1))
            using (Mat roots = new Mat())
            {
                symmetric.CopyFrom<double>(new double[] { 2.0, 0.0, 0.0, 3.0 });
                coeffs.CopyFrom<double>(new double[] { 1.0, -6.0, 11.0, -6.0 });

                bool ok = CoreCv2.Eigen(symmetric, eigenvalues, eigenvectors);
                int rootCount = CoreCv2.SolveCubic(coeffs, roots);

                Console.WriteLine($"eigen={ok}, values={string.Join(",", eigenvalues.ToArray<double>())}");
                Console.WriteLine($"roots={rootCount}, data={string.Join(",", roots.ToArray<double>())}");
            }
        }
    }
}
```

## Transform APIs / 变换 API

`Transform` and `PerspectiveTransform` accept matrix-backed point/vector data. They are useful as the low-level core equivalent of OpenCV vector transforms.

`Transform` 和 `PerspectiveTransform` 接收矩阵承载的点/向量数据，适合作为 OpenCV 向量变换的 core 层基础能力。

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat points = new Mat(1, 2, MatType.CV_32FC2))
            using (Mat affine = new Mat(2, 3, MatType.CV_32FC1))
            using (Mat transformed = new Mat())
            {
                points.CopyFrom<float>(new float[] { 1.0F, 2.0F, 3.0F, 4.0F });
                affine.CopyFrom<float>(new float[] { 1.0F, 0.0F, 10.0F, 0.0F, 1.0F, 20.0F });

                CoreCv2.Transform(points, transformed, affine);

                Console.WriteLine(string.Join(",", transformed.ToArray<float>()));
            }
        }
    }
}
```

## Performance Notes / 性能说明

Prefer destination-matrix overloads in hot loops so the caller can reuse `Mat` storage. Convenience overloads are intended for quick scripts, tests, and readable one-off operations.

热路径中建议优先使用目标矩阵重载，让调用方复用 `Mat` 存储。返回新 `Mat` 的便利重载更适合快速示例、测试和一次性操作。
