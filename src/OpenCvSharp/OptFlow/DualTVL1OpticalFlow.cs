using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.OptFlow
{
    /// <summary>
    /// Dual TV-L1 dense optical flow algorithm.
    /// Dual TV-L1 密集光流算法。
    /// </summary>
    public sealed class DualTVL1OpticalFlow : DenseOpticalFlow
    {
        private const int IntScalesNumber = 0;
        private const int IntWarpingsNumber = 1;
        private const int IntInnerIterations = 2;
        private const int IntOuterIterations = 3;
        private const int IntUseInitialFlow = 4;
        private const int IntMedianFiltering = 5;

        private const int DoubleTau = 0;
        private const int DoubleLambda = 1;
        private const int DoubleTheta = 2;
        private const int DoubleGamma = 3;
        private const int DoubleEpsilon = 4;
        private const int DoubleScaleStep = 5;

        private DualTVL1OpticalFlow(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets tau. 获取或设置 tau。</summary>
        public double Tau { get { return GetDouble(DoubleTau); } set { SetDouble(DoubleTau, value); } }

        /// <summary>Gets or sets lambda. 获取或设置 lambda。</summary>
        public double Lambda { get { return GetDouble(DoubleLambda); } set { SetDouble(DoubleLambda, value); } }

        /// <summary>Gets or sets theta. 获取或设置 theta。</summary>
        public double Theta { get { return GetDouble(DoubleTheta); } set { SetDouble(DoubleTheta, value); } }

        /// <summary>Gets or sets gamma. 获取或设置 gamma。</summary>
        public double Gamma { get { return GetDouble(DoubleGamma); } set { SetDouble(DoubleGamma, value); } }

        /// <summary>Gets or sets the epsilon threshold. 获取或设置 epsilon 阈值。</summary>
        public double Epsilon { get { return GetDouble(DoubleEpsilon); } set { SetDouble(DoubleEpsilon, value); } }

        /// <summary>Gets or sets scale step. 获取或设置尺度步长。</summary>
        public double ScaleStep { get { return GetDouble(DoubleScaleStep); } set { SetDouble(DoubleScaleStep, value); } }

        /// <summary>Gets or sets number of scales. 获取或设置尺度数量。</summary>
        public int ScalesNumber { get { return GetInt(IntScalesNumber); } set { SetInt(IntScalesNumber, value); } }

        /// <summary>Gets or sets number of warpings. 获取或设置 warp 次数。</summary>
        public int WarpingsNumber { get { return GetInt(IntWarpingsNumber); } set { SetInt(IntWarpingsNumber, value); } }

        /// <summary>Gets or sets inner iterations. 获取或设置内层迭代次数。</summary>
        public int InnerIterations { get { return GetInt(IntInnerIterations); } set { SetInt(IntInnerIterations, value); } }

        /// <summary>Gets or sets outer iterations. 获取或设置外层迭代次数。</summary>
        public int OuterIterations { get { return GetInt(IntOuterIterations); } set { SetInt(IntOuterIterations, value); } }

        /// <summary>Gets or sets whether initial flow is used. 获取或设置是否使用初始光流。</summary>
        public bool UseInitialFlow { get { return GetInt(IntUseInitialFlow) != 0; } set { SetInt(IntUseInitialFlow, value ? 1 : 0); } }

        /// <summary>Gets or sets median filtering size. 获取或设置中值滤波大小。</summary>
        public int MedianFiltering { get { return GetInt(IntMedianFiltering); } set { SetInt(IntMedianFiltering, value); } }

        /// <summary>Creates a Dual TV-L1 optical flow instance. 创建 Dual TV-L1 光流实例。</summary>
        public static DualTVL1OpticalFlow Create(
            double tau = 0.25,
            double lambda = 0.15,
            double theta = 0.3,
            int nscales = 5,
            int warps = 5,
            double epsilon = 0.01,
            int innerIterations = 30,
            int outerIterations = 10,
            double scaleStep = 0.8,
            double gamma = 0.0,
            int medianFiltering = 5,
            bool useInitialFlow = false)
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowDualTvl1Create(
                tau,
                lambda,
                theta,
                nscales,
                warps,
                epsilon,
                innerIterations,
                outerIterations,
                scaleStep,
                gamma,
                medianFiltering,
                useInitialFlow ? 1 : 0,
                out IntPtr nativeHandle));
            return new DualTVL1OpticalFlow(nativeHandle);
        }

        private int GetInt(int propertyId)
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowDualTvl1GetInt(NativeHandle, propertyId, out int value));
            return value;
        }

        private void SetInt(int propertyId, int value)
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowDualTvl1SetInt(NativeHandle, propertyId, value));
        }

        private double GetDouble(int propertyId)
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowDualTvl1GetDouble(NativeHandle, propertyId, out double value));
            return value;
        }

        private void SetDouble(int propertyId, double value)
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowDualTvl1SetDouble(NativeHandle, propertyId, value));
        }
    }
}
