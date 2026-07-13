#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
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

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_backend_name_length")]
        internal static partial int VideoIORegistryGetBackendNameLength(int api, out int length);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_backend_name_fill")]
        internal static unsafe partial int VideoIORegistryGetBackendNameFill(int api, byte* buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_has_backend")]
        internal static partial int VideoIORegistryHasBackend(int api, out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_is_backend_built_in")]
        internal static partial int VideoIORegistryIsBackendBuiltIn(int api, out int result);
    }
}
#endif
