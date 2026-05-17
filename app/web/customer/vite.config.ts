import fs from 'fs'
import path from "path"
import child_process from 'child_process'
import tailwindcss from "@tailwindcss/vite"
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'
import { tanstackRouter } from '@tanstack/router-vite-plugin'

const baseFolder = process.env.APPDATA
  ? `${process.env.APPDATA}/ASP.NET/https`
  : `${process.env.HOME}/.aspnet/https`

const certName = 'concertable-customer'
const certFile = path.join(baseFolder, `${certName}.pem`)
const keyFile = path.join(baseFolder, `${certName}.key`)

if (!fs.existsSync(certFile) || !fs.existsSync(keyFile)) {
  fs.mkdirSync(baseFolder, { recursive: true })
  if (child_process.spawnSync('dotnet', ['dev-certs', 'https', '--export-path', certFile, '--format', 'Pem', '--no-password'], { stdio: 'inherit' }).status !== 0)
    throw new Error('Could not create dev certificate.')
}

export default defineConfig({
  plugins: [tanstackRouter(), react(), tailwindcss()],
  envDir: path.resolve(__dirname, '..'),
  server: {
    port: 5174,
    https: { key: fs.readFileSync(keyFile), cert: fs.readFileSync(certFile) },
  },
  define: {
    'import.meta.env.VITE_OIDC_CLIENT_ID': JSON.stringify('customer-web'),
  },
  resolve: {
    alias: [
      { find: /^@\/(components|features|hooks|lib|providers|context|types|assets)(\/.*)?$/, replacement: path.resolve(__dirname, "../shared/src/$1$2") },
      { find: /^shared\/(.*)$/, replacement: path.resolve(__dirname, "../shared/src/$1") },
      { find: "@", replacement: path.resolve(__dirname, "./src") },
    ],
  },
})
