using System;
using System.Collections.Generic;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class FisheyeGeometryTests
    {
        [Fact]
        public void FisheyeGeometryValidatesInputs()
        {
            CalibrationTestData.CreateSyntheticFisheyePoseData(
                out Point3f[] objectPoints,
                out Point2f[] imagePoints,
                out Mat cameraMatrix,
                out Mat distCoeffs,
                out Mat expectedRvec,
                out Mat expectedTvec);
            using (cameraMatrix)
            using (distCoeffs)
            using (expectedRvec)
            using (expectedTvec)
            using (Mat objectPointMat = CreatePointMat(objectPoints))
            using (Mat imagePointMat = CreatePointMat(imagePoints))
            using (var output = new Mat())
            using (var invalidCameraMatrix = new Mat(2, 3, MatType.CV_64FC1))
            using (var invalidDistCoeffs = new Mat(5, 1, MatType.CV_64FC1))
            using (var invalidObjectPointDepth = new Mat(1, 1, MatType.CV_32SC3))
            using (var invalidRvecDepth = new Mat(3, 1, MatType.CV_32SC1))
            using (var invalidDistortionDepth = new Mat(4, 1, MatType.CV_32SC1))
            using (var jacobian = new Mat())
            using (var invalidRectification = new Mat(2, 2, MatType.CV_64FC1))
            using (var invalidProjection = new Mat(2, 3, MatType.CV_64FC1))
            using (var rvec = new Mat())
            using (var tvec = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.FisheyeProjectPoints(
                        (Mat)null!,
                        expectedRvec,
                        expectedTvec,
                        cameraMatrix,
                        distCoeffs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeProjectPoints(
                        invalidObjectPointDepth,
                        expectedRvec,
                        expectedTvec,
                        cameraMatrix,
                        distCoeffs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeProjectPoints(
                        objectPointMat,
                        invalidRvecDepth,
                        expectedTvec,
                        cameraMatrix,
                        distCoeffs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeProjectPoints(
                        objectPointMat,
                        expectedRvec,
                        expectedTvec,
                        cameraMatrix,
                        invalidDistortionDepth));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeProjectPoints(
                        objectPointMat,
                        expectedRvec,
                        expectedTvec,
                        invalidCameraMatrix,
                        distCoeffs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeProjectPoints(
                        objectPointMat,
                        expectedRvec,
                        expectedTvec,
                        cameraMatrix,
                        invalidDistCoeffs));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.FisheyeProjectPoints(
                        objectPointMat,
                        expectedRvec,
                        expectedTvec,
                        cameraMatrix,
                        distCoeffs,
                        double.NaN));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeProjectPoints(
                        objectPointMat,
                        expectedRvec,
                        expectedTvec,
                        cameraMatrix,
                        distCoeffs,
                        objectPointMat));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeProjectPoints(
                        objectPointMat,
                        expectedRvec,
                        expectedTvec,
                        cameraMatrix,
                        distCoeffs,
                        output,
                        jacobian: cameraMatrix));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeProjectPoints(
                        objectPointMat,
                        expectedRvec,
                        expectedTvec,
                        cameraMatrix,
                        distCoeffs,
                        output,
                        jacobian: output));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeUndistortPoints(
                        imagePointMat,
                        cameraMatrix,
                        distCoeffs,
                        r: invalidRectification));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeUndistortPoints(
                        imagePointMat,
                        cameraMatrix,
                        distCoeffs,
                        p: invalidProjection));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.FisheyeEstimateNewCameraMatrixForUndistortRectify(
                        cameraMatrix,
                        distCoeffs,
                        new Size(0, CalibrationTestData.ImageSize.Height)));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.FisheyeEstimateNewCameraMatrixForUndistortRectify(
                        cameraMatrix,
                        distCoeffs,
                        CalibrationTestData.ImageSize,
                        balance: 1.01));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.FisheyeEstimateNewCameraMatrixForUndistortRectify(
                        cameraMatrix,
                        distCoeffs,
                        CalibrationTestData.ImageSize,
                        fovScale: 0.0));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.FisheyeEstimateNewCameraMatrixForUndistortRectify(
                        cameraMatrix,
                        distCoeffs,
                        CalibrationTestData.ImageSize,
                        newSize: new Size(0, 480)));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeSolvePnP(
                        objectPoints,
                        new Point2f[imagePoints.Length - 1],
                        cameraMatrix,
                        distCoeffs,
                        rvec,
                        tvec));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.FisheyeSolvePnP(
                        objectPoints,
                        imagePoints,
                        cameraMatrix,
                        distCoeffs,
                        rvec,
                        tvec,
                        flags: (SolvePnPFlags)99));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.FisheyeSolvePnP(
                        objectPoints,
                        imagePoints,
                        cameraMatrix,
                        distCoeffs,
                        rvec,
                        tvec,
                        criteria: new TermCriteria((TermCriteriaTypes)0, 10, 1.0e-8)));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.FisheyeSolvePnPRansac(
                        objectPoints,
                        imagePoints,
                        cameraMatrix,
                        distCoeffs,
                        rvec,
                        tvec,
                        iterationsCount: 0));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.FisheyeSolvePnPRansac(
                        objectPoints,
                        imagePoints,
                        cameraMatrix,
                        distCoeffs,
                        rvec,
                        tvec,
                        reprojectionError: float.PositiveInfinity));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.FisheyeSolvePnPRansac(
                        objectPoints,
                        imagePoints,
                        cameraMatrix,
                        distCoeffs,
                        rvec,
                        tvec,
                        confidence: 1.0));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeProjectPoints(
                        Array.Empty<Point3f>(),
                        expectedRvec,
                        expectedTvec,
                        cameraMatrix,
                        distCoeffs));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeDistortPoints(
                        ReadOnlySpan<Point2f>.Empty,
                        cameraMatrix,
                        distCoeffs));
#endif
            }

            var disposedCameraMatrix = new Mat(3, 3, MatType.CV_64FC1);
            disposedCameraMatrix.Dispose();
            using (var points = new Mat(1, 1, MatType.CV_32FC2))
            using (var distortion = new Mat(4, 1, MatType.CV_64FC1))
            {
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.FisheyeDistortPoints(
                        points,
                        disposedCameraMatrix,
                        distortion));
            }
        }

        [Fact]
        public void FisheyeProjectPointsMatchesFixtureAndReturnsJacobianWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CalibrationTestData.CreateSyntheticFisheyePoseData(
                out Point3f[] objectPoints,
                out Point2f[] expectedImagePoints,
                out Mat cameraMatrix,
                out Mat distCoeffs,
                out Mat rvec,
                out Mat tvec);
            using (cameraMatrix)
            using (distCoeffs)
            using (rvec)
            using (tvec)
            using (Mat objectPointMat = CreatePointMat(objectPoints))
            using (var imagePointMat = new Mat())
            using (var jacobian = new Mat())
            using (Mat arrayResult = Calib3DCv2.FisheyeProjectPoints(
                objectPoints,
                rvec,
                tvec,
                cameraMatrix,
                distCoeffs))
#if NETCOREAPP3_1_OR_GREATER
            using (Mat spanResult = Calib3DCv2.FisheyeProjectPoints(
                objectPoints.AsSpan(),
                rvec,
                tvec,
                cameraMatrix,
                distCoeffs))
#endif
            {
                Calib3DCv2.FisheyeProjectPoints(
                    objectPointMat,
                    rvec,
                    tvec,
                    cameraMatrix,
                    distCoeffs,
                    imagePointMat,
                    jacobian: jacobian);

                AssertPointMatNear(expectedImagePoints, imagePointMat, 2.0e-4);
                AssertPointMatsNear(imagePointMat, arrayResult, 1.0e-6);
#if NETCOREAPP3_1_OR_GREATER
                AssertPointMatsNear(imagePointMat, spanResult, 1.0e-6);
#endif
                Assert.Equal(objectPoints.Length * 2, jacobian.Rows);
                Assert.Equal(15, jacobian.Cols);
                for (int i = 0; i < jacobian.Rows * jacobian.Cols; ++i)
                {
                    Assert.True(double.IsFinite(jacobian.GetValue<double>(i)));
                }
            }
        }

        [Fact]
        public void FisheyeDistortAndUndistortRoundTripCoordinatesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CalibrationTestData.CreateSyntheticFisheyePoseData(
                out _,
                out _,
                out Mat cameraMatrix,
                out Mat distCoeffs,
                out Mat rvec,
                out Mat tvec);
            rvec.Dispose();
            tvec.Dispose();
            Point2f[] normalizedPoints =
            {
                new Point2f(-0.42F, -0.28F),
                new Point2f(-0.18F, 0.23F),
                new Point2f(0.0F, 0.0F),
                new Point2f(0.31F, -0.17F),
                new Point2f(0.48F, 0.26F)
            };

            using (cameraMatrix)
            using (distCoeffs)
            using (Mat distorted = Calib3DCv2.FisheyeDistortPoints(
                normalizedPoints,
                cameraMatrix,
                distCoeffs))
            using (Mat normalizedRoundTrip = Calib3DCv2.FisheyeUndistortPoints(
                distorted,
                cameraMatrix,
                distCoeffs))
            using (Mat undistortedCameraMatrix = cameraMatrix.Clone())
            {
                undistortedCameraMatrix.SetValue(0, 510.0);
                undistortedCameraMatrix.SetValue(2, 302.0);
                undistortedCameraMatrix.SetValue(4, 505.0);
                undistortedCameraMatrix.SetValue(5, 236.0);
                Point2f[] undistortedPixels = ToPixelPoints(
                    normalizedPoints,
                    undistortedCameraMatrix);

                using (Mat distortedPixels =
                    Calib3DCv2.FisheyeDistortPointsWithCameraMatrix(
                        undistortedPixels,
                        undistortedCameraMatrix,
                        cameraMatrix,
                        distCoeffs))
                using (Mat pixelRoundTrip = Calib3DCv2.FisheyeUndistortPoints(
                    distortedPixels,
                    cameraMatrix,
                    distCoeffs,
                    p: undistortedCameraMatrix))
#if NETCOREAPP3_1_OR_GREATER
                using (Mat spanDistorted = Calib3DCv2.FisheyeDistortPoints(
                    normalizedPoints.AsSpan(),
                    cameraMatrix,
                    distCoeffs))
                using (Mat spanUndistorted = Calib3DCv2.FisheyeUndistortPoints(
                    ToPointArray(distortedPixels).AsSpan(),
                    cameraMatrix,
                    distCoeffs,
                    p: undistortedCameraMatrix))
#endif
                {
                    AssertPointMatNear(normalizedPoints, normalizedRoundTrip, 2.0e-5);
                    AssertPointMatNear(undistortedPixels, pixelRoundTrip, 2.0e-3);
#if NETCOREAPP3_1_OR_GREATER
                    AssertPointMatsNear(distorted, spanDistorted, 1.0e-6);
                    AssertPointMatsNear(pixelRoundTrip, spanUndistorted, 2.0e-5);
#endif
                }
            }
        }

        [Fact]
        public void FisheyeNewCameraMatrixIsFiniteAndRespondsToBalanceWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CalibrationTestData.CreateSyntheticFisheyePoseData(
                out _,
                out _,
                out Mat cameraMatrix,
                out Mat distCoeffs,
                out Mat rvec,
                out Mat tvec);
            rvec.Dispose();
            tvec.Dispose();
            using (cameraMatrix)
            using (distCoeffs)
            using (Mat cropped = Calib3DCv2.FisheyeEstimateNewCameraMatrixForUndistortRectify(
                cameraMatrix,
                distCoeffs,
                CalibrationTestData.ImageSize,
                balance: 0.0))
            using (Mat fullView = Calib3DCv2.FisheyeEstimateNewCameraMatrixForUndistortRectify(
                cameraMatrix,
                distCoeffs,
                CalibrationTestData.ImageSize,
                balance: 1.0))
            {
                Assert.Equal(3, cropped.Rows);
                Assert.Equal(3, cropped.Cols);
                Assert.Equal(3, fullView.Rows);
                Assert.Equal(3, fullView.Cols);
                AssertFinite(cropped);
                AssertFinite(fullView);
                Assert.True(Math.Abs(cropped.GetValue<double>(0) - fullView.GetValue<double>(0)) > 1.0e-6);
            }
        }

        [Fact]
        public void FisheyePnPAndRansacRecoverKnownPoseWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CalibrationTestData.CreateSyntheticFisheyePoseData(
                out Point3f[] objectPoints,
                out Point2f[] imagePoints,
                out Mat cameraMatrix,
                out Mat distCoeffs,
                out Mat expectedRvec,
                out Mat expectedTvec);
            using (cameraMatrix)
            using (distCoeffs)
            using (expectedRvec)
            using (expectedTvec)
            using (var rvec = new Mat())
            using (var tvec = new Mat())
            using (var spanRvec = new Mat())
            using (var spanTvec = new Mat())
            {
                Assert.True(Calib3DCv2.FisheyeSolvePnP(
                    objectPoints,
                    imagePoints,
                    cameraMatrix,
                    distCoeffs,
                    rvec,
                    tvec));
                AssertVectorNear(expectedRvec, rvec, 2.0e-5);
                AssertVectorNear(expectedTvec, tvec, 2.0e-5);

#if NETCOREAPP3_1_OR_GREATER
                Assert.True(Calib3DCv2.FisheyeSolvePnP(
                    objectPoints.AsSpan(),
                    imagePoints.AsSpan(),
                    cameraMatrix,
                    distCoeffs,
                    spanRvec,
                    spanTvec));
                AssertVectorNear(rvec, spanRvec, 1.0e-8);
                AssertVectorNear(tvec, spanTvec, 1.0e-8);
#endif

                using (Mat guessRvec = expectedRvec.Clone())
                using (Mat guessTvec = expectedTvec.Clone())
                {
                    guessRvec.SetValue(0, guessRvec.GetValue<double>(0) + 0.01);
                    guessTvec.SetValue(1, guessTvec.GetValue<double>(1) - 0.01);
                    Assert.True(Calib3DCv2.FisheyeSolvePnP(
                        objectPoints,
                        imagePoints,
                        cameraMatrix,
                        distCoeffs,
                        guessRvec,
                        guessTvec,
                        useExtrinsicGuess: true));
                    AssertVectorNear(expectedRvec, guessRvec, 2.0e-5);
                    AssertVectorNear(expectedTvec, guessTvec, 2.0e-5);
                }

                Point2f[] imagePointsWithOutliers = (Point2f[])imagePoints.Clone();
                int[] outlierIndices = { 2, 11, 24 };
                foreach (int index in outlierIndices)
                {
                    imagePointsWithOutliers[index] = new Point2f(
                        imagePointsWithOutliers[index].X + 80.0F,
                        imagePointsWithOutliers[index].Y - 65.0F);
                }

                using (var ransacRvec = new Mat())
                using (var ransacTvec = new Mat())
                using (var inliers = new Mat())
                {
                    Assert.True(Calib3DCv2.FisheyeSolvePnPRansac(
                        objectPoints,
                        imagePointsWithOutliers,
                        cameraMatrix,
                        distCoeffs,
                        ransacRvec,
                        ransacTvec,
                        iterationsCount: 300,
                        reprojectionError: 2.0F,
                        confidence: 0.999,
                        inliers: inliers));

                    AssertVectorNear(expectedRvec, ransacRvec, 1.0e-3);
                    AssertVectorNear(expectedTvec, ransacTvec, 1.0e-3);
                    var actualInliers = new HashSet<int>();
                    for (int i = 0; i < inliers.Rows * inliers.Cols; ++i)
                    {
                        actualInliers.Add(inliers.GetValue<int>(i));
                    }
                    Assert.Equal(objectPoints.Length - outlierIndices.Length, actualInliers.Count);
                    foreach (int outlierIndex in outlierIndices)
                    {
                        Assert.DoesNotContain(outlierIndex, actualInliers);
                    }
                }
            }
        }

        private static Mat CreatePointMat(Point3f[] points)
        {
            var result = new Mat(points.Length, 1, MatType.CV_32FC3);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue(i, points[i]);
            }
            return result;
        }

        private static Mat CreatePointMat(Point2f[] points)
        {
            var result = new Mat(points.Length, 1, MatType.CV_32FC2);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue(i, points[i]);
            }
            return result;
        }

        private static Point2f[] ToPixelPoints(Point2f[] normalizedPoints, Mat cameraMatrix)
        {
            double fx = cameraMatrix.GetValue<double>(0);
            double cx = cameraMatrix.GetValue<double>(2);
            double fy = cameraMatrix.GetValue<double>(4);
            double cy = cameraMatrix.GetValue<double>(5);
            var result = new Point2f[normalizedPoints.Length];
            for (int i = 0; i < normalizedPoints.Length; ++i)
            {
                result[i] = new Point2f(
                    (float)(fx * normalizedPoints[i].X + cx),
                    (float)(fy * normalizedPoints[i].Y + cy));
            }
            return result;
        }

        private static void AssertPointMatNear(Point2f[] expected, Mat actual, double tolerance)
        {
            Assert.Equal(expected.Length, actual.Rows * actual.Cols);
            for (int i = 0; i < expected.Length; ++i)
            {
                Point2f value = GetPoint(actual, i);
                Assert.InRange(Math.Abs(expected[i].X - value.X), 0.0, tolerance);
                Assert.InRange(Math.Abs(expected[i].Y - value.Y), 0.0, tolerance);
            }
        }

        private static void AssertPointMatsNear(Mat expected, Mat actual, double tolerance)
        {
            Assert.Equal(expected.Rows * expected.Cols, actual.Rows * actual.Cols);
            int count = expected.Rows * expected.Cols;
            for (int i = 0; i < count; ++i)
            {
                Point2f expectedValue = GetPoint(expected, i);
                Point2f actualValue = GetPoint(actual, i);
                Assert.InRange(Math.Abs(expectedValue.X - actualValue.X), 0.0, tolerance);
                Assert.InRange(Math.Abs(expectedValue.Y - actualValue.Y), 0.0, tolerance);
            }
        }

        private static Point2f[] ToPointArray(Mat points)
        {
            int count = points.Rows * points.Cols;
            var result = new Point2f[count];
            for (int i = 0; i < count; ++i)
            {
                result[i] = GetPoint(points, i);
            }
            return result;
        }

        private static Point2f GetPoint(Mat points, int index)
        {
            if (points.Type == MatType.CV_32FC2)
            {
                return points.GetValue<Point2f>(index);
            }
            if (points.Type == MatType.CV_64FC2)
            {
                Point2d value = points.GetValue<Point2d>(index);
                return new Point2f((float)value.X, (float)value.Y);
            }
            throw new InvalidOperationException($"Expected a two-channel point matrix, got type {points.Type}.");
        }

        private static void AssertVectorNear(Mat expected, Mat actual, double tolerance)
        {
            Assert.Equal(3, actual.Rows * actual.Cols);
            for (int i = 0; i < 3; ++i)
            {
                Assert.InRange(
                    Math.Abs(expected.GetValue<double>(i) - actual.GetValue<double>(i)),
                    0.0,
                    tolerance);
            }
        }

        private static void AssertFinite(Mat value)
        {
            for (int i = 0; i < value.Rows * value.Cols; ++i)
            {
                Assert.True(double.IsFinite(value.GetValue<double>(i)));
            }
        }
    }
}
