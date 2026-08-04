using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.OptFlow
{
    /// <summary>
    /// Parameter object for robust local optical flow.
    /// 鲁棒局部光流参数对象。
    /// </summary>
    public sealed class RLOFOpticalFlowParameter : IDisposable
    {
        private const int IntSolverType = 0;
        private const int IntSupportRegionType = 1;
        private const int IntSmallWinSize = 2;
        private const int IntLargeWinSize = 3;
        private const int IntCrossSegmentationThreshold = 4;
        private const int IntMaxLevel = 5;
        private const int IntUseInitialFlow = 6;
        private const int IntUseIlluminationModel = 7;
        private const int IntUseGlobalMotionPrior = 8;
        private const int IntMaxIteration = 9;

        private const int FloatNormSigma0 = 0;
        private const int FloatNormSigma1 = 1;
        private const int FloatMinEigenValue = 2;
        private const int FloatGlobalMotionRansacThreshold = 3;

        private NativeOptFlowRlofParameterHandle handle;
        private bool disposed;

        internal RLOFOpticalFlowParameter(IntPtr nativeHandle)
        {
            handle = NativeOptFlowRlofParameterHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets or sets the solver type. 获取或设置求解器类型。</summary>
        public OptFlowSolverType SolverType
        {
            get { return (OptFlowSolverType)GetInt(IntSolverType); }
            set
            {
                ThrowIfDisposed();
                OptFlowCv2.ValidateSolverType(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.OptFlowRlofParameterSetInt(NativeHandle, IntSolverType, (int)value));
            }
        }

        /// <summary>Gets or sets the support region type. 获取或设置支持区域类型。</summary>
        public OptFlowSupportRegionType SupportRegionType
        {
            get { return (OptFlowSupportRegionType)GetInt(IntSupportRegionType); }
            set
            {
                ThrowIfDisposed();
                OptFlowCv2.ValidateSupportRegionType(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.OptFlowRlofParameterSetInt(NativeHandle, IntSupportRegionType, (int)value));
            }
        }

        /// <summary>Gets or sets norm sigma 0. 获取或设置 norm sigma 0。</summary>
        public float NormSigma0 { get { return GetFloat(FloatNormSigma0); } set { SetFloat(FloatNormSigma0, value); } }

        /// <summary>Gets or sets norm sigma 1. 获取或设置 norm sigma 1。</summary>
        public float NormSigma1 { get { return GetFloat(FloatNormSigma1); } set { SetFloat(FloatNormSigma1, value); } }

        /// <summary>Gets or sets the small window size. 获取或设置小窗口尺寸。</summary>
        public int SmallWinSize { get { return GetInt(IntSmallWinSize); } set { SetInt(IntSmallWinSize, value); } }

        /// <summary>Gets or sets the large window size. 获取或设置大窗口尺寸。</summary>
        public int LargeWinSize { get { return GetInt(IntLargeWinSize); } set { SetInt(IntLargeWinSize, value); } }

        /// <summary>Gets or sets the cross segmentation threshold. 获取或设置十字分割阈值。</summary>
        public int CrossSegmentationThreshold { get { return GetInt(IntCrossSegmentationThreshold); } set { SetInt(IntCrossSegmentationThreshold, value); } }

        /// <summary>Gets or sets max pyramid level. 获取或设置最大金字塔层级。</summary>
        public int MaxLevel { get { return GetInt(IntMaxLevel); } set { SetInt(IntMaxLevel, value); } }

        /// <summary>Gets or sets whether initial flow is used. 获取或设置是否使用初始光流。</summary>
        public bool UseInitialFlow { get { return GetInt(IntUseInitialFlow) != 0; } set { SetInt(IntUseInitialFlow, value ? 1 : 0); } }

        /// <summary>Gets or sets whether illumination model is used. 获取或设置是否使用光照模型。</summary>
        public bool UseIlluminationModel { get { return GetInt(IntUseIlluminationModel) != 0; } set { SetInt(IntUseIlluminationModel, value ? 1 : 0); } }

        /// <summary>Gets or sets whether global motion prior is used. 获取或设置是否使用全局运动先验。</summary>
        public bool UseGlobalMotionPrior { get { return GetInt(IntUseGlobalMotionPrior) != 0; } set { SetInt(IntUseGlobalMotionPrior, value ? 1 : 0); } }

        /// <summary>Gets or sets max iteration count. 获取或设置最大迭代次数。</summary>
        public int MaxIteration { get { return GetInt(IntMaxIteration); } set { SetInt(IntMaxIteration, value); } }

        /// <summary>Gets or sets the min eigenvalue threshold. 获取或设置最小特征值阈值。</summary>
        public float MinEigenValue { get { return GetFloat(FloatMinEigenValue); } set { SetFloat(FloatMinEigenValue, value); } }

        /// <summary>Gets or sets global motion RANSAC threshold. 获取或设置全局运动 RANSAC 阈值。</summary>
        public float GlobalMotionRansacThreshold { get { return GetFloat(FloatGlobalMotionRansacThreshold); } set { SetFloat(FloatGlobalMotionRansacThreshold, value); } }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Creates a default parameter object. 创建默认参数对象。</summary>
        public static RLOFOpticalFlowParameter Create()
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowRlofParameterCreate(out IntPtr nativeHandle));
            return new RLOFOpticalFlowParameter(nativeHandle);
        }

        /// <summary>Enables or disables the M-estimator preset. 启用或禁用 M-estimator 预设。</summary>
        public void SetUseMEstimator(bool value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.OptFlowRlofParameterSetUseMEstimator(NativeHandle, value ? 1 : 0));
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        internal static IntPtr OptionalHandle(RLOFOpticalFlowParameter? parameter)
        {
            return parameter == null ? IntPtr.Zero : parameter.NativeHandle;
        }

        private int GetInt(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.OptFlowRlofParameterGetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.OptFlowRlofParameterSetInt(NativeHandle, propertyId, value));
        }

        private float GetFloat(int propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.OptFlowRlofParameterGetFloat(NativeHandle, propertyId, out float value));
            return value;
        }

        private void SetFloat(int propertyId, float value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.OptFlowRlofParameterSetFloat(NativeHandle, propertyId, value));
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(RLOFOpticalFlowParameter));
            }
        }
    }
}
