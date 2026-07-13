param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$RuntimeProject = "packaging/runtime/JYPPX.OpenCV.runtime",
    [string]$RuntimePackageMatrix = "packaging/runtime/runtime-package-matrix.json",
    [string]$Rid = "win-x64",
    [string]$RuntimeProfile = "full",
    [string]$RuntimePackageId = "",
    [string]$PackageVersion = "",
    [string]$OpenCvVersion = "",
    [string]$OpenCvRid = "",
    [switch]$AllowSyntheticRuntimeInputs
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path

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

function Resolve-RepoCandidatePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $candidate = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    }
    else {
        Join-Path $repo $Path
    }

    return [System.IO.Path]::GetFullPath($candidate)
}

function Resolve-RuntimeProjectRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $candidate = Resolve-RepoCandidatePath -Path $Path
    if ((Split-Path -Leaf $candidate).EndsWith(".csproj", [System.StringComparison]::OrdinalIgnoreCase)) {
        if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
            throw "Runtime package project file was not found: $candidate"
        }

        return (Split-Path -Parent (Resolve-Path -LiteralPath $candidate).Path)
    }

    if (-not (Test-Path -LiteralPath $candidate -PathType Container)) {
        throw "Runtime package project directory was not found: $candidate"
    }

    return (Resolve-Path -LiteralPath $candidate).Path
}

function Get-JsonProperty {
    param(
        [AllowNull()]
        [object]$Object,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if ($null -eq $Object) {
        return $null
    }

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function ConvertTo-StringArray {
    param([AllowNull()][object]$Value)

    if ($null -eq $Value) {
        return @()
    }

    return @($Value | ForEach-Object { [string]$_ })
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

function Get-RuntimeMatrix {
    param([Parameter(Mandatory = $true)][string]$MatrixPath)

    $matrixCandidate = Resolve-RepoCandidatePath -Path $MatrixPath
    if (-not (Test-Path -LiteralPath $matrixCandidate -PathType Leaf)) {
        throw "Runtime package matrix was not found: $matrixCandidate"
    }

    return Get-Content -LiteralPath $matrixCandidate -Raw | ConvertFrom-Json
}

function Test-WindowsRid {
    param([Parameter(Mandatory = $true)][string]$RuntimeIdentifier)
    return $RuntimeIdentifier.StartsWith("win-", [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-ExpectedNativeLoaderNames {
    param([Parameter(Mandatory = $true)][string]$RuntimeIdentifier)

    $compatibilityNativeLoaderBaseName = "Open" + "Cv5Sharp.Native" # compatibility loader copy for already-compiled consumers
    if (Test-WindowsRid -RuntimeIdentifier $RuntimeIdentifier) {
        return @("JYPPX.OpenCV.Native.dll", "$compatibilityNativeLoaderBaseName.dll")
    }

    return @("libJYPPX.OpenCV.Native.so", "lib$compatibilityNativeLoaderBaseName.so")
}

function Test-StringSequenceEqual {
    param(
        [Parameter(Mandatory = $true)][string[]]$Actual,
        [Parameter(Mandatory = $true)][string[]]$Expected
    )

    if ($Actual.Count -ne $Expected.Count) {
        return $false
    }

    for ($i = 0; $i -lt $Expected.Count; $i++) {
        if (-not $Actual[$i].Equals($Expected[$i], [System.StringComparison]::Ordinal)) {
            return $false
        }
    }

    return $true
}

function Test-StringSetEqual {
    param(
        [Parameter(Mandatory = $true)][string[]]$Actual,
        [Parameter(Mandatory = $true)][string[]]$Expected
    )

    $actualSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($item in $Actual) {
        [void]$actualSet.Add($item)
    }

    $expectedSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($item in $Expected) {
        [void]$expectedSet.Add($item)
    }

    return $actualSet.SetEquals($expectedSet)
}

function Get-RelativePackagePath {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path
    )

    return ([System.IO.Path]::GetRelativePath($Root, $Path) -replace "\\", "/")
}

$violations = [System.Collections.Generic.List[object]]::new()
$runtimeProjectRoot = Resolve-RuntimeProjectRoot -Path $RuntimeProject
$manifestPath = Join-Path $runtimeProjectRoot "build/JYPPX.OpenCV.runtime.provenance.json"
$runtimeNativeDir = Join-Path (Join-Path (Join-Path $runtimeProjectRoot "runtimes") $Rid) "native"
$runtimeLicensesDir = Join-Path $runtimeProjectRoot "licenses"
$runtimeBuildDir = Join-Path $runtimeProjectRoot "build"

$centralProperties = Get-DirectoryBuildProperties -RepositoryRoot $repo
$runtimePackagePrefix = Get-RequiredDirectoryBuildProperty -Properties $centralProperties -Name "OpenCvCSharpRuntimePackageIdPrefix"
$centralPackageVersion = Get-RequiredDirectoryBuildProperty -Properties $centralProperties -Name "OpenCvCSharpPackageVersion"
$centralOpenCvVersion = Get-RequiredDirectoryBuildProperty -Properties $centralProperties -Name "OpenCvCSharpOpenCvVersion"

$matrix = Get-RuntimeMatrix -MatrixPath $RuntimePackageMatrix
$ridSpec = @($matrix.rids | Where-Object { $_.rid -eq $Rid -or $_.opencvRid -eq $Rid } | Select-Object -First 1)
if ($ridSpec.Count -eq 0) {
    throw "RID '$Rid' was not found in runtime package matrix."
}

$profileSpec = @($matrix.profiles | Where-Object { $_.name -eq $RuntimeProfile } | Select-Object -First 1)
if ($profileSpec.Count -eq 0) {
    throw "Runtime profile '$RuntimeProfile' was not found in runtime package matrix."
}

$ridSpec = $ridSpec[0]
$profileSpec = $profileSpec[0]

if ([string]::IsNullOrWhiteSpace($OpenCvRid)) {
    $OpenCvRid = [string]$ridSpec.opencvRid
}

if ([string]::IsNullOrWhiteSpace($OpenCvVersion)) {
    $OpenCvVersion = $centralOpenCvVersion
}

if ([string]::IsNullOrWhiteSpace($PackageVersion)) {
    $PackageVersion = $centralPackageVersion
}

if ([string]::IsNullOrWhiteSpace($RuntimePackageId)) {
    $RuntimePackageId = "$runtimePackagePrefix.$Rid$([string]$profileSpec.packageIdSuffix)"
}

if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    Add-Violation -Violations $violations -Path $manifestPath -Issue "Release candidate preflight requires a generated runtime provenance manifest"
}
else {
    $manifestBytes = [System.IO.File]::ReadAllBytes($manifestPath)
    if ($manifestBytes.Length -ge 3 -and $manifestBytes[0] -eq 0xEF -and $manifestBytes[1] -eq 0xBB -and $manifestBytes[2] -eq 0xBF) {
        Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance manifest must be UTF-8 without BOM"
    }

    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $expectedLoaderNames = Get-ExpectedNativeLoaderNames -RuntimeIdentifier $Rid

    foreach ($check in @(
            [pscustomobject]@{ Name = "PackageId"; Expected = $RuntimePackageId },
            [pscustomobject]@{ Name = "PackageVersion"; Expected = $PackageVersion },
            [pscustomobject]@{ Name = "OpenCvVersion"; Expected = $OpenCvVersion },
            [pscustomobject]@{ Name = "OpenCvRid"; Expected = $OpenCvRid },
            [pscustomobject]@{ Name = "Rid"; Expected = $Rid },
            [pscustomobject]@{ Name = "RuntimeProfile"; Expected = $RuntimeProfile },
            [pscustomobject]@{ Name = "PrimaryNativeLoaderName"; Expected = $expectedLoaderNames[0] },
            [pscustomobject]@{ Name = "CompatibilityNativeLoaderName"; Expected = $expectedLoaderNames[1] })) {
        $actual = [string](Get-JsonProperty -Object $manifest -Name $check.Name)
        if (-not $actual.Equals($check.Expected, [System.StringComparison]::Ordinal)) {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance $($check.Name) must match selected release candidate input" -Text "Expected '$($check.Expected)', found '$actual'"
        }
    }

    $isSynthetic = [bool](Get-JsonProperty -Object $manifest -Name "SyntheticRuntimeInputs")
    if ($isSynthetic -and -not $AllowSyntheticRuntimeInputs) {
        Add-Violation -Violations $violations -Path $manifestPath -Issue "Release candidate preflight rejects synthetic runtime inputs"
    }

    $requiredModules = ConvertTo-StringArray -Value (Get-JsonProperty -Object $manifest -Name "RequiredModules")
    $expectedRequiredModules = ConvertTo-StringArray -Value $profileSpec.modules
    if (-not (Test-StringSequenceEqual -Actual $requiredModules -Expected $expectedRequiredModules)) {
        Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance required modules must match selected profile" -Text "Expected '$($expectedRequiredModules -join ",")', found '$($requiredModules -join ",")'"
    }

    $optionalModulesRequested = ConvertTo-StringArray -Value (Get-JsonProperty -Object $manifest -Name "OptionalModulesRequested")
    $expectedOptionalModules = ConvertTo-StringArray -Value $profileSpec.optionalModules
    if (-not (Test-StringSequenceEqual -Actual $optionalModulesRequested -Expected $expectedOptionalModules)) {
        Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance optional modules must match selected profile" -Text "Expected '$($expectedOptionalModules -join ",")', found '$($optionalModulesRequested -join ",")'"
    }

    $runtimeFiles = @((Get-JsonProperty -Object $manifest -Name "RuntimeFiles"))
    $runtimeFileNames = @($runtimeFiles | ForEach-Object { [string](Get-JsonProperty -Object $_ -Name "FileName") } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if (-not $runtimeFileNames.Contains($expectedLoaderNames[0])) {
        Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance must include the primary native loader"
    }

    if (-not $runtimeFileNames.Contains($expectedLoaderNames[1])) {
        Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance must include the compatibility native loader copy"
    }

    $openCvBinarySuffix = (($OpenCvVersion -split "\.") | Select-Object -First 3) -join ""
    foreach ($module in $expectedRequiredModules) {
        $escapedModule = [regex]::Escape($module)
        $modulePattern = if (Test-WindowsRid -RuntimeIdentifier $Rid) {
            "^opencv_$escapedModule$openCvBinarySuffix\.dll$"
        }
        else {
            "^libopencv_$escapedModule\.so(?:\..+)?$"
        }

        if (-not @($runtimeFileNames | Where-Object { $_ -match $modulePattern })) {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance must include required OpenCV module '$module'" -Text $modulePattern
        }
    }

    foreach ($runtimeFile in $runtimeFiles) {
        $fileName = [string](Get-JsonProperty -Object $runtimeFile -Name "FileName")
        $sourcePath = [string](Get-JsonProperty -Object $runtimeFile -Name "SourcePath")
        if ([string]::IsNullOrWhiteSpace($fileName) -or [string]::IsNullOrWhiteSpace($sourcePath)) {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance file entries must include FileName and SourcePath"
            continue
        }

        if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
            Add-Violation -Violations $violations -Path $sourcePath -Issue "Runtime provenance source file must exist for release candidate preflight"
        }

        $packagedRuntimeFile = Join-Path $runtimeNativeDir $fileName
        if (-not (Test-Path -LiteralPath $packagedRuntimeFile -PathType Leaf)) {
            Add-Violation -Violations $violations -Path $packagedRuntimeFile -Issue "Runtime project native mirror must contain every provenance runtime file"
        }
    }

    if (-not (Test-Path -LiteralPath $runtimeNativeDir -PathType Container)) {
        Add-Violation -Violations $violations -Path $runtimeNativeDir -Issue "Runtime project native mirror must exist for selected RID"
    }
    else {
        $actualRuntimeFiles = @(Get-ChildItem -LiteralPath $runtimeNativeDir -File | ForEach-Object { $_.Name })
        if (-not (Test-StringSetEqual -Actual $actualRuntimeFiles -Expected $runtimeFileNames)) {
            Add-Violation -Violations $violations -Path $runtimeNativeDir -Issue "Runtime project native mirror must match provenance exactly and contain no stale files" -Text "Actual '$($actualRuntimeFiles -join ",")'; manifest '$($runtimeFileNames -join ",")'"
        }
    }

    $licenseFiles = @((Get-JsonProperty -Object $manifest -Name "LicenseFiles"))
    $manifestLicensePackagePaths = @($licenseFiles | ForEach-Object { [string](Get-JsonProperty -Object $_ -Name "PackagePath") } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($manifestLicensePackagePaths.Count -eq 0) {
        Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance must include license file evidence"
    }

    foreach ($licenseFile in $licenseFiles) {
        $packagePath = [string](Get-JsonProperty -Object $licenseFile -Name "PackagePath")
        $sourcePath = [string](Get-JsonProperty -Object $licenseFile -Name "SourcePath")
        if ([string]::IsNullOrWhiteSpace($packagePath) -or [string]::IsNullOrWhiteSpace($sourcePath)) {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance license entries must include PackagePath and SourcePath"
            continue
        }

        if (-not $packagePath.StartsWith("licenses/", [System.StringComparison]::OrdinalIgnoreCase)) {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance license package paths must be under licenses/" -Text $packagePath
        }

        $licenseMirrorPath = Join-Path $runtimeProjectRoot ($packagePath -replace "/", [System.IO.Path]::DirectorySeparatorChar)
        if (-not (Test-Path -LiteralPath $licenseMirrorPath -PathType Leaf)) {
            Add-Violation -Violations $violations -Path $licenseMirrorPath -Issue "Runtime project license mirror must contain every provenance license file"
        }

        if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
            Add-Violation -Violations $violations -Path $sourcePath -Issue "Runtime provenance license source file must exist"
        }
    }

    if (-not (Test-Path -LiteralPath $runtimeLicensesDir -PathType Container)) {
        Add-Violation -Violations $violations -Path $runtimeLicensesDir -Issue "Runtime project license mirror must exist"
    }
    else {
        $actualLicensePaths = @(Get-ChildItem -LiteralPath $runtimeLicensesDir -Recurse -File | ForEach-Object { Get-RelativePackagePath -Root $runtimeProjectRoot -Path $_.FullName })
        if (-not (Test-StringSetEqual -Actual $actualLicensePaths -Expected $manifestLicensePackagePaths)) {
            Add-Violation -Violations $violations -Path $runtimeLicensesDir -Issue "Runtime project license mirror must match provenance exactly and contain no stale files" -Text "Actual '$($actualLicensePaths -join ",")'; manifest '$($manifestLicensePackagePaths -join ",")'"
        }
    }

    if (-not (Test-Path -LiteralPath $runtimeBuildDir -PathType Container)) {
        Add-Violation -Violations $violations -Path $runtimeBuildDir -Issue "Runtime project build mirror must exist"
    }
    else {
        $buildFiles = @(Get-ChildItem -LiteralPath $runtimeBuildDir -File | ForEach-Object { $_.Name })
        if (-not (Test-StringSetEqual -Actual $buildFiles -Expected @("JYPPX.OpenCV.runtime.provenance.json"))) {
            Add-Violation -Violations $violations -Path $runtimeBuildDir -Issue "Runtime project build mirror must contain only the current provenance manifest" -Text ($buildFiles -join ",")
        }
    }

    $inputRoots = Get-JsonProperty -Object $manifest -Name "InputRoots"
    foreach ($inputRootName in @("NativeWrapperRuntimeDir", "OpenCvRuntimeDir", "OpenCvSourceDir")) {
        $inputRootPath = [string](Get-JsonProperty -Object $inputRoots -Name $inputRootName)
        if ([string]::IsNullOrWhiteSpace($inputRootPath) -or -not (Test-Path -LiteralPath $inputRootPath -PathType Container)) {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance input root '$inputRootName' must exist for release candidate preflight" -Text $inputRootPath
        }
    }

    $openCvInstallDir = [string](Get-JsonProperty -Object $inputRoots -Name "OpenCvInstallDir")
    if (-not [string]::IsNullOrWhiteSpace($openCvInstallDir) -and -not (Test-Path -LiteralPath $openCvInstallDir -PathType Container)) {
        Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance OpenCvInstallDir must exist when recorded" -Text $openCvInstallDir
    }

    $outputRoots = Get-JsonProperty -Object $manifest -Name "OutputRoots"
    $manifestRuntimeProjectRoot = [string](Get-JsonProperty -Object $outputRoots -Name "RuntimeProjectRoot")
    if ([string]::IsNullOrWhiteSpace($manifestRuntimeProjectRoot) -or -not ([System.IO.Path]::GetFullPath($manifestRuntimeProjectRoot).Equals([System.IO.Path]::GetFullPath($runtimeProjectRoot), [System.StringComparison]::OrdinalIgnoreCase))) {
        Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance RuntimeProjectRoot must match the preflight runtime project"
    }

    $packageManifestPath = [string](Get-JsonProperty -Object $outputRoots -Name "PackageManifestPath")
    if (-not $packageManifestPath.Equals("build/JYPPX.OpenCV.runtime.provenance.json", [System.StringComparison]::Ordinal)) {
        Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance package manifest path must remain stable" -Text $packageManifestPath
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Runtime release-candidate preflight failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-List Path, Issue, Text
    throw "Runtime release-candidate preflight failed."
}

Write-Host "Runtime release-candidate preflight passed."
Write-Host "Runtime package: $RuntimePackageId $PackageVersion."
Write-Host "RID/profile: $Rid / $RuntimeProfile."
