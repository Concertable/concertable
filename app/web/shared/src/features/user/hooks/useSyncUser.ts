import { useEffect } from "react";
import { useAuth } from "react-oidc-context";
import { useAuthStore, userManager } from "@/features/auth";
import userApi from "../api/userApi";

export function useSyncUser() {
  const { isAuthenticated, isLoading } = useAuth();
  const setUser = useAuthStore((s) => s.setUser);

  useEffect(() => {
    if (isLoading) return;

    if (!isAuthenticated) {
      setUser(null);
      userManager.signinSilent().catch(() => {});
      return;
    }

    userApi.getMe().then(setUser).catch(() => setUser(null));
  }, [isAuthenticated, isLoading, setUser]);
}
