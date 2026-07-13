using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XObjDetect
{
    /// <summary>
    /// Contrib cascade classifier from OpenCV <c>xobjdetect</c>.
    /// OpenCV <c>xobjdetect</c> contrib 级联分类器。
    /// </summary>
    public sealed class CascadeClassifier : IDisposable
    {
        private NativeCascadeClassifierHandle handle;
        private bool disposed;

        /// <summary>Initializes an empty cascade classifier. 初始化空级联分类器。</summary>
        public CascadeClassifier()
        {
            NativeException.ThrowIfError(NativeMethods.CascadeClassifierCreate(out IntPtr nativeHandle));
            handle = NativeCascadeClassifierHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Initializes a cascade classifier from a file. 从文件初始化级联分类器。</summary>
        public CascadeClassifier(string filename)
        {
            byte[] path = XObjDetectStringConvert.ToNullTerminatedUtf8(filename, nameof(filename));
            NativeException.ThrowIfError(NativeMethods.CascadeClassifierCreateFromFile(path, out IntPtr nativeHandle));
            handle = NativeCascadeClassifierHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this classifier has been disposed. 获取分类器是否已经释放。</summary>
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

        /// <summary>Gets whether the classifier has no loaded cascade. 获取分类器是否未加载 cascade。</summary>
        public bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.CascadeClassifierEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        /// <summary>Loads a cascade file. 加载 cascade 文件。</summary>
        public bool Load(string filename)
        {
            ThrowIfDisposed();
            byte[] path = XObjDetectStringConvert.ToNullTerminatedUtf8(filename, nameof(filename));
            NativeException.ThrowIfError(NativeMethods.CascadeClassifierLoad(NativeHandle, path, out int loaded));
            return loaded != 0;
        }

        /// <summary>Gets original detection window size. 获取原始检测窗口尺寸。</summary>
        public Size GetOriginalWindowSize()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.CascadeClassifierGetOriginalWindowSize(NativeHandle, out int width, out int height));
            return new Size(width, height);
        }

        /// <summary>Returns whether the loaded cascade uses the old format. 返回加载的 cascade 是否为旧格式。</summary>
        public bool IsOldFormatCascade()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.CascadeClassifierIsOldFormatCascade(NativeHandle, out int result));
            return result != 0;
        }

        /// <summary>Gets the feature type used by the cascade. 获取 cascade 使用的特征类型。</summary>
        public int GetFeatureType()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.CascadeClassifierGetFeatureType(NativeHandle, out int featureType));
            return featureType;
        }

        /// <summary>Detects objects at multiple scales. 多尺度检测目标。</summary>
        public Rect[] DetectMultiScale(Mat image, double scaleFactor = 1.1, int minNeighbors = 3, CascadeClassifierFlags flags = CascadeClassifierFlags.None, Size minSize = default, Size maxSize = default)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.CascadeClassifierDetectMultiScaleCount(NativeHandle, image.NativeHandle, scaleFactor, minNeighbors, (int)flags, minSize.Width, minSize.Height, maxSize.Width, maxSize.Height, out int count));
            if (count <= 0)
            {
                return Array.Empty<Rect>();
            }

            var raw = new int[count * 4];
            NativeException.ThrowIfError(NativeMethods.CascadeClassifierDetectMultiScaleFill(NativeHandle, image.NativeHandle, scaleFactor, minNeighbors, (int)flags, minSize.Width, minSize.Height, maxSize.Width, maxSize.Height, raw, raw.Length, out int written));
            return ToRectangles(raw, Math.Min(written, count));
        }

        /// <summary>Detects objects and returns detection counts. 检测目标并返回检测次数。</summary>
        public CascadeDetectionResult DetectMultiScale2(Mat image, double scaleFactor = 1.1, int minNeighbors = 3, CascadeClassifierFlags flags = CascadeClassifierFlags.None, Size minSize = default, Size maxSize = default)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.CascadeClassifierDetectMultiScale2Count(NativeHandle, image.NativeHandle, scaleFactor, minNeighbors, (int)flags, minSize.Width, minSize.Height, maxSize.Width, maxSize.Height, out int count));
            if (count <= 0)
            {
                return new CascadeDetectionResult(Array.Empty<Rect>(), Array.Empty<int>(), Array.Empty<double>());
            }

            var raw = new int[count * 4];
            var detections = new int[count];
            NativeException.ThrowIfError(NativeMethods.CascadeClassifierDetectMultiScale2Fill(NativeHandle, image.NativeHandle, scaleFactor, minNeighbors, (int)flags, minSize.Width, minSize.Height, maxSize.Width, maxSize.Height, raw, raw.Length, detections, detections.Length, out int written));
            return new CascadeDetectionResult(ToRectangles(raw, Math.Min(written, count)), Trim(detections, Math.Min(written, detections.Length)), Array.Empty<double>());
        }

        /// <summary>Detects objects and returns reject levels and level weights. 检测目标并返回 reject levels 和 level weights。</summary>
        public CascadeDetectionResult DetectMultiScale3(Mat image, double scaleFactor = 1.1, int minNeighbors = 3, CascadeClassifierFlags flags = CascadeClassifierFlags.None, Size minSize = default, Size maxSize = default, bool outputRejectLevels = false)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.CascadeClassifierDetectMultiScale3Count(NativeHandle, image.NativeHandle, scaleFactor, minNeighbors, (int)flags, minSize.Width, minSize.Height, maxSize.Width, maxSize.Height, outputRejectLevels ? 1 : 0, out int count));
            if (count <= 0)
            {
                return new CascadeDetectionResult(Array.Empty<Rect>(), Array.Empty<int>(), Array.Empty<double>());
            }

            var raw = new int[count * 4];
            var levels = new int[count];
            var weights = new double[count];
            NativeException.ThrowIfError(NativeMethods.CascadeClassifierDetectMultiScale3Fill(NativeHandle, image.NativeHandle, scaleFactor, minNeighbors, (int)flags, minSize.Width, minSize.Height, maxSize.Width, maxSize.Height, outputRejectLevels ? 1 : 0, raw, raw.Length, levels, levels.Length, weights, weights.Length, out int written));
            int resultCount = Math.Min(written, count);
            return new CascadeDetectionResult(ToRectangles(raw, resultCount), Trim(levels, Math.Min(resultCount, levels.Length)), Trim(weights, Math.Min(resultCount, weights.Length)));
        }

        /// <summary>Releases the native classifier. 释放 native 分类器。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static Rect[] ToRectangles(int[] raw, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<Rect>();
            }

            var result = new Rect[count];
            for (int i = 0; i < result.Length; i++)
            {
                int offset = i * 4;
                result[i] = new Rect(raw[offset], raw[offset + 1], raw[offset + 2], raw[offset + 3]);
            }

            return result;
        }

        private static int[] Trim(int[] values, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<int>();
            }

            if (count == values.Length)
            {
                return values;
            }

            var result = new int[count];
            Array.Copy(values, result, count);
            return result;
        }

        private static double[] Trim(double[] values, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<double>();
            }

            if (count == values.Length)
            {
                return values;
            }

            var result = new double[count];
            Array.Copy(values, result, count);
            return result;
        }

        private void Dispose(bool disposing)
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

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
