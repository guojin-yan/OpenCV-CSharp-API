using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.HighGui;
using JYPPX.OpenCvSharp.ImgCodecs;
using HighGuiCv2 = JYPPX.OpenCvSharp.HighGui.Cv2;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Headless
{
    public sealed class HeadlessRuntimeCandidateTests
    {
        [Fact]
        public void MiniProfileRejectsHighGuiAndKeepsCodecUsable()
        {
            if (!TestEnvironment.IsHeadlessSmokeEnabled() || !TestEnvironment.IsNativeSmokeEnabled()) return;

            Assert.True(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")));
            Assert.True(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")));

            using (var source = new Mat(2, 2, MatType.CV_8UC1))
            {
                AssertDeterministicUnavailable(() => HighGuiCv2.NamedWindow("JYPPX.Headless.Candidate", WindowFlags.AutoSize));
                AssertDeterministicUnavailable(() => HighGuiCv2.DestroyWindow("JYPPX.Headless.Candidate"));
                AssertDeterministicUnavailable(() => HighGuiCv2.ImShow("JYPPX.Headless.Candidate", source));
                AssertDeterministicUnavailable(() => HighGuiCv2.CreateTrackbar("position", "JYPPX.Headless.Candidate", 0, 10));
                AssertDeterministicUnavailable(() => HighGuiCv2.GetCurrentUIFramework());

                source.SetTo(new Scalar(11));
                byte[] encoded = ImgCodecsCv2.ImEncode(".png", source);
                Assert.NotEmpty(encoded);
                Assert.Equal(0x89, encoded[0]);
                Assert.Equal((byte)'P', encoded[1]);
                Assert.Equal((byte)'N', encoded[2]);
                Assert.Equal((byte)'G', encoded[3]);
            }
        }

        private static void AssertDeterministicUnavailable(Action action)
        {
            Exception? failure = Record.Exception(action);
            Assert.NotNull(failure);
            Assert.True(
                failure is EntryPointNotFoundException ||
                failure is OpenCvException openCvFailure && openCvFailure.Message.ToUpperInvariant().Contains("NOT_LINKED"),
                "Expected NOT_LINKED or a missing HighGui entrypoint, got " + failure.GetType().FullName + ": " + failure.Message);
        }
    }
}
