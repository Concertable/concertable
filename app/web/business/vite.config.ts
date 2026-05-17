import fs from 'fs'
import path from 'path'
import child_process from 'child_process'
import { defineConfig } from 'vite'

const baseFolder = process.env.APPDATA
  ? `${process.env.APPDATA}/ASP.NET/https`
  : `${process.env.HOME}/.aspnet/https`

const certName = 'concertable-business'
const certFile = path.join(baseFolder, `${certName}.pem`)
const keyFile = path.join(baseFolder, `${certName}.key`)

if (!fs.existsSync(certFile) || !fs.existsSync(keyFile)) {
  fs.mkdirSync(baseFolder, { recursive: true })
  if (child_process.spawnSync('dotnet', ['dev-certs', 'https', '--export-path', certFile, '--format', 'Pem', '--no-password'], { stdio: 'inherit' }).status !== 0)
    throw new Error('Could not create dev certificate.')
}

export default defineConfig({
  plugins: [],
  envDir: path.resolve(__dirname, '..'),
  server: {
    port: 5177,
    https: { key: fs.readFileSync(keyFile), cert: fs.readFileSync(certFile) },
  },
})
