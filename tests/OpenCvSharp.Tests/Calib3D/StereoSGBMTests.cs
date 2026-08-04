using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class StereoSGBMTests
    {
        [Fact]
        public void ConstantsModesAndInvalidModeValidationMatchOpenCv()
        {
            Assert.Equal(4, StereoSGBM.DispShift);
            Assert.Equal(16, StereoSGBM.DispScale);
            Assert.Equal(0, (int)StereoSGBMMode.SGBM);
            Assert.Equal(1, (int)StereoSGBMMode.HH);
            Assert.Equal(2, (int)StereoSGBMMode.SGBM3Way);
            Assert.Equal(3, (int)StereoSGBMMode.HH4);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                StereoSGBM.Create(mode: (StereoSGBMMode)99));
        }

        [Fact]
        public void PropertiesRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using StereoSGBM matcher = CreateMatcher();
            matcher.MinDisparity = -2;
            matcher.NumDisparities = 32;
            matcher.BlockSize = 5;
            matcher.SpeckleWindowSize = 24;
            matcher.SpeckleRange = 2;
            matcher.Disp12MaxDiff = -1;
            matcher.PreFilterCap = 31;
            matcher.UniquenessRatio = 7;
            matcher.P1 = 200;
            matcher.P2 = 800;
            matcher.Mode = StereoSGBMMode.HH4;

            Assert.Equal(-2, matcher.MinDisparity);
            Assert.Equal(32, matcher.NumDisparities);
            Assert.Equal(5, matcher.BlockSize);
            Assert.Equal(24, matcher.SpeckleWindowSize);
            Assert.Equal(2, matcher.SpeckleRange);
            Assert.Equal(-1, matcher.Disp12MaxDiff);
            Assert.Equal(31, matcher.PreFilterCap);
            Assert.Equal(7, matcher.UniquenessRatio);
            Assert.Equal(200, matcher.P1);
            Assert.Equal(800, matcher.P2);
            Assert.Equal(StereoSGBMMode.HH4, matcher.Mode);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                matcher.Mode = (StereoSGBMMode)99);
        }

        [Fact]
        public void ManagedValidationAndDisposedStateAreEnforcedWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using StereoSGBM matcher = CreateMatcher();
            using var empty = new Mat();
            using var left = new Mat(8, 32, MatType.CV_8UC1, new Scalar(1));
            using var right = new Mat(8, 32, MatType.CV_8UC1, new Scalar(1));
            using var mismatchedSize = new Mat(7, 32, MatType.CV_8UC1, new Scalar(1));
            using var mismatchedType = new Mat(8, 32, MatType.CV_8UC3, new Scalar(1, 1, 1));
            using var unsupported = new Mat(8, 32, MatType.CV_16SC1, new Scalar(1));
            using var output = new Mat();

            Assert.Throws<ArgumentNullException>(() => matcher.Compute(null!, right, output));
            Assert.Throws<ArgumentNullException>(() => matcher.Compute(left, null!, output));
            Assert.Throws<ArgumentNullException>(() => matcher.Compute(left, right, null!));
            Assert.Throws<ArgumentException>(() => matcher.Compute(empty, right, output));
            Assert.Throws<ArgumentException>(() => matcher.Compute(left, empty, output));
            Assert.Throws<ArgumentException>(() => matcher.Compute(unsupported, right, output));
            Assert.Throws<ArgumentException>(() => matcher.Compute(left, mismatchedSize, output));
            Assert.Throws<ArgumentException>(() => matcher.Compute(left, mismatchedType, output));
            Assert.Throws<ArgumentException>(() => matcher.Compute(left, right, left));
            Assert.Throws<ArgumentException>(() => matcher.Compute(left, right, right));

            using var disposedOutput = new Mat();
            disposedOutput.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
                matcher.Compute(left, right, disposedOutput));

            matcher.Dispose();
            Assert.True(matcher.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => matcher.Compute(left, right, output));
            Assert.Throws<ObjectDisposedException>(() => matcher.Compute(left, right));
            Assert.Throws<ObjectDisposedException>(() => matcher.MinDisparity);
            Assert.Throws<ObjectDisposedException>(() => matcher.MinDisparity = 0);
            Assert.Throws<ObjectDisposedException>(() => matcher.Mode);
            Assert.Throws<ObjectDisposedException>(() => matcher.Mode = StereoSGBMMode.SGBM);
        }

        [Fact]
        public void GrayscaleComputeMatchesOwnedOutputAndKnownShiftWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CreateGrayPair(48, 96, 4, out Mat left, out Mat right);
            using (left)
            using (right)
            using (StereoSGBM matcher = CreateMatcher())
            using (var callerOwned = new Mat())
            {
                byte[] leftSnapshot = left.ToArray<byte>();
                byte[] rightSnapshot = right.ToArray<byte>();

                matcher.Compute(left, right, callerOwned);
                using Mat owned = matcher.Compute(left, right);

                Assert.Equal(48, callerOwned.Rows);
                Assert.Equal(96, callerOwned.Cols);
                Assert.Equal(MatType.CV_16SC1, callerOwned.Type);
                Assert.Equal(callerOwned.ToArray<short>(), owned.ToArray<short>());
                Assert.Equal(leftSnapshot, left.ToArray<byte>());
                Assert.Equal(rightSnapshot, right.ToArray<byte>());

                short median = GetValidMedian(callerOwned, 24, 88, 6, 42);
                Assert.InRange(median, (short)(3 * StereoSGBM.DispScale), (short)(5 * StereoSGBM.DispScale));
            }
        }

        [Fact]
        public void ColorComputeProducesFixedPointDisparityAndPreservesInputsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CreateColorPair(40, 96, 4, out Mat left, out Mat right);
            using (left)
            using (right)
            using (StereoSGBM matcher = CreateMatcher(channelCount: 3))
            using (var disparity = new Mat())
            {
                BgrPixel[] leftSnapshot = left.ToArray<BgrPixel>();
                BgrPixel[] rightSnapshot = right.ToArray<BgrPixel>();

                matcher.Compute(left, right, disparity);

                Assert.Equal(40, disparity.Rows);
                Assert.Equal(96, disparity.Cols);
                Assert.Equal(MatType.CV_16SC1, disparity.Type);
                Assert.Equal(leftSnapshot, left.ToArray<BgrPixel>());
                Assert.Equal(rightSnapshot, right.ToArray<BgrPixel>());
                short median = GetValidMedian(disparity, 24, 88, 5, 35);
                Assert.InRange(median, (short)(3 * StereoSGBM.DispScale), (short)(5 * StereoSGBM.DispScale));
            }
        }

        [Fact]
        public void AllModesComputeOnDeterministicPairWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CreateGrayPair(40, 80, 3, out Mat left, out Mat right);
            using (left)
            using (right)
            {
                foreach (StereoSGBMMode mode in new[]
                {
                    StereoSGBMMode.SGBM,
                    StereoSGBMMode.HH,
                    StereoSGBMMode.SGBM3Way,
                    StereoSGBMMode.HH4
                })
                {
                    using StereoSGBM matcher = CreateMatcher(mode: mode);
                    using Mat disparity = matcher.Compute(left, right);
                    Assert.Equal(MatType.CV_16SC1, disparity.Type);
                    Assert.Equal(left.Rows, disparity.Rows);
                    Assert.Equal(left.Cols, disparity.Cols);
                    Assert.Equal(mode, matcher.Mode);
                }
            }
        }

        private static StereoSGBM CreateMatcher(
            int channelCount = 1,
            StereoSGBMMode mode = StereoSGBMMode.SGBM)
        {
            const int blockSize = 3;
            int p1 = 8 * channelCount * blockSize * blockSize;
            int p2 = 32 * channelCount * blockSize * blockSize;
            return StereoSGBM.Create(
                minDisparity: 0,
                numDisparities: 16,
                blockSize: blockSize,
                p1: p1,
                p2: p2,
                disp12MaxDiff: -1,
                preFilterCap: 31,
                uniquenessRatio: 0,
                speckleWindowSize: 0,
                speckleRange: 0,
                mode: mode);
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
                    byte rightValue = x + shift < cols ? Pattern(y, x + shift) : (byte)0;
                    right.SetValue(y * cols + x, rightValue);
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
                    BgrPixel rightValue = x + shift < cols
                        ? ColorPattern(y, x + shift)
                        : default;
                    right.SetValue(y * cols + x, rightValue);
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

        private static short GetValidMedian(
            Mat disparity,
            int startX,
            int endX,
            int startY,
            int endY)
        {
            short[] values = disparity.ToArray<short>();
            var valid = new List<short>();
            for (int y = startY; y < endY; ++y)
            {
                for (int x = startX; x < endX; ++x)
                {
                    short value = values[y * disparity.Cols + x];
                    if (value >= 0)
                    {
                        valid.Add(value);
                    }
                }
            }

            Assert.NotEmpty(valid);
            valid.Sort();
            return valid[valid.Count / 2];
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
