# MCC Checker Guide

`JYPPX.OpenCvSharp.ObjDetect` exposes OpenCV MCC checker detection through `CCheckerDetector`, `CChecker`, `DetectorParametersMCC`, and `ColorChart`.

`JYPPX.OpenCvSharp.ObjDetect` 通过 `CCheckerDetector`、`CChecker`、`DetectorParametersMCC` 和 `ColorChart` 暴露 OpenCV MCC 色卡检测能力。

## Detector / 检测器

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ObjDetect;

namespace MccCheckerGuideSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (CCheckerDetector detector = new CCheckerDetector())
            using (Mat image = new Mat(640, 480, MatType.CV_8UC3))
            {
                DetectorParametersMCC parameters = detector.GetDetectionParams();
                parameters.ConfidenceThreshold = 0.55;
                detector.SetDetectionParams(parameters);
                detector.ColorChartType = ColorChart.Mcc24;

                bool found = detector.Process(image, nc: 1);
                CChecker[] checkers = detector.GetListColorChecker();

                System.Console.WriteLine("Found=" + found + ", checkers=" + checkers.Length);
                foreach (CChecker checker in checkers)
                {
                    checker.Dispose();
                }
            }
        }
    }
}
```

`Process` stores detections in the detector. `GetBestColorChecker` and `GetListColorChecker` return owned managed wrappers over native checker handles.

`Process` 会把检测结果保存在 detector 内部。`GetBestColorChecker` 和 `GetListColorChecker` 返回拥有 native checker handle 的 managed wrapper。

## Checker Data / Checker 数据

`CChecker` can expose the detected quadrilateral, color patch centers, sampled RGB/YCbCr matrices, cost, center point, and target chart type. Mat-returning methods transfer a new managed-owned `Mat`.

`CChecker` 可暴露检测四边形、色块中心、采样 RGB/YCbCr 矩阵、cost、中心点和目标色卡类型。返回 `Mat` 的方法会转交一个新的 managed-owned `Mat`。

## Runtime Notes / 运行时说明

MCC belongs to the main factual OpenCV 5.0.0 runtime artifact `opencv_objdetect500.dll`. Non-DNN detection is available through this object chain. DNN-assisted MCC APIs are deferred until the full linked smoke runtime includes the factual OpenCV 5.0.0 runtime artifact `opencv_dnn500.dll` and a stable model-loading contract.

MCC 属于主线事实性 OpenCV 5.0.0 runtime 产物 `opencv_objdetect500.dll`。当前对象链提供非 DNN 检测。DNN 辅助 MCC API 会等完整 linked smoke runtime 包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_dnn500.dll` 且模型加载契约稳定后再补。

A freshly created `CChecker` is only a mutable result container. Its target chart value is meaningful after detection or after explicitly setting `Target`; smoke tests should validate explicit set/get wiring instead of assuming a default target from an uninitialized native checker.

刚创建的 `CChecker` 只是一个可变结果容器。它的目标色卡值只有在检测之后或显式设置 `Target` 之后才有意义；smoke 测试应验证显式 set/get 接线，而不是假设未初始化 native checker 的默认 target。
