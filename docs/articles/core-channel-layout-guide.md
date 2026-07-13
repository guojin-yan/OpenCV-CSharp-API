# Core Channel and Layout Guide / Core 通道与布局指南

Core channel APIs handle common matrix layout tasks such as splitting, merging, rotating, transposing, and look-up table transforms.

Core 通道 API 处理常见矩阵布局任务，例如拆分、合并、旋转、转置和查找表变换。

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
            using (Mat color = new Mat(2, 2, MatType.CV_8UC3))
            using (Mat gray = new Mat(2, 2, MatType.CV_8UC1))
            using (Mat flipped = new Mat())
            using (Mat rotated = new Mat())
            {
                color.CopyFrom(new byte[]
                {
                    1, 10, 100,
                    2, 20, 110,
                    3, 30, 120,
                    4, 40, 130
                });
                gray.CopyFrom(new byte[] { 9, 8, 7, 6 });

                Mat[] channels = CoreCv2.Split(color);
                try
                {
                    using (Mat merged = CoreCv2.Merge(channels))
                    using (Mat extracted = CoreCv2.ExtractChannel(color, 1))
                    {
                        CoreCv2.Flip(gray, flipped, 1);
                        CoreCv2.Rotate(gray, rotated, RotateFlags.Rotate90Clockwise);

                        Console.WriteLine($"channels={channels.Length}, merged={merged.Type}");
                        Console.WriteLine(string.Join(",", extracted.ToBytes()));
                        Console.WriteLine(string.Join(",", flipped.ToBytes()));
                    }
                }
                finally
                {
                    for (int i = 0; i < channels.Length; i++)
                    {
                        channels[i].Dispose();
                    }
                }
            }
        }
    }
}
```

## Implemented APIs / 已实现 API

- Channel movement: `Split`, `Merge`, `ExtractChannel`, `InsertChannel`, `MixChannels`.
- Layout transforms: `Repeat`, `Flip`, `Rotate`, `Transpose`.
- Value transforms: `Lut`, `ConvertScaleAbs`.
- Matrix helpers: `CompleteSymm`, `SetIdentity`.

- 通道移动：`Split`、`Merge`、`ExtractChannel`、`InsertChannel`、`MixChannels`。
- 布局变换：`Repeat`、`Flip`、`Rotate`、`Transpose`。
- 值变换：`Lut`、`ConvertScaleAbs`。
- 矩阵辅助：`CompleteSymm`、`SetIdentity`。

## Ownership / 所有权

`Split` returns new `Mat` instances and transfers each native handle to managed ownership. Dispose every returned channel matrix when it is no longer needed.

`Split` 返回新的 `Mat` 实例，并将每个 native 句柄交给 managed 层拥有。不再需要时应释放每个返回的通道矩阵。

`Merge`, `ExtractChannel`, `Repeat`, `Flip`, `Rotate`, `Transpose`, `Lut`, and `ConvertScaleAbs` also provide convenience overloads that allocate a new destination matrix and return it.

`Merge`、`ExtractChannel`、`Repeat`、`Flip`、`Rotate`、`Transpose`、`Lut` 和 `ConvertScaleAbs` 也提供会分配并返回新目标矩阵的便利重载。

## Modern Fast Paths / 现代快速路径

On modern targets, channel handle arrays and mapping buffers use pointer-based interop after validation. This avoids extra marshaling overhead in hot paths such as `Split`, `Merge`, and `MixChannels`.

在现代目标框架上，通道句柄数组和映射缓冲在校验后使用指针 interop。这样可以降低 `Split`、`Merge`、`MixChannels` 等热路径的额外 marshaling 开销。
