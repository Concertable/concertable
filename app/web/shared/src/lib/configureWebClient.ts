import type { AxiosInstance } from "axios";
import { userManager } from "@/features/auth";
import { attachAuth, configureClient } from "@concertable/shared/lib/client";

export const configureWebClient = (instance: AxiosInstance, baseURL: string) => {
  const client = configureClient(instance, baseURL);
  attachAuth(
    instance,
    async () => (await userManager.getUser())?.access_token ?? null,
    () => userManager.removeUser(),
  );
  return client;
};
