. (Join-Path $PSScriptRoot 'ModernApi51Evidence.Common.ps1')

function ConvertFrom-LocalPackageSbom51Json {
    param([string]$Json)
    $arguments = @{ InputObject = $Json }
    if ((Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')) { $arguments.DateKind = 'String' }
    $value = ConvertFrom-Json @arguments
    if ($value.PSObject.Properties['created'] -and $value.created -is [DateTime]) { $value.created = $value.created.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") }
    if ($value.PSObject.Properties['creationInfo'] -and $value.creationInfo.created -is [DateTime]) { $value.creationInfo.created = $value.creationInfo.created.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") }
    return $value
}

function Get-LocalPackageSbom51SourceSet {
    param([string]$RepositoryRoot)
    $scopes = @('Directory.Build.props', 'README.md', 'LICENSE', 'nuget/logo.jpg', 'src/OpenCvSharp',
        'packaging/runtime/JYPPX.OpenCV.runtime', 'packaging/runtime/runtime-distro-rid-graph.json',
        'packaging/runtime/runtime-package-matrix.json', 'scripts/PackageVersion.ps1', 'scripts/Pack-Managed.ps1',
        'scripts/Pack-Runtime.ps1', 'scripts/Stage-Runtime.ps1', 'scripts/Normalize-NuGetPackageDeterminism.ps1', 'scripts/New-ReleasePackageSbom.ps1',
        'scripts/Test-GitHubPackConsumerRestoreSurface.ps1', 'scripts/Test-PackagedWindowsRuntimePeClosure.ps1',
        'scripts/Test-WindowsRuntimePeClosure.ps1', 'scripts/ModernApi51Evidence.Common.ps1',
        'scripts/LocalPackageSbom51.Common.ps1', 'scripts/Measure-LocalPackageSbom51Evidence.ps1')
    $paths = [string[]]@(& git -C $RepositoryRoot ls-files --cached --others --exclude-standard -- @scopes)
    if ($LASTEXITCODE -ne 0 -or $paths.Count -eq 0) { throw 'Cannot enumerate local package SBOM source inputs.' }
    [Array]::Sort($paths, [StringComparer]::Ordinal)
    $rows = foreach ($path in $paths) {
        $absolute = Join-Path $RepositoryRoot $path
        $bytes = if ([IO.Path]::GetExtension($path) -ceq '.jpg') { [IO.File]::ReadAllBytes($absolute) } else {
            $text = [IO.File]::ReadAllText($absolute)
            [Text.UTF8Encoding]::new($false).GetBytes((($text -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n")
        }
        $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        "$path|$($bytes.Length)|$hash"
    }
    $manifest = [Text.UTF8Encoding]::new($false).GetBytes(($rows -join "`n") + "`n")
    return [ordered]@{ paths = $scopes; fileCount = $paths.Count; sha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($manifest)).ToLowerInvariant() }
}

function Get-LocalPackageSbom51Manifest {
    param([string]$PackagePath)
    $archive = [IO.Compression.ZipFile]::OpenRead($PackagePath)
    try {
        $names = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        $rows = foreach ($entry in $archive.Entries) {
            $path = $entry.FullName
            if ($path.EndsWith('/')) { continue }
            if ($path -notmatch '^[A-Za-z0-9_\[\]./-]+$' -or $path.StartsWith('/') -or
                @($path.Split('/') | Where-Object { $_ -in @('', '.', '..') }).Count -gt 0 -or
                -not $names.Add($path) -or $path -ieq '.signature.p7s' -or
                $entry.LastWriteTime.DateTime -ne [DateTime]::new(2000, 1, 1)) { throw "Invalid or non-normalized package entry: $path" }
            $stream = $entry.Open()
            $memory = [IO.MemoryStream]::new()
            try { $stream.CopyTo($memory); $bytes = $memory.ToArray() } finally { $memory.Dispose(); $stream.Dispose() }
            [pscustomobject][ordered]@{
                path = $path; bytes = [int64]$bytes.Length
                sha1 = [Convert]::ToHexString([Security.Cryptography.SHA1]::HashData($bytes)).ToLowerInvariant()
                sha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
            }
        }
        $sorted = [object[]]@($rows)
        $keys = [string[]]@($sorted.path)
        [Array]::Sort[string, object]($keys, $sorted, [StringComparer]::Ordinal)
        return $sorted
    }
    finally { $archive.Dispose() }
}

function Assert-LocalPackageSbom51Document {
    param([object]$Document, [object]$Package, [string]$SourceCommit, [string]$Created)
    $repoUrl = 'https://github.com/guojin-yan/OpenCV-CSharp-API'
    $mainId = 'SPDXRef-Package-' + $Package.packageId
    $expectedNamespace = "$repoUrl/sbom/$($Package.packageId)/5.1.0/$($Package.sha256)"
    if ($Document.spdxVersion -cne 'SPDX-2.3' -or $Document.SPDXID -cne 'SPDXRef-DOCUMENT' -or
        $Document.dataLicense -cne 'CC0-1.0' -or $Document.documentNamespace -cne $expectedNamespace -or
        $Document.name -cne "$($Package.packageId)-5.1.0-sbom" -or $Document.creationInfo.created -cne $Created -or
        (@($Document.creationInfo.creators) -join '|') -cne 'Tool: OpenCV-CSharp-API/scripts/New-ReleasePackageSbom.ps1@1.0.0' -or
        $Document.creationInfo.comment -cne "Deterministic release SBOM for normalized package SHA256 $($Package.sha256) at source commit $SourceCommit.") { throw 'Local SPDX document identity/source/timestamp drifted.' }
    $packages = @($Document.packages)
    $main = @($packages | Where-Object SPDXID -CEQ $mainId)
    $upstream = @($packages | Where-Object SPDXID -CEQ 'SPDXRef-Package-OpenCV')
    if ($packages.Count -ne 2 -or $main.Count -ne 1 -or $upstream.Count -ne 1) { throw 'Local SPDX package inventory drifted.' }
    $main = $main[0]
    $upstream = $upstream[0]
    if ($main.name -cne $Package.packageId -or $main.versionInfo -cne '5.1.0' -or $main.packageFileName -cne $Package.fileName -or
        $main.sourceInfo -cne "$repoUrl commit $SourceCommit; OpenCV 5.1.0." -or $main.homepage -cne $repoUrl -or
        -not $main.filesAnalyzed -or $main.downloadLocation -cne 'NOASSERTION' -or
        $main.licenseDeclared -cne 'Apache-2.0' -or $main.licenseConcluded -cne 'Apache-2.0' -or
        @($main.checksums).Count -ne 1 -or $main.checksums[0].algorithm -cne 'SHA256' -or
        $main.checksums[0].checksumValue -cne $Package.sha256 -or @($main.externalRefs).Count -ne 1 -or
        $main.externalRefs[0].referenceLocator -cne "pkg:nuget/$($Package.packageId)@5.1.0" -or
        $upstream.name -cne 'OpenCV' -or $upstream.versionInfo -cne '5.1.0' -or $upstream.filesAnalyzed -or
        $upstream.downloadLocation -cne 'https://github.com/opencv/opencv/archive/refs/tags/5.1.0.tar.gz') { throw 'Local SPDX package checksum, provenance or upstream version drifted.' }
    $entries = @($Package.entries)
    $files = @($Document.files)
    if ($entries.Count -eq 0 -or $files.Count -ne $entries.Count -or $Package.entryCount -ne $entries.Count) { throw 'Local SPDX file closure count drifted.' }
    $hashes = [string[]]@($entries.sha1)
    [Array]::Sort($hashes, [StringComparer]::Ordinal)
    $verificationCode = [Convert]::ToHexString([Security.Cryptography.SHA1]::HashData([Text.Encoding]::ASCII.GetBytes([string]::Concat($hashes)))).ToLowerInvariant()
    if ($main.packageVerificationCode.packageVerificationCodeValue -cne $verificationCode) { throw 'Local SPDX package verification code drifted.' }
    $relations = [Collections.Generic.List[string]]::new()
    $relations.Add("SPDXRef-DOCUMENT|DESCRIBES|$mainId")
    $relations.Add("$mainId|DEPENDS_ON|SPDXRef-Package-OpenCV")
    $entryNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    for ($index = 0; $index -lt $entries.Count; $index++) {
        $entry = $entries[$index]
        $file = $files[$index]
        $id = 'SPDXRef-File-{0:D4}-{1}' -f ($index + 1), ([string]$entry.sha256).Substring(0, 12)
        if (-not $entryNames.Add([string]$entry.path) -or $file.fileName -cne ('./' + $entry.path) -or $file.SPDXID -cne $id -or
            @($file.checksums).Count -ne 2 -or $file.checksums[0].algorithm -cne 'SHA1' -or $file.checksums[0].checksumValue -cne $entry.sha1 -or
            $file.checksums[1].algorithm -cne 'SHA256' -or $file.checksums[1].checksumValue -cne $entry.sha256 -or
            $file.licenseConcluded -cne 'NOASSERTION' -or $file.copyrightText -cne 'NOASSERTION') { throw "Local SPDX file checksum/identity drifted: $($entry.path)" }
        $relations.Add("$mainId|CONTAINS|$id")
    }
    $actualRelations = @($Document.relationships | ForEach-Object { "$($_.spdxElementId)|$($_.relationshipType)|$($_.relatedSpdxElement)" } | Sort-Object)
    if (($actualRelations -join "`n") -cne (@($relations | Sort-Object) -join "`n")) { throw 'Local SPDX relationship closure drifted.' }
}

function Assert-LocalPackageSbom51Payload {
    param([object]$Package, [object]$Bindings)
    $expectedId = switch ($Package.role) {
        managed { 'JYPPX.OpenCV.CSharp.API' }
        full { 'JYPPX.OpenCV.runtime.win-x64' }
        mini { 'JYPPX.OpenCV.runtime.win-x64.mini' }
        default { throw 'Unexpected local SBOM package role.' }
    }
    if ($Package.packageId -cne $expectedId -or $Package.fileName -cne "$expectedId.5.1.0.nupkg") { throw 'Local SBOM package identity drifted.' }
    $entries = @($Package.entries)
    $native = @($entries | Where-Object path -Like 'runtimes/*')
    if ($Package.role -ceq 'managed') {
        if ($native.Count -ne 0) { throw 'Managed SBOM package contains native assets.' }
        $frameworks = @('net46','net461','net462','net47','net471','net472','net48','net481','netcoreapp3.1','net5.0','net6.0','net7.0','net8.0','net9.0','net10.0')
        $expected = @($frameworks | ForEach-Object { "lib/$_/JYPPX.OpenCV.CSharp.API.dll"; "lib/$_/JYPPX.OpenCV.CSharp.API.xml" } | Sort-Object)
        if ((@($entries | Where-Object path -Like 'lib/*' | Select-Object -ExpandProperty path | Sort-Object) -join '|') -cne ($expected -join '|')) { throw 'Managed local SBOM package must cover all 15 TFM DLL/XML pairs.' }
    }
    else {
        $expectedPackage = @($Bindings.packages | Where-Object profile -CEQ $Package.role)[0]
        $expected = @($expectedPackage.payload | ForEach-Object { "runtimes/win-x64/native/$($_.name)|$($_.bytes)|$($_.sha256)" } | Sort-Object)
        $actual = @($native | ForEach-Object { "$($_.path)|$($_.bytes)|$($_.sha256)" } | Sort-Object)
        if (($actual -join "`n") -cne ($expected -join "`n")) { throw 'Local SBOM native payload differs from the verified 5.1 package-consumer DLL bytes.' }
        if (@($entries | Where-Object path -Like 'licenses/*').Count -lt 3 -or
            @($entries | Where-Object path -CEQ 'build/JYPPX.OpenCV.runtime.provenance.json').Count -ne 1) { throw 'Local SBOM runtime package license/provenance closure is incomplete.' }
    }
}
