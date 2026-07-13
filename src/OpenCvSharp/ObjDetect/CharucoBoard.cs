using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// ChArUco board combining chessboard corners and ArUco markers.
    /// 结合棋盘角点与 ArUco marker 的 ChArUco board。
    /// </summary>
    public sealed unsafe class CharucoBoard : IDisposable
    {
        private NativeArucoCharucoBoardHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes a ChArUco board with automatically assigned marker ids.
        /// 使用自动分配的 marker id 初始化 ChArUco board。
        /// </summary>
        public CharucoBoard(Size chessboardSize, float squareLength, float markerLength, ArucoDictionary dictionary)
            : this(chessboardSize, squareLength, markerLength, dictionary, Array.Empty<int>())
        {
        }

        /// <summary>
        /// Initializes a ChArUco board with explicit marker ids.
        /// 使用显式 marker id 初始化 ChArUco board。
        /// </summary>
        public CharucoBoard(Size chessboardSize, float squareLength, float markerLength, ArucoDictionary dictionary, int[] ids)
        {
            ValidateNotNull(dictionary, nameof(dictionary));
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            NativeException.ThrowIfError(NativeMethods.ArucoCharucoBoardCreate(
                chessboardSize.Width,
                chessboardSize.Height,
                squareLength,
                markerLength,
                dictionary.NativeHandle,
                ids,
                ids.Length,
                out IntPtr nativeHandle));
            handle = NativeArucoCharucoBoardHandle.FromNativePointer(nativeHandle);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Initializes a ChArUco board with explicit marker ids from a span.
        /// 使用 Span 中的显式 marker id 初始化 ChArUco board。
        /// </summary>
        public CharucoBoard(Size chessboardSize, float squareLength, float markerLength, ArucoDictionary dictionary, ReadOnlySpan<int> ids)
        {
            ValidateNotNull(dictionary, nameof(dictionary));
            fixed (int* idsPtr = ids)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoCharucoBoardCreate(
                    chessboardSize.Width,
                    chessboardSize.Height,
                    squareLength,
                    markerLength,
                    dictionary.NativeHandle,
                    idsPtr,
                    ids.Length,
                    out IntPtr nativeHandle));
                handle = NativeArucoCharucoBoardHandle.FromNativePointer(nativeHandle);
            }
        }
#endif

        internal CharucoBoard(IntPtr nativeHandle)
        {
            handle = NativeArucoCharucoBoardHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this board has been disposed. 获取 board 是否已经释放。</summary>
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

        /// <summary>Gets chessboard square count. 获取棋盘格方格数量。</summary>
        public Size ChessboardSize
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoCharucoBoardGetChessboardSize(NativeHandle, out int squaresX, out int squaresY));
                return new Size(squaresX, squaresY);
            }
        }

        /// <summary>Gets square side length. 获取棋盘格边长。</summary>
        public float SquareLength
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoCharucoBoardGetSquareLength(NativeHandle, out float squareLength));
                return squareLength;
            }
        }

        /// <summary>Gets marker side length. 获取 marker 边长。</summary>
        public float MarkerLength
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoCharucoBoardGetMarkerLength(NativeHandle, out float markerLength));
                return markerLength;
            }
        }

        /// <summary>Gets or sets whether the legacy board pattern is used. 获取或设置是否使用 legacy board pattern。</summary>
        public bool LegacyPattern
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoCharucoBoardGetLegacyPattern(NativeHandle, out int legacyPattern));
                return legacyPattern != 0;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoCharucoBoardSetLegacyPattern(NativeHandle, value ? 1 : 0));
            }
        }

        /// <summary>Gets chessboard corner coordinates in board space. 获取 board 坐标系中的棋盘角点坐标。</summary>
        public Point3f[] GetChessboardCorners()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ArucoCharucoBoardGetChessboardCornersCount(NativeHandle, out int count));
            var native = new NativeMethods.Point3fNative[Math.Max(count, 0)];
            fixed (NativeMethods.Point3fNative* nativePtr = native)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoCharucoBoardGetChessboardCornersFill(NativeHandle, nativePtr, native.Length, out count));
            }

            int safeCount = Math.Max(0, Math.Min(count, native.Length));
            var result = new Point3f[safeCount];
            for (int i = 0; i < safeCount; i++)
            {
                result[i] = new Point3f(native[i].X, native[i].Y, native[i].Z);
            }

            return result;
        }

        /// <summary>Checks whether the selected ChArUco corners are collinear. 检查指定 ChArUco 角点是否共线。</summary>
        public bool CheckCharucoCornersCollinear(int[] charucoIds)
        {
            ThrowIfDisposed();
            if (charucoIds == null)
            {
                throw new ArgumentNullException(nameof(charucoIds));
            }

            NativeException.ThrowIfError(NativeMethods.ArucoCharucoBoardCheckCornersCollinear(NativeHandle, charucoIds, charucoIds.Length, out int collinear));
            return collinear != 0;
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Checks whether the selected ChArUco corners are collinear. 检查指定 ChArUco 角点是否共线。</summary>
        public bool CheckCharucoCornersCollinear(ReadOnlySpan<int> charucoIds)
        {
            ThrowIfDisposed();
            fixed (int* idsPtr = charucoIds)
            {
                NativeException.ThrowIfError(NativeMethods.ArucoCharucoBoardCheckCornersCollinear(NativeHandle, idsPtr, charucoIds.Length, out int collinear));
                return collinear != 0;
            }
        }
#endif

        /// <summary>Generates a printable board image into an existing matrix. 生成可打印的 board 图像到已有矩阵。</summary>
        public void GenerateImage(Size outSize, Mat image, int marginSize = 0, int borderBits = 1)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.ArucoCharucoBoardGenerateImage(NativeHandle, outSize.Width, outSize.Height, image.NativeHandle, marginSize, borderBits));
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
