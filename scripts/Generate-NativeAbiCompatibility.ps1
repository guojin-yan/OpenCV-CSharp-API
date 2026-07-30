[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [ValidateSet("jyppx_ocv_", "jyppx_ocv5_")]
    [string]$PrimaryPrefix = "jyppx_ocv_",
    [switch]$ValidateOnly,
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$nativeRoot = Join-Path $RepositoryRoot "src/OpenCvSharp.Native"
$headerRoot = Join-Path $nativeRoot "include/open_cv_sharp"
$compatibilityAliasHeaderPath = Join-Path $nativeRoot "include/open_cv_5_sharp/legacy_names.h"
$compatibilityAbiPath = Join-Path $nativeRoot "generated/legacy_abi.cpp"
$compatibilityManifestPath = Join-Path $nativeRoot "generated/legacy_abi_manifest.txt"
$miniCompatibilityAbiPath = Join-Path $nativeRoot "generated/legacy_abi_mini.cpp"
$miniCompatibilityManifestPath = Join-Path $nativeRoot "generated/legacy_abi_mini_manifest.txt"
$miniHeaderPaths = @(
    "core/decomp.h"
    "core/mat.h"
    "core/operations.h"
    "core/persistence.h"
    "core/utility.h"
    "error.h"
    "imgcodecs.h"
    "imgproc.h"
    "version.h"
    "videoio/videoio.h"
)
$declarationMarkerPattern =
    "(?:OPENCV_CSHARP|OPENCV5SHARP)_EXTERN_C\s+" +
    "(?:OPENCV_CSHARP|OPENCV5SHARP)_API\s+"

function Normalize-NewLines {
    param([Parameter(Mandatory)][string]$Text)

    return ($Text -replace "`r`n", "`n" -replace "`r", "`n")
}

function Get-OrdinalSortedObjects {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][object[]]$Values,
        [Parameter(Mandatory)][string]$Property
    )

    $copy = [object[]]$Values.Clone()
    $keys = [string[]]@($copy | ForEach-Object { [string]$_.$Property })
    [Array]::Sort[string, object]($keys, $copy, [StringComparer]::Ordinal)
    return $copy
}

function Get-OrdinalUniqueStrings {
    param([Parameter(Mandatory)][AllowEmptyCollection()][string[]]$Values)

    $unique = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($value in $Values) {
        [void]$unique.Add($value)
    }
    $copy = [string[]]@($unique)
    [Array]::Sort($copy, [StringComparer]::Ordinal)
    return $copy
}

function Write-GeneratedText {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Content
    )

    $normalized = (Normalize-NewLines $Content).TrimEnd() + "`n"
    if ($Check) {
        if (-not (Test-Path -LiteralPath $Path)) {
            throw "Generated file is missing: $Path"
        }

        $current = Normalize-NewLines ([System.IO.File]::ReadAllText($Path))
        if ($current -cne $normalized) {
            throw "Generated file is out of date: $Path"
        }

        return
    }

    $directory = Split-Path -Parent $Path
    [System.IO.Directory]::CreateDirectory($directory) | Out-Null
    [System.IO.File]::WriteAllText(
        $Path,
        $normalized,
        [System.Text.UTF8Encoding]::new($false))
}

function Find-ClosingParenthesis {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][int]$OpenIndex,
        [Parameter(Mandatory)][string]$SourceDescription
    )

    $depth = 0
    for ($index = $OpenIndex; $index -lt $Text.Length; $index++) {
        switch ($Text[$index]) {
            "(" { $depth++ }
            ")" {
                $depth--
                if ($depth -eq 0) {
                    return $index
                }
                if ($depth -lt 0) {
                    throw "Unbalanced declaration parentheses in $SourceDescription"
                }
            }
        }
    }

    throw "Unterminated declaration parentheses in $SourceDescription"
}

function Split-Parameters {
    param(
        [Parameter(Mandatory)][string]$ParameterText,
        [Parameter(Mandatory)][string]$SourceDescription
    )

    $trimmed = $ParameterText.Trim()
    if ($trimmed.Length -eq 0 -or $trimmed -eq "void") {
        return @()
    }

    $parts = [System.Collections.Generic.List[string]]::new()
    $start = 0
    $parenthesisDepth = 0
    $bracketDepth = 0
    $braceDepth = 0

    for ($index = 0; $index -lt $ParameterText.Length; $index++) {
        switch ($ParameterText[$index]) {
            "(" { $parenthesisDepth++ }
            ")" { $parenthesisDepth-- }
            "[" { $bracketDepth++ }
            "]" { $bracketDepth-- }
            "{" { $braceDepth++ }
            "}" { $braceDepth-- }
            "," {
                if ($parenthesisDepth -eq 0 -and
                    $bracketDepth -eq 0 -and
                    $braceDepth -eq 0) {
                    $parts.Add($ParameterText.Substring($start, $index - $start).Trim())
                    $start = $index + 1
                }
            }
        }

        if ($parenthesisDepth -lt 0 -or $bracketDepth -lt 0 -or $braceDepth -lt 0) {
            throw "Unbalanced parameter delimiters in $SourceDescription"
        }
    }

    if ($parenthesisDepth -ne 0 -or $bracketDepth -ne 0 -or $braceDepth -ne 0) {
        throw "Unbalanced parameter delimiters in $SourceDescription"
    }

    $parts.Add($ParameterText.Substring($start).Trim())
    return $parts.ToArray()
}

function Get-ParameterName {
    param(
        [Parameter(Mandatory)][string]$Parameter,
        [Parameter(Mandatory)][string]$SourceDescription
    )

    $withoutComments = $Parameter -replace "/\*.*?\*/", " "
    $match = [regex]::Match(
        $withoutComments.Trim(),
        "(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*(?:\[[^\]]*\]\s*)?$")
    if (-not $match.Success) {
        throw "Could not identify parameter name in '$Parameter' from $SourceDescription"
    }

    return $match.Groups["name"].Value
}

function Get-PublicDeclarations {
    param(
        [Parameter(Mandatory)][System.IO.FileInfo[]]$Headers,
        [Parameter(Mandatory)][string]$Prefix
    )

    $declarations = [System.Collections.Generic.List[object]]::new()
    $escapedPrefix = [regex]::Escape($Prefix)

    foreach ($header in $Headers) {
        $text = Normalize-NewLines ([System.IO.File]::ReadAllText($header.FullName))
        $relativeHeader = [System.IO.Path]::GetRelativePath($headerRoot, $header.FullName).
            Replace("\", "/")
        $markerMatches = [regex]::Matches($text, $declarationMarkerPattern)

        foreach ($markerMatch in $markerMatches) {
            $signatureStart = $markerMatch.Index + $markerMatch.Length
            $openParenthesis = $text.IndexOf("(", $signatureStart)
            if ($openParenthesis -lt 0) {
                throw "Missing parameter list after export marker in $relativeHeader"
            }

            $signatureHead = $text.Substring(
                $signatureStart,
                $openParenthesis - $signatureStart).Trim()
            $signatureMatch = [regex]::Match(
                $signatureHead,
                "^(?<return>.+?)\s+(?<name>${escapedPrefix}[A-Za-z0-9_]+)$",
                [System.Text.RegularExpressions.RegexOptions]::Singleline)
            if (-not $signatureMatch.Success) {
                throw "Could not parse exported signature '$signatureHead' in $relativeHeader"
            }

            $functionName = $signatureMatch.Groups["name"].Value
            $returnType = ($signatureMatch.Groups["return"].Value -replace "\s+", " ").Trim()
            $closeParenthesis = Find-ClosingParenthesis `
                -Text $text `
                -OpenIndex $openParenthesis `
                -SourceDescription "$relativeHeader::$functionName"
            $cursor = $closeParenthesis + 1
            while ($cursor -lt $text.Length -and [char]::IsWhiteSpace($text[$cursor])) {
                $cursor++
            }
            if ($cursor -ge $text.Length -or $text[$cursor] -ne ";") {
                throw "Expected ';' after declaration $relativeHeader::$functionName"
            }

            $parameterText = $text.Substring(
                $openParenthesis + 1,
                $closeParenthesis - $openParenthesis - 1).Trim()
            $parameters = @(
                Split-Parameters `
                    -ParameterText $parameterText `
                    -SourceDescription "$relativeHeader::$functionName"
            )
            $argumentNames = @(
                $parameters | ForEach-Object {
                    Get-ParameterName `
                        -Parameter $_ `
                        -SourceDescription "$relativeHeader::$functionName"
                }
            )

            $declarations.Add([pscustomobject]@{
                Header = $relativeHeader
                Name = $functionName
                ReturnType = $returnType
                ParameterText = $parameterText
                ParameterCount = $parameters.Count
                ArgumentNames = $argumentNames
            })
        }
    }

    return $declarations.ToArray()
}

if (-not (Test-Path -LiteralPath $headerRoot)) {
    throw "Primary native header tree was not found: $headerRoot"
}

$headers = @(Get-OrdinalSortedObjects -Values @(
        Get-ChildItem -LiteralPath $headerRoot -Recurse -File -Filter "*.h"
    ) -Property "FullName")
$declarations = @(
    Get-PublicDeclarations -Headers $headers -Prefix $PrimaryPrefix
)
$miniDeclarations = @(
    $declarations | Where-Object { $_.Header -in $miniHeaderPaths }
)

if ($declarations.Count -eq 0) {
    throw "No exported declarations using prefix '$PrimaryPrefix' were found."
}

if ($miniDeclarations.Count -eq 0) {
    throw "No mini-profile declarations using prefix '$PrimaryPrefix' were found."
}

$miniDeclarationHeaders = @(Get-OrdinalUniqueStrings -Values @($miniDeclarations.Header))
$missingMiniHeaders = @($miniHeaderPaths | Where-Object { $_ -notin $miniDeclarationHeaders })
if ($missingMiniHeaders.Count -gt 0) {
    throw "Mini-profile headers contain no exported declarations: $($missingMiniHeaders -join ', ')"
}

$duplicateFunctions = @(
    $declarations |
        Group-Object Name |
        Where-Object Count -gt 1
)
if ($duplicateFunctions.Count -gt 0) {
    $names = $duplicateFunctions.Name -join ", "
    throw "Duplicate public function declarations were found: $names"
}

$supportedReturnTypes = @("int", "void", "const char*")
$unsupportedReturnTypes = @(Get-OrdinalUniqueStrings -Values @($declarations.ReturnType) |
    Where-Object { $_ -notin $supportedReturnTypes })
if ($unsupportedReturnTypes.Count -gt 0) {
    throw "Unsupported exported return types: $($unsupportedReturnTypes -join ', ')"
}

$identifierPattern = "\b$([regex]::Escape($PrimaryPrefix))[A-Za-z0-9_]+\b"
$primaryIdentifiers = @(Get-OrdinalUniqueStrings -Values @(
    foreach ($header in $headers) {
        $text = [System.IO.File]::ReadAllText($header.FullName)
        [regex]::Matches($text, $identifierPattern) |
            ForEach-Object Value
    }
))
$miniPrimaryIdentifiers = @(Get-OrdinalUniqueStrings -Values @(
    foreach ($header in $headers) {
        $relativeHeader = [System.IO.Path]::GetRelativePath($headerRoot, $header.FullName).Replace("\", "/")
        if ($relativeHeader -notin $miniHeaderPaths) {
            continue
        }

        $text = [System.IO.File]::ReadAllText($header.FullName)
        [regex]::Matches($text, $identifierPattern) |
            ForEach-Object Value
    }
))

$headerCounts = @(Get-OrdinalSortedObjects -Values @(
        $declarations | Group-Object Header
    ) -Property "Name")
$returnTypeCounts = @(Get-OrdinalSortedObjects -Values @(
        $declarations | Group-Object ReturnType
    ) -Property "Name")
$sortedDeclarations = @(Get-OrdinalSortedObjects -Values $declarations -Property "Name")

Write-Host "Parsed $($declarations.Count) public declarations from $($headerCounts.Count) headers."
Write-Host "Found $($primaryIdentifiers.Count) unique public identifiers using '$PrimaryPrefix'."
Write-Host ("Return types: " + (($returnTypeCounts | ForEach-Object {
    "$($_.Name)=$($_.Count)"
}) -join ", "))

if ($ValidateOnly) {
    return
}

if ($PrimaryPrefix -ne "jyppx_ocv_") {
    throw "Generated compatibility files require the neutral primary prefix 'jyppx_ocv_'."
}

$compatibilityExportPrefix = "jyppx_ocv5_"
$includeLines = @(
    $headerCounts.Name |
        ForEach-Object { "#include `"open_cv_sharp/$_`"" }
)
$wrapperLines = [System.Collections.Generic.List[string]]::new()
foreach ($declaration in $sortedDeclarations) {
    $compatibilityName = $compatibilityExportPrefix + $declaration.Name.Substring($PrimaryPrefix.Length)
    $parameters = if ($declaration.ParameterText.Length -eq 0) {
        "void"
    }
    else {
        $declaration.ParameterText
    }
    $arguments = $declaration.ArgumentNames -join ", "

    $wrapperLines.Add(
        "OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API $($declaration.ReturnType) " +
        "$compatibilityName($parameters)")
    $wrapperLines.Add("{")
    if ($declaration.ReturnType -eq "void") {
        $wrapperLines.Add("    $($declaration.Name)($arguments);")
    }
    else {
        $wrapperLines.Add("    return $($declaration.Name)($arguments);")
    }
    $wrapperLines.Add("}")
    $wrapperLines.Add("")
}

$compatibilityAbiContent = @"
// Generated by scripts/Generate-NativeAbiCompatibility.ps1. Do not edit.
// Exports the fixed-major ABI only as a forwarding compatibility layer.

$($includeLines -join "`n")

$($wrapperLines -join "`n")
"@

$compatibilityAliasLines = @(
    foreach ($identifier in $primaryIdentifiers) {
        $compatibilityIdentifier = $compatibilityExportPrefix + $identifier.Substring($PrimaryPrefix.Length)
        "#define $compatibilityIdentifier $identifier"
    }
)
$compatibilityAliasHeaderContent = @"
#pragma once

// Generated by scripts/Generate-NativeAbiCompatibility.ps1. Do not edit.
// Source compatibility aliases for existing code that includes open_cv_5_sharp headers.

$($compatibilityAliasLines -join "`n")
"@

$manifestLines = @(
    "# Generated by scripts/Generate-NativeAbiCompatibility.ps1. Do not edit."
    "primary-prefix=$PrimaryPrefix"
    "legacy-prefix=$compatibilityExportPrefix"
    "function-count=$($declarations.Count)"
    "identifier-count=$($primaryIdentifiers.Count)"
    ""
    "[functions]"
    $sortedDeclarations |
        ForEach-Object {
            $compatibilityName = $compatibilityExportPrefix + $_.Name.Substring($PrimaryPrefix.Length)
            "$($_.Name)|$compatibilityName|$($_.ReturnType)|$($_.ParameterCount)|$($_.Header)"
        }
)

Write-GeneratedText -Path $compatibilityAbiPath -Content $compatibilityAbiContent
Write-GeneratedText -Path $compatibilityAliasHeaderPath -Content $compatibilityAliasHeaderContent
Write-GeneratedText -Path $compatibilityManifestPath -Content ($manifestLines -join "`n")

$miniHeaderCounts = @(Get-OrdinalSortedObjects -Values @(
        $miniDeclarations | Group-Object Header
    ) -Property "Name")
$sortedMiniDeclarations = @(Get-OrdinalSortedObjects -Values $miniDeclarations -Property "Name")
$miniIncludeLines = @(
    $miniHeaderCounts.Name |
        ForEach-Object { "#include `"open_cv_sharp/$_`"" }
)
$miniWrapperLines = [System.Collections.Generic.List[string]]::new()
foreach ($declaration in $sortedMiniDeclarations) {
    $compatibilityName = $compatibilityExportPrefix + $declaration.Name.Substring($PrimaryPrefix.Length)
    $parameters = if ($declaration.ParameterText.Length -eq 0) {
        "void"
    }
    else {
        $declaration.ParameterText
    }
    $arguments = $declaration.ArgumentNames -join ", "

    $miniWrapperLines.Add(
        "OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API $($declaration.ReturnType) " +
        "$compatibilityName($parameters)")
    $miniWrapperLines.Add("{")
    if ($declaration.ReturnType -eq "void") {
        $miniWrapperLines.Add("    $($declaration.Name)($arguments);")
    }
    else {
        $miniWrapperLines.Add("    return $($declaration.Name)($arguments);")
    }
    $miniWrapperLines.Add("}")
    $miniWrapperLines.Add("")
}

$miniCompatibilityAbiContent = @"
// Generated by scripts/Generate-NativeAbiCompatibility.ps1. Do not edit.
// Exports the fixed-major ABI only for entrypoints present in the mini native profile.

$($miniIncludeLines -join "`n")

$($miniWrapperLines -join "`n")
"@
$miniManifestLines = @(
    "# Generated by scripts/Generate-NativeAbiCompatibility.ps1. Do not edit."
    "runtime-profile=mini"
    "primary-prefix=$PrimaryPrefix"
    "legacy-prefix=$compatibilityExportPrefix"
    "function-count=$($miniDeclarations.Count)"
    "identifier-count=$($miniPrimaryIdentifiers.Count)"
    ""
    "[functions]"
    $sortedMiniDeclarations |
        ForEach-Object {
            $compatibilityName = $compatibilityExportPrefix + $_.Name.Substring($PrimaryPrefix.Length)
            "$($_.Name)|$compatibilityName|$($_.ReturnType)|$($_.ParameterCount)|$($_.Header)"
        }
)

Write-GeneratedText -Path $miniCompatibilityAbiPath -Content $miniCompatibilityAbiContent
Write-GeneratedText -Path $miniCompatibilityManifestPath -Content ($miniManifestLines -join "`n")

$mode = if ($Check) { "Verified" } else { "Generated" }
Write-Host "$mode $compatibilityAbiPath"
Write-Host "$mode $compatibilityAliasHeaderPath"
Write-Host "$mode $compatibilityManifestPath"
Write-Host "$mode $miniCompatibilityAbiPath"
Write-Host "$mode $miniCompatibilityManifestPath"
