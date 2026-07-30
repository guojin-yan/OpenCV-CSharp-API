using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.ImgCodecs
{
    /// <summary>Pairs an image metadata type with its matrix-backed payload. 将元数据类型与矩阵负载配对。</summary>
    public sealed class ImageMetadataChunk
    {
        /// <summary>Creates a typed metadata chunk without taking ownership of input data. 创建类型化元数据块但不接管输入数据。</summary>
        public ImageMetadataChunk(ImageMetadataType type, Mat data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Type = type;
        }

        /// <summary>Gets the metadata type. 获取元数据类型。</summary>
        public ImageMetadataType Type { get; }
        /// <summary>Gets the matrix-backed payload. 获取矩阵负载。</summary>
        public Mat Data { get; }
    }
}
