import * as AuthSession from "expo-auth-session";
import { tokenStorage } from "./tokenStorage";
import Config from "../lib/config";

export async function getValidAccessToken(): Promise<string> {
  const token = await tokenStorage.getAccessToken();
  if (!token) return "";

  try {
    const payload = JSON.parse(atob(token.split(".")[1]!)) as { exp?: number };
    if (payload.exp && Date.now() < payload.exp * 1000 - 60_000)
      return token;
  } catch {
    return token;
  }

  const [refreshToken, idToken] = await Promise.all([
    tokenStorage.getRefreshToken(),
    tokenStorage.getIdToken(),
  ]);
  if (!refreshToken) return token;

  try {
    const timeout = new Promise<never>((_, reject) =>
      setTimeout(() => reject(new Error("timeout")), 5_000),
    );
    const discovery = await Promise.race([
      AuthSession.fetchDiscoveryAsync(Config.authAuthority),
      timeout,
    ]);
    const tokens = await AuthSession.refreshAsync(
      { clientId: Config.authClientId, refreshToken },
      discovery,
    );
    await tokenStorage.setTokens(
      tokens.accessToken,
      tokens.refreshToken ?? refreshToken,
      tokens.idToken ?? idToken ?? "",
    );
    return tokens.accessToken;
  } catch {
    return token;
  }
}
