using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Face;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Face
{
    public sealed class FaceTests
    {
        private static readonly string FactualOpenCvInstallCacheName =
            "opencv-" + global::JYPPX.OpenCvSharp.OpenCvSharpBuildInfo.OpenCvVersion + "-windows-x64";

        [Fact]
        public void ResultObjectsExposeConstructorValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FacePredictionResult(9, -1.0));

            var prediction = new FacePrediction(7, 0.25);
            var result = new FacePredictionResult(9, 1.5);
            var fit = new FacemarkFitResult(true, new[]
            {
                new[] { new Point2f(1.5F, 2.5F), new Point2f(3.5F, 4.5F) }
            });

            Assert.Equal(7, prediction.Label);
            Assert.Equal(0.25, prediction.Confidence, 3);
            Assert.Equal(new FacePrediction(7, 0.25), prediction);
            Assert.True(prediction == new FacePrediction(7, 0.25));
            Assert.True(prediction != new FacePrediction(8, 0.25));
            Assert.False(prediction.Equals("not a prediction"));
            Assert.Equal(new FacePrediction(7, 0.25).GetHashCode(), prediction.GetHashCode());
            Assert.Equal("{Label=7,Confidence=0.25}", prediction.ToString());
            Assert.Equal(9, result.Label);
            Assert.Equal(1.5, result.Distance, 3);
            Assert.Equal(new FacePredictionResult(9, 1.5), result);
            Assert.True(result == new FacePredictionResult(9, 1.5));
            Assert.True(result != new FacePredictionResult(10, 1.5));
            Assert.False(result.Equals("not a prediction result"));
            Assert.Equal(new FacePredictionResult(9, 1.5).GetHashCode(), result.GetHashCode());
            Assert.Equal("{Label=9,Distance=1.5}", result.ToString());
            Assert.True(fit.Success);
            Assert.Equal(1, fit.FaceCount);
            Assert.Equal(2, fit.LandmarkCount);
            Assert.True(fit.HasLandmarks);
            Assert.Equal(new Point2f(1.5F, 2.5F), fit.Landmarks[0][0]);
            Assert.Equal(new Point2f(3.5F, 4.5F), fit.FlattenedLandmarks[1]);
            Assert.Equal("FacemarkFitResult(Success=True, FaceCount=1, LandmarkCount=2, HasLandmarks=True)", fit.ToString());
        }

        [Fact]
        public void PredictionValueTypesFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                Assert.Equal("{Label=7,Confidence=0.25}", new FacePrediction(7, 0.25).ToString());
                Assert.Equal("{Label=9,Distance=1.5}", new FacePredictionResult(9, 1.5).ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void FacemarkFitResultClonesLandmarkGroups()
        {
            Point2f[][] landmarks =
            {
                new[] { new Point2f(1.0F, 2.0F), new Point2f(3.0F, 4.0F) },
                new[] { new Point2f(5.0F, 6.0F) }
            };

            var fit = new FacemarkFitResult(false, landmarks);
            landmarks[0][0] = new Point2f(9.0F, 9.0F);

            Assert.False(fit.Success);
            Assert.Equal(2, fit.FaceCount);
            Assert.Equal(3, fit.LandmarkCount);
            Assert.Equal(new Point2f(1.0F, 2.0F), fit.Landmarks[0][0]);
            Assert.Equal(new Point2f(3.0F, 4.0F), fit.FlattenedLandmarks[1]);

            Point2f[][] returnedLandmarks = fit.Landmarks;
            Point2f[] returnedFlattenedLandmarks = fit.FlattenedLandmarks;
            returnedLandmarks[0][1] = new Point2f(8.0F, 8.0F);
            returnedFlattenedLandmarks[1] = new Point2f(7.0F, 7.0F);

            Assert.Equal(new Point2f(3.0F, 4.0F), fit.Landmarks[0][1]);
            Assert.Equal(new Point2f(3.0F, 4.0F), fit.FlattenedLandmarks[1]);
            Assert.True(fit.HasLandmarks);
            Assert.Equal("FacemarkFitResult(Success=False, FaceCount=2, LandmarkCount=3, HasLandmarks=True)", fit.ToString());
            Assert.Throws<ArgumentNullException>(() => new FacemarkFitResult(true, null!));
            Assert.Throws<ArgumentException>(() => new FacemarkFitResult(true, new Point2f[][] { null! }));
        }

        [Fact]
        public void FacemarkFitResultReportsLandmarkPresence()
        {
            var empty = new FacemarkFitResult(false, Array.Empty<Point2f[]>());
            var groupWithoutPoints = new FacemarkFitResult(true, new[] { Array.Empty<Point2f>() });
            var groupWithPoint = new FacemarkFitResult(true, new[] { new[] { new Point2f(1.0F, 2.0F) } });

            Assert.False(empty.HasLandmarks);
            Assert.Equal(0, empty.FaceCount);
            Assert.Equal(0, empty.LandmarkCount);
            Assert.Equal("FacemarkFitResult(Success=False, FaceCount=0, LandmarkCount=0, HasLandmarks=False)", empty.ToString());
            Assert.False(groupWithoutPoints.HasLandmarks);
            Assert.Equal(1, groupWithoutPoints.FaceCount);
            Assert.Equal(0, groupWithoutPoints.LandmarkCount);
            Assert.Equal("FacemarkFitResult(Success=True, FaceCount=1, LandmarkCount=0, HasLandmarks=False)", groupWithoutPoints.ToString());
            Assert.True(groupWithPoint.HasLandmarks);
            Assert.Equal(1, groupWithPoint.FaceCount);
            Assert.Equal(1, groupWithPoint.LandmarkCount);
            Assert.Equal("FacemarkFitResult(Success=True, FaceCount=1, LandmarkCount=1, HasLandmarks=True)", groupWithPoint.ToString());
        }

        [Fact]
        public void FacemarkLbfParametersCanBeCopiedAndClonedWithoutNativeRuntime()
        {
            var parameters = new FacemarkLBFParams
            {
                ShapeOffset = 1.25,
                CascadeFace = "face.xml",
                Verbose = true,
                NLandmarks = 29,
                InitialShapeCount = 2,
                StageCount = 3,
                TreeCount = 4,
                TreeDepth = 5,
                BaggingOverlap = 0.35,
                ModelFilename = "model.yaml",
                SaveModel = false,
                Seed = 123,
                FeatureCounts = new[] { 9, 8, 7 },
                RadiusValues = new[] { 0.3, 0.2, 0.1 },
                LeftPupilIndices = new[] { 1, 2 },
                RightPupilIndices = new[] { 3, 4 },
                DetectRegion = new Rect(1, 2, 3, 4)
            };

            FacemarkLBFParams copy = new FacemarkLBFParams(parameters);
            FacemarkLBFParams clone = parameters.Clone();
            Assert.NotSame(parameters, copy);
            Assert.NotSame(parameters, clone);
            Assert.NotSame(copy, clone);
            int[] originalFeatureCounts = parameters.FeatureCounts;
            double[] originalRadiusValues = parameters.RadiusValues;
            int[] originalLeftPupilIndices = parameters.LeftPupilIndices;
            int[] originalRightPupilIndices = parameters.RightPupilIndices;
            originalFeatureCounts[0] = 99;
            originalRadiusValues[0] = 9.9;
            originalLeftPupilIndices[0] = 99;
            originalRightPupilIndices[0] = 99;
            parameters.FeatureCounts = new[] { 1 };
            parameters.RadiusValues = new[] { 0.5 };
            parameters.LeftPupilIndices = new[] { 5 };
            parameters.RightPupilIndices = new[] { 6 };
            parameters.NLandmarks = 68;
            clone.FeatureCounts = new[] { 10 };

            Assert.Equal(1.25, copy.ShapeOffset, 3);
            Assert.Equal("face.xml", copy.CascadeFace);
            Assert.True(copy.Verbose);
            Assert.Equal(29, copy.NLandmarks);
            Assert.Equal(2, copy.InitialShapeCount);
            Assert.Equal(3, copy.StageCount);
            Assert.Equal(4, copy.TreeCount);
            Assert.Equal(5, copy.TreeDepth);
            Assert.Equal(0.35, copy.BaggingOverlap, 3);
            Assert.Equal("model.yaml", copy.ModelFilename);
            Assert.False(copy.SaveModel);
            Assert.Equal((uint)123, copy.Seed);
            Assert.Equal(new[] { 9, 8, 7 }, copy.FeatureCounts);
            Assert.Equal(new[] { 0.3, 0.2, 0.1 }, copy.RadiusValues);
            Assert.Equal(new[] { 1, 2 }, copy.LeftPupilIndices);
            Assert.Equal(new[] { 3, 4 }, copy.RightPupilIndices);
            Assert.Equal(new Rect(1, 2, 3, 4), copy.DetectRegion);
            Assert.Equal(new[] { 10 }, clone.FeatureCounts);
            Assert.Equal(new[] { 0.3, 0.2, 0.1 }, clone.RadiusValues);
            Assert.Equal(new[] { 1, 2 }, clone.LeftPupilIndices);
            Assert.Equal(new[] { 3, 4 }, clone.RightPupilIndices);
            Assert.Equal(29, clone.NLandmarks);
            Assert.Throws<ArgumentNullException>(() => new FacemarkLBFParams(null!));
            Assert.Throws<ArgumentNullException>(() => parameters.FeatureCounts = null!);
            Assert.Throws<ArgumentNullException>(() => parameters.RadiusValues = null!);
            Assert.Throws<ArgumentNullException>(() => parameters.LeftPupilIndices = null!);
            Assert.Throws<ArgumentNullException>(() => parameters.RightPupilIndices = null!);
        }

        [Fact]
        public void FacemarkLbfParametersToStringFormatsFloatingValuesInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                var parameters = new FacemarkLBFParams
                {
                    ShapeOffset = 1.25,
                    CascadeFace = "face.xml",
                    Verbose = true,
                    NLandmarks = 29,
                    InitialShapeCount = 2,
                    StageCount = 3,
                    TreeCount = 4,
                    TreeDepth = 5,
                    BaggingOverlap = 0.35,
                    ModelFilename = "model.yaml",
                    SaveModel = false,
                    Seed = 123,
                    FeatureCounts = new[] { 9, 8, 7 },
                    RadiusValues = new[] { 0.3, 0.2, 0.1 },
                    LeftPupilIndices = new[] { 1, 2 },
                    RightPupilIndices = new[] { 3, 4 },
                    DetectRegion = new Rect(1, 2, 3, 4)
                };

                string formatted = parameters.ToString();

                Assert.Contains("ShapeOffset=1.25", formatted, StringComparison.Ordinal);
                Assert.Contains("BaggingOverlap=0.35", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("ShapeOffset=1,25", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("BaggingOverlap=0,35", formatted, StringComparison.Ordinal);
                Assert.Contains("FeatureCounts=3", formatted, StringComparison.Ordinal);
                Assert.Contains("RadiusValues=3", formatted, StringComparison.Ordinal);
                Assert.Contains("LeftPupilIndices=2", formatted, StringComparison.Ordinal);
                Assert.Contains("RightPupilIndices=2", formatted, StringComparison.Ordinal);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void FacemarkLbfCreateValidatesManagedParametersBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() => FacemarkLBF.Create(null!));
            Assert.Throws<ArgumentNullException>(() => FacemarkLBF.Create(new FacemarkLBFParams { CascadeFace = null! }));
            Assert.Throws<ArgumentNullException>(() => FacemarkLBF.Create(new FacemarkLBFParams { ModelFilename = null! }));
            Assert.Throws<ArgumentOutOfRangeException>(() => FacemarkLBF.Create(new FacemarkLBFParams { NLandmarks = 0 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => FacemarkLBF.Create(new FacemarkLBFParams { InitialShapeCount = 0 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => FacemarkLBF.Create(new FacemarkLBFParams { StageCount = 0 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => FacemarkLBF.Create(new FacemarkLBFParams { TreeCount = 0 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => FacemarkLBF.Create(new FacemarkLBFParams { TreeDepth = 0 }));
            Assert.Throws<ArgumentException>(() => FacemarkLBF.Create(new FacemarkLBFParams
            {
                FeatureCounts = new[] { 1, 2 },
                RadiusValues = new[] { 0.1 }
            }));
        }

        [Fact]
        public void BifCreateValidatesManagedParametersBeforeNativeCall()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BIF.Create(numBands: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => BIF.Create(numBands: 9));
            Assert.Throws<ArgumentOutOfRangeException>(() => BIF.Create(numBands: 8, numRotations: 0));
        }

        [Fact]
        public void PredictionValueTypesHaveSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(16, Marshal.SizeOf<FacePrediction>());
            Assert.Equal(16, Marshal.SizeOf<FacePredictionResult>());

            Assert.Equal(0, FieldOffset<FacePrediction>("<Label>k__BackingField"));
            Assert.Equal(8, FieldOffset<FacePrediction>("<Confidence>k__BackingField"));
            Assert.Equal(0, FieldOffset<FacePredictionResult>("<Label>k__BackingField"));
            Assert.Equal(8, FieldOffset<FacePredictionResult>("<Distance>k__BackingField"));
        }

        [Fact]
        public void FactoryReturnsObjectOrExplicitNativeBoundary()
        {
            if (TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (LBPHFaceRecognizer.Create())
                {
                }

                using (FacemarkLBF.Create(CreateTinyLbfParams()))
                {
                }

                using (MACE.Create(imageSize: 32))
                {
                }
            }
            catch (OpenCvException ex) when (IsFaceModuleMissing(ex))
            {
                Assert.True(IsFaceModuleMissing(ex), ex.Message);
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
        }

        [Fact]
        public void ManagedArrayValidationRunsBeforeNativeCall()
        {
            using (LBPHFaceRecognizer? recognizer = TryCreateLbph())
            {
                if (recognizer != null)
                {
                    using (Mat image = CreateFaceImage(20))
                    using (StandardCollector collector = StandardCollector.Create())
                    {
                        Assert.Throws<ArgumentNullException>(() => recognizer.Train(null!, new[] { 1 }));
                        Assert.Throws<ArgumentNullException>(() => recognizer.Train(new[] { image }, null!));
                        Assert.Throws<ArgumentException>(() => recognizer.Train(new[] { image }, Array.Empty<int>()));
                        Assert.Throws<ArgumentNullException>(() => recognizer.Train(new Mat[] { null! }, new[] { 1 }));
                        Assert.Throws<ArgumentNullException>(() => recognizer.Update(null!, new[] { 1 }));
                        Assert.Throws<ArgumentNullException>(() => recognizer.Update(new[] { image }, null!));
                        Assert.Throws<ArgumentException>(() => recognizer.Update(new[] { image }, Array.Empty<int>()));
                        Assert.Throws<ArgumentNullException>(() => recognizer.Update(new Mat[] { null! }, new[] { 1 }));
                        Assert.Throws<ArgumentNullException>(() => recognizer.Predict(null!));
                        Assert.Throws<ArgumentNullException>(() => recognizer.Read(null!));
                        Assert.Throws<ArgumentNullException>(() => recognizer.Write(null!));
                        Assert.Throws<ArgumentNullException>(() => recognizer.SetLabelInfo(1, null!));
                        Assert.Throws<ArgumentNullException>(() => recognizer.GetLabelsByString(null!));
                        Assert.Throws<ArgumentNullException>(() => recognizer.Predict(null!, collector));
                        Assert.Throws<ArgumentNullException>(() => recognizer.Predict(image, null!));
                    }
                }
            }

            using (FacemarkLBF? facemark = TryCreateFacemark())
            using (MACE? mace = TryCreateMace())
            {
                if (facemark == null && mace == null)
                {
                    return;
                }

                using (Mat image = CreateFaceImage(80))
                {
                    if (facemark != null)
                    {
                        Assert.Throws<ArgumentNullException>(() => facemark.LoadModel(null!));
                        Assert.Throws<ArgumentNullException>(() => facemark.Save(null!));
                        Assert.Throws<ArgumentNullException>(() => facemark.Fit(null!, new[] { new Rect(1, 1, 10, 10) }));
                        Assert.Throws<ArgumentNullException>(() => facemark.Fit(image, null!));
                        Assert.Throws<ArgumentNullException>(() => facemark.AddTrainingSample(null!, CreateLandmarks()));
                        Assert.Throws<ArgumentNullException>(() => facemark.AddTrainingSample(image, null!));
                        Assert.Throws<ArgumentException>(() => facemark.AddTrainingSample(image, Array.Empty<Point2f>()));
                    }

                    if (mace != null)
                    {
                        Assert.Throws<ArgumentNullException>(() => mace.Salt(null!));
                        Assert.Throws<ArgumentNullException>(() => mace.Train(null!));
                        Assert.Throws<ArgumentException>(() => mace.Train(Array.Empty<Mat>()));
                        Assert.Throws<ArgumentNullException>(() => mace.Train(new Mat[] { null! }));
                        Assert.Throws<ArgumentNullException>(() => mace.Same(null!));
                        Assert.Throws<ArgumentNullException>(() => mace.Save(null!));
                        Assert.Throws<ArgumentNullException>(() => MACE.Load(null!));
                    }
                }
            }
        }

        [Fact]
        public void DisposedStateRejectsCalls()
        {
            LBPHFaceRecognizer? recognizer = TryCreateLbph();
            if (recognizer == null)
            {
                return;
            }

            recognizer.Dispose();
            Assert.True(recognizer.IsDisposed);
            using (Mat image = CreateFaceImage(20))
            {
                Assert.Throws<ObjectDisposedException>(() => recognizer.Predict(image));
            }

            using (FacemarkLBF? facemark = TryCreateFacemark())
            using (MACE? mace = TryCreateMace())
            {
                if (facemark != null || mace != null)
                {
                    using (Mat image = CreateFaceImage(20))
                    {
                        if (facemark != null)
                        {
                            facemark.Dispose();
                            Assert.True(facemark.IsDisposed);
                            Assert.Throws<ObjectDisposedException>(() => facemark.Fit(image, new[] { new Rect(1, 1, 10, 10) }));
                        }

                        if (mace != null)
                        {
                            mace.Dispose();
                            Assert.True(mace.IsDisposed);
                            Assert.Throws<ObjectDisposedException>(() => mace.Same(image));
                        }
                    }
                }
            }
        }

        [Fact]
        public void LbphAndCollectorSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat first = CreateFaceImage(30))
            using (Mat second = CreateFaceImage(180))
            using (Mat query = CreateFaceImage(32))
            using (LBPHFaceRecognizer recognizer = LBPHFaceRecognizer.Create(radius: 1, neighbors: 8, gridX: 4, gridY: 4))
            using (StandardCollector collector = StandardCollector.Create())
            {
                recognizer.Train(new[] { first, second }, new[] { 10, 20 });
                recognizer.SetLabelInfo(10, "first");

                FacePrediction prediction = recognizer.PredictWithConfidence(query);
                recognizer.Predict(query, collector);
                FacePredictionResult[] results = collector.GetResults(sorted: true);

                Assert.False(recognizer.Empty);
                Assert.Equal(1, recognizer.Radius);
                Assert.Equal(8, recognizer.Neighbors);
                Assert.Equal(4, recognizer.GridX);
                Assert.Equal(4, recognizer.GridY);
                Assert.Contains(10, recognizer.GetLabelsByString("first"));
                Assert.Equal("first", recognizer.GetLabelInfo(10));
                Assert.True(prediction.Label == 10 || prediction.Label == 20);
                Assert.True(results.Length >= 1);

                Mat[] histograms = recognizer.GetHistograms();
                try
                {
                    Assert.True(histograms.Length >= 1);
                }
                finally
                {
                    DisposeAll(histograms);
                }

                using (Mat labels = recognizer.GetLabels())
                {
                    Assert.False(labels.Empty);
                }
            }
        }

        [Fact]
        public void BasicRecognizersSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat first = CreateFaceImage(30))
            using (Mat second = CreateFaceImage(180))
            using (Mat query = CreateFaceImage(32))
            using (EigenFaceRecognizer eigen = EigenFaceRecognizer.Create(numComponents: 2))
            using (FisherFaceRecognizer fisher = FisherFaceRecognizer.Create(numComponents: 1))
            {
                eigen.Train(new[] { first, second }, new[] { 10, 20 });
                fisher.Train(new[] { first, second }, new[] { 10, 20 });

                Assert.False(eigen.Empty);
                Assert.False(fisher.Empty);
                Assert.Equal(2, eigen.NumComponents);
                Assert.Equal(1, fisher.NumComponents);
                Assert.True(eigen.Predict(query) == 10 || eigen.Predict(query) == 20);
                Assert.True(fisher.Predict(query) == 10 || fisher.Predict(query) == 20);

                using (Mat eigenValues = eigen.GetEigenValues())
                using (Mat eigenVectors = eigen.GetEigenVectors())
                using (Mat mean = eigen.GetMean())
                using (Mat labels = fisher.GetLabels())
                {
                    Assert.False(eigenValues.Empty);
                    Assert.False(eigenVectors.Empty);
                    Assert.False(mean.Empty);
                    Assert.False(labels.Empty);
                }

                Mat[] projections = eigen.GetProjections();
                try
                {
                    Assert.True(projections.Length >= 1);
                    Assert.False(projections[0].Empty);
                }
                finally
                {
                    DisposeAll(projections);
                }
            }
        }

        [Fact]
        public void BifSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image = CreateBifImage())
            using (BIF bif = BIF.Create(numBands: 2, numRotations: 3))
            using (Mat byteImage = CreateFaceImage(80))
            using (Mat featuresOutput = new Mat())
            using (Mat features = bif.Compute(image))
            {
                Assert.Equal(2, bif.NumBands);
                Assert.Equal(3, bif.NumRotations);
                Assert.False(features.Empty);
                Assert.Throws<ArgumentException>(() => bif.Compute(byteImage, featuresOutput));
                Assert.Throws<ArgumentException>(() => bif.Compute(byteImage));
            }
        }

        [Fact]
        public void FacemarkAndMaceSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat first = CreateFaceImage(70))
            using (Mat second = CreateFaceImage(90))
            using (Mat query = CreateFaceImage(72))
            using (FacemarkLBF facemark = FacemarkLBF.Create(CreateTinyLbfParams()))
            using (MACE mace = MACE.Create(imageSize: 32))
            {
                Assert.Equal(68, facemark.NLandmarks);
                if (!string.IsNullOrEmpty(facemark.Parameters.CascadeFace))
                {
                    facemark.AddTrainingSample(first, CreateLandmarks());
                }

                mace.Salt("round64");
                mace.Train(first, second);
                Assert.False(mace.Empty);
                Assert.True(mace.Same(query) || !mace.Same(query));

                string path = Path.Combine(Path.GetTempPath(), "opencv-csharp-mace-round64.xml");
                try
                {
                    mace.Save(path);
                    using (MACE loaded = MACE.Load(path))
                    {
                        Assert.False(loaded.Empty);
                    }
                }
                finally
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
        }

        private static LBPHFaceRecognizer? TryCreateLbph()
        {
            try
            {
                return LBPHFaceRecognizer.Create();
            }
            catch (OpenCvException ex) when (IsFaceModuleMissing(ex))
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

        private static FacemarkLBF? TryCreateFacemark()
        {
            try
            {
                return FacemarkLBF.Create(CreateTinyLbfParams());
            }
            catch (OpenCvException ex) when (IsFaceModuleMissing(ex))
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

        private static MACE? TryCreateMace()
        {
            try
            {
                return MACE.Create(imageSize: 32);
            }
            catch (OpenCvException ex) when (IsFaceModuleMissing(ex))
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

        private static FacemarkLBFParams CreateTinyLbfParams()
        {
            return new FacemarkLBFParams
            {
                NLandmarks = 68,
                InitialShapeCount = 1,
                StageCount = 1,
                TreeCount = 1,
                TreeDepth = 2,
                FeatureCounts = new[] { 8 },
                RadiusValues = new[] { 0.2 },
                CascadeFace = FindFaceCascadePath() ?? string.Empty,
                SaveModel = false,
                Verbose = false,
                Seed = 123
            };
        }

        private static string? FindFaceCascadePath()
        {
            string? configured = TestEnvironment.GetFaceCascadeVariable();
            if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
            {
                return configured;
            }

            string? fromCurrent = FindFaceCascadePathFromRoot(Directory.GetCurrentDirectory());
            if (fromCurrent != null)
            {
                return fromCurrent;
            }

            return FindFaceCascadePathFromRoot(AppContext.BaseDirectory);
        }

        private static string? FindFaceCascadePathFromRoot(string root)
        {
            var directory = new DirectoryInfo(root);
            for (int i = 0; i < 8 && directory != null; i++)
            {
                string candidate = Path.Combine(
                    directory.FullName,
                    "artifacts",
                    "opencv-install",
                    FactualOpenCvInstallCacheName,
                    "etc",
                    "haarcascades",
                    "haarcascade_frontalface_default.xml");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            return null;
        }

        private static Point2f[] CreateLandmarks()
        {
            var points = new Point2f[68];
            for (int i = 0; i < points.Length; i++)
            {
                float angle = (float)(i * Math.PI * 2.0 / points.Length);
                points[i] = new Point2f(16.0F + 8.0F * (float)Math.Cos(angle), 16.0F + 9.0F * (float)Math.Sin(angle));
            }

            return points;
        }

        private static Mat CreateFaceImage(byte value)
        {
            var image = new Mat(32, 32, MatType.CV_8UC1, new Scalar(value));
            ImgProcCv2.Circle(image, new Point(12, 12), 4, new Scalar(220 - value / 2), -1);
            ImgProcCv2.Circle(image, new Point(22, 12), 4, new Scalar(220 - value / 2), -1);
            ImgProcCv2.Rectangle(image, new Rect(11, 22, 12, 3), new Scalar(20 + value / 2), -1);
            return image;
        }

        private static Mat CreateBifImage()
        {
            using (Mat gray = CreateFaceImage(80))
            {
                return gray.ConvertTo(MatType.CV_32FC1, 1.0 / 255.0);
            }
        }

        private static void DisposeAll(Mat[] mats)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].Dispose();
            }
        }

        private static bool IsFaceModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("face", StringComparison.OrdinalIgnoreCase) >= 0
                && (exception.Message.IndexOf("OpenCV", StringComparison.OrdinalIgnoreCase) >= 0
                    || exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }

    }
}
