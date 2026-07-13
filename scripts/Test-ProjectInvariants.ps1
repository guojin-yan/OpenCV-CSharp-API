param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
if ($null -eq $pwsh) {
    throw "pwsh was not found. Project invariant checks require PowerShell 7+."
}

function Invoke-InvariantGuard {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [string]$ScriptPath,
        [string[]]$Arguments = @()
    )

    if (-not (Test-Path -LiteralPath $ScriptPath -PathType Leaf)) {
        throw "Invariant guard script was not found: $ScriptPath"
    }

    Write-Host "==> $Name"
    & $pwsh.Source -NoProfile -File $ScriptPath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Invariant guard failed: $Name"
    }

    Write-Host "<== $Name passed"
}

$guards = @(
    [pscustomobject]@{
        Name = "Version-neutral naming"
        Script = Join-Path $repo "scripts/Test-VersionNeutralNaming.ps1"
        Arguments = @("-RepoRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Managed native interop neutrality"
        Script = Join-Path $repo "scripts/Test-ManagedNativeInteropNeutrality.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Build-info/runtime metadata consistency"
        Script = Join-Path $repo "scripts/Test-BuildInfoRuntimeMetadataConsistency.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Public API namespace neutrality"
        Script = Join-Path $repo "scripts/Test-PublicApiNamespaceNeutrality.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Consumer-facing naming"
        Script = Join-Path $repo "scripts/Test-ConsumerFacingNaming.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Package install consumer surface"
        Script = Join-Path $repo "scripts/Test-PackageInstallConsumerSurface.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Package metadata neutrality"
        Script = Join-Path $repo "scripts/Test-PackageMetadataNeutrality.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Managed package isolated artifact surface"
        Script = Join-Path $repo "scripts/Test-ManagedPackageIsolatedArtifactSurface.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Managed package representative API compile surface"
        Script = Join-Path $repo "scripts/Test-ManagedPackageStandaloneLocalConsumerCompile.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Release package artifact surface"
        Script = Join-Path $repo "scripts/Test-ReleasePackageArtifactSurface.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Path/artifact naming"
        Script = Join-Path $repo "scripts/Test-PathArtifactNaming.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Documentation surface neutrality"
        Script = Join-Path $repo "scripts/Test-DocumentationSurfaceNeutrality.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Workflow invariant coverage"
        Script = Join-Path $repo "scripts/Test-WorkflowInvariantCoverage.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Runtime package neutrality"
        Script = Join-Path $repo "scripts/Test-RuntimePackageNeutrality.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Runtime RID package template scalability"
        Script = Join-Path $repo "scripts/Test-RuntimeRidPackageTemplateScalability.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Runtime RID consumer selection surface"
        Script = Join-Path $repo "scripts/Test-RuntimeRidConsumerSelectionSurface.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Runtime package availability/fallback guidance"
        Script = Join-Path $repo "scripts/Test-RuntimePackageAvailabilityFallbackGuidance.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Runtime fallback command/path consistency"
        Script = Join-Path $repo "scripts/Test-RuntimeFallbackCommandPathConsistency.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Runtime native copy property propagation"
        Script = Join-Path $repo "scripts/Test-RuntimeNativeCopyPropertyPropagation.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Runtime staging dry-run isolation"
        Script = Join-Path $repo "scripts/Test-RuntimeStagingDryRunIsolation.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Runtime pack stage-forwarding isolation"
        Script = Join-Path $repo "scripts/Test-RuntimePackStageForwardingIsolation.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Runtime release-candidate preflight"
        Script = Join-Path $repo "scripts/Test-RuntimeReleaseCandidatePreflightGuard.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Runtime package local consumer restore"
        Script = Join-Path $repo "scripts/Test-RuntimePackageLocalConsumerRestore.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Managed/runtime package-pair local consumer"
        Script = Join-Path $repo "scripts/Test-ManagedRuntimePackagePairLocalConsumer.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Runtime availability workflow/release surface"
        Script = Join-Path $repo "scripts/Test-RuntimeAvailabilityWorkflowReleaseSurface.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Real native runtime build matrix coverage"
        Script = Join-Path $repo "scripts/Test-RealNativeRuntimeBuildMatrixCoverage.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Runtime package docs discoverability"
        Script = Join-Path $repo "scripts/Test-RuntimePackageDocsDiscoverability.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Native header package boundary"
        Script = Join-Path $repo "scripts/Test-NativeHeaderPackageBoundary.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Runtime doc link integrity"
        Script = Join-Path $repo "scripts/Test-RuntimeDocLinkIntegrity.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Native legacy include parity"
        Script = Join-Path $repo "scripts/Test-NativeLegacyIncludeParity.ps1"
        Arguments = @("-RepositoryRoot", $repo)
    },
    [pscustomobject]@{
        Name = "Native ABI compatibility generated-file freshness"
        Script = Join-Path $repo "scripts/Generate-NativeAbiCompatibility.ps1"
        Arguments = @("-RepositoryRoot", $repo, "-Check")
    }
)

foreach ($guard in $guards) {
    Invoke-InvariantGuard `
        -Name $guard.Name `
        -ScriptPath $guard.Script `
        -Arguments @($guard.Arguments)
}

Write-Host "Project invariant guard suite passed. Guards run: $($guards.Count)."
