using System;
using System.Collections.Generic;
using System.Linq;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using JYPPX.OpenCvSharp.Video;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Video.MotionAnalysis
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "motion-analysis");
            var trajectory = new List<Point>();
            using BackgroundSubtractorMOG2 subtractor = BackgroundSubtractorMOG2.Create(36, 18, false);
            using Mat kernel = ImgProcCv2.GetStructuringElement(MorphShapes.Ellipse, new Size(7, 7));
            using var mask = new Mat();
            using var cleaned = new Mat();
            Mat? finalFrame = null;
            Rect finalBox = new Rect();
            int detectedFrames = 0;
            try
            {
                for (int frameIndex = 0; frameIndex < 42; frameIndex++)
                {
                    using Mat frame = CreateFrame(frameIndex, out _);
                    subtractor.Apply(frame, mask, frameIndex < 5 ? 0.45 : 0.015);
                    ImgProcCv2.MorphologyEx(mask, cleaned, MorphTypes.Open, kernel);
                    ImgProcCv2.MorphologyEx(cleaned, cleaned, MorphTypes.Close, kernel, iterations: 2);
                    ImgProcCv2.FindContours(cleaned, out Point[][] contours, out _,
                        RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                    Point[]? target = contours
                        .Where(contour =>
                        {
                            double area = Math.Abs(ImgProcCv2.ContourArea(contour));
                            return area >= 800 && area <= 12000;
                        })
                        .OrderByDescending(contour => Math.Abs(ImgProcCv2.ContourArea(contour)))
                        .FirstOrDefault();
                    if (target != null)
                    {
                        finalBox = ImgProcCv2.BoundingRect(target);
                        trajectory.Add(new Point(finalBox.X + (finalBox.Width / 2), finalBox.Y + (finalBox.Height / 2)));
                        detectedFrames++;
                    }
                    if (frameIndex == 41)
                    {
                        finalFrame = frame.Clone();
                    }
                }

                if (finalFrame == null || trajectory.Count < 20)
                {
                    throw new InvalidOperationException("Motion analysis did not produce a stable trajectory.");
                }
                ImgProcCv2.Polylines(finalFrame, trajectory.ToArray(), false, new Scalar(73, 214, 251), 3, LineTypes.AntiAlias);
                ImgProcCv2.Rectangle(finalFrame, finalBox, new Scalar(58, 232, 154), 4, LineTypes.AntiAlias);
                foreach (Point center in trajectory.Where((_, index) => index % 4 == 0))
                {
                    ImgProcCv2.Circle(finalFrame, center, 4, new Scalar(73, 214, 251), -1, LineTypes.AntiAlias);
                }
                SampleSupport.AddPanelLabel(finalFrame, "MOTION DETECTION + TRAJECTORY", new Scalar(58, 232, 154));
                SampleSupport.AddMetric(finalFrame, "Detected  " + detectedFrames + "/42");
                SampleSupport.WritePng(outputDirectory, "motion-mask.png", cleaned);
                SampleSupport.WritePng(outputDirectory, "motion-analysis.png", finalFrame);
                SampleSupport.WriteSummary("Motion analysis pipeline", outputDirectory,
                    "frames=42, detectedFrames=" + detectedFrames + ", trajectoryPoints=" + trajectory.Count +
                    ", foregroundPixels=" + CoreCv2.CountNonZero(cleaned));
            }
            finally
            {
                finalFrame?.Dispose();
            }
        }

        private static Mat CreateFrame(int frameIndex, out Rect target)
        {
            var frame = new Mat(SampleSupport.PanelHeight, SampleSupport.PanelWidth, MatType.CV_8UC3, new Scalar(26, 31, 37));
            for (int x = 0; x < frame.Cols; x += 80)
            {
                ImgProcCv2.Line(frame, new Point(x, 78), new Point(x, frame.Rows), new Scalar(38, 44, 51), 1);
            }
            ImgProcCv2.Rectangle(frame, new Rect(0, 0, frame.Cols, 68), new Scalar(42, 49, 58), -1);
            ImgProcCv2.Circle(frame, new Point(535, 175), 44, new Scalar(65, 80, 98), -1);
            int xPosition = 28 + (frameIndex * 11);
            int yPosition = 180 + (int)Math.Round(Math.Sin(frameIndex * 0.22) * 28);
            target = new Rect(xPosition, yPosition, 84, 58);
            ImgProcCv2.Rectangle(frame, target, new Scalar(58, 218, 148), -1);
            ImgProcCv2.Circle(frame, new Point(target.X + 22, target.Y + 29), 12, new Scalar(238, 182, 71), -1);
            ImgProcCv2.Line(frame, new Point(target.X + 42, target.Y + 8), new Point(target.X + 72, target.Y + 49),
                new Scalar(246, 247, 249), 4, LineTypes.AntiAlias);
            return frame;
        }
    }
}
