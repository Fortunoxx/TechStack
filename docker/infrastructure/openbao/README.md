# OpenBao for Local Infrastructure

Two OpenBao 2.7.1 clusters, each with three nodes using integrated Raft storage:

| Cluster | Nodes (API port on 127.0.0.1) | Seal | Purpose |
| --- | --- | --- | --- |
| Transit | `openbao-transit-1..3` (8210, 8212, 8214) | Shamir | Holds the `techstack-seal` key used to auto-unseal the main cluster |
| Main | `openbao-1..3` (8200, 8202, 8204) | Transit (auto-unseal) | Stores the application secrets (KV v2 + AppRole) |

`openbao-N` unseals itself through `openbao-transit-N`. Standby transit nodes forward requests to the active node, so the main cluster keeps working while one transit node is down. The seal token is stored in `secrets/transit-seal.env` and injected as `BAO_TOKEN`; it is a periodic orphan token that is renewed on every start.

TLS is enabled for all traffic with a local CA (`tls/`). The scripts validate the certificates against that CA directly, so it does not need to be trusted by Windows. API ports bind to loopback only, each server runs as the unprivileged `openbao` user, and every node writes a file audit log (mode `0600`, `log_raw` off) to its own volume.

This is a local development setup, not a production HA deployment: all nodes share one Docker host.

## Installation

Run `..\startup.ps1` from `docker/infrastructure`. It calls [`scripts/install-openbao.ps1`](../scripts/install-openbao.ps1), which is safe to rerun:

1. Creates the local CA and the node certificates that are missing (requires OpenSSL).
2. Starts the three transit nodes. On the **first install** it initializes them, then unseals all three with the saved keys and creates the transit mount, key, policy and seal token.
3. Starts the three main nodes. On the **first install** it initializes the cluster; the nodes then auto-unseal and join via Raft.
4. Removes the one-shot volume-initializer container.

`startup.ps1` then runs [`scripts/configure-secrets.ps1`](../scripts/configure-secrets.ps1) (prompts once for the Discord webhook URL and the new SQL Server password, only if they are not stored yet) and starts the rest of the stack. The initial SQL Server SA password is `P@ssw0rd1!` in `docker-compose.yml` and is not stored in OpenBao.

## Saved keys (`secrets/`, Git-ignored)

Written on the first install, with access restricted to the current Windows user:

| File | Content |
| --- | --- |
| `transit-init.json` | 5 Shamir unseal keys (threshold 3) and root token of the transit cluster |
| `main-init.json` | 5 recovery keys (threshold 3) and root token of the main cluster |
| `transit-seal.env` | `BAO_TOKEN` used by `openbao-1..3` to reach the transit cluster |
| `approle-secret-id.txt` | Secret ID of the read-only `techstack-startup` AppRole used by `startup.ps1` |

These files are the only copy of the keys. Back them up somewhere safe; losing `transit-init.json` after a restart means the transit cluster, and with it the main cluster, can no longer be unsealed. The directory and `tls/` are excluded by `.gitignore`.

## After a restart

Containers restart automatically (`restart: unless-stopped`) but the transit nodes come back sealed. Either run `..\startup.ps1` again or only:

```powershell
.\scripts\unseal-openbao.ps1
```

It unseals the transit nodes from `transit-init.json`; `openbao-1..3` then unseal themselves.

## Secrets

- `secret/techstack/alertmanager`: `discord_webhook_url`
- `secret/techstack/mssql`: `new_sa_password`

The `techstack-startup` AppRole can only read these two paths and receives a 5 minute token. `startup.ps1` reads the initial SA password from the resolved Compose configuration when it needs to create the SQL login. To change the OpenBao values, run `\.\scripts\configure-secrets.ps1 -Reset`.

## Layout

```text
openbao/
  config/      HCL for the six nodes
  policies/    transit-seal.hcl, startup.hcl
  tls/         generated, Git-ignored
  secrets/     generated, Git-ignored
scripts/       install-openbao, unseal-openbao, configure-secrets, generate-certs, shared helpers
```

## Operations

- Use the admin tokens in `secrets/*.json` for administration, for example `docker compose exec -e BAO_ADDR=https://localhost:8200 -e BAO_CACERT=/openbao/tls/ca.crt -e BAO_TOKEN=<token> openbao-1 bao operator raft list-peers`.
- Back up with the Raft snapshot procedure before upgrades or volume changes. Audit logs live in the `*-audit` volumes and can contain sensitive request metadata.
- Start over: `docker compose down -v openbao-1 openbao-2 openbao-3 openbao-transit-1 openbao-transit-2 openbao-transit-3`, then run `startup.ps1`. Always wipe both clusters together, since the main cluster cannot be unsealed without its transit key. Existing key files are renamed to `*.old-<timestamp>` on the next initialization.
- Do not delete `tls/`; `generate-certs.ps1` only creates what is missing and never overwrites existing files.
- Each node sets `mem_swappiness: 0`. On Docker Compose v5 / Engine 29 the zero may be reported as unset (`<nil>`); check with `docker inspect --format '{{.Name}} {{.HostConfig.MemorySwappiness}}' openbao-1`.

See the OpenBao [Raft](https://openbao.org/docs/configuration/storage/raft/), [Transit seal](https://openbao.org/docs/configuration/seal/transit/), [AppRole](https://openbao.org/docs/auth/approle/) and [seal/unseal](https://openbao.org/docs/concepts/seal/) documentation.

## Windows hardening

OpenBao recommends encrypting Windows paging files so secrets are not exposed if memory is paged to disk. Run `pwsh .\scripts\harden-windows.ps1` from an elevated PowerShell session. The script checks the current setting, enables it if necessary, and reports that Windows must be restarted; it does not reboot automatically. See the [OpenBao installation guidance](https://openbao.org/docs/install/#windows).
