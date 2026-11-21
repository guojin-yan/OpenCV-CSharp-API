namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private struct Jp2Header
        {
            public int Width;
            public int Height;
            public int Components;
            public byte Descriptor;
            public int ComponentDescriptors;
        }

        private static bool TryFindJp2Codestream(byte[] data, out int codestreamStart, out int codestreamEnd,
            out Jp2Header header)
        {
            codestreamStart = codestreamEnd = 0;
            header = default(Jp2Header);
            if (!IsJp2Container(data)) return false;
            bool sawFileType = false, sawHeader = false, sawCodestream = false;
            int offset = 12;
            while (offset < data.Length)
            {
                int payload, next;
                if (!TryReadJp2Box(data, offset, data.Length, out payload, out next)) return false;
                uint type = ReadBe32(data, offset + 4);
                if (!sawFileType)
                {
                    // The file-type box immediately follows the signature. Compatible future
                    // brands are accepted when either the major or a compatibility brand is JP2.
                    if (type != 0x66747970 || !IsJp2FileType(data, payload, next)) return false; // ftyp
                    sawFileType = true;
                }
                else if (type == 0x66747970 || type == 0x6A502020) return false; // ftyp / jP
                else if (type == 0x6A703268) // jp2h
                {
                    if (sawHeader || sawCodestream || !TryReadJp2Header(data, payload, next, out header)) return false;
                    sawHeader = true;
                }
                else if (type == 0x6A703263) // jp2c
                {
                    if (!sawHeader || sawCodestream) return false;
                    codestreamStart = payload; codestreamEnd = next;
                    sawCodestream = true;
                }
                // Unknown boxes remain opaque. Walk through EOF before trusting the chosen
                // stream; a duplicate stream or a broken suffix must not be ignored.
                offset = next;
            }
            return sawFileType && sawHeader && sawCodestream;
        }

        private static bool TryReadJp2Box(byte[] data, int offset, int end, out int payload, out int next)
        {
            payload = next = 0;
            if (offset < 0 || end > data.Length || end - offset < 8) return false;
            uint shortLength = ReadBe32(data, offset);
            int headerLength = 8;
            ulong length = shortLength;
            if (shortLength == 1)
            {
                if (end - offset < 16) return false;
                headerLength = 16;
                length = ReadJpeg2000Be64(data, offset + 8);
            }
            else if (shortLength == 0)
            {
                // LBox=0 extends to file EOF, including when encountered inside a superbox.
                length = (ulong)(data.Length - offset);
            }
            if (length < (ulong)headerLength || length > (ulong)(end - offset)) return false;
            payload = offset + headerLength;
            next = offset + (int)length;
            return true;
        }

        private static bool IsJp2FileType(byte[] data, int offset, int end)
        {
            if (end - offset < 8 || ((end - offset) & 3) != 0) return false;
            if (ReadBe32(data, offset) == 0x6A703220) return true; // jp2
            for (int brand = offset + 8; brand < end; brand += 4)
                if (ReadBe32(data, brand) == 0x6A703220) return true;
            return false;
        }

        private static bool TryReadJp2Header(byte[] data, int offset, int end, out Jp2Header header)
        {
            header = default(Jp2Header);
            bool sawImageHeader = false, sawComponentDepth = false;
            int descriptorCount = 0;
            while (offset < end)
            {
                int payload, next;
                if (!TryReadJp2Box(data, offset, end, out payload, out next)) return false;
                uint type = ReadBe32(data, offset + 4);
                if (!sawImageHeader)
                {
                    if (type != 0x69686472 || next - payload != 14) return false; // ihdr must be first
                    uint height = ReadBe32(data, payload), width = ReadBe32(data, payload + 4);
                    int components = ReadJpeg2000Be16(data, payload + 8);
                    byte descriptor = data[payload + 10];
                    if (width == 0 || height == 0 || width > int.MaxValue || height > int.MaxValue ||
                        components == 0 || components > 16384 || data[payload + 11] != 7 ||
                        (descriptor != 255 && (descriptor & 127) + 1 > 38)) return false;
                    header.Width = (int)width; header.Height = (int)height;
                    header.Components = components; header.Descriptor = descriptor;
                    sawImageHeader = true;
                }
                else if (type == 0x69686472) return false; // duplicate ihdr
                else if (type == 0x62706363) // bpcc
                {
                    if (sawComponentDepth) return false;
                    header.ComponentDescriptors = payload;
                    descriptorCount = next - payload;
                    sawComponentDepth = true;
                }
                offset = next;
            }
            // Redundant BPCC is tolerated only when it agrees too. The sentinel BPC=255
            // requires exactly one stored descriptor for every explicitly declared component.
            return sawImageHeader && (header.Descriptor != 255 || sawComponentDepth) &&
                (!sawComponentDepth || descriptorCount == header.Components);
        }

        private static bool Jp2HeaderMatchesSiz(byte[] data, Jp2Header header, int start,
            int width, int height, int components)
        {
            if (header.Width != width || header.Height != height || header.Components != components) return false;
            for (int component = 0; component < components; ++component)
            {
                byte descriptor = data[start + 42 + component * 3];
                // Include the sign bit when reconciling two descriptors, even though the public
                // facts expose only encoded depth/count, not signedness or final Mat precision.
                if ((header.Descriptor != 255 && header.Descriptor != descriptor) ||
                    (header.ComponentDescriptors != 0 && data[header.ComponentDescriptors + component] != descriptor))
                    return false;
            }
            return true;
        }
    }
}
