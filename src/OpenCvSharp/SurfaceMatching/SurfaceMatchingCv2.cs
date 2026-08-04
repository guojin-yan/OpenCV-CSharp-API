namespace JYPPX.OpenCvSharp.SurfaceMatching
{
    /// <summary>
    /// Factory helpers for the OpenCV surface_matching module.
    /// OpenCV surface_matching 模块的工厂辅助方法。
    /// </summary>
    public static class SurfaceMatchingCv2
    {
        /// <summary>Creates an ICP object. 创建 ICP 对象。</summary>
        public static Icp CreateIcp(
            int iterations = 250,
            float tolerance = 0.005F,
            float rejectionScale = 2.5F,
            int numLevels = 6,
            IcpSamplingType sampleType = IcpSamplingType.Uniform,
            int numMaxCorr = 1)
        {
            return Icp.Create(iterations, tolerance, rejectionScale, numLevels, sampleType, numMaxCorr);
        }

        /// <summary>Creates a PPF 3D detector. 创建 PPF 3D 检测器。</summary>
        public static Ppf3DDetector CreatePpf3DDetector(double relativeSamplingStep = 0.05, double relativeDistanceStep = 0.05, double numAngles = 30.0)
        {
            return Ppf3DDetector.Create(relativeSamplingStep, relativeDistanceStep, numAngles);
        }
    }
}
