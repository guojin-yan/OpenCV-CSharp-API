using System;
using OpenCvSharp.Core;
using OpenCvSharp.HighGui;
using HighGuiCv2 = OpenCvSharp.HighGui.Cv2;

namespace OpenCvSharp.Tests.HighGui
{
    public sealed class HighGuiTests
    {
        [Fact]
        public void WindowFlagsMatchOpenCvValues()
        {
            Assert.Equal(0, (int)WindowFlags.Normal);
            Assert.Equal(1, (int)WindowFlags.AutoSize);
            Assert.Equal(0x00001000, (int)WindowFlags.OpenGL);
            Assert.Equal(0x00000100, (int)WindowFlags.FreeRatio);
            Assert.Equal(0x00000010, (int)WindowFlags.GuiNormal);
        }

        [Fact]
        public void HighGuiValidatesManagedArguments()
        {
            using (var image = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => HighGuiCv2.NamedWindow(null!));
                Assert.Throws<ArgumentNullException>(() => HighGuiCv2.DestroyWindow(null!));
                Assert.Throws<ArgumentNullException>(() => HighGuiCv2.ImShow(null!, image));
                Assert.Throws<ArgumentNullException>(() => HighGuiCv2.ImShow("image", null!));
                Assert.Throws<ArgumentNullException>(() => HighGuiCv2.MoveWindow(null!, 0, 0));
                Assert.Throws<ArgumentNullException>(() => HighGuiCv2.ResizeWindow(null!, 10, 10));
            }
        }

        [Fact]
        public void HighGuiSmokeRunsOnlyWhenExplicitlyEnabled()
        {
            if (!TestEnvironment.IsHighGuiSmokeEnabled())
            {
                return;
            }

            using (var image = new Mat(8, 8, MatType.CV_8UC1, new Scalar(0)))
            {
                HighGuiCv2.NamedWindow("OpenCvSharp.HighGui.Tests", WindowFlags.AutoSize);
                HighGuiCv2.ImShow("OpenCvSharp.HighGui.Tests", image);
                HighGuiCv2.PollKey();
                HighGuiCv2.DestroyWindow("OpenCvSharp.HighGui.Tests");
            }
        }

    }
}
