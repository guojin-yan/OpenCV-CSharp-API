using System;
using System.Text;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Dnn;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.ObjDetect;

namespace JYPPX.OpenCvSharp.Tests.ObjDetect
{
    public sealed class ObjDetectStructuredParityTests
    {
        [Fact]
        public void GenericBoardRoundTripsNestedValuesAndOwnsOutputs()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;
            using var dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50);
            Point3f[][] objectPoints =
            {
                new[] { new Point3f(0, 0, 0), new Point3f(1, 0, 0), new Point3f(1, 1, 0), new Point3f(0, 1, 0) }
            };
            using var board = new ArucoBoard(objectPoints, dictionary, new[] { 7 });
            objectPoints[0][0] = new Point3f(99, 99, 99);

            Assert.Equal(new[] { 7 }, board.Ids);
            Assert.Equal(new Point3f(0, 0, 0), board.ObjectPoints[0][0]);
            Assert.Equal(new Point3f(1, 1, 0), board.RightBottomCorner);
            using (ArucoDictionary clone = board.Dictionary)
            {
                Assert.Equal(dictionary.MarkerSize, clone.MarkerSize);
            }
            using Mat image = board.GenerateImage(new Size(96, 96), 4);
            Assert.Equal(96, image.Rows);
            Assert.Equal(96, image.Cols);
            using var matchedObjectPoints = new Mat();
            using var matchedImagePoints = new Mat();
            board.MatchImagePoints(
                new[] { new[] { new Point2f(10, 10), new Point2f(20, 10), new Point2f(20, 20), new Point2f(10, 20) } },
                new[] { 7 }, matchedObjectPoints, matchedImagePoints);
            Assert.False(matchedObjectPoints.Empty);
            Assert.False(matchedImagePoints.Empty);
        }

        [Fact]
        public void DictionaryExtensionAndMultiDictionaryDetectionAreDeterministic()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;
            using var first = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50);
            using var second = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict5X5_50);
            using var extended = ArucoDictionary.Extend(55, 4, first, 12345);
            using Mat bytes = extended.BytesList;
            Assert.Equal(4, extended.MarkerSize);
            Assert.Equal(55, bytes.Rows);

            using var detector = ArucoDetector.Create(new[] { first, second });
            using Mat marker = first.GenerateImageMarker(3, 128);
            using Mat borderedMarker = JYPPX.OpenCvSharp.Core.Cv2.CopyMakeBorder(marker, 32, 32, 32, 32, BorderTypes.Constant, new Scalar(255));
            ArucoMultiDictionaryDetectionResult result = detector.DetectMarkersMultiDictionary(borderedMarker);
            Assert.Single(result.Detection.Ids);
            Assert.Equal(3, result.Detection.Ids[0]);
            Assert.Equal(new[] { 0 }, result.DictionaryIndices);
            ArucoDictionary[] values = detector.GetDictionaries();
            try
            {
                Assert.Equal(2, values.Length);
                Assert.Equal(4, values[0].MarkerSize);
                Assert.Equal(5, values[1].MarkerSize);
            }
            finally
            {
                foreach (ArucoDictionary value in values) value.Dispose();
            }
            Assert.Same(detector, detector.SetDictionaries(new[] { second, first }));
            ArucoMultiDictionaryDetectionResult reordered = detector.DetectMarkersMultiDictionary(borderedMarker);
            Assert.Equal(new[] { 1 }, reordered.DictionaryIndices);
        }

        [Fact]
        public void DrawingHelpersMutateCallerOwnedImages()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;
            Point2f[][] corners = { new[] { new Point2f(8, 8), new Point2f(40, 8), new Point2f(40, 40), new Point2f(8, 40) } };
            using var image = new Mat(48, 48, MatType.CV_8UC3, new Scalar(255, 255, 255, 255));
            ArucoDetector.DrawDetectedMarkers(image, corners, new[] { 5 });
            CharucoDetector.DrawDetectedCorners(image, new[] { new Point2f(24, 24) }, new[] { 3 });
            CharucoDetector.DrawDetectedDiamonds(image, corners, new[] { new Vec4i(1, 2, 3, 4) });
            Assert.False(image.Empty);
        }

        [Fact]
        public void QRCodeBytesPreserveEncodedPayload()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;
            const string payload = "objdetect-bytes-1527";
            using var encoder = QRCodeEncoder.Create();
            using Mat code = encoder.Encode(payload);
            using var scaledCode = new Mat();
            JYPPX.OpenCvSharp.ImgProc.Cv2.Resize(code, scaledCode, new Size(code.Cols * 8, code.Rows * 8), interpolation: InterpolationFlags.Nearest);
            using Mat borderedCode = JYPPX.OpenCvSharp.Core.Cv2.CopyMakeBorder(scaledCode, 32, 32, 32, 32, BorderTypes.Constant, new Scalar(255));
            using var detector = QRCodeDetector.Create();
            Assert.Equal(Encoding.UTF8.GetBytes(payload), detector.DetectAndDecodeBytes(borderedCode));
            Assert.Throws<ArgumentNullException>(() => detector.DecodeBytes(null!, null!));

            var source = new[] { new byte[] { 1, 0, 2 } };
            var result = new QRCodeMultiByteDecodeResult(true, source, null);
            source[0][0] = 9;
            byte[][] returned = result.DecodedInfo;
            returned[0][1] = 9;
            Assert.Equal(new byte[] { 1, 0, 2 }, result.DecodedInfo[0]);
        }

        [Fact]
        public void ArucoParameterFamiliesRoundTripByValue()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;
            using var qr = QRCodeDetectorAruco.Create();
            ArucoDetectorParameters qrParameters = qr.GetArucoParameters();
            qrParameters.AdaptiveThreshConstant = 11.25;
            qrParameters.DetectInvertedMarker = true;
            Assert.Same(qr, qr.SetArucoParameters(qrParameters));
            Assert.Equal(11.25, qr.GetArucoParameters().AdaptiveThreshConstant);
            Assert.True(qr.GetArucoParameters().DetectInvertedMarker);

            using var dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50);
            using var board = new CharucoBoard(new Size(3, 3), 0.04F, 0.02F, dictionary);
            using var charuco = new CharucoDetector(board);
            ArucoDetectorParameters detectorParameters = charuco.GetDetectorParameters();
            detectorParameters.CornerRefinementWinSize = 7;
            Assert.Same(charuco, charuco.SetDetectorParameters(detectorParameters));
            Assert.Equal(7, charuco.GetDetectorParameters().CornerRefinementWinSize);
            var refine = new ArucoRefineParameters(12, 2.5F, false);
            Assert.Same(charuco, charuco.SetRefineParameters(refine));
            Assert.Equal(refine, charuco.GetRefineParameters());
        }

        [Fact]
        public void DiamondDetectionUsesCountFillAndReturnsOwnedCollections()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;
            using var dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50);
            using var board = new CharucoBoard(new Size(3, 3), 80, 40, dictionary);
            using Mat boardImage = board.GenerateImage(new Size(600, 600), 20);
            using var detector = new CharucoDetector(board);
            CharucoDiamondDetectionResult result = detector.DetectDiamonds(boardImage);
            Assert.Equal(result.DiamondCorners.Length, result.DiamondIds.Length);
            Assert.Equal(result.MarkerCorners.Length, result.MarkerIds.Length);
            Assert.All(result.DiamondCorners, value => Assert.Equal(4, value.Length));
            if (result.Count > 0)
            {
                Point2f original = result.DiamondCorners[0][0];
                Point2f[][] returned = result.DiamondCorners;
                returned[0][0] = new Point2f(-1, -1);
                Assert.Equal(original, result.DiamondCorners[0][0]);
            }
        }

        [Fact]
        public void AdvancedChessboardCallablesExposeCallerOwnedOutputs()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;
            using var dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50);
            using var board = new CharucoBoard(new Size(5, 7), 80, 40, dictionary);
            using Mat image = board.GenerateImage(new Size(600, 840), 40);
            using var corners = new Mat();
            bool found = JYPPX.OpenCvSharp.Calib3D.Cv2.FindChessboardCornersSB(image, new Size(4, 6), corners);
            Assert.True(found);
            Assert.False(corners.Empty);

            using var cornersWithMeta = new Mat();
            using var meta = new Mat();
            Assert.True(JYPPX.OpenCvSharp.Calib3D.Cv2.FindChessboardCornersSB(image, new Size(4, 6), cornersWithMeta, meta));
            Assert.Equal(6, meta.Rows);
            Assert.Equal(4, meta.Cols);
            using var sharpness = new Mat();
            Scalar summary = JYPPX.OpenCvSharp.Calib3D.Cv2.EstimateChessboardSharpness(image, new Size(4, 6), corners, sharpness: sharpness);
            Assert.True(summary.V0 >= 0);
            Assert.False(sharpness.Empty);
            Assert.True(JYPPX.OpenCvSharp.Calib3D.Cv2.Find4QuadCornerSubpix(image, corners, new Size(5, 5)));
        }

        [Fact]
        public void MccDnnFactoryAndDisposalPathsAreStable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;
            using var net = Net.CreateEmpty();
            using var detector = new CCheckerDetector(net);
            detector.UseDnnModel = false;
            Assert.False(detector.UseDnnModel);
            detector.Dispose();
            Assert.Throws<ObjectDisposedException>(() => _ = detector.UseDnnModel);

            using var dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50);
            Assert.Throws<ArgumentException>(() => ArucoDetector.Create(Array.Empty<ArucoDictionary>()));
            Assert.Throws<ArgumentException>(() => new ArucoBoard(
                new[] { new[] { new Point3f(0, 0, 0) } }, dictionary, Array.Empty<int>()));
        }
    }
}
