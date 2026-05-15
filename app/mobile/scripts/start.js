const { spawn } = require("child_process");
const fs = require("fs");
const path = require("path");
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

const mobileDir = path.join(__dirname, "..");
fs.writeFileSync(path.join(mobileDir, ".metro-pid"), String(process.pid));

const clearFlag = path.join(mobileDir, ".metro-clear");
const shouldClear = fs.existsSync(clearFlag);
if (shouldClear) fs.unlinkSync(clearFlag);

const expo = spawn(
  "npx",
  ["expo", "start", "--port", String(port), ...(shouldClear ? ["--clear"] : [])],
  { stdio: "inherit", shell: true, env: { ...process.env, EXPO_OFFLINE: "1" } }
);
expo.on("close", (code) => process.exit(code ?? 0));
