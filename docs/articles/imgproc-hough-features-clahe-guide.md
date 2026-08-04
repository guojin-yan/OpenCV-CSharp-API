# ImgProc Hough, Features, CLAHE Guide

This guide covers the current `imgproc` analysis pack for local contrast enhancement, histogram analysis, Hough transforms, sub-pixel corners, and line segment detection.

本文档说明当前 `imgproc` 分析能力包：局部对比度增强、直方图分析、霍夫变换、亚像素角点和线段检测。

## Scope / 范围

The current managed surface includes:

当前托管表层包含：

- `ImgProcCv2.CreateCLAHE` and the disposable `CLAHE` object, including output-`Mat` and returning `Apply` overloads.
- `ImgProcCv2.CalcHist`, `ImgProcCv2.CalcBackProject`, and `ImgProcCv2.CompareHist`.
- `ImgProcCv2.HoughLines`, `ImgProcCv2.HoughLinesP`, `ImgProcCv2.HoughLinesPointSet`, and `ImgProcCv2.HoughCircles`.
- `ImgProcCv2.CornerSubPix`.
- `ImgProcCv2.GoodFeaturesToTrack`, with a defined not-linked boundary when the local OpenCV build does not provide the features header/library.
- `ImgProcCv2.CreateLineSegmentDetector` and the disposable `LineSegmentDetector` object.
- Value objects: `HoughLine`, `HoughLinePointSet`, `HoughCircle`, `LineSegment`, and `TermCriteria`.

## Pipeline / 流水线

The example below keeps the public C# names close to OpenCV C++ while using explicit `Program.Main` and disposable managed wrappers.

下面示例保持 C# 公开命名接近 OpenCV C++，同时使用显式 `Program.Main` 和可释放托管包装对象。

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Geometry;
using JYPPX.OpenCvSharp.ImgProc;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat gray = new Mat(64, 64, MatType.CV_8UC1))
            using (Mat claheImage = new Mat())
            using (Mat hist = new Mat())
            using (Mat backProject = new Mat())
            using (Mat linesImage = new Mat(64, 64, MatType.CV_8UC1))
            using (Mat circlesImage = new Mat(64, 64, MatType.CV_8UC1))
            {
                gray.SetTo(new Scalar(0));
                linesImage.SetTo(new Scalar(0));
                circlesImage.SetTo(new Scalar(0));

                ImgProcCv2.Rectangle(gray, new Rect(8, 8, 24, 24), new Scalar(120), -1);
                ImgProcCv2.Rectangle(gray, new Rect(28, 28, 24, 24), new Scalar(220), -1);

                using (CLAHE clahe = ImgProcCv2.CreateCLAHE(2.0, new Size(4, 4)))
                {
                    clahe.ClipLimit = 3.0;
                    clahe.Apply(gray, claheImage);
                    clahe.CollectGarbage();
                }

                ImgProcCv2.CalcHist(claheImage, 0, null, hist, 8, 0, 256);
                ImgProcCv2.CalcBackProject(claheImage, 0, hist, backProject, 0, 256);

                ImgProcCv2.Line(linesImage, new Point(5, 8), new Point(58, 8), new Scalar(255), 1);
                ImgProcCv2.Line(linesImage, new Point(5, 20), new Point(58, 50), new Scalar(255), 1);
                ImgProcCv2.Circle(circlesImage, new Point(32, 32), 12, new Scalar(255), 2);

                HoughLine[] standardLines = ImgProcCv2.HoughLines(linesImage, 1.0, Math.PI / 180.0, 20);
                Vec4i[] segments = ImgProcCv2.HoughLinesP(linesImage, 1.0, Math.PI / 180.0, 10, 8.0, 2.0);
                HoughCircle[] circles = ImgProcCv2.HoughCircles(circlesImage, HoughModes.Gradient, 1.0, 16.0, 80.0, 8.0, 5, 20);

                Point2f[] corners = new[]
                {
                    new Point2f(8.0F, 8.0F),
                    new Point2f(31.0F, 31.0F)
                };
                ImgProcCv2.CornerSubPix(claheImage, corners, new Size(3, 3), new Size(-1, -1), TermCriteria.ByCountAndEpsilon(20, 0.01));

                using (LineSegmentDetector detector = ImgProcCv2.CreateLineSegmentDetector())
                {
                    LineSegment[] detectedSegments = detector.Detect(linesImage);
                    Console.WriteLine($"lsd={detectedSegments.Length}");
                }

                Console.WriteLine($"histBins={hist.ValueCount}, backProject={backProject.Rows}x{backProject.Cols}");
                Console.WriteLine($"hough={standardLines.Length}, houghP={segments.Length}, circles={circles.Length}");
                Console.WriteLine($"corner0={corners[0]}");
            }
        }
    }
}
```

## Modern Span Paths / 现代 Span 路径

On modern targets, selected APIs accept span-backed data so the wrapper can pin contiguous buffers and reduce temporary allocations.

在现代目标框架上，部分 API 接受基于 Span 的数据，包装层可以固定连续缓冲并减少临时分配。

```csharp
#if NETCOREAPP3_1_OR_GREATER
Point[] points = new[]
{
    new Point(0, 0),
    new Point(5, 5),
    new Point(10, 10),
    new Point(15, 15)
};

HoughLinePointSet[] lines = ImgProcCv2.HoughLinesPointSet(
    points.AsSpan(),
    4,
    2,
    -50,
    50,
    1,
    0,
    Math.PI,
    Math.PI / 180.0);
#endif
```

## Native Boundary / Native 边界

The native layer still exposes only opaque handles and plain buffers:

native 层仍只暴露不透明句柄和基础缓冲：

- `jyppx_ocv_clahe*` owns the native CLAHE implementation.
- `jyppx_ocv_line_segment_detector*` owns the native line segment detector.
- Hough line, circle, histogram, corner, and line segment results use count/fill buffer patterns.
- C++ exceptions are caught at the native boundary and converted to project status codes.

## Known Boundary / 已知边界

The local OpenCV build used for this stage provides `core`, `geometry`, `imgcodecs`, and `imgproc`, but not the optional features module/header. Because of that, `GoodFeaturesToTrack` has a managed and native boundary, but the local runtime for the current packaged runtime identity returns a not-linked error instead of detecting corners.

本阶段使用的本地 OpenCV 构建提供 `core`、`geometry`、`imgcodecs` 和 `imgproc`，但没有可选 features 模块/头文件。因此 `GoodFeaturesToTrack` 已有托管与 native 边界，但当前打包 runtime 身份下的本地 runtime 会返回 not-linked 错误，而不是实际检测角点。
