using System;
using OpenCvSharp.HighGui;
using HighGuiCv2 = OpenCvSharp.HighGui.Cv2;

namespace OpenCvSharp.Tests.HighGui
{
    public sealed class HighGuiInteractionTests
    {
        [Fact]
        public void HighGuiInteractionEnumsMatchOpenCvValues()
        {
            Assert.Equal(0, (int)WindowPropertyFlags.Fullscreen);
            Assert.Equal(4, (int)WindowPropertyFlags.Visible);
            Assert.Equal(6, (int)WindowPropertyFlags.VSync);
            Assert.Equal(0, (int)MouseEventTypes.MouseMove);
            Assert.Equal(10, (int)MouseEventTypes.MouseWheel);
            Assert.Equal(32, (int)MouseEventFlags.AltKey);
            Assert.Equal(0, (int)QtButtonTypes.PushButton);
            Assert.Equal(1024, (int)QtButtonTypes.NewButtonbar);
        }

        [Fact]
        public void HighGuiInteractionValidatesManagedArguments()
        {
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.SetWindowProperty(null!, WindowPropertyFlags.Visible, 1.0));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.GetWindowProperty(null!, WindowPropertyFlags.Visible));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.SetWindowTitle(null!, "title"));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.SetWindowTitle("window", null!));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.GetWindowImageRect(null!));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.CreateTrackbar(null!, "window", 0, 10));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.CreateTrackbar("trackbar", null!, 0, 10));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.GetTrackbarPos(null!, "window"));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.GetTrackbarPos("trackbar", null!));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.SetTrackbarPos(null!, "window", 1));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.SetTrackbarPos("trackbar", null!, 1));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.SetTrackbarMin(null!, "window", 0));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.SetTrackbarMin("trackbar", null!, 0));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.SetTrackbarMax(null!, "window", 10));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.SetTrackbarMax("trackbar", null!, 10));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.SetMouseCallback(null!, null));
            Assert.Throws<ArgumentNullException>(() => HighGuiCv2.CreateButton(null!));
        }

        [Fact]
        public void HighGuiInteractionSmokeRunsOnlyWhenExplicitlyEnabled()
        {
            if (!TestEnvironment.IsHighGuiSmokeEnabled())
            {
                return;
            }

            const string windowName = "OpenCvSharp.HighGui.Interaction.Tests";
            HighGuiCv2.NamedWindow(windowName, WindowFlags.AutoSize);
            using (var trackbar = HighGuiCv2.CreateTrackbar("value", windowName, 0, 10, _ => { }))
            {
                HighGuiCv2.SetWindowTitle(windowName, "OpenCvSharp HighGui Interaction");
                HighGuiCv2.SetWindowProperty(windowName, WindowPropertyFlags.Topmost, 0.0);
                HighGuiCv2.SetTrackbarMax("value", windowName, 10);
                HighGuiCv2.SetTrackbarMin("value", windowName, 0);
                HighGuiCv2.SetTrackbarPos("value", windowName, 3);
                Assert.True(HighGuiCv2.GetTrackbarPos("value", windowName) >= 0);
                Assert.False(trackbar.IsDisposed);
            }

            HighGuiCv2.SetMouseCallback(windowName, null);
            HighGuiCv2.DestroyWindow(windowName);
        }

    }
}
