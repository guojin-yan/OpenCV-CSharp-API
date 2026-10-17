# Ccalib upstream parity evidence

This release evidence maps the OpenCV 5.0.0 `ccalib` public-header closure emitted by the pinned Python header parser. The umbrella `opencv2/ccalib.hpp` includes `multicalib.hpp`, `omnidir.hpp`, and `randpattern.hpp`; the checked-in raw evidence retains all three parser source headers and their upstream hashes.

The closure contains four enum metadata declarations and eight `cv.omnidir` callable declarations. The current package has no native Ccalib ABI entrypoints or managed Ccalib owner, so all eight callables are explicitly classified as intentional omissions and no missing declaration is hidden. The surface also requires caller-owned calibration matrices, feature/matcher objects, pattern data, and output ownership; those contracts are not inferred as package support.

Evidence is generated with `scripts/Generate-CcalibUpstreamMap.ps1` and validated by `scripts/Test-CcalibUpstreamMap.ps1`. The map is scoped evidence for this header closure and does not claim repository-wide contrib parity.
