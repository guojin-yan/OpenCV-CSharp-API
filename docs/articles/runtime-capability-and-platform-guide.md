# Runtime Capability And Platform Guide / 运行时能力与平台指南

`OpenCvCapabilities.GetCurrent()` returns a side-effect-free snapshot that can be collected before an application selects an image, video, DNN, or native-runtime path. It reports the process identity and the capabilities that OpenCV can actually enumerate; it does not open a camera, load a model, or execute an image algorithm.

`OpenCvCapabilities.GetCurrent()` 提供一个无副作用快照，可在应用选择图像、视频、DNN 或 native runtime 路径前采集。它报告进程身份以及 OpenCV 能够实际枚举的能力；不会打开摄像头、加载模型或执行图像算法。

## Read The Snapshot / 读取快照

```csharp
using System;
using System.Linq;
using JYPPX.OpenCvSharp;

OpenCvCapabilities capabilities = OpenCvCapabilities.GetCurrent();
Console.WriteLine(capabilities.OperatingSystemDescription);
Console.WriteLine(capabilities.ProcessArchitecture);
Console.WriteLine(capabilities.RuntimeIdentifier);
Console.WriteLine(capabilities.NativeRuntime);

foreach (OpenCvDnnBackendCapability backend in capabilities.DnnBackends.Where(value => value.IsAvailable))
{
    Console.WriteLine(backend.Backend + " => " + string.Join(",", backend.Targets));
}
```

`NativeRuntime.State == Verified` means that both the wrapper ABI and the OpenCV version probes matched. `VideoIOBackends` describes registry entries and built-in status; it is not proof that a camera, codec, or device can be opened. DNN `Verified` means that the backend returned a target list; a model execution probe is still required for a production claim.

`NativeRuntime.State == Verified` 表示 wrapper ABI 和 OpenCV 版本探针都匹配。`VideoIOBackends` 描述 registry 条目和 built-in 状态，不代表摄像头、编解码器或设备一定可以打开。DNN 的 `Verified` 表示 backend 返回了 target 列表；生产支持声明仍需模型执行探针。

`Accelerators` deliberately reports `opencl-tapi` and `cuda` as `Unknown` until the library owns a public, side-effect-free execution probe. A DNN target enum, a CUDA-named stitching helper, or a build flag is not GPU execution evidence. The current CPU runtime therefore remains honest about the GPU/OpenCL boundary.

`Accelerators` 会在库拥有公开、无副作用的执行探针之前，刻意将 `opencl-tapi` 和 `cuda` 报告为 `Unknown`。DNN target 枚举、CUDA 命名的 stitching helper 或构建开关都不是 GPU 执行证据。因此当前 CPU runtime 对 GPU/OpenCL 边界保持明确。

## Platform Identity

The platform fields are diagnostic inputs, not support claims:

| Field | Meaning |
| --- | --- |
| `OperatingSystemDescription` | Runtime-provided OS description; useful for evidence logs. |
| `ProcessArchitecture` | Architecture of the current .NET process, not the host kernel. |
| `RuntimeIdentifier` | SDK/runtime RID when available; it may be empty on .NET Framework. |
| `ProcessBitness` | 32 or 64 bit process size. |

Use the exact runtime package RID that matches the native artifact. Do not infer `linux-x64` from an x64 process when the package is distro-specific, and do not infer `arm64` support from a successful cross-build.

这些平台字段是诊断输入，不是支持声明：

| 字段 | 含义 |
| --- | --- |
| `OperatingSystemDescription` | runtime 提供的 OS 描述，用于证据日志。 |
| `ProcessArchitecture` | 当前 .NET 进程架构，不是 host kernel 架构。 |
| `RuntimeIdentifier` | runtime 可提供的 SDK/RID；.NET Framework 上可能为空。 |
| `ProcessBitness` | 进程位数，32 或 64。 |

应使用与 native artifact 完全匹配的 runtime package RID。不能因为进程是 x64 就推断为 `linux-x64`，也不能因为交叉编译成功就推断 ARM64 已支持。

## Domestic Linux Candidates / 国产 Linux 候选

The following platforms are candidates for a separate evidence track. None is added to the package matrix by this guide:

| Candidate | C#/.NET prerequisite | Native prerequisite | Initial decision |
| --- | --- | --- | --- |
| openEuler x86-64 / ARM64 | A supported .NET 8/10 runtime in the target userspace | Native OpenCV 5.0.0 and this wrapper built natively, with libc/toolchain recorded | Highest priority |
| KylinOS x86-64 / ARM64 | Vendor image with a supported .NET runtime and `dotnet` consumer process | Native build plus loader/dependency evidence in the exact Kylin image | High priority |
| UOS x86-64 / ARM64 | Supported .NET runtime package or a documented local installation path | Native build, package handoff, and independent consumer | High priority |
| Loongnix / LoongArch64 | A maintained .NET runtime for `loongarch64` and C# P/Invoke compatibility | LoongArch OpenCV toolchain, ABI/export audit, and a native consumer | Research only |
| RISC-V Linux distributions | A maintained .NET runtime for the exact RISC-V ABI | OpenCV and wrapper toolchain plus package/consumer proof | Research only |

The C# prerequisite is a gate, not a checkbox. A platform enters `runtime-package-matrix.json` only after all of these are independently recorded:

1. The target image/version, libc, compiler, assembler, CMake, and .NET runtime identity.
2. Native wrapper export and dependency closure for Full and Mini, where both profiles are offered.
3. Same-run package bytes and a consumer process that restores only the selected managed/runtime packages.
4. A native smoke call through the packaged loader, without producer `PATH`, `LD_LIBRARY_PATH`, or runtime-root overrides.
5. Documentation, support-contract classification, and a reproducible artifact handoff.

在以上证据齐备前，国产平台只能通过本地 native 构建和 `OpenCvNativeRuntimeDir` 验证，不应发布为正式 RID 包，也不应把 generic Linux 包当作兼容性证明。

## Local Probe Workflow / 本地验证流程

```powershell
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release `
  -p:OpenCvNativeRuntimeDir=C:\path\to\native-runtime
```

Capture the first `Runtime platform` and `Runtime capability` lines together with the native producer provenance. For a new domestic target, first validate the managed process and the loader locally; only then start the separate package and consumer evidence required for support promotion.

采集输出中的第一行 `Runtime platform` 和第二行 `Runtime capability`，并与 native producer provenance 一起保存。对于新的国产平台，应先在本地验证 managed 进程和 loader，再开始晋升支持所需的独立 package 与 consumer 证据。
