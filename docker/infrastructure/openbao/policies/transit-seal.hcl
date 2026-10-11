path "transit/encrypt/techstack-seal" {
  capabilities = ["update"]
}

path "transit/decrypt/techstack-seal" {
  capabilities = ["update"]
}

# The token is created without the default policy, so it needs these to stay valid.
path "auth/token/lookup-self" {
  capabilities = ["read"]
}

path "auth/token/renew-self" {
  capabilities = ["update"]
}
