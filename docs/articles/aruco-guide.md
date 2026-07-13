# ArUco Guide

`OpenCvSharp.ObjDetect` exposes the first ArUco batch from the main OpenCV `objdetect` module. These APIs are not contrib `xobjdetect` APIs; they are staged with the factual OpenCV 5.0.0 runtime artifact `opencv_objdetect500.dll`.

`OpenCvSharp.ObjDetect` 暴露 OpenCV 主线 `objdetect` 模块的第一批 ArUco 能力。这些 API 不是 contrib `xobjdetect` API，而是随事实性 OpenCV 5.0.0 runtime 产物 `opencv_objdetect500.dll` 暂存。

## Covered APIs / 已覆盖接口

- `ArucoDictionary`: default dictionaries, predefined dictionaries, byte-list access, marker-size and correction-bit access, marker image generation, marker bits, identification, distance-to-id, and bit/byte-list conversion helpers.
- `ArucoDetectorParameters`: flattened scalar fields from the local OpenCV 5.0.0 `DetectorParameters` header.
- `ArucoRefineParameters`: minimum reprojection distance, error-correction rate, and corner-order checking.
- `ArucoDetector`: dictionary get/set, detector parameter get/set, refine parameter get/set, `DetectMarkers`, `DetectMarkersWithConfidence`, and `RefineDetectedMarkers`.
- `ArucoGridBoard`: grid-board creation, size/spacing/marker-length queries, and printable image generation.
- `PredefinedDictionaryType`, `CornerRefineMethod`, `ArucoDetectionResult`, and `ArucoIdentificationResult`.

- `ArucoDictionary`：默认字典、预定义字典、byte-list 访问、marker size 和纠错 bit 访问、marker 图像生成、marker bits、识别、到指定 id 的距离，以及 bit/byte-list 转换辅助。
- `ArucoDetectorParameters`：按本地 OpenCV 5.0.0 `DetectorParameters` 头文件平铺的标量字段。
- `ArucoRefineParameters`：最小重投影距离、纠错比例和角点顺序检查。
- `ArucoDetector`：字典 get/set、检测参数 get/set、细化参数 get/set、`DetectMarkers`、`DetectMarkersWithConfidence` 和 `RefineDetectedMarkers`。
- `ArucoGridBoard`：网格 board 创建、尺寸/间距/marker 边长查询，以及可打印图像生成。
- `PredefinedDictionaryType`、`CornerRefineMethod`、`ArucoDetectionResult` 和 `ArucoIdentificationResult`。

## Marker Generation / Marker 生成

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ObjDetect;

namespace ArucoMarkerSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (ArucoDictionary dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50))
            using (Mat marker = dictionary.GenerateImageMarker(0, 128))
            {
                System.Console.WriteLine("Marker=" + marker.Rows + "x" + marker.Cols);
            }
        }
    }
}
```

`GenerateImageMarker` returns a managed-owned `Mat`; dispose it when the image is no longer needed.

`GenerateImageMarker` 返回由 managed 层拥有的 `Mat`；图像不再使用时需要释放。

## Detection Result Layout / 检测结果布局

`ArucoDetector.DetectMarkers` returns grouped corners as `Point2f[][]`, ids as `int[]`, and rejected candidates as `Point2f[][]`. The native ABI uses a two-step count/fill call with group offsets and a flat point buffer. No `std::vector` or `OutputArrayOfArrays` crosses the C ABI.

`ArucoDetector.DetectMarkers` 返回 `Point2f[][]` 形式的角点分组、`int[]` id 和 `Point2f[][]` 形式的 rejected candidates。native ABI 使用 count/fill 两阶段调用，并通过 group offsets 和扁平点缓冲区传递数据。`std::vector` 或 `OutputArrayOfArrays` 不会穿过 C ABI。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ObjDetect;

namespace ArucoDetectSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (ArucoDictionary dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50))
            using (ArucoDetector detector = new ArucoDetector(dictionary))
            using (Mat image = dictionary.GenerateImageMarker(0, 128))
            {
                ArucoDetectionResult result = detector.DetectMarkers(image);
                System.Console.WriteLine("Markers=" + result.Count + ", rejected=" + result.RejectedCandidates.Length);
            }
        }
    }
}
```

Real detection quality still depends on marker size, border bits, image resolution, blur, exposure, and the selected dictionary.

真实检测效果仍取决于 marker 尺寸、边框 bit、图像分辨率、模糊、曝光和选用字典。

## Grid Board / 网格板

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ObjDetect;

namespace ArucoBoardSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (ArucoDictionary dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict5X5_100))
            using (ArucoGridBoard board = new ArucoGridBoard(new Size(4, 3), 0.04F, 0.01F, dictionary))
            using (Mat boardImage = board.GenerateImage(new Size(640, 480), 16, 1))
            {
                System.Console.WriteLine("Board=" + boardImage.Size);
            }
        }
    }
}
```

ChArUco board and `CharucoDetector` are covered in the dedicated [ChArUco Guide](charuco-guide.md), using the same stable point-array ABI as marker detection.

ChArUco board 和 `CharucoDetector` 见专门的 [ChArUco Guide](charuco-guide.md)，并复用 marker 检测同一套稳定点集数组 ABI。

`ArucoDetector.RefineDetectedMarkers` is covered in the dedicated [ArUco Refine Guide](aruco-refine-guide.md).

`ArucoDetector.RefineDetectedMarkers` 见专门的 [ArUco Refine Guide](aruco-refine-guide.md)。

## Runtime Notes / 运行时说明

ArUco APIs require the factual OpenCV 5.0.0 runtime artifact `opencv_objdetect500.dll`. When native OpenCV is not linked, the ABI is still exported and reports the defined `NOT_LINKED` status.

ArUco API 需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_objdetect500.dll`。当 native OpenCV 未链接时，ABI 仍然导出，并返回定义明确的 `NOT_LINKED` 状态。
