using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Tests.Core
{
    public class MatViewTests
    {
#if NETCOREAPP3_1_OR_GREATER
        [Fact]
        public void TypedViewReadsWritesAndCopiesRegisteredPixelsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Mat mat = new Mat(2, 3, MatType.CV_8UC1))
            using (MatView<byte> view = mat.AsView<byte>())
            {
                Assert.Equal(2, view.Rows);
                Assert.Equal(3, view.Columns);
                Assert.Equal(MatType.CV_8UC1, view.Descriptor.MatType);
                Assert.True(view.IsContinuous);

                Span<byte> pixels = view.AsSpan();
                for (int i = 0; i < pixels.Length; i++) pixels[i] = (byte)(i + 1);
                Assert.Equal((byte)4, view.GetValue(1, 0));

                view.SetValue(0, 2, 42);
                Assert.Equal((byte)42, view.AsReadOnlyRowSpan(0)[2]);
                Assert.True(view.TryGetSpan(out Span<byte> trySpan));
                Assert.Equal(6, trySpan.Length);
                Assert.Equal(new byte[] { 1, 2, 42, 4, 5, 6 }, view.ToArray());

                byte[] copied = new byte[6];
                view.CopyTo(copied);
                Assert.Equal(new byte[] { 1, 2, 42, 4, 5, 6 }, copied);
                view.CopyFrom(new byte[] { 9, 8, 7, 6, 5, 4 });
                Assert.Equal(new byte[] { 9, 8, 7, 6, 5, 4 }, mat.ToBytes());
            }
        }

        [Fact]
        public void TypedViewSupportsRowsOfNonContinuousRoiWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Mat source = new Mat(3, 4, MatType.CV_8UC3))
            using (Mat roi = source.SubMat(new Rect(1, 0, 2, 3)))
            using (MatView<Vec3b> view = roi.AsView<Vec3b>())
            {
                source.CopyFrom(new byte[source.ByteLength]);
                Assert.False(view.IsContinuous);
                Assert.False(view.TryGetSpan(out Span<Vec3b> span));
                Assert.True(span.IsEmpty);
                Assert.Throws<OpenCvException>(() => view.AsSpan());

                view.AsRowSpan(1)[0] = new Vec3b(10, 20, 30);
                Assert.Equal(new Vec3b(10, 20, 30), view.GetValue(1, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => view.AsRowSpan(3));
                Assert.Throws<ArgumentOutOfRangeException>(() => view.GetValue(0, 2));
            }
        }

        [Fact]
        public void TypedViewRejectsUnregisteredAndMismatchedTypesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Mat color = new Mat(1, 1, MatType.CV_8UC3))
            {
                Assert.Throws<OpenCvException>(() => color.AsView<byte>());
                Assert.Throws<NotSupportedException>(() => color.AsView<UnknownPixel>());
            }
        }

        [Fact]
        public void TypedViewAndOwnerDisposalInvalidateFutureAccessWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            Mat mat = new Mat(1, 1, MatType.CV_8UC1);
            MatView<byte> view = mat.AsView<byte>();
            view.Dispose();
            Assert.Throws<ObjectDisposedException>(() => view.AsRowSpan(0));
            view.Dispose();
            mat.Dispose();

            Mat ownerDisposed = new Mat(1, 1, MatType.CV_8UC1);
            MatView<byte> borrowed = ownerDisposed.AsView<byte>();
            ownerDisposed.Dispose();
            Assert.Throws<ObjectDisposedException>(() => borrowed.AsRowSpan(0));
            borrowed.Dispose();
        }

        [Fact]
        public void TypedViewDoesNotAllowMatrixHeaderChangesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Mat mat = new Mat(1, 1, MatType.CV_8UC1))
            using (MatView<byte> view = mat.AsView<byte>())
            {
                mat.Create(2, 2, MatType.CV_8UC1);
                Assert.Throws<InvalidOperationException>(() => view.AsRowSpan(0));
            }
        }

        [Fact]
        public void TypedViewCloneAndMatCopyPreserveTheViewedRoiWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Mat source = new Mat(4, 5, MatType.CV_8UC3))
            using (Mat roi = source.SubMat(new Rect(1, 1, 3, 2)))
            using (MatView<Vec3b> view = roi.AsView<Vec3b>())
            using (Mat cloned = view.Clone())
            using (Mat copied = new Mat())
            {
                Assert.Equal(2, cloned.Rows);
                Assert.Equal(3, cloned.Cols);
                Assert.Equal(MatType.CV_8UC3, cloned.Type);

                view.CopyTo(copied);

                Assert.Equal(2, copied.Rows);
                Assert.Equal(3, copied.Cols);
                Assert.Equal(MatType.CV_8UC3, copied.Type);
                Assert.Equal(view.ToArray(), cloned.ToArray<Vec3b>());
                Assert.Equal(view.ToArray(), copied.ToArray<Vec3b>());
            }
        }

        [Fact]
        public void TypedViewCopyToValidatesDestinationBeforeNativeRuntimeWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Mat mat = new Mat(1, 1, MatType.CV_8UC1))
            using (MatView<byte> view = mat.AsView<byte>())
            {
                Assert.Throws<ArgumentNullException>(() => view.CopyTo((Mat)null!));
            }
        }

        private struct UnknownPixel
        {
            public int Value { get; set; }
            public long Padding { get; set; }
        }
#endif
    }
}
