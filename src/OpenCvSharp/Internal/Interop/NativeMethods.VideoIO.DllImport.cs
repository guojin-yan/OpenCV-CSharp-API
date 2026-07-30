#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_create")]
        internal static extern int VideoCaptureCreate(out IntPtr capture);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_release_handle")]
        internal static extern void VideoCaptureReleaseHandle(IntPtr capture);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_open_file")]
        internal static extern int VideoCaptureOpenFile(IntPtr capture, byte[] filename, int apiPreference, out int opened);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_open_index")]
        internal static extern int VideoCaptureOpenIndex(IntPtr capture, int index, int apiPreference, out int opened);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_open_file_params")]
        internal static extern int VideoCaptureOpenFileParams(IntPtr capture, byte[] filename, int apiPreference, int[] parameters, int parameterCount, out int opened);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_open_index_params")]
        internal static extern int VideoCaptureOpenIndexParams(IntPtr capture, int index, int apiPreference, int[] parameters, int parameterCount, out int opened);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_open_stream")]
        internal static extern int VideoCaptureOpenStream(IntPtr capture, IntPtr reader, int apiPreference, int[] parameters, int parameterCount, out int opened);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_is_opened")]
        internal static extern int VideoCaptureIsOpened(IntPtr capture, out int opened);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_release")]
        internal static extern int VideoCaptureRelease(IntPtr capture);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_grab")]
        internal static extern int VideoCaptureGrab(IntPtr capture, out int grabbed);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_retrieve")]
        internal static extern int VideoCaptureRetrieve(IntPtr capture, IntPtr image, int flag, out int retrieved);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_read")]
        internal static extern int VideoCaptureRead(IntPtr capture, IntPtr image, out int read);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_get")]
        internal static extern int VideoCaptureGet(IntPtr capture, int propertyId, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_set")]
        internal static extern int VideoCaptureSet(IntPtr capture, int propertyId, double value, out int success);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_set_exception_mode")]
        internal static extern int VideoCaptureSetExceptionMode(IntPtr capture, int enabled);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_get_exception_mode")]
        internal static extern int VideoCaptureGetExceptionMode(IntPtr capture, out int enabled);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_wait_any")]
        internal static extern int VideoCaptureWaitAny(IntPtr[] captures, int captureCount, int[] readyIndices, int readyCapacity, long timeoutNs, out int readyCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_backend_name_length")]
        internal static extern int VideoCaptureBackendNameLength(IntPtr capture, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_capture_backend_name_fill")]
        internal static extern int VideoCaptureBackendNameFill(IntPtr capture, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_create")]
        internal static extern int VideoWriterCreate(out IntPtr writer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_release_handle")]
        internal static extern void VideoWriterReleaseHandle(IntPtr writer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_open")]
        internal static extern int VideoWriterOpen(IntPtr writer, byte[] filename, int apiPreference, int fourcc, double fps, int frameWidth, int frameHeight, int isColor, out int opened);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_open_params")]
        internal static extern int VideoWriterOpenParams(IntPtr writer, byte[] filename, int fourcc, double fps, int frameWidth, int frameHeight, int[] parameters, int parameterCount, out int opened);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_open_api_params")]
        internal static extern int VideoWriterOpenApiParams(IntPtr writer, byte[] filename, int apiPreference, int fourcc, double fps, int frameWidth, int frameHeight, int[] parameters, int parameterCount, out int opened);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_is_opened")]
        internal static extern int VideoWriterIsOpened(IntPtr writer, out int opened);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_release")]
        internal static extern int VideoWriterRelease(IntPtr writer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_write")]
        internal static extern int VideoWriterWrite(IntPtr writer, IntPtr image, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_get")]
        internal static extern int VideoWriterGet(IntPtr writer, int propertyId, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_set")]
        internal static extern int VideoWriterSet(IntPtr writer, int propertyId, double value, out int success);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_backend_name_length")]
        internal static extern int VideoWriterBackendNameLength(IntPtr writer, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_backend_name_fill")]
        internal static extern int VideoWriterBackendNameFill(IntPtr writer, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_writer_fourcc")]
        internal static extern int VideoWriterFourcc(int c1, int c2, int c3, int c4, out int fourcc);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_backends_count")]
        internal static extern int VideoIORegistryGetBackendsCount(out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_backends_fill")]
        internal static extern int VideoIORegistryGetBackendsFill(int[] backends, int backendCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_camera_backends_count")]
        internal static extern int VideoIORegistryGetCameraBackendsCount(out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_camera_backends_fill")]
        internal static extern int VideoIORegistryGetCameraBackendsFill(int[] backends, int backendCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_backends_count")]
        internal static extern int VideoIORegistryGetStreamBackendsCount(out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_backends_fill")]
        internal static extern int VideoIORegistryGetStreamBackendsFill(int[] backends, int backendCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_buffered_backends_count")]
        internal static extern int VideoIORegistryGetStreamBufferedBackendsCount(out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_buffered_backends_fill")]
        internal static extern int VideoIORegistryGetStreamBufferedBackendsFill(int[] backends, int backendCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_writer_backends_count")]
        internal static extern int VideoIORegistryGetWriterBackendsCount(out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_writer_backends_fill")]
        internal static extern int VideoIORegistryGetWriterBackendsFill(int[] backends, int backendCapacity, out int count);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_backend_name_length")]
        internal static extern int VideoIORegistryGetBackendNameLength(int api, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_backend_name_fill")]
        internal static extern int VideoIORegistryGetBackendNameFill(int api, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_has_backend")]
        internal static extern int VideoIORegistryHasBackend(int api, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_is_backend_built_in")]
        internal static extern int VideoIORegistryIsBackendBuiltIn(int api, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_camera_plugin_version_length")]
        internal static extern int VideoIORegistryGetCameraPluginVersionLength(int api, out int versionAbi, out int versionApi, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_camera_plugin_version_fill")]
        internal static extern int VideoIORegistryGetCameraPluginVersionFill(int api, out int versionAbi, out int versionApi, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_plugin_version_length")]
        internal static extern int VideoIORegistryGetStreamPluginVersionLength(int api, out int versionAbi, out int versionApi, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_plugin_version_fill")]
        internal static extern int VideoIORegistryGetStreamPluginVersionFill(int api, out int versionAbi, out int versionApi, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_buffered_plugin_version_length")]
        internal static extern int VideoIORegistryGetStreamBufferedPluginVersionLength(int api, out int versionAbi, out int versionApi, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_stream_buffered_plugin_version_fill")]
        internal static extern int VideoIORegistryGetStreamBufferedPluginVersionFill(int api, out int versionAbi, out int versionApi, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_writer_plugin_version_length")]
        internal static extern int VideoIORegistryGetWriterPluginVersionLength(int api, out int versionAbi, out int versionApi, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_writer_plugin_version_fill")]
        internal static extern int VideoIORegistryGetWriterPluginVersionFill(int api, out int versionAbi, out int versionApi, byte* buffer, int bufferCapacity, out int written);

        internal delegate long VideoStreamReaderReadCallback(IntPtr context, IntPtr buffer, long size);
        internal delegate long VideoStreamReaderSeekCallback(IntPtr context, long offset, int origin);
        internal delegate void VideoStreamReaderReleaseCallback(IntPtr context);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_stream_reader_create")]
        internal static extern int VideoStreamReaderCreate(IntPtr context, VideoStreamReaderReadCallback readCallback, VideoStreamReaderSeekCallback seekCallback, VideoStreamReaderReleaseCallback? releaseCallback, out IntPtr reader);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_stream_reader_read")]
        internal static extern int VideoStreamReaderRead(IntPtr reader, IntPtr buffer, long size, out long bytesRead);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_stream_reader_seek")]
        internal static extern int VideoStreamReaderSeek(IntPtr reader, long offset, int origin, out long position);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_video_stream_reader_release_handle")]
        internal static extern void VideoStreamReaderReleaseHandle(IntPtr reader);
    }
}
#endif
