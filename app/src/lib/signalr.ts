import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { userManager } from "@/lib/oidcConfig";

export const notificationConnection = new HubConnectionBuilder()
  .withUrl(`${import.meta.env.VITE_BASE_URL}/hub/notifications`, {
    accessTokenFactory: async () => {
      const user = await userManager.getUser();
      return user?.access_token ?? "";
    },
  })
  .withAutomaticReconnect()
  .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.None)
  .build();

userManager.events.addUserLoaded(() => {
  if (notificationConnection.state === HubConnectionState.Disconnected)
    notificationConnection.start().catch(console.error);
});

userManager.events.addUserUnloaded(() => {
  if (notificationConnection.state === HubConnectionState.Connected)
    notificationConnection.stop().catch(console.error);
});
