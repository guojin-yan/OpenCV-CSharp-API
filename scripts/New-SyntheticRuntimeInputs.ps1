param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$Rid = "win-x64",
    [string]$RuntimeProfile = "full",
    [string]$OpenCvVersion = "5.0.0",
    [string]$OutputRoot = "",
    [string]$RuntimePackageMatrix = "packaging/runtime/runtime-package-matrix.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-runtime-inputs-" + [System.Guid]::NewGuid().ToString("N"))
}

$matrixPath = if ([System.IO.Path]::IsPathRooted($RuntimePackageMatrix)) {
    $RuntimePackageMatrix
}
else {
    Join-Path $repo $RuntimePackageMatrix
}

if (-not (Test-Path -LiteralPath $matrixPath -PathType Leaf)) {
    throw "Runtime package matrix was not found: $matrixPath"
}

$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$ridSpec = @($matrix.rids | Where-Object { $_.rid -eq $Rid } | Select-Object -First 1)
if ($ridSpec.Count -eq 0) {
    throw "RID '$Rid' was not found in runtime package matrix: $matrixPath"
}

$profileSpec = @($matrix.profiles | Where-Object { $_.name -eq $RuntimeProfile } | Select-Object -First 1)
if ($profileSpec.Count -eq 0) {
    throw "Runtime profile '$RuntimeProfile' was not found in runtime package matrix: $matrixPath"
}

$outputFullPath = [System.IO.Path]::GetFullPath($OutputRoot)
$nativeWrapperDir = Join-Path $outputFullPath "native-wrapper"
$openCvRuntimeDir = Join-Path $outputFullPath "opencv-runtime"
$openCvSourceDir = Join-Path $outputFullPath "opencv-source"
$openCvInstallDir = Join-Path $outputFullPath "opencv-install"
$licenseDir = Join-Path $openCvInstallDir "etc/licenses"

New-Item -ItemType Directory -Force -Path $nativeWrapperDir, $openCvRuntimeDir, $openCvSourceDir, $licenseDir | Out-Null

function Write-SyntheticBinary {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Extension
    )

    $directory = Split-Path -Parent $Path
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    if ($Extension -eq ".dll") {
        [System.IO.File]::WriteAllBytes($Path, [byte[]](0x4D, 0x5A, 0x00, 0x00))
        return
    }

    [System.IO.File]::WriteAllBytes($Path, [byte[]](0x7F, 0x45, 0x4C, 0x46))
}

$extension = [string]$ridSpec.nativeExtension
if ([string]::IsNullOrWhiteSpace($extension)) {
    $extension = if ($Rid.StartsWith("win-", [System.StringComparison]::OrdinalIgnoreCase)) { ".dll" } else { ".so" }
}

$nativeLoaderNames = if ($extension -eq ".dll") {
    @("JYPPX.OpenCV.Native.dll", "OpenCv5Sharp.Native.dll") # compatibility loader copy for already-compiled consumers
}
else {
    @("libJYPPX.OpenCV.Native$extension", "libOpenCv5Sharp.Native$extension")
}

foreach ($loaderName in $nativeLoaderNames) {
    Write-SyntheticBinary -Path (Join-Path $nativeWrapperDir $loaderName) -Extension $extension
}

$openCvBinarySuffix = (($OpenCvVersion -split "\.") | Select-Object -First 3) -join ""
foreach ($module in @($profileSpec.modules)) {
    if ([string]::IsNullOrWhiteSpace($module)) {
        continue
    }

    $fileName = if ($extension -eq ".dll") {
        "opencv_$module$openCvBinarySuffix.dll"
    }
    elseif ([string]$ridSpec.platformFamily -eq "android") {
        "libopencv_$module.so"
    }
    else {
        "libopencv_$module.so.$OpenCvVersion"
    }

    Write-SyntheticBinary -Path (Join-Path $openCvRuntimeDir $fileName) -Extension $extension
}

[System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "LICENSE"), "Synthetic OpenCV license for runtime package surface validation.")
New-Item -ItemType Directory -Force -Path (Join-Path $openCvSourceDir "3rdparty/ippicv") | Out-Null
[System.IO.File]::WriteAllText((Join-Path $openCvSourceDir "3rdparty/ippicv/readme.htm"), "Synthetic IPPICV notice.")
[System.IO.File]::WriteAllText((Join-Path $licenseDir "synthetic-opencv-third-party.txt"), "Synthetic OpenCV third-party notice.")

$result = [pscustomobject]@{
    OutputRoot = $outputFullPath
    NativeWrapperDir = $nativeWrapperDir
    OpenCvRuntimeDir = $openCvRuntimeDir
    OpenCvSourceDir = $openCvSourceDir
    OpenCvInstallDir = $openCvInstallDir
    Rid = $Rid
    RuntimeProfile = $RuntimeProfile
    Modules = @($profileSpec.modules)
    NativeExtension = $extension
}

$result | ConvertTo-Json -Depth 5
