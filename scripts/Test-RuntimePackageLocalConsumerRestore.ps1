param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
if ($null -eq $pwsh) {
    throw "pwsh was not found. Runtime package local consumer restore validation requires PowerShell 7+."
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnet) {
    throw "dotnet was not found. Runtime package local consumer restore validation requires dotnet restore/build."
}

$packRuntimePath = Join-Path $repo "scripts/Pack-Runtime.ps1"
if (-not (Test-Path -LiteralPath $packRuntimePath -PathType Leaf)) {
    throw "Pack-Runtime.ps1 was not found: $packRuntimePath"
}

$primaryNativeLoader = "JYPPX.OpenCV.Native.dll"
$runtimePackageId = "JYPPX.OpenCV.runtime.win-x64"
$runtimePackageVersion = "5.0.0.0"
$normalizedPackageVersion = "5.0.0"
$packageFileName = "$runtimePackageId.$normalizedPackageVersion.nupkg"
$rid = "win-x64"
$openCvBinarySuffix = "500"
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
    "ml",
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

function Assert-TextContains {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Needle,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    if ($Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $Needle
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
    <Description>Synthetic native runtime package for local consumer restore validation.</Description>
    <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>
    <IsPackable>true</IsPackable>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <NoWarn>$(NoWarn);NU5128</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
    <None Include="build\JYPPX.OpenCV.runtime.provenance.json" Condition="Exists('build\JYPPX.OpenCV.runtime.provenance.json')" Pack="true" PackagePath="build" />
    <None Include="runtimes\$(RuntimePackageRid)\native\**\*" Pack="true" PackagePath="runtimes\$(RuntimePackageRid)\native" />
    <None Include="licenses\**\*" Pack="true" PackagePath="licenses" />
  </ItemGroup>
</Project>
'@
    [System.IO.File]::WriteAllText($projectPath, $projectText)
    [System.IO.File]::WriteAllText((Join-Path $RuntimeProjectDirectory "README.md"), "Synthetic runtime package README")

    return $projectPath
}

function New-TemporaryConsumerProject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ConsumerDirectory
    )

    New-Item -ItemType Directory -Force -Path $ConsumerDirectory | Out-Null
    $projectPath = Join-Path $ConsumerDirectory "RuntimeConsumer.csproj"
    $projectText = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <RuntimeIdentifier>$rid</RuntimeIdentifier>
    <SelfContained>false</SelfContained>
    <UseAppHost>false</UseAppHost>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="$runtimePackageId" Version="$runtimePackageVersion" />
  </ItemGroup>
</Project>
"@
    [System.IO.File]::WriteAllText($projectPath, $projectText)

    $programText = @'
namespace RuntimeConsumer;

internal static class Program
{
    private static int Main()
    {
        return 0;
    }
}
'@
    [System.IO.File]::WriteAllText((Join-Path $ConsumerDirectory "Program.cs"), $programText)

    return $projectPath
}

$violations = [System.Collections.Generic.List[object]]::new()
$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-runtime-local-consumer-" + [System.Guid]::NewGuid().ToString("N"))
$nativeWrapperRuntimeDir = Join-Path $temporaryRoot "native-wrapper-runtime"
$openCvRuntimeDir = Join-Path $temporaryRoot "opencv-runtime"
$openCvSourceDir = Join-Path $temporaryRoot "opencv-source"
$openCvInstallDir = Join-Path $temporaryRoot "opencv-install"
$stageOutputRoot = Join-Path $temporaryRoot "stage-output"
$packageSourceDir = Join-Path $temporaryRoot "local-nuget-source"
$runtimeProjectDir = Join-Path $temporaryRoot "runtime-package-project"
$consumerDir = Join-Path $temporaryRoot "consumer"
$nugetPackagesDir = Join-Path $temporaryRoot "nuget-packages"
$nugetHttpCacheDir = Join-Path $temporaryRoot "nuget-http-cache"
$nugetScratchDir = Join-Path $temporaryRoot "nuget-scratch"
$nugetPluginsCacheDir = Join-Path $temporaryRoot "nuget-plugin-cache"
$nugetConfigPath = Join-Path $temporaryRoot "NuGet.config"

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

$oldNuGetPackages = $env:NUGET_PACKAGES
$oldNuGetHttpCache = $env:NUGET_HTTP_CACHE_PATH
$oldNuGetScratch = $env:NUGET_SCRATCH
$oldNuGetPluginsCache = $env:NUGET_PLUGINS_CACHE_PATH

try {
    foreach ($directory in @(
            $nativeWrapperRuntimeDir,
            $openCvRuntimeDir,
            (Join-Path $openCvSourceDir "3rdparty/ippicv"),
            (Join-Path $openCvInstallDir "etc/licenses"),
            $stageOutputRoot,
            $packageSourceDir,
            $nugetPackagesDir,
            $nugetHttpCacheDir,
            $nugetScratchDir,
            $nugetPluginsCacheDir)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $runtimeProjectPath = New-TemporaryRuntimeProject -RuntimeProjectDirectory $runtimeProjectDir
    $consumerProjectPath = New-TemporaryConsumerProject -ConsumerDirectory $consumerDir

    foreach ($dllName in @($primaryNativeLoader)) {
        Write-SyntheticDll -Path (Join-Path $nativeWrapperRuntimeDir $dllName)
    }

    foreach ($module in $requiredOpenCvModules) {
        Write-SyntheticDll -Path (Join-Path $openCvRuntimeDir "opencv_$module$openCvBinarySuffix.dll")
    }

    [System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "LICENSE"), "Synthetic OpenCV source license")
    [System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "opencv_contrib-LICENSE"), "Synthetic OpenCV contrib source license")
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
        "-SyntheticRuntimeInputs",
        "-OpenCvNativeRuntimeDir", $nativeWrapperRuntimeDir,
        "-OpenCvRuntimeDir", $openCvRuntimeDir,
        "-OpenCvSourceDir", $openCvSourceDir,
        "-OpenCvInstallDir", $openCvInstallDir,
        "-StageOutputRoot", $stageOutputRoot,
        "-OutputDir", $packageSourceDir,
        "-RuntimeProject", $runtimeProjectPath
    )

    $packOutput = & $pwsh.Source @packArguments 2>&1
    $packOutputText = ($packOutput | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine
    if ($LASTEXITCODE -ne 0) {
        Add-Violation -Violations $violations -Path "scripts/Pack-Runtime.ps1" -Issue "Synthetic runtime package generation failed" -Text $packOutputText
    }

    $packagePath = Join-Path $packageSourceDir $packageFileName
    Assert-FileExists -Violations $violations -Path $packagePath -Issue "Synthetic runtime package was not created in the temporary local NuGet source"

    $nugetConfigText = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-runtime" value="$packageSourceDir" />
  </packageSources>
</configuration>
"@
    [System.IO.File]::WriteAllText($nugetConfigPath, $nugetConfigText)

    $env:NUGET_PACKAGES = $nugetPackagesDir
    $env:NUGET_HTTP_CACHE_PATH = $nugetHttpCacheDir
    $env:NUGET_SCRATCH = $nugetScratchDir
    $env:NUGET_PLUGINS_CACHE_PATH = $nugetPluginsCacheDir

    $restoreArguments = @(
        "restore",
        $consumerProjectPath,
        "--configfile", $nugetConfigPath,
        "--packages", $nugetPackagesDir,
        "--no-cache",
        "-p:RuntimeIdentifier=$rid",
        "-v:minimal"
    )
    $restoreOutput = & $dotnet.Source @restoreArguments 2>&1
    $restoreOutputText = ($restoreOutput | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine
    if ($LASTEXITCODE -ne 0) {
        Add-Violation -Violations $violations -Path "consumer/RuntimeConsumer.csproj" -Issue "Temporary consumer restore failed" -Text $restoreOutputText
    }

    $buildArguments = @(
        "build",
        $consumerProjectPath,
        "-c", "Release",
        "--no-restore",
        "-p:RuntimeIdentifier=$rid",
        "-p:RestorePackagesPath=$nugetPackagesDir",
        "-v:minimal"
    )
    $buildOutput = & $dotnet.Source @buildArguments 2>&1
    $buildOutputText = ($buildOutput | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine
    if ($LASTEXITCODE -ne 0) {
        Add-Violation -Violations $violations -Path "consumer/RuntimeConsumer.csproj" -Issue "Temporary consumer build failed" -Text $buildOutputText
    }

    $expectedRuntimeFiles = @($primaryNativeLoader)
    foreach ($module in $requiredOpenCvModules) {
        $expectedRuntimeFiles += "opencv_$module$openCvBinarySuffix.dll"
    }

    $assetsPath = Join-Path $consumerDir "obj/project.assets.json"
    Assert-FileExists -Violations $violations -Path $assetsPath -Issue "Temporary consumer restore did not create project.assets.json"
    if (Test-Path -LiteralPath $assetsPath -PathType Leaf) {
        $assetsText = [System.IO.File]::ReadAllText($assetsPath)
        Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle "$runtimePackageId/$normalizedPackageVersion" -Issue "Consumer assets file must reference the neutral runtime package"
        Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle '"runtimeTargets"' -Issue "Consumer assets file must include runtimeTargets for native runtime assets"
        foreach ($runtimeFile in $expectedRuntimeFiles) {
            Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle "runtimes/$rid/native/$runtimeFile" -Issue "Consumer assets file did not select expected RID native asset"
        }

        if ($assetsText -match "OpenCv5Sharp\.runtime|opencv5sharp\.runtime") {
            Add-Violation -Violations $violations -Path $assetsPath -Issue "Consumer assets file must not contain fixed-major runtime package identity"
        }
    }

    $packageInstallRoot = Join-Path $nugetPackagesDir ("$($runtimePackageId.ToLowerInvariant())/$normalizedPackageVersion")
    foreach ($runtimeFile in $expectedRuntimeFiles) {
        Assert-FileExists `
            -Violations $violations `
            -Path (Join-Path $packageInstallRoot "runtimes/$rid/native/$runtimeFile") `
            -Issue "Isolated NuGet package cache did not contain expected RID native asset"
    }

    $manifestPath = Join-Path $packageInstallRoot "build/JYPPX.OpenCV.runtime.provenance.json"
    Assert-FileExists -Violations $violations -Path $manifestPath -Issue "Isolated NuGet package cache did not contain runtime provenance manifest"
    if (Test-Path -LiteralPath $manifestPath -PathType Leaf) {
        $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
        if ($manifest.PackageId -ne $runtimePackageId -or $manifest.PackageVersion -ne $runtimePackageVersion) {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Runtime provenance manifest must record restored package identity and version" -Text "$($manifest.PackageId) / $($manifest.PackageVersion)"
        }

        if ($manifest.Rid -ne $rid -or -not [bool]$manifest.SyntheticRuntimeInputs) {
            Add-Violation -Violations $violations -Path $manifestPath -Issue "Synthetic runtime provenance manifest must record selected RID and synthetic input status" -Text "$($manifest.Rid) / $($manifest.SyntheticRuntimeInputs)"
        }
    }

    $consumerOutputFiles = @{}
    if (Test-Path -LiteralPath (Join-Path $consumerDir "bin") -PathType Container) {
        foreach ($file in Get-ChildItem -LiteralPath (Join-Path $consumerDir "bin") -Recurse -File) {
            $consumerOutputFiles[$file.Name] = $file.FullName
        }
    }

    foreach ($runtimeFile in $expectedRuntimeFiles) {
        if (-not $consumerOutputFiles.ContainsKey($runtimeFile)) {
            Add-Violation -Violations $violations -Path $consumerDir -Issue "Temporary consumer build output did not copy expected RID native asset" -Text $runtimeFile
        }
    }

    foreach ($outputDirectory in @(
            (Join-Path $consumerDir "bin"),
            (Join-Path $consumerDir "obj"),
            $nugetPackagesDir,
            $nugetHttpCacheDir,
            $nugetScratchDir,
            $nugetPluginsCacheDir)) {
        if ((Test-Path -LiteralPath $outputDirectory -PathType Container) -and
            -not (Test-IsPathUnder -Path $outputDirectory -Root $temporaryRoot)) {
            Add-Violation -Violations $violations -Path $outputDirectory -Issue "Temporary consumer or NuGet output escaped the dry-run root"
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
            Add-Violation -Violations $violations -Path $directory -Issue "Local consumer restore dry-run unexpectedly created a repository package/staging output or runtime mirror directory"
            Remove-DirectoryIfSafe -Path $directory -AllowedRoot $repo
        }
    }

    Remove-DirectoryIfSafe -Path $temporaryRoot -AllowedRoot ([System.IO.Path]::GetTempPath())
}

if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
    Add-Violation -Violations $violations -Path $temporaryRoot -Issue "Temporary runtime package local consumer restore output was not cleaned"
}

foreach ($directory in $repoSensitiveDirectories) {
    if (-not $preexistingSensitiveDirectories[$directory] -and (Test-Path -LiteralPath $directory -PathType Container)) {
        Add-Violation -Violations $violations -Path $directory -Issue "Repository package/staging output or runtime mirror directory remains after dry-run cleanup"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Runtime package local consumer restore guard failed with $($violations.Count) violation(s)."
    foreach ($violation in ($violations | Sort-Object Path, Issue)) {
        Write-Host "Path: $($violation.Path)"
        Write-Host "Issue: $($violation.Issue)"
        if (-not [string]::IsNullOrWhiteSpace($violation.Text)) {
            Write-Host "Text: $($violation.Text)"
        }
    }

    exit 1
}

Write-Host "Runtime package local consumer restore guard passed."
Write-Host "Required OpenCV runtime modules restored and copied: $($requiredOpenCvModules.Count)."
Write-Host "Temporary local NuGet source, packages cache, and consumer project were outside the repository and cleaned."
