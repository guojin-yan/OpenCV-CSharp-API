param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$PackageRoot = "",
    [string]$SbomRoot = "",
    [string]$OutputPath = "",
    [ValidatePattern('^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$')]
    [string]$Created = "2000-01-01T00:00:00Z",
    [int]$ExpectedPackageCount = 0,
    [switch]$Check
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

function Get-AttributeValue {
    param([System.Xml.XmlNode]$Node,[Parameter(Mandatory = $true)][string]$Name)
    if ($null -eq $Node -or $null -eq $Node.Attributes -or $null -eq $Node.Attributes[$Name]) { return '' }
    return $Node.Attributes[$Name].Value.Trim()
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
        $repositoryNode = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='repository']")
        $fileEntries = @($archive.Entries | Where-Object { -not [string]::IsNullOrWhiteSpace($_.FullName) -and -not $_.FullName.EndsWith('/') })
        $normalizedTimestamp = [DateTimeOffset]::new(2000, 1, 1, 0, 0, 0, [TimeSpan]::Zero)
        return [ordered]@{
            PackageId = Get-NuspecValue -Nuspec $nuspec -Name 'id'
            PackageVersion = Get-NuspecValue -Nuspec $nuspec -Name 'version'
            FileName = [IO.Path]::GetFileName($Path)
            Sha256 = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
            Length = (Get-Item -LiteralPath $Path).Length
            EntryCount = $fileEntries.Count
            RepositoryUrl = Get-AttributeValue -Node $repositoryNode -Name 'url'
            RepositoryType = Get-AttributeValue -Node $repositoryNode -Name 'type'
            RepositoryCommit = Get-AttributeValue -Node $repositoryNode -Name 'commit'
            Unsigned = @($fileEntries | Where-Object { $_.FullName.Equals('.signature.p7s', [StringComparison]::OrdinalIgnoreCase) }).Count -eq 0
            DeterministicallyNormalized = @($fileEntries | Where-Object { $_.LastWriteTime.DateTime -ne $normalizedTimestamp.DateTime }).Count -eq 0
        }
    }
    finally { $archive.Dispose() }
}

function Get-SbomRecord {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][object[]]$Packages,
        [Parameter(Mandatory = $true)][string]$SourceRevision,
        [Parameter(Mandatory = $true)][string]$ExpectedCreated
    )

    $bytes = [IO.File]::ReadAllBytes($Path)
    $text = [Text.UTF8Encoding]::new($false, $true).GetString($bytes)
    $document = $text | ConvertFrom-Json
    if ($document.spdxVersion -ne 'SPDX-2.3' -or $document.dataLicense -ne 'CC0-1.0' -or $document.SPDXID -ne 'SPDXRef-DOCUMENT') {
        throw "SBOM document header is not SPDX-2.3: $Path"
    }

    $knownIds = @($Packages | ForEach-Object { [string]$_.PackageId })
    $mainPackages = @($document.packages | Where-Object { [string]$_.name -in $knownIds })
    if ($mainPackages.Count -ne 1) { throw "SBOM must describe exactly one candidate package: $Path" }
    $mainPackage = $mainPackages[0]
    $package = @($Packages | Where-Object { $_.PackageId -eq $mainPackage.name })
    if ($package.Count -ne 1) { throw "SBOM package identity is not unique in the candidate: $Path" }
    $package = $package[0]

    $sha256Checksums = @($mainPackage.checksums | Where-Object { $_.algorithm -eq 'SHA256' } | ForEach-Object { [string]$_.checksumValue })
    $createdValue = if ($document.creationInfo.created -is [DateTime]) {
        $document.creationInfo.created.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
    }
    else { [string]$document.creationInfo.created }
    $sourceInfo = [string]$mainPackage.sourceInfo
    if ($mainPackage.versionInfo -ne $package.PackageVersion -or
        $sha256Checksums.Count -ne 1 -or
        $sha256Checksums[0] -ne $package.Sha256 -or
        $createdValue -ne $ExpectedCreated -or
        $sourceInfo.IndexOf($SourceRevision, [StringComparison]::Ordinal) -lt 0 -or
        @($document.files).Count -ne [int]$package.EntryCount) {
        throw "SBOM does not bind the exact package, source commit, creation time, and file closure: $Path"
    }

    return [ordered]@{
        PackageId = [string]$package.PackageId
        PackageVersion = [string]$package.PackageVersion
        FileName = [IO.Path]::GetFileName($Path)
        Sha256 = ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))).ToLowerInvariant()
        Length = $bytes.LongLength
        PackageSha256 = [string]$package.Sha256
        Created = $createdValue
        Format = 'SPDX-2.3'
        FileCount = @($document.files).Count
    }
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
    param(
        [Parameter(Mandatory = $true)][object[]]$Packages,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Sboms,
        [Parameter(Mandatory = $true)][string]$SourceRevision,
        [Parameter(Mandatory = $true)][string]$CreatedValue,
        [Parameter(Mandatory = $true)][string]$MatrixSha256,
        [Parameter(Mandatory = $true)][int]$MatrixEntryCount,
        [Parameter(Mandatory = $true)][string]$SupportContractSha256,
        [Parameter(Mandatory = $true)][object]$Closeout,
        [Parameter(Mandatory = $true)][bool]$ExternalPackages
    )

    [ordered]@{
        SchemaVersion = 2
        RecordKind = 'release-change-control'
        CandidateId = "local-review/$SourceRevision/$($Packages.Count)"
        Created = $CreatedValue
        SourceRevision = $SourceRevision
        OpenCvRevision = '5.0.0'
        PackageEvidenceKind = if ($ExternalPackages) { 'current-unsigned-candidate' } else { 'deterministic-fixture' }
        Packages = @($Packages | Sort-Object PackageId, PackageVersion, FileName)
        SbomEvidenceKind = if ($Sboms.Count -gt 0) { 'deterministic-package-bound' } else { 'not-provisioned' }
        Sboms = @($Sboms | Sort-Object PackageId, FileName)
        Closeout = $Closeout
        SupportMatrix = [ordered]@{
            Sha256 = $MatrixSha256
            EntryCount = $MatrixEntryCount
            SupportContractSha256 = $SupportContractSha256
            RealSupportCount = 28
            PendingSupportCount = 5
            ExcludedSupportCount = 1
            WinX86FullStatus = 'hosted-evidence-pending'
            WinX86MiniStatus = 'excluded'
        }
        EvidenceReferences = @(
            'scripts/Test-ReleaseCandidateProvenance.ps1',
            'scripts/Test-ReleaseReadinessContract.ps1',
            'scripts/Test-ReleaseSigningBoundary.ps1',
            'scripts/Test-NuGetRepositorySigningBoundary.ps1',
            'scripts/Test-NuGetRepositorySignedPackage.ps1',
            'scripts/New-NuGetPublicationBundle.ps1',
            'scripts/Test-NuGetPublicationManifest.ps1',
            'scripts/New-ReleasePackageSbom.ps1',
            'scripts/Test-ReleasePackageSbom.ps1',
            'scripts/Test-ReleaseSupportContract.ps1',
            'scripts/Test-PublicFeedVerificationContract.ps1',
            'scripts/Test-TargetedPackConsumerVerificationSurface.ps1',
            'scripts/Test-WindowsRuntimePeClosure.ps1',
            '.github/workflows/runtime-input.yml',
            '.github/workflows/pack.yml',
            '.github/workflows/publish-nuget.yml'
        )
        SigningStatus = 'repository-signing-pending'
        SbomStatus = if ($Sboms.Count -eq $Packages.Count -and $Packages.Count -gt 0) { 'generated-unapproved' } else { 'not-ready' }
        Reviewer = [ordered]@{ Id = 'automated-local-preflight'; Status = 'completed' }
        Approver = [ordered]@{ Id = 'unassigned'; Status = 'not-approved' }
        ChangeSummary = if ($ExternalPackages) { 'first-preview-publication-handoff' } else { 'deterministic-release-guard-fixture' }
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
            [ordered]@{ Area = 'repository-signing'; Guard = 'scripts/Test-NuGetRepositorySigningBoundary.ps1' },
            [ordered]@{ Area = 'package-sbom'; Guard = 'scripts/Test-ReleasePackageSbom.ps1' },
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
        RequiredApprovalInputs = @(
            'nuget-production-environment',
            'nuget-api-key-custody-owner',
            'designated-publisher',
            'independent-approver',
            'explicit-publication-authorization',
            'post-publish-repository-signature-verification',
            'github-packages-public-visibility',
            'github-packages-verification'
        )
        PrivateKeyMaterialPresent = $false
        SecretMaterialPresent = $false
        Deterministic = $true
    }
}

function Test-ReviewRecord {
    param(
        [Parameter(Mandatory = $true)][object]$Record,
        [Parameter(Mandatory = $true)][object[]]$Packages,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Sboms,
        [Parameter(Mandatory = $true)][string]$SourceRevision,
        [Parameter(Mandatory = $true)][string]$CreatedValue,
        [Parameter(Mandatory = $true)][string]$MatrixSha256,
        [Parameter(Mandatory = $true)][int]$MatrixEntryCount,
        [Parameter(Mandatory = $true)][string]$SupportContractSha256,
        [Parameter(Mandatory = $true)][object]$ExpectedCloseout,
        [Parameter(Mandatory = $true)][bool]$ExternalPackages,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory = $true)][string]$Path
    )

    foreach ($field in @('SchemaVersion','RecordKind','CandidateId','Created','SourceRevision','OpenCvRevision','PackageEvidenceKind','Packages','SbomEvidenceKind','Sboms','Closeout','SupportMatrix','EvidenceReferences','SigningStatus','SbomStatus','Reviewer','Approver','ChangeSummary','ChangeControl','HostedGate','Rollback','Publication','RequiredApprovalInputs','PrivateKeyMaterialPresent','SecretMaterialPresent','Deterministic')) {
        Assert-True -List $List -Condition ($null -ne $Record.PSObject.Properties[$field]) -Path $Path -Issue 'Release review record is missing required field' -Text $field
    }
    if ($null -eq $Record.SupportMatrix -or $null -eq $Record.Reviewer -or $null -eq $Record.Approver -or $null -eq $Record.Rollback -or $null -eq $Record.Publication -or $null -eq $Record.HostedGate) { return }

    Assert-True -List $List -Condition ([int]$Record.SchemaVersion -eq 2 -and $Record.RecordKind -eq 'release-change-control') -Path $Path -Issue 'Release review schema identity must be version 2'
    $recordCreated = if ($Record.Created -is [DateTime]) { $Record.Created.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") } else { [string]$Record.Created }
    Assert-True -List $List -Condition ($recordCreated -eq $CreatedValue) -Path $Path -Issue 'Release review creation timestamp drifted'
    Assert-True -List $List -Condition ($Record.SourceRevision -eq $SourceRevision) -Path $Path -Issue 'Release review source revision drifted'
    Assert-True -List $List -Condition ($Record.CandidateId -eq "local-review/$SourceRevision/$($Packages.Count)" -and $Record.OpenCvRevision -eq '5.0.0') -Path $Path -Issue 'Release review candidate or OpenCV identity drifted'
    $expectedPackageEvidenceKind = if ($ExternalPackages) { 'current-unsigned-candidate' } else { 'deterministic-fixture' }
    Assert-True -List $List -Condition ($Record.PackageEvidenceKind -eq $expectedPackageEvidenceKind) -Path $Path -Issue 'Release review package evidence classification drifted'
    $expectedChangeSummary = if ($ExternalPackages) { 'first-preview-publication-handoff' } else { 'deterministic-release-guard-fixture' }
    Assert-True -List $List -Condition ($Record.ChangeSummary -eq $expectedChangeSummary) -Path $Path -Issue 'Release review change summary drifted'
    Assert-True -List $List -Condition ($Record.SupportMatrix.Sha256 -eq $MatrixSha256 -and [int]$Record.SupportMatrix.EntryCount -eq $MatrixEntryCount -and $Record.SupportMatrix.SupportContractSha256 -eq $SupportContractSha256 -and [int]$Record.SupportMatrix.RealSupportCount -eq 28 -and [int]$Record.SupportMatrix.PendingSupportCount -eq 5 -and [int]$Record.SupportMatrix.ExcludedSupportCount -eq 1) -Path $Path -Issue 'Release review support matrix or support contract drifted'
    Assert-True -List $List -Condition ($Record.SupportMatrix.WinX86FullStatus -eq 'hosted-evidence-pending' -and $Record.SupportMatrix.WinX86MiniStatus -eq 'excluded') -Path $Path -Issue 'Windows x86 support status changed without hosted evidence'
    $expectedCloseoutValues = "$($ExpectedCloseout.Path)|$($ExpectedCloseout.Sha256)|$($ExpectedCloseout.CandidateId)|$($ExpectedCloseout.SourceSetSha256)|$($ExpectedCloseout.Status)|$($ExpectedCloseout.InvariantGuardCount)|$($ExpectedCloseout.SigningStatus)|$($ExpectedCloseout.SbomStatus)|$($ExpectedCloseout.ApprovalStatus)|$($ExpectedCloseout.PublicationAllowed)"
    $recordCloseoutValues = "$($Record.Closeout.Path)|$($Record.Closeout.Sha256)|$($Record.Closeout.CandidateId)|$($Record.Closeout.SourceSetSha256)|$($Record.Closeout.Status)|$($Record.Closeout.InvariantGuardCount)|$($Record.Closeout.SigningStatus)|$($Record.Closeout.SbomStatus)|$($Record.Closeout.ApprovalStatus)|$($Record.Closeout.PublicationAllowed)"
    Assert-True -List $List -Condition ($recordCloseoutValues -ceq $expectedCloseoutValues -and $Record.Closeout.Status -eq 'locally-validated' -and [int]$Record.Closeout.InvariantGuardCount -eq 76 -and $Record.Closeout.SigningStatus -eq 'repository-signing-pending' -and $Record.Closeout.ApprovalStatus -eq 'not-approved' -and -not [bool]$Record.Closeout.PublicationAllowed -and $Record.Closeout.Sha256 -match '^[0-9a-f]{64}$') -Path $Path -Issue 'Release review closeout binding drifted'
    $expectedSbomStatus = if ($Sboms.Count -eq $Packages.Count -and $Packages.Count -gt 0) { 'generated-unapproved' } else { 'not-ready' }
    $expectedSbomEvidenceKind = if ($Sboms.Count -gt 0) { 'deterministic-package-bound' } else { 'not-provisioned' }
    Assert-True -List $List -Condition ($Record.SigningStatus -eq 'repository-signing-pending' -and $Record.SbomStatus -eq $expectedSbomStatus -and $Record.SbomEvidenceKind -eq $expectedSbomEvidenceKind) -Path $Path -Issue 'Release review signing/SBOM state drifted'
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
        'scripts/Test-NuGetRepositorySigningBoundary.ps1',
        'scripts/Test-NuGetRepositorySignedPackage.ps1',
        'scripts/New-NuGetPublicationBundle.ps1',
        'scripts/Test-NuGetPublicationManifest.ps1',
        'scripts/New-ReleasePackageSbom.ps1',
        'scripts/Test-ReleasePackageSbom.ps1',
        'scripts/Test-ReleaseSupportContract.ps1',
        'scripts/Test-PublicFeedVerificationContract.ps1',
        'scripts/Test-TargetedPackConsumerVerificationSurface.ps1',
        'scripts/Test-WindowsRuntimePeClosure.ps1',
        '.github/workflows/runtime-input.yml',
        '.github/workflows/pack.yml',
        '.github/workflows/publish-nuget.yml'
    )
    $actualEvidenceReferences = @($Record.EvidenceReferences | ForEach-Object { [string]$_ })
    Assert-True -List $List -Condition ((@($actualEvidenceReferences | Sort-Object) -join "`n") -eq (@($expectedEvidenceReferences | Sort-Object) -join "`n")) -Path $Path -Issue 'Release review evidence references changed or are incomplete'
    Assert-True -List $List -Condition ($actualEvidenceReferences.Count -eq @($actualEvidenceReferences | Sort-Object -Unique).Count) -Path $Path -Issue 'Release review evidence references must be unique'
    foreach ($evidenceReference in $actualEvidenceReferences) {
        Assert-True -List $List -Condition (Test-Path -LiteralPath (Join-Path $repo $evidenceReference) -PathType Leaf) -Path $Path -Issue 'Release review evidence reference is missing' -Text $evidenceReference
    }

    $actualPackages = @($Packages | Sort-Object PackageId, PackageVersion, FileName | ForEach-Object { "$($_.PackageId)|$($_.PackageVersion)|$($_.FileName)|$($_.Sha256)|$($_.Length)|$($_.EntryCount)|$($_.RepositoryUrl)|$($_.RepositoryType)|$($_.RepositoryCommit)|$($_.Unsigned)|$($_.DeterministicallyNormalized)" })
    $recordPackages = @($Record.Packages | Sort-Object PackageId, PackageVersion, FileName | ForEach-Object { "$($_.PackageId)|$($_.PackageVersion)|$($_.FileName)|$($_.Sha256)|$($_.Length)|$($_.EntryCount)|$($_.RepositoryUrl)|$($_.RepositoryType)|$($_.RepositoryCommit)|$($_.Unsigned)|$($_.DeterministicallyNormalized)" })
    Assert-True -List $List -Condition ([string]::Join("`n", $actualPackages) -eq [string]::Join("`n", $recordPackages)) -Path $Path -Issue 'Release review package hashes or identities do not match actual packages'
    foreach ($package in $Record.Packages) {
        Assert-True -List $List -Condition ($package.PackageId -match '^JYPPX\.OpenCV(?:\.CSharp\.API|\.runtime\.[a-z0-9.-]+(?:\.mini)?)$') -Path $Path -Issue 'Release review package identity must remain version-neutral' -Text $package.PackageId
        Assert-True -List $List -Condition ([bool]$package.Unsigned -and [bool]$package.DeterministicallyNormalized) -Path $Path -Issue 'Release review packages must remain normalized and unsigned' -Text $package.FileName
        if ($ExternalPackages) {
            Assert-True -List $List -Condition ($package.RepositoryUrl -eq 'https://github.com/guojin-yan/OpenCV-CSharp-API' -and $package.RepositoryType -eq 'git' -and $package.RepositoryCommit -eq $SourceRevision) -Path $Path -Issue 'Current candidate package repository provenance drifted' -Text $package.FileName
        }
    }

    $actualSboms = @($Sboms | Sort-Object PackageId, FileName | ForEach-Object { "$($_.PackageId)|$($_.PackageVersion)|$($_.FileName)|$($_.Sha256)|$($_.Length)|$($_.PackageSha256)|$($_.Created)|$($_.Format)|$($_.FileCount)" })
    $recordSboms = @($Record.Sboms | Sort-Object PackageId, FileName | ForEach-Object {
            $sbomCreated = if ($_.Created -is [DateTime]) { $_.Created.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") } else { [string]$_.Created }
            "$($_.PackageId)|$($_.PackageVersion)|$($_.FileName)|$($_.Sha256)|$($_.Length)|$($_.PackageSha256)|$sbomCreated|$($_.Format)|$($_.FileCount)"
        })
    Assert-True -List $List -Condition ([string]::Join("`n", $actualSboms) -eq [string]::Join("`n", $recordSboms)) -Path $Path -Issue 'Release review SBOM hashes or package bindings do not match actual documents'

    $requiredApprovalInputs = @('designated-publisher','explicit-publication-authorization','github-packages-public-visibility','github-packages-verification','independent-approver','nuget-api-key-custody-owner','nuget-production-environment','post-publish-repository-signature-verification')
    Assert-True -List $List -Condition ((@($Record.RequiredApprovalInputs | Sort-Object) -join ',') -eq ($requiredApprovalInputs -join ',')) -Path $Path -Issue 'Release review approval input checklist drifted'

    $expectedAreas = @('abi-api','feed-trust','hosted-x86','package-metadata','package-sbom','public-feed','release-scripts','repository-signing','runtime-matrix','signing-sbom','support-contract','toolchain-pins','workflow-permissions')
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
        [pscustomobject]@{ Text = "'win-x86/full'"; Path = $runtimeInputWorkflowPath },
        [pscustomobject]@{ Text = 'runtime-input-${{ matrix.rid }}-${{ matrix.profile }}'; Path = $runtimeInputWorkflowPath },
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
    if ($Check -and [string]::IsNullOrWhiteSpace($OutputPath)) { throw '-Check requires -OutputPath.' }
    if (-not [string]::IsNullOrWhiteSpace($SbomRoot) -and [string]::IsNullOrWhiteSpace($PackageRoot)) { throw '-SbomRoot requires -PackageRoot.' }
    $createdTimestamp = [DateTimeOffset]::MinValue
    if (-not [DateTimeOffset]::TryParseExact($Created, "yyyy-MM-dd'T'HH:mm:ss'Z'", [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::AssumeUniversal -bor [Globalization.DateTimeStyles]::AdjustToUniversal, [ref]$createdTimestamp)) {
        throw "Created must be a factual UTC RFC3339 timestamp with whole seconds. Actual: $Created"
    }

    New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null
    $externalPackages = -not [string]::IsNullOrWhiteSpace($PackageRoot)
    if ($externalPackages -and $Created -eq '2000-01-01T00:00:00Z') { throw 'External release candidates require an explicit factual -Created timestamp.' }
    if ($externalPackages -and -not [string]::IsNullOrWhiteSpace($OutputPath) -and $ExpectedPackageCount -le 0) { throw 'Durable current-candidate output requires an explicit positive -ExpectedPackageCount.' }
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
    if ($ExpectedPackageCount -gt 0 -and $packageFiles.Count -ne $ExpectedPackageCount) { throw "Package count does not match the expected release handoff. Expected: $ExpectedPackageCount; actual: $($packageFiles.Count)" }
    $packages = @($packageFiles | ForEach-Object { [pscustomobject](Get-PackageRecord -Path $_.FullName) })
    $sourceRevision = (& git -C $repo rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or $sourceRevision -notmatch '^[0-9a-f]{40}$') { throw 'Source revision could not be resolved.' }
    if ($externalPackages) {
        $packageVersions = @($packages | ForEach-Object { [string]$_.PackageVersion } | Sort-Object -Unique)
        if ($packageVersions.Count -ne 1) { throw 'Current release candidate packages must share one normalized NuGet version.' }
        foreach ($package in $packages) {
            $expectedFileName = "$($package.PackageId).$($package.PackageVersion).nupkg"
            if ($package.FileName -cne $expectedFileName -or
                -not [bool]$package.Unsigned -or
                -not [bool]$package.DeterministicallyNormalized -or
                $package.RepositoryUrl -ne 'https://github.com/guojin-yan/OpenCV-CSharp-API' -or
                $package.RepositoryType -ne 'git' -or
                $package.RepositoryCommit -ne $sourceRevision) {
                throw "Current release candidate package identity, normalization, signature, or repository provenance is invalid: $($package.FileName)"
            }
        }
    }

    $sboms = @()
    if (-not [string]::IsNullOrWhiteSpace($SbomRoot)) {
        $resolvedSbomRoot = (Resolve-Path -LiteralPath $SbomRoot).Path
        $sbomFiles = @(Get-ChildItem -LiteralPath $resolvedSbomRoot -Filter *.json -File | Sort-Object Name)
        $sboms = @($sbomFiles | ForEach-Object { [pscustomobject](Get-SbomRecord -Path $_.FullName -Packages $packages -SourceRevision $sourceRevision -ExpectedCreated $Created) })
        if ($sboms.Count -ne $packages.Count -or @($sboms.PackageId | Sort-Object -Unique).Count -ne $packages.Count) {
            throw 'SBOM root must contain exactly one package-bound document for every candidate package.'
        }
    }
    if ($externalPackages -and -not [string]::IsNullOrWhiteSpace($OutputPath) -and $sboms.Count -ne $packages.Count) {
        throw 'Durable current-candidate output requires one SBOM document per package.'
    }
    $matrixPath = Join-Path $repo 'packaging/runtime/runtime-package-matrix.json'
    $matrix = [IO.File]::ReadAllText($matrixPath) | ConvertFrom-Json
    $matrixSha256 = (Get-FileHash -LiteralPath $matrixPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $matrixEntryCount = @($matrix.rids).Count * @($matrix.profiles).Count
    $supportContractPath = Join-Path $repo 'packaging/runtime/runtime-support-contract.json'
    $supportContractSha256 = (Get-FileHash -LiteralPath $supportContractPath -Algorithm SHA256).Hash.ToLowerInvariant()

    $closeoutPath = Join-Path $repo 'packaging/release/local-release-candidate-closeout.json'
    $closeoutJson = [IO.File]::ReadAllText($closeoutPath) | ConvertFrom-Json
    $closeout = [ordered]@{
        Path = 'packaging/release/local-release-candidate-closeout.json'
        Sha256 = (Get-FileHash -LiteralPath $closeoutPath -Algorithm SHA256).Hash.ToLowerInvariant()
        CandidateId = [string]$closeoutJson.CandidateId
        SourceSetSha256 = [string]$closeoutJson.SourceSet.Sha256
        Status = [string]$closeoutJson.LocalValidation.Status
        InvariantGuardCount = [int]$closeoutJson.LocalValidation.InvariantGuardCount
        SigningStatus = [string]$closeoutJson.Signing.Status
        SbomStatus = [string]$closeoutJson.Sbom.Status
        ApprovalStatus = [string]$closeoutJson.Approval.Status
        PublicationAllowed = [bool]$closeoutJson.LocalValidation.PublicationAllowed
    }

    $record = New-ReviewRecord -Packages $packages -Sboms $sboms -SourceRevision $sourceRevision -CreatedValue $Created -MatrixSha256 $matrixSha256 -MatrixEntryCount $matrixEntryCount -SupportContractSha256 $supportContractSha256 -Closeout $closeout -ExternalPackages $externalPackages
    $json = (($record | ConvertTo-Json -Depth 12) -replace "`r`n", "`n").TrimEnd() + "`n"
    $recordPath = Join-Path $temporaryRoot 'release-review.json'
    [IO.File]::WriteAllText($recordPath, $json, [Text.UTF8Encoding]::new($false))
    Test-ReviewRecord -Record ($json | ConvertFrom-Json) -Packages $packages -Sboms $sboms -SourceRevision $sourceRevision -CreatedValue $Created -MatrixSha256 $matrixSha256 -MatrixEntryCount $matrixEntryCount -SupportContractSha256 $supportContractSha256 -ExpectedCloseout $closeout -ExternalPackages $externalPackages -List $violations -Path $recordPath

    $negativeCases = @('hash drift','package entry closure drift','wrong reviewer','missing rollback target','premature publication','private key','secret token','nondeterministic record','changed support matrix','changed support contract','hosted gate drift','creation timestamp drift','candidate identity drift','OpenCV revision drift','package evidence classification drift','change summary drift','unnormalized package state','SBOM state drift','closeout drift','closeout hash drift','approval input drift','record kind drift')
    if ($sboms.Count -gt 0) { $negativeCases += 'SBOM format drift' }
    foreach ($case in $negativeCases) {
        $copy = $json | ConvertFrom-Json
        switch ($case) {
            'hash drift' { $copy.Packages[0].Sha256 = ('0' * 64) }
            'package entry closure drift' { $copy.Packages[0].EntryCount = [int]$copy.Packages[0].EntryCount + 1 }
            'wrong reviewer' { $copy.Reviewer.Id = 'unknown' }
            'missing rollback target' { $copy.Rollback.PriorKnownGood = ''; $copy.Rollback.PublicFeedStatus = 'available' }
            'premature publication' { $copy.Publication.Decision = 'publish'; $copy.Publication.Status = 'published'; $copy.Publication.Allowed = $true }
            'private key' { $copy.PrivateKeyMaterialPresent = $true }
            'secret token' { $copy.SecretMaterialPresent = $true }
            'nondeterministic record' { $copy.Deterministic = $false }
            'changed support matrix' { $copy.SupportMatrix.Sha256 = ('0' * 64) }
            'changed support contract' { $copy.SupportMatrix.SupportContractSha256 = ('0' * 64) }
            'hosted gate drift' { $copy.HostedGate.Sequence[0] = 'publication-attempt' }
            'creation timestamp drift' { $copy.Created = '2001-01-01T00:00:00Z' }
            'candidate identity drift' { $copy.CandidateId = 'local-review/unknown/0' }
            'OpenCV revision drift' { $copy.OpenCvRevision = 'unknown' }
            'package evidence classification drift' { $copy.PackageEvidenceKind = 'historical-local-artifacts' }
            'change summary drift' { $copy.ChangeSummary = 'unknown' }
            'unnormalized package state' { $copy.Packages[0].DeterministicallyNormalized = $false }
            'SBOM state drift' { $copy.SbomStatus = 'verified' }
            'closeout drift' { $copy.Closeout.InvariantGuardCount = 0 }
            'closeout hash drift' { $copy.Closeout.Sha256 = ('0' * 64) }
            'approval input drift' { $copy.RequiredApprovalInputs[0] = 'author-signing-certificate-substitute' }
            'record kind drift' { $copy.RecordKind = 'unknown' }
            'SBOM format drift' { $copy.Sboms[0].Format = 'unknown' }
        }
        $caseViolations = [Collections.Generic.List[object]]::new()
        Test-ReviewRecord -Record $copy -Packages $packages -Sboms $sboms -SourceRevision $sourceRevision -CreatedValue $Created -MatrixSha256 $matrixSha256 -MatrixEntryCount $matrixEntryCount -SupportContractSha256 $supportContractSha256 -ExpectedCloseout $closeout -ExternalPackages $externalPackages -List $caseViolations -Path "$recordPath/$case"
        Assert-True -List $violations -Condition ($caseViolations.Count -gt 0) -Path $case -Issue 'Release change-control negative fixture was accepted'
    }

    $jsonAgain = (($record | ConvertTo-Json -Depth 12) -replace "`r`n", "`n").TrimEnd() + "`n"
    Assert-True -List $violations -Condition ($json -eq $jsonAgain) -Path $recordPath -Issue 'Release review serialization is not deterministic'
    if ($violations.Count -eq 0 -and -not [string]::IsNullOrWhiteSpace($OutputPath)) {
        $outputFullPath = [IO.Path]::GetFullPath($OutputPath)
        $outputBytes = [Text.UTF8Encoding]::new($false).GetBytes($json)
        if ($Check) {
            if (-not (Test-Path -LiteralPath $outputFullPath -PathType Leaf) -or -not [Linq.Enumerable]::SequenceEqual($outputBytes, [IO.File]::ReadAllBytes($outputFullPath))) {
                throw "Release change-control output is missing or stale: $outputFullPath"
            }
        }
        else {
            [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($outputFullPath)) | Out-Null
            $temporaryOutput = "$outputFullPath.tmp-$PID-$([guid]::NewGuid().ToString('N'))"
            try {
                [IO.File]::WriteAllBytes($temporaryOutput, $outputBytes)
                [IO.File]::Move($temporaryOutput, $outputFullPath, $true)
            }
            finally { if (Test-Path -LiteralPath $temporaryOutput -PathType Leaf) { [IO.File]::Delete($temporaryOutput) } }
        }
    }
    $recordSha256 = ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.UTF8Encoding]::new($false).GetBytes($json)))).ToLowerInvariant()
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

Write-Host "RELEASE_CHANGE_CONTROL_OK packages=$($packages.Count) sboms=$($sboms.Count) support_matrix_entries=$matrixEntryCount publication=not-published approval=not-approved evidence_kind=$($record.PackageEvidenceKind) record_sha256=$recordSha256 mode=$(if($Check){'check'}else{'write'})"
foreach ($package in $packages) { Write-Host "RELEASE_PACKAGE_HASH package=$($package.PackageId)/$($package.PackageVersion) sha256=$($package.Sha256) bytes=$($package.Length)" }
Write-Host 'Release change-control record passed.'
Write-Host 'Negative fixtures rejected: package/SBOM closure, timestamp, candidate/OpenCV identity, evidence classification, change summary, closeout hash/state, approval inputs, record identity, hash, reviewer, rollback, publication, private key, secret token, nondeterminism, support matrix/contract, hosted gate.'
