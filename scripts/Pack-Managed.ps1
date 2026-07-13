param(
    [string]$Configuration = "Release",
    [string]$OpenCvVersion = "5.0.0",
    [int]$PackageRevision = 0,
    [string]$PackageVersion = "",
    [string]$OutputDir = "artifacts\packages",
    [string]$ProjectPath = "src\OpenCvSharp\OpenCvSharp.csproj",
    [string]$TargetFrameworks = "",
    [string]$BuildOutputRoot = "",
    [string]$RestorePackagesPath = "",
    [switch]$NoBuild,
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$projectPathCandidate = if ([System.IO.Path]::IsPathRooted($ProjectPath)) {
    $ProjectPath
}
else {
    Join-Path $repoRoot $ProjectPath
}

if (-not (Test-Path -LiteralPath $projectPathCandidate -PathType Leaf)) {
    throw "Managed project file was not found: $projectPathCandidate"
}

$projectFullPath = (Resolve-Path -LiteralPath $projectPathCandidate).Path
$outputPathCandidate = if ([System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir
}
else {
    Join-Path $repoRoot $OutputDir
}

$outputFullPath = [System.IO.Path]::GetFullPath($outputPathCandidate)

$buildOutputRootFullPath = ""
if (-not [string]::IsNullOrWhiteSpace($BuildOutputRoot)) {
    $buildOutputRootCandidate = if ([System.IO.Path]::IsPathRooted($BuildOutputRoot)) {
        $BuildOutputRoot
    }
    else {
        Join-Path $repoRoot $BuildOutputRoot
    }

    $buildOutputRootFullPath = [System.IO.Path]::GetFullPath($buildOutputRootCandidate)
}

$restorePackagesFullPath = ""
if (-not [string]::IsNullOrWhiteSpace($RestorePackagesPath)) {
    $restorePackagesCandidate = if ([System.IO.Path]::IsPathRooted($RestorePackagesPath)) {
        $RestorePackagesPath
    }
    else {
        Join-Path $repoRoot $RestorePackagesPath
    }

    $restorePackagesFullPath = [System.IO.Path]::GetFullPath($restorePackagesCandidate)
}

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

New-Item -ItemType Directory -Force $outputFullPath | Out-Null

$managedPackageId = "JYPPX.OpenCV.CSharp.API"
$packageFileVersion = Get-NuGetPackageFileVersion -Version $PackageVersion
$packagePath = Join-Path $outputFullPath "$managedPackageId.$packageFileVersion.nupkg"
if (Test-Path -LiteralPath $packagePath) {
    Remove-Item -LiteralPath $packagePath -Force
}

$arguments = @(
    "pack",
    $projectFullPath,
    "-c",
    $Configuration,
    "-o",
    $outputFullPath,
    "-p:Version=$PackageVersion",
    "-p:PackageVersion=$PackageVersion"
)

if (-not [string]::IsNullOrWhiteSpace($TargetFrameworks)) {
    $arguments += "-p:TargetFrameworks=$TargetFrameworks"
}

if (-not [string]::IsNullOrWhiteSpace($buildOutputRootFullPath)) {
    $baseOutputPath = (Join-Path $buildOutputRootFullPath "bin").TrimEnd("\", "/") + [System.IO.Path]::DirectorySeparatorChar
    $baseIntermediateOutputPath = (Join-Path $buildOutputRootFullPath "obj").TrimEnd("\", "/") + [System.IO.Path]::DirectorySeparatorChar
    $arguments += "-p:BaseOutputPath=$baseOutputPath"
    $arguments += "-p:BaseIntermediateOutputPath=$baseIntermediateOutputPath"
    $arguments += "-p:MSBuildProjectExtensionsPath=$baseIntermediateOutputPath"
}

if (-not [string]::IsNullOrWhiteSpace($restorePackagesFullPath)) {
    $arguments += "-p:RestorePackagesPath=$restorePackagesFullPath"
}

if ($NoBuild) {
    $arguments += "--no-build"
}

if ($NoRestore) {
    $arguments += "--no-restore"
}

Write-Host "Packing managed API package $managedPackageId $PackageVersion"
& dotnet @arguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet pack failed with exit code $LASTEXITCODE"
}

if (-not (Test-Path -LiteralPath $packagePath)) {
    throw "Managed package artifact was not found: $packagePath"
}

Write-Host "Managed package output directory: $outputFullPath"
Write-Host "Managed package artifact: $packagePath"
