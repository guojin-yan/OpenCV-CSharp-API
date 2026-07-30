param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$manifestRelativePath = ".config/dotnet-tools.json"
$workflowRelativePath = ".github/workflows/docs.yml"
$aggregateRelativePath = "scripts/Test-ProjectInvariants.ps1"
$guardRelativePath = "scripts/Test-DocumentationToolchainReproducibility.ps1"
$docfxVersion = "2.78.5"

function Read-RequiredText {
    param([Parameter(Mandatory)][string]$RelativePath)

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required documentation toolchain file was not found: $RelativePath"
    }
    return [System.IO.File]::ReadAllText($path)
}

function Get-RelativePath {
    param([Parameter(Mandatory)][string]$Path)

    return ([System.IO.Path]::GetRelativePath($repo, $Path)) -replace "\\", "/"
}

function Add-Violation {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Issue,
        [string]$Text = ""
    )

    [void]$Violations.Add([pscustomobject]@{
        Path = $Path
        Issue = $Issue
        Text = $Text.Trim()
    })
}

function Assert-ExactKeys {
    param(
        [Parameter(Mandatory)][System.Collections.IDictionary]$Object,
        [Parameter(Mandatory)][string[]]$ExpectedKeys,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Context,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations
    )

    foreach ($key in $ExpectedKeys) {
        if (-not $Object.Contains($key)) {
            Add-Violation `
                -Violations $Violations `
                -Path $Path `
                -Issue "$Context is missing required key" `
                -Text $key
        }
    }
    foreach ($key in $Object.Keys) {
        if ($key -notin $ExpectedKeys) {
            Add-Violation `
                -Violations $Violations `
                -Path $Path `
                -Issue "$Context contains an unexpected key" `
                -Text ([string]$key)
        }
    }
}

function Assert-Contains {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$Needle,
        [Parameter(Mandatory)][string]$Issue
    )

    if ($Text.IndexOf($Needle, [System.StringComparison]::Ordinal) -lt 0) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $Needle
    }
}

$violations = [System.Collections.Generic.List[object]]::new()
$manifestPath = Join-Path $repo $manifestRelativePath
$manifestFiles = @(
    Get-ChildItem -LiteralPath $repo -Recurse -File -Filter "dotnet-tools.json" |
        Where-Object {
            $relativePath = Get-RelativePath -Path $_.FullName
            $relativePath -notmatch '(^|/)(?:bin|obj|artifacts|build|\.git)/'
        }
)
$manifestRelativePaths = @($manifestFiles | ForEach-Object { Get-RelativePath -Path $_.FullName })
if ($manifestRelativePaths.Count -ne 1 -or $manifestRelativePaths[0] -ne $manifestRelativePath) {
    Add-Violation `
        -Violations $violations `
        -Path $manifestRelativePath `
        -Issue "Repository must contain exactly one root local tool manifest" `
        -Text ($manifestRelativePaths -join ", ")
}

if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    Add-Violation `
        -Violations $violations `
        -Path $manifestRelativePath `
        -Issue "Exact local DocFX tool manifest is missing"
}
else {
    $manifestBytes = [System.IO.File]::ReadAllBytes($manifestPath)
    if ($manifestBytes.Length -ge 3 -and
        $manifestBytes[0] -eq 0xEF -and
        $manifestBytes[1] -eq 0xBB -and
        $manifestBytes[2] -eq 0xBF) {
        Add-Violation `
            -Violations $violations `
            -Path $manifestRelativePath `
            -Issue "Local tool manifest must be UTF-8 without BOM"
    }

    $manifestText = [System.IO.File]::ReadAllText($manifestPath)
    try {
        $manifest = $manifestText | ConvertFrom-Json -AsHashtable -Depth 20
    }
    catch {
        Add-Violation `
            -Violations $violations `
            -Path $manifestRelativePath `
            -Issue "Local tool manifest must be valid JSON" `
            -Text $_.Exception.Message
        $manifest = $null
    }

    if ($null -ne $manifest) {
        Assert-ExactKeys `
            -Object $manifest `
            -ExpectedKeys @("version", "isRoot", "tools") `
            -Path $manifestRelativePath `
            -Context "Tool manifest root" `
            -Violations $violations

        if ($manifest["version"] -ne 1) {
            Add-Violation -Violations $violations -Path $manifestRelativePath -Issue "Tool manifest schema version must be 1" -Text ([string]$manifest["version"])
        }
        if ($manifest["isRoot"] -ne $true) {
            Add-Violation -Violations $violations -Path $manifestRelativePath -Issue "Tool manifest must stop parent-directory manifest discovery" -Text "isRoot=$($manifest['isRoot'])"
        }

        $tools = $manifest["tools"]
        if ($tools -isnot [System.Collections.IDictionary]) {
            Add-Violation -Violations $violations -Path $manifestRelativePath -Issue "Tool manifest tools value must be an object"
        }
        else {
            Assert-ExactKeys `
                -Object $tools `
                -ExpectedKeys @("docfx") `
                -Path $manifestRelativePath `
                -Context "Tool manifest tools" `
                -Violations $violations

            $docfx = $tools["docfx"]
            if ($docfx -isnot [System.Collections.IDictionary]) {
                Add-Violation -Violations $violations -Path $manifestRelativePath -Issue "DocFX tool entry must be an object"
            }
            else {
                Assert-ExactKeys `
                    -Object $docfx `
                    -ExpectedKeys @("version", "commands", "rollForward") `
                    -Path $manifestRelativePath `
                    -Context "DocFX tool entry" `
                    -Violations $violations

                if ($docfx["version"] -ne $docfxVersion) {
                    Add-Violation -Violations $violations -Path $manifestRelativePath -Issue "DocFX package version must be exact" -Text "actual=$($docfx['version']) expected=$docfxVersion"
                }
                $commands = @($docfx["commands"])
                if ($commands.Count -ne 1 -or $commands[0] -ne "docfx") {
                    Add-Violation -Violations $violations -Path $manifestRelativePath -Issue "DocFX manifest must expose exactly the docfx command" -Text ($commands -join ", ")
                }
                if ($docfx["rollForward"] -ne $false) {
                    Add-Violation -Violations $violations -Path $manifestRelativePath -Issue "DocFX tool roll-forward must remain disabled" -Text "rollForward=$($docfx['rollForward'])"
                }
            }
        }
    }
}

$workflowText = Read-RequiredText -RelativePath $workflowRelativePath
$restoreCommand = "run: dotnet tool restore"
$buildCommand = "run: dotnet tool run docfx ./docs/docfx.json"
foreach ($required in @(
        "name: Restore documentation tools",
        $restoreCommand,
        "name: Build docfx site",
        $buildCommand)) {
    Assert-Contains `
        -Violations $violations `
        -Path $workflowRelativePath `
        -Text $workflowText `
        -Needle $required `
        -Issue "Docs workflow must use the repository-local exact DocFX toolchain"
}

$restoreIndex = $workflowText.IndexOf($restoreCommand, [System.StringComparison]::Ordinal)
$buildIndex = $workflowText.IndexOf($buildCommand, [System.StringComparison]::Ordinal)
if ($restoreIndex -lt 0 -or $buildIndex -lt 0 -or $restoreIndex -ge $buildIndex) {
    Add-Violation `
        -Violations $violations `
        -Path $workflowRelativePath `
        -Issue "Local tool restore must occur before the DocFX build" `
        -Text "$restoreCommand before $buildCommand"
}

foreach ($forbidden in @(
        '(?im)^\s*run:\s*dotnet\s+tool\s+(?:install|update)\b',
        '(?im)^\s*run:\s*docfx\b',
        '(?im)^\s*run:\s*dotnet\s+docfx\b',
        '(?im)^\s*run:.*dotnet\s+tool.*(?:\s-g(?:\s|$)|--global\b)')) {
    if ([regex]::IsMatch($workflowText, $forbidden)) {
        Add-Violation `
            -Violations $violations `
            -Path $workflowRelativePath `
            -Issue "Docs workflow must not install or invoke a floating/global DocFX tool" `
            -Text $forbidden
    }
}

$aggregateText = Read-RequiredText -RelativePath $aggregateRelativePath
Assert-Contains `
    -Violations $violations `
    -Path $aggregateRelativePath `
    -Text $aggregateText `
    -Needle $guardRelativePath `
    -Issue "Aggregate invariant suite must include the documentation toolchain reproducibility guard"

if ($violations.Count -gt 0) {
    Write-Host "Documentation toolchain reproducibility guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Issue | Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Documentation toolchain reproducibility guard passed."
Write-Host "DocFX package: docfx $docfxVersion; local command: dotnet tool run docfx."
Write-Host "Tool manifests checked: $($manifestRelativePaths.Count); floating/global DocFX workflow commands: 0."
