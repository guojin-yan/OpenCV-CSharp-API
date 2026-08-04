namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>Specifies the implementation preference selected by the OpenCV build.</summary>
    public enum AlgorithmHint
    {
        /// <summary>Uses the build-defined default behavior.</summary>
        Default = 0,

        /// <summary>Uses the generic portable implementation.</summary>
        Accurate = 1,

        /// <summary>Allows faster platform-dependent approximations.</summary>
        Approximate = 2,
    }
}
