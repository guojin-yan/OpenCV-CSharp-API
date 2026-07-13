using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class TranslationEstimationTests
    {
        [Fact]
        public void TranslationEstimationValidatesInputs()
        {
            Point3f[] source3D = CreateSource3D(4);
            Point3f[] destination3D = Translate(
                source3D,
                new Point3f(1.0F, 2.0F, 3.0F));
            Point2f[] source2D = CreateSource2D(3);
            Point2f[] destination2D = Translate(
                source2D,
                new Point2f(1.0F, 2.0F));

            using (Mat source3DMat = CreatePointMat(source3D))
            using (Mat destination3DMat = CreatePointMat(destination3D))
            using (Mat source2DMat = CreatePointMat(source2D))
            using (Mat destination2DMat = CreatePointMat(destination2D))
            using (var translation = new Mat())
            using (var inliers = new Mat())
            using (var invalid3D = new Mat(4, 1, MatType.CV_32FC2))
            using (var invalid2D = new Mat(3, 1, MatType.CV_32FC3))
            using (Mat short3D = CreatePointMat(CreateSource3D(3)))
            using (Mat shortDestination3D = CreatePointMat(
                Translate(
                    CreateSource3D(3),
                    new Point3f(1.0F, 2.0F, 3.0F))))
            using (Mat mismatched2D = CreatePointMat(CreateSource2D(2)))
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.EstimateTranslation3D(
                        (Mat)null!,
                        destination3DMat,
                        translation));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.EstimateTranslation3D(
                        source3DMat,
                        destination3DMat,
                        null!));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateTranslation3D(
                        invalid3D,
                        destination3DMat,
                        translation));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateTranslation3D(
                        short3D,
                        shortDestination3D,
                        translation));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateTranslation3D(
                        source3DMat,
                        destination3DMat,
                        translation,
                        ransacThreshold: 0.0));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateTranslation3D(
                        source3DMat,
                        destination3DMat,
                        translation,
                        confidence: 1.0));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateTranslation3D(
                        Array.Empty<Point3f>(),
                        Array.Empty<Point3f>(),
                        translation));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateTranslation3D(
                        source3D,
                        destination3D[..3],
                        translation));

                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.EstimateTranslation2D(
                        (Mat)null!,
                        destination2DMat));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateTranslation2D(
                        invalid2D,
                        destination2DMat));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateTranslation2D(
                        source2DMat,
                        mismatched2D));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateTranslation2D(
                        source2DMat,
                        destination2DMat,
                        method: RobustEstimationAlgorithms.LeastSquares));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateTranslation2D(
                        source2DMat,
                        destination2DMat,
                        ransacReprojThreshold: double.NaN));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateTranslation2D(
                        source2DMat,
                        destination2DMat,
                        maxIters: 0));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateTranslation2D(
                        source2DMat,
                        destination2DMat,
                        confidence: 0.0));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateTranslation2D(
                        source2DMat,
                        destination2DMat,
                        refineIters: -1));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateTranslation2D(
                        Array.Empty<Point2f>(),
                        Array.Empty<Point2f>()));

#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateTranslation3D(
                        ReadOnlySpan<Point3f>.Empty,
                        ReadOnlySpan<Point3f>.Empty,
                        translation));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateTranslation2D(
                        ReadOnlySpan<Point2f>.Empty,
                        ReadOnlySpan<Point2f>.Empty));
#endif
            }

            var disposed = new Mat(4, 1, MatType.CV_32FC3);
            disposed.Dispose();
            using (var translation = new Mat())
            using (Mat destination = CreatePointMat(destination3D))
            {
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.EstimateTranslation3D(
                        disposed,
                        destination,
                        translation));
            }
        }

        [Fact]
        public void EstimateTranslation3DRecoversExactTranslationAcrossOverloadsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f expected = new Point3f(2.5F, -3.25F, 4.75F);
            Point3f[] source = CreateSource3D(6);
            Point3f[] destination = Translate(source, expected);

            using (Mat sourceMat = CreatePointMat(source))
            using (Mat destinationMat = CreatePointMat(destination))
            using (var callerTranslation = new Mat())
            using (var callerInliers = new Mat())
            using (Mat rowSource = CreatePoint3RowMat(source))
            using (Mat rowDestination = CreatePoint3RowMat(destination))
            using (var rowTranslation = new Mat())
            using (Mat scalarSource = CreateScalarPoint3Mat(source))
            using (Mat scalarDestination = CreateScalarPoint3Mat(destination))
            using (var scalarTranslation = new Mat())
            {
                Assert.True(Calib3DCv2.EstimateTranslation3D(
                    sourceMat,
                    destinationMat,
                    callerTranslation,
                    callerInliers,
                    ransacThreshold: 0.01));
                AssertTranslation3D(expected, callerTranslation, 1.0e-6);
                AssertAllInliers(callerInliers, source.Length);

                Assert.True(Calib3DCv2.EstimateTranslation3D(
                    sourceMat,
                    destinationMat,
                    out Mat ownedTranslation,
                    out Mat ownedInliers,
                    ransacThreshold: 0.01));
                using (ownedTranslation)
                using (ownedInliers)
                {
                    AssertTranslation3D(expected, ownedTranslation, 1.0e-6);
                    AssertMasksEqual(callerInliers, ownedInliers);
                }

                Assert.True(Calib3DCv2.EstimateTranslation3D(
                    source,
                    destination,
                    out Mat arrayTranslation,
                    out Mat arrayInliers,
                    ransacThreshold: 0.01));
                using (arrayTranslation)
                using (arrayInliers)
                {
                    AssertTranslation3D(expected, arrayTranslation, 1.0e-6);
                    AssertMasksEqual(callerInliers, arrayInliers);
                }

#if NETCOREAPP3_1_OR_GREATER
                Assert.True(Calib3DCv2.EstimateTranslation3D(
                    source.AsSpan(),
                    destination.AsSpan(),
                    out Mat spanTranslation,
                    out Mat spanInliers,
                    ransacThreshold: 0.01));
                using (spanTranslation)
                using (spanInliers)
                {
                    AssertTranslation3D(expected, spanTranslation, 1.0e-6);
                    AssertMasksEqual(callerInliers, spanInliers);
                }
#endif

                Assert.True(Calib3DCv2.EstimateTranslation3D(
                    rowSource,
                    rowDestination,
                    rowTranslation,
                    ransacThreshold: 0.01));
                AssertTranslation3D(expected, rowTranslation, 1.0e-6);

                Assert.True(Calib3DCv2.EstimateTranslation3D(
                    scalarSource,
                    scalarDestination,
                    scalarTranslation,
                    ransacThreshold: 0.01));
                AssertTranslation3D(expected, scalarTranslation, 1.0e-6);
            }
        }

        [Fact]
        public void EstimateTranslation3DRansacClassifiesDeterministicOutliersWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f expected = new Point3f(-1.5F, 2.25F, 3.5F);
            Point3f[] source = CreateSource3D(12);
            Point3f[] destination = Translate(source, expected);
            for (int i = 8; i < destination.Length; ++i)
            {
                destination[i] = new Point3f(
                    destination[i].X + (20.0F * (i - 7)),
                    destination[i].Y - 15.0F,
                    destination[i].Z + 11.0F);
            }

            using (var translation = new Mat())
            using (var inliers = new Mat())
            {
                Assert.True(Calib3DCv2.EstimateTranslation3D(
                    source,
                    destination,
                    translation,
                    inliers,
                    ransacThreshold: 0.1,
                    confidence: 0.999));
                AssertTranslation3D(expected, translation, 1.0e-5);
                AssertMask(inliers, 8, 4);
            }
        }

        [Fact]
        public void EstimateTranslation2DRecoversRansacAndLmedsFixturesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point2f expected = new Point2f(4.5F, -2.75F);

            Point2f[] oneSource = { new Point2f(3.0F, 8.0F) };
            Point2f[] oneDestination = Translate(oneSource, expected);
            Point2d oneRansac = Calib3DCv2.EstimateTranslation2D(
                oneSource,
                oneDestination,
                method: RobustEstimationAlgorithms.RANSAC,
                ransacReprojThreshold: 0.01);
            Point2d oneLmeds = Calib3DCv2.EstimateTranslation2D(
                oneSource,
                oneDestination,
                method: RobustEstimationAlgorithms.LMEDS,
                ransacReprojThreshold: 0.01);
            AssertTranslation2D(expected, oneRansac, 1.0e-6);
            AssertTranslation2D(expected, oneLmeds, 1.0e-6);

            Point2f[] ransacSource = CreateSource2D(10);
            Point2f[] ransacDestination = Translate(ransacSource, expected);
            for (int i = 4; i < ransacDestination.Length; ++i)
            {
                ransacDestination[i] = new Point2f(
                    ransacDestination[i].X + (15.0F * (i - 3)),
                    ransacDestination[i].Y + 20.0F);
            }
            using (var ransacInliers = new Mat())
            {
                Point2d actual = Calib3DCv2.EstimateTranslation2D(
                    ransacSource,
                    ransacDestination,
                    ransacInliers,
                    RobustEstimationAlgorithms.RANSAC,
                    ransacReprojThreshold: 0.1,
                    maxIters: 5000,
                    confidence: 0.999);
                AssertTranslation2D(expected, actual, 1.0e-5);
                AssertMask(ransacInliers, 4, 6);
            }

            Point2f[] lmedsSource = CreateSource2D(10);
            Point2f[] lmedsDestination = Translate(lmedsSource, expected);
            for (int i = 7; i < lmedsDestination.Length; ++i)
            {
                lmedsDestination[i] = new Point2f(
                    lmedsDestination[i].X - 25.0F,
                    lmedsDestination[i].Y + (17.0F * (i - 6)));
            }
            using (var lmedsInliers = new Mat())
            {
                Point2d actual = Calib3DCv2.EstimateTranslation2D(
                    lmedsSource,
                    lmedsDestination,
                    lmedsInliers,
                    RobustEstimationAlgorithms.LMEDS,
                    ransacReprojThreshold: 0.1,
                    maxIters: 5000,
                    confidence: 0.999);
                AssertTranslation2D(expected, actual, 1.0e-5);
                AssertMask(lmedsInliers, 7, 3);
            }

#if NETCOREAPP3_1_OR_GREATER
            using (var arrayInliers = new Mat())
            using (var spanInliers = new Mat())
            {
                Point2d arrayResult = Calib3DCv2.EstimateTranslation2D(
                    oneSource,
                    oneDestination,
                    arrayInliers);
                Point2d spanResult = Calib3DCv2.EstimateTranslation2D(
                    oneSource.AsSpan(),
                    oneDestination.AsSpan(),
                    spanInliers);
                AssertNear(arrayResult.X, spanResult.X, 1.0e-12);
                AssertNear(arrayResult.Y, spanResult.Y, 1.0e-12);
                AssertMasksEqual(arrayInliers, spanInliers);
            }
#endif
        }

        [Fact]
        public void EstimateTranslation2DConvertsIntegerInputsPreservesInputsAndReportsFailureWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point[] integerSource =
            {
                new Point(1, 5),
                new Point(3, 3),
                new Point(1, 3)
            };
            Point[] integerDestination =
            {
                new Point(6, 3),
                new Point(8, 1),
                new Point(6, 1)
            };
            using (Mat integerSourceMat = CreatePointMat(integerSource))
            using (Mat integerDestinationMat = CreatePointMat(integerDestination))
            using (var integerInliers = new Mat())
            {
                Point2d actual = Calib3DCv2.EstimateTranslation2D(
                    integerSourceMat,
                    integerDestinationMat,
                    integerInliers);
                AssertTranslation2D(new Point2f(5.0F, -2.0F), actual, 1.0e-6);
                AssertAllInliers(integerInliers, integerSource.Length);
            }

            Point2f[] source = CreateSource2D(5);
            Point2f[] destination = Translate(
                source,
                new Point2f(0.1F, 0.1F));
            destination[2] = new Point2f(0.0F, 4.0F);
            using (Mat sourceMat = CreatePointMat(source))
            using (Mat destinationMat = CreatePointMat(destination))
            using (Mat sourceCopy = sourceMat.Clone())
            using (Mat destinationCopy = destinationMat.Clone())
            using (var inliers = new Mat())
            {
                Point2d actual = Calib3DCv2.EstimateTranslation2D(
                    sourceMat,
                    destinationMat,
                    inliers,
                    ransacReprojThreshold: 0.5);
                AssertTranslation2D(new Point2f(0.1F, 0.1F), actual, 1.0e-5);
                AssertPointMatsEqual(sourceCopy, sourceMat);
                AssertPointMatsEqual(destinationCopy, destinationMat);
                Assert.Equal((byte)0, inliers.GetValue<byte>(2));
            }

            Point2f[] invalidSource =
            {
                new Point2f(float.NaN, 0.0F),
                new Point2f(float.NaN, 1.0F),
                new Point2f(float.NaN, 2.0F)
            };
            Point2f[] invalidDestination =
            {
                new Point2f(1.0F, 0.0F),
                new Point2f(1.0F, 1.0F),
                new Point2f(1.0F, 2.0F)
            };
            using (var failureInliers = new Mat())
            {
                Point2d failure = Calib3DCv2.EstimateTranslation2D(
                    invalidSource,
                    invalidDestination,
                    failureInliers,
                    ransacReprojThreshold: 0.1);
                Assert.True(double.IsNaN(failure.X));
                Assert.True(double.IsNaN(failure.Y));
                AssertMask(failureInliers, 0, invalidSource.Length);
            }
        }

        private static Point3f[] CreateSource3D(int count)
        {
            var result = new Point3f[count];
            for (int i = 0; i < count; ++i)
            {
                result[i] = new Point3f(
                    (i * 1.75F) + 0.5F,
                    ((i % 4) * 2.5F) - 3.0F,
                    ((i * i) * 0.4F) + 1.0F);
            }
            return result;
        }

        private static Point2f[] CreateSource2D(int count)
        {
            var result = new Point2f[count];
            for (int i = 0; i < count; ++i)
            {
                result[i] = new Point2f(
                    (i * 3.0F) + 1.0F,
                    ((i % 3) * 4.0F) + (i * 0.25F));
            }
            return result;
        }

        private static Point3f[] Translate(
            Point3f[] source,
            Point3f translation)
        {
            var result = new Point3f[source.Length];
            for (int i = 0; i < source.Length; ++i)
            {
                result[i] = new Point3f(
                    source[i].X + translation.X,
                    source[i].Y + translation.Y,
                    source[i].Z + translation.Z);
            }
            return result;
        }

        private static Point2f[] Translate(
            Point2f[] source,
            Point2f translation)
        {
            var result = new Point2f[source.Length];
            for (int i = 0; i < source.Length; ++i)
            {
                result[i] = new Point2f(
                    source[i].X + translation.X,
                    source[i].Y + translation.Y);
            }
            return result;
        }

        private static Mat CreatePointMat(Point3f[] points)
        {
            var result = new Mat(points.Length, 1, MatType.CV_32FC3);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue(i, points[i]);
            }
            return result;
        }

        private static Mat CreatePoint3RowMat(Point3f[] points)
        {
            var result = new Mat(1, points.Length, MatType.CV_32FC3);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue(i, points[i]);
            }
            return result;
        }

        private static Mat CreateScalarPoint3Mat(Point3f[] points)
        {
            var result = new Mat(points.Length, 3, MatType.CV_64FC1);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue((i * 3) + 0, (double)points[i].X);
                result.SetValue((i * 3) + 1, (double)points[i].Y);
                result.SetValue((i * 3) + 2, (double)points[i].Z);
            }
            return result;
        }

        private static Mat CreatePointMat(Point2f[] points)
        {
            var result = new Mat(points.Length, 1, MatType.CV_32FC2);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue(i, points[i]);
            }
            return result;
        }

        private static Mat CreatePointMat(Point[] points)
        {
            var result = new Mat(points.Length, 1, MatType.CV_32SC2);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue(i, points[i]);
            }
            return result;
        }

        private static void AssertTranslation3D(
            Point3f expected,
            Mat actual,
            double tolerance)
        {
            Assert.Equal(1, actual.Rows);
            Assert.Equal(3, actual.Cols);
            Assert.Equal(MatType.CV_64FC1, actual.Type);
            AssertNear(expected.X, actual.GetValue<double>(0), tolerance);
            AssertNear(expected.Y, actual.GetValue<double>(1), tolerance);
            AssertNear(expected.Z, actual.GetValue<double>(2), tolerance);
        }

        private static void AssertTranslation2D(
            Point2f expected,
            Point2d actual,
            double tolerance)
        {
            AssertNear(expected.X, actual.X, tolerance);
            AssertNear(expected.Y, actual.Y, tolerance);
        }

        private static void AssertAllInliers(Mat mask, int count)
        {
            AssertMask(mask, count, 0);
        }

        private static void AssertMask(
            Mat mask,
            int inlierCount,
            int outlierCount)
        {
            Assert.Equal(inlierCount + outlierCount, mask.Rows);
            Assert.Equal(1, mask.Cols);
            Assert.Equal(MatType.CV_8UC1, mask.Type);
            for (int i = 0; i < inlierCount; ++i)
            {
                Assert.Equal((byte)1, mask.GetValue<byte>(i));
            }
            for (int i = inlierCount; i < inlierCount + outlierCount; ++i)
            {
                Assert.Equal((byte)0, mask.GetValue<byte>(i));
            }
        }

        private static void AssertMasksEqual(Mat expected, Mat actual)
        {
            Assert.Equal(expected.Rows, actual.Rows);
            Assert.Equal(expected.Cols, actual.Cols);
            Assert.Equal(expected.Type, actual.Type);
            for (int i = 0; i < expected.Rows * expected.Cols; ++i)
            {
                Assert.Equal(
                    expected.GetValue<byte>(i),
                    actual.GetValue<byte>(i));
            }
        }

        private static void AssertPointMatsEqual(Mat expected, Mat actual)
        {
            Assert.Equal(expected.Rows, actual.Rows);
            Assert.Equal(expected.Cols, actual.Cols);
            Assert.Equal(expected.Type, actual.Type);
            for (int i = 0; i < expected.Rows * expected.Cols; ++i)
            {
                Assert.Equal(
                    expected.GetValue<Point2f>(i),
                    actual.GetValue<Point2f>(i));
            }
        }

        private static void AssertNear(
            double expected,
            double actual,
            double tolerance)
        {
            Assert.InRange(Math.Abs(expected - actual), 0.0, tolerance);
        }
    }
}
