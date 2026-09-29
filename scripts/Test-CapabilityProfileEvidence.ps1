param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$EvidencePath = 'packaging/runtime/capability-profile-evidence.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidenceFullPath = if ([IO.Path]::IsPathRooted($EvidencePath)) {
    [IO.Path]::GetFullPath($EvidencePath)
}
else {
    [IO.Path]::GetFullPath((Join-Path $repo ($EvidencePath -replace '/', [IO.Path]::DirectorySeparatorChar)))
}
$schemaPath = Join-Path $repo 'packaging/runtime/capability-profile-evidence.schema.json'
if (-not (Test-Path -LiteralPath $evidenceFullPath -PathType Leaf)) { throw "Capability profile evidence is missing: $evidenceFullPath" }
if (-not (Test-Path -LiteralPath $schemaPath -PathType Leaf)) { throw "Capability profile evidence schema is missing: $schemaPath" }

$json = Get-Content -LiteralPath $evidenceFullPath -Raw
if (-not (Test-Json -Json $json -SchemaFile $schemaPath -ErrorAction Stop)) {
    throw 'Capability profile evidence failed JSON Schema validation.'
}
$evidence = $json | ConvertFrom-Json

$head = ([string](& git -C $repo rev-parse HEAD)).Trim().ToLowerInvariant()
$sourceCommit = ([string]$evidence.sourceCommit).Trim().ToLowerInvariant()
if ($sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Capability profile evidence sourceCommit is invalid.' }
& git -C $repo merge-base --is-ancestor $sourceCommit $head
if ($LASTEXITCODE -ne 0) { throw "Capability profile evidence sourceCommit is not an ancestor of HEAD: $sourceCommit" }

if ([string]$evidence.status -cne 'verified') { throw 'Capability profile evidence status must be verified.' }
if ([string]$evidence.runner.processArchitecture -cne 'X64' -or [string]$evidence.runner.configuration -cne 'Release') {
    throw 'Capability profile evidence runner must be Windows x64 Release.'
}
if ([string]$evidence.runner.dotnetSdk -notmatch '^10\.0\.') { throw 'Capability profile evidence must use the .NET 10 SDK.' }
foreach ($hash in @([string]$evidence.runner.managedAssemblySha256, [string]$evidence.runner.sampleAssemblySha256)) {
    if ($hash -notmatch '^[0-9a-f]{64}$') { throw 'Capability profile managed assembly hash is invalid.' }
}
if ([string]$evidence.boundary -notmatch 'Declared-only' -or [string]$evidence.boundary -notmatch 'Unknown') {
    throw 'Capability profile boundary must preserve optional-module and accelerator limitations.'
}

function Assert-ArrayEqual {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][object[]]$Actual,
        [Parameter(Mandatory)][string[]]$Expected
    )

    $actualStrings = @($Actual | ForEach-Object { [string]$_ })
    if (($actualStrings -join '|') -cne ($Expected -join '|')) {
        throw "$Name drifted: expected '$($Expected -join ',')', found '$($actualStrings -join ',')'."
    }
}

function Get-UniqueRows {
    param([Parameter(Mandatory)][object[]]$Rows)

    return @($Rows | Group-Object name | Where-Object Count -ne 1)
}

$fullExpectedNames = @(
    'JYPPX.OpenCV.Native.dll', 'opencv_calib500.dll', 'opencv_core500.dll', 'opencv_dnn500.dll',
    'opencv_features500.dll', 'opencv_flann500.dll', 'opencv_geometry500.dll', 'opencv_highgui500.dll',
    'opencv_imgcodecs500.dll', 'opencv_imgproc500.dll', 'opencv_ml500.dll', 'opencv_objdetect500.dll',
    'opencv_photo500.dll', 'opencv_ptcloud500.dll', 'opencv_stereo500.dll', 'opencv_stitching500.dll',
    'opencv_video500.dll', 'opencv_videoio500.dll'
)
$miniExpectedNames = @(
    'JYPPX.OpenCV.Native.dll', 'opencv_core500.dll', 'opencv_flann500.dll', 'opencv_geometry500.dll',
    'opencv_imgcodecs500.dll', 'opencv_imgproc500.dll', 'opencv_videoio500.dll'
)
$expectedPackageHashes = @{
    full = '07de65af0ec14420d84d49caa4663d08393880a1bae402c789177d2dfff2544e'
    mini = 'c4712ed62f34f31eacaaa4c51f84ee6003bf5bab3cfcca850d39c3b812628918'
}

$runtimePackages = @($evidence.runtimePackages)
if ($runtimePackages.Count -ne 2) { throw "Expected exactly two capability runtime packages; found $($runtimePackages.Count)." }
foreach ($profileName in @('full', 'mini')) {
    $packageRows = @($runtimePackages | Where-Object { [string]$_.profile -ceq $profileName })
    if ($packageRows.Count -ne 1) { throw "Missing unique runtime package evidence for profile $profileName." }
    $package = $packageRows[0].package
    $expectedPackageName = if ($profileName -ceq 'full') { 'jyppx.opencv.runtime.win-x64.5.0.0.nupkg' } else { 'jyppx.opencv.runtime.win-x64.mini.5.0.0.nupkg' }
    if ([string]$package.packageFileName -cne $expectedPackageName) { throw "Unexpected package file for $profileName." }
    if ([string]$package.packageSha256 -cne $expectedPackageHashes[$profileName]) { throw "Unexpected package hash for $profileName." }
    $expectedNames = if ($profileName -ceq 'full') { $fullExpectedNames } else { $miniExpectedNames }
    $payload = @($package.payload)
    if ($payload.Count -ne $expectedNames.Count) { throw "Unexpected native payload count for $profileName." }
    if (@(Get-UniqueRows -Rows $payload).Count -ne 0) { throw "Duplicate native payload names for $profileName." }
    Assert-ArrayEqual -Name "$profileName native payload" -Actual @($payload | ForEach-Object { [string]$_.name } | Sort-Object) -Expected @($expectedNames | Sort-Object)
    foreach ($row in $payload) {
        if ([string]$row.sha256 -notmatch '^[0-9a-f]{64}$' -or [int64]$row.bytes -le 0) { throw "Invalid native payload row for ${profileName}: $($row.name)." }
    }
}

$profiles = @($evidence.profiles)
if ($profiles.Count -ne 3) { throw "Expected exactly three capability profiles; found $($profiles.Count)." }
if ((@($profiles | Group-Object name | Where-Object Count -ne 1)).Count -ne 0) { throw 'Capability profile names must be unique.' }
Assert-ArrayEqual -Name 'capability profile set' -Actual @($profiles | ForEach-Object { [string]$_.name }) -Expected @('none', 'full', 'mini')

$none = @($profiles | Where-Object { [string]$_.name -ceq 'none' })[0]
$full = @($profiles | Where-Object { [string]$_.name -ceq 'full' })[0]
$mini = @($profiles | Where-Object { [string]$_.name -ceq 'mini' })[0]
$unknownModules = @('Unknown', 'Unknown', 'Unknown', 'Unknown')
$verifiedModules = @('Verified', 'Verified', 'Verified', 'Verified')
$unavailableDnn = @('Unavailable', 'Unavailable', 'Unavailable', 'Unavailable', 'Unavailable', 'Unavailable', 'Unavailable', 'Unavailable')
$fullDnn = @('Verified', 'Unavailable', 'Verified', 'Unavailable', 'Unavailable', 'Unavailable', 'Unavailable', 'Unavailable')
$codecWriters = @('Verified', 'Verified', 'Verified', 'Verified', 'Verified', 'Verified', 'Unavailable', 'Verified')

if ([string]$none.nativeRuntimeState -cne 'Unknown' -or [string]$none.guiBackendState -cne 'Unavailable' -or [int]$none.warningsCount -le 0) { throw 'No-runtime profile does not prove the negative capability path.' }
Assert-ArrayEqual -Name 'none required modules' -Actual $none.requiredModuleStates -Expected $unknownModules
Assert-ArrayEqual -Name 'none dnn backends' -Actual $none.dnnBackendStates -Expected $unavailableDnn
Assert-ArrayEqual -Name 'none codec writers' -Actual $none.codecWriterStates -Expected (@('Unavailable') * 8)

if ([string]$full.nativeRuntimeState -cne 'Verified' -or [string]$full.guiBackendState -notin @('Available', 'Verified') -or [int]$full.warningsCount -ne 0) { throw 'Full profile does not prove the positive capability path.' }
Assert-ArrayEqual -Name 'full required modules' -Actual $full.requiredModuleStates -Expected $verifiedModules
Assert-ArrayEqual -Name 'full dnn backends' -Actual $full.dnnBackendStates -Expected $fullDnn
Assert-ArrayEqual -Name 'full codec writers' -Actual $full.codecWriterStates -Expected $codecWriters

if ([string]$mini.nativeRuntimeState -cne 'Verified' -or [string]$mini.guiBackendState -cne 'Unavailable' -or [int]$mini.warningsCount -le 0) { throw 'Mini profile does not prove the reduced-profile boundary.' }
Assert-ArrayEqual -Name 'mini required modules' -Actual $mini.requiredModuleStates -Expected $verifiedModules
Assert-ArrayEqual -Name 'mini dnn backends' -Actual $mini.dnnBackendStates -Expected $unavailableDnn
Assert-ArrayEqual -Name 'mini codec writers' -Actual $mini.codecWriterStates -Expected $codecWriters
foreach ($profile in @($none, $full, $mini)) {
    if ([int]$profile.optionalModuleCount -ne 25) { throw "Optional module declaration count drifted for $($profile.name)." }
    if ([string]$profile.snapshotSha256 -notmatch '^[0-9a-f]{64}$') { throw "Snapshot hash is invalid for $($profile.name)." }
}

Write-Host "CAPABILITY_PROFILE_EVIDENCE_OK source_commit=$sourceCommit packages=full,mini profiles=none,full,mini full_dlls=$($fullExpectedNames.Count) mini_dlls=$($miniExpectedNames.Count)"
