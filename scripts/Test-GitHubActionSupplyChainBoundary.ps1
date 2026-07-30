param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workflowRoot = Join-Path $repo ".github/workflows"
if (-not (Test-Path -LiteralPath $workflowRoot -PathType Container)) {
    throw "GitHub workflow directory was not found: $workflowRoot"
}

# Update an action SHA only after independently resolving and auditing its official release tag.
$approvedActions = [ordered]@{
    "actions/checkout" = [pscustomobject]@{
        Sha = "11d5960a326750d5838078e36cf38b85af677262"
        Major = "v4"
    }
    "actions/setup-dotnet" = [pscustomobject]@{
        Sha = "67a3573c9a986a3f9c594539f4ab511d57bb3ce9"
        Major = "v4"
    }
    "actions/download-artifact" = [pscustomobject]@{
        Sha = "d3f86a106a0bac45b974a628896c90dbdf5c8093"
        Major = "v4"
    }
    "actions/upload-artifact" = [pscustomobject]@{
        Sha = "ea165f8d65b6e75b540449e92b4886f43607fa02"
        Major = "v4"
    }
    "actions/upload-pages-artifact" = [pscustomobject]@{
        Sha = "56afc609e74202658d3ffba0e8f6dda462b719fa"
        Major = "v3"
    }
    "actions/deploy-pages" = [pscustomobject]@{
        Sha = "d6db90164ac5ed86f2b6aed7e0febac5b3c0c03e"
        Major = "v4"
    }
}

function Get-RelativePath {
    param([Parameter(Mandatory)][string]$Path)

    return ([System.IO.Path]::GetRelativePath($repo, $Path)) -replace "\\", "/"
}

function Add-Violation {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory)][string]$Path,
        [int]$Line = 0,
        [Parameter(Mandatory)][string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Line = $Line
        Issue = $Issue
        Text = $Text.Trim()
    })
}

$workflowFiles = @(
    Get-ChildItem -LiteralPath $workflowRoot -File |
        Where-Object { $_.Extension -in @(".yml", ".yaml") } |
        Sort-Object FullName
)
if ($workflowFiles.Count -eq 0) {
    throw "No GitHub workflow YAML files were found under $workflowRoot"
}

$violations = [System.Collections.Generic.List[object]]::new()
$approvedUseCounts = @{}
foreach ($actionName in $approvedActions.Keys) {
    $approvedUseCounts[$actionName] = 0
}

$usesDeclarationCount = 0
$parsedUsesCount = 0
$localUseCount = 0
$dockerUseCount = 0
foreach ($workflowFile in $workflowFiles) {
    $relativePath = Get-RelativePath -Path $workflowFile.FullName
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($workflowFile.FullName)) {
        $lineNumber++
        if ($line -notmatch '^\s*(?:-\s*)?uses\s*:') {
            continue
        }

        $usesDeclarationCount++
        $match = [regex]::Match(
            $line,
            '^\s*(?:-\s*)?uses:\s*(?<reference>[^\s#]+)\s*(?:#\s*(?<comment>.*?))?\s*$')
        if (-not $match.Success) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "Workflow uses declaration must be a plain inspectable reference" `
                -Text $line
            continue
        }

        $parsedUsesCount++
        $reference = $match.Groups["reference"].Value
        $comment = $match.Groups["comment"].Value.Trim()
        if ($reference.StartsWith("./", [System.StringComparison]::Ordinal)) {
            $localUseCount++
            continue
        }
        if ($reference.StartsWith("docker://", [System.StringComparison]::OrdinalIgnoreCase)) {
            $dockerUseCount++
            if ($reference -notmatch '@sha256:[0-9a-fA-F]{64}$') {
                Add-Violation `
                    -Violations $violations `
                    -Path $relativePath `
                    -Line $lineNumber `
                    -Issue "Docker action references must use an immutable SHA256 digest" `
                    -Text $reference
            }
            continue
        }

        $separatorIndex = $reference.LastIndexOf("@", [System.StringComparison]::Ordinal)
        if ($separatorIndex -le 0 -or $separatorIndex -eq $reference.Length - 1) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "External action reference must contain an explicit revision" `
                -Text $reference
            continue
        }

        $actionName = $reference.Substring(0, $separatorIndex)
        $revision = $reference.Substring($separatorIndex + 1)
        if (-not $approvedActions.Contains($actionName)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "External action is not present in the audited allowlist" `
                -Text $actionName
            continue
        }

        $approved = $approvedActions[$actionName]
        if ($revision -notmatch '^[0-9a-fA-F]{40}$') {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "External action revision must be an immutable 40-character commit SHA" `
                -Text $reference
        }
        elseif (-not $revision.Equals($approved.Sha, [System.StringComparison]::OrdinalIgnoreCase)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "External action revision does not match the audited commit" `
                -Text "$reference expected=$($approved.Sha)"
        }

        if (-not $comment.Equals($approved.Major, [System.StringComparison]::Ordinal)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "Pinned external action must retain its audited major-version comment" `
                -Text "$reference comment='$comment' expected='$($approved.Major)'"
        }

        $approvedUseCounts[$actionName]++
    }
}

if ($usesDeclarationCount -ne $parsedUsesCount) {
    Add-Violation `
        -Violations $violations `
        -Path ".github/workflows" `
        -Issue "Every workflow uses declaration must be parsed exactly once" `
        -Text "declarations=$usesDeclarationCount parsed=$parsedUsesCount"
}

foreach ($actionName in $approvedActions.Keys) {
    if ($approvedUseCounts[$actionName] -eq 0) {
        Add-Violation `
            -Violations $violations `
            -Path ".github/workflows" `
            -Issue "Audited action allowlist entry is no longer used and must be removed or restored" `
            -Text $actionName
    }
}

if ($violations.Count -gt 0) {
    Write-Host "GitHub Action supply-chain boundary guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Line, Issue | Format-Table Path, Line, Issue, Text -AutoSize
    exit 1
}

Write-Host "GitHub Action supply-chain boundary guard passed."
Write-Host "Workflow files checked: $($workflowFiles.Count); uses declarations: $usesDeclarationCount; approved external actions: $($approvedActions.Count)."
Write-Host "Local actions: $localUseCount; digest-pinned Docker actions: $dockerUseCount."
