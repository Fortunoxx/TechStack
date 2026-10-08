$ErrorActionPreference = "Stop"

$infrastructureDirectory = Split-Path -Parent $PSScriptRoot
$environmentFile = Join-Path $infrastructureDirectory ".env"
$policyFile = Join-Path $PSScriptRoot "startup-policy.hcl"
$address = "https://localhost:8200"

function ConvertTo-PlainText {
    param([System.Security.SecureString]$Value)

    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try {
        [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    } finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    }
}

function Invoke-OpenBaoAdminRequest {
    param(
        [string]$Path,
        [string]$Method = "Get",
        [object]$Body
    )

    $request = @{
        Uri = "$address/v1/$Path"
        Method = $Method
        Headers = @{ "X-Vault-Token" = $rootToken }
        TimeoutSec = 10
    }
    if ($null -ne $Body) {
        $request.ContentType = "application/json"
        $request.Body = $Body | ConvertTo-Json -Depth 8 -Compress
    }
    Invoke-RestMethod @request
}

$rootTokenSecure = Read-Host "OpenBao root token" -AsSecureString
$rootToken = ConvertTo-PlainText $rootTokenSecure
$rootTokenSecure.Dispose()

$discordWebhookSecure = $null
$initialSaPasswordSecure = $null
$newSaPasswordSecure = $null
$discordWebhookUrl = $null
$initialSaPassword = $null
$newSaPassword = $null

try {
    $mounts = Invoke-OpenBaoAdminRequest -Path "sys/mounts"
    $secretMountProperty = $mounts.data.PSObject.Properties["secret/"]
    if (-not $secretMountProperty) {
        Invoke-OpenBaoAdminRequest -Path "sys/mounts/secret" -Method "Post" -Body @{
            type = "kv"
            options = @{ version = "2" }
        } | Out-Null
    } elseif ($secretMountProperty.Value.options.version -ne "2") {
        throw "The secret/ mount already exists but is not KV v2. Resolve this manually before continuing."
    }

    Invoke-OpenBaoAdminRequest -Path "sys/policies/acl/techstack-startup" -Method "Put" -Body @{
        policy = Get-Content $policyFile -Raw
    } | Out-Null

    $authMethods = Invoke-OpenBaoAdminRequest -Path "sys/auth"
    if (-not $authMethods.data.PSObject.Properties["approle/"]) {
        Invoke-OpenBaoAdminRequest -Path "sys/auth/approle" -Method "Post" -Body @{
            type = "approle"
        } | Out-Null
    }

    Invoke-OpenBaoAdminRequest -Path "auth/approle/role/techstack-startup" -Method "Post" -Body @{
        token_policies = @("techstack-startup")
        token_no_default_policy = $true
        token_ttl = "5m"
        token_max_ttl = "15m"
        bind_secret_id = $true
        secret_id_ttl = "720h"
        secret_id_num_uses = 0
    } | Out-Null

    $discordWebhookSecure = Read-Host "Discord webhook URL" -AsSecureString
    $initialSaPasswordSecure = Read-Host "Initial SQL Server SA password" -AsSecureString
    $newSaPasswordSecure = Read-Host "New SQL Server SA password" -AsSecureString
    $discordWebhookUrl = ConvertTo-PlainText $discordWebhookSecure
    $initialSaPassword = ConvertTo-PlainText $initialSaPasswordSecure
    $newSaPassword = ConvertTo-PlainText $newSaPasswordSecure

    if ([string]::IsNullOrWhiteSpace($discordWebhookUrl) -or
        [string]::IsNullOrWhiteSpace($initialSaPassword) -or
        [string]::IsNullOrWhiteSpace($newSaPassword)) {
        throw "All three secret values are required."
    }

    Invoke-OpenBaoAdminRequest -Path "secret/data/techstack/alertmanager" -Method "Post" -Body @{
        data = @{ discord_webhook_url = $discordWebhookUrl }
    } | Out-Null
    Invoke-OpenBaoAdminRequest -Path "secret/data/techstack/mssql" -Method "Post" -Body @{
        data = @{
            initial_sa_password = $initialSaPassword
            new_sa_password = $newSaPassword
        }
    } | Out-Null

    $role = Invoke-OpenBaoAdminRequest -Path "auth/approle/role/techstack-startup/role-id"
    $secretId = Invoke-OpenBaoAdminRequest -Path "auth/approle/role/techstack-startup/secret-id" -Method "Post" -Body @{}

    $environmentLines = @()
    if (Test-Path $environmentFile) {
        $environmentLines = @(Get-Content $environmentFile)
    } else {
        $environmentLines = @("REGISTRY=cr.jaegertracing.io/")
    }
    $roleIdLine = "OPENBAO_ROLE_ID=$($role.data.role_id)"
    $roleIdIndex = -1
    for ($index = 0; $index -lt $environmentLines.Count; $index++) {
        if ($environmentLines[$index] -match '^OPENBAO_ROLE_ID=') {
            $roleIdIndex = $index
            break
        }
    }
    if ($roleIdIndex -ge 0) {
        $environmentLines[$roleIdIndex] = $roleIdLine
    } else {
        $environmentLines += $roleIdLine
    }
    [System.IO.File]::WriteAllLines(
        $environmentFile,
        [string[]]$environmentLines,
        (New-Object System.Text.UTF8Encoding($false))
    )

    Write-Host "OpenBao KV v2 secrets and the read-only startup AppRole are configured."
    Write-Host "Role ID saved in the ignored .env file: $($role.data.role_id)"
    Write-Host "Save this Secret ID in a password manager; startup.ps1 prompts for it and it expires in 30 days:"
    Write-Host $secretId.data.secret_id
} finally {
    $rootToken = $null
    $discordWebhookUrl = $null
    $initialSaPassword = $null
    $newSaPassword = $null
    $secretId = $null
    if ($discordWebhookSecure) { $discordWebhookSecure.Dispose() }
    if ($initialSaPasswordSecure) { $initialSaPasswordSecure.Dispose() }
    if ($newSaPasswordSecure) { $newSaPasswordSecure.Dispose() }
}