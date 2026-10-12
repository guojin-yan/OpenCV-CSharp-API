# HDF Upstream Map / HDF 上游映射

This guide records the scoped OpenCV 5.0.0 contrib `hdf` parser/header boundary. It is evidence for the public header closure, not a claim of repository-wide contrib parity.

本指南记录固定 OpenCV 5.0.0 contrib `hdf` 的 parser/header 边界证据，不宣称整个 contrib 仓库已经 parity。

## Scope / 范围

The map covers `opencv-source/opencv_contrib-5.0.0/modules/hdf/include/opencv2/hdf.hpp` and its parser-emitted `hdf/hdf5.hpp` closure: one `HDF5` class and 37 callable declarations. Every callable is intentionally omitted because the surface requires an external HDF5 dependency and the current package has no native HDF wrapper entrypoint or managed public owner.

映射覆盖 `opencv-source/opencv_contrib-5.0.0/modules/hdf/include/opencv2/hdf.hpp` 及其 parser 发出的 `hdf/hdf5.hpp` closure：一个 `HDF5` class 与 37 个 callable 声明。所有 callable 均标记为 intentional omission，因为该 surface 依赖外部 HDF5，当前包没有 HDF native wrapper entrypoint 或 managed public owner。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-HdfUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The focused guard verifies the pinned header/include closure, the 38-declaration partition, external HDF5 dependency rationale, zero native/managed evidence, and the prohibition on repository-wide parity claims.

专项 guard 会检查固定 header/include closure、38 个声明的 partition、外部 HDF5 依赖理由、native/managed 证据为零，以及禁止 repository-wide parity 声明。
