using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Tests.Core
{
    public class KMeansTests
    {
        [Fact]
        public void VConcatAndKMeansWorkWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat a = CreateSamples(0.0F, 0.0F, 0.1F, 0.2F))
            using (Mat b = CreateSamples(9.8F, 10.0F, 10.0F, 10.1F))
            using (Mat merged = new Mat())
            using (Mat labels = new Mat())
            using (Mat centers = new Mat())
            {
                Cv2.VConcat(new[] { a, b }, merged);
                double compactness = Cv2.KMeans(
                    merged,
                    2,
                    labels,
                    TermCriteria.ByCountAndEpsilon(20, 0.001),
                    1,
                    KMeansFlags.PpCenters,
                    centers);

                Assert.Equal(4, merged.Rows);
                Assert.Equal(2, merged.Cols);
                Assert.Equal(4, labels.Rows);
                Assert.Equal(2, centers.Rows);
                Assert.Equal(2, centers.Cols);
                Assert.True(compactness >= 0.0);

                using (Mat returnedMerged = Cv2.VConcat(new[] { a, b }))
                {
                    Assert.Equal(merged.ToArray<float>(), returnedMerged.ToArray<float>());
                }

#if NETCOREAPP3_1_OR_GREATER
                using (Mat spanMerged = Cv2.VConcat(new[] { a, b }.AsSpan()))
                {
                    Assert.Equal(merged.ToArray<float>(), spanMerged.ToArray<float>());
                }
#endif
            }
        }

        [Fact]
        public void HConcatWorksWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat a = CreateSamples(0.0F, 0.0F, 0.1F, 0.2F))
            using (Mat b = CreateSamples(9.8F, 10.0F, 10.0F, 10.1F))
            using (Mat merged = new Mat())
            {
                Cv2.HConcat(new[] { a, b }, merged);

                Assert.Equal(2, merged.Rows);
                Assert.Equal(4, merged.Cols);
                Assert.Equal(new[] { 0.0F, 0.0F, 9.8F, 10.0F, 0.1F, 0.2F, 10.0F, 10.1F }, merged.ToArray<float>());

                using (Mat returnedMerged = Cv2.HConcat(new[] { a, b }))
                {
                    Assert.Equal(merged.ToArray<float>(), returnedMerged.ToArray<float>());
                }

#if NETCOREAPP3_1_OR_GREATER
                using (Mat spanMerged = Cv2.HConcat(new[] { a, b }.AsSpan()))
                {
                    Assert.Equal(merged.ToArray<float>(), spanMerged.ToArray<float>());
                }
#endif
            }
        }

        [Fact]
        public void ClusteringValidationRejectsInvalidArguments()
        {
            Assert.Throws<ArgumentNullException>(() => Cv2.HConcat(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.HConcat((Mat[])null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.VConcat(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.VConcat((Mat[])null!));
            Assert.Throws<ArgumentNullException>(() => Cv2.KMeans(null!, 2, null!, null!));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Cv2.KMeans(new Mat(), 2, new Mat(), TermCriteria.ByCount(1), 1, (KMeansFlags)3, new Mat()));

            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat empty = new Mat())
            {
                Assert.Throws<ArgumentException>(() => Cv2.HConcat(Array.Empty<Mat>(), empty));
                Assert.Throws<ArgumentException>(() => Cv2.HConcat(Array.Empty<Mat>()));
                Assert.Throws<ArgumentException>(() => Cv2.VConcat(Array.Empty<Mat>(), empty));
                Assert.Throws<ArgumentException>(() => Cv2.VConcat(Array.Empty<Mat>()));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ArgumentException>(() => Cv2.HConcat(Array.Empty<Mat>().AsSpan()));
                Assert.Throws<ArgumentException>(() => Cv2.VConcat(Array.Empty<Mat>().AsSpan()));
#endif
                Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.KMeans(empty, 0, empty, empty));
            }
        }

        [Fact]
        public void HConcatRejectsInvalidSourceContracts()
        {
            using (var first = new Mat(2, 2, MatType.CV_8UC1))
            using (var mismatchedRows = new Mat(3, 2, MatType.CV_8UC1))
            using (var mismatchedType = new Mat(2, 2, MatType.CV_32FC1))
            using (var image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (Mat blob = OpenCvSharp.Dnn.Cv2.BlobFromImage(image, 1.0, new Size(4, 4)))
            using (var dst = new Mat())
            {
                Assert.Equal(4, blob.Dims);

                ArgumentException dimsException = Assert.Throws<ArgumentException>(() =>
                    Cv2.HConcat(new[] { first, blob }, dst));
                ArgumentException rowsException = Assert.Throws<ArgumentException>(() =>
                    Cv2.HConcat(new[] { first, mismatchedRows }, dst));
                ArgumentException typeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.HConcat(new[] { first, mismatchedType }, dst));

                Assert.Equal("src", dimsException.ParamName);
                Assert.Equal("src", rowsException.ParamName);
                Assert.Equal("src", typeException.ParamName);
            }
        }

        [Fact]
        public void VConcatRejectsInvalidSourceContracts()
        {
            using (var first = new Mat(2, 2, MatType.CV_8UC1))
            using (var mismatchedCols = new Mat(2, 3, MatType.CV_8UC1))
            using (var mismatchedType = new Mat(2, 2, MatType.CV_32FC1))
            using (var image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (Mat blob = OpenCvSharp.Dnn.Cv2.BlobFromImage(image, 1.0, new Size(4, 4)))
            using (var dst = new Mat())
            {
                Assert.Equal(4, blob.Dims);

                ArgumentException dimsException = Assert.Throws<ArgumentException>(() =>
                    Cv2.VConcat(new[] { first, blob }, dst));
                ArgumentException colsException = Assert.Throws<ArgumentException>(() =>
                    Cv2.VConcat(new[] { first, mismatchedCols }, dst));
                ArgumentException typeException = Assert.Throws<ArgumentException>(() =>
                    Cv2.VConcat(new[] { first, mismatchedType }, dst));

                Assert.Equal("src", dimsException.ParamName);
                Assert.Equal("src", colsException.ParamName);
                Assert.Equal("src", typeException.ParamName);
            }
        }

        [Fact]
        public void KMeansRejectsInvalidDataContracts()
        {
            using (var labels = new Mat())
            using (var centers = new Mat())
            using (var nonFloat = new Mat(2, 2, MatType.CV_8UC1))
            using (var tooFewSamples = new Mat(1, 1, MatType.CV_32FC1))
            using (var image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (Mat blob = OpenCvSharp.Dnn.Cv2.BlobFromImage(image, 1.0, new Size(4, 4)))
            {
                Assert.Equal(4, blob.Dims);

                ArgumentException dimsException = Assert.Throws<ArgumentException>(() =>
                    Cv2.KMeans(blob, 2, labels, centers));
                ArgumentException depthException = Assert.Throws<ArgumentException>(() =>
                    Cv2.KMeans(nonFloat, 2, labels, centers));
                ArgumentException clusterCountException = Assert.Throws<ArgumentException>(() =>
                    Cv2.KMeans(tooFewSamples, 2, labels, centers));

                Assert.Equal("data", dimsException.ParamName);
                Assert.Equal("data", depthException.ParamName);
                Assert.Equal("k", clusterCountException.ParamName);
            }
        }

        private static Mat CreateSamples(params float[] values)
        {
            Mat mat = new Mat(values.Length / 2, 2, MatType.CV_32FC1);
            mat.CopyFrom(values);
            return mat;
        }

    }
}
