. (Join-Path $PSScriptRoot 'LocalLinux51.Common.ps1')

function Get-Headless51Specs {
    @(
        [pscustomobject]@{ rid = 'ubuntu.22.04-x64'; libc = 'glibc 2.35'; image = 'ubuntu:22.04@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982' },
        [pscustomobject]@{ rid = 'ubuntu.24.04-x64'; libc = 'glibc 2.39'; image = 'ubuntu:24.04@sha256:562111b88caa79c71391c0c4d3c2114a7d5c35915c20561fd893cb1e07929e6b' },
        [pscustomobject]@{ rid = 'debian.12-x64'; libc = 'glibc 2.36'; image = 'debian:12@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc' }
    )
}

function Get-Headless51Inputs {
    param([string]$RepositoryRoot)
    $scopes = @('src/OpenCvSharp.Native', 'tools/LinuxHeadlessPackageSmoke',
        'scripts/Build-Linux51HeadlessCandidate.py', 'scripts/Audit-Linux51Runtime.py',
        'scripts/Generate-NativeAbiManifest.ps1', 'scripts/Test-NativeAbiExports.ps1',
        'scripts/Initialize-Linux51HeadlessConsumer.sh', 'scripts/Invoke-Linux51HeadlessConsumer.ps1',
        'scripts/Linux51Headless.Common.ps1', 'scripts/LocalLinux51.Common.ps1',
        'scripts/Measure-Linux51HeadlessEvidence.ps1', 'scripts/Test-Linux51HeadlessEvidence.ps1',
        'packaging/runtime/runtime-headless-profile-contract.json',
        'packaging/runtime/runtime-generic-linux-preview-matrix.json',
        'packaging/runtime/runtime-package-matrix.json',
        'packaging/runtime/opencv-5.1.0-local-linux-headless-evidence.schema.json',
        'tests/OpenCvSharp.Tests/Dnn/Fixtures/identity-opset13.onnx.base64')
    $paths = [string[]]@(& git -C $RepositoryRoot ls-files --cached --others --exclude-standard -- @scopes)
    if ($LASTEXITCODE -ne 0 -or $paths.Count -eq 0) { throw 'Cannot enumerate headless source inputs.' }
    [Array]::Sort($paths, [StringComparer]::Ordinal)
    foreach ($path in $paths) {
        $text = [IO.File]::ReadAllText((Join-Path $RepositoryRoot $path)).Replace("`r`n", "`n").Replace("`r", "`n").TrimEnd() + "`n"
        [ordered]@{ path = $path; bytes = [Text.Encoding]::UTF8.GetByteCount($text); sha256 = Get-LocalLinux51TextHash $text }
    }
}

function Get-Headless51Blob {
    param([string]$Directory, [string]$Name)
    $path = Join-Path $Directory $Name
    [ordered]@{ name = $Name; bytes = (Get-Item $path).Length; sha256 = (Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant(); text = [IO.File]::ReadAllText($path) }
}

function Get-Headless51Manifest {
    param([string]$Directory)
    foreach ($file in Get-ChildItem $Directory -File -Recurse | Sort-Object FullName -CaseSensitive) {
        [ordered]@{ path = [IO.Path]::GetRelativePath($Directory, $file.FullName).Replace('\', '/'); bytes = $file.Length; sha256 = (Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
    }
}

function Get-Headless51Identity {
    param([string]$Container)
    $identity = Get-LocalLinux51DockerIdentity $Container
    $inspection = @((& docker inspect $Container) | ConvertFrom-Json)[0]
    if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect headless container.' }
    $image = @((& docker image inspect $identity.imageId) | ConvertFrom-Json)[0]
    if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect headless image.' }
    $identity.imageArchitecture = $image.Architecture; $identity.imageOs = $image.Os
    $identity.oomKilled = $inspection.State.OOMKilled
    $identity
}
