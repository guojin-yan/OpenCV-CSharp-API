# ML Guide / ML 指南

`OpenCvSharp.ML` wraps the first OpenCV 5.0.0 machine-learning objects from the local contrib `ml` tree.

`OpenCvSharp.ML` 封装第一批来自本地 OpenCV 5.0.0 contrib `ml` 树的机器学习对象。

## Scope / 范围

- Training data: `TrainData` from in-memory matrices or CSV files.
- Model base: `StatModel` state, training, prediction, error calculation, save, and clear.
- Models: `KNearest`, `SVM`, `SVMSGD`, `LogisticRegression`, `NormalBayesClassifier`, `EM`, `DTrees`, `RTrees`, `Boost`, and `ANN_MLP`.
- Parameter grids: `ParamGrid` and `SVM.GetDefaultGrid`.
- Enums: sample layout, variable type, model flags, KNN algorithm, SVM type, SVM kernel, SVM parameter ids, and EM covariance constraints.

- 训练数据：从内存矩阵或 CSV 文件创建 `TrainData`。
- 模型基类：`StatModel` 状态、训练、预测、误差计算、保存和清理。
- 模型：`KNearest`、`SVM`、`SVMSGD`、`LogisticRegression`、`NormalBayesClassifier`、`EM`、`DTrees`、`RTrees`、`Boost` 和 `ANN_MLP`。
- 参数网格：`ParamGrid` 与 `SVM.GetDefaultGrid`。
- 枚举：样本布局、变量类型、模型标志、KNN 算法、SVM 类型、SVM 核函数、SVM 参数 id 和 EM 协方差约束。

## Runtime / 运行时

In this local OpenCV 5.0.0 source layout, `ml` is provided by the contrib tree, not the main OpenCV module tree. A linked runtime should include the factual OpenCV 5.0.0 runtime artifact `opencv_ml500.dll`. If the module is not linked, the exported ABI remains present and managed calls report `NOT_LINKED`.

ML entrypoints belong to the full runtime profile. The mini profile deliberately excludes the ML source and ABI surface; use a full runtime package for `OpenCvSharp.ML`.

在当前本地 OpenCV 5.0.0 源码布局中，`ml` 来自 contrib 树，而不是 OpenCV 主仓库模块树。linked runtime 应包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_ml500.dll`。如果模块未链接，导出的 ABI 仍存在，managed 调用会报告 `NOT_LINKED`。

## Data Notes / 数据说明

OpenCV ML training samples usually use `CV_32F`. For row samples, every row is one sample and every column is one variable. Response type and classifier/regression settings affect training behavior, so keep `responses` aligned with the model type.

OpenCV ML 训练样本通常使用 `CV_32F`。使用 row samples 时，每一行是一条样本，每一列是一个变量。`responses` 类型以及分类/回归设置会影响训练行为，因此要让响应数据与模型类型保持一致。

`TrainData` exposes the stable data surface that can be represented through this C ABI. String and vector-like outputs stay inside native code and are copied through count/fill style APIs.

`GetSample` and `GetValues` return new `float[]` instances or fill caller-owned arrays. Caller-owned arrays must have exactly the required length; undersized and oversized arrays are both rejected before native memory is written. Optional `CV_32S` index vectors preserve caller order, while omitted indexes select every variable or sample.

`TrainData` 只暴露可通过当前 C ABI 稳定表达的数据面。字符串和类似 vector 的输出留在 native 内部，并通过 count/fill 风格 API 复制出来。

`SVM.TrainAuto` can be slower than tiny smoke examples because OpenCV searches parameter grids. Samples and default tests use direct training on small matrices.

`SVM.TrainAuto` 会搜索参数网格，因此可能比 tiny smoke 示例慢。示例和默认测试使用小矩阵直接训练。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ML;

using Mat samples = new Mat(4, 2, MatType.CV_32FC1);
samples.CopyFrom<float>(new float[]
{
    0.0F, 0.0F,
    0.0F, 1.0F,
    5.0F, 5.0F,
    6.0F, 5.0F
});

using Mat responses = new Mat(4, 1, MatType.CV_32SC1);
responses.CopyFrom<int>(new[] { 0, 0, 1, 1 });

using KNearest knn = KNearest.Create();
knn.DefaultK = 1;
knn.Train(samples, SampleTypes.RowSample, responses);

using Mat query = new Mat(1, 2, MatType.CV_32FC1);
query.CopyFrom<float>(new[] { 0.1F, 0.2F });
float predicted = knn.Predict(query);
```

## LogisticRegression and SVMSGD

`LogisticRegression` exposes learning rate, iteration count, regularization, batch or mini-batch training, mini-batch size, and termination criteria. Training samples and responses must use `CV_32F`; classifier responses use `0` and `1`. `GetLearntThetas()` and its caller-owned overload always deep-copy the learned parameter matrix.

`SVMSGD` exposes SGD/ASGD and soft/hard-margin configuration, regularization and step parameters, recommended-parameter initialization, termination criteria, decision shift, and copied weights. Training samples and responses use `CV_32F`; signed classifier responses should include negative and positive values. `GetWeights()` and its caller-owned overload return independent matrices.

Both types inherit `StatModel`, including training, prediction, state, clear, save, and load. OpenCV's SVMSGD implementation returns the direct `-1/+1` label when a single-sample prediction omits the results matrix. When a results matrix is supplied it writes labels there and the upstream scalar return is `0`; this binding preserves that behavior.

```csharp
using Mat binaryResponses = new Mat(4, 1, MatType.CV_32FC1);
binaryResponses.CopyFrom<float>(new[] { 0.0F, 0.0F, 1.0F, 1.0F });

using LogisticRegression logistic = LogisticRegression.Create();
logistic.LearningRate = 0.05;
logistic.TrainingMethod = LogisticRegressionTrainingMethods.MiniBatch;
logistic.MiniBatchSize = 2;
logistic.Train(samples, SampleTypes.RowSample, binaryResponses);
using Mat thetas = logistic.GetLearntThetas();

using Mat signedResponses = new Mat(4, 1, MatType.CV_32FC1);
signedResponses.CopyFrom<float>(new[] { -1.0F, -1.0F, 1.0F, 1.0F });

using SVMSGD svmsgd = SVMSGD.Create();
svmsgd.SetOptimalParameters();
svmsgd.Train(samples, SampleTypes.RowSample, signedResponses);
using Mat weights = svmsgd.GetWeights();
```

## ANN_MLP

`ANN_MLP` separates network configuration from training. Supply a `CV_32S` layer-size vector that includes the input and output layers, select an activation function and training method, and then train with `CV_32F` samples and responses. The `Train(TrainData, ANN_MLPTrainFlags)` overload exposes ANN-specific update and scaling flags.

`GetLayerSizes()` and `GetWeights(int)` return independently owned `Mat` copies. Disposing or modifying those matrices does not modify the model's internal storage. `SetAnnealEnergySeed(ulong)` provides deterministic seed control without exposing OpenCV's C++ `RNG` type through the stable C ABI.

```csharp
using Mat layers = new Mat(1, 3, MatType.CV_32SC1);
layers.CopyFrom<int>(new[] { 2, 4, 1 });

using Mat annResponses = new Mat(4, 1, MatType.CV_32FC1);
annResponses.CopyFrom<float>(new[] { -2.0F, 0.0F, 0.0F, 2.0F });

using ANN_MLP ann = ANN_MLP.Create();
ann.SetLayerSizes(layers);
ann.SetActivationFunction(ANN_MLPActivationFunctions.Identity);
ann.SetTrainMethod(ANN_MLPTrainingMethods.Rprop, 0.1, 1e-6);
ann.TermCriteria = TermCriteria.ByCountAndEpsilon(300, 1e-6);
ann.Train(samples, SampleTypes.RowSample, annResponses);

using Mat weights = ann.GetWeights(1);
```

## EM

`EM` models a Gaussian mixture with typed cluster-count, covariance-type, and termination-criteria properties. `TrainEM` performs automatic initialization. `TrainE` accepts initial means plus an optional `Mat[]` covariance collection and optional weights, preserving OpenCV's `InputArrayOfArrays` semantics. `TrainM` starts from caller-provided posterior probabilities. Training inputs are consumed synchronously and are never retained as managed or unmanaged pointers.

Optional log-likelihood, label, and probability outputs are caller-owned `Mat` instances. Omitting one passes OpenCV's no-output sentinel; supplying one lets OpenCV create or replace that matrix's contents. `GetWeights()`, `GetMeans()`, and every element returned by `GetCovariances()` are independent deep copies, so their lifetime and mutation are isolated from the model. `Predict2` returns the log-likelihood and component label as `EMPredictionResult` and can also fill a caller-owned probability matrix. The inherited `StatModel.Predict` remains available for batch label prediction.

Use `CV_32F` or another OpenCV-supported floating-point representation consistently for samples and initial estimates. Every sample row must have the same variable count; initial means use one row per cluster, covariance collections use one square matrix per cluster, initial weights contain one value per cluster, and initial probability matrices contain one row per sample and one column per cluster. Invalid shapes, depths, values, or an untrained prediction are reported through the normal `OpenCvException` bridge.

```csharp
using Mat emSamples = new Mat(4, 2, MatType.CV_32FC1);
emSamples.CopyFrom<float>(new float[]
{
    0.0F, 0.0F,
    0.2F, 0.1F,
    5.0F, 5.0F,
    5.2F, 4.9F
});

using EM em = EM.Create();
em.ClustersNumber = 2;
em.CovarianceMatrixType = EMCovarianceMatrixTypes.Generic;
em.TrainEM(emSamples);

using Mat emQuery = new Mat(1, 2, MatType.CV_32FC1);
emQuery.CopyFrom<float>(new[] { 0.1F, 0.2F });
using Mat probabilities = new Mat();
EMPredictionResult prediction = em.Predict2(emQuery, probabilities);

using Mat mixtureWeights = em.GetWeights();
Mat[] covariances = em.GetCovariances();
try
{
    Console.WriteLine($"component={prediction.Label}, likelihood={prediction.LogLikelihood}");
}
finally
{
    foreach (Mat covariance in covariances)
    {
        covariance.Dispose();
    }
}
```

## Tree Models

`DTrees` exposes the shared tree parameters and is the managed base class of `RTrees` and `Boost`, matching the OpenCV inheritance model. `RTrees` adds forest termination criteria, variable-importance calculation, per-tree responses or class-vote counts, and the source-reviewed OpenCV 5.0.0 out-of-bag error. `Boost` adds a typed boosting algorithm, weak-learner count, and weight-trimming threshold.

`DTrees.GetPriors()`, `RTrees.GetVarImportance()`, and every allocating `RTrees.GetVotes()` overload return independent `Mat` values. Modifying or disposing those outputs does not mutate the model. `DTrees.SetPriors(Mat)` follows OpenCV's exact shallow `cv::Mat` assignment: the model keeps reference-counted ownership after the caller disposes its wrapper, while caller mutations to the same underlying matrix remain visible until the priors are replaced, training consumes or clears the state, or the model is disposed.

`DTreesPredictionFlags.Auto`, `Sum`, and `MaxVote` are valid prediction modes. `Mask` is exposed because it is part of the upstream enum contract, but it is only a bit mask and managed prediction/vote methods reject it as a standalone mode. Combine a tree mode with `StatModelFlags.RawOutput` through the separate `flags` argument.

Random-forest training uses OpenCV's current-thread global RNG. Call `OpenCvSharp.Core.Cv2.SetRngSeed` immediately before training when a repeatable local sequence is required, and do not interleave other RNG-consuming operations on that thread. Model configuration, training, clearing, and disposal are mutable lifecycle operations and must not race with prediction. After successful training, callers may coordinate concurrent read-only prediction according to their own model lifetime policy; the wrapper does not add locking.

```csharp
using RTrees forest = RTrees.Create();
forest.MaxDepth = 4;
forest.MinSampleCount = 1;
forest.CalculateVarImportance = true;
forest.ActiveVarCount = 1;
forest.TermCriteria = TermCriteria.ByCount(32);

OpenCvSharp.Core.Cv2.SetRngSeed(12345);
forest.Train(samples, SampleTypes.RowSample, responses);

using Mat votes = forest.GetVotes(samples, DTreesPredictionFlags.MaxVote);
using Mat importance = forest.GetVarImportance();
```

OpenCV's non-`CV_WRAP` `DTrees::getRoots`, `getNodes`, `getSplits`, and `getSubsets` return borrowed internal vectors whose contents and indexes are invalidated by mutable model operations. They are intentionally not projected as borrowed managed objects. A future API may expose copied immutable snapshots only after their cross-version serialization and invalidation contract is independently established.
