#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_param_grid_create")]
        internal static extern int MlParamGridCreate(double minVal, double maxVal, double logStep, out IntPtr grid);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_param_grid_release_handle")]
        internal static extern void MlParamGridReleaseHandle(IntPtr grid);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_param_grid_get")]
        internal static extern int MlParamGridGet(IntPtr grid, out double minVal, out double maxVal, out double logStep);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_param_grid_set")]
        internal static extern int MlParamGridSet(IntPtr grid, double minVal, double maxVal, double logStep);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_default_grid")]
        internal static extern int MlSvmGetDefaultGrid(int paramId, out IntPtr grid);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_create")]
        internal static extern int MlTrainDataCreate(IntPtr samples, int layout, IntPtr responses, IntPtr varIdx, IntPtr sampleIdx, IntPtr sampleWeights, IntPtr varType, out IntPtr trainData);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_load_csv")]
        internal static extern int MlTrainDataLoadCsv(byte[] filename, int headerLineCount, int responseStartIdx, int responseEndIdx, byte[] varTypeSpec, int delimiter, int missch, out IntPtr trainData);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_release_handle")]
        internal static extern void MlTrainDataReleaseHandle(IntPtr trainData);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_int")]
        internal static extern int MlTrainDataGetInt(IntPtr trainData, int propertyId, int argument, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_mat")]
        internal static extern int MlTrainDataGetMat(IntPtr trainData, int propertyId, int layout, int compressSamples, int compressVars, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_set_train_test_split")]
        internal static extern int MlTrainDataSetTrainTestSplit(IntPtr trainData, int count, int shuffle);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_set_train_test_split_ratio")]
        internal static extern int MlTrainDataSetTrainTestSplitRatio(IntPtr trainData, double ratio, int shuffle);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_shuffle_train_test")]
        internal static extern int MlTrainDataShuffleTrainTest(IntPtr trainData);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_names_count")]
        internal static extern int MlTrainDataGetNamesCount(IntPtr trainData, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_names_fill")]
        internal static extern unsafe int MlTrainDataGetNamesFill(IntPtr trainData, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int stringCount, out int byteCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_sub_vector")]
        internal static extern int MlTrainDataGetSubVector(IntPtr vec, IntPtr idx, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_sub_matrix")]
        internal static extern int MlTrainDataGetSubMatrix(IntPtr matrix, IntPtr idx, int layout, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_knearest_create")]
        internal static extern int MlKNearestCreate(out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_knearest_load")]
        internal static extern int MlKNearestLoad(byte[] filepath, out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_create")]
        internal static extern int MlSvmCreate(out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_load")]
        internal static extern int MlSvmLoad(byte[] filepath, out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_normal_bayes_classifier_create")]
        internal static extern int MlNormalBayesClassifierCreate(out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_normal_bayes_classifier_load")]
        internal static extern int MlNormalBayesClassifierLoad(byte[] filepath, byte[] nodeName, out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_model_release_handle")]
        internal static extern void MlModelReleaseHandle(IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_get_int")]
        internal static extern int MlStatModelGetInt(IntPtr model, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_train_data")]
        internal static extern int MlStatModelTrainData(IntPtr model, IntPtr trainData, int flags, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_train_samples")]
        internal static extern int MlStatModelTrainSamples(IntPtr model, IntPtr samples, int layout, IntPtr responses, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_predict")]
        internal static extern int MlStatModelPredict(IntPtr model, IntPtr samples, IntPtr results, int flags, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_calc_error")]
        internal static extern int MlStatModelCalcError(IntPtr model, IntPtr data, int test, IntPtr responses, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_save")]
        internal static extern int MlStatModelSave(IntPtr model, byte[] filepath);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_clear")]
        internal static extern int MlStatModelClear(IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_knearest_get_int")]
        internal static extern int MlKNearestGetInt(IntPtr model, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_knearest_set_int")]
        internal static extern int MlKNearestSetInt(IntPtr model, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_knearest_find_nearest")]
        internal static extern int MlKNearestFindNearest(IntPtr model, IntPtr samples, int k, IntPtr results, IntPtr neighborResponses, IntPtr dist, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_int")]
        internal static extern int MlSvmGetInt(IntPtr model, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_set_int")]
        internal static extern int MlSvmSetInt(IntPtr model, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_double")]
        internal static extern int MlSvmGetDouble(IntPtr model, int propertyId, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_set_double")]
        internal static extern int MlSvmSetDouble(IntPtr model, int propertyId, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_term_criteria")]
        internal static extern int MlSvmGetTermCriteria(IntPtr model, out int type, out int maxCount, out double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_set_term_criteria")]
        internal static extern int MlSvmSetTermCriteria(IntPtr model, int type, int maxCount, double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_class_weights")]
        internal static extern int MlSvmGetClassWeights(IntPtr model, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_set_class_weights")]
        internal static extern int MlSvmSetClassWeights(IntPtr model, IntPtr weights);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_train_auto")]
        internal static extern int MlSvmTrainAuto(IntPtr model, IntPtr samples, int layout, IntPtr responses, int kFold, IntPtr cGrid, IntPtr gammaGrid, IntPtr pGrid, IntPtr nuGrid, IntPtr coeffGrid, IntPtr degreeGrid, int balanced, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_support_vectors")]
        internal static extern int MlSvmGetSupportVectors(IntPtr model, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_uncompressed_support_vectors")]
        internal static extern int MlSvmGetUncompressedSupportVectors(IntPtr model, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_decision_function")]
        internal static extern int MlSvmGetDecisionFunction(IntPtr model, int index, IntPtr alpha, IntPtr svidx, out double rho);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_normal_bayes_classifier_predict_prob")]
        internal static extern int MlNormalBayesClassifierPredictProb(IntPtr model, IntPtr inputs, IntPtr outputs, IntPtr outputProbs, int flags, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_create")]
        internal static extern int MlAnnMlpCreate(out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_load")]
        internal static extern int MlAnnMlpLoad(byte[] filepath, out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_get_int")]
        internal static extern int MlAnnMlpGetInt(IntPtr model, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_int")]
        internal static extern int MlAnnMlpSetInt(IntPtr model, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_get_double")]
        internal static extern int MlAnnMlpGetDouble(IntPtr model, int propertyId, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_double")]
        internal static extern int MlAnnMlpSetDouble(IntPtr model, int propertyId, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_train_method")]
        internal static extern int MlAnnMlpSetTrainMethod(IntPtr model, int method, double param1, double param2);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_activation_function")]
        internal static extern int MlAnnMlpSetActivationFunction(IntPtr model, int type, double param1, double param2);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_get_layer_sizes")]
        internal static extern int MlAnnMlpGetLayerSizes(IntPtr model, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_layer_sizes")]
        internal static extern int MlAnnMlpSetLayerSizes(IntPtr model, IntPtr layerSizes);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_get_term_criteria")]
        internal static extern int MlAnnMlpGetTermCriteria(IntPtr model, out int type, out int maxCount, out double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_term_criteria")]
        internal static extern int MlAnnMlpSetTermCriteria(IntPtr model, int type, int maxCount, double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_get_weights")]
        internal static extern int MlAnnMlpGetWeights(IntPtr model, int layerIndex, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_anneal_energy_seed")]
        internal static extern int MlAnnMlpSetAnnealEnergySeed(IntPtr model, ulong seed);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_create")]
        internal static extern int MlDTreesCreate(out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_load")]
        internal static extern int MlDTreesLoad(byte[] filepath, byte[] nodeName, out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_get_int")]
        internal static extern int MlDTreesGetInt(IntPtr model, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_set_int")]
        internal static extern int MlDTreesSetInt(IntPtr model, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_get_regression_accuracy")]
        internal static extern int MlDTreesGetRegressionAccuracy(IntPtr model, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_set_regression_accuracy")]
        internal static extern int MlDTreesSetRegressionAccuracy(IntPtr model, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_get_priors")]
        internal static extern int MlDTreesGetPriors(IntPtr model, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_set_priors")]
        internal static extern int MlDTreesSetPriors(IntPtr model, IntPtr priors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_create")]
        internal static extern int MlRTreesCreate(out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_load")]
        internal static extern int MlRTreesLoad(byte[] filepath, byte[] nodeName, out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_get_int")]
        internal static extern int MlRTreesGetInt(IntPtr model, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_set_int")]
        internal static extern int MlRTreesSetInt(IntPtr model, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_get_term_criteria")]
        internal static extern int MlRTreesGetTermCriteria(IntPtr model, out int type, out int maxCount, out double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_set_term_criteria")]
        internal static extern int MlRTreesSetTermCriteria(IntPtr model, int type, int maxCount, double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_get_var_importance")]
        internal static extern int MlRTreesGetVarImportance(IntPtr model, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_get_votes")]
        internal static extern int MlRTreesGetVotes(IntPtr model, IntPtr samples, IntPtr results, int flags);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_get_oob_error")]
        internal static extern int MlRTreesGetOobError(IntPtr model, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_boost_create")]
        internal static extern int MlBoostCreate(out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_boost_load")]
        internal static extern int MlBoostLoad(byte[] filepath, byte[] nodeName, out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_boost_get_int")]
        internal static extern int MlBoostGetInt(IntPtr model, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_boost_set_int")]
        internal static extern int MlBoostSetInt(IntPtr model, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_boost_get_weight_trim_rate")]
        internal static extern int MlBoostGetWeightTrimRate(IntPtr model, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_boost_set_weight_trim_rate")]
        internal static extern int MlBoostSetWeightTrimRate(IntPtr model, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_create")]
        internal static extern int MlEMCreate(out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_load")]
        internal static extern int MlEMLoad(byte[] filepath, byte[] nodeName, out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_get_int")]
        internal static extern int MlEMGetInt(IntPtr model, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_set_int")]
        internal static extern int MlEMSetInt(IntPtr model, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_get_term_criteria")]
        internal static extern int MlEMGetTermCriteria(IntPtr model, out int type, out int maxCount, out double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_set_term_criteria")]
        internal static extern int MlEMSetTermCriteria(IntPtr model, int type, int maxCount, double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_get_weights")]
        internal static extern int MlEMGetWeights(IntPtr model, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_get_means")]
        internal static extern int MlEMGetMeans(IntPtr model, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_get_covariances_count")]
        internal static extern int MlEMGetCovariancesCount(IntPtr model, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_get_covariances_fill")]
        internal static extern int MlEMGetCovariancesFill(IntPtr model, IntPtr[] covariances, int covarianceCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_predict2")]
        internal static extern int MlEMPredict2(IntPtr model, IntPtr sample, IntPtr probabilities, out double logLikelihood, out int label);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_train_em")]
        internal static extern int MlEMTrainEM(IntPtr model, IntPtr samples, IntPtr logLikelihoods, IntPtr labels, IntPtr probabilities, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_train_e")]
        internal static extern int MlEMTrainE(IntPtr model, IntPtr samples, IntPtr initialMeans, IntPtr[] initialCovariances, int initialCovarianceCount, IntPtr initialWeights, IntPtr logLikelihoods, IntPtr labels, IntPtr probabilities, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_em_train_m")]
        internal static extern int MlEMTrainM(IntPtr model, IntPtr samples, IntPtr initialProbabilities, IntPtr logLikelihoods, IntPtr labels, IntPtr probabilities, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_sample_count")]
        internal static extern int MlTrainDataGetSampleCount(IntPtr trainData, IntPtr variableIndices, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_sample_fill")]
        internal static unsafe extern int MlTrainDataGetSampleFill(IntPtr trainData, IntPtr variableIndices, int sampleIndex, float* values, int valueCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_values_count")]
        internal static extern int MlTrainDataGetValuesCount(IntPtr trainData, IntPtr sampleIndices, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_values_fill")]
        internal static unsafe extern int MlTrainDataGetValuesFill(IntPtr trainData, int variableIndex, IntPtr sampleIndices, float* values, int valueCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_logistic_regression_create")]
        internal static extern int MlLogisticRegressionCreate(out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_logistic_regression_load")]
        internal static extern int MlLogisticRegressionLoad(byte[] filepath, byte[] nodeName, out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_logistic_regression_get_learning_rate")]
        internal static extern int MlLogisticRegressionGetLearningRate(IntPtr model, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_logistic_regression_set_learning_rate")]
        internal static extern int MlLogisticRegressionSetLearningRate(IntPtr model, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_logistic_regression_get_int")]
        internal static extern int MlLogisticRegressionGetInt(IntPtr model, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_logistic_regression_set_int")]
        internal static extern int MlLogisticRegressionSetInt(IntPtr model, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_logistic_regression_get_term_criteria")]
        internal static extern int MlLogisticRegressionGetTermCriteria(IntPtr model, out int type, out int maxCount, out double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_logistic_regression_set_term_criteria")]
        internal static extern int MlLogisticRegressionSetTermCriteria(IntPtr model, int type, int maxCount, double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_logistic_regression_get_learnt_thetas")]
        internal static extern int MlLogisticRegressionGetLearntThetas(IntPtr model, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svmsgd_create")]
        internal static extern int MlSVMSGDCreate(out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svmsgd_load")]
        internal static extern int MlSVMSGDLoad(byte[] filepath, byte[] nodeName, out IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svmsgd_get_weights")]
        internal static extern int MlSVMSGDGetWeights(IntPtr model, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svmsgd_get_shift")]
        internal static extern int MlSVMSGDGetShift(IntPtr model, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svmsgd_set_optimal_parameters")]
        internal static extern int MlSVMSGDSetOptimalParameters(IntPtr model, int svmsgdType, int marginType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svmsgd_get_int")]
        internal static extern int MlSVMSGDGetInt(IntPtr model, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svmsgd_set_int")]
        internal static extern int MlSVMSGDSetInt(IntPtr model, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svmsgd_get_float")]
        internal static extern int MlSVMSGDGetFloat(IntPtr model, int propertyId, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svmsgd_set_float")]
        internal static extern int MlSVMSGDSetFloat(IntPtr model, int propertyId, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svmsgd_get_term_criteria")]
        internal static extern int MlSVMSGDGetTermCriteria(IntPtr model, out int type, out int maxCount, out double epsilon);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svmsgd_set_term_criteria")]
        internal static extern int MlSVMSGDSetTermCriteria(IntPtr model, int type, int maxCount, double epsilon);
    }
}
#endif
