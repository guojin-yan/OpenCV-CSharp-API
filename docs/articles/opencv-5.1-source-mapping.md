# OpenCV 5.1 source mapping

Current evidence uses official OpenCV commit `e4f78fbcf1a773fbed69e14500e8b734963eb585` and its own `hdr_parser.py`. The [versioned inventory](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/compatibility/opencv-5.1.0/upstream-map-refresh-summary.json) records four reviewed parser slices: Core, DNN, ImgProc and ImgCodecs. The 73-map historical catalog retains its OpenCV 5.0.0 identity; 69 maps still need 5.1 review.

本批复审 Core、ImgProc、ImgCodecs，共 521 个声明。加上前批 DNN 的 185 个声明，当前四个范围合计 706 项。implemented 表示记录的 managed/C ABI 绑定存在，并受现有参数校验限制；它不证明上游新输入类型、可选 backend/codec、完整 C++ 或模板覆盖。

| Parser slice | Declarations | Callables | Added identities | Removed identities | Changed retained identities | Implemented | Intentionally omitted |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Core, 11 headers | 276 | 232 | 19 | 1 | 7 | 176 | 46 |
| ImgProc, imgproc.hpp | 206 | 169 | 3 | 0 | 6 | 161 | 8 |
| ImgCodecs, imgcodecs.hpp | 39 | 22 | 0 | 0 | 2 | 22 | 0 |

Core also retains five unsupported and five conditional declarations. All three slices have zero unexplained missing declarations. Types/enums remain metadata with complete parser properties. Each delta keeps the before/after declarations and a specific decision. Core's enum identities include their values; ImgProc/ImgCodecs preserve their historical name-only enum identities, so enum-value changes appear among retained changes.

## Reviewed API boundaries

- Core adds MatShape transforms, ReduceParams, N-D tensor operations and linear assignment. These declarations have explicit omissions. Existing Mat header reshapes, channel Split/Concat helpers and the 2D Reduce API do not implement those new overloads.
- Upstream widened dim-based Reduce to N-D input and PatchNaNs to half/bfloat inputs. Managed validation still admits at-most-2D reductions with Sum/Avg/Max/Min and CV_32F/CV_64F PatchNaNs inputs. The map keeps these limitations explicit.
- Upstream deprecated transposeND in favor of a new transpose overload. The existing managed/C ABI entry points and their continuous single-channel input contract remain available.
- ImgProc's alphaComposite requires Porter-Duff, alpha convention and output-channel contracts. FontFace.setBuiltinFont changes global slots for future objects across threads. Neither new callable is wrapped; AddWeighted and instance font loading are different operations.
- ImgProc adds Oklab codes 155–158 and changes its upper sentinel to 159. These names are not exposed by the existing managed enum; its published ColorCvtMax value remains 155. The map does not claim value-level enum parity.
- ImgCodecs adds TIFF_COMPRESSION_LEVEL=260 and AVIF_ALPHA_QUALITY=515 metadata. These are not new managed options. AVIF 16-bit writing depends on libavif 1.4.0+; fully opaque alpha can be omitted. A writer binding does not certify this dependency combination or alpha preservation.
- Remap documentation now describes hardware-dependent, usually modest fixed-point speed gains; no fixed speedup is promised. Other documentation changes are recorded individually.

本次不新增公开 API 或 native 导出；ABI 1 和包版本 5.1.0 保持。后续实现新的 callable、常量或扩展输入范围时，须补充参数、所有权、真实运行和兼容性证据，更新 ledger 后再调整分类。

## Reproduction and review

```powershell
pwsh -NoProfile -File scripts/Generate-UpstreamMap51.ps1 -RegenerateRaw -PythonPath <python>
pwsh -NoProfile -File scripts/Test-UpstreamMap51.ps1
```

Raw extraction requires an explicit Python interpreter and pinned source files. Routine semantic checks use .NET 10 without Python or network access. `-OutputDirectory <temporary-directory>` supports independent regeneration. Source hashes normalize CRLF/CR to official LF without trimming; generated artifacts require UTF-8 without BOM, LF and a final newline.

The shared generator pins a reviewed catalog containing source hashes, historical input hashes, raw snapshots and every added/removed/changed decision. It matches exact identities, never old ordinals. Each module runs 30 mutations covering source scope, signatures, enum/property/documentation loss, classifications, false evidence and incomplete reviews/deltas (90 total). Unknown declarations or unreviewed changes fail closed. Adding a slice requires reviewing official source, regenerating its snapshot, recording all decisions, and updating the catalog pin and release inventory together.

Rollback removes this batch's versioned artifacts and shared registration while retaining historical maps and runtime/ABI. The separate [DNN migration guide](dnn-5.1-migration.md) covers enum conversion and actual inference evidence.
