using System;
using System.Collections.Generic;
using System.IO;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.VideoIO
{
    /// <summary>
    /// Video file or camera capture object compatible with OpenCV <c>cv::VideoCapture</c>.
    /// 与 OpenCV <c>cv::VideoCapture</c> 兼容的视频文件或摄像头捕获对象。
    /// </summary>
    public sealed class VideoCapture : IDisposable
    {
        private NativeVideoCaptureHandle handle;
        private VideoStreamReader? streamReader;
        private bool ownsStreamReader;
        private bool disposed;

        /// <summary>
        /// Initializes an unopened capture object.
        /// 初始化一个尚未打开的捕获对象。
        /// </summary>
        public VideoCapture()
        {
            NativeException.ThrowIfError(NativeMethods.VideoCaptureCreate(out IntPtr nativeHandle));
            handle = NativeVideoCaptureHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Initializes and opens a video file or stream.
        /// 初始化并打开视频文件或视频流。
        /// </summary>
        /// <param name="filename">The file name, image sequence pattern, or stream URL. 文件名、图像序列模式或流 URL。</param>
        /// <param name="apiPreference">The preferred backend. 首选后端。</param>
        public VideoCapture(string filename, VideoCaptureAPIs apiPreference = VideoCaptureAPIs.Any)
            : this()
        {
            Open(filename, apiPreference);
        }

        /// <summary>
        /// Initializes and opens a camera device.
        /// 初始化并打开摄像头设备。
        /// </summary>
        /// <param name="index">The camera index. 摄像头索引。</param>
        /// <param name="apiPreference">The preferred backend. 首选后端。</param>
        public VideoCapture(int index, VideoCaptureAPIs apiPreference = VideoCaptureAPIs.Any)
            : this()
        {
            Open(index, apiPreference);
        }

        /// <summary>
        /// Initializes and opens a video file with parameter pairs.
        /// 使用参数对初始化并打开视频文件。
        /// </summary>
        public VideoCapture(string filename, VideoCaptureAPIs apiPreference, params int[] parameters)
            : this()
        {
            Open(filename, apiPreference, parameters);
        }

        /// <summary>
        /// Initializes and opens a camera device with parameter pairs.
        /// 使用参数对初始化并打开摄像头设备。
        /// </summary>
        public VideoCapture(int index, VideoCaptureAPIs apiPreference, params int[] parameters)
            : this()
        {
            Open(index, apiPreference, parameters);
        }

        /// <summary>
        /// Initializes and opens a callback-backed stream reader.
        /// 初始化并打开回调流读取器。
        /// </summary>
        public VideoCapture(VideoStreamReader reader, VideoCaptureAPIs apiPreference, params int[] parameters)
            : this()
        {
            OpenStreamReaderCore(reader, apiPreference, parameters);
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
        /// Gets a value indicating whether the capture source is opened.
        /// 获取捕获源是否已经打开。
        /// </summary>
        public bool IsOpened
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.VideoCaptureIsOpened(NativeHandle, out int opened));
                return opened != 0;
            }
        }

        /// <summary>
        /// Gets or sets whether backend errors are raised as exceptions.
        /// 获取或设置是否将后端错误作为异常抛出。
        /// </summary>
        public bool ExceptionMode
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.VideoCaptureGetExceptionMode(NativeHandle, out int enabled));
                return enabled != 0;
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.VideoCaptureSetExceptionMode(NativeHandle, value ? 1 : 0));
            }
        }

        /// <summary>
        /// Gets or sets current position in milliseconds.
        /// 获取或设置当前毫秒位置。
        /// </summary>
        public double PositionMsec
        {
            get { return Get(VideoCaptureProperties.PosMsec); }
            set { Set(VideoCaptureProperties.PosMsec, value); }
        }

        /// <summary>
        /// Gets or sets current frame index.
        /// 获取或设置当前帧索引。
        /// </summary>
        public double PositionFrames
        {
            get { return Get(VideoCaptureProperties.PosFrames); }
            set { Set(VideoCaptureProperties.PosFrames, value); }
        }

        /// <summary>
        /// Gets or sets the relative position inside the video file.
        /// 获取或设置视频文件内的相对位置。
        /// </summary>
        public double PositionAviRatio
        {
            get { return Get(VideoCaptureProperties.PosAviRatio); }
            set { Set(VideoCaptureProperties.PosAviRatio, value); }
        }

        /// <summary>
        /// Gets or sets frame width.
        /// 获取或设置帧宽。
        /// </summary>
        public double FrameWidth
        {
            get { return Get(VideoCaptureProperties.FrameWidth); }
            set { Set(VideoCaptureProperties.FrameWidth, value); }
        }

        /// <summary>
        /// Gets or sets frame height.
        /// 获取或设置帧高。
        /// </summary>
        public double FrameHeight
        {
            get { return Get(VideoCaptureProperties.FrameHeight); }
            set { Set(VideoCaptureProperties.FrameHeight, value); }
        }

        /// <summary>
        /// Gets or sets frame rate.
        /// 获取或设置帧率。
        /// </summary>
        public double Fps
        {
            get { return Get(VideoCaptureProperties.Fps); }
            set { Set(VideoCaptureProperties.Fps, value); }
        }

        /// <summary>
        /// Gets or sets codec FourCC as a numeric value.
        /// 获取或设置编解码器 FourCC 数值。
        /// </summary>
        public double FourCC
        {
            get { return Get(VideoCaptureProperties.FourCC); }
            set { Set(VideoCaptureProperties.FourCC, value); }
        }

        /// <summary>
        /// Gets frame count.
        /// 获取帧数。
        /// </summary>
        public double FrameCount
        {
            get { return Get(VideoCaptureProperties.FrameCount); }
        }

        /// <summary>
        /// Gets or sets retrieved Mat format.
        /// 获取或设置检索到的 Mat 格式。
        /// </summary>
        public double Format
        {
            get { return Get(VideoCaptureProperties.Format); }
            set { Set(VideoCaptureProperties.Format, value); }
        }

        /// <summary>
        /// Gets or sets backend-specific mode.
        /// 获取或设置后端特定模式。
        /// </summary>
        public double Mode
        {
            get { return Get(VideoCaptureProperties.Mode); }
            set { Set(VideoCaptureProperties.Mode, value); }
        }

        /// <summary>
        /// Gets or sets brightness.
        /// 获取或设置亮度。
        /// </summary>
        public double Brightness
        {
            get { return Get(VideoCaptureProperties.Brightness); }
            set { Set(VideoCaptureProperties.Brightness, value); }
        }

        /// <summary>
        /// Gets or sets contrast.
        /// 获取或设置对比度。
        /// </summary>
        public double Contrast
        {
            get { return Get(VideoCaptureProperties.Contrast); }
            set { Set(VideoCaptureProperties.Contrast, value); }
        }

        /// <summary>
        /// Gets or sets saturation.
        /// 获取或设置饱和度。
        /// </summary>
        public double Saturation
        {
            get { return Get(VideoCaptureProperties.Saturation); }
            set { Set(VideoCaptureProperties.Saturation, value); }
        }

        /// <summary>
        /// Gets or sets hue.
        /// 获取或设置色调。
        /// </summary>
        public double Hue
        {
            get { return Get(VideoCaptureProperties.Hue); }
            set { Set(VideoCaptureProperties.Hue, value); }
        }

        /// <summary>
        /// Gets or sets gain.
        /// 获取或设置增益。
        /// </summary>
        public double Gain
        {
            get { return Get(VideoCaptureProperties.Gain); }
            set { Set(VideoCaptureProperties.Gain, value); }
        }

        /// <summary>
        /// Gets or sets exposure.
        /// 获取或设置曝光。
        /// </summary>
        public double Exposure
        {
            get { return Get(VideoCaptureProperties.Exposure); }
            set { Set(VideoCaptureProperties.Exposure, value); }
        }

        /// <summary>
        /// Gets or sets whether frames are converted to BGR.
        /// 获取或设置是否将帧转换为 BGR。
        /// </summary>
        public double ConvertRgb
        {
            get { return Get(VideoCaptureProperties.ConvertRgb); }
            set { Set(VideoCaptureProperties.ConvertRgb, value); }
        }

        /// <summary>
        /// Gets or sets focus.
        /// 获取或设置焦点。
        /// </summary>
        public double Focus
        {
            get { return Get(VideoCaptureProperties.Focus); }
            set { Set(VideoCaptureProperties.Focus, value); }
        }

        /// <summary>
        /// Gets current backend as a numeric value.
        /// 获取当前后端数值。
        /// </summary>
        public double Backend
        {
            get { return Get(VideoCaptureProperties.Backend); }
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
        /// Creates an unopened capture object.
        /// 创建一个尚未打开的捕获对象。
        /// </summary>
        /// <returns>The created capture object. 创建的捕获对象。</returns>
        public static VideoCapture Create()
        {
            return new VideoCapture();
        }

        /// <summary>
        /// Opens a video file, image sequence, or stream.
        /// 打开视频文件、图像序列或视频流。
        /// </summary>
        /// <param name="filename">The file name, image sequence pattern, or stream URL. 文件名、图像序列模式或流 URL。</param>
        /// <param name="apiPreference">The preferred backend. 首选后端。</param>
        /// <returns><c>true</c> if the source was opened. 如果源已打开则返回 <c>true</c>。</returns>
        public bool Open(string filename, VideoCaptureAPIs apiPreference = VideoCaptureAPIs.Any)
        {
            ThrowIfDisposed();
            ReleaseOwnedStreamReader();
            byte[] nativeFilename = VideoIOStringConvert.ToNullTerminatedUtf8(filename, nameof(filename));
            NativeException.ThrowIfError(NativeMethods.VideoCaptureOpenFile(NativeHandle, nativeFilename, (int)apiPreference, out int opened));
            return opened != 0;
        }

        /// <summary>
        /// Opens a video file with parameter pairs.
        /// 使用参数对打开视频文件。
        /// </summary>
        public bool Open(string filename, VideoCaptureAPIs apiPreference, params int[] parameters)
        {
            ThrowIfDisposed();
            int[] nativeParameters = NormalizeParameters(parameters);
            ReleaseOwnedStreamReader();
            byte[] nativeFilename = VideoIOStringConvert.ToNullTerminatedUtf8(filename, nameof(filename));
            NativeException.ThrowIfError(NativeMethods.VideoCaptureOpenFileParams(NativeHandle, nativeFilename, (int)apiPreference, nativeParameters, nativeParameters.Length, out int opened));
            return opened != 0;
        }

        /// <summary>
        /// Opens a camera device.
        /// 打开摄像头设备。
        /// </summary>
        /// <param name="index">The camera index. 摄像头索引。</param>
        /// <param name="apiPreference">The preferred backend. 首选后端。</param>
        /// <returns><c>true</c> if the device was opened. 如果设备已打开则返回 <c>true</c>。</returns>
        public bool Open(int index, VideoCaptureAPIs apiPreference = VideoCaptureAPIs.Any)
        {
            ThrowIfDisposed();
            ReleaseOwnedStreamReader();
            NativeException.ThrowIfError(NativeMethods.VideoCaptureOpenIndex(NativeHandle, index, (int)apiPreference, out int opened));
            return opened != 0;
        }

        /// <summary>
        /// Opens a camera device with parameter pairs.
        /// 使用参数对打开摄像头设备。
        /// </summary>
        public bool Open(int index, VideoCaptureAPIs apiPreference, params int[] parameters)
        {
            ThrowIfDisposed();
            int[] nativeParameters = NormalizeParameters(parameters);
            ReleaseOwnedStreamReader();
            NativeException.ThrowIfError(NativeMethods.VideoCaptureOpenIndexParams(NativeHandle, index, (int)apiPreference, nativeParameters, nativeParameters.Length, out int opened));
            return opened != 0;
        }

        /// <summary>
        /// Opens a managed stream with parameter pairs.
        /// 使用参数对打开托管流。
        /// </summary>
        internal bool OpenStreamCore(Stream stream, VideoCaptureAPIs apiPreference, bool leaveOpen, int[] parameters)
        {
            ThrowIfDisposed();
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            int[] nativeParameters = NormalizeParameters(parameters);
            ReleaseOwnedStreamReader();
            var reader = new VideoStreamReader(stream, leaveOpen);
            try
            {
                int status = NativeMethods.VideoCaptureOpenStream(NativeHandle, reader.NativeHandle, (int)apiPreference, nativeParameters, nativeParameters.Length, out int opened);
                reader.ThrowIfCallbackFailed();
                NativeException.ThrowIfError(status);
                if (opened != 0)
                {
                    streamReader = reader;
                    ownsStreamReader = true;
                    reader = null!;
                }
                return opened != 0;
            }
            finally
            {
                reader?.Dispose();
            }
        }

        /// <summary>
        /// Opens a previously created stream reader.
        /// 打开已有的流读取器。
        /// </summary>
        internal bool OpenStreamReaderCore(VideoStreamReader reader, VideoCaptureAPIs apiPreference, int[] parameters)
        {
            ThrowIfDisposed();
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }
            int[] nativeParameters = NormalizeParameters(parameters);
            ReleaseOwnedStreamReader();
            int status = NativeMethods.VideoCaptureOpenStream(NativeHandle, reader.NativeHandle, (int)apiPreference, nativeParameters, nativeParameters.Length, out int opened);
            reader.ThrowIfCallbackFailed();
            NativeException.ThrowIfError(status);
            if (opened != 0)
            {
                streamReader = reader;
                ownsStreamReader = false;
            }
            return opened != 0;
        }

        /// <summary>
        /// Releases the current capture source.
        /// 释放当前捕获源。
        /// </summary>
        public void Release()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.VideoCaptureRelease(NativeHandle));
            ReleaseOwnedStreamReader();
        }

        /// <summary>
        /// Grabs the next frame from the source.
        /// 从源抓取下一帧。
        /// </summary>
        /// <returns><c>true</c> if a frame was grabbed. 如果抓取到帧则返回 <c>true</c>。</returns>
        public bool Grab()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.VideoCaptureGrab(NativeHandle, out int grabbed));
            return grabbed != 0;
        }

        /// <summary>
        /// Retrieves a decoded frame after <see cref="Grab"/>.
        /// 在 <see cref="Grab"/> 后检索解码帧。
        /// </summary>
        /// <param name="image">The destination image. 目标图像。</param>
        /// <param name="flag">The backend-specific retrieval flag. 后端特定检索标志。</param>
        /// <returns><c>true</c> if a frame was retrieved. 如果检索到帧则返回 <c>true</c>。</returns>
        public bool Retrieve(Mat image, int flag = 0)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.VideoCaptureRetrieve(NativeHandle, image.NativeHandle, flag, out int retrieved));
            return retrieved != 0;
        }

        /// <summary>
        /// Retrieves and returns a decoded frame after <see cref="Grab"/>.
        /// 在 <see cref="Grab"/> 后检索并返回解码帧。
        /// </summary>
        /// <param name="flag">The backend-specific retrieval flag. 后端特定检索标志。</param>
        /// <returns>The retrieved frame. 检索到的帧。</returns>
        public Mat Retrieve(int flag = 0)
        {
            var image = new Mat();
            try
            {
                Retrieve(image, flag);
                return image;
            }
            catch
            {
                image.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Tries to retrieve the previously grabbed frame and returns <c>null</c> when no frame is available.
        /// </summary>
        public bool TryRetrieve(out Mat? image, int flag = 0)
        {
            var candidate = new Mat();
            try
            {
                if (Retrieve(candidate, flag))
                {
                    image = candidate;
                    return true;
                }

                candidate.Dispose();
                image = null;
                return false;
            }
            catch
            {
                candidate.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Grabs, decodes, and returns the next frame.
        /// 抓取、解码并返回下一帧。
        /// </summary>
        /// <param name="image">The destination image. 目标图像。</param>
        /// <returns><c>true</c> if a frame was read. 如果读取到帧则返回 <c>true</c>。</returns>
        public bool Read(Mat image)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.VideoCaptureRead(NativeHandle, image.NativeHandle, out int read));
            return read != 0;
        }

        /// <summary>
        /// Grabs, decodes, and returns the next frame.
        /// 抓取、解码并返回下一帧。
        /// </summary>
        /// <returns>The read frame. 读取到的帧。</returns>
        public Mat Read()
        {
            var image = new Mat();
            try
            {
                Read(image);
                return image;
            }
            catch
            {
                image.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Tries to read the next frame and returns <c>null</c> at end of stream or when no frame is available.
        /// </summary>
        public bool TryRead(out Mat? image)
        {
            var candidate = new Mat();
            try
            {
                if (Read(candidate))
                {
                    image = candidate;
                    return true;
                }

                candidate.Dispose();
                image = null;
                return false;
            }
            catch
            {
                candidate.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Gets a capture property value.
        /// 获取捕获属性值。
        /// </summary>
        /// <param name="propertyId">The property identifier. 属性标识。</param>
        /// <returns>The property value. 属性值。</returns>
        public double Get(VideoCaptureProperties propertyId)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.VideoCaptureGet(NativeHandle, (int)propertyId, out double value));
            return value;
        }

        /// <summary>
        /// Sets a capture property value.
        /// 设置捕获属性值。
        /// </summary>
        /// <param name="propertyId">The property identifier. 属性标识。</param>
        /// <param name="value">The property value. 属性值。</param>
        /// <returns><c>true</c> if the backend accepted the property. 如果后端接受该属性则返回 <c>true</c>。</returns>
        public bool Set(VideoCaptureProperties propertyId, double value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.VideoCaptureSet(NativeHandle, (int)propertyId, value, out int success));
            return success != 0;
        }

        /// <summary>
        /// Returns the backend API name used by this capture.
        /// 返回此捕获对象使用的后端 API 名称。
        /// </summary>
        /// <returns>The backend name. 后端名称。</returns>
        public string GetBackendName()
        {
            ThrowIfDisposed();
            unsafe
            {
                return NativeStringMarshaller.GetString(NativeHandle, NativeMethods.VideoCaptureBackendNameLength, NativeMethods.VideoCaptureBackendNameFill);
            }
        }

        /// <summary>
        /// Waits until one of the captures has a ready frame.
        /// 等待捕获对象中的任意一个准备好帧。
        /// </summary>
        public static bool WaitAny(IReadOnlyList<VideoCapture> streams, out int[] readyIndices, long timeoutNs = 0)
        {
            if (streams == null)
            {
                throw new ArgumentNullException(nameof(streams));
            }
            if (streams.Count == 0)
            {
                throw new ArgumentException("At least one capture is required.", nameof(streams));
            }
            var nativeCaptures = new IntPtr[streams.Count];
            for (int i = 0; i < streams.Count; i++)
            {
                if (streams[i] == null)
                {
                    throw new ArgumentException("The capture list cannot contain null entries.", nameof(streams));
                }
                nativeCaptures[i] = streams[i].NativeHandle;
            }

            var rawReadyIndices = new int[streams.Count];
            NativeException.ThrowIfError(NativeMethods.VideoCaptureWaitAny(nativeCaptures, nativeCaptures.Length, rawReadyIndices, rawReadyIndices.Length, timeoutNs, out int readyCount));
            readyCount = Math.Max(0, Math.Min(readyCount, rawReadyIndices.Length));
            readyIndices = new int[readyCount];
            Array.Copy(rawReadyIndices, readyIndices, readyCount);
            return readyCount > 0;
        }

        /// <summary>
        /// Releases the native capture object.
        /// 释放 native 捕获对象。
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

                ReleaseOwnedStreamReader();

                disposed = true;
            }
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

        private void ReleaseOwnedStreamReader()
        {
            VideoStreamReader? reader = streamReader;
            bool owns = ownsStreamReader;
            streamReader = null;
            ownsStreamReader = false;
            if (owns)
            {
                reader?.Dispose();
            }
        }
    }
}
