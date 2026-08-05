# Tutorial Series / 教程系列

This is the primary learning path for OpenCV CSharp API. Every tutorial implements a complete workflow with deterministic or documented input, executable code, inspectable output, metrics, runtime requirements, and links to deeper module guides.

这是 OpenCV CSharp API 的主要学习路线。每篇教程都实现一个完整工作流，包含确定性或明确说明的输入、可执行代码、可检查输出、量化指标、runtime 要求和深入模块指南。

![OpenCV CSharp API tutorial workflows](../images/showcase/showcase-overview.png)

## Learning Path / 学习路线

| # | Tutorial / 教程 | Output / 输出 | Runtime |
|---:|---|---|---|
| 01 | [Image Pipeline / 图像流水线](tutorial-01-image-pipeline.md) | `image-pipeline.png` | Mini or Full |
| 02 | [OpenCV PutText With Chinese / OpenCV 中文写字](tutorial-02-chinese-puttext.md) | `chinese-text.png` | Mini or Full |
| 03 | [Contours And Objects / 轮廓与目标](tutorial-03-contours.md) | `contours.png` | Mini or Full |
| 04 | [ORB Features / ORB 特征](tutorial-04-orb-features.md) | `orb-features.png` | Full |
| 05 | [Template Matching / 模板匹配](tutorial-05-template-matching.md) | `template-match.png` | Mini or Full |
| 06 | [KNN Classification / KNN 分类](tutorial-06-knn-classification.md) | `knn-classification.png` | corrected Full |
| 07 | [Android Runtime And Native Loading / Android Runtime 与原生加载](tutorial-07-android-runtime.md) | APK `PASS` marker | Android runtime |
| 08 | [Threshold And Morphology / 阈值与形态学](tutorial-08-threshold-morphology.md) | `threshold-morphology.png` | Mini or Full |
| 09 | [Histogram Equalization / 直方图均衡化](tutorial-09-histogram-equalization.md) | `histogram-equalization.png` | Mini or Full |
| 10 | [Hough Features / Hough 特征](tutorial-10-hough-features.md) | `hough-features.png` | Full |
| 11 | [Descriptor Matching / 描述子匹配](tutorial-11-descriptor-matching.md) | `descriptor-matching.png` | Full |
| 12 | [Perspective Transform / 透视变换](tutorial-12-perspective-transform.md) | `perspective-transform.png` | Full |
| 13 | [Feature Homography / 特征单应](tutorial-13-feature-homography.md) | `feature-homography.png` | Full |
| 14 | [Background Subtraction / 背景差分](tutorial-14-background-subtraction.md) | `background-subtraction.png` | Full |
| 15 | [Sparse Optical Flow / 稀疏光流](tutorial-15-optical-flow.md) | `optical-flow.png` | Full |
| 16 | [SVM Classification / SVM 分类](tutorial-16-svm-classification.md) | `svm-classification.png` | corrected Full |
| 17 | [ONNX Inference / ONNX 推理](tutorial-17-onnx-inference.md) | `onnx-inference.png` | Full with DNN |
| 18 | [MobileNetV2 Classification / MobileNetV2 图像分类](tutorial-18-mobilenet-classification.md) | `image-classification.png` | Full with DNN |
| 19 | [NanoDet Object Detection / NanoDet 目标检测](tutorial-19-nanodet-object-detection.md) | `object-detection.png` | Full with DNN |
| 20 | [PPHumanSeg Segmentation / PPHumanSeg 人像分割](tutorial-20-pphumanseg-segmentation.md) | `human-segmentation.png` | Full with DNN |
| 21 | [Document Scanner / 文档扫描](tutorial-21-document-scanner.md) | `document-scan.png` | Full |
| 22 | [Panorama Stitching / 全景拼接](tutorial-22-panorama-stitching.md) | `panorama.png` | Full with Stitching |
| 23 | [Motion Analysis / 运动分析](tutorial-23-motion-analysis.md) | `motion-analysis.png` | Full |
| 24 | [MIL Object Tracking / MIL 目标跟踪](tutorial-24-mil-object-tracking.md) | `mil-object-tracking.png` | Full with Video |

## Run Grouped Examples / 运行分组案例

The package-backed catalog lives directly under [`samples`](https://github.com/guojin-yan/OpenCV-CSharp-API/tree/opencv5.x/samples). Choose a project from the [Example Catalog](example-catalog.md):

分组案例直接位于 [`samples`](https://github.com/guojin-yan/OpenCV-CSharp-API/tree/opencv5.x/samples)。从[案例目录](example-catalog.md)选择项目运行：

```powershell
dotnet run --project .\samples\Geometry\03.DocumentScanner\DocumentScanner.csproj -c Release
dotnet run --project .\samples\Stitching\01.PanoramaStitching\PanoramaStitching.csproj -c Release
dotnet run --project .\samples\Tracking\01.MilObjectTracking\MilObjectTracking.csproj -c Release
```

Model-backed tutorials require a one-time verified download. The [Sample Model Assets](sample-model-assets-guide.md) guide documents bundle commands, source revisions, SHA-256 verification, caching, and licenses.

模型教程需要一次经过校验的下载。[案例模型资产](sample-model-assets-guide.md)说明 bundle 命令、来源版本、SHA-256 校验、缓存和许可证。

## Chinese Text / 中文写字

The Chinese text workflow uses OpenCV `FontFace` and `putText` with a real TTF/TTC font. Pass the font path or configure `OPENCV_CSHARP_CJK_FONT`; the repository does not redistribute operating-system fonts.

中文写字案例使用 OpenCV `FontFace` 和 `putText` 读取真实 TTF/TTC 字体。可以传入字体路径或配置 `OPENCV_CSHARP_CJK_FONT`；仓库不重新分发操作系统字体。

```powershell
$env:OPENCV_CSHARP_CJK_FONT = "C:\Windows\Fonts\msyh.ttc"
dotnet run --project .\samples\ImageProcessing\02.ChinesePutText\ChinesePutText.csproj -c Release
```

## Continue By Scenario / 按场景继续

Use [Scenario Recipes](scenario-recipes.md) for product-oriented routes, then move into module guides for API depth, ownership, native boundaries, and deployment requirements. Existing guide URLs remain stable while the numbered tutorials provide the ordered path through them.

使用[场景路线](scenario-recipes.md)选择产品方向，再进入模块指南了解 API 深度、所有权、原生边界和部署要求。既有指南 URL 保持稳定，编号教程负责提供清晰的学习顺序。
