# Point-Set Marshalling Guide

OpenCV frequently uses `std::vector<std::vector<Point2f>>` or `OutputArrayOfArrays` for marker corners, board corners, and calibration point groups. OpenCvSharp keeps those C++ containers inside the native boundary and represents grouped points with an offset table plus a flat point buffer.

OpenCV 经常使用 `std::vector<std::vector<Point2f>>` 或 `OutputArrayOfArrays` 表示 marker corners、board corners 和标定点分组。OpenCvSharp 将这些 C++ 容器保留在 native 边界内部，并使用偏移表加扁平点缓冲区表示分组点集。

## Native Layout / Native 布局

For grouped `Point2f` output, native count functions return:

对于分组 `Point2f` 输出，native count 函数返回：

- group count
- total flat point count
- any parallel array counts, such as marker ids or confidence values

- 分组数量
- 扁平点总数
- 并行数组数量，例如 marker id 或置信度

Fill functions then write:

fill 函数随后写入：

- `offsets[groupCount + 1]`, where `offsets[0]` is `0` and `offsets[groupCount]` is the flat point count.
- `points[pointCount]`, storing every group contiguously.
- optional parallel arrays such as `ids[groupCount]`.

- `offsets[groupCount + 1]`，其中 `offsets[0]` 为 `0`，`offsets[groupCount]` 为扁平点总数。
- `points[pointCount]`，每个分组连续存储。
- 可选的并行数组，例如 `ids[groupCount]`。

This shape avoids exposing `std::vector`, `cv::InputArray`, `cv::OutputArray`, or OpenCV object layouts through exported C signatures.

这种布局避免在导出的 C 签名中暴露 `std::vector`、`cv::InputArray`、`cv::OutputArray` 或 OpenCV 对象布局。

## Managed Layout / Managed 布局

Managed wrappers convert the flat data to arrays such as:

managed wrapper 会将扁平数据转换为：

```csharp
Point2f[][] corners;
Point2f[][] rejectedCandidates;
```

Each inner array is a managed copy owned by the caller. It remains valid after the native call returns and does not borrow memory from OpenCV.

每个内部数组都是由调用方拥有的 managed 副本。native 调用返回后仍然有效，不借用 OpenCV 内存。

## Current Uses / 当前用途

- `ArucoDetector.DetectMarkers`
- `ArucoDetector.DetectMarkersWithConfidence`
- `ArucoDetector.RefineDetectedMarkers`
- `CharucoDetector.DetectBoard`
- `CChecker.GetBox`
- `CChecker.GetColorCharts`
- `Cv2.CalibrateCamera`
- `Cv2.CalibrateCameraExtended`
- `Cv2.StereoCalibrate`
- `Cv2.StereoCalibrateExtended`
- `Cv2.Rectify3Collinear`

`CharucoDetector.DetectBoard`, ArUco refine, and full calibration APIs also use this shape for input point groups. The managed input arrays are pinned or copied to a short-lived native buffer only for the duration of the call.

`CharucoDetector.DetectBoard`、ArUco refine 和完整标定 API 也用这种布局传递输入点分组。managed 输入数组只会在调用期间被固定或复制到短生命周期 native 缓冲区。
