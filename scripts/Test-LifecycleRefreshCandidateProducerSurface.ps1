param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workflowRelativePath = '.github/workflows/runtime-input.yml'
$scriptRelativePath = 'scripts/Invoke-LifecycleRefreshContainerProducer.ps1'
$shellRelativePath = 'scripts/Invoke-LifecycleRefreshContainerProducer.sh'
$matrixRelativePath = 'packaging/runtime/runtime-lifecycle-refresh-matrix.json'
$supportRelativePath = 'packaging/runtime/runtime-support-contract.json'

function Read-RequiredText {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $path = Join-Path $repo ($RelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required lifecycle-refresh surface was not found: $RelativePath"
    }
    return [IO.File]::ReadAllText($path)
}

function Assert-Contains {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Needle,
        [Parameter(Mandatory = $true)][string]$Path
    )
    if (-not $Text.Contains($Needle, [StringComparison]::Ordinal)) {
        throw "$Path is missing the required lifecycle-refresh token: $Needle"
    }
}

$workflow = Read-RequiredText -RelativePath $workflowRelativePath
$producer = Read-RequiredText -RelativePath $scriptRelativePath
$shell = Read-RequiredText -RelativePath $shellRelativePath
$matrix = Read-RequiredText -RelativePath $matrixRelativePath | ConvertFrom-Json
$support = Read-RequiredText -RelativePath $supportRelativePath | ConvertFrom-Json

foreach ($token in @(
        'lifecycle_refresh_candidate:',
        'type: boolean',
        'default: false',
        'validate-lifecycle-refresh-candidate:',
        'produce-lifecycle-refresh-container:',
        'if: ${{ !inputs.lifecycle_refresh_candidate }}',
        'if: ${{ github.repository == ''guojin-yan/OpenCV-CSharp-API'' && inputs.lifecycle_refresh_candidate == true }}',
        'fromJSON(needs.validate-lifecycle-refresh-candidate.outputs.target_matrix)',
        'LIFECYCLE_REFRESH_TARGET_SELECTION_OK',
        'scripts/Invoke-LifecycleRefreshContainerProducer.ps1',
        'lifecycle-refresh-runtime-input-${{ matrix.rid }}-${{ matrix.profile }}',
        'artifacts/lifecycle-refresh-runtime-inputs/${{ matrix.rid }}-${{ matrix.profile }}',
        'if-no-files-found: error')) {
    Assert-Contains -Text $workflow -Needle $token -Path $workflowRelativePath
}

foreach ($token in @(
        "[ValidateSet('fedora.44-x64', 'alpine.3.23-x64')]",
        "[ValidateSet('full', 'mini')]",
        'packaging/runtime/runtime-lifecycle-refresh-matrix.json',
        'artifacts/lifecycle-refresh-runtime-inputs',
        '[string]$candidate.status -cne ''candidate-only''',
        '[bool]$candidate.publicationAllowed',
        'Lifecycle-refresh producer refuses an RID that is already in the active package matrix',
        "@sha256:[0-9a-f]{64}$",
        'Invoke-CheckedCommand $dockerCommand.Source pull',
        'image inspect',
        'RepoDigests',
        'PublicationAllowed = $false',
        'LIFECYCLE_REFRESH_CANDIDATE_PRODUCER_OK')) {
    Assert-Contains -Text $producer -Needle $token -Path $scriptRelativePath
}

foreach ($token in @(
        'powershell-7.4.17-linux-x64.tar.gz',
        'dcfe6060fc86abcb859ce1ff80843ce50bab0585396de56380ed9f25176ac6d',
        'powershell-7.4.17-linux-musl-x64.tar.gz',
        '143a1de65ea320c36a0b4bd1808fe65561e5ab12fd66d5f63f78b0b3d66b4397',
        'fedora.44-x64)',
        'alpine.3.23-x64)',
        'Test-LifecycleRefreshRuntimeMatrix.ps1',
        'Get-NativeRuntimeProfileEvidence.ps1',
        'New-RuntimeInputArtifact.ps1',
        'ctest --test-dir build/native-lifecycle-refresh',
        'LIFECYCLE_REFRESH_PRODUCER_ELF_EVIDENCE')) {
    Assert-Contains -Text $shell -Needle $token -Path $shellRelativePath
}

foreach ($workflowRelative in @('.github/workflows/pack.yml', '.github/workflows/publish-nuget.yml')) {
    $text = Read-RequiredText -RelativePath $workflowRelative
    if ($text.Contains($matrixRelativePath, [StringComparison]::Ordinal)) {
        throw "$workflowRelative must not consume the lifecycle-refresh candidate matrix."
    }
}
if ($producer -match '(?i)pack\.yml|publish-nuget|dotnet\s+nuget\s+push|gh\s+release') {
    throw "$scriptRelativePath contains a publication workflow or publication command."
}

$expectedImages = @{
    'fedora.44-x64' = 'fedora:44@sha256:43b29f65a41eb9c35e1cd5323e3bdf3b655c2357a9f4f1ff2f9c2798e5045d80'
    'alpine.3.23-x64' = 'alpine:3.23@sha256:fd791d74b68913cbb027c6546007b3f0d3bc45125f797758156952bc2d6daf40'
}
foreach ($rid in $expectedImages.Keys) {
    $matrixRow = @($matrix.rids | Where-Object { [string]$_.rid -ceq $rid })
    $supportRow = @($support.lifecycleRefreshCandidates | Where-Object { [string]$_.rid -ceq $rid })
    if ($matrixRow.Count -ne 1 -or $supportRow.Count -ne 1 -or
        [string]$matrixRow[0].producer.containerImage -cne $expectedImages[$rid] -or
        [string]$supportRow[0].containerImage -cne $expectedImages[$rid]) {
        throw "Lifecycle-refresh image binding drifted: $rid"
    }
}

Write-Host 'Lifecycle-refresh candidate producer surface passed.'
Write-Host 'The dispatch-only producer is isolated from active package and publication workflows, with digest-pinned Fedora 44/Alpine 3.23 container evidence and fixed PowerShell archive hashes.'
