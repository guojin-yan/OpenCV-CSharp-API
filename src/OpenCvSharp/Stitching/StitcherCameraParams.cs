using System;
using System.Globalization;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Stitching
{
    /// <summary>
    /// Camera parameters returned by <see cref="Stitcher.GetCameras"/>.
    /// <see cref="Stitcher.GetCameras"/> 返回的相机参数。
    /// </summary>
    public sealed class StitcherCameraParams
    {
        /// <summary>
        /// Initializes a camera parameter object.
        /// 初始化相机参数对象。
        /// </summary>
        public StitcherCameraParams(double focal, double aspect, double ppx, double ppy, Mat rotation, Mat translation)
        {
            if (rotation == null)
            {
                throw new ArgumentNullException(nameof(rotation));
            }

            if (translation == null)
            {
                throw new ArgumentNullException(nameof(translation));
            }

            Focal = focal;
            Aspect = aspect;
            PrincipalPointX = ppx;
            PrincipalPointY = ppy;
            Rotation = rotation;
            Translation = translation;
        }

        /// <summary>
        /// Initializes camera parameters by copying another instance.
        /// 通过复制另一个实例初始化相机参数。
        /// </summary>
        /// <param name="other">The parameters to copy. 要复制的参数。</param>
        public StitcherCameraParams(StitcherCameraParams other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Focal = other.Focal;
            Aspect = other.Aspect;
            PrincipalPointX = other.PrincipalPointX;
            PrincipalPointY = other.PrincipalPointY;
            Rotation = other.Rotation;
            Translation = other.Translation;
        }

        /// <summary>Gets focal length. 获取焦距。</summary>
        public double Focal { get; }

        /// <summary>Gets aspect ratio. 获取宽高比。</summary>
        public double Aspect { get; }

        /// <summary>Gets principal point X. 获取主点 X 坐标。</summary>
        public double PrincipalPointX { get; }

        /// <summary>Gets principal point Y. 获取主点 Y 坐标。</summary>
        public double PrincipalPointY { get; }

        /// <summary>Gets rotation matrix. 获取旋转矩阵。</summary>
        public Mat Rotation { get; }

        /// <summary>Gets translation vector. 获取平移向量。</summary>
        public Mat Translation { get; }

        /// <summary>
        /// Creates a shallow copy of this camera parameter object.
        /// 创建此相机参数对象的浅拷贝。
        /// </summary>
        /// <remarks>
        /// Matrix references are preserved; ownership remains with the caller.
        /// 矩阵引用会被保留；所有权仍由调用方持有。
        /// </remarks>
        public StitcherCameraParams Clone()
        {
            return new StitcherCameraParams(this);
        }

        /// <summary>Copies the 3 x 3 CV_64FC1 intrinsic matrix into caller-owned storage.</summary>
        public void GetCameraMatrix(Mat cameraMatrix)
        {
            if (cameraMatrix == null) throw new ArgumentNullException(nameof(cameraMatrix));
            NativeException.ThrowIfError(NativeMethods.StitchingCameraParamsGetK(
                Focal, Aspect, PrincipalPointX, PrincipalPointY, cameraMatrix.NativeHandle));
            GC.KeepAlive(cameraMatrix);
        }

        /// <summary>Returns an independently owned 3 x 3 CV_64FC1 intrinsic matrix.</summary>
        public Mat GetCameraMatrix()
        {
            var result = new Mat();
            try
            {
                GetCameraMatrix(result);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{Focal={0},Aspect={1},PrincipalPointX={2},PrincipalPointY={3},Rotation={4}x{5},Translation={6}x{7}}}",
                Focal,
                Aspect,
                PrincipalPointX,
                PrincipalPointY,
                Rotation.Rows,
                Rotation.Cols,
                Translation.Rows,
                Translation.Cols);
        }
    }
}
