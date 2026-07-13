# Quality Guide / Quality 指南

`OpenCvSharp.Quality` wraps the first contrib `quality` metrics: MSE, PSNR, SSIM, GMSD, and BRISQUE.

`OpenCvSharp.Quality` 封装第一批 contrib `quality` 图像质量指标：MSE、PSNR、SSIM、GMSD 和 BRISQUE。

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

BRISQUE scoring requires user-provided model and range files. Tests and samples only run the real BRISQUE score path when `OPENCV_CSHARP_BRISQUE_MODEL` and `OPENCV_CSHARP_BRISQUE_RANGE` are set. The older `OPENCV5SHARP_BRISQUE_*` names remain accepted only as existing-smoke-workflow compatibility aliases.

BRISQUE 评分需要用户提供 model 和 range 文件。测试和示例只有在设置 `OPENCV_CSHARP_BRISQUE_MODEL` 与 `OPENCV_CSHARP_BRISQUE_RANGE` 时才运行真实 BRISQUE 评分路径。旧的 `OPENCV5SHARP_BRISQUE_*` 名称仍仅作为既有 smoke workflow 的兼容别名使用。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.Quality;

using Mat reference = new Mat(4, 4, MatType.CV_8UC1, new Scalar(10));
using Mat comparison = new Mat(4, 4, MatType.CV_8UC1, new Scalar(12));
using Mat qualityMap = new Mat();

Scalar mse = QualityMSE.Compute(reference, comparison, qualityMap);
Scalar psnr = QualityPSNR.Compute(reference, comparison);

using QualitySSIM ssim = QualitySSIM.Create(reference);
Scalar score = ssim.Compute(comparison);
```
