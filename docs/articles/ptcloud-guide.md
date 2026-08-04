# PtCloud Guide / PtCloud 指南

`JYPPX.OpenCvSharp.PtCloud` wraps the first OpenCV `ptcloud` depth and RGB-D utilities through stable `Mat` handles.

`JYPPX.OpenCvSharp.PtCloud` 通过稳定的 `Mat` 句柄封装第一批 OpenCV `ptcloud` 深度与 RGB-D 工具。

## Scope / 范围

- `PtCloudCv2.RescaleDepth`, `DepthTo3d`, `DepthTo3dSparse`, `RegisterDepth`, `WarpFrame`, and `FindPlanes`.
- `RgbdNormals` with rows, cols, window size, depth, camera matrix, method, `Cache`, and `Apply`.
- `RgbdNormalsMethod` and `RgbdPlaneMethod` enums.

- `PtCloudCv2.RescaleDepth`、`DepthTo3d`、`DepthTo3dSparse`、`RegisterDepth`、`WarpFrame` 和 `FindPlanes`。
- `RgbdNormals`，包含 rows、cols、window size、depth、相机矩阵、method、`Cache` 和 `Apply`。
- `RgbdNormalsMethod` 与 `RgbdPlaneMethod` 枚举。

## Runtime / 运行时

`ptcloud` is a main OpenCV module for the current packaged runtime identity. Linked runtime packages should include the factual OpenCV 5.0.0 runtime artifact `opencv_ptcloud500.dll`; the native ABI remains exported in stub builds and returns `NOT_LINKED` when the module is unavailable.

`ptcloud` 是当前打包 runtime 身份中的 OpenCV 主线模块。linked runtime 包应包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_ptcloud500.dll`；stub build 中 native ABI 仍会导出，模块不可用时返回 `NOT_LINKED`。

## Input Notes / 输入说明

Depth behavior depends on camera intrinsics, depth units, and matrix type. `CV_16U` depth is treated like millimeters by OpenCV helpers, while `CV_32F` and `CV_64F` are treated as metric depth values. Plane extraction expects organized 3D points and benefits from calibrated intrinsics and realistic sensor-error parameters.

深度处理结果取决于相机内参、深度单位和矩阵类型。OpenCV helper 会把 `CV_16U` 深度视作类似毫米的输入，而 `CV_32F` 与 `CV_64F` 视作米制深度值。平面提取需要有组织的 3D 点，并依赖合理的内参和传感器误差参数。

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.PtCloud;

using Mat depth = new Mat(2, 2, MatType.CV_16UC1, new Scalar(1000));
using Mat k = Mat.Eye(3, 3, MatType.CV_32F);
using Mat meters = PtCloudCv2.RescaleDepth(depth, MatType.CV_32F);
using Mat points = PtCloudCv2.DepthTo3d(meters, k);

using RgbdNormals normals = RgbdNormals.Create(2, 2, MatType.CV_32F, k);
using Mat normalMap = normals.Apply(points);
```
