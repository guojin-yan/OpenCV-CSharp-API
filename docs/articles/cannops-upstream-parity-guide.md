# CannOps upstream parity boundary

This map records declarations from the pinned OpenCV contrib 5.0.0 CannOps module, whose C++ namespace is `cv::cann`. All 188 callable declarations are intentional omissions because the current package has no CannOps native ABI entrypoints or managed owner. The map adds declaration evidence; it does not provide Huawei CANN or Ascend NPU execution support.

## Installed header scope

All eight installed `include/opencv2/*.hpp` headers remain in scope, including internal implementation helpers. The scope describes parser-visible declarations in those installed headers, not a promise that every helper is a stable upstream API.

| Header | Retained declarations | Parser declarations before reconciliation |
| --- | ---: | ---: |
| `ascendc_kernels.hpp` | 0 | 0 |
| `cann.hpp` | 61 | 61 |
| `cann.inl.hpp` | 0 | 18 |
| `cann_call.hpp` | 41 | 41 |
| `cann_interface.hpp` | 61 | 61 |
| `cann_private.hpp` | 10 | 10 |
| `dvpp_call.hpp` | 22 | 22 |
| `stream_accessor.hpp` | 6 | 6 |

The final closure has 201 unique declarations: 188 callables, 12 classes/structs, and one enum. Thirteen rows are non-callable metadata; there are zero implemented and zero unexplained missing callables.

The pinned `hdr_parser.py` runs in non-wrapper mode with `CV_VERSION_MAJOR=5`, `OPENCV_ABI_COMPATIBILITY=500`, and `HAVE_CANN=1`. A temporary parser copy renames the qualified `AscendStream::Impl` and `AscendEvent::Impl` class/constructor spellings that the parser cannot handle directly. The extractor restores their original qualified identities, types, constructors, and destructor. It never changes upstream files.

Sixteen definitions in `cann.inl.hpp` match existing `cann.hpp` declarations uniquely by qualified name and argument types. Parameter names and defaults may differ, so the map retains the declaration identity and records each match. The other two inline definitions are private `AscendStream`/`AscendEvent` constructors taking `Ptr<...::Impl>`; both remain explicitly excluded. Seven `NEVER_DEFINED` blocks in `cann_interface.hpp` contain binding-only placeholder overloads. That macro stays undefined, leaving four real overloads for each affected arithmetic/bitwise operation.

## Upstream build and runtime boundary

The upstream module disables CannOps on iOS, WinRT, Android, Apple, and Windows, or when `HAVE_CANN` is absent. It requires OpenCV core, external `CANN_INCLUDE_DIRS`/`CANN_LIBRARIES`, and the AscendC kernel subdirectory. The pinned core CANN finder requires UNIX on `aarch64`, `x86_64`, or `amd64`, external AscendCL, operator compiler, DVPP, graph/compiler and operator-prototype libraries, and a successful `VALID_ASCENDCL` compile probe. Those configuration checks are not package runtime verification.

AscendC additionally requires an external compiler/devkit CMake package. The pinned defaults are `SOC_VERSION=ascend310p3`, `RUN_MODE=npu`, and `/usr/local/Ascend/ascend-toolkit/latest`; the upstream run-mode setting also describes simulation/CPU alternatives. This map does not test those execution modes.

Three external dependency records separate the Huawei CANN SDK/libraries, AscendC compiler/kernel/SOC policy, and Ascend device/runtime initialization plus allocator, stream/event/context synchronization and Mat ownership. This package bundles none of those contracts. Native and managed evidence counts are both zero, and repository-wide upstream parity is not claimed.

## Verification

`scripts/Test-CannOpsUpstreamMap.ps1` checks source hashes, per-header counts and ordinals, restored implementation names, inline/private reconciliation, inactive placeholders, SDK/platform/toolchain gates, classifications, and absence of package wrapper evidence. Twelve rejected negative fixtures cover altered source closure, hashes, declarations, normalization, inline reconciliation, private constructors, parser definitions, classifications, false native/managed evidence, build gates, and parity claims.

The catalog, source-evidence initializer, change ledger, and release closeout bind all five generated compatibility artifacts. Run the semantic guard with:

```powershell
pwsh -NoProfile -File .\scripts\Test-CannOpsUpstreamMap.ps1
```

## 中文边界说明

本映射覆盖 CannOps 的全部 8 个安装头文件（含内部 helper），共 201 项声明：188 个 callable、12 个 class/struct、1 个 enum。16 个内联定义归并到已有声明，2 个私有内联构造函数明确排除，7 个 `NEVER_DEFINED` 占位块保持禁用。嵌套 `Impl` 类仅在临时 parser 副本中改名，最终身份恢复为原始限定名。

188 个 callable 均为 intentional omission，未解释缺项和 native/managed evidence 均为 0。外部 Huawei CANN SDK、AscendC 编译工具链及匹配的 Ascend 设备/运行时与资源生命周期不属于当前包提供的能力。目录与发布证据记录这些边界，不宣称提供 CANN 加速或全上游 parity。
