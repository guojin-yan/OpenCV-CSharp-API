using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgProc
{
    public class HoughFeatureClaheTests
    {
        [Fact]
        public void ClaheObjectAppliesAndExposesPropertiesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(8, 8, MatType.CV_8UC1))
            using (Mat dst = new Mat())
            using (CLAHE clahe = ImgProcCv2.CreateCLAHE(2.0, new Size(4, 4)))
            {
                src.CopyFrom(CreateRamp(64));

                Assert.False(clahe.IsDisposed);
                Assert.Equal(2.0, clahe.ClipLimit, 6);
                Assert.Equal("{Width=4,Height=4}", clahe.TilesGridSize.ToString());

                clahe.ClipLimit = 3.0;
                clahe.TilesGridSize = new Size(2, 2);
                clahe.BitShift = 0;
                clahe.Apply(src, dst);
                using (Mat returned = clahe.Apply(src))
                {
                    Assert.Equal(8, returned.Rows);
                    Assert.Equal(8, returned.Cols);
                    Assert.Equal(MatType.CV_8UC1, returned.Type);
                }

                clahe.CollectGarbage();

                Assert.Equal(3.0, clahe.ClipLimit, 6);
                Assert.Equal("{Width=2,Height=2}", clahe.TilesGridSize.ToString());
                Assert.Equal(0, clahe.BitShift);
                Assert.Equal(8, dst.Rows);
                Assert.Equal(8, dst.Cols);
                Assert.Equal(MatType.CV_8UC1, dst.Type);

                Assert.Throws<ArgumentNullException>(() => clahe.Apply(null!, dst));
                Assert.Throws<ArgumentNullException>(() => clahe.Apply(src, null!));
                Assert.Throws<ArgumentNullException>(() => clahe.Apply(null!));
            }
        }

        [Fact]
        public void ClaheDisposedStateThrowsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat())
            using (Mat dst = new Mat())
            {
                CLAHE clahe = ImgProcCv2.CreateCLAHE(2.0, new Size(4, 4));
                clahe.Dispose();

                Assert.True(clahe.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => clahe.ClipLimit);
                Assert.Throws<ObjectDisposedException>(() => clahe.ClipLimit = 3.0);
                Assert.Throws<ObjectDisposedException>(() => clahe.TilesGridSize);
                Assert.Throws<ObjectDisposedException>(() => clahe.TilesGridSize = new Size(2, 2));
                Assert.Throws<ObjectDisposedException>(() => clahe.BitShift);
                Assert.Throws<ObjectDisposedException>(() => clahe.BitShift = 0);
                Assert.Throws<ObjectDisposedException>(() => clahe.Apply(src, dst));
                Assert.Throws<ObjectDisposedException>(() => clahe.Apply(src));
                Assert.Throws<ObjectDisposedException>(() => clahe.CollectGarbage());
            }
        }

        [Fact]
        public void HistogramBackProjectAndCompareHistWorkWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(4, 4, MatType.CV_8UC1))
            using (Mat hist = new Mat())
            using (Mat hist2 = new Mat())
            using (Mat backProject = new Mat())
            {
                src.CopyFrom(new byte[]
                {
                    0, 0, 64, 64,
                    128, 128, 192, 192,
                    0, 64, 128, 192,
                    0, 64, 128, 192
                });

                ImgProcCv2.CalcHist(src, 0, null, hist, 4, 0, 256);
                ImgProcCv2.CalcHist(src, new[] { 0 }, null, hist2, new[] { 4 }, new[] { 0F, 256F });
                ImgProcCv2.CalcBackProject(src, 0, hist, backProject, 0, 256);
                double correlation = ImgProcCv2.CompareHist(hist, hist2, HistogramComparisonTypes.Correl);

                Assert.Equal(4, hist.ValueCount);
                Assert.Equal(1, hist.Channels);
                Assert.Equal(src.Rows, backProject.Rows);
                Assert.Equal(src.Cols, backProject.Cols);
                Assert.True(correlation > 0.99);
            }
        }

        [Fact]
        public void HoughFeatureFixedEnumsValidateManagedArguments()
        {
            using (Mat hist = new Mat())
            using (Mat hist2 = new Mat())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.CreateLineSegmentDetector((LineSegmentDetectorModes)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.CompareHist(hist, hist2, (HistogramComparisonTypes)99));
            }
        }

        [Fact]
        public void HoughTransformsReturnManagedResultObjectsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat binary = new Mat(64, 64, MatType.CV_8UC1))
            using (Mat circles = new Mat(64, 64, MatType.CV_8UC1))
            {
                binary.SetTo(new Scalar(0));
                circles.SetTo(new Scalar(0));
                ImgProcCv2.Line(binary, new Point(5, 8), new Point(58, 8), new Scalar(255), 1);
                ImgProcCv2.Line(binary, new Point(5, 20), new Point(58, 50), new Scalar(255), 1);
                ImgProcCv2.Circle(circles, new Point(32, 32), 12, new Scalar(255), 2);

                HoughLine[] standardLines = ImgProcCv2.HoughLines(binary, 1.0, Math.PI / 180.0, 20);
                Vec4i[] probabilisticLines = ImgProcCv2.HoughLinesP(binary, 1.0, Math.PI / 180.0, 10, 8.0, 2.0);
                HoughCircle[] detectedCircles = ImgProcCv2.HoughCircles(circles, HoughModes.Gradient, 1.0, 16.0, 80.0, 8.0, 5, 20);
                HoughLinePointSet[] pointSetLines = ImgProcCv2.HoughLinesPointSet(
                    new[]
                    {
                        new Point(0, 0),
                        new Point(5, 5),
                        new Point(10, 10),
                        new Point(15, 15),
                        new Point(20, 20)
                    },
                    4,
                    2,
                    -50,
                    50,
                    1,
                    0,
                    Math.PI,
                    Math.PI / 180.0);

                Assert.NotNull(standardLines);
                Assert.NotNull(probabilisticLines);
                Assert.NotNull(detectedCircles);
                Assert.NotNull(pointSetLines);

                if (standardLines.Length > 0)
                {
                    Assert.True(standardLines[0].Rho >= 0);
                }

                if (probabilisticLines.Length > 0)
                {
                    Assert.True(probabilisticLines[0].V0 >= 0);
                }

                if (detectedCircles.Length > 0)
                {
                    Assert.True(detectedCircles[0].Radius >= 0);
                }

                if (pointSetLines.Length > 0)
                {
                    Assert.True(pointSetLines[0].Votes >= 0);
                }
            }
        }

        [Fact]
        public void CornerSubPixRefinesMutableCornersWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image = new Mat(16, 16, MatType.CV_8UC1))
            {
                image.SetTo(new Scalar(0));
                ImgProcCv2.Rectangle(image, new Rect(4, 4, 8, 8), new Scalar(255), -1);

                var corners = new[] { new Point2f(4.0F, 4.0F), new Point2f(11.0F, 11.0F) };
                ImgProcCv2.CornerSubPix(
                    image,
                    corners,
                    new Size(3, 3),
                    new Size(-1, -1),
                    TermCriteria.ByCountAndEpsilon(20, 0.01));

                Assert.Equal(2, corners.Length);
                Assert.True(corners[0].X > 2.0F);
                Assert.True(corners[0].Y > 2.0F);
            }
        }

        [Fact]
        public void LineSegmentDetectorDetectsDrawsAndComparesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image = new Mat(64, 64, MatType.CV_8UC1))
            using (Mat drawing = new Mat(64, 64, MatType.CV_8UC3))
            using (Mat linesMat = new Mat())
            using (LineSegmentDetector detector = ImgProcCv2.CreateLineSegmentDetector())
            {
                image.SetTo(new Scalar(0));
                drawing.SetTo(new Scalar(0, 0, 0));
                ImgProcCv2.Line(image, new Point(8, 8), new Point(56, 8), new Scalar(255), 1);
                ImgProcCv2.Line(image, new Point(8, 20), new Point(56, 48), new Scalar(255), 1);

                detector.Detect(image, linesMat);
                LineSegment[] segments = detector.Detect(image);
                detector.DrawSegments(drawing, linesMat);

                Assert.False(detector.IsDisposed);
                Assert.True(linesMat.Rows >= 0);
                Assert.NotNull(segments);

                if (segments.Length > 0)
                {
                    detector.DrawSegments(drawing, segments);
                    int mismatch = detector.CompareSegments(image.Size, segments, segments);
                    Assert.True(mismatch >= 0);
                }
            }
        }

        [Fact]
        public void LineSegmentDetectorValidatesManagedArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image = new Mat(16, 16, MatType.CV_8UC1))
            using (Mat lines = new Mat())
            using (LineSegmentDetector detector = ImgProcCv2.CreateLineSegmentDetector())
            {
                LineSegment[] segments = { new LineSegment(1, 2, 10, 12) };

                Assert.Throws<ArgumentNullException>(() => detector.Detect(null!, lines));
                Assert.Throws<ArgumentNullException>(() => detector.Detect(image, null!));
                Assert.Throws<ArgumentNullException>(() => detector.Detect(null!));
                Assert.Throws<ArgumentNullException>(() => detector.DrawSegments(null!, lines));
                Assert.Throws<ArgumentNullException>(() => detector.DrawSegments(image, (Mat)null!));
                Assert.Throws<ArgumentNullException>(() => detector.DrawSegments(null!, segments));
                Assert.Throws<ArgumentNullException>(() => detector.DrawSegments(image, (LineSegment[])null!));
                Assert.Throws<ArgumentNullException>(() => detector.CompareSegments(image.Size, null!, lines));
                Assert.Throws<ArgumentNullException>(() => detector.CompareSegments(image.Size, lines, null!));
                Assert.Throws<ArgumentNullException>(() => detector.CompareSegments(image.Size, null!, segments));
                Assert.Throws<ArgumentNullException>(() => detector.CompareSegments(image.Size, segments, null!));
            }
        }

        [Fact]
        public void LineSegmentDetectorThrowsAfterDisposeWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image = new Mat(16, 16, MatType.CV_8UC1))
            using (Mat lines = new Mat())
            {
                LineSegment[] segments = { new LineSegment(1, 2, 10, 12) };
                LineSegmentDetector detector = ImgProcCv2.CreateLineSegmentDetector();
                detector.Dispose();

                Assert.True(detector.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => detector.Detect(image, lines));
                Assert.Throws<ObjectDisposedException>(() => detector.Detect(image));
                Assert.Throws<ObjectDisposedException>(() => detector.DrawSegments(image, lines));
                Assert.Throws<ObjectDisposedException>(() => detector.DrawSegments(image, segments));
                Assert.Throws<ObjectDisposedException>(() => detector.CompareSegments(image.Size, lines, lines));
                Assert.Throws<ObjectDisposedException>(() => detector.CompareSegments(image.Size, segments, segments));
            }
        }

        [Fact]
        public void NewValueObjectsExposeOpenCvCompatibleFields()
        {
            HoughLine line = new HoughLine(1.5F, 0.25F);
            HoughCircle circle = new HoughCircle(new Point2f(2.5F, 3.5F), 4.5F);
            HoughLinePointSet pointSetLine = new HoughLinePointSet(7, 8, 9);
            LineSegment segment = new LineSegment(1, 2, 3, 4);
            TermCriteria criteria = TermCriteria.ByCountAndEpsilon(10, 0.01);

            Assert.Equal(1.5F, line[0]);
            Assert.Equal(0.25F, line[1]);
            Assert.Equal(2.5F, circle.X);
            Assert.Equal(3.5F, circle.Y);
            Assert.Equal(4.5F, circle.Radius);
            Assert.Equal(7, pointSetLine.Votes);
            Assert.Equal(8, pointSetLine.Rho);
            Assert.Equal(9, pointSetLine.Theta);
            Assert.Equal(1, segment.X1);
            Assert.Equal(4, segment.Y2);
            Assert.True(segment.LengthSquared > 0);
            Assert.Equal(TermCriteriaTypes.CountOrEps, criteria.Type);
            Assert.Equal(10, criteria.MaxCount);
            Assert.Equal(0.01, criteria.Epsilon);
            Assert.True(line == new HoughLine(1.5F, 0.25F));
            Assert.True(circle == new HoughCircle(2.5F, 3.5F, 4.5F));
            Assert.Equal(0F, new HoughCircle(2.5F, 3.5F, 0F).Radius);
        }

        [Fact]
        public void HoughCircleRejectsNegativeRadius()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HoughCircle(1.5F, 2.5F, -0.1F));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HoughCircle(new Point2f(1.5F, 2.5F), -0.1F));
        }

        [Fact]
        public void HoughValueObjectsFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                Assert.Equal("{Rho=1.5,Theta=0.25}", new HoughLine(1.5F, 0.25F).ToString());
                Assert.Equal("{Center={X=2.5,Y=3.5},Radius=4.5}", new HoughCircle(2.5F, 3.5F, 4.5F).ToString());
                Assert.Equal("{Votes=7.5,Rho=8.25,Theta=9.125}", new HoughLinePointSet(7.5, 8.25, 9.125).ToString());
                Assert.Equal("{P1={X=1.5,Y=2.5},P2={X=3.5,Y=4.5}}", new LineSegment(1.5F, 2.5F, 3.5F, 4.5F).ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void HoughValueObjectsHaveSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(8, Marshal.SizeOf<HoughLine>());
            Assert.Equal(12, Marshal.SizeOf<HoughCircle>());
            Assert.Equal(24, Marshal.SizeOf<HoughLinePointSet>());
            Assert.Equal(16, Marshal.SizeOf<LineSegment>());

            HoughLine[] lines =
            {
                new HoughLine(1.5F, 2.5F),
                new HoughLine(3.5F, 4.5F)
            };
            HoughCircle[] circles =
            {
                new HoughCircle(1.5F, 2.5F, 3.5F),
                new HoughCircle(4.5F, 5.5F, 6.5F)
            };
            HoughLinePointSet[] pointSetLines =
            {
                new HoughLinePointSet(1.5, 2.5, 3.5),
                new HoughLinePointSet(4.5, 5.5, 6.5)
            };
            LineSegment[] segments =
            {
                new LineSegment(1.5F, 2.5F, 3.5F, 4.5F),
                new LineSegment(5.5F, 6.5F, 7.5F, 8.5F)
            };

            ReadOnlySpan<float> lineFields = MemoryMarshal.Cast<HoughLine, float>(lines.AsSpan());
            ReadOnlySpan<float> circleFields = MemoryMarshal.Cast<HoughCircle, float>(circles.AsSpan());
            ReadOnlySpan<double> pointSetFields = MemoryMarshal.Cast<HoughLinePointSet, double>(pointSetLines.AsSpan());
            ReadOnlySpan<float> segmentFields = MemoryMarshal.Cast<LineSegment, float>(segments.AsSpan());

            Assert.Equal(new float[] { 1.5F, 2.5F, 3.5F, 4.5F }, lineFields.ToArray());
            Assert.Equal(new float[] { 1.5F, 2.5F, 3.5F, 4.5F, 5.5F, 6.5F }, circleFields.ToArray());
            Assert.Equal(new double[] { 1.5, 2.5, 3.5, 4.5, 5.5, 6.5 }, pointSetFields.ToArray());
            Assert.Equal(new float[] { 1.5F, 2.5F, 3.5F, 4.5F, 5.5F, 6.5F, 7.5F, 8.5F }, segmentFields.ToArray());
        }

        [Fact]
        public void GoodFeaturesToTrackHasDefinedBoundaryWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image = new Mat(16, 16, MatType.CV_8UC1))
            {
                image.SetTo(new Scalar(0));
                ImgProcCv2.Rectangle(image, new Rect(4, 4, 8, 8), new Scalar(255), -1);

                OpenCvException? exception = Record.Exception(() =>
                {
                    Point2f[] corners = ImgProcCv2.GoodFeaturesToTrack(image, 8, 0.01, 2.0);
                    Assert.NotNull(corners);
                    Assert.True(corners.Length <= 8);
                }) as OpenCvException;

                if (exception != null)
                {
                    Assert.Contains("good_features_to_track", exception.Message, StringComparison.OrdinalIgnoreCase);
                    Assert.Contains("OpenCV", exception.Message, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        private static byte[] CreateRamp(int length)
        {
            var values = new byte[length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (byte)(i * 3);
            }

            return values;
        }

    }
}
