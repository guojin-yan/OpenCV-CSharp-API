using System;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class Calib3DUpstreamParityTests
    {
        [Fact]
        public void ValueObjectsAndEnumsPreserveNativeLayoutsAndConstants()
        {
            var rect = new Rect2f(1.5F, 2.5F, 20.0F, 10.0F);
            Assert.True(rect.Contains(new Point2f(2.0F, 3.0F)));
            Assert.False(rect.Contains(new Point2f(30.0F, 3.0F)));
            Assert.Equal(16, System.Runtime.InteropServices.Marshal.SizeOf<Rect2f>());

            var triangle = new Vec6f(1, 2, 3, 4, 5, 6);
            Assert.Equal(6.0F, triangle[5]);
            Assert.Equal(24, System.Runtime.InteropServices.Marshal.SizeOf<Vec6f>());
            Assert.Equal(-2, (int)Subdiv2DPointLocation.Error);
            Assert.Equal(0x31, (int)Subdiv2DEdgeNavigation.NextAroundRight);
        }

        [Fact]
        public void UsacDefaultsAndValidationMatchOpenCv5()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            var parameters = new UsacParams();
            Assert.Equal(0.99, parameters.Confidence, 12);
            Assert.False(parameters.IsParallel);
            Assert.Equal(5, parameters.LocalOptimizationIterations);
            Assert.Equal(UsacLocalOptimizationMethod.Inner, parameters.LocalOptimizationMethod);
            Assert.Equal(14, parameters.LocalOptimizationSampleSize);
            Assert.Equal(5000, parameters.MaxIterations);
            Assert.Equal(UsacNeighborSearchMethod.Grid, parameters.NeighborSearchMethod);
            Assert.Equal(UsacSamplingMethod.Uniform, parameters.SamplingMethod);
            Assert.Equal(UsacScoreMethod.Msac, parameters.ScoreMethod);
            Assert.Equal(1.5, parameters.Threshold, 12);
            Assert.Equal(UsacPolishingMethod.Covariance, parameters.FinalPolishingMethod);
            Assert.Equal(3, parameters.FinalPolishingIterations);

            parameters.Confidence = 0.0;
            using Mat points = CreatePointMatrix(new[] { 0F, 0F, 1F, 0F, 1F, 1F, 0F, 1F }, 2);
            using var mask = new Mat();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.FindHomography(points, points, mask, parameters));
        }

        [Fact]
        public void Subdiv2DExposesLifecycleGeometryAndTopology()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using var subdiv = new Subdiv2D(new Rect2f(0, 0, 100, 100));
            Point2f[] points =
            {
                new Point2f(10, 10), new Point2f(85, 10), new Point2f(85, 85),
                new Point2f(10, 85), new Point2f(48, 48)
            };
            subdiv.Insert(points);

            Subdiv2DPointLocation location = subdiv.Locate(points[4], out int edge, out int vertex);
            Assert.Equal(Subdiv2DPointLocation.Vertex, location);
            Assert.True(vertex > 0);
            Assert.Equal(vertex, subdiv.FindNearest(new Point2f(47, 47), out Point2f nearest));
            Assert.Equal(points[4], nearest);

            Vec4f[] edges = subdiv.GetEdgeList();
            Vec6f[] triangles = subdiv.GetTriangleList();
            int[] leadingEdges = subdiv.GetLeadingEdgeList();
            Assert.NotEmpty(edges);
            Assert.NotEmpty(triangles);
            Assert.NotEmpty(leadingEdges);

            Point2f vertexPoint = subdiv.GetVertex(vertex, out int firstEdge);
            Assert.Equal(points[4], vertexPoint);
            Assert.True(firstEdge > 0);
            Assert.True(subdiv.NextEdge(firstEdge) > 0);
            Assert.True(subdiv.SymEdge(firstEdge) > 0);
            Assert.Equal(firstEdge, subdiv.RotateEdge(firstEdge, 0));
            Assert.True(subdiv.GetEdge(firstEdge, Subdiv2DEdgeNavigation.NextAroundOrigin) > 0);
            Assert.True(subdiv.EdgeOrg(firstEdge, out Point2f _) > 0);
            Assert.True(subdiv.EdgeDst(firstEdge, out Point2f _) > 0);

            subdiv.GetVoronoiFacetList(new[] { vertex }, out Point2f[][] facets, out Point2f[] centers);
            Assert.Single(facets);
            Assert.Single(centers);
            Assert.True(facets[0].Length >= 3);
            Assert.Equal(points[4], centers[0]);

            Assert.Throws<ArgumentOutOfRangeException>(() => subdiv.RotateEdge(edge, 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => subdiv.InitDelaunay(new Rect2f(0, 0, 0, 1)));
        }

        [Fact]
        public void UsacOverloadsExecuteAllFiveUpstreamOperations()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            float[] planar = { 0, 0, 40, 0, 40, 30, 0, 30, 8, 7, 31, 6, 34, 24, 7, 22 };
            float[] translated = new float[planar.Length];
            for (int index = 0; index < planar.Length; index += 2)
            {
                translated[index] = planar[index] + 3;
                translated[index + 1] = planar[index + 1] + 4;
            }

            CreateStereoCorrespondences(out float[] objectValues, out float[] firstImage, out float[] secondImage);
            using Mat planarPoints = CreatePointMatrix(planar, 2);
            using Mat translatedPoints = CreatePointMatrix(translated, 2);
            using Mat objectPoints = CreatePointMatrix(objectValues, 3);
            using Mat imagePoints1 = CreatePointMatrix(firstImage, 2);
            using Mat imagePoints2 = CreatePointMatrix(secondImage, 2);
            using Mat cameraMatrix1 = CreateCameraMatrix();
            using Mat cameraMatrix2 = CreateCameraMatrix();
            using var distCoeffs1 = new Mat();
            using var distCoeffs2 = new Mat();
            using var homographyMask = new Mat();
            using var affineMask = new Mat();
            using var fundamentalMask = new Mat();
            using var essentialMask = new Mat();
            using var pnpInliers = new Mat();
            using var rvec = new Mat();
            using var tvec = new Mat();
            var parameters = new UsacParams { RandomGeneratorState = 7 };

            using Mat homography = Calib3DCv2.FindHomography(planarPoints, translatedPoints, homographyMask, parameters);
            using Mat affine = Calib3DCv2.EstimateAffine2D(planarPoints, translatedPoints, affineMask, parameters);
            using Mat fundamental = Calib3DCv2.FindFundamentalMat(imagePoints1, imagePoints2, fundamentalMask, parameters);
            using Mat essential = Calib3DCv2.FindEssentialMat(
                imagePoints1, imagePoints2, cameraMatrix1, cameraMatrix2,
                distCoeffs1, distCoeffs2, essentialMask, parameters);
            bool solved = Calib3DCv2.SolvePnPRansac(
                objectPoints, imagePoints1, cameraMatrix1, distCoeffs1,
                rvec, tvec, pnpInliers, parameters);

            Assert.Equal((3, 3), (homography.Rows, homography.Cols));
            Assert.Equal((2, 3), (affine.Rows, affine.Cols));
            Assert.Equal((3, 3), (fundamental.Rows, fundamental.Cols));
            Assert.Equal((3, 3), (essential.Rows, essential.Cols));
            Assert.True(solved);
            Assert.False(pnpInliers.Empty);
        }

        [Fact]
        public void FisheyeStereoRectifyReturnsOwnedModelSpecificOutputs()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using Mat cameraMatrix1 = CreateCameraMatrix();
            using Mat cameraMatrix2 = CreateCameraMatrix();
            using Mat distCoeffs1 = Mat.Zeros(4, 1, MatType.CV_64FC1);
            using Mat distCoeffs2 = Mat.Zeros(4, 1, MatType.CV_64FC1);
            using Mat r = Mat.Eye(3, 3, MatType.CV_64FC1);
            using var t = new Mat(3, 1, MatType.CV_64FC1);
            t.SetValue(0, 0.2);

            FisheyeStereoRectifyResult result = Calib3DCv2.FisheyeStereoRectify(
                cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2,
                new Size(640, 480), r, t);
            using (result.R1)
            using (result.R2)
            using (result.P1)
            using (result.P2)
            using (result.Q)
            {
                Assert.Equal((3, 3), (result.R1.Rows, result.R1.Cols));
                Assert.Equal((3, 3), (result.R2.Rows, result.R2.Cols));
                Assert.Equal(3, result.P1.Rows);
                Assert.Equal(3, result.P2.Rows);
                Assert.Equal((4, 4), (result.Q.Rows, result.Q.Cols));
            }
        }

        private static Mat CreateCameraMatrix()
        {
            Mat result = Mat.Eye(3, 3, MatType.CV_64FC1);
            result.SetValue(0, 500.0);
            result.SetValue(2, 320.0);
            result.SetValue(4, 500.0);
            result.SetValue(5, 240.0);
            return result;
        }

        private static Mat CreatePointMatrix(float[] values, int dimensions)
        {
            var result = new Mat(values.Length / dimensions, dimensions, MatType.CV_32FC1);
            for (int index = 0; index < values.Length; ++index) result.SetValue(index, values[index]);
            return result;
        }

        private static void CreateStereoCorrespondences(
            out float[] objectValues,
            out float[] firstImage,
            out float[] secondImage)
        {
            Point3f[] points =
            {
                new Point3f(-1.0F, -0.8F, 4.0F), new Point3f(0.7F, -0.9F, 4.5F),
                new Point3f(1.2F, 0.6F, 5.0F), new Point3f(-0.8F, 0.9F, 5.5F),
                new Point3f(0.1F, -0.2F, 6.0F), new Point3f(1.5F, 1.0F, 6.5F),
                new Point3f(-1.4F, 0.3F, 7.0F), new Point3f(0.4F, 1.4F, 7.5F),
                new Point3f(-0.3F, -1.2F, 8.0F), new Point3f(1.1F, -0.1F, 8.5F)
            };
            objectValues = new float[points.Length * 3];
            firstImage = new float[points.Length * 2];
            secondImage = new float[points.Length * 2];
            for (int index = 0; index < points.Length; ++index)
            {
                Point3f point = points[index];
                objectValues[(index * 3) + 0] = point.X;
                objectValues[(index * 3) + 1] = point.Y;
                objectValues[(index * 3) + 2] = point.Z;
                firstImage[(index * 2) + 0] = (500.0F * point.X / point.Z) + 320.0F;
                firstImage[(index * 2) + 1] = (500.0F * point.Y / point.Z) + 240.0F;
                secondImage[(index * 2) + 0] = (500.0F * (point.X - 0.2F) / point.Z) + 320.0F;
                secondImage[(index * 2) + 1] = firstImage[(index * 2) + 1];
            }
        }
    }
}
