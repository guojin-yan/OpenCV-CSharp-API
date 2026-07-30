namespace OpenCvSharp.ImgProc
{
    /// <summary>GrabCut initialization and evaluation modes. GrabCut 初始化与求值模式。</summary>
    public enum GrabCutModes
    {
        /// <summary>Initializes from a rectangle. 从矩形初始化。</summary>
        InitWithRect = 0,
        /// <summary>Initializes from a mask. 从掩码初始化。</summary>
        InitWithMask = 1,
        /// <summary>Evaluates using initialized models. 使用已初始化模型求值。</summary>
        Eval = 2,
        /// <summary>Evaluates without updating models. 求值但不更新模型。</summary>
        EvalFreezeModel = 3
    }
}
