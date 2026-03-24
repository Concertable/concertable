import { useRouterState } from "@tanstack/react-router";
import type { UserRole } from "@/types/auth";

const prefixMap: [string, UserRole][] = [
  ["/venue", "VenueManager"],
  ["/artist", "ArtistManager"],
];

export function useNavSection(): UserRole {
  const pathname = useRouterState({ select: (s) => s.location.pathname });
  return prefixMap.find(([prefix]) => pathname.startsWith(prefix))?.[1] ?? "Customer";
}
