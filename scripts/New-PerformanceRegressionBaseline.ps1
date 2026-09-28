param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OutputPath = 'packaging/performance/perf-001-regression-baseline.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$output = if ([IO.Path]::IsPathRooted($OutputPath)) { [IO.Path]::GetFullPath($OutputPath) } else { [IO.Path]::GetFullPath((Join-Path $repo ($OutputPath -replace '/', [IO.Path]::DirectorySeparatorChar))) }
if (Test-Path -LiteralPath $output -PathType Leaf) { throw "PERF-001 regression baseline already exists and is immutable: $output" }
$status = @(& git -C $repo status --porcelain 2>$null)
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect repository status before creating the PERF-001 baseline.' }
$allowedEvidencePaths = @('packaging/performance/codec-typed-mat-benchmark-evidence.json','packaging/performance/video-dnn-benchmark-evidence.json')
$changedPaths = @($status | ForEach-Object { ([string]$_).Substring(3).Replace('\','/') })
if (@($changedPaths | Where-Object { $_ -notin $allowedEvidencePaths }).Count -gt 0) { throw "Only the two freshly measured PERF-001 evidence files may differ from HEAD: $($changedPaths -join ', ')" }
$head = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $head -notmatch '^[0-9a-f]{40}$') { throw 'Could not resolve HEAD for the PERF-001 baseline.' }

$pwsh = Get-Command pwsh -ErrorAction Stop
foreach ($scriptName in @('Test-PerformanceBenchmarkEvidence.ps1','Test-VideoDnnBenchmarkEvidence.ps1')) {
    & $pwsh.Source -NoProfile -File (Join-Path $repo "scripts/$scriptName") -RepositoryRoot $repo
    if ($LASTEXITCODE -ne 0) { throw "Evidence validation failed: $scriptName" }
}
$codec = Get-Content -LiteralPath (Join-Path $repo 'packaging/performance/codec-typed-mat-benchmark-evidence.json') -Raw | ConvertFrom-Json
$video = Get-Content -LiteralPath (Join-Path $repo 'packaging/performance/video-dnn-benchmark-evidence.json') -Raw | ConvertFrom-Json
if ([string]$codec.runner.sourceCommit -cne $head -or [string]$video.sourceCommit -cne $head) { throw 'Codec/typed Mat and Video/DNN evidence must both measure the exact clean HEAD.' }
if ([string]$codec.runner.sourceCommit -cne [string]$video.sourceCommit) { throw 'PERF-001 evidence source commits differ.' }
if ([string]$codec.runner.architecture -cne [string]$video.runner.architecture -or
    [string]$codec.runner.dotnet -cne [string]$video.runner.dotnet -or
    [string]$codec.runner.os -cne [string]$video.runner.os -or
    [string]$codec.runner.nativeRuntimePackageSha256 -cne [string]$video.runner.nativeRuntimePackageSha256) {
    throw 'PERF-001 evidence runner or runtime package identities differ.'
}
$payloadIdentity = { param($e) (@($e.runner.nativePayload | ForEach-Object { "$($_.name):$($_.bytes):$($_.sha256)" } | Sort-Object) -join '|') }
if ((& $payloadIdentity $codec) -cne (& $payloadIdentity $video)) { throw 'PERF-001 evidence native payload identities differ.' }
$net10Assembly = @($codec.runner.managedAssemblies | Where-Object targetFramework -ceq 'net10.0')
if ($net10Assembly.Count -ne 1 -or [string]$net10Assembly[0].sha256 -cne [string]$video.runner.managedAssemblySha256) { throw 'PERF-001 net10.0 managed assembly hashes differ across the benchmark sets.' }
$typed = @($codec.typedMat | Sort-Object targetFramework)
if (@($typed | ForEach-Object { [string]$_.inputSha256 } | Select-Object -Unique).Count -ne 1) { throw 'Typed Mat workloads differ across target frameworks.' }

$metrics = [ordered]@{}
foreach ($name in @('encodeByteArray','encodeBufferWriter','decodeByteArray','decodeSpanWithPreflight','decodeStreamWithPreflight')) {
    $row = $codec.codec.$name
    $metrics["codec/$name"] = [ordered]@{ managedAllocatedBytes = [int64]$row.managedAllocatedBytes; elapsedTicks = [int64]$row.elapsedTicks }
}
foreach ($row in $typed) {
    foreach ($name in @('rowAccessor','matView','readOnlyMatView')) {
        $measure = $row.$name
        $metrics["typedMat/$($row.targetFramework)/$name"] = [ordered]@{ managedAllocatedBytes = [int64]$measure.managedAllocatedBytes; elapsedTicks = [int64]$measure.elapsedTicks }
    }
}
foreach ($name in @('video','dnn')) {
    $row = $video.$name.metric
    $metrics["$name/net10.0"] = [ordered]@{ managedAllocatedBytes = [int64]$row.managedAllocatedBytes; elapsedTicks = [int64]$row.elapsedTicks; peakWorkingSetBytes = [int64]$row.peakWorkingSetBytes }
}
$baseline = [ordered]@{
    '$schema' = 'perf-001-regression-baseline.schema.json'
    schemaVersion = 1
    status = 'measured-baseline'
    createdFromCommit = $head
    runner = [ordered]@{
        os = [string]$codec.runner.os
        architecture = 'x64'
        dotnet = [string]$codec.runner.dotnet
        configuration = 'Release'
        nativeRuntimePackage = [string]$codec.runner.nativeRuntimePackage
        nativeRuntimePackageSha256 = [string]$codec.runner.nativeRuntimePackageSha256
        nativePayload = @($codec.runner.nativePayload | Sort-Object name | ForEach-Object { [ordered]@{ name = [string]$_.name; bytes = [int64]$_.bytes; sha256 = [string]$_.sha256 } })
    }
    workloads = [ordered]@{
        codecInputSha256 = [string]$codec.codec.inputSha256
        typedMatInputSha256 = [string]$typed[0].inputSha256
        videoFileSha256 = [string]$video.video.fileSha256
        dnnModelSha256 = [string]$video.dnn.modelSha256
        iterations = 100
    }
    thresholds = [ordered]@{
        elapsedTicksPercent = 20
        managedAllocatedBytesPercent = 10
        peakWorkingSetBytesPercent = 10
    }
    metrics = $metrics
    limitations = @(
        'Thresholds are per metric relative to the immutable measured baseline: elapsed ticks +20%, managed allocations +10%, and sampled peak working set +10%.',
        'Comparisons require the same Windows x64 OS identity, .NET SDK, fixed workloads, exact native NuGet package hash, and all 18 native payload hashes.',
        'Elapsed ticks and sampled working set are runner-specific signals, not cross-machine performance claims. Refreshing this baseline requires an explicit reviewed change.'
    )
}
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $output) | Out-Null
[IO.File]::WriteAllText($output, (($baseline | ConvertTo-Json -Depth 12) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Host "PERF001_REGRESSION_BASELINE_CREATED source_commit=$head metrics=$($metrics.Count) package_sha256=$($codec.runner.nativeRuntimePackageSha256) output=$output"
