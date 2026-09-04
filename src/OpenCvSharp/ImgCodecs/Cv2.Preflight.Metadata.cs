using System;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private struct MetadataFacts
        {
            public long MetadataBytes;
            public bool MetadataSizeKnown;
            public long IccProfileBytes;
            public bool IccProfileSizeKnown;
        }

        private static MetadataFacts ReadMetadataFacts(byte[] data, string format)
        {
            if (string.Equals(format, "png", StringComparison.Ordinal)) return ReadPngMetadataFacts(data);
            if (string.Equals(format, "jpeg", StringComparison.Ordinal)) return ReadJpegMetadataFacts(data);
            if (string.Equals(format, "webp", StringComparison.Ordinal)) return ReadWebpMetadataFacts(data);
            return default(MetadataFacts);
        }

        private static MetadataFacts ReadPngMetadataFacts(byte[] data)
        {
            MetadataFacts facts = new MetadataFacts();
            if (data.Length < 8) return facts;
            int offset = 8;
            while (offset + 12 <= data.Length)
            {
                uint chunkLength = ReadBe32(data, offset);
                if (chunkLength > int.MaxValue || offset > data.Length - 12 || offset + 12L + chunkLength > data.Length)
                {
                    return default(MetadataFacts);
                }

                int payloadStart = offset + 8;
                int payloadLength = (int)chunkLength;
                bool metadata = IsPngMetadataChunk(data, offset + 4);
                bool isIend = IsPngChunkType(data, offset + 4, 'I', 'E', 'N', 'D');
                if (metadata) facts.MetadataBytes = checked(facts.MetadataBytes + payloadLength);
                if (IsPngChunkType(data, offset + 4, 'i', 'C', 'C', 'P'))
                {
                    int profileNameEnd = payloadStart;
                    while (profileNameEnd < payloadStart + payloadLength && data[profileNameEnd] != 0) ++profileNameEnd;
                    if (profileNameEnd == payloadStart || profileNameEnd + 2 > payloadStart + payloadLength) return default(MetadataFacts);
                    facts.IccProfileBytes = checked(facts.IccProfileBytes + (payloadStart + payloadLength - profileNameEnd - 2));
                }

                offset += 12 + payloadLength;
                if (isIend)
                {
                    if (payloadLength != 0 || offset != data.Length) return default(MetadataFacts);
                    facts.MetadataSizeKnown = true;
                    facts.IccProfileSizeKnown = true;
                    return facts;
                }
            }
            return default(MetadataFacts);
        }

        private static bool IsPngMetadataChunk(byte[] data, int typeOffset)
        {
            return IsPngChunkType(data, typeOffset, 't', 'E', 'X', 't') ||
                IsPngChunkType(data, typeOffset, 'z', 'T', 'X', 't') ||
                IsPngChunkType(data, typeOffset, 'i', 'T', 'X', 't') ||
                IsPngChunkType(data, typeOffset, 'e', 'X', 'I', 'f') ||
                IsPngChunkType(data, typeOffset, 'i', 'C', 'C', 'P');
        }

        private static bool IsPngChunkType(byte[] data, int offset, char first, char second, char third, char fourth)
        {
            return data[offset] == (byte)first && data[offset + 1] == (byte)second &&
                data[offset + 2] == (byte)third && data[offset + 3] == (byte)fourth;
        }

        private static MetadataFacts ReadJpegMetadataFacts(byte[] data)
        {
            MetadataFacts facts = new MetadataFacts();
            if (data.Length < 2) return facts;
            int offset = 2;
            int iccSegmentCount = 0;
            int nextIccSegment = 1;
            bool inEntropyData = false;
            while (offset < data.Length)
            {
                int marker;
                if (inEntropyData)
                {
                    while (offset < data.Length && data[offset] != 0xFF) ++offset;
                    if (offset >= data.Length) return default(MetadataFacts);
                    while (offset < data.Length && data[offset] == 0xFF) ++offset;
                    if (offset >= data.Length) return default(MetadataFacts);
                    marker = data[offset++];
                    if (marker == 0x00 || (marker >= 0xD0 && marker <= 0xD7)) continue;
                    inEntropyData = false;
                }
                else
                {
                    if (data[offset] != 0xFF) return default(MetadataFacts);
                    while (offset < data.Length && data[offset] == 0xFF) ++offset;
                    if (offset >= data.Length) return default(MetadataFacts);
                    marker = data[offset++];
                }

                if (marker == 0xD9)
                {
                    if (iccSegmentCount != 0 && nextIccSegment != iccSegmentCount + 1) return default(MetadataFacts);
                    facts.MetadataSizeKnown = true;
                    facts.IccProfileSizeKnown = true;
                    return facts;
                }
                if ((marker >= 0xD0 && marker <= 0xD7) || marker == 0x01) continue;
                if (offset + 1 >= data.Length) return default(MetadataFacts);
                int length = (data[offset] << 8) | data[offset + 1];
                if (length < 2 || offset + length > data.Length) return default(MetadataFacts);
                int payloadStart = offset + 2;
                int payloadLength = length - 2;
                if ((marker >= 0xE0 && marker <= 0xEF) || marker == 0xFE)
                {
                    facts.MetadataBytes = checked(facts.MetadataBytes + payloadLength);
                    if (marker == 0xE2 && IsJpegIccProfileSegment(data, payloadStart, payloadLength))
                    {
                        int sequence = data[payloadStart + 12];
                        int count = data[payloadStart + 13];
                        if (count == 0 || sequence != nextIccSegment || sequence > count ||
                            (iccSegmentCount != 0 && count != iccSegmentCount)) return default(MetadataFacts);
                        iccSegmentCount = count;
                        ++nextIccSegment;
                        facts.IccProfileBytes = checked(facts.IccProfileBytes + payloadLength - 14);
                    }
                }
                offset += length;
                if (marker == 0xDA) inEntropyData = true;
            }
            return default(MetadataFacts);
        }

        private static bool IsJpegIccProfileSegment(byte[] data, int offset, int length)
        {
            return length >= 14 && data[offset] == (byte)'I' && data[offset + 1] == (byte)'C' &&
                data[offset + 2] == (byte)'C' && data[offset + 3] == (byte)'_' &&
                data[offset + 4] == (byte)'P' && data[offset + 5] == (byte)'R' &&
                data[offset + 6] == (byte)'O' && data[offset + 7] == (byte)'F' &&
                data[offset + 8] == (byte)'I' && data[offset + 9] == (byte)'L' &&
                data[offset + 10] == (byte)'E' && data[offset + 11] == 0;
        }

        private static MetadataFacts ReadWebpMetadataFacts(byte[] data)
        {
            MetadataFacts facts = new MetadataFacts();
            if (data.Length < 12) return facts;
            uint declaredLength = (uint)(data[4] | (data[5] << 8) | (data[6] << 16) | (data[7] << 24));
            if (declaredLength != data.Length - 8) return facts;
            int offset = 12;
            while (offset + 8 <= data.Length)
            {
                int chunkLength = ReadSignedLe32(data, offset + 4);
                if (chunkLength < 0 || offset + 8L + chunkLength > data.Length) return default(MetadataFacts);
                bool metadata = IsWebpMetadataChunk(data, offset);
                if (metadata) facts.MetadataBytes = checked(facts.MetadataBytes + chunkLength);
                if (data[offset] == (byte)'I' && data[offset + 1] == (byte)'C' && data[offset + 2] == (byte)'C' && data[offset + 3] == (byte)'P')
                {
                    facts.IccProfileBytes = checked(facts.IccProfileBytes + chunkLength);
                    facts.IccProfileSizeKnown = true;
                }
                offset += 8 + chunkLength + (chunkLength & 1);
            }
            if (offset != data.Length) return default(MetadataFacts);
            facts.MetadataSizeKnown = true;
            facts.IccProfileSizeKnown = true;
            return facts;
        }

        private static bool IsWebpMetadataChunk(byte[] data, int offset)
        {
            return (data[offset] == (byte)'E' && data[offset + 1] == (byte)'X' && data[offset + 2] == (byte)'I' && data[offset + 3] == (byte)'F') ||
                (data[offset] == (byte)'X' && data[offset + 1] == (byte)'M' && data[offset + 2] == (byte)'P' && data[offset + 3] == (byte)' ') ||
                (data[offset] == (byte)'I' && data[offset + 1] == (byte)'C' && data[offset + 2] == (byte)'C' && data[offset + 3] == (byte)'P');
        }
    }
}
