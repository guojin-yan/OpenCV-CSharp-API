using System;

namespace JYPPX.OpenCvSharp.ML
{
    /// <summary>Prediction modes used by decision-tree models.</summary>
    [Flags]
    public enum DTreesPredictionFlags
    {
        /// <summary>Selects sum or majority vote according to the trained model.</summary>
        Auto = 0,

        /// <summary>Returns the sum of individual tree responses.</summary>
        Sum = 1 << 8,

        /// <summary>Returns the class with the largest vote count.</summary>
        MaxVote = 2 << 8,

        /// <summary>Bit mask for extracting the prediction mode. This is not a prediction mode by itself.</summary>
        Mask = 3 << 8
    }

    /// <summary>Boosting algorithms supported by <see cref="Boost"/>.</summary>
    public enum BoostTypes
    {
        /// <summary>Discrete AdaBoost.</summary>
        Discrete = 0,

        /// <summary>Real AdaBoost.</summary>
        Real = 1,

        /// <summary>LogitBoost.</summary>
        Logit = 2,

        /// <summary>Gentle AdaBoost.</summary>
        Gentle = 3
    }
}
