param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$violations = [System.Collections.Generic.List[object]]::new()
$contractRelativePath = 'packaging/runtime/runtime-support-contract.json'
$contractSchemaRelativePath = 'packaging/runtime/runtime-support-contract.schema.json'
$matrixRelativePath = 'packaging/runtime/runtime-package-matrix.json'
$androidEvidenceRelativePath = 'packaging/runtime/android-runtime-evidence.json'

function Add-Violation {
    param([Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Issue,[string]$Text = '')
    $violations.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param([Parameter(Mandatory = $true)][bool]$Condition,[Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Issue,[string]$Text = '')
    if (-not $Condition) { Add-Violation -Path $Path -Issue $Issue -Text $Text }
}

function Read-RequiredJson {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Required support contract file was not found: $RelativePath" }
    return [pscustomobject]@{ Path = $path; RelativePath = $RelativePath; Value = ([IO.File]::ReadAllText($path) | ConvertFrom-Json) }
}

function Get-TargetSet {
    param([Parameter(Mandatory = $true)][object[]]$Items,[Parameter(Mandatory = $true)][string]$Property)
    return @($Items | ForEach-Object { [string]$_.$Property } | Sort-Object)
}

function Assert-ExactSet {
    param([Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Issue,[Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$Expected,[Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$Actual)
    $expectedText = [string]::Join("`n", @($Expected | Sort-Object))
    $actualText = [string]::Join("`n", @($Actual | Sort-Object))
    Assert-True -Condition ($expectedText -eq $actualText) -Path $Path -Issue $Issue -Text "expected=$expectedText actual=$actualText"
}

function Assert-ExactPropertySet {
    param([Parameter(Mandatory = $true)][object]$Value,[Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Context,[Parameter(Mandatory = $true)][string[]]$Expected)
    $actual = @($Value.PSObject.Properties.Name | Sort-Object)
    $wanted = @($Expected | Sort-Object)
    Assert-ExactSet -Path $Path -Issue "$Context property set drifted" -Expected $wanted -Actual $actual
}

function Get-WorkflowJobText {
    param([Parameter(Mandatory = $true)][string]$Text,[Parameter(Mandatory = $true)][string]$JobName)
    $pattern = "(?ms)^  $([regex]::Escape($JobName)):\r?\n.*?(?=^  [A-Za-z0-9_-]+:\r?\n|\z)"
    $match = [regex]::Match($Text, $pattern)
    if (-not $match.Success) { Add-Violation -Path '.github/workflows/runtime-input.yml' -Issue 'Required workflow job was not found' -Text $JobName; return '' }
    return $match.Value
}

try {
    $contract = Read-RequiredJson -RelativePath $contractRelativePath
    $matrix = Read-RequiredJson -RelativePath $matrixRelativePath
    $androidEvidence = Read-RequiredJson -RelativePath $androidEvidenceRelativePath
    $c = $contract.Value
    $m = $matrix.Value
    $a = $androidEvidence.Value
    $lifecycleCandidates = @($c.lifecycleRefreshCandidates)

    $contractSchemaPath = Join-Path $repo $contractSchemaRelativePath
    if (-not (Test-Path -LiteralPath $contractSchemaPath -PathType Leaf)) { throw "Required support contract schema was not found: $contractSchemaRelativePath" }
    Assert-True -Condition (Test-Json -LiteralPath $contract.Path -SchemaFile $contractSchemaPath -ErrorAction Stop) -Path $contract.RelativePath -Issue 'Support contract must validate against its JSON Schema'
    foreach ($fixtureName in @('schema-v1', 'unknown-root-field', 'compatibility-status', 'lifecycle-candidate-status', 'lifecycle-candidate-extra-field')) {
        $fixture = ([IO.File]::ReadAllText($contract.Path) | ConvertFrom-Json)
        switch ($fixtureName) {
            'schema-v1' { $fixture.schemaVersion = 1 }
            'unknown-root-field' { $fixture | Add-Member -NotePropertyName legacyRealSupport -NotePropertyValue @() }
            'compatibility-status' { $fixture.compatibilityOnly[0].status = 'real-supported' }
            'lifecycle-candidate-status' { $fixture.lifecycleRefreshCandidates[0].status = 'real-supported' }
            'lifecycle-candidate-extra-field' { $fixture.lifecycleRefreshCandidates[0] | Add-Member -NotePropertyName promoted -NotePropertyValue $true }
        }
        $fixtureJson = $fixture | ConvertTo-Json -Depth 20
        $schemaAccepted = Test-Json -Json $fixtureJson -SchemaFile $contractSchemaPath -ErrorAction SilentlyContinue
        Assert-True -Condition (-not $schemaAccepted) -Path "$($contract.RelativePath)/$fixtureName" -Issue 'Support contract JSON Schema accepted an invalid migration fixture'
    }

    Assert-ExactPropertySet -Value $c -Path $contract.RelativePath -Context 'Support contract schema v2' -Expected @(
        '$schema',
        'schemaVersion',
        'packageMatrix',
        'androidRuntimeEvidence',
        'lifecycleRefreshCandidates',
        'packageSurface',
        'hostedPromotionEvidence',
        'realSupport',
        'compatibilityOnly',
        'pending',
        'excluded',
        'outsideMatrix',
        'policy'
    )
    Assert-True -Condition ([string]$c.'$schema' -eq 'runtime-support-contract.schema.json') -Path $contract.RelativePath -Issue 'Support contract must identify its schema file'
    Assert-ExactPropertySet -Value $c.policy -Path $contract.RelativePath -Context 'Support contract policy' -Expected @(
        'packageSurfaceIsSupport',
        'releaseCandidate',
        'compatibilityOnlyPublication',
        'syntheticRuntimeInputs',
        'publication'
    )
    Assert-True -Condition ([int]$c.schemaVersion -eq 2) -Path $contract.RelativePath -Issue 'Support contract schema version must be 2'
    Assert-True -Condition ([string]$c.packageMatrix -eq $matrixRelativePath) -Path $contract.RelativePath -Issue 'Support contract must identify the package matrix'
    Assert-True -Condition ([string]$c.androidRuntimeEvidence -eq $androidEvidenceRelativePath) -Path $contract.RelativePath -Issue 'Support contract must identify the Android runtime evidence record'
    Assert-True -Condition ($lifecycleCandidates.Count -eq 2) -Path $contract.RelativePath -Issue 'Support contract must retain exactly two lifecycle refresh candidates'
    $expectedLifecycleCandidates = @(
        [pscustomobject]@{
            Rid = 'alpine.3.23-x64'
            Distro = 'alpine'
            DistroVersion = '3.23'
            ContainerImage = 'alpine:3.23@sha256:fd791d74b68913cbb027c6546007b3f0d3bc45125f797758156952bc2d6daf40'
            ContainerRepoDigest = 'alpine@sha256:fd791d74b68913cbb027c6546007b3f0d3bc45125f797758156952bc2d6daf40'
        },
        [pscustomobject]@{
            Rid = 'fedora.44-x64'
            Distro = 'fedora'
            DistroVersion = '44'
            ContainerImage = 'fedora:44@sha256:43b29f65a41eb9c35e1cd5323e3bdf3b655c2357a9f4f1ff2f9c2798e5045d80'
            ContainerRepoDigest = 'fedora@sha256:43b29f65a41eb9c35e1cd5323e3bdf3b655c2357a9f4f1ff2f9c2798e5045d80'
        }
    )
    $expectedDotnetSupportedOs = 'https://raw.githubusercontent.com/dotnet/core/20e72eb1b769d71b4dd208419d66d8a0ef3b1961/release-notes/10.0/supported-os.md'
    $expectedDotnetDockerSdk = 'https://raw.githubusercontent.com/dotnet/dotnet-docker/a84faedde9e3070d05dc992569ca1b466e24300a/README.sdk.md'
    foreach ($expected in $expectedLifecycleCandidates) {
        $matches = @($lifecycleCandidates | Where-Object { [string]$_.rid -ceq $expected.Rid })
        Assert-True -Condition ($matches.Count -eq 1) -Path $contract.RelativePath -Issue 'Lifecycle refresh candidate must appear exactly once' -Text $expected.Rid
        if ($matches.Count -eq 1) {
            $entry = $matches[0]
            Assert-ExactPropertySet -Value $entry -Path $contract.RelativePath -Context "Lifecycle refresh candidate $($expected.Rid)" -Expected @('rid','distro','distroVersion','containerImage','containerRepoDigest','architecture','status','observedAtUtc','reason','sources')
            Assert-ExactPropertySet -Value $entry.sources -Path $contract.RelativePath -Context "Lifecycle refresh candidate sources $($expected.Rid)" -Expected @('dotnetSupportedOs','dotnetDockerSdk','dockerHubManifest')
            Assert-True -Condition ([string]$entry.distro -ceq $expected.Distro -and [string]$entry.distroVersion -ceq $expected.DistroVersion -and [string]$entry.containerImage -ceq $expected.ContainerImage -and [string]$entry.containerRepoDigest -ceq $expected.ContainerRepoDigest -and [string]$entry.architecture -ceq 'x86_64' -and [string]$entry.status -ceq 'lifecycle-refresh-pending' -and -not [string]::IsNullOrWhiteSpace([string]$entry.reason) -and [datetime]$entry.observedAtUtc -le [datetime]::UtcNow) -Path $contract.RelativePath -Issue 'Lifecycle refresh candidate facts or pending status drifted' -Text $expected.Rid
            $expectedManifest = if ($expected.Distro -eq 'alpine') { 'https://hub.docker.com/v2/repositories/library/alpine/tags/3.23' } else { 'https://hub.docker.com/v2/repositories/library/fedora/tags/44' }
            Assert-True -Condition ([string]$entry.sources.dotnetSupportedOs -ceq $expectedDotnetSupportedOs -and [string]$entry.sources.dotnetDockerSdk -ceq $expectedDotnetDockerSdk -and [string]$entry.sources.dockerHubManifest -ceq $expectedManifest) -Path $contract.RelativePath -Issue 'Lifecycle refresh candidate source pin drifted' -Text $expected.Rid
        }
    }
    Assert-True -Condition ($c.policy.packageSurfaceIsSupport -eq $false) -Path $contract.RelativePath -Issue 'Package surface must not be treated as real support'
    Assert-True -Condition ([string]$c.policy.releaseCandidate -eq 'real-supported only; compatibility-only, pending, excluded, and lifecycle-refresh candidate targets are not published') -Path $contract.RelativePath -Issue 'Release candidate classification policy drifted'
    Assert-True -Condition ([string]$c.policy.compatibilityOnlyPublication -eq 'excluded from the current release candidate; historical package identity and reproducible evidence are retained') -Path $contract.RelativePath -Issue 'Compatibility-only publication policy drifted'
    Assert-True -Condition ([string]$c.policy.syntheticRuntimeInputs -eq 'package-shape-only; never real support') -Path $contract.RelativePath -Issue 'Synthetic runtime policy drifted'
    Assert-True -Condition ([string]$c.policy.publication -match 'blocked until') -Path $contract.RelativePath -Issue 'Support contract must keep publication blocked until all release gates pass'

    $matrixTargets = @($m.rids | ForEach-Object { $rid = [string]$_.rid; foreach ($profile in @($m.profiles)) { "$rid/$([string]$profile.name)" } } | Sort-Object)
    $packageSurfaceTargets = @($c.packageSurface | ForEach-Object { [string]$_ } | Sort-Object)
    $realTargets = @($c.realSupport | ForEach-Object { [string]$_ } | Sort-Object)
    $compatibilityTargets = @(Get-TargetSet -Items @($c.compatibilityOnly) -Property 'target')
    $pendingTargets = @(Get-TargetSet -Items @($c.pending) -Property 'target')
    $excludedTargets = @(Get-TargetSet -Items @($c.excluded) -Property 'target')
    $classifiedTargets = @($realTargets + $compatibilityTargets + $pendingTargets + $excludedTargets | Sort-Object)

    Assert-ExactSet -Path $contract.RelativePath -Issue 'Schema v2 package surface must match every package matrix RID/profile pair' -Expected $matrixTargets -Actual $packageSurfaceTargets
    Assert-True -Condition (@($matrixTargets | Where-Object { $_ -like 'alpine.3.23-x64/*' -or $_ -like 'fedora.44-x64/*' }).Count -eq 0 -and @($packageSurfaceTargets | Where-Object { $_ -like 'alpine.3.23-x64/*' -or $_ -like 'fedora.44-x64/*' }).Count -eq 0) -Path $contract.RelativePath -Issue 'Lifecycle refresh candidates must remain outside the active package matrix and package surface'
    Assert-ExactSet -Path $contract.RelativePath -Issue 'Support contract must partition every package-surface target exactly once' -Expected $packageSurfaceTargets -Actual $classifiedTargets
    Assert-True -Condition (@($realTargets).Count -eq 25) -Path $contract.RelativePath -Issue 'Real support target count must be 25 after lifecycle migration'
    $expectedCompatibilityTargets = @(
        'alpine.3.20-x64/full',
        'alpine.3.20-x64/mini',
        'fedora.40-x64/full',
        'fedora.40-x64/mini'
    )
    Assert-ExactSet -Path $contract.RelativePath -Issue 'Compatibility-only targets must contain the ended Fedora 40 and Alpine 3.20 profiles' -Expected $expectedCompatibilityTargets -Actual $compatibilityTargets
    $expectedAndroidPendingTargets = @(
        'android-arm/full',
        'android-arm/mini',
        'android-arm64/full',
        'android-arm64/mini'
    )
    $expectedAndroidRealTargets = @(
        'android-x64/full',
        'android-x64/mini',
        'android-x86/full',
        'android-x86/mini'
    )
    $expectedSupersededAndroidTargets = @(
        'android-x64/full',
        'android-x64/mini',
        'android-x86/full',
        'android-x86/mini'
    )
    Assert-ExactSet -Path $contract.RelativePath -Issue 'Pending support targets must contain only Android ARM/ARM64 profiles' -Expected $expectedAndroidPendingTargets -Actual $pendingTargets
    Assert-ExactSet -Path $contract.RelativePath -Issue 'Android x64/x86 targets must match promoted single-loader evidence' -Expected $expectedAndroidRealTargets -Actual @($realTargets | Where-Object { $_.StartsWith('android-', [StringComparison]::Ordinal) })
    Assert-True -Condition ($realTargets -contains 'win-x86/full') -Path $contract.RelativePath -Issue 'Windows x86 full must be real-supported after verified hosted evidence'
    Assert-ExactSet -Path $contract.RelativePath -Issue 'Only Windows x86 mini may remain excluded' -Expected @('win-x86/mini') -Actual $excludedTargets
    Assert-True -Condition (@($c.outsideMatrix | Where-Object { $_.platform -eq 'macOS' -and $_.status -eq 'not-supported' }).Count -eq 1) -Path $contract.RelativePath -Issue 'macOS must remain explicitly outside support'

    foreach ($entry in @($c.pending)) {
        $target = [string]$entry.target
        Assert-True -Condition ($target.StartsWith('android-', [StringComparison]::Ordinal) -and [string]$entry.status -eq 'android-evidence-pending') -Path $contract.RelativePath -Issue 'Android pending target must remain android-evidence-pending' -Text $target
        Assert-ExactSet -Path $contract.RelativePath -Issue "Pending target requirements drifted for $target" -Expected @('device-or-emulator-loader') -Actual @($entry.requires)
    }
    foreach ($entry in @($c.compatibilityOnly)) {
        Assert-ExactPropertySet -Value $entry -Path $contract.RelativePath -Context "Compatibility-only target $([string]$entry.target)" -Expected @('target', 'status', 'reason', 'migration')
        Assert-True -Condition (
            [string]$entry.status -eq 'compatibility-only' -and
            -not [string]::IsNullOrWhiteSpace([string]$entry.reason) -and
            -not [string]::IsNullOrWhiteSpace([string]$entry.migration)
        ) -Path $contract.RelativePath -Issue 'Compatibility-only target must carry status, lifecycle reason, and migration guidance' -Text $entry.target
    }
    $hostedEvidence = $c.hostedPromotionEvidence
    Assert-True -Condition ($null -ne $hostedEvidence -and [string]$hostedEvidence.target -eq 'win-x86/full' -and [string]$hostedEvidence.status -eq 'verified-hosted-evidence') -Path $contract.RelativePath -Issue 'Windows x86 hosted promotion evidence identity drifted'
    Assert-True -Condition ([string]$hostedEvidence.sourceCommit -eq '7f53e03e7d6ad5839711ba5ea32a0fcc02d8d5b8' -and [long]$hostedEvidence.runtimeInputRunId -eq 31162854992 -and [long]$hostedEvidence.packRunId -eq 31171822232 -and [long]$hostedEvidence.consumerRunId -eq 31171822232) -Path $contract.RelativePath -Issue 'Windows x86 hosted promotion run binding drifted'
    Assert-True -Condition ([long]$hostedEvidence.runtimeArtifact.id -eq 8988306107 -and [string]$hostedEvidence.runtimeArtifact.name -eq 'runtime-input-win-x86-full' -and [string]$hostedEvidence.runtimeArtifact.digest -eq 'sha256:9e73da169b3d30d3602bc77516a4db571be4af4728637b89c6e8563311afa4b8') -Path $contract.RelativePath -Issue 'Windows x86 runtime artifact evidence drifted'
    Assert-True -Condition ([long]$hostedEvidence.packageArtifact.id -eq 8991471168 -and [string]$hostedEvidence.packageArtifact.name -eq 'nupkg-win-x86-full' -and [string]$hostedEvidence.packageArtifact.digest -eq 'sha256:0cedaef44f1bef7059763c22dd95d2bd722d0e083ccffeb814e7034143dfacbe') -Path $contract.RelativePath -Issue 'Windows x86 package artifact evidence drifted'
    Assert-True -Condition ([string]$hostedEvidence.hostArchitecture -eq 'AMD64' -and [string]$hostedEvidence.targetArchitecture -eq 'X86' -and [string]$hostedEvidence.peMachine -eq 'I386' -and [string]$hostedEvidence.wow64Probe -eq 'passed' -and [string]$hostedEvidence.consumerProcessArchitecture -eq 'X86') -Path $contract.RelativePath -Issue 'Windows x86 hosted architecture evidence drifted'
    Assert-True -Condition ([string]$hostedEvidence.hostedCloseoutSha256 -eq '61f3ce0263fa41126c7ac857cde56ce6096147f0a1f9a3f3fa499fab1478bd81' -and [string]$hostedEvidence.artifactDigestAuditSha256 -eq 'c0bf6886787fbbc105390ec8861acd6d608b60e9b86c63aef2c7456d436d9ded' -and [string]$hostedEvidence.hostedPackageManifestSha256 -eq 'eedb212724a7f176bc913cc92b09ca086a4d7129f069a964c3195a3b8557c353' -and [string]$hostedEvidence.hostedChangeControlSha256 -eq '69b1444692d389bb3452575a494035faddf5eb4c2f111bf3b60a686986841332') -Path $contract.RelativePath -Issue 'Windows x86 hosted audit hash binding drifted'
    foreach ($entry in @($c.excluded)) {
        Assert-True -Condition ([string]$entry.status -eq 'excluded' -and -not [string]::IsNullOrWhiteSpace([string]$entry.reason)) -Path $contract.RelativePath -Issue 'Excluded target must carry an explicit reason' -Text $entry.target
    }

    Assert-True -Condition ([int]$a.schemaVersion -eq 2 -and [string]$a.repository -eq 'guojin-yan/OpenCV-CSharp-API' -and [string]$a.workflow -eq '.github/workflows/runtime-input.yml') -Path $androidEvidence.RelativePath -Issue 'Android evidence identity drifted'
    Assert-ExactSet -Path $androidEvidence.RelativePath -Issue 'Android verified evidence targets must match promoted real support' -Expected $expectedAndroidRealTargets -Actual @($a.verified | ForEach-Object { [string]$_.target })
    Assert-ExactSet -Path $androidEvidence.RelativePath -Issue 'Android pending device-loading evidence must match support contract' -Expected $expectedAndroidPendingTargets -Actual @($a.pendingDeviceLoading)
    Assert-True -Condition (-not [string]::IsNullOrWhiteSpace([string]$a.supersededEvidenceReason)) -Path $androidEvidence.RelativePath -Issue 'Superseded Android evidence must explain why it is no longer current'
    Assert-ExactSet -Path $androidEvidence.RelativePath -Issue 'Superseded Android evidence must retain the four retired dual-loader runs' -Expected $expectedSupersededAndroidTargets -Actual @($a.superseded | ForEach-Object { [string]$_.target })
    foreach ($entry in @($a.verified)) {
        $target = [string]$entry.target
        $parts = $target.Split('/')
        $rid = $parts[0]
        $profile = $parts[1]
        $expectedAbi = if ($rid -eq 'android-x64') { 'x86_64' } else { 'x86' }
        $expectedImage = if ($rid -eq 'android-x64') { 'system-images;android-35;default;x86_64' } else { 'system-images;android-29;default;x86' }
        $expectedNativeFiles = if ($profile -eq 'full') { 17 } else { 7 }
        $expectedMarker = "ANDROID_EMULATOR_LOADING_OK rid=$rid abi=$expectedAbi profile=$profile native_call=Mat+Cv2.Sum version=5.0.0 sum=448"
        $completedAt = [datetime]$entry.completedAt
        Assert-True -Condition (
            [string]$entry.sourceCommit -cmatch '^[0-9a-f]{40}$' -and
            [long]$entry.runId -gt 0 -and
            [long]$entry.jobId -gt 0 -and
            $completedAt -ne [datetime]::MinValue -and
            $completedAt.ToUniversalTime() -le [datetime]::UtcNow -and
            [string]$entry.abi -eq $expectedAbi -and
            [string]$entry.systemImage -eq $expectedImage -and
            [int]$entry.nativeFiles -eq $expectedNativeFiles -and
            [string]$entry.nativeCall -eq 'Mat+Cv2.Sum' -and
            [string]$entry.marker -eq $expectedMarker -and
            [long]$entry.runtimeArtifact.id -gt 0 -and
            [string]$entry.runtimeArtifact.name -eq "runtime-input-$rid-$profile" -and
            [long]$entry.runtimeArtifact.size -gt 0 -and
            [long]$entry.proofArtifact.id -gt 0 -and
            [string]$entry.proofArtifact.name -eq "android-proof-$rid-$profile" -and
            [long]$entry.proofArtifact.size -gt 0
        ) -Path $androidEvidence.RelativePath -Issue 'Android authoritative runtime evidence drifted' -Text $target
    }
    foreach ($entry in @($a.superseded)) {
        $target = [string]$entry.target
        $parts = $target.Split('/')
        $rid = $parts[0]
        $profile = $parts[1]
        $expectedNativeFiles = if ($profile -eq 'full') { 18 } else { 8 }
        Assert-True -Condition (
            [string]$entry.sourceCommit -cmatch '^[0-9a-f]{40}$' -and
            [long]$entry.runId -gt 0 -and
            [long]$entry.jobId -gt 0 -and
            [int]$entry.nativeFiles -eq $expectedNativeFiles -and
            [long]$entry.runtimeArtifact.id -gt 0 -and
            [long]$entry.proofArtifact.id -gt 0
        ) -Path $androidEvidence.RelativePath -Issue 'Superseded Android evidence record drifted' -Text $target
    }

    $runtimeInputPath = Join-Path $repo '.github/workflows/runtime-input.yml'
    $runtimeInputText = [IO.File]::ReadAllText($runtimeInputPath)
    foreach ($selectionToken in @(
            'Get-Content -LiteralPath ./packaging/runtime/runtime-support-contract.json -Raw | ConvertFrom-Json',
            '$supportedTargets = @($supportContract.realSupport) + @($supportContract.compatibilityOnly | ForEach-Object { [string]$_.target }) + @($supportContract.pending | ForEach-Object { [string]$_.target })',
            '$selectedTarget = "$($env:RID_INPUT)/$($env:RUNTIME_PROFILE_INPUT)"',
            'if ($supportedTargets -notcontains $selectedTarget)',
            'Lifecycle-refresh candidates are catalogued but not active until producer/package/consumer evidence is promoted.')) {
        Assert-True -Condition $runtimeInputText.Contains($selectionToken, [StringComparison]::Ordinal) -Path '.github/workflows/runtime-input.yml' -Issue 'runtime-input.yml must select supported targets from the structured release support contract' -Text $selectionToken
    }

    $producerTargets = @($m.rids | ForEach-Object {
            $rid = [string]$_.rid
            foreach ($profile in @($_.producer.profiles)) {
                "$rid/$([string]$profile)"
            }
        } | Sort-Object)
    Assert-ExactSet -Path $matrix.RelativePath -Issue 'Runtime producer metadata must equal real support, compatibility-only, and pending evidence targets' -Expected @($realTargets + $compatibilityTargets + $pendingTargets) -Actual $producerTargets
    Assert-True -Condition (@($producerTargets | Where-Object { $excludedTargets -contains $_ }).Count -eq 0) -Path $matrix.RelativePath -Issue 'Runtime producer metadata must not include excluded targets'

    $packWorkflowPath = Join-Path $repo '.github/workflows/pack.yml'
    $packWorkflowText = [IO.File]::ReadAllText($packWorkflowPath)
    $runtimeJobText = Get-WorkflowJobText -Text $packWorkflowText -JobName 'pack-runtime'
    Assert-True -Condition ($runtimeJobText.Contains('validate_synthetic_runtime')) -Path '.github/workflows/pack.yml' -Issue 'Pack workflow must retain explicit synthetic-input gating'
    Assert-True -Condition ($runtimeJobText.Contains("publish_github_packages == 'true'")) -Path '.github/workflows/pack.yml' -Issue 'Pack workflow must retain explicit publication gating'
    Assert-True -Condition ($runtimeJobText.Contains('real_runtime_artifact_run_id')) -Path '.github/workflows/pack.yml' -Issue 'Pack workflow must retain real artifact handoff input'
    Assert-True -Condition (-not $runtimeJobText.Contains('-SelectedRid android-')) -Path '.github/workflows/pack.yml' -Issue 'Pack workflow must not claim Android real verification'

    $readmePath = Join-Path $repo 'packaging/runtime/JYPPX.OpenCV.runtime/README.md'
    $readmeText = [IO.File]::ReadAllText($readmePath)
    Assert-True -Condition ($readmeText.Contains('runtime-support-contract.json')) -Path 'packaging/runtime/JYPPX.OpenCV.runtime/README.md' -Issue 'Runtime README must link the support contract'
    Assert-True -Condition ($readmeText.Contains('Windows x86 Full is real-supported after verified hosted WoW64 evidence') -and $readmeText.Contains('Android x64/x86 Full and Mini are real-supported after authoritative single-loader emulator loading') -and $readmeText.Contains('Android ARM/ARM64 remain android-evidence-pending')) -Path 'packaging/runtime/JYPPX.OpenCV.runtime/README.md' -Issue 'Runtime README must preserve x86 and Android support wording'

    $guidePath = Join-Path $repo 'docs/articles/linked-runtime-build-guide.md'
    $guideText = [IO.File]::ReadAllText($guidePath)
    Assert-True -Condition ($guideText.Contains('runtime-support-contract.json')) -Path 'docs/articles/linked-runtime-build-guide.md' -Issue 'Linked runtime guide must link the support contract'
    Assert-True -Condition ($guideText.Contains('Windows x86 Full is real-supported after verified hosted WoW64 evidence') -and $guideText.Contains('Android x64/x86 Full and Mini are real-supported after authoritative single-loader emulator loading') -and $guideText.Contains('Android ARM/ARM64 remain android-evidence-pending')) -Path 'docs/articles/linked-runtime-build-guide.md' -Issue 'Linked runtime guide must preserve x86 and Android support wording'

    Write-Host "RELEASE_SUPPORT_CONTRACT_OK schema=2 package_surface=$($packageSurfaceTargets.Count) real=$($realTargets.Count) compatibility_only=$($compatibilityTargets.Count) pending=$($pendingTargets.Count) excluded=$($excludedTargets.Count) lifecycle_refresh_candidates=$($lifecycleCandidates.Count) outside_matrix=macOS package_surface_support=false"
}
catch {
    Add-Violation -Path $contractRelativePath -Issue 'Release support contract execution failed' -Text $_.Exception.Message
}

if ($violations.Count -gt 0) {
    Write-Host "Release support contract failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Issue | Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host 'Release support contract passed.'
    Write-Host 'Package surface is explicitly separated from real support; Fedora 40 and Alpine 3.20 are compatibility-only, Fedora 44 and Alpine 3.23 are lifecycle-refresh candidates outside the active matrix, Windows x86 Full has verified hosted WoW64 evidence, Android x64/x86 Full/Mini have authoritative single-loader emulator evidence, Android ARM/ARM64 remain device-evidence-pending, Windows x86 mini remains excluded, and macOS remains outside the matrix.'
    Write-Host 'Schema migration fixtures rejected: schema v1, unknown root field, compatibility status promotion, lifecycle candidate status promotion, and lifecycle candidate field expansion.'
