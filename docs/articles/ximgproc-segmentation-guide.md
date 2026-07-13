# XImgProc Segmentation Guide / XImgProc 分割指南

`OpenCvSharp.XImgProc` includes model-free segmentation and proposal wrappers from OpenCV contrib `ximgproc`.

`OpenCvSharp.XImgProc` 已包含 OpenCV contrib `ximgproc` 中不依赖模型文件的分割与候选框包装。

## Scope / 范围

- `ScanSegment`: superpixel object with `Iterate`, labels, contour mask, and `NumberOfSuperpixels`.
- `GraphSegmentation`: graph-based segmentation with `Sigma`, `K`, `MinSize`, and `ProcessImage`.
- `SelectiveSearchSegmentationStrategy`: color, size, texture, fill, and multiple strategy factories.
- `SelectiveSearchSegmentation`: base image, strategy preset switches, image/graph/strategy collections, and `Process` proposal rectangles.

- `ScanSegment`：超像素对象，包含 `Iterate`、标签、轮廓 mask 和 `NumberOfSuperpixels`。
- `GraphSegmentation`：基于图的分割，包含 `Sigma`、`K`、`MinSize` 和 `ProcessImage`。
- `SelectiveSearchSegmentationStrategy`：颜色、大小、纹理、填充度和 multiple strategy 工厂。
- `SelectiveSearchSegmentation`：base image、策略预设切换、image/graph/strategy 集合，以及 `Process` 候选矩形。

## Ownership / 所有权

Strategy and graph segmentation handles are owned by their managed wrappers. Adding a strategy or graph segmentation to Selective Search passes the native `cv::Ptr` by value; it does not dispose or steal the managed wrapper.

strategy 和 graph segmentation 句柄由各自 managed wrapper 持有。向 Selective Search 添加 strategy 或 graph segmentation 时，native 层按值传递 `cv::Ptr`；不会释放或夺取 managed wrapper 所有权。

## Input Notes / 输入说明

`ScanSegment` expects an image matching the creation size and documented by OpenCV as Lab `CV_8UC3`. `GraphSegmentation` writes a label matrix with the same rows and columns as the input. Selective Search can validly return zero proposals for tiny synthetic inputs.

`ScanSegment` 要求输入图像尺寸与创建参数匹配，OpenCV 文档要求为 Lab `CV_8UC3`。`GraphSegmentation` 输出与输入同尺寸的标签矩阵。Selective Search 对 tiny 合成输入合法地可能返回零个 proposal。

## Example / 示例

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.XImgProc;

namespace XImgProcSegmentationExample
{
    internal static class Program
    {
        private static void Main()
        {
            using Mat image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(24, 48, 72));

            using ScanSegment scan = XImgProcCv2.CreateScanSegment(image.Cols, image.Rows, 16);
            scan.Iterate(image);
            using Mat scanLabels = scan.GetLabels();

            using GraphSegmentation graph = XImgProcCv2.CreateGraphSegmentation(0.5, 50.0F, 4);
            using Mat graphLabels = graph.ProcessImage(image);

            using SelectiveSearchSegmentation search = XImgProcCv2.CreateSelectiveSearchSegmentation();
            search.SetBaseImage(image);
            search.SwitchToSingleStrategy(k: 50, sigma: 0.8F);
            Rect[] proposals = search.Process();
        }
    }
}
```

## Smoke / Smoke

Linked smoke uses small synthetic images and checks output shape, object lifetime, and proposal marshalling. It does not measure segmentation quality.

linked smoke 使用小型合成图像，检查输出形状、对象生命周期和 proposal marshalling，不衡量分割质量。
