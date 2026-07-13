# Mat Data Access / Mat 数据访问

`Mat` exposes safe byte-copy APIs for continuous matrices and object-level helpers for clone, ROI, and reshape:

`Mat` 为连续矩阵提供安全的字节复制 API，并提供 clone、ROI 和 reshape 等对象级辅助：

```csharp
byte[] source = new byte[] { 1, 2, 3, 4 };
byte[] destination = new byte[4];

using (Mat mat = new Mat(2, 2, MatType.CV_8UC1))
{
    mat.CopyFrom(source);
    mat.CopyTo(destination);
}

using (Mat mat = Mat.Eye(new Size(3, 3), MatType.CV_8UC1))
using (Mat roi = mat.SubMat(new Rect(0, 0, 2, 2)))
using (Mat clone = mat.Clone())
{
    roi.SetTo(new Scalar(2));
    using (Mat reshaped = clone.Reshape(1, 3))
    {
        byte[] bytes = reshaped.ToBytes();
    }
}
```

## API Surface / API 表面

- `ByteLength`: total bytes required by the matrix data.
- `Clone()`: creates a deep copy with independent storage.
- `SubMat(Rect)`, `Row(int)`, `Col(int)`: create shared views.
- `Reshape(int, int)`: creates a new view when OpenCV can represent it without copying.
- `CopyFrom(byte[])`: copies a managed byte array into the matrix.
- `CopyTo(byte[])`: copies matrix bytes into a managed byte array.
- `AsByteSpan()`: available on `netcoreapp3.1` and newer for continuous matrices.
- `CopyFrom(ReadOnlySpan<byte>)`: available on `netcoreapp3.1` and newer.
- `CopyTo(Span<byte>)`: available on `netcoreapp3.1` and newer.
- `AsSpan<T>()`: typed view for unmanaged element types on `netcoreapp3.1` and newer.
- `TryGetSpan<T>()`: returns a typed span when the matrix is continuous and compatible.
- `ToArray<T>()`: copies typed values into a managed array.

- `ByteLength`：矩阵数据所需的总字节数。
- `Clone()`：创建具有独立存储的深拷贝。
- `SubMat(Rect)`、`Row(int)`、`Col(int)`：创建共享视图。
- `Reshape(int, int)`：在 OpenCV 可无拷贝表达时创建新的视图。
- `CopyFrom(byte[])`：将 managed 字节数组复制到矩阵。
- `CopyTo(byte[])`：将矩阵字节复制到 managed 字节数组。
- `AsByteSpan()`：在 `netcoreapp3.1` 及更新框架上可用，要求矩阵连续。
- `CopyFrom(ReadOnlySpan<byte>)`：在 `netcoreapp3.1` 及更新框架上可用。
- `CopyTo(Span<byte>)`：在 `netcoreapp3.1` 及更新框架上可用。
- `AsSpan<T>()`：在 `netcoreapp3.1` 及更新框架上为非托管元素类型提供类型化视图。
- `TryGetSpan<T>()`：在矩阵连续且类型兼容时返回类型化 Span。
- `ToArray<T>()`：将类型化值复制到 managed 数组。

## Ownership / 所有权

`Mat` keeps ownership of the native `cv::Mat` handle. Data-copy APIs borrow the native memory only for the duration of the call.

`Mat` 持有 native `cv::Mat` 句柄所有权。数据复制 API 只在调用期间临时借用 native 内存。

## Continuous Matrices / 连续矩阵

These APIs require `Mat.IsContinuous` to be true. Non-continuous matrices throw `OpenCvException`; use `Clone()` first if you need a contiguous copy of a view.

这些 API 要求 `Mat.IsContinuous` 为 true。非连续矩阵会抛出 `OpenCvException`；如果你需要视图的连续副本，先调用 `Clone()`。

```csharp
using (Mat source = new Mat(3, 4, MatType.CV_8UC1))
using (Mat roi = source.SubMat(new Rect(1, 1, 2, 2)))
using (Mat continuousRoi = roi.Clone())
{
    byte[] safeBytes = continuousRoi.ToBytes();
}
```

ROI, column ranges, and some reshaped matrices can share storage with a larger parent matrix. This is useful for performance, but the memory may include row gaps. Treat `AsByteSpan()`, `AsSpan<T>()`, `ToBytes()`, and typed copy APIs as continuous-matrix APIs.

ROI、列范围和部分 reshape 后的矩阵可能会与更大的父矩阵共享存储。这对性能有利，但内存中可能包含行间隙。请把 `AsByteSpan()`、`AsSpan<T>()`、`ToBytes()` 和类型化复制 API 都视为连续矩阵 API。
