import { useEffect, useState } from "react";
import { useAuthStore } from "@concertable/shared/features/auth";
import { userApi } from "@concertable/shared/features/user";
import { tokenStorage } from "./tokenStorage";
import "../lib/axios";

export function useAuthInit() {
  const setUser = useAuthStore((s) => s.setUser);
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    tokenStorage.getAccessToken().then(async (token) => {
      if (token) {
        try {
          const user = await userApi.getMe();
          setUser(user);
        } catch {
          await tokenStorage.clear();
        }
      }
      setIsReady(true);
    });
  }, []);

  return isReady;
}
