# BgSegm Guide / BgSegm 指南

`OpenCvSharp.BgSegm` wraps the first OpenCV 5.0.0 contrib `bgsegm` background-modeling objects.

`OpenCvSharp.BgSegm` 封装第一批 OpenCV 5.0.0 contrib `bgsegm` 背景建模对象。

## Scope / 范围

- Base object: `BgSegmBackgroundSubtractor` with `Apply`, known-foreground `Apply`, `ApplyWithKnownForeground`, and `GetBackgroundImage`.
- Background subtractors: `BackgroundSubtractorMOG`, `BackgroundSubtractorGMG`, and `BackgroundSubtractorCNT`.
- Synthetic data: `SyntheticSequenceGenerator` with `GetNextFrame`.
- Parameters: MOG history/mixtures/background ratio/noise sigma, GMG feature and threshold settings, CNT stability/history/parallel settings.

- 基类对象：带 `Apply`、known-foreground `Apply`、`ApplyWithKnownForeground` 和 `GetBackgroundImage` 的 `BgSegmBackgroundSubtractor`。
- 背景减除器：`BackgroundSubtractorMOG`、`BackgroundSubtractorGMG` 和 `BackgroundSubtractorCNT`。
- 合成数据：带 `GetNextFrame` 的 `SyntheticSequenceGenerator`。
- 参数：MOG history/mixtures/background ratio/noise sigma，GMG feature/threshold 设置，CNT stability/history/parallel 设置。

## Runtime / 运行时

`bgsegm` is an optional OpenCV contrib module. A linked runtime should include the factual OpenCV 5.0.0 runtime artifact `opencv_bgsegm500.dll` plus the normal image-processing/video dependencies staged by the runtime package. If the module is not linked, the exported ABI remains present and managed calls report `NOT_LINKED`.

`bgsegm` 是可选 OpenCV contrib 模块。linked runtime 应包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_bgsegm500.dll`，以及 runtime 包已暂存的常规图像处理/video 依赖。如果模块未链接，导出的 ABI 仍存在，managed 调用会报告 `NOT_LINKED`。

## Modeling Notes / 建模说明

Background modeling needs a frame sequence before the model becomes useful. Tiny generated-frame samples and tests only verify that object creation, properties, `Apply`, and background-image calls cross the native boundary correctly.

背景建模需要多帧序列才能形成有用模型。tiny 合成帧样例和测试只验证对象创建、属性、`Apply` 与背景图调用能正确跨过 native 边界。

`BgSegmBackgroundSubtractor` is separate from `OpenCvSharp.Video.BackgroundSubtractor` because the native layer keeps each module's opaque handle ownership independent. In the local OpenCV 5.0.0 contrib implementation, MOG reports `StsNotImplemented` for `getBackgroundImage`; use `Apply` for MOG smoke and call `GetBackgroundImage` on algorithms that implement it, such as CNT.

`BgSegmBackgroundSubtractor` 与 `OpenCvSharp.Video.BackgroundSubtractor` 分离，因为 native 层保持各模块 opaque handle 的所有权边界独立。在本地 OpenCV 5.0.0 contrib 实现中，MOG 的 `getBackgroundImage` 会返回 `StsNotImplemented`；MOG smoke 建议使用 `Apply`，背景图调用应放在 CNT 等已实现该函数的算法上。

```csharp
using OpenCvSharp.BgSegm;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;

using Mat first = new Mat(24, 24, MatType.CV_8UC3, new Scalar(20, 40, 60));
using Mat second = first.Clone();
using Mat mask = new Mat();
using Mat knownForeground = new Mat(first.Rows, first.Cols, MatType.CV_8UC1, new Scalar(0));
using Mat background = new Mat();

Cv2.Rectangle(second, new Rect(8, 5, 8, 9), new Scalar(200, 30, 80), -1);

using BackgroundSubtractorMOG mog = BackgroundSubtractorMOG.Create(history: 10, nmixtures: 3);
using BackgroundSubtractorCNT cnt = BackgroundSubtractorCNT.Create(minPixelStability: 2, maxPixelStability: 6);
mog.Apply(first, mask, 1.0);
mog.Apply(second, mask, 0.5);
cnt.Apply(first, mask, 1.0);
cnt.Apply(second, mask, 0.5);
using Mat knownForegroundMask = cnt.ApplyWithKnownForeground(second, knownForeground, 0.5);
cnt.GetBackgroundImage(background);
```
