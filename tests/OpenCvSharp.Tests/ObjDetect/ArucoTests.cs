using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ObjDetect;

namespace JYPPX.OpenCvSharp.Tests.ObjDetect
{
    public sealed class ArucoTests
    {
        [Fact]
        public void EnumValuesMatchOpenCvArucoConstants()
        {
            Assert.Equal(0, (int)PredefinedDictionaryType.Dict4X4_50);
            Assert.Equal(3, (int)PredefinedDictionaryType.Dict4X4_1000);
            Assert.Equal(8, (int)PredefinedDictionaryType.Dict6X6_50);
            Assert.Equal(16, (int)PredefinedDictionaryType.DictArucoOriginal);
            Assert.Equal(17, (int)PredefinedDictionaryType.DictAprilTag16h5);
            Assert.Equal(20, (int)PredefinedDictionaryType.DictAprilTag36h11);
            Assert.Equal(21, (int)PredefinedDictionaryType.DictArucoMip36h12);

            Assert.Equal(0, (int)CornerRefineMethod.None);
            Assert.Equal(1, (int)CornerRefineMethod.Subpix);
            Assert.Equal(2, (int)CornerRefineMethod.Contour);
            Assert.Equal(3, (int)CornerRefineMethod.AprilTag);
        }

        [Fact]
        public void ResultObjectsStorePointGroupsAndIds()
        {
            Point2f[][] corners = new[]
            {
                new[]
                {
                    new Point2f(1.0F, 2.0F),
                    new Point2f(3.0F, 4.0F),
                    new Point2f(5.0F, 6.0F),
                    new Point2f(7.0F, 8.0F)
                }
            };
            Point2f[][] rejected = new[]
            {
                new[]
                {
                    new Point2f(9.0F, 10.0F),
                    new Point2f(11.0F, 12.0F)
                }
            };
            var ids = new[] { 42 };
            var confidence = new[] { 0.75F };
            var recoveredIndices = new[] { 0 };
            var detection = new ArucoDetectionResult(corners, ids, rejected, confidence);
            var refine = new ArucoRefineResult(corners, ids, rejected, recoveredIndices);
            var identification = new ArucoIdentificationResult(true, 42, 2);
            corners[0][0] = new Point2f(101.0F, 102.0F);
            corners[0][1] = new Point2f(103.0F, 104.0F);
            rejected[0][0] = new Point2f(109.0F, 110.0F);
            rejected[0][1] = new Point2f(111.0F, 112.0F);
            ids[0] = 7;
            confidence[0] = 0.25F;
            recoveredIndices[0] = 9;

            Assert.Equal(1, detection.Count);
            Assert.Equal(1, detection.CornerCount);
            Assert.Equal(1, detection.IdCount);
            Assert.Equal(1, detection.RejectedCandidateCount);
            Assert.Equal(1, detection.ConfidenceCount);
            Assert.Equal(42, detection.Ids[0]);
            Assert.Equal(new Point2f(1.0F, 2.0F), detection.Corners[0][0]);
            Assert.Equal(new Point2f(11.0F, 12.0F), detection.RejectedCandidates[0][1]);
            Assert.Equal(0.75F, detection.Confidence[0], 5);

            Point2f[][] returnedDetectionCorners = detection.Corners;
            int[] returnedDetectionIds = detection.Ids;
            Point2f[][] returnedDetectionRejected = detection.RejectedCandidates;
            float[] returnedDetectionConfidence = detection.Confidence;
            returnedDetectionCorners[0][0] = new Point2f(201.0F, 202.0F);
            returnedDetectionIds[0] = 11;
            returnedDetectionRejected[0][1] = new Point2f(211.0F, 212.0F);
            returnedDetectionConfidence[0] = 0.10F;

            Assert.Equal(42, detection.Ids[0]);
            Assert.Equal(new Point2f(1.0F, 2.0F), detection.Corners[0][0]);
            Assert.Equal(new Point2f(11.0F, 12.0F), detection.RejectedCandidates[0][1]);
            Assert.Equal(0.75F, detection.Confidence[0], 5);
            Assert.Equal("ArucoDetectionResult(Count=1, Corners=1, Ids=1, RejectedCandidates=1, Confidence=1)", detection.ToString());
            Assert.Equal(1, refine.Count);
            Assert.Equal(1, refine.CornerCount);
            Assert.Equal(1, refine.IdCount);
            Assert.Equal(1, refine.RejectedCandidateCount);
            Assert.Equal(1, refine.RecoveredIndexCount);
            Assert.Equal(42, refine.Ids[0]);
            Assert.Equal(new Point2f(3.0F, 4.0F), refine.Corners[0][1]);
            Assert.Equal(new Point2f(9.0F, 10.0F), refine.RejectedCandidates[0][0]);
            Assert.Equal(0, refine.RecoveredIndices[0]);

            Point2f[][] returnedRefineCorners = refine.Corners;
            int[] returnedRefineIds = refine.Ids;
            Point2f[][] returnedRefineRejected = refine.RejectedCandidates;
            int[] returnedRecoveredIndices = refine.RecoveredIndices;
            returnedRefineCorners[0][1] = new Point2f(203.0F, 204.0F);
            returnedRefineIds[0] = 12;
            returnedRefineRejected[0][0] = new Point2f(209.0F, 210.0F);
            returnedRecoveredIndices[0] = 13;

            Assert.Equal(42, refine.Ids[0]);
            Assert.Equal(new Point2f(3.0F, 4.0F), refine.Corners[0][1]);
            Assert.Equal(new Point2f(9.0F, 10.0F), refine.RejectedCandidates[0][0]);
            Assert.Equal(0, refine.RecoveredIndices[0]);
            Assert.Equal("ArucoRefineResult(Count=1, Corners=1, Ids=1, RejectedCandidates=1, RecoveredIndices=1)", refine.ToString());
            Assert.True(identification.Identified);
            Assert.Equal(42, identification.Index);
            Assert.Equal(2, identification.Rotation);
            Assert.Equal(0, new ArucoIdentificationResult(true, 0, 0).Index);
            Assert.Equal(3, new ArucoIdentificationResult(true, 0, 3).Rotation);
            Assert.False(new ArucoIdentificationResult(false, -1, 0).Identified);
            Assert.Throws<ArgumentOutOfRangeException>(() => new ArucoIdentificationResult(true, -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ArucoIdentificationResult(true, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ArucoIdentificationResult(true, 0, 4));
            Assert.Equal(new ArucoIdentificationResult(true, 42, 2), identification);
            Assert.True(identification == new ArucoIdentificationResult(true, 42, 2));
            Assert.True(identification != new ArucoIdentificationResult(false, 42, 2));
            Assert.Equal(new ArucoIdentificationResult(true, 42, 2).GetHashCode(), identification.GetHashCode());
            Assert.Equal("{Identified=True,Index=42,Rotation=2}", identification.ToString());
        }

        [Fact]
        public void ResultObjectsNormalizeNullArrays()
        {
            var detection = new ArucoDetectionResult(null!, null!, null!, null);

            Assert.Empty(detection.Corners);
            Assert.Empty(detection.Ids);
            Assert.Empty(detection.RejectedCandidates);
            Assert.Empty(detection.Confidence);
            Assert.Equal(0, detection.Count);
            Assert.Equal(0, detection.CornerCount);
            Assert.Equal(0, detection.IdCount);
            Assert.Equal(0, detection.RejectedCandidateCount);
            Assert.Equal(0, detection.ConfidenceCount);
            Assert.Equal("ArucoDetectionResult(Count=0, Corners=0, Ids=0, RejectedCandidates=0, Confidence=0)", detection.ToString());

            var refine = new ArucoRefineResult(null!, null!, null!, null!);

            Assert.Empty(refine.Corners);
            Assert.Empty(refine.Ids);
            Assert.Empty(refine.RejectedCandidates);
            Assert.Empty(refine.RecoveredIndices);
            Assert.Equal(0, refine.Count);
            Assert.Equal(0, refine.CornerCount);
            Assert.Equal(0, refine.IdCount);
            Assert.Equal(0, refine.RejectedCandidateCount);
            Assert.Equal(0, refine.RecoveredIndexCount);
            Assert.Equal("ArucoRefineResult(Count=0, Corners=0, Ids=0, RejectedCandidates=0, RecoveredIndices=0)", refine.ToString());

            var refineCornerCountMismatch = Assert.Throws<ArgumentException>(() => new ArucoRefineResult(
                new[] { new[] { new Point2f(1.0F, 2.0F) }, new[] { new Point2f(3.0F, 4.0F) } },
                new[] { 1 },
                null!,
                null!));
            Assert.Equal("corners", refineCornerCountMismatch.ParamName);

            var cornerCountMismatch = Assert.Throws<ArgumentException>(() => new ArucoDetectionResult(
                new[] { new[] { new Point2f(1.0F, 2.0F) }, new[] { new Point2f(3.0F, 4.0F) } },
                new[] { 1 },
                null!,
                null));
            Assert.Equal("corners", cornerCountMismatch.ParamName);

            var confidenceCountMismatch = Assert.Throws<ArgumentException>(() => new ArucoDetectionResult(
                new[] { new[] { new Point2f(1.0F, 2.0F) } },
                new[] { 1 },
                null!,
                new[] { 0.25F, 0.50F }));
            Assert.Equal("confidence", confidenceCountMismatch.ParamName);
        }

        [Fact]
        public void ResultObjectsNormalizeNullPointGroups()
        {
            Point2f[][] corners = { null!, Array.Empty<Point2f>() };
            Point2f[][] rejected = { null!, new[] { new Point2f(1.0F, 2.0F) } };
            var detection = new ArucoDetectionResult(corners, new[] { 1, 2 }, rejected, null);
            var refine = new ArucoRefineResult(corners, new[] { 1, 2 }, rejected, Array.Empty<int>());

            Assert.Equal(2, detection.Count);
            Assert.Equal(2, detection.CornerCount);
            Assert.Equal(2, detection.IdCount);
            Assert.Equal(2, detection.RejectedCandidateCount);
            Assert.Empty(detection.Corners[0]);
            Assert.Empty(detection.Corners[1]);
            Assert.Empty(detection.RejectedCandidates[0]);
            Assert.Equal(new Point2f(1.0F, 2.0F), detection.RejectedCandidates[1][0]);

            Point2f[][] returnedDetectionRejected = detection.RejectedCandidates;
            returnedDetectionRejected[1][0] = new Point2f(9.0F, 9.0F);

            Assert.Equal(new Point2f(1.0F, 2.0F), detection.RejectedCandidates[1][0]);
            Assert.Equal("ArucoDetectionResult(Count=2, Corners=2, Ids=2, RejectedCandidates=2, Confidence=0)", detection.ToString());

            Assert.Equal(2, refine.Count);
            Assert.Equal(2, refine.CornerCount);
            Assert.Equal(2, refine.IdCount);
            Assert.Equal(2, refine.RejectedCandidateCount);
            Assert.Empty(refine.Corners[0]);
            Assert.Empty(refine.Corners[1]);
            Assert.Empty(refine.RejectedCandidates[0]);
            Assert.Equal(new Point2f(1.0F, 2.0F), refine.RejectedCandidates[1][0]);

            Point2f[][] returnedRefineRejected = refine.RejectedCandidates;
            returnedRefineRejected[1][0] = new Point2f(9.0F, 9.0F);

            Assert.Equal(new Point2f(1.0F, 2.0F), refine.RejectedCandidates[1][0]);
            Assert.Equal("ArucoRefineResult(Count=2, Corners=2, Ids=2, RejectedCandidates=2, RecoveredIndices=0)", refine.ToString());
        }

        [Fact]
        public void ArucoIdentificationResultHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(12, Marshal.SizeOf<ArucoIdentificationResult>());
            Assert.Equal(12, Marshal.SizeOf<ArucoRefineParameters>());

            Assert.Equal(0, FieldOffset<ArucoIdentificationResult>("<Identified>k__BackingField"));
            Assert.Equal(4, FieldOffset<ArucoIdentificationResult>("<Index>k__BackingField"));
            Assert.Equal(8, FieldOffset<ArucoIdentificationResult>("<Rotation>k__BackingField"));

            Assert.Equal(0, FieldOffset<ArucoRefineParameters>("<MinRepDistance>k__BackingField"));
            Assert.Equal(4, FieldOffset<ArucoRefineParameters>("<ErrorCorrectionRate>k__BackingField"));
            Assert.Equal(8, FieldOffset<ArucoRefineParameters>("<CheckAllOrders>k__BackingField"));
        }

        [Fact]
        public void ArucoRefineParametersExposeValueObjectBehavior()
        {
            var parameters = new ArucoRefineParameters(10.0F, 3.0F, true);
            var same = new ArucoRefineParameters(10.0F, 3.0F, true);
            var different = new ArucoRefineParameters(11.0F, 3.0F, true);

            Assert.Equal(10.0F, parameters.MinRepDistance, 5);
            Assert.Equal(3.0F, parameters.ErrorCorrectionRate, 5);
            Assert.True(parameters.CheckAllOrders);
            Assert.Equal(same, parameters);
            Assert.True(parameters == same);
            Assert.False(parameters != same);
            Assert.True(parameters != different);
            Assert.Equal(parameters.GetHashCode(), same.GetHashCode());
            Assert.Equal("{MinRepDistance=10,ErrorCorrectionRate=3,CheckAllOrders=True}", parameters.ToString());
        }

        [Fact]
        public void ArucoRefineParametersFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                Assert.Equal(
                    "{MinRepDistance=10.5,ErrorCorrectionRate=3.25,CheckAllOrders=True}",
                    new ArucoRefineParameters(10.5F, 3.25F, true).ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void DictionaryConstructorRejectsNullBytesListBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() => new ArucoDictionary(null!, 4));
        }

        [Fact]
        public void ArucoObjectsValidateManagedArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (ArucoDictionary dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50))
            using (ArucoGridBoard board = new ArucoGridBoard(new Size(1, 1), 0.04F, 0.01F, dictionary, new[] { 0 }))
            using (ArucoDetector detector = new ArucoDetector(dictionary))
            using (Mat image = new Mat())
            {
                Assert.False(dictionary.IsDisposed);
                Assert.False(board.IsDisposed);
                Assert.False(detector.IsDisposed);

                Assert.Throws<ArgumentNullException>(() => new ArucoGridBoard(new Size(1, 1), 0.04F, 0.01F, null!));
                Assert.Throws<ArgumentNullException>(() => new ArucoGridBoard(new Size(1, 1), 0.04F, 0.01F, dictionary, (int[])null!));
                Assert.Throws<ArgumentNullException>(() => board.GenerateImage(new Size(64, 64), null!));

                Assert.Throws<ArgumentNullException>(() => new ArucoDetector(null!));
                Assert.Throws<ArgumentNullException>(() => detector.SetDictionary(null!));
                Assert.Throws<ArgumentNullException>(() => detector.SetDetectorParameters(null!));
                Assert.Throws<ArgumentNullException>(() => detector.DetectMarkers(null!));
                Assert.Throws<ArgumentNullException>(() => detector.DetectMarkersWithConfidence(null!));
                Assert.Throws<ArgumentNullException>(() => dictionary.Identify(null!, 0.5));
                Assert.Throws<ArgumentNullException>(() => dictionary.GetDistanceToId(null!, 0));
                Assert.Throws<ArgumentNullException>(() => dictionary.GenerateImageMarker(0, 32, null!));
                Assert.Throws<ArgumentNullException>(() => ArucoDictionary.GetByteListFromBits(null!));
                Assert.Throws<ArgumentNullException>(() => ArucoDictionary.GetBitsFromByteList(null!, 4));

                Point2f[][] corners = CreateMarkerCorners(10.0F);
                Point2f[][] rejected = CreateMarkerCorners(20.0F);
                ArucoDetectorParameters detectorParameters = detector.GetDetectorParameters();
                ArucoRefineParameters refineParameters = detector.GetRefineParameters();
                detectorParameters.AdaptiveThreshWinSizeMin = 5;
                detectorParameters.AdaptiveThreshWinSizeMax = 25;
                detectorParameters.AdaptiveThreshWinSizeStep = 4;
                detectorParameters.AdaptiveThreshConstant = 9.5;
                detectorParameters.MinMarkerPerimeterRate = 0.04;
                detectorParameters.MaxMarkerPerimeterRate = 3.5;
                detectorParameters.CornerRefinementMethod = CornerRefineMethod.Subpix;
                detectorParameters.CornerRefinementWinSize = 6;
                detectorParameters.DetectInvertedMarker = true;
                detectorParameters.UseAruco3Detection = true;

                ArucoDetectorParameters copiedParameters = new ArucoDetectorParameters(detectorParameters);
                ArucoDetectorParameters clonedParameters = detectorParameters.Clone();
                Assert.NotSame(detectorParameters, copiedParameters);
                Assert.NotSame(detectorParameters, clonedParameters);
                Assert.NotSame(copiedParameters, clonedParameters);
                detectorParameters.AdaptiveThreshWinSizeMin = 7;
                detectorParameters.DetectInvertedMarker = false;
                Assert.Equal(5, copiedParameters.AdaptiveThreshWinSizeMin);
                Assert.True(copiedParameters.DetectInvertedMarker);
                Assert.Equal(5, clonedParameters.AdaptiveThreshWinSizeMin);
                Assert.True(clonedParameters.DetectInvertedMarker);

                Assert.Same(detector, detector.SetDetectorParameters(copiedParameters));
                ArucoDetectorParameters roundTripParameters = detector.GetDetectorParameters();
                Assert.Equal(copiedParameters.AdaptiveThreshWinSizeMin, roundTripParameters.AdaptiveThreshWinSizeMin);
                Assert.Equal(copiedParameters.AdaptiveThreshWinSizeMax, roundTripParameters.AdaptiveThreshWinSizeMax);
                Assert.Equal(copiedParameters.AdaptiveThreshWinSizeStep, roundTripParameters.AdaptiveThreshWinSizeStep);
                Assert.Equal(copiedParameters.AdaptiveThreshConstant, roundTripParameters.AdaptiveThreshConstant);
                Assert.Equal(copiedParameters.MinMarkerPerimeterRate, roundTripParameters.MinMarkerPerimeterRate);
                Assert.Equal(copiedParameters.MaxMarkerPerimeterRate, roundTripParameters.MaxMarkerPerimeterRate);
                Assert.Equal(copiedParameters.CornerRefinementMethod, roundTripParameters.CornerRefinementMethod);
                Assert.Equal(copiedParameters.CornerRefinementWinSize, roundTripParameters.CornerRefinementWinSize);
                Assert.Equal(copiedParameters.DetectInvertedMarker, roundTripParameters.DetectInvertedMarker);
                Assert.Equal(copiedParameters.UseAruco3Detection, roundTripParameters.UseAruco3Detection);

                detector.Dispose();

                Assert.True(detector.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => detector.GetDictionary());
                Assert.Throws<ObjectDisposedException>(() => detector.SetDictionary(dictionary));
                Assert.Throws<ObjectDisposedException>(() => detector.GetDetectorParameters());
                Assert.Throws<ObjectDisposedException>(() => detector.SetDetectorParameters(detectorParameters));
                Assert.Throws<ObjectDisposedException>(() => detector.GetRefineParameters());
                Assert.Throws<ObjectDisposedException>(() => detector.SetRefineParameters(refineParameters));
                Assert.Throws<ObjectDisposedException>(() => detector.DetectMarkers(image));
                Assert.Throws<ObjectDisposedException>(() => detector.DetectMarkersWithConfidence(image));
                Assert.Throws<ObjectDisposedException>(() => detector.RefineDetectedMarkers(image, board, corners, new[] { 0 }, rejected));

                board.Dispose();

                Assert.True(board.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => board.GridSize);
                Assert.Throws<ObjectDisposedException>(() => board.MarkerLength);
                Assert.Throws<ObjectDisposedException>(() => board.MarkerSeparation);
                Assert.Throws<ObjectDisposedException>(() => board.GenerateImage(new Size(64, 64), image));
                Assert.Throws<ObjectDisposedException>(() => board.GenerateImage(new Size(64, 64)));
            }
        }

        [Fact]
        public void ArucoDictionaryCanGenerateMarkerWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (ArucoDictionary dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50))
            using (Mat marker = dictionary.GenerateImageMarker(0, 32))
            {
                Assert.Equal(32, marker.Rows);
                Assert.Equal(32, marker.Cols);
            }
        }

        private static Point2f[][] CreateMarkerCorners(float offset)
        {
            return new[]
            {
                new[]
                {
                    new Point2f(offset, offset),
                    new Point2f(offset + 1.0F, offset),
                    new Point2f(offset + 1.0F, offset + 1.0F),
                    new Point2f(offset, offset + 1.0F)
                }
            };
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }
    }
}
