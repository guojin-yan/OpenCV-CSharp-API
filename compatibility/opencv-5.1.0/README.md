# OpenCV 5.1.0 upstream map refresh

The historical catalog contains 73 OpenCV 5.0.0 source maps. Current-version evidence is added here after extraction with the official 5.1 parser, declaration review and semantic checks. The historical catalog remains a 5.0.0 record.

| Map | Current evidence | Scope |
| --- | --- | --- |
| DNN | [summary](dnn-upstream-summary.json), [delta](dnn-upstream-delta.json), [classifications](dnn-upstream-classifications.json) | Same three-header parser slice, 185 declarations, 22 negative fixtures |
| Core | [summary](core-upstream-summary.json), [delta](core-upstream-delta.json), [classifications](core-upstream-classifications.json) | 11-header parser closure, 276 declarations, 30 negative fixtures |
| ImgProc | [summary](imgproc-upstream-summary.json), [delta](imgproc-upstream-delta.json), [classifications](imgproc-upstream-classifications.json) | imgproc.hpp parser slice, 206 declarations, 30 negative fixtures |
| Calib3D | [summary](calib3d-upstream-summary.json), [delta](calib3d-upstream-delta.json), [classifications](calib3d-upstream-classifications.json) | Four-header parser closure, 208 declarations, 37 negative fixtures; three explicit existing-binding reviews |
| ImgCodecs | [summary](imgcodecs-upstream-summary.json), [delta](imgcodecs-upstream-delta.json), [classifications](imgcodecs-upstream-classifications.json) | imgcodecs.hpp parser slice, 39 declarations, 30 negative fixtures |

The [shared catalog](upstream-map-refresh-catalog.json) and [inventory](upstream-map-refresh-summary.json) record five reviewed slices (914 declarations) and 68 pending maps. This record does not establish complete C++ coverage, repository-wide parity or optional backend/codec runtime support. See the [source mapping guide](../../docs/articles/opencv-5.1-source-mapping.md) [calibration migration guide](../../docs/articles/calibration-5.1-migration.md), and [DNN migration guide](../../docs/articles/dnn-5.1-migration.md).
