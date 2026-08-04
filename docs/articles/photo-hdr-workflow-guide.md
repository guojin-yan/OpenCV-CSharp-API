# Photo HDR Workflow Guide

The main `JYPPX.OpenCvSharp.Photo` HDR surface covers OpenCV 5.0.0 exposure alignment, inverse camera-response calibration, HDR merge, and exposure fusion. It uses the version-neutral `jyppx_ocv_` C ABI and the full linked runtime with `opencv_photo500.dll`. Contrib `xphoto` and CUDA Photo are separate surfaces and are not included in this contract.

## Practical Workflow

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Photo;

using Mat dark = new Mat(16, 16, MatType.CV_8UC3, new Scalar(32, 40, 48));
using Mat middle = new Mat(16, 16, MatType.CV_8UC3, new Scalar(96, 104, 112));
using Mat bright = new Mat(16, 16, MatType.CV_8UC3, new Scalar(192, 200, 208));
using Mat times = new Mat(3, 1, MatType.CV_32FC1, new Scalar(0.5));
using AlignMTB align = PhotoCv2.CreateAlignMTB(cut: false);
using CalibrateDebevec calibrate = PhotoCv2.CreateCalibrateDebevec(samples: 16);
using MergeDebevec merge = PhotoCv2.CreateMergeDebevec();
using MergeMertens fuse = PhotoCv2.CreateMergeMertens();

Mat[] images = { dark, middle, bright };
Mat[] aligned = align.Process(images);
try
{
    using Mat response = calibrate.Process(aligned, times);
    using Mat hdr = merge.Process(aligned, times);
    using Mat fused = fuse.Process(aligned);
    System.Console.WriteLine($"response={response.Size}, hdr={hdr.Type}, fused={fused.Size}");
}
finally
{
    foreach (Mat image in aligned) image.Dispose();
}
```

Use measured exposure times in production. The uniform `times` matrix above keeps the sample self-contained; it is not a photographic calibration recommendation.

## Input Matrices

All image arrays must be non-null, non-empty, and contain non-empty matrices with identical rows, columns, and type. Collection handles and their explicit count are borrowed only for the duration of the native call. No managed pointer or array element is retained.

`AlignMTB.Process` accepts CV_8U three- or four-channel images because the OpenCV 5.0.0 implementation converts each exposure to grayscale internally. `CalculateShift` and `ComputeBitmaps` require CV_8UC1. ROI and other non-contiguous matrices are supported because the ABI passes ref-counted `Mat` headers rather than flattening image storage.

`CalibrateDebevec` and `CalibrateRobertson` accept CV_8U images with one or three channels. `times` must be CV_32FC1 and contain exactly one value per image. Calibration returns a 256-by-1 CV_32F response matrix with the same channel count as the input.

`MergeDebevec` and `MergeRobertson` accept CV_8U, CV_16U, or CV_32F images with one or three channels. Their output has the source rows and columns, CV_32F depth, and the source channel count. `MergeMertens` accepts OpenCV numeric depths with one or three channels because its OpenCV 5.0.0 implementation converts each input to CV_32F; it returns a CV_32F exposure-fusion result and does not require times or a camera response.

## Overloads And Optional Response

The base `AlignExposures.Process` overload accepts `times` and `response`, matching the parser-emitted base contract. AlignMTB ignores those two values upstream and also exposes its short array overload.

The base `MergeExposures.Process` overload accepts both times and response. An empty response matrix requests OpenCV's linear response where the selected merge implementation permits it. `MergeDebevec.Process(images, times)` and `MergeRobertson.Process(images, times)` expose the corresponding short upstream overloads. `MergeMertens.Process(images)` is the response-free exposure-fusion path.

## Ownership And Aliasing

Aligner, calibrator, and merger objects own independent opaque native handles through `SafeHandle`. `Dispose` is repeatable, and any member access after disposal throws `ObjectDisposedException`. Factory failure cannot leak a partially returned handle.

Caller-output overloads write into caller-owned `Mat` objects. Returning overloads allocate new `Mat` objects and dispose partial results if native execution fails. `CalibrateRobertson.GetRadiance` copies the algorithm's internal radiance matrix into a caller-owned destination, so no borrowed view survives the call.

`AlignMTB.ShiftMat` rejects the same managed `Mat` as input and output because the upstream implementation creates the destination before reading all source pixels. `ComputeBitmaps` requires input, threshold bitmap, and exclusion bitmap to be distinct. General HDR process methods do not promise in-place operation; use distinct destination matrices.

Aligned output headers have independent ref-counted lifetimes. With `Cut=false`, the pivot output can share pixel storage with the corresponding source, matching upstream `cv::Mat` semantics. Disposing either header is safe, but mutating shared pixels can be observed through the other header.

## Errors And Numeric Boundaries

Null elements, empty collections, inconsistent image shapes or types, mismatched output counts, invalid time matrices, unsafe shift magnitudes, and prohibited aliases fail in managed code before the native call. OpenCV algorithm assertions and backend failures cross the ABI through `NativeException` as `OpenCvException`; C++ exceptions never cross the C boundary.

Property setters preserve OpenCV values rather than imposing undocumented ranges. Invalid solver or weighting values can therefore fail when the algorithm runs. Debevec sample count must also be practical for the image dimensions. Floating-point merge input should normally be normalized to the range expected by the selected algorithm.

## Measured Boundary

The deterministic OpenCV 5.0.0 Photo map measures 145 declarations and 120 callable rows. The HDR batch implements all 43 parser-emitted alignment, calibration, and merge callables. With the separately documented CCM, Intelligent Scissors, TV-L1, and chromatic-aberration batches, the complete map records all 120 callable rows implemented.

The remaining groups are TV-L1 denoising and chromatic-aberration correction/loading. They are linked CPU Photo surfaces but have no current native/managed evidence. CUDA is excluded from the CPU compatibility closure, and contrib `JYPPX.OpenCvSharp.XPhoto` remains separate. No repository-wide upstream parity claim is made.
