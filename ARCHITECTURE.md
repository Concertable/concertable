# Concertable Architecture

Concertable is a monorepo split into a backend and frontend surfaces. The monorepo is a
convenience only — the backend services are independently-owned microservices and are designed to
split into separate repos. That premise and the rules it imposes live with the code it governs:

- **Backend (`api/`)** — five independent .NET microservices (`Auth`, `B2B`, `Customer`, `Search`,
  `Payment`) plus shared infra. Architecture, service boundaries, and the microservice premise:
  [`api/ARCHITECTURE.md`](./api/ARCHITECTURE.md).
- **Web (`app/web/`)** — per-surface SPAs (customer, venue, artist, business): [`app/web/AGENTS.md`](./app/web/AGENTS.md).
- **Mobile (`app/mobile/`)** — React Native (Expo) apps, b2b + customer: [`app/mobile/AGENTS.md`](./app/mobile/AGENTS.md).

See root `CLAUDE.md` for top-of-context rules and pointers.
