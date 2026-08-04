using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Photo;

namespace JYPPX.OpenCvSharp.Tests.Photo
{
    public sealed class PhotoFinalCallableTests
    {
        [Fact]
        public void Tvl1SupportsMultipleRoiObservationsAndStableOwnedOutput()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using var parent0 = new Mat(10, 12, MatType.CV_8UC1, new Scalar(80));
            using var parent1 = new Mat(10, 12, MatType.CV_8UC1, new Scalar(96));
            using var observation0 = parent0.SubMat(new Rect(2, 1, 8, 8));
            using var observation1 = parent1.SubMat(new Rect(1, 0, 8, 8));
            using var output = new Mat();
            PhotoCv2.DenoiseTvl1(new[] { observation0, observation1 }, output, lambda: 1.0, niters: 2);
            Assert.Equal(new Size(8, 8), output.Size);
            Assert.Equal(MatType.CV_8UC1, output.Type);

            byte[] first = output.ToArray<byte>();
            using Mat repeated = PhotoCv2.DenoiseTvl1(new[] { observation0, observation1 }, lambda: 1.0, niters: 2);
            Assert.Equal(first, repeated.ToArray<byte>());

            using var alias = new Mat(1, 1, MatType.CV_8UC1, new Scalar(48));
            PhotoCv2.DenoiseTvl1(new[] { alias }, alias, lambda: 1.0, niters: 1);
            Assert.Equal(new Size(1, 1), alias.Size);
            Assert.Equal(MatType.CV_8UC1, alias.Type);
        }

        [Fact]
        public void Tvl1RejectsInvalidCollectionsAndParameters()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using var valid = new Mat(4, 4, MatType.CV_8UC1, new Scalar(64));
            using var mismatch = new Mat(5, 4, MatType.CV_8UC1);
            using var badType = new Mat(4, 4, MatType.CV_16UC1);
            using var output = new Mat();
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.DenoiseTvl1(null!, output));
            Assert.Throws<ArgumentException>(() => PhotoCv2.DenoiseTvl1(Array.Empty<Mat>(), output));
            Assert.Throws<ArgumentException>(() => PhotoCv2.DenoiseTvl1(new[] { valid, mismatch }, output));
            Assert.Throws<ArgumentException>(() => PhotoCv2.DenoiseTvl1(new[] { badType }, output));
            Assert.Throws<ArgumentOutOfRangeException>(() => PhotoCv2.DenoiseTvl1(new[] { valid }, output, 0.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => PhotoCv2.DenoiseTvl1(new[] { valid }, output, 1.0, 0));
        }

        [Fact]
        public void ChromaticCorrectionPreservesIdentityModelAndValidatesShape()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using var parent = new Mat(6, 6, MatType.CV_8UC3, new Scalar(20, 40, 80));
            using var input = parent.SubMat(new Rect(1, 1, 4, 4));
            using var inputCopy = input.Clone();
            using var coefficients = new Mat(4, 1, MatType.CV_32FC1, new Scalar(0));
            using var output = new Mat();
            float[] before = coefficients.ToArray<float>();
            PhotoCv2.CorrectChromaticAberration(input, coefficients, output, new Size(4, 4), 0);
            Assert.Equal(input.Size, output.Size);
            Assert.Equal(input.Type, output.Type);
            Assert.Equal(inputCopy.ToArray<byte>(), output.ToArray<byte>());
            Assert.Equal(before, coefficients.ToArray<float>());

            using var alias = input.Clone();
            PhotoCv2.CorrectChromaticAberration(alias, coefficients, alias, new Size(4, 4), 0);
            Assert.Equal(inputCopy.ToArray<byte>(), alias.ToArray<byte>());

            using var raw = new Mat(8, 8, MatType.CV_8UC1, new Scalar(32));
            using Mat rawCorrected = PhotoCv2.CorrectChromaticAberration(
                raw,
                coefficients,
                new Size(8, 8),
                0,
                (int)ColorConversionCodes.BayerBG2BGR);
            Assert.Equal(new Size(8, 8), rawCorrected.Size);
            Assert.Equal(MatType.CV_8UC3, rawCorrected.Type);

            using var gray = new Mat(4, 4, MatType.CV_8UC1, new Scalar(32));
            using var wrongType = new Mat(4, 1, MatType.CV_64FC1);
            Assert.Throws<ArgumentOutOfRangeException>(() => PhotoCv2.CorrectChromaticAberration(gray, coefficients, output, new Size(4, 4), 0));
            Assert.Throws<OpenCvException>(() => PhotoCv2.CorrectChromaticAberration(gray, coefficients, output, new Size(4, 4), 0, int.MaxValue));
            Assert.Throws<ArgumentException>(() => PhotoCv2.CorrectChromaticAberration(input, wrongType, output, new Size(4, 4), 0));
            Assert.Throws<ArgumentException>(() => PhotoCv2.CorrectChromaticAberration(input, coefficients, output, new Size(3, 4), 0));
            Assert.Throws<ArgumentException>(() => PhotoCv2.CorrectChromaticAberration(input, coefficients, output, new Size(4, 4), 1));
        }

        [Fact]
        public void ChromaticParametersLoadWithIndependentLifetimeAndRowOrdering()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            const string document = "%YAML:1.0\n" +
                "image_width: 4\n" +
                "image_height: 4\n" +
                "red_channel:\n" +
                "  coeffs_x: [10., 11., 12.]\n" +
                "  coeffs_y: [13., 14., 15.]\n" +
                "blue_channel:\n" +
                "  coeffs_x: [20., 21., 22.]\n" +
                "  coeffs_y: [23., 24., 25.]\n";

            ChromaticAberrationParameters parameters;
            using (var storage = new FileStorage(document, FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml))
            using (FileNode root = storage.Root())
            {
                parameters = PhotoCv2.LoadChromaticAberrationParams(root);
            }

            try
            {
                Assert.Equal(new Size(4, 4), parameters.CalibrationSize);
                Assert.Equal(1, parameters.Degree);
                Assert.Equal(4, parameters.Coefficients.Rows);
                Assert.Equal(3, parameters.Coefficients.Cols);
                Assert.Equal(MatType.CV_32FC1, parameters.Coefficients.Type);
                Assert.Equal(
                    new[] { 20F, 21F, 22F, 23F, 24F, 25F, 10F, 11F, 12F, 13F, 14F, 15F },
                    parameters.Coefficients.ToArray<float>());
            }
            finally
            {
                parameters.Dispose();
                parameters.Dispose();
            }

            Assert.True(parameters.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => _ = parameters.Coefficients);
        }

        [Fact]
        public void ChromaticParametersRejectMalformedSchema()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            const string malformed = "%YAML:1.0\nimage_width: 4\nimage_height: 4\nred_channel:\n  coeffs_x: [1., 2.]\n  coeffs_y: [1.]\nblue_channel:\n  coeffs_x: [1., 2.]\n  coeffs_y: [1., 2.]\n";
            using var storage = new FileStorage(malformed, FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml);
            using FileNode root = storage.Root();
            using var coefficients = new Mat();
            Assert.Throws<OpenCvException>(() => PhotoCv2.LoadChromaticAberrationParams(root, coefficients, out _, out _));
        }
    }
}
