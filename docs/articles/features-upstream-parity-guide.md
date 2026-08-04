# Features Upstream Coverage And ANNIndex Guide

The Features map starts from both OpenCV 5 compatibility includes, `opencv2/features2d.hpp` and `opencv2/features/features.hpp`. Both forward to `opencv2/features.hpp`. The repository runs the official OpenCV 5 `hdr_parser.py` against that implementation header with FLANN and DNN enabled, preserving declaration order, overloads, defaults, direction modifiers, and source hashes.

The measured closure contains 183 declarations: 160 callables, 17 classes, and 6 enums. Of the callables, 134 have native and managed evidence, 26 are intentionally omitted with source-based reasons, and none remain unexplained. Optional `xfeatures2d` APIs are not mixed into this main-module count. `KeyPointsFilter` is public source without wrapper annotations and is recorded separately in `compatibility/features-source-reviewed-extensions.json`.

## ANNIndex Workflow

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

using ANNIndex index = ANNIndex.Create(2, ANNIndexDistance.Euclidean);
using Mat features = new Mat(4, 2, MatType.CV_32FC1);
using Mat query = new Mat(2, 2, MatType.CV_32FC1);
using Mat indices = new Mat();
using Mat distances = new Mat();

features.CopyFrom(new float[] { 0, 0, 10, 10, 2, 2, -2, -2 });
query.CopyFrom(new float[] { 0.1F, 0.1F, 9.5F, 10.5F });

index.SetSeed(1234);
index.AddItems(features);
index.Build(2);
index.KnnSearch(query, indices, distances, 1);
```

`features` is `itemCount x dimension`. Euclidean, Manhattan, Angular, and DotProduct indices require single-channel `CV_32F`; Hamming requires single-channel `CV_8U`. `AddItems` accepts a row-strided ROI because OpenCV reads each row independently. A query must be continuous, non-empty, two-dimensional, use the configured column count and type, and contain at least one row.

`knn` must be positive and cannot exceed `ItemNumber`. `searchK` is positive or `-1`; `trees` is positive or `-1`. Search creates or reshapes caller-owned outputs. `indices` becomes `query.Rows x knn` `CV_32SC1`; `distances` has the same shape and element type as the configured feature matrix. Query and output objects must be distinct. The index does not retain managed pointers to input Mats.

## Ownership And Failure

`ANNIndex` owns one opaque native handle through `SafeHandle`. Repeated disposal is safe. Any operation after disposal throws `ObjectDisposedException`. Input and output Mats remain caller-owned and must be disposed by the caller. Native OpenCV and Annoy failures cross the ABI through the existing exception bridge; C++ exceptions never cross the C boundary.

Validation is duplicated at the managed and native boundaries for dimensions, matrix shape and type, continuity, K values, search limits, output aliasing, paths, and enum values. Failed creation returns no handle. Caller-owned outputs are never converted into separately owned native results.

## Persistence

`Save`, `Load`, and `SetOnDiskBuild` accept UTF-8 paths with explicit byte lengths. Null, empty, invalid UTF-16, invalid UTF-8, and embedded-NUL paths are rejected. Annoy still uses narrow file APIs on Windows, so the native wrapper bridges through an ASCII temporary mapping and `std::filesystem` copies to preserve the requested Unicode path. The handle owns and removes any temporary mapping after releasing the Annoy index.

A loaded file must match the dimension and distance metric supplied to `Create`. Files are native Annoy index artifacts, not a portable interchange format across incompatible OpenCV, architecture, or metric configurations.

## Conditional Surface

The main Features implementation is a full-profile module. A full wrapper without `opencv_features` reports `NOT_LINKED`; mini packages do not constitute ANNIndex execution evidence. FLANN rows remain separately marked with `HAVE_OPENCV_FLANN=1`. DISK, ALIKED, and LightGlue rows are DNN/model-backed and require external model-specific shape evidence, so they are intentionally outside the deterministic offline success path.

The parser map is module-scoped. It does not claim parity for optional `xfeatures2d`, all OpenCV modules, or the repository as a whole.
