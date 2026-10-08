$ErrorActionPreference = "Stop"

$tlsDirectory = Join-Path $PSScriptRoot "tls"
$caKey = Join-Path $tlsDirectory "ca.key"
$caCertificate = Join-Path $tlsDirectory "ca.crt"
$name = "openbao-transit"
$key = Join-Path $tlsDirectory "$name.key"
$request = Join-Path $tlsDirectory "$name.csr"
$certificate = Join-Path $tlsDirectory "$name.crt"
$extensions = Join-Path $tlsDirectory "$name.ext"

if (-not (Test-Path $caKey) -or -not (Test-Path $caCertificate)) {
    throw "The existing local CA is required. Generate it with .\openbao\generate-certs.ps1 first."
}
if ((Test-Path $certificate) -or (Test-Path $key)) {
    throw "A Transit certificate already exists. Back it up or remove it intentionally before regenerating."
}
if (-not (Get-Command openssl -ErrorAction SilentlyContinue)) {
    throw "OpenSSL is required. Install OpenSSL and rerun this script."
}

@"
basicConstraints=CA:FALSE
keyUsage=digitalSignature,keyEncipherment
extendedKeyUsage=serverAuth
subjectAltName=DNS:$name,DNS:localhost,IP:127.0.0.1
"@ | Set-Content -Path $extensions -Encoding ascii

& openssl req -new -newkey rsa:2048 -nodes -keyout $key `
    -out $request -subj "/CN=$name"
if ($LASTEXITCODE -ne 0) { throw "Failed to create the Transit certificate request." }

& openssl x509 -req -in $request -CA $caCertificate -CAkey $caKey `
    -CAcreateserial -out $certificate -days 825 -sha256 -extfile $extensions
if ($LASTEXITCODE -ne 0) { throw "Failed to sign the Transit certificate." }

Remove-Item $request, $extensions
Write-Host "Created the Transit provider TLS certificate in $tlsDirectory."