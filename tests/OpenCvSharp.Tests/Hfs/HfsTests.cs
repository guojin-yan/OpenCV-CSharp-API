using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Hfs;

namespace JYPPX.OpenCvSharp.Tests.Hfs
{
    public sealed class HfsTests
    {
        [Fact]
        public void ParametersExposeDefaultsAndValidateRanges()
        {
            HfsSegmentParams parameters = HfsSegmentParams.Default(32, 48);
            var same = new HfsSegmentParams(32, 48, 0.08F, 100, 0.28F, 200, 0.6F, 8, 5);
            var different = new HfsSegmentParams(32, 49, 0.08F, 100, 0.28F, 200, 0.6F, 8, 5);

            Assert.Equal(32, parameters.Height);
            Assert.Equal(48, parameters.Width);
            Assert.Equal(0.08F, parameters.SegEgbThresholdI, 4);
            Assert.Equal(100, parameters.MinRegionSizeI);
            Assert.Equal(0.28F, parameters.SegEgbThresholdII, 4);
            Assert.Equal(200, parameters.MinRegionSizeII);
            Assert.Equal(0.6F, parameters.SpatialWeight, 4);
            Assert.Equal(8, parameters.SlicSpixelSize);
            Assert.Equal(5, parameters.NumSlicIter);
            Assert.Equal(same, parameters);
            Assert.True(parameters == same);
            Assert.False(parameters != same);
            Assert.True(parameters != different);
            Assert.Equal(parameters.GetHashCode(), same.GetHashCode());
            Assert.Contains("SegEgbThresholdI", parameters.ToString());

            Assert.Throws<ArgumentOutOfRangeException>(() => new HfsSegmentParams(0, 32, 0.08F, 100, 0.28F, 200, 0.6F, 8, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HfsSegmentParams(32, 0, 0.08F, 100, 0.28F, 200, 0.6F, 8, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HfsSegmentParams(32, 32, 0.0F, 100, 0.28F, 200, 0.6F, 8, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HfsSegmentParams(32, 32, float.NaN, 100, 0.28F, 200, 0.6F, 8, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HfsSegmentParams(32, 32, 0.08F, 0, 0.28F, 200, 0.6F, 8, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HfsSegmentParams(32, 32, 0.08F, 100, 0.0F, 200, 0.6F, 8, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HfsSegmentParams(32, 32, 0.08F, 100, 0.28F, 0, 0.6F, 8, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HfsSegmentParams(32, 32, 0.08F, 100, 0.28F, 200, float.PositiveInfinity, 8, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HfsSegmentParams(32, 32, 0.08F, 100, 0.28F, 200, 0.6F, 0, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HfsSegmentParams(32, 32, 0.08F, 100, 0.28F, 200, 0.6F, 8, 0).Validate());
        }

        [Fact]
        public void ParametersFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                Assert.Equal(
                    "{Height=32,Width=48,SegEgbThresholdI=0.08,MinRegionSizeI=100,SegEgbThresholdII=0.28,MinRegionSizeII=200,SpatialWeight=0.6,SlicSpixelSize=8,NumSlicIter=5}",
                    HfsSegmentParams.Default(32, 48).ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void ParametersHaveSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(36, Marshal.SizeOf<HfsSegmentParams>());
            Assert.Equal(0, FieldOffset<HfsSegmentParams>("<Height>k__BackingField"));
            Assert.Equal(4, FieldOffset<HfsSegmentParams>("<Width>k__BackingField"));
            Assert.Equal(8, FieldOffset<HfsSegmentParams>("<SegEgbThresholdI>k__BackingField"));
            Assert.Equal(12, FieldOffset<HfsSegmentParams>("<MinRegionSizeI>k__BackingField"));
            Assert.Equal(16, FieldOffset<HfsSegmentParams>("<SegEgbThresholdII>k__BackingField"));
            Assert.Equal(20, FieldOffset<HfsSegmentParams>("<MinRegionSizeII>k__BackingField"));
            Assert.Equal(24, FieldOffset<HfsSegmentParams>("<SpatialWeight>k__BackingField"));
            Assert.Equal(28, FieldOffset<HfsSegmentParams>("<SlicSpixelSize>k__BackingField"));
            Assert.Equal(32, FieldOffset<HfsSegmentParams>("<NumSlicIter>k__BackingField"));
        }

        [Fact]
        public void ValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (HfsSegment? segment = TryCreateSegment())
            {
                if (segment == null)
                {
                    return;
                }

                using (Mat image = CreateBgrImage())
                using (Mat differentRows = new Mat(31, 32, MatType.CV_8UC3, new Scalar(0)))
                using (Mat differentCols = new Mat(32, 31, MatType.CV_8UC3, new Scalar(0)))
                using (Mat output = new Mat())
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => segment.SegEgbThresholdI = 0.0F);
                    Assert.Throws<ArgumentOutOfRangeException>(() => segment.SegEgbThresholdII = float.NaN);
                    Assert.Throws<ArgumentOutOfRangeException>(() => segment.SpatialWeight = float.PositiveInfinity);
                    Assert.Throws<ArgumentOutOfRangeException>(() => segment.MinRegionSizeI = 0);
                    Assert.Throws<ArgumentOutOfRangeException>(() => segment.MinRegionSizeII = 0);
                    Assert.Throws<ArgumentOutOfRangeException>(() => segment.SlicSpixelSize = 0);
                    Assert.Throws<ArgumentOutOfRangeException>(() => segment.NumSlicIter = 0);
                    Assert.Throws<ArgumentNullException>(() => segment.PerformSegmentCpu(null!, output));
                    Assert.Throws<ArgumentNullException>(() => segment.PerformSegmentCpu(image, null!));
                    Assert.Throws<ArgumentNullException>(() => segment.PerformSegmentCpu(null!));
                    Assert.Throws<ArgumentNullException>(() => segment.PerformSegmentGpu(null!, output));
                    Assert.Throws<ArgumentNullException>(() => segment.PerformSegmentGpu(image, null!));
                    Assert.Throws<ArgumentNullException>(() => segment.PerformSegmentGpu(null!));
                    Assert.Throws<ArgumentException>(() => segment.PerformSegmentCpu(differentRows, output));
                    Assert.Throws<ArgumentException>(() => segment.PerformSegmentCpu(differentCols));
                    Assert.Throws<ArgumentException>(() => segment.PerformSegmentGpu(differentRows, output));
                    Assert.Throws<ArgumentException>(() => segment.PerformSegmentGpu(differentCols));

                    segment.SegEgbThresholdI = 0.12F;
                    Assert.Equal(0.12F, segment.SegEgbThresholdI, 4);
                    segment.SegEgbThresholdII = 0.34F;
                    Assert.Equal(0.34F, segment.SegEgbThresholdII, 4);
                    segment.SpatialWeight = 0.75F;
                    Assert.Equal(0.75F, segment.SpatialWeight, 4);
                    segment.MinRegionSizeI = 16;
                    Assert.Equal(16, segment.MinRegionSizeI);
                    segment.MinRegionSizeII = 24;
                    Assert.Equal(24, segment.MinRegionSizeII);
                    segment.SlicSpixelSize = 12;
                    Assert.Equal(12, segment.SlicSpixelSize);
                    segment.NumSlicIter = 3;
                    Assert.Equal(3, segment.NumSlicIter);

                    segment.Dispose();
                    Assert.True(segment.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => segment.PerformSegmentCpu(image, output));
                    Assert.Throws<ObjectDisposedException>(() => segment.PerformSegmentCpu(image));
                    Assert.Throws<ObjectDisposedException>(() => segment.PerformSegmentGpu(image, output));
                    Assert.Throws<ObjectDisposedException>(() => segment.PerformSegmentGpu(image));
                    Assert.Throws<ObjectDisposedException>(() => segment.SegEgbThresholdI);
                }
            }
        }

        [Fact]
        public void CpuSmokeOrBoundaryRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (Mat image = CreateBgrImage())
                using (HfsSegment segment = HfsCv2.CreateHfsSegment(32, 32))
                using (Mat drawn = segment.PerformSegmentCpu(image))
                using (Mat labels = segment.PerformSegmentCpu(image, draw: false))
                {
                    Assert.False(drawn.Empty);
                    Assert.Equal(image.Rows, drawn.Rows);
                    Assert.Equal(image.Cols, drawn.Cols);
                    Assert.False(labels.Empty);
                    Assert.Equal(image.Rows, labels.Rows);
                    Assert.Equal(image.Cols, labels.Cols);
                }
            }
            catch (OpenCvException ex) when (IsHfsModuleMissing(ex))
            {
                Assert.True(IsHfsModuleMissing(ex), ex.Message);
            }
        }

        private static HfsSegment? TryCreateSegment()
        {
            try
            {
                return HfsSegment.Create(32, 32);
            }
            catch (OpenCvException ex) when (IsHfsModuleMissing(ex))
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
        }

        private static Mat CreateBgrImage()
        {
            var mat = new Mat(32, 32, MatType.CV_8UC3);
            var values = new byte[32 * 32 * 3];
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    int offset = (y * 32 + x) * 3;
                    bool topLeft = x < 16 && y < 16;
                    bool topRight = x >= 16 && y < 16;
                    bool bottomLeft = x < 16 && y >= 16;
                    values[offset] = (byte)(topLeft ? 220 : bottomLeft ? 50 : 90);
                    values[offset + 1] = (byte)(topRight ? 220 : bottomLeft ? 180 : 70);
                    values[offset + 2] = (byte)(bottomLeft ? 220 : topRight ? 50 : 80);
                }
            }

            mat.CopyFrom(values);
            return mat;
        }

        private static bool IsHfsModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("hfs", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("Hfs", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }

    }
}
