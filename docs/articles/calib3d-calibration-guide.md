# Calib3D Calibration Pattern Guide

This guide covers calibration-support APIs exposed by `OpenCvSharp.Calib3D`. They focus on pattern detection, pose refinement, camera matrix utilities, and drawing helpers. Full multi-view `CalibrateCamera` is covered in the dedicated [Calib3D Full Calibration Guide](calib3d-full-calibration-guide.md).

本指南覆盖 `OpenCvSharp.Calib3D` 标定辅助接口，重点是标定图案检测、位姿 refinement、相机矩阵工具和绘制辅助。完整多视角 `CalibrateCamera` 见专门的 [Calib3D Full Calibration Guide](calib3d-full-calibration-guide.md)。

## Covered APIs / 已覆盖接口

- Pattern detection: `FindChessboardCorners`, `CheckChessboard`, `FindCirclesGrid`.
- Pattern visualization: `DrawChessboardCorners`.
- Pose refinement: `SolvePnPRefineLM`, `SolvePnPRefineVVS`.
- Camera utilities: `GetOptimalNewCameraMatrix`, `CalibrationMatrixValues`.
- Object-releasing calibration: `CalibrateCameraRO`, `CalibrateCameraROExtended`.
- Hand-eye calibration: `CalibrateHandEye`, `CalibrateRobotWorldHandEye`.
- Flags and result objects: `ChessboardFlags`, `CirclesGridFlags`, `CalibrationFlags`, `CalibrationROResult`, `CalibrationROExtendedResult`, `HandEyeCalibrationMethod`, `RobotWorldHandEyeCalibrationMethod`, `HandEyeCalibrationResult`, `RobotWorldHandEyeCalibrationResult`, `OptimalNewCameraMatrixResult`, `CalibrationMatrixValuesResult`.

- 图案检测：`FindChessboardCorners`、`CheckChessboard`、`FindCirclesGrid`。
- 图案可视化：`DrawChessboardCorners`。
- 位姿优化：`SolvePnPRefineLM`、`SolvePnPRefineVVS`。
- 相机工具：`GetOptimalNewCameraMatrix`、`CalibrationMatrixValues`。
- 释放物点标定：`CalibrateCameraRO`、`CalibrateCameraROExtended`。
- 手眼标定：`CalibrateHandEye`、`CalibrateRobotWorldHandEye`。
- 标志和结果对象：`ChessboardFlags`、`CirclesGridFlags`、`CalibrationFlags`、`CalibrationROResult`、`CalibrationROExtendedResult`、`HandEyeCalibrationMethod`、`RobotWorldHandEyeCalibrationMethod`、`HandEyeCalibrationResult`、`RobotWorldHandEyeCalibrationResult`、`OptimalNewCameraMatrixResult`、`CalibrationMatrixValuesResult`。

## Chessboard Corners / 棋盘格角点

`FindChessboardCorners` writes corner coordinates into an output `Mat`. The overload with `out Mat` creates the output matrix for convenience and keeps ownership on the managed side.

`FindChessboardCorners` 将角点坐标写入输出 `Mat`。带 `out Mat` 的重载会创建输出矩阵，方便 managed 侧管理生命周期。

```csharp
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace CalibrationPatternSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(480, 640, MatType.CV_8UC1))
            {
                image.SetTo(new Scalar(255));

                bool hasBoard = Calib3DCv2.CheckChessboard(image, new Size(9, 6));
                using (Mat corners = new Mat())
                {
                    bool found = Calib3DCv2.FindChessboardCorners(
                        image,
                        new Size(9, 6),
                        corners,
                        ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage);

                    Calib3DCv2.DrawChessboardCorners(image, new Size(9, 6), corners, found);
                    System.Console.WriteLine("Check=" + hasBoard + ", found=" + found + ", corners=" + corners.Rows);
                }
            }
        }
    }
}
```

## Circles Grid / 圆点阵

Use `FindCirclesGrid` for symmetric or asymmetric circle patterns. The initial API accepts the OpenCV flags and returns detected centers in a `Mat`.

对于对称或非对称圆点阵，可以使用 `FindCirclesGrid`。当前 API 接收 OpenCV 标志，并将检测到的圆心返回到 `Mat`。

```csharp
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace CirclesGridSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat image = new Mat(480, 640, MatType.CV_8UC1))
            {
                image.SetTo(new Scalar(0));

                using (Mat centers = new Mat())
                {
                    bool found = Calib3DCv2.FindCirclesGrid(
                        image,
                        new Size(4, 11),
                        centers,
                        CirclesGridFlags.AsymmetricGrid);

                    System.Console.WriteLine("Found circles grid: " + found);
                }
            }
        }
    }
}
```

## Camera Matrix Utilities / 相机矩阵工具

`GetOptimalNewCameraMatrix` returns both the adjusted camera matrix and the valid pixel ROI. `CalibrationMatrixValues` converts a camera matrix and physical aperture size into field-of-view information.

`GetOptimalNewCameraMatrix` 同时返回调整后的相机矩阵和有效像素 ROI。`CalibrationMatrixValues` 根据相机矩阵和物理光圈尺寸计算视场角等信息。

```csharp
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace CameraMatrixUtilitySample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat cameraMatrix = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (Mat distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            {
                cameraMatrix.SetValue(0, 800.0);
                cameraMatrix.SetValue(2, 320.0);
                cameraMatrix.SetValue(4, 800.0);
                cameraMatrix.SetValue(5, 240.0);
                distCoeffs.SetTo(new Scalar(0));

                OptimalNewCameraMatrixResult optimal = Calib3DCv2.GetOptimalNewCameraMatrix(
                    cameraMatrix,
                    distCoeffs,
                    new Size(640, 480),
                    alpha: 0.0);

                CalibrationMatrixValuesResult values = Calib3DCv2.CalibrationMatrixValues(
                    cameraMatrix,
                    new Size(640, 480),
                    apertureWidth: 36.0,
                    apertureHeight: 24.0);

                using (optimal.CameraMatrix)
                {
                    System.Console.WriteLine("Valid ROI width: " + optimal.ValidPixROI.Width);
                    System.Console.WriteLine("FOV X: " + values.FovX);
                }
            }
        }
    }
}
```

## Hand-Eye Calibration / 手眼标定

`CalibrateHandEye` estimates the camera-to-gripper transform from matching gripper-to-base and target-to-camera poses. `CalibrateRobotWorldHandEye` jointly estimates the base-to-world and gripper-to-camera transforms.

`CalibrateHandEye` 根据一一对应的夹爪到基座位姿和目标到相机位姿，估计相机到夹爪的变换。`CalibrateRobotWorldHandEye` 联合估计基座到世界坐标系和夹爪到相机的变换。

All four input collections must contain the same number of poses, with at least three poses. Rotations may be `3 x 3` matrices or three-element rotation vectors; translations must be three-element vectors. Array overloads are available on all target frameworks, and `ReadOnlySpan<Mat>` overloads are available on modern .NET targets.

四组输入集合必须包含相同数量的位姿，且至少需要三个位姿。旋转可以是 `3 x 3` 矩阵或三个元素的旋转向量；平移必须是三个元素的向量。所有目标框架均提供数组重载，现代 .NET 目标还提供 `ReadOnlySpan<Mat>` 重载。

```csharp
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace HandEyeCalibrationSample
{
    internal static class Program
    {
        private static void Main()
        {
            Mat[] rGripper2Base = LoadGripperRotations();
            Mat[] tGripper2Base = LoadGripperTranslations();
            Mat[] rTarget2Cam = LoadTargetRotations();
            Mat[] tTarget2Cam = LoadTargetTranslations();

            HandEyeCalibrationResult result = Calib3DCv2.CalibrateHandEye(
                rGripper2Base,
                tGripper2Base,
                rTarget2Cam,
                tTarget2Cam,
                HandEyeCalibrationMethod.Park);

            using (result.RCam2Gripper)
            using (result.TCam2Gripper)
            {
                System.Console.WriteLine(result);
            }
        }

        private static Mat[] LoadGripperRotations() => LoadMeasuredPoseData("gripper-to-base rotations");
        private static Mat[] LoadGripperTranslations() => LoadMeasuredPoseData("gripper-to-base translations");
        private static Mat[] LoadTargetRotations() => LoadMeasuredPoseData("target-to-camera rotations");
        private static Mat[] LoadTargetTranslations() => LoadMeasuredPoseData("target-to-camera translations");

        private static Mat[] LoadMeasuredPoseData(string name) =>
            throw new System.InvalidOperationException(
                "Provide measured " + name + " from your calibration capture before running this sample.");
    }
}
```

## Runtime Notes / 运行时说明

The pattern APIs depend on OpenCV `objdetect`, while undistortion and drawing paths also use `imgproc`. Full camera/stereo calibration now also requires the factual OpenCV 5.0.0 runtime artifact `opencv_calib500.dll`. Runtime packages that include Calib3D should stage the corresponding factual OpenCV 5.0.0 module artifacts `opencv_core500`, `opencv_geometry500`, `opencv_calib500`, `opencv_objdetect500`, `opencv_stereo500`, and `opencv_imgproc500`.

图案检测 API 依赖 OpenCV `objdetect`，去畸变和绘制路径还会使用 `imgproc`。完整相机/双目标定现在还需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_calib500.dll`。包含 Calib3D 的 runtime 包应暂存对应的事实性 OpenCV 5.0.0 模块产物 `opencv_core500`、`opencv_geometry500`、`opencv_calib500`、`opencv_objdetect500`、`opencv_stereo500` 和 `opencv_imgproc500`。

Grouped `Point2f[][]` and `Point3f[][]` inputs use the shared point-set marshalling design. Hand-eye calibration uses validated `Mat` collections so callers may supply either rotation matrices or Rodrigues rotation vectors.

分组 `Point2f[][]` 和 `Point3f[][]` 输入使用共享点集封送设计。手眼标定使用经过校验的 `Mat` 集合，因此调用方可以传入旋转矩阵或 Rodrigues 旋转向量。
