using System;
using System.IO;
using JYPPX.OpenCvSharp.Core;
using VideoCv2 = JYPPX.OpenCvSharp.Video.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Video
{
    public sealed class VideoOpticalFlowIoTests
    {
        [Fact]
        public void FlowIoValidatesManagedArguments()
        {
            using (var flow = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => VideoCv2.ReadOpticalFlow(null!));
                Assert.Throws<ArgumentNullException>(() => VideoCv2.WriteOpticalFlow(null!, flow));
                Assert.Throws<ArgumentNullException>(() => VideoCv2.WriteOpticalFlow("flow.flo", null!));
            }
        }

        [Fact]
        public void FlowIoSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string path = Path.Combine(Path.GetTempPath(), "opencv-csharp-flow-" + Guid.NewGuid().ToString("N") + ".flo");
            try
            {
                using (Mat flow = new Mat(2, 3, MatType.CV_32FC2, new Scalar(1.0, 2.0, 0.0, 0.0)))
                {
                    bool written = VideoCv2.WriteOpticalFlow(path, flow);
                    Assert.True(written);
                }

                using (Mat read = VideoCv2.ReadOpticalFlow(path))
                {
                    Assert.False(read.Empty);
                    Assert.Equal(2, read.Rows);
                    Assert.Equal(3, read.Cols);
                    Assert.Equal(2, read.Channels);
                    Assert.Equal(MatType.CV_32FC2, read.Type);
                }
            }
            catch (OpenCvException ex) when (IsNativeBoundary(ex))
            {
                Assert.Contains("NOT_LINKED", ex.Message, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private static bool IsNativeBoundary(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("video", StringComparison.OrdinalIgnoreCase) >= 0;
        }

    }
}
