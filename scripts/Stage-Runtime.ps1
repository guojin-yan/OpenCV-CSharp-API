param(
    [string]$Rid = "win-x64",
    [string]$Configuration = "Release",
    [string]$OpenCvNativeRuntimeDir = "",
    [string]$NativeRuntimeDir = "",
    [string]$OpenCvVersion = "5.0.0",
    [string]$OpenCvRid = "",
    [string]$OpenCvRuntimeVersionSuffix = "",
    [string]$OpenCvSourceRoot = "",
    [string]$OpenCvInstallRoot = "",
    [string]$OpenCvRuntimeDir = "",
    [string]$OpenCvInstallDir = "",
    [string]$OpenCvSourceDir = "",
    [string]$OutputRoot = "artifacts/runtime",
    [string]$RuntimeProject = "packaging/runtime/JYPPX.OpenCV.runtime",
    [string]$RuntimePackageMatrix = "packaging/runtime/runtime-package-matrix.json",
    [string]$RuntimeProfile = "full",
    [string]$RuntimePackageId = "",
    [string]$PackageVersion = "",
    [string[]]$OpenCvModules = @(),
    [string[]]$OptionalOpenCvModules = @("xfeatures2d", "xobjdetect", "quality", "xphoto", "ml", "img_hash", "ximgproc", "optflow", "bgsegm", "tracking", "face", "saliency", "plot", "shape", "line_descriptor", "phase_unwrapping", "structured_light", "intensity_transform", "fuzzy", "hfs", "reg", "surface_matching", "rapid", "alphamat", "bioinspired", "xstereo"),
    [switch]$SyntheticRuntimeInputs
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")
$workspaceRoot = Resolve-Path -LiteralPath (Join-Path $repoRoot "..")

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

function Get-RuntimeRidSpec {
    param(
        [Parameter(Mandatory = $true)]
        [string]$MatrixPath,
        [Parameter(Mandatory = $true)]
        [string]$RuntimeIdentifier
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
    $ridSpec = @($matrix.rids | Where-Object { $_.rid -eq $RuntimeIdentifier -or $_.opencvRid -eq $RuntimeIdentifier } | Select-Object -First 1)
    if ($ridSpec.Count -eq 0) {
        throw "RID '$RuntimeIdentifier' was not found in runtime package matrix: $matrixCandidate"
    }

    return $ridSpec[0]
}

function Test-WindowsRid {
    param([Parameter(Mandatory = $true)][string]$RuntimeIdentifier)
    return $RuntimeIdentifier.StartsWith("win-", [System.StringComparison]::OrdinalIgnoreCase)
}

function Test-AndroidRid {
    param([Parameter(Mandatory = $true)][string]$RuntimeIdentifier)
    return $RuntimeIdentifier.StartsWith("android-", [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-WindowsOpenCvArchFolder {
    param([Parameter(Mandatory = $true)][string]$RuntimeIdentifier)

    switch ($RuntimeIdentifier) {
        "win-x64" { return "x64" }
        "win-x86" { return "x86" }
        "win-arm64" { return "ARM64" }
        default { return "x64" }
    }
}

function Get-AndroidAbi {
    param([Parameter(Mandatory = $true)][string]$RuntimeIdentifier)

    switch ($RuntimeIdentifier) {
        "android-arm64" { return "arm64-v8a" }
        "android-arm" { return "armeabi-v7a" }
        "android-x64" { return "x86_64" }
        "android-x86" { return "x86" }
        default { return "" }
    }
}

function Get-NativeLoaderFileNames {
    param([Parameter(Mandatory = $true)][string]$RuntimeIdentifier)

    $compatibilityNativeLoaderBaseName = "Open" + "Cv5Sharp.Native" # compatibility loader for already-compiled consumers
    if (Test-WindowsRid -RuntimeIdentifier $RuntimeIdentifier) {
        return @("JYPPX.OpenCV.Native.dll", "$compatibilityNativeLoaderBaseName.dll")
    }

    return @("libJYPPX.OpenCV.Native.so", "lib$compatibilityNativeLoaderBaseName.so")
}

function Resolve-OpenCvModuleRuntimeFiles {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RuntimeDirectory,
        [Parameter(Mandatory = $true)]
        [string]$Module,
        [Parameter(Mandatory = $true)]
        [string]$RuntimeIdentifier,
        [Parameter(Mandatory = $true)]
        [string]$BinarySuffix,
        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    if (Test-WindowsRid -RuntimeIdentifier $RuntimeIdentifier) {
        return ,(Join-Path $RuntimeDirectory "opencv_$Module$BinarySuffix.dll")
    }

    if (Test-AndroidRid -RuntimeIdentifier $RuntimeIdentifier) {
        return ,(Join-Path $RuntimeDirectory "libopencv_$Module.so")
    }

    # Linux OpenCV libraries carry versioned DT_NEEDED/SONAME entries. Keep the
    # unversioned linker name and every versioned companion in the runtime package.
    $globbed = @(Get-ChildItem -LiteralPath $RuntimeDirectory -Filter "libopencv_$Module.so*" -File -ErrorAction SilentlyContinue | Sort-Object Name)
    if ($globbed.Count -gt 0) {
        return @($globbed.FullName)
    }

    return ,(Join-Path $RuntimeDirectory "libopencv_$Module.so.$Version")
}

function Test-OpenCvRuntimeDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Directory,
        [Parameter(Mandatory = $true)]
        [string]$RuntimeIdentifier,
        [Parameter(Mandatory = $true)]
        [string]$BinarySuffix,
        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    if (-not (Test-Path -LiteralPath $Directory -PathType Container)) {
        return $false
    }

    if (Test-WindowsRid -RuntimeIdentifier $RuntimeIdentifier) {
        return Test-Path -LiteralPath (Join-Path $Directory "opencv_core$BinarySuffix.dll") -PathType Leaf
    }

    if (Test-AndroidRid -RuntimeIdentifier $RuntimeIdentifier) {
        return Test-Path -LiteralPath (Join-Path $Directory "libopencv_core.so") -PathType Leaf
    }

    foreach ($candidate in @(
            (Join-Path $Directory "libopencv_core.so"),
            (Join-Path $Directory "libopencv_core.so.$Version"))) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return $true
        }
    }

    return @(Get-ChildItem -LiteralPath $Directory -Filter "libopencv_core.so*" -File -ErrorAction SilentlyContinue | Select-Object -First 1).Count -gt 0
}

function Get-OpenCvRuntimeDirectoryCandidates {
    param(
        [Parameter(Mandatory = $true)]
        [string]$InstallDirectory,
        [Parameter(Mandatory = $true)]
        [string]$RuntimeIdentifier,
        [Parameter(Mandatory = $true)]
        [string]$Configuration
    )

    if (Test-WindowsRid -RuntimeIdentifier $RuntimeIdentifier) {
        $archFolder = Get-WindowsOpenCvArchFolder -RuntimeIdentifier $RuntimeIdentifier
        return @(
            (Join-Path $InstallDirectory "bin"),
            (Join-Path $InstallDirectory "bin\$Configuration"),
            (Join-Path $InstallDirectory "$archFolder\vc18\bin"),
            (Join-Path $InstallDirectory "$archFolder\vc18\bin\$Configuration")
        )
    }

    if (Test-AndroidRid -RuntimeIdentifier $RuntimeIdentifier) {
        $abi = Get-AndroidAbi -RuntimeIdentifier $RuntimeIdentifier
        return @(
            (Join-Path $InstallDirectory "sdk\native\libs\$abi"),
            (Join-Path $InstallDirectory "lib\$abi"),
            (Join-Path $InstallDirectory "lib"),
            (Join-Path $InstallDirectory "bin")
        )
    }

    return @(
        (Join-Path $InstallDirectory "lib"),
        (Join-Path $InstallDirectory "lib64"),
        (Join-Path $InstallDirectory "bin")
    )
}

function Get-DefaultOpenCvRuntimeDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$WorkspaceRoot,
        [Parameter(Mandatory = $true)]
        [string]$RuntimeVersionSuffix,
        [Parameter(Mandatory = $true)]
        [string]$RuntimeIdentifier,
        [Parameter(Mandatory = $true)]
        [string]$Configuration
    )

    if (Test-WindowsRid -RuntimeIdentifier $RuntimeIdentifier) {
        return Join-Path $WorkspaceRoot "artifacts\opencv-build\opencv-$RuntimeVersionSuffix\bin\$Configuration"
    }

    if (Test-AndroidRid -RuntimeIdentifier $RuntimeIdentifier) {
        $abi = Get-AndroidAbi -RuntimeIdentifier $RuntimeIdentifier
        return Join-Path $WorkspaceRoot "artifacts\opencv-install\opencv-$RuntimeVersionSuffix\sdk\native\libs\$abi"
    }

    return Join-Path $WorkspaceRoot "artifacts\opencv-install\opencv-$RuntimeVersionSuffix\lib"
}

$profileSpec = Get-RuntimeProfileSpec -MatrixPath $RuntimePackageMatrix -Profile $RuntimeProfile
$ridSpec = Get-RuntimeRidSpec -MatrixPath $RuntimePackageMatrix -RuntimeIdentifier $Rid
if (-not $PSBoundParameters.ContainsKey("OpenCvRid") -or [string]::IsNullOrWhiteSpace($OpenCvRid)) {
    $OpenCvRid = [string]$ridSpec.opencvRid
}

if (-not $PSBoundParameters.ContainsKey("OpenCvModules")) {
    $OpenCvModules = @($profileSpec.modules)
}

if (-not $PSBoundParameters.ContainsKey("OptionalOpenCvModules")) {
    $OptionalOpenCvModules = @($profileSpec.optionalModules)
}

function Get-DefaultOpenCvSourceRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$WorkspaceRoot,
        [Parameter(Mandatory = $true)]
        [string]$OpenCvVersion
    )

    # Prefer the version-neutral workspace source root when it exists.
    $neutralSourceRoot = Join-Path $WorkspaceRoot "opencv-source"
    if (Test-Path -LiteralPath $neutralSourceRoot) {
        return $neutralSourceRoot
    }

    $versionMatch = [regex]::Match($OpenCvVersion, "^(\d+)(?:\.|$)")
    if (-not $versionMatch.Success) {
        throw "OpenCvVersion must start with a numeric major version: $OpenCvVersion"
    }

    # Use the major-version source directory only when an existing local checkout still uses that older layout.
    $legacyMajorSourceRoot = Join-Path $WorkspaceRoot "opencv$($versionMatch.Groups[1].Value)-source code"
    if (Test-Path -LiteralPath $legacyMajorSourceRoot) {
        return $legacyMajorSourceRoot
    }

    return $neutralSourceRoot
}

if ([string]::IsNullOrWhiteSpace($OpenCvRuntimeVersionSuffix)) {
    # The suffix carries factual local runtime artifact identity, not a package ID or generic project naming surface.
    $OpenCvRuntimeVersionSuffix = "$OpenCvVersion-$OpenCvRid"
}

if ([string]::IsNullOrWhiteSpace($OpenCvNativeRuntimeDir)) {
    if (-not [string]::IsNullOrWhiteSpace($NativeRuntimeDir)) {
        # OpenCvNativeRuntimeDir is the preferred version-neutral runtime path/staging parameter.
        # NativeRuntimeDir is accepted only as an older existing-packaging-script compatibility alias.
        $OpenCvNativeRuntimeDir = $NativeRuntimeDir
    }
    else {
        # Default native runtime input path is a current local build-output fallback; it is not a runtime package identity or naming surface.
        $OpenCvNativeRuntimeDir = "build\native-opencv-core\Release"
    }
}

if ([string]::IsNullOrWhiteSpace($OpenCvSourceRoot)) {
    $OpenCvSourceRoot = Get-DefaultOpenCvSourceRoot -WorkspaceRoot $workspaceRoot -OpenCvVersion $OpenCvVersion
}

if ([string]::IsNullOrWhiteSpace($OpenCvInstallRoot)) {
    $OpenCvInstallRoot = Join-Path $workspaceRoot "artifacts\opencv-install"
}

# OutputRoot is the version-neutral runtime staging-output root.
# Its default remains the existing artifacts\runtime compatibility directory.
$outputRootCandidate = if ([System.IO.Path]::IsPathRooted($OutputRoot)) {
    $OutputRoot
}
else {
    Join-Path $repoRoot $OutputRoot
}

$outputRootFullPath = [System.IO.Path]::GetFullPath($outputRootCandidate)

$runtimeProjectRootCandidate = if ([System.IO.Path]::IsPathRooted($RuntimeProject)) {
    $RuntimeProject
}
else {
    Join-Path $repoRoot $RuntimeProject
}

$runtimeProjectRootFullPath = [System.IO.Path]::GetFullPath($runtimeProjectRootCandidate)

function Resolve-RepoPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if ([System.IO.Path]::IsPathFullyQualified($Path)) {
        return Resolve-Path -LiteralPath $Path
    }

    return Resolve-Path -LiteralPath (Join-Path $repoRoot $Path)
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

$centralProperties = Get-DirectoryBuildProperties -RepositoryRoot $repoRoot
$runtimePackagePrefix = Get-RequiredDirectoryBuildProperty -Properties $centralProperties -Name "OpenCvCSharpRuntimePackageIdPrefix"
$centralPackageVersion = Get-RequiredDirectoryBuildProperty -Properties $centralProperties -Name "OpenCvCSharpPackageVersion"

if ([string]::IsNullOrWhiteSpace($RuntimePackageId)) {
    $RuntimePackageId = "$runtimePackagePrefix.$Rid$([string]$profileSpec.packageIdSuffix)"
}

if ([string]::IsNullOrWhiteSpace($PackageVersion)) {
    $PackageVersion = $centralPackageVersion
}

if ([string]::IsNullOrWhiteSpace($OpenCvSourceDir)) {
    # Upstream OpenCV source leaf directory includes the selected version as factual source artifact identity.
    $OpenCvSourceDir = Join-Path $OpenCvSourceRoot "opencv-$OpenCvVersion"
}

if ([string]::IsNullOrWhiteSpace($OpenCvInstallDir)) {
    # Local OpenCV install leaf directory carries selected runtime artifact identity, not a package ID or generic naming surface.
    $defaultOpenCvInstallDir = Join-Path $OpenCvInstallRoot "opencv-$OpenCvRuntimeVersionSuffix"
    if (Test-Path -LiteralPath $defaultOpenCvInstallDir) {
        $OpenCvInstallDir = $defaultOpenCvInstallDir
    }
}

if ([string]::IsNullOrWhiteSpace($OpenCvRuntimeDir)) {
    if (-not [string]::IsNullOrWhiteSpace($OpenCvInstallDir)) {
        $resolvedInstallDir = (Resolve-Path -LiteralPath $OpenCvInstallDir).Path
        # Derived only for factual upstream OpenCV runtime probe names such as opencv_core500.dll or libopencv_core.so.
        $openCvBinarySuffix = (($OpenCvVersion -split "\.") | Select-Object -First 3) -join ""
        $installRuntimeCandidates = Get-OpenCvRuntimeDirectoryCandidates `
            -InstallDirectory $resolvedInstallDir `
            -RuntimeIdentifier $Rid `
            -Configuration $Configuration

        foreach ($candidate in $installRuntimeCandidates) {
            if (Test-OpenCvRuntimeDirectory -Directory $candidate -RuntimeIdentifier $Rid -BinarySuffix $openCvBinarySuffix -Version $OpenCvVersion) {
                $OpenCvRuntimeDir = (Resolve-Path -LiteralPath $candidate).Path
                break
            }
        }

        if ([string]::IsNullOrWhiteSpace($OpenCvRuntimeDir)) {
            $installLeafName = Split-Path $resolvedInstallDir -Leaf
            if ($installLeafName.StartsWith("opencv-$OpenCvVersion-", [System.StringComparison]::OrdinalIgnoreCase)) {
                # Reuse the factual local build leaf that matches the selected install artifact identity.
                $OpenCvRuntimeDir = Get-DefaultOpenCvRuntimeDirectory `
                    -WorkspaceRoot $workspaceRoot `
                    -RuntimeVersionSuffix ($installLeafName.Substring("opencv-".Length)) `
                    -RuntimeIdentifier $Rid `
                    -Configuration $Configuration
            }
        }
    }

    if ([string]::IsNullOrWhiteSpace($OpenCvRuntimeDir)) {
        # Default local OpenCV build runtime path is factual artifact metadata derived from version-neutral parameters.
        $OpenCvRuntimeDir = Get-DefaultOpenCvRuntimeDirectory `
            -WorkspaceRoot $workspaceRoot `
            -RuntimeVersionSuffix $OpenCvRuntimeVersionSuffix `
            -RuntimeIdentifier $Rid `
            -Configuration $Configuration
    }
}

if ([string]::IsNullOrWhiteSpace($OpenCvInstallDir)) {
    $runtimeCandidate = Resolve-Path -LiteralPath $OpenCvRuntimeDir
    $openCvInstallCandidates = @(
        (Join-Path $runtimeCandidate "..\..\..\.."),
        (Join-Path $runtimeCandidate "..\..\.."),
        (Join-Path $runtimeCandidate "..\.."),
        (Join-Path $runtimeCandidate "..")
    )

    foreach ($candidate in $openCvInstallCandidates) {
        $licenseCandidate = Join-Path $candidate "etc\licenses"
        if (Test-Path -LiteralPath $licenseCandidate) {
            $OpenCvInstallDir = (Resolve-Path -LiteralPath $candidate).Path
            break
        }
    }
}

$nativeRuntimePath = Resolve-RepoPath $OpenCvNativeRuntimeDir
$openCvRuntimePath = Resolve-Path -LiteralPath $OpenCvRuntimeDir
$openCvSourcePath = Resolve-Path -LiteralPath $OpenCvSourceDir
$openCvInstallPath = if ([string]::IsNullOrWhiteSpace($OpenCvInstallDir)) { $null } else { Resolve-Path -LiteralPath $OpenCvInstallDir }
$stagingNativeDir = Join-Path (Join-Path $outputRootFullPath $Rid) "native"
$runtimeProjectNativeDir = Join-Path (Join-Path (Join-Path $runtimeProjectRootFullPath "runtimes") $Rid) "native"
$runtimeProjectLicenseDir = Join-Path $runtimeProjectRootFullPath "licenses"
$runtimeProjectOpenCvLicenseDir = Join-Path $runtimeProjectLicenseDir "opencv-3rdparty"
$runtimeProjectBuildDir = Join-Path $runtimeProjectRootFullPath "build"
$runtimeProvenanceManifestPath = Join-Path $runtimeProjectBuildDir "JYPPX.OpenCV.runtime.provenance.json"

New-Item -ItemType Directory -Force $stagingNativeDir | Out-Null
New-Item -ItemType Directory -Force $runtimeProjectNativeDir | Out-Null
New-Item -ItemType Directory -Force $runtimeProjectLicenseDir | Out-Null
New-Item -ItemType Directory -Force $runtimeProjectOpenCvLicenseDir | Out-Null
New-Item -ItemType Directory -Force $runtimeProjectBuildDir | Out-Null

# Regenerate staging mirrors from the current runtime inputs only. This avoids
# preserving stale DLLs, license files, or nested generated content when modules
# disappear between builds.
Get-ChildItem -LiteralPath $stagingNativeDir -Force | Remove-Item -Recurse -Force
Get-ChildItem -LiteralPath $runtimeProjectNativeDir -Force | Remove-Item -Recurse -Force
Get-ChildItem -LiteralPath $runtimeProjectLicenseDir -Force | Remove-Item -Recurse -Force
if (Test-Path -LiteralPath $runtimeProvenanceManifestPath -PathType Leaf) {
    Remove-Item -LiteralPath $runtimeProvenanceManifestPath -Force
}
New-Item -ItemType Directory -Force $runtimeProjectOpenCvLicenseDir | Out-Null

# Derived only for factual upstream OpenCV runtime names such as opencv_core500.dll or libopencv_core.so.5.0.0.
$openCvBinarySuffix = (($OpenCvVersion -split "\.") | Select-Object -First 3) -join ""
# JYPPX.OpenCV.Native is the version-neutral primary loader.
# OpenCv5Sharp.Native remains a compatibility loader copy for already-compiled consumers.
$nativeLoaderFileNames = Get-NativeLoaderFileNames -RuntimeIdentifier $Rid
$primaryNativeLoaderFileName = $nativeLoaderFileNames[0]
$compatibilityNativeLoaderCopyFileName = $nativeLoaderFileNames[1]
$runtimeFiles = @(
    (Join-Path $nativeRuntimePath $primaryNativeLoaderFileName),
    (Join-Path $nativeRuntimePath $compatibilityNativeLoaderCopyFileName)
)

foreach ($module in $OpenCvModules) {
    if ([string]::IsNullOrWhiteSpace($module)) {
        continue
    }

    $runtimeFiles += @(Resolve-OpenCvModuleRuntimeFiles -RuntimeDirectory $openCvRuntimePath -Module $module -RuntimeIdentifier $Rid -BinarySuffix $openCvBinarySuffix -Version $OpenCvVersion)
}

$optionalRuntimeFiles = @()
$optionalModulesStaged = @()
foreach ($module in $OptionalOpenCvModules) {
    if ([string]::IsNullOrWhiteSpace($module)) {
        continue
    }

    $optionalCandidates = @(Resolve-OpenCvModuleRuntimeFiles -RuntimeDirectory $openCvRuntimePath -Module $module -RuntimeIdentifier $Rid -BinarySuffix $openCvBinarySuffix -Version $OpenCvVersion)
    $existingOptionalFiles = @($optionalCandidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf })
    if ($existingOptionalFiles.Count -gt 0) {
        $optionalRuntimeFiles += $existingOptionalFiles
        $optionalModulesStaged += $module
    }
    else {
        Write-Warning "Optional OpenCV runtime module was not found and will be skipped: $($optionalCandidates -join ', ')"
    }
}

$runtimeFiles += $optionalRuntimeFiles
$runtimeFiles = @($runtimeFiles | Select-Object -Unique)

foreach ($file in $runtimeFiles) {
    if (-not (Test-Path -LiteralPath $file)) {
        throw "Runtime file was not found: $file"
    }

    Copy-Item -LiteralPath $file -Destination $stagingNativeDir -Force
    Copy-Item -LiteralPath $file -Destination $runtimeProjectNativeDir -Force
}

Write-Host "Copied runtime files:"
foreach ($file in $runtimeFiles) {
    Write-Host (" - " + (Split-Path $file -Leaf))
}

if ($optionalRuntimeFiles.Count -gt 0) {
    Write-Host "Copied optional runtime files:"
    foreach ($file in $optionalRuntimeFiles) {
        Write-Host (" - " + (Split-Path $file -Leaf))
    }
}

$licenseFiles = @(
    (Join-Path $repoRoot "LICENSE"),
    (Join-Path $openCvSourcePath "LICENSE"),
    (Join-Path (Join-Path (Join-Path $openCvSourcePath "3rdparty") "ippicv") "readme.htm")
)

foreach ($file in $licenseFiles) {
    if (Test-Path -LiteralPath $file) {
        Copy-Item -LiteralPath $file -Destination $runtimeProjectLicenseDir -Force
    }
}

if ($null -ne $openCvInstallPath) {
    $openCvLicenseDir = Join-Path $openCvInstallPath "etc\licenses"
    if (Test-Path -LiteralPath $openCvLicenseDir) {
        Get-ChildItem -File -LiteralPath $openCvLicenseDir | ForEach-Object {
            Copy-Item -LiteralPath $_.FullName -Destination $runtimeProjectOpenCvLicenseDir -Force
        }
    }
}

$runtimeFileEntries = @($runtimeFiles | ForEach-Object {
        [pscustomobject]@{
            FileName = Split-Path $_ -Leaf
            SourcePath = [System.IO.Path]::GetFullPath($_)
        }
    })

$licenseEntries = @()
if (Test-Path -LiteralPath $runtimeProjectLicenseDir -PathType Container) {
    $licenseEntries = @(Get-ChildItem -LiteralPath $runtimeProjectLicenseDir -Recurse -File | ForEach-Object {
            [pscustomobject]@{
                PackagePath = ([System.IO.Path]::GetRelativePath($runtimeProjectRootFullPath, $_.FullName) -replace "\\", "/")
                SourcePath = $_.FullName
            }
        })
}

$provenance = [ordered]@{
    SchemaVersion = 1
    PackageId = $RuntimePackageId
    PackageVersion = $PackageVersion
    OpenCvVersion = $OpenCvVersion
    OpenCvRid = $OpenCvRid
    Rid = $Rid
    RuntimeProfile = $RuntimeProfile
    SyntheticRuntimeInputs = [bool]$SyntheticRuntimeInputs.IsPresent
    PrimaryNativeLoaderName = $primaryNativeLoaderFileName
    CompatibilityNativeLoaderName = $compatibilityNativeLoaderCopyFileName
    RequiredModules = @($OpenCvModules)
    OptionalModulesRequested = @($OptionalOpenCvModules)
    OptionalModulesStaged = @($optionalModulesStaged)
    RuntimeFiles = @($runtimeFileEntries)
    LicenseFiles = @($licenseEntries)
    InputRoots = [ordered]@{
        NativeWrapperRuntimeDir = $nativeRuntimePath.Path
        OpenCvRuntimeDir = $openCvRuntimePath.Path
        OpenCvSourceDir = $openCvSourcePath.Path
        OpenCvInstallDir = if ($null -ne $openCvInstallPath) { $openCvInstallPath.Path } else { "" }
    }
    OutputRoots = [ordered]@{
        StageOutputRoot = $outputRootFullPath
        RuntimeProjectRoot = $runtimeProjectRootFullPath
        RuntimeProjectNativeDir = $runtimeProjectNativeDir
        RuntimeProjectLicenseDir = $runtimeProjectLicenseDir
        PackageManifestPath = "build/JYPPX.OpenCV.runtime.provenance.json"
    }
}

$json = ($provenance | ConvertTo-Json -Depth 8) + [System.Environment]::NewLine
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($runtimeProvenanceManifestPath, $json, $utf8NoBom)

Write-Host "Runtime staging directory: $stagingNativeDir"
Write-Host "Runtime package project directory: $runtimeProjectNativeDir"
Write-Host "Runtime license directory: $runtimeProjectLicenseDir"
Write-Host "Runtime provenance manifest: $runtimeProvenanceManifestPath"
