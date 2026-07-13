using System;
using OpenCvSharp.Core;
using OpenCvSharp.Fuzzy;

namespace OpenCvSharp.Tests.Fuzzy
{
    public sealed class FuzzyTests
    {
        [Fact]
        public void EnumsMatchOpenCvConstants()
        {
            Assert.Equal(1, (int)FuzzyFunctionType.Linear);
            Assert.Equal(2, (int)FuzzyFunctionType.Sinus);
            Assert.Equal(1, (int)FuzzyInpaintAlgorithm.OneStep);
            Assert.Equal(2, (int)FuzzyInpaintAlgorithm.MultiStep);
            Assert.Equal(3, (int)FuzzyInpaintAlgorithm.Iterative);
        }

        [Fact]
        public void ManagedValidationRejectsNullAndOutOfRangeArguments()
        {
            using (Mat matrix = new Mat(4, 4, MatType.CV_32FC1, new Scalar(1.0)))
            using (Mat image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(8, 16, 32)))
            using (Mat mask = new Mat(4, 4, MatType.CV_8UC1, new Scalar(0)))
            using (Mat colorMask = new Mat(4, 4, MatType.CV_8UC3, new Scalar(0)))
            using (Mat kernel = new Mat(3, 3, MatType.CV_32FC1, new Scalar(1.0)))
            using (Mat colorKernel = new Mat(3, 3, MatType.CV_32FC3, new Scalar(1.0)))
            using (Mat output = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.CreateKernel(null!, matrix, output, 1));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.CreateKernel(matrix, null!, output, 1));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.CreateKernel(matrix, matrix, null!, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.CreateKernel(matrix, matrix, output, 0));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.CreateKernel(null!, matrix, 1));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.CreateKernel(matrix, null!, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.CreateKernel(matrix, matrix, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.CreateKernel((FuzzyFunctionType)99, 1, output, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.CreateKernel(FuzzyFunctionType.Linear, 0, output, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.CreateKernel(FuzzyFunctionType.Linear, 1, output, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.CreateKernel((FuzzyFunctionType)99, 1, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.CreateKernel(FuzzyFunctionType.Linear, 0, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.CreateKernel(FuzzyFunctionType.Linear, 1, 0));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.Inpaint(null!, mask, output, 1, FuzzyFunctionType.Linear, FuzzyInpaintAlgorithm.OneStep));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.Inpaint(image, null!, output, 1, FuzzyFunctionType.Linear, FuzzyInpaintAlgorithm.OneStep));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.Inpaint(image, mask, null!, 1, FuzzyFunctionType.Linear, FuzzyInpaintAlgorithm.OneStep));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.Inpaint(image, mask, output, 0, FuzzyFunctionType.Linear, FuzzyInpaintAlgorithm.OneStep));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.Inpaint(image, mask, output, 1, (FuzzyFunctionType)99, FuzzyInpaintAlgorithm.OneStep));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.Inpaint(image, mask, output, 1, FuzzyFunctionType.Linear, (FuzzyInpaintAlgorithm)99));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.Inpaint(null!, mask, 1, FuzzyFunctionType.Linear, FuzzyInpaintAlgorithm.OneStep));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.Inpaint(image, null!, 1, FuzzyFunctionType.Linear, FuzzyInpaintAlgorithm.OneStep));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.Inpaint(image, mask, 0, FuzzyFunctionType.Linear, FuzzyInpaintAlgorithm.OneStep));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.Inpaint(image, mask, 1, (FuzzyFunctionType)99, FuzzyInpaintAlgorithm.OneStep));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.Inpaint(image, mask, 1, FuzzyFunctionType.Linear, (FuzzyInpaintAlgorithm)99));
                ArgumentException colorMaskVoidException = Assert.Throws<ArgumentException>(() =>
                    FuzzyCv2.Inpaint(image, colorMask, output, 1, FuzzyFunctionType.Linear, FuzzyInpaintAlgorithm.OneStep));
                Assert.Equal("mask", colorMaskVoidException.ParamName);
                ArgumentException colorMaskReturningException = Assert.Throws<ArgumentException>(() =>
                    FuzzyCv2.Inpaint(image, colorMask, 1, FuzzyFunctionType.Linear, FuzzyInpaintAlgorithm.OneStep));
                Assert.Equal("mask", colorMaskReturningException.ParamName);
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.Filter(null!, kernel, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.Filter(matrix, null!, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.Filter(matrix, kernel, null!));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.Filter(null!, kernel));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.Filter(matrix, null!));
                ArgumentException filterKernelException = Assert.Throws<ArgumentException>(() =>
                    FuzzyCv2.Filter(image, kernel, output));
                Assert.Equal("kernel", filterKernelException.ParamName);
                ArgumentException returningFilterKernelException = Assert.Throws<ArgumentException>(() =>
                    FuzzyCv2.Filter(image, kernel));
                Assert.Equal("kernel", returningFilterKernelException.ParamName);
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DComponents(null!, kernel));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DComponents(matrix, null!));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DComponents(null!, kernel, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DComponents(matrix, null!, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DComponents(matrix, kernel, components: null!));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DComponents(matrix, colorKernel, output));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DComponents(matrix, kernel, output, colorMask));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DComponents(matrix, colorKernel));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DComponents(matrix, kernel, colorMask));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DInverseFT(null!, kernel, output, 4, 4));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DInverseFT(matrix, null!, output, 4, 4));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DInverseFT(matrix, kernel, null!, 4, 4));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT02DInverseFT(matrix, kernel, output, 0, 4));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT02DInverseFT(matrix, kernel, output, 4, 0));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DInverseFT(image, kernel, output, 4, 4));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DInverseFT(matrix, colorKernel, output, 4, 4));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DInverseFT(null!, kernel, 4, 4));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DInverseFT(matrix, null!, 4, 4));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT02DInverseFT(matrix, kernel, 0, 4));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT02DInverseFT(matrix, kernel, 4, 0));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DInverseFT(image, kernel, 4, 4));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DInverseFT(matrix, colorKernel, 4, 4));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DProcess(null!, kernel));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DProcess(matrix, null!));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DProcess(null!, kernel, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DProcess(matrix, null!, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DProcess(matrix, kernel, output: null!));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DProcess(matrix, colorKernel, output));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DProcess(matrix, kernel, output, colorMask));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DProcess(matrix, colorKernel));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DProcess(matrix, kernel, colorMask));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DIteration(null!, kernel, output, mask));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DIteration(matrix, null!, output, mask));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DIteration(matrix, kernel, null!, mask));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DIteration(matrix, colorKernel, output, mask));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DIteration(matrix, kernel, output, colorMask));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT12DInverseFT(matrix, kernel, output, 4, 0));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DIteration(matrix, kernel, output, null!));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DFLProcess(null!, 1, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DFLProcess(image, 1, null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT02DFLProcess(image, 0, output));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DFLProcess(matrix, 1, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DFLProcess(null!, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT02DFLProcess(image, 0));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DFLProcess(matrix, 1));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DFLProcessFloat(null!, 1, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DFLProcessFloat(image, 1, null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT02DFLProcessFloat(image, 0, output));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DFLProcessFloat(matrix, 1, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT02DFLProcessFloat(null!, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT02DFLProcessFloat(image, 0));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT02DFLProcessFloat(matrix, 1));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DComponents(null!, kernel, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DComponents(matrix, null!, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DComponents(matrix, kernel, null!));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DComponents(null!, kernel));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DComponents(matrix, null!));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DComponents(image, kernel, output));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DComponents(matrix, colorKernel, output));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DComponents(image, kernel));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DComponents(matrix, colorKernel));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DCreatePolynomMatrixVertical(1, null!, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT12DCreatePolynomMatrixVertical(0, output, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT12DCreatePolynomMatrixVertical(1, output, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT12DCreatePolynomMatrixVertical(0, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT12DCreatePolynomMatrixVertical(1, 0));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DCreatePolynomMatrixHorizontal(1, null!, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT12DCreatePolynomMatrixHorizontal(0, output, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT12DCreatePolynomMatrixHorizontal(1, output, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT12DCreatePolynomMatrixHorizontal(0, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT12DCreatePolynomMatrixHorizontal(1, 0));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DInverseFT(null!, kernel, output, 4, 4));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DInverseFT(matrix, null!, output, 4, 4));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DInverseFT(matrix, kernel, null!, 4, 4));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT12DInverseFT(matrix, kernel, output, 0, 4));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DInverseFT(image, kernel, output, 4, 4));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DInverseFT(matrix, colorKernel, output, 4, 4));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DInverseFT(null!, kernel, 4, 4));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DInverseFT(matrix, null!, 4, 4));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT12DInverseFT(matrix, kernel, 0, 4));
                Assert.Throws<ArgumentOutOfRangeException>(() => FuzzyCv2.FT12DInverseFT(matrix, kernel, 4, 0));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DInverseFT(image, kernel, 4, 4));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DInverseFT(matrix, colorKernel, 4, 4));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DProcess(null!, kernel));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DProcess(matrix, null!));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DProcess(null!, kernel, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DProcess(matrix, null!, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DProcess(matrix, kernel, output: null!));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DProcess(matrix, colorKernel, output));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DProcess(matrix, kernel, output, colorMask));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DProcess(matrix, colorKernel));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DProcess(matrix, kernel, colorMask));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DPolynomial(null!, kernel, output, output, output, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DPolynomial(matrix, null!, output, output, output, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DPolynomial(matrix, kernel, null!, output, output, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DPolynomial(matrix, kernel, output, null!, output, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DPolynomial(matrix, kernel, output, output, null!, output));
                Assert.Throws<ArgumentNullException>(() => FuzzyCv2.FT12DPolynomial(matrix, kernel, output, output, output, null!));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DPolynomial(image, kernel, output, output, output, output));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DPolynomial(matrix, colorKernel, output, output, output, output));
                Assert.Throws<ArgumentException>(() => FuzzyCv2.FT12DPolynomial(matrix, kernel, output, output, output, output, colorMask));
            }
        }

        [Fact]
        public void FunctionSmokeOrBoundaryRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (Mat image = CreateBgrImage())
                using (Mat gray = CreateFloatImage())
                using (Mat mask = CreateMask())
                using (Mat kernelGray = FuzzyCv2.CreateKernel(FuzzyFunctionType.Linear, 2, 1))
                using (Mat kernelBgr = FuzzyCv2.CreateKernel(FuzzyFunctionType.Linear, 2, image.Channels))
                using (Mat filtered = FuzzyCv2.Filter(image, kernelBgr))
                using (Mat inpainted = FuzzyCv2.Inpaint(image, mask, 2, FuzzyFunctionType.Linear, FuzzyInpaintAlgorithm.OneStep))
                using (Mat components0 = FuzzyCv2.FT02DComponents(gray, kernelGray))
                using (Mat inverse0 = FuzzyCv2.FT02DInverseFT(components0, kernelGray, gray.Cols, gray.Rows))
                using (Mat process0 = FuzzyCv2.FT02DProcess(gray, kernelGray))
                using (Mat flProcess = FuzzyCv2.FT02DFLProcess(image, 2))
                using (Mat flProcessFloat = FuzzyCv2.FT02DFLProcessFloat(image, 2))
                using (Mat components1 = FuzzyCv2.FT12DComponents(gray, kernelGray))
                using (Mat vertical = FuzzyCv2.FT12DCreatePolynomMatrixVertical(2, 1))
                using (Mat horizontal = FuzzyCv2.FT12DCreatePolynomMatrixHorizontal(2, 1))
                using (Mat inverse1 = FuzzyCv2.FT12DInverseFT(components1, kernelGray, gray.Cols, gray.Rows))
                using (Mat process1 = FuzzyCv2.FT12DProcess(gray, kernelGray))
                {
                    AssertImageShape(filtered, image);
                    AssertImageShape(inpainted, image);
                    AssertImageShape(inverse0, gray);
                    AssertImageShape(process0, gray);
                    AssertImageShape(flProcess, image);
                    AssertImageShape(flProcessFloat, image);
                    AssertImageShape(inverse1, gray);
                    AssertImageShape(process1, gray);
                    Assert.False(components0.Empty);
                    Assert.False(components1.Empty);
                    Assert.False(vertical.Empty);
                    Assert.False(horizontal.Empty);
                }

                using (Mat gray = CreateFloatImage())
                using (Mat mask = CreateMask())
                using (Mat kernel = FuzzyCv2.CreateKernel(FuzzyFunctionType.Linear, 2, 1))
                using (Mat output = new Mat())
                using (Mat maskOutput = new Mat())
                {
                    int state = FuzzyCv2.FT02DIteration(gray, kernel, output, mask, maskOutput);
                    Assert.True(state >= -1);
                    Assert.False(output.Empty);
                    Assert.Equal(gray.Rows, output.Rows);
                    Assert.Equal(gray.Cols, output.Cols);
                }
            }
            catch (OpenCvException ex) when (IsFuzzyModuleMissing(ex))
            {
                Assert.True(IsFuzzyModuleMissing(ex), ex.Message);
            }
        }

        private static Mat CreateFloatImage()
        {
            var mat = new Mat(8, 8, MatType.CV_32FC1);
            var values = new float[64];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (float)(0.1 + (i % 11) * 0.05);
            }

            mat.CopyFrom(values);
            return mat;
        }

        private static Mat CreateBgrImage()
        {
            var mat = new Mat(8, 8, MatType.CV_8UC3);
            var values = new byte[8 * 8 * 3];
            for (int i = 0; i < values.Length; i += 3)
            {
                int pixel = i / 3;
                values[i] = (byte)(20 + pixel % 21);
                values[i + 1] = (byte)(40 + pixel % 23);
                values[i + 2] = (byte)(80 + pixel % 29);
            }

            mat.CopyFrom(values);
            return mat;
        }

        private static Mat CreateMask()
        {
            var mat = new Mat(8, 8, MatType.CV_8UC1);
            var values = new byte[64];
            for (int y = 3; y < 5; y++)
            {
                for (int x = 3; x < 5; x++)
                {
                    values[y * 8 + x] = 255;
                }
            }

            mat.CopyFrom(values);
            return mat;
        }

        private static void AssertImageShape(Mat output, Mat source)
        {
            Assert.False(output.Empty);
            Assert.Equal(source.Rows, output.Rows);
            Assert.Equal(source.Cols, output.Cols);
        }

        private static bool IsFuzzyModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("fuzzy", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("Fuzzy", StringComparison.OrdinalIgnoreCase) >= 0;
        }

    }
}
