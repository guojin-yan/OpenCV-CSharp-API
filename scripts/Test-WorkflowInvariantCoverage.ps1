param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$aggregateScriptToken = "scripts/Test-ProjectInvariants.ps1"
$sourceEvidenceScriptToken = "scripts/Initialize-UpstreamMapSourceEvidence.ps1"

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

function Read-RequiredText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required CI/release gate surface was not found: $RelativePath"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Normalize-CiText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    return $Text.Replace("\", "/")
}

function Get-TokenIndex {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Token
    )

    $normalizedText = Normalize-CiText -Text $Text
    $normalizedToken = (Normalize-CiText -Text $Token).TrimStart("./".ToCharArray())
    return $normalizedText.IndexOf($normalizedToken, [System.StringComparison]::OrdinalIgnoreCase)
}

function Assert-ContainsAggregateGate {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$RelativePath,
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    $gateIndex = Get-TokenIndex -Text $Text -Token $aggregateScriptToken
    if ($gateIndex -lt 0) {
        Add-Violation `
            -Violations $Violations `
            -Path $RelativePath `
            -Issue "Workflow or PR surface must reference $aggregateScriptToken"
    }

    return $gateIndex
}

$violations = [System.Collections.Generic.List[object]]::new()

$workflowRequirements = @(
    [pscustomobject]@{
        Path = ".github/workflows/build-managed.yml"
        MustRunBefore = @("dotnet restore", "dotnet build")
    },
    [pscustomobject]@{
        Path = ".github/workflows/pack.yml"
        MustRunBefore = @("dotnet restore", "dotnet build", "scripts/Pack-Managed.ps1", "scripts/Pack-Runtime.ps1")
    },
    [pscustomobject]@{
        Path = ".github/workflows/docs.yml"
        MustRunBefore = @("dotnet restore", "dotnet build", "dotnet tool restore", "dotnet tool run docfx ./docs/docfx.json", "upload-pages-artifact", "path: docs/_site")
    },
    [pscustomobject]@{
        Path = ".github/workflows/build-native.yml"
        MustRunBefore = @("cmakeArgs = @(", "& `$cmakeExe @cmakeArgs", "cmake --build", "ctest --test-dir")
    }
)

foreach ($requirement in $workflowRequirements) {
    $text = Read-RequiredText -RelativePath $requirement.Path
    $gateIndex = Assert-ContainsAggregateGate -Violations $violations -RelativePath $requirement.Path -Text $text

    if ($gateIndex -ge 0) {
        if ((Get-TokenIndex -Text $text -Token "Check project invariants") -lt 0) {
            Add-Violation `
                -Violations $violations `
                -Path $requirement.Path `
                -Issue "Workflow invariant step should be named 'Check project invariants'"
        }

        foreach ($token in $requirement.MustRunBefore) {
            $tokenIndex = Get-TokenIndex -Text $text -Token $token
            if ($tokenIndex -ge 0 -and $gateIndex -gt $tokenIndex) {
                Add-Violation `
                    -Violations $violations `
                    -Path $requirement.Path `
                    -Issue "Project invariant gate must run before '$token'"
            }
        }
    }
}

$sourceEvidenceCounts = [ordered]@{
    ".github/workflows/build-managed.yml" = 1
    ".github/workflows/build-native.yml" = 1
    ".github/workflows/docs.yml" = 1
    ".github/workflows/pack.yml" = 1
    ".github/workflows/runtime-input.yml" = 3
}
foreach ($entry in $sourceEvidenceCounts.GetEnumerator()) {
    $text = Normalize-CiText -Text (Read-RequiredText -RelativePath $entry.Key)
    $sourceToken = $sourceEvidenceScriptToken.TrimStart("./".ToCharArray())
    $aggregateToken = $aggregateScriptToken.TrimStart("./".ToCharArray())
    $sourceMatches = @([regex]::Matches($text, [regex]::Escape($sourceToken), [Text.RegularExpressions.RegexOptions]::IgnoreCase))
    $aggregateMatches = @([regex]::Matches($text, [regex]::Escape($aggregateToken), [Text.RegularExpressions.RegexOptions]::IgnoreCase))
    if ($sourceMatches.Count -ne $entry.Value -or $aggregateMatches.Count -ne $entry.Value) {
        Add-Violation -Violations $violations -Path $entry.Key -Issue "Every aggregate invariant invocation must have one upstream source-evidence bootstrap" -Text "expected=$($entry.Value) source=$($sourceMatches.Count) aggregate=$($aggregateMatches.Count)"
        continue
    }
    for ($index = 0; $index -lt $entry.Value; $index++) {
        if ($sourceMatches[$index].Index -gt $aggregateMatches[$index].Index) {
            Add-Violation -Violations $violations -Path $entry.Key -Issue "Upstream source evidence must be initialized before aggregate invariants" -Text "invocation=$($index + 1)"
        }
    }
}

$sourceEvidenceText = Read-RequiredText -RelativePath $sourceEvidenceScriptToken
foreach ($token in @(
        '40738fb16ceddb5fb3fea747585f7ce6abb0605b',
        'https://raw.githubusercontent.com/opencv/opencv/$openCvCommit/$relativePath',
        'Get-FileHash -LiteralPath $downloadPath -Algorithm SHA256',
        '@("imgproc", "imgcodecs", "videoio", "calib3d", "core", "dnn", "features", "objdetect", "photo", "video")',
        'opencv-source/opencv-5.0.0/')) {
    if ((Get-TokenIndex -Text $sourceEvidenceText -Token $token) -lt 0) {
        Add-Violation -Violations $violations -Path $sourceEvidenceScriptToken -Issue "Upstream source-evidence bootstrap must retain exact commit/path/hash contract" -Text $token
    }
}

$attributesPath = ".gitattributes"
$attributesText = Read-RequiredText -RelativePath $attributesPath
if ((Get-TokenIndex -Text $attributesText -Token '* text=auto eol=lf') -lt 0) {
    Add-Violation -Violations $violations -Path $attributesPath -Issue "Hosted checkouts must retain canonical LF text bytes" -Text '* text=auto eol=lf'
}

$buildManagedWorkflowPath = ".github/workflows/build-managed.yml"
$buildManagedWorkflowText = Read-RequiredText -RelativePath $buildManagedWorkflowPath
if ((Get-TokenIndex -Text $buildManagedWorkflowText -Token "dotnet test") -ge 0) {
    Add-Violation `
        -Violations $violations `
        -Path $buildManagedWorkflowPath `
        -Issue "Build-managed workflow must not run the full test suite without staged native runtime assets"
}

if ((Get-TokenIndex -Text (Read-RequiredText -RelativePath $aggregateScriptToken) -Token "scripts/Test-ManagedPackageStandaloneLocalConsumerCompile.ps1") -lt 0) {
    Add-Violation `
        -Violations $violations `
        -Path $aggregateScriptToken `
        -Issue "Aggregate invariant suite must verify the representative managed package consumer compile surface used by build-managed"
}

$packWorkflowText = Read-RequiredText -RelativePath ".github/workflows/pack.yml"
foreach ($token in @(
        "verify-artifacts:",
        "verify-consumers:",
        "uses: actions/download-artifact@",
        "scripts/Test-GitHubPackArtifactMatrixSurface.ps1",
        "scripts/Test-GitHubPackConsumerRestoreSurface.ps1",
        "artifacts/pack-download",
        "inputs.rid == 'all' && inputs.runtime_profile == 'all'")) {
    if ((Get-TokenIndex -Text $packWorkflowText -Token $token) -lt 0) {
        Add-Violation `
            -Violations $violations `
            -Path ".github/workflows/pack.yml" `
            -Issue "Pack workflow must keep full-matrix artifact self-validation wired through '$token'"
    }
}

$prTemplateText = Read-RequiredText -RelativePath ".github/pull_request_template.md"
[void](Assert-ContainsAggregateGate -Violations $violations -RelativePath ".github/pull_request_template.md" -Text $prTemplateText)

$fixedMajorReleaseSurfaceRegex = [System.Text.RegularExpressions.Regex]::new(
    "OpenCv5Sharp|opencv5sharp|OpenCV-CSharp-API-opencv5\.x",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

$githubPath = Join-Path $repo ".github"
$githubFiles = Get-ChildItem -LiteralPath $githubPath -Recurse -File |
    Where-Object { $_.Extension -in @(".yml", ".yaml", ".md") } |
    Sort-Object FullName

foreach ($file in $githubFiles) {
    $relativePath = Get-RepositoryRelativePath -Path $file.FullName
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
        $lineNumber++
        if ($fixedMajorReleaseSurfaceRegex.IsMatch($line)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "GitHub workflow, PR, issue, and release surfaces must not use fixed-major project identities" `
                -Text $line
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Workflow invariant coverage guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Line, Issue |
        Format-Table Path, Line, Issue, Text -AutoSize
    exit 1
}

Write-Host "Workflow invariant coverage guard passed."
Write-Host "Workflow gates checked: $($workflowRequirements.Count)."
Write-Host "GitHub surface files scanned: $($githubFiles.Count)."
