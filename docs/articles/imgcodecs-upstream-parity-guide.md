# ImgCodecs Upstream Coverage And Workflow Guide

This guide covers the exact OpenCV `5.0.0` declarations emitted from `opencv2/imgcodecs.hpp` and the separately reviewed `ImageCollection` source extension. It is not a repository-wide OpenCV parity claim, and enum availability does not imply that every optional codec is present in a particular runtime build.

## Measured Contract

OpenCV's `hdr_parser.py` emits 39 declarations: 16 enums, one `Animation` class declaration, and 22 callable declarations. All 22 callable identities are implemented; none are missing or omitted. The source-order map and evidence are:

- `compatibility/imgcodecs-upstream-raw.json`
- `compatibility/imgcodecs-upstream-classifications.json`
- `compatibility/imgcodecs-upstream-map.txt`
- `compatibility/imgcodecs-upstream-summary.json`
- `compatibility/imgcodecs-implemented-families.json`
- `compatibility/imgcodecs-source-reviewed-extensions.json`

`ImageCollection` is linkable in OpenCV 5.0.0 but is not marked `CV_WRAP`, so it is deliberately excluded from the parser-derived 39-declaration count and recorded as a source-reviewed extension.

Regenerate the raw extraction only with an explicit Python runtime:

```powershell
pwsh -NoProfile -File ./scripts/Generate-ImgCodecsUpstreamMap.ps1 `
  -RegenerateRaw `
  -PythonPath C:\path\to\python.exe `
  -InitializeClassification
```

Normal checks do not require Python:

```powershell
pwsh -NoProfile -File ./scripts/Test-ImgCodecsUpstreamMap.ps1
```

## Multi-Page Images

`ImReadMulti` and `ImDecodeMulti` return arrays of independently owned `Mat` clones. Dispose every returned matrix. The ranged file overload uses `start, count`; the memory overload uses the half-open range `[start, end)`. `ImWriteMulti` and `ImEncodeMulti` require at least one non-null image and encoder parameters in key/value pairs.

```csharp
using Mat first = new Mat(32, 32, MatType.CV_8UC3, new Scalar(20, 40, 60));
using Mat second = new Mat(32, 32, MatType.CV_8UC3, new Scalar(80, 100, 120));

byte[] tiff = Cv2.ImEncodeMulti(".tiff", new[] { first, second });
Cv2.ImDecodeMulti(tiff, 1, 2, out Mat[] pages, ImreadModes.Unchanged);
try
{
    Console.WriteLine(pages[0].Rows);
}
finally
{
    foreach (Mat page in pages) page.Dispose();
}
```

## Metadata

`ImageMetadataResult` owns its decoded image and every returned metadata `Mat`; disposing the result releases all of them. Input `ImageMetadataChunk` objects remain caller-owned. Metadata type and payload counts cannot diverge because they are represented as paired objects.

```csharp
using Mat exif = new Mat(1, exifBytes.Length, MatType.CV_8UC1);
exif.CopyFrom(exifBytes);
byte[] jpeg = Cv2.ImEncodeWithMetadata(
    ".jpeg",
    image,
    new[] { new ImageMetadataChunk(ImageMetadataType.Exif, exif) });

using ImageMetadataResult decoded = Cv2.ImDecodeWithMetadata(jpeg);
Console.WriteLine(decoded.Metadata[0].Type);
```

Formats decide which metadata types they accept. Unsupported or malformed chunks surface as an `OpenCvException`; an enum member alone is not codec evidence.

## Animation

`Animation` owns a native animation through `SafeHandle`. `SetFrames` deep-copies every input frame and duration. `GetFrame` and `StillImage` return independent clones that the caller disposes. Negative ranges and mismatched frame/duration counts fail before native invocation.

```csharp
using Animation animation = new Animation(loopCount: 2, backgroundColor: new Scalar(0, 0, 0, 0));
animation.SetFrames(new[] { first, second }, new[] { 40, 80 });
byte[] gif = Cv2.ImEncodeAnimation(".gif", animation);

using Animation decoded = new Animation();
Cv2.ImDecodeAnimation(gif, decoded);
using AnimationFrame frame = decoded.GetFrame(1);
Console.WriteLine(frame.DurationMilliseconds);
```

GIF durations are constrained by the GIF format and OpenCV rounds them to 10 ms units. Animation frames must use dimensions and types accepted by the selected encoder.

## Lazy Collection And Paths

`ImageCollection` provides indexed access without exposing C++ iterators. Its indexer returns an independent `Mat` clone that remains valid after `ReleaseCache`, reinitialization, or collection disposal. Non-ASCII collection paths are copied to a collection-lifetime temporary file because OpenCV's class accepts only a narrow `String`; the wrapper removes the copy during reinitialization or disposal.

All other file workflows read or write through `std::filesystem::u8path` and memory codecs. Reader/writer probes report the exact linked runtime, so use them when optional codec availability matters.

## Failure And Ownership Summary

- Invalid managed ranges, empty buffers, null images, and odd encoder parameter counts throw managed argument exceptions.
- OpenCV false/empty results are represented by the documented boolean result or an `OpenCvException`, depending on whether the existing API promises a usable image object.
- No STL container crosses the C ABI. Native vector and metadata handles are transient and copied into owned managed objects.
- `Animation`, `ImageCollection`, `ImageMetadataResult`, `AnimationFrame`, and every returned `Mat` must be disposed.
