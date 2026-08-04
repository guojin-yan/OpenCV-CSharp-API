namespace JYPPX.OpenCvSharp.ML
{
    /// <summary>
    /// Specifies OpenCV ML error kinds.
    /// 指定 OpenCV ML 误差类型。
    /// </summary>
    public enum MlErrorType
    {
        /// <summary>Test-set error. 测试集误差。</summary>
        TestError = 0,

        /// <summary>Training-set error. 训练集误差。</summary>
        TrainError = 1
    }
}
