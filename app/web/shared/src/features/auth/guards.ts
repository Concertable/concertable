import { redirect } from "@tanstack/react-router";
import userApi from "@/features/user/api/userApi";
import { meQueryOptions } from "@/features/user/hooks/useMeQuery";
import { queryClient } from "@/lib/queryClient";
import { userManager } from "./config/oidcConfig";
import type { User } from "./types";

async function hasValidSession() {
  const oidcUser = await userManager.getUser();
  return !!oidcUser && !oidcUser.expired;
}

async function ensureUser<TUser extends User>(
  getMe: () => Promise<TUser>,
): Promise<TUser | undefined> {
  try {
    return await queryClient.ensureQueryData(meQueryOptions(getMe));
  } catch {
    return undefined;
  }
}

export function redirectToBusiness(): Promise<never> {
  window.location.href = import.meta.env.VITE_BUSINESS_URL;
  return new Promise<never>(() => {});
}

export async function requireAuth({
  location,
  getMe = userApi.getMe,
}: {
  location?: { pathname: string };
  getMe?: () => Promise<User>;
} = {}) {
  if (!(await hasValidSession()))
    throw redirect({
      to: "/login",
      search: { redirect: location?.pathname ?? "" },
    });
  const user = await ensureUser(getMe);
  if (!user)
    throw redirect({
      to: "/login",
      search: { redirect: location?.pathname ?? "" },
    });
  return user;
}

export async function requireBusinessAuth(
  getMe: () => Promise<User>,
): Promise<void> {
  if (!(await hasValidSession())) return redirectToBusiness();
  const user = await ensureUser(getMe);
  if (!user) return redirectToBusiness();
}
