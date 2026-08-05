# Example Catalog / 案例目录

The executable examples live directly under [`samples/`](https://github.com/guojin-yan/OpenCV-CSharp-API/tree/opencv5.x/samples), grouped by capability. The layout is deliberately open-ended: add the next numbered directory to the appropriate group and give it a focused article.

可执行案例直接位于 [`samples/`](https://github.com/guojin-yan/OpenCV-CSharp-API/tree/opencv5.x/samples) 下，并按能力分组。目录结构可以持续扩展：在合适分组中增加下一个编号目录，并为它配套一篇聚焦文章。

## Groups / 分组

| Group | Cases | Scope |
|---|---:|---|
| `ImageProcessing` | 6 | image pipelines, Chinese text, contours, morphology, histograms, Hough |
| `Features` | 3 | ORB, template localization, descriptor matching |
| `Geometry` | 2 | perspective transforms and feature homography |
| `Video` | 2 | background subtraction and sparse optical flow |
| `MachineLearning` | 2 | KNN and RBF SVM decision surfaces |
| `DeepLearning` | 1 | deterministic ONNX loading and CPU inference |

Each case restores `JYPPX.OpenCV.CSharp.API` plus the matching Windows x64 runtime from `samples/SamplePackages.props`. The exact `5.0.0-preview.1` pin exists only so release evidence can be reproduced; consumer commands use version-neutral installation instructions.

每个案例都通过 `samples/SamplePackages.props` 恢复 `JYPPX.OpenCV.CSharp.API` 和匹配的 Windows x64 runtime。精确的 `5.0.0-preview.1` 只用于复现发布验证证据；普通用户命令使用版本中立的安装方式。

## Run / 运行

```powershell
dotnet run --project .\samples\ImageProcessing\01.ImagePipeline\ImagePipeline.csproj -c Release
dotnet run --project .\samples\Features\03.DescriptorMatching\DescriptorMatching.csproj -c Release
dotnet run --project .\samples\Geometry\02.FeatureHomography\FeatureHomography.csproj -c Release
dotnet run --project .\samples\Video\01.BackgroundSubtraction\BackgroundSubtraction.csproj -c Release
dotnet run --project .\samples\MachineLearning\02.SvmClassification\SvmClassification.csproj -c Release
dotnet run --project .\samples\DeepLearning\01.OnnxInference\OnnxInference.csproj -c Release
```

The Chinese text case additionally needs a CJK font path. KNN and SVM report `NOT_LINKED` when the selected runtime profile does not contain OpenCV's optional ML native module; this is an explicit capability result. The ONNX case uses the checked-in identity fixture and does not download a model.

中文写字案例还需要 CJK 字体路径。当所选 runtime profile 未包含 OpenCV 可选 ML native 模块时，KNN 和 SVM 会输出 `NOT_LINKED` 能力诊断，而不是静默失败。ONNX 案例使用仓库内的 identity 夹具，不下载模型。

## Articles / 配套文章

The catalog in [`samples/README.md`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/README.md) is the authoritative case-to-article map. Tutorials 01-07 cover the original pipeline, text, contours, features, template matching, KNN, and Android loading. Tutorials 08-17 cover the expanded image-processing, feature, geometry, video, SVM, and ONNX workflows.

[`samples/README.md`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/README.md) 是案例与文章的权威映射。教程 01-07 覆盖原有流水线、中文写字、轮廓、特征、模板匹配、KNN 和 Android 加载；教程 08-17 覆盖新增的图像处理、特征、几何、视频、SVM 和 ONNX 工作流。
