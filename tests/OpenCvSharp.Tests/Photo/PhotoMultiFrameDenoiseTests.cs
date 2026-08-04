using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Photo;

namespace JYPPX.OpenCvSharp.Tests.Photo
{
    public sealed class PhotoMultiFrameDenoiseTests
    {
        [Fact]
        public void MultiFrameDenoiseValidatesManagedArguments()
        {
            using (var dst = new Mat())
            using (var frame = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(null!, dst, 0, 1));
                Assert.Throws<ArgumentNullException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(Array.Empty<Mat>(), null!, 0, 1));
                Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(Array.Empty<Mat>(), dst, 0, 1));
                Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(new Mat[] { null! }, dst, 0, 1));
                Assert.Throws<ArgumentNullException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(Array.Empty<Mat>(), dst, 0, 1, (float[])null!));
                Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(new[] { frame }, dst, 0, 1, Array.Empty<float>()));
                Assert.Throws<ArgumentNullException>(() =>
                    PhotoCv2.FastNlMeansDenoisingColoredMulti(null!, dst, 0, 1));
                Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingColoredMulti(Array.Empty<Mat>(), dst, 0, 1));
                Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingColoredMulti(new Mat[] { null! }, dst, 0, 1));
            }
        }

        [Fact]
        public void MultiFrameDenoiseSpanOverloadsValidateManagedArguments()
        {
            using (var dst = new Mat())
            using (var frame = new Mat())
            {
                Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(ReadOnlySpan<Mat>.Empty, dst, 0, 1));
                Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(new Mat[] { null! }.AsSpan(), dst, 0, 1));
                Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(ReadOnlySpan<Mat>.Empty, dst, 0, 1, ReadOnlySpan<float>.Empty));
                Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(new[] { frame }.AsSpan(), dst, 0, 1, ReadOnlySpan<float>.Empty));
                Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(new Mat[] { null! }.AsSpan(), dst, 0, 1, new[] { 3.0F }.AsSpan()));
                Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingColoredMulti(ReadOnlySpan<Mat>.Empty, dst, 0, 1));
                Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingColoredMulti(new Mat[] { null! }.AsSpan(), dst, 0, 1));
            }
        }

        [Fact]
        public void MultiFrameDenoiseRejectsInvalidTemporalWindowContractBeforeNativeCall()
        {
            using (var frame1 = new Mat(4, 4, MatType.CV_8UC1))
            using (var frame2 = new Mat(4, 4, MatType.CV_8UC1))
            using (var frame3 = new Mat(4, 4, MatType.CV_8UC1))
            using (var dst = new Mat())
            {
                var frames = new[] { frame1, frame2, frame3 };

                ArgumentException temporalWindowException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(frames, dst, 1, 2));
                Assert.Equal("temporalWindowSize", temporalWindowException.ParamName);

                ArgumentException templateWindowException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(frames, dst, 1, 3, 3.0F, 4));
                Assert.Equal("templateWindowSize", templateWindowException.ParamName);

                ArgumentException searchWindowException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(frames, dst, 1, 3, new[] { 3.0F }, 7, 20));
                Assert.Equal("searchWindowSize", searchWindowException.ParamName);

                ArgumentOutOfRangeException indexException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(frames, dst, 0, 3));
                Assert.Equal("imgToDenoiseIndex", indexException.ParamName);
            }
        }

        [Fact]
        public void MultiFrameDenoiseRejectsMismatchedFrameContractBeforeNativeCall()
        {
            using (var frame1 = new Mat(4, 4, MatType.CV_8UC1))
            using (var sizeMismatch = new Mat(5, 4, MatType.CV_8UC1))
            using (var typeMismatch = new Mat(4, 4, MatType.CV_8UC3))
            using (var dst = new Mat())
            {
                ArgumentException sizeException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(new[] { frame1, sizeMismatch, frame1 }, dst, 1, 3));
                Assert.Equal("srcImages", sizeException.ParamName);

                ArgumentException typeException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(new[] { frame1, typeMismatch, frame1 }, dst, 1, 3, new[] { 3.0F }));
                Assert.Equal("srcImages", typeException.ParamName);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        [Fact]
        public void MultiFrameDenoiseSpanOverloadsRejectInvalidTemporalAndFrameContractsBeforeNativeCall()
        {
            using (var frame1 = new Mat(4, 4, MatType.CV_8UC1))
            using (var frame2 = new Mat(4, 4, MatType.CV_8UC1))
            using (var frame3 = new Mat(4, 4, MatType.CV_8UC1))
            using (var typeMismatch = new Mat(4, 4, MatType.CV_8UC3))
            using (var dst = new Mat())
            {
                ArgumentException temporalWindowException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(new[] { frame1, frame2, frame3 }.AsSpan(), dst, 1, 2));
                Assert.Equal("temporalWindowSize", temporalWindowException.ParamName);

                ArgumentOutOfRangeException indexException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(new[] { frame1, frame2, frame3 }.AsSpan(), dst, 2, 3, new[] { 3.0F }.AsSpan()));
                Assert.Equal("imgToDenoiseIndex", indexException.ParamName);

                ArgumentException frameException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingMulti(new[] { frame1, typeMismatch, frame3 }.AsSpan(), dst, 1, 3));
                Assert.Equal("srcImages", frameException.ParamName);
            }
        }
#endif

        [Fact]
        public void MultiFrameDenoiseRunsOnTinyFramesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var frame1 = new Mat(8, 8, MatType.CV_8UC1, new Scalar(10)))
            using (var frame2 = new Mat(8, 8, MatType.CV_8UC1, new Scalar(12)))
            using (var frame3 = new Mat(8, 8, MatType.CV_8UC1, new Scalar(11)))
            using (var dst = new Mat())
            {
                PhotoCv2.FastNlMeansDenoisingMulti(new[] { frame1, frame2, frame3 }, dst, 1, 3);

                Assert.Equal(8, dst.Rows);
                Assert.Equal(8, dst.Cols);
            }
        }

    }
}
