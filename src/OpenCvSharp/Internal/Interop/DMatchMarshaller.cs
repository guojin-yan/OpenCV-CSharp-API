using System;
using OpenCvSharp.Features2D;

namespace OpenCvSharp.Internal.Interop
{
    internal static class DMatchMarshaller
    {
        internal static NativeDMatch[] ToNative(DMatch[] matches)
        {
            if (matches == null)
            {
                throw new ArgumentNullException(nameof(matches));
            }

            var result = new NativeDMatch[matches.Length];
            for (int i = 0; i < matches.Length; i++)
            {
                result[i] = matches[i].ToNative();
            }

            return result;
        }

        internal static DMatch[] FromNative(NativeDMatch[] matches, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<DMatch>();
            }

            var result = new DMatch[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = DMatch.FromNative(matches[i]);
            }

            return result;
        }

        internal static DMatch[][] FromGroupedNative(int[] offsets, int groupCount, NativeDMatch[] matches, int matchCount)
        {
            if (groupCount <= 0)
            {
                return Array.Empty<DMatch[]>();
            }

            var result = new DMatch[groupCount][];
            for (int i = 0; i < groupCount; i++)
            {
                int start = offsets[i];
                int end = offsets[i + 1];
                int length = Math.Max(0, Math.Min(end, matchCount) - start);
                var group = new DMatch[length];
                for (int j = 0; j < length; j++)
                {
                    group[j] = DMatch.FromNative(matches[start + j]);
                }

                result[i] = group;
            }

            return result;
        }

        internal static void Flatten(DMatch[][] matches, out int[] offsets, out NativeDMatch[] nativeMatches)
        {
            if (matches == null)
            {
                throw new ArgumentNullException(nameof(matches));
            }

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
    }
}
