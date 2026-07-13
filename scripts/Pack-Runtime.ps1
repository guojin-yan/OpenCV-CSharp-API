param(
    [string]$Rid = "win-x64",
    [string]$Configuration = "Release",
    [string]$OpenCvVersion = "5.0.0",
    [int]$PackageRevision = 0,
    [string]$PackageVersion = "",
    [string]$OpenCvRuntimeVersionSuffix = "",
    [string]$OpenCvSourceRoot = "",
    [string]$OpenCvInstallRoot = "",
    [string]$OutputDir = "artifacts/packages",
    [string]$StageOutputRoot = "",
    [string]$RuntimeProject = "packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj",
    [string]$RuntimePackageMatrix = "packaging/runtime/runtime-package-matrix.json",
    [string]$RuntimeProfile = "full",
    [string]$OpenCvNativeRuntimeDir = "",
    [string]$NativeRuntimeDir = "",
    [string]$OpenCvRuntimeDir = "",
    [string]$OpenCvInstallDir = "",
    [string]$OpenCvSourceDir = "",
    [string[]]$OpenCvModules = @(),
    [string[]]$OptionalOpenCvModules = @("xfeatures2d", "xobjdetect", "quality", "xphoto", "ml", "img_hash", "ximgproc", "optflow", "bgsegm", "tracking", "face", "saliency", "plot", "shape", "line_descriptor", "phase_unwrapping", "structured_light", "intensity_transform", "fuzzy", "hfs", "reg", "surface_matching", "rapid", "alphamat", "bioinspired", "xstereo"),
    [switch]$StageRuntime,
    [switch]$NoBuild,
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

function Get-RuntimeProfileSpec {
    param(
        [Parameter(Mandatory = $true)]
        [string]$MatrixPath,
        [Parameter(Mandatory = $true)]
        [string]$Profile
    )

    $matrixCandidate = if ([System.IO.Path]::IsPathRooted($MatrixPath)) {
        $MatrixPath
    }
    else {
        Join-Path $repoRoot $MatrixPath
    }

    if (-not (Test-Path -LiteralPath $matrixCandidate -PathType Leaf)) {
        throw "Runtime package matrix was not found: $matrixCandidate"
    }

    $matrix = Get-Content -LiteralPath $matrixCandidate -Raw | ConvertFrom-Json
    $profileSpec = @($matrix.profiles | Where-Object { $_.name -eq $Profile } | Select-Object -First 1)
    if ($profileSpec.Count -eq 0) {
        throw "Runtime profile '$Profile' was not found in runtime package matrix: $matrixCandidate"
    }

    return $profileSpec[0]
}

$profileSpec = Get-RuntimeProfileSpec -MatrixPath $RuntimePackageMatrix -Profile $RuntimeProfile
if (-not $PSBoundParameters.ContainsKey("OpenCvModules")) {
    $OpenCvModules = @($profileSpec.modules)
}

if (-not $PSBoundParameters.ContainsKey("OptionalOpenCvModules")) {
    $OptionalOpenCvModules = @($profileSpec.optionalModules)
}

$runtimePackageSuffix = [string]$profileSpec.packageIdSuffix
$runtimeProjectCandidate = if ([System.IO.Path]::IsPathRooted($RuntimeProject)) {
    $RuntimeProject
}
else {
    Join-Path $repoRoot $RuntimeProject
}

if (-not (Test-Path -LiteralPath $runtimeProjectCandidate -PathType Leaf)) {
    throw "Runtime package project file was not found: $runtimeProjectCandidate"
}

$runtimeProjectFullPath = (Resolve-Path -LiteralPath $runtimeProjectCandidate).Path
$runtimeProjectDirectory = Split-Path -Parent $runtimeProjectFullPath
$outputPathCandidate = if ([System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir
}
else {
    Join-Path $repoRoot $OutputDir
}

$outputFullPath = [System.IO.Path]::GetFullPath($outputPathCandidate)

if ([string]::IsNullOrWhiteSpace($PackageVersion)) {
    # PackageVersion carries OpenCV runtime identity as version metadata; it is not a package ID or naming surface.
    $PackageVersion = "$OpenCvVersion.$PackageRevision"
}

if ($PackageVersion -notmatch '^\d+\.\d+\.\d+\.\d+$') {
    throw "PackageVersion must use four numeric parts, for example 5.0.0.0. Actual: $PackageVersion"
}

if (-not $PackageVersion.StartsWith("$OpenCvVersion.", [System.StringComparison]::Ordinal)) {
    throw "PackageVersion must start with the OpenCV version '$OpenCvVersion.'. Actual: $PackageVersion"
}

function Get-NuGetPackageFileVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    $parts = @($Version.Split(".") | ForEach-Object { [int]$_ })
    if ($parts.Count -ne 4) {
        throw "PackageVersion must use four numeric parts, for example 5.0.0.0. Actual: $Version"
    }

    if ($parts[3] -eq 0) {
        return "{0}.{1}.{2}" -f $parts[0], $parts[1], $parts[2]
    }

    return "{0}.{1}.{2}.{3}" -f $parts[0], $parts[1], $parts[2], $parts[3]
}

if ($StageRuntime) {
    if ([string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir)) {
        # OpenCvNativeRuntimeDir is the preferred version-neutral runtime path/staging parameter.
        # NativeRuntimeDir is accepted only as an older existing-packaging-script compatibility alias.
        $OpenCvNativeRuntimeDir = $NativeRuntimeDir
    }

    if ([string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir)) {
        throw "OpenCvNativeRuntimeDir is required when StageRuntime is set. The older NativeRuntimeDir parameter remains accepted only as an existing-packaging-script compatibility alias."
    }

    $stageParameters = @{
        Rid = $Rid
        Configuration = $Configuration
        OpenCvNativeRuntimeDir = $OpenCvNativeRuntimeDir
        OpenCvVersion = $OpenCvVersion
        RuntimeProject = $runtimeProjectDirectory
        RuntimePackageMatrix = $RuntimePackageMatrix
        RuntimeProfile = $RuntimeProfile
        OpenCvModules = $OpenCvModules
        OptionalOpenCvModules = $OptionalOpenCvModules
    }

    if (-not [string]::IsNullOrWhiteSpace($StageOutputRoot)) {
        $stageParameters.OutputRoot = $StageOutputRoot
    }

    if (-not [string]::IsNullOrWhiteSpace($OpenCvRuntimeDir)) {
        $stageParameters.OpenCvRuntimeDir = $OpenCvRuntimeDir
    }

    if (-not [string]::IsNullOrWhiteSpace($OpenCvRuntimeVersionSuffix)) {
        $stageParameters.OpenCvRuntimeVersionSuffix = $OpenCvRuntimeVersionSuffix
    }

    if (-not [string]::IsNullOrWhiteSpace($OpenCvSourceRoot)) {
        $stageParameters.OpenCvSourceRoot = $OpenCvSourceRoot
    }

    if (-not [string]::IsNullOrWhiteSpace($OpenCvInstallRoot)) {
        $stageParameters.OpenCvInstallRoot = $OpenCvInstallRoot
    }

    if (-not [string]::IsNullOrWhiteSpace($OpenCvInstallDir)) {
        $stageParameters.OpenCvInstallDir = $OpenCvInstallDir
    }

    if (-not [string]::IsNullOrWhiteSpace($OpenCvSourceDir)) {
        $stageParameters.OpenCvSourceDir = $OpenCvSourceDir
    }

    try {
        & (Join-Path $PSScriptRoot "Stage-Runtime.ps1") @stageParameters
    }
    catch {
        throw "Stage-Runtime.ps1 failed: $($_.Exception.Message)"
    }
}

New-Item -ItemType Directory -Force $outputFullPath | Out-Null

$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$runtimePackageId = "$runtimePackagePrefix.$Rid$runtimePackageSuffix"
$packageFileVersion = Get-NuGetPackageFileVersion -Version $PackageVersion
$packagePath = Join-Path $outputFullPath "$runtimePackageId.$packageFileVersion.nupkg"
if (Test-Path -LiteralPath $packagePath) {
    Remove-Item -LiteralPath $packagePath -Force
}

$arguments = @(
    "pack",
    $runtimeProjectFullPath,
    "-c",
    $Configuration,
    "-o",
    $outputFullPath,
    "-p:RuntimePackageRid=$Rid",
    "-p:RuntimePackageProfile=$RuntimeProfile",
    "-p:Version=$PackageVersion",
    "-p:PackageVersion=$PackageVersion",
    "-p:PackageId=$runtimePackageId"
)

if ($NoBuild) {
    $arguments += "--no-build"
}

if ($NoRestore) {
    $arguments += "--no-restore"
}

Write-Host "Packing runtime package $runtimePackageId $PackageVersion ($RuntimeProfile)"
& dotnet @arguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet pack failed with exit code $LASTEXITCODE"
}

if (-not (Test-Path -LiteralPath $packagePath)) {
    throw "Runtime package artifact was not found: $packagePath"
}

Write-Host "Runtime package output directory: $outputFullPath"
Write-Host "Runtime package artifact: $packagePath"
