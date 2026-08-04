using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.StructuredLight;

namespace JYPPX.OpenCvSharp.Tests.StructuredLight
{
    public sealed class StructuredLightTests
    {
        [Fact]
        public void GrayCodeParametersExposeDefaultsAndValidateRanges()
        {
            GrayCodePatternParams parameters = GrayCodePatternParams.Default;
            var same = new GrayCodePatternParams(1024, 768);
            var different = new GrayCodePatternParams(1024, 769);

            Assert.Equal(1024, parameters.Width);
            Assert.Equal(768, parameters.Height);
            Assert.Equal(same, parameters);
            Assert.True(parameters == same);
            Assert.False(parameters != same);
            Assert.True(parameters != different);
            Assert.Equal(parameters.GetHashCode(), same.GetHashCode());
            Assert.Equal("{Width=1024,Height=768}", parameters.ToString());
            Assert.Throws<ArgumentOutOfRangeException>(() => new GrayCodePatternParams(0, 4).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new GrayCodePatternParams(4, 0).Validate());
        }

        [Fact]
        public void GrayCodeParametersEqualityAndHashCodeAreStable()
        {
            var first = new GrayCodePatternParams(640, 480);
            var second = new GrayCodePatternParams(640, 480);
            var differentWidth = new GrayCodePatternParams(641, 480);
            var differentHeight = new GrayCodePatternParams(640, 481);

            Assert.True(first == second);
            Assert.False(first != second);
            Assert.True(first != differentWidth);
            Assert.True(first != differentHeight);
            Assert.True(first.Equals((object)second));
            Assert.False(first.Equals("not-parameters"));
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public void GrayCodeParametersHaveSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(8, Marshal.SizeOf<GrayCodePatternParams>());
            Assert.Equal(0, FieldOffset<GrayCodePatternParams>("<Width>k__BackingField"));
            Assert.Equal(4, FieldOffset<GrayCodePatternParams>("<Height>k__BackingField"));
        }

        [Fact]
        public void SinusoidalParametersExposeDefaultsAndValidateRanges()
        {
            SinusoidalPatternParams parameters = SinusoidalPatternParams.Default();

            Assert.Equal(800, parameters.Width);
            Assert.Equal(600, parameters.Height);
            Assert.Equal(20, parameters.NbrOfPeriods);
            Assert.Equal((float)(2.0 * Math.PI / 3.0), parameters.ShiftValue, 4);
            Assert.Equal(SinusoidalPatternMethod.Ftp, parameters.Method);
            Assert.False(parameters.Horizontal);
            Assert.False(parameters.SetMarkers);
            Assert.Empty(parameters.MarkersLocation);

            Assert.Throws<ArgumentOutOfRangeException>(() => new SinusoidalPatternParams { Width = 0 }.Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new SinusoidalPatternParams { Height = 0 }.Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new SinusoidalPatternParams { NbrOfPeriods = 0 }.Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new SinusoidalPatternParams { ShiftValue = float.NaN }.Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new SinusoidalPatternParams { Method = (SinusoidalPatternMethod)99 }.Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new SinusoidalPatternParams { NbrOfPixelsBetweenMarkers = -1 }.Validate());
            Assert.Throws<ArgumentNullException>(() => new SinusoidalPatternParams { MarkersLocation = null! }.Validate());
        }

        [Fact]
        public void SinusoidalParametersToStringFormatsFloatingValuesInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                var parameters = new SinusoidalPatternParams
                {
                    Width = 24,
                    Height = 16,
                    NbrOfPeriods = 4,
                    ShiftValue = 1.5F,
                    Method = SinusoidalPatternMethod.Psp,
                    NbrOfPixelsBetweenMarkers = 9,
                    Horizontal = true,
                    SetMarkers = true,
                    MarkersLocation = new[] { new Point2f(1.0F, 2.0F), new Point2f(3.0F, 4.0F) }
                };

                string formatted = parameters.ToString();

                Assert.Contains("ShiftValue=1.5", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("ShiftValue=1,5", formatted, StringComparison.Ordinal);
                Assert.Contains("MarkersLocation=2", formatted, StringComparison.Ordinal);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void SinusoidalParametersCloneMarkerLocations()
        {
            var markers = new[] { new Point2f(1.0F, 2.0F), new Point2f(3.0F, 4.0F) };
            var parameters = new SinusoidalPatternParams
            {
                Width = 24,
                Height = 16,
                NbrOfPeriods = 4,
                ShiftValue = 1.5F,
                Method = SinusoidalPatternMethod.Psp,
                NbrOfPixelsBetweenMarkers = 9,
                Horizontal = true,
                SetMarkers = true,
                MarkersLocation = markers
            };

            markers[0] = new Point2f(9.0F, 9.0F);
            Point2f[] firstSnapshot = parameters.MarkersLocation;
            firstSnapshot[1] = new Point2f(8.0F, 8.0F);

            SinusoidalPatternParams copy = new SinusoidalPatternParams(parameters);
            SinusoidalPatternParams clone = parameters.Clone();
            Assert.NotSame(parameters, copy);
            Assert.NotSame(parameters, clone);
            Assert.NotSame(copy, clone);
            Point2f[] copySnapshot = copy.MarkersLocation;
            Point2f[] cloneSnapshot = clone.MarkersLocation;
            copySnapshot[0] = new Point2f(7.0F, 7.0F);
            cloneSnapshot[1] = new Point2f(6.0F, 6.0F);

            Assert.Equal(24, copy.Width);
            Assert.Equal(16, copy.Height);
            Assert.Equal(4, copy.NbrOfPeriods);
            Assert.Equal(1.5F, copy.ShiftValue, 4);
            Assert.Equal(SinusoidalPatternMethod.Psp, copy.Method);
            Assert.Equal(9, copy.NbrOfPixelsBetweenMarkers);
            Assert.True(copy.Horizontal);
            Assert.True(copy.SetMarkers);
            Assert.Equal(new Point2f(1.0F, 2.0F), parameters.MarkersLocation[0]);
            Assert.Equal(new Point2f(3.0F, 4.0F), parameters.MarkersLocation[1]);
            Assert.Equal(new Point2f(1.0F, 2.0F), copy.MarkersLocation[0]);
            Assert.Equal(new Point2f(3.0F, 4.0F), copy.MarkersLocation[1]);
            Assert.Equal(new Point2f(1.0F, 2.0F), clone.MarkersLocation[0]);
            Assert.Equal(new Point2f(3.0F, 4.0F), clone.MarkersLocation[1]);
            Assert.Throws<ArgumentNullException>(() => new SinusoidalPatternParams(null!));
        }

        [Fact]
        public void GrayCodeValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (GrayCodePattern? pattern = TryCreateGrayCode())
            {
                if (pattern == null)
                {
                    return;
                }

                using (Mat image = new Mat())
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => pattern.SetWhiteThreshold(-1));
                    Assert.Throws<ArgumentOutOfRangeException>(() => pattern.SetBlackThreshold(-1));
                    Assert.Throws<ArgumentNullException>(() => pattern.GetImagesForShadowMasks(null!, image));
                    Assert.Throws<ArgumentNullException>(() => pattern.GetImagesForShadowMasks(image, null!));
                    Assert.Throws<ArgumentNullException>(() => pattern.GetProjPixel(null!, 0, 0, out _));
                    Assert.Throws<ArgumentNullException>(() => pattern.GetProjPixel(new Mat[] { null! }, 0, 0, out _));
                    Assert.Throws<ArgumentException>(() => pattern.GetProjPixel(Array.Empty<Mat>(), 0, 0, out _));
                    Assert.Throws<ArgumentException>(() => pattern.GetProjPixel(new[] { image }, 0, 0, out _));

                    pattern.Dispose();
                    Assert.True(pattern.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => pattern.Generate());
                    Assert.Throws<ObjectDisposedException>(() => pattern.NumberOfPatternImages);
                    Assert.Throws<ObjectDisposedException>(() => pattern.SetWhiteThreshold(0));
                    Assert.Throws<ObjectDisposedException>(() => pattern.SetBlackThreshold(0));
                    Assert.Throws<ObjectDisposedException>(() => pattern.GetImagesForShadowMasks(image, image));
                    Assert.Throws<ObjectDisposedException>(() => pattern.GetImagesForShadowMasks(out _, out _));
                    Assert.Throws<ObjectDisposedException>(() => pattern.GetProjPixel(new[] { image }, 0, 0, out _));
                }
            }
        }

        [Fact]
        public void SinusoidalValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (SinusoidalPattern? pattern = TryCreateSinusoidal())
            {
                if (pattern == null)
                {
                    return;
                }

                using (Mat image = new Mat())
                using (Mat output = new Mat())
                {
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputePhaseMap(null!, output, image));
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputePhaseMap(new Mat[] { null! }, output, image));
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputePhaseMap(new[] { image }, null!, image));
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputePhaseMap(new[] { image }, output, shadowMask: null!));
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputePhaseMap(null!, image));
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputePhaseMap(new Mat[] { null! }, image));
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputePhaseMap(new[] { image }, null!));
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputeDataModulationTerm(null!, output, image));
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputeDataModulationTerm(new Mat[] { null! }, output, image));
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputeDataModulationTerm(new[] { image }, null!, image));
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputeDataModulationTerm(new[] { image }, output, null!));
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputeDataModulationTerm(null!, image));
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputeDataModulationTerm(new Mat[] { null! }, image));
                    Assert.Throws<ArgumentNullException>(() => pattern.ComputeDataModulationTerm(new[] { image }, null!));
                    Assert.Throws<ArgumentNullException>(() => pattern.UnwrapPhaseMap(null!, output, new Size(8, 8)));
                    Assert.Throws<ArgumentNullException>(() => pattern.UnwrapPhaseMap(image, null!, new Size(8, 8)));
                    Assert.Throws<ArgumentNullException>(() => pattern.UnwrapPhaseMap(null!, new Size(8, 8)));

                    pattern.Dispose();
                    Assert.True(pattern.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => pattern.Generate());
                    Assert.Throws<ObjectDisposedException>(() => pattern.ComputePhaseMap(new[] { image }, output, image));
                    Assert.Throws<ObjectDisposedException>(() => pattern.ComputePhaseMap(new[] { image }, image));
                    Assert.Throws<ObjectDisposedException>(() => pattern.ComputeDataModulationTerm(new[] { image }, output, image));
                    Assert.Throws<ObjectDisposedException>(() => pattern.ComputeDataModulationTerm(new[] { image }, image));
                    Assert.Throws<ObjectDisposedException>(() => pattern.UnwrapPhaseMap(image, output, new Size(8, 8)));
                    Assert.Throws<ObjectDisposedException>(() => pattern.UnwrapPhaseMap(image, new Size(8, 8)));
                }
            }
        }

        [Fact]
        public void GrayCodeSmokeOrBoundaryRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (GrayCodePattern pattern = GrayCodePattern.Create(16, 8))
                {
                    int expectedCount = pattern.NumberOfPatternImages;
                    using (MatArrayScope scope = new MatArrayScope(pattern.Generate()))
                    {
                        Mat[] images = scope.Mats;
                        Assert.Equal(expectedCount, images.Length);
                        Assert.NotEmpty(images);
                        Assert.All(images, image =>
                        {
                            Assert.False(image.Empty);
                            Assert.Equal(8, image.Rows);
                            Assert.Equal(16, image.Cols);
                        });

                        bool found = pattern.GetProjPixel(images, 0, 0, out Point projectorPixel);
                        Assert.False(found && (projectorPixel.X < 0 || projectorPixel.Y < 0));
                    }

                    pattern.GetImagesForShadowMasks(out Mat black, out Mat white);
                    using (black)
                    using (white)
                    {
                        Assert.False(black.Empty);
                        Assert.False(white.Empty);
                        Assert.Equal(8, black.Rows);
                        Assert.Equal(16, white.Cols);
                    }
                }
            }
            catch (OpenCvException ex) when (IsStructuredLightModuleMissing(ex))
            {
                Assert.True(IsStructuredLightModuleMissing(ex), ex.Message);
            }
        }

        [Fact]
        public void SinusoidalSmokeOrBoundaryRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (SinusoidalPattern pattern = SinusoidalPattern.Create(new SinusoidalPatternParams
                {
                    Width = 24,
                    Height = 16,
                    NbrOfPeriods = 4,
                    Method = SinusoidalPatternMethod.Psp
                }))
                using (MatArrayScope scope = new MatArrayScope(pattern.Generate()))
                {
                    Mat[] images = scope.Mats;
                    Assert.NotEmpty(images);
                    Assert.All(images, image =>
                    {
                        Assert.False(image.Empty);
                        Assert.Equal(16, image.Rows);
                        Assert.Equal(24, image.Cols);
                    });
                }
            }
            catch (OpenCvException ex) when (IsStructuredLightModuleMissing(ex))
            {
                Assert.True(IsStructuredLightModuleMissing(ex), ex.Message);
            }
        }

        private static GrayCodePattern? TryCreateGrayCode()
        {
            try
            {
                return GrayCodePattern.Create(8, 8);
            }
            catch (OpenCvException ex) when (IsStructuredLightModuleMissing(ex))
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

        private static SinusoidalPattern? TryCreateSinusoidal()
        {
            try
            {
                return SinusoidalPattern.Create(8, 8, 2, SinusoidalPatternMethod.Psp);
            }
            catch (OpenCvException ex) when (IsStructuredLightModuleMissing(ex))
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

        private static bool IsStructuredLightModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("structured_light", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("StructuredLight", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }

        private sealed class MatArrayScope : IDisposable
        {
            private readonly Mat[] mats;

            public MatArrayScope(Mat[] mats)
            {
                this.mats = mats;
            }

            public Mat[] Mats
            {
                get { return mats; }
            }

            public void Dispose()
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i].Dispose();
                }
            }
        }
    }
}
