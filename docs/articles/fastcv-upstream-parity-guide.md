# FastCV upstream parity boundary

This page records a parser-backed declaration map for the pinned OpenCV contrib 5.0.0 FastCV headers. It is evidence about upstream declarations only; it does not add FastCV support to this C# package.

## Scope and upstream build boundary

The map covers `opencv2/fastcv.hpp` and its 30 directly included `opencv2/fastcv/*.hpp` headers. The pinned upstream CMake file enables the module only when `HAVE_FASTCV` is set, links the external `FASTCV_LIBRARY`, and declares dependencies on OpenCV core, geometry, imgproc, features, and video. The upstream README restricts the module to ARM and states that it cannot configure or build on x86.

The extractor records both the full non-wrapper public declaration pass and the subset emitted by `hdr_parser.py` in `wmode=True` (upstream `CV_EXPORTS_W`/`CV_WRAP` wrapper annotations). Those annotations are for OpenCV's upstream Python/Java bindings; they are not evidence of a FastCV C# wrapper in this repository.

The full pass yields 71 distinct declarations: 66 callables, four classes/structs, and one enum. The wrapper pass marks 48 declarations, including 46 callables. It normalizes OpenCV array and vector argument types, so annotations are joined by fully qualified declaration name and argument count after asserting that each join has exactly one candidate. Allocator helpers, DSP functions, `moments`, the four-argument `FCVMSER.detect` overload, and destructors remain outside that annotated subset.

## Package boundary

All scoped callable declarations remain intentionally omitted because the current package has no FastCV native ABI entrypoints or managed FastCV owner. The external Qualcomm FastCV SDK/library, ARM/DSP runtime behavior, DSP initialization, and `QcAllocator`/Mat ownership are not bundled or represented by this package. No FastCV runtime support or repository-wide upstream parity is claimed.

The generated map retains full exact signatures; the pinned parser emits zero duplicate identities. `scripts/Test-FastCVUpstreamMap.ps1` checks the parser closure, wrapper-annotation pass, CMake dependencies and gate, ARM-only boundary, zero missing rows, and absence of native/managed wrapper evidence. Twelve negative fixtures verify that altered declarations, annotations, classifications, build gates, and parity claims are rejected. The catalog and release closeout retain the five compatibility artifacts.
