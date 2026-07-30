param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workflowRoot = Join-Path $repo ".github/workflows"
$aggregatePath = Join-Path $repo "scripts/Test-ProjectInvariants.ps1"
$expectedKeyUrl = "https://packages.microsoft.com/keys/microsoft.asc"
$expectedKeySha256 = "2fa9c05d591a1582a9aba276272478c262e95ad00acf60eaee1644d93941e3c6"
$expectedKeyFingerprint = "BC528686B50D79E339D3721CEB3E94ADBE1229CF"
$expectedKeyUid = "Microsoft (Release signing) <gpgsecurity@microsoft.com>"
$expectedLocalRpmKey = "/etc/pki/rpm-gpg/RPM-GPG-KEY-Microsoft"

function Add-Violation {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory)][string]$Path,
        [int]$Line = 0,
        [Parameter(Mandatory)][string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Line = $Line
        Issue = $Issue
        Text = $Text.Trim()
    })
}

function Get-RelativePath {
    param([Parameter(Mandatory)][string]$Path)

    return ([System.IO.Path]::GetRelativePath($repo, $Path)) -replace "\\", "/"
}

function Get-TextCount {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$Needle
    )

    return [regex]::Matches($Text, [regex]::Escape($Needle)).Count
}

function Get-LineNumber {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][int]$Index
    )

    if ($Index -lt 0) {
        return 0
    }
    return [regex]::Matches($Text.Substring(0, $Index), "\n").Count + 1
}

function Get-DelimitedContext {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$StartNeedle,
        [Parameter(Mandatory)][string]$EndNeedle,
        [int]$SearchStart = 0,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Name
    )

    $start = $Text.IndexOf($StartNeedle, $SearchStart, [System.StringComparison]::Ordinal)
    if ($start -lt 0) {
        return $null
    }
    $end = $Text.IndexOf($EndNeedle, $start + $StartNeedle.Length, [System.StringComparison]::Ordinal)
    if ($end -lt 0) {
        return $null
    }
    $end += $EndNeedle.Length
    return [pscustomobject]@{
        Name = $Name
        Path = $Path
        Line = Get-LineNumber -Text $Text -Index $start
        Text = $Text.Substring($start, $end - $start)
    }
}

function Get-JobContext {
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string[]]$Lines,
        [Parameter(Mandatory)][string]$JobName,
        [Parameter(Mandatory)][string]$Path
    )

    $start = -1
    for ($index = 0; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -ceq "  $JobName`:") {
            $start = $index
            break
        }
    }
    if ($start -lt 0) {
        return $null
    }

    $end = $Lines.Count
    for ($index = $start + 1; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -match '^  [A-Za-z0-9_-]+:\s*$') {
            $end = $index
            break
        }
    }

    return [pscustomobject]@{
        Name = $JobName
        Path = $Path
        Line = $start + 1
        Text = ($Lines[$start..($end - 1)] -join "`n")
    }
}

function Assert-ExactToken {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory)][object]$Context,
        [Parameter(Mandatory)][string]$Needle,
        [int]$ExpectedCount = 1,
        [Parameter(Mandatory)][string]$Issue
    )

    $count = Get-TextCount -Text $Context.Text -Needle $Needle
    if ($count -ne $ExpectedCount) {
        Add-Violation -Violations $Violations -Path $Context.Path -Line $Context.Line -Issue $Issue -Text "$($Context.Name): '$Needle' count=$count expected=$ExpectedCount"
    }
}

function Assert-OrderedTokens {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory)][object]$Context,
        [Parameter(Mandatory)][string[]]$Tokens,
        [Parameter(Mandatory)][string]$Issue
    )

    $previous = -1
    foreach ($token in $Tokens) {
        $index = $Context.Text.IndexOf($token, $previous + 1, [System.StringComparison]::Ordinal)
        if ($index -lt 0 -or $index -le $previous) {
            Add-Violation -Violations $Violations -Path $Context.Path -Line $Context.Line -Issue $Issue -Text "$($Context.Name): missing/out-of-order '$token'"
            return
        }
        $previous = $index
    }
}

$violations = [System.Collections.Generic.List[object]]::new()
$expectedWorkflowNames = @(
    "build-managed.yml",
    "build-native.yml",
    "docs.yml",
    "pack.yml",
    "runtime-input.yml"
)
$workflowFiles = @(
    Get-ChildItem -LiteralPath $workflowRoot -File |
        Where-Object { $_.Extension -in @(".yml", ".yaml") } |
        Sort-Object Name
)
$workflowDifference = @(
    Compare-Object -ReferenceObject $expectedWorkflowNames -DifferenceObject @($workflowFiles.Name)
)
if ($workflowDifference.Count -gt 0) {
    Add-Violation -Violations $violations -Path ".github/workflows" -Issue "Microsoft feed trust guard requires the exact five-workflow set" -Text (($workflowDifference | ForEach-Object { "$($_.SideIndicator)$($_.InputObject)" }) -join ", ")
}

$runtimePath = Join-Path $workflowRoot "runtime-input.yml"
$packPath = Join-Path $workflowRoot "pack.yml"
$runtimeRelativePath = Get-RelativePath -Path $runtimePath
$packRelativePath = Get-RelativePath -Path $packPath
$runtimeText = [System.IO.File]::ReadAllText($runtimePath)
$packText = [System.IO.File]::ReadAllText($packPath)
$packLines = [System.IO.File]::ReadAllLines($packPath)
$controlledText = $runtimeText + "`n" + $packText

foreach ($workflowFile in $workflowFiles) {
    $relativePath = Get-RelativePath -Path $workflowFile.FullName
    $text = [System.IO.File]::ReadAllText($workflowFile.FullName)
    if ($workflowFile.Name -notin @("runtime-input.yml", "pack.yml") -and
        $text.IndexOf("packages.microsoft.com", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        Add-Violation -Violations $violations -Path $relativePath -Issue "Microsoft package-feed trust bootstrap is not allowed in this workflow"
    }
}

$runtimeProducer = Get-DelimitedContext `
    -Text $runtimeText `
    -StartNeedle "      - name: Run distro container producer" `
    -EndNeedle '              container_distro="${ID:-debian}"' `
    -Path $runtimeRelativePath `
    -Name "runtime-input/produce-container"
if ($null -eq $runtimeProducer) {
    Add-Violation -Violations $violations -Path $runtimeRelativePath -Issue "Runtime container producer trust bootstrap block was not found"
}
else {
    Assert-ExactToken -Violations $violations -Context $runtimeProducer -Needle "MICROSOFT_SIGNING_KEY_SHA256: $expectedKeySha256" -Issue "Runtime producer must bind the exact signing-key SHA256 once"
    Assert-ExactToken -Violations $violations -Context $runtimeProducer -Needle "MICROSOFT_SIGNING_KEY_FINGERPRINT: $expectedKeyFingerprint" -Issue "Runtime producer must bind the exact full signing-key fingerprint once"
    Assert-ExactToken -Violations $violations -Context $runtimeProducer -Needle '-e MICROSOFT_SIGNING_KEY_SHA256="$MICROSOFT_SIGNING_KEY_SHA256"' -Issue "Runtime producer must pass the exact key hash into its container"
    Assert-ExactToken -Violations $violations -Context $runtimeProducer -Needle '-e MICROSOFT_SIGNING_KEY_FINGERPRINT="$MICROSOFT_SIGNING_KEY_FINGERPRINT"' -Issue "Runtime producer must pass the exact key fingerprint into its container"
}

$runtimeApt = Get-DelimitedContext `
    -Text $runtimeText `
    -StartNeedle '                    test "$package_architecture" = "amd64"' `
    -EndNeedle '                    apt-get install -y --no-install-recommends "powershell=$POWERSHELL_DEBIAN_PACKAGE_VERSION"' `
    -Path $runtimeRelativePath `
    -Name "runtime-input/debian-amd64"
$rpmBranchStart = $runtimeText.IndexOf('                fedora|rhel|rocky)', [System.StringComparison]::Ordinal)
$runtimeRpm = Get-DelimitedContext `
    -Text $runtimeText `
    -StartNeedle '                  microsoft_key_url="https://packages.microsoft.com/keys/microsoft.asc"' `
    -EndNeedle '                  dnf install -y "$POWERSHELL_RPM_PACKAGE_NEVRA"' `
    -SearchStart $rpmBranchStart `
    -Path $runtimeRelativePath `
    -Name "runtime-input/fedora-rocky-rhel"

$packContexts = [ordered]@{
    "pack/debian.12-x64" = Get-JobContext -Lines $packLines -JobName "verify-targeted-real-debian" -Path $packRelativePath
    "pack/fedora.40-x64" = Get-JobContext -Lines $packLines -JobName "verify-targeted-real-fedora" -Path $packRelativePath
    "pack/rocky.9-x64" = Get-JobContext -Lines $packLines -JobName "verify-targeted-real-rocky" -Path $packRelativePath
    "pack/rhel.9-x64" = Get-JobContext -Lines $packLines -JobName "verify-targeted-real-rhel" -Path $packRelativePath
}
foreach ($name in @($packContexts.Keys)) {
    if ($null -ne $packContexts[$name]) {
        $packContexts[$name].Name = $name
    }
}

$aptContexts = [System.Collections.Generic.List[object]]::new()
if ($null -eq $runtimeApt) {
    Add-Violation -Violations $violations -Path $runtimeRelativePath -Issue "Runtime APT trust bootstrap context was not found"
}
else {
    $aptContexts.Add($runtimeApt)
}
if ($null -eq $packContexts["pack/debian.12-x64"]) {
    Add-Violation -Violations $violations -Path $packRelativePath -Issue "Debian package verifier trust context was not found"
}
else {
    $aptContexts.Add($packContexts["pack/debian.12-x64"])
}

$rpmContexts = [System.Collections.Generic.List[object]]::new()
if ($null -eq $runtimeRpm) {
    Add-Violation -Violations $violations -Path $runtimeRelativePath -Issue "Runtime RPM trust bootstrap context was not found"
}
else {
    $rpmContexts.Add($runtimeRpm)
}
foreach ($name in @("pack/fedora.40-x64", "pack/rocky.9-x64", "pack/rhel.9-x64")) {
    if ($null -eq $packContexts[$name]) {
        Add-Violation -Violations $violations -Path $packRelativePath -Issue "RPM package verifier trust context was not found" -Text $name
    }
    else {
        $rpmContexts.Add($packContexts[$name])
    }
}

$allContexts = @($aptContexts) + @($rpmContexts)
foreach ($context in $allContexts) {
    Assert-ExactToken -Violations $violations -Context $context -Needle "microsoft_key_url=`"$expectedKeyUrl`"" -Issue "Trust context must use the exact Microsoft HTTPS signing-key URL"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'microsoft_key="/tmp/microsoft-packages-key.asc"' -Issue "Trust context must download to the controlled temporary key file"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'curl --proto "=https" --tlsv1.2 -fsSL "$microsoft_key_url" -o "$microsoft_key"' -Issue "Trust context must acquire the key as a controlled HTTPS file"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'echo "$MICROSOFT_SIGNING_KEY_SHA256  $microsoft_key" | sha256sum -c -' -Issue "Trust context must verify the exact key payload hash"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'gpg --batch --show-keys --with-colons --fingerprint "$microsoft_key"' -Issue "Trust context must parse the downloaded key fingerprint"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'test "$microsoft_key_fingerprints" = "$MICROSOFT_SIGNING_KEY_FINGERPRINT"' -Issue "Trust context must require exactly the approved full fingerprint"
    Assert-ExactToken -Violations $violations -Context $context -Needle $expectedKeyUid -Issue "Trust context must retain the audited Microsoft release-signing UID"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'test "$installed_key_fingerprints" = "$MICROSOFT_SIGNING_KEY_FINGERPRINT"' -Issue "Trust context must recheck the installed local key fingerprint"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'rm -f "$microsoft_key"' -Issue "Trust context must remove its temporary downloaded key"
    Assert-OrderedTokens `
        -Violations $violations `
        -Context $context `
        -Tokens @(
            'microsoft_key_url=',
            'curl --proto "=https"',
            'sha256sum -c -',
            'microsoft_key_metadata=',
            'test "$microsoft_key_fingerprints" = "$MICROSOFT_SIGNING_KEY_FINGERPRINT"') `
        -Issue "Key download, hash, parse, and full-fingerprint verification must remain ordered"
}

foreach ($context in $aptContexts) {
    Assert-ExactToken -Violations $violations -Context $context -Needle 'gpg --batch --yes --dearmor --output /etc/apt/keyrings/packages.microsoft.gpg "$microsoft_key"' -Issue "APT trust context must dearmor only the verified local key"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'gpg --batch --show-keys --with-colons --fingerprint /etc/apt/keyrings/packages.microsoft.gpg' -Issue "APT trust context must reparse the installed scoped keyring"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'deb [arch=amd64 signed-by=/etc/apt/keyrings/packages.microsoft.gpg] https://packages.microsoft.com/debian/12/prod bookworm main' -Issue "APT trust context must use the exact scoped Debian 12 source"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'MICROSOFT_APT_TRUST_EVIDENCE' -Issue "APT trust context must emit exact trust evidence"
    Assert-OrderedTokens `
        -Violations $violations `
        -Context $context `
        -Tokens @(
            'test "$microsoft_key_fingerprints" = "$MICROSOFT_SIGNING_KEY_FINGERPRINT"',
            'gpg --batch --yes --dearmor',
            'installed_key_fingerprints=',
            'test "$installed_key_fingerprints" = "$MICROSOFT_SIGNING_KEY_FINGERPRINT"',
            'signed-by=/etc/apt/keyrings/packages.microsoft.gpg',
            'MICROSOFT_APT_TRUST_EVIDENCE') `
        -Issue "APT fingerprint, dearmor, installed-key check, source, and evidence must remain ordered"

    $sourceIndex = $context.Text.IndexOf('signed-by=/etc/apt/keyrings/packages.microsoft.gpg', [System.StringComparison]::Ordinal)
    $refreshIndex = $context.Text.LastIndexOf('apt-get update', [System.StringComparison]::Ordinal)
    $installIndex = $context.Text.LastIndexOf('apt-get install -y --no-install-recommends "powershell=', [System.StringComparison]::Ordinal)
    if ($sourceIndex -lt 0 -or $refreshIndex -le $sourceIndex -or $installIndex -le $refreshIndex) {
        Add-Violation -Violations $violations -Path $context.Path -Line $context.Line -Issue "APT source must be trusted before metadata refresh and PowerShell installation" -Text $context.Name
    }
}

$repoTokens = @(
    '"[packages-microsoft-com-prod]" \',
    '"name=Microsoft Production" \',
    '"baseurl=https://packages.microsoft.com/rhel/9/prod/" \',
    '"enabled=1" \',
    '"gpgcheck=1" \',
    '"repo_gpgcheck=1" \',
    '"gpgkey=file:///etc/pki/rpm-gpg/RPM-GPG-KEY-Microsoft" \',
    '"sslverify=1" \'
)
foreach ($context in $rpmContexts) {
    Assert-ExactToken -Violations $violations -Context $context -Needle "MICROSOFT_SIGNING_KEY_SHA256: $expectedKeySha256" -ExpectedCount ($(if ($context.Name -like "pack/*") { 1 } else { 0 })) -Issue "Pack RPM context must bind the exact key SHA256 locally"
    Assert-ExactToken -Violations $violations -Context $context -Needle "MICROSOFT_SIGNING_KEY_FINGERPRINT: $expectedKeyFingerprint" -ExpectedCount ($(if ($context.Name -like "pack/*") { 1 } else { 0 })) -Issue "Pack RPM context must bind the exact full fingerprint locally"
    Assert-ExactToken -Violations $violations -Context $context -Needle "microsoft_local_key=`"$expectedLocalRpmKey`"" -Issue "RPM trust context must bind the exact local key path"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'install -o root -g root -m 0644 "$microsoft_key" "$microsoft_local_key"' -Issue "RPM trust context must install only the verified key with controlled ownership and mode"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'gpg --batch --show-keys --with-colons --fingerprint "$microsoft_local_key"' -Issue "RPM trust context must reparse the installed local key"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'rpm --import "$microsoft_local_key"' -Issue "RPM trust context must import only the verified local key"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'rpm -q gpg-pubkey-be1229cf-5631588c' -Issue "RPM trust context must confirm the imported audited key package"
    foreach ($token in $repoTokens) {
        Assert-ExactToken -Violations $violations -Context $context -Needle $token -Issue "RPM trust context must render the exact hardened local repository configuration"
    }
    Assert-ExactToken -Violations $violations -Context $context -Needle '> /etc/yum.repos.d/microsoft-prod.repo' -Issue "RPM trust context must write the exact local repository file"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'dnf -y makecache --disablerepo="*" --enablerepo="packages-microsoft-com-prod"' -Issue "RPM trust context must noninteractively probe only the exact hardened Microsoft repository"
    Assert-ExactToken -Violations $violations -Context $context -Needle 'MICROSOFT_RPM_TRUST_EVIDENCE' -Issue "RPM trust context must emit exact trust evidence"
    Assert-OrderedTokens `
        -Violations $violations `
        -Context $context `
        -Tokens @(
            'test "$microsoft_key_fingerprints" = "$MICROSOFT_SIGNING_KEY_FINGERPRINT"',
            'install -o root -g root -m 0644',
            'installed_key_fingerprints=',
            'test "$installed_key_fingerprints" = "$MICROSOFT_SIGNING_KEY_FINGERPRINT"',
            'rpm --import "$microsoft_local_key"',
            '[packages-microsoft-com-prod]',
            'gpgkey=file:///etc/pki/rpm-gpg/RPM-GPG-KEY-Microsoft',
            'dnf -y makecache',
            'MICROSOFT_RPM_TRUST_EVIDENCE',
            'dnf install -y "$POWERSHELL_RPM_PACKAGE_NEVRA"') `
        -Issue "RPM fingerprint, local import, exact repo, metadata, evidence, and install must remain ordered"
}

$globalExpectedCounts = [ordered]@{
    $expectedKeyUrl = 6
    'microsoft_key="/tmp/microsoft-packages-key.asc"' = 6
    'curl --proto "=https" --tlsv1.2 -fsSL "$microsoft_key_url" -o "$microsoft_key"' = 6
    'echo "$MICROSOFT_SIGNING_KEY_SHA256  $microsoft_key" | sha256sum -c -' = 6
    'gpg --batch --yes --dearmor --output /etc/apt/keyrings/packages.microsoft.gpg "$microsoft_key"' = 2
    'rpm --import "$microsoft_local_key"' = 4
    'gpgkey=file:///etc/pki/rpm-gpg/RPM-GPG-KEY-Microsoft' = 4
    'dnf -y makecache --disablerepo="*" --enablerepo="packages-microsoft-com-prod"' = 4
    'MICROSOFT_APT_TRUST_EVIDENCE' = 2
    'MICROSOFT_RPM_TRUST_EVIDENCE' = 4
}
foreach ($needle in $globalExpectedCounts.Keys) {
    $count = Get-TextCount -Text $controlledText -Needle $needle
    if ($count -ne $globalExpectedCounts[$needle]) {
        Add-Violation -Violations $violations -Path ".github/workflows" -Issue "Microsoft feed trust token count must match the exact classified surface" -Text "'$needle' actual=$count expected=$($globalExpectedCounts[$needle])"
    }
}
if ((Get-TextCount -Text $controlledText -Needle $expectedKeySha256) -ne 5 -or
    (Get-TextCount -Text $controlledText -Needle $expectedKeyFingerprint) -ne 5) {
    Add-Violation -Violations $violations -Path ".github/workflows" -Issue "Exact key hash and fingerprint must be bound once in the runtime producer and four verifier jobs"
}

$forbiddenPatterns = [ordered]@{
    'rpm\s+--import\s+https?://' = "Direct network RPM key import is forbidden"
    'https://packages\.microsoft\.com/config/rhel/9/prod\.repo' = "Mutable remote prod.repo download is forbidden"
    'curl[^\r\n]*microsoft\.asc[^\r\n]*\|\s*gpg' = "Network key bytes must not be piped directly into GPG"
    'gpgkey\s*=\s*https?://' = "RPM repository must reference only the verified local key"
    '(?m)^\s*"?gpgcheck=0"?' = "RPM package signature verification must remain enabled"
    '(?m)^\s*"?repo_gpgcheck=0"?' = "RPM repository metadata signature verification must remain enabled"
    '(?m)^\s*"?sslverify=0"?' = "RPM repository TLS verification must remain enabled"
    '(?i)--nogpgcheck|trusted=yes|allow-insecure|apt-key\s|/etc/apt/trusted\.gpg|gpg\s+--recv-keys|keyserver' = "Package-feed trust bypass is forbidden"
}
foreach ($pattern in $forbiddenPatterns.Keys) {
    foreach ($match in [regex]::Matches($controlledText, $pattern)) {
        $path = if ($match.Index -lt $runtimeText.Length) { $runtimeRelativePath } else { $packRelativePath }
        $localIndex = if ($path -eq $runtimeRelativePath) { $match.Index } else { $match.Index - $runtimeText.Length - 1 }
        $sourceText = if ($path -eq $runtimeRelativePath) { $runtimeText } else { $packText }
        Add-Violation -Violations $violations -Path $path -Line (Get-LineNumber -Text $sourceText -Index $localIndex) -Issue $forbiddenPatterns[$pattern] -Text $match.Value
    }
}

foreach ($workflowFile in @($runtimePath, $packPath)) {
    $relativePath = Get-RelativePath -Path $workflowFile
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($workflowFile)) {
        $lineNumber++
        if ($line.Contains($expectedKeyUrl, [System.StringComparison]::Ordinal) -and
            $line.Trim() -cne "microsoft_key_url=`"$expectedKeyUrl`"") {
            Add-Violation -Violations $violations -Path $relativePath -Line $lineNumber -Issue "Microsoft signing-key URL may appear only in the controlled file-acquisition binding" -Text $line
        }
    }
}

$aggregateText = [System.IO.File]::ReadAllText($aggregatePath)
foreach ($scriptName in @(
        "Test-MicrosoftPackageFeedTrustBoundary.ps1",
        "Test-WorkflowContainerImageSupplyChainBoundary.ps1",
        "Test-DotNetInstallerSupplyChainBoundary.ps1",
        "Test-GitHubActionSupplyChainBoundary.ps1")) {
    $count = Get-TextCount -Text $aggregateText -Needle $scriptName
    if ($count -ne 1) {
        Add-Violation -Violations $violations -Path (Get-RelativePath -Path $aggregatePath) -Issue "Supply-chain guard must be registered exactly once in the aggregate suite" -Text "$scriptName count=$count"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Microsoft package-feed trust boundary failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Line, Issue | Format-Table Path, Line, Issue, Text -AutoSize -Wrap
    exit 1
}

Write-Host "Microsoft package-feed trust boundary passed."
Write-Host "Key acquisitions: 6; APT scoped dearmor paths: $($aptContexts.Count); RPM verified local imports: $($rpmContexts.Count); local RHEL 9 repo configurations: $($rpmContexts.Count)."
Write-Host "Key SHA256: $($expectedKeySha256.ToUpperInvariant()); full fingerprint: $expectedKeyFingerprint."
Write-Host "All trust actions occur after payload and full-fingerprint verification; remote prod.repo and remote gpgkey paths: 0."
