# Cudev source inventory and partial callable map

Cudev is OpenCV's CUDA device layer. This evidence inventories all 76 installed `.hpp` files from pinned OpenCV contrib 5.0.0, including component and detail headers. It records a partial callable map and a separate inventory of template/macro source units. Complete callable coverage remains open.

The pinned OpenCV `hdr_parser.py` deliberately skips statements beginning with `template` and does not expand declaration macros. Its 144 emitted declarations comprise 110 callables, 24 classes/structs and ten enums under the recorded CUDA source configuration. Every emitted callable is intentionally omitted because this package has no Cudev native ABI entrypoint or managed wrapper. Zero unexplained missing declarations applies to this parser subset.

## Template and macro evidence

The extractor uses only Python's standard library and the pinned parser. `source_inventory.py` separately scans every installed header with all preprocessor branches retained. It masks comments and literals, removes logical directive lines from declaration discovery, balances template parameter lists and declaration bodies, and records exact source positions and SHA256 values. It distinguishes template-template parameters from declarations and preserves nested template declarations inside class bodies.

| Source unit | Count | Meaning |
| --- | ---: | --- |
| Direct template declarations | 1,100 | 454 class/struct templates and 646 function templates, including forward declarations and specializations |
| Template-template parameters | 11 | Nested template parameter lists, associated with their enclosing declaration |
| Function-like macro definitions | 49 | Full replacement text, parameters, source positions and conditional context |
| Macro invocations | 1,206 | Source arguments and references to named source definitions |
| Conditional directives | 397 | Every `if`/`ifdef`/`ifndef`/`elif`/`else`/`endif`, including header guards |
| Template keywords within macro bodies | 92 | Counted inside the 49 definitions, separately from direct declarations |

The 1,100 direct declarations, 11 template-template parameters and 92 macro-body template keywords reconcile all 1,203 `template <` tokens outside comments and literals. Counts retain inactive branches. Source units overlap: a class-template span can contain member templates or macro invocations, and one macro definition can be invoked repeatedly. These counts must not be added to the parser's callable count or interpreted as concrete template instantiations.

Coordinates use UTF-8 source normalized to LF, zero-based Unicode offsets with an exclusive end, and one-based line numbers. A macro invocation's definition references are lexical name references across the installed source inventory; they do not assert that a definition is active in every configuration. The inventory performs no macro expansion.

The source inventory retains complete class-template bodies, but it does not semantically enumerate their non-template members, expanded macro declarations, external `using` declarations, or concrete template instantiations. Those remain work for a future semantic extraction pass. This batch does not mark Cudev's complete callable mapping or MAP-005 as done.

## Parser normalization

CUDA attribute words are removed only in temporary parser copies. The original installed-header paths and hashes remain the evidence sources. Static and const qualifiers and conversion/call operators remain in the parser metadata.

The pinned parser first computes namespace-scope static function names through a different path from its non-wrapper argument parser. This contaminates 13 names with their return type, for example `cv.cudev.color_cvt_detail.int LabCbrt_b`. The extractor records each repair and restores `cv.cudev.color_cvt_detail.LabCbrt_b` while preserving `/S`. It does not remove `static` from the source or discard these declarations. Exact parser identities remain unique.

## Build and configuration boundaries

The module's CMake file disables Cudev unless `HAVE_CUDA` is true. It recursively includes the component/detail headers in the module header list, includes OpenCV core, and adds its tests only when `BUILD_TESTS` is enabled and `BUILD_opencv_world` is disabled. The umbrella is not used to shrink the recursive installed-header inventory.

The recorded parser configuration uses CUDA 12.4 source definitions and source architecture 750. Five direct-header probes vary `CV_CUDEV_ARCH` over 0, 120, 200, 300 and 750. `warp/shuffle.hpp` emits 0/0/0/25/25 declarations respectively; `util/atomic.hpp` and `util/simd_functions.hpp` emit 12 and 38 in each probe. These are parser visibility results. The source inventory separately retains architecture gates, the `__CUDACC_VER_MAJOR__ >= 9` shuffle compatibility branches and the `CUDART_VERSION < 12040` tuple branches even where the non-template parser count does not change.

Architecture zero simulates NVCC's host pass with `__CUDACC__` still defined. None of the configurations is a compiler, linking, GPU, driver or runtime test. The package claims no Cudev runtime support. Three dependency records cover the external CUDA toolkit/runtime, Thrust and other external templates, and caller-owned device memory/GpuMat/Stream plus NVIDIA device/driver execution.

## Verification

`Test-CudevUpstreamMap.ps1` independently reconstructs source token discovery and conditional context in .NET, so checked-in evidence can be validated without a Python installation. It checks all 76 header hashes/partitions, every recorded source span/hash/line/head, template-template parent links, macro definitions/arguments/references, conditional directives, all six source counts, static-name repairs, source-architecture probes, omission classifications, build conditions and empty wrapper evidence.

Sixteen real negative fixtures reject lost headers/templates/macros, damaged spans, false conditional contexts or macro replacements, broken template parents, static-name corruption, weakened build gates, false host shuffle visibility, implementation/native/managed evidence, complete callable claims and CUDA execution claims. Generation also runs lexical tests for nested parameters, shift expressions, comment/literal false positives, alternate branches and malformed/unbalanced source.

```powershell
pwsh -NoProfile -File .\scripts\Test-CudevUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Generate-CudevUpstreamMap.ps1 -PythonPath <python-executable>
```

The catalog, source-evidence initializer, change ledger and release closeout bind the five parser-map artifacts, the supplemental source inventory, both extraction scripts, generation/verification scripts and this guide. A complete callable/instantiation pass and CUDA compiler/runtime validation remain open.

## 中文边界说明

本次是 cudev 源码审计增量，纳入全部 76 个安装头文件。144 项 parser declarations 仅代表非模板子集，其中 110 个 callable 全部 intentional omission，24 个 class/struct、10 个 enum 为 metadata；子集内无未解释 missing，native/managed wrapper evidence 均为 0。

独立清单保留全部条件分支中的 1,100 个直接模板声明、11 个模板模板参数、49 个函数式宏定义、1,206 处宏调用、397 条条件指令及宏体中的 92 处 template token。每项带源码位置、片段哈希及条件，不能合计为模板实例化后的 API 总数。13 处 static 名称修正保留 `/S`；五组架构探测只证明 parser 可见性。

完整 callable 覆盖、class-template 非模板成员及宏展开/模板实例化的语义枚举、CUDA 编译/链接/设备运行验证均未闭合，MAP-005 继续保持 in-progress。外部 CUDA/Thrust、NVIDIA driver/device、GpuMat/Stream 与异步执行所有权均不据源码可见性推断为本包支持。
