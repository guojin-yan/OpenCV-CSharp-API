# OpenCV Examples / OpenCV 案例

This directory is a growing catalog of complete, headless workflows. Cases are grouped by capability and numbered inside each group:

```text
samples/
|-- Common/                         Shared fixture, output, and reporting helpers
|-- ImageProcessing/               Pixels, enhancement, segmentation, drawing
|-- Features/                      Keypoints, descriptors, and matching
|-- Geometry/                      Projective geometry and homography
|-- Video/                         Temporal frames, motion masks, optical flow
|-- MachineLearning/               Classical OpenCV ML algorithms
|-- DeepLearning/                  DNN model loading and inference
|-- ConsoleSamples/                Broad API smoke and showcase runner
`-- AndroidSmoke/                  Android native loading smoke project
```

Each numbered directory is an independent executable implementation of one useful workflow. New cases should be added as `NN.ShortName`, with a local `Program.cs`, a project file, a focused output image or report, and a matching article. Do not put algorithm code in `Common`.

The package-backed cases import `SamplePackages.props`. That file pins `5.0.0-preview.1` only for reproducible validation of a known package fixture. Normal applications should install packages without a version pin and use `--prerelease` when appropriate.

## Catalog / 案例目录

### Image Processing / 图像处理

| Case | Workflow | Output | Article |
|---|---|---|---|
| `01.ImagePipeline` | grayscale, blur, Canny, statistics | `image-pipeline.png` | [Tutorial 01](../docs/articles/tutorial-01-image-pipeline.md) |
| `02.ChinesePutText` | OpenCV `FontFace` + `putText` UTF-8 Chinese | `chinese-text.png` | [Tutorial 02](../docs/articles/tutorial-02-chinese-puttext.md) |
| `03.Contours` | threshold, contour hierarchy, object annotation | `contours.png` | [Tutorial 03](../docs/articles/tutorial-03-contours.md) |
| `04.ThresholdMorphology` | Otsu threshold, opening, closing, foreground report | `threshold-morphology.png` | [Tutorial 08](../docs/articles/tutorial-08-threshold-morphology.md) |
| `05.HistogramEqualization` | histogram, equalization, before/after visualization | `histogram-equalization.png` | [Tutorial 09](../docs/articles/tutorial-09-histogram-equalization.md) |
| `06.HoughFeatures` | Hough lines, probabilistic lines, circles | `hough-features.png` | [Tutorial 10](../docs/articles/tutorial-10-hough-features.md) |

### Features / 特征

| Case | Workflow | Output | Article |
|---|---|---|---|
| `01.OrbFeatures` | ORB keypoints and binary descriptors | `orb-features.png` | [Tutorial 04](../docs/articles/tutorial-04-orb-features.md) |
| `02.TemplateMatching` | normalized template localization and confidence | `template-match.png` | [Tutorial 05](../docs/articles/tutorial-05-template-matching.md) |
| `03.DescriptorMatching` | rotated scene, ORB descriptors, cross-check matches | `descriptor-matching.png` | [Tutorial 11](../docs/articles/tutorial-11-descriptor-matching.md) |

### Geometry / 几何

| Case | Workflow | Output | Article |
|---|---|---|---|
| `01.PerspectiveTransform` | four-point projective warp and overlay | `perspective-transform.png` | [Tutorial 12](../docs/articles/tutorial-12-perspective-transform.md) |
| `02.FeatureHomography` | ORB matches, RANSAC homography, projected corners | `feature-homography.png` | [Tutorial 13](../docs/articles/tutorial-13-feature-homography.md) |

### Video / 视频与运动

| Case | Workflow | Output | Article |
|---|---|---|---|
| `01.BackgroundSubtraction` | synthetic frame sequence, MOG2, mask cleanup | `background-subtraction.png` | [Tutorial 14](../docs/articles/tutorial-14-background-subtraction.md) |
| `02.OpticalFlow` | Shi-Tomasi points and pyramidal Lucas-Kanade flow | `optical-flow.png` | [Tutorial 15](../docs/articles/tutorial-15-optical-flow.md) |

### Machine Learning / 机器学习

| Case | Workflow | Output | Article |
|---|---|---|---|
| `01.KnnClassification` | KNN decision surface and training points | `knn-classification.png` | [Tutorial 06](../docs/articles/tutorial-06-knn-classification.md) |
| `02.SvmClassification` | RBF SVM training, prediction, decision surface | `svm-classification.png` | [Tutorial 16](../docs/articles/tutorial-16-svm-classification.md) |

The `ML` native module is optional in some runtime profiles. KNN and SVM catch `NOT_LINKED`, write a diagnostic PNG, and exit successfully so the catalog remains useful on every supported runtime profile.

### Deep Learning / 深度学习

| Case | Workflow | Output | Article |
|---|---|---|---|
| `01.OnnxInference` | load an ONNX opset 13 model from bytes, create a blob, CPU forward | `onnx-inference.png` | [Tutorial 17](../docs/articles/tutorial-17-onnx-inference.md) |

The ONNX identity fixture is intentionally tiny and deterministic. It proves model loading, input binding, forward execution, and output extraction without a network download. Future model-backed cases can add detection, classification, and segmentation under this same group.

## Run A Case / 运行案例

Use the exact SDK configured by the repository when validating the fixture. The output directory is optional; every case chooses a stable default under `artifacts/`.

```powershell
$dotnet = "E:\GitSpace\OpenCV-CSharp-API-workspace\build\tools\dotnet-10.0.302\dotnet.exe"
& $dotnet run --project .\samples\ImageProcessing\04.ThresholdMorphology\ThresholdMorphology.csproj -c Release
& $dotnet run --project .\samples\Features\03.DescriptorMatching\DescriptorMatching.csproj -c Release -- .\artifacts\descriptor-matching
& $dotnet run --project .\samples\MachineLearning\02.SvmClassification\SvmClassification.csproj -c Release
& $dotnet run --project .\samples\DeepLearning\01.OnnxInference\OnnxInference.csproj -c Release
```

For the Chinese text case, pass a TTF/TTC file or set `OPENCV_CSHARP_CJK_FONT`:

```powershell
$env:OPENCV_CSHARP_CJK_FONT = "C:\Windows\Fonts\Deng.ttf"
dotnet run --project .\samples\ImageProcessing\02.ChinesePutText\ChinesePutText.csproj -c Release
```

Every process prints the fixture package version, managed namespace, native OpenCV build information, output directory, and a focused summary. No case requires a camera, desktop window, network download, or external data set.

## Adding A New Case / 新增案例

1. Choose the capability group and the next `NN.ShortName` directory.
2. Add an executable project importing `..\..\SamplePackages.props` and linking `..\..\Common\SampleSupport.cs`.
3. Keep the workflow complete: deterministic input or documented input, algorithm stages, metrics, visualization, and a summary.
4. Add the project to `OpenCV-CSharp-API.slnx`, this catalog, and one focused article in `docs/articles/`.
5. Build and run locally before changing a workflow or publishing a package.
