# ChArUco Guide

`JYPPX.OpenCvSharp.ObjDetect` exposes `CharucoBoard`, `CharucoDetector`, `CharucoParameters`, and `CharucoDetectionResult` for OpenCV ChArUco workflows.

`JYPPX.OpenCvSharp.ObjDetect` 为 OpenCV ChArUco 工作流暴露 `CharucoBoard`、`CharucoDetector`、`CharucoParameters` 和 `CharucoDetectionResult`。

## Basic Use / 基本用法

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ObjDetect;

namespace CharucoGuideSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (ArucoDictionary dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict5X5_100))
            using (CharucoBoard board = new CharucoBoard(new Size(5, 7), 0.04F, 0.02F, dictionary))
            using (CharucoDetector detector = new CharucoDetector(board))
            using (Mat printable = board.GenerateImage(new Size(800, 600), 24, 1))
            {
                CharucoDetectionResult result = detector.DetectBoard(printable);
                System.Console.WriteLine("Printable=" + printable.Size + ", ChArUco corners=" + result.Count);
            }
        }
    }
}
```

`CharucoBoard.GetChessboardCorners()` returns a managed `Point3f[]` copy of board-space corners. `CharucoDetector.DetectBoard` returns managed copies of ChArUco corners, ChArUco ids, marker corner groups, and marker ids.

`CharucoBoard.GetChessboardCorners()` 返回 board 坐标系角点的 managed `Point3f[]` 副本。`CharucoDetector.DetectBoard` 返回 ChArUco 角点、ChArUco id、marker 角点分组和 marker id 的 managed 副本。

## Marker Input / Marker 输入

When you already detected markers, pass `Point2f[][] markerCorners` and `int[] markerIds` to `DetectBoard`. The group count must match the id count.

当你已经检测出 marker，可将 `Point2f[][] markerCorners` 和 `int[] markerIds` 传给 `DetectBoard`。分组数量必须和 id 数量一致。

```csharp
CharucoDetectionResult result = detector.DetectBoard(image, markerCorners, markerIds);
```

The native ABI receives an offset table plus a flat `Point2f` buffer. The input buffers are borrowed only during the call; the output arrays are managed copies.

native ABI 接收偏移表和扁平 `Point2f` 缓冲区。输入缓冲只在调用期间被借用；输出数组是 managed 副本。

## Runtime Notes / 运行时说明

ChArUco lives in the main OpenCV `objdetect` module and requires the factual OpenCV 5.0.0 runtime artifact `opencv_objdetect500.dll`. It is separate from contrib `xobjdetect`.

ChArUco 位于 OpenCV 主线 `objdetect` 模块，需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_objdetect500.dll`。它不同于 contrib `xobjdetect`。
