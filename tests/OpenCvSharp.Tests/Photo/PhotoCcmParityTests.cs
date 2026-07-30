using System;
using OpenCvSharp;
using OpenCvSharp.Core;
using OpenCvSharp.Photo;

namespace OpenCvSharp.Tests.Photo
{
    public sealed class PhotoCcmParityTests
    {
        private static readonly double[] MacbethMeasured =
        {
            214.11, 98.67, 37.97,
            231.94, 153.1, 85.27,
            204.08, 143.71, 78.46,
            190.58, 122.99, 30.84,
            230.93, 148.46, 100.84,
            228.64, 206.97, 97.5,
            229.09, 137.07, 55.29,
            189.21, 111.22, 92.66,
            223.5, 96.42, 75.45,
            201.82, 69.71, 50.9,
            240.52, 196.47, 59.3,
            235.73, 172.13, 54.0,
            131.6, 75.04, 68.86,
            189.04, 170.43, 42.05,
            222.23, 74.0, 71.95,
            241.01, 199.1, 61.15,
            224.99, 101.4, 100.24,
            174.58, 152.63, 91.52,
            248.06, 227.69, 140.5,
            241.15, 201.38, 115.58,
            236.49, 175.87, 88.86,
            212.19, 133.49, 54.79,
            181.17, 102.94, 36.18,
            115.1, 53.77, 15.23,
        };

        private static readonly double[] ExpectedLinearCcm =
        {
            0.37406520, 0.02066507, 0.05804047,
            0.12719672, 0.77389268, -0.01569404,
            -0.27627010, 0.00603427, 2.74272981,
        };

        [Fact]
        public void GammaCorrectionSupportsDocumentedDepthsAndOverloads()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var source = new Mat(1, 3, MatType.CV_64FC1))
            using (var callerOwned = new Mat())
            {
                source.CopyFrom(new[] { 0.25, 0.5, 1.0 });
                PhotoCv2.GammaCorrection(source, callerOwned, 2.0);
                using Mat returned = PhotoCv2.GammaCorrection(source, 2.0);
                Assert.Equal(source.Size, callerOwned.Size);
                Assert.Equal(source.Type, callerOwned.Type);
                AssertNear(new[] { 0.0625, 0.25, 1.0 }, callerOwned.ToArray<double>(), 1.0e-12);
                AssertNear(callerOwned.ToArray<double>(), returned.ToArray<double>(), 0.0);
            }

            AssertGammaDepth(MatType.CV_8UC3, new Scalar(128, 64, 32));
            AssertGammaDepth(MatType.CV_16UC1, new Scalar(32768));
            AssertGammaDepth(MatType.CV_16SC1, new Scalar(16384));
            AssertGammaDepth(MatType.CV_32FC4, new Scalar(0.25, 0.5, 0.75, 1.0));

            using var invalidDepth = new Mat(2, 2, MatType.CV_32SC1);
            using var valid = new Mat(2, 2, MatType.CV_32FC1, new Scalar(0.5));
            using var output = new Mat();
            Assert.Throws<ArgumentException>(() => PhotoCv2.GammaCorrection(invalidDepth, output, 2.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => PhotoCv2.GammaCorrection(valid, output, 0.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => PhotoCv2.GammaCorrection(valid, output, double.NaN));
        }

        [Fact]
        public void MacbethModelMatchesUpstreamAndReturnsIndependentMatrices()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            Mat samples = CreateMacbethSamples();
            ColorCorrectionModel model = PhotoCv2.CreateColorCorrectionModel(samples, ColorCheckerType.Macbeth);
            samples.Dispose();
            try
            {
                using (var callerOwned = new Mat())
                {
                    model.Compute(callerOwned);
                    Assert.Equal(3, callerOwned.Rows);
                    Assert.Equal(3, callerOwned.Cols);
                    Assert.Equal(MatType.CV_64FC1, callerOwned.Type);
                    AssertNear(ExpectedLinearCcm, callerOwned.ToArray<double>(), 1.0e-4);
                }

                using Mat computedAgain = model.Compute();
                using Mat matrixA = model.GetColorCorrectionMatrix();
                using Mat matrixB = model.GetColorCorrectionMatrix();
                using Mat srcLinear = model.GetSrcLinearRGB();
                using Mat refLinear = model.GetRefLinearRGB();
                using Mat mask = model.GetMask();
                using Mat weights = model.GetWeights();
                AssertNear(ExpectedLinearCcm, computedAgain.ToArray<double>(), 1.0e-4);
                AssertNear(ExpectedLinearCcm, matrixA.ToArray<double>(), 1.0e-4);
                Assert.Equal(new Size(1, 24), srcLinear.Size);
                Assert.Equal(MatType.CV_64FC3, srcLinear.Type);
                Assert.Equal(new Size(1, 24), refLinear.Size);
                Assert.Equal(MatType.CV_64FC3, refLinear.Type);
                Assert.Equal(new Size(1, 24), mask.Size);
                Assert.Equal(MatType.CV_8UC1, mask.Type);
                Assert.All(mask.ToArray<byte>(), value => Assert.Equal((byte)1, value));
                Assert.True(weights.Empty);
                Assert.True(model.GetLoss() >= 0.0);
                Assert.False(double.IsNaN(model.GetLoss()));

                matrixA.SetTo(new Scalar(0.0));
                AssertNear(ExpectedLinearCcm, matrixB.ToArray<double>(), 1.0e-4);
            }
            finally
            {
                model.Dispose();
                model.Dispose();
            }

            Assert.True(model.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => model.GetLoss());
        }

        [Fact]
        public void ConfigurationClonesInputsSupportsCustomLabAndNonContiguousSamples()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var source = CreateMacbethSamples())
            using (var parent = new Mat(24, 2, MatType.CV_64FC3))
            using (var roi = parent.Col(0))
            {
                source.CopyTo(roi);
                using var model = PhotoCv2.CreateColorCorrectionModel(roi, ColorCheckerType.Macbeth);
                using var weights = new Mat(24, 1, MatType.CV_64FC1, new Scalar(1.0));
                model.SetColorSpace(ColorSpace.AdobeRgb);
                model.SetCcmType(CcmType.Linear);
                model.SetDistance(DistanceType.RgbLinear);
                model.SetLinearization(LinearizationType.Identity);
                model.SetLinearizationGamma(2.1);
                model.SetLinearizationDegree(2);
                model.SetSaturatedThreshold(0.0, 0.99);
                model.SetWeightsList(weights);
                model.SetWeightCoeff(1.5);
                model.SetInitialMethod(InitialMethodType.WhiteBalance);
                model.SetMaxCount(20);
                model.SetEpsilon(0.01);
                model.SetRGB(false);
                weights.Dispose();

                using Mat weighted = model.Compute();
                Assert.Equal(3, weighted.Rows);
                Assert.Equal(3, weighted.Cols);
                Assert.Equal(MatType.CV_64FC1, weighted.Type);
                using Mat normalizedWeights = model.GetWeights();
                Assert.Equal(24, normalizedWeights.Rows);
                Assert.All(normalizedWeights.ToArray<double>(), value => Assert.InRange(value, 0.999999, 1.000001));

                model.SetEpsilon(0.02);
                Assert.Throws<InvalidOperationException>(() => model.GetColorCorrectionMatrix());
                using Mat recomputed = model.Compute();
                Assert.Equal(3, recomputed.Rows);
                Assert.Equal(3, recomputed.Cols);
                model.SetRGB(true);
                using Mat stillReady = model.GetColorCorrectionMatrix();
                Assert.Equal(recomputed.Size, stillReady.Size);

                using var affineModel = PhotoCv2.CreateColorCorrectionModel(roi, ColorCheckerType.Macbeth);
                affineModel.SetCcmType(CcmType.Affine);
                affineModel.SetDistance(DistanceType.RgbLinear);
                affineModel.SetLinearization(LinearizationType.Identity);
                affineModel.SetInitialMethod(InitialMethodType.WhiteBalance);
                affineModel.SetMaxCount(20);
                affineModel.SetEpsilon(0.02);
                using Mat affine = affineModel.Compute();
                Assert.Equal(4, affine.Rows);
                Assert.Equal(3, affine.Cols);
                Assert.Equal(MatType.CV_64FC1, affine.Type);
            }

            using (var source = CreateMacbethSamples())
            using (var labReference = new Mat(24, 1, MatType.CV_64FC3, new Scalar(50.0, -10.0, 20.0)))
            using (var coloredMask = new Mat(24, 1, MatType.CV_8UC1, new Scalar(1.0)))
            using (ColorCorrectionModel custom = ColorCorrectionModel.Create(
                source, labReference, ColorSpace.LabD50TwoDegree, coloredMask))
            {
                Assert.False(custom.IsDisposed);
            }
        }

        [Fact]
        public void CorrectionPreservesTypeSupportsAliasingAndRejectsInvalidState()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using var samples = CreateMacbethSamples();
            using var model = ColorCorrectionModel.Create(samples, ColorCheckerType.Macbeth);
            using Mat ccm = model.Compute();
            AssertCorrectedType(model, MatType.CV_8UC3, new Scalar(32, 96, 192));
            AssertCorrectedType(model, MatType.CV_16UC3, new Scalar(8192, 32768, 49152));
            AssertCorrectedType(model, MatType.CV_32FC3, new Scalar(0.2, 0.5, 0.8));

            using (var inPlace = new Mat(3, 4, MatType.CV_8UC3, new Scalar(32, 64, 96)))
            {
                model.CorrectImage(inPlace, inPlace);
                Assert.Equal(MatType.CV_8UC3, inPlace.Type);
                Assert.Equal(new Size(4, 3), inPlace.Size);
            }

            using (var image = new Mat(2, 2, MatType.CV_32FC3, new Scalar(0.25, 0.5, 0.75)))
            using (Mat nonlinear = model.CorrectImage(image))
            using (Mat requestedLinear = model.CorrectImage(image, isLinear: true))
            {
                AssertNear(nonlinear.ToArray<float>(), requestedLinear.ToArray<float>(), 0.0F);
            }

            using var unready = ColorCorrectionModel.Create();
            using var empty = new Mat();
            using var output = new Mat();
            Assert.Throws<InvalidOperationException>(() => unready.Compute(output));
            Assert.Throws<InvalidOperationException>(() => unready.GetLoss());
            Assert.Throws<InvalidOperationException>(() => unready.CorrectImage(empty, output));
        }

        [Fact]
        public void PersistenceRoundTripRetainsReadyStateAndRejectsInvalidNodesAndArguments()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            string document;
            using (var samples = CreateMacbethSamples())
            using (var model = ColorCorrectionModel.Create(samples, ColorCheckerType.Macbeth))
            using (Mat ccm = model.Compute())
            using (var writer = new FileStorage(
                "memory.yml",
                FileStorageModes.Write | FileStorageModes.Memory | FileStorageModes.FormatYaml))
            {
                model.Write(writer);
                document = writer.ReleaseAndGetString();
            }

            Assert.Contains("ColorCorrectionModel", document);
            var reader = new FileStorage(
                document,
                FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml);
            FileNode node = reader["ColorCorrectionModel"];
            reader.Dispose();
            using (node)
            using (var loaded = ColorCorrectionModel.Create())
            {
                loaded.Read(node);
                loaded.SetRGB(false);
                using Mat matrix = loaded.GetColorCorrectionMatrix();
                AssertNear(ExpectedLinearCcm, matrix.ToArray<double>(), 1.0e-4);
                Assert.True(loaded.GetLoss() >= 0.0);

                using var writer = new FileStorage(
                    "memory.yml",
                    FileStorageModes.Write | FileStorageModes.Memory | FileStorageModes.FormatYaml);
                loaded.Write(writer);
                Assert.Equal(document, writer.ReleaseAndGetString());
            }

            using (var staleReader = new FileStorage(
                document,
                FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml))
            using (FileNode stale = staleReader["ColorCorrectionModel"])
            using (var unloaded = ColorCorrectionModel.Create())
            {
                staleReader.Release();
                OpenCvException exception = Assert.Throws<OpenCvException>(() => unloaded.Read(stale));
                Assert.Contains("invalidated", exception.Message, StringComparison.OrdinalIgnoreCase);
            }

            const string malformed = "%YAML:1.0\n---\nColorCorrectionModel:\n   ccm: 7\n";
            using (var malformedReader = new FileStorage(
                malformed,
                FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml))
            using (FileNode malformedNode = malformedReader["ColorCorrectionModel"])
            using (FileNode missingNode = malformedReader["missing"])
            using (var unloaded = ColorCorrectionModel.Create())
            {
                Assert.Throws<OpenCvException>(() => unloaded.Read(malformedNode));
                Assert.Throws<OpenCvException>(() => unloaded.Read(missingNode));
            }

            AssertInvalidManagedArguments();
        }

        private static void AssertInvalidManagedArguments()
        {
            using var samples = CreateMacbethSamples();
            using var wrongType = new Mat(24, 1, MatType.CV_32FC3);
            using var wrongCount = new Mat(23, 1, MatType.CV_64FC3, new Scalar(0.5, 0.5, 0.5));
            using var reference = samples.Clone();
            using var badMask = new Mat(24, 1, MatType.CV_8UC1, new Scalar(2.0));
            using var badWeights = new Mat(23, 1, MatType.CV_64FC1, new Scalar(1.0));
            using var nonFiniteWeights = new Mat(24, 1, MatType.CV_64FC1, new Scalar(1.0));
            nonFiniteWeights.CopyFrom(CreateValues(24, double.NaN));

            Assert.Throws<ArgumentException>(() => ColorCorrectionModel.Create(wrongType, ColorCheckerType.Macbeth));
            Assert.Throws<ArgumentException>(() => ColorCorrectionModel.Create(wrongCount, ColorCheckerType.Macbeth));
            Assert.Throws<ArgumentOutOfRangeException>(() => ColorCorrectionModel.Create(samples, (ColorCheckerType)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => ColorCorrectionModel.Create(samples, reference, (ColorSpace)99));
            Assert.Throws<ArgumentException>(() => ColorCorrectionModel.Create(samples, reference, ColorSpace.Srgb, badMask));

            using var model = ColorCorrectionModel.Create(samples, ColorCheckerType.Macbeth);
            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetColorSpace(ColorSpace.SrgbLinear));
            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetCcmType((CcmType)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetDistance((DistanceType)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetLinearization((LinearizationType)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetLinearizationGamma(double.PositiveInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetLinearizationDegree(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetSaturatedThreshold(0.9, 0.5));
            Assert.Throws<ArgumentException>(() => model.SetWeightsList(badWeights));
            Assert.Throws<ArgumentException>(() => model.SetWeightsList(nonFiniteWeights));
            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetWeightCoeff(double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetInitialMethod((InitialMethodType)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetMaxCount(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetEpsilon(0.0));

            var disposed = ColorCorrectionModel.Create();
            disposed.Dispose();
            disposed.Dispose();
            Assert.Throws<ObjectDisposedException>(() => disposed.SetRGB(true));
        }

        private static Mat CreateMacbethSamples()
        {
            var result = new Mat(24, 1, MatType.CV_64FC3);
            var values = new double[MacbethMeasured.Length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = MacbethMeasured[i] / 255.0;
            }

            result.CopyFrom(values);
            return result;
        }

        private static double[] CreateValues(int count, double value)
        {
            var result = new double[count];
            for (int i = 0; i < result.Length; i++) result[i] = value;
            return result;
        }

        private static void AssertGammaDepth(int type, Scalar value)
        {
            using var src = new Mat(2, 3, type, value);
            using Mat dst = PhotoCv2.GammaCorrection(src, 1.8);
            Assert.Equal(src.Size, dst.Size);
            Assert.Equal(src.Type, dst.Type);
        }

        private static void AssertCorrectedType(ColorCorrectionModel model, int type, Scalar value)
        {
            using var src = new Mat(3, 4, type, value);
            using var callerOwned = new Mat();
            model.CorrectImage(src, callerOwned);
            using Mat returned = model.CorrectImage(src);
            Assert.Equal(src.Size, callerOwned.Size);
            Assert.Equal(src.Type, callerOwned.Type);
            Assert.Equal(callerOwned.ToBytes(), returned.ToBytes());
        }

        private static void AssertNear(double[] expected, double[] actual, double tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.InRange(actual[i], expected[i] - tolerance, expected[i] + tolerance);
            }
        }

        private static void AssertNear(float[] expected, float[] actual, float tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.InRange(actual[i], expected[i] - tolerance, expected[i] + tolerance);
            }
        }
    }
}
