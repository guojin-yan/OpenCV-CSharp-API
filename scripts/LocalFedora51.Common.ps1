. (Join-Path $PSScriptRoot 'LocalLinux51Module.Common.ps1')

function Get-LocalFedora51SourceInputs([string]$RepositoryRoot) {
    $scopes = @('Directory.Build.props', 'LICENSE', 'nuget/logo.jpg', 'src/OpenCvSharp.Native',
        'packaging/runtime/runtime-lifecycle-refresh-matrix.json', 'packaging/runtime/runtime-package-matrix.json',
        'packaging/runtime/runtime-distro-rid-graph.json', 'packaging/runtime/runtime-fedora-5.1-local-consumer-rid-graph.json',
        'packaging/runtime/patches/opencv-5.1.0-photo-ccm-instance-color-space.patch',
        'packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj',
        'tools/LinuxPackageModuleSmoke/Program.cs', 'tools/LinuxPackageModuleSmoke/fedora-consumer.csproj.in',
        'tests/OpenCvSharp.Tests/Dnn/Fixtures/identity-opset13.onnx.base64',
        'scripts/Build-OpenCV.ps1', 'scripts/Apply-OpenCvSourcePatches.ps1', 'scripts/Stage-Runtime.ps1',
        'scripts/Pack-Runtime.ps1', 'scripts/PackageVersion.ps1', 'scripts/Normalize-NuGetPackageDeterminism.ps1',
        'scripts/Initialize-LocalFedora51Producer.sh', 'scripts/Build-LocalFedora51Candidate.py',
        'scripts/Audit-LocalFedora51Runtime.py', 'scripts/Initialize-LocalLinux51ModuleConsumer.sh',
        'scripts/Invoke-LocalFedora51Consumer.ps1', 'scripts/LocalFedora51.Common.ps1',
        'scripts/Measure-LocalFedora51Evidence.ps1', 'scripts/Test-LocalFedora51Evidence.ps1',
        'scripts/New-LocalFedora51Packages.ps1', 'scripts/LocalLinux51Module.Common.ps1', 'scripts/LocalLinux51.Common.ps1',
        'packaging/runtime/opencv-5.1.0-local-fedora44-evidence.schema.json')
    $paths = [string[]]@(& git -C $RepositoryRoot ls-files --cached --others --exclude-standard -- @scopes | Sort-Object -Unique -CaseSensitive)
    if ($LASTEXITCODE) { throw 'Cannot enumerate Fedora source inputs.' }
    [Array]::Sort($paths, [StringComparer]::Ordinal)
    foreach ($path in $paths) {
        $absolute = Join-Path $RepositoryRoot $path
        $bytes = if ($path.EndsWith('.jpg')) { [IO.File]::ReadAllBytes($absolute) }
            else { [Text.Encoding]::UTF8.GetBytes([IO.File]::ReadAllText($absolute).Replace("`r`n", "`n").Replace("`r", "`n").TrimEnd() + "`n") }
        [ordered]@{ path = $path; bytes = $bytes.Length; sha256 = Get-LocalLinux51Hash $bytes }
    }
}

function Get-LocalFedora51File([string]$RepositoryRoot, [string]$Path) {
    [ordered]@{ path = [IO.Path]::GetRelativePath($RepositoryRoot, $Path).Replace('\', '/');
        bytes = (Get-Item $Path).Length; sha256 = (Get-FileHash $Path).Hash.ToLowerInvariant() }
}

function Get-LocalFedora51Identity([string]$Container) {
    $identity = Get-LocalLinux51DockerIdentity $Container
    $image = @((& docker image inspect $identity.imageId) | ConvertFrom-Json)[0]
    if ($LASTEXITCODE) { throw 'Cannot inspect Fedora container image.' }
    $identity.imageArchitecture = $image.Architecture; $identity.imageOs = $image.Os
    $state = @((& docker inspect $Container) | ConvertFrom-Json)[0].State
    $identity.oomKilled = $state.OOMKilled
    $identity
}
