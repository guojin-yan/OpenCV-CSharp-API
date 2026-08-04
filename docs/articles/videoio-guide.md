# VideoIO Guide

`JYPPX.OpenCvSharp.VideoIO` wraps OpenCV `cv::VideoCapture`, `cv::VideoWriter`, `cv::IStreamReader`, and backend-registry queries through stable C ABI calls. This package focuses on file/device/managed-stream workflows: open, read, write, get/set properties, exception mode, wait-any coordination, inspect backend names, query backend availability, and build FourCC values.

`JYPPX.OpenCvSharp.VideoIO` 通过稳定 C ABI 调用封装 OpenCV `cv::VideoCapture`、`cv::VideoWriter` 和 backend registry 查询。该能力包聚焦常见文件/设备流程：打开、读取、写入、读写属性、查看后端名称、查询后端可用性，以及构造 FourCC 值。

## Covered APIs / 已覆盖接口

- `VideoCapture`: constructor, `Create`, file/index parameter pairs, stream extension overloads, `IsOpened`, `Release`, `Grab`, `Retrieve`, `Read`, `Get`, `Set`, `ExceptionMode`, `WaitAny`, and `GetBackendName`.
- `VideoWriter`: constructor, `Create`, parameter-pair `Open` overloads, `IsOpened`, `Release`, `Write`, `Get`, `Set`, `GetBackendName`, and `FourCC`.
- `VideoStreamReader`: callback-backed `Stream` adapter with explicit `Read` and `Seek` lifetime management.
- `VideoIORegistry`: category lists (`GetCameraBackends`, `GetStreamBackends`, `GetStreamBufferedBackends`, `GetWriterBackends`), backend names, availability, built-in status, and typed plugin-version results.
- Enums: `VideoCaptureAPIs`, `VideoCaptureProperties`, `VideoWriterProperties`, `VideoAccelerationType`.
- Common property wrappers: frame size, FPS, FourCC, frame count, position, brightness, contrast, saturation, hue, gain, exposure, format, mode, writer quality, writer depth, and hardware settings.

- `VideoCapture`：构造、`Create`、文件/索引参数对、流扩展重载、`IsOpened`、`Release`、`Grab`、`Retrieve`、`Read`、`Get`、`Set`、`ExceptionMode`、`WaitAny` 和 `GetBackendName`。
- `VideoWriter`：构造、`Create`、参数对 `Open` 重载、`IsOpened`、`Release`、`Write`、`Get`、`Set`、`GetBackendName` 和 `FourCC`。
- `VideoStreamReader`：带显式 `Read`、`Seek` 生命周期管理的回调 `Stream` 适配器。
- `VideoIORegistry`：分类列表（`GetCameraBackends`、`GetStreamBackends`、`GetStreamBufferedBackends`、`GetWriterBackends`）、后端名称、可用性、内建状态和类型化插件版本结果。
- 枚举：`VideoCaptureAPIs`、`VideoCaptureProperties`、`VideoWriterProperties`、`VideoAccelerationType`。
- 常用属性包装：帧尺寸、FPS、FourCC、帧数、位置、亮度、对比度、饱和度、色调、增益、曝光、格式、模式、writer 质量、writer 深度和硬件设置。

## Read Frames / 读取帧

`VideoCapture.Open` returns `false` when a file or device cannot be opened. It throws only for invalid arguments, native errors, or a missing native VideoIO link.

当文件或设备无法打开时，`VideoCapture.Open` 返回 `false`。只有参数非法、native 错误或 VideoIO native 链接缺失时才会抛异常。

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.VideoIO;

namespace VideoCaptureSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (VideoCapture capture = new VideoCapture())
            using (Mat frame = new Mat())
            {
                if (capture.Open("input.mp4", VideoCaptureAPIs.Any))
                {
                    bool read = capture.Read(frame);
                    System.Console.WriteLine("Read=" + read + ", size=" + frame.Size);
                    System.Console.WriteLine("Backend=" + capture.GetBackendName());
                }
            }
        }
    }
}
```

## Managed Streams And Coordination / 托管流与协调

The stream overload is supplied by `VideoCaptureExtensions`, so the existing `capture.Open(null)` string validation remains source-compatible. The adapter roots the managed stream and callback delegates until native VideoIO releases its `IStreamReader` reference.

流重载由 `VideoCaptureExtensions` 提供，因此既有的 `capture.Open(null)` 字符串校验仍保持源码兼容。适配器会一直保持托管流和回调委托，直到 native VideoIO 释放其 `IStreamReader` 引用。

```csharp
using System.IO;
using JYPPX.OpenCvSharp.VideoIO;

using (var capture = new VideoCapture())
using (var stream = File.OpenRead("input.mp4"))
{
    bool opened = capture.Open(stream, VideoCaptureAPIs.Any, leaveOpen: true,
        VideoCaptureProperties.BufferSize, 4);
    capture.ExceptionMode = true;
    if (opened && VideoCapture.WaitAny(new[] { capture }, out int[] ready, 1_000_000_000))
    {
        System.Console.WriteLine("Ready captures=" + ready.Length);
    }
}
```

Parameter arrays are key/value pairs and are validated before crossing the ABI. Backend support remains runtime-dependent; a valid parameter contract does not force a backend to accept a particular property.

参数数组必须是键/值对，并会在进入 ABI 前校验。后端支持仍取决于运行时；参数契约有效并不代表某个后端一定接受特定属性。

## Write Frames / 写入帧

Use `VideoWriter.FourCC` to build codec identifiers. The string overload validates that the code contains exactly four characters, and modern target frameworks also get a `ReadOnlySpan<char>` overload.

使用 `VideoWriter.FourCC` 构造编解码器标识。字符串重载会校验编码必须正好包含四个字符，现代目标框架还提供 `ReadOnlySpan<char>` 重载。

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.VideoIO;

namespace VideoWriterSample
{
    internal static class Program
    {
        private static void Main()
        {
            int fourcc = VideoWriter.FourCC("MJPG");

            using (VideoWriter writer = new VideoWriter())
            using (Mat frame = new Mat(240, 320, MatType.CV_8UC3, new Scalar(0, 128, 255)))
            {
                if (writer.Open("output.avi", fourcc, 30.0, frame.Size))
                {
                    writer.Write(frame);
                    System.Console.WriteLine("Backend=" + writer.GetBackendName());
                }
            }
        }
    }
}
```

## Backend Registry / 后端注册表

`VideoIORegistry` reports backend availability for the OpenCV runtime on the current machine. The result depends on build flags and discoverable backend plugins.

`VideoIORegistry` 报告当前机器上 OpenCV runtime 的后端可用性。结果取决于构建开关和可发现的后端插件。

```csharp
using JYPPX.OpenCvSharp.VideoIO;

namespace VideoIORegistrySample
{
    internal static class Program
    {
        private static void Main()
        {
            foreach (VideoCaptureAPIs backend in VideoIORegistry.GetBackends())
            {
                string name = VideoIORegistry.GetBackendName(backend);
                bool builtIn = VideoIORegistry.IsBackendBuiltIn(backend);
                System.Console.WriteLine(name + " builtIn=" + builtIn);
            }
        }
    }
}
```

## Runtime Notes / 运行时说明

Runtime packages that include VideoIO must stage `opencv_videoio500` in addition to the existing core/image modules. Actual codec support depends on how OpenCV was built and what platform backends are available, such as FFmpeg, GStreamer, Microsoft Media Foundation, DirectShow, AVFoundation, or Android MediaNDK.

包含 VideoIO 的 runtime 包除现有 core/image 模块外，还必须暂存 `opencv_videoio500`。实际编解码能力取决于 OpenCV 构建方式和平台可用后端，例如 FFmpeg、GStreamer、Microsoft Media Foundation、DirectShow、AVFoundation 或 Android MediaNDK。

The public API shape is stable across target frameworks. `net7.0+` uses source-generated P/Invoke, while older targets use `DllImport`. `VideoWriter.FourCC` is implemented in managed code so it does not require loading the native runtime.

公开 API 形状在所有目标框架上保持稳定。`net7.0+` 使用 source-generated P/Invoke，旧目标框架使用 `DllImport`。`VideoWriter.FourCC` 在 managed 层实现，因此不需要加载 native runtime。
