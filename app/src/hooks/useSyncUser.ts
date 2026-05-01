import { useEffect } from "react";
import { useAuth } from "react-oidc-context";
import userApi from "@/api/userApi";
import { useAuthStore } from "@/store/useAuthStore";

export function useSyncUser() {
  const { isAuthenticated } = useAuth();
  const setUser = useAuthStore((s) => s.setUser);

  useEffect(() => {
    if (!isAuthenticated) {
      setUser(null);
      return;
    }
    userApi.getMe().then(setUser).catch(() => setUser(null));
  }, [isAuthenticated, setUser]);
}
