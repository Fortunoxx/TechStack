#Requires -Version 7.0
# Unseals the transit cluster from the saved keys (needed after a host or container restart).
# The openbao-1..3 nodes unseal themselves through transit once it is open.
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "openbao-common.ps1")

Initialize-BaoClient
$transitInit = Read-InitFile -Path $TransitInitFile -What "Transit unseal keys"

foreach ($node in $TransitNodes) {
    $status = Get-BaoStatus $node.Address
    if (-not $status) {
        Write-Host "$($node.Name) is not running." -ForegroundColor Yellow
        continue
    }
    if ($status.initialized -and -not $status.sealed) {
        Write-Host "$($node.Name) is already unsealed."
        continue
    }
    Open-BaoNode -Node $node -Keys $transitInit.keys
    Write-Host "$($node.Name) is unsealed." -ForegroundColor Green
}

foreach ($node in $MainNodes) {
    $status = Get-BaoStatus $node.Address
    if (-not $status) {
        Write-Host "$($node.Name) is not running." -ForegroundColor Yellow
        continue
    }
    Wait-BaoUnsealed $node
    Write-Host "$($node.Name) is unsealed (auto-unseal)." -ForegroundColor Green
}
