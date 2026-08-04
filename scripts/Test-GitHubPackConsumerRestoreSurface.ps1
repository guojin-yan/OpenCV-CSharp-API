param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactRoot,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$ExpectedPackageVersion = "",
    [string]$ExpectedSyntheticRuntimeInputs = "true",
    [string]$SelectedRid = "",
    [string]$SelectedRuntimeProfile = "",
    [switch]$CompileNativeSmoke,
    [switch]$RunNativeSmoke,
    [string]$NativeExecutionHost = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$artifactRootFullPath = (Resolve-Path -LiteralPath $ArtifactRoot).Path
. (Join-Path $repo "scripts/PackageVersion.ps1")
$managedPackageId = "JYPPX.OpenCV.CSharp.API"
$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$managedAssemblyName = "$managedPackageId.dll"
$runtimeMatrixPath = "packaging/runtime/runtime-package-matrix.json"
$directoryBuildPropsPath = "Directory.Build.props"
$runtimeProvenanceManifestEntry = "build/JYPPX.OpenCV.runtime.provenance.json"

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnet) {
    throw "dotnet was not found. GitHub pack consumer validation requires dotnet restore/build/run."
}

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

function Get-OpenCvBinarySuffix {
    param(
        [Parameter(Mandatory = $true)]
        [string]$OpenCvVersion
    )

    $version = [System.Version]::Parse($OpenCvVersion)
    return "$($version.Major)$($version.Minor)$($version.Build)"
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

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,
        [switch]$EchoOutputOnSuccess
    )

    $output = & $FilePath @Arguments 2>&1
    $outputText = ($output | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine
    if ($LASTEXITCODE -ne 0) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $outputText
        return $false
    }

    if ($EchoOutputOnSuccess -and -not [string]::IsNullOrWhiteSpace($outputText)) {
        Write-Host $outputText
    }

    return $true
}

function Read-NupkgRuntimeProvenance {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations
    )

    $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entry = $zip.GetEntry($runtimeProvenanceManifestEntry)
        if ($null -eq $entry) {
            Add-Violation -Violations $Violations -Path $Path -Issue "Runtime package must include provenance before consumer native asset validation" -Text $runtimeProvenanceManifestEntry
            return $null
        }

        $stream = $entry.Open()
        try {
            $reader = [System.IO.StreamReader]::new($stream)
            try {
                return ($reader.ReadToEnd() | ConvertFrom-Json)
            }
            finally {
                $reader.Dispose()
            }
        }
        finally {
            $stream.Dispose()
        }
    }
    catch {
        Add-Violation -Violations $Violations -Path $Path -Issue "Runtime package provenance must be readable JSON for consumer validation" -Text $_.Exception.Message
        return $null
    }
    finally {
        $zip.Dispose()
    }
}

function Get-RuntimePackageId {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Rid,
        [Parameter(Mandatory = $true)]
        [string]$Profile
    )

    $profileSuffix = if ($Profile -eq "mini") { ".mini" } else { "" }
    return "$runtimePackagePrefix.$Rid$profileSuffix"
}

function Get-NativeFileNames {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Rid,
        [Parameter(Mandatory = $true)]
        [string]$PlatformFamily,
        [Parameter(Mandatory = $true)]
        [string[]]$Modules,
        [Parameter(Mandatory = $true)]
        [string]$OpenCvVersion,
        [Parameter(Mandatory = $true)]
        [string]$OpenCvBinarySuffix,
        [Parameter(Mandatory = $true)]
        [bool]$SyntheticRuntimeInputs
    )

    $ridIsWindows = $Rid.StartsWith("win-", [System.StringComparison]::OrdinalIgnoreCase)
    $primaryLoaderName = if ($ridIsWindows) { "JYPPX.OpenCV.Native.dll" } else { "libJYPPX.OpenCV.Native.so" }
    $moduleFileNames = foreach ($module in $Modules) {
        if ($ridIsWindows) {
            "opencv_$module$OpenCvBinarySuffix.dll"
        }
        elseif ($PlatformFamily -eq "linux" -and -not $SyntheticRuntimeInputs) {
            @(
                "libopencv_$module.so",
                "libopencv_$module.so.$OpenCvBinarySuffix",
                "libopencv_$module.so.$OpenCvVersion"
            )
        }
        elseif ($PlatformFamily -eq "android") {
            "libopencv_$module.so"
        }
        else {
            "libopencv_$module.so.$OpenCvVersion"
        }
    }

    return [pscustomobject]@{
        PrimaryLoader = $primaryLoaderName
        Modules = @($moduleFileNames)
        All = @($primaryLoaderName) + @($moduleFileNames)
    }
}

function New-TemporaryConsumerProject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ConsumerDirectory,
        [Parameter(Mandatory = $true)]
        [string]$Rid,
        [Parameter(Mandatory = $true)]
        [string]$RuntimePackageId,
        [Parameter(Mandatory = $true)]
        [string]$PackageVersion,
        [Parameter(Mandatory = $true)]
        [string]$RuntimeIdentifierGraphPath,
        [Parameter(Mandatory = $true)]
        [string]$RuntimeProfile,
        [switch]$RunNativeSmoke
    )

    New-Item -ItemType Directory -Force -Path $ConsumerDirectory | Out-Null
    $projectPath = Join-Path $ConsumerDirectory "PackageConsumer.csproj"
    $projectText = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <RuntimeIdentifier>$Rid</RuntimeIdentifier>
    <RuntimeIdentifierGraphPath>$RuntimeIdentifierGraphPath</RuntimeIdentifierGraphPath>
    <SelfContained>false</SelfContained>
    <UseAppHost>false</UseAppHost>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="$managedPackageId" Version="$PackageVersion" />
    <PackageReference Include="$RuntimePackageId" Version="$PackageVersion" />
  </ItemGroup>
</Project>
"@
    [System.IO.File]::WriteAllText($projectPath, $projectText)

    if ($RunNativeSmoke) {
        $programText = @'
using System;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;

namespace PackageConsumer;

internal static class Program
{
    [DllImport("JYPPX.OpenCV.Native", EntryPoint = "jyppx_ocv_imgproc_good_features_to_track_count", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GoodFeaturesToTrackCount(
        IntPtr image,
        IntPtr mask,
        int maxCorners,
        double qualityLevel,
        double minDistance,
        int blockSize,
        int gradientSize,
        int useHarrisDetector,
        double k,
        out int cornerCount);

    private static int Main()
    {
        try
        {
__TARGET_PROCESS_ARCHITECTURE_GUARD__
            using var source = new Mat(3, 4, MatType.CV_8UC3, new Scalar(10, 20, 30));
            using var gray = new Mat();
            JYPPX.OpenCvSharp.ImgProc.Cv2.CvtColor(source, gray, JYPPX.OpenCvSharp.ImgProc.ColorConversionCodes.BGR2GRAY);
            if (source.Empty || source.Rows != 3 || source.Cols != 4 || source.Channels != 3)
            {
                Console.Error.WriteLine("CORE_SMOKE_FAILED");
                return 10;
            }

            if (gray.Empty || gray.Rows != 3 || gray.Cols != 4 || gray.Channels != 1)
            {
                Console.Error.WriteLine("IMGPROC_SMOKE_FAILED");
                return 11;
            }

            byte[] encoded = JYPPX.OpenCvSharp.ImgCodecs.Cv2.ImEncode(".png", gray);
            using var decoded = JYPPX.OpenCvSharp.ImgCodecs.Cv2.ImDecode(encoded, JYPPX.OpenCvSharp.ImgCodecs.ImreadModes.Grayscale);
            if (encoded.Length == 0 || decoded.Empty || decoded.Rows != 3 || decoded.Cols != 4 || decoded.Channels != 1)
            {
                Console.Error.WriteLine("IMGCODECS_SMOKE_FAILED");
                return 12;
            }

            using var capture = new JYPPX.OpenCvSharp.VideoIO.VideoCapture();
            if (capture.IsOpened)
            {
                Console.Error.WriteLine("VIDEOIO_SMOKE_FAILED");
                return 13;
            }

__PROFILE_SPECIFIC_NATIVE_SMOKE__
            Console.WriteLine("__TARGETED_NATIVE_SMOKE_SUCCESS__");
            return 0;
        }
        catch (DllNotFoundException exception)
        {
            Console.Error.WriteLine("NATIVE_LOADER_OR_SONAME_MISSING: " + exception.Message);
            return 20;
        }
        catch (EntryPointNotFoundException exception)
        {
            Console.Error.WriteLine("SUPPORTED_PROFILE_ENTRYPOINT_MISSING: " + exception.Message);
            return 21;
        }
        catch (OpenCvException exception)
        {
            Console.Error.WriteLine("SUPPORTED_PROFILE_OPENCV_FAILURE: " + exception.Message);
            return 22;
        }
    }
}
'@

        $profileSpecificNativeSmoke = ""
        $targetProcessArchitectureGuard = ""
        $successMarker = "TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio"
        if ($Rid -eq "win-x86") {
            $targetProcessArchitectureGuard = @'
            string processorArchitecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") ?? "";
            string processorArchitectureWow64 = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432") ?? "";
            if (RuntimeInformation.ProcessArchitecture != Architecture.X86 ||
                RuntimeInformation.OSArchitecture != Architecture.X64 ||
                Environment.Is64BitProcess ||
                !Environment.Is64BitOperatingSystem ||
                !string.Equals(processorArchitecture, "x86", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(processorArchitectureWow64, "AMD64", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine("WINDOWS_X86_PROCESS_ARCHITECTURE_FAILED OSArchitecture=" + RuntimeInformation.OSArchitecture +
                    " ProcessArchitecture=" + RuntimeInformation.ProcessArchitecture +
                    " Is64BitProcess=" + Environment.Is64BitProcess +
                    " Is64BitOperatingSystem=" + Environment.Is64BitOperatingSystem +
                    " PROCESSOR_ARCHITECTURE=" + processorArchitecture +
                    " PROCESSOR_ARCHITEW6432=" + processorArchitectureWow64);
                return 9;
            }
            Console.WriteLine("WINDOWS_X86_PACKAGE_CONSUMER_PROCESS_OK OSArchitecture=" + RuntimeInformation.OSArchitecture +
                " ProcessArchitecture=" + RuntimeInformation.ProcessArchitecture +
                " Is64BitProcess=" + Environment.Is64BitProcess +
                " Is64BitOperatingSystem=" + Environment.Is64BitOperatingSystem +
                " PROCESSOR_ARCHITECTURE=" + processorArchitecture +
                " PROCESSOR_ARCHITEW6432=" + processorArchitectureWow64);
'@
        }
        if ($RuntimeProfile -eq "full") {
            $profileSpecificNativeSmoke = @'
            using (Mat blob = JYPPX.OpenCvSharp.Dnn.Cv2.BlobFromImage(source, 1.0, new Size(4, 3)))
            {
                if (blob.Empty || blob.Dims != 4)
                {
                    Console.Error.WriteLine("FULL_DNN_SMOKE_FAILED");
                    return 14;
                }
            }
'@
            $successMarker = "TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,dnn profile=full"
        }
        elseif ($RuntimeProfile -eq "mini") {
            $profileSpecificNativeSmoke = @'
            int cornerCount = -1;
            int status = GoodFeaturesToTrackCount(IntPtr.Zero, IntPtr.Zero, 8, 0.01, 1.0, 3, 3, 0, 0.04, out cornerCount);
            if (status != -100 || cornerCount != 0)
            {
                Console.Error.WriteLine("MINI_NOT_LINKED_SMOKE_FAILED status=" + status + " cornerCount=" + cornerCount);
                return 15;
            }
'@
            $successMarker = "TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,not_linked profile=mini"
        }
        else {
            throw "Targeted native smoke supports only full or mini profiles, got '$RuntimeProfile'."
        }

        $programText = $programText.Replace("__TARGET_PROCESS_ARCHITECTURE_GUARD__", $targetProcessArchitectureGuard)
        $programText = $programText.Replace("__PROFILE_SPECIFIC_NATIVE_SMOKE__", $profileSpecificNativeSmoke)
        $programText = $programText.Replace("__TARGETED_NATIVE_SMOKE_SUCCESS__", $successMarker)
    }
    else {
        $programText = @'
using JYPPX.OpenCvSharp;

namespace PackageConsumer;

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
    }
    [System.IO.File]::WriteAllText((Join-Path $ConsumerDirectory "Program.cs"), $programText)

    return $projectPath
}

function Test-ContainsDisallowedFixedMajorIdentity {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    $fixedMajorManagedIdentity = "Open" + "Cv5Sharp"
    $pattern = (
        [regex]::Escape($fixedMajorManagedIdentity) + "\.runtime|" +
        "opencv" + "5sharp\.runtime|" +
        "PackageReference.*" + [regex]::Escape($fixedMajorManagedIdentity) + "|" +
        "<AssemblyName>\s*" + [regex]::Escape($fixedMajorManagedIdentity))
    return $Text -match $pattern
}

function Assert-NoFixedMajorEntries {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Root
    )

    if (-not (Test-Path -LiteralPath $Root -PathType Container)) {
        return
    }

    foreach ($file in Get-ChildItem -LiteralPath $Root -Recurse -File) {
        if ($file.FullName -match ("Open" + "Cv5Sharp|opencv" + "5sharp")) {
            Add-Violation -Violations $Violations -Path $file.FullName -Issue "Restored package content must not contain a fixed-major file identity"
        }
    }
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

$propertyMap = Get-DirectoryBuildPropertyMap
if ([string]::IsNullOrWhiteSpace($ExpectedPackageVersion)) {
    $ExpectedPackageVersion = Resolve-DirectoryBuildProperty -PropertyMap $propertyMap -Name "OpenCvCSharpPackageVersion"
}
$openCvVersion = Resolve-DirectoryBuildProperty -PropertyMap $propertyMap -Name "OpenCvCSharpOpenCvVersion"
$normalizedPackageVersion = Get-NormalizedPackageFileVersion -VersionText $ExpectedPackageVersion
$openCvBinarySuffix = Get-OpenCvBinarySuffix -OpenCvVersion $openCvVersion
$matrixText = Read-RequiredText -RelativePath $runtimeMatrixPath
$matrix = $matrixText | ConvertFrom-Json
$violations = [System.Collections.Generic.List[object]]::new()
$consumerResults = [System.Collections.Generic.List[object]]::new()
$expectedSyntheticRuntimeInputsValue = [bool]::Parse($ExpectedSyntheticRuntimeInputs)
$selectedMode = -not [string]::IsNullOrWhiteSpace($SelectedRid) -or -not [string]::IsNullOrWhiteSpace($SelectedRuntimeProfile)
if ($selectedMode -and ([string]::IsNullOrWhiteSpace($SelectedRid) -or [string]::IsNullOrWhiteSpace($SelectedRuntimeProfile))) {
    throw "SelectedRid and SelectedRuntimeProfile must be provided together."
}

$selectedRidSpecs = @($matrix.rids | Where-Object { $_.rid -eq $SelectedRid })
$selectedProfileSpecs = @($matrix.profiles | Where-Object { $_.name -eq $SelectedRuntimeProfile })
if ($selectedMode -and ($selectedRidSpecs.Count -ne 1 -or $selectedProfileSpecs.Count -ne 1)) {
    throw "Selected RID/profile was not found exactly once in the runtime matrix: $SelectedRid / $SelectedRuntimeProfile"
}

if ($CompileNativeSmoke -and -not $selectedMode) {
    throw "CompileNativeSmoke requires one selected RID/profile package pair."
}

if ($RunNativeSmoke -and (-not $selectedMode -or $expectedSyntheticRuntimeInputsValue)) {
    throw "RunNativeSmoke requires one selected non-synthetic RID/profile package pair."
}
if ($RunNativeSmoke -and $SelectedRid -eq "win-x86" -and [string]::IsNullOrWhiteSpace($NativeExecutionHost)) {
    throw "win-x86 native execution requires an explicit factual x86 .NET runtime host."
}
if (-not [string]::IsNullOrWhiteSpace($NativeExecutionHost) -and $SelectedRid -ne "win-x86") {
    throw "NativeExecutionHost is reserved for the exact win-x86 package consumer boundary."
}

$nativeExecutionHostPath = ""
if (-not [string]::IsNullOrWhiteSpace($NativeExecutionHost)) {
    $nativeExecutionHostPath = if ([System.IO.Path]::IsPathRooted($NativeExecutionHost)) {
        $NativeExecutionHost
    }
    else {
        Join-Path $repo $NativeExecutionHost
    }
    if (-not (Test-Path -LiteralPath $nativeExecutionHostPath -PathType Leaf)) {
        throw "NativeExecutionHost was not found: $nativeExecutionHostPath"
    }
    $nativeExecutionHostPath = (Resolve-Path -LiteralPath $nativeExecutionHostPath).Path
}

$managedArtifactDir = Join-Path $artifactRootFullPath "nupkg-managed"
$managedPackagePath = Join-Path $managedArtifactDir "$managedPackageId.$normalizedPackageVersion.nupkg"
Assert-FileExists -Violations $violations -Path $managedPackagePath -Issue "Downloaded artifacts must include the managed package before consumer restore validation"

$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-github-pack-consumer-" + [System.Guid]::NewGuid().ToString("N"))
$packageSourceDir = Join-Path $temporaryRoot "local-nuget-source"
$consumerRoot = Join-Path $temporaryRoot "consumers"
$nugetPackagesDir = Join-Path $temporaryRoot "nuget-packages"
$nugetHttpCacheDir = Join-Path $temporaryRoot "nuget-http-cache"
$nugetScratchDir = Join-Path $temporaryRoot "nuget-scratch"
$nugetPluginsCacheDir = Join-Path $temporaryRoot "nuget-plugin-cache"
$nugetConfigPath = Join-Path $temporaryRoot "NuGet.config"
$runtimeIdentifierGraphPath = Join-Path $temporaryRoot "runtime-distro-rid-graph.json"

$oldNuGetPackages = $env:NUGET_PACKAGES
$oldNuGetHttpCache = $env:NUGET_HTTP_CACHE_PATH
$oldNuGetScratch = $env:NUGET_SCRATCH
$oldNuGetPluginsCache = $env:NUGET_PLUGINS_CACHE_PATH

try {
    foreach ($directory in @(
            $packageSourceDir,
            $consumerRoot,
            $nugetPackagesDir,
            $nugetHttpCacheDir,
            $nugetScratchDir,
            $nugetPluginsCacheDir)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    Copy-Item -LiteralPath (Join-Path $repo "packaging/runtime/runtime-distro-rid-graph.json") -Destination $runtimeIdentifierGraphPath -Force

    $artifactPackages = @(Get-ChildItem -LiteralPath $artifactRootFullPath -Recurse -Filter "*.nupkg" -File)
    if ($artifactPackages.Count -eq 0) {
        Add-Violation -Violations $violations -Path $ArtifactRoot -Issue "Downloaded artifact root did not contain any .nupkg files"
    }
    else {
        foreach ($package in $artifactPackages) {
            Copy-Item -LiteralPath $package.FullName -Destination $packageSourceDir -Force
        }
    }

    $nugetConfigText = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="github-pack-artifacts" value="$packageSourceDir" />
  </packageSources>
</configuration>
"@
    [System.IO.File]::WriteAllText($nugetConfigPath, $nugetConfigText)

    $env:NUGET_PACKAGES = $nugetPackagesDir
    $env:NUGET_HTTP_CACHE_PATH = $nugetHttpCacheDir
    $env:NUGET_SCRATCH = $nugetScratchDir
    $env:NUGET_PLUGINS_CACHE_PATH = $nugetPluginsCacheDir

    foreach ($ridSpec in @($matrix.rids)) {
        foreach ($profileSpec in @($matrix.profiles)) {
            $rid = [string]$ridSpec.rid
            $profile = [string]$profileSpec.name
            if ($selectedMode -and ($rid -ne $SelectedRid -or $profile -ne $SelectedRuntimeProfile)) {
                continue
            }

            $requiredModules = @($profileSpec.modules | ForEach-Object { [string]$_ })
            $expectedOptionalModules = @($profileSpec.optionalModules | ForEach-Object { [string]$_ })
            $runtimePackageId = Get-RuntimePackageId -Rid $rid -Profile $profile
            $artifactName = "nupkg-$rid-$profile"
            $artifactDir = Join-Path $artifactRootFullPath $artifactName
            $runtimePackagePath = Join-Path $artifactDir "$runtimePackageId.$normalizedPackageVersion.nupkg"
            Assert-FileExists -Violations $violations -Path $runtimePackagePath -Issue "Downloaded artifacts must include the selected runtime package before consumer restore validation"
            if (-not (Test-Path -LiteralPath $runtimePackagePath -PathType Leaf)) {
                continue
            }

            $runtimeManifest = Read-NupkgRuntimeProvenance -Path $runtimePackagePath -Violations $violations
            $optionalModulesStaged = @()
            if ($null -ne $runtimeManifest -and $null -ne $runtimeManifest.PSObject.Properties["OptionalModulesStaged"]) {
                $optionalModulesStaged = @($runtimeManifest.OptionalModulesStaged | ForEach-Object { [string]$_ })
            }

            $expectedOptionalModulesStaged = @($expectedOptionalModules | Where-Object { $optionalModulesStaged -contains $_ })
            if ($optionalModulesStaged.Count -ne $expectedOptionalModulesStaged.Count -or (($optionalModulesStaged -join ",") -ne ($expectedOptionalModulesStaged -join ","))) {
                Add-Violation -Violations $violations -Path $runtimePackagePath -Issue "Consumer runtime provenance staged optional modules must be an ordered unique subset of the selected runtime profile" -Text "Found $($optionalModulesStaged -join ','), allowed $($expectedOptionalModules -join ',')"
            }

            $modules = @($requiredModules) + @($optionalModulesStaged)

            $consumerName = "$rid-$profile"
            $consumerDir = Join-Path $consumerRoot $consumerName
            $consumerProjectPath = New-TemporaryConsumerProject `
                -ConsumerDirectory $consumerDir `
                -Rid $rid `
                -RuntimePackageId $runtimePackageId `
                -PackageVersion $ExpectedPackageVersion `
                -RuntimeIdentifierGraphPath $runtimeIdentifierGraphPath `
                -RuntimeProfile $profile `
                -RunNativeSmoke:($CompileNativeSmoke -or $RunNativeSmoke)
            $nativeNames = Get-NativeFileNames `
                -Rid $rid `
                -PlatformFamily ([string]$ridSpec.platformFamily) `
                -Modules $modules `
                -OpenCvVersion $openCvVersion `
                -OpenCvBinarySuffix $openCvBinarySuffix `
                -SyntheticRuntimeInputs $expectedSyntheticRuntimeInputsValue

            $restoreArguments = @(
                "restore",
                $consumerProjectPath,
                "--configfile", $nugetConfigPath,
                "--packages", $nugetPackagesDir,
                "--no-cache",
                "-p:RuntimeIdentifier=$rid",
                "-v:minimal"
            )
            $restoreSucceeded = Invoke-CheckedCommand `
                -Violations $violations `
                -Path $consumerProjectPath `
                -Issue "Temporary GitHub artifact consumer restore failed" `
                -FilePath $dotnet.Source `
                -Arguments $restoreArguments

            $buildSucceeded = $false
            if ($restoreSucceeded) {
                $buildArguments = @(
                    "build",
                    $consumerProjectPath,
                    "-c", "Release",
                    "--no-restore",
                    "-p:RuntimeIdentifier=$rid",
                    "-p:RestorePackagesPath=$nugetPackagesDir",
                    "-v:minimal"
                )
                $buildSucceeded = Invoke-CheckedCommand `
                    -Violations $violations `
                    -Path $consumerProjectPath `
                    -Issue "Temporary GitHub artifact consumer build failed" `
                    -FilePath $dotnet.Source `
                    -Arguments $buildArguments
            }

            $nativeSmokeSucceeded = $null
            if ($RunNativeSmoke -and $buildSucceeded) {
                $runHost = $dotnet.Source
                $runArguments = @(
                    "run",
                    "--project", $consumerProjectPath,
                    "-c", "Release",
                    "-r", $rid,
                    "--no-build",
                    "--no-restore",
                    "-p:RestorePackagesPath=$nugetPackagesDir"
                )
                if ($rid -eq "win-x86") {
                    $consumerAssemblyCandidates = @(
                        Get-ChildItem -LiteralPath (Join-Path $consumerDir "bin/Release/net8.0/win-x86") -File -Filter "PackageConsumer.dll"
                    )
                    if ($consumerAssemblyCandidates.Count -ne 1) {
                        Add-Violation -Violations $violations -Path $consumerDir -Issue "win-x86 consumer build must produce exactly one executable managed assembly for the x86 runtime host" -Text "Found $($consumerAssemblyCandidates.Count)"
                        $nativeSmokeSucceeded = $false
                        continue
                    }
                    $runHost = $nativeExecutionHostPath
                    $runArguments = @($consumerAssemblyCandidates[0].FullName)
                }
                $nativeSmokeSucceeded = Invoke-CheckedCommand `
                    -Violations $violations `
                    -Path $consumerProjectPath `
                    -Issue "Targeted GitHub artifact consumer native smoke failed; inspect loader, SONAME, RID selection, and supported profile entrypoint diagnostics" `
                    -FilePath $runHost `
                    -Arguments $runArguments `
                    -EchoOutputOnSuccess
            }

            $assetsPath = Join-Path $consumerDir "obj/project.assets.json"
            Assert-FileExists -Violations $violations -Path $assetsPath -Issue "Temporary GitHub artifact consumer restore did not create project.assets.json"
            if (Test-Path -LiteralPath $assetsPath -PathType Leaf) {
                $assetsText = [System.IO.File]::ReadAllText($assetsPath)
                Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle "$managedPackageId/$normalizedPackageVersion" -Issue "Consumer assets file must reference the neutral managed package"
                Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle "$runtimePackageId/$normalizedPackageVersion" -Issue "Consumer assets file must reference the selected neutral runtime package"
                Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle "lib/net8.0/$managedAssemblyName" -Issue "Consumer assets file must include the managed compile asset"
                Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle '"runtimeTargets"' -Issue "Consumer assets file must include runtimeTargets for native runtime assets"
                foreach ($runtimeFile in @($nativeNames.All)) {
                    Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle "runtimes/$rid/native/$runtimeFile" -Issue "Consumer assets file did not select expected RID native asset"
                }

                if (Test-ContainsDisallowedFixedMajorIdentity -Text $assetsText) {
                    Add-Violation -Violations $violations -Path $assetsPath -Issue "Consumer assets file must not contain fixed-major package identity, assembly name, or package reference"
                }
            }

            $managedPackageInstallRoot = Join-Path $nugetPackagesDir "$($managedPackageId.ToLowerInvariant())/$normalizedPackageVersion"
            $runtimePackageInstallRoot = Join-Path $nugetPackagesDir "$($runtimePackageId.ToLowerInvariant())/$normalizedPackageVersion"
            Assert-FileExists -Violations $violations -Path (Join-Path $managedPackageInstallRoot "lib/net8.0/$managedAssemblyName") -Issue "Isolated NuGet package cache did not contain managed compile/runtime assembly"
            foreach ($runtimeFile in @($nativeNames.All)) {
                Assert-FileExists `
                    -Violations $violations `
                    -Path (Join-Path $runtimePackageInstallRoot "runtimes/$rid/native/$runtimeFile") `
                    -Issue "Isolated NuGet package cache did not contain expected RID native asset"
            }

            $nativeCacheDirectory = Join-Path $runtimePackageInstallRoot "runtimes/$rid/native"
            if (Test-Path -LiteralPath $nativeCacheDirectory -PathType Container) {
                $restoredModuleFiles = @(Get-ChildItem -LiteralPath $nativeCacheDirectory -File | Where-Object {
                        $_.Name -match "^(opencv_|libopencv_)"
                    })
                if ($restoredModuleFiles.Count -ne @($nativeNames.Modules).Count) {
                    Add-Violation -Violations $violations -Path $nativeCacheDirectory -Issue "Restored runtime package module file count must match selected runtime profile and provenance mode" -Text "Found $($restoredModuleFiles.Count), expected $(@($nativeNames.Modules).Count)"
                }

                Assert-NoFixedMajorEntries -Violations $violations -Root $nativeCacheDirectory
            }

            $consumerOutputFiles = @{}
            $consumerBinDirectory = Join-Path $consumerDir "bin"
            if (Test-Path -LiteralPath $consumerBinDirectory -PathType Container) {
                foreach ($file in Get-ChildItem -LiteralPath $consumerBinDirectory -Recurse -File) {
                    $consumerOutputFiles[$file.Name] = $file.FullName
                }
            }

            if (-not $consumerOutputFiles.ContainsKey($managedAssemblyName)) {
                Add-Violation -Violations $violations -Path $consumerDir -Issue "Temporary consumer build output did not copy managed package assembly" -Text $managedAssemblyName
            }

            foreach ($runtimeFile in @($nativeNames.All)) {
                if (-not $consumerOutputFiles.ContainsKey($runtimeFile)) {
                    Add-Violation -Violations $violations -Path $consumerDir -Issue "Temporary consumer build output did not copy expected RID native asset" -Text $runtimeFile
                }
            }

            Assert-NoFixedMajorEntries -Violations $violations -Root $consumerBinDirectory

            $consumerProjectText = [System.IO.File]::ReadAllText($consumerProjectPath)
            foreach ($packageReference in @(
                    "PackageReference Include=`"$managedPackageId`" Version=`"$ExpectedPackageVersion`"",
                    "PackageReference Include=`"$runtimePackageId`" Version=`"$ExpectedPackageVersion`"")) {
                Assert-TextContains -Violations $violations -Path $consumerProjectPath -Text $consumerProjectText -Needle $packageReference -Issue "Temporary consumer package references must use neutral package IDs and matching four-part version metadata"
            }

            if (Test-ContainsDisallowedFixedMajorIdentity -Text $consumerProjectText) {
                Add-Violation -Violations $violations -Path $consumerProjectPath -Issue "Temporary consumer project must not contain fixed-major package identity, assembly name, or package reference"
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

            $consumerResults.Add([pscustomobject]@{
                Rid = $rid
                Profile = $profile
                RuntimePackage = $runtimePackageId
                RestoreSucceeded = $restoreSucceeded
                BuildSucceeded = $buildSucceeded
                NativeSmokeSucceeded = $nativeSmokeSucceeded
                ExpectedModuleCount = $modules.Count
                NativeAssetCount = @($nativeNames.All).Count
            })
        }
    }
}
finally {
    $env:NUGET_PACKAGES = $oldNuGetPackages
    $env:NUGET_HTTP_CACHE_PATH = $oldNuGetHttpCache
    $env:NUGET_SCRATCH = $oldNuGetScratch
    $env:NUGET_PLUGINS_CACHE_PATH = $oldNuGetPluginsCache

    Remove-DirectoryIfSafe -Path $temporaryRoot -AllowedRoot ([System.IO.Path]::GetTempPath())
}

if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
    Add-Violation -Violations $violations -Path $temporaryRoot -Issue "Temporary GitHub pack consumer restore output was not cleaned"
}

if ($violations.Count -gt 0) {
    Write-Host "GitHub pack consumer restore guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        ForEach-Object {
            $text = if ([string]::IsNullOrWhiteSpace($_.Text)) { "" } else { " :: $($_.Text)" }
            Write-Host "$($_.Path) :: $($_.Issue)$text"
        }
    exit 1
}

$runtimeByProfile = $consumerResults |
    Group-Object Profile |
    Sort-Object Name |
    ForEach-Object {
        [pscustomobject]@{
            Profile = $_.Name
            Count = $_.Count
            ModuleCounts = (@($_.Group | Select-Object -ExpandProperty ExpectedModuleCount | Sort-Object -Unique) -join ",")
        }
    }

Write-Host "GitHub pack consumer restore guard passed."
Write-Host "Temporary local NuGet source and package cache stayed outside the repository and were cleaned."
Write-Host "Consumer package pairs checked: $($consumerResults.Count)."
if ($selectedMode) {
    Write-Host "Selected consumer package pair: $SelectedRid / $SelectedRuntimeProfile."
}
foreach ($profileSummary in @($runtimeByProfile)) {
    Write-Host "$($profileSummary.Profile) consumer package pairs: $($profileSummary.Count), module counts: $($profileSummary.ModuleCounts)."
}
