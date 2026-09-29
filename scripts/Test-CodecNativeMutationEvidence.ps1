param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidenceRel = 'packaging/codec/codec-native-mutation-evidence.json'
$schemaRel = 'packaging/codec/codec-native-mutation-evidence.schema.json'
$corpusRel = 'packaging/codec/image-preflight-mutation-corpus.json'
$evidencePath = Join-Path $repo ($evidenceRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$corpusPath = Join-Path $repo ($corpusRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($evidencePath,$schemaPath,$corpusPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Native codec mutation evidence input is missing: $path" } }
if (-not (Test-Json -LiteralPath $evidencePath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Native codec mutation evidence failed JSON Schema validation.' }
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
$corpus = Get-Content -LiteralPath $corpusPath -Raw | ConvertFrom-Json
if ([string]$evidence.status -cne 'verified' -or [string]$evidence.sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Native codec mutation evidence identity is invalid.' }
$commitType = ((& git -C $repo cat-file -t ([string]$evidence.sourceCommit) 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $commitType -cne 'commit') { throw 'Native codec mutation source commit is not present in the repository.' }
& git -C $repo merge-base --is-ancestor ([string]$evidence.sourceCommit) HEAD 2>$null
if ($LASTEXITCODE -ne 0) { throw 'Native codec mutation source commit is not an ancestor of HEAD.' }
$corpusHash = (Get-FileHash -LiteralPath $corpusPath -Algorithm SHA256).Hash.ToLowerInvariant()
if ([string]$evidence.corpusManifestSha256 -cne $corpusHash) { throw 'Native codec mutation corpus hash drifted.' }
if ([string]$evidence.runner.architecture -cne 'x64' -or [string]$evidence.runner.configuration -cne 'Release' -or [string]$evidence.runner.dotnet -notmatch '^10\.0\.\d+$') { throw 'Native codec mutation runner identity drifted.' }
if ([bool]$evidence.nativeRuntimeRequired -ne $true) { throw 'Native codec mutation evidence must require native runtime payloads.' }

$fullNames = @('JYPPX.OpenCV.Native.dll','opencv_calib500.dll','opencv_core500.dll','opencv_dnn500.dll','opencv_features500.dll','opencv_flann500.dll','opencv_geometry500.dll','opencv_highgui500.dll','opencv_imgcodecs500.dll','opencv_imgproc500.dll','opencv_ml500.dll','opencv_objdetect500.dll','opencv_photo500.dll','opencv_ptcloud500.dll','opencv_stereo500.dll','opencv_stitching500.dll','opencv_video500.dll','opencv_videoio500.dll')
$miniNames = @('JYPPX.OpenCV.Native.dll','opencv_core500.dll','opencv_flann500.dll','opencv_geometry500.dll','opencv_imgcodecs500.dll','opencv_imgproc500.dll','opencv_videoio500.dll')
$expectedHashes = @{ full = '07de65af0ec14420d84d49caa4663d08393880a1bae402c789177d2dfff2544e'; mini = 'c4712ed62f34f31eacaaa4c51f84ee6003bf5bab3cfcca850d39c3b812628918' }
$packages = @($evidence.runtimePackages)
if ($packages.Count -ne 2) { throw 'Native codec mutation evidence must bind full and mini packages.' }
foreach ($profile in @('full','mini')) {
    $rows = @($packages | Where-Object { [string]$_.profile -ceq $profile })
    if ($rows.Count -ne 1) { throw "Native codec mutation package identity is not unique for $profile." }
    $package = $rows[0]
    $expectedFile = if ($profile -ceq 'full') { 'jyppx.opencv.runtime.win-x64.5.0.0.nupkg' } else { 'jyppx.opencv.runtime.win-x64.mini.5.0.0.nupkg' }
    $expectedNames = if ($profile -ceq 'full') { $fullNames } else { $miniNames }
    if ([string]$package.packageFileName -cne $expectedFile -or [string]$package.packageSha256 -cne $expectedHashes[$profile]) { throw "Native codec mutation package hash/name drifted for $profile." }
    $actualNames = @($package.payload | ForEach-Object { [string]$_.name } | Sort-Object)
    if (($actualNames -join '|') -cne (@($expectedNames | Sort-Object) -join '|')) { throw "Native codec mutation payload inventory drifted for $profile." }
    foreach ($row in @($package.payload)) { if ([int64]$row.bytes -le 0 -or [string]$row.sha256 -notmatch '^[0-9a-f]{64}$') { throw "Native codec mutation payload row is invalid for $profile/$($row.name)." } }
}

$frameworks = @($evidence.frameworks)
$expectedKeys = @('full/net8.0','full/net10.0','mini/net8.0','mini/net10.0')
$actualKeys = @($frameworks | ForEach-Object { "$($_.profile)/$($_.targetFramework)" } | Sort-Object)
if (($actualKeys -join '|') -cne (@($expectedKeys | Sort-Object) -join '|')) { throw 'Native codec mutation profile/framework matrix drifted.' }
foreach ($row in $frameworks) {
    foreach ($field in @('status','failureClass')) { if ([string]$row.$field -cne 'clean') { throw "Native codec mutation was not clean for $($row.profile)/$($row.targetFramework)." } }
    if ([int]$row.exitCode -ne 0 -or -not [bool]$row.passed -or -not [bool]$row.nativeSmokeEnabled -or [bool]$row.resourceLeakObserved) { throw "Native codec mutation pass/runtime/resource state drifted for $($row.profile)/$($row.targetFramework)." }
    if ([int]$row.fixtureCount -ne @($corpus.fixtures).Count) { throw "Native codec mutation fixture count drifted for $($row.profile)/$($row.targetFramework)." }
    if ([long]$row.processTreeWorkingSetGrowthBytes -gt [long]$row.maxProcessTreeWorkingSetGrowthBytes) { throw "Native codec mutation process-tree RSS growth exceeded its bound for $($row.profile)/$($row.targetFramework)." }
    foreach ($field in @('oomCount','crashCount','hangCount')) { if ([int]$row.$field -ne 0) { throw "Native codec mutation failure classification is nonzero for $($row.profile)/$($row.targetFramework)/$field." } }
}
foreach ($field in @('oom','crash','hang','managedTestFailure','resourceLeak')) { if ([int]$evidence.classifications.$field -ne 0) { throw "Native codec mutation classification is nonzero: $field." } }
if ([int]$evidence.classifications.clean -ne 4) { throw 'Native codec mutation clean row count drifted.' }
Write-Host "CODEC_NATIVE_MUTATION_EVIDENCE_OK source_commit=$($evidence.sourceCommit) profiles=full,mini frameworks=net8.0,net10.0 clean=4 oom=0 crash=0 hang=0 rss_limit=$($evidence.maxWorkingSetGrowthBytes)"
