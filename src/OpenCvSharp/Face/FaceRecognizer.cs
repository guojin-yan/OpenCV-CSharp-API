using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Face
{
    /// <summary>
    /// Base wrapper for OpenCV contrib traditional face recognizers.
    /// OpenCV contrib 传统人脸识别器基类包装。
    /// </summary>
    public class FaceRecognizer : IDisposable
    {
        private NativeFaceRecognizerHandle handle;
        private bool disposed;

        internal FaceRecognizer(IntPtr nativeHandle)
        {
            handle = NativeFaceRecognizerHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this recognizer has been disposed. 获取识别器是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets whether the native recognizer is empty. 获取 native 识别器是否为空。</summary>
        public bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.FaceRecognizerEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        /// <summary>Gets or sets the recognition threshold. 获取或设置识别阈值。</summary>
        public virtual double Threshold
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.FaceRecognizerGetThreshold(NativeHandle, out double threshold));
                return threshold;
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.FaceRecognizerSetThreshold(NativeHandle, value));
            }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>
        /// Trains the recognizer from image and label arrays.
        /// 使用图像和标签数组训练识别器。
        /// </summary>
        public void Train(Mat[] images, int[] labels)
        {
            ThrowIfDisposed();
            ValidateImageLabelArrays(images, labels);
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerTrain(NativeHandle, ToNativeHandles(images), images.Length, labels, labels.Length));
        }

        /// <summary>
        /// Updates the recognizer with additional image and label arrays.
        /// 使用额外图像和标签数组更新识别器。
        /// </summary>
        public void Update(Mat[] images, int[] labels)
        {
            ThrowIfDisposed();
            ValidateImageLabelArrays(images, labels);
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerUpdate(NativeHandle, ToNativeHandles(images), images.Length, labels, labels.Length));
        }

        /// <summary>
        /// Predicts a label for an image.
        /// 预测图像标签。
        /// </summary>
        public int Predict(Mat image)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerPredictLabel(NativeHandle, image.NativeHandle, out int label));
            return label;
        }

        /// <summary>
        /// Predicts a label and confidence for an image.
        /// 预测图像标签和置信度。
        /// </summary>
        public FacePrediction PredictWithConfidence(Mat image)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerPredict(NativeHandle, image.NativeHandle, out int label, out double confidence));
            return new FacePrediction(label, confidence);
        }

        /// <summary>
        /// Predicts into a collector.
        /// 将预测结果写入 collector。
        /// </summary>
        public void Predict(Mat image, StandardCollector collector)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(collector, nameof(collector));
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerPredictCollect(NativeHandle, image.NativeHandle, collector.NativeHandle));
        }

        /// <summary>
        /// Reads recognizer state from a file.
        /// 从文件读取识别器状态。
        /// </summary>
        public void Read(string path)
        {
            ThrowIfDisposed();
            byte[] nativePath = FaceStringConvert.ToNullTerminatedUtf8(path, nameof(path));
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerRead(NativeHandle, nativePath));
        }

        /// <summary>
        /// Writes recognizer state to a file.
        /// 将识别器状态写入文件。
        /// </summary>
        public void Write(string path)
        {
            ThrowIfDisposed();
            byte[] nativePath = FaceStringConvert.ToNullTerminatedUtf8(path, nameof(path));
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerWrite(NativeHandle, nativePath));
        }

        /// <summary>
        /// Sets label information text.
        /// 设置标签说明文本。
        /// </summary>
        public void SetLabelInfo(int label, string info)
        {
            ThrowIfDisposed();
            byte[] nativeInfo = FaceStringConvert.ToNullTerminatedUtf8(info, nameof(info));
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerSetLabelInfo(NativeHandle, label, nativeInfo));
        }

        /// <summary>
        /// Gets label information text.
        /// 获取标签说明文本。
        /// </summary>
        public unsafe string GetLabelInfo(int label)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerGetLabelInfoLength(NativeHandle, label, out int length));
            if (length <= 0)
            {
                return string.Empty;
            }

            var buffer = new byte[length];
            fixed (byte* bufferPtr = buffer)
            {
                NativeException.ThrowIfError(NativeMethods.FaceRecognizerGetLabelInfoFill(NativeHandle, label, bufferPtr, buffer.Length, out int written));
                return DecodeUtf8(buffer, written);
            }
        }

        /// <summary>
        /// Gets labels whose text contains a substring.
        /// 获取说明文本包含指定子串的标签。
        /// </summary>
        public int[] GetLabelsByString(string substring)
        {
            ThrowIfDisposed();
            byte[] nativeSubstring = FaceStringConvert.ToNullTerminatedUtf8(substring, nameof(substring));
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerGetLabelsByStringCount(NativeHandle, nativeSubstring, out int count));
            if (count <= 0)
            {
                return Array.Empty<int>();
            }

            var labels = new int[count];
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerGetLabelsByStringFill(NativeHandle, nativeSubstring, labels, labels.Length, out int written));
            return Trim(labels, written);
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        internal static void ValidateImageLabelArrays(Mat[] images, int[] labels)
        {
            ValidateNotNull(images, nameof(images));
            ValidateNotNull(labels, nameof(labels));
            if (images.Length != labels.Length)
            {
                throw new ArgumentException("Image and label arrays must have the same length.", nameof(labels));
            }

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == null)
                {
                    throw new ArgumentNullException(nameof(images));
                }
            }
        }

        internal static IntPtr[] ToNativeHandles(Mat[] mats)
        {
            var handles = new IntPtr[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                handles[i] = mats[i].NativeHandle;
            }

            return handles;
        }

        internal static Mat[] ToMatArray(IntPtr[] nativeHandles, int count)
        {
            int resultCount = Math.Max(0, Math.Min(count, nativeHandles.Length));
            if (resultCount == 0)
            {
                return Array.Empty<Mat>();
            }

            var result = new Mat[resultCount];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new Mat(nativeHandles[i]);
            }

            return result;
        }

        /// <summary>Throws when disposed. 已释放时抛出异常。</summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>Disposes this instance. 释放当前实例。</summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && handle != null)
                {
                    handle.Dispose();
                }

                disposed = true;
            }
        }

        private static string DecodeUtf8(byte[] buffer, int written)
        {
            int count = Math.Max(0, Math.Min(written, buffer.Length));
            return count == 0 ? string.Empty : System.Text.Encoding.UTF8.GetString(buffer, 0, count);
        }

        private static int[] Trim(int[] values, int count)
        {
            int resultCount = Math.Max(0, Math.Min(count, values.Length));
            if (resultCount == values.Length)
            {
                return values;
            }

            var result = new int[resultCount];
            Array.Copy(values, result, result.Length);
            return result;
        }
    }
}
