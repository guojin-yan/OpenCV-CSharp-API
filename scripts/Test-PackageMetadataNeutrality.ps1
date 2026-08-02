param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$fixedMajorManagedIdentity = "Open" + "Cv5Sharp"
$packageVersionContractPath = Join-Path $repo "scripts/PackageVersion.ps1"
if (-not (Test-Path -LiteralPath $packageVersionContractPath -PathType Leaf)) {
    throw "Package version contract was not found: $packageVersionContractPath"
}
. $packageVersionContractPath

function Get-RepositoryRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return ([System.IO.Path]::GetRelativePath($repo, $Path)) -replace "\\", "/"
}

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Issue = $Issue
    })
}

function Read-XmlProject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required project file was not found: $path"
    }

    [xml]$xml = [System.IO.File]::ReadAllText($path)
    return $xml
}

function Read-RequiredText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required file was not found: $path"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Get-XmlTextValue {
    param(
        [AllowNull()]
        [object]$Value
    )

    if ($null -eq $Value) {
        return ""
    }

    if ($Value -is [System.Xml.XmlNode]) {
        return $Value.InnerText
    }

    return [string]$Value
}

function Get-ProjectPropertyValues {
    param(
        [Parameter(Mandatory = $true)]
        [xml]$Project,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $values = [System.Collections.Generic.List[string]]::new()
    foreach ($propertyGroup in $Project.Project.PropertyGroup) {
        if ($null -eq $propertyGroup) {
            continue
        }

        $property = $propertyGroup.$Name
        if ($null -eq $property) {
            continue
        }

        if ($property -is [System.Array]) {
            foreach ($item in $property) {
                $value = Get-XmlTextValue -Value $item
                if (-not [string]::IsNullOrWhiteSpace($value)) {
                    $values.Add($value)
                }
            }
        }
        else {
            $value = Get-XmlTextValue -Value $property
            if (-not [string]::IsNullOrWhiteSpace($value)) {
                $values.Add($value)
            }
        }
    }

    return @($values)
}

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
        [xml]$Project
    )

    $properties = [ordered]@{}
    foreach ($propertyGroup in $Project.Project.PropertyGroup) {
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

function Resolve-ProjectPropertyValues {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Values,
        [Parameter(Mandatory = $true)]
        [hashtable]$Properties
    )

    return @($Values | ForEach-Object { Resolve-PropertyReferences -Value $_ -Properties $Properties })
}

function Test-FourPartVersion {
    param(
        [AllowNull()]
        [string]$Value
    )

    return $null -ne $Value -and $Value -match '^\d+\.\d+\.\d+\.\d+$'
}

function Test-ContainsText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Needle
    )

    return $Text.IndexOf($Needle, [System.StringComparison]::Ordinal) -ge 0
}

$directoryBuildProps = Read-XmlProject -RelativePath "Directory.Build.props"
$centralProperties = Get-DirectoryBuildProperties -Project $directoryBuildProps
$primaryManagedPackageId = Get-RequiredDirectoryBuildProperty $centralProperties "OpenCvCSharpManagedPackageId"
$primaryManagedAssemblyName = $primaryManagedPackageId
$primaryManagedRootNamespace = Get-RequiredDirectoryBuildProperty $centralProperties "OpenCvCSharpRootNamespace"
$runtimePackagePrefix = Get-RequiredDirectoryBuildProperty $centralProperties "OpenCvCSharpRuntimePackageIdPrefix"
$primaryNativeLoader = Get-RequiredDirectoryBuildProperty $centralProperties "OpenCvCSharpCurrentNativeLibraryName"

$violations = [System.Collections.Generic.List[object]]::new()

$acceptedPackageVersions = @(
    [pscustomobject]@{ Version = "5.0.0.0"; OpenCvVersion = "5.0.0"; Revision = 0; NuGetVersion = "5.0.0"; Prerelease = $false },
    [pscustomobject]@{ Version = "5.0.0.0-preview.1"; OpenCvVersion = "5.0.0"; Revision = 0; NuGetVersion = "5.0.0-preview.1"; Prerelease = $true },
    [pscustomobject]@{ Version = "5.0.0.1-rc.2"; OpenCvVersion = "5.0.0"; Revision = 1; NuGetVersion = "5.0.0.1-rc.2"; Prerelease = $true }
)
foreach ($fixture in $acceptedPackageVersions) {
    try {
        $record = Assert-OpenCvCSharpPackageVersion `
            -Version $fixture.Version `
            -OpenCvVersion $fixture.OpenCvVersion `
            -PackageRevision $fixture.Revision
        if ($record.NuGetVersion -ne $fixture.NuGetVersion -or $record.IsPrerelease -ne $fixture.Prerelease) {
            Add-Violation $violations "scripts/PackageVersion.ps1" "Accepted package version normalized incorrectly: $($fixture.Version)"
        }
    }
    catch {
        Add-Violation $violations "scripts/PackageVersion.ps1" "Accepted package version was rejected: $($fixture.Version)"
    }
}

$rejectedPackageVersions = @(
    "5.0.0-preview.1",
    "5.0.0.0-Preview.1",
    "5.0.0.0-preview.01",
    "05.0.0.0-preview.1",
    "5.0.0.0-preview.1+build.7",
    "5.0.0.0-preview..1",
    "5.0.0.0-preview.",
    "5.0.0.0-",
    "5.0.0.2147483648-preview.1"
)
foreach ($fixture in $rejectedPackageVersions) {
    try {
        ConvertTo-OpenCvCSharpPackageVersion -Version $fixture | Out-Null
        Add-Violation $violations "scripts/PackageVersion.ps1" "Malformed package version was accepted: $fixture"
    }
    catch {
    }
}

foreach ($fixture in @(
        [pscustomobject]@{ Version = "5.0.0.0-preview.1"; OpenCvVersion = "5.0.1"; Revision = 0 },
        [pscustomobject]@{ Version = "5.0.0.0-preview.1"; OpenCvVersion = "5.0.0"; Revision = 1 }
    )) {
    try {
        Assert-OpenCvCSharpPackageVersion `
            -Version $fixture.Version `
            -OpenCvVersion $fixture.OpenCvVersion `
            -PackageRevision $fixture.Revision | Out-Null
        Add-Violation $violations "scripts/PackageVersion.ps1" "Mismatched package version identity was accepted: $($fixture.Version) / $($fixture.OpenCvVersion) / $($fixture.Revision)"
    }
    catch {
    }
}

$managedProjectPath = "src/OpenCvSharp/OpenCvSharp.csproj"
$managedProject = Read-XmlProject -RelativePath $managedProjectPath
$managedPackageIds = @(Resolve-ProjectPropertyValues -Values @(Get-ProjectPropertyValues -Project $managedProject -Name "PackageId") -Properties $centralProperties)
$managedAssemblyNames = @(Resolve-ProjectPropertyValues -Values @(Get-ProjectPropertyValues -Project $managedProject -Name "AssemblyName") -Properties $centralProperties)
$managedRootNamespaces = @(Resolve-ProjectPropertyValues -Values @(Get-ProjectPropertyValues -Project $managedProject -Name "RootNamespace") -Properties $centralProperties)
$managedVersions = @(Resolve-ProjectPropertyValues -Values @(Get-ProjectPropertyValues -Project $managedProject -Name "Version") -Properties $centralProperties)
$managedPackageVersions = @(Resolve-ProjectPropertyValues -Values @(Get-ProjectPropertyValues -Project $managedProject -Name "PackageVersion") -Properties $centralProperties)

if ($managedPackageIds.Count -ne 1 -or $managedPackageIds[0] -ne $primaryManagedPackageId) {
    Add-Violation $violations $managedProjectPath "Managed project PackageId must be $primaryManagedPackageId"
}

if ($managedAssemblyNames.Count -ne 1 -or $managedAssemblyNames[0] -ne $primaryManagedAssemblyName) {
    Add-Violation $violations $managedProjectPath "Managed project AssemblyName must be $primaryManagedAssemblyName"
}

if ($managedRootNamespaces.Count -ne 1 -or $managedRootNamespaces[0] -ne $primaryManagedRootNamespace) {
    Add-Violation $violations $managedProjectPath "Managed project RootNamespace must be $primaryManagedRootNamespace"
}

if ($managedVersions.Count -ne 1 -or -not (Test-FourPartVersion -Value $managedVersions[0])) {
    Add-Violation $violations $managedProjectPath "Managed project Version must resolve to four numeric parts"
}

if ($managedPackageVersions.Count -ne 1 -or -not (Test-FourPartVersion -Value $managedPackageVersions[0])) {
    Add-Violation $violations $managedProjectPath "Managed project PackageVersion must resolve to four numeric parts"
}

if ($managedVersions.Count -eq 1 -and $managedPackageVersions.Count -eq 1 -and $managedVersions[0] -ne $managedPackageVersions[0]) {
    Add-Violation $violations $managedProjectPath "Managed project Version and PackageVersion should match so OpenCV runtime identity stays in package version metadata"
}

$runtimeProjectFiles = @(
    Get-ChildItem -LiteralPath (Join-Path $repo "packaging/runtime") -Recurse -File -Filter "*.csproj" |
        Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" } |
        Sort-Object FullName
)

if ($runtimeProjectFiles.Count -eq 0) {
    throw "No runtime package project files were found under packaging/runtime"
}

foreach ($runtimeProjectFile in $runtimeProjectFiles) {
    $relativePath = Get-RepositoryRelativePath -Path $runtimeProjectFile.FullName
    [xml]$runtimeProject = [System.IO.File]::ReadAllText($runtimeProjectFile.FullName)
    $runtimePackageIds = @(Resolve-ProjectPropertyValues -Values @(Get-ProjectPropertyValues -Project $runtimeProject -Name "PackageId") -Properties $centralProperties)
    $runtimeVersions = @(Resolve-ProjectPropertyValues -Values @(Get-ProjectPropertyValues -Project $runtimeProject -Name "Version") -Properties $centralProperties)
    $runtimePackageVersions = @(Resolve-ProjectPropertyValues -Values @(Get-ProjectPropertyValues -Project $runtimeProject -Name "PackageVersion") -Properties $centralProperties)
    $runtimeRidValues = @(Get-ProjectPropertyValues -Project $runtimeProject -Name "RuntimePackageRid")
    $runtimeProfileValues = @(Get-ProjectPropertyValues -Project $runtimeProject -Name "RuntimePackageProfile")

    if ($runtimePackageIds.Count -ne 1 -or $runtimePackageIds[0] -ne "$runtimePackagePrefix.`$(RuntimePackageRid)`$(RuntimePackageProfileSuffix)") {
        Add-Violation $violations $relativePath "Runtime PackageId must resolve to $runtimePackagePrefix.`$(RuntimePackageRid)`$(RuntimePackageProfileSuffix)"
    }

    if ($runtimeRidValues.Count -lt 1) {
        Add-Violation $violations $relativePath "Runtime package project must define RuntimePackageRid"
    }

    if ($runtimeProfileValues.Count -lt 1) {
        Add-Violation $violations $relativePath "Runtime package project must define RuntimePackageProfile"
    }

    if ($runtimeVersions.Count -ne 1 -or -not (Test-FourPartVersion -Value $runtimeVersions[0])) {
        Add-Violation $violations $relativePath "Runtime project Version must resolve to four numeric parts"
    }

    if ($runtimePackageVersions.Count -ne 1 -or -not (Test-FourPartVersion -Value $runtimePackageVersions[0])) {
        Add-Violation $violations $relativePath "Runtime project PackageVersion must resolve to four numeric parts"
    }

    if ($runtimeVersions.Count -eq 1 -and $runtimePackageVersions.Count -eq 1 -and $runtimeVersions[0] -ne $runtimePackageVersions[0]) {
        Add-Violation $violations $relativePath "Runtime project Version and PackageVersion should match so OpenCV runtime identity stays in package version metadata"
    }
}

$projectFiles = @(
    Get-ChildItem -LiteralPath $repo -Recurse -File -Include "*.csproj", "*.props", "*.targets", "*.nuspec" |
        Where-Object { $_.FullName -notmatch "\\(bin|obj|artifacts|packages)\\" } |
        Sort-Object FullName
)

foreach ($projectFile in $projectFiles) {
    $relativePath = Get-RepositoryRelativePath -Path $projectFile.FullName
    $text = [System.IO.File]::ReadAllText($projectFile.FullName)

    foreach ($identityElementName in @("PackageId", "AssemblyName", "RootNamespace")) {
        $pattern = "<$identityElementName>[^<]*$([System.Text.RegularExpressions.Regex]::Escape($fixedMajorManagedIdentity))"
        if ($text -match $pattern) {
            Add-Violation $violations $relativePath "$identityElementName must not use the fixed-major managed identity"
        }
    }

    $packageReferencePattern = ("Package" + "Reference[^`n`r]*Open" + "Cv5Sharp")
    if ($text -match $packageReferencePattern) {
        Add-Violation $violations $relativePath "Project PackageReference must not use fixed-major managed package identity"
    }
}

$packManagedPath = "scripts/Pack-Managed.ps1"
$packRuntimePath = "scripts/Pack-Runtime.ps1"
$stageRuntimePath = "scripts/Stage-Runtime.ps1"
$packManagedText = Read-RequiredText -RelativePath $packManagedPath
$packRuntimeText = Read-RequiredText -RelativePath $packRuntimePath
$stageRuntimeText = Read-RequiredText -RelativePath $stageRuntimePath

foreach ($packScript in @(
        [pscustomobject]@{ Path = $packManagedPath; Text = $packManagedText },
        [pscustomobject]@{ Path = $packRuntimePath; Text = $packRuntimeText }
    )) {
    if (-not (Test-ContainsText -Text $packScript.Text -Needle 'PackageVersion.ps1') -or
        -not (Test-ContainsText -Text $packScript.Text -Needle 'Assert-OpenCvCSharpPackageVersion')) {
        Add-Violation $violations $packScript.Path "Pack script must use the shared fail-closed package version contract"
    }
}

if (-not (Test-ContainsText -Text $packManagedText -Needle "OpenCvCSharpManagedPackageId") -or
    -not (Test-ContainsText -Text $packManagedText -Needle "`$managedPackageId = Get-RequiredDirectoryBuildProperty")) {
    Add-Violation $violations $packManagedPath "Pack-Managed must derive the version-neutral managed package ID from Directory.Build.props"
}

foreach ($requiredProperty in @(
        "OpenCvCSharpOpenCvVersion",
        "OpenCvCSharpPackageRevision",
        "OpenCvCSharpPackageVersion")) {
    if (-not (Test-ContainsText -Text $packManagedText -Needle $requiredProperty)) {
        Add-Violation $violations $packManagedPath "Pack-Managed must derive $requiredProperty from Directory.Build.props"
    }
}

if (-not (Test-ContainsText -Text $packRuntimeText -Needle "OpenCvCSharpRuntimePackageIdPrefix") -or
    -not (Test-ContainsText -Text $packRuntimeText -Needle "`$runtimePackagePrefix = Get-RequiredDirectoryBuildProperty")) {
    Add-Violation $violations $packRuntimePath "Pack-Runtime must derive the version-neutral runtime package prefix from Directory.Build.props"
}

foreach ($requiredProperty in @(
        "OpenCvCSharpOpenCvVersion",
        "OpenCvCSharpPackageRevision",
        "OpenCvCSharpPackageVersion")) {
    if (-not (Test-ContainsText -Text $packRuntimeText -Needle $requiredProperty)) {
        Add-Violation $violations $packRuntimePath "Pack-Runtime must derive $requiredProperty from Directory.Build.props"
    }
}

if (-not (Test-ContainsText -Text $packRuntimeText -Needle '$runtimePackageId = "$runtimePackagePrefix.$Rid$runtimePackageSuffix"')) {
    Add-Violation $violations $packRuntimePath "Pack-Runtime must derive runtime package ID from $runtimePackagePrefix"
}

if (-not (Test-ContainsText -Text $packRuntimeText -Needle "`"-p:PackageId=`$runtimePackageId`"")) {
    Add-Violation $violations $packRuntimePath "Pack-Runtime must pass the derived neutral runtime package ID to dotnet pack"
}

if (-not (Test-ContainsText -Text $stageRuntimeText -Needle "`"$primaryNativeLoader.dll`"")) {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must name $primaryNativeLoader.dll as the Windows primary loader"
}

if (-not (Test-ContainsText -Text $stageRuntimeText -Needle "`"lib$primaryNativeLoader.so`"")) {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must name lib$primaryNativeLoader.so as the non-Windows primary loader"
}

if (-not (Test-ContainsText -Text $packManagedText -Needle "PackageVersion carries OpenCV runtime identity as version metadata")) {
    Add-Violation $violations $packManagedPath "Pack-Managed must document PackageVersion as version metadata, not package identity"
}

if (-not (Test-ContainsText -Text $packRuntimeText -Needle "PackageVersion carries OpenCV runtime identity as version metadata")) {
    Add-Violation $violations $packRuntimePath "Pack-Runtime must document PackageVersion as version metadata, not package identity"
}

if ($violations.Count -gt 0) {
    Write-Host "Package metadata neutrality guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue -AutoSize
    exit 1
}

Write-Host "Package metadata neutrality guard passed."
Write-Host "Project metadata files scanned: $($projectFiles.Count)."
Write-Host "Runtime package projects scanned: $($runtimeProjectFiles.Count)."
Write-Host "Managed package ID: $primaryManagedPackageId."
Write-Host "Runtime package prefix: $runtimePackagePrefix."
Write-Host "Package version fixtures: $($acceptedPackageVersions.Count) accepted, $($rejectedPackageVersions.Count + 2) rejected."
