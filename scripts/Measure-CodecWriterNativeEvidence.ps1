param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [Parameter(Mandatory)][string]$FullNativeRuntimeDir,
    [Parameter(Mandatory)][string]$MiniNativeRuntimeDir,
    [Parameter(Mandatory)][string]$FullRuntimePackagePath,
    [Parameter(Mandatory)][string]$MiniRuntimePackagePath,
    [string]$OutputPath = 'packaging/performance/codec-writer-native-evidence.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$outputFullPath = if ([IO.Path]::IsPathRooted($OutputPath)) {
    [IO.Path]::GetFullPath($OutputPath)
}
else {
    [IO.Path]::GetFullPath((Join-Path $repo ($OutputPath -replace '/', [IO.Path]::DirectorySeparatorChar)))
}

if (-not [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::Windows) -or
    [Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [Runtime.InteropServices.Architecture]::X64) {
    throw 'Codec writer native evidence requires a Windows x64 runner.'
}

$status = @(& git -C $repo status --porcelain 2>$null)
if ($LASTEXITCODE -ne 0) { throw 'Unable to inspect repository status.' }
if ($status.Count -ne 0) { throw 'Codec writer native measurement requires a clean repository.' }
$sourceCommit = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim().ToLowerInvariant()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Unable to resolve the source commit.' }

function Get-Sha256Bytes {
    param([Parameter(Mandatory)][byte[]]$Bytes)
    [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
}

function Get-Sha256File {
    param([Parameter(Mandatory)][string]$Path)
    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-PackagePayload {
    param(
        [Parameter(Mandatory)][string]$PackagePath,
        [Parameter(Mandatory)][string]$RuntimeDirectory,
        [Parameter(Mandatory)][string[]]$ExpectedNames,
        [Parameter(Mandatory)][string]$ExpectedPackageName
    )

    $package = (Resolve-Path -LiteralPath $PackagePath).Path
    $runtime = (Resolve-Path -LiteralPath $RuntimeDirectory).Path
    if ([IO.Path]::GetFileName($package).ToLowerInvariant() -cne $ExpectedPackageName) {
        throw "Unexpected runtime package name: $([IO.Path]::GetFileName($package))."
    }

    $runtimeFiles = @(Get-ChildItem -LiteralPath $runtime -File -Filter '*.dll' | Sort-Object Name)
    $actualNames = @($runtimeFiles | ForEach-Object { $_.Name } | Sort-Object)
    $sortedExpectedNames = @($ExpectedNames | Sort-Object)
    if (($actualNames -join '|') -cne ($sortedExpectedNames -join '|')) {
        throw "Runtime directory payload does not match the expected $($ExpectedNames.Count)-DLL profile."
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($package)
    try {
        $entries = @($archive.Entries |
            Where-Object { $_.FullName -like 'runtimes/win-x64/native/*.dll' } |
            Sort-Object FullName)
        $expectedEntries = @($ExpectedNames | ForEach-Object { "runtimes/win-x64/native/$_" } | Sort-Object)
        $actualEntries = @($entries | ForEach-Object { $_.FullName } | Sort-Object)
        if (($actualEntries -join '|') -cne ($expectedEntries -join '|')) {
            throw 'Runtime package native payload does not match the expected profile.'
        }
        $packagePayload = [System.Collections.Generic.List[object]]::new()
        foreach ($entry in $entries) {
            $stream = $entry.Open()
            $memory = [IO.MemoryStream]::new()
            try {
                $stream.CopyTo($memory)
                $bytes = $memory.ToArray()
            }
            finally {
                $memory.Dispose()
                $stream.Dispose()
            }
            $packagePayload.Add([ordered]@{
                name = [IO.Path]::GetFileName($entry.FullName)
                bytes = [int64]$bytes.Length
                sha256 = Get-Sha256Bytes -Bytes $bytes
            })
        }
    }
    finally {
        $archive.Dispose()
    }

    $runtimePayload = @($runtimeFiles | ForEach-Object {
        [ordered]@{ name = $_.Name; bytes = [int64]$_.Length; sha256 = Get-Sha256File -Path $_.FullName }
    })
    for ($index = 0; $index -lt $packagePayload.Count; $index++) {
        $packageRow = $packagePayload[$index]
        $runtimeRow = $runtimePayload[$index]
        if ($packageRow.name -cne $runtimeRow.name -or [int64]$packageRow.bytes -ne [int64]$runtimeRow.bytes -or $packageRow.sha256 -cne $runtimeRow.sha256) {
            throw "Runtime directory and package payload differ at $($packageRow.name)."
        }
    }

    [ordered]@{
        packageFileName = [IO.Path]::GetFileName($package)
        packageSha256 = Get-Sha256File -Path $package
        payload = @($packagePayload)
    }
}

function Get-TestCounters {
    param(
        [Parameter(Mandatory)][string]$Profile,
        [Parameter(Mandatory)][string]$Framework,
        [Parameter(Mandatory)][string]$TrxPath
    )

    if (-not (Test-Path -LiteralPath $TrxPath -PathType Leaf)) { throw "TRX result is missing for ${Profile}/${Framework}." }
    [xml]$trx = Get-Content -LiteralPath $TrxPath -Raw
    $counters = $trx.TestRun.ResultSummary.Counters
    if ($null -eq $counters) { throw "TRX counters are missing for ${Profile}/${Framework}." }
    $result = [ordered]@{
        profile = $Profile
        targetFramework = $Framework
        total = [int]$counters.total
        executed = [int]$counters.executed
        passed = [int]$counters.passed
        failed = [int]$counters.failed
        skipped = [int]$counters.notExecuted
        error = [int]$counters.error
        timeout = [int]$counters.timeout
        aborted = [int]$counters.aborted
        nativeSmokeEnabled = $true
    }
    foreach ($field in @('total', 'executed', 'passed')) { if ($result[$field] -ne 8) { throw "Codec writer counters drifted for ${Profile}/${Framework}: $field=$($result[$field])." } }
    foreach ($field in @('failed', 'skipped', 'error', 'timeout', 'aborted')) { if ($result[$field] -ne 0) { throw "Codec writer negative counter for ${Profile}/${Framework}: $field=$($result[$field])." } }
    $result
}

$dotnet = Get-Command dotnet -ErrorAction Stop
$project = Join-Path $repo 'tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj'
if (-not (Test-Path -LiteralPath $project -PathType Leaf)) { throw "Test project is missing: $project" }
$dotnetSdk = ((& $dotnet.Source --version 2>$null) | Select-Object -First 1).Trim()
$testFilter = 'FullyQualifiedName~JYPPX.OpenCvSharp.Tests.ImgCodecs.Cv2InteropTests'
$fullExpectedNames = @(
    'JYPPX.OpenCV.Native.dll', 'opencv_calib500.dll', 'opencv_core500.dll', 'opencv_dnn500.dll',
    'opencv_features500.dll', 'opencv_flann500.dll', 'opencv_geometry500.dll', 'opencv_highgui500.dll',
    'opencv_imgcodecs500.dll', 'opencv_imgproc500.dll', 'opencv_ml500.dll', 'opencv_objdetect500.dll',
    'opencv_photo500.dll', 'opencv_ptcloud500.dll', 'opencv_stereo500.dll', 'opencv_stitching500.dll',
    'opencv_video500.dll', 'opencv_videoio500.dll'
)
$miniExpectedNames = @(
    'JYPPX.OpenCV.Native.dll', 'opencv_core500.dll', 'opencv_flann500.dll', 'opencv_geometry500.dll',
    'opencv_imgcodecs500.dll', 'opencv_imgproc500.dll', 'opencv_videoio500.dll'
)
$runtimePackages = @(
    [pscustomobject]@{ Profile = 'full'; RuntimeDirectory = $FullNativeRuntimeDir; PackagePath = $FullRuntimePackagePath; ExpectedNames = $fullExpectedNames; PackageName = 'jyppx.opencv.runtime.win-x64.5.0.0.nupkg' },
    [pscustomobject]@{ Profile = 'mini'; RuntimeDirectory = $MiniNativeRuntimeDir; PackagePath = $MiniRuntimePackagePath; ExpectedNames = $miniExpectedNames; PackageName = 'jyppx.opencv.runtime.win-x64.mini.5.0.0.nupkg' }
)
$packageEvidence = [System.Collections.Generic.List[object]]::new()
foreach ($runtimePackage in $runtimePackages) {
    $packageEvidence.Add([ordered]@{
        profile = $runtimePackage.Profile
        package = Get-PackagePayload -PackagePath $runtimePackage.PackagePath -RuntimeDirectory $runtimePackage.RuntimeDirectory -ExpectedNames $runtimePackage.ExpectedNames -ExpectedPackageName $runtimePackage.PackageName
    })
}

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-codec-writer-native-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null
$frameworkResults = [System.Collections.Generic.List[object]]::new()
$previousSmoke = $env:OPENCV_CSHARP_NATIVE_SMOKE
$env:OPENCV_CSHARP_NATIVE_SMOKE = '1'
try {
    foreach ($runtimePackage in $runtimePackages) {
        foreach ($framework in @('net8.0', 'net10.0')) {
            $trxPath = Join-Path $temporaryRoot ("CodecWriter-$($runtimePackage.Profile)-$framework.trx")
            $arguments = @('test', $project, '-c', 'Release', '-f', $framework, '--no-restore', '--filter', $testFilter,
                ('-p:OpenCvNativeRuntimeDir=' + (Resolve-Path -LiteralPath $runtimePackage.RuntimeDirectory).Path), '--logger', ('trx;LogFileName=' + $trxPath))
            & $dotnet.Source @arguments | Out-Host
            if ($LASTEXITCODE -ne 0) { throw "Cv2InteropTests failed for $($runtimePackage.Profile)/$framework with exit code $LASTEXITCODE." }
            $row = Get-TestCounters -Profile $runtimePackage.Profile -Framework $framework -TrxPath $trxPath
            $testAssembly = Join-Path $repo ("tests/OpenCvSharp.Tests/bin/Release/$framework/OpenCvSharp.Tests.dll" -replace '/', [IO.Path]::DirectorySeparatorChar)
            $apiAssembly = Join-Path $repo ("src/OpenCvSharp/bin/Release/$framework/JYPPX.OpenCV.CSharp.API.dll" -replace '/', [IO.Path]::DirectorySeparatorChar)
            foreach ($assembly in @($testAssembly, $apiAssembly)) { if (-not (Test-Path -LiteralPath $assembly -PathType Leaf)) { throw "Managed assembly is missing for $($runtimePackage.Profile)/${framework}: $assembly" } }
            $row.testAssemblySha256 = Get-Sha256File -Path $testAssembly
            $row.apiAssemblySha256 = Get-Sha256File -Path $apiAssembly
            $frameworkResults.Add($row)
        }
    }
}
finally {
    if ($null -eq $previousSmoke) { Remove-Item Env:OPENCV_CSHARP_NATIVE_SMOKE -ErrorAction SilentlyContinue } else { $env:OPENCV_CSHARP_NATIVE_SMOKE = $previousSmoke }
    if (Test-Path -LiteralPath $temporaryRoot -PathType Container) { Remove-Item -LiteralPath $temporaryRoot -Recurse -Force }
}

$evidence = [ordered]@{
    '$schema' = 'codec-writer-native-evidence.schema.json'
    schemaVersion = 1
    status = 'verified'
    sourceCommit = $sourceCommit
    runner = [ordered]@{
        os = [Runtime.InteropServices.RuntimeInformation]::OSDescription
        dotnetSdk = $dotnetSdk
        architecture = 'x64'
        configuration = 'Release'
    }
    runtimePackages = @($packageEvidence)
    test = [ordered]@{
        class = 'JYPPX.OpenCvSharp.Tests.ImgCodecs.Cv2InteropTests'
        filter = $testFilter
        nativeSmokeEnabled = $true
        targetFrameworks = @($frameworkResults)
        verifiedCases = @(
            'ImEncode and ImDecode PNG round trip',
            'IBufferWriter exact caller-owned segment and one Advance',
            'IBufferWriter segmented output and payload equality',
            'IBufferWriter GetSpan fault is terminal',
            'IBufferWriter Advance fault is terminal',
            'IBufferWriter undersized span is rejected',
            'managed invalid inputs are rejected before native entry',
            'typed PNG/JPEG encode parameters round trip'
        )
    }
    limitations = @(
        'Focused native writer tests are verified on Windows x64 net8.0 and net10.0 against exact full and mini runtime package payloads.',
        'Allocation benchmark evidence remains runner-specific and is recorded separately; this evidence does not establish a stable allocation guarantee.',
        'ImEncodeTo remains a conditional preview contract until the allocation and owner/lifetime stabilization review is complete.'
    )
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) | Out-Null
[IO.File]::WriteAllText($outputFullPath, (($evidence | ConvertTo-Json -Depth 16) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Host "CODEC_WRITER_NATIVE_EVIDENCE_MEASURED source_commit=$sourceCommit profiles=full,mini frameworks=net8.0,net10.0 passed=$(@($frameworkResults | ForEach-Object { $_.passed } | Measure-Object -Sum).Sum) output=$outputFullPath"
