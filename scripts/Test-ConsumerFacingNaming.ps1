param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path

$scanRelativePaths = @(
    "samples",
    "tests",
    "docs",
    ".github",
    "packaging",
    "README.md",
    "CONTRIBUTING.md"
)

$ignoredDirectoryNames = @(
    "bin",
    "obj",
    "_site",
    "api"
)

$activeLeakChecks = @(
    [pscustomobject]@{
        Name = "Consumer examples must not use the retired OpenCv5Sharp namespace"
        Pattern = "(^|[`'\s])using\s+OpenCv5Sharp(?:[.;])"
    },
    [pscustomobject]@{
        Name = "Consumer examples must not declare the retired OpenCv5Sharp namespace"
        Pattern = "(^|[`'\s])namespace\s+OpenCv5Sharp(?:[.;\s{])"
    },
    [pscustomobject]@{
        Name = "Install docs must not recommend fixed-major OpenCv5Sharp package identity"
        Pattern = ("dotnet\s+add\s+package\s+Open" + "Cv5Sharp\b")
    },
    [pscustomobject]@{
        Name = "Consumer project snippets must not reference fixed-major OpenCv5Sharp packages"
        Pattern = ("Package" + "Reference.*Open" + "Cv5Sharp")
    },
    [pscustomobject]@{
        Name = "Runtime package identity must stay version-neutral in consumer-facing docs"
        Pattern = "OpenCv5Sharp\.runtime|opencv5sharp\.runtime"
    },
    [pscustomobject]@{
        Name = "Consumer-facing docs must not point at the retired fixed-major root"
        Pattern = "OpenCV-CSharp-API-opencv5\.x"
    }
)

$compatibilityTokenPattern = "OpenCv5SharpBuildInfo|OpenCv5Sharp\.Native\.dll|OpenCv5SharpNativeRuntimeDir|OPENCV5SHARP_[A-Z0-9_]+|jyppx_ocv5_[A-Za-z0-9_]+|OpenCv5Sharp\."
$compatibilityContextPattern = "compatib|legacy|existing|older|already-compiled|source-compatible|historical|retired|previous|alias|facade|kept stable|not the primary|保留|兼容|旧|既有|已编译|历史|别名|早期|不再是|主"

function Get-RepositoryRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return ([System.IO.Path]::GetRelativePath($repo, $Path)) -replace "\\", "/"
}

function Test-IsIgnoredPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $relativePath = Get-RepositoryRelativePath -Path $Path
    $segments = $relativePath -split "/"
    foreach ($segment in $segments) {
        if ($ignoredDirectoryNames -contains $segment) {
            return $true
        }
    }

    return $false
}

function Get-ScannableFiles {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $alternateFullPath = $fullPath -replace '\.+$', ''
    if ($alternateFullPath -ne $fullPath -and (Test-Path -LiteralPath $alternateFullPath)) {
        $fullPath = $alternateFullPath
    }

    if (-not (Test-Path -LiteralPath $fullPath)) {
        return
    }

    $item = Get-Item -LiteralPath $fullPath
    if (-not $item.PSIsContainer) {
        if (-not (Test-IsIgnoredPath -Path $item.FullName)) {
            $item
        }

        return
    }

    foreach ($child in Get-ChildItem -LiteralPath $item.FullName -Recurse -File) {
        if (-not (Test-IsIgnoredPath -Path $child.FullName)) {
            $child
        }
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

$regexOptions = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
    [System.Text.RegularExpressions.RegexOptions]::Compiled
$compiledActiveLeakChecks = foreach ($check in $activeLeakChecks) {
    [pscustomobject]@{
        Name = $check.Name
        Regex = [System.Text.RegularExpressions.Regex]::new($check.Pattern, $regexOptions)
    }
}
$compatibilityTokenRegex = [System.Text.RegularExpressions.Regex]::new($compatibilityTokenPattern, $regexOptions)
$compatibilityContextRegex = [System.Text.RegularExpressions.Regex]::new($compatibilityContextPattern, $regexOptions)

$violations = [System.Collections.Generic.List[object]]::new()
$files = [System.Collections.Generic.List[object]]::new()

foreach ($relativePath in $scanRelativePaths) {
    $path = Join-Path $repo $relativePath
    foreach ($file in Get-ScannableFiles -Path $path) {
        $files.Add($file)
    }
}

$files = $files |
    Sort-Object FullName -Unique

$fixedMajorContextualReferenceCount = 0

foreach ($file in $files) {
    $relativePath = Get-RepositoryRelativePath -Path $file.FullName
    $lineNumber = 0

    try {
        foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
            $lineNumber++

            foreach ($check in $compiledActiveLeakChecks) {
                if ($check.Regex.IsMatch($line)) {
                    Add-Violation `
                        -Violations $violations `
                        -Path $relativePath `
                        -Line $lineNumber `
                        -Rule $check.Name `
                        -Text $line
                }
            }

            if ($compatibilityTokenRegex.IsMatch($line)) {
                $fixedMajorContextualReferenceCount++
                if (-not $compatibilityContextRegex.IsMatch($line)) {
                    Add-Violation `
                        -Violations $violations `
                        -Path $relativePath `
                        -Line $lineNumber `
                        -Rule "Fixed-major compatibility references must be explicitly labelled as compatibility, legacy, alias, facade, or existing-consumer surface" `
                        -Text $line
                }
            }
        }
    }
    catch [System.Text.DecoderFallbackException] {
        continue
    }
    catch [System.IO.IOException] {
        continue
    }
    catch [System.UnauthorizedAccessException] {
        continue
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Consumer-facing naming guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Line, Rule |
        Format-Table Path, Line, Rule, Text -AutoSize
    exit 1
}

Write-Host "Consumer-facing naming guard passed."
Write-Host "Consumer-facing files scanned: $($files.Count)."
Write-Host "Fixed-major compatibility references with explicit context: $fixedMajorContextualReferenceCount."
