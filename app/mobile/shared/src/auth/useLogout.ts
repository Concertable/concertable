import * as AuthSession from "expo-auth-session";
import * as WebBrowser from "expo-web-browser";
import { useAuthStore } from "@concertable/shared/features/auth";
import { tokenStorage } from "./tokenStorage";
import Config from "../lib/config";

export function useLogout() {
  const setUser = useAuthStore((s) => s.setUser);

  async function logout() {
    const [discovery, idToken] = await Promise.all([
      AuthSession.fetchDiscoveryAsync(Config.authAuthority),
      tokenStorage.getIdToken(),
    ]);

    await tokenStorage.clear();
    setUser(null);

    if (discovery.endSessionEndpoint && idToken) {
      void WebBrowser.openAuthSessionAsync(
        `${discovery.endSessionEndpoint}?id_token_hint=${idToken}&post_logout_redirect_uri=${encodeURIComponent("concertable://logout")}`,
        "concertable://logout",
      );
    }
  }

  return { logout };
}
