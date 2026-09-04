# Typed Mat View ADR

## Status

Accepted for the 5.0.1 P0 boundary. The stable public deliverable is the explicit PixelTypeDescriptor and PixelTypeTraits registry. A conditional Span-capable `MatView<TPixel>` reference preview is now implemented; it is not yet a stable cross-target contract.

## Context

Mat owns a native OpenCV allocation. Current typed APIs expose spans and row accessors after checking the element byte size; that is intentionally useful for generic binary data, but byte-size equality cannot establish an OpenCV depth/channel contract or a color and alpha meaning. A typed view must work for continuous matrices and ROIs with a stride, preserve the Mat owner lifetime, and remain honest about the older target frameworks that do not expose Span<T> APIs.

## Decision

- Pixel traits are a pure managed, immutable allow-list. Every entry records OpenCV depth, channel count, complete element size, alignment, channel-order evidence, alpha mode, and writable eligibility. Unregistered structs fail explicitly.
- Existing raw typed APIs retain their byte-size-only behavior. Tightening them to require registration would be a breaking behavior change for callers that intentionally use their own binary struct layouts.
- The built-in vector types describe storage. Their traits therefore report PixelChannelOrder.Unknown; a CV_8UC3 matrix is not automatically BGR or RGB. An adapter that owns source-format knowledge must state the color and alpha convention itself.
- A public ref struct view is rejected for a stable cross-target API: it cannot be stored, boxed, or used from older target-framework surfaces, and it would split the package contract.
- A public value struct view is rejected for the first release because default values and copies obscure owner/disposal guarantees.
- The preview is a sealed reference view that retains the Mat owner and checks its disposed state and native header identity on each memory-exposing operation. Its construction may allocate once; row and pixel operations do not allocate beyond the returned span. The view is borrowed: disposing it never disposes the Mat.

## Preconditions for a MatView preview

1. Type, depth, channels, element size, two-dimensional shape, and stride are validated when the view is created; header identity is rechecked before each operation.
2. A continuous matrix may expose one typed span. A non-continuous ROI exposes row access only and never a fabricated cross-stride span.
3. Row/column bounds, offset arithmetic, mismatched/unregistered types, owner/view disposal, and header changes have focused tests. Empty-matrix coverage remains a follow-up because native construction semantics vary by OpenCV build.
4. All package target frameworks compile, but the public view is conditionally available only on Span-capable targets. Allocation and ROI benchmarks against MatRowAccessor<T> are still required before stabilization.
5. Copy/array escape hatches are provided. N-D typed views remain unsupported rather than silently flattened. A previously obtained Span cannot be revoked; callers must keep the view and Mat alive for the span lifetime.
