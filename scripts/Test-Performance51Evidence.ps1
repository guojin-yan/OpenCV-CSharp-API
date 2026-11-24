[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$CandidatePath = '',
    [switch]$RequirePackageArtifacts
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Performance51.Common.ps1')
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$schema = Join-Path $repo 'packaging/performance/perf-001-5.1.0-baseline.schema.json'
$baselineJson = Get-Content -LiteralPath (Join-Path $repo 'packaging/performance/perf-001-5.1.0-baseline.json') -Raw
$baseline = $baselineJson | ConvertFrom-Json
$sourceSet = Get-Performance51SourceSet -RepositoryRoot $repo

function Get-Performance51WorkloadIdentity {
    param([object]$Record)
    $rows = foreach ($run in $Record.runs) {
        $r = $run.stdoutJson | ConvertFrom-Json
        if ($run.scenario -ceq 'codec') { "$($run.round)/codec:$($r.inputSha256):$($r.encodedSha256):$($r.encodedBytes)" }
        elseif ($run.scenario -ceq 'typedMat') { "$($run.round)/typedMat/$($run.targetFramework):$($r.inputSha256)" }
        else { "$($run.round)/scenario:$($r.video.fileSha256):$($r.video.fileBytes):$($r.video.metric.checksum):$($r.dnn.modelSha256)" }
    }
    return $rows -join '|'
}

function Assert-Performance51Record {
    param([object]$Record, [bool]$RequireCurrentSource = $true)
    if (-not (Test-Json -Json ($Record | ConvertTo-Json -Depth 16) -SchemaFile $schema -ErrorAction Stop)) { throw 'Performance 5.1 schema failed.' }
    & git -C $repo merge-base --is-ancestor ([string]$Record.sourceCommit) HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { throw 'Performance source is not an ancestor of HEAD.' }
    if ($RequireCurrentSource) {
        & git -C $repo diff --quiet ([string]$Record.sourceCommit) -- @($sourceSet.paths)
        if ($LASTEXITCODE -ne 0 -or ($Record.sourceInputSet | ConvertTo-Json -Depth 5 -Compress) -cne ($sourceSet | ConvertTo-Json -Depth 5 -Compress)) { throw 'Performance source inputs drifted.' }
    }
    $package = Get-Performance51Package -RepositoryRoot $repo -SourceCommit $Record.sourceCommit
    if (($Record.runtimePackage | ConvertTo-Json -Depth 8 -Compress) -cne ($package | ConvertTo-Json -Depth 8 -Compress)) { throw 'Performance package identity/payload drifted.' }
    $expectedKeys = foreach ($round in 1..3) { foreach ($key in @('codec/net8.0', 'typedMat/net8.0', 'typedMat/net10.0', 'scenario/net10.0')) { "$round/$key" } }
    if ((@($Record.runs | ForEach-Object { "$($_.round)/$($_.scenario)/$($_.targetFramework)" }) -join '|') -cne ($expectedKeys -join '|')) { throw 'Performance run inventory/order drifted.' }
    foreach ($run in $Record.runs) {
        if ((Get-Performance51TextHash -Text $run.stdoutJson) -cne $run.stdoutSha256 -or (Get-Performance51TextHash -Text $run.stderr) -cne $run.stderrSha256) { throw 'Performance output bytes drifted.' }
        Assert-Performance51Output -Scenario $run.scenario -TargetFramework $run.targetFramework -Output ($run.stdoutJson | ConvertFrom-Json)
    }
    foreach ($tfm in @('net8.0', 'net10.0')) {
        if (@($Record.runs | Where-Object targetFramework -CEQ $tfm | Select-Object -ExpandProperty apiAssemblySha256 -Unique).Count -ne 1) { throw 'Harnesses did not use identical API assemblies.' }
    }
    foreach ($group in @($Record.runs | Group-Object scenario, targetFramework)) {
        if (@($group.Group.harnessSha256 | Select-Object -Unique).Count -ne 1) { throw 'Harness bytes drifted between repetitions.' }
        $outputs = @($group.Group | ForEach-Object { $_.stdoutJson | ConvertFrom-Json })
        if ($outputs[0].PSObject.Properties['encodedSha256'] -and @($outputs.encodedSha256 | Select-Object -Unique).Count -ne 1) { throw 'Codec output drifted between repetitions.' }
        if ($outputs[0].PSObject.Properties['video'] -and @($outputs | ForEach-Object { "$($_.video.fileSha256):$($_.video.metric.checksum)" } | Select-Object -Unique).Count -ne 1) { throw 'Video workload drifted between repetitions.' }
    }
    $metrics = Get-Performance51Metrics -Runs $Record.runs
    if ($metrics.Count -ne 14 -or ($Record.metrics | ConvertTo-Json -Depth 5 -Compress) -cne ($metrics | ConvertTo-Json -Depth 5 -Compress)) { throw 'Performance medians do not match raw measured outputs.' }
}

Assert-Performance51Record -Record $baseline -RequireCurrentSource ([string]::IsNullOrWhiteSpace($CandidatePath))
if ($RequirePackageArtifacts) {
    $packagePath = Join-Path $repo $baseline.runtimePackage.artifactPath
    if ((Get-ModernApi51Sha256 -Path $packagePath) -cne $baseline.runtimePackage.sha256 -or (Get-Item -LiteralPath $packagePath).Length -ne $baseline.runtimePackage.bytes) { throw 'Measured performance package is missing or changed.' }
    $sbom = Get-Content -LiteralPath (Join-Path $repo $baseline.runtimePackage.evidence.path) -Raw | ConvertFrom-Json
    if ((Get-LocalPackageSbom51Manifest -PackagePath $packagePath | ConvertTo-Json -Depth 5 -Compress) -cne (@($sbom.packages | Where-Object role -CEQ 'full')[0].entries | ConvertTo-Json -Depth 5 -Compress)) { throw 'Measured performance package entries drifted.' }
}
$negativeCases = [ordered]@{
    'source drift' = { param($r) $r.sourceInputSet.sha256 = '0' * 64 }
    'wrong OpenCV version' = { param($r) $r.openCvVersion = '5.0.0' }
    'package hash drift' = { param($r) $r.runtimePackage.sha256 = '0' * 64 }
    'missing native DLL' = { param($r) $r.runtimePackage.payload = @($r.runtimePackage.payload | Select-Object -Skip 1) }
    'DLL hash drift' = { param($r) $r.runtimePackage.payload[0].sha256 = '0' * 64 }
    'duplicate run' = { param($r) $r.runs[1] = $r.runs[0] }
    'output byte drift' = { param($r) $r.runs[0].stdoutJson += ' ' }
    'stderr byte drift' = { param($r) $r.runs[0].stderr = 'unrecorded diagnostic' }
    'unrestricted path' = { param($r) $r.runs[0].isolation.pathRestricted = $false }
    'false DLL audit' = { param($r) $r.runs[0].isolation.payloadVerified = $false }
    'wrong managed assembly' = { param($r) $r.runs[1].apiAssemblySha256 = '0' * 64 }
    'changed harness' = { param($r) $r.runs[4].harnessSha256 = '0' * 64 }
    'invented median' = { param($r) $r.metrics.'codec/encodeByteArray'.elapsedTicks++ }
    'weakened threshold' = { param($r) $r.thresholds.elapsedTicksPercent = 200 }
    'false cross RID claim' = { param($r) $r.limits.crossRidValidated = $true }
    'false historical comparison' = { param($r) $r.limits.historical50Comparison = $true }
    'preview promotion' = { param($r) $r.limits.stablePreviewApisPromoted = $true }
    'publication approval' = { param($r) $r.limits.finalReleaseApproved = $true }
    'skipped benchmark with valid hash' = {
        param($r)
        $o = $r.runs[0].stdoutJson | ConvertFrom-Json; $o.status = 'skipped'
        $r.runs[0].stdoutJson = $o | ConvertTo-Json -Depth 8 -Compress; $r.runs[0].stdoutSha256 = Get-Performance51TextHash -Text $r.runs[0].stdoutJson
    }
    'wrong checksum with valid hash' = {
        param($r)
        $o = $r.runs[1].stdoutJson | ConvertFrom-Json; $o.matView.checksum++
        $r.runs[1].stdoutJson = $o | ConvertTo-Json -Depth 8 -Compress; $r.runs[1].stdoutSha256 = Get-Performance51TextHash -Text $r.runs[1].stdoutJson
    }
}
foreach ($fixture in $negativeCases.GetEnumerator()) {
    $mutant = $baselineJson | ConvertFrom-Json; & $fixture.Value $mutant
    $rejected = $false
    try { Assert-Performance51Record -Record $mutant } catch { $rejected = $true }
    if (-not $rejected) { throw "Performance guard accepted negative fixture: $($fixture.Key)" }
}
Assert-Performance51Comparison -Baseline $baseline -Candidate $baseline -RepositoryRoot $repo
foreach ($field in @('elapsedTicks', 'managedAllocatedBytes', 'peakWorkingSetBytes')) {
    $mutant = $baselineJson | ConvertFrom-Json
    $mutant.metrics.'video/net10.0'.$field = [int64]([decimal]$mutant.metrics.'video/net10.0'.$field * 2 + 1)
    $rejected = $false
    try { Assert-Performance51Comparison -Baseline $baseline -Candidate $mutant -RepositoryRoot $repo } catch { $rejected = $true }
    if (-not $rejected) { throw "Performance comparator accepted $field regression." }
}
if (-not [string]::IsNullOrWhiteSpace($CandidatePath)) {
    $candidate = Get-Content -LiteralPath (Resolve-ModernApi51Path -RepositoryRoot $repo -Path $CandidatePath) -Raw | ConvertFrom-Json
    Assert-Performance51Record -Record $candidate
    if ((Get-Performance51WorkloadIdentity -Record $baseline) -cne (Get-Performance51WorkloadIdentity -Record $candidate)) { throw 'Regression comparison workloads differ.' }
    Assert-Performance51Comparison -Baseline $baseline -Candidate $candidate -RepositoryRoot $repo
}
Write-Host "PERFORMANCE_51_EVIDENCE_OK source=$($baseline.sourceCommit) runs=12 metrics=14 repetitions=3 iterations=100 dlls=43 negative_fixtures=$($negativeCases.Count + 3) initial_baseline=true historical_comparison=false"
