import { check, sleep } from "k6";
import http from "k6/http";

const isNumeric = (value) => /^\d+$/.test(value);

const default_vus = 5;

const target_vus_env = `${__ENV.TARGET_VUS}`;
const target_vus = isNumeric(target_vus_env)
  ? Number(target_vus_env)
  : default_vus;

export let options = {
  stages: [
    // Ramp-up from 1 to TARGET_VUS virtual users (VUs) in 5s
    { duration: "5s", target: target_vus },

    // Stay at rest on TARGET_VUS VUs for 10s
    { duration: "10s", target: target_vus },

    // Ramp-down from TARGET_VUS to 0 VUs for 5s
    { duration: "5s", target: 0 },
  ],
};

const domain = "http://host.docker.internal:5131/api/Test/anonymous";

export default function () {
  let responses = http.batch([
    ["GET", domain + "/69"],
    ["GET", domain + "/42"],
    ["GET", domain + "/33"],
  ]);

  check(responses[0], { "status is 200": (r) => r.status === 200 });

  check(responses[0], {
    "Controller works": (r) => r.body.includes("Hello from TestQueryConsumer"),
  });

  sleep(0.2);
}
