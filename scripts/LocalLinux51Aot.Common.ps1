. (Join-Path $PSScriptRoot 'LocalLinux51.Common.ps1')

function Get-LocalLinux51AotSpecs {
    @(
        [pscustomobject]@{ rid = 'ubuntu.22.04-x64'; distro = 'ubuntu'; version = '22.04'; libc = 'glibc 2.35'; image = 'ubuntu:22.04@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982' },
        [pscustomobject]@{ rid = 'debian.12-x64'; distro = 'debian'; version = '12'; libc = 'glibc 2.36'; image = 'debian:12@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc' },
        [pscustomobject]@{ rid = 'fedora.44-x64'; distro = 'fedora'; version = '44'; libc = 'glibc 2.43'; image = 'fedora:44@sha256:43b29f65a41eb9c35e1cd5323e3bdf3b655c2357a9f4f1ff2f9c2798e5045d80' }
    )
}

function Get-LocalLinux51AotSourceInputs {
    param([string]$RepositoryRoot)
    $paths = [string[]]@('tools/LinuxAotPackageSmoke/Program.cs', 'tools/LinuxAotPackageSmoke/consumer.csproj.in',
        'scripts/Initialize-LocalLinux51AotProducer.sh', 'scripts/Build-LocalLinux51AotConsumer.ps1',
        'scripts/Initialize-LocalLinux51AotConsumer.sh', 'scripts/Invoke-LocalLinux51AotConsumer.sh',
        'scripts/LocalLinux51Aot.Common.ps1', 'scripts/Measure-LocalLinux51AotEvidence.ps1',
        'scripts/Test-LocalLinux51AotEvidence.ps1', 'scripts/LocalLinux51.Common.ps1',
        'packaging/runtime/opencv-5.1.0-local-linux-aot-evidence.schema.json',
        'tests/OpenCvSharp.Tests/Dnn/Fixtures/identity-opset13.onnx.base64')
    [Array]::Sort($paths, [StringComparer]::Ordinal)
    foreach ($path in $paths) {
        $text = [IO.File]::ReadAllText((Join-Path $RepositoryRoot $path)).Replace("`r`n", "`n").Replace("`r", "`n").TrimEnd() + "`n"
        [ordered]@{ path = $path; bytes = [Text.Encoding]::UTF8.GetByteCount($text); sha256 = Get-LocalLinux51TextHash $text }
    }
}

function Get-LocalLinux51AotManifest {
    param([string]$Directory)
    foreach ($file in Get-ChildItem $Directory -Recurse -File | Sort-Object FullName -CaseSensitive) {
        [ordered]@{ path = [IO.Path]::GetRelativePath($Directory, $file.FullName).Replace('\', '/'); bytes = $file.Length; sha256 = (Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
    }
}

function Get-LocalLinux51AotBlob {
    param([string]$Directory, [string]$Name)
    $file = Join-Path $Directory $Name
    [ordered]@{ name = $Name; text = [IO.File]::ReadAllText($file); bytes = (Get-Item $file).Length; sha256 = (Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant() }
}

function Get-LocalLinux51AotIdentity {
    param([string]$Container)
    $identity = Get-LocalLinux51DockerIdentity $Container
    $image = @((& docker image inspect $identity.imageId) | ConvertFrom-Json)[0]
    if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect AOT image.' }
    $state = @((& docker inspect $Container) | ConvertFrom-Json)[0].State
    if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect AOT container state.' }
    $identity.imageArchitecture = $image.Architecture; $identity.imageOs = $image.Os
    $identity.oomKilled = $state.OOMKilled
    $identity
}
