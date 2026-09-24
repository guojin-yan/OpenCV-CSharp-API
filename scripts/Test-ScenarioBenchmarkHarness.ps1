param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OpenCvNativeRuntimeDir = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$project = Join-Path $repo 'tools/ScenarioBenchmark/ScenarioBenchmark.csproj'
if (-not (Test-Path -LiteralPath $project -PathType Leaf)) { throw "Scenario benchmark project was not found: $project" }

# Keep skip results independent from a previous measured run or repository-level DLLs.
$outputDirectory = Join-Path $repo 'tools/ScenarioBenchmark/bin/Release/net10.0'
if (Test-Path -LiteralPath $outputDirectory -PathType Container) {
    Get-ChildItem -LiteralPath $outputDirectory -File |
        Where-Object { $_.Name -match '^(JYPPX\.OpenCV\.Native|opencv_.+)\.dll$' } |
        Remove-Item -Force
}

$dotnet = Get-Command dotnet -ErrorAction Stop
$buildArguments = @('build', $project, '-c', 'Release')
if (-not [string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir)) {
    $runtimePath = (Resolve-Path -LiteralPath $OpenCvNativeRuntimeDir).Path
    $buildArguments += '-p:OpenCvNativeRuntimeDir=' + $runtimePath
}
& $dotnet.Source @buildArguments
if ($LASTEXITCODE -ne 0) { throw "Scenario benchmark harness build failed with exit code $LASTEXITCODE." }

$runArguments = @('run', '--project', $project, '-c', 'Release', '--no-build')
Push-Location $outputDirectory
try {
    $output = @(& $dotnet.Source @runArguments 2>&1)
} finally {
    Pop-Location
}
if ($LASTEXITCODE -ne 0) { throw "Scenario benchmark harness run failed with exit code $LASTEXITCODE." }
$jsonLines = @($output | ForEach-Object { [string]$_ } | Where-Object { $_.TrimStart().StartsWith('{') -and $_.TrimEnd().EndsWith('}') })
if ($jsonLines.Count -ne 1) { throw "Scenario benchmark harness must emit exactly one JSON object; found $($jsonLines.Count)." }
$result = $jsonLines[0] | ConvertFrom-Json
if ([string]$result.status -eq 'skipped') {
    if ([string]$result.reason -cne 'native-runtime-required') { throw 'Scenario benchmark skipped for an undocumented reason.' }
} elseif ([string]$result.status -eq 'measured') {
    foreach ($property in @('iterations', 'video', 'dnn')) {
        if ($null -eq $result.$property) { throw "Measured scenario benchmark is missing property: $property" }
    }
    if ([int]$result.iterations -ne 100 -or [string]$result.targetFramework -cne 'net10.0') { throw 'Measured scenario benchmark identity drifted.' }
    if ([int]$result.video.iterations -ne 100 -or [int]$result.video.frameCount -ne 3 -or [string]$result.video.codec -cne 'MJPG') { throw 'Video benchmark identity drifted.' }
    if ([int]$result.dnn.iterations -ne 100 -or [int]$result.dnn.modelBytes -ne 147 -or [string]$result.dnn.modelSha256 -cne '326793cdb2fc2da739a715c3f3ff71d09779b389ad29e56bbfccc4313e900744') { throw 'DNN benchmark identity drifted.' }
    foreach ($metricName in @('video','dnn')) {
        $metric = $result.$metricName.metric
        foreach ($field in @('managedAllocatedBytes','elapsedTicks','checksum','baselineWorkingSetBytes','peakWorkingSetBytes')) {
            if ($null -eq $metric.$field -or [int64]$metric.$field -lt 0) { throw "Scenario metric is invalid: $metricName/$field" }
        }
        if ([int64]$metric.peakWorkingSetBytes -lt [int64]$metric.baselineWorkingSetBytes) { throw "Scenario peak working set is below baseline: $metricName" }
    }
} else {
    throw "Scenario benchmark returned an unknown status: $($result.status)"
}

$iterations = if ($result.PSObject.Properties.Name -contains 'iterations') { [int]$result.iterations } else { 0 }
Write-Host "SCENARIO_BENCHMARK_HARNESS_OK status=$($result.status) project=tools/ScenarioBenchmark iterations=$iterations"
