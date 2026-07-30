param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$runtimeReadmeRelativePath = "packaging/runtime/JYPPX.OpenCV.runtime/README.md"
$oldRootIdentity = "OpenCV-CSharp-API-opencv" + "5.x"

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

function Read-RequiredText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required runtime documentation link file was not found: $RelativePath"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Assert-Contains {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Needle,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    if ($Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $Needle
    }
}

function Test-SkippableLinkTarget {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Target
    )

    return (
        [string]::IsNullOrWhiteSpace($Target) -or
        $Target.StartsWith("#", [System.StringComparison]::Ordinal) -or
        $Target -match "^[a-z][a-z0-9+.-]*:" -or
        $Target.StartsWith("mailto:", [System.StringComparison]::OrdinalIgnoreCase))
}

function Resolve-LinkTarget {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceRelativePath,
        [Parameter(Mandatory = $true)]
        [string]$RawTarget
    )

    $target = $RawTarget.Trim()
    if ($target.StartsWith("<", [System.StringComparison]::Ordinal) -and $target.EndsWith(">", [System.StringComparison]::Ordinal)) {
        $target = $target.Substring(1, $target.Length - 2)
    }

    $hashIndex = $target.IndexOf("#", [System.StringComparison]::Ordinal)
    if ($hashIndex -ge 0) {
        $target = $target.Substring(0, $hashIndex)
    }

    if (Test-SkippableLinkTarget -Target $target) {
        return $null
    }

    if ([System.IO.Path]::IsPathFullyQualified($target) -or $target.StartsWith("/", [System.StringComparison]::Ordinal)) {
        return [pscustomobject]@{
            Target = $target
            ResolvedPath = $target
            IsAbsolute = $true
            Exists = $false
            InsideRepo = $false
        }
    }

    $sourceDirectory = Split-Path -Parent (Join-Path $repo $SourceRelativePath)
    if ([string]::IsNullOrWhiteSpace($sourceDirectory)) {
        $sourceDirectory = $repo
    }

    $resolved = [System.IO.Path]::GetFullPath((Join-Path $sourceDirectory $target))
    $repoPrefix = $repo.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    $insideRepo = (
        $resolved.Equals($repo, [System.StringComparison]::OrdinalIgnoreCase) -or
        $resolved.StartsWith($repoPrefix, [System.StringComparison]::OrdinalIgnoreCase))

    return [pscustomobject]@{
        Target = $target
        ResolvedPath = $resolved
        IsAbsolute = $false
        Exists = (Test-Path -LiteralPath $resolved)
        InsideRepo = $insideRepo
    }
}

$auditedPaths = @(
    "README.md",
    "docs/articles/quick-start.md",
    "docs/articles/linked-runtime-build-guide.md",
    "docs/articles/linked-runtime-smoke-guide.md",
    "docs/articles/smoke-profiles-guide.md",
    "docs/articles/runtime-licenses.md",
    "docs/articles/native-module-boundary.md",
    "docs/articles/version-neutral-naming-guide.md",
    "docs/toc.yml",
    $runtimeReadmeRelativePath,
    ".github/ISSUE_TEMPLATE/bug_report.yml",
    "CONTRIBUTING.md"
)

$violations = [System.Collections.Generic.List[object]]::new()
$texts = @{}
foreach ($path in $auditedPaths) {
    $texts[$path] = Read-RequiredText -RelativePath $path
}

$markdownLinkRegex = [System.Text.RegularExpressions.Regex]::new("(?<!!)\[[^\]]+\]\((?<target>[^)]+)\)")
foreach ($path in $auditedPaths) {
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines((Join-Path $repo $path))) {
        $lineNumber++
        foreach ($match in $markdownLinkRegex.Matches($line)) {
            $rawTarget = $match.Groups["target"].Value
            $resolved = Resolve-LinkTarget -SourceRelativePath $path -RawTarget $rawTarget
            if ($null -eq $resolved) {
                continue
            }

            if ($resolved.IsAbsolute) {
                Add-Violation -Violations $violations -Path $path -Line $lineNumber -Issue "Runtime docs links must be relative, not absolute paths" -Text $rawTarget
                continue
            }

            if (-not $resolved.InsideRepo) {
                Add-Violation -Violations $violations -Path $path -Line $lineNumber -Issue "Runtime docs links must not escape the repository" -Text $rawTarget
                continue
            }

            if (-not $resolved.Exists) {
                Add-Violation -Violations $violations -Path $path -Line $lineNumber -Issue "Runtime docs Markdown link target must resolve from the source file directory" -Text $rawTarget
            }
        }
    }
}

$tocRuntimeHrefs = @(
    "articles/quick-start.md",
    "articles/version-neutral-naming-guide.md",
    "articles/native-module-boundary.md",
    "articles/linked-runtime-build-guide.md",
    "articles/smoke-profiles-guide.md",
    "articles/linked-runtime-smoke-guide.md",
    "articles/runtime-licenses.md"
)
foreach ($href in $tocRuntimeHrefs) {
    Assert-Contains -Violations $violations -Path "docs/toc.yml" -Text $texts["docs/toc.yml"] -Needle "href: $href" -Issue "Docs TOC runtime href must be present"
    $resolvedHref = Join-Path $repo (Join-Path "docs" $href)
    if (-not (Test-Path -LiteralPath $resolvedHref -PathType Leaf)) {
        Add-Violation -Violations $violations -Path "docs/toc.yml" -Issue "Docs TOC runtime href must resolve under docs/" -Text $href
    }
}

foreach ($path in @(
        "docs/articles/quick-start.md",
        "docs/articles/linked-runtime-build-guide.md",
        "docs/articles/runtime-licenses.md",
        "docs/articles/native-module-boundary.md",
        "docs/articles/version-neutral-naming-guide.md")) {
    Assert-Contains `
        -Violations $violations `
        -Path $path `
        -Text $texts[$path] `
        -Needle "https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/$runtimeReadmeRelativePath" `
        -Issue "$path must link to the canonical runtime package README source"
}

foreach ($needle in @(
        "docs/articles/quick-start.md",
        "docs/articles/linked-runtime-build-guide.md",
        "docs/articles/linked-runtime-smoke-guide.md",
        "docs/articles/smoke-profiles-guide.md",
        "docs/articles/runtime-licenses.md",
        $runtimeReadmeRelativePath)) {
    Assert-Contains -Violations $violations -Path "README.md" -Text $texts["README.md"] -Needle $needle -Issue "README runtime guidance links must resolve from repo root"
}

foreach ($needle in @(
        "../../../docs/articles/quick-start.md",
        "../../../docs/articles/linked-runtime-build-guide.md",
        "../../../docs/articles/linked-runtime-smoke-guide.md",
        "../../../docs/articles/smoke-profiles-guide.md",
        "../../../docs/articles/runtime-licenses.md")) {
    Assert-Contains -Violations $violations -Path $runtimeReadmeRelativePath -Text $texts[$runtimeReadmeRelativePath] -Needle $needle -Issue "Runtime package README must link back to docs using the package-readme relative prefix"
}

$issueTemplatePath = ".github/ISSUE_TEMPLATE/bug_report.yml"
$plainDocsPathRegex = [System.Text.RegularExpressions.Regex]::new("docs/articles/[A-Za-z0-9._/-]+\.md")
foreach ($match in $plainDocsPathRegex.Matches($texts[$issueTemplatePath])) {
    $target = $match.Value
    if (-not (Test-Path -LiteralPath (Join-Path $repo $target) -PathType Leaf)) {
        Add-Violation -Violations $violations -Path $issueTemplatePath -Issue "Issue template plain docs path must resolve from repo root" -Text $target
    }
}

foreach ($path in $auditedPaths) {
    if ($texts[$path].IndexOf($oldRootIdentity, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        Add-Violation -Violations $violations -Path $path -Issue "Runtime docs links must not use the old fixed-major repository root" -Text $oldRootIdentity
    }
}

foreach ($path in $auditedPaths) {
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines((Join-Path $repo $path))) {
        $lineNumber++
        if ($line.IndexOf("win-x64 runtime package README", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -and
            $line -notmatch "current|concrete|example|default|当前|具体|示例|默认") {
            Add-Violation -Violations $violations -Path $path -Line $lineNumber -Issue "win-x64 runtime package README link context must label it as current/concrete/example/default" -Text $line
        }
    }
}

Assert-Contains -Violations $violations -Path "docs/articles/version-neutral-naming-guide.md" -Text $texts["docs/articles/version-neutral-naming-guide.md"] -Needle "Test-RuntimeDocLinkIntegrity.ps1" -Issue "Version-neutral naming guide must list the runtime doc link integrity guard"
Assert-Contains -Violations $violations -Path "scripts/Test-ProjectInvariants.ps1" -Text (Read-RequiredText -RelativePath "scripts/Test-ProjectInvariants.ps1") -Needle "Test-RuntimeDocLinkIntegrity.ps1" -Issue "Aggregate invariant suite must include runtime doc link integrity guard"

if ($violations.Count -gt 0) {
    Write-Host "Runtime doc link integrity guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Line, Issue |
        Format-Table Path, Line, Issue, Text -AutoSize
    exit 1
}

Write-Host "Runtime doc link integrity guard passed."
Write-Host "Runtime docs entry points checked: $($auditedPaths.Count)."
Write-Host "Runtime TOC hrefs checked: $($tocRuntimeHrefs.Count)."
Write-Host "Runtime package README path: $runtimeReadmeRelativePath."
