import * as AuthSession from "expo-auth-session";
import * as WebBrowser from "expo-web-browser";
import { useEffect, useRef, useState } from "react";
import { userApi } from "@concertable/shared/features/user";
import { mobileAuthSession } from "./mobileAuthSession";
import { tokenStorage } from "./tokenStorage";
import "../lib/apiClient";
import "../lib/searchClient";
import "../lib/paymentClient";
import Config from "../lib/config";

WebBrowser.maybeCompleteAuthSession();

const REDIRECT_URI = AuthSession.makeRedirectUri();
console.log("[auth] redirect URI:", REDIRECT_URI);

export function useLogin() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>();
  const processedCode = useRef<string | undefined>(undefined);

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
    setError(undefined);

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
        mobileAuthSession.set(user);
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
      setError(undefined);
      loginPromptAsync();
    },
    signup: async (clientId: string = Config.authClientId) => {
      if (!discovery) return;
      const request = new AuthSession.AuthRequest({
        clientId,
        scopes: Config.authScopes,
        redirectUri: REDIRECT_URI,
        usePKCE: true,
      });
      const authUrl = await request.makeAuthUrlAsync(discovery);
      const returnUrl = `/connect/authorize/callback${authUrl.substring(authUrl.indexOf("?"))}`;
      const registerUrl = `${Config.authAuthority}/Account/Register?ReturnUrl=${encodeURIComponent(returnUrl)}`;
      await WebBrowser.openAuthSessionAsync(registerUrl, REDIRECT_URI);
    },
    loading: loading || !isReady,
    error,
  };
}
