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
            public int BitDepth;
            public bool BitDepthKnown;
            public int Channels;
            public bool ChannelsKnown;
        }

        private static TiffFacts ReadTiffFacts(byte[] data)
        {
            return ReadTiffDirectoryFacts(data, false);
        }

        private static TiffFacts ReadBigTiffFacts(byte[] data)
        {
            return ReadTiffDirectoryFacts(data, true);
        }

        private static PixelFacts ToPixelFacts(TiffFacts facts)
        {
            PixelFacts pixels = new PixelFacts();
            pixels.BitDepth = facts.BitDepth;
            pixels.BitDepthKnown = facts.BitDepthKnown;
            pixels.Channels = facts.Channels;
            pixels.ChannelsKnown = facts.ChannelsKnown;
            return pixels;
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
