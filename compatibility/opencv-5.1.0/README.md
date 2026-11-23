# OpenCV 5.1.0 upstream map refresh

The historical catalog contains 73 OpenCV 5.0.0 source maps. Current-version evidence is added here after extraction with the official 5.1 parser, declaration review and semantic checks. The historical catalog remains a 5.0.0 record.

| Map | Current evidence | Scope |
| --- | --- | --- |
| DNN | [summary](dnn-upstream-summary.json), [delta](dnn-upstream-delta.json), [classifications](dnn-upstream-classifications.json) | Same three-header parser slice, 185 declarations, 22 negative fixtures |
| Core | [summary](core-upstream-summary.json), [delta](core-upstream-delta.json), [classifications](core-upstream-classifications.json) | 11-header parser closure, 276 declarations, 30 negative fixtures |
| ImgProc | [summary](imgproc-upstream-summary.json), [delta](imgproc-upstream-delta.json), [classifications](imgproc-upstream-classifications.json) | imgproc.hpp parser slice, 206 declarations, 30 negative fixtures |
| Calib3D | [summary](calib3d-upstream-summary.json), [delta](calib3d-upstream-delta.json), [classifications](calib3d-upstream-classifications.json) | Four-header parser closure, 208 declarations, 37 negative fixtures; three explicit existing-binding reviews |
| ImgCodecs | [summary](imgcodecs-upstream-summary.json), [delta](imgcodecs-upstream-delta.json), [classifications](imgcodecs-upstream-classifications.json) | imgcodecs.hpp parser slice, 39 declarations, 30 negative fixtures |
| features | [summary](features-upstream-summary.json), [delta](features-upstream-delta.json), [classifications](features-upstream-classifications.json) | 1 contributing headers, 199 declarations, 32 negative fixtures |
| highgui | [summary](highgui-upstream-summary.json), [delta](highgui-upstream-delta.json), [classifications](highgui-upstream-classifications.json) | 1 contributing headers, 35 declarations, 32 negative fixtures |
| ml | [summary](ml-upstream-summary.json), [delta](ml-upstream-delta.json), [classifications](ml-upstream-classifications.json) | 1 contributing headers, 241 declarations, 33 negative fixtures |
| objdetect | [summary](objdetect-upstream-summary.json), [delta](objdetect-upstream-delta.json), [classifications](objdetect-upstream-classifications.json) | 9 contributing headers, 196 declarations, 32 negative fixtures |
| photo | [summary](photo-upstream-summary.json), [delta](photo-upstream-delta.json), [classifications](photo-upstream-classifications.json) | 3 contributing headers, 145 declarations, 32 negative fixtures |
| stitching | [summary](stitching-upstream-summary.json), [delta](stitching-upstream-delta.json), [classifications](stitching-upstream-classifications.json) | 14 contributing headers, 207 declarations, 33 negative fixtures |
| tracking | [summary](tracking-upstream-summary.json), [delta](tracking-upstream-delta.json), [classifications](tracking-upstream-classifications.json) | 2 contributing headers, 35 declarations, 34 negative fixtures |
| video | [summary](video-upstream-summary.json), [delta](video-upstream-delta.json), [classifications](video-upstream-classifications.json) | 2 contributing headers, 168 declarations, 32 negative fixtures |
| videoio | [summary](videoio-upstream-summary.json), [delta](videoio-upstream-delta.json), [classifications](videoio-upstream-classifications.json) | 1 contributing headers, 74 declarations, 31 negative fixtures |

The [shared catalog](upstream-map-refresh-catalog.json) and [inventory](upstream-map-refresh-summary.json) record 14 reviewed scopes (2,214 declarations) and 59 pending maps. This record does not establish complete C++ coverage, repository-wide parity or optional backend/codec runtime support. See the [source mapping guide](../../docs/articles/opencv-5.1-source-mapping.md), [calibration migration guide](../../docs/articles/calibration-5.1-migration.md), and [DNN migration guide](../../docs/articles/dnn-5.1-migration.md).

The [additional module review](../../docs/articles/opencv-5.1-module-mapping.md) explains the nine new scopes, fixed contrib revision, compatibility headers, new explicit omissions and unchanged-surface limits.
