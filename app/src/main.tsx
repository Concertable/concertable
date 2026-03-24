import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { createRouter, RouterProvider } from "@tanstack/react-router";
import { APIProvider as MapsProvider } from "@vis.gl/react-google-maps";
import { routeTree } from "./routeTree.gen";
import { ThemeProvider } from "./providers/ThemeProvider";
import { useAuthStore } from "./store/useAuthStore";
import "./index.css";

const router = createRouter({ routeTree });

declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
  }
}

async function init() {
  const { refreshToken, refresh } = useAuthStore.getState();
  if (refreshToken) await refresh().catch(() => {});

  createRoot(document.getElementById("root")!).render(
    <StrictMode>
      <MapsProvider apiKey={import.meta.env.VITE_GOOGLE_MAPS_API_KEY} libraries={["places"]}>
        <ThemeProvider>
          <RouterProvider router={router} />
        </ThemeProvider>
      </MapsProvider>
    </StrictMode>
  );
}

init();
