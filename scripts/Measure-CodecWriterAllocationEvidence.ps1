param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [Parameter(Mandatory)][string]$FullNativeRuntimeDir,
    [Parameter(Mandatory)][string]$MiniNativeRuntimeDir,
    [Parameter(Mandatory)][string]$FullRuntimePackagePath,
    [Parameter(Mandatory)][string]$MiniRuntimePackagePath,
    [string]$OutputPath = 'packaging/performance/codec-writer-allocation-evidence.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$outputFullPath = if ([IO.Path]::IsPathRooted($OutputPath)) {
    [IO.Path]::GetFullPath($OutputPath)
} else {
    [IO.Path]::GetFullPath((Join-Path $repo ($OutputPath -replace '/', [IO.Path]::DirectorySeparatorChar)))
}

if (-not [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::Windows) -or
    [Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [Runtime.InteropServices.Architecture]::X64) {
    throw 'Codec writer allocation evidence requires a Windows x64 runner.'
}

$status = @(& git -C $repo status --porcelain 2>$null)
if ($LASTEXITCODE -ne 0 -or $status.Count -ne 0) { throw 'Codec writer allocation measurement requires a clean repository.' }
$sourceCommit = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim().ToLowerInvariant()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Unable to resolve the source commit.' }

function Get-Sha256File {
    param([Parameter(Mandatory)][string]$Path)
    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-PackagePayload {
    param(
        [Parameter(Mandatory)][string]$Profile,
        [Parameter(Mandatory)][string]$RuntimeDirectory,
        [Parameter(Mandatory)][string]$PackagePath,
        [Parameter(Mandatory)][string[]]$ExpectedNames,
        [Parameter(Mandatory)][string]$ExpectedPackageName,
        [Parameter(Mandatory)][string]$ExpectedPackageHash
    )

    $runtime = (Resolve-Path -LiteralPath $RuntimeDirectory).Path
    $package = (Resolve-Path -LiteralPath $PackagePath).Path
    if ([IO.Path]::GetFileName($package).ToLowerInvariant() -cne $ExpectedPackageName) { throw "Unexpected $Profile package name." }
    if ((Get-Sha256File -Path $package) -cne $ExpectedPackageHash) { throw "Unexpected $Profile package hash." }

    $runtimeFiles = @(Get-ChildItem -LiteralPath $runtime -File -Filter '*.dll' | Sort-Object Name)
    $actualNames = @($runtimeFiles | ForEach-Object { $_.Name } | Sort-Object)
    if (($actualNames -join '|') -cne (@($ExpectedNames | Sort-Object) -join '|')) { throw "$Profile runtime payload names drifted." }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($package)
    try {
        $entries = @($archive.Entries | Where-Object { $_.FullName -like 'runtimes/win-x64/native/*.dll' } | Sort-Object FullName)
        $expectedEntries = @($ExpectedNames | ForEach-Object { "runtimes/win-x64/native/$_" } | Sort-Object)
        if ((@($entries | ForEach-Object FullName) -join '|') -cne ($expectedEntries -join '|')) { throw "$Profile package payload names drifted." }
        $payload = @($entries | ForEach-Object {
            $entry = $_
            $stream = $entry.Open()
            try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)).ToLowerInvariant() }
            finally { $stream.Dispose() }
            [ordered]@{ name = [IO.Path]::GetFileName($entry.FullName); bytes = [int64]$entry.Length; sha256 = $hash }
        })
    }
    finally { $archive.Dispose() }

    $runtimePayload = @($runtimeFiles | ForEach-Object { [ordered]@{ name = $_.Name; bytes = [int64]$_.Length; sha256 = Get-Sha256File -Path $_.FullName } })
    for ($index = 0; $index -lt $payload.Count; $index++) {
        if ($payload[$index].name -cne $runtimePayload[$index].name -or
            [int64]$payload[$index].bytes -ne [int64]$runtimePayload[$index].bytes -or
            $payload[$index].sha256 -cne $runtimePayload[$index].sha256) {
            throw "$Profile runtime directory and package payload differ at $($payload[$index].name)."
        }
    }

    [ordered]@{
        profile = $Profile
        package = [ordered]@{
            packageFileName = [IO.Path]::GetFileName($package)
            packageSha256 = Get-Sha256File -Path $package
            payload = @($payload)
        }
    }
}

function Get-TestCounters {
    param([Parameter(Mandatory)][string]$Profile,[Parameter(Mandatory)][string]$Framework,[Parameter(Mandatory)][string]$TrxPath)
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
    foreach ($field in @('total', 'executed', 'passed')) { if ($result[$field] -ne 2) { throw "Codec writer allocation counters drifted for ${Profile}/${Framework}: $field=$($result[$field])." } }
    foreach ($field in @('failed', 'skipped', 'error', 'timeout', 'aborted')) { if ($result[$field] -ne 0) { throw "Codec writer allocation negative counter for ${Profile}/${Framework}: $field=$($result[$field])." } }
    $result
}

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
    [pscustomobject]@{ Profile = 'full'; RuntimeDirectory = $FullNativeRuntimeDir; PackagePath = $FullRuntimePackagePath; ExpectedNames = $fullExpectedNames; PackageName = 'jyppx.opencv.runtime.win-x64.5.0.0.nupkg'; PackageHash = '07de65af0ec14420d84d49caa4663d08393880a1bae402c789177d2dfff2544e' },
    [pscustomobject]@{ Profile = 'mini'; RuntimeDirectory = $MiniNativeRuntimeDir; PackagePath = $MiniRuntimePackagePath; ExpectedNames = $miniExpectedNames; PackageName = 'jyppx.opencv.runtime.win-x64.mini.5.0.0.nupkg'; PackageHash = 'c4712ed62f34f31eacaaa4c51f84ee6003bf5bab3cfcca850d39c3b812628918' }
)
$packageEvidence = @($runtimePackages | ForEach-Object { Get-PackagePayload -Profile $_.Profile -RuntimeDirectory $_.RuntimeDirectory -PackagePath $_.PackagePath -ExpectedNames $_.ExpectedNames -ExpectedPackageName $_.PackageName -ExpectedPackageHash $_.PackageHash })

$dotnet = Get-Command dotnet -ErrorAction Stop
$project = Join-Path $repo 'tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj'
$testFilter = 'FullyQualifiedName~JYPPX.OpenCvSharp.Tests.ImgCodecs.CodecWriterAllocationTests'
$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-codec-writer-allocation-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null
$frameworkResults = [System.Collections.Generic.List[object]]::new()
$previousSmoke = $env:OPENCV_CSHARP_NATIVE_SMOKE
$env:OPENCV_CSHARP_NATIVE_SMOKE = '1'
try {
    foreach ($runtimePackage in $runtimePackages) {
        foreach ($framework in @('net8.0', 'net10.0')) {
            $trxPath = Join-Path $temporaryRoot ("CodecWriterAllocation-$($runtimePackage.Profile)-$framework.trx")
            $arguments = @('test', $project, '-c', 'Release', '-f', $framework, '--no-restore', '--filter', $testFilter,
                ('-p:OpenCvNativeRuntimeDir=' + (Resolve-Path -LiteralPath $runtimePackage.RuntimeDirectory).Path), '--logger', ('trx;LogFileName=' + $trxPath))
            & $dotnet.Source @arguments | Out-Host
            if ($LASTEXITCODE -ne 0) { throw "CodecWriterAllocationTests failed for $($runtimePackage.Profile)/$framework with exit code $LASTEXITCODE." }
            $frameworkResults.Add((Get-TestCounters -Profile $runtimePackage.Profile -Framework $framework -TrxPath $trxPath))
        }
    }
}
finally {
    if ($null -eq $previousSmoke) { Remove-Item Env:OPENCV_CSHARP_NATIVE_SMOKE -ErrorAction SilentlyContinue } else { $env:OPENCV_CSHARP_NATIVE_SMOKE = $previousSmoke }
    if (Test-Path -LiteralPath $temporaryRoot -PathType Container) { Remove-Item -LiteralPath $temporaryRoot -Recurse -Force }
}

$evidence = [ordered]@{
    '$schema' = 'codec-writer-allocation-evidence.schema.json'
    schemaVersion = 1
    status = 'verified'
    sourceCommit = $sourceCommit
    runner = [ordered]@{
        os = [Runtime.InteropServices.RuntimeInformation]::OSDescription
        dotnetSdk = ((& $dotnet.Source --version 2>$null) | Select-Object -First 1).Trim()
        architecture = 'x64'
        configuration = 'Release'
    }
    runtimePackages = @($packageEvidence)
    test = [ordered]@{
        class = 'JYPPX.OpenCvSharp.Tests.ImgCodecs.CodecWriterAllocationTests'
        filter = $testFilter
        nativeSmokeEnabled = $true
        targetFrameworks = @($frameworkResults)
        verifiedCases = @(
            'reused caller-owned writer preserves one segment and payload across 64 encodes',
            'repeated Advance faults do not poison the next native encode'
        )
    }
    limitations = @(
        'Focused allocation/owner/lifetime tests are verified on Windows x64 net8.0 and net10.0 against exact full and mini runtime package payloads.',
        'The tests establish reusable-writer and native-buffer cleanup behavior; they do not claim a cross-runner absolute allocation threshold.',
        'ImEncodeTo remains a conditional preview contract until the broader TFM and stable allocation review is complete.'
    )
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) | Out-Null
[IO.File]::WriteAllText($outputFullPath, (($evidence | ConvertTo-Json -Depth 16) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Host "CODEC_WRITER_ALLOCATION_EVIDENCE_MEASURED source_commit=$sourceCommit profiles=full,mini frameworks=net8.0,net10.0 passed=$(@($frameworkResults | ForEach-Object { $_.passed } | Measure-Object -Sum).Sum) output=$outputFullPath"
