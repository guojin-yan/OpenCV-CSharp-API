using System;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private static bool TryReadSunRasterHeader(byte[] data, out int width, out int height, out bool frameKnown, out PixelFacts pixelFacts)
        {
            width = 0;
            height = 0;
            frameKnown = false;
            pixelFacts = new PixelFacts();
            if (data.Length < 32 || ReadBe32(data, 0) != 0x59A66A95) return false;

            uint unsignedWidth = ReadBe32(data, 4);
            uint unsignedHeight = ReadBe32(data, 8);
            uint depth = ReadBe32(data, 12);
            uint payloadLength = ReadBe32(data, 16);
            uint type = ReadBe32(data, 20);
            uint mapType = ReadBe32(data, 24);
            uint mapLength = ReadBe32(data, 28);
            if (unsignedWidth == 0 || unsignedHeight == 0 || unsignedWidth > int.MaxValue || unsignedHeight > int.MaxValue)
            {
                return false;
            }

            width = (int)unsignedWidth;
            height = (int)unsignedHeight;
            bool mapKnown = mapType <= 2 && (mapType != 0 || mapLength == 0) && (mapType != 1 || mapLength % 3 == 0);

            switch (depth)
            {
                case 1:
                case 8:
                    pixelFacts.BitDepth = (int)depth;
                    pixelFacts.BitDepthKnown = true;
                    pixelFacts.Channels = 1;
                    pixelFacts.ChannelsKnown = true;
                    break;
                case 24:
                    pixelFacts.BitDepth = 8;
                    pixelFacts.BitDepthKnown = true;
                    pixelFacts.Channels = 3;
                    pixelFacts.ChannelsKnown = true;
                    break;
                case 32:
                    // Four encoded bytes include padding; this is not an alpha claim.
                    pixelFacts.BitDepth = 8;
                    pixelFacts.BitDepthKnown = true;
                    pixelFacts.Channels = 4;
                    pixelFacts.ChannelsKnown = true;
                    break;
            }

            if (!pixelFacts.BitDepthKnown || !mapKnown || type > 3) return true;
            try
            {
                long payloadOffset = checked(32L + mapLength);
                if (payloadOffset > data.Length) return true;

                // RLE encodes the same padded byte stream as raw storage at every
                // supported depth. Runs may cross pixels, padding and scanlines.
                long rowBits = checked((long)width * depth);
                long rowBytes = checked(((rowBits + 15) / 16) * 2);
                long expectedDecodedBytes = checked(rowBytes * height);

                long declaredPayload = payloadLength;
                if (type == 0 && declaredPayload == 0) declaredPayload = expectedDecodedBytes;
                if (declaredPayload <= 0) return true;
                long payloadEnd = checked(payloadOffset + declaredPayload);
                if (payloadEnd != data.Length) return true;
                if (type == 2)
                {
                    // Only encoded offsets are Int32-bounded by the actual input.
                    // Count expansion in Int64 without allocating decoded storage.
                    frameKnown = IsSunRasterRlePayloadComplete(data, (int)payloadOffset, data.Length, expectedDecodedBytes);
                }
                else
                {
                    frameKnown = declaredPayload == expectedDecodedBytes;
                }
            }
            catch (OverflowException)
            {
                frameKnown = false;
            }
            return true;
        }

        private static bool IsSunRasterRlePayloadComplete(byte[] data, int offset, int end, long expectedDecodedBytes)
        {
            if (offset < 0 || offset > end || end > data.Length || expectedDecodedBytes <= 0) return false;
            long decodedBytes = 0;
            while (decodedBytes < expectedDecodedBytes)
            {
                if (offset >= end) return false;
                byte value = data[offset++];
                if (value != 0x80)
                {
                    ++decodedBytes;
                    continue;
                }

                if (offset >= end) return false;
                int runLength = data[offset++];
                if (runLength == 0)
                {
                    ++decodedBytes;
                    continue;
                }
                if (offset >= end) return false;
                ++offset;
                long runBytes = runLength + 1L;
                if (runBytes > expectedDecodedBytes - decodedBytes) return false;
                decodedBytes += runBytes;
            }
            return decodedBytes == expectedDecodedBytes && offset == end;
        }
    }
}
