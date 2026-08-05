# 14 Background Subtraction / 背景差分

Thirty deterministic frames simulate a moving object. `BackgroundSubtractorMOG2` learns the background, while a small morphology pass cleans the foreground mask before it is highlighted.

三十帧确定性图像模拟运动目标。`BackgroundSubtractorMOG2` 学习背景，再用形态学操作清理前景掩码并高亮运动区域。

```powershell
dotnet run --project .\samples\Video\01.BackgroundSubtraction\BackgroundSubtraction.csproj -c Release -- .\artifacts\tutorial-14
```

This headless example verifies the temporal API without a camera or video file. For a live stream, feed each decoded frame to the same subtractor instance and retain its state.

## Pipeline / 流程

The first frames warm up the model, then the moving rectangle is introduced. Each mask is thresholded to a binary foreground, opened with a small ellipse, and used with `BitwiseAnd` to highlight motion on the current frame. The last frame and total foreground area are written to the report.

前几帧用于预热背景模型，然后加入移动矩形。每个掩码先阈值化为二值前景，再用小椭圆核开运算，并通过 `BitwiseAnd` 在当前帧上高亮运动区域。报告输出最后一帧和累计前景面积。
