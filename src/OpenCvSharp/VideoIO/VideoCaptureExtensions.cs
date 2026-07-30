using System;
using System.IO;

namespace OpenCvSharp.VideoIO
{
    /// <summary>
    /// Stream-oriented VideoCapture entry points.
    /// 面向流的 VideoCapture 入口。
    /// </summary>
    public static class VideoCaptureExtensions
    {
        /// <summary>
        /// Opens a managed stream with parameter pairs.
        /// 使用参数对打开托管流。
        /// </summary>
        public static bool Open(this VideoCapture capture, Stream stream, VideoCaptureAPIs apiPreference = VideoCaptureAPIs.Any, params int[] parameters)
        {
            return Open(capture, stream, apiPreference, false, parameters);
        }

        /// <summary>
        /// Opens a managed stream and optionally leaves it open after release.
        /// 打开托管流，并可选择在释放后保持流打开。
        /// </summary>
        public static bool Open(this VideoCapture capture, Stream stream, VideoCaptureAPIs apiPreference, bool leaveOpen, params int[] parameters)
        {
            if (capture == null)
            {
                throw new ArgumentNullException(nameof(capture));
            }
            return capture.OpenStreamCore(stream, apiPreference, leaveOpen, parameters ?? Array.Empty<int>());
        }

        /// <summary>
        /// Opens a previously created stream reader.
        /// 打开已有的流读取器。
        /// </summary>
        public static bool Open(this VideoCapture capture, VideoStreamReader reader, VideoCaptureAPIs apiPreference = VideoCaptureAPIs.Any, params int[] parameters)
        {
            if (capture == null)
            {
                throw new ArgumentNullException(nameof(capture));
            }
            return capture.OpenStreamReaderCore(reader, apiPreference, parameters ?? Array.Empty<int>());
        }
    }
}
