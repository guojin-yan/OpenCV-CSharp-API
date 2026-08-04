# Surface Matching Guide / Surface Matching 指南

`JYPPX.OpenCvSharp.SurfaceMatching` wraps the first OpenCV contrib `surface_matching` registration and PPF detection surface with opaque native handles and flat result objects.

`JYPPX.OpenCvSharp.SurfaceMatching` 通过 opaque native 句柄和平铺结果对象封装第一批 OpenCV contrib `surface_matching` 配准与 PPF 检测能力。

## Scope / 范围

- ICP: `Icp`, `IcpSamplingType`, and `IcpRegistrationResult`.
- PPF detector: `Ppf3DDetector`, `TrainModel`, `SetSearchParams`, and `Match`.
- Pose summaries: `Pose3DResult` with translation, quaternion, residual, votes, and row-major 4x4 pose.
- Factory helpers: `SurfaceMatchingCv2.CreateIcp` and `CreatePpf3DDetector`.

- ICP：`Icp`、`IcpSamplingType` 和 `IcpRegistrationResult`。
- PPF detector：`Ppf3DDetector`、`TrainModel`、`SetSearchParams` 和 `Match`。
- pose 摘要：`Pose3DResult`，包含平移、四元数、残差、投票数和行优先 4x4 pose。
- 工厂 helper：`SurfaceMatchingCv2.CreateIcp` 与 `CreatePpf3DDetector`。

## Runtime / 运行时

`surface_matching` is an optional OpenCV contrib module. Runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_surface_matching500.dll` when the module is built. If a runtime lacks it, the managed API shape remains stable and linked calls report `NOT_LINKED`.

`surface_matching` 是可选 OpenCV contrib 模块。构建该模块时 runtime staging 会包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_surface_matching500.dll`。如果某个 runtime 缺少它，managed API 形状仍保持稳定，linked 调用会报告 `NOT_LINKED`。

## Input Notes / 输入说明

OpenCV ICP and PPF expect point clouds with normals, commonly `Nx6 CV_32FC1`: `x, y, z, nx, ny, nz`. The tiny smoke path verifies linked call shape only. Real model-to-scene matching needs sufficiently sampled and normalized geometry.

OpenCV ICP 与 PPF 期望带法线点云，常见格式为 `Nx6 CV_32FC1`：`x, y, z, nx, ny, nz`。tiny smoke 只验证 linked 调用形状。真实 model-to-scene 匹配需要采样充分且法线合理的几何数据。

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.SurfaceMatching;

internal static class Program
{
    private static void Main()
    {
        using Mat cloud = new Mat(8, 6, MatType.CV_32FC1);
        cloud.CopyFrom(new float[]
        {
            0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 1,
            0, 1, 0, 0, 0, 1,
            1, 1, 0, 0, 0, 1,
            0, 0, 1, 0, 0, 1,
            1, 0, 1, 0, 0, 1,
            0, 1, 1, 0, 0, 1,
            1, 1, 1, 0, 0, 1
        });

        using Icp icp = SurfaceMatchingCv2.CreateIcp(iterations: 1, tolerance: 0.05F, numLevels: 1);
        IcpRegistrationResult icpResult = icp.RegisterModelToScene(cloud, cloud);

        using Ppf3DDetector detector = SurfaceMatchingCv2.CreatePpf3DDetector(0.2, 0.2, 20.0);
        detector.SetSearchParams();
        detector.TrainModel(cloud);
        Pose3DResult[] poses = detector.Match(cloud, 1.0, 0.2);
    }
}
```

On very small clouds OpenCV can reject the data with an assertion or numeric boundary. Treat that as an input-size limitation rather than a managed ABI failure.

在非常小的点云上，OpenCV 可能因为断言或数值边界拒绝输入。这属于输入规模限制，不代表 managed ABI 失败。
