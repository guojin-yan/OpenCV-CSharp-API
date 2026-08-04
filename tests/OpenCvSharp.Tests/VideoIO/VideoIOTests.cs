using System;
using System.IO;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.VideoIO;

namespace JYPPX.OpenCvSharp.Tests.VideoIO
{
    public sealed class VideoIOTests
    {
        [Fact]
        public void EnumValuesMatchOpenCvConstants()
        {
            Assert.Equal(0, (int)VideoCaptureAPIs.Any);
            Assert.Equal(200, (int)VideoCaptureAPIs.V4L);
            Assert.Equal((int)VideoCaptureAPIs.V4L, (int)VideoCaptureAPIs.V4L2);
            Assert.Equal(700, (int)VideoCaptureAPIs.DShow);
            Assert.Equal(1400, (int)VideoCaptureAPIs.MSMF);
            Assert.Equal(1800, (int)VideoCaptureAPIs.GStreamer);
            Assert.Equal(1900, (int)VideoCaptureAPIs.FFmpeg);
            Assert.Equal(2600, (int)VideoCaptureAPIs.OBSensor);

            Assert.Equal(-1, (int)VideoCaptureProperties.Unknown);
            Assert.Equal(0, (int)VideoCaptureProperties.PosMsec);
            Assert.Equal(3, (int)VideoCaptureProperties.FrameWidth);
            Assert.Equal(4, (int)VideoCaptureProperties.FrameHeight);
            Assert.Equal(5, (int)VideoCaptureProperties.Fps);
            Assert.Equal(6, (int)VideoCaptureProperties.FourCC);
            Assert.Equal(42, (int)VideoCaptureProperties.Backend);
            Assert.Equal(50, (int)VideoCaptureProperties.HwAcceleration);
            Assert.Equal(73, (int)VideoCaptureProperties.ImageSequenceStart);

            Assert.Equal(-1, (int)VideoWriterProperties.Unknown);
            Assert.Equal(1, (int)VideoWriterProperties.Quality);
            Assert.Equal(2, (int)VideoWriterProperties.FrameBytes);
            Assert.Equal(6, (int)VideoWriterProperties.HwAcceleration);
            Assert.Equal(15, (int)VideoWriterProperties.EnableAlpha);

            Assert.Equal(0, (int)VideoAccelerationType.None);
            Assert.Equal(1, (int)VideoAccelerationType.Any);
            Assert.Equal(2, (int)VideoAccelerationType.D3D11);
            Assert.Equal(5, (int)VideoAccelerationType.DRM);
        }

        [Fact]
        public void FourCCBuildsExpectedInteger()
        {
            int expected = 'M' | ('J' << 8) | ('P' << 16) | ('G' << 24);

            Assert.Equal(expected, VideoWriter.FourCC('M', 'J', 'P', 'G'));
            Assert.Equal(expected, VideoWriter.FourCC("MJPG"));
        }

#if NETCOREAPP3_1_OR_GREATER
        [Fact]
        public void FourCCSpanBuildsExpectedInteger()
        {
            int expected = 'X' | ('V' << 8) | ('I' << 16) | ('D' << 24);

            Assert.Equal(expected, VideoWriter.FourCC("XVID".AsSpan()));
        }
#endif

        [Fact]
        public void FourCCRejectsInvalidInput()
        {
            Assert.Throws<ArgumentNullException>(() => VideoWriter.FourCC((string)null!));
            Assert.Throws<ArgumentException>(() => VideoWriter.FourCC("MJ"));
            Assert.Throws<ArgumentOutOfRangeException>(() => VideoWriter.FourCC('\u0100', 'J', 'P', 'G'));
        }

        [Fact]
        public void VideoWriterRejectsInvalidFrameSizeBeforeNativeOpen()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var writer = new VideoWriter())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    writer.Open("out.avi", VideoWriter.FourCC("MJPG"), 30.0, new Size(0, 10)));
            }
        }

        [Fact]
        public void VideoCaptureCanBeCreatedWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var capture = new VideoCapture())
            {
                Assert.False(capture.IsDisposed);
                Assert.False(capture.IsOpened);
                Assert.False(capture.Open(-1));
            }
        }

        [Fact]
        public void VideoWriterCanBeCreatedWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var writer = new VideoWriter())
            {
                Assert.False(writer.IsDisposed);
                Assert.False(writer.IsOpened);
            }
        }

        [Fact]
        public void VideoCaptureNullFilenameValidationRunsBeforeNativeCall()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var capture = new VideoCapture())
            {
                Assert.Throws<ArgumentNullException>(() => capture.Open(null!));
            }
        }

        [Fact]
        public void VideoWriterNullFilenameValidationRunsBeforeNativeCall()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var writer = new VideoWriter())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    writer.Open(null!, VideoWriter.FourCC("MJPG"), 30.0, new Size(16, 16)));
            }
        }

        [Fact]
        public void ReadAndWriteValidateMatArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var capture = new VideoCapture())
            using (var writer = new VideoWriter())
            using (var image = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => capture.Read(null!));
                Assert.Throws<ArgumentNullException>(() => capture.Retrieve(null!));
                Assert.Throws<ArgumentNullException>(() => writer.Write(null!));

                capture.Dispose();
                Assert.True(capture.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => capture.Read(image));
                Assert.Throws<ObjectDisposedException>(() => capture.Read());
                Assert.Throws<ObjectDisposedException>(() => capture.Retrieve(image));
                Assert.Throws<ObjectDisposedException>(() => capture.Retrieve());

                writer.Dispose();
                Assert.True(writer.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => writer.Write(image));
            }
        }

        [Fact]
        public void VideoCaptureDisposedStateRejectsNonReadOperationsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            var capture = new VideoCapture();
            capture.Dispose();

            Assert.True(capture.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => capture.IsOpened);
            Assert.Throws<ObjectDisposedException>(() => capture.Open("missing.avi"));
            Assert.Throws<ObjectDisposedException>(() => capture.Open(-1));
            Assert.Throws<ObjectDisposedException>(() => capture.Release());
            Assert.Throws<ObjectDisposedException>(() => capture.Grab());
            Assert.Throws<ObjectDisposedException>(() => capture.Get(VideoCaptureProperties.FrameWidth));
            Assert.Throws<ObjectDisposedException>(() => capture.Set(VideoCaptureProperties.FrameWidth, 16.0));
            Assert.Throws<ObjectDisposedException>(() => capture.FrameWidth);
            Assert.Throws<ObjectDisposedException>(() => capture.FrameWidth = 16.0);
            Assert.Throws<ObjectDisposedException>(() => capture.GetBackendName());
        }

        [Fact]
        public void VideoWriterDisposedStateRejectsNonWriteOperationsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            var writer = new VideoWriter();
            writer.Dispose();

            Assert.True(writer.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => writer.IsOpened);
            Assert.Throws<ObjectDisposedException>(() => writer.Open("out.avi", VideoWriter.FourCC("MJPG"), 30.0, new Size(16, 16)));
            Assert.Throws<ObjectDisposedException>(() => writer.Release());
            Assert.Throws<ObjectDisposedException>(() => writer.Get(VideoWriterProperties.Quality));
            Assert.Throws<ObjectDisposedException>(() => writer.Set(VideoWriterProperties.Quality, 90.0));
            Assert.Throws<ObjectDisposedException>(() => writer.Quality);
            Assert.Throws<ObjectDisposedException>(() => writer.Quality = 90.0);
            Assert.Throws<ObjectDisposedException>(() => writer.GetBackendName());
        }

        [Fact]
        public void WaitAnyValidatesCaptureCollectionBeforeNativeCall()
        {
            Assert.Throws<ArgumentException>(() => VideoCapture.WaitAny(Array.Empty<VideoCapture>(), out _));
            Assert.Throws<ArgumentNullException>(() => VideoCapture.WaitAny(null!, out _));
        }

        [Fact]
        public void VideoIOParameterPairsRejectOddLength()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var capture = new VideoCapture())
            using (var writer = new VideoWriter())
            {
                Assert.Throws<ArgumentException>(() => capture.Open(-1, VideoCaptureAPIs.Any, 1));
                Assert.Throws<ArgumentException>(() => writer.Open("out.avi", VideoWriter.FourCC("MJPG"), 30.0, new Size(16, 16), 1));
            }
        }

        [Fact]
        public void ManagedStreamReaderRoundTripsReadAndSeek()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var stream = new MemoryStream(new byte[] { 10, 20, 30, 40 }))
            using (var reader = new VideoStreamReader(stream, leaveOpen: true))
            {
                var buffer = new byte[2];
                Assert.Equal(2, reader.Read(buffer, 0, buffer.Length));
                Assert.Equal(new byte[] { 10, 20 }, buffer);
                Assert.Equal(3, reader.Seek(1, SeekOrigin.Current));
                var final = new byte[1];
                Assert.Equal(1, reader.Read(final, 0, final.Length));
                Assert.Equal(40, final[0]);
            }
        }

    }
}
