# XFeatures2D Upstream Parity Guide

This guide records the scoped OpenCV 5.0.0 contrib `xfeatures2d` evidence map. It is a source-scope and wrapper-boundary record, not a claim of repository-wide upstream parity.

## Scope

The map covers the parser-emitted declarations reached from `opencv2/xfeatures2d.hpp`, with the two source headers `xfeatures2d/cuda.hpp` and `xfeatures2d/nonfree.hpp`. The pinned closure contains 21 declarations: 18 callable declarations, two classes, and one enum.

All 18 callable declarations are intentionally omitted. The current package has no XFeatures2D native ABI entrypoint or managed public owner, so the map reports zero implemented, zero missing, and zero native or managed evidence rows. The class and enum rows remain non-callable parser metadata.

## Dependency and ownership boundary

The CUDA declarations require an optional CUDA toolkit/runtime and build configuration. The nonfree declarations require an explicit `OPENCV_ENABLE_NONFREE` build and licensing decision. The package bundles neither dependency, CUDA runtime, nonfree feature model, nor an unverified wrapper surface. These two external dependencies are recorded in `compatibility/xfeatures2d-upstream-classifications.json`.

## Verification

Run the focused guard from the repository root:

```powershell
pwsh -NoProfile -File .\scripts\Test-XFeatures2DUpstreamMap.ps1
```

The guard verifies the pinned header/parser hashes, two-header source closure, 21-declaration ordinal closure, 18 intentional omissions, zero missing rows, zero native/managed evidence, the two dependency notes, and the prohibition on repository-wide parity claims.

中文说明：本映射只覆盖固定的 `xfeatures2d.hpp` parser 声明范围。由于当前包没有 XFeatures2D 的 native ABI 或 managed owner，18 个 callable 均明确标记为 intentionally-omitted；CUDA 工具链/运行时以及 `OPENCV_ENABLE_NONFREE` 构建和许可决策属于外部依赖，仓库不宣称已封装或已验证这些功能。
