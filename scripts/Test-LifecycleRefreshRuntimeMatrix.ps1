param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$violations = [System.Collections.Generic.List[object]]::new()
$candidateMatrixRelativePath = 'packaging/runtime/runtime-lifecycle-refresh-matrix.json'
$activeMatrixRelativePath = 'packaging/runtime/runtime-package-matrix.json'
$supportContractRelativePath = 'packaging/runtime/runtime-support-contract.json'

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Issue,
        [string]$Text = ''
    )

    $violations.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Issue,
        [string]$Text = ''
    )

    if (-not $Condition) {
        Add-Violation -Path $Path -Issue $Issue -Text $Text
    }
}

function Read-RequiredJson {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $path = Join-Path $repo ($RelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required lifecycle refresh file was not found: $RelativePath"
    }

    return [pscustomobject]@{
        Path = $path
        RelativePath = $RelativePath
        Value = ([IO.File]::ReadAllText($path) | ConvertFrom-Json)
    }
}

function Assert-ExactSet {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Issue,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$Expected,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$Actual
    )

    $expectedText = [string]::Join("`n", @($Expected | Sort-Object))
    $actualText = [string]::Join("`n", @($Actual | Sort-Object))
    Assert-True -Condition ($expectedText -ceq $actualText) -Path $Path -Issue $Issue -Text "expected=$expectedText actual=$actualText"
}

function Assert-ExactPropertySet {
    param(
        [Parameter(Mandatory = $true)][object]$Value,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Context,
        [Parameter(Mandatory = $true)][string[]]$Expected
    )

    Assert-ExactSet -Path $Path -Issue "$Context property set drifted" -Expected $Expected -Actual @($Value.PSObject.Properties.Name)
}

function Get-ProfileTargets {
    param([Parameter(Mandatory = $true)][object]$Matrix)

    return @($Matrix.rids | ForEach-Object {
            $rid = [string]$_.rid
            foreach ($profile in @($_.producer.profiles)) {
                "$rid/$([string]$profile)"
            }
        })
}

try {
    $candidateRecord = Read-RequiredJson -RelativePath $candidateMatrixRelativePath
    $activeRecord = Read-RequiredJson -RelativePath $activeMatrixRelativePath
    $supportRecord = Read-RequiredJson -RelativePath $supportContractRelativePath
    $candidateMatrix = $candidateRecord.Value
    $activeMatrix = $activeRecord.Value
    $supportContract = $supportRecord.Value

    Assert-ExactPropertySet -Value $candidateMatrix -Path $candidateRecord.RelativePath -Context 'Lifecycle refresh matrix root' -Expected @(
        'schemaVersion',
        'description',
        'status',
        'activePackageMatrix',
        'supportContract',
        'publicationAllowed',
        'rids',
        'profiles'
    )
    Assert-True -Condition (
        [int]$candidateMatrix.schemaVersion -eq 1 -and
        [string]$candidateMatrix.status -ceq 'candidate-only' -and
        [string]$candidateMatrix.activePackageMatrix -ceq $activeMatrixRelativePath -and
        [string]$candidateMatrix.supportContract -ceq $supportContractRelativePath -and
        -not [bool]$candidateMatrix.publicationAllowed -and
        [string]$candidateMatrix.description -match 'not an active package, support, RID-graph, or publication matrix'
    ) -Path $candidateRecord.RelativePath -Issue 'Lifecycle refresh matrix identity or non-publication policy drifted'

    $expectedCandidates = @(
        [pscustomobject]@{
            Rid = 'alpine.3.23-x64'
            Distro = 'alpine'
            DistroVersion = '3.23'
            Image = 'alpine:3.23@sha256:fd791d74b68913cbb027c6546007b3f0d3bc45125f797758156952bc2d6daf40'
        },
        [pscustomobject]@{
            Rid = 'fedora.44-x64'
            Distro = 'fedora'
            DistroVersion = '44'
            Image = 'fedora:44@sha256:43b29f65a41eb9c35e1cd5323e3bdf3b655c2357a9f4f1ff2f9c2798e5045d80'
        }
    )
    $candidateRows = @($candidateMatrix.rids)
    Assert-True -Condition ($candidateRows.Count -eq 2) -Path $candidateRecord.RelativePath -Issue 'Lifecycle refresh matrix must contain exactly two candidate RIDs'
    Assert-ExactSet -Path $candidateRecord.RelativePath -Issue 'Lifecycle refresh candidate RID set drifted' -Expected @($expectedCandidates.Rid) -Actual @($candidateRows | ForEach-Object { [string]$_.rid })

    foreach ($expected in $expectedCandidates) {
        $matches = @($candidateRows | Where-Object { [string]$_.rid -ceq $expected.Rid })
        Assert-True -Condition ($matches.Count -eq 1) -Path $candidateRecord.RelativePath -Issue 'Lifecycle refresh candidate must appear exactly once' -Text $expected.Rid
        if ($matches.Count -ne 1) {
            continue
        }

        $row = $matches[0]
        Assert-ExactPropertySet -Value $row -Path $candidateRecord.RelativePath -Context "Lifecycle refresh row $($expected.Rid)" -Expected @(
            'rid',
            'platformFamily',
            'distro',
            'distroVersion',
            'runner',
            'opencvRid',
            'nativeExtension',
            'producer',
            'notes'
        )
        Assert-ExactPropertySet -Value $row.producer -Path $candidateRecord.RelativePath -Context "Lifecycle refresh producer $($expected.Rid)" -Expected @(
            'kind',
            'profiles',
            'containerImage',
            'openCvExtraCMakeArgs'
        )
        Assert-ExactPropertySet -Value $row.producer.openCvExtraCMakeArgs -Path $candidateRecord.RelativePath -Context "Lifecycle refresh CMake arguments $($expected.Rid)" -Expected @('full', 'mini')
        Assert-True -Condition (
            [string]$row.platformFamily -ceq 'linux' -and
            [string]$row.distro -ceq $expected.Distro -and
            [string]$row.distroVersion -ceq $expected.DistroVersion -and
            [string]$row.runner -ceq 'ubuntu-24.04' -and
            [string]$row.opencvRid -ceq $expected.Rid -and
            [string]$row.nativeExtension -ceq '.so' -and
            [string]$row.producer.kind -ceq 'container' -and
            [string]$row.producer.containerImage -ceq $expected.Image -and
            [string]$row.producer.openCvExtraCMakeArgs.full -ceq '' -and
            [string]$row.producer.openCvExtraCMakeArgs.mini -ceq '' -and
            [string]$row.notes -match 'cannot be published'
        ) -Path $candidateRecord.RelativePath -Issue 'Lifecycle refresh producer facts or non-publication wording drifted' -Text $expected.Rid
        Assert-ExactSet -Path $candidateRecord.RelativePath -Issue "Lifecycle refresh profile set drifted for $($expected.Rid)" -Expected @('full', 'mini') -Actual @($row.producer.profiles)

        $supportMatches = @($supportContract.lifecycleRefreshCandidates | Where-Object { [string]$_.rid -ceq $expected.Rid })
        Assert-True -Condition (
            $supportMatches.Count -eq 1 -and
            [string]$supportMatches[0].status -ceq 'lifecycle-refresh-pending' -and
            [string]$supportMatches[0].distro -ceq $expected.Distro -and
            [string]$supportMatches[0].distroVersion -ceq $expected.DistroVersion -and
            [string]$supportMatches[0].containerImage -ceq $expected.Image
        ) -Path $supportRecord.RelativePath -Issue 'Lifecycle refresh overlay must exactly match the pending support-contract candidate' -Text $expected.Rid
    }

    $candidateProfiles = @($candidateMatrix.profiles)
    $activeProfiles = @($activeMatrix.profiles)
    Assert-ExactSet -Path $candidateRecord.RelativePath -Issue 'Lifecycle refresh profile names must match the active matrix' -Expected @($activeProfiles | ForEach-Object { [string]$_.name }) -Actual @($candidateProfiles | ForEach-Object { [string]$_.name })
    foreach ($activeProfile in $activeProfiles) {
        $profileName = [string]$activeProfile.name
        $matches = @($candidateProfiles | Where-Object { [string]$_.name -ceq $profileName })
        Assert-True -Condition ($matches.Count -eq 1) -Path $candidateRecord.RelativePath -Issue 'Lifecycle refresh profile must appear exactly once' -Text $profileName
        if ($matches.Count -ne 1) {
            continue
        }

        $candidateProfile = $matches[0]
        Assert-ExactPropertySet -Value $candidateProfile -Path $candidateRecord.RelativePath -Context "Lifecycle refresh profile $profileName" -Expected @('name', 'packageIdSuffix', 'buildList', 'modules', 'optionalModules')
        Assert-True -Condition (
            [string]$candidateProfile.packageIdSuffix -ceq [string]$activeProfile.packageIdSuffix -and
            [string]$candidateProfile.buildList -ceq [string]$activeProfile.buildList -and
            (@($candidateProfile.modules) -join "`n") -ceq (@($activeProfile.modules) -join "`n") -and
            (@($candidateProfile.optionalModules) -join "`n") -ceq (@($activeProfile.optionalModules) -join "`n")
        ) -Path $candidateRecord.RelativePath -Issue 'Lifecycle refresh profile definition must exactly match the active build contract' -Text $profileName
    }

    $candidateRids = @($expectedCandidates.Rid)
    $activeRids = @($activeMatrix.rids | ForEach-Object { [string]$_.rid })
    Assert-True -Condition (@($activeRids | Where-Object { $candidateRids -contains $_ }).Count -eq 0) -Path $activeRecord.RelativePath -Issue 'Lifecycle refresh candidates must not enter the active package matrix'

    $candidateTargets = @(Get-ProfileTargets -Matrix $candidateMatrix)
    Assert-True -Condition ($candidateTargets.Count -eq 4 -and @($candidateTargets | Sort-Object -Unique).Count -eq 4) -Path $candidateRecord.RelativePath -Issue 'Lifecycle refresh matrix must describe four unique RID/profile validation targets'
    $activeSupportTargets = @(
        @($supportContract.packageSurface | ForEach-Object { [string]$_ })
        @($supportContract.realSupport | ForEach-Object { [string]$_ })
        @($supportContract.compatibilityOnly | ForEach-Object { [string]$_.target })
        @($supportContract.pending | ForEach-Object { [string]$_.target })
        @($supportContract.excluded | ForEach-Object { [string]$_.target })
    )
    Assert-True -Condition (@($candidateTargets | Where-Object { $activeSupportTargets -contains $_ }).Count -eq 0) -Path $supportRecord.RelativePath -Issue 'Lifecycle refresh validation targets must remain outside every active support/package partition'

    foreach ($workflowRelativePath in @('.github/workflows/pack.yml', '.github/workflows/publish-nuget.yml')) {
        $workflowPath = Join-Path $repo ($workflowRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
        $workflowText = [IO.File]::ReadAllText($workflowPath)
        Assert-True -Condition (-not $workflowText.Contains($candidateMatrixRelativePath, [StringComparison]::Ordinal)) -Path $workflowRelativePath -Issue 'Publication workflow must not consume the lifecycle refresh candidate matrix'
    }

    Write-Host "LIFECYCLE_REFRESH_RUNTIME_MATRIX_OK candidates=$($candidateRows.Count) targets=$($candidateTargets.Count) profiles=$($candidateProfiles.Count) publication_allowed=false"
}
catch {
    Add-Violation -Path $candidateMatrixRelativePath -Issue 'Lifecycle refresh runtime matrix execution failed' -Text $_.Exception.Message
}

if ($violations.Count -gt 0) {
    Write-Host "Lifecycle refresh runtime matrix failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Issue | Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host 'Lifecycle refresh runtime matrix passed.'
Write-Host 'Fedora 44 and Alpine 3.23 Full/Mini validation inputs are digest-pinned and profile-aligned while remaining outside active package, support, and publication surfaces.'
