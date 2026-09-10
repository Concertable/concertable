# Concertable Architecture

Concertable is a monorepo split into a backend and frontend surfaces. The monorepo is a
convenience only — the backend services are independently-owned microservices and are designed to
split into separate repos. That premise and the rules it imposes live with the code it governs:

- **Backend (`api/`)** — five independent .NET microservices (`Auth`, `B2B`, `Customer`, `Search`,
  `Payment`) plus shared infra. Service boundaries and the microservice premise: the
  `microservice-boundaries` skill; the design rationale and decision history: the
  `microservices-architecture` skill.
- **Web (`app/web/`)** — per-surface SPAs (customer, venue, artist, business): [`app/web/AGENTS.md`](./app/web/AGENTS.md).
- **Mobile (`app/mobile/`)** — React Native (Expo) apps, b2b + customer: [`app/mobile/AGENTS.md`](./app/mobile/AGENTS.md).

See root [`AGENTS.md`](./AGENTS.md) for top-of-context rules and pointers.
