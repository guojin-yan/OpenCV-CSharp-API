using System;
using System.Linq;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using Features2DCv2 = JYPPX.OpenCvSharp.Features2D.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Features.DescriptorMatching
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "descriptor-matching");
            using (Mat source = SampleSupport.CreateSourceImage())
            using (Mat rotation = ImgProcCv2.GetRotationMatrix2D(new Point2f(source.Cols / 2.0F, source.Rows / 2.0F), 6.0, 0.96))
            using (var transformed = new Mat())
            using (ORB orb = ORB.Create(maxFeatures: 500, fastThreshold: 7))
            using (var descriptors1 = new Mat())
            using (var descriptors2 = new Mat())
            using (BFMatcher matcher = BFMatcher.Create(NormTypes.Hamming, crossCheck: true))
            {
                ImgProcCv2.WarpAffine(source, transformed, rotation, source.Size);
                orb.DetectAndCompute(source, null, out KeyPoint[] keypoints1, descriptors1);
                orb.DetectAndCompute(transformed, null, out KeyPoint[] keypoints2, descriptors2);
                DMatch[] matches = matcher.Match(descriptors1, descriptors2)
                    .OrderBy(match => match.Distance)
                    .Take(40)
                    .ToArray();

                using (var panel = new Mat())
                {
                    Features2DCv2.DrawMatches(source, keypoints1, transformed, keypoints2, matches, panel,
                        new Scalar(52, 226, 164), new Scalar(120, 130, 140), DrawMatchesFlags.NotDrawSinglePoints);
                    SampleSupport.AddPanelLabel(panel, "ORB DESCRIPTOR MATCHING", new Scalar(52, 226, 164));
                    SampleSupport.AddMetric(panel, "Best matches  " + matches.Length);
                    SampleSupport.WritePng(outputDirectory, "descriptor-matching.png", panel);
                }

                double meanDistance = matches.Length == 0 ? 0.0 : matches.Average(match => match.Distance);
                SampleSupport.WriteSummary("ORB descriptor matching", outputDirectory,
                    "keypoints=" + keypoints1.Length + "/" + keypoints2.Length +
                    ", matches=" + matches.Length + ", meanDistance=" + SampleSupport.Format(meanDistance));
            }
        }
    }
}
