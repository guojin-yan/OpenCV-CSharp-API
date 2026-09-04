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
            FrameFacts staticImage = new FrameFacts { FrameCount = 1, FrameCountKnown = true };
            if (data.Length < 8) return default(FrameFacts);

            bool sawAnimationControl = false;
            bool sawIhdr = false;
            bool sawIdat = false;
            bool sawFrameControl = false;
            bool currentFrameHasData = false;
            int frameControls = 0;
            uint declaredFrameCount = 0;
            uint nextSequence = 0;
            int offset = 8;

            while (offset + 12 <= data.Length)
            {
                uint chunkLength = ReadBe32(data, offset);
                if (chunkLength > int.MaxValue || offset + 12L + chunkLength > data.Length)
                {
                    return default(FrameFacts);
                }

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
                    if (sawIhdr || offset != 8 || payloadLength != 13)
                    {
                    return default(FrameFacts);
                    }
                    sawIhdr = true;
                }
                else if (!sawIhdr)
                {
                    return default(FrameFacts);
                }

                if (isActl)
                {
                    if (sawAnimationControl || sawIdat || payloadLength != 8) return default(FrameFacts);
                    declaredFrameCount = ReadBe32(data, payloadStart);
                    if (declaredFrameCount == 0 || declaredFrameCount > int.MaxValue) return default(FrameFacts);
                    sawAnimationControl = true;
                }
                else if (isFctl)
                {
                    if (!sawAnimationControl || payloadLength != 26 || (sawFrameControl && !currentFrameHasData))
                    {
                        return default(FrameFacts);
                    }
                    if (ReadBe32(data, payloadStart) != nextSequence || nextSequence == uint.MaxValue) return default(FrameFacts);
                    ++nextSequence;
                    ++frameControls;
                    sawFrameControl = true;
                    currentFrameHasData = false;
                }
                else if (isFdat)
                {
                    if (!sawAnimationControl || !sawFrameControl || payloadLength <= 4) return default(FrameFacts);
                    if (ReadBe32(data, payloadStart) != nextSequence || nextSequence == uint.MaxValue) return default(FrameFacts);
                    ++nextSequence;
                    currentFrameHasData = true;
                }
                else if (isIdat)
                {
                    if (payloadLength > 0)
                    {
                        sawIdat = true;
                        if (sawAnimationControl && sawFrameControl) currentFrameHasData = true;
                    }
                }

                offset += 12 + payloadLength;
                if (isIend)
                {
                    if (payloadLength != 0 || offset != data.Length)
                    {
                        return default(FrameFacts);
                    }
                    if (!sawAnimationControl) return sawIdat ? staticImage : default(FrameFacts);
                    if (!sawFrameControl || !currentFrameHasData || frameControls != (int)declaredFrameCount)
                    {
                        return default(FrameFacts);
                    }
                    return new FrameFacts { FrameCount = frameControls, FrameCountKnown = true };
                }
            }

            return default(FrameFacts);
        }

        private static FrameFacts ReadWebpFrameFacts(byte[] data)
        {
            FrameFacts staticImage = new FrameFacts { FrameCount = 1, FrameCountKnown = true };
            if (data.Length < 12) return default(FrameFacts);

            uint declaredLength = (uint)(data[4] | (data[5] << 8) | (data[6] << 16) | (data[7] << 24));
            if (declaredLength != data.Length - 8) return default(FrameFacts);

            bool sawVp8x = false;
            bool sawImageChunk = false;
            bool animationEnabled = false;
            bool sawAnimationParameters = false;
            int frameCount = 0;
            int offset = 12;
            while (offset + 8 <= data.Length)
            {
                int chunkLength = ReadSignedLe32(data, offset + 4);
                if (chunkLength < 0 || offset + 8L + chunkLength > data.Length)
                {
                    return default(FrameFacts);
                }

                bool isVp8x = IsWebpChunkType(data, offset, 'V', 'P', '8', 'X');
                bool isVp8 = IsWebpChunkType(data, offset, 'V', 'P', '8', ' ');
                bool isVp8l = IsWebpChunkType(data, offset, 'V', 'P', '8', 'L');
                bool isAnim = IsWebpChunkType(data, offset, 'A', 'N', 'I', 'M');
                bool isAnmf = IsWebpChunkType(data, offset, 'A', 'N', 'M', 'F');
                if (isVp8x)
                {
                    if (sawVp8x || chunkLength != 10) return default(FrameFacts);
                    sawVp8x = true;
                    animationEnabled = (data[offset + 8] & 0x02) != 0;
                }
                else if (isVp8 || isVp8l)
                {
                    if (chunkLength <= 0) return default(FrameFacts);
                    sawImageChunk = true;
                }
                else if (isAnim)
                {
                    if (!sawVp8x || !animationEnabled || sawAnimationParameters || chunkLength != 6) return default(FrameFacts);
                    sawAnimationParameters = true;
                }
                else if (isAnmf)
                {
                    if (!sawVp8x || !animationEnabled || !sawAnimationParameters || chunkLength < 16) return default(FrameFacts);
                    if (frameCount == int.MaxValue) return default(FrameFacts);
                    ++frameCount;
                }

                offset += 8 + chunkLength + (chunkLength & 1);
            }

            if (offset != data.Length) return default(FrameFacts);
            if (!animationEnabled) return sawImageChunk ? staticImage : default(FrameFacts);
            return sawAnimationParameters && frameCount > 0
                ? new FrameFacts { FrameCount = frameCount, FrameCountKnown = true }
                : default(FrameFacts);
        }

        private static bool IsWebpChunkType(byte[] data, int offset, char first, char second, char third, char fourth)
        {
            return data[offset] == (byte)first && data[offset + 1] == (byte)second &&
                data[offset + 2] == (byte)third && data[offset + 3] == (byte)fourth;
        }
    }
}
