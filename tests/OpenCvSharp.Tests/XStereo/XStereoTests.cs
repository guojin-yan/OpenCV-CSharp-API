using System;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;
using OpenCvSharp.XStereo;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace OpenCvSharp.Tests.XStereo
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class XStereoTests
    {
        [Fact]
        public void ValueTypesAndEnumsExposeExpectedValues()
        {
            var match = new MatchQuasiDense(new Point(1, 2), new Point(3, 4), 0.75f);
            var parameters = new PropagationParameters(5, 5, 2, 2, 0.8f, 0.1f, 3, 1, 7, 2, 10, 0.01f, 0.02f, 4, 100);

            Assert.Equal(1, match.P0.X);
            Assert.Equal(4, match.P1.Y);
            Assert.Equal(0.75f, match.Correlation);
            Assert.Equal(new MatchQuasiDense(new Point(1, 2), new Point(3, 4), 0.75f), match);
            Assert.True(match == new MatchQuasiDense(new Point(1, 2), new Point(3, 4), 0.75f));
            Assert.True(match != new MatchQuasiDense(new Point(1, 2), new Point(3, 4), 0.5f));
            Assert.False(match.Equals("not a quasi dense match"));
            Assert.Equal(new MatchQuasiDense(new Point(1, 2), new Point(3, 4), 0.75f).GetHashCode(), match.GetHashCode());
            Assert.Equal("{P0={X=1,Y=2},P1={X=3,Y=4},Correlation=0.75}", match.ToString());
            Assert.Equal(5, parameters.CorrWinSizeX);
            Assert.Equal(100, parameters.GftMaxNumFeatures);
            Assert.Equal(new PropagationParameters(5, 5, 2, 2, 0.8f, 0.1f, 3, 1, 7, 2, 10, 0.01f, 0.02f, 4, 100), parameters);
            Assert.True(parameters == new PropagationParameters(5, 5, 2, 2, 0.8f, 0.1f, 3, 1, 7, 2, 10, 0.01f, 0.02f, 4, 100));
            Assert.True(parameters != new PropagationParameters(5, 5, 2, 2, 0.8f, 0.1f, 3, 1, 7, 2, 10, 0.01f, 0.02f, 4, 101));
            Assert.False(parameters.Equals("not propagation parameters"));
            Assert.Equal(new PropagationParameters(5, 5, 2, 2, 0.8f, 0.1f, 3, 1, 7, 2, 10, 0.01f, 0.02f, 4, 100).GetHashCode(), parameters.GetHashCode());
            Assert.Equal("{CorrWinSizeX=5,CorrWinSizeY=5,BorderX=2,BorderY=2,CorrelationThreshold=0.8,TextureThreshold=0.1,NeighborhoodSize=3,DisparityGradient=1,LkTemplateSize=7,LkPyrLevel=2,LkTermParam1=10,LkTermParam2=0.01,GftQualityThreshold=0.02,GftMinSeparationDistance=4,GftMaxNumFeatures=100}", parameters.ToString());
            Assert.Equal(CensusTransformType.StarKernel, (CensusTransformType)6);
            Assert.Equal(StereoSpeckleRemovalAlgorithm.Average, (StereoSpeckleRemovalAlgorithm)1);
            Assert.Equal(StereoSubPixelInterpolationMethod.Symmetric, (StereoSubPixelInterpolationMethod)1);
            Assert.Equal(StereoBinarySGBMMode.HH, (StereoBinarySGBMMode)1);
        }

        [Fact]
        public void ValueTypesFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                Assert.Equal(
                    "{P0={X=1,Y=2},P1={X=3,Y=4},Correlation=0.75}",
                    new MatchQuasiDense(new Point(1, 2), new Point(3, 4), 0.75f).ToString());
                Assert.Equal(
                    "{CorrWinSizeX=5,CorrWinSizeY=5,BorderX=2,BorderY=2,CorrelationThreshold=0.8,TextureThreshold=0.1,NeighborhoodSize=3,DisparityGradient=1,LkTemplateSize=7,LkPyrLevel=2,LkTermParam1=10,LkTermParam2=0.01,GftQualityThreshold=0.02,GftMinSeparationDistance=4,GftMaxNumFeatures=100}",
                    new PropagationParameters(5, 5, 2, 2, 0.8f, 0.1f, 3, 1, 7, 2, 10, 0.01f, 0.02f, 4, 100).ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void MatchQuasiDenseHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(20, Marshal.SizeOf<MatchQuasiDense>());

            Assert.Equal(0, FieldOffset<MatchQuasiDense>("<P0>k__BackingField"));
            Assert.Equal(8, FieldOffset<MatchQuasiDense>("<P1>k__BackingField"));
            Assert.Equal(16, FieldOffset<MatchQuasiDense>("<Correlation>k__BackingField"));
        }

        [Fact]
        public void PropagationParametersHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(60, Marshal.SizeOf<PropagationParameters>());

            Assert.Equal(0, FieldOffset<PropagationParameters>("<CorrWinSizeX>k__BackingField"));
            Assert.Equal(4, FieldOffset<PropagationParameters>("<CorrWinSizeY>k__BackingField"));
            Assert.Equal(8, FieldOffset<PropagationParameters>("<BorderX>k__BackingField"));
            Assert.Equal(12, FieldOffset<PropagationParameters>("<BorderY>k__BackingField"));
            Assert.Equal(16, FieldOffset<PropagationParameters>("<CorrelationThreshold>k__BackingField"));
            Assert.Equal(20, FieldOffset<PropagationParameters>("<TextureThreshold>k__BackingField"));
            Assert.Equal(24, FieldOffset<PropagationParameters>("<NeighborhoodSize>k__BackingField"));
            Assert.Equal(28, FieldOffset<PropagationParameters>("<DisparityGradient>k__BackingField"));
            Assert.Equal(32, FieldOffset<PropagationParameters>("<LkTemplateSize>k__BackingField"));
            Assert.Equal(36, FieldOffset<PropagationParameters>("<LkPyrLevel>k__BackingField"));
            Assert.Equal(40, FieldOffset<PropagationParameters>("<LkTermParam1>k__BackingField"));
            Assert.Equal(44, FieldOffset<PropagationParameters>("<LkTermParam2>k__BackingField"));
            Assert.Equal(48, FieldOffset<PropagationParameters>("<GftQualityThreshold>k__BackingField"));
            Assert.Equal(52, FieldOffset<PropagationParameters>("<GftMinSeparationDistance>k__BackingField"));
            Assert.Equal(56, FieldOffset<PropagationParameters>("<GftMaxNumFeatures>k__BackingField"));
        }

        [Fact]
        public void StaticValidationRuns()
        {
            using (StereoBinaryBM? nativeBoundary = TryCreateBinaryBM())
            {
                if (nativeBoundary == null)
                {
                    return;
                }
            }

            using (Mat gray = CreateLeftImage())
            using (Mat right = CreateRightImage())
            using (Mat dist = new Mat())
            using (Mat dist2 = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => XStereoCv2.CensusTransform(null!, 5, dist));
                Assert.Throws<ArgumentOutOfRangeException>(() => XStereoCv2.CensusTransform(gray, 0, dist));
                Assert.Throws<ArgumentNullException>(() => XStereoCv2.CensusTransform(gray, 5, null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => XStereoCv2.CensusTransform(gray, 5, dist, (CensusTransformType)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XStereoCv2.CensusTransform(gray, right, 5, dist, dist2, (CensusTransformType)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XStereoCv2.ModifiedCensusTransform(gray, 5, dist, (CensusTransformType)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XStereoCv2.ModifiedCensusTransform(gray, right, 5, dist, dist2, (CensusTransformType)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XStereoCv2.SymmetricCensusTransform(gray, 5, dist, (CensusTransformType)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XStereoCv2.SymmetricCensusTransform(gray, right, 5, dist, dist2, (CensusTransformType)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => StereoBinarySGBM.Create(0, 16, 3, mode: (StereoBinarySGBMMode)99));
                Assert.Throws<ArgumentNullException>(() => XStereoCv2.StarCensusTransform(null!, 5, dist));
            }
        }

        [Fact]
        public void BinaryBMValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (StereoBinaryBM? matcher = TryCreateBinaryBM())
            {
                if (matcher == null)
                {
                    return;
                }

                using (Mat left = CreateLeftImage())
                using (Mat right = CreateRightImage())
                using (Mat disparity = new Mat())
                {
                    Assert.Throws<ArgumentNullException>(() => matcher.Compute(null!, right, disparity));
                    Assert.Throws<ArgumentNullException>(() => matcher.Compute(left, null!, disparity));
                    Assert.Throws<ArgumentNullException>(() => matcher.Compute(left, right, null!));
                    matcher.MinDisparity = 0;
                    matcher.NumDisparities = 16;
                    matcher.BlockSize = 9;
                    matcher.PreFilterType = StereoBinaryBMPreFilterType.XSobel;
                    matcher.SpeckleRemovalTechnique = StereoSpeckleRemovalAlgorithm.Average;
                    matcher.BinaryKernelType = CensusTransformType.Dense;
                    matcher.UsePrefilter = true;
                    Assert.Throws<ArgumentOutOfRangeException>(() => matcher.PreFilterType = (StereoBinaryBMPreFilterType)99);
                    Assert.Throws<ArgumentOutOfRangeException>(() => matcher.SpeckleRemovalTechnique = (StereoSpeckleRemovalAlgorithm)99);
                    Assert.Throws<ArgumentOutOfRangeException>(() => matcher.BinaryKernelType = (CensusTransformType)99);
                    matcher.Dispose();
                    Assert.True(matcher.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => matcher.Compute(left, right, disparity));
                    Assert.Throws<ObjectDisposedException>(() => matcher.Compute(left, right));
                }
            }
        }

        [Fact]
        public void BinarySGBMValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (StereoBinarySGBM? matcher = TryCreateBinarySGBM())
            {
                if (matcher == null)
                {
                    return;
                }

                using (Mat left = CreateLeftImage())
                using (Mat right = CreateRightImage())
                using (Mat disparity = new Mat())
                {
                    Assert.Throws<ArgumentNullException>(() => matcher.Compute(null!, right, disparity));
                    Assert.Throws<ArgumentNullException>(() => matcher.Compute(left, null!, disparity));
                    Assert.Throws<ArgumentNullException>(() => matcher.Compute(left, right, null!));
                    matcher.Mode = StereoBinarySGBMMode.Sgbm;
                    matcher.SpeckleRemovalTechnique = StereoSpeckleRemovalAlgorithm.Average;
                    matcher.BinaryKernelType = CensusTransformType.Dense;
                    matcher.SubPixelInterpolationMethod = StereoSubPixelInterpolationMethod.Quadratic;
                    Assert.Throws<ArgumentOutOfRangeException>(() => matcher.Mode = (StereoBinarySGBMMode)99);
                    Assert.Throws<ArgumentOutOfRangeException>(() => matcher.SpeckleRemovalTechnique = (StereoSpeckleRemovalAlgorithm)99);
                    Assert.Throws<ArgumentOutOfRangeException>(() => matcher.BinaryKernelType = (CensusTransformType)99);
                    Assert.Throws<ArgumentOutOfRangeException>(() => matcher.SubPixelInterpolationMethod = (StereoSubPixelInterpolationMethod)99);
                    matcher.Dispose();
                    Assert.True(matcher.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => matcher.Compute(left, right, disparity));
                    Assert.Throws<ObjectDisposedException>(() => matcher.Compute(left, right));
                }
            }
        }

        [Fact]
        public void QuasiDenseValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (QuasiDenseStereo? matcher = TryCreateQuasiDense())
            {
                if (matcher == null)
                {
                    return;
                }

                using (Mat left = CreateLeftImage())
                using (Mat right = CreateRightImage())
                {
                    Assert.Throws<ArgumentNullException>(() => matcher.Process(null!, right));
                    Assert.Throws<ArgumentNullException>(() => matcher.Process(left, null!));
                    Assert.Throws<ArgumentNullException>(() => matcher.GetDisparity(null!));
                    matcher.Dispose();
                    Assert.True(matcher.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => matcher.Process(left, right));
                    Assert.Throws<ObjectDisposedException>(() => matcher.GetSparseMatches());
                    Assert.Throws<ObjectDisposedException>(() => matcher.GetDenseMatches());
                    Assert.Throws<ObjectDisposedException>(() => matcher.GetDisparity());
                }
            }
        }

        [Fact]
        public void QuasiDenseParametersRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (QuasiDenseStereo? matcher = TryCreateQuasiDense())
            {
                if (matcher == null)
                {
                    return;
                }

                var parameters = new PropagationParameters(
                    5,
                    5,
                    2,
                    2,
                    0.8f,
                    0.1f,
                    3,
                    1,
                    7,
                    2,
                    10,
                    0.01f,
                    0.02f,
                    4,
                    100);

                matcher.Parameters = parameters;

                PropagationParameters roundTrip = matcher.Parameters;
                Assert.Equal(parameters.CorrWinSizeX, roundTrip.CorrWinSizeX);
                Assert.Equal(parameters.CorrWinSizeY, roundTrip.CorrWinSizeY);
                Assert.Equal(parameters.BorderX, roundTrip.BorderX);
                Assert.Equal(parameters.BorderY, roundTrip.BorderY);
                Assert.Equal(parameters.CorrelationThreshold, roundTrip.CorrelationThreshold);
                Assert.Equal(parameters.TextureThreshold, roundTrip.TextureThreshold);
                Assert.Equal(parameters.NeighborhoodSize, roundTrip.NeighborhoodSize);
                Assert.Equal(parameters.DisparityGradient, roundTrip.DisparityGradient);
                Assert.Equal(parameters.LkTemplateSize, roundTrip.LkTemplateSize);
                Assert.Equal(parameters.LkPyrLevel, roundTrip.LkPyrLevel);
                Assert.Equal(parameters.LkTermParam1, roundTrip.LkTermParam1);
                Assert.Equal(parameters.LkTermParam2, roundTrip.LkTermParam2);
                Assert.Equal(parameters.GftQualityThreshold, roundTrip.GftQualityThreshold);
                Assert.Equal(parameters.GftMinSeparationDistance, roundTrip.GftMinSeparationDistance);
                Assert.Equal(parameters.GftMaxNumFeatures, roundTrip.GftMaxNumFeatures);
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
                using (Mat left = CreateLeftImage())
                using (Mat right = CreateRightImage())
                using (Mat census = XStereoCv2.CensusTransform(left, 5))
                {
                    Assert.False(census.Empty);
                    Assert.Equal(left.Rows, census.Rows);
                    Assert.Equal(left.Cols, census.Cols);
                }

                using (Mat left = CreateLeftImage())
                using (Mat right = CreateRightImage())
                using (StereoBinaryBM bm = StereoBinaryBM.Create(16, 9))
                using (Mat disparity = bm.Compute(left, right))
                {
                    Assert.False(disparity.Empty);
                    Assert.Equal(left.Rows, disparity.Rows);
                    Assert.Equal(left.Cols, disparity.Cols);
                }

                using (Mat left = CreateLeftImage())
                using (Mat right = CreateRightImage())
                using (StereoBinarySGBM sgbm = StereoBinarySGBM.Create(0, 16, 3))
                using (Mat disparity = sgbm.Compute(left, right))
                {
                    Assert.False(disparity.Empty);
                    Assert.Equal(left.Rows, disparity.Rows);
                    Assert.Equal(left.Cols, disparity.Cols);
                }

                using (Mat left = CreateLeftImage())
                using (Mat right = CreateRightImage())
                using (QuasiDenseStereo quasiDense = QuasiDenseStereo.Create(left.Size))
                {
                    quasiDense.Process(left, right);
                    MatchQuasiDense[] sparse = quasiDense.GetSparseMatches();
                    MatchQuasiDense[] dense = quasiDense.GetDenseMatches();
                    using (Mat disparity = quasiDense.GetDisparity())
                    {
                        Assert.NotNull(sparse);
                        Assert.NotNull(dense);
                        Assert.False(disparity.Empty);
                    }
                }
            }
            catch (OpenCvException ex) when (IsXStereoModuleMissing(ex) || IsTinyDataBoundary(ex))
            {
                Assert.True(IsXStereoModuleMissing(ex) || IsTinyDataBoundary(ex), ex.Message);
            }
        }

        private static StereoBinaryBM? TryCreateBinaryBM()
        {
            try
            {
                return StereoBinaryBM.Create(16, 9);
            }
            catch (OpenCvException ex) when (IsXStereoModuleMissing(ex))
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

        private static StereoBinarySGBM? TryCreateBinarySGBM()
        {
            try
            {
                return StereoBinarySGBM.Create(0, 16, 3);
            }
            catch (OpenCvException ex) when (IsXStereoModuleMissing(ex))
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

        private static QuasiDenseStereo? TryCreateQuasiDense()
        {
            try
            {
                return QuasiDenseStereo.Create(new Size(48, 32));
            }
            catch (OpenCvException ex) when (IsXStereoModuleMissing(ex))
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

        private static Mat CreateLeftImage()
        {
            var image = new Mat(32, 48, MatType.CV_8UC1, new Scalar(30));
            ImgProcCv2.Rectangle(image, new Rect(16, 8, 16, 14), new Scalar(220), -1);
            ImgProcCv2.Circle(image, new Point(32, 24), 5, new Scalar(130), -1);
            return image;
        }

        private static Mat CreateRightImage()
        {
            var image = new Mat(32, 48, MatType.CV_8UC1, new Scalar(30));
            ImgProcCv2.Rectangle(image, new Rect(13, 8, 16, 14), new Scalar(220), -1);
            ImgProcCv2.Circle(image, new Point(29, 24), 5, new Scalar(130), -1);
            return image;
        }

        private static bool IsXStereoModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("xstereo", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsTinyDataBoundary(OpenCvException exception)
        {
            return exception.Message.IndexOf("assert", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("failed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("size", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("disparit", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }

    }
}
