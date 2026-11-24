[CmdletBinding()]
param([string]$ProducerRoot = '/producer')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $IsLinux -or (& uname -m) -cne 'x86_64' -or (& getconf GNU_LIBC_VERSION) -cne 'glibc 2.35') { throw 'Expected native Ubuntu 22.04 x64 AOT producer.' }
if (@(Get-ChildItem Env: | Where-Object Name -Match '^OPENCV_|^OpenCvSharp|^LD_LIBRARY_PATH$|^LD_PRELOAD$|^LD_AUDIT$').Count) { throw 'Native loader overrides are forbidden.' }
$env:DOTNET_ROOT = '/opt/dotnet'; $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'; $env:DOTNET_PROCESSOR_COUNT = '2'
if ((& dotnet --version) -cne '10.0.401') { throw 'Unexpected producer SDK.' }
function Write-AotText([string]$Path, [string]$Text) { [IO.File]::WriteAllText($Path, $Text, [Text.UTF8Encoding]::new($false)) }
function Get-AotHash([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
function Invoke-AotProducerTool {
    param([string]$Executable, [string[]]$Arguments, [string]$WorkingDirectory, [string]$LogPrefix, [int]$TimeoutSeconds = 600)
    $start = [Diagnostics.ProcessStartInfo]::new($Executable)
    $start.WorkingDirectory = $WorkingDirectory; $start.UseShellExecute = $false
    $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
    foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEndAsync(); $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) { $process.Kill($true); throw 'AOT producer tool timed out.' }
    Write-AotText "$LogPrefix.stdout.log" ($stdout.GetAwaiter().GetResult())
    Write-AotText "$LogPrefix.stderr.log" ($stderr.GetAwaiter().GetResult())
    $exitCode = $process.ExitCode; $process.Dispose()
    if ($exitCode -ne 0) { Get-Content "$LogPrefix.stdout.log" -Tail 35; Get-Content "$LogPrefix.stderr.log" -Tail 35; throw "AOT tool failed: $Executable/$exitCode" }
}
$expectedPackages = @{
    'JYPPX.OpenCV.CSharp.API.5.1.0.nupkg' = '0081cd584da74aac7e51187985fbe4ceff3406f960525734633a3ba32768fd80'
    'JYPPX.OpenCV.runtime.linux-x64.preview.5.1.0.nupkg' = 'd947dbbee1d923c2b3eeb2106c88eb4b4489cee3dbb87c61c0613b209b71cb4d'
    'JYPPX.OpenCV.runtime.linux-x64.preview.mini.5.1.0.nupkg' = '9dbc87064ad6c35809cc4c48afa8f2cbb5ce0d421a2b8ed55b189145de87af5e'
}
$feed = Join-Path $ProducerRoot 'feed'; $output = Join-Path $ProducerRoot 'artifacts'
if (Test-Path $output) { throw 'AOT producer requires a fresh output directory.' }
[IO.Directory]::CreateDirectory($output) | Out-Null
foreach ($name in $expectedPackages.Keys) { if ((Get-AotHash (Join-Path $feed $name)) -cne $expectedPackages[$name]) { throw 'AOT input package bytes drifted.' } }
$source = Join-Path $ProducerRoot 'source'
$profiles = foreach ($profile in @('full', 'mini')) {
    $work = Join-Path $ProducerRoot "build-$profile"; $profileOutput = Join-Path $output $profile
    if (Test-Path $work) { throw 'AOT producer requires a fresh project and cache.' }
    [IO.Directory]::CreateDirectory($work) | Out-Null; [IO.Directory]::CreateDirectory($profileOutput) | Out-Null
    $runtimeId = if ($profile -ceq 'full') { 'JYPPX.OpenCV.runtime.linux-x64.preview' } else { 'JYPPX.OpenCV.runtime.linux-x64.preview.mini' }
    $project = (Get-Content (Join-Path $source 'consumer.csproj.in') -Raw).Replace('__RUNTIME_PACKAGE__', $runtimeId)
    Write-AotText (Join-Path $work 'Consumer.csproj') $project
    Copy-Item (Join-Path $source 'Program.cs') $work
    $config = @"
<configuration>
  <packageSources><clear/><add key="local" value="$feed"/><add key="nuget" value="https://api.nuget.org/v3/index.json"/></packageSources>
  <packageSourceMapping><packageSource key="local"><package pattern="JYPPX.*"/></packageSource><packageSource key="nuget"><package pattern="Microsoft.*"/><package pattern="runtime.*"/><package pattern="System.*"/></packageSource></packageSourceMapping>
</configuration>
"@
    Write-AotText (Join-Path $work 'NuGet.Config') ($config + "`n")
    $env:NUGET_PACKAGES = Join-Path $work 'nuget'
    Invoke-AotProducerTool '/opt/dotnet/dotnet' @('restore', 'Consumer.csproj', '--configfile', 'NuGet.Config', '--no-cache') $work (Join-Path $profileOutput 'restore')
    $assetsPath = Join-Path $work 'obj/project.assets.json'
    $assets = Get-Content $assetsPath -Raw | ConvertFrom-Json
    # NativeAOT carries the runtime in the platform ILCompiler pack, rather than the CoreCLR runtime pack.
    foreach ($id in @('Microsoft.DotNet.ILCompiler', 'runtime.linux-x64.Microsoft.DotNet.ILCompiler', 'Microsoft.NET.ILLink.Tasks')) {
        if (@($assets.libraries.PSObject.Properties.Name | Where-Object { $_ -ceq "$id/10.0.15" }).Count -ne 1) { throw "AOT toolchain version drifted: $id" }
    }
    $publish = Join-Path $profileOutput 'publish'
    Invoke-AotProducerTool '/opt/dotnet/dotnet' @('publish', 'Consumer.csproj', '--no-restore', '-c', 'Release', '-o', $publish) $work (Join-Path $profileOutput 'publish')
    $logs = (Get-Content (Join-Path $profileOutput 'restore.stdout.log') -Raw) + (Get-Content (Join-Path $profileOutput 'restore.stderr.log') -Raw) +
        (Get-Content (Join-Path $profileOutput 'publish.stdout.log') -Raw) + (Get-Content (Join-Path $profileOutput 'publish.stderr.log') -Raw)
    if ($logs -match '(?im)\bwarning\s+(?:IL|NU|CS|NETSDK)[0-9]+\s*:') { throw 'AOT restore/publish emitted an unreviewed warning.' }
    if (-not (Test-Path (Join-Path $publish 'LinuxAotConsumer')) -or @(Get-ChildItem $publish -File -Filter '*.dll').Count) { throw 'AOT publish did not produce a native executable without managed DLLs.' }
    Invoke-AotProducerTool 'readelf' @('-hW', (Join-Path $publish 'LinuxAotConsumer')) $work (Join-Path $profileOutput 'elf-header')
    Invoke-AotProducerTool 'readelf' @('-dW', (Join-Path $publish 'LinuxAotConsumer')) $work (Join-Path $profileOutput 'elf-dynamic')
    Invoke-AotProducerTool 'readelf' @('--version-info', '-W', (Join-Path $publish 'LinuxAotConsumer')) $work (Join-Path $profileOutput 'elf-versions')
    Invoke-AotProducerTool 'ldd' @((Join-Path $publish 'LinuxAotConsumer')) $work (Join-Path $profileOutput 'ldd')
    Copy-Item $assetsPath (Join-Path $profileOutput 'project.assets.json')
    Copy-Item (Join-Path $work 'Consumer.csproj'),(Join-Path $work 'NuGet.Config') $profileOutput
    $managed = Join-Path $env:NUGET_PACKAGES 'jyppx.opencv.csharp.api/5.1.0/lib/net10.0/JYPPX.OpenCV.CSharp.API.dll'
    $packageArchives = @(Get-ChildItem $env:NUGET_PACKAGES -Recurse -File -Filter '*.nupkg' | Sort-Object FullName | ForEach-Object {
        [ordered]@{ path = [IO.Path]::GetRelativePath($env:NUGET_PACKAGES, $_.FullName).Replace('\', '/'); bytes = $_.Length; sha256 = Get-AotHash $_.FullName; sha512 = (Get-FileHash $_.FullName -Algorithm SHA512).Hash.ToLowerInvariant() }
    })
    $publishedFiles = @(Get-ChildItem $publish -File | Sort-Object Name -CaseSensitive | ForEach-Object { [ordered]@{ name = $_.Name; bytes = $_.Length; sha256 = Get-AotHash $_.FullName } })
    Write-Host "LOCAL_LINUX_51_AOT_PUBLISH_OK profile=$profile files=$($publishedFiles.Count) warnings=0"
    [ordered]@{ name = $profile; managedAssemblySha256 = Get-AotHash $managed; freshProjectAndCache = $true; warningCount = 0; packageArchives = $packageArchives; publishedFiles = $publishedFiles }
}
$record = [ordered]@{
    schemaVersion = 1; sdk = (& dotnet --version); runtimeFrameworkVersion = '10.0.15'; targetFramework = 'net10.0'; runtimeIdentifier = 'linux-x64'
    clang = ((& clang --version) -join "`n"); libc = (& getconf GNU_LIBC_VERSION); architecture = (& uname -m)
    publishAot = $true; publishTrimmed = $true; trimMode = 'partial'; publishSingleFile = $true; selfContained = $true
    nugetAuditEnabled = $true; nu1903TreatedAsError = $true; profiles = @($profiles)
    sourceFiles = @(Get-ChildItem $source,(Join-Path $ProducerRoot 'scripts') -File | Sort-Object FullName -CaseSensitive | ForEach-Object {
        [ordered]@{ path = [IO.Path]::GetRelativePath($ProducerRoot, $_.FullName).Replace('\', '/'); bytes = $_.Length; sha256 = Get-AotHash $_.FullName }
    })
}
Write-AotText (Join-Path $output 'producer.json') (($record | ConvertTo-Json -Depth 10) + "`n")
Write-Host 'LOCAL_LINUX_51_AOT_PRODUCER_OK profiles=2 warnings=0'
