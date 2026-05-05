import { redirect } from "@tanstack/react-router";
import userApi from "@/features/user/api/userApi";
import { userManager } from "./config/oidcConfig";
import { useAuthStore } from "./store/useAuthStore";
import type { Role, User } from "./types";

async function hasValidSession() {
  const oidcUser = await userManager.getUser();
  return !!oidcUser && !oidcUser.expired;
}

async function ensureUser(): Promise<User | null> {
  const cached = useAuthStore.getState().user;
  if (cached) return cached;
  try {
    const user = await userApi.getMe();
    useAuthStore.getState().setUser(user);
    return user;
  } catch {
    return null;
  }
}

export async function requireAuth({
  location,
}: { location?: { pathname: string } } = {}) {
  if (!(await hasValidSession()))
    throw redirect({
      to: "/login",
      search: { redirect: location?.pathname ?? "" },
    });
  const user = await ensureUser();
  if (!user)
    throw redirect({
      to: "/login",
      search: { redirect: location?.pathname ?? "" },
    });
  return user;
}

export async function requireRole(
  role: Role,
  { location }: { location?: { pathname: string } } = {},
) {
  const user = await requireAuth({ location });
  if (user.role !== role) throw redirect({ to: "/" });
}
