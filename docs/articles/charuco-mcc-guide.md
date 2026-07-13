# Charuco And MCC Guide

Local OpenCV 5.0.0 keeps ArUco, Charuco, and MCC under the main `objdetect` module in the local source tree. The wrapper now exposes ChArUco board/detector objects, MCC checker detector/checker objects, and the grouped point-array layout used by their native ABI.

在本地 OpenCV 5.0.0 源码树中，ArUco、Charuco 和 MCC 都位于主线 `objdetect` 模块下。当前 wrapper 已暴露 ChArUco board/detector 对象、MCC checker detector/checker 对象，以及它们 native ABI 使用的分组点集数组布局。

## Covered APIs / 已覆盖接口

- `CharucoBoard`: board creation, image generation, board geometry queries, chessboard corner output, legacy pattern switch, and collinearity checks.
- `CharucoDetector`: board get/set, parameter get/set, and `DetectBoard` with grouped marker-corner input/output.
- `ColorChart`: `Mcc24`, `Sg140`, and `Vinyl18`.
- `DetectorParametersMCC`: flattened scalar defaults for OpenCV MCC checker detection parameters.
- `CCheckerDetector`: process, process-with-ROI, best/list checker retrieval, draw, reference colors, detection params, and chart type.
- `CChecker`: target, box, color chart centers, RGB/YCbCr Mat data, cost, and center.

- `CharucoBoard`：board 创建、图像生成、board 几何查询、棋盘角点输出、legacy pattern 开关和共线检查。
- `CharucoDetector`：board get/set、参数 get/set，以及带分组 marker-corner 输入/输出的 `DetectBoard`。
- `ColorChart`：`Mcc24`、`Sg140` 和 `Vinyl18`。
- `DetectorParametersMCC`：OpenCV MCC 色卡检测参数的平铺标量默认值。
- `CCheckerDetector`：process、process-with-ROI、最佳/列表 checker 获取、绘制、参考颜色、检测参数和色卡类型。
- `CChecker`：target、box、色块中心、RGB/YCbCr Mat 数据、cost 和 center。

## ChArUco / ChArUco

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ObjDetect;

namespace CharucoSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (ArucoDictionary dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50))
            using (CharucoBoard board = new CharucoBoard(new Size(4, 5), 0.04F, 0.02F, dictionary))
            using (CharucoDetector detector = new CharucoDetector(board))
            using (Mat image = board.GenerateImage(new Size(640, 480), 16, 1))
            {
                CharucoDetectionResult result = detector.DetectBoard(image);
                System.Console.WriteLine("Board=" + board.ChessboardSize + ", corners=" + result.Count);
            }
        }
    }
}
```

`DetectBoard` accepts optional marker corners as `Point2f[][]` plus matching marker ids. Managed arrays are borrowed only during the native call; returned arrays are managed copies owned by the caller.

`DetectBoard` 可接收可选的 marker corners（`Point2f[][]`）和对应 marker id。managed 输入数组只在 native 调用期间被借用；返回数组是调用方拥有的 managed 副本。

## MCC Checker / MCC 色卡

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ObjDetect;

namespace MccCheckerSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (CCheckerDetector detector = new CCheckerDetector())
            using (Mat image = new Mat(640, 480, MatType.CV_8UC3))
            {
                DetectorParametersMCC parameters = detector.GetDetectionParams();
                parameters.ConfidenceThreshold = 0.6;
                detector.SetDetectionParams(parameters);
                detector.ColorChartType = ColorChart.Mcc24;

                bool found = detector.Process(image);
                CChecker? best = detector.GetBestColorChecker();

                System.Console.WriteLine("Found=" + found + ", best=" + (best != null));
                best?.Dispose();
            }
        }
    }
}
```

Detector output checker handles are owned by the managed `CChecker` instances. Dispose checker objects when you keep them beyond a short scope.

detector 输出的 checker handle 由 managed `CChecker` 实例拥有。若 checker 对象离开短作用域后仍被保留，请显式释放。

## Deferred / 后续内容

OpenCV exposes optional DNN-assisted MCC detector creation and DNN toggles behind OpenCV DNN build macros. This wrapper keeps the non-DNN checker object chain stable first; DNN model loading will be added once the full linked smoke runtime includes `objdetect,dnn,photo,videoio,stereo,xobjdetect`.

OpenCV 将 DNN 辅助的 MCC detector 创建和 DNN 开关放在 OpenCV DNN 构建宏之后。当前 wrapper 先稳定非 DNN checker 对象链；等完整 linked smoke runtime 包含 `objdetect,dnn,photo,videoio,stereo,xobjdetect` 后，再补 DNN 模型加载。

## Runtime Notes / 运行时说明

ChArUco and MCC APIs are part of the factual OpenCV 5.0.0 runtime artifact `opencv_objdetect500.dll`, not contrib `xobjdetect`. Real ChArUco detection needs marker/board images with readable corners. Real MCC detection depends on color-checker images with sufficient size, contrast, and chart visibility. DNN-assisted MCC workflows additionally require the factual OpenCV 5.0.0 runtime artifact `opencv_dnn500.dll` and user-supplied models.

ChArUco 和 MCC API 属于事实性 OpenCV 5.0.0 runtime 产物 `opencv_objdetect500.dll`，不是 contrib `xobjdetect`。真实 ChArUco 检测需要角点可读的 marker/board 图像。真实 MCC 检测依赖尺寸、对比度和色卡可见性足够的图像。DNN 辅助 MCC 工作流还需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_dnn500.dll` 和用户提供的模型。
