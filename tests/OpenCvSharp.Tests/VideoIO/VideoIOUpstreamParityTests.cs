using System;
using System.IO;
using System.Linq;
using OpenCvSharp.Core;
using OpenCvSharp.VideoIO;

namespace OpenCvSharp.Tests.VideoIO
{
    public sealed class VideoIOUpstreamParityTests
    {
        [Fact]
        public void ParameterExceptionAndWaitAnySurfacesArePublic()
        {
            Assert.Contains(typeof(VideoCapture).GetConstructors(), constructor =>
                constructor.GetParameters().Any(parameter => parameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length != 0));
            Assert.Contains(typeof(VideoWriter).GetConstructors(), constructor =>
                constructor.GetParameters().Any(parameter => parameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length != 0));
            Assert.NotNull(typeof(VideoCapture).GetProperty(nameof(VideoCapture.ExceptionMode)));
            Assert.Contains(typeof(VideoCapture).GetMethods(), method => method.Name == nameof(VideoCapture.WaitAny));
            Assert.Contains(typeof(VideoCaptureExtensions).GetMethods(), method => method.Name == nameof(VideoCaptureExtensions.Open));
        }

        [Fact]
        public void StreamReaderCallbacksReadSeekAndPreserveLeaveOpen()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            using (var reader = new VideoStreamReader(stream, leaveOpen: true))
            {
                var bytes = new byte[3];
                Assert.Equal(3, reader.Read(bytes, 0, bytes.Length));
                Assert.Equal(new byte[] { 1, 2, 3 }, bytes);
                Assert.Equal(1, reader.Seek(1, SeekOrigin.Begin));
            }
            Assert.True(stream.CanRead);
            stream.Dispose();
        }

        [Fact]
        public void WriterAndCaptureParameterOverloadsRoundTripFrames()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            string path = Path.Combine(Path.GetTempPath(), "opencv-csharp-videoio-" + Guid.NewGuid().ToString("N") + ".avi");
            try
            {
                using (var writer = new VideoWriter())
                using (var frame = new Mat(24, 32, MatType.CV_8UC3))
                {
                    Assert.True(writer.Open(path, VideoWriter.FourCC("MJPG"), 10.0, frame.Size, Array.Empty<int>()));
                    for (int index = 0; index < 3; index++)
                    {
                        frame.SetTo(new Scalar(20 + index, 60 + index, 120 + index));
                        Assert.True(writer.Write(frame));
                    }
                    writer.Release();
                }

                using (var capture = new VideoCapture())
                using (var decoded = new Mat())
                {
                    Assert.True(capture.Open(path, VideoCaptureAPIs.Any, Array.Empty<int>()));
                    capture.ExceptionMode = true;
                    Assert.True(capture.ExceptionMode);
                    Assert.True(capture.Read(decoded));
                    Assert.Equal(24, decoded.Rows);
                    Assert.Equal(32, decoded.Cols);
                }
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void RegistryCategoryListsHaveStableManagedShapes()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            Assert.NotNull(VideoIORegistry.GetBackends());
            Assert.NotNull(VideoIORegistry.GetCameraBackends());
            Assert.NotNull(VideoIORegistry.GetStreamBackends());
            Assert.NotNull(VideoIORegistry.GetStreamBufferedBackends());
            Assert.NotNull(VideoIORegistry.GetWriterBackends());
        }
    }
}
