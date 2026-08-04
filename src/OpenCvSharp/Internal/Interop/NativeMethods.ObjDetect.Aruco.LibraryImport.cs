#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct ArucoDetectorParamsNative
        {
            internal int AdaptiveThreshWinSizeMin;
            internal int AdaptiveThreshWinSizeMax;
            internal int AdaptiveThreshWinSizeStep;
            internal double AdaptiveThreshConstant;
            internal double MinMarkerPerimeterRate;
            internal double MaxMarkerPerimeterRate;
            internal double PolygonalApproxAccuracyRate;
            internal double MinCornerDistanceRate;
            internal int MinDistanceToBorder;
            internal double MinMarkerDistanceRate;
            internal float MinGroupDistance;
            internal int CornerRefinementMethod;
            internal int CornerRefinementWinSize;
            internal float RelativeCornerRefinementWinSize;
            internal int CornerRefinementMaxIterations;
            internal double CornerRefinementMinAccuracy;
            internal int MarkerBorderBits;
            internal int PerspectiveRemovePixelPerCell;
            internal double PerspectiveRemoveIgnoredMarginPerCell;
            internal double MaxErroneousBitsInBorderRate;
            internal double MinOtsuStdDev;
            internal double ErrorCorrectionRate;
            internal float AprilTagQuadDecimate;
            internal float AprilTagQuadSigma;
            internal int AprilTagMinClusterPixels;
            internal int AprilTagMaxNmaxima;
            internal float AprilTagCriticalRad;
            internal float AprilTagMaxLineFitMse;
            internal int AprilTagMinWhiteBlackDiff;
            internal int AprilTagDeglitch;
            internal int DetectInvertedMarker;
            internal int UseAruco3Detection;
            internal int MinSideLengthCanonicalImg;
            internal float MinMarkerLengthRatioOriginalImg;
            internal float ValidBitIdThreshold;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ArucoRefineParamsNative
        {
            internal float MinRepDistance;
            internal float ErrorCorrectionRate;
            internal int CheckAllOrders;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ArucoCharucoParamsNative
        {
            internal int MinMarkers;
            internal int TryRefineMarkers;
            internal int CheckMarkers;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MccDetectorParamsNative
        {
            internal int AdaptiveThreshWinSizeMin;
            internal int AdaptiveThreshWinSizeMax;
            internal int AdaptiveThreshWinSizeStep;
            internal double AdaptiveThreshConstant;
            internal double MinContoursAreaRate;
            internal double MinContoursArea;
            internal double ConfidenceThreshold;
            internal double MinContourSolidity;
            internal double FindCandidatesApproxPolyDPEpsMultiplier;
            internal int BorderWidth;
            internal float B0Factor;
            internal float MaxError;
            internal int MinContourPointsAllowed;
            internal int MinContourLengthAllowed;
            internal int MinInterContourDistance;
            internal int MinInterCheckerDistance;
            internal int MinImageSize;
            internal int MinGroupSize;
        }

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_create_default")]
        internal static partial int ArucoDictionaryCreateDefault(out IntPtr dictionary);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_create_predefined")]
        internal static partial int ArucoDictionaryCreatePredefined(int dictionaryId, out IntPtr dictionary);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_create_from_bytes_list")]
        internal static partial int ArucoDictionaryCreateFromBytesList(IntPtr bytesList, int markerSize, int maxCorrectionBits, out IntPtr dictionary);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_extend")]
        internal static partial int ArucoDictionaryExtend(int markerCount, int markerSize, IntPtr baseDictionary, int randomSeed, out IntPtr dictionary);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_release_handle")]
        internal static partial void ArucoDictionaryReleaseHandle(IntPtr dictionary);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_bytes_list")]
        internal static partial int ArucoDictionaryGetBytesList(IntPtr dictionary, out IntPtr bytesList);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_set_bytes_list")]
        internal static partial int ArucoDictionarySetBytesList(IntPtr dictionary, IntPtr bytesList);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_marker_size")]
        internal static partial int ArucoDictionaryGetMarkerSize(IntPtr dictionary, out int markerSize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_set_marker_size")]
        internal static partial int ArucoDictionarySetMarkerSize(IntPtr dictionary, int markerSize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_max_correction_bits")]
        internal static partial int ArucoDictionaryGetMaxCorrectionBits(IntPtr dictionary, out int maxCorrectionBits);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_set_max_correction_bits")]
        internal static partial int ArucoDictionarySetMaxCorrectionBits(IntPtr dictionary, int maxCorrectionBits);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_identify")]
        internal static partial int ArucoDictionaryIdentify(IntPtr dictionary, IntPtr bits, double maxCorrectionRate, out int identified, out int index, out int rotation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_identify_with_threshold")]
        internal static partial int ArucoDictionaryIdentifyWithThreshold(IntPtr dictionary, IntPtr cellPixelRatio, double maxCorrectionRate, float validBitIdThreshold, out int identified, out int index, out int rotation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_distance_to_id")]
        internal static partial int ArucoDictionaryGetDistanceToId(IntPtr dictionary, IntPtr bits, int id, int allRotations, out int distance);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_generate_image_marker")]
        internal static partial int ArucoDictionaryGenerateImageMarker(IntPtr dictionary, int id, int sidePixels, IntPtr image, int borderBits);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_marker_bits")]
        internal static partial int ArucoDictionaryGetMarkerBits(IntPtr dictionary, int markerId, int rotationId, out IntPtr bits);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_byte_list_from_bits")]
        internal static partial int ArucoDictionaryGetByteListFromBits(IntPtr bits, out IntPtr byteList);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_bits_from_byte_list")]
        internal static partial int ArucoDictionaryGetBitsFromByteList(IntPtr byteList, int markerSize, int rotationId, out IntPtr bits);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_default_params")]
        internal static partial int ArucoDetectorDefaultParams(out ArucoDetectorParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_refine_default_params")]
        internal static partial int ArucoRefineDefaultParams(out ArucoRefineParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_detector_default_params")]
        internal static partial int MccDetectorDefaultParams(out MccDetectorParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_default_params")]
        internal static partial int ArucoCharucoDefaultParams(out ArucoCharucoParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_create")]
        internal static partial int ArucoDetectorCreate(IntPtr dictionary, ref ArucoDetectorParamsNative detectorParameters, ref ArucoRefineParamsNative refineParameters, out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_create_multi_dictionary")]
        internal static partial int ArucoDetectorCreateMultiDictionary(IntPtr[] dictionaries, int dictionaryCount, ref ArucoDetectorParamsNative detectorParameters, ref ArucoRefineParamsNative refineParameters, out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_release_handle")]
        internal static partial void ArucoDetectorReleaseHandle(IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_get_dictionary")]
        internal static partial int ArucoDetectorGetDictionary(IntPtr detector, out IntPtr dictionary);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_set_dictionary")]
        internal static partial int ArucoDetectorSetDictionary(IntPtr detector, IntPtr dictionary);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_get_dictionaries_count")]
        internal static partial int ArucoDetectorGetDictionariesCount(IntPtr detector, out int dictionaryCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_get_dictionary_at")]
        internal static partial int ArucoDetectorGetDictionaryAt(IntPtr detector, int dictionaryIndex, out IntPtr dictionary);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_set_dictionaries")]
        internal static partial int ArucoDetectorSetDictionaries(IntPtr detector, IntPtr[] dictionaries, int dictionaryCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_get_detector_parameters")]
        internal static partial int ArucoDetectorGetDetectorParameters(IntPtr detector, out ArucoDetectorParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_set_detector_parameters")]
        internal static partial int ArucoDetectorSetDetectorParameters(IntPtr detector, ref ArucoDetectorParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_get_refine_parameters")]
        internal static partial int ArucoDetectorGetRefineParameters(IntPtr detector, out ArucoRefineParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_set_refine_parameters")]
        internal static partial int ArucoDetectorSetRefineParameters(IntPtr detector, ref ArucoRefineParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_detect_markers_count")]
        internal static partial int ArucoDetectorDetectMarkersCount(IntPtr detector, IntPtr image, out int markerCount, out int cornerPointCount, out int rejectedCount, out int rejectedPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_detect_markers_fill")]
        internal static partial int ArucoDetectorDetectMarkersFill(IntPtr detector, IntPtr image, int* cornerOffsets, int cornerOffsetCapacity, Point2fNative* corners, int cornerCapacity, int* ids, int idCapacity, int* rejectedOffsets, int rejectedOffsetCapacity, Point2fNative* rejectedPoints, int rejectedPointCapacity, out int markerCount, out int cornerPointCount, out int rejectedCount, out int rejectedPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_detect_markers_with_confidence_count")]
        internal static partial int ArucoDetectorDetectMarkersWithConfidenceCount(IntPtr detector, IntPtr image, out int markerCount, out int cornerPointCount, out int rejectedCount, out int rejectedPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_detect_markers_multi_dictionary_count")]
        internal static partial int ArucoDetectorDetectMarkersMultiDictionaryCount(IntPtr detector, IntPtr image, out int markerCount, out int cornerPointCount, out int rejectedCount, out int rejectedPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_detect_markers_multi_dictionary_fill")]
        internal static unsafe partial int ArucoDetectorDetectMarkersMultiDictionaryFill(IntPtr detector, IntPtr image, int* cornerOffsets, int cornerOffsetCapacity, Point2fNative* corners, int cornerCapacity, int* ids, int idCapacity, int* dictionaryIndices, int dictionaryIndexCapacity, int* rejectedOffsets, int rejectedOffsetCapacity, Point2fNative* rejectedPoints, int rejectedPointCapacity, out int markerCount, out int cornerPointCount, out int rejectedCount, out int rejectedPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_detect_markers_with_confidence_fill")]
        internal static partial int ArucoDetectorDetectMarkersWithConfidenceFill(IntPtr detector, IntPtr image, int* cornerOffsets, int cornerOffsetCapacity, Point2fNative* corners, int cornerCapacity, int* ids, int idCapacity, float* confidence, int confidenceCapacity, int* rejectedOffsets, int rejectedOffsetCapacity, Point2fNative* rejectedPoints, int rejectedPointCapacity, out int markerCount, out int cornerPointCount, out int rejectedCount, out int rejectedPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_refine_detected_markers_count")]
        internal static partial int ArucoDetectorRefineDetectedMarkersCount(IntPtr detector, IntPtr image, IntPtr board, int* detectedOffsets, int detectedGroupCount, Point2fNative* detectedPoints, int detectedPointCount, int* detectedIds, int detectedIdCount, int* rejectedOffsets, int rejectedGroupCount, Point2fNative* rejectedPoints, int rejectedPointCount, IntPtr cameraMatrix, IntPtr distCoeffs, out int refinedMarkerCount, out int refinedCornerPointCount, out int refinedRejectedCount, out int refinedRejectedPointCount, out int recoveredIndexCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_refine_detected_markers_fill")]
        internal static partial int ArucoDetectorRefineDetectedMarkersFill(IntPtr detector, IntPtr image, IntPtr board, int* detectedOffsets, int detectedGroupCount, Point2fNative* detectedPoints, int detectedPointCount, int* detectedIds, int detectedIdCount, int* rejectedOffsets, int rejectedGroupCount, Point2fNative* rejectedPoints, int rejectedPointCount, IntPtr cameraMatrix, IntPtr distCoeffs, int* refinedOffsets, int refinedOffsetCapacity, Point2fNative* refinedPoints, int refinedPointCapacity, int* refinedIds, int refinedIdCapacity, int* refinedRejectedOffsets, int refinedRejectedOffsetCapacity, Point2fNative* refinedRejectedPoints, int refinedRejectedPointCapacity, int* recoveredIndices, int recoveredIndexCapacity, out int refinedMarkerCount, out int refinedCornerPointCount, out int refinedRejectedCount, out int refinedRejectedPointCount, out int recoveredIndexCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_create")]
        internal static partial int ArucoGridBoardCreate(int markersX, int markersY, float markerLength, float markerSeparation, IntPtr dictionary, int[] ids, int idCount, out IntPtr board);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_board_create")]
        internal static unsafe partial int ArucoBoardCreate(int* objectPointOffsets, int markerCount, Point3fNative* objectPoints, int objectPointCount, IntPtr dictionary, int* ids, int idCount, out IntPtr board);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_board_release_handle")]
        internal static partial void ArucoBoardReleaseHandle(IntPtr board);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_board_get_dictionary")]
        internal static partial int ArucoBoardGetDictionary(IntPtr board, out IntPtr dictionary);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_board_get_object_points_count")]
        internal static partial int ArucoBoardGetObjectPointsCount(IntPtr board, out int markerCount, out int objectPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_board_get_object_points_fill")]
        internal static unsafe partial int ArucoBoardGetObjectPointsFill(IntPtr board, int* offsets, int offsetCapacity, Point3fNative* points, int pointCapacity, out int markerCount, out int objectPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_board_get_ids_count")]
        internal static partial int ArucoBoardGetIdsCount(IntPtr board, out int idCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_board_get_ids_fill")]
        internal static unsafe partial int ArucoBoardGetIdsFill(IntPtr board, int* ids, int idCapacity, out int idCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_board_get_right_bottom_corner")]
        internal static partial int ArucoBoardGetRightBottomCorner(IntPtr board, out Point3fNative corner);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_board_match_image_points")]
        internal static unsafe partial int ArucoBoardMatchImagePoints(IntPtr board, int* detectedOffsets, int detectedGroupCount, Point2fNative* detectedPoints, int detectedPointCount, int* detectedIds, int detectedIdCount, IntPtr objectPoints, IntPtr imagePoints);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_board_generate_image")]
        internal static partial int ArucoBoardGenerateImage(IntPtr board, int width, int height, IntPtr image, int marginSize, int borderBits);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_draw_detected_markers")]
        internal static unsafe partial int ArucoDrawDetectedMarkers(IntPtr image, int* cornerOffsets, int markerCount, Point2fNative* corners, int cornerPointCount, int* ids, int idCount, double colorV0, double colorV1, double colorV2, double colorV3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_create")]
        internal static partial int ArucoGridBoardCreate(int markersX, int markersY, float markerLength, float markerSeparation, IntPtr dictionary, int* ids, int idCount, out IntPtr board);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_release_handle")]
        internal static partial void ArucoGridBoardReleaseHandle(IntPtr board);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_get_grid_size")]
        internal static partial int ArucoGridBoardGetGridSize(IntPtr board, out int markersX, out int markersY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_get_marker_length")]
        internal static partial int ArucoGridBoardGetMarkerLength(IntPtr board, out float markerLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_get_marker_separation")]
        internal static partial int ArucoGridBoardGetMarkerSeparation(IntPtr board, out float markerSeparation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_generate_image")]
        internal static partial int ArucoGridBoardGenerateImage(IntPtr board, int width, int height, IntPtr image, int marginSize, int borderBits);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_create")]
        internal static partial int ArucoCharucoBoardCreate(int squaresX, int squaresY, float squareLength, float markerLength, IntPtr dictionary, int[] ids, int idCount, out IntPtr board);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_create")]
        internal static partial int ArucoCharucoBoardCreate(int squaresX, int squaresY, float squareLength, float markerLength, IntPtr dictionary, int* ids, int idCount, out IntPtr board);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_release_handle")]
        internal static partial void ArucoCharucoBoardReleaseHandle(IntPtr board);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_get_chessboard_size")]
        internal static partial int ArucoCharucoBoardGetChessboardSize(IntPtr board, out int squaresX, out int squaresY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_get_square_length")]
        internal static partial int ArucoCharucoBoardGetSquareLength(IntPtr board, out float squareLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_get_marker_length")]
        internal static partial int ArucoCharucoBoardGetMarkerLength(IntPtr board, out float markerLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_get_legacy_pattern")]
        internal static partial int ArucoCharucoBoardGetLegacyPattern(IntPtr board, out int legacyPattern);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_set_legacy_pattern")]
        internal static partial int ArucoCharucoBoardSetLegacyPattern(IntPtr board, int legacyPattern);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_get_chessboard_corners_count")]
        internal static partial int ArucoCharucoBoardGetChessboardCornersCount(IntPtr board, out int cornerCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_get_chessboard_corners_fill")]
        internal static partial int ArucoCharucoBoardGetChessboardCornersFill(IntPtr board, Point3fNative* corners, int cornerCapacity, out int cornerCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_check_corners_collinear")]
        internal static partial int ArucoCharucoBoardCheckCornersCollinear(IntPtr board, int[] charucoIds, int charucoIdCount, out int collinear);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_check_corners_collinear")]
        internal static partial int ArucoCharucoBoardCheckCornersCollinear(IntPtr board, int* charucoIds, int charucoIdCount, out int collinear);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_generate_image")]
        internal static partial int ArucoCharucoBoardGenerateImage(IntPtr board, int width, int height, IntPtr image, int marginSize, int borderBits);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_create")]
        internal static partial int ArucoCharucoDetectorCreate(IntPtr board, ref ArucoCharucoParamsNative charucoParameters, IntPtr cameraMatrix, IntPtr distCoeffs, ref ArucoDetectorParamsNative detectorParameters, ref ArucoRefineParamsNative refineParameters, out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_release_handle")]
        internal static partial void ArucoCharucoDetectorReleaseHandle(IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_get_board")]
        internal static partial int ArucoCharucoDetectorGetBoard(IntPtr detector, out IntPtr board);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_set_board")]
        internal static partial int ArucoCharucoDetectorSetBoard(IntPtr detector, IntPtr board);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_get_charuco_parameters")]
        internal static partial int ArucoCharucoDetectorGetCharucoParameters(IntPtr detector, out ArucoCharucoParamsNative parameters, out IntPtr cameraMatrix, out IntPtr distCoeffs);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_set_charuco_parameters")]
        internal static partial int ArucoCharucoDetectorSetCharucoParameters(IntPtr detector, ref ArucoCharucoParamsNative parameters, IntPtr cameraMatrix, IntPtr distCoeffs);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_get_detector_parameters")]
        internal static partial int ArucoCharucoDetectorGetDetectorParameters(IntPtr detector, out ArucoDetectorParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_set_detector_parameters")]
        internal static partial int ArucoCharucoDetectorSetDetectorParameters(IntPtr detector, ref ArucoDetectorParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_get_refine_parameters")]
        internal static partial int ArucoCharucoDetectorGetRefineParameters(IntPtr detector, out ArucoRefineParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_set_refine_parameters")]
        internal static partial int ArucoCharucoDetectorSetRefineParameters(IntPtr detector, ref ArucoRefineParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_detect_board_count")]
        internal static partial int ArucoCharucoDetectorDetectBoardCount(IntPtr detector, IntPtr image, int* inputMarkerOffsets, int inputMarkerGroupCount, Point2fNative* inputMarkerPoints, int inputMarkerPointCount, int* inputMarkerIds, int inputMarkerIdCount, out int charucoCount, out int markerCount, out int markerPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_detect_board_fill")]
        internal static partial int ArucoCharucoDetectorDetectBoardFill(IntPtr detector, IntPtr image, int* inputMarkerOffsets, int inputMarkerGroupCount, Point2fNative* inputMarkerPoints, int inputMarkerPointCount, int* inputMarkerIds, int inputMarkerIdCount, Point2fNative* charucoCorners, int charucoCornerCapacity, int* charucoIds, int charucoIdCapacity, int* markerOffsets, int markerOffsetCapacity, Point2fNative* markerCorners, int markerCornerCapacity, int* markerIds, int markerIdCapacity, out int charucoCount, out int markerCount, out int markerPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_detect_diamonds_count")]
        internal static partial int ArucoCharucoDetectorDetectDiamondsCount(IntPtr detector, IntPtr image, int* inputMarkerOffsets, int inputMarkerCount, Point2fNative* inputMarkerPoints, int inputMarkerPointCount, int* inputMarkerIds, int inputMarkerIdCount, out int diamondCount, out int diamondPointCount, out int markerCount, out int markerPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_detect_diamonds_fill")]
        internal static partial int ArucoCharucoDetectorDetectDiamondsFill(IntPtr detector, IntPtr image, int* inputMarkerOffsets, int inputMarkerCount, Point2fNative* inputMarkerPoints, int inputMarkerPointCount, int* inputMarkerIds, int inputMarkerIdCount, int* diamondOffsets, int diamondOffsetCapacity, Point2fNative* diamondPoints, int diamondPointCapacity, int* diamondIds, int diamondIdCapacity, int* markerOffsets, int markerOffsetCapacity, Point2fNative* markerPoints, int markerPointCapacity, int* markerIds, int markerIdCapacity, out int diamondCount, out int diamondPointCount, out int markerCount, out int markerPointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_draw_detected_corners_charuco")]
        internal static partial int ArucoDrawDetectedCornersCharuco(IntPtr image, Point2fNative* corners, int cornerCount, int* ids, int idCount, double colorV0, double colorV1, double colorV2, double colorV3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_draw_detected_diamonds")]
        internal static partial int ArucoDrawDetectedDiamonds(IntPtr image, int* diamondOffsets, int diamondCount, Point2fNative* diamondPoints, int diamondPointCount, int* diamondIds, int diamondIdCount, double colorV0, double colorV1, double colorV2, double colorV3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_create")]
        internal static partial int MccCheckerCreate(out IntPtr checker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_release_handle")]
        internal static partial void MccCheckerReleaseHandle(IntPtr checker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_target")]
        internal static partial int MccCheckerGetTarget(IntPtr checker, out int target);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_set_target")]
        internal static partial int MccCheckerSetTarget(IntPtr checker, int target);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_box_count")]
        internal static partial int MccCheckerGetBoxCount(IntPtr checker, out int pointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_box_fill")]
        internal static partial int MccCheckerGetBoxFill(IntPtr checker, Point2fNative* points, int pointCapacity, out int pointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_set_box")]
        internal static partial int MccCheckerSetBox(IntPtr checker, Point2fNative* points, int pointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_color_charts_count")]
        internal static partial int MccCheckerGetColorChartsCount(IntPtr checker, out int pointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_color_charts_fill")]
        internal static partial int MccCheckerGetColorChartsFill(IntPtr checker, Point2fNative* points, int pointCapacity, out int pointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_charts_rgb")]
        internal static partial int MccCheckerGetChartsRgb(IntPtr checker, int getStats, out IntPtr chartsRgb);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_set_charts_rgb")]
        internal static partial int MccCheckerSetChartsRgb(IntPtr checker, IntPtr chartsRgb);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_charts_ycbcr")]
        internal static partial int MccCheckerGetChartsYCbCr(IntPtr checker, out IntPtr chartsYCbCr);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_set_charts_ycbcr")]
        internal static partial int MccCheckerSetChartsYCbCr(IntPtr checker, IntPtr chartsYCbCr);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_cost")]
        internal static partial int MccCheckerGetCost(IntPtr checker, out float cost);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_set_cost")]
        internal static partial int MccCheckerSetCost(IntPtr checker, float cost);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_center")]
        internal static partial int MccCheckerGetCenter(IntPtr checker, out Point2fNative center);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_set_center")]
        internal static partial int MccCheckerSetCenter(IntPtr checker, Point2fNative center);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_create")]
        internal static partial int MccCheckerDetectorCreate(out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_create_from_net")]
        internal static partial int MccCheckerDetectorCreateFromNet(IntPtr net, out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_release_handle")]
        internal static partial void MccCheckerDetectorReleaseHandle(IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_process")]
        internal static partial int MccCheckerDetectorProcess(IntPtr detector, IntPtr image, int nc, out int detected);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_process_with_roi")]
        internal static partial int MccCheckerDetectorProcessWithRoi(IntPtr detector, IntPtr image, int[] rois, int roiCount, int nc, out int detected);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_process_with_roi")]
        internal static partial int MccCheckerDetectorProcessWithRoi(IntPtr detector, IntPtr image, int* rois, int roiCount, int nc, out int detected);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_get_best_color_checker")]
        internal static partial int MccCheckerDetectorGetBestColorChecker(IntPtr detector, out IntPtr checker, out int hasChecker);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_get_list_color_checker_count")]
        internal static partial int MccCheckerDetectorGetListColorCheckerCount(IntPtr detector, out int checkerCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_get_list_color_checker_fill")]
        internal static partial int MccCheckerDetectorGetListColorCheckerFill(IntPtr detector, IntPtr[] checkers, int checkerCapacity, out int checkerCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_draw")]
        internal static partial int MccCheckerDetectorDraw(IntPtr detector, IntPtr[] checkers, int checkerCount, IntPtr image, double colorV0, double colorV1, double colorV2, double colorV3, int thickness);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_get_ref_colors")]
        internal static partial int MccCheckerDetectorGetRefColors(IntPtr detector, out IntPtr refColors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_get_detection_params")]
        internal static partial int MccCheckerDetectorGetDetectionParams(IntPtr detector, out MccDetectorParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_set_detection_params")]
        internal static partial int MccCheckerDetectorSetDetectionParams(IntPtr detector, ref MccDetectorParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_get_color_chart_type")]
        internal static partial int MccCheckerDetectorGetColorChartType(IntPtr detector, out int chartType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_set_color_chart_type")]
        internal static partial int MccCheckerDetectorSetColorChartType(IntPtr detector, int chartType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_get_use_dnn_model")]
        internal static partial int MccCheckerDetectorGetUseDnnModel(IntPtr detector, out int useDnnModel);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_set_use_dnn_model")]
        internal static partial int MccCheckerDetectorSetUseDnnModel(IntPtr detector, int useDnnModel);
    }
}
#endif
