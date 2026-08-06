# Changelog / 版本变更总览

This file is the concise chronological index for package-visible changes. Detailed bilingual notes for every version live in [`docs/releases`](docs/releases/README.md).

本文件用于汇总所有影响使用者的版本变更。每个版本的中英文详细说明保存在 [`docs/releases`](docs/releases/README.md)。

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
