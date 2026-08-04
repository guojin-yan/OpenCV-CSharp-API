using System;

namespace JYPPX.OpenCvSharp.HighGui
{
    /// <summary>
    /// HighGUI window flags compatible with OpenCV <c>cv::WindowFlags</c>.
    /// 与 OpenCV <c>cv::WindowFlags</c> 兼容的 HighGUI 窗口标志。
    /// </summary>
    [Flags]
    public enum WindowFlags
    {
        /// <summary>Resizable normal window. 可调整大小的普通窗口。</summary>
        Normal = 0,
        /// <summary>Auto-sized window. 自动适配图像尺寸的窗口。</summary>
        AutoSize = 1,
        /// <summary>OpenGL-capable window. 支持 OpenGL 的窗口。</summary>
        OpenGL = 0x00001000,
        /// <summary>Free aspect-ratio mode. 自由宽高比模式。</summary>
        FreeRatio = 0x00000100,
        /// <summary>Keep image aspect-ratio mode. 保持图像宽高比模式。</summary>
        KeepRatio = 0,
        /// <summary>Normal legacy GUI mode. 传统普通 GUI 模式。</summary>
        GuiNormal = 0x00000010,
        /// <summary>Expanded GUI mode. 扩展 GUI 模式。</summary>
        GuiExpanded = 0
    }
}
