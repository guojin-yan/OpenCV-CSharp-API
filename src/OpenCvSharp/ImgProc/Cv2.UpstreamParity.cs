using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ImgProc
{
    public static partial class Cv2
    {
        /// <summary>Converts separate Y and interleaved UV planes to a color image. 将独立 Y 平面和交错 UV 平面转换为彩色图像。</summary>
        public static void CvtColorTwoPlane(Mat src1, Mat src2, Mat dst, ColorConversionCodes code)
        {
            ValidateNotNull(src1, nameof(src1));
            ValidateNotNull(src2, nameof(src2));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.ImgProcCvtColorTwoPlane(src1.NativeHandle, src2.NativeHandle, dst.NativeHandle, (int)code));
        }

        /// <summary>Converts separate Y and UV planes and returns the color image. 转换独立 Y 和 UV 平面并返回彩色图像。</summary>
        public static Mat CvtColorTwoPlane(Mat src1, Mat src2, ColorConversionCodes code)
        {
            var dst = new Mat();
            CvtColorTwoPlane(src1, src2, dst, code);
            return dst;
        }

        /// <summary>Converts a Bayer image to gray or color. 将 Bayer 图像转换为灰度或彩色图像。</summary>
        public static void Demosaicing(Mat src, Mat dst, ColorConversionCodes code, int dstCn = 0)
        {
            ValidateMatPair(src, dst);
            if (dstCn < 0) throw new ArgumentOutOfRangeException(nameof(dstCn), "Destination channel count cannot be negative.");
            NativeException.ThrowIfError(NativeMethods.ImgProcDemosaicing(src.NativeHandle, dst.NativeHandle, (int)code, dstCn));
        }

        /// <summary>Converts a Bayer image and returns the result. 转换 Bayer 图像并返回结果。</summary>
        public static Mat Demosaicing(Mat src, ColorConversionCodes code, int dstCn = 0)
        {
            var dst = new Mat();
            Demosaicing(src, dst, code, dstCn);
            return dst;
        }

        /// <summary>Applies an OpenCV built-in color map. 应用 OpenCV 内置颜色映射。</summary>
        public static void ApplyColorMap(Mat src, Mat dst, ColormapTypes colormap)
        {
            ValidateMatPair(src, dst);
            ValidateColormap(colormap, nameof(colormap));
            NativeException.ThrowIfError(NativeMethods.ImgProcApplyColorMap(src.NativeHandle, dst.NativeHandle, (int)colormap));
        }

        /// <summary>Applies an OpenCV built-in color map and returns the result. 应用内置颜色映射并返回结果。</summary>
        public static Mat ApplyColorMap(Mat src, ColormapTypes colormap)
        {
            var dst = new Mat();
            ApplyColorMap(src, dst, colormap);
            return dst;
        }

        /// <summary>Applies a user-provided 256-entry color map. 应用用户提供的 256 项颜色映射。</summary>
        public static void ApplyColorMap(Mat src, Mat dst, Mat userColor)
        {
            ValidateMatPair(src, dst);
            ValidateNotNull(userColor, nameof(userColor));
            NativeException.ThrowIfError(NativeMethods.ImgProcApplyColorMapUser(src.NativeHandle, dst.NativeHandle, userColor.NativeHandle));
        }

        /// <summary>Applies a user color map and returns the result. 应用用户颜色映射并返回结果。</summary>
        public static Mat ApplyColorMap(Mat src, Mat userColor)
        {
            var dst = new Mat();
            ApplyColorMap(src, dst, userColor);
            return dst;
        }

        /// <summary>Blends two images with per-pixel floating-point weights. 使用逐像素浮点权重混合两幅图像。</summary>
        public static void BlendLinear(Mat src1, Mat src2, Mat weights1, Mat weights2, Mat dst)
        {
            ValidateNotNull(src1, nameof(src1));
            ValidateNotNull(src2, nameof(src2));
            ValidateNotNull(weights1, nameof(weights1));
            ValidateNotNull(weights2, nameof(weights2));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.ImgProcBlendLinear(
                src1.NativeHandle,
                src2.NativeHandle,
                weights1.NativeHandle,
                weights2.NativeHandle,
                dst.NativeHandle));
        }

        /// <summary>Blends two images and returns the result. 混合两幅图像并返回结果。</summary>
        public static Mat BlendLinear(Mat src1, Mat src2, Mat weights1, Mat weights2)
        {
            var dst = new Mat();
            BlendLinear(src1, src2, weights1, weights2, dst);
            return dst;
        }

        /// <summary>Applies OpenCV's stack blur filter. 应用 OpenCV stack blur 滤波器。</summary>
        public static void StackBlur(Mat src, Mat dst, Size ksize)
        {
            ValidateMatPair(src, dst);
            ValidateOddGreaterThanOne(ksize.Width, nameof(ksize));
            ValidateOddGreaterThanOne(ksize.Height, nameof(ksize));
            NativeException.ThrowIfError(NativeMethods.ImgProcStackBlur(src.NativeHandle, dst.NativeHandle, ksize.Width, ksize.Height));
        }

        /// <summary>Applies stack blur and returns the result. 应用 stack blur 并返回结果。</summary>
        public static Mat StackBlur(Mat src, Size ksize)
        {
            var dst = new Mat();
            StackBlur(src, dst, ksize);
            return dst;
        }

        /// <summary>Computes first-order image derivatives in both directions. 计算两个方向的一阶图像导数。</summary>
        public static void SpatialGradient(Mat src, Mat dx, Mat dy, int ksize = 3, BorderTypes borderType = BorderTypes.Default)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dx, nameof(dx));
            ValidateNotNull(dy, nameof(dy));
            if (ksize != 3) throw new ArgumentOutOfRangeException(nameof(ksize), "OpenCV spatialGradient requires ksize=3.");
            NativeException.ThrowIfError(NativeMethods.ImgProcSpatialGradient(src.NativeHandle, dx.NativeHandle, dy.NativeHandle, ksize, (int)borderType));
        }

        /// <summary>Applies a threshold only where the mask is nonzero. 仅在掩码非零位置应用阈值。</summary>
        public static double ThresholdWithMask(Mat src, Mat dst, Mat mask, double thresh, double maxval, ThresholdTypes type)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateNotNull(mask, nameof(mask));
            NativeException.ThrowIfError(NativeMethods.ImgProcThresholdWithMask(
                src.NativeHandle,
                dst.NativeHandle,
                mask.NativeHandle,
                thresh,
                maxval,
                (int)type,
                out double result));
            return result;
        }

        /// <summary>Draws a marker at the specified image position. 在指定图像位置绘制标记。</summary>
        public static void DrawMarker(
            Mat image,
            Point position,
            Scalar color,
            MarkerTypes markerType = MarkerTypes.Cross,
            int markerSize = 20,
            int thickness = 1,
            LineTypes lineType = LineTypes.Line8)
        {
            ValidateNotNull(image, nameof(image));
            ValidateMarkerType(markerType, nameof(markerType));
            if (markerSize <= 0) throw new ArgumentOutOfRangeException(nameof(markerSize), "Marker size must be positive.");
            if (thickness <= 0) throw new ArgumentOutOfRangeException(nameof(thickness), "Thickness must be positive.");
            NativeException.ThrowIfError(NativeMethods.ImgProcDrawMarker(
                image.NativeHandle,
                position.X,
                position.Y,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                (int)markerType,
                markerSize,
                thickness,
                (int)lineType));
        }

        /// <summary>Fills a convex polygon. 填充凸多边形。</summary>
        public static void FillConvexPoly(Mat image, Point[] points, Scalar color, LineTypes lineType = LineTypes.Line8, int shift = 0)
        {
            ValidateNotNull(image, nameof(image));
            int[] pointsXy = ToInterleavedPoints(points, nameof(points));
            if (points.Length < 3) throw new ArgumentException("At least three points are required.", nameof(points));
            NativeException.ThrowIfError(NativeMethods.ImgProcFillConvexPoly(
                image.NativeHandle,
                pointsXy,
                points.Length,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                (int)lineType,
                shift));
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Fills a convex polygon from a zero-allocation point span. 从零分配点 span 填充凸多边形。</summary>
        public static unsafe void FillConvexPoly(Mat image, ReadOnlySpan<Point> points, Scalar color, LineTypes lineType = LineTypes.Line8, int shift = 0)
        {
            ValidateNotNull(image, nameof(image));
            if (points.Length < 3) throw new ArgumentException("At least three points are required.", nameof(points));
            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);
            fixed (int* pointsPtr = pointsXy)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcFillConvexPolyPtr(
                    image.NativeHandle,
                    pointsPtr,
                    points.Length,
                    color.V0,
                    color.V1,
                    color.V2,
                    color.V3,
                    (int)lineType,
                    shift));
            }
        }
#endif

        /// <summary>Returns the Hershey font scale for a requested pixel height. 返回指定像素高度对应的 Hershey 字体缩放值。</summary>
        public static double GetFontScaleFromHeight(HersheyFonts fontFace, int pixelHeight, int thickness = 1)
        {
            ValidateHersheyFontFace(fontFace, nameof(fontFace));
            if (pixelHeight <= 0) throw new ArgumentOutOfRangeException(nameof(pixelHeight), "Pixel height must be positive.");
            if (thickness < 0) throw new ArgumentOutOfRangeException(nameof(thickness), "Thickness cannot be negative.");
            NativeException.ThrowIfError(NativeMethods.ImgProcGetFontScaleFromHeight((int)fontFace, pixelHeight, thickness, out double result));
            return result;
        }

        private static void ValidateColormap(ColormapTypes value, string parameterName)
        {
            if (value < ColormapTypes.Autumn || value > ColormapTypes.DeepGreen)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported OpenCV color map.");
            }
        }

        private static void ValidateMarkerType(MarkerTypes value, string parameterName)
        {
            if (value < MarkerTypes.Cross || value > MarkerTypes.TriangleDown)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported marker type.");
            }
        }
    }
}
