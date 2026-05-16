import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";

export { HubConnectionState, LogLevel };

interface NotificationConnectionOptions {
  baseUrl: string;
  accessTokenFactory: () => Promise<string>;
  dev?: boolean;
}

export function createNotificationConnection({
  baseUrl,
  accessTokenFactory,
  dev = false,
}: NotificationConnectionOptions) {
  return new HubConnectionBuilder()
    .withUrl(`${baseUrl}/hub/notifications`, { accessTokenFactory })
    .withAutomaticReconnect()
    .configureLogging(dev ? LogLevel.Information : LogLevel.None)
    .build();
}
