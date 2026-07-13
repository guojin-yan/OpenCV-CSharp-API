using System;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class ComputeCorrespondEpilinesTests
    {
        [Fact]
        public void ComputeCorrespondEpilinesValidatesInputsBeforeNativeCall()
        {
            using Mat points = CreatePointMat(
                new Point2f(10.0F, 20.0F),
                new Point2f(12.0F, 23.0F));
            using Mat fundamental = CreateFundamentalMatrix(MatType.CV_64FC1);
            using var lines = new Mat();
            using var invalidFundamentalShape = new Mat(2, 3, MatType.CV_64FC1);
            using var invalidFundamentalChannels = new Mat(3, 3, MatType.CV_64FC2);
            using var invalidFundamentalDepth = new Mat(3, 3, MatType.CV_32SC1);
            using var invalidPointShape = new Mat(2, 4, MatType.CV_32FC1);
            using var invalidPointDepth = new Mat(2, 1, MatType.CV_8UC2);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.ComputeCorrespondEpilines(points, 0, fundamental, lines));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.ComputeCorrespondEpilines(points, 3, fundamental, lines));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ComputeCorrespondEpilines(points, 1, invalidFundamentalShape, lines));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ComputeCorrespondEpilines(points, 1, invalidFundamentalChannels, lines));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ComputeCorrespondEpilines(points, 1, invalidFundamentalDepth, lines));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ComputeCorrespondEpilines(invalidPointShape, 1, fundamental, lines));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ComputeCorrespondEpilines(invalidPointDepth, 1, fundamental, lines));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ComputeCorrespondEpilines(points, 1, fundamental, points));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ComputeCorrespondEpilines(points, 1, fundamental, fundamental));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.ComputeCorrespondEpilines(null!, 1, fundamental));
        }

        [Fact]
        public void ComputeCorrespondEpilinesOwnedAndCallerOwnedOutputsAgreeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point2f[] sourcePoints =
            {
                new Point2f(10.0F, 20.0F),
                new Point2f(12.0F, 23.0F),
                new Point2f(-4.0F, -8.0F)
            };
            using Mat points = CreatePointMat(sourcePoints);
            using Mat scalarPoints = CreateScalarPointMatrix(
                sourcePoints.Length,
                2,
                MatType.CV_32FC1,
                10.0,
                20.0,
                12.0,
                23.0,
                -4.0,
                -8.0);
            using Mat homogeneousPoints = CreatePointMat(
                new Point3f(10.0F, 20.0F, 1.0F),
                new Point3f(12.0F, 23.0F, 1.0F),
                new Point3f(-4.0F, -8.0F, 1.0F));
            using Mat fundamental = CreateFundamentalMatrix(MatType.CV_64FC1);
            using var callerOwned = new Mat();

            Calib3DCv2.ComputeCorrespondEpilines(points, 1, fundamental, callerOwned);
            using Mat owned = Calib3DCv2.ComputeCorrespondEpilines(points, 1, fundamental);
            using Mat scalarOwned = Calib3DCv2.ComputeCorrespondEpilines(scalarPoints, 1, fundamental);
            using Mat homogeneousOwned = Calib3DCv2.ComputeCorrespondEpilines(homogeneousPoints, 1, fundamental);

            AssertMatrixShape(callerOwned, sourcePoints.Length, 1, MatType.CV_32FC3);
            AssertMatrixShape(owned, sourcePoints.Length, 1, MatType.CV_32FC3);
            AssertMatrixShape(scalarOwned, sourcePoints.Length, 1, MatType.CV_32FC3);
            AssertMatrixShape(homogeneousOwned, sourcePoints.Length, 1, MatType.CV_32FC3);
            AssertArrayNear(callerOwned.ToArray<float>(), owned.ToArray<float>(), 1.0e-6F);
            AssertArrayNear(callerOwned.ToArray<float>(), scalarOwned.ToArray<float>(), 1.0e-6F);
            AssertArrayNear(callerOwned.ToArray<float>(), homogeneousOwned.ToArray<float>(), 1.0e-6F);

            for (int i = 0; i < sourcePoints.Length; ++i)
            {
                Point3f line = owned.GetValue<Point3f>(i);
                AssertLineNear(new Point3f(0.0F, -1.0F, sourcePoints[i].Y), line, 1.0e-6F);
                AssertNear(0.0F, EvaluateLine(line, new Point2f(sourcePoints[i].X + 2.0F, sourcePoints[i].Y)), 1.0e-5F);
            }
        }

        [Fact]
        public void ComputeCorrespondEpilinesReturnsDoubleLinesForDoubleInputWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using Mat points = CreatePoint2dMat(
                new Point2d(10.0, 20.0),
                new Point2d(12.0, 23.0));
            using Mat fundamental = CreateFundamentalMatrix(MatType.CV_64FC1);
            using Mat lines = Calib3DCv2.ComputeCorrespondEpilines(points, 1, fundamental);

            AssertMatrixShape(lines, 2, 1, MatType.CV_64FC3);
            double[] coefficients = lines.ToArray<double>();
            Assert.Equal(6, coefficients.Length);
            AssertLineNear(0, -1.0, 20.0, coefficients, 1.0e-12);
            AssertLineNear(3, -1.0, 23.0, coefficients, 1.0e-12);
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

        private static Mat CreateFundamentalMatrix(int type)
        {
            return CreateScalarPointMatrix(
                3,
                3,
                type,
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

        private static void AssertMatrixShape(Mat value, int rows, int cols, int type)
        {
            Assert.Equal(rows, value.Rows);
            Assert.Equal(cols, value.Cols);
            Assert.Equal(type, value.Type);
        }

        private static void AssertLineNear(Point3f expected, Point3f actual, float tolerance)
        {
            AssertNear(expected.X, actual.X, tolerance);
            AssertNear(expected.Y, actual.Y, tolerance);
            AssertNear(expected.Z, actual.Z, tolerance);
        }

        private static void AssertLineNear(int offset, double expectedY, double expectedZ, double[] actual, double tolerance)
        {
            AssertNear(0.0, actual[offset], tolerance);
            AssertNear(expectedY, actual[offset + 1], tolerance);
            AssertNear(expectedZ, actual[offset + 2], tolerance);
        }

        private static void AssertArrayNear(float[] expected, float[] actual, float tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                AssertNear(expected[i], actual[i], tolerance);
            }
        }

        private static float EvaluateLine(Point3f line, Point2f point)
        {
            return (line.X * point.X) + (line.Y * point.Y) + line.Z;
        }

        private static void AssertNear(float expected, float actual, float tolerance)
        {
            Assert.InRange(Math.Abs(expected - actual), 0.0F, tolerance);
        }

        private static void AssertNear(double expected, double actual, double tolerance)
        {
            Assert.InRange(Math.Abs(expected - actual), 0.0, tolerance);
        }
    }
}
