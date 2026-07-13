using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Face
{
    /// <summary>
    /// Fisherfaces recognizer from OpenCV contrib face.
    /// OpenCV contrib face 的 Fisherfaces 识别器。
    /// </summary>
    public sealed class FisherFaceRecognizer : BasicFaceRecognizer
    {
        private FisherFaceRecognizer(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates a FisherFace recognizer. 创建 FisherFace 识别器。</summary>
        public static FisherFaceRecognizer Create(int numComponents = 0, double threshold = double.MaxValue)
        {
            NativeException.ThrowIfError(NativeMethods.FaceFisherCreate(numComponents, threshold, out IntPtr nativeHandle));
            return new FisherFaceRecognizer(nativeHandle);
        }
    }
}
