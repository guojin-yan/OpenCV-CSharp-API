using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Photo;

namespace JYPPX.OpenCvSharp.Tests.Photo
{
    public sealed class PhotoHdrParityTests
    {
        [Fact]
        public void HdrFactoriesPreserveDefaultsPropertiesAndDisposal()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            AlignMTB align = PhotoCv2.CreateAlignMTB();
            CalibrateDebevec debevec = PhotoCv2.CreateCalibrateDebevec();
            CalibrateRobertson robertson = PhotoCv2.CreateCalibrateRobertson();
            MergeDebevec mergeDebevec = PhotoCv2.CreateMergeDebevec();
            MergeMertens mertens = PhotoCv2.CreateMergeMertens();
            MergeRobertson mergeRobertson = PhotoCv2.CreateMergeRobertson();
            try
            {
                Assert.Equal(6, align.MaxBits);
                Assert.Equal(4, align.ExcludeRange);
                Assert.True(align.Cut);
                align.MaxBits = 5;
                align.ExcludeRange = 3;
                align.Cut = false;
                Assert.Equal(5, align.MaxBits);
                Assert.Equal(3, align.ExcludeRange);
                Assert.False(align.Cut);

                Assert.Equal(70, debevec.Samples);
                Assert.Equal(10.0F, debevec.Lambda, 4);
                Assert.False(debevec.Random);
                debevec.Samples = 24;
                debevec.Lambda = 8.0F;
                debevec.Random = true;
                Assert.Equal(24, debevec.Samples);
                Assert.Equal(8.0F, debevec.Lambda, 4);
                Assert.True(debevec.Random);

                Assert.Equal(30, robertson.MaxIter);
                Assert.Equal(0.01F, robertson.Threshold, 5);
                robertson.MaxIter = 2;
                robertson.Threshold = 0.02F;
                Assert.Equal(2, robertson.MaxIter);
                Assert.Equal(0.02F, robertson.Threshold, 5);

                Assert.Equal(1.0F, mertens.ContrastWeight, 5);
                Assert.Equal(1.0F, mertens.SaturationWeight, 5);
                Assert.Equal(0.0F, mertens.ExposureWeight, 5);
                mertens.ContrastWeight = 0.8F;
                mertens.SaturationWeight = 0.7F;
                mertens.ExposureWeight = 0.2F;
                Assert.Equal(0.8F, mertens.ContrastWeight, 5);
                Assert.Equal(0.7F, mertens.SaturationWeight, 5);
                Assert.Equal(0.2F, mertens.ExposureWeight, 5);
            }
            finally
            {
                align.Dispose();
                align.Dispose();
                debevec.Dispose();
                robertson.Dispose();
                mergeDebevec.Dispose();
                mertens.Dispose();
                mergeRobertson.Dispose();
            }

            Assert.True(align.IsDisposed);
            Assert.True(debevec.IsDisposed);
            Assert.True(robertson.IsDisposed);
            Assert.True(mergeDebevec.IsDisposed);
            Assert.True(mertens.IsDisposed);
            Assert.True(mergeRobertson.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => _ = align.MaxBits);
            Assert.Throws<ObjectDisposedException>(() => _ = debevec.Lambda);
            Assert.Throws<ObjectDisposedException>(() => _ = robertson.MaxIter);
            Assert.Throws<ObjectDisposedException>(() => _ = mertens.ContrastWeight);
        }

        [Fact]
        public void AlignMtbSupportsCollectionsHelpersAndNonContiguousInputs()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var parent0 = new Mat(20, 20, MatType.CV_8UC3, new Scalar(32, 48, 64)))
            using (var parent1 = new Mat(20, 20, MatType.CV_8UC3, new Scalar(64, 80, 96)))
            using (var parent2 = new Mat(20, 20, MatType.CV_8UC3, new Scalar(96, 112, 128)))
            using (Mat src0 = parent0.SubMat(new Rect(1, 1, 16, 16)))
            using (Mat src1 = parent1.SubMat(new Rect(2, 2, 16, 16)))
            using (Mat src2 = parent2.SubMat(new Rect(3, 3, 16, 16)))
            using (var times = new Mat(3, 1, MatType.CV_32FC1, new Scalar(0.5)))
            using (var response = new Mat())
            using (var align = PhotoCv2.CreateAlignMTB(cut: false))
            {
                Mat[] aligned = align.Process(new[] { src0, src1, src2 });
                try
                {
                    Assert.Equal(3, aligned.Length);
                    Assert.All(aligned, value =>
                    {
                        Assert.Equal(16, value.Rows);
                        Assert.Equal(16, value.Cols);
                        Assert.Equal(MatType.CV_8UC3, value.Type);
                    });
                }
                finally
                {
                    DisposeAll(aligned);
                }

                Mat[] fullAligned = align.Process(new[] { src0, src1, src2 }, times, response);
                DisposeAll(fullAligned);

                using (var gray0 = new Mat(16, 16, MatType.CV_8UC1, new Scalar(80)))
                using (var gray1 = new Mat(16, 16, MatType.CV_8UC1, new Scalar(80)))
                using (var shifted = align.ShiftMat(gray0, new Point(1, -1)))
                using (var threshold = new Mat())
                using (var exclude = new Mat())
                {
                    Point shift = align.CalculateShift(gray0, gray1);
                    Assert.InRange(shift.X, -15, 15);
                    Assert.InRange(shift.Y, -15, 15);
                    Assert.Equal(gray0.Type, shifted.Type);
                    Assert.Equal(gray0.Size, shifted.Size);
                    align.ComputeBitmaps(gray0, threshold, exclude);
                    Assert.Equal(MatType.CV_8UC1, threshold.Type);
                    Assert.Equal(MatType.CV_8UC1, exclude.Type);
                    Assert.Equal(gray0.Size, threshold.Size);
                    Assert.Equal(gray0.Size, exclude.Size);
                }
            }
        }

        [Fact]
        public void CalibrationAndMergeProduceExpectedOwnedShapes()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var image0 = new Mat(16, 16, MatType.CV_8UC3, new Scalar(32, 40, 48)))
            using (var image1 = new Mat(16, 16, MatType.CV_8UC3, new Scalar(96, 104, 112)))
            using (var image2 = new Mat(16, 16, MatType.CV_8UC3, new Scalar(192, 200, 208)))
            using (var times = new Mat(3, 1, MatType.CV_32FC1, new Scalar(0.5)))
            using (var calibrateDebevec = PhotoCv2.CreateCalibrateDebevec(samples: 16))
            using (var calibrateRobertson = PhotoCv2.CreateCalibrateRobertson(maxIter: 1))
            using (var mergeDebevec = PhotoCv2.CreateMergeDebevec())
            using (var mergeMertens = PhotoCv2.CreateMergeMertens())
            using (var mergeRobertson = PhotoCv2.CreateMergeRobertson())
            {
                Mat[] images = { image0, image1, image2 };
                using (Mat debevecResponse = calibrateDebevec.Process(images, times))
                using (Mat robertsonResponse = calibrateRobertson.Process(images, times))
                using (Mat radiance = calibrateRobertson.GetRadiance())
                using (Mat mergedDebevec = mergeDebevec.Process(images, times))
                using (Mat mergedMertens = mergeMertens.Process(images))
                using (Mat mergedRobertson = mergeRobertson.Process(images, times))
                {
                    AssertResponse(debevecResponse);
                    AssertResponse(robertsonResponse);
                    Assert.Equal(new Size(16, 16), radiance.Size);
                    Assert.Equal(MatType.CV_32FC3, radiance.Type);
                    AssertMerge(mergedDebevec);
                    AssertMerge(mergedMertens);
                    AssertMerge(mergedRobertson);
                }
            }
        }

        [Fact]
        public void MergeDebevecSupportsAllDocumentedInputDepths()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var times = new Mat(2, 1, MatType.CV_32FC1, new Scalar(0.5)))
            using (var merger = PhotoCv2.CreateMergeDebevec())
            {
                AssertMergedDepth(merger, times, MatType.CV_8UC1, 32.0, 96.0);
                AssertMergedDepth(merger, times, MatType.CV_16UC1, 8192.0, 32768.0);
                AssertMergedDepth(merger, times, MatType.CV_32FC1, 0.25, 0.75);
            }
        }

        [Fact]
        public void HdrManagedValidationRejectsInvalidCollectionsTimesAndAliasing()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var color = new Mat(8, 8, MatType.CV_8UC3, new Scalar(32, 64, 96)))
            using (var colorMismatch = new Mat(7, 8, MatType.CV_8UC3))
            using (var gray = new Mat(8, 8, MatType.CV_8UC1))
            using (var badDepth = new Mat(8, 8, MatType.CV_64FC1))
            using (var times = new Mat(1, 1, MatType.CV_32FC1, new Scalar(1.0)))
            using (var badTimes = new Mat(1, 1, MatType.CV_64FC1, new Scalar(1.0)))
            using (var output = new Mat())
            using (var align = PhotoCv2.CreateAlignMTB())
            using (var calibrate = PhotoCv2.CreateCalibrateDebevec())
            using (var merge = PhotoCv2.CreateMergeDebevec())
            {
                Assert.Throws<ArgumentNullException>(() => align.Process(null!));
                Assert.Throws<ArgumentException>(() => align.Process(Array.Empty<Mat>()));
                Assert.Throws<ArgumentException>(() => align.Process(new[] { color, colorMismatch }));
                Assert.Throws<ArgumentException>(() => align.Process(new[] { color }, Array.Empty<Mat>()));
                Assert.Throws<ArgumentException>(() => calibrate.Process(new[] { gray }, output, badTimes));
                Assert.Throws<ArgumentException>(() => merge.Process(new[] { badDepth }, output, times));
                Assert.Throws<ArgumentException>(() => align.ShiftMat(gray, gray, new Point(1, 0)));
                Assert.Throws<ArgumentOutOfRangeException>(() => align.ShiftMat(gray, output, new Point(8, 0)));
                Assert.Throws<ArgumentException>(() => align.ComputeBitmaps(gray, gray, output));
            }
        }

        private static void AssertResponse(Mat response)
        {
            Assert.Equal(256, response.Rows);
            Assert.Equal(1, response.Cols);
            Assert.Equal(MatType.CV_32FC3, response.Type);
        }

        private static void AssertMerge(Mat result)
        {
            Assert.Equal(new Size(16, 16), result.Size);
            Assert.Equal(MatType.CV_32FC3, result.Type);
        }

        private static void AssertMergedDepth(
            MergeDebevec merger,
            Mat times,
            int type,
            double firstValue,
            double secondValue)
        {
            using (var first = new Mat(8, 8, type, new Scalar(firstValue)))
            using (var second = new Mat(8, 8, type, new Scalar(secondValue)))
            using (Mat merged = merger.Process(new[] { first, second }, times))
            {
                Assert.Equal(new Size(8, 8), merged.Size);
                Assert.Equal(MatType.CV_32FC1, merged.Type);
            }
        }

        private static void DisposeAll(Mat[] values)
        {
            foreach (Mat value in values)
            {
                value.Dispose();
            }
        }
    }
}
