# Plot Guide / Plot 指南

`OpenCvSharp.Plot` wraps the optional contrib `plot` module for rendering small two-dimensional plots into `Mat` images.

`OpenCvSharp.Plot` 封装可选 contrib `plot` 模块，用于把二维曲线图渲染为 `Mat` 图像。

## Scope / 范围

- Object wrapper: `Plot2d`.
- Factories: `Plot2d.Create(data)` and `PlotCv2.CreatePlot2d(data)` for Y-series input.
- XY factories: `Plot2d.Create(dataX, dataY)` and `PlotCv2.CreatePlot2d(dataX, dataY)`.
- Render helpers: `Render(Mat result)` and `Render()`.
- Plot styling setters for bounds, line width, line/grid/text visibility, colors, size, orientation, grid count, and point index text.

- 对象封装：`Plot2d`。
- 工厂：`Plot2d.Create(data)` 与 `PlotCv2.CreatePlot2d(data)` 用于 Y 序列输入。
- XY 工厂：`Plot2d.Create(dataX, dataY)` 与 `PlotCv2.CreatePlot2d(dataX, dataY)`。
- 渲染 helper：`Render(Mat result)` 与 `Render()`。
- 曲线图样式 setter 覆盖坐标范围、线宽、线/网格/文本显示、颜色、尺寸、方向、网格数量和点索引文本。

## Runtime / 运行时

`plot` is an optional OpenCV contrib module. Runtime staging should include the factual OpenCV 5.0.0 runtime artifact `opencv_plot500.dll` when the module is built. If the DLL is missing, the managed API shape remains stable and calls report `NOT_LINKED`.

`plot` 是可选 OpenCV contrib 模块。构建该模块时，runtime staging 应包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_plot500.dll`。如果缺少该 DLL，managed API 形状仍保持稳定，调用会报告 `NOT_LINKED`。

## Data Notes / 数据说明

The first batch accepts numeric `Mat` vectors. OpenCV 5.0.0 `Plot2d` expects `CV_64FC1` input vectors, and its `setPlotSize` implementation clamps small requests to at least 400 by 300 pixels. The wrapper keeps the OpenCV `cv::plot::Plot2d` object behind an opaque native handle; callers own input and output matrices.

第一批接口接受数值 `Mat` 向量。OpenCV 5.0.0 `Plot2d` 期望 `CV_64FC1` 输入向量，且其 `setPlotSize` 实现会把较小请求夹到至少 400 x 300 像素。封装层将 OpenCV `cv::plot::Plot2d` 对象保持在 opaque native handle 后面；输入与输出矩阵仍由调用方持有。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Plot;

using Mat y = new Mat(5, 1, MatType.CV_64FC1);
y.CopyFrom(new double[] { 0.0, 1.0, 0.0, 2.0, 1.5 });

using Plot2d plot = PlotCv2.CreatePlot2d(y);
plot.SetPlotSize(480, 320)
    .SetShowGrid(true)
    .SetShowText(false)
    .SetPlotLineWidth(2);

using Mat rendered = plot.Render();
```
