using System;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class HomogeneousEpipolarUtilityTests
    {
        [Fact]
        public void HomogeneousEpipolarUtilitiesValidateInputs()
        {
            using (var destination = new Mat())
            using (Mat points2 = CreatePointMat(
                new Point2f(1.0F, 2.0F),
                new Point2f(3.0F, 4.0F)))
            using (Mat points3 = CreatePointMat(
                new Point3f(1.0F, 2.0F, 1.0F),
                new Point3f(3.0F, 4.0F, 1.0F)))
            using (Mat fundamental = CreateFundamentalMatrix())
            using (var invalidComponents = new Mat(2, 4, MatType.CV_32FC1))
            using (var transposedScalarPoints = new Mat(3, 2, MatType.CV_32FC1))
            using (var invalidDepth = new Mat(2, 1, MatType.CV_8UC2))
            using (var invalidFundamentalShape = new Mat(2, 3, MatType.CV_64FC1))
            using (var invalidFundamentalChannels = new Mat(3, 3, MatType.CV_64FC2))
            using (var invalidFundamentalDepth = new Mat(3, 3, MatType.CV_32SC1))
            using (Mat onePoint = CreatePointMat(new Point2f(1.0F, 2.0F)))
            using (Mat doublePoints = CreatePoint2dMat(
                new Point2d(1.0, 2.0),
                new Point2d(3.0, 4.0)))
            using (Mat sampsonPoint = CreateHomogeneousPoint(1.0, 2.0))
            using (var invalidSampsonPointShape = new Mat(1, 3, MatType.CV_64FC1))
            using (var invalidSampsonPointDepth = new Mat(3, 1, MatType.CV_32FC1))
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ConvertPointsToHomogeneous((Mat)null!, destination));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ConvertPointsToHomogeneous(points2, null!));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ConvertPointsFromHomogeneous((Mat)null!, destination));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.ConvertPointsToHomogeneous(invalidComponents, destination));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.ConvertPointsFromHomogeneous(transposedScalarPoints, destination));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.ConvertPointsToHomogeneous(invalidDepth, destination));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.ConvertPointsToHomogeneous(points2, destination, MatType.CV_8U));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.ConvertPointsToHomogeneous(Array.Empty<Point2f>()));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.ConvertPointsFromHomogeneous(Array.Empty<Point3f>()));

#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.ConvertPointsToHomogeneous(ReadOnlySpan<Point2f>.Empty));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.ConvertPointsFromHomogeneous(ReadOnlySpan<Point3f>.Empty));
#endif

                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.CorrectMatches(
                        invalidFundamentalShape,
                        points2,
                        points2,
                        destination,
                        destination));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.CorrectMatches(
                        invalidFundamentalChannels,
                        points2,
                        points2,
                        destination,
                        destination));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.CorrectMatches(
                        invalidFundamentalDepth,
                        points2,
                        points2,
                        destination,
                        destination));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.CorrectMatches(
                        fundamental,
                        points2,
                        onePoint,
                        destination,
                        destination));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.CorrectMatches(
                        fundamental,
                        points2,
                        doublePoints,
                        destination,
                        destination));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.CorrectMatches(
                        fundamental,
                        Array.Empty<Point2f>(),
                        Array.Empty<Point2f>(),
                        out _,
                        out _));

#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.CorrectMatches(
                        fundamental,
                        ReadOnlySpan<Point2f>.Empty,
                        ReadOnlySpan<Point2f>.Empty,
                        out _,
                        out _));
#endif

                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SampsonDistance(
                        invalidSampsonPointShape,
                        sampsonPoint,
                        fundamental));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SampsonDistance(
                        invalidSampsonPointDepth,
                        sampsonPoint,
                        fundamental));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SampsonDistance(
                        sampsonPoint,
                        sampsonPoint,
                        invalidFundamentalDepth));
            }

            var disposed = new Mat(1, 1, MatType.CV_32FC2);
            disposed.Dispose();
            using (var destination = new Mat())
            {
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.ConvertPointsToHomogeneous(disposed, destination));
            }
        }

        [Fact]
        public void ConvertPointsToHomogeneousSupportsLayoutsDepthsAndOverloadsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point2f[] source =
            {
                new Point2f(-2.5F, 4.0F),
                new Point2f(0.0F, -3.25F),
                new Point2f(7.5F, 1.5F)
            };

            using (Mat sourceMat = CreatePointMat(source))
            using (var callerOwned = new Mat())
            using (Mat owned = Calib3DCv2.ConvertPointsToHomogeneous(sourceMat))
            using (Mat arrayResult = Calib3DCv2.ConvertPointsToHomogeneous(source))
            using (Mat doubleResult = Calib3DCv2.ConvertPointsToHomogeneous(sourceMat, MatType.CV_64F))
            using (Mat rowVector = CreatePointRowMat(source))
            using (Mat rowResult = Calib3DCv2.ConvertPointsToHomogeneous(rowVector))
            using (Mat scalarSource = CreateScalarPointMatrix(
                3,
                2,
                MatType.CV_64FC1,
                -2.5,
                4.0,
                0.0,
                -3.25,
                7.5,
                1.5))
            using (Mat scalarResult = Calib3DCv2.ConvertPointsToHomogeneous(scalarSource))
            using (Mat points3 = CreatePointMat(
                new Point3f(1.0F, 2.0F, 3.0F),
                new Point3f(-4.0F, 5.0F, -6.0F)))
            using (Mat points4 = Calib3DCv2.ConvertPointsToHomogeneous(points3))
#if NETCOREAPP3_1_OR_GREATER
            using (Mat spanResult = Calib3DCv2.ConvertPointsToHomogeneous(source.AsSpan()))
#endif
            {
                Calib3DCv2.ConvertPointsToHomogeneous(sourceMat, callerOwned);

                Assert.Equal(MatType.CV_32FC3, callerOwned.Type);
                Assert.Equal(source.Length, callerOwned.Rows);
                Assert.Equal(1, callerOwned.Cols);
                AssertHomogeneousPoints(source, callerOwned, 1.0e-6);
                AssertMatNear(callerOwned, owned, 1.0e-6);
                AssertMatNear(callerOwned, arrayResult, 1.0e-6);
                AssertMatNear(callerOwned, rowResult, 1.0e-6);
#if NETCOREAPP3_1_OR_GREATER
                AssertMatNear(callerOwned, spanResult, 1.0e-6);
#endif

                Assert.Equal(MatType.CV_64FC3, doubleResult.Type);
                for (int i = 0; i < source.Length; ++i)
                {
                    AssertNear(source[i].X, doubleResult.GetValue<double>((i * 3) + 0), 1.0e-12);
                    AssertNear(source[i].Y, doubleResult.GetValue<double>((i * 3) + 1), 1.0e-12);
                    AssertNear(1.0, doubleResult.GetValue<double>((i * 3) + 2), 1.0e-12);
                }

                Assert.Equal(MatType.CV_64FC3, scalarResult.Type);
                Assert.Equal(source.Length, scalarResult.Rows);
                for (int i = 0; i < source.Length; ++i)
                {
                    AssertNear(source[i].X, scalarResult.GetValue<double>((i * 3) + 0), 1.0e-12);
                    AssertNear(source[i].Y, scalarResult.GetValue<double>((i * 3) + 1), 1.0e-12);
                    AssertNear(1.0, scalarResult.GetValue<double>((i * 3) + 2), 1.0e-12);
                }

                Assert.Equal(MatType.CV_32FC4, points4.Type);
                Assert.Equal(new Vec4f(1.0F, 2.0F, 3.0F, 1.0F), points4.GetValue<Vec4f>(0));
                Assert.Equal(new Vec4f(-4.0F, 5.0F, -6.0F, 1.0F), points4.GetValue<Vec4f>(1));
            }
        }

        [Fact]
        public void ConvertPointsFromHomogeneousDividesAndPreservesZeroWCoordinatesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] source =
            {
                new Point3f(6.0F, -3.0F, 3.0F),
                new Point3f(-4.0F, 8.0F, -2.0F),
                new Point3f(5.0F, -7.0F, 0.0F)
            };

            using (Mat sourceMat = CreatePointMat(source))
            using (var callerOwned = new Mat())
            using (Mat owned = Calib3DCv2.ConvertPointsFromHomogeneous(sourceMat))
            using (Mat arrayResult = Calib3DCv2.ConvertPointsFromHomogeneous(source))
            using (Mat doubleResult = Calib3DCv2.ConvertPointsFromHomogeneous(sourceMat, MatType.CV_64F))
            using (Mat scalarSource = CreateScalarPointMatrix(
                3,
                3,
                MatType.CV_32FC1,
                6.0,
                -3.0,
                3.0,
                -4.0,
                8.0,
                -2.0,
                5.0,
                -7.0,
                0.0))
            using (Mat scalarResult = Calib3DCv2.ConvertPointsFromHomogeneous(scalarSource))
            using (var points4 = new Mat(2, 1, MatType.CV_32FC4))
#if NETCOREAPP3_1_OR_GREATER
            using (Mat spanResult = Calib3DCv2.ConvertPointsFromHomogeneous(source.AsSpan()))
#endif
            {
                points4.SetValue(0, new Vec4f(2.0F, 4.0F, 6.0F, 2.0F));
                points4.SetValue(1, new Vec4f(-3.0F, 6.0F, 9.0F, -3.0F));
                using (Mat points3 = Calib3DCv2.ConvertPointsFromHomogeneous(points4))
                {
                    Assert.Equal(MatType.CV_32FC3, points3.Type);
                    AssertPoint3Near(new Point3f(1.0F, 2.0F, 3.0F), points3.GetValue<Point3f>(0), 1.0e-6);
                    AssertPoint3Near(new Point3f(1.0F, -2.0F, -3.0F), points3.GetValue<Point3f>(1), 1.0e-6);
                }

                Calib3DCv2.ConvertPointsFromHomogeneous(sourceMat, callerOwned);

                Assert.Equal(MatType.CV_32FC2, callerOwned.Type);
                AssertPoint2Near(new Point2f(2.0F, -1.0F), callerOwned.GetValue<Point2f>(0), 1.0e-6);
                AssertPoint2Near(new Point2f(2.0F, -4.0F), callerOwned.GetValue<Point2f>(1), 1.0e-6);
                AssertPoint2Near(new Point2f(5.0F, -7.0F), callerOwned.GetValue<Point2f>(2), 1.0e-6);
                AssertMatNear(callerOwned, owned, 1.0e-6);
                AssertMatNear(callerOwned, arrayResult, 1.0e-6);
                AssertMatNear(callerOwned, scalarResult, 1.0e-6);
#if NETCOREAPP3_1_OR_GREATER
                AssertMatNear(callerOwned, spanResult, 1.0e-6);
#endif

                Assert.Equal(MatType.CV_64FC2, doubleResult.Type);
                AssertPoint2Near(new Point2d(2.0, -1.0), doubleResult.GetValue<Point2d>(0), 1.0e-12);
                AssertPoint2Near(new Point2d(2.0, -4.0), doubleResult.GetValue<Point2d>(1), 1.0e-12);
                AssertPoint2Near(new Point2d(5.0, -7.0), doubleResult.GetValue<Point2d>(2), 1.0e-12);

                Point2f[] euclidean =
                {
                    new Point2f(-4.0F, 2.5F),
                    new Point2f(0.25F, -8.0F),
                    new Point2f(6.0F, 9.0F)
                };
                using (Mat homogeneous = Calib3DCv2.ConvertPointsToHomogeneous(euclidean))
                using (Mat roundTrip = Calib3DCv2.ConvertPointsFromHomogeneous(homogeneous))
                {
                    for (int i = 0; i < euclidean.Length; ++i)
                    {
                        AssertPoint2Near(euclidean[i], roundTrip.GetValue<Point2f>(i), 1.0e-6);
                    }
                }
            }
        }

        [Fact]
        public void CorrectMatchesProducesFiniteEpipolarConsistentOutputsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point2f[] points1 =
            {
                new Point2f(10.0F, 20.0F),
                new Point2f(30.0F, 40.0F),
                new Point2f(50.0F, 60.0F)
            };
            Point2f[] points2 =
            {
                new Point2f(12.0F, 22.0F),
                new Point2f(33.0F, 39.0F),
                new Point2f(56.0F, 65.0F)
            };

            using (Mat fundamental = CreateFundamentalMatrix())
            using (Mat points1Mat = CreatePointMat(points1))
            using (Mat points2Mat = CreatePointMat(points2))
            using (var callerOwned1 = new Mat())
            using (var callerOwned2 = new Mat())
            {
                double residualBefore = SumAbsoluteEpipolarResiduals(
                    fundamental,
                    points1Mat,
                    points2Mat);

                Calib3DCv2.CorrectMatches(
                    fundamental,
                    points1Mat,
                    points2Mat,
                    callerOwned1,
                    callerOwned2);
                Calib3DCv2.CorrectMatches(
                    fundamental,
                    points1Mat,
                    points2Mat,
                    out Mat owned1,
                    out Mat owned2);
                using (owned1)
                using (owned2)
                {
                    Calib3DCv2.CorrectMatches(
                        fundamental,
                        points1,
                        points2,
                        out Mat arrayResult1,
                        out Mat arrayResult2);
                    using (arrayResult1)
                    using (arrayResult2)
#if NETCOREAPP3_1_OR_GREATER
                    {
                        Calib3DCv2.CorrectMatches(
                            fundamental,
                            points1.AsSpan(),
                            points2.AsSpan(),
                            out Mat spanResult1,
                            out Mat spanResult2);
                        using (spanResult1)
                        using (spanResult2)
#endif
                        {
                            Assert.Equal(points1Mat.Rows, callerOwned1.Rows);
                            Assert.Equal(points1Mat.Cols, callerOwned1.Cols);
                            Assert.Equal(points1Mat.Type, callerOwned1.Type);
                            Assert.Equal(points2Mat.Rows, callerOwned2.Rows);
                            Assert.Equal(points2Mat.Cols, callerOwned2.Cols);
                            Assert.Equal(points2Mat.Type, callerOwned2.Type);
                            AssertFinitePoints(callerOwned1);
                            AssertFinitePoints(callerOwned2);
                            AssertMatNear(callerOwned1, owned1, 1.0e-6);
                            AssertMatNear(callerOwned2, owned2, 1.0e-6);
                            AssertMatNear(callerOwned1, arrayResult1, 1.0e-6);
                            AssertMatNear(callerOwned2, arrayResult2, 1.0e-6);
#if NETCOREAPP3_1_OR_GREATER
                            AssertMatNear(callerOwned1, spanResult1, 1.0e-6);
                            AssertMatNear(callerOwned2, spanResult2, 1.0e-6);
#endif

                            double residualAfter = SumAbsoluteEpipolarResiduals(
                                fundamental,
                                callerOwned1,
                                callerOwned2);
                            Assert.InRange(residualAfter, 0.0, 1.0e-4);
                            Assert.True(residualAfter <= residualBefore);
                        }
#if NETCOREAPP3_1_OR_GREATER
                    }
#endif
                }
            }
        }

        [Fact]
        public void SampsonDistanceMatchesDirectFormulaAndPerturbationWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat fundamental = CreateFundamentalMatrix())
            using (Mat point1 = CreateHomogeneousPoint(10.0, 20.0))
            using (Mat exactPoint2 = CreateHomogeneousPoint(12.0, 20.0))
            using (Mat perturbedPoint2 = CreateHomogeneousPoint(12.0, 23.0))
            {
                double exact = Calib3DCv2.SampsonDistance(point1, exactPoint2, fundamental);
                double perturbed = Calib3DCv2.SampsonDistance(point1, perturbedPoint2, fundamental);
                double convenience = Calib3DCv2.SampsonDistance(
                    new Point2d(10.0, 20.0),
                    new Point2d(12.0, 23.0),
                    fundamental);
                double direct = CalculateSampsonDistance(point1, perturbedPoint2, fundamental);

                Assert.InRange(Math.Abs(exact), 0.0, 1.0e-12);
                Assert.True(perturbed > 0.0);
                AssertNear(4.5, perturbed, 1.0e-12);
                AssertNear(direct, perturbed, 1.0e-12);
                AssertNear(perturbed, convenience, 1.0e-12);
            }
        }

        private static Mat CreatePointMat(params Point2f[] points)
        {
            var result = new Mat(points.Length, 1, MatType.CV_32FC2);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue(i, points[i]);
            }
            return result;
        }

        private static Mat CreatePointMat(params Point3f[] points)
        {
            var result = new Mat(points.Length, 1, MatType.CV_32FC3);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue(i, points[i]);
            }
            return result;
        }

        private static Mat CreatePoint2dMat(params Point2d[] points)
        {
            var result = new Mat(points.Length, 1, MatType.CV_64FC2);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue(i, points[i]);
            }
            return result;
        }

        private static Mat CreatePointRowMat(Point2f[] points)
        {
            var result = new Mat(1, points.Length, MatType.CV_32FC2);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue(i, points[i]);
            }
            return result;
        }

        private static Mat CreateScalarPointMatrix(
            int rows,
            int columns,
            int type,
            params double[] values)
        {
            if (values.Length != rows * columns)
            {
                throw new ArgumentException("Value count must match the matrix size.", nameof(values));
            }

            var result = new Mat(rows, columns, type);
            try
            {
                if (type == MatType.CV_32FC1)
                {
                    for (int i = 0; i < values.Length; ++i)
                    {
                        result.SetValue(i, (float)values[i]);
                    }
                }
                else if (type == MatType.CV_64FC1)
                {
                    for (int i = 0; i < values.Length; ++i)
                    {
                        result.SetValue(i, values[i]);
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(type));
                }
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        private static Mat CreateFundamentalMatrix()
        {
            return CreateScalarPointMatrix(
                3,
                3,
                MatType.CV_64FC1,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                -1.0,
                0.0,
                1.0,
                0.0);
        }

        private static Mat CreateHomogeneousPoint(double x, double y)
        {
            return CreateScalarPointMatrix(
                3,
                1,
                MatType.CV_64FC1,
                x,
                y,
                1.0);
        }

        private static void AssertHomogeneousPoints(Point2f[] expected, Mat actual, double tolerance)
        {
            for (int i = 0; i < expected.Length; ++i)
            {
                Point3f point = actual.GetValue<Point3f>(i);
                AssertNear(expected[i].X, point.X, tolerance);
                AssertNear(expected[i].Y, point.Y, tolerance);
                AssertNear(1.0, point.Z, tolerance);
            }
        }

        private static void AssertFinitePoints(Mat points)
        {
            for (int i = 0; i < points.Rows * points.Cols; ++i)
            {
                Point2f point = points.GetValue<Point2f>(i);
                Assert.True(float.IsFinite(point.X));
                Assert.True(float.IsFinite(point.Y));
            }
        }

        private static void AssertMatNear(Mat expected, Mat actual, double tolerance)
        {
            Assert.Equal(expected.Rows, actual.Rows);
            Assert.Equal(expected.Cols, actual.Cols);
            Assert.Equal(expected.Type, actual.Type);

            if (expected.Type == MatType.CV_32FC2)
            {
                for (int i = 0; i < expected.Rows * expected.Cols; ++i)
                {
                    AssertPoint2Near(
                        expected.GetValue<Point2f>(i),
                        actual.GetValue<Point2f>(i),
                        tolerance);
                }
                return;
            }
            if (expected.Type == MatType.CV_32FC3)
            {
                for (int i = 0; i < expected.Rows * expected.Cols; ++i)
                {
                    AssertPoint3Near(
                        expected.GetValue<Point3f>(i),
                        actual.GetValue<Point3f>(i),
                        tolerance);
                }
                return;
            }

            throw new InvalidOperationException($"Unsupported comparison type {expected.Type}.");
        }

        private static double SumAbsoluteEpipolarResiduals(
            Mat fundamental,
            Mat points1,
            Mat points2)
        {
            double result = 0.0;
            for (int i = 0; i < points1.Rows * points1.Cols; ++i)
            {
                Point2f point1 = points1.GetValue<Point2f>(i);
                Point2f point2 = points2.GetValue<Point2f>(i);
                result += Math.Abs(EpipolarResidual(fundamental, point1, point2));
            }
            return result;
        }

        private static double EpipolarResidual(
            Mat fundamental,
            Point2f point1,
            Point2f point2)
        {
            double f0 = fundamental.GetValue<double>(0);
            double f1 = fundamental.GetValue<double>(1);
            double f2 = fundamental.GetValue<double>(2);
            double f3 = fundamental.GetValue<double>(3);
            double f4 = fundamental.GetValue<double>(4);
            double f5 = fundamental.GetValue<double>(5);
            double f6 = fundamental.GetValue<double>(6);
            double f7 = fundamental.GetValue<double>(7);
            double f8 = fundamental.GetValue<double>(8);
            double lineX = (f0 * point1.X) + (f1 * point1.Y) + f2;
            double lineY = (f3 * point1.X) + (f4 * point1.Y) + f5;
            double lineW = (f6 * point1.X) + (f7 * point1.Y) + f8;
            return (point2.X * lineX) + (point2.Y * lineY) + lineW;
        }

        private static double CalculateSampsonDistance(
            Mat point1,
            Mat point2,
            Mat fundamental)
        {
            double x1 = point1.GetValue<double>(0);
            double y1 = point1.GetValue<double>(1);
            double w1 = point1.GetValue<double>(2);
            double x2 = point2.GetValue<double>(0);
            double y2 = point2.GetValue<double>(1);
            double w2 = point2.GetValue<double>(2);
            double f0 = fundamental.GetValue<double>(0);
            double f1 = fundamental.GetValue<double>(1);
            double f2 = fundamental.GetValue<double>(2);
            double f3 = fundamental.GetValue<double>(3);
            double f4 = fundamental.GetValue<double>(4);
            double f5 = fundamental.GetValue<double>(5);
            double f6 = fundamental.GetValue<double>(6);
            double f7 = fundamental.GetValue<double>(7);
            double f8 = fundamental.GetValue<double>(8);

            double fx1 = (f0 * x1) + (f1 * y1) + (f2 * w1);
            double fy1 = (f3 * x1) + (f4 * y1) + (f5 * w1);
            double fw1 = (f6 * x1) + (f7 * y1) + (f8 * w1);
            double ftx2 = (f0 * x2) + (f3 * y2) + (f6 * w2);
            double fty2 = (f1 * x2) + (f4 * y2) + (f7 * w2);
            double residual = (x2 * fx1) + (y2 * fy1) + (w2 * fw1);
            return (residual * residual) /
                ((fx1 * fx1) + (fy1 * fy1) + (ftx2 * ftx2) + (fty2 * fty2));
        }

        private static void AssertPoint2Near(Point2f expected, Point2f actual, double tolerance)
        {
            AssertNear(expected.X, actual.X, tolerance);
            AssertNear(expected.Y, actual.Y, tolerance);
        }

        private static void AssertPoint2Near(Point2d expected, Point2d actual, double tolerance)
        {
            AssertNear(expected.X, actual.X, tolerance);
            AssertNear(expected.Y, actual.Y, tolerance);
        }

        private static void AssertPoint3Near(Point3f expected, Point3f actual, double tolerance)
        {
            AssertNear(expected.X, actual.X, tolerance);
            AssertNear(expected.Y, actual.Y, tolerance);
            AssertNear(expected.Z, actual.Z, tolerance);
        }

        private static void AssertNear(double expected, double actual, double tolerance)
        {
            Assert.InRange(Math.Abs(expected - actual), 0.0, tolerance);
        }
    }
}
