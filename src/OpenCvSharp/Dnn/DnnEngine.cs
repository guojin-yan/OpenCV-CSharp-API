namespace JYPPX.OpenCvSharp.Dnn
{
    /// <summary>
    /// DNN engine selection for OpenCV model import.
    /// OpenCV 模型导入的 DNN engine 选择。
    /// </summary>
    public enum DnnEngine
    {
        /// <summary>OpenCV auto selection. OpenCV 自动选择。</summary>
        Auto = 3,
        /// <summary>Classic DNN engine request. OpenCV 5.1 ONNX import falls back to its unified OpenCV engine. 经典 engine 请求；5.1 ONNX 导入回退至统一 OpenCV engine。</summary>
        Classic = 1,
        /// <summary>New DNN engine. 新 DNN engine。</summary>
        New = 2,
        /// <summary>ONNX Runtime engine when OpenCV was built with it. OpenCV 启用 ONNX Runtime 时使用该 engine。</summary>
        Ort = 4
    }
}
