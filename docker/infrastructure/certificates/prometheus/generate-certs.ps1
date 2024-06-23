openssl genrsa -out prometheus.key 2048
openssl req -new -key prometheus.key -out prometheus.csr -config prometheus.csr.cnf
openssl x509 -req -in prometheus.csr -signkey prometheus.key -out prometheus.crt -days 365 -extfile prometheus.extfile.cnf

# check the files, the hashes should match
openssl rsa -noout -modulus -in .\prometheus.key | openssl md5
openssl x509 -noout -modulus -in .\prometheus.crt | openssl md5
openssl req -noout -modulus -in .\prometheus.csr | openssl md5
openssl x509 -enddate -noout -in .\prometheus.crt
