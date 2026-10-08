ui = true
api_addr = "https://localhost:8202"
cluster_addr = "https://openbao-2:8201"

listener "tcp" {
  address = "0.0.0.0:8200"
  cluster_address = "0.0.0.0:8201"
  tls_cert_file = "/openbao/tls/server.crt"
  tls_key_file = "/openbao/tls/server.key"
  tls_min_version = "tls12"
}

audit "file" "local" {
  options {
    file_path = "/openbao/audit/audit.log"
    mode = "0600"
    log_raw = "false"
  }
}

storage "raft" {
  path = "/openbao/data"
  node_id = "openbao-2"

  retry_join {
    leader_api_addr = "https://openbao-1:8200"
    leader_ca_cert_file = "/openbao/tls/ca.crt"
    leader_tls_servername = "openbao-1"
  }
}