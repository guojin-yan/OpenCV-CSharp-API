using System;
using OpenCvSharp.Core;
using OpenCvSharp.XObjDetect;

namespace OpenCvSharp.Tests.XObjDetect
{
    public sealed class XObjDetectTests
    {
        [Fact]
        public void EnumValuesMatchOpenCvXObjDetectConstants()
        {
            Assert.Equal(0, (int)CascadeClassifierFlags.None);
            Assert.Equal(1, (int)CascadeClassifierFlags.DoCannyPruning);
            Assert.Equal(2, (int)CascadeClassifierFlags.ScaleImage);
            Assert.Equal(4, (int)CascadeClassifierFlags.FindBiggestObject);
            Assert.Equal(8, (int)CascadeClassifierFlags.DoRoughSearch);
            Assert.Equal(0, (int)HOGDescriptorHistogramNormType.L2Hys);
        }

        [Fact]
        public void ResultObjectsStoreValuesAndReturnSnapshots()
        {
            var cascadeRectangles = new[] { new Rect(1, 2, 3, 4) };
            var rejectLevels = new[] { 5 };
            var levelWeights = new[] { 0.75 };
            var cascade = new CascadeDetectionResult(
                cascadeRectangles,
                rejectLevels,
                levelWeights);
            cascadeRectangles[0] = new Rect(10, 20, 30, 40);
            rejectLevels[0] = 9;
            levelWeights[0] = 1.25;
            Assert.Equal(1, cascade.RectangleCount);
            Assert.Equal(1, cascade.RejectLevelCount);
            Assert.Equal(1, cascade.LevelWeightCount);
            Assert.Equal(new Rect(1, 2, 3, 4), cascade.Rectangles[0]);
            Assert.Equal(5, cascade.RejectLevels[0]);
            Assert.Equal(0.75, cascade.LevelWeights[0]);

            Rect[] returnedCascadeRectangles = cascade.Rectangles;
            int[] returnedRejectLevels = cascade.RejectLevels;
            double[] returnedLevelWeights = cascade.LevelWeights;
            returnedCascadeRectangles[0] = new Rect(100, 200, 300, 400);
            returnedRejectLevels[0] = 50;
            returnedLevelWeights[0] = 7.5;

            Assert.Equal(new Rect(1, 2, 3, 4), cascade.Rectangles[0]);
            Assert.Equal(5, cascade.RejectLevels[0]);
            Assert.Equal(0.75, cascade.LevelWeights[0]);
            Assert.Equal("CascadeDetectionResult(Rectangles=1, RejectLevels=1, LevelWeights=1)", cascade.ToString());

            var locations = new[] { new Point(6, 7) };
            var pointWeights = new[] { 0.5 };
            var hogPoint = new HOGDetectionResult(locations, pointWeights);
            locations[0] = new Point(60, 70);
            pointWeights[0] = 1.5;
            Assert.Equal(1, hogPoint.LocationCount);
            Assert.Equal(0, hogPoint.RectangleCount);
            Assert.Equal(1, hogPoint.WeightCount);
            Assert.Equal(new Point(6, 7), hogPoint.Locations[0]);
            Assert.Empty(hogPoint.Rectangles);
            Assert.Equal(0.5, hogPoint.Weights[0]);

            Point[] returnedLocations = hogPoint.Locations;
            double[] returnedPointWeights = hogPoint.Weights;
            returnedLocations[0] = new Point(600, 700);
            returnedPointWeights[0] = 5.0;

            Assert.Equal(new Point(6, 7), hogPoint.Locations[0]);
            Assert.Equal(0.5, hogPoint.Weights[0]);
            Assert.Equal("HOGDetectionResult(Locations=1, Rectangles=0, Weights=1)", hogPoint.ToString());

            var rectangles = new[] { new Rect(8, 9, 10, 11) };
            var rectWeights = new[] { 0.25 };
            var hogRect = new HOGDetectionResult(rectangles, rectWeights);
            rectangles[0] = new Rect(80, 90, 100, 110);
            rectWeights[0] = 1.75;
            Assert.Equal(0, hogRect.LocationCount);
            Assert.Equal(1, hogRect.RectangleCount);
            Assert.Equal(1, hogRect.WeightCount);
            Assert.Empty(hogRect.Locations);
            Assert.Equal(new Rect(8, 9, 10, 11), hogRect.Rectangles[0]);
            Assert.Equal(0.25, hogRect.Weights[0]);

            Rect[] returnedRectangles = hogRect.Rectangles;
            double[] returnedRectWeights = hogRect.Weights;
            returnedRectangles[0] = new Rect(800, 900, 1000, 1100);
            returnedRectWeights[0] = 2.5;

            Assert.Equal(new Rect(8, 9, 10, 11), hogRect.Rectangles[0]);
            Assert.Equal(0.25, hogRect.Weights[0]);
            Assert.Equal("HOGDetectionResult(Locations=0, Rectangles=1, Weights=1)", hogRect.ToString());
        }

        [Fact]
        public void ResultObjectsNormalizeNullArrays()
        {
            var cascade = new CascadeDetectionResult(null!, null!, null!);

            Assert.Equal(0, cascade.RectangleCount);
            Assert.Equal(0, cascade.RejectLevelCount);
            Assert.Equal(0, cascade.LevelWeightCount);
            Assert.Empty(cascade.Rectangles);
            Assert.Empty(cascade.RejectLevels);
            Assert.Empty(cascade.LevelWeights);
            Assert.Equal("CascadeDetectionResult(Rectangles=0, RejectLevels=0, LevelWeights=0)", cascade.ToString());

            var hogPoint = new HOGDetectionResult((Point[])null!, null!);

            Assert.Equal(0, hogPoint.LocationCount);
            Assert.Equal(0, hogPoint.RectangleCount);
            Assert.Equal(0, hogPoint.WeightCount);
            Assert.Empty(hogPoint.Locations);
            Assert.Empty(hogPoint.Rectangles);
            Assert.Empty(hogPoint.Weights);
            Assert.Equal("HOGDetectionResult(Locations=0, Rectangles=0, Weights=0)", hogPoint.ToString());

            var hogRect = new HOGDetectionResult((Rect[])null!, null!);

            Assert.Equal(0, hogRect.LocationCount);
            Assert.Equal(0, hogRect.RectangleCount);
            Assert.Equal(0, hogRect.WeightCount);
            Assert.Empty(hogRect.Locations);
            Assert.Empty(hogRect.Rectangles);
            Assert.Empty(hogRect.Weights);
            Assert.Equal("HOGDetectionResult(Locations=0, Rectangles=0, Weights=0)", hogRect.ToString());

            Assert.Throws<ArgumentException>(() => new CascadeDetectionResult(new[] { new Rect(1, 2, 3, 4), new Rect(5, 6, 7, 8) }, new[] { 1 }, Array.Empty<double>()));
            Assert.Throws<ArgumentException>(() => new CascadeDetectionResult(new[] { new Rect(1, 2, 3, 4), new Rect(5, 6, 7, 8) }, Array.Empty<int>(), new[] { 0.5 }));
            Assert.Throws<ArgumentException>(() => new HOGDetectionResult(new[] { new Point(1, 2), new Point(3, 4) }, new[] { 0.5 }));
            Assert.Throws<ArgumentException>(() => new HOGDetectionResult(new[] { new Rect(1, 2, 3, 4), new Rect(5, 6, 7, 8) }, new[] { 0.5 }));
        }

        [Fact]
        public void ConstructorsRejectNullPathsBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() => new CascadeClassifier(null!));
            Assert.Throws<ArgumentNullException>(() => new HOGDescriptor(null!));
        }

        [Fact]
        public void ClassesValidateManagedArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var cascade = new CascadeClassifier())
            using (var hog = new HOGDescriptor())
            using (var image = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => cascade.Load(null!));
                Assert.Throws<ArgumentNullException>(() => cascade.DetectMultiScale(null!));
                Assert.Throws<ArgumentNullException>(() => cascade.DetectMultiScale2(null!));
                Assert.Throws<ArgumentNullException>(() => cascade.DetectMultiScale3(null!));
                Assert.Throws<ArgumentNullException>(() => hog.Detect(null!));
                Assert.Throws<ArgumentNullException>(() => hog.DetectMultiScale(null!));
                Assert.Throws<ArgumentNullException>(() => hog.SetSVMDetector((float[])null!));

                cascade.Dispose();
                Assert.True(cascade.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => cascade.Empty);
                Assert.Throws<ObjectDisposedException>(() => cascade.Load("missing.xml"));
                Assert.Throws<ObjectDisposedException>(() => cascade.GetOriginalWindowSize());
                Assert.Throws<ObjectDisposedException>(() => cascade.GetFeatureType());
                Assert.Throws<ObjectDisposedException>(() => cascade.DetectMultiScale(image));

                hog.Dispose();
                Assert.True(hog.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => hog.SetSVMDetector(Array.Empty<float>()));
                Assert.Throws<ObjectDisposedException>(() => hog.CheckDetectorSize());
                Assert.Throws<ObjectDisposedException>(() => hog.GetDescriptorSize());
                Assert.Throws<ObjectDisposedException>(() => hog.GetWinSigma());
                Assert.Throws<ObjectDisposedException>(() => hog.Detect(image));
                Assert.Throws<ObjectDisposedException>(() => hog.DetectMultiScale(image));
            }
        }

    }
}
