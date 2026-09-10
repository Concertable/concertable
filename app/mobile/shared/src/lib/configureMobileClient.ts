import type { AxiosInstance } from "axios";
import { attachAuth, configureClient } from "@concertable/shared/lib/client";
import { mobileAuthSession } from "../auth/mobileAuthSession";
import { tokenStorage } from "../auth/tokenStorage";

export const configureMobileClient = (instance: AxiosInstance, baseURL: string) => {
  const client = configureClient(instance, baseURL);
  attachAuth(
    instance,
    () => tokenStorage.getAccessToken(),
    async () => {
      await tokenStorage.clear();
      mobileAuthSession.clear();
    },
  );
  return client;
};
