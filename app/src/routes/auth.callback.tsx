import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect } from "react";
import { useAuth } from "react-oidc-context";

function AuthCallback() {
  const auth = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!auth.isAuthenticated) return;
    const redirect = (auth.user?.state as { redirect?: string } | undefined)?.redirect;
    void navigate({ to: redirect ?? "/" });
  }, [auth.isAuthenticated, auth.user, navigate]);

  if (auth.error) return <div className="p-8">Sign-in failed: {auth.error.message}</div>;
  return <div className="p-8">Signing you in…</div>;
}

export const Route = createFileRoute("/auth/callback")({
  component: AuthCallback,
});
