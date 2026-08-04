using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Features2D;

namespace JYPPX.OpenCvSharp.Tests.Features2D
{
    public class DMatchTests
    {
        [Fact]
        public void DMatchReportsOpenCvCompatibleFields()
        {
            DMatch match = new DMatch(3, 7, 2, 1.25F);

            Assert.Equal(3, match.QueryIdx);
            Assert.Equal(7, match.TrainIdx);
            Assert.Equal(2, match.ImgIdx);
            Assert.Equal(1.25F, match.Distance);
            Assert.Equal(3, match[0]);
            Assert.Equal(7, match[1]);
            Assert.Equal(2, match[2]);
            Assert.Equal(1.25F, match[3]);
            Assert.Equal("{QueryIdx=3,TrainIdx=7,ImgIdx=2,Distance=1.25}", match.ToString());
        }

        [Fact]
        public void DMatchThreeArgumentConstructorUsesDefaultImageIndex()
        {
            DMatch match = new DMatch(3, 7, 1.25F);

            Assert.Equal(3, match.QueryIdx);
            Assert.Equal(7, match.TrainIdx);
            Assert.Equal(0, match.ImgIdx);
            Assert.Equal(1.25F, match.Distance);
        }

        [Fact]
        public void DMatchToStringFormatsDistanceInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                string text = new DMatch(3, 7, 2, 1.25F).ToString();

                Assert.Contains("Distance=1.25", text, StringComparison.Ordinal);
                Assert.DoesNotContain("Distance=1,25", text, StringComparison.Ordinal);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void DMatchEqualityAndComparisonUseOpenCvFields()
        {
            DMatch value = new DMatch(3, 7, 2, 1.25F);
            DMatch same = new DMatch(3, 7, 2, 1.25F);
            DMatch different = new DMatch(3, 7, 2, 2.25F);

            Assert.Equal(same, value);
            Assert.True(value == same);
            Assert.True(value != different);
            Assert.True(value.CompareTo(different) < 0);
            Assert.False(value.Equals("not a match"));
            Assert.Equal(same.GetHashCode(), value.GetHashCode());
        }

        [Fact]
        public void DMatchInequalityUsesEachOpenCvField()
        {
            DMatch value = new DMatch(3, 7, 2, 1.25F);

            Assert.NotEqual(new DMatch(9, 7, 2, 1.25F), value);
            Assert.NotEqual(new DMatch(3, 9, 2, 1.25F), value);
            Assert.NotEqual(new DMatch(3, 7, 9, 1.25F), value);
            Assert.NotEqual(new DMatch(3, 7, 2, 9.25F), value);
        }

        [Fact]
        public void DMatchComparisonUsesDistanceOnly()
        {
            DMatch value = new DMatch(3, 7, 2, 1.25F);
            DMatch sameDistance = new DMatch(30, 70, 20, 1.25F);
            DMatch farther = new DMatch(0, 0, 0, 2.25F);

            Assert.Equal(0, value.CompareTo(sameDistance));
            Assert.True(value.CompareTo(farther) < 0);
            Assert.True(farther.CompareTo(value) > 0);
        }

        [Fact]
        public void DMatchIndexerRejectsInvalidIndex()
        {
            DMatch match = new DMatch(3, 7, 2, 1.25F);

            Assert.Throws<IndexOutOfRangeException>(() => match[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => match[4]);
        }

        [Fact]
        public void Features2DEnumsMatchOpenCvValues()
        {
            Assert.Equal(0, (int)OrbScoreType.HarrisScore);
            Assert.Equal(1, (int)OrbScoreType.FastScore);
            Assert.Equal(0, (int)FastFeatureDetectorType.Type5_8);
            Assert.Equal(1, (int)FastFeatureDetectorType.Type7_12);
            Assert.Equal(2, (int)FastFeatureDetectorType.Type9_16);
            Assert.Equal(1, (int)DescriptorMatcherType.FlannBased);
            Assert.Equal(2, (int)DescriptorMatcherType.BruteForce);
            Assert.Equal(3, (int)DescriptorMatcherType.BruteForceL1);
            Assert.Equal(4, (int)DescriptorMatcherType.BruteForceHamming);
            Assert.Equal(5, (int)DescriptorMatcherType.BruteForceHammingLut);
            Assert.Equal(6, (int)DescriptorMatcherType.BruteForceSL2);
            Assert.Equal(0, (int)DrawMatchesFlags.Default);
            Assert.Equal(1, (int)DrawMatchesFlags.DrawOverOutImg);
            Assert.Equal(2, (int)DrawMatchesFlags.NotDrawSinglePoints);
            Assert.Equal(4, (int)DrawMatchesFlags.DrawRichKeypoints);
        }
    }
}
