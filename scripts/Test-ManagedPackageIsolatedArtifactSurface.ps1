param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
if ($null -eq $pwsh) {
    throw "pwsh was not found. Managed package isolated artifact validation requires PowerShell 7+."
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnet) {
    throw "dotnet was not found. Managed package isolated artifact validation requires dotnet pack."
}

$packManagedPath = Join-Path $repo "scripts/Pack-Managed.ps1"
if (-not (Test-Path -LiteralPath $packManagedPath -PathType Leaf)) {
    throw "Required managed pack script was not found: $packManagedPath"
}

$managedPackageId = "JYPPX.OpenCV.CSharp.API"
$managedAssemblyName = "JYPPX.OpenCV.CSharp.API"
$managedAssemblyFileName = "$managedAssemblyName.dll"
$packageVersion = "5.0.0.0"
$normalizedPackageVersion = "5.0.0"
$targetFramework = "net8.0"
$expectedPackageFileName = "$managedPackageId.$normalizedPackageVersion.nupkg"
$expectedNuspecEntryName = "$managedPackageId.nuspec"
$expectedAssemblyEntryName = "lib/$targetFramework/$managedAssemblyFileName"
$fixedMajorManagedIdentity = "Open" + "Cv5Sharp"
$fixedMajorPackageIdentityRegex = [System.Text.RegularExpressions.Regex]::new(
    "<id>\s*$fixedMajorManagedIdentity\b|<PackageId>\s*$fixedMajorManagedIdentity\b|<AssemblyName>\s*$fixedMajorManagedIdentity\b|$fixedMajorManagedIdentity\.runtime|opencv" + "5sharp\.runtime",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

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

function Test-IsPathUnder {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Root
    )

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $fullRoot = [System.IO.Path]::GetFullPath($Root).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $rootPrefix = $fullRoot + [System.IO.Path]::DirectorySeparatorChar

    return (
        $fullPath.Equals($fullRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
        $fullPath.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase))
}

function Remove-DirectoryIfSafe {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$AllowedRoot
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        return
    }

    if (-not (Test-IsPathUnder -Path $Path -Root $AllowedRoot)) {
        throw "Refusing to remove path outside allowed root. Path: $Path; allowed root: $AllowedRoot"
    }

    Remove-Item -LiteralPath $Path -Recurse -Force
}

function Get-DirectoryFileCount {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        return 0
    }

    return @(Get-ChildItem -LiteralPath $Path -Recurse -File -Force).Count
}

function Assert-FileExists {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue
    }
}

function Read-ZipEntryText {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.Compression.ZipArchiveEntry]$Entry
    )

    $stream = $Entry.Open()
    try {
        $reader = [System.IO.StreamReader]::new($stream, [System.Text.Encoding]::UTF8, $true)
        try {
            return $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }
}

function Copy-ZipEntryToFile {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.Compression.ZipArchiveEntry]$Entry,
        [Parameter(Mandatory = $true)]
        [string]$DestinationPath
    )

    $destinationDirectory = Split-Path -Parent $DestinationPath
    New-Item -ItemType Directory -Force -Path $destinationDirectory | Out-Null

    $entryStream = $Entry.Open()
    try {
        $fileStream = [System.IO.File]::Create($DestinationPath)
        try {
            $entryStream.CopyTo($fileStream)
        }
        finally {
            $fileStream.Dispose()
        }
    }
    finally {
        $entryStream.Dispose()
    }
}

function Get-NuspecMetadataValue {
    param(
        [Parameter(Mandatory = $true)]
        [xml]$Nuspec,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $node = $Nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='$Name']")
    if ($null -eq $node) {
        return ""
    }

    return $node.InnerText.Trim()
}

$violations = [System.Collections.Generic.List[object]]::new()
$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-managed-package-artifact-" + [System.Guid]::NewGuid().ToString("N"))
$packageOutputDir = Join-Path $temporaryRoot "packages"
$managedBuildOutputRoot = Join-Path $temporaryRoot "managed-build"
$restorePackagesDir = Join-Path $temporaryRoot "nuget-packages"
$unexpectedNuGetPackagesDir = Join-Path $temporaryRoot "unexpected-nuget-packages"
$nugetHttpCacheDir = Join-Path $temporaryRoot "nuget-http-cache"
$nugetScratchDir = Join-Path $temporaryRoot "nuget-scratch"
$nugetPluginsCacheDir = Join-Path $temporaryRoot "nuget-plugin-cache"
$extractedAssemblyPath = Join-Path $temporaryRoot "extracted/$managedAssemblyFileName"

$repoSensitiveDirectories = @(
    (Join-Path $repo "src/OpenCvSharp/bin"),
    (Join-Path $repo "src/OpenCvSharp/obj"),
    (Join-Path $repo "artifacts/packages"),
    (Join-Path $repo "artifacts/runtime"),
    (Join-Path $repo "artifacts/staging"),
    (Join-Path $repo "packaging/runtime/JYPPX.OpenCV.runtime/runtimes"),
    (Join-Path $repo "packaging/runtime/JYPPX.OpenCV.runtime/licenses")
)

$preexistingSensitiveDirectories = @{}
foreach ($directory in $repoSensitiveDirectories) {
    $preexistingSensitiveDirectories[$directory] = Test-Path -LiteralPath $directory -PathType Container
}

$oldNuGetPackages = $env:NUGET_PACKAGES
$oldNuGetHttpCache = $env:NUGET_HTTP_CACHE_PATH
$oldNuGetScratch = $env:NUGET_SCRATCH
$oldNuGetPluginsCache = $env:NUGET_PLUGINS_CACHE_PATH

try {
    foreach ($directory in @(
            $packageOutputDir,
            $managedBuildOutputRoot,
            $restorePackagesDir,
            $unexpectedNuGetPackagesDir,
            $nugetHttpCacheDir,
            $nugetScratchDir,
            $nugetPluginsCacheDir)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $env:NUGET_PACKAGES = $unexpectedNuGetPackagesDir
    $env:NUGET_HTTP_CACHE_PATH = $nugetHttpCacheDir
    $env:NUGET_SCRATCH = $nugetScratchDir
    $env:NUGET_PLUGINS_CACHE_PATH = $nugetPluginsCacheDir

    $managedPackArguments = @(
        "-NoProfile",
        "-File", $packManagedPath,
        "-Configuration", "Release",
        "-OpenCvVersion", "5.0.0",
        "-PackageRevision", "0",
        "-OutputDir", $packageOutputDir,
        "-TargetFrameworks", $targetFramework,
        "-BuildOutputRoot", $managedBuildOutputRoot,
        "-RestorePackagesPath", $restorePackagesDir
    )

    $managedPackOutput = & $pwsh.Source @managedPackArguments 2>&1
    $managedPackOutputText = ($managedPackOutput | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine
    if ($LASTEXITCODE -ne 0) {
        Add-Violation -Violations $violations -Path "scripts/Pack-Managed.ps1" -Issue "Managed package generation failed" -Text $managedPackOutputText
    }

    $packagePath = Join-Path $packageOutputDir $expectedPackageFileName
    Assert-FileExists -Violations $violations -Path $packagePath -Issue "Managed package was not created with the normalized neutral package file name"

    if (Test-Path -LiteralPath $packagePath -PathType Leaf) {
        $archive = $null
        try {
            $archive = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
            $entriesByName = @{}
            foreach ($entry in $archive.Entries) {
                $entriesByName[$entry.FullName] = $entry
            }

            foreach ($entryName in @($expectedNuspecEntryName, "README.md", $expectedAssemblyEntryName)) {
                if (-not $entriesByName.ContainsKey($entryName)) {
                    Add-Violation -Violations $violations -Path $packagePath -Issue "Managed package is missing required entry" -Text $entryName
                }
            }

            $managedLibEntries = @(
                $archive.Entries |
                    Where-Object {
                        $_.FullName.StartsWith("lib/", [System.StringComparison]::OrdinalIgnoreCase) -and
                        $_.FullName.EndsWith(".dll", [System.StringComparison]::OrdinalIgnoreCase)
                    } |
                    ForEach-Object { $_.FullName }
            )
            if ($managedLibEntries.Count -ne 1 -or $managedLibEntries[0] -ne $expectedAssemblyEntryName) {
                Add-Violation `
                    -Violations $violations `
                    -Path $packagePath `
                    -Issue "Managed package must contain exactly the isolated net8.0 managed assembly under lib" `
                    -Text ($managedLibEntries -join "; ")
            }

            if ($entriesByName.ContainsKey($expectedNuspecEntryName)) {
                $nuspecText = Read-ZipEntryText -Entry $entriesByName[$expectedNuspecEntryName]
                [xml]$nuspecXml = $nuspecText
                $nuspecPackageId = Get-NuspecMetadataValue -Nuspec $nuspecXml -Name "id"
                $nuspecVersion = Get-NuspecMetadataValue -Nuspec $nuspecXml -Name "version"

                if ($nuspecPackageId -ne $managedPackageId) {
                    Add-Violation -Violations $violations -Path $expectedNuspecEntryName -Issue "Nuspec package ID must stay version-neutral" -Text $nuspecPackageId
                }

                if ($nuspecVersion -ne $normalizedPackageVersion) {
                    Add-Violation -Violations $violations -Path $expectedNuspecEntryName -Issue "Nuspec package version must be normalized from four-part package metadata" -Text $nuspecVersion
                }

                if ($fixedMajorPackageIdentityRegex.IsMatch($nuspecText)) {
                    Add-Violation -Violations $violations -Path $expectedNuspecEntryName -Issue "Nuspec metadata must not contain fixed-major package ID or assembly identity" -Text $fixedMajorPackageIdentityRegex.Match($nuspecText).Value
                }
            }

            if ($entriesByName.ContainsKey($expectedAssemblyEntryName)) {
                Copy-ZipEntryToFile -Entry $entriesByName[$expectedAssemblyEntryName] -DestinationPath $extractedAssemblyPath
                $assemblyName = [System.Reflection.AssemblyName]::GetAssemblyName($extractedAssemblyPath).Name
                if ($assemblyName -ne $managedAssemblyName) {
                    Add-Violation -Violations $violations -Path $expectedAssemblyEntryName -Issue "Packaged managed assembly name must stay version-neutral" -Text $assemblyName
                }
            }
        }
        catch {
            Add-Violation -Violations $violations -Path $packagePath -Issue "Managed package artifact could not be inspected" -Text $_.Exception.Message
        }
        finally {
            if ($null -ne $archive) {
                $archive.Dispose()
            }
        }
    }

    $projectAssetsPath = Join-Path $managedBuildOutputRoot "obj/project.assets.json"
    Assert-FileExists -Violations $violations -Path $projectAssetsPath -Issue "Pack-Managed did not write restore assets under the explicit temporary build output root"
    if (Test-Path -LiteralPath $projectAssetsPath -PathType Leaf) {
        $assetsText = [System.IO.File]::ReadAllText($projectAssetsPath)
        $assetsTextNormalized = $assetsText.Replace("\\", "/").Replace("\", "/")
        $restorePackagesDirNormalized = [System.IO.Path]::GetFullPath($restorePackagesDir).Replace("\", "/")
        if ($assetsTextNormalized.IndexOf($restorePackagesDirNormalized, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            Add-Violation -Violations $violations -Path $projectAssetsPath -Issue "Pack-Managed did not restore through the explicit temporary RestorePackagesPath" -Text $restorePackagesDirNormalized
        }
    }

    if ((Get-DirectoryFileCount -Path $unexpectedNuGetPackagesDir) -gt 0) {
        Add-Violation -Violations $violations -Path $unexpectedNuGetPackagesDir -Issue "Pack-Managed restore output escaped the explicit RestorePackagesPath"
    }

    foreach ($outputDirectory in @(
            $packageOutputDir,
            $managedBuildOutputRoot,
            $restorePackagesDir,
            $nugetHttpCacheDir,
            $nugetScratchDir,
            $nugetPluginsCacheDir)) {
        if ((Test-Path -LiteralPath $outputDirectory -PathType Container) -and
            -not (Test-IsPathUnder -Path $outputDirectory -Root $temporaryRoot)) {
            Add-Violation -Violations $violations -Path $outputDirectory -Issue "Managed package dry-run output escaped the temporary root"
        }
    }
}
finally {
    $env:NUGET_PACKAGES = $oldNuGetPackages
    $env:NUGET_HTTP_CACHE_PATH = $oldNuGetHttpCache
    $env:NUGET_SCRATCH = $oldNuGetScratch
    $env:NUGET_PLUGINS_CACHE_PATH = $oldNuGetPluginsCache

    foreach ($directory in $repoSensitiveDirectories) {
        $existsAfter = Test-Path -LiteralPath $directory -PathType Container
        if (-not $preexistingSensitiveDirectories[$directory] -and $existsAfter) {
            Add-Violation -Violations $violations -Path $directory -Issue "Managed package dry-run unexpectedly created a repository output directory"
            Remove-DirectoryIfSafe -Path $directory -AllowedRoot $repo
        }
    }

    Remove-DirectoryIfSafe -Path $temporaryRoot -AllowedRoot ([System.IO.Path]::GetTempPath())
}

if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
    Add-Violation -Violations $violations -Path $temporaryRoot -Issue "Temporary managed package artifact output was not cleaned"
}

foreach ($directory in $repoSensitiveDirectories) {
    if (-not $preexistingSensitiveDirectories[$directory] -and (Test-Path -LiteralPath $directory -PathType Container)) {
        Add-Violation -Violations $violations -Path $directory -Issue "Repository output directory remains after managed package dry-run cleanup"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Managed package isolated artifact surface guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Managed package isolated artifact surface guard passed."
Write-Host "Package artifact checked: $expectedPackageFileName."
Write-Host "Nuspec, root README, and metadata-only assembly name verified for $targetFramework."
