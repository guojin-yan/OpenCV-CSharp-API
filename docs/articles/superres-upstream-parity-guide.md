# Super Resolution Upstream Map / Super Resolution 上游映射

This document records the measured OpenCV 5.0.0 contrib `superres` public-header scope. It is source/parser evidence, not a claim of repository-wide contrib parity.

本文记录 OpenCV 5.0.0 contrib `superres` 公共头文件范围，是源码/parser 证据，不代表整个 contrib 仓库 parity。

## Scope / 范围

The closure starts at `opencv2/superres.hpp` and includes `opencv2/superres/optical_flow.hpp`. The pinned parser emits 96 public declarations: 89 callable rows and seven class metadata rows. The headers use `CV_EXPORTS` rather than `CV_WRAP`, so extraction uses non-wrapper parser mode. The protected `SuperResolution` constructor, `initImpl`, and `processImpl` hooks are explicitly excluded from the public scope.

映射从 `opencv2/superres.hpp` 开始并包含 `opencv2/superres/optical_flow.hpp`，固定 parser 共输出 96 个公共声明：89 个 callable 和 7 个 class metadata。由于头文件使用 `CV_EXPORTS` 而不是 `CV_WRAP`，提取使用 non-wrapper parser mode；`SuperResolution` 的 protected 构造函数、`initImpl` 和 `processImpl` 明确排除。

## Classification boundary / 分类边界

All 89 callable rows are `intentionally-omitted`, with zero native entrypoints, zero managed members, and zero missing rows. The current package has no Superres native/managed owner. Video/camera frame sources and CUDA algorithms also depend on the OpenCV build, VideoIO backend, and CUDA toolkit/runtime; frame ownership and algorithm state are therefore outside the current ABI/API and deterministic smoke contract.

89 个 callable 全部标记为 `intentionally-omitted`，native entrypoint、managed member 和 missing 均为零。当前包没有 Superres native/managed owner；视频/相机 frame source 与 CUDA 算法还依赖 OpenCV 构建、VideoIO backend 及 CUDA toolkit/runtime，因此 frame 所有权和算法状态不在当前 ABI/API 与确定性 smoke 契约内。

## Evidence / 证据

```text
compatibility/superres-upstream-raw.json
compatibility/superres-upstream-classifications.json
compatibility/superres-upstream-map.txt
compatibility/superres-upstream-summary.json
compatibility/superres-implemented-families.json
```

```powershell
pwsh -NoProfile -File .\scripts\Test-SuperresUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```
