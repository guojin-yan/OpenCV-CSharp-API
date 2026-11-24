[CmdletBinding()]
param(
    [string]$RepositoryRoot = '/consumer/repository',
    [string]$ArtifactRoot = '/consumer/artifacts'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $IsLinux -or (& uname -m) -cne 'x86_64' -or (& getconf GNU_LIBC_VERSION) -cne 'glibc 2.35') { throw 'Unexpected consumer platform.' }
foreach ($tool in @('gcc', 'g++', 'cmake', 'ninja')) {
    if (Get-Command $tool -ErrorAction SilentlyContinue) { throw "Compiler/build tool present in independent consumer: $tool" }
}
foreach ($path in @('/workspace', '/repo-input', '/upstream-input')) {
    if (Test-Path $path) { throw "Producer path is visible: $path" }
}
foreach ($item in @(Get-ChildItem Env: | Where-Object Name -Match '^OPENCV_|^OpenCvSharp|^LD_LIBRARY_PATH$|^LD_PRELOAD$|^LD_AUDIT$')) {
    throw "Loader override present in consumer: $($item.Name)"
}
$env:DOTNET_ROOT = '/opt/dotnet'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$managed = Join-Path $ArtifactRoot 'nupkg-managed/JYPPX.OpenCV.CSharp.API.5.1.0.nupkg'
if ((Get-FileHash $managed -Algorithm SHA256).Hash.ToLowerInvariant() -cne '0081cd584da74aac7e51187985fbe4ceff3406f960525734633a3ba32768fd80') { throw 'Verified managed 5.1 package changed.' }
foreach ($profile in @('full', 'mini')) {
    $log = Join-Path $ArtifactRoot "consumer-ubuntu.22.04-x64-$profile.log"
    & pwsh -NoProfile -File (Join-Path $RepositoryRoot 'scripts/Test-GenericLinuxPreviewConsumer.ps1') `
        -RepositoryRoot $RepositoryRoot -ArtifactRoot $ArtifactRoot -ConsumerRid ubuntu.22.04-x64 `
        -RuntimeProfile $profile -ExpectedPackageVersion 5.1.0.0 -OpenCvVersion 5.1.0 -RunNativeSmoke *> $log
    if ($LASTEXITCODE -ne 0) { Get-Content $log -Tail 30; throw "Independent consumer failed: $profile" }
    $output = [IO.File]::ReadAllText($log)
    $marker = if ($profile -ceq 'full') { 'TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,dnn,ml_knn profile=full' }
        else { 'TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,not_linked profile=mini' }
    if (-not $output.Contains($marker) -or -not $output.Contains('GENERIC_LINUX_PREVIEW_CONSUMER_OK')) { throw 'Missing actual consumer execution marker.' }
    Write-Host $marker
}
Write-Host 'LOCAL_LINUX_51_INDEPENDENT_CONSUMER_OK compiler_absent=true producer_paths_absent=true loader_overrides_absent=true'
