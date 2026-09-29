param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidenceRel = 'packaging/performance/codec-writer-allocation-evidence.json'
$schemaRel = 'packaging/performance/codec-writer-allocation-evidence.schema.json'
$evidencePath = Join-Path $repo ($evidenceRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($evidencePath, $schemaPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Codec writer allocation evidence file is missing: $path" } }
if (-not (Test-Json -LiteralPath $evidencePath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Codec writer allocation evidence failed JSON Schema validation.' }
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json

if ([string]$evidence.status -cne 'verified' -or [string]$evidence.sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Codec writer allocation evidence identity is invalid.' }
$commitType = ((& git -C $repo cat-file -t ([string]$evidence.sourceCommit) 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $commitType -cne 'commit') { throw 'Codec writer allocation source commit is not present in the repository.' }
& git -C $repo merge-base --is-ancestor ([string]$evidence.sourceCommit) HEAD 2>$null
if ($LASTEXITCODE -ne 0) { throw 'Codec writer allocation source commit is not an ancestor of HEAD.' }
if ([string]$evidence.runner.architecture -cne 'x64' -or [string]$evidence.runner.configuration -cne 'Release' -or [string]$evidence.runner.dotnetSdk -notmatch '^10\.0\.') { throw 'Codec writer allocation runner identity drifted.' }
if ([string]$evidence.test.class -cne 'JYPPX.OpenCvSharp.Tests.ImgCodecs.CodecWriterAllocationTests' -or [bool]$evidence.test.nativeSmokeEnabled -ne $true) { throw 'Codec writer allocation test identity drifted.' }

$expectedPackageHashes = @{
    full = '07de65af0ec14420d84d49caa4663d08393880a1bae402c789177d2dfff2544e'
    mini = 'c4712ed62f34f31eacaaa4c51f84ee6003bf5bab3cfcca850d39c3b812628918'
}
$expectedPackageNames = @{
    full = 'jyppx.opencv.runtime.win-x64.5.0.0.nupkg'
    mini = 'jyppx.opencv.runtime.win-x64.mini.5.0.0.nupkg'
}
$packageRows = @($evidence.runtimePackages)
if ($packageRows.Count -ne 2) { throw 'Codec writer allocation evidence must contain exactly full and mini package rows.' }
foreach ($profile in @('full', 'mini')) {
    $rows = @($packageRows | Where-Object { [string]$_.profile -ceq $profile })
    if ($rows.Count -ne 1) { throw "Codec writer allocation package row is not unique for $profile." }
    $package = $rows[0].package
    if ([string]$package.packageFileName -cne $expectedPackageNames[$profile] -or [string]$package.packageSha256 -cne $expectedPackageHashes[$profile]) { throw "Codec writer allocation package identity drifted for $profile." }
    if (@($package.payload).Count -lt 1) { throw "Codec writer allocation package payload is empty for $profile." }
    if (@($package.payload | Group-Object name | Where-Object Count -ne 1).Count -ne 0) { throw "Codec writer allocation package payload has duplicate names for $profile." }
    foreach ($row in @($package.payload)) { if ([int64]$row.bytes -le 0 -or [string]$row.sha256 -notmatch '^[0-9a-f]{64}$') { throw "Codec writer allocation package payload row is invalid for $profile/$($row.name)." } }
}

$testRows = @($evidence.test.targetFrameworks)
if ($testRows.Count -ne 4) { throw "Expected four codec writer allocation test rows; found $($testRows.Count)." }
$expectedKeys = @('full/net8.0', 'full/net10.0', 'mini/net8.0', 'mini/net10.0')
$actualKeys = @($testRows | ForEach-Object { "$($_.profile)/$($_.targetFramework)" } | Sort-Object)
if (($actualKeys -join '|') -cne (@($expectedKeys | Sort-Object) -join '|')) { throw "Codec writer allocation test matrix drifted: $($actualKeys -join ',')." }
foreach ($row in $testRows) {
    foreach ($field in @('total', 'executed', 'passed')) { if ([int]$row.$field -ne 2) { throw "Codec writer allocation pass count drifted for $($row.profile)/$($row.targetFramework): $field=$($row.$field)." } }
    foreach ($field in @('failed', 'skipped', 'error', 'timeout', 'aborted')) { if ([int]$row.$field -ne 0) { throw "Codec writer allocation negative count for $($row.profile)/$($row.targetFramework): $field=$($row.$field)." } }
    if ([bool]$row.nativeSmokeEnabled -ne $true) { throw "Native smoke was not enabled for $($row.profile)/$($row.targetFramework)." }
}
if (@($evidence.test.verifiedCases).Count -lt 2) { throw 'Codec writer allocation verified case inventory is incomplete.' }
Write-Host "CODEC_WRITER_ALLOCATION_EVIDENCE_OK source_commit=$($evidence.sourceCommit) packages=2 test_rows=4 passed=8"
