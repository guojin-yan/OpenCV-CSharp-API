# Core Spectral And Numeric Transform Guide / Core 频谱与数值变换指南

The core spectral batch exposes OpenCV DFT, DCT, spectrum arithmetic, Cartesian/polar conversion, and element-wise numeric transforms.

core 频谱批次暴露 OpenCV DFT、DCT、频谱运算、笛卡尔/极坐标转换和逐元素数值变换。

## DFT And DCT / DFT 与 DCT

Use `Dft`/`Idft` and `Dct`/`Idct` for transform round-trips.

使用 `Dft`/`Idft` 和 `Dct`/`Idct` 完成变换与逆变换。

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat signal = new Mat(1, 4, MatType.CV_64FC1))
            using (Mat spectrum = new Mat())
            using (Mat recovered = new Mat())
            using (Mat dct = new Mat())
            using (Mat idct = new Mat())
            {
                signal.CopyFrom<double>(new double[] { 1.0, 2.0, 3.0, 4.0 });

                CoreCv2.Dft(signal, spectrum, DftFlags.ComplexOutput);
                CoreCv2.Idft(spectrum, recovered, DftFlags.Scale | DftFlags.RealOutput);
                CoreCv2.Dct(signal, dct);
                CoreCv2.Idct(dct, idct);

                Console.WriteLine(string.Join(",", recovered.ToArray<double>()));
                Console.WriteLine(string.Join(",", idct.ToArray<double>()));
            }
        }
    }
}
```

Use `GetOptimalDftSize` when preparing padded buffers for FFT-friendly dimensions.

准备适合 FFT 的填充缓冲区时，可使用 `GetOptimalDftSize` 获取推荐尺寸。

## Spectrum Arithmetic / 频谱运算

`MulSpectrums` and `DivSpectrums` operate on OpenCV-compatible packed or complex spectrums.

`MulSpectrums` 和 `DivSpectrums` 作用于与 OpenCV 兼容的打包频谱或复数频谱。

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat signal = new Mat(1, 4, MatType.CV_64FC1))
            {
                signal.CopyFrom<double>(new double[] { 1.0, 2.0, 3.0, 4.0 });

                using (Mat spectrum = CoreCv2.Dft(signal, DftFlags.ComplexOutput))
                using (Mat multiplied = CoreCv2.MulSpectrums(spectrum, spectrum))
                using (Mat divided = CoreCv2.DivSpectrums(spectrum, spectrum))
                {
                    Console.WriteLine($"{multiplied.Rows}x{multiplied.Cols}");
                    Console.WriteLine($"{divided.Rows}x{divided.Cols}");
                }
            }
        }
    }
}
```

## Vector And Numeric Transforms / 向量与数值变换

The current transform group includes:

当前变换组包括：

- `Magnitude`
- `Phase`
- `CartToPolar`
- `PolarToCart`
- `Exp`
- `Log`
- `Sqrt`
- `Pow`

```csharp
using System;
using JYPPX.OpenCvSharp.Core;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;

namespace Demo
{
    internal static class Program
    {
        private static void Main()
        {
            using (Mat x = new Mat(1, 2, MatType.CV_32FC1))
            using (Mat y = new Mat(1, 2, MatType.CV_32FC1))
            using (Mat magnitude = new Mat())
            using (Mat angle = new Mat())
            using (Mat values = new Mat(1, 3, MatType.CV_64FC1))
            using (Mat sqrt = new Mat())
            {
                x.CopyFrom<float>(new float[] { 3.0F, 0.0F });
                y.CopyFrom<float>(new float[] { 4.0F, 1.0F });
                values.CopyFrom<double>(new double[] { 1.0, 4.0, 9.0 });

                CoreCv2.CartToPolar(x, y, magnitude, angle, angleInDegrees: true);
                CoreCv2.Sqrt(values, sqrt);

                Console.WriteLine(string.Join(",", magnitude.ToArray<float>()));
                Console.WriteLine(string.Join(",", angle.ToArray<float>()));
                Console.WriteLine(string.Join(",", sqrt.ToArray<double>()));
            }
        }
    }
}
```

## Flags / 标志

`DftFlags`, `DctFlags`, and `MulSpectrumsFlags` keep OpenCV values stable while presenting C# enum names.

`DftFlags`、`DctFlags` 和 `MulSpectrumsFlags` 保持 OpenCV 数值稳定，同时提供 C# 风格的枚举命名。
