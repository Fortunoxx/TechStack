$info_color = "Green"
$warning_color = "Yellow"
$highlight_color = "Magenta"

Write-Host "=> Stopping docker compose project..." -ForegroundColor $info_color
docker compose down

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

if (-not (Get-Module -ListAvailable -Name Mdbc)) {
    Install-Module Mdbc -Force
}
if (-not (Get-Module -ListAvailable -Name SqlServer)) {
    Install-Module SqlServer -Force
}
Import-Module Mdbc 
Import-Module SqlServer 

Write-Host "=> Starting mongodb to get configurations..." -ForegroundColor $info_color
docker compose up -d mongodb

Connect-Mdbc . docker-compose config

Write-Host "=> Setting up prometheus alertmanager..." -ForegroundColor $info_color
Connect-Mdbc . docker-compose config
$data = Get-MdbcData @{key = "prometheus.alertmanager.discord_webhook_url" }
Write-Host "=> updating discord webhook url:" -ForegroundColor $info_color
Write-Host "=>" $data.value -ForegroundColor $highlight_color 

Copy-Item ./templates/prometheus/alertmanager.template.yml ./prometheus/config/alertmanager.yml
$content = Get-Content ./prometheus/config/alertmanager.yml
$content = $content -replace '<replace_me_discord_webhook_url>', $data.value
Set-Content ./prometheus/config/alertmanager.yml -Value $content

docker compose up -d mssql

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
if ($mssql_running -eq $false) {
    Write-Host "=> MSSQL container is not running. Starting MSSQL container..." -ForegroundColor $warning_color
    docker compose up -d mssql
    Start-Sleep -Seconds 10
}

$is_initial_startup = $tsdb -eq $null

if ($is_initial_startup -eq $true) {
    Write-Host "=> Initial startup detected. Waiting additional time for MSSQL to be ready..." -ForegroundColor $warning_color

    $msSqlPasswordIniString = (docker inspect -f '{{.Config.Env}}' mssql).Split(" ") | Where-Object { $_ -like "*MSSQL_SA_PASSWORD*" } | Select -First 1
    $msSqlPasswordIni = $msSqlPasswordIniString.Split("=")[1] # old password, we want to change that later
    $msSqlPassword = (Get-MdbcData @{key = "mssql.new_sa_password" }).value # new and secure password

    $newUser = "TechStackUser"
    Write-Host "=> creating $newUser..." -ForegroundColor $info_color

    $query = "USE [master];
    GO
    CREATE LOGIN [$($newUser)] WITH PASSWORD=N'$($msSqlPassword)', DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=ON;
    GO
    ALTER SERVER ROLE [sysadmin] ADD MEMBER [$($newUser)];
    GO";

    Invoke-Sqlcmd -Query $query `
        -ServerInstance "localhost,1433" `
        -TrustServerCertificate `
        -Username "sa" `
        -Password $msSqlPasswordIni
        
    Write-Host "=> updating sa password:" -ForegroundColor $info_color
    Write-Host "=>" $msSqlPassword -ForegroundColor $highlight_color
        
    $query = "USE [master]; ALTER LOGIN [sa] WITH PASSWORD=N'$($msSqlPassword)', CHECK_EXPIRATION=OFF, CHECK_POLICY=ON;"
        
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
docker compose up -d 