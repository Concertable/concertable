import * as SecureStore from "expo-secure-store";

const ACCESS_TOKEN_KEY = "access_token";
const REFRESH_TOKEN_KEY = "refresh_token";
const ID_TOKEN_KEY = "id_token";

export const tokenStorage = {
  getAccessToken: () => SecureStore.getItemAsync(ACCESS_TOKEN_KEY),
  getRefreshToken: () => SecureStore.getItemAsync(REFRESH_TOKEN_KEY),
  getIdToken: () => SecureStore.getItemAsync(ID_TOKEN_KEY),
  setTokens: (access: string, refresh: string, id: string) =>
    Promise.all([
      SecureStore.setItemAsync(ACCESS_TOKEN_KEY, access),
      SecureStore.setItemAsync(REFRESH_TOKEN_KEY, refresh),
      SecureStore.setItemAsync(ID_TOKEN_KEY, id),
    ]),
  clear: () =>
    Promise.all([
      SecureStore.deleteItemAsync(ACCESS_TOKEN_KEY),
      SecureStore.deleteItemAsync(REFRESH_TOKEN_KEY),
      SecureStore.deleteItemAsync(ID_TOKEN_KEY),
    ]),
};
