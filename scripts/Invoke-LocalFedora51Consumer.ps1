[CmdletBinding()]
param([string]$ConsumerRoot = '/consumer')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $IsLinux -or (& uname -m) -cne 'x86_64') { throw 'Expected native Linux x64.' }
foreach ($tool in @('gcc', 'g++', 'clang', 'clang++', 'cc', 'c++', 'cmake', 'ninja')) {
    if (Get-Command $tool -ErrorAction SilentlyContinue) { throw "Compiler present in independent consumer: $tool" }
}
foreach ($path in @('/workspace', '/repo-input', '/upstream-input')) {
    if (Test-Path $path) { throw "Producer path visible: $path" }
}
if (@(Get-ChildItem Env: | Where-Object Name -Match '^OPENCV_|^OpenCvSharp|^LD_LIBRARY_PATH$|^LD_PRELOAD$|^LD_AUDIT$').Count) { throw 'Native loader override present.' }
function Get-ModuleConsumerHash([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
function Write-ModuleConsumerText([string]$Path, [string]$Text) { [IO.File]::WriteAllText($Path, $Text, [Text.UTF8Encoding]::new($false)) }
function Invoke-ModuleConsumerTool {
    param([string]$Executable, [string[]]$Arguments, [string]$WorkingDirectory, [string]$LogPrefix, [int]$TimeoutSeconds = 300)
    $start = [Diagnostics.ProcessStartInfo]::new($Executable)
    $start.WorkingDirectory = $WorkingDirectory
    $start.UseShellExecute = $false; $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
    foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEndAsync(); $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) { $process.Kill($true); throw 'Consumer tool timed out.' }
    Write-ModuleConsumerText "$LogPrefix.stdout.log" ($stdout.GetAwaiter().GetResult())
    Write-ModuleConsumerText "$LogPrefix.stderr.log" ($stderr.GetAwaiter().GetResult())
    $exitCode = $process.ExitCode; $process.Dispose()
    if ($exitCode -ne 0) { Get-Content "$LogPrefix.stderr.log" -Tail 20; Get-Content "$LogPrefix.stdout.log" -Tail 20; throw "Consumer tool failed: $Executable/$exitCode" }
}
$os = @{}
foreach ($line in Get-Content /etc/os-release) { if ($line -match '^([A-Z_]+)=(.*)$') { $os[$Matches[1]] = $Matches[2].Trim('"') } }
$rid = "$($os.ID).$($os.VERSION_ID)-x64"
if ($rid -cne 'fedora.44-x64') { throw 'Unexpected distro.' }
$source = Join-Path $ConsumerRoot 'source'; $artifacts = Join-Path $ConsumerRoot 'artifacts'
$feed = Join-Path $ConsumerRoot 'feed'
if (Test-Path $feed) { if (@(Get-ChildItem $feed -File).Count) { throw 'Fresh consumer feed required.' } }
else { [IO.Directory]::CreateDirectory($feed) | Out-Null }
Copy-Item /kit/packages/* $feed
$packageManifest = @(Get-Content /kit/packages.json -Raw | ConvertFrom-Json)
if ($packageManifest.Count -ne 3 -or @($packageManifest.name | Select-Object -Unique).Count -ne 3 -or @(Get-ChildItem $feed -File).Count -ne 3) { throw 'Expected exactly three measured packages.' }
foreach ($package in $packageManifest) {
    $file = Join-Path $feed $package.name
    if ((Get-ModuleConsumerHash $file) -cne $package.sha256 -or (Get-Item $file).Length -ne $package.bytes) { throw 'Measured Fedora package bytes drifted.' }
}
$environment = [ordered]@{
    consumerRid = $rid; distro = $os.ID; distroVersion = $os.VERSION_ID; architecture = (& uname -m)
    libc = (& getconf GNU_LIBC_VERSION); powerShellVersion = $PSVersionTable.PSVersion.ToString()
    compilerAbsent = $true; producerPathsAbsent = $true; nativeLoaderOverridesAbsent = $true
    sourceManifest = @(Get-ChildItem $source -File | Sort-Object Name | ForEach-Object { [ordered]@{ name = $_.Name; bytes = $_.Length; sha256 = Get-ModuleConsumerHash $_.FullName } })
    scriptManifest = @(Get-ChildItem (Join-Path $ConsumerRoot 'scripts') -File | Sort-Object Name | ForEach-Object { [ordered]@{ name = $_.Name; bytes = $_.Length; sha256 = Get-ModuleConsumerHash $_.FullName } })
    packages = $packageManifest
}
Write-ModuleConsumerText (Join-Path $artifacts 'environment.json') (($environment | ConvertTo-Json -Depth 8) + "`n")
$runs = @()
foreach ($tfm in @('net8.0', 'net10.0')) {
    $toolchain = if ($tfm -ceq 'net8.0') { '/opt/dotnet8' } else { '/opt/dotnet10' }
    $runtimeVersion = if ($tfm -ceq 'net8.0') { '8.0.34' } else { '10.0.12' }
    $sdkVersion = if ($tfm -ceq 'net8.0') { '8.0.428' } else { '10.0.401' }
    $env:DOTNET_ROOT = $toolchain; $env:DOTNET_MULTILEVEL_LOOKUP = '0'; $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    $dotnet = "$toolchain/dotnet"
    if ((& $dotnet --version) -cne $sdkVersion) { throw 'Selected SDK drifted.' }
    foreach ($profile in @('full', 'mini')) {
        $run = Join-Path $ConsumerRoot "runs/$tfm-$profile"
        if (Test-Path $run) { throw 'Each consumer run requires a fresh project and package cache.' }
        [IO.Directory]::CreateDirectory($run) | Out-Null
        $env:NUGET_PACKAGES = Join-Path $run 'nuget'
        $runtimePackage = if ($profile -ceq 'full') { 'JYPPX.OpenCV.runtime.fedora.44-x64' } else { 'JYPPX.OpenCV.runtime.fedora.44-x64.mini' }
        $project = (Get-Content (Join-Path $source 'fedora-consumer.csproj.in') -Raw).Replace('__TFM__', $tfm).Replace('__RUNTIME_VERSION__', $runtimeVersion).Replace('__RUNTIME_PACKAGE__', $runtimePackage)
        Write-ModuleConsumerText (Join-Path $run 'Consumer.csproj') $project
        Write-ModuleConsumerText (Join-Path $run 'NuGet.Config') "<configuration><packageSources><clear/><add key=`"local`" value=`"$feed`" /></packageSources></configuration>`n"
        Copy-Item (Join-Path $source 'Program.cs') $run
        Copy-Item (Join-Path $source 'candidate-rid-graph.json') $run
        Copy-Item (Join-Path $source 'identity-opset13.onnx.base64') $run
        $resultDir = Join-Path $artifacts "$tfm-$profile"
        [IO.Directory]::CreateDirectory($resultDir) | Out-Null
        Invoke-ModuleConsumerTool $dotnet @('restore', 'Consumer.csproj', '--configfile', 'NuGet.Config', '--no-cache') $run (Join-Path $resultDir 'restore')
        Invoke-ModuleConsumerTool $dotnet @('build', 'Consumer.csproj', '--no-restore', '-c', 'Release') $run (Join-Path $resultDir 'build')
        $assembly = Join-Path $run "bin/Release/$tfm/fedora.44-x64/Consumer.dll"
        Invoke-ModuleConsumerTool $dotnet @($assembly, $profile, $resultDir) $run (Join-Path $resultDir 'execute') 60
        $result = Get-Content (Join-Path $resultDir 'result.json') -Raw | ConvertFrom-Json
        if ($result.status -cne 'passed' -or $result.targetFramework -cne $tfm -or $result.runtimeVersion -cne $runtimeVersion) { throw 'Native execution used the wrong runtime or did not pass.' }
        Copy-Item (Join-Path $run 'obj/project.assets.json') (Join-Path $resultDir 'project.assets.json')
        Copy-Item (Join-Path $run 'Consumer.csproj') $resultDir
        Copy-Item (Join-Path $run 'candidate-rid-graph.json') $resultDir
        $runs += [ordered]@{ targetFramework = $tfm; profile = $profile; sdkVersion = $sdkVersion; runtimeVersion = $runtimeVersion; resultDirectory = "$tfm-$profile"; freshPackageCache = $true }
        Write-Host "FEDORA_51_CONSUMER_RUN_OK rid=$rid tfm=$tfm profile=$profile video_frames=3 dnn=$($result.dnn.status)"
    }
}
Write-ModuleConsumerText (Join-Path $artifacts 'runs.json') (($runs | ConvertTo-Json -Depth 5) + "`n")
Write-Host "FEDORA_51_CONSUMER_OK rid=$rid runs=4"
