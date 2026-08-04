using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Tests.Core
{
    public class CoreLinearAlgebraTests
    {
        [Fact]
        public void GemmAndMulTransposedWorkWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat a = new Mat(2, 2, MatType.CV_64FC1))
            using (Mat b = new Mat(2, 2, MatType.CV_64FC1))
            using (Mat c = new Mat(2, 2, MatType.CV_64FC1, new Scalar(1.0)))
            using (Mat gemm = new Mat())
            using (Mat mulTransposed = new Mat())
            {
                a.CopyFrom<double>(new double[] { 1.0, 2.0, 3.0, 4.0 });
                b.CopyFrom<double>(new double[] { 5.0, 6.0, 7.0, 8.0 });

                Cv2.Gemm(a, b, 1.0, c, 1.0, gemm);
                Cv2.MulTransposed(a, mulTransposed, aTa: false);

                Assert.Equal(new double[] { 20.0, 23.0, 44.0, 51.0 }, gemm.ToArray<double>());
                Assert.Equal(new double[] { 5.0, 11.0, 11.0, 25.0 }, mulTransposed.ToArray<double>());

                using (Mat returned = Cv2.Gemm(a, b))
                using (Mat returnedMulTransposed = Cv2.MulTransposed(a, aTa: false))
                {
                    Assert.Equal(new double[] { 19.0, 22.0, 43.0, 50.0 }, returned.ToArray<double>());
                    Assert.Equal(mulTransposed.ToArray<double>(), returnedMulTransposed.ToArray<double>());
                }
            }
        }

        [Fact]
        public void TransformAndPerspectiveTransformWorkWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat points = new Mat(1, 2, MatType.CV_32FC2))
            using (Mat affine = new Mat(2, 3, MatType.CV_32FC1))
            using (Mat perspective = new Mat(3, 3, MatType.CV_32FC1))
            using (Mat transformed = new Mat())
            using (Mat perspectiveResult = new Mat())
            {
                points.CopyFrom<float>(new float[] { 1.0f, 2.0f, 3.0f, 4.0f });
                affine.CopyFrom<float>(new float[] { 1.0f, 0.0f, 10.0f, 0.0f, 1.0f, 20.0f });
                perspective.CopyFrom<float>(new float[]
                {
                    1.0f, 0.0f, 10.0f,
                    0.0f, 1.0f, 20.0f,
                    0.0f, 0.0f, 1.0f
                });

                Cv2.Transform(points, transformed, affine);
                Cv2.PerspectiveTransform(points, perspectiveResult, perspective);

                Assert.Equal(new float[] { 11.0f, 22.0f, 13.0f, 24.0f }, transformed.ToArray<float>());
                Assert.Equal(new float[] { 11.0f, 22.0f, 13.0f, 24.0f }, perspectiveResult.ToArray<float>());

                using (Mat returnedTransformed = Cv2.Transform(points, affine))
                using (Mat returnedPerspective = Cv2.PerspectiveTransform(points, perspective))
                {
                    Assert.Equal(new float[] { 11.0f, 22.0f, 13.0f, 24.0f }, returnedTransformed.ToArray<float>());
                    Assert.Equal(new float[] { 11.0f, 22.0f, 13.0f, 24.0f }, returnedPerspective.ToArray<float>());
                }
            }
        }

        [Fact]
        public void EigenAndPolynomialSolversWorkWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat symmetric = new Mat(2, 2, MatType.CV_64FC1))
            using (Mat eigenvalues = new Mat())
            using (Mat eigenvectors = new Mat())
            using (Mat nonsymmetric = new Mat(2, 2, MatType.CV_64FC1))
            using (Mat nonSymValues = new Mat())
            using (Mat nonSymVectors = new Mat())
            using (Mat cubicCoeffs = new Mat(4, 1, MatType.CV_64FC1))
            using (Mat cubicRoots = new Mat())
            using (Mat polyCoeffs = new Mat(3, 1, MatType.CV_64FC1))
            using (Mat polyRoots = new Mat())
            {
                symmetric.CopyFrom<double>(new double[] { 2.0, 0.0, 0.0, 3.0 });
                nonsymmetric.CopyFrom<double>(new double[] { 0.0, 1.0, -2.0, -3.0 });
                cubicCoeffs.CopyFrom<double>(new double[] { 1.0, -6.0, 11.0, -6.0 });
                polyCoeffs.CopyFrom<double>(new double[] { 1.0, -3.0, 2.0 });

                bool success = Cv2.Eigen(symmetric, eigenvalues, eigenvectors);
                Cv2.EigenNonSymmetric(nonsymmetric, nonSymValues, nonSymVectors);
                int cubicCount = Cv2.SolveCubic(cubicCoeffs, cubicRoots);
                double error = Cv2.SolvePoly(polyCoeffs, polyRoots);

                Assert.True(success);
                Assert.Equal(new double[] { 3.0, 2.0 }, eigenvalues.ToArray<double>());
                Assert.False(nonSymValues.Empty);
                Assert.False(nonSymVectors.Empty);
                Assert.Equal(3, cubicCount);
                Assert.False(cubicRoots.Empty);
                Assert.True(error >= 0.0);
                Assert.False(polyRoots.Empty);
            }
        }

        [Fact]
        public void LinearAlgebraValidationRejectsNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => Cv2.Gemm(null!, null!, 1.0, null, 0.0, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Gemm(null!, null!));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Gemm(new Mat(), new Mat(), 1.0, null, 0.0, new Mat(), (GemmFlags)8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Gemm(new Mat(), new Mat(), flags: (GemmFlags)8));
            Assert.Throws<ArgumentNullException>(() => Cv2.MulTransposed(null!, aTa: false));
            Assert.Throws<ArgumentNullException>(() => Cv2.Transform(null!, null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Transform(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.PerspectiveTransform(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.Eigen(null!, null!, null!));
        }

        [Fact]
        public void GemmRejectsInvalidInputContract()
        {
            using (var src1 = new Mat(2, 3, MatType.CV_32FC1))
            using (var src2 = new Mat(3, 2, MatType.CV_32FC1))
            using (var unsupportedType = new Mat(2, 3, MatType.CV_8UC1))
            using (var typeMismatch = new Mat(3, 2, MatType.CV_64FC1))
            using (var shapeMismatch = new Mat(4, 2, MatType.CV_32FC1))
            using (var src3TypeMismatch = new Mat(2, 2, MatType.CV_64FC1))
            using (var src3ShapeMismatch = new Mat(1, 2, MatType.CV_32FC1))
            using (var transposedSrc2 = new Mat(3, 4, MatType.CV_32FC1))
            using (var transposedSrc3ShapeMismatch = new Mat(2, 4, MatType.CV_32FC1))
            using (var dst = new Mat())
            {
                ArgumentException unsupportedTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Gemm(unsupportedType, src2, 1.0, null, 0.0, dst));
                ArgumentException typeMismatchException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Gemm(src1, typeMismatch, 1.0, null, 0.0, dst));
                ArgumentException shapeMismatchException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Gemm(src1, shapeMismatch, 1.0, null, 0.0, dst));
                ArgumentException src3TypeMismatchException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Gemm(src1, src2, 1.0, src3TypeMismatch, 1.0, dst));
                ArgumentException src3ShapeMismatchException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Gemm(src1, src2, 1.0, src3ShapeMismatch, 1.0, dst));
                ArgumentException transposedSrc3ShapeMismatchException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Gemm(src1, transposedSrc2, 1.0, transposedSrc3ShapeMismatch, 1.0, dst, GemmFlags.TransposeSrc3));
                ArgumentException returnedShapeMismatchException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Gemm(src1, shapeMismatch));

                Assert.Equal("src1", unsupportedTypeException.ParamName);
                Assert.Equal("src2", typeMismatchException.ParamName);
                Assert.Equal("src2", shapeMismatchException.ParamName);
                Assert.Equal("src3", src3TypeMismatchException.ParamName);
                Assert.Equal("src3", src3ShapeMismatchException.ParamName);
                Assert.Equal("src3", transposedSrc3ShapeMismatchException.ParamName);
                Assert.Equal("src2", returnedShapeMismatchException.ParamName);
            }
        }

        [Fact]
        public void EigenRejectsInvalidSourceContract()
        {
            using (var unsupportedType = new Mat(2, 2, MatType.CV_8UC1))
            using (var multiChannel = new Mat(2, 2, MatType.CV_32FC2))
            using (var rectangular = new Mat(2, 3, MatType.CV_64FC1))
            using (var eigenvalues = new Mat())
            using (var eigenvectors = new Mat())
            {
                ArgumentException eigenUnsupportedTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Eigen(unsupportedType, eigenvalues, eigenvectors));
                ArgumentException eigenMultiChannelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Eigen(multiChannel, eigenvalues, eigenvectors));
                ArgumentException eigenRectangularException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Eigen(rectangular, eigenvalues, eigenvectors));
                ArgumentException nonSymUnsupportedTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.EigenNonSymmetric(unsupportedType, eigenvalues, eigenvectors));
                ArgumentException nonSymMultiChannelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.EigenNonSymmetric(multiChannel, eigenvalues, eigenvectors));
                ArgumentException nonSymRectangularException = Assert.Throws<ArgumentException>(() =>
                    Cv2.EigenNonSymmetric(rectangular, eigenvalues, eigenvectors));

                Assert.Equal("src", eigenUnsupportedTypeException.ParamName);
                Assert.Equal("src", eigenMultiChannelException.ParamName);
                Assert.Equal("src", eigenRectangularException.ParamName);
                Assert.Equal("src", nonSymUnsupportedTypeException.ParamName);
                Assert.Equal("src", nonSymMultiChannelException.ParamName);
                Assert.Equal("src", nonSymRectangularException.ParamName);
            }
        }

        [Fact]
        public void SolveCubicRejectsInvalidCoefficientContract()
        {
            using (var unsupportedType = new Mat(4, 1, MatType.CV_8UC1))
            using (var multiChannel = new Mat(4, 1, MatType.CV_32FC2))
            using (var invalidShape = new Mat(2, 2, MatType.CV_64FC1))
            using (var dst = new Mat())
            {
                ArgumentException unsupportedTypeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.SolveCubic(unsupportedType, dst));
                ArgumentException multiChannelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.SolveCubic(multiChannel, dst));
                ArgumentException invalidShapeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.SolveCubic(invalidShape, dst));

                Assert.Equal("coeffs", unsupportedTypeException.ParamName);
                Assert.Equal("coeffs", multiChannelException.ParamName);
                Assert.Equal("coeffs", invalidShapeException.ParamName);
            }
        }

        [Fact]
        public void SolvePolyRejectsInvalidCoefficientContract()
        {
            using (var unsupportedDepth = new Mat(3, 1, MatType.CV_8UC1))
            using (var tooManyChannels = new Mat(3, 1, MatType.CV_32FC3))
            using (var invalidShape = new Mat(2, 2, MatType.CV_64FC1))
            using (var dst = new Mat())
            {
                ArgumentException unsupportedDepthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.SolvePoly(unsupportedDepth, dst));
                ArgumentException tooManyChannelsException = Assert.Throws<ArgumentException>(() =>
                    Cv2.SolvePoly(tooManyChannels, dst));
                ArgumentException invalidShapeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.SolvePoly(invalidShape, dst));

                Assert.Equal("coeffs", unsupportedDepthException.ParamName);
                Assert.Equal("coeffs", tooManyChannelsException.ParamName);
                Assert.Equal("coeffs", invalidShapeException.ParamName);
            }
        }

        [Fact]
        public void GemmAllowsTransposedInputsAndAddendWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var src1 = new Mat(2, 3, MatType.CV_64FC1))
            using (var src2 = new Mat(2, 4, MatType.CV_64FC1))
            using (var src3 = new Mat(4, 3, MatType.CV_64FC1, new Scalar(1.0)))
            using (var dst = new Mat())
            {
                src1.CopyFrom<double>(new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 });
                src2.CopyFrom<double>(new[] { 1.0, 0.0, 2.0, 1.0, 0.0, 1.0, 3.0, 2.0 });

                Cv2.Gemm(src1, src2, 1.0, src3, 1.0, dst, GemmFlags.TransposeSrc1 | GemmFlags.TransposeSrc3);

                Assert.Equal(3, dst.Rows);
                Assert.Equal(4, dst.Cols);
                Assert.Equal(
                    new[] { 2.0, 5.0, 15.0, 10.0, 3.0, 6.0, 20.0, 13.0, 4.0, 7.0, 25.0, 16.0 },
                    dst.ToArray<double>());
            }
        }

        [Fact]
        public void MulTransposedRejectsInvalidInputContract()
        {
            using (var src = new Mat(2, 3, MatType.CV_64FC1))
            using (var multiChannelSource = new Mat(2, 3, MatType.CV_64FC2))
            using (var multiChannelDelta = new Mat(2, 3, MatType.CV_64FC2))
            using (var rowMismatchDelta = new Mat(4, 3, MatType.CV_64FC1))
            using (var colMismatchDelta = new Mat(2, 4, MatType.CV_64FC1))
            using (var dst = new Mat())
            {
                ArgumentException sourceException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MulTransposed(multiChannelSource, dst, aTa: false));
                ArgumentException deltaChannelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MulTransposed(src, dst, aTa: false, delta: multiChannelDelta));
                ArgumentException deltaRowException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MulTransposed(src, dst, aTa: false, delta: rowMismatchDelta));
                ArgumentException deltaColException = Assert.Throws<ArgumentException>(() =>
                    Cv2.MulTransposed(src, dst, aTa: false, delta: colMismatchDelta));

                Assert.Equal("src", sourceException.ParamName);
                Assert.Equal("delta", deltaChannelException.ParamName);
                Assert.Equal("delta", deltaRowException.ParamName);
                Assert.Equal("delta", deltaColException.ParamName);
            }
        }

        [Fact]
        public void MulTransposedAllowsValidDeltaBroadcastShapesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var src = new Mat(2, 3, MatType.CV_64FC1))
            using (var rowDelta = new Mat(1, 3, MatType.CV_64FC1))
            using (var colDelta = new Mat(2, 1, MatType.CV_64FC1))
            using (var rowResult = new Mat())
            using (var colResult = new Mat())
            {
                src.CopyFrom<double>(new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 });
                rowDelta.CopyFrom<double>(new[] { 1.0, 1.0, 1.0 });
                colDelta.CopyFrom<double>(new[] { 1.0, 4.0 });

                Cv2.MulTransposed(src, rowResult, aTa: false, delta: rowDelta);
                using (Mat returned = Cv2.MulTransposed(src, aTa: false, delta: colDelta))
                {
                    Assert.Equal(2, rowResult.Rows);
                    Assert.Equal(2, rowResult.Cols);
                    Assert.Equal(MatType.CV_64FC1, rowResult.Type);
                    Assert.Equal(2, returned.Rows);
                    Assert.Equal(2, returned.Cols);
                    Assert.Equal(MatType.CV_64FC1, returned.Type);
                }
            }
        }

        [Fact]
        public void TransformRejectsInvalidMatrixColumnContract()
        {
            using (var points = new Mat(1, 2, MatType.CV_32FC2))
            using (var invalidMatrix = new Mat(2, 4, MatType.CV_32FC1))
            using (var dst = new Mat())
            {
                ArgumentException voidException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Transform(points, dst, invalidMatrix));
                ArgumentException returnedException = Assert.Throws<ArgumentException>(() =>
                    Cv2.Transform(points, invalidMatrix));

                Assert.Equal("m", voidException.ParamName);
                Assert.Equal("m", returnedException.ParamName);
            }
        }

        [Fact]
        public void PerspectiveTransformRejectsInvalidSourceAndMatrixContract()
        {
            using (var points = new Mat(1, 2, MatType.CV_32FC2))
            using (var oneChannelPoints = new Mat(1, 2, MatType.CV_32FC1))
            using (var fourChannelPoints = new Mat(1, 2, MatType.CV_32FC4))
            using (var unsupportedDepth = new Mat(1, 2, MatType.CV_8UC2))
            using (var invalidMatrix = new Mat(3, 2, MatType.CV_32FC1))
            using (var invalidRowMatrix = new Mat(4, 3, MatType.CV_32FC1))
            using (var validMatrix = new Mat(3, 3, MatType.CV_32FC1))
            using (var dst = new Mat())
            {
                ArgumentException matrixException = Assert.Throws<ArgumentException>(() =>
                    Cv2.PerspectiveTransform(points, dst, invalidMatrix));
                ArgumentException returnedMatrixException = Assert.Throws<ArgumentException>(() =>
                    Cv2.PerspectiveTransform(points, invalidMatrix));
                ArgumentException matrixRowsException = Assert.Throws<ArgumentException>(() =>
                    Cv2.PerspectiveTransform(points, dst, invalidRowMatrix));
                ArgumentException oneChannelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.PerspectiveTransform(oneChannelPoints, dst, validMatrix));
                ArgumentException fourChannelException = Assert.Throws<ArgumentException>(() =>
                    Cv2.PerspectiveTransform(fourChannelPoints, dst, validMatrix));
                ArgumentException depthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.PerspectiveTransform(unsupportedDepth, dst, validMatrix));

                Assert.Equal("m", matrixException.ParamName);
                Assert.Equal("m", returnedMatrixException.ParamName);
                Assert.Equal("m", matrixRowsException.ParamName);
                Assert.Equal("src", oneChannelException.ParamName);
                Assert.Equal("src", fourChannelException.ParamName);
                Assert.Equal("src", depthException.ParamName);
            }
        }

    }
}
