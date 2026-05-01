import { redirect } from "@tanstack/react-router";
import { useAuthStore } from "./store/useAuthStore";
import type { Role } from "./types";

export function requireAuth({
  location,
}: { location?: { href: string } } = {}) {
  const user = useAuthStore.getState().user;
  if (!user)
    throw redirect({
      to: "/login",
      search: { redirect: location?.href ?? "" },
    });
  return user;
}

export function requireRole(
  role: Role,
  { location }: { location?: { href: string } } = {},
) {
  const user = requireAuth({ location });
  if (user.role !== role) throw redirect({ to: "/" });
}
