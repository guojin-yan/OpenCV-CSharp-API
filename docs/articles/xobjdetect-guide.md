# XObjDetect Guide

`OpenCvSharp.XObjDetect` wraps the first optional contrib object-detection APIs found in the local OpenCV 5.0.0 contrib tree. In this source layout, legacy cascade and HOG workflows are in `opencv_contrib/modules/xobjdetect`, not the main OpenCV `objdetect` module.

`OpenCvSharp.XObjDetect` 封装本地 OpenCV 5.0.0 contrib 源码树中的第一批可选目标检测 API。在当前源码布局中，传统级联分类器和 HOG 工作流位于 `opencv_contrib/modules/xobjdetect`，而不是 OpenCV 主仓库 `objdetect` 模块。

## Main vs Contrib / 主线与 contrib

- `OpenCvSharp.ObjDetect` maps to the main factual OpenCV 5.0.0 runtime artifact `opencv_objdetect500.dll` module and contains QR, barcode, and DNN-backed face APIs.
- `OpenCvSharp.XObjDetect` maps to optional contrib factual OpenCV 5.0.0 runtime artifact `opencv_xobjdetect500.dll` and currently contains cascade and HOG APIs.
- If OpenCV is built without contrib `xobjdetect`, the managed API shape still exists, but native calls report the defined `NOT_LINKED` boundary.

- `OpenCvSharp.ObjDetect` 对应主线事实性 OpenCV 5.0.0 runtime 产物 `opencv_objdetect500.dll` 模块，包含二维码、条形码和 DNN 人脸 API。
- `OpenCvSharp.XObjDetect` 对应可选 contrib 事实性 OpenCV 5.0.0 runtime 产物 `opencv_xobjdetect500.dll`，当前包含级联分类器和 HOG API。
- 如果 OpenCV 构建未包含 contrib `xobjdetect`，managed API 形状仍保持存在，但 native 调用会返回定义好的 `NOT_LINKED` 边界。

## Covered APIs / 已覆盖接口

- `CascadeClassifier`: default creation, file creation, `Load`, `Empty`, `DetectMultiScale`, `DetectMultiScale2`, `DetectMultiScale3`, `GetOriginalWindowSize`, `IsOldFormatCascade`, and `GetFeatureType`.
- `CascadeDetectionResult`: rectangles, reject levels or detection counts, and level weights.
- `CascadeClassifierFlags`: cascade detection flag values.
- `HOGDescriptor`: default creation, full-parameter creation, file creation, people-detector vector factories, `SetSVMDetector`, `Detect`, `DetectMultiScale`, `CheckDetectorSize`, `GetDescriptorSize`, `GetWinSigma`, and primary descriptor properties.
- `HOGDetectionResult`: single-scale locations, multi-scale rectangles, and confidence weights.
- `HOGDescriptorHistogramNormType`: histogram normalization enum.

- `CascadeClassifier`：默认创建、文件创建、`Load`、`Empty`、`DetectMultiScale`、`DetectMultiScale2`、`DetectMultiScale3`、`GetOriginalWindowSize`、`IsOldFormatCascade` 和 `GetFeatureType`。
- `CascadeDetectionResult`：矩形、reject levels 或检测次数，以及 level weights。
- `CascadeClassifierFlags`：级联检测 flag 值。
- `HOGDescriptor`：默认创建、完整参数创建、文件创建、行人检测器向量工厂、`SetSVMDetector`、`Detect`、`DetectMultiScale`、`CheckDetectorSize`、`GetDescriptorSize`、`GetWinSigma` 和主要描述子属性。
- `HOGDetectionResult`：单尺度位置、多尺度矩形和置信权重。
- `HOGDescriptorHistogramNormType`：直方图归一化枚举。

## Cascade Classifier / 级联分类器

Cascade models are loaded from user-supplied XML files. Default tests and samples do not bundle Haar or LBP models.

级联模型由用户提供 XML 文件加载。默认测试和示例不内置 Haar 或 LBP 模型。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgCodecs;
using OpenCvSharp.XObjDetect;

namespace XObjDetectCascadeSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = Cv2.ImRead("face.jpg", ImreadModes.Grayscale))
            using (CascadeClassifier cascade = new CascadeClassifier())
            {
                if (cascade.Load("haarcascade_frontalface_default.xml"))
                {
                    Rect[] faces = cascade.DetectMultiScale(image, 1.1, 3);
                    System.Console.WriteLine("Faces=" + faces.Length);
                }
            }
        }
    }
}
```

`DetectMultiScale2` returns rectangles with detection counts in `RejectLevels`. `DetectMultiScale3` can return reject levels and level weights when the underlying OpenCV cascade supports that path.

`DetectMultiScale2` 返回矩形，并在 `RejectLevels` 中放置检测次数。`DetectMultiScale3` 在底层 OpenCV cascade 支持时可返回 reject levels 和 level weights。

## HOG Descriptor / HOG 描述子

`HOGDescriptor` exposes OpenCV's default and Daimler people detector vectors, plus custom SVM detector loading through `float[]` and `ReadOnlySpan<float>` on modern .NET targets.

`HOGDescriptor` 暴露 OpenCV 默认行人检测器和 Daimler 行人检测器向量，并在现代 .NET 目标上通过 `float[]` 与 `ReadOnlySpan<float>` 支持自定义 SVM detector。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgCodecs;
using OpenCvSharp.XObjDetect;

namespace XObjDetectHogSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = Cv2.ImRead("people.jpg", ImreadModes.Color))
            using (HOGDescriptor hog = new HOGDescriptor())
            {
                hog.SetSVMDetector(HOGDescriptor.GetDefaultPeopleDetector());
                HOGDetectionResult result = hog.DetectMultiScale(image);

                System.Console.WriteLine("People=" + result.Rectangles.Length);
            }
        }
    }
}
```

The main HOG properties mirror OpenCV names: `WinSize`, `BlockSize`, `BlockStride`, `CellSize`, `NBins`, `DerivAperture`, `WinSigma`, `HistogramNormType`, `L2HysThreshold`, `GammaCorrection`, `NLevels`, and `SignedGradient`.

HOG 主要属性贴近 OpenCV 命名：`WinSize`、`BlockSize`、`BlockStride`、`CellSize`、`NBins`、`DerivAperture`、`WinSigma`、`HistogramNormType`、`L2HysThreshold`、`GammaCorrection`、`NLevels` 和 `SignedGradient`。

## Runtime Notes / 运行时说明

`xobjdetect` is optional. Runtime staging copies the factual OpenCV 5.0.0 runtime artifact `opencv_xobjdetect500.dll` only when the OpenCV build provides it. Cascade results depend on user-supplied cascade XML files; HOG detection depends on image content and the selected SVM vector.

`xobjdetect` 是可选模块。runtime staging 只有在 OpenCV 构建提供该模块时才会复制事实性 OpenCV 5.0.0 runtime 产物 `opencv_xobjdetect500.dll`。级联检测结果依赖用户提供的 cascade XML 文件；HOG 检测结果依赖图像内容和所选 SVM 向量。

The native C ABI never exposes contrib C++ object layout, `std::vector`, `cv::InputArray`, or `cv::OutputArray`; arrays use count/fill marshalling.

native C ABI 不暴露 contrib C++ 对象布局、`std::vector`、`cv::InputArray` 或 `cv::OutputArray`；数组通过 count/fill 封送。
