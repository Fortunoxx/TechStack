# Documentation
[k6 docs](https://k6.io/docs/get-started/running-k6/)

```powershell
cat .\examples\ewoks.js | docker run --rm -i grafana/k6 run -
```
or
```powershell
docker compose up -d influxdb grafana
docker compose run k6 run /scripts/ewoks.js
```

# Configuration
## Grafana
- add a datasource to InfluxDB:
  - http://host.docker.internal:8086
  - DatabaseName: k6
- Import Dashboard 2587
  - bind datasource influxdb
- this is also scripted now in the /grafana folder