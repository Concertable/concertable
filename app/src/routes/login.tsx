import { createFileRoute } from "@tanstack/react-router";
import { useEffect } from "react";
import { useAuth } from "react-oidc-context";
import { z } from "zod";

function LoginRedirect() {
  const auth = useAuth();
  const { redirect } = Route.useSearch();

  useEffect(() => {
    if (auth.isLoading) return;
    if (auth.isAuthenticated) return;
    if (auth.activeNavigator) return;
    void auth.signinRedirect({ state: { redirect } });
  }, [auth.isLoading, auth.isAuthenticated, auth.activeNavigator, auth, redirect]);

  return null;
}

export const Route = createFileRoute("/login")({
  validateSearch: z.object({ redirect: z.string().optional() }),
  component: LoginRedirect,
});
