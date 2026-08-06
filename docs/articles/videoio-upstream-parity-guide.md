# VideoIO Upstream Coverage And Workflow Guide

The checked VideoIO map measures the OpenCV 5.0.0 `opencv2/videoio.hpp` declaration slice emitted by OpenCV `hdr_parser.py`. It contains 71 declarations: 28 enums, 3 classes, and 40 callable declarations. All 40 callable declarations have reviewed stable C ABI and managed public evidence; the checked missing count is zero.

已检入的 VideoIO 映射衡量 OpenCV 5.0.0 `hdr_parser.py` 从 `opencv2/videoio.hpp` 提取的声明切片。该切片包含 71 个声明：28 个枚举、3 个类和 40 个 callable。40 个 callable 均具有经过复核的稳定 C ABI 与托管公开证据，缺失数为零。

## Checked Evidence / 已检入证据

- `compatibility/videoio-upstream-raw.json`: exact parser extraction, including `CV_VERSION_MAJOR=5`, overload metadata, and anonymous-enum value identities.
- `compatibility/videoio-upstream-classifications.json`: one reviewed classification row per raw declaration.
- `compatibility/videoio-upstream-map.txt`: deterministic upstream-to-native-to-managed evidence.
- `compatibility/videoio-implemented-families.json`: four non-overlapping families partitioning all 71 declarations.
- `compatibility/videoio-registry-surface.json`: separate source-reviewed `opencv2/videoio/registry.hpp` surface with 12 functions, 22 native entrypoints, and 12 managed members.

The map is declaration-level evidence, not a claim that C++ layouts are ABI-compatible with .NET, that every optional backend exists on every platform, or that the repository covers every OpenCV module.

该映射是声明级证据，不表示 C++ 布局与 .NET 具有 ABI 兼容性，也不表示每个平台都拥有全部可选后端，或本仓库覆盖全部 OpenCV 模块。

## Covered Families / 已覆盖家族

- Capture lifecycle and properties: file/index constructors and opens, parameter pairs, frame operations, properties, backend identity, and release.
- Stream reader and coordination: callback `Read`/`Seek`, `VideoCaptureExtensions.Open`, exception mode, and `WaitAny`.
- Writer lifecycle and parameters: both parameter overload groups, frame writes, properties, FourCC, backend identity, and release.
- Enum contract: every parser-emitted VideoIO enum and specialized anonymous constant group.

## Reproduce / 复现

Use the repository-pinned .NET SDK and the workspace OpenCV source tree:

```powershell
pwsh -NoProfile -File scripts/Generate-VideoIOUpstreamMap.ps1 `
  -RepositoryRoot . `
  -DotNetPath C:\Users\guoji\.dotnet\dotnet.exe `
  -PythonPath <python> `
  -RegenerateRaw

pwsh -NoProfile -File scripts/Test-VideoIOUpstreamMap.ps1 `
  -RepositoryRoot . `
  -DotNetPath C:\Users\guoji\.dotnet\dotnet.exe

pwsh -NoProfile -File scripts/Test-VideoIORegistrySurface.ps1 -RepositoryRoot .
```

Routine verification should omit `-RegenerateRaw`; the guard then checks the reviewed extraction, hashes, negative fixtures, classification partition, native manifest, managed baseline, and registry source hash without rewriting files.

## End-Of-Stream-Friendly Reads

`TryRead(out Mat? frame)` and `TryRetrieve(out Mat? frame, int flag = 0)` return `false` with a `null` output when no frame is available. On success, the returned matrix is independently owned and must be disposed. This keeps end-of-stream handling explicit and avoids allocating a destination `Mat` at every call site.

```csharp
using var capture = new VideoCapture(path);
while (capture.TryRead(out Mat? frame))
{
    using (frame)
    {
        Process(frame);
    }
}
```

The existing `Read(Mat)` and `Retrieve(Mat, flag)` overloads remain available for allocation-sensitive loops that reuse a destination matrix.

日常验证应省略 `-RegenerateRaw`；此时 guard 会在不重写文件的情况下检查已复核提取、哈希、负向夹具、分类分区、native manifest、托管基线和 registry 源文件哈希。

## Runtime Verification / 运行时验证

`src/OpenCvSharp.Native/tests/native_smoke.cpp` exercises the new ABI, including empty parameter arrays, exception mode, FourCC, registry lists, and callback reader read/seek. `VideoIOUpstreamParityTests` adds deterministic MJPG writer/readback, managed callback lifetime, parameter validation, and public surface checks.

`src/OpenCvSharp.Native/tests/native_smoke.cpp` 覆盖新增 ABI，包括空参数数组、异常模式、FourCC、registry 列表和回调读取器 read/seek。`VideoIOUpstreamParityTests` 进一步覆盖确定性 MJPG 写入/回读、托管回调生命周期、参数校验和公开表面检查。
