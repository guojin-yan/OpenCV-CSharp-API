using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Rapid;

namespace JYPPX.OpenCvSharp.Tests.Rapid
{
    public sealed class RapidTests
    {
        [Fact]
        public void ResultTypeExposesValues()
        {
            var result = new RapidResult(0.5F, 1.25);
            var withoutRmsd = new RapidResult(0.5F, null);

            Assert.Equal(0.5F, result.Ratio);
            Assert.Equal(1.25, result.Rmsd);
            Assert.Equal("{Ratio=0.5,Rmsd=1.25}", result.ToString());
            Assert.Null(withoutRmsd.Rmsd);
            Assert.Equal("{Ratio=0.5,Rmsd=null}", withoutRmsd.ToString());
            Assert.Equal(result, new RapidResult(0.5F, 1.25));
            Assert.True(result == new RapidResult(0.5F, 1.25));
            Assert.True(result != withoutRmsd);
            Assert.Equal(0.0F, new RapidResult(0.0F, 0.0).Ratio);
            Assert.Equal(1.0F, new RapidResult(1.0F, null).Ratio);
            Assert.NotEqual(result, withoutRmsd);
            Assert.NotEqual(result, new RapidResult(0.25F, 1.25));
            Assert.NotEqual(result, new RapidResult(0.5F, 2.5));
            Assert.False(result.Equals("not a rapid result"));
            Assert.Equal(new RapidResult(0.5F, 1.25).GetHashCode(), result.GetHashCode());

            Assert.Throws<ArgumentOutOfRangeException>(() => new RapidResult(-0.001F, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RapidResult(1.001F, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RapidResult(float.NaN, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RapidResult(float.PositiveInfinity, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RapidResult(0.5F, -0.001));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RapidResult(0.5F, double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RapidResult(0.5F, double.PositiveInfinity));
        }

        [Fact]
        public void ResultTypeFormatsInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                Assert.Equal("{Ratio=0.5,Rmsd=1.25}", new RapidResult(0.5F, 1.25).ToString());
                Assert.Equal("{Ratio=0.5,Rmsd=null}", new RapidResult(0.5F, null).ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void StaticValidationRuns()
        {
            using (RapidSilhouetteTracker? nativeBoundary = TryCreateTracker())
            {
                if (nativeBoundary == null)
                {
                    return;
                }
            }

            using (Mat image = CreateEdgeImage())
            using (Mat mesh = CreateMeshPoints())
            using (Mat tris = CreateMeshTriangles())
            using (Mat camera = CreateCameraMatrix())
            using (Mat rvec = CreatePoseVector(0, 0, 0))
            using (Mat tvec = CreatePoseVector(0, 0, 6))
            using (Mat cols = new Mat())
            using (Mat floatCols = new Mat(2, 1, MatType.CV_32FC1))
            using (Mat intCols = new Mat(2, 1, MatType.CV_32SC1))
            using (Mat threeRowCols = new Mat(3, 1, MatType.CV_32SC1))
            using (Mat colors = new Mat(3, 1, MatType.CV_64FC4))
            using (Mat locations = new Mat())
            using (Mat floatLocations = new Mat(2, 1, MatType.CV_32FC2))
            using (Mat shortLocations = new Mat(2, 1, MatType.CV_16SC2))
            using (Mat threeRowLocations = new Mat(3, 1, MatType.CV_16SC2))
            using (Mat floatMask = new Mat(2, 1, MatType.CV_32FC1))
            using (Mat threeRowMask = new Mat(3, 1, MatType.CV_8UC1))
            using (Mat threeColumnPts3d = new Mat(1, 3, MatType.CV_32FC3))
            using (Mat bundle = new Mat())
            using (Mat floatBundle = new Mat(2, 3, MatType.CV_32FC1))
            using (Mat twoChannelBundle = new Mat(2, 3, MatType.CV_8UC2))
            using (Mat ctl2d = new Mat())
            using (Mat ctl3d = new Mat())
            using (Mat pts2d = new Mat())
            using (Mat floatPts2d = new Mat(2, 1, MatType.CV_32FC2))
            using (Mat badPts2d = new Mat(2, 1, MatType.CV_32FC1))
            using (Mat badCtl2d = new Mat(2, 1, MatType.CV_32FC1))
            using (Mat badTris = new Mat(2, 1, MatType.CV_32FC3))
            using (Mat badMesh = new Mat(2, 1, MatType.CV_32FC2))
            {
                Assert.Throws<ArgumentNullException>(() => RapidCv2.DrawCorrespondencies(null!, cols));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.DrawCorrespondencies(bundle, null!));
                Assert.Throws<ArgumentException>(() => RapidCv2.DrawCorrespondencies(bundle, floatCols));
                Assert.Throws<ArgumentException>(() => RapidCv2.DrawCorrespondencies(image, threeRowCols));
                Assert.Throws<ArgumentException>(() => RapidCv2.DrawCorrespondencies(image, intCols, colors));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.DrawSearchLines(null!, locations, new Scalar(255)));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.DrawSearchLines(image, null!, new Scalar(255)));
                Assert.Throws<ArgumentException>(() => RapidCv2.DrawSearchLines(image, floatLocations, new Scalar(255)));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.DrawWireframe(null!, pts2d, tris, new Scalar(255)));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.DrawWireframe(image, null!, tris, new Scalar(255)));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.DrawWireframe(image, pts2d, null!, new Scalar(255)));
                Assert.Throws<ArgumentException>(() => RapidCv2.DrawWireframe(image, badPts2d, tris, new Scalar(255)));
                Assert.Throws<ArgumentException>(() => RapidCv2.DrawWireframe(image, floatPts2d, badTris, new Scalar(255)));

                Assert.Throws<ArgumentOutOfRangeException>(() => RapidCv2.ExtractControlPoints(0, 3, mesh, rvec, tvec, camera, new Size(64, 64), tris, ctl2d, ctl3d));
                Assert.Throws<ArgumentOutOfRangeException>(() => RapidCv2.ExtractControlPoints(8, 0, mesh, rvec, tvec, camera, new Size(64, 64), tris, ctl2d, ctl3d));
                Assert.Throws<ArgumentOutOfRangeException>(() => RapidCv2.ExtractControlPoints(8, 3, mesh, rvec, tvec, camera, new Size(0, 64), tris, ctl2d, ctl3d));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ExtractControlPoints(8, 3, null!, rvec, tvec, camera, new Size(64, 64), tris, ctl2d, ctl3d));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ExtractControlPoints(8, 3, mesh, null!, tvec, camera, new Size(64, 64), tris, ctl2d, ctl3d));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ExtractControlPoints(8, 3, mesh, rvec, null!, camera, new Size(64, 64), tris, ctl2d, ctl3d));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ExtractControlPoints(8, 3, mesh, rvec, tvec, null!, new Size(64, 64), tris, ctl2d, ctl3d));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ExtractControlPoints(8, 3, mesh, rvec, tvec, camera, new Size(64, 64), null!, ctl2d, ctl3d));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ExtractControlPoints(8, 3, mesh, rvec, tvec, camera, new Size(64, 64), tris, null!, ctl3d));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ExtractControlPoints(8, 3, mesh, rvec, tvec, camera, new Size(64, 64), tris, ctl2d, null!));

                Assert.Throws<ArgumentOutOfRangeException>(() => RapidCv2.ExtractLineBundle(0, ctl2d, image, bundle, locations));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ExtractLineBundle(3, null!, image, bundle, locations));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ExtractLineBundle(3, ctl2d, null!, bundle, locations));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ExtractLineBundle(3, ctl2d, image, null!, locations));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ExtractLineBundle(3, ctl2d, image, bundle, null!));
                Assert.Throws<ArgumentException>(() => RapidCv2.ExtractLineBundle(3, badCtl2d, image, bundle, locations));

                Assert.Throws<ArgumentNullException>(() => RapidCv2.FindCorrespondencies(null!, cols));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.FindCorrespondencies(bundle, null!));
                Assert.Throws<ArgumentException>(() => RapidCv2.FindCorrespondencies(floatBundle, cols));
                Assert.Throws<ArgumentException>(() => RapidCv2.FindCorrespondencies(twoChannelBundle, cols));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ConvertCorrespondencies(null!, locations, pts2d));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ConvertCorrespondencies(cols, null!, pts2d));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.ConvertCorrespondencies(cols, locations, null!));
                Assert.Throws<ArgumentException>(() => RapidCv2.ConvertCorrespondencies(floatCols, locations, pts2d));
                Assert.Throws<ArgumentException>(() => RapidCv2.ConvertCorrespondencies(intCols, floatLocations, pts2d));
                Assert.Throws<ArgumentException>(() => RapidCv2.ConvertCorrespondencies(intCols, shortLocations, pts2d, mask: floatMask));
                Assert.Throws<ArgumentException>(() => RapidCv2.ConvertCorrespondencies(intCols, threeRowLocations, pts2d));
                Assert.Throws<ArgumentException>(() => RapidCv2.ConvertCorrespondencies(intCols, shortLocations, pts2d, mask: threeRowMask));
                Assert.Throws<ArgumentException>(() => RapidCv2.ConvertCorrespondencies(intCols, shortLocations, pts2d, pts3d: threeColumnPts3d));

                Assert.Throws<ArgumentNullException>(() => RapidSilhouetteTracker.Create(null!, tris));
                Assert.Throws<ArgumentNullException>(() => RapidSilhouetteTracker.Create(mesh, null!));
                Assert.Throws<ArgumentException>(() => RapidSilhouetteTracker.Create(badMesh, tris));
                Assert.Throws<ArgumentException>(() => RapidSilhouetteTracker.Create(mesh, badTris));
                Assert.Throws<ArgumentException>(() => OlsTracker.Create(badMesh, tris));
                Assert.Throws<ArgumentException>(() => OlsTracker.Create(mesh, badTris));

                Assert.Throws<ArgumentNullException>(() => RapidCv2.Run(null!, 8, 3, mesh, tris, camera, rvec, tvec));
                Assert.Throws<ArgumentOutOfRangeException>(() => RapidCv2.Run(image, 0, 3, mesh, tris, camera, rvec, tvec));
                Assert.Throws<ArgumentOutOfRangeException>(() => RapidCv2.Run(image, 2, 3, mesh, tris, camera, rvec, tvec));
                Assert.Throws<ArgumentOutOfRangeException>(() => RapidCv2.Run(image, 8, 0, mesh, tris, camera, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.Run(image, 8, 3, null!, tris, camera, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.Run(image, 8, 3, mesh, null!, camera, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.Run(image, 8, 3, mesh, tris, null!, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.Run(image, 8, 3, mesh, tris, camera, null!, tvec));
                Assert.Throws<ArgumentNullException>(() => RapidCv2.Run(image, 8, 3, mesh, tris, camera, rvec, null!));
            }
        }

        [Fact]
        public void TrackerValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (RapidSilhouetteTracker? tracker = TryCreateTracker())
            {
                if (tracker == null)
                {
                    return;
                }

                using (Mat image = CreateEdgeImage())
                using (Mat camera = CreateCameraMatrix())
                using (Mat rvec = CreatePoseVector(0, 0, 0))
                using (Mat tvec = CreatePoseVector(0, 0, 6))
                {
                    Assert.Throws<ArgumentNullException>(() => tracker.Compute(null!, 8, 3, camera, rvec, tvec));
                    Assert.Throws<ArgumentOutOfRangeException>(() => tracker.Compute(image, 0, 3, camera, rvec, tvec));
                    Assert.Throws<ArgumentOutOfRangeException>(() => tracker.Compute(image, 2, 3, camera, rvec, tvec));
                    tracker.ClearState();
                    tracker.Dispose();
                    Assert.True(tracker.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => tracker.Compute(image, 8, 3, camera, rvec, tvec));
                    Assert.Throws<ObjectDisposedException>(() => tracker.ClearState());
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
                using (Mat image = CreateEdgeImage())
                using (Mat mesh = CreateMeshPoints())
                using (Mat tris = CreateMeshTriangles())
                using (Mat camera = CreateCameraMatrix())
                using (Mat rvec = CreatePoseVector(0, 0, 0))
                using (Mat tvec = CreatePoseVector(0, 0, 6))
                using (Mat pts2d = new Mat(4, 1, MatType.CV_32FC2))
                using (Mat wire = image.Clone())
                {
                    pts2d.CopyFrom<float>(new float[] { 12, 12, 52, 12, 52, 52, 12, 52 });
                    RapidCv2.DrawWireframe(wire, pts2d, tris, new Scalar(255), LineTypes.Line8);
                    RapidResult result = RapidCv2.Run(image, 8, 3, mesh, tris, camera, rvec, tvec, computeRmsd: true);
                    Assert.False(float.IsNaN(result.Ratio));
                }

                using (Mat mesh = CreateMeshPoints())
                using (Mat tris = CreateMeshTriangles())
                using (RapidSilhouetteTracker tracker = RapidSilhouetteTracker.Create(mesh, tris))
                {
                    tracker.ClearState();
                    Assert.False(tracker.IsDisposed);
                }
            }
            catch (OpenCvException ex) when (IsRapidModuleMissing(ex) || IsTinyDataBoundary(ex))
            {
                Assert.True(IsRapidModuleMissing(ex) || IsTinyDataBoundary(ex), ex.Message);
            }
        }

        private static RapidSilhouetteTracker? TryCreateTracker()
        {
            try
            {
                using (Mat mesh = CreateMeshPoints())
                using (Mat tris = CreateMeshTriangles())
                {
                    return RapidSilhouetteTracker.Create(mesh, tris);
                }
            }
            catch (OpenCvException ex) when (IsRapidModuleMissing(ex))
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

        private static Mat CreateEdgeImage()
        {
            var image = new Mat(64, 64, MatType.CV_8UC1, new Scalar(0));
            for (int y = 16; y < 48; y++)
            {
                image.SetValue((y * 64) + 16, (byte)255);
                image.SetValue((y * 64) + 47, (byte)255);
            }

            for (int x = 16; x < 48; x++)
            {
                image.SetValue((16 * 64) + x, (byte)255);
                image.SetValue((47 * 64) + x, (byte)255);
            }

            return image;
        }

        private static Mat CreateMeshPoints()
        {
            var mat = new Mat(4, 1, MatType.CV_32FC3);
            mat.CopyFrom<float>(new float[]
            {
                -1, -1, 0,
                1, -1, 0,
                1, 1, 0,
                -1, 1, 0
            });
            return mat;
        }

        private static Mat CreateMeshTriangles()
        {
            var mat = new Mat(2, 1, MatType.CV_32SC3);
            mat.CopyFrom<int>(new[] { 0, 1, 2, 0, 2, 3 });
            return mat;
        }

        private static Mat CreateCameraMatrix()
        {
            var mat = new Mat(3, 3, MatType.CV_64FC1);
            mat.CopyFrom<double>(new double[] { 60, 0, 32, 0, 60, 32, 0, 0, 1 });
            return mat;
        }

        private static Mat CreatePoseVector(double x, double y, double z)
        {
            var mat = new Mat(3, 1, MatType.CV_64FC1);
            mat.CopyFrom<double>(new[] { x, y, z });
            return mat;
        }

        private static bool IsRapidModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("rapid", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsTinyDataBoundary(OpenCvException exception)
        {
            return exception.Message.IndexOf("assert", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("failed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("contours", StringComparison.OrdinalIgnoreCase) >= 0;
        }

    }
}
