import type { AxiosInstance } from "axios";
import { userManager } from "@/features/auth";
import { configureClient } from "@concertable/shared/lib/client";

export const configureWebClient = (instance: AxiosInstance, baseURL: string) =>
  configureClient(instance, baseURL).withAuth(
    async () => (await userManager.getUser())?.access_token ?? null,
    () => userManager.removeUser(),
  );
