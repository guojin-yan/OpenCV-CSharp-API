using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Face
{
    /// <summary>
    /// Standard prediction collector for face recognizers.
    /// 人脸识别器标准预测 collector。
    /// </summary>
    public sealed class StandardCollector : IDisposable
    {
        private NativeFaceStandardCollectorHandle handle;
        private bool disposed;

        private StandardCollector(IntPtr nativeHandle)
        {
            handle = NativeFaceStandardCollectorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this collector has been disposed. 获取 collector 是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets the best label. 获取最佳标签。</summary>
        public int MinLabel
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.FaceStandardCollectorGetMinLabel(NativeHandle, out int label));
                return label;
            }
        }

        /// <summary>Gets the best distance. 获取最佳距离。</summary>
        public double MinDist
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.FaceStandardCollectorGetMinDist(NativeHandle, out double distance));
                return distance;
            }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Creates a collector with a threshold. 使用阈值创建 collector。</summary>
        public static StandardCollector Create(double threshold = double.MaxValue)
        {
            NativeException.ThrowIfError(NativeMethods.FaceStandardCollectorCreate(threshold, out IntPtr nativeHandle));
            return new StandardCollector(nativeHandle);
        }

        /// <summary>Gets collected results. 获取收集的预测结果。</summary>
        public FacePredictionResult[] GetResults(bool sorted = false)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.FaceStandardCollectorGetResultsCount(NativeHandle, sorted ? 1 : 0, out int count));
            if (count <= 0)
            {
                return Array.Empty<FacePredictionResult>();
            }

            var nativeResults = new NativeMethods.FacePredictionResultNative[count];
            NativeException.ThrowIfError(NativeMethods.FaceStandardCollectorGetResultsFill(NativeHandle, sorted ? 1 : 0, nativeResults, nativeResults.Length, out int written));
            int resultCount = Math.Max(0, Math.Min(written, nativeResults.Length));
            var results = new FacePredictionResult[resultCount];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = new FacePredictionResult(nativeResults[i].Label, nativeResults[i].Distance);
            }

            return results;
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
