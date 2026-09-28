param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [Parameter(Mandatory)][string]$OpenCvNativeRuntimeDir,
    [Parameter(Mandatory)][string]$NativeRuntimePackagePath,
    [string]$OutputPath = 'packaging/performance/codec-typed-mat-benchmark-evidence.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$runtimePath = (Resolve-Path -LiteralPath $OpenCvNativeRuntimeDir).Path
$packagePath = (Resolve-Path -LiteralPath $NativeRuntimePackagePath).Path
$outputFullPath = if ([IO.Path]::IsPathRooted($OutputPath)) {
    [IO.Path]::GetFullPath($OutputPath)
} else {
    [IO.Path]::GetFullPath((Join-Path $repo ($OutputPath -replace '/', [IO.Path]::DirectorySeparatorChar)))
}

if ([Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [Runtime.InteropServices.Architecture]::X64) {
    throw 'PERF-001 codec/typed Mat evidence requires an x64 runner.'
}
if (-not [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::Windows)) {
    throw 'PERF-001 codec/typed Mat evidence requires Windows because its paired Video/DNN baseline is Windows-specific.'
}

$gitStatus = @(& git -C $repo status --porcelain 2>$null)
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect repository status before benchmark measurement.' }
if ($gitStatus.Count -ne 0) { throw 'Commit source changes before measuring PERF-001 evidence; the working tree must be clean so sourceCommit is accurate.' }
$sourceCommit = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Could not resolve the PERF-001 source commit.' }

$nativePayload = @(Get-ChildItem -LiteralPath $runtimePath -File |
    Where-Object { $_.Name -match '^JYPPX\.OpenCV\.Native\.dll$|^opencv_.+\.dll$' } |
    Sort-Object Name)
if ($nativePayload.Count -ne 18) { throw "PERF-001 requires exactly 18 full native runtime DLLs; found $($nativePayload.Count)." }
foreach ($name in @('JYPPX.OpenCV.Native.dll', 'opencv_core500.dll', 'opencv_imgcodecs500.dll', 'opencv_imgproc500.dll', 'opencv_dnn500.dll', 'opencv_videoio500.dll')) {
    if (@($nativePayload | Where-Object Name -ceq $name).Count -ne 1) { throw "Native runtime payload is missing required DLL: $name" }
}
$payloadEvidence = @($nativePayload | ForEach-Object {
    [ordered]@{
        name = $_.Name
        bytes = [int64]$_.Length
        sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    }
})

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($packagePath)
try {
    $packagePayload = @($archive.Entries |
        Where-Object { $_.FullName -match '^runtimes/win-x64/native/[^/]+\.dll$' } |
        Sort-Object FullName |
        ForEach-Object {
            $entry = $_
            $stream = $entry.Open()
            try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)).ToLowerInvariant() }
            finally { $stream.Dispose() }
            [ordered]@{ name = [IO.Path]::GetFileName($entry.FullName); bytes = [int64]$entry.Length; sha256 = $hash }
        })
} finally { $archive.Dispose() }
if ($packagePayload.Count -ne 18) { throw "Runtime NuGet package must contain exactly 18 win-x64 native DLLs; found $($packagePayload.Count)." }
for ($index = 0; $index -lt 18; $index++) {
    $folderRow = $payloadEvidence[$index]
    $packageRow = $packagePayload[$index]
    if ($folderRow.name -cne $packageRow.name -or [int64]$folderRow.bytes -ne [int64]$packageRow.bytes -or $folderRow.sha256 -cne $packageRow.sha256) {
        throw "Native runtime directory and NuGet package differ at $($folderRow.name)."
    }
}

$dotnet = Get-Command dotnet -ErrorAction Stop
$projects = @(
    [pscustomobject]@{ Name = 'CodecBenchmark'; Path = Join-Path $repo 'tools/CodecBenchmark/CodecBenchmark.csproj'; Frameworks = @('net8.0') },
    [pscustomobject]@{ Name = 'TypedMatBenchmark'; Path = Join-Path $repo 'tools/TypedMatBenchmark/TypedMatBenchmark.csproj'; Frameworks = @('net8.0', 'net10.0') }
)
foreach ($project in $projects) {
    if (-not (Test-Path -LiteralPath $project.Path -PathType Leaf)) { throw "Benchmark project is missing: $($project.Path)" }
    foreach ($framework in $project.Frameworks) {
        $outputDirectory = Join-Path $repo ("tools/$($project.Name)/bin/Release/$framework" -replace '/', [IO.Path]::DirectorySeparatorChar)
        if (Test-Path -LiteralPath $outputDirectory -PathType Container) {
            Get-ChildItem -LiteralPath $outputDirectory -File |
                Where-Object { $_.Name -match '^(JYPPX\.OpenCV\.Native|opencv_.+)\.dll$' } |
                Remove-Item -Force
        }
        $buildArguments = @('build', $project.Path, '-c', 'Release', '-f', $framework, ('-p:OpenCvNativeRuntimeDir=' + $runtimePath))
        & $dotnet.Source @buildArguments
        if ($LASTEXITCODE -ne 0) { throw "$($project.Name) $framework build failed with exit code $LASTEXITCODE." }
    }
}

function Invoke-Benchmark {
    param([string]$AssemblyPath, [string]$WorkingDirectory, [string]$Name)

    if (-not (Test-Path -LiteralPath $AssemblyPath -PathType Leaf)) { throw "$Name assembly is missing: $AssemblyPath" }
    Push-Location $WorkingDirectory
    try {
        $lines = @(& $dotnet.Source $AssemblyPath 2>&1)
        $exitCode = $LASTEXITCODE
    } finally { Pop-Location }
    if ($exitCode -ne 0) { throw "$Name failed with exit code ${exitCode}: $($lines -join [Environment]::NewLine)" }
    $jsonLines = @($lines | ForEach-Object { [string]$_ } | Where-Object { $_.TrimStart().StartsWith('{') -and $_.TrimEnd().EndsWith('}') })
    if ($jsonLines.Count -ne 1) { throw "$Name must emit exactly one JSON object; found $($jsonLines.Count)." }
    $result = $jsonLines[0] | ConvertFrom-Json
    if ([string]$result.status -cne 'measured') { throw "$Name did not measure with the supplied runtime: $($result.status)" }
    return $result
}

$codecOutput = Join-Path $repo 'tools/CodecBenchmark/bin/Release/net8.0'
$typedNet8Output = Join-Path $repo 'tools/TypedMatBenchmark/bin/Release/net8.0'
$typedNet10Output = Join-Path $repo 'tools/TypedMatBenchmark/bin/Release/net10.0'
$codec = Invoke-Benchmark -AssemblyPath (Join-Path $codecOutput 'CodecBenchmark.dll') -WorkingDirectory $codecOutput -Name 'CodecBenchmark net8.0'
$typedMat = @(
    (Invoke-Benchmark -AssemblyPath (Join-Path $typedNet8Output 'TypedMatBenchmark.dll') -WorkingDirectory $typedNet8Output -Name 'TypedMatBenchmark net8.0'),
    (Invoke-Benchmark -AssemblyPath (Join-Path $typedNet10Output 'TypedMatBenchmark.dll') -WorkingDirectory $typedNet10Output -Name 'TypedMatBenchmark net10.0')
)
if ([int]$codec.iterations -ne 100 -or @($typedMat | Where-Object { [int]$_.iterations -ne 100 }).Count -ne 0) {
    throw 'PERF-001 benchmark iteration count drifted from 100.'
}

$managedAssemblies = @(
    [ordered]@{
        targetFramework = 'net8.0'
        sha256 = (Get-FileHash -LiteralPath (Join-Path $repo 'src/OpenCvSharp/bin/Release/net8.0/JYPPX.OpenCV.CSharp.API.dll') -Algorithm SHA256).Hash.ToLowerInvariant()
    },
    [ordered]@{
        targetFramework = 'net10.0'
        sha256 = (Get-FileHash -LiteralPath (Join-Path $repo 'src/OpenCvSharp/bin/Release/net10.0/JYPPX.OpenCV.CSharp.API.dll') -Algorithm SHA256).Hash.ToLowerInvariant()
    }
)
$os = [Runtime.InteropServices.RuntimeInformation]::OSDescription
$evidence = [ordered]@{
    '$schema' = 'codec-typed-mat-benchmark-evidence.schema.json'
    schemaVersion = 1
    status = 'measured'
    runner = [ordered]@{
        os = $os
        architecture = 'x64'
        dotnet = ((& $dotnet.Source --version 2>$null) | Select-Object -First 1).Trim()
        configuration = 'Release'
        sourceCommit = $sourceCommit
        managedAssemblies = $managedAssemblies
        nativeRuntimePackage = [IO.Path]::GetFileName($packagePath)
        nativeRuntimePackageSha256 = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash.ToLowerInvariant()
        nativePayload = $payloadEvidence
    }
    codec = $codec
    typedMat = $typedMat
    limitations = @(
        'Each scenario measures 100 warmed iterations and records runner-specific allocation and elapsed-tick regression evidence.',
        'The codec, typed Mat, and paired Video/DNN records use the same exact win-x64 native NuGet package hash and the same source commit.',
        'The native runtime package is the factual OpenCV 5.0.0 full runtime identified by package and per-DLL hashes; it is not claimed to be rebuilt from this managed source commit.',
        'The Stream decode path includes full managed preflight buffering and is expected to allocate more than the byte-array and span paths.'
    )
}

$outputDirectory = Split-Path -Parent $outputFullPath
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
[IO.File]::WriteAllText($outputFullPath, (($evidence | ConvertTo-Json -Depth 14) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Host "CODEC_TYPED_MAT_BENCHMARK_MEASURED source_commit=$sourceCommit package_sha256=$($evidence.runner.nativeRuntimePackageSha256) native_payload=$($payloadEvidence.Count) codec_tfm=$($codec.targetFramework) typed_mat_tfms=net8.0,net10.0 output=$outputFullPath"
