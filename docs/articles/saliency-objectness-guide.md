# Saliency Objectness Guide / Saliency Objectness 指南

`ObjectnessBING` wraps the OpenCV 5.0.0 contrib `saliency` BING objectness proposal algorithm. The linked runtime module is the factual OpenCV 5.0.0 runtime artifact `opencv_saliency500.dll`.

`ObjectnessBING` 封装 OpenCV 5.0.0 contrib `saliency` 的 BING objectness proposal 算法。linked runtime 模块是事实性 OpenCV 5.0.0 runtime 产物 `opencv_saliency500.dll`。

## Scope / 范围

- `ObjectnessBING.Create()`.
- `SetTrainingPath` and `SetBBResDir`.
- `Base`, `NSS`, and `W` properties.
- `ComputeObjectness(Mat)` returning `ObjectnessBINGResult`.
- Cached `GetBoxes()` and `GetObjectnessValues()` count/fill outputs.

- `ObjectnessBING.Create()`。
- `SetTrainingPath` 和 `SetBBResDir`。
- `Base`、`NSS` 和 `W` 属性。
- `ComputeObjectness(Mat)` 返回 `ObjectnessBINGResult`。
- 缓存的 `GetBoxes()` 与 `GetObjectnessValues()` count/fill 输出。

## Training Data / 训练数据

Real `ComputeObjectness` use requires the BING training-data directory expected by OpenCV. The wrapper does not download or bundle that data. `SetTrainingPath` and `SetBBResDir` reject null strings and embedded null characters in managed code before native dispatch; an empty or otherwise invalid directory is still an OpenCV/runtime data boundary rather than proof that BING training data is available. Default tests and console samples set paths and validate parameter/cached-output shape without requiring model data.

真实 `ComputeObjectness` 使用需要 OpenCV 期望的 BING 训练数据目录。封装层不会下载或内置该数据。`SetTrainingPath` 和 `SetBBResDir` 会在 native 分派前用 managed 代码拒绝 null 字符串和内嵌 null 字符；空目录或其他无效目录仍属于 OpenCV/runtime 数据边界，并不代表 BING 训练数据可用。默认测试和 console sample 只设置路径并验证参数/缓存输出形状，不要求模型数据。

`ObjectnessBINGBox` stores OpenCV's min/max integer coordinates. `ToRect()` converts them to a `Rect` using `Width = MaxX - MinX` and `Height = MaxY - MinY`.

`ObjectnessBINGBox` 保存 OpenCV 的 min/max 整数坐标。`ToRect()` 会按 `Width = MaxX - MinX` 和 `Height = MaxY - MinY` 转换为 `Rect`。

If the factual OpenCV 5.0.0 runtime artifact `opencv_saliency500.dll` is not linked, calls report the defined `NOT_LINKED` boundary.

如果未链接事实性 OpenCV 5.0.0 runtime 产物 `opencv_saliency500.dll`，调用会报告定义明确的 `NOT_LINKED` 边界。

## Minimal Parameter Sample / 最小参数示例

```csharp
using System;
using System.IO;
using JYPPX.OpenCvSharp.Saliency;

namespace SaliencyObjectnessSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (ObjectnessBING objectness = ObjectnessBING.Create())
            {
                objectness.SetTrainingPath(Path.GetTempPath());
                objectness.SetBBResDir(Path.GetTempPath());
                objectness.Base = 2.0;
                objectness.NSS = 3;
                objectness.W = 8;

                Console.WriteLine("BING=" + objectness.Base + "/" + objectness.NSS + "/" + objectness.W);
                Console.WriteLine("cached=" + objectness.GetBoxes().Length + "/" + objectness.GetObjectnessValues().Length);
            }
        }
    }
}
```

When valid BING data is available, call `ComputeObjectness(image)` and inspect `Boxes` and `ObjectnessValues` from the returned result.

当存在有效 BING 数据时，可以调用 `ComputeObjectness(image)`，并读取返回结果的 `Boxes` 和 `ObjectnessValues`。
