using System;
using System.Runtime.InteropServices;
using System.Text;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    /// <summary>
    /// Provides image encoding and decoding functions aligned with OpenCV <c>cv</c> free functions.
    /// 提供与 OpenCV <c>cv</c> 自由函数对齐的图像编码和解码函数。
    /// </summary>
    public static partial class Cv2
    {
        /// <summary>
        /// Encodes an image into an in-memory compressed image buffer.
        /// 将图像编码为内存中的压缩图像缓冲区。
        /// </summary>
        /// <param name="ext">The image file extension, such as <c>.png</c> or <c>.jpg</c>. 图像文件扩展名，例如 <c>.png</c> 或 <c>.jpg</c>。</param>
        /// <param name="image">The image to encode. 要编码的图像。</param>
        /// <returns>The encoded image bytes. 编码后的图像字节。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ext"/> is null or whitespace. 当 <paramref name="ext"/> 为空或空白时抛出。</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null. 当 <paramref name="image"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static byte[] ImEncode(string ext, Mat image)
        {
            if (string.IsNullOrWhiteSpace(ext))
            {
                throw new ArgumentException("Image extension cannot be null or whitespace.", nameof(ext));
            }

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            NativeException.ThrowIfError(NativeMethods.ImgCodecsImEncode(ext, image.NativeHandle, out IntPtr nativeBuffer));
            using (NativeEncodedBufferHandle buffer = NativeEncodedBufferHandle.FromNativePointer(nativeBuffer))
            {
                return CopyEncodedBufferToArray(buffer);
            }
        }

        /// <summary>
        /// Encodes an image into an in-memory compressed image buffer with encoder parameters.
        /// 带编码参数地将图像编码为内存中的压缩图像缓冲区。
        /// </summary>
        /// <param name="ext">The image file extension, such as <c>.png</c> or <c>.jpg</c>. 图像文件扩展名，例如 <c>.png</c> 或 <c>.jpg</c>。</param>
        /// <param name="image">The image to encode. 要编码的图像。</param>
        /// <param name="parameters">Encoder parameters as key-value pairs. 编码参数，按键值对传入。</param>
        /// <returns>The encoded image bytes. 编码后的图像字节。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ext"/> is null or whitespace, or when <paramref name="parameters"/> has an odd length. 当 <paramref name="ext"/> 为空或空白，或 <paramref name="parameters"/> 长度为奇数时抛出。</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null. 当 <paramref name="image"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static byte[] ImEncode(string ext, Mat image, int[] parameters)
        {
            if (string.IsNullOrWhiteSpace(ext))
            {
                throw new ArgumentException("Image extension cannot be null or whitespace.", nameof(ext));
            }

            if (parameters == null)
            {
                return ImEncode(ext, image);
            }

            if ((parameters.Length % 2) != 0)
            {
                throw new ArgumentException("Encoder parameters must contain key-value pairs.", nameof(parameters));
            }

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return ImEncodeCore(ext, image, parameters);
        }

        /// <summary>
        /// Decodes an image from an in-memory compressed image buffer.
        /// 从内存中的压缩图像缓冲区解码图像。
        /// </summary>
        /// <param name="buffer">The encoded image bytes. 编码后的图像字节。</param>
        /// <param name="flags">The image read mode. 图像读取模式。</param>
        /// <returns>The decoded image. 解码后的图像。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="buffer"/> is null. 当 <paramref name="buffer"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="buffer"/> is empty. 当 <paramref name="buffer"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Mat ImDecode(byte[] buffer, ImreadModes flags = ImreadModes.Color)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (buffer.Length == 0)
            {
                throw new ArgumentException("Encoded image buffer cannot be empty.", nameof(buffer));
            }

            ValidateImreadMode(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImDecode(buffer, ToUIntPtr(buffer.Length), (int)flags, out IntPtr image));
            return new Mat(image);
        }

        /// <summary>
        /// Loads an image from a file.
        /// 从文件加载图像。
        /// </summary>
        /// <param name="filename">The image file path encoded as UTF-8 for native OpenCV. 图像文件路径，传递给 native OpenCV 时使用 UTF-8。</param>
        /// <param name="flags">The image read mode. 图像读取模式。</param>
        /// <returns>The loaded image. 加载得到的图像。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filename"/> is null or whitespace. 当 <paramref name="filename"/> 为空或空白时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Mat ImRead(string filename, ImreadModes flags = ImreadModes.Color)
        {
            byte[] nativeFilename = ToNullTerminatedUtf8(filename, nameof(filename));
            ValidateImreadMode(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImRead(nativeFilename, (int)flags, out IntPtr image));
            return new Mat(image);
        }

        /// <summary>
        /// Saves an image to a file.
        /// 将图像保存到文件。
        /// </summary>
        /// <param name="filename">The output image file path encoded as UTF-8 for native OpenCV. 输出图像文件路径，传递给 native OpenCV 时使用 UTF-8。</param>
        /// <param name="image">The image to save. 要保存的图像。</param>
        /// <returns><c>true</c> when OpenCV reports that the image was written. OpenCV 报告图像已写入时返回 <c>true</c>。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filename"/> is null or whitespace. 当 <paramref name="filename"/> 为空或空白时抛出。</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null. 当 <paramref name="image"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static bool ImWrite(string filename, Mat image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            byte[] nativeFilename = ToNullTerminatedUtf8(filename, nameof(filename));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImWrite(nativeFilename, image.NativeHandle, out int written));
            return written != 0;
        }

        /// <summary>
        /// Saves an image to a file with encoder parameters.
        /// 带编码参数地将图像保存到文件。
        /// </summary>
        /// <param name="filename">The output image file path encoded as UTF-8 for native OpenCV. 输出图像文件路径，传递给 native OpenCV 时使用 UTF-8。</param>
        /// <param name="image">The image to save. 要保存的图像。</param>
        /// <param name="parameters">Encoder parameters as key-value pairs. 编码参数，按键值对传入。</param>
        /// <returns><c>true</c> when OpenCV reports that the image was written. OpenCV 报告图像已写入时返回 <c>true</c>。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filename"/> is null or whitespace, or when <paramref name="parameters"/> has an odd length. 当 <paramref name="filename"/> 为空或空白，或 <paramref name="parameters"/> 长度为奇数时抛出。</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null. 当 <paramref name="image"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static bool ImWrite(string filename, Mat image, int[] parameters)
        {
            if (parameters == null)
            {
                return ImWrite(filename, image);
            }

            if ((parameters.Length % 2) != 0)
            {
                throw new ArgumentException("Encoder parameters must contain key-value pairs.", nameof(parameters));
            }

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            byte[] nativeFilename = ToNullTerminatedUtf8(filename, nameof(filename));
            return ImWriteCore(nativeFilename, image, parameters);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Decodes an image from an in-memory compressed image span.
        /// 从内存中的压缩图像 Span 解码图像。
        /// </summary>
        /// <param name="buffer">The encoded image bytes. 编码后的图像字节。</param>
        /// <param name="flags">The image read mode. 图像读取模式。</param>
        /// <returns>The decoded image. 解码后的图像。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="buffer"/> is empty. 当 <paramref name="buffer"/> 为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe Mat ImDecode(ReadOnlySpan<byte> buffer, ImreadModes flags = ImreadModes.Color)
        {
            if (buffer.Length == 0)
            {
                throw new ArgumentException("Encoded image buffer cannot be empty.", nameof(buffer));
            }

            ValidateImreadMode(flags, nameof(flags));
            fixed (byte* bufferPointer = buffer)
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsImDecode(bufferPointer, ToUIntPtr(buffer.Length), (int)flags, out IntPtr image));
                return new Mat(image);
            }
        }
#endif

        private static void ValidateImreadMode(ImreadModes value, string parameterName)
        {
            if (value == ImreadModes.Unchanged)
            {
                return;
            }

            const ImreadModes allowed =
                ImreadModes.Color |
                ImreadModes.AnyDepth |
                ImreadModes.AnyColor |
                ImreadModes.LoadGdal |
                ImreadModes.ReducedGrayscale2 |
                ImreadModes.ReducedGrayscale4 |
                ImreadModes.ReducedGrayscale8 |
                ImreadModes.IgnoreOrientation |
                ImreadModes.ColorRgb;

            if ((value < 0) || ((value & ~allowed) != 0))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported image read mode.");
            }
        }

        private static UIntPtr ToUIntPtr(int value)
        {
            return new UIntPtr((uint)value);
        }

        private static byte[] ToNullTerminatedUtf8(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Path cannot be null or whitespace.", parameterName);
            }

            int byteCount = Encoding.UTF8.GetByteCount(value);
            var buffer = new byte[byteCount + 1];
            Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);
            return buffer;
        }

        private static byte[] CopyEncodedBufferToArray(NativeEncodedBufferHandle buffer)
        {
            IntPtr bufferHandle = buffer.DangerousGetHandle();
            NativeException.ThrowIfError(NativeMethods.EncodedBufferSize(bufferHandle, out UIntPtr size));
            NativeException.ThrowIfError(NativeMethods.EncodedBufferData(bufferHandle, out IntPtr data));

            ulong byteLength = size.ToUInt64();
            if (byteLength > int.MaxValue)
            {
                throw new OpenCvException("Encoded image byte length is larger than Int32.MaxValue.");
            }

            var managedBuffer = new byte[(int)byteLength];
            if (managedBuffer.Length == 0)
            {
                return managedBuffer;
            }

            Marshal.Copy(data, managedBuffer, 0, managedBuffer.Length);
            return managedBuffer;
        }

        private static byte[] ImEncodeCore(string ext, Mat image, int[] parameters)
        {
#if NETCOREAPP3_1_OR_GREATER
            unsafe
            {
                fixed (int* parametersPointer = parameters)
                {
                    NativeException.ThrowIfError(NativeMethods.ImgCodecsImEncodeWithParams(ext, image.NativeHandle, parametersPointer, ToUIntPtr(parameters.Length), out IntPtr nativeBuffer));
                    using (NativeEncodedBufferHandle buffer = NativeEncodedBufferHandle.FromNativePointer(nativeBuffer))
                    {
                        return CopyEncodedBufferToArray(buffer);
                    }
                }
            }
#else
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImEncodeWithParams(ext, image.NativeHandle, parameters, ToUIntPtr(parameters.Length), out IntPtr nativeBuffer));
            using (NativeEncodedBufferHandle buffer = NativeEncodedBufferHandle.FromNativePointer(nativeBuffer))
            {
                return CopyEncodedBufferToArray(buffer);
            }
#endif
        }

        private static bool ImWriteCore(byte[] nativeFilename, Mat image, int[] parameters)
        {
#if NETCOREAPP3_1_OR_GREATER
            unsafe
            {
                fixed (int* parametersPointer = parameters)
                {
                    NativeException.ThrowIfError(NativeMethods.ImgCodecsImWriteWithParams(nativeFilename, image.NativeHandle, parametersPointer, ToUIntPtr(parameters.Length), out int written));
                    return written != 0;
                }
            }
#else
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImWriteWithParams(nativeFilename, image.NativeHandle, parameters, ToUIntPtr(parameters.Length), out int written));
            return written != 0;
#endif
        }
    }
}
