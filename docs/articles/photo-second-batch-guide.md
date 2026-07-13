# Photo Second Batch Guide

The second `OpenCvSharp.Photo` batch adds decoloring, seamless cloning, local editing, edge-preserving filters, sketching, and stylization.

第二批 `OpenCvSharp.Photo` 增加去色、seamless cloning、局部编辑、边缘保持滤波、素描和风格化能力。

## Covered APIs / 已覆盖接口

- `PhotoCv2.Decolor`
- `PhotoCv2.SeamlessClone`
- `PhotoCv2.ColorChange`
- `PhotoCv2.IlluminationChange`
- `PhotoCv2.TextureFlattening`
- `PhotoCv2.EdgePreservingFilter`
- `PhotoCv2.DetailEnhance`
- `PhotoCv2.PencilSketch`
- `PhotoCv2.Stylization`
- `SeamlessCloneFlags`
- `EdgePreservingFilterFlags`

## Example / 示例

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Photo;

namespace PhotoSecondBatchSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat src = new Mat(64, 64, MatType.CV_8UC3, new Scalar(32, 64, 96)))
            using (Mat mask = new Mat(64, 64, MatType.CV_8UC1, new Scalar(255)))
            using (Mat gray = new Mat())
            using (Mat boost = new Mat())
            using (Mat clone = new Mat())
            using (Mat sketch = new Mat())
            using (Mat sketchColor = new Mat())
            using (Mat smooth = PhotoCv2.EdgePreservingFilter(src))
            using (Mat detail = PhotoCv2.DetailEnhance(src))
            using (Mat stylized = PhotoCv2.Stylization(src))
            {
                PhotoCv2.Decolor(src, gray, boost);
                PhotoCv2.SeamlessClone(src, src, mask, new Point(32, 32), clone, SeamlessCloneFlags.NormalClone);
                PhotoCv2.PencilSketch(src, sketch, sketchColor);

                System.Console.WriteLine("Gray=" + gray.Size + ", smooth=" + smooth.Size + ", detail=" + detail.Size + ", stylized=" + stylized.Size);
            }
        }
    }
}
```

`EdgePreservingFilter`, `DetailEnhance`, and `Stylization` include returning `Mat` overloads for simple single-output calls. The output-`Mat` overloads remain available for destination reuse. Multi-output functions such as `Decolor` and `PencilSketch`, plus mask/editing functions such as `SeamlessClone`, continue to use explicit output matrices.

`EdgePreservingFilter`、`DetailEnhance` 和 `Stylization` 为简单单输出调用提供返回 `Mat` 的重载。需要复用目标矩阵时仍可使用 output-`Mat` 重载。`Decolor`、`PencilSketch` 等多输出函数，以及 `SeamlessClone` 等 mask/editing 函数继续使用显式输出矩阵。

Most editing functions require compatible image types and masks. The wrapper validates managed null arguments before entering native code; OpenCV validates detailed image type and size rules.

大多数编辑函数需要兼容的图像类型和 mask。wrapper 会先校验 managed null 参数；更细的图像类型与尺寸规则由 OpenCV 校验。

## Runtime Notes / 运行时说明

These APIs are part of the factual OpenCV 5.0.0 runtime artifact `opencv_photo500.dll`. Default tests do not depend on external photos; real visual quality depends on valid source images, masks, and parameter ranges.

这些 API 属于事实性 OpenCV 5.0.0 runtime 产物 `opencv_photo500.dll`。默认测试不依赖外部照片；真实视觉质量取决于有效源图、mask 和参数范围。
