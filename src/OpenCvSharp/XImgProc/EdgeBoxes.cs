using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// EdgeBoxes object proposal wrapper.
    /// EdgeBoxes 目标候选框包装。
    /// </summary>
    public sealed class EdgeBoxes : IDisposable
    {
        private NativeXImgProcEdgeBoxesHandle handle;
        private bool disposed;

        private EdgeBoxes(IntPtr nativeHandle)
        {
            handle = NativeXImgProcEdgeBoxesHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this proposal generator has been disposed. 获取候选框生成器是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Gets or sets sliding-window step size. 获取或设置滑窗步长。</summary>
        public float Alpha { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetAlpha(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesSetAlpha(NativeHandle, value)); } }

        /// <summary>Gets or sets NMS threshold. 获取或设置 NMS 阈值。</summary>
        public float Beta { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetBeta(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesSetBeta(NativeHandle, value)); } }

        /// <summary>Gets or sets NMS adaptation rate. 获取或设置 NMS 自适应率。</summary>
        public float Eta { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetEta(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesSetEta(NativeHandle, value)); } }

        /// <summary>Gets or sets minimum proposal score. 获取或设置最小候选分数。</summary>
        public float MinScore { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetMinScore(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesSetMinScore(NativeHandle, value)); } }

        /// <summary>Gets or sets maximum number of boxes. 获取或设置最大候选框数量。</summary>
        public int MaxBoxes { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetMaxBoxes(NativeHandle, out int v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesSetMaxBoxes(NativeHandle, value)); } }

        /// <summary>Gets or sets edge minimum magnitude. 获取或设置边缘最小幅值。</summary>
        public float EdgeMinMag { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetEdgeMinMag(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesSetEdgeMinMag(NativeHandle, value)); } }

        /// <summary>Gets or sets edge merge threshold. 获取或设置边缘合并阈值。</summary>
        public float EdgeMergeThr { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetEdgeMergeThr(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesSetEdgeMergeThr(NativeHandle, value)); } }

        /// <summary>Gets or sets cluster minimum magnitude. 获取或设置聚类最小幅值。</summary>
        public float ClusterMinMag { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetClusterMinMag(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesSetClusterMinMag(NativeHandle, value)); } }

        /// <summary>Gets or sets maximum aspect ratio. 获取或设置最大宽高比。</summary>
        public float MaxAspectRatio { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetMaxAspectRatio(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesSetMaxAspectRatio(NativeHandle, value)); } }

        /// <summary>Gets or sets minimum box area. 获取或设置最小候选框面积。</summary>
        public float MinBoxArea { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetMinBoxArea(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesSetMinBoxArea(NativeHandle, value)); } }

        /// <summary>Gets or sets affinity sensitivity. 获取或设置亲和敏感度。</summary>
        public float Gamma { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetGamma(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesSetGamma(NativeHandle, value)); } }

        /// <summary>Gets or sets scale sensitivity. 获取或设置尺度敏感度。</summary>
        public float Kappa { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetKappa(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesSetKappa(NativeHandle, value)); } }

        /// <summary>Creates an EdgeBoxes proposal generator. 创建 EdgeBoxes 候选框生成器。</summary>
        public static EdgeBoxes Create(float alpha = 0.65F, float beta = 0.75F, float eta = 1.0F, float minScore = 0.01F, int maxBoxes = 10000, float edgeMinMag = 0.1F, float edgeMergeThr = 0.5F, float clusterMinMag = 0.5F, float maxAspectRatio = 3.0F, float minBoxArea = 1000.0F, float gamma = 2.0F, float kappa = 1.5F)
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesCreate(alpha, beta, eta, minScore, maxBoxes, edgeMinMag, edgeMergeThr, clusterMinMag, maxAspectRatio, minBoxArea, gamma, kappa, out IntPtr nativeHandle));
            return new EdgeBoxes(nativeHandle);
        }

        /// <summary>Gets proposal boxes from edge and orientation maps. 从 edge/orientation map 获取候选框。</summary>
        public EdgeBox[] GetBoundingBoxes(Mat edgeMap, Mat orientationMap)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(edgeMap, nameof(edgeMap));
            XImgProcCv2.ValidateNotNull(orientationMap, nameof(orientationMap));
            ValidateFloatDepth(edgeMap, nameof(edgeMap));
            ValidateFloatDepth(orientationMap, nameof(orientationMap));
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetBoundingBoxesCount(NativeHandle, edgeMap.NativeHandle, orientationMap.NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<EdgeBox>();
            }

            var nativeBoxes = new NativeMethods.XImgProcEdgeBoxNative[count];
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeBoxesGetBoundingBoxesFill(NativeHandle, edgeMap.NativeHandle, orientationMap.NativeHandle, nativeBoxes, nativeBoxes.Length, out int writtenCount));
            return ToEdgeBoxes(nativeBoxes, writtenCount);
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static EdgeBox[] ToEdgeBoxes(NativeMethods.XImgProcEdgeBoxNative[] values, int count)
        {
            var result = new EdgeBox[count];
            for (int i = 0; i < count; i++)
            {
                NativeMethods.XImgProcEdgeBoxNative value = values[i];
                result[i] = new EdgeBox(new Rect(value.X, value.Y, value.Width, value.Height), value.Score);
            }

            return result;
        }

        private static void ValidateFloatDepth(Mat value, string parameterName)
        {
            if (MatType.Depth(value.Type) != MatType.CV_32F)
            {
                throw new ArgumentException("EdgeBoxes maps must have CV_32F depth.", parameterName);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && handle != null)
                {
                    handle.Dispose();
                }

                disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
