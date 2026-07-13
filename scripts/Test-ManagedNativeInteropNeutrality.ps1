param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$managedRoot = Join-Path $repo "src/OpenCvSharp"
$manifestPath = Join-Path $repo "src/OpenCvSharp.Native/generated/legacy_abi_manifest.txt"
$nativeLibraryNamesPath = Join-Path $managedRoot "Internal/Interop/NativeLibraryNames.cs"
$buildInfoPath = Join-Path $managedRoot "OpenCvSharpBuildInfo.cs"
$currentNativeLibraryName = "JYPPX.OpenCV.Native"
$currentNativeLibraryExpression = "NativeLibraryNames.CurrentNativeLibrary"
$neutralEntryPointPrefix = "jyppx_ocv_"
$fixedMajorEntryPointPrefix = "jyppx_ocv5_"

foreach ($requiredPath in @($managedRoot, $manifestPath, $nativeLibraryNamesPath, $buildInfoPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required managed interop path was not found: $requiredPath"
    }
}

$manifestSymbols = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::Ordinal)
Get-Content -LiteralPath $manifestPath |
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

        [void]$manifestSymbols.Add($parts[0])
    }

if ($manifestSymbols.Count -eq 0) {
    throw "No neutral ABI functions were found in $manifestPath"
}

function Get-RepositoryRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return [System.IO.Path]::GetRelativePath($repo, $Path).Replace("\", "/")
}

function Get-LineNumber {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [int]$Index
    )

    if ($Index -le 0) {
        return 1
    }

    return (($Text.Substring(0, $Index) -split "`n").Count)
}

$violations = [System.Collections.Generic.List[object]]::new()
$entryPoints = [System.Collections.Generic.List[string]]::new()
$importCount = 0
$attributeRegex = [regex]::new(
    "\[(?<api>DllImport|LibraryImport)\((?<args>[^\]]*)\)\]",
    [System.Text.RegularExpressions.RegexOptions]::Singleline)

$csFiles = @(
    Get-ChildItem -LiteralPath $managedRoot -Recurse -File -Filter "*.cs" |
        Where-Object {
            $_.FullName -notmatch "\\(bin|obj)\\"
        } |
        Sort-Object FullName
)

foreach ($file in $csFiles) {
    $text = [System.IO.File]::ReadAllText($file.FullName)
    $relativePath = Get-RepositoryRelativePath -Path $file.FullName

    if ($text.Contains("OpenCv5Sharp.Native")) {
        $violations.Add([pscustomobject]@{
            Path = $relativePath
            Line = 0
            Issue = "Managed source must not use fixed-major loader name OpenCv5Sharp.Native"
        })
    }

    foreach ($match in $attributeRegex.Matches($text)) {
        $importCount++
        $line = Get-LineNumber -Text $text -Index $match.Index
        $args = $match.Groups["args"].Value.Trim()
        $firstComma = $args.IndexOf(",")
        $libraryExpression = if ($firstComma -lt 0) {
            $args.Trim()
        }
        else {
            $args.Substring(0, $firstComma).Trim()
        }

        if ($libraryExpression -ne $currentNativeLibraryExpression) {
            $violations.Add([pscustomobject]@{
                Path = $relativePath
                Line = $line
                Issue = "Import uses '$libraryExpression' instead of $currentNativeLibraryExpression"
            })
        }

        $entryPointMatch = [regex]::Match($args, "EntryPoint\s*=\s*`"(?<name>[^`"]+)`"")
        if (-not $entryPointMatch.Success) {
            $violations.Add([pscustomobject]@{
                Path = $relativePath
                Line = $line
                Issue = "Import is missing an explicit neutral EntryPoint"
            })
            continue
        }

        $entryPoint = $entryPointMatch.Groups["name"].Value
        $entryPoints.Add($entryPoint)

        if ($entryPoint.StartsWith($fixedMajorEntryPointPrefix, [System.StringComparison]::Ordinal)) {
            $violations.Add([pscustomobject]@{
                Path = $relativePath
                Line = $line
                Issue = "Import uses fixed-major compatibility EntryPoint '$entryPoint'"
            })
            continue
        }

        if (-not $entryPoint.StartsWith($neutralEntryPointPrefix, [System.StringComparison]::Ordinal)) {
            $violations.Add([pscustomobject]@{
                Path = $relativePath
                Line = $line
                Issue = "Import EntryPoint '$entryPoint' does not use neutral prefix $neutralEntryPointPrefix"
            })
            continue
        }

        if (-not $manifestSymbols.Contains($entryPoint)) {
            $violations.Add([pscustomobject]@{
                Path = $relativePath
                Line = $line
                Issue = "Import EntryPoint '$entryPoint' is missing from native ABI manifest"
            })
        }
    }
}

$nativeLibraryNamesText = [System.IO.File]::ReadAllText($nativeLibraryNamesPath)
if (-not $nativeLibraryNamesText.Contains("CurrentNativeLibrary = `"$currentNativeLibraryName`"")) {
    $violations.Add([pscustomobject]@{
        Path = Get-RepositoryRelativePath -Path $nativeLibraryNamesPath
        Line = 0
        Issue = "NativeLibraryNames.CurrentNativeLibrary must stay '$currentNativeLibraryName'"
    })
}

$buildInfoText = [System.IO.File]::ReadAllText($buildInfoPath)
if (-not $buildInfoText.Contains("CurrentNativeLibraryName = `"$currentNativeLibraryName`"")) {
    $violations.Add([pscustomobject]@{
        Path = Get-RepositoryRelativePath -Path $buildInfoPath
        Line = 0
        Issue = "OpenCvSharpBuildInfo.CurrentNativeLibraryName must stay '$currentNativeLibraryName'"
    })
}

if ($importCount -eq 0) {
    throw "No managed DllImport or LibraryImport declarations were found under $managedRoot"
}

if ($violations.Count -gt 0) {
    Write-Host "Managed native interop neutrality guard failed with $($violations.Count) issue(s)."
    $violations |
        Sort-Object Path, Line, Issue |
        Format-Table Path, Line, Issue -AutoSize
    exit 1
}

$uniqueEntryPointCount = @($entryPoints | Sort-Object -Unique).Count
Write-Host (
    "Managed native interop neutrality guard passed. " +
    "Imports: $importCount; " +
    "unique neutral entrypoints: $uniqueEntryPointCount; " +
    "native manifest functions: $($manifestSymbols.Count).")
