ui = true
api_addr = "https://openbao-transit:8200"
cluster_addr = "https://openbao-transit:8201"

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
  node_id = "openbao-transit"
}