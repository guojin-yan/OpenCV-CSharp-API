using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.VideoIO
{
    /// <summary>
    /// Video writer object compatible with OpenCV <c>cv::VideoWriter</c>.
    /// 与 OpenCV <c>cv::VideoWriter</c> 兼容的视频写入对象。
    /// </summary>
    public sealed class VideoWriter : IDisposable
    {
        private NativeVideoWriterHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes an unopened writer object.
        /// 初始化一个尚未打开的写入对象。
        /// </summary>
        public VideoWriter()
        {
            NativeException.ThrowIfError(NativeMethods.VideoWriterCreate(out IntPtr nativeHandle));
            handle = NativeVideoWriterHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Initializes and opens a video writer.
        /// 初始化并打开视频写入器。
        /// </summary>
        /// <param name="filename">The output file name. 输出文件名。</param>
        /// <param name="fourcc">The codec FourCC. 编解码器 FourCC。</param>
        /// <param name="fps">The output frame rate. 输出帧率。</param>
        /// <param name="frameSize">The output frame size. 输出帧尺寸。</param>
        /// <param name="isColor">Whether frames are color. 帧是否为彩色。</param>
        public VideoWriter(string filename, int fourcc, double fps, Size frameSize, bool isColor = true)
            : this()
        {
            Open(filename, fourcc, fps, frameSize, isColor);
        }

        /// <summary>
        /// Initializes and opens a video writer with a preferred backend.
        /// 使用首选后端初始化并打开视频写入器。
        /// </summary>
        /// <param name="filename">The output file name. 输出文件名。</param>
        /// <param name="apiPreference">The preferred backend. 首选后端。</param>
        /// <param name="fourcc">The codec FourCC. 编解码器 FourCC。</param>
        /// <param name="fps">The output frame rate. 输出帧率。</param>
        /// <param name="frameSize">The output frame size. 输出帧尺寸。</param>
        /// <param name="isColor">Whether frames are color. 帧是否为彩色。</param>
        public VideoWriter(string filename, VideoCaptureAPIs apiPreference, int fourcc, double fps, Size frameSize, bool isColor = true)
            : this()
        {
            Open(filename, apiPreference, fourcc, fps, frameSize, isColor);
        }

        /// <summary>
        /// Initializes and opens a video writer with parameter pairs.
        /// 使用参数对初始化并打开视频写入器。
        /// </summary>
        public VideoWriter(string filename, int fourcc, double fps, Size frameSize, params int[] parameters)
            : this()
        {
            Open(filename, fourcc, fps, frameSize, parameters);
        }

        /// <summary>
        /// Initializes and opens a video writer with a preferred backend and parameter pairs.
        /// 使用首选后端和参数对初始化并打开视频写入器。
        /// </summary>
        public VideoWriter(string filename, VideoCaptureAPIs apiPreference, int fourcc, double fps, Size frameSize, params int[] parameters)
            : this()
        {
            Open(filename, apiPreference, fourcc, fps, frameSize, parameters);
        }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// 获取此对象是否已经释放。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets a value indicating whether the writer is opened.
        /// 获取写入器是否已经打开。
        /// </summary>
        public bool IsOpened
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.VideoWriterIsOpened(NativeHandle, out int opened));
                return opened != 0;
            }
        }

        /// <summary>
        /// Gets or sets encoder quality.
        /// 获取或设置编码质量。
        /// </summary>
        public double Quality
        {
            get { return Get(VideoWriterProperties.Quality); }
            set { Set(VideoWriterProperties.Quality, value); }
        }

        /// <summary>
        /// Gets encoded frame byte count.
        /// 获取编码帧字节数。
        /// </summary>
        public double FrameBytes
        {
            get { return Get(VideoWriterProperties.FrameBytes); }
        }

        /// <summary>
        /// Gets or sets the number of stripes for parallel encoding.
        /// 获取或设置并行编码条带数。
        /// </summary>
        public double NStripes
        {
            get { return Get(VideoWriterProperties.NStripes); }
            set { Set(VideoWriterProperties.NStripes, value); }
        }

        /// <summary>
        /// Gets or sets whether frames are color.
        /// 获取或设置帧是否为彩色。
        /// </summary>
        public double IsColor
        {
            get { return Get(VideoWriterProperties.IsColor); }
            set { Set(VideoWriterProperties.IsColor, value); }
        }

        /// <summary>
        /// Gets or sets frame depth.
        /// 获取或设置帧深度。
        /// </summary>
        public double Depth
        {
            get { return Get(VideoWriterProperties.Depth); }
            set { Set(VideoWriterProperties.Depth, value); }
        }

        /// <summary>
        /// Gets or sets hardware acceleration type.
        /// 获取或设置硬件加速类型。
        /// </summary>
        public double HwAcceleration
        {
            get { return Get(VideoWriterProperties.HwAcceleration); }
            set { Set(VideoWriterProperties.HwAcceleration, value); }
        }

        /// <summary>
        /// Gets or sets hardware device index.
        /// 获取或设置硬件设备索引。
        /// </summary>
        public double HwDevice
        {
            get { return Get(VideoWriterProperties.HwDevice); }
            set { Set(VideoWriterProperties.HwDevice, value); }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>
        /// Creates an unopened writer object.
        /// 创建一个尚未打开的写入对象。
        /// </summary>
        /// <returns>The created writer object. 创建的写入对象。</returns>
        public static VideoWriter Create()
        {
            return new VideoWriter();
        }

        /// <summary>
        /// Opens a video writer.
        /// 打开视频写入器。
        /// </summary>
        /// <param name="filename">The output file name. 输出文件名。</param>
        /// <param name="fourcc">The codec FourCC. 编解码器 FourCC。</param>
        /// <param name="fps">The output frame rate. 输出帧率。</param>
        /// <param name="frameSize">The output frame size. 输出帧尺寸。</param>
        /// <param name="isColor">Whether frames are color. 帧是否为彩色。</param>
        /// <returns><c>true</c> if the writer was opened. 如果写入器已打开则返回 <c>true</c>。</returns>
        public bool Open(string filename, int fourcc, double fps, Size frameSize, bool isColor = true)
        {
            return Open(filename, VideoCaptureAPIs.Any, fourcc, fps, frameSize, isColor);
        }

        /// <summary>
        /// Opens a video writer with a preferred backend.
        /// 使用首选后端打开视频写入器。
        /// </summary>
        /// <param name="filename">The output file name. 输出文件名。</param>
        /// <param name="apiPreference">The preferred backend. 首选后端。</param>
        /// <param name="fourcc">The codec FourCC. 编解码器 FourCC。</param>
        /// <param name="fps">The output frame rate. 输出帧率。</param>
        /// <param name="frameSize">The output frame size. 输出帧尺寸。</param>
        /// <param name="isColor">Whether frames are color. 帧是否为彩色。</param>
        /// <returns><c>true</c> if the writer was opened. 如果写入器已打开则返回 <c>true</c>。</returns>
        public bool Open(string filename, VideoCaptureAPIs apiPreference, int fourcc, double fps, Size frameSize, bool isColor = true)
        {
            ThrowIfDisposed();
            if (frameSize.Width <= 0 || frameSize.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frameSize), "Frame size must be positive.");
            }

            byte[] nativeFilename = VideoIOStringConvert.ToNullTerminatedUtf8(filename, nameof(filename));
            NativeException.ThrowIfError(NativeMethods.VideoWriterOpen(
                NativeHandle,
                nativeFilename,
                (int)apiPreference,
                fourcc,
                fps,
                frameSize.Width,
                frameSize.Height,
                isColor ? 1 : 0,
                out int opened));
            return opened != 0;
        }

        /// <summary>
        /// Opens a video writer with parameter pairs.
        /// 使用参数对打开视频写入器。
        /// </summary>
        public bool Open(string filename, int fourcc, double fps, Size frameSize, params int[] parameters)
        {
            ThrowIfDisposed();
            ValidateFrameSize(frameSize);
            int[] nativeParameters = NormalizeParameters(parameters);
            byte[] nativeFilename = VideoIOStringConvert.ToNullTerminatedUtf8(filename, nameof(filename));
            NativeException.ThrowIfError(NativeMethods.VideoWriterOpenParams(
                NativeHandle,
                nativeFilename,
                fourcc,
                fps,
                frameSize.Width,
                frameSize.Height,
                nativeParameters,
                nativeParameters.Length,
                out int opened));
            return opened != 0;
        }

        /// <summary>
        /// Opens a video writer with a preferred backend and parameter pairs.
        /// 使用首选后端和参数对打开视频写入器。
        /// </summary>
        public bool Open(string filename, VideoCaptureAPIs apiPreference, int fourcc, double fps, Size frameSize, params int[] parameters)
        {
            ThrowIfDisposed();
            ValidateFrameSize(frameSize);
            int[] nativeParameters = NormalizeParameters(parameters);
            byte[] nativeFilename = VideoIOStringConvert.ToNullTerminatedUtf8(filename, nameof(filename));
            NativeException.ThrowIfError(NativeMethods.VideoWriterOpenApiParams(
                NativeHandle,
                nativeFilename,
                (int)apiPreference,
                fourcc,
                fps,
                frameSize.Width,
                frameSize.Height,
                nativeParameters,
                nativeParameters.Length,
                out int opened));
            return opened != 0;
        }

        /// <summary>
        /// Releases the current writer target.
        /// 释放当前写入目标。
        /// </summary>
        public void Release()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.VideoWriterRelease(NativeHandle));
        }

        /// <summary>
        /// Writes a frame to the video stream.
        /// 向视频流写入一帧。
        /// </summary>
        /// <param name="image">The frame to write. 要写入的帧。</param>
        /// <returns><c>true</c> if the backend reports a successful write. 如果后端报告写入成功则返回 <c>true</c>。</returns>
        public bool Write(Mat image)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.VideoWriterWrite(NativeHandle, image.NativeHandle, out int written));
            return written != 0;
        }

        /// <summary>
        /// Gets a writer property value.
        /// 获取写入器属性值。
        /// </summary>
        /// <param name="propertyId">The property identifier. 属性标识。</param>
        /// <returns>The property value. 属性值。</returns>
        public double Get(VideoWriterProperties propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.VideoWriterGet(NativeHandle, (int)propertyId, out double value));
            return value;
        }

        /// <summary>
        /// Sets a writer property value.
        /// 设置写入器属性值。
        /// </summary>
        /// <param name="propertyId">The property identifier. 属性标识。</param>
        /// <param name="value">The property value. 属性值。</param>
        /// <returns><c>true</c> if the backend accepted the property. 如果后端接受该属性则返回 <c>true</c>。</returns>
        public bool Set(VideoWriterProperties propertyId, double value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.VideoWriterSet(NativeHandle, (int)propertyId, value, out int success));
            return success != 0;
        }

        /// <summary>
        /// Returns the backend API name used by this writer.
        /// 返回此写入器使用的后端 API 名称。
        /// </summary>
        /// <returns>The backend name. 后端名称。</returns>
        public string GetBackendName()
        {
            ThrowIfDisposed();
            unsafe
            {
                return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.VideoWriterBackendNameLength, NativeMethods.VideoWriterBackendNameFill);
            }
        }

        /// <summary>
        /// Constructs a FourCC code from four characters.
        /// 根据四个字符构造 FourCC 编码。
        /// </summary>
        /// <param name="c1">The first character. 第一个字符。</param>
        /// <param name="c2">The second character. 第二个字符。</param>
        /// <param name="c3">The third character. 第三个字符。</param>
        /// <param name="c4">The fourth character. 第四个字符。</param>
        /// <returns>The FourCC code. FourCC 编码。</returns>
        public static int FourCC(char c1, char c2, char c3, char c4)
        {
            ValidateFourCCChar(c1, nameof(c1));
            ValidateFourCCChar(c2, nameof(c2));
            ValidateFourCCChar(c3, nameof(c3));
            ValidateFourCCChar(c4, nameof(c4));
            return (c1 & 255) | ((c2 & 255) << 8) | ((c3 & 255) << 16) | ((c4 & 255) << 24);
        }

        /// <summary>
        /// Constructs a FourCC code from a four-character string.
        /// 根据四字符字符串构造 FourCC 编码。
        /// </summary>
        /// <param name="fourcc">The four-character code. 四字符编码。</param>
        /// <returns>The FourCC code. FourCC 编码。</returns>
        public static int FourCC(string fourcc)
        {
            if (fourcc == null)
            {
                throw new ArgumentNullException(nameof(fourcc));
            }

            if (fourcc.Length != 4)
            {
                throw new ArgumentException("FourCC string must contain exactly four characters.", nameof(fourcc));
            }

            return FourCC(fourcc[0], fourcc[1], fourcc[2], fourcc[3]);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Constructs a FourCC code from a four-character span without allocating a string.
        /// 使用四字符 span 构造 FourCC 编码，避免分配字符串。
        /// </summary>
        /// <param name="fourcc">The four-character code. 四字符编码。</param>
        /// <returns>The FourCC code. FourCC 编码。</returns>
        public static int FourCC(ReadOnlySpan<char> fourcc)
        {
            if (fourcc.Length != 4)
            {
                throw new ArgumentException("FourCC span must contain exactly four characters.", nameof(fourcc));
            }

            return FourCC(fourcc[0], fourcc[1], fourcc[2], fourcc[3]);
        }
#endif

        /// <summary>
        /// Releases the native writer object.
        /// 释放 native 写入器对象。
        /// </summary>
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

        private static void ValidateFourCCChar(char value, string parameterName)
        {
            if (value > 255)
            {
                throw new ArgumentOutOfRangeException(parameterName, "FourCC characters must be in the byte range 0..255.");
            }
        }

        private static void ValidateFrameSize(Size frameSize)
        {
            if (frameSize.Width <= 0 || frameSize.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frameSize), "Frame size must be positive.");
            }
        }

        private static int[] NormalizeParameters(int[]? parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return Array.Empty<int>();
            }
            if ((parameters.Length & 1) != 0)
            {
                throw new ArgumentException("VideoIO parameters must contain key/value pairs.", nameof(parameters));
            }
            return parameters;
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
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
