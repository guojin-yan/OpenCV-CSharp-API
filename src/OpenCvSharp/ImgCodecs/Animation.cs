using System;
using System.Collections.Generic;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    /// <summary>Owns an OpenCV animation and exposes cloned frames. 拥有 OpenCV 动画并公开克隆帧。</summary>
    public sealed class Animation : IDisposable
    {
        private readonly NativeImgCodecsAnimationHandle handle;

        /// <summary>Creates an animation with a transparent background. 创建透明背景动画。</summary>
        public Animation(int loopCount = 0)
            : this(loopCount, new Scalar())
        {
        }

        /// <summary>Creates an animation with a loop count and BGRA background. 创建具有循环次数和 BGRA 背景的动画。</summary>
        public Animation(int loopCount, Scalar backgroundColor)
        {
            NativeException.ThrowIfError(NativeMethods.ImgCodecsAnimationCreate(
                loopCount,
                backgroundColor.V0,
                backgroundColor.V1,
                backgroundColor.V2,
                backgroundColor.V3,
                out IntPtr value));
            handle = NativeImgCodecsAnimationHandle.FromNativePointer(value);
        }

        /// <summary>Gets whether this animation has been disposed. 获取动画是否已释放。</summary>
        public bool IsDisposed { get { return handle.IsClosed; } }

        /// <summary>Gets or sets the normalized loop count; zero means infinite. 获取或设置规范化循环次数，零表示无限。</summary>
        public int LoopCount
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsAnimationGetLoopCount(NativeHandle, out int value));
                return value;
            }
            set { NativeException.ThrowIfError(NativeMethods.ImgCodecsAnimationSetLoopCount(NativeHandle, value)); }
        }

        /// <summary>Gets or sets the BGRA background color. 获取或设置 BGRA 背景色。</summary>
        public Scalar BackgroundColor
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsAnimationGetBackgroundColor(
                    NativeHandle, out double v0, out double v1, out double v2, out double v3));
                return new Scalar(v0, v1, v2, v3);
            }
            set
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsAnimationSetBackgroundColor(
                    NativeHandle, value.V0, value.V1, value.V2, value.V3));
            }
        }

        /// <summary>Gets the number of frames. 获取帧数。</summary>
        public int FrameCount
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsAnimationFrameCount(NativeHandle, out UIntPtr count));
                return CheckedCount(count, "Animation frame count");
            }
        }

        /// <summary>Gets an owned clone or assigns a cloned still image. 获取拥有的克隆，或设置克隆后的静态图像。</summary>
        public Mat StillImage
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsAnimationStillImageClone(NativeHandle, out IntPtr image));
                return new Mat(image);
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                NativeException.ThrowIfError(NativeMethods.ImgCodecsAnimationSetStillImage(NativeHandle, value.NativeHandle));
            }
        }

        /// <summary>Returns an independently owned frame clone and duration. 返回独立拥有的帧克隆及持续时间。</summary>
        public AnimationFrame GetFrame(int index)
        {
            if (index < 0 || index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
            NativeException.ThrowIfError(NativeMethods.ImgCodecsAnimationFrameCloneAt(
                NativeHandle, new UIntPtr((uint)index), out IntPtr frame, out int duration));
            return new AnimationFrame(new Mat(frame), duration);
        }

        /// <summary>Deep-copies frames and matching durations into the native animation. 将帧及对应持续时间深拷贝到原生动画。</summary>
        public unsafe void SetFrames(IReadOnlyList<Mat> frames, IReadOnlyList<int> durationsMilliseconds)
        {
            if (frames == null) throw new ArgumentNullException(nameof(frames));
            if (durationsMilliseconds == null) throw new ArgumentNullException(nameof(durationsMilliseconds));
            if (frames.Count == 0) throw new ArgumentException("At least one frame is required.", nameof(frames));
            if (frames.Count != durationsMilliseconds.Count) throw new ArgumentException("Frame and duration counts must match.", nameof(durationsMilliseconds));

            var nativeFrames = new IntPtr[frames.Count];
            var durations = new int[frames.Count];
            for (int index = 0; index < frames.Count; ++index)
            {
                Mat frame = frames[index] ?? throw new ArgumentException("Frames cannot contain null values.", nameof(frames));
                nativeFrames[index] = frame.NativeHandle;
                durations[index] = durationsMilliseconds[index];
                if (durations[index] < 0) throw new ArgumentOutOfRangeException(nameof(durationsMilliseconds), "Durations cannot be negative.");
            }

            fixed (IntPtr* framesPointer = nativeFrames)
            fixed (int* durationsPointer = durations)
            {
                NativeException.ThrowIfError(NativeMethods.ImgCodecsAnimationSetFrames(
                    NativeHandle,
                    (IntPtr)framesPointer,
                    (IntPtr)durationsPointer,
                    new UIntPtr((uint)nativeFrames.Length)));
            }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                if (handle.IsClosed || handle.IsInvalid) throw new ObjectDisposedException(nameof(Animation));
                return handle.DangerousGetHandle();
            }
        }

        /// <inheritdoc/>
        public void Dispose() { handle.Dispose(); }

        private static int CheckedCount(UIntPtr value, string name)
        {
            ulong count = value.ToUInt64();
            if (count > int.MaxValue) throw new OpenCvException(name + " is larger than Int32.MaxValue.");
            return (int)count;
        }
    }
}
