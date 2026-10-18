# VideoStab Upstream Map

This map records the parser-backed public component-header closure for OpenCV 5.0.0 contrib VideoStab. It covers the 14 `opencv2/videostab/*.hpp` headers; the `opencv2/videostab.hpp` umbrella itself contributes no parser declarations. The checked artifacts are `compatibility/videostab-upstream-map.txt`, `compatibility/videostab-upstream-classifications.json`, and `compatibility/videostab-upstream-summary.json`.

The closure contains 278 declarations: 232 callables, 45 class metadata rows, and one enum. VideoStab headers use `CV_EXPORTS`, not `CV_WRAP`, so extraction uses non-wrapper parser mode. In temporary parser-only copies, protected sections are changed to private to keep implementation hooks outside the public map. The semantic guard explicitly locks all 22 affected protected declarations out of the checked map; pinned source headers remain unchanged.

All 232 callables are intentionally omitted. The repository has no VideoStab native ABI entrypoint or managed public owner, and the API carries caller-selected frame-source, callback, stabilization-state, and `Mat` ownership contracts. The module requires `opencv_imgproc`, `opencv_features`, `opencv_video`, `opencv_photo`, and `opencv_geometry`; CUDA feature/warping/optical-flow and VideoIO modules are optional build-time integrations. These dependencies do not imply that this package supplies VideoStab, CUDA, or VideoIO runtime support.

The guard verifies zero unexplained missing declarations, zero native/managed wrapper evidence, the parser/source scope and CMake dependency contract, and no repository-wide parity claim. This is declaration-map evidence only, not behavioral or runtime validation.
