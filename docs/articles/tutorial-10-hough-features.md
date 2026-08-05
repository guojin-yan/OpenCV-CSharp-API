# 10 Hough Lines And Circles / Hough 直线与圆

The case creates a scene containing line segments and a circle, runs standard and probabilistic Hough line detection plus Hough circle detection, and overlays the detections with counts.

本案例生成包含线段和圆的场景，分别运行标准/概率 Hough 直线和 Hough 圆检测，并把检测结果和数量叠加到图像上。

```powershell
dotnet run --project .\samples\ImageProcessing\06.HoughFeatures\HoughFeatures.csproj -c Release -- .\artifacts\tutorial-10
```

The example is intentionally synthetic so a CI machine can verify geometry without a camera. Tune the edge threshold and accumulator thresholds when adapting it to real images.

## Pipeline / 流程

The input is converted to gray and Canny edges first. `HoughLines` returns `(rho, theta)` infinite-line parameters, `HoughLinesP` returns segment endpoints, and `HoughCircles` returns center/radius triples. Draw each result in a different color so review can distinguish the three detectors.

先把输入转换为灰度并提取 Canny 边缘。`HoughLines` 返回 `(rho, theta)` 形式的无限直线，`HoughLinesP` 返回线段端点，`HoughCircles` 返回圆心和半径三元组。用不同颜色绘制，便于检查三种检测器的结果。
