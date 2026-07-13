using System;

namespace OpenCvSharp.HighGui
{
    /// <summary>
    /// Qt HighGUI button types compatible with OpenCV <c>cv::QtButtonTypes</c>.
    /// 与 OpenCV <c>cv::QtButtonTypes</c> 兼容的 Qt HighGUI 按钮类型。
    /// </summary>
    [Flags]
    public enum QtButtonTypes
    {
        /// <summary>Push button. 按钮。</summary>
        PushButton = 0,
        /// <summary>Checkbox button. 复选框按钮。</summary>
        Checkbox = 1,
        /// <summary>Radio button. 单选按钮。</summary>
        Radiobox = 2,
        /// <summary>Create a new button bar. 创建新的按钮栏。</summary>
        NewButtonbar = 1024
    }
}
