using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Photo
{
    /// <summary>
    /// Entry points for OpenCV photo module functions.
    /// OpenCV photo 模块函数入口。
    /// </summary>
    public static unsafe class PhotoCv2
    {
        /// <summary>
        /// Converts a color image to grayscale while preserving contrast.
        /// 将彩色图像转换为灰度图，同时保持对比度。
        /// </summary>
        public static void Decolor(Mat src, Mat grayscale, Mat colorBoost)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(grayscale, nameof(grayscale));
            ValidateNotNull(colorBoost, nameof(colorBoost));
            NativeException.ThrowIfError(NativeMethods.PhotoDecolor(src.NativeHandle, grayscale.NativeHandle, colorBoost.NativeHandle));
        }

        /// <summary>
        /// Restores selected image regions using inpainting.
        /// 使用图像修复恢复选定区域。
        /// </summary>
        public static void Inpaint(Mat src, Mat inpaintMask, Mat dst, double inpaintRadius, InpaintMethod flags)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(inpaintMask, nameof(inpaintMask));
            ValidateNotNull(dst, nameof(dst));
            ValidateInpaintMethod(flags, nameof(flags));
            ValidateInpaintInputs(src, inpaintMask);
            NativeException.ThrowIfError(NativeMethods.PhotoInpaint(src.NativeHandle, inpaintMask.NativeHandle, dst.NativeHandle, inpaintRadius, (int)flags));
        }

        /// <summary>
        /// Restores selected image regions using inpainting and returns a new matrix.
        /// 使用图像修复恢复选定区域并返回新矩阵。
        /// </summary>
        public static Mat Inpaint(Mat src, Mat inpaintMask, double inpaintRadius, InpaintMethod flags)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(inpaintMask, nameof(inpaintMask));
            ValidateInpaintMethod(flags, nameof(flags));
            var dst = new Mat();
            try
            {
                Inpaint(src, inpaintMask, dst, inpaintRadius, flags);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Denoises an image using non-local means.
        /// 使用非局部均值算法对图像去噪。
        /// </summary>
        public static void FastNlMeansDenoising(Mat src, Mat dst, float h = 3.0F, int templateWindowSize = 7, int searchWindowSize = 21)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateFastNlMeansDenoisingInput(src, hLength: 1, normType: NormTypes.L2);
            NativeException.ThrowIfError(NativeMethods.PhotoFastNlMeansDenoising(src.NativeHandle, dst.NativeHandle, h, templateWindowSize, searchWindowSize));
        }

        /// <summary>
        /// Denoises an image using non-local means and returns a new matrix.
        /// 使用非局部均值算法对图像去噪并返回新矩阵。
        /// </summary>
        public static Mat FastNlMeansDenoising(Mat src, float h = 3.0F, int templateWindowSize = 7, int searchWindowSize = 21)
        {
            ValidateNotNull(src, nameof(src));
            var dst = new Mat();
            try
            {
                FastNlMeansDenoising(src, dst, h, templateWindowSize, searchWindowSize);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Denoises an image using per-channel non-local means strength.
        /// 使用按通道强度的非局部均值算法对图像去噪。
        /// </summary>
        public static void FastNlMeansDenoising(Mat src, Mat dst, float[] h, int templateWindowSize = 7, int searchWindowSize = 21, NormTypes normType = NormTypes.L2)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            if (h == null)
            {
                throw new ArgumentNullException(nameof(h));
            }

            ValidateNotEmpty(h, nameof(h));
            ValidateFastNlMeansDenoisingInput(src, h.Length, normType);
            NativeException.ThrowIfError(NativeMethods.PhotoFastNlMeansDenoisingWithHArray(src.NativeHandle, dst.NativeHandle, h, h.Length, templateWindowSize, searchWindowSize, (int)normType));
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Denoises an image using per-channel strength from a span.
        /// 使用 Span 中的按通道强度对图像去噪。
        /// </summary>
        public static void FastNlMeansDenoising(Mat src, Mat dst, ReadOnlySpan<float> h, int templateWindowSize = 7, int searchWindowSize = 21, NormTypes normType = NormTypes.L2)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateNotEmpty(h, nameof(h));
            ValidateFastNlMeansDenoisingInput(src, h.Length, normType);
            fixed (float* hPtr = h)
            {
                NativeException.ThrowIfError(NativeMethods.PhotoFastNlMeansDenoisingWithHArray(src.NativeHandle, dst.NativeHandle, hPtr, h.Length, templateWindowSize, searchWindowSize, (int)normType));
            }
        }

#endif

        /// <summary>
        /// Denoises a color image using non-local means.
        /// 使用非局部均值算法对彩色图像去噪。
        /// </summary>
        public static void FastNlMeansDenoisingColored(Mat src, Mat dst, float h = 3.0F, float hColor = 3.0F, int templateWindowSize = 7, int searchWindowSize = 21)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateFastNlMeansDenoisingColoredInput(src);
            NativeException.ThrowIfError(NativeMethods.PhotoFastNlMeansDenoisingColored(src.NativeHandle, dst.NativeHandle, h, hColor, templateWindowSize, searchWindowSize));
        }

        /// <summary>
        /// Denoises a color image using non-local means and returns a new matrix.
        /// 使用非局部均值算法对彩色图像去噪并返回新矩阵。
        /// </summary>
        public static Mat FastNlMeansDenoisingColored(Mat src, float h = 3.0F, float hColor = 3.0F, int templateWindowSize = 7, int searchWindowSize = 21)
        {
            ValidateNotNull(src, nameof(src));
            var dst = new Mat();
            try
            {
                FastNlMeansDenoisingColored(src, dst, h, hColor, templateWindowSize, searchWindowSize);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Denoises one image from a temporal sequence using non-local means.
        /// 使用非局部均值算法对时序图像序列中的一个图像去噪。
        /// </summary>
        /// <param name="srcImages">The input temporal image sequence. 输入时序图像序列。</param>
        /// <param name="dst">The output denoised image. 输出去噪图像。</param>
        /// <param name="imgToDenoiseIndex">The index of the image to denoise. 要去噪的图像索引。</param>
        /// <param name="temporalWindowSize">The number of temporal frames used for denoising. 去噪使用的时序窗口帧数。</param>
        /// <param name="h">The filter strength. 滤波强度。</param>
        /// <param name="templateWindowSize">The template patch size. 模板窗口尺寸。</param>
        /// <param name="searchWindowSize">The search window size. 搜索窗口尺寸。</param>
        public static void FastNlMeansDenoisingMulti(
            Mat[] srcImages,
            Mat dst,
            int imgToDenoiseIndex,
            int temporalWindowSize,
            float h = 3.0F,
            int templateWindowSize = 7,
            int searchWindowSize = 21)
        {
            ValidateNotNull(dst, nameof(dst));
            IntPtr[] handles = ToNativeHandles(srcImages, nameof(srcImages));
            ValidateFastNlMeansDenoisingMultiInputs(
                srcImages,
                imgToDenoiseIndex,
                temporalWindowSize,
                templateWindowSize,
                searchWindowSize);
            NativeException.ThrowIfError(NativeMethods.PhotoFastNlMeansDenoisingMulti(
                handles,
                handles.Length,
                dst.NativeHandle,
                imgToDenoiseIndex,
                temporalWindowSize,
                h,
                templateWindowSize,
                searchWindowSize));
        }

        /// <summary>
        /// Denoises one image from a temporal sequence using per-channel non-local means strength.
        /// 使用按通道强度的非局部均值算法对时序图像序列中的一个图像去噪。
        /// </summary>
        /// <param name="srcImages">The input temporal image sequence. 输入时序图像序列。</param>
        /// <param name="dst">The output denoised image. 输出去噪图像。</param>
        /// <param name="imgToDenoiseIndex">The index of the image to denoise. 要去噪的图像索引。</param>
        /// <param name="temporalWindowSize">The number of temporal frames used for denoising. 去噪使用的时序窗口帧数。</param>
        /// <param name="h">The per-channel filter strengths. 每通道滤波强度。</param>
        /// <param name="templateWindowSize">The template patch size. 模板窗口尺寸。</param>
        /// <param name="searchWindowSize">The search window size. 搜索窗口尺寸。</param>
        /// <param name="normType">The norm type. 范数类型。</param>
        public static void FastNlMeansDenoisingMulti(
            Mat[] srcImages,
            Mat dst,
            int imgToDenoiseIndex,
            int temporalWindowSize,
            float[] h,
            int templateWindowSize = 7,
            int searchWindowSize = 21,
            NormTypes normType = NormTypes.L2)
        {
            ValidateNotNull(dst, nameof(dst));
            if (h == null)
            {
                throw new ArgumentNullException(nameof(h));
            }

            ValidateNotEmpty(h, nameof(h));
            IntPtr[] handles = ToNativeHandles(srcImages, nameof(srcImages));
            ValidateFastNlMeansDenoisingMultiInputs(
                srcImages,
                imgToDenoiseIndex,
                temporalWindowSize,
                templateWindowSize,
                searchWindowSize);
            NativeException.ThrowIfError(NativeMethods.PhotoFastNlMeansDenoisingMultiWithHArray(
                handles,
                handles.Length,
                dst.NativeHandle,
                imgToDenoiseIndex,
                temporalWindowSize,
                h,
                h.Length,
                templateWindowSize,
                searchWindowSize,
                (int)normType));
        }

        /// <summary>
        /// Denoises one color image from a temporal sequence using non-local means.
        /// 使用非局部均值算法对时序图像序列中的一个彩色图像去噪。
        /// </summary>
        /// <param name="srcImages">The input temporal color image sequence. 输入时序彩色图像序列。</param>
        /// <param name="dst">The output denoised image. 输出去噪图像。</param>
        /// <param name="imgToDenoiseIndex">The index of the image to denoise. 要去噪的图像索引。</param>
        /// <param name="temporalWindowSize">The number of temporal frames used for denoising. 去噪使用的时序窗口帧数。</param>
        /// <param name="h">The luminance filter strength. 亮度滤波强度。</param>
        /// <param name="hColor">The color filter strength. 色彩滤波强度。</param>
        /// <param name="templateWindowSize">The template patch size. 模板窗口尺寸。</param>
        /// <param name="searchWindowSize">The search window size. 搜索窗口尺寸。</param>
        public static void FastNlMeansDenoisingColoredMulti(
            Mat[] srcImages,
            Mat dst,
            int imgToDenoiseIndex,
            int temporalWindowSize,
            float h = 3.0F,
            float hColor = 3.0F,
            int templateWindowSize = 7,
            int searchWindowSize = 21)
        {
            ValidateNotNull(dst, nameof(dst));
            IntPtr[] handles = ToNativeHandles(srcImages, nameof(srcImages));
            NativeException.ThrowIfError(NativeMethods.PhotoFastNlMeansDenoisingColoredMulti(
                handles,
                handles.Length,
                dst.NativeHandle,
                imgToDenoiseIndex,
                temporalWindowSize,
                h,
                hColor,
                templateWindowSize,
                searchWindowSize));
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Denoises one image from a temporal span using non-local means.
        /// 使用非局部均值算法对时序 Span 中的一个图像去噪。
        /// </summary>
        /// <param name="srcImages">The input temporal image sequence. 输入时序图像序列。</param>
        /// <param name="dst">The output denoised image. 输出去噪图像。</param>
        /// <param name="imgToDenoiseIndex">The index of the image to denoise. 要去噪的图像索引。</param>
        /// <param name="temporalWindowSize">The number of temporal frames used for denoising. 去噪使用的时序窗口帧数。</param>
        /// <param name="h">The filter strength. 滤波强度。</param>
        /// <param name="templateWindowSize">The template patch size. 模板窗口尺寸。</param>
        /// <param name="searchWindowSize">The search window size. 搜索窗口尺寸。</param>
        public static void FastNlMeansDenoisingMulti(
            ReadOnlySpan<Mat> srcImages,
            Mat dst,
            int imgToDenoiseIndex,
            int temporalWindowSize,
            float h = 3.0F,
            int templateWindowSize = 7,
            int searchWindowSize = 21)
        {
            ValidateNotNull(dst, nameof(dst));
            IntPtr[] handles = ToNativeHandles(srcImages, nameof(srcImages));
            ValidateFastNlMeansDenoisingMultiInputs(
                srcImages,
                imgToDenoiseIndex,
                temporalWindowSize,
                templateWindowSize,
                searchWindowSize);
            NativeException.ThrowIfError(NativeMethods.PhotoFastNlMeansDenoisingMulti(
                handles,
                handles.Length,
                dst.NativeHandle,
                imgToDenoiseIndex,
                temporalWindowSize,
                h,
                templateWindowSize,
                searchWindowSize));
        }

        /// <summary>
        /// Denoises one image from a temporal span using per-channel non-local means strength.
        /// 使用按通道强度的非局部均值算法对时序 Span 中的一个图像去噪。
        /// </summary>
        /// <param name="srcImages">The input temporal image sequence. 输入时序图像序列。</param>
        /// <param name="dst">The output denoised image. 输出去噪图像。</param>
        /// <param name="imgToDenoiseIndex">The index of the image to denoise. 要去噪的图像索引。</param>
        /// <param name="temporalWindowSize">The number of temporal frames used for denoising. 去噪使用的时序窗口帧数。</param>
        /// <param name="h">The per-channel filter strengths. 每通道滤波强度。</param>
        /// <param name="templateWindowSize">The template patch size. 模板窗口尺寸。</param>
        /// <param name="searchWindowSize">The search window size. 搜索窗口尺寸。</param>
        /// <param name="normType">The norm type. 范数类型。</param>
        public static void FastNlMeansDenoisingMulti(
            ReadOnlySpan<Mat> srcImages,
            Mat dst,
            int imgToDenoiseIndex,
            int temporalWindowSize,
            ReadOnlySpan<float> h,
            int templateWindowSize = 7,
            int searchWindowSize = 21,
            NormTypes normType = NormTypes.L2)
        {
            ValidateNotNull(dst, nameof(dst));
            IntPtr[] handles = ToNativeHandles(srcImages, nameof(srcImages));
            ValidateNotEmpty(h, nameof(h));
            ValidateFastNlMeansDenoisingMultiInputs(
                srcImages,
                imgToDenoiseIndex,
                temporalWindowSize,
                templateWindowSize,
                searchWindowSize);
            fixed (float* hPtr = h)
            {
                NativeException.ThrowIfError(NativeMethods.PhotoFastNlMeansDenoisingMultiWithHArray(
                    handles,
                    handles.Length,
                    dst.NativeHandle,
                    imgToDenoiseIndex,
                    temporalWindowSize,
                    hPtr,
                    h.Length,
                    templateWindowSize,
                    searchWindowSize,
                    (int)normType));
            }
        }

        /// <summary>
        /// Denoises one color image from a temporal span using non-local means.
        /// 使用非局部均值算法对时序 Span 中的一个彩色图像去噪。
        /// </summary>
        /// <param name="srcImages">The input temporal color image sequence. 输入时序彩色图像序列。</param>
        /// <param name="dst">The output denoised image. 输出去噪图像。</param>
        /// <param name="imgToDenoiseIndex">The index of the image to denoise. 要去噪的图像索引。</param>
        /// <param name="temporalWindowSize">The number of temporal frames used for denoising. 去噪使用的时序窗口帧数。</param>
        /// <param name="h">The luminance filter strength. 亮度滤波强度。</param>
        /// <param name="hColor">The color filter strength. 色彩滤波强度。</param>
        /// <param name="templateWindowSize">The template patch size. 模板窗口尺寸。</param>
        /// <param name="searchWindowSize">The search window size. 搜索窗口尺寸。</param>
        public static void FastNlMeansDenoisingColoredMulti(
            ReadOnlySpan<Mat> srcImages,
            Mat dst,
            int imgToDenoiseIndex,
            int temporalWindowSize,
            float h = 3.0F,
            float hColor = 3.0F,
            int templateWindowSize = 7,
            int searchWindowSize = 21)
        {
            ValidateNotNull(dst, nameof(dst));
            IntPtr[] handles = ToNativeHandles(srcImages, nameof(srcImages));
            NativeException.ThrowIfError(NativeMethods.PhotoFastNlMeansDenoisingColoredMulti(
                handles,
                handles.Length,
                dst.NativeHandle,
                imgToDenoiseIndex,
                temporalWindowSize,
                h,
                hColor,
                templateWindowSize,
                searchWindowSize));
        }
#endif

        /// <summary>
        /// Blends a source region into a destination image using seamless cloning.
        /// 使用 seamless cloning 将源区域融合到目标图像。
        /// </summary>
        public static void SeamlessClone(Mat src, Mat dst, Mat mask, Point center, Mat blend, SeamlessCloneFlags flags)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateNotNull(mask, nameof(mask));
            ValidateNotNull(blend, nameof(blend));
            ValidateSeamlessCloneFlags(flags, nameof(flags));
            ValidateSeamlessCloneInputs(src, dst);
            NativeException.ThrowIfError(NativeMethods.PhotoSeamlessClone(src.NativeHandle, dst.NativeHandle, mask.NativeHandle, center.X, center.Y, blend.NativeHandle, (int)flags));
        }

        /// <summary>
        /// Changes color locally under a mask.
        /// 在 mask 区域内局部调整颜色。
        /// </summary>
        public static void ColorChange(Mat src, Mat mask, Mat dst, float redMul = 1.0F, float greenMul = 1.0F, float blueMul = 1.0F)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(mask, nameof(mask));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.PhotoColorChange(src.NativeHandle, mask.NativeHandle, dst.NativeHandle, redMul, greenMul, blueMul));
        }

        /// <summary>
        /// Changes local illumination under a mask.
        /// 在 mask 区域内局部调整光照。
        /// </summary>
        public static void IlluminationChange(Mat src, Mat mask, Mat dst, float alpha = 0.2F, float beta = 0.4F)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(mask, nameof(mask));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.PhotoIlluminationChange(src.NativeHandle, mask.NativeHandle, dst.NativeHandle, alpha, beta));
        }

        /// <summary>
        /// Flattens texture in a selected region.
        /// 对选定区域进行纹理平坦化。
        /// </summary>
        public static void TextureFlattening(Mat src, Mat mask, Mat dst, float lowThreshold = 30.0F, float highThreshold = 45.0F, int kernelSize = 3)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(mask, nameof(mask));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.PhotoTextureFlattening(src.NativeHandle, mask.NativeHandle, dst.NativeHandle, lowThreshold, highThreshold, kernelSize));
        }

        /// <summary>
        /// Applies edge-preserving smoothing.
        /// 执行边缘保持平滑。
        /// </summary>
        public static void EdgePreservingFilter(Mat src, Mat dst, EdgePreservingFilterFlags flags = EdgePreservingFilterFlags.RecursiveFilter, float sigmaS = 60.0F, float sigmaR = 0.4F)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateEdgePreservingFilterFlags(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.PhotoEdgePreservingFilter(src.NativeHandle, dst.NativeHandle, (int)flags, sigmaS, sigmaR));
        }

        /// <summary>
        /// Applies edge-preserving smoothing and returns a new matrix.
        /// 执行边缘保持平滑并返回新矩阵。
        /// </summary>
        public static Mat EdgePreservingFilter(Mat src, EdgePreservingFilterFlags flags = EdgePreservingFilterFlags.RecursiveFilter, float sigmaS = 60.0F, float sigmaR = 0.4F)
        {
            ValidateNotNull(src, nameof(src));
            ValidateEdgePreservingFilterFlags(flags, nameof(flags));
            var dst = new Mat();
            try
            {
                EdgePreservingFilter(src, dst, flags, sigmaS, sigmaR);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Enhances image details.
        /// 增强图像细节。
        /// </summary>
        public static void DetailEnhance(Mat src, Mat dst, float sigmaS = 10.0F, float sigmaR = 0.15F)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.PhotoDetailEnhance(src.NativeHandle, dst.NativeHandle, sigmaS, sigmaR));
        }

        /// <summary>
        /// Enhances image details and returns a new matrix.
        /// 增强图像细节并返回新矩阵。
        /// </summary>
        public static Mat DetailEnhance(Mat src, float sigmaS = 10.0F, float sigmaR = 0.15F)
        {
            ValidateNotNull(src, nameof(src));
            var dst = new Mat();
            try
            {
                DetailEnhance(src, dst, sigmaS, sigmaR);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates pencil sketch outputs.
        /// 创建铅笔素描输出。
        /// </summary>
        public static void PencilSketch(Mat src, Mat dst1, Mat dst2, float sigmaS = 60.0F, float sigmaR = 0.07F, float shadeFactor = 0.02F)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst1, nameof(dst1));
            ValidateNotNull(dst2, nameof(dst2));
            NativeException.ThrowIfError(NativeMethods.PhotoPencilSketch(src.NativeHandle, dst1.NativeHandle, dst2.NativeHandle, sigmaS, sigmaR, shadeFactor));
        }

        /// <summary>
        /// Applies non-photorealistic stylization.
        /// 执行非真实感风格化。
        /// </summary>
        public static void Stylization(Mat src, Mat dst, float sigmaS = 60.0F, float sigmaR = 0.45F)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.PhotoStylization(src.NativeHandle, dst.NativeHandle, sigmaS, sigmaR));
        }

        /// <summary>
        /// Applies non-photorealistic stylization and returns a new matrix.
        /// 执行非真实感风格化并返回新矩阵。
        /// </summary>
        public static Mat Stylization(Mat src, float sigmaS = 60.0F, float sigmaR = 0.45F)
        {
            ValidateNotNull(src, nameof(src));
            var dst = new Mat();
            try
            {
                Stylization(src, dst, sigmaS, sigmaR);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a simple linear tonemap operator.
        /// 创建简单线性 tone mapping 算子。
        /// </summary>
        public static Tonemap CreateTonemap(float gamma = 1.0F)
        {
            return Tonemap.Create(gamma);
        }

        /// <summary>
        /// Creates a Drago tonemap operator.
        /// 创建 Drago tone mapping 算子。
        /// </summary>
        public static TonemapDrago CreateTonemapDrago(float gamma = 1.0F, float saturation = 1.0F, float bias = 0.85F)
        {
            return TonemapDrago.Create(gamma, saturation, bias);
        }

        /// <summary>
        /// Creates a Reinhard tonemap operator.
        /// 创建 Reinhard tone mapping 算子。
        /// </summary>
        public static TonemapReinhard CreateTonemapReinhard(float gamma = 1.0F, float intensity = 0.0F, float lightAdapt = 1.0F, float colorAdapt = 0.0F)
        {
            return TonemapReinhard.Create(gamma, intensity, lightAdapt, colorAdapt);
        }

        /// <summary>
        /// Creates a Mantiuk tonemap operator.
        /// 创建 Mantiuk tone mapping 算子。
        /// </summary>
        public static TonemapMantiuk CreateTonemapMantiuk(float gamma = 1.0F, float scale = 0.7F, float saturation = 1.0F)
        {
            return TonemapMantiuk.Create(gamma, scale, saturation);
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidateInpaintMethod(InpaintMethod value, string parameterName)
        {
            if (value != InpaintMethod.Ns && value != InpaintMethod.Telea)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Inpaint method must be Ns or Telea.");
            }
        }

        private static void ValidateInpaintInputs(Mat src, Mat inpaintMask)
        {
            int sourceType = src.Type;
            if (sourceType != MatType.CV_8UC1 &&
                sourceType != MatType.CV_16UC1 &&
                sourceType != MatType.CV_32FC1 &&
                sourceType != MatType.CV_8UC3)
            {
                throw new ArgumentException("Inpaint source image must be CV_8UC1, CV_16UC1, CV_32FC1, or CV_8UC3.", nameof(src));
            }

            if (inpaintMask.Type != MatType.CV_8UC1)
            {
                throw new ArgumentException("Inpaint mask must be CV_8UC1.", nameof(inpaintMask));
            }

            if (inpaintMask.Rows != src.Rows || inpaintMask.Cols != src.Cols)
            {
                throw new ArgumentException("Inpaint mask must have the same size as the source image.", nameof(inpaintMask));
            }
        }

        private static void ValidateFastNlMeansDenoisingInput(Mat src, int hLength, NormTypes normType)
        {
            if (src.Empty)
            {
                throw new ArgumentException("FastNlMeansDenoising source image must not be empty.", nameof(src));
            }

            if (hLength != 1 && hLength != src.Channels)
            {
                throw new ArgumentException("FastNlMeansDenoising h collection length must be one or match the source channel count.", "h");
            }

            if (normType == NormTypes.L2)
            {
                if (src.Depth != MatType.CV_8U)
                {
                    throw new ArgumentException("FastNlMeansDenoising with L2 norm supports only CV_8U source depth.", nameof(src));
                }

                return;
            }

            if (normType == NormTypes.L1)
            {
                if (src.Depth != MatType.CV_8U && src.Depth != MatType.CV_16U)
                {
                    throw new ArgumentException("FastNlMeansDenoising with L1 norm supports only CV_8U or CV_16U source depth.", nameof(src));
                }

                return;
            }

            throw new ArgumentOutOfRangeException(nameof(normType), "FastNlMeansDenoising norm type must be L1 or L2.");
        }

        private static void ValidateFastNlMeansDenoisingColoredInput(Mat src)
        {
            int sourceType = src.Type;
            if (sourceType != MatType.CV_8UC3 && sourceType != MatType.CV_8UC4)
            {
                throw new ArgumentException("FastNlMeansDenoisingColored source image must be CV_8UC3 or CV_8UC4.", nameof(src));
            }
        }

        private static void ValidateEdgePreservingFilterFlags(EdgePreservingFilterFlags value, string parameterName)
        {
            if (value != EdgePreservingFilterFlags.RecursiveFilter && value != EdgePreservingFilterFlags.NormalizedConvolutionFilter)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Edge-preserving filter flags must be RecursiveFilter or NormalizedConvolutionFilter.");
            }
        }

        private static void ValidateSeamlessCloneFlags(SeamlessCloneFlags value, string parameterName)
        {
            if (value != SeamlessCloneFlags.NormalClone &&
                value != SeamlessCloneFlags.MixedClone &&
                value != SeamlessCloneFlags.MonochromeTransfer &&
                value != SeamlessCloneFlags.NormalCloneWide &&
                value != SeamlessCloneFlags.MixedCloneWide &&
                value != SeamlessCloneFlags.MonochromeTransferWide)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Seamless clone flags must be a defined SeamlessCloneFlags value.");
            }
        }

        private static void ValidateSeamlessCloneInputs(Mat src, Mat dst)
        {
            if (src.Empty)
            {
                throw new ArgumentException("SeamlessClone source image must not be empty.", nameof(src));
            }

            if (dst.Empty)
            {
                throw new ArgumentException("SeamlessClone destination image must not be empty.", nameof(dst));
            }
        }

        private static void ValidateFastNlMeansDenoisingMultiInputs(
            Mat[] srcImages,
            int imgToDenoiseIndex,
            int temporalWindowSize,
            int templateWindowSize,
            int searchWindowSize)
        {
            ValidateFastNlMeansDenoisingMultiWindowSizes(temporalWindowSize, templateWindowSize, searchWindowSize);
            ValidateFastNlMeansDenoisingMultiFrameWindow(srcImages.Length, imgToDenoiseIndex, temporalWindowSize);
            ValidateFastNlMeansDenoisingMultiFrames(srcImages, nameof(srcImages));
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void ValidateFastNlMeansDenoisingMultiInputs(
            ReadOnlySpan<Mat> srcImages,
            int imgToDenoiseIndex,
            int temporalWindowSize,
            int templateWindowSize,
            int searchWindowSize)
        {
            ValidateFastNlMeansDenoisingMultiWindowSizes(temporalWindowSize, templateWindowSize, searchWindowSize);
            ValidateFastNlMeansDenoisingMultiFrameWindow(srcImages.Length, imgToDenoiseIndex, temporalWindowSize);
            ValidateFastNlMeansDenoisingMultiFrames(srcImages, nameof(srcImages));
        }
#endif

        private static void ValidateFastNlMeansDenoisingMultiWindowSizes(
            int temporalWindowSize,
            int templateWindowSize,
            int searchWindowSize)
        {
            if (temporalWindowSize % 2 == 0)
            {
                throw new ArgumentException("Temporal window size must be odd.", nameof(temporalWindowSize));
            }

            if (templateWindowSize % 2 == 0)
            {
                throw new ArgumentException("Template window size must be odd.", nameof(templateWindowSize));
            }

            if (searchWindowSize % 2 == 0)
            {
                throw new ArgumentException("Search window size must be odd.", nameof(searchWindowSize));
            }
        }

        private static void ValidateFastNlMeansDenoisingMultiFrameWindow(
            int imageCount,
            int imgToDenoiseIndex,
            int temporalWindowSize)
        {
            int halfWindow = temporalWindowSize / 2;
            if (imgToDenoiseIndex - halfWindow < 0 ||
                imgToDenoiseIndex + halfWindow >= imageCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(imgToDenoiseIndex),
                    "Image index and temporal window size must fit within the source image sequence.");
            }
        }

        private static void ValidateFastNlMeansDenoisingMultiFrames(Mat[] srcImages, string parameterName)
        {
            Mat first = srcImages[0];
            for (int i = 1; i < srcImages.Length; i++)
            {
                ValidateFastNlMeansDenoisingMultiFrame(first, srcImages[i], parameterName);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void ValidateFastNlMeansDenoisingMultiFrames(ReadOnlySpan<Mat> srcImages, string parameterName)
        {
            Mat first = srcImages[0];
            for (int i = 1; i < srcImages.Length; i++)
            {
                ValidateFastNlMeansDenoisingMultiFrame(first, srcImages[i], parameterName);
            }
        }
#endif

        private static void ValidateFastNlMeansDenoisingMultiFrame(Mat first, Mat current, string parameterName)
        {
            if (current.Rows != first.Rows ||
                current.Cols != first.Cols ||
                current.Type != first.Type)
            {
                throw new ArgumentException("Input images must have the same size and type.", parameterName);
            }
        }

        private static void ValidateNotEmpty(float[] values, string parameterName)
        {
            if (values.Length == 0)
            {
                throw new ArgumentException("Float collection cannot be empty.", parameterName);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void ValidateNotEmpty(ReadOnlySpan<float> values, string parameterName)
        {
            if (values.IsEmpty)
            {
                throw new ArgumentException("Float collection cannot be empty.", parameterName);
            }
        }
#endif

        private static IntPtr[] ToNativeHandles(Mat[] mats, string parameterName)
        {
            if (mats == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (mats.Length == 0)
            {
                throw new ArgumentException("Mat collection cannot be empty.", parameterName);
            }

            var handles = new IntPtr[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                Mat mat = mats[i];
                if (mat == null)
                {
                    throw new ArgumentException("Mat collection cannot contain null elements.", parameterName);
                }

                handles[i] = mat.NativeHandle;
            }

            return handles;
        }

#if NETCOREAPP3_1_OR_GREATER
        private static IntPtr[] ToNativeHandles(ReadOnlySpan<Mat> mats, string parameterName)
        {
            if (mats.IsEmpty)
            {
                throw new ArgumentException("Mat collection cannot be empty.", parameterName);
            }

            var handles = new IntPtr[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                Mat mat = mats[i];
                if (mat == null)
                {
                    throw new ArgumentException("Mat collection cannot contain null elements.", parameterName);
                }

                handles[i] = mat.NativeHandle;
            }

            return handles;
        }
#endif
    }
}
