param(
    [string]$Rid = "win-x64",
    [string]$Configuration = "Release",
    [string]$OpenCvVersion = "",
    [int]$PackageRevision = -1,
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
    [string]$OpenCvRuntimeDir = "",
    [string]$OpenCvInstallDir = "",
    [string]$OpenCvSourceDir = "",
    [string[]]$OpenCvModules = @(),
    [string[]]$OptionalOpenCvModules = @("xfeatures2d", "xobjdetect", "quality", "xphoto", "img_hash", "ximgproc", "optflow", "bgsegm", "tracking", "face", "saliency", "plot", "shape", "line_descriptor", "phase_unwrapping", "structured_light", "intensity_transform", "fuzzy", "hfs", "reg", "surface_matching", "rapid", "alphamat", "bioinspired", "xstereo"),
    [switch]$StageRuntime,
    [switch]$SyntheticRuntimeInputs,
    [switch]$RequireReleasePreflight,
    [switch]$NoBuild,
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
. (Join-Path $PSScriptRoot "PackageVersion.ps1")

function Resolve-PropertyReferences {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value,
        [Parameter(Mandatory = $true)]
        [hashtable]$Properties
    )

    $resolved = $Value
    for ($i = 0; $i -lt 8; $i++) {
        $previous = $resolved
        $resolved = [regex]::Replace(
            $resolved,
            '\$\((?<name>[A-Za-z_][A-Za-z0-9_.-]*)\)',
            {
                param($match)
                $name = $match.Groups["name"].Value
                if ($Properties.ContainsKey($name)) {
                    return [string]$Properties[$name]
                }

                return $match.Value
            })

        if ($resolved.Equals($previous, [System.StringComparison]::Ordinal)) {
            break
        }
    }

    return $resolved
}

function Get-DirectoryBuildProperties {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $propsPath = Join-Path $RepositoryRoot "Directory.Build.props"
    if (-not (Test-Path -LiteralPath $propsPath -PathType Leaf)) {
        throw "Directory.Build.props was not found: $propsPath"
    }

    [xml]$project = [System.IO.File]::ReadAllText($propsPath)
    $properties = [ordered]@{}
    foreach ($propertyGroup in $project.Project.PropertyGroup) {
        if ($null -eq $propertyGroup) {
            continue
        }

        foreach ($child in $propertyGroup.ChildNodes) {
            if ($child.NodeType -ne [System.Xml.XmlNodeType]::Element) {
                continue
            }

            if ([string]::IsNullOrWhiteSpace($child.InnerText)) {
                continue
            }

            $properties[$child.Name] = $child.InnerText
        }
    }

    foreach ($key in @($properties.Keys)) {
        $properties[$key] = Resolve-PropertyReferences -Value ([string]$properties[$key]) -Properties $properties
    }

    return $properties
}

function Get-RequiredDirectoryBuildProperty {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Properties,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if (-not $Properties.ContainsKey($Name) -or [string]::IsNullOrWhiteSpace([string]$Properties[$Name])) {
        throw "Required Directory.Build.props metadata property was not found: $Name"
    }

    return [string]$Properties[$Name]
}

$centralProperties = Get-DirectoryBuildProperties -RepositoryRoot $repoRoot
$runtimePackagePrefix = Get-RequiredDirectoryBuildProperty -Properties $centralProperties -Name "OpenCvCSharpRuntimePackageIdPrefix"
$centralOpenCvVersion = Get-RequiredDirectoryBuildProperty -Properties $centralProperties -Name "OpenCvCSharpOpenCvVersion"
$centralPackageRevision = [int](Get-RequiredDirectoryBuildProperty -Properties $centralProperties -Name "OpenCvCSharpPackageRevision")
$centralPackageVersion = Get-RequiredDirectoryBuildProperty -Properties $centralProperties -Name "OpenCvCSharpPackageVersion"

if ([string]::IsNullOrWhiteSpace($OpenCvVersion)) {
    $OpenCvVersion = $centralOpenCvVersion
}

if ($PackageRevision -lt 0) {
    $PackageRevision = $centralPackageRevision
}

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
    if ($OpenCvVersion -eq $centralOpenCvVersion -and $PackageRevision -eq $centralPackageRevision) {
        $PackageVersion = $centralPackageVersion
    }
    else {
        $PackageVersion = "$OpenCvVersion.$PackageRevision"
    }
}

$packageVersionRecord = Assert-OpenCvCSharpPackageVersion `
    -Version $PackageVersion `
    -OpenCvVersion $OpenCvVersion `
    -PackageRevision $PackageRevision

$runtimePackageId = "$runtimePackagePrefix.$Rid$runtimePackageSuffix"

if ($RequireReleasePreflight -and $SyntheticRuntimeInputs) {
    throw "Release-candidate runtime packages require real native runtime inputs; synthetic runtime inputs validate package shape only."
}

if ($StageRuntime) {
    if ([string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir)) {
        throw "OpenCvNativeRuntimeDir is required when StageRuntime is set."
    }

    $stageParameters = @{
        Rid = $Rid
        Configuration = $Configuration
        OpenCvNativeRuntimeDir = $OpenCvNativeRuntimeDir
        OpenCvVersion = $OpenCvVersion
        RuntimeProject = $runtimeProjectDirectory
        RuntimePackageMatrix = $RuntimePackageMatrix
        RuntimeProfile = $RuntimeProfile
        RuntimePackageId = $runtimePackageId
        PackageVersion = $PackageVersion
        OpenCvModules = $OpenCvModules
        OptionalOpenCvModules = $OptionalOpenCvModules
    }

    if ($SyntheticRuntimeInputs) {
        $stageParameters.SyntheticRuntimeInputs = $true
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

if ($RequireReleasePreflight) {
    $preflightParameters = @{
        RepositoryRoot = $repoRoot
        RuntimeProject = $runtimeProjectFullPath
        RuntimePackageMatrix = $RuntimePackageMatrix
        Rid = $Rid
        RuntimeProfile = $RuntimeProfile
        RuntimePackageId = $runtimePackageId
        PackageVersion = $PackageVersion
        OpenCvVersion = $OpenCvVersion
    }

    try {
        & (Join-Path $PSScriptRoot "Test-RuntimeReleaseCandidatePreflight.ps1") @preflightParameters
    }
    catch {
        throw "Runtime release-candidate preflight failed: $($_.Exception.Message)"
    }
}

New-Item -ItemType Directory -Force $outputFullPath | Out-Null

$packagePath = Join-Path $outputFullPath "$runtimePackageId.$($packageVersionRecord.NuGetVersion).nupkg"
$sdkPackagePath = Join-Path $outputFullPath "$runtimePackageId.$PackageVersion.nupkg"
foreach ($candidatePath in @($packagePath, $sdkPackagePath) | Sort-Object -Unique) {
    if (Test-Path -LiteralPath $candidatePath) {
        Remove-Item -LiteralPath $candidatePath -Force
    }
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

$producedPackagePaths = @(
    @($packagePath, $sdkPackagePath) |
        Sort-Object -Unique |
        Where-Object { Test-Path -LiteralPath $_ -PathType Leaf }
)
if ($producedPackagePaths.Count -ne 1) {
    throw "Runtime package output could not be identified exactly once. Expected SDK or canonical artifact: $sdkPackagePath / $packagePath"
}

if (-not $producedPackagePaths[0].Equals($packagePath, [System.StringComparison]::OrdinalIgnoreCase)) {
    Move-Item -LiteralPath $producedPackagePaths[0] -Destination $packagePath
}

& (Join-Path $PSScriptRoot "Normalize-NuGetPackageDeterminism.ps1") -PackagePath $packagePath

if (-not (Test-Path -LiteralPath $packagePath)) {
    throw "Runtime package artifact was not found: $packagePath"
}

Write-Host "Runtime package output directory: $outputFullPath"
Write-Host "Runtime package artifact: $packagePath"
