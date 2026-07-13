using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.LineDescriptor
{
    /// <summary>
    /// Drawing helpers for OpenCV line_descriptor key lines and matches.
    /// OpenCV line_descriptor 关键线段与匹配结果绘图辅助函数。
    /// </summary>
    public static class LineDescriptorCv2
    {
        /// <summary>
        /// Draws key lines into an output image.
        /// 将关键线段绘制到输出图像。
        /// </summary>
        public static void DrawKeylines(
            Mat image,
            KeyLine[] keylines,
            Mat outImage,
            Scalar color,
            DrawLinesMatchesFlags flags = DrawLinesMatchesFlags.Default)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(keylines, nameof(keylines));
            ValidateNotNull(outImage, nameof(outImage));
            ValidateDrawLinesMatchesFlags(flags, nameof(flags));
            NativeLineDescriptorKeyLine[] nativeKeylines = LineDescriptorKeyLineMarshaller.ToNative(keylines);
            unsafe
            {
                fixed (NativeLineDescriptorKeyLine* keylinesPtr = nativeKeylines)
                {
                    NativeException.ThrowIfError(NativeMethods.LineDescriptorDrawKeylines(
                        image.NativeHandle,
                        keylinesPtr,
                        nativeKeylines.Length,
                        outImage.NativeHandle,
                        color.V0,
                        color.V1,
                        color.V2,
                        color.V3,
                        (int)flags));
                }
            }
        }

        /// <summary>
        /// Draws key lines and returns the output image.
        /// 绘制关键线段并返回输出图像。
        /// </summary>
        public static Mat DrawKeylines(
            Mat image,
            KeyLine[] keylines,
            Scalar color,
            DrawLinesMatchesFlags flags = DrawLinesMatchesFlags.Default)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(keylines, nameof(keylines));
            ValidateDrawLinesMatchesFlags(flags, nameof(flags));
            var outImage = new Mat();
            try
            {
                DrawKeylines(image, keylines, outImage, color, flags);
                return outImage;
            }
            catch
            {
                outImage.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Draws key lines using the default random OpenCV color.
        /// 使用 OpenCV 默认随机颜色绘制关键线段。
        /// </summary>
        public static Mat DrawKeylines(Mat image, KeyLine[] keylines, DrawLinesMatchesFlags flags = DrawLinesMatchesFlags.Default)
        {
            return DrawKeylines(image, keylines, new Scalar(-1, -1, -1, -1), flags);
        }

        /// <summary>
        /// Draws line matches into an output image.
        /// 将线段匹配绘制到输出图像。
        /// </summary>
        public static void DrawLineMatches(
            Mat image1,
            KeyLine[] keylines1,
            Mat image2,
            KeyLine[] keylines2,
            DMatch[] matches,
            Mat outImage,
            Scalar matchColor,
            Scalar singleLineColor,
            DrawLinesMatchesFlags flags = DrawLinesMatchesFlags.Default)
        {
            ValidateNotNull(image1, nameof(image1));
            ValidateNotNull(keylines1, nameof(keylines1));
            ValidateNotNull(image2, nameof(image2));
            ValidateNotNull(keylines2, nameof(keylines2));
            ValidateNotNull(matches, nameof(matches));
            ValidateNotNull(outImage, nameof(outImage));
            ValidateDrawLinesMatchesFlags(flags, nameof(flags));

            NativeLineDescriptorKeyLine[] nativeKeylines1 = LineDescriptorKeyLineMarshaller.ToNative(keylines1);
            NativeLineDescriptorKeyLine[] nativeKeylines2 = LineDescriptorKeyLineMarshaller.ToNative(keylines2);
            NativeDMatch[] nativeMatches = DMatchMarshaller.ToNative(matches);
            unsafe
            {
                fixed (NativeLineDescriptorKeyLine* keylines1Ptr = nativeKeylines1)
                fixed (NativeLineDescriptorKeyLine* keylines2Ptr = nativeKeylines2)
                fixed (NativeDMatch* matchesPtr = nativeMatches)
                {
                    NativeException.ThrowIfError(NativeMethods.LineDescriptorDrawLineMatches(
                        image1.NativeHandle,
                        keylines1Ptr,
                        nativeKeylines1.Length,
                        image2.NativeHandle,
                        keylines2Ptr,
                        nativeKeylines2.Length,
                        matchesPtr,
                        nativeMatches.Length,
                        outImage.NativeHandle,
                        matchColor.V0,
                        matchColor.V1,
                        matchColor.V2,
                        matchColor.V3,
                        singleLineColor.V0,
                        singleLineColor.V1,
                        singleLineColor.V2,
                        singleLineColor.V3,
                        (int)flags));
                }
            }
        }

        /// <summary>
        /// Draws line matches and returns the output image.
        /// 绘制线段匹配并返回输出图像。
        /// </summary>
        public static Mat DrawLineMatches(
            Mat image1,
            KeyLine[] keylines1,
            Mat image2,
            KeyLine[] keylines2,
            DMatch[] matches,
            Scalar matchColor,
            Scalar singleLineColor,
            DrawLinesMatchesFlags flags = DrawLinesMatchesFlags.Default)
        {
            ValidateNotNull(image1, nameof(image1));
            ValidateNotNull(keylines1, nameof(keylines1));
            ValidateNotNull(image2, nameof(image2));
            ValidateNotNull(keylines2, nameof(keylines2));
            ValidateNotNull(matches, nameof(matches));
            ValidateDrawLinesMatchesFlags(flags, nameof(flags));
            var outImage = new Mat();
            try
            {
                DrawLineMatches(image1, keylines1, image2, keylines2, matches, outImage, matchColor, singleLineColor, flags);
                return outImage;
            }
            catch
            {
                outImage.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Draws line matches using OpenCV default random colors.
        /// 使用 OpenCV 默认随机颜色绘制线段匹配。
        /// </summary>
        public static Mat DrawLineMatches(
            Mat image1,
            KeyLine[] keylines1,
            Mat image2,
            KeyLine[] keylines2,
            DMatch[] matches,
            DrawLinesMatchesFlags flags = DrawLinesMatchesFlags.Default)
        {
            return DrawLineMatches(
                image1,
                keylines1,
                image2,
                keylines2,
                matches,
                new Scalar(-1, -1, -1, -1),
                new Scalar(-1, -1, -1, -1),
                flags);
        }

        private static void ValidateDrawLinesMatchesFlags(DrawLinesMatchesFlags value, string parameterName)
        {
            if (value != DrawLinesMatchesFlags.Default &&
                value != DrawLinesMatchesFlags.DrawOverOutImg &&
                value != DrawLinesMatchesFlags.NotDrawSingleLines)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported line-descriptor drawing flag value.");
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
    }
}
