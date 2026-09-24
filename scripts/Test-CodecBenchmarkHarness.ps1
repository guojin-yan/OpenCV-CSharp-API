param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$project = Join-Path $repo 'tools/CodecBenchmark/CodecBenchmark.csproj'
if (-not (Test-Path -LiteralPath $project -PathType Leaf)) { throw "Codec benchmark project was not found: $project" }

$dotnet = Get-Command dotnet -ErrorAction Stop
& $dotnet.Source build $project -c Release
if ($LASTEXITCODE -ne 0) { throw "Codec benchmark harness build failed with exit code $LASTEXITCODE." }

$output = @(& $dotnet.Source run --project $project -c Release --no-build 2>&1)
if ($LASTEXITCODE -ne 0) { throw "Codec benchmark harness run failed with exit code $LASTEXITCODE." }
$jsonLines = @($output | ForEach-Object { [string]$_ } | Where-Object { $_.TrimStart().StartsWith('{') -and $_.TrimEnd().EndsWith('}') })
if ($jsonLines.Count -ne 1) { throw "Codec benchmark harness must emit exactly one JSON object; found $($jsonLines.Count)." }
$result = $jsonLines[0] | ConvertFrom-Json
if ([string]$result.status -eq 'skipped') {
    if ([string]$result.reason -cne 'native-runtime-required') { throw 'Codec benchmark skipped for an undocumented reason.' }
} elseif ([string]$result.status -eq 'measured') {
    foreach ($property in @('iterations', 'inputBytes', 'inputSha256', 'byteArray', 'bufferWriter')) {
        if ($null -eq $result.$property) { throw "Measured codec benchmark is missing property: $property" }
    }
    if ([int]$result.iterations -ne 100 -or [string]$result.inputSha256 -notmatch '^[0-9a-f]{64}$') { throw 'Measured codec benchmark identity drifted.' }
} else {
    throw "Codec benchmark returned an unknown status: $($result.status)"
}

$iterations = if ($result.PSObject.Properties.Name -contains 'iterations') { [int]$result.iterations } else { 0 }
Write-Host "CODEC_BENCHMARK_HARNESS_OK status=$($result.status) project=tools/CodecBenchmark iterations=$iterations"
