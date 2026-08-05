# 08 Threshold And Morphology / 阈值与形态学

This workflow converts a deterministic grayscale scene into an Otsu binary mask, then removes isolated noise with opening and closes small gaps with closing. It is a practical pre-processing stage for inspection and OCR.

本案例将确定性灰度场景转换为 Otsu 二值掩码，再用开运算去除孤立噪声、用闭运算连接小间隙，适合质检和 OCR 的前处理。

```powershell
dotnet run --project .\samples\ImageProcessing\04.ThresholdMorphology\ThresholdMorphology.csproj -c Release -- .\artifacts\tutorial-08
```

The implementation uses `Threshold`, `GetStructuringElement`, `MorphologyEx`, and `CountNonZero`. It writes `binary.png` and the annotated `threshold-morphology.png`.

实现使用 `Threshold`、`GetStructuringElement`、`MorphologyEx` 和 `CountNonZero`，输出 `binary.png` 以及带指标的 `threshold-morphology.png`。

## Pipeline / 流程

1. Convert the BGR fixture to one-channel gray data.
2. Let Otsu select the threshold instead of hard-coding a scene-specific value.
3. Apply an elliptical 9x9 kernel for opening, then closing, and report the remaining foreground area.

开运算适合去除小白点，闭运算适合填补小黑洞。换用真实输入时，应根据目标尺寸重新选择 kernel 的形状和半径。
