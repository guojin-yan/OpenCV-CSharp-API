param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
if ($null -eq $pwsh) {
    throw "pwsh was not found. Real native runtime build matrix coverage validation requires PowerShell 7+."
}

$buildOpenCvPath = Join-Path $repo "scripts/Build-OpenCV.ps1"
$stageRuntimePath = Join-Path $repo "scripts/Stage-Runtime.ps1"
$runtimeMatrixPath = Join-Path $repo "packaging/runtime/runtime-package-matrix.json"

foreach ($requiredFile in @($buildOpenCvPath, $stageRuntimePath, $runtimeMatrixPath)) {
    if (-not (Test-Path -LiteralPath $requiredFile -PathType Leaf)) {
        throw "Required real native runtime build matrix file was not found: $requiredFile"
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

function Test-ContainsText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Needle
    )

    return $Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
}

function ConvertTo-NormalizedPathText {
    param([AllowNull()][string]$Text)
    if ($null -eq $Text) {
        return ""
    }

    return $Text.Replace("\", "/")
}

function Test-SequenceContains {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Values,
        [Parameter(Mandatory = $true)]
        [string]$Needle
    )

    return @($Values | Where-Object { [string]$_ -eq $Needle }).Count -gt 0
}

function Write-SyntheticBinary {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Kind
    )

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
    if ($Kind -eq "windows") {
        [System.IO.File]::WriteAllBytes($Path, [byte[]](0x4D, 0x5A, 0x00, 0x00))
        return
    }

    [System.IO.File]::WriteAllBytes($Path, [byte[]](0x7F, 0x45, 0x4C, 0x46))
}

function Remove-DirectoryIfPresent {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (Test-Path -LiteralPath $Path -PathType Container) {
        Remove-Item -LiteralPath $Path -Recurse -Force
    }
}

function Get-AndroidAbi {
    param([Parameter(Mandatory = $true)][string]$Rid)

    switch ($Rid) {
        "android-arm64" { return "arm64-v8a" }
        "android-arm" { return "armeabi-v7a" }
        "android-x64" { return "x86_64" }
        "android-x86" { return "x86" }
        default { return "" }
    }
}

function Get-WindowsPlatform {
    param([Parameter(Mandatory = $true)][string]$Rid)

    switch ($Rid) {
        "win-x64" { return "x64" }
        "win-x86" { return "Win32" }
        "win-arm64" { return "ARM64" }
        default { return "" }
    }
}

function Get-WindowsArchFolder {
    param([Parameter(Mandatory = $true)][string]$Rid)

    switch ($Rid) {
        "win-x64" { return "x64" }
        "win-x86" { return "x86" }
        "win-arm64" { return "ARM64" }
        default { return "" }
    }
}

function Get-PlatformFamily {
    param([Parameter(Mandatory = $true)][object]$RidSpec)

    $rid = [string]$RidSpec.rid
    if ($null -ne $RidSpec.PSObject.Properties["platformFamily"] -and
        -not [string]::IsNullOrWhiteSpace([string]$RidSpec.platformFamily)) {
        return ([string]$RidSpec.platformFamily).ToLowerInvariant()
    }

    if ($rid.StartsWith("win-", [System.StringComparison]::OrdinalIgnoreCase)) {
        return "windows"
    }

    if ($rid.StartsWith("linux-", [System.StringComparison]::OrdinalIgnoreCase)) {
        return "linux"
    }

    if ($rid.StartsWith("android-", [System.StringComparison]::OrdinalIgnoreCase)) {
        return "android"
    }

    return ""
}

function Invoke-BuildPlan {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Rid,
        [Parameter(Mandatory = $true)]
        [string]$BuildList
    )

    $arguments = @(
        "-NoProfile",
        "-File", $buildOpenCvPath,
        "-Rid", $Rid,
        "-BuildList", $BuildList,
        "-DescribeOnly"
    )

    $output = & $pwsh.Source @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Build-OpenCV -DescribeOnly failed for RID '$Rid'."
    }

    return (($output | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine) | ConvertFrom-Json
}

function Invoke-StageCase {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Rid,
        [Parameter(Mandatory = $true)]
        [string]$OpenCvRid,
        [Parameter(Mandatory = $true)]
        [string]$PlatformFamily,
        [Parameter(Mandatory = $true)]
        [string]$RuntimeSubdir,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations
    )

    $temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-real-runtime-stage-" + [System.Guid]::NewGuid().ToString("N"))
    try {
        $nativeRuntimeDir = Join-Path $temporaryRoot "native-wrapper"
        $sourceRoot = Join-Path $temporaryRoot "opencv-source"
        $openCvSourceDir = Join-Path $sourceRoot "opencv-5.0.0"
        $installRoot = Join-Path $temporaryRoot "opencv-install"
        $installDir = Join-Path $installRoot "opencv-5.0.0-$OpenCvRid"
        $runtimeDir = Join-Path $installDir $RuntimeSubdir
        $outputRoot = Join-Path $temporaryRoot "stage-output"
        $runtimeProject = Join-Path $temporaryRoot "runtime-project"
        $modules = @("core", "imgproc", "imgcodecs", "videoio", "geometry", "flann")

        New-Item -ItemType Directory -Force -Path `
            $nativeRuntimeDir,
            $runtimeDir,
            (Join-Path $openCvSourceDir "3rdparty/ippicv"),
            (Join-Path $installDir "etc/licenses"),
            $outputRoot,
            $runtimeProject | Out-Null

        $compatibilityNativeLoaderBaseName = "Open" + "Cv5Sharp.Native" # compatibility loader for already-compiled consumers
        if ($PlatformFamily -eq "windows") {
            foreach ($loaderName in @("JYPPX.OpenCV.Native.dll", "$compatibilityNativeLoaderBaseName.dll")) {
                Write-SyntheticBinary -Path (Join-Path $nativeRuntimeDir $loaderName) -Kind "windows"
            }

            foreach ($module in $modules) {
                Write-SyntheticBinary -Path (Join-Path $runtimeDir "opencv_$module`500.dll") -Kind "windows"
            }
        }
        elseif ($PlatformFamily -eq "linux") {
            foreach ($loaderName in @("libJYPPX.OpenCV.Native.so", "lib$compatibilityNativeLoaderBaseName.so")) {
                Write-SyntheticBinary -Path (Join-Path $nativeRuntimeDir $loaderName) -Kind "elf"
            }

            foreach ($module in $modules) {
                foreach ($suffix in @(".so", ".so.5.0.0", ".so.500")) {
                    Write-SyntheticBinary -Path (Join-Path $runtimeDir "libopencv_$module$suffix") -Kind "elf"
                }
            }
        }
        else {
            foreach ($loaderName in @("libJYPPX.OpenCV.Native.so", "lib$compatibilityNativeLoaderBaseName.so")) {
                Write-SyntheticBinary -Path (Join-Path $nativeRuntimeDir $loaderName) -Kind "elf"
            }

            foreach ($module in $modules) {
                Write-SyntheticBinary -Path (Join-Path $runtimeDir "libopencv_$module.so") -Kind "elf"
            }
        }

        [System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "LICENSE"), "Synthetic OpenCV source license")
        [System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "3rdparty/ippicv/readme.htm"), "Synthetic IPPICV license")
        [System.IO.File]::WriteAllText((Join-Path $installDir "etc/licenses/synthetic-real-runtime-build-plan.txt"), "Synthetic third-party license")

        $stageArguments = @(
            "-NoProfile",
            "-File", $stageRuntimePath,
            "-Rid", $Rid,
            "-RuntimeProfile", "mini",
            "-OpenCvNativeRuntimeDir", $nativeRuntimeDir,
            "-OpenCvInstallRoot", $installRoot,
            "-OpenCvSourceRoot", $sourceRoot,
            "-OutputRoot", $outputRoot,
            "-RuntimeProject", $runtimeProject,
            "-SyntheticRuntimeInputs"
        )

        $stageOutput = & $pwsh.Source @stageArguments 2>&1
        $stageOutputText = ($stageOutput | ForEach-Object { $_.ToString() }) -join [System.Environment]::NewLine
        if ($LASTEXITCODE -ne 0) {
            Add-Violation -Violations $Violations -Path "scripts/Stage-Runtime.ps1" -Issue "Stage-Runtime RID-aware default input resolution failed" -Text "$Rid :: $stageOutputText"
            return
        }

        $manifestPath = Join-Path $runtimeProject "build/JYPPX.OpenCV.runtime.provenance.json"
        if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
            Add-Violation -Violations $Violations -Path $manifestPath -Issue "Stage-Runtime did not write provenance manifest for RID-aware staging case" -Text $Rid
            return
        }

        $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
        if ($manifest.Rid -ne $Rid -or $manifest.OpenCvRid -ne $OpenCvRid -or $manifest.RuntimeProfile -ne "mini") {
            Add-Violation -Violations $Violations -Path $manifestPath -Issue "Stage-Runtime provenance must record RID, OpenCV runtime artifact RID, and profile" -Text "$($manifest.Rid) / $($manifest.OpenCvRid) / $($manifest.RuntimeProfile)"
        }

        if (@($manifest.RequiredModules).Count -ne $modules.Count) {
            Add-Violation -Violations $Violations -Path $manifestPath -Issue "Mini staging provenance must use mini module count" -Text "Found $(@($manifest.RequiredModules).Count), expected $($modules.Count)"
        }
        elseif (Compare-Object -ReferenceObject $modules -DifferenceObject @($manifest.RequiredModules) -SyncWindow 0) {
            Add-Violation -Violations $Violations -Path $manifestPath -Issue "Mini staging provenance must record the exact mini module sequence" -Text "$(@($manifest.RequiredModules) -join ',')"
        }

        $expectedRuntimeFileNames = if ($PlatformFamily -eq "windows") {
            @("JYPPX.OpenCV.Native.dll", "$compatibilityNativeLoaderBaseName.dll") + @($modules | ForEach-Object { "opencv_$_`500.dll" })
        }
        elseif ($PlatformFamily -eq "linux") {
            @("libJYPPX.OpenCV.Native.so", "lib$compatibilityNativeLoaderBaseName.so") + @($modules | ForEach-Object {
                    $module = $_
                    @("libopencv_$module.so", "libopencv_$module.so.5.0.0", "libopencv_$module.so.500")
                })
        }
        else {
            @("libJYPPX.OpenCV.Native.so", "lib$compatibilityNativeLoaderBaseName.so") + @($modules | ForEach-Object { "libopencv_$_.so" })
        }
        $actualRuntimeFileNames = @($manifest.RuntimeFiles | ForEach-Object { [string]$_.FileName })
        if (Compare-Object -ReferenceObject @($expectedRuntimeFileNames | Sort-Object) -DifferenceObject @($actualRuntimeFileNames | Sort-Object)) {
            Add-Violation -Violations $Violations -Path $manifestPath -Issue "Staging provenance must retain the exact platform runtime file set, including Linux SONAME companions" -Text "Found $($actualRuntimeFileNames -join ','), expected $($expectedRuntimeFileNames -join ',')"
        }

        $runtimeInputRoot = ConvertTo-NormalizedPathText ([string]$manifest.InputRoots.OpenCvRuntimeDir)
        $expectedRuntimeSubdir = ConvertTo-NormalizedPathText $RuntimeSubdir
        if (-not (Test-ContainsText -Text $runtimeInputRoot -Needle $expectedRuntimeSubdir)) {
            Add-Violation -Violations $Violations -Path $manifestPath -Issue "Stage-Runtime must resolve default OpenCV runtime input directory from selected RID" -Text "$Rid :: $runtimeInputRoot"
        }
    }
    finally {
        Remove-DirectoryIfPresent -Path $temporaryRoot
    }
}

$violations = [System.Collections.Generic.List[object]]::new()
$matrix = Get-Content -LiteralPath $runtimeMatrixPath -Raw | ConvertFrom-Json
$openCvVersion = "5.0.0"
$openCvMajor = (($openCvVersion -split "\.") | Select-Object -First 1)
$profileCount = @($matrix.profiles).Count
$ridCount = @($matrix.rids).Count

foreach ($ridSpec in @($matrix.rids)) {
    $rid = [string]$ridSpec.rid
    $openCvRid = [string]$ridSpec.opencvRid
    $platformFamily = Get-PlatformFamily -RidSpec $ridSpec

    foreach ($profileSpec in @($matrix.profiles)) {
        $profile = [string]$profileSpec.name
        $buildList = [string]$profileSpec.buildList
        $plan = Invoke-BuildPlan -Rid $rid -BuildList $buildList
        $planByOpenCvRid = Invoke-BuildPlan -Rid $openCvRid -BuildList $buildList

        if ($plan.PackageRid -ne $rid -or $planByOpenCvRid.PackageRid -ne $rid) {
            Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Build-OpenCV must resolve both package RID and OpenCV runtime artifact RID to the package RID" -Text "$rid / $openCvRid => $($plan.PackageRid) / $($planByOpenCvRid.PackageRid)"
        }

        if ($plan.OpenCvRid -ne $openCvRid -or $plan.RuntimeVersionSuffix -ne "$openCvVersion-$openCvRid") {
            Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Build-OpenCV build/install suffix must use matrix OpenCV runtime artifact RID" -Text "$rid => $($plan.OpenCvRid) / $($plan.RuntimeVersionSuffix)"
        }

        if ($plan.BuildList -ne $buildList -or -not (Test-SequenceContains -Values @($plan.CMakeArgs) -Needle "-DBUILD_LIST=$buildList")) {
            Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Build-OpenCV -DescribeOnly must preserve selected runtime profile build list" -Text "$rid/$profile"
        }

        if ($platformFamily -eq "windows") {
            $expectedPlatform = Get-WindowsPlatform -Rid $rid
            $expectedArchFolder = Get-WindowsArchFolder -Rid $rid
            if ($plan.PlatformFamily -ne "windows" -or $plan.BuildSystem -ne "multi-config" -or $plan.InstallTarget -ne "INSTALL") {
                Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Windows RID build plans must be Visual Studio multi-config install plans" -Text "$rid :: $($plan.PlatformFamily) / $($plan.BuildSystem) / $($plan.InstallTarget)"
            }

            if ($plan.Generator -ne "Visual Studio 18 2026" -or $plan.Platform -ne $expectedPlatform) {
                Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Windows RID build plans must select the matching Visual Studio platform" -Text "$rid :: $($plan.Generator) / $($plan.Platform)"
            }

            $runtimeDirs = ConvertTo-NormalizedPathText ((@($plan.ExpectedRuntimeDirs) -join "|"))
            if (-not (Test-ContainsText -Text $runtimeDirs -Needle "$expectedArchFolder/vc18/bin")) {
                Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Windows RID build plan must expose arch-specific OpenCV runtime directory candidates" -Text "$rid :: $runtimeDirs"
            }
        }
        elseif ($platformFamily -eq "linux") {
            if ($plan.PlatformFamily -ne "linux" -or $plan.Generator -ne "Ninja" -or $plan.BuildSystem -ne "single-config") {
                Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Linux RID build plans must be Ninja single-config plans" -Text "$rid :: $($plan.PlatformFamily) / $($plan.Generator) / $($plan.BuildSystem)"
            }

            if (-not (Test-SequenceContains -Values @($plan.CMakeArgs) -Needle "-DCMAKE_BUILD_TYPE=Release")) {
                Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Linux RID build plans must use CMAKE_BUILD_TYPE for single-config builds" -Text $rid
            }

            if (-not (Test-SequenceContains -Values @($plan.CMakeArgs) -Needle '-DCMAKE_INSTALL_RPATH=$ORIGIN')) {
                Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Linux OpenCV install libraries must resolve adjacent package dependencies without producer paths" -Text $rid
            }

            $configCandidates = ConvertTo-NormalizedPathText ((@($plan.ExpectedOpenCvConfigCMake) -join "|"))
            if (-not (Test-ContainsText -Text $configCandidates -Needle "lib/cmake/opencv$openCvMajor/OpenCVConfig.cmake")) {
                Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Linux OpenCVConfig.cmake candidate must derive the OpenCV major from version metadata" -Text "$rid :: $configCandidates"
            }

            if (-not (Test-ContainsText -Text $configCandidates -Needle "lib64/cmake/opencv$openCvMajor/OpenCVConfig.cmake")) {
                Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Linux OpenCVConfig.cmake candidates must include lib64 for Fedora-style installs" -Text "$rid :: $configCandidates"
            }

            $runtimeDirs = ConvertTo-NormalizedPathText ((@($plan.ExpectedRuntimeDirs) -join "|"))
            if (-not (Test-ContainsText -Text $runtimeDirs -Needle "lib64")) {
                Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Linux runtime directory candidates must include lib64 for Fedora-style installs" -Text "$rid :: $runtimeDirs"
            }
        }
        elseif ($platformFamily -eq "android") {
            $expectedAbi = Get-AndroidAbi -Rid $rid
            if ($plan.PlatformFamily -ne "android" -or -not [bool]$plan.RequiresAndroidNdk -or $plan.AndroidAbi -ne $expectedAbi) {
                Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Android RID build plans must require NDK and select matching Android ABI" -Text "$rid :: $($plan.PlatformFamily) / $($plan.RequiresAndroidNdk) / $($plan.AndroidAbi)"
            }

            foreach ($expectedArg in @(
                    "-DANDROID_ABI=$expectedAbi",
                    "-DANDROID_PLATFORM=android-24",
                    "-DBUILD_ANDROID_EXAMPLES=OFF",
                    "-DINSTALL_ANDROID_EXAMPLES=OFF")) {
                if (-not (Test-SequenceContains -Values @($plan.CMakeArgs) -Needle $expectedArg)) {
                    Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Android RID build plans must include Android CMake toolchain and example-disable arguments" -Text "$rid :: $expectedArg"
                }
            }

            $runtimeDirs = ConvertTo-NormalizedPathText ((@($plan.ExpectedRuntimeDirs) -join "|"))
            if (-not (Test-ContainsText -Text $runtimeDirs -Needle "sdk/native/libs/$expectedAbi")) {
                Add-Violation -Violations $violations -Path "scripts/Build-OpenCV.ps1" -Issue "Android RID build plan must expose ABI-specific runtime directory candidates" -Text "$rid :: $runtimeDirs"
            }
        }
        else {
            Add-Violation -Violations $violations -Path "packaging/runtime/runtime-package-matrix.json" -Issue "Runtime RID must declare or infer a supported platformFamily" -Text $rid
        }
    }
}

Invoke-StageCase -Rid "win-x86" -OpenCvRid "windows-x86" -PlatformFamily "windows" -RuntimeSubdir "x86/vc18/bin" -Violations $violations
Invoke-StageCase -Rid "ubuntu.24.04-x64" -OpenCvRid "ubuntu.24.04-x64" -PlatformFamily "linux" -RuntimeSubdir "lib" -Violations $violations
Invoke-StageCase -Rid "fedora.40-x64" -OpenCvRid "fedora.40-x64" -PlatformFamily "linux" -RuntimeSubdir "lib64" -Violations $violations
Invoke-StageCase -Rid "android-arm64" -OpenCvRid "android-arm64" -PlatformFamily "android" -RuntimeSubdir "sdk/native/libs/arm64-v8a" -Violations $violations

if ($violations.Count -gt 0) {
    Write-Host "Real native runtime build matrix coverage guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Real native runtime build matrix coverage guard passed."
Write-Host "RID build plans checked: $ridCount."
Write-Host "Runtime profiles checked per RID: $profileCount."
Write-Host "RID-aware staging default probes checked: win-x86, ubuntu.24.04-x64, fedora.40-x64, android-arm64."
