# OpenCV Examples / OpenCV 案例

This directory is a growing catalog of complete, headless workflows. Examples are grouped by capability and numbered inside each group, so new workflows can be added without reorganizing existing paths.

本目录是一套可持续扩展的完整案例库。案例按功能分组，并在组内编号；后续可以持续增加新案例，而不需要改变已有路径。

```text
samples/
|-- Common/                         Shared output, fixture, and asset helpers
|-- ImageProcessing/                Pixels, enhancement, contours, Chinese text
|-- Features/                       Keypoints, descriptors, and matching
|-- Geometry/                       Projective geometry, homography, scanning
|-- Video/                          Motion masks, optical flow, motion analysis
|-- Tracking/                       Stateful object tracking
|-- Stitching/                      Multi-image composition and panorama
|-- MachineLearning/                Classical OpenCV ML algorithms
|-- DeepLearning/                   ONNX classification, detection, segmentation
|-- ConsoleSamples/                 Broad API smoke and showcase runner
`-- AndroidSmoke/                   Android native loading smoke project
```

Each numbered directory is an independent executable workflow with deterministic or documented input, algorithm stages, metrics, visualization, and a focused article. Shared helpers stay in `Common`; workflow-specific algorithm code stays in its own case.

每个编号目录都是可独立运行的完整工作流，包含确定性或明确说明的输入、算法阶段、指标、可视化结果和对应文章。公共辅助代码放在 `Common`，具体算法保留在各自案例中。

## Catalog / 案例目录

### Image Processing / 图像处理

| Case | Workflow / 工作流 | Output | Article |
|---|---|---|---|
| `01.ImagePipeline` | grayscale, blur, Canny, statistics / 灰度、模糊、边缘与统计 | `image-pipeline.png` | [Tutorial 01](../docs/articles/tutorial-01-image-pipeline.md) |
| `02.ChinesePutText` | OpenCV `FontFace` + UTF-8 Chinese / OpenCV 中文写字 | `chinese-text.png` | [Tutorial 02](../docs/articles/tutorial-02-chinese-puttext.md) |
| `03.Contours` | threshold, contour hierarchy, annotation / 阈值、轮廓层级与标注 | `contours.png` | [Tutorial 03](../docs/articles/tutorial-03-contours.md) |
| `04.ThresholdMorphology` | Otsu, opening, closing / Otsu 与形态学 | `threshold-morphology.png` | [Tutorial 08](../docs/articles/tutorial-08-threshold-morphology.md) |
| `05.HistogramEqualization` | histogram and equalization / 直方图均衡化 | `histogram-equalization.png` | [Tutorial 09](../docs/articles/tutorial-09-histogram-equalization.md) |
| `06.HoughFeatures` | lines and circles / Hough 直线与圆 | `hough-features.png` | [Tutorial 10](../docs/articles/tutorial-10-hough-features.md) |

### Features / 特征

| Case | Workflow / 工作流 | Output | Article |
|---|---|---|---|
| `01.OrbFeatures` | ORB keypoints and descriptors / ORB 关键点与描述子 | `orb-features.png` | [Tutorial 04](../docs/articles/tutorial-04-orb-features.md) |
| `02.TemplateMatching` | normalized template localization / 归一化模板定位 | `template-match.png` | [Tutorial 05](../docs/articles/tutorial-05-template-matching.md) |
| `03.DescriptorMatching` | ORB cross-check matching / ORB 交叉匹配 | `descriptor-matching.png` | [Tutorial 11](../docs/articles/tutorial-11-descriptor-matching.md) |

### Geometry / 几何

| Case | Workflow / 工作流 | Output | Article |
|---|---|---|---|
| `01.PerspectiveTransform` | four-point projective warp / 四点透视变换 | `perspective-transform.png` | [Tutorial 12](../docs/articles/tutorial-12-perspective-transform.md) |
| `02.FeatureHomography` | ORB, RANSAC, projected corners / 特征单应与 RANSAC | `feature-homography.png` | [Tutorial 13](../docs/articles/tutorial-13-feature-homography.md) |
| `03.DocumentScanner` | page detection, corner ordering, rectification / 文档检测与矫正 | `document-scan.png` | [Tutorial 21](../docs/articles/tutorial-21-document-scanner.md) |

### Video / 视频与运动

| Case | Workflow / 工作流 | Output | Article |
|---|---|---|---|
| `01.BackgroundSubtraction` | MOG2 and mask cleanup / MOG2 背景建模与掩码清理 | `background-subtraction.png` | [Tutorial 14](../docs/articles/tutorial-14-background-subtraction.md) |
| `02.OpticalFlow` | Shi-Tomasi and pyramidal LK / 角点与金字塔 LK 光流 | `optical-flow.png` | [Tutorial 15](../docs/articles/tutorial-15-optical-flow.md) |
| `03.MotionAnalysis` | foreground filtering and trajectory / 运动前景筛选与轨迹 | `motion-analysis.png` | [Tutorial 23](../docs/articles/tutorial-23-motion-analysis.md) |

### Tracking / 目标跟踪

| Case | Workflow / 工作流 | Output | Article |
|---|---|---|---|
| `01.MilObjectTracking` | MIL sequence tracking and measured error / MIL 序列跟踪与误差测量 | `mil-object-tracking.png` | [Tutorial 24](../docs/articles/tutorial-24-mil-object-tracking.md) |

### Stitching / 图像拼接

| Case | Workflow / 工作流 | Output | Article |
|---|---|---|---|
| `01.PanoramaStitching` | three-view alignment and blending / 三视图配准与融合 | `panorama.png` | [Tutorial 22](../docs/articles/tutorial-22-panorama-stitching.md) |

### Machine Learning / 机器学习

| Case | Workflow / 工作流 | Output | Article |
|---|---|---|---|
| `01.KnnClassification` | KNN decision surface / KNN 决策面 | `knn-classification.png` | [Tutorial 06](../docs/articles/tutorial-06-knn-classification.md) |
| `02.SvmClassification` | RBF SVM training and prediction / RBF SVM 训练与预测 | `svm-classification.png` | [Tutorial 16](../docs/articles/tutorial-16-svm-classification.md) |

The Full runtime requires the `ML` native module; Mini deliberately excludes it. KNN and SVM keep an explicit `NOT_LINKED` diagnostic path for Mini and older packages published before the corrected Full/ML boundary.

Full runtime 必须包含原生 `ML` 模块，Mini 刻意不包含。KNN 和 SVM 在 Mini 或修正 Full/ML 边界之前的旧包上会明确输出 `NOT_LINKED`，不会静默失败。

### Deep Learning / 深度学习

| Case | Workflow / 工作流 | Output | Article |
|---|---|---|---|
| `01.OnnxInference` | deterministic in-memory ONNX forward / 内存 ONNX 确定性推理 | `onnx-inference.png` | [Tutorial 17](../docs/articles/tutorial-17-onnx-inference.md) |
| `02.ImageClassification` | MobileNetV2 ImageNet top-5 / MobileNetV2 图像分类 | `image-classification.png` | [Tutorial 18](../docs/articles/tutorial-18-mobilenet-classification.md) |
| `03.ObjectDetection` | NanoDet decode and NMS / NanoDet 解码与 NMS | `object-detection.png` | [Tutorial 19](../docs/articles/tutorial-19-nanodet-object-detection.md) |
| `04.HumanSegmentation` | PPHumanSeg mask and composition / PPHumanSeg 人像掩码与合成 | `human-segmentation.png` | [Tutorial 20](../docs/articles/tutorial-20-pphumanseg-segmentation.md) |

`01.OnnxInference` uses a tiny checked-in identity graph. Cases 02-04 use models pinned by immutable source revisions, byte lengths, SHA-256, and licenses. See [Sample Model Assets](../docs/articles/sample-model-assets-guide.md).

`01.OnnxInference` 使用仓库内的小型 identity 模型。02-04 的模型均固定不可变来源版本、文件长度、SHA-256 和许可证，详见[案例模型资产](../docs/articles/sample-model-assets-guide.md)。

## Run / 运行

Use the repository SDK or any compatible .NET 10 SDK. The output directory is optional; every example chooses a stable default below `artifacts/`.

使用仓库配置的 SDK 或兼容的 .NET 10 SDK。输出目录可以省略，案例会在 `artifacts/` 下选择稳定的默认目录。

```powershell
dotnet run --project .\samples\Geometry\03.DocumentScanner\DocumentScanner.csproj -c Release
dotnet run --project .\samples\Stitching\01.PanoramaStitching\PanoramaStitching.csproj -c Release
dotnet run --project .\samples\Video\03.MotionAnalysis\MotionAnalysis.csproj -c Release
dotnet run --project .\samples\Tracking\01.MilObjectTracking\MilObjectTracking.csproj -c Release
```

Download a DNN bundle once before running its example:

```powershell
pwsh .\scripts\Get-SampleModelAssets.ps1 -Bundle classification-mobilenet-v2
dotnet run --project .\samples\DeepLearning\02.ImageClassification\ImageClassification.csproj -c Release
```

For Chinese text, pass a TTF/TTC path or set `OPENCV_CSHARP_CJK_FONT`:

```powershell
$env:OPENCV_CSHARP_CJK_FONT = "C:\Windows\Fonts\msyh.ttc"
dotnet run --project .\samples\ImageProcessing\02.ChinesePutText\ChinesePutText.csproj -c Release
```

All processes print the managed package fixture, namespace, native build information, output directory, and focused metrics. Only model-backed DNN cases need a one-time verified asset download; the other grouped examples are fully offline and need no camera or desktop window.

所有程序都会输出托管包测试版本、命名空间、原生构建信息、输出目录和关键指标。只有模型 DNN 案例需要一次经过校验的资产下载；其他分组案例完全离线运行，不需要摄像头或桌面窗口。

## Package Fixture / 包回归基线

Grouped examples import `SamplePackages.props`. Its exact package pin exists only to reproduce post-publication regression evidence. Consumer installation commands in the repository remain version-neutral, so a new package release does not require editing tutorial commands.

分组案例通过 `SamplePackages.props` 引入包。该文件的精确版本仅用于复现发布后回归证据。仓库中的用户安装命令保持版本中立，发布新版本后无需修改教程命令。

## Add A Case / 新增案例

1. Select the capability group and next `NN.ShortName` directory. / 选择功能分组和下一个编号目录。
2. Add an executable project importing `..\..\SamplePackages.props` and linking `SampleSupport.cs`. / 创建可执行项目并引用公共辅助代码。
3. Implement a complete workflow with input, stages, metrics, visualization, and failure checks. / 实现包含输入、阶段、指标、可视化和失败检查的完整流程。
4. Add the project to `OpenCV-CSharp-API.slnx`, this catalog, and one focused bilingual article. / 更新解决方案、目录和一篇独立双语文章。
5. Build and run locally before changing a workflow or publishing. / 修改 Action 或发布之前先完成本地构建与运行。
