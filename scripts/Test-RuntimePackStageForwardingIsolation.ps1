param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
if ($null -eq $pwsh) {
    throw "pwsh was not found. Runtime pack stage-forwarding isolation validation requires PowerShell 7+."
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnet) {
    throw "dotnet was not found. Runtime pack stage-forwarding isolation validation requires dotnet pack."
}

$packRuntimePath = Join-Path $repo "scripts/Pack-Runtime.ps1"
if (-not (Test-Path -LiteralPath $packRuntimePath -PathType Leaf)) {
    throw "Pack-Runtime.ps1 was not found: $packRuntimePath"
}

$primaryNativeLoader = "JYPPX.OpenCV.Native.dll"
$compatibilityNativeLoader = "OpenCv5Sharp.Native.dll"
$runtimePackageId = "JYPPX.OpenCV.runtime.win-x64"
$packageFileName = "$runtimePackageId.5.0.0.nupkg"
$requiredOpenCvModules = @(
    "core",
    "imgcodecs",
    "imgproc",
    "videoio",
    "flann",
    "geometry",
    "calib",
    "stereo",
    "dnn",
    "objdetect",
    "photo",
    "features",
    "video",
    "highgui",
    "stitching",
    "ptcloud"
)

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

function Write-SyntheticDll {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $directory = Split-Path -Parent $Path
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    [System.IO.File]::WriteAllBytes($Path, [byte[]](0x4D, 0x5A, 0x00, 0x00))
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

function Assert-ZipEntry {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [System.Collections.Generic.HashSet[string]]$Entries,
        [Parameter(Mandatory = $true)]
        [string]$Entry,
        [Parameter(Mandatory = $true)]
        [string]$PackagePath,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    if (-not $Entries.Contains($Entry)) {
        Add-Violation -Violations $Violations -Path $PackagePath -Issue $Issue -Text $Entry
    }
}

function New-TemporaryRuntimeProject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RuntimeProjectDirectory
    )

    New-Item -ItemType Directory -Force -Path $RuntimeProjectDirectory | Out-Null
    $projectPath = Join-Path $RuntimeProjectDirectory "JYPPX.OpenCV.runtime.csproj"
    $projectText = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RuntimePackageRid Condition="'$(RuntimePackageRid)' == ''">win-x64</RuntimePackageRid>
    <PackageId>JYPPX.OpenCV.runtime.$(RuntimePackageRid)</PackageId>
    <Version>5.0.0.0</Version>
    <PackageVersion>5.0.0.0</PackageVersion>
    <Authors>synthetic</Authors>
    <Description>Synthetic native runtime package for pack-stage forwarding validation.</Description>
    <PackageLicenseExpression>MIT AND Apache-2.0</PackageLicenseExpression>
    <IsPackable>true</IsPackable>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <NoWarn>$(NoWarn);NU5128</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
    <None Include="runtimes\$(RuntimePackageRid)\native\**\*" Pack="true" PackagePath="runtimes\$(RuntimePackageRid)\native" />
    <None Include="licenses\**\*" Pack="true" PackagePath="licenses" />
  </ItemGroup>
</Project>
'@
    [System.IO.File]::WriteAllText($projectPath, $projectText)
    [System.IO.File]::WriteAllText((Join-Path $RuntimeProjectDirectory "README.md"), "Synthetic runtime package README")

    return $projectPath
}

$violations = [System.Collections.Generic.List[object]]::new()
$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-runtime-pack-stage-dry-run-" + [System.Guid]::NewGuid().ToString("N"))
$nativeWrapperRuntimeDir = Join-Path $temporaryRoot "native-wrapper-runtime"
$openCvRuntimeDir = Join-Path $temporaryRoot "opencv-runtime"
$openCvSourceDir = Join-Path $temporaryRoot "opencv-source"
$openCvInstallDir = Join-Path $temporaryRoot "opencv-install"
$stageOutputRoot = Join-Path $temporaryRoot "stage-output"
$packageOutputDir = Join-Path $temporaryRoot "package-output"
$runtimeProjectDir = Join-Path $temporaryRoot "runtime-package-project"
$runtimeProjectPath = Join-Path $runtimeProjectDir "JYPPX.OpenCV.runtime.csproj"
$rid = "win-x64"
$openCvBinarySuffix = "500"

$repoRuntimeOutputRoot = Join-Path $repo "artifacts/runtime"
$repoPackageOutputRoot = Join-Path $repo "artifacts/packages"
$repoRuntimeProjectRoot = Join-Path $repo "packaging/runtime/JYPPX.OpenCV.runtime"
$repoSensitiveDirectories = @(
    $repoRuntimeOutputRoot,
    $repoPackageOutputRoot,
    (Join-Path $repoRuntimeProjectRoot "runtimes"),
    (Join-Path $repoRuntimeProjectRoot "licenses")
)

$preexistingSensitiveDirectories = @{}
foreach ($directory in $repoSensitiveDirectories) {
    $preexistingSensitiveDirectories[$directory] = Test-Path -LiteralPath $directory -PathType Container
}

try {
    foreach ($directory in @(
            $nativeWrapperRuntimeDir,
            $openCvRuntimeDir,
            (Join-Path $openCvSourceDir "3rdparty/ippicv"),
            (Join-Path $openCvInstallDir "etc/licenses"),
            $stageOutputRoot,
            $packageOutputDir)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $runtimeProjectPath = New-TemporaryRuntimeProject -RuntimeProjectDirectory $runtimeProjectDir

    foreach ($dllName in @($primaryNativeLoader, $compatibilityNativeLoader)) {
        Write-SyntheticDll -Path (Join-Path $nativeWrapperRuntimeDir $dllName)
    }

    foreach ($module in $requiredOpenCvModules) {
        Write-SyntheticDll -Path (Join-Path $openCvRuntimeDir "opencv_$module$openCvBinarySuffix.dll")
    }

    [System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "LICENSE"), "Synthetic OpenCV source license")
    [System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "3rdparty/ippicv/readme.htm"), "Synthetic IPPICV license")
    [System.IO.File]::WriteAllText((Join-Path $openCvInstallDir "etc/licenses/synthetic-3rdparty.txt"), "Synthetic third-party license")

    $packArguments = @(
        "-NoProfile",
        "-File", $packRuntimePath,
        "-Rid", $rid,
        "-Configuration", "Release",
        "-OpenCvVersion", "5.0.0",
        "-PackageRevision", "0",
        "-StageRuntime",
        "-OpenCvNativeRuntimeDir", $nativeWrapperRuntimeDir,
        "-OpenCvRuntimeDir", $openCvRuntimeDir,
        "-OpenCvSourceDir", $openCvSourceDir,
        "-OpenCvInstallDir", $openCvInstallDir,
        "-StageOutputRoot", $stageOutputRoot,
        "-OutputDir", $packageOutputDir,
        "-RuntimeProject", $runtimeProjectPath
    )

    $packOutput = & $pwsh.Source @packArguments 2>&1
    $packOutputText = ($packOutput | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine
    if ($LASTEXITCODE -ne 0) {
        Add-Violation -Violations $violations -Path "scripts/Pack-Runtime.ps1" -Issue "Pack-Runtime synthetic stage-forwarding dry-run failed" -Text $packOutputText
    }

    $expectedRuntimeFiles = @($primaryNativeLoader, $compatibilityNativeLoader)
    foreach ($module in $requiredOpenCvModules) {
        $expectedRuntimeFiles += "opencv_$module$openCvBinarySuffix.dll"
    }

    $stagingNativeDir = Join-Path $stageOutputRoot (Join-Path $rid "native")
    $runtimeProjectNativeDir = Join-Path $runtimeProjectDir (Join-Path "runtimes/$rid" "native")
    foreach ($runtimeFile in $expectedRuntimeFiles) {
        Assert-FileExists -Violations $violations -Path (Join-Path $stagingNativeDir $runtimeFile) -Issue "Pack-Runtime did not forward StageOutputRoot to Stage-Runtime"
        Assert-FileExists -Violations $violations -Path (Join-Path $runtimeProjectNativeDir $runtimeFile) -Issue "Pack-Runtime did not forward the selected runtime project directory to Stage-Runtime"
    }

    $runtimeProjectLicenseDir = Join-Path $runtimeProjectDir "licenses"
    Assert-FileExists -Violations $violations -Path (Join-Path $runtimeProjectLicenseDir "LICENSE") -Issue "Pack-Runtime stage forwarding did not populate runtime project license layout"
    Assert-FileExists -Violations $violations -Path (Join-Path $runtimeProjectLicenseDir "readme.htm") -Issue "Pack-Runtime stage forwarding did not populate OpenCV 3rdparty readme.htm"
    Assert-FileExists -Violations $violations -Path (Join-Path $runtimeProjectLicenseDir "opencv-3rdparty/synthetic-3rdparty.txt") -Issue "Pack-Runtime stage forwarding did not populate OpenCV third-party license layout"

    $packagePath = Join-Path $packageOutputDir $packageFileName
    Assert-FileExists -Violations $violations -Path $packagePath -Issue "Runtime package artifact was not produced in the temporary package output directory"

    if (Test-Path -LiteralPath $packagePath -PathType Leaf) {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
        try {
            $entryNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
            foreach ($entry in $zip.Entries) {
                [void]$entryNames.Add($entry.FullName)
            }

            Assert-ZipEntry -Violations $violations -Entries $entryNames -Entry "$runtimePackageId.nuspec" -PackagePath $packagePath -Issue "Runtime package did not contain the expected neutral nuspec"
            foreach ($runtimeFile in $expectedRuntimeFiles) {
                Assert-ZipEntry -Violations $violations -Entries $entryNames -Entry "runtimes/$rid/native/$runtimeFile" -PackagePath $packagePath -Issue "Runtime package did not contain staged native DLL"
            }

            foreach ($licenseEntry in @(
                    "licenses/LICENSE",
                    "licenses/readme.htm",
                    "licenses/opencv-3rdparty/synthetic-3rdparty.txt")) {
                Assert-ZipEntry -Violations $violations -Entries $entryNames -Entry $licenseEntry -PackagePath $packagePath -Issue "Runtime package did not contain staged license file"
            }

            $nuspecEntry = $zip.GetEntry("$runtimePackageId.nuspec")
            if ($null -ne $nuspecEntry) {
                $reader = [System.IO.StreamReader]::new($nuspecEntry.Open())
                try {
                    $nuspecText = $reader.ReadToEnd()
                }
                finally {
                    $reader.Dispose()
                }

                if ($nuspecText.IndexOf("<id>$runtimePackageId</id>", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
                    Add-Violation -Violations $violations -Path $packagePath -Issue "Runtime package ID must remain JYPPX.OpenCV.runtime.<rid>" -Text $nuspecText
                }

                if ($nuspecText -match "OpenCv5Sharp\.runtime|opencv5sharp\.runtime") {
                    Add-Violation -Violations $violations -Path $packagePath -Issue "Runtime package nuspec must not contain a fixed-major runtime package identity" -Text $nuspecText
                }
            }
        }
        finally {
            $zip.Dispose()
        }
    }

    foreach ($packOutputDirectory in @(
            (Join-Path $runtimeProjectDir "bin"),
            (Join-Path $runtimeProjectDir "obj"))) {
        if ((Test-Path -LiteralPath $packOutputDirectory -PathType Container) -and
            -not (Test-IsPathUnder -Path $packOutputDirectory -Root $runtimeProjectDir)) {
            Add-Violation -Violations $violations -Path $packOutputDirectory -Issue "dotnet pack output was not contained by the temporary runtime package project"
        }
    }
}
finally {
    foreach ($directory in $repoSensitiveDirectories) {
        $existsAfter = Test-Path -LiteralPath $directory -PathType Container
        if (-not $preexistingSensitiveDirectories[$directory] -and $existsAfter) {
            Add-Violation -Violations $violations -Path $directory -Issue "Pack-stage dry-run unexpectedly created a repository package/staging output or runtime mirror directory"
            Remove-DirectoryIfSafe -Path $directory -AllowedRoot $repo
        }
    }

    Remove-DirectoryIfSafe -Path $temporaryRoot -AllowedRoot ([System.IO.Path]::GetTempPath())
}

if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
    Add-Violation -Violations $violations -Path $temporaryRoot -Issue "Temporary runtime pack-stage dry-run output was not cleaned"
}

foreach ($directory in $repoSensitiveDirectories) {
    if (-not $preexistingSensitiveDirectories[$directory] -and (Test-Path -LiteralPath $directory -PathType Container)) {
        Add-Violation -Violations $violations -Path $directory -Issue "Repository package/staging output or runtime mirror directory remains after dry-run cleanup"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Runtime pack stage-forwarding isolation guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Runtime pack stage-forwarding isolation guard passed."
Write-Host "Required OpenCV runtime modules packaged: $($requiredOpenCvModules.Count)."
Write-Host "Temporary StageOutputRoot, RuntimeProject, and package OutputDir were outside the repository and cleaned."
