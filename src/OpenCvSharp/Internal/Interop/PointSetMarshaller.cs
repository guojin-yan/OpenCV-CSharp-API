using System;
using JYPPX.OpenCvSharp.Core;

#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    /// <summary>
    /// Provides point-set validation and interleaved memory views for imgproc interop.
    /// 提供 imgproc 互操作使用的点集校验和交错内存视图。
    /// </summary>
    internal static class PointSetMarshaller
    {
        /// <summary>
        /// Builds grouped 2D point arrays from an offset table and a flat point buffer.
        /// 根据偏移表和扁平点缓冲区构建二维点分组数组。
        /// </summary>
        /// <param name="offsets">The group offsets, including the final end offset. 分组偏移，包含最终结束偏移。</param>
        /// <param name="points">The flat point buffer. 扁平点缓冲区。</param>
        /// <param name="groupCount">The number of groups to read. 要读取的分组数量。</param>
        /// <returns>The grouped points. 分组后的点。</returns>
        internal static Point2f[][] ToPoint2fGroups(int[] offsets, Point2f[] points, int groupCount)
        {
            if (groupCount <= 0)
            {
                return Array.Empty<Point2f[]>();
            }

            var result = new Point2f[groupCount][];
            for (int i = 0; i < groupCount; i++)
            {
                int start = offsets[i];
                int end = offsets[i + 1];
                if (start < 0 || end < start || end > points.Length)
                {
                    result[i] = Array.Empty<Point2f>();
                    continue;
                }

                int length = end - start;
                var group = new Point2f[length];
                Array.Copy(points, start, group, 0, length);
                result[i] = group;
            }

            return result;
        }

        /// <summary>
        /// Flattens grouped 2D points into offsets and a contiguous point buffer.
        /// 将分组二维点展平为偏移表和连续点缓冲区。
        /// </summary>
        /// <param name="groups">The point groups. 点分组。</param>
        /// <param name="parameterName">The parameter name. 参数名。</param>
        /// <param name="offsets">The group offsets, including the final end offset. 分组偏移，包含最终结束偏移。</param>
        /// <param name="points">The flat point buffer. 扁平点缓冲区。</param>
        internal static void FlattenPoint2fGroups(Point2f[][] groups, string parameterName, out int[] offsets, out Point2f[] points)
        {
            if (groups == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            offsets = new int[groups.Length + 1];
            int total = 0;
            for (int i = 0; i < groups.Length; i++)
            {
                Point2f[] group = groups[i];
                if (group == null)
                {
                    throw new ArgumentException("Point group cannot be null.", parameterName);
                }

                total += group.Length;
                offsets[i + 1] = total;
            }

            points = new Point2f[total];
            int offset = 0;
            for (int i = 0; i < groups.Length; i++)
            {
                Point2f[] group = groups[i];
                Array.Copy(group, 0, points, offset, group.Length);
                offset += group.Length;
            }
        }

        /// <summary>
        /// Flattens grouped 3D points into offsets and a contiguous point buffer.
        /// 将分组三维点展平为偏移表和连续点缓冲区。
        /// </summary>
        /// <param name="groups">The point groups. 点分组。</param>
        /// <param name="parameterName">The parameter name. 参数名。</param>
        /// <param name="offsets">The group offsets, including the final end offset. 分组偏移，包含最终结束偏移。</param>
        /// <param name="points">The flat point buffer. 扁平点缓冲区。</param>
        internal static void FlattenPoint3fGroups(Point3f[][] groups, string parameterName, out int[] offsets, out Point3f[] points)
        {
            if (groups == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            offsets = new int[groups.Length + 1];
            int total = 0;
            for (int i = 0; i < groups.Length; i++)
            {
                Point3f[] group = groups[i];
                if (group == null)
                {
                    throw new ArgumentException("Point group cannot be null.", parameterName);
                }

                total += group.Length;
                offsets[i + 1] = total;
            }

            points = new Point3f[total];
            int offset = 0;
            for (int i = 0; i < groups.Length; i++)
            {
                Point3f[] group = groups[i];
                Array.Copy(group, 0, points, offset, group.Length);
                offset += group.Length;
            }
        }

        /// <summary>
        /// Validates that an array is not null or empty.
        /// 验证数组不为 null 且不为空。
        /// </summary>
        /// <param name="values">The array to validate. 要验证的数组。</param>
        /// <param name="parameterName">The parameter name. 参数名。</param>
        internal static void ValidateNotEmpty<T>(T[] values, string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (values.Length == 0)
            {
                throw new ArgumentException("Array cannot be empty.", parameterName);
            }
        }

        /// <summary>
        /// Validates that an array contains at least the requested number of elements.
        /// 验证数组至少包含指定数量的元素。
        /// </summary>
        /// <param name="values">The array to validate. 要验证的数组。</param>
        /// <param name="minimumCount">The minimum required count. 所需的最小数量。</param>
        /// <param name="parameterName">The parameter name. 参数名。</param>
        internal static void ValidateCountAtLeast<T>(T[] values, int minimumCount, string parameterName)
        {
            ValidateNotEmpty(values, parameterName);

            if (values.Length < minimumCount)
            {
                throw new ArgumentException("Array does not contain enough elements.", parameterName);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Validates that a span is not empty.
        /// 验证 Span 不为空。
        /// </summary>
        /// <param name="values">The span to validate. 要验证的 Span。</param>
        /// <param name="parameterName">The parameter name. 参数名。</param>
        internal static void ValidateNotEmpty<T>(ReadOnlySpan<T> values, string parameterName)
        {
            if (values.IsEmpty)
            {
                throw new ArgumentException("Span cannot be empty.", parameterName);
            }
        }

        /// <summary>
        /// Validates that a span contains at least the requested number of elements.
        /// 验证 Span 至少包含指定数量的元素。
        /// </summary>
        /// <param name="values">The span to validate. 要验证的 Span。</param>
        /// <param name="minimumCount">The minimum required count. 所需的最小数量。</param>
        /// <param name="parameterName">The parameter name. 参数名。</param>
        internal static void ValidateCountAtLeast<T>(ReadOnlySpan<T> values, int minimumCount, string parameterName)
        {
            ValidateNotEmpty(values, parameterName);

            if (values.Length < minimumCount)
            {
                throw new ArgumentException("Span does not contain enough elements.", parameterName);
            }
        }

        /// <summary>
        /// Reinterprets point memory as an interleaved integer span.
        /// 将点内存重解释为交错的整数 Span。
        /// </summary>
        /// <param name="points">The point span. 点 Span。</param>
        /// <returns>The interleaved <c>x, y</c> values. 交错的 <c>x, y</c> 值。</returns>
        internal static ReadOnlySpan<int> AsInterleaved(ReadOnlySpan<Point> points)
        {
            return MemoryMarshal.Cast<Point, int>(points);
        }

        /// <summary>
        /// Reinterprets point memory as an interleaved integer span.
        /// 将点内存重解释为交错的整数 Span。
        /// </summary>
        /// <param name="points">The point span. 点 Span。</param>
        /// <returns>The interleaved <c>x, y</c> values. 交错的 <c>x, y</c> 值。</returns>
        internal static Span<int> AsInterleaved(Span<Point> points)
        {
            return MemoryMarshal.Cast<Point, int>(points);
        }

        /// <summary>
        /// Reinterprets point memory as an interleaved float span.
        /// 将点内存重解释为交错的浮点 Span。
        /// </summary>
        /// <param name="points">The point span. 点 Span。</param>
        /// <returns>The interleaved <c>x, y</c> values. 交错的 <c>x, y</c> 值。</returns>
        internal static ReadOnlySpan<float> AsInterleaved(ReadOnlySpan<Point2f> points)
        {
            return MemoryMarshal.Cast<Point2f, float>(points);
        }

        /// <summary>
        /// Reinterprets point memory as an interleaved float span.
        /// 将点内存重解释为交错的浮点 Span。
        /// </summary>
        /// <param name="points">The point span. 点 Span。</param>
        /// <returns>The interleaved <c>x, y</c> values. 交错的 <c>x, y</c> 值。</returns>
        internal static Span<float> AsInterleaved(Span<Point2f> points)
        {
            return MemoryMarshal.Cast<Point2f, float>(points);
        }

        /// <summary>
        /// Reinterprets 3D point memory as an interleaved float span.
        /// 将三维点内存重解释为交错的浮点 Span。
        /// </summary>
        /// <param name="points">The point span. 点 Span。</param>
        /// <returns>The interleaved <c>x, y, z</c> values. 交错的 <c>x, y, z</c> 值。</returns>
        internal static ReadOnlySpan<float> AsInterleaved(ReadOnlySpan<Point3f> points)
        {
            return MemoryMarshal.Cast<Point3f, float>(points);
        }

        /// <summary>
        /// Reinterprets 3D point memory as an interleaved float span.
        /// 将三维点内存重解释为交错的浮点 Span。
        /// </summary>
        /// <param name="points">The point span. 点 Span。</param>
        /// <returns>The interleaved <c>x, y, z</c> values. 交错的 <c>x, y, z</c> 值。</returns>
        internal static Span<float> AsInterleaved(Span<Point3f> points)
        {
            return MemoryMarshal.Cast<Point3f, float>(points);
        }
#endif
    }
}
