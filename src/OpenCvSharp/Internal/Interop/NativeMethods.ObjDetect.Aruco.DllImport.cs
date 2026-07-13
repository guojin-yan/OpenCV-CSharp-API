#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
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

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_create_default")]
        internal static extern int ArucoDictionaryCreateDefault(out IntPtr dictionary);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_create_predefined")]
        internal static extern int ArucoDictionaryCreatePredefined(int dictionaryId, out IntPtr dictionary);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_create_from_bytes_list")]
        internal static extern int ArucoDictionaryCreateFromBytesList(IntPtr bytesList, int markerSize, int maxCorrectionBits, out IntPtr dictionary);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_release_handle")]
        internal static extern void ArucoDictionaryReleaseHandle(IntPtr dictionary);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_bytes_list")]
        internal static extern int ArucoDictionaryGetBytesList(IntPtr dictionary, out IntPtr bytesList);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_set_bytes_list")]
        internal static extern int ArucoDictionarySetBytesList(IntPtr dictionary, IntPtr bytesList);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_marker_size")]
        internal static extern int ArucoDictionaryGetMarkerSize(IntPtr dictionary, out int markerSize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_set_marker_size")]
        internal static extern int ArucoDictionarySetMarkerSize(IntPtr dictionary, int markerSize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_max_correction_bits")]
        internal static extern int ArucoDictionaryGetMaxCorrectionBits(IntPtr dictionary, out int maxCorrectionBits);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_set_max_correction_bits")]
        internal static extern int ArucoDictionarySetMaxCorrectionBits(IntPtr dictionary, int maxCorrectionBits);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_identify")]
        internal static extern int ArucoDictionaryIdentify(IntPtr dictionary, IntPtr bits, double maxCorrectionRate, out int identified, out int index, out int rotation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_identify_with_threshold")]
        internal static extern int ArucoDictionaryIdentifyWithThreshold(IntPtr dictionary, IntPtr cellPixelRatio, double maxCorrectionRate, float validBitIdThreshold, out int identified, out int index, out int rotation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_distance_to_id")]
        internal static extern int ArucoDictionaryGetDistanceToId(IntPtr dictionary, IntPtr bits, int id, int allRotations, out int distance);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_generate_image_marker")]
        internal static extern int ArucoDictionaryGenerateImageMarker(IntPtr dictionary, int id, int sidePixels, IntPtr image, int borderBits);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_marker_bits")]
        internal static extern int ArucoDictionaryGetMarkerBits(IntPtr dictionary, int markerId, int rotationId, out IntPtr bits);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_byte_list_from_bits")]
        internal static extern int ArucoDictionaryGetByteListFromBits(IntPtr bits, out IntPtr byteList);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_dictionary_get_bits_from_byte_list")]
        internal static extern int ArucoDictionaryGetBitsFromByteList(IntPtr byteList, int markerSize, int rotationId, out IntPtr bits);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_default_params")]
        internal static extern int ArucoDetectorDefaultParams(out ArucoDetectorParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_refine_default_params")]
        internal static extern int ArucoRefineDefaultParams(out ArucoRefineParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_detector_default_params")]
        internal static extern int MccDetectorDefaultParams(out MccDetectorParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_default_params")]
        internal static extern int ArucoCharucoDefaultParams(out ArucoCharucoParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_create")]
        internal static extern int ArucoDetectorCreate(IntPtr dictionary, ref ArucoDetectorParamsNative detectorParameters, ref ArucoRefineParamsNative refineParameters, out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_release_handle")]
        internal static extern void ArucoDetectorReleaseHandle(IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_get_dictionary")]
        internal static extern int ArucoDetectorGetDictionary(IntPtr detector, out IntPtr dictionary);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_set_dictionary")]
        internal static extern int ArucoDetectorSetDictionary(IntPtr detector, IntPtr dictionary);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_get_detector_parameters")]
        internal static extern int ArucoDetectorGetDetectorParameters(IntPtr detector, out ArucoDetectorParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_set_detector_parameters")]
        internal static extern int ArucoDetectorSetDetectorParameters(IntPtr detector, ref ArucoDetectorParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_get_refine_parameters")]
        internal static extern int ArucoDetectorGetRefineParameters(IntPtr detector, out ArucoRefineParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_set_refine_parameters")]
        internal static extern int ArucoDetectorSetRefineParameters(IntPtr detector, ref ArucoRefineParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_detect_markers_count")]
        internal static extern int ArucoDetectorDetectMarkersCount(IntPtr detector, IntPtr image, out int markerCount, out int cornerPointCount, out int rejectedCount, out int rejectedPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_detect_markers_fill")]
        internal static extern int ArucoDetectorDetectMarkersFill(IntPtr detector, IntPtr image, int* cornerOffsets, int cornerOffsetCapacity, Point2fNative* corners, int cornerCapacity, int* ids, int idCapacity, int* rejectedOffsets, int rejectedOffsetCapacity, Point2fNative* rejectedPoints, int rejectedPointCapacity, out int markerCount, out int cornerPointCount, out int rejectedCount, out int rejectedPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_detect_markers_with_confidence_count")]
        internal static extern int ArucoDetectorDetectMarkersWithConfidenceCount(IntPtr detector, IntPtr image, out int markerCount, out int cornerPointCount, out int rejectedCount, out int rejectedPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_detect_markers_with_confidence_fill")]
        internal static extern int ArucoDetectorDetectMarkersWithConfidenceFill(IntPtr detector, IntPtr image, int* cornerOffsets, int cornerOffsetCapacity, Point2fNative* corners, int cornerCapacity, int* ids, int idCapacity, float* confidence, int confidenceCapacity, int* rejectedOffsets, int rejectedOffsetCapacity, Point2fNative* rejectedPoints, int rejectedPointCapacity, out int markerCount, out int cornerPointCount, out int rejectedCount, out int rejectedPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_refine_detected_markers_count")]
        internal static extern int ArucoDetectorRefineDetectedMarkersCount(IntPtr detector, IntPtr image, IntPtr board, int* detectedOffsets, int detectedGroupCount, Point2fNative* detectedPoints, int detectedPointCount, int* detectedIds, int detectedIdCount, int* rejectedOffsets, int rejectedGroupCount, Point2fNative* rejectedPoints, int rejectedPointCount, IntPtr cameraMatrix, IntPtr distCoeffs, out int refinedMarkerCount, out int refinedCornerPointCount, out int refinedRejectedCount, out int refinedRejectedPointCount, out int recoveredIndexCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_detector_refine_detected_markers_fill")]
        internal static extern int ArucoDetectorRefineDetectedMarkersFill(IntPtr detector, IntPtr image, IntPtr board, int* detectedOffsets, int detectedGroupCount, Point2fNative* detectedPoints, int detectedPointCount, int* detectedIds, int detectedIdCount, int* rejectedOffsets, int rejectedGroupCount, Point2fNative* rejectedPoints, int rejectedPointCount, IntPtr cameraMatrix, IntPtr distCoeffs, int* refinedOffsets, int refinedOffsetCapacity, Point2fNative* refinedPoints, int refinedPointCapacity, int* refinedIds, int refinedIdCapacity, int* refinedRejectedOffsets, int refinedRejectedOffsetCapacity, Point2fNative* refinedRejectedPoints, int refinedRejectedPointCapacity, int* recoveredIndices, int recoveredIndexCapacity, out int refinedMarkerCount, out int refinedCornerPointCount, out int refinedRejectedCount, out int refinedRejectedPointCount, out int recoveredIndexCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_create")]
        internal static extern int ArucoGridBoardCreate(int markersX, int markersY, float markerLength, float markerSeparation, IntPtr dictionary, int[] ids, int idCount, out IntPtr board);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_create")]
        internal static extern int ArucoGridBoardCreate(int markersX, int markersY, float markerLength, float markerSeparation, IntPtr dictionary, int* ids, int idCount, out IntPtr board);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_release_handle")]
        internal static extern void ArucoGridBoardReleaseHandle(IntPtr board);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_get_grid_size")]
        internal static extern int ArucoGridBoardGetGridSize(IntPtr board, out int markersX, out int markersY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_get_marker_length")]
        internal static extern int ArucoGridBoardGetMarkerLength(IntPtr board, out float markerLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_get_marker_separation")]
        internal static extern int ArucoGridBoardGetMarkerSeparation(IntPtr board, out float markerSeparation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_grid_board_generate_image")]
        internal static extern int ArucoGridBoardGenerateImage(IntPtr board, int width, int height, IntPtr image, int marginSize, int borderBits);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_create")]
        internal static extern int ArucoCharucoBoardCreate(int squaresX, int squaresY, float squareLength, float markerLength, IntPtr dictionary, int[] ids, int idCount, out IntPtr board);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_create")]
        internal static extern int ArucoCharucoBoardCreate(int squaresX, int squaresY, float squareLength, float markerLength, IntPtr dictionary, int* ids, int idCount, out IntPtr board);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_release_handle")]
        internal static extern void ArucoCharucoBoardReleaseHandle(IntPtr board);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_get_chessboard_size")]
        internal static extern int ArucoCharucoBoardGetChessboardSize(IntPtr board, out int squaresX, out int squaresY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_get_square_length")]
        internal static extern int ArucoCharucoBoardGetSquareLength(IntPtr board, out float squareLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_get_marker_length")]
        internal static extern int ArucoCharucoBoardGetMarkerLength(IntPtr board, out float markerLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_get_legacy_pattern")]
        internal static extern int ArucoCharucoBoardGetLegacyPattern(IntPtr board, out int legacyPattern);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_set_legacy_pattern")]
        internal static extern int ArucoCharucoBoardSetLegacyPattern(IntPtr board, int legacyPattern);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_get_chessboard_corners_count")]
        internal static extern int ArucoCharucoBoardGetChessboardCornersCount(IntPtr board, out int cornerCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_get_chessboard_corners_fill")]
        internal static extern int ArucoCharucoBoardGetChessboardCornersFill(IntPtr board, Point3fNative* corners, int cornerCapacity, out int cornerCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_check_corners_collinear")]
        internal static extern int ArucoCharucoBoardCheckCornersCollinear(IntPtr board, int[] charucoIds, int charucoIdCount, out int collinear);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_check_corners_collinear")]
        internal static extern int ArucoCharucoBoardCheckCornersCollinear(IntPtr board, int* charucoIds, int charucoIdCount, out int collinear);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_board_generate_image")]
        internal static extern int ArucoCharucoBoardGenerateImage(IntPtr board, int width, int height, IntPtr image, int marginSize, int borderBits);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_create")]
        internal static extern int ArucoCharucoDetectorCreate(IntPtr board, ref ArucoCharucoParamsNative charucoParameters, IntPtr cameraMatrix, IntPtr distCoeffs, ref ArucoDetectorParamsNative detectorParameters, ref ArucoRefineParamsNative refineParameters, out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_release_handle")]
        internal static extern void ArucoCharucoDetectorReleaseHandle(IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_get_board")]
        internal static extern int ArucoCharucoDetectorGetBoard(IntPtr detector, out IntPtr board);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_set_board")]
        internal static extern int ArucoCharucoDetectorSetBoard(IntPtr detector, IntPtr board);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_get_charuco_parameters")]
        internal static extern int ArucoCharucoDetectorGetCharucoParameters(IntPtr detector, out ArucoCharucoParamsNative parameters, out IntPtr cameraMatrix, out IntPtr distCoeffs);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_set_charuco_parameters")]
        internal static extern int ArucoCharucoDetectorSetCharucoParameters(IntPtr detector, ref ArucoCharucoParamsNative parameters, IntPtr cameraMatrix, IntPtr distCoeffs);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_detect_board_count")]
        internal static extern int ArucoCharucoDetectorDetectBoardCount(IntPtr detector, IntPtr image, int* inputMarkerOffsets, int inputMarkerGroupCount, Point2fNative* inputMarkerPoints, int inputMarkerPointCount, int* inputMarkerIds, int inputMarkerIdCount, out int charucoCount, out int markerCount, out int markerPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_aruco_charuco_detector_detect_board_fill")]
        internal static extern int ArucoCharucoDetectorDetectBoardFill(IntPtr detector, IntPtr image, int* inputMarkerOffsets, int inputMarkerGroupCount, Point2fNative* inputMarkerPoints, int inputMarkerPointCount, int* inputMarkerIds, int inputMarkerIdCount, Point2fNative* charucoCorners, int charucoCornerCapacity, int* charucoIds, int charucoIdCapacity, int* markerOffsets, int markerOffsetCapacity, Point2fNative* markerCorners, int markerCornerCapacity, int* markerIds, int markerIdCapacity, out int charucoCount, out int markerCount, out int markerPointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_create")]
        internal static extern int MccCheckerCreate(out IntPtr checker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_release_handle")]
        internal static extern void MccCheckerReleaseHandle(IntPtr checker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_target")]
        internal static extern int MccCheckerGetTarget(IntPtr checker, out int target);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_set_target")]
        internal static extern int MccCheckerSetTarget(IntPtr checker, int target);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_box_count")]
        internal static extern int MccCheckerGetBoxCount(IntPtr checker, out int pointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_box_fill")]
        internal static extern int MccCheckerGetBoxFill(IntPtr checker, Point2fNative* points, int pointCapacity, out int pointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_set_box")]
        internal static extern int MccCheckerSetBox(IntPtr checker, Point2fNative* points, int pointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_color_charts_count")]
        internal static extern int MccCheckerGetColorChartsCount(IntPtr checker, out int pointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_color_charts_fill")]
        internal static extern int MccCheckerGetColorChartsFill(IntPtr checker, Point2fNative* points, int pointCapacity, out int pointCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_charts_rgb")]
        internal static extern int MccCheckerGetChartsRgb(IntPtr checker, int getStats, out IntPtr chartsRgb);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_set_charts_rgb")]
        internal static extern int MccCheckerSetChartsRgb(IntPtr checker, IntPtr chartsRgb);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_charts_ycbcr")]
        internal static extern int MccCheckerGetChartsYCbCr(IntPtr checker, out IntPtr chartsYCbCr);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_set_charts_ycbcr")]
        internal static extern int MccCheckerSetChartsYCbCr(IntPtr checker, IntPtr chartsYCbCr);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_cost")]
        internal static extern int MccCheckerGetCost(IntPtr checker, out float cost);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_set_cost")]
        internal static extern int MccCheckerSetCost(IntPtr checker, float cost);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_get_center")]
        internal static extern int MccCheckerGetCenter(IntPtr checker, out Point2fNative center);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_set_center")]
        internal static extern int MccCheckerSetCenter(IntPtr checker, Point2fNative center);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_create")]
        internal static extern int MccCheckerDetectorCreate(out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_release_handle")]
        internal static extern void MccCheckerDetectorReleaseHandle(IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_process")]
        internal static extern int MccCheckerDetectorProcess(IntPtr detector, IntPtr image, int nc, out int detected);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_process_with_roi")]
        internal static extern int MccCheckerDetectorProcessWithRoi(IntPtr detector, IntPtr image, int[] rois, int roiCount, int nc, out int detected);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_process_with_roi")]
        internal static extern int MccCheckerDetectorProcessWithRoi(IntPtr detector, IntPtr image, int* rois, int roiCount, int nc, out int detected);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_get_best_color_checker")]
        internal static extern int MccCheckerDetectorGetBestColorChecker(IntPtr detector, out IntPtr checker, out int hasChecker);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_get_list_color_checker_count")]
        internal static extern int MccCheckerDetectorGetListColorCheckerCount(IntPtr detector, out int checkerCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_get_list_color_checker_fill")]
        internal static extern int MccCheckerDetectorGetListColorCheckerFill(IntPtr detector, IntPtr[] checkers, int checkerCapacity, out int checkerCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_draw")]
        internal static extern int MccCheckerDetectorDraw(IntPtr detector, IntPtr[] checkers, int checkerCount, IntPtr image, double colorV0, double colorV1, double colorV2, double colorV3, int thickness);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_get_ref_colors")]
        internal static extern int MccCheckerDetectorGetRefColors(IntPtr detector, out IntPtr refColors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_get_detection_params")]
        internal static extern int MccCheckerDetectorGetDetectionParams(IntPtr detector, out MccDetectorParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_set_detection_params")]
        internal static extern int MccCheckerDetectorSetDetectionParams(IntPtr detector, ref MccDetectorParamsNative parameters);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_get_color_chart_type")]
        internal static extern int MccCheckerDetectorGetColorChartType(IntPtr detector, out int chartType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_mcc_checker_detector_set_color_chart_type")]
        internal static extern int MccCheckerDetectorSetColorChartType(IntPtr detector, int chartType);
    }
}
#endif
