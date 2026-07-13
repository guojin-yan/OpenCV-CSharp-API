#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_create")]
        internal static extern int Plot2dCreate(IntPtr data, out IntPtr plot);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_create_xy")]
        internal static extern int Plot2dCreateXY(IntPtr dataX, IntPtr dataY, out IntPtr plot);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_release_handle")]
        internal static extern void Plot2dReleaseHandle(IntPtr plot);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_min_x")]
        internal static extern int Plot2dSetMinX(IntPtr plot, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_min_y")]
        internal static extern int Plot2dSetMinY(IntPtr plot, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_max_x")]
        internal static extern int Plot2dSetMaxX(IntPtr plot, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_max_y")]
        internal static extern int Plot2dSetMaxY(IntPtr plot, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_plot_line_width")]
        internal static extern int Plot2dSetPlotLineWidth(IntPtr plot, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_need_plot_line")]
        internal static extern int Plot2dSetNeedPlotLine(IntPtr plot, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_plot_line_color")]
        internal static extern int Plot2dSetPlotLineColor(IntPtr plot, double v0, double v1, double v2, double v3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_plot_background_color")]
        internal static extern int Plot2dSetPlotBackgroundColor(IntPtr plot, double v0, double v1, double v2, double v3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_plot_axis_color")]
        internal static extern int Plot2dSetPlotAxisColor(IntPtr plot, double v0, double v1, double v2, double v3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_plot_grid_color")]
        internal static extern int Plot2dSetPlotGridColor(IntPtr plot, double v0, double v1, double v2, double v3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_plot_text_color")]
        internal static extern int Plot2dSetPlotTextColor(IntPtr plot, double v0, double v1, double v2, double v3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_plot_size")]
        internal static extern int Plot2dSetPlotSize(IntPtr plot, int width, int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_show_grid")]
        internal static extern int Plot2dSetShowGrid(IntPtr plot, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_show_text")]
        internal static extern int Plot2dSetShowText(IntPtr plot, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_grid_lines_number")]
        internal static extern int Plot2dSetGridLinesNumber(IntPtr plot, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_invert_orientation")]
        internal static extern int Plot2dSetInvertOrientation(IntPtr plot, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_set_point_idx_to_print")]
        internal static extern int Plot2dSetPointIdxToPrint(IntPtr plot, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_plot_2d_render")]
        internal static extern int Plot2dRender(IntPtr plot, IntPtr result);
    }
}
#endif
