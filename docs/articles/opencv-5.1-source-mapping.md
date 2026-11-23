# OpenCV 5.1 source mapping

Current evidence uses official OpenCV commit `e4f78fbcf1a773fbed69e14500e8b734963eb585` and its own `hdr_parser.py`. The [versioned inventory](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/compatibility/opencv-5.1.0/upstream-map-refresh-summary.json) records five reviewed parser slices: Calib3D, Core, DNN, ImgProc and ImgCodecs. The 73-map historical catalog retains its OpenCV 5.0.0 identity; 68 maps still need 5.1 review.

本批新增 Calib3D 的 208 项声明复审，累计五个范围 914 项（此前 Core/ImgProc/ImgCodecs 521 项、DNN 185 项）。implemented 表示记录的 managed/C ABI 绑定存在，并受现有参数校验限制；它不证明上游新输入类型、可选 backend/codec、完整 C++ 或模板覆盖。

| Parser slice | Declarations | Callables | Added identities | Removed identities | Changed retained identities | Implemented | Intentionally omitted |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Calib3D, 4 headers | 208 | 169 | 16 | 2 | 8 | 169 | 0 |
| Core, 11 headers | 276 | 232 | 19 | 1 | 7 | 176 | 46 |
| ImgProc, imgproc.hpp | 206 | 169 | 3 | 0 | 6 | 161 | 8 |
| ImgCodecs, imgcodecs.hpp | 39 | 22 | 0 | 0 | 2 | 22 | 0 |

Core also retains five unsupported and five conditional declarations. All four shared-tool slices have zero unexplained missing declarations. Types/enums remain metadata with complete parser properties. Each delta keeps the before/after declarations and a specific decision. Core's enum identities include their values; ImgProc/ImgCodecs preserve their historical name-only enum identities, so enum-value changes appear among retained changes.

## Reviewed API boundaries

- Calib3D matches the `undistortImagePoints` parameter rename to its existing binding. The 5.1 parser newly emits hand-eye and robot-world hand-eye declarations, which bind to reviewed existing wrappers and synthetic tests. These are not new implementations.
- Calibration enum metadata exposes a real flag conflict: `DisableSchurComplement` moves to bit 27; bit 18 remains `TiltedModel`. The managed constant is corrected and four real calibration combinations verify distortion-vector shape and recovered focal lengths. Recompile callers and migrate saved numeric flags using the [calibration migration guide](calibration-5.1-migration.md).
- Calib3D documentation clarifies homogeneous camera position, fixed-iteration undistortion, fisheye initialization and scheduling-dependent Schur accumulation. Existing float-point validation remains; wider double inputs and extreme-coordinate domains need separate evidence.

- Core adds MatShape transforms, ReduceParams, N-D tensor operations and linear assignment. These declarations have explicit omissions. Existing Mat header reshapes, channel Split/Concat helpers and the 2D Reduce API do not implement those new overloads.
- Upstream widened dim-based Reduce to N-D input and PatchNaNs to half/bfloat inputs. Managed validation still admits at-most-2D reductions with Sum/Avg/Max/Min and CV_32F/CV_64F PatchNaNs inputs. The map keeps these limitations explicit.
- Upstream deprecated transposeND in favor of a new transpose overload. The existing managed/C ABI entry points and their continuous single-channel input contract remain available.
- ImgProc's alphaComposite requires Porter-Duff, alpha convention and output-channel contracts. FontFace.setBuiltinFont changes global slots for future objects across threads. Neither new callable is wrapped; AddWeighted and instance font loading are different operations.
- ImgProc adds Oklab codes 155–158 and changes its upper sentinel to 159. These names are not exposed by the existing managed enum; its published ColorCvtMax value remains 155. The map does not claim value-level enum parity.
- ImgCodecs adds TIFF_COMPRESSION_LEVEL=260 and AVIF_ALPHA_QUALITY=515 metadata. These are not new managed options. AVIF 16-bit writing depends on libavif 1.4.0+; fully opaque alpha can be omitted. A writer binding does not certify this dependency combination or alpha preservation.
- Remap documentation now describes hardware-dependent, usually modest fixed-point speed gains; no fixed speedup is promised. Other documentation changes are recorded individually.

本次不增加公开成员或 native 导出；修正一个公开枚举常量值并要求调用方重编译，ABI 1 和包版本 5.1.0 保持。后续实现新的 callable、常量或扩展输入范围时，须补充参数、所有权、真实运行和兼容性证据，更新 ledger 后再调整分类。

## Reproduction and review

```powershell
pwsh -NoProfile -File scripts/Generate-UpstreamMap51.ps1 -RegenerateRaw -PythonPath <python>
pwsh -NoProfile -File scripts/Test-UpstreamMap51.ps1
```

Raw extraction requires an explicit Python interpreter and pinned source files. Routine semantic checks use .NET 10 without Python or network access. `-OutputDirectory <temporary-directory>` supports independent regeneration. Source hashes normalize CRLF/CR to official LF without trimming; generated artifacts require UTF-8 without BOM, LF and a final newline.

The shared generator pins a reviewed catalog containing source hashes, historical input hashes, raw snapshots and every added/removed/changed decision. It matches exact identities and records explicit reviewed argument renames or existing bindings, with pinned implementation/test source evidence. It never infers a binding from an old ordinal. Core/ImgProc/ImgCodecs each run 30 mutations; Calib3D runs 37, including false new native/managed evidence, invalid review kinds, source/signature/evidence mutations in a rename (127 total). Unknown declarations or unreviewed changes fail closed. Adding a slice requires reviewing official source, regenerating its snapshot, recording all decisions, and updating the catalog pin and release inventory together.

The parser-map increment can be removed independently. Reverting the corrected calibration constant on 5.1 would reintroduce the tilted-distortion collision; rollback must align caller binaries, stored flags and native runtime together. The separate [DNN migration guide](dnn-5.1-migration.md) covers enum conversion and actual inference evidence.
