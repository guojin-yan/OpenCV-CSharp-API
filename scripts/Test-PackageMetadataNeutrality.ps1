param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$primaryManagedPackageId = "JYPPX.OpenCV.CSharp.API"
$primaryManagedAssemblyName = "JYPPX.OpenCV.CSharp.API"
$primaryManagedRootNamespace = "OpenCvSharp"
$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$primaryNativeLoader = "JYPPX.OpenCV.Native"
$fixedMajorManagedIdentity = "Open" + "Cv5Sharp"

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

$violations = [System.Collections.Generic.List[object]]::new()

$managedProjectPath = "src/OpenCvSharp/OpenCvSharp.csproj"
$managedProject = Read-XmlProject -RelativePath $managedProjectPath
$managedPackageIds = @(Get-ProjectPropertyValues -Project $managedProject -Name "PackageId")
$managedAssemblyNames = @(Get-ProjectPropertyValues -Project $managedProject -Name "AssemblyName")
$managedRootNamespaces = @(Get-ProjectPropertyValues -Project $managedProject -Name "RootNamespace")
$managedVersions = @(Get-ProjectPropertyValues -Project $managedProject -Name "Version")
$managedPackageVersions = @(Get-ProjectPropertyValues -Project $managedProject -Name "PackageVersion")

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
    Add-Violation $violations $managedProjectPath "Managed project Version must be four numeric parts"
}

if ($managedPackageVersions.Count -ne 1 -or -not (Test-FourPartVersion -Value $managedPackageVersions[0])) {
    Add-Violation $violations $managedProjectPath "Managed project PackageVersion must be four numeric parts"
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
    $runtimePackageIds = @(Get-ProjectPropertyValues -Project $runtimeProject -Name "PackageId")
    $runtimeVersions = @(Get-ProjectPropertyValues -Project $runtimeProject -Name "Version")
    $runtimePackageVersions = @(Get-ProjectPropertyValues -Project $runtimeProject -Name "PackageVersion")
    $runtimeRidValues = @(Get-ProjectPropertyValues -Project $runtimeProject -Name "RuntimePackageRid")
    $runtimeProfileValues = @(Get-ProjectPropertyValues -Project $runtimeProject -Name "RuntimePackageProfile")

    if ($runtimePackageIds.Count -ne 1 -or $runtimePackageIds[0] -ne "$runtimePackagePrefix.`$(RuntimePackageRid)`$(RuntimePackageProfileSuffix)") {
        Add-Violation $violations $relativePath "Runtime PackageId must be $runtimePackagePrefix.`$(RuntimePackageRid)`$(RuntimePackageProfileSuffix)"
    }

    if ($runtimeRidValues.Count -lt 1) {
        Add-Violation $violations $relativePath "Runtime package project must define RuntimePackageRid"
    }

    if ($runtimeProfileValues.Count -lt 1) {
        Add-Violation $violations $relativePath "Runtime package project must define RuntimePackageProfile"
    }

    if ($runtimeVersions.Count -ne 1 -or -not (Test-FourPartVersion -Value $runtimeVersions[0])) {
        Add-Violation $violations $relativePath "Runtime project Version must be four numeric parts"
    }

    if ($runtimePackageVersions.Count -ne 1 -or -not (Test-FourPartVersion -Value $runtimePackageVersions[0])) {
        Add-Violation $violations $relativePath "Runtime project PackageVersion must be four numeric parts"
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

if (-not (Test-ContainsText -Text $packManagedText -Needle "`$managedPackageId = `"$primaryManagedPackageId`"")) {
    Add-Violation $violations $packManagedPath "Pack-Managed must use the version-neutral managed package ID"
}

if (-not (Test-ContainsText -Text $packRuntimeText -Needle '$runtimePackageId = "$runtimePackagePrefix.$Rid$runtimePackageSuffix"')) {
    Add-Violation $violations $packRuntimePath "Pack-Runtime must derive runtime package ID from $runtimePackagePrefix"
}

if (-not (Test-ContainsText -Text $packRuntimeText -Needle "`"-p:PackageId=`$runtimePackageId`"")) {
    Add-Violation $violations $packRuntimePath "Pack-Runtime must pass the derived neutral runtime package ID to dotnet pack"
}

if (-not (Test-ContainsText -Text $stageRuntimeText -Needle "`"JYPPX.OpenCV.Native.dll`"")) {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must name JYPPX.OpenCV.Native.dll as the Windows primary loader"
}

if (-not (Test-ContainsText -Text $stageRuntimeText -Needle "`"libJYPPX.OpenCV.Native.so`"")) {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must name libJYPPX.OpenCV.Native.so as the non-Windows primary loader"
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
