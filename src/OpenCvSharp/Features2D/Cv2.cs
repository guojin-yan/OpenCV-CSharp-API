using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

#if NETCOREAPP3_1_OR_GREATER
using System.Buffers;
#endif

namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides feature drawing helpers aligned with OpenCV <c>cv</c> free functions.
    /// 提供与 OpenCV <c>cv</c> 自由函数对齐的特征绘制辅助函数。
    /// </summary>
    public static class Cv2
    {
#if NETCOREAPP3_1_OR_GREATER
        private const int StackallocKeyPointThreshold = 64;
        private const int StackallocDMatchThreshold = 128;
        private const int StackallocOffsetThreshold = 128;
#endif

        /// <summary>
        /// Draws keypoints on an image.
        /// 在图像上绘制关键点。
        /// </summary>
        /// <param name="image">The source image. 源图像。</param>
        /// <param name="keypoints">The keypoints to draw. 要绘制的关键点。</param>
        /// <param name="outImage">The output image. 输出图像。</param>
        /// <param name="color">The keypoint color, or all -1 to let OpenCV choose random colors. 关键点颜色，全部为 -1 时由 OpenCV 随机选择。</param>
        /// <param name="flags">The drawing flags. 绘制标志。</param>
        /// <exception cref="ArgumentNullException">Thrown when a required argument is null. 必需参数为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. native OpenCV 操作失败时抛出。</exception>
        public static void DrawKeypoints(
            Mat image,
            KeyPoint[] keypoints,
            Mat outImage,
            Scalar? color = null,
            DrawMatchesFlags flags = DrawMatchesFlags.Default)
        {
            ValidateMatPair(image, outImage);
            ValidateNotNull(keypoints, nameof(keypoints));
            ValidateDrawMatchesFlags(flags, nameof(flags));

            NativeKeyPoint[] nativeKeypoints = ToNativeKeyPoints(keypoints);
            Scalar actualColor = color ?? new Scalar(-1, -1, -1, -1);
            unsafe
            {
                fixed (NativeKeyPoint* keypointsPtr = nativeKeypoints)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DDrawKeypoints(
                        image.NativeHandle,
                        keypointsPtr,
                        nativeKeypoints.Length,
                        outImage.NativeHandle,
                        actualColor.V0,
                        actualColor.V1,
                        actualColor.V2,
                        actualColor.V3,
                        (int)flags));
                }
            }
        }

        /// <summary>
        /// Draws descriptor matches between two images.
        /// 绘制两幅图像之间的描述子匹配结果。
        /// </summary>
        /// <param name="img1">The first image. 第一幅图像。</param>
        /// <param name="keypoints1">Keypoints from the first image. 第一幅图像的关键点。</param>
        /// <param name="img2">The second image. 第二幅图像。</param>
        /// <param name="keypoints2">Keypoints from the second image. 第二幅图像的关键点。</param>
        /// <param name="matches">The matches to draw. 要绘制的匹配结果。</param>
        /// <param name="outImage">The output image. 输出图像。</param>
        /// <param name="matchColor">The match color, or all -1 for random colors. 匹配线颜色，全部为 -1 时随机选择。</param>
        /// <param name="singlePointColor">The single point color, or all -1 for random colors. 单点颜色，全部为 -1 时随机选择。</param>
        /// <param name="flags">The drawing flags. 绘制标志。</param>
        /// <exception cref="ArgumentNullException">Thrown when a required argument is null. 必需参数为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. native OpenCV 操作失败时抛出。</exception>
        public static void DrawMatches(
            Mat img1,
            KeyPoint[] keypoints1,
            Mat img2,
            KeyPoint[] keypoints2,
            DMatch[] matches,
            Mat outImage,
            Scalar? matchColor = null,
            Scalar? singlePointColor = null,
            DrawMatchesFlags flags = DrawMatchesFlags.Default)
        {
            ValidateDrawMatchesInputs(img1, keypoints1, img2, keypoints2, matches, outImage);
            ValidateDrawMatchesFlags(flags, nameof(flags));

            NativeKeyPoint[] nativeKeypoints1 = ToNativeKeyPoints(keypoints1);
            NativeKeyPoint[] nativeKeypoints2 = ToNativeKeyPoints(keypoints2);
            NativeDMatch[] nativeMatches = ToNativeMatches(matches);
            Scalar actualMatchColor = matchColor ?? new Scalar(-1, -1, -1, -1);
            Scalar actualSinglePointColor = singlePointColor ?? new Scalar(-1, -1, -1, -1);

            unsafe
            {
                fixed (NativeKeyPoint* keypoints1Ptr = nativeKeypoints1)
                fixed (NativeKeyPoint* keypoints2Ptr = nativeKeypoints2)
                fixed (NativeDMatch* matchesPtr = nativeMatches)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DDrawMatches(
                        img1.NativeHandle,
                        keypoints1Ptr,
                        nativeKeypoints1.Length,
                        img2.NativeHandle,
                        keypoints2Ptr,
                        nativeKeypoints2.Length,
                        matchesPtr,
                        nativeMatches.Length,
                        outImage.NativeHandle,
                        actualMatchColor.V0,
                        actualMatchColor.V1,
                        actualMatchColor.V2,
                        actualMatchColor.V3,
                        actualSinglePointColor.V0,
                        actualSinglePointColor.V1,
                        actualSinglePointColor.V2,
                        actualSinglePointColor.V3,
                        (int)flags));
                }
            }
        }

        /// <summary>
        /// Draws k-nearest descriptor matches between two images.
        /// 绘制两幅图像之间的 K 近邻描述子匹配结果。
        /// </summary>
        /// <param name="img1">The first image. 第一幅图像。</param>
        /// <param name="keypoints1">Keypoints from the first image. 第一幅图像的关键点。</param>
        /// <param name="img2">The second image. 第二幅图像。</param>
        /// <param name="keypoints2">Keypoints from the second image. 第二幅图像的关键点。</param>
        /// <param name="matches">The grouped matches to draw. 要绘制的分组匹配结果。</param>
        /// <param name="outImage">The output image. 输出图像。</param>
        /// <param name="matchColor">The match color, or all -1 for random colors. 匹配线颜色，全部为 -1 时随机选择。</param>
        /// <param name="singlePointColor">The single point color, or all -1 for random colors. 单点颜色，全部为 -1 时随机选择。</param>
        /// <param name="flags">The drawing flags. 绘制标志。</param>
        /// <exception cref="ArgumentNullException">Thrown when a required argument is null. 必需参数为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. native OpenCV 操作失败时抛出。</exception>
        public static void DrawMatchesKnn(
            Mat img1,
            KeyPoint[] keypoints1,
            Mat img2,
            KeyPoint[] keypoints2,
            DMatch[][] matches,
            Mat outImage,
            Scalar? matchColor = null,
            Scalar? singlePointColor = null,
            DrawMatchesFlags flags = DrawMatchesFlags.Default)
        {
            ValidateDrawMatchesKnnInputs(img1, keypoints1, img2, keypoints2, matches, outImage);
            ValidateDrawMatchesFlags(flags, nameof(flags));

            NativeKeyPoint[] nativeKeypoints1 = ToNativeKeyPoints(keypoints1);
            NativeKeyPoint[] nativeKeypoints2 = ToNativeKeyPoints(keypoints2);
            FlattenMatches(matches, out int[] offsets, out NativeDMatch[] nativeMatches);
            Scalar actualMatchColor = matchColor ?? new Scalar(-1, -1, -1, -1);
            Scalar actualSinglePointColor = singlePointColor ?? new Scalar(-1, -1, -1, -1);

            unsafe
            {
                fixed (NativeKeyPoint* keypoints1Ptr = nativeKeypoints1)
                fixed (NativeKeyPoint* keypoints2Ptr = nativeKeypoints2)
                fixed (int* offsetsPtr = offsets)
                fixed (NativeDMatch* matchesPtr = nativeMatches)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DDrawMatchesKnn(
                        img1.NativeHandle,
                        keypoints1Ptr,
                        nativeKeypoints1.Length,
                        img2.NativeHandle,
                        keypoints2Ptr,
                        nativeKeypoints2.Length,
                        offsetsPtr,
                        offsets.Length,
                        matchesPtr,
                        nativeMatches.Length,
                        outImage.NativeHandle,
                        actualMatchColor.V0,
                        actualMatchColor.V1,
                        actualMatchColor.V2,
                        actualMatchColor.V3,
                        actualSinglePointColor.V0,
                        actualSinglePointColor.V1,
                        actualSinglePointColor.V2,
                        actualSinglePointColor.V3,
                        (int)flags));
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Draws keypoints from a span-backed sequence.
        /// 从 Span 支持的序列绘制关键点。
        /// </summary>
        public static void DrawKeypoints(
            Mat image,
            ReadOnlySpan<KeyPoint> keypoints,
            Mat outImage,
            Scalar? color = null,
            DrawMatchesFlags flags = DrawMatchesFlags.Default)
        {
            ValidateMatPair(image, outImage);
            ValidateDrawMatchesFlags(flags, nameof(flags));

            NativeKeyPoint[]? rentedKeypoints = null;
            Span<NativeKeyPoint> nativeKeypoints = keypoints.Length <= StackallocKeyPointThreshold
                ? stackalloc NativeKeyPoint[keypoints.Length]
                : (rentedKeypoints = ArrayPool<NativeKeyPoint>.Shared.Rent(keypoints.Length)).AsSpan(0, keypoints.Length);
            Scalar actualColor = color ?? new Scalar(-1, -1, -1, -1);

            try
            {
                CopyToNative(keypoints, nativeKeypoints);
                unsafe
                {
                    fixed (NativeKeyPoint* keypointsPtr = nativeKeypoints)
                    {
                        NativeException.ThrowIfError(NativeMethods.Features2DDrawKeypoints(
                            image.NativeHandle,
                            keypointsPtr,
                            nativeKeypoints.Length,
                            outImage.NativeHandle,
                            actualColor.V0,
                            actualColor.V1,
                            actualColor.V2,
                            actualColor.V3,
                            (int)flags));
                    }
                }
            }
            finally
            {
                if (rentedKeypoints != null)
                {
                    ArrayPool<NativeKeyPoint>.Shared.Return(rentedKeypoints);
                }
            }
        }

        /// <summary>
        /// Draws descriptor matches from span-backed sequences.
        /// 从 Span 支持的序列绘制描述子匹配结果。
        /// </summary>
        public static void DrawMatches(
            Mat img1,
            ReadOnlySpan<KeyPoint> keypoints1,
            Mat img2,
            ReadOnlySpan<KeyPoint> keypoints2,
            ReadOnlySpan<DMatch> matches,
            Mat outImage,
            Scalar? matchColor = null,
            Scalar? singlePointColor = null,
            DrawMatchesFlags flags = DrawMatchesFlags.Default)
        {
            ValidateMatTriple(img1, img2, outImage);
            ValidateDrawMatchesFlags(flags, nameof(flags));

            NativeKeyPoint[]? rentedKeypoints1 = null;
            NativeKeyPoint[]? rentedKeypoints2 = null;
            NativeDMatch[]? rentedMatches = null;
            Span<NativeKeyPoint> nativeKeypoints1 = keypoints1.Length <= StackallocKeyPointThreshold
                ? stackalloc NativeKeyPoint[keypoints1.Length]
                : (rentedKeypoints1 = ArrayPool<NativeKeyPoint>.Shared.Rent(keypoints1.Length)).AsSpan(0, keypoints1.Length);
            Span<NativeKeyPoint> nativeKeypoints2 = keypoints2.Length <= StackallocKeyPointThreshold
                ? stackalloc NativeKeyPoint[keypoints2.Length]
                : (rentedKeypoints2 = ArrayPool<NativeKeyPoint>.Shared.Rent(keypoints2.Length)).AsSpan(0, keypoints2.Length);
            Span<NativeDMatch> nativeMatches = matches.Length <= StackallocDMatchThreshold
                ? stackalloc NativeDMatch[matches.Length]
                : (rentedMatches = ArrayPool<NativeDMatch>.Shared.Rent(matches.Length)).AsSpan(0, matches.Length);
            Scalar actualMatchColor = matchColor ?? new Scalar(-1, -1, -1, -1);
            Scalar actualSinglePointColor = singlePointColor ?? new Scalar(-1, -1, -1, -1);

            try
            {
                CopyToNative(keypoints1, nativeKeypoints1);
                CopyToNative(keypoints2, nativeKeypoints2);
                CopyToNative(matches, nativeMatches);
                unsafe
                {
                    fixed (NativeKeyPoint* keypoints1Ptr = nativeKeypoints1)
                    fixed (NativeKeyPoint* keypoints2Ptr = nativeKeypoints2)
                    fixed (NativeDMatch* matchesPtr = nativeMatches)
                    {
                        NativeException.ThrowIfError(NativeMethods.Features2DDrawMatches(
                            img1.NativeHandle,
                            keypoints1Ptr,
                            nativeKeypoints1.Length,
                            img2.NativeHandle,
                            keypoints2Ptr,
                            nativeKeypoints2.Length,
                            matchesPtr,
                            nativeMatches.Length,
                            outImage.NativeHandle,
                            actualMatchColor.V0,
                            actualMatchColor.V1,
                            actualMatchColor.V2,
                            actualMatchColor.V3,
                            actualSinglePointColor.V0,
                            actualSinglePointColor.V1,
                            actualSinglePointColor.V2,
                            actualSinglePointColor.V3,
                            (int)flags));
                    }
                }
            }
            finally
            {
                if (rentedKeypoints1 != null)
                {
                    ArrayPool<NativeKeyPoint>.Shared.Return(rentedKeypoints1);
                }

                if (rentedKeypoints2 != null)
                {
                    ArrayPool<NativeKeyPoint>.Shared.Return(rentedKeypoints2);
                }

                if (rentedMatches != null)
                {
                    ArrayPool<NativeDMatch>.Shared.Return(rentedMatches);
                }
            }
        }
#endif

        private static NativeKeyPoint[] ToNativeKeyPoints(KeyPoint[] keypoints)
        {
            var result = new NativeKeyPoint[keypoints.Length];
            for (int i = 0; i < keypoints.Length; i++)
            {
                result[i] = keypoints[i].ToNative();
            }

            return result;
        }

        private static NativeDMatch[] ToNativeMatches(DMatch[] matches)
        {
            var result = new NativeDMatch[matches.Length];
            for (int i = 0; i < matches.Length; i++)
            {
                result[i] = matches[i].ToNative();
            }

            return result;
        }

        private static void FlattenMatches(DMatch[][] matches, out int[] offsets, out NativeDMatch[] nativeMatches)
        {
            offsets = new int[matches.Length + 1];
            int totalCount = 0;
            for (int i = 0; i < matches.Length; i++)
            {
                if (matches[i] == null)
                {
                    throw new ArgumentNullException(nameof(matches));
                }

                offsets[i] = totalCount;
                totalCount += matches[i].Length;
            }

            offsets[matches.Length] = totalCount;
            nativeMatches = new NativeDMatch[totalCount];
            int index = 0;
            for (int i = 0; i < matches.Length; i++)
            {
                DMatch[] group = matches[i];
                for (int j = 0; j < group.Length; j++)
                {
                    nativeMatches[index] = group[j].ToNative();
                    index++;
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void CopyToNative(ReadOnlySpan<KeyPoint> source, Span<NativeKeyPoint> destination)
        {
            for (int i = 0; i < source.Length; i++)
            {
                destination[i] = source[i].ToNative();
            }
        }

        private static void CopyToNative(ReadOnlySpan<DMatch> source, Span<NativeDMatch> destination)
        {
            for (int i = 0; i < source.Length; i++)
            {
                destination[i] = source[i].ToNative();
            }
        }
#endif

        private static void ValidateDrawMatchesInputs(Mat img1, KeyPoint[] keypoints1, Mat img2, KeyPoint[] keypoints2, DMatch[] matches, Mat outImage)
        {
            ValidateMatTriple(img1, img2, outImage);
            ValidateNotNull(keypoints1, nameof(keypoints1));
            ValidateNotNull(keypoints2, nameof(keypoints2));
            ValidateNotNull(matches, nameof(matches));
        }

        private static void ValidateDrawMatchesKnnInputs(Mat img1, KeyPoint[] keypoints1, Mat img2, KeyPoint[] keypoints2, DMatch[][] matches, Mat outImage)
        {
            ValidateMatTriple(img1, img2, outImage);
            ValidateNotNull(keypoints1, nameof(keypoints1));
            ValidateNotNull(keypoints2, nameof(keypoints2));
            ValidateNotNull(matches, nameof(matches));
        }

        private static void ValidateMatPair(Mat src, Mat dst)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
        }

        private static void ValidateMatTriple(Mat mat1, Mat mat2, Mat mat3)
        {
            ValidateNotNull(mat1, nameof(mat1));
            ValidateNotNull(mat2, nameof(mat2));
            ValidateNotNull(mat3, nameof(mat3));
        }

        private static void ValidateDrawMatchesFlags(DrawMatchesFlags value, string parameterName)
        {
            const DrawMatchesFlags validMask =
                DrawMatchesFlags.DrawOverOutImg |
                DrawMatchesFlags.NotDrawSinglePoints |
                DrawMatchesFlags.DrawRichKeypoints;
            if ((value & ~validMask) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unknown draw-matches flag bits are not supported.");
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
