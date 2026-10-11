#Requires -Version 7.0

$ErrorActionPreference = "Stop"

$fsutil = Get-Command fsutil.exe -ErrorAction Stop
$queryOutput = & $fsutil.Source behavior query encryptpagingfile 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "Failed to query Windows paging-file encryption: $($queryOutput -join ' ')"
}

$queryText = $queryOutput -join [Environment]::NewLine
if ($queryText -notmatch '(?im)^\s*EncryptPagingFile\s*=\s*([01])') {
    throw "Could not determine the Windows paging-file encryption state: $queryText"
}

if ($Matches[1] -eq "1") {
    Write-Host "Windows paging-file encryption is already enabled."
    return
}

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Paging-file encryption is disabled. Rerun this script from an elevated PowerShell session to enable it."
}

& $fsutil.Source behavior set encryptpagingfile 1
if ($LASTEXITCODE -ne 0) {
    throw "Failed to enable Windows paging-file encryption."
}

Write-Host "Windows paging-file encryption is enabled. Restart Windows for the change to take effect."