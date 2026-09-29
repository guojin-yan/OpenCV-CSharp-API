param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [Parameter(Mandatory)][string]$OpenCvNativeRuntimeDir,
    [Parameter(Mandatory)][string]$NativeRuntimePackagePath,
    [string]$OutputPath = 'packaging/performance/typed-mat-native-lifetime-evidence.json'
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

if (-not [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::Windows) -or
    [Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [Runtime.InteropServices.Architecture]::X64) {
    throw 'Typed Mat native lifetime evidence requires a Windows x64 runner.'
}
if (-not (Test-Path -LiteralPath $packagePath -PathType Leaf)) { throw "Runtime package path is not a file: $packagePath" }

$gitStatus = @(& git -C $repo status --porcelain 2>$null)
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect repository status before Typed Mat lifetime measurement.' }
if ($gitStatus.Count -ne 0) { throw 'Commit source changes before measuring Typed Mat lifetime evidence; the working tree must be clean.' }
$sourceCommit = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Could not resolve the Typed Mat lifetime source commit.' }

$nativePayload = @(Get-ChildItem -LiteralPath $runtimePath -File |
    Where-Object { $_.Name -match '^JYPPX\.OpenCV\.Native\.dll$|^opencv_.+\.dll$' } |
    Sort-Object Name)
if ($nativePayload.Count -ne 18) { throw "Typed Mat lifetime evidence requires exactly 18 native runtime DLLs; found $($nativePayload.Count)." }
foreach ($name in @('JYPPX.OpenCV.Native.dll', 'opencv_core500.dll', 'opencv_imgproc500.dll', 'opencv_imgcodecs500.dll')) {
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
if ($packagePayload.Count -ne 18) { throw "Runtime package must contain exactly 18 win-x64 native DLLs; found $($packagePayload.Count)." }
for ($index = 0; $index -lt 18; $index++) {
    $folderRow = $payloadEvidence[$index]
    $packageRow = $packagePayload[$index]
    if ($folderRow.name -cne $packageRow.name -or [int64]$folderRow.bytes -ne [int64]$packageRow.bytes -or $folderRow.sha256 -cne $packageRow.sha256) {
        throw "Native runtime directory and NuGet package differ at $($folderRow.name)."
    }
}

$dotnet = Get-Command dotnet -ErrorAction Stop
$project = Join-Path $repo 'tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj'
if (-not (Test-Path -LiteralPath $project -PathType Leaf)) { throw "Test project is missing: $project" }
$testFilter = 'FullyQualifiedName~JYPPX.OpenCvSharp.Tests.Core.MatViewTests'
$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-typed-mat-lifetime-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null
$frameworkResults = [System.Collections.Generic.List[object]]::new()
$previousSmoke = $env:OPENCV_CSHARP_NATIVE_SMOKE
$env:OPENCV_CSHARP_NATIVE_SMOKE = '1'
try {
    foreach ($framework in @('net8.0', 'net10.0')) {
        $trxPath = Join-Path $temporaryRoot ("MatView-$framework.trx")
        $arguments = @('test', $project, '-c', 'Release', '-f', $framework, '--no-restore', '--filter', $testFilter,
            ('-p:OpenCvNativeRuntimeDir=' + $runtimePath), '--logger', ('trx;LogFileName=' + $trxPath))
        & $dotnet.Source @arguments | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "MatViewTests failed to run for $framework with exit code $LASTEXITCODE." }
        if (-not (Test-Path -LiteralPath $trxPath -PathType Leaf)) { throw "TRX result is missing for ${framework}: $trxPath" }
        [xml]$trx = Get-Content -LiteralPath $trxPath -Raw
        $counters = $trx.TestRun.ResultSummary.Counters
        if ($null -eq $counters) { throw "TRX counters are missing for $framework." }
        $total = [int]$counters.total
        $executed = [int]$counters.executed
        $passed = [int]$counters.passed
        $failed = [int]$counters.failed
        $skipped = [int]$counters.notExecuted
        $error = [int]$counters.error
        $timeout = [int]$counters.timeout
        $aborted = [int]$counters.aborted
        if ($total -ne 9 -or $executed -ne 9 -or $passed -ne 9 -or $failed -ne 0 -or $skipped -ne 0 -or $error -ne 0 -or $timeout -ne 0 -or $aborted -ne 0) {
            throw "Typed Mat lifetime counters drifted for ${framework}: total=$total executed=$executed passed=$passed failed=$failed skipped=$skipped error=$error timeout=$timeout aborted=$aborted."
        }
        $testAssembly = Join-Path $repo ("tests/OpenCvSharp.Tests/bin/Release/$framework/OpenCvSharp.Tests.dll" -replace '/', [IO.Path]::DirectorySeparatorChar)
        $apiAssembly = Join-Path $repo ("src/OpenCvSharp/bin/Release/$framework/JYPPX.OpenCV.CSharp.API.dll" -replace '/', [IO.Path]::DirectorySeparatorChar)
        foreach ($assembly in @($testAssembly, $apiAssembly)) { if (-not (Test-Path -LiteralPath $assembly -PathType Leaf)) { throw "Managed assembly is missing for ${framework}: $assembly" } }
        $frameworkResults.Add([ordered]@{
            framework = $framework
            total = $total
            executed = $executed
            passed = $passed
            failed = $failed
            skipped = $skipped
            error = $error
            timeout = $timeout
            aborted = $aborted
            testAssemblySha256 = (Get-FileHash -LiteralPath $testAssembly -Algorithm SHA256).Hash.ToLowerInvariant()
            apiAssemblySha256 = (Get-FileHash -LiteralPath $apiAssembly -Algorithm SHA256).Hash.ToLowerInvariant()
        })
    }
} finally {
    if ($null -eq $previousSmoke) { Remove-Item Env:OPENCV_CSHARP_NATIVE_SMOKE -ErrorAction SilentlyContinue } else { $env:OPENCV_CSHARP_NATIVE_SMOKE = $previousSmoke }
    if (Test-Path -LiteralPath $temporaryRoot -PathType Container) { Remove-Item -LiteralPath $temporaryRoot -Recurse -Force }
}

$managedVersion = Join-Path $repo 'src/OpenCvSharp/OpenCvSharpBuildInfo.cs'
$evidence = [ordered]@{
    '$schema' = 'typed-mat-native-lifetime-evidence.schema.json'
    schemaVersion = 2
    status = 'verified'
    sourceCommit = $sourceCommit
    runner = [ordered]@{
        os = [Runtime.InteropServices.RuntimeInformation]::OSDescription
        dotnetSdk = ((& $dotnet.Source --version 2>$null) | Select-Object -First 1).Trim()
        architecture = 'x64'
        configuration = 'Release'
        nativeRuntimePackage = [IO.Path]::GetFileName($packagePath)
        nativeRuntimePackageSha256 = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash.ToLowerInvariant()
        nativePayload = $payloadEvidence
    }
    runtime = [ordered]@{
        openCvVersion = '5.0.0'
        nativeAbiVersion = 1
        nativeModuleFiles = 17
        wrapperLoaderSha256 = [string]($payloadEvidence | Where-Object name -ceq 'JYPPX.OpenCV.Native.dll' | Select-Object -ExpandProperty sha256)
        opencvCoreSha256 = [string]($payloadEvidence | Where-Object name -ceq 'opencv_core500.dll' | Select-Object -ExpandProperty sha256)
        opencvImgProcSha256 = [string]($payloadEvidence | Where-Object name -ceq 'opencv_imgproc500.dll' | Select-Object -ExpandProperty sha256)
        opencvImgCodecsSha256 = [string]($payloadEvidence | Where-Object name -ceq 'opencv_imgcodecs500.dll' | Select-Object -ExpandProperty sha256)
    }
    test = [ordered]@{
        class = 'JYPPX.OpenCvSharp.Tests.Core.MatViewTests'
        filter = $testFilter
        nativeSmokeEnabled = $true
        targetFrameworks = $frameworkResults.ToArray()
        verifiedCases = @(
            'continuous registered pixel read/write and copy',
            'non-contiguous ROI row access and bounds',
            'unregistered and mismatched type rejection',
            'view disposal and owner disposal invalidation',
            'native header mutation rejection',
            'clone and destination copy ownership',
            'read-only surface and disposal/header behavior'
        )
    }
    limitations = @(
        'The factual full runtime package and all 18 native payload hashes are bound to this evidence; native payload provenance remains separate from managed source provenance.',
        'Native owner/lifetime execution is verified on Windows x64 net8.0 and net10.0; all 15 package target frameworks remain compilation-only evidence.',
        'The MatView and ReadOnlyMatView APIs remain conditional preview contracts and are not promoted to stable cross-target API by this evidence alone.'
    )
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) | Out-Null
[IO.File]::WriteAllText($outputFullPath, (($evidence | ConvertTo-Json -Depth 14) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Host "TYPED_MAT_NATIVE_LIFETIME_MEASURED source_commit=$sourceCommit package_sha256=$($evidence.runner.nativeRuntimePackageSha256) frameworks=net8.0,net10.0 cases=$($evidence.test.verifiedCases.Count) output=$outputFullPath"
