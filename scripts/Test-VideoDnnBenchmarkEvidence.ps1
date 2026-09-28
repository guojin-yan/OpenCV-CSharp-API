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
$commitType = ((& git -C $repo cat-file -t ([string]$evidence.sourceCommit) 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $commitType -cne 'commit') { throw 'Video/DNN benchmark source commit is not present in the repository.' }
& git -C $repo merge-base --is-ancestor ([string]$evidence.sourceCommit) HEAD 2>$null
if ($LASTEXITCODE -ne 0) { throw 'Video/DNN benchmark source commit is not an ancestor of HEAD.' }
if ([string]$evidence.runner.architecture -cne 'x64' -or [string]$evidence.runner.configuration -cne 'Release') { throw 'Video/DNN runner identity drifted.' }
if (@($evidence.runner.nativePayload).Count -lt 18) { throw 'Video/DNN evidence must bind the complete 18-file full runtime payload including the wrapper loader.' }
$payloadNames = @($evidence.runner.nativePayload | ForEach-Object { [string]$_.name })
if (@($payloadNames | Select-Object -Unique).Count -ne 18 -or $payloadNames -notcontains 'JYPPX.OpenCV.Native.dll' -or $payloadNames -notcontains 'opencv_dnn500.dll' -or $payloadNames -notcontains 'opencv_videoio500.dll') { throw 'Video/DNN native payload identity is incomplete or duplicated.' }
if ([string]$evidence.runner.managedAssemblySha256 -notmatch '^[0-9a-f]{64}$' -or [string]$evidence.runner.nativeRuntimePackageSha256 -notmatch '^[0-9a-f]{64}$') { throw 'Video/DNN artifact hashes are malformed.' }
foreach ($scenarioName in @('video','dnn')) {
    $metric = $evidence.$scenarioName.metric
    if ([int64]$metric.peakWorkingSetBytes -lt [int64]$metric.baselineWorkingSetBytes) { throw "Scenario peak working set is below baseline: $scenarioName" }
    foreach ($field in @('managedAllocatedBytes','elapsedTicks','checksum','baselineWorkingSetBytes','peakWorkingSetBytes')) {
        if ($null -eq $metric.$field -or [int64]$metric.$field -lt 0) { throw "Scenario metric is invalid: $scenarioName/$field" }
    }
}
if ([int64]$evidence.video.metric.checksum -le 0 -or [int64]$evidence.dnn.metric.checksum -le 0) { throw 'Video/DNN checksums must be positive.' }
Write-Host "VIDEO_DNN_BENCHMARK_EVIDENCE_OK source_commit=$($evidence.sourceCommit) native_payload=$(@($evidence.runner.nativePayload).Count) iterations=100 video_peak_ws=$($evidence.video.metric.peakWorkingSetBytes) dnn_peak_ws=$($evidence.dnn.metric.peakWorkingSetBytes)"
