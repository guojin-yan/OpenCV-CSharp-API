# Features2D Boundary / Features2D 边界

`JYPPX.OpenCvSharp.Features2D` is the first feature detection and descriptor matching package. It follows the same three-layer rule as the rest of the project:

`JYPPX.OpenCvSharp.Features2D` 是第一批特征检测与描述子匹配封装。它遵循项目统一的三层规则：

```text
OpenCV C++ object/function -> jyppx_ocv_* C ABI -> JYPPX.OpenCvSharp managed API
```

## Implemented Surface / 已实现接口面

- Value objects: `KeyPoint`, `DMatch`.
- Base objects: `Feature2D`, `DescriptorMatcher`.
- Algorithm objects: `ORB`, `SIFT`, `FastFeatureDetector`, `GFTTDetector`, `MSER`, `SimpleBlobDetector`, `BRISK`, `KAZE`, `AKAZE`, `AffineFeature`, `BFMatcher`, `FlannBasedMatcher`.
- Bag-of-words helpers: `BOWKMeansTrainer`, `BOWImgDescriptorExtractor`.
- Region objects: `MserRegion` for `MSER.DetectRegions`.
- Parameter objects: `SimpleBlobDetectorParams`.
- Drawing helpers: `Cv2.DrawKeypoints`, `Cv2.DrawMatches`, `Cv2.DrawMatchesKnn`.
- Common metadata: `Feature2D.DefaultName`, `DescriptorSize`, `DescriptorType`, and `DefaultNorm`.
- Batch detection: `Feature2D.Detect(Mat[])` and modern `ReadOnlySpan<Mat>` overloads.
- Modern .NET fast paths: `ReadOnlySpan<KeyPoint>`, `ReadOnlySpan<DMatch>`, `ReadOnlySpan<Mat>`, `ReadOnlySpan<float>`, and `ReadOnlySpan<int>` overloads for short-lived descriptor, draw, detector, matcher, and custom BRISK pattern input buffers.

- 值对象：`KeyPoint`、`DMatch`。
- 基类对象：`Feature2D`、`DescriptorMatcher`。
- 算法对象：`ORB`、`SIFT`、`FastFeatureDetector`、`GFTTDetector`、`MSER`、`SimpleBlobDetector`、`BRISK`、`KAZE`、`AKAZE`、`AffineFeature`、`BFMatcher`、`FlannBasedMatcher`。
- Bag-of-words 辅助对象：`BOWKMeansTrainer`、`BOWImgDescriptorExtractor`。
- 区域对象：`MSER.DetectRegions` 使用 `MserRegion` 返回区域点集。
- 参数对象：`SimpleBlobDetectorParams`。
- 绘制辅助：`Cv2.DrawKeypoints`、`Cv2.DrawMatches`、`Cv2.DrawMatchesKnn`。
- 通用元数据：`Feature2D.DefaultName`、`DescriptorSize`、`DescriptorType` 和 `DefaultNorm`。
- 批量检测：`Feature2D.Detect(Mat[])` 和现代 `ReadOnlySpan<Mat>` 重载。
- 新框架快速路径：为短生命周期描述子、绘制、检测器、匹配器和自定义 BRISK pattern 输入缓冲提供 `ReadOnlySpan<KeyPoint>`、`ReadOnlySpan<DMatch>`、`ReadOnlySpan<Mat>`、`ReadOnlySpan<float>`、`ReadOnlySpan<int>` 重载。

## Value Objects / 值对象

`KeyPoint` is an immutable `cv::KeyPoint`-compatible value object. The coordinate constructor fills OpenCV-style defaults: `Angle = -1`, `Response = 0`, `Octave = 0`, and `ClassId = -1`. Equality and hash codes include all OpenCV keypoint fields, and `ToString()` formats floating-point fields with invariant culture.

`KeyPoint` 是不可变、兼容 `cv::KeyPoint` 的值对象。坐标构造函数会填充 OpenCV 风格默认值：`Angle = -1`、`Response = 0`、`Octave = 0`、`ClassId = -1`。相等性和哈希包含全部 OpenCV keypoint 字段，`ToString()` 使用 invariant culture 格式化浮点字段。

`DMatch` is an immutable `cv::DMatch`-compatible value object. The short constructor defaults `ImgIdx` to zero. Its indexer maps OpenCV field indexes `0..3` to `QueryIdx`, `TrainIdx`, `ImgIdx`, and `Distance`, and rejects other indexes with `IndexOutOfRangeException`. Equality includes all fields, while `CompareTo` follows OpenCV sorting behavior by comparing `Distance` only.

`DMatch` 是不可变、兼容 `cv::DMatch` 的值对象。短构造函数默认 `ImgIdx` 为 0。它的索引器把 OpenCV 字段索引 `0..3` 映射到 `QueryIdx`、`TrainIdx`、`ImgIdx` 和 `Distance`，其他索引会抛出 `IndexOutOfRangeException`。相等性包含全部字段，而 `CompareTo` 遵循 OpenCV 排序语义，只按 `Distance` 比较。

## Enum Constants / 枚举常量

Features2D managed enums keep OpenCV-compatible numeric values so C# code can round-trip native settings without translation tables. `OrbScoreType` maps `HarrisScore = 0` and `FastScore = 1`. `FastFeatureDetectorType` maps `Type5_8 = 0`, `Type7_12 = 1`, and `Type9_16 = 2`. `DescriptorMatcherType` maps `FlannBased = 1`, `BruteForce = 2`, `BruteForceL1 = 3`, `BruteForceHamming = 4`, `BruteForceHammingLut = 5`, and `BruteForceSL2 = 6`. `DrawMatchesFlags` maps `Default = 0`, `DrawOverOutImg = 1`, `NotDrawSinglePoints = 2`, and `DrawRichKeypoints = 4`.

Features2D managed 枚举保留与 OpenCV 兼容的数值，这样 C# 代码可以在不引入额外转换表的情况下往返 native 设置。`OrbScoreType` 映射为 `HarrisScore = 0`、`FastScore = 1`。`FastFeatureDetectorType` 映射为 `Type5_8 = 0`、`Type7_12 = 1`、`Type9_16 = 2`。`DescriptorMatcherType` 映射为 `FlannBased = 1`、`BruteForce = 2`、`BruteForceL1 = 3`、`BruteForceHamming = 4`、`BruteForceHammingLut = 5`、`BruteForceSL2 = 6`。`DrawMatchesFlags` 映射为 `Default = 0`、`DrawOverOutImg = 1`、`NotDrawSinglePoints = 2`、`DrawRichKeypoints = 4`。

The xfeatures2d enums follow the same rule. `KazeDiffusivityType` maps `DiffPmG1 = 0`, `DiffPmG2 = 1`, `DiffWeickert = 2`, and `DiffCharbonnier = 3`. `AkazeDescriptorType` maps `DescriptorKazeUpright = 2`, `DescriptorKaze = 3`, `DescriptorMldbUpright = 4`, and `DescriptorMldb = 5`.

xfeatures2d 枚举也遵循同一规则。`KazeDiffusivityType` 映射为 `DiffPmG1 = 0`、`DiffPmG2 = 1`、`DiffWeickert = 2`、`DiffCharbonnier = 3`。`AkazeDescriptorType` 映射为 `DescriptorKazeUpright = 2`、`DescriptorKaze = 3`、`DescriptorMldbUpright = 4`、`DescriptorMldb = 5`。

## Managed Validation / managed 参数校验

Drawing helpers validate required managed inputs before native dispatch. `DrawKeypoints`, `DrawMatches`, and `DrawMatchesKnn` require non-null image and output `Mat` arguments. Array overloads also require non-null keypoint and match arrays, and `DrawMatchesKnn` rejects null inner match groups while flattening grouped matches. Drawing helpers accept only known `DrawMatchesFlags` bits: `DrawOverOutImg`, `NotDrawSinglePoints`, and `DrawRichKeypoints`. Unknown flag bits throw `ArgumentOutOfRangeException` before any native call. Span overloads follow the same required-`Mat` and flag-validation boundary as the array overloads.

绘制辅助会在进入 native 前校验必需 managed 输入。`DrawKeypoints`、`DrawMatches` 和 `DrawMatchesKnn` 要求 image 与输出 `Mat` 参数非 null。数组重载还要求 keypoint 与 match 数组非 null，`DrawMatchesKnn` 在展平分组匹配时会拒绝 null 的内部 match 组。绘制辅助只接受已知的 `DrawMatchesFlags` 位：`DrawOverOutImg`、`NotDrawSinglePoints` 和 `DrawRichKeypoints`。未知 flag 位会在进入 native 调用前抛出 `ArgumentOutOfRangeException`。Span 重载遵循与数组重载相同的必需 `Mat` 与 flag 校验边界。

Descriptor matching and bag-of-words helpers share the descriptor-matrix boundary documented in the [DescriptorMatcher guide](features2d-matcher-guide.md). `DescriptorMatcher` validates factory, descriptor collection, mask collection, KNN `k`, and radius `maxDistance` arguments before native dispatch. `BOWImgDescriptorExtractor` uses a trained `DescriptorMatcher`, so its vocabulary rows and computed or precomputed keypoint descriptors must remain compatible with the selected matcher norm and descriptor type. `BOWKMeansTrainer` only accepts non-empty `CV_32F` descriptor rows with compatible column counts and types, and `DescriptorConvert` provides explicit conversion and normalization helpers for workflows that need float descriptor rows before matching, k-means, or bag-of-words training. `DescriptorConvert` null source/destination validation is managed; real conversion and normalization still use core native matrix operations.

描述子匹配与词袋辅助对象共享 [DescriptorMatcher 使用指南](features2d-matcher-guide.md) 中记录的描述子矩阵边界。`DescriptorMatcher` 会在 native 分派前校验 factory、描述子集合、mask 集合、KNN `k` 和半径匹配 `maxDistance` 参数。`BOWImgDescriptorExtractor` 依赖已训练的 `DescriptorMatcher`，因此词典行以及计算得到或预计算传入的关键点描述子必须与所选 matcher 范数和描述子类型保持兼容。`BOWKMeansTrainer` 只接受非空 `CV_32F` 描述子行，并要求列数和矩阵类型兼容；需要在匹配、k-means 或词袋训练前准备 float 描述子行时，应显式使用 `DescriptorConvert` 的转换和归一化辅助方法。`DescriptorConvert` 对 null 源/目标矩阵的校验属于 managed 边界；真正的转换和归一化仍使用 core native 矩阵操作。

## Optional Native Module / 可选 native 模块

OpenCV 5.0.0 names this source module `features`. The native wrapper links `opencv_features` only when the installed OpenCV package exposes that target.

OpenCV 5.0.0 中该源码模块名为 `features`。native 封装仅在当前 OpenCV 安装包暴露 `opencv_features` target 时链接它。

When the module is unavailable:

- The C ABI symbols are still exported.
- Managed P/Invoke declarations remain stable.
- Calls such as `ORB.Create()`, `SIFT.Create()`, `FastFeatureDetector.Create()`, `GFTTDetector.Create()`, `MSER.Create()`, `SimpleBlobDetector.Create()`, `AffineFeature.Create(...)`, `BFMatcher.Create()`, and `FlannBasedMatcher.Create()` throw `OpenCvException` with a `NOT_LINKED` message.
- Runtime staging skips the factual OpenCV 5.0.0 runtime artifact `opencv_features500.dll` as an optional module.

当该模块不可用时：

- C ABI 符号仍然会导出。
- Managed P/Invoke 声明保持稳定。
- `ORB.Create()`、`SIFT.Create()`、`FastFeatureDetector.Create()`、`GFTTDetector.Create()`、`MSER.Create()`、`SimpleBlobDetector.Create()`、`AffineFeature.Create(...)`、`BFMatcher.Create()` 和 `FlannBasedMatcher.Create()` 等调用会抛出带有 `NOT_LINKED` 信息的 `OpenCvException`。
- Runtime staging 会把事实性 OpenCV 5.0.0 runtime 产物 `opencv_features500.dll` 作为可选模块处理。

This keeps package compatibility stable while allowing runtime packages to add `features` later without changing the managed API shape.

这样可以保持包兼容性稳定，并允许后续 runtime 包加入 `features` 时不改变 managed API 形状。

## Optional Contrib Module / 可选 contrib 模块

OpenCV 5.0.0 exposes `BRISK`, `KAZE`, and `AKAZE` through the contrib `xfeatures2d` module in this source layout. The native wrapper therefore treats `opencv_xfeatures2d` as a second optional component on top of `opencv_features`.

在当前 OpenCV 5.0.0 源码布局中，`BRISK`、`KAZE` 和 `AKAZE` 位于 contrib 的 `xfeatures2d` 模块。native 封装因此把 `opencv_xfeatures2d` 作为叠加在 `opencv_features` 之上的第二个可选组件处理。

When `opencv_features` is available but `opencv_xfeatures2d` is not available:

- `BRISK`, `KAZE`, and `AKAZE` managed classes still compile on every target framework.
- The C ABI exports all `brisk`, `kaze`, `akaze`, and typed `AffineFeature.Create(...)` bridge symbols.
- Constructors and methods report `NOT_LINKED` through `OpenCvException` instead of failing with `EntryPointNotFoundException`.
- Runtime packages can add the factual OpenCV 5.0.0 runtime artifact `opencv_xfeatures2d500.dll` later without changing managed package APIs.

当 `opencv_features` 可用但 `opencv_xfeatures2d` 不可用时：

- `BRISK`、`KAZE` 和 `AKAZE` managed 类仍然在所有目标框架上编译。
- C ABI 仍导出全部 `brisk`、`kaze`、`akaze` 以及对应的 typed `AffineFeature.Create(...)` bridge 符号。
- 构造函数和方法通过 `OpenCvException` 报告 `NOT_LINKED`，而不是出现 `EntryPointNotFoundException`。
- 后续 runtime 包可以加入事实性 OpenCV 5.0.0 runtime 产物 `opencv_xfeatures2d500.dll`，无需改变 managed 包 API。
