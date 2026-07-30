#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_param_grid_create")]
        internal static partial int MlParamGridCreate(double minVal, double maxVal, double logStep, out IntPtr grid);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_param_grid_release_handle")]
        internal static partial void MlParamGridReleaseHandle(IntPtr grid);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_param_grid_get")]
        internal static partial int MlParamGridGet(IntPtr grid, out double minVal, out double maxVal, out double logStep);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_param_grid_set")]
        internal static partial int MlParamGridSet(IntPtr grid, double minVal, double maxVal, double logStep);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_default_grid")]
        internal static partial int MlSvmGetDefaultGrid(int paramId, out IntPtr grid);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_create")]
        internal static partial int MlTrainDataCreate(IntPtr samples, int layout, IntPtr responses, IntPtr varIdx, IntPtr sampleIdx, IntPtr sampleWeights, IntPtr varType, out IntPtr trainData);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_load_csv")]
        internal static partial int MlTrainDataLoadCsv(byte[] filename, int headerLineCount, int responseStartIdx, int responseEndIdx, byte[] varTypeSpec, int delimiter, int missch, out IntPtr trainData);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_release_handle")]
        internal static partial void MlTrainDataReleaseHandle(IntPtr trainData);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_int")]
        internal static partial int MlTrainDataGetInt(IntPtr trainData, int propertyId, int argument, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_mat")]
        internal static partial int MlTrainDataGetMat(IntPtr trainData, int propertyId, int layout, int compressSamples, int compressVars, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_set_train_test_split")]
        internal static partial int MlTrainDataSetTrainTestSplit(IntPtr trainData, int count, int shuffle);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_set_train_test_split_ratio")]
        internal static partial int MlTrainDataSetTrainTestSplitRatio(IntPtr trainData, double ratio, int shuffle);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_shuffle_train_test")]
        internal static partial int MlTrainDataShuffleTrainTest(IntPtr trainData);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_names_count")]
        internal static partial int MlTrainDataGetNamesCount(IntPtr trainData, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_names_fill")]
        internal static unsafe partial int MlTrainDataGetNamesFill(IntPtr trainData, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int stringCount, out int byteCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_sub_vector")]
        internal static partial int MlTrainDataGetSubVector(IntPtr vec, IntPtr idx, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_train_data_get_sub_matrix")]
        internal static partial int MlTrainDataGetSubMatrix(IntPtr matrix, IntPtr idx, int layout, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_knearest_create")]
        internal static partial int MlKNearestCreate(out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_knearest_load")]
        internal static partial int MlKNearestLoad(byte[] filepath, out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_create")]
        internal static partial int MlSvmCreate(out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_load")]
        internal static partial int MlSvmLoad(byte[] filepath, out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_normal_bayes_classifier_create")]
        internal static partial int MlNormalBayesClassifierCreate(out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_normal_bayes_classifier_load")]
        internal static partial int MlNormalBayesClassifierLoad(byte[] filepath, byte[] nodeName, out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_model_release_handle")]
        internal static partial void MlModelReleaseHandle(IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_get_int")]
        internal static partial int MlStatModelGetInt(IntPtr model, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_train_data")]
        internal static partial int MlStatModelTrainData(IntPtr model, IntPtr trainData, int flags, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_train_samples")]
        internal static partial int MlStatModelTrainSamples(IntPtr model, IntPtr samples, int layout, IntPtr responses, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_predict")]
        internal static partial int MlStatModelPredict(IntPtr model, IntPtr samples, IntPtr results, int flags, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_calc_error")]
        internal static partial int MlStatModelCalcError(IntPtr model, IntPtr data, int test, IntPtr responses, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_save")]
        internal static partial int MlStatModelSave(IntPtr model, byte[] filepath);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_stat_model_clear")]
        internal static partial int MlStatModelClear(IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_knearest_get_int")]
        internal static partial int MlKNearestGetInt(IntPtr model, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_knearest_set_int")]
        internal static partial int MlKNearestSetInt(IntPtr model, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_knearest_find_nearest")]
        internal static partial int MlKNearestFindNearest(IntPtr model, IntPtr samples, int k, IntPtr results, IntPtr neighborResponses, IntPtr dist, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_int")]
        internal static partial int MlSvmGetInt(IntPtr model, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_set_int")]
        internal static partial int MlSvmSetInt(IntPtr model, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_double")]
        internal static partial int MlSvmGetDouble(IntPtr model, int propertyId, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_set_double")]
        internal static partial int MlSvmSetDouble(IntPtr model, int propertyId, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_term_criteria")]
        internal static partial int MlSvmGetTermCriteria(IntPtr model, out int type, out int maxCount, out double epsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_set_term_criteria")]
        internal static partial int MlSvmSetTermCriteria(IntPtr model, int type, int maxCount, double epsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_class_weights")]
        internal static partial int MlSvmGetClassWeights(IntPtr model, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_set_class_weights")]
        internal static partial int MlSvmSetClassWeights(IntPtr model, IntPtr weights);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_train_auto")]
        internal static partial int MlSvmTrainAuto(IntPtr model, IntPtr samples, int layout, IntPtr responses, int kFold, IntPtr cGrid, IntPtr gammaGrid, IntPtr pGrid, IntPtr nuGrid, IntPtr coeffGrid, IntPtr degreeGrid, int balanced, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_support_vectors")]
        internal static partial int MlSvmGetSupportVectors(IntPtr model, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_uncompressed_support_vectors")]
        internal static partial int MlSvmGetUncompressedSupportVectors(IntPtr model, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_svm_get_decision_function")]
        internal static partial int MlSvmGetDecisionFunction(IntPtr model, int index, IntPtr alpha, IntPtr svidx, out double rho);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_normal_bayes_classifier_predict_prob")]
        internal static partial int MlNormalBayesClassifierPredictProb(IntPtr model, IntPtr inputs, IntPtr outputs, IntPtr outputProbs, int flags, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_create")]
        internal static partial int MlAnnMlpCreate(out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_load")]
        internal static partial int MlAnnMlpLoad(byte[] filepath, out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_get_int")]
        internal static partial int MlAnnMlpGetInt(IntPtr model, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_int")]
        internal static partial int MlAnnMlpSetInt(IntPtr model, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_get_double")]
        internal static partial int MlAnnMlpGetDouble(IntPtr model, int propertyId, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_double")]
        internal static partial int MlAnnMlpSetDouble(IntPtr model, int propertyId, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_train_method")]
        internal static partial int MlAnnMlpSetTrainMethod(IntPtr model, int method, double param1, double param2);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_activation_function")]
        internal static partial int MlAnnMlpSetActivationFunction(IntPtr model, int type, double param1, double param2);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_get_layer_sizes")]
        internal static partial int MlAnnMlpGetLayerSizes(IntPtr model, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_layer_sizes")]
        internal static partial int MlAnnMlpSetLayerSizes(IntPtr model, IntPtr layerSizes);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_get_term_criteria")]
        internal static partial int MlAnnMlpGetTermCriteria(IntPtr model, out int type, out int maxCount, out double epsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_term_criteria")]
        internal static partial int MlAnnMlpSetTermCriteria(IntPtr model, int type, int maxCount, double epsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_get_weights")]
        internal static partial int MlAnnMlpGetWeights(IntPtr model, int layerIndex, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_ann_mlp_set_anneal_energy_seed")]
        internal static partial int MlAnnMlpSetAnnealEnergySeed(IntPtr model, ulong seed);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_create")]
        internal static partial int MlDTreesCreate(out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_load")]
        internal static partial int MlDTreesLoad(byte[] filepath, byte[] nodeName, out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_get_int")]
        internal static partial int MlDTreesGetInt(IntPtr model, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_set_int")]
        internal static partial int MlDTreesSetInt(IntPtr model, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_get_regression_accuracy")]
        internal static partial int MlDTreesGetRegressionAccuracy(IntPtr model, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_set_regression_accuracy")]
        internal static partial int MlDTreesSetRegressionAccuracy(IntPtr model, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_get_priors")]
        internal static partial int MlDTreesGetPriors(IntPtr model, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_dtrees_set_priors")]
        internal static partial int MlDTreesSetPriors(IntPtr model, IntPtr priors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_create")]
        internal static partial int MlRTreesCreate(out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_load")]
        internal static partial int MlRTreesLoad(byte[] filepath, byte[] nodeName, out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_get_int")]
        internal static partial int MlRTreesGetInt(IntPtr model, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_set_int")]
        internal static partial int MlRTreesSetInt(IntPtr model, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_get_term_criteria")]
        internal static partial int MlRTreesGetTermCriteria(IntPtr model, out int type, out int maxCount, out double epsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_set_term_criteria")]
        internal static partial int MlRTreesSetTermCriteria(IntPtr model, int type, int maxCount, double epsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_get_var_importance")]
        internal static partial int MlRTreesGetVarImportance(IntPtr model, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_get_votes")]
        internal static partial int MlRTreesGetVotes(IntPtr model, IntPtr samples, IntPtr results, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_rtrees_get_oob_error")]
        internal static partial int MlRTreesGetOobError(IntPtr model, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_boost_create")]
        internal static partial int MlBoostCreate(out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_boost_load")]
        internal static partial int MlBoostLoad(byte[] filepath, byte[] nodeName, out IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_boost_get_int")]
        internal static partial int MlBoostGetInt(IntPtr model, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_boost_set_int")]
        internal static partial int MlBoostSetInt(IntPtr model, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_boost_get_weight_trim_rate")]
        internal static partial int MlBoostGetWeightTrimRate(IntPtr model, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ml_boost_set_weight_trim_rate")]
        internal static partial int MlBoostSetWeightTrimRate(IntPtr model, double value);
    }
}
#endif
