#Requires -Version 7.0
# Installs (first run) or brings up (later runs) both OpenBao clusters:
#   openbao-transit-1..3  seal provider, Shamir-unsealed with the keys saved in openbao/secrets
#   openbao-1..3          application cluster, auto-unsealed through the transit cluster
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "openbao-common.ps1")

function Set-TransitSealToken {
    param([string]$Address, [string]$RootToken)

    if (Test-Path -LiteralPath $SealEnvFile) {
        $line = Get-Content -LiteralPath $SealEnvFile | Where-Object { $_ -match '^BAO_TOKEN=' } | Select-Object -First 1
        $token = if ($line) { $line.Substring("BAO_TOKEN=".Length) } else { $null }
        if ($token -and (Invoke-Bao -Address $Address -Path "auth/token/renew-self" -Method "POST" -Token $token -AllowError)) {
            return
        }
        Write-Host "=> The saved seal token is no longer valid; issuing a new one..." -ForegroundColor Yellow
    }

    $mounts = Invoke-Bao -Address $Address -Path "sys/mounts" -Token $RootToken
    if (-not $mounts.data.PSObject.Properties["transit/"]) {
        Invoke-Bao -Address $Address -Path "sys/mounts/transit" -Method "POST" -Token $RootToken -Body @{ type = "transit" } | Out-Null
    }
    if (-not (Invoke-Bao -Address $Address -Path "transit/keys/techstack-seal" -Token $RootToken -AllowError)) {
        Invoke-Bao -Address $Address -Path "transit/keys/techstack-seal" -Method "POST" -Token $RootToken -Body @{ type = "aes256-gcm96" } | Out-Null
    }
    Invoke-Bao -Address $Address -Path "sys/policies/acl/transit-seal" -Method "PUT" -Token $RootToken -Body @{
        policy = Get-Content -LiteralPath (Join-Path $BaoDir "policies\transit-seal.hcl") -Raw
    } | Out-Null

    $created = Invoke-Bao -Address $Address -Path "auth/token/create-orphan" -Method "POST" -Token $RootToken -Body @{
        policies = @("transit-seal")
        period = "720h"
        no_default_policy = $true
    }
    Set-Content -LiteralPath $SealEnvFile -Value "BAO_TOKEN=$($created.auth.client_token)" -Encoding ascii
    Protect-SecretFile -Path $SealEnvFile
}

Push-Location $InfraDir
try {
    & (Join-Path $PSScriptRoot "generate-certs.ps1")
    New-Item -ItemType Directory -Path $SecretsDir -Force | Out-Null
    Initialize-BaoClient

    # --- Transit cluster -----------------------------------------------------------------
    Write-Step "Starting the transit cluster..."
    # All three start together: after the first install a single node has no Raft quorum.
    Invoke-Compose up -d openbao-transit-1 openbao-transit-2 openbao-transit-3
    $transit1 = $TransitNodes[0]
    $status = Wait-BaoReachable $transit1
    foreach ($node in $TransitNodes[1..2]) { Wait-BaoReachable $node | Out-Null }

    if (-not $status.initialized) {
        Write-Step "Initializing the transit cluster (first install)..."
        $response = Invoke-Bao -Address $transit1.Address -Path "sys/init" -Method "PUT" -Body @{
            secret_shares = 5
            secret_threshold = 3
        }
        Save-InitFile -Path $TransitInitFile -Response $response
        Write-Host "   Unseal keys and root token saved to $TransitInitFile" -ForegroundColor Yellow
    }
    $transitInit = Read-InitFile -Path $TransitInitFile -What "Transit unseal keys"

    # Nodes 2 and 3 join node 1 on their own (retry_join) and accept keys once joined.
    foreach ($node in $TransitNodes) {
        Open-BaoNode -Node $node -Keys $transitInit.keys
    }
    Wait-BaoLeader $transit1
    Set-TransitSealToken -Address $transit1.Address -RootToken $transitInit.root_token
    Write-Step "Transit cluster is unsealed."

    # --- Application cluster -------------------------------------------------------------
    Write-Step "Starting the OpenBao cluster..."
    Invoke-Compose up -d openbao-1
    $main1 = $MainNodes[0]
    $status = Wait-BaoReachable $main1

    if (-not $status.initialized) {
        Write-Step "Initializing the OpenBao cluster (first install)..."
        $response = Invoke-Bao -Address $main1.Address -Path "sys/init" -Method "PUT" -Body @{
            recovery_shares = 5
            recovery_threshold = 3
        }
        Save-InitFile -Path $MainInitFile -Response $response
        Write-Host "   Recovery keys and root token saved to $MainInitFile" -ForegroundColor Yellow
    }
    Wait-BaoUnsealed $main1

    Invoke-Compose up -d openbao-2 openbao-3
    foreach ($node in $MainNodes) {
        Wait-BaoUnsealed $node
    }
    Wait-BaoLeader $main1
    Write-Step "OpenBao cluster is unsealed (auto-unseal via transit)."

    $mainInit = Read-InitFile -Path $MainInitFile -What "OpenBao root token"
    try {
        Wait-Until -Description "a Raft leader and three members" -Condition {
            @((Invoke-BaoCluster -Nodes $MainNodes -Path "sys/storage/raft/configuration" -Token $mainInit.root_token).data.config.servers).Count -eq 3
        }
    } catch {
        Write-Host "   Warning: $($_.Exception.Message)" -ForegroundColor Yellow
    }
} finally {
    Remove-VolumeInitContainer
    Pop-Location
}
