import { useEffect } from "react";
import { useAuth } from "react-oidc-context";
import { useQuery } from "@tanstack/react-query";
import { useAuthStore } from "@/features/auth";
import userApi from "../api/userApi";
import type { User } from "@/features/auth/types";

export const meQueryKey = ["auth", "me"] as const;

export function useSyncUser(getMe: () => Promise<User> = userApi.getMe) {
  const { isAuthenticated, isLoading } = useAuth();
  const setUser = useAuthStore((s) => s.setUser);

  const { data, isError } = useQuery({
    queryKey: meQueryKey,
    queryFn: getMe,
    enabled: !isLoading && isAuthenticated,
    meta: { expectedErrors: [404] },
  });

  useEffect(() => {
    if (isLoading) return;
    if (!isAuthenticated) {
      setUser(null);
      return;
    }
    if (isError) setUser(null);
    else if (data) setUser(data);
  }, [isAuthenticated, isLoading, data, isError, setUser]);
}
