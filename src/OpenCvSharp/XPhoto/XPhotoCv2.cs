using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.XPhoto
{
    /// <summary>
    /// Entry points for OpenCV xphoto functions.
    /// OpenCV xphoto 函数入口。
    /// </summary>
    public static class XPhotoCv2
    {
        /// <summary>Creates a simple white balancer. 创建简单白平衡器。</summary>
        public static SimpleWB CreateSimpleWB()
        {
            return SimpleWB.Create();
        }

        /// <summary>Creates a gray-world white balancer. 创建灰度世界白平衡器。</summary>
        public static GrayworldWB CreateGrayworldWB()
        {
            return GrayworldWB.Create();
        }

        /// <summary>Creates a learning-based white balancer. 创建基于学习的白平衡器。</summary>
        public static LearningBasedWB CreateLearningBasedWB(string? modelPath = null)
        {
            return LearningBasedWB.Create(modelPath);
        }

        /// <summary>
        /// Applies independent B, G, and R channel gains.
        /// 应用独立的 B、G、R 通道增益。
        /// </summary>
        public static void ApplyChannelGains(Mat src, Mat dst, float gainB, float gainG, float gainR)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateApplyChannelGainsSource(src);
            NativeException.ThrowIfError(NativeMethods.XPhotoApplyChannelGains(src.NativeHandle, dst.NativeHandle, gainB, gainG, gainR));
        }

        /// <summary>
        /// Applies channel gains and returns a new matrix.
        /// 应用通道增益并返回新矩阵。
        /// </summary>
        public static Mat ApplyChannelGains(Mat src, float gainB, float gainG, float gainR)
        {
            ValidateNotNull(src, nameof(src));
            ValidateApplyChannelGainsSource(src);
            var dst = new Mat();
            try
            {
                ApplyChannelGains(src, dst, gainB, gainG, gainR);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Denoises an image using DCT denoising.
        /// 使用 DCT 去噪处理图像。
        /// </summary>
        public static void DctDenoising(Mat src, Mat dst, double sigma, int psize = 16)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateDctDenoisingSource(src);
            NativeException.ThrowIfError(NativeMethods.XPhotoDctDenoising(src.NativeHandle, dst.NativeHandle, sigma, psize));
        }

        /// <summary>
        /// Denoises an image using DCT denoising and returns a new matrix.
        /// 使用 DCT 去噪处理图像并返回新矩阵。
        /// </summary>
        public static Mat DctDenoising(Mat src, double sigma, int psize = 16)
        {
            ValidateNotNull(src, nameof(src));
            ValidateDctDenoisingSource(src);
            var dst = new Mat();
            try
            {
                DctDenoising(src, dst, sigma, psize);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Denoises an image using BM3D single-output mode.
        /// 使用 BM3D 单输出模式对图像去噪。
        /// </summary>
        public static void Bm3dDenoising(
            Mat src,
            Mat dst,
            float h = 1.0F,
            int templateWindowSize = 4,
            int searchWindowSize = 16,
            int blockMatchingStep1 = 2500,
            int blockMatchingStep2 = 400,
            int groupSize = 8,
            int slidingStep = 1,
            float beta = 2.0F,
            NormTypes normType = NormTypes.L2,
            Bm3dSteps step = Bm3dSteps.StepAll,
            TransformTypes transformType = TransformTypes.Haar)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateTransformType(transformType, nameof(transformType));
            ValidateBm3dStep(step, nameof(step), allowStep2: false);
            ValidateBm3dNormType(normType, nameof(normType));
            ValidateBm3dDenoisingInputs(src, templateWindowSize, searchWindowSize, slidingStep);
            NativeException.ThrowIfError(NativeMethods.XPhotoBm3dDenoising(
                src.NativeHandle,
                dst.NativeHandle,
                h,
                templateWindowSize,
                searchWindowSize,
                blockMatchingStep1,
                blockMatchingStep2,
                groupSize,
                slidingStep,
                beta,
                (int)normType,
                (int)step,
                (int)transformType));
        }

        /// <summary>
        /// Denoises an image using BM3D and returns a new matrix.
        /// 使用 BM3D 对图像去噪并返回新矩阵。
        /// </summary>
        public static Mat Bm3dDenoising(
            Mat src,
            float h = 1.0F,
            int templateWindowSize = 4,
            int searchWindowSize = 16,
            int blockMatchingStep1 = 2500,
            int blockMatchingStep2 = 400,
            int groupSize = 8,
            int slidingStep = 1,
            float beta = 2.0F,
            NormTypes normType = NormTypes.L2,
            Bm3dSteps step = Bm3dSteps.StepAll,
            TransformTypes transformType = TransformTypes.Haar)
        {
            ValidateNotNull(src, nameof(src));
            ValidateTransformType(transformType, nameof(transformType));
            ValidateBm3dStep(step, nameof(step), allowStep2: false);
            ValidateBm3dNormType(normType, nameof(normType));
            ValidateBm3dDenoisingInputs(src, templateWindowSize, searchWindowSize, slidingStep);
            var dst = new Mat();
            try
            {
                Bm3dDenoising(src, dst, h, templateWindowSize, searchWindowSize, blockMatchingStep1, blockMatchingStep2, groupSize, slidingStep, beta, normType, step, transformType);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Denoises an image using BM3D two-output mode.
        /// 使用 BM3D 双输出模式对图像去噪。
        /// </summary>
        public static void Bm3dDenoising(
            Mat src,
            Mat dstStep1,
            Mat dstStep2,
            float h = 1.0F,
            int templateWindowSize = 4,
            int searchWindowSize = 16,
            int blockMatchingStep1 = 2500,
            int blockMatchingStep2 = 400,
            int groupSize = 8,
            int slidingStep = 1,
            float beta = 2.0F,
            NormTypes normType = NormTypes.L2,
            Bm3dSteps step = Bm3dSteps.StepAll,
            TransformTypes transformType = TransformTypes.Haar)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dstStep1, nameof(dstStep1));
            ValidateNotNull(dstStep2, nameof(dstStep2));
            ValidateTransformType(transformType, nameof(transformType));
            ValidateBm3dStep(step, nameof(step), allowStep2: true);
            ValidateBm3dNormType(normType, nameof(normType));
            ValidateBm3dDenoisingInputs(src, templateWindowSize, searchWindowSize, slidingStep);
            NativeException.ThrowIfError(NativeMethods.XPhotoBm3dDenoisingSteps(
                src.NativeHandle,
                dstStep1.NativeHandle,
                dstStep2.NativeHandle,
                h,
                templateWindowSize,
                searchWindowSize,
                blockMatchingStep1,
                blockMatchingStep2,
                groupSize,
                slidingStep,
                beta,
                (int)normType,
                (int)step,
                (int)transformType));
        }

        /// <summary>
        /// Applies the oil painting filter.
        /// 应用油画滤镜。
        /// </summary>
        public static void OilPainting(Mat src, Mat dst, int size, int dynRatio)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateOilPaintingInputs(src, size, dynRatio);
            NativeException.ThrowIfError(NativeMethods.XPhotoOilPainting(src.NativeHandle, dst.NativeHandle, size, dynRatio, 0, 0));
        }

        /// <summary>
        /// Applies the oil painting filter after color conversion.
        /// 颜色转换后应用油画滤镜。
        /// </summary>
        public static void OilPainting(Mat src, Mat dst, int size, int dynRatio, ColorConversionCodes code)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateOilPaintingInputs(src, size, dynRatio);
            NativeException.ThrowIfError(NativeMethods.XPhotoOilPainting(src.NativeHandle, dst.NativeHandle, size, dynRatio, (int)code, 1));
        }

        /// <summary>
        /// Applies the oil painting filter and returns a new matrix.
        /// 应用油画滤镜并返回新矩阵。
        /// </summary>
        public static Mat OilPainting(Mat src, int size, int dynRatio)
        {
            ValidateNotNull(src, nameof(src));
            ValidateOilPaintingInputs(src, size, dynRatio);
            var dst = new Mat();
            try
            {
                OilPainting(src, dst, size, dynRatio);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidateTransformType(TransformTypes value, string parameterName)
        {
            if (value != TransformTypes.Haar)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Transform type must be Haar.");
            }
        }

        private static void ValidateBm3dStep(Bm3dSteps value, string parameterName, bool allowStep2)
        {
            if (value != Bm3dSteps.StepAll && value != Bm3dSteps.Step1 && value != Bm3dSteps.Step2)
            {
                throw new ArgumentOutOfRangeException(parameterName, "BM3D step must be StepAll, Step1, or Step2.");
            }

            if (!allowStep2 && value == Bm3dSteps.Step2)
            {
                throw new ArgumentOutOfRangeException(parameterName, "BM3D Step2 requires a basic image output; use the two-output overload.");
            }
        }

        private static void ValidateBm3dNormType(NormTypes value, string parameterName)
        {
            if (value != NormTypes.L1 && value != NormTypes.L2)
            {
                throw new ArgumentOutOfRangeException(parameterName, "BM3D norm type must be L1 or L2.");
            }
        }

        private static void ValidateApplyChannelGainsSource(Mat src)
        {
            if (src.Empty)
            {
                throw new ArgumentException("ApplyChannelGains source image must not be empty.", nameof(src));
            }

            if (!src.IsContinuous)
            {
                throw new ArgumentException("ApplyChannelGains source image must be continuous.", nameof(src));
            }

            if (src.Type != MatType.CV_8UC3 && src.Type != MatType.CV_16UC3)
            {
                throw new ArgumentException("ApplyChannelGains source image must be CV_8UC3 or CV_16UC3.", nameof(src));
            }
        }

        private static void ValidateBm3dDenoisingInputs(Mat src, int templateWindowSize, int searchWindowSize, int slidingStep)
        {
            if (src.Channels != 1)
            {
                throw new ArgumentException("BM3D denoising requires a single-channel source image.", nameof(src));
            }

            if (searchWindowSize <= templateWindowSize)
            {
                throw new ArgumentOutOfRangeException(nameof(searchWindowSize), "Search window size must be greater than template window size.");
            }

            if (slidingStep <= 0 || slidingStep >= templateWindowSize)
            {
                throw new ArgumentOutOfRangeException(nameof(slidingStep), "Sliding step must be greater than zero and less than template window size.");
            }
        }

        private static void ValidateDctDenoisingSource(Mat src)
        {
            if (src.Channels != 1 && src.Channels != 3)
            {
                throw new ArgumentException("DCT denoising requires a single-channel or three-channel source image.", nameof(src));
            }
        }

        private static void ValidateOilPaintingInputs(Mat src, int size, int dynRatio)
        {
            if (src.Type != MatType.CV_8UC1 && src.Type != MatType.CV_8UC3)
            {
                throw new ArgumentException("Oil painting requires a CV_8UC1 or CV_8UC3 source image.", nameof(src));
            }

            if (size < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than or equal to one.");
            }

            if (dynRatio <= 0 || dynRatio >= 128)
            {
                throw new ArgumentOutOfRangeException(nameof(dynRatio), "Dynamic ratio must be greater than zero and less than 128.");
            }
        }
    }
}
