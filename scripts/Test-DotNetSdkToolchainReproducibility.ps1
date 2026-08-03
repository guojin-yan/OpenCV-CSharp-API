param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$globalJsonRelativePath = "global.json"
$aggregateRelativePath = "scripts/Test-ProjectInvariants.ps1"
$guardRelativePath = "scripts/Test-DotNetSdkToolchainReproducibility.ps1"
$expectedGlobalSdkVersion = "10.0.302"
$expectedSdkVersions = @("10.0.302", "9.0.316", "8.0.423")
$setupDotNetSha = "a98b56852c35b8e3190ac28c8c2271da59106c68"
$setupDotNetMajor = "v6"
$expectedWorkflowSetupCounts = [ordered]@{
    ".github/workflows/build-managed.yml" = 1
    ".github/workflows/build-native.yml" = 1
    ".github/workflows/docs.yml" = 1
    ".github/workflows/pack.yml" = 13
    ".github/workflows/publish-nuget.yml" = 5
    ".github/workflows/runtime-input.yml" = 4
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

    [void]$Violations.Add([pscustomobject]@{
        Path = $Path
        Line = $Line
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
            Add-Violation -Violations $Violations -Path $Path -Issue "$Context is missing a required key" -Text $key
        }
    }
    foreach ($key in $Object.Keys) {
        if ($key -notin $ExpectedKeys) {
            Add-Violation -Violations $Violations -Path $Path -Issue "$Context contains an unexpected key" -Text ([string]$key)
        }
    }
}

function Test-ExactSequence {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]]$Actual,
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]]$Expected
    )

    if ($Actual.Count -ne $Expected.Count) {
        return $false
    }
    for ($index = 0; $index -lt $Expected.Count; $index++) {
        if (-not $Actual[$index].Equals($Expected[$index], [System.StringComparison]::Ordinal)) {
            return $false
        }
    }
    return $true
}

$violations = [System.Collections.Generic.List[object]]::new()
$globalJsonPath = Join-Path $repo $globalJsonRelativePath
$globalJsonFiles = @(
    Get-ChildItem -LiteralPath $repo -Recurse -File -Filter "global.json" |
        Where-Object {
            $relativePath = Get-RelativePath -Path $_.FullName
            $relativePath -notmatch '(^|/)(?:\.git|bin|obj|artifacts|build|docs/api|docs/_site)/'
        }
)
$globalJsonRelativePaths = @($globalJsonFiles | ForEach-Object { Get-RelativePath -Path $_.FullName })
if ($globalJsonRelativePaths.Count -ne 1 -or $globalJsonRelativePaths[0] -ne $globalJsonRelativePath) {
    Add-Violation `
        -Violations $violations `
        -Path $globalJsonRelativePath `
        -Issue "Repository must contain exactly one root SDK selection file" `
        -Text ($globalJsonRelativePaths -join ", ")
}

if (-not (Test-Path -LiteralPath $globalJsonPath -PathType Leaf)) {
    Add-Violation -Violations $violations -Path $globalJsonRelativePath -Issue "Root global.json is missing"
}
else {
    $globalJsonBytes = [System.IO.File]::ReadAllBytes($globalJsonPath)
    if ($globalJsonBytes.Length -ge 3 -and
        $globalJsonBytes[0] -eq 0xEF -and
        $globalJsonBytes[1] -eq 0xBB -and
        $globalJsonBytes[2] -eq 0xBF) {
        Add-Violation -Violations $violations -Path $globalJsonRelativePath -Issue "global.json must be UTF-8 without BOM"
    }

    try {
        $globalJson = [System.IO.File]::ReadAllText($globalJsonPath) | ConvertFrom-Json -AsHashtable -Depth 20
    }
    catch {
        Add-Violation -Violations $violations -Path $globalJsonRelativePath -Issue "global.json must be valid JSON" -Text $_.Exception.Message
        $globalJson = $null
    }

    if ($null -ne $globalJson) {
        Assert-ExactKeys `
            -Object $globalJson `
            -ExpectedKeys @("sdk") `
            -Path $globalJsonRelativePath `
            -Context "global.json root" `
            -Violations $violations

        $sdk = $globalJson["sdk"]
        if ($sdk -isnot [System.Collections.IDictionary]) {
            Add-Violation -Violations $violations -Path $globalJsonRelativePath -Issue "global.json sdk value must be an object"
        }
        else {
            Assert-ExactKeys `
                -Object $sdk `
                -ExpectedKeys @("version", "rollForward") `
                -Path $globalJsonRelativePath `
                -Context "global.json sdk" `
                -Violations $violations

            $actualVersion = [string]$sdk["version"]
            if ($actualVersion -notmatch '^\d+\.\d+\.\d+$' -or
                -not $actualVersion.Equals($expectedGlobalSdkVersion, [System.StringComparison]::Ordinal)) {
                Add-Violation `
                    -Violations $violations `
                    -Path $globalJsonRelativePath `
                    -Issue "Repository SDK version must be the exact audited stable patch" `
                    -Text "actual=$actualVersion expected=$expectedGlobalSdkVersion"
            }

            $actualRollForward = [string]$sdk["rollForward"]
            if (-not $actualRollForward.Equals("disable", [System.StringComparison]::Ordinal)) {
                Add-Violation `
                    -Violations $violations `
                    -Path $globalJsonRelativePath `
                    -Issue "Repository SDK roll-forward must remain disabled" `
                    -Text "actual=$actualRollForward expected=disable"
            }
        }
    }
}

$workflowRoot = Join-Path $repo ".github/workflows"
if (-not (Test-Path -LiteralPath $workflowRoot -PathType Container)) {
    throw "GitHub workflow directory was not found: $workflowRoot"
}

$workflowFiles = @(
    Get-ChildItem -LiteralPath $workflowRoot -File |
        Where-Object { $_.Extension -in @(".yml", ".yaml") } |
        Sort-Object Name
)
$workflowRelativePaths = @($workflowFiles | ForEach-Object { Get-RelativePath -Path $_.FullName })
$expectedWorkflowPaths = @($expectedWorkflowSetupCounts.Keys)
if (-not (Test-ExactSequence -Actual $workflowRelativePaths -Expected ($expectedWorkflowPaths | Sort-Object))) {
    Add-Violation `
        -Violations $violations `
        -Path ".github/workflows" `
        -Issue "Workflow file set must remain exact for SDK setup auditing" `
        -Text "actual=$($workflowRelativePaths -join ', ') expected=$(($expectedWorkflowPaths | Sort-Object) -join ', ')"
}

$totalSetupCount = 0
$totalDotnetVersionDeclarationCount = 0
$directSdkInstallCount = 0
$wildcardPattern = '(?<![0-9])(?:8|9|10)\.0\.(?:x|\*)(?![0-9A-Za-z])'
foreach ($workflowRelativePath in $expectedWorkflowPaths) {
    $workflowPath = Join-Path $repo $workflowRelativePath
    if (-not (Test-Path -LiteralPath $workflowPath -PathType Leaf)) {
        Add-Violation -Violations $violations -Path $workflowRelativePath -Issue "Expected workflow file is missing"
        continue
    }

    $lines = [System.IO.File]::ReadAllLines($workflowPath)
    $setupLineIndexes = [System.Collections.Generic.List[int]]::new()
    $dotnetVersionDeclarationIndexes = [System.Collections.Generic.List[int]]::new()
    for ($lineIndex = 0; $lineIndex -lt $lines.Count; $lineIndex++) {
        $line = $lines[$lineIndex]
        if ($line -match '^\s*(?:-\s*)?uses:\s*actions/setup-dotnet@') {
            [void]$setupLineIndexes.Add($lineIndex)
        }
        if ($line -match '^\s*dotnet-version\s*:') {
            [void]$dotnetVersionDeclarationIndexes.Add($lineIndex)
        }
        if ($line -match $wildcardPattern) {
            Add-Violation `
                -Violations $violations `
                -Path $workflowRelativePath `
                -Line ($lineIndex + 1) `
                -Issue "Workflow SDK selections must not use patch wildcards" `
                -Text $line
        }
        if ($line -match '(?i)dotnet-install\.(?:sh|ps1).*?(?:--channel|-Channel)\s+') {
            Add-Violation `
                -Violations $violations `
                -Path $workflowRelativePath `
                -Line ($lineIndex + 1) `
                -Issue "Direct .NET SDK installation must use an exact version instead of a channel" `
                -Text $line
        }
        if ($line -match '(?i)^\s*(?:bash|pwsh|powershell)\s+\S*dotnet-install\.(?:sh|ps1)\b' -and
            $line -notmatch '(?i)(?:--runtime|-Runtime)\s+') {
            $directSdkInstallCount++
            $versionMatch = [regex]::Match($line, '(?i)(?:--version|-Version)\s+(?<version>\S+)')
            if (-not $versionMatch.Success -or $versionMatch.Groups["version"].Value -notin $expectedSdkVersions) {
                Add-Violation `
                    -Violations $violations `
                    -Path $workflowRelativePath `
                    -Line ($lineIndex + 1) `
                    -Issue "Direct .NET SDK installation must select an audited exact SDK version" `
                    -Text $line
            }
        }
    }

    $actualSetupCount = $setupLineIndexes.Count
    $expectedSetupCount = $expectedWorkflowSetupCounts[$workflowRelativePath]
    $totalSetupCount += $actualSetupCount
    $totalDotnetVersionDeclarationCount += $dotnetVersionDeclarationIndexes.Count
    if ($actualSetupCount -ne $expectedSetupCount) {
        Add-Violation `
            -Violations $violations `
            -Path $workflowRelativePath `
            -Issue "Workflow setup-dotnet use count must remain exact" `
            -Text "actual=$actualSetupCount expected=$expectedSetupCount"
    }
    if ($dotnetVersionDeclarationIndexes.Count -ne $actualSetupCount) {
        Add-Violation `
            -Violations $violations `
            -Path $workflowRelativePath `
            -Issue "Every setup-dotnet step must own exactly one dotnet-version declaration" `
            -Text "setups=$actualSetupCount declarations=$($dotnetVersionDeclarationIndexes.Count)"
    }

    foreach ($setupLineIndex in $setupLineIndexes) {
        $usesLine = $lines[$setupLineIndex]
        $usesMatch = [regex]::Match(
            $usesLine,
            '^(?<indent> +)- uses: actions/setup-dotnet@(?<revision>[^\s#]+)\s+#\s*(?<comment>\S+)\s*$')
        if (-not $usesMatch.Success) {
            Add-Violation `
                -Violations $violations `
                -Path $workflowRelativePath `
                -Line ($setupLineIndex + 1) `
                -Issue "setup-dotnet use must remain a plain inspectable pinned step" `
                -Text $usesLine
            continue
        }

        $revision = $usesMatch.Groups["revision"].Value
        if (-not $revision.Equals($setupDotNetSha, [System.StringComparison]::OrdinalIgnoreCase)) {
            Add-Violation `
                -Violations $violations `
                -Path $workflowRelativePath `
                -Line ($setupLineIndex + 1) `
                -Issue "setup-dotnet must retain the separately audited Action commit" `
                -Text "actual=$revision expected=$setupDotNetSha"
        }
        if (-not $usesMatch.Groups["comment"].Value.Equals($setupDotNetMajor, [System.StringComparison]::Ordinal)) {
            Add-Violation `
                -Violations $violations `
                -Path $workflowRelativePath `
                -Line ($setupLineIndex + 1) `
                -Issue "setup-dotnet must retain its audited major-version comment" `
                -Text $usesLine
        }

        $stepIndent = $usesMatch.Groups["indent"].Value.Length
        $stepEndIndex = $lines.Count
        for ($candidateIndex = $setupLineIndex + 1; $candidateIndex -lt $lines.Count; $candidateIndex++) {
            $siblingMatch = [regex]::Match($lines[$candidateIndex], '^(?<indent> *)-\s+')
            if ($siblingMatch.Success -and $siblingMatch.Groups["indent"].Value.Length -eq $stepIndent) {
                $stepEndIndex = $candidateIndex
                break
            }
        }

        $withPrefix = " " * ($stepIndent + 2)
        $versionPrefix = " " * ($stepIndent + 4)
        $withPattern = '^' + [regex]::Escape($withPrefix) + 'with:\s*$'
        $versionPattern = '^' + [regex]::Escape($versionPrefix) + 'dotnet-version:\s+\|\s*$'
        $withIndexes = @()
        $versionIndexes = @()
        for ($candidateIndex = $setupLineIndex + 1; $candidateIndex -lt $stepEndIndex; $candidateIndex++) {
            if ($lines[$candidateIndex] -match $withPattern) {
                $withIndexes += $candidateIndex
            }
            if ($lines[$candidateIndex] -match $versionPattern) {
                $versionIndexes += $candidateIndex
            }
        }

        if ($withIndexes.Count -ne 1 -or $versionIndexes.Count -ne 1 -or $versionIndexes[0] -le $withIndexes[0]) {
            Add-Violation `
                -Violations $violations `
                -Path $workflowRelativePath `
                -Line ($setupLineIndex + 1) `
                -Issue "setup-dotnet must bind one literal dotnet-version block beneath one with map" `
                -Text "with=$($withIndexes.Count) literal_version_blocks=$($versionIndexes.Count)"
            continue
        }

        $versionLineIndex = $versionIndexes[0]
        $versionIndent = $versionPrefix.Length
        $actualVersions = [System.Collections.Generic.List[string]]::new()
        for ($candidateIndex = $versionLineIndex + 1; $candidateIndex -lt $stepEndIndex; $candidateIndex++) {
            $candidateLine = $lines[$candidateIndex]
            if ([string]::IsNullOrWhiteSpace($candidateLine)) {
                continue
            }
            $indentMatch = [regex]::Match($candidateLine, '^(?<indent> *)')
            if ($indentMatch.Groups["indent"].Value.Length -le $versionIndent) {
                break
            }
            [void]$actualVersions.Add($candidateLine.Trim())
        }

        if (-not (Test-ExactSequence -Actual @($actualVersions) -Expected $expectedSdkVersions)) {
            Add-Violation `
                -Violations $violations `
                -Path $workflowRelativePath `
                -Line ($versionLineIndex + 1) `
                -Issue "setup-dotnet SDK list must contain only the audited exact versions in canonical order" `
                -Text "actual=$($actualVersions -join ', ') expected=$($expectedSdkVersions -join ', ')"
        }
    }
}

if ($totalSetupCount -ne 25 -or $totalDotnetVersionDeclarationCount -ne 25) {
    Add-Violation `
        -Violations $violations `
        -Path ".github/workflows" `
        -Issue "Repository workflow SDK setup surface must remain exactly 25 bound blocks" `
        -Text "setups=$totalSetupCount declarations=$totalDotnetVersionDeclarationCount"
}
if ($directSdkInstallCount -ne 2) {
    Add-Violation `
        -Violations $violations `
        -Path ".github/workflows/pack.yml" `
        -Issue "ARM64 target-container direct SDK installs must remain exactly two audited exact-version commands" `
        -Text "actual=$directSdkInstallCount expected=2"
}

$scriptRoot = Join-Path $repo "scripts"
if (Test-Path -LiteralPath $scriptRoot -PathType Container) {
    foreach ($scriptFile in Get-ChildItem -LiteralPath $scriptRoot -Recurse -File -Filter "*.ps1") {
        $scriptRelativePath = Get-RelativePath -Path $scriptFile.FullName
        if ($scriptRelativePath -eq $guardRelativePath) {
            continue
        }
        $lineNumber = 0
        foreach ($line in [System.IO.File]::ReadLines($scriptFile.FullName)) {
            $lineNumber++
            if ($line -match $wildcardPattern) {
                Add-Violation `
                    -Violations $violations `
                    -Path $scriptRelativePath `
                    -Line $lineNumber `
                    -Issue "Repository guards and scripts must not preserve floating SDK patch selections" `
                    -Text $line
            }
            if ($line -match '(?i)dotnet-install\.(?:sh|ps1).*?(?:--channel|-Channel)\s+') {
                Add-Violation `
                    -Violations $violations `
                    -Path $scriptRelativePath `
                    -Line $lineNumber `
                    -Issue "Repository scripts must not install .NET SDKs from a floating channel" `
                    -Text $line
            }
        }
    }
}

$aggregatePath = Join-Path $repo $aggregateRelativePath
if (-not (Test-Path -LiteralPath $aggregatePath -PathType Leaf)) {
    Add-Violation -Violations $violations -Path $aggregateRelativePath -Issue "Project invariant aggregate is missing"
}
else {
    $aggregateText = [System.IO.File]::ReadAllText($aggregatePath)
    $guardRegistrationCount = [regex]::Matches($aggregateText, [regex]::Escape($guardRelativePath)).Count
    $guardNameCount = [regex]::Matches($aggregateText, 'Name\s*=\s*"\.NET SDK toolchain reproducibility"').Count
    if ($guardRegistrationCount -ne 1 -or $guardNameCount -ne 1) {
        Add-Violation `
            -Violations $violations `
            -Path $aggregateRelativePath `
            -Issue "SDK toolchain reproducibility guard must be registered exactly once" `
            -Text "path_registrations=$guardRegistrationCount name_registrations=$guardNameCount"
    }
}

if ($violations.Count -gt 0) {
    Write-Host ".NET SDK toolchain reproducibility guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Line, Issue | Format-Table Path, Line, Issue, Text -AutoSize
    exit 1
}

Write-Host ".NET SDK toolchain reproducibility guard passed."
Write-Host "Repository SDK: $expectedGlobalSdkVersion (roll-forward disabled)."
Write-Host "Workflow setup blocks: $totalSetupCount across $($expectedWorkflowPaths.Count) workflows; direct exact SDK installs: $directSdkInstallCount."
Write-Host "Exact workflow SDKs: $($expectedSdkVersions -join ', ')."
