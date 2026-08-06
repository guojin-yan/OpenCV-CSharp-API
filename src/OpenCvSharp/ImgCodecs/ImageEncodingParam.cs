using System;
using System.Collections.Generic;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    /// <summary>Represents one type-safe key/value option for OpenCV image encoders.</summary>
    public readonly struct ImageEncodingParam : IEquatable<ImageEncodingParam>
    {
        /// <summary>Creates one encoder option.</summary>
        /// <param name="encodingId">The OpenCV encoder option identifier.</param>
        /// <param name="value">The option value.</param>
        public ImageEncodingParam(ImwriteFlags encodingId, int value)
        {
            EncodingId = encodingId;
            Value = value;
        }

        /// <summary>Gets the encoder option key.</summary>
        public ImwriteFlags EncodingId { get; }

        /// <summary>Gets the encoder option value.</summary>
        public int Value { get; }

        /// <inheritdoc/>
        public bool Equals(ImageEncodingParam other)
        {
            return EncodingId == other.EncodingId && Value == other.Value;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is ImageEncodingParam other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ((int)EncodingId * 397) ^ Value;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{" + EncodingId + "=" + Value + "}";
        }

        /// <summary>Determines whether two encoder options are equal.</summary>
        public static bool operator ==(ImageEncodingParam left, ImageEncodingParam right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two encoder options are different.</summary>
        public static bool operator !=(ImageEncodingParam left, ImageEncodingParam right)
        {
            return !left.Equals(right);
        }
    }

    public static partial class Cv2
    {
        /// <summary>Encodes an image with type-safe format parameters.</summary>
        public static byte[] ImEncode(string ext, Mat image, ImageEncodingParam[] parameters)
        {
            return ImEncode(ext, image, ToNativeParameters(parameters));
        }

        /// <summary>Writes an image with type-safe format parameters.</summary>
        public static bool ImWrite(string filename, Mat image, ImageEncodingParam[] parameters)
        {
            return ImWrite(filename, image, ToNativeParameters(parameters));
        }

        /// <summary>Writes multiple images with type-safe format parameters.</summary>
        public static bool ImWriteMulti(string filename, IReadOnlyList<Mat> images, ImageEncodingParam[] parameters)
        {
            return ImWriteMulti(filename, images, ToNativeParameters(parameters));
        }

        /// <summary>Encodes multiple images with type-safe format parameters.</summary>
        public static byte[] ImEncodeMulti(string ext, IReadOnlyList<Mat> images, ImageEncodingParam[] parameters)
        {
            return ImEncodeMulti(ext, images, ToNativeParameters(parameters));
        }

        /// <summary>Writes an image and metadata with type-safe format parameters.</summary>
        public static bool ImWriteWithMetadata(
            string filename,
            Mat image,
            IReadOnlyList<ImageMetadataChunk> metadata,
            ImageEncodingParam[] parameters)
        {
            return ImWriteWithMetadata(filename, image, metadata, ToNativeParameters(parameters));
        }

        /// <summary>Encodes an image and metadata with type-safe format parameters.</summary>
        public static byte[] ImEncodeWithMetadata(
            string ext,
            Mat image,
            IReadOnlyList<ImageMetadataChunk> metadata,
            ImageEncodingParam[] parameters)
        {
            return ImEncodeWithMetadata(ext, image, metadata, ToNativeParameters(parameters));
        }

        /// <summary>Writes an animation with type-safe format parameters.</summary>
        public static bool ImWriteAnimation(string filename, Animation animation, ImageEncodingParam[] parameters)
        {
            return ImWriteAnimation(filename, animation, ToNativeParameters(parameters));
        }

        /// <summary>Encodes an animation with type-safe format parameters.</summary>
        public static byte[] ImEncodeAnimation(string ext, Animation animation, ImageEncodingParam[] parameters)
        {
            return ImEncodeAnimation(ext, animation, ToNativeParameters(parameters));
        }

        private static int[] ToNativeParameters(ImageEncodingParam[] parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var values = new int[checked(parameters.Length * 2)];
            for (int index = 0; index < parameters.Length; index++)
            {
                values[index * 2] = (int)parameters[index].EncodingId;
                values[(index * 2) + 1] = parameters[index].Value;
            }

            return values;
        }
    }
}
