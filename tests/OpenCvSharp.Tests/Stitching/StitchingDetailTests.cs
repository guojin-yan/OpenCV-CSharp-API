using System;
using System.Linq;
using OpenCvSharp.Core;
using OpenCvSharp.Stitching;

namespace OpenCvSharp.Tests.Stitching
{
    public sealed class StitchingDetailTests
    {
        [Fact]
        public void EnumAndManagedValidationFailClosed()
        {
            Assert.Equal(0, (int)SeamFinderType.None);
            Assert.Equal(1, (int)SeamFinderType.Voronoi);
            Assert.Equal(2, (int)SeamFinderType.DynamicProgramming);
            Assert.Equal(0, (int)TimelapserType.AsIs);
            Assert.Equal(1, (int)TimelapserType.Crop);
            Assert.Throws<ArgumentOutOfRangeException>(() => SeamFinder.CreateDefault((SeamFinderType)3));
            Assert.Throws<ArgumentOutOfRangeException>(() => Timelapser.CreateDefault((TimelapserType)2));
            Assert.Throws<ArgumentOutOfRangeException>(() => StitchingUtilities.SelectRandomSubset(4, 3));
            Assert.Throws<ArgumentException>(() => StitchingUtilities.ResultTopLeft(Array.Empty<Point>()));
        }

        [Fact]
        public void SeamFactoriesAndTransactionalMasksWorkWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var image = new Mat(5, 6, MatType.CV_32FC3, new Scalar(10, 20, 30)))
            using (var mask = new Mat(5, 6, MatType.CV_8UC1, new Scalar(255)))
            using (SeamFinder none = SeamFinder.CreateDefault(SeamFinderType.None))
            using (SeamFinder voronoi = SeamFinder.CreateDefault(SeamFinderType.Voronoi))
            using (var dp = new DpSeamFinder(DpSeamCost.ColorGradient))
            using (var graph = new GraphCutSeamFinder(GraphCutSeamCost.Color))
            {
                Assert.IsType<NoSeamFinder>(none);
                Assert.IsType<VoronoiSeamFinder>(voronoi);
                dp.SetCostFunction(DpSeamCost.Color);
                var images = new[] { image };
                var corners = new[] { new Point(-2, 3) };
                var masks = new[] { mask };
                none.Find(images, corners, masks);
                voronoi.Find(images, corners, masks);
                dp.Find(images, corners, masks);
                graph.Find(images, corners, masks);
                Assert.Equal(255.0, Cv2.Mean(mask).V0, 12);
            }
        }

        [Fact]
        public void TimelapserReturnsIndependentCpuStorageWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var image = new Mat(2, 3, MatType.CV_16SC3, new Scalar(7, 11, 13)))
            using (var mask = new Mat(2, 3, MatType.CV_8UC1, new Scalar(255)))
            using (Timelapser timelapser = Timelapser.CreateDefault(TimelapserType.AsIs))
            {
                timelapser.Initialize(new[] { new Point(-1, 2) }, new[] { new Size(3, 2) });
                timelapser.Process(image, mask, new Point(-1, 2));
                using (Mat destination = timelapser.GetDestination())
                {
                    Assert.Equal(MatType.CV_16SC3, destination.Type);
                    Assert.Equal(new Size(3, 2), new Size(destination.Cols, destination.Rows));
                    Assert.Equal(7.0, Cv2.Mean(destination).V0, 12);
                    timelapser.Dispose();
                    Assert.Equal(11.0, Cv2.Mean(destination).V1, 12);
                }
            }
        }

        [Fact]
        public void PlacementUtilitiesMatchExactGeometryWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            var corners = new[] { new Point(-2, 3), new Point(2, 1) };
            var sizes = new[] { new Size(6, 4), new Size(5, 7) };
            Assert.Equal(new Rect(-2, 1, 9, 7), StitchingUtilities.ResultRoi(corners, sizes));
            Assert.Equal(new Rect(2, 3, 2, 4), StitchingUtilities.ResultRoiIntersection(corners, sizes));
            Assert.Equal(new Point(-2, 1), StitchingUtilities.ResultTopLeft(corners));
            Assert.True(StitchingUtilities.TryOverlapRoi(
                new Rect(-2, 3, 6, 4), new Rect(2, 1, 5, 7), out Rect overlap));
            Assert.Equal(new Rect(2, 3, 2, 4), overlap);

            using (var first = new Mat(4, 6, MatType.CV_8UC1))
            using (var second = new Mat(7, 5, MatType.CV_8UC1))
                Assert.Equal(new Rect(-2, 1, 9, 7), StitchingUtilities.ResultRoi(corners, new[] { first, second }));

            for (int repetition = 0; repetition < 32; ++repetition)
            {
                int[] subset = StitchingUtilities.SelectRandomSubset(4, 10);
                Assert.Equal(4, subset.Length);
                Assert.Equal(subset.OrderBy(value => value), subset);
                Assert.Equal(4, subset.Distinct().Count());
                Assert.All(subset, value => Assert.InRange(value, 0, 9));
            }
            Assert.True(StitchingUtilities.LogLevel >= 0);
        }

        [Fact]
        public void SphericalProjectionRoundTripsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var camera = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var rotationParent = Mat.Zeros(5, 5, MatType.CV_32FC1))
            using (var rotation = rotationParent.SubMat(new Rect(1, 1, 3, 3)))
            using (var identity = Mat.Eye(3, 3, MatType.CV_32FC1))
            {
                identity.CopyTo(rotation);
                using (var projector = new SphericalProjector(2F, camera, rotation))
                {
                    Point2f projected = projector.MapForward(new Point2f(0, 0));
                    Assert.Equal(0F, projected.X, 5);
                    Assert.Equal((float)Math.PI, projected.Y, 5);
                    Point2f restored = projector.MapBackward(projected);
                    Assert.Equal(0F, restored.X, 5);
                    Assert.Equal(0F, restored.Y, 5);
                    projector.Dispose();
                    Assert.Throws<ObjectDisposedException>(() => projector.MapForward(new Point2f()));
                }
            }
        }
    }
}
