[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$LibraryPath,
    [string]$ManifestPath = (
        Join-Path $PSScriptRoot "../src/OpenCvSharp.Native/generated/native_abi_manifest.txt"),
    [string]$ExportToolPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$resolvedLibraryPath = (Resolve-Path -LiteralPath $LibraryPath).Path
$resolvedManifestPath = (Resolve-Path -LiteralPath $ManifestPath).Path
$manifestRows = @(
    Get-Content -LiteralPath $resolvedManifestPath |
        Where-Object {
            $_ -and
            -not $_.StartsWith("#") -and
            -not $_.StartsWith("[") -and
            $_.Contains("|")
        } |
        ForEach-Object {
            $parts = $_.Split("|")
            if ($parts.Count -ne 4) {
                throw "Malformed ABI manifest row: $_"
            }

            [pscustomobject]@{
                Name = $parts[0]
                ReturnType = $parts[1]
                ParameterCount = [int]$parts[2]
                Header = $parts[3]
            }
        }
)

if ($manifestRows.Count -eq 0) {
    throw "No ABI functions were found in $resolvedManifestPath"
}

function Get-PeMachine {
    param([Parameter(Mandatory)][string]$Path)

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

function Assert-ManifestExports {
    param(
        [Parameter(Mandatory)][System.Collections.Generic.HashSet[string]]$ExportNames,
        [Parameter(Mandatory)][object[]]$Rows
    )

    $missingExports = [System.Collections.Generic.List[string]]::new()
    foreach ($row in $Rows) {
        if (-not $ExportNames.Contains($row.Name)) {
            $missingExports.Add($row.Name)
        }
    }

    if ($missingExports.Count -gt 0) {
        Write-Error (
            "Missing exports ($($missingExports.Count)): " +
            ($missingExports -join ", "))
        throw "Native ABI export audit failed."
    }
}

$runningOnWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
    [System.Runtime.InteropServices.OSPlatform]::Windows)
$requiresStaticExportAudit = $false
$libraryMachine = 0
$processMachine = 0
if ($runningOnWindows) {
    $libraryMachine = Get-PeMachine -Path $resolvedLibraryPath
    $processMachine = switch ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()) {
        "X86" { 0x014c }
        "X64" { 0x8664 }
        "Arm64" { 0xaa64 }
        default { 0 }
    }
    $requiresStaticExportAudit = $libraryMachine -ne $processMachine
}

if ($requiresStaticExportAudit) {
    if ([string]::IsNullOrWhiteSpace($ExportToolPath)) {
        throw (
            "Cross-architecture Windows ABI export audit requires ExportToolPath: " +
            "library_machine=0x$($libraryMachine.ToString('X4')) " +
            "process_machine=0x$($processMachine.ToString('X4')).")
    }

    $resolvedExportToolPath = (Resolve-Path -LiteralPath $ExportToolPath).Path
    if ([System.IO.Path]::GetFileName($resolvedExportToolPath) -ne "dumpbin.exe") {
        throw "Windows ABI export tool must be dumpbin.exe: $resolvedExportToolPath"
    }

    $output = @(& $resolvedExportToolPath /nologo /exports $resolvedLibraryPath 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "dumpbin /exports failed: $($output -join [System.Environment]::NewLine)"
    }

    $exportNames = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal)
    foreach ($line in $output) {
        $match = [regex]::Match(
            [string]$line,
            '^\s+\d+\s+[0-9A-Fa-f]+\s+[0-9A-Fa-f]+\s+(?<name>[^\s=]+)')
        if ($match.Success) {
            [void]$exportNames.Add($match.Groups['name'].Value)
        }
    }
    if ($exportNames.Count -eq 0) {
        throw "dumpbin did not report any named exports for $resolvedLibraryPath"
    }

    Assert-ManifestExports -ExportNames $exportNames -Rows $manifestRows
    Write-Host "Verified version-neutral exports: $($manifestRows.Count)"
    Write-Host (
        "Verified cross-architecture Windows export table: " +
        "library_machine=0x$($libraryMachine.ToString('X4')) " +
        "process_machine=0x$($processMachine.ToString('X4')) " +
        "tool=$resolvedExportToolPath")
    return
}

$addresses = @{}
$exportNames = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::Ordinal)
$libraryHandle = [System.Runtime.InteropServices.NativeLibrary]::Load(
    $resolvedLibraryPath)

try {
    foreach ($row in $manifestRows) {
        $address = [IntPtr]::Zero
        if ([System.Runtime.InteropServices.NativeLibrary]::TryGetExport(
                $libraryHandle,
                $row.Name,
                [ref]$address)) {
            $addresses[$row.Name] = $address
            [void]$exportNames.Add($row.Name)
        }
    }

    Assert-ManifestExports -ExportNames $exportNames -Rows $manifestRows

    if (-not ("OpenCvCSharpNativeIntDelegate" -as [type])) {
        Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int OpenCvCSharpNativeIntDelegate();

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr OpenCvCSharpNativeStringDelegate();
"@
    }

    function Invoke-NativeInt {
        param([Parameter(Mandatory)][string]$Symbol)

        $callback = [System.Runtime.InteropServices.Marshal]::
            GetDelegateForFunctionPointer(
                $addresses[$Symbol],
                [OpenCvCSharpNativeIntDelegate])
        return $callback.Invoke()
    }

    function Invoke-NativeString {
        param([Parameter(Mandatory)][string]$Symbol)

        $callback = [System.Runtime.InteropServices.Marshal]::
            GetDelegateForFunctionPointer(
                $addresses[$Symbol],
                [OpenCvCSharpNativeStringDelegate])
        $pointer = $callback.Invoke()
        return [System.Runtime.InteropServices.Marshal]::PtrToStringAnsi($pointer)
    }

    $versionValues = @(
        Invoke-NativeInt -Symbol "jyppx_ocv_get_native_abi_version"
        Invoke-NativeInt -Symbol "jyppx_ocv_get_version_major"
        Invoke-NativeInt -Symbol "jyppx_ocv_get_version_minor"
        Invoke-NativeInt -Symbol "jyppx_ocv_get_version_revision"
        Invoke-NativeString -Symbol "jyppx_ocv_get_version_string"
    )

    if ($versionValues[0] -ne 1) {
        throw "Native ABI version probe returned '$($versionValues[0])' instead of '1'."
    }

    Write-Host "Verified version-neutral exports: $($manifestRows.Count)"
    Write-Host (
        "Verified native version exports: " +
        "$($versionValues[1]).$($versionValues[2]).$($versionValues[3]) " +
        "($($versionValues[4])); ABI $($versionValues[0])")
}
finally {
    [System.Runtime.InteropServices.NativeLibrary]::Free($libraryHandle)
}
