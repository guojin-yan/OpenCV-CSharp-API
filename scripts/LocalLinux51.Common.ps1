function Get-LocalLinux51Hash {
    param([byte[]]$Bytes)
    [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
}
function Get-LocalLinux51TextHash {
    param([AllowEmptyString()][string]$Text)
    Get-LocalLinux51Hash ([Text.UTF8Encoding]::new($false).GetBytes($Text))
}
function Get-LocalLinux51SourceSet {
    param([string]$RepositoryRoot)
    $scopes = @('Directory.Build.props', 'README.md', 'LICENSE', 'nuget/logo.jpg', 'src/OpenCvSharp.Native',
        'packaging/runtime/runtime-generic-linux-preview-matrix.json',
        'packaging/runtime/runtime-package-matrix.json', 'packaging/runtime/runtime-distro-rid-graph.json',
        'packaging/runtime/patches/opencv-5.1.0-photo-ccm-instance-color-space.patch',
        'packaging/runtime/JYPPX.OpenCV.runtime/JYPPX.OpenCV.runtime.csproj',
        'packaging/runtime/JYPPX.OpenCV.runtime/README.md',
        'packaging/runtime/JYPPX.OpenCV.runtime/buildTransitive', 'scripts/Build-OpenCV.ps1',
        'scripts/Apply-OpenCvSourcePatches.ps1', 'scripts/New-RuntimeInputArtifact.ps1',
        'scripts/New-GenericLinuxPreviewPackage.ps1', 'scripts/Stage-Runtime.ps1',
        'scripts/Normalize-NuGetPackageDeterminism.ps1', 'scripts/Test-RuntimeReleaseCandidatePreflight.ps1',
        'scripts/PackageVersion.ps1', 'scripts/Get-NativeRuntimeProfileEvidence.ps1',
        'scripts/Test-GitHubPackConsumerRestoreSurface.ps1', 'scripts/Test-GenericLinuxPreviewConsumer.ps1',
        'scripts/Audit-Linux51Runtime.py', 'scripts/Build-LocalLinux51Preview.ps1',
        'scripts/LocalLinux51.Common.ps1', 'scripts/Measure-LocalLinux51Evidence.ps1',
        'scripts/Invoke-LocalLinux51Consumer.ps1')
    $paths = [string[]]@(& git -C $RepositoryRoot ls-files --cached --others --exclude-standard -- @scopes)
    if ($LASTEXITCODE -ne 0 -or $paths.Count -eq 0) { throw 'Cannot enumerate Linux source inputs.' }
    [Array]::Sort($paths, [StringComparer]::Ordinal)
    $rows = foreach ($path in $paths) {
        $bytes = if ([IO.Path]::GetExtension($path) -ceq '.jpg') { [IO.File]::ReadAllBytes((Join-Path $RepositoryRoot $path)) }
            else {
                $text = [IO.File]::ReadAllText((Join-Path $RepositoryRoot $path))
                [Text.Encoding]::UTF8.GetBytes((($text -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n")
            }
        "$path|$($bytes.Length)|$(Get-LocalLinux51Hash $bytes)"
    }
    [ordered]@{ paths = $scopes; fileCount = $paths.Count; sha256 = Get-LocalLinux51TextHash (($rows -join "`n") + "`n") }
}
function Get-LocalLinux51PackageManifest {
    param([string]$Path)
    $zip = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entries = [object[]]@($zip.Entries | Where-Object { -not $_.FullName.EndsWith('/') })
        $names = [string[]]@($entries.FullName)
        [Array]::Sort[string, object]($names, $entries, [StringComparer]::Ordinal)
        if (@($names | Select-Object -Unique).Count -ne $names.Count) { throw 'Duplicate NuGet archive paths.' }
        foreach ($entry in $entries) {
            $stream = $entry.Open()
            $memory = [IO.MemoryStream]::new()
            try { $stream.CopyTo($memory); $bytes = $memory.ToArray() }
            finally { $memory.Dispose(); $stream.Dispose() }
            [ordered]@{ path = $entry.FullName; bytes = $bytes.Length; sha256 = Get-LocalLinux51Hash $bytes }
        }
    }
    finally { $zip.Dispose() }
}
function Get-LocalLinux51DockerIdentity {
    param([string]$Container)
    $json = & docker inspect $Container
    if ($LASTEXITCODE -ne 0) { throw "Cannot inspect $Container" }
    $d = @($json | ConvertFrom-Json)[0]
    [ordered]@{
        id = $d.Id; name = $d.Name.TrimStart('/'); image = $d.Config.Image; imageId = $d.Image
        platform = $d.Platform; nanoCpus = $d.HostConfig.NanoCpus; memoryBytes = $d.HostConfig.Memory
        mounts = @($d.Mounts | ForEach-Object { [ordered]@{ type = $_.Type; source = $_.Source; destination = $_.Destination; writable = $_.RW } })
    }
}
