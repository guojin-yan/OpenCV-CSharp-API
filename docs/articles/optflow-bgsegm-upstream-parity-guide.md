# OptFlow And BgSegm Upstream Maps

MAP-003 records two OpenCV contrib 5.0.0 public header scopes. OptFlow covers four parser-emitted headers and 85 declarations; 70 callable declarations have exact native-symbol and managed-member evidence. The single `createOptFlow_PCAFlow` factory remains intentionally omitted because the optional dense-flow family has no native factory/opaque handle, ownership/lifetime contract, or deterministic runtime smoke in the current ABI/API. BgSegm covers its parser-emitted public header with 66 declarations; 50 callable declarations have exact evidence. The eight GSOC/LSBP rows remain intentionally omitted because the current ABI/API deliberately wraps MOG, GMG, CNT, and `SyntheticSequenceGenerator` only; those parameter-rich optional families have no native factory/handle, ownership/lifetime contract, or deterministic runtime smoke. Both scopes have zero unexplained missing declarations.

Run the scoped checks:

```powershell
pwsh -NoProfile -File .\scripts\Generate-OptFlowUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-OptFlowUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Generate-BgSegmUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-BgSegmUpstreamMap.ps1
```

The shared upstream map catalog invokes both guards and binds their exact source, parser, classification, mapping, and family hashes. The generator maps each parser identity to a specific ABI entrypoint and exact managed method/property evidence; it does not use a module-level entrypoint as evidence for an unrelated row. Intentional omissions preserve upstream identities and reasons, include the optional-module build condition, and do not imply repository-wide contrib parity.
