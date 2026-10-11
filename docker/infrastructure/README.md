# Infrastructure

## Startup

```powershell
.\startup.ps1
```

Requires PowerShell 7, Docker Compose and OpenSSL. The single script installs and unseals both OpenBao clusters (three Raft nodes each, the main cluster auto-unsealed by the transit cluster), reads the application secrets from OpenBao and starts the rest of the stack. On the first run it prompts for the Discord webhook URL and the new SQL Server password; the initial SA password `P@ssw0rd1!` remains in `docker-compose.yml`. Generated unseal keys and admin tokens go to `openbao/secrets/` (Git-ignored).

After a host restart, `.\scripts\unseal-openbao.ps1` unseals the transit nodes. See [openbao/README.md](openbao/README.md) for details.

## Configure dashboards

### speedtest-exporter

[SpeedTest-Exporter](https://docs.miguelndecarvalho.pt/projects/speedtest-exporter/)
use grafana dashboard #13665

### cadvisor

use grafana dashboard #14282 or #19792

### loki

use grafana dashboard #14055

### jaeger

[Jaeger](https://www.jaegertracing.io/docs/1.25/monitoring/)
use grafana dashboard #10001

### blackbox

[Blackbox](https://grafana.com/grafana/dashboards/?search=blackbox)
use grafana dashboard #7587 or #13659

### rabbitmq

[Dashboard #10991](https://grafana.com/grafana/dashboards/10991-rabbitmq-overview/)
