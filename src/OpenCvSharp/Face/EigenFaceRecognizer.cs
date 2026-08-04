using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Face
{
    /// <summary>
    /// Eigenfaces recognizer from OpenCV contrib face.
    /// OpenCV contrib face 的 Eigenfaces 识别器。
    /// </summary>
    public sealed class EigenFaceRecognizer : BasicFaceRecognizer
    {
        private EigenFaceRecognizer(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Creates an EigenFace recognizer. 创建 EigenFace 识别器。</summary>
        public static EigenFaceRecognizer Create(int numComponents = 0, double threshold = double.MaxValue)
        {
            NativeException.ThrowIfError(NativeMethods.FaceEigenCreate(numComponents, threshold, out IntPtr nativeHandle));
            return new EigenFaceRecognizer(nativeHandle);
        }
    }
}
