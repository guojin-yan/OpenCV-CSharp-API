# OptFlow And BgSegm Upstream Maps

MAP-003 records two OpenCV contrib 5.0.0 public header scopes. OptFlow covers four parser-emitted headers and 85 declarations; 14 callable declarations have explicit native and managed evidence while 57 callable declarations remain intentional omissions. BgSegm covers its parser-emitted public header with 66 declarations; 6 callable declarations have explicit evidence while 52 remain intentional omissions. Both scopes have zero unexplained missing declarations.

Run the scoped checks:

```powershell
pwsh -NoProfile -File .\scripts\Generate-OptFlowUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-OptFlowUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Generate-BgSegmUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-BgSegmUpstreamMap.ps1
```

The shared upstream map catalog invokes both guards and binds their exact source, parser, classification, mapping, and family hashes. Intentional omissions preserve upstream identities and reasons; they do not imply repository-wide contrib parity.
