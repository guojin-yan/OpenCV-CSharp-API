using System;
using System.IO;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        /// <summary>
        /// Identifies an encoded image from the stream's current position.
        /// Seekable streams are restored to their original position; non-seekable streams are consumed.
        /// 从流的当前位置识别编码图像。可 seek 流恢复原位置；不可 seek 流会被消费。
        /// </summary>
        /// <param name="stream">The readable encoded image stream. 可读的编码图像流。</param>
        /// <returns>Recognized format, dimensions, and proven frame facts.</returns>
        public static ImageIdentifyResult Identify(Stream stream)
        {
            return Identify(stream, new ImageDecodeOptions());
        }

        /// <summary>
        /// Identifies and validates an encoded image from a stream before native decoding.
        /// 从流识别并在 native 解码前应用限制。
        /// </summary>
        /// <param name="stream">The readable encoded image stream. 可读的编码图像流。</param>
        /// <param name="options">Managed preflight limits, including the maximum bytes read.</param>
        /// <returns>Recognized format, dimensions, and proven frame facts.</returns>
        public static ImageIdentifyResult Identify(Stream stream, ImageDecodeOptions options)
        {
            byte[] buffer = ReadPreflightStream(stream, options);
            ImageIdentifyResult result = IdentifyCore(buffer);
            ValidateIdentifiedInput(buffer.Length, result, options);
            return result;
        }

        /// <summary>
        /// Reads and validates an encoded image stream before native decoding.
        /// Seekable streams are restored to their original position; non-seekable streams are consumed.
        /// 在 native 解码前读取并验证编码图像流。可 seek 流恢复原位置；不可 seek 流会被消费。
        /// </summary>
        /// <param name="stream">The readable encoded image stream. 可读的编码图像流。</param>
        /// <param name="options">Managed preflight limits, including the maximum bytes read.</param>
        /// <param name="flags">The image read mode. 图像读取模式。</param>
        /// <returns>The decoded image. 解码后的图像。</returns>
        public static Mat ImDecode(Stream stream, ImageDecodeOptions options, ImreadModes flags = ImreadModes.Color)
        {
            byte[] buffer = ReadPreflightStream(stream, options);
            ValidateIdentifiedInput(buffer.Length, IdentifyCore(buffer), options);
            return ImDecode(buffer, flags);
        }

        private static byte[] ReadPreflightStream(Stream stream, ImageDecodeOptions options)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!stream.CanRead) throw new ArgumentException("The encoded image stream must be readable.", nameof(stream));

            long originalPosition = 0;
            bool restorePosition = stream.CanSeek;
            if (restorePosition)
            {
                originalPosition = stream.Position;
                if (stream.Length >= originalPosition && stream.Length - originalPosition > options.MaxInputBytes)
                {
                    throw new InvalidDataException("Encoded image exceeds the configured input byte limit.");
                }
            }

            try
            {
                using (var collected = new MemoryStream())
                {
                    byte[] chunk = new byte[81920];
                    long total = 0;
                    while (true)
                    {
                        long remaining = options.MaxInputBytes - total;
                        long readLimit = remaining == long.MaxValue ? remaining : remaining + 1;
                        int requested = (int)Math.Min((long)chunk.Length, readLimit);
                        if (requested <= 0)
                        {
                            throw new InvalidDataException("Encoded image exceeds the configured input byte limit.");
                        }

                        int read = stream.Read(chunk, 0, requested);
                        if (read == 0) break;
                        if (read > remaining)
                        {
                            throw new InvalidDataException("Encoded image exceeds the configured input byte limit.");
                        }
                        if (total > int.MaxValue - read)
                        {
                            throw new InvalidDataException("Encoded image cannot be represented by the managed decode buffer.");
                        }

                        collected.Write(chunk, 0, read);
                        total += read;
                    }

                    if (total == 0)
                    {
                        throw new ArgumentException("Encoded image stream is empty.", nameof(stream));
                    }
                    return collected.ToArray();
                }
            }
            finally
            {
                if (restorePosition)
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }
}
