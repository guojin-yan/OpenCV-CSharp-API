# SFM upstream parity boundary

This map records the installed Structure From Motion headers from pinned OpenCV contrib 5.0.0. It contains 62 declarations: 55 callables, four classes, and three enums. Every callable is intentionally omitted because this package has no SFM native ABI entrypoint or managed owner. Native/managed evidence and unexplained missing counts are zero.

## Header and parser scope

The extractor parses `opencv2/sfm.hpp` and all nine installed `opencv2/sfm/*.hpp` headers in non-wrapper mode. It preserves image-path overloads, the pipeline factory, and the base destructor even when they lack upstream wrapper annotations. Source normalization and exact-identity deduplication are unnecessary. The three anonymous enums retain their exact value lists so their shared `<unnamed>` parser name does not merge distinct declarations. Camera/reconstruction option class metadata retains 14 and five writable properties respectively.

| Header | Declarations | Callable declarations | Ceres surface |
| --- | ---: | ---: | --- |
| `sfm.hpp` | 0 | 0 | Umbrella includes are conditional |
| `sfm/conditioning.hpp` | 5 | 5 | No |
| `sfm/fundamental.hpp` | 11 | 11 | No |
| `sfm/io.hpp` | 2 | 1 | No |
| `sfm/numeric.hpp` | 2 | 2 | No |
| `sfm/projection.hpp` | 5 | 5 | No |
| `sfm/reconstruct.hpp` | 4 | 4 | Yes |
| `sfm/robust.hpp` | 2 | 2 | No |
| `sfm/simple_pipeline.hpp` | 30 | 24 | Yes |
| `sfm/triangulation.hpp` | 1 | 1 | No |

`robust.hpp` is installed but absent from the umbrella's eight includes. Its two robust estimators remain in scope. The base surface has 28 declarations (27 callables and one enum); the optional Ceres surface has 34 declarations (28 callables and six class/enum metadata rows).

## Build gates and direct header visibility

The module requires `HAVE_EIGEN` and `SFM_DEPS_OK`, which binds a successful Glog/Gflags compile/link probe. It depends on OpenCV core, geometry, features, xfeatures2d, and imgcodecs. External Ceres Solver enables reconstruction and the simple pipeline. The pipeline library links `CERES_LIBRARIES`, and the corresponding implementation files use `#if CERES_FOUND`. Each mapped row retains either `HAVE_EIGEN_AND_SFM_DEPS_OK` or the additional `_AND_CERES_FOUND` condition according to its source header.

The pinned CMake file defines `CERES_FOUND` as either one or zero. `reconstruct.hpp` tests `defined(CERES_FOUND)`, so a direct parse with the macro defined as zero still emits its four declarations; leaving it undefined emits none. The umbrella tests the macro's value before including the optional headers, while `simple_pipeline.hpp` has no Ceres guard of its own. The extractor records undefined/zero/one probes with counts 0/4/4. Header visibility alone does not establish linkable Ceres support.

The README advises Linux/GNU systems only. The module CMake has no matching unconditional platform disable and contains Windows-specific macro handling. The map preserves both facts as source evidence without certifying a platform or treating documentary advice as an enforced build condition.

## Runtime and package boundary

Three external dependency records cover Eigen/Glog/Gflags, optional Ceres Solver, and caller-provided image paths, point tracks, reconstruction files, calibration and output ownership. SFM execution and reconstruction object lifetime are outside the current package contract; no SFM runtime support or repository-wide upstream parity is claimed.

The pinned upstream implementation also has two explicit limitations. `computeOrientation` contains `TODO: finish implementation`, and all four `reconstruct` overloads reject their affine branch with `StsNotImplemented`. These limits appear in callable omission reasons and are bound to the relevant upstream source hashes.

## Verification

`scripts/Test-SfmUpstreamMap.ps1` validates all ten header partitions, eight build/runtime evidence sources, anonymous enum values, option properties, image/track overloads, factory/destructor closure, dependency gates, Ceres direct-header probes, per-row conditions, omission reasons and zero package wrapper evidence. Twelve rejected negative fixtures cover omitted `robust.hpp`, lost Ceres classification/gates, incorrect defined-zero behavior, dropped unannotated overloads, enum/source corruption, false implementation/native/managed evidence, erased affine limitations and false parity claims.

```powershell
pwsh -NoProfile -File .\scripts\Test-SfmUpstreamMap.ps1
```

The catalog, source-evidence initializer, change ledger and release closeout bind all five generated compatibility artifacts and the extractor's shared Python helper.

## 中文边界说明

本映射覆盖 SFM 的全部 10 个安装头文件，包括 umbrella 未直接引入的 `robust.hpp`。共 62 项声明：55 个 callable、4 个 class、3 个 enum；基础范围 28 项，需要 Ceres 的重建/管线范围 34 项，其中 28 项 callable。所有 callable 为 intentional omission，未解释缺项及 native/managed evidence 均为 0。

基础模块依赖 Eigen 和通过编译/链接探测的 Glog/Gflags；重建/管线另需 Ceres Solver。`CERES_FOUND=0` 仍能让直接包含的 `reconstruct.hpp` 暴露声明，但实现与 umbrella 受宏值控制，因此不能据头文件可见性推断运行能力。上游 `computeOrientation` 尚未完成，四个重建重载的 affine 分支明确抛出 `StsNotImplemented`。这些限制及调用方图像、轨迹、标定和输出所有权均已记录，未提供 SFM 加速/重建运行承诺。
