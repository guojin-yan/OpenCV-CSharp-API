# OpenCV 5.1 contrib mapping review

Eight historical parser scopes are refreshed against official opencv_contrib 5.1.0 commit b67a855188a3d683813731010f6d39ac614733a4, using the parser from OpenCV commit e4f78fbcf1a773fbed69e14500e8b734963eb585. The batch records 611 declarations and 511 callables across 61 contributing headers; 68 distinct source headers include the primary compatibility umbrellas. All 611 identities are retained. HoughPoint2Line has the only declaration change: Doxygen @retval/@remarks become @return/@note, with the signature and line-clipping descriptions unchanged.

The [current inventory](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/compatibility/opencv-5.1.0/upstream-map-refresh-summary.json) now records 22 scopes and 2,825 declarations. Another 51 historical maps need current-version review. See the [mapping overview](opencv-5.1-source-mapping.md) and [previous nine-module review](opencv-5.1-module-mapping.md) for earlier scope and behavior decisions.

| Module | Headers | Declarations | Callables | Implemented | Omitted | Metadata | Changed |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| xphoto | 6 | 47 | 39 | 30 | 9 | 8 | 0 |
| ximgproc | 26 | 249 | 212 | 189 | 23 | 37 | 1 |
| optflow | 4 | 85 | 71 | 70 | 1 | 14 | 0 |
| bgsegm | 1 | 66 | 58 | 50 | 8 | 8 | 0 |
| face | 9 | 62 | 49 | 40 | 9 | 13 | 0 |
| quality | 7 | 37 | 31 | 29 | 2 | 6 | 0 |
| img_hash | 7 | 31 | 23 | 23 | 0 | 8 | 0 |
| line_descriptor | 1 | 34 | 28 | 14 | 14 | 6 | 0 |

The 445 implemented callables retain exact native and managed evidence. The remaining 66 callables have explicit omissions; 100 declarations are type or enum metadata. Zero missing means every recorded declaration has a decision.

## Binding boundaries

- XPhoto retains the TonemapDurand omission. Its factory and accessors require an explicit nonfree build and release decision. Existing white-balance, denoising and inpainting bindings do not establish TonemapDurand availability.
- XImgProc retains 23 omissions, including stateful DTFilter/AdaptiveManifoldFilter objects, model-backed StructuredEdgeDetection/RFFeatureGetter, ellipse records, Radon output geometry and FileNode/FileStorage transport. One-shot filter calls and the EdgeDrawingParams value transport keep their recorded scope.
- OptFlow records its four contributing subheaders. The primary optflow.hpp umbrella is pinned as source identity but is outside that parser subset. The PCAFlow factory remains omitted; this review does not expand the map to every optical-flow declaration.
- BgSegm retains the GSOC/LSBP factory and apply/output omissions. Existing MOG/GMG/CNT and synthetic sequence wrappers retain their exact bindings.
- Face still requires caller-provided compatible landmark models. AAM/Kazemi factories, training-data/file helpers and other unbound calls remain omitted. An existing Facemark LBF wrapper does not certify arbitrary model provenance or fitting accuracy.
- Quality retains the BRISQUE SVM-object overload and multi-image MSE omission. Path-based BRISQUE still requires caller-provided model/range files.
- ImgHash retains all 23 callable bindings within its seven-header parser subset. This does not establish complete C++ or value-level metadata parity.
- LineDescriptor retains 14 omissions, including LSDDetector, multi-input matcher calls and KeyLine value accessors without separate native entrypoints. Managed value properties are recorded separately from native-backed callables.

## Reproduction and validation

Run scripts/Generate-UpstreamMap51.ps1 with an explicit PythonPath and RegenerateRaw, then scripts/Test-UpstreamMap51.ps1. The generator uses the pinned source/catalog hashes, historical classifications and exact declaration identities. Historical 5.0 files keep their original version.

The shared generator rejects 675 mutations across 21 scopes; DNN retains its separate 22-fixture guard. This batch adds 257 cases. Face, Quality and LineDescriptor have no emitted enum: their guards reject invented enum metadata without assuming that an enum exists. XPhoto's two inherited unordered managed-evidence lists are emitted in canonical ordinal string order while preserving every member; a real permutation fixture rejects noncanonical output. Existing slice artifacts remain byte-identical.

Unchanged scopes exercise missing, empty and incomplete change reviews through a controlled documentation delta. Official Git blobs, independent regeneration, LF/CRLF extraction, wrong-version rejection and CLI corruption checks provide separate source and artifact evidence.

This increment changes the mapping tool and documentation. It adds no managed member, constant or native export and leaves managed/native build inputs unchanged. Existing Windows binary/regression evidence keeps its actual implementation/build commit; other RIDs, optional models, nonfree features and complete C++/template parity require their own evidence.
