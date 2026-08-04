using System;
using System.Collections.Generic;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        /// <summary>Loads a file into a caller-owned matrix, reusing compatible storage. 将文件加载到调用方矩阵并复用兼容存储。</summary>
        public static void ImRead(string filename, Mat destination, ImreadModes flags = ImreadModes.Color)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            byte[] path = ToNullTerminatedUtf8(filename, nameof(filename));
            ValidateImreadMode(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImReadInto(path, (int)flags, destination.NativeHandle));
        }

        /// <summary>Loads every page from a multi-page image. 加载多页图像的所有页面。</summary>
        public static bool ImReadMulti(string filename, out Mat[] images, ImreadModes flags = ImreadModes.Color)
        {
            byte[] path = ToNullTerminatedUtf8(filename, nameof(filename));
            ValidateImreadMode(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImReadMulti(path, (int)flags, 0, 0, 0, out IntPtr value, out int success));
            images = CopyMatVector(value);
            return success != 0;
        }

        /// <summary>Loads a page range expressed as start and count. 按起点和数量加载页面范围。</summary>
        public static bool ImReadMulti(string filename, int start, int count, out Mat[] images, ImreadModes flags = ImreadModes.AnyColor)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (start > int.MaxValue - count) throw new ArgumentOutOfRangeException(nameof(count));
            byte[] path = ToNullTerminatedUtf8(filename, nameof(filename));
            ValidateImreadMode(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImReadMulti(path, (int)flags, 1, start, count, out IntPtr value, out int success));
            images = CopyMatVector(value);
            return success != 0;
        }

        /// <summary>Decodes every page from an encoded memory buffer. 从编码内存缓冲区解码所有页面。</summary>
        public static bool ImDecodeMulti(byte[] buffer, out Mat[] images, ImreadModes flags = ImreadModes.AnyColor)
        {
            ValidateEncodedBuffer(buffer);
            ValidateImreadMode(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImDecodeMulti(
                buffer, ToUIntPtr(buffer.Length), (int)flags, 0, 0, 0, out IntPtr value, out int success));
            images = CopyMatVector(value);
            return success != 0;
        }

        /// <summary>Decodes the half-open page range [start, end). 解码半开页面范围 [start, end)。</summary>
        public static bool ImDecodeMulti(byte[] buffer, int start, int end, out Mat[] images, ImreadModes flags = ImreadModes.AnyColor)
        {
            ValidateEncodedBuffer(buffer);
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (end <= start) throw new ArgumentOutOfRangeException(nameof(end));
            ValidateImreadMode(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImDecodeMulti(
                buffer, ToUIntPtr(buffer.Length), (int)flags, 1, start, end, out IntPtr value, out int success));
            images = CopyMatVector(value);
            return success != 0;
        }

        /// <summary>Writes multiple images to a multi-page file. 将多个图像写入多页文件。</summary>
        public static unsafe bool ImWriteMulti(string filename, IReadOnlyList<Mat> images, int[]? parameters = null)
        {
            byte[] path = ToNullTerminatedUtf8(filename, nameof(filename));
            IntPtr[] handles = GetMatHandles(images, nameof(images));
            int[] nativeParameters = ValidateParameters(parameters);
            fixed (IntPtr* imagesPointer = handles)
            fixed (int* parametersPointer = nativeParameters)
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsImWriteMulti(
                    path,
                    (IntPtr)imagesPointer,
                    ToUIntPtr(handles.Length),
                    (IntPtr)parametersPointer,
                    ToUIntPtr(nativeParameters.Length),
                    out int written));
                return written != 0;
            }
        }

        /// <summary>Encodes multiple images into one multi-page memory buffer. 将多个图像编码到一个多页内存缓冲区。</summary>
        public static unsafe byte[] ImEncodeMulti(string ext, IReadOnlyList<Mat> images, int[]? parameters = null)
        {
            ValidateExtension(ext);
            IntPtr[] handles = GetMatHandles(images, nameof(images));
            int[] nativeParameters = ValidateParameters(parameters);
            fixed (IntPtr* imagesPointer = handles)
            fixed (int* parametersPointer = nativeParameters)
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsImEncodeMulti(
                    ext,
                    (IntPtr)imagesPointer,
                    ToUIntPtr(handles.Length),
                    (IntPtr)parametersPointer,
                    ToUIntPtr(nativeParameters.Length),
                    out IntPtr value));
                using (NativeEncodedBufferHandle buffer = NativeEncodedBufferHandle.FromNativePointer(value))
                {
                    return CopyEncodedBufferToArray(buffer);
                }
            }
        }

        /// <summary>Returns the number of decodable pages or animation frames in a file. 返回文件中可解码页面或动画帧的数量。</summary>
        public static int ImCount(string filename, ImreadModes flags = ImreadModes.Color)
        {
            byte[] path = ToNullTerminatedUtf8(filename, nameof(filename));
            ValidateImreadMode(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImCount(path, (int)flags, out UIntPtr value));
            return CheckedCount(value, "Image count");
        }

        /// <summary>Reports whether the file can be decoded by the linked runtime. 报告链接运行时能否解码该文件。</summary>
        public static bool HaveImageReader(string filename)
        {
            byte[] path = ToNullTerminatedUtf8(filename, nameof(filename));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsHaveImageReader(path, out int available));
            return available != 0;
        }

        /// <summary>Reports whether the extension can be encoded by the linked runtime. 报告链接运行时能否编码该扩展名。</summary>
        public static bool HaveImageWriter(string filenameOrExtension)
        {
            byte[] value = ToNullTerminatedUtf8(filenameOrExtension, nameof(filenameOrExtension));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsHaveImageWriter(value, out int available));
            return available != 0;
        }

        /// <summary>Loads an image and independently owned metadata chunks from a file. 从文件加载图像及独立拥有的元数据块。</summary>
        public static ImageMetadataResult ImReadWithMetadata(string filename, ImreadModes flags = ImreadModes.Unchanged)
        {
            byte[] path = ToNullTerminatedUtf8(filename, nameof(filename));
            ValidateImreadMode(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImReadWithMetadata(path, (int)flags, out IntPtr value));
            return CopyMetadataResult(value);
        }

        /// <summary>Decodes an image and independently owned metadata chunks from memory. 从内存解码图像及独立拥有的元数据块。</summary>
        public static ImageMetadataResult ImDecodeWithMetadata(byte[] buffer, ImreadModes flags = ImreadModes.Unchanged)
        {
            ValidateEncodedBuffer(buffer);
            ValidateImreadMode(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImDecodeWithMetadata(
                buffer, ToUIntPtr(buffer.Length), (int)flags, out IntPtr value));
            return CopyMetadataResult(value);
        }

        /// <summary>Writes an image with typed metadata chunks. 写入带类型化元数据块的图像。</summary>
        public static unsafe bool ImWriteWithMetadata(
            string filename,
            Mat image,
            IReadOnlyList<ImageMetadataChunk> metadata,
            int[]? parameters = null)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            byte[] path = ToNullTerminatedUtf8(filename, nameof(filename));
            PrepareMetadata(metadata, out int[] types, out IntPtr[] chunks);
            int[] nativeParameters = ValidateParameters(parameters);
            fixed (int* typesPointer = types)
            fixed (IntPtr* chunksPointer = chunks)
            fixed (int* parametersPointer = nativeParameters)
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsImWriteWithMetadata(
                    path,
                    image.NativeHandle,
                    (IntPtr)typesPointer,
                    (IntPtr)chunksPointer,
                    ToUIntPtr(types.Length),
                    (IntPtr)parametersPointer,
                    ToUIntPtr(nativeParameters.Length),
                    out int written));
                return written != 0;
            }
        }

        /// <summary>Encodes an image with typed metadata chunks. 编码带类型化元数据块的图像。</summary>
        public static unsafe byte[] ImEncodeWithMetadata(
            string ext,
            Mat image,
            IReadOnlyList<ImageMetadataChunk> metadata,
            int[]? parameters = null)
        {
            ValidateExtension(ext);
            if (image == null) throw new ArgumentNullException(nameof(image));
            PrepareMetadata(metadata, out int[] types, out IntPtr[] chunks);
            int[] nativeParameters = ValidateParameters(parameters);
            fixed (int* typesPointer = types)
            fixed (IntPtr* chunksPointer = chunks)
            fixed (int* parametersPointer = nativeParameters)
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsImEncodeWithMetadata(
                    ext,
                    image.NativeHandle,
                    (IntPtr)typesPointer,
                    (IntPtr)chunksPointer,
                    ToUIntPtr(types.Length),
                    (IntPtr)parametersPointer,
                    ToUIntPtr(nativeParameters.Length),
                    out IntPtr value));
                using (NativeEncodedBufferHandle buffer = NativeEncodedBufferHandle.FromNativePointer(value))
                {
                    return CopyEncodedBufferToArray(buffer);
                }
            }
        }

        /// <summary>Loads animation frames from a file into an existing animation. 从文件加载动画帧到现有动画。</summary>
        public static bool ImReadAnimation(string filename, Animation animation, int start = 0, int count = short.MaxValue)
        {
            if (animation == null) throw new ArgumentNullException(nameof(animation));
            ValidateAnimationRange(start, count);
            byte[] path = ToNullTerminatedUtf8(filename, nameof(filename));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImReadAnimation(path, start, count, animation.NativeHandle, out int success));
            return success != 0;
        }

        /// <summary>Decodes animation frames from memory into an existing animation. 从内存解码动画帧到现有动画。</summary>
        public static bool ImDecodeAnimation(byte[] buffer, Animation animation, int start = 0, int count = short.MaxValue)
        {
            ValidateEncodedBuffer(buffer);
            if (animation == null) throw new ArgumentNullException(nameof(animation));
            ValidateAnimationRange(start, count);
            NativeException.ThrowIfError(NativeMethods.ImgCodecsImDecodeAnimation(
                buffer, ToUIntPtr(buffer.Length), start, count, animation.NativeHandle, out int success));
            return success != 0;
        }

        /// <summary>Writes an animation to a file. 将动画写入文件。</summary>
        public static unsafe bool ImWriteAnimation(string filename, Animation animation, int[]? parameters = null)
        {
            if (animation == null) throw new ArgumentNullException(nameof(animation));
            byte[] path = ToNullTerminatedUtf8(filename, nameof(filename));
            int[] nativeParameters = ValidateParameters(parameters);
            fixed (int* parametersPointer = nativeParameters)
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsImWriteAnimation(
                    path,
                    animation.NativeHandle,
                    (IntPtr)parametersPointer,
                    ToUIntPtr(nativeParameters.Length),
                    out int written));
                return written != 0;
            }
        }

        /// <summary>Encodes an animation into memory. 将动画编码到内存。</summary>
        public static unsafe byte[] ImEncodeAnimation(string ext, Animation animation, int[]? parameters = null)
        {
            ValidateExtension(ext);
            if (animation == null) throw new ArgumentNullException(nameof(animation));
            int[] nativeParameters = ValidateParameters(parameters);
            fixed (int* parametersPointer = nativeParameters)
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsImEncodeAnimation(
                    ext,
                    animation.NativeHandle,
                    (IntPtr)parametersPointer,
                    ToUIntPtr(nativeParameters.Length),
                    out IntPtr value));
                using (NativeEncodedBufferHandle buffer = NativeEncodedBufferHandle.FromNativePointer(value))
                {
                    return CopyEncodedBufferToArray(buffer);
                }
            }
        }

        internal static byte[] ToNullTerminatedUtf8Value(string value, string parameterName)
        {
            return ToNullTerminatedUtf8(value, parameterName);
        }

        internal static void ValidateImreadModeValue(ImreadModes value, string parameterName)
        {
            ValidateImreadMode(value, parameterName);
        }

        private static void ValidateAnimationRange(int start, int count)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        }

        private static void ValidateEncodedBuffer(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (buffer.Length == 0) throw new ArgumentException("Encoded image buffer cannot be empty.", nameof(buffer));
        }

        private static void ValidateExtension(string ext)
        {
            if (string.IsNullOrWhiteSpace(ext)) throw new ArgumentException("Image extension cannot be null or whitespace.", nameof(ext));
        }

        private static int[] ValidateParameters(int[]? parameters)
        {
            if (parameters == null) return Array.Empty<int>();
            if ((parameters.Length % 2) != 0) throw new ArgumentException("Encoder parameters must contain key-value pairs.", nameof(parameters));
            return parameters;
        }

        private static IntPtr[] GetMatHandles(IReadOnlyList<Mat> images, string parameterName)
        {
            if (images == null) throw new ArgumentNullException(parameterName);
            if (images.Count == 0) throw new ArgumentException("At least one image is required.", parameterName);
            var result = new IntPtr[images.Count];
            for (int index = 0; index < images.Count; ++index)
            {
                Mat image = images[index] ?? throw new ArgumentException("Image lists cannot contain null values.", parameterName);
                result[index] = image.NativeHandle;
            }
            return result;
        }

        private static void PrepareMetadata(
            IReadOnlyList<ImageMetadataChunk> metadata,
            out int[] types,
            out IntPtr[] chunks)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            types = new int[metadata.Count];
            chunks = new IntPtr[metadata.Count];
            for (int index = 0; index < metadata.Count; ++index)
            {
                ImageMetadataChunk chunk = metadata[index] ?? throw new ArgumentException("Metadata cannot contain null values.", nameof(metadata));
                types[index] = (int)chunk.Type;
                chunks[index] = chunk.Data.NativeHandle;
            }
        }

        private static Mat[] CopyMatVector(IntPtr value)
        {
            using (NativeImgCodecsMatVectorHandle vector = NativeImgCodecsMatVectorHandle.FromNativePointer(value))
            {
                IntPtr vectorHandle = vector.DangerousGetHandle();
                NativeException.ThrowIfError(NativeMethods.ImgCodecsMatVectorCount(vectorHandle, out UIntPtr nativeCount));
                int count = CheckedCount(nativeCount, "Image vector count");
                var images = new Mat[count];
                try
                {
                    for (int index = 0; index < count; ++index)
                    {
                        NativeException.ThrowIfError(NativeMethods.ImgCodecsMatVectorCloneAt(
                            vectorHandle, new UIntPtr((uint)index), out IntPtr image));
                        images[index] = new Mat(image);
                    }
                    return images;
                }
                catch
                {
                    for (int index = 0; index < images.Length; ++index) images[index]?.Dispose();
                    throw;
                }
            }
        }

        private static ImageMetadataResult CopyMetadataResult(IntPtr value)
        {
            using (NativeImgCodecsMetadataResultHandle result = NativeImgCodecsMetadataResultHandle.FromNativePointer(value))
            {
                IntPtr resultHandle = result.DangerousGetHandle();
                NativeException.ThrowIfError(NativeMethods.ImgCodecsMetadataResultImageClone(resultHandle, out IntPtr imageValue));
                var image = new Mat(imageValue);
                ImageMetadataChunk[] chunks = Array.Empty<ImageMetadataChunk>();
                try
                {
                    NativeException.ThrowIfError(NativeMethods.ImgCodecsMetadataResultCount(resultHandle, out UIntPtr nativeCount));
                    int count = CheckedCount(nativeCount, "Metadata count");
                    chunks = new ImageMetadataChunk[count];
                    for (int index = 0; index < count; ++index)
                    {
                        NativeException.ThrowIfError(NativeMethods.ImgCodecsMetadataResultCloneAt(
                            resultHandle, new UIntPtr((uint)index), out int type, out IntPtr metadata));
                        chunks[index] = new ImageMetadataChunk((ImageMetadataType)type, new Mat(metadata));
                    }
                    return new ImageMetadataResult(image, chunks);
                }
                catch
                {
                    image.Dispose();
                    for (int index = 0; index < chunks.Length; ++index) chunks[index]?.Data.Dispose();
                    throw;
                }
            }
        }

        private static int CheckedCount(UIntPtr value, string name)
        {
            ulong count = value.ToUInt64();
            if (count > int.MaxValue) throw new OpenCvException(name + " is larger than Int32.MaxValue.");
            return (int)count;
        }
    }
}
