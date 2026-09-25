param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OpenCvNativeRuntimeDir = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$project = Join-Path $repo 'tools/CodecBenchmark/CodecBenchmark.csproj'
if (-not (Test-Path -LiteralPath $project -PathType Leaf)) { throw "Codec benchmark project was not found: $project" }

$outputDirectory = Join-Path $repo 'tools/CodecBenchmark/bin/Release/net8.0'
if (Test-Path -LiteralPath $outputDirectory -PathType Container) {
    Get-ChildItem -LiteralPath $outputDirectory -File |
        Where-Object { $_.Name -match '^(JYPPX\.OpenCV\.Native|opencv_.+)\.dll$' } |
        Remove-Item -Force
}

$dotnet = Get-Command dotnet -ErrorAction Stop
$buildArguments = @('build', $project, '-c', 'Release', '-f', 'net8.0')
if (-not [string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir)) {
    $runtimePath = (Resolve-Path -LiteralPath $OpenCvNativeRuntimeDir).Path
    $buildArguments += '-p:OpenCvNativeRuntimeDir=' + $runtimePath
}
& $dotnet.Source @buildArguments
if ($LASTEXITCODE -ne 0) { throw "Codec benchmark harness build failed with exit code $LASTEXITCODE." }

$runArguments = @('run', '--project', $project, '-c', 'Release', '-f', 'net8.0', '--no-build')
$runExitCode = 0
Push-Location $outputDirectory
try {
    $output = @(& $dotnet.Source @runArguments 2>&1)
    $runExitCode = $LASTEXITCODE
}
finally {
    Pop-Location
}
if ($runExitCode -ne 0) { throw "Codec benchmark harness run failed with exit code $runExitCode." }
$jsonLines = @($output | ForEach-Object { [string]$_ } | Where-Object { $_.TrimStart().StartsWith('{') -and $_.TrimEnd().EndsWith('}') })
if ($jsonLines.Count -ne 1) { throw "Codec benchmark harness must emit exactly one JSON object; found $($jsonLines.Count)." }
$result = $jsonLines[0] | ConvertFrom-Json
if ([string]$result.status -eq 'skipped') {
    if ([string]$result.reason -cne 'native-runtime-required') { throw 'Codec benchmark skipped for an undocumented reason.' }
} elseif ([string]$result.status -eq 'measured') {
    foreach ($property in @('iterations', 'inputBytes', 'inputSha256', 'encodedBytes', 'encodedSha256', 'encodeByteArray', 'encodeBufferWriter', 'decodeByteArray', 'decodeSpanWithPreflight', 'decodeStreamWithPreflight')) {
        if ($null -eq $result.$property) { throw "Measured codec benchmark is missing property: $property" }
    }
    if ([int]$result.iterations -ne 100 -or [string]$result.inputSha256 -notmatch '^[0-9a-f]{64}$' -or [string]$result.encodedSha256 -notmatch '^[0-9a-f]{64}$') { throw 'Measured codec benchmark identity drifted.' }
    foreach ($path in @('decodeByteArray', 'decodeSpanWithPreflight', 'decodeStreamWithPreflight')) {
        if ($null -eq $result.$path.decodedBytes -or $null -eq $result.$path.checksum -or $null -eq $result.$path.managedAllocatedBytes) { throw "Measured codec benchmark decode path is incomplete: $path" }
    }
} else {
    throw "Codec benchmark returned an unknown status: $($result.status)"
}

$iterations = if ($result.PSObject.Properties.Name -contains 'iterations') { [int]$result.iterations } else { 0 }
Write-Host "CODEC_BENCHMARK_HARNESS_OK status=$($result.status) project=tools/CodecBenchmark iterations=$iterations"
