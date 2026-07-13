using System;

namespace OpenCvSharp.Core
{
    /// <summary>
    /// Specifies matrix decomposition methods compatible with OpenCV <c>cv::DecompTypes</c>.
    /// 指定与 OpenCV <c>cv::DecompTypes</c> 兼容的矩阵分解方式。
    /// </summary>
    [Flags]
    public enum DecompTypes
    {
        /// <summary>
        /// Gaussian elimination with pivoting, equivalent to <c>cv::DECOMP_LU</c>.
        /// 带主元选择的高斯消元，等价于 <c>cv::DECOMP_LU</c>。
        /// </summary>
        LU = 0,

        /// <summary>
        /// Singular value decomposition, equivalent to <c>cv::DECOMP_SVD</c>.
        /// 奇异值分解，等价于 <c>cv::DECOMP_SVD</c>。
        /// </summary>
        SVD = 1,

        /// <summary>
        /// Eigenvalue decomposition, equivalent to <c>cv::DECOMP_EIG</c>.
        /// 特征值分解，等价于 <c>cv::DECOMP_EIG</c>。
        /// </summary>
        EIG = 2,

        /// <summary>
        /// Cholesky decomposition, equivalent to <c>cv::DECOMP_CHOLESKY</c>.
        /// Cholesky 分解，等价于 <c>cv::DECOMP_CHOLESKY</c>。
        /// </summary>
        Cholesky = 3,

        /// <summary>
        /// QR decomposition, equivalent to <c>cv::DECOMP_QR</c>.
        /// QR 分解，等价于 <c>cv::DECOMP_QR</c>。
        /// </summary>
        QR = 4,

        /// <summary>
        /// Solves normal equations, equivalent to <c>cv::DECOMP_NORMAL</c>.
        /// 求解正规方程，等价于 <c>cv::DECOMP_NORMAL</c>。
        /// </summary>
        Normal = 16
    }
}
