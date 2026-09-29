# XPhoto Upstream Map

The XPhoto map covers the six OpenCV contrib 5.0.0 public headers under `opencv2/xphoto`: BM3D, DCT denoising, inpainting, oil painting, tone mapping, and white balance. The parser emits 47 declarations. The map retains 8 enum/class metadata rows, verifies 29 callable rows against the current native XPhoto manifest and managed baseline, and records 10 callable declarations as intentional omissions where the current wrapper surface has no matching managed member.

Run the scoped extraction and check:

```powershell
pwsh -NoProfile -File .\scripts\Generate-XPhotoUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-XPhotoUpstreamMap.ps1
```

The evidence remains scoped to the exact OpenCV contrib source snapshot and does not claim every XPhoto algorithm, model, or optional runtime profile is available. The semantic guard is also invoked by the shared 15-module upstream map catalog.
