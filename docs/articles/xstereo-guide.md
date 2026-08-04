# XStereo Guide / XStereo 指南

`JYPPX.OpenCvSharp.XStereo` wraps the first OpenCV contrib `xstereo` descriptor, binary stereo, and quasi-dense stereo surfaces.

`JYPPX.OpenCvSharp.XStereo` 封装第一批 OpenCV contrib `xstereo` 描述子、二值 stereo 和 quasi-dense stereo 能力。

## Scope / 范围

- Descriptor helpers: `CensusTransform`, `ModifiedCensusTransform`, `SymmetricCensusTransform`, and `StarCensusTransform`.
- Matchers: `StereoBinaryBM`, `StereoBinarySGBM`, and `QuasiDenseStereo`.
- Value types and enums: `MatchQuasiDense`, `PropagationParameters`, `CensusTransformType`, `StereoBinaryBMPreFilterType`, `StereoSpeckleRemovalAlgorithm`, `StereoSubPixelInterpolationMethod`, and `StereoBinarySGBMMode`.
- Quasi-dense match vectors are exposed as managed arrays through native count/fill APIs; no STL container crosses the ABI.

- 描述子 helper：`CensusTransform`、`ModifiedCensusTransform`、`SymmetricCensusTransform` 和 `StarCensusTransform`。
- matcher：`StereoBinaryBM`、`StereoBinarySGBM` 和 `QuasiDenseStereo`。
- 值类型与枚举：`MatchQuasiDense`、`PropagationParameters`、`CensusTransformType`、`StereoBinaryBMPreFilterType`、`StereoSpeckleRemovalAlgorithm`、`StereoSubPixelInterpolationMethod` 和 `StereoBinarySGBMMode`。
- quasi-dense match vector 通过 native count/fill API 暴露为 managed 数组；不会让 STL 容器穿过 ABI。

## Runtime / 运行时

`xstereo` is an optional OpenCV contrib module. Runtime staging includes the factual OpenCV 5.0.0 runtime artifact `opencv_xstereo500.dll` when the module is built. If a runtime lacks it, the managed API shape remains stable and linked calls report `NOT_LINKED`.

`xstereo` 是可选 OpenCV contrib 模块。构建该模块时 runtime staging 会包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_xstereo500.dll`。如果某个 runtime 缺少它，managed API 形状仍保持稳定，linked 调用会报告 `NOT_LINKED`。

## Example / 示例

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.XStereo;

internal static class Program
{
    private static void Main()
    {
        using Mat left = new Mat(32, 48, MatType.CV_8UC1, new Scalar(30));
        using Mat right = new Mat(32, 48, MatType.CV_8UC1, new Scalar(30));
        Cv2.Rectangle(left, new Rect(16, 8, 16, 14), new Scalar(220), -1);
        Cv2.Rectangle(right, new Rect(13, 8, 16, 14), new Scalar(220), -1);

        using Mat census = XStereoCv2.CensusTransform(left, 5);

        using StereoBinaryBM bm = StereoBinaryBM.Create(16, 9);
        using Mat bmDisparity = bm.Compute(left, right);

        using StereoBinarySGBM sgbm = StereoBinarySGBM.Create(0, 16, 3);
        using Mat sgbmDisparity = sgbm.Compute(left, right);

        using QuasiDenseStereo quasiDense = QuasiDenseStereo.Create(left.Size);
        quasiDense.Process(left, right);
        MatchQuasiDense[] sparse = quasiDense.GetSparseMatches();
    }
}
```

Tiny stereo pairs are useful for ABI smoke, not for measuring disparity quality. Real stereo use needs rectified images, sufficient texture, and tuned disparity ranges.

tiny stereo pair 适合做 ABI smoke，不适合衡量视差质量。真实 stereo 使用需要已校正图像、足够纹理和调好的视差范围。
