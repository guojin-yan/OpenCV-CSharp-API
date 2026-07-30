param(
    [string]$RuntimeDirectory = "",
    [string]$NativeWrapperDirectory = "",
    [string]$OpenCvRuntimeDirectory = "",
    [string]$Rid = "win-x64",
    [ValidateSet("full", "mini")]
    [string]$RuntimeProfile = "full",
    [string]$OpenCvVersion = "5.0.0",
    [string]$RuntimePackageMatrix = "packaging/runtime/runtime-package-matrix.json",
    [string]$DumpbinPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path

function Resolve-InputDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "$Name is required."
    }

    $candidate = if ([System.IO.Path]::IsPathRooted($Value)) {
        $Value
    }
    else {
        Join-Path $repoRoot $Value
    }

    if (-not (Test-Path -LiteralPath $candidate -PathType Container)) {
        throw "$Name was not found: $candidate"
    }

    return (Resolve-Path -LiteralPath $candidate).Path
}

function Resolve-Dumpbin {
    param(
        [string]$ExplicitPath = "",
        [Parameter(Mandatory = $true)]
        [string]$TargetRid
    )

    $toolSpec = switch ($TargetRid) {
        "win-x64" {
            [pscustomobject]@{
                Component = "Microsoft.VisualStudio.Component.VC.Tools.x86.x64"
                Host = "Hostx64"
                Target = "x64"
            }
        }
        "win-x86" {
            [pscustomobject]@{
                Component = "Microsoft.VisualStudio.Component.VC.Tools.x86.x64"
                Host = "Hostx64"
                Target = "x86"
            }
        }
        "win-arm64" {
            [pscustomobject]@{
                Component = "Microsoft.VisualStudio.Component.VC.Tools.ARM64"
                Host = "Hostarm64"
                Target = "arm64"
            }
        }
        default { throw "Unsupported Windows RID while resolving dumpbin.exe: $TargetRid" }
    }
    $dumpbinPattern = '[\\/]bin[\\/]' + [regex]::Escape($toolSpec.Host) + '[\\/]' + [regex]::Escape($toolSpec.Target) + '[\\/]dumpbin\.exe$'

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        if (-not (Test-Path -LiteralPath $ExplicitPath -PathType Leaf)) {
            throw "dumpbin.exe was not found: $ExplicitPath"
        }

        $resolvedExplicitPath = (Resolve-Path -LiteralPath $ExplicitPath).Path
        if ($resolvedExplicitPath -notmatch $dumpbinPattern) {
            throw "Explicit dumpbin.exe path did not match the native $($toolSpec.Host)/$($toolSpec.Target) tool boundary: $resolvedExplicitPath"
        }
        return $resolvedExplicitPath
    }

    $fromPath = @(
        Get-Command dumpbin.exe -CommandType Application -All -ErrorAction SilentlyContinue |
            Where-Object { $_.Source -match $dumpbinPattern }
    )
    if ($fromPath.Count -gt 0) {
        return $fromPath[0].Source
    }

    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio/Installer/vswhere.exe"
    if (-not (Test-Path -LiteralPath $vswhere -PathType Leaf)) {
        throw "vswhere.exe was not found while resolving native $($toolSpec.Host)/$($toolSpec.Target) dumpbin.exe."
    }

    $installationPath = (& $vswhere -latest -products * -requires $toolSpec.Component -property installationPath).Trim()
    if ([string]::IsNullOrWhiteSpace($installationPath)) {
        throw "A Visual Studio installation with native $($toolSpec.Target) C++ tools was not found."
    }

    $matches = @(
        Get-ChildItem -LiteralPath (Join-Path $installationPath "VC/Tools/MSVC") -Recurse -File -Filter dumpbin.exe |
            Where-Object { $_.FullName -match $dumpbinPattern } |
            Sort-Object FullName -Descending |
            Select-Object -First 1
    )
    if ($matches.Count -ne 1) {
        throw "Exactly one latest $($toolSpec.Host)/$($toolSpec.Target) dumpbin.exe was expected."
    }

    return $matches[0].FullName
}

function Get-PeMachine {
    param([Parameter(Mandatory = $true)][string]$Path)

    $stream = [System.IO.File]::OpenRead($Path)
    $reader = [System.IO.BinaryReader]::new($stream)
    try {
        if ($stream.Length -lt 64 -or $reader.ReadUInt16() -ne 0x5a4d) {
            throw "File does not contain a valid DOS/PE header: $Path"
        }

        $stream.Position = 0x3c
        $peOffset = $reader.ReadInt32()
        if ($peOffset -lt 0 -or ($peOffset + 6) -gt $stream.Length) {
            throw "PE header offset is outside the file: $Path"
        }

        $stream.Position = $peOffset
        if ($reader.ReadUInt32() -ne 0x00004550) {
            throw "PE signature was not found: $Path"
        }

        return $reader.ReadUInt16()
    }
    finally {
        $reader.Dispose()
        $stream.Dispose()
    }
}

function Get-PeDependencies {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Dumpbin
    )

    $output = @(& $Dumpbin /dependents $Path 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "dumpbin /dependents failed for '$Path': $($output -join [System.Environment]::NewLine)"
    }

    $dependencies = [System.Collections.Generic.List[string]]::new()
    $inDependencies = $false
    foreach ($rawLine in $output) {
        $line = [string]$rawLine
        if ($line -match '^\s*Image has the following dependencies:\s*$') {
            $inDependencies = $true
            continue
        }

        if (-not $inDependencies) {
            continue
        }

        $trimmed = $line.Trim()
        if ($trimmed -eq "Summary") {
            break
        }

        if ([string]::IsNullOrWhiteSpace($trimmed)) {
            continue
        }

        if ($trimmed -notmatch '^[A-Za-z0-9_.-]+\.dll$') {
            throw "PE dependency must be a leaf DLL name without a producer path: $Path -> $trimmed"
        }

        $dependencies.Add($trimmed)
    }

    if (-not $inDependencies -or $dependencies.Count -eq 0) {
        throw "dumpbin did not report a dependency table for: $Path"
    }

    return @($dependencies)
}

$architectureSpec = switch ($Rid) {
    "win-x64" {
        [pscustomobject]@{
            HostProcessorArchitecture = "AMD64"
            HostRuntimeArchitecture = "X64"
            Machine = 0x8664
            MachineName = "AMD64"
        }
    }
    "win-x86" {
        [pscustomobject]@{
            HostProcessorArchitecture = "AMD64"
            HostRuntimeArchitecture = "X64"
            Machine = 0x014c
            MachineName = "I386"
        }
    }
    "win-arm64" {
        [pscustomobject]@{
            HostProcessorArchitecture = "ARM64"
            HostRuntimeArchitecture = "Arm64"
            Machine = 0xAA64
            MachineName = "ARM64"
        }
    }
    default { throw "This factual PE closure audit approves only exact RIDs win-x64, win-x86, and win-arm64, got '$Rid'." }
}

if (-not [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
    throw "Windows PE closure audit must run on Windows."
}

if ($env:PROCESSOR_ARCHITECTURE -ne $architectureSpec.HostProcessorArchitecture -or
    [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString() -ne $architectureSpec.HostRuntimeArchitecture -or
    [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString() -ne $architectureSpec.HostRuntimeArchitecture -or
    -not [Environment]::Is64BitProcess) {
    throw "Windows PE closure audit requires the audited $($architectureSpec.HostProcessorArchitecture) host and native $($architectureSpec.HostRuntimeArchitecture) inspection process for $Rid."
}
if ($Rid -eq "win-arm64" -and -not [string]::IsNullOrWhiteSpace($env:PROCESSOR_ARCHITEW6432)) {
    throw "Windows ARM64 PE closure audit must not run through x64 compatibility translation."
}
if ($Rid -eq "win-x86" -and
    ($env:RUNNER_ARCH -ne "X64" -or -not [Environment]::Is64BitOperatingSystem)) {
    throw "Windows x86 PE closure audit requires an AMD64 Windows host that can execute I386 payloads through supported WoW64."
}

$singleDirectoryMode = -not [string]::IsNullOrWhiteSpace($RuntimeDirectory)
$splitDirectoryMode = -not [string]::IsNullOrWhiteSpace($NativeWrapperDirectory) -or -not [string]::IsNullOrWhiteSpace($OpenCvRuntimeDirectory)
if ($singleDirectoryMode -eq $splitDirectoryMode) {
    throw "Provide either RuntimeDirectory or both NativeWrapperDirectory and OpenCvRuntimeDirectory."
}

$runtimeDirectories = if ($singleDirectoryMode) {
    @(Resolve-InputDirectory -Name "RuntimeDirectory" -Value $RuntimeDirectory)
}
else {
    @(
        (Resolve-InputDirectory -Name "NativeWrapperDirectory" -Value $NativeWrapperDirectory),
        (Resolve-InputDirectory -Name "OpenCvRuntimeDirectory" -Value $OpenCvRuntimeDirectory)
    )
}

$matrixPath = if ([System.IO.Path]::IsPathRooted($RuntimePackageMatrix)) {
    $RuntimePackageMatrix
}
else {
    Join-Path $repoRoot $RuntimePackageMatrix
}
if (-not (Test-Path -LiteralPath $matrixPath -PathType Leaf)) {
    throw "Runtime package matrix was not found: $matrixPath"
}

$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$ridSpecs = @($matrix.rids | Where-Object { $_.rid -eq $Rid })
$profileSpecs = @($matrix.profiles | Where-Object { $_.name -eq $RuntimeProfile })
if ($ridSpecs.Count -ne 1 -or $profileSpecs.Count -ne 1) {
    throw "RID/profile was not found exactly once in the runtime package matrix: $Rid / $RuntimeProfile"
}
if (-not ([string]$ridSpecs[0].platformFamily).Equals("windows", [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "PE closure audit requires a Windows runtime matrix row."
}

$version = [System.Version]::Parse($OpenCvVersion)
$binarySuffix = "$($version.Major)$($version.Minor)$($version.Build)"
$moduleFileNames = @($profileSpecs[0].modules | ForEach-Object { "opencv_$_$binarySuffix.dll" })
$primaryLoaderName = "JYPPX.OpenCV.Native.dll"
$compatibilityLoaderName = "OpenCv5Sharp.Native.dll"
$expectedFileNames = @($primaryLoaderName, $compatibilityLoaderName) + $moduleFileNames

$filesByName = [System.Collections.Generic.Dictionary[string, System.IO.FileInfo]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($directory in $runtimeDirectories) {
    foreach ($file in @(Get-ChildItem -LiteralPath $directory -File -Filter "*.dll")) {
        if ($filesByName.ContainsKey($file.Name)) {
            throw "Duplicate runtime DLL name was found across audit directories: $($file.Name)"
        }

        $filesByName.Add($file.Name, $file)
    }
}

$missing = @($expectedFileNames | Where-Object { -not $filesByName.ContainsKey($_) })
$unexpected = @($filesByName.Keys | Where-Object { $expectedFileNames -notcontains $_ })
if ($missing.Count -gt 0 -or $unexpected.Count -gt 0 -or $filesByName.Count -ne $expectedFileNames.Count) {
    throw "Windows runtime payload must contain exactly $($expectedFileNames.Count) DLLs. Missing: $($missing -join ', '); unexpected: $($unexpected -join ', ')."
}

$primaryHash = (Get-FileHash -LiteralPath $filesByName[$primaryLoaderName].FullName -Algorithm SHA256).Hash
$compatibilityHash = (Get-FileHash -LiteralPath $filesByName[$compatibilityLoaderName].FullName -Algorithm SHA256).Hash
if ($primaryHash -ne $compatibilityHash) {
    throw "Primary and compatibility native loaders must be byte-identical."
}

$dumpbin = Resolve-Dumpbin -ExplicitPath $DumpbinPath -TargetRid $Rid
$dependenciesByName = [System.Collections.Generic.Dictionary[string, string[]]]::new([System.StringComparer]::OrdinalIgnoreCase)
$opencvImportEdges = 0
foreach ($fileName in $expectedFileNames) {
    $file = $filesByName[$fileName]
    $machine = Get-PeMachine -Path $file.FullName
    if ($machine -ne $architectureSpec.Machine) {
        throw "Runtime DLL is not a $($architectureSpec.MachineName) PE image: $fileName machine=0x$($machine.ToString('X4'))"
    }

    $dependencies = @(Get-PeDependencies -Path $file.FullName -Dumpbin $dumpbin)
    $dependenciesByName.Add($fileName, $dependencies)
    foreach ($dependency in @($dependencies | Where-Object { $_ -match '^opencv_.+\d+\.dll$' })) {
        $opencvImportEdges++
        if (-not $filesByName.ContainsKey($dependency)) {
            throw "Packaged OpenCV dependency closure is incomplete: $fileName -> $dependency"
        }
    }
}

$primaryOpenCvImports = @(
    $dependenciesByName[$primaryLoaderName] |
        Where-Object { $_ -match '^opencv_.+\d+\.dll$' } |
        Sort-Object -Unique
)
if ($primaryOpenCvImports.Count -eq 0) {
    throw "Primary loader does not import any packaged OpenCV DLL."
}

$reachableModules = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$pendingFiles = [System.Collections.Generic.Queue[string]]::new()
$pendingFiles.Enqueue($primaryLoaderName)
while ($pendingFiles.Count -gt 0) {
    $current = $pendingFiles.Dequeue()
    foreach ($dependency in @($dependenciesByName[$current] | Where-Object { $_ -match '^opencv_.+\d+\.dll$' })) {
        if ($reachableModules.Add($dependency)) {
            $pendingFiles.Enqueue($dependency)
        }
    }
}

$unreachableModules = @($moduleFileNames | Where-Object { -not $reachableModules.Contains($_) })
if ($unreachableModules.Count -gt 0 -or $reachableModules.Count -ne $moduleFileNames.Count) {
    throw "Matrix-required OpenCV DLLs must all be reachable from the primary loader import graph. Unreachable: $($unreachableModules -join ', ')."
}

Write-Host "WINDOWS_PE_AUDIT_OK rid=$Rid profile=$RuntimeProfile files=$($filesByName.Count) machine=$($architectureSpec.MachineName) packaged_modules=$($moduleFileNames.Count) reachable_modules=$($reachableModules.Count) loader_opencv_imports=$($primaryOpenCvImports.Count) opencv_import_edges=$opencvImportEdges missing_opencv_imports=0 loader_equal=true"
