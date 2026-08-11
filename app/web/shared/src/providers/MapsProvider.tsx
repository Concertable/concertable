import { APIProvider } from "@vis.gl/react-google-maps";
import type { ReactNode } from "react";

// Mount around map/search components only — never at app boot; booting Maps globally
// contacts Google before the user invokes any map feature (load-on-use, PECR).
export function MapsProvider({ children }: { children: ReactNode }) {
  return (
    <APIProvider
      apiKey={import.meta.env.VITE_GOOGLE_MAPS_API_KEY}
      libraries={["places"]}
    >
      {children}
    </APIProvider>
  );
}
