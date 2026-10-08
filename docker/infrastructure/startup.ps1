$info_color = "Green"
$warning_color = "Yellow"
$highlight_color = "Magenta"

$composeFileArguments = @("-f", "docker-compose.yml")
$transitSealEnabled = Test-Path "./openbao/transit-seal.enabled"
if ($transitSealEnabled) {
    if (-not (Test-Path "./openbao/transit-seal.env")) {
        throw "The Transit seal token file is missing. Follow openbao\README.md."
    }
    $composeFileArguments += @("--profile", "transit", "-f", "docker-compose.openbao-transit.yml")
}

Write-Host "=> Preparing docker compose project..." -ForegroundColor $info_color

# Ensure .env file has REGISTRY setting
$envFile = ".env"
$registryLine = "REGISTRY=cr.jaegertracing.io/"

if (Test-Path $envFile) {
    $envContent = Get-Content $envFile
    if (-not ($envContent -match "^REGISTRY=")) {
        Write-Host "=> Adding REGISTRY to .env file..." -ForegroundColor $info_color
        Add-Content -Path $envFile -Value $registryLine
    }
} else {
    Write-Host "=> Creating .env file with REGISTRY..." -ForegroundColor $info_color
    Set-Content -Path $envFile -Value $registryLine
}

if (-not (Test-Path "./openbao/tls/ca.crt")) {
    throw "OpenBao TLS files are missing. Run .\openbao\generate-certs.ps1 and follow openbao\README.md first."
}
if (-not (Get-Module -ListAvailable -Name SqlServer)) {
    Install-Module SqlServer -Scope CurrentUser -Force
}
Import-Module SqlServer

$openBaoAddresses = @(
    "https://localhost:8200",
    "https://localhost:8202",
    "https://localhost:8204"
)

function ConvertTo-PlainText {
    param([System.Security.SecureString]$Value)

    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try {
        [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    } finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    }
}

function Invoke-OpenBaoRequest {
    param(
        [string]$Path,
        [string]$Method = "Get",
        [string]$Token,
        [object]$Body
    )

    $lastError = $null
    foreach ($address in $openBaoAddresses) {
        $request = @{
            Uri = "$address/v1/$Path"
            Method = $Method
            TimeoutSec = 10
        }
        if ($Token) {
            $request.Headers = @{ "X-Vault-Token" = $Token }
        }
        if ($null -ne $Body) {
            $request.ContentType = "application/json"
            $request.Body = $Body | ConvertTo-Json -Depth 6 -Compress
        }

        try {
            return Invoke-RestMethod @request
        } catch {
            $lastError = $_.Exception.Message
        }
    }

    throw "OpenBao request failed for all local nodes. Last error: $lastError"
}

$openBaoInitServices = @(
    "openbao-1-data-init", "openbao-1-audit-init",
    "openbao-2-data-init", "openbao-2-audit-init",
    "openbao-3-data-init", "openbao-3-audit-init"
)
if ($transitSealEnabled) {
    $openBaoInitServices += @("openbao-transit-data-init", "openbao-transit-audit-init")
}

function Remove-OpenBaoInitContainers {
    docker compose @composeFileArguments rm --force $openBaoInitServices | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to remove the completed OpenBao volume-initializer containers."
    }
}

if ($transitSealEnabled) {
    Write-Host "=> Starting the Transit seal provider..." -ForegroundColor $info_color
    docker compose @composeFileArguments up -d openbao-transit
    $transitStartupExitCode = $LASTEXITCODE
    Remove-OpenBaoInitContainers
    if ($transitStartupExitCode -ne 0) {
        throw "Failed to start the Transit seal provider. See openbao\README.md."
    }

    try {
        $transitSealStatus = Invoke-RestMethod -Uri "https://localhost:8210/v1/sys/seal-status" -TimeoutSec 10
    } catch {
        throw "Transit is unavailable. Check its TLS certificate and container logs."
    }
    if (-not $transitSealStatus.initialized) {
        throw "The Transit seal provider is not initialized. Follow openbao\README.md."
    }
    if ($transitSealStatus.sealed) {
        throw "Transit is sealed. Manually unseal openbao-transit before running startup.ps1."
    }
}

Write-Host "=> Starting OpenBao cluster..." -ForegroundColor $info_color
docker compose @composeFileArguments up -d openbao-1 openbao-2 openbao-3
$openBaoStartupExitCode = $LASTEXITCODE
Remove-OpenBaoInitContainers
if ($openBaoStartupExitCode -ne 0) {
    throw "Failed to start OpenBao. See openbao\README.md for setup steps."
}

foreach ($address in $openBaoAddresses) {
    try {
        $sealStatus = Invoke-RestMethod -Uri "$address/v1/sys/seal-status" -TimeoutSec 10
    } catch {
        throw "OpenBao at $address is unavailable. Check the node logs and follow openbao\README.md."
    }
    if (-not $sealStatus.initialized) {
        throw "OpenBao is not initialized. Follow the initialization steps in openbao\README.md."
    }
    if ($sealStatus.sealed) {
        throw "OpenBao at $address is sealed. Unseal all three nodes using openbao\README.md, then rerun this script."
    }
}

$roleIdLine = Get-Content $envFile | Where-Object { $_ -match '^OPENBAO_ROLE_ID=' } | Select-Object -First 1
if (-not $roleIdLine) {
    throw "OPENBAO_ROLE_ID is missing from .env. Run .\openbao\configure-secrets.ps1 first."
}
$openBaoRoleId = $roleIdLine.Substring("OPENBAO_ROLE_ID=".Length)
$secretIdSecure = Read-Host "OpenBao AppRole Secret ID" -AsSecureString
$secretId = ConvertTo-PlainText $secretIdSecure
try {
    $login = Invoke-OpenBaoRequest -Path "auth/approle/login" -Method "Post" -Body @{
        role_id = $openBaoRoleId
        secret_id = $secretId
    }
} finally {
    $secretId = $null
    $secretIdSecure.Dispose()
}
$openBaoToken = $login.auth.client_token

$discordWebhookUrl = (Invoke-OpenBaoRequest -Path "secret/data/techstack/alertmanager" -Token $openBaoToken).data.data.discord_webhook_url
$mssqlSecrets = (Invoke-OpenBaoRequest -Path "secret/data/techstack/mssql" -Token $openBaoToken).data.data
$initialSaPassword = $mssqlSecrets.initial_sa_password
$msSqlPassword = $mssqlSecrets.new_sa_password
if (-not $discordWebhookUrl -or -not $initialSaPassword -or -not $msSqlPassword) {
    throw "Required OpenBao secret fields are missing. Rerun openbao\configure-secrets.ps1."
}

Write-Host "=> Setting up prometheus alertmanager..." -ForegroundColor $info_color
Copy-Item ./templates/prometheus/alertmanager.template.yml ./prometheus/config/alertmanager.yml
$content = Get-Content ./prometheus/config/alertmanager.yml -Raw
$content = $content.Replace('<replace_me_discord_webhook_url>', $discordWebhookUrl)
Set-Content ./prometheus/config/alertmanager.yml -Value $content -NoNewline

$previousSaPassword = $env:MSSQL_SA_PASSWORD
$env:MSSQL_SA_PASSWORD = $initialSaPassword
try {
docker compose @composeFileArguments up -d mssql
if ($LASTEXITCODE -ne 0) {
    throw "Failed to start SQL Server. Verify the OpenBao SQL password values and container logs."
}

# Wait for SQL Server migration steps, otherwise login will fail
$sleeper = 0.0
$sleeper_increment = 0.33
$retry_count = 0

DO {
    $sleeper += $sleeper_increment
    $line = docker container logs mssql | Where-Object { $_ -like "*Service Broker manager has started.*" }
    $tsdb = docker container logs mssql | Where-Object { $_ -like "*Starting up database 'TechStackDatabase'*" }
    # $running = docker inspect -f '{{.State.Running}}' mssql
    Write-Host "=> Waiting for SQL Server migration steps ($sleeper s)" -ForegroundColor $warning_color
    Start-Sleep $sleeper_increment
    $retry_count++
} Until ($line -or $sleeper -ge 15.0)

$mssql_running = docker inspect -f '{{.State.Running}}' mssql
if ($mssql_running -ne "true") {
    Write-Host "=> MSSQL container is not running. Starting MSSQL container..." -ForegroundColor $warning_color
    docker compose @composeFileArguments up -d mssql
    Start-Sleep -Seconds 10
}

$is_initial_startup = $tsdb -eq $null

if ($is_initial_startup -eq $true) {
    Write-Host "=> Initial startup detected. Waiting additional time for MSSQL to be ready..." -ForegroundColor $warning_color

    $msSqlPasswordIni = $initialSaPassword
    $newUser = "TechStackUser"
    Write-Host "=> creating $newUser..." -ForegroundColor $info_color
    $escapedSqlPassword = $msSqlPassword.Replace("'", "''")

    $query = "USE [master];
    GO
    CREATE LOGIN [$($newUser)] WITH PASSWORD=N'$($escapedSqlPassword)', DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=ON;
    GO
    ALTER SERVER ROLE [sysadmin] ADD MEMBER [$($newUser)];
    GO";

    Invoke-Sqlcmd -Query $query `
        -ServerInstance "localhost,1433" `
        -TrustServerCertificate `
        -Username "sa" `
        -Password $msSqlPasswordIni
        
    Write-Host "=> updating sa password..." -ForegroundColor $info_color
        
    $query = "USE [master]; ALTER LOGIN [sa] WITH PASSWORD=N'$($escapedSqlPassword)', CHECK_EXPIRATION=OFF, CHECK_POLICY=ON;"
        
    Invoke-Sqlcmd -Query $query `
        -ServerInstance "localhost,1433" `
        -TrustServerCertificate `
        -Username $newUser `
        -Password $msSqlPassword
}

# create volumes if needed
$volume = "grafana-storage"
$volume_count = docker volume ls | Where-Object { $_ -like "* $($volume)" }
if ($volume_count.Count -eq 0) {
    Write-Host "Creating docker volume:" $volume -ForegroundColor $info_color
    docker volume create $volume
}

Write-Host "=> Starting docker compose project..." -ForegroundColor $info_color
docker compose @composeFileArguments up -d
$composeStartupExitCode = $LASTEXITCODE
Remove-OpenBaoInitContainers
if ($composeStartupExitCode -ne 0) {
    throw "Failed to start the Docker Compose project."
}
} finally {
    if ($null -eq $previousSaPassword) {
        Remove-Item Env:\MSSQL_SA_PASSWORD -ErrorAction SilentlyContinue
    } else {
        $env:MSSQL_SA_PASSWORD = $previousSaPassword
    }
    $openBaoToken = $null
    $initialSaPassword = $null
    $msSqlPassword = $null
    $discordWebhookUrl = $null
}