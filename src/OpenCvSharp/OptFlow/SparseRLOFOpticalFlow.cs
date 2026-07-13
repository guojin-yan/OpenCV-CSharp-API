using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.OptFlow
{
    /// <summary>
    /// Sparse RLOF optical flow algorithm.
    /// 稀疏 RLOF 光流算法。
    /// </summary>
    public sealed class SparseRLOFOpticalFlow : SparseOpticalFlow
    {
        private SparseRLOFOpticalFlow(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets the forward-backward threshold. 获取或设置前后向一致性阈值。</summary>
        public float ForwardBackward
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.OptFlowSparseRlofGetForwardBackward(NativeHandle, out float value));
                return value;
            }

            set { NativeException.ThrowIfError(NativeMethods.OptFlowSparseRlofSetForwardBackward(NativeHandle, value)); }
        }

        /// <summary>Creates a Sparse RLOF optical flow instance. 创建 Sparse RLOF 光流实例。</summary>
        public static SparseRLOFOpticalFlow Create(RLOFOpticalFlowParameter? parameter = null, float forwardBackwardThreshold = 1.0F)
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowSparseRlofCreate(RLOFOpticalFlowParameter.OptionalHandle(parameter), forwardBackwardThreshold, out IntPtr nativeHandle));
            return new SparseRLOFOpticalFlow(nativeHandle);
        }

        /// <summary>Gets a copy of the RLOF parameter object. 获取 RLOF 参数对象副本。</summary>
        public RLOFOpticalFlowParameter GetRLOFOpticalFlowParameter()
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowSparseRlofGetParameter(NativeHandle, out IntPtr nativeHandle));
            return new RLOFOpticalFlowParameter(nativeHandle);
        }

        /// <summary>Sets the RLOF parameter object. 设置 RLOF 参数对象。</summary>
        public void SetRLOFOpticalFlowParameter(RLOFOpticalFlowParameter parameter)
        {
            DenseOpticalFlow.ValidateNotNull(parameter, nameof(parameter));
            NativeException.ThrowIfError(NativeMethods.OptFlowSparseRlofSetParameter(NativeHandle, parameter.NativeHandle));
        }
    }
}
