import { redirect } from "@tanstack/react-router";
import { useAuthStore } from "@/store/useAuthStore";
import type { Role } from "@/types/auth";

export function requireAuth() {
  const user = useAuthStore.getState().user;
  if (!user) throw redirect({ to: "/login" });
  return user;
}

export function requireRole(role: Role) {
  const user = requireAuth();
  if (user.role !== role) throw redirect({ to: "/" });
}
