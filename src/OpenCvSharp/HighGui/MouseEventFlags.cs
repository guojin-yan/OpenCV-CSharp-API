using System;

namespace JYPPX.OpenCvSharp.HighGui
{
    /// <summary>
    /// HighGUI mouse event flags compatible with OpenCV <c>cv::MouseEventFlags</c>.
    /// 与 OpenCV <c>cv::MouseEventFlags</c> 兼容的 HighGUI 鼠标事件标志。
    /// </summary>
    [Flags]
    public enum MouseEventFlags
    {
        /// <summary>No flag. 无标志。</summary>
        None = 0,
        /// <summary>Left button is down. 左键处于按下状态。</summary>
        LeftButton = 1,
        /// <summary>Right button is down. 右键处于按下状态。</summary>
        RightButton = 2,
        /// <summary>Middle button is down. 中键处于按下状态。</summary>
        MiddleButton = 4,
        /// <summary>Ctrl key is pressed. Ctrl 键处于按下状态。</summary>
        CtrlKey = 8,
        /// <summary>Shift key is pressed. Shift 键处于按下状态。</summary>
        ShiftKey = 16,
        /// <summary>Alt key is pressed. Alt 键处于按下状态。</summary>
        AltKey = 32
    }
}
