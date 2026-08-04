#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Calib3DPoint2fNative
        {
            internal float X;
            internal float Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Calib3DPoint3fNative
        {
            internal float X;
            internal float Y;
            internal float Z;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Calib3DVec4fNative
        {
            internal float V0;
            internal float V1;
            internal float V2;
            internal float V3;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Calib3DVec6fNative
        {
            internal float V0;
            internal float V1;
            internal float V2;
            internal float V3;
            internal float V4;
            internal float V5;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Calib3DUsacParamsNative
        {
            internal double Confidence;
            internal int IsParallel;
            internal int LoIterations;
            internal int LoMethod;
            internal int LoSampleSize;
            internal int MaxIterations;
            internal int NeighborsSearch;
            internal int RandomGeneratorState;
            internal int Sampler;
            internal int Score;
            internal double Threshold;
            internal int FinalPolisher;
            internal int FinalPolisherIterations;
        }

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_rodrigues")]
        internal static partial int Calib3DRodrigues(IntPtr src, IntPtr dst, IntPtr jacobian);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_rq_decomp3x3")]
        internal static partial int Calib3DRQDecomp3x3(IntPtr src, IntPtr mtxR, IntPtr mtxQ, IntPtr qx, IntPtr qy, IntPtr qz, out double eulerX, out double eulerY, out double eulerZ);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_decompose_projection_matrix")]
        internal static partial int Calib3DDecomposeProjectionMatrix(IntPtr projMatrix, IntPtr cameraMatrix, IntPtr rotMatrix, IntPtr transVect, IntPtr rotMatrixX, IntPtr rotMatrixY, IntPtr rotMatrixZ, IntPtr eulerAngles);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_compose_rt")]
        internal static partial int Calib3DComposeRT(IntPtr rvec1, IntPtr tvec1, IntPtr rvec2, IntPtr tvec2, IntPtr rvec3, IntPtr tvec3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_compose_rt_extended")]
        internal static partial int Calib3DComposeRTExtended(
            IntPtr rvec1,
            IntPtr tvec1,
            IntPtr rvec2,
            IntPtr tvec2,
            IntPtr rvec3,
            IntPtr tvec3,
            IntPtr dr3dr1,
            IntPtr dr3dt1,
            IntPtr dr3dr2,
            IntPtr dr3dt2,
            IntPtr dt3dr1,
            IntPtr dt3dt1,
            IntPtr dt3dr2,
            IntPtr dt3dt2);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_project_points")]
        internal static partial int Calib3DProjectPoints(IntPtr objectPoints, IntPtr rvec, IntPtr tvec, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr imagePoints, IntPtr jacobian, double aspectRatio);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_project_points_separated_jacobians")]
        internal static partial int Calib3DProjectPointsSeparatedJacobians(
            IntPtr objectPoints,
            IntPtr rvec,
            IntPtr tvec,
            IntPtr cameraMatrix,
            IntPtr distCoeffs,
            IntPtr imagePoints,
            IntPtr dpdr,
            IntPtr dpdt,
            IntPtr dpdf,
            IntPtr dpdc,
            IntPtr dpdk,
            IntPtr dpdo,
            double aspectRatio);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_solve_pnp")]
        internal static partial int Calib3DSolvePnP(IntPtr objectPoints, IntPtr imagePoints, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvec, IntPtr tvec, int useExtrinsicGuess, int flags, out int solved);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_solve_pnp_ransac")]
        internal static partial int Calib3DSolvePnPRansac(IntPtr objectPoints, IntPtr imagePoints, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvec, IntPtr tvec, int useExtrinsicGuess, int iterationsCount, float reprojectionError, double confidence, IntPtr inliers, int flags, out int solved);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_fisheye_project_points")]
        internal static partial int Calib3DFisheyeProjectPoints(IntPtr objectPoints, IntPtr rvec, IntPtr tvec, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr imagePoints, double alpha, IntPtr jacobian);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_fisheye_distort_points")]
        internal static partial int Calib3DFisheyeDistortPoints(IntPtr undistorted, IntPtr distorted, IntPtr cameraMatrix, IntPtr distCoeffs, double alpha);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_fisheye_distort_points_with_camera_matrix")]
        internal static partial int Calib3DFisheyeDistortPointsWithCameraMatrix(IntPtr undistorted, IntPtr distorted, IntPtr undistortedCameraMatrix, IntPtr cameraMatrix, IntPtr distCoeffs, double alpha);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_fisheye_undistort_points")]
        internal static partial int Calib3DFisheyeUndistortPoints(IntPtr distorted, IntPtr undistorted, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr r, IntPtr p, int criteriaType, int criteriaMaxCount, double criteriaEpsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_fisheye_estimate_new_camera_matrix")]
        internal static partial int Calib3DFisheyeEstimateNewCameraMatrix(IntPtr cameraMatrix, IntPtr distCoeffs, int imageWidth, int imageHeight, IntPtr r, IntPtr newCameraMatrix, double balance, int newImageWidth, int newImageHeight, double fovScale);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_fisheye_solve_pnp")]
        internal static partial int Calib3DFisheyeSolvePnP(IntPtr objectPoints, IntPtr imagePoints, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvec, IntPtr tvec, int useExtrinsicGuess, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out int solved);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_fisheye_solve_pnp_ransac")]
        internal static partial int Calib3DFisheyeSolvePnPRansac(IntPtr objectPoints, IntPtr imagePoints, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvec, IntPtr tvec, int useExtrinsicGuess, int iterationsCount, float reprojectionError, double confidence, IntPtr inliers, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out int solved);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_find_homography")]
        internal static partial int Calib3DFindHomography(IntPtr srcPoints, IntPtr dstPoints, int method, double ransacReprojThreshold, IntPtr mask, int maxIters, double confidence, out IntPtr homography);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_find_fundamental_mat")]
        internal static partial int Calib3DFindFundamentalMat(IntPtr points1, IntPtr points2, int method, double ransacReprojThreshold, double confidence, int maxIters, IntPtr mask, out IntPtr fundamental);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_find_essential_mat")]
        internal static partial int Calib3DFindEssentialMat(IntPtr points1, IntPtr points2, IntPtr cameraMatrix, int method, double prob, double threshold, int maxIters, IntPtr mask, out IntPtr essential);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_find_essential_mat_focal")]
        internal static partial int Calib3DFindEssentialMatFocal(IntPtr points1, IntPtr points2, double focal, double ppX, double ppY, int method, double prob, double threshold, int maxIters, IntPtr mask, out IntPtr essential);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_find_essential_mat_two_cameras")]
        internal static partial int Calib3DFindEssentialMatTwoCameras(IntPtr points1, IntPtr points2, IntPtr cameraMatrix1, IntPtr distCoeffs1, IntPtr cameraMatrix2, IntPtr distCoeffs2, int method, double prob, double threshold, IntPtr mask, out IntPtr essential);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_decompose_essential_mat")]
        internal static partial int Calib3DDecomposeEssentialMat(IntPtr essential, IntPtr r1, IntPtr r2, IntPtr t);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_recover_pose")]
        internal static partial int Calib3DRecoverPose(IntPtr essential, IntPtr points1, IntPtr points2, IntPtr cameraMatrix, IntPtr r, IntPtr t, IntPtr mask, out int inlierCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_recover_pose_focal")]
        internal static partial int Calib3DRecoverPoseFocal(IntPtr essential, IntPtr points1, IntPtr points2, IntPtr r, IntPtr t, double focal, double ppX, double ppY, IntPtr mask, out int inlierCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_recover_pose_with_distance")]
        internal static partial int Calib3DRecoverPoseWithDistance(IntPtr essential, IntPtr points1, IntPtr points2, IntPtr cameraMatrix, IntPtr r, IntPtr t, double distanceThresh, IntPtr mask, IntPtr triangulatedPoints, out int inlierCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_recover_pose_two_cameras")]
        internal static partial int Calib3DRecoverPoseTwoCameras(IntPtr points1, IntPtr points2, IntPtr cameraMatrix1, IntPtr distCoeffs1, IntPtr cameraMatrix2, IntPtr distCoeffs2, IntPtr essential, IntPtr r, IntPtr t, int method, double prob, double threshold, IntPtr mask, out int inlierCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_compute_correspond_epilines")]
        internal static partial int Calib3DComputeCorrespondEpilines(IntPtr points, int whichImage, IntPtr fundamental, IntPtr lines);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_estimate_translation_3d")]
        internal static partial int Calib3DEstimateTranslation3D(IntPtr source, IntPtr destination, IntPtr translation, IntPtr inliers, double ransacThreshold, double confidence, out int found);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_estimate_translation_2d")]
        internal static partial int Calib3DEstimateTranslation2D(IntPtr source, IntPtr destination, IntPtr inliers, int method, double ransacReprojThreshold, int maxIters, double confidence, int refineIters, out double translationX, out double translationY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_estimate_affine_3d_ransac")]
        internal static partial int Calib3DEstimateAffine3DRansac(IntPtr source, IntPtr destination, IntPtr transform, IntPtr inliers, double ransacThreshold, double confidence, out int found);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_estimate_affine_3d_umeyama")]
        internal static partial int Calib3DEstimateAffine3DUmeyama(IntPtr source, IntPtr destination, IntPtr transform, int forceRotation, out double scale);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_estimate_affine_2d")]
        internal static partial int Calib3DEstimateAffine2D(IntPtr source, IntPtr destination, IntPtr transform, IntPtr inliers, int method, double ransacReprojThreshold, int maxIters, double confidence, int refineIters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_estimate_affine_partial_2d")]
        internal static partial int Calib3DEstimateAffinePartial2D(IntPtr source, IntPtr destination, IntPtr transform, IntPtr inliers, int method, double ransacReprojThreshold, int maxIters, double confidence, int refineIters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_decompose_homography_mat")]
        internal static partial int Calib3DDecomposeHomographyMat(IntPtr homography, IntPtr cameraMatrix, IntPtr* rotations, IntPtr* translations, IntPtr* normals, int outputCapacity, out int solutionCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_filter_homography_decomp_by_visible_refpoints")]
        internal static partial int Calib3DFilterHomographyDecompByVisibleRefpoints(IntPtr* rotations, int rotationCount, IntPtr* normals, int normalCount, IntPtr beforePoints, IntPtr afterPoints, IntPtr possibleSolutions, IntPtr pointsMask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_convert_points_to_homogeneous")]
        internal static partial int Calib3DConvertPointsToHomogeneous(IntPtr source, IntPtr destination, int dtype);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_convert_points_from_homogeneous")]
        internal static partial int Calib3DConvertPointsFromHomogeneous(IntPtr source, IntPtr destination, int dtype);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_correct_matches")]
        internal static partial int Calib3DCorrectMatches(IntPtr fundamental, IntPtr points1, IntPtr points2, IntPtr correctedPoints1, IntPtr correctedPoints2);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_sampson_distance")]
        internal static partial int Calib3DSampsonDistance(IntPtr point1, IntPtr point2, IntPtr fundamental, out double distance);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_triangulate_points")]
        internal static partial int Calib3DTriangulatePoints(IntPtr projMatr1, IntPtr projMatr2, IntPtr projPoints1, IntPtr projPoints2, IntPtr points4D);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_undistort_points")]
        internal static partial int Calib3DUndistortPoints(IntPtr src, IntPtr dst, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr r, IntPtr p, int criteriaType, int criteriaMaxCount, double criteriaEpsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_undistort_image_points")]
        internal static partial int Calib3DUndistortImagePoints(IntPtr src, IntPtr dst, IntPtr cameraMatrix, IntPtr distCoeffs, int criteriaType, int criteriaMaxCount, double criteriaEpsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_filter_speckles")]
        internal static partial int Calib3DFilterSpeckles(IntPtr image, double newValue, int maxSpeckleSize, double maxDifference, IntPtr buffer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_get_valid_disparity_roi")]
        internal static partial int Calib3DGetValidDisparityROI(int roi1X, int roi1Y, int roi1Width, int roi1Height, int roi2X, int roi2Y, int roi2Width, int roi2Height, int minDisparity, int numberOfDisparities, int blockSize, out int resultX, out int resultY, out int resultWidth, out int resultHeight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_validate_disparity")]
        internal static partial int Calib3DValidateDisparity(IntPtr disparity, IntPtr cost, int minDisparity, int numberOfDisparities, int disp12MaxDifference);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_reproject_image_to_3d")]
        internal static partial int Calib3DReprojectImageTo3D(IntPtr disparity, IntPtr image3D, IntPtr q, int handleMissingValues, int ddepth);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_init_undistort_rectify_map")]
        internal static partial int Calib3DInitUndistortRectifyMap(IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr r, IntPtr newCameraMatrix, int sizeWidth, int sizeHeight, int m1type, IntPtr map1, IntPtr map2);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_stereo_rectify")]
        internal static partial int Calib3DStereoRectify(IntPtr cameraMatrix1, IntPtr distCoeffs1, IntPtr cameraMatrix2, IntPtr distCoeffs2, int imageWidth, int imageHeight, IntPtr r, IntPtr t, IntPtr r1, IntPtr r2, IntPtr p1, IntPtr p2, IntPtr q, int flags, double alpha, int newImageWidth, int newImageHeight, out int roi1X, out int roi1Y, out int roi1Width, out int roi1Height, out int roi2X, out int roi2Y, out int roi2Width, out int roi2Height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_solve_pnp_generic")]
        internal static partial int Calib3DSolvePnPGeneric(IntPtr objectPoints, IntPtr imagePoints, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvecs, IntPtr tvecs, int useExtrinsicGuess, int flags, IntPtr rvec, IntPtr tvec, IntPtr reprojectionError, out int solutionCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_solve_p3p")]
        internal static partial int Calib3DSolveP3P(IntPtr objectPoints, IntPtr imagePoints, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvecs, IntPtr tvecs, int flags, out int solutionCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_mat_mul_deriv")]
        internal static partial int Calib3DMatMulDeriv(IntPtr a, IntPtr b, IntPtr dABdA, IntPtr dABdB);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_solve_pnp_refine_lm")]
        internal static partial int Calib3DSolvePnPRefineLM(IntPtr objectPoints, IntPtr imagePoints, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvec, IntPtr tvec, int criteriaType, int criteriaMaxCount, double criteriaEpsilon);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_solve_pnp_refine_vvs")]
        internal static partial int Calib3DSolvePnPRefineVVS(IntPtr objectPoints, IntPtr imagePoints, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvec, IntPtr tvec, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, double vvsLambda);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_find_chessboard_corners")]
        internal static partial int Calib3DFindChessboardCorners(IntPtr image, int patternWidth, int patternHeight, IntPtr corners, int flags, out int found);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_find_chessboard_corners_sb")]
        internal static partial int Calib3DFindChessboardCornersSB(IntPtr image, int patternWidth, int patternHeight, IntPtr corners, int flags, out int found);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_find_chessboard_corners_sb_with_meta")]
        internal static partial int Calib3DFindChessboardCornersSBWithMeta(IntPtr image, int patternWidth, int patternHeight, IntPtr corners, int flags, IntPtr meta, out int found);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_estimate_chessboard_sharpness")]
        internal static partial int Calib3DEstimateChessboardSharpness(IntPtr image, int patternWidth, int patternHeight, IntPtr corners, float riseDistance, int vertical, IntPtr sharpness, out double value0, out double value1, out double value2, out double value3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_find_4_quad_corner_subpix")]
        internal static partial int Calib3DFind4QuadCornerSubpix(IntPtr image, IntPtr corners, int regionWidth, int regionHeight, out int found);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_check_chessboard")]
        internal static partial int Calib3DCheckChessboard(IntPtr image, int patternWidth, int patternHeight, out int found);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_find_circles_grid")]
        internal static partial int Calib3DFindCirclesGrid(IntPtr image, int patternWidth, int patternHeight, IntPtr centers, int flags, out int found);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_draw_chessboard_corners")]
        internal static partial int Calib3DDrawChessboardCorners(IntPtr image, int patternWidth, int patternHeight, IntPtr corners, int patternWasFound);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_get_default_new_camera_matrix")]
        internal static partial int Calib3DGetDefaultNewCameraMatrix(IntPtr cameraMatrix, int imageWidth, int imageHeight, int centerPrincipalPoint, IntPtr newCameraMatrix);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_get_undistort_rectangles")]
        internal static partial int Calib3DGetUndistortRectangles(IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr r, IntPtr newCameraMatrix, int imageWidth, int imageHeight, out double innerX, out double innerY, out double innerWidth, out double innerHeight, out double outerX, out double outerY, out double outerWidth, out double outerHeight);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_get_optimal_new_camera_matrix")]
        internal static partial int Calib3DGetOptimalNewCameraMatrix(IntPtr cameraMatrix, IntPtr distCoeffs, int imageWidth, int imageHeight, double alpha, int newImageWidth, int newImageHeight, int centerPrincipalPoint, out int roiX, out int roiY, out int roiWidth, out int roiHeight, out IntPtr newCameraMatrix);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_calibration_matrix_values")]
        internal static partial int Calib3DCalibrationMatrixValues(IntPtr cameraMatrix, int imageWidth, int imageHeight, double apertureWidth, double apertureHeight, out double fovX, out double fovY, out double focalLength, out double principalPointX, out double principalPointY, out double aspectRatio);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_stereo_rectify_uncalibrated")]
        internal static partial int Calib3DStereoRectifyUncalibrated(IntPtr points1, IntPtr points2, IntPtr fundamental, int imageWidth, int imageHeight, IntPtr h1, IntPtr h2, double threshold, out int success);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_calibrate_hand_eye")]
        internal static partial int Calib3DCalibrateHandEye(IntPtr* rGripper2Base, IntPtr* tGripper2Base, IntPtr* rTarget2Cam, IntPtr* tTarget2Cam, int poseCount, IntPtr rCam2Gripper, IntPtr tCam2Gripper, int method);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_calibrate_robot_world_hand_eye")]
        internal static partial int Calib3DCalibrateRobotWorldHandEye(IntPtr* rWorld2Cam, IntPtr* tWorld2Cam, IntPtr* rBase2Gripper, IntPtr* tBase2Gripper, int poseCount, IntPtr rBase2World, IntPtr tBase2World, IntPtr rGripper2Cam, IntPtr tGripper2Cam, int method);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_init_camera_matrix_2d")]
        internal static partial int Calib3DInitCameraMatrix2D(int* objectPointOffsets, int objectPointGroupCount, Calib3DPoint3fNative* objectPoints, int objectPointCount, int* imagePointOffsets, int imagePointGroupCount, Calib3DPoint2fNative* imagePoints, int imagePointCount, int imageWidth, int imageHeight, double aspectRatio, IntPtr cameraMatrix);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_calibrate_camera")]
        internal static partial int Calib3DCalibrateCamera(int* objectPointOffsets, int objectPointGroupCount, Calib3DPoint3fNative* objectPoints, int objectPointCount, int* imagePointOffsets, int imagePointGroupCount, Calib3DPoint2fNative* imagePoints, int imagePointCount, int imageWidth, int imageHeight, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvecs, IntPtr tvecs, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out double reprojectionError);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_calibrate_camera_extended")]
        internal static partial int Calib3DCalibrateCameraExtended(int* objectPointOffsets, int objectPointGroupCount, Calib3DPoint3fNative* objectPoints, int objectPointCount, int* imagePointOffsets, int imagePointGroupCount, Calib3DPoint2fNative* imagePoints, int imagePointCount, int imageWidth, int imageHeight, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvecs, IntPtr tvecs, IntPtr stdDeviationsIntrinsics, IntPtr stdDeviationsExtrinsics, IntPtr perViewErrors, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out double reprojectionError);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_calibrate_camera_ro")]
        internal static partial int Calib3DCalibrateCameraRO(int* objectPointOffsets, int objectPointGroupCount, Calib3DPoint3fNative* objectPoints, int objectPointCount, int* imagePointOffsets, int imagePointGroupCount, Calib3DPoint2fNative* imagePoints, int imagePointCount, int imageWidth, int imageHeight, int iFixedPoint, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvecs, IntPtr tvecs, IntPtr newObjectPoints, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out double reprojectionError);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_calibrate_camera_ro_extended")]
        internal static partial int Calib3DCalibrateCameraROExtended(int* objectPointOffsets, int objectPointGroupCount, Calib3DPoint3fNative* objectPoints, int objectPointCount, int* imagePointOffsets, int imagePointGroupCount, Calib3DPoint2fNative* imagePoints, int imagePointCount, int imageWidth, int imageHeight, int iFixedPoint, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvecs, IntPtr tvecs, IntPtr newObjectPoints, IntPtr stdDeviationsIntrinsics, IntPtr stdDeviationsExtrinsics, IntPtr stdDeviationsObjectPoints, IntPtr perViewErrors, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out double reprojectionError);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_stereo_calibrate")]
        internal static partial int Calib3DStereoCalibrate(int* objectPointOffsets, int objectPointGroupCount, Calib3DPoint3fNative* objectPoints, int objectPointCount, int* imagePoint1Offsets, int imagePoint1GroupCount, Calib3DPoint2fNative* imagePoints1, int imagePoint1Count, int* imagePoint2Offsets, int imagePoint2GroupCount, Calib3DPoint2fNative* imagePoints2, int imagePoint2Count, IntPtr cameraMatrix1, IntPtr distCoeffs1, IntPtr cameraMatrix2, IntPtr distCoeffs2, int imageWidth, int imageHeight, IntPtr r, IntPtr t, IntPtr e, IntPtr f, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out double reprojectionError);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_stereo_calibrate_extended")]
        internal static partial int Calib3DStereoCalibrateExtended(int* objectPointOffsets, int objectPointGroupCount, Calib3DPoint3fNative* objectPoints, int objectPointCount, int* imagePoint1Offsets, int imagePoint1GroupCount, Calib3DPoint2fNative* imagePoints1, int imagePoint1Count, int* imagePoint2Offsets, int imagePoint2GroupCount, Calib3DPoint2fNative* imagePoints2, int imagePoint2Count, IntPtr cameraMatrix1, IntPtr distCoeffs1, IntPtr cameraMatrix2, IntPtr distCoeffs2, int imageWidth, int imageHeight, IntPtr r, IntPtr t, IntPtr e, IntPtr f, IntPtr rvecs, IntPtr tvecs, IntPtr perViewErrors, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out double reprojectionError);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_fisheye_calibrate")]
        internal static partial int Calib3DFisheyeCalibrate(int* objectPointOffsets, int objectPointGroupCount, Calib3DPoint3fNative* objectPoints, int objectPointCount, int* imagePointOffsets, int imagePointGroupCount, Calib3DPoint2fNative* imagePoints, int imagePointCount, int imageWidth, int imageHeight, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvecs, IntPtr tvecs, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out double reprojectionError);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_fisheye_stereo_calibrate")]
        internal static partial int Calib3DFisheyeStereoCalibrate(int* objectPointOffsets, int objectPointGroupCount, Calib3DPoint3fNative* objectPoints, int objectPointCount, int* imagePoint1Offsets, int imagePoint1GroupCount, Calib3DPoint2fNative* imagePoints1, int imagePoint1Count, int* imagePoint2Offsets, int imagePoint2GroupCount, Calib3DPoint2fNative* imagePoints2, int imagePoint2Count, IntPtr cameraMatrix1, IntPtr distCoeffs1, IntPtr cameraMatrix2, IntPtr distCoeffs2, int imageWidth, int imageHeight, IntPtr r, IntPtr t, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out double reprojectionError);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_fisheye_stereo_calibrate_extended")]
        internal static partial int Calib3DFisheyeStereoCalibrateExtended(int* objectPointOffsets, int objectPointGroupCount, Calib3DPoint3fNative* objectPoints, int objectPointCount, int* imagePoint1Offsets, int imagePoint1GroupCount, Calib3DPoint2fNative* imagePoints1, int imagePoint1Count, int* imagePoint2Offsets, int imagePoint2GroupCount, Calib3DPoint2fNative* imagePoints2, int imagePoint2Count, IntPtr cameraMatrix1, IntPtr distCoeffs1, IntPtr cameraMatrix2, IntPtr distCoeffs2, int imageWidth, int imageHeight, IntPtr r, IntPtr t, IntPtr rvecs, IntPtr tvecs, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out double reprojectionError);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_register_cameras")]
        internal static partial int Calib3DRegisterCameras(int* objectPoint1Offsets, int objectPoint1GroupCount, Calib3DPoint3fNative* objectPoints1, int objectPoint1Count, int* objectPoint2Offsets, int objectPoint2GroupCount, Calib3DPoint3fNative* objectPoints2, int objectPoint2Count, int* imagePoint1Offsets, int imagePoint1GroupCount, Calib3DPoint2fNative* imagePoints1, int imagePoint1Count, int* imagePoint2Offsets, int imagePoint2GroupCount, Calib3DPoint2fNative* imagePoints2, int imagePoint2Count, IntPtr cameraMatrix1, IntPtr distCoeffs1, int cameraModel1, IntPtr cameraMatrix2, IntPtr distCoeffs2, int cameraModel2, IntPtr r, IntPtr t, IntPtr e, IntPtr f, IntPtr perViewErrors, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out double reprojectionError);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_register_cameras_extended")]
        internal static partial int Calib3DRegisterCamerasExtended(int* objectPoint1Offsets, int objectPoint1GroupCount, Calib3DPoint3fNative* objectPoints1, int objectPoint1Count, int* objectPoint2Offsets, int objectPoint2GroupCount, Calib3DPoint3fNative* objectPoints2, int objectPoint2Count, int* imagePoint1Offsets, int imagePoint1GroupCount, Calib3DPoint2fNative* imagePoints1, int imagePoint1Count, int* imagePoint2Offsets, int imagePoint2GroupCount, Calib3DPoint2fNative* imagePoints2, int imagePoint2Count, IntPtr cameraMatrix1, IntPtr distCoeffs1, int cameraModel1, IntPtr cameraMatrix2, IntPtr distCoeffs2, int cameraModel2, IntPtr r, IntPtr t, IntPtr e, IntPtr f, IntPtr rvecs, IntPtr tvecs, IntPtr perViewErrors, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out double reprojectionError);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_calibrate_multiview")]
        internal static partial int Calib3DCalibrateMultiview(int* objectPointOffsets, int frameCount, Calib3DPoint3fNative* objectPoints, int objectPointCount, int* imagePointOffsets, int cameraCount, int imageFrameCount, Calib3DPoint2fNative* imagePoints, int imagePointCount, int* imageWidths, int* imageHeights, byte* detectionMask, int* cameraModels, IntPtr* cameraMatrices, IntPtr* distCoeffs, IntPtr* rotationVectors, IntPtr* translationVectors, int* flagsForIntrinsics, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out double reprojectionError);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_calibrate_multiview_extended")]
        internal static partial int Calib3DCalibrateMultiviewExtended(int* objectPointOffsets, int frameCount, Calib3DPoint3fNative* objectPoints, int objectPointCount, int* imagePointOffsets, int cameraCount, int imageFrameCount, Calib3DPoint2fNative* imagePoints, int imagePointCount, int* imageWidths, int* imageHeights, byte* detectionMask, int* cameraModels, IntPtr* cameraMatrices, IntPtr* distCoeffs, IntPtr* rotationVectors, IntPtr* translationVectors, IntPtr initializationPairs, IntPtr* rvecs0, IntPtr* tvecs0, IntPtr perFrameErrors, int* flagsForIntrinsics, int flags, int criteriaType, int criteriaMaxCount, double criteriaEpsilon, out double reprojectionError);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_rectify3_collinear")]
        internal static partial int Calib3DRectify3Collinear(IntPtr cameraMatrix1, IntPtr distCoeffs1, IntPtr cameraMatrix2, IntPtr distCoeffs2, IntPtr cameraMatrix3, IntPtr distCoeffs3, int* imagePoint1Offsets, int imagePoint1GroupCount, Calib3DPoint2fNative* imagePoints1, int imagePoint1Count, int* imagePoint3Offsets, int imagePoint3GroupCount, Calib3DPoint2fNative* imagePoints3, int imagePoint3Count, int imageWidth, int imageHeight, IntPtr r12, IntPtr t12, IntPtr r13, IntPtr t13, IntPtr r1, IntPtr r2, IntPtr r3, IntPtr p1, IntPtr p2, IntPtr p3, IntPtr q, double alpha, int newImageWidth, int newImageHeight, int flags, out int roi1X, out int roi1Y, out int roi1Width, out int roi1Height, out int roi2X, out int roi2Y, out int roi2Width, out int roi2Height, out float scale);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_create")]
        internal static partial int StereoBMCreate(int numDisparities, int blockSize, out IntPtr stereoBM);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_release")]
        internal static partial void StereoBMRelease(IntPtr stereoBM);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_compute")]
        internal static partial int StereoBMCompute(IntPtr stereoBM, IntPtr left, IntPtr right, IntPtr disparity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_min_disparity")]
        internal static partial int StereoBMGetMinDisparity(IntPtr stereoBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_min_disparity")]
        internal static partial int StereoBMSetMinDisparity(IntPtr stereoBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_num_disparities")]
        internal static partial int StereoBMGetNumDisparities(IntPtr stereoBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_num_disparities")]
        internal static partial int StereoBMSetNumDisparities(IntPtr stereoBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_block_size")]
        internal static partial int StereoBMGetBlockSize(IntPtr stereoBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_block_size")]
        internal static partial int StereoBMSetBlockSize(IntPtr stereoBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_speckle_window_size")]
        internal static partial int StereoBMGetSpeckleWindowSize(IntPtr stereoBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_speckle_window_size")]
        internal static partial int StereoBMSetSpeckleWindowSize(IntPtr stereoBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_speckle_range")]
        internal static partial int StereoBMGetSpeckleRange(IntPtr stereoBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_speckle_range")]
        internal static partial int StereoBMSetSpeckleRange(IntPtr stereoBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_disp12_max_diff")]
        internal static partial int StereoBMGetDisp12MaxDiff(IntPtr stereoBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_disp12_max_diff")]
        internal static partial int StereoBMSetDisp12MaxDiff(IntPtr stereoBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_pre_filter_type")]
        internal static partial int StereoBMGetPreFilterType(IntPtr stereoBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_pre_filter_type")]
        internal static partial int StereoBMSetPreFilterType(IntPtr stereoBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_pre_filter_size")]
        internal static partial int StereoBMGetPreFilterSize(IntPtr stereoBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_pre_filter_size")]
        internal static partial int StereoBMSetPreFilterSize(IntPtr stereoBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_pre_filter_cap")]
        internal static partial int StereoBMGetPreFilterCap(IntPtr stereoBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_pre_filter_cap")]
        internal static partial int StereoBMSetPreFilterCap(IntPtr stereoBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_texture_threshold")]
        internal static partial int StereoBMGetTextureThreshold(IntPtr stereoBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_texture_threshold")]
        internal static partial int StereoBMSetTextureThreshold(IntPtr stereoBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_uniqueness_ratio")]
        internal static partial int StereoBMGetUniquenessRatio(IntPtr stereoBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_uniqueness_ratio")]
        internal static partial int StereoBMSetUniquenessRatio(IntPtr stereoBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_smaller_block_size")]
        internal static partial int StereoBMGetSmallerBlockSize(IntPtr stereoBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_smaller_block_size")]
        internal static partial int StereoBMSetSmallerBlockSize(IntPtr stereoBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_roi1")]
        internal static partial int StereoBMGetROI1(IntPtr stereoBM, out int x, out int y, out int width, out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_roi1")]
        internal static partial int StereoBMSetROI1(IntPtr stereoBM, int x, int y, int width, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_get_roi2")]
        internal static partial int StereoBMGetROI2(IntPtr stereoBM, out int x, out int y, out int width, out int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_bm_set_roi2")]
        internal static partial int StereoBMSetROI2(IntPtr stereoBM, int x, int y, int width, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_create")]
        internal static partial int StereoSGBMCreate(int minDisparity, int numDisparities, int blockSize, int p1, int p2, int disp12MaxDiff, int preFilterCap, int uniquenessRatio, int speckleWindowSize, int speckleRange, int mode, out IntPtr stereoSGBM);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_release")]
        internal static partial void StereoSGBMRelease(IntPtr stereoSGBM);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_compute")]
        internal static partial int StereoSGBMCompute(IntPtr stereoSGBM, IntPtr left, IntPtr right, IntPtr disparity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_get_min_disparity")]
        internal static partial int StereoSGBMGetMinDisparity(IntPtr stereoSGBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_set_min_disparity")]
        internal static partial int StereoSGBMSetMinDisparity(IntPtr stereoSGBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_get_num_disparities")]
        internal static partial int StereoSGBMGetNumDisparities(IntPtr stereoSGBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_set_num_disparities")]
        internal static partial int StereoSGBMSetNumDisparities(IntPtr stereoSGBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_get_block_size")]
        internal static partial int StereoSGBMGetBlockSize(IntPtr stereoSGBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_set_block_size")]
        internal static partial int StereoSGBMSetBlockSize(IntPtr stereoSGBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_get_speckle_window_size")]
        internal static partial int StereoSGBMGetSpeckleWindowSize(IntPtr stereoSGBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_set_speckle_window_size")]
        internal static partial int StereoSGBMSetSpeckleWindowSize(IntPtr stereoSGBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_get_speckle_range")]
        internal static partial int StereoSGBMGetSpeckleRange(IntPtr stereoSGBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_set_speckle_range")]
        internal static partial int StereoSGBMSetSpeckleRange(IntPtr stereoSGBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_get_disp12_max_diff")]
        internal static partial int StereoSGBMGetDisp12MaxDiff(IntPtr stereoSGBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_set_disp12_max_diff")]
        internal static partial int StereoSGBMSetDisp12MaxDiff(IntPtr stereoSGBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_get_pre_filter_cap")]
        internal static partial int StereoSGBMGetPreFilterCap(IntPtr stereoSGBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_set_pre_filter_cap")]
        internal static partial int StereoSGBMSetPreFilterCap(IntPtr stereoSGBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_get_uniqueness_ratio")]
        internal static partial int StereoSGBMGetUniquenessRatio(IntPtr stereoSGBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_set_uniqueness_ratio")]
        internal static partial int StereoSGBMSetUniquenessRatio(IntPtr stereoSGBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_get_p1")]
        internal static partial int StereoSGBMGetP1(IntPtr stereoSGBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_set_p1")]
        internal static partial int StereoSGBMSetP1(IntPtr stereoSGBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_get_p2")]
        internal static partial int StereoSGBMGetP2(IntPtr stereoSGBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_set_p2")]
        internal static partial int StereoSGBMSetP2(IntPtr stereoSGBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_get_mode")]
        internal static partial int StereoSGBMGetMode(IntPtr stereoSGBM, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_sgbm_set_mode")]
        internal static partial int StereoSGBMSetMode(IntPtr stereoSGBM, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_release")]
        internal static partial void StereoMatcherRelease(IntPtr stereoMatcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_compute")]
        internal static partial int StereoMatcherCompute(IntPtr stereoMatcher, IntPtr left, IntPtr right, IntPtr disparity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_get_min_disparity")]
        internal static partial int StereoMatcherGetMinDisparity(IntPtr stereoMatcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_set_min_disparity")]
        internal static partial int StereoMatcherSetMinDisparity(IntPtr stereoMatcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_get_num_disparities")]
        internal static partial int StereoMatcherGetNumDisparities(IntPtr stereoMatcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_set_num_disparities")]
        internal static partial int StereoMatcherSetNumDisparities(IntPtr stereoMatcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_get_block_size")]
        internal static partial int StereoMatcherGetBlockSize(IntPtr stereoMatcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_set_block_size")]
        internal static partial int StereoMatcherSetBlockSize(IntPtr stereoMatcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_get_speckle_window_size")]
        internal static partial int StereoMatcherGetSpeckleWindowSize(IntPtr stereoMatcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_set_speckle_window_size")]
        internal static partial int StereoMatcherSetSpeckleWindowSize(IntPtr stereoMatcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_get_speckle_range")]
        internal static partial int StereoMatcherGetSpeckleRange(IntPtr stereoMatcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_set_speckle_range")]
        internal static partial int StereoMatcherSetSpeckleRange(IntPtr stereoMatcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_get_disp12_max_diff")]
        internal static partial int StereoMatcherGetDisp12MaxDiff(IntPtr stereoMatcher, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stereo_matcher_set_disp12_max_diff")]
        internal static partial int StereoMatcherSetDisp12MaxDiff(IntPtr stereoMatcher, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_create")]
        internal static partial int Calib3DSubdiv2DCreate(out IntPtr subdiv);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_create_rect")]
        internal static partial int Calib3DSubdiv2DCreateRect(int x, int y, int width, int height, out IntPtr subdiv);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_create_rect2f")]
        internal static partial int Calib3DSubdiv2DCreateRect2f(float x, float y, float width, float height, out IntPtr subdiv);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_release")]
        internal static partial void Calib3DSubdiv2DRelease(IntPtr subdiv);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_init_delaunay")]
        internal static partial int Calib3DSubdiv2DInitDelaunay(IntPtr subdiv, int x, int y, int width, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_init_delaunay_rect2f")]
        internal static partial int Calib3DSubdiv2DInitDelaunayRect2f(IntPtr subdiv, float x, float y, float width, float height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_insert")]
        internal static partial int Calib3DSubdiv2DInsert(IntPtr subdiv, float x, float y, out int vertex);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_insert_points")]
        internal static partial int Calib3DSubdiv2DInsertPoints(IntPtr subdiv, Calib3DPoint2fNative* points, int pointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_locate")]
        internal static partial int Calib3DSubdiv2DLocate(IntPtr subdiv, float x, float y, out int location, out int edge, out int vertex);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_find_nearest")]
        internal static partial int Calib3DSubdiv2DFindNearest(IntPtr subdiv, float x, float y, out int vertex, out float nearestX, out float nearestY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_get_edge_list_count")]
        internal static partial int Calib3DSubdiv2DGetEdgeListCount(IntPtr subdiv, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_get_edge_list_fill")]
        internal static partial int Calib3DSubdiv2DGetEdgeListFill(IntPtr subdiv, Calib3DVec4fNative* values, int capacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_get_leading_edge_list_count")]
        internal static partial int Calib3DSubdiv2DGetLeadingEdgeListCount(IntPtr subdiv, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_get_leading_edge_list_fill")]
        internal static partial int Calib3DSubdiv2DGetLeadingEdgeListFill(IntPtr subdiv, int* values, int capacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_get_triangle_list_count")]
        internal static partial int Calib3DSubdiv2DGetTriangleListCount(IntPtr subdiv, out int count);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_get_triangle_list_fill")]
        internal static partial int Calib3DSubdiv2DGetTriangleListFill(IntPtr subdiv, Calib3DVec6fNative* values, int capacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_get_voronoi_facet_list_count")]
        internal static partial int Calib3DSubdiv2DGetVoronoiFacetListCount(IntPtr subdiv, int* indices, int indexCount, out int facetCount, out int pointCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_get_voronoi_facet_list_fill")]
        internal static partial int Calib3DSubdiv2DGetVoronoiFacetListFill(IntPtr subdiv, int* indices, int indexCount, int* facetOffsets, int facetOffsetCapacity, Calib3DPoint2fNative* points, int pointCapacity, Calib3DPoint2fNative* centers, int centerCapacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_get_vertex")]
        internal static partial int Calib3DSubdiv2DGetVertex(IntPtr subdiv, int vertex, out float x, out float y, out int firstEdge);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_get_edge")]
        internal static partial int Calib3DSubdiv2DGetEdge(IntPtr subdiv, int edge, int nextEdgeType, out int relatedEdge);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_next_edge")]
        internal static partial int Calib3DSubdiv2DNextEdge(IntPtr subdiv, int edge, out int nextEdge);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_rotate_edge")]
        internal static partial int Calib3DSubdiv2DRotateEdge(IntPtr subdiv, int edge, int rotate, out int rotatedEdge);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_sym_edge")]
        internal static partial int Calib3DSubdiv2DSymEdge(IntPtr subdiv, int edge, out int symmetricEdge);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_edge_org")]
        internal static partial int Calib3DSubdiv2DEdgeOrg(IntPtr subdiv, int edge, out int vertex, out float x, out float y);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_subdiv2d_edge_dst")]
        internal static partial int Calib3DSubdiv2DEdgeDst(IntPtr subdiv, int edge, out int vertex, out float x, out float y);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_usac_params_get_default")]
        internal static partial int Calib3DUsacParamsGetDefault(out Calib3DUsacParamsNative parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_find_homography_usac")]
        internal static partial int Calib3DFindHomographyUsac(IntPtr srcPoints, IntPtr dstPoints, IntPtr mask, Calib3DUsacParamsNative* parameters, out IntPtr homography);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_solve_pnp_ransac_usac")]
        internal static partial int Calib3DSolvePnPRansacUsac(IntPtr objectPoints, IntPtr imagePoints, IntPtr cameraMatrix, IntPtr distCoeffs, IntPtr rvec, IntPtr tvec, IntPtr inliers, Calib3DUsacParamsNative* parameters, out int solved);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_find_fundamental_mat_usac")]
        internal static partial int Calib3DFindFundamentalMatUsac(IntPtr points1, IntPtr points2, IntPtr mask, Calib3DUsacParamsNative* parameters, out IntPtr fundamental);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_find_essential_mat_usac")]
        internal static partial int Calib3DFindEssentialMatUsac(IntPtr points1, IntPtr points2, IntPtr cameraMatrix1, IntPtr cameraMatrix2, IntPtr distCoeffs1, IntPtr distCoeffs2, IntPtr mask, Calib3DUsacParamsNative* parameters, out IntPtr essential);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_estimate_affine_2d_usac")]
        internal static partial int Calib3DEstimateAffine2DUsac(IntPtr source, IntPtr destination, IntPtr transform, IntPtr inliers, Calib3DUsacParamsNative* parameters);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_calib3d_fisheye_stereo_rectify")]
        internal static partial int Calib3DFisheyeStereoRectify(IntPtr cameraMatrix1, IntPtr distCoeffs1, IntPtr cameraMatrix2, IntPtr distCoeffs2, int imageWidth, int imageHeight, IntPtr r, IntPtr t, IntPtr r1, IntPtr r2, IntPtr p1, IntPtr p2, IntPtr q, int flags, int newImageWidth, int newImageHeight, double balance, double fovScale);
    }
}
#endif
