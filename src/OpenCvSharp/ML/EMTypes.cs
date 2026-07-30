using System;

namespace OpenCvSharp.ML
{
    /// <summary>Covariance-matrix constraints supported by <see cref="EM"/>.</summary>
    public enum EMCovarianceMatrixTypes
    {
        /// <summary>Each covariance is a scaled identity matrix.</summary>
        Spherical = 0,

        /// <summary>Each covariance is a diagonal matrix.</summary>
        Diagonal = 1,

        /// <summary>Each covariance is a full symmetric positive-definite matrix.</summary>
        Generic = 2,

        /// <summary>The OpenCV default covariance constraint.</summary>
        Default = Diagonal
    }

    /// <summary>Likelihood and most-probable component returned by <see cref="EM.Predict2"/>.</summary>
    public readonly struct EMPredictionResult : IEquatable<EMPredictionResult>
    {
        /// <summary>Initializes an EM prediction result.</summary>
        public EMPredictionResult(double logLikelihood, int label)
        {
            LogLikelihood = logLikelihood;
            Label = label;
        }

        /// <summary>Gets the sample log-likelihood.</summary>
        public double LogLikelihood { get; }

        /// <summary>Gets the index of the most-probable mixture component.</summary>
        public int Label { get; }

        /// <summary>Determines whether two results are equal.</summary>
        public static bool operator ==(EMPredictionResult left, EMPredictionResult right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two results are different.</summary>
        public static bool operator !=(EMPredictionResult left, EMPredictionResult right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(EMPredictionResult other)
        {
            return LogLikelihood.Equals(other.LogLikelihood) && Label == other.Label;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is EMPredictionResult other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (LogLikelihood.GetHashCode() * 397) ^ Label;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{LogLikelihood=" + LogLikelihood + ",Label=" + Label + "}";
        }
    }
}
