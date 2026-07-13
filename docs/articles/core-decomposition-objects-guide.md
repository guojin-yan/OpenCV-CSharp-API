# Core Decomposition Objects Guide / Core 分解对象指南

`Svd` and `Rng` are the first managed core objects backed by native OpenCV handles outside `Mat`.

`Svd` 和 `Rng` 是除 `Mat` 之外第一批由 OpenCV native 句柄支撑的 managed core 对象。

## SVD / 奇异值分解

`OpenCvSharp.Core.Svd` mirrors the lifetime and main operations of `cv::SVD`.

`OpenCvSharp.Core.Svd` 对齐 `cv::SVD` 的生命周期和主要操作。

```csharp
using System;
using OpenCvSharp.Core;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat a = new Mat(2, 2, MatType.CV_64FC1))
            using (Mat rhs = new Mat(2, 1, MatType.CV_64FC1))
            using (Mat solution = new Mat())
            {
                a.CopyFrom<double>(new double[] { 1.0, 0.0, 0.0, 2.0 });
                rhs.CopyFrom<double>(new double[] { 3.0, 8.0 });

                using (Svd svd = new Svd(a))
                using (Mat w = svd.W)
                {
                    svd.BackSubst(rhs, solution);

                    Console.WriteLine(string.Join(",", w.ToArray<double>()));
                    Console.WriteLine(string.Join(",", solution.ToArray<double>()));
                }
            }
        }
    }
}
```

Available SVD APIs:

已提供的 SVD API：

- Constructors: `Svd()`, `Svd(Mat, SvdFlags)`
- Properties: `W`, `U`, `Vt`, `IsDisposed`
- Instance methods: `Compute`, `BackSubst`
- Static methods: `Compute`, `ComputeValues`, `BackSubst`, `SolveZ`

`W`, `U`, and `Vt` return cloned `Mat` objects owned by the caller.

`W`、`U`、`Vt` 返回由调用方负责释放的克隆 `Mat` 对象。

## RNG / 随机数生成器

`OpenCvSharp.Core.Rng` mirrors `cv::RNG` scalar generation and matrix filling.

`OpenCvSharp.Core.Rng` 对齐 `cv::RNG` 的标量生成和矩阵填充能力。

```csharp
using System;
using OpenCvSharp.Core;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Rng rng = new Rng(42UL))
            using (Mat uniform = new Mat(2, 3, MatType.CV_32SC1))
            using (Mat normal = new Mat(2, 3, MatType.CV_64FC1))
            {
                uint next = rng.Next();
                int scalar = rng.Uniform(1, 10);
                rng.FillUniform(uniform, new Scalar(0), new Scalar(10));
                rng.FillNormal(normal, new Scalar(0.0), new Scalar(1.0));

                Console.WriteLine($"next={next}, scalar={scalar}");
                Console.WriteLine(string.Join(",", uniform.ToArray<int>()));
                Console.WriteLine(normal.ValueCount);
            }
        }
    }
}
```

Available RNG APIs:

已提供的 RNG API：

- Constructors: `Rng()`, `Rng(ulong)`
- Properties: `State`, `IsDisposed`
- Scalar methods: `Next`, `Uniform(int,int)`, `Uniform(float,float)`, `Uniform(double,double)`, `Gaussian`
- Matrix fill methods: `Fill`, `FillUniform`, `FillNormal`

## Ownership / 所有权

Both objects are `IDisposable`. Managed wrappers own native handles and release them through SafeHandle-based interop. Disposed objects reject further native calls with `ObjectDisposedException`.

两个对象都实现 `IDisposable`。managed 包装类拥有 native 句柄，并通过 SafeHandle 互操作释放。对象释放后继续调用 native 方法会抛出 `ObjectDisposedException`。
