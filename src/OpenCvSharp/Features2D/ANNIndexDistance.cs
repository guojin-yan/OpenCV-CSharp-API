namespace OpenCvSharp.Features2D
{
    /// <summary>Specifies the metric used by <see cref="ANNIndex"/>.</summary>
    public enum ANNIndexDistance
    {
        /// <summary>Euclidean distance over <c>CV_32FC1</c> feature rows.</summary>
        Euclidean = 0,

        /// <summary>Manhattan distance over <c>CV_32FC1</c> feature rows.</summary>
        Manhattan = 1,

        /// <summary>Angular distance over <c>CV_32FC1</c> feature rows.</summary>
        Angular = 2,

        /// <summary>Hamming distance over <c>CV_8UC1</c> feature rows.</summary>
        Hamming = 3,

        /// <summary>Dot-product distance over <c>CV_32FC1</c> feature rows.</summary>
        DotProduct = 4
    }
}
