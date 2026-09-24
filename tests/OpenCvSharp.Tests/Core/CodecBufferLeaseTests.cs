using System;
using JYPPX.OpenCvSharp.Internal;

namespace JYPPX.OpenCvSharp.Tests.Core
{
    public sealed class CodecBufferLeaseTests
    {
        [Fact]
        public void PinValidatesLayoutAndExposesCheckedRows()
        {
            byte[] owner = new byte[24];
            int releaseCount = 0;

            using (CodecBufferLease lease = CodecBufferLease.Pin(owner, 2, 3, 6, 4, () => releaseCount++))
            {
                Assert.NotEqual(IntPtr.Zero, lease.Data);
                Assert.Equal(22, lease.LengthBytes);
                Assert.Equal(6, lease.StepBytes);
                Assert.Equal(3, lease.Rows);
                Assert.Equal(4, lease.RowPayloadBytes);
                Assert.Equal(IntPtr.Add(lease.Data, 6), lease.GetRowPointer(1));
                Assert.Equal(IntPtr.Add(lease.Data, 12), lease.GetRowPointer(2));
                Assert.Throws<ArgumentOutOfRangeException>(() => { _ = lease.GetRowPointer(-1); });
                Assert.Throws<ArgumentOutOfRangeException>(() => { _ = lease.GetRowPointer(3); });
            }

            Assert.Equal(1, releaseCount);
        }

        [Fact]
        public void InvalidOwnerAndLayoutFailBeforePinning()
        {
            Assert.Throws<ArgumentNullException>(() => CodecBufferLease.Pin(null!, 0, 1, 1, 1));
            Assert.Throws<ArgumentException>(() => CodecBufferLease.Pin(Array.Empty<byte>(), 0, 1, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => CodecBufferLease.Pin(new byte[4], -1, 1, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => CodecBufferLease.Pin(new byte[4], 4, 1, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => CodecBufferLease.Pin(new byte[4], 0, 0, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => CodecBufferLease.Pin(new byte[4], 0, -1, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => CodecBufferLease.Pin(new byte[4], 0, 1, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => CodecBufferLease.Pin(new byte[4], 0, 1, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => CodecBufferLease.Pin(new byte[4], 0, 1, 1, 2));
            Assert.Throws<ArgumentException>(() => CodecBufferLease.Pin(new byte[8], 0, 2, 5, 4));
            Assert.Throws<OverflowException>(() => CodecBufferLease.Pin(new byte[1], 0, int.MaxValue, long.MaxValue, 1));
        }

        [Fact]
        public void OperationGuardDefersReleaseAndDisposeIsIdempotent()
        {
            int releaseCount = 0;
            CodecBufferLease lease = CodecBufferLease.Pin(new byte[8], 0, 1, 4, 4, () => releaseCount++);

            using (lease.EnterOperation())
            {
                lease.Dispose();
                Assert.Equal(0, releaseCount);
                Assert.Throws<ObjectDisposedException>(() => lease.EnterOperation());
            }

            lease.Dispose();
            Assert.Equal(1, releaseCount);
            Assert.Throws<ObjectDisposedException>(() => { _ = lease.Data; });
        }

        [Fact]
        public void ReleaseCallbackRunsOnceAndExceptionsAreCaptured()
        {
            int releaseCount = 0;
            CodecBufferLease lease = CodecBufferLease.Pin(
                new byte[8],
                0,
                1,
                4,
                4,
                () =>
                {
                    releaseCount++;
                    throw new InvalidOperationException("release fixture");
                });

            lease.Dispose();
            lease.Dispose();

            Assert.Equal(1, releaseCount);
            Assert.IsType<InvalidOperationException>(lease.ReleaseException);
        }

        [Fact]
        public void NativePointerLeaseRequiresOwnerCallbackAndReleasesOnce()
        {
            int releaseCount = 0;
            CodecBufferLease lease = CodecBufferLease.FromNativePointer(
                new IntPtr(1),
                16,
                2,
                8,
                4,
                () => releaseCount++);

            Assert.Equal(new IntPtr(1), lease.Data);
            Assert.Equal(16, lease.LengthBytes);
            lease.Dispose();
            lease.Dispose();
            Assert.Equal(1, releaseCount);

            Assert.Throws<ArgumentNullException>(() => CodecBufferLease.FromNativePointer(IntPtr.Zero, 16, 1, 4, 4, () => { }));
            Assert.Throws<ArgumentNullException>(() => CodecBufferLease.FromNativePointer(new IntPtr(1), 16, 1, 4, 4, null!));
            Assert.Throws<ArgumentException>(() => CodecBufferLease.FromNativePointer(new IntPtr(1), 4, 2, 4, 4, () => { }));
        }

        [Fact]
        public void PinAndReleaseStressKeepsExactlyOneCallbackPerLease()
        {
            const int iterations = 256;
            int releaseCount = 0;

            for (int index = 0; index < iterations; index++)
            {
                CodecBufferLease lease = CodecBufferLease.Pin(new byte[32], 4, 2, 8, 4, () => releaseCount++);
                using (lease.EnterOperation())
                {
                    Assert.NotEqual(IntPtr.Zero, lease.GetRowPointer(1));
                }
                lease.Dispose();
            }

            Assert.Equal(iterations, releaseCount);
        }
    }
}
