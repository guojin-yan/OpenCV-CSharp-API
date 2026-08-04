# Video Optical Flow IO Guide

OpenCV keeps `.flo` optical-flow file IO in the main `video` module, not in contrib `optflow`. The managed entry points are `JYPPX.OpenCvSharp.Video.Cv2.ReadOpticalFlow` and `JYPPX.OpenCvSharp.Video.Cv2.WriteOpticalFlow`.

OpenCV 将 `.flo` 光流文件读写放在主线 `video` 模块中，而不是 contrib `optflow`。managed 入口是 `JYPPX.OpenCvSharp.Video.Cv2.ReadOpticalFlow` 和 `JYPPX.OpenCvSharp.Video.Cv2.WriteOpticalFlow`。

```csharp
using JYPPX.OpenCvSharp.Core;
using VideoCv2 = JYPPX.OpenCvSharp.Video.Cv2;

using Mat flow = new Mat(2, 3, MatType.CV_32FC2, new Scalar(1.0, 0.5, 0.0, 0.0));
bool written = VideoCv2.WriteOpticalFlow("field.flo", flow);
using Mat loaded = VideoCv2.ReadOpticalFlow("field.flo");
```

The flow matrix should be a two-channel 32-bit float matrix, commonly `CV_32FC2`. `WriteOpticalFlow` returns OpenCV's boolean result. `ReadOpticalFlow` returns a new owned `Mat` handle.

光流矩阵应为两通道 32 位浮点矩阵，常见类型是 `CV_32FC2`。`WriteOpticalFlow` 返回 OpenCV 的布尔结果。`ReadOpticalFlow` 返回新的 owned `Mat` 句柄。

Default tests validate managed argument handling. Linked smoke creates a temporary `.flo` file, writes a tiny flow field, reads it back, and deletes the file.

默认测试验证 managed 参数处理。linked smoke 会创建临时 `.flo` 文件，写入 tiny 光流场，读取后删除该文件。
