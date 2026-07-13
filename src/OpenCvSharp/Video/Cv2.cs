using System;
using System.Text;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Video
{
    /// <summary>
    /// OpenCV video tracking and motion-analysis functions.
    /// OpenCV video 跟踪与运动分析函数。
    /// </summary>
    public static unsafe class Cv2
    {
        private static readonly TermCriteria DefaultLkCriteria = TermCriteria.ByCountAndEpsilon(30, 0.01);
        private static readonly TermCriteria DefaultShiftCriteria = TermCriteria.ByCountAndEpsilon(10, 1.0);

        /// <summary>
        /// Calculates sparse Lucas-Kanade optical flow for a point set.
        /// 为点集计算稀疏 Lucas-Kanade 光流。
        /// </summary>
        public static Point2f[] CalcOpticalFlowPyrLK(
            Mat prevImg,
            Mat nextImg,
            Point2f[] prevPts,
            out byte[] status,
            out float[] err,
            Size? winSize = null,
            int maxLevel = 3,
            TermCriteria? criteria = null,
            OpticalFlowFlags flags = OpticalFlowFlags.None,
            double minEigThreshold = 1e-4)
        {
            ValidateNotNull(prevPts, nameof(prevPts));
            return CalcOpticalFlowPyrLKCore(prevImg, nextImg, prevPts, Array.Empty<Point2f>(), out status, out err, winSize, maxLevel, criteria, flags, minEigThreshold);
        }

        /// <summary>
        /// Calculates sparse Lucas-Kanade optical flow with optional initial flow.
        /// 计算可带初始估计的稀疏 Lucas-Kanade 光流。
        /// </summary>
        public static Point2f[] CalcOpticalFlowPyrLK(
            Mat prevImg,
            Mat nextImg,
            Point2f[] prevPts,
            Point2f[] initialNextPts,
            out byte[] status,
            out float[] err,
            Size? winSize = null,
            int maxLevel = 3,
            TermCriteria? criteria = null,
            OpticalFlowFlags flags = OpticalFlowFlags.UseInitialFlow,
            double minEigThreshold = 1e-4)
        {
            ValidateNotNull(prevPts, nameof(prevPts));
            ValidateNotNull(initialNextPts, nameof(initialNextPts));
            return CalcOpticalFlowPyrLKCore(prevImg, nextImg, prevPts, initialNextPts, out status, out err, winSize, maxLevel, criteria, flags, minEigThreshold);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Calculates sparse Lucas-Kanade optical flow from span-backed point data.
        /// 使用 Span 支持的点数据计算稀疏 Lucas-Kanade 光流。
        /// </summary>
        public static Point2f[] CalcOpticalFlowPyrLK(
            Mat prevImg,
            Mat nextImg,
            ReadOnlySpan<Point2f> prevPts,
            out byte[] status,
            out float[] err,
            Size? winSize = null,
            int maxLevel = 3,
            TermCriteria? criteria = null,
            OpticalFlowFlags flags = OpticalFlowFlags.None,
            double minEigThreshold = 1e-4)
        {
            return CalcOpticalFlowPyrLKCore(prevImg, nextImg, prevPts.ToArray(), Array.Empty<Point2f>(), out status, out err, winSize, maxLevel, criteria, flags, minEigThreshold);
        }
#endif

        /// <summary>
        /// Computes dense Farneback optical flow.
        /// 计算密集 Farneback 光流。
        /// </summary>
        public static void CalcOpticalFlowFarneback(
            Mat prev,
            Mat next,
            Mat flow,
            double pyrScale,
            int levels,
            int winsize,
            int iterations,
            int polyN,
            double polySigma,
            OpticalFlowFlags flags = OpticalFlowFlags.None)
        {
            ValidateNotNull(prev, nameof(prev));
            ValidateNotNull(next, nameof(next));
            ValidateNotNull(flow, nameof(flow));
            ValidateOpticalFlowFlags(flags, nameof(flags));
            NativeException.ThrowIfError(NativeMethods.VideoCalcOpticalFlowFarneback(
                prev.NativeHandle,
                next.NativeHandle,
                flow.NativeHandle,
                pyrScale,
                levels,
                winsize,
                iterations,
                polyN,
                polySigma,
                (int)flags));
        }

        /// <summary>
        /// Reads an optical-flow field from a <c>.flo</c> file.
        /// 从 <c>.flo</c> 文件读取光流场。
        /// </summary>
        public static Mat ReadOpticalFlow(string path)
        {
            byte[] nativePath = ToNullTerminatedUtf8(path, nameof(path));
            NativeException.ThrowIfError(NativeMethods.VideoReadOpticalFlow(nativePath, out IntPtr nativeHandle));
            return new Mat(nativeHandle);
        }

        /// <summary>
        /// Writes an optical-flow field to a <c>.flo</c> file.
        /// 将光流场写入 <c>.flo</c> 文件。
        /// </summary>
        public static bool WriteOpticalFlow(string path, Mat flow)
        {
            ValidateNotNull(flow, nameof(flow));
            byte[] nativePath = ToNullTerminatedUtf8(path, nameof(path));
            NativeException.ThrowIfError(NativeMethods.VideoWriteOpticalFlow(nativePath, flow.NativeHandle, out int result));
            return result != 0;
        }

        /// <summary>
        /// Builds an optical-flow pyramid.
        /// 构建光流金字塔。
        /// </summary>
        public static OpticalFlowPyramidResult BuildOpticalFlowPyramid(
            Mat image,
            Size winSize,
            int maxLevel,
            bool withDerivatives = true,
            int pyrBorder = 4,
            int derivBorder = 0,
            bool tryReuseInputImage = true)
        {
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.VideoBuildOpticalFlowPyramidCount(
                image.NativeHandle,
                winSize.Width,
                winSize.Height,
                maxLevel,
                withDerivatives ? 1 : 0,
                pyrBorder,
                derivBorder,
                tryReuseInputImage ? 1 : 0,
                out int levelCount,
                out int matCount));

            if (matCount <= 0)
            {
                return new OpticalFlowPyramidResult(levelCount, Array.Empty<Mat>());
            }

            var handles = new IntPtr[matCount];
            NativeException.ThrowIfError(NativeMethods.VideoBuildOpticalFlowPyramidFill(
                image.NativeHandle,
                winSize.Width,
                winSize.Height,
                maxLevel,
                withDerivatives ? 1 : 0,
                pyrBorder,
                derivBorder,
                tryReuseInputImage ? 1 : 0,
                handles,
                handles.Length,
                out levelCount,
                out matCount));

            int count = Math.Max(0, Math.Min(matCount, handles.Length));
            var mats = new Mat[count];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = new Mat(handles[i]);
            }

            return new OpticalFlowPyramidResult(levelCount, mats);
        }

        /// <summary>
        /// Runs mean-shift tracking and returns the updated window.
        /// 执行 mean-shift 跟踪并返回更新后的窗口。
        /// </summary>
        public static MeanShiftResult MeanShift(Mat probImage, Rect window, TermCriteria? criteria = null)
        {
            ValidateNotNull(probImage, nameof(probImage));
            TermCriteria resolved = criteria ?? DefaultShiftCriteria;
            NativeMethods.VideoRectNative nativeWindow = ToNative(window);
            NativeException.ThrowIfError(NativeMethods.VideoMeanShift(
                probImage.NativeHandle,
                ref nativeWindow,
                (int)resolved.Type,
                resolved.MaxCount,
                resolved.Epsilon,
                out int iterations));
            return new MeanShiftResult(iterations, FromNative(nativeWindow));
        }

        /// <summary>
        /// Runs CamShift tracking and returns the updated window and rotated box.
        /// 执行 CamShift 跟踪并返回更新后的窗口和旋转框。
        /// </summary>
        public static CamShiftResult CamShift(Mat probImage, Rect window, TermCriteria? criteria = null)
        {
            ValidateNotNull(probImage, nameof(probImage));
            TermCriteria resolved = criteria ?? DefaultShiftCriteria;
            NativeMethods.VideoRectNative nativeWindow = ToNative(window);
            NativeException.ThrowIfError(NativeMethods.VideoCamShift(
                probImage.NativeHandle,
                ref nativeWindow,
                (int)resolved.Type,
                resolved.MaxCount,
                resolved.Epsilon,
                out NativeMethods.VideoRotatedRectNative box));
            return new CamShiftResult(FromNative(nativeWindow), FromNative(box));
        }

        private static Point2f[] CalcOpticalFlowPyrLKCore(
            Mat prevImg,
            Mat nextImg,
            Point2f[] prevPts,
            Point2f[] initialNextPts,
            out byte[] status,
            out float[] err,
            Size? winSize,
            int maxLevel,
            TermCriteria? criteria,
            OpticalFlowFlags flags,
            double minEigThreshold)
        {
            ValidateNotNull(prevImg, nameof(prevImg));
            ValidateNotNull(nextImg, nameof(nextImg));
            ValidateOpticalFlowFlags(flags, nameof(flags));
            bool useInitial = (flags & OpticalFlowFlags.UseInitialFlow) == OpticalFlowFlags.UseInitialFlow;
            if (useInitial && initialNextPts.Length != prevPts.Length)
            {
                throw new ArgumentException("Initial point count must match prevPts length.", nameof(initialNextPts));
            }

            Size actualWinSize = winSize ?? new Size(21, 21);
            TermCriteria actualCriteria = criteria ?? DefaultLkCriteria;
            status = new byte[prevPts.Length];
            err = new float[prevPts.Length];
            var next = new Point2f[prevPts.Length];
            NativeMethods.VideoPoint2fNative[] nativePrev = ToNative(prevPts);
            NativeMethods.VideoPoint2fNative[] nativeInitial = useInitial ? ToNative(initialNextPts) : Array.Empty<NativeMethods.VideoPoint2fNative>();
            NativeMethods.VideoPoint2fNative[] nativeNext = new NativeMethods.VideoPoint2fNative[prevPts.Length];

            fixed (NativeMethods.VideoPoint2fNative* prevPtr = nativePrev)
            fixed (NativeMethods.VideoPoint2fNative* initialPtr = nativeInitial)
            fixed (NativeMethods.VideoPoint2fNative* nextPtr = nativeNext)
            fixed (byte* statusPtr = status)
            fixed (float* errPtr = err)
            {
                NativeException.ThrowIfError(NativeMethods.VideoCalcOpticalFlowPyrLK(
                    prevImg.NativeHandle,
                    nextImg.NativeHandle,
                    prevPtr,
                    nativePrev.Length,
                    useInitial ? initialPtr : null,
                    useInitial ? 1 : 0,
                    nextPtr,
                    statusPtr,
                    errPtr,
                    actualWinSize.Width,
                    actualWinSize.Height,
                    maxLevel,
                    (int)actualCriteria.Type,
                    actualCriteria.MaxCount,
                    actualCriteria.Epsilon,
                    (int)flags,
                    minEigThreshold));
            }

            for (int i = 0; i < next.Length; i++)
            {
                next[i] = FromNative(nativeNext[i]);
            }

            return next;
        }

        private static void ValidateOpticalFlowFlags(OpticalFlowFlags value, string parameterName)
        {
            const OpticalFlowFlags validMask =
                OpticalFlowFlags.UseInitialFlow |
                OpticalFlowFlags.LkGetMinEigenvals |
                OpticalFlowFlags.FarnebackGaussian;
            if ((value & ~validMask) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unknown optical-flow flag bits are not supported.");
            }
        }

        private static NativeMethods.VideoPoint2fNative[] ToNative(Point2f[] points)
        {
            var result = new NativeMethods.VideoPoint2fNative[points.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new NativeMethods.VideoPoint2fNative { X = points[i].X, Y = points[i].Y };
            }

            return result;
        }

        private static Point2f FromNative(NativeMethods.VideoPoint2fNative point)
        {
            return new Point2f(point.X, point.Y);
        }

        private static NativeMethods.VideoRectNative ToNative(Rect rect)
        {
            return new NativeMethods.VideoRectNative { X = rect.X, Y = rect.Y, Width = rect.Width, Height = rect.Height };
        }

        private static Rect FromNative(NativeMethods.VideoRectNative rect)
        {
            return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }

        private static RotatedRect FromNative(NativeMethods.VideoRotatedRectNative rect)
        {
            return new RotatedRect(new Point2f(rect.CenterX, rect.CenterY), new Size2f(rect.Width, rect.Height), rect.Angle);
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static byte[] ToNullTerminatedUtf8(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            int byteCount = Encoding.UTF8.GetByteCount(value);
            var buffer = new byte[byteCount + 1];
            Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);
            return buffer;
        }
    }
}
