# ArUco Refine Guide

`ArucoDetector.RefineDetectedMarkers` refines marker detections against an `ArucoGridBoard`, using current detections and rejected candidates as grouped `Point2f[][]` inputs. The first implementation supports grid boards; ChArUco board sharing is intentionally left for a later shared-board-handle pass.

`ArucoDetector.RefineDetectedMarkers` 使用 `ArucoGridBoard`、当前检测结果和 rejected candidates 对 marker 检测进行细化，输入点集为分组 `Point2f[][]`。第一版支持 grid board；ChArUco board 的共享 board handle 会在后续阶段处理。

## API / API

- `ArucoDetector.RefineDetectedMarkers(...)`
- `ArucoRefineResult`

- `ArucoDetector.RefineDetectedMarkers(...)`
- `ArucoRefineResult`

The result contains refined corners, ids, rejected candidates, and recovered candidate indices. All arrays are managed copies owned by the caller.

结果包含细化后的角点、ID、rejected candidates 和 recovered candidate indices。所有数组都是调用方拥有的 managed 副本。

## Example / 示例

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ObjDetect;

namespace ArucoRefineSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (ArucoDictionary dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50))
            using (ArucoDetector detector = new ArucoDetector(dictionary))
            using (ArucoGridBoard board = new ArucoGridBoard(new Size(2, 2), 0.04F, 0.01F, dictionary))
            using (Mat image = dictionary.GenerateImageMarker(0, 96))
            {
                ArucoDetectionResult detected = detector.DetectMarkers(image);
                ArucoRefineResult refined = detector.RefineDetectedMarkers(
                    image,
                    board,
                    detected.Corners,
                    detected.Ids,
                    detected.RejectedCandidates);

                Console.WriteLine(refined.Count);
                Console.WriteLine(refined.RecoveredIndices.Length);
            }
        }
    }
}
```

## Runtime Notes / 运行时说明

ArUco refine belongs to the main OpenCV `objdetect` boundary and requires the factual OpenCV 5.0.0 runtime artifact `opencv_objdetect500.dll`. The native ABI uses count/fill calls with group offsets and flat point buffers; no STL container crosses the exported C ABI.

ArUco refine 属于 OpenCV 主线 `objdetect` 边界，需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_objdetect500.dll`。native ABI 使用 count/fill 两阶段调用、分组偏移和扁平点缓冲区；不会在导出的 C ABI 中暴露 STL 容器。
