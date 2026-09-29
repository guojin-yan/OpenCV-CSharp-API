param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidencePath = Join-Path $repo 'packaging/runtime/runtime-headless-candidate-evidence.json'
$schemaPath = Join-Path $repo 'packaging/runtime/runtime-headless-candidate-evidence.schema.json'
foreach ($path in @($evidencePath,$schemaPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Headless candidate evidence input is missing: $path" } }
if (-not (Test-Json -LiteralPath $evidencePath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Headless candidate evidence failed JSON Schema validation.' }
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
if ([string]$evidence.status -cne 'candidate-partial' -or [bool]$evidence.candidatePackageIdentityAllowed) { throw 'Headless candidate evidence must remain partial and non-publishable.' }
$commitType = ((& git -C $repo cat-file -t ([string]$evidence.sourceCommit) 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $commitType -cne 'commit') { throw 'Headless candidate source commit is not present in the repository.' }
& git -C $repo merge-base --is-ancestor ([string]$evidence.sourceCommit) HEAD 2>$null
if ($LASTEXITCODE -ne 0) { throw 'Headless candidate source commit is not an ancestor of HEAD.' }
if ([string]$evidence.runtimePackage.packageSha256 -cne 'c4712ed62f34f31eacaaa4c51f84ee6003bf5bab3cfcca850d39c3b812628918' -or [int]$evidence.runtimePackage.nativeDllCount -ne 7) { throw 'Headless candidate mini package identity drifted.' }
if (-not [bool]$evidence.environment.displayUnset -or -not [bool]$evidence.environment.waylandDisplayUnset -or -not [bool]$evidence.environment.ldLibraryPathUnset -or -not [bool]$evidence.environment.runtimeRootUnset) { throw 'Headless candidate environment boundary drifted.' }
foreach ($row in @($evidence.frameworks)) { if ([int]$row.total -ne 1 -or [int]$row.executed -ne 1 -or [int]$row.passed -ne 1 -or [int]$row.failed -ne 0 -or [int]$row.skipped -ne 0) { throw "Headless candidate test counters drifted for $($row.targetFramework)." } }
Write-Host "HEADLESS_CANDIDATE_EVIDENCE_OK source_commit=$($evidence.sourceCommit) profile=mini-headless-candidate frameworks=net8.0,net10.0 package_identity_allowed=false full_profile_pending=true"
