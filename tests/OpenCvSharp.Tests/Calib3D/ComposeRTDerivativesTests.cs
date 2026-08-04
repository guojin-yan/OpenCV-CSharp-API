using System;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class ComposeRTDerivativesTests
    {
        [Fact]
        public void ComposeRTDerivativesValidateManagedInputsAndAliases()
        {
            Mat[] inputs = CreateDoubleInputs(columnVectors: true);
            Mat[] outputs = CreateOutputs();
            try
            {
                Assert.Throws<ArgumentNullException>(() =>
                    CallExtended(null!, inputs[1], inputs[2], inputs[3], outputs));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ComposeRT(
                        inputs[0],
                        inputs[1],
                        inputs[2],
                        inputs[3],
                        outputs[0],
                        outputs[1],
                        null!,
                        outputs[3],
                        outputs[4],
                        outputs[5],
                        outputs[6],
                        outputs[7],
                        outputs[8],
                        outputs[9]));

                using (var empty = new Mat())
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallExtended(empty, inputs[1], inputs[2], inputs[3], outputs));
                }
                using (var invalidShape = new Mat(2, 2, MatType.CV_64FC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallExtended(invalidShape, inputs[1], inputs[2], inputs[3], outputs));
                }
                using (var multiChannel = new Mat(3, 1, MatType.CV_32FC2))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallExtended(multiChannel, multiChannel, multiChannel, multiChannel, outputs));
                }
                using (var integerVector = new Mat(3, 1, MatType.CV_32SC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallExtended(
                            integerVector,
                            integerVector,
                            integerVector,
                            integerVector,
                            outputs));
                }
                using (Mat mixedDepth = CreateFloatVector(true, 0.1F, 0.2F, 0.3F))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallExtended(inputs[0], mixedDepth, inputs[2], inputs[3], outputs));
                }
                using (Mat mixedOrientation = CreateDoubleVector(false, 0.1, 0.2, 0.3))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallExtended(inputs[0], inputs[1], mixedOrientation, inputs[3], outputs));
                }

                Mat originalRvec3 = outputs[0];
                outputs[0] = inputs[0];
                Assert.Throws<ArgumentException>(() => CallExtended(inputs, outputs));
                outputs[0] = originalRvec3;

                Mat originalDr3Dt1 = outputs[3];
                outputs[3] = outputs[2];
                Assert.Throws<ArgumentException>(() => CallExtended(inputs, outputs));
                outputs[3] = originalDr3Dt1;
            }
            finally
            {
                DisposeAll(inputs);
                DisposeAll(outputs);
            }

            Mat[] disposedInputs = CreateDoubleInputs(columnVectors: true);
            Mat[] disposedInputOutputs = CreateOutputs();
            disposedInputs[0].Dispose();
            try
            {
                Assert.Throws<ObjectDisposedException>(() =>
                    CallExtended(disposedInputs, disposedInputOutputs));
            }
            finally
            {
                DisposeAll(disposedInputs);
                DisposeAll(disposedInputOutputs);
            }

            Mat[] validInputs = CreateDoubleInputs(columnVectors: true);
            Mat[] disposedOutputs = CreateOutputs();
            disposedOutputs[5].Dispose();
            try
            {
                Assert.Throws<ObjectDisposedException>(() =>
                    CallExtended(validInputs, disposedOutputs));
            }
            finally
            {
                DisposeAll(validInputs);
                DisposeAll(disposedOutputs);
            }
        }

        [Fact]
        public void ComposeRTDerivativesMatchCompositionAndStructuralIdentitiesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Mat[] inputs = CreateDoubleInputs(columnVectors: true);
            Mat[] outputs = CreateOutputs();
            try
            {
                CallExtended(inputs, outputs);

                double[] rvec1 = inputs[0].ToArray<double>();
                double[] tvec1 = inputs[1].ToArray<double>();
                double[] rvec2 = inputs[2].ToArray<double>();
                double[] tvec2 = inputs[3].ToArray<double>();
                double[] rotation1 = Rodrigues(rvec1);
                double[] rotation2 = Rodrigues(rvec2);
                double[] expectedRvec3 = RotationVector(Multiply3x3(rotation2, rotation1));
                double[] expectedTvec3 = Add(Multiply3x3Vector(rotation2, tvec1), tvec2);

                AssertArrayNear(expectedRvec3, outputs[0].ToArray<double>(), 1.0e-12);
                AssertArrayNear(expectedTvec3, outputs[1].ToArray<double>(), 1.0e-12);
                AssertVectorOutput(outputs[0], 3, 1, MatType.CV_64FC1);
                AssertVectorOutput(outputs[1], 3, 1, MatType.CV_64FC1);
                for (int index = 2; index < outputs.Length; index++)
                {
                    AssertJacobianOutput(outputs[index], MatType.CV_64FC1);
                }

                AssertZeroMatrix(outputs[3], 0.0);
                AssertZeroMatrix(outputs[5], 0.0);
                AssertZeroMatrix(outputs[6], 0.0);
                AssertIdentityMatrix(outputs[9], 0.0);
                AssertArrayNear(rotation2, outputs[7].ToArray<double>(), 1.0e-12);
            }
            finally
            {
                DisposeAll(inputs);
                DisposeAll(outputs);
            }
        }

        [Fact]
        public void ComposeRTDerivativesPreserveFloatRowVectorLayoutWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat rvec1 = CreateFloatVector(false, 0.12F, -0.08F, 0.05F))
            using (Mat tvec1 = CreateFloatVector(false, 0.4F, -0.3F, 1.2F))
            using (Mat rvec2 = CreateFloatVector(false, -0.07F, 0.09F, 0.11F))
            using (Mat tvec2 = CreateFloatVector(false, -0.2F, 0.5F, 0.7F))
            {
                ComposeRTDerivativesResult result =
                    Calib3DCv2.ComposeRT(rvec1, tvec1, rvec2, tvec2);
                Mat[] matrices = GetResultMatrices(result);
                try
                {
                    AssertVectorOutput(result.Rvec3, 1, 3, MatType.CV_32FC1);
                    AssertVectorOutput(result.Tvec3, 1, 3, MatType.CV_32FC1);
                    for (int index = 2; index < matrices.Length; index++)
                    {
                        AssertJacobianOutput(matrices[index], MatType.CV_32FC1);
                    }
                    AssertZeroMatrix(result.Dr3Dt1, 1.0e-6);
                    AssertZeroMatrix(result.Dr3Dt2, 1.0e-6);
                    AssertZeroMatrix(result.Dt3Dr1, 1.0e-6);
                    AssertIdentityMatrix(result.Dt3Dt2, 1.0e-6);
                }
                finally
                {
                    DisposeAll(matrices);
                }
            }
        }

        [Fact]
        public void ComposeRTDerivativesMatchCentralFiniteDifferencesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Mat[] inputs = CreateDoubleInputs(columnVectors: true);
            Mat[] outputs = CreateOutputs();
            try
            {
                CallExtended(inputs, outputs);
                AssertFiniteDifferences(inputs, 0, outputs[2], outputs[6]);
                AssertFiniteDifferences(inputs, 1, outputs[3], outputs[7]);
                AssertFiniteDifferences(inputs, 2, outputs[4], outputs[8]);
                AssertFiniteDifferences(inputs, 3, outputs[5], outputs[9]);
            }
            finally
            {
                DisposeAll(inputs);
                DisposeAll(outputs);
            }
        }

        [Fact]
        public void ComposeRTDerivativeOwnedAndCallerOwnedResultsAgreeAndPreserveInputsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Mat[] inputs = CreateDoubleInputs(columnVectors: true);
            Mat[] callerOutputs = CreateOutputs();
            double[][] originalInputs = new double[inputs.Length][];
            for (int index = 0; index < inputs.Length; index++)
            {
                originalInputs[index] = inputs[index].ToArray<double>();
            }

            try
            {
                CallExtended(inputs, callerOutputs);
                ComposeRTDerivativesResult owned =
                    Calib3DCv2.ComposeRT(inputs[0], inputs[1], inputs[2], inputs[3]);
                Mat[] ownedOutputs = GetResultMatrices(owned);
                try
                {
                    using (var basicRvec3 = new Mat())
                    using (var basicTvec3 = new Mat())
                    {
                        Calib3DCv2.ComposeRT(
                            inputs[0],
                            inputs[1],
                            inputs[2],
                            inputs[3],
                            basicRvec3,
                            basicTvec3);
                        AssertMatricesNear(basicRvec3, callerOutputs[0], 0.0);
                        AssertMatricesNear(basicTvec3, callerOutputs[1], 0.0);
                    }

                    for (int index = 0; index < callerOutputs.Length; index++)
                    {
                        AssertMatricesNear(callerOutputs[index], ownedOutputs[index], 0.0);
                    }
                    for (int index = 0; index < inputs.Length; index++)
                    {
                        Assert.Equal(originalInputs[index], inputs[index].ToArray<double>());
                    }

                    DisposeAll(ownedOutputs);
                    for (int index = 0; index < ownedOutputs.Length; index++)
                    {
                        int captured = index;
                        Assert.Throws<ObjectDisposedException>(() =>
                            ownedOutputs[captured].GetValue<double>(0));
                    }
                }
                finally
                {
                    DisposeAll(ownedOutputs);
                }
            }
            finally
            {
                DisposeAll(inputs);
                DisposeAll(callerOutputs);
            }
        }

        private static void AssertFiniteDifferences(
            Mat[] inputs,
            int perturbedInputIndex,
            Mat rotationJacobian,
            Mat translationJacobian)
        {
            const double epsilon = 1.0e-6;
            Mat perturbed = inputs[perturbedInputIndex];
            using (var plusRvec = new Mat())
            using (var plusTvec = new Mat())
            using (var minusRvec = new Mat())
            using (var minusTvec = new Mat())
            {
                for (int variable = 0; variable < 3; variable++)
                {
                    double original = perturbed.GetValue<double>(variable);
                    perturbed.SetValue(variable, original + epsilon);
                    Calib3DCv2.ComposeRT(
                        inputs[0],
                        inputs[1],
                        inputs[2],
                        inputs[3],
                        plusRvec,
                        plusTvec);
                    double[] plusRotation = plusRvec.ToArray<double>();
                    double[] plusTranslation = plusTvec.ToArray<double>();

                    perturbed.SetValue(variable, original - epsilon);
                    Calib3DCv2.ComposeRT(
                        inputs[0],
                        inputs[1],
                        inputs[2],
                        inputs[3],
                        minusRvec,
                        minusTvec);
                    double[] minusRotation = minusRvec.ToArray<double>();
                    double[] minusTranslation = minusTvec.ToArray<double>();
                    perturbed.SetValue(variable, original);

                    for (int output = 0; output < 3; output++)
                    {
                        double rotationDifference =
                            (plusRotation[output] - minusRotation[output]) /
                            (2.0 * epsilon);
                        double translationDifference =
                            (plusTranslation[output] - minusTranslation[output]) /
                            (2.0 * epsilon);
                        Assert.InRange(
                            Math.Abs(
                                rotationDifference -
                                rotationJacobian.GetValue<double>((output * 3) + variable)),
                            0.0,
                            1.0e-8);
                        Assert.InRange(
                            Math.Abs(
                                translationDifference -
                                translationJacobian.GetValue<double>((output * 3) + variable)),
                            0.0,
                            1.0e-8);
                    }
                }
            }
        }

        private static Mat[] CreateDoubleInputs(bool columnVectors)
        {
            return new[]
            {
                CreateDoubleVector(columnVectors, 0.17, -0.11, 0.08),
                CreateDoubleVector(columnVectors, 0.45, -0.35, 1.1),
                CreateDoubleVector(columnVectors, -0.09, 0.13, 0.16),
                CreateDoubleVector(columnVectors, -0.25, 0.55, 0.8)
            };
        }

        private static Mat[] CreateOutputs()
        {
            return new[]
            {
                new Mat(),
                new Mat(),
                new Mat(),
                new Mat(),
                new Mat(),
                new Mat(),
                new Mat(),
                new Mat(),
                new Mat(),
                new Mat()
            };
        }

        private static Mat CreateDoubleVector(
            bool columnVector,
            params double[] values)
        {
            var vector = new Mat(
                columnVector ? 3 : 1,
                columnVector ? 1 : 3,
                MatType.CV_64FC1);
            vector.CopyFrom<double>(values);
            return vector;
        }

        private static Mat CreateFloatVector(
            bool columnVector,
            params float[] values)
        {
            var vector = new Mat(
                columnVector ? 3 : 1,
                columnVector ? 1 : 3,
                MatType.CV_32FC1);
            vector.CopyFrom<float>(values);
            return vector;
        }

        private static void CallExtended(Mat[] inputs, Mat[] outputs)
        {
            CallExtended(inputs[0], inputs[1], inputs[2], inputs[3], outputs);
        }

        private static void CallExtended(
            Mat rvec1,
            Mat tvec1,
            Mat rvec2,
            Mat tvec2,
            Mat[] outputs)
        {
            Calib3DCv2.ComposeRT(
                rvec1,
                tvec1,
                rvec2,
                tvec2,
                outputs[0],
                outputs[1],
                outputs[2],
                outputs[3],
                outputs[4],
                outputs[5],
                outputs[6],
                outputs[7],
                outputs[8],
                outputs[9]);
        }

        private static Mat[] GetResultMatrices(ComposeRTDerivativesResult result)
        {
            return new[]
            {
                result.Rvec3,
                result.Tvec3,
                result.Dr3Dr1,
                result.Dr3Dt1,
                result.Dr3Dr2,
                result.Dr3Dt2,
                result.Dt3Dr1,
                result.Dt3Dt1,
                result.Dt3Dr2,
                result.Dt3Dt2
            };
        }

        private static void AssertVectorOutput(Mat vector, int rows, int columns, int type)
        {
            Assert.Equal(rows, vector.Rows);
            Assert.Equal(columns, vector.Cols);
            Assert.Equal(type, vector.Type);
        }

        private static void AssertJacobianOutput(Mat jacobian, int type)
        {
            Assert.Equal(3, jacobian.Rows);
            Assert.Equal(3, jacobian.Cols);
            Assert.Equal(type, jacobian.Type);
        }

        private static void AssertZeroMatrix(Mat matrix, double tolerance)
        {
            double[] values = matrix.Type == MatType.CV_32FC1
                ? Array.ConvertAll(matrix.ToArray<float>(), value => (double)value)
                : matrix.ToArray<double>();
            for (int index = 0; index < values.Length; index++)
            {
                Assert.InRange(Math.Abs(values[index]), 0.0, tolerance);
            }
        }

        private static void AssertIdentityMatrix(Mat matrix, double tolerance)
        {
            double[] values = matrix.Type == MatType.CV_32FC1
                ? Array.ConvertAll(matrix.ToArray<float>(), value => (double)value)
                : matrix.ToArray<double>();
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    double expected = row == column ? 1.0 : 0.0;
                    Assert.InRange(
                        Math.Abs(values[(row * 3) + column] - expected),
                        0.0,
                        tolerance);
                }
            }
        }

        private static void AssertMatricesNear(Mat expected, Mat actual, double tolerance)
        {
            Assert.Equal(expected.Rows, actual.Rows);
            Assert.Equal(expected.Cols, actual.Cols);
            Assert.Equal(expected.Type, actual.Type);
            double[] expectedValues = expected.ToArray<double>();
            double[] actualValues = actual.ToArray<double>();
            AssertArrayNear(expectedValues, actualValues, tolerance);
        }

        private static void AssertArrayNear(
            double[] expected,
            double[] actual,
            double tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int index = 0; index < expected.Length; index++)
            {
                Assert.InRange(
                    Math.Abs(expected[index] - actual[index]),
                    0.0,
                    tolerance);
            }
        }

        private static double[] Rodrigues(double[] vector)
        {
            double theta = Math.Sqrt(
                (vector[0] * vector[0]) +
                (vector[1] * vector[1]) +
                (vector[2] * vector[2]));
            double x = vector[0] / theta;
            double y = vector[1] / theta;
            double z = vector[2] / theta;
            double cosine = Math.Cos(theta);
            double sine = Math.Sin(theta);
            double oneMinusCosine = 1.0 - cosine;
            return new[]
            {
                cosine + (x * x * oneMinusCosine),
                (x * y * oneMinusCosine) - (z * sine),
                (x * z * oneMinusCosine) + (y * sine),
                (y * x * oneMinusCosine) + (z * sine),
                cosine + (y * y * oneMinusCosine),
                (y * z * oneMinusCosine) - (x * sine),
                (z * x * oneMinusCosine) - (y * sine),
                (z * y * oneMinusCosine) + (x * sine),
                cosine + (z * z * oneMinusCosine)
            };
        }

        private static double[] RotationVector(double[] rotation)
        {
            double cosine = Math.Max(
                -1.0,
                Math.Min(
                    1.0,
                    (rotation[0] + rotation[4] + rotation[8] - 1.0) / 2.0));
            double theta = Math.Acos(cosine);
            double scale = theta / (2.0 * Math.Sin(theta));
            return new[]
            {
                (rotation[7] - rotation[5]) * scale,
                (rotation[2] - rotation[6]) * scale,
                (rotation[3] - rotation[1]) * scale
            };
        }

        private static double[] Multiply3x3(double[] left, double[] right)
        {
            var result = new double[9];
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    for (int inner = 0; inner < 3; inner++)
                    {
                        result[(row * 3) + column] +=
                            left[(row * 3) + inner] *
                            right[(inner * 3) + column];
                    }
                }
            }
            return result;
        }

        private static double[] Multiply3x3Vector(double[] matrix, double[] vector)
        {
            return new[]
            {
                (matrix[0] * vector[0]) + (matrix[1] * vector[1]) + (matrix[2] * vector[2]),
                (matrix[3] * vector[0]) + (matrix[4] * vector[1]) + (matrix[5] * vector[2]),
                (matrix[6] * vector[0]) + (matrix[7] * vector[1]) + (matrix[8] * vector[2])
            };
        }

        private static double[] Add(double[] left, double[] right)
        {
            return new[]
            {
                left[0] + right[0],
                left[1] + right[1],
                left[2] + right[2]
            };
        }

        private static void DisposeAll(Mat[] matrices)
        {
            for (int index = 0; index < matrices.Length; index++)
            {
                matrices[index].Dispose();
            }
        }
    }
}
