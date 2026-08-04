# Quality Guide / Quality 指南

`JYPPX.OpenCvSharp.Quality` wraps the first contrib `quality` metrics: MSE, PSNR, SSIM, GMSD, and BRISQUE.

`JYPPX.OpenCvSharp.Quality` 封装第一批 contrib `quality` 图像质量指标：MSE、PSNR、SSIM、GMSD 和 BRISQUE。

## Scope / 范围

- Object wrappers: `QualityMSE`, `QualityPSNR`, `QualitySSIM`, `QualityGMSD`, and `QualityBRISQUE`.
- Shared object operations: `Compute`, `GetQualityMap`, `Clear`, and `Empty`.
- Static compute helpers that return a four-channel `Scalar` and optionally fill a caller-owned quality map `Mat`.
- `QualityBRISQUE.ComputeFeatures` and model/range-file scoring.

- 对象封装：`QualityMSE`、`QualityPSNR`、`QualitySSIM`、`QualityGMSD` 和 `QualityBRISQUE`。
- 共享对象操作：`Compute`、`GetQualityMap`、`Clear` 和 `Empty`。
- 静态 compute helper 返回四通道 `Scalar`，并可选写入调用方持有的质量图 `Mat`。
- `QualityBRISQUE.ComputeFeatures` 以及基于 model/range 文件的评分。

## Runtime / 运行时

`quality` is an optional OpenCV contrib module and BRISQUE also depends on OpenCV `ml`. Runtime staging should include the factual OpenCV 5.0.0 runtime artifacts `opencv_quality500.dll` and `opencv_ml500.dll` when contrib quality support is enabled. Missing contrib DLLs are reported as `NOT_LINKED`.

`quality` 是可选 OpenCV contrib 模块，BRISQUE 还依赖 OpenCV `ml`。启用 contrib quality 支持时，runtime staging 应包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_quality500.dll` 和 `opencv_ml500.dll`。缺少 contrib DLL 时会返回 `NOT_LINKED`。

## BRISQUE Models / BRISQUE 模型



```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Quality;

using Mat reference = new Mat(4, 4, MatType.CV_8UC1, new Scalar(10));
using Mat comparison = new Mat(4, 4, MatType.CV_8UC1, new Scalar(12));
using Mat qualityMap = new Mat();

Scalar mse = QualityMSE.Compute(reference, comparison, qualityMap);
Scalar psnr = QualityPSNR.Compute(reference, comparison);

using QualitySSIM ssim = QualitySSIM.Create(reference);
Scalar score = ssim.Compute(comparison);
```
