param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$fixedMajorIncludeTree = "open_cv_" + "5_sharp"
$retiredRootPattern = "OpenCV-CSharp-API-opencv" + "5\.x"
$fixedRuntimePackagePattern = "OpenCv" + "5Sharp\.runtime|opencv" + "5sharp\.runtime"

$ignoredDirectoryNames = @(
    ".git",
    "bin",
    "obj",
    "artifacts",
    "packages"
)

$ignoredRelativePrefixes = @(
    "docs/_site",
    "docs/api"
)

$allowedFixedMajorPathPrefixes = @(
    "src/OpenCvSharp.Native/include/$fixedMajorIncludeTree"
)

$contentIgnoredRelativePrefixes = @(
    "src/OpenCvSharp.Native/generated",
    "src/OpenCvSharp.Native/include/$fixedMajorIncludeTree"
)

$contentIgnoredRelativePaths = @(
    "scripts/Generate-NativeAbiCompatibility.ps1",
    "scripts/Test-NativeLegacyIncludeParity.ps1",
    "scripts/Test-VersionNeutralNaming.ps1",
    "scripts/Test-PathArtifactNaming.ps1"
)

$contextPattern = "factual|upstream|cache|install|source|runtime|artifact|fallback|existing|local|concrete|versioned|compatib|legacy|source-compatible|generated|allowlist|事实|上游|缓存|安装|源码|产物|既有|本地|兼容|生成|保留|版本"
$contextRegex = [System.Text.RegularExpressions.Regex]::new($contextPattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

function Get-RepositoryRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return ([System.IO.Path]::GetRelativePath($repo, $Path)) -replace "\\", "/"
}

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [int]$Line = 0,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Line = $Line
        Issue = $Issue
        Text = $Text.Trim()
    })
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

    foreach ($prefix in $ignoredRelativePrefixes) {
        if ($relativePath.Equals($prefix, [StringComparison]::OrdinalIgnoreCase) -or
            $relativePath.StartsWith($prefix + "/", [StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function Test-IsAllowedFixedMajorPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    foreach ($prefix in $allowedFixedMajorPathPrefixes) {
        if ($RelativePath.Equals($prefix, [StringComparison]::OrdinalIgnoreCase) -or
            $RelativePath.StartsWith($prefix + "/", [StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function Test-IsIgnoredContentPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    if ($contentIgnoredRelativePaths -contains $RelativePath) {
        return $true
    }

    foreach ($prefix in $contentIgnoredRelativePrefixes) {
        if ($RelativePath.Equals($prefix, [StringComparison]::OrdinalIgnoreCase) -or
            $RelativePath.StartsWith($prefix + "/", [StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function Test-HasRequiredContext {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Line
    )

    return $contextRegex.IsMatch($Line)
}

$violations = [System.Collections.Generic.List[object]]::new()
$pathItemCount = 0
$contentFileCount = 0
$contextualFixedPathReferenceCount = 0

$pathChecks = @(
    [pscustomobject]@{
        Name = "Repository paths must not use the retired fixed-major root"
        Regex = [System.Text.RegularExpressions.Regex]::new("(^|/)$retiredRootPattern($|/)", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    },
    [pscustomobject]@{
        Name = "Repository paths must not use fixed-major managed identity"
        Regex = [System.Text.RegularExpressions.Regex]::new("(^|/)OpenCv" + "5Sharp($|[._/-])", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    },
    [pscustomobject]@{
        Name = "Repository paths must not use fixed-major OpenCV 5 shorthand"
        Regex = [System.Text.RegularExpressions.Regex]::new("(^|/)opencv" + "5(?:\.x)?($|[._/-])", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    },
    [pscustomobject]@{
        Name = "Repository paths must not use fixed-major native include identity outside the compatibility include tree"
        Regex = [System.Text.RegularExpressions.Regex]::new("(^|/)" + [System.Text.RegularExpressions.Regex]::Escape($fixedMajorIncludeTree) + "($|/)", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    },
    [pscustomobject]@{
        Name = "Repository paths must not use fixed-major native ABI identity"
        Regex = [System.Text.RegularExpressions.Regex]::new("(^|/)jyppx_ocv" + "5[^/]*($|/)", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    }
)

foreach ($item in Get-ChildItem -LiteralPath $repo -Recurse -Force) {
    if (Test-IsIgnoredPath -Path $item.FullName) {
        continue
    }

    $pathItemCount++
    $relativePath = Get-RepositoryRelativePath -Path $item.FullName
    if (Test-IsAllowedFixedMajorPath -RelativePath $relativePath) {
        continue
    }

    foreach ($check in $pathChecks) {
        if ($check.Regex.IsMatch($relativePath)) {
            Add-Violation $violations $relativePath 0 $check.Name $relativePath
        }
    }
}

$contentFiles = Get-ChildItem -LiteralPath $repo -Recurse -File |
    Where-Object { -not (Test-IsIgnoredPath -Path $_.FullName) } |
    Where-Object {
        $relativePath = Get-RepositoryRelativePath -Path $_.FullName
        -not (Test-IsIgnoredContentPath -RelativePath $relativePath)
    } |
    Where-Object { $_.Extension -in @(".md", ".ps1", ".cs", ".csproj", ".props", ".targets", ".yml", ".yaml", ".slnx") } |
    Sort-Object FullName

foreach ($file in $contentFiles) {
    $contentFileCount++
    $relativePath = Get-RepositoryRelativePath -Path $file.FullName
    $lineNumber = 0

    try {
        foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
            $lineNumber++

            if ($line -match $retiredRootPattern) {
                Add-Violation $violations $relativePath $lineNumber "Content must not reference the retired fixed-major repository root" $line
            }

            if ($line -match $fixedRuntimePackagePattern) {
                Add-Violation $violations $relativePath $lineNumber "Content must not reference fixed-major runtime package identities" $line
            }

            if ($line -match "opencv" + "\d+-source code") {
                Add-Violation $violations $relativePath $lineNumber "Content must not name a concrete major-version local source-root fallback; describe the compatibility fallback generically or use opencv-source" $line
            }

            if ($line -match "opencv(?:_contrib)?-5\.0\.0") {
                $contextualFixedPathReferenceCount++
                if (-not (Test-HasRequiredContext -Line $line)) {
                    Add-Violation $violations $relativePath $lineNumber "Versioned OpenCV source/install path text must be explicitly labelled factual/upstream/cache/install/source" $line
                }
            }

            if ($line -match [System.Text.RegularExpressions.Regex]::Escape($fixedMajorIncludeTree)) {
                $contextualFixedPathReferenceCount++
                if (-not (Test-HasRequiredContext -Line $line)) {
                    Add-Violation $violations $relativePath $lineNumber "open_cv_5_sharp references must be explicitly labelled generated/source-compatible/compatibility" $line
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
    Write-Host "Path/artifact naming guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Line, Issue |
        Format-Table Path, Line, Issue, Text -AutoSize
    exit 1
}

Write-Host "Path/artifact naming guard passed."
Write-Host "Repository path items scanned: $pathItemCount."
Write-Host "Content files scanned: $contentFileCount."
Write-Host "Contextual fixed-version path references: $contextualFixedPathReferenceCount."
