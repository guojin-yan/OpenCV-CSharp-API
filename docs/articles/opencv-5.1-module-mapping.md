# Additional OpenCV 5.1 module mappings

This review refreshes the historical parser scopes of Features, HighGui, ML, ObjDetect, Photo, Stitching, Tracking, Video and VideoIO. The nine scopes contribute 1,300 declarations; that batch brought the inventory to 14 scopes and 2,214 declarations. The subsequent [contrib review](opencv-5.1-contrib-mapping.md) brings the current total to 22 scopes and 2,825 declarations, with 51 historical maps pending.

OpenCV is pinned to `e4f78fbcf1a773fbed69e14500e8b734963eb585`; ML and Tracking use opencv_contrib `b67a855188a3d683813731010f6d39ac614733a4`. Both are official 5.1.0 sources. Forwarding/conditional compatibility headers are pinned separately from headers that contribute parser declarations. Tracking primary/legacy and Stitching detail surfaces retain their historical source labels.

| Module | Headers | Declarations | Callables | Implemented | Omitted | Other | Added / removed / changed |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| features | 1 | 199 | 173 | 134 | 39 | 26 | 18 / 2 / 2 |
| highgui | 1 | 35 | 28 | 20 | 5 | 10 | 2 / 0 / 0 |
| ml | 1 | 241 | 208 | 208 | 0 | 33 | 0 / 0 / 0 |
| objdetect | 9 | 196 | 164 | 153 | 11 | 32 | 1 / 0 / 5 |
| photo | 3 | 145 | 120 | 120 | 0 | 25 | 0 / 0 / 0 |
| stitching | 14 | 207 | 158 | 156 | 0 | 51 | 0 / 0 / 2 |
| tracking | 2 | 35 | 21 | 21 | 0 | 14 | 0 / 0 / 0 |
| video | 2 | 168 | 145 | 138 | 7 | 23 | 0 / 0 / 0 |
| videoio | 1 | 74 | 41 | 40 | 1 | 33 | 4 / 1 / 0 |

“Other” includes type/enum metadata, conditional declarations and unsupported declarations. Zero missing means every declaration has a decision; it does not mean every callable is implemented. See the [current inventory](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/compatibility/opencv-5.1.0/upstream-map-refresh-summary.json) for per-scope files and the [mapping overview](opencv-5.1-source-mapping.md) for the earlier scopes.

## Reviewed boundaries

- Features adds XFeat factories/properties for a DNN-backed local-feature extractor. No XFeat handle, model contract or C ABI is exposed. Its class is metadata and its callables are explicitly omitted.
- LightGlue factories now accept a model type (ALIKED or DISK). New keypoint-normalization controls and image-pair context setters remain unwrapped. Upstream documents match and knnMatch with k=1, and explicitly excludes radiusMatch. Existing BF/FLANN and generic descriptor operations do not establish LightGlue support.
- HighGui makes Qt window-parameter save/load calls visible to the parser. They persist geometry, flags, trackbars, zoom and panning, and remain omitted. This review neither runs GUI actions nor certifies Qt/OpenGL/WinRT availability.
- ObjDetect adds a Dictionary.getDistanceToId overload for per-cell ratios and an explicit valid-bit threshold. Existing bit-matrix distance and threshold-based Identify are separate APIs; neither implements that new overload.
- ChArUco legacy layout affects even row counts. A mismatch may detect markers while returning zero ChArUco corners. Existing legacy-pattern controls remain available; applications must select the layout matching the printed board.
- Chessboard corner ordering can be ambiguous by 180 degrees when the interior-corner dimensions share parity, and by 90 degrees for a square pattern. Circle-grid ordering and projected-center bias also need caller care. These documentation changes do not certify a stable physical origin across hand-eye views, corrected high-accuracy centers or wider managed input domains.
- VideoIO appends open-only bitrate, CRF, encoder preset and GOP-size metadata, plus preset values and VideoDeviceInfo. No new managed enum or device-discovery transport is exposed. Raw integer options do not prove encoder support; CRF overrides bitrate and unsupported controls can fail opening in FFmpeg/GStreamer.
- ML, Photo, Tracking and Video retain their exact parser identities and properties; Stitching has two documentation changes, including LightGlue model/normalization requirements. This proves the recorded parser surface only. The separate CCM patch and previous runtime evidence remain bound to their original source/build commits.

## Verification

The shared generator independently now rejects 675 mutations across its 21 scopes; DNN retains its separate 22-fixture guard. New modules contribute 291 mutations, including source-repository identity, contrib revision, compatibility headers and primary/legacy surface loss. Unchanged scopes use a controlled documentation delta to test omitted/empty/incomplete review rejection. Every fixture must fail for its stated reason.

Generated artifacts preserve complete declarations and explicit before/after decisions. Historical 5.0 files remain unchanged. Source inputs are checked against both official Git commits, and raw extraction must match under LF and CRLF. This batch adds no public member, constant or native export and changes no managed/native runtime implementation. Optional models, backend/encoder combinations, complete C++/template coverage and other RIDs require separate evidence.
