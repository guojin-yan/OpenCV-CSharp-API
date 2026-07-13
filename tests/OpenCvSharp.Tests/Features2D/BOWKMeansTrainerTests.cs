using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace OpenCvSharp.Tests.Features2D
{
    public class BOWKMeansTrainerTests
    {
        [Fact]
        public void ConstructorValidationRejectsInvalidArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BOWKMeansTrainer(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BOWKMeansTrainer(2, attempts: 0));
        }

        [Fact]
        public void ManagedValidationRejectsInvalidDescriptorCollectionsBeforeNativeCall()
        {
            using (var trainer = new BOWKMeansTrainer(2))
            {
                Assert.Throws<ArgumentNullException>(() => trainer.Add((Mat[])null!));
                Assert.Throws<ArgumentNullException>(() => trainer.Add(new Mat[] { null! }));
                trainer.Add(Array.Empty<Mat>());
                Assert.Equal(0, trainer.DescriptorsCount);
                Assert.False(trainer.HasDescriptors);

                Assert.Throws<ArgumentNullException>(() => trainer.Cluster((Mat[])null!));
                Assert.Throws<ArgumentException>(() => trainer.Cluster(Array.Empty<Mat>()));
                Assert.Throws<ArgumentNullException>(() => trainer.Cluster(new Mat[] { null! }));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ArgumentNullException>(() => trainer.Add(new Mat[] { null! }.AsSpan()));
                Assert.Throws<ArgumentException>(() => trainer.Cluster(ReadOnlySpan<Mat>.Empty));
                Assert.Throws<ArgumentNullException>(() => trainer.Cluster(new Mat[] { null! }.AsSpan()));
#endif
            }
        }

        [Fact]
        public void AddGetClearAndDisposeManageDescriptorClonesWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat first = Feature2DTestData.CreateFloatDescriptors(
                0.0F, 0.0F,
                0.2F, 0.1F))
            using (Mat second = Feature2DTestData.CreateFloatDescriptors(
                9.8F, 10.0F,
                10.2F, 9.9F))
            {
                var trainer = new BOWKMeansTrainer(2, TermCriteria.ByCountAndEpsilon(20, 0.001), attempts: 1);
                trainer.Add(first);
                trainer.Add(second);

                Assert.False(trainer.IsDisposed);
                Assert.Equal(4, trainer.DescriptorsCount);
                Assert.True(trainer.HasDescriptors);
                Assert.Equal("{ClusterCount=2,DescriptorsCount=4,Attempts=1}", trainer.ToString());

                Mat[] descriptors = trainer.GetDescriptors();
                try
                {
                    Assert.Equal(2, descriptors.Length);
                    Assert.Equal(2, descriptors[0].Rows);
                    Assert.Equal(2, descriptors[1].Rows);
                    Assert.NotSame(first, descriptors[0]);
                }
                finally
                {
                    DisposeAll(descriptors);
                }

                trainer.Clear();
                Assert.Equal(0, trainer.DescriptorsCount);
                Assert.False(trainer.HasDescriptors);

                trainer.Dispose();
                Assert.True(trainer.IsDisposed);
                Assert.Equal("{Disposed=True}", trainer.ToString());
                Assert.Throws<ObjectDisposedException>(() => trainer.DescriptorsCount);
                Assert.Throws<ObjectDisposedException>(() => trainer.HasDescriptors);
                Assert.Throws<ObjectDisposedException>(() => trainer.Add(first));
                Assert.Throws<ObjectDisposedException>(() => trainer.Add(new[] { first }));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ObjectDisposedException>(() => trainer.Add(new ReadOnlySpan<Mat>(new[] { first })));
#endif
                Assert.Throws<ObjectDisposedException>(() => trainer.GetDescriptors());
                Assert.Throws<ObjectDisposedException>(() => trainer.Clear());
                Assert.Throws<ObjectDisposedException>(() => trainer.Cluster());
                Assert.Throws<ObjectDisposedException>(() => trainer.Cluster(first));
                Assert.Throws<ObjectDisposedException>(() => trainer.Cluster(new[] { first }));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ObjectDisposedException>(() => trainer.Cluster(new ReadOnlySpan<Mat>(new[] { first })));
#endif
            }
        }

        [Fact]
        public void ClusterReturnsVocabularyCentersWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat descriptors = Feature2DTestData.CreateFloatDescriptors(
                0.0F, 0.0F,
                0.2F, 0.1F,
                9.8F, 10.0F,
                10.2F, 9.9F))
            using (var trainer = new BOWKMeansTrainer(2, TermCriteria.ByCountAndEpsilon(20, 0.001), attempts: 1))
            {
                using (Mat firstHalf = descriptors.RowRange(0, 2))
                using (Mat secondHalf = descriptors.RowRange(2, 4))
                {
                    trainer.Add(firstHalf);
                    trainer.Add(secondHalf);

                    using (Mat fromStored = trainer.Cluster())
                    using (Mat fromDirect = trainer.Cluster(descriptors))
                    {
                        Assert.Equal(2, fromStored.Rows);
                        Assert.Equal(2, fromStored.Cols);
                        Assert.Equal(MatType.CV_32FC1, fromStored.Type);
                        Assert.Equal(2, fromDirect.Rows);
                        Assert.Equal(2, fromDirect.Cols);
                    }
                }
            }
        }

        [Fact]
        public void ManagedValidationRejectsInvalidDescriptorRowsWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var trainer = new BOWKMeansTrainer(2))
            using (Mat bytes = new Mat(1, 2, MatType.CV_8UC1))
            using (Mat oneRow = Feature2DTestData.CreateFloatDescriptors(1.0F, 2.0F))
            {
                Assert.Throws<ArgumentNullException>(() => trainer.Add((Mat)null!));
                Assert.Throws<ArgumentException>(() => trainer.Add(bytes));
                Assert.Throws<ArgumentException>(() => trainer.Cluster(oneRow));
                Assert.Throws<InvalidOperationException>(() => trainer.Cluster());
            }
        }

        private static void DisposeAll(Mat[] descriptors)
        {
            for (int i = 0; i < descriptors.Length; i++)
            {
                descriptors[i].Dispose();
            }
        }
    }
}
