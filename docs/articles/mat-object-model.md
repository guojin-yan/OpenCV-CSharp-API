# Mat Object Model / Mat 对象模型

`JYPPX.OpenCvSharp.Core.Mat` is the managed object wrapper for OpenCV `cv::Mat`. It owns an opaque native `jyppx_ocv_mat*` handle and releases it through the native C ABI.

`JYPPX.OpenCvSharp.Core.Mat` 是 OpenCV `cv::Mat` 的 managed 对象包装。它持有 opaque native `jyppx_ocv_mat*` 句柄，并通过 native C ABI 释放。

## Creation / 创建

The first complete `Mat` object batch includes empty matrices, typed allocation, scalar-filled allocation, and OpenCV-style factory helpers:

第一批完整 `Mat` 对象能力包含空矩阵、指定类型分配、标量填充分配，以及接近 OpenCV 的工厂方法：

```csharp
using System;
using JYPPX.OpenCvSharp.Core;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat empty = new Mat())
            using (Mat image = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat filled = new Mat(2, 2, MatType.CV_8UC1, new Scalar(7)))
            using (Mat zeros = Mat.Zeros(2, 2, MatType.CV_8UC1))
            using (Mat ones = Mat.Ones(new Size(2, 2), MatType.CV_8UC1))
            using (Mat eye = Mat.Eye(3, 3, MatType.CV_8UC1))
            {
                image.SetTo(new Scalar(3));
                Console.WriteLine(empty.Empty);
                Console.WriteLine(filled.ByteLength);
                Console.WriteLine(zeros.Rows);
                Console.WriteLine(ones.Cols);
                Console.WriteLine(eye.Type);
            }
        }
    }
}
```

## Deep Copy And Views / 深拷贝与视图

`Clone()` creates independent storage. `CopyTo(Mat)` copies into a destination matrix. `SubMat(Rect)`, `RowRange`, `ColRange`, `Row`, `Col`, and `Reshape` create OpenCV-style matrix views when OpenCV can represent the operation without copying.

`Clone()` 会创建独立存储。`CopyTo(Mat)` 会复制到目标矩阵。`SubMat(Rect)`、`RowRange`、`ColRange`、`Row`、`Col` 和 `Reshape` 会在 OpenCV 可无拷贝表达时创建矩阵视图。

```csharp
using System;
using JYPPX.OpenCvSharp.Core;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat source = new Mat(3, 4, MatType.CV_8UC1))
            {
                source.CopyFrom(new byte[]
                {
                    1, 2, 3, 4,
                    5, 6, 7, 8,
                    9, 10, 11, 12
                });

                using (Mat clone = source.Clone())
                using (Mat roi = source.SubMat(new Rect(1, 1, 2, 2)))
                using (Mat row = source.Row(1))
                using (Mat reshaped = source.Reshape(3, 2))
                {
                    clone.SetTo(new Scalar(9));
                    roi.SetTo(new Scalar(99));

                    Console.WriteLine(source.IsContinuous);
                    Console.WriteLine(roi.IsSubmatrix);
                    Console.WriteLine(row.Rows);
                    Console.WriteLine(reshaped.Channels);
                }
            }
        }
    }
}
```

Views share native data with the source matrix. Mutating a view mutates the referenced region. Clone a view first when an independent continuous buffer is required.

视图会与源矩阵共享 native 数据。修改视图会修改源矩阵对应区域。如果需要独立且连续的缓冲区，应先对视图调用 `Clone()`。

## Data Access / 数据访问

Byte array APIs are available for all target frameworks. `CopyTo(byte[])` and `CopyFrom(byte[])` copy the logical matrix payload row by row, so a two-dimensional ROI or column view no longer requires `IsContinuous == true`. `RowByteLength` reports the bytes in one logical row and excludes padding represented by `Step`.

字节数组 API 在所有目标框架中可用。`CopyTo(byte[])` 与 `CopyFrom(byte[])` 会按逻辑行复制二维矩阵，因此 ROI 或列视图不再要求 `IsContinuous == true`；`RowByteLength` 返回单行逻辑字节数，不包含 `Step` 表示的填充。

```csharp
byte[] input = new byte[] { 1, 2, 3, 4 };
byte[] output = new byte[4];

using (Mat mat = new Mat(2, 2, MatType.CV_8UC1))
{
    mat.CopyFrom(input);
    mat.CopyTo(output);
}
```

For a non-contiguous view, use the explicit row methods when a caller is processing one row at a time. They never copy padding bytes and never expose the native data pointer:

```csharp
using (Mat source = new Mat(3, 4, MatType.CV_8UC1))
using (Mat roi = source.SubMat(new Rect(1, 0, 2, 3)))
{
    byte[] row = new byte[roi.RowByteLength];
    roi.CopyRowTo(1, row);
    row[0] = 99;
    roi.CopyRowFrom(1, row);
}
```

Color and multi-channel matrices can use OpenCV-compatible pixel vectors. A row span represents only the logical row, so it works for a non-contiguous ROI while preserving the parent matrix stride:

```csharp
using Mat image = new Mat(480, 640, MatType.CV_8UC3);
using Mat roi = image.SubMat(new Rect(20, 30, 100, 50));

Span<Vec3b> row = roi.AsRowSpan<Vec3b>(0);
row[0] = new Vec3b(255, 0, 0); // B, G, R
Vec3b pixel = roi.GetValue<Vec3b>(0, 0);
```

`AsRowByteSpan`, `AsRowSpan<T>`, `GetValue<T>(row, column)`, and `SetValue<T>(row, column, value)` do not require a continuous matrix. `GetValue` and `SetValue` require `sizeof(T)` to match one complete matrix element, which prevents a three-channel pixel from being accidentally treated as one byte.

For full image loops, `AsRows<T>()` validates the element type once and returns a stack-only `MatRowAccessor<T>`. Its indexer honors the matrix step, so the same loop works for continuous images and non-contiguous ROIs:

```csharp
MatRowAccessor<Vec3b> rows = image.AsRows<Vec3b>();
for (int y = 0; y < rows.Count; y++)
{
    Span<Vec3b> row = rows[y];
    for (int x = 0; x < row.Length; x++)
    {
        Vec3b pixel = row[x];
        row[x] = new Vec3b(pixel.V0, pixel.V1, 255);
    }
}
```

For native, camera, or UI buffers with their own pitch, use `CopyPixelsTo(pointer, destinationStep)` or `CopyPixelsFrom(pointer, sourceStep)`. They copy `RowByteLength` bytes per row and leave external padding untouched. The caller owns the pointer and must keep the complete strided buffer valid for the call.

On `netcoreapp3.1` and newer targets, `Mat` also exposes modern low-copy access through `Span<T>`:

在 `netcoreapp3.1` 及更新目标框架上，`Mat` 还通过 `Span<T>` 提供现代少拷贝访问路径：

```csharp
using System;
using JYPPX.OpenCvSharp.Core;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat mat = Mat.Zeros(2, 3, MatType.CV_8UC1))
            {
#if NETCOREAPP3_1_OR_GREATER
                Span<byte> pixels = mat.AsSpan<byte>();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = (byte)(i + 1);
                }

                if (mat.TryGetByteSpan(out Span<byte> view))
                {
                    Console.WriteLine(view.Length);
                }
#else
                mat.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6 });
                Console.WriteLine(mat.ByteLength);
#endif
            }
        }
    }
}
```

`CopyTo(Span<byte>)`, `CopyFrom(ReadOnlySpan<byte>)`, and their typed span overloads use the same logical row-by-row semantics as the byte-array APIs. A single `AsByteSpan()` or `AsSpan<T>()` still requires `Mat.IsContinuous == true`, because a span cannot represent a stride; use `Clone()` first when a contiguous span is required.

`CopyTo(Span<byte>)`、`CopyFrom(ReadOnlySpan<byte>)` 及其 typed Span 重载与字节数组 API 使用相同的按行语义。单个 `AsByteSpan()` 或 `AsSpan<T>()` 仍要求 `Mat.IsContinuous == true`，因为 Span 无法表示步长；需要连续 Span 时请先调用 `Clone()`。

## Native ABI / Native ABI

The current `Mat` C ABI keeps `cv::Mat` hidden behind `jyppx_ocv_mat*`. It supports construction, factories, lifecycle, shape queries, storage queries, deep copy, shared views, reshape, and continuous/submatrix checks.

当前 `Mat` C ABI 将 `cv::Mat` 隐藏在 `jyppx_ocv_mat*` 后面。它支持构造、工厂方法、生命周期、形状查询、存储查询、深拷贝、共享视图、reshape，以及连续/子矩阵判断。

No C++ STL type, `cv::Mat`, or exception crosses the C ABI.

C ABI 不跨越任何 C++ STL 类型、`cv::Mat` 或异常。
