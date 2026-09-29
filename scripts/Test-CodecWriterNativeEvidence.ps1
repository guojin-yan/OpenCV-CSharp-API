param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidenceRel = 'packaging/performance/codec-writer-native-evidence.json'
$schemaRel = 'packaging/performance/codec-writer-native-evidence.schema.json'
$evidencePath = Join-Path $repo ($evidenceRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($evidencePath, $schemaPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Codec writer native evidence file is missing: $path" } }
if (-not (Test-Json -LiteralPath $evidencePath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Codec writer native evidence failed JSON Schema validation.' }
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json

if ([string]$evidence.status -cne 'verified' -or [string]$evidence.sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Codec writer native evidence identity is invalid.' }
$commitType = ((& git -C $repo cat-file -t ([string]$evidence.sourceCommit) 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $commitType -cne 'commit') { throw 'Codec writer native source commit is not present in the repository.' }
& git -C $repo merge-base --is-ancestor ([string]$evidence.sourceCommit) HEAD 2>$null
if ($LASTEXITCODE -ne 0) { throw 'Codec writer native source commit is not an ancestor of HEAD.' }
if ([string]$evidence.runner.architecture -cne 'x64' -or [string]$evidence.runner.configuration -cne 'Release' -or [string]$evidence.runner.dotnetSdk -notmatch '^10\.0\.') { throw 'Codec writer native runner identity drifted.' }
if ([string]$evidence.test.class -cne 'JYPPX.OpenCvSharp.Tests.ImgCodecs.Cv2InteropTests' -or [bool]$evidence.test.nativeSmokeEnabled -ne $true) { throw 'Codec writer native test identity drifted.' }

function Assert-Names {
    param([Parameter(Mandatory)][string]$Name, [Parameter(Mandatory)][object[]]$Actual, [Parameter(Mandatory)][string[]]$Expected)
    $actualNames = @($Actual | ForEach-Object { [string]$_ } | Sort-Object)
    $expectedNames = @($Expected | Sort-Object)
    if (($actualNames -join '|') -cne ($expectedNames -join '|')) { throw "$Name drifted: expected '$($expectedNames -join ',')', found '$($actualNames -join ',')'." }
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
$packageRows = @($evidence.runtimePackages)
if ($packageRows.Count -ne 2) { throw 'Codec writer native evidence must contain exactly full and mini package rows.' }
foreach ($profile in @('full', 'mini')) {
    $rows = @($packageRows | Where-Object { [string]$_.profile -ceq $profile })
    if ($rows.Count -ne 1) { throw "Codec writer native package row is not unique for $profile." }
    $package = $rows[0].package
    $expectedName = if ($profile -ceq 'full') { 'jyppx.opencv.runtime.win-x64.5.0.0.nupkg' } else { 'jyppx.opencv.runtime.win-x64.mini.5.0.0.nupkg' }
    $expectedNames = if ($profile -ceq 'full') { $fullExpectedNames } else { $miniExpectedNames }
    if ([string]$package.packageFileName -cne $expectedName -or [string]$package.packageSha256 -cne $expectedPackageHashes[$profile]) { throw "Codec writer package identity drifted for $profile." }
    if (@($package.payload).Count -ne $expectedNames.Count) { throw "Codec writer package payload count drifted for $profile." }
    Assert-Names -Name "$profile package payload" -Actual @($package.payload | ForEach-Object { $_.name }) -Expected $expectedNames
    if (@($package.payload | Group-Object name | Where-Object Count -ne 1).Count -ne 0) { throw "Codec writer package payload has duplicate names for $profile." }
    foreach ($row in @($package.payload)) { if ([int64]$row.bytes -le 0 -or [string]$row.sha256 -notmatch '^[0-9a-f]{64}$') { throw "Codec writer package payload row is invalid for $profile/$($row.name)." } }
}

$testRows = @($evidence.test.targetFrameworks)
if ($testRows.Count -ne 4) { throw "Expected four codec writer native test rows; found $($testRows.Count)." }
$expectedKeys = @('full/net8.0', 'full/net10.0', 'mini/net8.0', 'mini/net10.0')
$actualKeys = @($testRows | ForEach-Object { "$($_.profile)/$($_.targetFramework)" } | Sort-Object)
if (($actualKeys -join '|') -cne (@($expectedKeys | Sort-Object) -join '|')) { throw "Codec writer native test matrix drifted: $($actualKeys -join ',')." }
foreach ($row in $testRows) {
    foreach ($field in @('total', 'executed', 'passed')) { if ([int]$row.$field -ne 8) { throw "Codec writer pass count drifted for $($row.profile)/$($row.targetFramework): $field=$($row.$field)." } }
    foreach ($field in @('failed', 'skipped', 'error', 'timeout', 'aborted')) { if ([int]$row.$field -ne 0) { throw "Codec writer negative count for $($row.profile)/$($row.targetFramework): $field=$($row.$field)." } }
    if ([bool]$row.nativeSmokeEnabled -ne $true) { throw "Native smoke was not enabled for $($row.profile)/$($row.targetFramework)." }
    foreach ($field in @('testAssemblySha256', 'apiAssemblySha256')) { if ([string]$row.$field -notmatch '^[0-9a-f]{64}$') { throw "Codec writer assembly hash is malformed for $($row.profile)/$($row.targetFramework)." } }
}
if (@($evidence.test.verifiedCases).Count -lt 8) { throw 'Codec writer verified case inventory is incomplete.' }
Write-Host "CODEC_WRITER_NATIVE_EVIDENCE_OK source_commit=$($evidence.sourceCommit) packages=2 test_rows=4 passed=32"
