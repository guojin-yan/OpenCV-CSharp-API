using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Describes the managed storage contract for one OpenCV matrix element.
    /// 描述一个 OpenCV 矩阵元素的托管存储契约。
    /// </summary>
    public readonly struct PixelTypeDescriptor : IEquatable<PixelTypeDescriptor>
    {
        internal PixelTypeDescriptor(
            Type elementType,
            int depth,
            int channels,
            int elementSizeBytes,
            int alignmentBytes,
            PixelChannelOrder channelOrder,
            PixelAlphaMode alphaMode,
            bool canWrite)
        {
            ElementType = elementType;
            Depth = depth;
            Channels = channels;
            ElementSizeBytes = elementSizeBytes;
            AlignmentBytes = alignmentBytes;
            ChannelOrder = channelOrder;
            AlphaMode = alphaMode;
            CanWrite = canWrite;
        }

        /// <summary>Gets the registered managed element type.</summary>
        public Type ElementType { get; }

        /// <summary>Gets the OpenCV depth constant, such as <see cref="MatType.CV_8U"/>.</summary>
        public int Depth { get; }

        /// <summary>Gets the number of OpenCV channels in one matrix element.</summary>
        public int Channels { get; }

        /// <summary>Gets the complete element size in bytes, including all channels.</summary>
        public int ElementSizeBytes { get; }

        /// <summary>Gets the required byte alignment for the managed element.</summary>
        public int AlignmentBytes { get; }

        /// <summary>Gets the known channel order, or <see cref="PixelChannelOrder.Unknown"/>.</summary>
        public PixelChannelOrder ChannelOrder { get; }

        /// <summary>Gets the alpha interpretation, or <see cref="PixelAlphaMode.Unknown"/>.</summary>
        public PixelAlphaMode AlphaMode { get; }

        /// <summary>Gets whether the registered element can be used with writable views.</summary>
        public bool CanWrite { get; }

        /// <summary>Gets the encoded OpenCV matrix type represented by this descriptor.</summary>
        public int MatType { get { return Core.MatType.MakeType(Depth, Channels); } }

        /// <summary>Determines whether an encoded matrix type exactly matches this descriptor.</summary>
        public bool MatchesMatType(int type)
        {
            return Core.MatType.TypeMask(type) == MatType;
        }

        /// <inheritdoc/>
        public bool Equals(PixelTypeDescriptor other)
        {
            return ElementType == other.ElementType &&
                Depth == other.Depth &&
                Channels == other.Channels &&
                ElementSizeBytes == other.ElementSizeBytes &&
                AlignmentBytes == other.AlignmentBytes &&
                ChannelOrder == other.ChannelOrder &&
                AlphaMode == other.AlphaMode &&
                CanWrite == other.CanWrite;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is PixelTypeDescriptor other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = ElementType == null ? 0 : ElementType.GetHashCode();
                hash = (hash * 397) ^ Depth;
                hash = (hash * 397) ^ Channels;
                hash = (hash * 397) ^ ElementSizeBytes;
                hash = (hash * 397) ^ AlignmentBytes;
                hash = (hash * 397) ^ (int)ChannelOrder;
                hash = (hash * 397) ^ (int)AlphaMode;
                return (hash * 397) ^ (CanWrite ? 1 : 0);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ElementType == null ? "Unknown" : ElementType.FullName + " -> depth=" + Depth + ", channels=" + Channels;
        }

        /// <summary>Gets the descriptor for a registered managed element type.</summary>
        /// <typeparam name="T">The managed element type.</typeparam>
        /// <exception cref="NotSupportedException">The type is not in the explicit registry.</exception>
        public static PixelTypeDescriptor Get<T>() where T : struct
        {
            PixelTypeDescriptor descriptor;
            if (!PixelTypeTraits.TryGet<T>(out descriptor))
            {
                throw new NotSupportedException("The managed pixel type is not registered: " + typeof(T).FullName);
            }

            return descriptor;
        }

        /// <summary>Gets a descriptor for a registered managed element type.</summary>
        /// <param name="type">The managed element type.</param>
        /// <param name="descriptor">The registered descriptor, when available.</param>
        /// <returns><c>true</c> when the type is registered; otherwise <c>false</c>.</returns>
        public static bool TryGet(Type type, out PixelTypeDescriptor descriptor)
        {
            return PixelTypeTraits.TryGet(type, out descriptor);
        }

        /// <summary>Determines whether a managed element type is explicitly registered.</summary>
        public static bool IsRegistered<T>() where T : struct
        {
            PixelTypeDescriptor descriptor;
            return PixelTypeTraits.TryGet<T>(out descriptor);
        }

        /// <summary>Determines whether two descriptors are equal.</summary>
        public static bool operator ==(PixelTypeDescriptor left, PixelTypeDescriptor right) { return left.Equals(right); }
        /// <summary>Determines whether two descriptors differ.</summary>
        public static bool operator !=(PixelTypeDescriptor left, PixelTypeDescriptor right) { return !left.Equals(right); }
    }

    /// <summary>Describes the channel ordering of a pixel representation.</summary>
    public enum PixelChannelOrder
    {
        /// <summary>The channel order is not known from the storage type.</summary>
        Unknown = 0,
        /// <summary>One grayscale channel.</summary>
        Gray = 1,
        /// <summary>Blue, green, red.</summary>
        Bgr = 2,
        /// <summary>Red, green, blue.</summary>
        Rgb = 3,
        /// <summary>Blue, green, red, alpha.</summary>
        Bgra = 4,
        /// <summary>Red, green, blue, alpha.</summary>
        Rgba = 5
    }

    /// <summary>Describes how an alpha channel is represented.</summary>
    public enum PixelAlphaMode
    {
        /// <summary>The storage type has no alpha channel.</summary>
        None = 0,
        /// <summary>The alpha interpretation is not known.</summary>
        Unknown = 1,
        /// <summary>Color channels are straight (unassociated) alpha.</summary>
        Straight = 2,
        /// <summary>Color channels are premultiplied by alpha.</summary>
        Premultiplied = 3
    }

    /// <summary>
    /// Explicit allow-list of managed element types supported by typed pixel APIs.
    /// 类型化像素 API 支持的托管元素显式白名单。
    /// </summary>
    public static class PixelTypeTraits
    {
        private static readonly IReadOnlyDictionary<Type, PixelTypeDescriptor> Registry = CreateRegistry();

        /// <summary>Gets all registered descriptors.</summary>
        public static IEnumerable<PixelTypeDescriptor> RegisteredTypes
        {
            get { return Registry.Values; }
        }

        /// <summary>Gets the number of explicitly registered managed element types.</summary>
        public static int RegisteredTypeCount
        {
            get { return Registry.Count; }
        }

        /// <summary>Looks up a registered managed element type.</summary>
        public static bool TryGet<T>(out PixelTypeDescriptor descriptor) where T : struct
        {
            return TryGet(typeof(T), out descriptor);
        }

        /// <summary>Gets the descriptor for a registered managed element type.</summary>
        /// <typeparam name="T">The managed element type.</typeparam>
        /// <exception cref="NotSupportedException">The type is not in the explicit registry.</exception>
        public static PixelTypeDescriptor Get<T>() where T : struct
        {
            PixelTypeDescriptor descriptor;
            if (!TryGet<T>(out descriptor))
            {
                throw new NotSupportedException("The managed pixel type is not registered: " + typeof(T).FullName);
            }

            return descriptor;
        }

        /// <summary>Looks up a registered managed element type.</summary>
        public static bool TryGet(Type type, out PixelTypeDescriptor descriptor)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return Registry.TryGetValue(type, out descriptor);
        }

        private static IReadOnlyDictionary<Type, PixelTypeDescriptor> CreateRegistry()
        {
            var descriptors = new Dictionary<Type, PixelTypeDescriptor>();
            Add<byte>(descriptors, MatType.CV_8U, 1, 1);
            Add<sbyte>(descriptors, MatType.CV_8S, 1, 1);
            Add<ushort>(descriptors, MatType.CV_16U, 1, 2);
            Add<short>(descriptors, MatType.CV_16S, 1, 2);
            Add<int>(descriptors, MatType.CV_32S, 1, 4);
            Add<uint>(descriptors, MatType.CV_32U, 1, 4);
            Add<long>(descriptors, MatType.CV_64S, 1, 8);
            Add<ulong>(descriptors, MatType.CV_64U, 1, 8);
            Add<float>(descriptors, MatType.CV_32F, 1, 4);
            Add<double>(descriptors, MatType.CV_64F, 1, 8);

            Add<Vec2b>(descriptors, MatType.CV_8U, 2, 1);
            Add<Vec3b>(descriptors, MatType.CV_8U, 3, 1);
            Add<Vec4b>(descriptors, MatType.CV_8U, 4, 1);
            Add<Vec2s>(descriptors, MatType.CV_16S, 2, 2);
            Add<Vec3s>(descriptors, MatType.CV_16S, 3, 2);
            Add<Vec4s>(descriptors, MatType.CV_16S, 4, 2);
            Add<Vec2w>(descriptors, MatType.CV_16U, 2, 2);
            Add<Vec3w>(descriptors, MatType.CV_16U, 3, 2);
            Add<Vec4w>(descriptors, MatType.CV_16U, 4, 2);
            Add<Vec2i>(descriptors, MatType.CV_32S, 2, 4);
            Add<Vec3i>(descriptors, MatType.CV_32S, 3, 4);
            Add<Vec4i>(descriptors, MatType.CV_32S, 4, 4);
            Add<Vec2f>(descriptors, MatType.CV_32F, 2, 4);
            Add<Vec3f>(descriptors, MatType.CV_32F, 3, 4);
            Add<Vec4f>(descriptors, MatType.CV_32F, 4, 4);
            Add<Vec2d>(descriptors, MatType.CV_64F, 2, 8);
            Add<Vec3d>(descriptors, MatType.CV_64F, 3, 8);
            Add<Vec4d>(descriptors, MatType.CV_64F, 4, 8);
            return descriptors;
        }

        private static void Add<T>(Dictionary<Type, PixelTypeDescriptor> descriptors, int depth, int channels, int alignment) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            descriptors.Add(typeof(T), new PixelTypeDescriptor(
                typeof(T), depth, channels, size, alignment, PixelChannelOrder.Unknown,
                channels == 1 ? PixelAlphaMode.None : PixelAlphaMode.Unknown, true));
        }
    }
}
