using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.BgSegm
{
    /// <summary>
    /// Synthetic sequence generator for background-subtraction tests.
    /// 用于背景减除测试的合成序列生成器。
    /// </summary>
    public sealed class SyntheticSequenceGenerator : IDisposable
    {
        private NativeBgSegmSyntheticSequenceGeneratorHandle handle;
        private bool disposed;

        private SyntheticSequenceGenerator(IntPtr nativeHandle)
        {
            handle = NativeBgSegmSyntheticSequenceGeneratorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Creates a synthetic sequence generator. 创建合成序列生成器。</summary>
        public static SyntheticSequenceGenerator Create(Mat background, Mat objectImage, double amplitude = 2.0, double wavelength = 20.0, double wavespeed = 0.2, double objspeed = 6.0)
        {
            BgSegmBackgroundSubtractor.ValidateNotNull(background, nameof(background));
            BgSegmBackgroundSubtractor.ValidateNotNull(objectImage, nameof(objectImage));
            ValidateInputImage(background, nameof(background));
            ValidateInputImage(objectImage, nameof(objectImage));
            ValidateObjectFitsBackground(background, objectImage, nameof(objectImage));
            NativeException.ThrowIfError(NativeMethods.BgSegmSyntheticSequenceGeneratorCreate(background.NativeHandle, objectImage.NativeHandle, amplitude, wavelength, wavespeed, objspeed, out IntPtr nativeHandle));
            return new SyntheticSequenceGenerator(nativeHandle);
        }

        /// <summary>Writes the next generated frame and ground-truth mask. 写入下一帧和真值掩码。</summary>
        public void GetNextFrame(Mat frame, Mat gtMask)
        {
            ThrowIfDisposed();
            BgSegmBackgroundSubtractor.ValidateNotNull(frame, nameof(frame));
            BgSegmBackgroundSubtractor.ValidateNotNull(gtMask, nameof(gtMask));
            NativeException.ThrowIfError(NativeMethods.BgSegmSyntheticSequenceGeneratorGetNextFrame(NativeHandle, frame.NativeHandle, gtMask.NativeHandle));
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(SyntheticSequenceGenerator));
            }
        }

        private static void ValidateInputImage(Mat value, string parameterName)
        {
            if (value.Empty)
            {
                throw new ArgumentException("Image must not be empty.", parameterName);
            }

            int channels = value.Channels;
            if (channels != 1 && channels != 3)
            {
                throw new ArgumentException("Image must have one or three channels.", parameterName);
            }
        }

        private static void ValidateObjectFitsBackground(Mat background, Mat objectImage, string parameterName)
        {
            if (background.Cols <= objectImage.Cols || background.Rows <= objectImage.Rows)
            {
                throw new ArgumentException("Object image must be smaller than the background in both width and height.", parameterName);
            }
        }
    }
}
