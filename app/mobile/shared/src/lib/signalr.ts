import { HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import { mobileAuthSession } from "../auth/mobileAuthSession";
import { getValidAccessToken } from "../auth/getValidAccessToken";
import { logger } from "./logger";
import Config from "./config";

export const notificationConnection = new HubConnectionBuilder()
  .withUrl(`${Config.apiUrl}/hub/notifications`, { accessTokenFactory: getValidAccessToken })
  .withAutomaticReconnect()
  .configureLogging(LogLevel.Trace)
  .build();

notificationConnection.onreconnecting((err) => {
  logger.warn("[signalr] onreconnecting", { state: notificationConnection.state, err });
});

notificationConnection.onreconnected((id) => {
  logger.log("[signalr] onreconnected", { connectionId: id, state: notificationConnection.state });
});

function startIfAuthenticated() {
  const hasUser = mobileAuthSession.current() !== undefined;
  logger.log("[signalr] startIfAuthenticated entry", {
    user: hasUser,
    state: notificationConnection.state,
  });
  if (hasUser && notificationConnection.state === HubConnectionState.Disconnected) {
    logger.log("[signalr] starting");
    notificationConnection.start()
      .then(() => logger.log("[signalr] started", { state: notificationConnection.state }))
      .catch((err) => logger.error("[signalr] start failed", err));
  }
}

notificationConnection.onclose((err) => {
  logger.warn("[signalr] onclose", { state: notificationConnection.state, err });
  setTimeout(startIfAuthenticated, 5_000);
});

mobileAuthSession.subscribe((user, previousUser) => {
  const wasAuthenticated = previousUser !== undefined;
  const isAuthenticated = user !== undefined;
  logger.log("[signalr] auth transition", { wasAuthenticated, isAuthenticated });

  if (!wasAuthenticated && isAuthenticated)
    startIfAuthenticated();

  if (wasAuthenticated && !isAuthenticated)
    notificationConnection.stop().catch((err) => logger.error("[signalr] stop failed", err));
});
