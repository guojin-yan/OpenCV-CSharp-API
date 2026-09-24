# Image Adapter Package Selection ADR / 图像 Adapter 包选择 ADR

## Status

Accepted for the 5.0.1 P1 design boundary. No adapter package is added to the core package by this ADR. A first adapter remains conditional on a separate dependency and license gate.

## Candidates and decision

| Candidate | Strength | Boundary risk | Decision |
| --- | --- | --- | --- |
| ImageSharp | Rich generic pixels, row spans, metadata and identify/limits patterns | Six Labors Split License from 4.x, version and transitive dependency policy | Opt-in independent package only; no core dependency or default template reference |
| SkiaSharp | Broad raster surface and established bitmap interop patterns | Native Skia assets, platform packaging, color-space/alpha ownership | Preferred first independent adapter candidate when native asset provenance is closed |
| NetVips | Lazy pipelines, glibc/musl and ARM64 deployment experience | libvips/native dependency graph and different execution model | Research/reference only for tile and peak-memory decisions |
| Magick.NET | Explicit Q8/Q16/HDRI precision profiles and resource limits | Large native dependency and external delegate surface | Research/reference only; no default adapter commitment |
| Neutral adapter | No third-party license or native dependency in core; explicit copy/lease boundary | Smaller feature surface and more conversion code in consumers | Core-compatible baseline; provide independently owned copy and optional borrowed views first |

The first package decision is therefore a neutral adapter contract with an independent SkiaSharp candidate. ImageSharp remains opt-in until its current license terms, transitive packages, and commercial use policy are reviewed for the consuming product. The core managed/native package must keep zero new third-party image dependencies.

## Conversion contract

- Every adapter declares source and destination channel order, alpha mode, premultiplication, depth, and color-space assumptions.
- A conversion either returns an independently owned `Mat`/bitmap or exposes a bounded borrow lease whose owner and release callback are explicit. It never returns a pointer to movable managed memory.
- Non-contiguous Mat rows are copied row by row unless the adapter can prove compatible stride and lifetime. A borrowed span cannot cross `await`, dispose, or a native header mutation.
- Copy paths validate dimensions, channels, depth, row bytes, stride, and checked total length before native or adapter code. Unsupported pixel formats fail explicitly.
- UI adapters remain separate from image adapters. Avalonia/WPF thread affinity and bitmap owner rules must not enter the core package.

## License and package gate

An independent adapter package must record package versions, transitive dependencies, licenses/notices, native asset provenance, vulnerability review, and source/package hashes. The core package dependency graph must remain unchanged. A package with unresolved license or native-asset evidence remains preview/internal and cannot enter the stable release matrix.

## Promotion and rollback

Promotion requires color/alpha/stride tests, non-contiguous ROI tests, dispose-order tests, at least one independent consumer, and cross-target compilation for the declared adapter matrix. Rollback is removal of the adapter package and return to `Mat` copy/clone APIs; no core API, runtime package identity, or native ABI is rewritten.

adapter package 在完成颜色/alpha/stride、非连续 ROI、dispose 顺序、独立 consumer 和目标框架编译证据前保持 preview/internal。回退是移除 adapter 并使用 `Mat` copy/clone；不能改写核心 API、runtime package identity 或 native ABI。
