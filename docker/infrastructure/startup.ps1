docker-compose down

Install-Module Mdbc 
Import-Module Mdbc 

docker-compose up -d mongodb

Connect-Mdbc . docker-compose config
$data = Get-MdbcData @{key="prometheus.alertmanager.discord_webhook_url"}
Write-Host "=> updating discord webhook url:" 
Write-Host -ForegroundColor Magenta "=>" $data.value

Copy-Item ./templates/prometheus/alertmanager.template.yml ./prometheus/config/alertmanager.yml
(Get-Content ./prometheus/config/alertmanager.yml).Replace('<replace_me_discord_webhook_url>', $data.value) | Set-Content ./prometheus/config/alertmanager.yml

docker-compose up -d 