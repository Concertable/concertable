import * as AuthSession from "expo-auth-session";
import api, { configureApi } from "@concertable/shared/lib/axiosClient";
import { useAuthStore } from "@concertable/shared/features/auth";
import { tokenStorage } from "../auth/tokenStorage";
import Config from "./config";

configureApi(`${Config.apiUrl}/api`);

api.interceptors.request.use(async (config) => {
  const token = await tokenStorage.getAccessToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (r) => r,
  async (error) => {
    const original = error.config;
    if (error.response?.status !== 401 || original._retry) {
      return Promise.reject(error);
    }

    original._retry = true;
    const refreshToken = await tokenStorage.getRefreshToken();
    if (refreshToken) {
      try {
        const discovery = await AuthSession.fetchDiscoveryAsync(Config.authAuthority);
        const tokens = await AuthSession.refreshAsync(
          { clientId: Config.authClientId, refreshToken },
          discovery,
        );
        await tokenStorage.setTokens(tokens.accessToken, tokens.refreshToken ?? refreshToken);
        original.headers.Authorization = `Bearer ${tokens.accessToken}`;
        return api(original);
      } catch {
        // Refresh failed — fall through to sign out
      }
    }

    await tokenStorage.clear();
    useAuthStore.getState().setUser(null);
    return Promise.reject(error);
  },
);

export default api;
