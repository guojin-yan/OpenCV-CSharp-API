# ImgProc Filter Transform Guide / ImgProc 滤波与变换指南

This guide explains the current filtering, derivative, pyramid, warp, and remap APIs in `OpenCvSharp.ImgProc.Cv2`.

本文说明 `OpenCvSharp.ImgProc.Cv2` 当前的滤波、导数、金字塔、几何变换和重映射 API。

## Filtering / 滤波

Most filtering APIs follow the OpenCV C++ shape: the caller provides `src` and `dst`, and OpenCV allocates or reshapes the destination matrix as needed.

大多数滤波 API 保持 OpenCV C++ 的调用形态：调用者传入 `src` 和 `dst`，OpenCV 会按需要分配或重塑目标矩阵。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat src = new Mat(5, 5, MatType.CV_8UC1))
            using (Mat blurred = new Mat())
            using (Mat boxed = new Mat())
            using (Mat median = new Mat())
            using (Mat bilateral = new Mat())
            {
                src.CopyFrom(new byte[]
                {
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 255, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0
                });

                ImgProcCv2.Blur(src, blurred, new Size(3, 3));
                ImgProcCv2.BoxFilter(src, boxed, -1, new Size(3, 3));
                ImgProcCv2.MedianBlur(src, median, 3);
                ImgProcCv2.BilateralFilter(src, bilateral, 3, 25.0, 25.0);
            }
        }
    }
}
```

Current filtering methods:

当前滤波方法：

- `Blur`
- `BoxFilter`
- `SqrBoxFilter`
- `MedianBlur`
- `BilateralFilter`
- `GaussianBlur`
- `Filter2D`
- `SepFilter2D`

## Kernels / 滤波核

Kernel factory APIs return new `Mat` instances. The managed wrapper owns the native matrix handle, so use `using` or `Dispose`.

滤波核工厂 API 返回新的 `Mat` 实例。managed 包装类拥有 native 矩阵句柄，因此应使用 `using` 或调用 `Dispose`。

```csharp
using (Mat gaussian = ImgProcCv2.GetGaussianKernel(3, 0, MatType.CV_64F))
using (Mat gabor = ImgProcCv2.GetGaborKernel(new Size(3, 3), 1.0, 0.0, 2.0, 0.5))
using (Mat filtered = new Mat())
{
    ImgProcCv2.SepFilter2D(src, filtered, -1, gaussian, gaussian);
}
```

`GetDerivKernels` writes into caller-provided output matrices:

`GetDerivKernels` 会写入调用者提供的输出矩阵：

```csharp
using (Mat kx = new Mat())
using (Mat ky = new Mat())
{
    ImgProcCv2.GetDerivKernels(kx, ky, 1, 0, 3);
}
```

## Derivatives And Edges / 导数与边缘

Derivative APIs are useful for edge detection, feature preprocessing, and custom filtering pipelines.

导数 API 适合用于边缘检测、特征预处理和自定义滤波流水线。

```csharp
using (Mat sobelX = new Mat())
using (Mat sobelY = new Mat())
using (Mat edges = new Mat())
{
    ImgProcCv2.Sobel(src, sobelX, MatType.CV_16S, 1, 0);
    ImgProcCv2.Sobel(src, sobelY, MatType.CV_16S, 0, 1);
    ImgProcCv2.Canny(src, edges, 40.0, 120.0);
}
```

When derivative images are already available, use the derivative overload:

如果已经有导数图像，可以使用导数重载：

```csharp
ImgProcCv2.Canny(sobelX, sobelY, edges, 40.0, 120.0);
```

Current derivative and edge methods:

当前导数和边缘方法：

- `Sobel`
- `Scharr`
- `Laplacian`
- `Canny(Mat image, Mat edges, ...)`
- `Canny(Mat dx, Mat dy, Mat edges, ...)`

## Pyramid And Warp / 金字塔与变换

`PyrDown` and `PyrUp` use OpenCV's Gaussian pyramid operations. Pass `null` or omit `dstsize` to let OpenCV derive the size.

`PyrDown` 和 `PyrUp` 使用 OpenCV 的高斯金字塔操作。传入 `null` 或省略 `dstsize` 时，输出尺寸由 OpenCV 推导。

```csharp
using (Mat down = new Mat())
using (Mat up = new Mat())
{
    ImgProcCv2.PyrDown(src, down);
    ImgProcCv2.PyrUp(down, up, new Size(src.Cols, src.Rows));
}
```

Affine and perspective transforms consume transformation matrices as `Mat`.

仿射和透视变换以 `Mat` 作为变换矩阵输入。

```csharp
using (Mat rotation = ImgProcCv2.GetRotationMatrix2D(new Point2f(0.0F, 0.0F), 0.0, 1.0))
using (Mat warped = new Mat())
{
    ImgProcCv2.WarpAffine(src, warped, rotation, new Size(src.Cols, src.Rows), InterpolationFlags.Nearest);
}
```

Build transforms from points when the control points are known:

已知控制点时，可以由点集构造变换矩阵：

```csharp
Point2f[] sourcePoints = new Point2f[]
{
    new Point2f(0.0F, 0.0F),
    new Point2f(3.0F, 0.0F),
    new Point2f(0.0F, 3.0F)
};

Point2f[] destinationPoints = new Point2f[]
{
    new Point2f(0.0F, 0.0F),
    new Point2f(3.0F, 0.0F),
    new Point2f(0.0F, 3.0F)
};

using (Mat affine = ImgProcCv2.GetAffineTransform(sourcePoints, destinationPoints))
using (Mat warped = new Mat())
{
    ImgProcCv2.WarpAffine(src, warped, affine, new Size(src.Cols, src.Rows));
}
```

On `netcoreapp3.1` and newer, `GetAffineTransform` and `GetPerspectiveTransform` also expose `ReadOnlySpan<Point2f>` overloads. These overloads pin contiguous point memory and avoid creating a temporary interleaved `float[]`.

在 `netcoreapp3.1` 及更新框架上，`GetAffineTransform` 和 `GetPerspectiveTransform` 还提供 `ReadOnlySpan<Point2f>` 重载。这些重载会固定连续点内存，避免创建临时交错 `float[]`。

```csharp
#if NETCOREAPP3_1_OR_GREATER
using (Mat affine = ImgProcCv2.GetAffineTransform(sourcePoints.AsSpan(), destinationPoints.AsSpan()))
{
    // Use affine.
}
#endif
```

Current transform methods:

当前变换方法：

- `PyrDown`
- `PyrUp`
- `WarpAffine`
- `WarpPerspective`
- `GetRotationMatrix2D`
- `GetAffineTransform`
- `GetPerspectiveTransform`
- `InvertAffineTransform`

## Remap / 重映射

`Remap` uses coordinate maps to sample from a source image. Floating-point maps are easy to build and read; `ConvertMaps` can convert them to fixed-point maps for repeated use.

`Remap` 使用坐标映射从源图像采样。浮点映射易于构造和阅读；`ConvertMaps` 可将其转换为定点映射，适合重复使用。

```csharp
using (Mat mapX = new Mat(src.Rows, src.Cols, MatType.CV_32FC1))
using (Mat mapY = new Mat(src.Rows, src.Cols, MatType.CV_32FC1))
using (Mat remapped = new Mat())
using (Mat fixedMap1 = new Mat())
using (Mat fixedMap2 = new Mat())
{
    mapX.CopyFrom<float>(new float[] { 0, 1, 2, 0, 1, 2 });
    mapY.CopyFrom<float>(new float[] { 0, 0, 0, 1, 1, 1 });

    ImgProcCv2.Remap(src, remapped, mapX, mapY, InterpolationFlags.Nearest);
    ImgProcCv2.ConvertMaps(mapX, mapY, fixedMap1, fixedMap2, MatType.CV_16SC2, nninterpolation: true);
}
```

## Boundary Notes / 边界说明

- All current filtering and transform C ABI functions return status codes and use thread-local native error state.
- No OpenCV C++ class, STL container, or C++ exception crosses the C ABI.
- `Mat` input and output parameters are borrowed by native code; returned `Mat` handles are owned by the managed wrapper.
- The fallback path for old .NET Framework targets keeps array-based marshaling, while modern targets add Span-based fast paths where the public API can remain consistent.

- 当前所有滤波和变换 C ABI 函数都返回状态码，并使用线程本地 native 错误状态。
- OpenCV C++ 类、STL 容器和 C++ 异常都不会穿过 C ABI。
- `Mat` 输入输出参数由 native 代码临时借用；返回的 `Mat` 句柄由 managed 包装类拥有。
- 老 .NET Framework 目标保留数组型 marshaling fallback，现代目标在 public API 保持一致的前提下增加 Span 快速路径。
