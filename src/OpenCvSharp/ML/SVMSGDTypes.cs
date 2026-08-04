namespace JYPPX.OpenCvSharp.ML
{
    /// <summary>Optimization variants supported by <see cref="SVMSGD"/>.</summary>
    public enum SVMSGDTypes
    {
        /// <summary>Classic stochastic gradient descent.</summary>
        Sgd = 0,

        /// <summary>Average stochastic gradient descent.</summary>
        Asgd = 1
    }

    /// <summary>Margin constraints supported by <see cref="SVMSGD"/>.</summary>
    public enum SVMSGDMarginTypes
    {
        /// <summary>Allows outliers through a regularized soft margin.</summary>
        SoftMargin = 0,

        /// <summary>Uses a hard margin for linearly separable data.</summary>
        HardMargin = 1
    }
}
