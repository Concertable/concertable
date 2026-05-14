import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { userManager } from "@/features/auth";

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

function startIfReady() {
  if (notificationConnection.state === HubConnectionState.Disconnected)
    notificationConnection.start().catch(console.error);
}

userManager.getUser().then((user) => {
  if (user && !user.expired) startIfReady();
});

userManager.events.addUserLoaded(startIfReady);

userManager.events.addUserUnloaded(() => {
  if (notificationConnection.state === HubConnectionState.Connected)
    notificationConnection.stop().catch(console.error);
});
