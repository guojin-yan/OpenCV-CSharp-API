namespace OpenCvSharp.Dnn
{
    /// <summary>
    /// DNN engine selection for OpenCV model import.
    /// OpenCV 模型导入的 DNN engine 选择。
    /// </summary>
    public enum DnnEngine
    {
        /// <summary>OpenCV auto selection. OpenCV 自动选择。</summary>
        Auto = 0,
        /// <summary>Classic DNN engine. 经典 DNN engine。</summary>
        Classic = 1,
        /// <summary>New DNN engine. 新 DNN engine。</summary>
        New = 2
    }
}
