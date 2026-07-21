import type { AxiosInstance } from "axios";
import { configureClient } from "@concertable/shared/lib/client";
import { useAuthStore } from "@concertable/shared/features/auth";
import { tokenStorage } from "../auth/tokenStorage";

export const configureMobileClient = (instance: AxiosInstance, baseURL: string) =>
  configureClient(instance, baseURL).withAuth(
    () => tokenStorage.getAccessToken(),
    async () => {
      await tokenStorage.clear();
      useAuthStore.getState().setUser(null);
    },
  );
