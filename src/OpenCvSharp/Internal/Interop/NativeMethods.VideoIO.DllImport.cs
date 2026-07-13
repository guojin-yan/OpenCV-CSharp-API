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

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_backend_name_length")]
        internal static extern int VideoIORegistryGetBackendNameLength(int api, out int length);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_get_backend_name_fill")]
        internal static extern int VideoIORegistryGetBackendNameFill(int api, byte* buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_has_backend")]
        internal static extern int VideoIORegistryHasBackend(int api, out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_videoio_registry_is_backend_built_in")]
        internal static extern int VideoIORegistryIsBackendBuiltIn(int api, out int result);
    }
}
#endif
