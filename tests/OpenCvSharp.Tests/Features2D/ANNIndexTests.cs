using System;
using System.IO;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace OpenCvSharp.Tests.Features2D
{
    public class ANNIndexTests
    {
        [Fact]
        public void CreateRejectsInvalidConfigurationBeforeNativeCall()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ANNIndex.Create(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ANNIndex.Create(2, (ANNIndexDistance)99));
        }

        [Fact]
        public void EuclideanIndexBuildsAndSearchesDeterministically()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled()) return;
            ANNIndex? index = TryCreate(2);
            if (index == null) return;

            using (index)
            using (Mat features = CreateFloatMat(4, 2, 0.0F, 0.0F, 10.0F, 10.0F, 2.0F, 2.0F, -2.0F, -2.0F))
            using (Mat query = CreateFloatMat(2, 2, 0.1F, 0.1F, 9.5F, 10.5F))
            using (var indices = new Mat())
            using (var distances = new Mat())
            {
                index.SetSeed(1234);
                index.AddItems(features);
                index.Build(2);
                index.KnnSearch(query, indices, distances, 1);

                Assert.Equal(4, index.ItemNumber);
                Assert.Equal(2, index.TreeNumber);
                Assert.Equal(2, indices.Rows);
                Assert.Equal(1, indices.Cols);
                Assert.Equal(MatType.CV_32SC1, indices.Type);
                Assert.Equal(MatType.CV_32FC1, distances.Type);
                Assert.Equal(new[] { 0, 1 }, indices.ToArray<int>());
                Assert.All(distances.ToArray<float>(), value => Assert.True(value >= 0.0F));
            }
        }

        [Fact]
        public void AddItemsAcceptsRowStridedInputButQueryRequiresContinuity()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled()) return;
            ANNIndex? index = TryCreate(2);
            if (index == null) return;

            using (index)
            using (Mat featureParent = CreateFloatMat(
                4,
                3,
                0.0F, 0.0F, 99.0F,
                10.0F, 10.0F, 99.0F,
                2.0F, 2.0F, 99.0F,
                -2.0F, -2.0F, 99.0F))
            using (Mat features = featureParent.ColRange(0, 2))
            using (Mat queryParent = CreateFloatMat(2, 3, 0.1F, 0.1F, 99.0F, 9.5F, 10.5F, 99.0F))
            using (Mat query = queryParent.ColRange(0, 2))
            using (var indices = new Mat())
            using (var distances = new Mat())
            {
                Assert.False(features.IsContinuous);
                Assert.False(query.IsContinuous);
                index.AddItems(features);
                index.Build(2);
                Assert.Throws<ArgumentException>(() => index.KnnSearch(query, indices, distances, 1));
            }
        }

        [Fact]
        public void HammingIndexRequiresByteRowsAndReturnsByteDistances()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled()) return;
            ANNIndex? index = TryCreate(2, ANNIndexDistance.Hamming);
            if (index == null) return;

            using (index)
            using (Mat features = CreateByteMat(3, 2, 0, 0, 255, 255, 15, 15))
            using (Mat query = CreateByteMat(1, 2, 1, 0))
            using (var indices = new Mat())
            using (var distances = new Mat())
            using (var wrongType = new Mat(1, 2, MatType.CV_32FC1))
            {
                Assert.Throws<ArgumentException>(() => index.AddItems(wrongType));
                index.AddItems(features);
                index.Build(2);
                index.KnnSearch(query, indices, distances, 1);

                Assert.Equal(new[] { 0 }, indices.ToArray<int>());
                Assert.Equal(MatType.CV_8UC1, distances.Type);
            }
        }

        [Fact]
        public void SaveAndLoadRoundTripUsesUtf8Path()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled()) return;
            ANNIndex? index = TryCreate(2);
            if (index == null) return;

            string directory = Path.Combine(Path.GetTempPath(), "opencv-ann-\u7279\u5f81-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(directory, "index-\u8fd1\u90bb.ann");
            Directory.CreateDirectory(directory);
            try
            {
                using (index)
                using (Mat features = CreateFloatMat(3, 2, 0.0F, 0.0F, 5.0F, 5.0F, 10.0F, 10.0F))
                {
                    index.AddItems(features);
                    index.Build(2);
                    index.Save(path);
                    Assert.True(File.Exists(path));
                }

                using (ANNIndex loaded = ANNIndex.Create(2))
                using (Mat query = CreateFloatMat(1, 2, 5.1F, 4.9F))
                using (var indices = new Mat())
                using (var distances = new Mat())
                {
                    loaded.Load(path);
                    Assert.Equal(3, loaded.ItemNumber);
                    loaded.KnnSearch(query, indices, distances, 1);
                    Assert.Equal(new[] { 1 }, indices.ToArray<int>());
                }
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
                if (Directory.Exists(directory)) Directory.Delete(directory);
            }
        }

        [Fact]
        public void OnDiskBuildWritesTheRequestedUtf8Path()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled()) return;
            ANNIndex? index = TryCreate(2);
            if (index == null) return;

            string directory = Path.Combine(Path.GetTempPath(), "opencv-ann-disk-\u7279\u5f81-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(directory, "build-\u7d22\u5f15.ann");
            Directory.CreateDirectory(directory);
            try
            {
                using (index)
                using (Mat features = CreateFloatMat(3, 2, 0.0F, 0.0F, 5.0F, 5.0F, 10.0F, 10.0F))
                {
                    index.SetSeed(42);
                    Assert.True(index.SetOnDiskBuild(path));
                    index.AddItems(features);
                    index.Build(2);
                    Assert.True(File.Exists(path));
                    Assert.True(new FileInfo(path).Length > 0);
                }
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
                if (Directory.Exists(directory)) Directory.Delete(directory);
            }
        }

        [Fact]
        public void InvalidArgumentsAndDisposedUseFailBeforeUnsafeAccess()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled()) return;
            ANNIndex? index = TryCreate(2);
            if (index == null) return;

            using (Mat features = CreateFloatMat(2, 2, 0.0F, 0.0F, 1.0F, 1.0F))
            using (Mat wrongColumns = new Mat(1, 3, MatType.CV_32FC1))
            using (Mat wrongType = new Mat(1, 2, MatType.CV_8UC1))
            using (var output = new Mat())
            using (var secondOutput = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => index.AddItems(null!));
                Assert.Throws<ArgumentException>(() => index.AddItems(wrongColumns));
                Assert.Throws<ArgumentException>(() => index.AddItems(wrongType));
                Assert.Throws<ArgumentOutOfRangeException>(() => index.Build(0));
                Assert.Throws<ArgumentException>(() => index.Save(string.Empty));
                Assert.Throws<ArgumentException>(() => index.Load("bad\0path"));

                index.AddItems(features);
                index.Build(2);
                Assert.Throws<ArgumentOutOfRangeException>(() => index.KnnSearch(features, output, secondOutput, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => index.KnnSearch(features, output, secondOutput, 3));
                Assert.Throws<ArgumentOutOfRangeException>(() => index.KnnSearch(features, output, secondOutput, 1, 0));
                Assert.Throws<ArgumentException>(() => index.KnnSearch(features, output, output, 1));

                index.Dispose();
                index.Dispose();
                Assert.True(index.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => index.ItemNumber);
                Assert.Throws<ObjectDisposedException>(() => index.SetSeed(1));
                Assert.Throws<ObjectDisposedException>(() => index.KnnSearch(features, output, secondOutput, 1));
            }
        }

        private static ANNIndex? TryCreate(int dimension, ANNIndexDistance distance = ANNIndexDistance.Euclidean)
        {
            try
            {
                return ANNIndex.Create(dimension, distance);
            }
            catch (OpenCvException exception) when (Feature2DTestData.IsFeaturesModuleMissing(exception))
            {
                return null;
            }
        }

        private static Mat CreateFloatMat(int rows, int columns, params float[] values)
        {
            var result = new Mat(rows, columns, MatType.CV_32FC1);
            result.CopyFrom(values);
            return result;
        }

        private static Mat CreateByteMat(int rows, int columns, params byte[] values)
        {
            var result = new Mat(rows, columns, MatType.CV_8UC1);
            result.CopyFrom(values);
            return result;
        }
    }
}
