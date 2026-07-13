using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// ArUco dictionary compatible with OpenCV <c>cv::aruco::Dictionary</c>.
    /// 与 OpenCV <c>cv::aruco::Dictionary</c> 兼容的 ArUco 字典。
    /// </summary>
    public sealed class ArucoDictionary : IDisposable
    {
        private NativeArucoDictionaryHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes an empty dictionary with OpenCV defaults.
        /// 使用 OpenCV 默认值初始化空字典。
        /// </summary>
        public ArucoDictionary()
        {
            NativeException.ThrowIfError(NativeMethods.ArucoDictionaryCreateDefault(out IntPtr nativeHandle));
            handle = NativeArucoDictionaryHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Initializes a dictionary from a bytes-list matrix.
        /// 从 bytes-list 矩阵初始化字典。
        /// </summary>
        public ArucoDictionary(Mat bytesList, int markerSize, int maxCorrectionBits = 0)
        {
            ValidateNotNull(bytesList, nameof(bytesList));
            NativeException.ThrowIfError(NativeMethods.ArucoDictionaryCreateFromBytesList(bytesList.NativeHandle, markerSize, maxCorrectionBits, out IntPtr nativeHandle));
            handle = NativeArucoDictionaryHandle.FromNativePointer(nativeHandle);
        }

        internal ArucoDictionary(IntPtr nativeHandle)
        {
            handle = NativeArucoDictionaryHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this dictionary has been disposed. 获取字典是否已经释放。</summary>
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

        /// <summary>Gets or sets marker byte list. 获取或设置 marker 字节列表。</summary>
        public Mat BytesList
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoDictionaryGetBytesList(NativeHandle, out IntPtr bytesList));
                return new Mat(bytesList);
            }

            set
            {
                ThrowIfDisposed();
                ValidateNotNull(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.ArucoDictionarySetBytesList(NativeHandle, value.NativeHandle));
            }
        }

        /// <summary>Gets or sets marker size in bits per dimension. 获取或设置 marker 每边 bit 数。</summary>
        public int MarkerSize
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoDictionaryGetMarkerSize(NativeHandle, out int markerSize));
                return markerSize;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoDictionarySetMarkerSize(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets the maximum correctable bit count. 获取或设置最大可纠正 bit 数。</summary>
        public int MaxCorrectionBits
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoDictionaryGetMaxCorrectionBits(NativeHandle, out int maxCorrectionBits));
                return maxCorrectionBits;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ArucoDictionarySetMaxCorrectionBits(NativeHandle, value));
            }
        }

        /// <summary>Creates a default dictionary. 创建默认字典。</summary>
        public static ArucoDictionary Create()
        {
            return new ArucoDictionary();
        }

        /// <summary>Gets a predefined dictionary. 获取预定义字典。</summary>
        public static ArucoDictionary GetPredefinedDictionary(PredefinedDictionaryType dictionaryType)
        {
            NativeException.ThrowIfError(NativeMethods.ArucoDictionaryCreatePredefined((int)dictionaryType, out IntPtr nativeHandle));
            return new ArucoDictionary(nativeHandle);
        }

        /// <summary>Gets a predefined dictionary by OpenCV integer id. 按 OpenCV 整数 id 获取预定义字典。</summary>
        public static ArucoDictionary GetPredefinedDictionary(int dictionaryId)
        {
            NativeException.ThrowIfError(NativeMethods.ArucoDictionaryCreatePredefined(dictionaryId, out IntPtr nativeHandle));
            return new ArucoDictionary(nativeHandle);
        }

        /// <summary>Identifies marker bits against this dictionary. 在此字典中识别 marker bits。</summary>
        public ArucoIdentificationResult Identify(Mat bits, double maxCorrectionRate)
        {
            ThrowIfDisposed();
            ValidateNotNull(bits, nameof(bits));
            NativeException.ThrowIfError(NativeMethods.ArucoDictionaryIdentify(NativeHandle, bits.NativeHandle, maxCorrectionRate, out int identified, out int index, out int rotation));
            return new ArucoIdentificationResult(identified != 0, index, rotation);
        }

        /// <summary>Identifies marker cell-pixel ratios against this dictionary. 在此字典中识别 marker cell 像素比例矩阵。</summary>
        public ArucoIdentificationResult Identify(Mat cellPixelRatio, double maxCorrectionRate, float validBitIdThreshold)
        {
            ThrowIfDisposed();
            ValidateNotNull(cellPixelRatio, nameof(cellPixelRatio));
            NativeException.ThrowIfError(NativeMethods.ArucoDictionaryIdentifyWithThreshold(NativeHandle, cellPixelRatio.NativeHandle, maxCorrectionRate, validBitIdThreshold, out int identified, out int index, out int rotation));
            return new ArucoIdentificationResult(identified != 0, index, rotation);
        }

        /// <summary>Gets Hamming distance to the specified marker id. 获取到指定 marker id 的 Hamming 距离。</summary>
        public int GetDistanceToId(Mat bits, int id, bool allRotations = true)
        {
            ThrowIfDisposed();
            ValidateNotNull(bits, nameof(bits));
            NativeException.ThrowIfError(NativeMethods.ArucoDictionaryGetDistanceToId(NativeHandle, bits.NativeHandle, id, allRotations ? 1 : 0, out int distance));
            return distance;
        }

        /// <summary>Generates a marker image into an existing matrix. 生成 marker 图像到已有矩阵。</summary>
        public void GenerateImageMarker(int id, int sidePixels, Mat image, int borderBits = 1)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.ArucoDictionaryGenerateImageMarker(NativeHandle, id, sidePixels, image.NativeHandle, borderBits));
        }

        /// <summary>Generates a marker image. 生成 marker 图像。</summary>
        public Mat GenerateImageMarker(int id, int sidePixels, int borderBits = 1)
        {
            var image = new Mat();
            GenerateImageMarker(id, sidePixels, image, borderBits);
            return image;
        }

        /// <summary>Gets marker bits for an id and rotation. 获取指定 id 和旋转的 marker bits。</summary>
        public Mat GetMarkerBits(int markerId, int rotationId = 0)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ArucoDictionaryGetMarkerBits(NativeHandle, markerId, rotationId, out IntPtr bits));
            return new Mat(bits);
        }

        /// <summary>Converts a bit matrix to a byte-list matrix. 将 bit 矩阵转换为 byte-list 矩阵。</summary>
        public static Mat GetByteListFromBits(Mat bits)
        {
            ValidateNotNull(bits, nameof(bits));
            NativeException.ThrowIfError(NativeMethods.ArucoDictionaryGetByteListFromBits(bits.NativeHandle, out IntPtr byteList));
            return new Mat(byteList);
        }

        /// <summary>Converts a byte-list matrix back to marker bits. 将 byte-list 矩阵转换回 marker bits。</summary>
        public static Mat GetBitsFromByteList(Mat byteList, int markerSize, int rotationId = 0)
        {
            ValidateNotNull(byteList, nameof(byteList));
            NativeException.ThrowIfError(NativeMethods.ArucoDictionaryGetBitsFromByteList(byteList.NativeHandle, markerSize, rotationId, out IntPtr bits));
            return new Mat(bits);
        }

        /// <summary>Releases the native dictionary. 释放 native 字典。</summary>
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
