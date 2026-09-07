namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private static bool IsJpeg2000Codestream(byte[] data)
        {
            return data.Length >= 4 && data[0] == 0xFF && data[1] == 0x4F && data[2] == 0xFF && data[3] == 0x51;
        }

        private static bool IsJp2Container(byte[] data)
        {
            return data.Length >= 12 &&
                ReadBe32(data, 0) == 12 &&
                data[4] == (byte)'j' && data[5] == (byte)'P' && data[6] == (byte)' ' && data[7] == (byte)' ' &&
                data[8] == 0x0D && data[9] == 0x0A && data[10] == 0x87 && data[11] == 0x0A;
        }

        private static bool TryReadJpeg2000Facts(byte[] data, bool isJp2, out int width, out int height,
            out PixelFacts pixelFacts)
        {
            width = 0;
            height = 0;
            pixelFacts = new PixelFacts();

            int codestreamStart = 0;
            int codestreamEnd = data.Length;
            if (isJp2 && !TryFindJp2Codestream(data, out codestreamStart, out codestreamEnd)) return false;
            return TryReadJpeg2000Siz(data, codestreamStart, codestreamEnd, out width, out height, out pixelFacts);
        }

        private static bool TryFindJp2Codestream(byte[] data, out int codestreamStart, out int codestreamEnd)
        {
            codestreamStart = 0;
            codestreamEnd = 0;
            if (!IsJp2Container(data)) return false;

            int offset = 12;
            while (offset < data.Length)
            {
                if (data.Length - offset < 8) return false;
                uint shortLength = ReadBe32(data, offset);
                int headerLength = 8;
                ulong boxLength = shortLength;
                if (shortLength == 1)
                {
                    if (data.Length - offset < 16) return false;
                    boxLength = ReadJpeg2000Be64(data, offset + 8);
                    headerLength = 16;
                }
                else if (shortLength == 0)
                {
                    boxLength = (ulong)(data.Length - offset);
                }

                if (boxLength < (ulong)headerLength || boxLength > (ulong)(data.Length - offset)) return false;
                int boxEnd = offset + (int)boxLength;
                bool isCodestream = data[offset + 4] == (byte)'j' && data[offset + 5] == (byte)'p' &&
                    data[offset + 6] == (byte)'2' && data[offset + 7] == (byte)'c';
                if (isCodestream)
                {
                    codestreamStart = offset + headerLength;
                    codestreamEnd = boxEnd;
                    return true;
                }

                offset = boxEnd;
                if (shortLength == 0) return false;
            }
            return false;
        }

        private static bool TryReadJpeg2000Siz(byte[] data, int offset, int end, out int width, out int height,
            out PixelFacts pixelFacts)
        {
            width = 0;
            height = 0;
            pixelFacts = new PixelFacts();
            if (offset < 0 || end < offset || end > data.Length || end - offset < 45) return false;
            if (data[offset] != 0xFF || data[offset + 1] != 0x4F || data[offset + 2] != 0xFF || data[offset + 3] != 0x51)
                return false;

            int sizLength = ReadJpeg2000Be16(data, offset + 4);
            int componentCount = ReadJpeg2000Be16(data, offset + 40);
            if (componentCount <= 0 || componentCount > 16384 || sizLength != 38 + 3 * componentCount) return false;
            if (sizLength > end - offset - 4) return false;

            uint xSize = ReadBe32(data, offset + 8);
            uint ySize = ReadBe32(data, offset + 12);
            uint xOrigin = ReadBe32(data, offset + 16);
            uint yOrigin = ReadBe32(data, offset + 20);
            uint tileWidth = ReadBe32(data, offset + 24);
            uint tileHeight = ReadBe32(data, offset + 28);
            uint tileXOrigin = ReadBe32(data, offset + 32);
            uint tileYOrigin = ReadBe32(data, offset + 36);
            if (xSize <= xOrigin || ySize <= yOrigin || tileWidth == 0 || tileHeight == 0) return false;
            if (tileXOrigin > xOrigin || tileYOrigin > yOrigin) return false;
            if ((ulong)tileXOrigin + tileWidth <= xOrigin || (ulong)tileYOrigin + tileHeight <= yOrigin) return false;

            uint imageWidth = xSize - xOrigin;
            uint imageHeight = ySize - yOrigin;
            if (imageWidth > int.MaxValue || imageHeight > int.MaxValue) return false;

            int bitDepth = 0;
            bool uniformBitDepth = true;
            int componentOffset = offset + 42;
            for (int component = 0; component < componentCount; ++component)
            {
                int currentBitDepth = (data[componentOffset] & 0x7F) + 1;
                if (data[componentOffset + 1] == 0 || data[componentOffset + 2] == 0) return false;
                if (component == 0)
                    bitDepth = currentBitDepth;
                else if (bitDepth != currentBitDepth)
                    uniformBitDepth = false;
                componentOffset += 3;
            }

            width = (int)imageWidth;
            height = (int)imageHeight;
            pixelFacts.Channels = componentCount;
            pixelFacts.ChannelsKnown = true;
            if (uniformBitDepth)
            {
                pixelFacts.BitDepth = bitDepth;
                pixelFacts.BitDepthKnown = true;
            }
            return true;
        }

        private static int ReadJpeg2000Be16(byte[] data, int offset)
        {
            return (data[offset] << 8) | data[offset + 1];
        }

        private static ulong ReadJpeg2000Be64(byte[] data, int offset)
        {
            return ((ulong)ReadBe32(data, offset) << 32) | ReadBe32(data, offset + 4);
        }
    }
}
