import { useCallback } from "react";
import { useAuth as useOidcAuth } from "react-oidc-context";
import { userManager } from "../config/oidcConfig";

export function useAuth() {
  const auth = useOidcAuth();

  const register = useCallback(async () => {
    const req = await userManager.createSigninRequest({});
    const authUrl = new URL(req.url);
    const returnUrl = authUrl.pathname + authUrl.search;
    window.location.href = `${import.meta.env.VITE_AUTH_AUTHORITY}/Account/Register?returnUrl=${encodeURIComponent(returnUrl)}`;
  }, []);

  return { ...auth, register };
}
