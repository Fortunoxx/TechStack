#Requires -Version 7.0
# Configures the KV v2 mount, the read-only startup AppRole and the application secrets.
# Uses the admin token saved by install-openbao.ps1. Values are only prompted for when missing (or with -Reset).
[CmdletBinding()]
param([switch]$Reset)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "openbao-common.ps1")

function ConvertTo-PlainText {
    param([System.Security.SecureString]$Value)

    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try {
        [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    } finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    }
}

function Read-SecretValue {
    param([string]$Prompt)
    $secure = Read-Host $Prompt -AsSecureString
    try { ConvertTo-PlainText $secure } finally { $secure.Dispose() }
}

function Set-EnvValue {
    param([string]$Path, [string]$Name, [string]$Value)

    [string[]]$lines = if (Test-Path $Path) { Get-Content $Path } else { @() }
    $found = $false
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match "^$Name=") {
            $lines[$i] = "$Name=$Value"
            $found = $true
            break
        }
    }
    if (-not $found) { $lines += "$Name=$Value" }
    [System.IO.File]::WriteAllLines($Path, [string[]]$lines, (New-Object System.Text.UTF8Encoding($false)))
}

Initialize-BaoClient
$rootToken = (Read-InitFile -Path $MainInitFile -What "OpenBao root token").root_token

function Invoke-Admin {
    param([string]$Path, [string]$Method = "GET", [object]$Body)
    Invoke-BaoCluster -Nodes $MainNodes -Path $Path -Method $Method -Token $rootToken -Body $Body
}

$mounts = Invoke-Admin "sys/mounts"
$secretMount = $mounts.data.PSObject.Properties["secret/"]
if (-not $secretMount) {
    Invoke-Admin "sys/mounts/secret" "POST" @{ type = "kv"; options = @{ version = "2" } } | Out-Null
} elseif ($secretMount.Value.options.version -ne "2") {
    throw "The secret/ mount already exists but is not KV v2. Resolve this manually before continuing."
}

Invoke-Admin "sys/policies/acl/techstack-startup" "PUT" @{
    policy = Get-Content (Join-Path $BaoDir "policies\startup.hcl") -Raw
} | Out-Null

if (-not (Invoke-Admin "sys/auth").data.PSObject.Properties["approle/"]) {
    Invoke-Admin "sys/auth/approle" "POST" @{ type = "approle" } | Out-Null
}
Invoke-Admin "auth/approle/role/techstack-startup" "POST" @{
    token_policies = @("techstack-startup")
    token_no_default_policy = $true
    token_ttl = "5m"
    token_max_ttl = "15m"
    bind_secret_id = $true
    secret_id_ttl = "0"
    secret_id_num_uses = 0
} | Out-Null

$secretsExist = $true
try {
    Invoke-Admin "secret/data/techstack/mssql" | Out-Null
    Invoke-Admin "secret/data/techstack/alertmanager" | Out-Null
} catch {
    $secretsExist = $false
}

if ($Reset -or -not $secretsExist) {
    Write-Step "Enter the application secrets (stored in OpenBao only)."
    $discordWebhookUrl = Read-SecretValue "Discord webhook URL"
    $initialSaPassword = Read-SecretValue "Initial SQL Server SA password"
    $newSaPassword = Read-SecretValue "New SQL Server SA password"
    if ([string]::IsNullOrWhiteSpace($discordWebhookUrl) -or
        [string]::IsNullOrWhiteSpace($initialSaPassword) -or
        [string]::IsNullOrWhiteSpace($newSaPassword)) {
        throw "All three secret values are required."
    }

    Invoke-Admin "secret/data/techstack/alertmanager" "POST" @{
        data = @{ discord_webhook_url = $discordWebhookUrl }
    } | Out-Null
    Invoke-Admin "secret/data/techstack/mssql" "POST" @{
        data = @{ initial_sa_password = $initialSaPassword; new_sa_password = $newSaPassword }
    } | Out-Null
    $discordWebhookUrl = $initialSaPassword = $newSaPassword = $null
}

$roleId = (Invoke-Admin "auth/approle/role/techstack-startup/role-id").data.role_id
Set-EnvValue -Path (Join-Path $InfraDir ".env") -Name "OPENBAO_ROLE_ID" -Value $roleId

# Keep the existing Secret ID while it still logs in; otherwise issue a new one.
$loginWorks = $false
if (Test-Path -LiteralPath $AppRoleSecretIdFile) {
    $existing = (Get-Content -LiteralPath $AppRoleSecretIdFile -Raw).Trim()
    try {
        Invoke-BaoCluster -Nodes $MainNodes -Path "auth/approle/login" -Method "POST" -Body @{
            role_id = $roleId
            secret_id = $existing
        } | Out-Null
        $loginWorks = $true
    } catch {
        $loginWorks = $false
    }
}
if (-not $loginWorks) {
    $secretId = (Invoke-Admin "auth/approle/role/techstack-startup/secret-id" "POST" @{}).data.secret_id
    Set-Content -LiteralPath $AppRoleSecretIdFile -Value $secretId -Encoding ascii -NoNewline
    Protect-SecretFile -Path $AppRoleSecretIdFile
}

$rootToken = $null
