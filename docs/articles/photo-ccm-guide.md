# Photo Color Correction Model Guide

`JYPPX.OpenCvSharp.Photo.ColorCorrectionModel` wraps OpenCV 5.0.0 `cv::ccm::ColorCorrectionModel`. The managed object owns an opaque native handle, and every matrix returned by `Compute`, `GetColorCorrectionMatrix`, `GetSrcLinearRGB`, `GetRefLinearRGB`, `GetMask`, `GetWeights`, or `CorrectImage` is independently owned by the caller.

## Construction

Use one of four factories:

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Photo;

using Mat measured = new Mat(24, 1, MatType.CV_64FC3);
// Fill measured with RGB patch values in [0, 1], in checker order.

using ColorCorrectionModel builtIn =
    PhotoCv2.CreateColorCorrectionModel(measured, ColorCheckerType.Macbeth);

using Mat reference = measured.Clone();
using ColorCorrectionModel custom =
    PhotoCv2.CreateColorCorrectionModel(measured, reference, ColorSpace.Srgb);
```

The built-in checker sample counts are exact: Macbeth and Vinyl use 24 patches; DigitalSG uses 140. Measured samples and custom reference colors are `N x 1 CV_64FC3`. Measured values are RGB, not BGR, and must be finite values in `[0,1]`.

RGB reference spaces also use finite `[0,1]` values. XYZ and Lab reference spaces use their native numeric units; Lab data can contain negative chroma values and lightness values above 1. The custom mask overload accepts an `N x 1 CV_8UC1` matrix containing only zero and one. A non-contiguous `N x 1` ROI is legal because construction clones all input matrices.

The parameterless factory creates an empty model for `Read`. It has no source samples and cannot run `Compute`.

## Gamma Correction

`PhotoCv2.GammaCorrection` has caller-output and returning overloads:

```csharp
using Mat corrected = PhotoCv2.GammaCorrection(source, 2.2);
```

The source can have 8U, 16U, 16S, 32F, or 64F depth. OpenCV preserves its size, channel count, and type. Gamma must be finite and greater than zero. Integer depths are normalized by their depth maximum, corrected, and converted back; floating-point inputs use the conventional `[0,1]` scale.

## Configuration And Defaults

The audited OpenCV 5.0.0 implementation defaults are:

| Setting | Default |
|---|---|
| Working color space | `ColorSpace.Srgb` |
| CCM type | `CcmType.Linear` |
| Distance | `DistanceType.Cie2000` |
| Linearization | `LinearizationType.Gamma` |
| Gamma | `2.2` |
| Polynomial degree | `3` |
| Saturation interval | `[0,0.98]` |
| Initial method | `InitialMethodType.LeastSquare` |
| Weight coefficient | `0` |
| Maximum iterations | `5000` |
| Epsilon | `1e-4` |
| RGB channel conversion flag | `true` |

The implementation default for the RGB flag is `true`, despite an inconsistent upstream header comment. With `true`, `CorrectImage` converts a conventional OpenCV BGR image to RGB before correction and converts it back afterward. Measured patch matrices remain RGB ordered.

All configuration setters except `SetRGB` invalidate a previously computed or loaded ready state. Call `Compute` again before getters, correction, or persistence. `SetRGB` can be changed after `Read` because that flag is not persisted.

`SetWeightsList` accepts an empty matrix or an `N x 1 CV_64FC1` finite matrix. The input is cloned, so it can be disposed immediately. OpenCV normalizes retained positive weights during `Compute`; `GetWeights` returns only weights selected by the fitted mask.

OpenCV 5.0.0 retains normalized internal weights after a compute. Setting an empty weight list and a zero coefficient afterward does not clear that internal result. Create a fresh model when changing from a weighted fit to an unweighted fit. Weighted Affine fitting also reaches an upstream 3-channel/4-channel arithmetic mismatch; use a fresh unweighted model for Affine fitting.

## Compute And Outputs

```csharp
model.SetDistance(DistanceType.RgbLinear);
using Mat ccm = model.Compute();
double loss = model.GetLoss();
using Mat mask = model.GetMask();
```

Linear fitting returns a `3 x 3 CV_64FC1` matrix. Affine fitting returns a `4 x 3 CV_64FC1` coefficient matrix, matching the orientation returned by OpenCV's `solve(N x 4, N x 3)` implementation. Caller-output overloads can reuse an existing `Mat`; returning overloads create a new owned `Mat`.

Getters are valid only after a successful `Compute` or `Read`. A failed compute or read leaves the model unready. Repeated compute and getter calls are supported, subject to the retained-weight behavior above.

## Image Correction

`CorrectImage` accepts non-empty `CV_8UC3`, `CV_16UC3`, or `CV_32FC3` images and preserves size and type. Caller-output, returning, and in-place calls are supported by the audited implementation.

The upstream OpenCV 5.0.0 `islinear=true` branch copies the linear result and then unconditionally overwrites it with the delinearized result. Therefore the managed `isLinear` argument preserves upstream behavior but does not currently produce a distinct linear output. This guide does not claim otherwise.

## Persistence And Lifetime

```csharp
string yaml;
using (var writer = new FileStorage(
    "memory.yml",
    FileStorageModes.Write | FileStorageModes.Memory | FileStorageModes.FormatYaml))
{
    model.Write(writer);
    yaml = writer.ReleaseAndGetString();
}

using var reader = new FileStorage(
    yaml,
    FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml);
using FileNode node = reader["ColorCorrectionModel"];
using ColorCorrectionModel loaded = PhotoCv2.CreateColorCorrectionModel();
loaded.Read(node);
```

`Write` emits one top-level `ColorCorrectionModel` map. `Read` expects that inner map node, not the document root. The schema contains `ccm`, `loss`, `csEnum`, `ccm_type`, `shape`, `linear`, `distance`, `linear_type`, `gamma`, `deg`, and `saturated_threshold`. The binding validates required fields, enum domains, scalar ranges, and CCM shape before accepting the model.

`FileNode` keeps its parent storage state alive after the `FileStorage` wrapper is disposed. Calling `FileStorage.Release` or reopening it increments the storage generation and makes previously obtained nodes invalid; `Read` rejects those stale nodes. The loaded model owns its state after `Read`, so the node and storage can then be disposed.

Persistence stores the fitted CCM and correction configuration, but not original samples, masks, or fitted source/reference matrices. A loaded model can correct images and expose its CCM/loss. It cannot be reconfigured and recomputed without constructing a new source-based model.

## Runtime Boundary

CCM is part of the main CPU `opencv_photo` module and is included in the full runtime profile. Its native wrapper source and exports are intentionally excluded from the mini profile. Contrib `JYPPX.OpenCvSharp.XPhoto` and CUDA Photo APIs are separate surfaces.

The remaining main CPU Photo gaps are TV-L1 denoising, chromatic-aberration correction/loading, and `IntelligentScissorsMB`. No repository-wide upstream parity claim is implied.
