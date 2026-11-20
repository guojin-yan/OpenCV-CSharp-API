# MATLAB installed-helper source map

OpenCV 5.0.0 contrib MATLAB is a `BINDINGS` module that generates MATLAB `+cv` gateways from available OpenCV module headers. This source map covers its four installed helper headers: `bridge.hpp`, `map.hpp`, `mxarray.hpp` and `transpose.hpp`. It records 190 unique declarations in the pinned parser's non-template public subset: 185 intentional callable omissions and five class metadata rows, with zero missing rows within that subset and zero native/managed wrapper evidence.

MATLAB 是生成 `+cv` gateway 的 bindings 模块。本清单纳入全部四个安装 helper 头文件，parser 非模板子集有 190 项声明（185 个 callable intentional omission、5 个 class metadata）。子集内 0 missing，不代表模块全部 callable 已闭合；当前包没有 MATLAB native/managed wrapper。

| Installed header | Parser declarations | Parser callables | Lexical templates | Source function occurrences |
| --- | ---: | ---: | ---: | ---: |
| `opencv2/matlab/bridge.hpp` | 154 | 153 | 9 | 164 |
| `opencv2/matlab/map.hpp` | 0 | 0 | 2 | 3 |
| `opencv2/matlab/mxarray.hpp` | 36 | 32 | 23 | 71 |
| `opencv2/matlab/transpose.hpp` | 0 | 0 | 4 | 4 |

The linked `compatibility/matlab-source-reviewed-extensions.json` preserves 38 lexical template units (15 class templates/specializations, 22 function templates/specializations and one alias template) and all 22 conditional directives. These source units retain exact LF-normalized offsets, line numbers, snippet hashes, declaration heads and nested branch conditions. They overlap and include inactive branches, declaration/definition pairs, private helpers and uninstantiated templates. Their count is not an API or concrete instantiation count. The C++11 `Map` alias and pre-C++11 fallback class are both retained; SSE2 transpose specialization and type traits remain outside the parser callable subset.

源码清单保留全部分支的模板、specialization、alias 和条件位置，包括未激活分支与重叠片段；这些数量不能计为具体实例化 API。`map.hpp` 和 `transpose.hpp` 的 parser 计数为零，也不能据此声称它们没有 callable。

The linked `compatibility/matlab-source-callables.json` uses pinned tree-sitter 0.25.2 and C++ grammar 0.23.4 to enumerate 242 uninstantiated source function occurrences: 238 definitions and four declarations, with 39 occurrences in template contexts. This includes the fallback Map members, Traits specializations, nested ArgumentParser::Variant helpers and transpose specializations that the original parser skips. All 185 parser callables reconcile by exact owner, parameter and return signatures and static/const qualifiers; the two inline declaration/definition pairs retain separate source occurrences. Each function preserves its source head/hash, UTF-8 byte and LF text offsets, namespace/class/template contexts, lexical access, storage qualifiers, method const, parameter count, variadic status, branch conditions, constructor initializer and definition-body witness.

源码 CST 新增 242 处未实例化函数声明（238 处定义、4 处仅声明，其中 39 处带模板上下文），不改变原 parser 的 185 个 callable omission。源码位置、完整签名、private/nested/template 上下文和所有分支均保留；计数含声明/定义重复及未激活分支，不能等同于公开或具体实例化 API 数。

For CST input only, preprocessor directive lines are blanked while preserving every UTF-8 byte position and retaining both branches. The checked source has one conditional scope mismatch: `map.hpp` opens `namespace matlab` before its C++11 test, but places the closing brace inside the pre-C++11 fallback branch. The inventory records the different opening/closing conditions instead of treating the flattened syntax tree as an evaluated C++11 translation unit. `MxArray`'s friend `swap` retains its private lexical placement and namespace-qualified `matlab::swap` identity; it is not counted as a private member. Ten constructor initializer lists are retained. The grammar reports zero declaration errors and one body diagnostic at `va_arg(opt, const char*)`; its exact source/hash and enclosing body remain explicit, without claiming compilation success.

预处理行仅在语法输入副本中按字节位置留白，没有执行预处理，也没有修改上游源码。`map.hpp` 的条件命名空间闭合、friend 的命名空间身份、10 个构造初始化列表和 `va_arg` 函数体诊断均由独立 .NET 守卫核验；实际宏配置的 C++ 编译/链接仍待验证。

The pinned OpenCV parser deliberately ignores `__cplusplus` directives. Temporary copies rename only that identifier and strip the `L` integer suffix in its two `>= 201103L` directives so the Python expression evaluator can apply the recorded source input `201103`. Original parser probes produce 36 `mxarray.hpp` declarations for `199711`, `201103` and `201703`; normalized probes produce 35, 36 and 36. The single move constructor retains a separate C++11 condition. These are extraction inputs, not measured compiler capabilities. Upstream sources and hashes remain unchanged.

Fourteen parser name repairs restore twelve qualified conversion operators to the `Bridge` owner and remove return-type text from two namespace-scope static names. Static and const modifiers, assignment/constructor overloads and the variadic `addVariant` signature remain intact. Two out-of-class inline definitions reconcile to their existing class declarations rather than inflating the unique declaration count.

The compatibility inventory binds twelve upstream build/generator/test files. The actual module requires an external MATLAB installation (`mex.h`, `mx`, `mex`, `mat`, MEX script, extension and architecture), Python/Jinja2, matching compiler/MATLAB bitness, and successful generator/MEX configure probes. Android and Apple framework builds disable it. Nineteen optional OpenCV modules feed the generator according to their availability. `compile.cmake` unconditionally skips Debug configurations even though its preceding comment mentions MSVC. Pinning configure-test sources does not prove those probes were executed.

`MxArray(const mxArray*)` borrows its input; copy construction duplicates it; owned destruction calls `mxDestroyArray`; `releaseOwnership()` transfers responsibility back to MATLAB. Bridge matrix conversions copy and transpose between OpenCV and MATLAB layouts. Generated object pointers, interpreter behavior and MEX exception handling require a separate lifetime contract. The pinned Bridge also contains 48 no-op assignment definitions and a `toBool()` body that returns zero. Those upstream placeholders remain explicit limits; a parsed declaration does not establish conversion correctness.

构建、所有权与精度仍按源码事实记录：外部 MATLAB/MEX 环境、可用 OpenCV 模块、指针与解释器生命周期没有由本轮验证。上游存在空实现，不能以 header 或 parser 通过来承诺运行正确性。

Regenerate with a supplied Python executable, or verify checked evidence without Python/MATLAB/downloads:

```powershell
pwsh -NoProfile -File scripts/Generate-MatlabUpstreamMap.ps1 -PythonPath <python>
pwsh -NoProfile -File scripts/Test-MatlabUpstreamMap.ps1
```

The semantic guard independently discovers lexical templates and branch context in .NET and rejects 36 real mutations. The original 18 cover lost headers/templates, span/hash and branch damage, wrong alias kind, broken name/inline repairs, lost C++11/MEX conditions, false wrapper evidence, erased upstream placeholder limits and overstated coverage. Eighteen additional fixtures cover source functions, signatures, same-arity overload and inline-definition reconciliation, qualifiers, friend scope, constructor initializers, conditional namespace closure, hidden diagnostics and false instantiation/external/generated/runtime claims. Ordinary verification needs no Python or downloads. Generation reuses the isolated, hash-pinned Cudev syntax-parser wheels without adding a package dependency. The catalog, source initializer, change ledger and release closeout bind all seven generated artifacts.

Complete semantic callable coverage, external include/using/alias closure, concrete template instantiation, generated `+cv` gateway coverage, C++ compilation/linking and MATLAB/MEX runtime verification remain open. Roll back the CST increment by reverting its source-callable inventory, generation/guard extensions, linked hashes and closeout readout together while retaining the earlier parser/lexical evidence. Package API, ABI and dependencies are unaffected. MAP-005 remains in progress.
