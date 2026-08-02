# Visual Showcase / 可视化案例

The release showcase is a compact executable path inside `samples/ConsoleSamples`. It uses only generated input data, runs real native OpenCV operations, and writes inspection-ready PNG output. No camera, model, network download, or desktop window is required.

首版 showcase 是 `samples/ConsoleSamples` 中的紧凑可执行入口。它只使用生成的输入数据，执行真实 native OpenCV 运算，并输出可直接查看的 PNG；不需要相机、模型、网络下载或桌面窗口。

![Four OpenCV CSharp API showcase workflows](../images/showcase/showcase-overview.png)

## Run All Cases / 运行全部案例

Build against a factual full runtime directory and pass `showcase all` after `--`:

使用真实 full runtime 目录构建，并在 `--` 后传入 `showcase all`：

```powershell
C:\Users\guoji\.dotnet\dotnet.exe run `
  --project .\samples\ConsoleSamples\ConsoleSamples.csproj `
  -c Release `
  -p:OpenCvNativeRuntimeDir=E:\path\to\full-runtime `
  -- showcase all .\artifacts\showcase
```

The command writes:

命令会生成：

- `source.png`: synthetic BGR source image with lines, shapes, and text.
- `image-pipeline.png`: BGR to grayscale, Gaussian blur, and Canny edge pipeline.
- `orb-features.png`: ORB detection and rich keypoint rendering.
- `template-match.png`: normalized template response and best-match localization.
- `knn-classification.png`: batch K-nearest classification over 3,600 queries.
- `showcase-overview.png`: a 2x2 composition built with `HConcat` and `VConcat`.

- `source.png`：包含线条、形状与文字的合成 BGR 输入图。
- `image-pipeline.png`：BGR 转灰度、Gaussian blur 和 Canny 边缘流水线。
- `orb-features.png`：ORB 检测与 rich keypoint 绘制。
- `template-match.png`：归一化模板响应和最佳匹配定位。
- `knn-classification.png`：对 3,600 个 query 批量执行 K 近邻分类。
- `showcase-overview.png`：使用 `HConcat` 与 `VConcat` 合成的 2x2 总览图。

## Run One Case / 运行单个案例

Replace `all` with `image`, `features`, `template`, or `ml` to run one focused path:

将 `all` 替换为 `image`、`features`、`template` 或 `ml`，即可只运行一个专题：

```powershell
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release `
  -p:OpenCvNativeRuntimeDir=E:\path\to\full-runtime `
  -- showcase features .\artifacts\features
```

The source is intentionally split into a dedicated [`ShowcaseRunner.cs`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/ConsoleSamples/ShowcaseRunner.cs), while the original [`Program.cs`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/ConsoleSamples/Program.cs) remains the broad module smoke program.

源码有意拆分到独立的 [`ShowcaseRunner.cs`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/ConsoleSamples/ShowcaseRunner.cs)，原有 [`Program.cs`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/ConsoleSamples/Program.cs) 继续承担广覆盖模块 smoke。

## What Each Case Proves / 每个案例证明什么

### Image pipeline / 图像流水线

The pipeline proves matrix allocation, color conversion, filtering, edge detection, channel merge, nonzero statistics, PNG encoding, and deterministic disposal in one short workflow. It is a useful starting point for OCR preprocessing, inspection, segmentation, and thumbnail services.

该流水线在一个简短工作流中验证矩阵分配、颜色转换、滤波、边缘检测、通道合并、非零统计、PNG 编码和确定性释放，适合作为 OCR 预处理、质检、分割和缩略图服务的起点。

### ORB features / ORB 特征

The feature case detects ORB keypoints, produces binary descriptors, and renders scale/orientation-aware markers. Applications can extend it with `BFMatcher`, homography estimation, or the stitching feature pipeline.

特征案例检测 ORB keypoint、生成二进制 descriptor，并绘制包含尺度/方向信息的标记。应用可以继续组合 `BFMatcher`、homography estimation 或 stitching feature pipeline。

### Template localization / 模板定位

The template case uses `CCoeffNormed`, reads the best location with `MinMaxLoc`, and draws the result. It demonstrates a complete result-map workflow without external model files.

模板案例使用 `CCoeffNormed`，通过 `MinMaxLoc` 读取最佳位置并绘制结果，在不依赖外部模型的情况下展示完整 response-map 工作流。

### KNN classification / KNN 分类

The ML case trains a `KNearest` classifier, classifies a batch matrix of 3,600 samples in one native call, converts the result matrix to managed data, and renders the decision surface. It demonstrates that classical OpenCV ML can be composed with the image API rather than living in an isolated console-only example.

ML 案例训练 `KNearest` 分类器，在一次 native 调用中批量分类 3,600 个样本，将结果矩阵转回 managed 数据，并绘制 decision surface。这说明 OpenCV 传统机器学习可以直接与图像 API 组合，而不是停留在孤立的控制台输出中。

## Headless And CI Friendly / 适合无头与 CI 环境

The showcase never calls HighGui. Every output is file based, so the same command works in a local terminal, a server process, and a headless CI worker as long as the selected runtime contains the required full modules.

showcase 不调用 HighGui，所有结果都写入文件。因此只要所选 runtime 包含所需 full 模块，同一命令即可在本地终端、服务器进程和无头 CI worker 中运行。
