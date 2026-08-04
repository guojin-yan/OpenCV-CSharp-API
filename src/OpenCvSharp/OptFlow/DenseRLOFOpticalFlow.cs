using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.OptFlow
{
    /// <summary>
    /// Dense RLOF optical flow algorithm.
    /// 密集 RLOF 光流算法。
    /// </summary>
    public sealed class DenseRLOFOpticalFlow : DenseOpticalFlow
    {
        private const int IntInterpolation = 0;
        private const int IntEpicK = 1;
        private const int IntUsePostProc = 2;
        private const int IntUseVariationalRefinement = 3;
        private const int IntRicSpSize = 4;
        private const int IntRicSlicType = 5;

        private const int FloatForwardBackward = 0;
        private const int FloatEpicSigma = 1;
        private const int FloatEpicLambda = 2;
        private const int FloatFgsLambda = 3;
        private const int FloatFgsSigma = 4;

        private DenseRLOFOpticalFlow(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets the forward-backward threshold. 获取或设置前后向一致性阈值。</summary>
        public float ForwardBackward { get { return GetFloat(FloatForwardBackward); } set { SetFloat(FloatForwardBackward, value); } }

        /// <summary>Gets or sets grid step. 获取或设置网格步长。</summary>
        public Size GridStep
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.OptFlowDenseRlofGetGridStep(NativeHandle, out int width, out int height));
                return new Size(width, height);
            }

            set { NativeException.ThrowIfError(NativeMethods.OptFlowDenseRlofSetGridStep(NativeHandle, value.Width, value.Height)); }
        }

        /// <summary>Gets or sets interpolation type. 获取或设置插值类型。</summary>
        public OptFlowInterpolationType Interpolation
        {
            get { return (OptFlowInterpolationType)GetInt(IntInterpolation); }
            set
            {
                ThrowIfDisposed();
                OptFlowCv2.ValidateInterpolationType(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.OptFlowDenseRlofSetInt(NativeHandle, IntInterpolation, (int)value));
            }
        }

        /// <summary>Gets or sets EPIC K. 获取或设置 EPIC K。</summary>
        public int EPICK { get { return GetInt(IntEpicK); } set { SetInt(IntEpicK, value); } }

        /// <summary>Gets or sets EPIC sigma. 获取或设置 EPIC sigma。</summary>
        public float EPICSigma { get { return GetFloat(FloatEpicSigma); } set { SetFloat(FloatEpicSigma, value); } }

        /// <summary>Gets or sets EPIC lambda. 获取或设置 EPIC lambda。</summary>
        public float EPICLambda { get { return GetFloat(FloatEpicLambda); } set { SetFloat(FloatEpicLambda, value); } }

        /// <summary>Gets or sets FGS lambda. 获取或设置 FGS lambda。</summary>
        public float FgsLambda { get { return GetFloat(FloatFgsLambda); } set { SetFloat(FloatFgsLambda, value); } }

        /// <summary>Gets or sets FGS sigma. 获取或设置 FGS sigma。</summary>
        public float FgsSigma { get { return GetFloat(FloatFgsSigma); } set { SetFloat(FloatFgsSigma, value); } }

        /// <summary>Gets or sets whether post-processing is used. 获取或设置是否使用后处理。</summary>
        public bool UsePostProc { get { return GetInt(IntUsePostProc) != 0; } set { SetInt(IntUsePostProc, value ? 1 : 0); } }

        /// <summary>Gets or sets whether variational refinement is used. 获取或设置是否使用变分细化。</summary>
        public bool UseVariationalRefinement { get { return GetInt(IntUseVariationalRefinement) != 0; } set { SetInt(IntUseVariationalRefinement, value ? 1 : 0); } }

        /// <summary>Gets or sets RIC superpixel size. 获取或设置 RIC 超像素尺寸。</summary>
        public int RICSPSize { get { return GetInt(IntRicSpSize); } set { SetInt(IntRicSpSize, value); } }

        /// <summary>Gets or sets RIC SLIC type. 获取或设置 RIC SLIC 类型。</summary>
        public int RICSLICType { get { return GetInt(IntRicSlicType); } set { SetInt(IntRicSlicType, value); } }

        /// <summary>Creates a Dense RLOF optical flow instance. 创建 Dense RLOF 光流实例。</summary>
        public static DenseRLOFOpticalFlow Create(
            RLOFOpticalFlowParameter? parameter = null,
            float forwardBackwardThreshold = 1.0F,
            Size? gridStep = null,
            OptFlowInterpolationType interpolation = OptFlowInterpolationType.Epic,
            int epicK = 128,
            float epicSigma = 0.05F,
            float epicLambda = 999.0F,
            int ricSpSize = 15,
            int ricSlicType = 100,
            bool usePostProc = true,
            float fgsLambda = 500.0F,
            float fgsSigma = 1.5F,
            bool useVariationalRefinement = false)
        {
            OptFlowCv2.ValidateInterpolationType(interpolation, nameof(interpolation));
            Size step = gridStep ?? new Size(6, 6);
            NativeException.ThrowIfError(NativeMethods.OptFlowDenseRlofCreate(
                RLOFOpticalFlowParameter.OptionalHandle(parameter),
                forwardBackwardThreshold,
                step.Width,
                step.Height,
                (int)interpolation,
                epicK,
                epicSigma,
                epicLambda,
                ricSpSize,
                ricSlicType,
                usePostProc ? 1 : 0,
                fgsLambda,
                fgsSigma,
                useVariationalRefinement ? 1 : 0,
                out IntPtr nativeHandle));
            return new DenseRLOFOpticalFlow(nativeHandle);
        }

        /// <summary>Gets a copy of the RLOF parameter object. 获取 RLOF 参数对象副本。</summary>
        public RLOFOpticalFlowParameter GetRLOFOpticalFlowParameter()
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowDenseRlofGetParameter(NativeHandle, out IntPtr nativeHandle));
            return new RLOFOpticalFlowParameter(nativeHandle);
        }

        /// <summary>Sets the RLOF parameter object. 设置 RLOF 参数对象。</summary>
        public void SetRLOFOpticalFlowParameter(RLOFOpticalFlowParameter parameter)
        {
            DenseOpticalFlow.ValidateNotNull(parameter, nameof(parameter));
            NativeException.ThrowIfError(NativeMethods.OptFlowDenseRlofSetParameter(NativeHandle, parameter.NativeHandle));
        }

        private int GetInt(int propertyId)
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowDenseRlofGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowDenseRlofSetInt(NativeHandle, propertyId, value));
        }

        private float GetFloat(int propertyId)
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowDenseRlofGetFloat(NativeHandle, propertyId, out float value));
            return value;
        }

        private void SetFloat(int propertyId, float value)
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowDenseRlofSetFloat(NativeHandle, propertyId, value));
        }
    }
}
