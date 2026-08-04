# Stitching Runtime Guide

The `JYPPX.OpenCvSharp.Stitching` package requires the factual OpenCV 5.0.0 runtime artifact `opencv_stitching500.dll` when the native wrapper is linked to OpenCV. The high-level `Stitcher` pipeline also uses capabilities from core image processing and feature modules that are already staged by the runtime package.

`JYPPX.OpenCvSharp.Stitching` 包在 linked OpenCV runtime 中需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_stitching500.dll`。高层 `Stitcher` pipeline 还会使用 runtime 包中已暂存的 core、图像处理和特征模块能力。

## Runtime Files / 运行时文件

Expected Windows x64 runtime files include:

Windows x64 runtime 预期包含：



- `JYPPX.OpenCV.Native.dll` (primary loader / 主 loader)
- factual OpenCV 5.0.0 runtime artifact `opencv_core500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_imgproc500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_imgcodecs500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_features500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_flann500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_stitching500.dll`

If the factual OpenCV 5.0.0 runtime artifact `opencv_stitching500.dll` is missing, the managed API remains present but native calls report a clear `NOT_LINKED` error.

如果缺少事实性 OpenCV 5.0.0 runtime 产物 `opencv_stitching500.dll`，managed API 仍会存在，但 native 调用会返回明确的 `NOT_LINKED` 错误。

## Smoke Strategy / Smoke 策略



ConsoleSamples can also use real image files:

ConsoleSamples 也可以使用真实图像文件：

```powershell
$env:OPENCV_CSHARP_STITCHING_IMAGES="left.jpg;middle.jpg;right.jpg"
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release
```

Real input images should have enough overlap and stable visual features.

真实输入图像应具有足够重叠和稳定视觉特征。
