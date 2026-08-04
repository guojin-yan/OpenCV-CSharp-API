using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Planar grid board of ArUco markers.
    /// ArUco marker 平面网格板。
    /// </summary>
    public sealed unsafe class ArucoGridBoard : IDisposable
    {
        private NativeArucoGridBoardHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes a grid board with automatically assigned marker ids.
        /// 使用自动分配的 marker id 初始化网格板。
        /// </summary>
        public ArucoGridBoard(Size gridSize, float markerLength, float markerSeparation, ArucoDictionary dictionary)
            : this(gridSize, markerLength, markerSeparation, dictionary, Array.Empty<int>())
        {
        }

        /// <summary>
        /// Initializes a grid board with explicit marker ids.
        /// 使用显式 marker id 初始化网格板。
        /// </summary>
        public ArucoGridBoard(Size gridSize, float markerLength, float markerSeparation, ArucoDictionary dictionary, int[] ids)
        {
            ValidateNotNull(dictionary, nameof(dictionary));
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            NativeException.ThrowIfError(NativeMethods.ArucoGridBoardCreate(
                gridSize.Width,
                gridSize.Height,
                markerLength,
                markerSeparation,
                dictionary.NativeHandle,
                ids,
                ids.Length,
                out IntPtr nativeHandle));
            handle = NativeArucoGridBoardHandle.FromNativePointer(nativeHandle);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Initializes a grid board with explicit marker ids from a span.
        /// 使用 Span 中的显式 marker id 初始化网格板。
        /// </summary>
        public ArucoGridBoard(Size gridSize, float markerLength, float markerSeparation, ArucoDictionary dictionary, ReadOnlySpan<int> ids)
        {
            ValidateNotNull(dictionary, nameof(dictionary));
            fixed (int* idsPtr = ids)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoGridBoardCreate(
                    gridSize.Width,
                    gridSize.Height,
                    markerLength,
                    markerSeparation,
                    dictionary.NativeHandle,
                    idsPtr,
                    ids.Length,
                    out IntPtr nativeHandle));
                handle = NativeArucoGridBoardHandle.FromNativePointer(nativeHandle);
            }
        }
#endif

        /// <summary>Gets whether this board has been disposed. 获取网格板是否已经释放。</summary>
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

        /// <summary>Gets grid size in marker counts. 获取以 marker 数量表示的网格尺寸。</summary>
        public Size GridSize
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoGridBoardGetGridSize(NativeHandle, out int markersX, out int markersY));
                return new Size(markersX, markersY);
            }
        }

        /// <summary>Gets marker side length. 获取 marker 边长。</summary>
        public float MarkerLength
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoGridBoardGetMarkerLength(NativeHandle, out float markerLength));
                return markerLength;
            }
        }

        /// <summary>Gets marker separation. 获取 marker 间距。</summary>
        public float MarkerSeparation
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoGridBoardGetMarkerSeparation(NativeHandle, out float markerSeparation));
                return markerSeparation;
            }
        }

        /// <summary>Generates a printable board image into an existing matrix. 生成可打印的 board 图像到已有矩阵。</summary>
        public void GenerateImage(Size outSize, Mat image, int marginSize = 0, int borderBits = 1)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.ArucoGridBoardGenerateImage(NativeHandle, outSize.Width, outSize.Height, image.NativeHandle, marginSize, borderBits));
        }

        /// <summary>Generates a printable board image. 生成可打印的 board 图像。</summary>
        public Mat GenerateImage(Size outSize, int marginSize = 0, int borderBits = 1)
        {
            var image = new Mat();
            GenerateImage(outSize, image, marginSize, borderBits);
            return image;
        }

        /// <summary>Releases the native board. 释放 native board。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
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
