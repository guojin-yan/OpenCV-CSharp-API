# Example Catalog / 案例目录

The executable examples live directly under [`samples/`](https://github.com/guojin-yan/OpenCV-CSharp-API/tree/opencv5.x/samples). They are grouped by capability and numbered inside each group. The structure is intentionally open-ended: add a numbered directory and a focused bilingual article without moving established examples.

可执行案例直接位于 [`samples/`](https://github.com/guojin-yan/OpenCV-CSharp-API/tree/opencv5.x/samples)，按功能分组并在组内编号。目录结构支持持续扩展：增加编号目录和独立双语文章即可，不需要移动已有案例。

## Groups / 分组

| Group | Cases | Scope / 范围 |
|---|---:|---|
| `ImageProcessing` | 6 | pipelines, Chinese text, contours, morphology, histograms, Hough / 图像流水线、中文写字、轮廓、形态学、直方图、Hough |
| `Features` | 3 | ORB, template localization, descriptor matching / ORB、模板定位、描述子匹配 |
| `Geometry` | 3 | perspective transforms, homography, document scanning / 透视变换、单应、文档扫描 |
| `Video` | 3 | background subtraction, optical flow, motion trajectory / 背景建模、光流、运动轨迹 |
| `Tracking` | 1 | measured MIL single-object tracking / 可量化的 MIL 单目标跟踪 |
| `Stitching` | 1 | three-view feature panorama / 三视图特征全景拼接 |
| `MachineLearning` | 2 | KNN and RBF SVM decision surfaces / KNN 与 RBF SVM 决策面 |
| `DeepLearning` | 4 | ONNX boundary, classification, detection, segmentation / ONNX 边界、分类、检测、分割 |

The 23 grouped examples are independent executables. Each one owns its workflow code, emits inspectable output, prints metrics, and has a direct article mapping in the [samples README](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/README.md).

23 个分组案例均可独立执行。每个案例保留自己的工作流代码，生成可检查结果，输出量化指标，并在 [samples README](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/README.md) 中映射到对应文章。

## Representative Workflows / 代表性工作流

```powershell
dotnet run --project .\samples\ImageProcessing\02.ChinesePutText\ChinesePutText.csproj -c Release
dotnet run --project .\samples\Geometry\03.DocumentScanner\DocumentScanner.csproj -c Release
dotnet run --project .\samples\Video\03.MotionAnalysis\MotionAnalysis.csproj -c Release
dotnet run --project .\samples\Tracking\01.MilObjectTracking\MilObjectTracking.csproj -c Release
dotnet run --project .\samples\Stitching\01.PanoramaStitching\PanoramaStitching.csproj -c Release
```

For DNN examples, first fetch the required verified bundle:

```powershell
pwsh .\scripts\Get-SampleModelAssets.ps1 -Bundle detection-nanodet
dotnet run --project .\samples\DeepLearning\03.ObjectDetection\ObjectDetection.csproj -c Release
```

DNN assets are immutable, hash-checked, license-declared, and excluded from Git. See [Sample Model Assets](sample-model-assets-guide.md).

DNN 资产固定不可变版本，校验哈希，声明许可证，并排除在 Git 之外，详见[案例模型资产](sample-model-assets-guide.md)。

## Runtime Requirements / Runtime 要求

| Capability | Runtime |
|---|---|
| Core image processing and Chinese text | Mini or Full / Mini 或 Full |
| Features, geometry, video | Full recommended / 推荐 Full |
| ML | Corrected Full runtime; Mini excludes ML / 修正后的 Full，Mini 不含 ML |
| DNN classification, detection, segmentation | Full with DNN / 包含 DNN 的 Full |
| Panorama stitching | Full with Stitching / 包含 Stitching 的 Full |
| MIL example | Main Video module in Full; contrib Tracking is not required / Full 主 Video 模块，不要求 contrib Tracking |

The grouped projects import `samples/SamplePackages.props` to reproduce a known published package fixture. Normal consumer installation and tutorial commands remain version-neutral.

分组项目通过 `samples/SamplePackages.props` 复现已知发布包基线。普通用户安装命令和教程命令保持版本中立。

## Article Map / 文章映射

Tutorials 01-17 cover the original image, feature, geometry, video, ML, Android, and ONNX workflows. Tutorials 18-24 add real model classification/detection/segmentation, document scanning, panorama stitching, motion analysis, and quantitative object tracking.

教程 01-17 覆盖原有图像、特征、几何、视频、机器学习、Android 和 ONNX 工作流。教程 18-24 新增真实模型分类/检测/分割、文档扫描、全景拼接、运动分析和量化目标跟踪。

Use the [Tutorial Series](tutorial-series.md) as the ordered learning path and the [samples README](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/README.md) as the authoritative case-to-article table.
