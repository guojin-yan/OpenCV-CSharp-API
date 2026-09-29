# XImgProc Upstream Map

The XImgProc map covers the 26 OpenCV contrib 5.0.0 public headers included by `opencv2/ximgproc.hpp`. The parser emits 249 declarations and the current evidence classifies 35 callable rows as implemented with both native manifest and managed baseline evidence, 177 callable rows as intentional omissions, and 37 enum/class metadata rows. There are no unexplained missing callable declarations.

Run the scoped extraction and check:

```powershell
pwsh -NoProfile -File .\scripts\Generate-XImgProcUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-XImgProcUpstreamMap.ps1
```

The shared [`upstream-map-catalog.json`](../../compatibility/upstream-map-catalog.json) invokes this guard together with the other parser-backed maps. Intentional omissions preserve exact upstream identities and reasons; they do not claim that every XImgProc algorithm, optional dependency, or model-backed operation is available in the managed API.
