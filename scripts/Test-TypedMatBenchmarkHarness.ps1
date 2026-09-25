param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OpenCvNativeRuntimeDir = ''
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$project = Join-Path $repo 'tools/TypedMatBenchmark/TypedMatBenchmark.csproj'
$dotnet = Get-Command dotnet -ErrorAction Stop
$runtimePath = ''
if (-not [string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir)) {
    $runtimePath = (Resolve-Path -LiteralPath $OpenCvNativeRuntimeDir).Path
}
foreach ($framework in @('net8.0', 'net10.0')) {
    $outputDirectory = Join-Path $repo ("tools/TypedMatBenchmark/bin/Release/$framework" -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (Test-Path -LiteralPath $outputDirectory -PathType Container) {
        Get-ChildItem -LiteralPath $outputDirectory -File |
            Where-Object { $_.Name -match '^(JYPPX\.OpenCV\.Native|opencv_.+)\.dll$' } |
            Remove-Item -Force
    }

    $buildArguments = @('build', $project, '-c', 'Release', '-f', $framework)
    if ($runtimePath -ne '') { $buildArguments += '-p:OpenCvNativeRuntimeDir=' + $runtimePath }
    & $dotnet.Source @buildArguments
    if ($LASTEXITCODE -ne 0) { throw "Typed Mat benchmark build failed for $framework." }
    $runArguments = @('run', '--project', $project, '-c', 'Release', '-f', $framework, '--no-build')
    $runExitCode = 0
    Push-Location $outputDirectory
    try {
        $lines = @(& $dotnet.Source @runArguments 2>&1)
        $runExitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }
    if ($runExitCode -ne 0) { throw "Typed Mat benchmark run failed with exit code $runExitCode for $framework." }
    $jsonLines = @($lines | ForEach-Object { [string]$_ } | Where-Object { $_.TrimStart().StartsWith('{') -and $_.TrimEnd().EndsWith('}') })
    if ($jsonLines.Count -ne 1) { throw "Typed Mat benchmark must emit one JSON object for $framework." }
    $result = $jsonLines[0] | ConvertFrom-Json
    if ([string]$result.status -eq 'skipped') {
        if ([string]$result.reason -cne 'native-runtime-required') { throw "Typed Mat benchmark skipped for an undocumented reason on ${framework}." }
    } elseif ([string]$result.status -eq 'measured') {
        if ([string]$result.targetFramework -cne $framework -or [int]$result.iterations -ne 100 -or [string]$result.inputSha256 -notmatch '^[0-9a-f]{64}$') { throw "Typed Mat benchmark identity drifted for ${framework}." }
        foreach ($name in @('rowAccessor','matView','readOnlyMatView')) { if ($null -eq $result.$name) { throw "Typed Mat benchmark missing $name on ${framework}." } }
    } else { throw "Typed Mat benchmark returned unknown status on ${framework}: $($result.status)" }
    $iterations = if ($result.PSObject.Properties.Name -contains 'iterations') { [int]$result.iterations } else { 0 }
    Write-Host "TYPED_MAT_BENCHMARK_HARNESS_OK framework=$framework status=$($result.status) iterations=$iterations"
}
