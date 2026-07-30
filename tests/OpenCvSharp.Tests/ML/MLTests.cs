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
            Assert.Equal(0, (int)DTreesPredictionFlags.Auto);
            Assert.Equal(1 << 8, (int)DTreesPredictionFlags.Sum);
            Assert.Equal(2 << 8, (int)DTreesPredictionFlags.MaxVote);
            Assert.Equal(3 << 8, (int)DTreesPredictionFlags.Mask);
            Assert.Equal(0, (int)BoostTypes.Discrete);
            Assert.Equal(1, (int)BoostTypes.Real);
            Assert.Equal(2, (int)BoostTypes.Logit);
            Assert.Equal(3, (int)BoostTypes.Gentle);
            Assert.Equal(0, (int)EMCovarianceMatrixTypes.Spherical);
            Assert.Equal(1, (int)EMCovarianceMatrixTypes.Diagonal);
            Assert.Equal(2, (int)EMCovarianceMatrixTypes.Generic);
            Assert.Equal(EMCovarianceMatrixTypes.Diagonal, EMCovarianceMatrixTypes.Default);
            Assert.Equal(5, EM.DefaultClustersNumber);
            Assert.Equal(100, EM.DefaultMaxIterations);

            var prediction = new EMPredictionResult(-1.25, 1);
            Assert.Equal(-1.25, prediction.LogLikelihood);
            Assert.Equal(1, prediction.Label);
            Assert.Equal(prediction, new EMPredictionResult(-1.25, 1));
            Assert.NotEqual(prediction, new EMPredictionResult(-1.25, 0));
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
                Assert.Throws<ArgumentNullException>(() => DTrees.Load(null!));
                Assert.Throws<ArgumentNullException>(() => RTrees.Load(null!));
                Assert.Throws<ArgumentNullException>(() => Boost.Load(null!));
                Assert.Throws<ArgumentNullException>(() => EM.Load(null!));

                Assert.Throws<ArgumentException>(() => TrainData.LoadFromCsv("data\0file.csv", 0));
                Assert.Throws<ArgumentException>(() => KNearest.Load("model\0file.yml"));
                Assert.Throws<ArgumentException>(() => SVM.Load("model\0file.yml"));
                Assert.Throws<ArgumentException>(() => NormalBayesClassifier.Load("model\0file.yml"));
                Assert.Throws<ArgumentException>(() => NormalBayesClassifier.Load("model.yml", "node\0name"));
                Assert.Throws<ArgumentException>(() => ANN_MLP.Load("model\0file.yml"));
                Assert.Throws<ArgumentException>(() => DTrees.Load("model\0file.yml"));
                Assert.Throws<ArgumentException>(() => RTrees.Load("model\0file.yml"));
                Assert.Throws<ArgumentException>(() => Boost.Load("model\0file.yml"));
                Assert.Throws<ArgumentException>(() => EM.Load("model\0file.yml"));
                Assert.Throws<ArgumentException>(() => DTrees.Load("model.yml", "node\0name"));
                Assert.Throws<ArgumentException>(() => RTrees.Load("model.yml", "node\0name"));
                Assert.Throws<ArgumentException>(() => Boost.Load("model.yml", "node\0name"));
                Assert.Throws<ArgumentException>(() => EM.Load("model.yml", "node\0name"));
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

                DTrees dtrees = DTrees.Create();
                dtrees.Dispose();
                Assert.True(dtrees.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => dtrees.MaxDepth);
                Assert.Throws<ObjectDisposedException>(() => dtrees.GetPriors());

                RTrees rtrees = RTrees.Create();
                rtrees.Dispose();
                Assert.True(rtrees.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => rtrees.TermCriteria);
                Assert.Throws<ObjectDisposedException>(() => rtrees.GetVarImportance());
                Assert.Throws<ObjectDisposedException>(() => rtrees.GetVotes(samples));
                Assert.Throws<ObjectDisposedException>(() => rtrees.OobError);

                Boost boost = Boost.Create();
                boost.Dispose();
                Assert.True(boost.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => boost.BoostType);
                Assert.Throws<ObjectDisposedException>(() => boost.WeightTrimRate);

                EM em = EM.Create();
                em.Dispose();
                Assert.True(em.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => em.ClustersNumber);
                Assert.Throws<ObjectDisposedException>(() => em.GetWeights());
                Assert.Throws<ObjectDisposedException>(() => em.GetMeans());
                Assert.Throws<ObjectDisposedException>(() => em.GetCovariances());
                Assert.Throws<ObjectDisposedException>(() => em.Predict2(samples));
                Assert.Throws<ObjectDisposedException>(() => em.TrainEM(samples));
            }
        }

        [Fact]
        public void TreeModelDefaultsPropertiesAndPriorsRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var dtrees = DTrees.Create())
            using (var rtrees = RTrees.Create())
            using (var boost = Boost.Create())
            {
                Assert.Equal(10, dtrees.MaxCategories);
                Assert.Equal(int.MaxValue, dtrees.MaxDepth);
                Assert.Equal(10, dtrees.MinSampleCount);
                Assert.Equal(10, dtrees.CVFolds);
                Assert.False(dtrees.UseSurrogates);
                Assert.True(dtrees.Use1SERule);
                Assert.True(dtrees.TruncatePrunedTree);
                Assert.Equal(0.01F, dtrees.RegressionAccuracy, 5);

                Assert.Equal(5, rtrees.MaxDepth);
                Assert.Equal(0, rtrees.CVFolds);
                Assert.False(rtrees.Use1SERule);
                Assert.False(rtrees.TruncatePrunedTree);
                Assert.False(rtrees.CalculateVarImportance);
                Assert.Equal(0, rtrees.ActiveVarCount);
                Assert.Equal(TermCriteriaTypes.CountOrEps, rtrees.TermCriteria.Type);
                Assert.Equal(50, rtrees.TermCriteria.MaxCount);
                Assert.Equal(0.1, rtrees.TermCriteria.Epsilon, 8);
                Assert.Equal(0.0, rtrees.OobError, 8);

                Assert.Equal(1, boost.MaxDepth);
                Assert.Equal(0, boost.CVFolds);
                Assert.Equal(BoostTypes.Real, boost.BoostType);
                Assert.Equal(100, boost.WeakCount);
                Assert.Equal(0.95, boost.WeightTrimRate, 8);

                dtrees.MaxCategories = 12;
                dtrees.MaxDepth = 4;
                dtrees.MinSampleCount = 3;
                dtrees.CVFolds = 0;
                dtrees.UseSurrogates = true;
                dtrees.Use1SERule = false;
                dtrees.TruncatePrunedTree = false;
                dtrees.RegressionAccuracy = 0.02F;
                Assert.Equal(12, dtrees.MaxCategories);
                Assert.Equal(4, dtrees.MaxDepth);
                Assert.Equal(3, dtrees.MinSampleCount);
                Assert.Equal(0, dtrees.CVFolds);
                Assert.True(dtrees.UseSurrogates);
                Assert.False(dtrees.Use1SERule);
                Assert.False(dtrees.TruncatePrunedTree);
                Assert.Equal(0.02F, dtrees.RegressionAccuracy, 5);
                Assert.Throws<OpenCvException>(() => dtrees.MaxDepth = -1);
                Assert.Throws<OpenCvException>(() => dtrees.RegressionAccuracy = -0.1F);
                Assert.Throws<ArgumentOutOfRangeException>(() => dtrees.Predict(new Mat(), DTreesPredictionFlags.Mask));
                Assert.Throws<ArgumentNullException>(() => dtrees.SetPriors(null!));
                Assert.Throws<ArgumentNullException>(() => dtrees.GetPriors(null!));

                rtrees.CalculateVarImportance = true;
                rtrees.ActiveVarCount = 1;
                rtrees.TermCriteria = TermCriteria.ByCountAndEpsilon(8, 0.01);
                Assert.True(rtrees.CalculateVarImportance);
                Assert.Equal(1, rtrees.ActiveVarCount);
                Assert.Equal(TermCriteria.ByCountAndEpsilon(8, 0.01), rtrees.TermCriteria);
                Assert.Throws<ArgumentNullException>(() => rtrees.GetVarImportance(null!));
                Assert.Throws<ArgumentNullException>(() => rtrees.GetVotes(null!));
                Assert.Throws<ArgumentNullException>(() => rtrees.GetVotes(new Mat(), null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => rtrees.GetVotes(new Mat(), DTreesPredictionFlags.Mask));

                Assert.Throws<ArgumentOutOfRangeException>(() => boost.BoostType = (BoostTypes)99);
                boost.BoostType = BoostTypes.Discrete;
                boost.WeakCount = 12;
                boost.WeightTrimRate = 0.8;
                Assert.Equal(BoostTypes.Discrete, boost.BoostType);
                Assert.Equal(12, boost.WeakCount);
                Assert.Equal(0.8, boost.WeightTrimRate, 8);

                var priors = new Mat(1, 2, MatType.CV_32FC1);
                priors.CopyFrom<float>(new float[] { 1.0F, 2.0F });
                dtrees.SetPriors(priors);
                priors.CopyFrom<float>(new float[] { 2.0F, 3.0F });
                priors.Dispose();

                using (Mat first = dtrees.GetPriors())
                {
                    Assert.Equal(new float[] { 2.0F, 3.0F }, first.ToArray<float>());
                    first.SetTo(new Scalar(99.0));
                }

                using (Mat second = dtrees.GetPriors())
                using (Mat empty = new Mat())
                {
                    Assert.Equal(new float[] { 2.0F, 3.0F }, second.ToArray<float>());
                    dtrees.SetPriors(empty);
                }
            }
        }

        [Fact]
        public void TreeModelsTrainVotePersistAndOwnOutputsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string modelDir = TestEnvironment.GetMlModelDirVariable() ?? Path.GetTempPath();
            string dtreesPath = Path.Combine(modelDir, "opencv-csharp-ml-dtrees-模型.yml");
            string rtreesPath = Path.Combine(modelDir, "opencv-csharp-ml-rtrees-模型.yml");
            string boostPath = Path.Combine(modelDir, "opencv-csharp-ml-boost-模型.yml");

            using (var samples = CreateSamples())
            using (var responses = CreateResponses())
            using (var query = new Mat(1, 2, MatType.CV_32FC1))
            using (var dtreesResults = new Mat())
            using (var rtreesResults = new Mat())
            using (var boostResults = new Mat())
            using (var dtrees = DTrees.Create())
            using (var rtrees = RTrees.Create())
            using (var boost = Boost.Create())
            {
                query.CopyFrom<float>(new float[] { 0.1F, 0.2F });

                dtrees.MaxDepth = 4;
                dtrees.MinSampleCount = 1;
                dtrees.CVFolds = 0;
                Assert.True(dtrees.Train(samples, SampleTypes.RowSample, responses));
                float dtreesPrediction = dtrees.Predict(query, DTreesPredictionFlags.Auto, dtreesResults);
                Assert.True(dtreesPrediction == 0.0F || dtreesPrediction == 1.0F);
                Assert.Equal(1, dtreesResults.Rows);
                dtrees.Save(dtreesPath);

                Cv2.SetRngSeed(12345);
                rtrees.MaxDepth = 4;
                rtrees.MinSampleCount = 1;
                rtrees.CalculateVarImportance = true;
                rtrees.ActiveVarCount = 1;
                rtrees.TermCriteria = TermCriteria.ByCount(8);
                Assert.True(rtrees.Train(samples, SampleTypes.RowSample, responses));
                float rtreesPrediction = rtrees.Predict(query, rtreesResults);
                Assert.True(rtreesPrediction == 0.0F || rtreesPrediction == 1.0F);
                Assert.True(double.IsFinite(rtrees.OobError));

                using (Mat votes = rtrees.GetVotes(query, DTreesPredictionFlags.MaxVote))
                using (Mat sumVotes = rtrees.GetVotes(query, DTreesPredictionFlags.Sum, StatModelFlags.RawOutput))
                using (Mat callerVotes = new Mat())
                using (Mat importance = rtrees.GetVarImportance())
                {
                    rtrees.GetVotes(query, callerVotes, DTreesPredictionFlags.MaxVote);
                    Assert.Equal(2, votes.Rows);
                    Assert.Equal(2, votes.Cols);
                    Assert.Equal(votes.ToArray<int>(), callerVotes.ToArray<int>());
                    Assert.Equal(1, sumVotes.Rows);
                    Assert.Equal(8, sumVotes.Cols);
                    Assert.Equal((UIntPtr)2U, importance.Total);
                    importance.SetTo(new Scalar(123.0));
                }

                using (Mat secondImportance = rtrees.GetVarImportance())
                {
                    Assert.DoesNotContain(123.0F, secondImportance.ToArray<float>());
                }
                rtrees.Save(rtreesPath);

                boost.BoostType = BoostTypes.Discrete;
                boost.WeakCount = 8;
                boost.WeightTrimRate = 0.9;
                boost.MinSampleCount = 1;
                Assert.True(boost.Train(samples, SampleTypes.RowSample, responses));
                float boostPrediction = boost.Predict(query, boostResults);
                Assert.True(boostPrediction == 0.0F || boostPrediction == 1.0F);
                boost.Save(boostPath);
            }

            try
            {
                using (var query = new Mat(1, 2, MatType.CV_32FC1))
                using (var dtrees = DTrees.Load(dtreesPath))
                using (var rtrees = RTrees.Load(rtreesPath))
                using (var boost = Boost.Load(boostPath))
                {
                    query.CopyFrom<float>(new float[] { 0.1F, 0.2F });
                    Assert.True(dtrees.IsTrained);
                    Assert.True(rtrees.IsTrained);
                    Assert.True(boost.IsTrained);
                    Assert.True(dtrees.Predict(query) == 0.0F || dtrees.Predict(query) == 1.0F);
                    Assert.True(rtrees.Predict(query) == 0.0F || rtrees.Predict(query) == 1.0F);
                    Assert.True(boost.Predict(query) == 0.0F || boost.Predict(query) == 1.0F);
                }
            }
            finally
            {
                DeleteModelFiles(dtreesPath, rtreesPath, boostPath);
            }
        }

        [Fact]
        public void RTreesRegressionReturnsPerTreeResponsesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var samples = CreateRegressionSamples())
            using (var responses = CreateRegressionResponses())
            using (var query = new Mat(1, 2, MatType.CV_32FC1))
            using (var results = new Mat())
            using (var model = RTrees.Create())
            {
                query.CopyFrom<float>(new float[] { 2.5F, 1.0F });
                Cv2.SetRngSeed(54321);
                model.MaxDepth = 4;
                model.MinSampleCount = 1;
                model.TermCriteria = TermCriteria.ByCount(5);
                Assert.True(model.Train(samples, SampleTypes.RowSample, responses));
                Assert.False(model.IsClassifier);
                float prediction = model.Predict(query, results);
                Assert.True(float.IsFinite(prediction));
                Assert.Equal(1, results.Rows);

                using (Mat votes = model.GetVotes(query, DTreesPredictionFlags.Sum))
                {
                    Assert.Equal(1, votes.Rows);
                    Assert.Equal(5, votes.Cols);
                    Assert.All(votes.ToArray<float>(), value => Assert.True(float.IsFinite(value)));
                }
            }
        }

        [Fact]
        public void EMDefaultsPropertiesAndArgumentsRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var model = EM.Create())
            using (var mat = new Mat())
            {
                Assert.Equal(EM.DefaultClustersNumber, model.ClustersNumber);
                Assert.Equal(EMCovarianceMatrixTypes.Diagonal, model.CovarianceMatrixType);
                Assert.Equal(TermCriteriaTypes.CountOrEps, model.TermCriteria.Type);
                Assert.Equal(EM.DefaultMaxIterations, model.TermCriteria.MaxCount);
                Assert.Equal(1e-6, model.TermCriteria.Epsilon, 12);
                Assert.True(model.IsClassifier);
                Assert.False(model.IsTrained);

                model.ClustersNumber = 2;
                model.CovarianceMatrixType = EMCovarianceMatrixTypes.Generic;
                model.TermCriteria = TermCriteria.ByCountAndEpsilon(75, 1e-7);
                Assert.Equal(2, model.ClustersNumber);
                Assert.Equal(EMCovarianceMatrixTypes.Generic, model.CovarianceMatrixType);
                Assert.Equal(TermCriteria.ByCountAndEpsilon(75, 1e-7), model.TermCriteria);

                Assert.Throws<ArgumentOutOfRangeException>(() => model.ClustersNumber = 0);
                Assert.Throws<ArgumentOutOfRangeException>(() => model.CovarianceMatrixType = (EMCovarianceMatrixTypes)99);
                Assert.Throws<ArgumentNullException>(() => model.GetWeights(null!));
                Assert.Throws<ArgumentNullException>(() => model.GetMeans(null!));
                Assert.Throws<ArgumentNullException>(() => model.Predict2(null!));
                Assert.Throws<ArgumentNullException>(() => model.TrainEM(null!));
                Assert.Throws<ArgumentNullException>(() => model.TrainE(null!, mat));
                Assert.Throws<ArgumentNullException>(() => model.TrainE(mat, null!));
                Assert.Throws<ArgumentException>(() => model.TrainE(mat, mat, new Mat[] { null! }));
                Assert.Throws<ArgumentNullException>(() => model.TrainM(null!, mat));
                Assert.Throws<ArgumentNullException>(() => model.TrainM(mat, null!));

                using (Mat weights = model.GetWeights())
                using (Mat means = model.GetMeans())
                {
                    Assert.True(weights.Empty);
                    Assert.True(means.Empty);
                }
                Assert.Empty(model.GetCovariances());
            }
        }

        [Fact]
        public void EMTrainPredictOwnOutputsAndPersistWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string modelDir = TestEnvironment.GetMlModelDirVariable() ?? Path.GetTempPath();
            string modelPath = Path.Combine(modelDir, "opencv-csharp-ml-em-模型.yml");
            using (var samples = CreateEMSamples())
            using (var query = CreateEMQuery())
            using (var logLikelihoods = new Mat())
            using (var labels = new Mat())
            using (var probabilities = new Mat())
            using (var predictProbabilities = new Mat())
            using (var batchProbabilities = new Mat())
            using (var model = EM.Create())
            {
                Cv2.SetRngSeed(20260731);
                model.ClustersNumber = 2;
                model.CovarianceMatrixType = EMCovarianceMatrixTypes.Generic;
                model.TermCriteria = TermCriteria.ByCountAndEpsilon(100, 1e-8);
                Assert.True(model.TrainEM(samples, logLikelihoods, labels, probabilities));
                Assert.True(model.IsTrained);
                Assert.True(model.IsClassifier);
                Assert.Equal(2, model.VarCount);
                Assert.Equal(8, logLikelihoods.Rows);
                Assert.Equal(MatType.CV_64FC1, logLikelihoods.Type);
                Assert.Equal(8, labels.Rows);
                Assert.Equal(MatType.CV_32SC1, labels.Type);
                Assert.Equal(8, probabilities.Rows);
                Assert.Equal(2, probabilities.Cols);
                Assert.Equal(MatType.CV_64FC1, probabilities.Type);

                EMPredictionResult prediction = model.Predict2(query, predictProbabilities);
                Assert.True(double.IsFinite(prediction.LogLikelihood));
                Assert.InRange(prediction.Label, 0, 1);
                Assert.Equal(1, predictProbabilities.Rows);
                Assert.Equal(2, predictProbabilities.Cols);
                float batchPrediction = model.Predict(query, batchProbabilities);
                Assert.True(batchPrediction == 0.0F || batchPrediction == 1.0F);
                Assert.Equal(1, batchProbabilities.Rows);
                Assert.Equal(2, batchProbabilities.Cols);

                using (Mat weights = model.GetWeights())
                using (Mat means = model.GetMeans())
                {
                    double[] expectedWeights = weights.ToArray<double>();
                    double[] expectedMeans = means.ToArray<double>();
                    Assert.Equal(2, expectedWeights.Length);
                    Assert.Equal(4, expectedMeans.Length);
                    weights.SetTo(new Scalar(0.0));
                    means.SetTo(new Scalar(0.0));
                    using (Mat copiedWeights = model.GetWeights())
                    using (Mat copiedMeans = model.GetMeans())
                    {
                        Assert.Equal(expectedWeights, copiedWeights.ToArray<double>());
                        Assert.Equal(expectedMeans, copiedMeans.ToArray<double>());
                    }
                }

                Mat[] covariances = model.GetCovariances();
                try
                {
                    Assert.Equal(2, covariances.Length);
                    Assert.All(covariances, covariance =>
                    {
                        Assert.Equal(2, covariance.Rows);
                        Assert.Equal(2, covariance.Cols);
                        Assert.Equal(MatType.CV_64FC1, covariance.Type);
                    });
                    double[] expected = covariances[0].ToArray<double>();
                    covariances[0].SetTo(new Scalar(0.0));
                    Mat[] copied = model.GetCovariances();
                    try
                    {
                        Assert.Equal(expected, copied[0].ToArray<double>());
                    }
                    finally
                    {
                        DisposeMats(copied);
                    }
                }
                finally
                {
                    DisposeMats(covariances);
                }

                model.Save(modelPath);
            }

            try
            {
                using (var query = CreateEMQuery())
                using (var probabilities = new Mat())
                using (var loaded = EM.Load(modelPath))
                {
                    Assert.True(loaded.IsTrained);
                    EMPredictionResult prediction = loaded.Predict2(query, probabilities);
                    Assert.True(double.IsFinite(prediction.LogLikelihood));
                    Assert.InRange(prediction.Label, 0, 1);
                    Assert.Equal(2, probabilities.Cols);
                }
            }
            finally
            {
                DeleteModelFiles(modelPath);
            }
        }

        [Fact]
        public void EMTrainEAndTrainMUseInitialEstimatesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var samples = CreateEMSamples())
            using (var means = CreateEMInitialMeans())
            using (var weights = CreateEMInitialWeights())
            using (var initialProbabilities = CreateEMInitialProbabilities())
            using (var eProbabilities = new Mat())
            using (var mProbabilities = new Mat())
            using (var eModel = EM.Create())
            using (var mModel = EM.Create())
            {
                Mat[] covariances = CreateEMInitialCovariances();
                try
                {
                    eModel.ClustersNumber = 2;
                    eModel.CovarianceMatrixType = EMCovarianceMatrixTypes.Generic;
                    Assert.True(eModel.TrainE(samples, means, covariances, weights, probabilities: eProbabilities));
                }
                finally
                {
                    DisposeMats(covariances);
                }

                mModel.ClustersNumber = 2;
                mModel.CovarianceMatrixType = EMCovarianceMatrixTypes.Diagonal;
                Assert.True(mModel.TrainM(samples, initialProbabilities, probabilities: mProbabilities));
                Assert.True(eModel.IsTrained);
                Assert.True(mModel.IsTrained);
                Assert.Equal(8, eProbabilities.Rows);
                Assert.Equal(2, eProbabilities.Cols);
                Assert.Equal(8, mProbabilities.Rows);
                Assert.Equal(2, mProbabilities.Cols);

                using (var query = CreateEMQuery())
                {
                    Assert.InRange(eModel.Predict2(query).Label, 0, 1);
                    Assert.InRange(mModel.Predict2(query).Label, 0, 1);
                }
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

        private static Mat CreateRegressionSamples()
        {
            var samples = new Mat(6, 2, MatType.CV_32FC1);
            samples.CopyFrom<float>(new float[]
            {
                0.0F, 0.0F,
                1.0F, 0.0F,
                2.0F, 1.0F,
                3.0F, 1.0F,
                4.0F, 2.0F,
                5.0F, 2.0F
            });
            return samples;
        }

        private static Mat CreateRegressionResponses()
        {
            var responses = new Mat(6, 1, MatType.CV_32FC1);
            responses.CopyFrom<float>(new float[] { 0.0F, 1.0F, 3.0F, 4.0F, 6.0F, 7.0F });
            return responses;
        }

        private static Mat CreateEMSamples()
        {
            var samples = new Mat(8, 2, MatType.CV_64FC1);
            samples.CopyFrom<double>(new double[]
            {
                -0.2, 0.1,
                0.1, -0.2,
                0.3, 0.2,
                -0.1, -0.3,
                4.8, 5.1,
                5.2, 4.9,
                5.3, 5.2,
                4.7, 4.8
            });
            return samples;
        }

        private static Mat CreateEMQuery()
        {
            var query = new Mat(1, 2, MatType.CV_64FC1);
            query.CopyFrom<double>(new double[] { 0.0, 0.0 });
            return query;
        }

        private static Mat CreateEMInitialMeans()
        {
            var means = new Mat(2, 2, MatType.CV_64FC1);
            means.CopyFrom<double>(new double[] { 0.0, 0.0, 5.0, 5.0 });
            return means;
        }

        private static Mat CreateEMInitialWeights()
        {
            var weights = new Mat(1, 2, MatType.CV_64FC1);
            weights.CopyFrom<double>(new double[] { 0.5, 0.5 });
            return weights;
        }

        private static Mat[] CreateEMInitialCovariances()
        {
            return new[]
            {
                Mat.Eye(2, 2, MatType.CV_64FC1),
                Mat.Eye(2, 2, MatType.CV_64FC1)
            };
        }

        private static Mat CreateEMInitialProbabilities()
        {
            var probabilities = new Mat(8, 2, MatType.CV_64FC1);
            probabilities.CopyFrom<double>(new double[]
            {
                1.0, 0.0,
                1.0, 0.0,
                1.0, 0.0,
                1.0, 0.0,
                0.0, 1.0,
                0.0, 1.0,
                0.0, 1.0,
                0.0, 1.0
            });
            return probabilities;
        }

        private static void DisposeMats(Mat[] values)
        {
            foreach (Mat value in values)
            {
                value.Dispose();
            }
        }

        private static void DeleteModelFiles(params string[] paths)
        {
            foreach (string path in paths)
            {
                try
                {
                    File.Delete(path);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

    }
}
