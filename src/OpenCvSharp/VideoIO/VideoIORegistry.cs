using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.VideoIO
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
            return GetBackendList(NativeMethods.VideoIORegistryGetBackendsCount, NativeMethods.VideoIORegistryGetBackendsFill);
        }

        /// <summary>Gets camera-capable backends. 获取支持摄像头的后端。</summary>
        public static VideoCaptureAPIs[] GetCameraBackends()
        {
            return GetBackendList(NativeMethods.VideoIORegistryGetCameraBackendsCount, NativeMethods.VideoIORegistryGetCameraBackendsFill);
        }

        /// <summary>Gets stream-capable backends. 获取支持流读取的后端。</summary>
        public static VideoCaptureAPIs[] GetStreamBackends()
        {
            return GetBackendList(NativeMethods.VideoIORegistryGetStreamBackendsCount, NativeMethods.VideoIORegistryGetStreamBackendsFill);
        }

        /// <summary>Gets buffered stream backends. 获取支持缓冲流读取的后端。</summary>
        public static VideoCaptureAPIs[] GetStreamBufferedBackends()
        {
            return GetBackendList(NativeMethods.VideoIORegistryGetStreamBufferedBackendsCount, NativeMethods.VideoIORegistryGetStreamBufferedBackendsFill);
        }

        /// <summary>Gets writer-capable backends. 获取支持写入的后端。</summary>
        public static VideoCaptureAPIs[] GetWriterBackends()
        {
            return GetBackendList(NativeMethods.VideoIORegistryGetWriterBackendsCount, NativeMethods.VideoIORegistryGetWriterBackendsFill);
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

        /// <summary>Gets the camera plugin version. 获取摄像头插件版本。</summary>
        public static unsafe VideoIOPluginVersion GetCameraPluginVersion(VideoCaptureAPIs api)
        {
            return GetPluginVersion((int)api, NativeMethods.VideoIORegistryGetCameraPluginVersionLength, NativeMethods.VideoIORegistryGetCameraPluginVersionFill);
        }

        /// <summary>Gets the stream plugin version. 获取流插件版本。</summary>
        public static unsafe VideoIOPluginVersion GetStreamPluginVersion(VideoCaptureAPIs api)
        {
            return GetPluginVersion((int)api, NativeMethods.VideoIORegistryGetStreamPluginVersionLength, NativeMethods.VideoIORegistryGetStreamPluginVersionFill);
        }

        /// <summary>Gets the buffered stream plugin version. 获取缓冲流插件版本。</summary>
        public static unsafe VideoIOPluginVersion GetStreamBufferedPluginVersion(VideoCaptureAPIs api)
        {
            return GetPluginVersion((int)api, NativeMethods.VideoIORegistryGetStreamBufferedPluginVersionLength, NativeMethods.VideoIORegistryGetStreamBufferedPluginVersionFill);
        }

        /// <summary>Gets the writer plugin version. 获取写入插件版本。</summary>
        public static unsafe VideoIOPluginVersion GetWriterPluginVersion(VideoCaptureAPIs api)
        {
            return GetPluginVersion((int)api, NativeMethods.VideoIORegistryGetWriterPluginVersionLength, NativeMethods.VideoIORegistryGetWriterPluginVersionFill);
        }

        private delegate int BackendCountGetter(out int count);
        private delegate int BackendFillMethod(int[] backends, int capacity, out int count);

        private static VideoCaptureAPIs[] GetBackendList(BackendCountGetter getCount, BackendFillMethod fill)
        {
            NativeException.ThrowIfError(getCount(out int count));
            if (count <= 0)
            {
                return Array.Empty<VideoCaptureAPIs>();
            }

            var raw = new int[count];
            NativeException.ThrowIfError(fill(raw, raw.Length, out int written));
            int resultCount = Math.Max(0, Math.Min(written, raw.Length));
            var result = new VideoCaptureAPIs[resultCount];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (VideoCaptureAPIs)raw[i];
            }
            return result;
        }

        private unsafe delegate int PluginVersionLength(int api, out int versionAbi, out int versionApi, out int length);
        private unsafe delegate int PluginVersionFill(int api, out int versionAbi, out int versionApi, byte* buffer, int capacity, out int written);

        private static unsafe VideoIOPluginVersion GetPluginVersion(int api, PluginVersionLength getLength, PluginVersionFill fill)
        {
            NativeException.ThrowIfError(getLength(api, out int versionAbi, out int versionApi, out int length));
            if (length <= 0)
            {
                return new VideoIOPluginVersion(versionAbi, versionApi, string.Empty);
            }

            var buffer = new byte[length];
            int written;
            fixed (byte* bufferPtr = buffer)
            {
                NativeException.ThrowIfError(fill(api, out versionAbi, out versionApi, bufferPtr, buffer.Length, out written));
            }
            written = Math.Max(0, Math.Min(written, buffer.Length));
            string version = System.Text.Encoding.UTF8.GetString(buffer, 0, written);
            return new VideoIOPluginVersion(versionAbi, versionApi, version);
        }
    }
}
