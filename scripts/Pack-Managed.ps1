param(
    [string]$Configuration = "Release",
    [string]$OpenCvVersion = "",
    [int]$PackageRevision = -1,
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
$managedPackageId = Get-RequiredDirectoryBuildProperty -Properties $centralProperties -Name "OpenCvCSharpManagedPackageId"
$centralOpenCvVersion = Get-RequiredDirectoryBuildProperty -Properties $centralProperties -Name "OpenCvCSharpOpenCvVersion"
$centralPackageRevision = [int](Get-RequiredDirectoryBuildProperty -Properties $centralProperties -Name "OpenCvCSharpPackageRevision")
$centralPackageVersion = Get-RequiredDirectoryBuildProperty -Properties $centralProperties -Name "OpenCvCSharpPackageVersion"

if ([string]::IsNullOrWhiteSpace($OpenCvVersion)) {
    $OpenCvVersion = $centralOpenCvVersion
}

if ($PackageRevision -lt 0) {
    $PackageRevision = $centralPackageRevision
}

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

New-Item -ItemType Directory -Force $outputFullPath | Out-Null

$packagePath = Join-Path $outputFullPath "$managedPackageId.$($packageVersionRecord.NuGetVersion).nupkg"
$sdkPackagePath = Join-Path $outputFullPath "$managedPackageId.$PackageVersion.nupkg"
foreach ($candidatePath in @($packagePath, $sdkPackagePath) | Sort-Object -Unique) {
    if (Test-Path -LiteralPath $candidatePath) {
        Remove-Item -LiteralPath $candidatePath -Force
    }
}

$arguments = @(
    "pack",
    $projectFullPath,
    "-c",
    $Configuration,
    "-o",
    $outputFullPath,
    "-p:Version=$PackageVersion",
    "-p:PackageVersion=$PackageVersion",
    "-p:OpenCvCSharpNuGetPackageVersion=$($packageVersionRecord.NuGetVersion)"
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

$producedPackagePaths = @(
    @($packagePath, $sdkPackagePath) |
        Sort-Object -Unique |
        Where-Object { Test-Path -LiteralPath $_ -PathType Leaf }
)
if ($producedPackagePaths.Count -ne 1) {
    throw "Managed package output could not be identified exactly once. Expected SDK or canonical artifact: $sdkPackagePath / $packagePath"
}

if (-not $producedPackagePaths[0].Equals($packagePath, [System.StringComparison]::OrdinalIgnoreCase)) {
    Move-Item -LiteralPath $producedPackagePaths[0] -Destination $packagePath
}

& (Join-Path $PSScriptRoot "Normalize-NuGetPackageDeterminism.ps1") -PackagePath $packagePath

if (-not (Test-Path -LiteralPath $packagePath)) {
    throw "Managed package artifact was not found: $packagePath"
}

Write-Host "Managed package output directory: $outputFullPath"
Write-Host "Managed package artifact: $packagePath"
