using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.PtCloud
{
    /// <summary>
    /// Entry points for OpenCV ptcloud depth and RGB-D utilities.
    /// OpenCV ptcloud 深度与 RGB-D 工具入口。
    /// </summary>
    public static class PtCloudCv2
    {
        /// <summary>
        /// Registers depth data to another camera plane.
        /// 将深度数据注册到另一个相机平面。
        /// </summary>
        public static void RegisterDepth(
            Mat unregisteredCameraMatrix,
            Mat registeredCameraMatrix,
            Mat registeredDistCoeffs,
            Mat rt,
            Mat unregisteredDepth,
            Size outputImagePlaneSize,
            Mat registeredDepth,
            bool depthDilation = false)
        {
            ValidateNotNull(unregisteredCameraMatrix, nameof(unregisteredCameraMatrix));
            ValidateNotNull(registeredCameraMatrix, nameof(registeredCameraMatrix));
            ValidateNotNull(registeredDistCoeffs, nameof(registeredDistCoeffs));
            ValidateNotNull(rt, nameof(rt));
            ValidateNotNull(unregisteredDepth, nameof(unregisteredDepth));
            ValidateNotNull(registeredDepth, nameof(registeredDepth));
            NativeException.ThrowIfError(NativeMethods.PtCloudRegisterDepth(
                unregisteredCameraMatrix.NativeHandle,
                registeredCameraMatrix.NativeHandle,
                registeredDistCoeffs.NativeHandle,
                rt.NativeHandle,
                unregisteredDepth.NativeHandle,
                outputImagePlaneSize.Width,
                outputImagePlaneSize.Height,
                registeredDepth.NativeHandle,
                depthDilation ? 1 : 0));
        }

        /// <summary>
        /// Converts a depth image to organized 3D points.
        /// 将深度图转换为有组织的 3D 点。
        /// </summary>
        public static void DepthTo3d(Mat depth, Mat cameraMatrix, Mat points3d, Mat? mask = null)
        {
            ValidateNotNull(depth, nameof(depth));
            ValidateNotNull(cameraMatrix, nameof(cameraMatrix));
            ValidateNotNull(points3d, nameof(points3d));
            NativeException.ThrowIfError(NativeMethods.PtCloudDepthTo3d(
                depth.NativeHandle,
                cameraMatrix.NativeHandle,
                points3d.NativeHandle,
                mask == null ? IntPtr.Zero : mask.NativeHandle));
        }

        /// <summary>
        /// Converts a depth image to organized 3D points and returns a new matrix.
        /// 将深度图转换为有组织的 3D 点并返回新矩阵。
        /// </summary>
        public static Mat DepthTo3d(Mat depth, Mat cameraMatrix, Mat? mask = null)
        {
            var points3d = new Mat();
            try
            {
                DepthTo3d(depth, cameraMatrix, points3d, mask);
                return points3d;
            }
            catch
            {
                points3d.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Converts selected depth pixels to sparse 3D points.
        /// 将选定深度像素转换为稀疏 3D 点。
        /// </summary>
        public static void DepthTo3dSparse(Mat depth, Mat cameraMatrix, Mat points, Mat points3d)
        {
            ValidateNotNull(depth, nameof(depth));
            ValidateNotNull(cameraMatrix, nameof(cameraMatrix));
            ValidateNotNull(points, nameof(points));
            ValidateNotNull(points3d, nameof(points3d));
            NativeException.ThrowIfError(NativeMethods.PtCloudDepthTo3dSparse(depth.NativeHandle, cameraMatrix.NativeHandle, points.NativeHandle, points3d.NativeHandle));
        }

        /// <summary>
        /// Converts selected depth pixels to sparse 3D points and returns a new matrix.
        /// 将选定深度像素转换为稀疏 3D 点并返回新矩阵。
        /// </summary>
        public static Mat DepthTo3dSparse(Mat depth, Mat cameraMatrix, Mat points)
        {
            var points3d = new Mat();
            try
            {
                DepthTo3dSparse(depth, cameraMatrix, points, points3d);
                return points3d;
            }
            catch
            {
                points3d.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Rescales a depth image to floating-point depth units.
        /// 将深度图缩放为浮点深度单位。
        /// </summary>
        public static void RescaleDepth(Mat src, int type, Mat dst, double depthFactor = 1000.0)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.PtCloudRescaleDepth(src.NativeHandle, type, dst.NativeHandle, depthFactor));
        }

        /// <summary>
        /// Rescales a depth image and returns a new matrix.
        /// 缩放深度图并返回新矩阵。
        /// </summary>
        public static Mat RescaleDepth(Mat src, int type = MatType.CV_32F, double depthFactor = 1000.0)
        {
            var dst = new Mat();
            try
            {
                RescaleDepth(src, type, dst, depthFactor);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Warps an RGB-D frame by a camera transform.
        /// 使用相机变换扭曲 RGB-D 帧。
        /// </summary>
        public static void WarpFrame(
            Mat depth,
            Mat? image,
            Mat? mask,
            Mat rt,
            Mat cameraMatrix,
            Mat? warpedDepth = null,
            Mat? warpedImage = null,
            Mat? warpedMask = null)
        {
            ValidateNotNull(depth, nameof(depth));
            ValidateNotNull(rt, nameof(rt));
            ValidateNotNull(cameraMatrix, nameof(cameraMatrix));
            NativeException.ThrowIfError(NativeMethods.PtCloudWarpFrame(
                depth.NativeHandle,
                image == null ? IntPtr.Zero : image.NativeHandle,
                mask == null ? IntPtr.Zero : mask.NativeHandle,
                rt.NativeHandle,
                cameraMatrix.NativeHandle,
                warpedDepth == null ? IntPtr.Zero : warpedDepth.NativeHandle,
                warpedImage == null ? IntPtr.Zero : warpedImage.NativeHandle,
                warpedMask == null ? IntPtr.Zero : warpedMask.NativeHandle));
        }

        /// <summary>
        /// Finds planes from organized points and optional normals.
        /// 从有组织的点和可选法线中查找平面。
        /// </summary>
        public static void FindPlanes(
            Mat points3d,
            Mat? normals,
            Mat mask,
            Mat planeCoefficients,
            int blockSize = 40,
            int minSize = 1600,
            double threshold = 0.01,
            double sensorErrorA = 0,
            double sensorErrorB = 0,
            double sensorErrorC = 0,
            RgbdPlaneMethod method = RgbdPlaneMethod.Default)
        {
            ValidateNotNull(points3d, nameof(points3d));
            ValidateNotNull(mask, nameof(mask));
            ValidateNotNull(planeCoefficients, nameof(planeCoefficients));
            NativeException.ThrowIfError(NativeMethods.PtCloudFindPlanes(
                points3d.NativeHandle,
                normals == null ? IntPtr.Zero : normals.NativeHandle,
                mask.NativeHandle,
                planeCoefficients.NativeHandle,
                blockSize,
                minSize,
                threshold,
                sensorErrorA,
                sensorErrorB,
                sensorErrorC,
                (int)method));
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
