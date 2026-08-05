using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;
using JYPPX.OpenCvSharp.Samples.PublishedPackageSamples;
using Features2DCv2 = JYPPX.OpenCvSharp.Features2D.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Case04OrbFeatures
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = PublishedPackageSampleSupport.GetOutputDirectory(args, "published-package-features");
            using (Mat source = PublishedPackageSampleSupport.CreateSourceImage())
            using (ORB orb = ORB.Create(maxFeatures: 320, fastThreshold: 8))
            using (var descriptors = new Mat())
            {
                orb.DetectAndCompute(source, null, out KeyPoint[] keypoints, descriptors);
                using (var panel = new Mat())
                {
                    Features2DCv2.DrawKeypoints(source, keypoints, panel, new Scalar(48, 238, 255),
                        DrawMatchesFlags.DrawRichKeypoints);
                    PublishedPackageSampleSupport.AddPanelLabel(panel, "04  ORB FEATURES", new Scalar(48, 238, 255));
                    PublishedPackageSampleSupport.AddMetric(panel, "Keypoints  " + keypoints.Length);
                    PublishedPackageSampleSupport.WritePng(outputDirectory, "orb-features.png", panel);
                    PublishedPackageSampleSupport.WriteSummary("ORB features", outputDirectory,
                        "keypoints=" + keypoints.Length + ", descriptorColumns=" + descriptors.Cols);
                }
            }
        }
    }
}
