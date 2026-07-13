# ImgProc Boundary / ImgProc 边界

This file tracks implemented `imgproc` ABI boundaries.

本文档记录已经落地的 `imgproc` ABI 边界。

## Navigation / 导航

- [ImgProc Geometry Guide](imgproc-geometry-guide.md)
- [ImgProc Filter Transform Guide](imgproc-filter-transform-guide.md)
- [ImgProc Segmentation Contours Features Guide](imgproc-segmentation-contours-features-guide.md)
- [ImgProc Hough Features CLAHE Guide](imgproc-hough-features-clahe-guide.md)

## Implemented Native APIs / 已实现 Native API

- `jyppx_ocv_imgproc_cvt_color`
- `jyppx_ocv_imgproc_resize`
- `jyppx_ocv_imgproc_threshold`
- `jyppx_ocv_imgproc_adaptive_threshold`
- `jyppx_ocv_imgproc_integral`
- `jyppx_ocv_imgproc_integral2`
- `jyppx_ocv_imgproc_integral3`
- `jyppx_ocv_imgproc_distance_transform`
- `jyppx_ocv_imgproc_distance_transform_with_labels`
- `jyppx_ocv_imgproc_flood_fill`
- `jyppx_ocv_imgproc_flood_fill_mask`
- `jyppx_ocv_imgproc_connected_components`
- `jyppx_ocv_imgproc_connected_components_with_algorithm`
- `jyppx_ocv_imgproc_connected_components_with_stats`
- `jyppx_ocv_imgproc_connected_components_with_stats_with_algorithm`
- `jyppx_ocv_imgproc_equalize_hist`
- `jyppx_ocv_imgproc_clahe_create`
- `jyppx_ocv_imgproc_clahe_release`
- `jyppx_ocv_imgproc_clahe_apply`
- `jyppx_ocv_imgproc_clahe_get_clip_limit`
- `jyppx_ocv_imgproc_clahe_set_clip_limit`
- `jyppx_ocv_imgproc_clahe_get_tiles_grid_size`
- `jyppx_ocv_imgproc_clahe_set_tiles_grid_size`
- `jyppx_ocv_imgproc_clahe_get_bit_shift`
- `jyppx_ocv_imgproc_clahe_set_bit_shift`
- `jyppx_ocv_imgproc_clahe_collect_garbage`
- `jyppx_ocv_imgproc_corner_harris`
- `jyppx_ocv_imgproc_corner_min_eigen_val`
- `jyppx_ocv_imgproc_corner_eigen_vals_and_vecs`
- `jyppx_ocv_imgproc_pre_corner_detect`
- `jyppx_ocv_imgproc_corner_sub_pix`
- `jyppx_ocv_imgproc_good_features_to_track_count`
- `jyppx_ocv_imgproc_good_features_to_track_fill`
- `jyppx_ocv_imgproc_hough_lines_count`
- `jyppx_ocv_imgproc_hough_lines_fill`
- `jyppx_ocv_imgproc_hough_lines_p_count`
- `jyppx_ocv_imgproc_hough_lines_p_fill`
- `jyppx_ocv_imgproc_hough_lines_point_set_count`
- `jyppx_ocv_imgproc_hough_lines_point_set_fill`
- `jyppx_ocv_imgproc_hough_circles_count`
- `jyppx_ocv_imgproc_hough_circles_fill`
- `jyppx_ocv_imgproc_calc_hist_uniform`
- `jyppx_ocv_imgproc_calc_back_project_uniform`
- `jyppx_ocv_imgproc_compare_hist`
- `jyppx_ocv_imgproc_line_segment_detector_create`
- `jyppx_ocv_imgproc_line_segment_detector_release`
- `jyppx_ocv_imgproc_line_segment_detector_detect`
- `jyppx_ocv_imgproc_line_segment_detector_detect_count`
- `jyppx_ocv_imgproc_line_segment_detector_detect_fill`
- `jyppx_ocv_imgproc_line_segment_detector_draw_segments`
- `jyppx_ocv_imgproc_line_segment_detector_draw_segments_array`
- `jyppx_ocv_imgproc_line_segment_detector_compare_segments`
- `jyppx_ocv_imgproc_line_segment_detector_compare_segments_array`
- `jyppx_ocv_imgproc_gaussian_blur`
- `jyppx_ocv_imgproc_blur`
- `jyppx_ocv_imgproc_box_filter`
- `jyppx_ocv_imgproc_sqr_box_filter`
- `jyppx_ocv_imgproc_median_blur`
- `jyppx_ocv_imgproc_bilateral_filter`
- `jyppx_ocv_imgproc_filter2d`
- `jyppx_ocv_imgproc_sep_filter2d`
- `jyppx_ocv_imgproc_sobel`
- `jyppx_ocv_imgproc_scharr`
- `jyppx_ocv_imgproc_laplacian`
- `jyppx_ocv_imgproc_canny`
- `jyppx_ocv_imgproc_canny_derivatives`
- `jyppx_ocv_imgproc_get_gaussian_kernel`
- `jyppx_ocv_imgproc_get_deriv_kernels`
- `jyppx_ocv_imgproc_get_gabor_kernel`
- `jyppx_ocv_imgproc_pyr_down`
- `jyppx_ocv_imgproc_pyr_up`
- `jyppx_ocv_imgproc_warp_affine`
- `jyppx_ocv_imgproc_warp_perspective`
- `jyppx_ocv_imgproc_get_rotation_matrix2d`
- `jyppx_ocv_imgproc_get_affine_transform`
- `jyppx_ocv_imgproc_get_perspective_transform`
- `jyppx_ocv_imgproc_invert_affine_transform`
- `jyppx_ocv_imgproc_remap`
- `jyppx_ocv_imgproc_convert_maps`
- `jyppx_ocv_imgproc_get_structuring_element`
- `jyppx_ocv_imgproc_erode`
- `jyppx_ocv_imgproc_dilate`
- `jyppx_ocv_imgproc_morphology_ex`
- `jyppx_ocv_imgproc_line`
- `jyppx_ocv_imgproc_arrowed_line`
- `jyppx_ocv_imgproc_clip_line_rect`
- `jyppx_ocv_imgproc_polylines`
- `jyppx_ocv_imgproc_fill_poly`
- `jyppx_ocv_imgproc_ellipse2_poly_count`
- `jyppx_ocv_imgproc_ellipse2_poly_fill`
- `jyppx_ocv_imgproc_contour_area`
- `jyppx_ocv_imgproc_find_contours_count`
- `jyppx_ocv_imgproc_find_contours_fill`
- `jyppx_ocv_imgproc_draw_contours`
- `jyppx_ocv_imgproc_moments_points`
- `jyppx_ocv_imgproc_moments_mat`
- `jyppx_ocv_imgproc_hu_moments`
- `jyppx_ocv_imgproc_arc_length`
- `jyppx_ocv_imgproc_approx_poly_dp_count`
- `jyppx_ocv_imgproc_approx_poly_dp_fill`
- `jyppx_ocv_imgproc_approx_poly_n_count`
- `jyppx_ocv_imgproc_approx_poly_n_fill`
- `jyppx_ocv_imgproc_bounding_rect`
- `jyppx_ocv_imgproc_is_contour_convex`
- `jyppx_ocv_imgproc_convex_hull_count`
- `jyppx_ocv_imgproc_convex_hull_fill`
- `jyppx_ocv_imgproc_convex_hull_indices_count`
- `jyppx_ocv_imgproc_convex_hull_indices_fill`
- `jyppx_ocv_imgproc_convexity_defects_count`
- `jyppx_ocv_imgproc_convexity_defects_fill`
- `jyppx_ocv_imgproc_min_enclosing_circle`
- `jyppx_ocv_imgproc_point_polygon_test`
- `jyppx_ocv_imgproc_match_shapes`
- `jyppx_ocv_imgproc_min_area_rect`
- `jyppx_ocv_imgproc_box_points`
- `jyppx_ocv_imgproc_fit_ellipse`
- `jyppx_ocv_imgproc_fit_ellipse_ams`
- `jyppx_ocv_imgproc_fit_ellipse_direct`
- `jyppx_ocv_imgproc_rotated_rectangle_intersection_count`
- `jyppx_ocv_imgproc_rotated_rectangle_intersection_fill`
- `jyppx_ocv_imgproc_get_closest_ellipse_points`
- `jyppx_ocv_imgproc_min_enclosing_triangle`
- `jyppx_ocv_imgproc_min_enclosing_convex_polygon`
- `jyppx_ocv_imgproc_intersect_convex_convex_count`
- `jyppx_ocv_imgproc_intersect_convex_convex_fill`
- `jyppx_ocv_imgproc_fit_line_2d`
- `jyppx_ocv_imgproc_rectangle`
- `jyppx_ocv_imgproc_rectangle_by_rect`
- `jyppx_ocv_imgproc_circle`
- `jyppx_ocv_imgproc_ellipse`
- `jyppx_ocv_imgproc_put_text`
- `jyppx_ocv_imgproc_get_text_size`

## C# Surface / C# 表层接口

Namespace:

命名空间：

```csharp
namespace OpenCvSharp.ImgProc
```

Static class:

静态类：

```csharp
public static class Cv2
```

Implemented methods:

已实现方法：

```csharp
public static void CvtColor(Mat src, Mat dst, ColorConversionCodes code, int dstCn = 0);
public static void Resize(Mat src, Mat dst, Size dsize, double fx = 0, double fy = 0, InterpolationFlags interpolation = InterpolationFlags.Linear);
public static double Threshold(Mat src, Mat dst, double thresh, double maxval, ThresholdTypes type);
public static void AdaptiveThreshold(Mat src, Mat dst, double maxValue, AdaptiveThresholdTypes adaptiveMethod, ThresholdTypes thresholdType, int blockSize, double c);
public static void Integral(Mat src, Mat sum, int sdepth = -1);
public static void Integral2(Mat src, Mat sum, Mat sqsum, int sdepth = -1, int sqdepth = -1);
public static void Integral3(Mat src, Mat sum, Mat sqsum, Mat tilted, int sdepth = -1, int sqdepth = -1);
public static void DistanceTransform(Mat src, Mat dst, DistanceTypes distanceType, DistanceTransformMasks maskSize, int dstType = MatType.CV_32F);
public static void DistanceTransform(Mat src, Mat dst, Mat labels, DistanceTypes distanceType, DistanceTransformMasks maskSize, DistanceTransformLabelTypes labelType = DistanceTransformLabelTypes.CComp);
public static int FloodFill(Mat image, Point seedPoint, Scalar newVal, out Rect rect, Scalar? loDiff = null, Scalar? upDiff = null, FloodFillFlags flags = (FloodFillFlags)4);
public static int FloodFill(Mat image, Mat mask, Point seedPoint, Scalar newVal, out Rect rect, Scalar? loDiff = null, Scalar? upDiff = null, FloodFillFlags flags = (FloodFillFlags)4);
public static int ConnectedComponents(Mat image, Mat labels, int connectivity = 8, int ltype = MatType.CV_32S);
public static int ConnectedComponentsWithAlgorithm(Mat image, Mat labels, int connectivity, int ltype, ConnectedComponentsAlgorithmsTypes ccltype);
public static int ConnectedComponentsWithStats(Mat image, Mat labels, Mat stats, Mat centroids, int connectivity = 8, int ltype = MatType.CV_32S);
public static int ConnectedComponentsWithStatsWithAlgorithm(Mat image, Mat labels, Mat stats, Mat centroids, int connectivity, int ltype, ConnectedComponentsAlgorithmsTypes ccltype);
public static void EqualizeHist(Mat src, Mat dst);
public static CLAHE CreateCLAHE(double clipLimit = 40.0, Size? tileGridSize = null);
public static void CornerHarris(Mat src, Mat dst, int blockSize, int ksize, double k, BorderTypes borderType = BorderTypes.Default);
public static void CornerMinEigenVal(Mat src, Mat dst, int blockSize, int ksize = 3, BorderTypes borderType = BorderTypes.Default);
public static void CornerEigenValsAndVecs(Mat src, Mat dst, int blockSize, int ksize, BorderTypes borderType = BorderTypes.Default);
public static void PreCornerDetect(Mat src, Mat dst, int ksize, BorderTypes borderType = BorderTypes.Default);
public static void CornerSubPix(Mat image, Point2f[] corners, Size winSize, Size zeroZone, TermCriteria criteria);
public static Point2f[] GoodFeaturesToTrack(Mat image, int maxCorners, double qualityLevel, double minDistance, Mat? mask = null, int blockSize = 3, int gradientSize = 3, bool useHarrisDetector = false, double k = 0.04);
public static HoughLine[] HoughLines(Mat image, double rho, double theta, int threshold, double srn = 0.0, double stn = 0.0, double minTheta = 0.0, double maxTheta = Math.PI, bool useEdgeval = false);
public static Vec4i[] HoughLinesP(Mat image, double rho, double theta, int threshold, double minLineLength = 0.0, double maxLineGap = 0.0);
public static HoughLinePointSet[] HoughLinesPointSet(Point[] points, int linesMax, int threshold, double minRho, double maxRho, double rhoStep, double minTheta, double maxTheta, double thetaStep);
public static HoughCircle[] HoughCircles(Mat image, HoughModes method, double dp, double minDist, double param1 = 100.0, double param2 = 100.0, int minRadius = 0, int maxRadius = 0);
public static void CalcHist(Mat image, int[] channels, Mat? mask, Mat hist, int[] histSize, float[] ranges, bool accumulate = false);
public static void CalcHist(Mat image, int channel, Mat? mask, Mat hist, int histSize, float rangeMin, float rangeMax, bool accumulate = false);
public static void CalcBackProject(Mat image, int[] channels, Mat hist, Mat backProject, float[] ranges, double scale = 1.0);
public static void CalcBackProject(Mat image, int channel, Mat hist, Mat backProject, float rangeMin, float rangeMax, double scale = 1.0);
public static double CompareHist(Mat h1, Mat h2, HistogramComparisonTypes method);
public static LineSegmentDetector CreateLineSegmentDetector(LineSegmentDetectorModes refine = LineSegmentDetectorModes.Standard, double scale = 0.8, double sigmaScale = 0.6, double quant = 2.0, double angTh = 22.5, double logEps = 0.0, double densityTh = 0.7, int nBins = 1024);
public static void GaussianBlur(Mat src, Mat dst, Size ksize, double sigmaX, double sigmaY = 0, BorderTypes borderType = BorderTypes.Default);
public static Mat GetStructuringElement(MorphShapes shape, Size ksize, Point? anchor = null);
public static void Erode(Mat src, Mat dst, Mat kernel, Point? anchor = null, int iterations = 1, BorderTypes borderType = BorderTypes.Constant, Scalar? borderValue = null);
public static void Dilate(Mat src, Mat dst, Mat kernel, Point? anchor = null, int iterations = 1, BorderTypes borderType = BorderTypes.Constant, Scalar? borderValue = null);
public static void MorphologyEx(Mat src, Mat dst, MorphTypes op, Mat kernel, Point? anchor = null, int iterations = 1, BorderTypes borderType = BorderTypes.Constant, Scalar? borderValue = null);
public static void Line(Mat img, Point pt1, Point pt2, Scalar color, int thickness = 1, LineTypes lineType = LineTypes.Line8, int shift = 0);
public static void ArrowedLine(Mat img, Point pt1, Point pt2, Scalar color, int thickness = 1, LineTypes lineType = LineTypes.Line8, int shift = 0, double tipLength = 0.1);
public static bool ClipLine(Rect imgRect, ref Point pt1, ref Point pt2);
public static void Polylines(Mat img, Point[] pts, bool isClosed, Scalar color, int thickness = 1, LineTypes lineType = LineTypes.Line8, int shift = 0);
public static void FillPoly(Mat img, Point[] pts, Scalar color, LineTypes lineType = LineTypes.Line8, int shift = 0, Point? offset = null);
public static Point[] Ellipse2Poly(Point center, Size axes, int angle, int arcStart, int arcEnd, int delta);
public static double ContourArea(Point[] contour, bool oriented = false);
public static void FindContours(Mat image, out Point[][] contours, out Vec4i[] hierarchy, RetrievalModes mode, ContourApproximationModes method, Point? offset = null);
public static void DrawContours(Mat image, Point[][] contours, int contourIdx, Scalar color, int thickness = 1, LineTypes lineType = LineTypes.Line8, Vec4i[]? hierarchy = null, int maxLevel = int.MaxValue, Point? offset = null);
public static Moments Moments(Mat array, bool binaryImage = false);
public static Moments Moments(Point[] points, bool binaryImage = false);
public static double[] HuMoments(Moments moments);
public static double ArcLength(Point[] curve, bool closed);
public static Point[] ApproxPolyDP(Point[] curve, double epsilon, bool closed);
public static Point2f[] ApproxPolyN(Point[] curve, int nsides, float epsilonPercentage = -1.0F, bool ensureConvex = true);
public static Rect BoundingRect(Point[] points);
public static bool IsContourConvex(Point[] contour);
public static Point[] ConvexHull(Point[] points, bool clockwise = false);
public static int[] ConvexHullIndices(Point[] points, bool clockwise = false);
public static Vec4i[] ConvexityDefects(Point[] contour, int[] convexHullIndices);
public static void MinEnclosingCircle(Point[] points, out Point2f center, out float radius);
public static double PointPolygonTest(Point[] contour, Point2f pt, bool measureDist);
public static double MatchShapes(Point[] contour1, Point[] contour2, ShapeMatchModes method, double parameter = 0);
public static RotatedRect MinAreaRect(Point[] points);
public static Point2f[] BoxPoints(RotatedRect box);
public static RotatedRect FitEllipse(Point[] points);
public static RotatedRect FitEllipseAMS(Point[] points);
public static RotatedRect FitEllipseDirect(Point[] points);
public static RectanglesIntersectTypes RotatedRectangleIntersection(RotatedRect rect1, RotatedRect rect2, out Point2f[] intersectingRegion);
public static Point2f[] GetClosestEllipsePoints(RotatedRect ellipseParams, Point[] points);
public static double MinEnclosingTriangle(Point[] points, out Point2f[] triangle);
public static double MinEnclosingConvexPolygon(Point[] points, int k, out Point2f[] polygon);
public static float IntersectConvexConvex(Point[] polygon1, Point[] polygon2, out Point2f[] intersectingRegion, bool handleNested = true);
public static Vec4f FitLine(Point[] points, DistanceTypes distType, double param, double reps, double aeps);
public static void Rectangle(Mat img, Point pt1, Point pt2, Scalar color, int thickness = 1, LineTypes lineType = LineTypes.Line8, int shift = 0);
public static void Rectangle(Mat img, Rect rect, Scalar color, int thickness = 1, LineTypes lineType = LineTypes.Line8, int shift = 0);
public static void Circle(Mat img, Point center, int radius, Scalar color, int thickness = 1, LineTypes lineType = LineTypes.Line8, int shift = 0);
public static void Ellipse(Mat img, Point center, Size axes, double angle, double startAngle, double endAngle, Scalar color, int thickness = 1, LineTypes lineType = LineTypes.Line8, int shift = 0);
public static void PutText(Mat img, string text, Point org, HersheyFonts fontFace, double fontScale, Scalar color, int thickness = 1, LineTypes lineType = LineTypes.Line8, bool bottomLeftOrigin = false);
public static Size GetTextSize(string text, HersheyFonts fontFace, double fontScale, int thickness, out int baseLine);
```

Initial enum values:

初始枚举值：

```csharp
ColorConversionCodes.BGR2GRAY = 6
InterpolationFlags.Nearest = 0
InterpolationFlags.Linear = 1
ThresholdTypes.Binary = 0
ThresholdTypes.BinaryInv = 1
BorderTypes.Replicate = 1
BorderTypes.Default = 4
MorphShapes.Rect = 0
MorphShapes.Cross = 1
MorphShapes.Ellipse = 2
MorphShapes.Diamond = 3
MorphTypes.Erode = 0
MorphTypes.Dilate = 1
MorphTypes.Open = 2
MorphTypes.Close = 3
LineTypes.Line4 = 4
LineTypes.Line8 = 8
LineTypes.AntiAlias = 16
HersheyFonts.HersheySimplex = 0
HersheyFonts.HersheyPlain = 1
HersheyFonts.HersheyDuplex = 2
HersheyFonts.HersheyComplex = 3
HersheyFonts.HersheyTriplex = 4
HersheyFonts.HersheyComplexSmall = 5
HersheyFonts.HersheyScriptSimplex = 6
HersheyFonts.HersheyScriptComplex = 7
HersheyFonts.Italic = 16
ShapeMatchModes.I1 = 1
ShapeMatchModes.I2 = 2
ShapeMatchModes.I3 = 3
DistanceTypes.User = -1
DistanceTypes.L1 = 1
DistanceTypes.L2 = 2
DistanceTypes.C = 3
DistanceTypes.L12 = 4
DistanceTypes.Fair = 5
DistanceTypes.Welsch = 6
DistanceTypes.Huber = 7
RectanglesIntersectTypes.IntersectNone = 0
RectanglesIntersectTypes.IntersectPartial = 1
RectanglesIntersectTypes.IntersectFull = 2
AdaptiveThresholdTypes.MeanC = 0
AdaptiveThresholdTypes.GaussianC = 1
RetrievalModes.External = 0
RetrievalModes.List = 1
RetrievalModes.CComp = 2
RetrievalModes.Tree = 3
ContourApproximationModes.ApproxNone = 1
ContourApproximationModes.ApproxSimple = 2
ConnectedComponentsTypes.Left = 0
ConnectedComponentsTypes.Top = 1
ConnectedComponentsTypes.Width = 2
ConnectedComponentsTypes.Height = 3
ConnectedComponentsTypes.Area = 4
DistanceTransformMasks.Mask3 = 3
DistanceTransformMasks.Mask5 = 5
DistanceTransformMasks.MaskPrecise = 0
DistanceTransformLabelTypes.CComp = 0
DistanceTransformLabelTypes.Pixel = 1
FloodFillFlags.None = 0
FloodFillFlags.FixedRange = 65536
FloodFillFlags.MaskOnly = 131072
HoughModes.Standard = 0
HoughModes.Probabilistic = 1
HoughModes.MultiScale = 2
HoughModes.Gradient = 3
HoughModes.GradientAlt = 4
LineSegmentDetectorModes.None = 0
LineSegmentDetectorModes.Standard = 1
LineSegmentDetectorModes.Advanced = 2
HistogramComparisonTypes.Correl = 0
HistogramComparisonTypes.ChiSquare = 1
HistogramComparisonTypes.Intersect = 2
HistogramComparisonTypes.Bhattacharyya = 3
HistogramComparisonTypes.ChiSquareAlt = 4
HistogramComparisonTypes.KlDiv = 5
```

## Notes / 说明

- Native ABI must keep using status codes and thread-local error state.
- `Mat` ownership stays in managed wrapper instances; `imgproc` functions only borrow handles.
- This boundary needs `opencv_imgproc` in addition to `opencv_core`.
- OpenCV 5 uses `CV_CN_SHIFT = 5`, so `CV_8UC3` is `64`, not the OpenCV 4-era value `16`.
- `GetStructuringElement` returns a new native `cv::Mat` handle; the managed `Mat` wrapper owns and releases it.
- `Erode` and `Dilate` borrow `src`, `dst`, and `kernel` handles; they do not transfer ownership.
- `MorphologyEx` borrows `src`, `dst`, and `kernel` handles and accepts `MorphTypes` as the operation selector.
- `Scalar? borderValue = null` maps to OpenCV `morphologyDefaultBorderValue()` in native code. When a value is supplied, the C ABI passes the four scalar components as doubles.
- Filtering APIs keep the OpenCV semantics around `anchor`, `ksize`, `sigma`, `delta`, and `borderType`. Functions such as `Blur`, `BoxFilter`, `SqrBoxFilter`, `MedianBlur`, `BilateralFilter`, `Filter2D`, `SepFilter2D`, `Sobel`, `Scharr`, `Laplacian`, `Canny`, `PyrDown`, and `PyrUp` require caller-provided destination matrices.
- `GetGaussianKernel`, `GetDerivKernels`, `GetGaborKernel`, `GetRotationMatrix2D`, `GetAffineTransform`, and `GetPerspectiveTransform` return newly owned `Mat` handles. The managed wrapper owns and releases those handles.
- `WarpAffine`, `WarpPerspective`, `Remap`, and `ConvertMaps` use the same C ABI pattern as the other matrix-to-matrix APIs, with `InterpolationFlags`, `BorderTypes`, and optional `Scalar` border values crossing the boundary as primitives.
- `GetAffineTransform(ReadOnlySpan<Point2f>)` and `GetPerspectiveTransform(ReadOnlySpan<Point2f>)` use pinned point spans on modern .NET targets to avoid temporary heap arrays when the input is already contiguous.
- Drawing APIs mutate the input/output `Mat` in place and pass `Point`, `Size`, `Rect`, and `Scalar` as primitive values across the C ABI.
- `ArrowedLine` follows the same drawing ABI model as `Line`, with `tipLength` passed as a primitive `double`.
- `ClipLine` keeps the native status code separate from the OpenCV boolean result. The C ABI writes the boolean result to `int* intersects` and writes clipped endpoint coordinates back through primitive `int*` parameters.
- `Polylines` currently wraps a single polyline. Managed `Point[]` values are flattened into an interleaved `int[]` buffer (`x0, y0, x1, y1, ...`) before crossing the C ABI. Native code may build `std::vector<cv::Point>` internally, but the C ABI does not expose STL containers.
- `FillPoly` reuses the same interleaved `int[]` point buffer shape as `Polylines`. The optional `offset` is passed as primitive `int` coordinates in the C ABI, while native code may build a temporary `std::vector<cv::Point>` and call the OpenCV overload with `Point offset`.
- `Ellipse2Poly` uses a two-call output buffer pattern: first query the point count, then pass a managed interleaved `int[]` buffer for native code to fill. Native code may use `std::vector<cv::Point>` internally, but no vector crosses the C ABI.
- `ContourArea` reuses the interleaved `Point[]` input boundary and returns the computed `double` through a primitive output pointer. OpenCV 5.0.0 exposes this API from the `geometry` module, so the native target links `opencv_geometry` in addition to `opencv_imgproc`.
- `ArcLength` uses the same interleaved `Point[]` input boundary as `ContourArea`. The `closed` flag crosses the C ABI as an `int`, and the computed `double` length is written through a primitive output pointer.
- `ApproxPolyDP` reuses the interleaved `Point[]` input boundary and the same count/fill two-call output pattern as `Ellipse2Poly`. The native side builds a temporary `std::vector<cv::Point>`, approximates the curve with `cv::approxPolyDP`, then writes the simplified vertices back as an interleaved `int[]` buffer.
- `ApproxPolyN` reuses the interleaved `Point[]` input boundary and returns `Point2f[]` through a two-call count/fill pattern. `nsides` and `ensureConvex` cross the C ABI as primitive `int` values, and `epsilonPercentage` crosses as a primitive `float`.
- `BoundingRect` reuses the interleaved `Point[]` input boundary and returns the rectangle through primitive `int` outputs. The native side converts the points to a temporary `std::vector<cv::Point>` and calls `cv::boundingRect`.
- `IsContourConvex` reuses the interleaved `Point[]` input boundary and returns the boolean result as an `int` flag in the C ABI.
- `ConvexHull` reuses the interleaved `Point[]` input boundary and the same count/fill two-call output pattern. The native side builds a temporary `std::vector<cv::Point>`, calls `cv::convexHull(..., returnPoints = true)`, and writes the hull vertices back as interleaved `int[]`.
- `ConvexHullIndices` calls the same OpenCV hull algorithm with `returnPoints = false` and writes zero-based source point indices as a plain `int[]`.
- `ConvexityDefects` accepts the source contour and hull indices as primitive `int[]` buffers, then returns each defect as a managed `Vec4i` carrying start index, end index, farthest point index, and fixed-point depth.
- `MinEnclosingCircle` reuses the interleaved `Point[]` input boundary and returns the center as primitive `float` outputs plus the radius as a primitive `float`.
- `PointPolygonTest` reuses the interleaved `Point[]` input boundary, accepts the test point as primitive `float` coordinates, and returns the signed distance or sign through a primitive `double` output.
- `MatchShapes` reuses the interleaved `Point[]` input boundary for both contours, accepts the comparison method as a primitive `int` enum value, and returns the distance through a primitive `double` output.
- `MinAreaRect` reuses the interleaved `Point[]` input boundary and returns the rotated rectangle through primitive `float` outputs for center, size, and angle.
- `BoxPoints` accepts a managed `RotatedRect`, unpacks it into primitive `float` values in the C ABI, and writes four `Point2f` vertices back as a fixed-length `float[8]` buffer.
- `FitEllipse`, `FitEllipseAMS`, and `FitEllipseDirect` reuse the same `Point[]` boundary and return the fitted ellipse as primitive `float` outputs for center, size, and angle. The managed layer validates that at least five points are provided before invoking native code.
- `RotatedRectangleIntersection` uses a count/fill pattern, returns `RectanglesIntersectTypes`, and writes up to eight `Point2f` vertices for the intersecting region.
- `GetClosestEllipsePoints` accepts ellipse parameters as primitive `float` values from `RotatedRect` and returns one `Point2f` closest point per input point.
- `MinEnclosingTriangle` writes a fixed three-point `Point2f[]` output and returns the triangle area as `double`.
- `MinEnclosingConvexPolygon` accepts `k` as a primitive `int`, writes up to `k` `Point2f` vertices, and returns the enclosing polygon area as `double`.
- `IntersectConvexConvex` uses a count/fill pattern, accepts two convex polygons as interleaved `Point[]`, writes the intersection region as `Point2f[]`, and returns the intersection area as `float`.
- `FitLine` returns a managed `Vec4f(vx, vy, x0, y0)` for the OpenCV 2D line result and accepts `DistanceTypes` as the primitive distance enum value.
- On modern .NET targets, selected point-set APIs also expose `ReadOnlySpan<Point>` overloads. The managed wrapper pins sequential point memory and uses pointer-based internal P/Invoke declarations to reduce intermediate allocations.
- `PutText` passes managed `string` values as UTF-8 null-terminated byte buffers. The native C ABI receives `const char*` and may build `std::string` internally, but no C++ string type crosses the ABI.
- `GetTextSize` uses the same UTF-8 string boundary and returns `width`, `height`, and `baseLine` through primitive `int*` outputs in the C ABI.
- `FindContours` uses count/fill calls and returns managed `Point[][]` contours plus `Vec4i[]` hierarchy.
- `DrawContours` accepts the managed contour/hierarchy buffers produced by `FindContours`.
- `Moments` crosses the C ABI as a 24-element `double` buffer and becomes a managed value object.
- `HuMoments` accepts the managed `Moments` value and returns a seven-element `double[]`.
- `FloodFill` has separate mask and non-mask native entries to keep the ABI explicit.
- `CLAHE` and `LineSegmentDetector` are owned native objects wrapped by disposable managed classes. Their C ABI uses opaque handles and never exposes OpenCV class types.
- Hough line and circle APIs use count/fill calls and return managed value objects instead of raw float buffers at the public surface.
- Histogram APIs currently cover uniform dense histograms for one source image, with channel, range, and accumulation arguments crossing the ABI as primitive buffers.
- `CornerSubPix` mutates caller-provided `Point2f[]` values, and modern target frameworks also get a span-backed path.
- The current local OpenCV runtime does not include the optional features module/header. `GoodFeaturesToTrack` is present at the managed and native boundary, but this runtime reports a defined not-linked error until the OpenCV build includes that dependency.

- native ABI 必须继续使用状态码和线程本地错误状态。
- `Mat` 所有权仍由 managed 包装类持有；`imgproc` 函数只借用句柄。
- 此边界除 `opencv_core` 外还需要 `opencv_imgproc`。
- OpenCV 5 使用 `CV_CN_SHIFT = 5`，因此 `CV_8UC3` 是 `64`，不是 OpenCV 4 时代常见的 `16`。
- `GetStructuringElement` 返回新的 native `cv::Mat` 句柄；managed `Mat` 包装类拥有并释放它。
- `Erode` 和 `Dilate` 借用 `src`、`dst`、`kernel` 句柄，不转移所有权。
- `MorphologyEx` 借用 `src`、`dst`、`kernel` 句柄，并使用 `MorphTypes` 选择操作类型。
- `Scalar? borderValue = null` 在 native 侧映射为 OpenCV `morphologyDefaultBorderValue()`；显式传值时，C ABI 以四个 double 分量传递标量。
- 绘图 API 会原地修改输入输出 `Mat`，并通过 C ABI 以基础类型传递 `Point`、`Size`、`Rect` 和 `Scalar`。
- `ArrowedLine` 沿用与 `Line` 相同的绘图 ABI 模型，`tipLength` 以基础类型 `double` 传递。
- `ClipLine` 将 native 状态码与 OpenCV 的 bool 业务结果分离。C ABI 通过 `int* intersects` 写出是否相交，并通过基础类型 `int*` 参数写回裁剪后的端点坐标。
- `Polylines` 当前封装单条折线。managed `Point[]` 会在跨越 C ABI 前展平为交错 `int[]` 缓冲（`x0, y0, x1, y1, ...`）。native 内部可以构造 `std::vector<cv::Point>`，但 C ABI 不暴露 STL 容器。
- `FillPoly` 复用与 `Polylines` 相同的交错 `int[]` 点缓冲。可选 `offset` 在 C ABI 中以基础类型 `int` 坐标传递，native 内部可以构造临时 `std::vector<cv::Point>` 并调用 OpenCV 带 `Point offset` 的重载。
- `Ellipse2Poly` 使用两次调用的输出缓冲模式：先查询点数量，再传入 managed 侧分配的交错 `int[]` 缓冲供 native 填充。native 内部可以使用 `std::vector<cv::Point>`，但 vector 不跨越 C ABI。
- `ContourArea` 复用交错 `Point[]` 输入边界，并通过基础类型输出指针返回计算得到的 `double`。OpenCV 5.0.0 从 `geometry` 模块暴露此 API，因此 native target 除 `opencv_imgproc` 外还会链接 `opencv_geometry`。
- `ArcLength` 使用与 `ContourArea` 相同的交错 `Point[]` 输入边界。`closed` 标志以 `int` 穿过 C ABI，计算得到的 `double` 长度通过基础类型输出指针写回。
- `ApproxPolyDP` 复用交错 `Point[]` 输入边界，并沿用与 `Ellipse2Poly` 相同的两次调用输出缓冲模式。native 内部先构造临时 `std::vector<cv::Point>`，再用 `cv::approxPolyDP` 生成简化顶点并回写交错 `int[]` 缓冲。
- `ApproxPolyN` 复用交错 `Point[]` 输入边界，并通过两次调用 count/fill 模式返回 `Point2f[]`。`nsides` 和 `ensureConvex` 在 C ABI 中以基础类型 `int` 传递，`epsilonPercentage` 以基础类型 `float` 传递。
- `BoundingRect` 复用交错 `Point[]` 输入边界，并通过基础类型 `int` 输出返回矩形。native 侧会把点集转成临时 `std::vector<cv::Point>`，再调用 `cv::boundingRect`。
- `IsContourConvex` 复用交错 `Point[]` 输入边界，并在 C ABI 中通过 `int` 标志返回布尔结果。
- `ConvexHull` 复用交错 `Point[]` 输入边界，并沿用与 `Ellipse2Poly` 相同的两次调用输出缓冲模式。native 侧会构造临时 `std::vector<cv::Point>`，调用 `cv::convexHull(..., returnPoints = true)`，再把凸包顶点写回交错 `int[]` 缓冲。
- `ConvexHullIndices` 使用同一个 OpenCV 凸包算法，但以 `returnPoints = false` 返回原始点数组中的零基索引，并以普通 `int[]` 写回。
- `ConvexityDefects` 接收原始轮廓和凸包索引两个基础 `int[]` 缓冲，并返回 managed `Vec4i`，四个分量分别表示起点索引、终点索引、最远点索引和定点深度。
- `MinEnclosingCircle` 复用交错 `Point[]` 输入边界，并通过基础类型 `float` 输出圆心和半径。
- `PointPolygonTest` 复用交错 `Point[]` 输入边界，测试点通过基础类型 `float` 坐标传入，结果通过基础类型 `double` 输出返回带符号距离或符号值。
- `MatchShapes` 复用两个交错 `Point[]` 轮廓输入，比较方法通过基础类型 `int` 枚举值传入，结果通过基础类型 `double` 输出返回形状距离。
- `MinAreaRect` 复用交错 `Point[]` 输入边界，并通过基础类型 `float` 输出返回旋转矩形的中心、尺寸和角度。
- `BoxPoints` 接收 managed `RotatedRect`，在 C ABI 中拆成基础类型 `float`，再以固定长度 `float[8]` 缓冲写回四个顶点。
- `FitEllipse`、`FitEllipseAMS` 和 `FitEllipseDirect` 复用相同的 `Point[]` 边界，并通过基础类型 `float` 输出返回拟合椭圆的中心、尺寸和角度。managed 层会先校验点数不少于 5 个。
- `RotatedRectangleIntersection` 使用两次调用 count/fill 模式，返回 `RectanglesIntersectTypes`，并写回最多八个 `Point2f` 相交区域顶点。
- `GetClosestEllipsePoints` 从 `RotatedRect` 拆出基础类型 `float` 椭圆参数，并为每个输入点返回一个 `Point2f` 最近点。
- `MinEnclosingTriangle` 写回固定三个 `Point2f` 顶点，并以 `double` 返回三角形面积。
- `MinEnclosingConvexPolygon` 以基础类型 `int` 接收 `k`，写回最多 `k` 个 `Point2f` 顶点，并以 `double` 返回外接凸多边形面积。
- `IntersectConvexConvex` 使用两次调用 count/fill 模式，接收两个交错 `Point[]` 凸多边形，写回 `Point2f[]` 相交区域，并以 `float` 返回相交面积。
- `FitLine` 为 OpenCV 二维直线结果返回 managed `Vec4f(vx, vy, x0, y0)`，并以基础枚举值传递 `DistanceTypes`。
- 在现代 .NET 目标上，部分点集 API 还提供 `ReadOnlySpan<Point>` 重载。managed 包装会固定顺序布局的点内存，并通过指针型内部 P/Invoke 声明减少中间分配。
- `PutText` 将 managed `string` 作为 UTF-8 null-terminated 字节缓冲传递。native C ABI 接收 `const char*`，内部可以构造 `std::string`，但 C++ 字符串类型不会穿过 ABI。
- `GetTextSize` 沿用相同的 UTF-8 字符串边界，并在 C ABI 中通过基础类型 `int*` 输出 `width`、`height` 和 `baseLine`。
