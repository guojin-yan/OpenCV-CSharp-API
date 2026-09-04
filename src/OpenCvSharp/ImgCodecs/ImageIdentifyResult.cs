using System;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    /// <summary>
    /// Managed-only facts extracted from an encoded image header.
    /// 从编码图像头中提取的纯 managed 事实。
    /// </summary>
    public sealed class ImageIdentifyResult
    {
        internal ImageIdentifyResult(
            string format,
            int width,
            int height,
            bool sizeKnown,
            int frameCount,
            bool frameCountKnown,
            long inputBytes,
            int bytesInspected,
            long metadataBytes,
            bool metadataSizeKnown,
            long iccProfileBytes,
            bool iccProfileSizeKnown,
            int bitDepth,
            bool bitDepthKnown,
            int channelCount,
            bool channelCountKnown)
        {
            Format = format ?? string.Empty;
            Width = width;
            Height = height;
            IsSizeKnown = sizeKnown;
            FrameCount = frameCount;
            IsFrameCountKnown = frameCountKnown;
            InputBytes = inputBytes;
            BytesInspected = bytesInspected;
            MetadataBytes = metadataBytes;
            IsMetadataSizeKnown = metadataSizeKnown;
            IccProfileBytes = iccProfileBytes;
            IsIccProfileSizeKnown = iccProfileSizeKnown;
            BitDepth = bitDepth;
            IsBitDepthKnown = bitDepthKnown;
            ChannelCount = channelCount;
            IsChannelCountKnown = channelCountKnown;
        }

        /// <summary>Gets a lowercase format name, or <c>unknown</c>.</summary>
        public string Format { get; }

        /// <summary>Gets whether the format was recognized by the managed header parser.</summary>
        public bool IsFormatKnown { get { return !string.Equals(Format, "unknown", StringComparison.Ordinal); } }

        /// <summary>Gets the decoded width when known; otherwise zero.</summary>
        public int Width { get; }

        /// <summary>Gets the decoded height when known; otherwise zero.</summary>
        public int Height { get; }

        /// <summary>Gets whether both dimensions were available in the header.</summary>
        public bool IsSizeKnown { get; }

        /// <summary>Gets the frame/page count when known; otherwise zero.</summary>
        public int FrameCount { get; }

        /// <summary>Gets whether the frame/page count was proven by the header.</summary>
        public bool IsFrameCountKnown { get; }

        /// <summary>Gets the encoded input length.</summary>
        public long InputBytes { get; }

        /// <summary>Gets the number of input bytes inspected by the managed parser.</summary>
        public int BytesInspected { get; }

        /// <summary>Gets the selected inspected metadata payload size when known; otherwise zero.</summary>
        public long MetadataBytes { get; }

        /// <summary>Gets whether the managed parser proved the selected metadata payload size.</summary>
        public bool IsMetadataSizeKnown { get; }

        /// <summary>Gets the inspected ICC profile payload size when known; otherwise zero.</summary>
        public long IccProfileBytes { get; }

        /// <summary>Gets whether the managed parser proved the ICC profile payload size.</summary>
        public bool IsIccProfileSizeKnown { get; }

        /// <summary>Gets the encoded sample depth in bits when known; otherwise zero.</summary>
        public int BitDepth { get; }

        /// <summary>Gets whether the encoded sample depth was proven by the header.</summary>
        public bool IsBitDepthKnown { get; }

        /// <summary>Gets the encoded channel count when known; otherwise zero.</summary>
        public int ChannelCount { get; }

        /// <summary>Gets whether the encoded channel count was proven by the header.</summary>
        public bool IsChannelCountKnown { get; }

        /// <summary>Gets whether both encoded depth and channel count are known.</summary>
        public bool IsPixelFormatKnown { get { return IsBitDepthKnown && IsChannelCountKnown; } }

        /// <inheritdoc />
        public override string ToString()
        {
            string size = IsSizeKnown ? Width + "x" + Height : "unknown-size";
            string frames = IsFrameCountKnown ? FrameCount.ToString() : "unknown-frames";
            return Format + " " + size + " frames=" + frames + " bytes=" + InputBytes;
        }
    }
}
