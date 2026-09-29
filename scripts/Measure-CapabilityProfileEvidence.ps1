param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [Parameter(Mandatory)][string]$FullNativeRuntimeDir,
    [Parameter(Mandatory)][string]$MiniNativeRuntimeDir,
    [Parameter(Mandatory)][string]$FullRuntimePackagePath,
    [Parameter(Mandatory)][string]$MiniRuntimePackagePath,
    [string]$OutputPath = 'packaging/runtime/capability-profile-evidence.json'
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

if (-not $IsWindows -or -not [Environment]::Is64BitOperatingSystem) {
    throw 'Capability profile evidence requires a Windows x64 runner.'
}

$status = @(& git -C $repo status --porcelain)
if ($LASTEXITCODE -ne 0) { throw 'Unable to inspect repository status.' }
if ($status.Count -ne 0) { throw 'Capability profile measurement requires a clean repository.' }

$sourceCommit = ([string](& git -C $repo rev-parse HEAD)).Trim().ToLowerInvariant()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Unable to resolve the source commit.' }

function Get-Sha256Bytes {
    param([Parameter(Mandatory)][byte[]]$Bytes)

    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
}

function Get-Sha256File {
    param([Parameter(Mandatory)][string]$Path)

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-JsonSnapshot {
    param([Parameter(Mandatory)][string]$AssemblyPath)

    $workingDirectory = Split-Path -Parent $AssemblyPath
    Push-Location $workingDirectory
    try {
        $lines = @(& $dotnet.Source $AssemblyPath 'capabilities-json' 2>&1)
        if ($LASTEXITCODE -ne 0) {
            throw "capabilities-json failed for $workingDirectory with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }

    $jsonLines = @($lines | ForEach-Object { [string]$_ } | Where-Object {
        $_.TrimStart().StartsWith('{') -and $_.TrimEnd().EndsWith('}')
    })
    if ($jsonLines.Count -ne 1) {
        throw "capabilities-json must emit exactly one JSON object; found $($jsonLines.Count)."
    }

    $json = $jsonLines[0].Trim()
    [pscustomobject]@{
        Json = $json
        Snapshot = ($json | ConvertFrom-Json)
        Sha256 = Get-Sha256Bytes -Bytes ([Text.UTF8Encoding]::new($false).GetBytes($json))
    }
}

function Get-PackagePayload {
    param(
        [Parameter(Mandatory)][string]$PackagePath,
        [Parameter(Mandatory)][string]$RuntimeDirectory,
        [Parameter(Mandatory)][string[]]$ExpectedNames
    )

    $package = (Resolve-Path -LiteralPath $PackagePath).Path
    $runtime = (Resolve-Path -LiteralPath $RuntimeDirectory).Path
    $packageName = [IO.Path]::GetFileName($package).ToLowerInvariant()
    $expectedPackageName = if ($ExpectedNames.Count -eq 18) {
        'jyppx.opencv.runtime.win-x64.5.0.0.nupkg'
    }
    else {
        'jyppx.opencv.runtime.win-x64.mini.5.0.0.nupkg'
    }
    if ($packageName -cne $expectedPackageName) {
        throw "Unexpected runtime package name: $packageName."
    }

    $runtimeFiles = @(Get-ChildItem -LiteralPath $runtime -File -Filter '*.dll' | Sort-Object Name)
    $actualNames = @($runtimeFiles | ForEach-Object { $_.Name } | Sort-Object)
    $sortedExpectedNames = @($ExpectedNames | Sort-Object)
    if (($actualNames -join '|') -cne ($sortedExpectedNames -join '|')) {
        throw "Runtime directory payload does not match the expected $($ExpectedNames.Count)-DLL profile."
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($package)
    try {
        $entries = @($zip.Entries | Where-Object { $_.FullName -like 'runtimes/win-x64/native/*.dll' } | Sort-Object FullName)
        $expectedEntryNames = @($ExpectedNames | ForEach-Object { "runtimes/win-x64/native/$_" } | Sort-Object)
        $entryNames = @($entries | ForEach-Object { $_.FullName } | Sort-Object)
        if (($entryNames -join '|') -cne ($expectedEntryNames -join '|')) {
            throw 'Runtime package native payload does not match the expected profile.'
        }

        $payload = [System.Collections.Generic.List[object]]::new()
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
            $payload.Add([pscustomobject]@{
                name = [IO.Path]::GetFileName($entry.FullName)
                bytes = $bytes.Length
                sha256 = Get-Sha256Bytes -Bytes $bytes
            })
        }
    }
    finally {
        $zip.Dispose()
    }

    $runtimePayload = @($runtimeFiles | ForEach-Object {
        [pscustomobject]@{
            name = $_.Name
            bytes = $_.Length
            sha256 = Get-Sha256File -Path $_.FullName
        }
    })
    foreach ($row in $payload) {
        $runtimeRow = @($runtimePayload | Where-Object { $_.name -ceq $row.name })
        if ($runtimeRow.Count -ne 1 -or $runtimeRow[0].bytes -ne $row.bytes -or $runtimeRow[0].sha256 -cne $row.sha256) {
            throw "Runtime directory and package payload differ for $($row.name)."
        }
    }

    [pscustomobject]@{
        packageFileName = [IO.Path]::GetFileName($package)
        packageSha256 = Get-Sha256File -Path $package
        payload = @($payload)
    }
}

function Get-ProfileRecord {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)]$SnapshotResult
    )

    $snapshot = $SnapshotResult.Snapshot
    [pscustomobject]@{
        name = $Name
        snapshotSha256 = $SnapshotResult.Sha256
        nativeRuntimeState = [string]$snapshot.nativeRuntime.state
        requiredModuleStates = @($snapshot.modules | ForEach-Object { [string]$_.state })
        guiBackendState = [string]$snapshot.guiBackend.state
        dnnBackendStates = @($snapshot.dnnBackends | ForEach-Object { [string]$_.state })
        codecWriterStates = @($snapshot.codecs | ForEach-Object { [string]$_.writer.state })
        optionalModuleCount = @($snapshot.optionalModules).Count
        warningsCount = @($snapshot.warnings).Count
    }
}

$dotnet = Get-Command dotnet -ErrorAction Stop
$dotnetVersion = ([string](& $dotnet.Source --version)).Trim()
if ($LASTEXITCODE -ne 0 -or $dotnetVersion -notmatch '^10\.0\.') { throw "The repository requires the .NET 10 SDK; found $dotnetVersion." }

$project = Join-Path $repo 'samples/ConsoleSamples/ConsoleSamples.csproj'
& $dotnet.Source build $project -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'ConsoleSamples Release build failed.' }
$buildOutput = Join-Path $repo 'samples/ConsoleSamples/bin/Release/net10.0'
$sampleAssembly = Join-Path $buildOutput 'ConsoleSamples.dll'
$managedAssembly = Join-Path $buildOutput 'JYPPX.OpenCV.CSharp.API.dll'
foreach ($path in @($sampleAssembly, $managedAssembly)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Build output is missing: $path" }
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
$fullPayload = Get-PackagePayload -PackagePath $FullRuntimePackagePath -RuntimeDirectory $FullNativeRuntimeDir -ExpectedNames $fullExpectedNames
$miniPayload = Get-PackagePayload -PackagePath $MiniRuntimePackagePath -RuntimeDirectory $MiniNativeRuntimeDir -ExpectedNames $miniExpectedNames

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-capability-profile-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporaryRoot -Force | Out-Null
try {
    $managedFiles = @(Get-ChildItem -LiteralPath $buildOutput -File | Where-Object {
        $_.Name -notmatch '^(JYPPX\.OpenCV\.Native|opencv_).+\.dll$'
    })
    $profileResults = [System.Collections.Generic.List[object]]::new()
    foreach ($profile in @(
        [pscustomobject]@{ Name = 'none'; RuntimeDirectory = $null },
        [pscustomobject]@{ Name = 'full'; RuntimeDirectory = (Resolve-Path -LiteralPath $FullNativeRuntimeDir).Path },
        [pscustomobject]@{ Name = 'mini'; RuntimeDirectory = (Resolve-Path -LiteralPath $MiniNativeRuntimeDir).Path }
    )) {
        $profileDirectory = Join-Path $temporaryRoot $profile.Name
        New-Item -ItemType Directory -Path $profileDirectory -Force | Out-Null
        foreach ($file in $managedFiles) {
            Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $profileDirectory $file.Name) -Force
        }
        if ($null -ne $profile.RuntimeDirectory) {
            Copy-Item -Path (Join-Path $profile.RuntimeDirectory '*.dll') -Destination $profileDirectory -Force
        }
        $profileAssembly = Join-Path $profileDirectory 'ConsoleSamples.dll'
        $snapshotResult = Get-JsonSnapshot -AssemblyPath $profileAssembly
        $profileResults.Add((Get-ProfileRecord -Name $profile.Name -SnapshotResult $snapshotResult))
    }

    $record = [ordered]@{
        '$schema' = 'capability-profile-evidence.schema.json'
        schemaVersion = 1
        status = 'verified'
        sourceCommit = $sourceCommit
        runner = [ordered]@{
            os = [Runtime.InteropServices.RuntimeInformation]::OSDescription
            dotnetSdk = $dotnetVersion
            processArchitecture = [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()
            configuration = 'Release'
            managedAssemblySha256 = Get-Sha256File -Path $managedAssembly
            sampleAssemblySha256 = Get-Sha256File -Path $sampleAssembly
        }
        runtimePackages = @(
            [ordered]@{ profile = 'full'; package = $fullPayload },
            [ordered]@{ profile = 'mini'; package = $miniPayload }
        )
        profiles = @($profileResults)
        boundary = 'Required modules may be Verified in both full and mini; optional modules remain Declared-only, GPU/OpenCL remains Unknown, and profile-specific package evidence is required before promotion.'
    }
    $parent = Split-Path -Parent $outputFullPath
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
    $json = $record | ConvertTo-Json -Depth 20
    [IO.File]::WriteAllText($outputFullPath, $json + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
    Write-Host "CAPABILITY_PROFILE_EVIDENCE_MEASURED source_commit=$sourceCommit profiles=none,full,mini full_dlls=$($fullPayload.payload.Count) mini_dlls=$($miniPayload.payload.Count) output=$outputFullPath"
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot) { Remove-Item -LiteralPath $temporaryRoot -Recurse -Force }
}
