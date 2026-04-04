import { createFileRoute, redirect } from "@tanstack/react-router";
import HomePage from "@/pages/customer/HomePage";
import { useAuthStore } from "@/store/useAuthStore";

export const Route = createFileRoute("/_customer/")({
  beforeLoad: () => {
    const baseUrl = useAuthStore.getState().user?.baseUrl;
    if (baseUrl && baseUrl !== "/") throw redirect({ to: baseUrl });
  },
  component: HomePage,
});
