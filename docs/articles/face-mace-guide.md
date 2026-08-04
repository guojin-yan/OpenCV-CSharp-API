# Face MACE Guide / Face MACE 指南

`MACE` wraps the OpenCV contrib Minimum Average Correlation Energy filter in the `face` module. It is a template/filter workflow, not a modern DNN face embedding model.

`MACE` 封装 `face` 模块中的 OpenCV contrib Minimum Average Correlation Energy 滤波器。它是模板/滤波器工作流，不是现代 DNN 人脸 embedding 模型。

## Scope / 范围

- `MACE.Create(imageSize)`.
- `MACE.Load(filename, objname)`.
- `Salt(passphrase)`.
- `Train(params Mat[] images)`.
- `Same(Mat query)`.
- `Save(path)` and `Empty`.

- `MACE.Create(imageSize)`。
- `MACE.Load(filename, objname)`。
- `Salt(passphrase)`。
- `Train(params Mat[] images)`。
- `Same(Mat query)`。
- `Save(path)` 和 `Empty`。

## Input Notes / 输入说明

Training images should be same-size positive examples that match the `imageSize` used at creation. The tiny smoke path uses generated grayscale images only to verify train/save/load/same call paths; it does not imply real template quality.

训练图像应是与创建时 `imageSize` 匹配的同尺寸正样本。tiny smoke 路径只使用生成的灰度图验证 train/save/load/same 调用路径，不代表真实模板质量。

If the factual OpenCV 5.0.0 runtime artifact `opencv_face500.dll` is unavailable, calls report `NOT_LINKED`. The wrapper does not download or bundle image datasets.

如果事实性 OpenCV 5.0.0 runtime 产物 `opencv_face500.dll` 不可用，调用会报告 `NOT_LINKED`。封装层不会下载或内置图像数据集。

## Minimal Sample / 最小示例

```csharp
using System;
using System.IO;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Face;

namespace FaceMaceSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat first = new Mat(32, 32, MatType.CV_8UC1, new Scalar(70)))
            using (Mat second = new Mat(32, 32, MatType.CV_8UC1, new Scalar(90)))
            using (Mat query = new Mat(32, 32, MatType.CV_8UC1, new Scalar(72)))
            using (MACE mace = MACE.Create(imageSize: 32))
            {
                mace.Salt("sample-passphrase");
                mace.Train(first, second);
                bool same = mace.Same(query);

                string path = Path.Combine(Path.GetTempPath(), "opencv-csharp-mace.xml");
                try
                {
                    mace.Save(path);
                    using (MACE loaded = MACE.Load(path))
                    {
                        Console.WriteLine("same=" + same + ", loadedEmpty=" + loaded.Empty);
                    }
                }
                finally
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
        }
    }
}
```
