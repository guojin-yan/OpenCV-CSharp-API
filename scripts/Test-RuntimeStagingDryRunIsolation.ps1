param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
if ($null -eq $pwsh) {
    throw "pwsh was not found. Runtime staging dry-run isolation validation requires PowerShell 7+."
}

$stageRuntimePath = Join-Path $repo "scripts/Stage-Runtime.ps1"
if (-not (Test-Path -LiteralPath $stageRuntimePath -PathType Leaf)) {
    throw "Stage-Runtime.ps1 was not found: $stageRuntimePath"
}

$primaryNativeLoader = "JYPPX.OpenCV.Native.dll"
$compatibilityNativeLoader = "OpenCv5Sharp.Native.dll"
$requiredOpenCvModules = @(
    "core",
    "imgcodecs",
    "imgproc",
    "videoio",
    "flann",
    "geometry",
    "calib",
    "stereo",
    "dnn",
    "objdetect",
    "photo",
    "features",
    "video",
    "highgui",
    "stitching",
    "ptcloud"
)

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Issue = $Issue
        Text = $Text.Trim()
    })
}

function Test-IsPathUnder {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Root
    )

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $fullRoot = [System.IO.Path]::GetFullPath($Root).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $rootPrefix = $fullRoot + [System.IO.Path]::DirectorySeparatorChar

    return (
        $fullPath.Equals($fullRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
        $fullPath.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase))
}

function Remove-DirectoryIfSafe {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$AllowedRoot
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        return
    }

    if (-not (Test-IsPathUnder -Path $Path -Root $AllowedRoot)) {
        throw "Refusing to remove path outside allowed root. Path: $Path; allowed root: $AllowedRoot"
    }

    Remove-Item -LiteralPath $Path -Recurse -Force
}

function Write-SyntheticDll {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $directory = Split-Path -Parent $Path
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    [System.IO.File]::WriteAllBytes($Path, [byte[]](0x4D, 0x5A, 0x00, 0x00))
}

function Assert-FileExists {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue
    }
}

$violations = [System.Collections.Generic.List[object]]::new()
$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-runtime-staging-dry-run-" + [System.Guid]::NewGuid().ToString("N"))
$nativeWrapperRuntimeDir = Join-Path $temporaryRoot "native-wrapper-runtime"
$openCvRuntimeDir = Join-Path $temporaryRoot "opencv-runtime"
$openCvSourceDir = Join-Path $temporaryRoot "opencv-source"
$openCvInstallDir = Join-Path $temporaryRoot "opencv-install"
$outputRoot = Join-Path $temporaryRoot "staging-output"
$runtimeProjectDir = Join-Path $temporaryRoot "runtime-package-project"
$rid = "win-x64"
$openCvBinarySuffix = "500"

$repoRuntimeOutputRoot = Join-Path $repo "artifacts/runtime"
$repoRuntimeProjectRoot = Join-Path $repo "packaging/runtime/JYPPX.OpenCV.runtime"
$repoSensitiveDirectories = @(
    $repoRuntimeOutputRoot,
    (Join-Path $repoRuntimeProjectRoot "runtimes"),
    (Join-Path $repoRuntimeProjectRoot "licenses")
)

$preexistingSensitiveDirectories = @{}
foreach ($directory in $repoSensitiveDirectories) {
    $preexistingSensitiveDirectories[$directory] = Test-Path -LiteralPath $directory -PathType Container
}

try {
    foreach ($directory in @(
            $nativeWrapperRuntimeDir,
            $openCvRuntimeDir,
            (Join-Path $openCvSourceDir "3rdparty/ippicv"),
            (Join-Path $openCvInstallDir "etc/licenses"),
            $outputRoot,
            $runtimeProjectDir)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    foreach ($dllName in @($primaryNativeLoader, $compatibilityNativeLoader)) {
        Write-SyntheticDll -Path (Join-Path $nativeWrapperRuntimeDir $dllName)
    }

    foreach ($module in $requiredOpenCvModules) {
        Write-SyntheticDll -Path (Join-Path $openCvRuntimeDir "opencv_$module$openCvBinarySuffix.dll")
    }

    [System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "LICENSE"), "Synthetic OpenCV source license")
    [System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "3rdparty/ippicv/readme.htm"), "Synthetic IPPICV license")
    [System.IO.File]::WriteAllText((Join-Path $openCvInstallDir "etc/licenses/synthetic-3rdparty.txt"), "Synthetic third-party license")

    $stageArguments = @(
        "-NoProfile",
        "-File", $stageRuntimePath,
        "-Rid", $rid,
        "-Configuration", "Release",
        "-OpenCvNativeRuntimeDir", $nativeWrapperRuntimeDir,
        "-OpenCvRuntimeDir", $openCvRuntimeDir,
        "-OpenCvSourceDir", $openCvSourceDir,
        "-OpenCvInstallDir", $openCvInstallDir,
        "-OutputRoot", $outputRoot,
        "-RuntimeProject", $runtimeProjectDir,
        "-SyntheticRuntimeInputs"
    )

    $stageOutput = & $pwsh.Source @stageArguments 2>&1
    $stageOutputText = ($stageOutput | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine
    if ($LASTEXITCODE -ne 0) {
        Add-Violation -Violations $violations -Path "scripts/Stage-Runtime.ps1" -Issue "Stage-Runtime synthetic isolation dry-run failed" -Text $stageOutputText
    }

    $expectedRuntimeFiles = @($primaryNativeLoader, $compatibilityNativeLoader)
    foreach ($module in $requiredOpenCvModules) {
        $expectedRuntimeFiles += "opencv_$module$openCvBinarySuffix.dll"
    }

    $stagingNativeDir = Join-Path $outputRoot (Join-Path $rid "native")
    $runtimeProjectNativeDir = Join-Path $runtimeProjectDir (Join-Path "runtimes/$rid" "native")
    foreach ($runtimeFile in $expectedRuntimeFiles) {
        Assert-FileExists -Violations $violations -Path (Join-Path $stagingNativeDir $runtimeFile) -Issue "Synthetic runtime file was not staged under temporary OutputRoot"
        Assert-FileExists -Violations $violations -Path (Join-Path $runtimeProjectNativeDir $runtimeFile) -Issue "Synthetic runtime file was not staged under temporary RuntimeProject mirror"
    }

    $runtimeProjectLicenseDir = Join-Path $runtimeProjectDir "licenses"
    Assert-FileExists -Violations $violations -Path (Join-Path $runtimeProjectLicenseDir "LICENSE") -Issue "Runtime project license layout did not include LICENSE"
    Assert-FileExists -Violations $violations -Path (Join-Path $runtimeProjectLicenseDir "readme.htm") -Issue "Runtime project license layout did not include OpenCV 3rdparty readme.htm"
    Assert-FileExists -Violations $violations -Path (Join-Path $runtimeProjectLicenseDir "opencv-3rdparty/synthetic-3rdparty.txt") -Issue "Runtime project license layout did not include OpenCV install third-party license"

    $manifestPath = Join-Path $runtimeProjectDir "build/JYPPX.OpenCV.runtime.provenance.json"
    Assert-FileExists -Violations $violations -Path $manifestPath -Issue "Runtime project build layout did not include provenance manifest"
    if (Test-Path -LiteralPath $manifestPath -PathType Leaf) {
        $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
        if ($manifest.PackageId -ne "JYPPX.OpenCV.runtime.$rid") {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Provenance manifest PackageId must match neutral runtime package ID" -Text $manifest.PackageId
        }

        if ($manifest.PackageVersion -ne "5.0.0.0") {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Provenance manifest PackageVersion must carry OpenCV runtime identity as version metadata" -Text $manifest.PackageVersion
        }

        if ($manifest.Rid -ne $rid -or $manifest.RuntimeProfile -ne "full") {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Provenance manifest must record selected RID/profile" -Text "$($manifest.Rid)/$($manifest.RuntimeProfile)"
        }

        if (-not [bool]$manifest.SyntheticRuntimeInputs) {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Synthetic dry-run provenance manifest must be marked synthetic"
        }

        if (@($manifest.RequiredModules).Count -ne $requiredOpenCvModules.Count) {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Provenance manifest required module count must match staged profile" -Text "Found $(@($manifest.RequiredModules).Count), expected $($requiredOpenCvModules.Count)"
        }

        if ($manifest.PrimaryNativeLoaderName -ne $primaryNativeLoader -or $manifest.CompatibilityNativeLoaderName -ne $compatibilityNativeLoader) {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Provenance manifest must record primary and compatibility loader names" -Text "$($manifest.PrimaryNativeLoaderName) / $($manifest.CompatibilityNativeLoaderName)"
        }
    }

    if ($stageOutputText.IndexOf("Optional OpenCV runtime module was not found and will be skipped", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        Add-Violation -Violations $violations -Path "scripts/Stage-Runtime.ps1" -Issue "Missing optional OpenCV modules must warn and continue during staging dry-run" -Text $stageOutputText
    }
}
finally {
    foreach ($directory in $repoSensitiveDirectories) {
        $existsAfter = Test-Path -LiteralPath $directory -PathType Container
        if (-not $preexistingSensitiveDirectories[$directory] -and $existsAfter) {
            Add-Violation -Violations $violations -Path $directory -Issue "Staging dry-run unexpectedly created a repository runtime output/mirror directory"
            Remove-DirectoryIfSafe -Path $directory -AllowedRoot $repo
        }
    }

    Remove-DirectoryIfSafe -Path $temporaryRoot -AllowedRoot ([System.IO.Path]::GetTempPath())
}

if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
    Add-Violation -Violations $violations -Path $temporaryRoot -Issue "Temporary runtime staging dry-run output was not cleaned"
}

foreach ($directory in $repoSensitiveDirectories) {
    if (-not $preexistingSensitiveDirectories[$directory] -and (Test-Path -LiteralPath $directory -PathType Container)) {
        Add-Violation -Violations $violations -Path $directory -Issue "Repository runtime output/mirror directory remains after dry-run cleanup"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Runtime staging dry-run isolation guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Runtime staging dry-run isolation guard passed."
Write-Host "Required OpenCV runtime modules staged: $($requiredOpenCvModules.Count)."
Write-Host "Temporary OutputRoot and RuntimeProject were outside the repository and cleaned."
