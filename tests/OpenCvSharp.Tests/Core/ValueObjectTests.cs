using System;
using System.Globalization;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Tests.Core
{
    public class ValueObjectTests
    {
        [Fact]
        public void RectReportsGeometryAndContainsPoints()
        {
            Rect rect = new Rect(10, 20, 30, 40);
            Rect same = new Rect(new Point(10, 20), new Size(30, 40));

            Assert.Equal(10, rect.X);
            Assert.Equal(20, rect.Y);
            Assert.Equal(30, rect.Width);
            Assert.Equal(40, rect.Height);
            Assert.Equal(10, rect.Left);
            Assert.Equal(20, rect.Top);
            Assert.Equal(40, rect.Right);
            Assert.Equal(60, rect.Bottom);
            Assert.Equal(1200, rect.Area);
            Assert.False(rect.Empty);
            Assert.Equal(new Size(30, 40).ToString(), rect.Size.ToString());
            Assert.Equal(new Point(10, 20).ToString(), rect.Location.ToString());

            Assert.True(rect.Contains(new Point(10, 20)));
            Assert.True(rect.Contains(39, 59));
            Assert.False(rect.Contains(40, 59));
            Assert.False(rect.Contains(39, 60));
            Assert.Equal(same, rect);
            Assert.True(rect == same);
            Assert.False(rect.Equals("not a rect"));
            Assert.Equal(same.GetHashCode(), rect.GetHashCode());
            Assert.NotEqual(new Rect(9, 20, 30, 40), rect);
            Assert.NotEqual(new Rect(10, 9, 30, 40), rect);
            Assert.NotEqual(new Rect(10, 20, 9, 40), rect);
            Assert.True(rect != new Rect(10, 20, 30, 9));
            Assert.Equal("{X=10,Y=20,Width=30,Height=40}", rect.ToString());
        }

        [Fact]
        public void RectReportsEmptyForNonPositiveDimensions()
        {
            Assert.True(new Rect(0, 0, 0, 1).Empty);
            Assert.True(new Rect(0, 0, 1, 0).Empty);
            Assert.True(new Rect(0, 0, -1, 1).Empty);
        }

        [Fact]
        public void RectHasSequentialInterleavedIntegerLayout()
        {
            Assert.Equal(16, Marshal.SizeOf<Rect>());

            Rect[] rects = new[]
            {
                new Rect(1, 2, 3, 4),
                new Rect(5, 6, 7, 8)
            };

            ReadOnlySpan<int> fields = MemoryMarshal.Cast<Rect, int>(rects.AsSpan());

            Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, fields.ToArray());
        }

        [Fact]
        public void Rect2dHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(32, Marshal.SizeOf<Rect2d>());

            Rect2d[] rects = new[]
            {
                new Rect2d(1.5, 2.5, 3.5, 4.5),
                new Rect2d(5.5, 6.5, 7.5, 8.5)
            };

            ReadOnlySpan<double> fields = MemoryMarshal.Cast<Rect2d, double>(rects.AsSpan());

            Assert.Equal(new double[] { 1.5, 2.5, 3.5, 4.5, 5.5, 6.5, 7.5, 8.5 }, fields.ToArray());
        }

        [Fact]
        public void ScalarReportsComponents()
        {
            Scalar scalar = new Scalar(1, 2, 3, 4);
            Scalar same = new Scalar(1, 2, 3, 4);

            Assert.Equal(1, scalar.V0);
            Assert.Equal(2, scalar.V1);
            Assert.Equal(3, scalar.V2);
            Assert.Equal(4, scalar.V3);
            Assert.Equal(1, scalar[0]);
            Assert.Equal(2, scalar[1]);
            Assert.Equal(3, scalar[2]);
            Assert.Equal(4, scalar[3]);
            Assert.Equal(same, scalar);
            Assert.True(scalar == same);
            Assert.False(scalar.Equals("not a scalar"));
            Assert.Equal(same.GetHashCode(), scalar.GetHashCode());
            Assert.NotEqual(new Scalar(9, 2, 3, 4), scalar);
            Assert.NotEqual(new Scalar(1, 9, 3, 4), scalar);
            Assert.NotEqual(new Scalar(1, 2, 9, 4), scalar);
            Assert.True(scalar != new Scalar(1, 2, 3, 9));
            Assert.Equal("{V0=1,V1=2,V2=3,V3=4}", scalar.ToString());
        }

        [Fact]
        public void ScalarConstructorsFillMissingComponents()
        {
            Scalar repeated = new Scalar(7);
            Scalar three = new Scalar(1, 2, 3);

            Assert.Equal(7, repeated.V0);
            Assert.Equal(7, repeated.V1);
            Assert.Equal(7, repeated.V2);
            Assert.Equal(7, repeated.V3);

            Assert.Equal(1, three.V0);
            Assert.Equal(2, three.V1);
            Assert.Equal(3, three.V2);
            Assert.Equal(0, three.V3);
        }

        [Fact]
        public void ScalarIndexerRejectsInvalidIndex()
        {
            Scalar scalar = new Scalar(1, 2, 3, 4);

            Assert.Throws<System.IndexOutOfRangeException>(() => scalar[-1]);
            Assert.Throws<System.IndexOutOfRangeException>(() => scalar[4]);
        }

        [Fact]
        public void ScalarHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(32, Marshal.SizeOf<Scalar>());

            Scalar[] scalars = new[]
            {
                new Scalar(1, 2, 3, 4),
                new Scalar(5, 6, 7, 8)
            };

            ReadOnlySpan<double> fields = MemoryMarshal.Cast<Scalar, double>(scalars.AsSpan());

            Assert.Equal(new double[] { 1, 2, 3, 4, 5, 6, 7, 8 }, fields.ToArray());
        }

        [Fact]
        public void CoreStatisticResultsHaveSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(32, Marshal.SizeOf<MinMaxLocResult>());
            Assert.Equal(64, Marshal.SizeOf<MeanStdDevResult>());

            MinMaxLocResult[] minMaxResults =
            {
                new MinMaxLocResult(1.5, 2.5, new Point(3, 4), new Point(5, 6)),
                new MinMaxLocResult(7.5, 8.5, new Point(9, 10), new Point(11, 12))
            };
            MeanStdDevResult[] meanStdDevResults =
            {
                new MeanStdDevResult(new Scalar(1, 2, 3, 4), new Scalar(5, 6, 7, 8)),
                new MeanStdDevResult(new Scalar(9, 10, 11, 12), new Scalar(13, 14, 15, 16))
            };

            ReadOnlySpan<byte> minMaxBytes = MemoryMarshal.AsBytes(minMaxResults.AsSpan());
            ReadOnlySpan<double> meanStdDevFields = MemoryMarshal.Cast<MeanStdDevResult, double>(meanStdDevResults.AsSpan());

            Assert.Equal(BitConverter.GetBytes(1.5), minMaxBytes.Slice(0, 8).ToArray());
            Assert.Equal(BitConverter.GetBytes(2.5), minMaxBytes.Slice(8, 8).ToArray());
            Assert.Equal(BitConverter.GetBytes(3), minMaxBytes.Slice(16, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(4), minMaxBytes.Slice(20, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(5), minMaxBytes.Slice(24, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(6), minMaxBytes.Slice(28, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(7.5), minMaxBytes.Slice(32, 8).ToArray());
            Assert.Equal(BitConverter.GetBytes(8.5), minMaxBytes.Slice(40, 8).ToArray());
            Assert.Equal(BitConverter.GetBytes(9), minMaxBytes.Slice(48, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(10), minMaxBytes.Slice(52, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(11), minMaxBytes.Slice(56, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(12), minMaxBytes.Slice(60, 4).ToArray());
            Assert.Equal(
                new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 },
                meanStdDevFields.ToArray());
        }

        [Fact]
        public void CoreStatisticResultsReportValueEquality()
        {
            var minMax = new MinMaxLocResult(1.5, 2.5, new Point(3, 4), new Point(5, 6));
            var sameMinMax = new MinMaxLocResult(1.5, 2.5, new Point(3, 4), new Point(5, 6));
            var meanStdDev = new MeanStdDevResult(new Scalar(1, 2, 3, 4), new Scalar(5, 6, 7, 8));
            var sameMeanStdDev = new MeanStdDevResult(new Scalar(1, 2, 3, 4), new Scalar(5, 6, 7, 8));

            Assert.Equal(sameMinMax, minMax);
            Assert.True(minMax == sameMinMax);
            Assert.False(minMax.Equals("not a minmax result"));
            Assert.Equal(sameMinMax.GetHashCode(), minMax.GetHashCode());
            Assert.NotEqual(new MinMaxLocResult(9.5, 2.5, new Point(3, 4), new Point(5, 6)), minMax);
            Assert.NotEqual(new MinMaxLocResult(1.5, 9.5, new Point(3, 4), new Point(5, 6)), minMax);
            Assert.NotEqual(new MinMaxLocResult(1.5, 2.5, new Point(9, 4), new Point(5, 6)), minMax);
            Assert.True(minMax != new MinMaxLocResult(1.5, 2.5, new Point(3, 4), new Point(9, 6)));

            Assert.Equal(sameMeanStdDev, meanStdDev);
            Assert.True(meanStdDev == sameMeanStdDev);
            Assert.False(meanStdDev.Equals("not a mean/stddev result"));
            Assert.Equal(sameMeanStdDev.GetHashCode(), meanStdDev.GetHashCode());
            Assert.NotEqual(new MeanStdDevResult(new Scalar(9, 2, 3, 4), new Scalar(5, 6, 7, 8)), meanStdDev);
            Assert.True(meanStdDev != new MeanStdDevResult(new Scalar(1, 2, 3, 4), new Scalar(9, 6, 7, 8)));
        }

        [Fact]
        public void SizeReportsDimensionsAndEquality()
        {
            Size size = new Size(3, 4);
            Size same = new Size(3, 4);

            Assert.Equal(3, size.Width);
            Assert.Equal(4, size.Height);
            Assert.Equal(12, size.Area);
            Assert.False(size.Empty);
            Assert.Equal(same, size);
            Assert.True(size == same);
            Assert.False(size.Equals("not a size"));
            Assert.Equal(same.GetHashCode(), size.GetHashCode());
            Assert.NotEqual(new Size(9, 4), size);
            Assert.True(size != new Size(3, 9));
            Assert.Equal("{Width=3,Height=4}", size.ToString());
        }

        [Fact]
        public void SizeReportsEmptyForNonPositiveDimensions()
        {
            Assert.True(new Size(0, 1).Empty);
            Assert.True(new Size(1, 0).Empty);
            Assert.True(new Size(-1, 1).Empty);
        }

        [Fact]
        public void SizeHasSequentialInterleavedIntegerLayout()
        {
            Assert.Equal(8, Marshal.SizeOf<Size>());

            Size[] sizes = new[]
            {
                new Size(1, 2),
                new Size(3, 4)
            };

            ReadOnlySpan<int> fields = MemoryMarshal.Cast<Size, int>(sizes.AsSpan());

            Assert.Equal(new int[] { 1, 2, 3, 4 }, fields.ToArray());
        }

        [Fact]
        public void Point2fReportsCoordinates()
        {
            Point2f point = new Point2f(1.5F, -2.25F);
            Point2f same = new Point2f(1.5F, -2.25F);
            Point2f different = new Point2f(1.5F, 2.25F);

            Assert.Equal(1.5F, point.X);
            Assert.Equal(-2.25F, point.Y);
            Assert.Equal(same, point);
            Assert.True(point == same);
            Assert.True(point != different);
            Assert.False(point.Equals("not a point"));
            Assert.Equal(same.GetHashCode(), point.GetHashCode());
            Assert.Equal("{X=1.5,Y=-2.25}", point.ToString());
        }

        [Fact]
        public void PointReportsCoordinatesAndEquality()
        {
            Point point = new Point(1, -2);
            Point same = new Point(1, -2);

            Assert.Equal(1, point.X);
            Assert.Equal(-2, point.Y);
            Assert.Equal(same, point);
            Assert.True(point == same);
            Assert.False(point.Equals("not a point"));
            Assert.Equal(same.GetHashCode(), point.GetHashCode());
            Assert.NotEqual(new Point(9, -2), point);
            Assert.True(point != new Point(1, 9));
            Assert.Equal("{X=1,Y=-2}", point.ToString());
        }

        [Fact]
        public void PointHasSequentialInterleavedIntegerLayout()
        {
            Assert.Equal(8, Marshal.SizeOf<Point>());

            Point[] points = new Point[]
            {
                new Point(1, 2),
                new Point(3, 4)
            };

            ReadOnlySpan<int> xy = MemoryMarshal.Cast<Point, int>(points.AsSpan());

            Assert.Equal(new int[] { 1, 2, 3, 4 }, xy.ToArray());
        }

        [Fact]
        public void Point2fHasSequentialInterleavedFloatLayout()
        {
            Assert.Equal(8, Marshal.SizeOf<Point2f>());

            Point2f[] points = new Point2f[]
            {
                new Point2f(1.5F, 2.5F),
                new Point2f(3.5F, 4.5F)
            };

            ReadOnlySpan<float> xy = MemoryMarshal.Cast<Point2f, float>(points.AsSpan());

            Assert.Equal(new float[] { 1.5F, 2.5F, 3.5F, 4.5F }, xy.ToArray());
        }

        [Fact]
        public void Point2dReportsCoordinates()
        {
            Point2d point = new Point2d(1.5, -2.25);
            Point2d same = new Point2d(1.5, -2.25);
            Point2d different = new Point2d(1.5, 2.25);

            Assert.Equal(1.5, point.X);
            Assert.Equal(-2.25, point.Y);
            Assert.Equal(same, point);
            Assert.True(point == same);
            Assert.True(point != different);
            Assert.False(point.Equals("not a point"));
            Assert.Equal(same.GetHashCode(), point.GetHashCode());
            Assert.Equal("{X=1.5,Y=-2.25}", point.ToString());
        }

        [Fact]
        public void Point2dHasSequentialInterleavedDoubleLayout()
        {
            Assert.Equal(16, Marshal.SizeOf<Point2d>());

            Point2d[] points = new[]
            {
                new Point2d(1.5, 2.5),
                new Point2d(3.5, 4.5)
            };

            ReadOnlySpan<double> xy = MemoryMarshal.Cast<Point2d, double>(points.AsSpan());

            Assert.Equal(new double[] { 1.5, 2.5, 3.5, 4.5 }, xy.ToArray());
        }

        [Fact]
        public void Point3iReportsCoordinatesAndEquality()
        {
            Point3i point = new Point3i(1, -2, 3);
            Point3i same = new Point3i(1, -2, 3);

            Assert.Equal(1, point.X);
            Assert.Equal(-2, point.Y);
            Assert.Equal(3, point.Z);
            Assert.Equal(same, point);
            Assert.True(point == same);
            Assert.False(point.Equals("not a point"));
            Assert.Equal(same.GetHashCode(), point.GetHashCode());
            Assert.NotEqual(new Point3i(9, -2, 3), point);
            Assert.NotEqual(new Point3i(1, 9, 3), point);
            Assert.NotEqual(new Point3i(1, -2, 9), point);
            Assert.Equal("{X=1,Y=-2,Z=3}", point.ToString());
        }

        [Fact]
        public void Point3fReportsCoordinatesAndEquality()
        {
            Point3f point = new Point3f(1.5F, -2.25F, 3.75F);
            Point3f same = new Point3f(1.5F, -2.25F, 3.75F);

            Assert.Equal(1.5F, point.X);
            Assert.Equal(-2.25F, point.Y);
            Assert.Equal(3.75F, point.Z);
            Assert.Equal(same, point);
            Assert.True(point == same);
            Assert.False(point.Equals("not a point"));
            Assert.Equal(same.GetHashCode(), point.GetHashCode());
            Assert.NotEqual(new Point3f(9.5F, -2.25F, 3.75F), point);
            Assert.NotEqual(new Point3f(1.5F, 9.25F, 3.75F), point);
            Assert.NotEqual(new Point3f(1.5F, -2.25F, 9.75F), point);
            Assert.Equal("{X=1.5,Y=-2.25,Z=3.75}", point.ToString());
        }

        [Fact]
        public void FloatingPointTypesFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                Assert.Equal("{X=1.5,Y=-2.25}", new Point2f(1.5F, -2.25F).ToString());
                Assert.Equal("{X=1.5,Y=-2.25}", new Point2d(1.5, -2.25).ToString());
                Assert.Equal("{X=1.5,Y=-2.25,Z=3.75}", new Point3f(1.5F, -2.25F, 3.75F).ToString());
                Assert.Equal("{Width=3.5,Height=4.5}", new Size2f(3.5F, 4.5F).ToString());
                Assert.Equal("{Width=3.5,Height=4.5}", new Size2d(3.5, 4.5).ToString());
                Assert.Equal("{X=1.5,Y=-2.25,Width=3.5,Height=4.5}", new Rect2d(1.5, -2.25, 3.5, 4.5).ToString());
                Assert.Equal("{Center={X=1.5,Y=2.5},Size={Width=3.5,Height=4.5},Angle=12.5}", new RotatedRect(new Point2f(1.5F, 2.5F), new Size2f(3.5F, 4.5F), 12.5F).ToString());
                Assert.Equal("{V0=1.5,V1=-2.25,V2=3.75,V3=4.5}", new Scalar(1.5, -2.25, 3.75, 4.5).ToString());
                Assert.Equal("{V0=1.5,V1=-2.25,V2=3.75,V3=4.5}", new Vec4f(1.5F, -2.25F, 3.75F, 4.5F).ToString());
                Assert.Equal("{Type=CountOrEps,MaxCount=30,Epsilon=0.001}", TermCriteria.ByCountAndEpsilon(30, 0.001).ToString());
                Assert.Equal("{MinVal=1.5,MaxVal=2.25,MinLoc={X=3,Y=4},MaxLoc={X=5,Y=6}}", new MinMaxLocResult(1.5, 2.25, new Point(3, 4), new Point(5, 6)).ToString());
                Assert.Equal("{Mean={V0=1.5,V1=-2.25,V2=3.75,V3=4.5},StdDev={V0=0.125,V1=1.25,V2=2.5,V3=5.75}}", new MeanStdDevResult(new Scalar(1.5, -2.25, 3.75, 4.5), new Scalar(0.125, 1.25, 2.5, 5.75)).ToString());
                Assert.Equal(
                    "{M00=1.5,M10=-2.25,M01=3.75}",
                    CreateMoments(1.5, -2.25, 3.75, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0).ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void Point3TypesHaveSequentialInterleavedLayout()
        {
            Assert.Equal(12, Marshal.SizeOf<Point3i>());
            Assert.Equal(12, Marshal.SizeOf<Point3f>());

            Point3i[] integerPoints = new[]
            {
                new Point3i(1, 2, 3),
                new Point3i(4, 5, 6)
            };
            Point3f[] floatPoints = new[]
            {
                new Point3f(1.5F, 2.5F, 3.5F),
                new Point3f(4.5F, 5.5F, 6.5F)
            };

            ReadOnlySpan<int> xyzInteger = MemoryMarshal.Cast<Point3i, int>(integerPoints.AsSpan());
            ReadOnlySpan<float> xyzFloat = MemoryMarshal.Cast<Point3f, float>(floatPoints.AsSpan());

            Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6 }, xyzInteger.ToArray());
            Assert.Equal(new float[] { 1.5F, 2.5F, 3.5F, 4.5F, 5.5F, 6.5F }, xyzFloat.ToArray());
        }

        [Fact]
        public void Size2fReportsDimensions()
        {
            Size2f size = new Size2f(3.5F, 4.5F);
            Size2f same = new Size2f(3.5F, 4.5F);
            Size2f different = new Size2f(4.5F, 3.5F);

            Assert.Equal(3.5F, size.Width);
            Assert.Equal(4.5F, size.Height);
            Assert.Equal(15.75F, size.Area);
            Assert.False(size.Empty);
            Assert.Equal(same, size);
            Assert.True(size == same);
            Assert.True(size != different);
            Assert.Equal("{Width=3.5,Height=4.5}", size.ToString());
        }

        [Fact]
        public void Size2dReportsDimensions()
        {
            Size2d size = new Size2d(3.5, 4.5);
            Size2d same = new Size2d(3.5, 4.5);
            Size2d different = new Size2d(4.5, 3.5);

            Assert.Equal(3.5, size.Width);
            Assert.Equal(4.5, size.Height);
            Assert.Equal(15.75, size.Area);
            Assert.False(size.Empty);
            Assert.Equal(same, size);
            Assert.True(size == same);
            Assert.True(size != different);
            Assert.Equal("{Width=3.5,Height=4.5}", size.ToString());
        }

        [Fact]
        public void Size2TypesHaveSequentialInterleavedLayout()
        {
            Assert.Equal(8, Marshal.SizeOf<Size2f>());
            Assert.Equal(16, Marshal.SizeOf<Size2d>());

            Size2f[] floatSizes = new[]
            {
                new Size2f(1.5F, 2.5F),
                new Size2f(3.5F, 4.5F)
            };
            Size2d[] doubleSizes = new[]
            {
                new Size2d(1.5, 2.5),
                new Size2d(3.5, 4.5)
            };

            ReadOnlySpan<float> floatFields = MemoryMarshal.Cast<Size2f, float>(floatSizes.AsSpan());
            ReadOnlySpan<double> doubleFields = MemoryMarshal.Cast<Size2d, double>(doubleSizes.AsSpan());

            Assert.Equal(new float[] { 1.5F, 2.5F, 3.5F, 4.5F }, floatFields.ToArray());
            Assert.Equal(new double[] { 1.5, 2.5, 3.5, 4.5 }, doubleFields.ToArray());
        }

        [Fact]
        public void RotatedRectReportsFields()
        {
            RotatedRect rect = new RotatedRect(new Point2f(1.5F, 2.5F), new Size2f(3.5F, 4.5F), 12.5F);
            RotatedRect same = new RotatedRect(new Point2f(1.5F, 2.5F), new Size2f(3.5F, 4.5F), 12.5F);

            Assert.Equal(new Point2f(1.5F, 2.5F), rect.Center);
            Assert.Equal(new Size2f(3.5F, 4.5F), rect.Size);
            Assert.Equal(12.5F, rect.Angle);
            Assert.Equal(15.75F, rect.Area);
            Assert.Equal(same, rect);
            Assert.True(rect == same);
            Assert.False(rect.Equals("not a rect"));
            Assert.Equal(same.GetHashCode(), rect.GetHashCode());
            Assert.NotEqual(new RotatedRect(new Point2f(9.5F, 2.5F), new Size2f(3.5F, 4.5F), 12.5F), rect);
            Assert.NotEqual(new RotatedRect(new Point2f(1.5F, 9.5F), new Size2f(3.5F, 4.5F), 12.5F), rect);
            Assert.NotEqual(new RotatedRect(new Point2f(1.5F, 2.5F), new Size2f(9.5F, 4.5F), 12.5F), rect);
            Assert.NotEqual(new RotatedRect(new Point2f(1.5F, 2.5F), new Size2f(3.5F, 9.5F), 12.5F), rect);
            Assert.True(rect != new RotatedRect(new Point2f(1.5F, 2.5F), new Size2f(3.5F, 4.5F), 13.5F));
            Assert.Equal("{Center={X=1.5,Y=2.5},Size={Width=3.5,Height=4.5},Angle=12.5}", rect.ToString());
        }

        [Fact]
        public void RotatedRectHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(20, Marshal.SizeOf<RotatedRect>());

            RotatedRect[] rects = new[]
            {
                new RotatedRect(new Point2f(1.5F, 2.5F), new Size2f(3.5F, 4.5F), 5.5F),
                new RotatedRect(new Point2f(6.5F, 7.5F), new Size2f(8.5F, 9.5F), 10.5F)
            };

            ReadOnlySpan<float> fields = MemoryMarshal.Cast<RotatedRect, float>(rects.AsSpan());

            Assert.Equal(
                new float[] { 1.5F, 2.5F, 3.5F, 4.5F, 5.5F, 6.5F, 7.5F, 8.5F, 9.5F, 10.5F },
                fields.ToArray());
        }

        [Fact]
        public void TermCriteriaFactoriesReportOpenCvCompatibleFields()
        {
            TermCriteria count = TermCriteria.ByCount(20);
            TermCriteria epsilon = TermCriteria.ByEpsilon(0.01);
            TermCriteria countOrEpsilon = TermCriteria.ByCountAndEpsilon(30, 0.001);

            Assert.Equal(TermCriteriaTypes.Count, count.Type);
            Assert.Equal(20, count.MaxCount);
            Assert.Equal(0.0, count.Epsilon);

            Assert.Equal(TermCriteriaTypes.Eps, epsilon.Type);
            Assert.Equal(0, epsilon.MaxCount);
            Assert.Equal(0.01, epsilon.Epsilon);

            Assert.Equal(TermCriteriaTypes.CountOrEps, countOrEpsilon.Type);
            Assert.Equal(30, countOrEpsilon.MaxCount);
            Assert.Equal(0.001, countOrEpsilon.Epsilon);
            Assert.Equal("{Type=CountOrEps,MaxCount=30,Epsilon=0.001}", countOrEpsilon.ToString());
        }

        [Fact]
        public void TermCriteriaEqualityUsesAllOpenCvFields()
        {
            TermCriteria value = TermCriteria.ByCountAndEpsilon(30, 0.001);
            TermCriteria same = TermCriteria.ByCountAndEpsilon(30, 0.001);

            Assert.Equal(same, value);
            Assert.True(value == same);
            Assert.False(value.Equals("not criteria"));
            Assert.Equal(same.GetHashCode(), value.GetHashCode());
            Assert.NotEqual(TermCriteria.ByEpsilon(0.001), value);
            Assert.NotEqual(TermCriteria.ByCountAndEpsilon(31, 0.001), value);
            Assert.NotEqual(TermCriteria.ByCountAndEpsilon(30, 0.002), value);
        }

        [Fact]
        public void Vec4fReportsValues()
        {
            Vec4f value = new Vec4f(1.0F, 2.0F, 3.0F, 4.0F);
            Vec4f same = new Vec4f(1.0F, 2.0F, 3.0F, 4.0F);

            Assert.Equal(1.0F, value.V0);
            Assert.Equal(2.0F, value.V1);
            Assert.Equal(3.0F, value.V2);
            Assert.Equal(4.0F, value.V3);
            Assert.Equal(1.0F, value[0]);
            Assert.Equal(2.0F, value[1]);
            Assert.Equal(3.0F, value[2]);
            Assert.Equal(4.0F, value[3]);
            Assert.Equal(same, value);
            Assert.True(value == same);
            Assert.False(value.Equals("not a vector"));
            Assert.Equal(same.GetHashCode(), value.GetHashCode());
            Assert.NotEqual(new Vec4f(9.0F, 2.0F, 3.0F, 4.0F), value);
            Assert.NotEqual(new Vec4f(1.0F, 9.0F, 3.0F, 4.0F), value);
            Assert.NotEqual(new Vec4f(1.0F, 2.0F, 9.0F, 4.0F), value);
            Assert.True(value != new Vec4f(1.0F, 2.0F, 3.0F, 5.0F));
            Assert.Throws<System.IndexOutOfRangeException>(() => value[-1]);
            Assert.Throws<System.IndexOutOfRangeException>(() => value[4]);
            Assert.Equal("{V0=1,V1=2,V2=3,V3=4}", value.ToString());
        }

        [Fact]
        public void Vec4iReportsValues()
        {
            Vec4i value = new Vec4i(1, 2, 3, 4);
            Vec4i same = new Vec4i(1, 2, 3, 4);

            Assert.Equal(1, value.V0);
            Assert.Equal(2, value.V1);
            Assert.Equal(3, value.V2);
            Assert.Equal(4, value.V3);
            Assert.Equal(1, value[0]);
            Assert.Equal(2, value[1]);
            Assert.Equal(3, value[2]);
            Assert.Equal(4, value[3]);
            Assert.Equal(same, value);
            Assert.True(value == same);
            Assert.False(value.Equals("not a vector"));
            Assert.Equal(same.GetHashCode(), value.GetHashCode());
            Assert.NotEqual(new Vec4i(9, 2, 3, 4), value);
            Assert.NotEqual(new Vec4i(1, 9, 3, 4), value);
            Assert.NotEqual(new Vec4i(1, 2, 9, 4), value);
            Assert.True(value != new Vec4i(1, 2, 3, 5));
            Assert.Throws<System.IndexOutOfRangeException>(() => value[-1]);
            Assert.Throws<System.IndexOutOfRangeException>(() => value[4]);
            Assert.Equal("{V0=1,V1=2,V2=3,V3=4}", value.ToString());
        }

        [Fact]
        public void Vec4TypesHaveSequentialInterleavedLayout()
        {
            Assert.Equal(16, Marshal.SizeOf<Vec4f>());
            Assert.Equal(16, Marshal.SizeOf<Vec4i>());

            Vec4f[] floatVectors = new[]
            {
                new Vec4f(1.0F, 2.0F, 3.0F, 4.0F),
                new Vec4f(5.0F, 6.0F, 7.0F, 8.0F)
            };
            Vec4i[] integerVectors = new[]
            {
                new Vec4i(1, 2, 3, 4),
                new Vec4i(5, 6, 7, 8)
            };

            ReadOnlySpan<float> floatFields = MemoryMarshal.Cast<Vec4f, float>(floatVectors.AsSpan());
            ReadOnlySpan<int> integerFields = MemoryMarshal.Cast<Vec4i, int>(integerVectors.AsSpan());

            Assert.Equal(new float[] { 1.0F, 2.0F, 3.0F, 4.0F, 5.0F, 6.0F, 7.0F, 8.0F }, floatFields.ToArray());
            Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, integerFields.ToArray());
        }

        [Fact]
        public void MomentsReportsOpenCvFieldOrderAndEquality()
        {
            double[] values = CreateSequentialMomentValues(0);
            double[] differentValues = CreateSequentialMomentValues(0);
            differentValues[23] = 24;
            Moments moments = CreateMoments(values);
            Moments same = CreateMoments(CreateSequentialMomentValues(0));
            Moments different = CreateMoments(differentValues);

            Assert.Equal(0, moments.M00);
            Assert.Equal(1, moments.M10);
            Assert.Equal(2, moments.M01);
            Assert.Equal(10, moments.Mu20);
            Assert.Equal(17, moments.Nu20);
            Assert.Equal(23, moments.Nu03);
            Assert.Equal(values, moments.ToArray());
            Assert.Equal(23, moments[23]);
            Assert.Equal(same, moments);
            Assert.True(moments == same);
            Assert.True(moments != different);
            Assert.False(moments.Equals("not moments"));
            Assert.Equal(same.GetHashCode(), moments.GetHashCode());
            Assert.Throws<System.IndexOutOfRangeException>(() => moments[-1]);
            Assert.Throws<System.IndexOutOfRangeException>(() => moments[24]);
            Assert.Equal("{M00=0,M10=1,M01=2}", moments.ToString());
        }

        [Fact]
        public void MomentsEqualityUsesEachOpenCvField()
        {
            double[] values = CreateSequentialMomentValues(0);
            Moments moments = CreateMoments(values);

            for (int i = 0; i < values.Length; i++)
            {
                double[] changedValues = CreateSequentialMomentValues(0);
                changedValues[i] = 100 + i;

                Assert.NotEqual(moments, CreateMoments(changedValues));
            }
        }

        [Fact]
        public void MomentsHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(192, Marshal.SizeOf<Moments>());

            Moments[] moments = new[]
            {
                CreateMoments(CreateSequentialMomentValues(0)),
                CreateMoments(CreateSequentialMomentValues(24))
            };
            double[] expected = new double[48];
            for (int i = 0; i < expected.Length; i++)
            {
                expected[i] = i;
            }

            ReadOnlySpan<double> fields = MemoryMarshal.Cast<Moments, double>(moments.AsSpan());

            Assert.Equal(expected, fields.ToArray());
        }

        [Fact]
        public void CoreLinearAlgebraEnumsMatchOpenCvValues()
        {
            Assert.Equal(1, (int)SvdFlags.ModifyA);
            Assert.Equal(2, (int)SvdFlags.NoUv);
            Assert.Equal(4, (int)SvdFlags.FullUv);
            Assert.Equal(1, (int)GemmFlags.TransposeSrc1);
            Assert.Equal(2, (int)GemmFlags.TransposeSrc2);
            Assert.Equal(4, (int)GemmFlags.TransposeSrc3);
            Assert.Equal(1, (int)TermCriteriaTypes.Count);
            Assert.Equal((int)TermCriteriaTypes.Count, (int)TermCriteriaTypes.MaxIter);
            Assert.Equal(2, (int)TermCriteriaTypes.Eps);
            Assert.Equal(3, (int)TermCriteriaTypes.CountOrEps);
            Assert.Equal(1, (int)DftFlags.Inverse);
            Assert.Equal(2, (int)DftFlags.Scale);
            Assert.Equal(4, (int)DftFlags.Rows);
            Assert.Equal(16, (int)DftFlags.ComplexOutput);
            Assert.Equal(32, (int)DftFlags.RealOutput);
            Assert.Equal(64, (int)DftFlags.ComplexInput);
            Assert.Equal(1, (int)DctFlags.Inverse);
            Assert.Equal(4, (int)DctFlags.Rows);
            Assert.Equal(4, (int)MulSpectrumsFlags.Rows);
            Assert.Equal(0, (int)RngDistributionTypes.Uniform);
            Assert.Equal(1, (int)RngDistributionTypes.Normal);
        }

        private static double[] CreateSequentialMomentValues(double start)
        {
            double[] values = new double[24];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = start + i;
            }

            return values;
        }

        private static Moments CreateMoments(params double[] values)
        {
            if (values.Length != 24)
            {
                throw new ArgumentException("Moments require exactly 24 values.", nameof(values));
            }

            return new Moments(
                values[0],
                values[1],
                values[2],
                values[3],
                values[4],
                values[5],
                values[6],
                values[7],
                values[8],
                values[9],
                values[10],
                values[11],
                values[12],
                values[13],
                values[14],
                values[15],
                values[16],
                values[17],
                values[18],
                values[19],
                values[20],
                values[21],
                values[22],
                values[23]);
        }
    }
}
