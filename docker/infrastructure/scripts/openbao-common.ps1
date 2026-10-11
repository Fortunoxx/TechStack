# Shared helpers for the OpenBao scripts. Dot-source this file: . "$PSScriptRoot\openbao-common.ps1"

$ErrorActionPreference = "Stop"

$InfraDir = Split-Path $PSScriptRoot -Parent
$BaoDir = Join-Path $InfraDir "openbao"
$TlsDir = Join-Path $BaoDir "tls"
$SecretsDir = Join-Path $BaoDir "secrets"
$CaPath = Join-Path $TlsDir "ca.crt"

$TransitInitFile = Join-Path $SecretsDir "transit-init.json"
$MainInitFile = Join-Path $SecretsDir "main-init.json"
$SealEnvFile = Join-Path $SecretsDir "transit-seal.env"
$AppRoleSecretIdFile = Join-Path $SecretsDir "approle-secret-id.txt"

$TransitNodes = @(
    @{ Name = "openbao-transit-1"; Address = "https://localhost:8210" },
    @{ Name = "openbao-transit-2"; Address = "https://localhost:8212" },
    @{ Name = "openbao-transit-3"; Address = "https://localhost:8214" }
)
$MainNodes = @(
    @{ Name = "openbao-1"; Address = "https://localhost:8200" },
    @{ Name = "openbao-2"; Address = "https://localhost:8202" },
    @{ Name = "openbao-3"; Address = "https://localhost:8204" }
)

function Write-Step {
    param([string]$Message)
    Write-Host "=> $Message" -ForegroundColor Green
}

function Initialize-BaoClient {
    if (-not (Test-Path -LiteralPath $CaPath)) {
        throw "The local OpenBao CA is missing: $CaPath. Run scripts\generate-certs.ps1."
    }
    if (-not ("BaoClient" -as [type])) {
        # Validates server certificates against the project CA only, so Windows does not need to trust it.
        Add-Type -TypeDefinition @"
using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public class BaoResponse
{
    public int Status;
    public string Body;
}

public static class BaoClient
{
    private static HttpClient _client;

    public static void Initialize(string caPath)
    {
        var ca = X509CertificateLoader.LoadCertificateFromFile(caPath);
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
        {
            if (cert == null || (errors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
            {
                return false;
            }
            using (var custom = new X509Chain())
            {
                custom.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                custom.ChainPolicy.CustomTrustStore.Add(ca);
                custom.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                return custom.Build(cert);
            }
        };
        _client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
    }

    public static BaoResponse Send(string method, string url, string token, string body)
    {
        using (var request = new HttpRequestMessage(new HttpMethod(method), url))
        {
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("X-Vault-Token", token);
            }
            if (body != null)
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }
            using (var response = _client.SendAsync(request).GetAwaiter().GetResult())
            {
                return new BaoResponse
                {
                    Status = (int)response.StatusCode,
                    Body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                };
            }
        }
    }
}
"@
    }
    [BaoClient]::Initialize($CaPath)
}

# Returns the parsed response, or $null for empty bodies and (with -AllowError) HTTP errors.
function Invoke-Bao {
    param(
        [Parameter(Mandatory)][string]$Address,
        [Parameter(Mandatory)][string]$Path,
        [string]$Method = "GET",
        [string]$Token,
        [object]$Body,
        [switch]$AllowError
    )

    $json = if ($null -ne $Body) { $Body | ConvertTo-Json -Depth 8 -Compress } else { $null }
    $response = [BaoClient]::Send($Method, "$Address/v1/$Path", $Token, $json)
    if ($response.Status -ge 400) {
        if ($AllowError) { return $null }
        throw "OpenBao $Method $Path returned HTTP $($response.Status): $($response.Body)"
    }
    if ([string]::IsNullOrWhiteSpace($response.Body)) { return $null }
    $response.Body | ConvertFrom-Json
}

# Tries each node in turn; standby nodes forward requests to the active node.
function Invoke-BaoCluster {
    param(
        [Parameter(Mandatory)][hashtable[]]$Nodes,
        [Parameter(Mandatory)][string]$Path,
        [string]$Method = "GET",
        [string]$Token,
        [object]$Body
    )

    $lastError = $null
    foreach ($node in $Nodes) {
        try {
            return Invoke-Bao -Address $node.Address -Path $Path -Method $Method -Token $Token -Body $Body
        } catch {
            $lastError = $_.Exception.Message
        }
    }
    throw "No OpenBao node accepted the request. Last error: $lastError"
}

function Get-BaoStatus {
    param([string]$Address)
    try { Invoke-Bao -Address $Address -Path "sys/seal-status" } catch { $null }
}

function Wait-Until {
    param(
        [Parameter(Mandatory)][scriptblock]$Condition,
        [Parameter(Mandatory)][string]$Description,
        [int]$TimeoutSeconds = 90
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        try { if (& $Condition) { return } } catch { }
        Start-Sleep -Seconds 2
    } while ((Get-Date) -lt $deadline)
    throw "Timed out after $TimeoutSeconds seconds waiting for: $Description"
}

function Wait-BaoReachable {
    param([hashtable]$Node)
    Wait-Until -Description "$($Node.Name) to accept connections" -Condition { $null -ne (Get-BaoStatus $Node.Address) }
    Get-BaoStatus $Node.Address
}

# Standby nodes forward requests to the leader, so any node with a known leader is usable.
function Wait-BaoLeader {
    param([hashtable]$Node)
    Wait-Until -Description "$($Node.Name) to report an active leader" -Condition {
        -not [string]::IsNullOrEmpty((Invoke-Bao -Address $Node.Address -Path "sys/leader").leader_address)
    }
}

function Wait-BaoUnsealed {
    param([hashtable]$Node, [int]$TimeoutSeconds = 120)
    Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "$($Node.Name) to be initialized and unsealed" -Condition {
        $status = Get-BaoStatus $Node.Address
        $status -and $status.initialized -and -not $status.sealed
    }
}

function Protect-SecretFile {
    param([string]$Path)
    if ($IsWindows) {
        & icacls $Path /inheritance:r /grant:r "$($env:USERNAME):(F)" | Out-Null
    } else {
        & chmod 600 $Path
    }
}

function Read-InitFile {
    param([string]$Path, [string]$What)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "$What file not found: $Path. If the cluster was initialized before, its keys are lost; remove its volumes and rerun startup.ps1."
    }
    Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
}

function Save-InitFile {
    param([string]$Path, [object]$Response)
    New-Item -ItemType Directory -Path (Split-Path $Path -Parent) -Force | Out-Null
    # Write the new keys first: they exist nowhere else once sys/init has returned.
    $pending = "$Path.new"
    $Response | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $pending -Encoding ascii
    Protect-SecretFile -Path $pending
    if (Test-Path -LiteralPath $Path) {
        # Stale keys from wiped volumes; keep them instead of overwriting.
        Move-Item -LiteralPath $Path -Destination "$Path.old-$(Get-Date -Format yyyyMMddHHmmss)"
    }
    Move-Item -LiteralPath $pending -Destination $Path
}

function Invoke-Compose {
    & docker compose --project-directory $InfraDir @args
    if ($LASTEXITCODE -ne 0) {
        throw "docker compose failed: $($args -join ' ')"
    }
}

function Remove-VolumeInitContainer {
    & docker compose --project-directory $InfraDir rm --force openbao-volume-init 2>&1 | Out-Null
}

# Submits unseal shares until the node is open. Retries while a joining node is still becoming ready.
function Open-BaoNode {
    param([hashtable]$Node, [string[]]$Keys, [int]$TimeoutSeconds = 90)

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ($true) {
        try {
            $status = Get-BaoStatus $Node.Address
            if ($status -and $status.initialized -and -not $status.sealed) { return }
            foreach ($key in $Keys | Select-Object -First 3) {
                $status = Invoke-Bao -Address $Node.Address -Path "sys/unseal" -Method "PUT" -Body @{ key = $key }
                if (-not $status.sealed) { return }
            }
        } catch {
            $lastError = $_.Exception.Message
        }
        if ((Get-Date) -gt $deadline) {
            throw "$($Node.Name) could not be unsealed within $TimeoutSeconds seconds. Last error: $lastError"
        }
        Start-Sleep -Seconds 2
    }
}
