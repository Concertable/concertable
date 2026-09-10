import { useEffect, useState } from "react";
import { userApi } from "@concertable/shared/features/user";
import { mobileAuthSession } from "./mobileAuthSession";
import { tokenStorage } from "./tokenStorage";
import "../lib/apiClient";
import "../lib/searchClient";
import "../lib/paymentClient";

export function useAuthInit() {
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    tokenStorage.getAccessToken().then(async (token) => {
      if (token) {
        try {
          const user = await userApi.getMe();
          mobileAuthSession.set(user);
        } catch {
          await tokenStorage.clear();
          mobileAuthSession.clear();
        }
      }
      setIsReady(true);
    });
  }, []);

  return isReady;
}
