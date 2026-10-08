$ErrorActionPreference = "Stop"

$tlsDirectory = Join-Path $PSScriptRoot "tls"
if (Test-Path $tlsDirectory) {
    throw "TLS directory already exists at '$tlsDirectory'. Back it up or remove it intentionally before regenerating certificates."
}

if (-not (Get-Command openssl -ErrorAction SilentlyContinue)) {
    throw "OpenSSL is required. Install OpenSSL and rerun this script."
}

New-Item -ItemType Directory -Path $tlsDirectory | Out-Null
$caKey = Join-Path $tlsDirectory "ca.key"
$caCertificate = Join-Path $tlsDirectory "ca.crt"

& openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes `
    -keyout $caKey -out $caCertificate -subj "/CN=TechStack OpenBao Development CA"
if ($LASTEXITCODE -ne 0) { throw "Failed to create the local OpenBao CA." }

foreach ($node in 1..3) {
    $name = "openbao-$node"
    $key = Join-Path $tlsDirectory "node-$node.key"
    $request = Join-Path $tlsDirectory "node-$node.csr"
    $certificate = Join-Path $tlsDirectory "node-$node.crt"
    $extensions = Join-Path $tlsDirectory "node-$node.ext"

    @"
basicConstraints=CA:FALSE
keyUsage=digitalSignature,keyEncipherment
extendedKeyUsage=serverAuth
subjectAltName=DNS:$name,DNS:localhost,IP:127.0.0.1
"@ | Set-Content -Path $extensions -Encoding ascii

    & openssl req -new -newkey rsa:2048 -nodes -keyout $key `
        -out $request -subj "/CN=$name"
    if ($LASTEXITCODE -ne 0) { throw "Failed to create the certificate request for $name." }

    & openssl x509 -req -in $request -CA $caCertificate -CAkey $caKey `
        -CAcreateserial -out $certificate -days 825 -sha256 -extfile $extensions
    if ($LASTEXITCODE -ne 0) { throw "Failed to sign the certificate for $name." }

    Remove-Item $request, $extensions
}

Write-Host "Created local OpenBao TLS certificates in $tlsDirectory."
Write-Host "Keep ca.key private; the generated TLS directory is excluded from Git."