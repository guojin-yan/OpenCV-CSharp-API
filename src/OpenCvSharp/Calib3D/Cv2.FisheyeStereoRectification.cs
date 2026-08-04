using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        /// <summary>Computes stereo rectification for the fisheye camera model. 计算鱼眼相机模型的双目校正。</summary>
        public static void FisheyeStereoRectify(
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Size imageSize,
            Mat r,
            Mat t,
            Mat r1,
            Mat r2,
            Mat p1,
            Mat p2,
            Mat q,
            StereoRectifyFlags flags = StereoRectifyFlags.ZeroDisparity,
            Size? newImageSize = null,
            double balance = 0.0,
            double fovScale = 1.0)
        {
            ThrowIfNull(cameraMatrix1, nameof(cameraMatrix1));
            ThrowIfNull(distCoeffs1, nameof(distCoeffs1));
            ThrowIfNull(cameraMatrix2, nameof(cameraMatrix2));
            ThrowIfNull(distCoeffs2, nameof(distCoeffs2));
            ThrowIfNull(r, nameof(r));
            ThrowIfNull(t, nameof(t));
            ThrowIfNull(r1, nameof(r1));
            ThrowIfNull(r2, nameof(r2));
            ThrowIfNull(p1, nameof(p1));
            ThrowIfNull(p2, nameof(p2));
            ThrowIfNull(q, nameof(q));
            if (imageSize.Width <= 0 || imageSize.Height <= 0)
                throw new ArgumentOutOfRangeException(nameof(imageSize));
            Size outputSize = newImageSize ?? new Size(0, 0);
            if (outputSize.Width < 0 || outputSize.Height < 0)
                throw new ArgumentOutOfRangeException(nameof(newImageSize));
            if (double.IsNaN(balance) || double.IsInfinity(balance) || balance < 0.0 || balance > 1.0)
                throw new ArgumentOutOfRangeException(nameof(balance));
            if (double.IsNaN(fovScale) || double.IsInfinity(fovScale) || fovScale <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(fovScale));
            ValidateDistinctOutputSet(
                new[] { r1, r2, p1, p2, q },
                new[] { nameof(r1), nameof(r2), nameof(p1), nameof(p2), nameof(q) });

            NativeException.ThrowIfError(NativeMethods.Calib3DFisheyeStereoRectify(
                cameraMatrix1.NativeHandle,
                distCoeffs1.NativeHandle,
                cameraMatrix2.NativeHandle,
                distCoeffs2.NativeHandle,
                imageSize.Width,
                imageSize.Height,
                r.NativeHandle,
                t.NativeHandle,
                r1.NativeHandle,
                r2.NativeHandle,
                p1.NativeHandle,
                p2.NativeHandle,
                q.NativeHandle,
                (int)flags,
                outputSize.Width,
                outputSize.Height,
                balance,
                fovScale));
        }

        /// <summary>Computes fisheye stereo rectification and returns owned matrices. 计算鱼眼双目校正并返回拥有所有权的矩阵。</summary>
        public static FisheyeStereoRectifyResult FisheyeStereoRectify(
            Mat cameraMatrix1,
            Mat distCoeffs1,
            Mat cameraMatrix2,
            Mat distCoeffs2,
            Size imageSize,
            Mat r,
            Mat t,
            StereoRectifyFlags flags = StereoRectifyFlags.ZeroDisparity,
            Size? newImageSize = null,
            double balance = 0.0,
            double fovScale = 1.0)
        {
            var r1 = new Mat();
            var r2 = new Mat();
            var p1 = new Mat();
            var p2 = new Mat();
            var q = new Mat();
            try
            {
                FisheyeStereoRectify(
                    cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, imageSize,
                    r, t, r1, r2, p1, p2, q, flags, newImageSize, balance, fovScale);
                return new FisheyeStereoRectifyResult(r1, r2, p1, p2, q);
            }
            catch
            {
                r1.Dispose();
                r2.Dispose();
                p1.Dispose();
                p2.Dispose();
                q.Dispose();
                throw;
            }
        }
    }
}
