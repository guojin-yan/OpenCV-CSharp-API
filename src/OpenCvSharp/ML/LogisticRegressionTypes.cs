namespace JYPPX.OpenCvSharp.ML
{
    /// <summary>Regularization kinds supported by <see cref="LogisticRegression"/>.</summary>
    public enum LogisticRegressionRegularizationKinds
    {
        /// <summary>Disables regularization.</summary>
        Disable = -1,

        /// <summary>Uses L1 regularization.</summary>
        L1 = 0,

        /// <summary>Uses L2 regularization.</summary>
        L2 = 1
    }

    /// <summary>Optimization methods supported by <see cref="LogisticRegression"/>.</summary>
    public enum LogisticRegressionTrainingMethods
    {
        /// <summary>Uses full-batch gradient descent.</summary>
        Batch = 0,

        /// <summary>Uses mini-batch gradient descent.</summary>
        MiniBatch = 1
    }
}
