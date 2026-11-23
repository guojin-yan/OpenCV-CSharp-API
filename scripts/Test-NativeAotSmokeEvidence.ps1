param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidenceRel = 'packaging/performance/native-aot-smoke-evidence.json'
$schemaRel = 'packaging/performance/native-aot-smoke-evidence.schema.json'
$packageEvidenceRel = 'packaging/runtime/opencv-5.1.0-package-consumer-evidence.json'
$evidencePath = Join-Path $repo ($evidenceRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$packageEvidencePath = Join-Path $repo ($packageEvidenceRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($evidencePath, $schemaPath, $packageEvidencePath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "NativeAOT smoke evidence input is missing: $path" }
}
if (-not (Test-Json -LiteralPath $evidencePath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'NativeAOT smoke evidence failed JSON Schema validation.' }
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
$packageEvidence = Get-Content -LiteralPath $packageEvidencePath -Raw | ConvertFrom-Json

if ([string]$evidence.status -cne 'verified' -or [string]$evidence.sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'NativeAOT smoke evidence identity is invalid.' }
$commitType = ((& git -C $repo cat-file -t ([string]$evidence.sourceCommit) 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $commitType -cne 'commit') { throw 'NativeAOT smoke source commit is not present in the repository.' }
& git -C $repo merge-base --is-ancestor ([string]$evidence.sourceCommit) HEAD 2>$null
if ($LASTEXITCODE -ne 0) { throw 'NativeAOT smoke source commit is not an ancestor of HEAD.' }
if ([string]$evidence.runner.architecture -cne 'x64' -or [string]$evidence.runner.configuration -cne 'Release' -or [string]$evidence.runner.dotnet -notmatch '^10\.0\.\d+$') { throw 'NativeAOT smoke runner identity drifted.' }
if ([string]$evidence.runtimeIdentifier -cne 'win-x64' -or [string]$evidence.targetFramework -cne 'net10.0' -or [string]$evidence.sdk -notmatch '^10\.0\.\d+$') { throw 'NativeAOT smoke target identity drifted.' }
if (-not [bool]$evidence.publishAot -or -not [bool]$evidence.publishTrimmed -or [string]$evidence.trimMode -cne 'partial' -or -not [bool]$evidence.publishSingleFile) { throw 'NativeAOT publish settings drifted.' }
if ([string]$packageEvidence.status -cne 'verified-local-windows-x64' -or
    [string]$packageEvidence.recordKind -cne 'opencv-5.1.0-package-consumer-evidence' -or
    [string]$packageEvidence.openCvVersion -cne '5.1.0' -or [string]$packageEvidence.packageVersion -cne '5.1.0.0' -or
    [string]$packageEvidence.rid -cne 'win-x64') { throw 'NativeAOT smoke package-consumer evidence identity drifted.' }
foreach ($revision in @([string]$packageEvidence.sourceCommit, [string]$packageEvidence.nativeBuildSourceCommit)) {
    if ($revision -notmatch '^[0-9a-f]{40}$') { throw 'NativeAOT package-consumer evidence source identity is malformed.' }
    & git -C $repo merge-base --is-ancestor $revision ([string]$evidence.sourceCommit) 2>$null
    if ($LASTEXITCODE -ne 0) { throw "NativeAOT package-consumer evidence is newer than the measured smoke source: $revision" }
}
$managedPackage = @($packageEvidence.packages | Where-Object { [string]$_.label -ceq 'managed' })
$fullPackage = @($packageEvidence.packages | Where-Object { [string]$_.label -ceq 'full' })
if ($managedPackage.Count -ne 1 -or [string]$managedPackage[0].packageId -cne 'JYPPX.OpenCV.CSharp.API' -or
    [string]$managedPackage[0].nugetVersion -cne [string]$evidence.managedPackageVersion -or
    $fullPackage.Count -ne 1 -or [string]$fullPackage[0].packageId -cne 'JYPPX.OpenCV.runtime.win-x64' -or
    [string]$fullPackage[0].nugetVersion -cne '5.1.0' -or
    [string]$evidence.nativeRuntimeArtifact -cne "package-consumer-evidence:$packageEvidenceRel" -or
    [string]$evidence.nativeRuntimePackage -cne 'jyppx.opencv.runtime.win-x64.5.1.0.nupkg' -or
    [string]$fullPackage[0].sha256 -cne [string]$evidence.nativeRuntimePackageSha256 -or
    [string]$evidence.nativeRuntimeArtifactSha256 -cne [string]$evidence.nativeRuntimePackageSha256) { throw 'NativeAOT runtime package identity drifted.' }
if ([int]$evidence.nativeAbiVersion -ne 1 -or [int]$evidence.publishedFileCount -lt 1 -or
    [int]$evidence.nativePayloadFileCount -ne [int]$fullPackage[0].nativeFiles -or
    [string]$evidence.nativeSmoke -cne 'verified' -or [int]$evidence.encodedBytes -ne 71 -or
    -not [bool]$evidence.dnnCpuAvailable -or [int]$evidence.dnnTargetCount -lt 1) { throw 'NativeAOT native smoke result drifted.' }
if ([string]$evidence.managedAssemblySha256 -notmatch '^[0-9a-f]{64}$') { throw 'NativeAOT managed assembly hash is malformed.' }
$toolchain = $evidence.nativeAotToolchain
if ([string]$toolchain.runtimeFrameworkVersion -notmatch '^10\.0\.\d+$' -or
    [version]::Parse([string]$toolchain.runtimeFrameworkVersion) -lt [version]'10.0.14' -or
    [string]$toolchain.ilCompilerVersion -cne [string]$toolchain.runtimeFrameworkVersion -or
    [string]$toolchain.ilLinkTasksVersion -cne [string]$toolchain.runtimeFrameworkVersion -or
    -not [bool]$toolchain.nugetAuditEnabled -or -not [bool]$toolchain.nu1903TreatedAsError) {
    throw 'NativeAOT toolchain security policy drifted.'
}
$expectedPayload = @($fullPackage[0].nativeFilesSha256 | Sort-Object { [IO.Path]::GetFileName([string]$_.path) } | ForEach-Object {
        [pscustomobject]@{ name = [IO.Path]::GetFileName([string]$_.path); bytes = [int64]$_.length; sha256 = [string]$_.sha256 }
    })
$actualPayload = @($evidence.nativePayload | Sort-Object name)
if ($actualPayload.Count -ne [int]$fullPackage[0].nativeFiles -or $expectedPayload.Count -ne $actualPayload.Count) { throw 'NativeAOT native payload count drifted.' }
for ($index = 0; $index -lt $expectedPayload.Count; $index++) {
    $expected = $expectedPayload[$index]
    $actual = $actualPayload[$index]
    if ([string]$actual.name -cne [string]$expected.name -or [int64]$actual.bytes -ne [int64]$expected.bytes -or [string]$actual.sha256 -cne [string]$expected.sha256) {
        throw "NativeAOT native payload drifted at $($expected.name)."
    }
}
Write-Host "NATIVE_AOT_SMOKE_EVIDENCE_OK source_commit=$($evidence.sourceCommit) package_sha256=$($evidence.nativeRuntimePackageSha256) files=$($evidence.publishedFileCount) native_payload=$($evidence.nativePayloadFileCount) encoded_bytes=$($evidence.encodedBytes) dnn_targets=$($evidence.dnnTargetCount)"
