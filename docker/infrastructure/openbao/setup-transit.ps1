$ErrorActionPreference = "Stop"

$infrastructureDirectory = Split-Path $PSScriptRoot -Parent
$mainSharesPath = Join-Path $PSScriptRoot "secrets\secrets.txt"
$transitSharesPath = Join-Path $PSScriptRoot "secrets\transit-seal-shares.txt"
$transitTokenPath = Join-Path $PSScriptRoot "transit-seal.env"
$transitEnabledPath = Join-Path $PSScriptRoot "transit-seal.enabled"
$snapshotPath = Join-Path $PSScriptRoot "raft-before-transit.snap"
$caCertificatePath = Join-Path $PSScriptRoot "tls\ca.crt"
$mainNodes = @(
    @{ Name = "openbao-1"; Address = "https://localhost:8200" },
    @{ Name = "openbao-2"; Address = "https://localhost:8202" },
    @{ Name = "openbao-3"; Address = "https://localhost:8204" }
)
$transitAddress = "https://localhost:8210"

function Protect-LocalFile {
    param([string]$Path)

    $identity = [System.Security.Principal.WindowsIdentity]::GetCurrent().User
    $acl = [System.Security.AccessControl.FileSecurity]::new()
    $acl.SetAccessRuleProtection($true, $false)
    $acl.SetOwner($identity)
    $rule = [System.Security.AccessControl.FileSystemAccessRule]::new(
        $identity,
        [System.Security.AccessControl.FileSystemRights]::FullControl,
        [System.Security.AccessControl.AccessControlType]::Allow
    )
    $acl.AddAccessRule($rule)
    Set-Acl -LiteralPath $Path -AclObject $acl
}

function ConvertFrom-SecureInput {
    param([System.Security.SecureString]$Value)

    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try {
        [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    } finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
        $Value.Dispose()
    }
}

function Read-OpenBaoSecret {
    param([string]$Prompt)

    ConvertFrom-SecureInput (Read-Host -Prompt $Prompt -AsSecureString)
}

function Invoke-OpenBaoApi {
    param(
        [string]$Address,
        [string]$Path,
        [string]$Method = "Get",
        [string]$Token,
        [object]$Body
    )

    $request = @{
        Uri = "$Address/v1/$Path"
        Method = $Method
        TimeoutSec = 15
    }
    if ($Token) {
        $request.Headers = @{ "X-Vault-Token" = $Token }
    }
    if ($null -ne $Body) {
        $request.ContentType = "application/json"
        $request.Body = $Body | ConvertTo-Json -Depth 8 -Compress
    }

    Invoke-RestMethod @request
}

function Wait-ForOpenBao {
    param(
        [string]$Address,
        [int]$TimeoutSeconds = 120,
        [switch]$RequireUnsealed,
        [string]$ExpectedSealType
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        try {
            $status = Invoke-OpenBaoApi -Address $Address -Path "sys/seal-status"
            $isUnsealed = -not $RequireUnsealed -or ($status.initialized -and -not $status.sealed)
            $hasExpectedSealType = -not $ExpectedSealType -or $status.type -eq $ExpectedSealType
            if ($isUnsealed -and $hasExpectedSealType) {
                return $status
            }
        } catch {
        }
        Start-Sleep -Seconds 2
    } while ((Get-Date) -lt $deadline)

    throw "OpenBao at $Address did not reach the expected state within $TimeoutSeconds seconds."
}

function Wait-ForSealMigration {
    param(
        [hashtable[]]$Nodes,
        [string]$FormerLeader,
        [int]$TimeoutSeconds = 300
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $allStandbysMigrated = $true
        $newLeaderElected = $false

        foreach ($node in $Nodes) {
            try {
                $status = Invoke-OpenBaoApi -Address $node.Address -Path "sys/seal-status"
                $leaderStatus = Invoke-OpenBaoApi -Address $node.Address -Path "sys/leader"
                if (-not $status.initialized -or $status.sealed -or $status.type -ne "transit") {
                    $allStandbysMigrated = $false
                }
                if ($node.Name -ne $FormerLeader -and $leaderStatus.is_self) {
                    $newLeaderElected = $true
                }
            } catch {
                $allStandbysMigrated = $false
            }
        }

        if ($allStandbysMigrated -and $newLeaderElected) {
            return
        }
        Start-Sleep -Seconds 2
    } while ((Get-Date) -lt $deadline)

    throw "The migrated standby nodes did not complete Transit seal migration within $TimeoutSeconds seconds. The former active node was not restarted."
}

function Invoke-Compose {
    param([string[]]$Arguments)

    & docker compose @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "docker compose failed: $($Arguments -join ' ')"
    }
}

function Set-NodeSeal {
    param(
        [hashtable]$Node,
        [string[]]$Shares,
        [switch]$Migrate
    )

    foreach ($share in $Shares) {
        $body = @{ key = $share }
        if ($Migrate) {
            $body.migrate = $true
        }
        $status = Invoke-OpenBaoApi -Address $Node.Address -Path "sys/unseal" -Method "Post" -Body $body
    }
    if ($status.sealed) {
        throw "$($Node.Name) remained sealed after receiving the required shares."
    }
}

Push-Location $infrastructureDirectory
try {
    if (-not (Test-Path -LiteralPath $caCertificatePath)) {
        throw "The local OpenBao CA is missing. Generate it before setting up Transit."
    }
    if (-not (Test-Path -LiteralPath $mainSharesPath -PathType Leaf)) {
        throw "Main-cluster shares file not found: $mainSharesPath"
    }

    $mainShares = @(
        Get-Content -LiteralPath $mainSharesPath |
            ForEach-Object { $_.Trim() } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
    if ($mainShares.Count -ne 5 -or @($mainShares | Select-Object -Unique).Count -ne 5) {
        throw "Expected five distinct, non-empty main-cluster shares in $mainSharesPath."
    }

    foreach ($node in $mainNodes) {
        $status = Invoke-OpenBaoApi -Address $node.Address -Path "sys/seal-status"
        if (-not $status.initialized -or $status.sealed) {
            throw "$($node.Name) must be initialized and unsealed before seal migration."
        }
    }

    $transitCertificatePath = Join-Path $PSScriptRoot "tls\openbao-transit.crt"
    $transitKeyPath = Join-Path $PSScriptRoot "tls\openbao-transit.key"
    if (-not (Test-Path $transitCertificatePath) -and -not (Test-Path $transitKeyPath)) {
        & (Join-Path $PSScriptRoot "generate-transit-cert.ps1")
    } elseif (-not (Test-Path $transitCertificatePath) -or -not (Test-Path $transitKeyPath)) {
        throw "Only one Transit TLS file exists. Resolve the certificate pair before continuing."
    }

    Write-Host "Starting the Transit provider..."
    Invoke-Compose -Arguments @("--profile", "transit", "up", "-d", "openbao-transit")
    Invoke-Compose -Arguments @(
        "--profile", "transit", "rm", "--force",
        "openbao-transit-data-init", "openbao-transit-audit-init"
    )
    $transitStatus = Wait-ForOpenBao -Address $transitAddress

    $transitRootToken = $null
    if (-not $transitStatus.initialized) {
        $initialization = Invoke-OpenBaoApi -Address $transitAddress -Path "sys/init" -Method "Put" -Body @{
            secret_shares = 5
            secret_threshold = 3
        }
        $transitShares = @($initialization.keys)
        if ($transitShares.Count -ne 5 -or [string]::IsNullOrWhiteSpace($initialization.root_token)) {
            throw "Transit initialization returned an unexpected response. Do not discard its recovery material."
        }

        $shareDirectory = Split-Path $transitSharesPath -Parent
        New-Item -ItemType Directory -Path $shareDirectory -Force | Out-Null
        Set-Content -LiteralPath $transitSharesPath -Value $transitShares -Encoding ascii
        Protect-LocalFile -Path $transitSharesPath
        $transitRootToken = $initialization.root_token
        Write-Host "Transit was initialized. Its five shares were saved in the ignored, account-restricted secrets directory."
    }

    $transitStatus = Invoke-OpenBaoApi -Address $transitAddress -Path "sys/seal-status"
    if ($transitStatus.sealed) {
        if (-not $transitShares -or $transitShares.Count -lt 3) {
            if (-not (Test-Path -LiteralPath $transitSharesPath -PathType Leaf)) {
                throw "Transit is sealed and its shares file is missing. Recover Transit before continuing."
            }
            $transitShares = @(
                Get-Content -LiteralPath $transitSharesPath |
                    ForEach-Object { $_.Trim() } |
                    Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
            )
        }
        if ($transitShares.Count -lt 3) {
            throw "At least three Transit shares are required in $transitSharesPath."
        }
        Set-NodeSeal -Node @{ Name = "openbao-transit"; Address = $transitAddress } -Shares @($transitShares | Select-Object -First 3)
    }

    $sealToken = $null
    if (Test-Path -LiteralPath $transitTokenPath -PathType Leaf) {
        $sealTokenLine = Get-Content -LiteralPath $transitTokenPath | Where-Object { $_ -match '^BAO_TOKEN=' } | Select-Object -First 1
        if (-not $sealTokenLine) {
            throw "The Transit seal token file is malformed: $transitTokenPath"
        }
        $sealToken = $sealTokenLine.Substring("BAO_TOKEN=".Length)
        if ([string]::IsNullOrWhiteSpace($sealToken)) {
            throw "The Transit seal token file is empty: $transitTokenPath"
        }

        $challenge = [Guid]::NewGuid().ToString("N")
        $plaintext = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($challenge))
        $encrypted = Invoke-OpenBaoApi -Address $transitAddress -Path "transit/encrypt/techstack-seal" -Method "Post" -Token $sealToken -Body @{
            plaintext = $plaintext
        }
        $decrypted = Invoke-OpenBaoApi -Address $transitAddress -Path "transit/decrypt/techstack-seal" -Method "Post" -Token $sealToken -Body @{
            ciphertext = $encrypted.data.ciphertext
        }
        if ([Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($decrypted.data.plaintext)) -cne $challenge) {
            throw "The Transit seal token failed its encrypt/decrypt check."
        }
        Write-Host "Reusing the existing Transit seal token; its encrypt/decrypt permissions are verified."
    } else {
        if (-not $transitRootToken) {
            $transitRootToken = Read-OpenBaoSecret -Prompt "Transit provider root token (needed to configure Transit)"
        }
        Invoke-OpenBaoApi -Address $transitAddress -Path "auth/token/lookup-self" -Token $transitRootToken | Out-Null

        $mounts = Invoke-OpenBaoApi -Address $transitAddress -Path "sys/mounts" -Token $transitRootToken
        if (-not $mounts.data.PSObject.Properties["transit/"]) {
            Invoke-OpenBaoApi -Address $transitAddress -Path "sys/mounts/transit" -Method "Post" -Token $transitRootToken -Body @{
                type = "transit"
            } | Out-Null
        }

        try {
            Invoke-OpenBaoApi -Address $transitAddress -Path "transit/keys/techstack-seal" -Token $transitRootToken | Out-Null
        } catch {
            $statusCode = [int]$_.Exception.Response.StatusCode
            if ($statusCode -ne 404) {
                throw
            }
            Invoke-OpenBaoApi -Address $transitAddress -Path "transit/keys/techstack-seal" -Method "Post" -Token $transitRootToken -Body @{
                type = "aes256-gcm96"
            } | Out-Null
        }

        $policy = Get-Content -LiteralPath (Join-Path $PSScriptRoot "transit-seal-policy.hcl") -Raw
        Invoke-OpenBaoApi -Address $transitAddress -Path "sys/policies/acl/transit-seal" -Method "Put" -Token $transitRootToken -Body @{
            policy = $policy
        } | Out-Null

        $sealToken = (Invoke-OpenBaoApi -Address $transitAddress -Path "auth/token/create-orphan" -Method "Post" -Token $transitRootToken -Body @{
            policies = @("transit-seal")
            period = "24h"
            no_default_policy = $true
        }).auth.client_token
        if ([string]::IsNullOrWhiteSpace($sealToken)) {
            throw "OpenBao did not return a Transit seal token."
        }

        Set-Content -LiteralPath $transitTokenPath -Value "BAO_TOKEN=$sealToken" -Encoding ascii
        Protect-LocalFile -Path $transitTokenPath
    }

    if (Test-Path -LiteralPath $snapshotPath) {
        throw "A pre-Transit snapshot already exists at $snapshotPath. Move it to protected storage before rerunning."
    }

    $mainRootToken = Read-OpenBaoSecret -Prompt "Main cluster root token (required for a Raft snapshot)"
    $sealTokenLine = Get-Content -LiteralPath $transitTokenPath | Where-Object { $_ -match '^BAO_TOKEN=' } | Select-Object -First 1
    $configuredSealToken = $sealTokenLine.Substring("BAO_TOKEN=".Length)
    if ($mainRootToken -ceq $configuredSealToken) {
        throw "The entered token is the Transit encrypt/decrypt token. Enter the main cluster root token saved during main-cluster initialization; the Transit token cannot take a Raft snapshot."
    }
    try {
        Invoke-OpenBaoApi -Address $mainNodes[0].Address -Path "auth/token/lookup-self" -Token $mainRootToken | Out-Null
    } catch {
        throw "The token was not accepted by the main OpenBao cluster. Enter the main-cluster root token, not the Transit seal token."
    }
    $configuredSealToken = $null
    $sealTokenLine = $null
    Invoke-WebRequest -Uri "$($mainNodes[0].Address)/v1/sys/storage/raft/snapshot" `
        -Headers @{ "X-Vault-Token" = $mainRootToken } `
        -OutFile $snapshotPath `
        -TimeoutSec 60 | Out-Null
    if ((Get-Item -LiteralPath $snapshotPath).Length -eq 0) {
        throw "The Raft snapshot is empty; refusing to start seal migration."
    }
    Protect-LocalFile -Path $snapshotPath
    Write-Host "Raft snapshot saved to the ignored OpenBao directory."

    $confirmation = Read-Host "This will restart all three OpenBao nodes in sequence. Type MIGRATE to continue"
    if ($confirmation -cne "MIGRATE") {
        throw "Seal migration cancelled. Transit is prepared, but the main cluster was not changed."
    }

    $leader = $null
    foreach ($node in $mainNodes) {
        $leaderStatus = Invoke-OpenBaoApi -Address $node.Address -Path "sys/leader" -Token $mainRootToken
        if ($leaderStatus.is_self) {
            $leader = $node
            break
        }
    }
    if (-not $leader) {
        throw "Could not identify the active Raft node. The main cluster was not migrated."
    }

    $standbys = @($mainNodes | Where-Object { $_.Name -ne $leader.Name })
    foreach ($node in $standbys) {
        Write-Host "Migrating standby $($node.Name)..."
        Invoke-Compose -Arguments @("--profile", "transit", "stop", $node.Name)
        Invoke-Compose -Arguments @(
            "--profile", "transit", "-f", "docker-compose.yml", "-f", "docker-compose.openbao-transit.yml",
            "up", "-d", "--no-deps", "--force-recreate", $node.Name
        )
        Wait-ForOpenBao -Address $node.Address | Out-Null
        Set-NodeSeal -Node $node -Shares @($mainShares | Select-Object -First 3) -Migrate
        Write-Host "$($node.Name) accepted its migration shares; leaving it running for the active node to complete migration."
    }

    Write-Host "Stepping down active node $($leader.Name) and restarting it with Transit seal..."
    Invoke-OpenBaoApi -Address $leader.Address -Path "sys/step-down" -Method "Post" -Token $mainRootToken | Out-Null
    Write-Host "Waiting for the migrated standby nodes to elect a leader and report Transit seal status..."
    Wait-ForSealMigration -Nodes $standbys -FormerLeader $leader.Name
    Invoke-Compose -Arguments @("--profile", "transit", "stop", $leader.Name)
    Invoke-Compose -Arguments @(
        "--profile", "transit", "-f", "docker-compose.yml", "-f", "docker-compose.openbao-transit.yml",
        "up", "-d", "--no-deps", "--force-recreate", $leader.Name
    )
    Wait-ForOpenBao -Address $leader.Address -RequireUnsealed | Out-Null

    foreach ($node in $mainNodes) {
        Wait-ForOpenBao -Address $node.Address -RequireUnsealed -ExpectedSealType "transit" | Out-Null
        Write-Host "$($node.Name) is initialized and unsealed."
    }

    New-Item -ItemType File -Path $transitEnabledPath -Force | Out-Null
    Write-Host "Transit auto-unseal migration completed. The Transit provider itself still requires manual unseal after restart."
} finally {
    $mainRootToken = $null
    $transitRootToken = $null
    $transitShares = $null
    $mainShares = $null
    Pop-Location
}