param(
    [string]$Rid = "linux-x64",
    [string]$RuntimeProfile = "full",
    [string]$OpenCvVersion = "5.0.0",
    [string]$NativeRuntimeDir,
    [string]$OpenCvRuntimeDir,
    [string]$OpenCvSourceDir,
    [string]$OpenCvInstallDir = "",
    [string]$OutputRoot = "artifacts/runtime-inputs",
    [string]$RuntimePackageMatrix = "packaging/runtime/runtime-package-matrix.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path

function Resolve-InputDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "$Name is required to create a runtime-input artifact."
    }

    $candidate = if ([System.IO.Path]::IsPathRooted($Value)) {
        $Value
    }
    else {
        Join-Path $repoRoot $Value
    }

    if (-not (Test-Path -LiteralPath $candidate -PathType Container)) {
        throw "$Name directory was not found: $candidate"
    }

    return (Resolve-Path -LiteralPath $candidate).Path
}

function Get-RuntimeMatrix {
    param([Parameter(Mandatory = $true)][string]$MatrixPath)

    $matrixCandidate = if ([System.IO.Path]::IsPathRooted($MatrixPath)) {
        $MatrixPath
    }
    else {
        Join-Path $repoRoot $MatrixPath
    }

    if (-not (Test-Path -LiteralPath $matrixCandidate -PathType Leaf)) {
        throw "Runtime package matrix was not found: $matrixCandidate"
    }

    return Get-Content -LiteralPath $matrixCandidate -Raw | ConvertFrom-Json
}

function Test-WindowsRid {
    param([Parameter(Mandatory = $true)][string]$RuntimeIdentifier)
    return $RuntimeIdentifier.StartsWith("win-", [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-NativeLoaderFileNames {
    param([Parameter(Mandatory = $true)][string]$RuntimeIdentifier)

    $compatibilityNativeLoaderBaseName = "Open" + "Cv5Sharp.Native" # compatibility loader for already-compiled consumers
    if (Test-WindowsRid -RuntimeIdentifier $RuntimeIdentifier) {
        return @("JYPPX.OpenCV.Native.dll", "$compatibilityNativeLoaderBaseName.dll")
    }

    return @("libJYPPX.OpenCV.Native.so", "lib$compatibilityNativeLoaderBaseName.so")
}

function Resolve-OpenCvModuleRuntimeFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RuntimeDirectory,
        [Parameter(Mandatory = $true)]
        [string]$Module,
        [Parameter(Mandatory = $true)]
        [string]$RuntimeIdentifier,
        [Parameter(Mandatory = $true)]
        [string]$BinarySuffix,
        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    if (Test-WindowsRid -RuntimeIdentifier $RuntimeIdentifier) {
        return Join-Path $RuntimeDirectory "opencv_$Module$BinarySuffix.dll"
    }

    $candidates = @(
        (Join-Path $RuntimeDirectory "libopencv_$Module.so"),
        (Join-Path $RuntimeDirectory "libopencv_$Module.so.$Version")
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return $candidate
        }
    }

    $globbed = @(Get-ChildItem -LiteralPath $RuntimeDirectory -Filter "libopencv_$Module.so*" -File -ErrorAction SilentlyContinue | Sort-Object Name | Select-Object -First 1)
    if ($globbed.Count -gt 0) {
        return $globbed[0].FullName
    }

    return $candidates[1]
}

function Copy-RequiredFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Source,
        [Parameter(Mandatory = $true)]
        [string]$DestinationDirectory,
        [Parameter(Mandatory = $true)]
        [System.Collections.Generic.List[object]]$Entries
    )

    if (-not (Test-Path -LiteralPath $Source -PathType Leaf)) {
        throw "Runtime input file was not found: $Source"
    }

    New-Item -ItemType Directory -Force -Path $DestinationDirectory | Out-Null
    $destination = Join-Path $DestinationDirectory (Split-Path -Leaf $Source)
    Copy-Item -LiteralPath $Source -Destination $destination -Force
    $Entries.Add([pscustomobject]@{
        FileName = Split-Path -Leaf $Source
        SourcePath = [System.IO.Path]::GetFullPath($Source)
        ArtifactPath = ([System.IO.Path]::GetRelativePath($artifactRootFullPath, $destination) -replace "\\", "/")
    })
}

$matrix = Get-RuntimeMatrix -MatrixPath $RuntimePackageMatrix
$ridSpec = @($matrix.rids | Where-Object { $_.rid -eq $Rid } | Select-Object -First 1)
if ($ridSpec.Count -eq 0) {
    throw "RID '$Rid' was not found in runtime package matrix."
}

$profileSpec = @($matrix.profiles | Where-Object { $_.name -eq $RuntimeProfile } | Select-Object -First 1)
if ($profileSpec.Count -eq 0) {
    throw "Runtime profile '$RuntimeProfile' was not found in runtime package matrix."
}

$nativeRuntimePath = Resolve-InputDirectory -Name "NativeRuntimeDir" -Value $NativeRuntimeDir
$openCvRuntimePath = Resolve-InputDirectory -Name "OpenCvRuntimeDir" -Value $OpenCvRuntimeDir
$openCvSourcePath = Resolve-InputDirectory -Name "OpenCvSourceDir" -Value $OpenCvSourceDir
$openCvInstallPath = if ([string]::IsNullOrWhiteSpace($OpenCvInstallDir)) { "" } else { Resolve-InputDirectory -Name "OpenCvInstallDir" -Value $OpenCvInstallDir }

$outputRootCandidate = if ([System.IO.Path]::IsPathRooted($OutputRoot)) {
    $OutputRoot
}
else {
    Join-Path $repoRoot $OutputRoot
}

$artifactRootFullPath = [System.IO.Path]::GetFullPath((Join-Path $outputRootCandidate "$Rid-$RuntimeProfile"))
$nativeArtifactDir = Join-Path $artifactRootFullPath "native-wrapper"
$openCvRuntimeArtifactDir = Join-Path $artifactRootFullPath "opencv-runtime"
$openCvSourceArtifactDir = Join-Path $artifactRootFullPath "opencv-source"
$openCvInstallArtifactDir = Join-Path $artifactRootFullPath "opencv-install"

if (Test-Path -LiteralPath $artifactRootFullPath -PathType Container) {
    Remove-Item -LiteralPath $artifactRootFullPath -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $nativeArtifactDir, $openCvRuntimeArtifactDir, $openCvSourceArtifactDir | Out-Null

$runtimeEntries = [System.Collections.Generic.List[object]]::new()
$nativeEntries = [System.Collections.Generic.List[object]]::new()
$licenseEntries = [System.Collections.Generic.List[object]]::new()

foreach ($loader in (Get-NativeLoaderFileNames -RuntimeIdentifier $Rid)) {
    Copy-RequiredFile -Source (Join-Path $nativeRuntimePath $loader) -DestinationDirectory $nativeArtifactDir -Entries $nativeEntries
}

$openCvBinarySuffix = (($OpenCvVersion -split "\.") | Select-Object -First 3) -join ""
foreach ($module in @($profileSpec.modules)) {
    if ([string]::IsNullOrWhiteSpace([string]$module)) {
        continue
    }

    $moduleFile = Resolve-OpenCvModuleRuntimeFile `
        -RuntimeDirectory $openCvRuntimePath `
        -Module ([string]$module) `
        -RuntimeIdentifier $Rid `
        -BinarySuffix $openCvBinarySuffix `
        -Version $OpenCvVersion
    Copy-RequiredFile -Source $moduleFile -DestinationDirectory $openCvRuntimeArtifactDir -Entries $runtimeEntries
}

$runtimeCopyPatterns = if (Test-WindowsRid -RuntimeIdentifier $Rid) {
    @("opencv*.dll")
}
else {
    @("libopencv_*.so*")
}

foreach ($pattern in $runtimeCopyPatterns) {
    foreach ($file in @(Get-ChildItem -LiteralPath $openCvRuntimePath -Filter $pattern -File -ErrorAction SilentlyContinue)) {
        $destination = Join-Path $openCvRuntimeArtifactDir $file.Name
        if (-not (Test-Path -LiteralPath $destination -PathType Leaf)) {
            Copy-Item -LiteralPath $file.FullName -Destination $destination -Force
            $runtimeEntries.Add([pscustomobject]@{
                FileName = $file.Name
                SourcePath = $file.FullName
                ArtifactPath = ([System.IO.Path]::GetRelativePath($artifactRootFullPath, $destination) -replace "\\", "/")
            })
        }
    }
}

$openCvLicense = Join-Path $openCvSourcePath "LICENSE"
if (-not (Test-Path -LiteralPath $openCvLicense -PathType Leaf)) {
    throw "OpenCV source LICENSE was not found: $openCvLicense"
}

Copy-Item -LiteralPath $openCvLicense -Destination (Join-Path $openCvSourceArtifactDir "LICENSE") -Force
$licenseEntries.Add([pscustomobject]@{
    FileName = "LICENSE"
    SourcePath = [System.IO.Path]::GetFullPath($openCvLicense)
    ArtifactPath = "opencv-source/LICENSE"
})

$ippicvReadme = Join-Path (Join-Path (Join-Path $openCvSourcePath "3rdparty") "ippicv") "readme.htm"
if (Test-Path -LiteralPath $ippicvReadme -PathType Leaf) {
    $ippicvArtifactDir = Join-Path (Join-Path $openCvSourceArtifactDir "3rdparty") "ippicv"
    New-Item -ItemType Directory -Force -Path $ippicvArtifactDir | Out-Null
    Copy-Item -LiteralPath $ippicvReadme -Destination (Join-Path $ippicvArtifactDir "readme.htm") -Force
    $licenseEntries.Add([pscustomobject]@{
        FileName = "readme.htm"
        SourcePath = [System.IO.Path]::GetFullPath($ippicvReadme)
        ArtifactPath = "opencv-source/3rdparty/ippicv/readme.htm"
    })
}

if (-not [string]::IsNullOrWhiteSpace($openCvInstallPath)) {
    $installLicenseDir = Join-Path $openCvInstallPath "etc/licenses"
    if (Test-Path -LiteralPath $installLicenseDir -PathType Container) {
        $installArtifactLicenseDir = Join-Path $openCvInstallArtifactDir "etc/licenses"
        New-Item -ItemType Directory -Force -Path $installArtifactLicenseDir | Out-Null
        foreach ($file in @(Get-ChildItem -LiteralPath $installLicenseDir -File -ErrorAction SilentlyContinue)) {
            Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $installArtifactLicenseDir $file.Name) -Force
            $licenseEntries.Add([pscustomobject]@{
                FileName = $file.Name
                SourcePath = $file.FullName
                ArtifactPath = "opencv-install/etc/licenses/$($file.Name)"
            })
        }
    }
}

$manifest = [ordered]@{
    SchemaVersion = 1
    Rid = $Rid
    OpenCvRid = [string]$ridSpec[0].opencvRid
    RuntimeProfile = $RuntimeProfile
    OpenCvVersion = $OpenCvVersion
    SyntheticRuntimeInputs = $false
    ArtifactLayout = [ordered]@{
        NativeWrapper = "native-wrapper"
        OpenCvRuntime = "opencv-runtime"
        OpenCvSource = "opencv-source"
        OpenCvInstall = if ([string]::IsNullOrWhiteSpace($openCvInstallPath)) { "" } else { "opencv-install" }
    }
    RequiredModules = @($profileSpec.modules)
    InputRoots = [ordered]@{
        NativeRuntimeDir = $nativeRuntimePath
        OpenCvRuntimeDir = $openCvRuntimePath
        OpenCvSourceDir = $openCvSourcePath
        OpenCvInstallDir = $openCvInstallPath
    }
    NativeLoaderFiles = @($nativeEntries)
    RuntimeFiles = @($runtimeEntries)
    LicenseFiles = @($licenseEntries)
}

$manifestPath = Join-Path $artifactRootFullPath "runtime-input.provenance.json"
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($manifestPath, (($manifest | ConvertTo-Json -Depth 8) + [System.Environment]::NewLine), $utf8NoBom)

Write-Host "Runtime input artifact directory: $artifactRootFullPath"
Write-Host "Runtime input artifact name: runtime-input-$Rid-$RuntimeProfile"
Write-Host "Runtime input provenance: $manifestPath"
