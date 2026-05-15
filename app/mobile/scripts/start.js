const { spawn } = require("child_process");
const qrcode = require("qrcode-terminal");

// Map Aspire service-discovery vars injected by dev tunnel to Expo public vars
const authUrl = process.env["services__auth__https__0"];
const apiUrl = process.env["services__api__https__0"];
if (authUrl) process.env["EXPO_PUBLIC_AUTH_AUTHORITY"] = authUrl;
if (apiUrl) process.env["EXPO_PUBLIC_API_URL"] = apiUrl;

const host = process.env.REACT_NATIVE_PACKAGER_HOSTNAME || "localhost";
const port = 8082;
const url = `exp://${host}:${port}`;

console.log("\n=== Scan with Expo Go ===");
qrcode.generate(url, { small: true });
console.log(`URL: ${url}\n`);

const expo = spawn(
  "npx",
  ["expo", "start", "--port", String(port)],
  { stdio: "inherit", shell: true, env: { ...process.env, EXPO_OFFLINE: "1" } }
);
expo.on("close", (code) => process.exit(code ?? 0));
