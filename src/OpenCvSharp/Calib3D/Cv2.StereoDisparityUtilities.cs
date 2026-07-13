using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        /// <summary>
        /// Removes small connected speckle regions from an 8-bit or 16-bit disparity map in place.
        /// 原地移除 8 位或 16 位视差图中的小连通斑点区域。
        /// </summary>
        /// <param name="image">The single-channel disparity map modified in place. 原地修改的单通道视差图。</param>
        /// <param name="newValue">The replacement disparity value. 替换斑点时使用的视差值。</param>
        /// <param name="maxSpeckleSize">The maximum connected-region size to replace. 要替换的最大连通区域大小。</param>
        /// <param name="maxDifference">The maximum neighbor disparity difference within a region. 区域内相邻视差的最大差值。</param>
        /// <param name="buffer">
        /// Optional caller-owned temporary buffer that OpenCV may resize. OpenCV 可调整大小的可选调用方缓冲区。
        /// </param>
        /// <remarks>
        /// OpenCV rounds <paramref name="newValue"/> and <paramref name="maxDifference"/> to integer
        /// values. Fixed-point StereoBM/StereoSGBM disparities use a scale factor of 16.
        /// OpenCV 会将 <paramref name="newValue"/> 和 <paramref name="maxDifference"/> 四舍五入为整数。
        /// StereoBM/StereoSGBM 定点视差使用 16 倍缩放。
        /// </remarks>
        public static void FilterSpeckles(
            Mat image,
            double newValue,
            int maxSpeckleSize,
            double maxDifference,
            Mat? buffer = null)
        {
            ThrowIfNull(image, nameof(image));
            ValidateStereoSpeckleImage(image, nameof(image));
            ValidateFinite(newValue, nameof(newValue));
            ValidateFinite(maxDifference, nameof(maxDifference));

            IntPtr imageHandle = image.NativeHandle;
            IntPtr bufferHandle = GetNativeHandleOrZero(buffer);
            if (buffer != null &&
                (ReferenceEquals(image, buffer) || imageHandle == bufferHandle))
            {
                throw new ArgumentException(
                    "The disparity image and temporary buffer must not alias.",
                    nameof(buffer));
            }

            NativeException.ThrowIfError(NativeMethods.Calib3DFilterSpeckles(
                imageHandle,
                newValue,
                maxSpeckleSize,
                maxDifference,
                bufferHandle));
        }

        /// <summary>
        /// Computes the valid disparity ROI from two rectified-image ROIs.
        /// 根据两幅校正图像的 ROI 计算有效视差 ROI。
        /// </summary>
        public static Rect GetValidDisparityROI(
            Rect roi1,
            Rect roi2,
            int minDisparity,
            int numberOfDisparities,
            int blockSize)
        {
            NativeException.ThrowIfError(NativeMethods.Calib3DGetValidDisparityROI(
                roi1.X,
                roi1.Y,
                roi1.Width,
                roi1.Height,
                roi2.X,
                roi2.Y,
                roi2.Width,
                roi2.Height,
                minDisparity,
                numberOfDisparities,
                blockSize,
                out int x,
                out int y,
                out int width,
                out int height));
            return new Rect(x, y, width, height);
        }

        /// <summary>
        /// Validates a fixed-point disparity map in place using a left-right consistency check.
        /// 使用左右一致性检查原地验证定点视差图。
        /// </summary>
        /// <param name="disparity">The in-place <c>CV_16SC1</c> disparity map. 原地修改的 <c>CV_16SC1</c> 视差图。</param>
        /// <param name="cost">The unchanged <c>CV_16SC1</c> or <c>CV_32SC1</c> cost map. 保持不变的代价图。</param>
        /// <param name="minDisparity">The minimum disparity. 最小视差。</param>
        /// <param name="numberOfDisparities">The positive disparity range. 正的视差范围。</param>
        /// <param name="disp12MaxDifference">The maximum left-right disparity difference before fixed-point scaling. 定点缩放前允许的最大左右视差差值。</param>
        public static void ValidateDisparity(
            Mat disparity,
            Mat cost,
            int minDisparity,
            int numberOfDisparities,
            int disp12MaxDifference = 1)
        {
            ThrowIfNull(disparity, nameof(disparity));
            ThrowIfNull(cost, nameof(cost));
            ValidateStereoDisparityForConsistency(disparity, nameof(disparity));
            ValidateStereoCost(cost, nameof(cost));
            if (disparity.Rows != cost.Rows || disparity.Cols != cost.Cols)
            {
                throw new ArgumentException(
                    "Disparity and cost matrices must have identical dimensions.",
                    nameof(cost));
            }
            if (numberOfDisparities <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(numberOfDisparities),
                    "Number of disparities must be positive.");
            }

            IntPtr disparityHandle = disparity.NativeHandle;
            IntPtr costHandle = cost.NativeHandle;
            if (ReferenceEquals(disparity, cost) || disparityHandle == costHandle)
            {
                throw new ArgumentException(
                    "Disparity and cost matrices must not alias.",
                    nameof(cost));
            }

            NativeException.ThrowIfError(NativeMethods.Calib3DValidateDisparity(
                disparityHandle,
                costHandle,
                minDisparity,
                numberOfDisparities,
                disp12MaxDifference));
        }

        /// <summary>
        /// Reprojects a disparity image to a caller-owned three-channel 3D image.
        /// 将视差图重投影到调用方拥有的三通道三维图像。
        /// </summary>
        /// <param name="disparity">The single-channel disparity image. 单通道视差图。</param>
        /// <param name="image3D">The caller-owned output image. 调用方拥有的输出图像。</param>
        /// <param name="q">The single-channel <c>4 x 4</c> perspective transformation matrix. 单通道 <c>4 x 4</c> 透视变换矩阵。</param>
        /// <param name="handleMissingValues">Whether minimum-disparity values receive Z = 10000. 是否将最小视差值的 Z 设置为 10000。</param>
        /// <param name="ddepth">Output depth: -1, <c>CV_16S</c>, <c>CV_32S</c>, or <c>CV_32F</c>. 输出深度。</param>
        public static void ReprojectImageTo3D(
            Mat disparity,
            Mat image3D,
            Mat q,
            bool handleMissingValues = false,
            int ddepth = -1)
        {
            ThrowIfNull(disparity, nameof(disparity));
            ThrowIfNull(image3D, nameof(image3D));
            ThrowIfNull(q, nameof(q));
            ValidateReprojectDisparity(disparity, nameof(disparity));
            ValidateReprojectQ(q, nameof(q));
            ValidateReprojectDepth(ddepth, nameof(ddepth));

            IntPtr disparityHandle = disparity.NativeHandle;
            IntPtr outputHandle = image3D.NativeHandle;
            IntPtr qHandle = q.NativeHandle;
            if (ReferenceEquals(disparity, image3D) || disparityHandle == outputHandle)
            {
                throw new ArgumentException(
                    "The disparity input and 3D output matrices must not alias.",
                    nameof(image3D));
            }
            if (ReferenceEquals(q, image3D) || qHandle == outputHandle)
            {
                throw new ArgumentException(
                    "The Q matrix and 3D output matrix must not alias.",
                    nameof(image3D));
            }

            NativeException.ThrowIfError(NativeMethods.Calib3DReprojectImageTo3D(
                disparityHandle,
                outputHandle,
                qHandle,
                handleMissingValues ? 1 : 0,
                ddepth));
        }

        /// <summary>
        /// Reprojects a disparity image and returns an owned three-channel 3D image.
        /// 重投影视差图并返回拥有所有权的三通道三维图像。
        /// </summary>
        public static Mat ReprojectImageTo3D(
            Mat disparity,
            Mat q,
            bool handleMissingValues = false,
            int ddepth = -1)
        {
            var result = new Mat();
            try
            {
                ReprojectImageTo3D(disparity, result, q, handleMissingValues, ddepth);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        private static void ValidateStereoSpeckleImage(Mat value, string parameterName)
        {
            if (value.Empty)
            {
                throw new ArgumentException("Disparity image cannot be empty.", parameterName);
            }
            if (value.Type != MatType.CV_8UC1 && value.Type != MatType.CV_16SC1)
            {
                throw new ArgumentException(
                    "Speckle filtering requires CV_8UC1 or CV_16SC1.",
                    parameterName);
            }
        }

        private static void ValidateStereoDisparityForConsistency(Mat value, string parameterName)
        {
            if (value.Empty)
            {
                throw new ArgumentException("Disparity matrix cannot be empty.", parameterName);
            }
            if (value.Type != MatType.CV_16SC1)
            {
                throw new ArgumentException(
                    "Disparity validation requires CV_16SC1.",
                    parameterName);
            }
        }

        private static void ValidateStereoCost(Mat value, string parameterName)
        {
            if (value.Empty)
            {
                throw new ArgumentException("Cost matrix cannot be empty.", parameterName);
            }
            if (value.Type != MatType.CV_16SC1 && value.Type != MatType.CV_32SC1)
            {
                throw new ArgumentException(
                    "Cost matrix must be CV_16SC1 or CV_32SC1.",
                    parameterName);
            }
        }

        private static void ValidateReprojectDisparity(Mat value, string parameterName)
        {
            if (value.Empty)
            {
                throw new ArgumentException("Disparity matrix cannot be empty.", parameterName);
            }
            if (value.Type != MatType.CV_8UC1 &&
                value.Type != MatType.CV_16SC1 &&
                value.Type != MatType.CV_32SC1 &&
                value.Type != MatType.CV_32FC1)
            {
                throw new ArgumentException(
                    "Reprojection disparity must be CV_8UC1, CV_16SC1, CV_32SC1, or CV_32FC1.",
                    parameterName);
            }
        }

        private static void ValidateReprojectQ(Mat value, string parameterName)
        {
            if (value.Empty)
            {
                throw new ArgumentException("Q matrix cannot be empty.", parameterName);
            }
            if (value.Rows != 4 || value.Cols != 4 || value.Channels != 1)
            {
                throw new ArgumentException(
                    "Q matrix must be 4 x 4 and single-channel.",
                    parameterName);
            }
        }

        private static void ValidateReprojectDepth(int ddepth, string parameterName)
        {
            if (ddepth != -1 &&
                ddepth != MatType.CV_16S &&
                ddepth != MatType.CV_32S &&
                ddepth != MatType.CV_32F)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Output depth must be -1, CV_16S, CV_32S, or CV_32F.");
            }
        }
    }
}
