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
    }
}
#endif
