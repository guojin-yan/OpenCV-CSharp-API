param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$sourceRoot = Join-Path $repo "src/OpenCvSharp"

if (-not (Test-Path -LiteralPath $sourceRoot -PathType Container)) {
    throw "Managed source root was not found: $sourceRoot"
}

$allowedCompatibilityTypes = @(
    [pscustomobject]@{
        Name = "OpenCv5SharpBuildInfo"
        Kind = "class"
        Namespace = "OpenCvSharp"
        RelativePath = "src/OpenCvSharp/OpenCvSharpBuildInfo.cs"
    }
)

$requiredCompatibilityDocumentation = @(
    [pscustomobject]@{
        RelativePath = "README.md"
        Text = "OpenCv5SharpBuildInfo"
    },
    [pscustomobject]@{
        RelativePath = "docs/articles/version-neutral-naming-guide.md"
        Text = "OpenCv5SharpBuildInfo"
    },
    [pscustomobject]@{
        RelativePath = "tests/OpenCvSharp.Tests/Core/BuildInfoTests.cs"
        Text = "OpenCv5SharpBuildInfo"
    }
)

function Get-RepositoryRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return ([System.IO.Path]::GetRelativePath($repo, $Path)) -replace "\\", "/"
}

function Test-IsUnderIgnoredBuildDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $relativePath = Get-RepositoryRelativePath -Path $Path
    $segments = $relativePath -split "/"
    return ($segments -contains "bin") -or ($segments -contains "obj")
}

function Test-IsOpenCvSharpNamespace {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Namespace
    )

    return $Namespace.Equals("OpenCvSharp", [System.StringComparison]::Ordinal) -or
        $Namespace.StartsWith("OpenCvSharp.", [System.StringComparison]::Ordinal)
}

function Test-IsAllowedCompatibilityType {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [string]$Kind,
        [AllowNull()]
        [string]$Namespace,
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    foreach ($allowed in $allowedCompatibilityTypes) {
        if ($Name.Equals($allowed.Name, [System.StringComparison]::Ordinal) -and
            $Kind.Equals($allowed.Kind, [System.StringComparison]::Ordinal) -and
            $RelativePath.Equals($allowed.RelativePath, [System.StringComparison]::Ordinal) -and
            $null -ne $Namespace -and
            $Namespace.Equals($allowed.Namespace, [System.StringComparison]::Ordinal)) {
            return $true
        }
    }

    return $false
}

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [int]$Line,
        [Parameter(Mandatory = $true)]
        [string]$Rule,
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Line = $Line
        Rule = $Rule
        Text = $Text.Trim()
    })
}

$namespaceRegex = [System.Text.RegularExpressions.Regex]::new(
    "^\s*namespace\s+([A-Za-z_][A-Za-z0-9_.]*)(?:\s*[;{])?\s*$",
    [System.Text.RegularExpressions.RegexOptions]::Compiled)
$typeRegex = [System.Text.RegularExpressions.Regex]::new(
    "^\s*public\s+(?:(?:new|abstract|sealed|static|partial|readonly|unsafe)\s+)*(class|struct|enum|interface)\s+([A-Za-z_][A-Za-z0-9_]*)\b",
    [System.Text.RegularExpressions.RegexOptions]::Compiled)
$recordRegex = [System.Text.RegularExpressions.Regex]::new(
    "^\s*public\s+(?:(?:new|sealed|partial|readonly|unsafe)\s+)*record\s+(?:(?:class|struct)\s+)?([A-Za-z_][A-Za-z0-9_]*)\b",
    [System.Text.RegularExpressions.RegexOptions]::Compiled)
$delegateRegex = [System.Text.RegularExpressions.Regex]::new(
    "^\s*public\s+delegate\s+.+?\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(",
    [System.Text.RegularExpressions.RegexOptions]::Compiled)

$violations = [System.Collections.Generic.List[object]]::new()
$publicTypeCount = 0
$namespaceCount = 0
$fixedMajorPublicTypeCount = 0

$files = Get-ChildItem -LiteralPath $sourceRoot -Recurse -File -Filter "*.cs" |
    Where-Object { -not (Test-IsUnderIgnoredBuildDirectory -Path $_.FullName) } |
    Sort-Object FullName

foreach ($file in $files) {
    $relativePath = Get-RepositoryRelativePath -Path $file.FullName
    $currentNamespace = $null
    $lineNumber = 0

    foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
        $lineNumber++

        $namespaceMatch = $namespaceRegex.Match($line)
        if ($namespaceMatch.Success) {
            $namespaceCount++
            $currentNamespace = $namespaceMatch.Groups[1].Value

            if (-not (Test-IsOpenCvSharpNamespace -Namespace $currentNamespace)) {
                Add-Violation `
                    -Violations $violations `
                    -Path $relativePath `
                    -Line $lineNumber `
                    -Rule "Managed namespaces must stay under OpenCvSharp" `
                    -Text $line
            }
        }

        if ($line -match "\bOpenCv5Sharp\.") {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Rule "Source must not reference the retired OpenCv5Sharp namespace" `
                -Text $line
        }

        $kind = $null
        $name = $null
        $typeMatch = $typeRegex.Match($line)
        if ($typeMatch.Success) {
            $kind = $typeMatch.Groups[1].Value
            $name = $typeMatch.Groups[2].Value
        }
        else {
            $recordMatch = $recordRegex.Match($line)
            if ($recordMatch.Success) {
                $kind = "record"
                $name = $recordMatch.Groups[1].Value
            }
            else {
                $delegateMatch = $delegateRegex.Match($line)
                if ($delegateMatch.Success) {
                    $kind = "delegate"
                    $name = $delegateMatch.Groups[1].Value
                }
            }
        }

        if ($null -eq $name) {
            continue
        }

        $publicTypeCount++

        if ($name.Contains("OpenCv5")) {
            $fixedMajorPublicTypeCount++
            if (-not (Test-IsAllowedCompatibilityType -Name $name -Kind $kind -Namespace $currentNamespace -RelativePath $relativePath)) {
                Add-Violation `
                    -Violations $violations `
                    -Path $relativePath `
                    -Line $lineNumber `
                    -Rule "Fixed-major public managed types require an explicit compatibility allowlist" `
                    -Text $line
            }
        }
    }
}

foreach ($required in $requiredCompatibilityDocumentation) {
    $path = Join-Path $repo $required.RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Violation `
            -Violations $violations `
            -Path $required.RelativePath `
            -Line 0 `
            -Rule "Compatibility facade must remain documented and tested" `
            -Text "Required file was not found."
        continue
    }

    $content = [System.IO.File]::ReadAllText($path)
    if (-not $content.Contains($required.Text, [System.StringComparison]::Ordinal)) {
        Add-Violation `
            -Violations $violations `
            -Path $required.RelativePath `
            -Line 0 `
            -Rule "Compatibility facade must remain documented and tested" `
            -Text "Missing required text: $($required.Text)"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Public API namespace neutrality guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Line, Rule |
        Format-Table Path, Line, Rule, Text -AutoSize
    exit 1
}

Write-Host "Public API namespace neutrality guard passed."
Write-Host "Managed source files scanned: $($files.Count)."
Write-Host "Namespaces scanned: $namespaceCount."
Write-Host "Public type declarations scanned: $publicTypeCount."
Write-Host "Allowed fixed-major public compatibility types: $fixedMajorPublicTypeCount."
