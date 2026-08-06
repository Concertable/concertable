import { createServer, type Server } from "node:http";
import { afterAll, beforeAll, describe, expect, it, vi } from "vitest";
import { createApiClient } from "./apiClient";
import { ApiError, isApiError } from "./apiError";
import { configureClient } from "./client";

let server: Server;
let baseURL: string;

beforeAll(async () => {
  server = createServer((request, response) => {
    response.setHeader("content-type", "application/problem+json");

    if (request.url === "/missing") {
      response.statusCode = 404;
      response.end(JSON.stringify({ title: "Not found", detail: "Not ready" }));
      return;
    }

    if (request.url === "/unauthorized") {
      response.statusCode = 401;
      response.end(JSON.stringify({ title: "Unauthorized" }));
      return;
    }

    response.statusCode = 503;
    response.end(JSON.stringify({ detail: "Unavailable" }));
  });

  await new Promise<void>((resolve) => server.listen(0, "127.0.0.1", resolve));
  const address = server.address();
  if (address === null || typeof address === "string")
    throw new Error("Test server did not bind to a TCP port");
  baseURL = `http://127.0.0.1:${address.port}`;
});

afterAll(async () => {
  await new Promise<void>((resolve, reject) =>
    server.close((error) => (error ? reject(error) : resolve())),
  );
});

function createConfiguredClient(onUnauthorized = vi.fn()) {
  const client = createApiClient();
  configureClient(client, baseURL).withAuth(() => null, onUnauthorized);
  return { client, onUnauthorized };
}

describe("configureClient", () => {
  it("identifies the transport-neutral error contract", () => {
    const error = new ApiError(503, { detail: "Unavailable" }, "get", "/", null);

    expect(isApiError(error)).toBe(true);
    expect(error.message).toBe("Unavailable");
  });

  it("resolves an optional 404 as null", async () => {
    const { client } = createConfiguredClient();

    const response = await client.getOptional<{ id: number }>("/missing");

    expect(response.status).toBe(404);
    expect(response.data).toBeNull();
  });

  it("preserves unexpected errors during the additive rollout", async () => {
    const { client } = createConfiguredClient();

    await expect(client.get("/missing")).rejects.toMatchObject({
      isAxiosError: true,
      response: {
        status: 404,
        data: { title: "Not found", detail: "Not ready" },
      },
    });
  });

  it("runs unauthorized handling before preserving the error", async () => {
    const { client, onUnauthorized } = createConfiguredClient();

    await expect(client.get("/unauthorized")).rejects.toMatchObject({
      isAxiosError: true,
      response: { status: 401 },
    });
    expect(onUnauthorized).toHaveBeenCalledOnce();
  });
});
