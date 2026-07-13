# XImgProc Filter Utilities Guide / XImgProc 滤波工具指南

`OpenCvSharp.XImgProc` includes the model-free ridge and recursive gradient utilities from OpenCV contrib `ximgproc`.

`OpenCvSharp.XImgProc` 已包含 OpenCV contrib `ximgproc` 中不依赖模型文件的 ridge 与递归梯度工具。

## Scope / 范围

- `RidgeDetectionFilter`: opaque native object with `Create` and `GetRidgeFilteredImage`.
- `XImgProcCv2.GradientDericheX` and `GradientDericheY`.
- `XImgProcCv2.GradientPaillouX` and `GradientPaillouY`.

- `RidgeDetectionFilter`：opaque native 对象，包含 `Create` 和 `GetRidgeFilteredImage`。
- `XImgProcCv2.GradientDericheX` 与 `GradientDericheY`。
- `XImgProcCv2.GradientPaillouX` 与 `GradientPaillouY`。

## Input Notes / 输入说明

The OpenCV algorithms are sensitive to image type and parameter scale. The thin wrapper passes caller-owned `Mat` values through the stable native ABI, so OpenCV reports type or parameter problems directly as `OpenCvException`.

这些 OpenCV 算法对图像类型和参数尺度敏感。薄包装通过稳定 native ABI 传递调用方持有的 `Mat`，因此类型或参数问题会由 OpenCV 直接以 `OpenCvException` 报告。

## Example / 示例

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.XImgProc;

namespace XImgProcFilterUtilitiesExample
{
    internal static class Program
    {
        private static void Main()
        {
            using Mat color = new Mat(32, 32, MatType.CV_8UC3, new Scalar(24, 48, 72));
            Cv2.Rectangle(color, new Rect(6, 6, 12, 12), new Scalar(220, 40, 30), -1);

            using Mat dericheX = XImgProcCv2.GradientDericheX(color, 0.5, 0.0005);
            using Mat dericheY = XImgProcCv2.GradientDericheY(color, 0.5, 0.0005);
            using Mat paillouX = XImgProcCv2.GradientPaillouX(color, 1.0, 1.0);
            using Mat paillouY = XImgProcCv2.GradientPaillouY(color, 1.0, 1.0);

            using Mat gray = new Mat(32, 32, MatType.CV_8UC1, new Scalar(96));
            Cv2.Rectangle(gray, new Rect(8, 8, 12, 12), new Scalar(180), -1);
            using RidgeDetectionFilter ridge = XImgProcCv2.CreateRidgeDetectionFilter();
            using Mat ridges = ridge.GetRidgeFilteredImage(gray);
        }
    }
}
```

## Smoke / Smoke

Default tests cover managed argument validation. Linked smoke runs tiny synthetic images when `OPENCV_CSHARP_NATIVE_SMOKE=1`. The older `OPENCV5SHARP_NATIVE_SMOKE=1` name remains accepted only as an existing-smoke-workflow compatibility alias.

默认测试覆盖 managed 参数校验。设置 `OPENCV_CSHARP_NATIVE_SMOKE=1` 后，linked smoke 会使用 tiny 合成图像执行真实 native 调用。旧的 `OPENCV5SHARP_NATIVE_SMOKE=1` 名称仍仅作为既有 smoke workflow 的兼容别名使用。
