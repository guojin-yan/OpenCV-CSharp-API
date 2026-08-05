using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;
using JYPPX.OpenCvSharp.Samples.Common;
using Features2DCv2 = JYPPX.OpenCvSharp.Features2D.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Features.OrbFeatures
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "orb-features");
            using (Mat source = SampleSupport.CreateSourceImage())
            using (ORB orb = ORB.Create(maxFeatures: 320, fastThreshold: 8))
            using (var descriptors = new Mat())
            {
                orb.DetectAndCompute(source, null, out KeyPoint[] keypoints, descriptors);
                using (var panel = new Mat())
                {
                    Features2DCv2.DrawKeypoints(source, keypoints, panel, new Scalar(48, 238, 255),
                        DrawMatchesFlags.DrawRichKeypoints);
                    SampleSupport.AddPanelLabel(panel, "ORB FEATURES", new Scalar(48, 238, 255));
                    SampleSupport.AddMetric(panel, "Keypoints  " + keypoints.Length);
                    SampleSupport.WritePng(outputDirectory, "orb-features.png", panel);
                    SampleSupport.WriteSummary("ORB features", outputDirectory,
                        "keypoints=" + keypoints.Length + ", descriptorColumns=" + descriptors.Cols);
                }
            }
        }
    }
}
