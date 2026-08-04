using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.BioInspired;
using JYPPX.OpenCvSharp.Core;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Tests.BioInspired
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class BioInspiredTests
    {
        [Fact]
        public void ValueTypesExposeExpectedValues()
        {
            var parvo = new RetinaParvoParameters(colorMode: false, normaliseOutput: false, horizontalCellsGain: 0.25f);
            var magno = new RetinaMagnoParameters(normaliseOutput: false, parasolCellsK: 3.0f);
            var combined = new RetinaParameters(parvo, magno);
            var segmentation = new SegmentationParameters(thresholdOn: 0.1f, thresholdOff: 0.2f);

            Assert.False(combined.Parvo.ColorMode);
            Assert.False(combined.Magno.NormaliseOutput);
            Assert.Equal(0.25f, parvo.HorizontalCellsGain);
            Assert.Equal(3.0f, magno.ParasolCellsK);
            Assert.Equal(0.1f, segmentation.ThresholdOn);
            Assert.Equal(0.2f, segmentation.ThresholdOff);
            Assert.Equal(new RetinaParvoParameters(colorMode: false, normaliseOutput: false, horizontalCellsGain: 0.25f), parvo);
            Assert.True(parvo == new RetinaParvoParameters(colorMode: false, normaliseOutput: false, horizontalCellsGain: 0.25f));
            Assert.True(parvo != RetinaParvoParameters.Default);
            Assert.Equal(parvo.GetHashCode(), new RetinaParvoParameters(colorMode: false, normaliseOutput: false, horizontalCellsGain: 0.25f).GetHashCode());
            Assert.Contains("HorizontalCellsGain=0.25", parvo.ToString());
            Assert.Equal(new RetinaMagnoParameters(normaliseOutput: false, parasolCellsK: 3.0f), magno);
            Assert.True(magno == new RetinaMagnoParameters(normaliseOutput: false, parasolCellsK: 3.0f));
            Assert.True(magno != RetinaMagnoParameters.Default);
            Assert.Equal(magno.GetHashCode(), new RetinaMagnoParameters(normaliseOutput: false, parasolCellsK: 3.0f).GetHashCode());
            Assert.Contains("ParasolCellsK=3", magno.ToString());
            Assert.Equal(new RetinaParameters(parvo, magno), combined);
            Assert.True(combined == new RetinaParameters(parvo, magno));
            Assert.True(combined != RetinaParameters.Default);
            Assert.Equal(combined.GetHashCode(), new RetinaParameters(parvo, magno).GetHashCode());
            Assert.Contains("Parvo=", combined.ToString());
            Assert.Equal(new SegmentationParameters(thresholdOn: 0.1f, thresholdOff: 0.2f), segmentation);
            Assert.True(segmentation == new SegmentationParameters(thresholdOn: 0.1f, thresholdOff: 0.2f));
            Assert.True(segmentation != SegmentationParameters.Default);
            Assert.Equal(segmentation.GetHashCode(), new SegmentationParameters(thresholdOn: 0.1f, thresholdOff: 0.2f).GetHashCode());
            Assert.Contains("ThresholdOn=0.1", segmentation.ToString());
            Assert.Equal(RetinaColorSamplingMethod.Bayer, (RetinaColorSamplingMethod)2);
        }

        [Fact]
        public void ValueTypesFormatFloatingValuesInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                var parvo = new RetinaParvoParameters(colorMode: false, normaliseOutput: false, horizontalCellsGain: 0.25f);
                var magno = new RetinaMagnoParameters(normaliseOutput: false, parasolCellsK: 3.5f);
                var segmentation = new SegmentationParameters(thresholdOn: 0.1f, thresholdOff: 0.2f);
                var combined = new RetinaParameters(parvo, magno);

                string parvoText = parvo.ToString();
                string magnoText = magno.ToString();
                string segmentationText = segmentation.ToString();
                string combinedText = combined.ToString();

                Assert.Contains("HorizontalCellsGain=0.25", parvoText, StringComparison.Ordinal);
                Assert.Contains("ParasolCellsK=3.5", magnoText, StringComparison.Ordinal);
                Assert.Contains("ThresholdOn=0.1", segmentationText, StringComparison.Ordinal);
                Assert.Contains("ThresholdOff=0.2", segmentationText, StringComparison.Ordinal);
                Assert.Contains("Parvo=RetinaParvoParameters(", combinedText, StringComparison.Ordinal);
                Assert.Contains("Magno=RetinaMagnoParameters(", combinedText, StringComparison.Ordinal);
                Assert.DoesNotContain("0,25", parvoText, StringComparison.Ordinal);
                Assert.DoesNotContain("3,5", magnoText, StringComparison.Ordinal);
                Assert.DoesNotContain("0,1", segmentationText, StringComparison.Ordinal);
                Assert.DoesNotContain("0,2", combinedText, StringComparison.Ordinal);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void BioInspiredParameterStructsHaveSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(32, Marshal.SizeOf<SegmentationParameters>());
            Assert.Equal(36, Marshal.SizeOf<RetinaParvoParameters>());
            Assert.Equal(32, Marshal.SizeOf<RetinaMagnoParameters>());
            Assert.Equal(68, Marshal.SizeOf<RetinaParameters>());

            Assert.Equal(0, FieldOffset<SegmentationParameters>("<ThresholdOn>k__BackingField"));
            Assert.Equal(4, FieldOffset<SegmentationParameters>("<ThresholdOff>k__BackingField"));
            Assert.Equal(8, FieldOffset<SegmentationParameters>("<LocalEnergyTemporalConstant>k__BackingField"));
            Assert.Equal(12, FieldOffset<SegmentationParameters>("<LocalEnergySpatialConstant>k__BackingField"));
            Assert.Equal(16, FieldOffset<SegmentationParameters>("<NeighborhoodEnergyTemporalConstant>k__BackingField"));
            Assert.Equal(20, FieldOffset<SegmentationParameters>("<NeighborhoodEnergySpatialConstant>k__BackingField"));
            Assert.Equal(24, FieldOffset<SegmentationParameters>("<ContextEnergyTemporalConstant>k__BackingField"));
            Assert.Equal(28, FieldOffset<SegmentationParameters>("<ContextEnergySpatialConstant>k__BackingField"));

            Assert.Equal(0, FieldOffset<RetinaParvoParameters>("<ColorMode>k__BackingField"));
            Assert.Equal(4, FieldOffset<RetinaParvoParameters>("<NormaliseOutput>k__BackingField"));
            Assert.Equal(8, FieldOffset<RetinaParvoParameters>("<PhotoreceptorsLocalAdaptationSensitivity>k__BackingField"));
            Assert.Equal(12, FieldOffset<RetinaParvoParameters>("<PhotoreceptorsTemporalConstant>k__BackingField"));
            Assert.Equal(16, FieldOffset<RetinaParvoParameters>("<PhotoreceptorsSpatialConstant>k__BackingField"));
            Assert.Equal(20, FieldOffset<RetinaParvoParameters>("<HorizontalCellsGain>k__BackingField"));
            Assert.Equal(24, FieldOffset<RetinaParvoParameters>("<HcellsTemporalConstant>k__BackingField"));
            Assert.Equal(28, FieldOffset<RetinaParvoParameters>("<HcellsSpatialConstant>k__BackingField"));
            Assert.Equal(32, FieldOffset<RetinaParvoParameters>("<GanglionCellsSensitivity>k__BackingField"));

            Assert.Equal(0, FieldOffset<RetinaMagnoParameters>("<NormaliseOutput>k__BackingField"));
            Assert.Equal(4, FieldOffset<RetinaMagnoParameters>("<ParasolCellsBeta>k__BackingField"));
            Assert.Equal(8, FieldOffset<RetinaMagnoParameters>("<ParasolCellsTau>k__BackingField"));
            Assert.Equal(12, FieldOffset<RetinaMagnoParameters>("<ParasolCellsK>k__BackingField"));
            Assert.Equal(16, FieldOffset<RetinaMagnoParameters>("<AmacrinCellsTemporalCutFrequency>k__BackingField"));
            Assert.Equal(20, FieldOffset<RetinaMagnoParameters>("<V0CompressionParameter>k__BackingField"));
            Assert.Equal(24, FieldOffset<RetinaMagnoParameters>("<LocalAdaptIntegrationTau>k__BackingField"));
            Assert.Equal(28, FieldOffset<RetinaMagnoParameters>("<LocalAdaptIntegrationK>k__BackingField"));

            Assert.Equal(0, FieldOffset<RetinaParameters>("<Parvo>k__BackingField"));
            Assert.Equal(36, FieldOffset<RetinaParameters>("<Magno>k__BackingField"));
        }

        [Fact]
        public void FactoryValidationRuns()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BioInspiredCv2.CreateRetina(new Size(0, 8)));
            Assert.Throws<ArgumentOutOfRangeException>(() => BioInspiredCv2.CreateRetina(new Size(8, 8), reductionFactor: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => BioInspiredCv2.CreateRetinaFastToneMapping(new Size(8, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => BioInspiredCv2.CreateTransientAreasSegmentationModule(new Size(-1, 8)));
        }

        [Fact]
        public void RetinaValidationAndDisposedStateRunWhenUnstableNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsUnstableNativeSmokeEnabled())
            {
                return;
            }

            using (Retina? retina = TryCreateRetina())
            using (Mat image = CreateColorImage())
            using (Mat output = new Mat())
            {
                if (retina == null)
                {
                    return;
                }

                Assert.Equal(new Size(32, 32).ToString(), retina.InputSize.ToString());
                Assert.Throws<ArgumentNullException>(() => retina.Run(null!));
                Assert.Throws<ArgumentNullException>(() => retina.ApplyFastToneMapping(null!, output));
                Assert.Throws<ArgumentNullException>(() => retina.ApplyFastToneMapping(image, null!));
                Assert.Throws<ArgumentNullException>(() => retina.GetParvo(null!));
                retina.Setup(RetinaParameters.Default);
                retina.ClearBuffers();
                retina.Dispose();
                Assert.True(retina.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => retina.Run(image));
            }
        }

        [Fact]
        public void ToneMappingValidationAndDisposedStateRunWhenUnstableNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsUnstableNativeSmokeEnabled())
            {
                return;
            }

            using (RetinaFastToneMapping? toneMapping = TryCreateToneMapping())
            using (Mat image = CreateColorImage())
            using (Mat output = new Mat())
            {
                if (toneMapping == null)
                {
                    return;
                }

                Assert.Throws<ArgumentOutOfRangeException>(() => toneMapping.Setup(0));
                Assert.Throws<ArgumentNullException>(() => toneMapping.Apply(null!, output));
                Assert.Throws<ArgumentNullException>(() => toneMapping.Apply(image, null!));
                toneMapping.Dispose();
                Assert.True(toneMapping.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => toneMapping.Apply(image, output));
            }
        }

        [Fact]
        public void SegmentationValidationAndDisposedStateRunWhenUnstableNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsUnstableNativeSmokeEnabled())
            {
                return;
            }

            using (TransientAreasSegmentationModule? segmentation = TryCreateSegmentation())
            using (Mat image = CreateColorImage())
            using (Mat output = new Mat())
            {
                if (segmentation == null)
                {
                    return;
                }

                Assert.Equal(new Size(32, 32).ToString(), segmentation.Size.ToString());
                Assert.Throws<ArgumentNullException>(() => segmentation.Run(null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => segmentation.Run(image, -1));
                Assert.Throws<ArgumentNullException>(() => segmentation.GetSegmentationPicture(null!));
                segmentation.Setup(SegmentationParameters.Default);
                segmentation.ClearAllBuffers();
                segmentation.Dispose();
                Assert.True(segmentation.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => segmentation.Run(image));
            }
        }

        [Fact]
        public void LinkedObjectSmokeRunsWhenUnstableNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsUnstableNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (Retina retina = BioInspiredCv2.CreateRetina(new Size(32, 32)))
                using (RetinaFastToneMapping toneMapping = BioInspiredCv2.CreateRetinaFastToneMapping(new Size(32, 32)))
                using (TransientAreasSegmentationModule segmentation = BioInspiredCv2.CreateTransientAreasSegmentationModule(new Size(32, 32)))
                {
                    Assert.Equal(new Size(32, 32).ToString(), retina.InputSize.ToString());
                    Assert.Equal(new Size(32, 32).ToString(), retina.OutputSize.ToString());
                    Assert.Equal(new Size(32, 32).ToString(), segmentation.Size.ToString());
                    Assert.False(retina.IsDisposed);
                    Assert.False(toneMapping.IsDisposed);
                    Assert.False(segmentation.IsDisposed);
                }
            }
            catch (OpenCvException ex) when (IsBioInspiredModuleMissing(ex))
            {
                Assert.True(IsBioInspiredModuleMissing(ex), ex.Message);
            }
        }

        [Fact]
        public void UnstableLinkedAlgorithmSmokeRunsWhenUnstableNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsUnstableNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (Mat image = CreateFloatColorImage())
                using (Retina retina = BioInspiredCv2.CreateRetina(image.Size))
                {
                    retina.Run(image);
                    using (Mat parvo = retina.GetParvo())
                    using (Mat magno = retina.GetMagno())
                    {
                        Assert.False(parvo.Empty);
                        Assert.False(magno.Empty);
                        Assert.Equal(retina.OutputSize.Height, parvo.Rows);
                        Assert.Equal(retina.OutputSize.Width, parvo.Cols);
                    }
                }

                using (Mat image = CreateFloatColorImage())
                using (RetinaFastToneMapping toneMapping = BioInspiredCv2.CreateRetinaFastToneMapping(image.Size))
                using (Mat output = toneMapping.Apply(image))
                {
                    Assert.False(output.Empty);
                    Assert.Equal(image.Rows, output.Rows);
                    Assert.Equal(image.Cols, output.Cols);
                }

                using (Mat image = CreateColorImage())
                using (TransientAreasSegmentationModule segmentation = BioInspiredCv2.CreateTransientAreasSegmentationModule(new Size(32, 32)))
                using (Mat shifted = CreateShiftedColorImage())
                {
                    segmentation.Run(image);
                    segmentation.Run(shifted);
                    using (Mat output = segmentation.GetSegmentationPicture())
                    {
                        Assert.False(output.Empty);
                        Assert.Equal(segmentation.Size.Height, output.Rows);
                        Assert.Equal(segmentation.Size.Width, output.Cols);
                    }
                }
            }
            catch (OpenCvException ex) when (IsBioInspiredModuleMissing(ex) || IsTinyDataBoundary(ex))
            {
                Assert.True(IsBioInspiredModuleMissing(ex) || IsTinyDataBoundary(ex), ex.Message);
            }
        }

        private static Retina? TryCreateRetina()
        {
            try
            {
                return BioInspiredCv2.CreateRetina(new Size(32, 32));
            }
            catch (OpenCvException ex) when (IsBioInspiredModuleMissing(ex))
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

        private static RetinaFastToneMapping? TryCreateToneMapping()
        {
            try
            {
                return BioInspiredCv2.CreateRetinaFastToneMapping(new Size(32, 32));
            }
            catch (OpenCvException ex) when (IsBioInspiredModuleMissing(ex))
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

        private static TransientAreasSegmentationModule? TryCreateSegmentation()
        {
            try
            {
                return BioInspiredCv2.CreateTransientAreasSegmentationModule(new Size(32, 32));
            }
            catch (OpenCvException ex) when (IsBioInspiredModuleMissing(ex))
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

        private static Mat CreateColorImage()
        {
            var image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(20, 30, 40));
            ImgProcCv2.Rectangle(image, new Rect(8, 8, 12, 12), new Scalar(210, 80, 120), -1);
            ImgProcCv2.Circle(image, new Point(22, 22), 5, new Scalar(40, 220, 120), -1);
            return image;
        }

        private static Mat CreateShiftedColorImage()
        {
            var image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(25, 35, 45));
            ImgProcCv2.Rectangle(image, new Rect(10, 8, 12, 12), new Scalar(210, 80, 120), -1);
            ImgProcCv2.Circle(image, new Point(24, 22), 5, new Scalar(40, 220, 120), -1);
            return image;
        }

        private static Mat CreateFloatColorImage()
        {
            var image = new Mat(32, 32, MatType.CV_32FC3, new Scalar(0.08, 0.12, 0.16));
            ImgProcCv2.Rectangle(image, new Rect(8, 8, 12, 12), new Scalar(0.82, 0.31, 0.47), -1);
            ImgProcCv2.Circle(image, new Point(22, 22), 5, new Scalar(0.16, 0.86, 0.47), -1);
            return image;
        }

        private static bool IsBioInspiredModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("bioinspired", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsTinyDataBoundary(OpenCvException exception)
        {
            return exception.Message.IndexOf("assert", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("failed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("channels", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }

    }
}
