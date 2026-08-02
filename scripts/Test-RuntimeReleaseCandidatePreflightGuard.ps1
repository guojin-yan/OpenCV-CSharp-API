param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
if ($null -eq $pwsh) {
    throw "pwsh was not found. Runtime release-candidate preflight validation requires PowerShell 7+."
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnet) {
    throw "dotnet was not found. Runtime release-candidate pack integration validation requires dotnet pack."
}

$stageRuntimePath = Join-Path $repo "scripts/Stage-Runtime.ps1"
$packRuntimePath = Join-Path $repo "scripts/Pack-Runtime.ps1"
$preflightPath = Join-Path $repo "scripts/Test-RuntimeReleaseCandidatePreflight.ps1"
foreach ($scriptPath in @($stageRuntimePath, $packRuntimePath, $preflightPath)) {
    if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf)) {
        throw "Required runtime release preflight script was not found: $scriptPath"
    }
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
    param([Parameter(Mandatory = $true)][string]$Path)

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

function Invoke-ChildPwsh {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)

    $output = & $pwsh.Source @Arguments 2>&1
    return [pscustomobject]@{
        ExitCode = $LASTEXITCODE
        Output = (($output | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine)
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
    <RuntimePackageProfile Condition="'$(RuntimePackageProfile)' == ''">full</RuntimePackageProfile>
    <RuntimePackageProfileSuffix Condition="'$(RuntimePackageProfile)' == 'mini'">.mini</RuntimePackageProfileSuffix>
    <PackageId>JYPPX.OpenCV.runtime.$(RuntimePackageRid)$(RuntimePackageProfileSuffix)</PackageId>
    <Version>5.0.0.0</Version>
    <PackageVersion>5.0.0.0</PackageVersion>
    <Authors>synthetic</Authors>
    <Description>Synthetic native runtime package for release preflight pack integration validation.</Description>
    <PackageLicenseExpression>MIT AND Apache-2.0</PackageLicenseExpression>
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

$violations = [System.Collections.Generic.List[object]]::new()
$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-runtime-release-preflight-" + [System.Guid]::NewGuid().ToString("N"))
$nativeWrapperRuntimeDir = Join-Path $temporaryRoot "native-wrapper-runtime"
$openCvRuntimeDir = Join-Path $temporaryRoot "opencv-runtime"
$openCvSourceDir = Join-Path $temporaryRoot "opencv-source"
$openCvInstallDir = Join-Path $temporaryRoot "opencv-install"
$outputRoot = Join-Path $temporaryRoot "staging-output"
$runtimeProjectDir = Join-Path $temporaryRoot "runtime-package-project"
$miniOutputRoot = Join-Path $temporaryRoot "mini-staging-output"
$miniRuntimeProjectDir = Join-Path $temporaryRoot "mini-runtime-package-project"
$releasePackStageOutputRoot = Join-Path $temporaryRoot "release-pack-stage-output"
$releasePackOutputDir = Join-Path $temporaryRoot "release-package-output"
$releasePackProjectDir = Join-Path $temporaryRoot "release-pack-runtime-package-project"
$negativePackProjectDir = Join-Path $temporaryRoot "negative-pack-runtime-package-project"
$negativePackOutputDir = Join-Path $temporaryRoot "negative-package-output"
$negativePackStageOutputRoot = Join-Path $temporaryRoot "negative-pack-stage-output"
$rid = "win-x64"
$runtimeProfile = "full"
$packageId = "JYPPX.OpenCV.runtime.win-x64"
$miniPackageId = "JYPPX.OpenCV.runtime.win-x64.mini"
$packageFileName = "$packageId.5.0.0.nupkg"
$packageVersion = "5.0.0.0"
$openCvVersion = "5.0.0"
$openCvBinarySuffix = "500"
$primaryNativeLoader = "JYPPX.OpenCV.Native.dll"
$compatibilityNativeLoader = "OpenCv5Sharp.Native.dll" # compatibility loader copy for already-compiled consumers

$matrix = Get-Content -LiteralPath (Join-Path $repo "packaging/runtime/runtime-package-matrix.json") -Raw | ConvertFrom-Json
$profileSpec = @($matrix.profiles | Where-Object { $_.name -eq $runtimeProfile } | Select-Object -First 1)
$requiredOpenCvModules = @($profileSpec[0].modules)

try {
    foreach ($directory in @(
            $nativeWrapperRuntimeDir,
            $openCvRuntimeDir,
            (Join-Path $openCvSourceDir "3rdparty/ippicv"),
            (Join-Path $openCvInstallDir "etc/licenses"),
            $outputRoot,
            $miniOutputRoot,
            $releasePackStageOutputRoot,
            $releasePackOutputDir,
            $negativePackOutputDir,
            $negativePackStageOutputRoot,
            $runtimeProjectDir,
            $miniRuntimeProjectDir)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    foreach ($dllName in @($primaryNativeLoader, $compatibilityNativeLoader)) {
        Write-SyntheticDll -Path (Join-Path $nativeWrapperRuntimeDir $dllName)
    }

    foreach ($module in $requiredOpenCvModules) {
        Write-SyntheticDll -Path (Join-Path $openCvRuntimeDir "opencv_$module$openCvBinarySuffix.dll")
    }

    [System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "LICENSE"), "Synthetic OpenCV source license")
    [System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "3rdparty/ippicv/readme.htm"), "Synthetic IPPICV license")
    [System.IO.File]::WriteAllText((Join-Path $openCvInstallDir "etc/licenses/synthetic-3rdparty.txt"), "Synthetic third-party license")

    $stageResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $stageRuntimePath,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-Configuration", "Release",
        "-OpenCvNativeRuntimeDir", $nativeWrapperRuntimeDir,
        "-OpenCvRuntimeDir", $openCvRuntimeDir,
        "-OpenCvSourceDir", $openCvSourceDir,
        "-OpenCvInstallDir", $openCvInstallDir,
        "-OutputRoot", $outputRoot,
        "-RuntimeProject", $runtimeProjectDir,
        "-RuntimePackageId", $packageId,
        "-PackageVersion", $packageVersion
    )
    if ($stageResult.ExitCode -ne 0) {
        Add-Violation -Violations $violations -Path $stageRuntimePath -Issue "Release-shaped staging fixture failed" -Text $stageResult.Output
    }

    $preflightResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $preflightPath,
        "-RepositoryRoot", $repo,
        "-RuntimeProject", $runtimeProjectDir,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-RuntimePackageId", $packageId,
        "-PackageVersion", $packageVersion,
        "-OpenCvVersion", $openCvVersion
    )
    if ($preflightResult.ExitCode -ne 0) {
        Add-Violation -Violations $violations -Path $preflightPath -Issue "Release-shaped runtime preflight should pass" -Text $preflightResult.Output
    }

    $miniStageResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $stageRuntimePath,
        "-Rid", $rid,
        "-RuntimeProfile", "mini",
        "-Configuration", "Release",
        "-OpenCvNativeRuntimeDir", $nativeWrapperRuntimeDir,
        "-OpenCvRuntimeDir", $openCvRuntimeDir,
        "-OpenCvSourceDir", $openCvSourceDir,
        "-OpenCvInstallDir", $openCvInstallDir,
        "-OutputRoot", $miniOutputRoot,
        "-RuntimeProject", $miniRuntimeProjectDir,
        "-RuntimePackageId", $miniPackageId,
        "-PackageVersion", $packageVersion
    )
    if ($miniStageResult.ExitCode -ne 0) {
        Add-Violation -Violations $violations -Path $stageRuntimePath -Issue "Mini release-shaped staging fixture failed" -Text $miniStageResult.Output
    }

    $miniPreflightResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $preflightPath,
        "-RepositoryRoot", $repo,
        "-RuntimeProject", $miniRuntimeProjectDir,
        "-Rid", $rid,
        "-RuntimeProfile", "mini",
        "-RuntimePackageId", $miniPackageId,
        "-PackageVersion", $packageVersion,
        "-OpenCvVersion", $openCvVersion
    )
    if ($miniPreflightResult.ExitCode -ne 0) {
        Add-Violation -Violations $violations -Path $preflightPath -Issue "Mini runtime preflight should accept empty optional-module collections" -Text $miniPreflightResult.Output
    }

    $staleNativeFile = Join-Path $runtimeProjectDir "runtimes/$rid/native/stale-opencv-file.dll"
    Write-SyntheticDll -Path $staleNativeFile
    $staleResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $preflightPath,
        "-RepositoryRoot", $repo,
        "-RuntimeProject", $runtimeProjectDir,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-RuntimePackageId", $packageId,
        "-PackageVersion", $packageVersion,
        "-OpenCvVersion", $openCvVersion
    )
    if ($staleResult.ExitCode -eq 0) {
        Add-Violation -Violations $violations -Path $preflightPath -Issue "Runtime preflight must reject stale native mirror files" -Text $staleResult.Output
    }
    Remove-Item -LiteralPath $staleNativeFile -Force

    $releasePackProjectPath = New-TemporaryRuntimeProject -RuntimeProjectDirectory $releasePackProjectDir
    $releasePackResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $packRuntimePath,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-Configuration", "Release",
        "-OpenCvVersion", $openCvVersion,
        "-PackageRevision", "0",
        "-StageRuntime",
        "-RequireReleasePreflight",
        "-OpenCvNativeRuntimeDir", $nativeWrapperRuntimeDir,
        "-OpenCvRuntimeDir", $openCvRuntimeDir,
        "-OpenCvSourceDir", $openCvSourceDir,
        "-OpenCvInstallDir", $openCvInstallDir,
        "-StageOutputRoot", $releasePackStageOutputRoot,
        "-OutputDir", $releasePackOutputDir,
        "-RuntimeProject", $releasePackProjectPath
    )
    if ($releasePackResult.ExitCode -ne 0) {
        Add-Violation -Violations $violations -Path $packRuntimePath -Issue "Pack-Runtime -RequireReleasePreflight integration should pass for release-shaped staged inputs" -Text $releasePackResult.Output
    }

    $releasePackagePath = Join-Path $releasePackOutputDir $packageFileName
    Assert-FileExists -Violations $violations -Path $releasePackagePath -Issue "Pack-Runtime -RequireReleasePreflight integration did not produce the expected runtime package"
    if (Test-Path -LiteralPath $releasePackagePath -PathType Leaf) {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [System.IO.Compression.ZipFile]::OpenRead($releasePackagePath)
        try {
            $entryNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
            foreach ($entry in $zip.Entries) {
                [void]$entryNames.Add($entry.FullName)
            }

            Assert-ZipEntry -Violations $violations -Entries $entryNames -Entry "$packageId.nuspec" -PackagePath $releasePackagePath -Issue "Release-preflight pack integration package did not contain the expected neutral nuspec"
            Assert-ZipEntry -Violations $violations -Entries $entryNames -Entry "build/JYPPX.OpenCV.runtime.provenance.json" -PackagePath $releasePackagePath -Issue "Release-preflight pack integration package did not contain provenance manifest"
            Assert-ZipEntry -Violations $violations -Entries $entryNames -Entry "runtimes/$rid/native/$primaryNativeLoader" -PackagePath $releasePackagePath -Issue "Release-preflight pack integration package did not contain the primary native loader"
            Assert-ZipEntry -Violations $violations -Entries $entryNames -Entry "runtimes/$rid/native/$compatibilityNativeLoader" -PackagePath $releasePackagePath -Issue "Release-preflight pack integration package did not contain the compatibility native loader"
            foreach ($licenseEntry in @(
                    "licenses/LICENSE",
                    "licenses/readme.htm",
                    "licenses/opencv-3rdparty/synthetic-3rdparty.txt")) {
                Assert-ZipEntry -Violations $violations -Entries $entryNames -Entry $licenseEntry -PackagePath $releasePackagePath -Issue "Release-preflight pack integration package did not contain staged license file"
            }

            $manifestEntry = $zip.GetEntry("build/JYPPX.OpenCV.runtime.provenance.json")
            if ($null -ne $manifestEntry) {
                $reader = [System.IO.StreamReader]::new($manifestEntry.Open())
                try {
                    $manifest = $reader.ReadToEnd() | ConvertFrom-Json
                }
                finally {
                    $reader.Dispose()
                }

                if ([bool]$manifest.SyntheticRuntimeInputs) {
                    Add-Violation -Violations $violations -Path $releasePackagePath -Issue "Release-preflight pack integration provenance must be non-synthetic"
                }

                if ($manifest.PackageId -ne $packageId -or $manifest.PackageVersion -ne $packageVersion) {
                    Add-Violation -Violations $violations -Path $releasePackagePath -Issue "Release-preflight pack integration provenance must match package ID/version" -Text "$($manifest.PackageId) / $($manifest.PackageVersion)"
                }
            }
        }
        finally {
            $zip.Dispose()
        }
    }

    $negativePackProjectPath = New-TemporaryRuntimeProject -RuntimeProjectDirectory $negativePackProjectDir
    $negativePackResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $packRuntimePath,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-Configuration", "Release",
        "-OpenCvVersion", $openCvVersion,
        "-PackageRevision", "0",
        "-StageRuntime",
        "-SyntheticRuntimeInputs",
        "-RequireReleasePreflight",
        "-OpenCvNativeRuntimeDir", $nativeWrapperRuntimeDir,
        "-OpenCvRuntimeDir", $openCvRuntimeDir,
        "-OpenCvSourceDir", $openCvSourceDir,
        "-OpenCvInstallDir", $openCvInstallDir,
        "-StageOutputRoot", $negativePackStageOutputRoot,
        "-OutputDir", $negativePackOutputDir,
        "-RuntimeProject", $negativePackProjectPath
    )
    if ($negativePackResult.ExitCode -eq 0) {
        Add-Violation -Violations $violations -Path $packRuntimePath -Issue "Pack-Runtime -RequireReleasePreflight must reject synthetic runtime inputs before package creation" -Text $negativePackResult.Output
    }

    if (Test-Path -LiteralPath (Join-Path $negativePackOutputDir $packageFileName) -PathType Leaf) {
        Add-Violation -Violations $violations -Path $negativePackOutputDir -Issue "Synthetic release-preflight negative path must not produce a runtime package"
    }

    $syntheticStageResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $stageRuntimePath,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-Configuration", "Release",
        "-OpenCvNativeRuntimeDir", $nativeWrapperRuntimeDir,
        "-OpenCvRuntimeDir", $openCvRuntimeDir,
        "-OpenCvSourceDir", $openCvSourceDir,
        "-OpenCvInstallDir", $openCvInstallDir,
        "-OutputRoot", $outputRoot,
        "-RuntimeProject", $runtimeProjectDir,
        "-RuntimePackageId", $packageId,
        "-PackageVersion", $packageVersion,
        "-SyntheticRuntimeInputs"
    )
    if ($syntheticStageResult.ExitCode -ne 0) {
        Add-Violation -Violations $violations -Path $stageRuntimePath -Issue "Synthetic staging fixture failed" -Text $syntheticStageResult.Output
    }

    $syntheticPreflightResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $preflightPath,
        "-RepositoryRoot", $repo,
        "-RuntimeProject", $runtimeProjectDir,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-RuntimePackageId", $packageId,
        "-PackageVersion", $packageVersion,
        "-OpenCvVersion", $openCvVersion
    )
    if ($syntheticPreflightResult.ExitCode -eq 0) {
        Add-Violation -Violations $violations -Path $preflightPath -Issue "Runtime preflight must reject synthetic runtime inputs by default" -Text $syntheticPreflightResult.Output
    }

    $allowSyntheticPreflightResult = Invoke-ChildPwsh -Arguments @(
        "-NoProfile",
        "-File", $preflightPath,
        "-RepositoryRoot", $repo,
        "-RuntimeProject", $runtimeProjectDir,
        "-Rid", $rid,
        "-RuntimeProfile", $runtimeProfile,
        "-RuntimePackageId", $packageId,
        "-PackageVersion", $packageVersion,
        "-OpenCvVersion", $openCvVersion,
        "-AllowSyntheticRuntimeInputs"
    )
    if ($allowSyntheticPreflightResult.ExitCode -ne 0) {
        Add-Violation -Violations $violations -Path $preflightPath -Issue "Runtime preflight should allow synthetic inputs only when explicitly requested" -Text $allowSyntheticPreflightResult.Output
    }
}
finally {
    Remove-DirectoryIfSafe -Path $temporaryRoot -AllowedRoot ([System.IO.Path]::GetTempPath())
}

if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
    Add-Violation -Violations $violations -Path $temporaryRoot -Issue "Temporary runtime release-candidate preflight fixture was not cleaned"
}

if ($violations.Count -gt 0) {
    Write-Host "Runtime release-candidate preflight guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-List Path, Issue, Text
    exit 1
}

Write-Host "Runtime release-candidate preflight guard passed."
Write-Host "Release-shaped manifests pass; Pack-Runtime -RequireReleasePreflight produces a package only for non-synthetic staged inputs."
Write-Host "Mini release-shaped manifests accept empty optional-module collections."
Write-Host "Synthetic manifests and stale mirrors are rejected by default."
