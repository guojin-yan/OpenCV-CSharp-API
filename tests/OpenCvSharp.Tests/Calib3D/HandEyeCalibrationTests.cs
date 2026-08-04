using System;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    public sealed class HandEyeCalibrationTests
    {
        [Fact]
        public void MethodEnumsMatchOpenCvValues()
        {
            Assert.Equal(0, (int)HandEyeCalibrationMethod.Tsai);
            Assert.Equal(1, (int)HandEyeCalibrationMethod.Park);
            Assert.Equal(2, (int)HandEyeCalibrationMethod.Horaud);
            Assert.Equal(3, (int)HandEyeCalibrationMethod.Andreff);
            Assert.Equal(4, (int)HandEyeCalibrationMethod.Daniilidis);
            Assert.Equal(0, (int)RobotWorldHandEyeCalibrationMethod.Shah);
            Assert.Equal(1, (int)RobotWorldHandEyeCalibrationMethod.Li);
        }

        [Fact]
        public void ResultObjectsExposeOwnedMatrices()
        {
            using (var rCam2Gripper = new Mat())
            using (var tCam2Gripper = new Mat())
            using (var rBase2World = new Mat())
            using (var tBase2World = new Mat())
            using (var rGripper2Cam = new Mat())
            using (var tGripper2Cam = new Mat())
            {
                var handEye = new HandEyeCalibrationResult(rCam2Gripper, tCam2Gripper);
                var robotWorld = new RobotWorldHandEyeCalibrationResult(
                    rBase2World,
                    tBase2World,
                    rGripper2Cam,
                    tGripper2Cam);

                Assert.Same(rCam2Gripper, handEye.RCam2Gripper);
                Assert.Same(tCam2Gripper, handEye.TCam2Gripper);
                Assert.Same(rBase2World, robotWorld.RBase2World);
                Assert.Same(tBase2World, robotWorld.TBase2World);
                Assert.Same(rGripper2Cam, robotWorld.RGripper2Cam);
                Assert.Same(tGripper2Cam, robotWorld.TGripper2Cam);
                Assert.Equal("{RCam2Gripper=0x0,TCam2Gripper=0x0}", handEye.ToString());
                Assert.Contains("RBase2World=0x0", robotWorld.ToString(), StringComparison.Ordinal);
                Assert.Throws<ArgumentNullException>(() => new HandEyeCalibrationResult(null!, tCam2Gripper));
                Assert.Throws<ArgumentNullException>(() =>
                    new RobotWorldHandEyeCalibrationResult(rBase2World, tBase2World, null!, tGripper2Cam));
            }
        }

        [Fact]
        public void CalibrateHandEyeValidatesManagedArguments()
        {
            Mat[] rotations = CreateIdentityRotations(3);
            Mat[] translations = CreateZeroTranslations(3);
            using (var rOutput = new Mat())
            using (var tOutput = new Mat())
            using (var invalidRotation = new Mat(2, 2, MatType.CV_64FC1))
            using (var invalidTranslation = new Mat(2, 1, MatType.CV_64FC1))
            {
                try
                {
                    Assert.Throws<ArgumentNullException>(() =>
                        Calib3DCv2.CalibrateHandEye(null!, translations, rotations, translations, rOutput, tOutput));
                    Assert.Throws<ArgumentException>(() =>
                        Calib3DCv2.CalibrateHandEye(
                            new[] { rotations[0], rotations[1] },
                            new[] { translations[0], translations[1] },
                            new[] { rotations[0], rotations[1] },
                            new[] { translations[0], translations[1] },
                            rOutput,
                            tOutput));
                    Assert.Throws<ArgumentException>(() =>
                        Calib3DCv2.CalibrateHandEye(
                            rotations,
                            new[] { translations[0], translations[1] },
                            rotations,
                            translations,
                            rOutput,
                            tOutput));
                    Assert.Throws<ArgumentNullException>(() =>
                        Calib3DCv2.CalibrateHandEye(
                            new[] { rotations[0], null!, rotations[2] },
                            translations,
                            rotations,
                            translations,
                            rOutput,
                            tOutput));
                    Assert.Throws<ArgumentException>(() =>
                        Calib3DCv2.CalibrateHandEye(
                            new[] { rotations[0], invalidRotation, rotations[2] },
                            translations,
                            rotations,
                            translations,
                            rOutput,
                            tOutput));
                    Assert.Throws<ArgumentException>(() =>
                        Calib3DCv2.CalibrateHandEye(
                            rotations,
                            new[] { translations[0], invalidTranslation, translations[2] },
                            rotations,
                            translations,
                            rOutput,
                            tOutput));
                    Assert.Throws<ArgumentNullException>(() =>
                        Calib3DCv2.CalibrateHandEye(rotations, translations, rotations, translations, null!, tOutput));
                    Assert.Throws<ArgumentOutOfRangeException>(() =>
                        Calib3DCv2.CalibrateHandEye(
                            rotations,
                            translations,
                            rotations,
                            translations,
                            rOutput,
                            tOutput,
                            (HandEyeCalibrationMethod)99));
                }
                finally
                {
                    DisposeAll(rotations);
                    DisposeAll(translations);
                }
            }
        }

        [Fact]
        public void CalibrateRobotWorldHandEyeValidatesManagedArguments()
        {
            Mat[] rotations = CreateIdentityRotations(3);
            Mat[] translations = CreateZeroTranslations(3);
            using (var rBase2World = new Mat())
            using (var tBase2World = new Mat())
            using (var rGripper2Cam = new Mat())
            using (var tGripper2Cam = new Mat())
            {
                try
                {
                    Assert.Throws<ArgumentException>(() =>
                        Calib3DCv2.CalibrateRobotWorldHandEye(
                            rotations,
                            translations,
                            rotations,
                            new[] { translations[0], translations[1] },
                            rBase2World,
                            tBase2World,
                            rGripper2Cam,
                            tGripper2Cam));
                    Assert.Throws<ArgumentNullException>(() =>
                        Calib3DCv2.CalibrateRobotWorldHandEye(
                            rotations,
                            translations,
                            rotations,
                            translations,
                            rBase2World,
                            tBase2World,
                            null!,
                            tGripper2Cam));
                    Assert.Throws<ArgumentOutOfRangeException>(() =>
                        Calib3DCv2.CalibrateRobotWorldHandEye(
                            rotations,
                            translations,
                            rotations,
                            translations,
                            rBase2World,
                            tBase2World,
                            rGripper2Cam,
                            tGripper2Cam,
                            (RobotWorldHandEyeCalibrationMethod)99));
                }
                finally
                {
                    DisposeAll(rotations);
                    DisposeAll(translations);
                }
            }
        }

        [Fact]
        public void SpanOverloadsValidatePoseCounts()
        {
            Mat[] rotations = CreateIdentityRotations(2);
            Mat[] translations = CreateZeroTranslations(2);
            using (var rOutput = new Mat())
            using (var tOutput = new Mat())
            {
                try
                {
                    Assert.Throws<ArgumentException>(() =>
                        Calib3DCv2.CalibrateHandEye(
                            rotations.AsSpan(),
                            translations.AsSpan(),
                            rotations.AsSpan(),
                            translations.AsSpan(),
                            rOutput,
                            tOutput));
                }
                finally
                {
                    DisposeAll(rotations);
                    DisposeAll(translations);
                }
            }
        }

        [Fact]
        public void CalibrateHandEyeRecoversSyntheticTransformWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Pose expectedCam2Gripper = new Pose(
                RotationFromEuler(0.24, -0.18, 0.12),
                new[] { 0.12, -0.08, 0.20 });
            Pose target2Base = new Pose(
                RotationFromEuler(-0.32, 0.26, 0.19),
                new[] { 1.10, 0.60, 1.70 });
            Pose[] gripper2Base = CreateDiversePoses();
            Pose[] target2Cam = new Pose[gripper2Base.Length];
            for (int i = 0; i < gripper2Base.Length; ++i)
            {
                target2Cam[i] = Compose(
                    Compose(Inverse(expectedCam2Gripper), Inverse(gripper2Base[i])),
                    target2Base);
            }

            CreateMatCollections(gripper2Base, out Mat[] rGripper2Base, out Mat[] tGripper2Base);
            CreateMatCollections(target2Cam, out Mat[] rTarget2Cam, out Mat[] tTarget2Cam);
            try
            {
                HandEyeCalibrationResult result = Calib3DCv2.CalibrateHandEye(
                    rGripper2Base,
                    tGripper2Base,
                    rTarget2Cam,
                    tTarget2Cam,
                    HandEyeCalibrationMethod.Park);
                try
                {
                    AssertRotationClose(expectedCam2Gripper.Rotation, result.RCam2Gripper, 1e-6);
                    AssertTranslationClose(expectedCam2Gripper.Translation, result.TCam2Gripper, 1e-6);
                }
                finally
                {
                    result.RCam2Gripper.Dispose();
                    result.TCam2Gripper.Dispose();
                }
            }
            finally
            {
                DisposeAll(rGripper2Base);
                DisposeAll(tGripper2Base);
                DisposeAll(rTarget2Cam);
                DisposeAll(tTarget2Cam);
            }
        }

        [Fact]
        public void CalibrateRobotWorldHandEyeRecoversSyntheticTransformsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Pose expectedBase2World = new Pose(
                RotationFromEuler(-0.17, 0.21, 0.29),
                new[] { 0.70, -0.35, 1.25 });
            Pose expectedGripper2Cam = new Pose(
                RotationFromEuler(0.20, -0.14, 0.11),
                new[] { 0.09, 0.04, -0.16 });
            Pose[] base2Gripper = CreateDiversePoses();
            Pose[] world2Cam = new Pose[base2Gripper.Length];
            for (int i = 0; i < base2Gripper.Length; ++i)
            {
                world2Cam[i] = Compose(
                    Compose(expectedGripper2Cam, base2Gripper[i]),
                    Inverse(expectedBase2World));
            }

            CreateMatCollections(world2Cam, out Mat[] rWorld2Cam, out Mat[] tWorld2Cam);
            CreateMatCollections(base2Gripper, out Mat[] rBase2Gripper, out Mat[] tBase2Gripper);
            try
            {
                RobotWorldHandEyeCalibrationResult result = Calib3DCv2.CalibrateRobotWorldHandEye(
                    rWorld2Cam,
                    tWorld2Cam,
                    rBase2Gripper,
                    tBase2Gripper,
                    RobotWorldHandEyeCalibrationMethod.Shah);
                try
                {
                    AssertRotationClose(expectedBase2World.Rotation, result.RBase2World, 1e-6);
                    AssertTranslationClose(expectedBase2World.Translation, result.TBase2World, 1e-6);
                    AssertRotationClose(expectedGripper2Cam.Rotation, result.RGripper2Cam, 1e-6);
                    AssertTranslationClose(expectedGripper2Cam.Translation, result.TGripper2Cam, 1e-6);
                }
                finally
                {
                    result.RBase2World.Dispose();
                    result.TBase2World.Dispose();
                    result.RGripper2Cam.Dispose();
                    result.TGripper2Cam.Dispose();
                }
            }
            finally
            {
                DisposeAll(rWorld2Cam);
                DisposeAll(tWorld2Cam);
                DisposeAll(rBase2Gripper);
                DisposeAll(tBase2Gripper);
            }
        }

        private static Mat[] CreateIdentityRotations(int count)
        {
            var result = new Mat[count];
            for (int i = 0; i < count; ++i)
            {
                result[i] = Mat.Eye(3, 3, MatType.CV_64FC1);
            }

            return result;
        }

        private static Mat[] CreateZeroTranslations(int count)
        {
            var result = new Mat[count];
            for (int i = 0; i < count; ++i)
            {
                result[i] = new Mat(3, 1, MatType.CV_64FC1);
            }

            return result;
        }

        private static Pose[] CreateDiversePoses()
        {
            return new[]
            {
                CreatePose(0.15, -0.10, 0.08, 0.60, -0.40, 0.90),
                CreatePose(-0.25, 0.12, 0.18, -0.75, 0.55, 1.10),
                CreatePose(0.31, 0.22, -0.14, 0.45, 0.80, -0.65),
                CreatePose(-0.18, -0.29, 0.24, 1.20, -0.70, 0.35),
                CreatePose(0.27, -0.21, -0.30, -0.55, -0.95, 0.75),
                CreatePose(-0.34, 0.16, -0.11, 0.85, 0.25, 1.30),
                CreatePose(0.12, 0.35, 0.28, -1.05, 0.65, -0.40),
                CreatePose(-0.22, -0.13, 0.33, 0.30, -1.10, 0.55)
            };
        }

        private static Pose CreatePose(double x, double y, double z, double tx, double ty, double tz)
        {
            return new Pose(RotationFromEuler(x, y, z), new[] { tx, ty, tz });
        }

        private static void CreateMatCollections(Pose[] poses, out Mat[] rotations, out Mat[] translations)
        {
            rotations = new Mat[poses.Length];
            translations = new Mat[poses.Length];
            for (int i = 0; i < poses.Length; ++i)
            {
                rotations[i] = CreateRotationMat(poses[i].Rotation);
                translations[i] = CreateTranslationMat(poses[i].Translation);
            }
        }

        private static Mat CreateRotationMat(double[,] values)
        {
            var result = new Mat(3, 3, MatType.CV_64FC1);
            for (int row = 0; row < 3; ++row)
            {
                for (int column = 0; column < 3; ++column)
                {
                    result.SetValue(row * 3 + column, values[row, column]);
                }
            }

            return result;
        }

        private static Mat CreateTranslationMat(double[] values)
        {
            var result = new Mat(3, 1, MatType.CV_64FC1);
            for (int i = 0; i < 3; ++i)
            {
                result.SetValue(i, values[i]);
            }

            return result;
        }

        private static void AssertRotationClose(double[,] expected, Mat actual, double tolerance)
        {
            Assert.Equal(3, actual.Rows);
            Assert.Equal(3, actual.Cols);
            for (int row = 0; row < 3; ++row)
            {
                for (int column = 0; column < 3; ++column)
                {
                    Assert.InRange(
                        Math.Abs(expected[row, column] - actual.GetValue<double>(row * 3 + column)),
                        0.0,
                        tolerance);
                }
            }
        }

        private static void AssertTranslationClose(double[] expected, Mat actual, double tolerance)
        {
            Assert.Equal(3, actual.Rows * actual.Cols);
            for (int i = 0; i < 3; ++i)
            {
                Assert.InRange(Math.Abs(expected[i] - actual.GetValue<double>(i)), 0.0, tolerance);
            }
        }

        private static Pose Compose(Pose first, Pose second)
        {
            double[,] rotation = Multiply(first.Rotation, second.Rotation);
            double[] rotatedTranslation = Multiply(first.Rotation, second.Translation);
            return new Pose(
                rotation,
                new[]
                {
                    rotatedTranslation[0] + first.Translation[0],
                    rotatedTranslation[1] + first.Translation[1],
                    rotatedTranslation[2] + first.Translation[2]
                });
        }

        private static Pose Inverse(Pose value)
        {
            double[,] rotation = Transpose(value.Rotation);
            double[] translation = Multiply(rotation, value.Translation);
            return new Pose(
                rotation,
                new[] { -translation[0], -translation[1], -translation[2] });
        }

        private static double[,] RotationFromEuler(double x, double y, double z)
        {
            double cx = Math.Cos(x);
            double sx = Math.Sin(x);
            double cy = Math.Cos(y);
            double sy = Math.Sin(y);
            double cz = Math.Cos(z);
            double sz = Math.Sin(z);

            double[,] rx =
            {
                { 1.0, 0.0, 0.0 },
                { 0.0, cx, -sx },
                { 0.0, sx, cx }
            };
            double[,] ry =
            {
                { cy, 0.0, sy },
                { 0.0, 1.0, 0.0 },
                { -sy, 0.0, cy }
            };
            double[,] rz =
            {
                { cz, -sz, 0.0 },
                { sz, cz, 0.0 },
                { 0.0, 0.0, 1.0 }
            };

            return Multiply(Multiply(rz, ry), rx);
        }

        private static double[,] Multiply(double[,] left, double[,] right)
        {
            var result = new double[3, 3];
            for (int row = 0; row < 3; ++row)
            {
                for (int column = 0; column < 3; ++column)
                {
                    for (int i = 0; i < 3; ++i)
                    {
                        result[row, column] += left[row, i] * right[i, column];
                    }
                }
            }

            return result;
        }

        private static double[] Multiply(double[,] left, double[] right)
        {
            var result = new double[3];
            for (int row = 0; row < 3; ++row)
            {
                for (int i = 0; i < 3; ++i)
                {
                    result[row] += left[row, i] * right[i];
                }
            }

            return result;
        }

        private static double[,] Transpose(double[,] value)
        {
            var result = new double[3, 3];
            for (int row = 0; row < 3; ++row)
            {
                for (int column = 0; column < 3; ++column)
                {
                    result[row, column] = value[column, row];
                }
            }

            return result;
        }

        private static void DisposeAll(Mat[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                values[i]?.Dispose();
            }
        }

        private readonly struct Pose
        {
            internal Pose(double[,] rotation, double[] translation)
            {
                Rotation = rotation;
                Translation = translation;
            }

            internal double[,] Rotation { get; }

            internal double[] Translation { get; }
        }
    }
}
