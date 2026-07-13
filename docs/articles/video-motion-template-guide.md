# Video Motion Template Guide

Round 57 rechecked the local OpenCV 5.0.0 public video headers for motion-template APIs such as `updateMotionHistory`, `calcMotionGradient`, `calcGlobalOrientation`, and `segmentMotion`.

Round 57 重新核对了本地 OpenCV 5.0.0 的 video public header，目标是确认 `updateMotionHistory`、`calcMotionGradient`、`calcGlobalOrientation` 和 `segmentMotion` 等 motion-template API。

## Local Header Result / 本地头文件结论

In this source tree, motion-template calls appear in OpenCV tests and performance sources, but they are not declared in the public headers under `modules/video/include/opencv2/video`. Because the project follows the local OpenCV 5.0.0 headers as the ABI source of truth, no public C ABI or managed wrapper is exported for these functions in this round.

在当前源码树中，motion-template 调用出现在 OpenCV 测试和性能源码里，但没有出现在 `modules/video/include/opencv2/video` 下的 public header 中。由于本项目以本地 OpenCV 5.0.0 头文件作为 ABI 的事实来源，本轮不会为这些函数导出 public C ABI 或 managed wrapper。

## Current Alternatives / 当前替代能力

- Use `BackgroundSubtractorMOG2` or `BackgroundSubtractorKNN` to derive motion masks from synthetic or real video frames.
- Use `Cv2.CalcOpticalFlowPyrLK` or `Cv2.CalcOpticalFlowFarneback` for feature motion and dense flow.
- Use `Cv2.MeanShift` and `Cv2.CamShift` for tracking windows over probability images.

- 使用 `BackgroundSubtractorMOG2` 或 `BackgroundSubtractorKNN` 从合成帧或真实视频帧得到运动掩码。
- 使用 `Cv2.CalcOpticalFlowPyrLK` 或 `Cv2.CalcOpticalFlowFarneback` 做特征运动和密集光流。
- 使用 `Cv2.MeanShift` 和 `Cv2.CamShift` 在概率图上做窗口跟踪。

## Future Rule / 后续规则

If a future OpenCV source tree exposes motion-template declarations in public headers, the wrapper should add them as regular stable C ABI functions with `Mat` handles and flat value buffers.

如果后续 OpenCV 源码树在 public header 中暴露 motion-template 声明，本项目应通过 `Mat` handle 和平铺值缓冲把它们加入稳定 C ABI。
