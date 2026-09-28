param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [Parameter(Mandatory)][string]$OpenCvNativeRuntimeDir,
    [Parameter(Mandatory)][string]$NativeRuntimePackagePath,
    [string]$OutputPath = 'packaging/performance/video-dnn-benchmark-evidence.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$project = Join-Path $repo 'tools/ScenarioBenchmark/ScenarioBenchmark.csproj'
$runtimePath = (Resolve-Path -LiteralPath $OpenCvNativeRuntimeDir).Path
$packagePath = (Resolve-Path -LiteralPath $NativeRuntimePackagePath).Path
$outputFullPath = if ([IO.Path]::IsPathRooted($OutputPath)) {
    [IO.Path]::GetFullPath($OutputPath)
} else {
    [IO.Path]::GetFullPath((Join-Path $repo ($OutputPath -replace '/', [IO.Path]::DirectorySeparatorChar)))
}
foreach ($path in @($project, $runtimePath, $packagePath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf) -and -not (Test-Path -LiteralPath $path -PathType Container)) {
        throw "Video/DNN benchmark input is missing: $path"
    }
}
if (-not (Test-Path -LiteralPath $packagePath -PathType Leaf)) { throw "Native runtime package path is not a file: $packagePath" }
$gitStatus = @(& git -C $repo status --porcelain 2>$null)
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect repository status before Video/DNN benchmark measurement.' }
if ($gitStatus.Count -ne 0) { throw 'Commit source changes before measuring PERF-001 evidence; the working tree must be clean so sourceCommit is accurate.' }
if ([Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [Runtime.InteropServices.Architecture]::X64 -or
    -not [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::Windows)) {
    throw 'PERF-001 Video/DNN evidence requires a Windows x64 runner.'
}

$nativePayload = @(Get-ChildItem -LiteralPath $runtimePath -File | Where-Object { $_.Name -match '^JYPPX\.OpenCV\.Native\.dll$|^opencv_.+\.dll$' } | Sort-Object Name)
if ($nativePayload.Count -ne 18) { throw "Video/DNN benchmark requires exactly 18 full native payload files; found $($nativePayload.Count)." }
$requiredPayload = @('JYPPX.OpenCV.Native.dll', 'opencv_core500.dll', 'opencv_dnn500.dll', 'opencv_imgcodecs500.dll', 'opencv_imgproc500.dll', 'opencv_video500.dll', 'opencv_videoio500.dll')
foreach ($name in $requiredPayload) {
    if (@($nativePayload | Where-Object Name -ceq $name).Count -ne 1) { throw "Video/DNN benchmark native payload is missing: $name" }
}
$payloadEvidence = @($nativePayload | ForEach-Object {
        [ordered]@{
            name = $_.Name
            bytes = [int64]$_.Length
            sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        }
    })

Add-Type -AssemblyName System.IO.Compression.FileSystem
$packageArchive = [IO.Compression.ZipFile]::OpenRead($packagePath)
try {
    $packagePayload = @($packageArchive.Entries |
            Where-Object { $_.FullName -match '^runtimes/win-x64/native/[^/]+\.dll$' } |
            Sort-Object FullName |
            ForEach-Object {
                $entry = $_
                $stream = $entry.Open()
                try {
                    $entryHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)).ToLowerInvariant()
                } finally {
                    $stream.Dispose()
                }
                [ordered]@{
                    name = [IO.Path]::GetFileName($entry.FullName)
                    bytes = [int64]$entry.Length
                    sha256 = $entryHash
                }
            })
} finally {
    $packageArchive.Dispose()
}
if ($packagePayload.Count -ne 18) { throw "Native runtime package must contain exactly 18 win-x64 native DLL entries; found $($packagePayload.Count)." }
for ($index = 0; $index -lt $payloadEvidence.Count; $index++) {
    $directoryRow = $payloadEvidence[$index]
    $packageRow = $packagePayload[$index]
    if ($directoryRow.name -cne $packageRow.name -or [int64]$directoryRow.bytes -ne [int64]$packageRow.bytes -or $directoryRow.sha256 -cne $packageRow.sha256) {
        throw "Native runtime directory does not match package entry: directory=$($directoryRow.name) package=$($packageRow.name)"
    }
}

$dotnet = Get-Command dotnet -ErrorAction Stop
$outputDirectory = Join-Path $repo 'tools/ScenarioBenchmark/bin/Release/net10.0'
if (Test-Path -LiteralPath $outputDirectory -PathType Container) {
    Get-ChildItem -LiteralPath $outputDirectory -File |
        Where-Object { $_.Name -match '^(JYPPX\.OpenCV\.Native|opencv_.+)\.dll$' } |
        Remove-Item -Force
}

$buildArguments = @('build', $project, '-c', 'Release', ('-p:OpenCvNativeRuntimeDir=' + $runtimePath))
& $dotnet.Source @buildArguments
if ($LASTEXITCODE -ne 0) { throw "Scenario benchmark build failed with exit code $LASTEXITCODE." }

$runOutput = @()
Push-Location $outputDirectory
try {
    $runOutput = @(& $dotnet.Source run '--project' $project '-c' 'Release' '--no-build' 2>&1)
    $runExitCode = $LASTEXITCODE
} finally {
    Pop-Location
}
if ($runExitCode -ne 0) { throw "Scenario benchmark run failed with exit code $runExitCode." }
$jsonLines = @($runOutput | ForEach-Object { [string]$_ } | Where-Object { $_.TrimStart().StartsWith('{') -and $_.TrimEnd().EndsWith('}') })
if ($jsonLines.Count -ne 1) { throw "Scenario benchmark must emit exactly one JSON object; found $($jsonLines.Count)." }
$result = $jsonLines[0] | ConvertFrom-Json
if ([string]$result.status -cne 'measured') { throw "Scenario benchmark did not produce measured evidence: $($result.status)" }

$sourceCommit = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw "Could not resolve repository source commit: $sourceCommit" }
$managedAssembly = Join-Path $repo 'src/OpenCvSharp/bin/Release/net10.0/JYPPX.OpenCV.CSharp.API.dll'
if (-not (Test-Path -LiteralPath $managedAssembly -PathType Leaf)) { throw "Managed assembly was not found: $managedAssembly" }

$evidence = [ordered]@{
    '$schema' = 'video-dnn-benchmark-evidence.schema.json'
    schemaVersion = 1
    status = 'measured'
    sourceCommit = $sourceCommit
    runner = [ordered]@{
        os = [Runtime.InteropServices.RuntimeInformation]::OSDescription
        architecture = 'x64'
        dotnet = ((& $dotnet.Source --version 2>$null) | Select-Object -First 1).Trim()
        configuration = 'Release'
        managedAssemblySha256 = (Get-FileHash -LiteralPath $managedAssembly -Algorithm SHA256).Hash.ToLowerInvariant()
        nativeRuntimePackage = [IO.Path]::GetFileName($packagePath)
        nativeRuntimePackageSha256 = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash.ToLowerInvariant()
        nativePayload = $payloadEvidence
    }
    video = $result.video
    dnn = $result.dnn
    limitations = @(
        'Elapsed ticks and process working-set values are runner-specific regression evidence.',
        'WorkingSet64 is the Windows process RSS-equivalent used by this benchmark; it is sampled between operations.',
        'The video path uses a temporary MJPG AVI and exercises file-backed VideoIO, not camera or network backends.',
        'The DNN path uses the fixed 147-byte identity ONNX fixture and the OpenCV CPU backend.',
        'The native runtime package and payload hashes identify the factual full win-x64 runtime consumed by this measurement.'
    )
}
$outputDirectoryPath = Split-Path -Parent $outputFullPath
New-Item -ItemType Directory -Force -Path $outputDirectoryPath | Out-Null
[IO.File]::WriteAllText($outputFullPath, (($evidence | ConvertTo-Json -Depth 12) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Host "VIDEO_DNN_BENCHMARK_MEASURED source_commit=$sourceCommit native_payload=$($nativePayload.Count) video_peak_ws=$($evidence.video.metric.peakWorkingSetBytes) dnn_peak_ws=$($evidence.dnn.metric.peakWorkingSetBytes) output=$outputFullPath"
