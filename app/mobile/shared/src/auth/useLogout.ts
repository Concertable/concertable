import * as AuthSession from "expo-auth-session";
import * as WebBrowser from "expo-web-browser";
import { mobileAuthSession } from "./mobileAuthSession";
import { tokenStorage } from "./tokenStorage";
import Config from "../lib/config";

export function useLogout() {
  async function logout() {
    const [discovery, idToken] = await Promise.all([
      AuthSession.fetchDiscoveryAsync(Config.authAuthority),
      tokenStorage.getIdToken(),
    ]);

    await tokenStorage.clear();
    mobileAuthSession.clear();

    if (discovery.endSessionEndpoint && idToken) {
      const logoutUri = `${Config.urlScheme}://logout`;
      void WebBrowser.openAuthSessionAsync(
        `${discovery.endSessionEndpoint}?id_token_hint=${idToken}&post_logout_redirect_uri=${encodeURIComponent(logoutUri)}`,
        logoutUri,
      );
    }
  }

  return { logout };
}
