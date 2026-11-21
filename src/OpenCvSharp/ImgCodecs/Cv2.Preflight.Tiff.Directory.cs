using System;
using System.Collections.Generic;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private struct TiffSampleScan
        {
            public long RemainingValues;
            public int CachedOffset;
            public ulong CachedCount;
            public int CachedDepth;
        }

        private static TiffFacts ReadTiffDirectoryFacts(byte[] data, bool bigTiff)
        {
            const int maxDirectoriesInspected = 65536;
            int headerSize = bigTiff ? 16 : 8;
            int countSize = bigTiff ? 8 : 2;
            int slotSize = bigTiff ? 8 : 4;
            int entrySize = bigTiff ? 20 : 12;
            if (data.Length < headerSize) return default(TiffFacts);
            bool littleEndian = data[0] == (byte)'I' && data[1] == (byte)'I';
            if (!littleEndian && !(data[0] == (byte)'M' && data[1] == (byte)'M')) return default(TiffFacts);
            if (ReadTiff16(data, 2, littleEndian) != (bigTiff ? 43 : 42) ||
                (bigTiff && (ReadTiff16(data, 4, littleEndian) != 8 || ReadTiff16(data, 6, littleEndian) != 0)))
                return default(TiffFacts);

            ulong ifd = bigTiff ? ReadTiff64(data, 8, littleEndian) : ReadTiff32(data, 4, littleEndian);
            if (ifd == 0) return default(TiffFacts);
            var visited = new HashSet<ulong>();
            long remainingEntries = data.Length / entrySize;
            var samples = new TiffSampleScan { RemainingValues = data.Length };
            int pageCount = 0, commonWidth = 0, commonHeight = 0, commonDepth = 0, commonChannels = 0;
            bool uniformSize = true, uniformDepth = true, uniformChannels = true, cumulativeKnown = true;
            long cumulative = 0;
            while (ifd != 0)
            {
                if (pageCount == maxDirectoriesInspected || ifd < (ulong)headerSize || ifd > (ulong)data.Length ||
                    !visited.Add(ifd)) return default(TiffFacts);
                int offset = (int)ifd;
                if (data.Length - offset < countSize + slotSize) return default(TiffFacts);
                ulong entries = bigTiff ? ReadTiff64(data, offset, littleEndian) : ReadTiff16(data, offset, littleEndian);
                // Division bounds every later product/conversion by the actual input, including
                // hostile 64-bit directory counts. Shared/overlapping IFDs cannot multiply work.
                if (entries > (ulong)((data.Length - offset - countSize - slotSize) / entrySize) ||
                    entries > (ulong)remainingEntries) return default(TiffFacts);
                remainingEntries -= (long)entries;
                int entryOffset = offset + countSize;
                int seenFacts = 0;
                int width = 0, height = 0, channels = 0, depth = 0;
                bool widthKnown = false, heightKnown = false, channelsKnown = false, depthKnown = false;
                int bitsOffset = 0;
                ulong bitsCount = 0;
                ushort bitsType = 0;
                for (ulong index = 0; index < entries; ++index, entryOffset += entrySize)
                {
                    ushort tag = ReadTiff16(data, entryOffset, littleEndian);
                    ushort type = ReadTiff16(data, entryOffset + 2, littleEndian);
                    ulong count = bigTiff ? ReadTiff64(data, entryOffset + 4, littleEndian) : ReadTiff32(data, entryOffset + 4, littleEndian);
                    int valueOffset;
                    if (!TryReadTiffValueRange(data, entryOffset, type, count, littleEndian, bigTiff, out valueOffset))
                        return default(TiffFacts);
                    int fact = tag == 256 ? 1 : tag == 257 ? 2 : tag == 258 ? 4 : tag == 277 ? 8 : 0;
                    // Native readers may select the first duplicate or process it again. No
                    // last-tag-wins value is a trustworthy size/channel/storage admission fact.
                    if (fact != 0 && (seenFacts & fact) != 0) return default(TiffFacts);
                    seenFacts |= fact;
                    if (tag == 256 || tag == 257)
                    {
                        ulong value = 0;
                        bool known = count == 1 && (type == 3 || type == 4 || (bigTiff && type == 16));
                        if (known) value = type == 3 ? ReadTiff16(data, valueOffset, littleEndian) :
                            type == 4 ? ReadTiff32(data, valueOffset, littleEndian) : ReadTiff64(data, valueOffset, littleEndian);
                        known = known && value > 0 && value <= int.MaxValue;
                        if (tag == 256) { width = known ? (int)value : 0; widthKnown = known; }
                        else { height = known ? (int)value : 0; heightKnown = known; }
                    }
                    else if (tag == 258)
                    {
                        bitsOffset = valueOffset; bitsCount = count; bitsType = type;
                    }
                    else if (tag == 277)
                    {
                        channelsKnown = type == 3 && count == 1;
                        if (channelsKnown) channels = ReadTiff16(data, valueOffset, littleEndian);
                        channelsKnown = channelsKnown && channels > 0;
                    }
                }
                if (bitsType == 3 && bitsCount > 0 && bitsCount <= ushort.MaxValue &&
                    (bitsCount == 1 || (channelsKnown && bitsCount == (ulong)channels)))
                    depthKnown = TryReadUniformTiffSamples(data, bitsOffset, bitsCount, littleEndian, ref samples, out depth);

                if (!widthKnown || !heightKnown) { uniformSize = false; cumulativeKnown = false; }
                else
                {
                    if (pageCount == 0) { commonWidth = width; commonHeight = height; }
                    else if (width != commonWidth || height != commonHeight) uniformSize = false;
                    if (cumulativeKnown)
                    {
                        try { cumulative = checked(cumulative + (long)width * height); }
                        catch (OverflowException) { cumulative = 0; cumulativeKnown = false; }
                    }
                }
                if (pageCount == 0) { commonDepth = depth; commonChannels = channels; }
                uniformDepth &= depthKnown && depth == commonDepth;
                uniformChannels &= channelsKnown && channels == commonChannels;
                ++pageCount;
                ifd = bigTiff ? ReadTiff64(data, entryOffset, littleEndian) : ReadTiff32(data, entryOffset, littleEndian);
            }
            return new TiffFacts
            {
                Width = uniformSize ? commonWidth : 0, Height = uniformSize ? commonHeight : 0, SizeKnown = uniformSize,
                PageCount = pageCount, PageCountKnown = true,
                CumulativePixelCount = cumulativeKnown ? cumulative : 0, CumulativePixelCountKnown = cumulativeKnown,
                BitDepth = uniformDepth ? commonDepth : 0, BitDepthKnown = uniformDepth,
                Channels = uniformChannels ? commonChannels : 0, ChannelsKnown = uniformChannels
            };
        }

        private static bool TryReadTiffValueRange(byte[] data, int entry, ushort type, ulong count,
            bool littleEndian, bool bigTiff, out int valueOffset)
        {
            valueOffset = entry + (bigTiff ? 12 : 8);
            int size;
            switch (type)
            {
                case 1: case 2: case 6: case 7: size = 1; break;
                case 3: case 8: size = 2; break;
                case 4: case 9: case 11: case 13: size = 4; break;
                case 5: case 10: case 12: size = 8; break;
                case 16: case 17: case 18: if (!bigTiff) return false; size = 8; break;
                default: return false;
            }
            if (count > ulong.MaxValue / (ulong)size) return false;
            ulong bytes = count * (ulong)size;
            if (bytes > (bigTiff ? 8UL : 4UL))
            {
                ulong offset = bigTiff ? ReadTiff64(data, valueOffset, littleEndian) : ReadTiff32(data, valueOffset, littleEndian);
                if (offset > (ulong)data.Length || bytes > (ulong)data.Length - offset) return false;
                valueOffset = (int)offset;
            }
            return true;
        }

        private static bool TryReadUniformTiffSamples(byte[] data, int offset, ulong count, bool littleEndian,
            ref TiffSampleScan scan, out int depth)
        {
            depth = 0;
            if (scan.CachedDepth != 0 && scan.CachedOffset == offset && scan.CachedCount == count)
            {
                depth = scan.CachedDepth;
                return true;
            }
            if (count > (ulong)scan.RemainingValues) return false;
            scan.RemainingValues -= (long)count;
            ushort first = ReadTiff16(data, offset, littleEndian);
            if (first == 0 || first > 64) return false;
            for (int index = 1; index < (int)count; ++index)
                if (ReadTiff16(data, offset + index * 2, littleEndian) != first) return false;
            depth = first;
            scan.CachedOffset = offset; scan.CachedCount = count; scan.CachedDepth = depth;
            return true;
        }
    }
}
