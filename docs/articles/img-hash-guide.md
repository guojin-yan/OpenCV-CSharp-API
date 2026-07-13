# ImgHash Guide / ImgHash 指南

`OpenCvSharp.ImgHash` wraps the first optional contrib `img_hash` perceptual image hash algorithms.

`OpenCvSharp.ImgHash` 封装第一批可选 contrib `img_hash` 感知图像哈希算法。

## Scope / 范围

- Base object: `ImgHashBase` with `Compute` and `Compare`.
- Hash objects: `AverageHash`, `PHash`, `BlockMeanHash`, `ColorMomentHash`, `MarrHildrethHash`, and `RadialVarianceHash`.
- One-shot helpers: `ImgHashCv2.AverageHash`, `PHash`, `BlockMeanHash`, `ColorMomentHash`, `MarrHildrethHash`, and `RadialVarianceHash`.
- Parameters: `BlockMeanHashMode`, `BlockMeanHash.GetMean`, `MarrHildrethHash` kernel parameters, and `RadialVarianceHash` sigma/angle-line settings.

- 基类对象：带 `Compute` 与 `Compare` 的 `ImgHashBase`。
- 哈希对象：`AverageHash`、`PHash`、`BlockMeanHash`、`ColorMomentHash`、`MarrHildrethHash` 和 `RadialVarianceHash`。
- 一次性 helper：`ImgHashCv2.AverageHash`、`PHash`、`BlockMeanHash`、`ColorMomentHash`、`MarrHildrethHash` 和 `RadialVarianceHash`。
- 参数：`BlockMeanHashMode`、`BlockMeanHash.GetMean`、`MarrHildrethHash` 核参数，以及 `RadialVarianceHash` sigma/角线数量设置。

## Runtime / 运行时

`img_hash` is an optional OpenCV contrib module. A linked runtime should include the factual OpenCV 5.0.0 runtime artifact `opencv_img_hash500.dll`. If the DLL is missing, the managed API shape remains stable and calls report `NOT_LINKED`.

`img_hash` 是可选 OpenCV contrib 模块。linked runtime 应包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_img_hash500.dll`。如果缺少该 DLL，managed API 形状仍保持稳定，调用会报告 `NOT_LINKED`。

## Hash Notes / 哈希说明

`Compute` writes the algorithm-specific hash into a caller-owned `Mat` or returns a new `Mat`. `Compare` compares hashes produced by the same algorithm; the numeric meaning differs by algorithm and should not be compared across algorithms.

`Compute` 会把算法特定的哈希写入调用方持有的 `Mat`，也可以返回新 `Mat`。`Compare` 用于比较同一算法生成的哈希；数值含义随算法不同而变化，不应跨算法直接比较。

`ColorMomentHash` produces `CV_64F` output. Most other first-batch hashes produce compact `CV_8U` output. Applications should inspect `Mat.Type` when storing or serializing hash matrices.

`ColorMomentHash` 输出 `CV_64F`。第一批其他大多数哈希会输出紧凑的 `CV_8U`。应用在存储或序列化 hash Mat 时应检查 `Mat.Type`。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.ImgHash;

using Mat image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(16, 32, 48));
using Mat average = ImgHashCv2.AverageHash(image);
using Mat colorMoment = ImgHashCv2.ColorMomentHash(image);

using BlockMeanHash comparer = BlockMeanHash.Create(BlockMeanHashMode.Mode0);
using Mat block = comparer.Compute(image);
double distanceToSelf = comparer.Compare(block, block);
```
