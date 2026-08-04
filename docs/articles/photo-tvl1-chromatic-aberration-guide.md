# Photo TV-L1 And Chromatic Aberration Guide

`PhotoCv2.DenoiseTvl1`, `PhotoCv2.CorrectChromaticAberration`, and `PhotoCv2.LoadChromaticAberrationParams` complete the measured OpenCV 5.0.0 main CPU Photo callable slice. They are full-profile APIs and require the Photo runtime module. The mini profile deliberately does not export them.

## TV-L1 Denoising

TV-L1 accepts one or more non-empty, two-dimensional `CV_8UC1` observations with identical dimensions. The wrapper requires this exact type because the upstream implementation reads every observation through byte iterators. ROI and other non-contiguous matrices are accepted. `lambda` and `niters` must both be positive.

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Photo;

using var first = new Mat(64, 64, MatType.CV_8UC1, new Scalar(72));
using var second = new Mat(64, 64, MatType.CV_8UC1, new Scalar(88));
using Mat denoised = PhotoCv2.DenoiseTvl1(
    new[] { first, second },
    lambda: 1.0,
    niters: 30);
```

The returning overload owns a new `Mat`. The output overload writes to caller-owned storage. Native code borrows the observation handles only for the duration of the call and retains neither the array nor the image headers. The output is allocated as `CV_8UC1` with the observation dimensions.

## Calibration Schema

`LoadChromaticAberrationParams` reads an opened `FileNode` map with this shape:

```yaml
%YAML:1.0
image_width: 640
image_height: 480
red_channel:
  coeffs_x: [0., 0., 0.]
  coeffs_y: [0., 0., 0.]
blue_channel:
  coeffs_x: [0., 0., 0.]
  coeffs_y: [0., 0., 0.]
```

All four coefficient lists must be non-empty, finite, and equal in length. For degree `d`, each list must contain `(d + 1) * (d + 2) / 2` terms. Red and blue degrees must match. The loaded `4 x N CV_32FC1` matrix uses this row order:

1. blue x displacement
2. blue y displacement
3. red x displacement
4. red y displacement

```csharp
const string yaml = "%YAML:1.0\n" +
    "image_width: 4\nimage_height: 4\n" +
    "red_channel:\n  coeffs_x: [0.]\n  coeffs_y: [0.]\n" +
    "blue_channel:\n  coeffs_x: [0.]\n  coeffs_y: [0.]\n";

using var storage = new FileStorage(
    yaml,
    FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml);
using FileNode root = storage.Root();
using ChromaticAberrationParameters parameters =
    PhotoCv2.LoadChromaticAberrationParams(root);
```

`ChromaticAberrationParameters` owns an independent coefficient `Mat`. It remains valid after the `FileNode` or `FileStorage` is disposed. Disposing the parameter object disposes that matrix. The caller-owned overload provides the same copy semantics without allocating a result object.

## Applying Correction

The input dimensions must exactly equal `CalibrationSize`. Coefficients must be `CV_32FC1`, have four rows, and have the triangular column count for `Degree`.

```csharp
using var input = new Mat(4, 4, MatType.CV_8UC3, new Scalar(20, 40, 80));
using Mat corrected = PhotoCv2.CorrectChromaticAberration(
    input,
    parameters.Coefficients,
    parameters.CalibrationSize,
    parameters.Degree);
```

Three-channel input is interpreted as BGR and ignores `bayerPattern`. Single-channel input is treated as raw Bayer data and requires a non-negative OpenCV demosaicing code, such as `(int)ColorConversionCodes.BayerBG2BGR`; unsupported codes fail through the native exception bridge. BGR output retains the input size and type. Raw Bayer input is demosaiced before correction and therefore produces a three-channel image.

Input and coefficient matrices are borrowed for one call. Returning overloads create independent output storage, while output overloads keep ownership with the caller. OpenCV performs the coefficient remap without modifying the coefficient matrix.

## Boundary Notes

The C ABI uses a pointer/count array for TV-L1 observations, flattened checked integers for size and degree, caller-owned `Mat` outputs, and the shared type-safe Core `FileNode` accessor. No `std::vector`, `cv::Mat`, `cv::Size`, `cv::FileNode`, C++ reference, or exception crosses the ABI.

This closes the exact measured main CPU Photo parser slice only. CUDA Photo, contrib `xphoto`, and repository-wide OpenCV C++ parity remain separate scopes.
