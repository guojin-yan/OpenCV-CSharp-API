namespace OpenCvSharp.HighGui
{
    /// <summary>
    /// HighGUI mouse event types compatible with OpenCV <c>cv::MouseEventTypes</c>.
    /// 与 OpenCV <c>cv::MouseEventTypes</c> 兼容的 HighGUI 鼠标事件类型。
    /// </summary>
    public enum MouseEventTypes
    {
        /// <summary>Mouse moved. 鼠标移动。</summary>
        MouseMove = 0,
        /// <summary>Left button down. 左键按下。</summary>
        LeftButtonDown = 1,
        /// <summary>Right button down. 右键按下。</summary>
        RightButtonDown = 2,
        /// <summary>Middle button down. 中键按下。</summary>
        MiddleButtonDown = 3,
        /// <summary>Left button up. 左键释放。</summary>
        LeftButtonUp = 4,
        /// <summary>Right button up. 右键释放。</summary>
        RightButtonUp = 5,
        /// <summary>Middle button up. 中键释放。</summary>
        MiddleButtonUp = 6,
        /// <summary>Left button double click. 左键双击。</summary>
        LeftButtonDoubleClick = 7,
        /// <summary>Right button double click. 右键双击。</summary>
        RightButtonDoubleClick = 8,
        /// <summary>Middle button double click. 中键双击。</summary>
        MiddleButtonDoubleClick = 9,
        /// <summary>Vertical mouse wheel. 垂直鼠标滚轮。</summary>
        MouseWheel = 10,
        /// <summary>Horizontal mouse wheel. 水平鼠标滚轮。</summary>
        MouseHWheel = 11
    }
}
