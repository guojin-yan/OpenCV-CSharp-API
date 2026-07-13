using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Tests.Core
{
    public class MatInteropTests
    {
        [Fact]
        public void MatReportsShapeWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat mat = new Mat(2, 3, MatType.CV_8UC1))
            {
                Assert.False(mat.Empty);
                Assert.Equal(2, mat.Rows);
                Assert.Equal(3, mat.Cols);
                Assert.Equal(1, mat.Channels);
                Assert.Equal(MatType.CV_8UC1, mat.Type);
                Assert.Equal((UIntPtr)6, mat.Total);
                Assert.Equal((UIntPtr)1, mat.ElemSize);
                Assert.True(mat.Step.ToUInt64() >= 3);
                Assert.NotEqual(IntPtr.Zero, mat.Data);
                Assert.True(mat.IsContinuous);
                Assert.Equal(6, mat.ByteLength);

                byte[] source = new byte[] { 1, 2, 3, 4, 5, 6 };
                byte[] destination = new byte[6];

                mat.CopyFrom(source);
                mat.CopyTo(destination);

                Assert.Equal(source, destination);
#if NETCOREAPP3_1_OR_GREATER
                Assert.Equal(6, mat.AsByteSpan().Length);

                Span<byte> spanDestination = stackalloc byte[6];
                mat.CopyTo(spanDestination);
                Assert.True(source.AsSpan().SequenceEqual(spanDestination));
#endif
            }
        }

        [Fact]
        public void MatFactoriesAndScalarFillCreateExpectedPixelsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat scalar = new Mat(2, 2, MatType.CV_8UC1, new Scalar(7)))
            using (Mat zeros = Mat.Zeros(2, 2, MatType.CV_8UC1))
            using (Mat ones = Mat.Ones(2, 2, MatType.CV_8UC1))
            using (Mat eye = Mat.Eye(3, 3, MatType.CV_8UC1))
            {
                Assert.Equal(new byte[] { 7, 7, 7, 7 }, scalar.ToBytes());
                Assert.Equal(new byte[] { 0, 0, 0, 0 }, zeros.ToBytes());
                Assert.Equal(new byte[] { 1, 1, 1, 1 }, ones.ToBytes());
                Assert.Equal(new byte[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 }, eye.ToBytes());

                scalar.SetTo(new Scalar(3));
                Assert.Equal(new byte[] { 3, 3, 3, 3 }, scalar.ToBytes());
            }
        }

        [Fact]
        public void CloneAndCopyToUseIndependentStorageWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat source = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat destination = new Mat())
            {
                source.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6 });

                using (Mat clone = source.Clone())
                {
                    clone.SetTo(new Scalar(9));

                    Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6 }, source.ToBytes());
                    Assert.Equal(new byte[] { 9, 9, 9, 9, 9, 9 }, clone.ToBytes());
                }

                source.CopyTo(destination);
                Assert.Equal(2, destination.Rows);
                Assert.Equal(3, destination.Cols);
                Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6 }, destination.ToBytes());
            }
        }

        [Fact]
        public void ConvertToReturningHelperMatchesDestinationOverloadWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat source = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat destination = new Mat())
            {
                source.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6 });

                source.ConvertTo(destination, MatType.CV_32F, 2.0, 1.0);
                using (Mat converted = source.ConvertTo(MatType.CV_32F, 2.0, 1.0))
                {
                    Assert.Equal(destination.Rows, converted.Rows);
                    Assert.Equal(destination.Cols, converted.Cols);
                    Assert.Equal(destination.Type, converted.Type);
                    Assert.Equal(destination.ToBytes(), converted.ToBytes());
                }
            }
        }

        [Fact]
        public void SubMatRowRangeAndColRangeShareStorageWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat source = new Mat(3, 4, MatType.CV_8UC1))
            {
                source.CopyFrom(new byte[]
                {
                    1, 2, 3, 4,
                    5, 6, 7, 8,
                    9, 10, 11, 12
                });

                using (Mat roi = source.SubMat(new Rect(1, 1, 2, 2)))
                using (Mat row = source.Row(1))
                using (Mat col = source.Col(2))
                {
                    Assert.Equal(2, roi.Rows);
                    Assert.Equal(2, roi.Cols);
                    Assert.True(roi.IsSubmatrix);
                    Assert.False(roi.IsContinuous);
                    Assert.True(row.IsSubmatrix);
                    Assert.True(col.IsSubmatrix);

                    roi.SetTo(new Scalar(99));

                    Assert.Equal(new byte[]
                    {
                        1, 2, 3, 4,
                        5, 99, 99, 8,
                        9, 99, 99, 12
                    }, source.ToBytes());
                }
            }
        }

        [Fact]
        public void ReshapeChangesRowsAndChannelsWithoutCopyWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat source = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat reshaped = source.Reshape(3, 2))
            {
                source.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6 });

                Assert.Equal(2, reshaped.Rows);
                Assert.Equal(1, reshaped.Cols);
                Assert.Equal(3, reshaped.Channels);
                Assert.Equal(MatType.CV_8UC3, reshaped.Type);
                Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6 }, reshaped.ToBytes());
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        [Fact]
        public void TypedSpanPathsReadAndWriteContinuousMatricesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat mat = new Mat(2, 3, MatType.CV_8UC1))
            {
                Span<byte> bytes = mat.AsSpan<byte>();
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(i + 1);
                }

                Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6 }, mat.ToBytes());
                Assert.True(mat.TryGetSpan<byte>(out Span<byte> trySpan));
                Assert.Equal(6, trySpan.Length);
                Assert.Equal(4, mat.GetValue<byte>(3));

                mat.SetValue(3, (byte)42);
                Assert.Equal(42, mat.ToBytes()[3]);
            }
        }

        [Fact]
        public void TryGetSpanRejectsNonContinuousMatricesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat source = new Mat(3, 4, MatType.CV_8UC1))
            using (Mat roi = source.SubMat(new Rect(1, 1, 2, 2)))
            {
                Assert.False(roi.IsContinuous);
                Assert.False(roi.TryGetByteSpan(out Span<byte> span));
                Assert.True(span.IsEmpty);
                Assert.Throws<OpenCvException>(() => roi.AsByteSpan());
            }
        }
#endif

        [Fact]
        public void InvalidRangesAndDisposedMatThrowStableManagedExceptionsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat source = new Mat(3, 4, MatType.CV_8UC1))
            {
                Assert.Throws<OpenCvException>(() => source.SubMat(new Rect(-1, 0, 1, 1)));
                Assert.Throws<OpenCvException>(() => source.RowRange(2, 1));
                Assert.Throws<OpenCvException>(() => source.ColRange(0, 5));
            }

            Mat disposed = new Mat(1, 1, MatType.CV_8UC1);
            disposed.Dispose();
            Assert.True(disposed.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => disposed.CopyTo(new byte[1]));
            Assert.Throws<ObjectDisposedException>(() => disposed.ConvertTo(MatType.CV_32F));
        }

    }
}
