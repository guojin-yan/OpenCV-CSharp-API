using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    public static unsafe partial class Cv2
    {
        private const float PlanarCalibrationTolerance = 1.0e-6F;

        /// <summary>
        /// Estimates an initial camera intrinsic matrix from planar 3D-2D correspondences.
        /// 根据平面三维-二维对应点估计初始相机内参矩阵。
        /// </summary>
        /// <param name="objectPoints">Planar calibration target points grouped by view. 按视图分组的平面标定目标物点。</param>
        /// <param name="imagePoints">Projected image points grouped by view. 按视图分组的投影像点。</param>
        /// <param name="imageSize">Image size in pixels. 像素图像尺寸。</param>
        /// <param name="cameraMatrix">Writable output <c>3 x 3</c> camera matrix. 可写输出 <c>3 x 3</c> 相机矩阵。</param>
        /// <param name="aspectRatio">
        /// Positive values constrain <c>fx = fy * aspectRatio</c>; zero or negative values estimate both focal lengths independently.
        /// 正值约束 <c>fx = fy * aspectRatio</c>；零或负值独立估计两个焦距。
        /// </param>
        public static void InitCameraMatrix2D(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints,
            Size imageSize,
            Mat cameraMatrix,
            double aspectRatio = 1.0)
        {
            ThrowIfNull(cameraMatrix, nameof(cameraMatrix));
            ValidatePositiveSize(imageSize, nameof(imageSize));
            if (double.IsNaN(aspectRatio) || double.IsInfinity(aspectRatio))
            {
                throw new ArgumentOutOfRangeException(nameof(aspectRatio), "Aspect ratio must be finite.");
            }

            PrepareCalibrationPointGroups(
                objectPoints,
                imagePoints,
                nameof(objectPoints),
                nameof(imagePoints),
                out int[] objectOffsets,
                out NativeMethods.Calib3DPoint3fNative[] nativeObjectPoints,
                out int[] imageOffsets,
                out NativeMethods.Calib3DPoint2fNative[] nativeImagePoints);
            ValidatePlanarCalibrationPoints(objectPoints, nameof(objectPoints));

            fixed (int* objectOffsetsPtr = objectOffsets)
            fixed (NativeMethods.Calib3DPoint3fNative* objectPointsPtr = nativeObjectPoints)
            fixed (int* imageOffsetsPtr = imageOffsets)
            fixed (NativeMethods.Calib3DPoint2fNative* imagePointsPtr = nativeImagePoints)
            {
                NativeException.ThrowIfError(NativeMethods.Calib3DInitCameraMatrix2D(
                    objectOffsetsPtr,
                    objectPoints.Length,
                    objectPointsPtr,
                    nativeObjectPoints.Length,
                    imageOffsetsPtr,
                    imagePoints.Length,
                    imagePointsPtr,
                    nativeImagePoints.Length,
                    imageSize.Width,
                    imageSize.Height,
                    aspectRatio,
                    cameraMatrix.NativeHandle));
            }
        }

        /// <summary>
        /// Estimates and returns an owned initial camera intrinsic matrix from planar correspondences.
        /// 根据平面对应点估计并返回拥有所有权的初始相机内参矩阵。
        /// </summary>
        /// <remarks>
        /// The caller owns the returned matrix and must dispose it.
        /// 调用方拥有返回矩阵并负责释放。
        /// </remarks>
        public static Mat InitCameraMatrix2D(
            Point3f[][] objectPoints,
            Point2f[][] imagePoints,
            Size imageSize,
            double aspectRatio = 1.0)
        {
            var cameraMatrix = new Mat();
            try
            {
                InitCameraMatrix2D(objectPoints, imagePoints, imageSize, cameraMatrix, aspectRatio);
                return cameraMatrix;
            }
            catch
            {
                cameraMatrix.Dispose();
                throw;
            }
        }

        private static void ValidatePlanarCalibrationPoints(Point3f[][] objectPoints, string parameterName)
        {
            for (int group = 0; group < objectPoints.Length; ++group)
            {
                Point3f[] points = objectPoints[group];
                for (int point = 0; point < points.Length; ++point)
                {
                    if (Math.Abs(points[point].Z) > PlanarCalibrationTolerance)
                    {
                        throw new ArgumentException(
                            "InitCameraMatrix2D requires planar object points with Z coordinates near zero.",
                            parameterName);
                    }
                }
            }
        }
    }
}
