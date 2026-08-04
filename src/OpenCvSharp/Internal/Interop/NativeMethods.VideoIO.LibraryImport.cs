#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_create")]
        internal static partial int VideoCaptureCreate(out IntPtr capture);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_release_handle")]
        internal static partial void VideoCaptureReleaseHandle(IntPtr capture);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_open_file")]
        internal static partial int VideoCaptureOpenFile(IntPtr capture, byte[] filename, int apiPreference, out int opened);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_open_index")]
        internal static partial int VideoCaptureOpenIndex(IntPtr capture, int index, int apiPreference, out int opened);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_open_file_params")]
        internal static partial int VideoCaptureOpenFileParams(IntPtr capture, byte[] filename, int apiPreference, int[] parameters, int parameterCount, out int opened);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_open_index_params")]
        internal static partial int VideoCaptureOpenIndexParams(IntPtr capture, int index, int apiPreference, int[] parameters, int parameterCount, out int opened);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_open_stream")]
        internal static partial int VideoCaptureOpenStream(IntPtr capture, IntPtr reader, int apiPreference, int[] parameters, int parameterCount, out int opened);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_is_opened")]
        internal static partial int VideoCaptureIsOpened(IntPtr capture, out int opened);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_release")]
        internal static partial int VideoCaptureRelease(IntPtr capture);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_grab")]
        internal static partial int VideoCaptureGrab(IntPtr capture, out int grabbed);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_retrieve")]
        internal static partial int VideoCaptureRetrieve(IntPtr capture, IntPtr image, int flag, out int retrieved);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_read")]
        internal static partial int VideoCaptureRead(IntPtr capture, IntPtr image, out int read);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_get")]
        internal static partial int VideoCaptureGet(IntPtr capture, int propertyId, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_set")]
        internal static partial int VideoCaptureSet(IntPtr capture, int propertyId, double value, out int success);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_set_exception_mode")]
        internal static partial int VideoCaptureSetExceptionMode(IntPtr capture, int enabled);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_get_exception_mode")]
        internal static partial int VideoCaptureGetExceptionMode(IntPtr capture, out int enabled);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_wait_any")]
        internal static partial int VideoCaptureWaitAny(IntPtr[] captures, int captureCount, int[] readyIndices, int readyCapacity, long timeoutNs, out int readyCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_backend_name_length")]
        internal static partial int VideoCaptureBackendNameLength(IntPtr capture, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_backend_name_fill")]
        internal static partial int VideoCaptureBackendNameFill(IntPtr capture, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_create")]
        internal static partial int VideoWriterCreate(out IntPtr writer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_release_handle")]
        internal static partial void VideoWriterReleaseHandle(IntPtr writer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_open")]
        internal static partial int VideoWriterOpen(IntPtr writer, byte[] filename, int apiPreference, int fourcc, double fps, int frameWidth, int frameHeight, int isColor, out int opened);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_open_params")]
        internal static partial int VideoWriterOpenParams(IntPtr writer, byte[] filename, int fourcc, double fps, int frameWidth, int frameHeight, int[] parameters, int parameterCount, out int opened);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_open_api_params")]
        internal static partial int VideoWriterOpenApiParams(IntPtr writer, byte[] filename, int apiPreference, int fourcc, double fps, int frameWidth, int frameHeight, int[] parameters, int parameterCount, out int opened);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_is_opened")]
        internal static partial int VideoWriterIsOpened(IntPtr writer, out int opened);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_release")]
        internal static partial int VideoWriterRelease(IntPtr writer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_write")]
        internal static partial int VideoWriterWrite(IntPtr writer, IntPtr image, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_get")]
        internal static partial int VideoWriterGet(IntPtr writer, int propertyId, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_set")]
        internal static partial int VideoWriterSet(IntPtr writer, int propertyId, double value, out int success);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_backend_name_length")]
        internal static partial int VideoWriterBackendNameLength(IntPtr writer, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_backend_name_fill")]
        internal static partial int VideoWriterBackendNameFill(IntPtr writer, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_fourcc")]
        internal static partial int VideoWriterFourcc(int c1, int c2, int c3, int c4, out int fourcc);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_backends_count")]
        internal static partial int VideoIORegistryGetBackendsCount(out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_backends_fill")]
        internal static partial int VideoIORegistryGetBackendsFill(int[] backends, int backendCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_camera_backends_count")]
        internal static partial int VideoIORegistryGetCameraBackendsCount(out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_camera_backends_fill")]
        internal static partial int VideoIORegistryGetCameraBackendsFill(int[] backends, int backendCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_backends_count")]
        internal static partial int VideoIORegistryGetStreamBackendsCount(out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_backends_fill")]
        internal static partial int VideoIORegistryGetStreamBackendsFill(int[] backends, int backendCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_buffered_backends_count")]
        internal static partial int VideoIORegistryGetStreamBufferedBackendsCount(out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_buffered_backends_fill")]
        internal static partial int VideoIORegistryGetStreamBufferedBackendsFill(int[] backends, int backendCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_writer_backends_count")]
        internal static partial int VideoIORegistryGetWriterBackendsCount(out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_writer_backends_fill")]
        internal static partial int VideoIORegistryGetWriterBackendsFill(int[] backends, int backendCapacity, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_backend_name_length")]
        internal static partial int VideoIORegistryGetBackendNameLength(int api, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_backend_name_fill")]
        internal static unsafe partial int VideoIORegistryGetBackendNameFill(int api, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_has_backend")]
        internal static partial int VideoIORegistryHasBackend(int api, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_is_backend_built_in")]
        internal static partial int VideoIORegistryIsBackendBuiltIn(int api, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_camera_plugin_version_length")]
        internal static partial int VideoIORegistryGetCameraPluginVersionLength(int api, out int versionAbi, out int versionApi, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_camera_plugin_version_fill")]
        internal static unsafe partial int VideoIORegistryGetCameraPluginVersionFill(int api, out int versionAbi, out int versionApi, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_plugin_version_length")]
        internal static partial int VideoIORegistryGetStreamPluginVersionLength(int api, out int versionAbi, out int versionApi, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_plugin_version_fill")]
        internal static unsafe partial int VideoIORegistryGetStreamPluginVersionFill(int api, out int versionAbi, out int versionApi, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_buffered_plugin_version_length")]
        internal static partial int VideoIORegistryGetStreamBufferedPluginVersionLength(int api, out int versionAbi, out int versionApi, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_buffered_plugin_version_fill")]
        internal static unsafe partial int VideoIORegistryGetStreamBufferedPluginVersionFill(int api, out int versionAbi, out int versionApi, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_writer_plugin_version_length")]
        internal static partial int VideoIORegistryGetWriterPluginVersionLength(int api, out int versionAbi, out int versionApi, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_writer_plugin_version_fill")]
        internal static unsafe partial int VideoIORegistryGetWriterPluginVersionFill(int api, out int versionAbi, out int versionApi, byte* buffer, int bufferCapacity, out int written);

        internal delegate long VideoStreamReaderReadCallback(IntPtr context, IntPtr buffer, long size);
        internal delegate long VideoStreamReaderSeekCallback(IntPtr context, long offset, int origin);
        internal delegate void VideoStreamReaderReleaseCallback(IntPtr context);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_stream_reader_create")]
        internal static partial int VideoStreamReaderCreate(IntPtr context, VideoStreamReaderReadCallback readCallback, VideoStreamReaderSeekCallback seekCallback, VideoStreamReaderReleaseCallback? releaseCallback, out IntPtr reader);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_stream_reader_read")]
        internal static partial int VideoStreamReaderRead(IntPtr reader, IntPtr buffer, long size, out long bytesRead);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_stream_reader_seek")]
        internal static partial int VideoStreamReaderSeek(IntPtr reader, long offset, int origin, out long position);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_stream_reader_release_handle")]
        internal static partial void VideoStreamReaderReleaseHandle(IntPtr reader);
    }
}
#endif
