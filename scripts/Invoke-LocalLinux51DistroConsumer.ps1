[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('debian.12-x64', 'fedora.44-x64')]
    [string]$ConsumerRid,
    [string]$RepositoryRoot = '/consumer/repository',
    [string]$ArtifactRoot = '/consumer/artifacts'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $IsLinux -or (& uname -m) -cne 'x86_64') { throw 'Expected a native Linux x64 independent consumer.' }
$osRelease = @{}
foreach ($line in Get-Content /etc/os-release) {
    if ($line -match '^([A-Z_]+)=(.*)$') { $osRelease[$Matches[1]] = $Matches[2].Trim('"') }
}
$expectedDistro = if ($ConsumerRid -ceq 'debian.12-x64') { 'debian' } else { 'fedora' }
$expectedVersion = if ($expectedDistro -ceq 'debian') { '12' } else { '44' }
if ($osRelease.ID -cne $expectedDistro -or $osRelease.VERSION_ID -cne $expectedVersion) { throw 'Consumer distro does not match the selected image.' }
foreach ($tool in @('gcc', 'g++', 'clang', 'clang++', 'cc', 'c++', 'cmake', 'ninja')) {
    if (Get-Command $tool -ErrorAction SilentlyContinue) { throw "Compiler/build tool present in independent consumer: $tool" }
}
foreach ($path in @('/workspace', '/repo-input', '/upstream-input')) {
    if (Test-Path $path) { throw "Producer path is visible: $path" }
}
foreach ($item in @(Get-ChildItem Env: | Where-Object Name -Match '^OPENCV_|^OpenCvSharp|^LD_LIBRARY_PATH$|^LD_PRELOAD$|^LD_AUDIT$')) {
    throw "Loader override present in consumer: $($item.Name)"
}
function Get-DistroConsumerHash {
    param([string]$Path)
    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}
$matrixRelativePath = 'packaging/runtime/runtime-generic-linux-5.1-local-consumer-matrix.json'
$matrix = Get-Content (Join-Path $RepositoryRoot $matrixRelativePath) -Raw | ConvertFrom-Json
if ($matrix.publicationAllowed -or $matrix.activePackageIdentityAllowed -or $matrix.targetRid -cne 'linux-x64') { throw 'Expected an isolated preview consumer matrix.' }
$selected = @($matrix.consumers | Where-Object rid -CEQ $ConsumerRid)
if ($selected.Count -ne 1 -or $selected[0].execution -cne 'container' -or $selected[0].runner -cne 'local-docker') { throw 'Invalid local consumer selection.' }

# Add the candidate RID only to this disposable validation kit's graph. The
# canonical repository graph and the already produced packages are unchanged.
$graphPath = Join-Path $RepositoryRoot 'packaging/runtime/runtime-distro-rid-graph.json'
$graphBeforeSha256 = Get-DistroConsumerHash $graphPath
$graph = Get-Content $graphPath -Raw | ConvertFrom-Json
$addedRids = @()
if (-not $graph.runtimes.PSObject.Properties[$ConsumerRid]) {
    $graph.runtimes | Add-Member -NotePropertyName $ConsumerRid -NotePropertyValue ([pscustomobject]@{ '#import' = @('linux-x64') })
    $addedRids += $ConsumerRid
    [IO.File]::WriteAllText($graphPath, (($graph | ConvertTo-Json -Depth 8) -replace "`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
}
if (($graph.runtimes.$ConsumerRid.'#import' -join '|') -cne 'linux-x64') { throw 'Isolated consumer RID must import only linux-x64.' }
$env:DOTNET_ROOT = '/opt/dotnet'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$managed = Join-Path $ArtifactRoot 'nupkg-managed/JYPPX.OpenCV.CSharp.API.5.1.0.nupkg'
if ((Get-DistroConsumerHash $managed) -cne '0081cd584da74aac7e51187985fbe4ceff3406f960525734633a3ba32768fd80') { throw 'Verified managed 5.1 package changed.' }
$packageHashes = @{
    full = 'd947dbbee1d923c2b3eeb2106c88eb4b4489cee3dbb87c61c0613b209b71cb4d'
    mini = '9dbc87064ad6c35809cc4c48afa8f2cbb5ce0d421a2b8ed55b189145de87af5e'
}
$preflight = [ordered]@{
    schemaVersion = 1; consumerRid = $ConsumerRid; execution = 'local-docker'; hosted = $false
    publicationAllowed = $false; distro = $osRelease.ID; distroVersion = $osRelease.VERSION_ID
    architecture = (& uname -m); libc = (& getconf GNU_LIBC_VERSION)
    powerShell = $PSVersionTable.PSVersion.ToString(); dotNetSdk = (& dotnet --version)
    compilerAbsent = $true; producerPathsAbsent = $true; loaderOverridesAbsent = $true
    matrixSha256 = Get-DistroConsumerHash (Join-Path $RepositoryRoot $matrixRelativePath)
    graphBeforeSha256 = $graphBeforeSha256; graphAfterSha256 = Get-DistroConsumerHash $graphPath
    addedGraphRids = $addedRids; graphJson = [IO.File]::ReadAllText($graphPath)
}
$preflightPath = Join-Path $ArtifactRoot "consumer-preflight-$ConsumerRid.json"
[IO.File]::WriteAllText($preflightPath, (($preflight | ConvertTo-Json -Depth 8) -replace "`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
foreach ($profile in @('full', 'mini')) {
    $id = if ($profile -ceq 'full') { 'JYPPX.OpenCV.runtime.linux-x64.preview' } else { 'JYPPX.OpenCV.runtime.linux-x64.preview.mini' }
    if ((Get-DistroConsumerHash (Join-Path $ArtifactRoot "nupkg-linux-x64-preview-$profile/$id.5.1.0.nupkg")) -cne $packageHashes[$profile]) { throw 'Verified native preview package changed.' }
    $log = Join-Path $ArtifactRoot "consumer-$ConsumerRid-$profile.log"
    & pwsh -NoProfile -File (Join-Path $RepositoryRoot 'scripts/Test-GenericLinuxPreviewConsumer.ps1') `
        -RepositoryRoot $RepositoryRoot -ArtifactRoot $ArtifactRoot -ConsumerRid $ConsumerRid `
        -PreviewMatrixPath $matrixRelativePath -RuntimeProfile $profile -ExpectedPackageVersion 5.1.0.0 -OpenCvVersion 5.1.0 -RunNativeSmoke *> $log
    if ($LASTEXITCODE -ne 0) { Get-Content $log -Tail 35; throw "Independent consumer failed: $ConsumerRid/$profile" }
    $output = [IO.File]::ReadAllText($log)
    $marker = if ($profile -ceq 'full') { 'TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,dnn,ml_knn profile=full' }
        else { 'TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,not_linked profile=mini' }
    if (-not $output.Contains($marker) -or -not $output.Contains('GENERIC_LINUX_PREVIEW_CONSUMER_OK')) { throw 'Missing actual consumer execution marker.' }
    Write-Host $marker
}
Write-Host "LOCAL_LINUX_51_DISTRO_CONSUMER_OK rid=$ConsumerRid compiler_absent=true producer_paths_absent=true loader_overrides_absent=true"
