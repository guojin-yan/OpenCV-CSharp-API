# LineDescriptor Guide / LineDescriptor 指南

`JYPPX.OpenCvSharp.LineDescriptor` wraps OpenCV contrib `line_descriptor` value types, binary descriptor objects, matchers, and drawing helpers.

`JYPPX.OpenCvSharp.LineDescriptor` 封装 OpenCV contrib `line_descriptor` 的值类型、二进制描述子对象、匹配器和绘图 helper。

## Scope / 范围

- Value object: `KeyLine`.
- Parameters: `BinaryDescriptorParameters`.
- Native objects: `BinaryDescriptor` and `BinaryDescriptorMatcher`.
- Detection and descriptor paths: `Detect`, `Compute`, and `DetectAndCompute`.
- Matching paths: `Match` and `KnnMatch`.
- Drawing helpers: `LineDescriptorCv2.DrawKeylines` and `LineDescriptorCv2.DrawLineMatches`.
- Drawing flags: `DrawLinesMatchesFlags`.

- 值对象：`KeyLine`。
- 参数：`BinaryDescriptorParameters`。
- native 对象：`BinaryDescriptor` 与 `BinaryDescriptorMatcher`。
- 检测与描述子路径：`Detect`、`Compute` 与 `DetectAndCompute`。
- 匹配路径：`Match` 与 `KnnMatch`。
- 绘图 helper：`LineDescriptorCv2.DrawKeylines` 与 `LineDescriptorCv2.DrawLineMatches`。
- 绘图标志：`DrawLinesMatchesFlags`。

## Runtime / 运行时



## ABI Notes / ABI 说明

`KeyLine` and `DMatch` arrays are flattened before crossing the native boundary, and descriptor matrices remain caller-owned `Mat` values. The `DrawLineMatches` wrapper passes an explicit all-true mask internally because OpenCV 5.0.0 indexes the mask vector while drawing matches.

`KeyLine` 与 `DMatch` 数组会在进入 native 边界前平铺，描述子矩阵保持为调用方持有的 `Mat`。`DrawLineMatches` wrapper 内部传入显式全 true mask，因为 OpenCV 5.0.0 绘制匹配时会索引 mask vector。

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.LineDescriptor;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

using Mat image = new Mat(64, 64, MatType.CV_8UC1, new Scalar(0));
ImgProcCv2.Line(image, new Point(8, 12), new Point(56, 12), new Scalar(255), 2);
ImgProcCv2.Line(image, new Point(10, 50), new Point(54, 18), new Scalar(255), 2);

using BinaryDescriptor descriptor = BinaryDescriptor.Create(BinaryDescriptorParameters.Default);
using BinaryDescriptorMatcher matcher = BinaryDescriptorMatcher.Create();
using Mat descriptors = new Mat();

KeyLine[] keylines = descriptor.Detect(image);
keylines = descriptor.DetectAndCompute(image, null, keylines, descriptors, useProvidedKeylines: keylines.Length > 0);

using Mat drawn = LineDescriptorCv2.DrawKeylines(image, keylines, new Scalar(0, 255, 0));

if (!descriptors.Empty && descriptors.Rows > 0)
{
    var matches = matcher.Match(descriptors, descriptors);
    using Mat matched = LineDescriptorCv2.DrawLineMatches(image, keylines, image, keylines, matches);
}
```
