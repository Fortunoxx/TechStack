# OpenBao for Local Infrastructure

The Compose stack runs three OpenBao 2.7.1 nodes using separate integrated Raft storage and audit volumes. TLS is enabled for API and node traffic, API ports bind to loopback only, and each server runs as the unprivileged `openbao` user. Each node declares the same file audit device in HCL; it writes to that node's private audit volume with mode `0600` and leaves `log_raw` disabled. OpenBao manages this declaratively, so `configure-secrets.ps1` does not enable audit devices through the API. Each node also declares `mem_swappiness: 0`, the Compose equivalent of Docker's `--memory-swappiness=0`, to disable anonymous-page swapping. Verify the effective setting after creation:

```powershell
docker inspect --format '{{.Name}} {{.HostConfig.MemorySwappiness}}' openbao-1 openbao-2 openbao-3
```

The value should be `0`. On the current Docker Compose v5.5.1 / Engine 29.8.2 setup, Compose drops the explicit zero during normalization and Docker reports it as unset (`<nil>`). Do not assume swap is disabled on that setup until this runtime check returns `0`. A persistent fix requires a Compose/Engine path that preserves zero when creating the containers. A persistent, mode-restricted file audit device records API access separately on each node. A narrowly scoped AppRole lets `startup.ps1` read only the Alertmanager webhook and SQL Server passwords.

The volume ownership helpers are one-shot BusyBox containers. `startup.ps1` removes their stopped containers after the full stack starts successfully; their named Raft and audit volumes are retained. When starting services manually with Compose, remove the completed helpers with `docker compose rm --force openbao-1-data-init openbao-1-audit-init openbao-2-data-init openbao-2-audit-init openbao-3-data-init openbao-3-audit-init`.

This is a local development setup, not a production HA deployment. All three nodes share one Docker host, so they do not protect against host or disk failure. Production deployments should place Raft voters across independent failure domains, use a managed auto-unseal mechanism such as a KMS/HSM, configure backups and monitoring, and follow OpenBao's production hardening guidance.

## First-Time Setup

Run commands from `docker/infrastructure` in PowerShell.

For an already-running cluster, restart each node after changing its HCL config and unseal it before restarting the next node. The node will load the declarative audit stanza during startup.

1. Generate a local CA and one TLS certificate per node:

   ```powershell
   .\openbao\generate-certs.ps1
   ```

   The script requires OpenSSL. The generated `openbao/tls` directory is Git-ignored. Keep `ca.key` private. Trust the local CA for the current Windows user so PowerShell validates the OpenBao HTTPS certificates:

   ```powershell
   Import-Certificate -FilePath .\openbao\tls\ca.crt -CertStoreLocation Cert:\CurrentUser\Root
   ```

2. Start the first node and initialize the Raft cluster:

   ```powershell
   docker compose up -d openbao-1
   docker compose exec -e BAO_ADDR=https://localhost:8200 -e BAO_CACERT=/openbao/tls/ca.crt openbao-1 bao operator init -key-shares=5 -key-threshold=3
   ```

   Securely record the five unseal-key shares and root token outside this repository. Do not put them in `.env`, scripts, or source control.

3. Unseal node 1 by supplying three distinct shares. The command prompts for each share:

   ```powershell
   docker compose exec -e BAO_ADDR=https://localhost:8200 -e BAO_CACERT=/openbao/tls/ca.crt openbao-1 bao operator unseal
   ```

   Run that command three times, entering a different share each time. Then start nodes 2 and 3; their Raft `retry_join` settings join them to node 1:

   ```powershell
   docker compose up -d openbao-2 openbao-3
   docker compose exec -e BAO_ADDR=https://localhost:8200 -e BAO_CACERT=/openbao/tls/ca.crt openbao-1 bao login
   docker compose exec -e BAO_ADDR=https://localhost:8200 -e BAO_CACERT=/openbao/tls/ca.crt openbao-1 bao operator raft list-peers
   ```

   Enter the root token at the login prompt. The startup AppRole is intentionally not authorized to inspect Raft configuration.

4. Unseal each follower with three distinct shares as well. Use the same command three times per node, replacing the service name and API port:

   ```powershell
   docker compose exec -e BAO_ADDR=https://localhost:8200 -e BAO_CACERT=/openbao/tls/ca.crt openbao-2 bao operator unseal
   docker compose exec -e BAO_ADDR=https://localhost:8200 -e BAO_CACERT=/openbao/tls/ca.crt openbao-3 bao operator unseal
   ```

   Each Raft node must independently receive the unseal threshold. After a container restart, unseal all three nodes again.

5. Configure the KV v2 mount, startup policy, AppRole, and secret values:

   ```powershell
   .\openbao\configure-secrets.ps1
   ```

   The script prompts securely for the root token and secret values. It stores only the non-secret AppRole Role ID in the ignored `.env` file and displays the generated Secret ID once. Save the Secret ID in a password manager; do not add it to `.env`. Secret IDs expire after 30 days; rerun this script to rotate them. Audit records are stored in per-node volumes `openbao-1-audit`, `openbao-2-audit`, and `openbao-3-audit`.

6. Run the infrastructure startup script. It starts the OpenBao nodes, verifies that they are initialized and unsealed, prompts for the AppRole Secret ID, reads the secrets over verified TLS, then starts the rest of the stack:

   ```powershell
   .\startup.ps1
   ```

`startup.ps1` passes the initial SQL Server SA password to the container through its environment only for that PowerShell run. Docker administrators can still inspect container environment variables. The generated Alertmanager configuration contains the webhook URL and is Git-ignored.

## Secret Paths

- `secret/techstack/alertmanager`: `discord_webhook_url`
- `secret/techstack/mssql`: `initial_sa_password`, `new_sa_password`

The startup AppRole has read-only access to these two KV v2 data paths and receives a short-lived token. The initial SQL password is used only when initializing an empty SQL data volume; `new_sa_password` is applied during that initialization.

## Operations

- Check the cluster: first run `docker compose exec -e BAO_ADDR=https://localhost:8200 -e BAO_CACERT=/openbao/tls/ca.crt openbao-1 bao login` and enter the root token at the prompt; then run `docker compose exec -e BAO_ADDR=https://localhost:8200 -e BAO_CACERT=/openbao/tls/ca.crt openbao-1 bao operator raft list-peers`. The startup AppRole cannot inspect Raft configuration.
- Back up OpenBao using its documented Raft snapshot procedure before upgrades or volume changes.
- Protect and rotate all per-node audit volumes with the Raft data; audit logs can contain sensitive request metadata even though raw secret values are not logged.
- Never use `docker compose down -v` unless intentionally destroying the OpenBao data volumes.
- If the local CA or node certificates need rotation, follow the OpenBao TLS rotation procedure; do not overwrite existing TLS files with `generate-certs.ps1`.

See the [OpenBao Raft](https://openbao.org/docs/configuration/storage/raft/), [HA](https://openbao.org/docs/concepts/ha/), [AppRole](https://openbao.org/docs/auth/approle/), and [seal/unseal](https://openbao.org/docs/concepts/seal/) documentation.