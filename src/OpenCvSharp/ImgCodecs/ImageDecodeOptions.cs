using System;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    /// <summary>
    /// Limits applied by the optional managed image decode preflight.
    /// 可选的 managed 图像解码预检所使用的限制。
    /// </summary>
    public sealed class ImageDecodeOptions
    {
        /// <summary>Creates conservative limits suitable for untrusted image buffers.</summary>
        public ImageDecodeOptions()
            : this(256L * 1024L * 1024L, 65536, 65536, 1073741824L, 1024, false, false,
                64L * 1024L * 1024L, 4L * 1024L * 1024L, false, false,
                long.MaxValue, int.MaxValue, int.MaxValue, false)
        {
        }

        /// <summary>Creates explicit image input and allocation limits.</summary>
        /// <param name="maxInputBytes">Maximum encoded input size.</param>
        /// <param name="maxWidth">Maximum decoded width.</param>
        /// <param name="maxHeight">Maximum decoded height.</param>
        /// <param name="maxPixels">Maximum decoded width multiplied by height.</param>
        /// <param name="maxFrames">Maximum frame/page count when the header proves it.</param>
        /// <param name="rejectUnknownFormat">Whether an unrecognized format must be rejected.</param>
        /// <param name="requireKnownSize">Whether dimensions must be available from the header.</param>
        public ImageDecodeOptions(
            long maxInputBytes,
            int maxWidth,
            int maxHeight,
            long maxPixels,
            int maxFrames,
            bool rejectUnknownFormat,
            bool requireKnownSize)
            : this(maxInputBytes, maxWidth, maxHeight, maxPixels, maxFrames, rejectUnknownFormat, requireKnownSize,
                long.MaxValue, long.MaxValue, false, false,
                long.MaxValue, int.MaxValue, int.MaxValue, false)
        {
        }

        /// <summary>Creates explicit image, metadata, and ICC payload limits.</summary>
        /// <param name="maxInputBytes">Maximum encoded input size.</param>
        /// <param name="maxWidth">Maximum decoded width.</param>
        /// <param name="maxHeight">Maximum decoded height.</param>
        /// <param name="maxPixels">Maximum decoded width multiplied by height.</param>
        /// <param name="maxFrames">Maximum frame/page count when the header proves it.</param>
        /// <param name="rejectUnknownFormat">Whether an unrecognized format must be rejected.</param>
        /// <param name="requireKnownSize">Whether dimensions must be available from the header.</param>
        /// <param name="maxMetadataBytes">Maximum inspected metadata payload size when the header proves it.</param>
        /// <param name="maxIccProfileBytes">Maximum inspected ICC profile payload size when the header proves it.</param>
        public ImageDecodeOptions(
            long maxInputBytes,
            int maxWidth,
            int maxHeight,
            long maxPixels,
            int maxFrames,
            bool rejectUnknownFormat,
            bool requireKnownSize,
            long maxMetadataBytes,
            long maxIccProfileBytes)
            : this(maxInputBytes, maxWidth, maxHeight, maxPixels, maxFrames, rejectUnknownFormat, requireKnownSize,
                maxMetadataBytes, maxIccProfileBytes, false, false,
                long.MaxValue, int.MaxValue, int.MaxValue, false)
        {
        }

        /// <summary>Creates explicit image, metadata, and strict-known-fact limits.</summary>
        /// <param name="maxInputBytes">Maximum encoded input size.</param>
        /// <param name="maxWidth">Maximum decoded width.</param>
        /// <param name="maxHeight">Maximum decoded height.</param>
        /// <param name="maxPixels">Maximum decoded width multiplied by height.</param>
        /// <param name="maxFrames">Maximum frame/page count when the header proves it.</param>
        /// <param name="rejectUnknownFormat">Whether an unrecognized format must be rejected.</param>
        /// <param name="requireKnownSize">Whether dimensions must be available from the header.</param>
        /// <param name="maxMetadataBytes">Maximum inspected metadata payload size when the header proves it.</param>
        /// <param name="maxIccProfileBytes">Maximum inspected ICC profile payload size when the header proves it.</param>
        /// <param name="requireKnownMetadataSize">Whether a proven metadata payload size is required.</param>
        /// <param name="requireKnownIccProfileSize">Whether a proven ICC profile payload size is required.</param>
        public ImageDecodeOptions(
            long maxInputBytes,
            int maxWidth,
            int maxHeight,
            long maxPixels,
            int maxFrames,
            bool rejectUnknownFormat,
            bool requireKnownSize,
            long maxMetadataBytes,
            long maxIccProfileBytes,
            bool requireKnownMetadataSize,
            bool requireKnownIccProfileSize)
            : this(maxInputBytes, maxWidth, maxHeight, maxPixels, maxFrames, rejectUnknownFormat, requireKnownSize,
                maxMetadataBytes, maxIccProfileBytes, requireKnownMetadataSize, requireKnownIccProfileSize,
                long.MaxValue, int.MaxValue, int.MaxValue, false)
        {
        }

        /// <summary>Creates explicit image, metadata, and ICC payload limits with strict-known-fact controls.</summary>
        /// <param name="maxInputBytes">Maximum encoded input size.</param>
        /// <param name="maxWidth">Maximum decoded width.</param>
        /// <param name="maxHeight">Maximum decoded height.</param>
        /// <param name="maxPixels">Maximum decoded width multiplied by height.</param>
        /// <param name="maxFrames">Maximum frame/page count when the header proves it.</param>
        /// <param name="rejectUnknownFormat">Whether an unrecognized format must be rejected.</param>
        /// <param name="requireKnownSize">Whether dimensions must be available from the header.</param>
        /// <param name="maxMetadataBytes">Maximum inspected metadata payload size when the header proves it.</param>
        /// <param name="maxIccProfileBytes">Maximum inspected ICC profile payload size when the header proves it.</param>
        /// <param name="requireKnownMetadataSize">Whether a proven metadata payload size is required.</param>
        /// <param name="requireKnownIccProfileSize">Whether a proven ICC profile payload size is required.</param>
        /// <param name="maxCumulativePixels">Maximum width-times-height budget across known frames or pages.</param>
        /// <param name="maxBitDepth">Maximum encoded sample depth in bits when known.</param>
        /// <param name="maxChannels">Maximum encoded channel count when known.</param>
        /// <param name="rejectUnknownPixelFormat">Whether unknown encoded depth or channel count must be rejected.</param>
        public ImageDecodeOptions(
            long maxInputBytes,
            int maxWidth,
            int maxHeight,
            long maxPixels,
            int maxFrames,
            bool rejectUnknownFormat,
            bool requireKnownSize,
            long maxMetadataBytes,
            long maxIccProfileBytes,
            bool requireKnownMetadataSize,
            bool requireKnownIccProfileSize,
            long maxCumulativePixels,
            int maxBitDepth,
            int maxChannels,
            bool rejectUnknownPixelFormat)
        {
            if (maxInputBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxInputBytes));
            if (maxWidth <= 0) throw new ArgumentOutOfRangeException(nameof(maxWidth));
            if (maxHeight <= 0) throw new ArgumentOutOfRangeException(nameof(maxHeight));
            if (maxPixels <= 0) throw new ArgumentOutOfRangeException(nameof(maxPixels));
            if (maxFrames <= 0) throw new ArgumentOutOfRangeException(nameof(maxFrames));
            if (maxMetadataBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxMetadataBytes));
            if (maxIccProfileBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxIccProfileBytes));
            if (maxCumulativePixels <= 0) throw new ArgumentOutOfRangeException(nameof(maxCumulativePixels));
            if (maxBitDepth <= 0) throw new ArgumentOutOfRangeException(nameof(maxBitDepth));
            if (maxChannels <= 0) throw new ArgumentOutOfRangeException(nameof(maxChannels));

            MaxInputBytes = maxInputBytes;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            MaxPixels = maxPixels;
            MaxFrames = maxFrames;
            RejectUnknownFormat = rejectUnknownFormat;
            RequireKnownSize = requireKnownSize;
            MaxMetadataBytes = maxMetadataBytes;
            MaxIccProfileBytes = maxIccProfileBytes;
            RequireKnownMetadataSize = requireKnownMetadataSize;
            RequireKnownIccProfileSize = requireKnownIccProfileSize;
            MaxCumulativePixels = maxCumulativePixels;
            MaxBitDepth = maxBitDepth;
            MaxChannels = maxChannels;
            RejectUnknownPixelFormat = rejectUnknownPixelFormat;
        }

        /// <summary>Gets the maximum encoded input size in bytes.</summary>
        public long MaxInputBytes { get; }

        /// <summary>Gets the maximum decoded width.</summary>
        public int MaxWidth { get; }

        /// <summary>Gets the maximum decoded height.</summary>
        public int MaxHeight { get; }

        /// <summary>Gets the maximum decoded pixel count.</summary>
        public long MaxPixels { get; }

        /// <summary>Gets the maximum known frame/page count.</summary>
        public int MaxFrames { get; }

        /// <summary>Gets whether unknown formats are rejected before native decoding.</summary>
        public bool RejectUnknownFormat { get; }

        /// <summary>Gets whether a header without dimensions is rejected before native decoding.</summary>
        public bool RequireKnownSize { get; }

        /// <summary>Gets the maximum known metadata payload size.</summary>
        public long MaxMetadataBytes { get; }

        /// <summary>Gets the maximum known ICC profile payload size.</summary>
        public long MaxIccProfileBytes { get; }

        /// <summary>Gets whether a proven metadata payload size is required.</summary>
        public bool RequireKnownMetadataSize { get; }

        /// <summary>Gets whether a proven ICC profile payload size is required.</summary>
        public bool RequireKnownIccProfileSize { get; }

        /// <summary>Gets the maximum cumulative width-times-height budget across known frames or pages.</summary>
        public long MaxCumulativePixels { get; }

        /// <summary>Gets the maximum encoded sample depth in bits.</summary>
        public int MaxBitDepth { get; }

        /// <summary>Gets the maximum encoded channel count.</summary>
        public int MaxChannels { get; }

        /// <summary>Gets whether unknown encoded depth or channel facts are rejected before native decoding.</summary>
        public bool RejectUnknownPixelFormat { get; }
    }
}
