using System;
using System.Globalization;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace OpenCvSharp.Tests.Features2D
{
    public class KeyPointTests
    {
        [Fact]
        public void DMatchReportsOpenCvCompatibleFields()
        {
            DMatch match = new DMatch(queryIdx: 1, trainIdx: 2, imgIdx: 3, distance: 4.5F);

            Assert.Equal(1, match.QueryIdx);
            Assert.Equal(2, match.TrainIdx);
            Assert.Equal(3, match.ImgIdx);
            Assert.Equal(4.5F, match.Distance);
            Assert.Equal(1.0F, match[0]);
            Assert.Equal(2.0F, match[1]);
            Assert.Equal(3.0F, match[2]);
            Assert.Equal(4.5F, match[3]);
            Assert.Equal("{QueryIdx=1,TrainIdx=2,ImgIdx=3,Distance=4.5}", match.ToString());
            Assert.Throws<IndexOutOfRangeException>(() => match[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => match[4]);
        }

        [Fact]
        public void DMatchShortConstructorUsesDefaultImageIndex()
        {
            DMatch match = new DMatch(queryIdx: 1, trainIdx: 2, distance: 3.5F);

            Assert.Equal(1, match.QueryIdx);
            Assert.Equal(2, match.TrainIdx);
            Assert.Equal(0, match.ImgIdx);
            Assert.Equal(3.5F, match.Distance);
        }

        [Fact]
        public void DMatchEqualityComparisonAndHashUseOpenCvFields()
        {
            DMatch value = new DMatch(1, 2, 3, 4.5F);
            DMatch same = new DMatch(1, 2, 3, 4.5F);
            DMatch differentDistance = new DMatch(1, 2, 3, 9.0F);

            Assert.Equal(same, value);
            Assert.True(value == same);
            Assert.True(value != differentDistance);
            Assert.False(value.Equals("not a match"));
            Assert.Equal(same.GetHashCode(), value.GetHashCode());
            Assert.True(value.CompareTo(differentDistance) < 0);
            Assert.True(differentDistance.CompareTo(value) > 0);
            Assert.Equal(0, value.CompareTo(same));
        }

        [Fact]
        public void DMatchInequalityUsesEachOpenCvField()
        {
            DMatch value = new DMatch(1, 2, 3, 4.5F);

            Assert.NotEqual(new DMatch(9, 2, 3, 4.5F), value);
            Assert.NotEqual(new DMatch(1, 9, 3, 4.5F), value);
            Assert.NotEqual(new DMatch(1, 2, 9, 4.5F), value);
            Assert.NotEqual(new DMatch(1, 2, 3, 9.0F), value);
        }

        [Fact]
        public void KeyPointReportsOpenCvCompatibleFields()
        {
            KeyPoint keyPoint = new KeyPoint(new Point2f(12.5F, 7.25F), 31.0F, 90.0F, 0.75F, 2, 42);

            Assert.Equal(new Point2f(12.5F, 7.25F), keyPoint.Pt);
            Assert.Equal(12.5F, keyPoint.X);
            Assert.Equal(7.25F, keyPoint.Y);
            Assert.Equal(31.0F, keyPoint.Size);
            Assert.Equal(90.0F, keyPoint.Angle);
            Assert.Equal(0.75F, keyPoint.Response);
            Assert.Equal(2, keyPoint.Octave);
            Assert.Equal(42, keyPoint.ClassId);
            Assert.Equal("{Pt={X=12.5,Y=7.25},Size=31,Angle=90,Response=0.75,Octave=2,ClassId=42}", keyPoint.ToString());
        }

        [Fact]
        public void KeyPointToStringUsesInvariantCulture()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

                KeyPoint keyPoint = new KeyPoint(new Point2f(12.5F, 7.25F), 31.5F, 90.25F, 0.75F, 2, 42);

                Assert.Equal("{Pt={X=12.5,Y=7.25},Size=31.5,Angle=90.25,Response=0.75,Octave=2,ClassId=42}", keyPoint.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void KeyPointCoordinateConstructorFillsDefaults()
        {
            KeyPoint keyPoint = new KeyPoint(1.5F, 2.5F, 9.0F);

            Assert.Equal(new Point2f(1.5F, 2.5F), keyPoint.Pt);
            Assert.Equal(9.0F, keyPoint.Size);
            Assert.Equal(-1.0F, keyPoint.Angle);
            Assert.Equal(0.0F, keyPoint.Response);
            Assert.Equal(0, keyPoint.Octave);
            Assert.Equal(-1, keyPoint.ClassId);
        }

        [Fact]
        public void KeyPointEqualityUsesAllOpenCvFields()
        {
            KeyPoint value = new KeyPoint(1.0F, 2.0F, 3.0F, 4.0F, 5, 6);
            KeyPoint same = new KeyPoint(1.0F, 2.0F, 3.0F, 4.0F, 5, 6);
            KeyPoint different = new KeyPoint(1.0F, 2.0F, 3.0F, 4.0F, 5, 7);

            Assert.Equal(same, value);
            Assert.True(value == same);
            Assert.True(value != different);
            Assert.False(value.Equals("not a keypoint"));
            Assert.Equal(same.GetHashCode(), value.GetHashCode());
        }

        [Fact]
        public void KeyPointInequalityUsesEachOpenCvField()
        {
            KeyPoint value = new KeyPoint(1.0F, 2.0F, 3.0F, 4.0F, 5, 6);

            Assert.NotEqual(new KeyPoint(9.0F, 2.0F, 3.0F, 4.0F, 5, 6), value);
            Assert.NotEqual(new KeyPoint(1.0F, 9.0F, 3.0F, 4.0F, 5, 6), value);
            Assert.NotEqual(new KeyPoint(1.0F, 2.0F, 9.0F, 4.0F, 5, 6), value);
            Assert.NotEqual(new KeyPoint(1.0F, 2.0F, 3.0F, 9.0F, 5, 6), value);
            Assert.NotEqual(new KeyPoint(1.0F, 2.0F, 3.0F, 4.0F, 9, 6), value);
            Assert.NotEqual(new KeyPoint(1.0F, 2.0F, 3.0F, 4.0F, 5, 9), value);
        }
    }
}
