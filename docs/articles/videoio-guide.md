# VideoIO Guide

`OpenCvSharp.VideoIO` wraps OpenCV `cv::VideoCapture`, `cv::VideoWriter`, and backend-registry queries through stable C ABI calls. This package focuses on the common file/device workflow: open, read, write, get/set properties, inspect backend names, query backend availability, and build FourCC values.

`OpenCvSharp.VideoIO` 通过稳定 C ABI 调用封装 OpenCV `cv::VideoCapture`、`cv::VideoWriter` 和 backend registry 查询。该能力包聚焦常见文件/设备流程：打开、读取、写入、读写属性、查看后端名称、查询后端可用性，以及构造 FourCC 值。

## Covered APIs / 已覆盖接口

- `VideoCapture`: constructor, `Create`, `Open`, `IsOpened`, `Release`, `Grab`, `Retrieve`, `Read`, `Get`, `Set`, `GetBackendName`.
- `VideoWriter`: constructor, `Create`, `Open`, `IsOpened`, `Release`, `Write`, `Get`, `Set`, `GetBackendName`, `FourCC`.
- `VideoIORegistry`: `GetBackends`, `GetBackendName`, `HasBackend`, and `IsBackendBuiltIn`.
- Enums: `VideoCaptureAPIs`, `VideoCaptureProperties`, `VideoWriterProperties`, `VideoAccelerationType`.
- Common property wrappers: frame size, FPS, FourCC, frame count, position, brightness, contrast, saturation, hue, gain, exposure, format, mode, writer quality, writer depth, and hardware settings.

- `VideoCapture`：构造、`Create`、`Open`、`IsOpened`、`Release`、`Grab`、`Retrieve`、`Read`、`Get`、`Set`、`GetBackendName`。
- `VideoWriter`：构造、`Create`、`Open`、`IsOpened`、`Release`、`Write`、`Get`、`Set`、`GetBackendName`、`FourCC`。
- `VideoIORegistry`：`GetBackends`、`GetBackendName`、`HasBackend` 和 `IsBackendBuiltIn`。
- 枚举：`VideoCaptureAPIs`、`VideoCaptureProperties`、`VideoWriterProperties`、`VideoAccelerationType`。
- 常用属性包装：帧尺寸、FPS、FourCC、帧数、位置、亮度、对比度、饱和度、色调、增益、曝光、格式、模式、writer 质量、writer 深度和硬件设置。

## Read Frames / 读取帧

`VideoCapture.Open` returns `false` when a file or device cannot be opened. It throws only for invalid arguments, native errors, or a missing native VideoIO link.

当文件或设备无法打开时，`VideoCapture.Open` 返回 `false`。只有参数非法、native 错误或 VideoIO native 链接缺失时才会抛异常。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.VideoIO;

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

## Write Frames / 写入帧

Use `VideoWriter.FourCC` to build codec identifiers. The string overload validates that the code contains exactly four characters, and modern target frameworks also get a `ReadOnlySpan<char>` overload.

使用 `VideoWriter.FourCC` 构造编解码器标识。字符串重载会校验编码必须正好包含四个字符，现代目标框架还提供 `ReadOnlySpan<char>` 重载。

```csharp
using OpenCvSharp.Core;
using OpenCvSharp.VideoIO;

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
using OpenCvSharp.VideoIO;

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
