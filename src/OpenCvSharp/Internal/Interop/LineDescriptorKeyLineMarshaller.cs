using System;
using JYPPX.OpenCvSharp.LineDescriptor;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static class LineDescriptorKeyLineMarshaller
    {
#if NETCOREAPP3_1_OR_GREATER
        internal const int StackallocThreshold = 64;
#endif

        internal static NativeLineDescriptorKeyLine[] ToNative(KeyLine[] keylines)
        {
            if (keylines == null)
            {
                throw new ArgumentNullException(nameof(keylines));
            }

            var result = new NativeLineDescriptorKeyLine[keylines.Length];
            for (int i = 0; i < keylines.Length; i++)
            {
                result[i] = keylines[i].ToNative();
            }

            return result;
        }

        internal static KeyLine[] FromNative(NativeLineDescriptorKeyLine[] keylines, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<KeyLine>();
            }

            int resultCount = Math.Min(count, keylines.Length);
            var result = new KeyLine[resultCount];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = KeyLine.FromNative(keylines[i]);
            }

            return result;
        }

#if NETCOREAPP3_1_OR_GREATER
        internal static void CopyToNative(ReadOnlySpan<KeyLine> source, Span<NativeLineDescriptorKeyLine> destination)
        {
            for (int i = 0; i < source.Length; i++)
            {
                destination[i] = source[i].ToNative();
            }
        }

        internal static KeyLine[] FromNative(ReadOnlySpan<NativeLineDescriptorKeyLine> keylines, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<KeyLine>();
            }

            int resultCount = Math.Min(count, keylines.Length);
            var result = new KeyLine[resultCount];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = KeyLine.FromNative(keylines[i]);
            }

            return result;
        }
#endif
    }
}
