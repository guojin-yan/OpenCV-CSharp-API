using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ObjDetect;

namespace JYPPX.OpenCvSharp.Tests.ObjDetect
{
    public sealed class ObjDetectSecondBatchTests
    {
        [Fact]
        public void EnumValuesMatchOpenCvObjDetectConstants()
        {
            Assert.Equal(0, (int)QRCodeEncoderCorrectionLevel.L);
            Assert.Equal(1, (int)QRCodeEncoderCorrectionLevel.M);
            Assert.Equal(2, (int)QRCodeEncoderCorrectionLevel.Q);
            Assert.Equal(3, (int)QRCodeEncoderCorrectionLevel.H);

            Assert.Equal(-1, (int)QRCodeEncoderEncodeMode.Auto);
            Assert.Equal(1, (int)QRCodeEncoderEncodeMode.Numeric);
            Assert.Equal(2, (int)QRCodeEncoderEncodeMode.Alphanumeric);
            Assert.Equal(3, (int)QRCodeEncoderEncodeMode.StructuredAppend);
            Assert.Equal(4, (int)QRCodeEncoderEncodeMode.Byte);
            Assert.Equal(7, (int)QRCodeEncoderEncodeMode.Eci);
            Assert.Equal(8, (int)QRCodeEncoderEncodeMode.Kanji);
        }

        [Fact]
        public void ParamsAndResultObjectsStoreValues()
        {
            var aruco = new QRCodeDetectorArucoParams(4, 0.25F, 1.75F, 2, 0.4F, 0.2F, 0.9F);
            Assert.Equal(4, aruco.MinModuleSizeInPyramid);
            Assert.Equal(0.25F, aruco.MaxRotation);
            Assert.Equal(1.75F, aruco.MaxModuleSizeMismatch);
            Assert.Equal(2, aruco.MaxTimingPatternMismatch);
            Assert.Equal(0.4F, aruco.MaxPenalties);
            Assert.Equal(0.2F, aruco.MaxColorsMismatch);
            Assert.Equal(0.9F, aruco.ScaleTimingPatternScore);
            Assert.Equal(new QRCodeDetectorArucoParams(4, 0.25F, 1.75F, 2, 0.4F, 0.2F, 0.9F), aruco);
            Assert.True(aruco == new QRCodeDetectorArucoParams(4, 0.25F, 1.75F, 2, 0.4F, 0.2F, 0.9F));
            Assert.True(aruco != new QRCodeDetectorArucoParams(5, 0.25F, 1.75F, 2, 0.4F, 0.2F, 0.9F));
            Assert.Equal(new QRCodeDetectorArucoParams(4, 0.25F, 1.75F, 2, 0.4F, 0.2F, 0.9F).GetHashCode(), aruco.GetHashCode());
            Assert.Equal(
                "{MinModuleSizeInPyramid=4,MaxRotation=0.25,MaxModuleSizeMismatch=1.75,MaxTimingPatternMismatch=2,MaxPenalties=0.4,MaxColorsMismatch=0.2,ScaleTimingPatternScore=0.9}",
                aruco.ToString());

            var encoder = new QRCodeEncoderParams(7, QRCodeEncoderCorrectionLevel.H, QRCodeEncoderEncodeMode.Byte, 2);
            Assert.Equal(7, encoder.Version);
            Assert.Equal(QRCodeEncoderCorrectionLevel.H, encoder.CorrectionLevel);
            Assert.Equal(QRCodeEncoderEncodeMode.Byte, encoder.Mode);
            Assert.Equal(2, encoder.StructureNumber);
            Assert.Equal(new QRCodeEncoderParams(7, QRCodeEncoderCorrectionLevel.H, QRCodeEncoderEncodeMode.Byte, 2), encoder);
            Assert.True(encoder == new QRCodeEncoderParams(7, QRCodeEncoderCorrectionLevel.H, QRCodeEncoderEncodeMode.Byte, 2));
            Assert.True(encoder != new QRCodeEncoderParams(7, QRCodeEncoderCorrectionLevel.L, QRCodeEncoderEncodeMode.Byte, 2));
            Assert.Equal(new QRCodeEncoderParams(7, QRCodeEncoderCorrectionLevel.H, QRCodeEncoderEncodeMode.Byte, 2).GetHashCode(), encoder.GetHashCode());
            Assert.Equal("{Version=7,CorrectionLevel=H,Mode=Byte,StructureNumber=2}", encoder.ToString());
            Assert.Equal(0, new QRCodeEncoderParams(0, QRCodeEncoderCorrectionLevel.L, QRCodeEncoderEncodeMode.Auto, 1).Version);
            Assert.Equal(40, new QRCodeEncoderParams(40, QRCodeEncoderCorrectionLevel.L, QRCodeEncoderEncodeMode.Byte, 16).Version);

            using (var points = new Mat(1, 4, MatType.CV_32FC2))
            {
                var decodedInfo = new[] { "9780000000002" };
                var decodedTypes = new[] { "EAN_13" };
                var result = new BarcodeDecodeResult(true, decodedInfo, decodedTypes, points);
                decodedInfo[0] = "mutated";
                decodedTypes[0] = "mutated";

                Assert.True(result.Success);
                Assert.Equal(1, result.DecodedInfoCount);
                Assert.Equal(1, result.DecodedTypeCount);
                Assert.Equal(new[] { "9780000000002" }, result.DecodedInfo);
                Assert.Equal(new[] { "EAN_13" }, result.DecodedTypes);

                string[] returnedDecodedInfo = result.DecodedInfo;
                string[] returnedDecodedTypes = result.DecodedTypes;
                returnedDecodedInfo[0] = "returned-mutated";
                returnedDecodedTypes[0] = "returned-mutated";

                Assert.Equal(new[] { "9780000000002" }, result.DecodedInfo);
                Assert.Equal(new[] { "EAN_13" }, result.DecodedTypes);
                Assert.Same(points, result.Points);
                Assert.True(result.HasPoints);
                Assert.Equal("BarcodeDecodeResult(Success=True, DecodedInfo=1, DecodedTypes=1, Points=1x4)", result.ToString());
            }
        }

        [Fact]
        public void QRCodeDetectorArucoParamsFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                Assert.Equal(
                    "{MinModuleSizeInPyramid=4.5,MaxRotation=0.25,MaxModuleSizeMismatch=1.75,MaxTimingPatternMismatch=2.125,MaxPenalties=0.4,MaxColorsMismatch=0.2,ScaleTimingPatternScore=0.9}",
                    new QRCodeDetectorArucoParams(4.5F, 0.25F, 1.75F, 2.125F, 0.4F, 0.2F, 0.9F).ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void BarcodeDecodeResultNormalizesNullArrays()
        {
            var result = new BarcodeDecodeResult(false, null!, null!, null);

            Assert.False(result.Success);
            Assert.Equal(0, result.DecodedInfoCount);
            Assert.Equal(0, result.DecodedTypeCount);
            Assert.Empty(result.DecodedInfo);
            Assert.Empty(result.DecodedTypes);
            Assert.Null(result.Points);
            Assert.False(result.HasPoints);
            Assert.Equal("BarcodeDecodeResult(Success=False, DecodedInfo=0, DecodedTypes=0, Points=<null>)", result.ToString());

            Assert.Throws<ArgumentException>(() => new BarcodeDecodeResult(true, new[] { "first", "second" }, new[] { "EAN_13" }, null));
        }

        [Fact]
        public void BarcodeDecodeResultRejectsNullDecodedStringElements()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BarcodeDecodeResult(true, new[] { "9780000000002", null! }, Array.Empty<string>(), null));
            Assert.Throws<ArgumentNullException>(() =>
                new BarcodeDecodeResult(true, new[] { "9780000000002" }, new string[] { null! }, null));
        }

        [Fact]
        public void QRCodeEncoderParamsRejectsInvalidValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new QRCodeEncoderParams(-1, QRCodeEncoderCorrectionLevel.L, QRCodeEncoderEncodeMode.Auto, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new QRCodeEncoderParams(41, QRCodeEncoderCorrectionLevel.L, QRCodeEncoderEncodeMode.Auto, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new QRCodeEncoderParams(1, (QRCodeEncoderCorrectionLevel)(-1), QRCodeEncoderEncodeMode.Auto, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new QRCodeEncoderParams(1, (QRCodeEncoderCorrectionLevel)4, QRCodeEncoderEncodeMode.Auto, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new QRCodeEncoderParams(1, QRCodeEncoderCorrectionLevel.L, (QRCodeEncoderEncodeMode)0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new QRCodeEncoderParams(1, QRCodeEncoderCorrectionLevel.L, (QRCodeEncoderEncodeMode)9, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new QRCodeEncoderParams(1, QRCodeEncoderCorrectionLevel.L, QRCodeEncoderEncodeMode.Auto, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new QRCodeEncoderParams(1, QRCodeEncoderCorrectionLevel.L, QRCodeEncoderEncodeMode.Auto, -1));
        }

        [Fact]
        public void QRCodeEncoderParamsHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(28, Marshal.SizeOf<QRCodeDetectorArucoParams>());
            Assert.Equal(0, FieldOffset<QRCodeDetectorArucoParams>("<MinModuleSizeInPyramid>k__BackingField"));
            Assert.Equal(4, FieldOffset<QRCodeDetectorArucoParams>("<MaxRotation>k__BackingField"));
            Assert.Equal(8, FieldOffset<QRCodeDetectorArucoParams>("<MaxModuleSizeMismatch>k__BackingField"));
            Assert.Equal(12, FieldOffset<QRCodeDetectorArucoParams>("<MaxTimingPatternMismatch>k__BackingField"));
            Assert.Equal(16, FieldOffset<QRCodeDetectorArucoParams>("<MaxPenalties>k__BackingField"));
            Assert.Equal(20, FieldOffset<QRCodeDetectorArucoParams>("<MaxColorsMismatch>k__BackingField"));
            Assert.Equal(24, FieldOffset<QRCodeDetectorArucoParams>("<ScaleTimingPatternScore>k__BackingField"));

            Assert.Equal(16, Marshal.SizeOf<QRCodeEncoderParams>());
            Assert.Equal(0, FieldOffset<QRCodeEncoderParams>("<Version>k__BackingField"));
            Assert.Equal(4, FieldOffset<QRCodeEncoderParams>("<CorrectionLevel>k__BackingField"));
            Assert.Equal(8, FieldOffset<QRCodeEncoderParams>("<Mode>k__BackingField"));
            Assert.Equal(12, FieldOffset<QRCodeEncoderParams>("<StructureNumber>k__BackingField"));
        }

        [Fact]
        public void BarcodeConstructorRejectsNullModelPath()
        {
            Assert.Throws<ArgumentNullException>(() => new BarcodeDetector(null!));
        }

        [Fact]
        public void QRCodeEncoderRejectsNullTextBeforeNativeCall()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var encoder = new QRCodeEncoder(new QRCodeEncoderParams(0, QRCodeEncoderCorrectionLevel.L, QRCodeEncoderEncodeMode.Auto, 1)))
            {
                Assert.Throws<ArgumentNullException>(() => encoder.Encode(null!));
            }
        }

        [Fact]
        public void ObjDetectSecondBatchValidatesMatArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var barcode = BarcodeDetector.Create())
            using (var aruco = QRCodeDetectorAruco.Create())
            using (var image = new Mat())
            using (var points = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => barcode.Detect(null!, points));
                Assert.Throws<ArgumentNullException>(() => barcode.Detect(image, null!));
                Assert.Throws<ArgumentNullException>(() => barcode.Decode(null!, points));
                Assert.Throws<ArgumentNullException>(() => barcode.Decode(image, null!));
                Assert.Throws<ArgumentNullException>(() => barcode.DetectAndDecode(null!));
                Assert.Throws<ArgumentNullException>(() => barcode.DecodeWithType(null!, points));
                Assert.Throws<ArgumentNullException>(() => barcode.DecodeWithType(image, null!));
                Assert.Throws<ArgumentNullException>(() => barcode.DetectAndDecodeWithType(null!));

                Assert.Throws<ArgumentNullException>(() => aruco.Detect(null!, points));
                Assert.Throws<ArgumentNullException>(() => aruco.Detect(image, null!));
                Assert.Throws<ArgumentNullException>(() => aruco.Decode(null!, points));
                Assert.Throws<ArgumentNullException>(() => aruco.Decode(image, null!));
                Assert.Throws<ArgumentNullException>(() => aruco.DetectAndDecode(null!));
                Assert.Throws<ArgumentNullException>(() => aruco.DetectMulti(null!, points));
                Assert.Throws<ArgumentNullException>(() => aruco.DetectMulti(image, null!));
                Assert.Throws<ArgumentNullException>(() => aruco.DecodeMulti(null!, points));
                Assert.Throws<ArgumentNullException>(() => aruco.DecodeMulti(image, null!));
                Assert.Throws<ArgumentNullException>(() => aruco.DetectAndDecodeMulti(null!));
            }
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }
    }
}
