using System;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private struct FrameFacts
        {
            public int FrameCount;
            public bool FrameCountKnown;
        }

        private static FrameFacts ReadPngFrameFacts(byte[] data)
        {
            bool sawAnimationControl = false;
            bool sawIhdr = false;
            bool sawIdat = false;
            bool idatEnded = false;
            bool defaultImageHasData = false;
            bool sawFrameControl = false;
            bool currentFrameUsesIdat = false;
            bool currentFrameHasData = false;
            int frameControls = 0;
            uint canvasWidth = 0;
            uint canvasHeight = 0;
            uint declaredFrameCount = 0;
            uint nextSequence = 0;
            int offset = 8;

            // Fixed state only: chunk lengths and animation declarations never allocate frame storage.
            // Every sequence-bearing chunk consumes at least 16 input bytes, bounding nextSequence
            // below int.MaxValue / 16 even when all declared counts are hostile.
            while (data.Length - offset >= 12)
            {
                uint chunkLength = ReadBe32(data, offset);
                if (chunkLength > (uint)(data.Length - offset - 12)) return default(FrameFacts);

                int payloadStart = offset + 8;
                int payloadLength = (int)chunkLength;
                bool isIhdr = IsPngChunkType(data, offset + 4, 'I', 'H', 'D', 'R');
                bool isActl = IsPngChunkType(data, offset + 4, 'a', 'c', 'T', 'L');
                bool isFctl = IsPngChunkType(data, offset + 4, 'f', 'c', 'T', 'L');
                bool isFdat = IsPngChunkType(data, offset + 4, 'f', 'd', 'A', 'T');
                bool isIdat = IsPngChunkType(data, offset + 4, 'I', 'D', 'A', 'T');
                bool isIend = IsPngChunkType(data, offset + 4, 'I', 'E', 'N', 'D');

                if (isIhdr)
                {
                    if (sawIhdr || offset != 8 || payloadLength != 13) return default(FrameFacts);
                    canvasWidth = ReadBe32(data, payloadStart);
                    canvasHeight = ReadBe32(data, payloadStart + 4);
                    if (!ReadPngHeader(data).SizeKnown) return default(FrameFacts);
                    sawIhdr = true;
                }
                else if (!sawIhdr) return default(FrameFacts);

                if (sawIdat && !isIdat) idatEnded = true;
                if (isActl)
                {
                    if (sawAnimationControl || sawIdat || payloadLength != 8) return default(FrameFacts);
                    declaredFrameCount = ReadBe32(data, payloadStart);
                    if (declaredFrameCount == 0 || declaredFrameCount > int.MaxValue ||
                        ReadBe32(data, payloadStart + 4) > int.MaxValue) return default(FrameFacts);
                    sawAnimationControl = true;
                }
                else if (isFctl)
                {
                    if (!sawAnimationControl || payloadLength != 26 || frameControls >= declaredFrameCount ||
                        (sawFrameControl && !currentFrameHasData)) return default(FrameFacts);
                    if (ReadBe32(data, payloadStart) != nextSequence) return default(FrameFacts);
                    ++nextSequence;
                    uint width = ReadBe32(data, payloadStart + 4);
                    uint height = ReadBe32(data, payloadStart + 8);
                    uint x = ReadBe32(data, payloadStart + 12);
                    uint y = ReadBe32(data, payloadStart + 16);
                    if (width == 0 || height == 0 || (ulong)x + width > canvasWidth || (ulong)y + height > canvasHeight ||
                        data[payloadStart + 24] > 2 || data[payloadStart + 25] > 1) return default(FrameFacts);

                    // A control preceding IDAT makes the default image the first animation frame.
                    // A control following IDAT starts a separate fdAT frame and may use a subrectangle.
                    currentFrameUsesIdat = !sawIdat;
                    if (currentFrameUsesIdat)
                    {
                        if (x != 0 || y != 0 || width != canvasWidth || height != canvasHeight) return default(FrameFacts);
                    }
                    else if (!defaultImageHasData) return default(FrameFacts);
                    ++frameControls;
                    sawFrameControl = true;
                    currentFrameHasData = false;
                }
                else if (isFdat)
                {
                    if (!sawAnimationControl || !sawFrameControl || currentFrameUsesIdat ||
                        !defaultImageHasData || payloadLength < 4) return default(FrameFacts);
                    if (ReadBe32(data, payloadStart) != nextSequence) return default(FrameFacts);
                    ++nextSequence;
                    if (payloadLength > 4) currentFrameHasData = true;
                }
                else if (isIdat)
                {
                    if (idatEnded || (sawFrameControl && !currentFrameUsesIdat)) return default(FrameFacts);
                    sawIdat = true;
                    if (payloadLength > 0)
                    {
                        defaultImageHasData = true;
                        if (sawFrameControl) currentFrameHasData = true;
                    }
                }

                offset += 12 + payloadLength;
                if (isIend)
                {
                    if (payloadLength != 0 || offset != data.Length || !defaultImageHasData) return default(FrameFacts);
                    if (!sawAnimationControl) return new FrameFacts { FrameCount = 1, FrameCountKnown = true };
                    if (!sawFrameControl || !currentFrameHasData || frameControls != declaredFrameCount) return default(FrameFacts);
                    return new FrameFacts { FrameCount = frameControls, FrameCountKnown = true };
                }
            }
            return default(FrameFacts);
        }

    }
}
