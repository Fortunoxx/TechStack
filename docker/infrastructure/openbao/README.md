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

   To unseal all three nodes after a restart, store the five shares one per line in the Git-ignored `openbao/secrets/secrets.txt` file, then run:

   ```powershell
   .\openbao\unseal-cluster.ps1
   ```

   The script submits the first three shares to each initialized sealed node, which meets the three-share threshold. It skips nodes that are already unsealed and never prints the shares. Run it from `docker/infrastructure` after the nodes are running and the local OpenBao CA is trusted by Windows.

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

## Transit Auto-Unseal

To provision the local Transit provider and migrate the existing three-node
cluster, run this from `docker/infrastructure`:

```powershell
.\openbao\setup-transit.ps1
```

The script requires the five main-cluster shares in the ignored
`openbao/secrets/secrets.txt`, creates and account-restricts a separate Transit
share file, prompts securely for the main root token, saves a protected Raft
snapshot, and requires typing `MIGRATE` before restarting nodes. It creates the
seal token file and enables the Transit profile only after all three main nodes
verify unsealed with the Transit seal. Keep a protected copy of the snapshot
outside the repository.
At the root-token prompt, use the root token saved when the main three-node
cluster was initialized. `transit-seal.env` contains a separate, restricted
encrypt/decrypt token; it cannot authorize a Raft snapshot or administer the
main cluster. On reruns, the script validates and reuses that existing seal
token instead of requiring the Transit provider root token again.

The three-node cluster can use a separate `openbao-transit` service as its
auto-unseal provider. The provider has its own Raft and audit volumes, TLS
certificate, and Shamir keys. It is intentionally a separate OpenBao instance:
the cluster cannot use its own Transit engine to unseal itself. This local
provider is a single node on the same Docker host, so it is a development
convenience, not an independent failure domain. It must be manually unsealed
after every restart. For production, use a separately protected HA Transit
provider or an external KMS/HSM.

The existing three-node cluster was initialized with Shamir (5 shares,
threshold 3). Do not add the Transit seal stanza to the active node configs or
restart all nodes together. Seal migration requires a backup, both seals to be
available, and planned downtime.

### Prepare Transit

Run these steps from `docker/infrastructure` in PowerShell. Generate a
certificate signed by the existing local CA, then start the provider:

```powershell
.\openbao\generate-transit-cert.ps1
docker compose --profile transit up -d openbao-transit
docker compose --profile transit exec -e BAO_ADDR=https://localhost:8210 -e BAO_CACERT=/openbao/tls/ca.crt openbao-transit bao operator init -key-shares=5 -key-threshold=3
docker compose --profile transit rm --force openbao-transit-data-init openbao-transit-audit-init
```

Securely record the provider's five Shamir shares and root token outside this
repository. Unseal the provider with three distinct shares by running this
command three times:

```powershell
docker compose --profile transit exec -e BAO_ADDR=https://localhost:8210 -e BAO_CACERT=/openbao/tls/ca.crt openbao-transit bao operator unseal
```

Log in interactively with the provider root token, then configure the narrowly
scoped seal key and policy:

```powershell
docker compose --profile transit exec -e BAO_ADDR=https://localhost:8210 -e BAO_CACERT=/openbao/tls/ca.crt openbao-transit bao login
docker compose --profile transit exec -e BAO_ADDR=https://localhost:8210 -e BAO_CACERT=/openbao/tls/ca.crt openbao-transit bao secrets enable -path=transit transit
docker compose --profile transit exec -e BAO_ADDR=https://localhost:8210 -e BAO_CACERT=/openbao/tls/ca.crt openbao-transit bao write -f transit/keys/techstack-seal
docker compose --profile transit exec -e BAO_ADDR=https://localhost:8210 -e BAO_CACERT=/openbao/tls/ca.crt openbao-transit bao policy write transit-seal /openbao/transit-seal-policy.hcl
docker compose --profile transit exec -e BAO_ADDR=https://localhost:8210 -e BAO_CACERT=/openbao/tls/ca.crt openbao-transit bao token create -orphan -policy=transit-seal -period=24h -no-default-policy
```

The last command returns the token used by the three main nodes. Put it in the
ignored file `openbao/transit-seal.env` as `BAO_TOKEN=<token>`. Restrict that
file to your Windows account with an appropriate ACL; do not commit it or put
the token in HCL. The token is still visible to Docker administrators through
container inspection. Keep the Transit key and its old key versions: existing
seal data may require them for decryption.

### Migrate the Existing Cluster

1. Unseal all three existing nodes with three of their current Shamir shares and
   confirm the Raft cluster is healthy. Take a Raft snapshot and copy it out of
   the container to protected storage before continuing:

   ```powershell
   docker compose exec -e BAO_ADDR=https://localhost:8200 -e BAO_CACERT=/openbao/tls/ca.crt openbao-1 bao operator raft snapshot save /tmp/raft-before-transit.snap
   docker cp openbao-1:/tmp/raft-before-transit.snap .\openbao\raft-before-transit.snap
   ```

2. Migrate standby nodes one at a time. Start with `openbao-2`, then repeat for
   `openbao-3`. After the three migration shares are accepted, leave that node
   running and continue to the next standby. It may not report normal health
   while the active node still has the Shamir seal configuration:

   ```powershell
   docker compose stop openbao-2
   docker compose --profile transit -f docker-compose.yml -f docker-compose.openbao-transit.yml up -d --no-deps --force-recreate openbao-2
   docker compose --profile transit -f docker-compose.yml -f docker-compose.openbao-transit.yml exec -e BAO_ADDR=https://localhost:8202 -e BAO_CACERT=/openbao/tls/ca.crt openbao-2 bao operator unseal -migrate
   ```

   Run the final `unseal -migrate` command three times, entering a different
   existing Shamir share each time. For `openbao-3`, replace the service and
   local API port with `openbao-3` and `8204`.

3. Identify the active node with `bao status` and step it down. Wait until the
   migrated standby nodes have elected a leader and report Transit seal status
   before restarting the former active node using the Transit overlay. Replace
   `openbao-1` below with the actual former active node. It should auto-unseal
   and rejoin; do not run `unseal -migrate` on this former active node:

   ```powershell
   docker compose --profile transit exec -e BAO_ADDR=https://localhost:8200 -e BAO_CACERT=/openbao/tls/ca.crt openbao-1 bao operator step-down
   docker compose stop openbao-1
   docker compose --profile transit -f docker-compose.yml -f docker-compose.openbao-transit.yml up -d --no-deps --force-recreate openbao-1
   ```

4. After verifying all nodes are unsealed and healthy, create the local
   activation marker. `startup.ps1` will then use the Transit overlay and check
   that the provider is initialized and unsealed before starting the main
   cluster:

   ```powershell
   New-Item -ItemType File .\openbao\transit-seal.enabled
   ```

The former Shamir shares become recovery keys after migration. Keep them
securely; they authorize recovery operations but cannot unseal the main cluster
if the Transit provider or its key is unavailable. Start and manually unseal
`openbao-transit` before running `startup.ps1` after a host or provider restart.

See OpenBao's [Transit seal](https://openbao.org/docs/configuration/seal/transit/)
and [seal migration](https://openbao.org/docs/concepts/seal/#seal-migration)
guidance for the authoritative procedure and recovery requirements.