param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactRoot,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$ExpectedPackageVersion = "",
    [string]$ExpectedSyntheticRuntimeInputs = "true",
    [string]$SelectedRid = "",
    [string]$SelectedRuntimeProfile = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$artifactRootFullPath = (Resolve-Path -LiteralPath $ArtifactRoot).Path
. (Join-Path $repo "scripts/PackageVersion.ps1")
$managedPackageId = "JYPPX.OpenCV.CSharp.API"
$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$runtimeMatrixPath = "packaging/runtime/runtime-package-matrix.json"
$directoryBuildPropsPath = "Directory.Build.props"
$runtimeProvenanceManifestEntry = "build/JYPPX.OpenCV.runtime.provenance.json"

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

function Read-RequiredText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required file was not found: $RelativePath"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Get-DirectoryBuildPropertyMap {
    [xml]$props = Read-RequiredText -RelativePath $directoryBuildPropsPath
    $propertyMap = @{}
    foreach ($propertyGroup in @($props.Project.PropertyGroup)) {
        foreach ($child in @($propertyGroup.ChildNodes)) {
            if ($child.NodeType -eq [System.Xml.XmlNodeType]::Element) {
                $propertyMap[$child.Name] = $child.InnerText.Trim()
            }
        }
    }

    return $propertyMap
}

function Resolve-DirectoryBuildProperty {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$PropertyMap,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if (-not $PropertyMap.ContainsKey($Name)) {
        throw "Directory.Build.props property was not found: $Name"
    }

    $value = [string]$PropertyMap[$Name]
    for ($i = 0; $i -lt 10; $i++) {
        $replaced = [System.Text.RegularExpressions.Regex]::Replace(
            $value,
            "\$\(([A-Za-z0-9_.-]+)\)",
            {
                param($match)
                $propertyName = $match.Groups[1].Value
                if (-not $PropertyMap.ContainsKey($propertyName)) {
                    throw "Directory.Build.props property '$Name' references missing property '$propertyName'."
                }

                return [string]$PropertyMap[$propertyName]
            })

        if ($replaced -eq $value) {
            return $value
        }

        $value = $replaced
    }

    throw "Directory.Build.props property '$Name' could not be resolved after recursive expansion."
}

function Get-NormalizedPackageFileVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$VersionText
    )

    return (ConvertTo-OpenCvCSharpPackageVersion -Version $VersionText).NuGetVersion
}

function Get-EntryFileName {
    param(
        [Parameter(Mandatory = $true)]
        [string]$EntryName
    )

    $normalized = $EntryName.Replace("\", "/")
    return ($normalized -split "/")[-1]
}

function Read-NupkgInfo {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entries = @($zip.Entries | ForEach-Object { $_.FullName })
        $nuspecEntry = @($zip.Entries | Where-Object { $_.FullName -like "*.nuspec" } | Select-Object -First 1)
        if ($nuspecEntry.Count -eq 0) {
            throw "No nuspec entry was found in package: $Path"
        }

        $stream = $nuspecEntry[0].Open()
        try {
            $reader = [System.IO.StreamReader]::new($stream)
            try {
                [xml]$nuspec = $reader.ReadToEnd()
            }
            finally {
                $reader.Dispose()
            }
        }
        finally {
            $stream.Dispose()
        }

        $idNode = $nuspec.SelectSingleNode('/*[local-name()="package"]/*[local-name()="metadata"]/*[local-name()="id"]')
        $versionNode = $nuspec.SelectSingleNode('/*[local-name()="package"]/*[local-name()="metadata"]/*[local-name()="version"]')

        return [pscustomobject]@{
            Path = $Path
            FileName = Split-Path -Leaf $Path
            Id = if ($null -ne $idNode) { $idNode.InnerText } else { "" }
            Version = if ($null -ne $versionNode) { $versionNode.InnerText } else { "" }
            Entries = $entries
        }
    }
    finally {
        $zip.Dispose()
    }
}

function Read-NupkgJsonEntry {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$EntryName,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations
    )

    $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entry = $zip.GetEntry($EntryName)
        if ($null -eq $entry) {
            Add-Violation -Violations $Violations -Path (Split-Path -Leaf $Path) -Issue "Runtime package must include provenance manifest" -Text $EntryName
            return $null
        }

        $stream = $entry.Open()
        try {
            $reader = [System.IO.StreamReader]::new($stream)
            try {
                $jsonText = $reader.ReadToEnd()
            }
            finally {
                $reader.Dispose()
            }
        }
        finally {
            $stream.Dispose()
        }

        try {
            return $jsonText | ConvertFrom-Json
        }
        catch {
            Add-Violation -Violations $Violations -Path (Split-Path -Leaf $Path) -Issue "Runtime provenance manifest must be valid JSON" -Text $_.Exception.Message
            return $null
        }
    }
    finally {
        $zip.Dispose()
    }
}

function Test-ContainsFixedMajorIdentity {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    return $Text -match "OpenCv5Sharp|opencv5sharp" # Detect fixed-major compatibility names outside allowed compatibility entries.
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

$propertyMap = Get-DirectoryBuildPropertyMap
if ([string]::IsNullOrWhiteSpace($ExpectedPackageVersion)) {
    $ExpectedPackageVersion = Resolve-DirectoryBuildProperty -PropertyMap $propertyMap -Name "OpenCvCSharpPackageVersion"
}

$normalizedPackageVersion = Get-NormalizedPackageFileVersion -VersionText $ExpectedPackageVersion
$expectedOpenCvVersion = (($ExpectedPackageVersion -split "\.") | Select-Object -First 3) -join "."
$expectedSyntheticRuntimeInputsValue = [bool]::Parse($ExpectedSyntheticRuntimeInputs)
$matrixText = Read-RequiredText -RelativePath $runtimeMatrixPath
$matrix = $matrixText | ConvertFrom-Json
$violations = [System.Collections.Generic.List[object]]::new()
$selectedMode = -not [string]::IsNullOrWhiteSpace($SelectedRid) -or -not [string]::IsNullOrWhiteSpace($SelectedRuntimeProfile)
if ($selectedMode -and ([string]::IsNullOrWhiteSpace($SelectedRid) -or [string]::IsNullOrWhiteSpace($SelectedRuntimeProfile))) {
    throw "SelectedRid and SelectedRuntimeProfile must be provided together."
}

$selectedRidSpecs = @($matrix.rids | Where-Object { $_.rid -eq $SelectedRid })
$selectedProfileSpecs = @($matrix.profiles | Where-Object { $_.name -eq $SelectedRuntimeProfile })
if ($selectedMode -and ($selectedRidSpecs.Count -ne 1 -or $selectedProfileSpecs.Count -ne 1)) {
    throw "Selected RID/profile was not found exactly once in the runtime matrix: $SelectedRid / $SelectedRuntimeProfile"
}

$artifactDirs = @(Get-ChildItem -LiteralPath $artifactRootFullPath -Directory | Sort-Object Name)
$artifactNames = @($artifactDirs | ForEach-Object { $_.Name })
$expectedArtifacts = @("nupkg-managed")

foreach ($ridSpec in @($matrix.rids)) {
    foreach ($profileSpec in @($matrix.profiles)) {
        if ($selectedMode -and ($ridSpec.rid -ne $SelectedRid -or $profileSpec.name -ne $SelectedRuntimeProfile)) {
            continue
        }

        $expectedArtifacts += "nupkg-$($ridSpec.rid)-$($profileSpec.name)"
    }
}

foreach ($expectedArtifact in $expectedArtifacts) {
    if ($artifactNames -notcontains $expectedArtifact) {
        Add-Violation -Violations $violations -Path $ArtifactRoot -Issue "Missing expected GitHub pack artifact directory" -Text $expectedArtifact
    }
}

foreach ($actualArtifact in $artifactNames) {
    if ($expectedArtifacts -notcontains $actualArtifact) {
        Add-Violation -Violations $violations -Path $actualArtifact -Issue "Unexpected GitHub pack artifact directory"
    }

    if (Test-ContainsFixedMajorIdentity -Text $actualArtifact) {
        Add-Violation -Violations $violations -Path $actualArtifact -Issue "Artifact directory names must not use fixed-major package identity"
    }
}

$managedInfo = $null
$managedDir = Join-Path $artifactRootFullPath "nupkg-managed"
if (Test-Path -LiteralPath $managedDir -PathType Container) {
    $managedPackages = @(Get-ChildItem -LiteralPath $managedDir -Filter "*.nupkg" -File)
    if ($managedPackages.Count -ne 1) {
        Add-Violation -Violations $violations -Path "nupkg-managed" -Issue "Managed artifact must contain exactly one .nupkg" -Text "Found $($managedPackages.Count)"
    }
    else {
        $managedInfo = Read-NupkgInfo -Path $managedPackages[0].FullName
        $expectedManagedFileName = "$managedPackageId.$normalizedPackageVersion.nupkg"
        if ($managedInfo.FileName -ne $expectedManagedFileName) {
            Add-Violation -Violations $violations -Path "nupkg-managed" -Issue "Managed package file name must match neutral package ID plus normalized version" -Text $managedInfo.FileName
        }

        if ($managedInfo.Id -ne $managedPackageId) {
            Add-Violation -Violations $violations -Path $managedInfo.FileName -Issue "Managed package nuspec ID must stay version-neutral" -Text $managedInfo.Id
        }

        if ($managedInfo.Version -ne $normalizedPackageVersion) {
            Add-Violation -Violations $violations -Path $managedInfo.FileName -Issue "Managed package version mismatch" -Text $managedInfo.Version
        }

        if ($managedInfo.Entries -notcontains "lib/net8.0/$managedPackageId.dll") {
            Add-Violation -Violations $violations -Path $managedInfo.FileName -Issue "Managed package must include the net8.0 compile asset"
        }

        foreach ($entry in @($managedInfo.Entries)) {
            if (Test-ContainsFixedMajorIdentity -Text $entry) {
                Add-Violation -Violations $violations -Path $managedInfo.FileName -Issue "Managed package entries must not use fixed-major identity" -Text $entry
            }
        }
    }
}

$runtimeResults = [System.Collections.Generic.List[object]]::new()

foreach ($ridSpec in @($matrix.rids)) {
    foreach ($profileSpec in @($matrix.profiles)) {
        $rid = [string]$ridSpec.rid
        $profile = [string]$profileSpec.name
        if ($selectedMode -and ($rid -ne $SelectedRid -or $profile -ne $SelectedRuntimeProfile)) {
            continue
        }

        $artifactName = "nupkg-$rid-$profile"
        $artifactDir = Join-Path $artifactRootFullPath $artifactName
        if (-not (Test-Path -LiteralPath $artifactDir -PathType Container)) {
            continue
        }

        $packages = @(Get-ChildItem -LiteralPath $artifactDir -Filter "*.nupkg" -File)
        if ($packages.Count -ne 1) {
            Add-Violation -Violations $violations -Path $artifactName -Issue "Runtime artifact must contain exactly one .nupkg" -Text "Found $($packages.Count)"
            continue
        }

        $info = Read-NupkgInfo -Path $packages[0].FullName
        $manifest = Read-NupkgJsonEntry -Path $info.Path -EntryName $runtimeProvenanceManifestEntry -Violations $violations
        $profileSuffix = if ($profile -eq "mini") { ".mini" } else { "" }
        $expectedId = "$runtimePackagePrefix.$rid$profileSuffix"
        $expectedFileName = "$expectedId.$normalizedPackageVersion.nupkg"
        $androidBuildTargetEntry = "buildTransitive/$expectedId.targets"
        $nativePrefix = "runtimes/$rid/native/"
        $nativeEntries = @($info.Entries | Where-Object {
                $_.StartsWith($nativePrefix, [System.StringComparison]::OrdinalIgnoreCase) -and -not $_.EndsWith("/")
            })
        $moduleEntries = @($nativeEntries | Where-Object {
                (Get-EntryFileName -EntryName $_) -match "^(opencv_|libopencv_)"
            })
        $expectedModuleCount = @($profileSpec.modules).Count
        $expectedRequiredModules = @($profileSpec.modules | ForEach-Object { [string]$_ })
        $expectedOptionalModules = @($profileSpec.optionalModules | ForEach-Object { [string]$_ })
        $manifestOptionalModulesStaged = @()
        if ($null -ne $manifest -and $null -ne $manifest.PSObject.Properties["OptionalModulesStaged"]) {
            $manifestOptionalModulesStaged = @($manifest.OptionalModulesStaged | ForEach-Object { [string]$_ })
        }

        $expectedStagedModules = @($expectedRequiredModules)
        if ($selectedMode) {
            $expectedStagedModules += $manifestOptionalModulesStaged
        }

        $expectedModuleFileCount = $expectedStagedModules.Count
        $expectedNativeFileNames = @()
        $primaryLoaderName = if ($rid.StartsWith("win-", [System.StringComparison]::OrdinalIgnoreCase)) { "JYPPX.OpenCV.Native.dll" } else { "libJYPPX.OpenCV.Native.so" }
        if ($selectedMode) {
            $expectedNativeFileNames = @($primaryLoaderName)
            if ($ridSpec.platformFamily -eq "linux" -and -not $expectedSyntheticRuntimeInputsValue) {
                $openCvBinarySuffix = ([System.Version]::Parse($expectedOpenCvVersion).Major.ToString() + [System.Version]::Parse($expectedOpenCvVersion).Minor.ToString() + [System.Version]::Parse($expectedOpenCvVersion).Build.ToString())
                foreach ($module in $expectedStagedModules) {
                    $expectedNativeFileNames += @(
                        "libopencv_$module.so",
                        "libopencv_$module.so.$openCvBinarySuffix",
                        "libopencv_$module.so.$expectedOpenCvVersion"
                    )
                }
                $expectedModuleFileCount = $expectedStagedModules.Count * 3
            }
            elseif ($ridSpec.platformFamily -eq "windows") {
                $openCvBinarySuffix = ([System.Version]::Parse($expectedOpenCvVersion).Major.ToString() + [System.Version]::Parse($expectedOpenCvVersion).Minor.ToString() + [System.Version]::Parse($expectedOpenCvVersion).Build.ToString())
                foreach ($module in $expectedStagedModules) {
                    $expectedNativeFileNames += "opencv_$module$openCvBinarySuffix.dll"
                }
            }
            elseif ($ridSpec.platformFamily -eq "android") {
                foreach ($module in $expectedStagedModules) {
                    $expectedNativeFileNames += "libopencv_$module.so"
                }
            }
            else {
                foreach ($module in $expectedStagedModules) {
                    $expectedNativeFileNames += "libopencv_$module.so.$expectedOpenCvVersion"
                }
            }
        }
        $hasPrimaryLoader = @($nativeEntries | Where-Object { (Get-EntryFileName -EntryName $_) -eq $primaryLoaderName }).Count -gt 0

        if ($info.FileName -ne $expectedFileName) {
            Add-Violation -Violations $violations -Path $artifactName -Issue "Runtime package file name must match neutral package ID plus normalized version" -Text $info.FileName
        }

        if ($info.Id -ne $expectedId) {
            Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime package nuspec ID must stay RID/profile-derived and version-neutral" -Text $info.Id
        }

        if ($info.Version -ne $normalizedPackageVersion) {
            Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime package version mismatch" -Text $info.Version
        }

        if ($ridSpec.platformFamily -eq "android") {
            if ($info.Entries -notcontains $androidBuildTargetEntry) {
                Add-Violation -Violations $violations -Path $info.FileName -Issue "Android runtime package must include the auto-imported buildTransitive target" -Text $androidBuildTargetEntry
            }
        }
        elseif ($info.Entries -contains $androidBuildTargetEntry) {
            Add-Violation -Violations $violations -Path $info.FileName -Issue "Desktop runtime package must not include Android buildTransitive integration" -Text $androidBuildTargetEntry
        }

        if ($nativeEntries.Count -eq 0) {
            Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime package must include selected RID native payload path" -Text $nativePrefix
        }

        if ($moduleEntries.Count -ne $expectedModuleFileCount) {
            Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime package OpenCV module file count must match selected runtime profile and provenance mode" -Text "Found $($moduleEntries.Count), expected $expectedModuleFileCount"
        }

        if ($selectedMode) {
            $nativeFileNames = @($nativeEntries | ForEach-Object { Get-EntryFileName -EntryName $_ } | Sort-Object)
            $expectedNativeFileNames = @($expectedNativeFileNames | Sort-Object)
            if (($nativeFileNames -join "|") -ne ($expectedNativeFileNames -join "|")) {
                Add-Violation -Violations $violations -Path $info.FileName -Issue "Targeted runtime package native payload must exactly match the selected RID/profile contract" -Text "Found $($nativeFileNames -join ','), expected $($expectedNativeFileNames -join ',')"
            }
        }

        if (-not $hasPrimaryLoader) {
            Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime package must include the neutral primary native loader" -Text $primaryLoaderName
        }

        if ($null -ne $manifest) {
            if ($manifest.PackageId -ne $expectedId -or $manifest.PackageVersion -ne $ExpectedPackageVersion) {
                Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime provenance manifest must record package identity and four-part version metadata" -Text "$($manifest.PackageId) / $($manifest.PackageVersion)"
            }

            if ($manifest.OpenCvVersion -ne $expectedOpenCvVersion) {
                Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime provenance manifest must record OpenCV runtime version" -Text $manifest.OpenCvVersion
            }

            if ($manifest.Rid -ne $rid -or $manifest.RuntimeProfile -ne $profile) {
                Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime provenance manifest must record selected RID/profile" -Text "$($manifest.Rid) / $($manifest.RuntimeProfile)"
            }

            if ([bool]$manifest.SyntheticRuntimeInputs -ne $expectedSyntheticRuntimeInputsValue) {
                Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime provenance manifest must distinguish synthetic validation inputs from real runtime inputs" -Text $manifest.SyntheticRuntimeInputs
            }

            if ($manifest.PrimaryNativeLoaderName -ne $primaryLoaderName) {
                Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime provenance manifest must record the version-neutral native loader name" -Text $manifest.PrimaryNativeLoaderName
            }

            $manifestRequiredModules = @($manifest.RequiredModules | ForEach-Object { [string]$_ })
            if ($manifestRequiredModules.Count -ne $expectedModuleCount -or (($manifestRequiredModules -join ",") -ne ($expectedRequiredModules -join ","))) {
                Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime provenance manifest required modules must match selected runtime profile" -Text "Found $($manifestRequiredModules -join ','), expected $($expectedRequiredModules -join ',')"
            }

            $manifestOptionalModulesRequested = @($manifest.OptionalModulesRequested | ForEach-Object { [string]$_ })
            if ($manifestOptionalModulesRequested.Count -ne $expectedOptionalModules.Count -or (($manifestOptionalModulesRequested -join ",") -ne ($expectedOptionalModules -join ","))) {
                Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime provenance manifest optional modules must match selected runtime profile" -Text "Found $($manifestOptionalModulesRequested -join ','), expected $($expectedOptionalModules -join ',')"
            }

            $expectedOptionalModulesStaged = @($expectedOptionalModules | Where-Object { $manifestOptionalModulesStaged -contains $_ })
            if ($manifestOptionalModulesStaged.Count -ne $expectedOptionalModulesStaged.Count -or (($manifestOptionalModulesStaged -join ",") -ne ($expectedOptionalModulesStaged -join ","))) {
                Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime provenance staged optional modules must be an ordered unique subset of the selected runtime profile" -Text "Found $($manifestOptionalModulesStaged -join ','), allowed $($expectedOptionalModules -join ',')"
            }

            $manifestRuntimeFiles = @($manifest.RuntimeFiles)
            if ($manifestRuntimeFiles.Count -lt ($expectedStagedModules.Count + 1)) {
                Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime provenance manifest must list staged native loader and OpenCV runtime files" -Text "Found $($manifestRuntimeFiles.Count), expected at least $($expectedStagedModules.Count + 2)"
            }

            if ($selectedMode) {
                $manifestRuntimeFileNames = @($manifestRuntimeFiles | ForEach-Object { [string]$_.FileName } | Sort-Object)
                if (($manifestRuntimeFileNames -join "|") -ne ($expectedNativeFileNames -join "|")) {
                    Add-Violation -Violations $violations -Path $info.FileName -Issue "Targeted runtime provenance files must exactly match the packaged native payload" -Text "Found $($manifestRuntimeFileNames -join ','), expected $($expectedNativeFileNames -join ',')"
                }

                if ($profile -eq "mini" -and $manifestOptionalModulesStaged.Count -ne 0) {
                    Add-Violation -Violations $violations -Path $info.FileName -Issue "Targeted mini runtime provenance must not stage optional/full-only modules" -Text ($manifestOptionalModulesStaged -join ',')
                }
            }
        }

        foreach ($entry in @($info.Entries)) {
            if (Test-ContainsFixedMajorIdentity -Text $entry) {
                Add-Violation -Violations $violations -Path $info.FileName -Issue "Runtime package must not contain fixed-major entries" -Text $entry
            }
        }

        $runtimeResults.Add([pscustomobject]@{
            Artifact = $artifactName
            Package = $info.FileName
            Id = $info.Id
            Rid = $rid
            Profile = $profile
            NativePayloadFiles = $nativeEntries.Count
            ModuleCount = $moduleEntries.Count
            ExpectedModuleCount = $expectedModuleFileCount
            HasPrimaryLoader = $hasPrimaryLoader
            HasCompatibilityLoader = $hasCompatibilityLoader
            HasProvenanceManifest = $null -ne $manifest
        })
    }
}

if ($violations.Count -gt 0) {
    Write-Host "GitHub pack artifact matrix surface guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        ForEach-Object {
            $text = if ([string]::IsNullOrWhiteSpace($_.Text)) { "" } else { " :: $($_.Text)" }
            Write-Host "$($_.Path) :: $($_.Issue)$text"
        }
    exit 1
}

$runtimeByProfile = $runtimeResults |
    Group-Object Profile |
    Sort-Object Name |
    ForEach-Object {
        [pscustomobject]@{
            Profile = $_.Name
            Count = $_.Count
            ModuleCounts = (@($_.Group | Select-Object -ExpandProperty ModuleCount | Sort-Object -Unique) -join ",")
        }
    }

Write-Host "GitHub pack artifact matrix surface guard passed."
Write-Host "Artifact directories checked: $($artifactDirs.Count)."
Write-Host "Managed package artifact: $($managedInfo.FileName)."
Write-Host "Runtime packages checked: $($runtimeResults.Count)."
if ($selectedMode) {
    Write-Host "Selected runtime package: $SelectedRid / $SelectedRuntimeProfile."
}
foreach ($profileSummary in @($runtimeByProfile)) {
    Write-Host "$($profileSummary.Profile) runtime packages: $($profileSummary.Count), module counts: $($profileSummary.ModuleCounts)."
}
