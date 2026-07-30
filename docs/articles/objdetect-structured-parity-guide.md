# ObjDetect Structured Parity Guide

This guide covers the exact OpenCV 5.0.0 ObjDetect compatibility include closure measured by `compatibility/objdetect-upstream-summary.json`. The official parser emitted 195 declarations from nine public source headers: 163 callables, 153 implemented, 10 intentionally omitted, and zero missing. This is a module-scoped contract, not a repository-wide parity claim.

## Generic ArUco Boards

`ArucoBoard` owns a native board value. Constructor inputs are copied, and `Dictionary`, `ObjectPoints`, and `Ids` return independent values. Disposing a returned dictionary or changing a returned array does not mutate the board.

```csharp
using var dictionary = ArucoDictionary.GetPredefinedDictionary(
    PredefinedDictionaryType.Dict4X4_50);
using var board = new ArucoBoard(
    new[]
    {
        new[]
        {
            new Point3f(0, 0, 0),
            new Point3f(1, 0, 0),
            new Point3f(1, 1, 0),
            new Point3f(0, 1, 0)
        }
    },
    dictionary,
    new[] { 7 });
using Mat image = board.GenerateImage(new Size(320, 320), marginSize: 16);
```

Nested marker points cross the C ABI as group offsets plus a flat `Point3f` or `Point2f` buffer. Count and fill calls must agree; a size change fails instead of writing a partial managed collection. `MatchImagePoints` writes into caller-owned `Mat` outputs and requires one id for each detected corner group.

## Multiple Dictionaries

Use `ArucoDetector.Create(ArucoDictionary[])` for multi-dictionary construction. Every dictionary must be non-null, alive, and the array must be non-empty. The detector owns native dictionary values after construction or `SetDictionaries`; the managed dictionary wrappers can then be disposed independently.

`DetectMarkersMultiDictionary` returns ordinary corners, ids, rejected candidates, and one dictionary index per detected marker. All arrays in `ArucoMultiDictionaryDetectionResult` are owned copies. `GetDictionaries` returns independently disposable dictionary wrappers.

## QR Bytes

Text decode methods apply UTF-8 conversion. Use `DecodeBytes`, `DetectAndDecodeBytes`, `DecodeMultiBytes`, or `DetectAndDecodeMultiBytes` when payload bytes must be preserved exactly.

```csharp
using var encoder = QRCodeEncoder.Create();
using Mat code = encoder.Encode("binary-boundary");
using var detector = QRCodeDetector.Create();
byte[] payload = detector.DetectAndDecodeBytes(code);
```

The native boundary first reports the exact byte length and then fills caller-provided storage. Embedded null bytes are data, not terminators. Multi-code byte output uses offsets and one flat byte buffer, and the managed result clones nested arrays on construction and access.

## ChArUco And Chessboards

`CharucoDetector` copies detector and refine parameter values on get and set. `DetectDiamonds` returns owned corner and id arrays through count/fill. Drawing helpers mutate the caller-owned image and never retain its pointer.

Advanced chessboard helpers use caller-owned matrices:

- `FindChessboardCornersSB` writes `corners`; the meta overload also writes a `patternHeight x patternWidth` `CV_8UC1` matrix.
- `EstimateChessboardSharpness` accepts 8-bit grayscale or color input, reads the detected corners, returns a four-value summary, and optionally writes per-edge measurements.
- `Find4QuadCornerSubpix` updates the supplied corner matrix in place.

The SB default flags value is `(ChessboardFlags)0`. The existing `ChessboardFlags.Default` value is `1` and is not valid for the SB algorithm.

## Ownership And Failure Rules

- `ArucoBoard`, detectors, dictionaries, and MCC/DNN objects own opaque native handles and support repeated `Dispose`.
- Returned `Mat` objects and returned dictionary wrappers are owned by the managed caller.
- Methods reject null, disposed handles, empty dictionary sets, mismatched ids and point groups, invalid flags, and count overflow before unsafe access.
- OpenCV failures cross the ABI through the existing native exception bridge as `OpenCvException`; no C++ exception or STL object crosses the boundary.
- Full ObjDetect support requires the full native profile with `opencv_objdetect`. The mini profile intentionally does not link these entrypoints and reports `NOT_LINKED` where the ABI is present.
- MCC construction from `Net` and DNN-assisted face or checker paths require `opencv_dnn`; model execution still depends on caller-supplied model data.

## Intentional Omissions

Eight persistence rows remain omitted: ordinals `3`, `4`, `40`, `41`, `44`, `45`, `61`, and `62`. They will remain omitted until ObjDetect can consume shared Core `FileStorage` and `FileNode` handles without private cross-module casts.

Two circles-grid rows remain omitted: the `CirclesGridFinderParameters` constructor at ordinal `145` and the explicit blob-detector plus parameter overload at ordinal `147`. They require a complete parameter value model and a stable Feature2D blob-detector ownership contract. Ordinal `148`, the default blob-detector overload, is implemented and is not part of this omission.

Run `scripts/Test-ObjDetectUpstreamMap.ps1` to verify classification completeness, evidence identity, parser and header hashes, deterministic ordering, and all 15 fail-closed fixtures.
