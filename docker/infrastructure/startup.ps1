#Requires -Version 7.0
# Single entry point for the local infrastructure:
#   1. installs / starts both OpenBao clusters (scripts\install-openbao.ps1)
#   2. makes sure the application secrets exist (scripts\configure-secrets.ps1)
#   3. reads them through AppRole and starts the rest of the stack
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$info_color = "Green"
$warning_color = "Yellow"

. (Join-Path $PSScriptRoot "scripts\openbao-common.ps1")

Write-Host "=> Preparing docker compose project..." -ForegroundColor $info_color

# Ensure .env file has REGISTRY setting
$envFile = ".env"
$registryLine = "REGISTRY=cr.jaegertracing.io/"
if (Test-Path $envFile) {
    if (-not ((Get-Content $envFile) -match "^REGISTRY=")) {
        Write-Host "=> Adding REGISTRY to .env file..." -ForegroundColor $info_color
        Add-Content -Path $envFile -Value $registryLine
    }
} else {
    Write-Host "=> Creating .env file with REGISTRY..." -ForegroundColor $info_color
    Set-Content -Path $envFile -Value $registryLine
}

if (-not (Get-Module -ListAvailable -Name SqlServer)) {
    Install-Module SqlServer -Scope CurrentUser -Force
}
Import-Module SqlServer

& (Join-Path $PSScriptRoot "scripts\install-openbao.ps1")
& (Join-Path $PSScriptRoot "scripts\configure-secrets.ps1")

Initialize-BaoClient
$roleIdLine = Get-Content $envFile | Where-Object { $_ -match '^OPENBAO_ROLE_ID=' } | Select-Object -First 1
$secretId = (Get-Content -LiteralPath $AppRoleSecretIdFile -Raw).Trim()
$login = Invoke-BaoCluster -Nodes $MainNodes -Path "auth/approle/login" -Method "POST" -Body @{
    role_id = $roleIdLine.Substring("OPENBAO_ROLE_ID=".Length)
    secret_id = $secretId
}
$openBaoToken = $login.auth.client_token
$secretId = $null

$discordWebhookUrl = (Invoke-BaoCluster -Nodes $MainNodes -Path "secret/data/techstack/alertmanager" -Token $openBaoToken).data.data.discord_webhook_url
$mssqlSecrets = (Invoke-BaoCluster -Nodes $MainNodes -Path "secret/data/techstack/mssql" -Token $openBaoToken).data.data
$initialSaPassword = $mssqlSecrets.initial_sa_password
$msSqlPassword = $mssqlSecrets.new_sa_password
if (-not $discordWebhookUrl -or -not $initialSaPassword -or -not $msSqlPassword) {
    throw "Required OpenBao secret fields are missing. Run scripts\configure-secrets.ps1 -Reset."
}

Write-Host "=> Setting up prometheus alertmanager..." -ForegroundColor $info_color
Copy-Item ./templates/prometheus/alertmanager.template.yml ./prometheus/config/alertmanager.yml
$content = Get-Content ./prometheus/config/alertmanager.yml -Raw
$content = $content.Replace('<replace_me_discord_webhook_url>', $discordWebhookUrl)
Set-Content ./prometheus/config/alertmanager.yml -Value $content -NoNewline

$previousSaPassword = $env:MSSQL_SA_PASSWORD
$env:MSSQL_SA_PASSWORD = $initialSaPassword
try {
    docker compose up -d mssql
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to start SQL Server. Verify the OpenBao SQL password values and container logs."
    }

    # Wait for SQL Server migration steps, otherwise login will fail
    $sleeper = 0.0
    $sleeper_increment = 0.33

    DO {
        $sleeper += $sleeper_increment
        $line = docker container logs mssql | Where-Object { $_ -like "*Service Broker manager has started.*" }
        $tsdb = docker container logs mssql | Where-Object { $_ -like "*Starting up database 'TechStackDatabase'*" }
        Write-Host "=> Waiting for SQL Server migration steps ($sleeper s)" -ForegroundColor $warning_color
        Start-Sleep $sleeper_increment
    } Until ($line -or $sleeper -ge 15.0)

    $mssql_running = docker inspect -f '{{.State.Running}}' mssql
    if ($mssql_running -ne "true") {
        Write-Host "=> MSSQL container is not running. Starting MSSQL container..." -ForegroundColor $warning_color
        docker compose up -d mssql
        Start-Sleep -Seconds 10
    }

    $is_initial_startup = $tsdb -eq $null

    if ($is_initial_startup -eq $true) {
        Write-Host "=> Initial startup detected. Waiting additional time for MSSQL to be ready..." -ForegroundColor $warning_color

        $newUser = "TechStackUser"
        Write-Host "=> creating $newUser..." -ForegroundColor $info_color
        $escapedSqlPassword = $msSqlPassword.Replace("'", "''")

        $query = "USE [master];
CREATE LOGIN [$($newUser)] WITH PASSWORD=N'$($escapedSqlPassword)', DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=ON;
ALTER SERVER ROLE [sysadmin] ADD MEMBER [$($newUser)];";

        Invoke-Sqlcmd -Query $query `
            -ServerInstance "localhost,1433" `
            -TrustServerCertificate `
            -Username "sa" `
            -Password $initialSaPassword
    }

    # create volumes if needed
    $volume = "grafana-storage"
    $volume_count = docker volume ls | Where-Object { $_ -like "* $($volume)" }
    if ($volume_count.Count -eq 0) {
        Write-Host "Creating docker volume:" $volume -ForegroundColor $info_color
        docker volume create $volume
    }

    Write-Host "=> Starting docker compose project..." -ForegroundColor $info_color
    docker compose up -d
    $composeStartupExitCode = $LASTEXITCODE
    Remove-VolumeInitContainer
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
