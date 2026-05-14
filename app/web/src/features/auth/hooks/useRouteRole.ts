import { useRouterState } from "@tanstack/react-router";

export function useRouteRole() {
  return useRouterState({
    select: (s) => {
      const ids = s.matches.map((m) => m.routeId);
      if (ids.some((id) => id.includes("_customer"))) return "customer";
      if (ids.some((id) => id.includes("/artist"))) return "artist";
      if (ids.some((id) => id.includes("/venue"))) return "venue";
      return undefined;
    },
  });
}
