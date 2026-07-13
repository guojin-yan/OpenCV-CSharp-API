using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Fuzzy
{
    /// <summary>
    /// Entry points for OpenCV fuzzy image-processing functions.
    /// OpenCV fuzzy 图像处理函数入口。
    /// </summary>
    public static class FuzzyCv2
    {
        /// <summary>Creates a fuzzy kernel from two function matrices. 从两个函数矩阵创建 fuzzy kernel。</summary>
        public static void CreateKernel(Mat functionX, Mat functionY, Mat kernel, int channels)
        {
            ValidateNotNull(functionX, nameof(functionX));
            ValidateNotNull(functionY, nameof(functionY));
            ValidateNotNull(kernel, nameof(kernel));
            ValidatePositive(channels, nameof(channels));
            NativeException.ThrowIfError(NativeMethods.FuzzyCreateKernelFromFunctions(functionX.NativeHandle, functionY.NativeHandle, kernel.NativeHandle, channels));
        }

        /// <summary>Creates a fuzzy kernel from two function matrices and returns it. 从两个函数矩阵创建并返回 fuzzy kernel。</summary>
        public static Mat CreateKernel(Mat functionX, Mat functionY, int channels)
        {
            var kernel = new Mat();
            try
            {
                CreateKernel(functionX, functionY, kernel, channels);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>Creates a fuzzy kernel from a predefined function. 从预定义函数创建 fuzzy kernel。</summary>
        public static void CreateKernel(FuzzyFunctionType functionType, int radius, Mat kernel, int channels)
        {
            ValidateEnum(functionType, nameof(functionType));
            ValidatePositive(radius, nameof(radius));
            ValidateNotNull(kernel, nameof(kernel));
            ValidatePositive(channels, nameof(channels));
            NativeException.ThrowIfError(NativeMethods.FuzzyCreateKernel((int)functionType, radius, kernel.NativeHandle, channels));
        }

        /// <summary>Creates and returns a fuzzy kernel from a predefined function. 从预定义函数创建并返回 fuzzy kernel。</summary>
        public static Mat CreateKernel(FuzzyFunctionType functionType, int radius, int channels)
        {
            var kernel = new Mat();
            try
            {
                CreateKernel(functionType, radius, kernel, channels);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>Inpaints an image using fuzzy mathematics. 使用 fuzzy 数学进行图像修复。</summary>
        public static void Inpaint(Mat image, Mat mask, Mat output, int radius, FuzzyFunctionType functionType, FuzzyInpaintAlgorithm algorithm)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(mask, nameof(mask));
            ValidateNotNull(output, nameof(output));
            ValidatePositive(radius, nameof(radius));
            ValidateEnum(functionType, nameof(functionType));
            ValidateEnum(algorithm, nameof(algorithm));
            ValidateInpaintInputs(mask);
            NativeException.ThrowIfError(NativeMethods.FuzzyInpaint(image.NativeHandle, mask.NativeHandle, output.NativeHandle, radius, (int)functionType, (int)algorithm));
        }

        /// <summary>Inpaints an image using fuzzy mathematics and returns the output. 使用 fuzzy 数学进行图像修复并返回输出。</summary>
        public static Mat Inpaint(Mat image, Mat mask, int radius, FuzzyFunctionType functionType, FuzzyInpaintAlgorithm algorithm)
        {
            var output = new Mat();
            try
            {
                Inpaint(image, mask, output, radius, functionType, algorithm);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>Filters an image by means of F-transform. 使用 F-transform 过滤图像。</summary>
        public static void Filter(Mat image, Mat kernel, Mat output)
        {
            ValidateMatrixKernelOutput(image, kernel, output);
            ValidateFilterInputs(image, kernel);
            NativeException.ThrowIfError(NativeMethods.FuzzyFilter(image.NativeHandle, kernel.NativeHandle, output.NativeHandle));
        }

        /// <summary>Filters an image by means of F-transform and returns the output. 使用 F-transform 过滤图像并返回输出。</summary>
        public static Mat Filter(Mat image, Mat kernel)
        {
            var output = new Mat();
            try
            {
                Filter(image, kernel, output);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>Computes F0-transform components. 计算 F0-transform components。</summary>
        public static void FT02DComponents(Mat matrix, Mat kernel, Mat components, Mat? mask = null)
        {
            ValidateMatrixKernelOutput(matrix, kernel, components);
            ValidateFT02DComponentsInputs(matrix, kernel, mask);
            NativeException.ThrowIfError(NativeMethods.FuzzyFT02DComponents(matrix.NativeHandle, kernel.NativeHandle, components.NativeHandle, OptionalHandle(mask)));
        }

        /// <summary>Computes and returns F0-transform components. 计算并返回 F0-transform components。</summary>
        public static Mat FT02DComponents(Mat matrix, Mat kernel, Mat? mask = null)
        {
            var components = new Mat();
            try
            {
                FT02DComponents(matrix, kernel, components, mask);
                return components;
            }
            catch
            {
                components.Dispose();
                throw;
            }
        }

        /// <summary>Computes inverse F0-transform. 计算 inverse F0-transform。</summary>
        public static void FT02DInverseFT(Mat components, Mat kernel, Mat output, int width, int height)
        {
            ValidateMatrixKernelOutput(components, kernel, output);
            ValidateFT02DInverseInputs(components, kernel);
            ValidatePositive(width, nameof(width));
            ValidatePositive(height, nameof(height));
            NativeException.ThrowIfError(NativeMethods.FuzzyFT02DInverseFT(components.NativeHandle, kernel.NativeHandle, output.NativeHandle, width, height));
        }

        /// <summary>Computes inverse F0-transform and returns the output. 计算 inverse F0-transform 并返回输出。</summary>
        public static Mat FT02DInverseFT(Mat components, Mat kernel, int width, int height)
        {
            var output = new Mat();
            try
            {
                FT02DInverseFT(components, kernel, output, width, height);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>Computes F0-transform and inverse F0-transform in one step. 一步计算 F0-transform 与 inverse F0-transform。</summary>
        public static void FT02DProcess(Mat matrix, Mat kernel, Mat output, Mat? mask = null)
        {
            ValidateMatrixKernelOutput(matrix, kernel, output);
            ValidateFTransformProcessInputs("FT02D process", matrix, kernel, mask);
            NativeException.ThrowIfError(NativeMethods.FuzzyFT02DProcess(matrix.NativeHandle, kernel.NativeHandle, output.NativeHandle, OptionalHandle(mask)));
        }

        /// <summary>Computes F0-transform and inverse F0-transform in one step and returns the output. 一步计算 F0-transform 与 inverse F0-transform 并返回输出。</summary>
        public static Mat FT02DProcess(Mat matrix, Mat kernel, Mat? mask = null)
        {
            var output = new Mat();
            try
            {
                FT02DProcess(matrix, kernel, output, mask);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>Computes one F0-transform iteration and returns the state. 计算一次 F0-transform 迭代并返回状态。</summary>
        public static int FT02DIteration(Mat matrix, Mat kernel, Mat output, Mat mask, Mat? maskOutput = null, bool firstStop = true)
        {
            ValidateMatrixKernelOutput(matrix, kernel, output);
            ValidateNotNull(mask, nameof(mask));
            ValidateFT02DIterationInputs(matrix, kernel, mask);
            NativeException.ThrowIfError(NativeMethods.FuzzyFT02DIteration(matrix.NativeHandle, kernel.NativeHandle, output.NativeHandle, mask.NativeHandle, OptionalHandle(maskOutput), firstStop ? 1 : 0, out int state));
            return state;
        }

        /// <summary>Computes optimized F0-transform with linear basic function. 使用线性基础函数计算优化 F0-transform。</summary>
        public static void FT02DFLProcess(Mat matrix, int radius, Mat output)
        {
            ValidateNotNull(matrix, nameof(matrix));
            ValidatePositive(radius, nameof(radius));
            ValidateNotNull(output, nameof(output));
            ValidateFT02DFLProcessInput("FT02D FL process", matrix);
            NativeException.ThrowIfError(NativeMethods.FuzzyFT02DFLProcess(matrix.NativeHandle, radius, output.NativeHandle));
        }

        /// <summary>Computes optimized F0-transform with linear basic function and returns the output. 使用线性基础函数计算优化 F0-transform 并返回输出。</summary>
        public static Mat FT02DFLProcess(Mat matrix, int radius)
        {
            var output = new Mat();
            try
            {
                FT02DFLProcess(matrix, radius, output);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>Computes float optimized F0-transform with linear basic function. 使用线性基础函数计算 float 优化 F0-transform。</summary>
        public static void FT02DFLProcessFloat(Mat matrix, int radius, Mat output)
        {
            ValidateNotNull(matrix, nameof(matrix));
            ValidatePositive(radius, nameof(radius));
            ValidateNotNull(output, nameof(output));
            ValidateFT02DFLProcessInput("FT02D FL process float", matrix);
            NativeException.ThrowIfError(NativeMethods.FuzzyFT02DFLProcessFloat(matrix.NativeHandle, radius, output.NativeHandle));
        }

        /// <summary>Computes float optimized F0-transform with linear basic function and returns the output. 使用线性基础函数计算 float 优化 F0-transform 并返回输出。</summary>
        public static Mat FT02DFLProcessFloat(Mat matrix, int radius)
        {
            var output = new Mat();
            try
            {
                FT02DFLProcessFloat(matrix, radius, output);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>Computes F1-transform components. 计算 F1-transform components。</summary>
        public static void FT12DComponents(Mat matrix, Mat kernel, Mat components)
        {
            ValidateMatrixKernelOutput(matrix, kernel, components);
            ValidateFT12DSingleChannelInputs("FT12D components", matrix, kernel);
            NativeException.ThrowIfError(NativeMethods.FuzzyFT12DComponents(matrix.NativeHandle, kernel.NativeHandle, components.NativeHandle));
        }

        /// <summary>Computes and returns F1-transform components. 计算并返回 F1-transform components。</summary>
        public static Mat FT12DComponents(Mat matrix, Mat kernel)
        {
            var components = new Mat();
            try
            {
                FT12DComponents(matrix, kernel, components);
                return components;
            }
            catch
            {
                components.Dispose();
                throw;
            }
        }

        /// <summary>Computes F1-transform polynomial component matrices. 计算 F1-transform polynomial component 矩阵。</summary>
        public static void FT12DPolynomial(Mat matrix, Mat kernel, Mat c00, Mat c10, Mat c01, Mat components, Mat? mask = null)
        {
            ValidateNotNull(matrix, nameof(matrix));
            ValidateNotNull(kernel, nameof(kernel));
            ValidateNotNull(c00, nameof(c00));
            ValidateNotNull(c10, nameof(c10));
            ValidateNotNull(c01, nameof(c01));
            ValidateNotNull(components, nameof(components));
            ValidateFT12DPolynomialInputs(matrix, kernel, mask);
            NativeException.ThrowIfError(NativeMethods.FuzzyFT12DPolynomial(matrix.NativeHandle, kernel.NativeHandle, c00.NativeHandle, c10.NativeHandle, c01.NativeHandle, components.NativeHandle, OptionalHandle(mask)));
        }

        /// <summary>Creates the vertical polynomial matrix. 创建 vertical polynomial 矩阵。</summary>
        public static void FT12DCreatePolynomMatrixVertical(int radius, Mat matrix, int channels)
        {
            ValidatePositive(radius, nameof(radius));
            ValidateNotNull(matrix, nameof(matrix));
            ValidatePositive(channels, nameof(channels));
            NativeException.ThrowIfError(NativeMethods.FuzzyFT12DCreatePolynomMatrixVertical(radius, matrix.NativeHandle, channels));
        }

        /// <summary>Creates and returns the vertical polynomial matrix. 创建并返回 vertical polynomial 矩阵。</summary>
        public static Mat FT12DCreatePolynomMatrixVertical(int radius, int channels)
        {
            var matrix = new Mat();
            try
            {
                FT12DCreatePolynomMatrixVertical(radius, matrix, channels);
                return matrix;
            }
            catch
            {
                matrix.Dispose();
                throw;
            }
        }

        /// <summary>Creates the horizontal polynomial matrix. 创建 horizontal polynomial 矩阵。</summary>
        public static void FT12DCreatePolynomMatrixHorizontal(int radius, Mat matrix, int channels)
        {
            ValidatePositive(radius, nameof(radius));
            ValidateNotNull(matrix, nameof(matrix));
            ValidatePositive(channels, nameof(channels));
            NativeException.ThrowIfError(NativeMethods.FuzzyFT12DCreatePolynomMatrixHorizontal(radius, matrix.NativeHandle, channels));
        }

        /// <summary>Creates and returns the horizontal polynomial matrix. 创建并返回 horizontal polynomial 矩阵。</summary>
        public static Mat FT12DCreatePolynomMatrixHorizontal(int radius, int channels)
        {
            var matrix = new Mat();
            try
            {
                FT12DCreatePolynomMatrixHorizontal(radius, matrix, channels);
                return matrix;
            }
            catch
            {
                matrix.Dispose();
                throw;
            }
        }

        /// <summary>Computes inverse F1-transform. 计算 inverse F1-transform。</summary>
        public static void FT12DInverseFT(Mat components, Mat kernel, Mat output, int width, int height)
        {
            ValidateMatrixKernelOutput(components, kernel, output);
            ValidateFT12DInverseInputs(components, kernel);
            ValidatePositive(width, nameof(width));
            ValidatePositive(height, nameof(height));
            NativeException.ThrowIfError(NativeMethods.FuzzyFT12DInverseFT(components.NativeHandle, kernel.NativeHandle, output.NativeHandle, width, height));
        }

        /// <summary>Computes inverse F1-transform and returns the output. 计算 inverse F1-transform 并返回输出。</summary>
        public static Mat FT12DInverseFT(Mat components, Mat kernel, int width, int height)
        {
            var output = new Mat();
            try
            {
                FT12DInverseFT(components, kernel, output, width, height);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>Computes F1-transform and inverse F1-transform in one step. 一步计算 F1-transform 与 inverse F1-transform。</summary>
        public static void FT12DProcess(Mat matrix, Mat kernel, Mat output, Mat? mask = null)
        {
            ValidateMatrixKernelOutput(matrix, kernel, output);
            ValidateFTransformProcessInputs("FT12D process", matrix, kernel, mask);
            NativeException.ThrowIfError(NativeMethods.FuzzyFT12DProcess(matrix.NativeHandle, kernel.NativeHandle, output.NativeHandle, OptionalHandle(mask)));
        }

        /// <summary>Computes F1-transform and inverse F1-transform in one step and returns the output. 一步计算 F1-transform 与 inverse F1-transform 并返回输出。</summary>
        public static Mat FT12DProcess(Mat matrix, Mat kernel, Mat? mask = null)
        {
            var output = new Mat();
            try
            {
                FT12DProcess(matrix, kernel, output, mask);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        private static IntPtr OptionalHandle(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        private static void ValidateMatrixKernelOutput(Mat matrix, Mat kernel, Mat output)
        {
            ValidateNotNull(matrix, nameof(matrix));
            ValidateNotNull(kernel, nameof(kernel));
            ValidateNotNull(output, nameof(output));
        }

        private static void ValidateInpaintInputs(Mat mask)
        {
            if (!mask.Empty && mask.Channels != 1)
            {
                throw new ArgumentException("Fuzzy inpaint mask must be single-channel.", nameof(mask));
            }
        }

        private static void ValidateFilterInputs(Mat image, Mat kernel)
        {
            if (image.Channels != kernel.Channels)
            {
                throw new ArgumentException("Fuzzy filter requires image and kernel to have the same channel count.", nameof(kernel));
            }
        }

        private static void ValidateFT02DComponentsInputs(Mat matrix, Mat kernel, Mat? mask)
        {
            if (matrix.Channels != kernel.Channels)
            {
                throw new ArgumentException("FT02D components require matrix and kernel to have the same channel count.", nameof(kernel));
            }

            if (mask != null && !mask.Empty && mask.Channels != 1)
            {
                throw new ArgumentException("FT02D components mask must be single-channel.", nameof(mask));
            }
        }

        private static void ValidateFTransformProcessInputs(string operationName, Mat matrix, Mat kernel, Mat? mask)
        {
            if (matrix.Channels != kernel.Channels)
            {
                throw new ArgumentException(operationName + " requires matrix and kernel to have the same channel count.", nameof(kernel));
            }

            if (mask != null && !mask.Empty && mask.Channels != 1)
            {
                throw new ArgumentException(operationName + " mask must be single-channel.", nameof(mask));
            }
        }

        private static void ValidateFT02DIterationInputs(Mat matrix, Mat kernel, Mat mask)
        {
            if (matrix.Channels != kernel.Channels)
            {
                throw new ArgumentException("FT02D iteration requires matrix and kernel to have the same channel count.", nameof(kernel));
            }

            if (mask.Channels != 1)
            {
                throw new ArgumentException("FT02D iteration mask must be single-channel.", nameof(mask));
            }
        }

        private static void ValidateFT02DInverseInputs(Mat components, Mat kernel)
        {
            if (components.Channels != 1)
            {
                throw new ArgumentException("FT02D inverse transform requires components to be single-channel.", nameof(components));
            }

            if (kernel.Channels != 1)
            {
                throw new ArgumentException("FT02D inverse transform requires kernel to be single-channel.", nameof(kernel));
            }
        }

        private static void ValidateFT02DFLProcessInput(string operationName, Mat matrix)
        {
            if (matrix.Channels != 3)
            {
                throw new ArgumentException(operationName + " requires matrix to have exactly three channels.", nameof(matrix));
            }
        }

        private static void ValidateFT12DSingleChannelInputs(string operationName, Mat matrix, Mat kernel)
        {
            if (matrix.Channels != 1)
            {
                throw new ArgumentException(operationName + " requires matrix to be single-channel.", nameof(matrix));
            }

            if (kernel.Channels != 1)
            {
                throw new ArgumentException(operationName + " requires kernel to be single-channel.", nameof(kernel));
            }
        }

        private static void ValidateFT12DPolynomialInputs(Mat matrix, Mat kernel, Mat? mask)
        {
            ValidateFT12DSingleChannelInputs("FT12D polynomial", matrix, kernel);

            if (mask != null && !mask.Empty && mask.Channels != 1)
            {
                throw new ArgumentException("FT12D polynomial mask must be single-channel.", nameof(mask));
            }
        }

        private static void ValidateFT12DInverseInputs(Mat components, Mat kernel)
        {
            if (components.Channels != 1)
            {
                throw new ArgumentException("FT12D inverse transform requires components to be single-channel.", nameof(components));
            }

            if (kernel.Channels != 1)
            {
                throw new ArgumentException("FT12D inverse transform requires kernel to be single-channel.", nameof(kernel));
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

        private static void ValidatePositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be positive.");
            }
        }

        private static void ValidateEnum(FuzzyFunctionType value, string parameterName)
        {
            if (value != FuzzyFunctionType.Linear && value != FuzzyFunctionType.Sinus)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported fuzzy function type.");
            }
        }

        private static void ValidateEnum(FuzzyInpaintAlgorithm value, string parameterName)
        {
            if (value != FuzzyInpaintAlgorithm.OneStep && value != FuzzyInpaintAlgorithm.MultiStep && value != FuzzyInpaintAlgorithm.Iterative)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported fuzzy inpaint algorithm.");
            }
        }
    }
}
