param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
if ($null -eq $pwsh) {
    throw "pwsh was not found. Managed/runtime package-pair local consumer validation requires PowerShell 7+."
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnet) {
    throw "dotnet was not found. Managed/runtime package-pair local consumer validation requires dotnet restore/build."
}

$packManagedPath = Join-Path $repo "scripts/Pack-Managed.ps1"
$packRuntimePath = Join-Path $repo "scripts/Pack-Runtime.ps1"
foreach ($scriptPath in @($packManagedPath, $packRuntimePath)) {
    if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf)) {
        throw "Required pack script was not found: $scriptPath"
    }
}

$managedPackageId = "JYPPX.OpenCV.CSharp.API"
$runtimePackageId = "JYPPX.OpenCV.runtime.win-x64"
$packageVersion = "5.0.0.0"
$normalizedPackageVersion = "5.0.0"
$managedPackageFileName = "$managedPackageId.$normalizedPackageVersion.nupkg"
$runtimePackageFileName = "$runtimePackageId.$normalizedPackageVersion.nupkg"
$primaryNativeLoader = "JYPPX.OpenCV.Native.dll"
$compatibilityNativeLoader = "OpenCv5Sharp.Native.dll"
$managedAssemblyName = "JYPPX.OpenCV.CSharp.API.dll"
$fixedMajorManagedIdentity = "Open" + "Cv5Sharp"
$fixedMajorConsumerPattern = (
    [regex]::Escape($fixedMajorManagedIdentity) + "\.runtime|" +
    "opencv" + "5sharp\.runtime|" +
    "PackageReference.*" + [regex]::Escape($fixedMajorManagedIdentity) + "|" +
    "<AssemblyName>\s*" + [regex]::Escape($fixedMajorManagedIdentity))
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
    <Description>Synthetic native runtime package for managed/runtime pair validation.</Description>
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

function New-TemporaryConsumerProject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ConsumerDirectory
    )

    New-Item -ItemType Directory -Force -Path $ConsumerDirectory | Out-Null
    $projectPath = Join-Path $ConsumerDirectory "PackagePairConsumer.csproj"
    $projectText = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <RuntimeIdentifier>$rid</RuntimeIdentifier>
    <SelfContained>false</SelfContained>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="$managedPackageId" Version="$packageVersion" />
    <PackageReference Include="$runtimePackageId" Version="$packageVersion" />
  </ItemGroup>
</Project>
"@
    [System.IO.File]::WriteAllText($projectPath, $projectText)

    $programText = @'
using OpenCvSharp;

namespace PackagePairConsumer;

internal static class Program
{
    private static int Main()
    {
        var message = OpenCvSharpBuildInfo.ManagedPackageId + ":" + OpenCvSharpBuildInfo.PackageVersion;
        var exception = new OpenCvException(message);
        return exception.Message == message ? 0 : 1;
    }
}
'@
    [System.IO.File]::WriteAllText((Join-Path $ConsumerDirectory "Program.cs"), $programText)

    return $projectPath
}

$violations = [System.Collections.Generic.List[object]]::new()
$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-package-pair-consumer-" + [System.Guid]::NewGuid().ToString("N"))
$nativeWrapperRuntimeDir = Join-Path $temporaryRoot "native-wrapper-runtime"
$openCvRuntimeDir = Join-Path $temporaryRoot "opencv-runtime"
$openCvSourceDir = Join-Path $temporaryRoot "opencv-source"
$openCvInstallDir = Join-Path $temporaryRoot "opencv-install"
$stageOutputRoot = Join-Path $temporaryRoot "stage-output"
$packageSourceDir = Join-Path $temporaryRoot "local-nuget-source"
$runtimeProjectDir = Join-Path $temporaryRoot "runtime-package-project"
$managedBuildOutputRoot = Join-Path $temporaryRoot "managed-build"
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
    (Join-Path $repoRuntimeProjectRoot "licenses"),
    (Join-Path $repo "src/OpenCvSharp/bin"),
    (Join-Path $repo "src/OpenCvSharp/obj")
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
            $managedBuildOutputRoot,
            $nugetPackagesDir,
            $nugetHttpCacheDir,
            $nugetScratchDir,
            $nugetPluginsCacheDir)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $runtimeProjectPath = New-TemporaryRuntimeProject -RuntimeProjectDirectory $runtimeProjectDir
    $consumerProjectPath = New-TemporaryConsumerProject -ConsumerDirectory $consumerDir

    foreach ($dllName in @($primaryNativeLoader, $compatibilityNativeLoader)) {
        Write-SyntheticDll -Path (Join-Path $nativeWrapperRuntimeDir $dllName)
    }

    foreach ($module in $requiredOpenCvModules) {
        Write-SyntheticDll -Path (Join-Path $openCvRuntimeDir "opencv_$module$openCvBinarySuffix.dll")
    }

    [System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "LICENSE"), "Synthetic OpenCV source license")
    [System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "3rdparty/ippicv/readme.htm"), "Synthetic IPPICV license")
    [System.IO.File]::WriteAllText((Join-Path $openCvInstallDir "etc/licenses/synthetic-3rdparty.txt"), "Synthetic third-party license")

    $env:NUGET_PACKAGES = $nugetPackagesDir
    $env:NUGET_HTTP_CACHE_PATH = $nugetHttpCacheDir
    $env:NUGET_SCRATCH = $nugetScratchDir
    $env:NUGET_PLUGINS_CACHE_PATH = $nugetPluginsCacheDir

    $managedPackArguments = @(
        "-NoProfile",
        "-File", $packManagedPath,
        "-Configuration", "Release",
        "-OpenCvVersion", "5.0.0",
        "-PackageRevision", "0",
        "-OutputDir", $packageSourceDir,
        "-TargetFrameworks", "net8.0",
        "-BuildOutputRoot", $managedBuildOutputRoot,
        "-RestorePackagesPath", $nugetPackagesDir
    )

    $managedPackOutput = & $pwsh.Source @managedPackArguments 2>&1
    $managedPackOutputText = ($managedPackOutput | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine
    if ($LASTEXITCODE -ne 0) {
        Add-Violation -Violations $violations -Path "scripts/Pack-Managed.ps1" -Issue "Managed package generation failed" -Text $managedPackOutputText
    }

    $runtimePackArguments = @(
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
        "-OutputDir", $packageSourceDir,
        "-RuntimeProject", $runtimeProjectPath
    )

    $runtimePackOutput = & $pwsh.Source @runtimePackArguments 2>&1
    $runtimePackOutputText = ($runtimePackOutput | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine
    if ($LASTEXITCODE -ne 0) {
        Add-Violation -Violations $violations -Path "scripts/Pack-Runtime.ps1" -Issue "Runtime package generation failed" -Text $runtimePackOutputText
    }

    Assert-FileExists -Violations $violations -Path (Join-Path $packageSourceDir $managedPackageFileName) -Issue "Managed package was not created in the temporary local NuGet source"
    Assert-FileExists -Violations $violations -Path (Join-Path $packageSourceDir $runtimePackageFileName) -Issue "Runtime package was not created in the temporary local NuGet source"

    $nugetConfigText = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-package-pair" value="$packageSourceDir" />
  </packageSources>
</configuration>
"@
    [System.IO.File]::WriteAllText($nugetConfigPath, $nugetConfigText)

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
        Add-Violation -Violations $violations -Path "consumer/PackagePairConsumer.csproj" -Issue "Temporary package-pair consumer restore failed" -Text $restoreOutputText
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
        Add-Violation -Violations $violations -Path "consumer/PackagePairConsumer.csproj" -Issue "Temporary package-pair consumer build failed" -Text $buildOutputText
    }

    $expectedRuntimeFiles = @($primaryNativeLoader, $compatibilityNativeLoader)
    foreach ($module in $requiredOpenCvModules) {
        $expectedRuntimeFiles += "opencv_$module$openCvBinarySuffix.dll"
    }

    $assetsPath = Join-Path $consumerDir "obj/project.assets.json"
    Assert-FileExists -Violations $violations -Path $assetsPath -Issue "Temporary package-pair consumer restore did not create project.assets.json"
    if (Test-Path -LiteralPath $assetsPath -PathType Leaf) {
        $assetsText = [System.IO.File]::ReadAllText($assetsPath)
        Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle "$managedPackageId/$normalizedPackageVersion" -Issue "Consumer assets file must reference the neutral managed package"
        Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle "$runtimePackageId/$normalizedPackageVersion" -Issue "Consumer assets file must reference the neutral runtime package"
        Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle "lib/net8.0/$managedAssemblyName" -Issue "Consumer assets file must include managed compile asset"
        Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle '"runtimeTargets"' -Issue "Consumer assets file must include runtimeTargets for native runtime assets"
        foreach ($runtimeFile in $expectedRuntimeFiles) {
            Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle "runtimes/$rid/native/$runtimeFile" -Issue "Consumer assets file did not select expected RID native asset"
        }

        if ($assetsText -match $fixedMajorConsumerPattern) {
            Add-Violation -Violations $violations -Path $assetsPath -Issue "Consumer assets file must not contain fixed-major package identity, assembly name, or package reference"
        }
    }

    $managedPackageInstallRoot = Join-Path $nugetPackagesDir ("$($managedPackageId.ToLowerInvariant())/$normalizedPackageVersion")
    $runtimePackageInstallRoot = Join-Path $nugetPackagesDir ("$($runtimePackageId.ToLowerInvariant())/$normalizedPackageVersion")
    Assert-FileExists -Violations $violations -Path (Join-Path $managedPackageInstallRoot "lib/net8.0/$managedAssemblyName") -Issue "Isolated NuGet package cache did not contain managed compile/runtime assembly"
    foreach ($runtimeFile in $expectedRuntimeFiles) {
        Assert-FileExists `
            -Violations $violations `
            -Path (Join-Path $runtimePackageInstallRoot "runtimes/$rid/native/$runtimeFile") `
            -Issue "Isolated NuGet package cache did not contain expected RID native asset"
    }

    $consumerOutputFiles = @{}
    if (Test-Path -LiteralPath (Join-Path $consumerDir "bin") -PathType Container) {
        foreach ($file in Get-ChildItem -LiteralPath (Join-Path $consumerDir "bin") -Recurse -File) {
            $consumerOutputFiles[$file.Name] = $file.FullName
        }
    }

    if (-not $consumerOutputFiles.ContainsKey($managedAssemblyName)) {
        Add-Violation -Violations $violations -Path $consumerDir -Issue "Temporary consumer build output did not copy managed package assembly" -Text $managedAssemblyName
    }

    foreach ($runtimeFile in $expectedRuntimeFiles) {
        if (-not $consumerOutputFiles.ContainsKey($runtimeFile)) {
            Add-Violation -Violations $violations -Path $consumerDir -Issue "Temporary consumer build output did not copy expected RID native asset" -Text $runtimeFile
        }
    }

    $consumerProjectText = [System.IO.File]::ReadAllText($consumerProjectPath)
    foreach ($packageReference in @(
            "PackageReference Include=`"$managedPackageId`" Version=`"$packageVersion`"",
            "PackageReference Include=`"$runtimePackageId`" Version=`"$packageVersion`"")) {
        Assert-TextContains -Violations $violations -Path $consumerProjectPath -Text $consumerProjectText -Needle $packageReference -Issue "Temporary consumer package references must use neutral package IDs and matching four-part version metadata"
    }

    if ($consumerProjectText -match $fixedMajorConsumerPattern) {
        Add-Violation -Violations $violations -Path $consumerProjectPath -Issue "Temporary consumer must not contain fixed-major package identity, assembly name, or package reference"
    }

    foreach ($outputDirectory in @(
            (Join-Path $consumerDir "bin"),
            (Join-Path $consumerDir "obj"),
            $managedBuildOutputRoot,
            $nugetPackagesDir,
            $nugetHttpCacheDir,
            $nugetScratchDir,
            $nugetPluginsCacheDir)) {
        if ((Test-Path -LiteralPath $outputDirectory -PathType Container) -and
            -not (Test-IsPathUnder -Path $outputDirectory -Root $temporaryRoot)) {
            Add-Violation -Violations $violations -Path $outputDirectory -Issue "Temporary managed/runtime package-pair output escaped the dry-run root"
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
            Add-Violation -Violations $violations -Path $directory -Issue "Package-pair consumer dry-run unexpectedly created a repository output directory"
            Remove-DirectoryIfSafe -Path $directory -AllowedRoot $repo
        }
    }

    Remove-DirectoryIfSafe -Path $temporaryRoot -AllowedRoot ([System.IO.Path]::GetTempPath())
}

if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
    Add-Violation -Violations $violations -Path $temporaryRoot -Issue "Temporary managed/runtime package-pair consumer output was not cleaned"
}

foreach ($directory in $repoSensitiveDirectories) {
    if (-not $preexistingSensitiveDirectories[$directory] -and (Test-Path -LiteralPath $directory -PathType Container)) {
        Add-Violation -Violations $violations -Path $directory -Issue "Repository output directory remains after package-pair dry-run cleanup"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Managed/runtime package-pair local consumer guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Managed/runtime package-pair local consumer guard passed."
Write-Host "Managed and runtime packages restored at matching four-part version metadata: $packageVersion."
Write-Host "Required OpenCV runtime modules restored and copied with managed compile asset: $($requiredOpenCvModules.Count)."
