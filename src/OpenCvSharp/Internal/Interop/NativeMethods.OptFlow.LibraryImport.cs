#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_release_handle")]
        internal static partial void OptFlowDenseReleaseHandle(IntPtr flow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_release_handle")]
        internal static partial void OptFlowSparseReleaseHandle(IntPtr flow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_release_handle")]
        internal static partial void OptFlowRlofParameterReleaseHandle(IntPtr parameter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_calc")]
        internal static partial int OptFlowDenseCalc(IntPtr flow, IntPtr i0, IntPtr i1, IntPtr outputFlow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_collect_garbage")]
        internal static partial int OptFlowDenseCollectGarbage(IntPtr flow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_calc")]
        internal static partial int OptFlowSparseCalc(IntPtr flow, IntPtr prevImg, IntPtr nextImg, IntPtr prevPts, IntPtr nextPts, IntPtr status, IntPtr err);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dual_tvl1_create")]
        internal static partial int OptFlowDualTvl1Create(double tau, double lambdaValue, double theta, int nscales, int warps, double epsilon, int innerIterations, int outerIterations, double scaleStep, double gamma, int medianFiltering, int useInitialFlow, out IntPtr flow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dual_tvl1_get_int")]
        internal static partial int OptFlowDualTvl1GetInt(IntPtr flow, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dual_tvl1_set_int")]
        internal static partial int OptFlowDualTvl1SetInt(IntPtr flow, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dual_tvl1_get_double")]
        internal static partial int OptFlowDualTvl1GetDouble(IntPtr flow, int propertyId, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dual_tvl1_set_double")]
        internal static partial int OptFlowDualTvl1SetDouble(IntPtr flow, int propertyId, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_create")]
        internal static partial int OptFlowRlofParameterCreate(out IntPtr parameter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_get_int")]
        internal static partial int OptFlowRlofParameterGetInt(IntPtr parameter, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_set_int")]
        internal static partial int OptFlowRlofParameterSetInt(IntPtr parameter, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_get_float")]
        internal static partial int OptFlowRlofParameterGetFloat(IntPtr parameter, int propertyId, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_set_float")]
        internal static partial int OptFlowRlofParameterSetFloat(IntPtr parameter, int propertyId, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_set_use_m_estimator")]
        internal static partial int OptFlowRlofParameterSetUseMEstimator(IntPtr parameter, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_create")]
        internal static partial int OptFlowDenseRlofCreate(IntPtr parameter, float forwardBackwardThreshold, int gridWidth, int gridHeight, int interpolationType, int epicK, float epicSigma, float epicLambda, int ricSpSize, int ricSlicType, int usePostProc, float fgsLambda, float fgsSigma, int useVariationalRefinement, out IntPtr flow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_get_parameter")]
        internal static partial int OptFlowDenseRlofGetParameter(IntPtr flow, out IntPtr parameter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_set_parameter")]
        internal static partial int OptFlowDenseRlofSetParameter(IntPtr flow, IntPtr parameter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_get_int")]
        internal static partial int OptFlowDenseRlofGetInt(IntPtr flow, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_set_int")]
        internal static partial int OptFlowDenseRlofSetInt(IntPtr flow, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_get_float")]
        internal static partial int OptFlowDenseRlofGetFloat(IntPtr flow, int propertyId, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_set_float")]
        internal static partial int OptFlowDenseRlofSetFloat(IntPtr flow, int propertyId, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_get_grid_step")]
        internal static partial int OptFlowDenseRlofGetGridStep(IntPtr flow, out int width, out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_set_grid_step")]
        internal static partial int OptFlowDenseRlofSetGridStep(IntPtr flow, int width, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_rlof_create")]
        internal static partial int OptFlowSparseRlofCreate(IntPtr parameter, float forwardBackwardThreshold, out IntPtr flow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_rlof_get_parameter")]
        internal static partial int OptFlowSparseRlofGetParameter(IntPtr flow, out IntPtr parameter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_rlof_set_parameter")]
        internal static partial int OptFlowSparseRlofSetParameter(IntPtr flow, IntPtr parameter);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_rlof_get_forward_backward")]
        internal static partial int OptFlowSparseRlofGetForwardBackward(IntPtr flow, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_rlof_set_forward_backward")]
        internal static partial int OptFlowSparseRlofSetForwardBackward(IntPtr flow, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_create_deep_flow")]
        internal static partial int OptFlowCreateDeepFlow(out IntPtr flow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_create_simple_flow")]
        internal static partial int OptFlowCreateSimpleFlow(out IntPtr flow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_create_farneback")]
        internal static partial int OptFlowCreateFarneback(out IntPtr flow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_create_sparse_to_dense")]
        internal static partial int OptFlowCreateSparseToDense(out IntPtr flow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_calc_optical_flow_sf_simple")]
        internal static partial int OptFlowCalcOpticalFlowSFSimple(IntPtr from, IntPtr to, IntPtr flow, int layers, int averagingBlockSize, int maxFlow);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_calc_optical_flow_sf")]
        internal static partial int OptFlowCalcOpticalFlowSF(IntPtr from, IntPtr to, IntPtr flow, int layers, int averagingBlockSize, int maxFlow, double sigmaDist, double sigmaColor, int postprocessWindow, double sigmaDistFix, double sigmaColorFix, double occThr, int upscaleAveragingRadius, double upscaleSigmaDist, double upscaleSigmaColor, double speedUpThr);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_calc_optical_flow_sparse_to_dense")]
        internal static partial int OptFlowCalcOpticalFlowSparseToDense(IntPtr from, IntPtr to, IntPtr flow, int gridStep, int k, float sigma, int usePostProc, float fgsLambda, float fgsSigma);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_calc_optical_flow_dense_rlof")]
        internal static partial int OptFlowCalcOpticalFlowDenseRlof(IntPtr i0, IntPtr i1, IntPtr flow, IntPtr parameter, float forwardBackwardThreshold, int gridWidth, int gridHeight, int interpolationType, int epicK, float epicSigma, float epicLambda, int ricSpSize, int ricSlicType, int usePostProc, float fgsLambda, float fgsSigma, int useVariationalRefinement);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_calc_optical_flow_sparse_rlof")]
        internal static partial int OptFlowCalcOpticalFlowSparseRlof(IntPtr prevImg, IntPtr nextImg, IntPtr prevPts, IntPtr nextPts, IntPtr status, IntPtr err, IntPtr parameter, float forwardBackwardThreshold);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_motempl_update_motion_history")]
        internal static partial int MotionTemplateUpdateMotionHistory(IntPtr silhouette, IntPtr mhi, double timestamp, double duration);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_motempl_calc_motion_gradient")]
        internal static partial int MotionTemplateCalcMotionGradient(IntPtr mhi, IntPtr mask, IntPtr orientation, double delta1, double delta2, int apertureSize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_motempl_calc_global_orientation")]
        internal static partial int MotionTemplateCalcGlobalOrientation(IntPtr orientation, IntPtr mask, IntPtr mhi, double timestamp, double duration, out double angle);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_motempl_segment_motion_count")]
        internal static partial int MotionTemplateSegmentMotionCount(IntPtr mhi, IntPtr segmask, double timestamp, double segThresh, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_motempl_segment_motion_fill")]
        internal static partial int MotionTemplateSegmentMotionFill(IntPtr mhi, IntPtr segmask, double timestamp, double segThresh, OptFlowRectNative[] rects, int rectCapacity, out int count);
    }
}
#endif
