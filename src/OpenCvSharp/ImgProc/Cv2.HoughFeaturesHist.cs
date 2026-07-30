using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

#if NETCOREAPP3_1_OR_GREATER
using System.Buffers;
#endif

namespace OpenCvSharp.ImgProc
{
    public static partial class Cv2
    {
        /// <summary>
        /// Creates a CLAHE object.
        /// 创建 CLAHE 对象。
        /// </summary>
        /// <param name="clipLimit">The contrast limiting threshold. 对比度限制阈值。</param>
        /// <param name="tileGridSize">The tile grid size. 分块网格尺寸。</param>
        /// <returns>The created CLAHE object. 创建的 CLAHE 对象。</returns>
        public static CLAHE CreateCLAHE(double clipLimit = 40.0, Size? tileGridSize = null)
        {
            Size actualTileGridSize = tileGridSize ?? new Size(8, 8);
            ValidatePositive(actualTileGridSize.Width, nameof(tileGridSize));
            ValidatePositive(actualTileGridSize.Height, nameof(tileGridSize));

            NativeException.ThrowIfError(NativeMethods.ImgProcClaheCreate(
                clipLimit,
                actualTileGridSize.Width,
                actualTileGridSize.Height,
                out IntPtr nativeHandle));
            return new CLAHE(nativeHandle);
        }

        /// <summary>
        /// Creates a line segment detector.
        /// 创建线段检测器。
        /// </summary>
        /// <param name="refine">The refinement mode. 细化模式。</param>
        /// <param name="scale">The image scale used for detection. 检测使用的图像缩放比例。</param>
        /// <param name="sigmaScale">The Gaussian sigma scale. 高斯 sigma 缩放比例。</param>
        /// <param name="quant">The gradient norm quantization bound. 梯度范数量化误差边界。</param>
        /// <param name="angTh">The gradient angle tolerance in degrees. 梯度角度容差，单位为度。</param>
        /// <param name="logEps">The detection threshold. 检测阈值。</param>
        /// <param name="densityTh">The minimal aligned point density. 最小对齐点密度。</param>
        /// <param name="nBins">The number of bins for gradient ordering. 梯度排序 bin 数。</param>
        /// <returns>The created line segment detector. 创建的线段检测器。</returns>
        public static LineSegmentDetector CreateLineSegmentDetector(
            LineSegmentDetectorModes refine = LineSegmentDetectorModes.Standard,
            double scale = 0.8,
            double sigmaScale = 0.6,
            double quant = 2.0,
            double angTh = 22.5,
            double logEps = 0.0,
            double densityTh = 0.7,
            int nBins = 1024)
        {
            ValidateLineSegmentDetectorMode(refine, nameof(refine));
            NativeException.ThrowIfError(NativeMethods.ImgProcLineSegmentDetectorCreate(
                (int)refine,
                scale,
                sigmaScale,
                quant,
                angTh,
                logEps,
                densityTh,
                nBins,
                out IntPtr nativeHandle));
            return new LineSegmentDetector(nativeHandle);
        }

        /// <summary>Creates a translation-only generalized Hough detector. 创建仅检测平移的广义霍夫检测器。</summary>
        public static GeneralizedHoughBallard CreateGeneralizedHoughBallard()
        {
            NativeException.ThrowIfError(NativeMethods.ImgProcGeneralizedHoughBallardCreate(out IntPtr nativeHandle));
            return new GeneralizedHoughBallard(nativeHandle);
        }

        /// <summary>Creates a position, scale, and rotation generalized Hough detector. 创建检测位置、缩放和旋转的广义霍夫检测器。</summary>
        public static GeneralizedHoughGuil CreateGeneralizedHoughGuil()
        {
            NativeException.ThrowIfError(NativeMethods.ImgProcGeneralizedHoughGuilCreate(out IntPtr nativeHandle));
            return new GeneralizedHoughGuil(nativeHandle);
        }

        /// <summary>
        /// Refines corner locations to sub-pixel accuracy.
        /// 将角点位置细化到亚像素精度。
        /// </summary>
        /// <param name="image">The source image. 源图像。</param>
        /// <param name="corners">The input and output corner coordinates. 输入和输出角点坐标。</param>
        /// <param name="winSize">The half window size. 半窗口尺寸。</param>
        /// <param name="zeroZone">The dead-region half size, usually (-1,-1). 死区半尺寸，通常为 (-1,-1)。</param>
        /// <param name="criteria">The termination criteria. 终止条件。</param>
        public static void CornerSubPix(Mat image, Point2f[] corners, Size winSize, Size zeroZone, TermCriteria criteria)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNonEmpty(corners, nameof(corners));
            float[] cornersXy = ToInterleavedPoint2fAll(corners, nameof(corners));

            NativeException.ThrowIfError(NativeMethods.ImgProcCornerSubPix(
                image.NativeHandle,
                cornersXy,
                corners.Length,
                winSize.Width,
                winSize.Height,
                zeroZone.Width,
                zeroZone.Height,
                (int)criteria.Type,
                criteria.MaxCount,
                criteria.Epsilon));

            CopyInterleavedPoint2f(cornersXy, corners, corners.Length);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Refines corner locations to sub-pixel accuracy using a span-backed modern path.
        /// 使用基于 Span 的现代路径将角点位置细化到亚像素精度。
        /// </summary>
        /// <param name="image">The source image. 源图像。</param>
        /// <param name="corners">The input and output corner coordinates. 输入和输出角点坐标。</param>
        /// <param name="winSize">The half window size. 半窗口尺寸。</param>
        /// <param name="zeroZone">The dead-region half size, usually (-1,-1). 死区半尺寸，通常为 (-1,-1)。</param>
        /// <param name="criteria">The termination criteria. 终止条件。</param>
        public static unsafe void CornerSubPix(Mat image, Span<Point2f> corners, Size winSize, Size zeroZone, TermCriteria criteria)
        {
            ValidateNotNull(image, nameof(image));
            if (corners.IsEmpty)
            {
                throw new ArgumentException("Span cannot be empty.", nameof(corners));
            }

            Span<float> cornersXy = PointSetMarshaller.AsInterleaved(corners);
            fixed (float* cornersPtr = cornersXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcCornerSubPixPtr(
                    image.NativeHandle,
                    cornersPtr,
                    corners.Length,
                    winSize.Width,
                    winSize.Height,
                    zeroZone.Width,
                    zeroZone.Height,
                    (int)criteria.Type,
                    criteria.MaxCount,
                    criteria.Epsilon));
            }
        }
#endif

        /// <summary>
        /// Finds strong corners using OpenCV good-features-to-track.
        /// 使用 OpenCV good-features-to-track 查找强角点。
        /// </summary>
        /// <param name="image">The source image. 源图像。</param>
        /// <param name="maxCorners">The maximum number of corners. 最大角点数量。</param>
        /// <param name="qualityLevel">The quality threshold multiplier. 质量阈值乘数。</param>
        /// <param name="minDistance">The minimum distance between returned corners. 返回角点之间的最小距离。</param>
        /// <param name="mask">The optional mask. 可选掩码。</param>
        /// <param name="blockSize">The neighborhood block size. 邻域块尺寸。</param>
        /// <param name="gradientSize">The gradient aperture size. 梯度孔径尺寸。</param>
        /// <param name="useHarrisDetector">Whether to use the Harris response. 是否使用 Harris 响应。</param>
        /// <param name="k">The Harris detector free parameter. Harris 检测器自由参数。</param>
        /// <returns>The detected corners. 检测到的角点。</returns>
        public static Point2f[] GoodFeaturesToTrack(
            Mat image,
            int maxCorners,
            double qualityLevel,
            double minDistance,
            Mat? mask = null,
            int blockSize = 3,
            int gradientSize = 3,
            bool useHarrisDetector = false,
            double k = 0.04)
        {
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.ImgProcGoodFeaturesToTrackCount(
                image.NativeHandle,
                mask == null ? IntPtr.Zero : mask.NativeHandle,
                maxCorners,
                qualityLevel,
                minDistance,
                blockSize,
                gradientSize,
                useHarrisDetector ? 1 : 0,
                k,
                out int cornerCount));

            if (cornerCount <= 0)
            {
                return Array.Empty<Point2f>();
            }

            var cornersXy = new float[cornerCount * 2];
            NativeException.ThrowIfError(NativeMethods.ImgProcGoodFeaturesToTrackFill(
                image.NativeHandle,
                mask == null ? IntPtr.Zero : mask.NativeHandle,
                maxCorners,
                qualityLevel,
                minDistance,
                blockSize,
                gradientSize,
                useHarrisDetector ? 1 : 0,
                k,
                cornersXy,
                cornerCount,
                out int writtenCount));
            return FromInterleavedPoint2f(cornersXy, writtenCount);
        }

        /// <summary>
        /// Finds lines in a binary image using the standard Hough transform.
        /// 使用标准霍夫变换在二值图像中查找直线。
        /// </summary>
        public static HoughLine[] HoughLines(
            Mat image,
            double rho,
            double theta,
            int threshold,
            double srn = 0.0,
            double stn = 0.0,
            double minTheta = 0.0,
            double maxTheta = Math.PI,
            bool useEdgeval = false)
        {
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.ImgProcHoughLinesCount(
                image.NativeHandle,
                rho,
                theta,
                threshold,
                srn,
                stn,
                minTheta,
                maxTheta,
                useEdgeval ? 1 : 0,
                out int lineCount));

            if (lineCount <= 0)
            {
                return Array.Empty<HoughLine>();
            }

            var values = new float[lineCount * 2];
            NativeException.ThrowIfError(NativeMethods.ImgProcHoughLinesFill(
                image.NativeHandle,
                rho,
                theta,
                threshold,
                srn,
                stn,
                minTheta,
                maxTheta,
                useEdgeval ? 1 : 0,
                values,
                lineCount,
                out int writtenCount));
            return FromInterleavedHoughLines(values, writtenCount);
        }

        /// <summary>
        /// Finds line segments in a binary image using the probabilistic Hough transform.
        /// 使用概率霍夫变换在二值图像中查找线段。
        /// </summary>
        public static Vec4i[] HoughLinesP(
            Mat image,
            double rho,
            double theta,
            int threshold,
            double minLineLength = 0.0,
            double maxLineGap = 0.0)
        {
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.ImgProcHoughLinesPCount(
                image.NativeHandle,
                rho,
                theta,
                threshold,
                minLineLength,
                maxLineGap,
                out int lineCount));

            if (lineCount <= 0)
            {
                return Array.Empty<Vec4i>();
            }

            var values = new int[lineCount * 4];
            NativeException.ThrowIfError(NativeMethods.ImgProcHoughLinesPFill(
                image.NativeHandle,
                rho,
                theta,
                threshold,
                minLineLength,
                maxLineGap,
                values,
                lineCount,
                out int writtenCount));
            return FromInterleavedVec4iLocal(values, writtenCount);
        }

        /// <summary>
        /// Finds lines from a set of points using the Hough transform.
        /// 使用霍夫变换从点集中查找直线。
        /// </summary>
        public static HoughLinePointSet[] HoughLinesPointSet(
            Point[] points,
            int linesMax,
            int threshold,
            double minRho,
            double maxRho,
            double rhoStep,
            double minTheta,
            double maxTheta,
            double thetaStep)
        {
            int[] pointsXy = ToInterleavedPoints(points, nameof(points));
            return HoughLinesPointSetCore(pointsXy, points.Length, linesMax, threshold, minRho, maxRho, rhoStep, minTheta, maxTheta, thetaStep);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Finds lines from a span-backed point set using the Hough transform.
        /// 使用基于 Span 的点集通过霍夫变换查找直线。
        /// </summary>
        public static unsafe HoughLinePointSet[] HoughLinesPointSet(
            ReadOnlySpan<Point> points,
            int linesMax,
            int threshold,
            double minRho,
            double maxRho,
            double rhoStep,
            double minTheta,
            double maxTheta,
            double thetaStep)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));
            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);
            fixed (int* pointsPtr = pointsXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcHoughLinesPointSetCountPtr(
                    pointsPtr,
                    points.Length,
                    linesMax,
                    threshold,
                    minRho,
                    maxRho,
                    rhoStep,
                    minTheta,
                    maxTheta,
                    thetaStep,
                    out int lineCount));

                if (lineCount <= 0)
                {
                    return Array.Empty<HoughLinePointSet>();
                }

                var values = new double[lineCount * 3];
                NativeException.ThrowIfError(NativeMethods.ImgProcHoughLinesPointSetFillPtr(
                    pointsPtr,
                    points.Length,
                    linesMax,
                    threshold,
                    minRho,
                    maxRho,
                    rhoStep,
                    minTheta,
                    maxTheta,
                    thetaStep,
                    values,
                    lineCount,
                    out int writtenCount));
                return FromInterleavedHoughLinePointSet(values, writtenCount);
            }
        }
#endif

        /// <summary>
        /// Finds circles in a grayscale image using the Hough transform.
        /// 使用霍夫变换在灰度图像中查找圆。
        /// </summary>
        public static HoughCircle[] HoughCircles(
            Mat image,
            HoughModes method,
            double dp,
            double minDist,
            double param1 = 100.0,
            double param2 = 100.0,
            int minRadius = 0,
            int maxRadius = 0)
        {
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.ImgProcHoughCirclesCount(
                image.NativeHandle,
                (int)method,
                dp,
                minDist,
                param1,
                param2,
                minRadius,
                maxRadius,
                out int circleCount));

            if (circleCount <= 0)
            {
                return Array.Empty<HoughCircle>();
            }

            var values = new float[circleCount * 3];
            NativeException.ThrowIfError(NativeMethods.ImgProcHoughCirclesFill(
                image.NativeHandle,
                (int)method,
                dp,
                minDist,
                param1,
                param2,
                minRadius,
                maxRadius,
                values,
                circleCount,
                out int writtenCount));
            return FromInterleavedHoughCircles(values, writtenCount);
        }

        /// <summary>
        /// Calculates a uniform dense histogram for a single image.
        /// 计算单幅图像的均匀密集直方图。
        /// </summary>
        public static void CalcHist(Mat image, int[] channels, Mat? mask, Mat hist, int[] histSize, float[] ranges, bool accumulate = false)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(hist, nameof(hist));
            ValidateNonEmpty(channels, nameof(channels));
            ValidateNonEmpty(histSize, nameof(histSize));
            ValidateNonEmpty(ranges, nameof(ranges));

            NativeException.ThrowIfError(NativeMethods.ImgProcCalcHistUniform(
                image.NativeHandle,
                mask == null ? IntPtr.Zero : mask.NativeHandle,
                channels,
                channels.Length,
                hist.NativeHandle,
                histSize,
                histSize.Length,
                ranges,
                ranges.Length,
                accumulate ? 1 : 0));
        }

        /// <summary>
        /// Calculates a uniform dense histogram for one channel.
        /// 计算单通道均匀密集直方图。
        /// </summary>
        public static void CalcHist(Mat image, int channel, Mat? mask, Mat hist, int histSize, float rangeMin, float rangeMax, bool accumulate = false)
        {
            CalcHist(image, new[] { channel }, mask, hist, new[] { histSize }, new[] { rangeMin, rangeMax }, accumulate);
        }

        /// <summary>
        /// Calculates a histogram back projection.
        /// 计算直方图反向投影。
        /// </summary>
        public static void CalcBackProject(Mat image, int[] channels, Mat hist, Mat backProject, float[] ranges, double scale = 1.0)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(hist, nameof(hist));
            ValidateNotNull(backProject, nameof(backProject));
            ValidateNonEmpty(channels, nameof(channels));
            ValidateNonEmpty(ranges, nameof(ranges));

            NativeException.ThrowIfError(NativeMethods.ImgProcCalcBackProjectUniform(
                image.NativeHandle,
                channels,
                channels.Length,
                hist.NativeHandle,
                backProject.NativeHandle,
                ranges,
                ranges.Length,
                scale));
        }

        /// <summary>
        /// Calculates a one-channel histogram back projection.
        /// 计算单通道直方图反向投影。
        /// </summary>
        public static void CalcBackProject(Mat image, int channel, Mat hist, Mat backProject, float rangeMin, float rangeMax, double scale = 1.0)
        {
            CalcBackProject(image, new[] { channel }, hist, backProject, new[] { rangeMin, rangeMax }, scale);
        }

        /// <summary>
        /// Compares two dense histograms.
        /// 比较两个密集直方图。
        /// </summary>
        public static double CompareHist(Mat h1, Mat h2, HistogramComparisonTypes method)
        {
            ValidateNotNull(h1, nameof(h1));
            ValidateNotNull(h2, nameof(h2));
            ValidateHistogramComparisonType(method, nameof(method));
            NativeException.ThrowIfError(NativeMethods.ImgProcCompareHist(h1.NativeHandle, h2.NativeHandle, (int)method, out double result));
            return result;
        }

        private static HoughLinePointSet[] HoughLinesPointSetCore(
            int[] pointsXy,
            int pointCount,
            int linesMax,
            int threshold,
            double minRho,
            double maxRho,
            double rhoStep,
            double minTheta,
            double maxTheta,
            double thetaStep)
        {
            NativeException.ThrowIfError(NativeMethods.ImgProcHoughLinesPointSetCount(
                pointsXy,
                pointCount,
                linesMax,
                threshold,
                minRho,
                maxRho,
                rhoStep,
                minTheta,
                maxTheta,
                thetaStep,
                out int lineCount));

            if (lineCount <= 0)
            {
                return Array.Empty<HoughLinePointSet>();
            }

            var values = new double[lineCount * 3];
            NativeException.ThrowIfError(NativeMethods.ImgProcHoughLinesPointSetFill(
                pointsXy,
                pointCount,
                linesMax,
                threshold,
                minRho,
                maxRho,
                rhoStep,
                minTheta,
                maxTheta,
                thetaStep,
                values,
                lineCount,
                out int writtenCount));
            return FromInterleavedHoughLinePointSet(values, writtenCount);
        }

        private static HoughLine[] FromInterleavedHoughLines(float[] values, int lineCount)
        {
            var result = new HoughLine[lineCount];
            for (int i = 0; i < lineCount; i++)
            {
                int offset = i * 2;
                result[i] = new HoughLine(values[offset], values[offset + 1]);
            }

            return result;
        }

        private static HoughLinePointSet[] FromInterleavedHoughLinePointSet(double[] values, int lineCount)
        {
            var result = new HoughLinePointSet[lineCount];
            for (int i = 0; i < lineCount; i++)
            {
                int offset = i * 3;
                result[i] = new HoughLinePointSet(values[offset], values[offset + 1], values[offset + 2]);
            }

            return result;
        }

        private static HoughCircle[] FromInterleavedHoughCircles(float[] values, int circleCount)
        {
            var result = new HoughCircle[circleCount];
            for (int i = 0; i < circleCount; i++)
            {
                int offset = i * 3;
                result[i] = new HoughCircle(values[offset], values[offset + 1], values[offset + 2]);
            }

            return result;
        }

        private static float[] ToInterleavedPoint2fAll(Point2f[] points, string parameterName)
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

        private static void CopyInterleavedPoint2f(float[] pointsXy, Point2f[] destination, int pointCount)
        {
            for (int i = 0; i < pointCount; i++)
            {
                int offset = i * 2;
                destination[i] = new Point2f(pointsXy[offset], pointsXy[offset + 1]);
            }
        }
    }
}
