using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ObjDetect;

namespace JYPPX.OpenCvSharp.Tests.ObjDetect
{
    public sealed class ObjDetectTests
    {
        [Fact]
        public void EnumValuesMatchOpenCvConstants()
        {
            Assert.Equal(20, (int)QRCodeEncoderECIEncodings.ShiftJis);
            Assert.Equal(26, (int)QRCodeEncoderECIEncodings.Utf8);

            Assert.Equal(0, (int)DnnBackend.Default);
            Assert.Equal(2, (int)DnnBackend.InferenceEngine);
            Assert.Equal(3, (int)DnnBackend.OpenCV);
            Assert.Equal(4, (int)DnnBackend.VkCom);
            Assert.Equal(5, (int)DnnBackend.Cuda);
            Assert.Equal(8, (int)DnnBackend.Cann);

            Assert.Equal(0, (int)DnnTarget.Cpu);
            Assert.Equal(1, (int)DnnTarget.OpenCL);
            Assert.Equal(2, (int)DnnTarget.OpenCLFp16);
            Assert.Equal(6, (int)DnnTarget.Cuda);
            Assert.Equal(10, (int)DnnTarget.CpuFp16);

            Assert.Equal(0, (int)FaceRecognizerSFDistanceType.Cosine);
            Assert.Equal(1, (int)FaceRecognizerSFDistanceType.NormL2);
        }

        [Fact]
        public void FaceDetectionStoresOpenCvFaceDetectorRowFields()
        {
            var detection = new FaceDetection(
                new Rect(1, 2, 3, 4),
                new Point2f(5, 6),
                new Point2f(7, 8),
                new Point2f(9, 10),
                new Point2f(11, 12),
                new Point2f(13, 14),
                0.95F);

            Assert.Equal(new Rect(1, 2, 3, 4), detection.Bounds);
            Assert.Equal(new Point2f(5, 6), detection.RightEye);
            Assert.Equal(new Point2f(7, 8), detection.LeftEye);
            Assert.Equal(new Point2f(9, 10), detection.NoseTip);
            Assert.Equal(new Point2f(11, 12), detection.RightMouthCorner);
            Assert.Equal(new Point2f(13, 14), detection.LeftMouthCorner);
            Assert.Equal(0.95F, detection.Score, 5);
            Assert.Equal(
                new FaceDetection(
                    new Rect(1, 2, 3, 4),
                    new Point2f(5, 6),
                    new Point2f(7, 8),
                    new Point2f(9, 10),
                    new Point2f(11, 12),
                    new Point2f(13, 14),
                    0.95F),
                detection);
            Assert.True(detection == new FaceDetection(
                new Rect(1, 2, 3, 4),
                new Point2f(5, 6),
                new Point2f(7, 8),
                new Point2f(9, 10),
                new Point2f(11, 12),
                new Point2f(13, 14),
                0.95F));
            Assert.True(detection != new FaceDetection(
                new Rect(1, 2, 3, 4),
                new Point2f(5, 6),
                new Point2f(7, 8),
                new Point2f(9, 10),
                new Point2f(11, 12),
                new Point2f(13, 14),
                0.5F));
            Assert.False(detection.Equals("not a detection"));
            Assert.Equal(
                new FaceDetection(
                    new Rect(1, 2, 3, 4),
                    new Point2f(5, 6),
                    new Point2f(7, 8),
                    new Point2f(9, 10),
                    new Point2f(11, 12),
                    new Point2f(13, 14),
                    0.95F).GetHashCode(),
                detection.GetHashCode());
            Assert.Equal(
                "{Bounds={X=1,Y=2,Width=3,Height=4},RightEye={X=5,Y=6},LeftEye={X=7,Y=8},NoseTip={X=9,Y=10},RightMouthCorner={X=11,Y=12},LeftMouthCorner={X=13,Y=14},Score=0.95}",
                detection.ToString());
        }

        [Fact]
        public void FaceDetectionFormatsInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                var detection = new FaceDetection(
                    new Rect(1, 2, 3, 4),
                    new Point2f(5.5F, 6.25F),
                    new Point2f(7.5F, 8.25F),
                    new Point2f(9.5F, 10.25F),
                    new Point2f(11.5F, 12.25F),
                    new Point2f(13.5F, 14.25F),
                    0.95F);

                Assert.Equal(
                    "{Bounds={X=1,Y=2,Width=3,Height=4},RightEye={X=5.5,Y=6.25},LeftEye={X=7.5,Y=8.25},NoseTip={X=9.5,Y=10.25},RightMouthCorner={X=11.5,Y=12.25},LeftMouthCorner={X=13.5,Y=14.25},Score=0.95}",
                    detection.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void FaceDetectionHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(60, Marshal.SizeOf<FaceDetection>());

            Assert.Equal(0, FieldOffset<FaceDetection>("<Bounds>k__BackingField"));
            Assert.Equal(16, FieldOffset<FaceDetection>("<RightEye>k__BackingField"));
            Assert.Equal(24, FieldOffset<FaceDetection>("<LeftEye>k__BackingField"));
            Assert.Equal(32, FieldOffset<FaceDetection>("<NoseTip>k__BackingField"));
            Assert.Equal(40, FieldOffset<FaceDetection>("<RightMouthCorner>k__BackingField"));
            Assert.Equal(48, FieldOffset<FaceDetection>("<LeftMouthCorner>k__BackingField"));
            Assert.Equal(56, FieldOffset<FaceDetection>("<Score>k__BackingField"));
        }

        [Fact]
        public void QRCodeMultiDecodeResultStoresDecodedInfoAndPoints()
        {
            using (var points = new Mat(2, 4, MatType.CV_32FC2))
            {
                var decodedInfo = new[] { "alpha", string.Empty, "beta" };
                var result = new QRCodeMultiDecodeResult(true, decodedInfo, points);
                decodedInfo[0] = "mutated";

                Assert.True(result.Success);
                Assert.Equal(3, result.DecodedInfoCount);
                Assert.Equal(new[] { "alpha", string.Empty, "beta" }, result.DecodedInfo);

                string[] returnedDecodedInfo = result.DecodedInfo;
                returnedDecodedInfo[0] = "returned-mutated";

                Assert.Equal(new[] { "alpha", string.Empty, "beta" }, result.DecodedInfo);
                Assert.Same(points, result.Points);
                Assert.True(result.HasPoints);
                Assert.Equal("QRCodeMultiDecodeResult(Success=True, DecodedInfo=3, Points=2x4)", result.ToString());
            }
        }

        [Fact]
        public void QRCodeMultiDecodeResultNormalizesNullDecodedInfo()
        {
            var result = new QRCodeMultiDecodeResult(false, null!, null);

            Assert.False(result.Success);
            Assert.Equal(0, result.DecodedInfoCount);
            Assert.Empty(result.DecodedInfo);
            Assert.Null(result.Points);
            Assert.False(result.HasPoints);
            Assert.Equal("QRCodeMultiDecodeResult(Success=False, DecodedInfo=0, Points=<null>)", result.ToString());
        }

        [Fact]
        public void QRCodeMultiDecodeResultRejectsNullDecodedInfoElements()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new QRCodeMultiDecodeResult(true, new[] { "alpha", null! }, null));
        }

        [Fact]
        public void FaceDetectorCreateRejectsNullModelBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() =>
                FaceDetectorYN.Create(null!, string.Empty, new Size(320, 320)));
        }

        [Fact]
        public void FaceDetectorCreateRejectsNullModelBufferBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() =>
                FaceDetectorYN.Create("onnx", (byte[])null!, null, new Size(320, 320)));
            Assert.Throws<ArgumentException>(() =>
                FaceDetectorYN.Create("onnx", Array.Empty<byte>(), null, new Size(320, 320)));
#if NETCOREAPP3_1_OR_GREATER
            Assert.Throws<ArgumentException>(() => CreateFaceDetectorFromEmptySpan());
#endif
        }

        [Fact]
        public void FaceRecognizerCreateRejectsNullModelBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() =>
                FaceRecognizerSF.Create(null!, string.Empty));
        }

        [Fact]
        public void FaceRecognizerCreateRejectsNullModelBufferBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() =>
                FaceRecognizerSF.Create("onnx", (byte[])null!, null));
            Assert.Throws<ArgumentException>(() =>
                FaceRecognizerSF.Create("onnx", Array.Empty<byte>(), null));
#if NETCOREAPP3_1_OR_GREATER
            Assert.Throws<ArgumentException>(() => CreateFaceRecognizerFromEmptySpan());
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void CreateFaceDetectorFromEmptySpan()
        {
            FaceDetectorYN.Create("onnx", ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty, new Size(320, 320));
        }

        private static void CreateFaceRecognizerFromEmptySpan()
        {
            FaceRecognizerSF.Create("onnx", ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty);
        }
#endif

        [Fact]
        public void QRCodeDetectorCanBeCreatedWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var detector = QRCodeDetector.Create())
            {
                Assert.False(detector.IsDisposed);
                Assert.Same(detector, detector.SetEpsX(0.2));
                Assert.Same(detector, detector.SetEpsY(0.2));
                Assert.Same(detector, detector.SetUseAlignmentMarkers(true));
            }
        }

        [Fact]
        public void QRCodeDetectorValidatesMatArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var detector = QRCodeDetector.Create())
            using (var image = new Mat())
            using (var points = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => detector.Detect(null!, points));
                Assert.Throws<ArgumentNullException>(() => detector.Detect(image, null!));
                Assert.Throws<ArgumentNullException>(() => detector.Decode(null!, points));
                Assert.Throws<ArgumentNullException>(() => detector.Decode(image, null!));
                Assert.Throws<ArgumentNullException>(() => detector.DetectAndDecode(null!));
                Assert.Throws<ArgumentNullException>(() => detector.DetectMulti(null!, points));
                Assert.Throws<ArgumentNullException>(() => detector.DetectMulti(image, null!));
                Assert.Throws<ArgumentNullException>(() => detector.DecodeMulti(null!, points));
                Assert.Throws<ArgumentNullException>(() => detector.DecodeMulti(image, null!));
                Assert.Throws<ArgumentNullException>(() => detector.DetectAndDecodeMulti(null!));

                detector.Dispose();

                Assert.True(detector.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => detector.SetEpsX(0.2));
                Assert.Throws<ObjectDisposedException>(() => detector.SetEpsY(0.2));
                Assert.Throws<ObjectDisposedException>(() => detector.SetUseAlignmentMarkers(true));
                Assert.Throws<ObjectDisposedException>(() => detector.Detect(image, points));
                Assert.Throws<ObjectDisposedException>(() => detector.Decode(image, points));
                Assert.Throws<ObjectDisposedException>(() => detector.DetectAndDecode(image));
                Assert.Throws<ObjectDisposedException>(() => detector.DecodeCurved(image, points));
                Assert.Throws<ObjectDisposedException>(() => detector.DetectAndDecodeCurved(image));
                Assert.Throws<ObjectDisposedException>(() => detector.DetectMulti(image, points));
                Assert.Throws<ObjectDisposedException>(() => detector.DecodeMulti(image, points));
                Assert.Throws<ObjectDisposedException>(() => detector.DetectAndDecodeMulti(image));
                Assert.Throws<ObjectDisposedException>(() => detector.GetEncoding());
            }
        }

        [Fact]
        public void QRCodeDetectorArucoValidatesMatArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var detector = QRCodeDetectorAruco.Create())
            using (var image = new Mat())
            using (var points = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => detector.Detect(null!, points));
                Assert.Throws<ArgumentNullException>(() => detector.Detect(image, null!));
                Assert.Throws<ArgumentNullException>(() => detector.Decode(null!, points));
                Assert.Throws<ArgumentNullException>(() => detector.Decode(image, null!));
                Assert.Throws<ArgumentNullException>(() => detector.DetectAndDecode(null!));
                Assert.Throws<ArgumentNullException>(() => detector.DetectMulti(null!, points));
                Assert.Throws<ArgumentNullException>(() => detector.DetectMulti(image, null!));
                Assert.Throws<ArgumentNullException>(() => detector.DecodeMulti(null!, points));
                Assert.Throws<ArgumentNullException>(() => detector.DecodeMulti(image, null!));
                Assert.Throws<ArgumentNullException>(() => detector.DetectAndDecodeMulti(null!));

                detector.Dispose();

                Assert.True(detector.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => detector.GetDetectorParameters());
                Assert.Throws<ObjectDisposedException>(() => detector.SetDetectorParameters(QRCodeDetectorArucoParams.Default));
                Assert.Throws<ObjectDisposedException>(() => detector.Detect(image, points));
                Assert.Throws<ObjectDisposedException>(() => detector.Decode(image, points));
                Assert.Throws<ObjectDisposedException>(() => detector.DetectAndDecode(image));
                Assert.Throws<ObjectDisposedException>(() => detector.DetectMulti(image, points));
                Assert.Throws<ObjectDisposedException>(() => detector.DecodeMulti(image, points));
                Assert.Throws<ObjectDisposedException>(() => detector.DetectAndDecodeMulti(image));
            }
        }

        [Fact]
        public void QRCodeDetectorArucoParametersRoundTripWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            var parameters = new QRCodeDetectorArucoParams(
                3.5F,
                0.45F,
                1.25F,
                1.75F,
                0.35F,
                0.15F,
                0.85F);

            using (var detector = QRCodeDetectorAruco.Create(parameters))
            {
                Assert.Equal(parameters, detector.GetDetectorParameters());
                Assert.Same(detector, detector.SetDetectorParameters(QRCodeDetectorArucoParams.Default));

                QRCodeDetectorArucoParams updated = detector.GetDetectorParameters();
                Assert.Equal(QRCodeDetectorArucoParams.Default.MinModuleSizeInPyramid, updated.MinModuleSizeInPyramid);
                Assert.Equal(QRCodeDetectorArucoParams.Default.MaxRotation, updated.MaxRotation);
                Assert.Equal(QRCodeDetectorArucoParams.Default.MaxModuleSizeMismatch, updated.MaxModuleSizeMismatch);
                Assert.Equal(QRCodeDetectorArucoParams.Default.MaxTimingPatternMismatch, updated.MaxTimingPatternMismatch);
                Assert.Equal(QRCodeDetectorArucoParams.Default.MaxPenalties, updated.MaxPenalties);
                Assert.Equal(QRCodeDetectorArucoParams.Default.MaxColorsMismatch, updated.MaxColorsMismatch);
                Assert.Equal(QRCodeDetectorArucoParams.Default.ScaleTimingPatternScore, updated.ScaleTimingPatternScore);
            }
        }

        [Fact]
        public void QRCodeEncoderValidatesArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var encoder = QRCodeEncoder.Create())
            using (var qrcode = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => encoder.Encode(null!));
                Assert.Throws<ArgumentNullException>(() => encoder.Encode(null!, qrcode));
                Assert.Throws<ArgumentNullException>(() => encoder.Encode("opencv", null!));
                Assert.Throws<ArgumentNullException>(() => encoder.EncodeStructuredAppend(null!));

                encoder.Dispose();

                Assert.True(encoder.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => encoder.Encode("opencv"));
                Assert.Throws<ObjectDisposedException>(() => encoder.Encode("opencv", qrcode));
                Assert.Throws<ObjectDisposedException>(() => encoder.EncodeStructuredAppend("opencv"));
            }
        }

        [Fact]
        public void QRCodeEncoderCustomParametersEncodeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            var parameters = new QRCodeEncoderParams(
                version: 4,
                correctionLevel: QRCodeEncoderCorrectionLevel.M,
                mode: QRCodeEncoderEncodeMode.Byte,
                structureNumber: 1);

            using (QRCodeEncoder encoder = QRCodeEncoder.Create(parameters))
            using (Mat qrcode = encoder.Encode("opencv"))
            {
                Assert.False(encoder.IsDisposed);
                Assert.False(qrcode.Empty);
                Assert.True(qrcode.Rows > 0);
                Assert.True(qrcode.Cols > 0);
                Assert.Equal(1, qrcode.Channels);
            }
        }

        [Fact]
        public void BarcodeDetectorValidatesArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Assert.Throws<ArgumentNullException>(() => BarcodeDetector.Create(null!));

            using (var detector = BarcodeDetector.Create())
            using (var image = new Mat())
            using (var points = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => detector.Detect(null!, points));
                Assert.Throws<ArgumentNullException>(() => detector.Detect(image, null!));
                Assert.Throws<ArgumentNullException>(() => detector.Decode(null!, points));
                Assert.Throws<ArgumentNullException>(() => detector.Decode(image, null!));
                Assert.Throws<ArgumentNullException>(() => detector.DecodeWithType(null!, points));
                Assert.Throws<ArgumentNullException>(() => detector.DecodeWithType(image, null!));
                Assert.Throws<ArgumentNullException>(() => detector.DetectAndDecode(null!));
                Assert.Throws<ArgumentNullException>(() => detector.DetectAndDecodeWithType(null!));
                Assert.Throws<ArgumentNullException>(() => detector.SetDetectorScales((float[])null!));

                detector.Dispose();

                Assert.True(detector.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => detector.DownsamplingThreshold);
                Assert.Throws<ObjectDisposedException>(() => detector.DownsamplingThreshold = 0.5);
                Assert.Throws<ObjectDisposedException>(() => detector.GradientThreshold);
                Assert.Throws<ObjectDisposedException>(() => detector.GradientThreshold = 0.5);
                Assert.Throws<ObjectDisposedException>(() => detector.Detect(image, points));
                Assert.Throws<ObjectDisposedException>(() => detector.Decode(image, points));
                Assert.Throws<ObjectDisposedException>(() => detector.DecodeWithType(image, points));
                Assert.Throws<ObjectDisposedException>(() => detector.DetectAndDecode(image));
                Assert.Throws<ObjectDisposedException>(() => detector.DetectAndDecodeWithType(image));
                Assert.Throws<ObjectDisposedException>(() => detector.GetDetectorScales());
                Assert.Throws<ObjectDisposedException>(() => detector.SetDetectorScales(Array.Empty<float>()));
            }
        }

        [Fact]
        public void ArucoDictionaryValidatesArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Assert.Throws<ArgumentNullException>(() => new ArucoDictionary(null!, 4));
            Assert.Throws<ArgumentNullException>(() => ArucoDictionary.GetByteListFromBits(null!));
            Assert.Throws<ArgumentNullException>(() => ArucoDictionary.GetBitsFromByteList(null!, 4));

            using (var dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50))
            using (var bits = new Mat())
            using (var image = new Mat())
            using (var bytesList = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => dictionary.BytesList = null!);
                Assert.Throws<ArgumentNullException>(() => dictionary.Identify(null!, 0.5));
                Assert.Throws<ArgumentNullException>(() => dictionary.Identify(null!, 0.5, 0.0F));
                Assert.Throws<ArgumentNullException>(() => dictionary.GetDistanceToId(null!, 0));
                Assert.Throws<ArgumentNullException>(() => dictionary.GenerateImageMarker(0, 32, null!));

                dictionary.Dispose();

                Assert.True(dictionary.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => dictionary.BytesList);
                Assert.Throws<ObjectDisposedException>(() => dictionary.BytesList = bytesList);
                Assert.Throws<ObjectDisposedException>(() => dictionary.MarkerSize);
                Assert.Throws<ObjectDisposedException>(() => dictionary.MarkerSize = 4);
                Assert.Throws<ObjectDisposedException>(() => dictionary.MaxCorrectionBits);
                Assert.Throws<ObjectDisposedException>(() => dictionary.MaxCorrectionBits = 1);
                Assert.Throws<ObjectDisposedException>(() => dictionary.Identify(bits, 0.5));
                Assert.Throws<ObjectDisposedException>(() => dictionary.Identify(bits, 0.5, 0.0F));
                Assert.Throws<ObjectDisposedException>(() => dictionary.GetDistanceToId(bits, 0));
                Assert.Throws<ObjectDisposedException>(() => dictionary.GenerateImageMarker(0, 32, image));
                Assert.Throws<ObjectDisposedException>(() => dictionary.GenerateImageMarker(0, 32));
                Assert.Throws<ObjectDisposedException>(() => dictionary.GetMarkerBits(0));
            }
        }

        [Fact]
        public void FaceDetectorValidatesMatArgumentsWhenNativeRuntimeAndModelAreAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string? modelPath = TestEnvironment.GetFaceDetectorModelVariable();
            if (string.IsNullOrWhiteSpace(modelPath))
            {
                return;
            }

            using (FaceDetectorYN detector = FaceDetectorYN.Create(modelPath, string.Empty, new Size(320, 320)))
            using (var image = new Mat())
            using (var faces = new Mat())
            {
                Assert.False(detector.IsDisposed);
                Assert.Throws<ArgumentNullException>(() => detector.Detect(null!, faces));
                Assert.Throws<ArgumentNullException>(() => detector.Detect(image, null!));
            }
        }

        [Fact]
        public void FaceRecognizerValidatesMatArgumentsWhenNativeRuntimeAndModelAreAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string? modelPath = TestEnvironment.GetFaceRecognizerModelVariable();
            if (string.IsNullOrWhiteSpace(modelPath))
            {
                return;
            }

            using (FaceRecognizerSF recognizer = FaceRecognizerSF.Create(modelPath, string.Empty))
            using (var image = new Mat())
            using (var faces = new Mat())
            using (var feature = new Mat())
            {
                Assert.False(recognizer.IsDisposed);
                Assert.Throws<ArgumentNullException>(() => recognizer.AlignCrop(null!, faces, image));
                Assert.Throws<ArgumentNullException>(() => recognizer.AlignCrop(image, null!, image));
                Assert.Throws<ArgumentNullException>(() => recognizer.AlignCrop(image, faces, null!));
                Assert.Throws<ArgumentNullException>(() => recognizer.Feature(null!, feature));
                Assert.Throws<ArgumentNullException>(() => recognizer.Feature(image, null!));
                Assert.Throws<ArgumentNullException>(() => recognizer.Match(null!, feature));
                Assert.Throws<ArgumentNullException>(() => recognizer.Match(feature, null!));
            }
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }

    }
}
