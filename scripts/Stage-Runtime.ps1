param(
    [string]$Rid = "win-x64",
    [string]$Configuration = "Release",
    [string]$OpenCvNativeRuntimeDir = "",
    [string]$NativeRuntimeDir = "",
    [string]$OpenCvVersion = "5.0.0",
    [string]$OpenCvRid = "windows-x64",
    [string]$OpenCvRuntimeVersionSuffix = "",
    [string]$OpenCvSourceRoot = "",
    [string]$OpenCvInstallRoot = "",
    [string]$OpenCvRuntimeDir = "",
    [string]$OpenCvInstallDir = "",
    [string]$OpenCvSourceDir = "",
    [string]$OutputRoot = "artifacts\runtime",
    [string]$RuntimeProject = "packaging\runtime\JYPPX.OpenCV.runtime.win-x64",
    [string[]]$OpenCvModules = @("core", "imgcodecs", "imgproc", "videoio", "flann", "geometry", "calib", "stereo", "dnn", "objdetect", "photo", "features", "video", "highgui", "stitching", "ptcloud"),
    [string[]]$OptionalOpenCvModules = @("xfeatures2d", "xobjdetect", "quality", "xphoto", "ml", "img_hash", "ximgproc", "optflow", "bgsegm", "tracking", "face", "saliency", "plot", "shape", "line_descriptor", "phase_unwrapping", "structured_light", "intensity_transform", "fuzzy", "hfs", "reg", "surface_matching", "rapid", "alphamat", "bioinspired", "xstereo")
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")
$workspaceRoot = Resolve-Path -LiteralPath (Join-Path $repoRoot "..")

function Get-DefaultOpenCvSourceRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$WorkspaceRoot,
        [Parameter(Mandatory = $true)]
        [string]$OpenCvVersion
    )

    # Prefer the version-neutral workspace source root when it exists.
    $neutralSourceRoot = Join-Path $WorkspaceRoot "opencv-source"
    if (Test-Path -LiteralPath $neutralSourceRoot) {
        return $neutralSourceRoot
    }

    $versionMatch = [regex]::Match($OpenCvVersion, "^(\d+)(?:\.|$)")
    if (-not $versionMatch.Success) {
        throw "OpenCvVersion must start with a numeric major version: $OpenCvVersion"
    }

    # Keep the major-version source directory only as a current local fallback.
    return Join-Path $WorkspaceRoot "opencv$($versionMatch.Groups[1].Value)-source code"
}

if ([string]::IsNullOrWhiteSpace($OpenCvRuntimeVersionSuffix)) {
    # The suffix carries factual local runtime artifact identity, not a package ID or generic project naming surface.
    $OpenCvRuntimeVersionSuffix = "$OpenCvVersion-$OpenCvRid"
}

if ([string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir)) {
    if (-not [string]::IsNullOrWhiteSpace($NativeRuntimeDir)) {
        # OpenCvNativeRuntimeDir is the preferred version-neutral runtime path/staging parameter.
        # NativeRuntimeDir is accepted only as an older existing-packaging-script compatibility alias.
        $OpenCvNativeRuntimeDir = $NativeRuntimeDir
    }
    else {
        # Default native runtime input path is a current local build-output fallback; it is not a runtime package identity or naming surface.
        $OpenCvNativeRuntimeDir = "build\native-opencv-core\Release"
    }
}

if ([string]::IsNullOrWhiteSpace($OpenCvSourceRoot)) {
    $OpenCvSourceRoot = Get-DefaultOpenCvSourceRoot -WorkspaceRoot $workspaceRoot -OpenCvVersion $OpenCvVersion
}

if ([string]::IsNullOrWhiteSpace($OpenCvInstallRoot)) {
    $OpenCvInstallRoot = Join-Path $workspaceRoot "artifacts\opencv-install"
}

# OutputRoot is the version-neutral runtime staging-output root.
# Its default remains the existing artifacts\runtime compatibility directory.
$outputRootCandidate = if ([System.IO.Path]::IsPathRooted($OutputRoot)) {
    $OutputRoot
}
else {
    Join-Path $repoRoot $OutputRoot
}

$outputRootFullPath = [System.IO.Path]::GetFullPath($outputRootCandidate)

$runtimeProjectRootCandidate = if ([System.IO.Path]::IsPathRooted($RuntimeProject)) {
    $RuntimeProject
}
else {
    Join-Path $repoRoot $RuntimeProject
}

$runtimeProjectRootFullPath = [System.IO.Path]::GetFullPath($runtimeProjectRootCandidate)

function Resolve-RepoPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if ([System.IO.Path]::IsPathFullyQualified($Path)) {
        return Resolve-Path -LiteralPath $Path
    }

    return Resolve-Path -LiteralPath (Join-Path $repoRoot $Path)
}

if ([string]::IsNullOrWhiteSpace($OpenCvSourceDir)) {
    # Upstream OpenCV source leaf directory includes the selected version as factual source artifact identity.
    $OpenCvSourceDir = Join-Path $OpenCvSourceRoot "opencv-$OpenCvVersion"
}

if ([string]::IsNullOrWhiteSpace($OpenCvInstallDir)) {
    # Local OpenCV install leaf directory carries selected runtime artifact identity, not a package ID or generic naming surface.
    $defaultOpenCvInstallDir = Join-Path $OpenCvInstallRoot "opencv-$OpenCvRuntimeVersionSuffix"
    if (Test-Path -LiteralPath $defaultOpenCvInstallDir) {
        $OpenCvInstallDir = $defaultOpenCvInstallDir
    }
}

if ([string]::IsNullOrWhiteSpace($OpenCvRuntimeDir)) {
    if (-not [string]::IsNullOrWhiteSpace($OpenCvInstallDir)) {
        $resolvedInstallDir = (Resolve-Path -LiteralPath $OpenCvInstallDir).Path
        # Derived only for factual upstream OpenCV runtime DLL probe names such as opencv_core500.dll.
        $openCvBinarySuffix = (($OpenCvVersion -split "\.") | Select-Object -First 3) -join ""
        $installRuntimeCandidates = @(
            (Join-Path $resolvedInstallDir "bin"),
            (Join-Path $resolvedInstallDir "bin\$Configuration"),
            (Join-Path $resolvedInstallDir "x64\vc18\bin"),
            (Join-Path $resolvedInstallDir "x64\vc18\bin\$Configuration")
        )

        foreach ($candidate in $installRuntimeCandidates) {
            $coreCandidate = Join-Path $candidate "opencv_core$openCvBinarySuffix.dll"
            if (Test-Path -LiteralPath $coreCandidate) {
                $OpenCvRuntimeDir = (Resolve-Path -LiteralPath $candidate).Path
                break
            }
        }

        if ([string]::IsNullOrWhiteSpace($OpenCvRuntimeDir)) {
            $installLeafName = Split-Path $resolvedInstallDir -Leaf
            if ($installLeafName.StartsWith("opencv-$OpenCvVersion-", [System.StringComparison]::OrdinalIgnoreCase)) {
                # Reuse the factual local build leaf that matches the selected install artifact identity.
                $OpenCvRuntimeDir = Join-Path $workspaceRoot "artifacts\opencv-build\$installLeafName\bin\$Configuration"
            }
        }
    }

    if ([string]::IsNullOrWhiteSpace($OpenCvRuntimeDir)) {
        # Default local OpenCV build runtime path is factual artifact metadata derived from version-neutral parameters.
        $OpenCvRuntimeDir = Join-Path $workspaceRoot "artifacts\opencv-build\opencv-$OpenCvRuntimeVersionSuffix\bin\$Configuration"
    }
}

if ([string]::IsNullOrWhiteSpace($OpenCvInstallDir)) {
    $runtimeCandidate = Resolve-Path -LiteralPath $OpenCvRuntimeDir
    $openCvInstallCandidates = @(
        (Join-Path $runtimeCandidate "..\..\.."),
        (Join-Path $runtimeCandidate "..\.."),
        (Join-Path $runtimeCandidate "..")
    )

    foreach ($candidate in $openCvInstallCandidates) {
        $licenseCandidate = Join-Path $candidate "etc\licenses"
        if (Test-Path -LiteralPath $licenseCandidate) {
            $OpenCvInstallDir = (Resolve-Path -LiteralPath $candidate).Path
            break
        }
    }
}

$nativeRuntimePath = Resolve-RepoPath $OpenCvNativeRuntimeDir
$openCvRuntimePath = Resolve-Path -LiteralPath $OpenCvRuntimeDir
$openCvSourcePath = Resolve-Path -LiteralPath $OpenCvSourceDir
$openCvInstallPath = if ([string]::IsNullOrWhiteSpace($OpenCvInstallDir)) { $null } else { Resolve-Path -LiteralPath $OpenCvInstallDir }
$stagingNativeDir = Join-Path $outputRootFullPath (Join-Path $Rid "native")
$runtimeProjectNativeDir = Join-Path $runtimeProjectRootFullPath (Join-Path "runtimes\$Rid" "native")
$runtimeProjectLicenseDir = Join-Path $runtimeProjectRootFullPath "licenses"
$runtimeProjectOpenCvLicenseDir = Join-Path $runtimeProjectLicenseDir "opencv-3rdparty"

New-Item -ItemType Directory -Force $stagingNativeDir | Out-Null
New-Item -ItemType Directory -Force $runtimeProjectNativeDir | Out-Null
New-Item -ItemType Directory -Force $runtimeProjectLicenseDir | Out-Null
New-Item -ItemType Directory -Force $runtimeProjectOpenCvLicenseDir | Out-Null

# Regenerate staging mirrors from the current runtime inputs only. This avoids
# preserving stale DLLs, license files, or nested generated content when modules
# disappear between builds.
Get-ChildItem -LiteralPath $stagingNativeDir -Force | Remove-Item -Recurse -Force
Get-ChildItem -LiteralPath $runtimeProjectNativeDir -Force | Remove-Item -Recurse -Force
Get-ChildItem -LiteralPath $runtimeProjectLicenseDir -Force | Remove-Item -Recurse -Force
New-Item -ItemType Directory -Force $runtimeProjectOpenCvLicenseDir | Out-Null

# Derived only for factual upstream OpenCV runtime DLL names such as opencv_core500.dll.
$openCvBinarySuffix = (($OpenCvVersion -split "\.") | Select-Object -First 3) -join ""
# JYPPX.OpenCV.Native.dll is the version-neutral primary loader.
# OpenCv5Sharp.Native.dll remains a compatibility loader copy for already-compiled consumers.
$primaryNativeLoaderFileName = "JYPPX.OpenCV.Native.dll"
$compatibilityNativeLoaderCopyFileName = "OpenCv5Sharp.Native.dll"
$runtimeFiles = @(
    (Join-Path $nativeRuntimePath $primaryNativeLoaderFileName),
    (Join-Path $nativeRuntimePath $compatibilityNativeLoaderCopyFileName)
)

foreach ($module in $OpenCvModules) {
    if ([string]::IsNullOrWhiteSpace($module)) {
        continue
    }

    $runtimeFiles += (Join-Path $openCvRuntimePath "opencv_$module$openCvBinarySuffix.dll")
}

$optionalRuntimeFiles = @()
foreach ($module in $OptionalOpenCvModules) {
    if ([string]::IsNullOrWhiteSpace($module)) {
        continue
    }

    $optionalFile = Join-Path $openCvRuntimePath "opencv_$module$openCvBinarySuffix.dll"
    if (Test-Path -LiteralPath $optionalFile) {
        $optionalRuntimeFiles += $optionalFile
    }
    else {
        Write-Warning "Optional OpenCV runtime module was not found and will be skipped: $optionalFile"
    }
}

$runtimeFiles += $optionalRuntimeFiles

foreach ($file in $runtimeFiles) {
    if (-not (Test-Path -LiteralPath $file)) {
        throw "Runtime file was not found: $file"
    }

    Copy-Item -LiteralPath $file -Destination $stagingNativeDir -Force
    Copy-Item -LiteralPath $file -Destination $runtimeProjectNativeDir -Force
}

Write-Host "Copied runtime files:"
foreach ($file in $runtimeFiles) {
    Write-Host (" - " + (Split-Path $file -Leaf))
}

if ($optionalRuntimeFiles.Count -gt 0) {
    Write-Host "Copied optional runtime files:"
    foreach ($file in $optionalRuntimeFiles) {
        Write-Host (" - " + (Split-Path $file -Leaf))
    }
}

$licenseFiles = @(
    (Join-Path $repoRoot "LICENSE"),
    (Join-Path $openCvSourcePath "LICENSE"),
    (Join-Path $openCvSourcePath "3rdparty\ippicv\readme.htm")
)

foreach ($file in $licenseFiles) {
    if (Test-Path -LiteralPath $file) {
        Copy-Item -LiteralPath $file -Destination $runtimeProjectLicenseDir -Force
    }
}

if ($null -ne $openCvInstallPath) {
    $openCvLicenseDir = Join-Path $openCvInstallPath "etc\licenses"
    if (Test-Path -LiteralPath $openCvLicenseDir) {
        Get-ChildItem -File -LiteralPath $openCvLicenseDir | ForEach-Object {
            Copy-Item -LiteralPath $_.FullName -Destination $runtimeProjectOpenCvLicenseDir -Force
        }
    }
}

Write-Host "Runtime staging directory: $stagingNativeDir"
Write-Host "Runtime package project directory: $runtimeProjectNativeDir"
Write-Host "Runtime license directory: $runtimeProjectLicenseDir"
