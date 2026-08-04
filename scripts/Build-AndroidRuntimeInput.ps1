[CmdletBinding()]
param(
    [ValidateSet("android-arm64", "android-arm", "android-x64", "android-x86")]
    [string]$Rid = "android-arm64",
    [ValidateSet("full", "mini")]
    [string]$RuntimeProfile = "mini",
    [string]$OpenCvVersion = "5.0.0",
    [string]$AndroidNdkRoot = "",
    [string]$AndroidNdkVersion = "28.2.13676358",
    [string]$AndroidApiLevel = "24",
    [string]$WorkspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [string]$OpenCvSourceRoot = "",
    [string]$OpenCvInstallRoot = "",
    [string]$OutputRoot = "artifacts/runtime-inputs",
    [switch]$Build,
    [switch]$DescribeOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path
$matrixPath = Join-Path $repoRoot "packaging/runtime/runtime-package-matrix.json"
$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$ridSpec = @($matrix.rids | Where-Object { $_.rid -eq $Rid -and $_.platformFamily -eq "android" })
$profileSpec = @($matrix.profiles | Where-Object { $_.name -eq $RuntimeProfile })
if ($ridSpec.Count -ne 1 -or $profileSpec.Count -ne 1) {
    throw "Android RID/profile was not found exactly once in the runtime package matrix: $Rid / $RuntimeProfile"
}

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath failed with exit code $LASTEXITCODE."
    }
}

function Get-AndroidAbi {
    param([Parameter(Mandatory = $true)][string]$RuntimeIdentifier)

    switch ($RuntimeIdentifier) {
        "android-arm64" { return "arm64-v8a" }
        "android-arm" { return "armeabi-v7a" }
        "android-x64" { return "x86_64" }
        "android-x86" { return "x86" }
        default { throw "Unsupported Android RID: $RuntimeIdentifier" }
    }
}

function Get-ExpectedElfIdentity {
    param([Parameter(Mandatory = $true)][string]$RuntimeIdentifier)

    switch ($RuntimeIdentifier) {
        "android-arm64" { return [pscustomobject]@{ Class = 2; Machine = 183 } }
        "android-arm" { return [pscustomobject]@{ Class = 1; Machine = 40 } }
        "android-x64" { return [pscustomobject]@{ Class = 2; Machine = 62 } }
        "android-x86" { return [pscustomobject]@{ Class = 1; Machine = 3 } }
        default { throw "Unsupported Android RID: $RuntimeIdentifier" }
    }
}

function Get-NativeProfileEvidence {
    param([Parameter(Mandatory = $true)][string]$Profile)

    $cmakePath = Join-Path $repoRoot "src/OpenCvSharp.Native/CMakeLists.txt"
    $cmakeText = [IO.File]::ReadAllText($cmakePath)
    function Get-SourceList([string]$VariableName) {
        $match = [regex]::Match($cmakeText, "(?s)set\($VariableName(?<body>.*?)\)")
        if (-not $match.Success) {
            throw "Native CMake source list was not found: $VariableName"
        }

        return @(
            [regex]::Matches($match.Groups["body"].Value, "(?m)^\s*(src/[^\s\)]+)\s*$") |
                ForEach-Object { $_.Groups[1].Value }
        )
    }

    $miniSources = @(Get-SourceList -VariableName "OPENCV_CSHARP_MINI_NATIVE_SOURCES")
    $fullSources = @($miniSources + @(Get-SourceList -VariableName "OPENCV_CSHARP_FULL_ONLY_NATIVE_SOURCES"))
    $sources = if ($Profile -eq "mini") { $miniSources } else { $fullSources }
    $manifestName = if ($Profile -eq "mini") { "native_abi_mini_manifest.txt" } else { "native_abi_manifest.txt" }
    $manifestPath = Join-Path $repoRoot "src/OpenCvSharp.Native/generated/$manifestName"
    $functionCountLine = @(Get-Content -LiteralPath $manifestPath | Where-Object { $_ -match "^function-count=" })
    if ($functionCountLine.Count -ne 1) {
        throw "Native ABI manifest did not contain exactly one function-count entry: $manifestPath"
    }

    foreach ($source in $sources) {
        if (-not (Test-Path -LiteralPath (Join-Path $repoRoot "src/OpenCvSharp.Native/$source") -PathType Leaf)) {
            throw "Native wrapper source was not found: $source"
        }
    }

    return [pscustomobject]@{
        Sources = @($sources)
        SourceCount = @($sources).Count
        AbiFunctionCount = [int](([string]$functionCountLine[0]) -replace "^function-count=", "")
    }
}

function Assert-AndroidElf {
    param(
        [Parameter(Mandatory = $true)][string]$ReadElf,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][int]$ElfClass,
        [Parameter(Mandatory = $true)][int]$MachineCode
    )

    $header = @(& $ReadElf -h $Path 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "Android ELF header audit failed for $Path`: $([string]::Join(' | ', $header))"
    }

    $elfHeaderBytes = [IO.File]::ReadAllBytes($Path)
    if ($elfHeaderBytes.Length -lt 20 -or
        $elfHeaderBytes[0] -ne 0x7f -or
        $elfHeaderBytes[1] -ne 0x45 -or
        $elfHeaderBytes[2] -ne 0x4c -or
        $elfHeaderBytes[3] -ne 0x46) {
        throw "Android native library is not a valid ELF file: $Path"
    }
    if ([int]$elfHeaderBytes[4] -ne $ElfClass -or [int]$elfHeaderBytes[5] -ne 1) {
        throw "Android ELF class/data audit failed for $Path`: class=$($elfHeaderBytes[4]) data=$($elfHeaderBytes[5]) expected_class=$ElfClass expected_data=1."
    }

    $actualMachineCode = [int]$elfHeaderBytes[18] -bor ([int]$elfHeaderBytes[19] -shl 8)
    if ($actualMachineCode -ne $MachineCode) {
        throw "Android ELF e_machine audit failed for $Path`: actual=$actualMachineCode expected=$MachineCode."
    }

    $programHeaders = [string]::Join("`n", @(& $ReadElf -lW $Path 2>&1))
    if ($LASTEXITCODE -ne 0) {
        throw "Android ELF program-header audit failed for $Path."
    }

    $loadAlignments = @(
        [regex]::Matches($programHeaders, "(?m)^\s*LOAD\s+.*?\s+(?<align>0x[0-9a-fA-F]+)\s*$") |
            ForEach-Object { [Convert]::ToInt64($_.Groups["align"].Value.Substring(2), 16) }
    )
    if ($loadAlignments.Count -eq 0 -or @($loadAlignments | Where-Object { $_ -lt 16384 }).Count -gt 0) {
        $alignmentEvidence = @($loadAlignments | ForEach-Object { "0x$($_.ToString('x'))" }) -join ","
        throw "Android ELF must retain at least 16 KB LOAD segment alignment: $Path; actual=$alignmentEvidence"
    }
}

$abi = Get-AndroidAbi -RuntimeIdentifier $Rid
$profile = $profileSpec[0]
$nativeEvidence = Get-NativeProfileEvidence -Profile $RuntimeProfile
$workspaceRootFullPath = [IO.Path]::GetFullPath($WorkspaceRoot)
if ([string]::IsNullOrWhiteSpace($OpenCvSourceRoot)) {
    $OpenCvSourceRoot = Join-Path $workspaceRootFullPath "opencv-source"
}
if ([string]::IsNullOrWhiteSpace($OpenCvInstallRoot)) {
    $OpenCvInstallRoot = Join-Path $workspaceRootFullPath "artifacts/opencv-install"
}

$openCvSourceDir = Join-Path ([IO.Path]::GetFullPath($OpenCvSourceRoot)) "opencv-$OpenCvVersion"
$openCvInstallDir = Join-Path ([IO.Path]::GetFullPath($OpenCvInstallRoot)) "opencv-$OpenCvVersion-$Rid"
$openCvConfigDir = Join-Path $openCvInstallDir "sdk/native/jni"
$openCvRuntimeDir = Join-Path $openCvInstallDir "sdk/native/libs/$abi"
$nativeBuildDir = Join-Path $repoRoot "build/native-$Rid-$RuntimeProfile"
$nativeRuntimeDir = $nativeBuildDir
$androidToolchainFile = if ([string]::IsNullOrWhiteSpace($AndroidNdkRoot)) {
    "<ANDROID_NDK_ROOT>/build/cmake/android.toolchain.cmake"
}
else {
    Join-Path ([IO.Path]::GetFullPath($AndroidNdkRoot)) "build/cmake/android.toolchain.cmake"
}
$flexiblePageSizeArgument = "-DANDROID_SUPPORT_FLEXIBLE_PAGE_SIZES=ON"

$buildPlan = & (Join-Path $PSScriptRoot "Build-OpenCV.ps1") `
    -OpenCvVersion $OpenCvVersion `
    -WorkspaceRoot $workspaceRootFullPath `
    -OpenCvSourceRoot $OpenCvSourceRoot `
    -OpenCvInstallRoot $OpenCvInstallRoot `
    -Rid $Rid `
    -BuildList ([string]$profile.buildList) `
    -AndroidNdkRoot $AndroidNdkRoot `
    -AndroidApiLevel $AndroidApiLevel `
    -ExtraCMakeArgs $flexiblePageSizeArgument `
    -DescribeOnly | ConvertFrom-Json

$plan = [ordered]@{
    Rid = $Rid
    Abi = $abi
    RuntimeProfile = $RuntimeProfile
    OpenCvVersion = $OpenCvVersion
    AndroidApiLevel = $AndroidApiLevel
    AndroidNdkVersion = $AndroidNdkVersion
    AndroidNdkRoot = $AndroidNdkRoot
    BuildList = [string]$profile.buildList
    OpenCvSourceDir = $openCvSourceDir
    OpenCvInstallDir = $openCvInstallDir
    OpenCvConfigDir = $openCvConfigDir
    OpenCvRuntimeDir = $openCvRuntimeDir
    NativeBuildDir = $nativeBuildDir
    NativeRuntimeDir = $nativeRuntimeDir
    OutputRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot $OutputRoot))
    NativeWrapperSourceCount = $nativeEvidence.SourceCount
    NativeAbiFunctionCount = $nativeEvidence.AbiFunctionCount
    OpenCvCMakeArguments = @($buildPlan.CMakeArgs)
}

if ($DescribeOnly) {
    $plan | ConvertTo-Json -Depth 8
    return
}
if (-not $Build) {
    throw "Use -DescribeOnly to inspect the Android plan or -Build to produce real runtime input."
}
if ([string]::IsNullOrWhiteSpace($AndroidNdkRoot) -or
    -not (Test-Path -LiteralPath $androidToolchainFile -PathType Leaf)) {
    throw "Android NDK toolchain file was not found: $androidToolchainFile"
}
if (-not (Test-Path -LiteralPath $openCvSourceDir -PathType Container)) {
    throw "OpenCV source directory was not found: $openCvSourceDir"
}

$ndkSourceProperties = Join-Path ([IO.Path]::GetFullPath($AndroidNdkRoot)) "source.properties"
$ndkPropertiesText = Get-Content -LiteralPath $ndkSourceProperties -Raw
if ($ndkPropertiesText -notmatch "(?m)^Pkg\.Revision\s*=\s*$([regex]::Escape($AndroidNdkVersion))\s*$") {
    throw "Android NDK version does not match the pinned producer version $AndroidNdkVersion."
}

& (Join-Path $PSScriptRoot "Build-OpenCV.ps1") `
    -OpenCvVersion $OpenCvVersion `
    -WorkspaceRoot $workspaceRootFullPath `
    -OpenCvSourceRoot $OpenCvSourceRoot `
    -OpenCvInstallRoot $OpenCvInstallRoot `
    -Rid $Rid `
    -BuildList ([string]$profile.buildList) `
    -AndroidNdkRoot $AndroidNdkRoot `
    -AndroidApiLevel $AndroidApiLevel `
    -ExtraCMakeArgs $flexiblePageSizeArgument `
    -Build

foreach ($requiredDirectory in @($openCvConfigDir, $openCvRuntimeDir)) {
    if (-not (Test-Path -LiteralPath $requiredDirectory -PathType Container)) {
        throw "Android OpenCV build output directory was not found: $requiredDirectory"
    }
}
if (-not (Test-Path -LiteralPath (Join-Path $openCvConfigDir "OpenCVConfig.cmake") -PathType Leaf)) {
    throw "Android OpenCVConfig.cmake was not found: $openCvConfigDir"
}

$nativeCMakeArguments = @(
    "-S", (Join-Path $repoRoot "src/OpenCvSharp.Native"),
    "-B", $nativeBuildDir,
    "-G", "Ninja",
    "-DCMAKE_BUILD_TYPE=Release",
    "-DCMAKE_TOOLCHAIN_FILE=$androidToolchainFile",
    "-DANDROID_ABI=$abi",
    "-DANDROID_PLATFORM=android-$AndroidApiLevel",
    $flexiblePageSizeArgument,
    "-DOPENCV_VERSION=$OpenCvVersion",
    "-DOPENCV_CSHARP_OPENCV_DIR=$openCvConfigDir",
    "-DOPENCV_CSHARP_OPENCV_BUILD_LIST=$([string]$profile.buildList)",
    "-DOPENCV_CSHARP_RUNTIME_PROFILE=$RuntimeProfile"
)
Invoke-CheckedCommand cmake @nativeCMakeArguments
Invoke-CheckedCommand cmake --build $nativeBuildDir --config Release --target JYPPX.OpenCV.Native

$primaryLoader = Join-Path $nativeRuntimeDir "libJYPPX.OpenCV.Native.so"
if (-not (Test-Path -LiteralPath $primaryLoader -PathType Leaf)) {
    throw "Android native wrapper loader was not found: $primaryLoader"
}

$prebuiltRoot = Join-Path ([IO.Path]::GetFullPath($AndroidNdkRoot)) "toolchains/llvm/prebuilt/linux-x86_64"
$readElf = Join-Path $prebuiltRoot "bin/llvm-readelf"
$clang = Join-Path $prebuiltRoot "bin/clang++"
if (-not (Test-Path -LiteralPath $readElf -PathType Leaf) -or -not (Test-Path -LiteralPath $clang -PathType Leaf)) {
    throw "Pinned Android NDK LLVM audit tools were not found under $prebuiltRoot."
}

$requiredModuleFiles = @($profile.modules | ForEach-Object { Join-Path $openCvRuntimeDir "libopencv_$_.so" })
$allAuditFiles = @($primaryLoader) + $requiredModuleFiles
$packagedNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($file in $allAuditFiles) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        throw "Required Android runtime library was not found: $file"
    }
    [void]$packagedNames.Add((Split-Path -Leaf $file))
}

$versionedAndroidLibraries = @(
    Get-ChildItem -LiteralPath $openCvRuntimeDir -File -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match '^libopencv_.+\.so\..+$' }
)
if ($versionedAndroidLibraries.Count -gt 0) {
    throw "Android runtime libraries must use unversioned APK-loadable .so names: $($versionedAndroidLibraries.Name -join ', ')"
}

$elfIdentity = Get-ExpectedElfIdentity -RuntimeIdentifier $Rid
$dependencyEdges = 0
foreach ($file in $allAuditFiles) {
    Assert-AndroidElf -ReadElf $readElf -Path $file -ElfClass $elfIdentity.Class -MachineCode $elfIdentity.Machine
    $dynamicText = [string]::Join("`n", @(& $readElf -dW $file 2>&1))
    if ($LASTEXITCODE -ne 0) {
        throw "Android ELF dynamic-section audit failed for $file."
    }
    if ($dynamicText -match "\((?:RPATH|RUNPATH)\).*\[(?:/|[A-Za-z]:)") {
        throw "Android ELF contains an absolute producer RPATH/RUNPATH: $file"
    }

    foreach ($needed in @([regex]::Matches($dynamicText, "\(NEEDED\).*\[(?<name>[^\]]+)\]") | ForEach-Object { $_.Groups["name"].Value })) {
        if ($needed.StartsWith("libopencv_", [StringComparison]::Ordinal) -and -not $packagedNames.Contains($needed)) {
            throw "Android ELF dependency is missing from the package closure: $file -> $needed"
        }
        if ($needed -eq "libc++_shared.so") {
            throw "Android runtime unexpectedly depends on libc++_shared.so; the package contract uses the static NDK C++ runtime."
        }
        $dependencyEdges++
    }
}

$cmakeVersion = (& cmake --version | Select-Object -First 1) -replace "^cmake version\s+", ""
$ninjaVersion = (& ninja --version | Select-Object -First 1)
$dotnetVersion = (& dotnet --version | Select-Object -First 1)
$compilerVersion = [string]::Join(" ", @(& $clang --version | Select-Object -First 1))
$nativeSourcesJson = ConvertTo-Json -InputObject ([object[]]$nativeEvidence.Sources) -Compress
$elfEvidence = "ANDROID_ELF_EVIDENCE rid=$Rid abi=$abi profile=$RuntimeProfile files=$($allAuditFiles.Count) required_modules=$($requiredModuleFiles.Count) dependencies=$dependencyEdges min_page_alignment=16384 versioned_so=0 libcxx_shared=0"
Write-Host $elfEvidence

& (Join-Path $PSScriptRoot "New-RuntimeInputArtifact.ps1") `
    -Rid $Rid `
    -RuntimeProfile $RuntimeProfile `
    -OpenCvVersion $OpenCvVersion `
    -NativeRuntimeDir $nativeRuntimeDir `
    -OpenCvRuntimeDir $openCvRuntimeDir `
    -OpenCvSourceDir $openCvSourceDir `
    -OpenCvInstallDir $openCvInstallDir `
    -HostedRunner "ubuntu-24.04" `
    -RunnerImage $env:ImageOS `
    -RunnerImageVersion $env:ImageVersion `
    -HostedDistro "ubuntu" `
    -HostedDistroVersion "24.04" `
    -HostedArchitecture "x86_64" `
    -HostedPackageArchitecture "amd64" `
    -HostedLibc "glibc" `
    -HostedProcessArchitecture ([Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()) `
    -CMakeVersion $cmakeVersion `
    -CMakeGenerator "Ninja" `
    -BuildConfiguration "Release" `
    -CompilerPath $clang `
    -CompilerVersion "$compilerVersion; NDK $AndroidNdkVersion; API $AndroidApiLevel; ABI $abi" `
    -NinjaVersion $ninjaVersion `
    -DotNetVersion $dotnetVersion `
    -OpenCvCMakeArguments ($buildPlan.CMakeArgs | ConvertTo-Json -Compress) `
    -ElfAuditEvidence $elfEvidence `
    -NativeWrapperSources $nativeSourcesJson `
    -NativeWrapperSourceCount ([string]$nativeEvidence.SourceCount) `
    -NativeAbiFunctionCount ([string]$nativeEvidence.AbiFunctionCount) `
    -OpenCvExtraCMakeArgs $flexiblePageSizeArgument `
    -PowerShellVersion $PSVersionTable.PSVersion.ToString() `
    -OutputRoot $OutputRoot

Write-Host "ANDROID_RUNTIME_INPUT_OK rid=$Rid abi=$abi profile=$RuntimeProfile ndk=$AndroidNdkVersion api=$AndroidApiLevel"
