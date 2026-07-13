using System;
using System.Runtime.InteropServices;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using OpenCvSharp.XImgProc;

namespace OpenCvSharp.Tests.XImgProc
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class StereoMatcherBridgeTests
    {
        [Fact]
        public void BridgeValidatesNullAndDisposedMatchers()
        {
            Assert.Throws<ArgumentNullException>(() =>
                XImgProcCv2.CreateDisparityWLSFilter((StereoBM)null!));
            Assert.Throws<ArgumentNullException>(() =>
                XImgProcCv2.CreateDisparityWLSFilter((StereoSGBM)null!));
            Assert.Throws<ArgumentNullException>(() =>
                XImgProcCv2.CreateDisparityWLSFilter((StereoMatcher)null!));
            Assert.Throws<ArgumentNullException>(() =>
                XImgProcCv2.CreateRightMatcher((StereoBM)null!));
            Assert.Throws<ArgumentNullException>(() =>
                XImgProcCv2.CreateRightMatcher((StereoSGBM)null!));
            Assert.Throws<ArgumentNullException>(() =>
                XImgProcCv2.CreateRightMatcher((StereoMatcher)null!));

            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using StereoSGBM source = CreateSgbm();
            StereoMatcher right = XImgProcCv2.CreateRightMatcher(source);
            right.Dispose();

            using var left = new Mat(16, 32, MatType.CV_8UC1, new Scalar(1));
            using var output = new Mat();
            Assert.True(right.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => right.MinDisparity);
            Assert.Throws<ObjectDisposedException>(() => right.MinDisparity = 0);
            Assert.Throws<ObjectDisposedException>(() => right.Compute(left, left, output));
            Assert.Throws<ObjectDisposedException>(() =>
                XImgProcCv2.CreateDisparityWLSFilter(right));
            Assert.Throws<ObjectDisposedException>(() =>
                XImgProcCv2.CreateRightMatcher(right));

            source.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
                XImgProcCv2.CreateDisparityWLSFilter(source));
            Assert.Throws<ObjectDisposedException>(() =>
                XImgProcCv2.CreateRightMatcher(source));
        }

        [Fact]
        public void TypedWlsFactoriesAcceptBmSgbmAndGenericMatchersWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using StereoBM bm = CreateBm();
            using StereoSGBM sgbm = CreateSgbm();
            using StereoMatcher generic = XImgProcCv2.CreateRightMatcher(sgbm);
            using DisparityWLSFilter bmFilter = XImgProcCv2.CreateDisparityWLSFilter(bm);
            using DisparityWLSFilter sgbmFilter = XImgProcCv2.CreateDisparityWLSFilter(sgbm);
            using DisparityWLSFilter genericFilter = XImgProcCv2.CreateDisparityWLSFilter(generic);

            bmFilter.Lambda = 7000.0;
            sgbmFilter.SigmaColor = 1.25;
            genericFilter.LrcThreshold = 19;

            Assert.Equal(7000.0, bmFilter.Lambda);
            Assert.Equal(1.25, sgbmFilter.SigmaColor);
            Assert.Equal(19, genericFilter.LrcThreshold);
            Assert.False(bmFilter.IsDisposed);
            Assert.False(sgbmFilter.IsDisposed);
            Assert.False(genericFilter.IsDisposed);
        }

        [Fact]
        public void BmRightMatcherCopiesSharedConfigurationAndRejectsColorWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using StereoBM leftMatcher = CreateBm();
            leftMatcher.MinDisparity = 2;
            leftMatcher.SpeckleWindowSize = 18;
            leftMatcher.SpeckleRange = 3;
            leftMatcher.Disp12MaxDiff = 4;
            using StereoMatcher rightMatcher = XImgProcCv2.CreateRightMatcher(leftMatcher);

            Assert.Equal(-leftMatcher.MinDisparity - leftMatcher.NumDisparities + 1, rightMatcher.MinDisparity);
            Assert.Equal(leftMatcher.NumDisparities, rightMatcher.NumDisparities);
            Assert.Equal(leftMatcher.BlockSize, rightMatcher.BlockSize);

            rightMatcher.SpeckleWindowSize = 20;
            rightMatcher.SpeckleRange = 5;
            rightMatcher.Disp12MaxDiff = 6;
            Assert.Equal(20, rightMatcher.SpeckleWindowSize);
            Assert.Equal(5, rightMatcher.SpeckleRange);
            Assert.Equal(6, rightMatcher.Disp12MaxDiff);

            CreateGrayPair(48, 96, 4, out Mat left, out Mat right);
            using (left)
            using (right)
            using (Mat disparity = rightMatcher.Compute(right, left))
            {
                Assert.Equal(MatType.CV_16SC1, disparity.Type);
                Assert.Equal(right.Rows, disparity.Rows);
                Assert.Equal(right.Cols, disparity.Cols);
            }

            using var color = new Mat(48, 96, MatType.CV_8UC3, new Scalar(1, 2, 3));
            using var output = new Mat();
            Assert.Throws<ArgumentException>(() => rightMatcher.Compute(color, color, output));
        }

        [Fact]
        public void SgbmRightMatcherSupportsColorAndPreservesInputsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using StereoSGBM leftMatcher = CreateSgbm();
            leftMatcher.SpeckleWindowSize = 22;
            leftMatcher.SpeckleRange = 2;
            leftMatcher.Disp12MaxDiff = -1;
            using StereoMatcher rightMatcher = XImgProcCv2.CreateRightMatcher(leftMatcher);

            Assert.Equal(-leftMatcher.MinDisparity - leftMatcher.NumDisparities + 1, rightMatcher.MinDisparity);
            Assert.Equal(leftMatcher.NumDisparities, rightMatcher.NumDisparities);
            Assert.Equal(leftMatcher.BlockSize, rightMatcher.BlockSize);

            rightMatcher.SpeckleWindowSize = 24;
            rightMatcher.SpeckleRange = 4;
            rightMatcher.Disp12MaxDiff = 5;
            Assert.Equal(24, rightMatcher.SpeckleWindowSize);
            Assert.Equal(4, rightMatcher.SpeckleRange);
            Assert.Equal(5, rightMatcher.Disp12MaxDiff);

            CreateColorPair(40, 96, 4, out Mat left, out Mat right);
            using (left)
            using (right)
            {
                BgrPixel[] leftSnapshot = left.ToArray<BgrPixel>();
                BgrPixel[] rightSnapshot = right.ToArray<BgrPixel>();
                using Mat disparity = rightMatcher.Compute(right, left);

                Assert.Equal(MatType.CV_16SC1, disparity.Type);
                Assert.Equal(right.Rows, disparity.Rows);
                Assert.Equal(right.Cols, disparity.Cols);
                Assert.Equal(leftSnapshot, left.ToArray<BgrPixel>());
                Assert.Equal(rightSnapshot, right.ToArray<BgrPixel>());
            }
        }

        [Fact]
        public void ConfidenceWlsFiltersDeterministicStereoPairWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CreateGrayPair(48, 96, 4, out Mat left, out Mat right);
            using (left)
            using (right)
            using (StereoSGBM leftMatcher = CreateSgbm())
            using (StereoMatcher rightMatcher = XImgProcCv2.CreateRightMatcher(leftMatcher))
            using (DisparityWLSFilter filter = XImgProcCv2.CreateDisparityWLSFilter(leftMatcher))
            using (Mat leftDisparity = leftMatcher.Compute(left, right))
            using (Mat rightDisparity = rightMatcher.Compute(right, left))
            using (var filtered = new Mat())
            {
                filter.Lambda = 8000.0;
                filter.SigmaColor = 1.5;
                filter.Filter(
                    leftDisparity,
                    left,
                    filtered,
                    rightDisparity,
                    new Rect(0, 0, left.Cols, left.Rows),
                    right);

                Assert.Equal(MatType.CV_16SC1, filtered.Type);
                Assert.Equal(left.Rows, filtered.Rows);
                Assert.Equal(left.Cols, filtered.Cols);
                using Mat confidence = filter.GetConfidenceMap();
                Assert.Equal(MatType.CV_32FC1, confidence.Type);
                Assert.Equal(left.Rows, confidence.Rows);
                Assert.Equal(left.Cols, confidence.Cols);
            }
        }

        [Fact]
        public void BridgeResultsOwnIndependentNativePointersWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CreateGrayPair(40, 96, 3, out Mat left, out Mat right);
            using (left)
            using (right)
            {
                StereoSGBM source = CreateSgbm();
                StereoMatcher firstRight = XImgProcCv2.CreateRightMatcher(source);
                DisparityWLSFilter sourceFilter = XImgProcCv2.CreateDisparityWLSFilter(source);
                source.Dispose();

                using (firstRight)
                using (sourceFilter)
                using (StereoMatcher secondRight = XImgProcCv2.CreateRightMatcher(firstRight))
                using (DisparityWLSFilter genericFilter = XImgProcCv2.CreateDisparityWLSFilter(firstRight))
                using (Mat firstDisparity = firstRight.Compute(right, left))
                {
                    firstRight.Dispose();
                    using Mat secondDisparity = secondRight.Compute(left, right);
                    Assert.Equal(MatType.CV_16SC1, firstDisparity.Type);
                    Assert.Equal(MatType.CV_16SC1, secondDisparity.Type);
                    Assert.False(secondRight.IsDisposed);
                    Assert.False(sourceFilter.IsDisposed);
                    Assert.False(genericFilter.IsDisposed);
                }
            }
        }

        private static StereoBM CreateBm()
        {
            return StereoBM.Create(numDisparities: 16, blockSize: 9);
        }

        private static StereoSGBM CreateSgbm()
        {
            const int blockSize = 3;
            return StereoSGBM.Create(
                minDisparity: 0,
                numDisparities: 16,
                blockSize: blockSize,
                p1: 8 * blockSize * blockSize,
                p2: 32 * blockSize * blockSize,
                disp12MaxDiff: -1,
                preFilterCap: 31,
                uniquenessRatio: 0,
                speckleWindowSize: 0,
                speckleRange: 0,
                mode: StereoSGBMMode.SGBM);
        }

        private static void CreateGrayPair(int rows, int cols, int shift, out Mat left, out Mat right)
        {
            left = new Mat(rows, cols, MatType.CV_8UC1);
            right = new Mat(rows, cols, MatType.CV_8UC1);
            for (int y = 0; y < rows; ++y)
            {
                for (int x = 0; x < cols; ++x)
                {
                    left.SetValue(y * cols + x, Pattern(y, x));
                    right.SetValue(y * cols + x, x + shift < cols ? Pattern(y, x + shift) : (byte)0);
                }
            }
        }

        private static void CreateColorPair(int rows, int cols, int shift, out Mat left, out Mat right)
        {
            left = new Mat(rows, cols, MatType.CV_8UC3);
            right = new Mat(rows, cols, MatType.CV_8UC3);
            for (int y = 0; y < rows; ++y)
            {
                for (int x = 0; x < cols; ++x)
                {
                    left.SetValue(y * cols + x, ColorPattern(y, x));
                    right.SetValue(
                        y * cols + x,
                        x + shift < cols ? ColorPattern(y, x + shift) : default(BgrPixel));
                }
            }
        }

        private static byte Pattern(int y, int x)
        {
            return (byte)((x * 37 + y * 19 + x * y * 3 + (x ^ y) * 11) & 255);
        }

        private static BgrPixel ColorPattern(int y, int x)
        {
            byte value = Pattern(y, x);
            return new BgrPixel(
                value,
                (byte)((value * 3 + x * 5) & 255),
                (byte)((value * 7 + y * 9) & 255));
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private readonly struct BgrPixel : IEquatable<BgrPixel>
        {
            public BgrPixel(byte blue, byte green, byte red)
            {
                Blue = blue;
                Green = green;
                Red = red;
            }

            public byte Blue { get; }

            public byte Green { get; }

            public byte Red { get; }

            public bool Equals(BgrPixel other)
            {
                return Blue == other.Blue && Green == other.Green && Red == other.Red;
            }

            public override bool Equals(object? obj)
            {
                return obj is BgrPixel other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Blue | (Green << 8) | (Red << 16);
            }
        }
    }
}
