using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class AffineEstimationTests
    {
        [Fact]
        public void AffineEstimationValidatesInputs()
        {
            Point3f[] source3D = CreateSource3D(5);
            Point3f[] destination3D = ApplyAffine3D(source3D, FullAffine3D);
            Point2f[] source2D = CreateSource2D(4);
            Point2f[] destination2D = ApplyAffine2D(source2D, FullAffine2D);

            using (Mat source3DMat = CreatePointMat(source3D))
            using (Mat destination3DMat = CreatePointMat(destination3D))
            using (Mat source2DMat = CreatePointMat(source2D))
            using (Mat destination2DMat = CreatePointMat(destination2D))
            using (Mat short3D = CreatePointMat(CreateSource3D(3)))
            using (Mat short2D = CreatePointMat(CreateSource2D(2)))
            using (var transform = new Mat())
            using (var inliers = new Mat())
            using (var invalid3D = new Mat(5, 1, MatType.CV_32FC2))
            using (var invalid2D = new Mat(4, 1, MatType.CV_32FC3))
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.EstimateAffine3D(
                        (Mat)null!,
                        destination3DMat,
                        transform));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.EstimateAffine3D(
                        source3DMat,
                        destination3DMat,
                        null!));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateAffine3D(
                        invalid3D,
                        destination3DMat,
                        transform));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateAffine3D(
                        short3D,
                        short3D,
                        transform));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateAffine3D(
                        source3DMat,
                        destination3DMat,
                        transform,
                        ransacThreshold: 0.0));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateAffine3D(
                        source3DMat,
                        destination3DMat,
                        transform,
                        confidence: 1.0));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateAffine3D(
                        source3DMat,
                        destination3DMat,
                        source3DMat));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateAffine3D(
                        source3DMat,
                        destination3DMat,
                        transform,
                        transform));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateAffine3D(
                        Array.Empty<Point3f>(),
                        Array.Empty<Point3f>(),
                        transform));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateAffine3D(
                        source3D,
                        destination3D[..4],
                        transform));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateAffine3D(
                        CreateSource3D(2),
                        CreateSource3D(2),
                        out double _));

                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.EstimateAffine2D(
                        (Mat)null!,
                        destination2DMat));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateAffine2D(
                        invalid2D,
                        destination2DMat));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateAffine2D(
                        short2D,
                        short2D));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateAffine2D(
                        source2DMat,
                        destination2DMat,
                        method: RobustEstimationAlgorithms.LeastSquares));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateAffine2D(
                        source2DMat,
                        destination2DMat,
                        ransacReprojThreshold: double.NaN));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateAffine2D(
                        source2DMat,
                        destination2DMat,
                        maxIters: 0));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateAffine2D(
                        source2DMat,
                        destination2DMat,
                        confidence: 0.0));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.EstimateAffine2D(
                        source2DMat,
                        destination2DMat,
                        refineIters: -1));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateAffine2D(
                        source2DMat,
                        destination2DMat,
                        source2DMat));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateAffinePartial2D(
                        Array.Empty<Point2f>(),
                        Array.Empty<Point2f>()));

#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateAffine3D(
                        ReadOnlySpan<Point3f>.Empty,
                        ReadOnlySpan<Point3f>.Empty,
                        transform));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.EstimateAffinePartial2D(
                        ReadOnlySpan<Point2f>.Empty,
                        ReadOnlySpan<Point2f>.Empty));
#endif
            }

            var disposed = new Mat(5, 1, MatType.CV_32FC3);
            disposed.Dispose();
            using (Mat destination = CreatePointMat(destination3D))
            using (var transform = new Mat())
            {
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.EstimateAffine3D(
                        disposed,
                        destination,
                        transform));
            }
        }

        [Fact]
        public void EstimateAffine3DRecoversExactTransformAcrossOverloadsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] source = CreateSource3D(8);
            Point3f[] destination = ApplyAffine3D(source, FullAffine3D);

            using (Mat sourceMat = CreatePointMat(source))
            using (Mat destinationMat = CreatePointMat(destination))
            using (var callerTransform = new Mat())
            using (var callerInliers = new Mat())
            {
                Assert.True(Calib3DCv2.EstimateAffine3D(
                    sourceMat,
                    destinationMat,
                    callerTransform,
                    callerInliers,
                    ransacThreshold: 0.01,
                    confidence: 0.999));
                AssertTransform(FullAffine3D, callerTransform, 3, 4, 1.0e-4);
                AssertMask(callerInliers, source.Length, 0);

                Assert.True(Calib3DCv2.EstimateAffine3D(
                    source,
                    destination,
                    out Mat arrayTransform,
                    out Mat arrayInliers,
                    ransacThreshold: 0.01,
                    confidence: 0.999));
                using (arrayTransform)
                using (arrayInliers)
                {
                    AssertTransform(FullAffine3D, arrayTransform, 3, 4, 1.0e-4);
                    AssertMasksEqual(callerInliers, arrayInliers);
                }

#if NETCOREAPP3_1_OR_GREATER
                Assert.True(Calib3DCv2.EstimateAffine3D(
                    source.AsSpan(),
                    destination.AsSpan(),
                    out Mat spanTransform,
                    out Mat spanInliers,
                    ransacThreshold: 0.01,
                    confidence: 0.999));
                using (spanTransform)
                using (spanInliers)
                {
                    AssertTransform(FullAffine3D, spanTransform, 3, 4, 1.0e-4);
                    AssertMasksEqual(callerInliers, spanInliers);
                }
#endif
            }
        }

        [Fact]
        public void EstimateAffine3DRansacRejectsDeterministicOutliersWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] source = CreateSource3D(12);
            Point3f[] destination = ApplyAffine3D(source, FullAffine3D);
            for (int i = 9; i < destination.Length; ++i)
            {
                destination[i] = new Point3f(
                    destination[i].X + (20.0F * (i - 8)),
                    destination[i].Y - 15.0F,
                    destination[i].Z + 11.0F);
            }

            using (var transform = new Mat())
            using (var inliers = new Mat())
            {
                Assert.True(Calib3DCv2.EstimateAffine3D(
                    source,
                    destination,
                    transform,
                    inliers,
                    ransacThreshold: 0.05,
                    confidence: 0.999));
                AssertTransform(FullAffine3D, transform, 3, 4, 1.0e-4);
                AssertMask(inliers, 9, 3);
            }
        }

        [Fact]
        public void EstimateAffine3DUmeyamaReturnsRotationTranslationAndScaleWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] source = CreateSource3D(8);
            double[] rotation =
            {
                0.0, -1.0, 0.0,
                1.0, 0.0, 0.0,
                0.0, 0.0, 1.0
            };
            const double expectedScale = 1.75;
            double[] expectedTransform =
            {
                0.0, -1.0, 0.0, 2.5,
                1.0, 0.0, 0.0, -3.25,
                0.0, 0.0, 1.0, 4.75
            };
            Point3f[] destination = ApplySimilarity3D(
                source,
                rotation,
                expectedScale,
                new Point3f(2.5F, -3.25F, 4.75F));

            using (Mat arrayTransform = Calib3DCv2.EstimateAffine3D(
                source,
                destination,
                out double arrayScale))
            {
                AssertNear(expectedScale, arrayScale, 1.0e-6);
                AssertTransform(expectedTransform, arrayTransform, 3, 4, 1.0e-6);
            }

#if NETCOREAPP3_1_OR_GREATER
            using (Mat spanTransform = Calib3DCv2.EstimateAffine3D(
                source.AsSpan(),
                destination.AsSpan(),
                out double spanScale))
            {
                AssertNear(expectedScale, spanScale, 1.0e-6);
                AssertTransform(expectedTransform, spanTransform, 3, 4, 1.0e-6);
            }
#endif
        }

        [Fact]
        public void EstimateAffine3DUmeyamaForceRotationControlsReflectionWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] source = CreateSource3D(8);
            double[] reflection =
            {
                -1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0
            };
            Point3f[] destination = ApplySimilarity3D(
                source,
                reflection,
                1.0,
                new Point3f(3.0F, -2.0F, 1.0F));

            using (Mat reflected = Calib3DCv2.EstimateAffine3D(
                source,
                destination,
                out double reflectedScale,
                forceRotation: false))
            using (Mat forcedRotation = Calib3DCv2.EstimateAffine3D(
                source,
                destination,
                out double forcedScale,
                forceRotation: true))
            {
                Assert.True(Determinant3x3(reflected) < 0.0);
                Assert.True(Determinant3x3(forcedRotation) > 0.0);
                AssertNear(1.0, reflectedScale, 1.0e-6);
                Assert.True(ReconstructionError(source, destination, reflected, reflectedScale) < 1.0e-6);
                Assert.True(ReconstructionError(source, destination, forcedRotation, forcedScale) > 1.0e-3);
            }
        }

        [Fact]
        public void EstimateAffine2DRansacRecoversTransformAndOutlierMaskWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point2f[] source = CreateSource2D(10);
            Point2f[] destination = ApplyAffine2D(source, FullAffine2D);
            destination[8] = new Point2f(destination[8].X + 30.0F, destination[8].Y - 20.0F);
            destination[9] = new Point2f(destination[9].X - 25.0F, destination[9].Y + 35.0F);

            using (var inliers = new Mat())
            using (Mat transform = Calib3DCv2.EstimateAffine2D(
                source,
                destination,
                inliers,
                RobustEstimationAlgorithms.RANSAC,
                ransacReprojThreshold: 0.05,
                maxIters: 5000,
                confidence: 0.999))
            {
                AssertTransform(FullAffine2D, transform, 2, 3, 1.0e-5);
                AssertMask(inliers, 8, 2);
            }
        }

        [Fact]
        public void EstimateAffine2DLmedsPreservesInputsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point2f[] source = CreateSource2D(8);
            Point2f[] destination = ApplyAffine2D(source, FullAffine2D);
            destination[7] = new Point2f(destination[7].X + 40.0F, destination[7].Y - 30.0F);

            using (Mat sourceMat = CreatePointMat(source))
            using (Mat destinationMat = CreatePointMat(destination))
            using (Mat sourceCopy = sourceMat.Clone())
            using (Mat destinationCopy = destinationMat.Clone())
            using (var inliers = new Mat())
            using (Mat transform = Calib3DCv2.EstimateAffine2D(
                sourceMat,
                destinationMat,
                inliers,
                RobustEstimationAlgorithms.LMEDS,
                ransacReprojThreshold: 0.05))
            {
                AssertTransform(FullAffine2D, transform, 2, 3, 1.0e-5);
                AssertMask(inliers, 7, 1);
                Assert.Equal(sourceCopy.ToArray<float>(), sourceMat.ToArray<float>());
                Assert.Equal(destinationCopy.ToArray<float>(), destinationMat.ToArray<float>());
            }
        }

        [Fact]
        public void EstimateAffinePartial2DMatchesArrayAndSpanOverloadsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            double cos = Math.Cos(Math.PI / 6.0);
            double sin = Math.Sin(Math.PI / 6.0);
            double[] expected =
            {
                1.5 * cos, -1.5 * sin, 4.0,
                1.5 * sin, 1.5 * cos, -2.0
            };
            Point2f[] source = CreateSource2D(8);
            Point2f[] destination = ApplyAffine2D(source, expected);

            using (var arrayInliers = new Mat())
            {
                Mat arrayTransform = Calib3DCv2.EstimateAffinePartial2D(
                    source,
                    destination,
                    arrayInliers,
                    RobustEstimationAlgorithms.RANSAC,
                    ransacReprojThreshold: 0.01);
                AssertTransform(expected, arrayTransform, 2, 3, 1.0e-5);
                AssertMask(arrayInliers, source.Length, 0);

#if NETCOREAPP3_1_OR_GREATER
                using (var spanInliers = new Mat())
                using (Mat spanTransform = Calib3DCv2.EstimateAffinePartial2D(
                    source.AsSpan(),
                    destination.AsSpan(),
                    spanInliers,
                    RobustEstimationAlgorithms.RANSAC,
                    ransacReprojThreshold: 0.01))
                {
                    AssertTransform(expected, spanTransform, 2, 3, 1.0e-5);
                    AssertMasksEqual(arrayInliers, spanInliers);
                }
#endif

                arrayTransform.Dispose();
                Assert.True(arrayTransform.IsDisposed);
            }
        }

        private static readonly double[] FullAffine3D =
        {
            1.2, 0.1, -0.2, 2.0,
            -0.3, 0.9, 0.15, -1.0,
            0.05, -0.25, 1.1, 3.0
        };

        private static readonly double[] FullAffine2D =
        {
            1.2, 0.3, 2.0,
            -0.2, 0.9, -1.0
        };

        private static Point3f[] CreateSource3D(int count)
        {
            Point3f[] points =
            {
                new Point3f(0.0F, 0.0F, 0.0F),
                new Point3f(1.0F, 0.0F, 0.0F),
                new Point3f(0.0F, 1.0F, 0.0F),
                new Point3f(0.0F, 0.0F, 1.0F),
                new Point3f(1.0F, 2.0F, 3.0F),
                new Point3f(-2.0F, 1.0F, 0.5F),
                new Point3f(3.0F, -1.0F, 2.0F),
                new Point3f(-1.0F, -2.0F, 1.0F),
                new Point3f(2.0F, 2.0F, -1.0F),
                new Point3f(-3.0F, 0.0F, 2.0F),
                new Point3f(0.5F, -1.5F, 2.5F),
                new Point3f(4.0F, 1.0F, -2.0F)
            };
            return points[..count];
        }

        private static Point2f[] CreateSource2D(int count)
        {
            Point2f[] points =
            {
                new Point2f(0.0F, 0.0F),
                new Point2f(1.0F, 0.0F),
                new Point2f(0.0F, 1.0F),
                new Point2f(2.0F, 1.0F),
                new Point2f(-1.0F, 2.0F),
                new Point2f(3.0F, -2.0F),
                new Point2f(-2.0F, -1.0F),
                new Point2f(1.5F, 3.0F),
                new Point2f(4.0F, 2.0F),
                new Point2f(-3.0F, 1.0F)
            };
            return points[..count];
        }

        private static Point3f[] ApplyAffine3D(Point3f[] source, double[] transform)
        {
            var result = new Point3f[source.Length];
            for (int i = 0; i < source.Length; ++i)
            {
                Point3f point = source[i];
                result[i] = new Point3f(
                    (float)((transform[0] * point.X) + (transform[1] * point.Y) + (transform[2] * point.Z) + transform[3]),
                    (float)((transform[4] * point.X) + (transform[5] * point.Y) + (transform[6] * point.Z) + transform[7]),
                    (float)((transform[8] * point.X) + (transform[9] * point.Y) + (transform[10] * point.Z) + transform[11]));
            }
            return result;
        }

        private static Point3f[] ApplySimilarity3D(
            Point3f[] source,
            double[] rotation,
            double scale,
            Point3f translation)
        {
            var result = new Point3f[source.Length];
            for (int i = 0; i < source.Length; ++i)
            {
                Point3f point = source[i];
                result[i] = new Point3f(
                    (float)(scale * ((rotation[0] * point.X) + (rotation[1] * point.Y) + (rotation[2] * point.Z)) + translation.X),
                    (float)(scale * ((rotation[3] * point.X) + (rotation[4] * point.Y) + (rotation[5] * point.Z)) + translation.Y),
                    (float)(scale * ((rotation[6] * point.X) + (rotation[7] * point.Y) + (rotation[8] * point.Z)) + translation.Z));
            }
            return result;
        }

        private static Point2f[] ApplyAffine2D(Point2f[] source, double[] transform)
        {
            var result = new Point2f[source.Length];
            for (int i = 0; i < source.Length; ++i)
            {
                Point2f point = source[i];
                result[i] = new Point2f(
                    (float)((transform[0] * point.X) + (transform[1] * point.Y) + transform[2]),
                    (float)((transform[3] * point.X) + (transform[4] * point.Y) + transform[5]));
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

        private static Mat CreatePointMat(Point2f[] points)
        {
            var result = new Mat(points.Length, 1, MatType.CV_32FC2);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue(i, points[i]);
            }
            return result;
        }

        private static void AssertTransform(
            double[] expected,
            Mat actual,
            int rows,
            int columns,
            double tolerance)
        {
            Assert.Equal(rows, actual.Rows);
            Assert.Equal(columns, actual.Cols);
            Assert.Equal(MatType.CV_64FC1, actual.Type);
            double[] values = actual.ToArray<double>();
            Assert.Equal(expected.Length, values.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                AssertNear(expected[i], values[i], tolerance);
            }
        }

        private static void AssertMask(Mat mask, int expectedInliers, int expectedOutliers)
        {
            Assert.Equal(MatType.CV_8UC1, mask.Type);
            byte[] values = mask.ToArray<byte>();
            Assert.Equal(expectedInliers + expectedOutliers, values.Length);
            Assert.Equal(expectedInliers, Array.FindAll(values, value => value != 0).Length);
            Assert.Equal(expectedOutliers, Array.FindAll(values, value => value == 0).Length);
        }

        private static void AssertMasksEqual(Mat expected, Mat actual)
        {
            Assert.Equal(expected.Rows, actual.Rows);
            Assert.Equal(expected.Cols, actual.Cols);
            Assert.Equal(expected.Type, actual.Type);
            Assert.Equal(expected.ToArray<byte>(), actual.ToArray<byte>());
        }

        private static double Determinant3x3(Mat transform)
        {
            double[] value = transform.ToArray<double>();
            return
                (value[0] * ((value[5] * value[10]) - (value[6] * value[9]))) -
                (value[1] * ((value[4] * value[10]) - (value[6] * value[8]))) +
                (value[2] * ((value[4] * value[9]) - (value[5] * value[8])));
        }

        private static double ReconstructionError(
            Point3f[] source,
            Point3f[] destination,
            Mat transform,
            double scale)
        {
            double[] value = transform.ToArray<double>();
            double squaredError = 0.0;
            for (int i = 0; i < source.Length; ++i)
            {
                Point3f point = source[i];
                double x = scale * ((value[0] * point.X) + (value[1] * point.Y) + (value[2] * point.Z)) + value[3];
                double y = scale * ((value[4] * point.X) + (value[5] * point.Y) + (value[6] * point.Z)) + value[7];
                double z = scale * ((value[8] * point.X) + (value[9] * point.Y) + (value[10] * point.Z)) + value[11];
                double dx = x - destination[i].X;
                double dy = y - destination[i].Y;
                double dz = z - destination[i].Z;
                squaredError += (dx * dx) + (dy * dy) + (dz * dz);
            }
            return Math.Sqrt(squaredError / source.Length);
        }

        private static void AssertNear(double expected, double actual, double tolerance)
        {
            Assert.InRange(actual, expected - tolerance, expected + tolerance);
        }
    }
}
