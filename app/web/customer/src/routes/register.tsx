import { createFileRoute } from "@tanstack/react-router";
import { useEffect } from "react";
import { useAuth } from "@/features/auth";

function RegisterRedirect() {
  const { register } = useAuth();

  useEffect(() => {
    void register();
  }, [register]);

  return null;
}

export const Route = createFileRoute("/register")({
  component: RegisterRedirect,
});
