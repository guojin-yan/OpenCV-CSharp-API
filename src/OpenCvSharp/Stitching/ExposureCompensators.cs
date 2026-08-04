using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>An exposure compensator that leaves images unchanged.</summary>
    public sealed class NoExposureCompensator : ExposureCompensator
    {
        /// <summary>Creates a no-op compensator.</summary>
        public NoExposureCompensator() : base(CreateNative()) { }
        internal NoExposureCompensator(IntPtr nativeHandle) : base(nativeHandle) { }

        private static IntPtr CreateNative()
        {
            NativeException.ThrowIfError(NativeMethods.StitchingExposureCreateNo(out IntPtr handle));
            return handle;
        }
    }

    /// <summary>Estimates one scalar gain per image.</summary>
    public sealed class GainCompensator : ExposureCompensator
    {
        /// <summary>Creates a gain compensator.</summary>
        public GainCompensator(int numberOfFeeds = 1) : base(CreateNative(numberOfFeeds)) { }
        internal GainCompensator(IntPtr nativeHandle) : base(nativeHandle) { }

        /// <summary>Gets or sets the number of estimation passes.</summary>
        public int NumberOfFeeds { get { return GetNumberOfFeeds(); } set { SetNumberOfFeeds(value); } }

        /// <summary>Gets or sets the overlap similarity threshold.</summary>
        public double SimilarityThreshold { get { return GetSimilarityThreshold(); } set { SetSimilarityThreshold(value); } }

        private static IntPtr CreateNative(int numberOfFeeds)
        {
            ValidateNumberOfFeeds(numberOfFeeds);
            NativeException.ThrowIfError(NativeMethods.StitchingExposureCreateGain(numberOfFeeds, out IntPtr handle));
            return handle;
        }
    }

    /// <summary>Estimates an independent gain for each image channel.</summary>
    public sealed class ChannelsCompensator : ExposureCompensator
    {
        /// <summary>Creates a channel compensator.</summary>
        public ChannelsCompensator(int numberOfFeeds = 1) : base(CreateNative(numberOfFeeds)) { }
        internal ChannelsCompensator(IntPtr nativeHandle) : base(nativeHandle) { }

        /// <summary>Gets or sets the number of estimation passes.</summary>
        public int NumberOfFeeds { get { return GetNumberOfFeeds(); } set { SetNumberOfFeeds(value); } }

        /// <summary>Gets or sets the overlap similarity threshold.</summary>
        public double SimilarityThreshold { get { return GetSimilarityThreshold(); } set { SetSimilarityThreshold(value); } }

        private static IntPtr CreateNative(int numberOfFeeds)
        {
            ValidateNumberOfFeeds(numberOfFeeds);
            NativeException.ThrowIfError(NativeMethods.StitchingExposureCreateChannels(numberOfFeeds, out IntPtr handle));
            return handle;
        }
    }

    /// <summary>Base class for block-local exposure compensators.</summary>
    public abstract class BlocksCompensator : ExposureCompensator
    {
        internal BlocksCompensator(IntPtr nativeHandle) : base(nativeHandle) { }

        /// <summary>Gets or sets the number of estimation passes.</summary>
        public int NumberOfFeeds { get { return GetNumberOfFeeds(); } set { SetNumberOfFeeds(value); } }

        /// <summary>Gets or sets the overlap similarity threshold.</summary>
        public double SimilarityThreshold { get { return GetSimilarityThreshold(); } set { SetSimilarityThreshold(value); } }

        /// <summary>Gets or sets the gain block size.</summary>
        public Size BlockSize { get { return GetBlockSize(); } set { SetBlockSize(value); } }

        /// <summary>Gets or sets the number of gain-map filtering iterations.</summary>
        public int FilteringIterations { get { return GetFilteringIterations(); } set { SetFilteringIterations(value); } }
    }

    /// <summary>Estimates block-local scalar gains.</summary>
    public sealed class BlocksGainCompensator : BlocksCompensator
    {
        /// <summary>Creates a block gain compensator.</summary>
        public BlocksGainCompensator(int blockWidth = 32, int blockHeight = 32, int numberOfFeeds = 1)
            : base(CreateNative(blockWidth, blockHeight, numberOfFeeds)) { }
        internal BlocksGainCompensator(IntPtr nativeHandle) : base(nativeHandle) { }

        private static IntPtr CreateNative(int blockWidth, int blockHeight, int numberOfFeeds)
        {
            ValidateBlockArguments(blockWidth, blockHeight, numberOfFeeds);
            NativeException.ThrowIfError(NativeMethods.StitchingExposureCreateBlocksGain(blockWidth, blockHeight, numberOfFeeds, out IntPtr handle));
            return handle;
        }
    }

    /// <summary>Estimates block-local gains independently for each channel.</summary>
    public sealed class BlocksChannelsCompensator : BlocksCompensator
    {
        /// <summary>Creates a block channel compensator.</summary>
        public BlocksChannelsCompensator(int blockWidth = 32, int blockHeight = 32, int numberOfFeeds = 1)
            : base(CreateNative(blockWidth, blockHeight, numberOfFeeds)) { }
        internal BlocksChannelsCompensator(IntPtr nativeHandle) : base(nativeHandle) { }

        private static IntPtr CreateNative(int blockWidth, int blockHeight, int numberOfFeeds)
        {
            ValidateBlockArguments(blockWidth, blockHeight, numberOfFeeds);
            NativeException.ThrowIfError(NativeMethods.StitchingExposureCreateBlocksChannels(blockWidth, blockHeight, numberOfFeeds, out IntPtr handle));
            return handle;
        }
    }
}
