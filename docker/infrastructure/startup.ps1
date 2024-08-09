Copy-Item ./templates/.env.template ./.env

Install-Module Mdbc 
Import-Module Mdbc 
Install-Module SqlServer
Import-Module SqlServer 

docker compose down

docker compose up -d mongodb

Connect-Mdbc . docker-compose config
$data = Get-MdbcData @{key="prometheus.alertmanager.discord_webhook_url"}
Write-Host "=> updating discord webhook url:" 
Write-Host -ForegroundColor Magenta "=>" $data.value

Copy-Item ./templates/prometheus/alertmanager.template.yml ./prometheus/config/alertmanager.yml
(Get-Content ./prometheus/config/alertmanager.yml).Replace('<replace_me_discord_webhook_url>', $data.value) | Set-Content ./prometheus/config/alertmanager.yml

$envdata = Get-MdbcData @{key="envdata.mongodb.exporter_password"}
Write-Host "=> updating mongodb password:"
Write-Host -ForegroundColor Magenta "=>" $envdata.value
# Copy-Item ./templates/.env.template ./.env
(Get-Content ./.env).Replace('<replace_me_mongo_exporter_password>', $envdata.value) | Set-Content ./.env

docker compose up -d mssql

$mssql_pw = Get-MdbcData @{key="mssql.new_sa_password"}
Write-Host "=> updating sql server password:"
Write-Host -ForegroundColor Magenta "=>" $mssql_pw.value

$pwd = "P@ssw0rd1!" # old password, we want to change that now
$query = "USE [master]; ALTER LOGIN [sa] WITH PASSWORD=N'$($mssql_pw.value)';"
$query

Invoke-Sqlcmd -Query $query -ServerInstance "localhost,1433" -Username "sa" -Password $pwd -TrustServerCertificate

docker compose up -d 