param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$docfxPath = Join-Path $repo "docs/docfx.json"
$tocPath = Join-Path $repo "docs/toc.yml"
$docsWorkflowPath = Join-Path $repo ".github/workflows/docs.yml"
$gitignorePath = Join-Path $repo ".gitignore"

$managedProjectPath = "src/OpenCvSharp/OpenCvSharp.csproj"
$docfxMetadataDest = "api"
$docfxBuildDest = "_site"
$docsSitePath = "docs/_site"
$docsApiPath = "docs/api"

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
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Issue = $Issue
        Text = $Text.Trim()
    })
}

function Read-RequiredText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required documentation surface file was not found: $Path"
    }

    return [System.IO.File]::ReadAllText($Path)
}

function Test-ContainsText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Needle
    )

    return $Text.IndexOf($Needle, [System.StringComparison]::Ordinal) -ge 0
}

function Test-GeneratedDocsLeak {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Directory,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations
    )

    if (-not (Test-Path -LiteralPath $Directory -PathType Container)) {
        return 0
    }

    $filesScanned = 0
    $activeLeakPatterns = @(
        [pscustomobject]@{
            Issue = "Generated docs must not use the retired OpenCv5Sharp namespace as current API docs"
            Regex = [System.Text.RegularExpressions.Regex]::new("(^|[\s`'""<])(?:using|namespace)\s+OpenCv5Sharp(?:[.;\s{])")
        },
        [pscustomobject]@{
            Issue = "Generated docs must not publish OpenCv5Sharp.* as a current API namespace"
            Regex = [System.Text.RegularExpressions.Regex]::new("OpenCv5Sharp\.")
        },
        [pscustomobject]@{
            Issue = "Generated docs must not recommend fixed-major OpenCv5Sharp package installation"
            Regex = [System.Text.RegularExpressions.Regex]::new("dotnet\s+add\s+package\s+Open" + "Cv5Sharp\b")
        },
        [pscustomobject]@{
            Issue = "Generated docs must not reference fixed-major runtime package identities"
            Regex = [System.Text.RegularExpressions.Regex]::new("OpenCv5Sharp\.runtime|opencv5sharp\.runtime", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        },
        [pscustomobject]@{
            Issue = "Generated docs must not reference the retired fixed-major repository root"
            Regex = [System.Text.RegularExpressions.Regex]::new("OpenCV-CSharp-API-opencv" + "5\.x")
        }
    )

    foreach ($file in Get-ChildItem -LiteralPath $Directory -Recurse -File) {
        $filesScanned++
        $relativePath = Get-RepositoryRelativePath -Path $file.FullName
        try {
            $content = [System.IO.File]::ReadAllText($file.FullName)
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

        foreach ($pattern in $activeLeakPatterns) {
            if ($pattern.Regex.IsMatch($content)) {
                Add-Violation $Violations $relativePath $pattern.Issue
            }
        }
    }

    return $filesScanned
}

$violations = [System.Collections.Generic.List[object]]::new()

if (-not (Test-Path -LiteralPath $docfxPath -PathType Leaf)) {
    Add-Violation $violations "docs/docfx.json" "DocFX config must exist at docs/docfx.json"
}
else {
    $docfxText = Read-RequiredText -Path $docfxPath
    $docfx = $docfxText | ConvertFrom-Json

    $metadataProjectFiles = @($docfx.metadata.src.files)
    if ($metadataProjectFiles -notcontains $managedProjectPath) {
        Add-Violation $violations "docs/docfx.json" "DocFX metadata must read the version-neutral managed project $managedProjectPath"
    }

    if ($docfx.metadata.dest -ne $docfxMetadataDest) {
        Add-Violation $violations "docs/docfx.json" "DocFX metadata output must be '$docfxMetadataDest'"
    }

    if ($docfx.build.dest -ne $docfxBuildDest) {
        Add-Violation $violations "docs/docfx.json" "DocFX site output must be '$docfxBuildDest'"
    }

    $contentFiles = @($docfx.build.content.files)
    foreach ($requiredContent in @("articles/**.md", "api/**.yml", "api/index.md", "index.md", "toc.yml")) {
        if ($contentFiles -notcontains $requiredContent) {
            Add-Violation $violations "docs/docfx.json" "DocFX build content must include $requiredContent"
        }
    }

    if ($docfxText -match "OpenCv5Sharp\.|OpenCV-CSharp-API-opencv" + "5\.x|OpenCv5Sharp\.runtime|opencv5sharp\.runtime") {
        Add-Violation $violations "docs/docfx.json" "DocFX config must not contain fixed-major current documentation identities"
    }
}

$tocText = Read-RequiredText -Path $tocPath
if (-not (Test-ContainsText -Text $tocText -Needle "- name: API")) {
    Add-Violation $violations "docs/toc.yml" "Docs TOC must expose the generated API section"
}

if (-not (Test-ContainsText -Text $tocText -Needle "href: api/")) {
    Add-Violation $violations "docs/toc.yml" "Docs TOC API section must point at api/"
}

if ($tocText -match "OpenCv5Sharp\.|OpenCv5Sharp runtime|OpenCv5Sharp package") {
    Add-Violation $violations "docs/toc.yml" "Docs TOC must not present fixed-major names as current documentation entries"
}

$docsWorkflowText = Read-RequiredText -Path $docsWorkflowPath
foreach ($needle in @(
        "dotnet build .\src\OpenCvSharp\OpenCvSharp.csproj -c Release --no-restore",
        "docfx .\docs\docfx.json",
        "path: docs/_site")) {
    if (-not (Test-ContainsText -Text $docsWorkflowText -Needle $needle)) {
        Add-Violation $violations ".github/workflows/docs.yml" "Docs workflow must contain '$needle'"
    }
}

if ($docsWorkflowText -match "OpenCv5Sharp\.|OpenCV-CSharp-API-opencv" + "5\.x|OpenCv5Sharp\.runtime|opencv5sharp\.runtime") {
    Add-Violation $violations ".github/workflows/docs.yml" "Docs workflow must not contain fixed-major current documentation identities"
}

$gitignoreText = Read-RequiredText -Path $gitignorePath
foreach ($needle in @("docs/_site/", "docs/api/")) {
    if (-not (Test-ContainsText -Text $gitignoreText -Needle $needle)) {
        Add-Violation $violations ".gitignore" "Generated DocFX output path must be ignored: $needle"
    }
}

$apiGeneratedFilesScanned = Test-GeneratedDocsLeak -Directory (Join-Path $repo $docsApiPath) -Violations $violations
$siteGeneratedFilesScanned = Test-GeneratedDocsLeak -Directory (Join-Path $repo $docsSitePath) -Violations $violations

if ($violations.Count -gt 0) {
    Write-Host "Documentation surface neutrality guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Documentation surface neutrality guard passed."
Write-Host "DocFX metadata project: $managedProjectPath."
Write-Host "DocFX metadata output: $docfxMetadataDest."
Write-Host "DocFX site output: $docfxBuildDest."
Write-Host "Generated docs/api files scanned: $apiGeneratedFilesScanned."
Write-Host "Generated docs/_site files scanned: $siteGeneratedFilesScanned."
