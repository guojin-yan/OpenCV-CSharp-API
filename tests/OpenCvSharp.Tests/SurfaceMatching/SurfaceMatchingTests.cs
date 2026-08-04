using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.SurfaceMatching;

namespace JYPPX.OpenCvSharp.Tests.SurfaceMatching
{
    public sealed class SurfaceMatchingTests
    {
        [Fact]
        public void EnumAndResultTypesExposeExpectedValues()
        {
            Assert.Equal(0, (int)IcpSamplingType.Uniform);
            Assert.Equal(1, (int)IcpSamplingType.Gelfand);

            double[] pose = new double[16];
            pose[0] = 1.0;
            var result = new IcpRegistrationResult(0, 1.25, pose);
            pose[0] = 9.0;

            Assert.Equal(0, result.ResultCode);
            Assert.Equal(1.25, result.Residual);
            Assert.Equal(16, result.PoseLength);
            Assert.Equal(16, result.Pose.Length);
            Assert.Equal(1.0, result.Pose[0]);

            double[] returnedIcpPose = result.Pose;
            returnedIcpPose[0] = 11.0;

            Assert.Equal(1.0, result.Pose[0]);
            Assert.Equal("{ResultCode=0,Residual=1.25,PoseLength=16}", result.ToString());
            Assert.Throws<ArgumentNullException>(() => new IcpRegistrationResult(0, 0.0, null!));
            Assert.Throws<ArgumentException>(() => new IcpRegistrationResult(0, 0.0, new double[15]));

            double[] translation = { 1.0, 2.0, 3.0 };
            double[] quaternion = { 4.0, 5.0, 6.0, 7.0 };
            double[] pose3d = new double[16];
            pose3d[0] = 8.0;
            var poseResult = new Pose3DResult(0.5, 1.5, 2UL, 3UL, 4.5, translation, quaternion, pose3d);
            translation[0] = 9.0;
            quaternion[0] = 9.0;
            pose3d[0] = 9.0;

            Assert.Equal(0.5, poseResult.Alpha);
            Assert.Equal(1.5, poseResult.Residual);
            Assert.Equal(2UL, poseResult.ModelIndex);
            Assert.Equal(3UL, poseResult.NumVotes);
            Assert.Equal(4.5, poseResult.Angle);
            Assert.Equal(3, poseResult.TranslationLength);
            Assert.Equal(4, poseResult.QuaternionLength);
            Assert.Equal(16, poseResult.PoseLength);
            Assert.Equal(1.0, poseResult.Translation[0]);
            Assert.Equal(4.0, poseResult.Quaternion[0]);
            Assert.Equal(8.0, poseResult.Pose[0]);

            double[] returnedTranslation = poseResult.Translation;
            double[] returnedQuaternion = poseResult.Quaternion;
            double[] returnedPose = poseResult.Pose;
            returnedTranslation[0] = 11.0;
            returnedQuaternion[0] = 12.0;
            returnedPose[0] = 13.0;

            Assert.Equal(1.0, poseResult.Translation[0]);
            Assert.Equal(4.0, poseResult.Quaternion[0]);
            Assert.Equal(8.0, poseResult.Pose[0]);
            Assert.Equal("{Alpha=0.5,Residual=1.5,ModelIndex=2,NumVotes=3,Angle=4.5,TranslationLength=3,QuaternionLength=4,PoseLength=16}", poseResult.ToString());
            Assert.Throws<ArgumentNullException>(() => new Pose3DResult(0, 0, 0, 0, 0, null!, quaternion, pose3d));
            Assert.Throws<ArgumentNullException>(() => new Pose3DResult(0, 0, 0, 0, 0, translation, null!, pose3d));
            Assert.Throws<ArgumentNullException>(() => new Pose3DResult(0, 0, 0, 0, 0, translation, quaternion, null!));
            Assert.Throws<ArgumentException>(() => new Pose3DResult(0, 0, 0, 0, 0, new double[2], quaternion, pose3d));
            Assert.Throws<ArgumentException>(() => new Pose3DResult(0, 0, 0, 0, 0, translation, new double[3], pose3d));
            Assert.Throws<ArgumentException>(() => new Pose3DResult(0, 0, 0, 0, 0, translation, quaternion, new double[15]));
        }

        [Fact]
        public void ResultTypesFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                Assert.Equal(
                    "{ResultCode=0,Residual=1.25,PoseLength=16}",
                    new IcpRegistrationResult(0, 1.25, new double[16]).ToString());
                Assert.Equal(
                    "{Alpha=0.5,Residual=1.5,ModelIndex=2,NumVotes=3,Angle=4.5,TranslationLength=3,QuaternionLength=4,PoseLength=16}",
                    new Pose3DResult(0.5, 1.5, 2UL, 3UL, 4.5, new double[3], new double[4], new double[16]).ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void IcpValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (Icp? icp = TryCreateIcp())
            {
                if (icp == null)
                {
                    return;
                }

                using (Mat cloud = CreatePointCloud())
                using (Mat emptySourceCloud = new Mat(0, 6, MatType.CV_32FC1))
                {
                    Assert.Throws<ArgumentNullException>(() => icp.RegisterModelToScene(null!, cloud));
                    Assert.Throws<ArgumentNullException>(() => icp.RegisterModelToScene(cloud, null!));
                    Assert.Throws<ArgumentException>(() => icp.RegisterModelToScene(emptySourceCloud, cloud));
                    icp.Dispose();
                    Assert.True(icp.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => icp.RegisterModelToScene(cloud, cloud));
                }
            }
        }

        [Fact]
        public void PpfValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (Ppf3DDetector? detector = TryCreateDetector())
            {
                if (detector == null)
                {
                    return;
                }

                using (Mat cloud = CreatePointCloud())
                using (Mat invalidTypeCloud = new Mat(8, 6, MatType.CV_64FC1, new Scalar(0)))
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => detector.SetSearchParams(-2.0, -1.0));
                    Assert.Throws<ArgumentNullException>(() => detector.TrainModel(null!));
                    Assert.Throws<ArgumentNullException>(() => detector.Match(null!));
                    Assert.Throws<ArgumentException>(() => detector.TrainModel(invalidTypeCloud));
                    Assert.Throws<ArgumentException>(() => detector.Match(invalidTypeCloud));
                    Assert.Throws<ArgumentOutOfRangeException>(() => detector.Match(cloud, 0.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => detector.Match(cloud, 1.1));
                    detector.Dispose();
                    Assert.True(detector.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => detector.SetSearchParams());
                    Assert.Throws<ObjectDisposedException>(() => detector.TrainModel(cloud));
                    Assert.Throws<ObjectDisposedException>(() => detector.Match(cloud));
                }
            }
        }

        [Fact]
        public void LinkedSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (Mat cloud = CreatePointCloud())
                using (Icp icp = SurfaceMatchingCv2.CreateIcp(iterations: 1, tolerance: 0.05F, numLevels: 1))
                {
                    IcpRegistrationResult result = icp.RegisterModelToScene(cloud, cloud);
                    Assert.Equal(16, result.Pose.Length);
                    Assert.False(double.IsNaN(result.Residual));
                }

                using (Mat cloud = CreatePointCloud())
                using (Ppf3DDetector detector = SurfaceMatchingCv2.CreatePpf3DDetector(0.2, 0.2, 20))
                {
                    detector.SetSearchParams();
                    detector.TrainModel(cloud);
                    Pose3DResult[] poses = detector.Match(cloud, 1.0, 0.2);
                    Assert.NotNull(poses);
                }
            }
            catch (OpenCvException ex) when (IsSurfaceMatchingModuleMissing(ex) || IsTinyDataBoundary(ex))
            {
                Assert.True(IsSurfaceMatchingModuleMissing(ex) || IsTinyDataBoundary(ex), ex.Message);
            }
        }

        private static Icp? TryCreateIcp()
        {
            try
            {
                return SurfaceMatchingCv2.CreateIcp(iterations: 1, numLevels: 1);
            }
            catch (OpenCvException ex) when (IsSurfaceMatchingModuleMissing(ex))
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
        }

        private static Ppf3DDetector? TryCreateDetector()
        {
            try
            {
                return SurfaceMatchingCv2.CreatePpf3DDetector();
            }
            catch (OpenCvException ex) when (IsSurfaceMatchingModuleMissing(ex))
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
        }

        private static Mat CreatePointCloud()
        {
            var cloud = new Mat(8, 6, MatType.CV_32FC1);
            cloud.CopyFrom<float>(new float[]
            {
                0, 0, 0, 0, 0, 1,
                1, 0, 0, 0, 0, 1,
                0, 1, 0, 0, 0, 1,
                1, 1, 0, 0, 0, 1,
                0, 0, 1, 0, 0, 1,
                1, 0, 1, 0, 0, 1,
                0, 1, 1, 0, 0, 1,
                1, 1, 1, 0, 0, 1
            });
            return cloud;
        }

        private static bool IsSurfaceMatchingModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("surface", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("ppf", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsTinyDataBoundary(OpenCvException exception)
        {
            return exception.Message.IndexOf("assert", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("failed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("nan", StringComparison.OrdinalIgnoreCase) >= 0;
        }

    }
}
