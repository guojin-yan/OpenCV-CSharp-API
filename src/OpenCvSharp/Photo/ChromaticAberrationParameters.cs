using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Photo
{
    /// <summary>Owns chromatic-aberration calibration coefficients and their calibration metadata.</summary>
    public sealed class ChromaticAberrationParameters : IDisposable
    {
        private readonly Mat coefficients;
        private bool disposed;

        internal ChromaticAberrationParameters(Mat coefficients, Size calibrationSize, int degree)
        {
            this.coefficients = coefficients ?? throw new ArgumentNullException(nameof(coefficients));
            CalibrationSize = calibrationSize;
            Degree = degree;
        }

        /// <summary>Gets whether this parameter object has been disposed.</summary>
        public bool IsDisposed { get { return disposed; } }

        /// <summary>Gets the independently owned 4xN CV_32F coefficient matrix.</summary>
        public Mat Coefficients
        {
            get
            {
                ThrowIfDisposed();
                return coefficients;
            }
        }

        /// <summary>Gets the image size used to calibrate the coefficients.</summary>
        public Size CalibrationSize { get; }

        /// <summary>Gets the polynomial degree represented by the coefficient columns.</summary>
        public int Degree { get; }

        /// <summary>Releases the owned coefficient matrix.</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                coefficients.Dispose();
                disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ChromaticAberrationParameters));
            }
        }
    }
}
