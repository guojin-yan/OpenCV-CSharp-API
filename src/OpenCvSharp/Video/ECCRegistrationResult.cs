using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Video
{
    /// <summary>Owns an ECC registration warp matrix and its final correlation score.</summary>
    public sealed class ECCRegistrationResult : IDisposable
    {
        private bool disposed;

        internal ECCRegistrationResult(double score, Mat warpMatrix)
        {
            Score = score;
            WarpMatrix = warpMatrix ?? throw new ArgumentNullException(nameof(warpMatrix));
        }

        /// <summary>Gets the final enhanced correlation coefficient.</summary>
        public double Score { get; }

        /// <summary>Gets the independently owned output warp matrix.</summary>
        public Mat WarpMatrix { get; }

        /// <summary>Releases the owned warp matrix.</summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            WarpMatrix.Dispose();
        }
    }
}
