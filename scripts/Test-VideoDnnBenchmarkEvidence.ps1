param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidenceRel = 'packaging/performance/video-dnn-benchmark-evidence.json'
$schemaRel = 'packaging/performance/video-dnn-benchmark-evidence.schema.json'
$evidencePath = Join-Path $repo ($evidenceRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($evidencePath, $schemaPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Video/DNN benchmark evidence file is missing: $path" } }
if (-not (Test-Json -LiteralPath $evidencePath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Video/DNN benchmark evidence failed JSON Schema validation.' }
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
if ([string]$evidence.status -cne 'measured' -or [string]$evidence.sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Video/DNN benchmark evidence identity is invalid.' }
if ([string]$evidence.runner.architecture -cne 'x64' -or [string]$evidence.runner.configuration -cne 'Release') { throw 'Video/DNN runner identity drifted.' }
if (@($evidence.runner.nativePayload).Count -lt 18) { throw 'Video/DNN evidence must bind the complete 18-file full runtime payload including the wrapper loader.' }
foreach ($scenarioName in @('video','dnn')) {
    $metric = $evidence.$scenarioName.metric
    if ([int64]$metric.peakWorkingSetBytes -lt [int64]$metric.baselineWorkingSetBytes) { throw "Scenario peak working set is below baseline: $scenarioName" }
    foreach ($field in @('managedAllocatedBytes','elapsedTicks','checksum','baselineWorkingSetBytes','peakWorkingSetBytes')) {
        if ($null -eq $metric.$field -or [int64]$metric.$field -lt 0) { throw "Scenario metric is invalid: $scenarioName/$field" }
    }
}
if ([int64]$evidence.video.metric.checksum -le 0 -or [int64]$evidence.dnn.metric.checksum -le 0) { throw 'Video/DNN checksums must be positive.' }
Write-Host "VIDEO_DNN_BENCHMARK_EVIDENCE_OK source_commit=$($evidence.sourceCommit) native_payload=$(@($evidence.runner.nativePayload).Count) iterations=100 video_peak_ws=$($evidence.video.metric.peakWorkingSetBytes) dnn_peak_ws=$($evidence.dnn.metric.peakWorkingSetBytes)"
