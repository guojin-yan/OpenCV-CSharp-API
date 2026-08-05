# 09 Histogram Equalization / 直方图均衡化

Histogram equalization expands the useful contrast of a grayscale image. The sample calculates the source histogram, applies `EqualizeHist`, redraws the histogram, and places both views in one report.

直方图均衡化可以扩展灰度图的有效对比度。本案例计算原始直方图，调用 `EqualizeHist`，重新绘制直方图，并把前后结果放入同一份报告。

```powershell
dotnet run --project .\samples\ImageProcessing\05.HistogramEqualization\HistogramEqualization.csproj -c Release -- .\artifacts\tutorial-09
```

This is a useful baseline before thresholding, feature extraction, or low-light inspection. The generated `histogram-equalization.png` is deterministic and needs no input file.

## Pipeline / 流程

The program keeps the source and equalized gray matrices separate, calculates 256 bins with `CalcHist`, normalizes the drawing to the panel height, and writes the image through `ImgCodecs.Cv2.ImWrite`. Keeping the histogram as data makes it straightforward to add percentile clipping or CLAHE later.

程序分别保留原始和均衡后的灰度矩阵，使用 `CalcHist` 计算 256 个 bin，将绘图归一化到面板高度，再通过 `ImgCodecs.Cv2.ImWrite` 输出。直方图保留为数据后，可以继续加入百分位裁剪或 CLAHE。
