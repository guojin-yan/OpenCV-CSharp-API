# Stitching Runtime Guide

The `OpenCvSharp.Stitching` package requires the factual OpenCV 5.0.0 runtime artifact `opencv_stitching500.dll` when the native wrapper is linked to OpenCV. The high-level `Stitcher` pipeline also uses capabilities from core image processing and feature modules that are already staged by the runtime package.

`OpenCvSharp.Stitching` 包在 linked OpenCV runtime 中需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_stitching500.dll`。高层 `Stitcher` pipeline 还会使用 runtime 包中已暂存的 core、图像处理和特征模块能力。

## Runtime Files / 运行时文件

Expected Windows x64 runtime files include:

Windows x64 runtime 预期包含：

`JYPPX.OpenCV.Native.dll` is the primary loader. `OpenCv5Sharp.Native.dll` is the explicitly named compatibility loader copy kept stable for already-compiled consumers. The `opencv_*500.dll` names are factual OpenCV 5.0.0 runtime artifacts.

`JYPPX.OpenCV.Native.dll` 是主 loader。`OpenCv5Sharp.Native.dll` 是为已编译消费者保持稳定的名称明确兼容 loader 副本。`opencv_*500.dll` 名称是 OpenCV 5.0.0 runtime 的事实性产物。

- `JYPPX.OpenCV.Native.dll` (primary loader / 主 loader)
- `OpenCv5Sharp.Native.dll` (explicit compatibility loader copy kept stable for already-compiled consumers / 为已编译消费者保持稳定的明确兼容 loader 副本)
- factual OpenCV 5.0.0 runtime artifact `opencv_core500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_imgproc500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_imgcodecs500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_features500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_flann500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_stitching500.dll`

If the factual OpenCV 5.0.0 runtime artifact `opencv_stitching500.dll` is missing, the managed API remains present but native calls report a clear `NOT_LINKED` error.

如果缺少事实性 OpenCV 5.0.0 runtime 产物 `opencv_stitching500.dll`，managed API 仍会存在，但 native 调用会返回明确的 `NOT_LINKED` 错误。

## Smoke Strategy / Smoke 策略

Default tests cover enum values, result object shape, managed validation, and no-OpenCV/stub behavior. Real native smoke uses `OPENCV_CSHARP_NATIVE_SMOKE=1` and should accept any defined `StitcherStatus`, because tiny synthetic images may legitimately fail homography or camera adjustment. The older `OPENCV5SHARP_NATIVE_SMOKE=1` name remains accepted only as an existing-smoke-workflow compatibility alias.

默认测试覆盖枚举值、结果对象形状、managed 参数校验和 no-OpenCV/stub 行为。真实 native smoke 使用 `OPENCV_CSHARP_NATIVE_SMOKE=1`，并应接受任何定义内的 `StitcherStatus`，因为小型合成图像完全可能合理地出现单应或相机调整失败。旧的 `OPENCV5SHARP_NATIVE_SMOKE=1` 名称仍仅作为既有 smoke workflow 的兼容别名使用。

ConsoleSamples can also use real image files:

ConsoleSamples 也可以使用真实图像文件：

```powershell
$env:OPENCV_CSHARP_STITCHING_IMAGES="left.jpg;middle.jpg;right.jpg"
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release
```

Real input images should have enough overlap and stable visual features.

真实输入图像应具有足够重叠和稳定视觉特征。
