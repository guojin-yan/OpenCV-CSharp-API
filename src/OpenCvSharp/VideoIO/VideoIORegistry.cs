using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.VideoIO
{
    /// <summary>
    /// Query helpers for OpenCV VideoIO backend registry.
    /// OpenCV VideoIO 后端注册表查询工具。
    /// </summary>
    public static class VideoIORegistry
    {
        /// <summary>
        /// Gets all available VideoIO backend IDs.
        /// 获取所有可用 VideoIO 后端 ID。
        /// </summary>
        public static VideoCaptureAPIs[] GetBackends()
        {
            NativeException.ThrowIfError(NativeMethods.VideoIORegistryGetBackendsCount(out int count));
            if (count <= 0)
            {
                return Array.Empty<VideoCaptureAPIs>();
            }

            var raw = new int[count];
            NativeException.ThrowIfError(NativeMethods.VideoIORegistryGetBackendsFill(raw, raw.Length, out int written));
            int resultCount = Math.Max(0, Math.Min(written, raw.Length));
            var result = new VideoCaptureAPIs[resultCount];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (VideoCaptureAPIs)raw[i];
            }

            return result;
        }

        /// <summary>
        /// Gets a backend name such as <c>FFMPEG</c> or <c>MSMF</c>.
        /// 获取后端名称，例如 <c>FFMPEG</c> 或 <c>MSMF</c>。
        /// </summary>
        public static string GetBackendName(VideoCaptureAPIs api)
        {
            unsafe
            {
                return NativeStringMarshaller.GetString(
                    (int)api,
                    NativeMethods.VideoIORegistryGetBackendNameLength,
                    NativeMethods.VideoIORegistryGetBackendNameFill);
            }
        }

        /// <summary>
        /// Returns whether the backend is available.
        /// 返回指定后端是否可用。
        /// </summary>
        public static bool HasBackend(VideoCaptureAPIs api)
        {
            NativeException.ThrowIfError(NativeMethods.VideoIORegistryHasBackend((int)api, out int result));
            return result != 0;
        }

        /// <summary>
        /// Returns whether the backend is built into OpenCV rather than loaded as a plugin.
        /// 返回指定后端是否内建于 OpenCV，而不是作为插件加载。
        /// </summary>
        public static bool IsBackendBuiltIn(VideoCaptureAPIs api)
        {
            NativeException.ThrowIfError(NativeMethods.VideoIORegistryIsBackendBuiltIn((int)api, out int result));
            return result != 0;
        }
    }
}
