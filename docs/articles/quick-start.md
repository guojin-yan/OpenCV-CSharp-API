# Quick Start / 快速开始

Install the managed package and a matching runtime package:

安装 managed 主包和对应平台 runtime 包：

Runtime package IDs use `JYPPX.OpenCV.runtime.<rid>`. Choose the package that matches your target RID when available; the command below uses `JYPPX.OpenCV.runtime.win-x64` as the current Windows x64 example.

runtime package ID 使用 `JYPPX.OpenCV.runtime.<rid>`。当对应包可用时，请选择与目标 target RID 匹配的 runtime 包；下方命令使用 `JYPPX.OpenCV.runtime.win-x64` 作为当前 Windows x64 示例。

Currently tracked runtime package project: `JYPPX.OpenCV.runtime.win-x64`. If no matching runtime package is available yet, build and stage a local native runtime with `Build-OpenCV.ps1` and `Stage-Runtime.ps1`, then use `OpenCvNativeRuntimeDir` for local builds or `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>` for packaging.

当前仓库跟踪的 runtime package project：`JYPPX.OpenCV.runtime.win-x64`。如果 no matching runtime package is available yet，请使用 `Build-OpenCV.ps1` 和 `Stage-Runtime.ps1` 构建并暂存 local native runtime，然后在本地构建中使用 `OpenCvNativeRuntimeDir`，或用 `Pack-Runtime.ps1 -StageRuntime -OpenCvNativeRuntimeDir <runtime-native-dir>` 打包。

For deeper runtime setup, fallback, smoke, and license details, see the [Linked Runtime Build Guide](linked-runtime-build-guide.md), [Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md), [Smoke Profiles Guide](smoke-profiles-guide.md), [Runtime Licenses](runtime-licenses.md), and the current concrete [win-x64 runtime package README](../../packaging/runtime/JYPPX.OpenCV.runtime.win-x64/README.md).

更深入的 runtime 设置、fallback、smoke 和 license 细节见 [Linked Runtime Build Guide](linked-runtime-build-guide.md)、[Linked Runtime Smoke Guide](linked-runtime-smoke-guide.md)、[Smoke Profiles Guide](smoke-profiles-guide.md)、[Runtime Licenses](runtime-licenses.md) 以及当前具体 [win-x64 runtime package README](../../packaging/runtime/JYPPX.OpenCV.runtime.win-x64/README.md)。

```powershell
dotnet add package JYPPX.OpenCV.CSharp.API --version 5.0.0.0
dotnet add package JYPPX.OpenCV.runtime.win-x64 --version 5.0.0.0
```

Keep the managed and runtime packages on the same four-part package version metadata; the package IDs and public namespace stay version-neutral.

managed 主包和 runtime 包应使用相同的四段 package version 元数据；包 ID 和公开命名空间保持版本中立。

Core array operations cover arithmetic, statistics, normalization, and channel layout work:

Core 数组运算覆盖算术、统计、归一化和通道布局处理：

```csharp
using System;
using OpenCvSharp.Core;
using CoreCv2 = OpenCvSharp.Core.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat a = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat b = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat added = new Mat())
            using (Mat normalized = new Mat())
            using (Mat color = new Mat(2, 2, MatType.CV_8UC3))
            {
                a.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6 });
                b.CopyFrom(new byte[] { 6, 5, 4, 3, 2, 1 });
                color.CopyFrom(new byte[]
                {
                    1, 10, 100,
                    2, 20, 110,
                    3, 30, 120,
                    4, 40, 130
                });

                CoreCv2.Add(a, b, added);
                CoreCv2.Normalize(a, normalized, 0.0, 255.0, NormTypes.MinMax);

                Scalar mean = CoreCv2.Mean(a);
                MinMaxLocResult minMax = CoreCv2.MinMaxLoc(a);
                Mat[] channels = CoreCv2.Split(color);
                try
                {
                    using (Mat merged = CoreCv2.Merge(channels))
                    {
                        Console.WriteLine(string.Join(",", added.ToBytes()));
                        Console.WriteLine(string.Join(",", normalized.ToBytes()));
                        Console.WriteLine($"mean={mean.V0}, min={minMax.MinVal}, max={minMax.MaxVal}");
                        Console.WriteLine($"channels={channels.Length}, mergedType={merged.Type}");
                    }
                }
                finally
                {
                    for (int i = 0; i < channels.Length; i++)
                    {
                        channels[i].Dispose();
                    }
                }
            }
        }
    }
}
```

Linear algebra, decomposition objects, random generation, and spectral transforms are available from `OpenCvSharp.Core`:

线性代数、分解对象、随机数生成和频谱变换可通过 `OpenCvSharp.Core` 使用：

```csharp
using System;
using OpenCvSharp.Core;
using CoreCv2 = OpenCvSharp.Core.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat a = new Mat(2, 2, MatType.CV_64FC1))
            using (Mat b = new Mat(2, 2, MatType.CV_64FC1))
            using (Mat rhs = new Mat(2, 1, MatType.CV_64FC1))
            using (Mat solution = new Mat())
            using (Mat random = new Mat(2, 3, MatType.CV_32SC1))
            using (Mat signal = new Mat(1, 4, MatType.CV_64FC1))
            using (Mat spectrum = new Mat())
            using (Mat recovered = new Mat())
            {
                a.CopyFrom<double>(new double[] { 1.0, 0.0, 0.0, 2.0 });
                b.CopyFrom<double>(new double[] { 5.0, 6.0, 7.0, 8.0 });
                rhs.CopyFrom<double>(new double[] { 3.0, 8.0 });
                signal.CopyFrom<double>(new double[] { 1.0, 2.0, 3.0, 4.0 });

                using (Mat product = CoreCv2.Gemm(a, b))
                using (Svd svd = new Svd(a))
                using (Rng rng = new Rng(42UL))
                {
                    svd.BackSubst(rhs, solution);
                    rng.FillUniform(random, new Scalar(0), new Scalar(10));
                    CoreCv2.Dft(signal, spectrum, DftFlags.ComplexOutput);
                    CoreCv2.Idft(spectrum, recovered, DftFlags.Scale | DftFlags.RealOutput);

                    Console.WriteLine(string.Join(",", product.ToArray<double>()));
                    Console.WriteLine(string.Join(",", solution.ToArray<double>()));
                    Console.WriteLine(string.Join(",", random.ToArray<int>()));
                    Console.WriteLine(string.Join(",", recovered.ToArray<double>()));
                }
            }
        }
    }
}
```

Local contrast, histogram, Hough, and line segment APIs can be composed into a compact analysis flow:

局部对比度、直方图、霍夫变换和线段检测 API 可以组合成一个紧凑的分析流程：

```csharp
using System;
using OpenCvSharp.Core;
using OpenCvSharp.Geometry;
using OpenCvSharp.ImgProc;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat gray = new Mat(64, 64, MatType.CV_8UC1))
            using (Mat enhanced = new Mat())
            using (Mat hist = new Mat())
            using (Mat linesImage = new Mat(64, 64, MatType.CV_8UC1))
            {
                gray.SetTo(new Scalar(0));
                linesImage.SetTo(new Scalar(0));
                ImgProcCv2.Rectangle(gray, new Rect(8, 8, 32, 32), new Scalar(180), -1);
                ImgProcCv2.Line(linesImage, new Point(5, 8), new Point(58, 8), new Scalar(255));

                using (CLAHE clahe = ImgProcCv2.CreateCLAHE(2.0, new Size(4, 4)))
                {
                    clahe.Apply(gray, enhanced);
                }

                ImgProcCv2.CalcHist(enhanced, 0, null, hist, 8, 0, 256);
                HoughLine[] lines = ImgProcCv2.HoughLines(linesImage, 1.0, Math.PI / 180.0, 20);

                using (LineSegmentDetector detector = ImgProcCv2.CreateLineSegmentDetector())
                {
                    LineSegment[] segments = detector.Detect(linesImage);
                    Console.WriteLine($"hist={hist.ValueCount}, hough={lines.Length}, lsd={segments.Length}");
                }
            }
        }
    }
}
```

Segmentation, contour, and moment APIs can be composed into an analysis pipeline:

分割、轮廓和矩 API 可以组合成图像分析流水线：

```csharp
using System;
using OpenCvSharp.Core;
using OpenCvSharp.Geometry;
using OpenCvSharp.ImgProc;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat src = new Mat(8, 8, MatType.CV_8UC1))
            using (Mat equalized = new Mat())
            using (Mat binary = new Mat())
            using (Mat labels = new Mat())
            using (Mat stats = new Mat())
            using (Mat centroids = new Mat())
            using (Mat corners = new Mat())
            {
                src.SetTo(new Scalar(0));
                ImgProcCv2.Rectangle(src, new Rect(1, 1, 3, 3), new Scalar(180), -1);
                ImgProcCv2.Rectangle(src, new Rect(5, 5, 2, 2), new Scalar(220), -1);

                ImgProcCv2.EqualizeHist(src, equalized);
                ImgProcCv2.AdaptiveThreshold(equalized, binary, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 3, 2);

                int componentCount = ImgProcCv2.ConnectedComponentsWithStats(binary, labels, stats, centroids);
                ImgProcCv2.FindContours(binary, out Point[][] contours, out Vec4i[] hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                Moments moments = ImgProcCv2.Moments(binary, true);
                double[] hu = ImgProcCv2.HuMoments(moments);
                ImgProcCv2.CornerHarris(binary, corners, 3, 3, 0.04);

                Console.WriteLine($"components={componentCount}");
                Console.WriteLine($"contours={contours.Length}, hierarchy={hierarchy.Length}");
                Console.WriteLine($"m00={moments.M00}, hu0={hu[0]}");
                Console.WriteLine($"corners={corners.Rows}x{corners.Cols}");
            }
        }
    }
}
```

Minimal code:

最小示例：

```csharp
using System;
using OpenCvSharp;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine(OpenCvSharpBuildInfo.PackageVersion);
        }
    }
}
```

Filtering and transform APIs can be composed into a small preprocessing pipeline:

滤波和变换 API 可以组合成一个小型预处理流水线：

```csharp
using System;
using OpenCvSharp.Core;
using OpenCvSharp.Geometry;
using OpenCvSharp.ImgProc;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat src = new Mat(2, 3, MatType.CV_8UC1))
            using (Mat smoothed = new Mat())
            using (Mat edges = new Mat())
            using (Mat rotation = ImgProcCv2.GetRotationMatrix2D(new Point2f(0.0F, 0.0F), 0.0, 1.0))
            using (Mat warped = new Mat())
            using (Mat mapX = new Mat(2, 3, MatType.CV_32FC1))
            using (Mat mapY = new Mat(2, 3, MatType.CV_32FC1))
            using (Mat remapped = new Mat())
            {
                src.CopyFrom(new byte[]
                {
                    10, 20, 30,
                    40, 50, 60
                });
                mapX.CopyFrom<float>(new float[] { 0, 1, 2, 0, 1, 2 });
                mapY.CopyFrom<float>(new float[] { 0, 0, 0, 1, 1, 1 });

                ImgProcCv2.BoxFilter(src, smoothed, -1, new Size(3, 3));
                ImgProcCv2.Canny(src, edges, 20.0, 60.0);
                ImgProcCv2.WarpAffine(src, warped, rotation, new Size(3, 2), InterpolationFlags.Nearest);
                ImgProcCv2.Remap(src, remapped, mapX, mapY, InterpolationFlags.Nearest);

#if NETCOREAPP3_1_OR_GREATER
                Point2f[] sourcePoints = new Point2f[]
                {
                    new Point2f(0.0F, 0.0F),
                    new Point2f(2.0F, 0.0F),
                    new Point2f(0.0F, 1.0F)
                };

                Point2f[] destinationPoints = new Point2f[]
                {
                    new Point2f(0.0F, 0.0F),
                    new Point2f(2.0F, 0.0F),
                    new Point2f(0.0F, 1.0F)
                };

                using (Mat affine = ImgProcCv2.GetAffineTransform(sourcePoints.AsSpan(), destinationPoints.AsSpan()))
                {
                    Console.WriteLine($"{affine.Rows}x{affine.Cols}");
                }
#endif

                Console.WriteLine($"{smoothed.Rows}x{smoothed.Cols}");
                Console.WriteLine($"{edges.Rows}x{edges.Cols}");
                Console.WriteLine(string.Join(",", remapped.ToBytes()));
            }
        }
    }
}
```

When the native runtime package is available, `Mat` can be created and inspected:

当 native runtime 包可用后，可以创建并检查 `Mat`：

```csharp
using System;
using OpenCvSharp.Core;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat mat = new Mat(2, 3, MatType.CV_8UC1))
            {
                Console.WriteLine($"{mat.Rows}x{mat.Cols}, channels={mat.Channels}, total={mat.Total}");
            }
        }
    }
}
```

`Mat` also supports OpenCV-style factories, deep copy, ROI views, reshape views, and modern continuous-memory spans on newer .NET targets:

`Mat` 还支持接近 OpenCV 的工厂方法、深拷贝、ROI 视图、reshape 视图，以及新 .NET 目标框架上的连续内存 Span：

```csharp
using System;
using OpenCvSharp.Core;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat eye = Mat.Eye(3, 3, MatType.CV_8UC1))
            using (Mat canvas = Mat.Zeros(new Size(4, 3), MatType.CV_8UC1))
            {
                canvas.SetTo(new Scalar(2));

                using (Mat clone = canvas.Clone())
                using (Mat roi = clone.SubMat(new Rect(1, 1, 2, 2)))
                using (Mat reshaped = eye.Reshape(1, 1))
                {
                    roi.SetTo(new Scalar(9));

                    using (Mat roiCopy = roi.Clone())
                    {
                        Console.WriteLine($"clone={clone.Rows}x{clone.Cols}, sub={roi.IsSubmatrix}");
                        Console.WriteLine($"roiCopy bytes={string.Join(",", roiCopy.ToBytes())}");
                        Console.WriteLine($"reshape={reshaped.Rows}x{reshaped.Cols}, channels={reshaped.Channels}");
#if NETCOREAPP3_1_OR_GREATER
                        if (clone.TryGetSpan<byte>(out Span<byte> pixels))
                        {
                            Console.WriteLine($"span={pixels.Length}, first={pixels[0]}");
                        }
#endif
                    }
                }
            }
        }
    }
}
```

The first `imgproc` APIs are available through `OpenCvSharp.ImgProc.Cv2`:

第一批 `imgproc` API 可通过 `OpenCvSharp.ImgProc.Cv2` 使用：

```csharp
using System;
using OpenCvSharp.Core;
using OpenCvSharp.Geometry;
using OpenCvSharp.ImgProc;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat src = new Mat(2, 2, MatType.CV_8UC3))
            using (Mat gray = new Mat())
            using (Mat resized = new Mat())
            using (Mat thresholded = new Mat())
            using (Mat blurred = new Mat())
            using (Mat kernel = ImgProcCv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)))
            using (Mat eroded = new Mat())
            using (Mat dilated = new Mat())
            using (Mat opened = new Mat())
            using (Mat drawing = new Mat(40, 120, MatType.CV_8UC1))
            {
                src.CopyFrom(new byte[]
                {
                    0, 0, 0,
                    10, 20, 30,
                    50, 100, 150,
                    255, 255, 255
                });

                ImgProcCv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
                ImgProcCv2.Resize(gray, resized, new Size(4, 4), interpolation: InterpolationFlags.Nearest);
                ImgProcCv2.Threshold(gray, thresholded, 127, 255, ThresholdTypes.Binary);
                ImgProcCv2.GaussianBlur(thresholded, blurred, new Size(3, 3), 0, 0);
                ImgProcCv2.Erode(thresholded, eroded, kernel);
                ImgProcCv2.Dilate(thresholded, dilated, kernel);
                ImgProcCv2.MorphologyEx(thresholded, opened, MorphTypes.Open, kernel);
                drawing.CopyFrom(new byte[drawing.ByteLength]);
                ImgProcCv2.Line(drawing, new Point(0, 0), new Point(4, 4), new Scalar(255));
                ImgProcCv2.ArrowedLine(drawing, new Point(32, 8), new Point(112, 8), new Scalar(180), tipLength: 0.2);
                Point clipPt1 = new Point(-5, 2);
                Point clipPt2 = new Point(15, 2);
                bool clipIntersects = ImgProcCv2.ClipLine(new Rect(0, 0, 10, 10), ref clipPt1, ref clipPt2);
                ImgProcCv2.Rectangle(drawing, new Rect(1, 1, 3, 3), new Scalar(128), -1);
                ImgProcCv2.Polylines(
                    drawing,
                    new Point[]
                    {
                        new Point(48, 18),
                        new Point(64, 30),
                        new Point(80, 18)
                    },
                    true,
                    new Scalar(210));
                ImgProcCv2.FillPoly(
                    drawing,
                    new Point[]
                    {
                        new Point(88, 18),
                        new Point(104, 30),
                        new Point(116, 18)
                    },
                    new Scalar(90));
                Point[] ellipsePoints = ImgProcCv2.Ellipse2Poly(new Point(96, 28), new Size(10, 6), 0, 0, 270, 30);
                Point[] contour = new Point[]
                {
                    new Point(0, 0),
                    new Point(4, 0),
                    new Point(4, 3),
                    new Point(0, 3)
                };
                double contourArea = ImgProcCv2.ContourArea(contour);
                double contourAreaFromSpan = ImgProcCv2.ContourArea(contour.AsSpan());
                double contourLength = ImgProcCv2.ArcLength(contour, true);
                double contourLengthFromSpan = ImgProcCv2.ArcLength(contour.AsSpan(), true);
                Point[] approxContour = ImgProcCv2.ApproxPolyDP(
                    new Point[]
                    {
                        new Point(0, 0),
                        new Point(2, 0),
                        new Point(4, 0),
                        new Point(4, 3),
                        new Point(0, 3)
                    },
                    0.5,
                    true);
                Rect boundingRect = ImgProcCv2.BoundingRect(approxContour);
                bool isConvex = ImgProcCv2.IsContourConvex(approxContour);
                Point[] convexHull = ImgProcCv2.ConvexHull(approxContour);
                int[] convexHullIndices = ImgProcCv2.ConvexHullIndices(approxContour);
                Point2f[] approxConvexPolygon = ImgProcCv2.ApproxPolyN(approxContour, 4);
                Point[] concaveContour = new Point[]
                {
                    new Point(0, 0),
                    new Point(4, 0),
                    new Point(4, 4),
                    new Point(2, 2),
                    new Point(0, 4)
                };
                int[] concaveHullIndices = ImgProcCv2.ConvexHullIndices(concaveContour);
                Vec4i[] convexityDefects = ImgProcCv2.ConvexityDefects(concaveContour, concaveHullIndices);
                ImgProcCv2.MinEnclosingCircle(approxContour, out Point2f enclosingCenter, out float enclosingRadius);
                double polygonTest = ImgProcCv2.PointPolygonTest(approxContour, new Point2f(2.0F, 1.0F), false);
                double shapeDistance = ImgProcCv2.MatchShapes(contour, approxContour, ShapeMatchModes.I1);
                RotatedRect minAreaRect = ImgProcCv2.MinAreaRect(approxContour);
                Point2f[] boxPoints = ImgProcCv2.BoxPoints(minAreaRect);
                Point[] ellipseFitPoints = new Point[]
                {
                    new Point(0, 2),
                    new Point(1, 0),
                    new Point(3, 0),
                    new Point(4, 2),
                    new Point(3, 4),
                    new Point(1, 4)
                };
                RotatedRect fitEllipse = ImgProcCv2.FitEllipse(ellipseFitPoints);
                RotatedRect fitEllipseAms = ImgProcCv2.FitEllipseAMS(ellipseFitPoints);
                RotatedRect fitEllipseDirect = ImgProcCv2.FitEllipseDirect(ellipseFitPoints);
                RectanglesIntersectTypes intersectionType = ImgProcCv2.RotatedRectangleIntersection(
                    minAreaRect,
                    new RotatedRect(new Point2f(2.5F, 1.5F), new Size2f(4.0F, 3.0F), 0.0F),
                    out Point2f[] intersectionRegion);
                Point2f[] closestEllipsePoints = ImgProcCv2.GetClosestEllipsePoints(fitEllipse, ellipseFitPoints);
                double enclosingTriangleArea = ImgProcCv2.MinEnclosingTriangle(approxContour, out Point2f[] enclosingTriangle);
                double enclosingConvexPolygonArea = ImgProcCv2.MinEnclosingConvexPolygon(approxContour, 4, out Point2f[] enclosingConvexPolygon);
                float intersectConvexArea = ImgProcCv2.IntersectConvexConvex(
                    contour,
                    new Point[]
                    {
                        new Point(2, 0),
                        new Point(6, 0),
                        new Point(6, 3),
                        new Point(2, 3)
                    },
                    out Point2f[] intersectConvexRegion);
                Vec4f fitLine = ImgProcCv2.FitLine(
                    new Point[]
                    {
                        new Point(0, 1),
                        new Point(2, 5),
                        new Point(4, 9)
                    },
                    DistanceTypes.L2,
                    0.0,
                    0.01,
                    0.01);
                ImgProcCv2.Polylines(drawing, ellipsePoints, false, new Scalar(150));
                ImgProcCv2.Circle(drawing, new Point(12, 12), 6, new Scalar(200), 1);
                ImgProcCv2.Ellipse(drawing, new Point(24, 12), new Size(8, 4), 0, 0, 360, new Scalar(64), 1);
                int baseLine;
                Size textSize = ImgProcCv2.GetTextSize("OpenCV", HersheyFonts.HersheySimplex, 0.45, 1, out baseLine);
                ImgProcCv2.PutText(drawing, "OpenCV", new Point(4, 32), HersheyFonts.HersheySimplex, 0.45, new Scalar(255));

                byte[] grayPixels = new byte[gray.ByteLength];
                byte[] thresholdPixels = new byte[thresholded.ByteLength];
                byte[] drawingPixels = new byte[drawing.ByteLength];
                gray.CopyTo(grayPixels);
                thresholded.CopyTo(thresholdPixels);
                drawing.CopyTo(drawingPixels);

                Console.WriteLine($"{gray.Rows}x{gray.Cols}, channels={gray.Channels}");
                Console.WriteLine($"{resized.Rows}x{resized.Cols}, channels={resized.Channels}");
                Console.WriteLine($"{kernel.Rows}x{kernel.Cols}, type={kernel.Type}");
                Console.WriteLine($"{eroded.Rows}x{eroded.Cols}, channels={eroded.Channels}");
                Console.WriteLine($"{dilated.Rows}x{dilated.Cols}, channels={dilated.Channels}");
                Console.WriteLine($"{opened.Rows}x{opened.Cols}, channels={opened.Channels}");
                Console.WriteLine($"clip={clipIntersects}, pt1={clipPt1}, pt2={clipPt2}");
                Console.WriteLine($"ellipsePoints={ellipsePoints.Length}");
                Console.WriteLine($"contourArea={contourArea}");
                Console.WriteLine($"contourAreaSpan={contourAreaFromSpan}");
                Console.WriteLine($"contourLength={contourLength}");
                Console.WriteLine($"contourLengthSpan={contourLengthFromSpan}");
                Console.WriteLine($"approxContour={approxContour.Length}");
                Console.WriteLine($"boundingRect={boundingRect}");
                Console.WriteLine($"isConvex={isConvex}");
                Console.WriteLine($"convexHull={convexHull.Length}");
                Console.WriteLine($"convexHullIndices={convexHullIndices.Length}");
                Console.WriteLine($"approxConvexPolygon={approxConvexPolygon.Length}");
                Console.WriteLine($"convexityDefects={convexityDefects.Length}");
                Console.WriteLine($"enclosingCircle={enclosingCenter}, radius={enclosingRadius}");
                Console.WriteLine($"polygonTest={polygonTest}");
                Console.WriteLine($"shapeDistance={shapeDistance}");
                Console.WriteLine($"minAreaRect={minAreaRect}");
                Console.WriteLine($"boxPoints={boxPoints.Length}");
                Console.WriteLine($"fitEllipse={fitEllipse}");
                Console.WriteLine($"fitEllipseAms={fitEllipseAms}");
                Console.WriteLine($"fitEllipseDirect={fitEllipseDirect}");
                Console.WriteLine($"intersectionType={intersectionType}, intersectionRegion={intersectionRegion.Length}");
                Console.WriteLine($"closestEllipsePoints={closestEllipsePoints.Length}");
                Console.WriteLine($"enclosingTriangleArea={enclosingTriangleArea}, enclosingTriangle={enclosingTriangle.Length}");
                Console.WriteLine($"enclosingConvexPolygonArea={enclosingConvexPolygonArea}, enclosingConvexPolygon={enclosingConvexPolygon.Length}");
                Console.WriteLine($"intersectConvexArea={intersectConvexArea}, intersectConvexRegion={intersectConvexRegion.Length}");
                Console.WriteLine($"fitLine={fitLine}");
                Console.WriteLine($"text={textSize.Width}x{textSize.Height}, baseline={baseLine}");
                Console.WriteLine(string.Join(",", grayPixels));
                Console.WriteLine(string.Join(",", thresholdPixels));
                Console.WriteLine(string.Join(",", drawingPixels));
            }
        }
    }
}
```
