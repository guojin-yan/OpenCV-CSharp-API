using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using JYPPX.OpenCvSharp.Stitching;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Stitching.PanoramaStitching
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "panorama-stitching");
            using Mat reference = CreatePanoramaScene();
            Mat[] views =
            {
                reference.SubMat(new Rect(0, 0, 640, 360)).Clone(),
                reference.SubMat(new Rect(280, 0, 640, 360)).Clone(),
                reference.SubMat(new Rect(560, 0, 640, 360)).Clone()
            };
            try
            {
                using Stitcher stitcher = Stitcher.Create(StitcherMode.Panorama);
                stitcher.RegistrationResol = 0.6;
                stitcher.SeamEstimationResol = 0.2;
                stitcher.CompositingResol = -1;
                using var panorama = new Mat();
                StitcherStatus status = stitcher.Stitch(views, panorama);
                if (status != StitcherStatus.OK || panorama.Empty)
                {
                    throw new InvalidOperationException("Panorama stitching failed: " + status);
                }

                int[] components = stitcher.GetComponent();
                if (components.Length != views.Length)
                {
                    throw new InvalidOperationException(
                        "Panorama did not retain every input view: " + components.Length + "/" + views.Length);
                }
                SampleSupport.AddPanelLabel(panorama, "FEATURE-BASED PANORAMA", new Scalar(63, 221, 155));
                SampleSupport.AddMetric(panorama, "Views  " + components.Length + "/" + views.Length);
                SampleSupport.WritePng(outputDirectory, "panorama-input-reference.png", reference);
                SampleSupport.WritePng(outputDirectory, "panorama.png", panorama);
                SampleSupport.WriteSummary("Feature-based panorama stitching", outputDirectory,
                    "status=" + status + ", components=" + components.Length + ", output=" + panorama.Cols + "x" + panorama.Rows);
            }
            finally
            {
                foreach (Mat view in views)
                {
                    view.Dispose();
                }
            }
        }

        private static Mat CreatePanoramaScene()
        {
            var image = new Mat(360, 1200, MatType.CV_8UC3, new Scalar(27, 32, 38));
            ImgProcCv2.Rectangle(image, new Rect(0, 0, 1200, 88), new Scalar(43, 51, 61), -1);
            ImgProcCv2.PutText(image, "OPEN COMPUTER VISION PANORAMA", new Point(34, 57),
                HersheyFonts.HersheyDuplex, 1.22, new Scalar(247, 249, 251), 2, LineTypes.AntiAlias);
            var random = new Random(20260805);
            for (int index = 0; index < 120; index++)
            {
                int x = random.Next(12, image.Cols - 12);
                int y = random.Next(96, image.Rows - 18);
                int radius = random.Next(2, 7);
                var color = new Scalar(random.Next(65, 245), random.Next(65, 235), random.Next(65, 245));
                ImgProcCv2.Circle(image, new Point(x, y), radius, color, -1, LineTypes.AntiAlias);
            }
            for (int x = 25; x < image.Cols; x += 75)
            {
                Scalar color = ((x / 75) % 2 == 0) ? new Scalar(69, 207, 137) : new Scalar(237, 163, 68);
                ImgProcCv2.Circle(image, new Point(x, 155 + ((x / 75) % 4) * 35), 18 + ((x / 75) % 3) * 7,
                    color, -1, LineTypes.AntiAlias);
                ImgProcCv2.Line(image, new Point(x - 22, 325), new Point(x + 28, 105),
                    new Scalar(75 + (x % 120), 95, 170), 3, LineTypes.AntiAlias);
            }
            for (int x = 0; x < image.Cols; x += 120)
            {
                ImgProcCv2.PutText(image, (x / 120).ToString("00"), new Point(x + 25, 326),
                    HersheyFonts.HersheyDuplex, 0.72, new Scalar(222, 227, 232), 2, LineTypes.AntiAlias);
            }
            ImgProcCv2.Rectangle(image, new Rect(420, 118, 350, 154), new Scalar(220, 112, 76), 6, LineTypes.AntiAlias);
            ImgProcCv2.PutText(image, "OVERLAP + ALIGN + BLEND", new Point(445, 205),
                HersheyFonts.HersheyDuplex, 0.77, new Scalar(246, 247, 249), 2, LineTypes.AntiAlias);
            return image;
        }
    }
}
