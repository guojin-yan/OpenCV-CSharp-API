using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class HomographyDecompositionTests
    {
        [Fact]
        public void HomographyDecompositionValidatesInputs()
        {
            using Mat homography = CreateRegressionHomography();
            using Mat cameraMatrix = CreateRegressionCameraMatrix();
            using var invalidShape = new Mat(2, 3, MatType.CV_64FC1);
            using var invalidDepth = new Mat(3, 3, MatType.CV_32SC1);
            Mat[] rotations = CreateOutputMats();
            Mat[] translations = CreateOutputMats();
            Mat[] normals = CreateOutputMats();
            Mat[] filterRotations = CreateFilterRotations();
            Mat[] filterNormals = CreateFilterNormals();
            Point2f[] before = CreateFilterPoints();
            Point2f[] after = CreateFilterPoints();

            try
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.DecomposeHomographyMat(
                        null!,
                        cameraMatrix,
                        rotations,
                        translations,
                        normals));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.DecomposeHomographyMat(
                        invalidShape,
                        cameraMatrix,
                        rotations,
                        translations,
                        normals));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.DecomposeHomographyMat(
                        invalidDepth,
                        cameraMatrix,
                        rotations,
                        translations,
                        normals));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.DecomposeHomographyMat(
                        homography,
                        cameraMatrix,
                        new Mat[3],
                        translations,
                        normals));

                Mat savedTranslation = translations[0];
                translations[0] = rotations[0];
                try
                {
                    Assert.Throws<ArgumentException>(() =>
                        Calib3DCv2.DecomposeHomographyMat(
                            homography,
                            cameraMatrix,
                            rotations,
                            translations,
                            normals));
                }
                finally
                {
                    translations[0] = savedTranslation;
                }

                rotations[0].Dispose();
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.DecomposeHomographyMat(
                        homography,
                        cameraMatrix,
                        rotations,
                        translations,
                        normals));
                rotations[0] = new Mat();

                using Mat beforeMat = Calib3DCv2.ToPointMat(before);
                using Mat afterMat = Calib3DCv2.ToPointMat(after);
                using var possibleSolutions = new Mat();
                using var invalidPoints = new Mat(2, 1, MatType.CV_64FC2);
                using var invalidMaskType = new Mat(2, 1, MatType.CV_32SC1);
                using var invalidMaskCount = new Mat(1, 1, MatType.CV_8UC1);

                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
                        Array.Empty<Mat>(),
                        filterNormals,
                        beforeMat,
                        afterMat,
                        possibleSolutions));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
                        filterRotations,
                        filterNormals[..3],
                        beforeMat,
                        afterMat,
                        possibleSolutions));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
                        filterRotations,
                        filterNormals,
                        invalidPoints,
                        afterMat,
                        possibleSolutions));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
                        filterRotations,
                        filterNormals,
                        beforeMat,
                        afterMat,
                        possibleSolutions,
                        invalidMaskType));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
                        filterRotations,
                        filterNormals,
                        beforeMat,
                        afterMat,
                        possibleSolutions,
                        invalidMaskCount));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
                        filterRotations,
                        filterNormals,
                        Array.Empty<Point2f>(),
                        Array.Empty<Point2f>()));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
                        filterRotations,
                        filterNormals,
                        before,
                        after[..1]));
            }
            finally
            {
                DisposeMats(rotations);
                DisposeMats(translations);
                DisposeMats(normals);
                DisposeMats(filterRotations);
                DisposeMats(filterNormals);
            }
        }

        [Fact]
        public void DecomposeHomographyMatchesUpstreamRegressionAcrossOwnershipModesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using Mat homography = CreateRegressionHomography();
            using Mat cameraMatrix = CreateRegressionCameraMatrix();
            Mat[] callerRotations = CreateOutputMats();
            Mat[] callerTranslations = CreateOutputMats();
            Mat[] callerNormals = CreateOutputMats();

            try
            {
                int callerCount = Calib3DCv2.DecomposeHomographyMat(
                    homography,
                    cameraMatrix,
                    callerRotations,
                    callerTranslations,
                    callerNormals);
                Assert.InRange(callerCount, 1, 4);
                AssertSolutionShapes(
                    callerRotations,
                    callerTranslations,
                    callerNormals,
                    callerCount);
                AssertContainsExpectedRegressionMotion(
                    callerRotations,
                    callerTranslations,
                    callerNormals,
                    callerCount);

                int ownedCount = Calib3DCv2.DecomposeHomographyMat(
                    homography,
                    cameraMatrix,
                    out Mat[] ownedRotations,
                    out Mat[] ownedTranslations,
                    out Mat[] ownedNormals);
                try
                {
                    Assert.Equal(callerCount, ownedCount);
                    Assert.Equal(ownedCount, ownedRotations.Length);
                    Assert.Equal(ownedCount, ownedTranslations.Length);
                    Assert.Equal(ownedCount, ownedNormals.Length);
                    for (int i = 0; i < ownedCount; ++i)
                    {
                        AssertMatrixNear(
                            callerRotations[i],
                            ownedRotations[i],
                            1.0e-12);
                        AssertMatrixNear(
                            callerTranslations[i],
                            ownedTranslations[i],
                            1.0e-12);
                        AssertMatrixNear(
                            callerNormals[i],
                            ownedNormals[i],
                            1.0e-12);
                    }
                }
                finally
                {
                    DisposeMats(ownedRotations);
                    DisposeMats(ownedTranslations);
                    DisposeMats(ownedNormals);
                }
            }
            finally
            {
                DisposeMats(callerRotations);
                DisposeMats(callerTranslations);
                DisposeMats(callerNormals);
            }
        }

        [Fact]
        public void DecomposeHomographyReturnsProperRotationsForIssue4978WhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using Mat cameraMatrix = CreateMatrix(
                3,
                3,
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);
            using Mat homography = CreateMatrix(
                3,
                3,
                -0.102896, 0.270191, -0.0031153,
                0.0406387, 1.19569, -0.0120456,
                0.445351, 0.0410889, 1.0);

            int solutionCount = Calib3DCv2.DecomposeHomographyMat(
                homography,
                cameraMatrix,
                out Mat[] rotations,
                out Mat[] translations,
                out Mat[] normals);
            try
            {
                Assert.InRange(solutionCount, 1, 4);
                for (int i = 0; i < solutionCount; ++i)
                {
                    double determinant = Determinant3x3(rotations[i]);
                    Assert.InRange(determinant, 0.99, 1.01);
                }
            }
            finally
            {
                DisposeMats(rotations);
                DisposeMats(translations);
                DisposeMats(normals);
            }
        }

        [Fact]
        public void FilterHomographySolutionsPreservesOrderingMaskAndPointOverloadsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Mat[] rotations = CreateFilterRotations();
            Mat[] normals = CreateFilterNormals();
            Point2f[] before = CreateFilterPoints();
            Point2f[] after = CreateFilterPoints();
            using Mat beforeMat = Calib3DCv2.ToPointMat(before);
            using Mat afterMat = Calib3DCv2.ToPointMat(after);
            using Mat mask = CreateMask(1, 0);
            using var callerOwned = new Mat();

            try
            {
                Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
                    rotations,
                    normals,
                    beforeMat,
                    afterMat,
                    possibleSolutions: callerOwned);
                Assert.False(
                    callerOwned.Empty,
                    $"Rows={callerOwned.Rows}, Cols={callerOwned.Cols}, Type={callerOwned.Type}, Continuous={callerOwned.IsContinuous}");
                Assert.True(
                    callerOwned.IsContinuous,
                    $"Rows={callerOwned.Rows}, Cols={callerOwned.Cols}, Type={callerOwned.Type}");
                Assert.Equal(new[] { 0 }, callerOwned.ToArray<int>());
                Assert.Equal(MatType.CV_32SC1, callerOwned.Type);

                using Mat masked = Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
                    rotations,
                    normals,
                    beforeMat,
                    afterMat,
                    mask);
                Assert.Equal(new[] { 0, 2 }, masked.ToArray<int>());

                using Mat arrayResult = Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
                    rotations,
                    normals,
                    before,
                    after,
                    mask);
                Assert.Equal(new[] { 0, 2 }, arrayResult.ToArray<int>());

                using Mat spanResult = Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
                    rotations,
                    normals,
                    before.AsSpan(),
                    after.AsSpan(),
                    mask);
                Assert.Equal(new[] { 0, 2 }, spanResult.ToArray<int>());
            }
            finally
            {
                DisposeMats(rotations);
                DisposeMats(normals);
            }
        }

        [Fact]
        public void HomographyDecompositionPreservesInputsAndOwnedFailureOutputsWhenNativeSmokeIsEnabled()
        {
            Mat[] failedRotations = null!;
            Mat[] failedTranslations = null!;
            Mat[] failedNormals = null!;
            using (var invalidHomography = new Mat(2, 3, MatType.CV_64FC1))
            using (Mat cameraMatrix = CreateRegressionCameraMatrix())
            {
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.DecomposeHomographyMat(
                        invalidHomography,
                        cameraMatrix,
                        out failedRotations,
                        out failedTranslations,
                        out failedNormals));
            }
            Assert.Empty(failedRotations);
            Assert.Empty(failedTranslations);
            Assert.Empty(failedNormals);

            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using Mat homography = CreateRegressionHomography();
            using Mat camera = CreateRegressionCameraMatrix();
            double[] homographySnapshot = homography.ToArray<double>();
            double[] cameraSnapshot = camera.ToArray<double>();
            int count = Calib3DCv2.DecomposeHomographyMat(
                homography,
                camera,
                out Mat[] rotations,
                out Mat[] translations,
                out Mat[] normals);
            try
            {
                Assert.InRange(count, 1, 4);
                Assert.Equal(homographySnapshot, homography.ToArray<double>());
                Assert.Equal(cameraSnapshot, camera.ToArray<double>());

                Point2f[] before = CreateFilterPoints();
                Point2f[] after = CreateFilterPoints();
                using Mat beforeMat = Calib3DCv2.ToPointMat(before);
                using Mat afterMat = Calib3DCv2.ToPointMat(after);
                using Mat mask = CreateMask(1, 0);
                float[] beforeSnapshot = beforeMat.ToArray<float>();
                float[] afterSnapshot = afterMat.ToArray<float>();
                byte[] maskSnapshot = mask.ToArray<byte>();
                using Mat possible = Calib3DCv2.FilterHomographyDecompByVisibleRefpoints(
                    rotations,
                    normals,
                    beforeMat,
                    afterMat,
                    mask);
                Assert.Equal(beforeSnapshot, beforeMat.ToArray<float>());
                Assert.Equal(afterSnapshot, afterMat.ToArray<float>());
                Assert.Equal(maskSnapshot, mask.ToArray<byte>());
            }
            finally
            {
                DisposeMats(rotations);
                DisposeMats(translations);
                DisposeMats(normals);
            }
        }

        private static Mat CreateRegressionCameraMatrix()
        {
            return CreateMatrix(
                3,
                3,
                640.0, 0.0, 320.0,
                0.0, 640.0, 240.0,
                0.0, 0.0, 1.0);
        }

        private static Mat CreateRegressionHomography()
        {
            return CreateMatrix(
                3,
                3,
                2.649157564634028,
                4.583875997496426,
                70.694447785121326,
                -1.072756858861583,
                3.533262150437228,
                1513.656999614321649,
                0.001303887589576,
                0.003042206876298,
                1.0);
        }

        private static Mat[] CreateOutputMats()
        {
            return new[]
            {
                new Mat(),
                new Mat(),
                new Mat(),
                new Mat()
            };
        }

        private static Mat[] CreateFilterRotations()
        {
            return new[]
            {
                CreateIdentityRotation(),
                CreateIdentityRotation(),
                CreateIdentityRotation(),
                CreateIdentityRotation()
            };
        }

        private static Mat[] CreateFilterNormals()
        {
            return new[]
            {
                CreateMatrix(3, 1, 0.0, 0.0, 1.0),
                CreateMatrix(3, 1, 0.0, 0.0, -1.0),
                CreateMatrix(3, 1, 1.0, 0.0, 1.0),
                CreateMatrix(3, 1, 0.0, 1.0, -1.0)
            };
        }

        private static Mat CreateIdentityRotation()
        {
            return CreateMatrix(
                3,
                3,
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);
        }

        private static Point2f[] CreateFilterPoints()
        {
            return new[]
            {
                new Point2f(0.0F, 0.0F),
                new Point2f(-2.0F, 0.0F)
            };
        }

        private static Mat CreateMask(params byte[] values)
        {
            var result = new Mat(values.Length, 1, MatType.CV_8UC1);
            for (int i = 0; i < values.Length; ++i)
            {
                result.SetValue(i, values[i]);
            }
            return result;
        }

        private static Mat CreateMatrix(
            int rows,
            int cols,
            params double[] values)
        {
            Assert.Equal(rows * cols, values.Length);
            var result = new Mat(rows, cols, MatType.CV_64FC1);
            for (int i = 0; i < values.Length; ++i)
            {
                result.SetValue(i, values[i]);
            }
            return result;
        }

        private static void AssertSolutionShapes(
            Mat[] rotations,
            Mat[] translations,
            Mat[] normals,
            int count)
        {
            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(3, rotations[i].Rows);
                Assert.Equal(3, rotations[i].Cols);
                Assert.Equal(MatType.CV_64FC1, rotations[i].Type);
                Assert.Equal(3, translations[i].Rows);
                Assert.Equal(1, translations[i].Cols);
                Assert.Equal(MatType.CV_64FC1, translations[i].Type);
                Assert.Equal(3, normals[i].Rows);
                Assert.Equal(1, normals[i].Cols);
                Assert.Equal(MatType.CV_64FC1, normals[i].Type);
            }
        }

        private static void AssertContainsExpectedRegressionMotion(
            Mat[] rotations,
            Mat[] translations,
            Mat[] normals,
            int count)
        {
            double[] expectedRotation =
            {
                0.43307983549125,
                0.545749113549648,
                -0.717356090899523,
                -0.85630229674426,
                0.497582023798831,
                -0.138414255706431,
                0.281404038139784,
                0.67421809131173,
                0.682818960388909
            };
            double[] expectedTranslation =
            {
                1.826751712278038,
                1.264718492450820,
                0.195080809998819
            };
            double[] expectedNormal =
            {
                0.244875830334816,
                0.480857890778889,
                0.841909446789566
            };

            for (int i = 0; i < count; ++i)
            {
                if (MaximumDifference(
                        rotations[i].ToArray<double>(),
                        expectedRotation) < 1.0e-3 &&
                    MaximumDifference(
                        translations[i].ToArray<double>(),
                        expectedTranslation) < 1.0e-3 &&
                    MaximumDifference(
                        normals[i].ToArray<double>(),
                        expectedNormal) < 1.0e-3)
                {
                    return;
                }
            }

            Assert.Fail("Expected upstream homography motion was not returned.");
        }

        private static double MaximumDifference(
            double[] actual,
            double[] expected)
        {
            Assert.Equal(expected.Length, actual.Length);
            double maximum = 0.0;
            for (int i = 0; i < actual.Length; ++i)
            {
                maximum = Math.Max(
                    maximum,
                    Math.Abs(actual[i] - expected[i]));
            }
            return maximum;
        }

        private static double Determinant3x3(Mat rotation)
        {
            double[] values = rotation.ToArray<double>();
            return
                (values[0] * ((values[4] * values[8]) - (values[5] * values[7]))) -
                (values[1] * ((values[3] * values[8]) - (values[5] * values[6]))) +
                (values[2] * ((values[3] * values[7]) - (values[4] * values[6])));
        }

        private static void AssertMatrixNear(
            Mat expected,
            Mat actual,
            double tolerance)
        {
            Assert.Equal(expected.Rows, actual.Rows);
            Assert.Equal(expected.Cols, actual.Cols);
            Assert.Equal(expected.Type, actual.Type);
            Assert.True(
                MaximumDifference(
                    actual.ToArray<double>(),
                    expected.ToArray<double>()) <= tolerance);
        }

        private static void DisposeMats(Mat[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                values[i]?.Dispose();
            }
        }
    }
}
