# Saliency Guide / Saliency 指南

`JYPPX.OpenCvSharp.Saliency` wraps OpenCV 5.0.0 contrib `saliency` static, motion, and objectness saliency objects. The linked runtime module is the factual OpenCV 5.0.0 runtime artifact `opencv_saliency500.dll`.

`JYPPX.OpenCvSharp.Saliency` 封装 OpenCV 5.0.0 contrib `saliency` 静态、运动和 objectness 显著性对象。linked runtime 模块是事实性 OpenCV 5.0.0 runtime 产物 `opencv_saliency500.dll`。

## Scope / 范围

- Base object: `Saliency` with `ComputeSaliency`.
- Static base: `StaticSaliency` with `ComputeBinaryMap`.
- Static algorithms: `StaticSaliencySpectralResidual` and `StaticSaliencyFineGrained`.
- Motion algorithm: `MotionSaliencyBinWangApr2014` with image-size properties, `SetImageSize`, and `Init`.
- Objectness algorithm: `ObjectnessBING` with training path, `BBResDir`, `Base`, `NSS`, `W`, candidate boxes, and objectness values.

- 基类对象：带 `ComputeSaliency` 的 `Saliency`。
- 静态基类：带 `ComputeBinaryMap` 的 `StaticSaliency`。
- 静态算法：`StaticSaliencySpectralResidual` 和 `StaticSaliencyFineGrained`。
- 运动算法：`MotionSaliencyBinWangApr2014`，包含图像尺寸属性、`SetImageSize` 和 `Init`。
- Objectness 算法：`ObjectnessBING`，包含 training path、`BBResDir`、`Base`、`NSS`、`W`、候选框和 objectness values。

## Runtime / 运行时

`saliency` is an optional OpenCV contrib module. Runtime staging should include the factual OpenCV 5.0.0 runtime artifact `opencv_saliency500.dll` when OpenCV was built with contrib. The module depends on image-processing support, including the factual OpenCV 5.0.0 runtime artifact `opencv_imgproc500.dll`, and the local OpenCV 5.0.0 build also stages the related factual runtime artifact `opencv_features500.dll` dependency.

`saliency` 是可选 OpenCV contrib 模块。OpenCV 使用 contrib 构建时，runtime staging 应包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_saliency500.dll`。该模块依赖图像处理支持，包括事实性 OpenCV 5.0.0 runtime 产物 `opencv_imgproc500.dll`；当前本地 OpenCV 5.0.0 构建也会暂存相关事实性 runtime 产物 `opencv_features500.dll` 依赖。

If the factual OpenCV 5.0.0 runtime artifact `opencv_saliency500.dll` is not linked, the exported native ABI remains present and managed calls report `NOT_LINKED`.

如果未链接事实性 OpenCV 5.0.0 runtime 产物 `opencv_saliency500.dll`，导出的 native ABI 仍存在，managed 调用会报告 `NOT_LINKED`。

## Output Notes / 输出说明

Static and motion saliency algorithms write saliency maps to caller-owned `Mat` objects or return a new `Mat`. `StaticSaliency.ComputeBinaryMap` converts a saliency map into a binary map through OpenCV. Tiny generated-image tests only verify linked calls and output shape; they do not measure saliency quality.

静态与运动显著性算法会把显著性图写入调用方持有的 `Mat`，也可以返回新 `Mat`。`StaticSaliency.ComputeBinaryMap` 通过 OpenCV 将显著性图转换为二值图。tiny 合成图测试只验证 linked 调用和输出形状，不衡量显著性质量。

`MotionSaliencyBinWangApr2014` should be configured with `SetImageSize` or `ImageWidth`/`ImageHeight`, then initialized with `Init` before processing frames. In the local OpenCV 5.0.0 implementation, it expects single-channel frames.

`MotionSaliencyBinWangApr2014` 应先通过 `SetImageSize` 或 `ImageWidth`/`ImageHeight` 配置尺寸，再调用 `Init` 初始化后处理帧。在本地 OpenCV 5.0.0 实现中，它需要单通道帧。

`ObjectnessBING` is included as a parameter/path and count/fill wrapper. Real objectness proposals require caller-supplied BING training data; default tests do not download or bundle it. See [Saliency Objectness Guide](saliency-objectness-guide.md).

`ObjectnessBING` 已作为参数/路径和 count/fill wrapper 纳入。真实 objectness proposal 需要调用方提供 BING 训练数据；默认测试不会下载或内置这些数据。见 [Saliency Objectness Guide](saliency-objectness-guide.md)。

Default tests do not require external models, image datasets, cameras, GUI windows, or downloads.

默认测试不依赖外部模型、图像数据集、摄像头、GUI 窗口或下载。

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Saliency;

using Mat image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(20, 30, 40));
using Mat motionFrame = new Mat(32, 32, MatType.CV_8UC1, new Scalar(40));
using StaticSaliencySpectralResidual spectral = StaticSaliencySpectralResidual.Create();
using StaticSaliencyFineGrained fine = StaticSaliencyFineGrained.Create();
using MotionSaliencyBinWangApr2014 motion = MotionSaliencyBinWangApr2014.Create();

using Mat spectralMap = spectral.ComputeSaliency(image);
using Mat binaryMap = spectral.ComputeBinaryMap(spectralMap);
using Mat fineMap = fine.ComputeSaliency(image);

motion.SetImageSize(motionFrame.Cols, motionFrame.Rows);
motion.Init();
using Mat motionMap = motion.ComputeSaliency(motionFrame);
```
