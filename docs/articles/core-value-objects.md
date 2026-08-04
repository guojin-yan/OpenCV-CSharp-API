# Core Value Objects

The core namespace contains small value objects that mirror common OpenCV C++ types.

`Core` 命名空间包含与常见 OpenCV C++ 类型对应的小型值对象。

## Current Types

- `Point` mirrors `cv::Point`.
- `Size` mirrors `cv::Size`.
- `Rect` mirrors `cv::Rect`.
- `Scalar` mirrors `cv::Scalar`.
- `Moments` mirrors `cv::Moments` in the `JYPPX.OpenCvSharp.ImgProc` namespace.
- `TermCriteria` mirrors `cv::TermCriteria` in the `JYPPX.OpenCvSharp.Core` namespace.
- `MinMaxLocResult` groups the scalar values and locations returned by `cv::minMaxLoc`.
- `MeanStdDevResult` groups the per-channel mean and standard deviation returned by `cv::meanStdDev`.
- `HoughLine`, `HoughLinePointSet`, `HoughCircle`, and `LineSegment` are `imgproc` result value objects in the `JYPPX.OpenCvSharp.ImgProc` namespace.
- `SvdFlags`, `GemmFlags`, `DftFlags`, `DctFlags`, `MulSpectrumsFlags`, and `RngDistributionTypes` map OpenCV core flags to C# enum names.

## Rect

`Rect` stores `X`, `Y`, `Width`, and `Height`.

It also exposes:

- `Left`
- `Top`
- `Right`
- `Bottom`
- `Location`
- `Size`
- `Area`
- `Empty`
- `Contains`

OpenCV rectangles use an inclusive left/top edge and exclusive right/bottom edge for point containment.

OpenCV 矩形在点包含判断中使用包含左/上边界、排除右/下边界的规则。

## Scalar

`Scalar` stores four `double` components: `V0`, `V1`, `V2`, and `V3`.

For color APIs, OpenCV commonly interprets scalar color values in BGR order.

对于颜色相关 API，OpenCV 通常按 BGR 顺序解释 scalar 颜色值。

## Moments

`Moments` stores the 24 OpenCV moment values in field order:

- Spatial moments: `M00`, `M10`, `M01`, `M20`, `M11`, `M02`, `M30`, `M21`, `M12`, `M03`.
- Central moments: `Mu20`, `Mu11`, `Mu02`, `Mu30`, `Mu21`, `Mu12`, `Mu03`.
- Normalized central moments: `Nu20`, `Nu11`, `Nu02`, `Nu30`, `Nu21`, `Nu12`, `Nu03`.

`Moments` provides an indexer and `ToArray()` for OpenCV field-order access.

`Moments` 按 OpenCV 字段顺序保存 24 个矩值，并提供索引器和 `ToArray()` 方便按顺序访问。

## TermCriteria

`TermCriteria` stores the OpenCV termination flags, maximum iteration count, and requested epsilon.

`TermCriteria` 保存 OpenCV 终止条件标志、最大迭代次数和请求精度。

Factory helpers keep common call sites short:

常用工厂方法让调用点更简洁：

- `TermCriteria.ByCount(maxCount)`
- `TermCriteria.ByEpsilon(epsilon)`
- `TermCriteria.ByCountAndEpsilon(maxCount, epsilon)`

## Core Result Values

`MinMaxLocResult` and `MeanStdDevResult` keep OpenCV multi-output calls readable in C#.

`MinMaxLocResult` 和 `MeanStdDevResult` 让 OpenCV 多输出调用在 C# 中保持清晰。

- `MinMaxLocResult` stores `MinVal`, `MaxVal`, `MinLoc`, and `MaxLoc`.
- `MeanStdDevResult` stores `Mean` and `StdDev` as `Scalar` values.

## Core Flag Enums

Core math and transform APIs expose OpenCV-compatible flag values with C# enum member names.

Core 数学和变换 API 使用 C# 枚举成员名称暴露与 OpenCV 兼容的标志值。

- `SvdFlags` maps `cv::SVD::Flags`.
- `GemmFlags` maps `cv::GemmFlags`.
- `DftFlags` maps OpenCV DFT flags.
- `DctFlags` maps OpenCV DCT flags.
- `MulSpectrumsFlags` maps spectrum row flags.
- `RngDistributionTypes` maps `cv::RNG` distribution types.

## ImgProc Result Values

Several `imgproc` APIs return small typed value objects instead of raw `float[]` or `double[]` buffers:

多个 `imgproc` API 返回小型强类型值对象，而不是直接暴露裸 `float[]` 或 `double[]` 缓冲：

- `HoughLine` stores `Rho` and `Theta`.
- `HoughLinePointSet` stores `Votes`, `Rho`, and `Theta`.
- `HoughCircle` stores `Center`, `X`, `Y`, and `Radius`.
- `LineSegment` stores `P1`, `P2`, `X1`, `Y1`, `X2`, `Y2`, and `LengthSquared`.

These types provide equality operators, `ToString()`, and indexers where the OpenCV result order is useful.

这些类型提供相等运算符、`ToString()`，并在 OpenCV 结果顺序有意义时提供索引器。
