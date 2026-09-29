using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.HighGui;
using JYPPX.OpenCvSharp.ImgCodecs;
using JYPPX.OpenCvSharp.VideoIO;
using JYPPX.OpenCvSharp.Dnn;
using DnnCv2 = JYPPX.OpenCvSharp.Dnn.Cv2;
using Xunit.Abstractions;
using HighGuiCv2 = JYPPX.OpenCvSharp.HighGui.Cv2;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Headless
{
    public sealed class HeadlessRuntimeCandidateTests
    {
        private readonly ITestOutputHelper output;

        public HeadlessRuntimeCandidateTests(ITestOutputHelper output)
        {
            this.output = output;
        }

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

                using (var capture = new VideoCapture())
                {
                    Assert.False(capture.Open("JYPPX-headless-missing-input.avi", VideoCaptureAPIs.Any));
                    Assert.False(capture.IsOpened);
                }

                AssertDeterministicUnavailable(() => DnnCv2.GetAvailableTargets(DnnBackend.OpenCV));
                output.WriteLine("HEADLESS_DNN_STATUS=omitted-deterministic-unavailable");

                output.WriteLine("HEADLESS_VIDEOIO_BACKENDS=" + DescribeBackends(VideoIORegistry.GetBackends()));
                output.WriteLine("HEADLESS_VIDEOIO_CAMERA_BACKENDS=" + DescribeBackends(VideoIORegistry.GetCameraBackends()));
                output.WriteLine("HEADLESS_VIDEOIO_MISSING_FILE_OPEN=returned-false");
            }
        }

        private static string DescribeBackends(VideoCaptureAPIs[] backends)
        {
            var descriptions = new string[backends.Length];
            for (int index = 0; index < backends.Length; index++)
            {
                VideoCaptureAPIs backend = backends[index];
                descriptions[index] = ((int)backend).ToString() + ":" + VideoIORegistry.GetBackendName(backend)
                    + (VideoIORegistry.IsBackendBuiltIn(backend) ? ":built-in" : ":plugin");
            }
            return string.Join("|", descriptions);
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
