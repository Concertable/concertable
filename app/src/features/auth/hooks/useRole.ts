import { useAuthStore } from "../store/useAuthStore";
import type { Role } from "../types";

export function useRole(): Role | undefined {
  const user = useAuthStore((s) => s.user);
  return user?.role;
}
