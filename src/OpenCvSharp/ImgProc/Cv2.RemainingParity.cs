using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Geometry;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgProc
{
    public static partial class Cv2
    {
        /// <summary>Extracts a sub-pixel rectangular patch. 提取亚像素矩形图像块。</summary>
        public static void GetRectSubPix(Mat image, Size patchSize, Point2f center, Mat patch, int patchType = -1)
        {
            ValidateRemainingMat(image, nameof(image));
            ValidateRemainingMat(patch, nameof(patch));
            ValidateRemainingPositiveSize(patchSize, nameof(patchSize));
            NativeException.ThrowIfError(NativeMethods.ImgProcGetRectSubPix(
                image.NativeHandle,
                patchSize.Width,
                patchSize.Height,
                center.X,
                center.Y,
                patch.NativeHandle,
                patchType));
        }

        /// <summary>Extracts and returns an owned sub-pixel patch. 提取并返回拥有所有权的亚像素图像块。</summary>
        public static Mat GetRectSubPix(Mat image, Size patchSize, Point2f center, int patchType = -1)
        {
            var patch = new Mat();
            try
            {
                GetRectSubPix(image, patchSize, center, patch, patchType);
                return patch;
            }
            catch
            {
                patch.Dispose();
                throw;
            }
        }

        /// <summary>Remaps an image to linear or logarithmic polar coordinates. 将图像重映射到线性或对数极坐标。</summary>
        public static void WarpPolar(
            Mat src,
            Mat dst,
            Size dsize,
            Point2f center,
            double maxRadius,
            InterpolationFlags flags,
            WarpPolarMode mode = WarpPolarMode.Linear)
        {
            ValidateRemainingMat(src, nameof(src));
            ValidateRemainingMat(dst, nameof(dst));
            if (dsize.Width < 0 || dsize.Height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dsize), "Destination dimensions cannot be negative.");
            }
            ValidateRemainingFinitePositive(maxRadius, nameof(maxRadius));
            ValidateRemainingInterpolation(flags, nameof(flags));
            if (mode != WarpPolarMode.Linear && mode != WarpPolarMode.Log)
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }

            NativeException.ThrowIfError(NativeMethods.ImgProcWarpPolar(
                src.NativeHandle,
                dst.NativeHandle,
                dsize.Width,
                dsize.Height,
                center.X,
                center.Y,
                maxRadius,
                (int)flags | (int)mode));
        }

        /// <summary>Remaps and returns an owned polar image. 重映射并返回拥有所有权的极坐标图像。</summary>
        public static Mat WarpPolar(
            Mat src,
            Size dsize,
            Point2f center,
            double maxRadius,
            InterpolationFlags flags,
            WarpPolarMode mode = WarpPolarMode.Linear)
        {
            var dst = new Mat();
            try
            {
                WarpPolar(src, dst, dsize, center, maxRadius, flags, mode);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Adds source values to an accumulator. 将源值累加到累加器。</summary>
        public static void Accumulate(Mat src, Mat dst, Mat? mask = null)
        {
            ValidateRemainingMat(src, nameof(src));
            ValidateRemainingMat(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.ImgProcAccumulate(src.NativeHandle, dst.NativeHandle, RemainingHandleOrZero(mask)));
        }

        /// <summary>Adds squared source values to an accumulator. 将源值平方后累加到累加器。</summary>
        public static void AccumulateSquare(Mat src, Mat dst, Mat? mask = null)
        {
            ValidateRemainingMat(src, nameof(src));
            ValidateRemainingMat(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.ImgProcAccumulateSquare(src.NativeHandle, dst.NativeHandle, RemainingHandleOrZero(mask)));
        }

        /// <summary>Adds the product of two sources to an accumulator. 将两个源的乘积累加到累加器。</summary>
        public static void AccumulateProduct(Mat src1, Mat src2, Mat dst, Mat? mask = null)
        {
            ValidateRemainingMat(src1, nameof(src1));
            ValidateRemainingMat(src2, nameof(src2));
            ValidateRemainingMat(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.ImgProcAccumulateProduct(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle, RemainingHandleOrZero(mask)));
        }

        /// <summary>Updates a running weighted average. 更新运行中的加权平均值。</summary>
        public static void AccumulateWeighted(Mat src, Mat dst, double alpha, Mat? mask = null)
        {
            ValidateRemainingMat(src, nameof(src));
            ValidateRemainingMat(dst, nameof(dst));
            if (double.IsNaN(alpha) || double.IsInfinity(alpha) || alpha < 0.0 || alpha > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(alpha), "Alpha must be finite and between zero and one.");
            }
            NativeException.ThrowIfError(NativeMethods.ImgProcAccumulateWeighted(src.NativeHandle, dst.NativeHandle, alpha, RemainingHandleOrZero(mask)));
        }

        /// <summary>Detects translation with phase correlation. 使用相位相关检测平移。</summary>
        public static Point2d PhaseCorrelate(Mat src1, Mat src2, Mat? window, out double response)
        {
            ValidateRemainingMat(src1, nameof(src1));
            ValidateRemainingMat(src2, nameof(src2));
            NativeException.ThrowIfError(NativeMethods.ImgProcPhaseCorrelate(
                src1.NativeHandle,
                src2.NativeHandle,
                RemainingHandleOrZero(window),
                out double shiftX,
                out double shiftY,
                out response));
            return new Point2d(shiftX, shiftY);
        }

        /// <summary>Detects translation with phase correlation. 使用相位相关检测平移。</summary>
        public static Point2d PhaseCorrelate(Mat src1, Mat src2, Mat? window = null)
        {
            return PhaseCorrelate(src1, src2, window, out _);
        }

        /// <summary>Detects translation with iterative phase correlation. 使用迭代相位相关检测平移。</summary>
        public static Point2d PhaseCorrelateIterative(Mat src1, Mat src2, int l2Size = 7, int maxIters = 10)
        {
            ValidateRemainingMat(src1, nameof(src1));
            ValidateRemainingMat(src2, nameof(src2));
            if (l2Size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(l2Size), "Neighborhood size must be positive.");
            }
            if (maxIters <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxIters), "Iteration count must be positive.");
            }
            NativeException.ThrowIfError(NativeMethods.ImgProcPhaseCorrelateIterative(
                src1.NativeHandle,
                src2.NativeHandle,
                l2Size,
                maxIters,
                out double shiftX,
                out double shiftY));
            return new Point2d(shiftX, shiftY);
        }

        /// <summary>Creates a two-dimensional Hanning window. 创建二维 Hanning 窗。</summary>
        public static void CreateHanningWindow(Mat dst, Size winSize, int type)
        {
            ValidateRemainingMat(dst, nameof(dst));
            if (winSize.Width <= 1 || winSize.Height <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(winSize), "Both window dimensions must be greater than one.");
            }
            NativeException.ThrowIfError(NativeMethods.ImgProcCreateHanningWindow(dst.NativeHandle, winSize.Width, winSize.Height, type));
        }

        /// <summary>Creates and returns an owned two-dimensional Hanning window. 创建并返回拥有所有权的二维 Hanning 窗。</summary>
        public static Mat CreateHanningWindow(Size winSize, int type)
        {
            var dst = new Mat();
            try
            {
                CreateHanningWindow(dst, winSize, type);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Computes the earth mover distance. 计算推土机距离。</summary>
        public static float EMD(Mat signature1, Mat signature2, DistanceTypes distanceType, Mat? cost = null, Mat? flow = null)
        {
            float lowerBound = 0.0F;
            return RunEmd(signature1, signature2, distanceType, cost, false, ref lowerBound, flow);
        }

        /// <summary>Computes the earth mover distance and updates a lower bound. 计算推土机距离并更新下界。</summary>
        public static float EMD(Mat signature1, Mat signature2, DistanceTypes distanceType, ref float lowerBound, Mat? cost = null, Mat? flow = null)
        {
            if (float.IsNaN(lowerBound) || float.IsInfinity(lowerBound))
            {
                throw new ArgumentOutOfRangeException(nameof(lowerBound), "Lower bound must be finite.");
            }
            return RunEmd(signature1, signature2, distanceType, cost, true, ref lowerBound, flow);
        }

        /// <summary>Runs marker-based watershed segmentation in place. 原位运行基于标记的分水岭分割。</summary>
        public static void Watershed(Mat image, Mat markers)
        {
            ValidateRemainingMat(image, nameof(image));
            ValidateRemainingMat(markers, nameof(markers));
            NativeException.ThrowIfError(NativeMethods.ImgProcWatershed(image.NativeHandle, markers.NativeHandle));
        }

        /// <summary>Applies pyramid mean-shift filtering. 应用金字塔均值漂移滤波。</summary>
        public static void PyrMeanShiftFiltering(
            Mat src,
            Mat dst,
            double spatialRadius,
            double colorRadius,
            int maxLevel = 1,
            TermCriteria? criteria = null)
        {
            ValidateRemainingMat(src, nameof(src));
            ValidateRemainingMat(dst, nameof(dst));
            ValidateRemainingFinitePositive(spatialRadius, nameof(spatialRadius));
            ValidateRemainingFinitePositive(colorRadius, nameof(colorRadius));
            if (maxLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLevel), "Maximum pyramid level cannot be negative.");
            }
            TermCriteria actualCriteria = criteria ?? TermCriteria.ByCountAndEpsilon(5, 1.0);
            NativeException.ThrowIfError(NativeMethods.ImgProcPyrMeanShiftFiltering(
                src.NativeHandle,
                dst.NativeHandle,
                spatialRadius,
                colorRadius,
                maxLevel,
                (int)actualCriteria.Type,
                actualCriteria.MaxCount,
                actualCriteria.Epsilon));
        }

        /// <summary>Applies mean-shift filtering and returns an owned result. 应用均值漂移滤波并返回拥有所有权的结果。</summary>
        public static Mat PyrMeanShiftFiltering(
            Mat src,
            double spatialRadius,
            double colorRadius,
            int maxLevel = 1,
            TermCriteria? criteria = null)
        {
            var dst = new Mat();
            try
            {
                PyrMeanShiftFiltering(src, dst, spatialRadius, colorRadius, maxLevel, criteria);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        /// <summary>Runs GrabCut segmentation. 运行 GrabCut 分割。</summary>
        public static void GrabCut(
            Mat image,
            Mat mask,
            Rect rect,
            Mat backgroundModel,
            Mat foregroundModel,
            int iterationCount,
            GrabCutModes mode = GrabCutModes.Eval)
        {
            ValidateRemainingMat(image, nameof(image));
            ValidateRemainingMat(mask, nameof(mask));
            ValidateRemainingMat(backgroundModel, nameof(backgroundModel));
            ValidateRemainingMat(foregroundModel, nameof(foregroundModel));
            if (iterationCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterationCount), "Iteration count cannot be negative.");
            }
            if (rect.Width < 0 || rect.Height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rect), "Rectangle dimensions cannot be negative.");
            }
            if ((int)mode < (int)GrabCutModes.InitWithRect || mode > GrabCutModes.EvalFreezeModel)
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }
            NativeException.ThrowIfError(NativeMethods.ImgProcGrabCut(
                image.NativeHandle,
                mask.NativeHandle,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height,
                backgroundModel.NativeHandle,
                foregroundModel.NativeHandle,
                iterationCount,
                (int)mode));
        }

        /// <summary>Compares an image with a template. 将图像与模板进行比较。</summary>
        public static void MatchTemplate(Mat image, Mat templ, Mat result, TemplateMatchModes method, Mat? mask = null)
        {
            ValidateRemainingMat(image, nameof(image));
            ValidateRemainingMat(templ, nameof(templ));
            ValidateRemainingMat(result, nameof(result));
            if (method < TemplateMatchModes.SqDiff || method > TemplateMatchModes.CCoeffNormed)
            {
                throw new ArgumentOutOfRangeException(nameof(method));
            }
            NativeException.ThrowIfError(NativeMethods.ImgProcMatchTemplate(
                image.NativeHandle,
                templ.NativeHandle,
                result.NativeHandle,
                (int)method,
                RemainingHandleOrZero(mask)));
        }

        /// <summary>Compares an image with a template and returns an owned response map. 比较图像与模板并返回拥有所有权的响应图。</summary>
        public static Mat MatchTemplate(Mat image, Mat templ, TemplateMatchModes method, Mat? mask = null)
        {
            var result = new Mat();
            try
            {
                MatchTemplate(image, templ, result, method, mask);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>Finds contours with the link-runs algorithm and returns hierarchy. 使用 link-runs 算法查找轮廓并返回层次结构。</summary>
        public static void FindContoursLinkRuns(Mat image, out Point[][] contours, out Vec4i[] hierarchy)
        {
            FindContoursLinkRunsCore(image, true, out contours, out hierarchy);
        }

        /// <summary>Finds contours with the link-runs algorithm. 使用 link-runs 算法查找轮廓。</summary>
        public static void FindContoursLinkRuns(Mat image, out Point[][] contours)
        {
            FindContoursLinkRunsCore(image, false, out contours, out _);
        }

        /// <summary>Finds and returns contours with the link-runs algorithm. 使用 link-runs 算法查找并返回轮廓。</summary>
        public static Point[][] FindContoursLinkRuns(Mat image)
        {
            FindContoursLinkRuns(image, out Point[][] contours);
            return contours;
        }

        /// <summary>Renders UTF-8 Unicode text with OpenCV <c>putText</c> and a TrueType/OpenType <see cref="FontFace"/>, then returns the continuation point. A font containing the requested glyphs enables Chinese rendering. 使用 OpenCV <c>putText</c> 和 TrueType/OpenType <see cref="FontFace"/> 渲染 UTF-8 Unicode 文本并返回续写位置；指定包含相应字形的字体即可绘制中文。</summary>
        public static Point PutText(
            Mat image,
            string text,
            Point origin,
            Scalar color,
            FontFace fontFace,
            int size,
            int weight = 0,
            PutTextFlags flags = PutTextFlags.AlignLeft,
            TextWrapRange? wrap = null)
        {
            ValidateRemainingMat(image, nameof(image));
            if (fontFace == null)
            {
                throw new ArgumentNullException(nameof(fontFace));
            }
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Font size must be positive.");
            }
            if (weight < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weight), "Font weight cannot be negative.");
            }
            ValidatePutTextFlags(flags, nameof(flags));
            byte[] nativeText = ToNullTerminatedUtf8Text(text, nameof(text));
            TextWrapRange actualWrap = wrap ?? default;
            NativeException.ThrowIfError(NativeMethods.ImgProcPutTextFontFace(
                image.NativeHandle,
                nativeText,
                origin.X,
                origin.Y,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                fontFace.NativeHandle,
                size,
                weight,
                (int)flags,
                wrap.HasValue ? 1 : 0,
                actualWrap.Start,
                actualWrap.End,
                out int nextX,
                out int nextY));
            return new Point(nextX, nextY);
        }

        /// <summary>Calculates the custom-font text bounding rectangle. 计算自定义字体文本边界矩形。</summary>
        public static Rect GetTextSize(
            Size imageSize,
            string text,
            Point origin,
            FontFace fontFace,
            int size,
            int weight = 0,
            PutTextFlags flags = PutTextFlags.AlignLeft,
            TextWrapRange? wrap = null)
        {
            if (imageSize.Width < 0 || imageSize.Height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(imageSize), "Image dimensions cannot be negative.");
            }
            if (fontFace == null)
            {
                throw new ArgumentNullException(nameof(fontFace));
            }
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Font size must be positive.");
            }
            if (weight < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weight), "Font weight cannot be negative.");
            }
            ValidatePutTextFlags(flags, nameof(flags));
            byte[] nativeText = ToNullTerminatedUtf8Text(text, nameof(text));
            TextWrapRange actualWrap = wrap ?? default;
            NativeException.ThrowIfError(NativeMethods.ImgProcGetTextSizeFontFace(
                imageSize.Width,
                imageSize.Height,
                nativeText,
                origin.X,
                origin.Y,
                fontFace.NativeHandle,
                size,
                weight,
                (int)flags,
                wrap.HasValue ? 1 : 0,
                actualWrap.Start,
                actualWrap.End,
                out int x,
                out int y,
                out int width,
                out int height));
            return new Rect(x, y, width, height);
        }

        private static float RunEmd(
            Mat signature1,
            Mat signature2,
            DistanceTypes distanceType,
            Mat? cost,
            bool hasLowerBound,
            ref float lowerBound,
            Mat? flow)
        {
            ValidateRemainingMat(signature1, nameof(signature1));
            ValidateRemainingMat(signature2, nameof(signature2));
            if (distanceType != DistanceTypes.User && (distanceType < DistanceTypes.L1 || distanceType > DistanceTypes.Huber))
            {
                throw new ArgumentOutOfRangeException(nameof(distanceType));
            }
            NativeException.ThrowIfError(NativeMethods.ImgProcEmd(
                signature1.NativeHandle,
                signature2.NativeHandle,
                (int)distanceType,
                RemainingHandleOrZero(cost),
                hasLowerBound ? 1 : 0,
                ref lowerBound,
                RemainingHandleOrZero(flow),
                out float distance));
            return distance;
        }

        private static void FindContoursLinkRunsCore(Mat image, bool includeHierarchy, out Point[][] contours, out Vec4i[] hierarchy)
        {
            ValidateRemainingMat(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.ImgProcFindContoursLinkRunsCount(
                image.NativeHandle,
                includeHierarchy ? 1 : 0,
                out int contourCount,
                out int totalPointCount,
                out int hierarchyCount));
            if (contourCount < 0 || totalPointCount < 0 || hierarchyCount < 0)
            {
                throw new OpenCvException("Native link-runs contour counts are invalid.");
            }

            var contoursXy = new int[totalPointCount * 2];
            var contourLengths = new int[contourCount];
            var hierarchyValues = new int[hierarchyCount * 4];
            NativeException.ThrowIfError(NativeMethods.ImgProcFindContoursLinkRunsFill(
                image.NativeHandle,
                includeHierarchy ? 1 : 0,
                contoursXy,
                totalPointCount,
                contourLengths,
                contourCount,
                hierarchyValues,
                hierarchyCount,
                out int writtenContourCount,
                out int writtenPointCount,
                out int writtenHierarchyCount));
            if (writtenContourCount != contourCount || writtenPointCount != totalPointCount || writtenHierarchyCount != hierarchyCount)
            {
                throw new OpenCvException("Native link-runs contour output changed between count and fill.");
            }

            contours = FromFlatContours(contoursXy, contourLengths, contourCount, totalPointCount);
            hierarchy = FromInterleavedVec4iLocal(hierarchyValues, hierarchyCount);
        }

        private static void ValidateRemainingMat(Mat value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static IntPtr RemainingHandleOrZero(Mat? value)
        {
            return value == null ? IntPtr.Zero : value.NativeHandle;
        }

        private static void ValidateRemainingPositiveSize(Size value, string parameterName)
        {
            if (value.Width <= 0 || value.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Both dimensions must be positive.");
            }
        }

        private static void ValidateRemainingFinitePositive(double value, string parameterName)
        {
            if (!(value > 0.0) || double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and positive.");
            }
        }

        private static void ValidateRemainingInterpolation(InterpolationFlags value, string parameterName)
        {
            const int allowedBits = 0x3f;
            int raw = (int)value;
            if ((raw & ~allowedBits) != 0 || (raw & (int)InterpolationFlags.Max) == (int)InterpolationFlags.Max)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported interpolation flags.");
            }
        }

        private static void ValidatePutTextFlags(PutTextFlags value, string parameterName)
        {
            const int allowedBits = (int)PutTextFlags.AlignMask | (int)PutTextFlags.OriginBottomLeft | (int)PutTextFlags.Wrap;
            if (((int)value & ~allowedBits) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported text flags.");
            }
        }
    }
}
