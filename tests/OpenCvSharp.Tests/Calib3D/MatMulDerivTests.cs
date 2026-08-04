using System;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class MatMulDerivTests
    {
        [Fact]
        public void MatMulDerivValidatesManagedInputs()
        {
            using (Mat a = CreateDoubleMat(2, 3, 1, 2, 3, 4, 5, 6))
            using (Mat b = CreateDoubleMat(3, 2, 7, 8, 9, 10, 11, 12))
            using (var dABdA = new Mat())
            using (var dABdB = new Mat())
            using (var empty = new Mat())
            using (var multiChannelA = new Mat(2, 3, MatType.CV_32FC2))
            using (var multiChannelB = new Mat(3, 2, MatType.CV_32FC2))
            using (var integerA = new Mat(2, 3, MatType.CV_32SC1))
            using (var integerB = new Mat(3, 2, MatType.CV_32SC1))
            using (var floatB = new Mat(3, 2, MatType.CV_32FC1))
            using (var incompatibleB = new Mat(4, 2, MatType.CV_64FC1))
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.MatMulDeriv(null!, b, dABdA, dABdB));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.MatMulDeriv(a, null!, dABdA, dABdB));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.MatMulDeriv(a, b, null!, dABdB));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.MatMulDeriv(a, b, dABdA, null!));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.MatMulDeriv(empty, b, dABdA, dABdB));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.MatMulDeriv(a, empty, dABdA, dABdB));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.MatMulDeriv(multiChannelA, multiChannelB, dABdA, dABdB));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.MatMulDeriv(integerA, integerB, dABdA, dABdB));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.MatMulDeriv(a, floatB, dABdA, dABdB));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.MatMulDeriv(a, incompatibleB, dABdA, dABdB));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.MatMulDeriv(a, b, dABdA, dABdA));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.MatMulDeriv(a, b, a, dABdB));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.MatMulDeriv(a, b, dABdA, b));
            }

            Mat disposedInput = CreateDoubleMat(2, 3, 1, 2, 3, 4, 5, 6);
            disposedInput.Dispose();
            using (Mat b = CreateDoubleMat(3, 2, 7, 8, 9, 10, 11, 12))
            using (var dABdA = new Mat())
            using (var dABdB = new Mat())
            {
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.MatMulDeriv(disposedInput, b, dABdA, dABdB));
            }

            Mat disposedOutput = new Mat();
            disposedOutput.Dispose();
            using (Mat a = CreateDoubleMat(2, 3, 1, 2, 3, 4, 5, 6))
            using (Mat b = CreateDoubleMat(3, 2, 7, 8, 9, 10, 11, 12))
            using (var dABdB = new Mat())
            {
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.MatMulDeriv(a, b, disposedOutput, dABdB));
            }
        }

        [Fact]
        public void MatMulDerivProducesExactDoubleJacobiansWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat a = CreateDoubleMat(2, 3, 1, 2, 3, 4, 5, 6))
            using (Mat b = CreateDoubleMat(3, 2, 7, 8, 9, 10, 11, 12))
            using (var dABdA = new Mat())
            using (var dABdB = new Mat())
            {
                Calib3DCv2.MatMulDeriv(a, b, dABdA, dABdB);

                AssertMatrix(
                    dABdA,
                    4,
                    6,
                    MatType.CV_64FC1,
                    new double[]
                    {
                        7, 9, 11, 0, 0, 0,
                        8, 10, 12, 0, 0, 0,
                        0, 0, 0, 7, 9, 11,
                        0, 0, 0, 8, 10, 12
                    },
                    0.0);
                AssertMatrix(
                    dABdB,
                    4,
                    6,
                    MatType.CV_64FC1,
                    new double[]
                    {
                        1, 0, 2, 0, 3, 0,
                        0, 1, 0, 2, 0, 3,
                        4, 0, 5, 0, 6, 0,
                        0, 4, 0, 5, 0, 6
                    },
                    0.0);
            }
        }

        [Fact]
        public void MatMulDerivPreservesFloatShapeTypeAndValuesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat a = CreateFloatMat(1, 2, 1.5F, -2.0F))
            using (Mat b = CreateFloatMat(2, 3, 3, 4, 5, 6, 7, 8))
            {
                Calib3DCv2.MatMulDeriv(a, b, out Mat dABdA, out Mat dABdB);
                using (dABdA)
                using (dABdB)
                {
                    AssertMatrix(
                        dABdA,
                        3,
                        2,
                        MatType.CV_32FC1,
                        new float[]
                        {
                            3, 6,
                            4, 7,
                            5, 8
                        },
                        1.0e-6F);
                    AssertMatrix(
                        dABdB,
                        3,
                        6,
                        MatType.CV_32FC1,
                        new float[]
                        {
                            1.5F, 0, 0, -2, 0, 0,
                            0, 1.5F, 0, 0, -2, 0,
                            0, 0, 1.5F, 0, 0, -2
                        },
                        1.0e-6F);
                }
            }
        }

        [Fact]
        public void MatMulDerivMatchesCentralFiniteDifferencesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat a = CreateDoubleMat(2, 2, 0.7, -1.2, 2.3, 0.4))
            using (Mat b = CreateDoubleMat(2, 3, 1.1, -0.8, 0.5, 2.0, -1.5, 0.9))
            {
                Calib3DCv2.MatMulDeriv(a, b, out Mat dABdA, out Mat dABdB);
                using (dABdA)
                using (dABdB)
                {
                    AssertFiniteDifferences(a, b, a, dABdA);
                    AssertFiniteDifferences(a, b, b, dABdB);
                }
            }
        }

        [Fact]
        public void MatMulDerivOwnedAndCallerOwnedOutputsAgreeAndPreserveInputsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            double[] aValues = { 1.25, -2.5, 3.75, 4.5, -5.25, 6.0 };
            double[] bValues = { -0.5, 1.5, 2.25, -3.0, 4.75, 5.5 };
            using (Mat a = CreateDoubleMat(2, 3, aValues))
            using (Mat b = CreateDoubleMat(3, 2, bValues))
            using (var callerDABdA = new Mat())
            using (var callerDABdB = new Mat())
            {
                Calib3DCv2.MatMulDeriv(a, b, callerDABdA, callerDABdB);
                Calib3DCv2.MatMulDeriv(a, b, out Mat ownedDABdA, out Mat ownedDABdB);

                AssertMatricesNear(callerDABdA, ownedDABdA, 0.0);
                AssertMatricesNear(callerDABdB, ownedDABdB, 0.0);
                Assert.Equal(aValues, a.ToArray<double>());
                Assert.Equal(bValues, b.ToArray<double>());

                ownedDABdA.Dispose();
                ownedDABdB.Dispose();
                Assert.Throws<ObjectDisposedException>(() => ownedDABdA.GetValue<double>(0));
                Assert.Throws<ObjectDisposedException>(() => ownedDABdB.GetValue<double>(0));
            }
        }

        private static void AssertFiniteDifferences(
            Mat a,
            Mat b,
            Mat perturbed,
            Mat jacobian)
        {
            const double epsilon = 1.0e-6;
            for (int variable = 0; variable < perturbed.Rows * perturbed.Cols; variable++)
            {
                double original = perturbed.GetValue<double>(variable);
                perturbed.SetValue(variable, original + epsilon);
                double[] plus = Multiply(a, b);
                perturbed.SetValue(variable, original - epsilon);
                double[] minus = Multiply(a, b);
                perturbed.SetValue(variable, original);

                for (int output = 0; output < plus.Length; output++)
                {
                    double finiteDifference = (plus[output] - minus[output]) / (2.0 * epsilon);
                    double analytic = jacobian.GetValue<double>((output * jacobian.Cols) + variable);
                    Assert.InRange(Math.Abs(finiteDifference - analytic), 0.0, 1.0e-9);
                }
            }
        }

        private static double[] Multiply(Mat a, Mat b)
        {
            var result = new double[a.Rows * b.Cols];
            for (int row = 0; row < a.Rows; row++)
            {
                for (int column = 0; column < b.Cols; column++)
                {
                    double value = 0.0;
                    for (int inner = 0; inner < a.Cols; inner++)
                    {
                        value +=
                            a.GetValue<double>((row * a.Cols) + inner) *
                            b.GetValue<double>((inner * b.Cols) + column);
                    }
                    result[(row * b.Cols) + column] = value;
                }
            }
            return result;
        }

        private static Mat CreateDoubleMat(int rows, int columns, params double[] values)
        {
            var matrix = new Mat(rows, columns, MatType.CV_64FC1);
            matrix.CopyFrom<double>(values);
            return matrix;
        }

        private static Mat CreateFloatMat(int rows, int columns, params float[] values)
        {
            var matrix = new Mat(rows, columns, MatType.CV_32FC1);
            matrix.CopyFrom<float>(values);
            return matrix;
        }

        private static void AssertMatrix(
            Mat matrix,
            int rows,
            int columns,
            int type,
            double[] expected,
            double tolerance)
        {
            Assert.Equal(rows, matrix.Rows);
            Assert.Equal(columns, matrix.Cols);
            Assert.Equal(type, matrix.Type);
            double[] actual = matrix.ToArray<double>();
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.InRange(Math.Abs(expected[i] - actual[i]), 0.0, tolerance);
            }
        }

        private static void AssertMatrix(
            Mat matrix,
            int rows,
            int columns,
            int type,
            float[] expected,
            float tolerance)
        {
            Assert.Equal(rows, matrix.Rows);
            Assert.Equal(columns, matrix.Cols);
            Assert.Equal(type, matrix.Type);
            float[] actual = matrix.ToArray<float>();
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.InRange(Math.Abs(expected[i] - actual[i]), 0.0F, tolerance);
            }
        }

        private static void AssertMatricesNear(Mat expected, Mat actual, double tolerance)
        {
            Assert.Equal(expected.Rows, actual.Rows);
            Assert.Equal(expected.Cols, actual.Cols);
            Assert.Equal(expected.Type, actual.Type);
            double[] expectedValues = expected.ToArray<double>();
            double[] actualValues = actual.ToArray<double>();
            for (int i = 0; i < expectedValues.Length; i++)
            {
                Assert.InRange(
                    Math.Abs(expectedValues[i] - actualValues[i]),
                    0.0,
                    tolerance);
            }
        }
    }
}
