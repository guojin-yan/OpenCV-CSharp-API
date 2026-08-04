using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.PhaseUnwrapping;

namespace JYPPX.OpenCvSharp.Tests.PhaseUnwrapping
{
    public sealed class PhaseUnwrappingTests
    {
        [Fact]
        public void HistogramParametersExposeDefaultsAndValidateRanges()
        {
            HistogramPhaseUnwrappingParams parameters = HistogramPhaseUnwrappingParams.Default;
            var same = new HistogramPhaseUnwrappingParams(800, 600, (float)(3.0 * Math.PI * Math.PI), 10, 5);
            var different = new HistogramPhaseUnwrappingParams(801, 600, (float)(3.0 * Math.PI * Math.PI), 10, 5);

            Assert.Equal(800, parameters.Width);
            Assert.Equal(600, parameters.Height);
            Assert.Equal((float)(3.0 * Math.PI * Math.PI), parameters.HistThresh, 4);
            Assert.Equal(10, parameters.NbrOfSmallBins);
            Assert.Equal(5, parameters.NbrOfLargeBins);
            Assert.Equal(same, parameters);
            Assert.True(parameters == same);
            Assert.False(parameters != same);
            Assert.True(parameters != different);
            Assert.Equal(parameters.GetHashCode(), same.GetHashCode());
            Assert.Contains("HistThresh", parameters.ToString());

            Assert.Throws<ArgumentOutOfRangeException>(() => new HistogramPhaseUnwrappingParams(0, 8, 1.0F, 10, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HistogramPhaseUnwrappingParams(8, 0, 1.0F, 10, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HistogramPhaseUnwrappingParams(8, 8, 0.0F, 10, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HistogramPhaseUnwrappingParams(8, 8, float.NaN, 10, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HistogramPhaseUnwrappingParams(8, 8, 1.0F, 0, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new HistogramPhaseUnwrappingParams(8, 8, 1.0F, 10, 0).Validate());
        }

        [Fact]
        public void HistogramParametersEqualityAndHashCodeAreStable()
        {
            var first = new HistogramPhaseUnwrappingParams(32, 24, 1.25F, 10, 5);
            var second = new HistogramPhaseUnwrappingParams(32, 24, 1.25F, 10, 5);
            var differentHeight = new HistogramPhaseUnwrappingParams(32, 25, 1.25F, 10, 5);
            var differentThreshold = new HistogramPhaseUnwrappingParams(32, 24, 1.5F, 10, 5);
            var differentSmallBins = new HistogramPhaseUnwrappingParams(32, 24, 1.25F, 11, 5);
            var differentLargeBins = new HistogramPhaseUnwrappingParams(32, 24, 1.25F, 10, 6);

            Assert.True(first == second);
            Assert.False(first != second);
            Assert.True(first != differentHeight);
            Assert.True(first != differentThreshold);
            Assert.True(first != differentSmallBins);
            Assert.True(first != differentLargeBins);
            Assert.True(first.Equals((object)second));
            Assert.False(first.Equals("not-parameters"));
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public void HistogramParametersFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                Assert.Equal(
                    "{Width=8,Height=9,HistThresh=1.25,NbrOfSmallBins=10,NbrOfLargeBins=5}",
                    new HistogramPhaseUnwrappingParams(8, 9, 1.25F, 10, 5).ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void HistogramParametersHaveSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(20, Marshal.SizeOf<HistogramPhaseUnwrappingParams>());
            Assert.Equal(0, FieldOffset<HistogramPhaseUnwrappingParams>("<Width>k__BackingField"));
            Assert.Equal(4, FieldOffset<HistogramPhaseUnwrappingParams>("<Height>k__BackingField"));
            Assert.Equal(8, FieldOffset<HistogramPhaseUnwrappingParams>("<HistThresh>k__BackingField"));
            Assert.Equal(12, FieldOffset<HistogramPhaseUnwrappingParams>("<NbrOfSmallBins>k__BackingField"));
            Assert.Equal(16, FieldOffset<HistogramPhaseUnwrappingParams>("<NbrOfLargeBins>k__BackingField"));
        }

        [Fact]
        public void HistogramValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (HistogramPhaseUnwrapping? unwrapper = TryCreateHistogram())
            {
                if (unwrapper == null)
                {
                    return;
                }

                using (Mat phase = CreateWrappedPhaseMap())
                using (Mat output = new Mat())
                using (Mat wrongPhaseType = new Mat(8, 8, MatType.CV_8UC1, new Scalar(0)))
                using (Mat wrongMaskType = new Mat(8, 8, MatType.CV_32FC1, new Scalar(1.0)))
                {
                    Assert.Throws<ArgumentNullException>(() => unwrapper.UnwrapPhaseMap(null!, output));
                    Assert.Throws<ArgumentNullException>(() => unwrapper.UnwrapPhaseMap(phase, unwrappedPhaseMap: null!));
                    Assert.Throws<ArgumentNullException>(() => unwrapper.UnwrapPhaseMap(null!));
                    Assert.Throws<ArgumentNullException>(() => unwrapper.GetInverseReliabilityMap(null!));
                    Assert.Throws<ArgumentException>(() => unwrapper.UnwrapPhaseMap(wrongPhaseType, output));
                    Assert.Throws<ArgumentException>(() => unwrapper.UnwrapPhaseMap(phase, output, wrongMaskType));
                    Assert.Throws<ArgumentException>(() => unwrapper.UnwrapPhaseMap(wrongPhaseType));
                    Assert.Throws<ArgumentException>(() => unwrapper.UnwrapPhaseMap(phase, wrongMaskType));

                    unwrapper.Dispose();
                    Assert.True(unwrapper.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => unwrapper.UnwrapPhaseMap(phase, output));
                    Assert.Throws<ObjectDisposedException>(() => unwrapper.UnwrapPhaseMap(phase));
                    Assert.Throws<ObjectDisposedException>(() => unwrapper.GetInverseReliabilityMap(output));
                    Assert.Throws<ObjectDisposedException>(() => unwrapper.GetInverseReliabilityMap());
                }
            }
        }

        [Fact]
        public void HistogramSmokeOrBoundaryRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (HistogramPhaseUnwrapping unwrapper = HistogramPhaseUnwrapping.Create(8, 8, (float)(3.0 * Math.PI * Math.PI)))
                using (Mat phase = CreateWrappedPhaseMap())
                using (Mat unwrapped = unwrapper.UnwrapPhaseMap(phase))
                using (Mat reliability = unwrapper.GetInverseReliabilityMap())
                {
                    Assert.False(unwrapped.Empty);
                    Assert.Equal(8, unwrapped.Rows);
                    Assert.Equal(8, unwrapped.Cols);
                    Assert.False(reliability.Empty);
                    Assert.Equal(8, reliability.Rows);
                    Assert.Equal(8, reliability.Cols);
                }
            }
            catch (OpenCvException ex) when (IsPhaseUnwrappingModuleMissing(ex))
            {
                Assert.True(IsPhaseUnwrappingModuleMissing(ex), ex.Message);
            }
        }

        private static Mat CreateWrappedPhaseMap()
        {
            var mat = new Mat(8, 8, MatType.CV_32FC1);
            var values = new float[64];
            for (int i = 0; i < values.Length; i++)
            {
                int x = i % 8;
                int y = i / 8;
                values[i] = (float)(Math.Sin(x * 0.35) + Math.Cos(y * 0.25));
            }

            mat.CopyFrom(values);
            return mat;
        }

        private static HistogramPhaseUnwrapping? TryCreateHistogram()
        {
            try
            {
                return HistogramPhaseUnwrapping.Create(8, 8, (float)(3.0 * Math.PI * Math.PI));
            }
            catch (OpenCvException ex) when (IsPhaseUnwrappingModuleMissing(ex))
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

        private static bool IsPhaseUnwrappingModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("phase_unwrapping", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("PhaseUnwrapping", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }

    }
}
