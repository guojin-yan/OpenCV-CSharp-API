using System;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private struct BmpFacts
        {
            public int Width;
            public int Height;
            public bool SizeKnown;
            public bool FrameKnown;
            public PixelFacts Pixels;
        }

        private static BmpFacts ReadBmpFacts(byte[] data)
        {
            BmpFacts facts = default(BmpFacts);
            if (data.Length < 18 || data[0] != (byte)'B' || data[1] != (byte)'M') return facts;
            uint dibSize = ReadUnsignedLe32(data, 14);
            // These layouts have established field offsets. An unknown DIB size cannot
            // borrow INFO geometry merely because enough bytes follow the signature.
            if (dibSize != 12 && dibSize != 40 && dibSize != 52 && dibSize != 56 &&
                dibSize != 64 && dibSize != 108 && dibSize != 124) return facts;
            long dibEnd = 14L + dibSize;
            if (dibEnd > data.Length) return facts;

            bool core = dibSize == 12;
            int width, height, bits;
            uint compression = 0, imageBytes = 0, usedColors = 0;
            if (core)
            {
                width = ReadLe16(data, 18); height = ReadLe16(data, 20);
                if (ReadLe16(data, 22) != 1) return facts;
                bits = ReadLe16(data, 24);
            }
            else
            {
                if (ReadLe16(data, 26) != 1) return facts;
                bits = ReadLe16(data, 28);
                if (dibSize == 64)
                {
                    // OS/2 2.x uses unsigned dimensions, rather than Windows top-down
                    // signed height. Public dimensions must still fit positive Int32.
                    uint x = ReadUnsignedLe32(data, 18), y = ReadUnsignedLe32(data, 22);
                    if (x > int.MaxValue || y > int.MaxValue) return facts;
                    width = (int)x; height = (int)y;
                }
                else
                {
                    width = ReadSignedLe32(data, 18);
                    int signedHeight = ReadSignedLe32(data, 22);
                    if (signedHeight == int.MinValue) return facts;
                    height = Math.Abs(signedHeight);
                }
                compression = ReadUnsignedLe32(data, 30);
                imageBytes = ReadUnsignedLe32(data, 34);
                usedColors = ReadUnsignedLe32(data, 46);
            }
            if (width <= 0 || height <= 0 ||
                (bits != 1 && bits != 4 && bits != 8 && bits != 24 && bits != 32 && (core || bits != 16))) return facts;
            facts.Width = width; facts.Height = height; facts.SizeKnown = true;

            // Geometry is a header fact. Pixel/storage completeness also requires the
            // supported BI_RGB layout, complete palette and exact declared file ranges.
            if (compression != 0 || ReadLe16(data, 6) != 0 || ReadLe16(data, 8) != 0 ||
                ReadUnsignedLe32(data, 2) != data.Length) return facts;
            uint colors = bits <= 8 && usedColors == 0 ? (uint)(1 << bits) : usedColors;
            if (bits <= 8 && colors > (uint)(1 << bits)) return facts;
            long paletteEnd = dibEnd + (long)colors * (core ? 3 : 4);
            uint pixelOffset = ReadUnsignedLe32(data, 10);
            if (paletteEnd > pixelOffset || pixelOffset >= data.Length) return facts;
            try
            {
                long rowBytes = (((long)width * bits + 31) / 32) * 4;
                long payload = checked(rowBytes * height);
                long pixelEnd = checked((long)pixelOffset + payload);
                if ((imageBytes != 0 && imageBytes != payload) || pixelEnd > data.Length) return facts;
                long fileEnd = pixelEnd;
                if (dibSize == 124)
                {
                    if (ReadUnsignedLe32(data, 134) != 0) return facts;
                    uint colorSpace = ReadUnsignedLe32(data, 70);
                    if (colorSpace == 0x4D424544 || colorSpace == 0x4C494E4B)
                    {
                        // A BMP file stores an embedded profile or linked-profile name
                        // after the pixels. Its offset is relative to the DIB, not the file.
                        // Only its byte range is inspected; linked names are never opened.
                        long profileStart = 14L + ReadUnsignedLe32(data, 126);
                        uint profileBytes = ReadUnsignedLe32(data, 130);
                        if (profileBytes == 0 || profileStart < pixelEnd) return facts;
                        fileEnd = profileStart + profileBytes;
                    }
                    // Profile pointer/size fields are inactive for other color spaces.
                }
                if (fileEnd != data.Length) return facts;
                facts.FrameKnown = true;
                if (bits != 16)
                {
                    facts.Pixels.BitDepth = bits <= 8 ? bits : 8;
                    facts.Pixels.BitDepthKnown = true;
                    facts.Pixels.Channels = bits <= 8 ? 1 : 3;
                    facts.Pixels.ChannelsKnown = true;
                }
            }
            catch (OverflowException) { }
            return facts;
        }
    }
}
