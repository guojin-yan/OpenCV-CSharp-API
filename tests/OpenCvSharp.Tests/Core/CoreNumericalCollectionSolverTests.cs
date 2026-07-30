using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Tests.Core
{
    public class CoreNumericalCollectionSolverTests
    {
        [Fact]
        public void ScalarUtilitiesAndBatchDistanceProduceExactResults()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Assert.InRange(Cv2.CubeRoot(-8.0f), -2.0001f, -1.9999f);
            Assert.InRange(Cv2.FastAtan2(1.0f, 0.0f), 89.7f, 90.3f);

            using var src1 = new Mat(2, 2, MatType.CV_32FC1);
            using var src2 = new Mat(2, 2, MatType.CV_32FC1);
            using var distances = new Mat();
            using var indices = new Mat();
            src1.CopyFrom(new[] { 0.0f, 0.0f, 3.0f, 4.0f });
            src2.CopyFrom(new[] { 0.0f, 0.0f, 6.0f, 8.0f });

            Cv2.BatchDistance(src1, src2, distances, MatType.CV_32F, indices, NormTypes.L2, 1);
            Assert.Equal(new[] { 0.0f, 5.0f }, distances.ToArray<float>());
            Assert.Equal(new[] { 0, 0 }, indices.ToArray<int>());

            Cv2.BatchDistance(src1, src2, distances);
            Assert.Equal(new[] { 0.0f, 10.0f, 5.0f, 5.0f }, distances.ToArray<float>());

            using var mask = new Mat(2, 2, MatType.CV_8UC1);
            mask.CopyFrom(new byte[] { 0, 255, 255, 0 });
            Cv2.BatchDistance(src1, src2, distances, MatType.CV_32F, indices, NormTypes.L2, 1, mask);
            Assert.Equal(new[] { 10.0f, 5.0f }, distances.ToArray<float>());
            Assert.Equal(new[] { 1, 0 }, indices.ToArray<int>());
        }

        [Fact]
        public void ExistingSplitPatchAndSvdEvidenceCoversFreeFunctionRows()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using var parent = new Mat(2, 3, MatType.CV_32FC1);
            parent.CopyFrom(new[] { 1.0f, float.NaN, 3.0f, 4.0f, float.NaN, 6.0f });
            using Mat roi = parent.SubMat(new Rect(0, 0, 2, 2));
            Assert.False(roi.IsContinuous);
            Cv2.PatchNaNs(roi, -2.0);
            Assert.Equal(new[] { 1.0f, -2.0f, 3.0f, 4.0f, -2.0f, 6.0f }, parent.ToArray<float>());

            using var color = new Mat(1, 2, MatType.CV_32FC2);
            color.CopyFrom(new[] { 1.0f, 10.0f, 2.0f, 20.0f });
            Mat[] channels = Cv2.Split(color);
            try
            {
                Assert.Equal(2, channels.Length);
                Assert.Equal(new[] { 1.0f, 2.0f }, channels[0].ToArray<float>());
                Assert.Equal(new[] { 10.0f, 20.0f }, channels[1].ToArray<float>());
            }
            finally
            {
                foreach (Mat channel in channels)
                {
                    channel.Dispose();
                }
            }

            using var matrix = new Mat(3, 2, MatType.CV_64FC1);
            using var w = new Mat();
            using var u = new Mat();
            using var vt = new Mat();
            using var rhs = new Mat(3, 1, MatType.CV_64FC1);
            using var solution = new Mat();
            matrix.CopyFrom(new[] { 1.0, 0.0, 0.0, 1.0, 1.0, 1.0 });
            rhs.CopyFrom(new[] { 1.0, 2.0, 3.0 });
            Svd.Compute(matrix, w, u, vt);
            Svd.BackSubst(w, u, vt, rhs, solution);
            Assert.Equal(2, solution.Rows);
            Assert.InRange(solution.ToArray<double>()[0], 0.999999, 1.000001);
            Assert.InRange(solution.ToArray<double>()[1], 1.999999, 2.000001);
        }

        [Fact]
        public void CovarianceAndPcaRoundTripPreserveShapeAndValues()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using var data = new Mat(3, 2, MatType.CV_64FC1);
            using var covar = new Mat();
            using var mean = new Mat();
            using var eigenvectors = new Mat();
            using var eigenvalues = new Mat();
            data.CopyFrom(new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 });

            Cv2.CalcCovarMatrix(data, covar, mean, CovarFlags.Normal | CovarFlags.Rows | CovarFlags.Scale);
            Assert.Equal(new Size(2, 2), covar.Size);
            Assert.Equal(new[] { 3.0, 4.0 }, mean.ToArray<double>());
            foreach (double value in covar.ToArray<double>())
            {
                Assert.InRange(value, 2.6666665, 2.6666669);
            }

            Cv2.PcaCompute(data, mean, eigenvectors, eigenvalues, 1);
            Assert.Equal(new Size(2, 1), mean.Size);
            Assert.Equal(new Size(2, 1), eigenvectors.Size);
            Assert.Equal((ulong)1, eigenvalues.Total.ToUInt64());
            using Mat projected = Cv2.PcaProject(data, mean, eigenvectors);
            using Mat reconstructed = Cv2.PcaBackProject(projected, mean, eigenvectors);
            Assert.Equal(new Size(1, 3), projected.Size);
            Assert.Equal(data.Size, reconstructed.Size);
            double[] original = data.ToArray<double>();
            double[] roundTrip = reconstructed.ToArray<double>();
            for (int i = 0; i < original.Length; i++)
            {
                Assert.InRange(roundTrip[i], original[i] - 1e-9, original[i] + 1e-9);
            }

            using var retainedMean = new Mat();
            using var retainedVectors = new Mat();
            using var retainedValues = new Mat();
            Cv2.PcaCompute(data, retainedMean, retainedVectors, retainedValues, 0.95);
            Assert.Equal((ulong)2, retainedValues.Total.ToUInt64());
        }

        [Fact]
        public void GlobalAndExplicitRandomGeneratorsAreDeterministicAndRoiSafe()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using var first = new Mat(1, 8, MatType.CV_32FC1);
            using var second = new Mat(1, 8, MatType.CV_32FC1);
            Cv2.SetRngSeed(12345);
            Cv2.Randu(first, new Scalar(0.0), new Scalar(1.0));
            Cv2.SetRngSeed(12345);
            using var low = new Mat(1, 1, MatType.CV_64FC1, new Scalar(0.0));
            using var high = new Mat(1, 1, MatType.CV_64FC1, new Scalar(1.0));
            Cv2.Randu(second, low, high);
            Assert.Equal(first.ToArray<float>(), second.ToArray<float>());
            Assert.All(first.ToArray<float>(), value => Assert.InRange(value, 0.0f, 0.9999999f));

            Cv2.SetRngSeed(54321);
            Cv2.Randn(first, new Scalar(2.0), new Scalar(0.5));
            Cv2.SetRngSeed(54321);
            Cv2.Randn(second, new Scalar(2.0), new Scalar(0.5));
            Assert.Equal(first.ToArray<float>(), second.ToArray<float>());

            using var values1 = new Mat(1, 8, MatType.CV_32SC1);
            using var values2 = new Mat(1, 8, MatType.CV_32SC1);
            int[] ordered = { 0, 1, 2, 3, 4, 5, 6, 7 };
            values1.CopyFrom(ordered);
            values2.CopyFrom(ordered);
            using var rng1 = new Rng(999);
            using var rng2 = new Rng(999);
            Cv2.RandShuffle(values1, rng: rng1);
            Cv2.RandShuffle(values2, rng: rng2);
            Assert.Equal(values1.ToArray<int>(), values2.ToArray<int>());
            Assert.NotEqual(string.Join(",", ordered), string.Join(",", values1.ToArray<int>()));

            using var parent = new Mat(3, 4, MatType.CV_32FC1, new Scalar(0.0));
            using Mat roi = parent.SubMat(new Rect(0, 0, 3, 3));
            Assert.False(roi.IsContinuous);
            Cv2.Randu(roi, new Scalar(-1.0), new Scalar(1.0));
            float[] parentValues = parent.ToArray<float>();
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    Assert.InRange(parentValues[(row * 4) + col], -1.0f, 0.9999999f);
                }
                Assert.Equal(0.0f, parentValues[(row * 4) + 3]);
            }
        }

        [Fact]
        public void SolveLpReturnsStatusAndSolution()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using var objective = new Mat(1, 2, MatType.CV_64FC1);
            using var constraints = new Mat(3, 3, MatType.CV_64FC1);
            using var solution = new Mat();
            objective.CopyFrom(new[] { 1.0, 1.0 });
            constraints.CopyFrom(new[]
            {
                1.0, 0.0, 2.0,
                0.0, 1.0, 3.0,
                1.0, 1.0, 4.0
            });

            SolveLpResult result = Cv2.SolveLp(objective, constraints, solution);
            Assert.Equal(SolveLpResult.Multiple, result);
            Assert.Equal(new Size(1, 2), solution.Size);
            double[] values = solution.ToArray<double>();
            Assert.InRange(values[0] + values[1], 3.999999, 4.000001);
            Assert.All(values, value => Assert.True(value >= 0.0));
        }

        [Fact]
        public void InvalidShapesFlagsAliasingAndDisposedObjectsFailBeforeUnsafeUse()
        {
            using var byteRows = new Mat(2, 2, MatType.CV_8UC1);
            using var floatRows = new Mat(2, 2, MatType.CV_32FC1);
            using var wrongCols = new Mat(2, 3, MatType.CV_32FC1);
            using var distances = new Mat();
            using var indices = new Mat();
            using var badMask = new Mat(1, 2, MatType.CV_8UC1);
            Assert.Throws<ArgumentException>(() => Cv2.BatchDistance(floatRows, wrongCols, distances));
            Assert.Throws<ArgumentException>(() => Cv2.BatchDistance(floatRows, floatRows, distances, MatType.CV_32F, null, NormTypes.L2, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.BatchDistance(floatRows, floatRows, distances, MatType.CV_32F, indices, NormTypes.Hamming, 1));
            Assert.Throws<ArgumentException>(() => Cv2.BatchDistance(byteRows, byteRows, distances, MatType.CV_32S, indices, NormTypes.Hamming, 1, badMask));
            Assert.Throws<ArgumentException>(() => Cv2.BatchDistance(floatRows, floatRows, floatRows));

            using var mean = new Mat();
            using var eigenvectors = new Mat();
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.CalcCovarMatrix(floatRows, distances, mean, CovarFlags.Normal));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.CalcCovarMatrix(floatRows, distances, mean, CovarFlags.Rows | CovarFlags.Cols));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.PcaCompute(floatRows, mean, eigenvectors, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.PcaCompute(floatRows, mean, eigenvectors, 0.0));
            Assert.Throws<ArgumentException>(() => Cv2.PcaProject(floatRows, mean, eigenvectors, distances));

            using var empty = new Mat();
            Assert.Throws<ArgumentException>(() => Cv2.Randu(empty, new Scalar(0.0), new Scalar(1.0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Randn(floatRows, new Scalar(0.0), new Scalar(-1.0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.RandShuffle(floatRows, double.NaN));
            using var disposedRng = new Rng(1);
            disposedRng.Dispose();
            Assert.Throws<ObjectDisposedException>(() => Cv2.RandShuffle(floatRows, rng: disposedRng));

            using var objective = new Mat(1, 2, MatType.CV_64FC1);
            using var invalidConstraints = new Mat(2, 2, MatType.CV_64FC1);
            Assert.Throws<ArgumentException>(() => Cv2.SolveLp(objective, invalidConstraints, distances));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.SolveLp(objective, wrongCols, distances, -1.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.CubeRoot(float.NaN));

            var disposed = new Mat(1, 1, MatType.CV_32FC1);
            disposed.Dispose();
            Assert.Throws<ObjectDisposedException>(() => Cv2.Randu(disposed, new Scalar(0.0), new Scalar(1.0)));
        }
    }
}
