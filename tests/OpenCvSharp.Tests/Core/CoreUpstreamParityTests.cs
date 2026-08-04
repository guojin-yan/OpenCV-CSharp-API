using System;
using JYPPX.OpenCvSharp.Core;
using Cv2 = JYPPX.OpenCvSharp.Core.Cv2;
using BorderTypes = JYPPX.OpenCvSharp.ImgProc.BorderTypes;

namespace JYPPX.OpenCvSharp.Tests.Core
{
    public class CoreUpstreamParityTests
    {
        [Fact]
        public void BorderAndPresenceOperationsPreserveShapeAndValues()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using var src = new Mat(2, 2, MatType.CV_8UC1);
            src.CopyFrom(new byte[] { 0, 2, 3, 0 });

            Assert.Equal(1, Cv2.BorderInterpolate(-1, 3, BorderTypes.Reflect101));
            Assert.True(Cv2.HasNonZero(src));
            using Mat border = Cv2.CopyMakeBorder(src, 1, 1, 1, 1, BorderTypes.Constant, new Scalar(9));
            Assert.Equal(new Size(4, 4), border.Size);
            Assert.Equal(new byte[]
            {
                9, 9, 9, 9,
                9, 0, 2, 9,
                9, 3, 0, 9,
                9, 9, 9, 9
            }, border.ToArray<byte>());

            using Mat points = Cv2.FindNonZero(src);
            Assert.Equal((ulong)2, points.Total.ToUInt64());
            Assert.Equal(MatType.CV_32SC2, points.Type);
            Assert.Equal(new[] { 1, 0, 0, 1 }, points.ToArray<int>());
        }

        [Fact]
        public void ReductionsSortingAndMaskedCopyAreDeterministic()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using var src = new Mat(2, 3, MatType.CV_32SC1);
            src.CopyFrom(new[] { 3, 1, 1, 2, 4, 0 });
            using Mat minFirst = Cv2.ReduceArgMin(src, 1);
            using Mat minLast = Cv2.ReduceArgMin(src, 1, true);
            using Mat max = Cv2.ReduceArgMax(src, 0);
            Assert.Equal(new[] { 1, 2 }, minFirst.ToArray<int>());
            Assert.Equal(new[] { 2, 2 }, minLast.ToArray<int>());
            Assert.Equal(new[] { 0, 1, 0 }, max.ToArray<int>());

            using Mat sorted = Cv2.Sort(src, SortFlags.EveryRow | SortFlags.Descending);
            using Mat indices = Cv2.SortIdx(src, SortFlags.EveryRow | SortFlags.Ascending);
            Assert.Equal(new[] { 3, 1, 1, 4, 2, 0 }, sorted.ToArray<int>());
            Assert.Equal(new[] { 1, 2, 0, 2, 0, 1 }, indices.ToArray<int>());

            using var mask = new Mat(2, 3, MatType.CV_8UC1);
            mask.CopyFrom(new byte[] { 255, 0, 255, 0, 255, 0 });
            using var copied = new Mat();
            Cv2.CopyTo(src, copied, mask);
            Assert.Equal(new[] { 3, 0, 1, 0, 4, 0 }, copied.ToArray<int>());
        }

        [Fact]
        public void NewTransformsHandleContinuousAndRoiInputsByContract()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using var src = new Mat(2, 3, MatType.CV_8UC1);
            src.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6 });
            using Mat flipped = Cv2.FlipND(src, -1);
            Assert.Equal(new byte[] { 3, 2, 1, 6, 5, 4 }, flipped.ToArray<byte>());

            using Mat transposed = Cv2.TransposeND(src, new[] { 1, 0 });
            Assert.Equal(new Size(2, 3), transposed.Size);
            Assert.Equal(new byte[] { 1, 4, 2, 5, 3, 6 }, transposed.ToArray<byte>());

            using var scalarRow = new Mat(1, 3, MatType.CV_32SC1);
            scalarRow.CopyFrom(new[] { 7, 8, 9 });
            using var shape = new Mat(1, 2, MatType.CV_32SC1);
            shape.CopyFrom(new[] { 2, 3 });
            using Mat broadcast = Cv2.Broadcast(scalarRow, shape);
            Assert.Equal(new Size(3, 2), broadcast.Size);
            Assert.Equal(new[] { 7, 8, 9, 7, 8, 9 }, broadcast.ToArray<int>());

            using var parent = new Mat(3, 4, MatType.CV_8UC1);
            parent.CopyFrom(new byte[] { 0, 1, 0, 0, 2, 0, 3, 0, 0, 4, 0, 0 });
            using Mat roi = parent.SubMat(new Rect(0, 0, 3, 3));
            Assert.False(roi.IsContinuous);
            using Mat roiPoints = Cv2.FindNonZero(roi);
            Assert.Equal((ulong)4, roiPoints.Total.ToUInt64());
        }

        [Fact]
        public void NumericChecksExposeValuesAndFailurePositions()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using var first = new Mat(1, 3, MatType.CV_32FC1);
            using var second = new Mat(1, 3, MatType.CV_32FC1);
            first.CopyFrom(new[] { 1.0f, 2.0f, 3.0f });
            second.CopyFrom(new[] { 1.0f, 2.0f, 4.0f });
            Assert.True(Cv2.Psnr(first, first) > 300.0);
            Assert.True(Cv2.Psnr(first, second) > 0.0);

            CheckRangeResult valid = Cv2.CheckRange(first, 0.0, 4.0);
            CheckRangeResult invalid = Cv2.CheckRange(second, 0.0, 4.0);
            Assert.True(valid.IsValid);
            Assert.Equal(new Point(-1, -1), valid.Position);
            Assert.False(invalid.IsValid);
            Assert.Equal(new Point(2, 0), invalid.Position);

            using var values = new Mat(1, 4, MatType.CV_64FC1);
            values.CopyFrom(new[] { 1.0, double.NaN, double.PositiveInfinity, -2.0 });
            using Mat finite = Cv2.FiniteMask(values);
            Assert.Equal(MatType.CV_8UC1, finite.Type);
            Assert.Equal(new byte[] { 255, 0, 0, 255 }, finite.ToArray<byte>());
        }

        [Fact]
        public void InvalidShapesDepthsAxesAndDisposedInputsFailBeforeUnsafeUse()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using var color = new Mat(2, 2, MatType.CV_8UC3);
            using var scalar = new Mat(2, 2, MatType.CV_8UC1);
            using var wrongShape = new Mat(1, 2, MatType.CV_32FC1);
            using var output = new Mat();
            Assert.Throws<ArgumentException>(() => Cv2.HasNonZero(color));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.ReduceArgMin(scalar, output, 2));
            Assert.Throws<ArgumentException>(() => Cv2.Broadcast(scalar, wrongShape, output));
            Assert.Throws<ArgumentException>(() => Cv2.FiniteMask(scalar, output));
            Assert.Throws<ArgumentException>(() => Cv2.TransposeND(scalar, new[] { 0, 0 }, output));
            Assert.Throws<ArgumentException>(() => Cv2.TransposeND(scalar, new[] { 1, 0 }, scalar));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.Sort(scalar, output, (SortFlags)2));
            Assert.Throws<ArgumentException>(() => Cv2.CheckRange(scalar, 2.0, 2.0));

            var disposed = new Mat(1, 1, MatType.CV_8UC1);
            disposed.Dispose();
            Assert.Throws<ObjectDisposedException>(() => Cv2.HasNonZero(disposed));
        }

        [Fact]
        public void RepeatedOwnedResultsCanBeDisposedIndependently()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using var src = new Mat(1, 3, MatType.CV_8UC1);
            src.CopyFrom(new byte[] { 0, 1, 2 });
            for (int i = 0; i < 32; i++)
            {
                using Mat result = Cv2.FindNonZero(src);
                Assert.Equal((ulong)2, result.Total.ToUInt64());
            }
        }
    }
}
