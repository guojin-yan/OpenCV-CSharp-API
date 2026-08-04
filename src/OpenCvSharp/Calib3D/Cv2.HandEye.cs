using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Calib3D
{
    public static unsafe partial class Cv2
    {
        /// <summary>
        /// Computes the camera-to-gripper transformation from robot and target poses.
        /// 根据机器人位姿和标定目标位姿计算相机到夹爪的变换。
        /// </summary>
        /// <param name="rGripper2Base">Gripper-to-base rotations for each pose. 每个位姿的夹爪到基座旋转。</param>
        /// <param name="tGripper2Base">Gripper-to-base translations for each pose. 每个位姿的夹爪到基座平移。</param>
        /// <param name="rTarget2Cam">Target-to-camera rotations for each pose. 每个位姿的目标到相机旋转。</param>
        /// <param name="tTarget2Cam">Target-to-camera translations for each pose. 每个位姿的目标到相机平移。</param>
        /// <param name="rCam2Gripper">Output camera-to-gripper rotation. 输出相机到夹爪旋转。</param>
        /// <param name="tCam2Gripper">Output camera-to-gripper translation. 输出相机到夹爪平移。</param>
        /// <param name="method">The calibration algorithm. 标定算法。</param>
        public static void CalibrateHandEye(
            Mat[] rGripper2Base,
            Mat[] tGripper2Base,
            Mat[] rTarget2Cam,
            Mat[] tTarget2Cam,
            Mat rCam2Gripper,
            Mat tCam2Gripper,
            HandEyeCalibrationMethod method = HandEyeCalibrationMethod.Tsai)
        {
            ThrowIfNull(rGripper2Base, nameof(rGripper2Base));
            ThrowIfNull(tGripper2Base, nameof(tGripper2Base));
            ThrowIfNull(rTarget2Cam, nameof(rTarget2Cam));
            ThrowIfNull(tTarget2Cam, nameof(tTarget2Cam));
            ThrowIfNull(rCam2Gripper, nameof(rCam2Gripper));
            ThrowIfNull(tCam2Gripper, nameof(tCam2Gripper));

            ValidateHandEyeCollectionLengths(
                rGripper2Base.Length,
                tGripper2Base.Length,
                rTarget2Cam.Length,
                tTarget2Cam.Length,
                nameof(rGripper2Base),
                nameof(tGripper2Base),
                nameof(rTarget2Cam),
                nameof(tTarget2Cam));
            ValidateHandEyeMethod(method);

            InvokeCalibrateHandEye(
                CreateTransformHandles(rGripper2Base, true, nameof(rGripper2Base)),
                CreateTransformHandles(tGripper2Base, false, nameof(tGripper2Base)),
                CreateTransformHandles(rTarget2Cam, true, nameof(rTarget2Cam)),
                CreateTransformHandles(tTarget2Cam, false, nameof(tTarget2Cam)),
                rCam2Gripper,
                tCam2Gripper,
                method);
        }

        /// <summary>
        /// Computes and returns the camera-to-gripper transformation.
        /// 计算并返回相机到夹爪的变换。
        /// </summary>
        public static HandEyeCalibrationResult CalibrateHandEye(
            Mat[] rGripper2Base,
            Mat[] tGripper2Base,
            Mat[] rTarget2Cam,
            Mat[] tTarget2Cam,
            HandEyeCalibrationMethod method = HandEyeCalibrationMethod.Tsai)
        {
            var rCam2Gripper = new Mat();
            var tCam2Gripper = new Mat();
            try
            {
                CalibrateHandEye(
                    rGripper2Base,
                    tGripper2Base,
                    rTarget2Cam,
                    tTarget2Cam,
                    rCam2Gripper,
                    tCam2Gripper,
                    method);
                return new HandEyeCalibrationResult(rCam2Gripper, tCam2Gripper);
            }
            catch
            {
                rCam2Gripper.Dispose();
                tCam2Gripper.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Computes robot-world and hand-eye transformations.
        /// 计算机器人世界坐标系变换和手眼变换。
        /// </summary>
        /// <param name="rWorld2Cam">World-to-camera rotations for each pose. 每个位姿的世界到相机旋转。</param>
        /// <param name="tWorld2Cam">World-to-camera translations for each pose. 每个位姿的世界到相机平移。</param>
        /// <param name="rBase2Gripper">Base-to-gripper rotations for each pose. 每个位姿的基座到夹爪旋转。</param>
        /// <param name="tBase2Gripper">Base-to-gripper translations for each pose. 每个位姿的基座到夹爪平移。</param>
        /// <param name="rBase2World">Output base-to-world rotation. 输出基座到世界坐标系旋转。</param>
        /// <param name="tBase2World">Output base-to-world translation. 输出基座到世界坐标系平移。</param>
        /// <param name="rGripper2Cam">Output gripper-to-camera rotation. 输出夹爪到相机旋转。</param>
        /// <param name="tGripper2Cam">Output gripper-to-camera translation. 输出夹爪到相机平移。</param>
        /// <param name="method">The calibration algorithm. 标定算法。</param>
        public static void CalibrateRobotWorldHandEye(
            Mat[] rWorld2Cam,
            Mat[] tWorld2Cam,
            Mat[] rBase2Gripper,
            Mat[] tBase2Gripper,
            Mat rBase2World,
            Mat tBase2World,
            Mat rGripper2Cam,
            Mat tGripper2Cam,
            RobotWorldHandEyeCalibrationMethod method = RobotWorldHandEyeCalibrationMethod.Shah)
        {
            ThrowIfNull(rWorld2Cam, nameof(rWorld2Cam));
            ThrowIfNull(tWorld2Cam, nameof(tWorld2Cam));
            ThrowIfNull(rBase2Gripper, nameof(rBase2Gripper));
            ThrowIfNull(tBase2Gripper, nameof(tBase2Gripper));
            ThrowIfNull(rBase2World, nameof(rBase2World));
            ThrowIfNull(tBase2World, nameof(tBase2World));
            ThrowIfNull(rGripper2Cam, nameof(rGripper2Cam));
            ThrowIfNull(tGripper2Cam, nameof(tGripper2Cam));

            ValidateHandEyeCollectionLengths(
                rWorld2Cam.Length,
                tWorld2Cam.Length,
                rBase2Gripper.Length,
                tBase2Gripper.Length,
                nameof(rWorld2Cam),
                nameof(tWorld2Cam),
                nameof(rBase2Gripper),
                nameof(tBase2Gripper));
            ValidateRobotWorldHandEyeMethod(method);

            InvokeCalibrateRobotWorldHandEye(
                CreateTransformHandles(rWorld2Cam, true, nameof(rWorld2Cam)),
                CreateTransformHandles(tWorld2Cam, false, nameof(tWorld2Cam)),
                CreateTransformHandles(rBase2Gripper, true, nameof(rBase2Gripper)),
                CreateTransformHandles(tBase2Gripper, false, nameof(tBase2Gripper)),
                rBase2World,
                tBase2World,
                rGripper2Cam,
                tGripper2Cam,
                method);
        }

        /// <summary>
        /// Computes and returns robot-world and hand-eye transformations.
        /// 计算并返回机器人世界坐标系变换和手眼变换。
        /// </summary>
        public static RobotWorldHandEyeCalibrationResult CalibrateRobotWorldHandEye(
            Mat[] rWorld2Cam,
            Mat[] tWorld2Cam,
            Mat[] rBase2Gripper,
            Mat[] tBase2Gripper,
            RobotWorldHandEyeCalibrationMethod method = RobotWorldHandEyeCalibrationMethod.Shah)
        {
            var rBase2World = new Mat();
            var tBase2World = new Mat();
            var rGripper2Cam = new Mat();
            var tGripper2Cam = new Mat();
            try
            {
                CalibrateRobotWorldHandEye(
                    rWorld2Cam,
                    tWorld2Cam,
                    rBase2Gripper,
                    tBase2Gripper,
                    rBase2World,
                    tBase2World,
                    rGripper2Cam,
                    tGripper2Cam,
                    method);
                return new RobotWorldHandEyeCalibrationResult(
                    rBase2World,
                    tBase2World,
                    rGripper2Cam,
                    tGripper2Cam);
            }
            catch
            {
                rBase2World.Dispose();
                tBase2World.Dispose();
                rGripper2Cam.Dispose();
                tGripper2Cam.Dispose();
                throw;
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Computes the camera-to-gripper transformation from span-backed pose collections.
        /// 根据 Span 支持的位姿集合计算相机到夹爪的变换。
        /// </summary>
        public static void CalibrateHandEye(
            ReadOnlySpan<Mat> rGripper2Base,
            ReadOnlySpan<Mat> tGripper2Base,
            ReadOnlySpan<Mat> rTarget2Cam,
            ReadOnlySpan<Mat> tTarget2Cam,
            Mat rCam2Gripper,
            Mat tCam2Gripper,
            HandEyeCalibrationMethod method = HandEyeCalibrationMethod.Tsai)
        {
            ThrowIfNull(rCam2Gripper, nameof(rCam2Gripper));
            ThrowIfNull(tCam2Gripper, nameof(tCam2Gripper));
            ValidateHandEyeCollectionLengths(
                rGripper2Base.Length,
                tGripper2Base.Length,
                rTarget2Cam.Length,
                tTarget2Cam.Length,
                nameof(rGripper2Base),
                nameof(tGripper2Base),
                nameof(rTarget2Cam),
                nameof(tTarget2Cam));
            ValidateHandEyeMethod(method);

            InvokeCalibrateHandEye(
                CreateTransformHandles(rGripper2Base, true, nameof(rGripper2Base)),
                CreateTransformHandles(tGripper2Base, false, nameof(tGripper2Base)),
                CreateTransformHandles(rTarget2Cam, true, nameof(rTarget2Cam)),
                CreateTransformHandles(tTarget2Cam, false, nameof(tTarget2Cam)),
                rCam2Gripper,
                tCam2Gripper,
                method);
        }

        /// <summary>
        /// Computes and returns the camera-to-gripper transformation from span-backed pose collections.
        /// 根据 Span 支持的位姿集合计算并返回相机到夹爪的变换。
        /// </summary>
        public static HandEyeCalibrationResult CalibrateHandEye(
            ReadOnlySpan<Mat> rGripper2Base,
            ReadOnlySpan<Mat> tGripper2Base,
            ReadOnlySpan<Mat> rTarget2Cam,
            ReadOnlySpan<Mat> tTarget2Cam,
            HandEyeCalibrationMethod method = HandEyeCalibrationMethod.Tsai)
        {
            var rCam2Gripper = new Mat();
            var tCam2Gripper = new Mat();
            try
            {
                CalibrateHandEye(
                    rGripper2Base,
                    tGripper2Base,
                    rTarget2Cam,
                    tTarget2Cam,
                    rCam2Gripper,
                    tCam2Gripper,
                    method);
                return new HandEyeCalibrationResult(rCam2Gripper, tCam2Gripper);
            }
            catch
            {
                rCam2Gripper.Dispose();
                tCam2Gripper.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Computes robot-world and hand-eye transformations from span-backed pose collections.
        /// 根据 Span 支持的位姿集合计算机器人世界坐标系变换和手眼变换。
        /// </summary>
        public static void CalibrateRobotWorldHandEye(
            ReadOnlySpan<Mat> rWorld2Cam,
            ReadOnlySpan<Mat> tWorld2Cam,
            ReadOnlySpan<Mat> rBase2Gripper,
            ReadOnlySpan<Mat> tBase2Gripper,
            Mat rBase2World,
            Mat tBase2World,
            Mat rGripper2Cam,
            Mat tGripper2Cam,
            RobotWorldHandEyeCalibrationMethod method = RobotWorldHandEyeCalibrationMethod.Shah)
        {
            ThrowIfNull(rBase2World, nameof(rBase2World));
            ThrowIfNull(tBase2World, nameof(tBase2World));
            ThrowIfNull(rGripper2Cam, nameof(rGripper2Cam));
            ThrowIfNull(tGripper2Cam, nameof(tGripper2Cam));
            ValidateHandEyeCollectionLengths(
                rWorld2Cam.Length,
                tWorld2Cam.Length,
                rBase2Gripper.Length,
                tBase2Gripper.Length,
                nameof(rWorld2Cam),
                nameof(tWorld2Cam),
                nameof(rBase2Gripper),
                nameof(tBase2Gripper));
            ValidateRobotWorldHandEyeMethod(method);

            InvokeCalibrateRobotWorldHandEye(
                CreateTransformHandles(rWorld2Cam, true, nameof(rWorld2Cam)),
                CreateTransformHandles(tWorld2Cam, false, nameof(tWorld2Cam)),
                CreateTransformHandles(rBase2Gripper, true, nameof(rBase2Gripper)),
                CreateTransformHandles(tBase2Gripper, false, nameof(tBase2Gripper)),
                rBase2World,
                tBase2World,
                rGripper2Cam,
                tGripper2Cam,
                method);
        }

        /// <summary>
        /// Computes and returns robot-world and hand-eye transformations from span-backed pose collections.
        /// 根据 Span 支持的位姿集合计算并返回机器人世界坐标系变换和手眼变换。
        /// </summary>
        public static RobotWorldHandEyeCalibrationResult CalibrateRobotWorldHandEye(
            ReadOnlySpan<Mat> rWorld2Cam,
            ReadOnlySpan<Mat> tWorld2Cam,
            ReadOnlySpan<Mat> rBase2Gripper,
            ReadOnlySpan<Mat> tBase2Gripper,
            RobotWorldHandEyeCalibrationMethod method = RobotWorldHandEyeCalibrationMethod.Shah)
        {
            var rBase2World = new Mat();
            var tBase2World = new Mat();
            var rGripper2Cam = new Mat();
            var tGripper2Cam = new Mat();
            try
            {
                CalibrateRobotWorldHandEye(
                    rWorld2Cam,
                    tWorld2Cam,
                    rBase2Gripper,
                    tBase2Gripper,
                    rBase2World,
                    tBase2World,
                    rGripper2Cam,
                    tGripper2Cam,
                    method);
                return new RobotWorldHandEyeCalibrationResult(
                    rBase2World,
                    tBase2World,
                    rGripper2Cam,
                    tGripper2Cam);
            }
            catch
            {
                rBase2World.Dispose();
                tBase2World.Dispose();
                rGripper2Cam.Dispose();
                tGripper2Cam.Dispose();
                throw;
            }
        }
#endif

        private static void InvokeCalibrateHandEye(
            IntPtr[] rGripper2Base,
            IntPtr[] tGripper2Base,
            IntPtr[] rTarget2Cam,
            IntPtr[] tTarget2Cam,
            Mat rCam2Gripper,
            Mat tCam2Gripper,
            HandEyeCalibrationMethod method)
        {
            fixed (IntPtr* rGripper2BasePtr = rGripper2Base)
            fixed (IntPtr* tGripper2BasePtr = tGripper2Base)
            fixed (IntPtr* rTarget2CamPtr = rTarget2Cam)
            fixed (IntPtr* tTarget2CamPtr = tTarget2Cam)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DCalibrateHandEye(
                    rGripper2BasePtr,
                    tGripper2BasePtr,
                    rTarget2CamPtr,
                    tTarget2CamPtr,
                    rGripper2Base.Length,
                    rCam2Gripper.NativeHandle,
                    tCam2Gripper.NativeHandle,
                    (int)method));
            }
        }

        private static void InvokeCalibrateRobotWorldHandEye(
            IntPtr[] rWorld2Cam,
            IntPtr[] tWorld2Cam,
            IntPtr[] rBase2Gripper,
            IntPtr[] tBase2Gripper,
            Mat rBase2World,
            Mat tBase2World,
            Mat rGripper2Cam,
            Mat tGripper2Cam,
            RobotWorldHandEyeCalibrationMethod method)
        {
            fixed (IntPtr* rWorld2CamPtr = rWorld2Cam)
            fixed (IntPtr* tWorld2CamPtr = tWorld2Cam)
            fixed (IntPtr* rBase2GripperPtr = rBase2Gripper)
            fixed (IntPtr* tBase2GripperPtr = tBase2Gripper)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DCalibrateRobotWorldHandEye(
                    rWorld2CamPtr,
                    tWorld2CamPtr,
                    rBase2GripperPtr,
                    tBase2GripperPtr,
                    rWorld2Cam.Length,
                    rBase2World.NativeHandle,
                    tBase2World.NativeHandle,
                    rGripper2Cam.NativeHandle,
                    tGripper2Cam.NativeHandle,
                    (int)method));
            }
        }

        private static IntPtr[] CreateTransformHandles(Mat[] values, bool rotation, string parameterName)
        {
            var handles = new IntPtr[values.Length];
            for (int i = 0; i < values.Length; ++i)
            {
                handles[i] = ValidateTransformAndGetHandle(values[i], rotation, parameterName);
            }

            return handles;
        }

#if NETCOREAPP3_1_OR_GREATER
        private static IntPtr[] CreateTransformHandles(ReadOnlySpan<Mat> values, bool rotation, string parameterName)
        {
            var handles = new IntPtr[values.Length];
            for (int i = 0; i < values.Length; ++i)
            {
                handles[i] = ValidateTransformAndGetHandle(values[i], rotation, parameterName);
            }

            return handles;
        }
#endif

        private static IntPtr ValidateTransformAndGetHandle(Mat value, bool rotation, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (value.Empty)
            {
                throw new ArgumentException("Pose matrices cannot be empty.", parameterName);
            }

            bool validShape = rotation
                ? (value.Rows == 3 && value.Cols == 3) || IsThreeElementVector(value)
                : IsThreeElementVector(value);
            if (!validShape)
            {
                throw new ArgumentException(
                    rotation
                        ? "Rotation matrices must be 3x3, 3x1, or 1x3."
                        : "Translation matrices must be 3x1 or 1x3.",
                    parameterName);
            }

            return value.NativeHandle;
        }

        private static bool IsThreeElementVector(Mat value)
        {
            return (value.Rows == 3 && value.Cols == 1)
                || (value.Rows == 1 && value.Cols == 3);
        }

        private static void ValidateHandEyeCollectionLengths(
            int firstCount,
            int secondCount,
            int thirdCount,
            int fourthCount,
            string firstParameterName,
            string secondParameterName,
            string thirdParameterName,
            string fourthParameterName)
        {
            if (firstCount < 3)
            {
                throw new ArgumentException("At least three poses are required.", firstParameterName);
            }

            if (secondCount != firstCount)
            {
                throw new ArgumentException("Pose collections must have the same length.", secondParameterName);
            }

            if (thirdCount != firstCount)
            {
                throw new ArgumentException("Pose collections must have the same length.", thirdParameterName);
            }

            if (fourthCount != firstCount)
            {
                throw new ArgumentException("Pose collections must have the same length.", fourthParameterName);
            }
        }

        private static void ValidateHandEyeMethod(HandEyeCalibrationMethod method)
        {
            if (method < HandEyeCalibrationMethod.Tsai || method > HandEyeCalibrationMethod.Daniilidis)
            {
                throw new ArgumentOutOfRangeException(nameof(method));
            }
        }

        private static void ValidateRobotWorldHandEyeMethod(RobotWorldHandEyeCalibrationMethod method)
        {
            if (method < RobotWorldHandEyeCalibrationMethod.Shah || method > RobotWorldHandEyeCalibrationMethod.Li)
            {
                throw new ArgumentOutOfRangeException(nameof(method));
            }
        }
    }
}
