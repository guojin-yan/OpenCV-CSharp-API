namespace OpenCvSharp.HighGui
{
    /// <summary>
    /// HighGUI window property flags compatible with OpenCV <c>cv::WindowPropertyFlags</c>.
    /// 与 OpenCV <c>cv::WindowPropertyFlags</c> 兼容的 HighGUI 窗口属性标志。
    /// </summary>
    public enum WindowPropertyFlags
    {
        /// <summary>Fullscreen property. 全屏属性。</summary>
        Fullscreen = 0,
        /// <summary>Autosize property. 自动尺寸属性。</summary>
        AutoSize = 1,
        /// <summary>Aspect-ratio property. 宽高比属性。</summary>
        AspectRatio = 2,
        /// <summary>OpenGL support property. OpenGL 支持属性。</summary>
        OpenGL = 3,
        /// <summary>Window visibility property. 窗口可见性属性。</summary>
        Visible = 4,
        /// <summary>Topmost window property. 窗口置顶属性。</summary>
        Topmost = 5,
        /// <summary>VSync property for OpenGL windows. OpenGL 窗口的 VSync 属性。</summary>
        VSync = 6
    }
}
