param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidenceRel = 'packaging/performance/codec-typed-mat-benchmark-evidence.json'
$schemaRel = 'packaging/performance/codec-typed-mat-benchmark-evidence.schema.json'
$evidencePath = Join-Path $repo ($evidenceRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($evidencePath, $schemaPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Benchmark evidence file is missing: $path" } }
if (-not (Test-Json -LiteralPath $evidencePath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Benchmark evidence failed JSON Schema validation.' }
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json

if ([string]$evidence.status -cne 'measured') { throw 'Benchmark evidence must be measured.' }
if ([string]$evidence.codec.targetFramework -cne 'net8.0' -or [int]$evidence.codec.iterations -ne 100) { throw 'Codec benchmark identity drifted.' }
if ([string]$evidence.codec.inputSha256 -notmatch '^[0-9a-f]{64}$' -or [string]$evidence.codec.encodedSha256 -notmatch '^[0-9a-f]{64}$') { throw 'Codec benchmark hashes are malformed.' }

$decodePaths = @('decodeByteArray', 'decodeSpanWithPreflight', 'decodeStreamWithPreflight')
$decodeChecksums = @($decodePaths | ForEach-Object { [int64]$evidence.codec.$_.checksum })
$decodeBytes = @($decodePaths | ForEach-Object { [int]$evidence.codec.$_.decodedBytes })
if (@($decodeChecksums | Select-Object -Unique).Count -ne 1 -or @($decodeBytes | Select-Object -Unique).Count -ne 1) { throw 'Codec decode paths do not have matching checksum and decoded-byte facts.' }
foreach ($path in $decodePaths) {
    if ([int64]$evidence.codec.$path.managedAllocatedBytes -lt 0 -or [int64]$evidence.codec.$path.elapsedTicks -lt 0) { throw "Codec metric is negative: $path" }
}

$typed = @($evidence.typedMat)
if ($typed.Count -lt 2) { throw 'Typed Mat evidence must contain net8.0 and net10.0 rows.' }
$frameworks = @($typed | ForEach-Object { [string]$_.targetFramework } | Sort-Object -Unique)
if (($frameworks -join '|') -ne 'net10.0|net8.0') { throw "Typed Mat framework set drifted: $($frameworks -join ',')" }
$allChecksums = [System.Collections.Generic.List[int64]]::new()
foreach ($row in $typed) {
    if ([int]$row.iterations -ne 100 -or [string]$row.inputSha256 -notmatch '^[0-9a-f]{64}$') { throw "Typed Mat benchmark identity drifted for $($row.targetFramework)." }
    $checksums = @([int64]$row.rowAccessor.checksum, [int64]$row.matView.checksum, [int64]$row.readOnlyMatView.checksum)
    if (@($checksums | Select-Object -Unique).Count -ne 1) { throw "Typed Mat checksums differ within $($row.targetFramework)." }
    foreach ($metric in @('rowAccessor','matView','readOnlyMatView')) {
        if ([int64]$row.$metric.managedAllocatedBytes -lt 0 -or [int64]$row.$metric.elapsedTicks -lt 0) { throw "Typed Mat metric is negative: $($row.targetFramework)/$metric" }
    }
    $allChecksums.Add($checksums[0])
}
if (@($allChecksums | Select-Object -Unique).Count -ne 1) { throw 'Typed Mat checksums differ across target frameworks.' }

Write-Host "PERFORMANCE_BENCHMARK_EVIDENCE_OK codec_paths=$($decodePaths.Count) typed_mat_frameworks=$($typed.Count) iterations=100 checksum=$($allChecksums[0])"
