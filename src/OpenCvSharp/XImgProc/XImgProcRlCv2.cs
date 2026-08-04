using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.XImgProc
{
    /// <summary>
    /// Static entry points for ximgproc run-length morphology.
    /// ximgproc run-length morphology 静态入口。
    /// </summary>
    public static class XImgProcRlCv2
    {
        /// <summary>Thresholds a single-channel image into run-length encoding. 将单通道图像阈值化为 RLE 图像。</summary>
        public static void Threshold(Mat src, Mat rlDst, double thresh, ThresholdTypes type)
        {
            XImgProcCv2.ValidateNotNull(src, nameof(src));
            XImgProcCv2.ValidateNotNull(rlDst, nameof(rlDst));
            ValidateThresholdSource(src, nameof(src));
            ValidateThresholdType(type, nameof(type));
            NativeException.ThrowIfError(NativeMethods.XImgProcRlThreshold(src.NativeHandle, rlDst.NativeHandle, thresh, (int)type));
        }

        /// <summary>Thresholds and returns a new RLE matrix. 阈值化并返回新的 RLE 矩阵。</summary>
        public static Mat Threshold(Mat src, double thresh, ThresholdTypes type)
        {
            return CreateOutput(delegate (Mat dst) { Threshold(src, dst, thresh, type); });
        }

        /// <summary>Dilates an RLE binary image. 膨胀 RLE 二值图像。</summary>
        public static void Dilate(Mat rlSrc, Mat rlDst, Mat rlKernel, Point? anchor = null)
        {
            XImgProcCv2.ValidateNotNull(rlSrc, nameof(rlSrc));
            XImgProcCv2.ValidateNotNull(rlDst, nameof(rlDst));
            XImgProcCv2.ValidateNotNull(rlKernel, nameof(rlKernel));
            Point actualAnchor = anchor ?? new Point(0, 0);
            NativeException.ThrowIfError(NativeMethods.XImgProcRlDilate(rlSrc.NativeHandle, rlDst.NativeHandle, rlKernel.NativeHandle, actualAnchor.X, actualAnchor.Y));
        }

        /// <summary>Dilates and returns a new RLE matrix. 膨胀并返回新的 RLE 矩阵。</summary>
        public static Mat Dilate(Mat rlSrc, Mat rlKernel, Point? anchor = null)
        {
            return CreateOutput(delegate (Mat dst) { Dilate(rlSrc, dst, rlKernel, anchor); });
        }

        /// <summary>Erodes an RLE binary image. 腐蚀 RLE 二值图像。</summary>
        public static void Erode(Mat rlSrc, Mat rlDst, Mat rlKernel, bool boundaryOn = true, Point? anchor = null)
        {
            XImgProcCv2.ValidateNotNull(rlSrc, nameof(rlSrc));
            XImgProcCv2.ValidateNotNull(rlDst, nameof(rlDst));
            XImgProcCv2.ValidateNotNull(rlKernel, nameof(rlKernel));
            Point actualAnchor = anchor ?? new Point(0, 0);
            NativeException.ThrowIfError(NativeMethods.XImgProcRlErode(rlSrc.NativeHandle, rlDst.NativeHandle, rlKernel.NativeHandle, boundaryOn ? 1 : 0, actualAnchor.X, actualAnchor.Y));
        }

        /// <summary>Erodes and returns a new RLE matrix. 腐蚀并返回新的 RLE 矩阵。</summary>
        public static Mat Erode(Mat rlSrc, Mat rlKernel, bool boundaryOn = true, Point? anchor = null)
        {
            return CreateOutput(delegate (Mat dst) { Erode(rlSrc, dst, rlKernel, boundaryOn, anchor); });
        }

        /// <summary>Creates an RLE structuring element. 创建 RLE 结构元素。</summary>
        public static void GetStructuringElement(MorphShapes shape, Size ksize, Mat rlKernel)
        {
            XImgProcCv2.ValidateNotNull(rlKernel, nameof(rlKernel));
            NativeException.ThrowIfError(NativeMethods.XImgProcRlGetStructuringElement((int)shape, ksize.Width, ksize.Height, rlKernel.NativeHandle));
        }

        /// <summary>Creates and returns an RLE structuring element. 创建并返回 RLE 结构元素。</summary>
        public static Mat GetStructuringElement(MorphShapes shape, Size ksize)
        {
            return CreateOutput(delegate (Mat dst) { GetStructuringElement(shape, ksize, dst); });
        }

        /// <summary>Paints foreground RLE pixels into an image. 将 RLE 前景像素绘制到图像中。</summary>
        public static void Paint(Mat image, Mat rlSrc, Scalar value)
        {
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            XImgProcCv2.ValidateNotNull(rlSrc, nameof(rlSrc));
            NativeException.ThrowIfError(NativeMethods.XImgProcRlPaint(image.NativeHandle, rlSrc.NativeHandle, value.V0, value.V1, value.V2, value.V3));
        }

        /// <summary>Checks whether an RLE structuring element can be used for RLE morphology. 检查 RLE 结构元素是否可用于 RLE 形态学。</summary>
        public static bool IsRLMorphologyPossible(Mat rlStructuringElement)
        {
            XImgProcCv2.ValidateNotNull(rlStructuringElement, nameof(rlStructuringElement));
            NativeException.ThrowIfError(NativeMethods.XImgProcRlIsMorphologyPossible(rlStructuringElement.NativeHandle, out int value));
            return value != 0;
        }

        /// <summary>Creates an RLE image from run triples. 从 run 三元组创建 RLE 图像。</summary>
        public static void CreateRLEImage(Point3i[] runs, Size size, Mat dst)
        {
            XImgProcCv2.ValidateNotNull(dst, nameof(dst));
            if (runs == null)
            {
                throw new ArgumentNullException(nameof(runs));
            }

            if (runs.Length == 0)
            {
                throw new ArgumentException("At least one run is required.", nameof(runs));
            }

            NativeMethods.XImgProcPoint3iNative[] nativeRuns = ToNativeRuns(runs);
            NativeException.ThrowIfError(NativeMethods.XImgProcRlCreateRleImage(nativeRuns, nativeRuns.Length, size.Width, size.Height, dst.NativeHandle));
        }

        /// <summary>Creates and returns an RLE image from run triples. 从 run 三元组创建并返回 RLE 图像。</summary>
        public static Mat CreateRLEImage(Point3i[] runs, Size size)
        {
            var dst = new Mat();
            try
            {
                CreateRLEImage(runs, size, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Creates an RLE image from run triples. 从 run 三元组创建 RLE 图像。</summary>
        public static void CreateRLEImage(ReadOnlySpan<Point3i> runs, Size size, Mat dst)
        {
            XImgProcCv2.ValidateNotNull(dst, nameof(dst));
            if (runs.Length == 0)
            {
                throw new ArgumentException("At least one run is required.", nameof(runs));
            }

            NativeMethods.XImgProcPoint3iNative[] nativeRuns = ToNativeRuns(runs);
            NativeException.ThrowIfError(NativeMethods.XImgProcRlCreateRleImage(nativeRuns, nativeRuns.Length, size.Width, size.Height, dst.NativeHandle));
        }

        /// <summary>Creates and returns an RLE image from run triples. 从 run 三元组创建并返回 RLE 图像。</summary>
        public static Mat CreateRLEImage(ReadOnlySpan<Point3i> runs, Size size)
        {
            var dst = new Mat();
            try
            {
                CreateRLEImage(runs, size, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }
#endif

        /// <summary>Applies an RLE morphological operation. 执行 RLE 形态学操作。</summary>
        public static void MorphologyEx(Mat rlSrc, Mat rlDst, MorphTypes op, Mat rlKernel, bool boundaryOnForErosion = true, Point? anchor = null)
        {
            XImgProcCv2.ValidateNotNull(rlSrc, nameof(rlSrc));
            XImgProcCv2.ValidateNotNull(rlDst, nameof(rlDst));
            XImgProcCv2.ValidateNotNull(rlKernel, nameof(rlKernel));
            Point actualAnchor = anchor ?? new Point(0, 0);
            NativeException.ThrowIfError(NativeMethods.XImgProcRlMorphologyEx(rlSrc.NativeHandle, rlDst.NativeHandle, (int)op, rlKernel.NativeHandle, boundaryOnForErosion ? 1 : 0, actualAnchor.X, actualAnchor.Y));
        }

        /// <summary>Applies an RLE morphological operation and returns a new RLE matrix. 执行 RLE 形态学操作并返回新 RLE 矩阵。</summary>
        public static Mat MorphologyEx(Mat rlSrc, MorphTypes op, Mat rlKernel, bool boundaryOnForErosion = true, Point? anchor = null)
        {
            return CreateOutput(delegate (Mat dst) { MorphologyEx(rlSrc, dst, op, rlKernel, boundaryOnForErosion, anchor); });
        }

        private static NativeMethods.XImgProcPoint3iNative[] ToNativeRuns(Point3i[] runs)
        {
            var result = new NativeMethods.XImgProcPoint3iNative[runs.Length];
            for (int i = 0; i < runs.Length; i++)
            {
                Point3i run = runs[i];
                result[i] = new NativeMethods.XImgProcPoint3iNative
                {
                    X = run.X,
                    Y = run.Y,
                    Z = run.Z
                };
            }

            return result;
        }

        private static void ValidateThresholdSource(Mat src, string parameterName)
        {
            if (src.Empty)
            {
                throw new ArgumentException("RLE threshold source image must not be empty.", parameterName);
            }

            if (src.Channels != 1)
            {
                throw new ArgumentException("RLE threshold source image must have one channel.", parameterName);
            }
        }

        private static void ValidateThresholdType(ThresholdTypes type, string parameterName)
        {
            if (type != ThresholdTypes.Binary && type != ThresholdTypes.BinaryInv)
            {
                throw new ArgumentOutOfRangeException(parameterName, "RLE threshold type must be Binary or BinaryInv.");
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static NativeMethods.XImgProcPoint3iNative[] ToNativeRuns(ReadOnlySpan<Point3i> runs)
        {
            var result = new NativeMethods.XImgProcPoint3iNative[runs.Length];
            for (int i = 0; i < runs.Length; i++)
            {
                Point3i run = runs[i];
                result[i] = new NativeMethods.XImgProcPoint3iNative
                {
                    X = run.X,
                    Y = run.Y,
                    Z = run.Z
                };
            }

            return result;
        }
#endif

        private static Mat CreateOutput(Action<Mat> action)
        {
            var dst = new Mat();
            try
            {
                action(dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }
    }
}
