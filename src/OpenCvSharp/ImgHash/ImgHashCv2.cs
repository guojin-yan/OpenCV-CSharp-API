using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgHash
{
    /// <summary>
    /// Provides one-shot image hash functions.
    /// 提供一次性图像哈希函数。
    /// </summary>
    public static class ImgHashCv2
    {
        /// <summary>Computes average hash. 计算 average hash。</summary>
        public static void AverageHash(Mat input, Mat output)
        {
            Run(input, output, NativeMethods.ImgHashAverageComputeStatic);
        }

        /// <summary>Computes average hash. 计算 average hash。</summary>
        public static Mat AverageHash(Mat input)
        {
            return Run(input, NativeMethods.ImgHashAverageComputeStatic);
        }

        /// <summary>Computes perceptual hash. 计算 perceptual hash。</summary>
        public static void PHash(Mat input, Mat output)
        {
            Run(input, output, NativeMethods.ImgHashPHashComputeStatic);
        }

        /// <summary>Computes perceptual hash. 计算 perceptual hash。</summary>
        public static Mat PHash(Mat input)
        {
            return Run(input, NativeMethods.ImgHashPHashComputeStatic);
        }

        /// <summary>Computes block mean hash. 计算 block mean hash。</summary>
        public static void BlockMeanHash(Mat input, Mat output, BlockMeanHashMode mode = BlockMeanHashMode.Mode0)
        {
            global::JYPPX.OpenCvSharp.ImgHash.BlockMeanHash.ValidateMode(mode, nameof(mode));
            Validate(input, nameof(input));
            Validate(output, nameof(output));
            ImgHashBase.ValidateInputImage(input, nameof(input));
            NativeException.ThrowIfError(NativeMethods.ImgHashBlockMeanComputeStatic(input.NativeHandle, output.NativeHandle, (int)mode));
        }

        /// <summary>Computes block mean hash. 计算 block mean hash。</summary>
        public static Mat BlockMeanHash(Mat input, BlockMeanHashMode mode = BlockMeanHashMode.Mode0)
        {
            var output = new Mat();
            try
            {
                BlockMeanHash(input, output, mode);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>Computes color moment hash. 计算 color moment hash。</summary>
        public static void ColorMomentHash(Mat input, Mat output)
        {
            Run(input, output, NativeMethods.ImgHashColorMomentComputeStatic);
        }

        /// <summary>Computes color moment hash. 计算 color moment hash。</summary>
        public static Mat ColorMomentHash(Mat input)
        {
            return Run(input, NativeMethods.ImgHashColorMomentComputeStatic);
        }

        /// <summary>Computes Marr-Hildreth hash. 计算 Marr-Hildreth hash。</summary>
        public static void MarrHildrethHash(Mat input, Mat output, float alpha = 2.0F, float scale = 1.0F)
        {
            Validate(input, nameof(input));
            Validate(output, nameof(output));
            ImgHashBase.ValidateInputImage(input, nameof(input));
            NativeException.ThrowIfError(NativeMethods.ImgHashMarrHildrethComputeStatic(input.NativeHandle, output.NativeHandle, alpha, scale));
        }

        /// <summary>Computes Marr-Hildreth hash. 计算 Marr-Hildreth hash。</summary>
        public static Mat MarrHildrethHash(Mat input, float alpha = 2.0F, float scale = 1.0F)
        {
            var output = new Mat();
            try
            {
                MarrHildrethHash(input, output, alpha, scale);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>Computes radial variance hash. 计算 radial variance hash。</summary>
        public static void RadialVarianceHash(Mat input, Mat output, double sigma = 1.0, int numOfAngleLine = 180)
        {
            Validate(input, nameof(input));
            Validate(output, nameof(output));
            ImgHashBase.ValidateInputImage(input, nameof(input));
            NativeException.ThrowIfError(NativeMethods.ImgHashRadialVarianceComputeStatic(input.NativeHandle, output.NativeHandle, sigma, numOfAngleLine));
        }

        /// <summary>Computes radial variance hash. 计算 radial variance hash。</summary>
        public static Mat RadialVarianceHash(Mat input, double sigma = 1.0, int numOfAngleLine = 180)
        {
            var output = new Mat();
            try
            {
                RadialVarianceHash(input, output, sigma, numOfAngleLine);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        private delegate int NativeHash(IntPtr input, IntPtr output);

        private static Mat Run(Mat input, NativeHash native)
        {
            var output = new Mat();
            try
            {
                Run(input, output, native);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        private static void Run(Mat input, Mat output, NativeHash native)
        {
            Validate(input, nameof(input));
            Validate(output, nameof(output));
            ImgHashBase.ValidateInputImage(input, nameof(input));
            NativeException.ThrowIfError(native(input.NativeHandle, output.NativeHandle));
        }

        private static void Validate<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
