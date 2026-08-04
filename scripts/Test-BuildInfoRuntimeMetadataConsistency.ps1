param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path

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

function Read-RequiredText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required metadata file was not found: $path"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Read-RequiredXml {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    [xml]$xml = Read-RequiredText -RelativePath $RelativePath
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

function Get-SingleProjectProperty {
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

    $uniqueValues = @($values | Sort-Object -Unique)
    if ($uniqueValues.Count -ne 1) {
        return ""
    }

    return $uniqueValues[0]
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

function Get-RegexValue {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Pattern
    )

    $match = [regex]::Match($Text, $Pattern)
    if (-not $match.Success) {
        return ""
    }

    return $match.Groups["value"].Value
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

function Assert-Equals {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [AllowNull()]
        [string]$Actual,
        [Parameter(Mandatory = $true)]
        [string]$Expected
    )

    if (-not $Expected.Equals($Actual, [System.StringComparison]::Ordinal)) {
        Add-Violation $Violations $Path "$Name must be '$Expected'. Actual: '$Actual'"
    }
}

$violations = [System.Collections.Generic.List[object]]::new()

$managedProjectPath = "src/OpenCvSharp/OpenCvSharp.csproj"
$runtimeProjectPath = "packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj"
$directoryBuildPropsPath = "Directory.Build.props"
$buildInfoPath = "src/OpenCvSharp/OpenCvSharpBuildInfo.cs"
$nativeLibraryNamesPath = "src/OpenCvSharp/Internal/Interop/NativeLibraryNames.cs"
$nativeCMakePath = "src/OpenCvSharp.Native/CMakeLists.txt"
$nativeVersionPath = "src/OpenCvSharp.Native/src/version.cpp"
$stageRuntimePath = "scripts/Stage-Runtime.ps1"

$managedProject = Read-RequiredXml -RelativePath $managedProjectPath
$runtimeProject = Read-RequiredXml -RelativePath $runtimeProjectPath
$directoryBuildProps = Read-RequiredXml -RelativePath $directoryBuildPropsPath
$centralProperties = Get-DirectoryBuildProperties -Project $directoryBuildProps
$expectedManagedPackageId = Get-RequiredDirectoryBuildProperty $centralProperties "OpenCvCSharpManagedPackageId"
$expectedRuntimePackagePrefix = Get-RequiredDirectoryBuildProperty $centralProperties "OpenCvCSharpRuntimePackageIdPrefix"
$expectedRootNamespace = Get-RequiredDirectoryBuildProperty $centralProperties "OpenCvCSharpRootNamespace"
$expectedOpenCvVersion = Get-RequiredDirectoryBuildProperty $centralProperties "OpenCvCSharpOpenCvVersion"
$expectedPackageVersion = Get-RequiredDirectoryBuildProperty $centralProperties "OpenCvCSharpPackageVersion"
$expectedCurrentNativeLibraryName = Get-RequiredDirectoryBuildProperty $centralProperties "OpenCvCSharpCurrentNativeLibraryName"
$buildInfoText = Read-RequiredText -RelativePath $buildInfoPath
$nativeLibraryNamesText = Read-RequiredText -RelativePath $nativeLibraryNamesPath
$nativeCMakeText = Read-RequiredText -RelativePath $nativeCMakePath
$nativeVersionText = Read-RequiredText -RelativePath $nativeVersionPath
$stageRuntimeText = Read-RequiredText -RelativePath $stageRuntimePath

$buildInfoManagedPackageId = Get-RegexValue $buildInfoText 'public\s+const\s+string\s+ManagedPackageId\s*=\s*"(?<value>[^"]+)";'
$buildInfoRuntimePackagePrefix = Get-RegexValue $buildInfoText 'public\s+const\s+string\s+RuntimePackageIdPrefix\s*=\s*"(?<value>[^"]+)";'
$buildInfoOpenCvVersion = Get-RegexValue $buildInfoText 'public\s+const\s+string\s+OpenCvVersion\s*=\s*"(?<value>[^"]+)";'
$buildInfoPackageVersion = Get-RegexValue $buildInfoText 'public\s+const\s+string\s+PackageVersion\s*=\s*"(?<value>[^"]+)";'
$buildInfoCurrentNativeLibraryName = Get-RegexValue $buildInfoText 'public\s+const\s+string\s+CurrentNativeLibraryName\s*=\s*"(?<value>[^"]+)";'

Assert-Equals $violations $buildInfoPath "OpenCvSharpBuildInfo.ManagedPackageId" $buildInfoManagedPackageId $expectedManagedPackageId
Assert-Equals $violations $buildInfoPath "OpenCvSharpBuildInfo.RuntimePackageIdPrefix" $buildInfoRuntimePackagePrefix $expectedRuntimePackagePrefix
Assert-Equals $violations $buildInfoPath "OpenCvSharpBuildInfo.OpenCvVersion" $buildInfoOpenCvVersion $expectedOpenCvVersion
Assert-Equals $violations $buildInfoPath "OpenCvSharpBuildInfo.PackageVersion" $buildInfoPackageVersion $expectedPackageVersion
Assert-Equals $violations $buildInfoPath "OpenCvSharpBuildInfo.CurrentNativeLibraryName" $buildInfoCurrentNativeLibraryName $expectedCurrentNativeLibraryName
if ($buildInfoText -match 'LegacyNativeLibraryName|public\s+const\s+string\s+NativeLibraryName') {
    Add-Violation $violations $buildInfoPath "Build info must not expose an unpublished native loader compatibility alias"
}

Assert-Equals $violations $managedProjectPath "Managed project AssemblyName" (Resolve-PropertyReferences -Value (Get-SingleProjectProperty $managedProject "AssemblyName") -Properties $centralProperties) $expectedManagedPackageId
Assert-Equals $violations $managedProjectPath "Managed project PackageId" (Resolve-PropertyReferences -Value (Get-SingleProjectProperty $managedProject "PackageId") -Properties $centralProperties) $expectedManagedPackageId
Assert-Equals $violations $managedProjectPath "Managed project RootNamespace" (Resolve-PropertyReferences -Value (Get-SingleProjectProperty $managedProject "RootNamespace") -Properties $centralProperties) $expectedRootNamespace
Assert-Equals $violations $managedProjectPath "Managed project Version" (Resolve-PropertyReferences -Value (Get-SingleProjectProperty $managedProject "Version") -Properties $centralProperties) $expectedPackageVersion
Assert-Equals $violations $managedProjectPath "Managed project PackageVersion" (Resolve-PropertyReferences -Value (Get-SingleProjectProperty $managedProject "PackageVersion") -Properties $centralProperties) $expectedPackageVersion

Assert-Equals $violations $runtimeProjectPath "Runtime project Version" (Resolve-PropertyReferences -Value (Get-SingleProjectProperty $runtimeProject "Version") -Properties $centralProperties) $expectedPackageVersion
Assert-Equals $violations $runtimeProjectPath "Runtime project PackageVersion" (Resolve-PropertyReferences -Value (Get-SingleProjectProperty $runtimeProject "PackageVersion") -Properties $centralProperties) $expectedPackageVersion
$runtimePackageId = Resolve-PropertyReferences -Value (Get-SingleProjectProperty $runtimeProject "PackageId") -Properties $centralProperties
if (-not $runtimePackageId.StartsWith("$expectedRuntimePackagePrefix.", [System.StringComparison]::Ordinal)) {
    Add-Violation $violations $runtimeProjectPath "Runtime project PackageId must start with '$expectedRuntimePackagePrefix.'. Actual: '$runtimePackageId'"
}

$nativeCurrentLibraryName = Get-RegexValue $nativeLibraryNamesText 'internal\s+const\s+string\s+CurrentNativeLibrary\s*=\s*"(?<value>[^"]+)";'
Assert-Equals $violations $nativeLibraryNamesPath "NativeLibraryNames.CurrentNativeLibrary" $nativeCurrentLibraryName $expectedCurrentNativeLibraryName
Assert-Equals $violations $nativeLibraryNamesPath "NativeLibraryNames.CurrentNativeLibrary vs build-info" $nativeCurrentLibraryName $buildInfoCurrentNativeLibraryName
if ($nativeLibraryNamesText -match 'LegacyNativeLibrary') {
    Add-Violation $violations $nativeLibraryNamesPath "Managed interop must not retain an unpublished native loader compatibility alias"
}

$cmakeProjectVersion = Get-RegexValue $nativeCMakeText 'project\s*\(\s*OpenCvCSharpNative\s+VERSION\s+(?<value>\d+\.\d+\.\d+)\s+LANGUAGES\s+CXX\s*\)'
$cmakeCurrentTarget = Get-RegexValue $nativeCMakeText 'set\s*\(\s*OPENCV_CSHARP_NATIVE_TARGET\s+"(?<value>[^"]+)"\s*\)'
Assert-Equals $violations $nativeCMakePath "Native CMake project version" $cmakeProjectVersion $expectedOpenCvVersion
Assert-Equals $violations $nativeCMakePath "Native CMake primary target" $cmakeCurrentTarget $expectedCurrentNativeLibraryName
if ($nativeCMakeText -match 'COMPATIBILITY_NATIVE_TARGET') {
    Add-Violation $violations $nativeCMakePath "Native CMake must not retain an unpublished compatibility target"
}

$nativeNoOpenCvVersion = Get-RegexValue $nativeVersionText 'return\s+"(?<value>\d+\.\d+\.\d+)";'
Assert-Equals $violations $nativeVersionPath "No-OpenCV native version fallback" $nativeNoOpenCvVersion $expectedOpenCvVersion

foreach ($scriptPath in @(
        "scripts/Build-OpenCV.ps1",
        "scripts/Generate-MatTypeConstants.ps1",
        "scripts/New-SyntheticRuntimeInputs.ps1",
        "scripts/Stage-Runtime.ps1")) {
    $scriptText = Read-RequiredText -RelativePath $scriptPath
    $defaultOpenCvVersion = Get-RegexValue $scriptText '\[string\]\$OpenCvVersion\s*=\s*"(?<value>[^"]+)"'
    Assert-Equals $violations $scriptPath "Default OpenCvVersion" $defaultOpenCvVersion $expectedOpenCvVersion
}

foreach ($scriptPath in @(
        "scripts/Pack-Managed.ps1",
        "scripts/Pack-Runtime.ps1")) {
    $scriptText = Read-RequiredText -RelativePath $scriptPath
    $defaultOpenCvVersion = Get-RegexValue $scriptText '\[string\]\$OpenCvVersion\s*=\s*"(?<value>[^"]*)"'
    if ($defaultOpenCvVersion.Length -ne 0) {
        Add-Violation $violations $scriptPath "Pack script OpenCvVersion default must be empty so Directory.Build.props supplies the default. Actual: '$defaultOpenCvVersion'"
    }

    foreach ($requiredProperty in @(
            "OpenCvCSharpOpenCvVersion",
            "OpenCvCSharpPackageRevision",
            "OpenCvCSharpPackageVersion")) {
        if (-not (Test-ContainsText -Text $scriptText -Needle $requiredProperty)) {
            Add-Violation $violations $scriptPath "Pack script must derive $requiredProperty from Directory.Build.props"
        }
    }
}

if (-not (Test-ContainsText -Text $stageRuntimeText -Needle "`"$expectedCurrentNativeLibraryName.dll`"")) {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must stage $expectedCurrentNativeLibraryName.dll as the Windows primary loader"
}

if (-not (Test-ContainsText -Text $stageRuntimeText -Needle "`"lib$expectedCurrentNativeLibraryName.so`"")) {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must stage lib$expectedCurrentNativeLibraryName.so as the non-Windows primary loader"
}

if ($stageRuntimeText -match 'compatibilityNativeLoader|LegacyNativeLibrary') {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must stage only the neutral native loader"
}

if ($violations.Count -gt 0) {
    Write-Host "Build-info/runtime metadata consistency guard failed with $($violations.Count) issue(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue -AutoSize
    exit 1
}

Write-Host "Build-info/runtime metadata consistency guard passed."
Write-Host "Managed package ID: $expectedManagedPackageId."
Write-Host "Package/OpenCV version: $expectedPackageVersion / $expectedOpenCvVersion."
Write-Host "Native loader: $expectedCurrentNativeLibraryName."
