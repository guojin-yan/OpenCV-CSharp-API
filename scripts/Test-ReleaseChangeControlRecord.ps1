param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$PackageRoot = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression.FileSystem

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$violations = [System.Collections.Generic.List[object]]::new()

function Add-Violation {
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,[Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Issue,[string]$Text = '')
    $List.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,[Parameter(Mandatory = $true)][bool]$Condition,[Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Issue,[string]$Text = '')
    if (-not $Condition) { Add-Violation -List $List -Path $Path -Issue $Issue -Text $Text }
}

function Get-NuspecValue {
    param([Parameter(Mandatory = $true)][xml]$Nuspec,[Parameter(Mandatory = $true)][string]$Name)
    $node = $Nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='$Name']")
    if ($null -eq $node) { return '' }
    return $node.InnerText.Trim()
}

function Get-PackageRecord {
    param([Parameter(Mandatory = $true)][string]$Path)

    $archive = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $nuspecEntries = @($archive.Entries | Where-Object { $_.FullName -match '\.nuspec$' })
        if ($nuspecEntries.Count -ne 1) { throw "Package must contain exactly one nuspec: $Path" }
        $stream = $nuspecEntries[0].Open()
        try { $reader = [IO.StreamReader]::new($stream); try { [xml]$nuspec = $reader.ReadToEnd() } finally { $reader.Dispose() } }
        finally { $stream.Dispose() }
        return [ordered]@{
            PackageId = Get-NuspecValue -Nuspec $nuspec -Name 'id'
            PackageVersion = Get-NuspecValue -Nuspec $nuspec -Name 'version'
            FileName = [IO.Path]::GetFileName($Path)
            Sha256 = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
            Length = (Get-Item -LiteralPath $Path).Length
            SourcePath = [IO.Path]::GetFullPath($Path)
        }
    }
    finally { $archive.Dispose() }
}

function New-FixturePackage {
    param([Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Id,[Parameter(Mandatory = $true)][string]$Version)
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
    $archive = [IO.Compression.ZipFile]::Open($Path, [IO.Compression.ZipArchiveMode]::Create)
    try {
        $entry = $archive.CreateEntry("$Id.nuspec")
        $entry.LastWriteTime = [DateTimeOffset]::new(2000, 1, 1, 0, 0, 0, [TimeSpan]::Zero)
        $stream = $entry.Open()
        try {
            $bytes = [Text.UTF8Encoding]::new($false).GetBytes("<package><metadata><id>$Id</id><version>$Version</version></metadata></package>")
            $stream.Write($bytes, 0, $bytes.Length)
        }
        finally { $stream.Dispose() }
    }
    finally { $archive.Dispose() }
}

function New-ReviewRecord {
    param([Parameter(Mandatory = $true)][object[]]$Packages,[Parameter(Mandatory = $true)][string]$SourceRevision,[Parameter(Mandatory = $true)][string]$MatrixSha256,[Parameter(Mandatory = $true)][int]$MatrixEntryCount,[Parameter(Mandatory = $true)][string]$SupportContractSha256,[Parameter(Mandatory = $true)][bool]$ExternalPackages)

    [ordered]@{
        SchemaVersion = 1
        CandidateId = "local-review/$SourceRevision/$($Packages.Count)"
        SourceRevision = $SourceRevision
        OpenCvRevision = '5.0.0'
        PackageEvidenceKind = if ($ExternalPackages) { 'historical-local-artifacts' } else { 'deterministic-fixture' }
        Packages = @($Packages | Sort-Object PackageId, PackageVersion, FileName)
        SupportMatrix = [ordered]@{
            Sha256 = $MatrixSha256
            EntryCount = $MatrixEntryCount
            SupportContractSha256 = $SupportContractSha256
            RealSupportCount = 24
            PendingSupportCount = 1
            ExcludedSupportCount = 9
            WinX86FullStatus = 'hosted-evidence-pending'
            WinX86MiniStatus = 'excluded'
        }
        EvidenceReferences = @(
            'scripts/Test-ReleaseCandidateProvenance.ps1',
            'scripts/Test-ReleaseReadinessContract.ps1',
            'scripts/Test-ReleaseSigningBoundary.ps1',
            'scripts/Test-ReleaseSupportContract.ps1',
            'scripts/Test-PublicFeedVerificationContract.ps1',
            'scripts/Test-TargetedPackConsumerVerificationSurface.ps1',
            'scripts/Test-WindowsRuntimePeClosure.ps1',
            '.github/workflows/runtime-input.yml',
            '.github/workflows/pack.yml'
        )
        SigningStatus = 'not-ready'
        SbomStatus = 'not-ready'
        Reviewer = [ordered]@{ Id = 'automated-local-preflight'; Status = 'completed' }
        Approver = [ordered]@{ Id = 'unassigned'; Status = 'not-approved' }
        ChangeSummary = 'cumulative-uncommitted-release-hardening'
        ChangeControl = @(
            [ordered]@{ Area = 'package-metadata'; Guard = 'scripts/Test-PackageMetadataNeutrality.ps1' },
            [ordered]@{ Area = 'runtime-matrix'; Guard = 'scripts/Test-RuntimeRidPackageTemplateScalability.ps1' },
            [ordered]@{ Area = 'support-contract'; Guard = 'scripts/Test-ReleaseSupportContract.ps1' },
            [ordered]@{ Area = 'abi-api'; Guard = 'scripts/Test-ManagedNativeInteropNeutrality.ps1' },
            [ordered]@{ Area = 'workflow-permissions'; Guard = 'scripts/Test-GitHubWorkflowPermissions.ps1' },
            [ordered]@{ Area = 'toolchain-pins'; Guard = 'scripts/Test-DotNetSdkToolchainReproducibility.ps1' },
            [ordered]@{ Area = 'feed-trust'; Guard = 'scripts/Test-PublicFeedVerificationContract.ps1' },
            [ordered]@{ Area = 'public-feed'; Guard = 'scripts/Test-PublicFeedVerificationContract.ps1' },
            [ordered]@{ Area = 'signing-sbom'; Guard = 'scripts/Test-ReleaseSigningBoundary.ps1' },
            [ordered]@{ Area = 'hosted-x86'; Guard = 'scripts/Test-TargetedPackConsumerVerificationSurface.ps1' },
            [ordered]@{ Area = 'release-scripts'; Guard = 'scripts/Test-ReleasePackageArtifactSurface.ps1' }
        )
        HostedGate = [ordered]@{
            Target = 'win-x86/full'
            Status = 'hosted-evidence-pending'
            ProducerWorkflow = '.github/workflows/runtime-input.yml'
            ProducerRunner = 'windows-latest'
            ProducerArtifact = 'runtime-input-win-x86-full'
            PackWorkflow = '.github/workflows/pack.yml'
            PackageArtifact = 'nupkg-win-x86-full'
            HostArchitecture = 'AMD64'
            TargetArchitecture = 'X86'
            PeMachine = 'I386'
            Wow64Probe = 'required'
            ConsumerProcessArchitecture = 'X86'
            Sequence = @('hosted-producer', 'artifact-handoff', 'same-run-pack', 'independent-artifact-audit', 'x86-consumer')
            StopConditions = @('missing-artifact', 'hash-mismatch', 'wrong-pe-machine', 'wrong-consumer-architecture', 'synthetic-input', 'path-or-loader-override', 'incomplete-provenance', 'failed-signing-or-sbom-handoff', 'publication-attempt')
        }
        Rollback = [ordered]@{
            Status = 'not-published'
            PriorKnownGood = ''
            PublicFeedStatus = 'not-found'
            PackageRemovalRequired = $false
        }
        Publication = [ordered]@{
            Decision = 'do-not-publish'
            Status = 'not-published'
            Allowed = $false
        }
        PrivateKeyMaterialPresent = $false
        SecretMaterialPresent = $false
        Deterministic = $true
    }
}

function Test-ReviewRecord {
    param([Parameter(Mandatory = $true)][object]$Record,[Parameter(Mandatory = $true)][object[]]$Packages,[Parameter(Mandatory = $true)][string]$SourceRevision,[Parameter(Mandatory = $true)][string]$MatrixSha256,[Parameter(Mandatory = $true)][int]$MatrixEntryCount,[Parameter(Mandatory = $true)][string]$SupportContractSha256,[Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,[Parameter(Mandatory = $true)][string]$Path)

    foreach ($field in @('SchemaVersion','CandidateId','SourceRevision','OpenCvRevision','PackageEvidenceKind','Packages','SupportMatrix','EvidenceReferences','SigningStatus','SbomStatus','Reviewer','Approver','ChangeSummary','ChangeControl','HostedGate','Rollback','Publication','PrivateKeyMaterialPresent','SecretMaterialPresent','Deterministic')) {
        Assert-True -List $List -Condition ($null -ne $Record.PSObject.Properties[$field]) -Path $Path -Issue 'Release review record is missing required field' -Text $field
    }
    if ($null -eq $Record.SupportMatrix -or $null -eq $Record.Reviewer -or $null -eq $Record.Approver -or $null -eq $Record.Rollback -or $null -eq $Record.Publication -or $null -eq $Record.HostedGate) { return }

    Assert-True -List $List -Condition ([int]$Record.SchemaVersion -eq 1) -Path $Path -Issue 'Release review schema version must be 1'
    Assert-True -List $List -Condition ($Record.SourceRevision -eq $SourceRevision) -Path $Path -Issue 'Release review source revision drifted'
    Assert-True -List $List -Condition ($Record.SupportMatrix.Sha256 -eq $MatrixSha256 -and [int]$Record.SupportMatrix.EntryCount -eq $MatrixEntryCount -and $Record.SupportMatrix.SupportContractSha256 -eq $SupportContractSha256 -and [int]$Record.SupportMatrix.RealSupportCount -eq 24 -and [int]$Record.SupportMatrix.PendingSupportCount -eq 1 -and [int]$Record.SupportMatrix.ExcludedSupportCount -eq 9) -Path $Path -Issue 'Release review support matrix or support contract drifted'
    Assert-True -List $List -Condition ($Record.SupportMatrix.WinX86FullStatus -eq 'hosted-evidence-pending' -and $Record.SupportMatrix.WinX86MiniStatus -eq 'excluded') -Path $Path -Issue 'Windows x86 support status changed without hosted evidence'
    Assert-True -List $List -Condition ($Record.SigningStatus -eq 'not-ready' -and $Record.SbomStatus -eq 'not-ready') -Path $Path -Issue 'Release review must not claim signing/SBOM readiness'
    Assert-True -List $List -Condition ($Record.Reviewer.Id -eq 'automated-local-preflight' -and $Record.Reviewer.Status -eq 'completed') -Path $Path -Issue 'Release review reviewer identity/status is invalid'
    Assert-True -List $List -Condition ($Record.Approver.Id -eq 'unassigned' -and $Record.Approver.Status -eq 'not-approved') -Path $Path -Issue 'Release review approver must remain explicitly unassigned/not-approved'
    Assert-True -List $List -Condition (-not [bool]$Record.PrivateKeyMaterialPresent -and -not [bool]$Record.SecretMaterialPresent) -Path $Path -Issue 'Release review must not contain private key or secret material'
    Assert-True -List $List -Condition ([bool]$Record.Deterministic) -Path $Path -Issue 'Release review serialization must be deterministic'
    Assert-True -List $List -Condition ($Record.Publication.Decision -eq 'do-not-publish' -and $Record.Publication.Status -eq 'not-published' -and -not [bool]$Record.Publication.Allowed) -Path $Path -Issue 'Release review publication decision must remain blocked'
    Assert-True -List $List -Condition ($Record.Rollback.Status -eq 'not-published' -and (($Record.Rollback.PriorKnownGood -ne '') -or $Record.Rollback.PublicFeedStatus -eq 'not-found')) -Path $Path -Issue 'Rollback must name prior-known-good or explicit not-published feed state'

    $expectedEvidenceReferences = @(
        'scripts/Test-ReleaseCandidateProvenance.ps1',
        'scripts/Test-ReleaseReadinessContract.ps1',
        'scripts/Test-ReleaseSigningBoundary.ps1',
        'scripts/Test-ReleaseSupportContract.ps1',
        'scripts/Test-PublicFeedVerificationContract.ps1',
        'scripts/Test-TargetedPackConsumerVerificationSurface.ps1',
        'scripts/Test-WindowsRuntimePeClosure.ps1',
        '.github/workflows/runtime-input.yml',
        '.github/workflows/pack.yml'
    )
    $actualEvidenceReferences = @($Record.EvidenceReferences | ForEach-Object { [string]$_ })
    Assert-True -List $List -Condition ((@($actualEvidenceReferences | Sort-Object) -join "`n") -eq (@($expectedEvidenceReferences | Sort-Object) -join "`n")) -Path $Path -Issue 'Release review evidence references changed or are incomplete'
    Assert-True -List $List -Condition ($actualEvidenceReferences.Count -eq @($actualEvidenceReferences | Sort-Object -Unique).Count) -Path $Path -Issue 'Release review evidence references must be unique'
    foreach ($evidenceReference in $actualEvidenceReferences) {
        Assert-True -List $List -Condition (Test-Path -LiteralPath (Join-Path $repo $evidenceReference) -PathType Leaf) -Path $Path -Issue 'Release review evidence reference is missing' -Text $evidenceReference
    }

    $actualPackages = @($Packages | Sort-Object PackageId, PackageVersion, FileName | ForEach-Object { "$($_.PackageId)|$($_.PackageVersion)|$($_.FileName)|$($_.Sha256)|$($_.Length)" })
    $recordPackages = @($Record.Packages | Sort-Object PackageId, PackageVersion, FileName | ForEach-Object { "$($_.PackageId)|$($_.PackageVersion)|$($_.FileName)|$($_.Sha256)|$($_.Length)" })
    Assert-True -List $List -Condition ([string]::Join("`n", $actualPackages) -eq [string]::Join("`n", $recordPackages)) -Path $Path -Issue 'Release review package hashes or identities do not match actual packages'
    foreach ($package in $Record.Packages) {
        Assert-True -List $List -Condition ($package.PackageId -match '^JYPPX\.OpenCV(?:\.CSharp\.API|\.runtime\.[a-z0-9.-]+(?:\.mini)?)$') -Path $Path -Issue 'Release review package identity must remain version-neutral' -Text $package.PackageId
    }

    $expectedAreas = @('abi-api','feed-trust','hosted-x86','package-metadata','public-feed','release-scripts','runtime-matrix','signing-sbom','support-contract','toolchain-pins','workflow-permissions')
    $actualAreas = @($Record.ChangeControl | ForEach-Object { $_.Area } | Sort-Object)
    Assert-True -List $List -Condition ([string]::Join(',', $actualAreas) -eq [string]::Join(',', $expectedAreas)) -Path $Path -Issue 'Release change-control area list changed'
    foreach ($entry in $Record.ChangeControl) {
        Assert-True -List $List -Condition (-not [string]::IsNullOrWhiteSpace([string]$entry.Guard) -and (Test-Path -LiteralPath (Join-Path $repo $entry.Guard) -PathType Leaf)) -Path $Path -Issue 'Release change-control guard reference is missing' -Text $entry.Guard
    }

    $gate = $Record.HostedGate
    Assert-True -List $List -Condition ($gate.Target -eq 'win-x86/full' -and $gate.Status -eq 'hosted-evidence-pending') -Path $Path -Issue 'Hosted x86 gate must remain pending until hosted evidence exists'
    Assert-True -List $List -Condition ($gate.ProducerWorkflow -eq '.github/workflows/runtime-input.yml' -and $gate.ProducerRunner -eq 'windows-latest' -and $gate.ProducerArtifact -eq 'runtime-input-win-x86-full') -Path $Path -Issue 'Hosted x86 producer handoff changed'
    Assert-True -List $List -Condition ($gate.PackWorkflow -eq '.github/workflows/pack.yml' -and $gate.PackageArtifact -eq 'nupkg-win-x86-full') -Path $Path -Issue 'Hosted x86 package handoff changed'
    Assert-True -List $List -Condition ($gate.HostArchitecture -eq 'AMD64' -and $gate.TargetArchitecture -eq 'X86' -and $gate.PeMachine -eq 'I386' -and $gate.Wow64Probe -eq 'required' -and $gate.ConsumerProcessArchitecture -eq 'X86') -Path $Path -Issue 'Hosted x86 architecture gate is incomplete'
    $expectedSequence = @('hosted-producer', 'artifact-handoff', 'same-run-pack', 'independent-artifact-audit', 'x86-consumer')
    Assert-True -List $List -Condition ((@($gate.Sequence) -join ',') -eq ($expectedSequence -join ',')) -Path $Path -Issue 'Hosted x86 evidence sequence changed'
    $expectedStopConditions = @('missing-artifact', 'hash-mismatch', 'wrong-pe-machine', 'wrong-consumer-architecture', 'synthetic-input', 'path-or-loader-override', 'incomplete-provenance', 'failed-signing-or-sbom-handoff', 'publication-attempt')
    Assert-True -List $List -Condition ((@($gate.StopConditions | Sort-Object) -join ',') -eq (@($expectedStopConditions | Sort-Object) -join ',')) -Path $Path -Issue 'Hosted x86 stop conditions changed'

    $runtimeInputWorkflowPath = Join-Path $repo $gate.ProducerWorkflow
    $packWorkflowPath = Join-Path $repo $gate.PackWorkflow
    $runtimeInputWorkflowText = [IO.File]::ReadAllText($runtimeInputWorkflowPath)
    $packWorkflowText = [IO.File]::ReadAllText($packWorkflowPath)
    foreach ($requirement in @(
        [pscustomobject]@{ Text = 'processor_architecture: AMD64'; Path = $runtimeInputWorkflowPath },
        [pscustomobject]@{ Text = 'runtime_architecture: X64'; Path = $runtimeInputWorkflowPath },
        [pscustomobject]@{ Text = 'package_architecture: x86'; Path = $runtimeInputWorkflowPath },
        [pscustomobject]@{ Text = 'platform: Win32'; Path = $runtimeInputWorkflowPath },
        [pscustomobject]@{ Text = 'tool_host: Hostx64'; Path = $runtimeInputWorkflowPath },
        [pscustomobject]@{ Text = 'tool_target: x86'; Path = $runtimeInputWorkflowPath },
        [pscustomobject]@{ Text = 'runtime-input-win-x86-full'; Path = $runtimeInputWorkflowPath },
        [pscustomobject]@{ Text = "real_runtime_artifact_run_id"; Path = $packWorkflowPath },
        [pscustomobject]@{ Text = 'runtime-input-${{ matrix.rid }}-${{ matrix.profile }}'; Path = $packWorkflowPath },
        [pscustomobject]@{ Text = "inputs.rid == 'win-x86' && inputs.runtime_profile == 'full' && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'"; Path = $packWorkflowPath },
        [pscustomobject]@{ Text = 'name: nupkg-win-x86-full'; Path = $packWorkflowPath },
        [pscustomobject]@{ Text = 'Test-WindowsRuntimePeClosure.ps1'; Path = $packWorkflowPath },
        [pscustomobject]@{ Text = '-NativeExecutionHost'; Path = $packWorkflowPath }
    )) {
        $workflowText = if ($requirement.Path -eq $runtimeInputWorkflowPath) { $runtimeInputWorkflowText } else { $packWorkflowText }
        Assert-True -List $List -Condition $workflowText.Contains($requirement.Text) -Path $requirement.Path -Issue 'Hosted x86 gate workflow requirement is missing' -Text $requirement.Text
    }
    $x86VerifierMatch = [regex]::Match($packWorkflowText, '(?ms)^  verify-targeted-real-windows-x86:\r?\n.*?(?=^  verify-targeted-real-windows-x64:)')
    Assert-True -List $List -Condition $x86VerifierMatch.Success -Path $packWorkflowPath -Issue 'Hosted x86 verifier job could not be isolated'
    if ($x86VerifierMatch.Success) {
        $x86VerifierText = $x86VerifierMatch.Value
        Assert-True -List $List -Condition (-not $x86VerifierText.Contains('dotnet nuget push')) -Path $packWorkflowPath -Issue 'Hosted x86 verifier must not publish packages'
        Assert-True -List $List -Condition ($x86VerifierText.Contains('CompileNativeSmoke') -and $x86VerifierText.Contains('RunNativeSmoke')) -Path $packWorkflowPath -Issue 'Hosted x86 verifier must run native smoke'
    }
}

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-csharp-release-review-' + [guid]::NewGuid().ToString('N'))
try {
    New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null
    $externalPackages = -not [string]::IsNullOrWhiteSpace($PackageRoot)
    if ($externalPackages) {
        $resolvedPackageRoot = (Resolve-Path -LiteralPath $PackageRoot).Path
    }
    else {
        $resolvedPackageRoot = Join-Path $temporaryRoot 'packages'
        New-FixturePackage -Path (Join-Path $resolvedPackageRoot 'JYPPX.OpenCV.CSharp.API.5.0.0.nupkg') -Id 'JYPPX.OpenCV.CSharp.API' -Version '5.0.0.0'
        New-FixturePackage -Path (Join-Path $resolvedPackageRoot 'JYPPX.OpenCV.runtime.win-x64.5.0.0.nupkg') -Id 'JYPPX.OpenCV.runtime.win-x64' -Version '5.0.0.0'
    }

    $packageFiles = @(Get-ChildItem -LiteralPath $resolvedPackageRoot -Filter *.nupkg -File | Sort-Object Name)
    if ($packageFiles.Count -eq 0) { throw "No local NuGet packages were found: $resolvedPackageRoot" }
    $packages = @($packageFiles | ForEach-Object { [pscustomobject](Get-PackageRecord -Path $_.FullName) })
    $sourceRevision = (& git -C $repo rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or $sourceRevision -notmatch '^[0-9a-f]{40}$') { throw 'Source revision could not be resolved.' }
    $matrixPath = Join-Path $repo 'packaging/runtime/runtime-package-matrix.json'
    $matrix = [IO.File]::ReadAllText($matrixPath) | ConvertFrom-Json
    $matrixSha256 = (Get-FileHash -LiteralPath $matrixPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $matrixEntryCount = @($matrix.rids).Count * @($matrix.profiles).Count
    $supportContractPath = Join-Path $repo 'packaging/runtime/runtime-support-contract.json'
    $supportContractSha256 = (Get-FileHash -LiteralPath $supportContractPath -Algorithm SHA256).Hash.ToLowerInvariant()

    $record = New-ReviewRecord -Packages $packages -SourceRevision $sourceRevision -MatrixSha256 $matrixSha256 -MatrixEntryCount $matrixEntryCount -SupportContractSha256 $supportContractSha256 -ExternalPackages $externalPackages
    $json = $record | ConvertTo-Json -Depth 12
    $recordPath = Join-Path $temporaryRoot 'release-review.json'
    [IO.File]::WriteAllText($recordPath, $json, [Text.UTF8Encoding]::new($false))
    Test-ReviewRecord -Record ($json | ConvertFrom-Json) -Packages $packages -SourceRevision $sourceRevision -MatrixSha256 $matrixSha256 -MatrixEntryCount $matrixEntryCount -SupportContractSha256 $supportContractSha256 -List $violations -Path $recordPath

    $negativeCases = @('hash drift','wrong reviewer','missing rollback target','premature publication','private key','secret token','nondeterministic record','changed support matrix','changed support contract','hosted gate drift')
    foreach ($case in $negativeCases) {
        $copy = $json | ConvertFrom-Json
        switch ($case) {
            'hash drift' { $copy.Packages[0].Sha256 = ('0' * 64) }
            'wrong reviewer' { $copy.Reviewer.Id = 'unknown' }
            'missing rollback target' { $copy.Rollback.PriorKnownGood = ''; $copy.Rollback.PublicFeedStatus = 'available' }
            'premature publication' { $copy.Publication.Decision = 'publish'; $copy.Publication.Status = 'published'; $copy.Publication.Allowed = $true }
            'private key' { $copy.PrivateKeyMaterialPresent = $true }
            'secret token' { $copy.SecretMaterialPresent = $true }
            'nondeterministic record' { $copy.Deterministic = $false }
            'changed support matrix' { $copy.SupportMatrix.Sha256 = ('0' * 64) }
            'changed support contract' { $copy.SupportMatrix.SupportContractSha256 = ('0' * 64) }
            'hosted gate drift' { $copy.HostedGate.Sequence[0] = 'publication-attempt' }
        }
        $caseViolations = [Collections.Generic.List[object]]::new()
        Test-ReviewRecord -Record $copy -Packages $packages -SourceRevision $sourceRevision -MatrixSha256 $matrixSha256 -MatrixEntryCount $matrixEntryCount -SupportContractSha256 $supportContractSha256 -List $caseViolations -Path "$recordPath/$case"
        Assert-True -List $violations -Condition ($caseViolations.Count -gt 0) -Path $case -Issue 'Release change-control negative fixture was accepted'
    }

    $jsonAgain = $record | ConvertTo-Json -Depth 12
    Assert-True -List $violations -Condition ($json -eq $jsonAgain) -Path $recordPath -Issue 'Release review serialization is not deterministic'
    Write-Host "RELEASE_CHANGE_CONTROL_OK packages=$($packages.Count) support_matrix_entries=$matrixEntryCount publication=not-published approval=not-approved evidence_kind=$($record.PackageEvidenceKind)"
    foreach ($package in $packages) { Write-Host "RELEASE_PACKAGE_HASH package=$($package.PackageId)/$($package.PackageVersion) sha256=$($package.Sha256) bytes=$($package.Length)" }
}
catch {
    Add-Violation -List $violations -Path $temporaryRoot -Issue 'Release change-control execution failed' -Text $_.Exception.Message
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot -PathType Container) { [IO.Directory]::Delete((Resolve-Path -LiteralPath $temporaryRoot).Path, $true) }
}

if ($violations.Count -gt 0) {
    Write-Host "Release change-control record failed with $($violations.Count) violation(s)."
    $violations | Format-List Path, Issue, Text
    exit 1
}

Write-Host 'Release change-control record passed.'
Write-Host 'Negative fixtures rejected: hash drift, wrong reviewer, missing rollback, premature publication, private key, secret token, nondeterminism, support-matrix drift, support-contract drift, hosted gate drift.'
