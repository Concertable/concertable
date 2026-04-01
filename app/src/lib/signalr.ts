import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useAuthStore } from "@/store/useAuthStore";

export const notificationConnection = new HubConnectionBuilder()
  .withUrl(`${import.meta.env.VITE_API_URL}/hubs/notifications`, {
    accessTokenFactory: () => useAuthStore.getState().accessToken ?? "",
  })
  .withAutomaticReconnect()
  .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.None)
  .build();
