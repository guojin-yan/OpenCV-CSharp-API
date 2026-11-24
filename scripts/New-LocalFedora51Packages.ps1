[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ArtifactDirectory,
    [string]$OpenCvSourceDirectory = (Join-Path (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path 'opencv-source/opencv-5.1.0'),
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path $RepositoryRoot).Path; $artifact = (Resolve-Path $ArtifactDirectory).Path
foreach ($profile in @('full','mini')) {
    # Pack-Runtime stages into its project. Give each profile a fresh project so
    # no previous RID, profile, license or native input can enter this candidate.
    $root = Join-Path $artifact "package-project/$profile"
    if (Test-Path $root) { throw 'A fresh Fedora package project is required.' }
    $project = Join-Path $root 'packaging/runtime/JYPPX.OpenCV.runtime'
    [IO.Directory]::CreateDirectory($project) | Out-Null
    [IO.Directory]::CreateDirectory((Join-Path $root 'nuget')) | Out-Null
    foreach ($file in @('Directory.Build.props','README.md','nuget/logo.jpg',
            'packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj',
            'packaging/runtime/runtime-distro-rid-graph.json')) {
        Copy-Item (Join-Path $repo $file) (Join-Path $root $file)
    }
    $runtime = Join-Path $artifact "producer/$profile/runtime"
    $parameters = @{
        Rid = 'fedora.44-x64'; RuntimeProfile = $profile; OpenCvVersion = '5.1.0'; PackageRevision = 0
        PackageVersion = '5.1.0.0-fedora44.b7f8180'; RuntimePackageMatrix = 'packaging/runtime/runtime-lifecycle-refresh-matrix.json'
        RuntimeProject = Join-Path $project 'JYPPX.OpenCV.runtime.csproj'; StageRuntime = $true
        OpenCvNativeRuntimeDir = $runtime; OpenCvRuntimeDir = $runtime; OpenCvSourceDir = $OpenCvSourceDirectory
        OutputDir = Join-Path $artifact 'packages'; StageOutputRoot = Join-Path $artifact "staged/$profile"
        OptionalOpenCvModules = @()
    }
    $install = Join-Path $artifact "producer/$profile/install"
    # Export Fedora's share/licenses/opencv5 files into the legacy etc/licenses
    # input layout expected by Stage-Runtime; retain the original bytes.
    if (-not (Test-Path (Join-Path $install 'etc/licenses'))) { throw 'The current producer installed-license export is required.' }
    $parameters.OpenCvInstallDir = $install
    & (Join-Path $repo 'scripts/Pack-Runtime.ps1') @parameters
}
Write-Host 'FEDORA_51_PACKAGES_OK profiles=2 candidate_only=true'
