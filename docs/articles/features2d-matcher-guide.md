# DescriptorMatcher Guide / DescriptorMatcher 使用指南

`DescriptorMatcher` is the common managed base type for `BFMatcher` and `FlannBasedMatcher`. It exposes direct matching, KNN matching, radius matching, and trained descriptor collection matching.

`DescriptorMatcher` 是 `BFMatcher` 和 `FlannBasedMatcher` 的 managed 公共基类型，提供直接匹配、KNN 匹配、半径匹配，以及基于训练描述子集合的匹配。

## BFMatcher / BFMatcher

`BFMatcher` wraps OpenCV `cv::BFMatcher`.

`BFMatcher` 封装 OpenCV `cv::BFMatcher`。

## Direct Match / 直接匹配

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat query = CreateDescriptors(new float[] { 0, 0, 10, 10 }))
            using (Mat train = CreateDescriptors(new float[] { 0, 0, 9, 9, 50, 50 }))
            using (DescriptorMatcher matcher = BFMatcher.Create(NormTypes.L2))
            {
                DMatch[] matches = matcher.Match(query, train);
                DMatch[][] knn = matcher.KnnMatch(query, train, 2);
                DMatch[][] radius = matcher.RadiusMatch(query, train, 3.0F);

                Console.WriteLine("matches=" + matches.Length + ", knn groups=" + knn.Length + ", radius groups=" + radius.Length);
            }
        }

        private static Mat CreateDescriptors(float[] values)
        {
            Mat descriptors = new Mat(values.Length / 2, 2, MatType.CV_32FC1);
            descriptors.CopyFrom(values);
            return descriptors;
        }
    }
}
```

## Trained Collection / 训练集合

Use `Add` and `Train` when a matcher should reuse one or more train descriptor matrices.

当匹配器需要复用一个或多个训练描述子矩阵时，可以使用 `Add` 和 `Train`。

```csharp
DescriptorMatcher matcher = BFMatcher.Create(NormTypes.L2);
matcher.Add(new[] { trainDescriptors });
matcher.Train();
DMatch[] matches = matcher.Match(queryDescriptors);
```

## Managed Validation / managed 参数校验

Quick boundary summary:

- Factory inputs are checked before native dispatch. `DescriptorMatcher.Create(string)` rejects null names, `DescriptorMatcher.Create(DescriptorMatcherType)` rejects undefined factory enum values, and `BFMatcher.Create(...)` accepts only descriptor distance norms supported by OpenCV BF matching: `L1`, `L2`, `L2Sqr`, `Hamming`, and `Hamming2`.
- Train-descriptor collections passed to `Add` must be non-null and contain no null `Mat` items. Empty collections are allowed and pass through as empty train-descriptor updates.
- Required query/train descriptor `Mat` inputs are validated before matching. KNN matching requires a positive `k`, and radius matching requires a finite, non-negative `maxDistance`.
- `BFMatcher` supports descriptor masks through direct and trained-collection overloads. Empty per-train mask collections are treated as no masks.
- `FlannBasedMatcher` reports `IsMaskSupported == false`: a non-null direct mask, or a non-empty per-train mask collection/span, throws `NotSupportedException`; null mask collections or null mask items still follow shared `ArgumentNullException` validation.
- Pure managed helper rules are covered without the native runtime. Live matcher creation and real matching behavior remain native-runtime guarded because matcher instances and real `Mat` handles require the Features2D native library.

快速边界摘要：

- factory 输入会在 native 分派前校验。`DescriptorMatcher.Create(string)` 拒绝 null 名称，`DescriptorMatcher.Create(DescriptorMatcherType)` 拒绝未定义 factory 枚举值，`BFMatcher.Create(...)` 只接受 OpenCV BF 匹配支持的描述子距离范数：`L1`、`L2`、`L2Sqr`、`Hamming` 和 `Hamming2`。
- 传给 `Add` 的训练描述子集合本身必须非 null，且不能包含 null `Mat` 项。空集合允许传入，并会作为空训练描述子更新传递。
- 必需的 query/train descriptor `Mat` 会在匹配前校验。KNN 匹配要求 `k` 为正数，半径匹配要求 `maxDistance` 为有限且非负。
- `BFMatcher` 通过直接匹配和训练集合重载支持 descriptor mask。空的 per-train mask 集合会按无 mask 处理。
- `FlannBasedMatcher` 会报告 `IsMaskSupported == false`：非 null 的直接 mask，或非空的 per-train mask 集合/span，会抛出 `NotSupportedException`；mask 集合本身为 null 或集合元素为 null 时仍遵循共享的 `ArgumentNullException` 校验。
- 纯 managed helper 规则可以在没有 native runtime 的情况下覆盖。实际 matcher 创建和真实匹配行为仍由 native runtime 条件保护，因为 matcher 实例和真实 `Mat` handle 需要 Features2D native library。

## Bag-of-Words / 词袋描述子

`BOWImgDescriptorExtractor` combines a `DescriptorMatcher` with a descriptor extractor to build bag-of-visual-words image descriptors. Image/keypoint compute paths currently dispatch descriptor computation through typed managed extractors that expose `Compute`: `ORB`, `SIFT`, `BRISK`, `KAZE`, and `AKAZE`. Supplying another `Feature2D` subclass reports `NotSupportedException` so the unsupported descriptor-computation boundary is explicit.

Constructors reject null extractors or matchers with `ArgumentNullException`. `SetVocabulary` requires a non-null, non-empty vocabulary, stores its own clone, and trains the matcher against that clone; `GetVocabulary` returns a caller-owned clone, and `Clear` drops the stored vocabulary so later compute calls fail with `InvalidOperationException` until a new vocabulary is set. Precomputed-descriptor compute paths also require a non-null, non-empty descriptor matrix and non-null output descriptor before matching.

The vocabulary is matcher training data: every vocabulary row must be compatible with the keypoint descriptors that will later be matched against it, including descriptor type, column count, and selected matcher norm. Image/keypoint compute paths pass the extractor output directly to the matcher, while precomputed-descriptor compute paths let callers prepare converted or normalized descriptors first. Managed validation catches null and empty inputs early; descriptor type, shape, and norm mismatches remain part of the shared matcher/native compatibility boundary.

`BOWImgDescriptorExtractor` 会把 `DescriptorMatcher` 与描述子提取器组合，用于生成 bag-of-visual-words 图像描述子。图像/keypoint compute 路径目前会通过显式支持 `Compute` 的强类型 managed 提取器分派描述子计算：`ORB`、`SIFT`、`BRISK`、`KAZE` 和 `AKAZE`。传入其他 `Feature2D` 子类会报告 `NotSupportedException`，从而明确标出尚未暴露的描述子计算边界。

构造函数会用 `ArgumentNullException` 拒绝空 extractor 或 matcher。`SetVocabulary` 要求词典非 null 且非空，会保存自己的克隆并用该克隆训练 matcher；`GetVocabulary` 返回调用方拥有的克隆，`Clear` 会丢弃已保存词典，因此之后的 compute 调用会在重新设置词典前抛出 `InvalidOperationException`。预计算描述子 compute 路径同样要求描述子矩阵非 null、非空，且输出 descriptor 非 null，之后才会进入匹配。

词典本质上是 matcher 的训练数据：每个词典行都必须与之后用于匹配的关键点描述子兼容，包括 descriptor type、列数以及所选 matcher 范数。图像/keypoint compute 路径会把提取器输出直接交给 matcher；预计算描述子 compute 路径则允许调用方先自行完成转换或归一化。managed 校验会提前拦截 null 和空输入；descriptor 类型、形状和范数不匹配仍属于共享的 matcher/native 兼容性边界。

`BOWKMeansTrainer` builds the visual vocabulary by running k-means over descriptor rows. Its constructor requires positive `clusterCount` and `attempts` values and throws `ArgumentOutOfRangeException` for invalid values. Descriptor rows passed to `Add` or `Cluster` must be non-null, non-empty, include at least one column, and use `CV_32F`; collection inputs must contain compatible matrices with matching column counts and matrix types. Empty `Add` collections are treated as no-ops, while empty `Cluster` collections are rejected. Clustering also rejects cases where `clusterCount` exceeds the available descriptor rows: stored-descriptor clustering throws `InvalidOperationException`, and supplied-row clustering throws `ArgumentException`.

`DescriptorConvert` provides helper steps for descriptor workflows before matching, k-means, or bag-of-words training. `ConvertDescriptorsToFloat` converts descriptor rows to `CV_32F` while preserving channel count, which is useful before `BOWKMeansTrainer.Add` or `Cluster` when the source descriptor extractor emits non-float rows. `NormalizeDescriptors` wraps OpenCV normalization for descriptor rows, and `ConvertToFloatAndNormalize` composes conversion followed by normalization. Allocating overloads return caller-owned `Mat` instances; output overloads write into the supplied destination. Null source or destination matrices are rejected with `ArgumentNullException` before conversion or normalization work begins. That null-input validation is managed and can be covered without the native runtime; the actual conversion and normalization steps still dispatch through core native `Mat.ConvertTo` and `Cv2.Normalize` operations.

`BOWKMeansTrainer` 通过对描述子行执行 k-means 来生成视觉词典。构造函数要求 `clusterCount` 和 `attempts` 都为正数，非法值会抛出 `ArgumentOutOfRangeException`。传给 `Add` 或 `Cluster` 的描述子行必须非空、至少包含一行一列，并使用 `CV_32F`；集合输入中的矩阵还必须列数和矩阵类型一致。空的 `Add` 集合会被当作 no-op，空的 `Cluster` 集合会被拒绝。聚类时如果 `clusterCount` 大于可用描述子行数也会被拒绝：对已保存描述子聚类时抛出 `InvalidOperationException`，对直接传入描述子行聚类时抛出 `ArgumentException`。

`DescriptorConvert` 为匹配、k-means 或词袋训练前的描述子流程提供辅助步骤。`ConvertDescriptorsToFloat` 会把描述子行转换为 `CV_32F`，同时保留通道数；当来源描述子提取器输出非 float 行，而之后要调用 `BOWKMeansTrainer.Add` 或 `Cluster` 时，这一步很有用。`NormalizeDescriptors` 封装 OpenCV 对描述子行的归一化，`ConvertToFloatAndNormalize` 则组合先转换再归一化。分配式重载返回调用方拥有的 `Mat`，输出式重载写入调用方提供的目标矩阵。空的源矩阵或目标矩阵会在转换或归一化开始前用 `ArgumentNullException` 拒绝。这类 null 输入校验属于 managed 边界，可以在没有 native runtime 的情况下覆盖；真正的转换和归一化步骤仍会分派到 core native 的 `Mat.ConvertTo` 与 `Cv2.Normalize` 操作。

## ORB Descriptors / ORB 描述子

For ORB descriptors, prefer `orb.DefaultNorm` when creating the matcher:

使用 ORB 描述子时，建议用 `orb.DefaultNorm` 创建匹配器：

```csharp
using (ORB orb = ORB.Create())
using (DescriptorMatcher matcher = BFMatcher.Create(orb.DefaultNorm))
{
}
```

`WtaK` affects whether OpenCV chooses Hamming or Hamming2 internally. Reading `DefaultNorm` keeps the matcher aligned with the descriptor extractor.

`WtaK` 会影响 OpenCV 内部选择 Hamming 还是 Hamming2。读取 `DefaultNorm` 可以让 matcher 与描述子提取器保持一致。

## FLANN / FLANN

`FlannBasedMatcher` is a better fit for floating-point descriptors such as SIFT.

`FlannBasedMatcher` 更适合 SIFT 这类浮点描述子。

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

namespace Samples
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat query = new Mat(2, 2, MatType.CV_32FC1))
            using (Mat train = new Mat(3, 2, MatType.CV_32FC1))
            using (DescriptorMatcher matcher = FlannBasedMatcher.Create())
            {
                query.CopyFrom(new float[] { 0, 0, 10, 10 });
                train.CopyFrom(new float[] { 0, 0, 9, 9, 50, 50 });

                DMatch[] matches = matcher.Match(query, train);
                Console.WriteLine("FLANN matches=" + matches.Length);
            }
        }
    }
}
```

`FlannBasedMatcher` currently rejects mask overloads with `NotSupportedException` because OpenCV FLANN mask behavior is not exposed through this project's stable C ABI yet.

`FlannBasedMatcher` 当前会对带 mask 的重载抛出 `NotSupportedException`，因为本项目稳定 C ABI 暂未暴露 OpenCV FLANN 的 mask 行为。
