param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidenceRel = 'packaging/performance/typed-mat-native-lifetime-evidence.json'
$schemaRel = 'packaging/performance/typed-mat-native-lifetime-evidence.schema.json'
$evidencePath = Join-Path $repo ($evidenceRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($evidencePath, $schemaPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Typed Mat lifetime evidence file is missing: $path" } }
if (-not (Test-Json -LiteralPath $evidencePath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Typed Mat lifetime evidence failed JSON Schema validation.' }
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
if ([string]$evidence.status -cne 'verified' -or [string]$evidence.sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Typed Mat lifetime evidence identity is invalid.' }
$commitType = ((& git -C $repo cat-file -t ([string]$evidence.sourceCommit) 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $commitType -cne 'commit') { throw 'Typed Mat lifetime source commit is not present in the repository.' }
& git -C $repo merge-base --is-ancestor ([string]$evidence.sourceCommit) HEAD 2>$null
if ($LASTEXITCODE -ne 0) { throw 'Typed Mat lifetime source commit is not an ancestor of HEAD.' }
if ([string]$evidence.runner.architecture -cne 'x64' -or [string]$evidence.runner.configuration -cne 'Release') { throw 'Typed Mat lifetime runner identity drifted.' }
if ([string]$evidence.runner.nativeRuntimePackageSha256 -notmatch '^[0-9a-f]{64}$') { throw 'Typed Mat lifetime runtime package hash is malformed.' }
$payload = @($evidence.runner.nativePayload)
if ($payload.Count -ne 18) { throw 'Typed Mat lifetime evidence must identify all 18 native runtime DLLs.' }
$names = @($payload | ForEach-Object { [string]$_.name })
if (@($names | Select-Object -Unique).Count -ne 18 -or $names -notcontains 'JYPPX.OpenCV.Native.dll' -or $names -notcontains 'opencv_core500.dll' -or $names -notcontains 'opencv_imgproc500.dll' -or $names -notcontains 'opencv_imgcodecs500.dll') { throw 'Typed Mat lifetime native payload identity is incomplete or duplicated.' }
if ([int]$evidence.runtime.nativeModuleFiles -ne 17) { throw 'Typed Mat lifetime runtime module count drifted.' }
foreach ($hash in @($evidence.runtime.wrapperLoaderSha256,$evidence.runtime.opencvCoreSha256,$evidence.runtime.opencvImgProcSha256,$evidence.runtime.opencvImgCodecsSha256)) { if ([string]$hash -notmatch '^[0-9a-f]{64}$') { throw 'Typed Mat lifetime runtime module hash is malformed.' } }
if ([string]$evidence.test.class -cne 'JYPPX.OpenCvSharp.Tests.Core.MatViewTests' -or [bool]$evidence.test.nativeSmokeEnabled -ne $true) { throw 'Typed Mat lifetime test identity drifted.' }
$rows = @($evidence.test.targetFrameworks | Sort-Object framework)
$frameworkSet = (@($rows | ForEach-Object { [string]$_.framework }) -join '|')
if ($frameworkSet -cne 'net10.0|net8.0') { throw 'Typed Mat lifetime framework set drifted.' }
foreach ($row in $rows) {
    foreach ($field in @('total','executed','passed')) { if ([int]$row.$field -ne 9) { throw "Typed Mat lifetime pass count drifted for $($row.framework): $field=$($row.$field)" } }
    foreach ($field in @('failed','skipped','error','timeout','aborted')) { if ([int]$row.$field -ne 0) { throw "Typed Mat lifetime negative count for $($row.framework): $field=$($row.$field)" } }
    foreach ($field in @('testAssemblySha256','apiAssemblySha256')) { if ([string]$row.$field -notmatch '^[0-9a-f]{64}$') { throw "Typed Mat lifetime assembly hash is malformed for $($row.framework): $field" } }
}
if (@($evidence.test.verifiedCases).Count -lt 7) { throw 'Typed Mat lifetime case inventory is incomplete.' }
Write-Host "TYPED_MAT_NATIVE_LIFETIME_EVIDENCE_OK source_commit=$($evidence.sourceCommit) native_payload=$($payload.Count) frameworks=net8.0,net10.0 passed=18"
