param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidenceRel = 'packaging/performance/native-aot-smoke-evidence.json'
$schemaRel = 'packaging/performance/native-aot-smoke-evidence.schema.json'
$codecRel = 'packaging/performance/codec-writer-native-evidence.json'
$evidencePath = Join-Path $repo ($evidenceRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$codecPath = Join-Path $repo ($codecRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($evidencePath, $schemaPath, $codecPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "NativeAOT smoke evidence input is missing: $path" }
}
if (-not (Test-Json -LiteralPath $evidencePath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'NativeAOT smoke evidence failed JSON Schema validation.' }
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
$codec = Get-Content -LiteralPath $codecPath -Raw | ConvertFrom-Json

if ([string]$evidence.status -cne 'verified' -or [string]$evidence.sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'NativeAOT smoke evidence identity is invalid.' }
$commitType = ((& git -C $repo cat-file -t ([string]$evidence.sourceCommit) 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $commitType -cne 'commit') { throw 'NativeAOT smoke source commit is not present in the repository.' }
& git -C $repo merge-base --is-ancestor ([string]$evidence.sourceCommit) HEAD 2>$null
if ($LASTEXITCODE -ne 0) { throw 'NativeAOT smoke source commit is not an ancestor of HEAD.' }
if ([string]$evidence.runner.architecture -cne 'x64' -or [string]$evidence.runner.configuration -cne 'Release' -or [string]$evidence.runner.dotnet -notmatch '^10\.0\.\d+$') { throw 'NativeAOT smoke runner identity drifted.' }
if ([string]$evidence.runtimeIdentifier -cne 'win-x64' -or [string]$evidence.targetFramework -cne 'net10.0' -or [string]$evidence.sdk -notmatch '^10\.0\.\d+$') { throw 'NativeAOT smoke target identity drifted.' }
if (-not [bool]$evidence.publishAot -or -not [bool]$evidence.publishTrimmed -or [string]$evidence.trimMode -cne 'partial' -or -not [bool]$evidence.publishSingleFile) { throw 'NativeAOT publish settings drifted.' }
if ([string]$evidence.nativeRuntimeArtifact -notmatch '^local-package-cache:jyppx\.opencv\.runtime\.win-x64\.5\.0\.0\.nupkg$' -or
    [string]$evidence.nativeRuntimePackage -cne 'jyppx.opencv.runtime.win-x64.5.0.0.nupkg' -or
    [string]$evidence.nativeRuntimePackageSha256 -cne '07de65af0ec14420d84d49caa4663d08393880a1bae402c789177d2dfff2544e' -or
    [string]$evidence.nativeRuntimeArtifactSha256 -cne [string]$evidence.nativeRuntimePackageSha256) { throw 'NativeAOT runtime package identity drifted.' }
if ([int]$evidence.publishedFileCount -ne 21 -or [int]$evidence.nativePayloadFileCount -ne 18 -or [string]$evidence.nativeSmoke -cne 'verified' -or [int]$evidence.encodedBytes -ne 71 -or -not [bool]$evidence.dnnCpuAvailable -or [int]$evidence.dnnTargetCount -lt 1) { throw 'NativeAOT native smoke result drifted.' }
if ([string]$evidence.managedAssemblySha256 -notmatch '^[0-9a-f]{64}$') { throw 'NativeAOT managed assembly hash is malformed.' }

$codecPackage = @($codec.runtimePackages | Where-Object { [string]$_.profile -ceq 'full' })
if ($codecPackage.Count -ne 1 -or [string]$codecPackage[0].package.packageSha256 -cne [string]$evidence.nativeRuntimePackageSha256) { throw 'NativeAOT package hash is not bound to the full runtime evidence.' }
$expectedPayload = @($codecPackage[0].package.payload | Sort-Object name)
$actualPayload = @($evidence.nativePayload | Sort-Object name)
if ($actualPayload.Count -ne 18) { throw 'NativeAOT native payload count drifted.' }
for ($index = 0; $index -lt $expectedPayload.Count; $index++) {
    $expected = $expectedPayload[$index]
    $actual = $actualPayload[$index]
    if ([string]$actual.name -cne [string]$expected.name -or [int64]$actual.bytes -ne [int64]$expected.bytes -or [string]$actual.sha256 -cne [string]$expected.sha256) {
        throw "NativeAOT native payload drifted at $($expected.name)."
    }
}
Write-Host "NATIVE_AOT_SMOKE_EVIDENCE_OK source_commit=$($evidence.sourceCommit) package_sha256=$($evidence.nativeRuntimePackageSha256) files=$($evidence.publishedFileCount) native_payload=$($evidence.nativePayloadFileCount) encoded_bytes=$($evidence.encodedBytes) dnn_targets=$($evidence.dnnTargetCount)"
