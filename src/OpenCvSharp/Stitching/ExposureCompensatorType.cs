namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>Specifies the built-in exposure compensation strategy.</summary>
    public enum ExposureCompensatorType
    {
        /// <summary>Performs no exposure compensation.</summary>
        None = 0,

        /// <summary>Uses one scalar gain per image.</summary>
        Gain = 1,

        /// <summary>Uses block-local scalar gains.</summary>
        GainBlocks = 2,

        /// <summary>Uses one gain per image channel.</summary>
        Channels = 3,

        /// <summary>Uses block-local gains per image channel.</summary>
        ChannelsBlocks = 4,
    }
}
