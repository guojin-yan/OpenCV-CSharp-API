using System;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Rapid
{
    /// <summary>
    /// Static helpers for the OpenCV rapid module.
    /// OpenCV rapid 模块的静态辅助方法。
    /// </summary>
    public static class RapidCv2
    {
        /// <summary>Draws matched correspondences onto a line bundle. 在 line bundle 上绘制匹配点。</summary>
        public static void DrawCorrespondencies(Mat bundle, Mat cols, Mat? colors = null)
        {
            ValidateNotNull(bundle, nameof(bundle));
            ValidateNotNull(cols, nameof(cols));
            ValidateMatType(cols, MatType.CV_32SC1, nameof(cols));
            ValidateMatRows(cols, bundle.Rows, nameof(cols));
            if (colors != null && !colors.Empty)
            {
                ValidateMatRows(colors, bundle.Rows, nameof(colors));
            }
            NativeException.ThrowIfError(NativeMethods.RapidDrawCorrespondencies(
                bundle.NativeHandle,
                cols.NativeHandle,
                colors == null ? IntPtr.Zero : colors.NativeHandle));
        }

        /// <summary>Draws search lines onto an image. 在图像上绘制搜索线。</summary>
        public static void DrawSearchLines(Mat img, Mat locations, Scalar color)
        {
            ValidateNotNull(img, nameof(img));
            ValidateNotNull(locations, nameof(locations));
            ValidateMatType(locations, MatType.CV_16SC2, nameof(locations));
            NativeException.ThrowIfError(NativeMethods.RapidDrawSearchLines(
                img.NativeHandle,
                locations.NativeHandle,
                color.V0,
                color.V1,
                color.V2,
                color.V3));
        }

        /// <summary>Draws a triangle mesh wireframe. 绘制三角网格线框。</summary>
        public static void DrawWireframe(Mat img, Mat pts2d, Mat tris, Scalar color, LineTypes lineType = LineTypes.Line8, bool cullBackface = false)
        {
            ValidateNotNull(img, nameof(img));
            ValidateNotNull(pts2d, nameof(pts2d));
            ValidateNotNull(tris, nameof(tris));
            ValidateMatVector(pts2d, 2, MatType.CV_32F, nameof(pts2d));
            ValidateMatVector(tris, 3, MatType.CV_32S, nameof(tris));
            NativeException.ThrowIfError(NativeMethods.RapidDrawWireframe(
                img.NativeHandle,
                pts2d.NativeHandle,
                tris.NativeHandle,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                (int)lineType,
                cullBackface ? 1 : 0));
        }

        /// <summary>Extracts projected control points from a mesh silhouette. 从网格轮廓投影提取控制点。</summary>
        public static void ExtractControlPoints(int num, int len, Mat pts3d, Mat rvec, Mat tvec, Mat cameraMatrix, Size imageSize, Mat tris, Mat ctl2d, Mat ctl3d)
        {
            ValidatePositive(num, nameof(num));
            ValidatePositive(len, nameof(len));
            ValidatePositiveSize(imageSize, nameof(imageSize));
            ValidateNotNull(pts3d, nameof(pts3d));
            ValidateNotNull(rvec, nameof(rvec));
            ValidateNotNull(tvec, nameof(tvec));
            ValidateNotNull(cameraMatrix, nameof(cameraMatrix));
            ValidateNotNull(tris, nameof(tris));
            ValidateNotNull(ctl2d, nameof(ctl2d));
            ValidateNotNull(ctl3d, nameof(ctl3d));
            NativeException.ThrowIfError(NativeMethods.RapidExtractControlPoints(
                num,
                len,
                pts3d.NativeHandle,
                rvec.NativeHandle,
                tvec.NativeHandle,
                cameraMatrix.NativeHandle,
                imageSize.Width,
                imageSize.Height,
                tris.NativeHandle,
                ctl2d.NativeHandle,
                ctl3d.NativeHandle));
        }

        /// <summary>Extracts a line bundle around control points. 提取控制点周围的 line bundle。</summary>
        public static void ExtractLineBundle(int len, Mat ctl2d, Mat img, Mat bundle, Mat srcLocations)
        {
            ValidatePositive(len, nameof(len));
            ValidateNotNull(ctl2d, nameof(ctl2d));
            ValidateNotNull(img, nameof(img));
            ValidateNotNull(bundle, nameof(bundle));
            ValidateNotNull(srcLocations, nameof(srcLocations));
            ValidateMatVector(ctl2d, 2, MatType.CV_32F, nameof(ctl2d));
            NativeException.ThrowIfError(NativeMethods.RapidExtractLineBundle(len, ctl2d.NativeHandle, img.NativeHandle, bundle.NativeHandle, srcLocations.NativeHandle));
        }

        /// <summary>Finds correspondencies along line bundle rows. 沿 line bundle 行查找匹配位置。</summary>
        public static void FindCorrespondencies(Mat bundle, Mat cols, Mat? response = null)
        {
            ValidateNotNull(bundle, nameof(bundle));
            ValidateNotNull(cols, nameof(cols));
            ValidateMatDepth(bundle, MatType.CV_8U, nameof(bundle));
            ValidateMatChannels(bundle, nameof(bundle), 1, 3);
            NativeException.ThrowIfError(NativeMethods.RapidFindCorrespondencies(
                bundle.NativeHandle,
                cols.NativeHandle,
                response == null ? IntPtr.Zero : response.NativeHandle));
        }

        /// <summary>Converts correspondence columns to 2D and optional 3D points. 将匹配列转换为 2D 和可选 3D 点。</summary>
        public static void ConvertCorrespondencies(Mat cols, Mat srcLocations, Mat pts2d, Mat? pts3d = null, Mat? mask = null)
        {
            ValidateNotNull(cols, nameof(cols));
            ValidateNotNull(srcLocations, nameof(srcLocations));
            ValidateNotNull(pts2d, nameof(pts2d));
            ValidateMatType(cols, MatType.CV_32SC1, nameof(cols));
            ValidateMatType(srcLocations, MatType.CV_16SC2, nameof(srcLocations));
            ValidateMatRows(srcLocations, cols.Rows, nameof(srcLocations));
            if (pts3d != null && !pts3d.Empty)
            {
                ValidateMatColumns(pts3d, cols.Rows, nameof(pts3d));
            }
            if (mask != null)
            {
                ValidateMatType(mask, MatType.CV_8UC1, nameof(mask));
                if (!mask.Empty)
                {
                    ValidateMatRows(mask, cols.Rows, nameof(mask));
                }
            }
            NativeException.ThrowIfError(NativeMethods.RapidConvertCorrespondencies(
                cols.NativeHandle,
                srcLocations.NativeHandle,
                pts2d.NativeHandle,
                pts3d == null ? IntPtr.Zero : pts3d.NativeHandle,
                mask == null ? IntPtr.Zero : mask.NativeHandle));
        }

        /// <summary>Runs one RAPID iteration and updates <paramref name="rvec"/> and <paramref name="tvec"/> in place. 运行一次 RAPID 迭代并原地更新 <paramref name="rvec"/> 与 <paramref name="tvec"/>。</summary>
        public static RapidResult Run(Mat img, int num, int len, Mat pts3d, Mat tris, Mat cameraMatrix, Mat rvec, Mat tvec, bool computeRmsd = false)
        {
            ValidateNotNull(img, nameof(img));
            ValidateAtLeast(num, 3, nameof(num));
            ValidatePositive(len, nameof(len));
            ValidateNotNull(pts3d, nameof(pts3d));
            ValidateNotNull(tris, nameof(tris));
            ValidateNotNull(cameraMatrix, nameof(cameraMatrix));
            ValidateNotNull(rvec, nameof(rvec));
            ValidateNotNull(tvec, nameof(tvec));
            NativeException.ThrowIfError(NativeMethods.RapidRun(
                img.NativeHandle,
                num,
                len,
                pts3d.NativeHandle,
                tris.NativeHandle,
                cameraMatrix.NativeHandle,
                rvec.NativeHandle,
                tvec.NativeHandle,
                computeRmsd ? 1 : 0,
                out float ratio,
                out double rmsd));
            return new RapidResult(ratio, computeRmsd ? (double?)rmsd : null);
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        internal static void ValidatePositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be positive.");
            }
        }

        internal static void ValidateAtLeast(int value, int minimum, string parameterName)
        {
            if (value < minimum)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be greater than or equal to " + minimum + ".");
            }
        }

        internal static void ValidateMatType(Mat value, int expectedType, string parameterName)
        {
            if (value.Type != expectedType)
            {
                throw new ArgumentException("Unexpected Mat type.", parameterName);
            }
        }

        internal static void ValidateMatDepth(Mat value, int expectedDepth, string parameterName)
        {
            if (value.Depth != expectedDepth)
            {
                throw new ArgumentException("Unexpected Mat depth.", parameterName);
            }
        }

        internal static void ValidateMatChannels(Mat value, string parameterName, params int[] allowedChannels)
        {
            int channels = value.Channels;
            for (int i = 0; i < allowedChannels.Length; i++)
            {
                if (channels == allowedChannels[i])
                {
                    return;
                }
            }

            throw new ArgumentException("Unexpected Mat channel count.", parameterName);
        }

        internal static void ValidateMatRows(Mat value, int expectedRows, string parameterName)
        {
            if (value.Rows != expectedRows)
            {
                throw new ArgumentException("Unexpected Mat row count.", parameterName);
            }
        }

        internal static void ValidateMatColumns(Mat value, int expectedColumns, string parameterName)
        {
            if (value.Cols != expectedColumns)
            {
                throw new ArgumentException("Unexpected Mat column count.", parameterName);
            }
        }

        internal static void ValidateMatVector(Mat value, int elementChannels, int depth, string parameterName)
        {
            bool hasVectorChannels = (value.Rows == 1 || value.Cols == 1) && value.Channels == elementChannels;
            bool hasVectorColumns = value.Cols == elementChannels && value.Channels == 1;
            if (value.Depth != depth || value.Rows <= 0 || value.Cols <= 0 || (!hasVectorChannels && !hasVectorColumns))
            {
                throw new ArgumentException("Unexpected Mat vector type or shape.", parameterName);
            }
        }

        internal static void ValidatePositiveSize(Size size, string parameterName)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Size dimensions must be positive.");
            }
        }
    }
}
