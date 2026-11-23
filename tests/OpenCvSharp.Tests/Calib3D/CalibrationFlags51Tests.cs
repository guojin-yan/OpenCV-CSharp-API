using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class CalibrationFlags51Tests
    {
        [Theory]
        [InlineData(false, false, 5)]
        [InlineData(true, false, 5)]
        [InlineData(false, true, 14)]
        [InlineData(true, true, 14)]
        public void SchurSolverSelectionPreservesDistortionModel(bool disableSchur, bool tiltedModel, int coefficientCount)
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            CalibrationFlags flags = (disableSchur ? CalibrationFlags.DisableSchurComplement : CalibrationFlags.None) |
                (tiltedModel ? CalibrationFlags.TiltedModel : CalibrationFlags.None);
            CalibrationTestData.CreateSyntheticCalibrationData(out Point3f[][] objectPoints, out Point2f[][] imagePoints);
            CalibrationResult result = Calib3DCv2.CalibrateCamera(objectPoints, imagePoints, CalibrationTestData.ImageSize, flags);
            using (result.CameraMatrix)
            using (result.DistCoeffs)
            using (result.Rvecs)
            using (result.Tvecs)
            {
                Assert.InRange(result.ReprojectionError, 0.0, 0.01);
                Assert.Equal(coefficientCount, result.DistCoeffs.Rows * result.DistCoeffs.Cols);
                Assert.InRange(result.CameraMatrix.GetValue<double>(0, 0), 780.0, 820.0);
                Assert.InRange(result.CameraMatrix.GetValue<double>(1, 1), 800.0, 840.0);
                Assert.Equal(objectPoints.Length, result.Rvecs.Rows);
            }
        }
    }
}
