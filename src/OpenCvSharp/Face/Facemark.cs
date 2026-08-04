using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Face
{
    /// <summary>
    /// Base wrapper for OpenCV contrib facemark models.
    /// OpenCV contrib 人脸关键点模型基类包装。
    /// </summary>
    public class Facemark : IDisposable
    {
        private NativeFaceFacemarkHandle handle;
        private bool disposed;

        internal Facemark(IntPtr nativeHandle)
        {
            handle = NativeFaceFacemarkHandle.FromNativePointer(nativeHandle);
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

        /// <summary>
        /// Loads a trained facemark model file before fitting.
        /// 在拟合前加载训练好的 facemark 模型文件。
        /// </summary>
        public void LoadModel(string modelPath)
        {
            ThrowIfDisposed();
            byte[] nativePath = FaceStringConvert.ToNullTerminatedUtf8(modelPath, nameof(modelPath));
            NativeException.ThrowIfError(NativeMethods.FaceFacemarkLoadModel(NativeHandle, nativePath));
        }

        /// <summary>
        /// Saves the facemark model through OpenCV Algorithm serialization.
        /// 通过 OpenCV Algorithm 序列化保存 facemark 模型。
        /// </summary>
        public void Save(string path)
        {
            ThrowIfDisposed();
            byte[] nativePath = FaceStringConvert.ToNullTerminatedUtf8(path, nameof(path));
            NativeException.ThrowIfError(NativeMethods.FaceFacemarkSave(NativeHandle, nativePath));
        }

        /// <summary>
        /// Fits landmarks for the specified face rectangles.
        /// 为指定人脸矩形拟合关键点。
        /// </summary>
        public FacemarkFitResult Fit(Mat image, Rect[] faces)
        {
            ThrowIfDisposed();
            FaceRecognizer.ValidateNotNull(image, nameof(image));
            ValidateRectArray(faces, nameof(faces));

            int[] nativeFaces = ToRectBuffer(faces);
            NativeException.ThrowIfError(NativeMethods.FaceFacemarkFit(NativeHandle, image.NativeHandle, nativeFaces, faces.Length, out int success));
            NativeException.ThrowIfError(NativeMethods.FaceFacemarkFitLandmarksCount(NativeHandle, out int faceCount, out int pointCount));
            if (faceCount <= 0 || pointCount <= 0)
            {
                return new FacemarkFitResult(success != 0, Array.Empty<Point2f[]>());
            }

            var offsets = new int[faceCount + 1];
            var rawPoints = new float[pointCount * 2];
            NativeException.ThrowIfError(NativeMethods.FaceFacemarkFitLandmarksFill(
                NativeHandle,
                offsets,
                offsets.Length,
                rawPoints,
                pointCount,
                out int writtenFaces,
                out int writtenPoints));

            Point2f[] points = ToPointArray(rawPoints, Math.Max(0, Math.Min(writtenPoints, pointCount)));
            Point2f[][] groups = PointSetMarshaller.ToPoint2fGroups(offsets, points, Math.Max(0, Math.Min(writtenFaces, faceCount)));
            return new FacemarkFitResult(success != 0, groups);
        }

        /// <summary>
        /// Fits landmarks and returns whether OpenCV reported success.
        /// 拟合关键点并返回 OpenCV 是否报告成功。
        /// </summary>
        public bool Fit(Mat image, Rect[] faces, out Point2f[][] landmarks)
        {
            FacemarkFitResult result = Fit(image, faces);
            landmarks = result.Landmarks;
            return result.Success;
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static void ValidateRectArray(Rect[] values, string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        internal static int[] ToRectBuffer(Rect[] faces)
        {
            var result = new int[faces.Length * 4];
            for (int i = 0; i < faces.Length; i++)
            {
                int offset = i * 4;
                result[offset] = faces[i].X;
                result[offset + 1] = faces[i].Y;
                result[offset + 2] = faces[i].Width;
                result[offset + 3] = faces[i].Height;
            }

            return result;
        }

        internal static float[] ToPointBuffer(Point2f[] points, string parameterName)
        {
            if (points == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            var result = new float[points.Length * 2];
            for (int i = 0; i < points.Length; i++)
            {
                int offset = i * 2;
                result[offset] = points[i].X;
                result[offset + 1] = points[i].Y;
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

        private static Point2f[] ToPointArray(float[] rawPoints, int pointCount)
        {
            var result = new Point2f[pointCount];
            for (int i = 0; i < result.Length; i++)
            {
                int offset = i * 2;
                result[i] = new Point2f(rawPoints[offset], rawPoints[offset + 1]);
            }

            return result;
        }
    }
}
