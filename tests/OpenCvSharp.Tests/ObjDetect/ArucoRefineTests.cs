using System;
using OpenCvSharp.Core;
using OpenCvSharp.ObjDetect;

namespace OpenCvSharp.Tests.ObjDetect
{
    public sealed class ArucoRefineTests
    {
        [Fact]
        public void ArucoRefineResultStoresPointGroupsIdsAndRecoveredIndices()
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
                    new Point2f(11.0F, 12.0F),
                    new Point2f(13.0F, 14.0F),
                    new Point2f(15.0F, 16.0F)
                }
            };

            var ids = new[] { 7 };
            var recoveredIndices = new[] { 0, 2 };
            var result = new ArucoRefineResult(corners, ids, rejected, recoveredIndices);
            corners[0][2] = new Point2f(50.0F, 60.0F);
            rejected[0][3] = new Point2f(150.0F, 160.0F);
            ids[0] = 9;
            recoveredIndices[0] = 4;
            recoveredIndices[1] = 5;

            Assert.Single(result.Corners);
            Assert.Equal(1, result.Count);
            Assert.Equal(1, result.CornerCount);
            Assert.Equal(1, result.IdCount);
            Assert.Equal(1, result.RejectedCandidateCount);
            Assert.Equal(2, result.RecoveredIndexCount);
            Assert.Equal(7, result.Ids[0]);
            Assert.Equal(new Point2f(5.0F, 6.0F), result.Corners[0][2]);
            Assert.Equal(new Point2f(15.0F, 16.0F), result.RejectedCandidates[0][3]);
            Assert.Equal(new[] { 0, 2 }, result.RecoveredIndices);

            Point2f[][] returnedCorners = result.Corners;
            int[] returnedIds = result.Ids;
            Point2f[][] returnedRejected = result.RejectedCandidates;
            int[] returnedRecoveredIndices = result.RecoveredIndices;
            returnedCorners[0][2] = new Point2f(250.0F, 260.0F);
            returnedIds[0] = 11;
            returnedRejected[0][3] = new Point2f(350.0F, 360.0F);
            returnedRecoveredIndices[0] = 12;

            Assert.Equal(7, result.Ids[0]);
            Assert.Equal(new Point2f(5.0F, 6.0F), result.Corners[0][2]);
            Assert.Equal(new Point2f(15.0F, 16.0F), result.RejectedCandidates[0][3]);
            Assert.Equal(new[] { 0, 2 }, result.RecoveredIndices);
        }

        [Fact]
        public void ArucoRefineResultNormalizesNullArrays()
        {
            var result = new ArucoRefineResult(null!, null!, null!, null!);

            Assert.Empty(result.Corners);
            Assert.Empty(result.Ids);
            Assert.Empty(result.RejectedCandidates);
            Assert.Empty(result.RecoveredIndices);
            Assert.Equal(0, result.Count);
            Assert.Equal(0, result.CornerCount);
            Assert.Equal(0, result.IdCount);
            Assert.Equal(0, result.RejectedCandidateCount);
            Assert.Equal(0, result.RecoveredIndexCount);
        }

        [Fact]
        public void RefineDetectedMarkersValidatesManagedShapeWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point2f[][] corners = CreateMarkerCorners(10.0F);
            Point2f[][] rejected = CreateMarkerCorners(20.0F);

            using (ArucoDictionary dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50))
            using (var board = new ArucoGridBoard(new Size(1, 1), 0.04F, 0.01F, dictionary, new[] { 0 }))
            using (var detector = new ArucoDetector(dictionary))
            using (var image = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    detector.RefineDetectedMarkers(null!, board, corners, new[] { 0 }, rejected));
                Assert.Throws<ArgumentNullException>(() =>
                    detector.RefineDetectedMarkers(image, null!, corners, new[] { 0 }, rejected));
                Assert.Throws<ArgumentException>(() =>
                    detector.RefineDetectedMarkers(image, board, corners, Array.Empty<int>(), rejected));
                Assert.Throws<ArgumentException>(() =>
                    detector.RefineDetectedMarkers(image, board, new[] { Array.Empty<Point2f>() }, new[] { 0 }, rejected));
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

    }
}
