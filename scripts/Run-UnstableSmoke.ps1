param(
    [string]$Configuration = "Release",
    [string]$Framework = "net10.0",
    [string]$Filter = "FullyQualifiedName~BioInspired",
    [string]$ProjectPath = "tests\OpenCvSharp.Tests\OpenCvSharp.Tests.csproj",
    # OpenCvNativeRuntimeDir is the preferred version-neutral runtime path/build property passed through to MSBuild.
    [string]$OpenCvNativeRuntimeDir = "",
    [string]$DiagLog = "artifacts\unstable-smoke-testhost.log",
    [switch]$IncludeOrdinaryNativeSmoke,
    [switch]$NoBuild,
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]]$Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath failed with exit code $LASTEXITCODE."
    }
}

function Restore-EnvironmentVariable {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [AllowNull()]
        [string]$Value
    )

    if ($null -eq $Value) {
        Remove-Item -Path "Env:$Name" -ErrorAction SilentlyContinue
        return
    }

    Set-Item -Path "Env:$Name" -Value $Value
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$projectPathCandidate = if ([System.IO.Path]::IsPathRooted($ProjectPath)) {
    $ProjectPath
}
else {
    Join-Path $repoRoot $ProjectPath
}

if (-not (Test-Path -LiteralPath $projectPathCandidate -PathType Leaf)) {
    throw "Smoke test project file was not found: $projectPathCandidate"
}

$projectFullPath = (Resolve-Path -LiteralPath $projectPathCandidate).Path
$diagPathCandidate = if ([System.IO.Path]::IsPathRooted($DiagLog)) {
    $DiagLog
}
else {
    Join-Path $repoRoot $DiagLog
}

$diagFullPath = [System.IO.Path]::GetFullPath($diagPathCandidate)
$diagDirectory = Split-Path -Parent $diagFullPath
if (-not [string]::IsNullOrWhiteSpace($diagDirectory)) {
    New-Item -ItemType Directory -Force $diagDirectory | Out-Null
}

$unstableNativeSmokeVariable = "OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE"
$compatibilityUnstableNativeSmokeAlias = "OPENCV5SHARP_UNSTABLE_NATIVE_SMOKE"
$nativeSmokeVariable = "OPENCV_CSHARP_NATIVE_SMOKE"
$compatibilityNativeSmokeAlias = "OPENCV5SHARP_NATIVE_SMOKE"
# OPENCV_CSHARP_* environment variables are primary; OPENCV5SHARP_* names remain only as existing-workflow compatibility aliases for smoke automation.

Write-Warning "Unstable native smoke may terminate the testhost process. Run only while diagnosing fragile linked runtime paths."
Write-Host "Project: $projectFullPath"
Write-Host "Framework: $Framework"
Write-Host "Filter: $Filter"

if (-not $NoBuild) {
    $buildArguments = @(
        "build",
        $projectFullPath,
        "-c",
        $Configuration,
        "-f",
        $Framework
    )

    if ($NoRestore) {
        $buildArguments += "--no-restore"
    }

    if (-not [string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir)) {
        $buildArguments += "/p:OpenCvNativeRuntimeDir=$OpenCvNativeRuntimeDir"
    }

    Invoke-CheckedCommand dotnet @buildArguments
}
elseif (-not [string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir)) {
    Write-Warning "OpenCvNativeRuntimeDir is only applied during the build step; -NoBuild assumes the test output is already staged."
}

$previousUnstableNativeSmoke = [Environment]::GetEnvironmentVariable($unstableNativeSmokeVariable)
$previousCompatibilityUnstableNativeSmoke = [Environment]::GetEnvironmentVariable($compatibilityUnstableNativeSmokeAlias)
$previousNativeSmoke = [Environment]::GetEnvironmentVariable($nativeSmokeVariable)
$previousCompatibilityNativeSmoke = [Environment]::GetEnvironmentVariable($compatibilityNativeSmokeAlias)

try {
    Set-Item -Path "Env:$unstableNativeSmokeVariable" -Value "1"
    Remove-Item -Path "Env:$compatibilityUnstableNativeSmokeAlias" -ErrorAction SilentlyContinue
    if ($IncludeOrdinaryNativeSmoke) {
        Set-Item -Path "Env:$nativeSmokeVariable" -Value "1"
    }
    else {
        Remove-Item -Path "Env:$nativeSmokeVariable" -ErrorAction SilentlyContinue
        Remove-Item -Path "Env:$compatibilityNativeSmokeAlias" -ErrorAction SilentlyContinue
    }

    $testArguments = @(
        "test",
        $projectFullPath,
        "-c",
        $Configuration,
        "-f",
        $Framework,
        "--no-build",
        "--filter",
        $Filter,
        "--diag",
        $diagFullPath
    )

    if ($NoRestore) {
        $testArguments += "--no-restore"
    }

    Invoke-CheckedCommand dotnet @testArguments
}
finally {
    Restore-EnvironmentVariable -Name $unstableNativeSmokeVariable -Value $previousUnstableNativeSmoke
    Restore-EnvironmentVariable -Name $compatibilityUnstableNativeSmokeAlias -Value $previousCompatibilityUnstableNativeSmoke
    Restore-EnvironmentVariable -Name $nativeSmokeVariable -Value $previousNativeSmoke
    Restore-EnvironmentVariable -Name $compatibilityNativeSmokeAlias -Value $previousCompatibilityNativeSmoke
}

Write-Host "Unstable smoke diagnostic log: $diagFullPath"
