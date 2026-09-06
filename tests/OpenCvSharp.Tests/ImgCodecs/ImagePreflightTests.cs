using System;
using System.IO;
using System.Text;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class ImagePreflightTests
    {
        [Fact]
        public void IdentifyReadsPngDimensionsWithoutNativeRuntime()
        {
            byte[] png = new byte[24];
            png[0] = 0x89; png[1] = 0x50; png[2] = 0x4E; png[3] = 0x47;
            png[4] = 0x0D; png[5] = 0x0A; png[6] = 0x1A; png[7] = 0x0A;
            png[12] = (byte)'I'; png[13] = (byte)'H'; png[14] = (byte)'D'; png[15] = (byte)'R';
            png[16] = 0x00; png[17] = 0x00; png[18] = 0x04; png[19] = 0x00;
            png[20] = 0x00; png[21] = 0x00; png[22] = 0x02; png[23] = 0x00;

            ImageIdentifyResult result = ImgCodecsCv2.Identify(png);

            Assert.Equal("png", result.Format);
            Assert.True(result.IsFormatKnown);
            Assert.True(result.IsSizeKnown);
            Assert.Equal(1024, result.Width);
            Assert.Equal(512, result.Height);
            Assert.Equal(0, result.FrameCount);
            Assert.False(result.IsFrameCountKnown);
        }

        [Fact]
        public void IdentifyStreamRestoresSeekablePosition()
        {
            byte[] png = CreateCompletePng(2, 3, 8, 2);
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x7F);
                stream.Write(png, 0, png.Length);
                stream.Position = 1;
                ImageDecodeOptions options = new ImageDecodeOptions(maxInputBytes: png.Length, maxWidth: 10, maxHeight: 10,
                    maxPixels: 100, maxFrames: 1, rejectUnknownFormat: true, requireKnownSize: true);

                ImageIdentifyResult result = ImgCodecsCv2.Identify(stream, options);

                Assert.Equal("png", result.Format);
                Assert.Equal(1, stream.Position);
            }
        }

        [Fact]
        public void IdentifyStreamConsumesNonSeekableInputAndSupportsShortReads()
        {
            byte[] png = CreateCompletePng(2, 3, 8, 2);
            using (var stream = new NonSeekableReadStream(png, 2))
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(stream, new ImageDecodeOptions());

                Assert.Equal("png", result.Format);
                Assert.Equal(png.Length, stream.BytesRead);
                Assert.True(stream.ReadCalls > 1);
            }
        }

        [Fact]
        public void IdentifyStreamRejectsInputLimitAndRestoresSeekablePosition()
        {
            byte[] png = CreateCompletePng(2, 3, 8, 2);
            using (var stream = new MemoryStream(png))
            {
                ImageDecodeOptions options = new ImageDecodeOptions(maxInputBytes: png.Length - 1, maxWidth: 10, maxHeight: 10,
                    maxPixels: 100, maxFrames: 1, rejectUnknownFormat: true, requireKnownSize: true);

                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.Identify(stream, options));
                Assert.Equal(0, stream.Position);
            }
        }

        [Fact]
        public void ImDecodeStreamAppliesPreflightBeforeNativeCall()
        {
            using (var stream = new MemoryStream(new byte[] { 1, 2, 3 }))
            {
                ImageDecodeOptions options = new ImageDecodeOptions(maxInputBytes: 32, maxWidth: 10, maxHeight: 10,
                    maxPixels: 100, maxFrames: 1, rejectUnknownFormat: true, requireKnownSize: false);

                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                Assert.Equal(0, stream.Position);
            }
        }

        [Fact]
        public void IdentifyReadsJpegAndWebpHeaders()
        {
            byte[] jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x01, 0x20, 0x02, 0x80, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xD9 };
            ImageIdentifyResult jpegResult = ImgCodecsCv2.Identify(jpeg);
            Assert.Equal("jpeg", jpegResult.Format);
            Assert.Equal(640, jpegResult.Width);
            Assert.Equal(288, jpegResult.Height);

            byte[] webp = new byte[30];
            webp[0] = (byte)'R'; webp[1] = (byte)'I'; webp[2] = (byte)'F'; webp[3] = (byte)'F';
            webp[8] = (byte)'W'; webp[9] = (byte)'E'; webp[10] = (byte)'B'; webp[11] = (byte)'P';
            webp[12] = (byte)'V'; webp[13] = (byte)'P'; webp[14] = (byte)'8'; webp[15] = (byte)'X';
            webp[16] = 10; webp[24] = 99; webp[25] = 0; webp[26] = 0; webp[27] = 49; webp[28] = 0; webp[29] = 0;
            ImageIdentifyResult webpResult = ImgCodecsCv2.Identify(webp);
            Assert.Equal("webp", webpResult.Format);
            Assert.Equal(100, webpResult.Width);
            Assert.Equal(50, webpResult.Height);
            Assert.True(jpegResult.IsPixelFormatKnown);
            Assert.Equal(8, jpegResult.BitDepth);
            Assert.Equal(1, jpegResult.ChannelCount);
            Assert.True(jpegResult.IsFrameCountKnown);
            Assert.Equal(1, jpegResult.FrameCount);

            byte[] incompleteJpeg = new byte[jpeg.Length - 2];
            Array.Copy(jpeg, incompleteJpeg, incompleteJpeg.Length);
            Assert.False(ImgCodecsCv2.Identify(incompleteJpeg).IsFrameCountKnown);

            byte[] jpegWithTrailingBytes = new byte[jpeg.Length + 1];
            Array.Copy(jpeg, jpegWithTrailingBytes, jpeg.Length);
            jpegWithTrailingBytes[jpegWithTrailingBytes.Length - 1] = 0x01;
            Assert.False(ImgCodecsCv2.Identify(jpegWithTrailingBytes).IsFrameCountKnown);
        }

        [Fact]
        public void IdentifyReadsPngEncodedDepthAndChannels()
        {
            byte[] png = CreateCompletePng(2, 3, 16, 6);

            ImageIdentifyResult result = ImgCodecsCv2.Identify(png);

            Assert.True(result.IsPixelFormatKnown);
            Assert.Equal(16, result.BitDepth);
            Assert.Equal(4, result.ChannelCount);
        }

        [Fact]
        public void IdentifyReadsPnmEncodedDepthAndChannels()
        {
            ImageIdentifyResult pbm = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("P1\n2 3\n"));
            Assert.True(pbm.IsSizeKnown);
            Assert.True(pbm.IsFrameCountKnown);
            Assert.True(pbm.IsPixelFormatKnown);
            Assert.Equal(1, pbm.BitDepth);
            Assert.Equal(1, pbm.ChannelCount);

            ImageIdentifyResult pbmBinary = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("P4\n2 3\n"));
            Assert.True(pbmBinary.IsPixelFormatKnown);
            Assert.Equal(1, pbmBinary.BitDepth);
            Assert.Equal(1, pbmBinary.ChannelCount);

            ImageIdentifyResult pgmAscii = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("P2\n4 2\n15\n"));
            Assert.True(pgmAscii.IsPixelFormatKnown);
            Assert.Equal(8, pgmAscii.BitDepth);
            Assert.Equal(1, pgmAscii.ChannelCount);

            ImageIdentifyResult pgm = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("P5\n# dimensions\n4 2\n255\n"));
            Assert.True(pgm.IsPixelFormatKnown);
            Assert.Equal(8, pgm.BitDepth);
            Assert.Equal(1, pgm.ChannelCount);

            ImageIdentifyResult ppmAscii = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("P3\n4 2\n255\n"));
            Assert.True(ppmAscii.IsPixelFormatKnown);
            Assert.Equal(8, ppmAscii.BitDepth);
            Assert.Equal(3, ppmAscii.ChannelCount);

            ImageIdentifyResult ppm = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("P6\n4 2\n65535\n"));
            Assert.True(ppm.IsPixelFormatKnown);
            Assert.Equal(16, ppm.BitDepth);
            Assert.Equal(3, ppm.ChannelCount);
        }

        [Fact]
        public void IdentifyReadsPamAndPfmEncodedDepthAndChannels()
        {
            byte[] pamBytes = Encoding.ASCII.GetBytes("P7\nWIDTH 2\nHEIGHT 3\nDEPTH 4\nMAXVAL 255\nTUPLTYPE RGB_ALPHA\nENDHDR\n");
            ImageIdentifyResult pam = ImgCodecsCv2.Identify(pamBytes);
            Assert.True(pam.IsSizeKnown);
            Assert.True(pam.IsFrameCountKnown);
            Assert.True(pam.IsPixelFormatKnown);
            Assert.Equal(8, pam.BitDepth);
            Assert.Equal(4, pam.ChannelCount);

            ImageIdentifyResult pfm = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("PF\n2 3\n-1.0\n"));
            Assert.True(pfm.IsPixelFormatKnown);
            Assert.Equal(32, pfm.BitDepth);
            Assert.Equal(3, pfm.ChannelCount);

            ImageIdentifyResult pf = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("Pf\n2 3\n1.0\n"));
            Assert.True(pf.IsPixelFormatKnown);
            Assert.Equal(32, pf.BitDepth);
            Assert.Equal(1, pf.ChannelCount);
        }

        [Fact]
        public void IdentifyDoesNotClaimIncompletePamOrInvalidPfmFacts()
        {
            ImageIdentifyResult missingEnd = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("P7\nWIDTH 2\nHEIGHT 3\nDEPTH 4\nMAXVAL 255\n"));
            Assert.False(missingEnd.IsSizeKnown);
            Assert.False(missingEnd.IsFrameCountKnown);
            Assert.False(missingEnd.IsPixelFormatKnown);

            ImageIdentifyResult zeroScale = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("PF\n2 3\n0\n"));
            Assert.False(zeroScale.IsSizeKnown);
            Assert.False(zeroScale.IsFrameCountKnown);
            Assert.False(zeroScale.IsPixelFormatKnown);

            ImageIdentifyResult invalidScale = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("Pf\n2 3\nnot-a-scale\n"));
            Assert.False(invalidScale.IsSizeKnown);
            Assert.False(invalidScale.IsFrameCountKnown);
            Assert.False(invalidScale.IsPixelFormatKnown);
        }

        [Fact]
        public void DecodeOptionsRejectKnownPfmPixelFormatLimitsBeforeNativeCall()
        {
            byte[] pfm = Encoding.ASCII.GetBytes("PF\n2 3\n-1.0\n");

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(pfm,
                new ImageDecodeOptions(4096, 100, 100, 10000, 1, true, true,
                    long.MaxValue, long.MaxValue, false, false, long.MaxValue, 16, 3, false)));
        }

        [Fact]
        public void IdentifyReadsSunRasterEncodedDepthAndChannels()
        {
            ImageIdentifyResult indexed = ImgCodecsCv2.Identify(CreateSunRaster(8));
            Assert.True(indexed.IsSizeKnown);
            Assert.True(indexed.IsFrameCountKnown);
            Assert.True(indexed.IsPixelFormatKnown);
            Assert.Equal(8, indexed.BitDepth);
            Assert.Equal(1, indexed.ChannelCount);

            ImageIdentifyResult bgr = ImgCodecsCv2.Identify(CreateSunRaster(24));
            Assert.Equal(8, bgr.BitDepth);
            Assert.Equal(3, bgr.ChannelCount);

            ImageIdentifyResult rgbaStorage = ImgCodecsCv2.Identify(CreateSunRaster(32));
            Assert.Equal(8, rgbaStorage.BitDepth);
            Assert.Equal(4, rgbaStorage.ChannelCount);
        }

        [Fact]
        public void IdentifyDoesNotClaimIncompleteSunRasterFacts()
        {
            byte[] raster = CreateSunRaster(24);
            WriteBe32(raster, 16, uint.MaxValue);
            ImageIdentifyResult oversized = ImgCodecsCv2.Identify(raster);
            Assert.True(oversized.IsSizeKnown);
            Assert.False(oversized.IsFrameCountKnown);
            Assert.False(oversized.IsPixelFormatKnown);

            byte[] truncatedMap = CreateSunRaster(24);
            WriteBe32(truncatedMap, 24, 1);
            WriteBe32(truncatedMap, 28, 3);
            Assert.False(ImgCodecsCv2.Identify(truncatedMap).IsFrameCountKnown);

            byte[] emptyPayload = CreateSunRaster(24);
            WriteBe32(emptyPayload, 16, 0);
            Assert.False(ImgCodecsCv2.Identify(emptyPayload).IsFrameCountKnown);

            Array.Resize(ref raster, 31);
            ImageIdentifyResult truncated = ImgCodecsCv2.Identify(raster);
            Assert.False(truncated.IsSizeKnown);
            Assert.False(truncated.IsFrameCountKnown);
            Assert.False(truncated.IsPixelFormatKnown);
        }

        [Fact]
        public void IdentifyReadsFlatAndRleRadianceHdrFacts()
        {
            ImageIdentifyResult flat = ImgCodecsCv2.Identify(CreateRadianceHdr(2, 3, false));
            Assert.Equal("hdr", flat.Format);
            Assert.True(flat.IsSizeKnown);
            Assert.Equal(2, flat.Width);
            Assert.Equal(3, flat.Height);
            Assert.True(flat.IsFrameCountKnown);
            Assert.Equal(1, flat.FrameCount);
            Assert.True(flat.IsPixelFormatKnown);
            Assert.Equal(8, flat.BitDepth);
            Assert.Equal(4, flat.ChannelCount);
            Assert.True(flat.IsCumulativePixelCountKnown);
            Assert.Equal(6, flat.CumulativePixelCount);

            ImageIdentifyResult rle = ImgCodecsCv2.Identify(CreateRadianceHdr(8, 2, true));
            Assert.True(rle.IsSizeKnown);
            Assert.Equal(8, rle.Width);
            Assert.Equal(2, rle.Height);
            Assert.True(rle.IsFrameCountKnown);
            Assert.True(rle.IsPixelFormatKnown);

            ImageIdentifyResult shortSignature = ImgCodecsCv2.Identify(CreateRadianceHdr(2, 3, false, "#?RGBE"));
            Assert.Equal("hdr", shortSignature.Format);
            Assert.True(shortSignature.IsSizeKnown);
            Assert.True(shortSignature.IsFrameCountKnown);
        }

        [Fact]
        public void IdentifyDoesNotClaimMalformedRadianceHdrFacts()
        {
            byte[] complete = CreateRadianceHdr(8, 2, true);
            int headerLength = GetRadianceHdrHeaderLength(8, 2);

            byte[] headerOnly = new byte[headerLength];
            Array.Copy(complete, headerOnly, headerLength);
            ImageIdentifyResult incomplete = ImgCodecsCv2.Identify(headerOnly);
            Assert.True(incomplete.IsSizeKnown);
            Assert.True(incomplete.IsPixelFormatKnown);
            Assert.False(incomplete.IsFrameCountKnown);
            Assert.False(incomplete.IsCumulativePixelCountKnown);

            byte[] zeroRun = (byte[])complete.Clone();
            zeroRun[headerLength + 4] = 0;
            Assert.False(ImgCodecsCv2.Identify(zeroRun).IsFrameCountKnown);

            byte[] wrongWidth = (byte[])complete.Clone();
            wrongWidth[headerLength + 3] = 7;
            Assert.False(ImgCodecsCv2.Identify(wrongWidth).IsFrameCountKnown);

            ImageIdentifyResult missingFormat = ImgCodecsCv2.Identify(
                Encoding.ASCII.GetBytes("#?RGBE\n\n-Y 2 +X 8\n"));
            Assert.True(missingFormat.IsFormatKnown);
            Assert.False(missingFormat.IsSizeKnown);
            Assert.False(missingFormat.IsPixelFormatKnown);

            ImageIdentifyResult oversized = ImgCodecsCv2.Identify(
                Encoding.ASCII.GetBytes("#?RADIANCE\nFORMAT=32-bit_rle_rgbe\n\n-Y 2147483648 +X 8\n"));
            Assert.False(oversized.IsSizeKnown);

            ImageIdentifyResult multiplicationOverflow = ImgCodecsCv2.Identify(
                Encoding.ASCII.GetBytes("#?RADIANCE\nFORMAT=32-bit_rle_rgbe\n\n-Y 2147483647 +X 2147483647\n"));
            Assert.True(multiplicationOverflow.IsSizeKnown);
            Assert.False(multiplicationOverflow.IsFrameCountKnown);
            Assert.False(multiplicationOverflow.IsCumulativePixelCountKnown);
        }

        [Fact]
        public void DecodeOptionsRejectKnownRadianceHdrStorageLimitsBeforeNativeCall()
        {
            byte[] hdr = CreateRadianceHdr(2, 3, false);

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(hdr,
                new ImageDecodeOptions(4096, 100, 100, 10000, 1, true, true,
                    long.MaxValue, long.MaxValue, false, false, long.MaxValue, 8, 3, false)));
        }

        [Fact]
        public void IdentifyReadsOpenExrDataWindowAndChannels()
        {
            ImageIdentifyResult exr = ImgCodecsCv2.Identify(CreateOpenExrHeader(640, 480, 4, 1));
            Assert.Equal("exr", exr.Format);
            Assert.True(exr.IsSizeKnown);
            Assert.Equal(640, exr.Width);
            Assert.Equal(480, exr.Height);
            Assert.False(exr.IsFrameCountKnown);
            Assert.True(exr.IsPixelFormatKnown);
            Assert.Equal(16, exr.BitDepth);
            Assert.Equal(4, exr.ChannelCount);

            ImageIdentifyResult mixed = ImgCodecsCv2.Identify(CreateOpenExrHeader(3, 2, 3, 2, 1));
            Assert.True(mixed.IsSizeKnown);
            Assert.True(mixed.IsChannelCountKnown);
            Assert.Equal(3, mixed.ChannelCount);
            Assert.False(mixed.IsPixelFormatKnown);
        }

        [Fact]
        public void IdentifyDoesNotClaimIncompleteOpenExrHeaderFacts()
        {
            byte[] complete = CreateOpenExrHeader(8, 6, 3, 2);
            ImageIdentifyResult completeResult = ImgCodecsCv2.Identify(complete);
            Assert.True(completeResult.IsSizeKnown);
            Assert.False(completeResult.IsFrameCountKnown);

            for (int length = 1; length < complete.Length; ++length)
            {
                byte[] prefix = new byte[length];
                Array.Copy(complete, prefix, length);
                ImageIdentifyResult result = ImgCodecsCv2.Identify(prefix);
                Assert.False(result.IsSizeKnown, "OpenEXR prefix " + length + " claimed a complete header");
                Assert.False(result.IsFrameCountKnown, "OpenEXR prefix " + length + " claimed a frame");
            }

            byte[] invalidWindow = (byte[])complete.Clone();
            int dataWindowValue = FindOpenExrAttributeValue(invalidWindow, "dataWindow");
            WriteLe32(invalidWindow, dataWindowValue + 8, -1);
            WriteLe32(invalidWindow, dataWindowValue + 12, -1);
            Assert.False(ImgCodecsCv2.Identify(invalidWindow).IsSizeKnown);

            byte[] invalidChannels = (byte[])complete.Clone();
            int channelsValue = FindOpenExrAttributeValue(invalidChannels, "channels");
            WriteLe32(invalidChannels, channelsValue + 2, 3);
            Assert.False(ImgCodecsCv2.Identify(invalidChannels).IsSizeKnown);
        }

        [Fact]
        public void DecodeOptionsRejectKnownOpenExrChannelLimitsBeforeNativeCall()
        {
            byte[] exr = CreateOpenExrHeader(2, 2, 4, 2);
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(exr,
                new ImageDecodeOptions(4096, 100, 100, 10000, 1, true, true,
                    long.MaxValue, long.MaxValue, false, false, long.MaxValue, 16, 3, false)));
        }

        [Fact]
        public void IdentifyReadsWebpEncodedDepthAndChannels()
        {
            byte[] vp8Payload = new byte[6];
            vp8Payload[3] = 0x9D; vp8Payload[4] = 0x01; vp8Payload[5] = 0x2A;
            ImageIdentifyResult vp8 = ImgCodecsCv2.Identify(CreateStaticWebp("VP8 ", vp8Payload));
            Assert.True(vp8.IsPixelFormatKnown);
            Assert.Equal(8, vp8.BitDepth);
            Assert.Equal(3, vp8.ChannelCount);

            byte[] vp8lPayload = new byte[] { 0x2F, 0, 0, 0, 0 };
            ImageIdentifyResult vp8l = ImgCodecsCv2.Identify(CreateStaticWebp("VP8L", vp8lPayload));
            Assert.True(vp8l.IsPixelFormatKnown);
            Assert.Equal(8, vp8l.BitDepth);
            Assert.Equal(3, vp8l.ChannelCount);

            vp8lPayload[4] = 0x10;
            ImageIdentifyResult vp8lAlpha = ImgCodecsCv2.Identify(CreateStaticWebp("VP8L", vp8lPayload));
            Assert.True(vp8lAlpha.IsPixelFormatKnown);
            Assert.Equal(8, vp8lAlpha.BitDepth);
            Assert.Equal(4, vp8lAlpha.ChannelCount);

            ImageIdentifyResult animatedAlpha = ImgCodecsCv2.Identify(CreateAnimatedWebp(2, true));
            Assert.True(animatedAlpha.IsPixelFormatKnown);
            Assert.Equal(8, animatedAlpha.BitDepth);
            Assert.Equal(4, animatedAlpha.ChannelCount);
        }

        [Fact]
        public void IdentifyReadsBmpEncodedDepthAndChannels()
        {
            ImageIdentifyResult indexed = ImgCodecsCv2.Identify(CreateBmpFixture(8, 0));
            Assert.True(indexed.IsSizeKnown);
            Assert.True(indexed.IsFrameCountKnown);
            Assert.True(indexed.IsPixelFormatKnown);
            Assert.Equal(8, indexed.BitDepth);
            Assert.Equal(1, indexed.ChannelCount);

            ImageIdentifyResult bgr = ImgCodecsCv2.Identify(CreateBmpFixture(24, 0));
            Assert.True(bgr.IsPixelFormatKnown);
            Assert.Equal(8, bgr.BitDepth);
            Assert.Equal(3, bgr.ChannelCount);

            ImageIdentifyResult bgrx = ImgCodecsCv2.Identify(CreateBmpFixture(32, 0));
            Assert.True(bgrx.IsPixelFormatKnown);
            Assert.Equal(8, bgrx.BitDepth);
            Assert.Equal(3, bgrx.ChannelCount);

            ImageIdentifyResult core = ImgCodecsCv2.Identify(CreateBmpCoreFixture(24));
            Assert.True(core.IsPixelFormatKnown);
            Assert.Equal(8, core.BitDepth);
            Assert.Equal(3, core.ChannelCount);
        }

        [Fact]
        public void IdentifyDoesNotClaimIncompleteOrCompressedBmpFacts()
        {
            byte[] truncated = CreateBmpFixture(24, 0);
            Array.Resize(ref truncated, 26);
            ImageIdentifyResult truncatedResult = ImgCodecsCv2.Identify(truncated);
            Assert.False(truncatedResult.IsFrameCountKnown);
            Assert.False(truncatedResult.IsPixelFormatKnown);

            ImageIdentifyResult compressed = ImgCodecsCv2.Identify(CreateBmpFixture(8, 1));
            Assert.True(compressed.IsFrameCountKnown);
            Assert.False(compressed.IsPixelFormatKnown);
        }

        [Fact]
        public void DecodeOptionsRejectKnownBmpPixelFormatLimitsBeforeNativeCall()
        {
            byte[] bmp = CreateBmpFixture(24, 0);

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(bmp,
                new ImageDecodeOptions(1024, 100, 100, 10000, 1, true, true,
                    long.MaxValue, long.MaxValue, false, false, long.MaxValue, 8, 2, false)));
        }

        [Fact]
        public void IdentifyReadsGifIndexedDepthAndChannels()
        {
            ImageIdentifyResult oneBit = ImgCodecsCv2.Identify(CreateAnimatedGif(2));
            Assert.True(oneBit.IsPixelFormatKnown);
            Assert.Equal(1, oneBit.BitDepth);
            Assert.Equal(1, oneBit.ChannelCount);

            ImageIdentifyResult eightBit = ImgCodecsCv2.Identify(CreateAnimatedGif(1, 8));
            Assert.True(eightBit.IsPixelFormatKnown);
            Assert.Equal(8, eightBit.BitDepth);
            Assert.Equal(1, eightBit.ChannelCount);
        }

        [Fact]
        public void DecodeOptionsRejectKnownGifPixelFormatLimitsBeforeNativeCall()
        {
            byte[] gif = CreateAnimatedGif(1, 8);

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(gif,
                new ImageDecodeOptions(4096, 100, 100, 10000, 1, true, true,
                    long.MaxValue, long.MaxValue, false, false, long.MaxValue, 4, 1, false)));
        }

        [Fact]
        public void IdentifyDoesNotClaimIncompletePnmHeaderFacts()
        {
            ImageIdentifyResult missingMaxValue = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("P6\n4 2\n"));
            Assert.False(missingMaxValue.IsSizeKnown);
            Assert.False(missingMaxValue.IsFrameCountKnown);
            Assert.False(missingMaxValue.IsPixelFormatKnown);

            ImageIdentifyResult invalidMaxValue = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("P5\n4 2\n0\n"));
            Assert.False(invalidMaxValue.IsSizeKnown);
            Assert.False(invalidMaxValue.IsFrameCountKnown);
            Assert.False(invalidMaxValue.IsPixelFormatKnown);

            ImageIdentifyResult oversizedInteger = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("P6\n999999999999999999999 2\n255\n"));
            Assert.False(oversizedInteger.IsSizeKnown);
            Assert.False(oversizedInteger.IsFrameCountKnown);
            Assert.False(oversizedInteger.IsPixelFormatKnown);

            ImageIdentifyResult gluedToken = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("P6\n4x 2\n255\n"));
            Assert.False(gluedToken.IsSizeKnown);
            Assert.False(gluedToken.IsFrameCountKnown);
            Assert.False(gluedToken.IsPixelFormatKnown);

            ImageIdentifyResult truncatedPpmSeparator = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("P6\n4 2\n255"));
            Assert.False(truncatedPpmSeparator.IsSizeKnown);
            Assert.False(truncatedPpmSeparator.IsFrameCountKnown);
            Assert.False(truncatedPpmSeparator.IsPixelFormatKnown);

            ImageIdentifyResult truncatedPbmSeparator = ImgCodecsCv2.Identify(Encoding.ASCII.GetBytes("P1\n2 3"));
            Assert.False(truncatedPbmSeparator.IsSizeKnown);
            Assert.False(truncatedPbmSeparator.IsFrameCountKnown);
            Assert.False(truncatedPbmSeparator.IsPixelFormatKnown);
        }

        [Fact]
        public void DecodeOptionsRejectKnownPnmPixelFormatLimitsBeforeNativeCall()
        {
            byte[] pnm = Encoding.ASCII.GetBytes("P6\n4 2\n65535\n");

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(pnm,
                new ImageDecodeOptions(1024, 100, 100, 10000, 1, true, true,
                    long.MaxValue, long.MaxValue, false, false, long.MaxValue, 8, 4, false)));
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(pnm,
                new ImageDecodeOptions(1024, 100, 100, 10000, 1, true, true,
                    long.MaxValue, long.MaxValue, false, false, long.MaxValue, 16, 2, false)));
        }

        [Fact]
        public void IdentifyReadsClassicTiffDimensionsAndPageCount()
        {
            ImageIdentifyResult littleEndian = ImgCodecsCv2.Identify(CreateTiff(false, 2));
            Assert.True(littleEndian.IsSizeKnown);
            Assert.Equal(320, littleEndian.Width);
            Assert.Equal(240, littleEndian.Height);
            Assert.True(littleEndian.IsFrameCountKnown);
            Assert.Equal(2, littleEndian.FrameCount);

            ImageIdentifyResult bigEndian = ImgCodecsCv2.Identify(CreateTiff(true, 1));
            Assert.True(bigEndian.IsSizeKnown);
            Assert.Equal(320, bigEndian.Width);
            Assert.Equal(240, bigEndian.Height);
            Assert.Equal(1, bigEndian.FrameCount);
        }

        [Fact]
        public void IdentifyReadsBigTiffDimensionsPageCountAndCumulativePixels()
        {
            ImageIdentifyResult littleEndian = ImgCodecsCv2.Identify(CreateBigTiff(false, 2));
            Assert.Equal("bigtiff", littleEndian.Format);
            Assert.True(littleEndian.IsSizeKnown);
            Assert.Equal(320, littleEndian.Width);
            Assert.Equal(240, littleEndian.Height);
            Assert.True(littleEndian.IsFrameCountKnown);
            Assert.Equal(2, littleEndian.FrameCount);
            Assert.True(littleEndian.IsCumulativePixelCountKnown);
            Assert.Equal(153600, littleEndian.CumulativePixelCount);
            Assert.True(littleEndian.IsPixelFormatKnown);
            Assert.Equal(8, littleEndian.BitDepth);
            Assert.Equal(3, littleEndian.ChannelCount);

            ImageIdentifyResult bigEndian = ImgCodecsCv2.Identify(CreateBigTiff(true, 1));
            Assert.Equal("bigtiff", bigEndian.Format);
            Assert.True(bigEndian.IsSizeKnown);
            Assert.Equal(320, bigEndian.Width);
            Assert.Equal(240, bigEndian.Height);
            Assert.Equal(1, bigEndian.FrameCount);
        }

        [Fact]
        public void IdentifyKeepsHeterogeneousBigTiffSizeUnknownWhileCountingPages()
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(CreateBigTiff(false, 2, true));

            Assert.Equal("bigtiff", result.Format);
            Assert.False(result.IsSizeKnown);
            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(2, result.FrameCount);
            Assert.True(result.IsCumulativePixelCountKnown);
            Assert.Equal(96000, result.CumulativePixelCount);
            Assert.True(result.IsPixelFormatKnown);
        }

        [Fact]
        public void IdentifyKeepsInconsistentBigTiffPixelFactsUnknown()
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(CreateBigTiff(false, 2, false, true));

            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(2, result.FrameCount);
            Assert.False(result.IsPixelFormatKnown);
            Assert.False(result.IsBitDepthKnown);
            Assert.False(result.IsChannelCountKnown);
        }

        [Fact]
        public void DecodeOptionsRejectKnownBigTiffCumulativePixelBudgetBeforeNativeCall()
        {
            byte[] bigTiff = CreateBigTiff(false, 2);

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(bigTiff,
                new ImageDecodeOptions(4096, 1000, 1000, long.MaxValue, 2, true, false,
                    long.MaxValue, long.MaxValue, false, false, 153599, int.MaxValue, int.MaxValue, false)));
        }

        [Fact]
        public void IdentifyDoesNotClaimMalformedBigTiffFacts()
        {
            byte[] cyclic = CreateBigTiff(false, 2);
            WriteTiff64(cyclic, 104, 16, false);
            ImageIdentifyResult cyclicResult = ImgCodecsCv2.Identify(cyclic);
            Assert.Equal("bigtiff", cyclicResult.Format);
            Assert.False(cyclicResult.IsSizeKnown);
            Assert.False(cyclicResult.IsFrameCountKnown);
            Assert.False(cyclicResult.IsCumulativePixelCountKnown);

            byte[] truncated = CreateBigTiff(true, 1);
            Array.Resize(ref truncated, truncated.Length - 1);
            ImageIdentifyResult truncatedResult = ImgCodecsCv2.Identify(truncated);
            Assert.False(truncatedResult.IsSizeKnown);
            Assert.False(truncatedResult.IsFrameCountKnown);
            Assert.False(truncatedResult.IsCumulativePixelCountKnown);
        }

        [Fact]
        public void IdentifyDoesNotClaimMalformedTiffFacts()
        {
            byte[] tiff = CreateTiff(false, 2);
            tiff[8] = 0xFF;
            tiff[9] = 0xFF;
            tiff[10] = 0xFF;
            tiff[11] = 0x7F;
            ImageIdentifyResult result = ImgCodecsCv2.Identify(tiff);
            Assert.False(result.IsSizeKnown);
            Assert.False(result.IsFrameCountKnown);
        }

        [Fact]
        public void IdentifyKeepsHeterogeneousTiffSizeUnknownWhileCountingPages()
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(CreateTiff(false, 2, true));

            Assert.False(result.IsSizeKnown);
            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(2, result.FrameCount);
            Assert.True(result.IsCumulativePixelCountKnown);
            Assert.Equal(96000, result.CumulativePixelCount);
        }

        [Fact]
        public void DecodeOptionsRejectKnownHeterogeneousTiffCumulativePixelBudgetBeforeNativeCall()
        {
            byte[] tiff = CreateTiff(false, 2, true);

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(tiff,
                new ImageDecodeOptions(4096, 1000, 1000, long.MaxValue, 2, true, false,
                    long.MaxValue, long.MaxValue, false, false, 95999, int.MaxValue, int.MaxValue, false)));
        }

        [Fact]
        public void DecodeOptionsRejectKnownTiffPageBudgetBeforeNativeCall()
        {
            byte[] tiff = CreateTiff(false, 2);

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(tiff,
                new ImageDecodeOptions(4096, 1000, 1000, 1000000, 1, true, true)));
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(tiff,
                new ImageDecodeOptions(4096, 1000, 1000, 1000000, 2, true, true,
                    long.MaxValue, long.MaxValue, false, false, 100000, int.MaxValue, int.MaxValue, false)));
        }

        [Fact]
        public void IdentifyCountsCompleteGifApngAndWebpAnimations()
        {
            ImageIdentifyResult gif = ImgCodecsCv2.Identify(CreateAnimatedGif(2));
            Assert.True(gif.IsFrameCountKnown);
            Assert.Equal(2, gif.FrameCount);

            ImageIdentifyResult png = ImgCodecsCv2.Identify(CreateApng(3));
            Assert.True(png.IsFrameCountKnown);
            Assert.Equal(3, png.FrameCount);
            Assert.True(png.IsCumulativePixelCountKnown);
            Assert.Equal(18, png.CumulativePixelCount);

            ImageIdentifyResult webp = ImgCodecsCv2.Identify(CreateAnimatedWebp(4));
            Assert.True(webp.IsFrameCountKnown);
            Assert.Equal(4, webp.FrameCount);
        }

        [Fact]
        public void IdentifyDoesNotClaimIncompleteAnimationFrameCounts()
        {
            byte[] apng = CreateApng(2);
            Array.Resize(ref apng, apng.Length - 12);
            Assert.False(ImgCodecsCv2.Identify(apng).IsFrameCountKnown);

            byte[] webp = CreateAnimatedWebp(2);
            webp[4]--;
            Assert.False(ImgCodecsCv2.Identify(webp).IsFrameCountKnown);

            byte[] gif = CreateAnimatedGif(2);
            Array.Resize(ref gif, gif.Length - 1);
            Assert.False(ImgCodecsCv2.Identify(gif).IsFrameCountKnown);

            byte[] incompletePng = CreatePngHeader(2, 3, 8, 2);
            Assert.False(ImgCodecsCv2.Identify(incompletePng).IsFrameCountKnown);

            byte[] incompleteWebp = new byte[30];
            incompleteWebp[0] = (byte)'R'; incompleteWebp[1] = (byte)'I'; incompleteWebp[2] = (byte)'F'; incompleteWebp[3] = (byte)'F';
            incompleteWebp[8] = (byte)'W'; incompleteWebp[9] = (byte)'E'; incompleteWebp[10] = (byte)'B'; incompleteWebp[11] = (byte)'P';
            incompleteWebp[12] = (byte)'V'; incompleteWebp[13] = (byte)'P'; incompleteWebp[14] = (byte)'8'; incompleteWebp[15] = (byte)'X';
            incompleteWebp[16] = 10;
            Assert.False(ImgCodecsCv2.Identify(incompleteWebp).IsFrameCountKnown);
        }

        [Fact]
        public void IdentifyTruncatedContainerCorpusFailsClosed()
        {
            var fixtures = new[]
            {
                new { Name = "png", Bytes = CreateCompletePng(2, 3, 8, 2) },
                new { Name = "jpeg", Bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x01, 0x20, 0x02, 0x80, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xD9 } },
                new { Name = "gif", Bytes = CreateAnimatedGif(2) },
                new { Name = "apng", Bytes = CreateApng(3) },
                new { Name = "webp", Bytes = CreateAnimatedWebp(4) },
                new { Name = "bmp", Bytes = CreateBmpFixture(24, 0) },
                new { Name = "pam", Bytes = Encoding.ASCII.GetBytes("P7\nWIDTH 2\nHEIGHT 3\nDEPTH 4\nMAXVAL 255\nENDHDR\n") },
                new { Name = "sunraster", Bytes = CreateSunRaster(24) },
                new { Name = "hdr", Bytes = CreateRadianceHdr(8, 2, true) },
                new { Name = "tiff", Bytes = CreateTiff(false, 2) },
                new { Name = "bigtiff", Bytes = CreateBigTiff(false, 2) }
            };

            foreach (var fixture in fixtures)
            {
                Assert.True(ImgCodecsCv2.Identify(fixture.Bytes).IsFrameCountKnown, fixture.Name + " fixture must be complete");
                for (int length = 1; length < fixture.Bytes.Length; ++length)
                {
                    byte[] truncated = new byte[length];
                    Array.Copy(fixture.Bytes, truncated, length);
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(truncated);

                    Assert.False(result.IsFrameCountKnown, fixture.Name + " prefix " + length + " claimed a complete frame chain");
                    Assert.False(result.IsCumulativePixelCountKnown, fixture.Name + " prefix " + length + " claimed cumulative pixels");
                }
            }
        }

        [Fact]
        public void IdentifyMalformedLengthAndDirectoryCorpusFailsClosed()
        {
            byte[] png = CreateCompletePng(2, 3, 8, 2);
            png[8] = 0x7F; png[9] = 0xFF; png[10] = 0xFF; png[11] = 0xFF;

            byte[] jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0xC0, 0xFF, 0xFF, 0x08, 0x01, 0x20, 0x02, 0x80, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xD9 };

            byte[] webp = CreateAnimatedWebp(2);
            webp[4] = 0xFF; webp[5] = 0xFF; webp[6] = 0xFF; webp[7] = 0xFF;

            byte[] bmp = CreateBmpFixture(24, 0);
            WriteBmp32(bmp, 2, int.MaxValue);

            byte[] gif = CreateAnimatedGif(1, 8);
            Array.Resize(ref gif, gif.Length - 1);

            byte[] tiff = CreateTiff(false, 2);
            WriteTiff32(tiff, 4, int.MaxValue, false);

            byte[] bigTiff = CreateBigTiff(false, 2);
            WriteTiff64(bigTiff, 8, long.MaxValue, false);

            var malformed = new[] { png, jpeg, webp, bmp, gif, tiff, bigTiff };
            foreach (byte[] input in malformed)
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(input);
                Assert.False(result.IsFrameCountKnown);
                Assert.False(result.IsCumulativePixelCountKnown);
            }
        }

        [Fact]
        public void IdentifyDoesNotClaimStaticContainerFrameWithoutImageData()
        {
            Assert.False(ImgCodecsCv2.Identify(CreatePngWithIccProfile(new byte[] { 1, 2, 3 })).IsFrameCountKnown);
            Assert.False(ImgCodecsCv2.Identify(CreateWebpWithMetadataAndIccProfile()).IsFrameCountKnown);
        }

        [Fact]
        public void DecodeOptionsRejectOversizedInputBeforeNativeCall()
        {
            byte[] input = new byte[32];
            ImageDecodeOptions options = new ImageDecodeOptions(16, 4096, 4096, 1000000, 4, false, false);

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(input, options));
        }

        [Fact]
        public void DecodeOptionsRejectKnownDimensionsAndUnknownFormats()
        {
            byte[] png = new byte[24];
            png[0] = 0x89; png[1] = 0x50; png[2] = 0x4E; png[3] = 0x47; png[4] = 0x0D; png[5] = 0x0A; png[6] = 0x1A; png[7] = 0x0A;
            png[12] = (byte)'I'; png[13] = (byte)'H'; png[14] = (byte)'D'; png[15] = (byte)'R';
            png[19] = 100; png[23] = 100;

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(png, new ImageDecodeOptions(1024, 10, 1000, 1000000, 4, false, false)));
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new byte[] { 1, 2, 3 }, new ImageDecodeOptions(1024, 10, 10, 100, 1, true, false)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ImageDecodeOptions(0, 1, 1, 1, 1, false, false));
        }

        [Fact]
        public void IdentifyReportsPngMetadataAndIccPayloadSizes()
        {
            byte[] png = CreatePngWithIccProfile(new byte[] { 1, 2, 3 });

            ImageIdentifyResult result = ImgCodecsCv2.Identify(png);

            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(9, result.MetadataBytes);
            Assert.True(result.IsIccProfileSizeKnown);
            Assert.Equal(3, result.IccProfileBytes);
        }

        [Fact]
        public void IdentifyReportsJpegAndWebpMetadataAndIccPayloadSizes()
        {
            ImageIdentifyResult jpeg = ImgCodecsCv2.Identify(CreateJpegWithMetadataAndIccProfile());
            Assert.True(jpeg.IsMetadataSizeKnown);
            Assert.Equal(19, jpeg.MetadataBytes);
            Assert.True(jpeg.IsIccProfileSizeKnown);
            Assert.Equal(2, jpeg.IccProfileBytes);

            ImageIdentifyResult webp = ImgCodecsCv2.Identify(CreateWebpWithMetadataAndIccProfile());
            Assert.True(webp.IsMetadataSizeKnown);
            Assert.Equal(5, webp.MetadataBytes);
            Assert.True(webp.IsIccProfileSizeKnown);
            Assert.Equal(3, webp.IccProfileBytes);
        }

        [Fact]
        public void IdentifyDoesNotClaimMetadataFactsForIncompleteContainers()
        {
            byte[] png = new byte[24];
            png[0] = 0x89; png[1] = 0x50; png[2] = 0x4E; png[3] = 0x47;
            png[4] = 0x0D; png[5] = 0x0A; png[6] = 0x1A; png[7] = 0x0A;
            ImageIdentifyResult pngResult = ImgCodecsCv2.Identify(png);
            Assert.False(pngResult.IsMetadataSizeKnown);
            Assert.False(pngResult.IsIccProfileSizeKnown);

            byte[] jpeg = CreateJpegWithMetadataAndIccProfile();
            Array.Resize(ref jpeg, jpeg.Length - 2);
            ImageIdentifyResult jpegResult = ImgCodecsCv2.Identify(jpeg);
            Assert.False(jpegResult.IsMetadataSizeKnown);
            Assert.False(jpegResult.IsIccProfileSizeKnown);

            byte[] webp = CreateWebpWithMetadataAndIccProfile();
            webp[4]--;
            ImageIdentifyResult webpResult = ImgCodecsCv2.Identify(webp);
            Assert.False(webpResult.IsMetadataSizeKnown);
            Assert.False(webpResult.IsIccProfileSizeKnown);
        }

        [Fact]
        public void DecodeOptionsRejectKnownMetadataAndIccPayloadLimitsBeforeNativeCall()
        {
            byte[] png = CreatePngWithIccProfile(new byte[] { 1, 2, 3 });

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(png,
                new ImageDecodeOptions(1024, 100, 100, 10000, 1, true, true, 8, 4)));
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(png,
                new ImageDecodeOptions(1024, 100, 100, 10000, 1, true, true, 10, 2)));
        }

        [Fact]
        public void DecodeOptionsCanRequireKnownMetadataFacts()
        {
            byte[] tiff = new byte[] { (byte)'I', (byte)'I', 42, 0 };
            ImageDecodeOptions options = new ImageDecodeOptions(
                1024, 100, 100, 10000, 1, true, false, 1024, 1024, true, true);

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(tiff, options));
        }

        [Fact]
        public void DecodeOptionsRejectPixelDepthChannelsAndCumulativePixels()
        {
            byte[] png = CreateCompletePng(2, 3, 16, 6);

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(png,
                new ImageDecodeOptions(1024, 100, 100, 10000, 1, true, false, 1024, 1024, false, false,
                    long.MaxValue, 8, 4, false)));
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(png,
                new ImageDecodeOptions(1024, 100, 100, 10000, 1, true, false, 1024, 1024, false, false,
                    long.MaxValue, 16, 3, false)));
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(png,
                new ImageDecodeOptions(1024, 100, 100, 10000, 1, true, false, 1024, 1024, false, false,
                    5, 16, 4, false)));
        }

        [Fact]
        public void DecodeOptionsRejectKnownAnimationFrameAndCumulativePixelLimits()
        {
            byte[] apng = CreateApng(3);

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(apng,
                new ImageDecodeOptions(4096, 100, 100, 100, 2, true, true)));
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(apng,
                new ImageDecodeOptions(4096, 100, 100, 100, 3, true, true,
                    long.MaxValue, long.MaxValue, false, false, 12, int.MaxValue, int.MaxValue, false)));
        }

        [Fact]
        public void DecodeOptionsCanRejectUnknownPixelFacts()
        {
            byte[] tiff = new byte[] { (byte)'I', (byte)'I', 42, 0 };
            ImageDecodeOptions options = new ImageDecodeOptions(
                1024, 100, 100, 10000, 1, true, false, 1024, 1024, false, false,
                long.MaxValue, 16, 4, true);

            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(tiff, options));
        }

#if NETCOREAPP3_1_OR_GREATER
        [Fact]
        public void SpanIdentifyUsesTheSameManagedContract()
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(new byte[] { (byte)'B', (byte)'M', 0, 0, 0, 0, 0, 0, 0, 0, 40, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0 }.AsSpan());
            Assert.Equal("bmp", result.Format);
            Assert.Equal(2, result.Width);
            Assert.Equal(3, result.Height);
        }
#endif

        private static byte[] CreatePngWithIccProfile(byte[] profile)
        {
            byte[] header = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            byte[] ihdr = new byte[] { 0, 0, 0, 1, 0, 0, 0, 1, 8, 2, 0, 0, 0 };
            byte[] iccp = new byte[6 + profile.Length];
            iccp[0] = (byte)'s'; iccp[1] = (byte)'R'; iccp[2] = (byte)'G'; iccp[3] = (byte)'B';
            Array.Copy(profile, 0, iccp, 6, profile.Length);

            byte[] png = new byte[header.Length + 12 + ihdr.Length + 12 + iccp.Length + 12];
            Array.Copy(header, png, header.Length);
            int offset = header.Length;
            offset = WritePngChunk(png, offset, "IHDR", ihdr);
            offset = WritePngChunk(png, offset, "iCCP", iccp);
            offset = WritePngChunk(png, offset, "IEND", Array.Empty<byte>());
            Assert.Equal(png.Length, offset);
            return png;
        }

        private static byte[] CreatePngHeader(int width, int height, byte bitDepth, byte colorType)
        {
            byte[] png = new byte[26];
            png[0] = 0x89; png[1] = 0x50; png[2] = 0x4E; png[3] = 0x47;
            png[4] = 0x0D; png[5] = 0x0A; png[6] = 0x1A; png[7] = 0x0A;
            png[11] = 13;
            png[12] = (byte)'I'; png[13] = (byte)'H'; png[14] = (byte)'D'; png[15] = (byte)'R';
            png[16] = (byte)(width >> 24); png[17] = (byte)(width >> 16); png[18] = (byte)(width >> 8); png[19] = (byte)width;
            png[20] = (byte)(height >> 24); png[21] = (byte)(height >> 16); png[22] = (byte)(height >> 8); png[23] = (byte)height;
            png[24] = bitDepth;
            png[25] = colorType;
            return png;
        }

        private static byte[] CreateCompletePng(int width, int height, byte bitDepth, byte colorType)
        {
            byte[] header = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            byte[] ihdr = new byte[]
            {
                (byte)(width >> 24), (byte)(width >> 16), (byte)(width >> 8), (byte)width,
                (byte)(height >> 24), (byte)(height >> 16), (byte)(height >> 8), (byte)height,
                bitDepth, colorType, 0, 0, 0
            };
            byte[] png = new byte[header.Length + 12 + ihdr.Length + 13 + 12];
            Array.Copy(header, png, header.Length);
            int offset = header.Length;
            offset = WritePngChunk(png, offset, "IHDR", ihdr);
            offset = WritePngChunk(png, offset, "IDAT", new byte[] { 0 });
            offset = WritePngChunk(png, offset, "IEND", Array.Empty<byte>());
            Assert.Equal(png.Length, offset);
            return png;
        }

        private static byte[] CreateTiff(bool bigEndian, int pageCount, bool varyingPageSizes = false)
        {
            const int firstIfdOffset = 8;
            const int entriesPerIfd = 2;
            int ifdSize = 2 + entriesPerIfd * 12 + 4;
            byte[] tiff = new byte[firstIfdOffset + pageCount * ifdSize];
            tiff[0] = (byte)(bigEndian ? 'M' : 'I');
            tiff[1] = tiff[0];
            WriteTiff16(tiff, 2, 42, bigEndian);
            WriteTiff32(tiff, 4, firstIfdOffset, bigEndian);
            for (int page = 0; page < pageCount; ++page)
            {
                int offset = firstIfdOffset + page * ifdSize;
                WriteTiff16(tiff, offset, entriesPerIfd, bigEndian);
                int entry = offset + 2;
                WriteTiff16(tiff, entry, 256, bigEndian);
                WriteTiff16(tiff, entry + 2, 4, bigEndian);
                WriteTiff32(tiff, entry + 4, 1, bigEndian);
                WriteTiff32(tiff, entry + 8, varyingPageSizes && page > 0 ? 160 : 320, bigEndian);
                entry += 12;
                WriteTiff16(tiff, entry, 257, bigEndian);
                WriteTiff16(tiff, entry + 2, 4, bigEndian);
                WriteTiff32(tiff, entry + 4, 1, bigEndian);
                WriteTiff32(tiff, entry + 8, varyingPageSizes && page > 0 ? 120 : 240, bigEndian);
                WriteTiff32(tiff, offset + 2 + entriesPerIfd * 12, page + 1 < pageCount ? offset + ifdSize : 0, bigEndian);
            }
            return tiff;
        }

        private static byte[] CreateBigTiff(bool bigEndian, int pageCount, bool varyingPageSizes = false, bool varyingPixelFormats = false)
        {
            const int firstIfdOffset = 16;
            const int entriesPerIfd = 4;
            const int ifdSize = 8 + entriesPerIfd * 20 + 8;
            byte[] tiff = new byte[firstIfdOffset + pageCount * ifdSize];
            tiff[0] = (byte)(bigEndian ? 'M' : 'I');
            tiff[1] = tiff[0];
            WriteTiff16(tiff, 2, 43, bigEndian);
            WriteTiff16(tiff, 4, 8, bigEndian);
            WriteTiff16(tiff, 6, 0, bigEndian);
            WriteTiff64(tiff, 8, firstIfdOffset, bigEndian);
            for (int page = 0; page < pageCount; ++page)
            {
                int offset = firstIfdOffset + page * ifdSize;
                WriteTiff64(tiff, offset, entriesPerIfd, bigEndian);
                int entry = offset + 8;
                WriteTiff16(tiff, entry, 256, bigEndian);
                WriteTiff16(tiff, entry + 2, 16, bigEndian);
                WriteTiff64(tiff, entry + 4, 1, bigEndian);
                WriteTiff64(tiff, entry + 12, varyingPageSizes && page > 0 ? 160 : 320, bigEndian);
                entry += 20;
                WriteTiff16(tiff, entry, 257, bigEndian);
                WriteTiff16(tiff, entry + 2, 16, bigEndian);
                WriteTiff64(tiff, entry + 4, 1, bigEndian);
                WriteTiff64(tiff, entry + 12, varyingPageSizes && page > 0 ? 120 : 240, bigEndian);
                entry += 20;
                WriteTiff16(tiff, entry, 258, bigEndian);
                WriteTiff16(tiff, entry + 2, 3, bigEndian);
                WriteTiff64(tiff, entry + 4, 3, bigEndian);
                int bitsPerSample = varyingPixelFormats && page > 0 ? 16 : 8;
                WriteTiff16(tiff, entry + 12, bitsPerSample, bigEndian);
                WriteTiff16(tiff, entry + 14, bitsPerSample, bigEndian);
                WriteTiff16(tiff, entry + 16, bitsPerSample, bigEndian);
                entry += 20;
                WriteTiff16(tiff, entry, 277, bigEndian);
                WriteTiff16(tiff, entry + 2, 3, bigEndian);
                WriteTiff64(tiff, entry + 4, 1, bigEndian);
                WriteTiff16(tiff, entry + 12, varyingPixelFormats && page > 0 ? 4 : 3, bigEndian);
                WriteTiff64(tiff, offset + 8 + entriesPerIfd * 20, page + 1 < pageCount ? offset + ifdSize : 0, bigEndian);
            }
            return tiff;
        }

        private static void WriteTiff16(byte[] destination, int offset, int value, bool bigEndian)
        {
            if (bigEndian)
            {
                destination[offset] = (byte)(value >> 8);
                destination[offset + 1] = (byte)value;
            }
            else
            {
                destination[offset] = (byte)value;
                destination[offset + 1] = (byte)(value >> 8);
            }
        }

        private static void WriteTiff32(byte[] destination, int offset, int value, bool bigEndian)
        {
            if (bigEndian)
            {
                destination[offset] = (byte)(value >> 24);
                destination[offset + 1] = (byte)(value >> 16);
                destination[offset + 2] = (byte)(value >> 8);
                destination[offset + 3] = (byte)value;
            }
            else
            {
                destination[offset] = (byte)value;
                destination[offset + 1] = (byte)(value >> 8);
                destination[offset + 2] = (byte)(value >> 16);
                destination[offset + 3] = (byte)(value >> 24);
            }
        }

        private static void WriteTiff64(byte[] destination, int offset, long signedValue, bool bigEndian)
        {
            ulong value = (ulong)signedValue;
            if (bigEndian)
            {
                for (int index = 0; index < 8; ++index)
                {
                    destination[offset + index] = (byte)(value >> (56 - index * 8));
                }
            }
            else
            {
                for (int index = 0; index < 8; ++index)
                {
                    destination[offset + index] = (byte)(value >> (index * 8));
                }
            }
        }

        private static byte[] CreateApng(int frameCount)
        {
            byte[] header = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            byte[] ihdr = new byte[] { 0, 0, 0, 2, 0, 0, 0, 3, 8, 2, 0, 0, 0 };
            byte[] png = new byte[header.Length + 12 + ihdr.Length + 20 + 38 + 13 + (frameCount - 1) * (38 + 17) + 12];
            Array.Copy(header, png, header.Length);
            int offset = header.Length;
            offset = WritePngChunk(png, offset, "IHDR", ihdr);
            offset = WritePngChunk(png, offset, "acTL", new byte[] { 0, 0, 0, (byte)frameCount, 0, 0, 0, 0 });
            offset = WritePngChunk(png, offset, "fcTL", CreateApngFrameControl(0));
            offset = WritePngChunk(png, offset, "IDAT", new byte[] { 1 });
            for (int frame = 1; frame < frameCount; ++frame)
            {
                offset = WritePngChunk(png, offset, "fcTL", CreateApngFrameControl(frame * 2 - 1));
                offset = WritePngChunk(png, offset, "fdAT", new byte[] { 0, 0, 0, (byte)(frame * 2), 1 });
            }
            offset = WritePngChunk(png, offset, "IEND", Array.Empty<byte>());
            Assert.Equal(png.Length, offset);
            return png;
        }

        private static byte[] CreateApngFrameControl(int sequence)
        {
            byte[] control = new byte[26];
            control[3] = (byte)sequence;
            control[7] = 2;
            control[11] = 3;
            return control;
        }

        private static byte[] CreateAnimatedGif(int frameCount, int globalDepth = 1)
        {
            int globalColorTableBytes = 3 * (1 << globalDepth);
            byte[] gif = new byte[6 + 7 + globalColorTableBytes + 15 + frameCount * 22 + 1];
            gif[0] = (byte)'G'; gif[1] = (byte)'I'; gif[2] = (byte)'F'; gif[3] = (byte)'8'; gif[4] = (byte)'9'; gif[5] = (byte)'a';
            gif[6] = 2; gif[8] = 3; gif[10] = (byte)(0x80 | (globalDepth - 1));
            int offset = 13 + globalColorTableBytes;
            gif[offset++] = 0x21; gif[offset++] = 0xFF; gif[offset++] = 11;
            byte[] application = new byte[] { (byte)'N', (byte)'E', (byte)'T', (byte)'S', (byte)'C', (byte)'A', (byte)'P', (byte)'E', (byte)'2', (byte)'.', (byte)'0' };
            Array.Copy(application, 0, gif, offset, application.Length);
            offset += application.Length;
            gif[offset++] = 0;
            for (int frame = 0; frame < frameCount; ++frame)
            {
                gif[offset++] = 0x21; gif[offset++] = 0xF9; gif[offset++] = 4;
                gif[offset++] = 0; gif[offset++] = 10; gif[offset++] = 0; gif[offset++] = 0; gif[offset++] = 0;
                gif[offset++] = 0x2C;
                offset += 4;
                gif[offset++] = 2;
                gif[offset++] = 0;
                gif[offset++] = 3;
                gif[offset++] = 0;
                gif[offset++] = 0;
                gif[offset++] = 2;
                gif[offset++] = 1;
                gif[offset++] = 0;
                gif[offset++] = 0;
            }
            gif[offset++] = 0x3B;
            Assert.Equal(gif.Length, offset);
            return gif;
        }

        private static byte[] CreateAnimatedWebp(int frameCount, bool alpha = false)
        {
            byte[] webp = new byte[12 + 18 + 14 + frameCount * 24];
            webp[0] = (byte)'R'; webp[1] = (byte)'I'; webp[2] = (byte)'F'; webp[3] = (byte)'F';
            int declaredLength = webp.Length - 8;
            webp[4] = (byte)declaredLength;
            webp[8] = (byte)'W'; webp[9] = (byte)'E'; webp[10] = (byte)'B'; webp[11] = (byte)'P';
            int offset = 12;
            offset = WriteWebpChunk(webp, offset, "VP8X", new byte[] { (byte)(0x02 | (alpha ? 0x10 : 0)), 0, 0, 0, 1, 0, 0, 2, 0, 0 });
            offset = WriteWebpChunk(webp, offset, "ANIM", new byte[6]);
            for (int frame = 0; frame < frameCount; ++frame)
            {
                offset = WriteWebpChunk(webp, offset, "ANMF", new byte[16]);
            }
            Assert.Equal(webp.Length, offset);
            return webp;
        }

        private static byte[] CreateStaticWebp(string imageType, byte[] payload)
        {
            byte[] webp = new byte[12 + 8 + payload.Length + (payload.Length & 1)];
            webp[0] = (byte)'R'; webp[1] = (byte)'I'; webp[2] = (byte)'F'; webp[3] = (byte)'F';
            int declaredLength = webp.Length - 8;
            webp[4] = (byte)declaredLength;
            webp[5] = (byte)(declaredLength >> 8);
            webp[6] = (byte)(declaredLength >> 16);
            webp[7] = (byte)(declaredLength >> 24);
            webp[8] = (byte)'W'; webp[9] = (byte)'E'; webp[10] = (byte)'B'; webp[11] = (byte)'P';
            WriteWebpChunk(webp, 12, imageType, payload);
            return webp;
        }

        private static byte[] CreateBmpFixture(int bitCount, int compression)
        {
            const int width = 2;
            const int height = 3;
            const int pixelOffset = 54;
            int rowStride = ((width * bitCount + 31) / 32) * 4;
            int fileSize = pixelOffset + rowStride * height;
            byte[] bmp = new byte[fileSize];
            bmp[0] = (byte)'B';
            bmp[1] = (byte)'M';
            WriteBmp32(bmp, 2, fileSize);
            WriteBmp32(bmp, 10, pixelOffset);
            WriteBmp32(bmp, 14, 40);
            WriteBmp32(bmp, 18, width);
            WriteBmp32(bmp, 22, height);
            WriteBmp16(bmp, 26, 1);
            WriteBmp16(bmp, 28, bitCount);
            WriteBmp32(bmp, 30, compression);
            return bmp;
        }

        private static byte[] CreateBmpCoreFixture(int bitCount)
        {
            const int width = 2;
            const int height = 3;
            const int pixelOffset = 26;
            int rowStride = ((width * bitCount + 31) / 32) * 4;
            int fileSize = pixelOffset + rowStride * height;
            byte[] bmp = new byte[fileSize];
            bmp[0] = (byte)'B';
            bmp[1] = (byte)'M';
            WriteBmp32(bmp, 2, fileSize);
            WriteBmp32(bmp, 10, pixelOffset);
            WriteBmp32(bmp, 14, 12);
            WriteBmp16(bmp, 18, width);
            WriteBmp16(bmp, 20, height);
            WriteBmp16(bmp, 22, 1);
            WriteBmp16(bmp, 24, bitCount);
            return bmp;
        }

        private static void WriteBmp16(byte[] destination, int offset, int value)
        {
            destination[offset] = (byte)value;
            destination[offset + 1] = (byte)(value >> 8);
        }

        private static void WriteBmp32(byte[] destination, int offset, int value)
        {
            destination[offset] = (byte)value;
            destination[offset + 1] = (byte)(value >> 8);
            destination[offset + 2] = (byte)(value >> 16);
            destination[offset + 3] = (byte)(value >> 24);
        }

        private static byte[] CreateSunRaster(int depth)
        {
            const int width = 2;
            const int height = 3;
            int rowStride = ((width * depth + 15) / 16) * 2;
            int payloadLength = rowStride * height;
            byte[] raster = new byte[32 + payloadLength];
            WriteBe32(raster, 0, 0x59A66A95);
            WriteBe32(raster, 4, (uint)width);
            WriteBe32(raster, 8, (uint)height);
            WriteBe32(raster, 12, (uint)depth);
            WriteBe32(raster, 16, (uint)payloadLength);
            WriteBe32(raster, 20, 1);
            return raster;
        }

        private static void WriteBe32(byte[] destination, int offset, uint value)
        {
            destination[offset] = (byte)(value >> 24);
            destination[offset + 1] = (byte)(value >> 16);
            destination[offset + 2] = (byte)(value >> 8);
            destination[offset + 3] = (byte)value;
        }

        private static byte[] CreateRadianceHdr(int width, int height, bool rle, string signature = "#?RADIANCE")
        {
            byte[] header = Encoding.ASCII.GetBytes(
                signature + "\nFORMAT=32-bit_rle_rgbe\n\n-Y " + height + " +X " + width + "\n");
            using (var stream = new MemoryStream())
            {
                stream.Write(header, 0, header.Length);
                if (!rle)
                {
                    stream.Write(new byte[checked(width * height * 4)], 0, checked(width * height * 4));
                }
                else
                {
                    Assert.InRange(width, 8, 127);
                    for (int row = 0; row < height; ++row)
                    {
                        stream.WriteByte(2);
                        stream.WriteByte(2);
                        stream.WriteByte((byte)(width >> 8));
                        stream.WriteByte((byte)width);
                        for (int channel = 0; channel < 4; ++channel)
                        {
                            stream.WriteByte((byte)(128 + width));
                            stream.WriteByte((byte)(row + channel));
                        }
                    }
                }
                return stream.ToArray();
            }
        }

        private static int GetRadianceHdrHeaderLength(int width, int height)
        {
            return Encoding.ASCII.GetByteCount(
                "#?RADIANCE\nFORMAT=32-bit_rle_rgbe\n\n-Y " + height + " +X " + width + "\n");
        }

        private static byte[] CreateOpenExrHeader(int width, int height, int channelCount, int pixelType, int secondPixelType = -1)
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x76); stream.WriteByte(0x2F); stream.WriteByte(0x31); stream.WriteByte(0x01);
                WriteLe32(stream, 2);
                WriteOpenExrAttribute(stream, "dataWindow", "box2i", new byte[]
                {
                    0, 0, 0, 0,
                    0, 0, 0, 0,
                    (byte)((width - 1) & 0xFF), (byte)((width - 1) >> 8), (byte)((width - 1) >> 16), (byte)((width - 1) >> 24),
                    (byte)((height - 1) & 0xFF), (byte)((height - 1) >> 8), (byte)((height - 1) >> 16), (byte)((height - 1) >> 24),
                });

                using (var channels = new MemoryStream())
                {
                    for (int channel = 0; channel < channelCount; ++channel)
                    {
                        string name = channel == 0 ? "R" : channel == 1 ? "G" : channel == 2 ? "B" : "A";
                        byte[] nameBytes = Encoding.ASCII.GetBytes(name);
                        channels.Write(nameBytes, 0, nameBytes.Length);
                        channels.WriteByte(0);
                        WriteLe32(channels, channel == 1 && secondPixelType >= 0 ? secondPixelType : pixelType);
                        channels.WriteByte(0);
                        channels.WriteByte(0); channels.WriteByte(0); channels.WriteByte(0);
                        WriteLe32(channels, 1);
                        WriteLe32(channels, 1);
                    }
                    channels.WriteByte(0);
                    WriteOpenExrAttribute(stream, "channels", "chlist", channels.ToArray());
                }
                stream.WriteByte(0);
                return stream.ToArray();
            }
        }

        private static void WriteOpenExrAttribute(Stream stream, string name, string type, byte[] value)
        {
            byte[] nameBytes = Encoding.ASCII.GetBytes(name);
            byte[] typeBytes = Encoding.ASCII.GetBytes(type);
            stream.Write(nameBytes, 0, nameBytes.Length); stream.WriteByte(0);
            stream.Write(typeBytes, 0, typeBytes.Length); stream.WriteByte(0);
            WriteLe32(stream, value.Length);
            stream.Write(value, 0, value.Length);
        }

        private static int FindOpenExrAttributeValue(byte[] data, string attributeName)
        {
            int offset = 8;
            while (offset < data.Length && data[offset] != 0)
            {
                int nameStart = offset;
                while (offset < data.Length && data[offset] != 0) ++offset;
                string name = Encoding.ASCII.GetString(data, nameStart, offset - nameStart);
                ++offset;
                while (offset < data.Length && data[offset] != 0) ++offset;
                ++offset;
                int size = ReadLe32ForTest(data, offset);
                offset += 4;
                if (name == attributeName) return offset;
                offset += size;
            }
            throw new InvalidOperationException("EXR attribute not found: " + attributeName);
        }

        private static void WriteLe32(Stream stream, int value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
        }

        private static void WriteLe32(byte[] destination, int offset, int value)
        {
            destination[offset] = (byte)value;
            destination[offset + 1] = (byte)(value >> 8);
            destination[offset + 2] = (byte)(value >> 16);
            destination[offset + 3] = (byte)(value >> 24);
        }

        private static int ReadLe32ForTest(byte[] data, int offset)
        {
            return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
        }

        private static int WritePngChunk(byte[] destination, int offset, string type, byte[] payload)
        {
            destination[offset + 3] = (byte)payload.Length;
            destination[offset + 4] = (byte)type[0];
            destination[offset + 5] = (byte)type[1];
            destination[offset + 6] = (byte)type[2];
            destination[offset + 7] = (byte)type[3];
            Array.Copy(payload, 0, destination, offset + 8, payload.Length);
            return offset + 12 + payload.Length;
        }

        private static byte[] CreateJpegWithMetadataAndIccProfile()
        {
            return new byte[]
            {
                0xFF, 0xD8,
                0xFF, 0xE1, 0x00, 0x05, 1, 2, 3,
                0xFF, 0xE2, 0x00, 0x12,
                (byte)'I', (byte)'C', (byte)'C', (byte)'_', (byte)'P', (byte)'R', (byte)'O', (byte)'F',
                (byte)'I', (byte)'L', (byte)'E', 0, 1, 1, 7, 8,
                0xFF, 0xD9
            };
        }

        private static byte[] CreateWebpWithMetadataAndIccProfile()
        {
            byte[] webp = new byte[34];
            webp[0] = (byte)'R'; webp[1] = (byte)'I'; webp[2] = (byte)'F'; webp[3] = (byte)'F';
            webp[4] = 26;
            webp[8] = (byte)'W'; webp[9] = (byte)'E'; webp[10] = (byte)'B'; webp[11] = (byte)'P';
            int offset = 12;
            offset = WriteWebpChunk(webp, offset, "EXIF", new byte[] { 1, 2 });
            offset = WriteWebpChunk(webp, offset, "ICCP", new byte[] { 3, 4, 5 });
            Assert.Equal(webp.Length, offset);
            return webp;
        }

        private static int WriteWebpChunk(byte[] destination, int offset, string type, byte[] payload)
        {
            destination[offset] = (byte)type[0];
            destination[offset + 1] = (byte)type[1];
            destination[offset + 2] = (byte)type[2];
            destination[offset + 3] = (byte)type[3];
            destination[offset + 4] = (byte)payload.Length;
            Array.Copy(payload, 0, destination, offset + 8, payload.Length);
            return offset + 8 + payload.Length + (payload.Length & 1);
        }

        private sealed class NonSeekableReadStream : Stream
        {
            private readonly byte[] data;
            private readonly int maxRead;
            private int position;

            public NonSeekableReadStream(byte[] data, int maxRead)
            {
                this.data = data;
                this.maxRead = maxRead;
            }

            public int BytesRead { get { return position; } }
            public int ReadCalls { get; private set; }
            public override bool CanRead { get { return true; } }
            public override bool CanSeek { get { return false; } }
            public override bool CanWrite { get { return false; } }
            public override long Length { get { throw new NotSupportedException(); } }
            public override long Position
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                ++ReadCalls;
                int available = data.Length - position;
                if (available <= 0) return 0;
                int read = Math.Min(Math.Min(count, maxRead), available);
                Array.Copy(data, position, buffer, offset, read);
                position += read;
                return read;
            }

            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
            public override void SetLength(long value) { throw new NotSupportedException(); }
            public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
        }
    }
}
