# Calib3D Full Calibration Guide

`OpenCvSharp.Calib3D.Cv2` exposes OpenCV camera calibration through stable grouped point-set marshalling. The managed API accepts `Point3f[][]` object points and `Point2f[][]` image points, while the native ABI keeps OpenCV `InputArrayOfArrays`, `OutputArrayOfArrays`, and `std::vector` inside the C++ boundary.

`OpenCvSharp.Calib3D.Cv2` 通过稳定的分组点集封送暴露 OpenCV 相机标定能力。managed API 接收 `Point3f[][]` 物点和 `Point2f[][]` 像点，native ABI 将 OpenCV `InputArrayOfArrays`、`OutputArrayOfArrays` 和 `std::vector` 保留在 C++ 边界内部。

## APIs / API

- `Cv2.CalibrateCamera(...)`
- `Cv2.CalibrateCameraExtended(...)`
- `Cv2.CalibrateCameraRO(...)`
- `Cv2.CalibrateCameraROExtended(...)`
- `Cv2.InitCameraMatrix2D(...)`
- `CalibrationResult`
- `CalibrationExtendedResult`
- `CalibrationROResult`
- `CalibrationROExtendedResult`
- `CalibrationFlags`

- `Cv2.CalibrateCamera(...)`
- `Cv2.CalibrateCameraExtended(...)`
- `Cv2.CalibrateCameraRO(...)`
- `Cv2.CalibrateCameraROExtended(...)`
- `Cv2.InitCameraMatrix2D(...)`
- `CalibrationResult`
- `CalibrationExtendedResult`
- `CalibrationROResult`
- `CalibrationROExtendedResult`
- `CalibrationFlags`

The returned `Rvecs` and `Tvecs` matrices pack one pose per row as `N x 3`. Returned result objects contain owned `Mat` instances; callers should dispose those matrices after use.

返回的 `Rvecs` 和 `Tvecs` 以 `N x 3` 矩阵打包，每行一个位姿。返回结果对象包含拥有所有权的 `Mat` 实例，调用方使用后应释放这些矩阵。

## Initial Intrinsic Estimation / 初始内参估计

`InitCameraMatrix2D` estimates a `3 x 3` initial camera intrinsic matrix before full calibration. It uses grouped 3D-2D correspondences to estimate the focal lengths and initializes the principal point from the image size.

`InitCameraMatrix2D` 在完整标定前估计一个 `3 x 3` 初始相机内参矩阵。它使用分组的三维-二维对应点估计焦距，并根据图像尺寸初始化主点。

All object points must describe a planar target with Z coordinates near zero. A positive `aspectRatio` constrains `fx = fy * aspectRatio`; zero or negative values estimate `fx` and `fy` independently.

所有物点必须描述 Z 坐标接近零的平面目标。正 `aspectRatio` 约束 `fx = fy * aspectRatio`；零或负值会独立估计 `fx` 和 `fy`。

The overload returning `Mat` transfers ownership to the caller and must be disposed. The output overload writes into a caller-owned `Mat`.

返回 `Mat` 的重载把所有权交给调用方，使用后必须释放；输出重载则写入由调用方拥有的 `Mat`。

```csharp
using Mat initialCameraMatrix = Calib3DCv2.InitCameraMatrix2D(
    objectPoints,
    imagePoints,
    new Size(640, 480),
    aspectRatio: 0.0);

Console.WriteLine(initialCameraMatrix.Rows + "x" + initialCameraMatrix.Cols);
```

## Object-Releasing Calibration / 释放物点标定

`CalibrateCameraRO` extends ordinary calibration for roughly planar targets whose printed or manufactured coordinates are not sufficiently accurate. When object release is active, OpenCV jointly refines the camera parameters, per-view poses, and target point coordinates.

`CalibrateCameraRO` 面向印刷或制造坐标不够精确的近似平面标定板扩展普通标定。当启用释放物点方法时，OpenCV 会联合精炼相机参数、每视图位姿和目标物点坐标。

`iFixedPoint` selects the behavior:

- `1 <= iFixedPoint <= objectPoints[0].Length - 2` enables object release.
- A value outside that range intentionally falls back to standard camera calibration.
- For object release, every view must use the same fully visible board, all object-point groups must describe the same roughly planar target, and the target must remain rigid or static.

`iFixedPoint` 用于选择行为：

- `1 <= iFixedPoint <= objectPoints[0].Length - 2` 时启用释放物点。
- 范围外值会按 OpenCV 设计回退到标准相机标定。
- 使用释放物点时，每个视图必须包含同一块完整可见的标定板，所有物点分组必须描述同一近似平面目标，并且目标必须保持刚性或静止。

Managed output shapes are deterministic:

- `Rvecs` and `Tvecs`: single-channel `N x 3`.
- `NewObjectPoints`: single-channel `P x 3`, where `P` is the number of points in one board view.
- `StdDeviationsObjectPoints`: single-channel `3P x 1`.
- `PerViewErrors`: single-channel `N x 1`.
- `NewObjectPoints` and `StdDeviationsObjectPoints` are empty when `iFixedPoint` selects standard calibration.

managed 输出形状固定为：

- `Rvecs` 和 `Tvecs`：单通道 `N x 3`。
- `NewObjectPoints`：单通道 `P x 3`，其中 `P` 是单个标定板视图的点数。
- `StdDeviationsObjectPoints`：单通道 `3P x 1`。
- `PerViewErrors`：单通道 `N x 1`。
- 当 `iFixedPoint` 选择标准标定时，`NewObjectPoints` 和 `StdDeviationsObjectPoints` 为空。

```csharp
CalibrationROExtendedResult ro = Calib3DCv2.CalibrateCameraROExtended(
    objectPoints,
    imagePoints,
    new Size(640, 480),
    iFixedPoint: 6);

try
{
    Console.WriteLine(ro.Calibration.Calibration.ReprojectionError);
    Console.WriteLine(ro.Calibration.NewObjectPoints.Rows + " refined points");
    Console.WriteLine(ro.StdDeviationsObjectPoints.Rows + " coordinate deviations");
}
finally
{
    ro.Calibration.Calibration.CameraMatrix.Dispose();
    ro.Calibration.Calibration.DistCoeffs.Dispose();
    ro.Calibration.Calibration.Rvecs.Dispose();
    ro.Calibration.Calibration.Tvecs.Dispose();
    ro.Calibration.NewObjectPoints.Dispose();
    ro.StdDeviationsIntrinsics.Dispose();
    ro.StdDeviationsExtrinsics.Dispose();
    ro.StdDeviationsObjectPoints.Dispose();
    ro.PerViewErrors.Dispose();
}
```

## Example / 示例

```csharp
using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace Calib3DFullCalibrationSample
{
    internal static class Program
    {
        private static void Main()
        {
            Point3f[][] objectPoints =
            {
                new[]
                {
                    new Point3f(0, 0, 0),
                    new Point3f(1, 0, 0),
                    new Point3f(0, 1, 0),
                    new Point3f(1, 1, 0)
                }
            };
            Point2f[][] imagePoints =
            {
                new[]
                {
                    new Point2f(100, 100),
                    new Point2f(200, 100),
                    new Point2f(100, 200),
                    new Point2f(200, 200)
                }
            };

            CalibrationResult result = Calib3DCv2.CalibrateCamera(
                objectPoints,
                imagePoints,
                new Size(640, 480));

            try
            {
                Console.WriteLine(result.ReprojectionError);
                Console.WriteLine(result.CameraMatrix.Rows + "x" + result.CameraMatrix.Cols);
            }
            finally
            {
                result.CameraMatrix.Dispose();
                result.DistCoeffs.Dispose();
                result.Rvecs.Dispose();
                result.Tvecs.Dispose();
            }
        }
    }
}
```

## Runtime Notes / 运行时说明

Full calibration APIs require the current packaged runtime identity's factual OpenCV 5.0.0 `calib` module, staged as the factual runtime artifact `opencv_calib500.dll`, in addition to core matrix dependencies. The no-OpenCV native build still exports the ABI and returns the defined `NOT_LINKED` status.

完整标定 API 需要当前打包 runtime 身份中的事实性 OpenCV 5.0.0 `calib` 模块；`opencv_calib500.dll` 是对应事实性 runtime 产物，并且还需要 core 矩阵相关依赖。no-OpenCV native 构建仍会导出 ABI，并返回定义明确的 `NOT_LINKED` 状态。
