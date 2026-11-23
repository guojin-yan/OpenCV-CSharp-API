# Changelog / 版本变更总览

This file is the concise chronological index for package-visible changes. Detailed bilingual notes for every version live in [`docs/releases`](docs/releases/README.md).

本文件用于汇总所有影响使用者的版本变更。每个版本的中英文详细说明保存在 [`docs/releases`](docs/releases/README.md)。

## [5.1.0] - In development / 开发中

- Complete the 5.1 refresh of all 73 historical parser scopes (5,778 declarations) by reviewing eighteen backend/dependency scopes with 1,727 declarations. Preserve specialized parser strategies and current Cudev/MATLAB lexical/CST inventories, including Cudev Windows 64-bit macro branches. Shared guards reject 2,420 mutations; source semantic guards retain 64 separate fixtures. Complete C++/template and optional backend runtime coverage remain open.
- Review nineteen public module parser scopes with 880 declarations, including the ColoredKinFu optional-color replacement and CVV Qt gate change. That batch brought the evidence to 55 scopes and 4,051 declarations; guards preserve non-wrapper parsing, exclusions and metadata-only boundaries.
- Refresh fourteen image/geometry contrib parser scopes with 346 unchanged declarations: that batch brought the evidence to 36 scopes and 3,171 declarations. Extend validation to singleton, pure-callable and wholly omitted scopes while preserving exact bindings and omissions.
- Refresh eight contrib parser scopes (611 declarations): that batch brought the 5.1 inventory to 22 scopes and 2,825 declarations. Preserve exact bindings and omissions, review HoughPoint2Line documentation, and support enum-free and unordered historical evidence inputs in the shared validator.
- Refresh nine more parser scopes against the fixed OpenCV/contrib 5.1 sources: that batch brought coverage to 14 scopes and 2,214 declarations. Record XFeat/LightGlue, Qt persistence, dictionary ratio distance and video writer/device gaps explicitly; runtime implementation is unchanged.
- Correct `CalibrationFlags.DisableSchurComplement` to OpenCV 5.1 bit 27 so it no longer enables tilted distortion. Recompile callers that used the old constant; see the calibration migration guide. BFMatcher mask support now follows 5.1's CrossCheck-dependent result.
- Refreshed Core, ImgProc and ImgCodecs against official 5.1 source with a shared review tool; the earlier five parser slices including Calib3D cover 914 declarations and new API/input-domain gaps recorded explicitly.
- Corrected DNN engine and TFLite format enum translation for OpenCV 5.1 while keeping public and ABI 1 values stable; refreshed the DNN parser slice with explicit remaining gaps.

- Move the source, package metadata and development target to the official matching OpenCV/opencv_contrib 5.1.0 tags.
- Preserve TensorFlow importer APIs with an explicit upstream-removal error; verify ONNX format diagnostics and inference against 5.1.0.
- Carry forward bounded codec preflight improvements, including Sun Raster byte-RLE across 1/8/24/32-bit storage, and retain the audited CCM instance-state patch.
- Windows x64 Full and Mini wrappers build with unchanged ABI 1; other 5.1.0 runtime and release evidence remains pending.
- Details / 详细说明: [`docs/releases/5.1.0.md`](docs/releases/5.1.0.md)

## [5.0.0] - 2026-08-06

- Expanded safe `Mat` access for non-contiguous images, typed rows, pixel vectors, two-dimensional element access, and external stride buffers.
- Added DNN NMS/Soft-NMS, type-safe image encoding parameters, end-of-stream-friendly VideoIO reads, package-version diagnostics, and broader color conversion coverage.
- Consolidated the `JYPPX.OpenCvSharp` public namespace, Apache-2.0 licensing, runtime package matrix, samples, tutorials, and protected multi-registry publication workflow.
- Updated the structured NuGet repository-signature verifier to audited `NuGet.Packaging 7.6.0`.
- Applied and provenance-checked the OpenCV 5.0.0 photo CCM instance-state fix so Adobe RGB configuration cannot mutate later sRGB models.
- Details / 详细说明: [`docs/releases/5.0.0.md`](docs/releases/5.0.0.md)

## [5.0.0-preview.1] - Published / 已发布

- First public preview of the managed OpenCV 5 API and separately installable native runtime packages.
- Established the initial NuGet.org package surface and managed/native package separation.
- Details / 详细说明: [`docs/releases/5.0.0-preview.1.md`](docs/releases/5.0.0-preview.1.md)

[5.0.0]: https://github.com/guojin-yan/OpenCV-CSharp-API/releases
[5.0.0-preview.1]: https://github.com/guojin-yan/OpenCV-CSharp-API/releases
