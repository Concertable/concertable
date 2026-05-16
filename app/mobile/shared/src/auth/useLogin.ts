import * as AuthSession from "expo-auth-session";
import * as WebBrowser from "expo-web-browser";
import { useEffect, useRef, useState } from "react";
import { useAuthStore } from "@concertable/shared/features/auth";
import { userApi } from "@concertable/shared/features/user";
import { tokenStorage } from "./tokenStorage";
import "../lib/axios";
import Config from "../lib/config";

WebBrowser.maybeCompleteAuthSession();

const REDIRECT_URI = AuthSession.makeRedirectUri();
console.log("[auth] redirect URI:", REDIRECT_URI);

export type SignupRole = "venue" | "artist" | "customer";

export function useLogin() {
  const setUser = useAuthStore((s) => s.setUser);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const processedCode = useRef<string | null>(null);

  const discovery = AuthSession.useAutoDiscovery(Config.authAuthority);

  const [loginRequest, loginResponse, loginPromptAsync] = AuthSession.useAuthRequest(
    {
      clientId: Config.authClientId,
      scopes: Config.authScopes,
      redirectUri: REDIRECT_URI,
      usePKCE: true,
    },
    discovery,
  );

  function handleResponse(
    response: AuthSession.AuthSessionResult | null,
    codeVerifier: string | undefined,
  ) {
    if (response?.type !== "success" || !codeVerifier || !discovery) return;

    const { code } = response.params;
    if (processedCode.current === code) return;
    processedCode.current = code;

    setLoading(true);
    setError(null);

    AuthSession.exchangeCodeAsync(
      {
        code,
        clientId: Config.authClientId,
        redirectUri: REDIRECT_URI,
        extraParams: { code_verifier: codeVerifier },
      },
      discovery,
    )
      .then(async (tokens) => {
        await tokenStorage.setTokens(
          tokens.accessToken,
          tokens.refreshToken ?? "",
          tokens.idToken ?? "",
        );
        const user = await userApi.getMe();
        setUser(user);
      })
      .catch((e: Error) => {
        console.error("[auth] error:", e.message, e);
        setError(e.message ?? "Login failed");
      })
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    handleResponse(loginResponse, loginRequest?.codeVerifier);
  }, [loginResponse]);

  const isReady = !!discovery && !!loginRequest;

  return {
    login: () => {
      setError(null);
      loginPromptAsync();
    },
    signup: (role?: SignupRole) => {
      const url = role
        ? `${Config.authAuthority}/Account/Register?roleHint=${role}`
        : `${Config.authAuthority}/Account/Register`;
      WebBrowser.openAuthSessionAsync(url, REDIRECT_URI);
    },
    loading: loading || !isReady,
    error,
  };
}
