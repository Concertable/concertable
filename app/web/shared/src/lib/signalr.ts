import { createNotificationConnection, HubConnectionState } from "@concertable/shared/lib";
import { userManager } from "@/features/auth";

export const notificationConnection = createNotificationConnection({
  baseUrl: import.meta.env.VITE_BASE_URL,
  accessTokenFactory: async () => {
    const user = await userManager.getUser();
    return user?.access_token ?? "";
  },
  dev: import.meta.env.DEV,
});

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
