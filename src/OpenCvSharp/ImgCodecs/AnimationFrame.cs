using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.ImgCodecs
{
    /// <summary>Owns one cloned animation frame and its duration. 拥有一个克隆动画帧及其持续时间。</summary>
    public sealed class AnimationFrame : IDisposable
    {
        internal AnimationFrame(Mat image, int durationMilliseconds)
        {
            Image = image;
            DurationMilliseconds = durationMilliseconds;
        }

        /// <summary>Gets the independently owned frame image. 获取独立拥有的帧图像。</summary>
        public Mat Image { get; }
        /// <summary>Gets the frame duration in milliseconds. 获取帧持续时间（毫秒）。</summary>
        public int DurationMilliseconds { get; }
        /// <inheritdoc/>
        public void Dispose() { Image.Dispose(); }
    }
}
