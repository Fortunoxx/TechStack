$info_color = "Green"
$warning_color = "Yellow"
$highlight_color = "Magenta"

Write-Host "=> Stopping docker compose project..." -ForegroundColor $info_color
docker compose down

Install-Module Mdbc 
Import-Module Mdbc 
Install-Module SqlServer
Import-Module SqlServer 

Write-Host "=> Starting mongodb to get configurations..." -ForegroundColor $info_color
docker compose up -d mongodb

Write-Host "=> Setting up prometheus alertmanager..." -ForegroundColor $info_color
Connect-Mdbc . docker-compose config
$data = Get-MdbcData @{key="prometheus.alertmanager.discord_webhook_url"}
Write-Host "=> updating discord webhook url:" -ForegroundColor $info_color
Write-Host "=>" $data.value -ForegroundColor $highlight_color 

Copy-Item ./templates/prometheus/alertmanager.template.yml ./prometheus/config/alertmanager.yml
(Get-Content ./prometheus/config/alertmanager.yml).Replace('<replace_me_discord_webhook_url>', $data.value) | Set-Content ./prometheus/config/alertmanager.yml

Write-Host "=> Starting mssql to apply credentials..." -ForegroundColor $info_color
docker compose up -d mssql

# Wait for SQL Server migration steps, otherwise login will fail
$sleeper = 0.0
$sleeper_increment = 0.33
DO 
{
    $sleeper += $sleeper_increment
    $line = docker container logs mssql | Where-Object { $_ -like "*Service Broker manager has started.*" }
    # $running = docker inspect -f '{{.State.Running}}' mssql
    Write-Host "=> Waiting for SQL Server migration steps ($sleeper s)" -ForegroundColor $warning_color
    sleep $sleeper_increment
} Until ($line -or $sleeper -ge 15.0)

$mssql_pw_ini_str = (docker inspect -f '{{.Config.Env}}' mssql).Split(" ") | Where-Object { $_ -like "*MSSQL_SA_PASSWORD*" } | Select -First 1
$mssql_pw_ini = $mssql_pw_ini_str.Split("=")[1] # old password, we want to change that later
$mssql_pw = (Get-MdbcData @{key="mssql.new_sa_password"}).value # new and secure password

$new_user = "TechStackUser"
Write-Host "=> creating $new_user..." -ForegroundColor $info_color

$query = "USE [master];
GO
CREATE LOGIN [$($new_user)] WITH PASSWORD=N'$($mssql_pw)', DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=ON;
GO
ALTER SERVER ROLE [sysadmin] ADD MEMBER [$($new_user)];
GO";

Invoke-Sqlcmd -Query $query `
    -ServerInstance "localhost,1433" `
    -Username "sa" `
    -Password $mssql_pw_ini `
    -TrustServerCertificate

Write-Host "=> updating sa password:" -ForegroundColor $info_color
Write-Host "=>" $mssql_pw -ForegroundColor $highlight_color

$query = "USE [master]; ALTER LOGIN [sa] WITH PASSWORD=N'$($mssql_pw)';"

Invoke-Sqlcmd -Query $query `
    -ServerInstance "localhost,1433" `
    -Username $new_user `
    -Password $mssql_pw `
    -TrustServerCertificate

Write-Host "=> Starting docker compose project..." -ForegroundColor $info_color
docker compose up -d 