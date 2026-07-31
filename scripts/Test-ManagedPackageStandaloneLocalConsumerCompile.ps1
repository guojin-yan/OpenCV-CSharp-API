param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
if ($null -eq $pwsh) {
    throw "pwsh was not found. Standalone managed package consumer validation requires PowerShell 7+."
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnet) {
    throw "dotnet was not found. Standalone managed package consumer validation requires dotnet restore/build."
}

$packManagedPath = Join-Path $repo "scripts/Pack-Managed.ps1"
if (-not (Test-Path -LiteralPath $packManagedPath -PathType Leaf)) {
    throw "Required managed pack script was not found: $packManagedPath"
}

$managedPackageId = "JYPPX.OpenCV.CSharp.API"
$managedPackageIdLower = $managedPackageId.ToLowerInvariant()
$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$packageVersion = "5.0.0.0"
$normalizedPackageVersion = "5.0.0"
$targetFramework = "net8.0"
$managedPackageFileName = "$managedPackageId.$normalizedPackageVersion.nupkg"
$managedAssemblyFileName = "$managedPackageId.dll"
$primaryNativeLoader = "JYPPX.OpenCV.Native.dll"
$compatibilityNativeLoader = "OpenCv5Sharp.Native.dll"
$fixedMajorManagedIdentity = "Open" + "Cv5Sharp"
$fixedMajorConsumerPattern = (
    "PackageReference.*" + [regex]::Escape($fixedMajorManagedIdentity) + "|" +
    "<PackageId>\s*" + [regex]::Escape($fixedMajorManagedIdentity) + "|" +
    "<AssemblyName>\s*" + [regex]::Escape($fixedMajorManagedIdentity) + "|" +
    [regex]::Escape($fixedMajorManagedIdentity) + "\.runtime|" +
    "opencv" + "5sharp\.runtime")
$fixedMajorSourcePattern = "Open" + "Cv5Sharp|opencv" + "5sharp"
$representativeSourceNeedles = @(
    "OpenCvSharp.Core",
    "OpenCvSharp.ImgProc",
    "OpenCvSharp.ImgCodecs",
    "OpenCvSharp.Features2D",
    "OpenCvSharp.Calib3D",
    "OpenCvSharp.Dnn",
    "OpenCvSharp.ObjDetect",
    "OpenCvSharp.Photo",
    "OpenCvSharp.Video",
    "OpenCvSharp.VideoIO",
    "OpenCvSharp.ML",
    "OpenCvSharp.Stitching",
    "OpenCvSharp.Geometry")

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

function Assert-TextDoesNotContain {
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

    if ($Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $Needle
    }
}

function Assert-ManagedPackageSurface {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$PackagePath
    )

    if (-not (Test-Path -LiteralPath $PackagePath -PathType Leaf)) {
        return
    }

    $archive = $null
    try {
        $archive = [System.IO.Compression.ZipFile]::OpenRead($PackagePath)
        $managedLibEntries = @(
            $archive.Entries |
                Where-Object {
                    $_.FullName.StartsWith("lib/", [System.StringComparison]::OrdinalIgnoreCase) -and
                    $_.FullName.EndsWith(".dll", [System.StringComparison]::OrdinalIgnoreCase)
                } |
                ForEach-Object { $_.FullName }
        )
        $expectedEntryName = "lib/$targetFramework/$managedAssemblyFileName"
        if ($managedLibEntries.Count -ne 1 -or $managedLibEntries[0] -ne $expectedEntryName) {
            Add-Violation `
                -Violations $Violations `
                -Path $PackagePath `
                -Issue "Standalone managed consumer package source must contain exactly the isolated net8.0 managed assembly under lib" `
                -Text ($managedLibEntries -join "; ")
        }
    }
    catch {
        Add-Violation -Violations $Violations -Path $PackagePath -Issue "Standalone managed consumer package artifact could not be inspected" -Text $_.Exception.Message
    }
    finally {
        if ($null -ne $archive) {
            $archive.Dispose()
        }
    }
}

function New-TemporaryConsumerProject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ConsumerDirectory
    )

    New-Item -ItemType Directory -Force -Path $ConsumerDirectory | Out-Null
    $projectPath = Join-Path $ConsumerDirectory "StandaloneManagedConsumer.csproj"
    $projectText = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>$targetFramework</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="$managedPackageId" Version="$packageVersion" />
  </ItemGroup>
</Project>
"@
    [System.IO.File]::WriteAllText($projectPath, $projectText)

    $programText = @'
using System;
using OpenCvSharp;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;
using CoreCv2 = OpenCvSharp.Core.Cv2;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;
using ImgCodecsCv2 = OpenCvSharp.ImgCodecs.Cv2;
using Features2DCv2 = OpenCvSharp.Features2D.Cv2;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;
using DnnCv2 = OpenCvSharp.Dnn.Cv2;
using HighGuiCv2 = OpenCvSharp.HighGui.Cv2;
using PhotoCv2 = OpenCvSharp.Photo.PhotoCv2;
using VideoCv2 = OpenCvSharp.Video.Cv2;

namespace StandaloneManagedConsumer;

internal static class Program
{
    private static readonly Action<Mat[], Mat, double, int> DenoiseTvl1 = PhotoCv2.DenoiseTvl1;
    private static readonly Action<Mat, Mat, Mat, Size, int, int> CorrectChromaticAberration = PhotoCv2.CorrectChromaticAberration;
    private static readonly Func<FileNode, OpenCvSharp.Photo.ChromaticAberrationParameters> LoadChromaticAberrationParams = PhotoCv2.LoadChromaticAberrationParams;
    private static readonly Func<OpenCvSharp.ML.ANN_MLP> CreateAnnMlp = OpenCvSharp.ML.ANN_MLP.Create;
    private static readonly Func<OpenCvSharp.ML.DTrees> CreateDTrees = OpenCvSharp.ML.DTrees.Create;
    private static readonly Func<OpenCvSharp.ML.RTrees> CreateRTrees = OpenCvSharp.ML.RTrees.Create;
    private static readonly Func<OpenCvSharp.ML.Boost> CreateBoost = OpenCvSharp.ML.Boost.Create;
    private static readonly Func<OpenCvSharp.ML.EM> CreateEM = OpenCvSharp.ML.EM.Create;
    private static readonly Func<OpenCvSharp.ML.LogisticRegression> CreateLogisticRegression = OpenCvSharp.ML.LogisticRegression.Create;
    private static readonly Func<OpenCvSharp.ML.SVMSGD> CreateSVMSGD = OpenCvSharp.ML.SVMSGD.Create;

    private static readonly Type[] RepresentativeTypes =
    {
        typeof(OpenCvException),
        typeof(OpenCvSharpBuildInfo),
        typeof(Mat),
        typeof(MatType),
        typeof(Point),
        typeof(Point2f),
        typeof(Rect),
        typeof(Scalar),
        typeof(CoreCv2),
        typeof(ImgProcCv2),
        typeof(ImgCodecsCv2),
        typeof(Features2DCv2),
        typeof(Calib3DCv2),
        typeof(DnnCv2),
        typeof(HighGuiCv2),
        typeof(PhotoCv2),
        typeof(VideoCv2),
        typeof(KeyPoint),
        typeof(DMatch),
        typeof(OpenCvSharp.ImgProc.LineSegment),
        typeof(OpenCvSharp.Calib3D.StereoBM),
        typeof(OpenCvSharp.Dnn.Net),
        typeof(OpenCvSharp.ObjDetect.QRCodeDetector),
        typeof(OpenCvSharp.Photo.AlignMTB),
        typeof(OpenCvSharp.Photo.CalibrateDebevec),
        typeof(OpenCvSharp.Photo.MergeMertens),
        typeof(OpenCvSharp.Photo.ColorCorrectionModel),
        typeof(OpenCvSharp.Photo.ChromaticAberrationParameters),
        typeof(OpenCvSharp.Photo.IntelligentScissorsMB),
        typeof(OpenCvSharp.Photo.CcmType),
        typeof(OpenCvSharp.Photo.InitialMethodType),
        typeof(OpenCvSharp.Photo.ColorCheckerType),
        typeof(OpenCvSharp.Photo.ColorSpace),
        typeof(OpenCvSharp.Photo.LinearizationType),
        typeof(OpenCvSharp.Photo.DistanceType),
        typeof(OpenCvSharp.Video.MotionType),
        typeof(OpenCvSharp.Video.ECCParameters),
        typeof(OpenCvSharp.Video.ECCRegistrationResult),
        typeof(OpenCvSharp.Video.Tracker),
        typeof(OpenCvSharp.Video.TrackerMIL),
        typeof(OpenCvSharp.Video.TrackerMILParams),
        typeof(OpenCvSharp.Tracking.Legacy.TrackerBoosting),
        typeof(OpenCvSharp.Tracking.Legacy.TrackerBoostingParams),
        typeof(OpenCvSharp.Tracking.Legacy.TrackerTLD),
        typeof(OpenCvSharp.Tracking.Legacy.TrackerKCF),
        typeof(OpenCvSharp.Tracking.Legacy.TrackerCSRT),
        typeof(OpenCvSharp.VideoIO.VideoCapture),
        typeof(OpenCvSharp.VideoIO.VideoWriter),
        typeof(OpenCvSharp.ML.SVM),
        typeof(OpenCvSharp.ML.ANN_MLP),
        typeof(OpenCvSharp.ML.DTrees),
        typeof(OpenCvSharp.ML.RTrees),
        typeof(OpenCvSharp.ML.Boost),
        typeof(OpenCvSharp.ML.DTreesPredictionFlags),
        typeof(OpenCvSharp.ML.BoostTypes),
        typeof(OpenCvSharp.ML.EM),
        typeof(OpenCvSharp.ML.EMCovarianceMatrixTypes),
        typeof(OpenCvSharp.ML.EMPredictionResult),
        typeof(OpenCvSharp.ML.LogisticRegression),
        typeof(OpenCvSharp.ML.LogisticRegressionRegularizationKinds),
        typeof(OpenCvSharp.ML.LogisticRegressionTrainingMethods),
        typeof(OpenCvSharp.ML.SVMSGD),
        typeof(OpenCvSharp.ML.SVMSGDTypes),
        typeof(OpenCvSharp.ML.SVMSGDMarginTypes),
        typeof(OpenCvSharp.Stitching.Stitcher),
        typeof(OpenCvSharp.Stitching.ExposureCompensatorType),
        typeof(OpenCvSharp.Stitching.ExposureCompensator),
        typeof(OpenCvSharp.Stitching.NoExposureCompensator),
        typeof(OpenCvSharp.Stitching.GainCompensator),
        typeof(OpenCvSharp.Stitching.ChannelsCompensator),
        typeof(OpenCvSharp.Stitching.BlocksCompensator),
        typeof(OpenCvSharp.Stitching.BlocksGainCompensator),
        typeof(OpenCvSharp.Stitching.BlocksChannelsCompensator),
        typeof(OpenCvSharp.Stitching.PyRotationWarper),
        typeof(OpenCvSharp.Geometry.DistanceTypes)
    };

    private static readonly object[] RepresentativeValues =
    {
        new Point(1, 2),
        new Point2f(1.5f, 2.5f),
        new Size(3, 4),
        new Rect(0, 1, 3, 4),
        new Scalar(1, 2, 3),
        new TermCriteria(TermCriteriaTypes.CountOrEps, 10, 0.01),
        new KeyPoint(1.0f, 2.0f, 3.0f),
        new DMatch(0, 1, 0.25f),
        default(OpenCvSharp.ImgCodecs.ImreadModes),
        default(OpenCvSharp.ImgProc.ColorConversionCodes),
        default(OpenCvSharp.ImgProc.ThresholdTypes),
        default(OpenCvSharp.Calib3D.SolvePnPFlags),
        default(OpenCvSharp.Dnn.DnnBackend),
        default(OpenCvSharp.Dnn.DnnTarget),
        default(OpenCvSharp.ObjDetect.PredefinedDictionaryType),
        default(OpenCvSharp.Photo.InpaintMethod),
        default(OpenCvSharp.Photo.CcmType),
        default(OpenCvSharp.Photo.InitialMethodType),
        default(OpenCvSharp.Photo.ColorCheckerType),
        default(OpenCvSharp.Photo.ColorSpace),
        default(OpenCvSharp.Photo.LinearizationType),
        default(OpenCvSharp.Photo.DistanceType),
        default(OpenCvSharp.Video.MotionType),
        new OpenCvSharp.Video.ECCParameters(),
        OpenCvSharp.Video.TrackerMILParams.Default,
        OpenCvSharp.Tracking.Legacy.TrackerBoostingParams.Default,
        OpenCvSharp.Tracking.TrackerKCFParams.Default,
        OpenCvSharp.Tracking.TrackerCSRTParams.Default,
        default(OpenCvSharp.Video.OpticalFlowFlags),
        default(OpenCvSharp.VideoIO.VideoCaptureAPIs),
        default(OpenCvSharp.ML.SVMTypes),
        default(OpenCvSharp.ML.ANN_MLPTrainingMethods),
        default(OpenCvSharp.ML.ANN_MLPActivationFunctions),
        default(OpenCvSharp.ML.ANN_MLPTrainFlags),
        default(OpenCvSharp.ML.DTreesPredictionFlags),
        default(OpenCvSharp.ML.BoostTypes),
        default(OpenCvSharp.ML.EMCovarianceMatrixTypes),
        default(OpenCvSharp.ML.EMPredictionResult),
        default(OpenCvSharp.ML.LogisticRegressionRegularizationKinds),
        default(OpenCvSharp.ML.LogisticRegressionTrainingMethods),
        default(OpenCvSharp.ML.SVMSGDTypes),
        default(OpenCvSharp.ML.SVMSGDMarginTypes),
        default(OpenCvSharp.Stitching.StitcherMode),
        default(OpenCvSharp.Stitching.ExposureCompensatorType),
        default(OpenCvSharp.Geometry.DistanceTypes)
    };

    private static int Main()
    {
        var message = OpenCvSharpBuildInfo.ManagedPackageId + ":" + OpenCvSharpBuildInfo.PackageVersion;
        var exceptionType = typeof(OpenCvException);
        return RepresentativeTypes.Length >= 70 &&
            RepresentativeValues.Length >= 42 &&
            DenoiseTvl1 != null && CorrectChromaticAberration != null && LoadChromaticAberrationParams != null &&
            CreateAnnMlp != null && CreateDTrees != null && CreateRTrees != null && CreateBoost != null && CreateEM != null &&
            CreateLogisticRegression != null && CreateSVMSGD != null &&
            exceptionType.Namespace == "OpenCvSharp" &&
            message.Length > 0
            ? 0
            : 1;
    }
}
'@
    [System.IO.File]::WriteAllText((Join-Path $ConsumerDirectory "Program.cs"), $programText)

    return $projectPath
}

$violations = [System.Collections.Generic.List[object]]::new()
$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-managed-standalone-consumer-" + [System.Guid]::NewGuid().ToString("N"))
$packageSourceDir = Join-Path $temporaryRoot "local-nuget-source"
$managedBuildOutputRoot = Join-Path $temporaryRoot "managed-build"
$packRestorePackagesDir = Join-Path $temporaryRoot "pack-restore-packages"
$consumerDir = Join-Path $temporaryRoot "consumer"
$consumerPackagesDir = Join-Path $temporaryRoot "consumer-packages"
$nugetHttpCacheDir = Join-Path $temporaryRoot "nuget-http-cache"
$nugetScratchDir = Join-Path $temporaryRoot "nuget-scratch"
$nugetPluginsCacheDir = Join-Path $temporaryRoot "nuget-plugin-cache"
$nugetConfigPath = Join-Path $temporaryRoot "NuGet.config"

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
            $packageSourceDir,
            $managedBuildOutputRoot,
            $packRestorePackagesDir,
            $consumerPackagesDir,
            $nugetHttpCacheDir,
            $nugetScratchDir,
            $nugetPluginsCacheDir)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $consumerProjectPath = New-TemporaryConsumerProject -ConsumerDirectory $consumerDir

    $env:NUGET_PACKAGES = $consumerPackagesDir
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
        "-TargetFrameworks", $targetFramework,
        "-BuildOutputRoot", $managedBuildOutputRoot,
        "-RestorePackagesPath", $packRestorePackagesDir
    )

    $managedPackOutput = & $pwsh.Source @managedPackArguments 2>&1
    $managedPackOutputText = ($managedPackOutput | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine
    if ($LASTEXITCODE -ne 0) {
        Add-Violation -Violations $violations -Path "scripts/Pack-Managed.ps1" -Issue "Managed package generation failed" -Text $managedPackOutputText
    }

    $managedPackagePath = Join-Path $packageSourceDir $managedPackageFileName
    Assert-FileExists -Violations $violations -Path $managedPackagePath -Issue "Managed package was not created in the temporary local NuGet source"
    Assert-ManagedPackageSurface -Violations $violations -PackagePath $managedPackagePath

    $packProjectAssetsPath = Join-Path $managedBuildOutputRoot "obj/project.assets.json"
    Assert-FileExists -Violations $violations -Path $packProjectAssetsPath -Issue "Pack-Managed did not write restore assets under the explicit temporary build output root"
    if (Test-Path -LiteralPath $packProjectAssetsPath -PathType Leaf) {
        $packAssetsText = [System.IO.File]::ReadAllText($packProjectAssetsPath)
        $packAssetsTextNormalized = $packAssetsText.Replace("\\", "/").Replace("\", "/")
        $packRestorePackagesDirNormalized = [System.IO.Path]::GetFullPath($packRestorePackagesDir).Replace("\", "/")
        if ($packAssetsTextNormalized.IndexOf($packRestorePackagesDirNormalized, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            Add-Violation -Violations $violations -Path $packProjectAssetsPath -Issue "Pack-Managed did not restore through the explicit temporary RestorePackagesPath" -Text $packRestorePackagesDirNormalized
        }
    }

    $nugetConfigText = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="standalone-managed-local" value="$packageSourceDir" />
  </packageSources>
</configuration>
"@
    [System.IO.File]::WriteAllText($nugetConfigPath, $nugetConfigText)

    $restoreArguments = @(
        "restore",
        $consumerProjectPath,
        "--configfile", $nugetConfigPath,
        "--packages", $consumerPackagesDir,
        "--no-cache",
        "-v:minimal"
    )
    $restoreOutput = & $dotnet.Source @restoreArguments 2>&1
    $restoreOutputText = ($restoreOutput | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine
    if ($LASTEXITCODE -ne 0) {
        Add-Violation -Violations $violations -Path "consumer/StandaloneManagedConsumer.csproj" -Issue "Temporary standalone managed consumer restore failed" -Text $restoreOutputText
    }

    $buildArguments = @(
        "build",
        $consumerProjectPath,
        "-c", "Release",
        "--no-restore",
        "-p:RestorePackagesPath=$consumerPackagesDir",
        "-v:minimal"
    )
    $buildOutput = & $dotnet.Source @buildArguments 2>&1
    $buildOutputText = ($buildOutput | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine
    if ($LASTEXITCODE -ne 0) {
        Add-Violation -Violations $violations -Path "consumer/StandaloneManagedConsumer.csproj" -Issue "Temporary standalone managed consumer build failed" -Text $buildOutputText
    }

    $consumerProjectText = [System.IO.File]::ReadAllText($consumerProjectPath)
    Assert-TextContains -Violations $violations -Path $consumerProjectPath -Text $consumerProjectText -Needle "PackageReference Include=`"$managedPackageId`" Version=`"$packageVersion`"" -Issue "Temporary standalone consumer must reference only the neutral managed package at four-part package version metadata"
    Assert-TextDoesNotContain -Violations $violations -Path $consumerProjectPath -Text $consumerProjectText -Needle $runtimePackagePrefix -Issue "Temporary standalone consumer must not reference a runtime package for compile-only usage"
    if ($consumerProjectText -match $fixedMajorConsumerPattern) {
        Add-Violation -Violations $violations -Path $consumerProjectPath -Issue "Temporary standalone consumer must not contain fixed-major package identity, assembly name, or package reference"
    }

    $consumerProgramPath = Join-Path $consumerDir "Program.cs"
    Assert-FileExists -Violations $violations -Path $consumerProgramPath -Issue "Temporary standalone consumer representative source was not created"
    if (Test-Path -LiteralPath $consumerProgramPath -PathType Leaf) {
        $consumerProgramText = [System.IO.File]::ReadAllText($consumerProgramPath)
        foreach ($needle in $representativeSourceNeedles) {
            Assert-TextContains -Violations $violations -Path $consumerProgramPath -Text $consumerProgramText -Needle $needle -Issue "Representative consumer source must reference selected OpenCvSharp module namespaces"
        }

        Assert-TextDoesNotContain -Violations $violations -Path $consumerProgramPath -Text $consumerProgramText -Needle $runtimePackagePrefix -Issue "Representative consumer source must not reference a runtime package for compile-only usage"
        if ($consumerProgramText -match $fixedMajorSourcePattern) {
            Add-Violation -Violations $violations -Path $consumerProgramPath -Issue "Representative consumer source must not contain fixed-major namespaces, package IDs, or assembly names" -Text $Matches[0]
        }
    }

    $assetsPath = Join-Path $consumerDir "obj/project.assets.json"
    Assert-FileExists -Violations $violations -Path $assetsPath -Issue "Temporary standalone managed consumer restore did not create project.assets.json"
    if (Test-Path -LiteralPath $assetsPath -PathType Leaf) {
        $assetsText = [System.IO.File]::ReadAllText($assetsPath)
        Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle "$managedPackageId/$normalizedPackageVersion" -Issue "Consumer assets file must reference the neutral managed package"
        Assert-TextContains -Violations $violations -Path $assetsPath -Text $assetsText -Needle "lib/$targetFramework/$managedAssemblyFileName" -Issue "Consumer assets file must include the managed compile/runtime assembly"
        Assert-TextDoesNotContain -Violations $violations -Path $assetsPath -Text $assetsText -Needle $runtimePackagePrefix -Issue "Standalone managed consumer assets must not reference a runtime package"
        Assert-TextDoesNotContain -Violations $violations -Path $assetsPath -Text $assetsText -Needle '"runtimeTargets"' -Issue "Standalone managed consumer assets must not require runtimeTargets for compile-only usage"
        Assert-TextDoesNotContain -Violations $violations -Path $assetsPath -Text $assetsText -Needle "runtimes/" -Issue "Standalone managed consumer assets must not select native runtime assets"
        if ($assetsText -match $fixedMajorConsumerPattern) {
            Add-Violation -Violations $violations -Path $assetsPath -Issue "Consumer assets file must not contain fixed-major package identity, assembly name, or package reference"
        }
    }

    $managedPackageInstallRoot = Join-Path $consumerPackagesDir "$managedPackageIdLower/$normalizedPackageVersion"
    Assert-FileExists -Violations $violations -Path (Join-Path $managedPackageInstallRoot "lib/$targetFramework/$managedAssemblyFileName") -Issue "Isolated NuGet package cache did not contain the managed compile/runtime assembly"

    $packageCacheNuspecPath = Join-Path $managedPackageInstallRoot "$managedPackageIdLower.nuspec"
    $packageCacheOriginalCaseNuspecPath = Join-Path $managedPackageInstallRoot "$managedPackageId.nuspec"
    if (-not (Test-Path -LiteralPath $packageCacheNuspecPath -PathType Leaf) -and
        (Test-Path -LiteralPath $packageCacheOriginalCaseNuspecPath -PathType Leaf)) {
        $packageCacheNuspecPath = $packageCacheOriginalCaseNuspecPath
    }

    Assert-FileExists -Violations $violations -Path $packageCacheNuspecPath -Issue "Isolated NuGet package cache did not contain the managed nuspec"
    if (Test-Path -LiteralPath $packageCacheNuspecPath -PathType Leaf) {
        $packageCacheNuspecText = [System.IO.File]::ReadAllText($packageCacheNuspecPath)
        Assert-TextContains -Violations $violations -Path $packageCacheNuspecPath -Text $packageCacheNuspecText -Needle "<id>$managedPackageId</id>" -Issue "Package cache nuspec must keep the neutral managed package ID"
        Assert-TextContains -Violations $violations -Path $packageCacheNuspecPath -Text $packageCacheNuspecText -Needle "<version>$normalizedPackageVersion</version>" -Issue "Package cache nuspec must keep normalized package version metadata"
        if ($packageCacheNuspecText -match $fixedMajorConsumerPattern) {
            Add-Violation -Violations $violations -Path $packageCacheNuspecPath -Issue "Package cache nuspec must not contain fixed-major package identity, assembly name, or package reference"
        }
    }

    $consumerOutputDir = Join-Path $consumerDir "bin/Release/$targetFramework"
    Assert-FileExists -Violations $violations -Path (Join-Path $consumerOutputDir $managedAssemblyFileName) -Issue "Temporary standalone consumer build output did not copy managed package assembly"

    if (Test-Path -LiteralPath $consumerOutputDir -PathType Container) {
        $nativeOutputFiles = @(
            Get-ChildItem -LiteralPath $consumerOutputDir -Recurse -File |
                Where-Object {
                    $_.Name -eq $primaryNativeLoader -or
                    $_.Name -eq $compatibilityNativeLoader -or
                    $_.Name -match '^opencv_.*\.dll$'
                } |
                ForEach-Object { $_.FullName }
        )

        if ($nativeOutputFiles.Count -gt 0) {
            Add-Violation -Violations $violations -Path $consumerOutputDir -Issue "Standalone managed consumer build output must not copy native runtime assets" -Text ($nativeOutputFiles -join "; ")
        }
    }

    foreach ($outputDirectory in @(
            $packageSourceDir,
            $managedBuildOutputRoot,
            $packRestorePackagesDir,
            $consumerDir,
            $consumerPackagesDir,
            $nugetHttpCacheDir,
            $nugetScratchDir,
            $nugetPluginsCacheDir)) {
        if ((Test-Path -LiteralPath $outputDirectory -PathType Container) -and
            -not (Test-IsPathUnder -Path $outputDirectory -Root $temporaryRoot)) {
            Add-Violation -Violations $violations -Path $outputDirectory -Issue "Standalone managed consumer dry-run output escaped the temporary root"
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
            Add-Violation -Violations $violations -Path $directory -Issue "Standalone managed consumer dry-run unexpectedly created a repository output directory"
            Remove-DirectoryIfSafe -Path $directory -AllowedRoot $repo
        }
    }

    Remove-DirectoryIfSafe -Path $temporaryRoot -AllowedRoot ([System.IO.Path]::GetTempPath())
}

if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
    Add-Violation -Violations $violations -Path $temporaryRoot -Issue "Temporary standalone managed consumer output was not cleaned"
}

foreach ($directory in $repoSensitiveDirectories) {
    if (-not $preexistingSensitiveDirectories[$directory] -and (Test-Path -LiteralPath $directory -PathType Container)) {
        Add-Violation -Violations $violations -Path $directory -Issue "Repository output directory remains after standalone managed consumer dry-run cleanup"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Managed package representative API compile surface guard failed with $($violations.Count) violation(s)."
    foreach ($violation in ($violations | Sort-Object Path, Issue)) {
        Write-Host "Path: $($violation.Path)"
        Write-Host "Issue: $($violation.Issue)"
        if (-not [string]::IsNullOrWhiteSpace($violation.Text)) {
            Write-Host "Text: $($violation.Text)"
        }
    }

    exit 1
}

Write-Host "Managed package representative API compile surface guard passed."
Write-Host "Temporary consumer referenced only $managedPackageId at $packageVersion."
Write-Host "Representative managed API surface compiled without native runtime package assets."
