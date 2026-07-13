param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$expectedManagedPackageId = "JYPPX.OpenCV.CSharp.API"
$expectedRuntimePackagePrefix = "JYPPX.OpenCV.runtime"
$expectedRootNamespace = "OpenCvSharp"
$expectedOpenCvVersion = "5.0.0"
$expectedPackageVersion = "5.0.0.0"
$expectedCurrentNativeLibraryName = "JYPPX.OpenCV.Native"
$expectedCompatibilityNativeLibraryName = "OpenCv5Sharp.Native"

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
$buildInfoPath = "src/OpenCvSharp/OpenCvSharpBuildInfo.cs"
$nativeLibraryNamesPath = "src/OpenCvSharp/Internal/Interop/NativeLibraryNames.cs"
$nativeCMakePath = "src/OpenCvSharp.Native/CMakeLists.txt"
$nativeVersionPath = "src/OpenCvSharp.Native/src/version.cpp"
$stageRuntimePath = "scripts/Stage-Runtime.ps1"

$managedProject = Read-RequiredXml -RelativePath $managedProjectPath
$runtimeProject = Read-RequiredXml -RelativePath $runtimeProjectPath
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
$buildInfoLegacyNativeLibraryName = Get-RegexValue $buildInfoText 'public\s+const\s+string\s+LegacyNativeLibraryName\s*=\s*"(?<value>[^"]+)";'

Assert-Equals $violations $buildInfoPath "OpenCvSharpBuildInfo.ManagedPackageId" $buildInfoManagedPackageId $expectedManagedPackageId
Assert-Equals $violations $buildInfoPath "OpenCvSharpBuildInfo.RuntimePackageIdPrefix" $buildInfoRuntimePackagePrefix $expectedRuntimePackagePrefix
Assert-Equals $violations $buildInfoPath "OpenCvSharpBuildInfo.OpenCvVersion" $buildInfoOpenCvVersion $expectedOpenCvVersion
Assert-Equals $violations $buildInfoPath "OpenCvSharpBuildInfo.PackageVersion" $buildInfoPackageVersion $expectedPackageVersion
Assert-Equals $violations $buildInfoPath "OpenCvSharpBuildInfo.CurrentNativeLibraryName" $buildInfoCurrentNativeLibraryName $expectedCurrentNativeLibraryName
Assert-Equals $violations $buildInfoPath "OpenCvSharpBuildInfo.LegacyNativeLibraryName" $buildInfoLegacyNativeLibraryName $expectedCompatibilityNativeLibraryName

if (-not (Test-ContainsText -Text $buildInfoText -Needle "public const string NativeLibraryName = LegacyNativeLibraryName;")) {
    Add-Violation $violations $buildInfoPath "OpenCvSharpBuildInfo.NativeLibraryName must forward to LegacyNativeLibraryName"
}

Assert-Equals $violations $managedProjectPath "Managed project AssemblyName" (Get-SingleProjectProperty $managedProject "AssemblyName") $expectedManagedPackageId
Assert-Equals $violations $managedProjectPath "Managed project PackageId" (Get-SingleProjectProperty $managedProject "PackageId") $expectedManagedPackageId
Assert-Equals $violations $managedProjectPath "Managed project RootNamespace" (Get-SingleProjectProperty $managedProject "RootNamespace") $expectedRootNamespace
Assert-Equals $violations $managedProjectPath "Managed project Version" (Get-SingleProjectProperty $managedProject "Version") $expectedPackageVersion
Assert-Equals $violations $managedProjectPath "Managed project PackageVersion" (Get-SingleProjectProperty $managedProject "PackageVersion") $expectedPackageVersion

Assert-Equals $violations $runtimeProjectPath "Runtime project Version" (Get-SingleProjectProperty $runtimeProject "Version") $expectedPackageVersion
Assert-Equals $violations $runtimeProjectPath "Runtime project PackageVersion" (Get-SingleProjectProperty $runtimeProject "PackageVersion") $expectedPackageVersion
$runtimePackageId = Get-SingleProjectProperty $runtimeProject "PackageId"
if (-not $runtimePackageId.StartsWith("$expectedRuntimePackagePrefix.", [System.StringComparison]::Ordinal)) {
    Add-Violation $violations $runtimeProjectPath "Runtime project PackageId must start with '$expectedRuntimePackagePrefix.'. Actual: '$runtimePackageId'"
}

$nativeCurrentLibraryName = Get-RegexValue $nativeLibraryNamesText 'internal\s+const\s+string\s+CurrentNativeLibrary\s*=\s*"(?<value>[^"]+)";'
$nativeLegacyLibraryName = Get-RegexValue $nativeLibraryNamesText 'internal\s+const\s+string\s+LegacyNativeLibrary\s*=\s*"(?<value>[^"]+)";'
Assert-Equals $violations $nativeLibraryNamesPath "NativeLibraryNames.CurrentNativeLibrary" $nativeCurrentLibraryName $expectedCurrentNativeLibraryName
Assert-Equals $violations $nativeLibraryNamesPath "NativeLibraryNames.LegacyNativeLibrary" $nativeLegacyLibraryName $expectedCompatibilityNativeLibraryName
Assert-Equals $violations $nativeLibraryNamesPath "NativeLibraryNames.CurrentNativeLibrary vs build-info" $nativeCurrentLibraryName $buildInfoCurrentNativeLibraryName
Assert-Equals $violations $nativeLibraryNamesPath "NativeLibraryNames.LegacyNativeLibrary vs build-info" $nativeLegacyLibraryName $buildInfoLegacyNativeLibraryName

$cmakeProjectVersion = Get-RegexValue $nativeCMakeText 'project\s*\(\s*OpenCvCSharpNative\s+VERSION\s+(?<value>\d+\.\d+\.\d+)\s+LANGUAGES\s+CXX\s*\)'
$cmakeCurrentTarget = Get-RegexValue $nativeCMakeText 'set\s*\(\s*OPENCV_CSHARP_NATIVE_TARGET\s+"(?<value>[^"]+)"\s*\)'
$cmakeCompatibilityTarget = Get-RegexValue $nativeCMakeText 'set\s*\(\s*OPENCV_CSHARP_COMPATIBILITY_NATIVE_TARGET\s+"(?<value>[^"]+)"\s*\)'
Assert-Equals $violations $nativeCMakePath "Native CMake project version" $cmakeProjectVersion $expectedOpenCvVersion
Assert-Equals $violations $nativeCMakePath "Native CMake primary target" $cmakeCurrentTarget $expectedCurrentNativeLibraryName
Assert-Equals $violations $nativeCMakePath "Native CMake compatibility target" $cmakeCompatibilityTarget $expectedCompatibilityNativeLibraryName

$nativeNoOpenCvVersion = Get-RegexValue $nativeVersionText 'return\s+"(?<value>\d+\.\d+\.\d+)";'
Assert-Equals $violations $nativeVersionPath "No-OpenCV native version fallback" $nativeNoOpenCvVersion $expectedOpenCvVersion

foreach ($scriptPath in @(
        "scripts/Build-OpenCV.ps1",
        "scripts/Generate-MatTypeConstants.ps1",
        "scripts/New-SyntheticRuntimeInputs.ps1",
        "scripts/Pack-Managed.ps1",
        "scripts/Pack-Runtime.ps1",
        "scripts/Stage-Runtime.ps1")) {
    $scriptText = Read-RequiredText -RelativePath $scriptPath
    $defaultOpenCvVersion = Get-RegexValue $scriptText '\[string\]\$OpenCvVersion\s*=\s*"(?<value>[^"]+)"'
    Assert-Equals $violations $scriptPath "Default OpenCvVersion" $defaultOpenCvVersion $expectedOpenCvVersion
}

if (-not (Test-ContainsText -Text $stageRuntimeText -Needle "`"$expectedCurrentNativeLibraryName.dll`"")) {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must stage $expectedCurrentNativeLibraryName.dll as the Windows primary loader"
}

if (-not (Test-ContainsText -Text $stageRuntimeText -Needle "`"lib$expectedCurrentNativeLibraryName.so`"")) {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must stage lib$expectedCurrentNativeLibraryName.so as the non-Windows primary loader"
}

if (-not (Test-ContainsText -Text $stageRuntimeText -Needle '"Open" + "Cv5Sharp.Native" # compatibility loader')) {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must construct $expectedCompatibilityNativeLibraryName only as an explicit compatibility loader"
}

if (-not (Test-ContainsText -Text $stageRuntimeText -Needle '$compatibilityNativeLoaderBaseName.dll') -or
    -not (Test-ContainsText -Text $stageRuntimeText -Needle 'lib$compatibilityNativeLoaderBaseName.so')) {
    Add-Violation $violations $stageRuntimePath "Stage-Runtime must stage the compatibility loader copy for Windows and non-Windows RIDs"
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
Write-Host "Native loaders: $expectedCurrentNativeLibraryName; compatibility $expectedCompatibilityNativeLibraryName."
