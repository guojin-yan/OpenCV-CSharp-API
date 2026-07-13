# VideoIO Registry Guide

`OpenCvSharp.VideoIO.VideoIORegistry` exposes OpenCV `cv::videoio_registry` queries. Use it to inspect which video backends are known to the current OpenCV runtime before opening captures or writers.

`OpenCvSharp.VideoIO.VideoIORegistry` 暴露 OpenCV `cv::videoio_registry` 查询能力。它可在打开 capture 或 writer 前检查当前 OpenCV runtime 已知的视频后端。

## Covered APIs / 已覆盖接口

- `GetBackends`: returns available backend IDs as `VideoCaptureAPIs[]`.
- `GetBackendName`: returns a backend name such as `FFMPEG`, `MSMF`, or `DSHOW`.
- `HasBackend`: returns whether a backend is available.
- `IsBackendBuiltIn`: returns whether a backend is built into OpenCV rather than loaded as a plugin.

- `GetBackends`：以 `VideoCaptureAPIs[]` 返回可用后端 ID。
- `GetBackendName`：返回后端名称，例如 `FFMPEG`、`MSMF` 或 `DSHOW`。
- `HasBackend`：返回指定后端是否可用。
- `IsBackendBuiltIn`：返回指定后端是否内建于 OpenCV，而不是以插件方式加载。

## Query Backends / 查询后端

```csharp
using OpenCvSharp.VideoIO;

namespace VideoIORegistrySample
{
    internal static class Program
    {
        private static void Main()
        {
            VideoCaptureAPIs[] backends = VideoIORegistry.GetBackends();
            System.Console.WriteLine("Backend count=" + backends.Length);

            foreach (VideoCaptureAPIs backend in backends)
            {
                string name = VideoIORegistry.GetBackendName(backend);
                bool builtIn = VideoIORegistry.IsBackendBuiltIn(backend);

                System.Console.WriteLine(name + " builtIn=" + builtIn);
            }
        }
    }
}
```

The returned list reflects how OpenCV was built and what backend plugins can be discovered at runtime. A backend can be known by enum value but unavailable in a specific package or machine environment.

返回列表反映 OpenCV 的构建方式以及运行时可发现的 backend 插件。某个后端可能拥有枚举值，但在特定包或机器环境中不可用。

## Runtime Notes / 运行时说明

The registry API requires the factual OpenCV 5.0.0 runtime artifact `opencv_videoio500.dll`. Actual decoding, encoding, camera access, and writer support still depend on platform components such as FFmpeg, GStreamer, Microsoft Media Foundation, DirectShow, AVFoundation, or Android MediaNDK.

registry API 需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_videoio500.dll`。实际解码、编码、摄像头访问和写入支持仍取决于平台组件，例如 FFmpeg、GStreamer、Microsoft Media Foundation、DirectShow、AVFoundation 或 Android MediaNDK。

`GetBackendName` uses the same UTF-8 length/fill string marshalling pattern as other native wrappers. When native OpenCV is absent, the exported ABI remains present and returns `NOT_LINKED`.

`GetBackendName` 使用与其他 native wrapper 相同的 UTF-8 length/fill 字符串封送模式。当缺少 native OpenCV 时，导出的 ABI 仍保持存在，并返回 `NOT_LINKED`。
