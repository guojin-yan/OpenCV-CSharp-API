# Deterministic DNN fixtures

`identity-opset13.onnx.base64` is the existing 147-byte ONNX identity graph (SHA256 `326793cdb2fc2da739a715c3f3ff71d09779b389ad29e56bbfccc4313e900744`).

`add-zero.tflite.base64` contains a synthetic 408-byte schema-v3 TFLite model with one FLOAT32 input and output, each NHWC `[1,2,2,1]`. One ADD operator adds an all-zero constant. There are no external model files, learned weights or converter dependencies. The decoded SHA256 is `bfec1c8a625eabcb718538f14221ad821f4614e32d8e8031b3c179d3502c56c1`.

To regenerate it, compile `generate-add-zero-tflite.cpp` as C++17 with the official OpenCV 5.1 source include directories `modules/dnn/misc/tflite` and `3rdparty/flatbuffers/include`. Run the resulting executable with an output `.tflite` path, then Base64-encode its bytes with one final LF. The generator verifies the completed FlatBuffer using OpenCV's generated schema before writing it. The fixture tests independently verify its hash, model format and all four numerical outputs.
