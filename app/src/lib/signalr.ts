import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { useAuthStore } from "@/store/useAuthStore";

export const notificationConnection = new HubConnectionBuilder()
  .withUrl(`${import.meta.env.VITE_BASE_URL}/hub/notifications`, {
    accessTokenFactory: () => useAuthStore.getState().accessToken ?? "",
  })
  .withAutomaticReconnect()
  .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.None)
  .build();

useAuthStore.subscribe((state) => {
  if (
    state.accessToken &&
    notificationConnection.state === HubConnectionState.Disconnected
  ) {
    notificationConnection.start().catch(console.error);
  } else if (
    !state.accessToken &&
    notificationConnection.state === HubConnectionState.Connected
  ) {
    notificationConnection.stop().catch(console.error);
  }
});
