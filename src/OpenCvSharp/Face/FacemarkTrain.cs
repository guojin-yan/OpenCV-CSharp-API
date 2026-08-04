using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Face
{
    /// <summary>
    /// Base wrapper for trainable OpenCV contrib facemark models.
    /// 可训练 OpenCV contrib 人脸关键点模型基类包装。
    /// </summary>
    public class FacemarkTrain : Facemark
    {
        internal FacemarkTrain(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>
        /// Adds one image and landmark set to the training data.
        /// 向训练数据添加一张图像和一组关键点。
        /// </summary>
        public void AddTrainingSample(Mat image, Point2f[] landmarks)
        {
            ThrowIfDisposed();
            FaceRecognizer.ValidateNotNull(image, nameof(image));
            ValidateLandmarks(landmarks, nameof(landmarks));
            float[] nativeLandmarks = ToPointBuffer(landmarks, nameof(landmarks));
            NativeException.ThrowIfError(NativeMethods.FaceFacemarkTrainAddSample(NativeHandle, image.NativeHandle, nativeLandmarks, landmarks.Length));
        }

        /// <summary>
        /// Runs OpenCV training for samples already added to the model.
        /// 对已添加到模型中的样本运行 OpenCV 训练。
        /// </summary>
        public void Training()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.FaceFacemarkTrainTraining(NativeHandle));
        }

        /// <summary>
        /// Detects faces through the facemark model's configured detector.
        /// 通过 facemark 模型配置的检测器检测人脸。
        /// </summary>
        public Rect[] GetFaces(Mat image)
        {
            ThrowIfDisposed();
            FaceRecognizer.ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.FaceFacemarkTrainGetFacesCount(NativeHandle, image.NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<Rect>();
            }

            var nativeFaces = new int[count * 4];
            NativeException.ThrowIfError(NativeMethods.FaceFacemarkTrainGetFacesFill(NativeHandle, image.NativeHandle, nativeFaces, count, out int written));
            return ToRectArray(nativeFaces, Math.Max(0, Math.Min(written, count)));
        }

        private static void ValidateLandmarks(Point2f[] landmarks, string parameterName)
        {
            if (landmarks == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (landmarks.Length == 0)
            {
                throw new ArgumentException("Landmarks cannot be empty.", parameterName);
            }
        }

        private static Rect[] ToRectArray(int[] nativeFaces, int count)
        {
            var result = new Rect[count];
            for (int i = 0; i < result.Length; i++)
            {
                int offset = i * 4;
                result[i] = new Rect(nativeFaces[offset], nativeFaces[offset + 1], nativeFaces[offset + 2], nativeFaces[offset + 3]);
            }

            return result;
        }
    }
}
