docker-compose down

Install-Module Mdbc 
Import-Module Mdbc 
Install-Module SqlServer
Import-Module SqlServer 

docker-compose up -d mongodb

Connect-Mdbc . docker-compose config
$data = Get-MdbcData @{key="prometheus.alertmanager.discord_webhook_url"}
Write-Host "=> updating discord webhook url:" 
Write-Host -ForegroundColor Magenta "=>" $data.value

Copy-Item ./templates/prometheus/alertmanager.template.yml ./prometheus/config/alertmanager.yml
(Get-Content ./prometheus/config/alertmanager.yml).Replace('<replace_me_discord_webhook_url>', $data.value) | Set-Content ./prometheus/config/alertmanager.yml

docker-compose up -d mssql

DO 
{
    $running = docker inspect -f '{{.State.Running}}' mssql
    sleep 0.1
    $running
} Until ($running -eq "true")

$mssql_pw_ini = "P@ssw0rd1!" # old password, we want to change that later
$mssql_pw = (Get-MdbcData @{key="mssql.new_sa_password"}).value

Write-Host "=> creating TechStackUser" -ForegroundColor Cyan
$query = "USE [master];
GO
CREATE LOGIN [TechStackUser] WITH PASSWORD=N'$($mssql_pw)', DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=ON;
GO
ALTER SERVER ROLE [sysadmin] ADD MEMBER [TechStackUser];
GO";
Invoke-Sqlcmd -Query $query `
    -ServerInstance "localhost,1433" `
    -Username "sa" `
    -Password $mssql_pw_ini `
    -TrustServerCertificate

Write-Host "=> updating sa password:" -ForegroundColor Cyan
Write-Host -ForegroundColor Magenta "=>" $mssql_pw
$query = "USE [master]; ALTER LOGIN [sa] WITH PASSWORD=N'$($mssql_pw)';"
$query
Invoke-Sqlcmd -Query $query `
    -ServerInstance "localhost,1433" `
    -Username "TechStackUser" `
    -Password $mssql_pw `
    -TrustServerCertificate

docker-compose up -d 