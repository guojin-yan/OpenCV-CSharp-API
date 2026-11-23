# OpenCV 5.1 public module mappings

Current totals after the [backend and dependency review](opencv-5.1-backend-mapping.md): 73 parser scopes, 5,778 declarations, zero historical parser maps pending, and 2,420 shared-generator negative fixtures. The batch-specific counts below describe the earlier review.

Nineteen historical parser scopes are reviewed against official OpenCV 5.1.0 commit e4f78fbcf1a773fbed69e14500e8b734963eb585 and opencv_contrib commit b67a855188a3d683813731010f6d39ac614733a4. PtCloud belongs to OpenCV; the other eighteen scopes belong to contrib. The 82 contributing headers, primary umbrellas, build files and one excluded umbrella add 94 distinct pinned inputs. All 879 retained identities preserve their declaration fields, including documentation. RGBD replaces one callable signature, reviewed below.

This batch contains 880 declarations: 681 callables (47 existing bindings and 634 intentional omissions) and 199 class/enum metadata declarations. At the end of this batch the inventory contained 55 scopes and 4,051 declarations; the final 18 scopes are now refreshed. See the [mapping overview](opencv-5.1-source-mapping.md), [previous image and geometry review](opencv-5.1-image-geometry-mapping.md) and [versioned inventory](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/compatibility/opencv-5.1.0/upstream-map-refresh-summary.json).

| Module | Headers | Declarations | Callables | Implemented | Omitted | Metadata |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| xstereo | 2 | 14 | 10 | 8 | 2 | 4 |
| ptcloud | 1 | 22 | 19 | 19 | 0 | 3 |
| xobjdetect | 1 | 34 | 27 | 20 | 7 | 7 |
| wechat_qrcode | 1 | 5 | 4 | 0 | 4 | 1 |
| dnn_superres | 1 | 10 | 9 | 0 | 9 | 1 |
| cnn_3dobj | 1 | 7 | 5 | 0 | 5 | 2 |
| signal | 1 | 1 | 1 | 0 | 1 | 0 |
| dpm | 1 | 2 | 0 | 0 | 0 | 2 |
| hdf | 1 | 38 | 37 | 0 | 37 | 1 |
| text | 4 | 47 | 30 | 0 | 30 | 17 |
| xfeatures2d | 2 | 21 | 18 | 0 | 18 | 3 |
| dnn_objdetect | 1 | 4 | 2 | 0 | 2 | 2 |
| ccalib | 3 | 12 | 8 | 0 | 8 | 4 |
| superres | 2 | 96 | 89 | 0 | 89 | 7 |
| julia | 1 | 1 | 1 | 0 | 1 | 0 |
| rgbd | 6 | 105 | 88 | 0 | 88 | 17 |
| videostab | 14 | 278 | 232 | 0 | 232 | 46 |
| cvv | 7 | 20 | 18 | 0 | 18 | 2 |
| datasets | 32 | 163 | 83 | 0 | 83 | 80 |

## Source and parser decisions

OpenCV contrib 5.1 replaces ColoredKinFu::getCloud(points, normals) with getCloud(points, normals, colors = noArray()). The parser represents the optional output as Mat colors=Mat()[/O]. Both signatures are explicitly recorded in the delta. The new callable remains omitted: the package has no RGBD/ColoredKinFu native wrapper or managed owner for reconstruction state, sensor inputs and output buffers. No existing binding is inferred from the old ordinal.

CVV's 5.1 CMake gate drops NOT HAVE_CXX11 and retains NOT HAVE_QT OR QT_VERSION_MAJOR LESS 5. Its recorded condition changes from HAVE_QT_GE_5_AND_CXX11 to HAVE_QT_GE_5, with an explicit review on each retained classification. This records the module gate; interactive Qt support remains unwrapped.

The extractor retains each historical parser strategy. DnnObjDetect, Superres, VideoStab, CVV and Datasets use non-wrapper mode because their public declarations lack a complete CV_WRAP closure. CVV's mode is now explicit in the catalog and raw snapshot. Temporary parser copies name DnnObjDetect's anonymous object struct, annotate six field declarations (seven object members plus InferBbox::detections), and hide protected methods. The historical normalization description is clarified without changing the parsed data shape. Superres excludes exactly its three protected SuperResolution constructor/initImpl/processImpl hooks. VideoStab hides protected sections in temporary copies and deduplicates repeated parser identities in header order. Upstream files and their hashes remain unchanged by normalization.

The raw snapshots, summaries and guards retain parser mode, normalization, deduplication, build conditions, compatibility files and excluded headers. The source initializer also fetches and verifies the excluded PtCloud umbrella. That umbrella gained point-cloud/mesh APIs and SLAM/visualization includes in 5.1; these remain outside the single depth.hpp slice. No full PtCloud or full C++ coverage is claimed.

## Binding and omission boundaries

- XStereo retains eight bindings and two MatchQuasiDense value-construction/comparison omissions; count/fill record transport remains its existing contract. PtCloud's depth slice retains nineteen bindings. XObjDetect retains twenty bindings and seven explicit omissions.
- WeChatQRCode and DnnSuperres remain model-dependent omissions. Cnn3DObj, Signal and Julia remain unwrapped; external models, dependencies, callback/runtime behavior and ownership need separate implementation and evidence.
- DPM emits only two class metadata declarations and zero callables. Its parser snapshot is not an inventory of every public C++ method and does not establish callable parity.
- HDF retains its HDF5 dependency and dataset/attribute/type ownership boundaries. Text retains scene-text/OCR model and optional Tesseract boundaries. XFeatures2D retains optional CUDA/nonfree, build-policy and ownership boundaries. Ccalib retains omnidirectional calibration inputs, feature/matcher objects and output-buffer omissions.
- Superres and RGBD retain their state, frame-source, reconstruction and output ownership omissions. VideoStab retains frame-source callbacks, stabilization state and optional CUDA/VideoIO boundaries. CVV remains a Qt GUI omission. Datasets remains dependent on caller-provided files, directories, annotations and dataset object ownership.

## Validation and reproduction

The shared generator rejects 1,753 mutations across 54 scopes, including 630 added by this batch. DNN retains its separate 22-fixture guard. DPM exercises metadata identity/qualifier/enum corruption, metadata falsely classified as a callable omission, false native/managed evidence and an unreviewed metadata addition without assuming a callable exists. Guards also reject lost parser strategies, exclusions and module conditions, incomplete CVV build reviews and unreviewed RGBD replacement decisions.

Run scripts/Generate-UpstreamMap51.ps1 with RegenerateRaw and an explicit PythonPath, then scripts/Test-UpstreamMap51.ps1. Independent regeneration, official Git blobs, LF/CRLF extraction, wrong-version rejection and actual CLI corruption checks verify generated evidence separately. Every retained native/managed evidence set is preserved. This increment changes mapping tools, evidence and documentation; it adds no managed API or C ABI export. Runtime, external dependency, hardware and complete C++/template coverage retain separate evidence requirements.
