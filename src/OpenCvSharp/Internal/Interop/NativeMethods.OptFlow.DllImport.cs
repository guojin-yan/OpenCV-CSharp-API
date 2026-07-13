#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_release_handle")]
        internal static extern void OptFlowDenseReleaseHandle(IntPtr flow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_release_handle")]
        internal static extern void OptFlowSparseReleaseHandle(IntPtr flow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_release_handle")]
        internal static extern void OptFlowRlofParameterReleaseHandle(IntPtr parameter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_calc")]
        internal static extern int OptFlowDenseCalc(IntPtr flow, IntPtr i0, IntPtr i1, IntPtr outputFlow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_collect_garbage")]
        internal static extern int OptFlowDenseCollectGarbage(IntPtr flow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_calc")]
        internal static extern int OptFlowSparseCalc(IntPtr flow, IntPtr prevImg, IntPtr nextImg, IntPtr prevPts, IntPtr nextPts, IntPtr status, IntPtr err);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dual_tvl1_create")]
        internal static extern int OptFlowDualTvl1Create(double tau, double lambdaValue, double theta, int nscales, int warps, double epsilon, int innerIterations, int outerIterations, double scaleStep, double gamma, int medianFiltering, int useInitialFlow, out IntPtr flow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dual_tvl1_get_int")]
        internal static extern int OptFlowDualTvl1GetInt(IntPtr flow, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dual_tvl1_set_int")]
        internal static extern int OptFlowDualTvl1SetInt(IntPtr flow, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dual_tvl1_get_double")]
        internal static extern int OptFlowDualTvl1GetDouble(IntPtr flow, int propertyId, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dual_tvl1_set_double")]
        internal static extern int OptFlowDualTvl1SetDouble(IntPtr flow, int propertyId, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_create")]
        internal static extern int OptFlowRlofParameterCreate(out IntPtr parameter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_get_int")]
        internal static extern int OptFlowRlofParameterGetInt(IntPtr parameter, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_set_int")]
        internal static extern int OptFlowRlofParameterSetInt(IntPtr parameter, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_get_float")]
        internal static extern int OptFlowRlofParameterGetFloat(IntPtr parameter, int propertyId, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_set_float")]
        internal static extern int OptFlowRlofParameterSetFloat(IntPtr parameter, int propertyId, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_rlof_parameter_set_use_m_estimator")]
        internal static extern int OptFlowRlofParameterSetUseMEstimator(IntPtr parameter, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_create")]
        internal static extern int OptFlowDenseRlofCreate(IntPtr parameter, float forwardBackwardThreshold, int gridWidth, int gridHeight, int interpolationType, int epicK, float epicSigma, float epicLambda, int ricSpSize, int ricSlicType, int usePostProc, float fgsLambda, float fgsSigma, int useVariationalRefinement, out IntPtr flow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_get_parameter")]
        internal static extern int OptFlowDenseRlofGetParameter(IntPtr flow, out IntPtr parameter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_set_parameter")]
        internal static extern int OptFlowDenseRlofSetParameter(IntPtr flow, IntPtr parameter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_get_int")]
        internal static extern int OptFlowDenseRlofGetInt(IntPtr flow, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_set_int")]
        internal static extern int OptFlowDenseRlofSetInt(IntPtr flow, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_get_float")]
        internal static extern int OptFlowDenseRlofGetFloat(IntPtr flow, int propertyId, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_set_float")]
        internal static extern int OptFlowDenseRlofSetFloat(IntPtr flow, int propertyId, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_get_grid_step")]
        internal static extern int OptFlowDenseRlofGetGridStep(IntPtr flow, out int width, out int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_dense_rlof_set_grid_step")]
        internal static extern int OptFlowDenseRlofSetGridStep(IntPtr flow, int width, int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_rlof_create")]
        internal static extern int OptFlowSparseRlofCreate(IntPtr parameter, float forwardBackwardThreshold, out IntPtr flow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_rlof_get_parameter")]
        internal static extern int OptFlowSparseRlofGetParameter(IntPtr flow, out IntPtr parameter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_rlof_set_parameter")]
        internal static extern int OptFlowSparseRlofSetParameter(IntPtr flow, IntPtr parameter);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_rlof_get_forward_backward")]
        internal static extern int OptFlowSparseRlofGetForwardBackward(IntPtr flow, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_sparse_rlof_set_forward_backward")]
        internal static extern int OptFlowSparseRlofSetForwardBackward(IntPtr flow, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_create_deep_flow")]
        internal static extern int OptFlowCreateDeepFlow(out IntPtr flow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_create_simple_flow")]
        internal static extern int OptFlowCreateSimpleFlow(out IntPtr flow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_create_farneback")]
        internal static extern int OptFlowCreateFarneback(out IntPtr flow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_create_sparse_to_dense")]
        internal static extern int OptFlowCreateSparseToDense(out IntPtr flow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_calc_optical_flow_sf_simple")]
        internal static extern int OptFlowCalcOpticalFlowSFSimple(IntPtr from, IntPtr to, IntPtr flow, int layers, int averagingBlockSize, int maxFlow);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_calc_optical_flow_sf")]
        internal static extern int OptFlowCalcOpticalFlowSF(IntPtr from, IntPtr to, IntPtr flow, int layers, int averagingBlockSize, int maxFlow, double sigmaDist, double sigmaColor, int postprocessWindow, double sigmaDistFix, double sigmaColorFix, double occThr, int upscaleAveragingRadius, double upscaleSigmaDist, double upscaleSigmaColor, double speedUpThr);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_calc_optical_flow_sparse_to_dense")]
        internal static extern int OptFlowCalcOpticalFlowSparseToDense(IntPtr from, IntPtr to, IntPtr flow, int gridStep, int k, float sigma, int usePostProc, float fgsLambda, float fgsSigma);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_calc_optical_flow_dense_rlof")]
        internal static extern int OptFlowCalcOpticalFlowDenseRlof(IntPtr i0, IntPtr i1, IntPtr flow, IntPtr parameter, float forwardBackwardThreshold, int gridWidth, int gridHeight, int interpolationType, int epicK, float epicSigma, float epicLambda, int ricSpSize, int ricSlicType, int usePostProc, float fgsLambda, float fgsSigma, int useVariationalRefinement);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_optflow_calc_optical_flow_sparse_rlof")]
        internal static extern int OptFlowCalcOpticalFlowSparseRlof(IntPtr prevImg, IntPtr nextImg, IntPtr prevPts, IntPtr nextPts, IntPtr status, IntPtr err, IntPtr parameter, float forwardBackwardThreshold);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_motempl_update_motion_history")]
        internal static extern int MotionTemplateUpdateMotionHistory(IntPtr silhouette, IntPtr mhi, double timestamp, double duration);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_motempl_calc_motion_gradient")]
        internal static extern int MotionTemplateCalcMotionGradient(IntPtr mhi, IntPtr mask, IntPtr orientation, double delta1, double delta2, int apertureSize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_motempl_calc_global_orientation")]
        internal static extern int MotionTemplateCalcGlobalOrientation(IntPtr orientation, IntPtr mask, IntPtr mhi, double timestamp, double duration, out double angle);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_motempl_segment_motion_count")]
        internal static extern int MotionTemplateSegmentMotionCount(IntPtr mhi, IntPtr segmask, double timestamp, double segThresh, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_motempl_segment_motion_fill")]
        internal static extern int MotionTemplateSegmentMotionFill(IntPtr mhi, IntPtr segmask, double timestamp, double segThresh, OptFlowRectNative[] rects, int rectCapacity, out int count);
    }
}
#endif
