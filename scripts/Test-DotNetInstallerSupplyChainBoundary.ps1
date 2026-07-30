param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$packWorkflowRelativePath = ".github/workflows/pack.yml"
$aggregateRelativePath = "scripts/Test-ProjectInvariants.ps1"
$guardRelativePath = "scripts/Test-DotNetInstallerSupplyChainBoundary.ps1"
$installerCommit = "9c8552ae791982d3674fbb4d6ad887e536e7f506"
$powerShellInstallerUrl = "https://raw.githubusercontent.com/dotnet/install-scripts/$installerCommit/src/dotnet-install.ps1"
$bashInstallerUrl = "https://raw.githubusercontent.com/dotnet/install-scripts/$installerCommit/src/dotnet-install.sh"
$powerShellInstallerSha256 = "BB1CE92F4397E24D4736A4658B9728FB8F9DB64A0D3F8E636BA408A866A6661D"
$bashInstallerSha256 = "082F7685E156738A1B2E2ED8381A621870D4CE8E8C59278034556F05C186EB2E"
$expectedWorkflowPaths = @(
    ".github/workflows/build-managed.yml",
    ".github/workflows/build-native.yml",
    ".github/workflows/docs.yml",
    ".github/workflows/pack.yml",
    ".github/workflows/runtime-input.yml"
)

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

function Get-LineNumber {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][int]$Index
    )

    if ($Index -lt 0) {
        return 0
    }
    return ([regex]::Matches($Text.Substring(0, $Index), "\n").Count + 1)
}

function Get-TokenIndexes {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$Token
    )

    $indexes = [System.Collections.Generic.List[int]]::new()
    $offset = 0
    while ($offset -le $Text.Length - $Token.Length) {
        $index = $Text.IndexOf($Token, $offset, [System.StringComparison]::Ordinal)
        if ($index -lt 0) {
            break
        }
        [void]$indexes.Add($index)
        $offset = $index + $Token.Length
    }
    return @($indexes)
}

function Assert-TokenOnce {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$Token,
        [Parameter(Mandatory)][string]$Issue,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations
    )

    $indexes = @(Get-TokenIndexes -Text $Text -Token $Token)
    if ($indexes.Count -ne 1) {
        Add-Violation `
            -Violations $Violations `
            -Path $Path `
            -Line $(if ($indexes.Count -gt 0) { Get-LineNumber -Text $Text -Index $indexes[0] } else { 0 }) `
            -Issue $Issue `
            -Text "$Token (expected 1, found $($indexes.Count))"
        return -1
    }
    return $indexes[0]
}

function Get-WorkflowJobText {
    param(
        [Parameter(Mandatory)][string]$WorkflowText,
        [Parameter(Mandatory)][string]$JobName,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations
    )

    $pattern = "(?ms)^  $([regex]::Escape($JobName)):\r?\n.*?(?=^  [A-Za-z0-9_-]+:\r?\n|\z)"
    $matches = [regex]::Matches($WorkflowText, $pattern)
    if ($matches.Count -ne 1) {
        Add-Violation `
            -Violations $Violations `
            -Path $packWorkflowRelativePath `
            -Issue "Installer-owning workflow job must exist exactly once" `
            -Text "$JobName count=$($matches.Count)"
        return ""
    }
    return $matches[0].Value
}

function Assert-OrderedIndexes {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][int[]]$Indexes,
        [Parameter(Mandatory)][string]$Issue,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations
    )

    if (@($Indexes | Where-Object { $_ -lt 0 }).Count -gt 0) {
        return
    }
    for ($index = 1; $index -lt $Indexes.Count; $index++) {
        if ($Indexes[$index - 1] -ge $Indexes[$index]) {
            Add-Violation `
                -Violations $Violations `
                -Path $Path `
                -Line (Get-LineNumber -Text $Text -Index $Indexes[$index]) `
                -Issue $Issue `
                -Text ($Indexes -join " < ")
            return
        }
    }
}

$violations = [System.Collections.Generic.List[object]]::new()
$workflowRoot = Join-Path $repo ".github/workflows"
if (-not (Test-Path -LiteralPath $workflowRoot -PathType Container)) {
    throw "GitHub workflow directory was not found: $workflowRoot"
}

$workflowFiles = @(
    Get-ChildItem -LiteralPath $workflowRoot -File |
        Where-Object { $_.Extension -in @(".yml", ".yaml") } |
        Sort-Object Name
)
$actualWorkflowPaths = @($workflowFiles | ForEach-Object { Get-RelativePath -Path $_.FullName })
if (($actualWorkflowPaths -join "`n") -cne (($expectedWorkflowPaths | Sort-Object) -join "`n")) {
    Add-Violation `
        -Violations $violations `
        -Path ".github/workflows" `
        -Issue "Workflow set must remain exact for installer source auditing" `
        -Text "actual=$($actualWorkflowPaths -join ', ') expected=$(($expectedWorkflowPaths | Sort-Object) -join ', ')"
}

$workflowTexts = [ordered]@{}
foreach ($workflowFile in $workflowFiles) {
    $workflowRelativePath = Get-RelativePath -Path $workflowFile.FullName
    $workflowTexts[$workflowRelativePath] = [System.IO.File]::ReadAllText($workflowFile.FullName)
}

$packWorkflowPath = Join-Path $repo $packWorkflowRelativePath
if (-not (Test-Path -LiteralPath $packWorkflowPath -PathType Leaf)) {
    throw "Pack workflow was not found: $packWorkflowPath"
}
$packWorkflowText = [System.IO.File]::ReadAllText($packWorkflowPath)

$approvedUrls = @($powerShellInstallerUrl, $bashInstallerUrl)
$installerUrlPattern = 'https?://[^\s"'']*dotnet-install\.(?:ps1|sh)'
$allWorkflowUrlMatches = [System.Collections.Generic.List[object]]::new()
foreach ($workflowRelativePath in $workflowTexts.Keys) {
    $workflowText = $workflowTexts[$workflowRelativePath]
    foreach ($match in [regex]::Matches($workflowText, $installerUrlPattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
        [void]$allWorkflowUrlMatches.Add([pscustomobject]@{
            Path = $workflowRelativePath
            Index = $match.Index
            Value = $match.Value
        })
        if ($workflowRelativePath -ne $packWorkflowRelativePath) {
            Add-Violation `
                -Violations $violations `
                -Path $workflowRelativePath `
                -Line (Get-LineNumber -Text $workflowText -Index $match.Index) `
                -Issue "Executable .NET installer acquisition is allowed only in the audited pack verifier jobs" `
                -Text $match.Value
        }
        if ($match.Value -cnotin $approvedUrls) {
            Add-Violation `
                -Violations $violations `
                -Path $workflowRelativePath `
                -Line (Get-LineNumber -Text $workflowText -Index $match.Index) `
                -Issue "Installer source URL must be the exact audited official commit" `
                -Text $match.Value
        }
    }
}
if ($allWorkflowUrlMatches.Count -ne 3) {
    Add-Violation `
        -Violations $violations `
        -Path ".github/workflows" `
        -Issue "Repository workflows must contain exactly three audited installer acquisitions" `
        -Text "actual=$($allWorkflowUrlMatches.Count) expected=3"
}

$forbiddenSourcePatterns = @(
    '(?i)https://dot\.net/v1/dotnet-install\.(?:ps1|sh)',
    '(?i)https://builds\.dotnet\.microsoft\.com/dotnet/scripts/v1/dotnet-install\.(?:ps1|sh)',
    '(?i)raw\.githubusercontent\.com/dotnet/install-scripts/(?:main|master|refs/heads/[^/]+|refs/tags/[^/]+|v\d[^/]*)/src/dotnet-install\.(?:ps1|sh)'
)
foreach ($workflowRelativePath in $workflowTexts.Keys) {
    $workflowText = $workflowTexts[$workflowRelativePath]
    foreach ($pattern in $forbiddenSourcePatterns) {
        foreach ($match in [regex]::Matches($workflowText, $pattern)) {
            Add-Violation `
                -Violations $violations `
                -Path $workflowRelativePath `
                -Line (Get-LineNumber -Text $workflowText -Index $match.Index) `
                -Issue "Mutable, redirected, branch-based, or tag-based installer source is forbidden" `
                -Text $match.Value
        }
    }
}

$scriptRoot = Join-Path $repo "scripts"
if (Test-Path -LiteralPath $scriptRoot -PathType Container) {
    foreach ($scriptFile in Get-ChildItem -LiteralPath $scriptRoot -Recurse -File -Filter "*.ps1") {
        $scriptRelativePath = Get-RelativePath -Path $scriptFile.FullName
        if ($scriptRelativePath -eq $guardRelativePath) {
            continue
        }
        $scriptText = [System.IO.File]::ReadAllText($scriptFile.FullName)
        foreach ($pattern in $forbiddenSourcePatterns) {
            foreach ($match in [regex]::Matches($scriptText, $pattern)) {
                Add-Violation `
                    -Violations $violations `
                    -Path $scriptRelativePath `
                    -Line (Get-LineNumber -Text $scriptText -Index $match.Index) `
                    -Issue "Repository scripts must not retain a mutable installer source" `
                    -Text $match.Value
            }
        }
        foreach ($match in [regex]::Matches($scriptText, $installerUrlPattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
            if ($match.Value -cnotin $approvedUrls) {
                Add-Violation `
                    -Violations $violations `
                    -Path $scriptRelativePath `
                    -Line (Get-LineNumber -Text $scriptText -Index $match.Index) `
                    -Issue "Repository script installer references must use only the audited commit-pinned source" `
                    -Text $match.Value
            }
        }
    }
}

$windowsJobName = "verify-targeted-real-windows-x86"
$windowsJobText = Get-WorkflowJobText -WorkflowText $packWorkflowText -JobName $windowsJobName -Violations $violations
$windowsPath = "$packWorkflowRelativePath#$windowsJobName"
$windowsExpectedHashToken = "`$expectedInstallScriptSha256 = '$powerShellInstallerSha256'"
$windowsDownloadToken = "Invoke-WebRequest -Uri $powerShellInstallerUrl -OutFile `$installScript"
$windowsActualHashToken = '`$installScriptSha256 = (Get-FileHash -LiteralPath $installScript -Algorithm SHA256).Hash'.Replace('`$', '$')
$windowsCompareToken = 'if (-not $installScriptSha256.Equals($expectedInstallScriptSha256, [System.StringComparison]::OrdinalIgnoreCase)) {'
$windowsThrowToken = 'throw "The commit-pinned dotnet-install.ps1 payload hash does not match the audited source.'
$windowsExecuteToken = '& $installScript -Runtime dotnet -Version $runtimeVersion -Architecture x86 -InstallDir $installRoot -NoPath'
$windowsIndexes = @(
    Assert-TokenOnce -Path $windowsPath -Text $windowsJobText -Token $windowsExpectedHashToken -Issue "Windows verifier must bind the exact expected PowerShell installer hash once" -Violations $violations
    Assert-TokenOnce -Path $windowsPath -Text $windowsJobText -Token $windowsDownloadToken -Issue "Windows verifier must download the exact commit-pinned PowerShell installer once" -Violations $violations
    Assert-TokenOnce -Path $windowsPath -Text $windowsJobText -Token $windowsActualHashToken -Issue "Windows verifier must compute the downloaded installer SHA256 once" -Violations $violations
    Assert-TokenOnce -Path $windowsPath -Text $windowsJobText -Token $windowsCompareToken -Issue "Windows verifier must compare actual and expected installer hashes once" -Violations $violations
    Assert-TokenOnce -Path $windowsPath -Text $windowsJobText -Token $windowsThrowToken -Issue "Windows verifier must fail closed on installer hash mismatch" -Violations $violations
    Assert-TokenOnce -Path $windowsPath -Text $windowsJobText -Token $windowsExecuteToken -Issue "Windows verifier must execute exactly one verified installer with exact runtime arguments" -Violations $violations
)
Assert-OrderedIndexes `
    -Path $windowsPath `
    -Text $windowsJobText `
    -Indexes $windowsIndexes `
    -Issue "Windows installer source/hash verification must complete before execution" `
    -Violations $violations

foreach ($requiredToken in @(
        "[regex]::Match([string]`$_, '^Microsoft\.NETCore\.App (8\.0\.\d+) ')",
        '$runtimeVersion = $runtimeVersions[0].ToString()',
        '"install_script_sha256=$installScriptSha256" >> $env:GITHUB_OUTPUT')) {
    [void](Assert-TokenOnce `
        -Path $windowsPath `
        -Text $windowsJobText `
        -Token $requiredToken `
        -Issue "Windows x86 runtime bootstrap must retain exact runtime derivation and verified hash evidence" `
        -Violations $violations)
}

$bashJobNames = @(
    "verify-targeted-real-ubuntu2204-arm64",
    "verify-targeted-real-debian-arm64"
)
foreach ($bashJobName in $bashJobNames) {
    $bashJobText = Get-WorkflowJobText -WorkflowText $packWorkflowText -JobName $bashJobName -Violations $violations
    $bashPath = "$packWorkflowRelativePath#$bashJobName"
    $bashExpectedHashToken = "dotnet_install_script_sha256=`"$bashInstallerSha256`""
    $bashDownloadToken = "curl -fsSL $bashInstallerUrl -o /tmp/dotnet-install.sh"
    $bashVerifyToken = 'echo "$dotnet_install_script_sha256  /tmp/dotnet-install.sh" | sha256sum -c -'
    $bashExecuteToken = "bash /tmp/dotnet-install.sh --version 8.0.423 --architecture arm64 --install-dir /usr/share/dotnet"
    $bashIndexes = @(
        Assert-TokenOnce -Path $bashPath -Text $bashJobText -Token $bashExpectedHashToken -Issue "ARM64 verifier must bind the exact expected Bash installer hash once" -Violations $violations
        Assert-TokenOnce -Path $bashPath -Text $bashJobText -Token $bashDownloadToken -Issue "ARM64 verifier must download the exact commit-pinned Bash installer once" -Violations $violations
        Assert-TokenOnce -Path $bashPath -Text $bashJobText -Token $bashVerifyToken -Issue "ARM64 verifier must validate the downloaded Bash installer once" -Violations $violations
        Assert-TokenOnce -Path $bashPath -Text $bashJobText -Token $bashExecuteToken -Issue "ARM64 verifier must execute exactly one verified installer with exact SDK arguments" -Violations $violations
    )
    Assert-OrderedIndexes `
        -Path $bashPath `
        -Text $bashJobText `
        -Indexes $bashIndexes `
        -Issue "ARM64 installer source/hash verification must complete before execution" `
        -Violations $violations
}

$acquisitionCount = [regex]::Matches(
    $packWorkflowText,
    '(?im)^\s*(?:Invoke-WebRequest|curl)\b[^\r\n]*dotnet-install\.(?:ps1|sh)\b').Count
$powerShellExecutionCount = [regex]::Matches(
    $packWorkflowText,
    '(?im)^\s*&\s+\$installScript\s+-Runtime\s+dotnet\b').Count
$bashExecutionCount = [regex]::Matches(
    $packWorkflowText,
    '(?im)^\s*bash\s+/tmp/dotnet-install\.sh\b').Count
$commitCount = [regex]::Matches($packWorkflowText, [regex]::Escape($installerCommit)).Count
$powerShellHashCount = [regex]::Matches($packWorkflowText, [regex]::Escape($powerShellInstallerSha256)).Count
$bashHashCount = [regex]::Matches($packWorkflowText, [regex]::Escape($bashInstallerSha256)).Count
foreach ($contract in @(
        [pscustomobject]@{ Actual = $acquisitionCount; Expected = 3; Issue = "Pack workflow must contain exactly three installer acquisitions" },
        [pscustomobject]@{ Actual = $powerShellExecutionCount; Expected = 1; Issue = "Pack workflow must contain exactly one PowerShell installer execution" },
        [pscustomobject]@{ Actual = $bashExecutionCount; Expected = 2; Issue = "Pack workflow must contain exactly two Bash installer executions" },
        [pscustomobject]@{ Actual = $commitCount; Expected = 3; Issue = "Each installer acquisition must name the audited commit exactly once" },
        [pscustomobject]@{ Actual = $powerShellHashCount; Expected = 1; Issue = "PowerShell installer expected hash must appear exactly once" },
        [pscustomobject]@{ Actual = $bashHashCount; Expected = 2; Issue = "Bash installer expected hash must appear once per ARM64 verifier" })) {
    if ($contract.Actual -ne $contract.Expected) {
        Add-Violation `
            -Violations $violations `
            -Path $packWorkflowRelativePath `
            -Issue $contract.Issue `
            -Text "actual=$($contract.Actual) expected=$($contract.Expected)"
    }
}

$aggregatePath = Join-Path $repo $aggregateRelativePath
if (-not (Test-Path -LiteralPath $aggregatePath -PathType Leaf)) {
    Add-Violation -Violations $violations -Path $aggregateRelativePath -Issue "Project invariant aggregate is missing"
}
else {
    $aggregateText = [System.IO.File]::ReadAllText($aggregatePath)
    $pathRegistrationCount = [regex]::Matches($aggregateText, [regex]::Escape($guardRelativePath)).Count
    $nameRegistrationCount = [regex]::Matches($aggregateText, 'Name\s*=\s*"\.NET installer supply-chain boundary"').Count
    if ($pathRegistrationCount -ne 1 -or $nameRegistrationCount -ne 1) {
        Add-Violation `
            -Violations $violations `
            -Path $aggregateRelativePath `
            -Issue "Installer supply-chain guard must be registered exactly once" `
            -Text "path_registrations=$pathRegistrationCount name_registrations=$nameRegistrationCount"
    }
}

if ($violations.Count -gt 0) {
    Write-Host ".NET installer supply-chain boundary guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Line, Issue | Format-Table Path, Line, Issue, Text -AutoSize
    exit 1
}

Write-Host ".NET installer supply-chain boundary guard passed."
Write-Host "Installer acquisitions/executions: $acquisitionCount/$($powerShellExecutionCount + $bashExecutionCount); audited commit: $installerCommit."
Write-Host "PowerShell installer SHA256: $powerShellInstallerSha256; Bash installer SHA256: $bashInstallerSha256."
Write-Host "Every installer payload is commit-pinned and hash-verified before execution."
