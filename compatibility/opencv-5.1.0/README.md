# OpenCV 5.1.0 upstream map refresh

The historical catalog contains 73 OpenCV 5.0.0 source maps. Current-version evidence is added here after extraction with the official 5.1 parser, declaration review and semantic checks. The historical catalog remains a 5.0.0 record.

| Map | Current evidence | Scope |
| --- | --- | --- |
| DNN | [summary](dnn-upstream-summary.json), [delta](dnn-upstream-delta.json), [classifications](dnn-upstream-classifications.json) | Same three-header parser slice, 185 declarations, 22 negative fixtures |

Only DNN's recorded slice has been refreshed. The other 72 catalog maps still need 5.1 review. This record does not establish complete DNN C++ coverage, repository-wide parity or optional backend runtime support. See the [migration guide](../../docs/articles/dnn-5.1-migration.md).
