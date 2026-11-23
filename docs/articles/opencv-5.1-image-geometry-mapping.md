# OpenCV 5.1 image and geometry mappings

Fourteen historical contrib parser scopes are refreshed using official opencv_contrib 5.1.0 commit b67a855188a3d683813731010f6d39ac614733a4 and the parser from OpenCV commit e4f78fbcf1a773fbed69e14500e8b734963eb585. All 346 identities and parser declaration fields, including documentation, are unchanged from their historical snapshots. The 42 contributing headers and primary umbrellas provide 49 distinct pinned source inputs.

This batch records 289 callables: 222 retain existing native/managed evidence and 67 retain explicit omissions. Another 57 declarations are class or enum metadata. The [current inventory](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/compatibility/opencv-5.1.0/upstream-map-refresh-summary.json) now contains 55 scopes and 4,051 declarations after the [public module review](opencv-5.1-public-module-mapping.md); 18 historical maps need current-version review. See the [mapping overview](opencv-5.1-source-mapping.md) and [previous contrib review](opencv-5.1-contrib-mapping.md).

| Module | Headers | Declarations | Callables | Implemented | Omitted | Metadata |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| freetype | 1 | 7 | 6 | 0 | 6 | 1 |
| alphamat | 1 | 1 | 1 | 1 | 0 | 0 |
| intensity_transform | 1 | 6 | 6 | 6 | 0 | 0 |
| plot | 1 | 21 | 20 | 20 | 0 | 1 |
| bioinspired | 4 | 36 | 32 | 32 | 0 | 4 |
| phase_unwrapping | 2 | 7 | 4 | 3 | 1 | 3 |
| hfs | 1 | 18 | 17 | 17 | 0 | 1 |
| fuzzy | 4 | 18 | 16 | 16 | 0 | 2 |
| rapid | 1 | 17 | 13 | 12 | 1 | 4 |
| shape | 5 | 66 | 55 | 20 | 35 | 11 |
| structured_light | 3 | 20 | 14 | 11 | 3 | 6 |
| saliency | 2 | 39 | 31 | 29 | 2 | 8 |
| surface_matching | 5 | 27 | 23 | 7 | 16 | 4 |
| reg | 11 | 63 | 51 | 48 | 3 | 12 |

## Reviewed boundaries

- FreeType remains wholly unwrapped. Its six callables need a FreeType/Harfbuzz dependency and font-buffer ownership contract. Core/ImgProc font APIs do not implement the contrib FreeType2 object or its memory-buffer font overload.
- AlphaMat has one callable; IntensityTransform has six and no type metadata. Exact parser identity retention does not certify arbitrary image types, trimaps or numerical domains beyond existing managed validation.
- Plot, Bioinspired, HFS and Fuzzy retain their exact bindings. HFS GPU selection and optional hardware availability require separate runtime evidence; metadata and bindings do not certify a GPU backend.
- PhaseUnwrapping's Params constructor is represented by managed value initialization and has no separate native callable. Its explicit omission remains.
- Rapid retains the GOSTracker factory omission; the current wrapper exposes Rapid and OLSTracker. No new tracker owner, ABI or behavior is introduced.
- Shape retains 35 omitted callables, including ShapeContext property/object composition, ShapeTransformer operations and thin-plate/affine transformer factories and controls. Existing distance/cost wrappers do not imply those ownership and transformation contracts.
- StructuredLight retains Params construction, findProCamMatches and the generic nested-pattern decode omissions. Other GrayCode/Sinusoidal bindings keep their exact source and transport scope.
- Saliency retains FileNode/FileStorage serialization omissions. SurfaceMatching retains 16 omissions covering mutable Pose3D/shared-pose collections, ICP in/out pose vectors, point-cloud file utilities and normal/noise/pose helpers.
- Reg retains MapTypeCaster conversions outside its typed map factory/owner contract. No unreviewed downcast or shared-handle transport is exposed.

## Validation and reproduction

The shared generator now rejects 1,753 mutations across 54 scopes. These fourteen scopes add 448 cases; DNN retains its independent 22-fixture guard. The singleton AlphaMat case tests duplicate identities with a consistent two-row count/partition, so duplicate rejection is exercised directly. Pure callable scopes test an incorrect metadata classification. Wholly omitted FreeType tests invented native/managed evidence and false promotion without assuming a working binding exists. Each case must fail for its expected reason.

Unchanged scopes still test missing/empty/incomplete reviews using a controlled documentation delta. Enum-free modules retain invented-enum rejection. Historical evidence and earlier 5.1 slices remain intact. Run scripts/Generate-UpstreamMap51.ps1 with RegenerateRaw and an explicit PythonPath, then scripts/Test-UpstreamMap51.ps1.

Independent regeneration, official Git blobs, LF/CRLF extraction, wrong-version rejection and real CLI corruption tests check source and generated artifacts separately. Every inherited native/managed evidence set is preserved. This increment changes the map tool and documentation; managed/native build inputs, public members/constants and native exports remain unchanged. Runtime, hardware, external dependency and complete C++/template coverage retain separate evidence requirements.
