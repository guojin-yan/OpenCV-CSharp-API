# RAPID Guide / RAPID 指南

`OpenCvSharp.Rapid` wraps the first OpenCV contrib `rapid` silhouette-tracking helpers and tracker objects.

`OpenCvSharp.Rapid` 封装第一批 OpenCV contrib `rapid` 轮廓跟踪 helper 和 tracker 对象。

## Scope / 范围

- Static helpers: `DrawCorrespondencies`, `DrawSearchLines`, `DrawWireframe`, `ExtractControlPoints`, `ExtractLineBundle`, `FindCorrespondencies`, `ConvertCorrespondencies`, and `Run`.
- Result value: `RapidResult`.
- Trackers: `RapidTracker`, `RapidSilhouetteTracker`, and `OlsTracker`.
- Tracker calls: `Compute` and `ClearState`.

- 静态 helper：`DrawCorrespondencies`、`DrawSearchLines`、`DrawWireframe`、`ExtractControlPoints`、`ExtractLineBundle`、`FindCorrespondencies`、`ConvertCorrespondencies` 和 `Run`。
- 结果值：`RapidResult`。
- tracker：`RapidTracker`、`RapidSilhouetteTracker` 和 `OlsTracker`。
- tracker 调用：`Compute` 与 `ClearState`。

## Runtime / 运行时

`rapid` is an optional OpenCV contrib module. Runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_rapid500.dll` when the module is built. If a runtime lacks it, the managed API shape remains stable and linked calls report `NOT_LINKED`.

`rapid` 是可选 OpenCV contrib 模块。构建该模块时 runtime staging 会包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_rapid500.dll`。如果某个 runtime 缺少它，managed API 形状仍保持稳定，linked 调用会报告 `NOT_LINKED`。

`GOSTracker` is not exposed in this batch because the local OpenCV 5.0.0 header/source surface returns the `OLSTracker` pointer type from its factory. The first managed API keeps that ambiguity inside native code.

本批次未暴露 `GOSTracker`，因为本地 OpenCV 5.0.0 header/source 的工厂返回 `OLSTracker` 指针类型。第一批 managed API 将这个歧义留在 native 内部。

## Input Notes / 输入说明

RAPID uses caller-owned mesh, camera, pose, and image matrices. The tiny smoke path uses a synthetic square mesh and edge image, checking call-path and finite result shape only. Real tracking quality depends on edge content, camera calibration, mesh topology, and pose initialization.

RAPID 使用调用方持有的网格、相机、位姿和图像矩阵。tiny smoke 使用合成方形网格和边缘图，只检查调用路径与有限结果形状。真实跟踪质量取决于边缘内容、相机标定、网格拓扑和位姿初值。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.Rapid;

internal static class Program
{
    private static void Main()
    {
        using Mat edge = new Mat(64, 64, MatType.CV_8UC1, new Scalar(0));
        using Mat mesh = new Mat(4, 1, MatType.CV_32FC3);
        using Mat tris = new Mat(2, 1, MatType.CV_32SC3);
        using Mat camera = new Mat(3, 3, MatType.CV_64FC1);
        using Mat rvec = new Mat(3, 1, MatType.CV_64FC1);
        using Mat tvec = new Mat(3, 1, MatType.CV_64FC1);
        using Mat pts2d = new Mat(4, 1, MatType.CV_32FC2);

        mesh.CopyFrom(new float[] { -1, -1, 0, 1, -1, 0, 1, 1, 0, -1, 1, 0 });
        tris.CopyFrom(new[] { 0, 1, 2, 0, 2, 3 });
        camera.CopyFrom(new double[] { 60, 0, 32, 0, 60, 32, 0, 0, 1 });
        rvec.CopyFrom(new double[] { 0, 0, 0 });
        tvec.CopyFrom(new double[] { 0, 0, 6 });
        pts2d.CopyFrom(new float[] { 12, 12, 52, 12, 52, 52, 12, 52 });

        using Mat wire = edge.Clone();
        RapidCv2.DrawWireframe(wire, pts2d, tris, new Scalar(255), LineTypes.Line8);
        RapidResult result = RapidCv2.Run(edge, 8, 3, mesh, tris, camera, rvec, tvec, computeRmsd: true);

        using RapidSilhouetteTracker tracker = RapidSilhouetteTracker.Create(mesh, tris);
        tracker.ClearState();
    }
}
```

On very small meshes or sparse edge images, OpenCV can report contour or assertion boundaries. Increase mesh detail, edge density, and search length for real use.

在非常小的网格或稀疏边缘图上，OpenCV 可能报告 contour 或 assertion 边界。真实使用时应提高网格细节、边缘密度和搜索长度。
