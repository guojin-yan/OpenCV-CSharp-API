using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Tests.Core
{
    public class CoreSpectralTransformTests
    {
        [Fact]
        public void PolarAndMagnitudeOperationsWorkWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat x = new Mat(1, 2, MatType.CV_32FC1))
            using (Mat y = new Mat(1, 2, MatType.CV_32FC1))
            using (Mat magnitude = new Mat())
            using (Mat angle = new Mat())
            using (Mat roundtripX = new Mat())
            using (Mat roundtripY = new Mat())
            {
                x.CopyFrom<float>(new float[] { 3.0f, 0.0f });
                y.CopyFrom<float>(new float[] { 4.0f, 1.0f });

                Cv2.Magnitude(x, y, magnitude);
                Cv2.Phase(x, y, angle);
                Cv2.CartToPolar(x, y, magnitude, angle);
                Cv2.PolarToCart(magnitude, angle, roundtripX, roundtripY);

                float[] mag = magnitude.ToArray<float>();
                Assert.Equal(5.0f, mag[0], 5);
                Assert.Equal(1.0f, mag[1], 5);
                Assert.True(Math.Abs(3.0f - roundtripX.ToArray<float>()[0]) < 1e-3f);
                Assert.True(Math.Abs(4.0f - roundtripY.ToArray<float>()[0]) < 1e-3f);

                using (Mat returnedMagnitude = Cv2.Magnitude(x, y))
                using (Mat returnedPhase = Cv2.Phase(x, y))
                {
                    Assert.False(returnedMagnitude.Empty);
                    Assert.False(returnedPhase.Empty);
                }
            }
        }

        [Fact]
        public void DftIdftAndDctIdctRoundtripWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat input = new Mat(1, 4, MatType.CV_64FC1))
            using (Mat spectrum = new Mat())
            using (Mat recovered = new Mat())
            using (Mat dct = new Mat())
            using (Mat idct = new Mat())
            {
                input.CopyFrom<double>(new double[] { 1.0, 2.0, 3.0, 4.0 });

                Cv2.Dft(input, spectrum, DftFlags.ComplexOutput);
                Cv2.Idft(spectrum, recovered, DftFlags.Scale | DftFlags.RealOutput);
                Cv2.Dct(input, dct);
                Cv2.Idct(dct, idct);

                AssertClose(input.ToArray<double>(), recovered.ToArray<double>(), 1e-9);
                AssertClose(input.ToArray<double>(), idct.ToArray<double>(), 1e-9);
                Assert.True(Cv2.GetOptimalDftSize(15) >= 15);

                using (Mat returnedSpectrum = Cv2.Dft(input, DftFlags.ComplexOutput))
                using (Mat returnedRecovered = Cv2.Idft(spectrum, DftFlags.Scale | DftFlags.RealOutput))
                using (Mat returnedDct = Cv2.Dct(input))
                using (Mat returnedIdct = Cv2.Idct(dct))
                using (Mat multiplied = Cv2.MulSpectrums(spectrum, spectrum))
                using (Mat divided = Cv2.DivSpectrums(spectrum, spectrum))
                {
                    AssertClose(spectrum.ToArray<double>(), returnedSpectrum.ToArray<double>(), 1e-9);
                    AssertClose(recovered.ToArray<double>(), returnedRecovered.ToArray<double>(), 1e-9);
                    AssertClose(dct.ToArray<double>(), returnedDct.ToArray<double>(), 1e-9);
                    AssertClose(idct.ToArray<double>(), returnedIdct.ToArray<double>(), 1e-9);
                    Assert.False(multiplied.Empty);
                    Assert.False(divided.Empty);
                }
            }
        }

        [Fact]
        public void ExpLogSqrtAndPowWorkWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat input = new Mat(1, 3, MatType.CV_64FC1))
            using (Mat exp = new Mat())
            using (Mat log = new Mat())
            using (Mat sqrt = new Mat())
            using (Mat pow = new Mat())
            {
                input.CopyFrom<double>(new double[] { 1.0, 4.0, 9.0 });

                Cv2.Exp(input, exp);
                Cv2.Log(exp, log);
                Cv2.Sqrt(input, sqrt);
                Cv2.Pow(sqrt, 2.0, pow);

                AssertClose(input.ToArray<double>(), log.ToArray<double>(), 1e-9);
                AssertClose(input.ToArray<double>(), pow.ToArray<double>(), 1e-9);

                using (Mat returned = Cv2.Sqrt(input))
                {
                    Assert.Equal(new double[] { 1.0, 2.0, 3.0 }, returned.ToArray<double>());
                }
            }
        }

        [Fact]
        public void SpectralValidationRejectsNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => Cv2.Dft(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Dft(null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Idft(null!));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Dft(new Mat(), new Mat(), (DftFlags)8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Dft(new Mat(), (DftFlags)8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Idft(new Mat(), new Mat(), (DftFlags)8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Idft(new Mat(), (DftFlags)8));
            Assert.Throws<ArgumentNullException>(() => Cv2.Dct(null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Idct(null!));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Dct(new Mat(), new Mat(), (DctFlags)8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Dct(new Mat(), (DctFlags)8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Idct(new Mat(), new Mat(), (DctFlags)8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Idct(new Mat(), (DctFlags)8));
            Assert.Throws<ArgumentNullException>(() => Cv2.MulSpectrums(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.DivSpectrums(null!, null!));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.MulSpectrums(new Mat(), new Mat(), new Mat(), (MulSpectrumsFlags)8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.MulSpectrums(new Mat(), new Mat(), (MulSpectrumsFlags)8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.DivSpectrums(new Mat(), new Mat(), new Mat(), (MulSpectrumsFlags)8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.DivSpectrums(new Mat(), new Mat(), (MulSpectrumsFlags)8));
            Assert.Throws<ArgumentNullException>(() => Cv2.Magnitude(null!, null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Magnitude(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Phase(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.CartToPolar(null!, null!, null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Exp(null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Log(null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Sqrt(null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Pow(null!, 2.0));
        }

        [Fact]
        public void DftAndIdftRejectInvalidInputContract()
        {
            using (var unsupportedDepth = new Mat(1, 2, MatType.CV_8UC1))
            using (var singleChannelFloat = new Mat(1, 2, MatType.CV_32FC1))
            using (var dst = new Mat())
            {
                ArgumentException dftDepthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Dft(unsupportedDepth, dst));
                ArgumentException dftReturnDepthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Dft(unsupportedDepth));
                ArgumentException idftDepthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Idft(unsupportedDepth, dst));
                ArgumentException idftReturnDepthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Idft(unsupportedDepth));
                ArgumentException dftComplexInputException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Dft(singleChannelFloat, dst, DftFlags.ComplexInput));
                ArgumentException idftComplexInputException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Idft(singleChannelFloat, dst, DftFlags.ComplexInput));

                Assert.Equal("src", dftDepthException.ParamName);
                Assert.Equal("src", dftReturnDepthException.ParamName);
                Assert.Equal("src", idftDepthException.ParamName);
                Assert.Equal("src", idftReturnDepthException.ParamName);
                Assert.Equal("src", dftComplexInputException.ParamName);
                Assert.Equal("src", idftComplexInputException.ParamName);
            }
        }

        [Fact]
        public void DftAndIdftRejectConflictingOutputFlags()
        {
            const DftFlags conflictingOutputFlags = DftFlags.ComplexOutput | DftFlags.RealOutput;

            using (var src = new Mat(1, 2, MatType.CV_32FC1))
            using (var dst = new Mat())
            {
                ArgumentException dftException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Dft(src, dst, conflictingOutputFlags));
                ArgumentException dftReturnException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Dft(src, conflictingOutputFlags));
                ArgumentException idftException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Idft(src, dst, conflictingOutputFlags));
                ArgumentException idftReturnException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Idft(src, conflictingOutputFlags));

                Assert.Equal("flags", dftException.ParamName);
                Assert.Equal("flags", dftReturnException.ParamName);
                Assert.Equal("flags", idftException.ParamName);
                Assert.Equal("flags", idftReturnException.ParamName);
            }
        }

        [Fact]
        public void DctAndIdctRejectInvalidInputContract()
        {
            using (var unsupportedDepth = new Mat(1, 2, MatType.CV_8UC1))
            using (var multiChannelFloat = new Mat(1, 2, MatType.CV_32FC2))
            using (var dst = new Mat())
            {
                ArgumentException dctDepthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Dct(unsupportedDepth, dst));
                ArgumentException dctTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Dct(multiChannelFloat, dst));
                ArgumentException dctReturnTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Dct(multiChannelFloat));
                ArgumentException idctDepthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Idct(unsupportedDepth, dst));
                ArgumentException idctTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Idct(multiChannelFloat, dst));
                ArgumentException idctReturnTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Idct(multiChannelFloat));

                Assert.Equal("src", dctDepthException.ParamName);
                Assert.Equal("src", dctTypeException.ParamName);
                Assert.Equal("src", dctReturnTypeException.ParamName);
                Assert.Equal("src", idctDepthException.ParamName);
                Assert.Equal("src", idctTypeException.ParamName);
                Assert.Equal("src", idctReturnTypeException.ParamName);
            }
        }

        [Fact]
        public void DftAndIdftAllowComplexInputWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var complexInput = new Mat(1, 2, MatType.CV_32FC2))
            using (var spectrum = new Mat())
            using (var recovered = new Mat())
            {
                complexInput.CopyFrom<float>(new[] { 1.0f, 0.0f, 2.0f, 0.0f });

                Cv2.Dft(complexInput, spectrum, DftFlags.ComplexInput);
                Cv2.Idft(spectrum, recovered, DftFlags.ComplexInput | DftFlags.Scale);

                Assert.Equal(MatType.CV_32FC2, spectrum.Type);
                Assert.Equal(MatType.CV_32FC2, recovered.Type);
                AssertClose(complexInput.ToArray<float>(), recovered.ToArray<float>(), 1e-5f);
            }
        }

        [Fact]
        public void ExpAndLogRejectUnsupportedSourceDepth()
        {
            using (var unsupportedDepth = new Mat(1, 2, MatType.CV_8UC1))
            using (var dst = new Mat())
            {
                ArgumentException expVoidException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Exp(unsupportedDepth, dst));
                ArgumentException expReturnException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Exp(unsupportedDepth));
                ArgumentException logVoidException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Log(unsupportedDepth, dst));
                ArgumentException logReturnException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Log(unsupportedDepth));

                Assert.Equal("src", expVoidException.ParamName);
                Assert.Equal("src", expReturnException.ParamName);
                Assert.Equal("src", logVoidException.ParamName);
                Assert.Equal("src", logReturnException.ParamName);
            }
        }

        [Fact]
        public void MagnitudeAndPhaseRejectInvalidInputContract()
        {
            using (var x = new Mat(1, 2, MatType.CV_32FC1))
            using (var sizeMismatch = new Mat(1, 3, MatType.CV_32FC1))
            using (var typeMismatch = new Mat(1, 2, MatType.CV_64FC1))
            using (var unsupportedDepth = new Mat(1, 2, MatType.CV_8UC1))
            using (var dst = new Mat())
            {
                ArgumentException magnitudeSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Magnitude(x, sizeMismatch, dst));
                ArgumentException magnitudeTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Magnitude(x, typeMismatch, dst));
                ArgumentException magnitudeDepthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Magnitude(unsupportedDepth, unsupportedDepth, dst));
                ArgumentException phaseSizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Phase(x, sizeMismatch, dst));
                ArgumentException phaseTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Phase(x, typeMismatch, dst));
                ArgumentException phaseDepthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Phase(unsupportedDepth, unsupportedDepth, dst));

                Assert.Equal("y", magnitudeSizeException.ParamName);
                Assert.Equal("y", magnitudeTypeException.ParamName);
                Assert.Equal("x", magnitudeDepthException.ParamName);
                Assert.Equal("y", phaseSizeException.ParamName);
                Assert.Equal("y", phaseTypeException.ParamName);
                Assert.Equal("x", phaseDepthException.ParamName);
            }
        }

        [Fact]
        public void CartToPolarRejectsInvalidInputAndOutputContract()
        {
            using (var x = new Mat(1, 2, MatType.CV_32FC1))
            using (var y = new Mat(1, 2, MatType.CV_32FC1))
            using (var sizeMismatch = new Mat(1, 3, MatType.CV_32FC1))
            using (var typeMismatch = new Mat(1, 2, MatType.CV_64FC1))
            using (var unsupportedDepth = new Mat(1, 2, MatType.CV_8UC1))
            using (var magnitude = new Mat())
            using (var angle = new Mat())
            {
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.CartToPolar(x, sizeMismatch, magnitude, angle));
                ArgumentException typeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.CartToPolar(x, typeMismatch, magnitude, angle));
                ArgumentException depthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.CartToPolar(unsupportedDepth, unsupportedDepth, magnitude, angle));
                ArgumentException outputException = Assert.Throws<ArgumentException>(() =>
                    Cv2.CartToPolar(x, y, magnitude, magnitude));

                Assert.Equal("y", sizeException.ParamName);
                Assert.Equal("y", typeException.ParamName);
                Assert.Equal("x", depthException.ParamName);
                Assert.Equal("angle", outputException.ParamName);
            }
        }

        [Fact]
        public void PolarToCartRejectsInvalidInputAndOutputContract()
        {
            using (var magnitude = new Mat(1, 2, MatType.CV_32FC1))
            using (var angle = new Mat(1, 2, MatType.CV_32FC1))
            using (var emptyMagnitude = new Mat())
            using (var unsupportedAngleDepth = new Mat(1, 2, MatType.CV_8UC1))
            using (var magnitudeTypeMismatch = new Mat(1, 2, MatType.CV_64FC1))
            using (var magnitudeSizeMismatch = new Mat(1, 3, MatType.CV_32FC1))
            using (var x = new Mat())
            using (var y = new Mat())
            {
                ArgumentException depthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.PolarToCart(magnitude, unsupportedAngleDepth, x, y));
                ArgumentException typeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.PolarToCart(magnitudeTypeMismatch, angle, x, y));
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.PolarToCart(magnitudeSizeMismatch, angle, x, y));
                ArgumentException outputException = Assert.Throws<ArgumentException>(() =>
                    Cv2.PolarToCart(magnitude, angle, x, x));

                Cv2.PolarToCart(emptyMagnitude, angle, x, y);

                Assert.Equal("angle", depthException.ParamName);
                Assert.Equal("magnitude", typeException.ParamName);
                Assert.Equal("magnitude", sizeException.ParamName);
                Assert.Equal("y", outputException.ParamName);
                Assert.False(x.Empty);
                Assert.False(y.Empty);
            }
        }

        [Fact]
        public void MulSpectrumsRejectsInvalidInputContract()
        {
            using (var a = new Mat(2, 2, MatType.CV_32FC2))
            using (var sizeMismatch = new Mat(2, 3, MatType.CV_32FC2))
            using (var typeMismatch = new Mat(2, 2, MatType.CV_64FC2))
            using (var unsupportedType = new Mat(2, 2, MatType.CV_8UC1))
            using (var dst = new Mat())
            {
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MulSpectrums(a, sizeMismatch, dst));
                ArgumentException typeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MulSpectrums(a, typeMismatch, dst));
                ArgumentException unsupportedException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MulSpectrums(unsupportedType, unsupportedType, dst));
                ArgumentException returnedMatException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MulSpectrums(a, sizeMismatch));

                Assert.Equal("b", sizeException.ParamName);
                Assert.Equal("b", typeException.ParamName);
                Assert.Equal("a", unsupportedException.ParamName);
                Assert.Equal("b", returnedMatException.ParamName);
            }
        }

        [Fact]
        public void DivSpectrumsRejectsInvalidInputAndOutputContract()
        {
            using (var a = new Mat(2, 2, MatType.CV_32FC2))
            using (var b = new Mat(2, 2, MatType.CV_32FC2))
            using (var sizeMismatch = new Mat(2, 3, MatType.CV_32FC2))
            using (var typeMismatch = new Mat(2, 2, MatType.CV_64FC2))
            using (var unsupportedType = new Mat(2, 2, MatType.CV_8UC1))
            using (var dst = new Mat())
            {
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.DivSpectrums(a, sizeMismatch, dst));
                ArgumentException typeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.DivSpectrums(a, typeMismatch, dst));
                ArgumentException unsupportedException = Assert.Throws<ArgumentException>(() =>
                    Cv2.DivSpectrums(unsupportedType, unsupportedType, dst));
                ArgumentException firstAliasException = Assert.Throws<ArgumentException>(() =>
                    Cv2.DivSpectrums(a, b, a));
                ArgumentException secondAliasException = Assert.Throws<ArgumentException>(() =>
                    Cv2.DivSpectrums(a, b, b));
                ArgumentException returnedMatException = Assert.Throws<ArgumentException>(() =>
                    Cv2.DivSpectrums(a, sizeMismatch));

                Assert.Equal("b", sizeException.ParamName);
                Assert.Equal("b", typeException.ParamName);
                Assert.Equal("a", unsupportedException.ParamName);
                Assert.Equal("c", firstAliasException.ParamName);
                Assert.Equal("c", secondAliasException.ParamName);
                Assert.Equal("b", returnedMatException.ParamName);
            }
        }

        private static void AssertClose(double[] expected, double[] actual, double tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.True(Math.Abs(expected[i] - actual[i]) <= tolerance, "Mismatch at " + i + ": expected " + expected[i] + ", actual " + actual[i]);
            }
        }

        private static void AssertClose(float[] expected, float[] actual, float tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.True(Math.Abs(expected[i] - actual[i]) <= tolerance, "Mismatch at " + i + ": expected " + expected[i] + ", actual " + actual[i]);
            }
        }

    }
}
