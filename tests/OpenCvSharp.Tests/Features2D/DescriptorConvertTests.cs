using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

namespace JYPPX.OpenCvSharp.Tests.Features2D
{
    [Collection(NativeSmokeCollection.Name)]
    public class DescriptorConvertTests
    {
        [Fact]
        public void AllocatingHelpersRejectNullSourceBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() => DescriptorConvert.ConvertDescriptorsToFloat(null!));
            Assert.Throws<ArgumentNullException>(() => DescriptorConvert.NormalizeDescriptors(null!));
            Assert.Throws<ArgumentNullException>(() => DescriptorConvert.ConvertToFloatAndNormalize(null!));
        }

        [Fact]
        public void OutputHelpersRejectNullSourceBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() => DescriptorConvert.ConvertDescriptorsToFloat(null!, null!));
            Assert.Throws<ArgumentNullException>(() => DescriptorConvert.NormalizeDescriptors(null!, null!));
            Assert.Throws<ArgumentNullException>(() => DescriptorConvert.ConvertToFloatAndNormalize(null!, null!));
        }

        [Fact]
        public void ConvertDescriptorsToFloatPreservesShapeAndValuesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(2, 2, MatType.CV_8UC1))
            using (Mat dst = new Mat())
            {
                src.CopyFrom(new byte[] { 1, 2, 3, 4 });

                DescriptorConvert.ConvertDescriptorsToFloat(src, dst);

                Assert.Equal(2, dst.Rows);
                Assert.Equal(2, dst.Cols);
                Assert.Equal(MatType.CV_32FC1, dst.Type);
                Assert.Equal(new[] { 1.0F, 2.0F, 3.0F, 4.0F }, dst.ToArray<float>());

                using (Mat owned = DescriptorConvert.ConvertDescriptorsToFloat(src))
                {
                    Assert.Equal(dst.ToArray<float>(), owned.ToArray<float>());
                }
            }
        }

        [Fact]
        public void NormalizeDescriptorsWritesCallerDestinationWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(1, 2, MatType.CV_32FC1))
            using (Mat dst = new Mat())
            {
                src.CopyFrom(new[] { 3.0F, 4.0F });

                DescriptorConvert.NormalizeDescriptors(src, dst);

                Assert.Equal(1, dst.Rows);
                Assert.Equal(2, dst.Cols);
                Assert.Equal(MatType.CV_32FC1, dst.Type);
                AssertDescriptorValuesNear(new[] { 0.6F, 0.8F }, dst.ToArray<float>());
            }
        }

        private static void AssertDescriptorValuesNear(float[] expected, float[] actual)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.True(Math.Abs(expected[i] - actual[i]) < 1.0e-5F);
            }
        }
    }
}
