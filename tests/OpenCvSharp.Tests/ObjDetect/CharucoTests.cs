using System;
using OpenCvSharp.Core;
using OpenCvSharp.ObjDetect;

namespace OpenCvSharp.Tests.ObjDetect
{
    public sealed class CharucoTests
    {
        [Fact]
        public void ResultObjectsStoreValues()
        {
            var charucoCorners = new[] { new Point2f(1.0F, 2.0F) };
            var charucoIds = new[] { 3 };
            Point2f[][] markerCorners =
            {
                new[]
                {
                    new Point2f(4.0F, 5.0F),
                    new Point2f(6.0F, 7.0F)
                }
            };
            var markerIds = new[] { 8 };
            var result = new CharucoDetectionResult(
                charucoCorners,
                charucoIds,
                markerCorners,
                markerIds);

            charucoCorners[0] = new Point2f(101.0F, 102.0F);
            charucoIds[0] = 33;
            markerCorners[0][1] = new Point2f(106.0F, 107.0F);
            markerIds[0] = 88;

            Assert.Equal(1, result.Count);
            Assert.Equal(1, result.CharucoCornerCount);
            Assert.Equal(1, result.CharucoIdCount);
            Assert.Equal(1, result.MarkerCornerCount);
            Assert.Equal(1, result.MarkerIdCount);
            Assert.Equal(new Point2f(1.0F, 2.0F), result.CharucoCorners[0]);
            Assert.Equal(3, result.CharucoIds[0]);
            Assert.Equal(new Point2f(6.0F, 7.0F), result.MarkerCorners[0][1]);
            Assert.Equal(8, result.MarkerIds[0]);

            Point2f[] returnedCharucoCorners = result.CharucoCorners;
            int[] returnedCharucoIds = result.CharucoIds;
            Point2f[][] returnedMarkerCorners = result.MarkerCorners;
            int[] returnedMarkerIds = result.MarkerIds;
            returnedCharucoCorners[0] = new Point2f(201.0F, 202.0F);
            returnedCharucoIds[0] = 303;
            returnedMarkerCorners[0][1] = new Point2f(206.0F, 207.0F);
            returnedMarkerIds[0] = 808;

            Assert.Equal(new Point2f(1.0F, 2.0F), result.CharucoCorners[0]);
            Assert.Equal(3, result.CharucoIds[0]);
            Assert.Equal(new Point2f(6.0F, 7.0F), result.MarkerCorners[0][1]);
            Assert.Equal(8, result.MarkerIds[0]);
            Assert.Equal("CharucoDetectionResult(Count=1, CharucoCorners=1, CharucoIds=1, MarkerCorners=1, MarkerIds=1)", result.ToString());
        }

        [Fact]
        public void ResultObjectsNormalizeNullArrays()
        {
            var result = new CharucoDetectionResult(null!, null!, null!, null!);

            Assert.Empty(result.CharucoCorners);
            Assert.Empty(result.CharucoIds);
            Assert.Empty(result.MarkerCorners);
            Assert.Empty(result.MarkerIds);
            Assert.Equal(0, result.Count);
            Assert.Equal(0, result.CharucoCornerCount);
            Assert.Equal(0, result.CharucoIdCount);
            Assert.Equal(0, result.MarkerCornerCount);
            Assert.Equal(0, result.MarkerIdCount);
            Assert.Equal("CharucoDetectionResult(Count=0, CharucoCorners=0, CharucoIds=0, MarkerCorners=0, MarkerIds=0)", result.ToString());

            var charucoCornerCountMismatch = Assert.Throws<ArgumentException>(() => new CharucoDetectionResult(
                new[] { new Point2f(1.0F, 2.0F), new Point2f(3.0F, 4.0F) },
                new[] { 1 },
                null!,
                null!));
            Assert.Equal("charucoCorners", charucoCornerCountMismatch.ParamName);

            var markerCornerCountMismatch = Assert.Throws<ArgumentException>(() => new CharucoDetectionResult(
                Array.Empty<Point2f>(),
                Array.Empty<int>(),
                new[] { new[] { new Point2f(5.0F, 6.0F) } },
                Array.Empty<int>()));
            Assert.Equal("markerCorners", markerCornerCountMismatch.ParamName);
        }

        [Fact]
        public void ResultObjectsNormalizeNullMarkerCornerGroups()
        {
            Point2f[][] markerCorners =
            {
                null!,
                Array.Empty<Point2f>(),
                new[] { new Point2f(1.0F, 2.0F) }
            };
            var result = new CharucoDetectionResult(
                Array.Empty<Point2f>(),
                Array.Empty<int>(),
                markerCorners,
                new[] { 1, 2, 3 });

            Assert.Equal(3, result.MarkerCornerCount);
            Assert.Equal(3, result.MarkerIdCount);
            Assert.Empty(result.MarkerCorners[0]);
            Assert.Empty(result.MarkerCorners[1]);
            Assert.Equal(new Point2f(1.0F, 2.0F), result.MarkerCorners[2][0]);

            Point2f[][] returnedMarkerCorners = result.MarkerCorners;
            returnedMarkerCorners[2][0] = new Point2f(9.0F, 9.0F);

            Assert.Equal(new Point2f(1.0F, 2.0F), result.MarkerCorners[2][0]);
            Assert.Equal("CharucoDetectionResult(Count=0, CharucoCorners=0, CharucoIds=0, MarkerCorners=3, MarkerIds=3)", result.ToString());
        }

        [Fact]
        public void ParametersStoreValues()
        {
            var parameters = new CharucoParameters(4, true, false);

            Assert.Equal(4, parameters.MinMarkers);
            Assert.True(parameters.TryRefineMarkers);
            Assert.False(parameters.CheckMarkers);
            Assert.Null(parameters.CameraMatrix);
            Assert.Null(parameters.DistCoeffs);

            Assert.Equal(
                "CharucoParameters(MinMarkers=4, TryRefineMarkers=True, CheckMarkers=False, CameraMatrix=<null>, DistCoeffs=<null>)",
                parameters.ToString());

            using (var cameraMatrix = new Mat(3, 3, MatType.CV_64FC1))
            using (var distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            {
                parameters.CameraMatrix = cameraMatrix;
                parameters.DistCoeffs = distCoeffs;

                Assert.Equal(
                    "CharucoParameters(MinMarkers=4, TryRefineMarkers=True, CheckMarkers=False, CameraMatrix=3x3, DistCoeffs=1x5)",
                    parameters.ToString());

                CharucoParameters copy = new CharucoParameters(parameters);
                CharucoParameters clone = parameters.Clone();
                Assert.NotSame(parameters, copy);
                Assert.NotSame(parameters, clone);
                Assert.NotSame(copy, clone);
                parameters.MinMarkers = 7;
                parameters.TryRefineMarkers = false;
                parameters.CheckMarkers = true;

                Assert.Equal(4, copy.MinMarkers);
                Assert.True(copy.TryRefineMarkers);
                Assert.False(copy.CheckMarkers);
                Assert.Same(cameraMatrix, copy.CameraMatrix);
                Assert.Same(distCoeffs, copy.DistCoeffs);
                Assert.Equal(4, clone.MinMarkers);
                Assert.True(clone.TryRefineMarkers);
                Assert.False(clone.CheckMarkers);
                Assert.Same(cameraMatrix, clone.CameraMatrix);
                Assert.Same(distCoeffs, clone.DistCoeffs);
            }

            Assert.Throws<ArgumentNullException>(() => new CharucoParameters(null!));
        }

        [Fact]
        public void ConstructorsValidateManagedArgumentsBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() => new CharucoBoard(new Size(4, 5), 0.04F, 0.02F, null!));
        }

        [Fact]
        public void CharucoDetectorParametersRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (ArucoDictionary dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50))
            using (CharucoBoard board = new CharucoBoard(new Size(4, 5), 0.04F, 0.02F, dictionary))
            using (CharucoDetector detector = new CharucoDetector(board))
            using (Mat cameraMatrix = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (Mat distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            {
                var parameters = new CharucoParameters(3, true, false)
                {
                    CameraMatrix = cameraMatrix,
                    DistCoeffs = distCoeffs
                };

                Assert.Same(detector, detector.SetCharucoParameters(parameters));

                CharucoParameters roundTrip = detector.GetCharucoParameters();
                using (roundTrip.CameraMatrix)
                using (roundTrip.DistCoeffs)
                {
                    Assert.Equal(3, roundTrip.MinMarkers);
                    Assert.True(roundTrip.TryRefineMarkers);
                    Assert.False(roundTrip.CheckMarkers);
                    Assert.NotNull(roundTrip.CameraMatrix);
                    Assert.NotNull(roundTrip.DistCoeffs);
                    Assert.NotSame(cameraMatrix, roundTrip.CameraMatrix);
                    Assert.NotSame(distCoeffs, roundTrip.DistCoeffs);
                    Assert.Equal(3, roundTrip.CameraMatrix!.Rows);
                    Assert.Equal(3, roundTrip.CameraMatrix.Cols);
                    Assert.Equal(1, roundTrip.DistCoeffs!.Rows);
                    Assert.Equal(5, roundTrip.DistCoeffs.Cols);
                }
            }
        }

        [Fact]
        public void CharucoObjectsValidateManagedArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (ArucoDictionary dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50))
            using (CharucoBoard board = new CharucoBoard(new Size(4, 5), 0.04F, 0.02F, dictionary))
            using (CharucoDetector detector = new CharucoDetector(board))
            using (Mat image = new Mat())
            {
                Assert.False(board.IsDisposed);
                Assert.False(detector.IsDisposed);
                Assert.Throws<ArgumentNullException>(() => new CharucoBoard(new Size(4, 5), 0.04F, 0.02F, dictionary, (int[])null!));
                Assert.Throws<ArgumentNullException>(() => new CharucoDetector(null!));
                Assert.Throws<ArgumentNullException>(() => detector.SetBoard(null!));
                Assert.Throws<ArgumentNullException>(() => detector.SetCharucoParameters(null!));
                Assert.Throws<ArgumentNullException>(() => detector.DetectBoard(null!));
                Assert.Throws<ArgumentNullException>(() => detector.DetectBoard(image, null!, Array.Empty<int>()));
                Assert.Throws<ArgumentNullException>(() => detector.DetectBoard(image, Array.Empty<Point2f[]>(), null!));
                Assert.Throws<ArgumentException>(() => detector.DetectBoard(image, new[] { Array.Empty<Point2f>() }, Array.Empty<int>()));
                Assert.Throws<ArgumentNullException>(() => board.GenerateImage(new Size(64, 64), null!));
                Assert.Throws<ArgumentNullException>(() => board.CheckCharucoCornersCollinear(null!));

                CharucoParameters parameters = detector.GetCharucoParameters();

                detector.Dispose();

                Assert.True(detector.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => detector.GetBoard());
                Assert.Throws<ObjectDisposedException>(() => detector.SetBoard(board));
                Assert.Throws<ObjectDisposedException>(() => detector.GetCharucoParameters());
                Assert.Throws<ObjectDisposedException>(() => detector.SetCharucoParameters(parameters));
                Assert.Throws<ObjectDisposedException>(() => detector.DetectBoard(image));
                Assert.Throws<ObjectDisposedException>(() => detector.DetectBoard(image, Array.Empty<Point2f[]>(), Array.Empty<int>()));

                board.Dispose();

                Assert.True(board.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => board.ChessboardSize);
                Assert.Throws<ObjectDisposedException>(() => board.SquareLength);
                Assert.Throws<ObjectDisposedException>(() => board.MarkerLength);
                Assert.Throws<ObjectDisposedException>(() => board.LegacyPattern);
                Assert.Throws<ObjectDisposedException>(() => board.LegacyPattern = true);
                Assert.Throws<ObjectDisposedException>(() => board.GetChessboardCorners());
                Assert.Throws<ObjectDisposedException>(() => board.CheckCharucoCornersCollinear(Array.Empty<int>()));
                Assert.Throws<ObjectDisposedException>(() => board.GenerateImage(new Size(64, 64), image));
                Assert.Throws<ObjectDisposedException>(() => board.GenerateImage(new Size(64, 64)));
            }
        }

    }
}
