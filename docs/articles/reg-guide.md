# Reg Guide / Reg 指南

`OpenCvSharp.Reg` wraps the first OpenCV contrib `reg` image-registration surface with opaque map and mapper handles.

`OpenCvSharp.Reg` 通过 opaque map 与 mapper 句柄封装第一批 OpenCV contrib `reg` 图像配准能力。

## Scope / 范围

- Maps: `RegMap`, `MapShift`, `MapAffine`, `MapProjec`, `RegMapKind`.
- Flat transform values: `AffineTransform2D` and `ProjectiveTransform2D`.
- Map operations: `Warp`, `InverseWarp`, `InverseMap`, `Compose`, and `Scale`.
- Mappers: `MapperGradShift`, `MapperGradEuclid`, `MapperGradSimilar`, `MapperGradAffine`, `MapperGradProj`, and `MapperPyramid`.
- Factory helpers: `RegCv2.CreateMapShift`, `CreateMapAffine`, `CreateMapProjec`, and mapper factories.

- map：`RegMap`、`MapShift`、`MapAffine`、`MapProjec`、`RegMapKind`。
- 平铺变换值：`AffineTransform2D` 与 `ProjectiveTransform2D`。
- map 操作：`Warp`、`InverseWarp`、`InverseMap`、`Compose` 和 `Scale`。
- mapper：`MapperGradShift`、`MapperGradEuclid`、`MapperGradSimilar`、`MapperGradAffine`、`MapperGradProj` 和 `MapperPyramid`。
- 工厂 helper：`RegCv2.CreateMapShift`、`CreateMapAffine`、`CreateMapProjec` 以及 mapper 工厂。

## Runtime / 运行时

`reg` is an optional OpenCV contrib module. Runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_reg500.dll` when the module is built. If a runtime lacks it, the managed API shape remains stable and linked calls report `NOT_LINKED`.

`reg` 是可选 OpenCV contrib 模块。构建该模块时 runtime staging 会包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_reg500.dll`。如果某个 runtime 缺少它，managed API 形状仍保持稳定，linked 调用会报告 `NOT_LINKED`。

## Input Notes / 输入说明

The first smoke path uses tiny generated grayscale images and checks output shape only. Registration quality depends on image content, initialization, mapper choice, and pyramid settings.

第一批 smoke 路径使用 tiny 合成灰度图，并只检查输出形状。配准质量取决于图像内容、初始 map、mapper 选择和 pyramid 参数。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Reg;

internal static class Program
{
    private static void Main()
    {
        using Mat image = new Mat(32, 32, MatType.CV_8UC1, new Scalar(30));
        using MapShift init = RegCv2.CreateMapShift(1.0, 0.0);
        using Mat shifted = init.InverseWarp(image);

        using MapperGradShift mapper = RegCv2.CreateMapperGradShift();
        using RegMap result = mapper.Calculate(image, shifted);
        using Mat warped = result.Warp(shifted);

        using RegMap inverse = result.InverseMap();
        using Mat restored = inverse.Warp(image);
    }
}
```

Use `MapAffine` or `MapProjec` when the transform model is known. `RegMap.Compose` is limited to maps of the same concrete kind so the native boundary does not perform unsafe cross-kind casts.

已知变换模型时可使用 `MapAffine` 或 `MapProjec`。`RegMap.Compose` 只允许同一具体类型的 map，以避免 native 边界执行不安全的跨类型转换。
