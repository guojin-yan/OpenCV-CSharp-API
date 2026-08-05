# Tutorial Series / 教程系列

This series is the primary learning path for the first OpenCV CSharp API release. Tutorials 01-06 map to deterministic commands in `samples/ConsoleSamples`, use generated input, run without a camera or desktop window, and write inspectable PNG files. Tutorial 07 uses `samples/AndroidSmoke` to build an APK and execute a package-owned native OpenCV call.

本系列是 OpenCV CSharp API 首版的主要学习路径。教程 01-06 对应 `samples/ConsoleSamples` 中可重复运行的命令，使用生成输入，不依赖相机或桌面窗口，并输出可检查的 PNG。教程 07 使用 `samples/AndroidSmoke` 构建 APK，在 Android 模拟器中执行 package 自带的 native OpenCV 调用。

![Six visual OpenCV CSharp API tutorial workflows](../images/showcase/showcase-overview.png)

## Learning Path / 学习路线

| # | Tutorial / 教程 | Command / 命令 | Output / 输出 | Runtime |
|---:|---|---|---|---|
| 01 | [Image Pipeline / 图像流水线](tutorial-01-image-pipeline.md) | `tutorial image` | `image-pipeline.png` | mini or full |
| 02 | [OpenCV PutText With Chinese / OpenCV 中文写字](tutorial-02-chinese-puttext.md) | `tutorial text` | `chinese-text.png` | mini or full |
| 03 | [Contours And Objects / 轮廓与目标](tutorial-03-contours.md) | `tutorial contours` | `contours.png` | mini or full |
| 04 | [ORB Features / ORB 特征](tutorial-04-orb-features.md) | `tutorial features` | `orb-features.png` | full |
| 05 | [Template Matching / 模板匹配](tutorial-05-template-matching.md) | `tutorial template` | `template-match.png` | mini or full |
| 06 | [KNN Classification / KNN 分类](tutorial-06-knn-classification.md) | `tutorial ml` | `knn-classification.png` or `NOT_LINKED` diagnostic | full when ML is linked |
| 07 | [Android Runtime And Native Loading / Android Runtime 与原生加载](tutorial-07-android-runtime.md) | `dotnet build` + `adb` | APK native `PASS` marker | Android x64/x86 mini or full |

## Run The Source-Tree Series / 运行源码系列

Select the managed package and the runtime package for the current RID as described in [Quick Start](quick-start.md). When building from this repository, point the sample at an extracted full native runtime:

按照[快速开始](quick-start.md)为当前 RID 选择 managed 包和 runtime 包。从仓库源码构建时，将示例指向已解压的 full native runtime：

```powershell
$env:OPENCV_CSHARP_CJK_FONT = "C:\path\to\a-cjk-font.ttf"
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release `
  -p:OpenCvNativeRuntimeDir=E:\path\to\full-runtime `
  -- tutorial all .\artifacts\tutorials
```

The font environment variable is optional when a known Windows, Linux, or macOS CJK system font is available. You can also pass the font file as the third argument after the output directory. The repository does not redistribute operating-system fonts.

系统中存在可识别的 Windows、Linux 或 macOS 中文字体时，字体环境变量可以省略；也可以把字体文件作为输出目录后的第三个参数传入。本仓库不重新分发操作系统字体。

`showcase` remains a compatibility alias for `tutorial`. The default command without either prefix remains the broad API smoke program.

`showcase` 继续作为 `tutorial` 的兼容别名；不带这两个前缀的默认命令仍是广覆盖 API smoke 程序。

## Run The Published-Package Cases / 运行已发布包案例

The six projects under [`PublishedPackageSamples`](https://github.com/guojin-yan/OpenCV-CSharp-API/tree/opencv5.x/samples/PublishedPackageSamples) are the package-backed implementations of Tutorials 01-06. Each project restores the managed API and matching runtime fixture, implements one complete module workflow in its own `Program.cs`, and writes one focused PNG. Run the project that matches the feature you need; the exact commands and article map are in [Published Package Samples](published-package-samples.md).

[`PublishedPackageSamples`](https://github.com/guojin-yan/OpenCV-CSharp-API/tree/opencv5.x/samples/PublishedPackageSamples) 下的 6 个项目是教程 01-06 对应的已发布包实现。每个项目恢复 managed API 和匹配的 runtime 夹具，在自己的 `Program.cs` 中实现一套完整模块流程，并输出一张聚焦 PNG。按需运行对应功能；完整命令和文章映射见[已发布包案例](published-package-samples.md)。

The root `PublishedPackageSamples` project remains an optional all-in-one gallery for release regression checks. Its ML panel records `NOT_LINKED` when the selected runtime does not include the optional native ML module; this is a supported capability result, not a failed image pipeline.

根目录 `PublishedPackageSamples` 项目仅作为发布回归时的一次性聚合图集入口。当所选 runtime 未包含可选 native ML 模块时，ML 面板记录 `NOT_LINKED`；这是受支持的能力结果，不会使整个图像流水线失败。

```powershell
dotnet run --project .\samples\PublishedPackageSamples\PublishedPackageSamples.csproj -c Release -- tutorial all .\artifacts\published-package-tutorials C:\Windows\Fonts\Deng.ttf
```

## From Tutorials To Reference Guides / 从教程进入专题指南

The numbered tutorials teach complete workflows. Use [Scenario Recipes](scenario-recipes.md) to select a product-oriented route, then use the module guides in the documentation navigation for API depth and ownership rules.

编号教程负责讲清端到端工作流；随后可以通过[场景路线](scenario-recipes.md)选择面向产品的路径，再从文档导航中的模块指南深入 API 和所有权规则。
