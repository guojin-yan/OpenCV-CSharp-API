. (Join-Path $PSScriptRoot 'LocalLinux51.Common.ps1')

function Get-LocalLinux51ModuleSpecs {
    @(
        [pscustomobject]@{ rid = 'ubuntu.22.04-x64'; distro = 'ubuntu'; version = '22.04'; libc = 'glibc 2.35'; image = 'ubuntu:22.04@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982' },
        [pscustomobject]@{ rid = 'debian.12-x64'; distro = 'debian'; version = '12'; libc = 'glibc 2.36'; image = 'debian:12@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc' },
        [pscustomobject]@{ rid = 'fedora.44-x64'; distro = 'fedora'; version = '44'; libc = 'glibc 2.43'; image = 'fedora:44@sha256:43b29f65a41eb9c35e1cd5323e3bdf3b655c2357a9f4f1ff2f9c2798e5045d80' }
    )
}

function Get-LocalLinux51ModuleSourceInputs {
    param([string]$RepositoryRoot)
    $paths = [string[]]@('tools/LinuxPackageModuleSmoke/Program.cs', 'tools/LinuxPackageModuleSmoke/consumer.csproj.in',
        'tests/OpenCvSharp.Tests/Dnn/Fixtures/identity-opset13.onnx.base64', 'scripts/Initialize-LocalLinux51ModuleConsumer.sh',
        'scripts/Invoke-LocalLinux51ModuleConsumer.ps1', 'scripts/LocalLinux51Module.Common.ps1',
        'scripts/Measure-LocalLinux51ModuleEvidence.ps1', 'scripts/Test-LocalLinux51ModuleEvidence.ps1', 'scripts/LocalLinux51.Common.ps1')
    [Array]::Sort($paths, [StringComparer]::Ordinal)
    foreach ($path in $paths) {
        $text = [IO.File]::ReadAllText((Join-Path $RepositoryRoot $path)).Replace("`r`n", "`n").Replace("`r", "`n").TrimEnd() + "`n"
        [ordered]@{ path = $path; bytes = [Text.Encoding]::UTF8.GetByteCount($text); sha256 = Get-LocalLinux51TextHash $text }
    }
}

function Get-LocalLinux51ModuleBlob {
    param([string]$Directory, [string]$Name)
    $file = Join-Path $Directory $Name
    $text = [IO.File]::ReadAllText($file)
    [ordered]@{ name = $Name; text = $text; bytes = (Get-Item -LiteralPath $file).Length; sha256 = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() }
}

function Get-LocalLinux51ModuleConsumerRecord {
    param([string]$RepositoryRoot, [string]$OutputDirectory, [string]$Container, [string]$Rid)
    $identity = Get-LocalLinux51DockerIdentity $Container
    $image = @((& docker image inspect $identity.imageId) | ConvertFrom-Json)[0]
    if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect module consumer image.' }
    $identity.imageArchitecture = $image.Architecture; $identity.imageOs = $image.Os
    $runs = foreach ($tfm in @('net8.0', 'net10.0')) {
        foreach ($profile in @('full', 'mini')) {
            $runDirectory = Join-Path $OutputDirectory "$tfm-$profile"
            $blobs = foreach ($file in @('Consumer.csproj', 'project.assets.json', 'result.json', 'restore.stdout.log', 'restore.stderr.log',
                    'build.stdout.log', 'build.stderr.log', 'execute.stdout.log', 'execute.stderr.log')) { Get-LocalLinux51ModuleBlob $runDirectory $file }
            $avi = Join-Path $runDirectory 'three-frames.avi'
            [ordered]@{
                targetFramework = $tfm; profile = $profile; blobs = @($blobs)
                video = [ordered]@{ artifactPath = [IO.Path]::GetRelativePath($RepositoryRoot, $avi).Replace('\', '/'); bytes = (Get-Item $avi).Length; sha256 = (Get-FileHash $avi -Algorithm SHA256).Hash.ToLowerInvariant() }
            }
        }
    }
    [ordered]@{
        rid = $Rid; identity = $identity
        outputDirectory = [IO.Path]::GetRelativePath($RepositoryRoot, $OutputDirectory).Replace('\', '/')
        blobs = @(foreach ($file in @('bootstrap.log', 'driver.log', 'environment.json', 'runs.json')) { Get-LocalLinux51ModuleBlob $OutputDirectory $file })
        runs = @($runs)
    }
}
