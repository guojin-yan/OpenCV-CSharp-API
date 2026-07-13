using System;

namespace OpenCvSharp.ML
{
    /// <summary>
    /// Specifies flags used by OpenCV ML statistical models.
    /// 指定 OpenCV ML 统计模型使用的标志。
    /// </summary>
    [Flags]
    public enum StatModelFlags
    {
        /// <summary>No flags. 无标志。</summary>
        None = 0,

        /// <summary>Update an existing model where supported. 在支持时更新已有模型。</summary>
        UpdateModel = 1,

        /// <summary>Return raw model output. 返回模型原始输出。</summary>
        RawOutput = 1,

        /// <summary>Input is compressed. 输入已经压缩。</summary>
        CompressedInput = 2,

        /// <summary>Input is preprocessed. 输入已经预处理。</summary>
        PreprocessedInput = 4
    }
}
