using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class MultiviewCalibrationTests
    {
        [Fact]
        public void ResultObjectsSupportThreeCamerasAndExtendedShapes()
        {
            Mat[] cameraMatrices = CreateMatArray(3, 3, 3);
            Mat[] distCoeffs = CreateMatArray(3, 1, 5);
            Mat[] rotationVectors = CreateMatArray(3, 3, 1);
            Mat[] translationVectors = CreateMatArray(3, 3, 1);
            Mat[] rvecs0 = CreateMatArray(4, 3, 1);
            Mat[] tvecs0 = CreateMatArray(4, 3, 1);
            using (var initializationPairs = new Mat(2, 2, MatType.CV_32SC1))
            using (var perFrameErrors = new Mat(3, 4, MatType.CV_64FC1))
            {
                try
                {
                    var calibration = new MultiviewCalibrationResult(
                        0.25,
                        cameraMatrices,
                        distCoeffs,
                        rotationVectors,
                        translationVectors);
                    var extended = new MultiviewCalibrationExtendedResult(
                        calibration,
                        initializationPairs,
                        rvecs0,
                        tvecs0,
                        perFrameErrors);

                    Assert.Equal(3, calibration.CameraCount);
                    Assert.Equal(4, extended.FrameCount);
                    Assert.Same(cameraMatrices, calibration.CameraMatrices);
                    Assert.Same(initializationPairs, extended.InitializationPairs);
                    Assert.Contains("CameraCount=3", calibration.ToString(), StringComparison.Ordinal);
                    Assert.Contains("FrameCount=4", extended.ToString(), StringComparison.Ordinal);

                    Assert.Throws<ArgumentException>(() =>
                        new MultiviewCalibrationResult(
                            0.25,
                            new[] { cameraMatrices[0] },
                            new[] { distCoeffs[0] },
                            new[] { rotationVectors[0] },
                            new[] { translationVectors[0] }));
                    Assert.Throws<ArgumentException>(() =>
                        new MultiviewCalibrationResult(
                            0.25,
                            cameraMatrices,
                            new[] { distCoeffs[0], distCoeffs[1] },
                            rotationVectors,
                            translationVectors));
                }
                finally
                {
                    Dispose(cameraMatrices);
                    Dispose(distCoeffs);
                    Dispose(rotationVectors);
                    Dispose(translationVectors);
                    Dispose(rvecs0);
                    Dispose(tvecs0);
                }
            }
        }

        [Fact]
        public void CalibrateMultiviewValidatesShapesConnectivityAndFlagsBeforeNativeCall()
        {
            CreateData(
                out Point3f[][] objectPoints,
                out Point2f[][][] imagePoints,
                out Size[] imageSizes,
                out bool[][] detectionMask,
                out CameraModel[] cameraModels);

            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    new[] { imagePoints[0] },
                    new[] { imageSizes[0] },
                    new[] { detectionMask[0] },
                    new[] { cameraModels[0] }));

            Point2f[][][] mismatchedImagePoints = CopyImagePoints(imagePoints);
            mismatchedImagePoints[0][0] =
                CopyPrefix(mismatchedImagePoints[0][0], mismatchedImagePoints[0][0].Length - 1);
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    mismatchedImagePoints,
                    imageSizes,
                    detectionMask,
                    cameraModels));

            bool[][] disconnectedMask = CopyMask(detectionMask);
            for (int frame = 10; frame <= 13; ++frame)
            {
                disconnectedMask[1][frame] = false;
            }
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    disconnectedMask,
                    cameraModels));

            bool[][] cameraWithoutFrames = CopyMask(detectionMask);
            Array.Clear(cameraWithoutFrames[2], 0, cameraWithoutFrames[2].Length);
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    cameraWithoutFrames,
                    cameraModels));

            CameraModel[] invalidModels = (CameraModel[])cameraModels.Clone();
            invalidModels[1] = (CameraModel)2;
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    detectionMask,
                    invalidModels));

            CameraModel[] mixedModels = (CameraModel[])cameraModels.Clone();
            mixedModels[1] = CameraModel.Fisheye;
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    detectionMask,
                    mixedModels,
                    flags: CalibrationFlags.StereoRegistration));

            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    detectionMask,
                    cameraModels,
                    flags: CalibrationFlags.UseIntrinsicGuess));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateMultiviewExtended(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    detectionMask,
                    cameraModels,
                    flags: CalibrationFlags.UseExtrinsicGuess));
        }

        [Fact]
        public void CallerOwnedGuessInputsRequireInitializedMatrices()
        {
            CreateData(
                out Point3f[][] objectPoints,
                out Point2f[][][] imagePoints,
                out Size[] imageSizes,
                out bool[][] detectionMask,
                out CameraModel[] cameraModels);
            Mat[] cameraMatrices = CreateEmptyMatArray(3);
            Mat[] distCoeffs = CreateEmptyMatArray(3);
            Mat[] rotationVectors = CreateEmptyMatArray(3);
            Mat[] translationVectors = CreateEmptyMatArray(3);
            try
            {
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.CalibrateMultiview(
                        objectPoints,
                        imagePoints,
                        imageSizes,
                        detectionMask,
                        cameraModels,
                        cameraMatrices,
                        distCoeffs,
                        rotationVectors,
                        translationVectors,
                        flags: CalibrationFlags.UseIntrinsicGuess));

                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.CalibrateMultiview(
                        objectPoints,
                        imagePoints,
                        imageSizes,
                        detectionMask,
                        cameraModels,
                        cameraMatrices,
                        distCoeffs,
                        rotationVectors,
                        translationVectors,
                        flags: CalibrationFlags.UseExtrinsicGuess));
            }
            finally
            {
                Dispose(cameraMatrices);
                Dispose(distCoeffs);
                Dispose(rotationVectors);
                Dispose(translationVectors);
            }
        }

        [Fact]
        public void CalibrateMultiviewValidatesTopLevelDimensionsSizesFlagsAndCriteria()
        {
            CreateData(
                out Point3f[][] objectPoints,
                out Point2f[][][] imagePoints,
                out Size[] imageSizes,
                out bool[][] detectionMask,
                out CameraModel[] cameraModels);

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    null!,
                    imagePoints,
                    imageSizes,
                    detectionMask,
                    cameraModels));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    Array.Empty<Point3f[]>(),
                    imagePoints,
                    imageSizes,
                    detectionMask,
                    cameraModels));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    null!,
                    imageSizes,
                    detectionMask,
                    cameraModels));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    null!,
                    detectionMask,
                    cameraModels));

            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    new[] { imageSizes[0], imageSizes[1] },
                    detectionMask,
                    cameraModels));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    new[] { detectionMask[0], detectionMask[1] },
                    cameraModels));

            bool[][] shortMask = CopyMask(detectionMask);
            shortMask[1] = new bool[objectPoints.Length - 1];
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    shortMask,
                    cameraModels));

            Size[] invalidSizes = (Size[])imageSizes.Clone();
            invalidSizes[2] = new Size(0, imageSizes[2].Height);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    invalidSizes,
                    detectionMask,
                    cameraModels));

            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    detectionMask,
                    cameraModels,
                    new[] { CalibrationFlags.None, CalibrationFlags.None }));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    detectionMask,
                    cameraModels,
                    new[]
                    {
                        CalibrationFlags.None,
                        unchecked((CalibrationFlags)0x40000000),
                        CalibrationFlags.None
                    }));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    detectionMask,
                    cameraModels,
                    criteria: new TermCriteria((TermCriteriaTypes)0, 10, 1.0e-6)));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    detectionMask,
                    cameraModels,
                    criteria: new TermCriteria(TermCriteriaTypes.Count, 0, 0.0)));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.CalibrateMultiview(
                    objectPoints,
                    imagePoints,
                    imageSizes,
                    detectionMask,
                    cameraModels,
                    criteria: new TermCriteria(TermCriteriaTypes.Eps, 0, double.NaN)));
        }

        [Fact]
        public void CalibrateMultiviewValidatesCallerOwnedOutputs()
        {
            CreateData(
                out Point3f[][] objectPoints,
                out Point2f[][][] imagePoints,
                out Size[] imageSizes,
                out bool[][] detectionMask,
                out CameraModel[] cameraModels);
            Mat[] cameraMatrices = CreateEmptyMatArray(3);
            Mat[] distCoeffs = CreateEmptyMatArray(3);
            Mat[] rotationVectors = CreateEmptyMatArray(3);
            Mat[] translationVectors = CreateEmptyMatArray(3);
            try
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.CalibrateMultiview(
                        objectPoints,
                        imagePoints,
                        imageSizes,
                        detectionMask,
                        cameraModels,
                        null!,
                        distCoeffs,
                        rotationVectors,
                        translationVectors));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.CalibrateMultiview(
                        objectPoints,
                        imagePoints,
                        imageSizes,
                        detectionMask,
                        cameraModels,
                        new[] { cameraMatrices[0], cameraMatrices[1] },
                        distCoeffs,
                        rotationVectors,
                        translationVectors));

                var nullElement = new[] { cameraMatrices[0], null!, cameraMatrices[2] };
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.CalibrateMultiview(
                        objectPoints,
                        imagePoints,
                        imageSizes,
                        detectionMask,
                        cameraModels,
                        nullElement,
                        distCoeffs,
                        rotationVectors,
                        translationVectors));

                var disposed = new Mat();
                disposed.Dispose();
                var disposedElement = new[] { cameraMatrices[0], disposed, cameraMatrices[2] };
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.CalibrateMultiview(
                        objectPoints,
                        imagePoints,
                        imageSizes,
                        detectionMask,
                        cameraModels,
                        disposedElement,
                        distCoeffs,
                        rotationVectors,
                        translationVectors));

                using (var initializationPairs = new Mat())
                using (var perFrameErrors = new Mat())
                {
                    Mat[] rvecs0 = CreateEmptyMatArray(objectPoints.Length);
                    Mat[] tvecs0 = CreateEmptyMatArray(objectPoints.Length);
                    try
                    {
                        Assert.Throws<ArgumentNullException>(() =>
                            Calib3DCv2.CalibrateMultiviewExtended(
                                objectPoints,
                                imagePoints,
                                imageSizes,
                                detectionMask,
                                cameraModels,
                                cameraMatrices,
                                distCoeffs,
                                rotationVectors,
                                translationVectors,
                                null!,
                                rvecs0,
                                tvecs0,
                                perFrameErrors));
                        Assert.Throws<ArgumentException>(() =>
                            Calib3DCv2.CalibrateMultiviewExtended(
                                objectPoints,
                                imagePoints,
                                imageSizes,
                                detectionMask,
                                cameraModels,
                                cameraMatrices,
                                distCoeffs,
                                rotationVectors,
                                translationVectors,
                                initializationPairs,
                                new Mat[objectPoints.Length - 1],
                                tvecs0,
                                perFrameErrors));
                    }
                    finally
                    {
                        Dispose(rvecs0);
                        Dispose(tvecs0);
                    }
                }
            }
            finally
            {
                Dispose(cameraMatrices);
                Dispose(distCoeffs);
                Dispose(rotationVectors);
                Dispose(translationVectors);
            }
        }

        [Fact]
        public void CalibrateMultiviewRunsOnPartialVisibilityDataWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CreateData(
                out Point3f[][] objectPoints,
                out Point2f[][][] imagePoints,
                out Size[] imageSizes,
                out bool[][] detectionMask,
                out CameraModel[] cameraModels);
            MultiviewCalibrationResult result = Calib3DCv2.CalibrateMultiview(
                objectPoints,
                imagePoints,
                imageSizes,
                detectionMask,
                cameraModels);
            try
            {
                Assert.True(double.IsFinite(result.ReprojectionError));
                Assert.InRange(result.ReprojectionError, 0.0, 0.25);
                Assert.Equal(3, result.CameraCount);
                Assert.Contains("CameraCount=3", result.ToString(), StringComparison.Ordinal);
                for (int camera = 0; camera < result.CameraCount; ++camera)
                {
                    AssertShape(result.CameraMatrices[camera], 3, 3);
                    Assert.False(result.DistCoeffs[camera].Empty);
                    AssertShape(result.RotationVectors[camera], 3, 1);
                    AssertShape(result.TranslationVectors[camera], 3, 1);
                    AssertAllFinite(result.CameraMatrices[camera]);
                    AssertAllFinite(result.DistCoeffs[camera]);
                    AssertAllFinite(result.RotationVectors[camera]);
                    AssertAllFinite(result.TranslationVectors[camera]);
                }
                AssertNearZero(result.RotationVectors[0], 1.0e-9);
                AssertNearZero(result.TranslationVectors[0], 1.0e-9);
            }
            finally
            {
                Dispose(result);
            }
        }

        [Fact]
        public void CalibrateMultiviewExtendedPreservesInvisibleFrameSemanticsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CreateData(
                out Point3f[][] objectPoints,
                out Point2f[][][] imagePoints,
                out Size[] imageSizes,
                out bool[][] detectionMask,
                out CameraModel[] cameraModels);
            MultiviewCalibrationExtendedResult result = Calib3DCv2.CalibrateMultiviewExtended(
                objectPoints,
                imagePoints,
                imageSizes,
                detectionMask,
                cameraModels);
            try
            {
                Assert.True(double.IsFinite(result.Calibration.ReprojectionError));
                Assert.InRange(result.Calibration.ReprojectionError, 0.0, 0.25);
                Assert.Equal(objectPoints.Length, result.FrameCount);
                AssertShape(result.InitializationPairs, 2, 2);
                AssertShape(result.PerFrameErrors, 3, objectPoints.Length);

                int[] pairs = result.InitializationPairs.ToArray<int>();
                Assert.Equal(4, pairs.Length);
                foreach (int camera in pairs)
                {
                    Assert.InRange(camera, 0, 2);
                }

                double[] errors = result.PerFrameErrors.ToArray<double>();
                for (int camera = 0; camera < detectionMask.Length; ++camera)
                {
                    for (int frame = 0; frame < objectPoints.Length; ++frame)
                    {
                        double error = errors[camera * objectPoints.Length + frame];
                        if (detectionMask[camera][frame])
                        {
                            Assert.True(double.IsFinite(error));
                            Assert.True(error >= 0.0);
                        }
                        else
                        {
                            Assert.Equal(-1.0, error);
                        }
                    }
                }

                for (int frame = 0; frame < objectPoints.Length - 1; ++frame)
                {
                    AssertShape(result.Rvecs0[frame], 3, 1);
                    AssertShape(result.Tvecs0[frame], 3, 1);
                    AssertAllFinite(result.Rvecs0[frame]);
                    AssertAllFinite(result.Tvecs0[frame]);
                }
                Assert.True(result.Rvecs0[objectPoints.Length - 1].Empty);
                Assert.True(result.Tvecs0[objectPoints.Length - 1].Empty);
            }
            finally
            {
                Dispose(result);
            }
        }

        [Fact]
        public void CalibrateMultiviewAcceptsMissingPointSentinelsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CreateData(
                out Point3f[][] objectPoints,
                out Point2f[][][] imagePoints,
                out Size[] imageSizes,
                out bool[][] detectionMask,
                out CameraModel[] cameraModels);
            Point2f[][][] pointsWithSentinels = CopyImagePoints(imagePoints);
            pointsWithSentinels[0][0][0] = new Point2f(-1.0F, -1.0F);

            MultiviewCalibrationResult result = Calib3DCv2.CalibrateMultiview(
                objectPoints,
                pointsWithSentinels,
                imageSizes,
                detectionMask,
                cameraModels);
            try
            {
                Assert.True(double.IsFinite(result.ReprojectionError));
                Assert.True(result.ReprojectionError >= 0.0);
                Assert.Equal(3, result.CameraCount);
                for (int camera = 0; camera < result.CameraCount; ++camera)
                {
                    AssertShape(result.CameraMatrices[camera], 3, 3);
                    AssertShape(result.RotationVectors[camera], 3, 1);
                    AssertShape(result.TranslationVectors[camera], 3, 1);
                    AssertAllFinite(result.CameraMatrices[camera]);
                    AssertAllFinite(result.DistCoeffs[camera]);
                    AssertAllFinite(result.RotationVectors[camera]);
                    AssertAllFinite(result.TranslationVectors[camera]);
                }
            }
            finally
            {
                Dispose(result);
            }
        }

        private static void CreateData(
            out Point3f[][] objectPoints,
            out Point2f[][][] imagePoints,
            out Size[] imageSizes,
            out bool[][] detectionMask,
            out CameraModel[] cameraModels)
        {
            CalibrationTestData.CreateSyntheticMultiviewCalibrationData(
                out objectPoints,
                out imagePoints,
                out imageSizes,
                out detectionMask,
                out cameraModels);
        }

        private static Mat[] CreateMatArray(int count, int rows, int cols)
        {
            var values = new Mat[count];
            for (int i = 0; i < values.Length; ++i)
            {
                values[i] = new Mat(rows, cols, MatType.CV_64FC1, new Scalar(0.0));
            }
            return values;
        }

        private static Mat[] CreateEmptyMatArray(int count)
        {
            var values = new Mat[count];
            for (int i = 0; i < values.Length; ++i)
            {
                values[i] = new Mat();
            }
            return values;
        }

        private static Point2f[][][] CopyImagePoints(Point2f[][][] source)
        {
            var result = new Point2f[source.Length][][];
            for (int camera = 0; camera < source.Length; ++camera)
            {
                result[camera] = new Point2f[source[camera].Length][];
                for (int frame = 0; frame < source[camera].Length; ++frame)
                {
                    result[camera][frame] = (Point2f[])source[camera][frame].Clone();
                }
            }
            return result;
        }

        private static bool[][] CopyMask(bool[][] source)
        {
            var result = new bool[source.Length][];
            for (int camera = 0; camera < source.Length; ++camera)
            {
                result[camera] = (bool[])source[camera].Clone();
            }
            return result;
        }

        private static T[] CopyPrefix<T>(T[] source, int count)
        {
            var result = new T[count];
            Array.Copy(source, result, count);
            return result;
        }

        private static void AssertShape(Mat value, int rows, int cols)
        {
            Assert.Equal(rows, value.Rows);
            Assert.Equal(cols, value.Cols);
            Assert.Equal(1, value.Channels);
        }

        private static void AssertAllFinite(Mat value)
        {
            double[] values = value.ToArray<double>();
            Assert.NotEmpty(values);
            foreach (double item in values)
            {
                Assert.True(double.IsFinite(item));
            }
        }

        private static void AssertNearZero(Mat value, double tolerance)
        {
            foreach (double item in value.ToArray<double>())
            {
                Assert.InRange(Math.Abs(item), 0.0, tolerance);
            }
        }

        private static void Dispose(MultiviewCalibrationResult result)
        {
            Dispose(result.CameraMatrices);
            Dispose(result.DistCoeffs);
            Dispose(result.RotationVectors);
            Dispose(result.TranslationVectors);
        }

        private static void Dispose(MultiviewCalibrationExtendedResult result)
        {
            Dispose(result.Calibration);
            result.InitializationPairs.Dispose();
            Dispose(result.Rvecs0);
            Dispose(result.Tvecs0);
            result.PerFrameErrors.Dispose();
        }

        private static void Dispose(Mat[] values)
        {
            foreach (Mat value in values)
            {
                value?.Dispose();
            }
        }
    }
}
