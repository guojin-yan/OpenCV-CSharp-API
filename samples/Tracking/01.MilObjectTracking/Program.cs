using System;
using System.Collections.Generic;
using System.Linq;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using JYPPX.OpenCvSharp.Video;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Tracking.MilObjectTracking
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "mil-object-tracking");
            using TrackerMIL tracker = TrackerMIL.Create();
            using Mat initial = CreateFrame(0, out Rect initialTarget);
            tracker.Init(initial, initialTarget);
            Rect estimate = initialTarget;
            int successfulUpdates = 0;
            var centers = new List<Point>();
            var errors = new List<double>();
            Mat? finalFrame = null;
            Rect finalTruth = initialTarget;
            try
            {
                for (int frameIndex = 1; frameIndex < 32; frameIndex++)
                {
                    using Mat frame = CreateFrame(frameIndex, out Rect groundTruth);
                    bool updated = tracker.Update(frame, ref estimate);
                    if (updated)
                    {
                        successfulUpdates++;
                    }
                    Point estimatedCenter = Center(estimate);
                    Point truthCenter = Center(groundTruth);
                    centers.Add(estimatedCenter);
                    double dx = estimatedCenter.X - truthCenter.X;
                    double dy = estimatedCenter.Y - truthCenter.Y;
                    errors.Add(Math.Sqrt((dx * dx) + (dy * dy)));
                    if (frameIndex == 31)
                    {
                        finalFrame = frame.Clone();
                        finalTruth = groundTruth;
                    }
                }

                if (finalFrame == null || successfulUpdates < 24)
                {
                    throw new InvalidOperationException("MIL tracking did not remain stable through the generated sequence.");
                }
                ImgProcCv2.Polylines(finalFrame, centers.ToArray(), false, new Scalar(75, 214, 250), 3, LineTypes.AntiAlias);
                ImgProcCv2.Rectangle(finalFrame, finalTruth, new Scalar(92, 162, 255), 2, LineTypes.AntiAlias);
                ImgProcCv2.Rectangle(finalFrame, estimate, new Scalar(55, 232, 151), 4, LineTypes.AntiAlias);
                ImgProcCv2.PutText(finalFrame, "GROUND TRUTH", new Point(finalTruth.X, Math.Max(82, finalTruth.Y - 10)),
                    HersheyFonts.HersheySimplex, 0.48, new Scalar(92, 162, 255), 1, LineTypes.AntiAlias);
                ImgProcCv2.PutText(finalFrame, "MIL TRACKER", new Point(estimate.X, Math.Min(finalFrame.Rows - 12, estimate.Y + estimate.Height + 22)),
                    HersheyFonts.HersheySimplex, 0.52, new Scalar(55, 232, 151), 1, LineTypes.AntiAlias);
                SampleSupport.AddPanelLabel(finalFrame, "MODEL-FREE MIL OBJECT TRACKING", new Scalar(55, 232, 151));
                SampleSupport.AddMetric(finalFrame, "Mean error  " + errors.Average().ToString("0.0") + " px");
                SampleSupport.WritePng(outputDirectory, "mil-object-tracking.png", finalFrame);
                SampleSupport.WriteSummary("MIL object tracking", outputDirectory,
                    "frames=32, successfulUpdates=" + successfulUpdates + ", meanCenterError=" +
                    SampleSupport.Format(errors.Average()) + ", maxCenterError=" + SampleSupport.Format(errors.Max()) +
                    ", trackingScore=" + SampleSupport.Format(tracker.TrackingScore));
            }
            finally
            {
                finalFrame?.Dispose();
            }
        }

        private static Mat CreateFrame(int frameIndex, out Rect target)
        {
            var frame = new Mat(SampleSupport.PanelHeight, SampleSupport.PanelWidth, MatType.CV_8UC3, new Scalar(27, 32, 39));
            ImgProcCv2.Rectangle(frame, new Rect(0, 0, frame.Cols, 70), new Scalar(43, 50, 60), -1);
            for (int x = 25; x < frame.Cols; x += 95)
            {
                ImgProcCv2.Circle(frame, new Point(x, 115 + ((x / 95) % 3) * 72), 20,
                    new Scalar(48 + (x % 90), 70, 92), -1, LineTypes.AntiAlias);
            }
            int xPosition = 62 + (frameIndex * 12);
            int yPosition = 174 + (int)Math.Round(Math.Sin(frameIndex * 0.19) * 24);
            target = new Rect(xPosition, yPosition, 92, 66);
            ImgProcCv2.Rectangle(frame, target, new Scalar(230, 151, 57), -1);
            ImgProcCv2.Rectangle(frame, new Rect(target.X + 8, target.Y + 8, 30, 24), new Scalar(55, 226, 150), -1);
            ImgProcCv2.Circle(frame, new Point(target.X + 66, target.Y + 34), 17, new Scalar(232, 84, 94), -1, LineTypes.AntiAlias);
            ImgProcCv2.Line(frame, new Point(target.X + 9, target.Y + 55), new Point(target.X + 82, target.Y + 9),
                new Scalar(247, 248, 250), 4, LineTypes.AntiAlias);
            return frame;
        }

        private static Point Center(Rect rectangle)
        {
            return new Point(rectangle.X + (rectangle.Width / 2), rectangle.Y + (rectangle.Height / 2));
        }
    }
}
