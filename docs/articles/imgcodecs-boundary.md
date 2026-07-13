# imgcodecs Boundary

`imgcodecs` is the first OpenCV module in this project that returns a variable-length native buffer.

`imgcodecs` 是本项目中第一个会返回可变长度 native 缓冲区的 OpenCV 模块。

## Current Scope

The first boundary contains:

- `imencode`
- `imdecode`
- `imread`
- `imwrite`
- `ImreadModes.Unchanged`
- `ImreadModes.Grayscale`
- `ImreadModes.Color`
- `ImwriteFlags.JpegQuality`
- `ImwriteFlags.PngCompression`
- `ImwriteFlags.WebPQuality`
- `ImwritePngStrategy`
- `ImwritePngFilterFlags`
- `ImwriteWebPLosslessMode`

The managed API is exposed under:

```csharp
OpenCvSharp.ImgCodecs.Cv2
```

## ABI Design

The native C ABI does not expose C++ STL types. `cv::imencode` writes to `std::vector<uchar>`, but that vector stays inside the native layer.

Native code returns an opaque `jyppx_ocv_encoded_buffer*` handle instead:

- `jyppx_ocv_imgcodecs_imencode`
- `jyppx_ocv_imgcodecs_imencode_with_params`
- `jyppx_ocv_imgcodecs_imread`
- `jyppx_ocv_imgcodecs_imwrite`
- `jyppx_ocv_imgcodecs_imwrite_with_params`
- `jyppx_ocv_encoded_buffer_size`
- `jyppx_ocv_encoded_buffer_data`
- `jyppx_ocv_encoded_buffer_release`

Managed code copies the encoded bytes into a `byte[]` and releases the native buffer through a `SafeHandle`.

`ImEncode(string, Mat, int[])` accepts key-value encoder parameters. The array length must be even.

## Decode Path

`ImDecode(byte[])` is available on all supported target frameworks.

`ImDecode(ReadOnlySpan<byte>)` is available on modern .NET targets. It pins the span and passes the pointer to native code directly, avoiding an extra managed array allocation.

The native layer wraps the incoming encoded memory as a temporary `cv::Mat` view before calling `cv::imdecode`, avoiding an extra native `std::vector` copy.

`ImRead` and `ImWrite` use UTF-8 null-terminated filenames on the managed side so the same API shape works across platforms.

## File API Error Model

`ImRead` throws `OpenCvException` when the file cannot be read or decoded.

`ImRead` 在文件无法读取或无法解码时抛出 `OpenCvException`。

`ImWrite` returns `false` when the output file cannot be opened or written. OpenCV/native exceptions are still translated to `OpenCvException`.

`ImWrite` 在输出文件无法打开或无法写入时返回 `false`。OpenCV/native 异常仍会转换为 `OpenCvException`。

## Future Work

Later stages should add:

- More image codec parameter enums from OpenCV.
- Metadata and animation APIs only after the basic still-image path is stable.
