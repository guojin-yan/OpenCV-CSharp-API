. (Join-Path $PSScriptRoot 'LocalLinux51.Common.ps1')

function Get-LocalLinux51DistroSourceInputs {
    param([string]$RepositoryRoot)
    $paths = [string[]]@('Directory.Build.props', 'scripts/PackageVersion.ps1',
        'scripts/Test-GitHubPackConsumerRestoreSurface.ps1', 'scripts/Test-GenericLinuxPreviewConsumer.ps1',
        'scripts/Initialize-LocalLinux51DistroConsumer.sh', 'scripts/Invoke-LocalLinux51DistroConsumer.ps1',
        'scripts/LocalLinux51.Common.ps1', 'scripts/LocalLinux51Distro.Common.ps1', 'scripts/Measure-LocalLinux51DistroEvidence.ps1',
        'packaging/runtime/runtime-distro-rid-graph.json', 'packaging/runtime/runtime-generic-linux-5.1-local-consumer-matrix.json',
        'packaging/runtime/runtime-generic-linux-preview-matrix.schema.json')
    [Array]::Sort($paths, [StringComparer]::Ordinal)
    foreach ($path in $paths) {
        $text = [IO.File]::ReadAllText((Join-Path $RepositoryRoot $path)).Replace("`r`n", "`n").Replace("`r", "`n").TrimEnd() + "`n"
        [ordered]@{ path = $path; bytes = [Text.Encoding]::UTF8.GetByteCount($text); sha256 = Get-LocalLinux51TextHash $text }
    }
}

function Get-LocalLinux51DistroKitManifest {
    param([string]$KitRoot)
    $files = @(Get-ChildItem -LiteralPath $KitRoot -Recurse -File | Sort-Object FullName -CaseSensitive)
    foreach ($file in $files) {
        [ordered]@{
            path = [IO.Path]::GetRelativePath($KitRoot, $file.FullName).Replace('\', '/')
            bytes = $file.Length
            sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        }
    }
}

function Get-LocalLinux51DistroConsumerRecord {
    param([string]$RepositoryRoot, [string]$OutputRoot, [string]$Container, [string]$ConsumerRid)
    $identity = Get-LocalLinux51DockerIdentity $Container
    $image = @((& docker image inspect $identity.imageId) | ConvertFrom-Json)[0]
    if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect consumer image architecture.' }
    $identity.imageArchitecture = $image.Architecture
    $identity.imageOs = $image.Os
    $bootstrap = [IO.File]::ReadAllText((Join-Path $OutputRoot 'bootstrap.log'))
    $driver = [IO.File]::ReadAllText((Join-Path $OutputRoot 'consumer-driver.log'))
    $preflight = [IO.File]::ReadAllText((Join-Path $OutputRoot "consumer-preflight-$ConsumerRid.json"))
    $profiles = foreach ($name in @('full', 'mini')) {
        $evidence = [IO.File]::ReadAllText((Join-Path $OutputRoot "generic-linux-preview-consumer-evidence-$ConsumerRid-$name.json"))
        $output = [IO.File]::ReadAllText((Join-Path $OutputRoot "consumer-$ConsumerRid-$name.log"))
        [ordered]@{
            name = $name; consumerEvidenceJson = $evidence; consumerEvidenceSha256 = Get-LocalLinux51TextHash $evidence
            consumerOutput = $output; consumerOutputSha256 = Get-LocalLinux51TextHash $output
        }
    }
    $packages = foreach ($dir in @('nupkg-managed', 'nupkg-linux-x64-preview-full', 'nupkg-linux-x64-preview-mini')) {
        $files = @(Get-ChildItem -LiteralPath (Join-Path $OutputRoot $dir) -Filter '*.nupkg' -File)
        if ($files.Count -ne 1) { throw 'Expected exactly one package in each independent consumer artifact folder.' }
        [ordered]@{
            artifactPath = [IO.Path]::GetRelativePath($RepositoryRoot, $files[0].FullName).Replace('\', '/')
            bytes = $files[0].Length; sha256 = (Get-FileHash -LiteralPath $files[0].FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        }
    }
    [ordered]@{
        rid = $ConsumerRid; identity = $identity
        outputDirectory = [IO.Path]::GetRelativePath($RepositoryRoot, $OutputRoot).Replace('\', '/')
        bootstrapOutput = $bootstrap; bootstrapOutputSha256 = Get-LocalLinux51TextHash $bootstrap
        driverOutput = $driver; driverOutputSha256 = Get-LocalLinux51TextHash $driver
        preflightJson = $preflight; preflightSha256 = Get-LocalLinux51TextHash $preflight
        packages = @($packages); profiles = @($profiles)
    }
}
