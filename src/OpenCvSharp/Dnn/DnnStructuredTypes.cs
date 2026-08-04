using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Dnn
{
    /// <summary>Controls OpenCV DNN graph tracing.</summary>
    public enum DnnTracingMode
    {
        /// <summary>Disables tracing.</summary>
        None = 0,
        /// <summary>Traces all graph activity.</summary>
        All = 1,
        /// <summary>Traces operation execution.</summary>
        Operation = 2
    }

    /// <summary>Controls the detail recorded by OpenCV DNN profiling.</summary>
    public enum DnnProfilingMode
    {
        /// <summary>Disables profiling.</summary>
        None = 0,
        /// <summary>Records summary profiling.</summary>
        Summary = 1,
        /// <summary>Records detailed profiling.</summary>
        Detailed = 2
    }

    /// <summary>Identifies the model format retained by an OpenCV DNN network.</summary>
    public enum DnnModelFormat
    {
        /// <summary>Generic or programmatically constructed graph.</summary>
        Generic = 0,
        /// <summary>ONNX model.</summary>
        Onnx = 1,
        /// <summary>TensorFlow model.</summary>
        TensorFlow = 2,
        /// <summary>TensorFlow Lite model.</summary>
        TensorFlowLite = 3
    }

    /// <summary>Supported tensor layouts for image-to-blob preprocessing.</summary>
    public enum DnnDataLayout
    {
        /// <summary>Batch, channel, height, width.</summary>
        Nchw = 2,
        /// <summary>Batch, height, width, channel.</summary>
        Nhwc = 4
    }

    /// <summary>Controls resize padding in parameterized image-to-blob conversion.</summary>
    public enum DnnImagePaddingMode
    {
        /// <summary>Resizes directly without padding.</summary>
        None = 0,
        /// <summary>Resizes and crops from the center.</summary>
        CropCenter = 1,
        /// <summary>Preserves aspect ratio and pads to the requested size.</summary>
        Letterbox = 2
    }

    /// <summary>Immutable parameters for OpenCV image-to-blob preprocessing.</summary>
    public sealed class Image2BlobParams
    {
        /// <summary>Creates OpenCV's default parameters: unit scale, CV_32F, and NCHW layout.</summary>
        public Image2BlobParams()
            : this(new Scalar(1.0))
        {
        }

        /// <summary>Creates parameterized image-to-blob preprocessing settings.</summary>
        /// <param name="scaleFactor">Per-channel multiplier applied after mean subtraction.</param>
        /// <param name="size">Output spatial size, or an empty size to preserve input dimensions.</param>
        /// <param name="mean">Per-channel mean to subtract, defaulting to zero.</param>
        /// <param name="swapRB">Whether to exchange the first and third channels.</param>
        /// <param name="ddepth">Output depth; OpenCV supports <see cref="MatType.CV_32F"/> and <see cref="MatType.CV_8U"/> here.</param>
        /// <param name="dataLayout">NCHW or NHWC output tensor layout.</param>
        /// <param name="paddingMode">Resize, center-crop, or letterbox behavior.</param>
        /// <param name="borderValue">Constant value used for letterbox padding.</param>
        /// <exception cref="ArgumentOutOfRangeException">A size, depth, layout, or padding value is outside the supported domain.</exception>
        /// <exception cref="ArgumentException"><paramref name="ddepth"/> is CV_8U with non-unit scale or non-zero mean.</exception>
        public Image2BlobParams(
            Scalar scaleFactor,
            Size? size = null,
            Scalar? mean = null,
            bool swapRB = false,
            int ddepth = MatType.CV_32F,
            DnnDataLayout dataLayout = DnnDataLayout.Nchw,
            DnnImagePaddingMode paddingMode = DnnImagePaddingMode.None,
            Scalar? borderValue = null)
        {
            Size actualSize = size ?? new Size(0, 0);
            if (!((actualSize.Width == 0 && actualSize.Height == 0) || (actualSize.Width > 0 && actualSize.Height > 0)))
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be empty or have positive width and height.");
            if (ddepth != MatType.CV_32F && ddepth != MatType.CV_8U)
                throw new ArgumentOutOfRangeException(nameof(ddepth), "Blob depth must be CV_32F or CV_8U.");
            if (dataLayout != DnnDataLayout.Nchw && dataLayout != DnnDataLayout.Nhwc)
                throw new ArgumentOutOfRangeException(nameof(dataLayout));
            if (paddingMode != DnnImagePaddingMode.None && paddingMode != DnnImagePaddingMode.CropCenter && paddingMode != DnnImagePaddingMode.Letterbox)
                throw new ArgumentOutOfRangeException(nameof(paddingMode));
            if (ddepth == MatType.CV_8U && (scaleFactor != new Scalar(1.0) || (mean ?? new Scalar(0.0)) != new Scalar(0.0)))
                throw new ArgumentException("CV_8U blobs require unit scale and zero mean.");

            ScaleFactor = scaleFactor;
            Size = actualSize;
            Mean = mean ?? new Scalar(0.0);
            SwapRB = swapRB;
            DDepth = ddepth;
            DataLayout = dataLayout;
            PaddingMode = paddingMode;
            BorderValue = borderValue ?? new Scalar(0.0);
        }

        /// <summary>Gets the per-channel multiplier applied after mean subtraction.</summary>
        public Scalar ScaleFactor { get; }
        /// <summary>Gets the requested spatial output size, or an empty size to preserve input dimensions.</summary>
        public Size Size { get; }
        /// <summary>Gets the per-channel mean subtracted from input values.</summary>
        public Scalar Mean { get; }
        /// <summary>Gets whether the first and third channels are swapped.</summary>
        public bool SwapRB { get; }
        /// <summary>Gets the output Mat depth.</summary>
        public int DDepth { get; }
        /// <summary>Gets the output tensor layout.</summary>
        public DnnDataLayout DataLayout { get; }
        /// <summary>Gets the resize/crop/padding behavior.</summary>
        public DnnImagePaddingMode PaddingMode { get; }
        /// <summary>Gets the constant border value used by letterbox padding.</summary>
        public Scalar BorderValue { get; }

        /// <summary>Maps one rectangle from blob coordinates back to original image coordinates.</summary>
        /// <param name="blobRect">Rectangle expressed in the configured blob coordinate system.</param>
        /// <param name="imageSize">Positive original image dimensions.</param>
        /// <returns>The projected rectangle in original image coordinates.</returns>
        /// <remarks>OpenCV applies coordinate conversion only when the configured blob size differs from the image size.</remarks>
        public Rect BlobRectToImageRect(Rect blobRect, Size imageSize)
        {
            ValidateImageSize(imageSize);
            NativeDnnImage2BlobParams native = ToNative();
            NativeDnnRect source = ToNative(blobRect);
            NativeException.ThrowIfError(NativeMethods.DnnBlobRectToImageRect(in native, in source, imageSize.Width, imageSize.Height, out NativeDnnRect result));
            return FromNative(result);
        }

        /// <summary>Maps rectangles from blob coordinates back to original image coordinates.</summary>
        /// <param name="blobRects">Caller-owned source rectangles; an empty array is valid.</param>
        /// <param name="imageSize">Positive original image dimensions.</param>
        /// <returns>A newly allocated array in the same order as <paramref name="blobRects"/>.</returns>
        public Rect[] BlobRectsToImageRects(Rect[] blobRects, Size imageSize)
        {
            if (blobRects == null) throw new ArgumentNullException(nameof(blobRects));
            ValidateImageSize(imageSize);
            NativeDnnRect[] source = new NativeDnnRect[blobRects.Length];
            for (int i = 0; i < source.Length; i++) source[i] = ToNative(blobRects[i]);
            var destination = new NativeDnnRect[source.Length];
            NativeDnnImage2BlobParams native = ToNative();
            NativeException.ThrowIfError(NativeMethods.DnnBlobRectsToImageRects(in native, source, source.Length, imageSize.Width, imageSize.Height, destination, destination.Length, out int written));
            if (written < 0 || written > destination.Length) throw new OpenCvException("Native DNN rectangle count is invalid.");
            var result = new Rect[written];
            for (int i = 0; i < result.Length; i++) result[i] = FromNative(destination[i]);
            return result;
        }

        internal NativeDnnImage2BlobParams ToNative()
        {
            return new NativeDnnImage2BlobParams
            {
                ScaleV0 = ScaleFactor.V0, ScaleV1 = ScaleFactor.V1, ScaleV2 = ScaleFactor.V2, ScaleV3 = ScaleFactor.V3,
                SizeWidth = Size.Width, SizeHeight = Size.Height,
                MeanV0 = Mean.V0, MeanV1 = Mean.V1, MeanV2 = Mean.V2, MeanV3 = Mean.V3,
                SwapRb = SwapRB ? 1 : 0, DDepth = DDepth, DataLayout = (int)DataLayout, PaddingMode = (int)PaddingMode,
                BorderV0 = BorderValue.V0, BorderV1 = BorderValue.V1, BorderV2 = BorderValue.V2, BorderV3 = BorderValue.V3
            };
        }

        private static void ValidateImageSize(Size value)
        {
            if (value.Width <= 0 || value.Height <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        }

        private static NativeDnnRect ToNative(Rect value)
        {
            return new NativeDnnRect { X = value.X, Y = value.Y, Width = value.Width, Height = value.Height };
        }

        private static Rect FromNative(NativeDnnRect value)
        {
            return new Rect(value.X, value.Y, value.Width, value.Height);
        }
    }

    /// <summary>Input and output shapes inferred for one DNN layer.</summary>
    public sealed class DnnLayerShapes
    {
        private readonly int[][] inputShapes;
        private readonly int[][] outputShapes;

        internal DnnLayerShapes(int[][] inputShapes, int[][] outputShapes)
        {
            this.inputShapes = Clone(inputShapes);
            this.outputShapes = Clone(outputShapes);
        }

        /// <summary>Gets independently owned input shapes.</summary>
        public int[][] InputShapes { get { return Clone(inputShapes); } }
        /// <summary>Gets independently owned output shapes.</summary>
        public int[][] OutputShapes { get { return Clone(outputShapes); } }

        private static int[][] Clone(int[][] values)
        {
            var result = new int[values.Length][];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = new int[values[i].Length];
                Array.Copy(values[i], result[i], values[i].Length);
            }
            return result;
        }
    }

    /// <summary>Estimated DNN memory consumption in bytes.</summary>
    public readonly struct DnnMemoryConsumption : IEquatable<DnnMemoryConsumption>
    {
        /// <summary>Creates a memory consumption result.</summary>
        /// <param name="weightsBytes">Bytes occupied by learned parameters.</param>
        /// <param name="blobBytes">Bytes occupied by intermediate blobs.</param>
        public DnnMemoryConsumption(ulong weightsBytes, ulong blobBytes)
        {
            WeightsBytes = weightsBytes;
            BlobBytes = blobBytes;
        }

        /// <summary>Gets bytes occupied by model weights.</summary>
        public ulong WeightsBytes { get; }
        /// <summary>Gets bytes occupied by intermediate blobs.</summary>
        public ulong BlobBytes { get; }
        /// <inheritdoc/>
        public bool Equals(DnnMemoryConsumption other) { return WeightsBytes == other.WeightsBytes && BlobBytes == other.BlobBytes; }
        /// <inheritdoc/>
        public override bool Equals(object? obj) { return obj is DnnMemoryConsumption other && Equals(other); }
        /// <inheritdoc/>
        public override int GetHashCode() { return (WeightsBytes.GetHashCode() * 397) ^ BlobBytes.GetHashCode(); }
        /// <summary>Determines whether two results contain the same byte counts.</summary>
        /// <param name="left">First result.</param>
        /// <param name="right">Second result.</param>
        /// <returns><see langword="true"/> when both byte counts are equal.</returns>
        public static bool operator ==(DnnMemoryConsumption left, DnnMemoryConsumption right) { return left.Equals(right); }
        /// <summary>Determines whether two results contain different byte counts.</summary>
        /// <param name="left">First result.</param>
        /// <param name="right">Second result.</param>
        /// <returns><see langword="true"/> when either byte count differs.</returns>
        public static bool operator !=(DnnMemoryConsumption left, DnnMemoryConsumption right) { return !left.Equals(right); }
    }

    /// <summary>Structured detailed profile strings produced by OpenCV DNN.</summary>
    public sealed class DnnDetailedPerfProfile
    {
        private readonly string[] names;
        private readonly string[] times;
        private readonly string[] invocationCounts;

        internal DnnDetailedPerfProfile(string[] names, string[] times, string[] invocationCounts)
        {
            if (names.Length != times.Length || names.Length != invocationCounts.Length) throw new ArgumentException("Detailed profile columns must have equal lengths.");
            this.names = Clone(names);
            this.times = Clone(times);
            this.invocationCounts = Clone(invocationCounts);
        }

        /// <summary>Gets the number of profile rows.</summary>
        public int Count { get { return names.Length; } }
        /// <summary>Gets independently owned operation/layer names.</summary>
        public string[] Names { get { return Clone(names); } }
        /// <summary>Gets independently owned formatted timing values.</summary>
        public string[] Times { get { return Clone(times); } }
        /// <summary>Gets independently owned formatted invocation counts.</summary>
        public string[] InvocationCounts { get { return Clone(invocationCounts); } }

        private static string[] Clone(string[] values)
        {
            var result = new string[values.Length];
            Array.Copy(values, result, values.Length);
            return result;
        }
    }
}
