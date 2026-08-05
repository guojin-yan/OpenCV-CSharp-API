# Published Package Samples / 已发布包案例

This page is the release-post validation path for `5.0.0-preview.1`. It runs the same six headless image workflows as the source-tree sample, but restores both dependencies from the public NuGet feed:

本页是 `5.0.0-preview.1` 的发布后验证路线。它与源码示例运行相同的六个无头图像工作流，但 managed API 和 native runtime 都从公开 NuGet 源恢复：

- `JYPPX.OpenCV.CSharp.API`
- `JYPPX.OpenCV.runtime.win-x64`

The independent projects are under [`samples/PublishedPackageSamples`](https://github.com/guojin-yan/OpenCV-CSharp-API/tree/opencv5.x/samples/PublishedPackageSamples). Each project owns one module workflow and one focused output. Their exact version pin is deliberate: it makes a regression report reproducible against the package that was published. Consumer installation instructions remain version-neutral and use `--prerelease`.

独立项目位于 [`samples/PublishedPackageSamples`](https://github.com/guojin-yan/OpenCV-CSharp-API/tree/opencv5.x/samples/PublishedPackageSamples) 下。每个项目负责一个模块工作流和一份聚焦输出。项目中的精确版本号是有意保留的，用于让回归报告能够复现到已发布包；普通用户安装仍使用不写死版本的 `--prerelease` 命令。

| Case / 案例 | Project / 项目 | Module / 模块 | Article / 文章 |
|---|---|---|---|
| 01 Image Pipeline / 图像流水线 | `Case01.ImagePipeline` | Core, ImgProc, ImgCodecs | [Tutorial 01](tutorial-01-image-pipeline.md) |
| 02 Chinese PutText / 中文写字 | `Case02.ChinesePutText` | ImgProc `FontFace` and `putText` | [Tutorial 02](tutorial-02-chinese-puttext.md) |
| 03 Contours / 轮廓 | `Case03.Contours` | ImgProc contours | [Tutorial 03](tutorial-03-contours.md) |
| 04 ORB Features / ORB 特征 | `Case04.OrbFeatures` | Features2D ORB | [Tutorial 04](tutorial-04-orb-features.md) |
| 05 Template Matching / 模板匹配 | `Case05.TemplateMatching` | ImgProc response map | [Tutorial 05](tutorial-05-template-matching.md) |
| 06 KNN / KNN 分类 | `Case06.KnnClassification` | ML KNN, optional | [Tutorial 06](tutorial-06-knn-classification.md) |

## Run Individual Cases / 运行独立案例

The text case needs a font containing the Chinese glyphs. OpenCV's `FontFace` and `putText` perform the rendering; the sample does not rasterize text through a UI toolkit.

中文案例需要包含中文字形的字体。渲染由 OpenCV 的 `FontFace` 和 `putText` 完成，示例不经过 UI 工具包栅格化文字。

```powershell
$env:OPENCV_CSHARP_CJK_FONT = "C:\Windows\Fonts\Deng.ttf"
dotnet run --project .\samples\PublishedPackageSamples\Case01.ImagePipeline\Case01.ImagePipeline.csproj -c Release -- .\artifacts\published-package-image
dotnet run --project .\samples\PublishedPackageSamples\Case02.ChinesePutText\Case02.ChinesePutText.csproj -c Release -- .\artifacts\published-package-text C:\Windows\Fonts\Deng.ttf
dotnet run --project .\samples\PublishedPackageSamples\Case03.Contours\Case03.Contours.csproj -c Release -- .\artifacts\published-package-contours
dotnet run --project .\samples\PublishedPackageSamples\Case04.OrbFeatures\Case04.OrbFeatures.csproj -c Release -- .\artifacts\published-package-features
dotnet run --project .\samples\PublishedPackageSamples\Case05.TemplateMatching\Case05.TemplateMatching.csproj -c Release -- .\artifacts\published-package-template
dotnet run --project .\samples\PublishedPackageSamples\Case06.KnnClassification\Case06.KnnClassification.csproj -c Release -- .\artifacts\published-package-ml
```

The text case needs a font containing the Chinese glyphs. The other cases need no camera, model, network download, or desktop window. Each process prints the fixture package version and native OpenCV version before its focused summary.

中文案例需要包含中文字形的字体；其他案例不需要相机、模型、网络下载或桌面窗口。每个进程都会在聚焦摘要前打印验证夹具包版本和 native OpenCV 版本。

The root `PublishedPackageSamples` project remains available for a one-command aggregate gallery when a release regression needs all outputs at once:

发布回归需要一次生成全部图像时，根目录 `PublishedPackageSamples` 项目仍可作为聚合入口：

```powershell
dotnet run --project .\samples\PublishedPackageSamples\PublishedPackageSamples.csproj -c Release -- tutorial all .\artifacts\published-package-tutorials C:\Windows\Fonts\Deng.ttf
```

The aggregate output directory contains:

| File | Workflow |
|---|---|
| `source.png` | deterministic BGR input |
| `image-pipeline.png` | grayscale, Gaussian blur, and Canny edges |
| `chinese-text.png` | UTF-8 Chinese rendered by OpenCV `putText` |
| `contours.png` | threshold and external contour drawing |
| `orb-features.png` | ORB keypoints and descriptors |
| `template-match.png` | normalized template localization |
| `knn-classification.png` | batch KNN decision surface or `NOT_LINKED` diagnostic |
| `showcase-overview.png` | six-panel composite |

输出目录包含：

| 文件 | 工作流 |
|---|---|
| `source.png` | 确定性的 BGR 输入 |
| `image-pipeline.png` | 灰度、Gaussian blur 和 Canny 边缘 |
| `chinese-text.png` | OpenCV `putText` 绘制 UTF-8 中文 |
| `contours.png` | 阈值和外轮廓绘制 |
| `orb-features.png` | ORB keypoint 与 descriptor |
| `template-match.png` | 归一化模板定位 |
| `knn-classification.png` | 批量 KNN 决策面或 `NOT_LINKED` 诊断图 |
| `showcase-overview.png` | 六宫格总览 |

## From Package To Product / 从包验证到产品代码

After this validation succeeds, choose the runtime package for the target RID and keep its version equal to the managed package. Use [Quick Start](quick-start.md) for installation, then follow the numbered [Tutorial Series](tutorial-series.md) and [Scenario Recipes](scenario-recipes.md) for image services, visual search, ML, DNN, video, and Android loading.

验证通过后，请为目标 RID 选择对应 runtime 包，并保持其版本与 managed 包一致。安装步骤见[快速开始](quick-start.md)，然后按编号[系列教程](tutorial-series.md)和[场景路线](scenario-recipes.md)继续学习图像服务、视觉搜索、ML、DNN、视频和 Android 加载。
