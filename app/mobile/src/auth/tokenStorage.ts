import * as SecureStore from "expo-secure-store";

const ACCESS_TOKEN_KEY = "access_token";
const REFRESH_TOKEN_KEY = "refresh_token";

export const tokenStorage = {
  getAccessToken: () => SecureStore.getItemAsync(ACCESS_TOKEN_KEY),
  getRefreshToken: () => SecureStore.getItemAsync(REFRESH_TOKEN_KEY),
  setTokens: (access: string, refresh: string) =>
    Promise.all([
      SecureStore.setItemAsync(ACCESS_TOKEN_KEY, access),
      SecureStore.setItemAsync(REFRESH_TOKEN_KEY, refresh),
    ]),
  clear: () =>
    Promise.all([
      SecureStore.deleteItemAsync(ACCESS_TOKEN_KEY),
      SecureStore.deleteItemAsync(REFRESH_TOKEN_KEY),
    ]),
};
