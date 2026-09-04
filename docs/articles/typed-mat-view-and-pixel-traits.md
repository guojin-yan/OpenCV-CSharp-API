# Typed Mat Views and Pixel Traits

## Current 5.0.1 boundary

5.0.1 adds the managed PixelTypeDescriptor and PixelTypeTraits registry as the first stage of the typed-matrix-view plan. The registry is an explicit allow-list for the scalar types and published Vec2*, Vec3*, and Vec4* types already present in the package. It records the OpenCV depth, channel count, complete element size, alignment, channel-order evidence, alpha mode, and writable-view eligibility without loading the native library.

The registry does not change the existing Mat.AsSpan<T>, AsRowSpan<T>, AsRows<T>, GetValue<T>, or SetValue<T> contracts. Those APIs continue to validate the native element size and are useful for generic binary views. On Span-capable target frameworks, the opt-in `MatView<TPixel>` preview uses the registry for exact depth/channel checks, two-dimensional shape, stride, and owner/header lifetime validation. It is a borrowed view: disposing it does not dispose the Mat. The design record is available in [Typed Mat View ADR](typed-mat-view-adr.md).

## Channel semantics

An OpenCV depth/channel encoding describes storage, not the meaning of application data. A CV_8UC3 matrix may contain BGR, RGB, a three-component feature vector, or another layout. For that reason the built-in traits deliberately report PixelChannelOrder.Unknown for existing C# scalar and vector storage types. Adapters that know the source convention must make the BGR/RGB/alpha decision explicitly; a C# type name is not evidence of color order.

## Usage

~~~csharp
PixelTypeDescriptor descriptor = PixelTypeDescriptor.Get<Vec3b>();
if (!descriptor.MatchesMatType(mat.Type))
{
    throw new InvalidOperationException("The matrix is not a three-component byte matrix.");
}

Span<Vec3b> row = mat.AsRowSpan<Vec3b>(0);
~~~

For the stronger preview contract, create a view from the matrix. Continuous matrices expose one flat typed span; non-continuous ROIs expose row spans only:

~~~csharp
using MatView<Vec3b> view = mat.AsView<Vec3b>();
Span<Vec3b> firstRow = view.AsRowSpan(0);
firstRow[0] = new Vec3b(10, 20, 30);

if (view.TryGetSpan(out Span<Vec3b> pixels))
{
    // The matrix is continuous; pixels.Length == view.Rows * view.Columns.
}
~~~

`MatView<TPixel>` rejects unregistered types, mismatched depth/channel encodings, and N-D matrices. A span returned by the view is borrowed native memory: do not use it after disposing the view or its Mat, and do not retain it across `Mat.Create` or another native header-changing operation. Use `ToArray`, `CopyTo`, or `CopyFrom` when a managed lifetime is required.

`Clone()` returns an owning deep copy of the viewed matrix or ROI, while `CopyTo(Mat)` reuses the existing destination-matrix copy contract. Both operations revalidate the borrowed header before entering native code; a disposed or changed owner is rejected.

`Clone()` 返回当前矩阵或 ROI 的独立深拷贝；`CopyTo(Mat)` 复用现有目标矩阵复制契约。两者在进入 native 代码前都会重新验证 borrowed header；owner 已释放或发生变化时会拒绝访问。

Unknown unmanaged structs are rejected by PixelTypeDescriptor.Get<T>(); use the existing raw byte/typed span APIs only when the binary layout is intentionally managed by the caller.

## Cross-target behavior

The descriptor and registry are pure managed code and compile for all package target frameworks, including .NET Framework. `MatView<TPixel>` and its Span-returning members are compiled only for `NETCOREAPP3_1_OR_GREATER`; older target frameworks continue to use the existing byte/typed array APIs. Neither layer depends on GPU/OpenCL or a platform-specific image backend.
