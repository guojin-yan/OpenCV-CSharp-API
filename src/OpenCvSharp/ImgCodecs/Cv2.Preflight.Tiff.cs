using System;
using System.Collections.Generic;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private struct TiffFacts
        {
            public int Width;
            public int Height;
            public bool SizeKnown;
            public int PageCount;
            public bool PageCountKnown;
            public long CumulativePixelCount;
            public bool CumulativePixelCountKnown;
        }

        private static TiffFacts ReadTiffFacts(byte[] data)
        {
            const int maxDirectoriesInspected = 65536;
            TiffFacts facts = new TiffFacts();
            if (data.Length < 8) return facts;

            bool littleEndian;
            if (data[0] == (byte)'I' && data[1] == (byte)'I') littleEndian = true;
            else if (data[0] == (byte)'M' && data[1] == (byte)'M') littleEndian = false;
            else return facts;
            if (ReadTiff16(data, 2, littleEndian) != 42) return facts;

            uint ifdOffset = ReadTiff32(data, 4, littleEndian);
            if (ifdOffset < 8 || ifdOffset > int.MaxValue) return facts;

            var visited = new HashSet<uint>();
            int pageCount = 0;
            int commonWidth = 0;
            int commonHeight = 0;
            bool uniformPageSize = true;
            long cumulativePixelCount = 0;
            bool cumulativePixelCountKnown = true;
            while (ifdOffset != 0)
            {
                if (pageCount == maxDirectoriesInspected || !visited.Add(ifdOffset) || ifdOffset < 8 || ifdOffset > int.MaxValue) return new TiffFacts();
                int offset = (int)ifdOffset;
                if ((long)offset + 2L > data.Length) return new TiffFacts();

                ushort entryCount = ReadTiff16(data, offset, littleEndian);
                long entriesEnd = (long)offset + 2L + entryCount * 12L + 4L;
                if (entriesEnd > data.Length) return new TiffFacts();

                bool widthKnown = false;
                bool heightKnown = false;
                int width = 0;
                int height = 0;
                int entryOffset = offset + 2;
                for (int index = 0; index < entryCount; ++index, entryOffset += 12)
                {
                    ushort tag = ReadTiff16(data, entryOffset, littleEndian);
                    ushort type = ReadTiff16(data, entryOffset + 2, littleEndian);
                    uint count = ReadTiff32(data, entryOffset + 4, littleEndian);
                    int typeSize;
                    if (!TryGetTiffTypeSize(type, out typeSize)) return new TiffFacts();
                    ulong payloadBytes = (ulong)typeSize * count;
                    if (payloadBytes > 4)
                    {
                        uint valueOffset = ReadTiff32(data, entryOffset + 8, littleEndian);
                        if (valueOffset > int.MaxValue || valueOffset > data.Length || payloadBytes > (ulong)data.Length - valueOffset) return new TiffFacts();
                    }

                    long scalar;
                    if (tag == 256 && TryReadTiffScalar(data, entryOffset, type, count, littleEndian, out scalar) && scalar > 0 && scalar <= int.MaxValue)
                    {
                        width = (int)scalar;
                        widthKnown = true;
                    }
                    else if (tag == 257 && TryReadTiffScalar(data, entryOffset, type, count, littleEndian, out scalar) && scalar > 0 && scalar <= int.MaxValue)
                    {
                        height = (int)scalar;
                        heightKnown = true;
                    }
                }

                if (!widthKnown || !heightKnown)
                {
                    uniformPageSize = false;
                    cumulativePixelCountKnown = false;
                }
                else if (pageCount == 0)
                {
                    commonWidth = width;
                    commonHeight = height;
                }
                else if (width != commonWidth || height != commonHeight)
                {
                    uniformPageSize = false;
                }
                if (widthKnown && heightKnown && cumulativePixelCountKnown)
                {
                    try
                    {
                        cumulativePixelCount = checked(cumulativePixelCount + checked((long)width * height));
                    }
                    catch (OverflowException)
                    {
                        cumulativePixelCount = 0;
                        cumulativePixelCountKnown = false;
                    }
                }
                ++pageCount;
                ifdOffset = ReadTiff32(data, offset + 2 + entryCount * 12, littleEndian);
            }

            if (pageCount > 0)
            {
                facts.PageCount = pageCount;
                facts.PageCountKnown = true;
                facts.CumulativePixelCount = cumulativePixelCount;
                facts.CumulativePixelCountKnown = cumulativePixelCountKnown;
                if (uniformPageSize)
                {
                    facts.Width = commonWidth;
                    facts.Height = commonHeight;
                    facts.SizeKnown = true;
                }
            }
            return facts;
        }

        private static TiffFacts ReadBigTiffFacts(byte[] data)
        {
            const int maxDirectoriesInspected = 65536;
            TiffFacts facts = new TiffFacts();
            if (data.Length < 16) return facts;

            bool littleEndian;
            if (data[0] == (byte)'I' && data[1] == (byte)'I') littleEndian = true;
            else if (data[0] == (byte)'M' && data[1] == (byte)'M') littleEndian = false;
            else return facts;
            if (ReadTiff16(data, 2, littleEndian) != 43 || ReadTiff16(data, 4, littleEndian) != 8 ||
                ReadTiff16(data, 6, littleEndian) != 0) return facts;

            ulong ifdOffset = ReadTiff64(data, 8, littleEndian);
            if (ifdOffset < 16 || ifdOffset > (ulong)int.MaxValue) return facts;

            var visited = new HashSet<ulong>();
            int pageCount = 0;
            int commonWidth = 0;
            int commonHeight = 0;
            bool uniformPageSize = true;
            long cumulativePixelCount = 0;
            bool cumulativePixelCountKnown = true;
            while (ifdOffset != 0)
            {
                if (pageCount == maxDirectoriesInspected || !visited.Add(ifdOffset) || ifdOffset < 16 || ifdOffset > int.MaxValue)
                {
                    return new TiffFacts();
                }

                int offset = (int)ifdOffset;
                if ((long)offset + 8L > data.Length) return new TiffFacts();
                ulong entryCount = ReadTiff64(data, offset, littleEndian);
                ulong entriesEnd;
                try
                {
                    entriesEnd = checked((ulong)offset + 8UL + checked(entryCount * 20UL) + 8UL);
                }
                catch (OverflowException)
                {
                    return new TiffFacts();
                }
                if (entriesEnd > (ulong)data.Length) return new TiffFacts();

                bool widthKnown = false;
                bool heightKnown = false;
                int width = 0;
                int height = 0;
                for (ulong index = 0; index < entryCount; ++index)
                {
                    int entryOffset = (int)((ulong)offset + 8UL + index * 20UL);
                    ushort tag = ReadTiff16(data, entryOffset, littleEndian);
                    ushort type = ReadTiff16(data, entryOffset + 2, littleEndian);
                    ulong count = ReadTiff64(data, entryOffset + 4, littleEndian);
                    int typeSize;
                    if (!TryGetBigTiffTypeSize(type, out typeSize)) return new TiffFacts();
                    ulong payloadBytes;
                    try
                    {
                        payloadBytes = checked((ulong)typeSize * count);
                    }
                    catch (OverflowException)
                    {
                        return new TiffFacts();
                    }
                    if (payloadBytes > 8)
                    {
                        ulong valueOffset = ReadTiff64(data, entryOffset + 12, littleEndian);
                        if (valueOffset > (ulong)data.Length || payloadBytes > (ulong)data.Length - valueOffset)
                        {
                            return new TiffFacts();
                        }
                    }

                    ulong scalar;
                    if (tag == 256 && TryReadBigTiffScalar(data, entryOffset, type, count, littleEndian, out scalar) &&
                        scalar > 0 && scalar <= int.MaxValue)
                    {
                        width = (int)scalar;
                        widthKnown = true;
                    }
                    else if (tag == 257 && TryReadBigTiffScalar(data, entryOffset, type, count, littleEndian, out scalar) &&
                        scalar > 0 && scalar <= int.MaxValue)
                    {
                        height = (int)scalar;
                        heightKnown = true;
                    }
                }

                if (!widthKnown || !heightKnown)
                {
                    uniformPageSize = false;
                    cumulativePixelCountKnown = false;
                }
                else if (pageCount == 0)
                {
                    commonWidth = width;
                    commonHeight = height;
                }
                else if (width != commonWidth || height != commonHeight)
                {
                    uniformPageSize = false;
                }
                if (widthKnown && heightKnown && cumulativePixelCountKnown)
                {
                    try
                    {
                        cumulativePixelCount = checked(cumulativePixelCount + checked((long)width * height));
                    }
                    catch (OverflowException)
                    {
                        cumulativePixelCount = 0;
                        cumulativePixelCountKnown = false;
                    }
                }

                ++pageCount;
                ifdOffset = ReadTiff64(data, (int)(entriesEnd - 8UL), littleEndian);
            }

            if (pageCount > 0)
            {
                facts.PageCount = pageCount;
                facts.PageCountKnown = true;
                facts.CumulativePixelCount = cumulativePixelCount;
                facts.CumulativePixelCountKnown = cumulativePixelCountKnown;
                if (uniformPageSize)
                {
                    facts.Width = commonWidth;
                    facts.Height = commonHeight;
                    facts.SizeKnown = true;
                }
            }
            return facts;
        }

        private static bool TryReadTiffScalar(byte[] data, int entryOffset, ushort type, uint count, bool littleEndian, out long value)
        {
            value = 0;
            if (count != 1) return false;
            if (type == 3)
            {
                value = ReadTiff16(data, entryOffset + 8, littleEndian);
                return true;
            }
            if (type == 4)
            {
                uint unsignedValue = ReadTiff32(data, entryOffset + 8, littleEndian);
                value = (long)unsignedValue;
                return true;
            }
            return false;
        }

        private static bool TryGetTiffTypeSize(ushort type, out int size)
        {
            switch (type)
            {
                case 1: case 2: case 6: case 7: size = 1; return true;
                case 3: case 8: size = 2; return true;
                case 4: case 9: case 11: size = 4; return true;
                case 5: case 10: case 12: size = 8; return true;
                default: size = 0; return false;
            }
        }

        private static bool TryGetBigTiffTypeSize(ushort type, out int size)
        {
            switch (type)
            {
                case 1: case 2: case 6: case 7: size = 1; return true;
                case 3: case 8: size = 2; return true;
                case 4: case 9: case 11: size = 4; return true;
                case 5: case 10: case 12: case 16: case 17: case 18: size = 8; return true;
                default: size = 0; return false;
            }
        }

        private static bool TryReadBigTiffScalar(byte[] data, int entryOffset, ushort type, ulong count, bool littleEndian, out ulong value)
        {
            value = 0;
            if (count != 1) return false;
            if (type == 3)
            {
                value = ReadTiff16(data, entryOffset + 8, littleEndian);
                return true;
            }
            if (type == 4)
            {
                value = ReadTiff32(data, entryOffset + 8, littleEndian);
                return true;
            }
            if (type == 16 || type == 18)
            {
                value = ReadTiff64(data, entryOffset + 12, littleEndian);
                return true;
            }
            return false;
        }

        private static ushort ReadTiff16(byte[] data, int offset, bool littleEndian)
        {
            return littleEndian
                ? (ushort)(data[offset] | (data[offset + 1] << 8))
                : (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        private static uint ReadTiff32(byte[] data, int offset, bool littleEndian)
        {
            return littleEndian
                ? (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24))
                : ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) | ((uint)data[offset + 2] << 8) | data[offset + 3];
        }

        private static ulong ReadTiff64(byte[] data, int offset, bool littleEndian)
        {
            ulong value = 0;
            if (littleEndian)
            {
                for (int index = 7; index >= 0; --index) value = (value << 8) | data[offset + index];
            }
            else
            {
                for (int index = 0; index < 8; ++index) value = (value << 8) | data[offset + index];
            }
            return value;
        }
    }
}
