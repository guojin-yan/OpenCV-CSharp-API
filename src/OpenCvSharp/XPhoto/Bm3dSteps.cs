namespace JYPPX.OpenCvSharp.XPhoto
{
    /// <summary>
    /// BM3D denoising steps.
    /// BM3D 去噪步骤。
    /// </summary>
    public enum Bm3dSteps
    {
        /// <summary>Run all BM3D steps. 运行全部 BM3D 步骤。</summary>
        StepAll = 0,

        /// <summary>Run the first BM3D step only. 仅运行 BM3D 第一步。</summary>
        Step1 = 1,

        /// <summary>Run the second BM3D step only. 仅运行 BM3D 第二步。</summary>
        Step2 = 2
    }
}
