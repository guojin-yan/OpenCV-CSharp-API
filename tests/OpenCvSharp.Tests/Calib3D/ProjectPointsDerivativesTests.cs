using System;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class ProjectPointsDerivativesTests
    {
        [Fact]
        public void ProjectPointsDerivativesValidateManagedInputsAliasesAndDisposedState()
        {
            Mat[] inputs = CreateDoubleInputs();
            Mat[] outputs = CreateOutputs();
            try
            {
                Assert.Throws<ArgumentNullException>(() =>
                    CallSeparated(null!, inputs[1], inputs[2], inputs[3], inputs[4], outputs));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ProjectPoints(
                        inputs[0],
                        inputs[1],
                        inputs[2],
                        inputs[3],
                        inputs[4],
                        outputs[0],
                        outputs[1],
                        outputs[2],
                        outputs[3],
                        outputs[4],
                        outputs[5],
                        null!));

                using (var invalidObjectPoints = new Mat(2, 2, MatType.CV_64FC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallSeparated(
                            invalidObjectPoints,
                            inputs[1],
                            inputs[2],
                            inputs[3],
                            inputs[4],
                            outputs));
                }
                using (var integerObjectPoints = new Mat(3, 3, MatType.CV_32SC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallSeparated(
                            integerObjectPoints,
                            inputs[1],
                            inputs[2],
                            inputs[3],
                            inputs[4],
                            outputs));
                }
                using (var invalidRotation = new Mat(2, 1, MatType.CV_64FC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallSeparated(
                            inputs[0],
                            invalidRotation,
                            inputs[2],
                            inputs[3],
                            inputs[4],
                            outputs));
                }
                using (var invalidTranslation = new Mat(1, 3, MatType.CV_32SC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallSeparated(
                            inputs[0],
                            inputs[1],
                            invalidTranslation,
                            inputs[3],
                            inputs[4],
                            outputs));
                }
                using (var invalidCameraMatrix = new Mat(2, 3, MatType.CV_64FC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallSeparated(
                            inputs[0],
                            inputs[1],
                            inputs[2],
                            invalidCameraMatrix,
                            inputs[4],
                            outputs));
                }
                using (var invalidDistortion = new Mat(1, 6, MatType.CV_64FC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallSeparated(
                            inputs[0],
                            inputs[1],
                            inputs[2],
                            inputs[3],
                            invalidDistortion,
                            outputs));
                }

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    CallSeparated(inputs, outputs, double.NaN));

                Mat originalImagePoints = outputs[0];
                outputs[0] = inputs[0];
                Assert.Throws<ArgumentException>(() => CallSeparated(inputs, outputs));
                outputs[0] = originalImagePoints;

                Mat originalDpDc = outputs[4];
                outputs[4] = outputs[3];
                Assert.Throws<ArgumentException>(() => CallSeparated(inputs, outputs));
                outputs[4] = originalDpDc;
            }
            finally
            {
                DisposeAll(inputs);
                DisposeAll(outputs);
            }

            Mat[] disposedInputs = CreateDoubleInputs();
            Mat[] disposedInputOutputs = CreateOutputs();
            disposedInputs[1].Dispose();
            try
            {
                Assert.Throws<ObjectDisposedException>(() =>
                    CallSeparated(disposedInputs, disposedInputOutputs));
            }
            finally
            {
                DisposeAll(disposedInputs);
                DisposeAll(disposedInputOutputs);
            }

            Mat[] validInputs = CreateDoubleInputs();
            Mat[] disposedOutputs = CreateOutputs();
            disposedOutputs[6].Dispose();
            try
            {
                Assert.Throws<ObjectDisposedException>(() =>
                    CallSeparated(validInputs, disposedOutputs));
            }
            finally
            {
                DisposeAll(validInputs);
                DisposeAll(disposedOutputs);
            }
        }

        [Fact]
        public void ProjectPointsDerivativesHaveExpectedShapesAndMatchCombinedJacobian()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Mat[] inputs = CreateDoubleInputs();
            Mat[] separated = CreateOutputs();
            using (var combinedImagePoints = new Mat())
            using (var combinedJacobian = new Mat())
            {
                try
                {
                    CallSeparated(inputs, separated);
                    Calib3DCv2.ProjectPoints(
                        inputs[0],
                        inputs[1],
                        inputs[2],
                        inputs[3],
                        inputs[4],
                        combinedImagePoints,
                        combinedJacobian);

                    AssertImagePoints(separated[0], 3, MatType.CV_64FC2);
                    AssertDerivative(separated[1], 6, 3);
                    AssertDerivative(separated[2], 6, 3);
                    AssertDerivative(separated[3], 6, 2);
                    AssertDerivative(separated[4], 6, 2);
                    AssertDerivative(separated[5], 6, 5);
                    AssertDerivative(separated[6], 6, 9);
                    AssertMatricesNear(combinedImagePoints, separated[0], 0.0);
                    Assert.Equal(6, combinedJacobian.Rows);
                    Assert.Equal(15, combinedJacobian.Cols);
                    Assert.Equal(MatType.CV_64FC1, combinedJacobian.Type);
                    AssertCombinedBlocks(
                        combinedJacobian,
                        separated[1],
                        separated[2],
                        separated[3],
                        separated[4],
                        separated[5],
                        0.0);
                }
                finally
                {
                    DisposeAll(inputs);
                    DisposeAll(separated);
                }
            }
        }

        [Fact]
        public void ProjectPointsDerivativesSupportFloatEmptyDistortionAndFixedAspectRatio()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            const double aspectRatio = 1.25;
            Mat[] inputs = CreateFloatInputsWithEmptyDistortion();
            ProjectPointsDerivativesResult result =
                Calib3DCv2.ProjectPointsWithDerivatives(
                    inputs[0],
                    inputs[1],
                    inputs[2],
                    inputs[3],
                    inputs[4],
                    aspectRatio);
            Mat[] owned = GetResultMatrices(result);
            using (var combinedImagePoints = new Mat())
            using (var combinedJacobian = new Mat())
            {
                try
                {
                    Calib3DCv2.ProjectPoints(
                        inputs[0],
                        inputs[1],
                        inputs[2],
                        inputs[3],
                        inputs[4],
                        combinedImagePoints,
                        combinedJacobian,
                        aspectRatio);

                    AssertImagePoints(result.ImagePoints, 3, MatType.CV_32FC2);
                    AssertDerivative(result.DpDr, 6, 3);
                    AssertDerivative(result.DpDt, 6, 3);
                    AssertDerivative(result.DpDf, 6, 2);
                    AssertDerivative(result.DpDc, 6, 2);
                    AssertDerivative(result.DpDk, 6, 5);
                    AssertDerivative(result.DpDo, 6, 9);
                    AssertMatricesNear(combinedImagePoints, result.ImagePoints, 0.0);
                    Assert.Equal(15, combinedJacobian.Cols);
                    AssertCombinedBlocks(
                        combinedJacobian,
                        result.DpDr,
                        result.DpDt,
                        result.DpDf,
                        result.DpDc,
                        result.DpDk,
                        1.0e-12);
                }
                finally
                {
                    DisposeAll(inputs);
                    DisposeAll(owned);
                }
            }
        }

        [Fact]
        public void ProjectPointsDerivativesMatchCentralFiniteDifferencesForAllParameters()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Mat[] inputs = CreateDoubleInputs();
            ProjectPointsDerivativesResult result =
                Calib3DCv2.ProjectPointsWithDerivatives(
                    inputs[0],
                    inputs[1],
                    inputs[2],
                    inputs[3],
                    inputs[4]);
            Mat[] owned = GetResultMatrices(result);
            try
            {
                AssertFiniteDifferences(inputs, inputs[1], new[] { 0, 1, 2 }, result.DpDr);
                AssertFiniteDifferences(inputs, inputs[2], new[] { 0, 1, 2 }, result.DpDt);
                AssertFiniteDifferences(inputs, inputs[3], new[] { 0, 4 }, result.DpDf);
                AssertFiniteDifferences(inputs, inputs[3], new[] { 2, 5 }, result.DpDc);
                AssertFiniteDifferences(inputs, inputs[4], new[] { 0, 1, 2, 3, 4 }, result.DpDk);
                AssertFiniteDifferences(
                    inputs,
                    inputs[0],
                    new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
                    result.DpDo);
            }
            finally
            {
                DisposeAll(inputs);
                DisposeAll(owned);
            }
        }

        [Fact]
        public void ProjectPointsDerivativeObjectBlockIsSparseAndOwnedMatchesCallerOwned()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Mat[] inputs = CreateDoubleInputs();
            Mat[] callerOwned = CreateOutputs();
            double[][] originalInputs = new double[inputs.Length][];
            for (int index = 0; index < inputs.Length; index++)
            {
                originalInputs[index] = inputs[index].ToArray<double>();
            }

            ProjectPointsDerivativesResult result = default;
            Mat[] owned = Array.Empty<Mat>();
            try
            {
                CallSeparated(inputs, callerOwned);
                result = Calib3DCv2.ProjectPointsWithDerivatives(
                    inputs[0],
                    inputs[1],
                    inputs[2],
                    inputs[3],
                    inputs[4]);
                owned = GetResultMatrices(result);

                for (int index = 0; index < callerOwned.Length; index++)
                {
                    AssertMatricesNear(callerOwned[index], owned[index], 0.0);
                }
                for (int index = 0; index < inputs.Length; index++)
                {
                    Assert.Equal(originalInputs[index], inputs[index].ToArray<double>());
                }

                double[] objectJacobian = result.DpDo.ToArray<double>();
                bool foundNonZero = false;
                int columns = result.DpDo.Cols;
                for (int row = 0; row < result.DpDo.Rows; row++)
                {
                    int pointIndex = row / 2;
                    for (int column = 0; column < columns; column++)
                    {
                        double value = objectJacobian[(row * columns) + column];
                        bool belongsToPoint =
                            column >= pointIndex * 3 &&
                            column < (pointIndex + 1) * 3;
                        if (belongsToPoint)
                        {
                            foundNonZero |= Math.Abs(value) > 1.0e-12;
                        }
                        else
                        {
                            Assert.InRange(Math.Abs(value), 0.0, 1.0e-14);
                        }
                    }
                }
                Assert.True(foundNonZero);

                DisposeAll(owned);
                foreach (Mat matrix in owned)
                {
                    Assert.Throws<ObjectDisposedException>(() =>
                        matrix.GetValue<double>(0));
                }
            }
            finally
            {
                DisposeAll(inputs);
                DisposeAll(callerOwned);
                DisposeAll(owned);
            }
        }

        private static void AssertFiniteDifferences(
            Mat[] inputs,
            Mat parameter,
            int[] flatIndices,
            Mat jacobian)
        {
            const double epsilon = 1.0e-6;
            const double tolerance = 2.0e-4;
            double[] analytic = jacobian.ToArray<double>();
            int variableCount = flatIndices.Length;

            for (int variable = 0; variable < variableCount; variable++)
            {
                int flatIndex = flatIndices[variable];
                double original = parameter.GetValue<double>(flatIndex);
                parameter.SetValue(flatIndex, original + epsilon);
                double[] plus = ProjectValues(inputs);
                parameter.SetValue(flatIndex, original - epsilon);
                double[] minus = ProjectValues(inputs);
                parameter.SetValue(flatIndex, original);

                for (int output = 0; output < plus.Length; output++)
                {
                    double numerical = (plus[output] - minus[output]) / (2.0 * epsilon);
                    double expected = analytic[(output * variableCount) + variable];
                    Assert.InRange(Math.Abs(numerical - expected), 0.0, tolerance);
                }
            }
        }

        private static double[] ProjectValues(Mat[] inputs)
        {
            using (Mat projected = Calib3DCv2.ProjectPoints(
                inputs[0],
                inputs[1],
                inputs[2],
                inputs[3],
                inputs[4]))
            {
                return projected.ToArray<double>();
            }
        }

        private static Mat[] CreateDoubleInputs()
        {
            return new[]
            {
                CreateMat(
                    3,
                    3,
                    MatType.CV_64FC1,
                    -0.7, -0.4, 3.2,
                    0.5, -0.2, 4.1,
                    0.2, 0.8, 5.0),
                CreateMat(3, 1, MatType.CV_64FC1, 0.08, -0.05, 0.03),
                CreateMat(1, 3, MatType.CV_64FC1, 0.15, -0.12, 0.7),
                CreateMat(
                    3,
                    3,
                    MatType.CV_64FC1,
                    500.0, 0.0, 320.0,
                    0.0, 520.0, 240.0,
                    0.0, 0.0, 1.0),
                CreateMat(
                    1,
                    5,
                    MatType.CV_64FC1,
                    0.01, -0.005, 0.0002, -0.0003, 0.0001)
            };
        }

        private static Mat[] CreateFloatInputsWithEmptyDistortion()
        {
            return new[]
            {
                CreateMat(
                    3,
                    3,
                    MatType.CV_32FC1,
                    -0.7F, -0.4F, 3.2F,
                    0.5F, -0.2F, 4.1F,
                    0.2F, 0.8F, 5.0F),
                CreateMat(1, 3, MatType.CV_32FC1, 0.08F, -0.05F, 0.03F),
                CreateMat(3, 1, MatType.CV_32FC1, 0.15F, -0.12F, 0.7F),
                CreateMat(
                    3,
                    3,
                    MatType.CV_32FC1,
                    500.0F, 0.0F, 320.0F,
                    0.0F, 400.0F, 240.0F,
                    0.0F, 0.0F, 1.0F),
                new Mat()
            };
        }

        private static Mat CreateMat(
            int rows,
            int columns,
            int type,
            params double[] values)
        {
            var result = new Mat(rows, columns, type);
            result.CopyFrom<double>(values);
            return result;
        }

        private static Mat CreateMat(
            int rows,
            int columns,
            int type,
            params float[] values)
        {
            var result = new Mat(rows, columns, type);
            result.CopyFrom<float>(values);
            return result;
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
                new Mat()
            };
        }

        private static Mat[] GetResultMatrices(ProjectPointsDerivativesResult result)
        {
            return new[]
            {
                result.ImagePoints,
                result.DpDr,
                result.DpDt,
                result.DpDf,
                result.DpDc,
                result.DpDk,
                result.DpDo
            };
        }

        private static void CallSeparated(
            Mat[] inputs,
            Mat[] outputs,
            double aspectRatio = 0)
        {
            CallSeparated(
                inputs[0],
                inputs[1],
                inputs[2],
                inputs[3],
                inputs[4],
                outputs,
                aspectRatio);
        }

        private static void CallSeparated(
            Mat objectPoints,
            Mat rvec,
            Mat tvec,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat[] outputs,
            double aspectRatio = 0)
        {
            Calib3DCv2.ProjectPoints(
                objectPoints,
                rvec,
                tvec,
                cameraMatrix,
                distCoeffs,
                outputs[0],
                outputs[1],
                outputs[2],
                outputs[3],
                outputs[4],
                outputs[5],
                outputs[6],
                aspectRatio);
        }

        private static void AssertImagePoints(Mat matrix, int pointCount, int type)
        {
            Assert.Equal(pointCount, matrix.Rows);
            Assert.Equal(1, matrix.Cols);
            Assert.Equal(type, matrix.Type);
        }

        private static void AssertDerivative(Mat matrix, int rows, int columns)
        {
            Assert.Equal(rows, matrix.Rows);
            Assert.Equal(columns, matrix.Cols);
            Assert.Equal(MatType.CV_64FC1, matrix.Type);
        }

        private static void AssertCombinedBlocks(
            Mat combined,
            Mat dpdr,
            Mat dpdt,
            Mat dpdf,
            Mat dpdc,
            Mat dpdk,
            double tolerance)
        {
            Mat[] blocks = { dpdr, dpdt, dpdf, dpdc, dpdk };
            double[] combinedValues = combined.ToArray<double>();
            int combinedColumns = combined.Cols;
            int columnOffset = 0;
            foreach (Mat block in blocks)
            {
                double[] blockValues = block.ToArray<double>();
                for (int row = 0; row < combined.Rows; row++)
                {
                    for (int column = 0; column < block.Cols; column++)
                    {
                        double expected =
                            combinedValues[(row * combinedColumns) + columnOffset + column];
                        double actual = blockValues[(row * block.Cols) + column];
                        Assert.InRange(Math.Abs(expected - actual), 0.0, tolerance);
                    }
                }
                columnOffset += block.Cols;
            }
            Assert.Equal(combinedColumns, columnOffset);
        }

        private static void AssertMatricesNear(Mat expected, Mat actual, double tolerance)
        {
            Assert.Equal(expected.Rows, actual.Rows);
            Assert.Equal(expected.Cols, actual.Cols);
            Assert.Equal(expected.Type, actual.Type);
            if (expected.Depth == MatType.CV_32F)
            {
                float[] expectedValues = expected.ToArray<float>();
                float[] actualValues = actual.ToArray<float>();
                Assert.Equal(expectedValues.Length, actualValues.Length);
                for (int index = 0; index < expectedValues.Length; index++)
                {
                    Assert.InRange(
                        Math.Abs((double)expectedValues[index] - actualValues[index]),
                        0.0,
                        tolerance);
                }
                return;
            }

            double[] expectedDoubleValues = expected.ToArray<double>();
            double[] actualDoubleValues = actual.ToArray<double>();
            Assert.Equal(expectedDoubleValues.Length, actualDoubleValues.Length);
            for (int index = 0; index < expectedDoubleValues.Length; index++)
            {
                Assert.InRange(
                    Math.Abs(expectedDoubleValues[index] - actualDoubleValues[index]),
                    0.0,
                    tolerance);
            }
        }

        private static void DisposeAll(Mat[] matrices)
        {
            foreach (Mat matrix in matrices)
            {
                matrix.Dispose();
            }
        }
    }
}
