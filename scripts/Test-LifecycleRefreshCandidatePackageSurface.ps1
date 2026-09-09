param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workflowPath = '.github/workflows/pack.yml'
$workflowFullPath = Join-Path $repo $workflowPath
if (-not (Test-Path -LiteralPath $workflowFullPath -PathType Leaf)) {
    throw "Required workflow was not found: $workflowPath"
}
$workflow = [IO.File]::ReadAllText($workflowFullPath)
$violations = [Collections.Generic.List[object]]::new()

function Assert-Contains {
    param([string]$Needle, [string]$Issue)
    if ($workflow.IndexOf($Needle, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        $violations.Add([pscustomobject]@{ Issue = $Issue; Text = $Needle })
    }
}

function Assert-NotContains {
    param([string]$Text, [string]$Needle, [string]$Issue)
    if ($Text.IndexOf($Needle, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
        $violations.Add([pscustomobject]@{ Issue = $Issue; Text = $Needle })
    }
}

function Assert-Matches {
    param([string]$Pattern, [string]$Issue)
    if (-not [regex]::IsMatch($workflow, $Pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
        $violations.Add([pscustomobject]@{ Issue = $Issue; Text = $Pattern })
    }
}

foreach ($token in @(
        'lifecycle_refresh_candidate:',
        'lifecycle_refresh_rid:',
        'lifecycle_refresh_profile:',
        'lifecycle_refresh_artifact_run_id:',
        'validate-lifecycle-refresh-candidate:',
        'pack-lifecycle-refresh-managed:',
        'pack-lifecycle-refresh-runtime:',
        'verify-lifecycle-refresh-candidate:',
        "packaging/runtime/runtime-lifecycle-refresh-matrix.json",
        'lifecycle-refresh-runtime-input-${{ matrix.rid }}-${{ matrix.profile }}',
        'scripts/Test-LifecycleRefreshRuntimeInputArtifact.ps1',
        'scripts/Pack-Managed.ps1',
        'scripts/Pack-Runtime.ps1',
        'scripts/Test-GitHubPackArtifactMatrixSurface.ps1',
        'scripts/Test-GitHubPackConsumerRestoreSurface.ps1',
        '-CompileNativeSmoke',
        "if (Test-Path -LiteralPath `$openCvInstallDir -PathType Container)",
        "'-OpenCvInstallDir', `$openCvInstallDir",
        'NativeSmokeExecuted = $false',
        'PublicationAllowed = $false')) {
    Assert-Contains -Needle $token -Issue 'Lifecycle-refresh package/consumer workflow lost a required token'
}

foreach ($jobName in @(
        'pack-lifecycle-refresh-managed:',
        'pack-lifecycle-refresh-runtime:',
        'verify-lifecycle-refresh-candidate:')) {
    $jobMatch = [regex]::Match($workflow, "(?ms)^  $([regex]::Escape($jobName.TrimEnd(':'))):(?<body>.*?)(?=^  [A-Za-z0-9_-]+:|\z)")
    if (-not $jobMatch.Success) {
        $violations.Add([pscustomobject]@{ Issue = 'Lifecycle-refresh package/consumer job was not found'; Text = $jobName })
        continue
    }

    $body = $jobMatch.Groups['body'].Value
    Assert-NotContains -Text $body -Needle 'dotnet nuget push' -Issue 'Candidate package jobs must never publish to a package feed'
    Assert-NotContains -Text $body -Needle 'publish_github_packages' -Issue 'Candidate package jobs must not consume the active publication input'
    Assert-NotContains -Text $body -Needle 'gh release' -Issue 'Candidate package jobs must never create a GitHub release'
}

Assert-Matches -Pattern '(?m)^  pack-managed:\s+needs: validate\s+if: \$\{\{ !inputs\.lifecycle_refresh_candidate \}\}' -Issue 'Active managed packaging must be skipped in candidate mode'
Assert-Matches -Pattern '(?m)^  pack-runtime:\s+needs: validate\s+if: \$\{\{ !inputs\.lifecycle_refresh_candidate \}\}' -Issue 'Active runtime packaging must be skipped in candidate mode'
Assert-Contains -Needle 'actions: read' -Issue 'Candidate cross-run artifact consumers must request only Actions read permission'

if ($violations.Count -gt 0) {
    Write-Host "Lifecycle-refresh candidate package surface failed with $($violations.Count) violation(s)."
    $violations | ForEach-Object { Write-Host "$($_.Issue) :: $($_.Text)" }
    exit 1
}

Write-Host 'Lifecycle-refresh candidate package/consumer surface passed.'
Write-Host 'Candidate producer artifacts are packaged and restore/build-verified through an isolated, non-publishing dispatch path.'
