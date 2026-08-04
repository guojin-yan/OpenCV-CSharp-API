using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Photo;

namespace JYPPX.OpenCvSharp.Tests.Photo
{
    public sealed class PhotoSecondBatchTests
    {
        [Fact]
        public void EnumValuesMatchOpenCvPhotoConstants()
        {
            Assert.Equal(1, (int)SeamlessCloneFlags.NormalClone);
            Assert.Equal(2, (int)SeamlessCloneFlags.MixedClone);
            Assert.Equal(3, (int)SeamlessCloneFlags.MonochromeTransfer);
            Assert.Equal(9, (int)SeamlessCloneFlags.NormalCloneWide);
            Assert.Equal(10, (int)SeamlessCloneFlags.MixedCloneWide);
            Assert.Equal(11, (int)SeamlessCloneFlags.MonochromeTransferWide);

            Assert.Equal(1, (int)EdgePreservingFilterFlags.RecursiveFilter);
            Assert.Equal(2, (int)EdgePreservingFilterFlags.NormalizedConvolutionFilter);
        }

        [Fact]
        public void PhotoSecondBatchValidatesFirstManagedArgumentBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.Decolor(null!, null!, null!));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.SeamlessClone(null!, null!, null!, new Point(0, 0), null!, SeamlessCloneFlags.NormalClone));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.ColorChange(null!, null!, null!));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.IlluminationChange(null!, null!, null!));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.TextureFlattening(null!, null!, null!));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.EdgePreservingFilter(null!, null!));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.EdgePreservingFilter(null!));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.DetailEnhance(null!, null!));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.DetailEnhance(null!));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.PencilSketch(null!, null!, null!));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.Stylization(null!, null!));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.Stylization(null!));
        }

        [Fact]
        public void EdgePreservingFilterRejectsInvalidFlagsBeforeNativeCall()
        {
            using (var src = new Mat())
            using (var dst = new Mat())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => PhotoCv2.EdgePreservingFilter(src, dst, (EdgePreservingFilterFlags)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => PhotoCv2.EdgePreservingFilter(src, (EdgePreservingFilterFlags)99));
            }
        }

        [Fact]
        public void SeamlessCloneRejectsInvalidFlagsBeforeNativeCall()
        {
            using (var src = new Mat())
            using (var dst = new Mat())
            using (var mask = new Mat())
            using (var blend = new Mat())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => PhotoCv2.SeamlessClone(src, dst, mask, new Point(0, 0), blend, (SeamlessCloneFlags)99));
            }
        }

        [Fact]
        public void SeamlessCloneRejectsEmptySourceAndDestinationBeforeNativeCall()
        {
            using (var empty = new Mat())
            using (var source = new Mat(4, 4, MatType.CV_8UC3))
            using (var destination = new Mat(4, 4, MatType.CV_8UC3))
            using (var mask = new Mat())
            using (var blend = new Mat())
            {
                ArgumentException sourceException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.SeamlessClone(empty, destination, mask, new Point(0, 0), blend, SeamlessCloneFlags.NormalClone));
                Assert.Equal("src", sourceException.ParamName);

                ArgumentException destinationException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.SeamlessClone(source, empty, mask, new Point(0, 0), blend, SeamlessCloneFlags.NormalClone));
                Assert.Equal("dst", destinationException.ParamName);
            }
        }

        [Fact]
        public void PhotoSecondBatchValidatesRemainingManagedArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat())
            using (Mat dst = new Mat())
            using (Mat mask = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.Decolor(src, null!, dst));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.Decolor(src, dst, null!));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.SeamlessClone(src, null!, mask, new Point(0, 0), dst, SeamlessCloneFlags.NormalClone));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.SeamlessClone(src, dst, null!, new Point(0, 0), dst, SeamlessCloneFlags.NormalClone));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.SeamlessClone(src, dst, mask, new Point(0, 0), null!, SeamlessCloneFlags.NormalClone));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.ColorChange(src, null!, dst));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.ColorChange(src, mask, null!));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.IlluminationChange(src, null!, dst));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.TextureFlattening(src, null!, dst));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.EdgePreservingFilter(src, null!));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.PencilSketch(src, null!, dst));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.PencilSketch(src, dst, null!));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.DetailEnhance(null!));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.Stylization(null!));
            }
        }

        [Fact]
        public void SingleOutputReturningOverloadsRunWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(8, 8, MatType.CV_8UC3, new Scalar(32, 64, 96)))
            using (Mat edgePreserved = PhotoCv2.EdgePreservingFilter(src, EdgePreservingFilterFlags.RecursiveFilter))
            using (Mat detailed = PhotoCv2.DetailEnhance(src))
            using (Mat stylized = PhotoCv2.Stylization(src))
            {
                AssertOutputMatchesSource(src, edgePreserved);
                AssertOutputMatchesSource(src, detailed);
                AssertOutputMatchesSource(src, stylized);
            }
        }

        private static void AssertOutputMatchesSource(Mat source, Mat output)
        {
            Assert.Equal(source.Rows, output.Rows);
            Assert.Equal(source.Cols, output.Cols);
            Assert.Equal(source.Type, output.Type);
        }

    }
}
