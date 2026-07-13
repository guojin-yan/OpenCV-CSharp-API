[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$LibraryPath,
    [string]$ManifestPath = (
        Join-Path $PSScriptRoot "../src/OpenCvSharp.Native/generated/legacy_abi_manifest.txt")
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
            if ($parts.Count -ne 5) {
                throw "Malformed ABI manifest row: $_"
            }

            [pscustomobject]@{
                Neutral = $parts[0]
                Compatibility = $parts[1]
                ReturnType = $parts[2]
                ParameterCount = [int]$parts[3]
                Header = $parts[4]
            }
        }
)

if ($manifestRows.Count -eq 0) {
    throw "No ABI functions were found in $resolvedManifestPath"
}

$addresses = @{}
$missingNeutral = [System.Collections.Generic.List[string]]::new()
$missingCompatibility = [System.Collections.Generic.List[string]]::new()
$libraryHandle = [System.Runtime.InteropServices.NativeLibrary]::Load(
    $resolvedLibraryPath)

try {
    foreach ($row in $manifestRows) {
        foreach ($symbol in @($row.Neutral, $row.Compatibility)) {
            $address = [IntPtr]::Zero
            if ([System.Runtime.InteropServices.NativeLibrary]::TryGetExport(
                    $libraryHandle,
                    $symbol,
                    [ref]$address)) {
                $addresses[$symbol] = $address
                continue
            }

            if ($symbol -eq $row.Neutral) {
                $missingNeutral.Add($symbol)
            }
            else {
                $missingCompatibility.Add($symbol)
            }
        }
    }

    if ($missingNeutral.Count -gt 0 -or $missingCompatibility.Count -gt 0) {
        if ($missingNeutral.Count -gt 0) {
            Write-Error (
                "Missing neutral exports ($($missingNeutral.Count)): " +
                ($missingNeutral -join ", "))
        }
        if ($missingCompatibility.Count -gt 0) {
            Write-Error (
                "Missing compatibility exports ($($missingCompatibility.Count)): " +
                ($missingCompatibility -join ", "))
        }

        throw "Native ABI export audit failed."
    }

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

    $versionPairs = @(
        @("jyppx_ocv_get_version_major", "jyppx_ocv5_get_version_major", "int"),
        @("jyppx_ocv_get_version_minor", "jyppx_ocv5_get_version_minor", "int"),
        @("jyppx_ocv_get_version_revision", "jyppx_ocv5_get_version_revision", "int"),
        @("jyppx_ocv_get_version_string", "jyppx_ocv5_get_version_string", "string")
    )
    $versionValues = [System.Collections.Generic.List[string]]::new()
    foreach ($pair in $versionPairs) {
        $neutralValue = if ($pair[2] -eq "int") {
            Invoke-NativeInt -Symbol $pair[0]
        }
        else {
            Invoke-NativeString -Symbol $pair[0]
        }
        $compatibilityValue = if ($pair[2] -eq "int") {
            Invoke-NativeInt -Symbol $pair[1]
        }
        else {
            Invoke-NativeString -Symbol $pair[1]
        }

        if ($neutralValue -ne $compatibilityValue) {
            throw (
                "Neutral and compatibility exports returned different values: " +
                "$($pair[0])='$neutralValue', $($pair[1])='$compatibilityValue'")
        }
        $versionValues.Add([string]$neutralValue)
    }

    Write-Host "Verified neutral exports: $($manifestRows.Count)"
    Write-Host "Verified compatibility exports: $($manifestRows.Count)"
    Write-Host (
        "Verified neutral/compatibility version equivalence: " +
        "$($versionValues[0]).$($versionValues[1]).$($versionValues[2]) " +
        "($($versionValues[3]))")
}
finally {
    [System.Runtime.InteropServices.NativeLibrary]::Free($libraryHandle)
}
