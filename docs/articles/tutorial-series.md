# Tutorial Series / 系列教程

This series is the primary learning path for the first OpenCV CSharp API release. Every article maps to a deterministic command in `samples/ConsoleSamples`, uses generated input, runs without a camera or desktop window, and writes a PNG that can be inspected in local or CI environments.

本系列是 OpenCV CSharp API 首版的主要学习路径。每篇文章都对应 `samples/ConsoleSamples` 中一个可重复执行的命令，使用生成输入，不依赖相机或桌面窗口，并输出可在本地或 CI 中检查的 PNG。

![Six OpenCV CSharp API tutorial workflows](../images/showcase/showcase-overview.png)

## Learning Path / 学习路线

| # | Tutorial / 教程 | Command / 命令 | Output / 输出 | Runtime |
|---:|---|---|---|---|
| 01 | [Image Pipeline / 图像流水线](tutorial-01-image-pipeline.md) | `tutorial image` | `image-pipeline.png` | mini or full |
| 02 | [OpenCV PutText With Chinese / OpenCV 中文写字](tutorial-02-chinese-puttext.md) | `tutorial text` | `chinese-text.png` | mini or full |
| 03 | [Contours And Objects / 轮廓与目标](tutorial-03-contours.md) | `tutorial contours` | `contours.png` | mini or full |
| 04 | [ORB Features / ORB 特征](tutorial-04-orb-features.md) | `tutorial features` | `orb-features.png` | full |
| 05 | [Template Matching / 模板匹配](tutorial-05-template-matching.md) | `tutorial template` | `template-match.png` | mini or full |
| 06 | [KNN Classification / KNN 分类](tutorial-06-knn-classification.md) | `tutorial ml` | `knn-classification.png` | full |

## Run The Series / 运行全系列

Select the managed package and the runtime package for the current RID as described in [Quick Start](quick-start.md). When building from this repository, point the sample at an extracted full native runtime:

按照 [Quick Start](quick-start.md) 选择 managed 包和当前 RID 对应的 runtime 包。从仓库源码构建时，将示例指向已解压的 full native runtime：

```powershell
$env:OPENCV_CSHARP_CJK_FONT = "C:\path\to\a-cjk-font.ttf"
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release `
  -p:OpenCvNativeRuntimeDir=E:\path\to\full-runtime `
  -- tutorial all .\artifacts\tutorials
```

The font environment variable is optional when a known Windows, Linux, or macOS CJK system font is available. You can also pass the font file as the third argument after the output directory. The repository does not redistribute operating-system fonts.

当系统中存在可识别的 Windows、Linux 或 macOS 中文字体时，字体环境变量可以省略；也可以把字体文件作为输出目录后的第三个参数传入。本仓库不重新分发操作系统字体。

`showcase` remains a compatibility alias for `tutorial`. The default command without either prefix remains the broad API smoke program.

`showcase` 继续作为 `tutorial` 的兼容别名；不带这两个前缀的默认命令仍是广覆盖 API smoke 程序。

## From Tutorials To Reference Guides / 从教程进入专题指南

The numbered tutorials teach complete workflows. Use [Scenario Recipes](scenario-recipes.md) to select a product-oriented route, then use the module guides in the documentation navigation for API depth and ownership rules.

编号教程负责讲清端到端流程；随后可通过 [Scenario Recipes](scenario-recipes.md) 选择面向产品场景的路线，再从文档导航中的模块指南深入了解 API、内存所有权和边界规则。
