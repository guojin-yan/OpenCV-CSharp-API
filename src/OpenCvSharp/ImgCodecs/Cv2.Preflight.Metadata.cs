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

        private static bool IsGifIccProfileExtension(byte[] data, int offset)
        {
            // The application identifier and authentication code occupy a validated 11-byte block.
            return data[offset] == (byte)'I' && data[offset + 1] == (byte)'C' && data[offset + 2] == (byte)'C' &&
                data[offset + 3] == (byte)'R' && data[offset + 4] == (byte)'G' && data[offset + 5] == (byte)'B' &&
                data[offset + 6] == (byte)'G' && data[offset + 7] == (byte)'1' && data[offset + 8] == (byte)'0' &&
                data[offset + 9] == (byte)'1' && data[offset + 10] == (byte)'2';
        }
    }
}
