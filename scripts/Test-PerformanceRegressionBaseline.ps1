param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$paths = @{
    Baseline = Join-Path $repo 'packaging/performance/perf-001-regression-baseline.json'
    Schema = Join-Path $repo 'packaging/performance/perf-001-regression-baseline.schema.json'
    Codec = Join-Path $repo 'packaging/performance/codec-typed-mat-benchmark-evidence.json'
    Video = Join-Path $repo 'packaging/performance/video-dnn-benchmark-evidence.json'
}
foreach ($path in $paths.Values) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "PERF-001 regression input is missing: $path" } }
if (-not (Test-Json -LiteralPath $paths.Baseline -SchemaFile $paths.Schema -ErrorAction Stop)) { throw 'PERF-001 regression baseline failed JSON Schema validation.' }
$baseline = Get-Content -LiteralPath $paths.Baseline -Raw | ConvertFrom-Json
$codec = Get-Content -LiteralPath $paths.Codec -Raw | ConvertFrom-Json
$video = Get-Content -LiteralPath $paths.Video -Raw | ConvertFrom-Json
if ([string]$codec.status -cne 'measured' -or [string]$video.status -cne 'measured') { throw 'PERF-001 regression comparison requires measured codec and Video/DNN evidence.' }
if ([string]$codec.runner.sourceCommit -cne [string]$video.sourceCommit) { throw 'PERF-001 comparison evidence source commits differ.' }
$baselineCommit = [string]$baseline.createdFromCommit
$measuredCommit = [string]$codec.runner.sourceCommit
foreach ($commit in @($baselineCommit,$measuredCommit)) {
    & git -C $repo cat-file -e "$commit^{commit}" 2>$null
    if ($LASTEXITCODE -ne 0) { throw "PERF-001 source commit is absent from the repository: $commit" }
}
& git -C $repo merge-base --is-ancestor $baselineCommit $measuredCommit 2>$null
if ($LASTEXITCODE -ne 0) { throw 'PERF-001 evidence must be measured from the baseline commit or a descendant.' }

if ([string]$codec.runner.os -cne [string]$baseline.runner.os -or
    [string]$codec.runner.architecture -cne [string]$baseline.runner.architecture -or
    [string]$codec.runner.dotnet -cne [string]$baseline.runner.dotnet -or
    [string]$codec.runner.configuration -cne [string]$baseline.runner.configuration -or
    [string]$codec.runner.nativeRuntimePackageSha256 -cne [string]$baseline.runner.nativeRuntimePackageSha256 -or
    [string]$video.runner.os -cne [string]$baseline.runner.os -or
    [string]$video.runner.architecture -cne [string]$baseline.runner.architecture -or
    [string]$video.runner.dotnet -cne [string]$baseline.runner.dotnet -or
    [string]$video.runner.nativeRuntimePackageSha256 -cne [string]$baseline.runner.nativeRuntimePackageSha256) {
    throw 'PERF-001 comparison runner or exact native package identity differs from its baseline.'
}
$baselinePayload = @($baseline.runner.nativePayload | ForEach-Object { "$($_.name):$($_.bytes):$($_.sha256)" } | Sort-Object) -join '|'
$codecPayload = @($codec.runner.nativePayload | ForEach-Object { "$($_.name):$($_.bytes):$($_.sha256)" } | Sort-Object) -join '|'
$videoPayload = @($video.runner.nativePayload | ForEach-Object { "$($_.name):$($_.bytes):$($_.sha256)" } | Sort-Object) -join '|'
if ($baselinePayload -cne $codecPayload -or $baselinePayload -cne $videoPayload) { throw 'PERF-001 comparison native DLL payload differs from its baseline.' }
$typed = @($codec.typedMat | Sort-Object targetFramework)
$workloadValues = @(
    [string]$codec.codec.inputSha256,
    [string]$typed[0].inputSha256,
    [string]$video.video.fileSha256,
    [string]$video.dnn.modelSha256
)
$expectedWorkloads = @([string]$baseline.workloads.codecInputSha256,[string]$baseline.workloads.typedMatInputSha256,[string]$baseline.workloads.videoFileSha256,[string]$baseline.workloads.dnnModelSha256)
if (($workloadValues -join '|') -cne ($expectedWorkloads -join '|') -or [int]$baseline.workloads.iterations -ne 100) { throw 'PERF-001 fixed input workload identity differs from its baseline.' }
if (@($typed | ForEach-Object { [string]$_.inputSha256 } | Select-Object -Unique).Count -ne 1) { throw 'Typed Mat workload differs by target framework.' }
$net10Managed = @($codec.runner.managedAssemblies | Where-Object targetFramework -ceq 'net10.0')
if ($net10Managed.Count -ne 1 -or [string]$net10Managed[0].sha256 -cne [string]$video.runner.managedAssemblySha256) { throw 'PERF-001 net10.0 managed assembly hashes differ.' }

$current = [ordered]@{}
foreach ($name in @('encodeByteArray','encodeBufferWriter','decodeByteArray','decodeSpanWithPreflight','decodeStreamWithPreflight')) {
    $row = $codec.codec.$name
    $current["codec/$name"] = [ordered]@{ managedAllocatedBytes = [decimal]$row.managedAllocatedBytes; elapsedTicks = [decimal]$row.elapsedTicks }
}
foreach ($row in $typed) {
    foreach ($name in @('rowAccessor','matView','readOnlyMatView')) {
        $measure = $row.$name
        $current["typedMat/$($row.targetFramework)/$name"] = [ordered]@{ managedAllocatedBytes = [decimal]$measure.managedAllocatedBytes; elapsedTicks = [decimal]$measure.elapsedTicks }
    }
}
foreach ($name in @('video','dnn')) {
    $row = $video.$name.metric
    $current["$name/net10.0"] = [ordered]@{ managedAllocatedBytes = [decimal]$row.managedAllocatedBytes; elapsedTicks = [decimal]$row.elapsedTicks; peakWorkingSetBytes = [decimal]$row.peakWorkingSetBytes }
}
if (@($current.Keys).Count -ne @($baseline.metrics.PSObject.Properties).Count) { throw "PERF-001 metric set drifted: current=$(@($current.Keys).Count) baseline=$(@($baseline.metrics.PSObject.Properties).Count)." }
$failures = [System.Collections.Generic.List[string]]::new()
foreach ($property in $baseline.metrics.PSObject.Properties) {
    $name = [string]$property.Name
    if (-not $current.Contains($name)) { $failures.Add("missing metric $name"); continue }
    foreach ($metricName in @('managedAllocatedBytes','elapsedTicks','peakWorkingSetBytes')) {
        $baselineProperty = $property.Value.PSObject.Properties[$metricName]
        $baselineValue = if ($null -eq $baselineProperty) { $null } else { $baselineProperty.Value }
        $currentValue = $current[$name][$metricName]
        if ($null -eq $baselineValue -and $null -eq $currentValue) { continue }
        if ($null -eq $baselineValue -or $null -eq $currentValue) { $failures.Add("$name/$metricName metric shape changed"); continue }
        $thresholdName = switch ($metricName) {
            'elapsedTicks' { 'elapsedTicksPercent' }
            'managedAllocatedBytes' { 'managedAllocatedBytesPercent' }
            'peakWorkingSetBytes' { 'peakWorkingSetBytesPercent' }
        }
        $limit = [decimal]$baselineValue * (1 + ([decimal]$baseline.thresholds.$thresholdName / 100))
        if ([decimal]$currentValue -gt $limit) { $failures.Add("$name/$metricName current=$currentValue baseline=$baselineValue limit=$([decimal]::Round($limit,0)) (+$($baseline.thresholds.$thresholdName)%)") }
    }
}
if ($failures.Count -gt 0) { throw "PERF-001 regression threshold(s) exceeded:`n - $($failures -join "`n - ")" }
Write-Host "PERF001_REGRESSION_BASELINE_OK source_commit=$measuredCommit metrics=$($current.Count) thresholds=elapsed+20%,allocations+10%,peak_ws+10%"
