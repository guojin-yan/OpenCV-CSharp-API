using System;
using System.Linq;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.Geometry.FeatureHomography
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "feature-homography");
            using (Mat source = SampleSupport.CreateSourceImage())
            {
                Point2f[] destinationQuad =
                {
                    new Point2f(50, 45), new Point2f(585, 75),
                    new Point2f(615, 330), new Point2f(30, 300)
                };
                Point2f[] sourceQuad =
                {
                    new Point2f(0, 0), new Point2f(source.Cols - 1, 0),
                    new Point2f(source.Cols - 1, source.Rows - 1), new Point2f(0, source.Rows - 1)
                };
                using (Mat knownTransform = ImgProcCv2.GetPerspectiveTransform(sourceQuad, destinationQuad))
                using (var transformed = new Mat())
                using (ORB orb = ORB.Create(maxFeatures: 700, fastThreshold: 6))
                using (var descriptors1 = new Mat())
                using (var descriptors2 = new Mat())
                using (BFMatcher matcher = BFMatcher.Create(NormTypes.Hamming, crossCheck: true))
                {
                    ImgProcCv2.WarpPerspective(source, transformed, knownTransform, source.Size);
                    orb.DetectAndCompute(source, null, out KeyPoint[] keypoints1, descriptors1);
                    orb.DetectAndCompute(transformed, null, out KeyPoint[] keypoints2, descriptors2);
                    DMatch[] matches = matcher.Match(descriptors1, descriptors2)
                        .OrderBy(match => match.Distance)
                        .Take(80)
                        .ToArray();
                    if (matches.Length < 4) throw new InvalidOperationException("At least four descriptor matches are required.");

                    Point2f[] matchedSource = matches.Select(match => keypoints1[match.QueryIdx].Pt).ToArray();
                    Point2f[] matchedDestination = matches.Select(match => keypoints2[match.TrainIdx].Pt).ToArray();
                    using (Mat sourcePoints = Calib3DCv2.ToPointMat(matchedSource))
                    using (Mat destinationPoints = Calib3DCv2.ToPointMat(matchedDestination))
                    using (var inlierMask = new Mat())
                    using (Mat estimated = Calib3DCv2.FindHomography(sourcePoints, destinationPoints,
                        RobustEstimationAlgorithms.RANSAC, 3.0, inlierMask))
                    using (Mat corners = Calib3DCv2.ToPointMat(sourceQuad))
                    using (Mat projectedCorners = CoreCv2.PerspectiveTransform(corners, estimated))
                    using (Mat panel = transformed.Clone())
                    {
                        float[] projected = projectedCorners.ToArray<float>();
                        for (int i = 0; i < 4; i++)
                        {
                            int next = (i + 1) % 4;
                            ImgProcCv2.Line(panel,
                                new Point((int)projected[i * 2], (int)projected[(i * 2) + 1]),
                                new Point((int)projected[next * 2], (int)projected[(next * 2) + 1]),
                                new Scalar(52, 226, 164), 4, LineTypes.AntiAlias);
                        }
                        int inliers = CoreCv2.CountNonZero(inlierMask);
                        SampleSupport.AddPanelLabel(panel, "FEATURE HOMOGRAPHY + RANSAC", new Scalar(52, 226, 164));
                        SampleSupport.AddMetric(panel, "Inliers  " + inliers + "/" + matches.Length);
                        SampleSupport.WritePng(outputDirectory, "feature-homography.png", panel);
                        SampleSupport.WriteSummary("Feature homography", outputDirectory,
                            "matches=" + matches.Length + ", inliers=" + inliers + ", matrix=" + estimated.Rows + "x" + estimated.Cols);
                    }
                }
            }
        }
    }
}
