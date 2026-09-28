param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidenceRel = 'packaging/performance/native-aot-trim-warning-ledger.json'
$schemaRel = 'packaging/performance/native-aot-trim-warning-ledger.schema.json'
$evidencePath = Join-Path $repo ($evidenceRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($evidencePath, $schemaPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "NativeAOT trim warning ledger input is missing: $path" }
}
if (-not (Test-Json -LiteralPath $evidencePath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'NativeAOT trim warning ledger failed JSON Schema validation.' }
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
if ([string]$evidence.status -cne 'verified') { throw "NativeAOT trim warning ledger is not verified: $($evidence.status)" }
if ([string]$evidence.sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'NativeAOT trim warning ledger source commit is invalid.' }
$commitType = ((& git -C $repo cat-file -t ([string]$evidence.sourceCommit) 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $commitType -cne 'commit') { throw 'NativeAOT trim warning ledger source commit is not present in the repository.' }
& git -C $repo merge-base --is-ancestor ([string]$evidence.sourceCommit) HEAD 2>$null
if ($LASTEXITCODE -ne 0) { throw 'NativeAOT trim warning ledger source commit is not an ancestor of HEAD.' }
if ([string]$evidence.runner.architecture -cne 'x64' -or [string]$evidence.runner.configuration -cne 'Release') { throw 'NativeAOT trim warning ledger runner identity drifted.' }
if ([string]$evidence.publish.runtimeIdentifier -cne 'win-x64' -or [int]$evidence.publish.nativePayloadFileCount -ne 0) { throw 'NativeAOT trim warning ledger must remain the current managed-only win-x64 smoke.' }
if ([bool]$evidence.publish.nativeRuntimeSupplied -or [string]$evidence.publish.nativeSmoke -cne 'skipped-native-runtime-missing') { throw 'NativeAOT trim warning ledger must record the missing native runtime boundary.' }
if ([int]$evidence.warningCount -ne @($evidence.warnings).Count -or [int]$evidence.warningCount -ne 0) { throw 'NativeAOT trim warning ledger contains an unclassified warning.' }
Write-Host "NATIVE_AOT_TRIM_WARNING_LEDGER_OK source_commit=$($evidence.sourceCommit) rid=$($evidence.publish.runtimeIdentifier) warnings=$($evidence.warningCount) files=$($evidence.publish.publishedFileCount)"
