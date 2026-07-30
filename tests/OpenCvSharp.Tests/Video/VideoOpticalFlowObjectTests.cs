using System;
using OpenCvSharp.Core;
using OpenCvSharp.Video;

namespace OpenCvSharp.Tests.Video
{
    public sealed class VideoOpticalFlowObjectTests
    {
        [Fact]
        public void PresetAndManagedValidationAreDeterministic()
        {
            Assert.Equal(0, (int)DisOpticalFlowPreset.UltraFast);
            Assert.Equal(1, (int)DisOpticalFlowPreset.Fast);
            Assert.Equal(2, (int)DisOpticalFlowPreset.Medium);
            Assert.Throws<ArgumentOutOfRangeException>(() => DisOpticalFlow.Create((DisOpticalFlowPreset)3));
            Assert.Throws<ArgumentOutOfRangeException>(() => FarnebackOpticalFlow.Create(flags: OpticalFlowFlags.LkGetMinEigenvals));
            Assert.Throws<ArgumentOutOfRangeException>(() => SparsePyrLkOpticalFlow.Create(flags: OpticalFlowFlags.FarnebackGaussian));

            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var sparse = SparsePyrLkOpticalFlow.Create())
            using (var image = new Mat(8, 8, MatType.CV_8UC1, new Scalar(0, 0, 0, 0)))
            {
                Assert.Throws<ArgumentNullException>(() => sparse.Calc(null!, image, Array.Empty<Point2f>(), out _, out _));
                Assert.Throws<ArgumentNullException>(() => sparse.Calc(image, null!, Array.Empty<Point2f>(), out _, out _));
                Assert.Throws<ArgumentNullException>(() => sparse.Calc(image, image, null!, out _, out _));
                Assert.Throws<ArgumentException>(() => sparse.Calc(image, image, new[] { new Point2f(1, 1) }, Array.Empty<Point2f>(), out _, out _));
            }
        }

        [Fact]
        public void FarnebackPropertiesRoundTripAndCalcOwnsOutput()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var first = CreateFrame(0))
            using (var second = CreateFrame(1))
            using (var algorithm = FarnebackOpticalFlow.Create(numLevels: 3, winSize: 9, numIterations: 3))
            {
                algorithm.NumLevels = 2;
                algorithm.PyrScale = 0.5;
                algorithm.FastPyramids = false;
                algorithm.WinSize = 11;
                algorithm.NumIterations = 4;
                algorithm.PolyN = 5;
                algorithm.PolySigma = 1.2;
                algorithm.Flags = OpticalFlowFlags.FarnebackGaussian;

                Assert.Equal(2, algorithm.NumLevels);
                Assert.Equal(0.5, algorithm.PyrScale, 8);
                Assert.False(algorithm.FastPyramids);
                Assert.Equal(11, algorithm.WinSize);
                Assert.Equal(4, algorithm.NumIterations);
                Assert.Equal(5, algorithm.PolyN);
                Assert.Equal(1.2, algorithm.PolySigma, 8);
                Assert.Equal(OpticalFlowFlags.FarnebackGaussian, algorithm.Flags);

                using Mat flow = algorithm.Calc(first, second);
                Assert.Equal(first.Rows, flow.Rows);
                Assert.Equal(first.Cols, flow.Cols);
                Assert.Equal(MatType.CV_32FC2, flow.Type);
                algorithm.CollectGarbage();
            }
        }

        [Fact]
        public void VariationalRefinementSupportsPackedAndSplitFlow()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var first = CreateFrame(0))
            using (var second = CreateFrame(1))
            using (var packed = new Mat(32, 32, MatType.CV_32FC2, new Scalar(0, 0, 0, 0)))
            using (var flowU = new Mat(32, 32, MatType.CV_32FC1, new Scalar(0, 0, 0, 0)))
            using (var flowV = new Mat(32, 32, MatType.CV_32FC1, new Scalar(0, 0, 0, 0)))
            using (var algorithm = VariationalRefinement.Create())
            {
                algorithm.FixedPointIterations = 2;
                algorithm.SorIterations = 3;
                algorithm.Omega = 1.5F;
                algorithm.Alpha = 20.0F;
                algorithm.Delta = 5.0F;
                algorithm.Gamma = 10.0F;
                algorithm.Epsilon = 0.01F;

                Assert.Equal(2, algorithm.FixedPointIterations);
                Assert.Equal(3, algorithm.SorIterations);
                Assert.Equal(1.5F, algorithm.Omega);
                Assert.Equal(20.0F, algorithm.Alpha);
                algorithm.Calc(first, second, packed);
                algorithm.CalcUV(first, second, flowU, flowV);
                Assert.Equal(MatType.CV_32FC2, packed.Type);
                Assert.Equal(MatType.CV_32FC1, flowU.Type);
                Assert.Equal(MatType.CV_32FC1, flowV.Type);
            }
        }

        [Fact]
        public void DisPropertiesRoundTripAndCalcDenseFlow()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var first = CreateFrame(0))
            using (var second = CreateFrame(1))
            using (var algorithm = DisOpticalFlow.Create(DisOpticalFlowPreset.UltraFast))
            {
                algorithm.FinestScale = 1;
                algorithm.CoarsestScale = 2;
                algorithm.PatchSize = 8;
                algorithm.PatchStride = 4;
                algorithm.GradientDescentIterations = 4;
                algorithm.VariationalRefinementIterations = 0;
                algorithm.UseMeanNormalization = true;
                algorithm.UseSpatialPropagation = false;

                Assert.Equal(1, algorithm.FinestScale);
                Assert.Equal(2, algorithm.CoarsestScale);
                Assert.Equal(8, algorithm.PatchSize);
                Assert.Equal(4, algorithm.PatchStride);
                Assert.True(algorithm.UseMeanNormalization);
                Assert.False(algorithm.UseSpatialPropagation);
                using Mat flow = algorithm.Calc(first, second);
                Assert.Equal(MatType.CV_32FC2, flow.Type);
                Assert.Equal(32, flow.Rows);
                Assert.Equal(32, flow.Cols);
            }
        }

        [Fact]
        public void SparsePyrLkPropertiesPointsAndDisposalAreStable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var first = CreateFrame(0))
            using (var second = CreateFrame(1))
            {
                var algorithm = SparsePyrLkOpticalFlow.Create(winSize: new Size(9, 9), maxLevel: 2);
                algorithm.WinSize = new Size(11, 11);
                algorithm.MaxLevel = 1;
                algorithm.Criteria = TermCriteria.ByCountAndEpsilon(15, 0.02);
                algorithm.Flags = OpticalFlowFlags.None;
                algorithm.MinEigThreshold = 1e-5;

                Assert.Equal(new Size(11, 11), algorithm.WinSize);
                Assert.Equal(1, algorithm.MaxLevel);
                Assert.Equal(TermCriteriaTypes.CountOrEps, algorithm.Criteria.Type);
                Assert.Equal(15, algorithm.Criteria.MaxCount);
                Assert.Equal(0.02, algorithm.Criteria.Epsilon, 8);
                Assert.Equal(OpticalFlowFlags.None, algorithm.Flags);
                Assert.Equal(1e-5, algorithm.MinEigThreshold, 10);

                Point2f[] points = { new Point2f(10, 10), new Point2f(15, 15), new Point2f(20, 20) };
                Point2f[] next = algorithm.Calc(first, second, points, out byte[] status, out float[] error);
                Assert.Equal(points.Length, next.Length);
                Assert.Equal(points.Length, status.Length);
                Assert.Equal(points.Length, error.Length);

                algorithm.Flags = OpticalFlowFlags.UseInitialFlow;
                Assert.Throws<InvalidOperationException>(() => algorithm.Calc(first, second, points, out _, out _));
                Point2f[] initializedNext = algorithm.Calc(first, second, points, points, out status, out error);
                Assert.Equal(points.Length, initializedNext.Length);
                Assert.Equal(points.Length, status.Length);
                Assert.Equal(points.Length, error.Length);

                algorithm.Dispose();
                algorithm.Dispose();
                Assert.True(algorithm.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => algorithm.Calc(first, second, points, out _, out _));
                Assert.Throws<ObjectDisposedException>(() => _ = algorithm.MaxLevel);
            }
        }

        private static Mat CreateFrame(int offset)
        {
            var frame = new Mat(32, 32, MatType.CV_8UC1);
            var values = new byte[32 * 32];
            for (int y = 7; y < 25; y++)
            {
                for (int x = 7 + offset; x < 23 + offset; x++)
                {
                    values[(y * 32) + x] = (byte)(((x + y) & 1) == 0 ? 255 : 80);
                }
            }
            frame.CopyFrom(values);
            return frame;
        }
    }
}
