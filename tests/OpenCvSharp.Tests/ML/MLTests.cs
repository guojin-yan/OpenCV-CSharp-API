using System;
using System.Globalization;
using System.IO;
using OpenCvSharp.Core;
using OpenCvSharp.ML;

namespace OpenCvSharp.Tests.ML
{
    public sealed class MLTests
    {
        [Fact]
        public void EnumValuesMatchOpenCvMlConstants()
        {
            Assert.Equal(0, (int)SampleTypes.RowSample);
            Assert.Equal(1, (int)SampleTypes.ColSample);
            Assert.Equal(0, (int)MlVariableType.Numerical);
            Assert.Equal(0, (int)MlVariableType.Ordered);
            Assert.Equal(1, (int)MlVariableType.Categorical);
            Assert.Equal(0, (int)MlErrorType.TestError);
            Assert.Equal(1, (int)MlErrorType.TrainError);
            Assert.Equal(1, (int)KNearestTypes.BruteForce);
            Assert.Equal(2, (int)KNearestTypes.KDTree);
            Assert.Equal(100, (int)SVMTypes.CSvc);
            Assert.Equal(0, (int)SVMKernelTypes.Linear);
            Assert.Equal(2, (int)SVMKernelTypes.Rbf);
            Assert.Equal(0, (int)SVMParamTypes.C);
            Assert.Equal(5, (int)SVMParamTypes.Degree);
            Assert.Equal(1, (int)StatModelFlags.RawOutput);
            Assert.Equal(4, (int)StatModelFlags.PreprocessedInput);
            Assert.Equal(0, (int)ANN_MLPTrainingMethods.Backprop);
            Assert.Equal(1, (int)ANN_MLPTrainingMethods.Rprop);
            Assert.Equal(2, (int)ANN_MLPTrainingMethods.Anneal);
            Assert.Equal(0, (int)ANN_MLPActivationFunctions.Identity);
            Assert.Equal(1, (int)ANN_MLPActivationFunctions.SigmoidSym);
            Assert.Equal(2, (int)ANN_MLPActivationFunctions.Gaussian);
            Assert.Equal(3, (int)ANN_MLPActivationFunctions.Relu);
            Assert.Equal(4, (int)ANN_MLPActivationFunctions.LeakyRelu);
            Assert.Equal(1, (int)ANN_MLPTrainFlags.UpdateWeights);
            Assert.Equal(2, (int)ANN_MLPTrainFlags.NoInputScale);
            Assert.Equal(4, (int)ANN_MLPTrainFlags.NoOutputScale);
        }

        [Fact]
        public void PublicMethodsValidateManagedArguments()
        {
            using (var mat = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => TrainData.Create(null!, SampleTypes.RowSample, mat));
                Assert.Throws<ArgumentNullException>(() => TrainData.Create(mat, SampleTypes.RowSample, null!));
                Assert.Throws<ArgumentNullException>(() => TrainData.LoadFromCsv(null!, 0));
                Assert.Throws<ArgumentNullException>(() => TrainData.GetSubVector(null!, mat, mat));
                Assert.Throws<ArgumentNullException>(() => TrainData.GetSubVector(mat, null!, mat));
                Assert.Throws<ArgumentNullException>(() => TrainData.GetSubVector(mat, mat, null!));
                Assert.Throws<ArgumentNullException>(() => TrainData.GetSubVector(null!, mat));
                Assert.Throws<ArgumentNullException>(() => TrainData.GetSubVector(mat, null!));
                Assert.Throws<ArgumentNullException>(() => TrainData.GetSubMatrix(null!, mat, SampleTypes.RowSample, mat));
                Assert.Throws<ArgumentNullException>(() => TrainData.GetSubMatrix(mat, null!, SampleTypes.RowSample, mat));
                Assert.Throws<ArgumentNullException>(() => TrainData.GetSubMatrix(mat, mat, SampleTypes.RowSample, null!));
                Assert.Throws<ArgumentNullException>(() => TrainData.GetSubMatrix(null!, mat, SampleTypes.RowSample));
                Assert.Throws<ArgumentNullException>(() => TrainData.GetSubMatrix(mat, null!, SampleTypes.RowSample));
                Assert.Throws<ArgumentNullException>(() => KNearest.Load(null!));
                Assert.Throws<ArgumentNullException>(() => SVM.Load(null!));
                Assert.Throws<ArgumentNullException>(() => NormalBayesClassifier.Load(null!));
                Assert.Throws<ArgumentNullException>(() => ANN_MLP.Load(null!));

                Assert.Throws<ArgumentException>(() => TrainData.LoadFromCsv("data\0file.csv", 0));
                Assert.Throws<ArgumentException>(() => KNearest.Load("model\0file.yml"));
                Assert.Throws<ArgumentException>(() => SVM.Load("model\0file.yml"));
                Assert.Throws<ArgumentException>(() => NormalBayesClassifier.Load("model\0file.yml"));
                Assert.Throws<ArgumentException>(() => NormalBayesClassifier.Load("model.yml", "node\0name"));
                Assert.Throws<ArgumentException>(() => ANN_MLP.Load("model\0file.yml"));
            }
        }

        [Fact]
        public void ParamGridShapeSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var grid = new ParamGrid(0.1, 10.0, 1.5))
            {
                Assert.False(grid.IsDisposed);
                Assert.Equal(0.1, grid.MinVal, 3);
                Assert.Equal(10.0, grid.MaxVal, 3);
                Assert.Equal(1.5, grid.LogStep, 3);

                grid.SetValues(1.0, 8.0, 2.0);
                grid.GetValues(out double minVal, out double maxVal, out double logStep);

                Assert.Equal(1.0, minVal, 3);
                Assert.Equal(8.0, maxVal, 3);
                Assert.Equal(2.0, logStep, 3);
            }
        }

        [Fact]
        public void ParamGridToStringUsesInvariantCultureWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

                using (var grid = new ParamGrid(0.125, 12.5, 1.25))
                {
                    Assert.Equal("{MinVal=0.125,MaxVal=12.5,LogStep=1.25}", grid.ToString());
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void ParamGridDisposedStateThrowsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            var grid = new ParamGrid(0.1, 10.0, 1.5);
            grid.Dispose();

            Assert.True(grid.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => grid.GetValues(out _, out _, out _));
            Assert.Throws<ObjectDisposedException>(() => grid.SetValues(1.0, 8.0, 2.0));
            Assert.Throws<ObjectDisposedException>(() => grid.MinVal);
            Assert.Throws<ObjectDisposedException>(() => grid.MinVal = 1.0);
            Assert.Throws<ObjectDisposedException>(() => grid.MaxVal);
            Assert.Throws<ObjectDisposedException>(() => grid.MaxVal = 8.0);
            Assert.Throws<ObjectDisposedException>(() => grid.LogStep);
            Assert.Throws<ObjectDisposedException>(() => grid.LogStep = 2.0);
            Assert.Equal("{Disposed=True}", grid.ToString());
        }

        [Fact]
        public void ModelDisposedStateThrowsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var samples = CreateSamples())
            using (var responses = CreateResponses())
            {
                KNearest knn = KNearest.Create();
                knn.Dispose();
                Assert.True(knn.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => knn.Train(samples, SampleTypes.RowSample, responses));

                SVM svm = SVM.Create();
                svm.Dispose();
                Assert.True(svm.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => svm.SetKernel(SVMKernelTypes.Linear));
                Assert.Throws<ObjectDisposedException>(() => svm.GetClassWeights());
                Assert.Throws<ObjectDisposedException>(() => svm.GetSupportVectors());
                Assert.Throws<ObjectDisposedException>(() => svm.GetUncompressedSupportVectors());

                NormalBayesClassifier bayes = NormalBayesClassifier.Create();
                bayes.Dispose();
                Assert.True(bayes.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => bayes.Predict(samples));

                ANN_MLP ann = ANN_MLP.Create();
                ann.Dispose();
                Assert.True(ann.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => ann.SetActivationFunction(ANN_MLPActivationFunctions.Identity));
                Assert.Throws<ObjectDisposedException>(() => ann.GetLayerSizes());
                Assert.Throws<ObjectDisposedException>(() => ann.GetWeights(0));
                Assert.Throws<ObjectDisposedException>(() => ann.SetAnnealEnergySeed(1));
            }
        }

        [Fact]
        public void AnnMlpDefaultsAndPropertiesRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var ann = ANN_MLP.Create())
            using (Mat emptyLayers = ann.GetLayerSizes())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => ann.SetTrainMethod((ANN_MLPTrainingMethods)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => ann.SetActivationFunction((ANN_MLPActivationFunctions)99));
                Assert.Throws<ArgumentNullException>(() => ann.SetLayerSizes(null!));
                Assert.Throws<ArgumentNullException>(() => ann.GetLayerSizes(null!));
                Assert.Throws<ArgumentNullException>(() => ann.GetWeights(0, null!));
                Assert.Equal(ANN_MLPTrainingMethods.Rprop, ann.TrainingMethod);
                Assert.True(emptyLayers.Empty);
                Assert.Equal(TermCriteriaTypes.CountOrEps, ann.TermCriteria.Type);
                Assert.Equal(1000, ann.TermCriteria.MaxCount);
                Assert.Equal(0.01, ann.TermCriteria.Epsilon, 8);
                Assert.Equal(0.1, ann.BackpropWeightScale, 8);
                Assert.Equal(0.1, ann.BackpropMomentumScale, 8);
                Assert.Equal(0.1, ann.RpropDW0, 8);
                Assert.Equal(1.2, ann.RpropDWPlus, 8);
                Assert.Equal(0.5, ann.RpropDWMinus, 8);
                Assert.True(ann.RpropDWMin > 0.0);
                Assert.Equal(50.0, ann.RpropDWMax, 8);
                Assert.Equal(10.0, ann.AnnealInitialT, 8);
                Assert.Equal(0.1, ann.AnnealFinalT, 8);
                Assert.Equal(0.95, ann.AnnealCoolingRatio, 8);
                Assert.Equal(10, ann.AnnealIterationsPerStep);

                ann.BackpropWeightScale = 0.2;
                ann.BackpropMomentumScale = 0.3;
                ann.RpropDW0 = 0.15;
                ann.RpropDWPlus = 1.3;
                ann.RpropDWMinus = 0.4;
                ann.RpropDWMin = 1e-5;
                ann.RpropDWMax = 40.0;
                ann.AnnealInitialT = 12.0;
                ann.AnnealFinalT = 0.2;
                ann.AnnealCoolingRatio = 0.9;
                ann.AnnealIterationsPerStep = 12;
                ann.TermCriteria = TermCriteria.ByCountAndEpsilon(250, 1e-5);
                ann.SetAnnealEnergySeed(0xffffffffUL);

                Assert.Equal(0.2, ann.BackpropWeightScale, 8);
                Assert.Equal(0.3, ann.BackpropMomentumScale, 8);
                Assert.Equal(0.15, ann.RpropDW0, 8);
                Assert.Equal(1.3, ann.RpropDWPlus, 8);
                Assert.Equal(0.4, ann.RpropDWMinus, 8);
                Assert.Equal(1e-5, ann.RpropDWMin, 12);
                Assert.Equal(40.0, ann.RpropDWMax, 8);
                Assert.Equal(12.0, ann.AnnealInitialT, 8);
                Assert.Equal(0.2, ann.AnnealFinalT, 8);
                Assert.Equal(0.9, ann.AnnealCoolingRatio, 8);
                Assert.Equal(12, ann.AnnealIterationsPerStep);
                Assert.Equal(TermCriteria.ByCountAndEpsilon(250, 1e-5), ann.TermCriteria);

                ann.SetTrainMethod(ANN_MLPTrainingMethods.Backprop, 0.25, 0.35);
                Assert.Equal(ANN_MLPTrainingMethods.Backprop, ann.TrainingMethod);
                Assert.Equal(0.25, ann.BackpropWeightScale, 8);
                Assert.Equal(0.35, ann.BackpropMomentumScale, 8);

                ann.SetTrainMethod(ANN_MLPTrainingMethods.Rprop, 0.2, 1e-6);
                Assert.Equal(ANN_MLPTrainingMethods.Rprop, ann.TrainingMethod);
                Assert.Equal(0.2, ann.RpropDW0, 8);
                Assert.Equal(1e-6, ann.RpropDWMin, 12);
            }
        }

        [Fact]
        public void AnnMlpTrainingWeightsAndPersistenceRunWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string modelDir = TestEnvironment.GetMlModelDirVariable() ?? Path.GetTempPath();
            string modelPath = Path.Combine(modelDir, "opencv-csharp-ml-ann-mlp-smoke.yml");
            using (var samples = CreateAnnSamples())
            using (var responses = CreateAnnResponses())
            using (var query = new Mat(1, 2, MatType.CV_32FC1))
            using (var layers = new Mat(1, 3, MatType.CV_32SC1))
            using (var trainData = TrainData.Create(samples, SampleTypes.RowSample, responses))
            using (var ann = ANN_MLP.Create())
            using (var predictions = new Mat())
            {
                query.CopyFrom<float>(new float[] { 0.25F, -0.5F });
                layers.CopyFrom<int>(new int[] { 2, 4, 1 });
                ann.SetLayerSizes(layers);
                ann.SetActivationFunction(ANN_MLPActivationFunctions.Identity);
                ann.SetTrainMethod(ANN_MLPTrainingMethods.Rprop, 0.1, 1e-6);
                ann.TermCriteria = TermCriteria.ByCountAndEpsilon(300, 1e-6);

                Assert.True(ann.Train(trainData, ANN_MLPTrainFlags.NoInputScale | ANN_MLPTrainFlags.NoOutputScale));
                Assert.True(ann.IsTrained);
                Assert.False(ann.IsClassifier);
                Assert.Equal(2, ann.VarCount);
                float prediction = ann.Predict(query, predictions);
                Assert.False(float.IsNaN(prediction));
                Assert.False(float.IsInfinity(prediction));
                Assert.Equal(1, predictions.Rows);
                Assert.Equal(1, predictions.Cols);

                using (Mat returnedLayers = ann.GetLayerSizes())
                using (Mat firstWeights = ann.GetWeights(1))
                {
                    Assert.Equal(new int[] { 2, 4, 1 }, returnedLayers.ToArray<int>());
                    Assert.False(firstWeights.Empty);
                    double[] nativeWeights = firstWeights.ToArray<double>();
                    firstWeights.SetTo(new Scalar(0.0));
                    using (Mat secondWeights = ann.GetWeights(1))
                    {
                        Assert.Equal(nativeWeights, secondWeights.ToArray<double>());
                    }
                }

                ann.Save(modelPath);
            }

            try
            {
                using (var query = new Mat(1, 2, MatType.CV_32FC1))
                using (var loadedResults = new Mat())
                using (var loaded = ANN_MLP.Load(modelPath))
                {
                    query.CopyFrom<float>(new float[] { 0.25F, -0.5F });
                    Assert.True(loaded.IsTrained);
                    float loadedPrediction = loaded.Predict(query, loadedResults);
                    Assert.False(float.IsNaN(loadedPrediction));
                    Assert.False(float.IsInfinity(loadedPrediction));
                    Assert.Equal(1, loadedResults.Rows);
                    Assert.Equal(1, loadedResults.Cols);
                }
            }
            finally
            {
                try
                {
                    File.Delete(modelPath);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        [Fact]
        public void TrainDataSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var samples = CreateSamples())
            using (var responses = CreateResponses())
            using (var trainData = TrainData.Create(samples, SampleTypes.RowSample, responses))
            {
                trainData.SetTrainTestSplit(4, shuffle: false);
                trainData.ShuffleTrainTest();

                using (Mat sampleMat = trainData.GetSamples())
                using (Mat trainSamples = trainData.GetTrainSamples())
                using (Mat testSamples = trainData.GetTestSamples())
                using (Mat responseMat = trainData.GetResponses())
                using (Mat trainResponses = trainData.GetTrainResponses())
                using (Mat testResponses = trainData.GetTestResponses())
                using (Mat varIdx = trainData.GetVarIdx())
                using (Mat varType = trainData.GetVarType())
                using (Mat varSymbolFlags = trainData.GetVarSymbolFlags())
                using (Mat trainSampleIdx = trainData.GetTrainSampleIdx())
                using (Mat testSampleIdx = trainData.GetTestSampleIdx())
                {
                    Assert.Equal(SampleTypes.RowSample, trainData.Layout);
                    Assert.Equal(6, trainData.NSamples);
                    Assert.Equal(2, trainData.NVars);
                    Assert.Equal(4, trainData.NTrainSamples);
                    Assert.Equal(2, trainData.NTestSamples);
                    Assert.Equal(6, sampleMat.Rows);
                    Assert.Equal(2, sampleMat.Cols);
                    Assert.Equal(4, trainSamples.Rows);
                    Assert.Equal(2, testSamples.Rows);
                    Assert.Equal(2, testSamples.Cols);
                    Assert.Equal(6, responseMat.Rows);
                    Assert.Equal(4, trainResponses.Rows);
                    Assert.Equal(2, testResponses.Rows);
                    Assert.True(varIdx.Rows >= 0);
                    Assert.True(varIdx.Cols >= 0);
                    Assert.True(varType.Rows >= 0);
                    Assert.True(varType.Cols >= 0);
                    Assert.True(varSymbolFlags.Rows >= 0);
                    Assert.True(varSymbolFlags.Cols >= 0);
                    Assert.Equal(trainData.NTrainSamples, trainSampleIdx.Rows * trainSampleIdx.Cols);
                    Assert.Equal(trainData.NTestSamples, testSampleIdx.Rows * testSampleIdx.Cols);
                    Assert.NotNull(trainData.GetNames());
                }
            }
        }

        [Fact]
        public void KNearestSvmAndBayesSmokeRunWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var samples = CreateSamples())
            using (var responses = CreateResponses())
            using (var query = new Mat(1, 2, MatType.CV_32FC1))
            using (var knnResults = new Mat())
            using (var neighborResponses = new Mat())
            using (var dists = new Mat())
            using (var svmResults = new Mat())
            using (var svmDecisionAlpha = new Mat())
            using (var svmDecisionSvidx = new Mat())
            using (var bayesResults = new Mat())
            using (var probabilities = new Mat())
            using (var knn = KNearest.Create())
            using (var svm = SVM.Create())
            using (var bayes = NormalBayesClassifier.Create())
            {
                query.CopyFrom<float>(new float[] { 0.1F, 0.2F });

                knn.DefaultK = 1;
                knn.IsClassifierModel = true;
                knn.AlgorithmType = KNearestTypes.BruteForce;
                Assert.True(knn.Train(samples, SampleTypes.RowSample, responses));
                float knnPrediction = knn.FindNearest(query, 1, knnResults, neighborResponses, dists);

                svm.Type = SVMTypes.CSvc;
                svm.SetKernel(SVMKernelTypes.Linear);
                svm.C = 1.0;
                svm.TermCriteria = TermCriteria.ByCountAndEpsilon(100, 1e-6);
                Assert.True(svm.Train(samples, SampleTypes.RowSample, responses));
                float svmPrediction = svm.Predict(query, svmResults);
                double svmRho = svm.GetDecisionFunction(0, svmDecisionAlpha, svmDecisionSvidx);

                Assert.True(bayes.Train(samples, SampleTypes.RowSample, responses));
                float bayesPrediction = bayes.PredictProb(query, bayesResults, probabilities);

                using (Mat supportVectors = svm.GetSupportVectors())
                using (Mat uncompressedSupportVectors = svm.GetUncompressedSupportVectors())
                {
                    Assert.False(knnResults.Empty);
                    Assert.False(neighborResponses.Empty);
                    Assert.False(dists.Empty);
                    Assert.False(svmResults.Empty);
                    Assert.False(supportVectors.Empty);
                    Assert.False(double.IsNaN(svmRho));
                    Assert.False(double.IsInfinity(svmRho));
                    Assert.True(svmDecisionAlpha.Rows >= 0);
                    Assert.True(svmDecisionAlpha.Cols >= 0);
                    Assert.True(svmDecisionSvidx.Rows >= 0);
                    Assert.True(svmDecisionSvidx.Cols >= 0);
                    Assert.True(uncompressedSupportVectors.Rows >= 0);
                    Assert.True(uncompressedSupportVectors.Cols >= 0);
                    Assert.False(bayesResults.Empty);
                    Assert.False(probabilities.Empty);
                    Assert.True(knnPrediction == 0.0F || knnPrediction == 1.0F);
                    Assert.True(svmPrediction == 0.0F || svmPrediction == 1.0F);
                    Assert.True(bayesPrediction == 0.0F || bayesPrediction == 1.0F);
                }
            }
        }

        [Fact]
        public void ModelSaveAndLoadSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string modelDir = TestEnvironment.GetMlModelDirVariable() ?? Path.GetTempPath();
            string modelPath = Path.Combine(modelDir, "opencv-csharp-ml-knearest-smoke.yml");

            using (var samples = CreateSamples())
            using (var responses = CreateResponses())
            using (var model = KNearest.Create())
            {
                model.DefaultK = 1;
                Assert.True(model.Train(samples, SampleTypes.RowSample, responses));
                model.Save(modelPath);
            }

            using (var loaded = KNearest.Load(modelPath))
            {
                Assert.False(loaded.IsDisposed);
            }

            try
            {
                File.Delete(modelPath);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private static Mat CreateSamples()
        {
            var samples = new Mat(6, 2, MatType.CV_32FC1);
            samples.CopyFrom<float>(new float[]
            {
                0.0F, 0.0F,
                0.0F, 1.0F,
                1.0F, 0.0F,
                5.0F, 5.0F,
                5.0F, 6.0F,
                6.0F, 5.0F
            });
            return samples;
        }

        private static Mat CreateResponses()
        {
            var responses = new Mat(6, 1, MatType.CV_32SC1);
            responses.CopyFrom<int>(new int[] { 0, 0, 0, 1, 1, 1 });
            return responses;
        }

        private static Mat CreateAnnSamples()
        {
            var samples = new Mat(6, 2, MatType.CV_32FC1);
            samples.CopyFrom<float>(new float[]
            {
                -1.0F, -1.0F,
                -1.0F, 1.0F,
                1.0F, -1.0F,
                1.0F, 1.0F,
                0.5F, -0.5F,
                -0.5F, 0.5F
            });
            return samples;
        }

        private static Mat CreateAnnResponses()
        {
            var responses = new Mat(6, 1, MatType.CV_32FC1);
            responses.CopyFrom<float>(new float[] { -2.0F, 0.0F, 0.0F, 2.0F, 0.0F, 0.0F });
            return responses;
        }

    }
}
