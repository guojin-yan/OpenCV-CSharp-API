namespace OpenCvSharp.Video
{
    /// <summary>Specifies the geometric motion model used by ECC registration.</summary>
    public enum MotionType
    {
        /// <summary>Two-parameter translation.</summary>
        Translation = 0,

        /// <summary>Three-parameter rigid Euclidean motion.</summary>
        Euclidean = 1,

        /// <summary>Six-parameter affine motion.</summary>
        Affine = 2,

        /// <summary>Eight-parameter projective homography.</summary>
        Homography = 3
    }
}
