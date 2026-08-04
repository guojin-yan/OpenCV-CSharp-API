# Face Alignment Guide / Face Alignment 指南

`JYPPX.OpenCvSharp.Face` now includes the OpenCV 5.0.0 contrib facemark base surface and `FacemarkLBF`. The linked runtime module is the factual OpenCV 5.0.0 runtime artifact `opencv_face500.dll`.

`JYPPX.OpenCvSharp.Face` 现在包含 OpenCV 5.0.0 contrib facemark 基类接口面和 `FacemarkLBF`。linked runtime 模块是事实性 OpenCV 5.0.0 runtime 产物 `opencv_face500.dll`。

## Scope / 范围

- `Facemark`: `LoadModel`, `Save`, `Fit`, and `FacemarkFitResult`.
- `FacemarkTrain`: `AddTrainingSample`, `Training`, and `GetFaces`.
- `FacemarkLBF`: OpenCV LBF aligner creation with `FacemarkLBFParams`.
- Stable marshaling: face rectangles use flat `Rect` buffers; landmark output uses count/fill offsets plus flat `Point2f` data.

- `Facemark`：`LoadModel`、`Save`、`Fit` 和 `FacemarkFitResult`。
- `FacemarkTrain`：`AddTrainingSample`、`Training` 和 `GetFaces`。
- `FacemarkLBF`：通过 `FacemarkLBFParams` 创建 OpenCV LBF aligner。
- 稳定封送：人脸矩形使用平铺 `Rect` 缓冲；关键点输出使用 count/fill offsets 加平铺 `Point2f` 数据。

## Model Files / 模型文件



When the `face` contrib module is not linked, the managed API remains present and calls report a clear `NOT_LINKED` boundary.

如果未链接 `face` contrib 模块，managed API 仍然存在，调用会报告明确的 `NOT_LINKED` 边界。

## Minimal Shape Sample / 最小形状示例

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Face;

namespace FaceAlignmentSample
{
    internal static class Program
    {
        private static void Main()
        {
            var parameters = new FacemarkLBFParams
            {
                NLandmarks = 68,
                InitialShapeCount = 1,
                StageCount = 1,
                TreeCount = 1,
                TreeDepth = 2,
                FeatureCounts = new[] { 8 },
                RadiusValues = new[] { 0.2 },
                SaveModel = false
            };

            using (Mat image = new Mat(32, 32, MatType.CV_8UC1, new Scalar(80)))
            using (FacemarkLBF facemark = FacemarkLBF.Create(parameters))
            {
                facemark.AddTrainingSample(image, CreateLandmarks());
                Console.WriteLine("LBF landmarks=" + facemark.NLandmarks + ", stages=" + facemark.StageCount);
            }
        }

        private static Point2f[] CreateLandmarks()
        {
            var points = new Point2f[68];
            for (int i = 0; i < points.Length; i++)
            {
                float angle = (float)(i * Math.PI * 2.0 / points.Length);
                points[i] = new Point2f(16.0F + 8.0F * (float)Math.Cos(angle), 16.0F + 9.0F * (float)Math.Sin(angle));
            }

            return points;
        }
    }
}
```

For real fitting, load or train a compatible model and pass detected face rectangles:

真实拟合时，需要加载或训练兼容模型，并传入检测到的人脸矩形：

```csharp
FacemarkFitResult result = facemark.Fit(image, new[] { new Rect(4, 4, 24, 24) });
```
