#Requires -Version 7.0
# Creates the local CA and the node certificates. Existing files are never overwritten.
$ErrorActionPreference = "Stop"

$tlsDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) "openbao\tls"
$caKey = Join-Path $tlsDirectory "ca.key"
$caCertificate = Join-Path $tlsDirectory "ca.crt"
$names = @(1..3 | ForEach-Object { "openbao-$_" }) + @(1..3 | ForEach-Object { "openbao-transit-$_" })

function Test-NeedsCertificate {
    param([string]$Name)
    -not ((Test-Path (Join-Path $tlsDirectory "$Name.crt")) -and (Test-Path (Join-Path $tlsDirectory "$Name.key")))
}

$missing = @($names | Where-Object { Test-NeedsCertificate $_ })
if ((Test-Path $caCertificate) -and (Test-Path $caKey) -and $missing.Count -eq 0) {
    return
}

if (-not (Get-Command openssl -ErrorAction SilentlyContinue)) {
    throw "OpenSSL is required to create the TLS certificates. Install it and rerun."
}
New-Item -ItemType Directory -Path $tlsDirectory -Force | Out-Null

if (-not (Test-Path $caCertificate) -or -not (Test-Path $caKey)) {
    Write-Host "=> Creating the local OpenBao CA..." -ForegroundColor Green
    & openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes `
        -keyout $caKey -out $caCertificate -subj "/CN=TechStack OpenBao Development CA" 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Failed to create the local OpenBao CA." }
    $missing = $names
}

foreach ($name in $missing) {
    Write-Host "=> Creating TLS certificate for $name..." -ForegroundColor Green
    $key = Join-Path $tlsDirectory "$name.key"
    $request = Join-Path $tlsDirectory "$name.csr"
    $certificate = Join-Path $tlsDirectory "$name.crt"
    $extensions = Join-Path $tlsDirectory "$name.ext"

    @"
basicConstraints=CA:FALSE
keyUsage=digitalSignature,keyEncipherment
extendedKeyUsage=serverAuth
subjectAltName=DNS:$name,DNS:localhost,IP:127.0.0.1
"@ | Set-Content -Path $extensions -Encoding ascii

    & openssl req -new -newkey rsa:2048 -nodes -keyout $key -out $request -subj "/CN=$name" 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Failed to create the certificate request for $name." }

    & openssl x509 -req -in $request -CA $caCertificate -CAkey $caKey `
        -CAcreateserial -out $certificate -days 825 -sha256 -extfile $extensions 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Failed to sign the certificate for $name." }

    Remove-Item $request, $extensions
}
