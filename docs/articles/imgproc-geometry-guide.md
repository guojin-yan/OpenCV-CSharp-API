# ImgProc Geometry Guide / ImgProc 几何指南

This guide explains the current point-set and shape-analysis APIs in `OpenCvSharp.ImgProc.Cv2`.

本文说明 `OpenCvSharp.ImgProc.Cv2` 当前的点集和形状分析 API。

## Point Input / 点集输入

Classic APIs accept `Point[]` for compatibility with all target frameworks:

传统 API 使用 `Point[]`，用于兼容所有目标框架：

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

Point[] contour = new Point[]
{
    new Point(0, 0),
    new Point(4, 0),
    new Point(4, 3),
    new Point(0, 3)
};

double area = ImgProcCv2.ContourArea(contour);
double length = ImgProcCv2.ArcLength(contour, true);
```

On `netcoreapp3.1` and newer, selected APIs also accept `ReadOnlySpan<Point>`. The span overloads reinterpret sequential `Point` memory as interleaved `x, y` values and pin it before calling the native C ABI.

在 `netcoreapp3.1` 及更新框架上，部分 API 也支持 `ReadOnlySpan<Point>`。Span 重载会把顺序布局的 `Point` 内存重解释为交错的 `x, y` 值，并在调用 native C ABI 前固定内存。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

ReadOnlySpan<Point> contourSpan = contour.AsSpan();

double spanArea = ImgProcCv2.ContourArea(contourSpan);
Point[] hull = ImgProcCv2.ConvexHull(contourSpan);
```

## Current Span Fast Paths / 当前 Span 快速路径

The current geometry fast-path batch covers:

当前几何快速路径批次覆盖：

- `ContourArea(ReadOnlySpan<Point>)`
- `ArcLength(ReadOnlySpan<Point>)`
- `ApproxPolyDP(ReadOnlySpan<Point>)`
- `BoundingRect(ReadOnlySpan<Point>)`
- `IsContourConvex(ReadOnlySpan<Point>)`
- `ConvexHull(ReadOnlySpan<Point>)`
- `ConvexHullIndices(ReadOnlySpan<Point>)`
- `MinEnclosingCircle(ReadOnlySpan<Point>)`
- `PointPolygonTest(ReadOnlySpan<Point>)`
- `MatchShapes(ReadOnlySpan<Point>, ReadOnlySpan<Point>)`
- `MinAreaRect(ReadOnlySpan<Point>)`
- `ApproxPolyN(ReadOnlySpan<Point>)`
- `FitEllipse(ReadOnlySpan<Point>)`
- `FitEllipseAMS(ReadOnlySpan<Point>)`
- `FitEllipseDirect(ReadOnlySpan<Point>)`
- `GetClosestEllipsePoints(RotatedRect, ReadOnlySpan<Point>)`
- `MinEnclosingTriangle(ReadOnlySpan<Point>)`
- `MinEnclosingConvexPolygon(ReadOnlySpan<Point>)`
- `IntersectConvexConvex(ReadOnlySpan<Point>, ReadOnlySpan<Point>)`
- `FitLine(ReadOnlySpan<Point>)`

## Common Shape Tasks / 常见形状任务

Use contour measurements for area, perimeter, bounding boxes, and convexity:

使用轮廓测量 API 计算面积、周长、外接矩形和凸性：

```csharp
double area = ImgProcCv2.ContourArea(contour);
double perimeter = ImgProcCv2.ArcLength(contour, true);
Rect bounds = ImgProcCv2.BoundingRect(contour);
bool convex = ImgProcCv2.IsContourConvex(contour);
```

Use convex hull APIs when later algorithms need hull vertices or source indices:

当后续算法需要凸包顶点或源点索引时，使用凸包 API：

```csharp
Point[] hullPoints = ImgProcCv2.ConvexHull(contour);
int[] hullIndices = ImgProcCv2.ConvexHullIndices(contour);
Vec4i[] defects = ImgProcCv2.ConvexityDefects(contour, hullIndices);
```

Use enclosing and fitting APIs to describe a point set with simple geometry:

使用外接和拟合 API 以简单几何描述点集：

```csharp
ImgProcCv2.MinEnclosingCircle(contour, out Point2f center, out float radius);
RotatedRect minRect = ImgProcCv2.MinAreaRect(contour);
Vec4f line = ImgProcCv2.FitLine(contour, DistanceTypes.L2, 0, 0.01, 0.01);
```

The advanced geometry overloads accept the same point storage directly and return managed `Point2f[]` results without first creating interleaved managed input or output arrays:

高级几何重载直接使用相同点集内存，并返回 managed `Point2f[]`，不需要先创建交错的 managed 输入或输出数组：

```csharp
ReadOnlySpan<Point> points = contour.AsSpan();

Point2f[] quadrilateral = ImgProcCv2.ApproxPolyN(points, 4);
double triangleArea = ImgProcCv2.MinEnclosingTriangle(points, out Point2f[] triangle);
double polygonArea = ImgProcCv2.MinEnclosingConvexPolygon(points, 4, out Point2f[] polygon);
float intersectionArea = ImgProcCv2.IntersectConvexConvex(points, points, out Point2f[] intersection);
```

## Degenerate Inputs / 退化输入

Empty arrays and empty spans are rejected in managed code before native calls for the span fast paths.

空数组和空 Span 会在 managed 层拒绝，Span 快速路径不会把空输入继续传入 native。

Some OpenCV algorithms have additional requirements:

部分 OpenCV 算法还有额外要求：

- `FitEllipse`, `FitEllipseAMS`, and `FitEllipseDirect` require at least five points.
- `ApproxPolyN` and `MinEnclosingConvexPolygon` require at least three requested sides or vertices.
- `ConvexityDefects` requires hull indices from `ConvexHullIndices`, not hull points.
- `IntersectConvexConvex` expects convex polygons.
- Self-intersecting contours, duplicated points, and collinear points can produce OpenCV-specific results.

- `FitEllipse`、`FitEllipseAMS`、`FitEllipseDirect` 至少需要五个点。
- `ApproxPolyN` 和 `MinEnclosingConvexPolygon` 要求目标边数或顶点数至少为三。
- `ConvexityDefects` 需要 `ConvexHullIndices` 返回的凸包索引，而不是凸包顶点。
- `IntersectConvexConvex` 期望输入为凸多边形。
- 自交轮廓、重复点、共线点可能产生 OpenCV 特定结果。
