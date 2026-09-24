param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$project = Join-Path $repo 'tools/TypedMatBenchmark/TypedMatBenchmark.csproj'
$dotnet = Get-Command dotnet -ErrorAction Stop
& $dotnet.Source build $project -c Release
if ($LASTEXITCODE -ne 0) { throw "Typed Mat benchmark build failed." }
$lines = @(& $dotnet.Source run --project $project -c Release --no-build 2>&1)
if ($LASTEXITCODE -ne 0) { throw "Typed Mat benchmark run failed." }
$jsonLines = @($lines | ForEach-Object { [string]$_ } | Where-Object { $_.TrimStart().StartsWith('{') -and $_.TrimEnd().EndsWith('}') })
if ($jsonLines.Count -ne 1) { throw "Typed Mat benchmark must emit one JSON object." }
$result = $jsonLines[0] | ConvertFrom-Json
if ([string]$result.status -eq 'skipped') {
    if ([string]$result.reason -cne 'native-runtime-required') { throw 'Typed Mat benchmark skipped for an undocumented reason.' }
} elseif ([string]$result.status -eq 'measured') {
    if ([int]$result.iterations -ne 100 -or [string]$result.inputSha256 -notmatch '^[0-9a-f]{64}$') { throw 'Typed Mat benchmark identity drifted.' }
    foreach ($name in @('rowAccessor','matView','readOnlyMatView')) { if ($null -eq $result.$name) { throw "Typed Mat benchmark missing $name." } }
} else { throw "Typed Mat benchmark returned unknown status: $($result.status)" }
$iterations = if ($result.PSObject.Properties.Name -contains 'iterations') { [int]$result.iterations } else { 0 }
Write-Host "TYPED_MAT_BENCHMARK_HARNESS_OK status=$($result.status) iterations=$iterations"
