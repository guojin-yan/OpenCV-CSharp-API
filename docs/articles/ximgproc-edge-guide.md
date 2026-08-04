# XImgProc Edge Guide / XImgProc Edge 指南

`JYPPX.OpenCvSharp.XImgProc` includes the second-batch edge and proposal APIs from OpenCV contrib `ximgproc`.

`JYPPX.OpenCvSharp.XImgProc` 已包含 OpenCV contrib `ximgproc` 第二批 edge 与 proposal API。

## Scope / 范围

- `FastBilateralSolverFilter`: reusable fast bilateral solver object plus one-shot `XImgProcCv2.FastBilateralSolverFilter`.
- `EdgeDrawing`: edge segment detection, edge/gradient image output, grouped segments, line output, segment indices, and ellipse candidates.
- `EdgeDrawingParams`: managed parameter structure including gradient operator, thresholds, scan/path controls, and line fitting values.
- `EdgeBoxes`: object proposal generator from edge and orientation maps.
- `EdgeBox` and `EdgeDrawingEllipse`: managed value objects for count/fill outputs.

- `FastBilateralSolverFilter`：可复用 fast bilateral solver 对象，以及一次性 `XImgProcCv2.FastBilateralSolverFilter`。
- `EdgeDrawing`：边缘片段检测、edge/gradient 图输出、分组 segments、line 输出、segment indices 和 ellipse candidates。
- `EdgeDrawingParams`：managed 参数结构，包含梯度算子、阈值、scan/path 控制和 line fitting 数值。
- `EdgeBoxes`：从 edge map 与 orientation map 生成 object proposal。
- `EdgeBox` 与 `EdgeDrawingEllipse`：用于 count/fill 输出的 managed 值对象。

## Runtime Notes / 运行时说明

`FastBilateralSolverFilter` depends on the OpenCV build configuration. Some local builds throw an OpenCV error saying the algorithm needs EIGEN support. Tests and samples treat that as a runtime capability absence, not a managed wrapper failure.

`FastBilateralSolverFilter` 依赖 OpenCV 构建配置。某些本地构建会抛出需要 EIGEN 支持的 OpenCV 错误。测试和示例会将其视为 runtime capability 不存在，而不是 managed wrapper 失败。

`StructuredEdgeDetection` needs a caller-supplied model file and is not part of the default smoke path.

`StructuredEdgeDetection` 需要调用方提供模型文件，不属于默认 smoke 路径。

## Example / 示例

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.XImgProc;

namespace XImgProcEdgeExample
{
    internal static class Program
    {
        private static void Main()
        {
            using Mat image = new Mat(48, 48, MatType.CV_8UC1, new Scalar(0));
            Cv2.Line(image, new Point(4, 4), new Point(42, 34), new Scalar(255), 1);
            Cv2.Circle(image, new Point(24, 24), 8, new Scalar(255), 1);

            using EdgeDrawing drawing = XImgProcCv2.CreateEdgeDrawing();
            EdgeDrawingParams parameters = drawing.Params;
            parameters.MinLineLength = 4;
            parameters.MinPathLength = 4;
            drawing.Params = parameters;

            drawing.DetectEdges(image);
            using Mat edgeImage = drawing.GetEdgeImage();
            Point[][] segments = drawing.GetSegments();
            LineSegment[] lines = drawing.DetectLines();
            EdgeDrawingEllipse[] ellipses = drawing.DetectEllipses();

            using Mat edgeMap = new Mat(48, 48, MatType.CV_32FC1, new Scalar(0.1));
            using Mat orientationMap = new Mat(48, 48, MatType.CV_32FC1, new Scalar(0.0));
            using EdgeBoxes boxes = XImgProcCv2.CreateEdgeBoxes(maxBoxes: 5, minScore: 0.0F, minBoxArea: 4.0F);
            EdgeBox[] proposals = boxes.GetBoundingBoxes(edgeMap, orientationMap);
        }
    }
}
```

## Smoke / Smoke

Tiny smoke allows zero lines, ellipses, or proposals because OpenCV edge algorithms can validly return no detections for small synthetic inputs. The wrapper contract is the stable object lifetime, output shape, and count/fill marshalling behavior.

tiny smoke 允许返回零条线段、零个椭圆或零个 proposal，因为 OpenCV edge 算法对小型合成输入合法地可能无检测结果。wrapper 契约关注稳定对象生命周期、输出形状和 count/fill marshalling 行为。
