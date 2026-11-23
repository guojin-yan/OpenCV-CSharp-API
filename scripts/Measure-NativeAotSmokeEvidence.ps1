[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$RuntimeIdentifier = 'win-x64',
    [Parameter(Mandatory)][string]$OpenCvNativeRuntimeDir,
    [string]$PackageConsumerEvidencePath = 'packaging/runtime/opencv-5.1.0-package-consumer-evidence.json',
    [string]$OutputPath = 'packaging/performance/native-aot-smoke-evidence.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
if (-not [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::Windows) -or
    [Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [Runtime.InteropServices.Architecture]::X64) {
    throw 'NativeAOT smoke evidence requires a Windows x64 runner.'
}
if ($RuntimeIdentifier -cne 'win-x64') { throw 'Only win-x64 native AOT smoke evidence is currently supported.' }

$status = @(& git -C $repo status --porcelain 2>$null)
if ($LASTEXITCODE -ne 0 -or $status.Count -ne 0) { throw 'NativeAOT smoke measurement requires a clean repository.' }
$sourceCommit = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim().ToLowerInvariant()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Unable to resolve the source commit.' }

function Get-Sha256File {
    param([Parameter(Mandatory)][string]$Path)
    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Resolve-OutputPath {
    param([Parameter(Mandatory)][string]$Path)
    if ([IO.Path]::IsPathRooted($Path)) { return [IO.Path]::GetFullPath($Path) }
    return [IO.Path]::GetFullPath((Join-Path $repo ($Path -replace '/', [IO.Path]::DirectorySeparatorChar)))
}

$runtimeCandidate = if ([IO.Path]::IsPathRooted($OpenCvNativeRuntimeDir)) {
    $OpenCvNativeRuntimeDir
}
else {
    Join-Path $repo ($OpenCvNativeRuntimeDir -replace '/', [IO.Path]::DirectorySeparatorChar)
}
$runtime = (Resolve-Path -LiteralPath $runtimeCandidate).Path
$packageEvidencePath = Resolve-OutputPath -Path $PackageConsumerEvidencePath
if (-not (Test-Path -LiteralPath $packageEvidencePath -PathType Leaf)) { throw "5.1 package consumer evidence was not found: $packageEvidencePath" }
$packageEvidenceRelativePath = [IO.Path]::GetRelativePath($repo, $packageEvidencePath) -replace '\\', '/'
if ($packageEvidenceRelativePath.StartsWith('../', [StringComparison]::Ordinal) -or [IO.Path]::IsPathRooted($packageEvidenceRelativePath)) {
    throw 'Package consumer evidence must be inside the repository.'
}
$packageEvidence = Get-Content -LiteralPath $packageEvidencePath -Raw | ConvertFrom-Json
if ([string]$packageEvidence.status -cne 'verified-local-windows-x64' -or
    [string]$packageEvidence.recordKind -cne 'opencv-5.1.0-package-consumer-evidence' -or
    [string]$packageEvidence.openCvVersion -cne '5.1.0' -or
    [string]$packageEvidence.packageVersion -cne '5.1.0.0' -or
    [string]$packageEvidence.rid -cne 'win-x64' -or [string]$packageEvidence.configuration -cne 'Release') {
    throw 'NativeAOT smoke requires verified OpenCV 5.1.0 Windows x64 package-consumer evidence.'
}
$fullPackage = @($packageEvidence.packages | Where-Object { [string]$_.label -ceq 'full' })
$managedPackage = @($packageEvidence.packages | Where-Object { [string]$_.label -ceq 'managed' })
if ($managedPackage.Count -ne 1 -or [string]$managedPackage[0].packageId -cne 'JYPPX.OpenCV.CSharp.API' -or
    [string]$managedPackage[0].packageVersion -cne '5.1.0.0' -or [string]$managedPackage[0].nugetVersion -cne '5.1.0' -or
    [string]$managedPackage[0].sha256 -notmatch '^[0-9a-f]{64}$') {
    throw 'OpenCV 5.1.0 managed package evidence is incomplete.'
}
if ($fullPackage.Count -ne 1 -or [string]$fullPackage[0].packageId -cne 'JYPPX.OpenCV.runtime.win-x64' -or
    [string]$fullPackage[0].packageVersion -cne '5.1.0.0' -or [string]$fullPackage[0].nugetVersion -cne '5.1.0' -or
    [string]$fullPackage[0].sha256 -notmatch '^[0-9a-f]{64}$' -or
    [int]$fullPackage[0].nativeFiles -ne @($fullPackage[0].nativeFilesSha256).Count -or
    [int]$fullPackage[0].nativeFiles -le 0) {
    throw 'OpenCV 5.1.0 Full runtime package evidence is incomplete.'
}
foreach ($revision in @([string]$packageEvidence.sourceCommit, [string]$packageEvidence.nativeBuildSourceCommit)) {
    if ($revision -notmatch '^[0-9a-f]{40}$') { throw 'OpenCV 5.1.0 package evidence source identity is malformed.' }
    & git -C $repo merge-base --is-ancestor $revision HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { throw "OpenCV 5.1.0 package evidence source revision is not an ancestor of HEAD: $revision" }
}
$packageFileName = "$([string]$fullPackage[0].packageId).$([string]$fullPackage[0].nugetVersion).nupkg".ToLowerInvariant()
$packageSha256 = [string]$fullPackage[0].sha256

$dotnet = Get-Command dotnet -ErrorAction Stop
$pwsh = Get-Command pwsh -ErrorAction Stop
$smokeScript = Join-Path $repo 'scripts/Test-NativeAotSmoke.ps1'
$smokeLines = @(& $pwsh.Source -NoProfile -File $smokeScript -RepositoryRoot $repo -RuntimeIdentifier $RuntimeIdentifier -OpenCvNativeRuntimeDir $runtime 2>&1)
$smokeExitCode = $LASTEXITCODE
$smokeLines | ForEach-Object { Write-Host ([string]$_) }
if ($smokeExitCode -ne 0) { throw "NativeAOT smoke failed with exit code $smokeExitCode." }

$nativeLine = @($smokeLines | ForEach-Object { [string]$_ } | Where-Object { $_ -match '^NATIVE_AOT_SMOKE_OK ' } | Select-Object -Last 1)
if ($nativeLine.Count -ne 1 -or $nativeLine[0] -notmatch '^NATIVE_AOT_SMOKE_OK rid=(?<rid>\S+) files=(?<files>\d+) native_payload=(?<payload>\d+) native_runtime_supplied=(?<supplied>True|False)$') {
    throw 'NativeAOT smoke summary was missing or malformed.'
}
if ($Matches.rid -cne $RuntimeIdentifier -or -not [bool]::Parse($Matches.supplied)) { throw 'NativeAOT smoke did not verify the supplied native runtime.' }
$publishedFileCount = [int]$Matches.files
$nativePayloadFileCount = [int]$Matches.payload
if ($nativePayloadFileCount -ne [int]$fullPackage[0].nativeFiles) {
    throw "NativeAOT smoke published $nativePayloadFileCount native DLLs; the verified 5.1 Full package contains $($fullPackage[0].nativeFiles)."
}
$abiLines = @($smokeLines | ForEach-Object { [string]$_ } | Where-Object { $_ -match '^native_abi=(?<abi>\d+)$' })
if ($abiLines.Count -ne 1 -or $abiLines[0] -notmatch '^native_abi=(?<abi>\d+)$' -or [int]$Matches.abi -ne 1) {
    throw 'NativeAOT smoke did not report the expected native ABI version 1.'
}
$nativeAbiVersion = [int]$Matches.abi
$encodedLines = @($smokeLines | ForEach-Object { [string]$_ } | Where-Object { $_ -match '^encoded_bytes=(?<bytes>\d+)$' } | Select-Object -Last 1)
if ($encodedLines.Count -ne 1 -or $encodedLines[0] -notmatch '^encoded_bytes=(?<bytes>\d+)$') { throw 'NativeAOT smoke did not report encoded byte output.' }
$encodedBytes = [int]$Matches.bytes
if ($encodedBytes -le 0 -or @($smokeLines | ForEach-Object { [string]$_ } | Where-Object { $_ -ceq 'native_smoke=verified' }).Count -ne 1) {
    throw 'NativeAOT native smoke output was not verified.'
}
$dnnAvailable = @($smokeLines | ForEach-Object { [string]$_ } | Where-Object { $_ -ceq 'dnn_cpu_available=true' })
$dnnTargetLines = @($smokeLines | ForEach-Object { [string]$_ } | Where-Object { $_ -match '^dnn_target_count=(?<count>\d+)$' })
if ($dnnAvailable.Count -ne 1 -or $dnnTargetLines.Count -ne 1 -or $dnnTargetLines[0] -notmatch '^dnn_target_count=(?<count>\d+)$' -or [int]$Matches.count -lt 1) {
    throw 'NativeAOT optional DNN module probe did not report an available CPU target.'
}
$dnnTargetCount = [int]$Matches.count

$runtimeFiles = @(Get-ChildItem -LiteralPath $runtime -File -Filter '*.dll' | Sort-Object Name)
$packageRows = @($fullPackage[0].nativeFilesSha256 | Sort-Object { [IO.Path]::GetFileName([string]$_.path) })
if ($runtimeFiles.Count -ne $packageRows.Count) {
    throw "Native runtime directory contains $($runtimeFiles.Count) DLLs; 5.1 Full package evidence lists $($packageRows.Count)."
}
$packagePayload = [System.Collections.Generic.List[object]]::new()
for ($index = 0; $index -lt $packageRows.Count; $index++) {
    $packageRow = $packageRows[$index]
    $runtimeFile = $runtimeFiles[$index]
    $packageName = [IO.Path]::GetFileName([string]$packageRow.path)
    $runtimeHash = Get-Sha256File -Path $runtimeFile.FullName
    if ($packageName -cne $runtimeFile.Name -or [int64]$packageRow.length -ne [int64]$runtimeFile.Length -or
        [string]$packageRow.sha256 -cne $runtimeHash) {
        throw "Native runtime directory differs from 5.1 Full package evidence at $packageName."
    }
    $packagePayload.Add([ordered]@{
            name = $packageName
            bytes = [int64]$packageRow.length
            sha256 = [string]$packageRow.sha256
        })
}

$managedAssembly = Join-Path $repo 'src/OpenCvSharp/bin/Release/net10.0/JYPPX.OpenCV.CSharp.API.dll'
if (-not (Test-Path -LiteralPath $managedAssembly -PathType Leaf)) { throw "Managed assembly is missing: $managedAssembly" }
$aotProjectPath = Join-Path $repo 'tools/AotSmoke/AotSmoke.csproj'
$assetsPath = Join-Path $repo 'tools/AotSmoke/obj/project.assets.json'
foreach ($path in @($aotProjectPath, $assetsPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "NativeAOT tooling input is missing: $path" }
}
[xml]$aotProject = Get-Content -LiteralPath $aotProjectPath -Raw
$runtimeFrameworkVersion = [string]$aotProject.SelectSingleNode('/Project/PropertyGroup/RuntimeFrameworkVersion').InnerText
$auditNode = $aotProject.SelectSingleNode('/Project/PropertyGroup/NuGetAudit')
$warningsAsErrorsNode = $aotProject.SelectSingleNode('/Project/PropertyGroup/WarningsAsErrors')
$nugetAuditEnabled = $null -ne $auditNode -and [bool]::Parse([string]$auditNode.InnerText)
$nu1903TreatedAsError = $null -ne $warningsAsErrorsNode -and [string]$warningsAsErrorsNode.InnerText -match '(^|[;,\s])NU1903($|[;,\s])'
$assets = Get-Content -LiteralPath $assetsPath -Raw | ConvertFrom-Json
$assetLibraryNames = @($assets.libraries.PSObject.Properties | ForEach-Object { [string]$_.Name })
function Get-ToolPackageVersion {
    param([Parameter(Mandatory)][string]$PackageId)
    $matches = @($assetLibraryNames | Where-Object { $_.StartsWith($PackageId + '/', [StringComparison]::OrdinalIgnoreCase) })
    if ($matches.Count -ne 1) { throw "Expected one resolved $PackageId package in NativeAOT restore assets." }
    return $matches[0].Substring($PackageId.Length + 1)
}
$ilCompilerVersion = Get-ToolPackageVersion -PackageId 'Microsoft.DotNet.ILCompiler'
$ilLinkTasksVersion = Get-ToolPackageVersion -PackageId 'Microsoft.NET.ILLink.Tasks'
if ($runtimeFrameworkVersion -notmatch '^10\.0\.\d+$' -or
    [version]::Parse($runtimeFrameworkVersion) -lt [version]'10.0.14' -or
    $ilCompilerVersion -cne $runtimeFrameworkVersion -or $ilLinkTasksVersion -cne $runtimeFrameworkVersion -or
    -not $nugetAuditEnabled -or -not $nu1903TreatedAsError) {
    throw "NativeAOT toolchain policy drifted: runtime=$runtimeFrameworkVersion ilcompiler=$ilCompilerVersion illink=$ilLinkTasksVersion audit=$nugetAuditEnabled NU1903Error=$nu1903TreatedAsError"
}
$evidence = [ordered]@{
    '$schema' = 'native-aot-smoke-evidence.schema.json'
    schemaVersion = 1
    status = 'verified'
    sourceCommit = $sourceCommit
    runner = [ordered]@{
        os = [Runtime.InteropServices.RuntimeInformation]::OSDescription
        architecture = 'x64'
        dotnet = ((& $dotnet.Source --version 2>$null) | Select-Object -First 1).Trim()
        configuration = 'Release'
    }
    runtimeIdentifier = $RuntimeIdentifier
    targetFramework = 'net10.0'
    sdk = ((& $dotnet.Source --version 2>$null) | Select-Object -First 1).Trim()
    configuration = 'Release'
    publishAot = $true
    publishTrimmed = $true
    trimMode = 'partial'
    publishSingleFile = $true
    nativeAotToolchain = [ordered]@{
        runtimeFrameworkVersion = $runtimeFrameworkVersion
        ilCompilerVersion = $ilCompilerVersion
        ilLinkTasksVersion = $ilLinkTasksVersion
        nugetAuditEnabled = $nugetAuditEnabled
        nu1903TreatedAsError = $nu1903TreatedAsError
    }
    nativeRuntimeArtifact = "package-consumer-evidence:$packageEvidenceRelativePath"
    nativeRuntimeArtifactSha256 = $packageSha256
    nativeRuntimePackage = $packageFileName
    nativeRuntimePackageSha256 = $packageSha256
    managedAssemblySha256 = Get-Sha256File -Path $managedAssembly
    managedPackageVersion = [string]$managedPackage[0].nugetVersion
    openCvVersion = [string]$packageEvidence.openCvVersion
    nativeAbiVersion = $nativeAbiVersion
    publishedFileCount = $publishedFileCount
    nativePayloadFileCount = $nativePayloadFileCount
    nativeSmoke = 'verified'
    encodedBytes = $encodedBytes
    dnnCpuAvailable = $true
    dnnTargetCount = $dnnTargetCount
    nativePayload = @($packagePayload)
    limitations = @(
        'This record binds a current-source Windows x64 NativeAOT consumer run to the verified OpenCV 5.1.0 Full package evidence and all 43 native DLL hashes.',
        'The AOT smoke copies the exact native DLL bytes recorded in the local package-consumer evidence; it does not restore the runtime package through NuGet.',
        'The win-arm64 linker and native runtime evidence remain pending until a matching Visual Studio ARM64 toolchain and runtime payload are available.',
        'This smoke does not promote package-wide IsAotCompatible or IsTrimmable metadata.'
    )
}

$outputFullPath = Resolve-OutputPath -Path $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) | Out-Null
[IO.File]::WriteAllText($outputFullPath, (($evidence | ConvertTo-Json -Depth 12) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Host "NATIVE_AOT_SMOKE_EVIDENCE_MEASURED source_commit=$sourceCommit package_sha256=$packageSha256 output=$outputFullPath"
