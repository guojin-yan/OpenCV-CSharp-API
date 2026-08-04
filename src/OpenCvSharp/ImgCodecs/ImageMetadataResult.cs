using System;
using System.Collections.Generic;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    /// <summary>Owns a decoded image and independent clones of all metadata chunks. 拥有解码图像及所有元数据块的独立克隆。</summary>
    public sealed class ImageMetadataResult : IDisposable
    {
        private bool disposed;

        internal ImageMetadataResult(Mat image, ImageMetadataChunk[] metadata)
        {
            Image = image;
            Metadata = metadata;
        }

        /// <summary>Gets the owned decoded image. 获取拥有的解码图像。</summary>
        public Mat Image { get; }
        /// <summary>Gets the owned metadata chunks. 获取拥有的元数据块。</summary>
        public IReadOnlyList<ImageMetadataChunk> Metadata { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            Image.Dispose();
            foreach (ImageMetadataChunk chunk in Metadata) chunk.Data.Dispose();
        }
    }
}
