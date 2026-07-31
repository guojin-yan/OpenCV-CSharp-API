using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;
using OpenCvSharp.Stitching;

namespace OpenCvSharp.Tests.Stitching
{
    public sealed class MotionEstimatorTests
    {
        [Fact]
        public void CameraMatrixAndAutocalibrationReturnExpectedOwnedValuesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Mat rotation = CreateMatrix(Identity()))
            using (Mat translation = new Mat(3, 1, MatType.CV_64FC1, new Scalar(0)))
            {
                var camera = new StitcherCameraParams(500.0, 1.2, 320.0, 240.0, rotation, translation);
                using (Mat cameraMatrix = camera.GetCameraMatrix())
                {
                    Assert.Equal(MatType.CV_64FC1, cameraMatrix.Type);
                    Assert.Equal(500.0, cameraMatrix.GetValue<double>(0), 8);
                    Assert.Equal(600.0, cameraMatrix.GetValue<double>(4), 8);
                    Assert.Equal(320.0, cameraMatrix.GetValue<double>(2), 8);
                    Assert.Equal(240.0, cameraMatrix.GetValue<double>(5), 8);
                }
            }

            Mat[] homographies = CreateRotatingHomographies();
            try
            {
                StitchingMotion.FocalsFromHomography(
                    homographies[0], out double focalX, out double focalY,
                    out bool focalXEstimated, out bool focalYEstimated);
                Assert.True(focalXEstimated || focalYEstimated);
                if (focalXEstimated) Assert.InRange(focalX, 499.0, 501.0);
                if (focalYEstimated) Assert.InRange(focalY, 499.0, 501.0);

                Assert.True(StitchingMotion.TryCalibrateRotatingCamera(homographies, out Mat calibrated));
                using (calibrated)
                {
                    Assert.Equal(MatType.CV_64FC1, calibrated.Type);
                    Assert.Equal(3, calibrated.Rows);
                    Assert.Equal(3, calibrated.Cols);
                    Assert.InRange(calibrated.GetValue<double>(0), 499.0, 501.0);
                    Assert.InRange(calibrated.GetValue<double>(4), 499.0, 501.0);
                    Assert.Equal(1.0, calibrated.GetValue<double>(8), 8);
                }
            }
            finally
            {
                DisposeMats(homographies);
            }
        }

        [Fact]
        public void HomographyAndAffineEstimatorsReturnIndependentCamerasWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            ImageFeatures[] projectiveFeatures = CreateProjectiveFeatures();
            MatchesInfo[] projectiveMatches = MatchProjective(projectiveFeatures);
            try
            {
                using (var estimator = new HomographyBasedEstimator())
                {
                    Assert.True(estimator.Apply(projectiveFeatures, projectiveMatches, out StitcherCameraParams[] cameras));
                    try
                    {
                        Assert.Equal(projectiveFeatures.Length, cameras.Length);
                        Assert.All(cameras, AssertValidCamera);
                        cameras[0].Rotation.SetTo(new Scalar(7));
                        Assert.NotEqual(7.0F, cameras[1].Rotation.GetValue<float>(0));
                    }
                    finally
                    {
                        DisposeCameras(cameras);
                    }
                }

                using (var estimator = new HomographyBasedEstimator(focalLengthsEstimated: true))
                {
                    Assert.Throws<ArgumentNullException>(() =>
                        estimator.Apply(projectiveFeatures, projectiveMatches, out _));
                }
            }
            finally
            {
                DisposeMatches(projectiveMatches);
                DisposeFeatures(projectiveFeatures);
            }

            ImageFeatures[] affineFeatures = CreateAffineFeatures();
            MatchesInfo[] affineMatches = MatchAffine(affineFeatures);
            try
            {
                using (var estimator = new AffineBasedEstimator())
                {
                    Assert.True(estimator.Apply(affineFeatures, affineMatches, out StitcherCameraParams[] cameras));
                    try
                    {
                        Assert.Equal(affineFeatures.Length, cameras.Length);
                        Assert.All(cameras, AssertValidCamera);
                    }
                    finally
                    {
                        DisposeCameras(cameras);
                    }
                }
            }
            finally
            {
                DisposeMatches(affineMatches);
                DisposeFeatures(affineFeatures);
            }
        }

        [Fact]
        public void BundleAdjusterPropertiesAndNoOpApplicationRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            BundleAdjusterBase[] adjusters =
            {
                new NoBundleAdjuster(),
                new BundleAdjusterReproj(),
                new BundleAdjusterRay(),
                new BundleAdjusterAffine(),
                new BundleAdjusterAffinePartial()
            };
            try
            {
                foreach (BundleAdjusterBase adjuster in adjusters)
                {
                    adjuster.ConfidenceThreshold = 0.25;
                    Assert.Equal(0.25, adjuster.ConfidenceThreshold, 8);
                    adjuster.TerminationCriteria = new TermCriteria(TermCriteriaTypes.Count, 2, 0.0);
                    Assert.Equal(TermCriteriaTypes.Count, adjuster.TerminationCriteria.Type);
                    Assert.Equal(2, adjuster.TerminationCriteria.MaxCount);

                    using (var mask = new Mat(3, 3, MatType.CV_8UC1, new Scalar(0)))
                    {
                        mask.CopyFrom(new byte[] { 1, 0, 0, 0, 1, 0, 0, 0, 0 });
                        adjuster.RefinementMask = mask;
                        mask.SetTo(new Scalar(0));
                    }
                    using (Mat copiedMask = adjuster.RefinementMask)
                    {
                        Assert.Equal((byte)1, copiedMask.GetValue<byte>(0));
                        Assert.Equal((byte)1, copiedMask.GetValue<byte>(4));
                    }
                }

                ImageFeatures[] features = CreateAffineFeatures();
                MatchesInfo[] matches = MatchAffine(features);
                StitcherCameraParams[] initial = CreateInitialCameras(features.Length, MatType.CV_32FC1);
                try
                {
                    Assert.True(adjusters[0].Apply(features, matches, initial, out StitcherCameraParams[] output));
                    try
                    {
                        Assert.Equal(initial.Length, output.Length);
                        Assert.Equal(initial[1].Focal, output[1].Focal, 8);
                        output[0].Rotation.SetTo(new Scalar(9));
                        Assert.NotEqual(9.0F, initial[0].Rotation.GetValue<float>(0));
                    }
                    finally
                    {
                        DisposeCameras(output);
                    }
                }
                finally
                {
                    DisposeCameras(initial);
                    DisposeMatches(matches);
                    DisposeFeatures(features);
                }
            }
            finally
            {
                foreach (BundleAdjusterBase adjuster in adjusters) adjuster.Dispose();
            }
        }

        [Fact]
        public void WaveGraphAndLargestComponentPreserveOwnershipWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            Mat[] rotations =
            {
                CreateMatrix(Rotation(0.02, 0.08, 0.01), MatType.CV_32FC1),
                CreateMatrix(Rotation(-0.03, -0.04, 0.02), MatType.CV_32FC1),
                CreateMatrix(Rotation(0.04, 0.02, -0.01), MatType.CV_32FC1)
            };
            try
            {
                StitchingMotion.WaveCorrect(rotations, WaveCorrectKind.Horizontal);
                Assert.All(rotations, value =>
                {
                    Assert.Equal(MatType.CV_32FC1, value.Type);
                    Assert.True(float.IsFinite(value.GetValue<float>(0)));
                });
            }
            finally
            {
                DisposeMats(rotations);
            }

            ImageFeatures[] features = CreateAffineFeatures();
            MatchesInfo[] matches;
            using (var matcher = new BestOf2NearestMatcher(matchConfidence: 0.8F))
            using (var mask = new Mat(3, 3, MatType.CV_8UC1, new Scalar(0)))
            {
                mask.CopyFrom(new byte[] { 0, 255, 0, 255, 0, 0, 0, 0, 0 });
                matches = matcher.Match(features, mask);
            }
            try
            {
                string graph = StitchingMotion.MatchesGraphAsString(
                    new[] { "图像-一.png", "scene-β.png", "unused.png" }, matches, 0.1F);
                Assert.Contains("图像-一.png", graph, StringComparison.Ordinal);
                Assert.Contains("scene-β.png", graph, StringComparison.Ordinal);

                int[] indices = StitchingMotion.LeaveBiggestComponent(
                    features, matches, 0.1F, out ImageFeatures[] selectedFeatures, out MatchesInfo[] selectedMatches);
                try
                {
                    Assert.Equal(new[] { 0, 1 }, indices);
                    Assert.Equal(2, selectedFeatures.Length);
                    Assert.Equal(4, selectedMatches.Length);
                    selectedFeatures[0].ImageIndex = 99;
                    Assert.NotEqual(99, features[0].ImageIndex);
                }
                finally
                {
                    DisposeMatches(selectedMatches);
                    DisposeFeatures(selectedFeatures);
                }
            }
            finally
            {
                DisposeMatches(matches);
                DisposeFeatures(features);
            }
        }

        [Fact]
        public void ValidationAndDisposalFailClosedWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var wrongHomography = new Mat(3, 3, MatType.CV_32FC1, new Scalar(1)))
            using (var wrongMask = new Mat(2, 2, MatType.CV_8UC1, new Scalar(1)))
            using (var adjuster = new NoBundleAdjuster())
            {
                Assert.Throws<ArgumentException>(() => StitchingMotion.FocalsFromHomography(
                    wrongHomography, out _, out _, out _, out _));
                Assert.Throws<ArgumentException>(() => adjuster.RefinementMask = wrongMask);
                Assert.Throws<ArgumentOutOfRangeException>(() => adjuster.ConfidenceThreshold = double.NaN);
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    adjuster.TerminationCriteria = new TermCriteria(TermCriteriaTypes.Count, 0, 0));
                Assert.Throws<ArgumentException>(() => StitchingMotion.WaveCorrect(
                    new[] { wrongHomography, wrongHomography }, WaveCorrectKind.Horizontal));
                Assert.Throws<ArgumentException>(() => StitchingMotion.MatchesGraphAsString(
                    Array.Empty<string>(), Array.Empty<MatchesInfo>(), 0));
            }

            var disposed = new HomographyBasedEstimator();
            disposed.Dispose();
            disposed.Dispose();
            Assert.True(disposed.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => disposed.Apply(
                Array.Empty<ImageFeatures>(), Array.Empty<MatchesInfo>(), out _));
        }

        private static Mat[] CreateRotatingHomographies()
        {
            return new[]
            {
                CreateMatrix(Homography(0.10, 0.20, 0.05)),
                CreateMatrix(Homography(-0.08, 0.15, -0.03)),
                CreateMatrix(Homography(0.12, -0.10, 0.07))
            };
        }

        private static ImageFeatures[] CreateProjectiveFeatures()
        {
            return CreateFeatures(new[]
            {
                Identity(),
                Homography(0.02, 0.04, 0.01),
                Homography(-0.015, 0.03, -0.01)
            });
        }

        private static ImageFeatures[] CreateAffineFeatures()
        {
            return CreateFeatures(new[]
            {
                Identity(),
                new double[] { 1, 0, 8, 0, 1, 4, 0, 0, 1 },
                new double[] { 1, 0, 16, 0, 1, 8, 0, 0, 1 }
            });
        }

        private static ImageFeatures[] CreateFeatures(double[][] transforms)
        {
            var points = new[]
            {
                new Point2d(180, 140), new Point2d(260, 140), new Point2d(340, 140), new Point2d(420, 140),
                new Point2d(180, 220), new Point2d(260, 220), new Point2d(340, 220), new Point2d(420, 220),
                new Point2d(220, 300), new Point2d(300, 300), new Point2d(380, 300), new Point2d(460, 300)
            };
            using (Mat descriptors = CreateDescriptors(points.Length))
            {
                var result = new ImageFeatures[transforms.Length];
                try
                {
                    for (int i = 0; i < transforms.Length; ++i)
                    {
                        var transformed = new KeyPoint[points.Length];
                        for (int j = 0; j < points.Length; ++j)
                        {
                            Point2d value = Transform(transforms[i], points[j]);
                            transformed[j] = new KeyPoint((float)value.X, (float)value.Y, 1);
                        }
                        result[i] = new ImageFeatures(i, new Size(640, 480), transformed, descriptors);
                    }
                    return result;
                }
                catch
                {
                    DisposeFeatures(result);
                    throw;
                }
            }
        }

        private static MatchesInfo[] MatchProjective(ImageFeatures[] features)
        {
            using (var matcher = new BestOf2NearestMatcher(matchConfidence: 0.8F))
            {
                return matcher.Match(features);
            }
        }

        private static MatchesInfo[] MatchAffine(ImageFeatures[] features)
        {
            using (var matcher = new AffineBestOf2NearestMatcher(fullAffine: false, matchConfidence: 0.8F))
            {
                return matcher.Match(features);
            }
        }

        private static Mat CreateDescriptors(int count)
        {
            var values = new float[count * 8];
            for (int row = 0; row < count; ++row)
            {
                for (int column = 0; column < 8; ++column)
                {
                    values[row * 8 + column] = row * row + column * 0.125F + (row % 3) * 17;
                }
            }
            var result = new Mat(count, 8, MatType.CV_32FC1);
            result.CopyFrom(values);
            return result;
        }

        private static StitcherCameraParams[] CreateInitialCameras(int count, int type)
        {
            var result = new StitcherCameraParams[count];
            try
            {
                for (int i = 0; i < count; ++i)
                {
                    result[i] = new StitcherCameraParams(
                        500, 1, 320, 240,
                        CreateMatrix(Identity(), type),
                        new Mat(3, 1, type, new Scalar(0)));
                }
                return result;
            }
            catch
            {
                DisposeCameras(result);
                throw;
            }
        }

        private static void AssertValidCamera(StitcherCameraParams camera)
        {
            Assert.True(double.IsFinite(camera.Focal));
            Assert.Equal(3, camera.Rotation.Rows);
            Assert.Equal(3, camera.Rotation.Cols);
            Assert.Equal(3, camera.Translation.Rows);
            Assert.Equal(1, camera.Translation.Cols);
        }

        private static Mat CreateMatrix(double[] values, int? type = null)
        {
            int matrixType = type ?? MatType.CV_64FC1;
            var result = new Mat(3, 3, matrixType);
            if (matrixType == MatType.CV_32FC1)
            {
                var converted = new float[values.Length];
                for (int i = 0; i < values.Length; ++i) converted[i] = (float)values[i];
                result.CopyFrom(converted);
            }
            else
            {
                result.CopyFrom(values);
            }
            return result;
        }

        private static double[] Homography(double ax, double ay, double az)
        {
            double[] intrinsic = { 500, 0, 0, 0, 500, 0, 0, 0, 1 };
            double[] inverse = { 0.002, 0, 0, 0, 0.002, 0, 0, 0, 1 };
            return Multiply(Multiply(intrinsic, Rotation(ax, ay, az)), inverse);
        }

        private static double[] Rotation(double ax, double ay, double az)
        {
            double sx = Math.Sin(ax), cx = Math.Cos(ax);
            double sy = Math.Sin(ay), cy = Math.Cos(ay);
            double sz = Math.Sin(az), cz = Math.Cos(az);
            double[] rx = { 1, 0, 0, 0, cx, -sx, 0, sx, cx };
            double[] ry = { cy, 0, sy, 0, 1, 0, -sy, 0, cy };
            double[] rz = { cz, -sz, 0, sz, cz, 0, 0, 0, 1 };
            return Multiply(Multiply(rz, ry), rx);
        }

        private static double[] Multiply(double[] left, double[] right)
        {
            var result = new double[9];
            for (int row = 0; row < 3; ++row)
            {
                for (int column = 0; column < 3; ++column)
                {
                    for (int inner = 0; inner < 3; ++inner)
                    {
                        result[row * 3 + column] += left[row * 3 + inner] * right[inner * 3 + column];
                    }
                }
            }
            return result;
        }

        private static Point2d Transform(double[] matrix, Point2d point)
        {
            double scale = matrix[6] * point.X + matrix[7] * point.Y + matrix[8];
            return new Point2d(
                (matrix[0] * point.X + matrix[1] * point.Y + matrix[2]) / scale,
                (matrix[3] * point.X + matrix[4] * point.Y + matrix[5]) / scale);
        }

        private static double[] Identity()
        {
            return new double[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
        }

        private static void DisposeCameras(StitcherCameraParams[] cameras)
        {
            if (cameras == null) return;
            foreach (StitcherCameraParams camera in cameras)
            {
                if (camera == null) continue;
                camera.Rotation.Dispose();
                camera.Translation.Dispose();
            }
        }

        private static void DisposeFeatures(ImageFeatures[] features)
        {
            if (features == null) return;
            foreach (ImageFeatures feature in features) feature?.Dispose();
        }

        private static void DisposeMatches(MatchesInfo[] matches)
        {
            if (matches == null) return;
            foreach (MatchesInfo match in matches) match?.Dispose();
        }

        private static void DisposeMats(Mat[] values)
        {
            foreach (Mat value in values) value.Dispose();
        }
    }
}
