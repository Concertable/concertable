# Testing

This standard defines how an adopted frontend test suite is shaped. It does not authorize creating tests.

## Adoption is explicit

Do not add a test, install a test dependency, create a test setup, widen CI, or introduce a new test tier
merely because production code changed or an untested unit exists. Tests may be authored only when the user
explicitly requests them for the current work, or repo-owned guidance or a plan says that the relevant tier
is adopted and requires them. A `test` script, test dependency, or a few existing files is not by itself an
adoption decision.

When tests are not authorized, run the relevant existing suite if it is part of verification, and fix an
existing test only when the production change legitimately invalidates it. Do not expand coverage. A test
standard is never implicit permission to turn a production refactor into a test-infrastructure project.

## Choose the narrowest honest tier

- **Vitest in Node** owns pure logic, schemas, request shaping, API modules, store transitions, storage
  adapters, and orchestration that does not require a browser.
- **A real-browser component project** owns isolated component contracts whose DOM behavior matters:
  user interaction, focus, accessibility, browser APIs, CSS/layout-dependent behavior, and hard-to-reach
  loading, empty, or error states.
- **Browser end-to-end** owns routed workflows and collaboration among pages, browser storage, the real
  backend, and deployment composition.

Do not re-prove one behavior in every tier. Do not test a library's contract: React Query caching, router
navigation, or a primitive rendering its children belongs to that library unless application-owned behavior
changes the outcome.

## Runner and environment

Vitest is the default unit runner. Keep Node as the default environment; a whole-suite `jsdom` setting makes
pure tests pay for a simulated browser and can hide browser-only assumptions. Use named
[Vitest projects](https://vitest.dev/guide/projects) when a repository has more than one environment.

For a newly adopted web component tier, prefer
[Vitest Browser Mode](https://vitest.dev/guide/browser/component-testing) with its Playwright provider in a
Vite application. If the repository already standardizes on Playwright Test, use its stable
[`@playwright/test` component mount](https://playwright.dev/docs/test-components) and story-gallery model.
Do not introduce `@playwright/experimental-ct-*`; the stable Playwright component model supersedes it. Do
not add a second component harness to a repository that already has a supported one.

`jsdom` is acceptable for an established Testing Library suite or a small DOM-only compatibility case. It
is not the default for new component-test architecture where a real browser is available.

## Files and names

Colocate a test with its subject as `thing.test.ts` or `thing.test.tsx`. Use `.spec` instead only when that is
the repository's established spelling; Vitest recognizes both and neither is more modern. Choose one
spelling per repository. Do not create a parallel `__tests__` tree for new code.

Test names state observable behavior, not the method under test and not a generic "should work". A name must
still explain the regression after the implementation is renamed.

## Pure logic, API modules, and stores

Pure functions and schema boundaries are the highest-value Node tests because they are deterministic and
need no renderer.

An API-module unit test may mock the one HTTP client boundary the module owns to pin method, URL, and body
shaping. A hook or component test should normally exercise the request through MSW and assert the resulting
application behavior instead of mocking the API module and every collaborator below it.

Reset a store to a known state for every test. Assert application-owned transitions through its public
actions; do not render a component merely to reach store logic that can be exercised directly.

For React Query tests, create a fresh `QueryClient` per test or wrapper, disable retries unless retry behavior
is the subject, and clear or dispose it after use. Shared caches make tests order-dependent.

## Component behavior

Follow [Testing Library's guiding principle](https://testing-library.com/docs/guiding-principles): interact
with the component as a user does and assert observable output, not component instances, private state,
implementation classes, or hook call counts.

Prefer accessible queries in this order: role plus accessible name, label, visible text, then test ID only
when the UI has no semantic selector. Use the browser runner's real interactions in Browser Mode. In a
Testing Library DOM suite, create `const user = userEvent.setup()` inside the test before rendering and
await every interaction. Use `fireEvent` only for an interaction `user-event` cannot express.

Render the smallest meaningful component boundary with the real child components it owns. If one assertion
requires mocks for half the feature, move the test down to the logic boundary or up to the browser suite.

## Network behavior with MSW

Use MSW when the unit crosses the network boundary. Keep the infrastructure explicit:

- `mocks/handlers.ts` composes successful baseline handlers, split by feature/domain once the list grows;
- `mocks/server.ts` contains only `setupServer(...handlers)` for Node-run tests;
- the test setup calls `server.listen({ onUnhandledRequest: "error" })`, resets handlers after every test,
  and closes the server after the suite;
- individual tests use `server.use(...)` for errors and other scenario overrides.

`mocks/handlers.ts`

```ts
import { http, HttpResponse } from "msw";

export const handlers = [
  http.get("https://api.example.test/projects", () =>
    HttpResponse.json([{ id: "project-1", name: "Apollo" }]),
  ),
];
```

`mocks/server.ts`

```ts
import { setupServer } from "msw/node";
import { handlers } from "./handlers";

export const server = setupServer(...handlers);
```

`setupTests.ts`

```ts
import { afterAll, afterEach, beforeAll } from "vitest";
import { server } from "./mocks/server";

beforeAll(() => server.listen({ onUnhandledRequest: "error" }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

Override only the scenario in the test:

```ts
import { http, HttpResponse } from "msw";
import { server } from "./mocks/server";

server.use(
  http.get("https://api.example.test/projects", () => new HttpResponse(null, { status: 500 })),
);
```

This follows MSW's official [Node lifecycle](https://mswjs.io/docs/integrations/node) and
[handler structure](https://mswjs.io/docs/best-practices/structuring-handlers). Do not turn one `server.ts`
into a product-wide fixture database.

Do not spy on handlers to prove that a request happened. Describe request validity in the handler and assert
how the application responds, as MSW's
[request-assertion guidance](https://mswjs.io/docs/best-practices/avoid-request-assertions) recommends. Direct
request assertions are reserved for one-way effects such as analytics where no application result exists.

## Mocking and isolation

Mock one owned boundary out, not every module below the subject. Prefer dependency seams and network
behavior over mocking React, the router, or query-library internals.

Anything a `vi.mock` factory closes over is created with `vi.hoisted`. Clear mocks between tests; restore
spies and global stubs after each test. Keep deterministic time behind fake timers or an injected clock, and
restore real timers before the test ends.

## Coverage

Coverage locates unexamined branches; it is not a target. Percentage quotas reward assertions that raise a
number rather than protect behavior. A repository may enforce a non-regression gate, but the standard never
uses coverage to decide what deserves a test.
