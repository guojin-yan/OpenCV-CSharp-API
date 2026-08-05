using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;
using VideoCv2 = JYPPX.OpenCvSharp.Video.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Video.OpticalFlow
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "optical-flow");
            using (Mat source = SampleSupport.CreateSourceImage())
            using (var previous = new Mat())
            using (var next = new Mat())
            using (var translation = new Mat(2, 3, MatType.CV_64FC1))
            {
                ImgProcCv2.CvtColor(source, previous, ColorConversionCodes.BGR2GRAY);
                translation.CopyFrom<double>(new[] { 1.0, 0.0, 14.0, 0.0, 1.0, 8.0 });
                ImgProcCv2.WarpAffine(previous, next, translation, previous.Size);
                using (Mat nextBgr = CoreCv2.Merge(new[] { next, next, next }))
                {
                    Point2f[] points = ImgProcCv2.GoodFeaturesToTrack(previous, 120, 0.01, 8.0);
                    Point2f[] tracked = VideoCv2.CalcOpticalFlowPyrLK(previous, next, points, out byte[] status, out float[] errors,
                        new Size(21, 21), maxLevel: 3);
                    int valid = 0;
                    double totalError = 0.0;
                    for (int i = 0; i < tracked.Length; i++)
                    {
                        if (status[i] == 0) continue;
                        valid++;
                        totalError += errors[i];
                        ImgProcCv2.ArrowedLine(nextBgr,
                            new Point((int)points[i].X, (int)points[i].Y),
                            new Point((int)tracked[i].X, (int)tracked[i].Y),
                            new Scalar(52, 226, 164), 2, LineTypes.AntiAlias, tipLength: 0.25);
                    }
                    SampleSupport.AddPanelLabel(nextBgr, "PYRAMIDAL LUCAS-KANADE FLOW", new Scalar(52, 226, 164));
                    SampleSupport.AddMetric(nextBgr, "Tracked  " + valid + "/" + points.Length);
                    SampleSupport.WritePng(outputDirectory, "optical-flow.png", nextBgr);
                    SampleSupport.WriteSummary("Sparse optical flow", outputDirectory,
                        "points=" + points.Length + ", tracked=" + valid +
                        ", meanError=" + SampleSupport.Format(valid == 0 ? 0.0 : totalError / valid));
                }
            }
        }
    }
}
