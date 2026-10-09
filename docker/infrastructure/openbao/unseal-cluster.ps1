$ErrorActionPreference = "Stop"

$secretFile = Join-Path $PSScriptRoot "secrets\secrets.txt"
if (-not (Test-Path -LiteralPath $secretFile -PathType Leaf)) {
    throw "Unseal shares file not found: $secretFile"
}

$shares = @(
    Get-Content -LiteralPath $secretFile |
        ForEach-Object { $_.Trim() } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
)
if ($shares.Count -ne 5) {
    throw "Expected exactly five non-empty unseal shares in $secretFile; found $($shares.Count)."
}

$nodes = @(
    @{ Name = "openbao-1"; Address = "https://localhost:8200" },
    @{ Name = "openbao-2"; Address = "https://localhost:8202" },
    @{ Name = "openbao-3"; Address = "https://localhost:8204" }
)

foreach ($node in $nodes) {
    $status = Invoke-RestMethod -Uri "$($node.Address)/v1/sys/seal-status" -TimeoutSec 10
    if (-not $status.initialized) {
        throw "$($node.Name) is not initialized. Initialize the cluster before unsealing it."
    }
    if (-not $status.sealed) {
        Write-Host "$($node.Name) is already unsealed."
        continue
    }

    foreach ($share in $shares | Select-Object -First 3) {
        $body = @{ key = $share } | ConvertTo-Json -Compress
        $status = Invoke-RestMethod `
            -Uri "$($node.Address)/v1/sys/unseal" `
            -Method Post `
            -ContentType "application/json" `
            -Body $body `
            -TimeoutSec 10

        if (-not $status.sealed) {
            break
        }
    }

    if ($status.sealed) {
        throw "$($node.Name) remained sealed after submitting three shares. Check that the file contains shares for this cluster."
    }

    Write-Host "$($node.Name) is unsealed."
}