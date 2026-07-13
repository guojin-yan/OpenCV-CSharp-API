# XImgProc Guide / XImgProc 指南

`OpenCvSharp.XImgProc` wraps optional contrib `ximgproc` extended image-processing batches from the local OpenCV 5.0.0 tree.

`OpenCvSharp.XImgProc` 封装本地 OpenCV 5.0.0 contrib `ximgproc` 的扩展图像处理能力批次。

## Scope / 范围

- Static helpers: `NiBlackThreshold`, `Thinning`, `AnisotropicDiffusion`, `JointBilateralFilter`, `GuidedFilter`, `RollingGuidanceFilter`, `WeightedMedianFilter`, `DtFilter`, `AmFilter`, `BilateralTextureFilter`, `EdgePreservingFilter`, `FastGlobalSmootherFilter`, `L0Smooth`, `FastHoughTransform`, `HoughPointToLine`, and `PeiLinNormalization`.
- Edge-aware objects: `GuidedFilter` and `FastGlobalSmootherFilter`.
- Superpixel objects: `SuperpixelSLIC`, `SuperpixelSEEDS`, and `SuperpixelLSC`.
- Line detection: `FastLineDetector` with `Mat` output, managed `LineSegment[]` output, and draw helpers.
- Disparity helpers: `DisparityFilter`, `DisparityWLSFilter`, generic WLS creation, disparity visualization, MSE, and bad-pixel percentage.
- Solver and sparse interpolation: `FastBilateralSolverFilter`, `SparseMatchInterpolator`, `EdgeAwareInterpolator`, and `RICInterpolator`.
- Edge/proposal objects: `EdgeDrawing`, `EdgeDrawingParams`, `EdgeDrawingEllipse`, `EdgeBoxes`, and `EdgeBox`.
- Filter utilities: `RidgeDetectionFilter`, Deriche gradients, and Paillou gradients.
- Fourier utilities: `FourierDescriptor`, `TransformFD`, `ContourSampling`, and `ContourFitting`.
- Run-length morphology: `XImgProcRlCv2` threshold, structuring element, dilate, erode, morphology, paint, feasibility, and `Point3i` run creation.
- Segmentation and proposals: `ScanSegment`, `GraphSegmentation`, Selective Search strategies, and Selective Search rectangle proposals.
- Covariance helper: `CovarianceEstimation` for complex-valued matrices.
- Enums for thinning, local binarization, weighted median modes, SLIC variants, domain-transform modes, fast-Hough options, and EdgeDrawing gradient operators.

- 静态 helper：`NiBlackThreshold`、`Thinning`、`AnisotropicDiffusion`、`JointBilateralFilter`、`GuidedFilter`、`RollingGuidanceFilter`、`WeightedMedianFilter`、`DtFilter`、`AmFilter`、`BilateralTextureFilter`、`EdgePreservingFilter`、`FastGlobalSmootherFilter`、`L0Smooth`、`FastHoughTransform`、`HoughPointToLine` 和 `PeiLinNormalization`。
- Edge-aware 对象：`GuidedFilter` 与 `FastGlobalSmootherFilter`。
- 超像素对象：`SuperpixelSLIC`、`SuperpixelSEEDS` 与 `SuperpixelLSC`。
- 线段检测：`FastLineDetector`，支持 `Mat` 输出、managed `LineSegment[]` 输出和绘制 helper。
- Disparity helper：`DisparityFilter`、`DisparityWLSFilter`、generic WLS 创建、disparity 可视化、MSE 和 bad-pixel percentage。
- Solver 与稀疏插值：`FastBilateralSolverFilter`、`SparseMatchInterpolator`、`EdgeAwareInterpolator` 和 `RICInterpolator`。
- Edge/proposal 对象：`EdgeDrawing`、`EdgeDrawingParams`、`EdgeDrawingEllipse`、`EdgeBoxes` 和 `EdgeBox`。
- 滤波工具：`RidgeDetectionFilter`、Deriche 梯度和 Paillou 梯度。
- Fourier 工具：`FourierDescriptor`、`TransformFD`、`ContourSampling` 和 `ContourFitting`。
- Run-length morphology：`XImgProcRlCv2` threshold、structuring element、dilate、erode、morphology、paint、可行性检查和 `Point3i` run 创建。
- 分割与候选框：`ScanSegment`、`GraphSegmentation`、Selective Search strategy 和 Selective Search 矩形 proposal。
- 协方差 helper：面向复数矩阵的 `CovarianceEstimation`。
- 枚举覆盖 thinning、局部二值化、weighted median 模式、SLIC 变体、domain-transform 模式、fast-Hough 选项和 EdgeDrawing 梯度算子。

## Focused Guides / 专题指南

- [XImgProc Disparity Guide](ximgproc-disparity-guide.md)
- [XImgProc Sparse Interpolation Guide](ximgproc-sparse-interpolation-guide.md)
- [XImgProc Edge Guide](ximgproc-edge-guide.md)
- [XImgProc Filter Utilities Guide](ximgproc-filter-utilities-guide.md)
- [XImgProc Fourier Guide](ximgproc-fourier-guide.md)
- [XImgProc Segmentation Guide](ximgproc-segmentation-guide.md)
- [XImgProc Run-Length Morphology Guide](ximgproc-run-length-morphology-guide.md)

## Runtime / 运行时

`ximgproc` is an optional OpenCV contrib module. Runtime staging should include the factual OpenCV 5.0.0 runtime artifact `opencv_ximgproc500.dll` when the module is built. If it is missing, the managed API shape remains stable and calls report `NOT_LINKED`.

`ximgproc` 是可选 OpenCV contrib 模块。构建该模块时，runtime staging 应包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_ximgproc500.dll`。如果缺少该 DLL，managed API 形状仍保持稳定，调用会报告 `NOT_LINKED`。

The same DLL can also be used by related OptFlow algorithms, but `OpenCvSharp.XImgProc` is the first-class public API surface for extended image processing.

同一个 DLL 也可被相关 OptFlow 算法复用，但 `OpenCvSharp.XImgProc` 是扩展图像处理的一等公开 API 接口面。

## Input Notes / 输入说明

XImgProc algorithms are sensitive to image depth, channel count, and parameter scale. Thinning expects a binary-style image. Superpixel results vary with image content and region parameters. Disparity maps commonly use `CV_16SC1` values scaled by 16. Sparse interpolation writes dense flow, commonly `CV_32FC2`. Fourier descriptor helpers expect valid contour matrices and conservative descriptor sizes. Run-length morphology stores RLE runs in `Mat` values and `CreateRLEImage` uses flat `Point3i` run triples. `ScanSegment` expects a Lab `CV_8UC3` image matching its creation size. Covariance estimation expects complex-valued input. `FastLineDetector`, `EdgeDrawing`, `EdgeBoxes`, and Selective Search can validly return zero detections or proposals for tiny or low-contrast images.

XImgProc 算法对图像位深、通道数和参数尺度敏感。Thinning 期望二值图风格输入。超像素结果会随图像内容和 region 参数变化。Disparity map 常见类型为 `CV_16SC1`，数值按 16 缩放。稀疏插值输出 dense flow，常见类型为 `CV_32FC2`。Fourier descriptor helper 期望有效轮廓矩阵和保守的 descriptor 尺寸。run-length morphology 将 RLE run 存放在 `Mat` 中，`CreateRLEImage` 使用平铺 `Point3i` run 三元组。`ScanSegment` 期望尺寸匹配创建参数的 Lab `CV_8UC3` 图像。covariance estimation 期望复数输入。`FastLineDetector`、`EdgeDrawing`、`EdgeBoxes` 和 Selective Search 对 tiny 或低对比图像返回零个检测结果或 proposal 也是合法结果。

`FastBilateralSolverFilter` may require OpenCV to be compiled with EIGEN support. If the linked runtime lacks that capability, calls can report the OpenCV EIGEN error while the managed wrapper and ABI remain valid.

`FastBilateralSolverFilter` 可能要求 OpenCV 编译时启用 EIGEN 支持。如果 linked runtime 缺少该能力，调用可能报告 OpenCV EIGEN 错误；这不影响 managed wrapper 和 ABI 本身有效。

## Smoke / Smoke

Default tests cover managed enum values, value objects, argument validation, and disposed-state behavior without requiring external images, models, cameras, GUI windows, or downloads. Linked native smoke is guarded by:

默认测试覆盖 managed 枚举值、值对象、参数校验和 disposed 状态，不依赖外部图片、模型、摄像头、GUI 窗口或下载。linked native smoke 由以下环境变量保护：

```powershell
$env:OPENCV_CSHARP_NATIVE_SMOKE='1'
dotnet test .\tests\OpenCvSharp.Tests\OpenCvSharp.Tests.csproj -c Release --filter "FullyQualifiedName~XImgProc"
```

The older `OPENCV5SHARP_NATIVE_SMOKE=1` name remains accepted only as an existing-smoke-workflow compatibility alias.

旧的 `OPENCV5SHARP_NATIVE_SMOKE=1` 名称仍仅作为既有 smoke workflow 的兼容别名使用。

## Example / 示例

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.XImgProc;

namespace XImgProcExample
{
    internal static class Program
    {
        private static void Main()
        {
            using Mat gray = new Mat(16, 16, MatType.CV_8UC1, new Scalar(96));
            Cv2.Rectangle(gray, new Rect(4, 4, 8, 8), new Scalar(180), -1);

            using Mat binary = XImgProcCv2.NiBlackThreshold(
                gray,
                255.0,
                ThresholdTypes.Binary,
                blockSize: 3,
                k: -0.2);
            using Mat skeleton = XImgProcCv2.Thinning(binary);

            using GuidedFilter guided = XImgProcCv2.CreateGuidedFilter(gray, radius: 2, eps: 1.0);
            using Mat filtered = guided.Filter(gray);

            using Mat color = new Mat(32, 32, MatType.CV_8UC3, new Scalar(24, 48, 72));
            Cv2.Rectangle(color, new Rect(4, 4, 10, 10), new Scalar(220, 40, 30), -1);

            using SuperpixelSLIC slic = XImgProcCv2.CreateSuperpixelSLIC(color, SLICType.SLICO, 8, 10.0F);
            slic.Iterate(1);
            using Mat labels = slic.GetLabels();
            using Mat contours = slic.GetLabelContourMask();

            using FastLineDetector detector = XImgProcCv2.CreateFastLineDetector(lengthThreshold: 6);
            using Mat lineImage = new Mat(32, 32, MatType.CV_8UC1, new Scalar(0));
            Cv2.Line(lineImage, new Point(3, 4), new Point(28, 24), new Scalar(255), 1);
            LineSegment[] lines = detector.Detect(lineImage);

            using Mat disparity = new Mat(16, 16, MatType.CV_16SC1, new Scalar(16));
            using DisparityWLSFilter wls = XImgProcCv2.CreateDisparityWLSFilterGeneric();
            using Mat filteredDisparity = wls.Filter(disparity, gray, roi: new Rect(0, 0, 16, 16));
            using Mat disparityVis = XImgProcCv2.GetDisparityVis(filteredDisparity);

            using EdgeDrawing edgeDrawing = XImgProcCv2.CreateEdgeDrawing();
            edgeDrawing.DetectEdges(lineImage);
            Point[][] segments = edgeDrawing.GetSegments();
            LineSegment[] drawnLines = edgeDrawing.DetectLines();
        }
    }
}
```
