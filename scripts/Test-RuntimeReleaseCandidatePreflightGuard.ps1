param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
if ($null -eq $pwsh) {
    throw "pwsh was not found. Runtime release-candidate preflight validation requires PowerShell 7+."
}

$stageRuntimePath = Join-Path $repo "scripts/Stage-Runtime.ps1"
$preflightPath = Join-Path $repo "scripts/Test-RuntimeReleaseCandidatePreflight.ps1"
foreach ($scriptPath in @($stageRuntimePath, $preflightPath)) {
    if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf)) {
        throw "Required runtime release preflight script was not found: $scriptPath"
    }
}

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
    param([Parameter(Mandatory = $true)][string]$Path)

    $directory = Split-Path -Parent $Path
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    [System.IO.File]::WriteAllBytes($Path, [byte[]](0x4D, 0x5A, 0x00, 0x00))
}

function Invoke-ChildPwsh {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)

    $output = & $pwsh.Source @Arguments 2>&1
    return [pscustomobject]@{
        ExitCode = $LASTEXITCODE
        Output = (($output | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine)
    }
}

$violations = [System.Collections.Generic.List[object]]::new()
$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-runtime-release-preflight-" + [System.Guid]::NewGuid().ToString("N"))
$nativeWrapperRuntimeDir = Join-Path $temporaryRoot "native-wrapper-runtime"
$openCvRuntimeDir = Join-Path $temporaryRoot "opencv-runtime"
$openCvSourceDir = Join-Path $temporaryRoot "opencv-source"
$openCvInstallDir = Join-Path $temporaryRoot "opencv-install"
$outputRoot = Join-Path $temporaryRoot "staging-output"
$runtimeProjectDir = Join-Path $temporaryRoot "runtime-package-project"
$rid = "win-x64"
$runtimeProfile = "full"
$packageId = "JYPPX.OpenCV.runtime.win-x64"
$packageVersion = "5.0.0.0"
$openCvVersion = "5.0.0"
$openCvBinarySuffix = "500"
$primaryNativeLoader = "JYPPX.OpenCV.Native.dll"
$compatibilityNativeLoader = "OpenCv5Sharp.Native.dll" # compatibility loader copy for already-compiled consumers

$matrix = Get-Content -LiteralPath (Join-Path $repo "packaging/runtime/runtime-package-matrix.json") -Raw | ConvertFrom-Json
$profileSpec = @($matrix.profiles | Where-Object { $_.name -eq $runtimeProfile } | Select-Object -First 1)
$requiredOpenCvModules = @($profileSpec[0].modules)

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

    $stageResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $stageRuntimePath,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-Configuration", "Release",
        "-OpenCvNativeRuntimeDir", $nativeWrapperRuntimeDir,
        "-OpenCvRuntimeDir", $openCvRuntimeDir,
        "-OpenCvSourceDir", $openCvSourceDir,
        "-OpenCvInstallDir", $openCvInstallDir,
        "-OutputRoot", $outputRoot,
        "-RuntimeProject", $runtimeProjectDir,
        "-RuntimePackageId", $packageId,
        "-PackageVersion", $packageVersion
    )
    if ($stageResult.ExitCode -ne 0) {
        Add-Violation -Violations $violations -Path $stageRuntimePath -Issue "Release-shaped staging fixture failed" -Text $stageResult.Output
    }

    $preflightResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $preflightPath,
        "-RepositoryRoot", $repo,
        "-RuntimeProject", $runtimeProjectDir,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-RuntimePackageId", $packageId,
        "-PackageVersion", $packageVersion,
        "-OpenCvVersion", $openCvVersion
    )
    if ($preflightResult.ExitCode -ne 0) {
        Add-Violation -Violations $violations -Path $preflightPath -Issue "Release-shaped runtime preflight should pass" -Text $preflightResult.Output
    }

    $staleNativeFile = Join-Path $runtimeProjectDir "runtimes/$rid/native/stale-opencv-file.dll"
    Write-SyntheticDll -Path $staleNativeFile
    $staleResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $preflightPath,
        "-RepositoryRoot", $repo,
        "-RuntimeProject", $runtimeProjectDir,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-RuntimePackageId", $packageId,
        "-PackageVersion", $packageVersion,
        "-OpenCvVersion", $openCvVersion
    )
    if ($staleResult.ExitCode -eq 0 -or $staleResult.Output.IndexOf("contain no stale files", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        Add-Violation -Violations $violations -Path $preflightPath -Issue "Runtime preflight must reject stale native mirror files" -Text $staleResult.Output
    }
    Remove-Item -LiteralPath $staleNativeFile -Force

    $syntheticStageResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $stageRuntimePath,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-Configuration", "Release",
        "-OpenCvNativeRuntimeDir", $nativeWrapperRuntimeDir,
        "-OpenCvRuntimeDir", $openCvRuntimeDir,
        "-OpenCvSourceDir", $openCvSourceDir,
        "-OpenCvInstallDir", $openCvInstallDir,
        "-OutputRoot", $outputRoot,
        "-RuntimeProject", $runtimeProjectDir,
        "-RuntimePackageId", $packageId,
        "-PackageVersion", $packageVersion,
        "-SyntheticRuntimeInputs"
    )
    if ($syntheticStageResult.ExitCode -ne 0) {
        Add-Violation -Violations $violations -Path $stageRuntimePath -Issue "Synthetic staging fixture failed" -Text $syntheticStageResult.Output
    }

    $syntheticPreflightResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $preflightPath,
        "-RepositoryRoot", $repo,
        "-RuntimeProject", $runtimeProjectDir,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-RuntimePackageId", $packageId,
        "-PackageVersion", $packageVersion,
        "-OpenCvVersion", $openCvVersion
    )
    if ($syntheticPreflightResult.ExitCode -eq 0 -or $syntheticPreflightResult.Output.IndexOf("rejects synthetic runtime inputs", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        Add-Violation -Violations $violations -Path $preflightPath -Issue "Runtime preflight must reject synthetic runtime inputs by default" -Text $syntheticPreflightResult.Output
    }

    $allowSyntheticPreflightResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $preflightPath,
        "-RepositoryRoot", $repo,
        "-RuntimeProject", $runtimeProjectDir,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-RuntimePackageId", $packageId,
        "-PackageVersion", $packageVersion,
        "-OpenCvVersion", $openCvVersion,
        "-AllowSyntheticRuntimeInputs"
    )
    if ($allowSyntheticPreflightResult.ExitCode -ne 0) {
        Add-Violation -Violations $violations -Path $preflightPath -Issue "Runtime preflight should allow synthetic inputs only when explicitly requested" -Text $allowSyntheticPreflightResult.Output
    }
}
finally {
    Remove-DirectoryIfSafe -Path $temporaryRoot -AllowedRoot ([System.IO.Path]::GetTempPath())
}

if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
    Add-Violation -Violations $violations -Path $temporaryRoot -Issue "Temporary runtime release-candidate preflight fixture was not cleaned"
}

if ($violations.Count -gt 0) {
    Write-Host "Runtime release-candidate preflight guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-List Path, Issue, Text
    exit 1
}

Write-Host "Runtime release-candidate preflight guard passed."
Write-Host "Release-shaped manifests pass; synthetic manifests and stale mirrors are rejected by default."
