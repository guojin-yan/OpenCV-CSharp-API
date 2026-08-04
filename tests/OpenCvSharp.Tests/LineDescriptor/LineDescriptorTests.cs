using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;
using JYPPX.OpenCvSharp.LineDescriptor;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Tests.LineDescriptor
{
    public sealed class LineDescriptorTests
    {
        [Fact]
        public void KeyLineValueObjectExposesLineGeometry()
        {
            var keyLine = new KeyLine(
                angle: 0.5F,
                classId: 7,
                octave: 2,
                pt: new Point2f(4.0F, 5.0F),
                response: 0.75F,
                size: 32.0F,
                startPoint: new Point2f(1.0F, 2.0F),
                endPoint: new Point2f(8.0F, 9.0F),
                startPointInOctave: new Point2f(0.5F, 1.0F),
                endPointInOctave: new Point2f(4.0F, 4.5F),
                lineLength: 12.0F,
                numOfPixels: 13);

            Assert.Equal(0.5F, keyLine.Angle);
            Assert.Equal(7, keyLine.ClassId);
            Assert.Equal(2, keyLine.Octave);
            Assert.Equal(new Point2f(4.0F, 5.0F), keyLine.Pt);
            Assert.Equal(0.75F, keyLine.Response);
            Assert.Equal(32.0F, keyLine.Size);
            Assert.Equal(new Point2f(1.0F, 2.0F), keyLine.GetStartPoint());
            Assert.Equal(new Point2f(8.0F, 9.0F), keyLine.GetEndPoint());
            Assert.Equal(new Point2f(0.5F, 1.0F), keyLine.GetStartPointInOctave());
            Assert.Equal(new Point2f(4.0F, 4.5F), keyLine.GetEndPointInOctave());
            Assert.Equal(12.0F, keyLine.LineLength);
            Assert.Equal(13, keyLine.NumOfPixels);
            Assert.Contains("StartPoint", keyLine.ToString());
        }

        [Fact]
        public void KeyLineToStringUsesInvariantCulture()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

                var keyLine = new KeyLine(
                    angle: 0.5F,
                    classId: 7,
                    octave: 2,
                    pt: new Point2f(4.5F, 5.25F),
                    response: 0.75F,
                    size: 32.5F,
                    startPoint: new Point2f(1.5F, 2.25F),
                    endPoint: new Point2f(8.5F, 9.25F),
                    startPointInOctave: new Point2f(0.5F, 1.25F),
                    endPointInOctave: new Point2f(4.25F, 4.5F),
                    lineLength: 12.25F,
                    numOfPixels: 13);

                Assert.Equal("{Pt={X=4.5,Y=5.25},StartPoint={X=1.5,Y=2.25},EndPoint={X=8.5,Y=9.25},Angle=0.5,LineLength=12.25}", keyLine.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void KeyLineEqualityAndFlagsAreStable()
        {
            var first = new KeyLine(0.0F, 1, 0, new Point2f(1.0F, 1.0F), 0.5F, 10.0F, new Point2f(0.0F, 0.0F), new Point2f(2.0F, 2.0F), new Point2f(0.0F, 0.0F), new Point2f(2.0F, 2.0F), 3.0F, 4);
            var second = new KeyLine(0.0F, 1, 0, new Point2f(1.0F, 1.0F), 0.5F, 10.0F, new Point2f(0.0F, 0.0F), new Point2f(2.0F, 2.0F), new Point2f(0.0F, 0.0F), new Point2f(2.0F, 2.0F), 3.0F, 4);
            var third = new KeyLine(0.2F, 1, 0, new Point2f(1.0F, 1.0F), 0.5F, 10.0F, new Point2f(0.0F, 0.0F), new Point2f(2.0F, 2.0F), new Point2f(0.0F, 0.0F), new Point2f(2.0F, 2.0F), 3.0F, 4);

            Assert.True(first == second);
            Assert.False(first != second);
            Assert.True(first != third);
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
            Assert.Equal(0, (int)DrawLinesMatchesFlags.Default);
            Assert.Equal(1, (int)DrawLinesMatchesFlags.DrawOverOutImg);
            Assert.Equal(2, (int)DrawLinesMatchesFlags.NotDrawSingleLines);
        }

        [Fact]
        public void BinaryDescriptorParametersExposeDefaultsAndValidateRanges()
        {
            BinaryDescriptorParameters parameters = BinaryDescriptorParameters.Default;

            Assert.Equal(1, parameters.NumOfOctaves);
            Assert.Equal(7, parameters.WidthOfBand);
            Assert.Equal(2, parameters.ReductionRatio);
            Assert.Equal(5, parameters.KSize);
            Assert.Equal(parameters, new BinaryDescriptorParameters(1, 7, 2, 5));
            Assert.Equal("{NumOfOctaves=1,WidthOfBand=7,ReductionRatio=2,KSize=5}", parameters.ToString());
            Assert.Throws<ArgumentOutOfRangeException>(() => new BinaryDescriptorParameters(0, 7, 2, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new BinaryDescriptorParameters(1, 0, 2, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new BinaryDescriptorParameters(1, 7, 0, 5).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new BinaryDescriptorParameters(1, 7, 2, 0).Validate());
        }

        [Fact]
        public void BinaryDescriptorParametersEqualityAndHashCodeAreStable()
        {
            var first = new BinaryDescriptorParameters(2, 9, 3, 7);
            var second = new BinaryDescriptorParameters(2, 9, 3, 7);
            var differentOctaves = new BinaryDescriptorParameters(3, 9, 3, 7);
            var differentBand = new BinaryDescriptorParameters(2, 11, 3, 7);

            Assert.True(first == second);
            Assert.False(first != second);
            Assert.True(first != differentOctaves);
            Assert.True(first != differentBand);
            Assert.True(first.Equals((object)second));
            Assert.False(first.Equals("not-parameters"));
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public void LineDescriptorValueObjectsHaveSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(16, Marshal.SizeOf<BinaryDescriptorParameters>());
            Assert.Equal(68, Marshal.SizeOf<KeyLine>());

            Assert.Equal(0, FieldOffset<BinaryDescriptorParameters>("<NumOfOctaves>k__BackingField"));
            Assert.Equal(4, FieldOffset<BinaryDescriptorParameters>("<WidthOfBand>k__BackingField"));
            Assert.Equal(8, FieldOffset<BinaryDescriptorParameters>("<ReductionRatio>k__BackingField"));
            Assert.Equal(12, FieldOffset<BinaryDescriptorParameters>("<KSize>k__BackingField"));

            Assert.Equal(0, FieldOffset<KeyLine>("<Angle>k__BackingField"));
            Assert.Equal(4, FieldOffset<KeyLine>("<ClassId>k__BackingField"));
            Assert.Equal(8, FieldOffset<KeyLine>("<Octave>k__BackingField"));
            Assert.Equal(12, FieldOffset<KeyLine>("<Pt>k__BackingField"));
            Assert.Equal(20, FieldOffset<KeyLine>("<Response>k__BackingField"));
            Assert.Equal(24, FieldOffset<KeyLine>("<Size>k__BackingField"));
            Assert.Equal(28, FieldOffset<KeyLine>("<StartPoint>k__BackingField"));
            Assert.Equal(36, FieldOffset<KeyLine>("<EndPoint>k__BackingField"));
            Assert.Equal(44, FieldOffset<KeyLine>("<StartPointInOctave>k__BackingField"));
            Assert.Equal(52, FieldOffset<KeyLine>("<EndPointInOctave>k__BackingField"));
            Assert.Equal(60, FieldOffset<KeyLine>("<LineLength>k__BackingField"));
            Assert.Equal(64, FieldOffset<KeyLine>("<NumOfPixels>k__BackingField"));
        }

        [Fact]
        public void DrawingHelpersValidateManagedArguments()
        {
            using (BinaryDescriptor? nativeBoundary = TryCreateDescriptor())
            {
                if (nativeBoundary == null)
                {
                    return;
                }
            }

            using (Mat image = CreateLineImage())
            using (Mat output = new Mat())
            {
                var keylines = new[] { CreateManualKeyLine() };
                var matches = new[] { new DMatch(0, 0, 0.0F) };

                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawKeylines(null!, keylines, output, new Scalar(255)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawKeylines(image, null!, output, new Scalar(255)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawKeylines(image, keylines, null!, new Scalar(255)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawKeylines(null!, keylines, new Scalar(255)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawKeylines(image, null!, new Scalar(255)));
                Assert.Throws<ArgumentOutOfRangeException>(() => LineDescriptorCv2.DrawKeylines(image, keylines, output, new Scalar(255), (DrawLinesMatchesFlags)4));
                Assert.Throws<ArgumentOutOfRangeException>(() => LineDescriptorCv2.DrawKeylines(image, keylines, new Scalar(255), (DrawLinesMatchesFlags)4));
                Assert.Throws<ArgumentOutOfRangeException>(() => LineDescriptorCv2.DrawKeylines(image, keylines, output, new Scalar(255), DrawLinesMatchesFlags.DrawOverOutImg | DrawLinesMatchesFlags.NotDrawSingleLines));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawLineMatches(null!, keylines, image, keylines, matches, output, new Scalar(255), new Scalar(0)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawLineMatches(image, null!, image, keylines, matches, output, new Scalar(255), new Scalar(0)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawLineMatches(image, keylines, null!, keylines, matches, output, new Scalar(255), new Scalar(0)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawLineMatches(image, keylines, image, null!, matches, output, new Scalar(255), new Scalar(0)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawLineMatches(image, keylines, image, keylines, null!, output, new Scalar(255), new Scalar(0)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawLineMatches(image, keylines, image, keylines, matches, null!, new Scalar(255), new Scalar(0)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawLineMatches(null!, keylines, image, keylines, matches, new Scalar(255), new Scalar(0)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawLineMatches(image, null!, image, keylines, matches, new Scalar(255), new Scalar(0)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawLineMatches(image, keylines, null!, keylines, matches, new Scalar(255), new Scalar(0)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawLineMatches(image, keylines, image, null!, matches, new Scalar(255), new Scalar(0)));
                Assert.Throws<ArgumentNullException>(() => LineDescriptorCv2.DrawLineMatches(image, keylines, image, keylines, null!, new Scalar(255), new Scalar(0)));
                Assert.Throws<ArgumentOutOfRangeException>(() => LineDescriptorCv2.DrawLineMatches(image, keylines, image, keylines, matches, output, new Scalar(255), new Scalar(0), (DrawLinesMatchesFlags)4));
                Assert.Throws<ArgumentOutOfRangeException>(() => LineDescriptorCv2.DrawLineMatches(image, keylines, image, keylines, matches, new Scalar(255), new Scalar(0), (DrawLinesMatchesFlags)4));
                Assert.Throws<ArgumentOutOfRangeException>(() => LineDescriptorCv2.DrawLineMatches(image, keylines, image, keylines, matches, DrawLinesMatchesFlags.DrawOverOutImg | DrawLinesMatchesFlags.NotDrawSingleLines));
            }
        }

        [Fact]
        public void BinaryDescriptorValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (BinaryDescriptor? descriptor = TryCreateDescriptor())
            {
                if (descriptor == null)
                {
                    return;
                }

                using (Mat image = CreateLineImage())
                using (Mat descriptors = new Mat())
                using (Mat wrongMaskType = new Mat(image.Rows, image.Cols, MatType.CV_32FC1, new Scalar(1.0)))
                using (Mat wrongMaskSize = new Mat(image.Rows - 1, image.Cols, MatType.CV_8UC1, new Scalar(255)))
                {
                    Assert.Throws<ArgumentNullException>(() => descriptor.Detect(null!));
                    Assert.Throws<ArgumentNullException>(() => descriptor.Compute(null!, Array.Empty<KeyLine>(), descriptors));
                    Assert.Throws<ArgumentNullException>(() => descriptor.Compute(image, null!, descriptors));
                    Assert.Throws<ArgumentNullException>(() => descriptor.Compute(image, Array.Empty<KeyLine>(), null!));
                    Assert.Throws<ArgumentException>(() => descriptor.Detect(image, wrongMaskType));
                    Assert.Throws<ArgumentException>(() => descriptor.Detect(image, wrongMaskSize));
                    Assert.Throws<ArgumentException>(() => descriptor.DetectAndCompute(image, wrongMaskType, out _, descriptors));
                    Assert.Throws<ArgumentException>(() => descriptor.DetectAndCompute(image, wrongMaskSize, out _, descriptors));
                    Assert.Throws<ArgumentException>(() => descriptor.DetectAndCompute(image, wrongMaskType, Array.Empty<KeyLine>(), descriptors));
                    Assert.Throws<ArgumentException>(() => descriptor.DetectAndCompute(image, wrongMaskSize, Array.Empty<KeyLine>(), descriptors));
                    Assert.Equal("{NumOfOctaves=1,WidthOfBand=7,ReductionRatio=2}", descriptor.ToString());

                    descriptor.Dispose();
                    Assert.True(descriptor.IsDisposed);
                    Assert.Equal("{Disposed=True}", descriptor.ToString());
                    Assert.Throws<ObjectDisposedException>(() => descriptor.Empty);
                    Assert.Throws<ObjectDisposedException>(() => descriptor.Detect(image));
                    Assert.Throws<ObjectDisposedException>(() =>
                    {
                        KeyLine[] keylines = Array.Empty<KeyLine>();
                        using (descriptor.Compute(image, ref keylines))
                        {
                        }
                    });
                    Assert.Throws<ObjectDisposedException>(() =>
                    {
                        using (descriptor.DetectAndCompute(image, null, out _))
                        {
                        }
                    });
                }
            }
        }

        [Fact]
        public void BinaryDescriptorMatcherValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (BinaryDescriptorMatcher? matcher = TryCreateMatcher())
            {
                if (matcher == null)
                {
                    return;
                }

                using (Mat descriptors = new Mat(1, 32, MatType.CV_8UC1, new Scalar(0)))
                {
                    Assert.Throws<ArgumentNullException>(() => matcher.Match(null!, descriptors));
                    Assert.Throws<ArgumentNullException>(() => matcher.Match(descriptors, null!));
                    Assert.Throws<ArgumentOutOfRangeException>(() => matcher.KnnMatch(descriptors, descriptors, 0));
                    Assert.Equal("{Empty=" + matcher.Empty + "}", matcher.ToString());
                }

                matcher.Dispose();
                Assert.True(matcher.IsDisposed);
                Assert.Equal("{Disposed=True}", matcher.ToString());
                Assert.Throws<ObjectDisposedException>(() => matcher.Empty);
                Assert.Throws<ObjectDisposedException>(() => matcher.Clear());
            }
        }

        [Fact]
        public void BinaryDescriptorSmokeOrBoundaryRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (BinaryDescriptor descriptor = BinaryDescriptor.Create())
                using (BinaryDescriptorMatcher matcher = BinaryDescriptorMatcher.Create())
                using (Mat image = CreateLineImage())
                using (Mat descriptors = new Mat())
                {
                    descriptor.NumOfOctaves = 1;
                    descriptor.WidthOfBand = 7;
                    descriptor.ReductionRatio = 2;

                    KeyLine[] detected = descriptor.Detect(image);
                    Mat drawn = LineDescriptorCv2.DrawKeylines(image, detected, new Scalar(0, 255, 0));
                    using (drawn)
                    {
                        Assert.False(drawn.Empty);
                    }

                    KeyLine[] output = descriptor.DetectAndCompute(image, null, detected, descriptors, useProvidedKeylines: detected.Length > 0);
                    Assert.True(output.Length >= 0);
                    Assert.False(descriptor.IsDisposed);
                    Assert.False(matcher.IsDisposed);
                    Assert.True(descriptor.DescriptorSize >= 0);
                    Assert.True(descriptor.DescriptorType >= 0);

                    if (!descriptors.Empty && descriptors.Rows > 0)
                    {
                        DMatch[] matches = matcher.Match(descriptors, descriptors);
                        DMatch[][] grouped = matcher.KnnMatch(descriptors, descriptors, 1);
                        Assert.True(matches.Length >= 0);
                        Assert.True(grouped.Length >= 0);

                        if (output.Length > 0 && matches.Length > 0)
                        {
                            using (Mat matched = LineDescriptorCv2.DrawLineMatches(image, output, image, output, matches))
                            {
                                Assert.False(matched.Empty);
                            }
                        }
                    }
                }
            }
            catch (OpenCvException ex) when (IsLineDescriptorModuleMissing(ex))
            {
                Assert.True(IsLineDescriptorModuleMissing(ex), ex.Message);
            }
        }

        private static Mat CreateLineImage()
        {
            var image = new Mat(64, 64, MatType.CV_8UC1, new Scalar(0));
            ImgProcCv2.Line(image, new Point(8, 12), new Point(56, 12), new Scalar(255), 2);
            ImgProcCv2.Line(image, new Point(10, 50), new Point(54, 18), new Scalar(255), 2);
            return image;
        }

        private static KeyLine CreateManualKeyLine()
        {
            return new KeyLine(
                0.0F,
                0,
                0,
                new Point2f(32.0F, 12.0F),
                1.0F,
                48.0F,
                new Point2f(8.0F, 12.0F),
                new Point2f(56.0F, 12.0F),
                new Point2f(8.0F, 12.0F),
                new Point2f(56.0F, 12.0F),
                48.0F,
                49);
        }

        private static BinaryDescriptor? TryCreateDescriptor()
        {
            try
            {
                return BinaryDescriptor.Create();
            }
            catch (OpenCvException ex) when (IsLineDescriptorModuleMissing(ex))
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

        private static BinaryDescriptorMatcher? TryCreateMatcher()
        {
            try
            {
                return BinaryDescriptorMatcher.Create();
            }
            catch (OpenCvException ex) when (IsLineDescriptorModuleMissing(ex))
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

        private static bool IsLineDescriptorModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("line_descriptor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("BinaryDescriptor", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }
    }
}
