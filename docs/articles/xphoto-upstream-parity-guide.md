# XPhoto Upstream Map

The XPhoto map covers the six OpenCV contrib 5.0.0 public headers under `opencv2/xphoto`: BM3D, DCT denoising, inpainting, oil painting, tone mapping, and white balance. The parser emits 47 declarations. The map retains 8 enum/class metadata rows, verifies 30 callable rows against the current native XPhoto manifest and managed baseline, and records 9 callable declarations as intentional omissions where the current wrapper surface has no matching managed member.

`XPhotoCv2.Inpaint` exposes Shiftmap and FSR Best/Fast. Its mask must be a same-size `CV_8UC1` matrix: non-zero pixels are valid source content and zero pixels are reconstructed. Shiftmap accepts one to four source channels at `CV_8U`, `CV_8S`, `CV_16U`, `CV_16S`, `CV_32S`, `CV_32F`, or `CV_64F` depth; FSR accepts grayscale or BGR images with `CV_8U`, `CV_16U`, `CV_32F`, or `CV_64F` depth. FSR conventional pixel ranges are 0–255, 0–65535, or 0–1 respectively. Runtime XPhoto linkage remains optional.

Run the scoped extraction and check:

```powershell
pwsh -NoProfile -File .\scripts\Generate-XPhotoUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-XPhotoUpstreamMap.ps1
```

The evidence remains scoped to the exact OpenCV contrib source snapshot and does not claim every XPhoto algorithm, model, or optional runtime profile is available. The semantic guard is also invoked by the shared 15-module upstream map catalog.
