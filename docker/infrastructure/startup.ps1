$param2 = @{
    Method = "Get"
    Uri = "http://localhost:8200/v1/secret/data/url"
    ContentType = "application/json"}

$headers = @{
'X-Vault-Token' = 'root'}

$secretVal = Invoke-RestMethod @param2 -Headers $headers #-OutFile output.json
$webhook_url = $secretVal.data.data.webhook_url
$webhook_url

$env:DISCORD_WEBHOOK_URL='http://www.abc.de'

docker-compose up -d 