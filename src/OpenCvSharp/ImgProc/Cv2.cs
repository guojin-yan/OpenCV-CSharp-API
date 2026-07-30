using System;
using System.Text;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;
using OpenCvSharp.Geometry;
using OpenCvSharp.Internal.Interop;

#if NETCOREAPP3_1_OR_GREATER
using System.Buffers;
#endif

namespace OpenCvSharp.ImgProc
{
    /// <summary>
    /// Provides image processing functions aligned with OpenCV <c>cv</c> free functions.
    /// 提供与 OpenCV <c>cv</c> 自由函数对齐的图像处理函数。
    /// </summary>
    public static partial class Cv2
    {
#if NETCOREAPP3_1_OR_GREATER
        private const int StackallocInterleavedPointPairThreshold = 32;
        private const int StackallocInterleavedFloatPairThreshold = 32;
#endif

        /// <summary>
        /// Converts an image from one color space to another.
        /// 将图像从一种色彩空间转换为另一种色彩空间。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="code">The color conversion code. 颜色转换代码。</param>
        /// <param name="dstCn">The number of channels in the destination image, or 0 to derive it automatically. 目标图像通道数，0 表示自动推导。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void CvtColor(Mat src, Mat dst, ColorConversionCodes code, int dstCn = 0)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (dst == null)
            {
                throw new ArgumentNullException(nameof(dst));
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcCvtColor(src.NativeHandle, dst.NativeHandle, (int)code, dstCn));
        }

        /// <summary>
        /// Resizes an image to an explicit size or by scale factors.
        /// 按显式尺寸或缩放因子调整图像大小。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="dsize">The output image size, or an empty size when using scale factors. 输出图像尺寸，使用缩放因子时可传入空尺寸。</param>
        /// <param name="fx">The horizontal scale factor. 水平方向缩放因子。</param>
        /// <param name="fy">The vertical scale factor. 垂直方向缩放因子。</param>
        /// <param name="interpolation">The interpolation algorithm. 插值算法。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dsize"/> contains a negative dimension. 当 <paramref name="dsize"/> 包含负数维度时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Resize(
            Mat src,
            Mat dst,
            Size dsize,
            double fx = 0,
            double fy = 0,
            InterpolationFlags interpolation = InterpolationFlags.Linear)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (dst == null)
            {
                throw new ArgumentNullException(nameof(dst));
            }

            if (dsize.Width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dsize), "Width cannot be negative.");
            }

            if (dsize.Height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dsize), "Height cannot be negative.");
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcResize(
                src.NativeHandle,
                dst.NativeHandle,
                dsize.Width,
                dsize.Height,
                fx,
                fy,
                (int)interpolation));
        }

        /// <summary>
        /// Applies a fixed-level threshold to each array element.
        /// 对数组中的每个元素应用固定级别阈值处理。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="thresh">The threshold value. 阈值。</param>
        /// <param name="maxval">The maximum value used with binary thresholding modes. 二值阈值模式使用的最大值。</param>
        /// <param name="type">The thresholding type. 阈值处理类型。</param>
        /// <returns>The computed threshold value. 计算得到的阈值。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static double Threshold(Mat src, Mat dst, double thresh, double maxval, ThresholdTypes type)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (dst == null)
            {
                throw new ArgumentNullException(nameof(dst));
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcThreshold(
                src.NativeHandle,
                dst.NativeHandle,
                thresh,
                maxval,
                (int)type,
                out double threshold));

            return threshold;
        }

        /// <summary>
        /// Blurs an image using a Gaussian filter.
        /// 使用高斯滤波器模糊图像。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="ksize">The Gaussian kernel size. 高斯核尺寸。</param>
        /// <param name="sigmaX">The Gaussian kernel standard deviation in the X direction. X 方向高斯核标准差。</param>
        /// <param name="sigmaY">The Gaussian kernel standard deviation in the Y direction. Y 方向高斯核标准差。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ksize"/> contains a negative dimension. 当 <paramref name="ksize"/> 包含负数维度时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void GaussianBlur(
            Mat src,
            Mat dst,
            Size ksize,
            double sigmaX,
            double sigmaY = 0,
            BorderTypes borderType = BorderTypes.Default)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (dst == null)
            {
                throw new ArgumentNullException(nameof(dst));
            }

            if (ksize.Width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ksize), "Width cannot be negative.");
            }

            if (ksize.Height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ksize), "Height cannot be negative.");
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcGaussianBlur(
                src.NativeHandle,
                dst.NativeHandle,
                ksize.Width,
                ksize.Height,
                sigmaX,
                sigmaY,
                (int)borderType));
        }

        /// <summary>
        /// Blurs an image using a normalized box filter.
        /// 使用归一化盒式滤波器模糊图像。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="ksize">The blurring kernel size. 模糊核尺寸。</param>
        /// <param name="anchor">The anchor position; null means the kernel center. 锚点位置；null 表示核中心。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ksize"/> contains a non-positive dimension. 当 <paramref name="ksize"/> 包含非正维度时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Blur(
            Mat src,
            Mat dst,
            Size ksize,
            Point? anchor = null,
            BorderTypes borderType = BorderTypes.Default)
        {
            Point actualAnchor = anchor ?? new Point(-1, -1);
            ValidateMatPair(src, dst);
            ValidatePositiveSize(ksize, nameof(ksize));

            NativeException.ThrowIfError(NativeMethods.ImgProcBlur(
                src.NativeHandle,
                dst.NativeHandle,
                ksize.Width,
                ksize.Height,
                actualAnchor.X,
                actualAnchor.Y,
                (int)borderType));
        }

        /// <summary>
        /// Blurs an image using a box filter.
        /// 使用盒式滤波器模糊图像。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="ddepth">The destination depth, or -1 to use the source depth. 目标深度，-1 表示使用源图像深度。</param>
        /// <param name="ksize">The box kernel size. 盒式核尺寸。</param>
        /// <param name="anchor">The anchor position; null means the kernel center. 锚点位置；null 表示核中心。</param>
        /// <param name="normalize">Whether to normalize the kernel by its area. 是否按核面积归一化。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ksize"/> contains a non-positive dimension. 当 <paramref name="ksize"/> 包含非正维度时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void BoxFilter(
            Mat src,
            Mat dst,
            int ddepth,
            Size ksize,
            Point? anchor = null,
            bool normalize = true,
            BorderTypes borderType = BorderTypes.Default)
        {
            Point actualAnchor = anchor ?? new Point(-1, -1);
            ValidateMatPair(src, dst);
            ValidatePositiveSize(ksize, nameof(ksize));

            NativeException.ThrowIfError(NativeMethods.ImgProcBoxFilter(
                src.NativeHandle,
                dst.NativeHandle,
                ddepth,
                ksize.Width,
                ksize.Height,
                actualAnchor.X,
                actualAnchor.Y,
                normalize ? 1 : 0,
                (int)borderType));
        }

        /// <summary>
        /// Computes the box-filtered sum of squared pixel values.
        /// 计算像素平方值的盒式滤波和。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="ddepth">The destination depth, or -1 to use the source depth. 目标深度，-1 表示使用源图像深度。</param>
        /// <param name="ksize">The box kernel size. 盒式核尺寸。</param>
        /// <param name="anchor">The anchor position; null means the kernel center. 锚点位置；null 表示核中心。</param>
        /// <param name="normalize">Whether to normalize the result. 是否归一化结果。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ksize"/> contains a non-positive dimension. 当 <paramref name="ksize"/> 包含非正维度时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void SqrBoxFilter(
            Mat src,
            Mat dst,
            int ddepth,
            Size ksize,
            Point? anchor = null,
            bool normalize = true,
            BorderTypes borderType = BorderTypes.Default)
        {
            Point actualAnchor = anchor ?? new Point(-1, -1);
            ValidateMatPair(src, dst);
            ValidatePositiveSize(ksize, nameof(ksize));

            NativeException.ThrowIfError(NativeMethods.ImgProcSqrBoxFilter(
                src.NativeHandle,
                dst.NativeHandle,
                ddepth,
                ksize.Width,
                ksize.Height,
                actualAnchor.X,
                actualAnchor.Y,
                normalize ? 1 : 0,
                (int)borderType));
        }

        /// <summary>
        /// Blurs an image using a median filter.
        /// 使用中值滤波器模糊图像。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="ksize">The odd aperture size greater than one. 大于一的奇数孔径尺寸。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ksize"/> is not an odd value greater than one. 当 <paramref name="ksize"/> 不是大于一的奇数时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void MedianBlur(Mat src, Mat dst, int ksize)
        {
            ValidateMatPair(src, dst);
            ValidateOddGreaterThanOne(ksize, nameof(ksize));

            NativeException.ThrowIfError(NativeMethods.ImgProcMedianBlur(src.NativeHandle, dst.NativeHandle, ksize));
        }

        /// <summary>
        /// Applies bilateral filtering while preserving edges.
        /// 应用保边双边滤波。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="d">The pixel neighborhood diameter, or non-positive to derive from sigmaSpace. 像素邻域直径，非正值表示根据 sigmaSpace 推导。</param>
        /// <param name="sigmaColor">The filter sigma in color space. 颜色空间滤波 sigma。</param>
        /// <param name="sigmaSpace">The filter sigma in coordinate space. 坐标空间滤波 sigma。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void BilateralFilter(
            Mat src,
            Mat dst,
            int d,
            double sigmaColor,
            double sigmaSpace,
            BorderTypes borderType = BorderTypes.Default)
        {
            ValidateMatPair(src, dst);

            NativeException.ThrowIfError(NativeMethods.ImgProcBilateralFilter(
                src.NativeHandle,
                dst.NativeHandle,
                d,
                sigmaColor,
                sigmaSpace,
                (int)borderType));
        }

        /// <summary>
        /// Applies an arbitrary linear filter to an image.
        /// 对图像应用任意线性滤波器。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="ddepth">The destination depth, or -1 to use the source depth. 目标深度，-1 表示使用源图像深度。</param>
        /// <param name="kernel">The filter kernel matrix. 滤波核矩阵。</param>
        /// <param name="anchor">The anchor position; null means the kernel center. 锚点位置；null 表示核中心。</param>
        /// <param name="delta">The value added to filtered pixels. 滤波后添加到像素的值。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when a required matrix is null. 当必需矩阵为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Filter2D(
            Mat src,
            Mat dst,
            int ddepth,
            Mat kernel,
            Point? anchor = null,
            double delta = 0,
            BorderTypes borderType = BorderTypes.Default)
        {
            Point actualAnchor = anchor ?? new Point(-1, -1);
            ValidateMatPair(src, dst);
            ValidateNotNull(kernel, nameof(kernel));

            NativeException.ThrowIfError(NativeMethods.ImgProcFilter2D(
                src.NativeHandle,
                dst.NativeHandle,
                ddepth,
                kernel.NativeHandle,
                actualAnchor.X,
                actualAnchor.Y,
                delta,
                (int)borderType));
        }

        /// <summary>
        /// Applies a separable linear filter to an image.
        /// 对图像应用可分离线性滤波器。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="ddepth">The destination depth, or -1 to use the source depth. 目标深度，-1 表示使用源图像深度。</param>
        /// <param name="kernelX">The row filter coefficients. 行方向滤波系数。</param>
        /// <param name="kernelY">The column filter coefficients. 列方向滤波系数。</param>
        /// <param name="anchor">The anchor position; null means the kernel center. 锚点位置；null 表示核中心。</param>
        /// <param name="delta">The value added to filtered pixels. 滤波后添加到像素的值。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when a required matrix is null. 当必需矩阵为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void SepFilter2D(
            Mat src,
            Mat dst,
            int ddepth,
            Mat kernelX,
            Mat kernelY,
            Point? anchor = null,
            double delta = 0,
            BorderTypes borderType = BorderTypes.Default)
        {
            Point actualAnchor = anchor ?? new Point(-1, -1);
            ValidateMatPair(src, dst);
            ValidateNotNull(kernelX, nameof(kernelX));
            ValidateNotNull(kernelY, nameof(kernelY));

            NativeException.ThrowIfError(NativeMethods.ImgProcSepFilter2D(
                src.NativeHandle,
                dst.NativeHandle,
                ddepth,
                kernelX.NativeHandle,
                kernelY.NativeHandle,
                actualAnchor.X,
                actualAnchor.Y,
                delta,
                (int)borderType));
        }

        /// <summary>
        /// Calculates image derivatives using the Sobel operator.
        /// 使用 Sobel 算子计算图像导数。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="ddepth">The destination depth. 目标深度。</param>
        /// <param name="dx">The derivative order in x. x 方向导数阶数。</param>
        /// <param name="dy">The derivative order in y. y 方向导数阶数。</param>
        /// <param name="ksize">The Sobel kernel size. Sobel 核尺寸。</param>
        /// <param name="scale">The scale factor for derivatives. 导数缩放因子。</param>
        /// <param name="delta">The value added to results. 添加到结果的值。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Sobel(
            Mat src,
            Mat dst,
            int ddepth,
            int dx,
            int dy,
            int ksize = 3,
            double scale = 1,
            double delta = 0,
            BorderTypes borderType = BorderTypes.Default)
        {
            ValidateMatPair(src, dst);

            NativeException.ThrowIfError(NativeMethods.ImgProcSobel(
                src.NativeHandle,
                dst.NativeHandle,
                ddepth,
                dx,
                dy,
                ksize,
                scale,
                delta,
                (int)borderType));
        }

        /// <summary>
        /// Calculates image derivatives using the Scharr operator.
        /// 使用 Scharr 算子计算图像导数。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="ddepth">The destination depth. 目标深度。</param>
        /// <param name="dx">The derivative order in x. x 方向导数阶数。</param>
        /// <param name="dy">The derivative order in y. y 方向导数阶数。</param>
        /// <param name="scale">The scale factor for derivatives. 导数缩放因子。</param>
        /// <param name="delta">The value added to results. 添加到结果的值。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Scharr(
            Mat src,
            Mat dst,
            int ddepth,
            int dx,
            int dy,
            double scale = 1,
            double delta = 0,
            BorderTypes borderType = BorderTypes.Default)
        {
            ValidateMatPair(src, dst);

            NativeException.ThrowIfError(NativeMethods.ImgProcScharr(
                src.NativeHandle,
                dst.NativeHandle,
                ddepth,
                dx,
                dy,
                scale,
                delta,
                (int)borderType));
        }

        /// <summary>
        /// Calculates the Laplacian of an image.
        /// 计算图像的拉普拉斯变换。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="ddepth">The destination depth. 目标深度。</param>
        /// <param name="ksize">The aperture size. 孔径尺寸。</param>
        /// <param name="scale">The scale factor. 缩放因子。</param>
        /// <param name="delta">The value added to results. 添加到结果的值。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Laplacian(
            Mat src,
            Mat dst,
            int ddepth,
            int ksize = 1,
            double scale = 1,
            double delta = 0,
            BorderTypes borderType = BorderTypes.Default)
        {
            ValidateMatPair(src, dst);

            NativeException.ThrowIfError(NativeMethods.ImgProcLaplacian(
                src.NativeHandle,
                dst.NativeHandle,
                ddepth,
                ksize,
                scale,
                delta,
                (int)borderType));
        }

        /// <summary>
        /// Finds edges using the Canny algorithm.
        /// 使用 Canny 算法查找边缘。
        /// </summary>
        /// <param name="image">The 8-bit source image. 8 位源图像。</param>
        /// <param name="edges">The output edge map. 输出边缘图。</param>
        /// <param name="threshold1">The first hysteresis threshold. 第一个滞后阈值。</param>
        /// <param name="threshold2">The second hysteresis threshold. 第二个滞后阈值。</param>
        /// <param name="apertureSize">The Sobel aperture size. Sobel 孔径尺寸。</param>
        /// <param name="l2Gradient">Whether to use the L2 gradient norm. 是否使用 L2 梯度范数。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> or <paramref name="edges"/> is null. 当 <paramref name="image"/> 或 <paramref name="edges"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Canny(
            Mat image,
            Mat edges,
            double threshold1,
            double threshold2,
            int apertureSize = 3,
            bool l2Gradient = false)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(edges, nameof(edges));

            NativeException.ThrowIfError(NativeMethods.ImgProcCanny(
                image.NativeHandle,
                edges.NativeHandle,
                threshold1,
                threshold2,
                apertureSize,
                l2Gradient ? 1 : 0));
        }

        /// <summary>
        /// Finds edges from custom image derivatives using the Canny algorithm.
        /// 使用自定义图像导数通过 Canny 算法查找边缘。
        /// </summary>
        /// <param name="dx">The x derivative image. x 方向导数图像。</param>
        /// <param name="dy">The y derivative image. y 方向导数图像。</param>
        /// <param name="edges">The output edge map. 输出边缘图。</param>
        /// <param name="threshold1">The first hysteresis threshold. 第一个滞后阈值。</param>
        /// <param name="threshold2">The second hysteresis threshold. 第二个滞后阈值。</param>
        /// <param name="l2Gradient">Whether to use the L2 gradient norm. 是否使用 L2 梯度范数。</param>
        /// <exception cref="ArgumentNullException">Thrown when a required matrix is null. 当必需矩阵为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Canny(
            Mat dx,
            Mat dy,
            Mat edges,
            double threshold1,
            double threshold2,
            bool l2Gradient = false)
        {
            ValidateNotNull(dx, nameof(dx));
            ValidateNotNull(dy, nameof(dy));
            ValidateNotNull(edges, nameof(edges));

            NativeException.ThrowIfError(NativeMethods.ImgProcCannyDerivatives(
                dx.NativeHandle,
                dy.NativeHandle,
                edges.NativeHandle,
                threshold1,
                threshold2,
                l2Gradient ? 1 : 0));
        }

        /// <summary>
        /// Returns Gaussian filter coefficients.
        /// 返回高斯滤波系数。
        /// </summary>
        /// <param name="ksize">The odd aperture size. 奇数孔径尺寸。</param>
        /// <param name="sigma">The Gaussian standard deviation. 高斯标准差。</param>
        /// <param name="ktype">The coefficient matrix type. 系数矩阵类型。</param>
        /// <returns>The Gaussian kernel matrix. 高斯核矩阵。</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ksize"/> is not positive. 当 <paramref name="ksize"/> 非正时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Mat GetGaussianKernel(int ksize, double sigma, int ktype = MatType.CV_64F)
        {
            if (ksize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ksize), "Kernel size must be positive.");
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcGetGaussianKernel(ksize, sigma, ktype, out IntPtr kernel));
            return new Mat(kernel);
        }

        /// <summary>
        /// Returns derivative filter coefficients.
        /// 返回导数滤波系数。
        /// </summary>
        /// <param name="kx">The output row filter coefficients. 输出行方向滤波系数。</param>
        /// <param name="ky">The output column filter coefficients. 输出列方向滤波系数。</param>
        /// <param name="dx">The derivative order in x. x 方向导数阶数。</param>
        /// <param name="dy">The derivative order in y. y 方向导数阶数。</param>
        /// <param name="ksize">The aperture size or Scharr marker. 孔径尺寸或 Scharr 标记。</param>
        /// <param name="normalize">Whether to normalize the coefficients. 是否归一化系数。</param>
        /// <param name="ktype">The coefficient matrix type. 系数矩阵类型。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="kx"/> or <paramref name="ky"/> is null. 当 <paramref name="kx"/> 或 <paramref name="ky"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void GetDerivKernels(
            Mat kx,
            Mat ky,
            int dx,
            int dy,
            int ksize,
            bool normalize = false,
            int ktype = MatType.CV_32F)
        {
            ValidateNotNull(kx, nameof(kx));
            ValidateNotNull(ky, nameof(ky));

            NativeException.ThrowIfError(NativeMethods.ImgProcGetDerivKernels(
                kx.NativeHandle,
                ky.NativeHandle,
                dx,
                dy,
                ksize,
                normalize ? 1 : 0,
                ktype));
        }

        /// <summary>
        /// Returns Gabor filter coefficients.
        /// 返回 Gabor 滤波系数。
        /// </summary>
        /// <param name="ksize">The filter size. 滤波器尺寸。</param>
        /// <param name="sigma">The Gaussian envelope standard deviation. 高斯包络标准差。</param>
        /// <param name="theta">The orientation angle in radians. 方向角，单位为弧度。</param>
        /// <param name="lambd">The wavelength of the sinusoidal factor. 正弦因子的波长。</param>
        /// <param name="gamma">The spatial aspect ratio. 空间纵横比。</param>
        /// <param name="psi">The phase offset. 相位偏移。</param>
        /// <param name="ktype">The coefficient matrix type. 系数矩阵类型。</param>
        /// <returns>The Gabor kernel matrix. Gabor 核矩阵。</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ksize"/> contains a non-positive dimension. 当 <paramref name="ksize"/> 包含非正维度时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Mat GetGaborKernel(
            Size ksize,
            double sigma,
            double theta,
            double lambd,
            double gamma,
            double psi = 1.5707963267948966,
            int ktype = MatType.CV_64F)
        {
            ValidatePositiveSize(ksize, nameof(ksize));

            NativeException.ThrowIfError(NativeMethods.ImgProcGetGaborKernel(
                ksize.Width,
                ksize.Height,
                sigma,
                theta,
                lambd,
                gamma,
                psi,
                ktype,
                out IntPtr kernel));
            return new Mat(kernel);
        }

        /// <summary>
        /// Downsamples an image using a Gaussian pyramid step.
        /// 使用高斯金字塔步骤下采样图像。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="dstsize">The output size, or empty to derive it automatically. 输出尺寸，空尺寸表示自动推导。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dstsize"/> contains a negative dimension. 当 <paramref name="dstsize"/> 包含负数维度时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void PyrDown(Mat src, Mat dst, Size? dstsize = null, BorderTypes borderType = BorderTypes.Default)
        {
            Size actualSize = dstsize ?? new Size();
            ValidateMatPair(src, dst);
            ValidateNonNegativeSize(actualSize, nameof(dstsize));

            NativeException.ThrowIfError(NativeMethods.ImgProcPyrDown(
                src.NativeHandle,
                dst.NativeHandle,
                actualSize.Width,
                actualSize.Height,
                (int)borderType));
        }

        /// <summary>
        /// Upsamples an image using a Gaussian pyramid step.
        /// 使用高斯金字塔步骤上采样图像。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="dstsize">The output size, or empty to derive it automatically. 输出尺寸，空尺寸表示自动推导。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dstsize"/> contains a negative dimension. 当 <paramref name="dstsize"/> 包含负数维度时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void PyrUp(Mat src, Mat dst, Size? dstsize = null, BorderTypes borderType = BorderTypes.Default)
        {
            Size actualSize = dstsize ?? new Size();
            ValidateMatPair(src, dst);
            ValidateNonNegativeSize(actualSize, nameof(dstsize));

            NativeException.ThrowIfError(NativeMethods.ImgProcPyrUp(
                src.NativeHandle,
                dst.NativeHandle,
                actualSize.Width,
                actualSize.Height,
                (int)borderType));
        }

        /// <summary>
        /// Applies an affine transformation to an image.
        /// 对图像应用仿射变换。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="m">The 2x3 transformation matrix. 2x3 变换矩阵。</param>
        /// <param name="dsize">The output image size. 输出图像尺寸。</param>
        /// <param name="flags">Interpolation and warp flags. 插值和变换标志。</param>
        /// <param name="borderMode">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <param name="borderValue">The constant border value. 常量边界值。</param>
        /// <exception cref="ArgumentNullException">Thrown when a required matrix is null. 当必需矩阵为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dsize"/> contains a negative dimension. 当 <paramref name="dsize"/> 包含负数维度时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void WarpAffine(
            Mat src,
            Mat dst,
            Mat m,
            Size dsize,
            InterpolationFlags flags = InterpolationFlags.Linear,
            BorderTypes borderMode = BorderTypes.Constant,
            Scalar? borderValue = null)
        {
            Scalar actualBorderValue = borderValue ?? new Scalar();
            ValidateMatPair(src, dst);
            ValidateNotNull(m, nameof(m));
            ValidateNonNegativeSize(dsize, nameof(dsize));

            NativeException.ThrowIfError(NativeMethods.ImgProcWarpAffine(
                src.NativeHandle,
                dst.NativeHandle,
                m.NativeHandle,
                dsize.Width,
                dsize.Height,
                (int)flags,
                (int)borderMode,
                actualBorderValue.V0,
                actualBorderValue.V1,
                actualBorderValue.V2,
                actualBorderValue.V3));
        }

        /// <summary>
        /// Applies a perspective transformation to an image.
        /// 对图像应用透视变换。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="m">The 3x3 transformation matrix. 3x3 变换矩阵。</param>
        /// <param name="dsize">The output image size. 输出图像尺寸。</param>
        /// <param name="flags">Interpolation and warp flags. 插值和变换标志。</param>
        /// <param name="borderMode">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <param name="borderValue">The constant border value. 常量边界值。</param>
        /// <exception cref="ArgumentNullException">Thrown when a required matrix is null. 当必需矩阵为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dsize"/> contains a negative dimension. 当 <paramref name="dsize"/> 包含负数维度时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void WarpPerspective(
            Mat src,
            Mat dst,
            Mat m,
            Size dsize,
            InterpolationFlags flags = InterpolationFlags.Linear,
            BorderTypes borderMode = BorderTypes.Constant,
            Scalar? borderValue = null)
        {
            Scalar actualBorderValue = borderValue ?? new Scalar();
            ValidateMatPair(src, dst);
            ValidateNotNull(m, nameof(m));
            ValidateNonNegativeSize(dsize, nameof(dsize));

            NativeException.ThrowIfError(NativeMethods.ImgProcWarpPerspective(
                src.NativeHandle,
                dst.NativeHandle,
                m.NativeHandle,
                dsize.Width,
                dsize.Height,
                (int)flags,
                (int)borderMode,
                actualBorderValue.V0,
                actualBorderValue.V1,
                actualBorderValue.V2,
                actualBorderValue.V3));
        }

        /// <summary>
        /// Computes a 2D rotation matrix.
        /// 计算二维旋转矩阵。
        /// </summary>
        /// <param name="center">The rotation center. 旋转中心。</param>
        /// <param name="angle">The rotation angle in degrees. 旋转角度，单位为度。</param>
        /// <param name="scale">The isotropic scale factor. 各向同性缩放因子。</param>
        /// <returns>The 2x3 rotation matrix. 2x3 旋转矩阵。</returns>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Mat GetRotationMatrix2D(Point2f center, double angle, double scale)
        {
            NativeException.ThrowIfError(NativeMethods.ImgProcGetRotationMatrix2D(
                center.X,
                center.Y,
                angle,
                scale,
                out IntPtr transform));
            return new Mat(transform);
        }

        /// <summary>
        /// Computes an affine transform from three source and destination points.
        /// 根据三组源点和目标点计算仿射变换。
        /// </summary>
        /// <param name="src">The three source points. 三个源点。</param>
        /// <param name="dst">The three destination points. 三个目标点。</param>
        /// <returns>The 2x3 affine transform matrix. 2x3 仿射变换矩阵。</returns>
        /// <exception cref="ArgumentNullException">Thrown when an array is null. 当数组为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when an array has fewer than three points. 当数组少于三个点时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Mat GetAffineTransform(Point2f[] src, Point2f[] dst)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (dst == null)
            {
                throw new ArgumentNullException(nameof(dst));
            }

            return GetAffineTransform(src.AsSpan(), dst.AsSpan());
#else
            float[] srcXy = ToInterleavedPoint2f(src, nameof(src), 3);
            float[] dstXy = ToInterleavedPoint2f(dst, nameof(dst), 3);
            return GetAffineTransformFromInterleaved(srcXy, dstXy);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Computes an affine transform from point spans.
        /// 根据点 Span 计算仿射变换。
        /// </summary>
        /// <param name="src">The three source points. 三个源点。</param>
        /// <param name="dst">The three destination points. 三个目标点。</param>
        /// <returns>The 2x3 affine transform matrix. 2x3 仿射变换矩阵。</returns>
        /// <exception cref="ArgumentException">Thrown when a span has fewer than three points. 当 Span 少于三个点时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe Mat GetAffineTransform(ReadOnlySpan<Point2f> src, ReadOnlySpan<Point2f> dst)
        {
            PointSetMarshaller.ValidateCountAtLeast(src, 3, nameof(src));
            PointSetMarshaller.ValidateCountAtLeast(dst, 3, nameof(dst));
            ReadOnlySpan<float> srcXy = PointSetMarshaller.AsInterleaved(src.Slice(0, 3));
            ReadOnlySpan<float> dstXy = PointSetMarshaller.AsInterleaved(dst.Slice(0, 3));

            fixed (float* srcPtr = srcXy)
            fixed (float* dstPtr = dstXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcGetAffineTransform(srcPtr, dstPtr, out IntPtr transform));
                return new Mat(transform);
            }
        }
#endif

        /// <summary>
        /// Computes a perspective transform from four source and destination points.
        /// 根据四组源点和目标点计算透视变换。
        /// </summary>
        /// <param name="src">The four source points. 四个源点。</param>
        /// <param name="dst">The four destination points. 四个目标点。</param>
        /// <param name="solveMethod">The matrix decomposition method. 矩阵分解方式。</param>
        /// <returns>The 3x3 perspective transform matrix. 3x3 透视变换矩阵。</returns>
        /// <exception cref="ArgumentNullException">Thrown when an array is null. 当数组为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when an array has fewer than four points. 当数组少于四个点时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Mat GetPerspectiveTransform(Point2f[] src, Point2f[] dst, DecompTypes solveMethod = DecompTypes.LU)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (dst == null)
            {
                throw new ArgumentNullException(nameof(dst));
            }

            return GetPerspectiveTransform(src.AsSpan(), dst.AsSpan(), solveMethod);
#else
            float[] srcXy = ToInterleavedPoint2f(src, nameof(src), 4);
            float[] dstXy = ToInterleavedPoint2f(dst, nameof(dst), 4);
            return GetPerspectiveTransformFromInterleaved(srcXy, dstXy, solveMethod);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Computes a perspective transform from point spans.
        /// 根据点 Span 计算透视变换。
        /// </summary>
        /// <param name="src">The four source points. 四个源点。</param>
        /// <param name="dst">The four destination points. 四个目标点。</param>
        /// <param name="solveMethod">The matrix decomposition method. 矩阵分解方式。</param>
        /// <returns>The 3x3 perspective transform matrix. 3x3 透视变换矩阵。</returns>
        /// <exception cref="ArgumentException">Thrown when a span has fewer than four points. 当 Span 少于四个点时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe Mat GetPerspectiveTransform(
            ReadOnlySpan<Point2f> src,
            ReadOnlySpan<Point2f> dst,
            DecompTypes solveMethod = DecompTypes.LU)
        {
            PointSetMarshaller.ValidateCountAtLeast(src, 4, nameof(src));
            PointSetMarshaller.ValidateCountAtLeast(dst, 4, nameof(dst));
            ReadOnlySpan<float> srcXy = PointSetMarshaller.AsInterleaved(src.Slice(0, 4));
            ReadOnlySpan<float> dstXy = PointSetMarshaller.AsInterleaved(dst.Slice(0, 4));

            fixed (float* srcPtr = srcXy)
            fixed (float* dstPtr = dstXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcGetPerspectiveTransform(
                    srcPtr,
                    dstPtr,
                    (int)solveMethod,
                    out IntPtr transform));
                return new Mat(transform);
            }
        }
#endif

        /// <summary>
        /// Inverts an affine transformation matrix.
        /// 反转仿射变换矩阵。
        /// </summary>
        /// <param name="m">The source 2x3 affine transform. 源 2x3 仿射变换。</param>
        /// <param name="inverseM">The output inverse affine transform. 输出逆仿射变换。</param>
        /// <exception cref="ArgumentNullException">Thrown when a required matrix is null. 当必需矩阵为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void InvertAffineTransform(Mat m, Mat inverseM)
        {
            ValidateNotNull(m, nameof(m));
            ValidateNotNull(inverseM, nameof(inverseM));

            NativeException.ThrowIfError(NativeMethods.ImgProcInvertAffineTransform(m.NativeHandle, inverseM.NativeHandle));
        }

        /// <summary>
        /// Remaps an image using coordinate maps.
        /// 使用坐标映射重映射图像。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="map1">The first map. 第一个映射。</param>
        /// <param name="map2">The second map, or an empty matrix when not used. 第二个映射；不使用时传入空矩阵。</param>
        /// <param name="interpolation">The interpolation method. 插值方式。</param>
        /// <param name="borderMode">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <param name="borderValue">The constant border value. 常量边界值。</param>
        /// <exception cref="ArgumentNullException">Thrown when a required matrix is null. 当必需矩阵为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Remap(
            Mat src,
            Mat dst,
            Mat map1,
            Mat map2,
            InterpolationFlags interpolation,
            BorderTypes borderMode = BorderTypes.Constant,
            Scalar? borderValue = null)
        {
            Scalar actualBorderValue = borderValue ?? new Scalar();
            ValidateMatPair(src, dst);
            ValidateNotNull(map1, nameof(map1));
            ValidateNotNull(map2, nameof(map2));

            NativeException.ThrowIfError(NativeMethods.ImgProcRemap(
                src.NativeHandle,
                dst.NativeHandle,
                map1.NativeHandle,
                map2.NativeHandle,
                (int)interpolation,
                (int)borderMode,
                actualBorderValue.V0,
                actualBorderValue.V1,
                actualBorderValue.V2,
                actualBorderValue.V3));
        }

        /// <summary>
        /// Converts remap coordinate maps from one representation to another.
        /// 将 remap 坐标映射从一种表示转换为另一种表示。
        /// </summary>
        /// <param name="map1">The first input map. 第一个输入映射。</param>
        /// <param name="map2">The second input map, or an empty matrix when not used. 第二个输入映射；不使用时传入空矩阵。</param>
        /// <param name="dstmap1">The first output map. 第一个输出映射。</param>
        /// <param name="dstmap2">The second output map. 第二个输出映射。</param>
        /// <param name="dstmap1type">The first output map type. 第一个输出映射类型。</param>
        /// <param name="nninterpolation">Whether fixed-point maps use nearest-neighbor interpolation. 固定点映射是否使用最近邻插值。</param>
        /// <exception cref="ArgumentNullException">Thrown when a required matrix is null. 当必需矩阵为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void ConvertMaps(
            Mat map1,
            Mat map2,
            Mat dstmap1,
            Mat dstmap2,
            int dstmap1type,
            bool nninterpolation = false)
        {
            ValidateNotNull(map1, nameof(map1));
            ValidateNotNull(map2, nameof(map2));
            ValidateNotNull(dstmap1, nameof(dstmap1));
            ValidateNotNull(dstmap2, nameof(dstmap2));

            NativeException.ThrowIfError(NativeMethods.ImgProcConvertMaps(
                map1.NativeHandle,
                map2.NativeHandle,
                dstmap1.NativeHandle,
                dstmap2.NativeHandle,
                dstmap1type,
                nninterpolation ? 1 : 0));
        }

        /// <summary>
        /// Returns a structuring element of the specified size and shape for morphological operations.
        /// 返回用于形态学操作的指定尺寸和形状的结构元素。
        /// </summary>
        /// <param name="shape">The structuring element shape. 结构元素形状。</param>
        /// <param name="ksize">The structuring element size. 结构元素尺寸。</param>
        /// <param name="anchor">The anchor position in the element; <c>(-1,-1)</c> means the center. 元素中的锚点位置；<c>(-1,-1)</c> 表示中心。</param>
        /// <returns>A matrix containing the structuring element. 包含结构元素的矩阵。</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="shape"/> is unsupported or <paramref name="ksize"/> contains a non-positive dimension. 当 <paramref name="shape"/> 不受支持或 <paramref name="ksize"/> 包含非正维度时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Mat GetStructuringElement(MorphShapes shape, Size ksize, Point? anchor = null)
        {
            if (ksize.Width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ksize), "Width must be positive.");
            }

            if (ksize.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ksize), "Height must be positive.");
            }

            ValidateMorphShape(shape, nameof(shape));

            Point actualAnchor = anchor ?? new Point(-1, -1);
            NativeException.ThrowIfError(NativeMethods.ImgProcGetStructuringElement(
                (int)shape,
                ksize.Width,
                ksize.Height,
                actualAnchor.X,
                actualAnchor.Y,
                out IntPtr element));

            return new Mat(element);
        }

        /// <summary>
        /// Erodes an image using the specified structuring element.
        /// 使用指定的结构元素腐蚀图像。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="kernel">The structuring element. 结构元素。</param>
        /// <param name="anchor">The anchor position in the element; <c>(-1,-1)</c> means the center. 元素中的锚点位置；<c>(-1,-1)</c> 表示中心。</param>
        /// <param name="iterations">The number of times erosion is applied. 腐蚀操作应用次数。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <param name="borderValue">The border value used with constant borders, or null to use OpenCV's morphology default. 常量边界使用的边界值；为 null 时使用 OpenCV 形态学默认值。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/>, <paramref name="dst"/>, or <paramref name="kernel"/> is null. 当 <paramref name="src"/>、<paramref name="dst"/> 或 <paramref name="kernel"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="iterations"/> is negative. 当 <paramref name="iterations"/> 为负数时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Erode(
            Mat src,
            Mat dst,
            Mat kernel,
            Point? anchor = null,
            int iterations = 1,
            BorderTypes borderType = BorderTypes.Constant,
            Scalar? borderValue = null)
        {
            RunMorphology(NativeMethods.ImgProcErode, src, dst, kernel, anchor, iterations, borderType, borderValue);
        }

        /// <summary>
        /// Dilates an image using the specified structuring element.
        /// 使用指定的结构元素膨胀图像。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="kernel">The structuring element. 结构元素。</param>
        /// <param name="anchor">The anchor position in the element; <c>(-1,-1)</c> means the center. 元素中的锚点位置；<c>(-1,-1)</c> 表示中心。</param>
        /// <param name="iterations">The number of times dilation is applied. 膨胀操作应用次数。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <param name="borderValue">The border value used with constant borders, or null to use OpenCV's morphology default. 常量边界使用的边界值；为 null 时使用 OpenCV 形态学默认值。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/>, <paramref name="dst"/>, or <paramref name="kernel"/> is null. 当 <paramref name="src"/>、<paramref name="dst"/> 或 <paramref name="kernel"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="iterations"/> is negative. 当 <paramref name="iterations"/> 为负数时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Dilate(
            Mat src,
            Mat dst,
            Mat kernel,
            Point? anchor = null,
            int iterations = 1,
            BorderTypes borderType = BorderTypes.Constant,
            Scalar? borderValue = null)
        {
            RunMorphology(NativeMethods.ImgProcDilate, src, dst, kernel, anchor, iterations, borderType, borderValue);
        }

        /// <summary>
        /// Performs an advanced morphological transformation.
        /// 执行高级形态学变换。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="op">The morphological operation type. 形态学操作类型。</param>
        /// <param name="kernel">The structuring element. 结构元素。</param>
        /// <param name="anchor">The anchor position in the element; <c>(-1,-1)</c> means the center. 元素中的锚点位置；<c>(-1,-1)</c> 表示中心。</param>
        /// <param name="iterations">The number of times erosion and dilation are applied. 腐蚀和膨胀操作应用次数。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <param name="borderValue">The border value used with constant borders, or null to use OpenCV's morphology default. 常量边界使用的边界值；为 null 时使用 OpenCV 形态学默认值。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/>, <paramref name="dst"/>, or <paramref name="kernel"/> is null. 当 <paramref name="src"/>、<paramref name="dst"/> 或 <paramref name="kernel"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="op"/> is unsupported or <paramref name="iterations"/> is negative. 当 <paramref name="op"/> 不受支持或 <paramref name="iterations"/> 为负数时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void MorphologyEx(
            Mat src,
            Mat dst,
            MorphTypes op,
            Mat kernel,
            Point? anchor = null,
            int iterations = 1,
            BorderTypes borderType = BorderTypes.Constant,
            Scalar? borderValue = null)
        {
            RunMorphologyEx(NativeMethods.ImgProcMorphologyEx, src, dst, op, kernel, anchor, iterations, borderType, borderValue);
        }

        /// <summary>
        /// Draws a line segment connecting two points.
        /// 绘制连接两个点的线段。
        /// </summary>
        /// <param name="img">The image to draw on. 要绘制的图像。</param>
        /// <param name="pt1">The first point of the line segment. 线段的第一个点。</param>
        /// <param name="pt2">The second point of the line segment. 线段的第二个点。</param>
        /// <param name="color">The line color. 线条颜色。</param>
        /// <param name="thickness">The line thickness. 线条粗细。</param>
        /// <param name="lineType">The line drawing algorithm. 线条绘制算法。</param>
        /// <param name="shift">The number of fractional bits in point coordinates. 点坐标中的小数位位数。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="img"/> is null. 当 <paramref name="img"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Line(
            Mat img,
            Point pt1,
            Point pt2,
            Scalar color,
            int thickness = 1,
            LineTypes lineType = LineTypes.Line8,
            int shift = 0)
        {
            if (img == null)
            {
                throw new ArgumentNullException(nameof(img));
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcLine(
                img.NativeHandle,
                pt1.X,
                pt1.Y,
                pt2.X,
                pt2.Y,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                thickness,
                (int)lineType,
                shift));
        }

        /// <summary>
        /// Draws a line segment with an arrow tip.
        /// 绘制带箭头的线段。
        /// </summary>
        /// <param name="img">The image to draw on. 要绘制的图像。</param>
        /// <param name="pt1">The first point of the arrowed line. 箭头线的起点。</param>
        /// <param name="pt2">The second point of the arrowed line. 箭头线的终点。</param>
        /// <param name="color">The line color. 线条颜色。</param>
        /// <param name="thickness">The line thickness. 线条粗细。</param>
        /// <param name="lineType">The line drawing algorithm. 线条绘制算法。</param>
        /// <param name="shift">The number of fractional bits in point coordinates. 点坐标中的小数位位数。</param>
        /// <param name="tipLength">The length of the arrow tip relative to the arrow length. 箭头尖端长度与箭头总长度的相对比例。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="img"/> is null. 当 <paramref name="img"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void ArrowedLine(
            Mat img,
            Point pt1,
            Point pt2,
            Scalar color,
            int thickness = 1,
            LineTypes lineType = LineTypes.Line8,
            int shift = 0,
            double tipLength = 0.1)
        {
            if (img == null)
            {
                throw new ArgumentNullException(nameof(img));
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcArrowedLine(
                img.NativeHandle,
                pt1.X,
                pt1.Y,
                pt2.X,
                pt2.Y,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                thickness,
                (int)lineType,
                shift,
                tipLength));
        }

        /// <summary>
        /// Clips a line segment by a rectangle.
        /// 按矩形裁剪线段。
        /// </summary>
        /// <param name="imgRect">The clipping rectangle. 裁剪矩形。</param>
        /// <param name="pt1">The first line endpoint; updated with the clipped endpoint. 第一个线段端点；会更新为裁剪后的端点。</param>
        /// <param name="pt2">The second line endpoint; updated with the clipped endpoint. 第二个线段端点；会更新为裁剪后的端点。</param>
        /// <returns><c>true</c> if the line segment intersects the rectangle; otherwise, <c>false</c>. 如果线段与矩形相交则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static bool ClipLine(Rect imgRect, ref Point pt1, ref Point pt2)
        {
            int pt1X = pt1.X;
            int pt1Y = pt1.Y;
            int pt2X = pt2.X;
            int pt2Y = pt2.Y;
            int intersects;

            NativeException.ThrowIfError(NativeMethods.ImgProcClipLineRect(
                imgRect.X,
                imgRect.Y,
                imgRect.Width,
                imgRect.Height,
                ref pt1X,
                ref pt1Y,
                ref pt2X,
                ref pt2Y,
                out intersects));

            pt1 = new Point(pt1X, pt1Y);
            pt2 = new Point(pt2X, pt2Y);
            return intersects != 0;
        }

        /// <summary>
        /// Draws a single polygonal curve.
        /// 绘制单条多段折线。
        /// </summary>
        /// <param name="img">The image to draw on. 要绘制的图像。</param>
        /// <param name="pts">The polyline vertices. 折线顶点。</param>
        /// <param name="isClosed">Whether the polyline should be closed. 是否闭合折线。</param>
        /// <param name="color">The polyline color. 折线颜色。</param>
        /// <param name="thickness">The thickness of the polyline edges. 折线边缘粗细。</param>
        /// <param name="lineType">The line drawing algorithm. 线条绘制算法。</param>
        /// <param name="shift">The number of fractional bits in point coordinates. 点坐标中的小数位位数。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="img"/> or <paramref name="pts"/> is null. 当 <paramref name="img"/> 或 <paramref name="pts"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="pts"/> is empty. 当 <paramref name="pts"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Polylines(
            Mat img,
            Point[] pts,
            bool isClosed,
            Scalar color,
            int thickness = 1,
            LineTypes lineType = LineTypes.Line8,
            int shift = 0)
        {
            if (img == null)
            {
                throw new ArgumentNullException(nameof(img));
            }

            int[] pointsXy = ToInterleavedPoints(pts, nameof(pts));
            NativeException.ThrowIfError(NativeMethods.ImgProcPolylines(
                img.NativeHandle,
                pointsXy,
                pts.Length,
                isClosed ? 1 : 0,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                thickness,
                (int)lineType,
                shift));
        }

        /// <summary>
        /// Fills an area bounded by a single polygonal contour.
        /// 填充由单个多边形轮廓包围的区域。
        /// </summary>
        /// <param name="img">The image to draw on. 要绘制的图像。</param>
        /// <param name="pts">The polygon vertices. 多边形顶点。</param>
        /// <param name="color">The polygon fill color. 多边形填充颜色。</param>
        /// <param name="lineType">The polygon boundary line type. 多边形边界线类型。</param>
        /// <param name="shift">The number of fractional bits in point coordinates. 点坐标中的小数位位数。</param>
        /// <param name="offset">The optional offset applied to all polygon points. 应用于所有多边形点的可选偏移。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="img"/> or <paramref name="pts"/> is null. 当 <paramref name="img"/> 或 <paramref name="pts"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="pts"/> is empty. 当 <paramref name="pts"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void FillPoly(
            Mat img,
            Point[] pts,
            Scalar color,
            LineTypes lineType = LineTypes.Line8,
            int shift = 0,
            Point? offset = null)
        {
            if (img == null)
            {
                throw new ArgumentNullException(nameof(img));
            }

            Point actualOffset = offset ?? new Point();
            int[] pointsXy = ToInterleavedPoints(pts, nameof(pts));
            NativeException.ThrowIfError(NativeMethods.ImgProcFillPoly(
                img.NativeHandle,
                pointsXy,
                pts.Length,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                (int)lineType,
                shift,
                actualOffset.X,
                actualOffset.Y));
        }

        /// <summary>
        /// Computes vertices of a polyline that approximates an elliptic arc.
        /// 计算用于近似椭圆弧的折线顶点。
        /// </summary>
        /// <param name="center">The center of the arc. 椭圆弧中心。</param>
        /// <param name="axes">Half of the size of the ellipse main axes. 椭圆主轴尺寸的一半。</param>
        /// <param name="angle">The rotation angle of the ellipse in degrees. 椭圆旋转角度，单位为度。</param>
        /// <param name="arcStart">The starting angle of the elliptic arc in degrees. 椭圆弧起始角度，单位为度。</param>
        /// <param name="arcEnd">The ending angle of the elliptic arc in degrees. 椭圆弧结束角度，单位为度。</param>
        /// <param name="delta">The angle between subsequent polyline vertices. 相邻折线顶点之间的角度。</param>
        /// <returns>The generated polyline vertices. 生成的折线顶点。</returns>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Point[] Ellipse2Poly(Point center, Size axes, int angle, int arcStart, int arcEnd, int delta)
        {
            int pointCount;
            NativeException.ThrowIfError(NativeMethods.ImgProcEllipse2PolyCount(
                center.X,
                center.Y,
                axes.Width,
                axes.Height,
                angle,
                arcStart,
                arcEnd,
                delta,
                out pointCount));

            var pointsXy = new int[pointCount * 2];
            int writtenCount;
            NativeException.ThrowIfError(NativeMethods.ImgProcEllipse2PolyFill(
                center.X,
                center.Y,
                axes.Width,
                axes.Height,
                angle,
                arcStart,
                arcEnd,
                delta,
                pointsXy,
                pointCount,
                out writtenCount));

            if (writtenCount != pointCount)
            {
                Array.Resize(ref pointsXy, writtenCount * 2);
            }

            return FromInterleavedPoints(pointsXy, writtenCount);
        }

        /// <summary>
        /// Calculates the area of a contour.
        /// 计算轮廓的面积。
        /// </summary>
        /// <param name="contour">The contour vertices. 轮廓顶点。</param>
        /// <param name="oriented">Whether to return a signed area based on contour orientation. 是否根据轮廓方向返回带符号的面积。</param>
        /// <returns>The contour area. 轮廓面积。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contour"/> is null. 当 <paramref name="contour"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="contour"/> is empty. 当 <paramref name="contour"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static double ContourArea(Point[] contour, bool oriented = false)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (contour == null)
            {
                throw new ArgumentNullException(nameof(contour));
            }

            return ContourArea(contour.AsSpan(), oriented);
#else
            int[] pointsXy = ToInterleavedPoints(contour, nameof(contour));
            NativeException.ThrowIfError(NativeMethods.ImgProcContourArea(pointsXy, contour.Length, oriented ? 1 : 0, out double area));
            return area;
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Calculates the area of a contour from a point span.
        /// 从点 Span 计算轮廓的面积。
        /// </summary>
        /// <param name="contour">The contour vertices. 轮廓顶点。</param>
        /// <param name="oriented">Whether to return a signed area based on contour orientation. 是否根据轮廓方向返回带符号的面积。</param>
        /// <returns>The contour area. 轮廓面积。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="contour"/> is empty. 当 <paramref name="contour"/> 为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe double ContourArea(ReadOnlySpan<Point> contour, bool oriented = false)
        {
            return RunPointSetScalar(contour, nameof(contour), oriented ? 1 : 0, NativeMethods.ImgProcContourAreaPtr);
        }
#endif

        /// <summary>
        /// Calculates a curve length or a closed contour perimeter.
        /// 计算曲线长度或闭合轮廓周长。
        /// </summary>
        /// <param name="curve">The curve vertices. 曲线顶点。</param>
        /// <param name="closed">Whether the curve is closed. 曲线是否闭合。</param>
        /// <returns>The curve length or contour perimeter. 曲线长度或轮廓周长。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="curve"/> is null. 当 <paramref name="curve"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="curve"/> is empty. 当 <paramref name="curve"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static double ArcLength(Point[] curve, bool closed)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (curve == null)
            {
                throw new ArgumentNullException(nameof(curve));
            }

            return ArcLength(curve.AsSpan(), closed);
#else
            int[] pointsXy = ToInterleavedPoints(curve, nameof(curve));
            NativeException.ThrowIfError(NativeMethods.ImgProcArcLength(pointsXy, curve.Length, closed ? 1 : 0, out double length));
            return length;
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Calculates a curve length or a closed contour perimeter from a point span.
        /// 从点 Span 计算曲线长度或闭合轮廓周长。
        /// </summary>
        /// <param name="curve">The curve vertices. 曲线顶点。</param>
        /// <param name="closed">Whether the curve is closed. 曲线是否闭合。</param>
        /// <returns>The curve length or contour perimeter. 曲线长度或轮廓周长。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="curve"/> is empty. 当 <paramref name="curve"/> 为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe double ArcLength(ReadOnlySpan<Point> curve, bool closed)
        {
            return RunPointSetScalar(curve, nameof(curve), closed ? 1 : 0, NativeMethods.ImgProcArcLengthPtr);
        }
#endif

        /// <summary>
        /// Approximates a curve or polygon with another curve that has fewer vertices.
        /// 使用顶点更少的曲线近似输入曲线或多边形。
        /// </summary>
        /// <param name="curve">The input curve vertices. 输入曲线顶点。</param>
        /// <param name="epsilon">The maximum distance between the original curve and its approximation. 原始曲线与近似曲线之间的最大距离。</param>
        /// <param name="closed">Whether the approximated curve should be closed. 近似曲线是否闭合。</param>
        /// <returns>The approximated curve vertices. 近似曲线顶点。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="curve"/> is null. 当 <paramref name="curve"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="curve"/> is empty. 当 <paramref name="curve"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Point[] ApproxPolyDP(Point[] curve, double epsilon, bool closed)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (curve == null)
            {
                throw new ArgumentNullException(nameof(curve));
            }

            return ApproxPolyDP(curve.AsSpan(), epsilon, closed);
#else
            int[] curveXy = ToInterleavedPoints(curve, nameof(curve));
            NativeException.ThrowIfError(NativeMethods.ImgProcApproxPolyDPCount(
                curveXy,
                curve.Length,
                epsilon,
                closed ? 1 : 0,
                out int approxPointCount));

            var approxPointsXy = new int[approxPointCount * 2];
            NativeException.ThrowIfError(NativeMethods.ImgProcApproxPolyDPFill(
                curveXy,
                curve.Length,
                epsilon,
                closed ? 1 : 0,
                approxPointsXy,
                approxPointCount,
                out int writtenCount));

            if (writtenCount != approxPointCount)
            {
                Array.Resize(ref approxPointsXy, writtenCount * 2);
            }

            return FromInterleavedPoints(approxPointsXy, writtenCount);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Approximates a curve or polygon from a point span with another curve that has fewer vertices.
        /// 使用顶点更少的曲线近似点 Span 输入曲线或多边形。
        /// </summary>
        /// <param name="curve">The input curve vertices. 输入曲线顶点。</param>
        /// <param name="epsilon">The maximum distance between the original curve and its approximation. 原始曲线与近似曲线之间的最大距离。</param>
        /// <param name="closed">Whether the approximated curve should be closed. 近似曲线是否闭合。</param>
        /// <returns>The approximated curve vertices. 近似曲线顶点。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="curve"/> is empty. 当 <paramref name="curve"/> 为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe Point[] ApproxPolyDP(ReadOnlySpan<Point> curve, double epsilon, bool closed)
        {
            PointSetMarshaller.ValidateNotEmpty(curve, nameof(curve));
            ReadOnlySpan<int> curveXy = PointSetMarshaller.AsInterleaved(curve);

            fixed (int* curvePtr = curveXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcApproxPolyDPCountPtr(
                    curvePtr,
                    curve.Length,
                    epsilon,
                    closed ? 1 : 0,
                    out int approxPointCount));

                int interleavedLength = approxPointCount * 2;
                int[]? rented = null;
                Span<int> approxPointsXy = interleavedLength <= StackallocInterleavedPointPairThreshold * 2 ?
                    stackalloc int[interleavedLength] :
                    (rented = ArrayPool<int>.Shared.Rent(interleavedLength)).AsSpan(0, interleavedLength);

                try
                {
                    fixed (int* approxPtr = approxPointsXy)
                    {
                        NativeException.ThrowIfError(NativeMethods.ImgProcApproxPolyDPFillPtr(
                            curvePtr,
                            curve.Length,
                            epsilon,
                            closed ? 1 : 0,
                            approxPtr,
                            approxPointCount,
                            out int writtenCount));

                        return FromInterleavedPoints(approxPointsXy, writtenCount);
                    }
                }
                finally
                {
                    if (rented != null)
                    {
                        ArrayPool<int>.Shared.Return(rented);
                    }
                }
            }
        }
#endif

        /// <summary>
        /// Approximates a polygon with a convex polygon with the requested number of sides.
        /// 使用指定边数的凸多边形近似输入多边形。
        /// </summary>
        /// <param name="curve">The input polygon vertices. 输入多边形顶点。</param>
        /// <param name="nsides">The target number of sides. 目标边数。</param>
        /// <param name="epsilonPercentage">The maximum additional area ratio, or -1 to disable this stop condition. 最大附加面积比例，传入 -1 表示禁用该停止条件。</param>
        /// <param name="ensureConvex">Whether OpenCV should compute the convex hull before approximation. 是否先由 OpenCV 计算凸包再近似。</param>
        /// <returns>The approximated polygon vertices. 近似多边形顶点。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="curve"/> is null. 当 <paramref name="curve"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="curve"/> is empty. 当 <paramref name="curve"/> 为空数组时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="nsides"/> is less than three. 当 <paramref name="nsides"/> 小于三时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Point2f[] ApproxPolyN(
            Point[] curve,
            int nsides,
            float epsilonPercentage = -1.0F,
            bool ensureConvex = true)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (curve == null)
            {
                throw new ArgumentNullException(nameof(curve));
            }

            return ApproxPolyN(curve.AsSpan(), nsides, epsilonPercentage, ensureConvex);
#else
            if (nsides < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(nsides), "Number of sides must be at least three.");
            }

            int[] curveXy = ToInterleavedPoints(curve, nameof(curve));
            int ensureConvexFlag = ensureConvex ? 1 : 0;
            NativeException.ThrowIfError(NativeMethods.ImgProcApproxPolyNCount(
                curveXy,
                curve.Length,
                nsides,
                epsilonPercentage,
                ensureConvexFlag,
                out int approxPointCount));

            var approxPointsXy = new float[approxPointCount * 2];
            NativeException.ThrowIfError(NativeMethods.ImgProcApproxPolyNFill(
                curveXy,
                curve.Length,
                nsides,
                epsilonPercentage,
                ensureConvexFlag,
                approxPointsXy,
                approxPointCount,
                out int writtenCount));

            if (writtenCount != approxPointCount)
            {
                Array.Resize(ref approxPointsXy, writtenCount * 2);
            }

            return FromInterleavedPoint2f(approxPointsXy, writtenCount);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Approximates a point span with a convex polygon with the requested number of sides.
        /// 使用指定边数的凸多边形近似点 Span。
        /// </summary>
        /// <param name="curve">The input polygon vertices. 输入多边形顶点。</param>
        /// <param name="nsides">The target number of sides. 目标边数。</param>
        /// <param name="epsilonPercentage">The maximum additional area ratio, or -1 to disable this stop condition. 最大附加面积比例，传入 -1 表示禁用该停止条件。</param>
        /// <param name="ensureConvex">Whether OpenCV should compute the convex hull before approximation. 是否先由 OpenCV 计算凸包再近似。</param>
        /// <returns>The approximated polygon vertices. 近似多边形顶点。</returns>
        public static unsafe Point2f[] ApproxPolyN(
            ReadOnlySpan<Point> curve,
            int nsides,
            float epsilonPercentage = -1.0F,
            bool ensureConvex = true)
        {
            PointSetMarshaller.ValidateNotEmpty(curve, nameof(curve));
            if (nsides < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(nsides), "Number of sides must be at least three.");
            }

            ReadOnlySpan<int> curveXy = PointSetMarshaller.AsInterleaved(curve);
            fixed (int* curvePtr = curveXy)
            {
                int ensureConvexFlag = ensureConvex ? 1 : 0;
                NativeException.ThrowIfError(NativeMethods.ImgProcApproxPolyNCountPtr(
                    curvePtr,
                    curve.Length,
                    nsides,
                    epsilonPercentage,
                    ensureConvexFlag,
                    out int approxPointCount));

                var result = new Point2f[approxPointCount];
                Span<float> resultXy = PointSetMarshaller.AsInterleaved(result.AsSpan());
                fixed (float* resultPtr = resultXy)
                {
                    NativeException.ThrowIfError(NativeMethods.ImgProcApproxPolyNFillPtr(
                        curvePtr,
                        curve.Length,
                        nsides,
                        epsilonPercentage,
                        ensureConvexFlag,
                        resultPtr,
                        result.Length,
                        out int writtenCount));
                    if (writtenCount != result.Length)
                    {
                        Array.Resize(ref result, writtenCount);
                    }
                }

                return result;
            }
        }
#endif

        /// <summary>
        /// Calculates the up-right bounding rectangle of a point set.
        /// 计算点集的正向外接矩形。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <returns>The bounding rectangle. 外接矩形。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null. 当 <paramref name="points"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Rect BoundingRect(Point[] points)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            return BoundingRect(points.AsSpan());
#else
            int[] pointsXy = ToInterleavedPoints(points, nameof(points));
            NativeException.ThrowIfError(NativeMethods.ImgProcBoundingRect(
                pointsXy,
                points.Length,
                out int x,
                out int y,
                out int width,
                out int height));
            return new Rect(x, y, width, height);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Calculates the up-right bounding rectangle of a point span.
        /// 计算点 Span 的正向外接矩形。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <returns>The bounding rectangle. 外接矩形。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe Rect BoundingRect(ReadOnlySpan<Point> points)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));
            return RunPointSetRect(points, nameof(points), NativeMethods.ImgProcBoundingRectPtr);
        }
#endif

        /// <summary>
        /// Tests whether a contour is convex.
        /// 判断轮廓是否为凸轮廓。
        /// </summary>
        /// <param name="contour">The input contour. 输入轮廓。</param>
        /// <returns><c>true</c> if the contour is convex; otherwise, <c>false</c>. 如果轮廓为凸轮廓则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contour"/> is null. 当 <paramref name="contour"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="contour"/> is empty. 当 <paramref name="contour"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static bool IsContourConvex(Point[] contour)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (contour == null)
            {
                throw new ArgumentNullException(nameof(contour));
            }

            return IsContourConvex(contour.AsSpan());
#else
            int[] pointsXy = ToInterleavedPoints(contour, nameof(contour));
            NativeException.ThrowIfError(NativeMethods.ImgProcIsContourConvex(pointsXy, contour.Length, out int isConvex));
            return isConvex != 0;
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Tests whether a contour span is convex.
        /// 判断轮廓 Span 是否为凸轮廓。
        /// </summary>
        /// <param name="contour">The input contour. 输入轮廓。</param>
        /// <returns><c>true</c> if the contour is convex; otherwise, <c>false</c>. 如果轮廓为凸轮廓则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="contour"/> is empty. 当 <paramref name="contour"/> 为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe bool IsContourConvex(ReadOnlySpan<Point> contour)
        {
            PointSetMarshaller.ValidateNotEmpty(contour, nameof(contour));
            return RunPointSetBoolean(contour, nameof(contour), NativeMethods.ImgProcIsContourConvexPtr);
        }
#endif

        /// <summary>
        /// Finds the convex hull of a point set and returns hull points.
        /// 查找点集的凸包并返回凸包顶点。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <param name="clockwise">Whether the output hull should be clockwise. 输出凸包是否按顺时针方向排列。</param>
        /// <returns>The convex hull points. 凸包顶点。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null. 当 <paramref name="points"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Point[] ConvexHull(Point[] points, bool clockwise = false)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            return ConvexHull(points.AsSpan(), clockwise);
#else
            int[] pointsXy = ToInterleavedPoints(points, nameof(points));
            int clockwiseFlag = clockwise ? 1 : 0;
            NativeException.ThrowIfError(NativeMethods.ImgProcConvexHullCount(
                pointsXy,
                points.Length,
                clockwiseFlag,
                out int hullPointCount));

            var hullPointsXy = new int[hullPointCount * 2];
            NativeException.ThrowIfError(NativeMethods.ImgProcConvexHullFill(
                pointsXy,
                points.Length,
                clockwiseFlag,
                hullPointsXy,
                hullPointCount,
                out int writtenCount));

            if (writtenCount != hullPointCount)
            {
                Array.Resize(ref hullPointsXy, writtenCount * 2);
            }

            return FromInterleavedPoints(hullPointsXy, writtenCount);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Finds the convex hull of a point span and returns hull points.
        /// 查找点 Span 的凸包并返回凸包顶点。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <param name="clockwise">Whether the output hull should be clockwise. 输出凸包是否按顺时针方向排列。</param>
        /// <returns>The convex hull points. 凸包顶点。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe Point[] ConvexHull(ReadOnlySpan<Point> points, bool clockwise = false)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));
            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);
            int clockwiseFlag = clockwise ? 1 : 0;

            fixed (int* pointsPtr = pointsXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcConvexHullCountPtr(
                    pointsPtr,
                    points.Length,
                    clockwiseFlag,
                    out int hullPointCount));

                int interleavedLength = hullPointCount * 2;
                int[]? rented = null;
                Span<int> hullPointsXy = interleavedLength <= StackallocInterleavedPointPairThreshold * 2 ?
                    stackalloc int[interleavedLength] :
                    (rented = ArrayPool<int>.Shared.Rent(interleavedLength)).AsSpan(0, interleavedLength);

                try
                {
                    fixed (int* hullPtr = hullPointsXy)
                    {
                        NativeException.ThrowIfError(NativeMethods.ImgProcConvexHullFillPtr(
                            pointsPtr,
                            points.Length,
                            clockwiseFlag,
                            hullPtr,
                            hullPointCount,
                            out int writtenCount));

                        return FromInterleavedPoints(hullPointsXy, writtenCount);
                    }
                }
                finally
                {
                    if (rented != null)
                    {
                        ArrayPool<int>.Shared.Return(rented);
                    }
                }
            }
        }
#endif

        /// <summary>
        /// Finds the convex hull of a point set and returns indices into the original point array.
        /// 查找点集的凸包并返回原始点数组中的索引。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <param name="clockwise">Whether the output hull should be clockwise. 输出凸包是否按顺时针方向排列。</param>
        /// <returns>The convex hull point indices. 凸包点索引。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null. 当 <paramref name="points"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static int[] ConvexHullIndices(Point[] points, bool clockwise = false)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            return ConvexHullIndices(points.AsSpan(), clockwise);
#else
            int[] pointsXy = ToInterleavedPoints(points, nameof(points));
            int clockwiseFlag = clockwise ? 1 : 0;
            NativeException.ThrowIfError(NativeMethods.ImgProcConvexHullIndicesCount(
                pointsXy,
                points.Length,
                clockwiseFlag,
                out int hullIndexCount));

            var hullIndices = new int[hullIndexCount];
            NativeException.ThrowIfError(NativeMethods.ImgProcConvexHullIndicesFill(
                pointsXy,
                points.Length,
                clockwiseFlag,
                hullIndices,
                hullIndexCount,
                out int writtenCount));

            if (writtenCount != hullIndexCount)
            {
                Array.Resize(ref hullIndices, writtenCount);
            }

            return hullIndices;
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Finds the convex hull of a point span and returns indices into the original point span.
        /// 查找点 Span 的凸包并返回原始点 Span 中的索引。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <param name="clockwise">Whether the output hull should be clockwise. 输出凸包是否按顺时针方向排列。</param>
        /// <returns>The convex hull point indices. 凸包点索引。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe int[] ConvexHullIndices(ReadOnlySpan<Point> points, bool clockwise = false)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));
            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);
            int clockwiseFlag = clockwise ? 1 : 0;

            fixed (int* pointsPtr = pointsXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcConvexHullIndicesCountPtr(
                    pointsPtr,
                    points.Length,
                    clockwiseFlag,
                    out int hullIndexCount));

                int[] hullIndices = new int[hullIndexCount];
                fixed (int* hullIndicesPtr = hullIndices)
                {
                    NativeException.ThrowIfError(NativeMethods.ImgProcConvexHullIndicesFillPtr(
                        pointsPtr,
                        points.Length,
                        clockwiseFlag,
                        hullIndicesPtr,
                        hullIndexCount,
                        out int writtenCount));

                    if (writtenCount != hullIndexCount)
                    {
                        Array.Resize(ref hullIndices, writtenCount);
                    }

                    return hullIndices;
                }
            }
        }
#endif

        /// <summary>
        /// Finds convexity defects for a contour and its convex hull indices.
        /// 根据轮廓及其凸包索引查找凸性缺陷。
        /// </summary>
        /// <param name="contour">The input contour. 输入轮廓。</param>
        /// <param name="convexHullIndices">The convex hull indices, usually returned by <see cref="ConvexHullIndices(Point[], bool)"/>. 凸包索引，通常由 <see cref="ConvexHullIndices(Point[], bool)"/> 返回。</param>
        /// <returns>Defects represented as <c>(start_index, end_index, farthest_point_index, fixed_point_depth)</c>. 以 <c>(起点索引, 终点索引, 最远点索引, 定点深度)</c> 表示的缺陷。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contour"/> or <paramref name="convexHullIndices"/> is null. 当 <paramref name="contour"/> 或 <paramref name="convexHullIndices"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="contour"/> or <paramref name="convexHullIndices"/> is empty. 当 <paramref name="contour"/> 或 <paramref name="convexHullIndices"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Vec4i[] ConvexityDefects(Point[] contour, int[] convexHullIndices)
        {
            int[] contourXy = ToInterleavedPoints(contour, nameof(contour));
            ValidateNonEmpty(convexHullIndices, nameof(convexHullIndices));

            NativeException.ThrowIfError(NativeMethods.ImgProcConvexityDefectsCount(
                contourXy,
                contour.Length,
                convexHullIndices,
                convexHullIndices.Length,
                out int defectCount));

            if (defectCount <= 0)
            {
                return Array.Empty<Vec4i>();
            }

            var defects = new int[defectCount * 4];
            NativeException.ThrowIfError(NativeMethods.ImgProcConvexityDefectsFill(
                contourXy,
                contour.Length,
                convexHullIndices,
                convexHullIndices.Length,
                defects,
                defectCount,
                out int writtenCount));

            if (writtenCount != defectCount)
            {
                Array.Resize(ref defects, writtenCount * 4);
            }

            return FromInterleavedVec4i(defects, writtenCount);
        }

        /// <summary>
        /// Finds the minimum-area enclosing circle of a point set.
        /// 查找点集的最小外接圆。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <param name="center">The circle center. 圆心。</param>
        /// <param name="radius">The circle radius. 圆半径。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null. 当 <paramref name="points"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void MinEnclosingCircle(Point[] points, out Point2f center, out float radius)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            MinEnclosingCircle(points.AsSpan(), out center, out radius);
#else
            int[] pointsXy = ToInterleavedPoints(points, nameof(points));
            NativeException.ThrowIfError(NativeMethods.ImgProcMinEnclosingCircle(
                pointsXy,
                points.Length,
                out float centerX,
                out float centerY,
                out radius));
            center = new Point2f(centerX, centerY);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Finds the minimum-area enclosing circle of a point span.
        /// 查找点 Span 的最小外接圆。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <param name="center">The circle center. 圆心。</param>
        /// <param name="radius">The circle radius. 圆半径。</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe void MinEnclosingCircle(ReadOnlySpan<Point> points, out Point2f center, out float radius)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));
            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);

            fixed (int* pointsPtr = pointsXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcMinEnclosingCirclePtr(
                    pointsPtr,
                    points.Length,
                    out float centerX,
                    out float centerY,
                    out radius));
                center = new Point2f(centerX, centerY);
            }
        }
#endif

        /// <summary>
        /// Tests whether a point is inside, outside, or on the edge of a contour.
        /// 判断点位于轮廓内部、外部还是边界上。
        /// </summary>
        /// <param name="contour">The input contour. 输入轮廓。</param>
        /// <param name="pt">The point to test. 要测试的点。</param>
        /// <param name="measureDist">Whether to return a signed distance instead of only the inside/outside sign. 是否返回带符号距离，而不仅是内外部符号。</param>
        /// <returns>A positive value for inside, a negative value for outside, and zero for the contour edge; when <paramref name="measureDist"/> is true, the absolute value is the distance. 正值表示内部，负值表示外部，零表示位于轮廓边界；当 <paramref name="measureDist"/> 为 true 时，绝对值为距离。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contour"/> is null. 当 <paramref name="contour"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="contour"/> is empty. 当 <paramref name="contour"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static double PointPolygonTest(Point[] contour, Point2f pt, bool measureDist)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (contour == null)
            {
                throw new ArgumentNullException(nameof(contour));
            }

            return PointPolygonTest(contour.AsSpan(), pt, measureDist);
#else
            int[] contourXy = ToInterleavedPoints(contour, nameof(contour));
            NativeException.ThrowIfError(NativeMethods.ImgProcPointPolygonTest(
                contourXy,
                contour.Length,
                pt.X,
                pt.Y,
                measureDist ? 1 : 0,
                out double result));
            return result;
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Tests whether a point span contour contains a point.
        /// 判断点 Span 轮廓是否包含某个点。
        /// </summary>
        /// <param name="contour">The input contour. 输入轮廓。</param>
        /// <param name="pt">The point to test. 要测试的点。</param>
        /// <param name="measureDist">Whether to return a signed distance instead of only the inside/outside sign. 是否返回带符号距离，而不仅是内外部符号。</param>
        /// <returns>A positive value for inside, a negative value for outside, and zero for the contour edge; when <paramref name="measureDist"/> is true, the absolute value is the distance. 正值表示内部，负值表示外部，零表示位于轮廓边界；当 <paramref name="measureDist"/> 为 true 时，绝对值为距离。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="contour"/> is empty. 当 <paramref name="contour"/> 为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe double PointPolygonTest(ReadOnlySpan<Point> contour, Point2f pt, bool measureDist)
        {
            PointSetMarshaller.ValidateNotEmpty(contour, nameof(contour));
            ReadOnlySpan<int> contourXy = PointSetMarshaller.AsInterleaved(contour);

            fixed (int* contourPtr = contourXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcPointPolygonTestPtr(
                    contourPtr,
                    contour.Length,
                    pt.X,
                    pt.Y,
                    measureDist ? 1 : 0,
                    out double result));
                return result;
            }
        }
#endif

        /// <summary>
        /// Compares two shapes represented by contours.
        /// 比较由轮廓表示的两个形状。
        /// </summary>
        /// <param name="contour1">The first contour. 第一个轮廓。</param>
        /// <param name="contour2">The second contour. 第二个轮廓。</param>
        /// <param name="method">The shape comparison method. 形状比较方法。</param>
        /// <param name="parameter">The method-specific parameter. 方法相关参数。</param>
        /// <returns>The shape distance; lower values mean more similar shapes. 形状距离；值越小表示形状越相似。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contour1"/> or <paramref name="contour2"/> is null. 当 <paramref name="contour1"/> 或 <paramref name="contour2"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="contour1"/> or <paramref name="contour2"/> is empty. 当 <paramref name="contour1"/> 或 <paramref name="contour2"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static double MatchShapes(Point[] contour1, Point[] contour2, ShapeMatchModes method, double parameter = 0)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (contour1 == null)
            {
                throw new ArgumentNullException(nameof(contour1));
            }

            if (contour2 == null)
            {
                throw new ArgumentNullException(nameof(contour2));
            }

            return MatchShapes(contour1.AsSpan(), contour2.AsSpan(), method, parameter);
#else
            int[] contour1Xy = ToInterleavedPoints(contour1, nameof(contour1));
            int[] contour2Xy = ToInterleavedPoints(contour2, nameof(contour2));
            ValidateShapeMatchMode(method, nameof(method));
            NativeException.ThrowIfError(NativeMethods.ImgProcMatchShapes(
                contour1Xy,
                contour1.Length,
                contour2Xy,
                contour2.Length,
                (int)method,
                parameter,
                out double result));
            return result;
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Compares two shapes represented by point span contours.
        /// 比较由点 Span 轮廓表示的两个形状。
        /// </summary>
        /// <param name="contour1">The first contour. 第一个轮廓。</param>
        /// <param name="contour2">The second contour. 第二个轮廓。</param>
        /// <param name="method">The shape comparison method. 形状比较方法。</param>
        /// <param name="parameter">The method-specific parameter. 方法相关参数。</param>
        /// <returns>The shape distance; lower values mean more similar shapes. 形状距离；值越小表示形状越相似。</returns>
        /// <exception cref="ArgumentException">Thrown when either contour is empty. 当任一轮廓为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe double MatchShapes(ReadOnlySpan<Point> contour1, ReadOnlySpan<Point> contour2, ShapeMatchModes method, double parameter = 0)
        {
            PointSetMarshaller.ValidateNotEmpty(contour1, nameof(contour1));
            PointSetMarshaller.ValidateNotEmpty(contour2, nameof(contour2));
            ValidateShapeMatchMode(method, nameof(method));

            ReadOnlySpan<int> contour1Xy = PointSetMarshaller.AsInterleaved(contour1);
            ReadOnlySpan<int> contour2Xy = PointSetMarshaller.AsInterleaved(contour2);

            fixed (int* contour1Ptr = contour1Xy)
            fixed (int* contour2Ptr = contour2Xy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcMatchShapesPtr(
                    contour1Ptr,
                    contour1.Length,
                    contour2Ptr,
                    contour2.Length,
                    (int)method,
                    parameter,
                    out double result));
                return result;
            }
        }
#endif

        /// <summary>
        /// Finds the minimum-area rotated rectangle enclosing a point set.
        /// 查找包围点集的最小面积旋转矩形。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <returns>The minimum-area rotated rectangle. 最小面积旋转矩形。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null. 当 <paramref name="points"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static RotatedRect MinAreaRect(Point[] points)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            return MinAreaRect(points.AsSpan());
#else
            int[] pointsXy = ToInterleavedPoints(points, nameof(points));
            NativeException.ThrowIfError(NativeMethods.ImgProcMinAreaRect(
                pointsXy,
                points.Length,
                out float centerX,
                out float centerY,
                out float width,
                out float height,
                out float angle));
            return ToRotatedRect(centerX, centerY, width, height, angle);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Finds the minimum-area rotated rectangle enclosing a point span.
        /// 查找包围点 Span 的最小面积旋转矩形。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <returns>The minimum-area rotated rectangle. 最小面积旋转矩形。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe RotatedRect MinAreaRect(ReadOnlySpan<Point> points)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));
            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);

            fixed (int* pointsPtr = pointsXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcMinAreaRectPtr(
                    pointsPtr,
                    points.Length,
                    out float centerX,
                    out float centerY,
                    out float width,
                    out float height,
                    out float angle));
                return ToRotatedRect(centerX, centerY, width, height, angle);
            }
        }
#endif

        /// <summary>
        /// Finds the four vertices of a rotated rectangle.
        /// 查找旋转矩形的四个顶点。
        /// </summary>
        /// <param name="box">The rotated rectangle. 旋转矩形。</param>
        /// <returns>The four rectangle vertices. 矩形的四个顶点。</returns>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Point2f[] BoxPoints(RotatedRect box)
        {
            var pointsXy = new float[8];
            NativeException.ThrowIfError(NativeMethods.ImgProcBoxPoints(
                box.Center.X,
                box.Center.Y,
                box.Size.Width,
                box.Size.Height,
                box.Angle,
                pointsXy,
                4));
            return FromInterleavedPoint2f(pointsXy, 4);
        }

        /// <summary>
        /// Fits an ellipse around a set of at least five points.
        /// 围绕至少五个点拟合椭圆。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <returns>The rotated rectangle in which the fitted ellipse is inscribed. 拟合椭圆的外接旋转矩形。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null. 当 <paramref name="points"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> contains fewer than five points. 当 <paramref name="points"/> 少于五个点时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static RotatedRect FitEllipse(Point[] points)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            return FitEllipse(points.AsSpan());
#else
            return RunFitEllipse(points, nameof(points), NativeMethods.ImgProcFitEllipse);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Fits an ellipse around a point span. 围绕点 Span 拟合椭圆。</summary>
        public static unsafe RotatedRect FitEllipse(ReadOnlySpan<Point> points)
        {
            return RunFitEllipse(points, nameof(points), NativeMethods.ImgProcFitEllipsePtr);
        }
#endif

        /// <summary>
        /// Fits an ellipse around a set of at least five points using the AMS method.
        /// 使用 AMS 方法围绕至少五个点拟合椭圆。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <returns>The rotated rectangle in which the fitted ellipse is inscribed. 拟合椭圆的外接旋转矩形。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null. 当 <paramref name="points"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> contains fewer than five points. 当 <paramref name="points"/> 少于五个点时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static RotatedRect FitEllipseAMS(Point[] points)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            return FitEllipseAMS(points.AsSpan());
#else
            return RunFitEllipse(points, nameof(points), NativeMethods.ImgProcFitEllipseAMS);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Fits an ellipse around a point span using the AMS method. 使用 AMS 方法围绕点 Span 拟合椭圆。</summary>
        public static unsafe RotatedRect FitEllipseAMS(ReadOnlySpan<Point> points)
        {
            return RunFitEllipse(points, nameof(points), NativeMethods.ImgProcFitEllipseAMSPtr);
        }
#endif

        /// <summary>
        /// Fits an ellipse around a set of at least five points using the direct least-squares method.
        /// 使用直接最小二乘方法围绕至少五个点拟合椭圆。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <returns>The rotated rectangle in which the fitted ellipse is inscribed. 拟合椭圆的外接旋转矩形。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null. 当 <paramref name="points"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> contains fewer than five points. 当 <paramref name="points"/> 少于五个点时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static RotatedRect FitEllipseDirect(Point[] points)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            return FitEllipseDirect(points.AsSpan());
#else
            return RunFitEllipse(points, nameof(points), NativeMethods.ImgProcFitEllipseDirect);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Fits an ellipse around a point span using direct least squares. 使用直接最小二乘方法围绕点 Span 拟合椭圆。</summary>
        public static unsafe RotatedRect FitEllipseDirect(ReadOnlySpan<Point> points)
        {
            return RunFitEllipse(points, nameof(points), NativeMethods.ImgProcFitEllipseDirectPtr);
        }
#endif

        /// <summary>
        /// Finds the intersection region between two rotated rectangles.
        /// 查找两个旋转矩形之间的相交区域。
        /// </summary>
        /// <param name="rect1">The first rotated rectangle. 第一个旋转矩形。</param>
        /// <param name="rect2">The second rotated rectangle. 第二个旋转矩形。</param>
        /// <param name="intersectingRegion">The vertices of the intersecting region. 相交区域的顶点。</param>
        /// <returns>The rectangle intersection type. 矩形相交类型。</returns>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static RectanglesIntersectTypes RotatedRectangleIntersection(
            RotatedRect rect1,
            RotatedRect rect2,
            out Point2f[] intersectingRegion)
        {
            NativeException.ThrowIfError(NativeMethods.ImgProcRotatedRectangleIntersectionCount(
                rect1.Center.X,
                rect1.Center.Y,
                rect1.Size.Width,
                rect1.Size.Height,
                rect1.Angle,
                rect2.Center.X,
                rect2.Center.Y,
                rect2.Size.Width,
                rect2.Size.Height,
                rect2.Angle,
                out int intersectionType,
                out int pointCount));

            if (pointCount <= 0)
            {
                intersectingRegion = Array.Empty<Point2f>();
                return (RectanglesIntersectTypes)intersectionType;
            }

            var pointsXy = new float[pointCount * 2];
            NativeException.ThrowIfError(NativeMethods.ImgProcRotatedRectangleIntersectionFill(
                rect1.Center.X,
                rect1.Center.Y,
                rect1.Size.Width,
                rect1.Size.Height,
                rect1.Angle,
                rect2.Center.X,
                rect2.Center.Y,
                rect2.Size.Width,
                rect2.Size.Height,
                rect2.Angle,
                pointsXy,
                pointCount,
                out intersectionType,
                out int writtenCount));

            intersectingRegion = FromInterleavedPoint2f(pointsXy, writtenCount);
            return (RectanglesIntersectTypes)intersectionType;
        }

        /// <summary>
        /// Computes closest points on an ellipse for the supplied point set.
        /// 计算输入点集在椭圆上的最近点。
        /// </summary>
        /// <param name="ellipseParams">The ellipse parameters represented as a rotated rectangle. 以旋转矩形表示的椭圆参数。</param>
        /// <param name="points">The input points. 输入点集。</param>
        /// <returns>The closest points on the ellipse. 椭圆上的最近点。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null. 当 <paramref name="points"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Point2f[] GetClosestEllipsePoints(RotatedRect ellipseParams, Point[] points)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            return GetClosestEllipsePoints(ellipseParams, points.AsSpan());
#else
            int[] pointsXy = ToInterleavedPoints(points, nameof(points));
            var closestPointsXy = new float[points.Length * 2];
            NativeException.ThrowIfError(NativeMethods.ImgProcGetClosestEllipsePoints(
                ellipseParams.Center.X,
                ellipseParams.Center.Y,
                ellipseParams.Size.Width,
                ellipseParams.Size.Height,
                ellipseParams.Angle,
                pointsXy,
                points.Length,
                closestPointsXy,
                points.Length));
            return FromInterleavedPoint2f(closestPointsXy, points.Length);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Computes closest points on an ellipse for a point span. 计算点 Span 在椭圆上的最近点。</summary>
        public static unsafe Point2f[] GetClosestEllipsePoints(RotatedRect ellipseParams, ReadOnlySpan<Point> points)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));
            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);
            var result = new Point2f[points.Length];
            Span<float> resultXy = PointSetMarshaller.AsInterleaved(result.AsSpan());
            fixed (int* pointsPtr = pointsXy)
            fixed (float* resultPtr = resultXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcGetClosestEllipsePointsPtr(
                    ellipseParams.Center.X,
                    ellipseParams.Center.Y,
                    ellipseParams.Size.Width,
                    ellipseParams.Size.Height,
                    ellipseParams.Angle,
                    pointsPtr,
                    points.Length,
                    resultPtr,
                    result.Length));
            }

            return result;
        }
#endif

        /// <summary>
        /// Finds a minimum-area triangle enclosing a point set.
        /// 查找包围点集的最小面积三角形。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <param name="triangle">The three triangle vertices. 三角形的三个顶点。</param>
        /// <returns>The area of the enclosing triangle. 外接三角形面积。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null. 当 <paramref name="points"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static double MinEnclosingTriangle(Point[] points, out Point2f[] triangle)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            return MinEnclosingTriangle(points.AsSpan(), out triangle);
#else
            int[] pointsXy = ToInterleavedPoints(points, nameof(points));
            var trianglePointsXy = new float[6];
            NativeException.ThrowIfError(NativeMethods.ImgProcMinEnclosingTriangle(
                pointsXy,
                points.Length,
                trianglePointsXy,
                3,
                out double area));
            triangle = FromInterleavedPoint2f(trianglePointsXy, 3);
            return area;
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Finds a minimum-area triangle enclosing a point span. 查找包围点 Span 的最小面积三角形。</summary>
        public static unsafe double MinEnclosingTriangle(ReadOnlySpan<Point> points, out Point2f[] triangle)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));
            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);
            triangle = new Point2f[3];
            Span<float> triangleXy = PointSetMarshaller.AsInterleaved(triangle.AsSpan());
            fixed (int* pointsPtr = pointsXy)
            fixed (float* trianglePtr = triangleXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcMinEnclosingTrianglePtr(
                    pointsPtr,
                    points.Length,
                    trianglePtr,
                    triangle.Length,
                    out double area));
                return area;
            }
        }
#endif

        /// <summary>
        /// Finds a minimum-area convex polygon with the specified number of vertices enclosing a point set.
        /// 查找以指定顶点数包围点集的最小面积凸多边形。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <param name="k">The number of polygon vertices. 多边形顶点数。</param>
        /// <param name="polygon">The enclosing polygon vertices. 外接多边形顶点。</param>
        /// <returns>The area of the enclosing convex polygon. 外接凸多边形面积。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null. 当 <paramref name="points"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空数组时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="k"/> is less than three. 当 <paramref name="k"/> 小于三时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static double MinEnclosingConvexPolygon(Point[] points, int k, out Point2f[] polygon)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            return MinEnclosingConvexPolygon(points.AsSpan(), k, out polygon);
#else
            if (k < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(k), "Polygon vertex count must be at least three.");
            }

            int[] pointsXy = ToInterleavedPoints(points, nameof(points));
            var polygonPointsXy = new float[k * 2];
            NativeException.ThrowIfError(NativeMethods.ImgProcMinEnclosingConvexPolygon(
                pointsXy,
                points.Length,
                k,
                polygonPointsXy,
                k,
                out int polygonPointCount,
                out double area));

            if (polygonPointCount != k)
            {
                Array.Resize(ref polygonPointsXy, polygonPointCount * 2);
            }

            polygon = FromInterleavedPoint2f(polygonPointsXy, polygonPointCount);
            return area;
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Finds a minimum-area convex polygon enclosing a point span. 查找包围点 Span 的最小面积凸多边形。</summary>
        public static unsafe double MinEnclosingConvexPolygon(ReadOnlySpan<Point> points, int k, out Point2f[] polygon)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));
            if (k < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(k), "Polygon vertex count must be at least three.");
            }

            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);
            polygon = new Point2f[k];
            Span<float> polygonXy = PointSetMarshaller.AsInterleaved(polygon.AsSpan());
            fixed (int* pointsPtr = pointsXy)
            fixed (float* polygonPtr = polygonXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcMinEnclosingConvexPolygonPtr(
                    pointsPtr,
                    points.Length,
                    k,
                    polygonPtr,
                    polygon.Length,
                    out int polygonPointCount,
                    out double area));
                if (polygonPointCount != polygon.Length)
                {
                    Array.Resize(ref polygon, polygonPointCount);
                }
                return area;
            }
        }
#endif

        /// <summary>
        /// Finds the intersection polygon of two convex polygons.
        /// 查找两个凸多边形的相交多边形。
        /// </summary>
        /// <param name="polygon1">The first convex polygon. 第一个凸多边形。</param>
        /// <param name="polygon2">The second convex polygon. 第二个凸多边形。</param>
        /// <param name="intersectingRegion">The intersecting polygon vertices. 相交多边形顶点。</param>
        /// <param name="handleNested">Whether nested polygons should be treated as intersecting. 是否将包含关系的多边形视为相交。</param>
        /// <returns>The area of the intersecting polygon. 相交多边形面积。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="polygon1"/> or <paramref name="polygon2"/> is null. 当 <paramref name="polygon1"/> 或 <paramref name="polygon2"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="polygon1"/> or <paramref name="polygon2"/> is empty. 当 <paramref name="polygon1"/> 或 <paramref name="polygon2"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static float IntersectConvexConvex(
            Point[] polygon1,
            Point[] polygon2,
            out Point2f[] intersectingRegion,
            bool handleNested = true)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (polygon1 == null)
            {
                throw new ArgumentNullException(nameof(polygon1));
            }
            if (polygon2 == null)
            {
                throw new ArgumentNullException(nameof(polygon2));
            }

            return IntersectConvexConvex(polygon1.AsSpan(), polygon2.AsSpan(), out intersectingRegion, handleNested);
#else
            int[] polygon1Xy = ToInterleavedPoints(polygon1, nameof(polygon1));
            int[] polygon2Xy = ToInterleavedPoints(polygon2, nameof(polygon2));
            int handleNestedFlag = handleNested ? 1 : 0;

            NativeException.ThrowIfError(NativeMethods.ImgProcIntersectConvexConvexCount(
                polygon1Xy,
                polygon1.Length,
                polygon2Xy,
                polygon2.Length,
                handleNestedFlag,
                out float area,
                out int pointCount));

            if (pointCount <= 0)
            {
                intersectingRegion = Array.Empty<Point2f>();
                return area;
            }

            var intersectingPointsXy = new float[pointCount * 2];
            NativeException.ThrowIfError(NativeMethods.ImgProcIntersectConvexConvexFill(
                polygon1Xy,
                polygon1.Length,
                polygon2Xy,
                polygon2.Length,
                handleNestedFlag,
                intersectingPointsXy,
                pointCount,
                out area,
                out int writtenCount));

            if (writtenCount != pointCount)
            {
                Array.Resize(ref intersectingPointsXy, writtenCount * 2);
            }

            intersectingRegion = FromInterleavedPoint2f(intersectingPointsXy, writtenCount);
            return area;
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Finds the intersection polygon of two convex point spans. 查找两个凸多边形点 Span 的相交多边形。</summary>
        public static unsafe float IntersectConvexConvex(
            ReadOnlySpan<Point> polygon1,
            ReadOnlySpan<Point> polygon2,
            out Point2f[] intersectingRegion,
            bool handleNested = true)
        {
            PointSetMarshaller.ValidateNotEmpty(polygon1, nameof(polygon1));
            PointSetMarshaller.ValidateNotEmpty(polygon2, nameof(polygon2));
            ReadOnlySpan<int> polygon1Xy = PointSetMarshaller.AsInterleaved(polygon1);
            ReadOnlySpan<int> polygon2Xy = PointSetMarshaller.AsInterleaved(polygon2);
            fixed (int* polygon1Ptr = polygon1Xy)
            fixed (int* polygon2Ptr = polygon2Xy)
            {
                int handleNestedFlag = handleNested ? 1 : 0;
                NativeException.ThrowIfError(NativeMethods.ImgProcIntersectConvexConvexCountPtr(
                    polygon1Ptr,
                    polygon1.Length,
                    polygon2Ptr,
                    polygon2.Length,
                    handleNestedFlag,
                    out float area,
                    out int pointCount));
                if (pointCount <= 0)
                {
                    intersectingRegion = Array.Empty<Point2f>();
                    return area;
                }

                intersectingRegion = new Point2f[pointCount];
                Span<float> intersectingXy = PointSetMarshaller.AsInterleaved(intersectingRegion.AsSpan());
                fixed (float* intersectingPtr = intersectingXy)
                {
                    NativeException.ThrowIfError(NativeMethods.ImgProcIntersectConvexConvexFillPtr(
                        polygon1Ptr,
                        polygon1.Length,
                        polygon2Ptr,
                        polygon2.Length,
                        handleNestedFlag,
                        intersectingPtr,
                        intersectingRegion.Length,
                        out area,
                        out int writtenCount));
                    if (writtenCount != intersectingRegion.Length)
                    {
                        Array.Resize(ref intersectingRegion, writtenCount);
                    }
                }

                return area;
            }
        }
#endif

        /// <summary>
        /// Fits a two-dimensional line to a point set.
        /// 拟合二维点集的直线。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <param name="distType">The distance metric. 距离度量。</param>
        /// <param name="param">The distance metric parameter, or 0 to choose automatically. 距离度量参数，0 表示自动选择。</param>
        /// <param name="reps">The radius accuracy. 半径精度。</param>
        /// <param name="aeps">The angle accuracy. 角度精度。</param>
        /// <returns>A vector containing <c>vx</c>, <c>vy</c>, <c>x0</c>, and <c>y0</c>. 包含 <c>vx</c>、<c>vy</c>、<c>x0</c> 和 <c>y0</c> 的向量。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null. 当 <paramref name="points"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Vec4f FitLine(
            Point[] points,
            DistanceTypes distType,
            double param,
            double reps,
            double aeps)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            return FitLine(points.AsSpan(), distType, param, reps, aeps);
#else
            int[] pointsXy = ToInterleavedPoints(points, nameof(points));
            NativeException.ThrowIfError(NativeMethods.ImgProcFitLine2D(
                pointsXy,
                points.Length,
                (int)distType,
                param,
                reps,
                aeps,
                out float vx,
                out float vy,
                out float x0,
                out float y0));
            return new Vec4f(vx, vy, x0, y0);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Fits a two-dimensional line to a point span.
        /// 拟合点 Span 的二维直线。
        /// </summary>
        /// <param name="points">The input points. 输入点集。</param>
        /// <param name="distType">The distance metric. 距离度量。</param>
        /// <param name="param">The distance metric parameter, or 0 to choose automatically. 距离度量参数，0 表示自动选择。</param>
        /// <param name="reps">The radius accuracy. 半径精度。</param>
        /// <param name="aeps">The angle accuracy. 角度精度。</param>
        /// <returns>A vector containing <c>vx</c>, <c>vy</c>, <c>x0</c>, and <c>y0</c>. 包含 <c>vx</c>、<c>vy</c>、<c>x0</c> 和 <c>y0</c> 的向量。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe Vec4f FitLine(ReadOnlySpan<Point> points, DistanceTypes distType, double param, double reps, double aeps)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));
            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);

            fixed (int* pointsPtr = pointsXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcFitLine2DPtr(
                    pointsPtr,
                    points.Length,
                    (int)distType,
                    param,
                    reps,
                    aeps,
                    out float vx,
                    out float vy,
                    out float x0,
                    out float y0));
                return new Vec4f(vx, vy, x0, y0);
            }
        }
#endif

        /// <summary>
        /// Draws a rectangle specified by two opposite corners.
        /// 绘制由两个对角点指定的矩形。
        /// </summary>
        /// <param name="img">The image to draw on. 要绘制的图像。</param>
        /// <param name="pt1">The first rectangle corner. 矩形的第一个角点。</param>
        /// <param name="pt2">The opposite rectangle corner. 矩形的对角点。</param>
        /// <param name="color">The rectangle color. 矩形颜色。</param>
        /// <param name="thickness">The line thickness, or a negative value to fill the rectangle. 线条粗细；负数表示填充矩形。</param>
        /// <param name="lineType">The line drawing algorithm. 线条绘制算法。</param>
        /// <param name="shift">The number of fractional bits in point coordinates. 点坐标中的小数位位数。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="img"/> is null. 当 <paramref name="img"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Rectangle(
            Mat img,
            Point pt1,
            Point pt2,
            Scalar color,
            int thickness = 1,
            LineTypes lineType = LineTypes.Line8,
            int shift = 0)
        {
            if (img == null)
            {
                throw new ArgumentNullException(nameof(img));
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcRectangle(
                img.NativeHandle,
                pt1.X,
                pt1.Y,
                pt2.X,
                pt2.Y,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                thickness,
                (int)lineType,
                shift));
        }

        /// <summary>
        /// Draws a rectangle specified by a rectangle value.
        /// 绘制由矩形值指定的矩形。
        /// </summary>
        /// <param name="img">The image to draw on. 要绘制的图像。</param>
        /// <param name="rect">The rectangle to draw. 要绘制的矩形。</param>
        /// <param name="color">The rectangle color. 矩形颜色。</param>
        /// <param name="thickness">The line thickness, or a negative value to fill the rectangle. 线条粗细；负数表示填充矩形。</param>
        /// <param name="lineType">The line drawing algorithm. 线条绘制算法。</param>
        /// <param name="shift">The number of fractional bits in rectangle coordinates. 矩形坐标中的小数位位数。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="img"/> is null. 当 <paramref name="img"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Rectangle(
            Mat img,
            Rect rect,
            Scalar color,
            int thickness = 1,
            LineTypes lineType = LineTypes.Line8,
            int shift = 0)
        {
            if (img == null)
            {
                throw new ArgumentNullException(nameof(img));
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcRectangleByRect(
                img.NativeHandle,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                thickness,
                (int)lineType,
                shift));
        }

        /// <summary>
        /// Draws a circle.
        /// 绘制圆形。
        /// </summary>
        /// <param name="img">The image to draw on. 要绘制的图像。</param>
        /// <param name="center">The circle center. 圆心。</param>
        /// <param name="radius">The circle radius. 圆半径。</param>
        /// <param name="color">The circle color. 圆颜色。</param>
        /// <param name="thickness">The outline thickness, or a negative value to fill the circle. 轮廓粗细；负数表示填充圆。</param>
        /// <param name="lineType">The line drawing algorithm. 线条绘制算法。</param>
        /// <param name="shift">The number of fractional bits in center coordinates and radius. 圆心坐标和半径中的小数位位数。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="img"/> is null. 当 <paramref name="img"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Circle(
            Mat img,
            Point center,
            int radius,
            Scalar color,
            int thickness = 1,
            LineTypes lineType = LineTypes.Line8,
            int shift = 0)
        {
            if (img == null)
            {
                throw new ArgumentNullException(nameof(img));
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcCircle(
                img.NativeHandle,
                center.X,
                center.Y,
                radius,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                thickness,
                (int)lineType,
                shift));
        }

        /// <summary>
        /// Draws an ellipse, an elliptic arc, or a filled ellipse sector.
        /// 绘制椭圆、椭圆弧或填充的椭圆扇区。
        /// </summary>
        /// <param name="img">The image to draw on. 要绘制的图像。</param>
        /// <param name="center">The ellipse center. 椭圆中心。</param>
        /// <param name="axes">Half of the ellipse axis sizes. 椭圆主轴尺寸的一半。</param>
        /// <param name="angle">The ellipse rotation angle in degrees. 椭圆旋转角度，单位为度。</param>
        /// <param name="startAngle">The starting angle of the elliptic arc in degrees. 椭圆弧起始角度，单位为度。</param>
        /// <param name="endAngle">The ending angle of the elliptic arc in degrees. 椭圆弧结束角度，单位为度。</param>
        /// <param name="color">The ellipse color. 椭圆颜色。</param>
        /// <param name="thickness">The outline thickness, or a negative value to fill the ellipse sector. 轮廓粗细；负数表示填充椭圆扇区。</param>
        /// <param name="lineType">The line drawing algorithm. 线条绘制算法。</param>
        /// <param name="shift">The number of fractional bits in center coordinates and axis values. 中心坐标和轴尺寸中的小数位位数。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="img"/> is null. 当 <paramref name="img"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Ellipse(
            Mat img,
            Point center,
            Size axes,
            double angle,
            double startAngle,
            double endAngle,
            Scalar color,
            int thickness = 1,
            LineTypes lineType = LineTypes.Line8,
            int shift = 0)
        {
            if (img == null)
            {
                throw new ArgumentNullException(nameof(img));
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcEllipse(
                img.NativeHandle,
                center.X,
                center.Y,
                axes.Width,
                axes.Height,
                angle,
                startAngle,
                endAngle,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                thickness,
                (int)lineType,
                shift));
        }

        /// <summary>
        /// Draws a text string.
        /// 绘制文本字符串。
        /// </summary>
        /// <param name="img">The image to draw on. 要绘制的图像。</param>
        /// <param name="text">The text string to draw. 要绘制的文本字符串。</param>
        /// <param name="org">The bottom-left corner of the text string in the image. 文本字符串在图像中的左下角位置。</param>
        /// <param name="fontFace">The font face. 字体。</param>
        /// <param name="fontScale">The font scale factor. 字体缩放因子。</param>
        /// <param name="color">The text color. 文本颜色。</param>
        /// <param name="thickness">The thickness of lines used to draw the text. 绘制文本所用线条的粗细。</param>
        /// <param name="lineType">The line drawing algorithm. 线条绘制算法。</param>
        /// <param name="bottomLeftOrigin">Whether the image data origin is at the bottom-left corner. 图像数据原点是否位于左下角。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="img"/> or <paramref name="text"/> is null. 当 <paramref name="img"/> 或 <paramref name="text"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="text"/> is empty. 当 <paramref name="text"/> 为空字符串时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void PutText(
            Mat img,
            string text,
            Point org,
            HersheyFonts fontFace,
            double fontScale,
            Scalar color,
            int thickness = 1,
            LineTypes lineType = LineTypes.Line8,
            bool bottomLeftOrigin = false)
        {
            if (img == null)
            {
                throw new ArgumentNullException(nameof(img));
            }

            byte[] nativeText = ToNullTerminatedUtf8Text(text, nameof(text));
            ValidateHersheyFontFace(fontFace, nameof(fontFace));
            NativeException.ThrowIfError(NativeMethods.ImgProcPutText(
                img.NativeHandle,
                nativeText,
                org.X,
                org.Y,
                (int)fontFace,
                fontScale,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                thickness,
                (int)lineType,
                bottomLeftOrigin ? 1 : 0));
        }

        /// <summary>
        /// Calculates the size of a box that contains the specified text.
        /// 计算包含指定文本的矩形框大小。
        /// </summary>
        /// <param name="text">The input text string. 输入文本字符串。</param>
        /// <param name="fontFace">The font face. 字体。</param>
        /// <param name="fontScale">The font scale factor. 字体缩放因子。</param>
        /// <param name="thickness">The thickness of lines used to render the text. 渲染文本所用线条的粗细。</param>
        /// <param name="baseLine">The y-coordinate of the baseline relative to the bottom-most text point. 相对于文本最低点的基线 y 坐标。</param>
        /// <returns>The size of the text bounding box. 文本边界框大小。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null. 当 <paramref name="text"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="text"/> is empty. 当 <paramref name="text"/> 为空字符串时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Size GetTextSize(
            string text,
            HersheyFonts fontFace,
            double fontScale,
            int thickness,
            out int baseLine)
        {
            byte[] nativeText = ToNullTerminatedUtf8Text(text, nameof(text));
            ValidateHersheyFontFace(fontFace, nameof(fontFace));

            int width;
            int height;
            NativeException.ThrowIfError(NativeMethods.ImgProcGetTextSize(
                nativeText,
                (int)fontFace,
                fontScale,
                thickness,
                out width,
                out height,
                out baseLine));

            return new Size(width, height);
        }

        private delegate int MorphologyNativeMethod(
            IntPtr src,
            IntPtr dst,
            IntPtr kernel,
            int anchorX,
            int anchorY,
            int iterations,
            int borderType,
            int hasBorderValue,
            double borderValueV0,
            double borderValueV1,
            double borderValueV2,
            double borderValueV3);

        private delegate int MorphologyExNativeMethod(
            IntPtr src,
            IntPtr dst,
            int op,
            IntPtr kernel,
            int anchorX,
            int anchorY,
            int iterations,
            int borderType,
            int hasBorderValue,
            double borderValueV0,
            double borderValueV1,
            double borderValueV2,
            double borderValueV3);

        private delegate int FitEllipseNativeMethod(
            int[] pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

#if NETCOREAPP3_1_OR_GREATER
        private unsafe delegate int PointSetScalarNativeMethod(
            int* pointsXy,
            int pointCount,
            int flag,
            out double result);

        private unsafe delegate int PointSetRectNativeMethod(
            int* pointsXy,
            int pointCount,
            out int x,
            out int y,
            out int width,
            out int height);

        private unsafe delegate int PointSetBooleanNativeMethod(
            int* pointsXy,
            int pointCount,
            out int result);

        private unsafe delegate int PointSetRotatedRectNativeMethod(
            int* pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);

        private unsafe delegate int FitEllipseNativePtrMethod(
            int* pointsXy,
            int pointCount,
            out float centerX,
            out float centerY,
            out float width,
            out float height,
            out float angle);
#endif

        private static void RunMorphology(
            MorphologyNativeMethod nativeMethod,
            Mat src,
            Mat dst,
            Mat kernel,
            Point? anchor,
            int iterations,
            BorderTypes borderType,
            Scalar? borderValue)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (dst == null)
            {
                throw new ArgumentNullException(nameof(dst));
            }

            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (iterations < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations), "Iterations cannot be negative.");
            }

            Point actualAnchor = anchor ?? new Point(-1, -1);
            Scalar actualBorderValue = borderValue ?? new Scalar();

            NativeException.ThrowIfError(nativeMethod(
                src.NativeHandle,
                dst.NativeHandle,
                kernel.NativeHandle,
                actualAnchor.X,
                actualAnchor.Y,
                iterations,
                (int)borderType,
                borderValue.HasValue ? 1 : 0,
                actualBorderValue.V0,
                actualBorderValue.V1,
                actualBorderValue.V2,
                actualBorderValue.V3));
        }

        private static void ValidateMatPair(Mat src, Mat dst)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidatePositiveSize(Size size, string parameterName)
        {
            if (size.Width <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Width must be positive.");
            }

            if (size.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Height must be positive.");
            }
        }

        private static void ValidateNonNegativeSize(Size size, string parameterName)
        {
            if (size.Width < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Width cannot be negative.");
            }

            if (size.Height < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Height cannot be negative.");
            }
        }

        private static void ValidateOddGreaterThanOne(int value, string parameterName)
        {
            if (value <= 1 || value % 2 == 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be an odd number greater than one.");
            }
        }

        private static void ValidateMorphShape(MorphShapes value, string parameterName)
        {
            if (value != MorphShapes.Rect
                && value != MorphShapes.Cross
                && value != MorphShapes.Ellipse
                && value != MorphShapes.Diamond)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported structuring element shape.");
            }
        }

        private static void ValidateMorphType(MorphTypes value, string parameterName)
        {
            if (value < MorphTypes.Erode || value > MorphTypes.HitMiss)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported morphological operation type.");
            }
        }

        private static void ValidateShapeMatchMode(ShapeMatchModes value, string parameterName)
        {
            if (value < ShapeMatchModes.I1 || value > ShapeMatchModes.I3)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported shape matching method.");
            }
        }

        private static void ValidateLineSegmentDetectorMode(LineSegmentDetectorModes value, string parameterName)
        {
            if (value < LineSegmentDetectorModes.None || value > LineSegmentDetectorModes.Advanced)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported line segment detector refinement mode.");
            }
        }

        private static void ValidateHistogramComparisonType(HistogramComparisonTypes value, string parameterName)
        {
            if (value < HistogramComparisonTypes.Correl || value > HistogramComparisonTypes.KlDiv)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported histogram comparison method.");
            }
        }

        private static void ValidateHersheyFontFace(HersheyFonts value, string parameterName)
        {
            const int baseFontMask = 0x07;
            const int italicMask = (int)HersheyFonts.Italic;
            const int allowedMask = baseFontMask | italicMask;
            int rawValue = (int)value;

            if ((rawValue & ~allowedMask) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported Hershey font face.");
            }
        }

        private static void RunMorphologyEx(
            MorphologyExNativeMethod nativeMethod,
            Mat src,
            Mat dst,
            MorphTypes op,
            Mat kernel,
            Point? anchor,
            int iterations,
            BorderTypes borderType,
            Scalar? borderValue)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (dst == null)
            {
                throw new ArgumentNullException(nameof(dst));
            }

            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            if (iterations < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations), "Iterations cannot be negative.");
            }

            ValidateMorphType(op, nameof(op));

            Point actualAnchor = anchor ?? new Point(-1, -1);
            Scalar actualBorderValue = borderValue ?? new Scalar();

            NativeException.ThrowIfError(nativeMethod(
                src.NativeHandle,
                dst.NativeHandle,
                (int)op,
                kernel.NativeHandle,
                actualAnchor.X,
                actualAnchor.Y,
                iterations,
                (int)borderType,
                borderValue.HasValue ? 1 : 0,
                actualBorderValue.V0,
                actualBorderValue.V1,
                actualBorderValue.V2,
                actualBorderValue.V3));
        }

        private static byte[] ToNullTerminatedUtf8Text(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (value.Length == 0)
            {
                throw new ArgumentException("Text cannot be empty.", parameterName);
            }

            int byteCount = Encoding.UTF8.GetByteCount(value);
            var buffer = new byte[byteCount + 1];
            Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);
            return buffer;
        }

        private static RotatedRect RunFitEllipse(Point[] points, string parameterName, FitEllipseNativeMethod nativeMethod)
        {
            int[] pointsXy = ToInterleavedPoints(points, parameterName);
            if (points.Length < 5)
            {
                throw new ArgumentException("At least five points are required.", parameterName);
            }

            NativeException.ThrowIfError(nativeMethod(
                pointsXy,
                points.Length,
                out float centerX,
                out float centerY,
                out float width,
                out float height,
                out float angle));
            return ToRotatedRect(centerX, centerY, width, height, angle);
        }

#if NETCOREAPP3_1_OR_GREATER
        private static unsafe RotatedRect RunFitEllipse(
            ReadOnlySpan<Point> points,
            string parameterName,
            FitEllipseNativePtrMethod nativeMethod)
        {
            if (points.Length < 5)
            {
                throw new ArgumentException("At least five points are required.", parameterName);
            }

            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);
            fixed (int* pointsPtr = pointsXy)
            {
                NativeException.ThrowIfError(nativeMethod(
                    pointsPtr,
                    points.Length,
                    out float centerX,
                    out float centerY,
                    out float width,
                    out float height,
                    out float angle));
                return ToRotatedRect(centerX, centerY, width, height, angle);
            }
        }

        private static unsafe double RunPointSetScalar(
            ReadOnlySpan<Point> points,
            string parameterName,
            int flag,
            PointSetScalarNativeMethod nativeMethod)
        {
            PointSetMarshaller.ValidateNotEmpty(points, parameterName);
            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);

            fixed (int* pointsPtr = pointsXy)
            {
                NativeException.ThrowIfError(nativeMethod(pointsPtr, points.Length, flag, out double result));
                return result;
            }
        }

        private static unsafe Rect RunPointSetRect(
            ReadOnlySpan<Point> points,
            string parameterName,
            PointSetRectNativeMethod nativeMethod)
        {
            PointSetMarshaller.ValidateNotEmpty(points, parameterName);
            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);

            fixed (int* pointsPtr = pointsXy)
            {
                NativeException.ThrowIfError(nativeMethod(
                    pointsPtr,
                    points.Length,
                    out int x,
                    out int y,
                    out int width,
                    out int height));
                return new Rect(x, y, width, height);
            }
        }

        private static unsafe bool RunPointSetBoolean(
            ReadOnlySpan<Point> points,
            string parameterName,
            PointSetBooleanNativeMethod nativeMethod)
        {
            PointSetMarshaller.ValidateNotEmpty(points, parameterName);
            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);

            fixed (int* pointsPtr = pointsXy)
            {
                NativeException.ThrowIfError(nativeMethod(pointsPtr, points.Length, out int result));
                return result != 0;
            }
        }
#endif

        private static int[] ToInterleavedPoints(Point[] points, string parameterName)
        {
            if (points == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (points.Length == 0)
            {
                throw new ArgumentException("Point array cannot be empty.", parameterName);
            }

            var result = new int[points.Length * 2];
            for (int i = 0; i < points.Length; i++)
            {
                int offset = i * 2;
                result[offset] = points[i].X;
                result[offset + 1] = points[i].Y;
            }

            return result;
        }

#if NETCOREAPP3_1_OR_GREATER
        private static Point[] FromInterleavedPoints(ReadOnlySpan<int> pointsXy, int pointCount)
        {
            var result = new Point[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                int offset = i * 2;
                result[i] = new Point(pointsXy[offset], pointsXy[offset + 1]);
            }

            return result;
        }

        private static Point2f[] FromInterleavedPoint2f(ReadOnlySpan<float> pointsXy, int pointCount)
        {
            var result = new Point2f[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                int offset = i * 2;
                result[i] = new Point2f(pointsXy[offset], pointsXy[offset + 1]);
            }

            return result;
        }
#endif

        private static void ValidateNonEmpty<T>(T[] values, string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (values.Length == 0)
            {
                throw new ArgumentException("Array cannot be empty.", parameterName);
            }
        }

        private static RotatedRect ToRotatedRect(float centerX, float centerY, float width, float height, float angle)
        {
            return new RotatedRect(new Point2f(centerX, centerY), new Size2f(width, height), angle);
        }

        private static Point[] FromInterleavedPoints(int[] pointsXy, int pointCount)
        {
            var result = new Point[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                int offset = i * 2;
                result[i] = new Point(pointsXy[offset], pointsXy[offset + 1]);
            }

            return result;
        }

        private static unsafe Mat GetAffineTransformFromInterleaved(float[] srcXy, float[] dstXy)
        {
            fixed (float* srcPtr = srcXy)
            fixed (float* dstPtr = dstXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcGetAffineTransform(srcPtr, dstPtr, out IntPtr transform));
                return new Mat(transform);
            }
        }

        private static unsafe Mat GetPerspectiveTransformFromInterleaved(float[] srcXy, float[] dstXy, DecompTypes solveMethod)
        {
            fixed (float* srcPtr = srcXy)
            fixed (float* dstPtr = dstXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcGetPerspectiveTransform(
                    srcPtr,
                    dstPtr,
                    (int)solveMethod,
                    out IntPtr transform));
                return new Mat(transform);
            }
        }

        private static float[] ToInterleavedPoint2f(Point2f[] points, string parameterName, int requiredCount)
        {
            if (points == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (points.Length < requiredCount)
            {
                throw new ArgumentException("Point array does not contain enough elements.", parameterName);
            }

            var result = new float[requiredCount * 2];
            for (int i = 0; i < requiredCount; i++)
            {
                int offset = i * 2;
                result[offset] = points[i].X;
                result[offset + 1] = points[i].Y;
            }

            return result;
        }

        private static Vec4i[] FromInterleavedVec4i(int[] values, int valueCount)
        {
            var result = new Vec4i[valueCount];
            for (int i = 0; i < valueCount; i++)
            {
                int offset = i * 4;
                result[i] = new Vec4i(values[offset], values[offset + 1], values[offset + 2], values[offset + 3]);
            }

            return result;
        }

        private static Point2f[] FromInterleavedPoint2f(float[] pointsXy, int pointCount)
        {
            var result = new Point2f[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                int offset = i * 2;
                result[i] = new Point2f(pointsXy[offset], pointsXy[offset + 1]);
            }

            return result;
        }
    }
}
