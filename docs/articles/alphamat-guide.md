# AlphaMat Guide / AlphaMat 指南

`OpenCvSharp.AlphaMat` wraps the first OpenCV contrib `alphamat` API for information-flow alpha matting.

`OpenCvSharp.AlphaMat` 封装第一批 OpenCV contrib `alphamat` 信息流 alpha matting API。

## Scope / 范围

- `AlphaMatCv2.InfoFlow(Mat image, Mat trimap, Mat result)` writes into a caller-owned output `Mat`.
- `AlphaMatCv2.InfoFlow(Mat image, Mat trimap)` returns a new `Mat`.

- `AlphaMatCv2.InfoFlow(Mat image, Mat trimap, Mat result)` 写入调用方持有的输出 `Mat`。
- `AlphaMatCv2.InfoFlow(Mat image, Mat trimap)` 返回新的 `Mat`。

## Runtime / 运行时

`alphamat` is an optional OpenCV contrib module. Runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_alphamat500.dll` when the module is built. If a runtime lacks it, the managed API shape remains stable and linked calls report `NOT_LINKED`.

`alphamat` 是可选 OpenCV contrib 模块。构建该模块时 runtime staging 会包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_alphamat500.dll`。如果某个 runtime 缺少它，managed API 形状仍保持稳定，linked 调用会报告 `NOT_LINKED`。

## Input Notes / 输入说明

The input image is normally BGR/RGB color and the trimap is a single-channel matrix with foreground, background, and unknown regions. Tiny smoke tests assert only output shape and type because matting quality depends strongly on trimap quality.

输入图像通常是 BGR/RGB 彩色图，trimap 是单通道矩阵，包含前景、背景和未知区域。tiny smoke 只断言输出形状和类型，因为 matting 质量强依赖 trimap 质量。

```csharp
using OpenCvSharp.AlphaMat;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;

internal static class Program
{
    private static void Main()
    {
        using Mat image = new Mat(24, 24, MatType.CV_8UC3, new Scalar(10, 20, 30));
        using Mat trimap = new Mat(24, 24, MatType.CV_8UC1, new Scalar(0));
        Cv2.Rectangle(image, new Rect(6, 6, 12, 12), new Scalar(210, 190, 120), -1);
        Cv2.Rectangle(trimap, new Rect(4, 4, 16, 16), new Scalar(128), -1);
        Cv2.Rectangle(trimap, new Rect(8, 8, 8, 8), new Scalar(255), -1);

        using Mat alpha = AlphaMatCv2.InfoFlow(image, trimap);
    }
}
```
