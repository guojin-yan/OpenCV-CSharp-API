[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$RuntimeIdentifier = 'win-x64',
    [Parameter(Mandatory)][string]$OpenCvNativeRuntimeDir,
    [Parameter(Mandatory)][string]$NativeRuntimePackagePath,
    [string]$OutputPath = 'packaging/performance/native-aot-smoke-evidence.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
if (-not [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::Windows) -or
    [Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [Runtime.InteropServices.Architecture]::X64) {
    throw 'NativeAOT smoke evidence requires a Windows x64 runner.'
}
if ($RuntimeIdentifier -cne 'win-x64') { throw 'Only win-x64 native AOT smoke evidence is currently supported.' }

$status = @(& git -C $repo status --porcelain 2>$null)
if ($LASTEXITCODE -ne 0 -or $status.Count -ne 0) { throw 'NativeAOT smoke measurement requires a clean repository.' }
$sourceCommit = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim().ToLowerInvariant()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Unable to resolve the source commit.' }

function Get-Sha256File {
    param([Parameter(Mandatory)][string]$Path)
    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-Sha256Bytes {
    param([Parameter(Mandatory)][byte[]]$Bytes)
    [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
}

function Resolve-OutputPath {
    param([Parameter(Mandatory)][string]$Path)
    if ([IO.Path]::IsPathRooted($Path)) { return [IO.Path]::GetFullPath($Path) }
    return [IO.Path]::GetFullPath((Join-Path $repo ($Path -replace '/', [IO.Path]::DirectorySeparatorChar)))
}

$runtime = (Resolve-Path -LiteralPath $OpenCvNativeRuntimeDir).Path
$package = (Resolve-Path -LiteralPath $NativeRuntimePackagePath).Path
$packageFileName = [IO.Path]::GetFileName($package)
if ($packageFileName.ToLowerInvariant() -cne 'jyppx.opencv.runtime.win-x64.5.0.0.nupkg') {
    throw "Unexpected native runtime package: $packageFileName"
}

$dotnet = Get-Command dotnet -ErrorAction Stop
$pwsh = Get-Command pwsh -ErrorAction Stop
$smokeScript = Join-Path $repo 'scripts/Test-NativeAotSmoke.ps1'
$smokeLines = @(& $pwsh.Source -NoProfile -File $smokeScript -RepositoryRoot $repo -RuntimeIdentifier $RuntimeIdentifier -OpenCvNativeRuntimeDir $runtime 2>&1)
$smokeExitCode = $LASTEXITCODE
$smokeLines | ForEach-Object { Write-Host ([string]$_) }
if ($smokeExitCode -ne 0) { throw "NativeAOT smoke failed with exit code $smokeExitCode." }

$nativeLine = @($smokeLines | ForEach-Object { [string]$_ } | Where-Object { $_ -match '^NATIVE_AOT_SMOKE_OK ' } | Select-Object -Last 1)
if ($nativeLine.Count -ne 1 -or $nativeLine[0] -notmatch '^NATIVE_AOT_SMOKE_OK rid=(?<rid>\S+) files=(?<files>\d+) native_payload=(?<payload>\d+) native_runtime_supplied=(?<supplied>True|False)$') {
    throw 'NativeAOT smoke summary was missing or malformed.'
}
if ($Matches.rid -cne $RuntimeIdentifier -or -not [bool]::Parse($Matches.supplied)) { throw 'NativeAOT smoke did not verify the supplied native runtime.' }
$publishedFileCount = [int]$Matches.files
$nativePayloadFileCount = [int]$Matches.payload
$encodedLines = @($smokeLines | ForEach-Object { [string]$_ } | Where-Object { $_ -match '^encoded_bytes=(?<bytes>\d+)$' } | Select-Object -Last 1)
if ($encodedLines.Count -ne 1 -or $encodedLines[0] -notmatch '^encoded_bytes=(?<bytes>\d+)$') { throw 'NativeAOT smoke did not report encoded byte output.' }
$encodedBytes = [int]$Matches.bytes
if ($encodedBytes -le 0 -or @($smokeLines | ForEach-Object { [string]$_ } | Where-Object { $_ -ceq 'native_smoke=verified' }).Count -ne 1) {
    throw 'NativeAOT native smoke output was not verified.'
}
$dnnAvailable = @($smokeLines | ForEach-Object { [string]$_ } | Where-Object { $_ -ceq 'dnn_cpu_available=true' })
$dnnTargetLines = @($smokeLines | ForEach-Object { [string]$_ } | Where-Object { $_ -match '^dnn_target_count=(?<count>\d+)$' })
if ($dnnAvailable.Count -ne 1 -or $dnnTargetLines.Count -ne 1 -or $dnnTargetLines[0] -notmatch '^dnn_target_count=(?<count>\d+)$' -or [int]$Matches.count -lt 1) {
    throw 'NativeAOT optional DNN module probe did not report an available CPU target.'
}
$dnnTargetCount = [int]$Matches.count

$runtimeFiles = @(Get-ChildItem -LiteralPath $runtime -File -Filter '*.dll' | Sort-Object Name)
if ($runtimeFiles.Count -ne 18) { throw "Expected 18 full runtime DLLs, found $($runtimeFiles.Count)." }
$expectedNames = @($runtimeFiles | ForEach-Object { $_.Name } | Sort-Object)

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($package)
try {
    $entries = @($archive.Entries | Where-Object { $_.FullName -like 'runtimes/win-x64/native/*.dll' } | Sort-Object FullName)
    $entryNames = @($entries | ForEach-Object { [IO.Path]::GetFileName($_.FullName) } | Sort-Object)
    if (($entryNames -join '|') -cne ($expectedNames -join '|')) { throw 'Native runtime directory and package DLL names differ.' }
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
        throw "Native runtime directory and package payload differ at $($packageRow.name)."
    }
}

$managedAssembly = Join-Path $repo 'src/OpenCvSharp/bin/Release/net10.0/JYPPX.OpenCV.CSharp.API.dll'
if (-not (Test-Path -LiteralPath $managedAssembly -PathType Leaf)) { throw "Managed assembly is missing: $managedAssembly" }
$packageSha256 = Get-Sha256File -Path $package
$evidence = [ordered]@{
    '$schema' = 'native-aot-smoke-evidence.schema.json'
    schemaVersion = 1
    status = 'verified'
    sourceCommit = $sourceCommit
    runner = [ordered]@{
        os = [Runtime.InteropServices.RuntimeInformation]::OSDescription
        architecture = 'x64'
        dotnet = ((& $dotnet.Source --version 2>$null) | Select-Object -First 1).Trim()
        configuration = 'Release'
    }
    runtimeIdentifier = $RuntimeIdentifier
    targetFramework = 'net10.0'
    sdk = ((& $dotnet.Source --version 2>$null) | Select-Object -First 1).Trim()
    configuration = 'Release'
    publishAot = $true
    publishTrimmed = $true
    trimMode = 'partial'
    publishSingleFile = $true
    nativeRuntimeArtifact = "local-package-cache:$packageFileName"
    nativeRuntimeArtifactSha256 = $packageSha256
    nativeRuntimePackage = $packageFileName
    nativeRuntimePackageSha256 = $packageSha256
    managedAssemblySha256 = Get-Sha256File -Path $managedAssembly
    managedPackageVersion = '5.0.0'
    openCvVersion = '5.0.0'
    nativeAbiVersion = 1
    publishedFileCount = $publishedFileCount
    nativePayloadFileCount = $nativePayloadFileCount
    nativeSmoke = 'verified'
    encodedBytes = $encodedBytes
    dnnCpuAvailable = $true
    dnnTargetCount = $dnnTargetCount
    nativePayload = @($packagePayload)
    limitations = @(
        'This record binds a current-source Windows x64 NativeAOT consumer run to the exact factual full runtime package and all 18 native DLL hashes.',
        'The win-arm64 linker and native runtime evidence remain pending until a matching Visual Studio ARM64 toolchain and runtime payload are available.',
        'This smoke does not promote package-wide IsAotCompatible or IsTrimmable metadata.'
    )
}

$outputFullPath = Resolve-OutputPath -Path $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) | Out-Null
[IO.File]::WriteAllText($outputFullPath, (($evidence | ConvertTo-Json -Depth 12) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Host "NATIVE_AOT_SMOKE_EVIDENCE_MEASURED source_commit=$sourceCommit package_sha256=$packageSha256 output=$outputFullPath"
