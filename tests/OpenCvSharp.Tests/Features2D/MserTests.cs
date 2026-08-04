using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

namespace JYPPX.OpenCvSharp.Tests.Features2D
{
    public class MserTests
    {
        [Fact]
        public void MserRegionClonesPointsAndGuardsNullInput()
        {
            Point[] points =
            {
                new Point(1, 2),
                new Point(3, 4)
            };
            var boundingBox = new Rect(1, 2, 3, 4);
            var region = new MserRegion(points, boundingBox);

            points[0] = new Point(9, 9);

            Assert.Equal(2, region.PointCount);
            Assert.True(region.HasPoints);
            Assert.Equal(new Point(1, 2), region.Points[0]);

            Point[] regionPoints = region.Points;
            regionPoints[0] = new Point(8, 8);

            Assert.Equal(new Point(1, 2), region.Points[0]);
            Assert.Equal(boundingBox, region.BoundingBox);
            Assert.Equal("{PointCount=2,BoundingBox={X=1,Y=2,Width=3,Height=4}}", region.ToString());
            Assert.Throws<ArgumentNullException>(() => new MserRegion(null!, boundingBox));
        }

        [Fact]
        public void MserRegionReportsPointPresence()
        {
            var empty = new MserRegion(Array.Empty<Point>(), new Rect(0, 0, 0, 0));
            var populated = new MserRegion(new[] { new Point(1, 2) }, new Rect(1, 2, 1, 1));

            Assert.False(empty.HasPoints);
            Assert.Equal(0, empty.PointCount);
            Assert.True(populated.HasPoints);
            Assert.Equal(1, populated.PointCount);
        }

        [Fact]
        public void CreateReportsDefinedBoundaryWhenFeaturesModuleIsNotLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            OpenCvException? exception = Record.Exception(() =>
            {
                using (MSER mser = MSER.Create())
                {
                    Assert.False(mser.IsDisposed);
                }
            }) as OpenCvException;

            if (exception != null)
            {
                Assert.Contains("features2d_mser_create", exception.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("OpenCV", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void PropertiesRoundTripWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            MSER? mser = TryCreateMser();
            if (mser == null)
            {
                return;
            }

            using (mser)
            {
                Assert.Contains("MSER", mser.DefaultName, StringComparison.OrdinalIgnoreCase);

                mser.Delta = 7;
                mser.MinArea = 32;
                mser.MaxArea = 18000;
                mser.MaxVariation = 0.35;
                mser.MinDiversity = 0.15;
                mser.MaxEvolution = 160;
                mser.AreaThreshold = 1.03;
                mser.MinMargin = 0.005;
                mser.EdgeBlurSize = 3;
                mser.Pass2Only = false;

                Assert.Equal(7, mser.Delta);
                Assert.Equal(32, mser.MinArea);
                Assert.Equal(18000, mser.MaxArea);
                Assert.Equal(0.35, mser.MaxVariation, 6);
                Assert.Equal(0.15, mser.MinDiversity, 6);
                Assert.Equal(160, mser.MaxEvolution);
                Assert.Equal(1.03, mser.AreaThreshold, 6);
                Assert.Equal(0.005, mser.MinMargin, 6);
                Assert.Equal(3, mser.EdgeBlurSize);
                Assert.False(mser.Pass2Only);
                Assert.Contains("Delta=7", mser.ToString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void DetectAndDetectRegionsReturnManagedResultsWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            MSER? mser = TryCreateMser();
            if (mser == null)
            {
                return;
            }

            using (mser)
            using (Mat image = Feature2DTestData.CreateMserImage())
            {
                KeyPoint[] keypoints = mser.Detect(image);
                KeyPoint[][] batch = mser.Detect(new[] { image });
                MserRegion[] regions = mser.DetectRegions(image);

                Assert.NotNull(keypoints);
                Assert.Single(batch);
                Assert.NotNull(batch[0]);
                Assert.NotNull(regions);
                for (int i = 0; i < regions.Length; i++)
                {
                    Assert.True(regions[i].PointCount >= 0);
                    Assert.True(regions[i].BoundingBox.Width >= 0);
                    Assert.True(regions[i].BoundingBox.Height >= 0);
                    Assert.Contains("PointCount=", regions[i].ToString(), StringComparison.Ordinal);
                }
            }
        }

        [Fact]
        public void DisposedMserRejectsUseWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            MSER? mser = TryCreateMser();
            if (mser == null)
            {
                return;
            }

            using Mat image = Feature2DTestData.CreateMserImage();
            mser.Dispose();

            Assert.True(mser.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => mser.Clear());
            Assert.Throws<ObjectDisposedException>(() => mser.Empty);
            Assert.Throws<ObjectDisposedException>(() => mser.DescriptorSize);
            Assert.Throws<ObjectDisposedException>(() => mser.DescriptorType);
            Assert.Throws<ObjectDisposedException>(() => mser.DefaultNorm);
            Assert.Throws<ObjectDisposedException>(() => mser.DefaultName);
            Assert.Throws<ObjectDisposedException>(() => mser.Delta);
            Assert.Throws<ObjectDisposedException>(() => mser.Delta = 7);
            Assert.Throws<ObjectDisposedException>(() => mser.MinArea);
            Assert.Throws<ObjectDisposedException>(() => mser.MinArea = 32);
            Assert.Throws<ObjectDisposedException>(() => mser.MaxArea);
            Assert.Throws<ObjectDisposedException>(() => mser.MaxArea = 18000);
            Assert.Throws<ObjectDisposedException>(() => mser.MaxVariation);
            Assert.Throws<ObjectDisposedException>(() => mser.MaxVariation = 0.35);
            Assert.Throws<ObjectDisposedException>(() => mser.MinDiversity);
            Assert.Throws<ObjectDisposedException>(() => mser.MinDiversity = 0.15);
            Assert.Throws<ObjectDisposedException>(() => mser.MaxEvolution);
            Assert.Throws<ObjectDisposedException>(() => mser.MaxEvolution = 160);
            Assert.Throws<ObjectDisposedException>(() => mser.AreaThreshold);
            Assert.Throws<ObjectDisposedException>(() => mser.AreaThreshold = 1.03);
            Assert.Throws<ObjectDisposedException>(() => mser.MinMargin);
            Assert.Throws<ObjectDisposedException>(() => mser.MinMargin = 0.005);
            Assert.Throws<ObjectDisposedException>(() => mser.EdgeBlurSize);
            Assert.Throws<ObjectDisposedException>(() => mser.EdgeBlurSize = 3);
            Assert.Throws<ObjectDisposedException>(() => mser.Pass2Only);
            Assert.Throws<ObjectDisposedException>(() => mser.Pass2Only = false);
            Assert.Throws<ObjectDisposedException>(() => mser.Detect(image));
            Assert.Throws<ObjectDisposedException>(() => mser.DetectRegions(image));
            Assert.Equal("{Disposed=True}", mser.ToString());
        }

        private static MSER? TryCreateMser()
        {
            try
            {
                return MSER.Create(delta: 5, minArea: 20, maxArea: 22000);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }
    }
}
