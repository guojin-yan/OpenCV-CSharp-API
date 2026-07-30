using System;

namespace OpenCvSharp.ML
{
    /// <summary>Training algorithms supported by <see cref="ANN_MLP"/>.</summary>
    public enum ANN_MLPTrainingMethods
    {
        /// <summary>Back-propagation.</summary>
        Backprop = 0,

        /// <summary>Resilient back-propagation.</summary>
        Rprop = 1,

        /// <summary>Simulated annealing.</summary>
        Anneal = 2
    }

    /// <summary>Activation functions supported by <see cref="ANN_MLP"/>.</summary>
    public enum ANN_MLPActivationFunctions
    {
        /// <summary>Identity activation.</summary>
        Identity = 0,

        /// <summary>Symmetrical sigmoid activation.</summary>
        SigmoidSym = 1,

        /// <summary>Gaussian activation.</summary>
        Gaussian = 2,

        /// <summary>Rectified linear unit activation.</summary>
        Relu = 3,

        /// <summary>Leaky rectified linear unit activation.</summary>
        LeakyRelu = 4
    }

    /// <summary>Additional training flags supported by <see cref="ANN_MLP"/>.</summary>
    [Flags]
    public enum ANN_MLPTrainFlags
    {
        /// <summary>No additional behavior.</summary>
        None = 0,

        /// <summary>Update existing weights instead of initializing them again.</summary>
        UpdateWeights = 1,

        /// <summary>Do not normalize input features.</summary>
        NoInputScale = 2,

        /// <summary>Do not normalize output values.</summary>
        NoOutputScale = 4
    }
}
